namespace ModbusBasic
{
    public interface ISlaveHandlerContext
    {
        IModbusFunctionService GetHandler(byte functionCode);
    }
}