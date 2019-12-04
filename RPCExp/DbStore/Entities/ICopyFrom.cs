namespace RPCExp.DbStore.Entities
{
    /// <summary>
    /// Копирует все, кроме ID
    /// </summary>
    public interface ICopyFrom
    {
        void CopyFrom(object original);
    }
}
