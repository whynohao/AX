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
    [ProgId(ProgId = "com.LiaisonType", ProgIdType = ProgIdType.Bcf)]
    public class ComLiaisonTypeBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new ComLiaisonTypeBcfTemplate("com.LiaisonType");
        }
    }
    public class ComLiaisonTypeBcfTemplate : LibTemplate
    {
        private const string tableName = "COMLIAISONTYPE";
        public ComLiaisonTypeBcfTemplate(string progId)
            : base(progId, BillType.Master, "联络单单据类型")
        {
        }
        ///<summary>
        ///构建数据库模型
        ///</summary>
        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable table = new DataTable(tableName);
            //构建表结构
            DataSourceHelper.AddColumn(new DefineField(table, "TYPEID", "单据类型代码", FieldSize.Size50) { DataType = LibDataType.Text, AllowCopy = false, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(table, "TYPENAME", "单据类型名称", FieldSize.Size20) { DataType = LibDataType.NText, AllowEmpty = false });
            DataSourceHelper.AddDefaultCreateState(table);
            DataSourceHelper.AddFixColumn(table, this.BillType);
            table.PrimaryKey = new DataColumn[] { table.Columns["TYPEID"] };
            this.DataSet.Tables.Add(table);
        }
        ///<summary>
        ///页面排版模型
        ///</summary>
        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "TYPEID", "TYPENAME", "DEFAULTCREATESTATE" });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }

        protected override void DefineFuncPermission()
        {
            base.DefineFuncPermission();
            this.FuncPermission.UsingCache = true;
        }
    }

}
