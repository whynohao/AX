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

namespace Axp.SystemBcf
{
    [ProgId(ProgId = "axp.FuncList", ProgIdType = ProgIdType.Bcf)]
    public class AxpFuncListBcf : LibBcfGrid
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpFuncListBcfTemplate("axp.FuncList");
        }
    }

    public class AxpFuncListBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPFUNCLIST";

        public AxpFuncListBcfTemplate(string progId)
            : base(progId, BillType.Grid, "功能清单")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "PROGID";
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "功能标识", FieldSize.Size50) { ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PROGNAME", "功能名称", FieldSize.Size50) { DataType = LibDataType.NText, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "CONFIGPACK", "配置包", FieldSize.Size20) { ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "CANMENU", "允许挂菜单") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "KEYCODE", "快捷码", FieldSize.Size20) { ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PERMISSION", "权限标识") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "BILLTYPE", "功能种类")
            {
                DataType = LibDataType.Int32,
                ControlType = LibControlType.TextOption,
                TextOption = new string[] { "主数据", "单据", "数据维护功能", "自定义功能", "报表", "日报表" },
                ReadOnly = true
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PROGTAG", "功能标签", FieldSize.Size100) { ReadOnly = true });
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns[primaryName] };
            this.DataSet.Tables.Add(masterTable);
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.GridRange = layout.BuildGrid(0, string.Empty, null, true);
            this.ViewTemplate = new LibGridTpl(this.DataSet, layout);
            this.FuncPermission.CanMenu = false;
        }
    }
}
