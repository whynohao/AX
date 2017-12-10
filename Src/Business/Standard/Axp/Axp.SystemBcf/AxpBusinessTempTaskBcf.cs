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

    [ProgId(ProgId = "axp.BusinessTempTask", ProgIdType = ProgIdType.Bcf)]
    public class AxpBusinessTempTaskBcf : LibBcfGrid
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpBusinessTempTaskBcfTemplate("axp.BusinessTempTask");
        }
    }

    public class AxpBusinessTempTaskBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPBUSINESSTEMPTASK";

        public AxpBusinessTempTaskBcfTemplate(string progId)
            : base(progId, BillType.Grid, "系统业务临时任务")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "TASKID";
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "任务代码", FieldSize.Size50));
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
            DataSourceHelper.AddColumn(new DefineField(masterTable, "BUSINESSTASKID", "任务代码", FieldSize.Size100) { AllowCopy = false, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "EXECDATE", "执行日期") { DataType = LibDataType.Int32, ControlType = LibControlType.Date });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "EXECTIME", "执行时间") { DataType = LibDataType.Int32, ControlType = LibControlType.Time });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "EXECCONDITION", "执行条件") { DataType = LibDataType.Binary, ControlType = LibControlType.Text, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "INTERNALID", "内码", FieldSize.Size50) { DataType = LibDataType.Text, ControlType = LibControlType.Text });
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
