using System;
using RPCExp.Modbus;
using System.Threading;
using RPCExp.Modbus.Factory;
using RPCExp.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RPCExp.Connections;

namespace RPCExp
{
    class Program
    {
        static CancellationTokenSource Cts = new CancellationTokenSource();

        static void Main(string[] args)
        {
            var storeSource = new Store.SqliteStoreSource();
            var store = new Common.Store();

            var f1 = FacilityTemplateGen.GetFacility();
            store.Facilities.Add(f1.Name, f1);

            storeSource.Save(store, "cfg.sqlite3");
            return;
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

            var facility = Factory.LoadFacility("cfg.json");
           
            Console.WriteLine("conf - ok");

            Factory.SaveFacility(facility);

            var dev = facility.Devices["Plc1"];
            dev.Start();

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
                    Terminal.TermForms.DisplayModbusDevice((ModbusDevice)dev);
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
