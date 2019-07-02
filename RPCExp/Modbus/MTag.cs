using RPCExp.Common;
using RPCExp.Common.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RPCExp.Modbus
{

    public class MTag : IRange
    {
        private object value;

        private readonly Ticker ticker = new Ticker();

        public string Name { get; set; }


        //### специфично только для modbus
        public ModbusRegion Region { get; set; }

        public int Begin { get; set; }

        public int Length => TypeConv.ByteLength / 2;

        public int End => Begin + (Length - 1);

        //### специфично только для modbus */

        public long TimestampLast { get; private set; } = 0;

        public long TimestampSuccess { get; private set; } = 0;

        public long UpdatePeriod
        {
            get
            {
                long p = ticker.Cache;

                p = p > ticker.Period ? p : ticker.Period;

                return p;
            }
        }

        public bool IsActive { get; private set; } = true;

        public TagQuality Quality { get; private set; } = TagQuality.BAD;

        public TypeConverterAbstract TypeConv { get; set; } 

        public object GetValue()
        {
            ticker.Tick();
            return value;
        }

        internal object GetInternalValue() => value;

        internal void SetValue(Span<byte> data)
        {
            TimestampLast = DateTime.Now.Ticks;

            //TODO костыль с конвертером
            if ((Region == ModbusRegion.Coils) || (Region == ModbusRegion.DiscreteInputs))
                value = data[0] > 0;
            else
                value = TypeConv.GetValue(data);

            SetQty(TagQuality.GOOD);
        }

        internal void SetQty(TagQuality qty = TagQuality.GOOD)
        {
            if (qty == TagQuality.GOOD)
                TimestampSuccess = TimestampLast;
            Quality = qty;
        }
    }

}
