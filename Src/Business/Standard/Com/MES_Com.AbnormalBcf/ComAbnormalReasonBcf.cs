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
    [ProgId(ProgId = "com.AbnormalReason", ProgIdType = ProgIdType.Bcf)]
    public class ComAbnormalReasonBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new ComAbnormalReasonBcfTemplate("com.AbnormalReason");
        }
    }
     public class ComAbnormalReasonBcfTemplate : LibTemplate
     {
         private const string tableName = "COMABNORMALREASON";
         public ComAbnormalReasonBcfTemplate(string progId)
             : base(progId, BillType.Master, "异常原因")
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
             DataSourceHelper.AddColumn(new DefineField(comType, "ABNORMALREASONID", "原因代码", FieldSize.Size20) { AllowCopy = false, AllowEmpty = false, DataType = LibDataType.Text });
             DataSourceHelper.AddColumn(new DefineField(comType, "ABNORMALREASONNAME", "原因名称", FieldSize.Size50) { AllowEmpty = false, DataType = LibDataType.NText });
             DataSourceHelper.AddColumn(new DefineField(comType, "ABNORMALREASONTYPEID", "原因类别", FieldSize.Size20)
             {
                 AllowEmpty = false,
                 DataType=LibDataType.Text,
                 RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.AbnormalReasonType")
                    {
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("ABNORMALREASONTYPENAME", LibDataType.NText,FieldSize.Size50,"原因类别名称")
                         } 
                    }
                }
             });
             DataSourceHelper.AddFixColumn(comType, this.BillType);//系统自动创建的内容
             comType.PrimaryKey = new DataColumn[] { comType.Columns["ABNORMALREASONID"] };//定义表的主键
             this.DataSet.Tables.Add(comType);
         }
         ///<summary>
         ///页面排版模型
         ///</summary>
         protected override void DefineViewTemplate(DataSet dataSet)
         {
             LibBillLayout layout = new LibBillLayout(this.DataSet);
             layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "ABNORMALREASONID", "ABNORMALREASONNAME", "ABNORMALREASONTYPEID" });
             this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
         }
     }
   
}
