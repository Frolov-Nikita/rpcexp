using RPCExp.Connections;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Store.Entities
{
    
    public class ConnectionSourceCfg: INameDescription, ICopyFrom, IIdentity
    {

        public int Id { get; set; }

        public string ClassName { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Cfg { get; set; }

        public void CopyFrom(object original)
        {
            var src = (ConnectionSourceCfg) original;
            ClassName = src.ClassName;
            Name = src.Name;
            Description = src.Description;
            Cfg = src.Cfg;
        }
    }
}
