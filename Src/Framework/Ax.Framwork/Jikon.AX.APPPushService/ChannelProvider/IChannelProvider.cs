/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：推送通道的接口
 * 创建标识：Zhangkj 2017/05/08
 * 
 *
************************************************************************/
using Jikon.AX.APPPushService.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jikon.AX.APPPushService.ChannelProvider
{
    /// <summary>
    /// 推送通道的接口
    /// </summary>
    public interface IChannelProvider
    {
        /// <summary>
        /// 向一批客户端发送提醒消息
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        PushResult Push(PushParams pushParams);
    }
}
