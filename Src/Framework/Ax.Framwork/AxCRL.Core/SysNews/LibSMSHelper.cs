using AxCRL.Comm.Service;
using AxCRL.Comm.Utils;
using AxCRL.Core.Mail;
using AxCRL.Data;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxSRL.SMS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.SysNews
{
    public static class LibSMSHelper
    {
        /// <summary>
        /// 自定义发送短信的委托
        /// </summary>
        /// <param name="msgParam"></param>
        /// <returns></returns>
        public delegate void CustomSendSmsMsgCall(SendSMSParam msgParam);
        /// <summary>
        /// 需要发送短信时触发的自定义发送短信的事件
        /// 如果有事件处理绑定到了此方法则不再使用AX框架中默认的发送短信方法
        /// </summary>
        public static event CustomSendSmsMsgCall CustomSendSmsMsg;
        /// <summary>
        /// 发送短信消息
        /// </summary>
        /// <param name="destObj">参数设置为object是为了ThreadPool.QueueUserWorkItem调用，具体类型为SendSMSParam</param>
        public static void SendMsg(object destObj)
        {
            try
            {
                if (destObj == null)
                    return;
                //如果有事件处理绑定到了此方法则不再使用AX框架中默认的发送短信方法
                if (CustomSendSmsMsg != null)
                {
                    CustomSendSmsMsg(destObj as SendSMSParam);
                    return;
                }
                ILibSMSService svc = new LibSMSService();
                SendSMSParam param = destObj as SendSMSParam;
                if (param != null)
                    svc.SendMsg(param);
            }
            catch (Exception)
            {
                // to do log
            }           
        }
        /// <summary>
        /// 发送微信消息
        /// </summary>
        /// <param name="destObj">类型为SendSMSParam</param>
        public static void SendWeiXinMsg(object destObj)
        {
            try
            {
                ILibSMSService svc = new LibWinXinService();
                SendSMSParam param = destObj as SendSMSParam;
                if (param != null)
                    svc.SendMsg(param);
            }
            catch (Exception)
            {
                // to do log
            }           
        }
        /// <summary>
        /// 发送短信消息，参数类型为统一的邮件信息参数
        /// </summary>
        /// <param name="destObj">类型为LibMailParam的List数组</param>
        public static void SendMsgByMailList(object destObj)
        {
            try
            {
                List<LibMailParam> sendMailParamList = destObj as List<LibMailParam>;
                if (sendMailParamList == null || sendMailParamList.Count == 0)
                    return;
                sendMailParamList.ForEach(mailParam =>
                {
                    if (mailParam == null)
                        return;
                    SendSMSParam smsParam = GetSendParam(mailParam, false);
                    if (smsParam == null || string.IsNullOrEmpty(smsParam.Message) || string.IsNullOrEmpty(smsParam.Message.TrimEnd())
                    || smsParam.PhoneList == null || smsParam.PhoneList.Count == 0)
                        return;
                    SendMsg(smsParam);
                });
            }
            catch (Exception)
            {
                // to do log
            }
        }
        /// <summary>
        /// 发送微信消息，参数类型为统一的邮件信息参数
        /// </summary>
        /// <param name="destObj">类型为LibMailParam的List对象</param>
        public static void SendWeiXinMsgByMailList(object destObj)
        {
            try
            {
                List<LibMailParam> sendMailParamList = destObj as List<LibMailParam>;
                if (sendMailParamList == null || sendMailParamList.Count == 0)
                    return;
                sendMailParamList.ForEach(mailParam => {
                    if (mailParam == null)
                        return;
                    SendSMSParam smsParam = GetSendParam(mailParam, true);
                    if (smsParam == null || string.IsNullOrEmpty(smsParam.Message) || string.IsNullOrEmpty(smsParam.Message.TrimEnd())
                    || smsParam.PhoneList == null || smsParam.PhoneList.Count == 0)
                        return;
                    SendWeiXinMsg(smsParam);
                });
            }
            catch (Exception)
            {
                // to do log
            }            
        }
        private static SendSMSParam GetSendParam(LibMailParam sendMailParam,bool isWeixin)
        {
            if (sendMailParam == null||string.IsNullOrEmpty(sendMailParam.Content)|| string.IsNullOrEmpty(sendMailParam.Content.TrimEnd()))
                return null;
            List<string> weixinList = new List<string>();
            List<string> phoneList = new List<string>();
            phoneList = GetSendPhoneList(sendMailParam, out weixinList);
            SendSMSParam smsParam = new SendSMSParam()
            {
                Message = sendMailParam.Content.TrimEnd(),
                PhoneList = (isWeixin) ? weixinList : phoneList
            };
            return smsParam;
        }
        /// <summary>
        /// 根据信息发送参数中的收件人、抄送人等获取各用户对应手机号
        /// </summary>
        /// <param name="sendMailParam">消息参数</param>
        /// <param name="weixinList">人员Id对应的微信号列表，如果人员表没有微信号字段，或者微信号字段为空，则使用手机号</param>
        /// <returns></returns>
        private static List<string> GetSendPhoneList(LibMailParam sendMailParam, out List<string> weixinList)
        {

            List<string> personIdList = new List<string>();
            List<string> phoneList = new List<string>();
            weixinList = new List<string>();
            if (sendMailParam == null)
                return phoneList;           
            if (string.IsNullOrEmpty(sendMailParam.PersonId) == false && personIdList.Contains(sendMailParam.PersonId) == false)
            {
                personIdList.Add(sendMailParam.PersonId);
            }
            if (sendMailParam.To != null)
            {
                foreach (string per in sendMailParam.To)
                {
                    if (personIdList.Contains(per) == false && personIdList.Contains(per) == false)
                        personIdList.Add(per);
                }
            }
            if (sendMailParam.CC != null)
            {
                foreach (string per in sendMailParam.CC)
                {
                    if (personIdList.Contains(per) == false && personIdList.Contains(per) == false)
                        personIdList.Add(per);
                }
            }
            if (personIdList.Count == 0)
                return phoneList;
            StringBuilder builder = new StringBuilder();
            personIdList.ForEach(personId =>
            {
                builder.AppendFormat("{0},", LibStringBuilder.GetQuotString(personId));
            });
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 1, 1);
                bool existsWeixin = false;
                LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel("com.Person");
                if (sqlModel != null && sqlModel.Tables.Count > 0 && sqlModel.Tables[0].Columns.Contains("WECHAT"))
                {
                    existsWeixin = true;
                }
                LibDataAccess dataAccess = new LibDataAccess();
                string sql = string.Empty;
                if (existsWeixin)
                {
                    sql = string.Format("select PHONENO,WECHAT from COMPERSON " +
                                           " where PERSONID in ({0})", builder.ToString());
                    using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            string phoneNo = LibSysUtils.ToString(reader[0]);
                            string weixin = LibSysUtils.ToString(reader[1]);
                            if (string.IsNullOrEmpty(phoneNo) == false)
                            {
                                phoneList.Add(phoneNo);
                                if (string.IsNullOrEmpty(weixin) == false)
                                    weixinList.Add(weixin);
                                else
                                    weixinList.Add(phoneNo);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(weixin) == false)
                                    weixinList.Add(weixin);
                            }
                        }
                    }
                }
                else
                {
                    sql = string.Format("select PHONENO from COMPERSON " +
                                           " where PERSONID in ({0})", builder.ToString());
                    using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            string phoneNo = LibSysUtils.ToString(reader[0]);
                            if (string.IsNullOrEmpty(phoneNo) == false)
                            {
                                phoneList.Add(phoneNo);
                                weixinList.Add(phoneNo);                      
                            }
                        }
                    }
                }  
            }
            return phoneList;
        }      
    }
}
