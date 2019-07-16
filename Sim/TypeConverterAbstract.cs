namespace Sim
{
    public abstract class TypeConverterAbstract
    {
        public abstract ModbusValueType ValueType { get; }

        public int ByteLength => GetByteLength(ValueType);

        public ushort WordsLength => (ushort)(ByteLength / 2);

        public abstract object FromWords(ushort[] buffer);

        public abstract ushort[] ToWords(object value);

        public static int GetByteLength(ModbusValueType modbusValueType)
        {
            switch (modbusValueType)
            {
                case ModbusValueType.Bool:
                    return 2;
                case ModbusValueType.Float:
                    return 4;
                case ModbusValueType.Int16:
                    return 2;
                case ModbusValueType.Int32:
                    return 4;
                default:
                    return 2;
            }
        }
    }
}