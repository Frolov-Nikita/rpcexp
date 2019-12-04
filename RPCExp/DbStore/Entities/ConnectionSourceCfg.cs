using System;

namespace RPCExp.DbStore.Entities
{

    public class ConnectionSourceCfg : INameDescription, ICopyFrom, IIdentity
    {

        public int Id { get; set; }

        public string ClassName { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Cfg { get; set; }

        public void CopyFrom(object original)
        {
            if (original is null)
                throw new ArgumentNullException(nameof(original));

            var src = (ConnectionSourceCfg)original;
            ClassName = src.ClassName;
            Name = src.Name;
            Description = src.Description;
            Cfg = src.Cfg;
        }
    }
}
