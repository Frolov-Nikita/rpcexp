using System;
using System.Collections.Generic;

namespace RPCExp.AlarmLogger.Model
{
    public class AlarmConfig: IValidatable
    {
        Condition condition;

        bool lastValue = false;

        //public int Id { get; set; }
        
        public string Condition { 
            get => condition?.ToString() ?? "";
            set => condition = new Condition(value);
        }

        public IEnumerable<string> ConditionRelatedTags  => condition.Tags;

        public AlarmCategory Category { get; set; }

        public string TxtTemplate { get; set; } = "";

        public string CustomTag1 { get; set; } = "";

        public string CustomTag2 { get; set; } = "";

        public string CustomTag3 { get; set; } = "";

        public string CustomTag4 { get; set; } = "";

        public bool IsValid => 
            (TxtTemplate.Length > 0) &&
            (condition?.IsValid ?? false);

        public bool IsRise(params object[] args)
        {
            if (condition == default)
                return false;

            var cv = condition.Check(args);            
            var retval = (lastValue == false) && (cv == true);
            lastValue = cv;
            return retval;
        }
        
        public string TxtMessage(Alarm alarm)
        {
            if (alarm == null)
                return "";

            var result = TxtTemplate;
            result = alarm.Custom1 != default ? "{Custom1}" : result;
            result = alarm.Custom2 != default ? "{Custom2}" : result;
            result = alarm.Custom3 != default ? "{Custom3}" : result;
            result = alarm.Custom4 != default ? "{Custom4}" : result;

            return result;
        }
    }
}
