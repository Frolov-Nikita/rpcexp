namespace RPCExp.Modbus.Factory
{
    public interface IClassWrapper<T>
    {
        void Wrap(T obj);

        T Unwrap();
    }

}
