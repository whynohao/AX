/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：APP应用使用的推送通道信息
 *           约定：一个APP只使用一个推送通道
 * 创建标识：Zhangkj 2017/05/09
 * 
 *
************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jikon.AX.APPPushService.Common
{
    /// <summary>
    /// App应用类别
    /// </summary>
    public enum AppType
    {
        /// <summary>
        /// 领导手持移动端
        /// </summary>
        LeaderMobile=0,
        /// <summary>
        /// 一线操作工App等
        /// </summary>
        PDA
    }
    /// <summary>
    /// APP应用使用的推送通道信息
    /// </summary>
    public class AppPushChannelInfo
    {
        /// <summary>
        /// APP应用类型
        /// </summary>
        public AppType Type { get; set; }
        /// <summary>
        /// 推送通道
        /// </summary>
        public PushChannelType Channel { get; set; }
        /// <summary>
        /// 推送通道中注册的应用Id
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 应用对应的推送Key
        /// </summary>
        public string AppKey { get; set; }
        /// <summary>
        /// 推送密钥
        /// </summary>
        public string Secret { get; set; }
    }
}