using System;
using RPCExp.Modbus;
using System.Threading;
using RPCExp.Modbus.Factory;
using RPCExp.Common;
using System.Collections.Generic;

namespace RPCExp
{

    class Program
    {
        static CancellationTokenSource Cts = new CancellationTokenSource();

        public interface ITest
        {

        }

        static void Main(string[] args)
        {


            //var st = new Ticker();
            //string TickToSecStr(long ticks) => (ticks / 10_000_000.0).ToString("#.###");
            //while (Console.ReadKey().Key != ConsoleKey.Escape)
            //{
            //    Console.Write($"was active: {st.IsActive},\tperiod: {TickToSecStr(st.Period)}");
            //    st.Tick();
            //    Console.WriteLine($"\tnow active: {st.IsActive},\tperiod: {TickToSecStr(st.Period)}");
            //}
            //return;

            Console.WriteLine("Starting");

            // var facility = Factory.LoadFacility("cfg.json");
            var conn = new ConnectionSource
            {
                ConnectionCfg = "127.0.0.1:11502",
                Physic = Physic.Tcp,
                Name = "localhost"
            };

            var facility = new Facility
            {
                ConnectionsSource = new Dictionary<string, ConnectionSource>
                {
                   [ conn.Name ] = conn,
                },

                DevicesSource = new Dictionary<string, DeviceAbstract>
                {
                    ["Plc1"] = new Device
                    {
                        Name = "Plc1",
                        Connection = conn,
                        MasterSource = new MasterSource{
                            frameType = FrameType.Ip,
                        },
                        SlaveId = 1,
                    }
                }
            };

            Console.WriteLine("conf - ok");

            var dev = facility.DevicesSource["Plc1"];

            dev.Tags.Add("Tag1", new MTag
            {
                Name = "Tag1",
                Region = ModbusRegion.HoldingRegisters,
                Begin = 0,
                ValueType = Modbus.TypeConverters.ModbusValueType.Int16,
            });

            dev.Tags.Add("Tag2", new MTag
            {
                Name = "Tag2",
                Region = ModbusRegion.HoldingRegisters,
                Begin = 1,
                ValueType = Modbus.TypeConverters.ModbusValueType.Int16,
            });

            dev.Tags.Add("Tag3", new MTag
            {
                Name = "Tag3",
                Region = ModbusRegion.HoldingRegisters,
                Begin = 2,
                ValueType = Modbus.TypeConverters.ModbusValueType.Int16,
            });

            dev.Tags.Add("Tag4", new MTag
            {
                Name = "Tag4",
                Region = ModbusRegion.HoldingRegisters,
                Begin = 3,
                ValueType = Modbus.TypeConverters.ModbusValueType.Float,
            });

            dev.Tags.Add("boolTag5", new MTag
            {
                Name = "boolTag5",
                Region = ModbusRegion.Coils,
                Begin = 3,
                ValueType = Modbus.TypeConverters.ModbusValueType.Bool,
            });


            Factory.SaveFacility(facility);
            return;
            //dev.Start();

            Console.WriteLine("Start  tasks");
            Console.CancelKeyPress += Console_CancelKeyPress;

            //Timer t1 = new Timer((x)=>dev.Tags["Tag1"].GetValue(), new AutoResetEvent(false), 0, 2000 );
            //Timer t2 = new Timer((x) => dev.Tags["Tag2"].GetValue(), new AutoResetEvent(false), 0, 2100);

            //Task.Run(() =>
            //{
            //    Task.Delay(10_000).Wait();
            //    dev.Write(new Dictionary<string, object> { ["Tag3"] = 33.21 });
            //});

            // ==== WebSocket ====
            Console.WriteLine("Start Socket");
            Router router = new Router();
            router.RegisterMethods(dev, nameof(dev));

            WebSocketServer wss = new WebSocketServer(router, new string[] { "http://localhost:8888/"/*, "http://*:8888/"*/});
            
            wss.Start();
            
            Console.Clear();
            while (!Cts.Token.IsCancellationRequested)
            {
                try
                {
                    Terminal.TermForms.DisplayModbusDevice((Device)dev);
                    //Console.WriteLine("Tag1: " + dev.Tags["Tag1"].GetInternalValue());
                    //Console.WriteLine("Tag2: " + dev.Tags["Tag2"].GetInternalValue());
                }
                catch (Exception ex)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.ToString());
                    Console.ResetColor();
                    break;
                }
                
                Thread.Sleep(1000);
            }

            wss.Stop();

            Console.ReadKey();
            Console.Clear();
            return;

            //Console.WriteLine("Started");            
            //wss.Stop();
            //client.Dispose();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Cts.Cancel();
        }
    }
}
