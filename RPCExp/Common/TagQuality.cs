using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Идентификаторы не должны содержать символы подчеркивания", Justification = "<Ожидание>")]
    public enum TagQuality
    {
        /// <summary>
        /// Плохое
        /// </summary>
        BAD = 0x0,
        /// <summary>
        /// Ошибка в конфигурации (например не существующий COM порт, или адрес у устройстве)
        /// </summary>
        BAD_CONFIGURATION_ERROR = 0x4,
        /// <summary>
        /// Ошибка подключения
        /// </summary>
        BAD_NOT_CONNECTED = 0x8,
        /// <summary>
        /// Самодиагностика устройства выдает неполадку
        /// </summary>
        BAD_DEVICE_FAILURE = 0xC,
        /// <summary>
        /// Ошибка датчика
        /// </summary>
        BAD_SENSOR_FAILURE = 0x10,
        /// <summary>
        /// Ошибка связи, но количество попыток еще не исчерпано и период достоверности значения тоже
        /// </summary>
        BAD_LAST_KNOWN_VALUE = 0x14,
        /// <summary>
        /// Ошибка связи
        /// </summary>
        BAD_COMM_FAILURE = 0x18,
        /// <summary>
        /// Опрос отключен намеренно
        /// </summary>
        BAD_OUT_OF_SERVICE = 0x1C,
        /// <summary>
        /// Вероятно не достоверное (из обрывка пакета данных)
        /// </summary>
        UNCERTAIN = 0x40,
        UNCERTAIN_LAST_USABLE_VALUE = 0x44,
        UNCERTAIN_SENSOR_NOT_ACCURATE = 0x50,
        UNCERTAIN_ENG_UNITS_EXCEEDED = 0x54,
        UNCERTAIN_SUB_NORMAL = 0x58,
        //---------
        //NOT_AVIABLE = 0x80,
        //---------
        /// <summary>
        /// хорошее
        /// </summary>
        GOOD = 0xC0,
        /// <summary>
        /// подмененное
        /// </summary>
        GOOD_LOCAL_OVERRIDE = 0xD8,
    }
}
