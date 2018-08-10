using AxCRL.Comm.Entity;
using AxCRL.Services.Entity;
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
    public interface ISystemService
    {
        [OperationContract]
        [WebGet(UriTemplate = "getJsPath", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string GetJsPath();
        [OperationContract]
        [WebInvoke(UriTemplate = "login", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        LoginInfo Login(string userId, string password, bool quitOther);
        [OperationContract]
        [WebInvoke(UriTemplate = "SSOLogin", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        LoginInfo SSOLogin(SSOInfo ssoInfo);
        [OperationContract]
        [WebInvoke(UriTemplate = "AppLogin", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        LoginInfo AppLogin(string userId, string password, string clientId, int clientType, bool quitOther);
        [OperationContract]
        [WebInvoke(UriTemplate = "checkLogin", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void CheckLogin(string handle);
        [OperationContract]
        [WebInvoke(UriTemplate = "loginOut", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void LoginOut(string handle);
        [OperationContract]
        [WebInvoke(UriTemplate = "setPwd", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        SetPwdResult SetPassword(string handle, string oldPwd, string newPwd);
        [OperationContract]
        [WebGet(UriTemplate = "getVisualHostName", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string GetVisualHostName();
        [OperationContract]
        [WebInvoke(UriTemplate = "setWallpaper", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void SetWallpaper(string handle, string wallpaper, bool stretch);
        [OperationContract]
        [WebInvoke(UriTemplate = "getWallpapers", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        List<string> GetWallpapers(string handle);
        [OperationContract]
        [WebGet(UriTemplate = "getDept", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        List<DeptInfo> GetDept();
        [OperationContract]
        [WebInvoke(UriTemplate = "register", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string Register(RegisterInfo info);
        [OperationContract]
        [WebInvoke(UriTemplate = "recoverPassword", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string RecoverPassword(string userId);
        [OperationContract]
        [WebInvoke(UriTemplate = "checkSSOLoginState", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string CheckSSOLoginState(SSOInfo ssoInfo);
        [OperationContract]
        [WebInvoke(UriTemplate = "getToken", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string GetToken(string userHandle);
        [OperationContract]
        [WebInvoke(UriTemplate = "getTokenByUserId", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string GetTokenByUserId(string userId, string pwd);

        [OperationContract]
        [WebInvoke(UriTemplate = "getLinkSites", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        List<LinkSiteInfo> GetLinkSites(string userHandle);

    }
    [DataContract]
    public class LoginInfo
    {
        [DataMember]
        public string Id = string.Empty;

        [DataMember]
        public string Name = string.Empty;

        [DataMember]
        public string UserImage = string.Empty;

        private string _cornet;
        [DataMember]
        public string Cornet
        {
            get { return _cornet; }
            set { _cornet = value; }
        }
        private string _userEMail;
        [DataMember]
        public string UserEMail
        {
            get { return _userEMail; }
            set { _userEMail = value; }
        }
        private string _userPhone;
        [DataMember]
        public string UserPhone
        {
            get { return _userPhone; }
            set { _userPhone = value; }
        }
        private string _headportrait;
        [DataMember]
        public string Headportrait
        {
            get { return _headportrait; }
            set { _headportrait = value; }
        }
        private bool _IsOverUser = false;
        [DataMember]
        public bool IsOverUser
        {
            get { return _IsOverUser; }
            set { _IsOverUser = value; }
        }
        private string _Handle;
        [DataMember]
        public string Handle
        {
            get { return _Handle; }
            set { _Handle = value; }
        }

        private string _PersonId;
        [DataMember]
        public string PersonId
        {
            get { return _PersonId; }
            set { _PersonId = value; }
        }

        private string _PersonName;
        [DataMember]
        public string PersonName
        {
            get { return _PersonName; }
            set { _PersonName = value; }
        }

        private bool _IsUsed = false;
        [DataMember]
        public bool IsUsed
        {
            get { return _IsUsed; }
            set { _IsUsed = value; }
        }

        private string _Wallpaper;
        [DataMember]
        public string Wallpaper
        {
            get { return _Wallpaper; }
            set { _Wallpaper = value; }
        }

        private bool _Stretch = true;
        [DataMember]
        public bool Stretch
        {
            get { return _Stretch; }
            set { _Stretch = value; }
        }
    }
    [DataContract]
    public class SetPwdResult
    {
        private string _Msg;
        private bool _Success;
        [DataMember]
        public string Msg
        {
            get { return _Msg; }
            set { _Msg = value; }
        }
        [DataMember]
        public bool Success
        {
            get { return _Success; }
            set { _Success = value; }
        }
    }
    [DataContract]
    public class DeptInfo
    {
        private string _DeptId;
        private string _DeptName;
        [DataMember]
        public string DeptName
        {
            get { return _DeptName; }
            set { _DeptName = value; }
        }
        [DataMember]
        public string DeptId
        {
            get { return _DeptId; }
            set { _DeptId = value; }
        }
    }
    [DataContract]
    public class RegisterInfo
    {
        [DataMember]
        public string VerificationCode { get; set; }

        [DataMember]
        public string inputId { get; set; }
        [DataMember]
        public string inputPassword1 { get; set; }
        [DataMember]
        public string inputName { get; set; }
        [DataMember]
        public string inputDept { get; set; }
        [DataMember]
        public string inputEmail { get; set; }
        [DataMember]
        public string inputPhone { get; set; }
        [DataMember]
        public int gender { get; set; }
        [DataMember]
        public string cornet { get; set; }


    }
}
