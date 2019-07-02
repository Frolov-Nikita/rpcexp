using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{
    public interface IDevice
    {
        string Name { get; set; }

        IDictionary<TagInfo, ITag> Tags { get; }

        IEnumerable<TagData> Get(IEnumerable<TagInfo> tags);

        bool Write(IEnumerable<TagInfo> tags, IEnumerable<object> values);
    }

    public abstract class DeviceAbstract : ServiceAbstract, IDevice
    {
        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IDictionary<TagInfo, ITag> Tags => throw new NotImplementedException();

        public IEnumerable<TagData> Get(IEnumerable<TagInfo> tags)
        {
            throw new NotImplementedException();
        }

        public bool Write(IEnumerable<TagInfo> tags, IEnumerable<object> values)
        {
            throw new NotImplementedException();
        }
    }
}
