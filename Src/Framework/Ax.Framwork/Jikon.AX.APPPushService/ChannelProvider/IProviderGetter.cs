/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：通道具体提供器的获取器接口
 * 创建标识：Zhangkj 2017/05/08
 * 
 *
************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jikon.AX.APPPushService.ChannelProvider
{
    /// <summary>
    /// 通道具体提供器的获取器接口
    /// </summary>
    public interface IProviderGetter
    {
        /// <summary>
        /// 根据默认通道的配置获取推送通道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultChannel">默认通道的配置</param>
        /// <returns></returns>
        T GetByConfig<T>(string defaultChannel);
    }
}
