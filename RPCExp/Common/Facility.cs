
using System;
using System.Linq;
using System.Collections.Generic;


namespace RPCExp.Common
{
    /// <summary>
    /// An automation object. Can contain devices (controllers, plcs, etc.)
    /// </summary>
    public class Facility : INameDescription
    {
        /// <summary>
        /// Identity
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Long name which contains the path of the tree automation objects
        /// </summary>
        /// <example>
        /// for example nameSeparator == "$"
        /// AccessName == "Нефтедобывающая компания$Цех 321$Месторождение 123$Куст 5$Скважина 456А"
        /// The tree should looks like:
        /// <para>"Нефтедобывающая компания"</para>
        /// <para> └ "Цех 321"</para>
        /// <para>    └ "Месторождение 123"</para>
        /// <para>       └ "Куст 5"</para>
        /// <para>          └ "Скважина 456А"</para>
        /// </example>
        public string AccessName { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <summary>
        /// Collection of automation devices inside the automation object
        /// </summary>
        public IDictionary<string, DeviceAbstract> Devices { get; } = new Dictionary<string, DeviceAbstract>();

        /// <summary>
        /// Получает имена всех устройств и их групп
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> GetDevices() =>
                from device in Devices.Values
                select new {
                    device.Name,
                    device.Description,
                    device.State,
                    Groups = device.Groups.Keys,
                };

    }
}
