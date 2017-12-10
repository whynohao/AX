/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：推送服务通道类型，用于标记不同的推送服务提供商的通道
 * 创建标识：Zhangkj 2017/05/08
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
    /// 推送服务通道类型，用于标记不同的推送服务提供商的通道
    /// </summary>
    public enum PushChannelType
    {
        /// <summary>
        /// 未设置
        /// </summary>
        None=0,
        /// <summary>
        /// 个推
        /// </summary>
        Getui,
        /// <summary>
        /// 阿里巴巴云推送
        /// </summary>
        Alibaba
    }
}