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
    [ProgId(ProgId = "axp.UserLogin", ProgIdType = ProgIdType.Bcf)]
    public class AxpUserLoginBcf : LibBcfGrid
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpUserLoginBcfTemplate("axp.UserLogin");
        }
    }

    public class AxpUserLoginBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPUSERLOGIN";

        public AxpUserLoginBcfTemplate(string progId)
            : base(progId, BillType.Grid, "用户登入信息")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "USERID", "账号", FieldSize.Size20));
            DataSourceHelper.AddColumn(new DefineField(masterTable, "CREATETIME", "句柄创建时间") { DataType = LibDataType.Int64, ControlType = LibControlType.DateTime });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "HANDLETYPE", "终端类型") { DataType = LibDataType.Int32 });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "FREETIME", "句柄释放时间") { DataType = LibDataType.Int64, ControlType = LibControlType.DateTime });
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["USERID"], masterTable.Columns["CREATETIME"] };
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
