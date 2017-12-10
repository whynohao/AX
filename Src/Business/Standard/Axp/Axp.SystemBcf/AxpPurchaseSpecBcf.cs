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
    [ProgId(ProgId = "axp.PurchaseSpec", ProgIdType = ProgIdType.Bcf)]
    public class AxpPurchaseSpecBcf : LibBcfGrid
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpPurchaseSpecBcfTemplate("axp.PurchaseSpec");
        }
    }

    public class AxpPurchaseSpecBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPPURCHASESPEC";

        public AxpPurchaseSpecBcfTemplate(string progId)
            : base(progId, BillType.Grid, "系统规格")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "PURCHASERID";
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "客户代码", FieldSize.Size20));
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PURCHASERNAME", "客户名称", FieldSize.Size50) { DataType = LibDataType.NText });
            //-1为不限制使用数
            DataSourceHelper.AddColumn(new DefineField(masterTable, "MAXUSERCOUNT", "最大用户数") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, DefaultValue = -1 });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "MAXWORKSTATIONCOUNT", "最大站点数") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, DefaultValue = -1 });
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns[primaryName] };
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
