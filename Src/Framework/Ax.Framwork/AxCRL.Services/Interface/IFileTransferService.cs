using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Services
{
    [ServiceContract]
    public interface IFileTransferService
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "upLoadFile", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        UpLoadFileResult UpLoadFile(Stream stream);
        [OperationContract]
        [WebInvoke(UriTemplate = "deleteExportFile", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void DeleteExportFile(string fileName);
        [OperationContract]
        [WebInvoke(UriTemplate = "upLoadUserPicture", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        UpLoadFileResult UpLoadUserPicture(Stream stream);
        [OperationContract]
        [WebInvoke(UriTemplate = "moveUserPicture", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string MoveUserPicture(string progId, string internalId, string fileName);
        [OperationContract]
        [WebInvoke(UriTemplate = "removeUserPicture", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void RemoveUserPicture(string progId, string internalId, string fileName);
        [OperationContract]
        [WebInvoke(UriTemplate = "saveMenuSetting", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void SaveMenuSetting(string handle, string menuData);
        [OperationContract]
        [WebInvoke(UriTemplate = "loadMenuSetting", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string LoadMenuSetting(string handle, bool setting = false);
        [OperationContract]
        [WebInvoke(UriTemplate = "upLoadAttach", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        UpLoadFileResult UpLoadAttach(Stream stream);
        [OperationContract]
        [WebInvoke(UriTemplate = "moveAttach", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string MoveAttach(LibAttachData attachData);
        [OperationContract]
        [WebInvoke(UriTemplate = "removeAttach", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void RemoveAttach(string attachSrc, int orderId, string personId);
        [OperationContract]
        [WebInvoke(UriTemplate = "saveAttachStruct", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string SaveAttachStruct(LibAttachData attachData);
        [OperationContract]
        [WebInvoke(UriTemplate = "downloadAttach", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void DownloadAttach(string progId, string attachSrc, string fileName);
        [OperationContract]
        [WebInvoke(UriTemplate = "upLoadWallpaper", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        UpLoadFileResult UpLoadWallpaper(Stream stream);
        [OperationContract]
        [WebInvoke(UriTemplate = "moveWallpaper", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string MoveWallpaper(string handle, string fileName);
        [OperationContract]
        [WebInvoke(UriTemplate = "RemoveWallpaper", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void RemoveWallpaper(string handle, string fileName);

        /// <summary>
        /// 文档管理模块的文档上传
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "upLoadDoc", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        UpLoadFileResult UpLoadDoc(Stream stream);

        /// <summary>
        /// 安卓PDA上传
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "UpLoadApk", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        UpLoadFileResult UpLoadApk(Stream stream);
    }

    [DataContract]
    public class UpLoadFileResult
    {
        [DataMember]
        public bool success { get; set; }
        [DataMember]
        public string FileName { get; set; }
    }

    [DataContract]
    public class LibAttachStruct
    {
        private int _OrderId;
        private int _OrderNum;
        private string _AttachmentName;
        /// <summary>
        /// 对应文档库的文档编号
        /// </summary>
        private string _DocId = string.Empty;

        private LibAttachStatus _Status;
        [DataMember]
        public LibAttachStatus Status
        {
            get { return _Status; }
            set { _Status = value; }
        }

        [DataMember]
        public string AttachmentName
        {
            get { return _AttachmentName; }
            set { _AttachmentName = value; }
        }

        [DataMember]
        public int OrderId
        {
            get { return _OrderId; }
            set { _OrderId = value; }
        }
        [DataMember]
        public int OrderNum
        {
            get { return _OrderNum; }
            set { _OrderNum = value; }
        }
        /// <summary>
        /// 对应文档库的编号
        /// </summary>
        [DataMember]
        public string DocId
        {
            get { return _DocId; }
            set { _DocId = value; }
        }
    }

    public enum LibAttachStatus
    {
        Add = 0,
        Modif = 1,
        Delete = 2
    }

    [DataContract]
    public class LibAttachData
    {
        private string _ProgId;
        private string _TableName;
        private string _AttachSrc;
        private string _FileName;
        private int _OrderId;
        private int _OrderNum;
        private string _PersonId;
        private Dictionary<string, object> _PkList;
        private List<LibAttachStruct> _AttachList;


        private string _RealFileName;

        [DataMember]
        public string PersonId
        {
            get { return _PersonId; }
            set { _PersonId = value; }
        }
        [DataMember]
        public string TableName
        {
            get { return _TableName; }
            set { _TableName = value; }
        }
        [DataMember]
        public int OrderNum
        {
            get { return _OrderNum; }
            set { _OrderNum = value; }
        }
        [DataMember]
        public int OrderId
        {
            get { return _OrderId; }
            set { _OrderId = value; }
        }

        [DataMember]
        public List<LibAttachStruct> AttachList
        {
            get
            {
                if (_AttachList == null)
                    _AttachList = new List<LibAttachStruct>();
                return _AttachList;
            }
        }
        [DataMember]
        public Dictionary<string, object> PkList
        {
            get
            {
                if (_PkList == null)
                    _PkList = new Dictionary<string, object>();
                return _PkList;
            }
        }
        [DataMember]
        public string FileName
        {
            get { return _FileName; }
            set { _FileName = value; }
        }
        [DataMember]
        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }
        [DataMember]
        public string AttachSrc
        {
            get { return _AttachSrc; }
            set { _AttachSrc = value; }
        }

        /// <summary>
        /// 附件的真实名称
        /// </summary>
        [DataMember]
        public string RealFileName
        {
            get { return _RealFileName; }
            set { _RealFileName = value; }
        }
    }
}
