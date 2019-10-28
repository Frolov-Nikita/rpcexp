using System;

namespace RPCExp.Store.Entities
{
    public class ArchiveCfg: INameDescription, ICopyFrom, IIdentity
    {
        public int Id { get; set; }
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void CopyFrom(object original)
        {
            throw new NotImplementedException();
        }
    }
}