using AxCRL.Bcf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Services
{
    [ServiceContract]
    public interface IWsService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "execWsMethod", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string ExecuteWsMethod(ExecuteWsMethodParam param);
    }

    [Serializable]
    [DataContract]
    public class ExecuteWsMethodResult
    {
        private object _Result;
        [DataMember]
        public object Result
        {
            get { return _Result; }
            set { _Result = value; }
        }

        private LibMessageList _Messages;
        [DataMember]
        public LibMessageList Messages
        {
            get
            {
                if (_Messages == null)
                    _Messages = new LibMessageList();
                return _Messages;
            }
            set { _Messages = value; }
        }
    }

    [DataContract]
    public class ExecuteWsMethodParam
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
        private string[] _MethodParam;

        [DataMember]
        public string[] MethodParam
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
