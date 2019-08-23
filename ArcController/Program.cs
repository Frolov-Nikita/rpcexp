using System;
using System.Linq;
using System.Collections.Generic;

namespace ArcController
{

    class Condition {

        class Argument
        {
            private string tagName = "";
            private decimal constValue = 0;

            public bool IsConst { get; set; } = false;

            public string TagName 
            { 
                get => tagName; 
                set 
                { 
                    tagName = value;
                    IsConst = false;
                }  
            }

            public decimal ConstValue { 
                get => constValue;
                set 
                {
                    constValue = value;
                    IsConst = true;
                } 
            }

            public override string ToString()
            {
                return IsConst ? ConstValue.ToString() : TagName;
            }
        }

        class Operator
        {
            public string Name { get; set; }

            public Func<object, object, bool> Predicate { get; set; }

            public Type Type { get; set; }

            public virtual object ArgCasting(object value) =>
                Convert.ChangeType(value, Type);
        }

        static System.Text.RegularExpressions.Regex regNum = new System.Text.RegularExpressions.Regex(@"^[\+\-]?[0-9\,\.]+$");

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

            {":", new Operator{Name = "!:",
                Type = typeof(Int32),
                Predicate = (a, b) => (((Int32)a) & (1 << ((Int32)b))) > 0 } } ,
            {"!:", new Operator{Name = "!:",
                Type = typeof(Int32),
                Predicate = (a, b) => (((Int32)a) & (1 << ((Int32)b))) == 0 } } ,
        };

        public Condition(string str)
        {
            FromString(str);
        }

        string OperatorName = "==";
        
        Argument[] Args = new Argument[2];

        public IEnumerable<string> Tags { get
            {
                return
                from a in Args
                where !a.IsConst
                select a.TagName;
            } 
        }

        public bool Check(params object[] args)
        {
            object[] a = new object[2];
            var op = Operators[OperatorName];

            for (var i = 0; i < 2; i++)
                a[i] = op.ArgCasting(
                    Args[i].IsConst ?
                        Args[i].ConstValue :
                        Convert.ChangeType(args[i], typeof(decimal)));
            
            return op.Predicate(a[0], a[1]);
        }
        
        void FromString(string str) {
            // Найти оператор            
            foreach(var o in Operators.Keys)
                if (str.Contains(o))
                {
                    OperatorName = o;
                    break;
                }
            //делим строку на 2 аргумента
            var args = str.Split(OperatorName, 2);
            for(var i = 0; i < 2; i++)
            {
                var arg = args[i].Trim();
                Args[i] = new Argument();
                if (regNum.IsMatch(arg))
                    Args[i].ConstValue = decimal.Parse(arg);
                else
                    Args[i].TagName = arg;
            }
        }

        public override string ToString()
        {
            return Args[0] + " " + OperatorName + " " + Args[1];
        }
    }

    class Program
    {
        

        static void Main(string[] args) {
            //var c = new Condition("a : b");
            var c = new Condition("a !: 2");
            c.Check(1, 2);

            Console.WriteLine(" - ");
        }
    }
}
