using System;
using RPCExp.Modbus;
using System.Threading;
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

            var dbfilename = "cfg.sqlite3";
            var storeSource = new Store.SqliteStoreSource();

            //var store = StoreTemplateGen.Get();
            //storeSource.Save(store, dbfilename);
            //return;

            var store = storeSource.Load(dbfilename);


            store.TagLogService.Start();

            store.AlarmService.Start();

            // ==== WebSocket ====
            Router router = new Router();

            router.RegisterMethods(store, nameof(store));
            foreach (var facility in store.Facilities.Values)
                foreach (var device in facility.Devices.Values)
                {
                    var fullAccesName = facility.AccessName + Common.Store.nameSeparator + device.Name;
                    Console.WriteLine($"Start {fullAccesName}");
                    device.Start();
                    router.RegisterMethods(device, fullAccesName);
                }

            WebSocketServer wss = new WebSocketServer(router, new string[] { "http://localhost:8888/"}); //any Ip -  "http://*:8888/"

            Console.WriteLine("Start webSocket");
            wss.Start();
            Console.WriteLine("Press \"Escape\" to quit");

            while (!Cts.Token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                    if (Console.ReadKey().Key == ConsoleKey.Escape)
                        break;
                Thread.Sleep(1000);
            }

            wss.Stop();

            Console.Clear();
            return;

            //Console.WriteLine("Started");            
            //wss.Stop();
            //client.Dispose();
            //*/
        }

    }
}
