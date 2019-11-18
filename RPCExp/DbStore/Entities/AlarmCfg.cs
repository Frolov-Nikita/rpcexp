using RPCExp.AlarmLogger;
using RPCExp.AlarmLogger.Entities;
using System;

namespace RPCExp.DbStore.Entities
{
    public class AlarmCfg: INameDescription, ICopyFrom, IIdentity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Condition { get; set; } = "";

        public AlarmCategory Category { get; set; }

        public string TemplateTxt { get; set; } = "";

        public string Custom1 { get; set; } = "";

        public string Custom2 { get; set; } = "";

        public string Custom3 { get; set; } = "";

        public string Custom4 { get; set; } = "";

        public void CopyFrom(object original)
        {
            
            var src = (AlarmCfg)original ?? throw new ArgumentNullException(nameof(original)); 

            Name = src.Name;
            Description = src.Description;
            Condition = src.Condition;
            Category = src.Category;
            TemplateTxt = src.TemplateTxt;
            Custom1 = src.Custom1;
            Custom2 = src.Custom2;
            Custom3 = src.Custom3;
            Custom4 = src.Custom4;
        }
    }
}