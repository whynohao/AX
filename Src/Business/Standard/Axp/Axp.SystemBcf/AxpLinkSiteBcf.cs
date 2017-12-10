using AxCRL.Bcf;
using AxCRL.Comm.Define;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxCRL.Template;
using System.Data;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;
using AxCRL.Template.DataSource;

namespace Axp.SystemBcf
{
    [ProgId(ProgId = "axp.LinkSite", ProgIdType = ProgIdType.Bcf)]
    public class AxpLinkSiteBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpLinkSiteBcfTemplate("axp.LinkSite");
        }
    }

    public class AxpLinkSiteBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPLINKSITE";

        public AxpLinkSiteBcfTemplate(string progId) : base(progId, BillType.Master, "链接站点")
        {
        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SITEID", "站点代码", FieldSize.Size20) { AllowCopy = false, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SITENAME", "站点名称", FieldSize.Size100) { AllowEmpty = false, DataType = LibDataType.NText, ControlType = LibControlType.NText });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SHORTNAME", "简称", FieldSize.Size10) { AllowEmpty = false, DataType = LibDataType.NText, ControlType = LibControlType.NText });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SITEURL", "站点地址", FieldSize.Size200) { AllowEmpty = false, DataType = LibDataType.NText, ControlType = LibControlType.NText });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SVCURL", "服务地址", FieldSize.Size200) { AllowEmpty = true, DataType = LibDataType.NText, ControlType = LibControlType.NText });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISSLAVE", "是否从站") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, AllowEmpty = false, DefaultValue = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISSENDTO", "是否向其发送同步数据") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, AllowEmpty = false, DefaultValue = true });
            DataSourceHelper.AddFixColumn(masterTable, this.BillType);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["SITEID"]};
            this.DataSet.Tables.Add(masterTable);
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "SITEID", "SITENAME", "SHORTNAME", "SITEURL", "SVCURL", "ISSLAVE", "ISSENDTO"});
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
