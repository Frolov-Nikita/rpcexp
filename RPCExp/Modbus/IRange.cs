namespace RPCExp.Modbus
{
    public interface IRange
    {
        int Begin { get; }
        int Length { get; }
        int End { get; }
    }
}
