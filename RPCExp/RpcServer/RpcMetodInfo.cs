using System.Collections.Generic;
using System.Reflection;

namespace RPCExp.RpcServer
{
    public class RpcMetodInfo
    {
        private string GetParamInfo(ParameterInfo info)
        {
            return info.ParameterType.GetDocFullName() + " " + info.Name;
        }

        public RpcMetodInfo(RpcMethod rpcMethod)
        {
            if (rpcMethod is null)
                throw new System.ArgumentNullException(nameof(rpcMethod));

            ObjName = rpcMethod.TargetName;
            MethodName = rpcMethod.MethodName;
            Description = rpcMethod.Description;
            IsAsync = rpcMethod.IsAsync;
            if(rpcMethod.ParametersLength > 0)
            {
                Parameters = new List<string>(rpcMethod.ParametersLength);
                foreach (var p in rpcMethod.Parameters)
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