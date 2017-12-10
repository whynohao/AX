using AxCRL.Comm.Runtime;
using AxCRL.Comm.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AxSRL.SMS
{
    public class LibSMSService : ILibSMSService
    {
        void ILibSMSService.SendMsg(SendSMSParam param)
        {
            if (param.PhoneList == null || param.PhoneList.Count == 0)
                throw new ArgumentNullException("短信接收号码为空值");
            try
            {
                StringBuilder builder = new StringBuilder();
                foreach (string phone in param.PhoneList)
                {
                    builder.AppendFormat("{0},", phone);
                }
                builder.Remove(builder.Length - 1, 1);
                string phoneStr = builder.ToString();

                SMSProvider provider = EnvProvider.Default.SMSProvider;
                //SMS.wmgwSoapClient client = new SMS.wmgwSoapClient();//调用的方法               
                //SMSProvider provider = EnvProvider.Default.SMSProvider;//提供必要参数信息：Host(地址),SMSSys（用户名）,SMSPwd,Port（端口）
                if (provider != null && !string.IsNullOrEmpty(provider.SMSSys))
                {
                    string message = "【" + provider.SMSSign + "】" + param.Message.Trim();
                    //string strparam = "action=send&userid=" + param.UserId + "&account=" + param.Account + "&password=" + param.PassWord + "&content=" + message + "&mobile=" + phoneStr + "&sendtime=";
                    string strparam = string.Format("action=send&userid={0}&account={1}&password={2}&content={3}&mobile={4}&sendtime=",
                        provider.SMSUserId, provider.SMSSys, provider.SMSPwd, message, phoneStr);
                    byte[] bs = Encoding.GetEncoding("GB2312").GetBytes(strparam);

                    string straddress = "http://" + provider.Host + ":" + provider.Port.ToString() + "/smsGBK.aspx";
                    HttpWebRequest postSmsInfoReq = (HttpWebRequest)HttpWebRequest.Create(straddress);
                    postSmsInfoReq.Method = "POST";
                    postSmsInfoReq.ContentType = "application/x-www-form-urlencoded";//头
                    postSmsInfoReq.ContentLength = bs.Length;

                    using (Stream reqStream = postSmsInfoReq.GetRequestStream())
                    {
                        reqStream.Write(bs, 0, bs.Length);
                    }
                    //string port = provider.Port == 0 ? "*" : provider.Port.ToString();
                    //string ret = client.MongateCsSpSendSmsNew(provider.SMSSys, provider.SMSPwd, phoneStr, message,
                    //    param.PhoneList.Count, port, string.Empty, string.Empty, string.Empty,
                    //    string.Empty, string.Empty, string.Empty, 0, string.Empty, string.Empty, 0);//调用方法，返回参数给ret
                    //string ret = string.Empty;                    
                    //using (WebResponse wr = postSmsInfoReq.GetResponse())
                    //{
                    //    StreamReader sr = new StreamReader(wr.GetResponseStream(), System.Text.Encoding.Default);
                    //    ret = sr.ReadToEnd().Trim();
                    //}
                    //if (ret.Length < 6)//txt文件保存错误信息
                    //{
                    //    string path = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output", "Error", "SMS", string.Format("{0}.txt", DateTime.Now.Ticks));//将字符串组成一个路径
                    //    using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
                    //    {
                    //        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                    //        {
                    //            sw.Write(string.Format("发送失败，错误码：{0}", ret));

                    //        }
                    //    }
                    //}
                    SendSmsResult ret = null;
                    using (WebResponse wr = postSmsInfoReq.GetResponse())
                    {
                        StreamReader sr = new StreamReader(wr.GetResponseStream(), System.Text.Encoding.Default);
                        ret = ReadParseRetXml(sr);
                    }
                    if (ret != null && ret.ReturnStatus == false)
                    {
                        string path = Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output", "Error", "SMS");//将字符串组成一个路径
                        if (Directory.Exists(path) == false)
                            Directory.CreateDirectory(path);
                        path = Path.Combine(path, string.Format("{0}.txt", DateTime.Now.Ticks));                        
                        using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
                        {
                            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                            {
                                sw.Write(string.Format("{0}:发送短信失败,{1}",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ret.ToString()));

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string path = Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output", "Error", "SMS");//将字符串组成一个路径
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);
                path = Path.Combine(path, string.Format("{0}.txt", DateTime.Now.Ticks));
                using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                    {
                        sw.Write(string.Format("{0}:发送短信失败,异常：{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ex));
                    }
                }
            }
        }
        /// <summary>
        /// 发送短信的结果
        /// </summary>
        class SendSmsResult
        {
            public bool ReturnStatus { get; set; }
            public string Message { get; set; }
            public uint RemainPoint { get; set; }
            public int TaskID { get; set; }
            public uint SuccessCounts { get; set; }
            public override string ToString()
            {
                return string.Format("是否成功:{0},消息:{1},本次发送成功数:{2},剩余总发送数:{3},任务编号:{4}",
                                   ReturnStatus, Message, SuccessCounts, RemainPoint, TaskID);
            }
        }
        static SendSmsResult ReadParseRetXml(StreamReader xmlResult)
        {
            //返回信息
            //<? xml version = "1.0" encoding = "gb2312" ?>< returnsms >   
            //< returnstatus > Faild </ returnstatus >
            //< message > 非法签名 </ message >   
            //< remainpoint > 0 </ remainpoint >
            //< taskID > 0 </ taskID >
            //< successCounts > 0 </ successCounts ></ returnsms >
            SendSmsResult result = new SendSmsResult();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlResult);
                //查找<returnsms>
                XmlNode root = xmlDoc.SelectSingleNode("returnsms");
                if(root!=null)
                {
                    //遍历所有子属性
                    foreach (XmlNode xmlNode in root.ChildNodes)
                    {
                        switch (xmlNode.Name)
                        {
                            case "returnstatus":
                                result.ReturnStatus = "Success".Equals(xmlNode.InnerText);
                                break;
                            case "message":
                                result.Message = xmlNode.InnerText;
                                break;
                            case "remainpoint":
                                uint remainPoint = 0;
                                uint.TryParse(xmlNode.InnerText, out remainPoint);
                                result.RemainPoint = remainPoint;
                                break;
                            case "taskID":
                                int taskId = 0;
                                int.TryParse(xmlNode.InnerText, out taskId);
                                result.TaskID = taskId;
                                break;
                            case "successCounts":
                                uint successCount = 0;
                                uint.TryParse(xmlNode.InnerText, out successCount);
                                result.SuccessCounts = successCount;
                                break;
                        }
                    }
                }
            }
            catch
            {

            }
            return result;
        }

    }
}
