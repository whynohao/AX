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
    [ProgId(ProgId = "axp.PrintTpl", ProgIdType = ProgIdType.Bcf, VclPath = @"/Scripts/module/mes/axp/axpPrintTplVcl.js")]
    public class AxpPrintTplBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpPrintTplBcfTemplate("axp.PrintTpl");
        }
    }

    public class AxpPrintTplBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPPRINTTPL";
        private const string bodyTableName = "AXPPRINTTPLDETAIL";
        private const string subTableName = "AXPPRINTTPLSUB";
        private const string mapTableName = "AXPPRINTTPLMAPPING";

        public AxpPrintTplBcfTemplate(string progId)
            : base(progId, BillType.Master, "打印模板设计")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "PRINTTPLID";
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "打印模板代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PRINTTPLNAME", "打印模板名称", FieldSize.Size50) { AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PROGID", "功能代码", FieldSize.Size50)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection(){
                    new RelativeSource("axp.FuncList"){
                           RelFields = new RelFieldCollection(){
                           new RelField("PROGNAME", LibDataType.NText,FieldSize.Size50,"功能名称")
                      }  
                    }
                }
            });
            DataSourceHelper.AddFixColumn(masterTable, this.BillType);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns[primaryName] };
            this.DataSet.Tables.Add(masterTable);

            DataTable bodyTable = new DataTable(bodyTableName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, primaryName, "打印模板代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "USECONDITION", "使用条件", FieldSize.Size500));
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "USECONDITIONDESC", "使用条件说明", FieldSize.Size200));
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "PRINTDETAIL", "打印明细") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true, SubTableIndex = 2 });
            DataSourceHelper.AddRemark(bodyTable);
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", masterTableName, bodyTableName), masterTable.Columns[primaryName], bodyTable.Columns[primaryName]);

            DataTable subTable = new DataTable(subTableName);
            DataSourceHelper.AddColumn(new DefineField(subTable, primaryName, "打印模板代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(subTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(subTable);
            DataSourceHelper.AddRowNo(subTable);
            DataSourceHelper.AddColumn(new DefineField(subTable, "ISTPL", "模板设计") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(subTable, "FIELDDETAIL", "字段明细") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true, SubTableIndex = 3 });
            DataSourceHelper.AddColumn(new DefineField(subTable, "TPLJS", "模板脚本") { DataType = LibDataType.Binary, ControlType = LibControlType.NText, ReadOnly = true });
            DataSourceHelper.AddRemark(subTable);
            subTable.PrimaryKey = new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"], subTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(subTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", bodyTableName, subTableName), new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] }, new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"] });

            DataTable mapTable = new DataTable(mapTableName);
            DataSourceHelper.AddColumn(new DefineField(mapTable, primaryName, "打印模板代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(mapTable, "GRANDFATHERROWID", "主行标识");
            DataSourceHelper.AddRowId(mapTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(mapTable);
            DataSourceHelper.AddRowNo(mapTable);
            DataSourceHelper.AddColumn(new DefineField(mapTable, "TPLPARAM", "参数", FieldSize.Size50) { DataType = LibDataType.Text, ControlType = LibControlType.Text, ReadOnly = true, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(mapTable, "TABLEINDEX", "对应表索引") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterOrEqualThanZero });
            DataSourceHelper.AddColumn(new DefineField(mapTable, "FIELDNAME", "对应字段", FieldSize.Size50) { ControlType = LibControlType.FieldControl, RelProgId = "A.PROGID", AllowEmpty = false, RelTableIndex = "C.TABLEINDEX" });
            DataSourceHelper.AddRemark(mapTable);
            mapTable.PrimaryKey = new DataColumn[] { mapTable.Columns[primaryName], mapTable.Columns["GRANDFATHERROWID"], mapTable.Columns["PARENTROWID"], mapTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(mapTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", subTableName, mapTableName), new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"], subTable.Columns["ROW_ID"] }, new DataColumn[] { mapTable.Columns[primaryName], mapTable.Columns["GRANDFATHERROWID"], mapTable.Columns["PARENTROWID"] });
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "PRINTTPLID", "PRINTTPLNAME", "PROGID" });
            layout.GridRange = layout.BuildGrid(1, "打印规则明细");
            layout.SubBill.Add(2, layout.BuildGrid(2, "打印模板设计明细"));
            layout.SubBill.Add(3, layout.BuildGrid(3, "字段映射明细"));
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
