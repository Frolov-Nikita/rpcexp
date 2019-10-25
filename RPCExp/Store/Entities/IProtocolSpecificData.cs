namespace RPCExp.Store.Entities
{
    public interface IProtocolSpecificData
    {
        string ClassName { get; set; }

        string Custom { get; set; }
    }
}