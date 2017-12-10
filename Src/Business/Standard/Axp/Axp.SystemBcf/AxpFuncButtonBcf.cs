using AxCRL.Bcf;
using AxCRL.Comm.Define;
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

namespace Mdvm_Axp.SystemBcf
{
    [ProgId(ProgId = "axp.FuncButton", ProgIdType = ProgIdType.Bcf)]
    public class AxpFuncButtonBcf : LibBcfGrid
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpFuncButtonBcfTemplate("axp.FuncButton");
        }
    }

    public class AxpFuncButtonBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPFUNCBUTTON";

        public AxpFuncButtonBcfTemplate(string progId)
            : base(progId, BillType.Grid, "功能按钮清单")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "PROGID";
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "功能标识", FieldSize.Size50) { ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "BUTTONID", "按钮标识", FieldSize.Size50) { ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "BUTTONNAME", "按钮名称", FieldSize.Size50) { ReadOnly = true, ControlType = LibControlType.NText, DataType = LibDataType.NText });
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns[primaryName], masterTable.Columns["BUTTONID"] };
            this.DataSet.Tables.Add(masterTable);
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.GridRange = layout.BuildGrid(0, string.Empty, null, true);
            this.ViewTemplate = new LibGridTpl(this.DataSet, layout);
        }
    }
}
