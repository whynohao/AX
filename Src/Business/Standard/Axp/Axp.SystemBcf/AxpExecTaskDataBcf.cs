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
    [ProgId(ProgId = "axp.ExecTaskData", ProgIdType = ProgIdType.Bcf)]
    public class AxpExecTaskDataBcf : LibBcfGrid
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpExecTaskDataBcfTemplate("axp.ExecTaskData");
        }
    }

    public class AxpExecTaskDataBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXAEXECTASKDATA";

        public AxpExecTaskDataBcfTemplate(string progId)
            : base(progId, BillType.Grid, "排程任务执行结果表")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "EXECTASKDATAID", "执行标识号", FieldSize.Size50));
            DataSourceHelper.AddColumn(new DefineField(masterTable, "CREATETIME", "创建时间") { DataType = LibDataType.Int64, ControlType = LibControlType.DateTime });
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
            DataSourceHelper.AddColumn(new DefineField(masterTable, "RESULTDATA", "结果数据") { DataType = LibDataType.Binary, ControlType = LibControlType.NText });
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["EXECTASKDATAID"] };
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
