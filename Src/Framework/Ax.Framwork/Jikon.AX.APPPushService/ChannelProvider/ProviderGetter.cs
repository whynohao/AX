/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：通道具体提供器的获取器
 * 创建标识：Zhangkj 2017/05/08
 * 
 *
************************************************************************/
using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jikon.AX.APPPushService.ChannelProvider
{
    /// <summary>
    /// 通道具体提供器的获取器
    /// </summary>
    public class ProviderGetter : IProviderGetter
    {
        public T GetByConfig<T>(string defaultChannel)
        {
            return AutofacDependencyResolver.Current.RequestLifetimeScope.ResolveNamed<T>(defaultChannel);
        }
    }
}