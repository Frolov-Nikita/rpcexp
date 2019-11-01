using RPCExp.AlarmLogger.Model;
using System;

namespace RPCExp.Store.Entities
{
    public class AlarmCfg: INameDescription, ICopyFrom, IIdentity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Condition { get; set; } = "";

        public AlarmCategory Category { get; set; }

        public string TxtTemplate { get; set; } = "";

        public string CustomTag1 { get; set; } = "";

        public string CustomTag2 { get; set; } = "";

        public string CustomTag3 { get; set; } = "";

        public string CustomTag4 { get; set; } = "";

        public void CopyFrom(object original)
        {
            var src = (AlarmCfg)original;

            Name = src.Name;
            Description = src.Description;
            Condition = src.Condition;
            Category = src.Category;
            TxtTemplate = src.TxtTemplate;
            CustomTag1 = src.CustomTag1;
            CustomTag2 = src.CustomTag2;
            CustomTag3 = src.CustomTag3;
            CustomTag4 = src.CustomTag4;
        }
    }
}