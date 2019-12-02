using RPCExp.Common;
using RPCExp.Modbus;
using RPCExp.DbStore.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RPCExp.DbStore.Serializers
{
    internal class ProtocolSerializerModbus : ProtocolSerializerAbstract
    {
        public override string ClassName => "Modbus";

        protected override string PackDeviceSpecific(DeviceAbstract device)
        {
            // SlaveId, 
            // ByteOrder, 
            // FrameType, 
            // MaxGroupLength
            // MaxGroupSpareLength
            var mdev = (ModbusDevice)device;
            return JsonConvert.SerializeObject(new
            {
                mdev.SlaveId,
                mdev.ByteOrder,
                FrameType = mdev.FrameType.ToString(),
                mdev.MaxGroupLength,
                mdev.MaxGroupSpareLength,
            });            
        }

        protected override DeviceAbstract UnpackDeviceSpecific(string custom)
        {
            var device = new ModbusDevice();

            var jo = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(custom);
            
            if (jo.ContainsKey(nameof(device.SlaveId)))
                device.SlaveId = (byte)jo[nameof(device.SlaveId)].ToObject(typeof(byte));

            if (jo.ContainsKey(nameof(device.ByteOrder)))
                device.ByteOrder = (byte[])jo[nameof(device.ByteOrder)].ToObject(typeof(byte[]));

            if (jo.ContainsKey("FrameType"))
                device.FrameType = (FrameType)jo["FrameType"].ToObject(typeof(FrameType));

            if (jo.ContainsKey(nameof(device.MaxGroupLength)))
                device.MaxGroupLength = (int)jo[nameof(device.MaxGroupLength)].ToObject(typeof(int));

            if (jo.ContainsKey(nameof(device.MaxGroupSpareLength)))
                device.MaxGroupSpareLength = (int)jo[nameof(device.MaxGroupSpareLength)].ToObject(typeof(int));

            return device;
        }

        protected override string PackTagSpecific(TagAbstract tag)
        {
            // Region
            // Begin
            var mtag = (MTag)tag;
            return JsonConvert.SerializeObject(new
            {
                mtag.Region,
                mtag.Begin,
            });
        }

        protected override TagAbstract UnpackTagSpecific(string custom)
        {
            // Region
            // Begin
            var jo = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(custom);
            var mtag = new MTag();

            if (jo.ContainsKey(nameof(mtag.Region)))
                mtag.Region = (ModbusRegion)jo[nameof(mtag.Region)].ToObject(typeof(ModbusRegion));

            if (jo.ContainsKey(nameof(mtag.Begin)))
                mtag.Begin = (int)jo[nameof(mtag.Begin)].ToObject(typeof(int));

            return mtag;
        }

    }
}
