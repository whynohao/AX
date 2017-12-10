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
    [ProgId(ProgId = "axp.AttachmentRecord", ProgIdType = ProgIdType.Bcf)]
    public class AxpAttachmentRecordBcf : LibBcfGrid
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpAttachmentRecordBcfTemplate("axp.AttachmentRecord");
        }
    }

    public class AxpAttachmentRecordBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPATTACHMENTRECORD";
        private const string bodyTableName = "AXPATTACHMENTRECORDDETAIL";

        public AxpAttachmentRecordBcfTemplate(string progId)
            : base(progId, BillType.Grid, "附件记录")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "BELONGTOID", "附件关联标识", FieldSize.Size50) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ORDERID", "附件标识号") { DataType = LibDataType.Int32, ControlType = LibControlType.Number });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ORDERNUM", "序号") { DataType = LibDataType.Int32, ControlType = LibControlType.Number });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ATTACHMENTNAME", "附件名") { DataType = LibDataType.Binary, ControlType = LibControlType.NText });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "CANUSE", "可用") { DataType = LibDataType.Boolean, ControlType = LibControlType.TextOption, ReadOnly = true });
            //Zhangkj 20170104 添加附件记录行对应的文档库中的文档编号
            DataSourceHelper.AddColumn(new DefineField(masterTable, "DOCID", "对应文档编号", FieldSize.Size50) { AllowCopy = false });
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["BELONGTOID"], masterTable.Columns["ORDERID"] };
            this.DataSet.Tables.Add(masterTable);

            DataTable bodyTable = new DataTable(bodyTableName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "BELONGTOID", "附件关联标识", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "ORDERID", "附件标识号") { DataType = LibDataType.Int32, ControlType = LibControlType.Number });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "FILENAME", "文件名", FieldSize.Size50));
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "PERSONID", "上传人代码", FieldSize.Size20)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() {
                 new RelativeSource("com.Person"){  RelFields = new RelFieldCollection()
                     { new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"上传人名称")}}
                }
            });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "CREATETIME", "上传时间") { DataType = LibDataType.Int64, ControlType = LibControlType.DateTime });
            DataSourceHelper.AddRemark(bodyTable);
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns["BELONGTOID"], bodyTable.Columns["ORDERID"], bodyTable.Columns["CREATETIME"] };
            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", masterTableName, bodyTableName), new DataColumn[] { masterTable.Columns["BELONGTOID"], masterTable.Columns["ORDERID"] },
                new DataColumn[] { bodyTable.Columns["BELONGTOID"], bodyTable.Columns["ORDERID"] });
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.GridRange = layout.BuildGrid(0, string.Empty, null, true);
            this.ViewTemplate = new LibGridTpl(this.DataSet, layout);
        }

        protected override void DefineFuncPermission()
        {
            base.DefineFuncPermission();
            this.FuncPermission.CanMenu = false;
        }
    }
}
