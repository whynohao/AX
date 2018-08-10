using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
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

namespace MES_Com.MasterDataBcf
{
    [ProgId(ProgId = "com.CodingRule", ProgIdType = ProgIdType.Bcf)]
    public class ComCodingRuleBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new ComCodingRuleBcfTemplate("com.CodingRule");
        }


        protected override void BeforeUpdate()
        {
            base.BeforeUpdate();
            CodingRuleHelper.CheckRuleData(this);
        }

        protected override void AfterUpdate()
        {
            base.AfterUpdate();
            if (this.BillAction != AxCRL.Bcf.BillAction.AddNew && this.BillAction != AxCRL.Bcf.BillAction.SaveToDraft && this.BillAction != AxCRL.Bcf.BillAction.SubmitDraft)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                string progId = masterRow.HasVersion(DataRowVersion.Original) ? LibSysUtils.ToString(masterRow["PROGID", DataRowVersion.Original]) : LibSysUtils.ToString(masterRow["PROGID"]);
                LibCodingRuleCache.Default.Remove(progId);
                LibCodingNoCache.Default.RemoveCacheByProgId(progId);
            }
        }

        protected override void AfterDelete()
        {
            base.AfterDelete();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            string progId = LibSysUtils.ToString(masterRow["PROGID"]);
            LibCodingRuleCache.Default.Remove(progId);
            LibCodingNoCache.Default.RemoveCacheByProgId(progId);
        }

        public DataSet Print(string[] pks)
        {
            DataSet ds = BrowseTo(pks);
            return ds;
        }
    }

    public class ComCodingRuleBcfTemplate : LibTemplate
    {
        private const string tableName = "COMCODINGRULE";
        private const string tableDetailName = "COMCODINGRULEDETAIL";
        private const string dyRuleDetailName = "COMCODINGDYRULE";

        public ComCodingRuleBcfTemplate(string progId)
            : base(progId, BillType.Master, "资料编码规则")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "CODINGRULEID";
            DataTable headTable = new DataTable(tableName);
            DataSourceHelper.AddColumn(new DefineField(headTable, primaryName, "规则代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(headTable, "CODINGRULENAME", "规则名称", FieldSize.Size50) { DataType = LibDataType.NText, ControlType = LibControlType.NText, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(headTable, "PROGID", "功能代码", FieldSize.Size50)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                SelectSql = "Select A.PROGID as Id,A.PROGNAME as Name From AXPFUNCLIST A",
                SelectFields = "A.PROGNAME",
                RelativeSource = new RelativeSourceCollection(){
                    new RelativeSource("axp.FuncList"){
                           RelFields = new RelFieldCollection(){
                           new RelField("PROGNAME", LibDataType.NText,FieldSize.Size50,"功能名称")
                      }  
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(headTable, "CODINGRULELENGTH", "编码长度") { DataType = LibDataType.Int32, ReadOnly = true });
            DataSourceHelper.AddFixColumn(headTable, this.BillType);
            headTable.PrimaryKey = new DataColumn[] { headTable.Columns[primaryName] };
            this.DataSet.Tables.Add(headTable);

            DataTable bodyTable = new DataTable(tableDetailName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, primaryName, "规则代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "SECTIONEXPLAIN", "编码段说明", FieldSize.Size50) { DataType = LibDataType.NText, ControlType = LibControlType.NText });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "SECTIONTYPE", "编码段类型") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, TextOption = new string[] { "固定值", "流水号", "动态段", "日期（yyyymmdd）", "日期（yymmdd）", "日期（ddmmyy）", "日期（ABmmdd）", "日期(yymm)" } });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "SECTIONLENGTH", "编码段长度") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterThanZero });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "FIELDNAME", "字段名", FieldSize.Size50) { ControlType = LibControlType.FieldControl, RelProgId = "A.PROGID" });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "SECTIONVALUE", "编码段内容", FieldSize.Size20));
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "DYRULEDETAIL", "动态规则明细") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true, SubTableIndex = 2 });
            DataSourceHelper.AddRemark(bodyTable);
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] };
            bodyTable.ExtendedProperties.Add(TableProperty.AllowEmpt, false);
            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", tableName, tableDetailName), headTable.Columns[primaryName], bodyTable.Columns[primaryName]);

            DataTable dyRuleDetailTable = new DataTable(dyRuleDetailName);
            DataSourceHelper.AddColumn(new DefineField(dyRuleDetailTable, primaryName, "规则代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(dyRuleDetailTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(dyRuleDetailTable);
            DataSourceHelper.AddRowNo(dyRuleDetailTable);
            DataSourceHelper.AddColumn(new DefineField(dyRuleDetailTable, "FIELDVALUE", "字段值", FieldSize.Size100) { ControlType = LibControlType.Text, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(dyRuleDetailTable, "SECTIONVALUE", "编码值", FieldSize.Size10) { ControlType = LibControlType.Text, AllowEmpty = false });
            DataSourceHelper.AddRemark(dyRuleDetailTable);
            dyRuleDetailTable.PrimaryKey = new DataColumn[] { dyRuleDetailTable.Columns[primaryName], dyRuleDetailTable.Columns["PARENTROWID"], dyRuleDetailTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(dyRuleDetailTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", tableDetailName, dyRuleDetailName), new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] }, new DataColumn[] { dyRuleDetailTable.Columns[primaryName], dyRuleDetailTable.Columns["PARENTROWID"] });
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "CODINGRULEID", "CODINGRULENAME", "PROGID", "CODINGRULELENGTH" });
            layout.GridRange = layout.BuildGrid(1, "编码规则明细");
            layout.SubBill.Add(2, layout.BuildGrid(2, "动态规则明细"));
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
