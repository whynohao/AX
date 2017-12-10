/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：网络请求与访问相关的工具类
 * 创建标识：Zhangkj 2017/06/29
 * 
************************************************************************/
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Utils
{
    /// <summary>
    /// 网络请求与访问相关的工具类
    /// </summary>
    public class LibNetUtils
    {
        /// <summary>
        /// 发起Http post请求并得到返回结果
        /// 本方法阻塞执行
        /// </summary>
        /// <typeparam name="T">请求执行后需要通过Json反序列化得到的对象的类型</typeparam>
        /// <param name="url">http请求绝对地址</param>
        /// <param name="postParamObj">post请求的参数。本方法会将其用UTF8格式Json序列化构造成StringContent再post到目标地址</param>
        /// <param name="errorInfo">异常信息，如果为空表示请求成功</param>
        /// <param name="timeoutMillSecs">请求的超时时间，默认为10秒。最小有效值为100</param>
        /// <returns></returns>
        public static T HttpPostCall<T>(string url, object postParamObj, out string errorInfo, int timeoutMillSecs = 10 * 1000)
        {
            errorInfo = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(url) || postParamObj == null)
                    return default(T);
                using (var httpClient = new HttpClient())
                {
                    if (timeoutMillSecs < 100)
                        timeoutMillSecs = 100;
                    httpClient.Timeout = new TimeSpan(0, 0, 0, 0, timeoutMillSecs);
                    //使用FormUrlEncodedContent做HttpContent                 
                    var content = new StringContent(JsonConvert.SerializeObject(postParamObj), Encoding.UTF8, "text/json");
                    var response = httpClient.PostAsync(url, content);
                    //确保HTTP成功状态值，如果不是正确的返回状态则抛出异常
                    response.Result.EnsureSuccessStatusCode();
                    //await异步读取最后的JSON
                    var retStr = response.Result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(retStr.Result);
                }
            }
            catch (Exception exp)
            {
                errorInfo = exp.Message;
                if (exp.InnerException != null)
                    errorInfo += " " + exp.InnerException.Message;
                if (exp.InnerException != null && exp.InnerException.InnerException != null)
                    errorInfo += " " + exp.InnerException.InnerException.Message;
                return default(T);
            }

        }
    }
}
