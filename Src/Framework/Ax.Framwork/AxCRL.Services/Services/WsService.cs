using AxCRL.Bcf;
using AxCRL.Bcf.Sfl;
using AxCRL.Comm.Runtime;
using AxCRL.Core;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Services
{
    public class WsService : IWsService
    {
        public string ExecuteWsMethod(ExecuteWsMethodParam param)
        {
            if (string.IsNullOrEmpty(param.ProgId))
            {
                throw new ArgumentNullException("ProgId", "ProgId is empty.");
            }
            ExecuteWsMethodResult result = new ExecuteWsMethodResult();
            if (ProgIdHost.Instance.ProgIdRef.ContainsKey(param.ProgId))
            {
                BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[param.ProgId];
                string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                Assembly assembly = Assembly.LoadFrom(path);
                Type t = assembly.GetType(info.ClassName);
                LibWsBcf destObj = (LibWsBcf)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                object[] destParam = RestoreParamFormat(t, param.MethodName, param.MethodParam);
                result.Result = t.InvokeMember(param.MethodName, BindingFlags.InvokeMethod, null, destObj, destParam);
                result.Messages = destObj.ManagerMessage.MessageList;
            }
            return JsonConvert.SerializeObject(result);
        }

        private object[] RestoreParamFormat(Type destType, string method, string[] param)
        {
            object[] destParam = null;
            ParameterInfo[] paramInfo = destType.GetMethod(method).GetParameters();
            int length = paramInfo.Length;
            if (length > 0)
            {
                destParam = new object[length];
                for (int i = 0; i < param.Length; i++)
                {
                    destParam[i] = JsonConvert.DeserializeObject(param[i], paramInfo[i].ParameterType);
                }
            }
            return destParam;
        }

        public ExecuteWsMethodResult ExecuteWsMethod_Ws(ExecuteWsMethodParam_Ws param)
        {
            if (string.IsNullOrEmpty(param.ProgId))
            {
                throw new ArgumentNullException("ProgId", "ProgId is empty.");
            }
            ExecuteWsMethodResult result = new ExecuteWsMethodResult();
            if (ProgIdHost.Instance.ProgIdRef.ContainsKey(param.ProgId))
            {
                BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[param.ProgId];
                string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                Assembly assembly = Assembly.LoadFrom(path);
                Type t = assembly.GetType(info.ClassName);
                LibWsBcf destObj = (LibWsBcf)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                result.Result = t.InvokeMember(param.MethodName, BindingFlags.InvokeMethod, null, destObj, param.MethodParam);
                result.Messages = destObj.ManagerMessage.MessageList;
            }
            return result;
        }


        public DataSet GetRpt_Ws(ExecuteWsMethodParam_Ws param)
        {
            if (string.IsNullOrEmpty(param.ProgId))
            {
                throw new ArgumentNullException("ProgId", "ProgId is empty.");
            }
            DataSet ds = null;
            if (ProgIdHost.Instance.ProgIdRef.ContainsKey(param.ProgId))
            {
                BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[param.ProgId];
                string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                Assembly assembly = Assembly.LoadFrom(path);
                Type t = assembly.GetType(info.ClassName);
                LibBcfBase destObj = t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null) as LibBcfBase;
                if (destObj != null)
                {
                    destObj.Handle = LibHandleCache.Default.GetSystemHandle();
                    ds = t.InvokeMember(param.MethodName, BindingFlags.InvokeMethod, null, destObj, param.MethodParam) as DataSet;
                }
            }
            return ds;
        }

    }



    [Serializable]
    public class ExecuteWsMethodParam_Ws
    {
        private string _ProgId;
        private string _MethodName;
        private string _Handle;
        [DataMember]
        public string Handle
        {
            get { return _Handle; }
            set { _Handle = value; }
        }
        private object[] _MethodParam;

        [DataMember]
        public object[] MethodParam
        {
            get { return _MethodParam; }
            set { _MethodParam = value; }
        }

        [DataMember]
        public string MethodName
        {
            get { return _MethodName; }
            set { _MethodName = value; }
        }

        [DataMember]
        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }
    }
}
