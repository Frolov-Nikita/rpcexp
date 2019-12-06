using System;

namespace RPCExp.DbStore
{
    /// <summary>
    /// Global configuration for entire project miscellaneous constants.
    /// object of this class should be serialized/deserialized into json config file.
    /// </summary>
    public class GlobalConfig
    {
        /// <summary>
        /// Db file that contains configuration for restoring store object.
        /// </summary>
        public string DbConfigFile { get; set; } = "rpcExpCfg.sqlite3";

        /// <summary>
        /// Db file which is used by TagLogging service.
        /// </summary>
        public string TagLogServiceDbFile { get; set; } = "tagLog.sqlite3";
        
        /// <summary>
        /// Base period to check conditions to decide that tags value need to be saved
        /// </summary>
        public TimeSpan TagLogServiceCheckPeriod { get; set; } = new TimeSpan(0, 0, 1);

        /// <summary>
        /// Base period for service saving function
        /// </summary>
        public TimeSpan TagLogServiceSavePeriod { get; set; } = new TimeSpan(0, 0, 20);

        /// <summary>
        /// When TagLogging service fails this timespan will be used to wait. Needs to avoid big load on cyclic exceptions.
        /// </summary>
        public TimeSpan TagLogServiceOnErrorWait { get; set; } = new TimeSpan(0, 0, 10);

        /// <summary>
        ///  count of stored values to avoid disk space overfill.
        /// </summary>
        public int TagLogServiceStoreItemsCount { get; set; } = 16_777_216;

        /// <summary>
        /// Db file which is used by AlarmService.
        /// </summary>
        public string AlarmServiceDbFile { get; set; } = "alarmLog.sqlite3";

        /// <summary>
        /// Base period to check conditions of rising alarms
        /// </summary>
        public TimeSpan AlarmServiceCheckPeriod { get; set; } = new TimeSpan(0, 0, 1);

        /// <summary>
        /// Base period for service saving function
        /// </summary>
        public TimeSpan AlarmServiceSavePeriod { get; set; } = new TimeSpan(0, 0, 20);

        /// <summary>
        /// When AlarmLogging service fails this timespan will be used to wait. Needs to avoid big load on cyclic exceptions.
        /// </summary>
        public TimeSpan AlarmServiceOnErrorWait { get; set; } = new TimeSpan(0, 0, 10);
        /// <summary>
        ///  count of stored values to avoid disk space overfill.
        /// </summary>
        public int AlarmServiceStoreItemsCount { get; set; } = 16_777_216;

#pragma warning disable CA1819 // Свойства не должны возвращать массивы. Но иначе десериализация работает неверно.
        /// <summary>
        /// host:port parameter for WebSocketRpcServers listener
        /// </summary>
        public string[] WebSocketRpcServerHosts { get; set; } = new string[] { "http://localhost:8888/" };
#pragma warning restore CA1819 // Свойства не должны возвращать массивы
    }
}
