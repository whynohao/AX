using AxCRL.Comm.Configs;
using AxCRL.Comm.Utils;
using AxSRL.SMS.Entity;
using cn.jpush.api;
using cn.jpush.api.push.mode;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AxSRL.SMS
{
    /// <summary>
    /// 移动端推送服务调用
    /// </summary>
    public class LibAppPushService
    {
        /// <summary>
        /// 调用推送服务进行推送
        /// </summary>
        /// <param name="pushParams"></param>
        /// <returns></returns>
        public static PushResult Push(PushParams pushParams)
        {
            if (MicroServicesConfig.Instance.AppPush.Enabled == false)
                return null;
            if (pushParams == null || pushParams.Targets == null || pushParams.Targets.Count == 0 ||
                pushParams.Message == null || string.IsNullOrEmpty(pushParams.Message.Message))
                return null;
            string url = string.Format("{0}/api/push/push", MicroServicesConfig.Instance.AppPush.BaseUrl);
            //创建HttpClient
            using (var http = new HttpClient())
            {
                //使用FormUrlEncodedContent做HttpContent  
                var content = new StringContent(JsonConvert.SerializeObject(pushParams), Encoding.UTF8, "text/json");
                var response = http.PostAsync(url, content);
                //确保HTTP成功状态值，如果不是正确的返回状态则抛出异常
                //response.Result.EnsureSuccessStatusCode();
                //await异步读取最后的JSON
                var retStr = response.Result.Content.ReadAsStringAsync();
                if (response.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return null;
                }
                else
                {
                    return JsonConvert.DeserializeObject<PushResult>(retStr.Result);
                }
            }
        }
    }

    public class LibMessageApp
    {
        //发送消息
        public static bool SendMessage(string message, string uid = "")
        {
            string app_key = System.Configuration.ConfigurationManager.AppSettings["APP_KEY"];
            string master_secret = System.Configuration.ConfigurationManager.AppSettings["MASTER_SECRET"];
            JPushClient client = new JPushClient(app_key, master_secret);
            DateTime dt1 = DateTime.Now;

            PushPayload payload = PushObject_All_Message(message, uid);
            try
            {
                var result = client.SendPush(payload);

                int a = 10;
            }
            catch (Exception ex)
            {
                LibLog.WriteLog(ex);
            }
            return true;
        }

        public static PushPayload PushObject_All_Message(string message, string uid)
        {
            PushPayload pushPayload = new PushPayload();
            if (string.IsNullOrEmpty(uid))
            {
                pushPayload = new PushPayload()
                {
                    platform = Platform.all(),
                    audience = Audience.all(),
                    notification = new Notification().setAlert(message)
                };
            }
            else
            {
                pushPayload = new PushPayload()
                {
                    platform = Platform.all(),
                    //audience = Audience.s_registrationId(uid),
                    audience = Audience.s_alias(uid),
                    notification = new Notification().setAlert(message)
                };
            }

            return pushPayload;
        }
    }
}
