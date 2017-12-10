/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：移动端推送服务调用
 * 创建标识：Zhangkj 2017/06/06
 * 
 *
************************************************************************/
using AxCRL.Comm.Configs;
using AxSRL.SMS.Entity;
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
}
