using System.Collections.Generic;
using System.Linq;
using RPCExp.Modbus.TypeConverters;
using RPCExp.Common;

namespace RPCExp.Modbus.Factory
{
    public class MTagCfgWrapper : IClassWrapper<MTag>
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Units { get; set; }

        public string Format { get; set; }

        public string[] Groups { get; set; }

        public int Begin { get; set; }

        public bool CanWrite { get; set; }

        public ModbusRegion Region { get; set; }

        public ValueType ValueType { get; set; }

        public Scale Scale { get; set; }

        public void Wrap(MTag obj)
        {
            Name = obj.Name;
            DisplayName = obj.DisplayName;
            Description = obj.Description;
            Units = obj.Units;
            Format = obj.Format;
            Begin = obj.Begin;
            CanWrite = obj.CanWrite;
            Region = obj.Region;
            ValueType = obj.ValueType;
            Groups = obj.Groups.Keys?.ToArray();
            Scale = obj.Scale?.Clone() as Scale;
        }

        public MTag Unwrap() =>
            new MTag
            {
                Name = this.Name,
                DisplayName = this.DisplayName,
                Description = this.Description,
                Units = this.Units,
                Format = this.Format,
                Begin = this.Begin,
                CanWrite = this.CanWrite,
                Region = this.Region,
                ValueType = this.ValueType,
                Groups = new Dictionary<string, TagsGroup>(),
                Scale = this.Scale?.Clone() as Scale,
            };
    }

}
