using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;
using AxCRL.Data.SqlBuilder;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;

namespace MES_Com.AbnormalBcf
{
    [ProgId(ProgId = "com.AbnormalReasonType", ProgIdType = ProgIdType.Bcf)]
    public class ComAbnormalReasonTypeBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new ComAbnormalReasonTypeBcfTemplate("com.AbnormalReasonType");
        }
    }
    public class ComAbnormalReasonTypeBcfTemplate : LibTemplate
    {
        private const string tableName = "COMABNORMALREASONTYPE";
        public ComAbnormalReasonTypeBcfTemplate(string progId)
            : base(progId, BillType.Master, "异常原因类别")
        {
        }
        ///<summary>
        ///构建数据库模型
        ///</summary>
        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable comType = new DataTable(tableName);
            //构建表结构
            DataSourceHelper.AddColumn(new DefineField(comType, "ABNORMALREASONTYPEID", "原因类别编号", FieldSize.Size50) { AllowCopy = false, AllowEmpty = false, DataType = LibDataType.Text });
            DataSourceHelper.AddColumn(new DefineField(comType, "ABNORMALREASONTYPENAME", "原因类别名称", FieldSize.Size20) { DataType = LibDataType.NText, AllowEmpty = false });
            DataSourceHelper.AddFixColumn(comType, this.BillType);//系统自动创建的内容
            comType.PrimaryKey = new DataColumn[] { comType.Columns["ABNORMALREASONTYPEID"] };//定义表的主键
            this.DataSet.Tables.Add(comType);
        }
        ///<summary>
        ///页面排版模型
        ///</summary>
        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "ABNORMALREASONTYPEID", "ABNORMALREASONTYPENAME" });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }

}
