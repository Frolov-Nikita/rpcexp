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
        public string Description { get; set; }
        public string FullMethodName =>
            (ObjName != default) ? ObjName + "." + MethodName : MethodName;
        public object Obj { get; set; }
        public bool IsAsync { get; set; }

        public System.Reflection.ParameterInfo[] Parameters { get; set; }

        public Type ReturnType { get; set; }

        public int ParametersLength => Parameters?.Length ?? 0;

        public async Task<object> Invoke(object parametrs)
        {
            object[] args = null;

            var mastParamsCount = 0;
            if(Parameters != null)
                foreach (var p in Parameters)
                    if (!p.IsOptional)
                        mastParamsCount++;

            if ((parametrs is Newtonsoft.Json.Linq.JArray) && (ParametersLength > 0))
            {
                var ps = (Newtonsoft.Json.Linq.JArray)parametrs;
                if(mastParamsCount > ps.Count)
                    throw new ArgumentException();
                int argsCount = ParametersLength < ps.Count ? ParametersLength : ps.Count;
                args = new object[argsCount];
                for (var i = 0; i < argsCount; i++)
                    args[i] = ps[i].ToObject(Parameters[i].ParameterType);//Convert.ChangeType(((Newtonsoft.Json.Linq.JValue)ps[i]).Value, Parameters[i].ParameterType);
            }

            if ((parametrs is Newtonsoft.Json.Linq.JObject) && (ParametersLength > 0))
            {
                var ps = (Newtonsoft.Json.Linq.JObject)parametrs;
                if (mastParamsCount > ps.Count)
                    throw new ArgumentException();
                args = new object[ParametersLength];
                int i = 0;
                foreach (var p in Parameters)
                    args[i++] = ps[p.Name].ToObject(p.ParameterType); //Convert.ChangeType(((Newtonsoft.Json.Linq.JValue)ps[p.Name]).Value, p.ParameterType);
            }           

            var ret = Obj.GetType().InvokeMember(MethodName,
                System.Reflection.BindingFlags.InvokeMethod,
                null,
                Obj,
                args);

            if (IsAsync)
            {
                //return = await Convert.ChangeType(ret, ret.GetType());
                //return await ((Task<ret.GetType().GenericTypeArguments[0]>) ret);
                var tsk = (Task)ret;
                await tsk;
                return tsk.GetType().GetProperty("Result").GetValue(tsk);
            }
            else
                return ret;
            //return IsAsync ? await (Task<object>)ret : ret;
        }

    }

}
