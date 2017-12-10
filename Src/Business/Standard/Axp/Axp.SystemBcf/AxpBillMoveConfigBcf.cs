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
namespace Axp.SystemBcf
{
    [ProgId(ProgId = "axp.BillMoveConfig", ProgIdType = ProgIdType.Bcf, VclPath = @"/Scripts/module/mes/axp/axpBillMoveConfigVcl.js")]
    public class AxpBillMoveConfigBcf : LibBcfGrid
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpBillMoveConfigBcfTemplate("axp.BillMoveConfig");
        }
    }

    public class AxpBillMoveConfigBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPBILLMOVECONFIG";
        private const string bodyTableName = "SETDATADETAIL";

        public AxpBillMoveConfigBcfTemplate(string progId)
            : base(progId, BillType.Grid, "单据流转配置")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "GUID";
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "GUID", "唯一标识", FieldSize.Size50) { AllowEmpty = false, ReadOnly = true });
            DataSourceHelper.AddRowId(masterTable);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SRCPROGID", "源单代码", FieldSize.Size50)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("axp.FuncList")
                    {
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("PROGNAME", LibDataType.NText,FieldSize.Size50,"源单名称","SRCPROGNAME")
                         } 
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SRCTYPEID", "源单类型", FieldSize.Size50) { AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "OBJPROGID", "目的单代码", FieldSize.Size50)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("axp.FuncList")
                    {

                         RelFields = new RelFieldCollection()
                         {
                             new RelField("PROGNAME", LibDataType.NText,FieldSize.Size50,"目的单名称","OBJPROGNAME")
                         } 
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "CONDITION", "条件", FieldSize.Size500));
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ENABLE", "启用") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = 1 });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SETDATADETAIL", "赋值明细表") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, SubTableIndex = 1, ReadOnly = true });
            DataSourceHelper.AddFixColumn(masterTable, this.BillType);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns[primaryName], masterTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(masterTable);

            DataTable bodyTable = new DataTable(bodyTableName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "GUID", "唯一标识", FieldSize.Size50) { ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "PARENTROW_ID", "父行标识") { ReadOnly = true,DataType = LibDataType.Int32, ControlType = LibControlType.Text });
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "SRCPROGID", "源单代码", FieldSize.Size50)
            {
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("axp.FuncList")
                    {
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("PROGNAME", LibDataType.NText,FieldSize.Size50,"源单名称","SRCPROGNAME")
                         } 
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "OBJPROGID", "目的单代码", FieldSize.Size50)
            {
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("axp.FuncList")
                    {

                         RelFields = new RelFieldCollection()
                         {
                             new RelField("PROGNAME", LibDataType.NText,FieldSize.Size50,"目的单名称","OBJPROGNAME")
                         } 
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "SRCTABLEINDEX", "源单表索引") { DataType = LibDataType.Int32, QtyLimit = LibQtyLimit.GreaterOrEqualThanZero });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "SRCFIELDNAME", "源单字段名", FieldSize.Size20) { ControlType = LibControlType.FieldControl, RelProgId = "B.SRCPROGID", AllowEmpty = false, RelTableIndex = "B.SRCTABLEINDEX" });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "OBJTABLEINDEX", "目的单表索引") { DataType = LibDataType.Int32, QtyLimit = LibQtyLimit.GreaterOrEqualThanZero });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "OBJFIELDNAME", "目的单字段名", FieldSize.Size20) { ControlType = LibControlType.FieldControl, RelProgId = "B.OBJPROGID", AllowEmpty = false, RelTableIndex = "B.OBJTABLEINDEX" });
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["PARENTROW_ID"], bodyTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(bodyTable);
            //this.DataSet.Relations.Add(string.Format("{0}_{1}", masterTableName, bodyTableName), masterTable.Columns[primaryName], bodyTable.Columns[primaryName]);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", masterTableName, bodyTableName), new DataColumn[] { masterTable.Columns["GUID"], masterTable.Columns["ROW_ID"], masterTable.Columns["SRCPROGID"], masterTable.Columns["OBJPROGID"] }, new DataColumn[] { bodyTable.Columns["GUID"], bodyTable.Columns["PARENTROW_ID"], bodyTable.Columns["SRCPROGID"], bodyTable.Columns["OBJPROGID"] });
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.GridRange = layout.BuildGrid(0, string.Empty, new List<string>() { "GUID", "ROW_ID", "SRCPROGID", "SRCTYPEID", "OBJPROGID", "CONDITION", "ENABLE", "SETDATADETAIL" });
            layout.SubBill.Add(1, layout.BuildGrid(1, "赋值明细表"));
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
