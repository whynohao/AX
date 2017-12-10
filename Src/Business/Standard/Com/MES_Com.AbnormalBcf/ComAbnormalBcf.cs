using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;


namespace MES_Com.AbnormalBcf
{
    [ProgId(ProgId = "com.Abnormal", ProgIdType = ProgIdType.Bcf)]
    public class ComAbnormalBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new ComAbnormalBcfTemplate("com.Abnormal");
        }
    }
    public class ComAbnormalBcfTemplate : LibTemplate
    {
        private const string tableName = "COMABNORMAL";

        public ComAbnormalBcfTemplate(string progId)
            : base(progId, BillType.Master, "异常主数据")
        {

        }
        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "ABNORMALID";
            DataTable dt = new DataTable(tableName);
            DataSourceHelper.AddColumn(new DefineField(dt, primaryName, "异常代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(dt, "ABNORMALNAME", "异常名称", FieldSize.Size50) { DataType = LibDataType.NText, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(dt, "ABNORMALTYPEID", "异常类别", FieldSize.Size20)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.AbnormalType")
                    {
                         RelFields = new RelFieldCollection()
                         { 
                             new RelField("ABNORMALTYPENAME", LibDataType.NText,FieldSize.Size50,"异常类别名称")
                         } 
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(dt, "BIZATTR", "业务属性") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption, TextOption = new string[] { "生产计划", "生产过程", "采购", "仓储物流" } });
            DataSourceHelper.AddColumn(new DefineField(dt, "DEPTID", "责任部门", FieldSize.Size20)
            {
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.Dept")
                    {
                         RelFields = new RelFieldCollection()
                         { 
                             new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"责任部门名称")
                         } 
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(dt, "CHANGETYPE", "所属类型") { DataType = LibDataType.Int32, ControlType = LibControlType.TextOption,TextOption = new string[] { "设计", "班组", "设备", "物料","品质","其它" } });
            DataSourceHelper.AddFixColumn(dt, this.BillType);
            dt.PrimaryKey = new DataColumn[] { dt.Columns[primaryName] };
            this.DataSet.Tables.Add(dt);
        }
        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "ABNORMALID", "ABNORMALNAME", "ABNORMALTYPEID", "BIZATTR", "DEPTID", "CHANGETYPE" });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
