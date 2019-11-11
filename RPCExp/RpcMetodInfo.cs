using System.Collections.Generic;
using System.Reflection;

namespace RPCExp
{
    public class RpcMetodInfo
    {
        private string GetParamInfo(ParameterInfo info)
        {
            return info.ParameterType.GetDocFullName() + " " + info.Name;
        }

        public RpcMetodInfo(RpcMethod r)
        {
            ObjName = r.TargetName;
            MethodName = r.MethodName;
            Description = r.Description;
            IsAsync = r.IsAsync;
            if(r.ParametersLength > 0)
            {
                Parameters = new List<string>(r.ParametersLength);
                foreach (var p in r.Parameters)
                    Parameters.Add(GetParamInfo(p));
            }
        }

        public string ObjName { get; private set; }
        public string MethodName { get; private set; }
        public string Description { get; private set; }
        public bool IsAsync { get; private set; }
        public List<string> Parameters { get; private set; }
    }
}