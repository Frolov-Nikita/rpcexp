using RPCExp.Connections;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Store.Entities
{
    
    public class ConnectionSourceCfg: INameDescription, ICopyFrom, IIdentity
    {

        public ConnectionSourceCfg()
        {

        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Cfg { get; set; }

        public void CopyFrom(object original)
        {
            var src = (ConnectionSourceCfg) original;
            Name = src.Name;
            Description = src.Description;
            Cfg = src.Cfg;
        }
    }
}
