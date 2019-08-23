namespace RPCExp.Common
{
    public interface IServiceAbstract
    {
        ServiceState State { get; }

        void Start();
        void Stop();
    }
}