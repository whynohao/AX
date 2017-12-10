using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;
using AxCRL.Data.SqlBuilder;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;

namespace MES_Com.AbnormalBcf
{
    /// <summary>
    /// 异常报告单单据类型
    /// </summary>
    [ProgId(ProgId = "com.AbnormalReportType", ProgIdType = ProgIdType.Bcf, VclPath = @"/Scripts/module/mes/com/comAbnormalReportTypeVcl.js")]
    public class ComAbnormalReportTypeBcf : LibBcfData
    {
        /// <summary>
        /// 异常报告单单据类型 模板注册
        /// </summary>
        /// <returns>返回 异常报告单单据类型 的数据模板</returns>
        protected override LibTemplate RegisterTemplate()
        {
            return new ComAbnormalReportTypeBcfTemplate("com.AbnormalReportType");
        }

        /// <summary>
        /// 异常报告单单据类型 保存前验证【不能出现相同的使用条件;相同的使用条件中不能出现相同的人；发短信或发微信时，接收人的电话或微信须存在；不被异常报告单引用】
        /// </summary>
        protected override void BeforeUpdate()
        {
            base.BeforeUpdate();
            #region 当异常报告单单据类型变更时，确保原始的异常报告单单据类型不被异常报告单引用
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            if (masterRow.HasVersion(DataRowVersion.Original))
            {
                string typeId = LibSysUtils.ToString(masterRow["TYPEID", DataRowVersion.Original]);
                if (LibSysUtils.ToString(masterRow["TYPEID"]).CompareTo(typeId) != 0)
                {
                    string sql = string.Format("SELECT COUNT(BILLNO) FROM COMABNORMALREPORT WHERE TYPEID={0}", LibStringBuilder.GetQuotString(typeId));
                    int count = LibSysUtils.ToInt32(this.DataAccess.ExecuteScalar(sql));
                    if (count > 0)
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("异常报告单单据类型{0}在异常报告单被引用，无法修改", typeId));
                    }
                }
            }
            #endregion

            #region 消息传递流程明细表中不能出现相同的使用条件;相同的使用条件中不能出现相同的人；发短信或发微信时，接收人的电话或微信须存在；
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
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("消息传递流程规则配置中行标识{0}的使用条件存在重复", LibSysUtils.ToInt32(curRow["ROW_ID"])));
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
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("消息传递流程规则中行标识{0}下消息传递流程明细中行标识{1}的接收人存在重复",
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
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("消息传递流程规则中行标识{0}下消息传递流程明细中行标识{1}的接收人的微信不存在，无法发微信",
                                LibSysUtils.ToInt32(childRow["PARENTROWID"]), LibSysUtils.ToInt32(childRow["ROW_ID"])));
                        }
                        if (string.IsNullOrWhiteSpace(phoneNo) && needSms)
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("消息传递流程规则中行标识{0}下消息传递流程明细中行标识{1}的接收人的电话不存在，无法发短信",
                                LibSysUtils.ToInt32(childRow["PARENTROWID"]), LibSysUtils.ToInt32(childRow["ROW_ID"])));
                        }
                    }
                }
                #endregion
            }
            #endregion
        }

        /// <summary>
        /// 异常报告单单据类型 删除前验证【不被异常报告单引用】
        /// </summary>
        protected override void BeforeDelete()
        {
            base.BeforeDelete();
            #region 当异常报告单单据类型删除时，确保原始的异常报告单单据类型不被异常报告单引用
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            if (masterRow.HasVersion(DataRowVersion.Original))
            {
                string typeId = LibSysUtils.ToString(masterRow["TYPEID", DataRowVersion.Original]);
                if (LibSysUtils.ToString(masterRow["TYPEID"]).CompareTo(typeId) != 0)
                {
                    string sql = string.Format("SELECT COUNT(BILLNO) FROM COMABNORMALREPORT WHERE TYPEID={0}", LibStringBuilder.GetQuotString(typeId));
                    int count = LibSysUtils.ToInt32(this.DataAccess.ExecuteScalar(sql));
                    if (count > 0)
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("异常报告单单据类型{0}在异常报告单被引用，无法删除", typeId));
                    }
                }
            }
            #endregion
        }
    }

    /// <summary>
    /// 异常报告单单据类型 数据模板
    /// </summary>
    public class ComAbnormalReportTypeBcfTemplate : LibTemplate
    {
        // 异常报告单单据类型 主表
        private const string tableName = "COMABNORMALREPORTTYPE";
        // 异常报告单单据类型 子表 消息传递流程规则配置
        private const string bodyTableName = "COMABNORMALREPORTTYPEDETAIL";
        // 异常报告单单据类型 消息传递流程规则配置 子子表 消息传递流程明细
        private const string subTableName = "COMABNORMALREPORTTYPEFLOW";

        /// <summary>
        /// 异常报告单单据类型 模板功能定义
        /// </summary>
        /// <param name="progId">异常报告单单据类型 功能标识</param>
        public ComAbnormalReportTypeBcfTemplate(string progId)
            : base(progId, BillType.Master, "异常报告单单据类型")
        {
        }

        ///<summary>
        ///异常报告单单据类型 数据模型
        ///</summary>
        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            #region 异常报告单单据类型 主表
            DataTable masterTable = new DataTable(tableName);
            string primaryName = "TYPEID";
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "单据类型代码", FieldSize.Size50) { DataType = LibDataType.Text, AllowCopy = false, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "TYPENAME", "单据类型名称", FieldSize.Size20) { DataType = LibDataType.NText, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISREPULSE", "可拒绝") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "DEFAULTCREATESTATE", "缺省创建状态") { AllowEmpty = false, DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, DefaultValue = 1, TextOption = new string[] { "未生效", "生效" } });
            DataSourceHelper.AddFixColumn(masterTable, this.BillType);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["TYPEID"] };
            this.DataSet.Tables.Add(masterTable);
            #endregion

            #region 异常报告单单据类型 子表 消息传递流程规则配置
            DataTable bodyTable = new DataTable(bodyTableName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, primaryName, "单据类型代码", FieldSize.Size50) { DataType = LibDataType.Text, AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "USECONDITION", "使用条件", FieldSize.Size500) { DataType = LibDataType.Text, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "USECONDITIONDESC", "使用条件说明", FieldSize.Size200) { DataType = LibDataType.Text });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "FLOWDETAIL", "消息传递流程明细") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true, SubTableIndex = 2 });
            DataSourceHelper.AddRemark(bodyTable);
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", tableName, bodyTableName), masterTable.Columns[primaryName], bodyTable.Columns[primaryName]);
            #endregion

            #region 异常报告单单据类型 消息传递流程规则配置 子子表 消息传递流程明细
            DataTable subTable = new DataTable(subTableName);
            DataSourceHelper.AddColumn(new DefineField(subTable, primaryName, "单据类型代码", FieldSize.Size50) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(subTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(subTable);
            DataSourceHelper.AddRowNo(subTable);
            DataSourceHelper.AddColumn(new DefineField(subTable, "TRANSMITLEVEL", "传递层级") { DefaultValue = 1, DataType = LibDataType.Int32, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterThanZero });
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
            DataSourceHelper.AddColumn(new DefineField(subTable, "CONTROLTIME", "响应时效") { DataType = LibDataType.Double, ControlType = LibControlType.Double });
            DataSourceHelper.AddColumn(new DefineField(subTable, "TIMEUNIT", "时间单位") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, TextOption = new string[] { "小时", "分钟" } });
            DataSourceHelper.AddRemark(subTable);
            subTable.PrimaryKey = new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"], subTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(subTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", bodyTableName, subTableName), new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] }, new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"] });
            #endregion
        }

        ///<summary>
        ///异常报告单单据类型 页面排版模型
        ///</summary>
        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "TYPEID", "TYPENAME", "ISREPULSE", "DEFAULTCREATESTATE" });
            layout.TabRange.Add(layout.BuildGrid(1, "消息传递流程规则配置"));
            layout.SubBill.Add(2, layout.BuildGrid(2, "消息传递流程明细"));
            layout.ButtonRange = layout.BuildButton(new List<FunButton>() { new FunButton("btnCondition", "选择条件") });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }

        /// <summary>
        /// 异常报告单单据类型 功能许可定义--使用缓存
        /// </summary>
        protected override void DefineFuncPermission()
        {
            base.DefineFuncPermission();
            this.FuncPermission.UsingCache = true;
        }
    }

}
