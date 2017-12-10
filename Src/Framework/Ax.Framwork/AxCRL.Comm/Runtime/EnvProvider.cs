using AxCRL.Comm.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Runtime
{
    public class EnvProvider
    {
        private static EnvProvider _Default = null;
        private static object lockObj = new object();
        private string _MainPath;
        private string _DocumentsPath;
        private string _ExtendPath;
        private string _RuningPath;
        private MailProvider _MailProvider = null;
        private string _LocalHostName = null;
        private string _VisualHostName = null;
        private int _CurrentPort;
        private int _VisualPort;
        private LibDatabaseType _DatabaseType;
        private bool _ScheduleTaskOpened = false;
        private SMSProvider _SMSProvider = null;
        private WeiXinProvider _WeiXinProvider = null;

        public WeiXinProvider WeiXinProvider
        {
            get { return _WeiXinProvider; }
            set { _WeiXinProvider = value; }
        }

        public SMSProvider SMSProvider
        {
            get { return _SMSProvider; }
            set { _SMSProvider = value; }
        }

        public bool ScheduleTaskOpened
        {
            get { return _ScheduleTaskOpened; }
            set { _ScheduleTaskOpened = value; }
        }

        public LibDatabaseType DatabaseType
        {
            get { return _DatabaseType; }
            set { _DatabaseType = value; }
        }

        public string LocalHostName
        {
            get { return _LocalHostName; }
            set { _LocalHostName = value; }
        }

        public string VisualHostName
        {
            get { return _VisualHostName; }
            set { _VisualHostName = value; }
        }

        public int CurrentPort
        {
            get { return _CurrentPort; }
            set { _CurrentPort = value; }
        }

        public int VisualPort
        {
            get { return _VisualPort; }
            set { _VisualPort = value; }
        }

        public MailProvider MailProvider
        {
            get { return _MailProvider; }
            set { _MailProvider = value; }
        }

        public string RuningPath
        {
            get { return _RuningPath; }
            set { _RuningPath = value; }
        }

        public string ExtendPath
        {
            get { return _ExtendPath; }
            set { _ExtendPath = value; }
        }
        /// <summary>
        /// MainPath
        /// </summary>
        public string MainPath
        {
            get { return _MainPath; }
            set { _MainPath = value; }
        }
        /// <summary>
        /// 文档库的路径
        /// 文档管理的文档库文件夹路径
        /// </summary>
        public string DocumentsPath
        {
            get { return _DocumentsPath; }
            set { _DocumentsPath = value; }
        }

        private bool _Default_CanEditWhenAuditing = false;
        /// <summary>
        /// 单据在审核时默认是否可修改。默认为false
        /// 如果单据配置了审核流，但因没有 审核时可修改 的相关配置（例如审核流配置主数据未升级），则使用此默认值
        /// </summary>
        public bool Default_CanEditWhenAuditing
        {
            get { return _Default_CanEditWhenAuditing; }
            set { _Default_CanEditWhenAuditing = value; }
        }

        private bool _Default_CanEditWhenAudited = false;
        /// <summary>
        /// 单据在审核通过后默认是否可修改。默认为false
        /// 如果单据配置了审核流，但因没有 审核后可修改 的相关配置（例如审核流配置主数据未升级），则使用此默认值
        /// </summary>
        public bool Default_CanEditWhenAudited
        {
            get { return _Default_CanEditWhenAudited; }
            set { _Default_CanEditWhenAudited = value; }
        }

        private bool _Default_CanDeleteWhenAudited = false;
        /// <summary>
        /// 单据在审核通过后默认是否可删除。默认为false
        /// 如果单据配置了审核流，但因没有 审核后可删除 的相关配置（例如审核流配置主数据未升级），则使用此默认值
        /// </summary>
        public bool Default_CanDeleteWhenAudited
        {
            get { return _Default_CanDeleteWhenAudited; }
            set { _Default_CanDeleteWhenAudited = value; }
        }

        private bool _IsSSOManageSite = false;
        /// <summary>
        /// 是否是SSO管理站点
        /// </summary>
        public bool IsSSOManageSite
        {
            get { return _IsSSOManageSite; }
            set { _IsSSOManageSite = value; }
        }
        private string _SSOManageSiteUrl = string.Empty;
        /// <summary>
        /// SSO管理站点的地址。
        /// 如果本地是SSO管理站点，则此值无意义
        /// </summary>
        public string SSOManageSiteUrl
        {
            get { return _SSOManageSiteUrl; }
            set { _SSOManageSiteUrl = value; }
        }
        private int _TokenValidMinutes = 3;
        /// <summary>
        /// 令牌有效分钟数。超过此时间未被使用则生成新的令牌。
        /// </summary>
        public int TokenValidMinutes
        {
            get { return _TokenValidMinutes; }
            set {
                if (value < 1)
                    value = 1;
                _TokenValidMinutes = value; }
        }
        private int _RedisDbIndex = 0;
        /// <summary>
        /// Redis存储的Db库索引
        /// </summary>
        public int RedisDbIndex
        {
            get { return _RedisDbIndex; }
            set { _RedisDbIndex = value; }
        }

        /// <summary>
        /// EnvProvider的实例
        /// </summary>
        public static EnvProvider Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (lockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new EnvProvider();
                        }
                    }
                }
                return _Default;
            }
        }
    }

    public class MailProvider
    {
        private string _Host;
        private string _MailSys;
        private string _MailPwd;

        public MailProvider(string host, string mailSys, string mailPwd)
        {
            this._Host = host;
            this._MailSys = mailSys;
            this._MailPwd = mailPwd;
        }

        public string MailPwd
        {
            get { return _MailPwd; }
            set { _MailPwd = value; }
        }

        public string MailSys
        {
            get { return _MailSys; }
            set { _MailSys = value; }
        }

        public string Host
        {
            get { return _Host; }
            set { _Host = value; }
        }
    }

    public class WeiXinProvider
    {
        private string _Host;
        private string _CorpId;
        private string _Secret;

        public string Secret
        {
            get { return _Secret; }
            set { _Secret = value; }
        }

        public string CorpId
        {
            get { return _CorpId; }
            set { _CorpId = value; }
        }

        public string Host
        {
            get { return _Host; }
            set { _Host = value; }
        }

    }

    public class SMSProvider
    {
        private string _Host;
        private string _SMSUserId;
        private string _SMSSys;
        private string _SMSPwd;
        private int _Port;
        private string _SMSSign;

        public SMSProvider(string host, int port, string smsSys, string smsPwd)
        {
            this._Host = host;
            this._Port = port;
            this._SMSSys = smsSys;
            this._SMSPwd = smsPwd;
        }
      

        public SMSProvider(string host, int port, string smsUserId, string smsSys, string smsPwd, string smsSign)
        {
            this._Host = host;
            this._Port = port;
            this._SMSUserId = smsUserId;
            this._SMSSys = smsSys;
            this._SMSPwd = smsPwd;
            this._SMSSign = smsSign;
        }

        public int Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        public string SMSUserId
        {
            get { return _SMSUserId; }
            set { _SMSUserId = value; }
        }

        public string SMSPwd
        {
            get { return _SMSPwd; }
            set { _SMSPwd = value; }
        }

        public string SMSSys
        {
            get { return _SMSSys; }
            set { _SMSSys = value; }
        }

        public string Host
        {
            get { return _Host; }
            set { _Host = value; }
        }

        public string SMSSign
        {
            get { return _SMSSign; }
            set { _SMSSign = value; }
        }
    }

    public enum LibDatabaseType
    {
        SqlServer = 0,
        Oracle = 1,
    }
}
