using RPCExp.Common;
using RPCExp.Modbus;
using RPCExp.Store.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace RPCExp.Store
{
    internal class ProtocolSerializerModbus : ProtocolSerializerAbstract
    {
        public override string ClassName => "Modbus";

        protected override string PackDeviceSpecific(DeviceAbstract device)
        {
            // SlaveId, 
            // ByteOrder, 
            // FrameType, 
            // ConnectionRef
            var mdev = (ModbusDevice)device;
            return JsonConvert.SerializeObject(new
            {
                mdev.SlaveId,
                mdev.ByteOrder,
                FrameType = mdev.MasterSource.frameType.ToString(),
                ConnectionRef = mdev.Connection.Name,
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
                device.MasterSource.frameType = (FrameType)jo["FrameType"].ToObject(typeof(FrameType));

            if (jo.ContainsKey("ConnectionRef"))
            {
                // TODO: 
                var connectionName = (FrameType)jo["ConnectionRef"].ToObject(typeof(string));
                // device.Connection =
            }

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
