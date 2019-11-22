using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using RPCExp.DbStore;
using RPCExp.AlarmLogger.Entities;
using RPCExp.TagLogger.Entities;
using Microsoft.EntityFrameworkCore;

namespace RPCExp
{
    class Program
    {
        static readonly CancellationTokenSource Cts = new CancellationTokenSource();

        static void Main(/*string[] args*/)
        {

            var global = GlobalConfigFactory.Get();

            var storeSource = new SqliteStoreSource();
            

            //var store = StoreTemplateGen.Get();
            //storeSource.Save(store, dbfilename);
            //return;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var store = storeSource.Get(global.DbConfigFile);
            Console.WriteLine($"Store loaded {stopwatch.ElapsedMilliseconds}ms");

            store.TagLogService.FileName = global.TagLogServiceDbFile;
            store.TagLogService.CheckPeriod = global.TagLogServiceCheckPeriod;
            store.TagLogService.SavePeriod = global.TagLogServiceSavePeriod;
            store.TagLogService.StoreItemsCount = global.TagLogServiceStoreItemsCount;
            //store.TagLogService.Start();

            store.AlarmService.FileName = global.AlarmServiceDbFile;
            store.AlarmService.CheckPeriod = global.AlarmServiceCheckPeriod;
            store.AlarmService.SavePeriod = global.AlarmServiceSavePeriod;
            store.AlarmService.StoreItemsCount = global.AlarmServiceStoreItemsCount;
            store.AlarmService.Start();

            // ==== WebSocket ====
            Router router = new Router();

            router.RegisterMethods(store, nameof(store));
            router.RegisterMethods(store.AlarmService, "Alarms");
            router.RegisterMethods(store.TagLogService, "TagLog");
            foreach (var facility in store.Facilities.Values)
                foreach (var device in facility.Devices.Values)
                {
                    var fullAccesName = facility.AccessName + Common.Store.nameSeparator + device.Name;
                    Console.WriteLine($"Start {fullAccesName}");
                    device.Start();
                    router.RegisterMethods(device, fullAccesName);
                }

            WebSocketServer wss = new WebSocketServer(router, global.WebSocketServerHosts); //any Ip - "http://*:8888/"; "http://localhost:8888/"

            Console.WriteLine($"Start webSocket at {global.WebSocketServerHosts[0]}");
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
