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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
namespace Axp.SystemBcf
{
    [ProgId(ProgId = "axp.AttachmentTpl", ProgIdType = ProgIdType.Bcf)]
    public class AxpAttachmentTplBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpAttachmentTplBcfTemplate("axp.AttachmentTpl");
        }
    }

    public class AxpAttachmentTplBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPATTACHMENTTPL";
        private const string bodyTableName = "AXPATTACHMENTTPLDETAIL";
        private const string subTableName = "AXPATTACHMENTTPLSUB";



        public AxpAttachmentTplBcfTemplate(string progId)
            : base(progId, BillType.Master, "附件模板")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "ATTACHMENTTPLID";
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "模板代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ATTACHMENTTPLNAME", "模板名称", FieldSize.Size50) { AllowEmpty = false });
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
            DataSourceHelper.AddColumn(new DefineField(bodyTable, primaryName, "模板代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "USECONDITION", "使用条件", FieldSize.Size500));
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "USECONDITIONDESC", "使用条件说明", FieldSize.Size200) { DataType = LibDataType.NText, ControlType = LibControlType.NText });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "ATTACHMENTDETAIL", "附件明细") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true, SubTableIndex = 2 });
            DataSourceHelper.AddRemark(bodyTable);
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", masterTableName, bodyTableName), masterTable.Columns[primaryName], bodyTable.Columns[primaryName]);

            DataTable subTable = new DataTable(subTableName);
            DataSourceHelper.AddColumn(new DefineField(subTable, primaryName, "模板代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(subTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(subTable);
            DataSourceHelper.AddRowNo(subTable);
            DataSourceHelper.AddColumn(new DefineField(subTable, "ATTACHMENTNAME", "附件名", FieldSize.Size200) { DataType = LibDataType.NText, ControlType = LibControlType.NText });
            DataSourceHelper.AddRemark(subTable);
            subTable.PrimaryKey = new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"], subTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(subTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", bodyTableName, subTableName), new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] }, new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"] });
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "ATTACHMENTTPLID", "ATTACHMENTTPLNAME", "PROGID" });
            layout.GridRange = layout.BuildGrid(1, "附件模板选用明细");
            layout.SubBill.Add(2, layout.BuildGrid(2, "附件明细"));
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
