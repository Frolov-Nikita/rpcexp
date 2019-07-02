using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NModbus;
using RPCExp.Modbus;
using RPCExp.Common;
using RPCExp.Common.TypeConverters;
using System.Threading;
using System.Threading.Tasks;


using Newtonsoft.Json;

namespace RPCExp
{
    class Program
    {
        static CancellationTokenSource Cts = new CancellationTokenSource();

        static void Main(string[] args)
        {

            //var st = new Ticker();
            //string TickToSecStr(long ticks) => (ticks/10_000_000.0).ToString("#.###");
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
                        Devices = new List<Device>
                        {
                            new Device
                            {
                                SlaveId = 1,
                                Tags = new List<MTag>
                                {
                                    new MTag
                                    {
                                        Region = ModbusRegion.HoldingRegisters,
                                        Name = "Tag1",
                                        Begin = 2,
                                        TypeConv = new TypeConverterInt16(DefaultByteOrder),
                                    },
                                    new MTag
                                    {
                                        Region = ModbusRegion.HoldingRegisters,
                                        Name = "Tag2",
                                        Begin = 3,
                                        TypeConv = new TypeConverterFloat(DefaultByteOrder),
                                    },
                                    new MTag
                                    {
                                        Region = ModbusRegion.Coils,
                                        Name = "BoolTag1",
                                        Begin = 500,
                                        TypeConv = new TypeConverterBool(DefaultByteOrder),
                                    },
                                }
                            }
                        }
                    }
                }
            };

            //var cfg = JsonConvert.SerializeObject(store, Formatting.Indented);
            //System.IO.File.WriteAllText(@"cfg.json", cfg);
            //return;

            store.Facilities[0].Devices[0].Start();
            
            Console.CancelKeyPress += Console_CancelKeyPress;
            
            while (!Cts.Token.IsCancellationRequested)
            {
                TermForms.DisplayDevice(store.Facilities[0].Devices[0]);
                
                Thread.Sleep(500);
            }

            //store.Facilities[0].Devices[0].main.Wait();
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
