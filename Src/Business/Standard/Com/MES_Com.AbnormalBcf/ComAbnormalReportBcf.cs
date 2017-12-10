using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;
using AxCRL.Data.SqlBuilder;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxCRL.Core.Server;
using AxCRL.Core.SysNews;
using AxCRL.Core.Comm;
using System.Threading;
using AxCRL.Comm.Service;
using AxCRL.Bcf.ScheduleTask;
using AxCRL.Core.Cache;
using AxCRL.Comm.Bill;
using MES_Sys.UtilsBcf.Com;
using System.Net.Mail;


namespace MES_Com.AbnormalBcf
{
    /// <summary>
    /// 异常报告单
    /// </summary>
    [ProgId(ProgId = "com.AbnormalReport", ProgIdType = ProgIdType.Bcf, VclPath = @"/Scripts/module/mes/com/comAbnormalReportVcl.js")]
    public class ComAbnormalReportBcf : LibBcfData
    {
        /// <summary>
        /// 异常报告单 注册模板
        /// </summary>
        /// <returns>返回 异常报告单 的数据模板</returns>
        protected override LibTemplate RegisterTemplate()
        {
            return new ComAbnormalReportBcfTemplate("com.AbnormalReport");
        }

        /// <summary>
        /// 修正表字段关联字段值
        /// </summary>
        /// <param name="tableIndex">异常报告单 哪张表</param>
        /// <param name="fieldName">异常报告单 表中字段</param>
        /// <param name="curPk">异常报告单 主键数组</param>
        /// <param name="fieldKeyAndValue">异常报告单 字段跟字段值键值对</param>
        /// <param name="returnValue">需要修正的字段跟字段值的字典</param>
        protected override void CheckFieldReturn(int tableIndex, string fieldName, object[] curPk, Dictionary<string, object> fieldKeyAndValue, Dictionary<string, object> returnValue)
        {
            base.CheckFieldReturn(tableIndex, fieldName, curPk, fieldKeyAndValue, returnValue);
            if (tableIndex == 0 && fieldName.CompareTo("ABNORMALTYPEID") == 0)
            {
                returnValue.Add("ABNORMALID", string.Empty);
                returnValue.Add("ABNORMALNAME", string.Empty);
            }
        }

        /// <summary>
        /// 异常报告单 保存前验证【异常开始时间不得为空且小于异常结束时间】
        /// </summary>
        protected override void BeforeUpdate()
        {
            base.BeforeUpdate();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            long startTime = LibSysUtils.ToInt64(masterRow["STARTTIME"]);
            long endTime = LibSysUtils.ToInt64(masterRow["ENDTIME"]);
            if (startTime == 0)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "异常开始时间必须>0并符合本系统的日期格式！");
            }
            if (endTime != 0 && startTime > endTime)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "异常开始时间不能大于异常结束时间。");
            }
        }

        /// <summary>
        /// 异常报告单 保存后发送桌面消息和邮件消息
        /// </summary>
        //protected override void AfterUpdate()
        //{
        //    base.AfterUpdate();
        //    DataRowCollection row = this.DataSet.Tables[0].Rows;
        //    if (row.Count <= 0) { return; }

        //    DataRow masterRow = this.DataSet.Tables[0].Rows[0];

        //    if (masterRow.RowState == DataRowState.Deleted) { return; }

        //    #region 桌面 我的信息
        //    LibSysNews news = new LibSysNews();
        //    news.Title = LibSysUtils.ToString(masterRow["ABNORMALDESC"]);
        //    news.Content = LibSysUtils.ToString(masterRow["ABNORMALDESC"]);
        //    news.PersonId = this.Handle.PersonId;
        //    news.ProgId = this.ProgId;
        //    for (int i = 0; i < row.Count; i++)
        //    {
        //        news.UserList.Add(LibSysUtils.ToString(row[i]["PERSONID"]));
        //    }
        //    LibSysNewsHelper.SendNews(news);
        //    #endregion

        //    #region 发送邮件信息
        //    SmtpClient smtpClient = new SmtpClient();
        //    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
        //    smtpClient.Host = "smtp.163.com";//指定SMTP服务器
        //    smtpClient.Credentials = new System.Net.NetworkCredential("15715702347", "hzjk_123");//用户名和密码
        //    MailMessage mailMessage = new MailMessage();
        //    mailMessage.From = new MailAddress("15715702347@163.com", "杭州集控");//发件人
        //    for (int i = 1; i < row.Count; i++)
        //    {
        //        string to_email = LibSysUtils.ToString(masterRow["FROMMAILNO"]);
        //        if (System.Text.RegularExpressions.Regex.IsMatch(to_email, @"^([\w-]+\.)*?[\w-]+@[\w-]+\.([\w-]+\.)*?[\w]+$"))
        //        {
        //            mailMessage.To.Add(to_email);//收件人
        //        }
        //    }
        //    mailMessage.Subject = LibSysUtils.ToString(masterRow["ABNORMALDESC"]); // "天下之大"; //主题
        //    mailMessage.Body = LibSysUtils.ToString(masterRow["ABNORMALDESC"]); // "为我独尊   工信智慧"; //内容
        //    mailMessage.BodyEncoding = System.Text.Encoding.UTF8;//正文编码
        //    mailMessage.IsBodyHtml = true;//设置为HTML格式
        //    mailMessage.Priority = MailPriority.High;//优先级
        //    smtpClient.Send(mailMessage);
        //    mailMessage.Dispose();
        //    #endregion
        //}

        /// <summary>
        /// 异常报告单 数据提交后开启或关闭排程任务
        /// </summary>
        protected override void AfterCommintData()
        {
            base.AfterCommintData();
            // 单据是否被删除，若是则删除其在系统业务临时任务中的排程任务；否则针对过账情况，修正排程任务
            if (this.BillAction != AxCRL.Bcf.BillAction.Delete)
            {
                // 异常报告单 数据保存无异常时进行排程任务的修正
                if (!this.ManagerMessage.IsThrow)
                {
                    DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                    // 异常报告单进行单据过账，查看其过账模式
                    PostAccountWay way = PostAccountHelper.GetPostAccountWay(PostAccountState.Release, masterRow);
                    switch (way)
                    {
                        case PostAccountWay.Positive:
                            // 正过账--发送系统信息、短信消息、添加排程任务到系统业务临时任务
                            SendMsg(masterRow);
                            break;
                        case PostAccountWay.Reverse:
                            // 反过账--若本单据在系统业务临时任务存在其排程任务，则将此任务删除
                            string taskId = LibSysUtils.ToString(this.DataAccess.ExecuteScalar(string.Format("select TASKID from AXPBUSINESSTEMPTASK where INTERNALID={0}", LibStringBuilder.GetQuotObject(masterRow["INTERNALID"]))));
                            if (!string.IsNullOrEmpty(taskId))
                            {
                                LibScheduleTaskHost.Default.DeleteTask(taskId);
                            }
                            break;
                        case PostAccountWay.Diff:
                            // 差异过账--当单据修改后，处理状态为已处理或已拒绝时，发送处理完成报告给相关人员
                            if (masterRow.HasVersion(DataRowVersion.Original))
                            {
                                //异常报告单 处理状态 当前值
                                int dealwithState = LibSysUtils.ToInt32(masterRow["DEALWITHSTATE"]);
                                //异常报告单 处理状态 原始值
                                int oldDealwithState = LibSysUtils.ToInt32(masterRow["DEALWITHSTATE", DataRowVersion.Original]);
                                //若异常报告单 处理状态为已处理或已拒绝时，发送处理完成报告给相关人员
                                if (dealwithState != oldDealwithState && (dealwithState == 2 || dealwithState == 3))
                                {
                                    SendFinishMsg(masterRow, dealwithState);
                                }
                                else if (dealwithState == 0)
                                {
                                    SendMsg(masterRow);
                                }
                            }
                            break;
                    }
                }
            }
            else
            {
                // 若本单据在系统业务临时任务存在其排程任务，则将此任务删除
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                string taskId = LibSysUtils.ToString(this.DataAccess.ExecuteScalar(string.Format("select TASKID from AXPBUSINESSTEMPTASK where INTERNALID={0}", LibStringBuilder.GetQuotObject(masterRow["INTERNALID"]))));
                if (!string.IsNullOrEmpty(taskId))
                {
                    LibScheduleTaskHost.Default.DeleteTask(taskId);
                }
            }
        }

        /// <summary>
        /// 获得当前异常报告单下当前信息预警层级下的信息预警流程信息集
        /// </summary>
        /// <param name="masterRow">异常报告单 主表行数据</param>
        /// <param name="transmitLevel">异常报告单 传递层级</param>
        /// <returns>返回要预警信息的流程信息集</returns>
        private List<TransmitInfo> GetTransmitInfo(DataRow masterRow, int transmitLevel)
        {
            //传输信息字典【使用条件--信息预警流程信息集】
            Dictionary<string, List<TransmitInfo>> transmitInfoDic = new Dictionary<string, List<TransmitInfo>>();
            //异常报告单 单据类型
            string typeId = LibSysUtils.ToString(masterRow["TYPEID"]);
            //异常报告单单据类型:com.AbnormalReportType
            SqlBuilder sqlBuilder = new SqlBuilder(this.Template.FuncPermission.BillTypeName);
            //获取异常报告单单据类型下的信息预警流程明细信息【使用条件，接收人代码，预警层级，接收人名称，微信，发微信，电话，发短信，响应时效，时间单位】
            string sql = sqlBuilder.GetQuerySql(1, "B.USECONDITION,C.TRANSMITLEVEL,C.PERSONID,C.PERSONNAME,C.WECHAT,C.SENDWECHAT,C.PHONENO,C.NEEDSMS,C.CONTROLTIME,C.TIMEUNIT", string.Format("B.TYPEID={0} and C.TRANSMITLEVEL={1}", LibStringBuilder.GetQuotString(typeId), transmitLevel), "B.USECONDITION,C.TRANSMITLEVEL");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    //异常报告单单据类型 信息预警流程规则配置 使用条件
                    string useCondition = LibSysUtils.ToString(reader["USECONDITION"]);
                    if (!transmitInfoDic.ContainsKey(useCondition))
                    {
                        transmitInfoDic.Add(useCondition, new List<TransmitInfo>());
                    }
                    TransmitInfo info = new TransmitInfo();
                    info.TransmitLevel = LibSysUtils.ToInt32(reader["TRANSMITLEVEL"]);
                    info.PersonId = LibSysUtils.ToString(reader["PERSONID"]);
                    info.PersonName = LibSysUtils.ToString(reader["PERSONNAME"]);
                    info.WeChat = LibSysUtils.ToString(reader["WECHAT"]);
                    info.SendWeChat = LibSysUtils.ToBoolean(reader["SENDWECHAT"]);
                    info.PhoneNo = LibSysUtils.ToString(reader["PHONENO"]);
                    info.NeedSMS = LibSysUtils.ToBoolean(reader["NEEDSMS"]);
                    info.ControlTime = LibSysUtils.ToDouble(reader["CONTROLTIME"]);
                    info.TimeUnit = LibSysUtils.ToInt32(reader["TIMEUNIT"]);
                    transmitInfoDic[useCondition].Add(info);
                }
            }
            //遍历字典找到跟异常报告单的预警层级和使用条件一致的信息预警流程明细集
            List<TransmitInfo> curTransmitInfo = null;
            foreach (var item in transmitInfoDic)
            {
                if (string.IsNullOrEmpty(item.Key))
                    continue;
                if (LibParseHelper.Parse(item.Key, new List<DataRow>() { masterRow }))
                {
                    curTransmitInfo = item.Value;
                    break;
                }
            }
            //若未找到符合使用条件的信息预警流程明细集，则取默认的无条件的信息预警流程明细集
            if (curTransmitInfo == null && transmitInfoDic.ContainsKey(string.Empty))
            {
                curTransmitInfo = transmitInfoDic[string.Empty];
            }
            return curTransmitInfo;
        }

        /// <summary>
        /// 当异常报告单数据修正后 发送处理完成报告给相关人员
        /// </summary>
        /// <param name="masterRow">异常报告单 主表行数据</param>
        /// <param name="dealwithState">异常报告单 处理状态</param>
        private void SendFinishMsg(DataRow masterRow, int dealwithState)
        {
            //异常报告单 传递层级
            int transmitLevel = LibSysUtils.ToInt32(masterRow["TRANSMITLEVEL"]);
            //异常报告单 报告人
            string fromPersonId = LibSysUtils.ToString(masterRow["FROMPERSONID"]);
            //异常报告单 报告人手机
            string fromPhoneNo = LibSysUtils.ToString(masterRow["FROMPHONENO"]);
            //异常报告单 报告人微信
            string fromWeChat = LibSysUtils.ToString(masterRow["FROMWECHAT"]);
            //当前异常报告单下当前传递消息层级下的消息传递流程信息集
            List<TransmitInfo> curTransmitInfo = GetTransmitInfo(masterRow, transmitLevel);
            //异常报告单 存在报告人或当前消息传递流程信息集
            if (curTransmitInfo != null || !string.IsNullOrEmpty(fromPersonId))
            {
                //系统消息类，用于发送系统消息,其属性用户列表中的用户不可重复
                LibSysNews news = new LibSysNews();
                //电话列表，用于发送短信，列表中手机号不可重复
                List<string> phoneList = new List<string>();
                //微信列表，用于发送微信，列表中微信号不可重复
                List<string> weChatList = new List<string>();

                #region 遍历当前消息传递流程信息集，为其接收人（根据需要）发送短信/发送微信、发系统信息
                if (curTransmitInfo != null)
                {
                    foreach (var info in curTransmitInfo)
                    {
                        //将接收人手机（手机号存在）添加到电话列表中，以便发短信
                        if (info.NeedSMS && !string.IsNullOrEmpty(info.PhoneNo))
                        {
                            if (!phoneList.Contains(info.PhoneNo))
                                phoneList.Add(info.PhoneNo);
                        }
                        //将接收人微信（微信号存在，若不存在则将其手机号作为微信号）添加到微信列表中，以便发微信
                        if (info.SendWeChat)
                        {
                            if (!string.IsNullOrEmpty(info.WeChat))
                            {
                                if (!weChatList.Contains(info.WeChat))
                                    weChatList.Add(info.WeChat);
                            }
                            else if (!string.IsNullOrEmpty(info.PhoneNo))
                            {
                                if (!weChatList.Contains(info.PhoneNo))
                                    weChatList.Add(info.PhoneNo);
                            }
                        }
                        //将接收人添加到其用户列表中，以便发系统信息
                        news.UserList.Add(info.PersonId);
                    }
                }
                #endregion

                #region 为报告人发送短信、发送微信、发系统信息
                //将报告人电话（手机号存在）添加到电话列表中，以便发短信
                if (!string.IsNullOrEmpty(fromPhoneNo) && !phoneList.Contains(fromPhoneNo))
                {
                    phoneList.Add(fromPhoneNo);
                }
                //将报告人微信（微信号存在，若不存在则将其手机号作为微信号）添加到微信列表中，以便发微信
                if (!string.IsNullOrEmpty(fromWeChat))
                {
                    if (!weChatList.Contains(fromWeChat))
                        weChatList.Add(fromWeChat);
                }
                else if (!string.IsNullOrEmpty(fromPhoneNo))
                {
                    if (!weChatList.Contains(fromPhoneNo))
                        weChatList.Add(fromPhoneNo);
                }
                //将报告人添加到人员列表中，以便发系统信息
                if (!string.IsNullOrEmpty(fromPersonId) && !news.UserList.Contains(fromPersonId))
                {
                    news.UserList.Add(fromPersonId);
                }
                #endregion

                #region 推送异常完结消息所需字段
                //班组
                string workTeamName = string.Empty;
                //异常报告单 单据编号
                string billNo = LibSysUtils.ToString(masterRow["BILLNO"]);
                //异常报告单 异常名称
                string abnormalName = LibSysUtils.ToString(masterRow["ABNORMALNAME"]);
                //异常报告单 异常描述
                string abnormalDesc = LibSysUtils.ToString(masterRow["ABNORMALDESC"]);
                //异常报告单 负责人名称，作为责任人
                string personName = LibSysUtils.ToString(masterRow["PERSONNAME"]);
                //异常报告单 异常结束时间
                string needTime = LibSysUtils.ToString(masterRow["ENDTIME"]);
                //异常报告单 异常处理时间
                string strNeedTime = string.Empty;
                //异常处理时间为异常结束时间的月日时分
                if (needTime.Length >= 14)
                {
                    DateTime dateTime = new DateTime(int.Parse(needTime.Substring(0, 4)), int.Parse(needTime.Substring(4, 2)), int.Parse(needTime.Substring(6, 2)), int.Parse(needTime.Substring(8, 2)), int.Parse(needTime.Substring(10, 2)), int.Parse(needTime.Substring(12, 2)));
                    strNeedTime = dateTime.ToString("MM/dd HH:mm");
                }
                //异常报告单 报告人名称，作为跟踪人
                string fromPersonName = LibSysUtils.ToString(masterRow["FROMPERSONNAME"]);
                string dealwithStateStr = dealwithState == 2 ? "已处理完成" : "已被拒绝处理";
                #endregion

                #region 发送系统消息
                string data = GetMsgData(masterRow);
                news.Title = string.Format("异常报告单{0}{1}", masterRow["BILLNO"], dealwithStateStr);
                news.Content = string.Format("\n 班组：{0}; \n异常类型：{1}; \n问题：{2}; \n责任人：{3}; \n要求解决时间：{4}; \n跟踪人：{5}; \n异常单号：{6}; \n处理状态：{7}; \n",
                    FormatData(workTeamName), abnormalName, abnormalDesc, FormatData(personName), FormatData(strNeedTime), FormatData(fromPersonName), billNo, dealwithStateStr);
                news.Data = data;
                news.PersonId = LibSysUtils.ToString(masterRow["CREATORID"]);
                news.ProgId = this.ProgId;
                LibSysNewsHelper.SendNews(news);
                #endregion

                #region 发送短信消息
                if (phoneList.Count > 0)
                {
                    SendSMSParam param = new SendSMSParam();
                    param.PhoneList = phoneList;
                    param.Message = news.Content;
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(LibSMSHelper.SendMsg), param);
                }
                #endregion

                #region 发送微信消息
                if (weChatList.Count > 0)
                {
                    SendSMSParam param = new SendSMSParam();
                    param.PhoneList = weChatList;
                    param.Message = news.Content;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(LibSMSHelper.SendWeiXinMsg), param);
                }
                #endregion
            }
        }

        /// <summary>
        /// 获取异常报告单 的主键数据集合和入口参数字段键值对集合
        /// </summary>
        /// <param name="masterRow">异常报告单 主表行数据</param>
        /// <returns>返回主表主键数据集和入口参数字段键值对集合</returns>
        private string GetMsgData(DataRow masterRow)
        {
            string data = string.Empty;
            StringBuilder pkBuilder = new StringBuilder();
            //遍历主表所有的主键字段，将其字段值用','连接起来并存储在pkBuilder中，例如："'2017060900001',1,'TC001',"
            foreach (DataColumn col in masterRow.Table.PrimaryKey)
            {
                LibDataType dataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                if (dataType == LibDataType.Text || dataType == LibDataType.NText)
                    pkBuilder.AppendFormat("'{0}',", masterRow[col]);
                else
                    pkBuilder.AppendFormat("{0},", masterRow[col]);
            }
            pkBuilder.Remove(pkBuilder.Length - 1, 1);
            //构造功能单据的主键数据集合，例如："['2017060900001',1,'TC001']"
            data = string.Format("[{0}]", pkBuilder.ToString());
            //当前数据模板的功能许可的入库参数存在
            if (this.Template.FuncPermission.EntryParam.Count > 0)
            {
                StringBuilder entryBuilder = new StringBuilder();
                entryBuilder.Append("{ParamStore:{");
                //遍历数据模板的入口参数，将其对应的主表上的字段值以“字段：字段值”的模式用','连接起来并存储在entryBuilder中，
                //例如："{ParamStore:{ID:'2017060900001',ROW_ID:1,TYPE:'TC001'}}"
                foreach (string entryParam in this.Template.FuncPermission.EntryParam)
                {
                    DataColumn col = this.DataSet.Tables[0].Columns[entryParam];
                    LibDataType dataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                    if (dataType == LibDataType.Text || dataType == LibDataType.NText)
                        entryBuilder.AppendFormat("{0}:'{1}',", entryParam, masterRow[col]);
                    else
                        entryBuilder.AppendFormat("{0}:{1},", entryParam, masterRow[col]);
                }
                entryBuilder.Remove(entryBuilder.Length - 1, 1);
                entryBuilder.Append("}}");
                data += string.Format(";{0}", entryBuilder.ToString());
            }
            //功能单据的主数据集合，例如"['2017060900001',1,'TC001'];{ParamStore:{ID:'2017060900001',ROW_ID:1,TYPE:'TC001'}}"
            return data;
        }

        /// <summary>
        /// 异常报告单 发送系统预警消息、短信预警信息、微信预警信息
        /// </summary>
        /// <param name="masterRow">异常报告单 主表行数据</param>
        private void SendMsg(DataRow masterRow)
        {
            string data = GetMsgData(masterRow);
            //根据异常报告单的单据编号获得异常报告单的数据模型
            LibBcfData bcfData = (LibBcfData)LibBcfSystem.Default.GetBcfInstance("com.AbnormalReport");
            DataSet ds = bcfData.BrowseTo(new object[] { masterRow["BILLNO"] });
            DataRow messageRow = ds.Tables[0].Rows[0];
            //系统消息类，用于发送系统消息,其属性用户列表中的用户不可重复
            LibSysNews news = new LibSysNews();
            //电话列表，用于发送短信，列表中手机号不可重复
            List<string> phoneList = new List<string>();
            //微信列表，用于发送微信，列表中微信号不可重复
            List<string> weChatList = new List<string>();

            #region 推送异常报告信息预警所需字段
            //班组
            string workTeamName = string.Empty;
            //异常报告单 单据编号
            string billNo = LibSysUtils.ToString(messageRow["BILLNO"]);
            //异常报告单 异常名称
            string abnormalName = LibSysUtils.ToString(messageRow["ABNORMALNAME"]);
            //异常报告单 异常描述
            string abnormalDesc = LibSysUtils.ToString(messageRow["ABNORMALDESC"]);
            //异常报告单 负责人手机
            string destPhoneNo = LibSysUtils.ToString(messageRow["DESTPHONENO"]);
            //异常报告单 负责人微信
            string destWeChat = LibSysUtils.ToString(messageRow["DESTWECHAT"]);
            //异常报告单 报告人手机
            string fromPhoneNo = LibSysUtils.ToString(messageRow["FROMPHONENO"]);
            //异常报告单 报告人微信
            string fromWeChat = LibSysUtils.ToString(messageRow["FROMWECHAT"]);
            //抄送人名称集合
            string fromPersonName = string.Empty;
            //解决人名称集合 
            string personName = string.Empty;
            //定时器结束定时时间
            DateTime? endTime = null;
            //异常报告单 传递层级+1为异常报告预警流程中的相关消息传递层级
            int transmitLevel = LibSysUtils.ToInt32(messageRow["TRANSMITLEVEL"]) + 1;
            #endregion

            #region 当初始进入定时推送预警信息时（传递层级为0），为异常报告单中的负责人、报告人发短信/发微信，并异常报告单中的负责人发系统信息
            if (transmitLevel <= 1)
            {
                //将异常报告单中的报告人手机（手机号存在）添加到电话列表中，以便发短信
                if (!string.IsNullOrEmpty(fromPhoneNo))
                {
                    phoneList.Add(fromPhoneNo);
                }
                //将异常报告单中的负责人手机（手机号存在）添加到电话列表中，以便发短信
                if (!string.IsNullOrEmpty(destPhoneNo))
                {
                    if (!phoneList.Contains(destPhoneNo))
                        phoneList.Add(destPhoneNo);
                }
                //将异常报告单中的报告人微信（微信号存在，若不存在则将其手机号作为微信号）添加到微信列表中，以便发微信
                if (!string.IsNullOrEmpty(fromWeChat))
                {
                    weChatList.Add(fromWeChat);
                }
                else if (!string.IsNullOrEmpty(fromPhoneNo))
                {
                    weChatList.Add(fromPhoneNo);
                }
                //将异常报告单中的负责人微信（微信号存在，若不存在则将其手机号作为微信号）添加到微信列表中，以便发微信
                if (!string.IsNullOrEmpty(destWeChat))
                {
                    if (!weChatList.Contains(destWeChat))
                        weChatList.Add(destWeChat);
                }
                else if (!string.IsNullOrEmpty(destPhoneNo))
                {
                    if (!weChatList.Contains(destPhoneNo))
                        weChatList.Add(destPhoneNo);
                }
                //将异常报告单中的负责人添加到其用户列表中，以便发系统信息
                string personId = LibSysUtils.ToString(messageRow["PERSONID"]);
                if (!string.IsNullOrEmpty(personId))
                {
                    news.UserList.Add(personId);
                }
            }
            #endregion

            #region 获取此消息传递层级下的消息传递流程明细信息并为其接收人（根据需要）发短信/发微信、发系统信息、得到其异常解决期限和接收人名称集合
            //获得异常报告单下的消息传递流程信息集
            List<TransmitInfo> curTransmitInfo = GetTransmitInfo(messageRow, transmitLevel);
            if (curTransmitInfo != null)
            {
                //遍历消息传递流程信息集，为系统消息类的电话列表和用户列表赋值
                foreach (var info in curTransmitInfo)
                {
                    //将当前消息传递层级下的接收人手机（需要接受人接收短信且手机号存在）添加到电话列表中，以便发短信
                    if (info.NeedSMS && !string.IsNullOrEmpty(info.PhoneNo) && !phoneList.Contains(info.PhoneNo))
                    {
                        phoneList.Add(info.PhoneNo);
                    }
                    //将当前消息传递层级下的接收人微信（需要接收人接受微信且微信号存在，若不存在则将其手机号作为微信号）添加到微信列表中，以便发微信
                    if (info.SendWeChat)
                    {
                        if (!string.IsNullOrEmpty(info.WeChat))
                        {
                            if (!weChatList.Contains(info.WeChat))
                                weChatList.Add(info.WeChat);
                        }
                        else if (!string.IsNullOrEmpty(info.PhoneNo))
                        {
                            if (!weChatList.Contains(info.PhoneNo))
                                weChatList.Add(info.PhoneNo);
                        }
                    }
                    //当信息的传递层级大于1时，报告人名称就是接收人名称集合
                    if (transmitLevel > 1)
                    {
                        fromPersonName += info.PersonName + ';';
                    }
                    //为定时器结束时间赋值（根据响应时效，时间单位）
                    if (endTime == null && info.ControlTime != 0)
                    {
                        if (info.TimeUnit == 0)
                        {
                            endTime = DateTime.Now.AddHours(info.ControlTime);
                        }
                        else if (info.TimeUnit == 1)
                        {
                            endTime = DateTime.Now.AddMinutes(info.ControlTime);
                        }
                    }
                    //将当前消息传递层级下的接收人添加到其用户列表中，以便发系统信息
                    if (!news.UserList.Contains(info.PersonId))
                    {
                        news.UserList.Add(info.PersonId);
                    }
                }
            }
            #endregion

            #region 得到当前消息传递层级下的消息传递流程明细信息中的所有可以接收短信或微信的接收人（若不存在，则将负责人作为接收人），用于催促这些人员尽快解决问题
            GetDealPerson(ref personName, masterRow, transmitLevel);
            if (string.IsNullOrEmpty(personName))
            {
                personName = LibSysUtils.ToString(messageRow["PERSONNAME"]);
            }
            #endregion

            #region 发送系统信息
            string strNeedTime = endTime == null ? string.Empty : ((DateTime)endTime).ToString("MM/dd HH:mm");
            news.Title = string.Format("发生{0}异常", abnormalName);
            news.Content = string.Format("\n异常报告单号：{4};{0}; \n需要 {1} 尽快处理,\n解决时间：{2}; \n 抄送：{3}  ", abnormalDesc, FormatData(personName), FormatData(strNeedTime), FormatData(fromPersonName), billNo);
            news.Data = data;
            news.PersonId = LibSysUtils.ToString(messageRow["CREATORID"]);
            news.ProgId = this.ProgId;
            LibSysNewsHelper.SendNews(news);
            #endregion

            #region 发送短信
            if (phoneList.Count > 0)
            {
                SendSMSParam param = new SendSMSParam();
                param.PhoneList = phoneList;
                param.Message = news.Content;
                //ThreadPool.QueueUserWorkItem(new WaitCallback(LibSMSHelper.SendMsg), param);
            }
            #endregion

            #region 发送微信
            if (weChatList.Count > 0)
            {
                SendSMSParam param = new SendSMSParam();
                param.PhoneList = weChatList;
                param.Message = news.Content;
                ThreadPool.QueueUserWorkItem(new WaitCallback(LibSMSHelper.SendWeiXinMsg), param);
            }
            #endregion

            #region 在系统业务临时任务中加入当前排程任务
            if (curTransmitInfo != null)
            {
                //过账异常报告单的传递信息层级
                this.DataAccess.ExecuteNonQuery(string.Format("update COMABNORMALREPORT set TRANSMITLEVEL={0} where BILLNO={1}", transmitLevel, LibStringBuilder.GetQuotObject(masterRow["BILLNO"])));
                //添加下一个异常处理进度监控任务
                LibBusinessTask task = new LibBusinessTask();
                task.TaskType = LibTaskType.TempTask;
                task.ProgId = this.ProgId;
                task.BusinessTaskId = "ScheduleMonitor";
                task.ExecCondition = LibSysUtils.ToString(messageRow["BILLNO"]);
                long execDateTime = LibDateUtils.DateTimeToLibDateTime(endTime == null ? DateTime.Now : endTime.Value);
                task.ExecDate = LibDateUtils.GetLibTimePart(execDateTime, LibDateTimePartEnum.Date);
                task.ExecTime.Add(LibDateUtils.GetLibTimePart(execDateTime, LibDateTimePartEnum.Time));
                task.TaskId = string.Format("#{0}", Guid.NewGuid().ToString());
                task.InternalId = LibSysUtils.ToString(messageRow["INTERNALID"]);
                //将排程任务加入到系统业务临时任务中
                LibScheduleTaskHost.Default.AddTask(task, true);
            }
            #endregion
        }

        /// <summary>
        /// 获取不同层级下的信息预警流程中的接收人名称集合并赋值给抄送人
        /// </summary>
        /// <param name="fromPersonName">抄送人名称</param>
        /// <param name="materRow">异常报告单 主表行数据</param>
        /// <param name="transmitLevel">异常报告单 预警层级</param>
        private void GetDealPerson(ref string fromPersonName, DataRow materRow, int transmitLevel)
        {
            //当信息的预警层级大于1时，将预警层级减去1
            if (transmitLevel > 1)
            {
                transmitLevel = --transmitLevel;
            }
            //遍历预警层级的各个层级，得到相应层级的信息预警流程集
            for (int i = 1; i <= transmitLevel; i++)
            {
                List<TransmitInfo> curTransmitInfo = GetTransmitInfo(materRow, i);
                if (curTransmitInfo != null)
                {
                    //遍历当前预警层级的信息预警流程集
                    foreach (var info in curTransmitInfo)
                    {
                        //若当前信息预警流程需要发送短信/发送微信且接收人名称存在且抄送人名称不包含接收人名称，则将接收人用‘,’连接到抄送人名称中
                        if ((info.NeedSMS || info.SendWeChat) && !string.IsNullOrEmpty(info.PersonName) && !fromPersonName.Contains(info.PersonName))
                        {
                            fromPersonName += info.PersonName + ",";
                        }
                    }
                }
            }
            //若抄送人名称不空，则去除接收人名称集合的最后连接点并返回给抄送人名称
            if (!string.IsNullOrEmpty(fromPersonName))
            {
                fromPersonName = fromPersonName.Substring(0, fromPersonName.LastIndexOf(','));
            }
        }

        /// <summary>
        /// 格式化字段值
        /// </summary>
        /// <param name="data">字段值</param>
        /// <returns>返回格式化后的字段值</returns>
        private string FormatData(string data)
        {
            string format = data;
            if (string.IsNullOrEmpty(data))
            {
                format = "无";
            }
            else
            {
                if (data.IndexOf('【') >= 0)
                {
                    string workstationInfo = data.Substring(data.IndexOf('【'), data.IndexOf('】'));
                    data = data.Replace(workstationInfo, string.Empty);
                    format = data;
                }

            }
            return format;
        }

        /// <summary>
        /// 异常处理进度监控,由自动排程任务调用
        /// </summary>
        /// <param name="billNo">异常报告单 单据编号</param>
        public void ScheduleMonitor(string billNo)
        {
            this.BrowseTo(new object[] { billNo });
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            //异常报告单 处理状态["未处理", "处理中", "已处理", "已拒绝"]
            int dealwithState = LibSysUtils.ToInt32(masterRow["DEALWITHSTATE"]);
            //处理状态为未处理时向下一层级传递消息
            if (dealwithState == 0)
            {
                SendMsg(masterRow);
            }
        }

        /// <summary>
        /// 消息传递流程明细
        /// </summary>
        private class TransmitInfo
        {
            private int _TransmitLevel;
            private string _PersonId;
            private bool _NeedSMS;
            private bool _SendWeChat;
            private double _ControlTime;
            private int _TimeUnit;
            private string _PhoneNo;
            private string _WeChat;
            private string _PersonName;

            /// <summary>
            /// 接收人名称
            /// </summary>
            public string PersonName
            {
                get { return _PersonName; }
                set { _PersonName = value; }
            }

            /// <summary>
            /// 接收人电话
            /// </summary>
            public string PhoneNo
            {
                get { return _PhoneNo; }
                set { _PhoneNo = value; }
            }

            /// <summary>
            /// 接收人微信
            /// </summary>
            public string WeChat
            {
                get { return _WeChat; }
                set { _WeChat = value; }
            }

            /// <summary>
            /// 时间单位：时分
            /// </summary>
            public int TimeUnit
            {
                get { return _TimeUnit; }
                set { _TimeUnit = value; }
            }

            /// <summary>
            /// 响应时效
            /// </summary>
            public double ControlTime
            {
                get { return _ControlTime; }
                set { _ControlTime = value; }
            }

            /// <summary>
            /// 发短信
            /// </summary>
            public bool NeedSMS
            {
                get { return _NeedSMS; }
                set { _NeedSMS = value; }
            }

            /// <summary>
            /// 发微信
            /// </summary>
            public bool SendWeChat
            {
                get { return _SendWeChat; }
                set { _SendWeChat = value; }
            }

            /// <summary>
            /// 接收人代码
            /// </summary>
            public string PersonId
            {
                get { return _PersonId; }
                set { _PersonId = value; }
            }

            /// <summary>
            /// 传递层级
            /// </summary>
            public int TransmitLevel
            {
                get { return _TransmitLevel; }
                set { _TransmitLevel = value; }
            }
        }

        /// <summary>
        /// 生成异常追踪单
        /// </summary>
        /// <param name="abnormalReport">异常报告单 数据模型</param>
        /// <returns></returns>
        public DataTable BuildAbnormalTrace(AbnormalReport abnormalReport)
        {
            //若异常报告单的当前单据状态未生效，则无法生成异常追踪单
            if (LibSysUtils.ToInt32(abnormalReport.CurrentState) != 2)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("异常报告单未生效，不能生成异常追踪单！"));
            }
            else
            {
                //找到与异常报告单相对应的异常追踪单单据编号
                string sql = string.Format("select BILLNO  from COMABNORMALTRACE where FROMBILLNO={0}", LibSysUtils.ToString(abnormalReport.BillNo));
                string billNo = string.Empty;
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    if (reader.Read())
                    {
                        billNo = LibSysUtils.ToString(reader["BILLNO"]);
                    }
                }
                //异常报告单单据类型
                string abnormalType = LibSysUtils.ToString(abnormalReport.TypeId);
                //获得异常追踪单的入口单据类型参数
                LibEntryParam entryParam = new LibEntryParam();
                string typeid = abnormalReport.TypeId;
                typeid = typeid.Insert(typeid.Length - 1, "T");
                entryParam.ParamStore.Add("TYPEID", typeid);
                //若异常报告单相对应的异常追踪单单据编号存在，则不允许重新生成异常追踪单；否则生成一张异常追踪单
                DataSet dataSet = null;
                LibBcfData bcfData = (LibBcfData)LibBcfSystem.Default.GetBcfInstance("com.AbnormalTrace");
                if (string.IsNullOrEmpty(billNo))
                {
                    //异常追踪单在此入口参数下的数据模型
                    dataSet = bcfData.AddNew(entryParam);
                    FillTraceData(dataSet, abnormalReport);
                    dataSet = bcfData.InnerSave(BillAction.AddNew, null, dataSet);
                }
                else
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("已存在相应的异常追踪单{0},不能重复生成。", billNo));
                    return null;
                }
                //若异常追踪单单据生成无异常，则提示生成成功且返回主表数据；否则提示错误信息
                if (bcfData.ManagerMessage.IsThrow)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("异常追踪单{0}生成出错，错误信息为:", billNo));
                    foreach (LibMessage msg in bcfData.ManagerMessage.MessageList)
                    {
                        this.ManagerMessage.AddMessage(msg);
                    }
                }
                else
                {
                    string newBillNo = LibSysUtils.ToString(dataSet.Tables[0].Rows[0]["BILLNO"]);
                    this.ManagerMessage.AddMessage(LibMessageKind.Info, string.Format("异常报告单{0}生成成功，异常追踪单号为:{1}", billNo, newBillNo));

                    return dataSet.Tables[0];
                }
            }
            return null;
        }

        /// <summary>
        /// 填充异常追踪单的数据
        /// </summary>
        /// <param name="ds">异常追踪单新增时的数据模型</param>
        /// <param name="abnormalReport">异常报告单 数据模型</param>
        private void FillTraceData(DataSet ds, AbnormalReport abnormalReport)
        {
            DataRow masterRow = ds.Tables[0].Rows[0];
            masterRow.BeginEdit();
            try
            {
                masterRow["FROMBILLNO"] = LibSysUtils.ToString(abnormalReport.BillNo);
                masterRow["PLANENDTIME"] = LibDateUtils.GetCurrentDateTime();
                masterRow["ABNORMALREASONID"] = "";
                masterRow["PERSONID"] = LibSysUtils.ToString(abnormalReport.PersonId);
                masterRow["DEALWITHPERSONID"] = LibSysUtils.ToString(abnormalReport.PersonId); ;
                masterRow["SOLUTION"] = "无";
                masterRow["DEALWITHSTATE"] = 0;
            }
            finally
            {
                masterRow.EndEdit();
            }
        }

        /// <summary>
        /// 异常报告单
        /// </summary>
        public class AbnormalReport
        {
            private string billNo;
            /// <summary>
            /// 单据编号
            /// </summary>
            public string BillNo
            {
                get { return billNo; }
                set { billNo = value; }
            }

            private string typeId;
            /// <summary>
            /// 单据类型
            /// </summary>
            public string TypeId
            {
                get { return typeId; }
                set { typeId = value; }
            }

            private int currentState;
            /// <summary>
            /// 当前单据状态
            /// </summary>
            public int CurrentState
            {
                get { return currentState; }
                set { currentState = value; }
            }

            private string personId;
            /// <summary>
            /// 负责人代码
            /// </summary>
            public string PersonId
            {
                get { return personId; }
                set { personId = value; }
            }
        }
    }

    /// <summary>
    /// 异常报告单 数据模板
    /// </summary>
    public class ComAbnormalReportBcfTemplate : LibTemplate
    {
        // 异常报告单 主表
        private const string tableName = "COMABNORMALREPORT";

        /// <summary>
        /// 异常报告单 模板功能定义
        /// </summary>
        /// <param name="progId">异常报告单 功能标识</param>
        public ComAbnormalReportBcfTemplate(string progId)
            : base(progId, BillType.Bill, "异常报告单")
        {
        }

        /// <summary>
        /// 异常报告单 数据模型
        /// </summary>
        protected override void BuildDataSet()
        {
            base.BuildDataSet();
            this.DataSet = new DataSet();
            string primaryName = "BILLNO";
            DataTable headTable = new DataTable(tableName);
            DataSourceHelper.AddColumn(new DefineField(headTable, primaryName, "单据编号", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false, DataType = LibDataType.Text });
            DataSourceHelper.AddColumn(new DefineField(headTable, "TYPEID", "单据类型", FieldSize.Size50)
            {
                #region 异常报告单单据类型
                AllowEmpty = false,
                DataType = LibDataType.Text,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.AbnormalReportType")
                    {
                        RelFields = new RelFieldCollection()
                        {
                            new RelField("TYPENAME", LibDataType.NText, FieldSize.Size20, "单据类型名称"),
                            new RelField("ISREPULSE", LibDataType.Boolean, 0, "可拒绝"){ DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo }
                        }
                    }
                }
                #endregion
            });
            DataSourceHelper.AddBillDate(headTable);
            DataSourceHelper.AddColumn(new DefineField(headTable, "ABNORMALPROTOTYPE", "异常属性") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, TextOption = ComCommonalityData.ABNORMALPROTOTYPE });
            DataSourceHelper.AddColumn(new DefineField(headTable, "ABNORMALID", "异常", FieldSize.Size20)
            {
                #region 异常主数据
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.Abnormal")
                    {
                        //SelConditions = new SelConditionCollection()
                        //{ 
                        //    new SelCondition(){ Condition = "A.ABNORMALTYPEID=@A.ABNORMALTYPEID"}
                        //}, 
                        RelFields = new RelFieldCollection()
                        {
                            new RelField("ABNORMALNAME", LibDataType.NText,FieldSize.Size50,"异常名称"){ ControlType = LibControlType.NText}
                        },
                        SetValueFields = new SetValueFieldCollection()
                        {
                            new SetValueField("DEPTID"),
                            new SetValueField("DEPTNAME"),
                            new SetValueField("ABNORMALTYPEID"),
                            new SetValueField("ABNORMALTYPENAME")
                        }
                    }
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "DEPTID", "负责部门", FieldSize.Size20)
            {
                #region 异常类别
                //AllowEmpty = false,
                ReadOnly = true,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.Dept")
                    {
                         RelFields = new RelFieldCollection()
                         { 
                             new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"负责部门名称")
                         } 
                    }
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "ABNORMALTYPEID", "异常类别", FieldSize.Size20)
            {
                #region 异常类别
                //AllowEmpty = false,
                ReadOnly = true,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.AbnormalType")
                    {
                         RelFields = new RelFieldCollection()
                         { 
                             new RelField("ABNORMALTYPENAME", LibDataType.NText,FieldSize.Size50,"异常类别名称")
                         } 
                    }
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "ABNORMALDESC", "异常描述", FieldSize.Size1000) { ControlType = LibControlType.NText, ColumnSpan = 3, RowSpan = 4 });
            DataSourceHelper.AddColumn(new DefineField(headTable, "PERSONID", "负责人", FieldSize.Size20)
            {
                #region 人员
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() 
                { 
                    new RelativeSource("com.Person")
                    {
                        RelFields = new RelFieldCollection()
                        {
                          new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"负责人名称"),
                          new RelField("DEPTID", LibDataType.NText,FieldSize.Size20,"负责人部门","DESTDEPTID"){ ControlType = LibControlType.IdName },
                          new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"负责人部门名称","DESTDEPTNAME"),
                          new RelField("PHONENO",LibDataType.Text,FieldSize.Size20,"负责人手机","DESTPHONENO"),
                          new RelField("WECHAT", LibDataType.NText,FieldSize.Size50,"负责人微信","DESTWECHAT"),
                          new RelField("MAIL",LibDataType.Text,FieldSize.Size20,"负责人邮箱","DESTMAILNO")
                        }
                    }
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "FROMPERSONID", "报告人", FieldSize.Size20)
            {
                #region 人员
                AllowEmpty = false,
                DataType = LibDataType.Text,
                ControlType = LibControlType.NText,
                RelativeSource = new RelativeSourceCollection() 
                { 
                    new RelativeSource("com.Person")
                    {
                         RelFields = new RelFieldCollection()
                        {
                          new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"报告人名称","FROMPERSONNAME"),
                          new RelField("DEPTID", LibDataType.NText,FieldSize.Size50,"报告人部门","FROMDEPTID"){ ControlType = LibControlType.IdName },
                          new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"报告人部门名称","FROMDEPTNAME"),
                          new RelField("PHONENO",LibDataType.Text,FieldSize.Size20,"报告人手机","FROMPHONENO"),
                          new RelField("WECHAT", LibDataType.NText,FieldSize.Size50,"报告人微信","FROMWECHAT"),
                          new RelField("MAIL",LibDataType.Text,FieldSize.Size20,"报告人邮箱","FROMMAILNO")

                       }  
                    }
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "AFFECTPRODUCESTATE", "影响生产") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, TextOption = new string[] { "没有影响", "影响", "严重影响" } });
            DataSourceHelper.AddColumn(new DefineField(headTable, "AFFECTTIME", "影响工时") { DataType = LibDataType.Double, ControlType = LibControlType.Double, QtyLimit = LibQtyLimit.GreaterOrEqualThanZero });
            DataSourceHelper.AddColumn(new DefineField(headTable, "TIMEUNIT", "时间单位") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, TextOption = new string[] { "小时", "分钟" } });
            DataSourceHelper.AddColumn(new DefineField(headTable, "AFFECTPERSONNUM", "影响人数") { DataType = LibDataType.Numeric, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterOrEqualThanZero, DefaultValue = 1 });

            DataSourceHelper.AddColumn(new DefineField(headTable, "STARTTIME", "异常开始时间") { DataType = LibDataType.Int64, ControlType = LibControlType.DateTime });
            DataSourceHelper.AddColumn(new DefineField(headTable, "ENDTIME", "异常结束时间") { DataType = LibDataType.Int64, ControlType = LibControlType.DateTime });
            DataSourceHelper.AddColumn(new DefineField(headTable, "ISSYSTEMBUILD", "系统创建") { DataType = LibDataType.Boolean, AllowCopy = false, ControlType = LibControlType.YesNo, ReadOnly = true });

            DataSourceHelper.AddColumn(new DefineField(headTable, "DEALWITHSTATE", "处理状态") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, AllowCopy = false, TextOption = new string[] { "未处理", "处理中", "已处理", "已拒绝" }, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(headTable, "FROMMARK", "来源标识", FieldSize.Size100) { DataType = LibDataType.Text, AllowCopy = false, ControlType = LibControlType.Text, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(headTable, "TRANSMITLEVEL", "传递层级") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true, AllowCopy = false });

            DataSourceHelper.AddFixColumn(headTable, this.BillType);
            headTable.PrimaryKey = new DataColumn[] { headTable.Columns[primaryName] };
            this.DataSet.Tables.Add(headTable);
        }

        ///<summary>
        ///异常报告单 页面排版模型
        ///</summary>
        protected override void DefineViewTemplate(DataSet dataSet)
        {
            base.DefineViewTemplate(dataSet);
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "BILLNO", "TYPEID", "ISREPULSE", "BILLDATE", "ABNORMALPROTOTYPE", "ABNORMALID", "ABNORMALTYPEID", "DEPTID",
                "PERSONID", "DESTDEPTID","DESTPHONENO","DESTWECHAT", "FROMPERSONID", "FROMDEPTID","FROMPHONENO","FROMWECHAT", "ABNORMALDESC", "AFFECTPRODUCESTATE", "AFFECTTIME", "TIMEUNIT", "AFFECTPERSONNUM", "STARTTIME", "ENDTIME", "DEALWITHSTATE", "TRANSMITLEVEL","ISSYSTEMBUILD" });
            layout.ButtonRange = layout.BuildButton(new List<FunButton>() { new FunButton("btnBuildTrace", "生成异常追踪单") });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }

        /// <summary>
        /// 异常报告单 功能许可定义--设定入口参数
        /// </summary>
        protected override void DefineFuncPermission()
        {
            base.DefineFuncPermission();
            this.FuncPermission = new LibFuncPermission("", this.BillType);
            this.FuncPermission.BillTypeName = string.Format("{0}Type", this.ProgId);
            this.FuncPermission.EntryParam.Add("TYPEID");
        }

    }
}
