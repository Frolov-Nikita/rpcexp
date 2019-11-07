using RPCExp.AlarmLogger.Entities;
using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPCExp.AlarmLogger
{
    //TODO: throw заменить на более дешевую диагностику.

    public class AlarmConfig
    {
        public Condition Condition { get; set; }

        public Argument Custom1 { get; set; }

        public Argument Custom2 { get; set; }

        public Argument Custom3 { get; set; }

        public Argument Custom4 { get; set; }

        public AlarmInfo AlarmInfo { get; set; }

        bool lastVal = false;

        public bool IsOk { get
            {
                return
                    (Custom1?.IsOk ?? true )&&
                    (Custom2?.IsOk ?? true )&&
                    (Custom3?.IsOk ?? true )&&
                    (Custom4?.IsOk ?? true )&&
                    Condition.IsOk;

            } 
        }

        public bool IsRized()
        {
            if (!Condition.IsOk)
                return false;
            var val = Condition.Check();
            var retval = val && (!lastVal);
            lastVal = val;
            return retval;
        }

        public static AlarmConfig From(Store.Entities.AlarmCfg alarmCfg, IEnumerable<TagAbstract> tags, AlarmInfo alarmInfo)
        {
            var config = new AlarmConfig();

            config.Condition = Condition.From(alarmCfg.Condition, tags);            

            if(alarmCfg.Custom1 != default)
                config.Custom1 = Argument.From(alarmCfg.Custom1, tags);

            if (alarmCfg.Custom2 != default)
                config.Custom2 = Argument.From(alarmCfg.Custom2, tags);

            if (alarmCfg.Custom2 != default)
                config.Custom3 = Argument.From(alarmCfg.Custom3, tags);

            if (alarmCfg.Custom4 != default)
                config.Custom4 = Argument.From(alarmCfg.Custom4, tags);

            config.AlarmInfo = alarmInfo;
            
            return config;
        }
    }

    public class Condition
    {
        
        private readonly string operatorName = "==";

        public Condition(string operatoreName, Argument arg1, Argument arg2)
        {
            this.operatorName = operatoreName;
            Arguments = new Argument[]
            {
                arg1, arg2
            };
        }

        class Operator
        {
            public string Name { get; set; }

            public Func<object, object, bool> Predicate { get; set; }

            public Type Type { get; set; }

            object ArgCasting(object value) =>
                Convert.ChangeType(value, Type);

            public bool Check(object arg1, object arg2)
            {
                var a1 = ArgCasting(arg1);
                var a2 = ArgCasting(arg2);
                return Predicate(a1, a2);
            }
        }

        static Dictionary<string, Operator> Operators = new Dictionary<string, Operator>()
        {
            {">=", new Operator{Name = ">=",
                Type = typeof(decimal),
                Predicate = (a, b) => ((decimal)a) >= ((decimal)b) } } ,
            {"<=", new Operator{Name = "<=",
                Type = typeof(decimal),
                Predicate = (a, b) => ((decimal)a) <= ((decimal)b) } } ,

            {"==", new Operator{Name = "==",
                Type = typeof(decimal),
                Predicate = (a, b) => ((decimal)a) == ((decimal)b) } } ,
            {"!=", new Operator{Name = "!=",
                Type = typeof(decimal),
                Predicate = (a, b) => ((decimal)a) != ((decimal)b) } } ,

            {">", new Operator{Name = ">",
                Type = typeof(decimal),
                Predicate = (a, b) => ((decimal)a) > ((decimal)b) } } ,
            {"<", new Operator{Name = "<",
                Type = typeof(decimal),
                Predicate = (a, b) => ((decimal)a) < ((decimal)b) } } ,

            {":", new Operator{Name = ":",
                Type = typeof(Int32),
                Predicate = (a, b) => (((Int32)a) & (1 << ((Int32)b))) > 0 } } ,
            {"!:", new Operator{Name = "!:",
                Type = typeof(Int32),
                Predicate = (a, b) => (((Int32)a) & (1 << ((Int32)b))) == 0 } } ,
        };

        Argument[] Arguments { get; set; }

        public bool IsOk =>
            Arguments[0].IsOk && 
            Arguments[1].IsOk;

        public bool Check()
            => Operators[operatorName].Check(Arguments[0].GetValue(), Arguments[1].GetValue());

        public static Condition From(string conditionString,  IEnumerable<TagAbstract> tags)
        {
            string operatorName = default;
            foreach (var o in Operators.Keys)
                if (conditionString.Contains(o))
                {
                    operatorName = o;
                    break;
                }
            if (operatorName == default)
                throw new ArgumentException($"Operator not found in condition \'{conditionString}\'.");

            var argsTxt = conditionString.Split(operatorName, 2);
            Argument[] args = new Argument[argsTxt.Length];

            for (var i = 0; i < argsTxt.Length; i++)
            {
                argsTxt[i] = argsTxt[i].Trim();
                args[i] = Argument.From(argsTxt[i], tags);
            }

            return new Condition(operatorName, args[0], args[1]) ;
        }
    }

    /// <summary>
    /// Источник значения. Может быть константой или тегом.
    /// </summary>
    public class Argument
    {
        private TagAbstract tag;
        private decimal constValue;

        public Argument(TagAbstract tag)
        {
            IsConst = false;
            this.tag = tag;
        }

        public Argument(decimal constValue)
        {
            IsConst = true;
            this.constValue = constValue;
        }

        public Argument(double constValue)
        {
            IsConst = true;
            this.constValue = (decimal)constValue;
        }

        public Argument(float constValue)
        {
            IsConst = true;
            this.constValue = (decimal)constValue;
        }

        public Argument(long constValue)
        {
            IsConst = true;
            this.constValue = (decimal)constValue;
        }

        public Argument(int constValue)
        {
            IsConst = true;
            this.constValue = (decimal)constValue;
        }

        public Argument(Int16 constValue)
        {
            IsConst = true;
            this.constValue = (decimal)constValue;
        }

        public Argument(uint constValue)
        {
            IsConst = true;
            this.constValue = (decimal)constValue;
        }

        public Argument(UInt16 constValue)
        {
            IsConst = true;
            this.constValue = (decimal)constValue;
        }

        bool IsConst { get; set; } = false;

        public bool IsOk => IsConst || ((tag?.Quality ?? 0) >= TagQuality.GOOD);

        public decimal GetValue()
        {
            if (IsConst)
                return constValue;

            return (decimal)Convert.ChangeType(tag?.GetValue() ?? 0, typeof(decimal));
        }

        static readonly System.Text.RegularExpressions.Regex regNumber = new System.Text.RegularExpressions.Regex(@"^[\+\-]?[0-9\,\.]+$");
        static readonly System.Text.RegularExpressions.Regex regString = new System.Text.RegularExpressions.Regex("^\\\".*\\\"$");
        static readonly System.Text.RegularExpressions.Regex regTagName = new System.Text.RegularExpressions.Regex("^[_a-zA-Zа-яА-Я]+[_a-zA-Zа-яА-Я0-9]*$");
        static TagsGroup AlarmsTagGroup = new TagsGroup(new BasicPeriodSource()) {
            Name = "AlarmsTagGroup", 
            Description = "Tags group to periodicly check alarms",
            Min = 2 * 10_000_000,
        };

        public static Argument From(string str, IEnumerable<TagAbstract> tags)
        {
            if (regNumber.IsMatch(str))
                return new Argument(decimal.Parse(str));

            if (regTagName.IsMatch(str))
            {
                var tag = tags.FirstOrDefault(t => t.Name == str);
                if (tag == default)
                    throw new ArgumentException($"Argument tag \'{str}\' doesn`t found.");

                if (!tag.Groups.ContainsKey(AlarmsTagGroup.Name))
                    tag.Groups.AddByName(AlarmsTagGroup);

                return new Argument(tag);
            }

            throw new ArgumentException($"Unknown argument \'{str}\'.");
        }
    }
}
