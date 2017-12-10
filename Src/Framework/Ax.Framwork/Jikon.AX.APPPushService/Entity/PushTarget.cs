/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：推送目标
 *           约定：一个APP只使用一个推送通道
 * 创建标识：Zhangkj 2017/05/08
 * 
 *
************************************************************************/
using Jikon.AX.APPPushService.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Jikon.AX.APPPushService.Entity
{
    /// <summary>
    /// 推送目标
    /// </summary>
    [DataContract]
    public class PushTarget
    {
        /// <summary>
        /// 本推送服务中间层与相关业务系统（如MES中的领导移动端）等共同确定的APP类型
        /// </summary>
        [DataMember]
        public int AppType { get; set; }
        /// <summary>
        /// App应用中接收推送消息的账户（当前使用手机）对应的ClientId。
        /// </summary>
        [DataMember]
        public string ClientId { get; set; }
    }
}