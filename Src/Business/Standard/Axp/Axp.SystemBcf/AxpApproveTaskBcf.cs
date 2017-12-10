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
    [ProgId(ProgId = "axp.ApproveTask", ProgIdType = ProgIdType.Bcf)]
    public class AxpApproveTaskBcf : LibBcfGrid
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpApproveTaskBcfTemplate("axp.ApproveTask");
        }
    }

    public class AxpApproveTaskBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPAPPROVETASK";

        public AxpApproveTaskBcfTemplate(string progId)
            : base(progId, BillType.Grid, "审核任务过账表")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "AUDITTASKID", "审核任务索引", FieldSize.Size50) { AllowCopy = false, ReadOnly = true, AllowEmpty = false });//作为唯一索引使用
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
            DataSourceHelper.AddColumn(new DefineField(masterTable, "INTERNALID", "单据内码", FieldSize.Size50));
            DataSourceHelper.AddColumn(new DefineField(masterTable, "BILLNO", "单据编号", FieldSize.Size20));
            DataSourceHelper.AddColumn(new DefineField(masterTable, "FROMROWID", "行标识号") { DataType = LibDataType.Int32, ControlType = LibControlType.Number });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "CURRENTLEVEL", "当前层级") { DefaultValue = 1, DataType = LibDataType.Int32, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterThanZero });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "FLOWLEVEL", "审核层级") { DefaultValue = 1, DataType = LibDataType.Int32, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterThanZero });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "SUBMITTERID", "提交人代码", FieldSize.Size20)
            {
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() { 
                 new RelativeSource("com.Person"){  
                     RelFields = new RelFieldCollection()
                 }
                }
            });
            //DataSourceHelper.AddColumn(new DefineField(masterTable, "PERSONID", "审核人代码", FieldSize.Size20)
            //{
            //    ControlType = LibControlType.IdName,
            //    RelativeSource = new RelativeSourceCollection() { 
            //     new RelativeSource("com.Person"){  RelFields = new RelFieldCollection()
            //         { new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"审核人名称"),
            //           new RelField("POSITION", LibDataType.NText,FieldSize.Size50,"职位")}}  
            //    }
            //});
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PERSONID", "审核人代码", FieldSize.Size20)
            {
                ControlType = LibControlType.IdName,
                ReadOnly = true,
                AllowEmpty = false,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.Person")
                     {
                         RelFields = new RelFieldCollection()
                         {
                            new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"审核人名称")
                         }
                     }
                }
            });
            //所属部门，字段需要在提交审核时根据配置的工作流审核过程计算得到，不一定是直接根据人员所属部门  //Zhangkj 20170321 实现按岗位审核增加
            DataSourceHelper.AddColumn(new DefineField(masterTable, "DEPTID", "部门", FieldSize.Size50)
            {
                ControlType = LibControlType.IdName,
                DataType = LibDataType.Text,
                ReadOnly =true,
                RelativeSource = new RelativeSourceCollection()
                {                    
                     new RelativeSource("com.Dept")
                     {
                         RelFields = new RelFieldCollection()
                         {
                            new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size20,"部门名称")
                         }
                     }
                }
            });
            //执行审核时的岗位，字段需要在提交审核时根据配置的工作流审核过程计算得到，不是直接根据人员表中的岗位名称  //Zhangkj 20170321 实现按岗位审核增加
            DataSourceHelper.AddColumn(new DefineField(masterTable, "DUTYID", "岗位", FieldSize.Size50)
            {
                ControlType = LibControlType.IdName,
                DataType = LibDataType.Text,
                ReadOnly = true,
                RelativeSource = new RelativeSourceCollection()
                {
                     new RelativeSource("com.Duty")
                     {
                         RelFields = new RelFieldCollection()
                         {
                            new RelField("DUTYNAME", LibDataType.NText,FieldSize.Size20,"岗位名称")
                         }
                     }
                }
            });

            DataSourceHelper.AddColumn(new DefineField(masterTable, "INDEPENDENT", "独立决策权") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "AUDITOPINION", "审核意见",FieldSize.Size100) { DataType = LibDataType.NText, ControlType=LibControlType.NText });
            
            // 执行说明 。具体的审核过程，根据配置来进行审核执行时的流程特殊说明。例如，配置为经理岗位审核，如因为对应部门没有经理，而又跳过了，则会说明哪个部门没有经理岗位，岗位进行了上溯等。
            DataSourceHelper.AddColumn(new DefineField(masterTable, "EXECUTEDESC", "执行说明", FieldSize.Size500) { DataType = LibDataType.NText, ControlType = LibControlType.NText });

            //同人默认”配置：如果本次审核过程的审核执行人与之前的审核过程中审核执行人是同一个人，则默认为本次审核过程与之前的一致。默认为true，即同一个审核执行人对同一个单据的同一次提交审核一次给出意见即可
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISSAMEDEFAULT", "同人默认") { DefaultValue = true, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });

            //是否是跳过的过程
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISJUMP", "是否跳过") { DefaultValue = false, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            //跳过原因
            DataSourceHelper.AddColumn(new DefineField(masterTable, "JUMPREASON", "跳过原因", FieldSize.Size100) { DataType = LibDataType.NText, ControlType = LibControlType.NText });
           

            DataSourceHelper.AddColumn(new DefineField(masterTable, "AUDITSTATE", "审核状态") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, TextOption = new string[] { "审核已提交", "审核已通过", "审核未通过" }, ReadOnly = true });
            //DataSourceHelper.AddColumn(new DefineField(masterTable, "ISPASS", "审核通过") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true });
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["PROGID"], masterTable.Columns["INTERNALID"], masterTable.Columns["FROMROWID"], masterTable.Columns["CURRENTLEVEL"], masterTable.Columns["FLOWLEVEL"], masterTable.Columns["PERSONID"] };
            //masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["AUDITTASKID"]};//Zhangkj 20170327 使用一个Guid作为主键
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
