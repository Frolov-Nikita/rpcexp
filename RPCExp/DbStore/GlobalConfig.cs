using System;

namespace RPCExp.DbStore
{
    public class GlobalConfig
    {
        public string DbConfigFile { get; set; } = "rpcExpCfg.sqlite3";

        public string TagLogServiceDbFile { get; set; } = "tagLog.sqlite3";
        public TimeSpan AlarmServiceCheckPeriod { get; set; } = new TimeSpan(0, 0, 1);
        public TimeSpan AlarmServiceSavePeriod { get; set; } = new TimeSpan(0, 0, 20);
        public TimeSpan AlarmServiceOnErrorWait { get; set; } = new TimeSpan(0, 0, 10);
        public int AlarmServiceStoreItemsCount { get; set; } = 16_777_216;

        public string AlarmServiceDbFile { get; set; } = "alarmLog.sqlite3";
        public TimeSpan TagLogServiceCheckPeriod { get; set; } = new TimeSpan(0, 0, 1);
        public TimeSpan TagLogServiceSavePeriod { get; set; } = new TimeSpan(0, 0, 20);
        public TimeSpan TagLogServiceOnErrorWait { get; set; } = new TimeSpan(0, 0, 10);
        public int TagLogServiceStoreItemsCount { get; set; } = 16_777_216;

#pragma warning disable CA1819 // Свойства не должны возвращать массивы. Но иначе десериализация работает неверно.
        public string[] WebSocketServerHosts { get; set; } = new string[] { "http://localhost:8888/" };
#pragma warning restore CA1819 // Свойства не должны возвращать массивы
    }
}
