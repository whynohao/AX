using AxCRL.Bcf.ScheduleTask;
using AxCRL.Comm.Runtime;
using AxCRL.Core;
using AxCRL.Data;
using AxCRL.Services;
//using MES_Dm.FullTextRetrieval.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Web;
using System.Web.Configuration;

namespace Ax.Server
{
    /// <summary>
    /// Ax服务
    /// </summary>
    public class AxServiceBus
    {
        private WebServiceHost _BillServiceHost = null;
        private WebServiceHost _SystemManagerHost = null;
        private WebServiceHost _SystemServiceHost = null;
        private WebServiceHost _WsServiceHost = null;
        private WebServiceHost _FileServiceHost = null;
        private WebServiceHost _IndexServiceHost = null;

        public WebServiceHost FileServiceHost
        {
            get { return _FileServiceHost; }
            set { _FileServiceHost = value; }
        }

        public WebServiceHost IndexServiceHost
        {
            get { return _IndexServiceHost; }
            set { _IndexServiceHost = value; }
        }

        public WebServiceHost WsServiceHost
        {
            get { return _WsServiceHost; }
            set { _WsServiceHost = value; }
        }

        public WebServiceHost SystemServiceHost
        {
            get { return _SystemServiceHost; }
            set { _SystemServiceHost = value; }
        }

        public WebServiceHost SystemManagerHost
        {
            get { return _SystemManagerHost; }
            set { _SystemManagerHost = value; }
        }
        public WebServiceHost BillServiceHost
        {
            get { return _BillServiceHost; }
            set { _BillServiceHost = value; }
        }



        public void Start()
        {
            EnvProvider.Default.Default_CanEditWhenAuditing = true;
            EnvProvider.Default.RuningPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            EnvProvider.Default.MainPath = (string)WebConfigurationManager.AppSettings["mainPath"];
            //Zhangkj 20161212 增加文档管理的文档库文件夹路径
            object obj = WebConfigurationManager.AppSettings["documentsPath"];
            string documentLibPath = Path.Combine(EnvProvider.Default.MainPath, "Documents");
            if (obj != null)
                documentLibPath = (string)obj;
            EnvProvider.Default.DocumentsPath = documentLibPath;
            //初始化全文索引服务
            //AxIndexer.Init();

            EnvProvider.Default.ExtendPath = (string)WebConfigurationManager.AppSettings["extendPath"];
            string localhostNameStr = (string)WebConfigurationManager.AppSettings["localhostName"];
            string visualhostNameStr = (string)WebConfigurationManager.AppSettings["visualhostName"];
            string[] localhostName = localhostNameStr.Split(':');
            EnvProvider.Default.LocalHostName = localhostName[0];
            EnvProvider.Default.CurrentPort = int.Parse(localhostName[1]);
            string[] visualhostName = visualhostNameStr.Split(':');
            EnvProvider.Default.VisualHostName = visualhostName[0];
            EnvProvider.Default.VisualPort = int.Parse(visualhostName[1]);
            string mailConfigStr = (string)WebConfigurationManager.AppSettings["mailConfig"];
            string[] mailConfig = mailConfigStr.Split('#');
            EnvProvider.Default.MailProvider = new MailProvider(mailConfig[0], mailConfig[1], mailConfig[2]);
            string smsConfigStr = (string)WebConfigurationManager.AppSettings["smsConfig"];
            if (!string.IsNullOrEmpty(smsConfigStr))
            {
                string[] smsConfig = smsConfigStr.Split('#');
                if (smsConfig != null)
                {
                    if (smsConfig.Length == 4)
                        EnvProvider.Default.SMSProvider = new SMSProvider(smsConfig[0], int.Parse(smsConfig[1]), smsConfig[2], smsConfig[3]);
                    else if (smsConfig.Length == 6)
                        EnvProvider.Default.SMSProvider = new SMSProvider(smsConfig[0], int.Parse(smsConfig[1]), smsConfig[2], smsConfig[3], smsConfig[4], smsConfig[5]);
                }
            }
            //企业微信账号配置  Zhangkj 20170612
            string weixinConfigStr = (string)WebConfigurationManager.AppSettings["weixinConfig"];
            if (!string.IsNullOrEmpty(weixinConfigStr))
            {
                string[] weixinConfig = weixinConfigStr.Split('#');
                if (weixinConfig != null && weixinConfig.Length > 1)
                {
                    EnvProvider.Default.WeiXinProvider = new WeiXinProvider() { CorpId = weixinConfig[0], Secret = weixinConfig[1] };
                }
            }
            LibDataAccess dataAccess = new LibDataAccess();
            EnvProvider.Default.DatabaseType = dataAccess.DatabaseType;
            LoadProgId();
            //打开服务
            OpenServices();
        }


        private void LoadProgId()
        {
            ProgIdHost.Instance.Run();
        }

        private void OpenServices()
        {
            SystemManagerHost = new WebServiceHost(typeof(SystemManager));
            SystemManagerHost.Open();
            BillServiceHost = new WebServiceHost(typeof(BillService));
            BillServiceHost.Open();
            FileServiceHost = new WebServiceHost(typeof(FileTransferService));
            FileServiceHost.Open();
            SystemServiceHost = new WebServiceHost(typeof(SystemService));
            SystemServiceHost.Open();
            //IndexServiceHost = new WebServiceHost(typeof(AxIndexer));
            //IndexServiceHost.Open();
            _WsServiceHost = new WebServiceHost(typeof(WsService));
            _WsServiceHost.Open();
        }
    }
}