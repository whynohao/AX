/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：个推推送通道的提供器
 * 创建标识：Zhangkj 2017/05/08
 * 
 *
************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jikon.AX.APPPushService.Entity;
using com.igetui.api.openservice;
using com.igetui.api.openservice.igetui.template;
using com.igetui.api.openservice.igetui;
using Jikon.AX.APPPushService.Common;
using com.igetui.api.openservice.payload;

namespace Jikon.AX.APPPushService.ChannelProvider
{
    /// <summary>
    /// 个推推送通道的提供器
    /// </summary>
    public class GetuiProvider : IChannelProvider
    {
        /// <summary>
        /// 个推推送的服务器地址
        /// </summary>
        private static string HOST = "http://sdk.open.api.igexin.com/apiex.htm";        

        public PushResult Push(PushParams pushParams)
        {
            if (pushParams == null || pushParams.Message == null || pushParams.Targets == null || pushParams.Targets.Count == 0)
                throw new Exception("推送参数为空。");
            if (pushParams.Targets.First() == null)
                throw new Exception("推送目标为空。");
            //入口参数中的Target都是同一个app类型的推送目标了。           
            AppPushChannelInfo channelInfo = null;
            if (EnvProvider.Default.DicAppPushInfo.TryGetValue((AppType)pushParams.Targets.First().AppType, out channelInfo) == false || channelInfo == null)
                throw new Exception(string.Format("App类型:{0}对应的推送通道信息为空。", pushParams.Targets.First().AppType));

            try
            {
                IGtPush push = new IGtPush(HOST, channelInfo.AppKey, channelInfo.Secret);
                ListMessage message = new ListMessage();
                TransmissionTemplate template = TransmissionTemplateDemo(channelInfo,pushParams.Message);
                message.IsOffline = true;
                message.OfflineExpireTime = 1000 * 3600 * 12;
                message.Data = template;

                List<Target> targetList = new List<Target>();
                for (int i = 0; i < pushParams.Targets.Count; i++)
                {
                    Target target = new Target();
                    target.appId = channelInfo.AppId;
                    target.clientId = pushParams.Targets[i].ClientId;
                    targetList.Add(target);
                }
                //com.igetui.api.openservice.igetui.Target target1 = new com.igetui.api.openservice.igetui.Target();
                //target1.appId = APPID;
                //target1.clientId = clientId;

                // 如需要，可以设置多个接收者
                //com.igetui.api.openservice.igetui.Target target2 = new com.igetui.api.openservice.igetui.Target();
                //target2.appId = APPID;
                //target2.clientId = "f70befc00249c7337c15ba253e7cc391";

                //com.igetui.api.openservice.igetui.Target target3 = new com.igetui.api.openservice.igetui.Target();
                //target3.appId = APPID;
                //target3.clientId = "8a4d45c5a7319237db8493d774befb4c";

                //targetList.Add(target1);
                //targetList.Add(target2);
                //targetList.Add(target3);

                String contentId = push.getContentId(message);
                String pushResult = push.pushMessageToList(contentId, targetList);
                System.Console.WriteLine("-----------------------------------------------");
                System.Console.WriteLine("服务端返回结果:" + pushResult);
                return new PushResult()
                {
                     IsCallPushError=false,
                     ResultMessage= pushResult
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        protected TransmissionTemplate TransmissionTemplateDemo(AppPushChannelInfo channelInfo,NoticeMsg message)
        {
            if (channelInfo == null)
                throw new Exception("推送通道信息为空。");
            TransmissionTemplate template = new TransmissionTemplate();
            try
            {
                template.AppId = channelInfo.AppId;
                template.AppKey = channelInfo.AppKey;
                //应用启动类型，1：强制应用启动 2：等待应用启动
                template.TransmissionType = "1";
                //透传内容  
                template.TransmissionContent = "透传内容";
                //设置通知定时展示时间，结束时间与开始时间相差需大于6分钟，消息推送后，客户端将在指定时间差内展示消息（误差6分钟）
                //String begin = "2017-03-13 00:36:10";
                //String end = "2017-03-13 23:46:20";
                //template.setDuration(begin, end);

                //APN高级推送 开始
                APNPayload apnpayload = new APNPayload();
                DictionaryAlertMsg alertMsg = new DictionaryAlertMsg();
                alertMsg.Body = message.Message;

                //alertMsg.Body = "您有新的异常消息，请尽快处理。";
                //(用于多语言支持）指定执行按钮所使用的Localizable.strings
                //alertMsg.ActionLocKey = "ActionLocKey";
                //(用于多语言支持）指定Localizable.strings文件中相应的key
                //alertMsg.LocKey = "LocKey";
                ////如果loc-key中使用的占位符，则在loc-args中指定各参数                     
                //alertMsg.addLocArg("LocArg");
                //指定启动界面图片名                 
                //alertMsg.LaunchImage = "LaunchImage";
                //iOS8.2支持字段
                //通知标题
                alertMsg.Title = message.Title;
                //(用于多语言支持）对于标题指定执行按钮所使用的Localizable.strings                       
                //alertMsg.TitleLocKey = "TitleLocKey";
                //对于标题, 如果loc-key中使用的占位符，则在loc-args中指定各参数
                //alertMsg.addTitleLocArg("TitleLocArg");

                apnpayload.AlertMsg = alertMsg;
                //应用icon上显示的数字
                //apnpayload.Badge = 1;
                apnpayload.ContentAvailable = 1;
                //apnpayload.Category = "";
                //通知铃声文件名
                //apnpayload.Sound = "test1.wav";
                //增加自定义的数据                 
                apnpayload.addCustomMsg("pushMsgType", "message");
                template.setAPNInfo(apnpayload);
                //APN高级推送 结束
            }
            catch (Exception)
            {
                throw;
            }
            return template;
        }
    }
}