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
    [ProgId(ProgId = "axp.ChangeDataReason", ProgIdType = ProgIdType.Bcf)]
    public class AxpChangeDataReasonBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpChangeDataReasonBcfTemplate("axp.ChangeDataReason");
        }
    }

    public class AxpChangeDataReasonBcfTemplate : LibTemplate
    {
        private const string TableName = "AXPCHANGEDATAREASON";
        public AxpChangeDataReasonBcfTemplate(string progId)
            : base(progId, BillType.Master, "审核变更异常原因")
        { }


        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "REASONID";
            DataTable masterTable = new DataTable(TableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "原因编号", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "REASONNAME", "原因名称", FieldSize.Size50) { DataType = LibDataType.NText, AllowEmpty = false });
            DataSourceHelper.AddFixColumn(masterTable, this.BillType);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns[primaryName] };
            this.DataSet.Tables.Add(masterTable);
        }


        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "REASONID", "REASONNAME" });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
