using AxCRL.Bcf;
using AxCRL.Comm.Define;
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
    [ProgId(ProgId = "axp.BusinessTask", ProgIdType = ProgIdType.Bcf)]
    public class AxpBusinessTaskBcf : LibBcfGrid
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpBusinessTaskBcfTemplate("axp.BusinessTask");
        }
    }

    public class AxpBusinessTaskBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPBUSINESSTASK";

        public AxpBusinessTaskBcfTemplate(string progId)
            : base(progId, BillType.Grid, "业务任务表")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable masterTable = new DataTable(masterTableName);
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
            DataSourceHelper.AddColumn(new DefineField(masterTable, "BUSINESSTASKID", "任务代码", FieldSize.Size50) { AllowCopy = false, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "BUSINESSTASKNAME", "任务名称", FieldSize.Size50) { ControlType = LibControlType.NText });
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["PROGID"], masterTable.Columns["BUSINESSTASKID"] };
            this.DataSet.Tables.Add(masterTable);
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
