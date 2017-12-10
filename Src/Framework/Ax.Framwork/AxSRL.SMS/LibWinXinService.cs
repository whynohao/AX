using AxCRL.Comm.Runtime;
using AxCRL.Comm.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace AxSRL.SMS
{
    public class LibWinXinService : ILibSMSService
    {
        public void SendMsg(SendSMSParam param)
        {
            if (param.PhoneList == null || param.PhoneList.Count == 0)
                throw new ArgumentNullException("接收号码为空值");
            try
            {
                WeiXinCorp wxmsg = new WeiXinCorp();
                StringBuilder builder = new StringBuilder();
                foreach (string phone in param.PhoneList)
                {
                    builder.AppendFormat("{0}|", phone);
                }
                builder.Remove(builder.Length - 1, 1);
                wxmsg.ToUserID = builder.ToString();

                string phoneStr = builder.ToString();
                string message = param.Message.Trim();

                string ret = wxmsg.SendMessage(param.Message);
                if (ret.Length < 6)
                {
                    string path = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output", "Error", "SMS", string.Format("{0}.txt", DateTime.Now.Ticks));
                    using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                        {
                            sw.Write(string.Format("发送失败，错误码：{0}", ret));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string path = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output", "Error", "SMS", string.Format("{0}.txt", DateTime.Now.Ticks));
                using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                    {
                        sw.Write(ex);
                    }
                }
            }
        }
    }

    class WeiXinCorp
    {
        /// <summary>
        /// 微信企业号
        /// </summary>
        public WeiXinCorp()
        {

        }

        /// <summary>
        /// 获取企业号的accessToken
        /// </summary>
        /// <param name="corpid">企业号ID</param>
        /// <param name="corpsecret">管理组密钥</param>
        /// <returns></returns>
        private string GetAccessToken(string corpid, string corpsecret)
        {
            string accessToken = "";
            string respText = "";
            string url = string.Format("https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={0}&corpsecret={1}", corpid, corpsecret);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (Stream resStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(resStream, Encoding.Default);
                respText = reader.ReadToEnd();
                resStream.Close();
            }
            JavaScriptSerializer Jss = new JavaScriptSerializer();
            Dictionary<string, object> respDic = (Dictionary<string, object>)Jss.DeserializeObject(respText);
            accessToken = respDic["access_token"].ToString();//通过键access_token获取值
            return accessToken;
        }

        /// <summary>
        /// Post数据接口
        /// </summary>
        /// <param name="postUrl">接口地址</param>
        /// <param name="paramData">提交json数据</param> 
        /// <param name="dataEncode">编码方式</param>
        /// <returns></returns>
        private string PostWebRequest(string postUrl, string paramData, Encoding dataEncode)
        {
            string ret = string.Empty;
            try
            {
                byte[] byteArray = dataEncode.GetBytes(paramData);
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(postUrl));
                webReq.Method = "POST";
                webReq.ContentType = "application/x-www-form-urlencoded";
                webReq.ContentLength = byteArray.Length;
                Stream newStream = webReq.GetRequestStream();
                newStream.Write(byteArray, 0, byteArray.Length);
                newStream.Close();
                HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);
                ret = sr.ReadToEnd();
                sr.Close();
                response.Close();
                newStream.Close();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return ret;
        }

        private string _toUserID = "@all";
        /// <summary>
        /// 目标用户ID
        /// </summary>
        public string ToUserID
        {
            get { return _toUserID; }
            set { _toUserID = value; }
        }

        private int _agentId = 0;
        /// <summary>
        /// 应用ID
        /// </summary>
        public int AgentId
        {
            get { return _agentId; }
            set { _agentId = value; }
        }

        private string _toPartyID = "";
        /// <summary>
        /// 目标组ID，设置时目标用户ID将清空
        /// </summary>
        public string ToPartyID
        {
            get { return _toPartyID; }
            set { _toPartyID = value; this.ToUserID = ""; }
        }

        private string _toTagID = "";
        /// <summary>
        /// 目标标签ID，设置时目标用户ID将清空
        /// </summary>
        public string ToTagID
        {
            get { return _toTagID; }
            set { _toTagID = value; this.ToUserID = ""; }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="text">发送文本</param>
        /// <returns></returns>
        public string SendMessage(string text)
        {
            string paramData = "{";
            if (!string.IsNullOrEmpty(ToUserID))
            {
                paramData += "\"touser\": \"" + ToUserID + "\",";
            }
            if (!string.IsNullOrEmpty(ToPartyID))
            {
                paramData += "\"toparty\": \"" + ToPartyID + "\",";
            }
            if (!string.IsNullOrEmpty(ToTagID))
            {
                paramData += "\"totag\": \"" + ToTagID + "\",";
            }
            paramData += "\"msgtype\": \"text\",";
            paramData += "\"agentid\": \"" + AgentId + "\",";
            paramData += "\"text\": {\"content\": \"" + text + "\"},";
            paramData += "\"safe\":\"0\"";
            paramData += "}";

            Encoding dataEncode = Encoding.UTF8;

            WeiXinProvider provider = EnvProvider.Default.WeiXinProvider;
            string accessToken = GetAccessToken(provider.CorpId, provider.Secret);
            string postUrl = string.Format("https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token={0}", accessToken);
            return PostWebRequest(postUrl, paramData, dataEncode);
        }
    }
}
