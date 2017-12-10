/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：阿里巴巴云推送通道的提供器
 * 创建标识：Zhangkj 2017/05/08
 * 
 *
************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jikon.AX.APPPushService.Entity;

namespace Jikon.AX.APPPushService.ChannelProvider
{
    /// <summary>
    /// 阿里巴巴云推送通道的提供器
    /// </summary>
    public class AlibabaProvider : IChannelProvider
    {
        public PushResult Push(PushParams pushParams)
        {
            throw new NotImplementedException();
        }
    }
}