using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RPCExp
{
    public class RpcMethod
    {

        public string ObjName { get; set; }
        public string MethodName { get; set; }
        public string FullMethodName =>
            (ObjName != default) ? ObjName + "." + MethodName : MethodName;
        public object Obj { get; set; }
        public bool IsAsync { get; set; }
        //public Dictionary<string, Type> Parameters { get; set; }
        public System.Reflection.ParameterInfo[] Parameters { get; set; }

        public async Task<object> Invoke(object parametrs)
        {
            object[] args = null;

            var mastParamsCount = 0;
            foreach (var p in Parameters)
                if (!p.IsOptional)
                    mastParamsCount++;

            try
            {
                if (parametrs is Newtonsoft.Json.Linq.JArray)
                {
                    var ps = (Newtonsoft.Json.Linq.JArray)parametrs;
                    if(mastParamsCount > ps.Count)
                        throw new ArgumentException();
                    int argsCount = Parameters.Length < ps.Count ? Parameters.Length : ps.Count;
                    args = new object[argsCount];
                    for (var i = 0; i < argsCount; i++)
                        args[i] = Convert.ChangeType(((Newtonsoft.Json.Linq.JValue)ps[i]).Value, Parameters[i].ParameterType);
                }

                if (parametrs is Newtonsoft.Json.Linq.JObject)
                {
                    var ps = (Newtonsoft.Json.Linq.JObject)parametrs;
                    if (mastParamsCount > ps.Count)
                        throw new ArgumentException();
                    args = new object[Parameters.Length];
                    int i = 0;
                    foreach (var p in Parameters)
                        args[i++] = Convert.ChangeType(((Newtonsoft.Json.Linq.JValue)ps[p.Name]).Value, p.ParameterType);
                }
            }
            catch
            {
                throw new ArgumentException();
            }            

            var ret = Obj.GetType().InvokeMember(MethodName,
                System.Reflection.BindingFlags.InvokeMethod,
                null,
                Obj,
                args);

            return IsAsync ? await (Task<object>)ret : ret;
        }

    }

}
