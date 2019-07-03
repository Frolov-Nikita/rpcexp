using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NModbus;
using RPCExp.Modbus;
using RPCExp.Common;
using System.Threading;
using System.Threading.Tasks;


using Newtonsoft.Json;
using RPCExp.Modbus.TypeConverters;
using RPCExp.Modbus.Factory;

namespace RPCExp
{

    class Program
    {
        static CancellationTokenSource Cts = new CancellationTokenSource();

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

            var DefaultByteOrder = new byte[] { 2, 3, 0, 1 };

            var store = new Common.Store
            {
                Facilities = new List<Facility>
                {
                    new Facility
                    {
                        Devices = new List<DeviceAbstract>
                        {
                            //new Device
                            //{
                            //    SlaveId = 1,
                            //}
                        }
                    }
                }
            };

            var dev = Factory.LoadDevice("cfg.json");
            store.Facilities[0].Devices.Add(dev);
            //var dev = (Device)store.Facilities[0].Devices[0];
            
            //dev.Tags.Add("tag1", new MTag {
            //    Region = ModbusRegion.HoldingRegisters,
            //    Name = "Tag1",
            //    Begin = 2,
            //    ValueType = ModbusValueType.Int16,
            //});

            //dev.Tags.Add("tag2", new MTag
            //{
            //    Region = ModbusRegion.HoldingRegisters,
            //    Name = "Tag2",
            //    Begin = 3,
            //    ValueType = ModbusValueType.Float,
            //}); ;
            
            //dev.Tags.Add("BoolTag1", new MTag
            //{
            //    Region = ModbusRegion.Coils,
            //    Name = "BoolTag1",
            //    Begin = 500,
            //    ValueType = ModbusValueType.Bool,
            //});

            //Factory.SaveDevice(dev, "cfg2.json");
            //return;

            dev.Start();
            
            Console.CancelKeyPress += Console_CancelKeyPress;

            Timer t1 = new Timer((x)=>dev.Tags["Tag1"].GetValue(), new AutoResetEvent(false), 0, 2000 );

            Timer t2 = new Timer((x) => dev.Tags["Tag2"].GetValue(), new AutoResetEvent(false), 0, 2100);

            //Task.Run(() =>
            //{
            //    Task.Delay(10_000).Wait();
            //    dev.Tags["Tag2"].GetValue();
            //});

            while (!Cts.Token.IsCancellationRequested)
            {
                Terminal.TermForms.DisplayModbusDevice(dev);
                
                Thread.Sleep(500);
            }

            return;


            //var factory = new ModbusFactory();
            //var client = new System.Net.Sockets.TcpClient("127.0.0.1", 11502);
            //IModbusMaster dev1 = factory.CreateMaster(client);
            //var hr1 = dev1.ReadHoldingRegisters(1, 0, 10);

            //Router router = new Router();
            //router.RegisterMethods(dev1, nameof(dev1));

            //WebSocketServer wss = new WebSocketServer(router);
            //var host = "http://localhost:8888/";
            //wss.Start(host);
            //Console.WriteLine("Started");
            //Console.ReadKey();
            //wss.Stop();
            //client.Dispose();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Cts.Cancel();
        }
    }
}
