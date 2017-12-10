/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：运行环境信息
 * 创建标识：Zhangkj 2017/05/08
 * 
 *
************************************************************************/
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace Jikon.AX.APPPushService.Common
{
    /// <summary>
    /// 运行环境信息
    /// </summary>
    public class EnvProvider
    {
        #region 单实例
        private static EnvProvider _Default = null;
        private static object lockObj = new object();
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
        /// <summary>
        /// 私有构造函数，确保外部不能直接构建
        /// </summary>
        private EnvProvider()
        {
            DefaultChannel = PushChannelType.Getui;
            DicAppPushInfo = new Dictionary<AppType, AppPushChannelInfo>();
        }
        #endregion
        #region 配置属性
        private string _RuningPath;
        /// <summary>
        /// 运行路径
        /// </summary>
        public string RuningPath
        {
            get { return _RuningPath; }
            set { _RuningPath = value; }
        }
        /// <summary>
        /// 默认推送通道
        /// </summary>
        public PushChannelType DefaultChannel { get; set; }
        /// <summary>
        /// App类别与对应的推送通道信息的字典
        /// </summary>
        public Dictionary<AppType,AppPushChannelInfo> DicAppPushInfo { get; set; }
        #endregion
        /// <summary>
        /// 初始化运行参数。可以从WebConfig等中读取相关配置
        /// </summary>
        public void Init()
        {
            RuningPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            string channelStr= WebConfigurationManager.AppSettings["DefaultChannel"];
            if (string.IsNullOrEmpty(channelStr) == false)
            {
                object obj = Enum.Parse(typeof(PushChannelType), channelStr);
                if(obj!=null&&obj is PushChannelType)
                {
                    DefaultChannel = (PushChannelType)obj;
                }
            }

            //加载App引用的推送配置信息
            this.DicAppPushInfo = LoadPushInfo();
        }
        #region App推送配置相关
        /// <summary>
        /// 加载App对应的推送通道字典
        /// </summary>
        /// <returns></returns>
        public Dictionary<AppType,AppPushChannelInfo> LoadPushInfo()
        {
            const string AppPushInfoConfigFileName = "AppPushInfo.json";
            string filePath = Path.Combine(RuningPath, "Config", AppPushInfoConfigFileName);
            if(Directory.Exists(Path.Combine(RuningPath, "Config")) == false)
            {
                Directory.CreateDirectory(Path.Combine(RuningPath, "Config"));
            }
            Dictionary<AppType, AppPushChannelInfo> dicAppInfo = null;
            try
            {
                if (File.Exists(filePath) == false)
                {
                    //如果配置文件不存在，则使用初始配置生成并保存
                    dicAppInfo = GetDefaultPushInfo();
                    string jsonStr = JsonConvert.SerializeObject(dicAppInfo);                   
                    File.WriteAllText(filePath, jsonStr, Encoding.UTF8);
                    return dicAppInfo;
                }
                else
                {
                    string jsonStr = File.ReadAllText(filePath, Encoding.UTF8);
                    dicAppInfo = JsonConvert.DeserializeObject<Dictionary<AppType, AppPushChannelInfo>>(jsonStr);
                }
            }
            catch(Exception exp)
            {

            }
            if (dicAppInfo == null || dicAppInfo.Count == 0)
            {
                dicAppInfo = GetDefaultPushInfo();
                try
                {
                    string jsonStr = JsonConvert.SerializeObject(dicAppInfo);
                    using (StreamWriter sw = File.CreateText(filePath))
                    {
                        sw.Close();
                    }
                    File.WriteAllText(filePath, jsonStr, Encoding.UTF8);
                }
                catch
                {
                    //再出现异常的话仅记录日志
                    // to do Log
                }
            }
            return dicAppInfo;
            
        }
        /// <summary>
        /// 获取默认的推送配置信息
        /// </summary>
        /// <returns></returns>
        public Dictionary<AppType, AppPushChannelInfo> GetDefaultPushInfo()
        {
            Dictionary<AppType, AppPushChannelInfo> dicAppInfo = new Dictionary<AppType, AppPushChannelInfo>();
            dicAppInfo.Add(AppType.LeaderMobile, new AppPushChannelInfo()
            {
                Type = AppType.LeaderMobile,
                Channel = PushChannelType.Getui,
                AppId = "C94ZuRr0uS6Bkh1T4kz867",
                AppKey = "7XQaaAlE8iAYPNOshhHdO3",
                Secret = "JPeL3VD9S96hyIfKNN75V5"
            });
            return dicAppInfo;
        }
        #endregion
    }
}