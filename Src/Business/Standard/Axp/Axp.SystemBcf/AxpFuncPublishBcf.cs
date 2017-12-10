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
    [ProgId(ProgId = "axp.FuncPublish", ProgIdType = ProgIdType.Bcf)]
    public class AxpFuncPublishBcf : LibBcfGrid
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpFuncPublishBcfTemplate("axp.FuncPublish");
        }
    }

    public class AxpFuncPublishBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPFUNCPUBLISH";

        public AxpFuncPublishBcfTemplate(string progId)
            : base(progId, BillType.Grid, "功能发布记录表")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "MENUITEM", "清单名称", FieldSize.Size50));
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PROGID", "功能代码", FieldSize.Size50));
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PROGNAME", "功能名称", FieldSize.Size50));
            DataSourceHelper.AddColumn(new DefineField(masterTable, "BILLTYPE", "功能种类")
            {
                DataType = LibDataType.Int32,
                ControlType = LibControlType.TextOption,
                TextOption = new string[] { "主数据", "单据", "数据维护功能", "自定义功能", "报表", "日报表" },
                ReadOnly = true
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ENTRYPARAM", "入口参数", FieldSize.Size200));
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PUBLISHDATE", "发布日期", FieldSize.Size200));
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["PROGID"], masterTable.Columns["ENTRYPARAM"]};
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
