
using System;

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
        /// "Нефтедобывающая компания"
        ///  └ "Цех 321"
        ///     └ "Месторождение 123"
        ///        └ "Куст 5"
        ///           └ "Скважина 456А"
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

    }
}
