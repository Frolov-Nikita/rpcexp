using NModbus;
using RPCExp.Modbus;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.Common
{

    public class Store
    {
        public List<Facility> Facilities { get; set; }

        //public Store()
        //{
        //    facilities = new List<Facility>();
        //}

        //public Common.TagData GetTagValue(int facilityId, int deviceId, int tagId)
        //{
        //    var facility = facilities.Find(f => f.Id == facilityId);
        //    if (facility == null)
        //        return null;
        //}
    }




}
