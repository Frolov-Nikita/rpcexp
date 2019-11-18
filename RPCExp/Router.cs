using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RPCExp.JsonRpc;
using System.ComponentModel.DataAnnotations;

namespace RPCExp
{
    public class Router
    {
        static readonly Encoding encoding = Encoding.UTF8;
        readonly List<RpcMethod> rpcMethods = new List<RpcMethod>();

        public IEnumerable<RpcMetodInfo> GetMethods()
        {
            List<RpcMetodInfo> infos = new List<RpcMetodInfo>();
            foreach (var r in rpcMethods)
                infos.Add(new RpcMetodInfo(r));
            return infos;
        }

        public Router()
        {
            var rm = new RpcMethod
            {
                TargetName = "rpc",
                Target = this,
                MethodName = "GetMethods",
                Parameters = null,
                Description = "Описание всех доступных методов"
            };
            rpcMethods.Add(rm);
        }

        private static string GetDesc(MethodInfo methodInfo)
        {
            return methodInfo.GetDocumentation()?.InnerXml;
        }

        public void RegisterMethods(object target, string targetName = default)
        {
            if (target == default) 
                return;
            var methods = target.GetType().GetMethods();
            Type asyncAttrType = typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute);
            foreach (var m in methods)
            {
                if (!m.IsPublic || m.IsSpecialName || (m.DeclaringType == typeof(Object))) continue;
                var rm = new RpcMethod
                {
                    Target = target,
                    TargetName = targetName,
                    MethodName = m.Name,
                    Description = GetDesc(m),
                    IsAsync = m.GetCustomAttributes(asyncAttrType, false).Length > 0,
                    Parameters = m.GetParameters()
                };
                rpcMethods.Add(rm);
            }
        }

        public async Task<byte[]> Handle(byte[] buffer, int index, int bytesCount)
        {
            string id = "";
            try
            {
                var req = Request.FromJson(encoding.GetString(buffer, index, bytesCount));
                id = req.Id;
                var resp = await Handle(req).ConfigureAwait(false);
                return encoding.GetBytes(resp.ToJson());
            }
            catch
            {
                return encoding.GetBytes(Response.GetErrorParse(id).ToJson());
            }
        }//Handle()

        private async Task<Response> Handle(Request request)
        {
            var tmp = request.MethodName.Split('.', 2);
            string objName = default, methodName = "";
            if(tmp.Length > 1)
            {
                objName = tmp[0];
                methodName = tmp[1];
            }
            else
                methodName = tmp[0];

            var parameters = (Newtonsoft.Json.Linq.JContainer)request.Parameters;

            var methods = rpcMethods.FindAll(
                m => m.MethodName == methodName &&
                m.TargetName == objName);

            var method = methods.Find(m => (m.Parameters?.Length ?? 0) == parameters.Count);
            
            if (method == default(RpcMethod))
                return Response.GetErrorMethodNotFound(request.Id, request.MethodName);

            var res = new Response() { Id = request.Id };
            try
            {
                res.Result = await method.InvokeAsync(request.Parameters).ConfigureAwait(false);
                return res;
            }
            catch (ArgumentException)
            {
                return Response.GetErrorInvalidParams(request.Id, request.MethodName, request.Parameters);
            }
            catch (Exception ex)
            {
                return Response.GetErrorInternalError(request.Id, request.MethodName, ex.Message);
            }
            
        }

    }
}
