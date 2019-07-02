using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.MB
{

    public class Facility
    {
        public IEnumerable<IDevice> Devices { get; }
    }

    public interface IDevice
    {
        string Name { get; set; }

        IDictionary<TagInfo, ITag> Tags { get;}

        IEnumerable<TagData> Get(IEnumerable<TagInfo> tags);

        bool Write(IEnumerable<TagInfo> tags, IEnumerable<object> values);
    }


    public interface ITag
    {        
        TagData Data { get; }
    }

    public class TagInfo
    {
        public string Name { get; set; }

        public string Description { get; set; }

    }
    
    public class TagData
    {
        public TagQuality Quality { get; private set; }

        public DateTime Last { get; private set; }

        public DateTime LastGood { get; private set; }
    }
}
