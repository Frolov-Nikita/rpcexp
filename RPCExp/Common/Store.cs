using System.Collections.Generic;

namespace RPCExp.Common
{

    public class Store : INameDescription
    {
        public List<Facility> Facilities { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }




}
