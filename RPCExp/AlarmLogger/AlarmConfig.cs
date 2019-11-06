using RPCExp.AlarmLogger.Entities;
using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.AlarmLogger
{
    public class AlarmConfig
    {
        static System.Text.RegularExpressions.Regex regNum = new System.Text.RegularExpressions.Regex(@"^[\+\-]?[0-9\,\.]+$");

        public Condition Condition { get; set; }

        public Argument CustomTag1 { get; set; }

        public Argument CustomTag2 { get; set; }

        public Argument CustomTag3 { get; set; }

        public Argument CustomTag4 { get; set; }

        public AlarmInfo AlarmInfo { get; set; }

        bool lastVal = false;

        public bool IsRized()
        {
            if (!Condition.IsOk)
                return false;
            var val = Condition.Check();
            var retval = val && (!lastVal);
            lastVal = val;
            return retval;
        }
    }

    public class Condition
    {
        
        string operatorName = "==";

        public Condition(string operatoreName, Argument arg1, Argument arg2)
        {
            this.operatoreName = operatoreName;
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
        private readonly string operatoreName;

        Argument[] Arguments { get; set; }

        public bool IsOk =>
            Arguments[0].IsOk && 
            Arguments[1].IsOk;

        public bool Check()
            => Operators[operatorName].Check(Arguments[0], Arguments[1]);
    }

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
    }
}
