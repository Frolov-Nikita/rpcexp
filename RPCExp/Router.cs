using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RPCExp.JsonRpc;

namespace RPCExp
{

 
    public class Router
    {
        Encoding enc = Encoding.UTF8;

        List<RpcMethod> rpcMethods = new List<RpcMethod>();

        public void RegisterMethods(object obj, string objName = default)
        {
            var methods = obj.GetType().GetMethods();
            Type asyncAttrType = typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute);
            foreach (var m in methods)
            {
                if (!m.IsPublic) continue;
                var rm = new RpcMethod();
                rm.Obj = obj;
                rm.ObjName = objName;
                rm.MethodName = m.Name;
                rm.IsAsync = m.GetCustomAttributes(asyncAttrType, false).Length > 0;
                rm.Parameters = m.GetParameters();
                rpcMethods.Add(rm);
            }
        }

        public async Task<byte[]> Handle(byte[] buffer, int index, int bytesCount)
        {
            try
            {
                var req = Request.FromJson(enc.GetString(buffer, index, bytesCount));
                var resp = await Handle(req);
                return enc.GetBytes(resp.ToJson());
            }
            catch
            {
                return enc.GetBytes(Response.GetErrorParse().ToJson());
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
                m.ObjName == objName);

            var method = methods.Find(m => m.Parameters.Length == parameters.Count);

            if(method == default(RpcMethod))
                method = methods.Find(m => m.Parameters.Length >= parameters.Count);

            if (method == default(RpcMethod))
                return Response.GetErrorMethodNotFound(request.Id, request.MethodName);

            var res = new Response() { Id = request.Id };
            try
            {
                res.Result = await method.Invoke(request.Parameters);
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
