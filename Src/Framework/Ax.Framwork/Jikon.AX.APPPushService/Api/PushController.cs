/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：推送服务对外提供的API接口
 * 创建标识：Zhangkj 2017/05/08
 * 
 *
************************************************************************/
using Jikon.AX.APPPushService.ChannelProvider;
using Jikon.AX.APPPushService.Common;
using Jikon.AX.APPPushService.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Jikon.AX.APPPushService.Api
{
    /// <summary>
    /// 推送服务对外提供的API接口
    /// </summary>
    public class PushController:ApiController
    {
        /// <summary>
        /// 推送接口提请器。需要的时候由Autofac自动注入
        /// </summary>
        public IProviderGetter getter { get; set; }
        /// <summary>
        /// 默认推送通道。通过提供器获取
        /// </summary>
        protected IChannelProvider defaultPushChannel
        {
            get
            {
                //使用配置中的默认通道获取实例
                return GetProviderByType(EnvProvider.Default.DefaultChannel);
            }
        }
        /// <summary>
        /// 根据通道类型获取通道实现
        /// 注意：通道的实现必须在Autofac中以类型的字符串为名称注册过
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected IChannelProvider GetProviderByType(PushChannelType type)
        {
            if (type == PushChannelType.None)
                type = EnvProvider.Default.DefaultChannel;
            return getter.GetByConfig<IChannelProvider>(type.ToString());
        }
        /// <summary>
        /// 根据功能代码，获取其数据表信息。
        /// </summary>
        /// <param name="progId"></param>
        /// <returns></returns>
        [Route("api/push/push")]
        public HttpResponseMessage Push(PushParams pushParams)
        {
            try
            {
                if (pushParams == null || pushParams.Targets == null || pushParams.Targets.Count == 0 || pushParams.Message == null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "推送参数为空。");

                //按apptype的类型分组
                Dictionary<int, List<PushTarget>> dicAppType = (from item in pushParams.Targets
                           group item by item.AppType into gtype
                           orderby gtype.Key
                           select new
                           {
                               Key = gtype.Key,
                               Value = gtype.ToList()
                           }).ToDictionary(a => a.Key, a => a.Value);
                foreach(int type in dicAppType.Keys)
                {
                    AppPushChannelInfo channelInfo = null;
                    if (EnvProvider.Default.DicAppPushInfo.TryGetValue((AppType)type, out channelInfo))
                    {
                        if(channelInfo==null)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format("App类型:{0}对应的推送配置信息为空。", type.ToString()));
                        IChannelProvider pushChannel = GetProviderByType(channelInfo.Channel);
                        if (pushChannel == null)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "未能找到需要的推送通道。");
                        PushParams thisPush = new PushParams() {
                             Message=pushParams.Message,
                             Targets= dicAppType[type]
                        };
                        if(thisPush.Targets==null||thisPush.Targets.Count==0)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format("App类型:{0}对应的推送目标为空。", type.ToString()));
                        PushResult result = pushChannel.Push(thisPush);
                        if (result == null || result.IsCallPushError)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format("向App类型:{0}对应的推送目标推送时结果为空。", type.ToString()));
                        if (result.IsCallPushError)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format("向App类型:{0}对应的推送目标推送时出现错误，推送结果:{1}。", type.ToString(), result.ResultMessage));
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format("未找到App类型:{0}对应的推送配置信息。",type.ToString()));
                    }
                    
                }               
                return Request.CreateResponse<PushResult>(HttpStatusCode.OK, new PushResult() {
                     IsCallPushError=false,
                      ResultMessage="调用推送接口成功。"
                });
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}