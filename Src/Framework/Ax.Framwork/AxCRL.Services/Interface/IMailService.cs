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
    public interface IMailService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "sendMail", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void SendMail(LibMailParam param);
    }

    [DataContract]
    public class LibMailParam
    {
        private LibMailKind _MailKind = LibMailKind.Info;
        private IList<string> _To;
        private IList<string> _CC;
        private string _Subject;
        private string _Content;
        private string _UserId;
        private string _Handle;
        private string _ProgId;
        private string _ProgName;
        private IList<string> _AttachmentList;
        private IDictionary<string, string> _ExpandData;

        [DataMember]
        public string ProgName
        {
            get { return _ProgName; }
            set { _ProgName = value; }
        }

        [DataMember]
        public IList<string> AttachmentList
        {
            get
            {
                if (_AttachmentList == null)
                    _AttachmentList = new List<string>();
                return _AttachmentList;
            }
        }

        [DataMember]
        public IDictionary<string, string> ExpandData
        {
            get
            {
                if (_ExpandData == null)
                    _ExpandData = new Dictionary<string, string>();
                return _ExpandData;
            }
        }

        [DataMember]
        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }

        [DataMember]
        public string Handle
        {
            get { return _Handle; }
            set { _Handle = value; }
        }
        [DataMember]
        public string Content
        {
            get { return _Content; }
            set { _Content = value; }
        }
        [DataMember]
        public string Subject
        {
            get { return _Subject; }
            set { _Subject = value; }
        }
        [DataMember]
        public LibMailKind MailKind
        {
            get { return _MailKind; }
            set { _MailKind = value; }
        }
        [DataMember]
        public IList<string> To
        {
            get
            {
                if (_To == null)
                    _To = new List<string>();
                return _To;
            }
        }
        [DataMember]
        public IList<string> CC
        {
            get
            {
                if (_CC == null)
                    _CC = new List<string>();
                return _CC;
            }
        }
        [DataMember]
        public string UserId
        {
            get { return _UserId; }
            set { _UserId = value; }
        }
    }

    public enum LibMailKind
    {
        Info = 0,
        Approve = 1,
        Warning = 2,
        Problem = 3
    }

}
