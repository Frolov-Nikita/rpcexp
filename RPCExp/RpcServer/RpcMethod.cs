using System;
using System.Globalization;
using System.Threading.Tasks;

namespace RPCExp.RpcServer
{
    /// <summary>
    /// container to store method, its object, parameters list and other related to rpc method
    /// </summary>
    public class RpcMethod
    {
        /// <summary>
        /// Access name of the object
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// Name of the method
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// some description if it is documented
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Full name contains object access name and method name concatenated by dot.
        /// </summary>
        public string FullMethodName =>
            (TargetName != default) ? TargetName + "." + MethodName : MethodName;

        /// <summary>
        /// reference to methods owner object
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// describe how to call this method
        /// </summary>
        public bool IsAsync { get; set; }

#pragma warning disable CA1819 // Свойства не должны возвращать массивы
        /// <summary>
        /// List of parameters information
        /// </summary>
        public System.Reflection.ParameterInfo[] Parameters { get; set; }
#pragma warning restore CA1819 // Свойства не должны возвращать массивы

        /// <summary>
        /// type that method should return
        /// </summary>
        public Type ReturnType { get; set; }

        /// <summary>
        /// count of parameters
        /// </summary>
        public int ParametersLength => Parameters?.Length ?? 0;

        /// <summary>
        /// Unified invoke function.
        /// </summary>
        /// <param name="parametrs"></param>
        /// <returns></returns>
        public async Task<object> InvokeAsync(object parametrs)
        {
            object[] args = null;

            var mastParamsCount = 0;
            if (Parameters != null)
                foreach (var p in Parameters)
                    if (!p.IsOptional)
                        mastParamsCount++;

            if ((parametrs is Newtonsoft.Json.Linq.JArray) && (ParametersLength > 0))
            {
                var ps = (Newtonsoft.Json.Linq.JArray)parametrs;
                if (mastParamsCount > ps.Count)
                    throw new ArgumentException($"Количество параметров должно быть не меньше {mastParamsCount}");
                int argsCount = ParametersLength < ps.Count ? ParametersLength : ps.Count;
                args = new object[argsCount];
                for (var i = 0; i < argsCount; i++)
                    args[i] = ps[i].ToObject(Parameters[i].ParameterType);//Convert.ChangeType(((Newtonsoft.Json.Linq.JValue)ps[i]).Value, Parameters[i].ParameterType);
            }

            if ((parametrs is Newtonsoft.Json.Linq.JObject) && (ParametersLength > 0))
            {
                var ps = (Newtonsoft.Json.Linq.JObject)parametrs;
                if (mastParamsCount > ps.Count)
                    throw new ArgumentException($"Количество параметров должно быть не меньше {mastParamsCount}");
                args = new object[ParametersLength];
                int i = 0;
                foreach (var p in Parameters)
                    args[i++] = ps[p.Name].ToObject(p.ParameterType); //Convert.ChangeType(((Newtonsoft.Json.Linq.JValue)ps[p.Name]).Value, p.ParameterType);
            }

            var ret = Target.GetType().InvokeMember(MethodName,
                System.Reflection.BindingFlags.InvokeMethod,
                null,
                Target,
                args,
                CultureInfo.CurrentCulture);

            if (IsAsync)
            {
                var tsk = (Task)ret;
                await tsk.ConfigureAwait(false);
                return tsk.GetType().GetProperty("Result").GetValue(tsk);
            }
            else
                return ret;
        }

    }

}
