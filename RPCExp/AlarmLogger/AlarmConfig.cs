using RPCExp.AlarmLogger.Entities;
using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPCExp.AlarmLogger
{
    /// <summary>
    /// Alarm configuration. 
    /// </summary>
    public class AlarmConfig
    {
        /// <summary>
        /// Condition for message triggering.
        /// </summary>
        public Condition Condition { get; private set; }

        /// <summary>
        /// Value for substitution
        /// </summary>
        public Argument Custom1 { get; private set; }

        public Argument Custom2 { get; private set; }

        public Argument Custom3 { get; private set; }

        public Argument Custom4 { get; private set; }

        /// <summary>
        /// Alarm information. Contain message text template, category, facility reference
        /// </summary>
        public AlarmInfo AlarmInfo { get; set; }

        private bool lastVal = false;

        /// <summary>
        /// Мертвая зона.
        /// Предназначена для фильтрации дребезга аварий.
        /// После срабатывания аварии возврат значения в 0 будет, если условие срабатывания не выполняется и аргументы условия отличаются на эту величину.
        /// </summary>
        public decimal DBandRValue { get; set; } = 0;

        /// <summary>
        /// В порядке ли все составные части аварии.
        /// True - все участвующие теги прочитаны и доступны (Quality >= 192)
        /// </summary>
        public bool IsOk
        {
            get
            {
                return
                    (Custom1?.IsOk ?? true) &&
                    (Custom2?.IsOk ?? true) &&
                    (Custom3?.IsOk ?? true) &&
                    (Custom4?.IsOk ?? true) &&
                    Condition.IsOk;

            }
        }

        /// <summary>
        /// Determine the front of triggered condition.
        /// </summary>
        /// <returns></returns>
        public bool IsRized()
        {
            if (!Condition.IsOk)
                return false;

            var val = Condition.Check();

            // dband
            if ((lastVal == true) && (val == false))
                val = Condition.InDBand(DBandRValue);

            var retval = val && (!lastVal);
            lastVal = val;
            return retval;
        }

        /// <summary>
        /// Create full AlarmConfig from db entity AlarmCfg
        /// </summary>
        /// <param name="alarmCfg">db entity</param>
        /// <param name="tags"></param>
        /// <param name="alarmInfo"></param>
        /// <returns></returns>
        public static AlarmConfig From(DbStore.Entities.AlarmCfg alarmCfg, IEnumerable<TagAbstract> tags, AlarmInfo alarmInfo)
        {
            if (alarmCfg is null)
                throw new ArgumentNullException(nameof(alarmCfg));

            var config = new AlarmConfig
            {
                Condition = Condition.From(alarmCfg.Condition, tags)
            };

            if (alarmCfg.DBandRValue != default)
                config.DBandRValue = alarmCfg.DBandRValue;

            if (alarmCfg.Custom1 != default)
                config.Custom1 = Argument.From(alarmCfg.Custom1, tags);

            if (alarmCfg.Custom2 != default)
                config.Custom2 = Argument.From(alarmCfg.Custom2, tags);

            if (alarmCfg.Custom3 != default)
                config.Custom3 = Argument.From(alarmCfg.Custom3, tags);

            if (alarmCfg.Custom4 != default)
                config.Custom4 = Argument.From(alarmCfg.Custom4, tags);

            config.AlarmInfo = alarmInfo;

            return config;
        }
    }

    /// <summary>
    /// Condition for triggering the message.
    /// It contains operator and its arguments.
    /// </summary>
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

        private class Operator
        {
            public string Name { get; set; }

            public Func<object, object, bool> Predicate { get; set; }

            public Type Type { get; set; }

            public bool UseDBand { get; set; } = false;

#pragma warning disable CA1305 // Укажите IFormatProvider
            private object ArgCasting(object value) =>
                Convert.ChangeType(value, Type);
#pragma warning restore CA1305 // Укажите IFormatProvider

            public bool Check(object arg1, object arg2)
            {
                var a1 = ArgCasting(arg1);
                var a2 = ArgCasting(arg2);
                return Predicate(a1, a2);
            }
        }

        private static readonly Dictionary<string, Operator> Operators = new Dictionary<string, Operator>()
        {
            {">=", new Operator{Name = ">=",
                Type = typeof(decimal),
                Predicate = (a, b) => ((decimal)a) >= ((decimal)b),
                UseDBand = true,
            } } ,
            {"<=", new Operator{Name = "<=",
                Type = typeof(decimal),
                Predicate = (a, b) => ((decimal)a) <= ((decimal)b),
                UseDBand = true,
             } } ,

            {"==", new Operator{Name = "==",
                Type = typeof(decimal),
                Predicate = (a, b) => ((decimal)a) == ((decimal)b),
                UseDBand = true,
             } } ,
            {"!=", new Operator{Name = "!=",
                Type = typeof(decimal),
                Predicate = (a, b) => ((decimal)a) != ((decimal)b),
                UseDBand = true,
             } } ,

            {">", new Operator{Name = ">",
                Type = typeof(decimal),
                Predicate = (a, b) => ((decimal)a) > ((decimal)b),
                UseDBand = true,
             } } ,
            {"<", new Operator{Name = "<",
                Type = typeof(decimal),
                Predicate = (a, b) => ((decimal)a) < ((decimal)b),
                UseDBand = true,
             } } ,

            {"!:", new Operator{Name = "!:",
                Type = typeof(Int32),
                Predicate = (a, b) => (((Int32)a) & (1 << ((Int32)b))) == 0 } } ,
            {":", new Operator{Name = ":",
                Type = typeof(Int32),
                Predicate = (a, b) => (((Int32)a) & (1 << ((Int32)b))) > 0 } } ,
        };

        private Argument[] Arguments { get; set; }

        /// <summary>
        /// В порядке ли аргументы?
        /// </summary>
        public bool IsOk =>
            Arguments[0].IsOk &&
            Arguments[1].IsOk;

        /// <summary>
        /// Выполняется ли условие
        /// </summary>
        /// <returns></returns>
        public bool Check()
            => Operators[operatorName].Check(Arguments[0].GetValue(), Arguments[1].GetValue());

        /// <summary>
        /// Определяет находятся ли аргументы условия внутри мертвой зоны друг друга.
        /// </summary>
        /// <param name="dband">величина мертвой зоны</param>
        /// <returns>true- находятся</returns>
        public bool InDBand(decimal dband = 0M)
        {
            if ((dband == 0) || (!IsOk) || (!Operators[operatorName].UseDBand))
                return false;

            var a0 = Arguments[0].GetValue();
            var a1 = Arguments[1].GetValue();

            return (a0 < (a1 + dband)) && (a0 > (a1 - dband));
        }

        /// <summary>
        /// Разбор строки условия и привязка аргументов к тегам (сохраняем ссылку)
        /// </summary>
        /// <param name="conditionString">Строка условия</param>
        /// <param name="tags">Список тегов из которых производится привязка</param>
        /// <returns>объект условия</returns>
        public static Condition From(string conditionString, IEnumerable<TagAbstract> tags)
        {
            string operatorName = default;
            foreach (var o in Operators.Keys)
                if (conditionString.Contains(o, StringComparison.OrdinalIgnoreCase))
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

            return new Condition(operatorName, args[0], args[1]);
        }
    }

    /// <summary>
    /// Source of the value for the condition. It can be reference for tag, or constant value.
    /// </summary>
    public class Argument
    {
        private readonly TagAbstract tag;
        private readonly decimal constValue;

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
            this.constValue = constValue;
        }

        public Argument(int constValue)
        {
            IsConst = true;
            this.constValue = constValue;
        }

        public Argument(Int16 constValue)
        {
            IsConst = true;
            this.constValue = constValue;
        }

        public Argument(uint constValue)
        {
            IsConst = true;
            this.constValue = constValue;
        }

        public Argument(UInt16 constValue)
        {
            IsConst = true;
            this.constValue = constValue;
        }

        private bool IsConst { get; set; } = false;

        public bool IsOk => IsConst || ((tag?.Quality ?? 0) >= TagQuality.GOOD);

        /// <summary>
        /// Gets the value of this argument.
        /// </summary>
        /// <returns></returns>
        public decimal GetValue()
        {
            if (IsConst)
                return constValue;

#pragma warning disable CA1305 // Укажите IFormatProvider
            return (decimal)Convert.ChangeType(tag?.Value ?? 0, typeof(decimal));
#pragma warning restore CA1305 // Укажите IFormatProvider
        }

        private static readonly System.Text.RegularExpressions.Regex regNumber = new System.Text.RegularExpressions.Regex(@"^[\+\-]?[0-9\,\.]+$");

        //static readonly System.Text.RegularExpressions.Regex regString = new System.Text.RegularExpressions.Regex("^\\\".*\\\"$");
        private static readonly System.Text.RegularExpressions.Regex regTagName = new System.Text.RegularExpressions.Regex("^[_a-zA-Zа-яА-Я]+[_a-zA-Zа-яА-Я0-9]*$");
        private static readonly TagsGroup AlarmsTagGroup = new TagsGroup(new BasicPeriodSource())
        {
            Name = "AlarmsTagGroup",
            Description = "Tags group to periodically check alarms",
            Min = 2 * 10_000_000,
        };


        /// <summary>
        /// Deserialization of argument from string.
        /// If string contains number, argument will be constant.
        /// If string contains tag name, argument will be contain the tag reference from tags list.
        /// If tag doesn't exist in the list the ArgumentException will be thrown.
        /// </summary>
        /// <param name="str">name of the tag or number</param>
        /// <param name="tags">list of tags</param>
        /// <returns></returns>
        public static Argument From(string str, IEnumerable<TagAbstract> tags)
        {
            var s = str?.Trim() ?? "";
            if (regNumber.IsMatch(s))
#pragma warning disable CA1305 // Укажите IFormatProvider
                return new Argument(decimal.Parse(s));
#pragma warning restore CA1305 // Укажите IFormatProvider

            if (regTagName.IsMatch(s))
            {
                var tag = tags.FirstOrDefault(t => t.Name == s);
                if (tag == default)
                    throw new ArgumentException($"Argument tag \'{s}\' doesn't found.");

                if (!tag.Groups.ContainsKey(AlarmsTagGroup.Name))
                    tag.Groups.AddByName(AlarmsTagGroup);

                return new Argument(tag);
            }

            throw new ArgumentException($"Unknown argument \'{s}\'.");
        }
    }
}
