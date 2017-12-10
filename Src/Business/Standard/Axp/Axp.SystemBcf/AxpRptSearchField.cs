using AxCRL.Bcf;
using AxCRL.Comm.Define;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxCRL.Template;
using System.Data;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;
using AxCRL.Comm.Utils;

namespace Axp.SystemBcf
{
    [ProgId(ProgId = "axp.RptSearchField", ProgIdType=ProgIdType.Bcf, VclPath = @"/Scripts/module/mes/axp/axpRptSearchFieldVcl.js")]
    public class AxpRptSearchFieldBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpRptSearchFieldBcfTemplate("axp.RptSearchField");
        }

        protected override void BeforeUpdate()
        {
            base.BeforeUpdate();
            DataTable dt = this.DataSet.Tables[1];
            var master = this.DataSet.Tables[0].Rows[0];
            List<string> lists = new List<string>();
            //string sqlById = string.Format("select count(*) from AXPRPTSEARCHFIELD where RPTSEARCHID = {0}", LibStringBuilder.GetQuotObject(master["RPTSEARCHID"]));
            //int num = LibSysUtils.ToInt32(DataAccess.ExecuteScalar(sqlById));
            //if (num > 0)
            //{
            //    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("条件代码已经存在"));
            //}
            int count = 1;
            int index = -1;
            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState != DataRowState.Added && row.RowState != DataRowState.Modified && row.RowState != DataRowState.Unchanged)
                {
                    continue;
                }
                string field = row["SEARCHFIELD"].ToString();
                if ((index = lists.IndexOf(field))>=0){
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("选择字段不能相同, 但第{0}行和第{1}行相同了", index + 1,count));
                }
                else
                {
                    lists.Add(field);
                }
                count++;
            }
            if (this.ManagerMessage.IsThrow)
            {
                return;
            }
            // 删除 该用户的 该功能的 数据库
            if (master.RowState == DataRowState.Added)
            {
                List<string> rptSearchId = new List<string>();
                string sql = string.Format("select RPTSEARCHID from AXPRPTSEARCHFIELD where PROGID={0} and USERID={1}", LibStringBuilder.GetQuotObject(master["PROGID"]), LibStringBuilder.GetQuotObject(master["USERID"]));
                using (var dr = DataAccess.ExecuteDataReader(sql))
                {
                    while (dr.Read())
                    {
                        rptSearchId.Add(dr["RPTSEARCHID"].ToString());
                    }
                }
                if (rptSearchId.Count > 0)
                {
                    List<string> sqls = new List<string>();
                    foreach (var item in rptSearchId)
                    {
                        string delSql = string.Format("delete AXPRPTSEARCHFIELD where RPTSEARCHID = {0}", LibStringBuilder.GetQuotString(item));
                        string delSql_child = string.Format("delete AXPRPTSEARCHFIELDDETAIL where RPTSEARCHID = {0}", LibStringBuilder.GetQuotString(item));
                        sqls.Add(delSql);
                        sqls.Add(delSql_child);
                    }
                    DataAccess.ExecuteNonQuery(sqls);
                }
            }
        }

        public bool IsExists(string pk)
        {
            string sqlById = string.Format("select count(*) from AXPRPTSEARCHFIELD where RPTSEARCHID = {0}", LibStringBuilder.GetQuotString(pk));
            int num = LibSysUtils.ToInt32(DataAccess.ExecuteScalar(sqlById));
            if (num > 0)
            {
                return true;
            }
            return false;
        }
    }

    public class AxpRptSearchFieldBcfTemplate : LibTemplate
    {
        public AxpRptSearchFieldBcfTemplate(string progId) : base(progId, BillType.Master, "报表头查询参数设置")
        {
        }

        private const string masterTableName = "AXPRPTSEARCHFIELD";
        private const string bodyTableName = "AXPRPTSEARCHFIELDDETAIL";

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, "RPTSEARCHID", "条件代码", FieldSize.Size20) { AllowCopy = false, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PROGID", "功能代码", FieldSize.Size50)
            {
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() {
                 new RelativeSource("axp.FuncList"){  RelFields = new RelFieldCollection()
                     {
                        new RelField("PROGNAME", LibDataType.NText, FieldSize.Size50, "功能名称")
                     }
                 }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISON", "是否启用") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "USERID", "账户ID", FieldSize.Size20) { DataType = LibDataType.Text, ControlType = LibControlType.NText });
            DataSourceHelper.AddFixColumn(masterTable, this.BillType);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns["RPTSEARCHID"] };
            this.DataSet.Tables.Add(masterTable);

            DataTable bodyTable = new DataTable(bodyTableName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "RPTSEARCHID", "条件代码", FieldSize.Size20));
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "SEARCHFIELD", "选择字段", FieldSize.Size50)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource() { }
                }
            });
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns["RPTSEARCHID"], bodyTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", masterTableName, bodyTableName), masterTable.Columns["RPTSEARCHID"], bodyTable.Columns["RPTSEARCHID"]);
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "RPTSEARCHID", "PROGID", "PROGNAME", "ISON" });
            layout.TabRange.Add(layout.BuildGrid(1, "设置查询字段"));
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
