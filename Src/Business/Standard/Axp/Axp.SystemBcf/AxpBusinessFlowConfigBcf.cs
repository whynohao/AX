using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Service;
using AxCRL.Comm.Utils;
using AxCRL.Core.Comm;
using AxCRL.Core.SysNews;
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
using System.Threading;
using System.Threading.Tasks;

namespace Axp.SystemBcf
{
    /// <summary>
    /// 单据业务流配置
    /// </summary>
    [ProgId(ProgId = "axp.BusinessFlowConfig", ProgIdType = ProgIdType.Bcf, VclPath = @"/Scripts/module/mes/Axp/axpBusinessFlowConfigVcl.js")]
    public class AxpBusinessFlowConfigBcf : LibBcfData
    {
        /// <summary>
        /// 单据业务流配置 模板注册
        /// </summary>
        /// <returns>返回 单据业务流配置 的数据模板</returns>
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpBusinessFlowConfigBcfTemplate("axp.BusinessFlowConfig");
        }

        /// <summary>
        /// 单据业务流配置 保存前验证【不能出现相同的功能配置；不能出现相同的使用条件;相同的使用条件中不能出现相同的人；发短信或发微信时，接收人的电话或微信须存在】
        /// </summary>
        protected override void BeforeUpdate()
        {
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            // 不能出现相同的功能配置
            string sql = string.Format("select PROGID from AXPBUSINESSFLOWCONFIG where BUSINESSFLOWCONFIGID<>{0} and PROGID={1}", LibStringBuilder.GetQuotObject(masterRow["BUSINESSFLOWCONFIGID"]), LibStringBuilder.GetQuotObject(masterRow["PROGID"]));
            if (!string.IsNullOrEmpty(LibSysUtils.ToString(this.DataAccess.ExecuteScalar(sql))))
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前功能已存在单据业务流配置主数据，不能重复建立");
            }
            //不能出现相同的使用条件;相同的使用条件中不能出现相同的人；发短信或发微信时，接收人的电话或微信须存在
            List<string> conditionList = new List<string>();
            DataTable subTable = this.DataSet.Tables[1];
            foreach (DataRow curRow in subTable.Rows)
            {
                if (curRow.RowState == DataRowState.Deleted)
                    continue;
                //不能出现相同的使用条件
                string useCondition = LibSysUtils.ToString(curRow["USECONDITION"]);
                if (conditionList.Contains(useCondition))
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("单据业务流配置规则中行标识{0}的使用条件存在重复", LibSysUtils.ToInt32(curRow["ROW_ID"])));
                }
                else
                {
                    conditionList.Add(useCondition);
                }

                #region 相同的使用条件中不能出现相同的人；发短信或发微信时，接收人的电话或微信须存在
                List<string> personList = new List<string>();
                DataRow[] childRows = curRow.GetChildRows(this.DataSet.Relations[1]);
                if (childRows != null)
                {
                    foreach (DataRow childRow in childRows)
                    {
                        if (childRow.RowState == DataRowState.Deleted)
                            continue;
                        //相同的使用条件中不能出现相同的人
                        string personId = LibSysUtils.ToString(childRow["PERSONID"]);
                        if (personList.Contains(personId))
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("单据业务流配置规则中行标识{0}下业务流配置明细中行标识{1}的接收人存在重复",
                               LibSysUtils.ToInt32(childRow["PARENTROWID"]), LibSysUtils.ToInt32(childRow["ROW_ID"])));
                        }
                        else
                        {
                            personList.Add(personId);
                        }
                        //发短信或发微信时，接收人的电话或微信须存在
                        string phoneNo = LibSysUtils.ToString(childRow["PHONENO"]);
                        string weChat = LibSysUtils.ToString(childRow["WECHAT"]);
                        bool needSms = LibSysUtils.ToBoolean(childRow["NEEDSMS"]);
                        bool sendWeChat = LibSysUtils.ToBoolean(childRow["SENDWECHAT"]);
                        if (string.IsNullOrWhiteSpace(weChat) && sendWeChat)
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("单据业务流配置规则中行标识{0}下业务流配置明细中行标识{1}的接收人的微信不存在，无法发微信",
                                LibSysUtils.ToInt32(childRow["PARENTROWID"]), LibSysUtils.ToInt32(childRow["ROW_ID"])));
                        }
                        if (string.IsNullOrWhiteSpace(phoneNo) && needSms)
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("单据业务流配置规则中行标识{0}下业务流配置明细中行标识{1}的接收人的电话不存在，无法发短信",
                                LibSysUtils.ToInt32(childRow["PARENTROWID"]), LibSysUtils.ToInt32(childRow["ROW_ID"])));
                        }
                    }
                }
                #endregion
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
        /// 获取功能单据 的主键数据集合和入口参数字段键值对集合
        /// </summary>
        /// <param name="masterRow">功能单据 主表行数据</param>
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
        /// 获得单据业务流配置下跟当前功能单据数据相匹配的业务流配置明细
        /// </summary>
        /// <param name="billMasterRow">功能单据 主表行数据</param>
        /// <param name="billProgId">功能单据 功能标识</param>
        /// <returns>返回业务流配置明细</returns>
        private List<BusinessInfo> GetBusinessInfo(DataRow billMasterRow, string billProgId)
        {
            //传输信息字典【使用条件--业务流配置明细】
            Dictionary<string, List<BusinessInfo>> businessInfoDic = new Dictionary<string, List<BusinessInfo>>();
            //单据业务流配置:axp.BusinessFlowConfig
            SqlBuilder sqlBuilder = new SqlBuilder(this.ProgId);
            //获取单据业务流配置下当前功能单据的业务流配置明细【使用条件，接收人代码，接收人名称，微信，发微信，电话，发短信】
            string sql = sqlBuilder.GetQuerySql(0, "B.USECONDITION,C.PERSONID,C.PERSONNAME,C.WECHAT,C.SENDWECHAT,C.PHONENO,C.NEEDSMS",
                string.Format("A.PROGID={0}", LibStringBuilder.GetQuotString(billProgId)), "B.USECONDITION");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    //单据业务流配置下当前功能单据 单据业务流配置规则配置 使用条件
                    string useCondition = LibSysUtils.ToString(reader["USECONDITION"]);
                    if (!businessInfoDic.ContainsKey(useCondition))
                    {
                        businessInfoDic.Add(useCondition, new List<BusinessInfo>());
                    }
                    BusinessInfo info = new BusinessInfo();
                    info.PersonId = LibSysUtils.ToString(reader["PERSONID"]);
                    info.PersonName = LibSysUtils.ToString(reader["PERSONNAME"]);
                    info.WeChat = LibSysUtils.ToString(reader["WECHAT"]);
                    info.SendWeChat = LibSysUtils.ToBoolean(reader["SENDWECHAT"]);
                    info.PhoneNo = LibSysUtils.ToString(reader["PHONENO"]);
                    info.NeedSMS = LibSysUtils.ToBoolean(reader["NEEDSMS"]);
                    businessInfoDic[useCondition].Add(info);
                }
            }
            //遍历字典找到使用条件跟当前功能单据数据相匹配的业务流配置明细
            List<BusinessInfo> curBusinessInfo = null;
            foreach (var item in businessInfoDic)
            {
                if (string.IsNullOrEmpty(item.Key))
                    continue;
                if (LibParseHelper.Parse(item.Key, new List<DataRow>() { billMasterRow }))
                {
                    curBusinessInfo = item.Value;
                    break;
                }
            }
            //若未找到符合当前功能单据数据相匹配的业务流配置明细，则取默认的无条件的业务流配置明细
            if (curBusinessInfo == null && businessInfoDic.ContainsKey(string.Empty))
            {
                curBusinessInfo = businessInfoDic[string.Empty];
            }
            return curBusinessInfo;
        }

        /// <summary>
        /// 功能单据 发送系统处理消息、短信处理信息、微信处理信息
        /// </summary>
        /// <param name="billMasterRow">功能单据 主表行数据</param>
        /// <param name="billProgId">功能单据 功能标识</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <param name="personIds">额外发送人员，可以为空</param>
        public void SendBusinessMsg(DataRow billMasterRow, string billProgId, string title, string content, List<string> personIds)
        {
            string data = GetMsgData(billMasterRow);

            //系统消息类，用于发送系统消息,其属性用户列表中的用户不可重复
            LibSysNews news = new LibSysNews();
            //电话列表，用于发送短信，列表中手机号不可重复
            List<string> phoneList = new List<string>();
            //微信列表，用于发送微信，列表中微信号不可重复
            List<string> weChatList = new List<string>();

            #region 为额外需要发送信息的人发短信、发微信、发系统信息
            if (personIds != null && personIds.Count > 0)
            {
                //获取人员信息【人员代码，微信，电话】
                string sql = "SELECT PERSONID,WECHAT,PHONENO FROM COMPERSON WHERE PERSONID in(";
                foreach (string personId in personIds)
                {
                    sql = sql + string.Format("{0},", LibStringBuilder.GetQuotString(personId));
                }
                sql = sql.TrimEnd(',') + ")";

                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        //将人员手机（手机号存在）添加到电话列表中，以便发短信
                        string phoneNo = LibSysUtils.ToString(reader["PHONENO"]);
                        if (!string.IsNullOrEmpty(phoneNo) && !phoneList.Contains(phoneNo))
                        {
                            phoneList.Add(phoneNo);
                        }
                        //将人员微信（微信号存在，若不存在则将其手机号作为微信号）添加到微信列表中，以便发微信
                        string weChat = LibSysUtils.ToString(reader["WECHAT"]);
                        if (!string.IsNullOrEmpty(weChat))
                        {
                            if (!weChatList.Contains(weChat))
                                weChatList.Add(weChat);
                        }
                        else if (!string.IsNullOrEmpty(phoneNo) && !weChatList.Contains(phoneNo))
                        {
                            weChatList.Add(phoneNo);
                        }
                        //将人员添加到其用户列表中，以便发系统信息
                        string personId = LibSysUtils.ToString(reader["PERSONID"]);
                        if (!string.IsNullOrEmpty(personId) && !news.UserList.Contains(personId))
                        {
                            news.UserList.Add(personId);
                        }
                    }
                }
            }
            #endregion

            #region 获得单据业务流配置下跟当前功能单据数据相匹配的业务流配置明细并为其接收人（根据需要）发短信/发微信、发系统信息
            List<BusinessInfo> curBusinessInfo = GetBusinessInfo(billMasterRow, billProgId);
            if (curBusinessInfo != null)
            {
                foreach (var info in curBusinessInfo)
                {
                    //将接收人手机（需要接受人接收短信且手机号存在）添加到电话列表中，以便发短信
                    if (info.NeedSMS && !string.IsNullOrEmpty(info.PhoneNo) && !phoneList.Contains(info.PhoneNo))
                    {
                        phoneList.Add(info.PhoneNo);
                    }
                    //将接收人微信（需要接收人接受微信且微信号存在，若不存在则将其手机号作为微信号）添加到微信列表中，以便发微信
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
                    if (!news.UserList.Contains(info.PersonId))
                    {
                        news.UserList.Add(info.PersonId);
                    }
                }
            }
            #endregion

            #region 发送系统信息
            if (news.UserList.Count > 0)
            {
                news.Title = title;
                news.Content = content;
                news.Data = data;
                news.PersonId = LibSysUtils.ToString(billMasterRow["CREATORID"]);
                news.ProgId = billProgId;
                LibSysNewsHelper.SendNews(news);
            }
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

        /// <summary>
        /// 信息处理流程明细
        /// </summary>
        private class BusinessInfo
        {
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
        }
    }

    /// <summary>
    /// 单据业务流配置 数据模板
    /// </summary>
    public class AxpBusinessFlowConfigBcfTemplate : LibTemplate
    {
        // 单据业务流配置 主表
        private const string masterTableName = "AXPBUSINESSFLOWCONFIG";
        // 单据业务流配置 子表 单据业务流配置规则配置
        private const string bodyTableName = "AXPBUSINESSFLOWCONFIGDETAIL";
        // 单据业务流配置 单据业务流配置规则配置 子子表 业务流配置明细
        private const string subTableName = "AXPBUSINESSFLOWCONFIGFLOW";

        /// <summary>
        /// 单据业务流配置 模板功能定义
        /// </summary>
        /// <param name="progId">单据业务流配置 功能标识</param>
        public AxpBusinessFlowConfigBcfTemplate(string progId)
            : base(progId, BillType.Master, "单据业务流配置")
        {
        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            #region 单据业务流配置 主表
            string primaryName = "BUSINESSFLOWCONFIGID";
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "业务流配置代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false, ControlType = LibControlType.Text });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "BUSINESSFLOWCONFIGNAME", "业务流配置名称", FieldSize.Size50) { AllowEmpty = false, ControlType = LibControlType.NText });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PROGID", "功能代码", FieldSize.Size50)
            {
                #region 功能清单
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("axp.FuncList")
                    {
                        RelFields = new RelFieldCollection()
                        {
                           new RelField("PROGNAME", LibDataType.NText,FieldSize.Size50,"功能名称")
                        }  
                    }
                }
                #endregion
            });
            DataSourceHelper.AddFixColumn(masterTable, this.BillType);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns[primaryName] };
            this.DataSet.Tables.Add(masterTable);
            #endregion

            #region 单据业务流配置 子表 单据业务流配置规则配置
            DataTable bodyTable = new DataTable(bodyTableName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, primaryName, "业务流配置代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false, ControlType = LibControlType.Text });
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "USECONDITION", "使用条件", FieldSize.Size500) { DataType = LibDataType.Text, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "USECONDITIONDESC", "使用条件说明", FieldSize.Size200) { DataType = LibDataType.Text });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "BUSINESSPUSHFLOWDETAIL", "业务流配置明细") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true, SubTableIndex = 2 });
            DataSourceHelper.AddRemark(bodyTable);
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", masterTableName, bodyTableName), masterTable.Columns[primaryName], bodyTable.Columns[primaryName]);
            #endregion

            #region 单据业务流配置 单据业务流配置规则配置 子子表 业务流配置明细
            DataTable subTable = new DataTable(subTableName);
            DataSourceHelper.AddColumn(new DefineField(subTable, primaryName, "业务流配置代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false, ControlType = LibControlType.Text });
            DataSourceHelper.AddRowId(subTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(subTable);
            DataSourceHelper.AddRowNo(subTable);
            DataSourceHelper.AddColumn(new DefineField(subTable, "PERSONID", "接收人代码", FieldSize.Size20)
            {
                #region 人员
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() 
                { 
                    new RelativeSource("com.Person")
                    {  
                        RelFields = new RelFieldCollection()
                        { 
                            new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"接收人名称"),
                            new RelField("POSITION", LibDataType.NText,FieldSize.Size50,"职位"),
                            new RelField("PHONENO",LibDataType.Text,FieldSize.Size20,"电话"),
                            new RelField("WECHAT", LibDataType.NText,FieldSize.Size50,"微信")
                        }
                    }  
                }
                #endregion
            });
            DataSourceHelper.AddColumn(new DefineField(subTable, "NEEDSMS", "发短信") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddColumn(new DefineField(subTable, "SENDWECHAT", "发微信") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddRemark(subTable);
            subTable.PrimaryKey = new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"], subTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(subTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", bodyTableName, subTableName), new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] }, new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"] });
            #endregion
        }

        ///<summary>
        ///单据业务流配置 页面排版模型
        ///</summary>
        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "BUSINESSFLOWCONFIGID", "BUSINESSFLOWCONFIGNAME", "PROGID" });
            layout.GridRange = layout.BuildGrid(1, "单据业务流配置规则");
            layout.SubBill.Add(2, layout.BuildGrid(2, "业务流配置明细"));
            layout.ButtonRange = layout.BuildButton(new List<FunButton>() { new FunButton("btnCondition", "条件") });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }

        /// <summary>
        /// 单据业务流配置 功能许可定义--使用缓存
        /// </summary>
        protected override void DefineFuncPermission()
        {
            base.DefineFuncPermission();
            this.FuncPermission.UsingCache = true;
        }
    }
}
