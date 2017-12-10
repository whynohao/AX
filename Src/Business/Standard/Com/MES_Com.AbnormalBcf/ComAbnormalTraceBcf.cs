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
using AxCRL.Core.Cache;
using AxCRL.Core.SysNews;
using AxCRL.Core.Comm;
using AxCRL.Comm.Service;
using System.Threading;

namespace MES_Com.AbnormalBcf
{
    /// <summary>
    /// 异常追踪单
    /// </summary>
    [ProgId(ProgId = "com.AbnormalTrace", ProgIdType = ProgIdType.Bcf, VclPath = @"/Scripts/module/mes/com/comAbnormalTraceVcl.js")]
    public class ComAbnormalTraceBcf : LibBcfData
    {
        /// <summary>
        /// 异常追踪单 注册模板
        /// </summary>
        /// <returns>返回 异常追踪单 的数据模板</returns>
        protected override LibTemplate RegisterTemplate()
        {
            return new ComAbnormalTraceBcfTemplate("com.AbnormalTrace");
        }

        /// <summary>
        /// 保存前校验 确定异常追踪单单据中的解决措施是否必填和处理状态是否可拒绝
        /// </summary>
        protected override void BeforeUpdate()
        {
            base.BeforeUpdate();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            //异常追踪单 处理状态[ "未处理", "处理中", "已处理", "已拒绝"]
            int dealwithState = LibSysUtils.ToInt32(masterRow["DEALWITHSTATE"]);
            if (dealwithState == 2)
            {
                //获得异常追踪单单据类型的字段“解决措施为必填”的值以确定异常追踪单单据中的解决措施是否必填
                bool needSolution = (bool)LibParamCache.Default.GetValueByName("com.AbnormalTraceType", new object[] { masterRow["TYPEID"] }, "NEEDSOLUTION");
                if (needSolution)
                {
                    string solution = LibSysUtils.ToString(masterRow["SOLUTION"]).Trim();
                    if (string.IsNullOrEmpty(solution))
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "解决措施不能为空。");
                    }
                }
            }
            else if (dealwithState == 3)
            {
                //获得异常报告单单据类型的字段“可拒绝”的值以确定异常追踪单单据中的处理状态是否可拒绝
                bool isRepulse = (bool)LibParamCache.Default.GetValueByName("com.AbnormalReportType", new object[] { masterRow["BILLTYPEID"] }, "ISREPULSE");
                if (!isRepulse)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "该异常报告的处理状态不能为已拒绝");
                }
            }
        }

        /// <summary>
        /// 确认异常报告单是否已结案处理，若已结案，则返回false，否则返回true
        /// </summary>
        /// <param name="fromBillNo">异常报告单 单据编号</param>
        /// <returns>若异常报告单已结案，则返回false，否则返回true</returns>
        private bool CheckFromBillEndCase(string fromBillNo)
        {
            bool ret = true;
            string sql = string.Format("select BILLNO from COMABNORMALREPORT where BILLNO={0} and CURRENTSTATE=4", LibStringBuilder.GetQuotString(fromBillNo));
            if (!string.IsNullOrEmpty(LibSysUtils.ToString(this.DataAccess.ExecuteScalar(sql, false))))
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "此操作将更新来源异常报告单的处理状态，但异常报告单已结案，所以无法做此操作。");
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// 数据保存后 更新异常报告单的处理状态和异常结束时间
        /// </summary>
        protected override void AfterCommintData()
        {
            base.AfterCommintData();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            // 单据是否被删除，若是则更新异常报告单的处理状态为未处理状态；否则针对过账情况，更新异常报告单的处理状态
            if (this.BillAction != AxCRL.Bcf.BillAction.Delete)
            {
                //保存异常追踪单无异常错误时，执行以下操作
                if (!this.ManagerMessage.IsThrow)
                {
                    //异常追踪单进行单据过账，查看其过账模式
                    PostAccountWay postWay = PostAccountHelper.GetPostAccountWay(PostAccountState.Release, masterRow);
                    switch (postWay)
                    {
                        case PostAccountWay.Diff:
                            // 差异过账--当单据修改后，更新异常报告单的处理状态
                            if (masterRow.HasVersion(DataRowVersion.Original))
                            {
                                //当来源单变更时，则将原始的异常报告单的处理状态为未处理状态，然后将当前的异常报告单的处理状态为当前异常追踪单的处理状态
                                if (LibSysUtils.ToString(masterRow["FROMBILLNO"]).CompareTo(LibSysUtils.ToString(masterRow["FROMBILLNO", DataRowVersion.Original])) != 0)
                                {
                                    if (CheckFromBillEndCase(LibSysUtils.ToString(masterRow["BILLNO", DataRowVersion.Original])))
                                    {
                                        UpdateFromBill(LibSysUtils.ToString(masterRow["FROMBILLNO", DataRowVersion.Original]), 0);
                                    }
                                    if (CheckFromBillEndCase(LibSysUtils.ToString(masterRow["BILLNO"])))
                                    {
                                        UpdateFromBill(LibSysUtils.ToString(masterRow["FROMBILLNO"]), LibSysUtils.ToInt32(masterRow["DEALWITHSTATE"]));
                                    }
                                }
                                else if (LibSysUtils.ToInt32(masterRow["DEALWITHSTATE"]) != LibSysUtils.ToInt32(masterRow["DEALWITHSTATE", DataRowVersion.Original]))
                                {
                                    if (CheckFromBillEndCase(LibSysUtils.ToString(masterRow["BILLNO"])))
                                    {
                                        UpdateFromBill(LibSysUtils.ToString(masterRow["FROMBILLNO"]), LibSysUtils.ToInt32(masterRow["DEALWITHSTATE"]));
                                    }
                                }
                                //异常追踪单 处理状态 当前值
                                int dealwithState = LibSysUtils.ToInt32(masterRow["DEALWITHSTATE"]);
                                //异常追踪单 处理状态 原始值
                                int oldDealwithState = LibSysUtils.ToInt32(masterRow["DEALWITHSTATE", DataRowVersion.Original]);
                                //若异常追踪单 处理状态为已处理或已拒绝时，发送处理完成报告给相关人员
                                if (dealwithState != oldDealwithState && (dealwithState == 2 || dealwithState == 3))
                                {
                                    SendFinishProcessMsg(masterRow, dealwithState);
                                }
                                else if ((dealwithState == 0 || dealwithState == 1))
                                {
                                    SendProcessMsg(masterRow);
                                }
                            }
                            break;
                        case PostAccountWay.Positive:
                            // 正过账--更新异常报告单的处理状态为当前异常追踪单的处理状态
                            if (CheckFromBillEndCase(LibSysUtils.ToString(masterRow["BILLNO"])))
                            {
                                UpdateFromBill(LibSysUtils.ToString(masterRow["FROMBILLNO"]), LibSysUtils.ToInt32(masterRow["DEALWITHSTATE"]));
                            }
                            // 正过账--发送系统信息、短信消息
                            SendProcessMsg(masterRow);
                            break;
                        case PostAccountWay.Reverse:
                            // 反过账--更新异常报告单的处理状态为未处理状态
                            if (CheckFromBillEndCase(LibSysUtils.ToString(masterRow["BILLNO"])))
                            {
                                UpdateFromBill(LibSysUtils.ToString(masterRow["FROMBILLNO"]), 0);
                            }
                            break;
                    }
                }
            }
            else
            {
                // 更新异常报告单的处理状态为未处理状态
                if (CheckFromBillEndCase(LibSysUtils.ToString(masterRow["BILLNO"])))
                {
                    UpdateFromBill(LibSysUtils.ToString(masterRow["FROMBILLNO"]), 0);
                }
            }
        }

        /// <summary>
        /// 更新异常报告单的处理状态和异常结束时间
        /// </summary>
        /// <param name="fromBillNo">异常报告单 单据编号</param>
        /// <param name="dealwithState">异常报告单 处理状态</param>
        private void UpdateFromBill(string fromBillNo, int dealwithState)
        {
            if (string.IsNullOrEmpty(fromBillNo))
                return;
            LibBcfData bcfData = LibBcfSystem.Default.GetBcfInstance("com.AbnormalReport") as LibBcfData;
            if (bcfData != null)
            {
                //异常报告单 数据模型
                DataSet ds = bcfData.Edit(new object[] { fromBillNo });
                DataRow fromRow = ds.Tables[0].Rows[0];
                //为异常报告单的处理状态赋值
                fromRow["DEALWITHSTATE"] = dealwithState;
                //若异常报告单的处理状态为已处理或已拒绝时，异常结束时间为当前系统时间
                if (dealwithState == 2 || dealwithState == 3)
                {
                    fromRow["ENDTIME"] = LibDateUtils.GetCurrentDateTime();
                }
                //保存异常报告单修改的数据
                bcfData.InnerSave(AxCRL.Bcf.BillAction.Modif, new object[] { fromBillNo }, ds);
                //保存异常报告单出现错误时，进行提示
                if (bcfData.ManagerMessage.IsThrow)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("过账异常报告单{0}出错，错误信息为:", fromBillNo));
                    foreach (LibMessage msg in bcfData.ManagerMessage.MessageList)
                    {
                        this.ManagerMessage.AddMessage(msg);
                    }
                }
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
        /// 获取异常追踪单 的主键数据集合和入口参数字段键值对集合
        /// </summary>
        /// <param name="masterRow">异常追踪单 主表行数据</param>
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
                        entryBuilder.AppendFormat("{0}:'{1}',", entryParam, masterRow[entryParam]);
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
        /// 获得当前异常追踪单下当前信息处理层级下的信息处理流程信息集
        /// </summary>
        /// <param name="masterRow">异常追踪单 主表行数据</param>
        /// <param name="processLevel">异常追踪单 处理层级</param>
        /// <returns>返回要处理信息的流程信息集</returns>
        private List<ProcessInfo> GetProcessInfo(DataRow masterRow, int processLevel)
        {
            //传输信息字典【使用条件--信息处理流程信息集】
            Dictionary<string, List<ProcessInfo>> processInfoDic = new Dictionary<string, List<ProcessInfo>>();
            //异常追踪单 单据类型
            string typeId = LibSysUtils.ToString(masterRow["TYPEID"]);
            //异常追踪单单据类型:com.AbnormalTraceType
            SqlBuilder sqlBuilder = new SqlBuilder(this.Template.FuncPermission.BillTypeName);
            //获取异常追踪单单据类型下的信息处理流程明细信息【使用条件，处理层级，接收人代码，接收人名称，微信，发微信，电话，发短信】
            string sql = sqlBuilder.GetQuerySql(1, "B.USECONDITION,C.PROCESSLEVEL,C.PERSONID,C.PERSONNAME,C.WECHAT,C.SENDWECHAT,C.PHONENO,C.NEEDSMS", string.Format("B.TYPEID={0} and C.PROCESSLEVEL={1}", LibStringBuilder.GetQuotString(typeId), processLevel), "B.USECONDITION,C.PROCESSLEVEL");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    //异常追踪单单据类型 信息处理流程规则配置 使用条件
                    string useCondition = LibSysUtils.ToString(reader["USECONDITION"]);
                    if (!processInfoDic.ContainsKey(useCondition))
                    {
                        processInfoDic.Add(useCondition, new List<ProcessInfo>());
                    }
                    ProcessInfo info = new ProcessInfo();
                    info.ProcessLevel = LibSysUtils.ToInt32(reader["PROCESSLEVEL"]);
                    info.PersonId = LibSysUtils.ToString(reader["PERSONID"]);
                    info.PersonName = LibSysUtils.ToString(reader["PERSONNAME"]);
                    info.WeChat = LibSysUtils.ToString(reader["WECHAT"]);
                    info.SendWeChat = LibSysUtils.ToBoolean(reader["SENDWECHAT"]);
                    info.PhoneNo = LibSysUtils.ToString(reader["PHONENO"]);
                    info.NeedSMS = LibSysUtils.ToBoolean(reader["NEEDSMS"]);
                    processInfoDic[useCondition].Add(info);
                }
            }
            //遍历字典找到跟异常追踪单的传递层级和使用条件一致的信息处理流程明细集
            List<ProcessInfo> curProcessInfo = null;
            foreach (var item in processInfoDic)
            {
                if (string.IsNullOrEmpty(item.Key))
                    continue;
                if (LibParseHelper.Parse(item.Key, new List<DataRow>() { masterRow }))
                {
                    curProcessInfo = item.Value;
                    break;
                }
            }
            //若未找到符合使用条件的信息处理流程明细集，则取默认的无条件的信息处理流程明细集
            if (curProcessInfo == null && processInfoDic.ContainsKey(string.Empty))
            {
                curProcessInfo = processInfoDic[string.Empty];
            }
            return curProcessInfo;
        }

        /// <summary>
        /// 获取不同层级下的信息处理流程中的接收人名称集合并赋值给抄送人
        /// </summary>
        /// <param name="destPersonName">抄送人名称</param>
        /// <param name="materRow">异常追踪单 主表行数据</param>
        /// <param name="processLevel">异常追踪单 处理层级</param>
        private void GetDealProcessPerson(ref string destPersonName, DataRow materRow, int processLevel)
        {
            //当信息的处理层级大于1时，将处理层级减去1
            if (processLevel > 1)
            {
                processLevel = --processLevel;
            }
            //遍历处理层级的各个层级，得到相应层级的信息处理流程集
            for (int i = 1; i <= processLevel; i++)
            {
                List<ProcessInfo> curProcessInfo = GetProcessInfo(materRow, i);
                if (curProcessInfo != null)
                {
                    //遍历当前处理层级的信息处理流程集
                    foreach (var info in curProcessInfo)
                    {
                        //若当前信息处理流程需要发送短信/发送微信且接收人名称存在且抄送人名称不包含接收人名称，则将接收人用‘,’连接到抄送人名称中
                        if ((info.NeedSMS || info.SendWeChat) && !string.IsNullOrEmpty(info.PersonName) && !destPersonName.Contains(info.PersonName))
                        {
                            destPersonName += info.PersonName + ",";
                        }
                    }
                }
            }
            //若抄送人名称不空，则去除接收人名称集合的最后连接点并返回给抄送人名称
            if (!string.IsNullOrEmpty(destPersonName))
            {
                destPersonName = destPersonName.Substring(0, destPersonName.LastIndexOf(','));
            }
        }

        /// <summary>
        /// 异常追踪单 发送系统处理消息、短信处理信息、微信处理信息
        /// </summary>
        /// <param name="masterRow">异常追踪单 主表行数据</param>
        private void SendProcessMsg(DataRow masterRow)
        {
            string data = GetMsgData(masterRow);
            //根据异常追踪单的单据编号获得异常追踪单的数据模型
            LibBcfData bcfData = (LibBcfData)LibBcfSystem.Default.GetBcfInstance("com.AbnormalTrace");
            DataSet ds = bcfData.BrowseTo(new object[] { masterRow["BILLNO"] });
            DataRow messageRow = ds.Tables[0].Rows[0];
            //系统消息类，用于发送系统消息,其属性用户列表中的用户不可重复
            LibSysNews news = new LibSysNews();
            //电话列表，用于发送短信，列表中手机号不可重复
            List<string> phoneList = new List<string>();
            //微信列表，用于发送微信，列表中微信号不可重复
            List<string> weChatList = new List<string>();

            #region 推送异常追踪处理信息所需字段
            //班组
            string workTeamName = string.Empty;
            //异常追踪单 单据编号
            string billNo = LibSysUtils.ToString(messageRow["BILLNO"]);
            //异常报告单 异常名称
            string abnormalName = LibSysUtils.ToString(messageRow["ABNORMALNAME"]);
            //异常报告单 异常描述
            string abnormalDesc = LibSysUtils.ToString(messageRow["ABNORMALDESC"]);
            //异常追踪单 处理人手机
            string dealwithPhoneNo = LibSysUtils.ToString(messageRow["DEALWITHPHONENO"]);
            //异常追踪单 处理人微信
            string dealwithWeChat = LibSysUtils.ToString(messageRow["DEALWITHWECHAT"]);
            //异常追踪单 责任人手机
            string destPhoneNo = LibSysUtils.ToString(messageRow["DESTPHONENO"]);
            //异常追踪单 责任人微信
            string destWeChat = LibSysUtils.ToString(messageRow["DESTWECHAT"]);
            //抄送人名称集合
            string destPersonName = string.Empty;
            //解决人名称集合 
            string dealwithPersonName = string.Empty;
            //异常报告单 处理层级+1为异常报告预警流程中的相关处理层级，当前应为第一层级
            int processLevel = LibSysUtils.ToInt32(messageRow["PROCESSLEVEL"]) + 1;
            #endregion

            #region 当初始推送业务处理信息时（处理层级为0），为异常追踪单中的的责任人、处理人发短信/发微信，并异常追踪单中的处理人发系统信息
            if (processLevel <= 1)
            {
                //将异常追踪单中的责任人手机（手机号存在）添加到电话列表中，以便发短信
                if (!string.IsNullOrEmpty(destPhoneNo))
                {
                    phoneList.Add(destPhoneNo);
                }
                //将异常追踪单中的处理人手机（手机号存在）添加到电话列表中，以便发短信
                if (!string.IsNullOrEmpty(dealwithPhoneNo))
                {
                    if (!phoneList.Contains(dealwithPhoneNo))
                        phoneList.Add(dealwithPhoneNo);
                }
                //将异常追踪单中的责任人微信（微信号存在，若不存在则将其手机号作为微信号）添加到微信列表中，以便发微信
                if (!string.IsNullOrEmpty(destWeChat))
                {
                    weChatList.Add(destWeChat);
                }
                else if (!string.IsNullOrEmpty(destPhoneNo))
                {
                    weChatList.Add(destPhoneNo);
                }
                //将异常追踪单中的处理人微信（微信号存在，若不存在则将其手机号作为微信号）添加到微信列表中，以便发微信
                if (!string.IsNullOrEmpty(dealwithWeChat))
                {
                    if (!weChatList.Contains(dealwithWeChat))
                        weChatList.Add(dealwithWeChat);
                }
                else if (!string.IsNullOrEmpty(dealwithPhoneNo))
                {
                    if (!weChatList.Contains(dealwithPhoneNo))
                        weChatList.Add(dealwithPhoneNo);
                }
                //将异常追踪单中的处理人添加到其用户列表中，以便发系统信息
                string personId = LibSysUtils.ToString(messageRow["DEALWITHPERSONID"]);
                if (!string.IsNullOrEmpty(personId))
                {
                    news.UserList.Add(personId);
                }
            }
            #endregion

            #region 获取第一层级的传递层级下的信息处理流程明细信息并为其接收人（根据需要）发短信/发微信、发系统信息、得到接收人名称集合
            //获得异常追踪单下的信息处理流程信息集
            List<ProcessInfo> curProcessInfo = GetProcessInfo(messageRow, processLevel);
            if (curProcessInfo != null)
            {
                //遍历信息处理流程信息集，为系统消息类的电话列表和用户列表赋值
                foreach (var info in curProcessInfo)
                {
                    //将当前传递层级下的接收人手机（需要接受人接收短信且手机号存在）添加到电话列表中，以便发短信
                    if (info.NeedSMS && !string.IsNullOrEmpty(info.PhoneNo) && !phoneList.Contains(info.PhoneNo))
                    {
                        phoneList.Add(info.PhoneNo);
                    }
                    //将当前传递层级下的接收人微信（需要接收人接受微信且微信号存在，若不存在则将其手机号作为微信号）添加到微信列表中，以便发微信
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
                    //当信息的传递层级大于1时，责任人名称就是接收人名称集合
                    if (processLevel > 1)
                    {
                        destPersonName += info.PersonName + ';';
                    }
                    //将当前传递层级下的接收人添加到其用户列表中，以便发系统信息
                    if (!news.UserList.Contains(info.PersonId))
                    {
                        news.UserList.Add(info.PersonId);
                    }
                }
            }
            #endregion

            #region 得到当前传递层级下的业务传递流程明细信息中的所有可以接收短信或微信的接收人（若不存在，则将处理人作为接收人），用于催促这些人员尽快解决问题
            GetDealProcessPerson(ref dealwithPersonName, masterRow, processLevel);
            if (string.IsNullOrEmpty(dealwithPersonName))
            {
                dealwithPersonName = LibSysUtils.ToString(messageRow["DEALWITHPERSONNAME"]);
            }
            #endregion

            #region 发送系统信息
            news.Title = string.Format("发生{0}异常", abnormalName);
            news.Content = string.Format("\n异常追踪单号：{3};{0}; \n需要 {1} 尽快填写处理措施; \n 抄送：{2}  ", abnormalDesc, FormatData(dealwithPersonName), FormatData(destPersonName), billNo);
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
                //ThreadPool.QueueUserWorkItem(LibSMSHelper.SendMsg, param);
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

            if (curProcessInfo != null)
            {
                //过账异常追踪单的传递信息层级
                this.DataAccess.ExecuteNonQuery(string.Format("update COMABNORMALTRACE set PROCESSLEVEL={0} where BILLNO={1}", processLevel, LibStringBuilder.GetQuotObject(masterRow["BILLNO"])));
            }
        }

        /// <summary>
        /// 当异常追踪单数据修正后 发送处理完成报告给相关人员
        /// </summary>
        /// <param name="masterRow">异常追踪单 主表行数据</param>
        /// <param name="dealwithState">异常追踪单 处理状态</param>
        private void SendFinishProcessMsg(DataRow masterRow, int dealwithState)
        {
            //异常追踪单 处理层级
            int processLevel = LibSysUtils.ToInt32(masterRow["PROCESSLEVEL"]);
            //异常追踪单 责任人
            string destPersonId = LibSysUtils.ToString(masterRow["PERSONID"]);
            //异常追踪单 责任人手机
            string destPhoneNo = LibSysUtils.ToString(masterRow["DESTPHONENO"]);
            //异常追踪单 责任人微信
            string destWeChat = LibSysUtils.ToString(masterRow["DESTWECHAT"]);
            //当前异常追踪单下当前信息处理层级下的信息处理流程信息集
            List<ProcessInfo> curProcessInfo = GetProcessInfo(masterRow, processLevel);
            //异常追踪单 存在责任人或当前信息处理流程信息集
            if (curProcessInfo != null || !string.IsNullOrEmpty(destPersonId))
            {
                //系统消息类，用于发送系统消息,其属性用户列表中的用户不可重复
                LibSysNews news = new LibSysNews();
                //电话列表，用于发送短信，列表中手机号不可重复
                List<string> phoneList = new List<string>();
                //微信列表，用于发送微信，列表中微信号不可重复
                List<string> weChatList = new List<string>();

                #region 遍历当前信息处理流程信息集，为其接收人（根据需要）发送短信/发送微信、发系统信息
                if (curProcessInfo != null)
                {
                    foreach (var info in curProcessInfo)
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

                #region 为责任人发送短信、发送微信、发系统信息
                //将责任人电话（手机号存在）添加到电话列表中，以便发短信
                if (!string.IsNullOrEmpty(destPhoneNo) && !phoneList.Contains(destPhoneNo))
                {
                    phoneList.Add(destPhoneNo);
                }
                //将责任人微信（微信号存在，若不存在则将其手机号作为微信号）添加到微信列表中，以便发微信
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
                //将责任人添加到人员列表中，以便发系统信息
                if (!string.IsNullOrEmpty(destPersonId) && !news.UserList.Contains(destPersonId))
                {
                    news.UserList.Add(destPersonId);
                }
                #endregion

                #region 推送异常完结消息所需字段
                //班组
                string workTeamName = string.Empty;
                //异常追踪单 单据编号
                string billNo = LibSysUtils.ToString(masterRow["BILLNO"]);
                //异常报告单 异常名称
                string abnormalName = LibSysUtils.ToString(masterRow["ABNORMALNAME"]);
                //异常报告单 异常描述
                string abnormalDesc = LibSysUtils.ToString(masterRow["ABNORMALDESC"]);
                //异常追踪单 处理人名称，作为处理人
                string dealwithPersonName = LibSysUtils.ToString(masterRow["DEALWITHPERSONNAME"]);
                //异常追踪单 计划完成时间
                string needTime = LibSysUtils.ToString(masterRow["PLANENDTIME"]);
                //异常追踪单 处理措施
                string solution = LibSysUtils.ToString(masterRow["SOLUTION"]);
                //异常追踪单 异常处理时间
                string strNeedTime = DateTime.Now.AddHours(2).ToString("MM/dd HH:mm");
                //异常处理时间为异常结束时间的月日时分
                if (needTime.Length >= 14)
                {
                    DateTime dateTime = new DateTime(int.Parse(needTime.Substring(0, 4)), int.Parse(needTime.Substring(4, 2)), int.Parse(needTime.Substring(6, 2)), int.Parse(needTime.Substring(8, 2)), int.Parse(needTime.Substring(10, 2)), int.Parse(needTime.Substring(12, 2)));
                    strNeedTime = dateTime.ToString("MM/dd HH:mm");
                }
                //异常追踪单 责任人名称，作为责任人
                string destPersonName = LibSysUtils.ToString(masterRow["PERSONNAME"]);
                string dealwithStateStr = dealwithState == 2 ? "已处理完成" : "已被拒绝处理";
                #endregion

                #region 发送系统消息
                news.Title = string.Format("异常追踪单{0}{1}", masterRow["BILLNO"], dealwithStateStr);
                news.Content = string.Format("\n 班组：{0}; \n异常类型：{1}; \n问题：{2}; \n处理人：{3}; \n要求解决时间：{4}; \n责任人：{5}; \n异常单号：{6}; \n处理状态：{7}; \n处理措施：{8}; ",
                    FormatData(workTeamName), abnormalName, abnormalDesc, FormatData(dealwithPersonName), FormatData(strNeedTime), FormatData(destPersonName), billNo, dealwithStateStr, solution);
                string data = GetMsgData(masterRow);
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
                    //ThreadPool.QueueUserWorkItem(LibSMSHelper.SendMsg, param);
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
        /// 为特殊处理人员发送处理信息
        /// </summary>
        /// <param name="list">特殊处理人员集合</param>
        /// <param name="billNo">异常追踪单单据编号</param>
        public void TransferProcessMsg(IList<ProcessInfo> list, string billNo)
        {
            if (list != null && list.Count > 0)
            {
                // 单据是否被删除，若是则更新异常报告单的处理状态为未处理状态；否则针对过账情况，更新异常报告单的处理状态
                if (this.BillAction != AxCRL.Bcf.BillAction.Delete)
                {
                    //保存异常追踪单无异常错误时，执行以下操作
                    if (!this.ManagerMessage.IsThrow)
                    {
                        //根据异常追踪单的单据编号获得异常追踪单的数据模型
                        LibBcfData bcfData = (LibBcfData)LibBcfSystem.Default.GetBcfInstance("com.AbnormalTrace");
                        DataSet ds = bcfData.BrowseTo(new object[] { billNo });
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            DataRow messageRow = ds.Tables[0].Rows[0];
                            string data = GetMsgData(messageRow);
                            //系统消息类，用于发送系统消息,其属性用户列表中的用户不可重复
                            LibSysNews news = new LibSysNews();
                            //电话列表，用于发送短信，列表中手机号不可重复
                            List<string> phoneList = new List<string>();
                            //微信列表，用于发送微信，列表中微信号不可重复
                            List<string> weChatList = new List<string>();

                            #region 推送异常追踪处理信息所需字段
                            //异常报告单 异常名称
                            string abnormalName = LibSysUtils.ToString(messageRow["ABNORMALNAME"]);
                            //异常报告单 异常描述
                            string abnormalDesc = LibSysUtils.ToString(messageRow["ABNORMALDESC"]);
                            //解决人名称集合 
                            string dealwithPersonName = LibSysUtils.ToString(messageRow["DEALWITHPERSONNAME"]);
                            #endregion

                            #region 为特殊处理人（根据需要）发短信/发微信、发系统信息
                            foreach (var info in list)
                            {
                                //将特殊处理人手机（需要接受人接收短信且手机号存在）添加到电话列表中，以便发短信
                                if (info.NeedSMS && !string.IsNullOrEmpty(info.PhoneNo) && !phoneList.Contains(info.PhoneNo))
                                {
                                    phoneList.Add(info.PhoneNo);
                                }
                                //将特殊处理人微信（需要特殊处理人接收微信且微信号存在，若不存在则将其手机号作为微信号）添加到微信列表中，以便发微信
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
                                //将特殊处理人添加到其用户列表中，以便发系统信息
                                if (!news.UserList.Contains(info.PersonId))
                                {
                                    news.UserList.Add(info.PersonId);
                                }
                            }
                            #endregion

                            #region 发送系统信息
                            news.Title = string.Format("发生{0}异常", abnormalName);
                            news.Content = string.Format("\n异常追踪单号：{2};{0}; \n需要 {1} 尽快填写处理措施;  ", FormatData(abnormalDesc), FormatData(dealwithPersonName), billNo);
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
                                //ThreadPool.QueueUserWorkItem(LibSMSHelper.SendMsg, param);
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
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 信息处理流程明细
        /// </summary>
        public class ProcessInfo
        {
            private int _processLevel;
            private string _PersonId;
            private bool _NeedSMS;
            private bool _SendWeChat;
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
            /// 处理层级
            /// </summary>
            public int ProcessLevel
            {
                get { return _processLevel; }
                set { _processLevel = value; }
            }
        }
    }

    /// <summary>
    /// 异常追踪单 数据模板
    /// </summary>
    public class ComAbnormalTraceBcfTemplate : LibTemplate
    {
        // 异常追踪单 主表
        private const string tableName = "COMABNORMALTRACE";
        // 异常追踪单 子表 处理意见明细
        private const string bodyTableName = "COMABNORMALTRACEDETAIL";

        /// <summary>
        /// 异常追踪单 模板功能定义
        /// </summary>
        /// <param name="progId">异常追踪单 功能标识</param>
        public ComAbnormalTraceBcfTemplate(string progId)
            : base(progId, BillType.Bill, "异常追踪单")
        {
        }

        /// <summary>
        /// 异常追踪单 数据模型
        /// </summary>
        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            #region 异常追踪单 主表
            string primaryName = "BILLNO";
            DataTable headTable = new DataTable(tableName);
            DataSourceHelper.AddColumn(new DefineField(headTable, primaryName, "单据编号", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false, DataType = LibDataType.Text });
            DataSourceHelper.AddColumn(new DefineField(headTable, "TYPEID", "单据类型", FieldSize.Size50)
            {
                #region 异常追踪单单据类型
                AllowEmpty = false,
                DataType = LibDataType.Text,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.AbnormalTraceType")
                    {
                        RelFields = new RelFieldCollection()
                        {
                            new RelField("TYPENAME", LibDataType.NText,FieldSize.Size20,"单据类型名称")
                        }
                    }
                }
                #endregion
            });
            DataSourceHelper.AddBillDate(headTable);
            DataSourceHelper.AddColumn(new DefineField(headTable, "FROMBILLNO", "异常报告单号", FieldSize.Size20)
            {
                #region 异常报告单
                AllowEmpty = false,
                RelativeSource = new RelativeSourceCollection() 
                { 
                   new RelativeSource("com.AbnormalReport")
                   {  
                       SelConditions = new SelConditionCollection()
                       {
                            new SelCondition (){ Condition="A.CURRENTSTATE=2", DisplayText = "", MsgCode = "", MsgParam = ""},
                            new SelCondition(){ Condition="Not Exists[com.AbnormalTrace,0,B.FROMBILLNO=A.BILLNO]"}
                        } ,
                        RelFields = new RelFieldCollection()
                        {
                            new RelField("ABNORMALID",LibDataType.Text,FieldSize.Size20,"异常"){ ControlType = LibControlType.IdName },
                            new RelField("ABNORMALNAME",LibDataType.NText,FieldSize.Size50,"异常名称"),
                            new RelField("ABNORMALTYPEID",LibDataType.Text,FieldSize.Size20,"异常类别"){ ControlType = LibControlType.IdName },
                            new RelField("ABNORMALTYPENAME",LibDataType.NText,FieldSize.Size50,"异常类别名称"),
                            new RelField("ABNORMALDESC",LibDataType.NText,FieldSize.Size1000,"异常描述"){ ColumnSpan =4, RowSpan =3},
                            new RelField("FROMPERSONID",LibDataType.Text,FieldSize.Size20,"报告人"){ ControlType = LibControlType.IdName },
                            new RelField("FROMPERSONNAME",LibDataType.NText,FieldSize.Size50,"报告人名称"),
                            new RelField("FROMDEPTID",LibDataType.Text,FieldSize.Size20,"报告人部门"){ ControlType = LibControlType.IdName },
                            new RelField("FROMDEPTNAME",LibDataType.NText,FieldSize.Size50,"报告人部门名称"),
                            new RelField("FROMPHONENO",LibDataType.Text,FieldSize.Size20,"报告人手机"),
                            new RelField("FROMWECHAT", LibDataType.NText,FieldSize.Size50,"报告人微信"),
                            new RelField("AFFECTPRODUCESTATE",LibDataType.Int32,0,"影响生产"){ControlType = LibControlType.TextOption,TextOption=new string[]{ "没有影响", "影响", "严重影响"}},
                            new RelField("TYPEID",LibDataType.Text,FieldSize.Size50,"单据类型","BILLTYPEID"){ControlType=LibControlType.IdName},
                            new RelField("TYPENAME", LibDataType.NText, FieldSize.Size20, "单据类型名称","BILLTYPENAME")
                        },
                        SetValueFields = new SetValueFieldCollection()
                        {
                            new SetValueField("PERSONID"),
                            new SetValueField("PERSONNAME"),
                            new SetValueField("DESTDEPTID"),
                            new SetValueField("DESTDEPTNAME"),
                            new SetValueField("DESTWECHAT"),
                            new SetValueField("DESTPHONENO")
                        } 
                   }
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "PLANENDTIME", "计划完成时间") { DataType = LibDataType.Int64, ControlType = LibControlType.DateTime, AllowEmpty = false, QtyLimit = LibQtyLimit.GreaterThanZero });
            DataSourceHelper.AddColumn(new DefineField(headTable, "ABNORMALREASONID", "异常原因", FieldSize.Size20)
            {
                #region 异常原因
                AllowEmpty = true,
                DataType = LibDataType.Text,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.AbnormalReason")
                    {
                        RelFields = new RelFieldCollection()
                        {
                            new RelField("ABNORMALREASONNAME", LibDataType.NText,FieldSize.Size50,"异常原因名称"),
                            new RelField("ABNORMALREASONTYPEID", LibDataType.Text,FieldSize.Size20,"异常原因类型"){ ControlType = LibControlType.IdName },
                            new RelField("ABNORMALREASONTYPENAME", LibDataType.NText,FieldSize.Size50,"异常原因类型名称")
                        } 
                    }
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "PERSONID", "责任人员", FieldSize.Size20)
            {
                #region 人员
                DataType = LibDataType.Text,
                ControlType = LibControlType.NText,
                RelativeSource = new RelativeSourceCollection() 
                { 
                    new RelativeSource("com.Person")
                    {
                        RelFields = new RelFieldCollection()
                        {
                            new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"责任人员名称"),
                            new RelField("DEPTID", LibDataType.Text,FieldSize.Size20,"责任部门","DESTDEPTID"){ ControlType = LibControlType.IdName},
                            new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"责任部门名称","DESTDEPTNAME"),
                            new RelField("PHONENO",LibDataType.Text,FieldSize.Size20,"责任人手机","DESTPHONENO"),
                            new RelField("WECHAT", LibDataType.NText,FieldSize.Size50,"责任人微信","DESTWECHAT"),
                            new RelField("MAIL",LibDataType.Text,FieldSize.Size20,"责任人邮箱","DESTMAILNO")
                        }  
                    }
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "DEALWITHPERSONID", "处理人员", FieldSize.Size20)
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
                            new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"处理人员名称","DEALWITHPERSONNAME"),
                            new RelField("DEPTID", LibDataType.Text,FieldSize.Size20,"处理部门","DEALWITHDEPTID"){ ControlType = LibControlType.IdName},
                            new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"处理部门名称","DEALWITHDEPTNAME"),
                            new RelField("PHONENO",LibDataType.Text,FieldSize.Size20,"负责人手机","DEALWITHPHONENO"),
                            new RelField("WECHAT", LibDataType.NText,FieldSize.Size50,"负责人微信","DEALWITHWECHAT"),
                            new RelField("MAIL",LibDataType.Text,FieldSize.Size20,"负责人邮箱","DEALWITHMAILNO")
                        }  
                    }
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "SOLUTION", "解决措施", FieldSize.Size500) { ColumnSpan = 3, RowSpan = 3 });
            DataSourceHelper.AddColumn(new DefineField(headTable, "DEALWITHSTATE", "处理状态") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, DefaultValue = 1, TextOption = new string[] { "未处理", "处理中", "已处理", "已拒绝" } });
            DataSourceHelper.AddColumn(new DefineField(headTable, "PROCESSLEVEL", "处理层级") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true, AllowCopy = false });
            DataSourceHelper.AddFixColumn(headTable, this.BillType);
            headTable.ExtendedProperties.Add(TableProperty.DBIndex, new DBIndexCollection() { new DBIndex("ABNORMAL_FROMBILLNO", new DBIndexFieldCollection() { new DBIndexField("FROMBILLNO") }, true) });
            headTable.PrimaryKey = new DataColumn[] { headTable.Columns[primaryName] };
            this.DataSet.Tables.Add(headTable);
            #endregion

            #region 异常追踪单 子表 处理意见明细
            DataTable bodyTable = new DataTable(bodyTableName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, primaryName, "单据编号", FieldSize.Size20) { DataType = LibDataType.Text, AllowEmpty = false, AllowCopy = false, ReadOnly = true });
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "PERSONID", "处理人员", FieldSize.Size20)
            {
                #region 人员
                AllowEmpty = false,
                ReadOnly = true,
                DataType = LibDataType.Text,
                ControlType = LibControlType.NText,
                RelativeSource = new RelativeSourceCollection() 
                { 
                    new RelativeSource("com.Person")
                    {
                        RelFields = new RelFieldCollection()
                        {
                            new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"处理人员名称"),
                            new RelField("DEPTID", LibDataType.Text,FieldSize.Size20,"处理部门"){ ControlType = LibControlType.IdName},
                            new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"处理部门名称")
                        }  
                    }
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "DEALWITHOPINION", "处理意见", FieldSize.Size500) { ColumnSpan = 4, RowSpan = 4, AllowEmpty = false, ReadOnly = true });
            DataSourceHelper.AddRemark(bodyTable);
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", tableName, bodyTableName), headTable.Columns[primaryName], bodyTable.Columns[primaryName]);
            #endregion
        }

        ///<summary>
        ///异常追踪单 页面排版模型
        ///</summary>
        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "BILLNO", "TYPEID", "BILLDATE", "FROMBILLNO", "ABNORMALID", "ABNORMALTYPEID",
                "AFFECTPRODUCESTATE", "BILLTYPEID", "FROMPERSONID", "FROMDEPTID","FROMPHONENO","FROMWECHAT",  "ABNORMALDESC", "PLANENDTIME", "ABNORMALREASONID", "ABNORMALREASONTYPEID", 
                "PERSONID", "DESTDEPTID","DESTPHONENO","DESTWECHAT", "DEALWITHPERSONID", "DEALWITHDEPTID","DEALWITHPHONENO","DEALWITHWECHAT", "SOLUTION", "DEALWITHSTATE", "PROCESSLEVEL" });
            layout.TabRange.Add(layout.BuildGrid(1, "处理意见明细"));
            layout.ButtonRange = layout.BuildButton(new List<FunButton>() { new FunButton("btnBuildOpinion", "添加处理意见") });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }

        /// <summary>
        /// 异常追踪单 功能许可定义--设定入口参数
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
