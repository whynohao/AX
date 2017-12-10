using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Axp.SystemBcf
{
    [ProgId(ProgId = "Axp.ValidityCalc", ProgIdType = ProgIdType.Bcf,VclPath="/Scripts/module/mes/axp/AxpValidityCalcVcl.js")]
    public class AxpValidityCalcBcf:LibBcfDataFunc,ILibLiveUpdate
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpValidityCalcBcfTemplate("Axp.ValidityCalc");
        }

        /// <summary>
        /// 手动排程
        /// </summary>
        /// <returns></returns>
        public void funSchedule()
        {
            string sql = string.Empty;
            List<string> list = new List<string>();
            int date = LibSysUtils.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
            sql = "SELECT PROGID FROM AXPFUNCLIST WHERE BILLTYPE=0";
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read()) {
                    list.Add(string.Format(@"UPDATE {0} SET ISVALIDITY=1 WHERE VALIDITYSTARTDATE<={1} AND (VALIDITYENDDATE>={1} OR VALIDITYENDDATE=0)",
                        LibSqlModelCache.Default.GetSqlModel(LibSysUtils.ToString(reader["PROGID"])).Tables[0].TableName, date));
                    list.Add(string.Format(@"UPDATE {0} SET ISVALIDITY=0 WHERE VALIDITYSTARTDATE>{1} OR (VALIDITYENDDATE<{1} AND VALIDITYENDDATE>0)",
                        LibSqlModelCache.Default.GetSqlModel(LibSysUtils.ToString(reader["PROGID"])).Tables[0].TableName, date));
                }
            }
            this.DataAccess.ExecuteDataTables(list, this.DataSet);
        }

        [LibBusinessTask(Name = "LiveUpdate", DisplayText = "实时更新")]
        public DataSet LiveUpdate()
        {
            string sql = string.Empty;
            sql = "SELECT PROGID FROM AXPFUNCLIST WHERE BILLTYPE=0";
            StringBuilder builder = new StringBuilder();
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read()) {
                    builder.Append(string.Format("SELECT COUNT(VALIDITYSTARTDATE) AS NUM,'{0}' AS TABLENAME FROM {0} WHERE VALIDITYSTARTDATE>0 OR VALIDITYENDDATE>0 UNION ALL ",
                         LibSqlModelCache.Default.GetSqlModel(LibSysUtils.ToString(reader["PROGID"])).Tables[0].TableName));
                }
            }
            if (builder.Length > 0)
            {
                sql=builder.Remove(builder.Length-10,10).ToString();
            }
            List<string> list = new List<string>();
            int date = LibSysUtils.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read()) 
                {
                    if (LibSysUtils.ToInt32(reader["NUM"]) > 0)
                    {
                        list.Add(string.Format("UPDATE {0} SET ISVALIDITY=1 WHERE VALIDITYSTARTDATE<={1} AND (VALIDITYENDDATE>={1} OR VALIDITYENDDATE=0) AND ISVALIDITY=0", LibSysUtils.ToString(reader["TABLENAME"]), date));
                        list.Add(string.Format("UPDATE {0} SET ISVALIDITY=0 WHERE VALIDITYENDDATE<{1} AND VALIDITYENDDATE>0 AND ISVALIDITY=1", LibSysUtils.ToString(reader["TABLENAME"]), date));
                    }
                }
            }
            if (list.Count > 0)
            {
                this.DataAccess.ExecuteDataTables(list, this.DataSet);
            }
            return this.DataSet;
        }
    }

    public class AxpValidityCalcBcfTemplate : LibTemplate
    {
        public AxpValidityCalcBcfTemplate(string progId)
            : base(progId, BillType.DataFunc, "主数据有效期排程") { }
        private const string tableName = "AXPVALIDITYCALC";
        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            DataTable headTable = new DataTable();
            DataSourceHelper.AddColumn(new DefineField(headTable, "KEY", "主键", FieldSize.Size20));
            headTable.PrimaryKey = new DataColumn[] { headTable.Columns["KEY"] };
            this.DataSet.Tables.Add(headTable);
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>());//{ "KEY" }
            layout.ButtonRange = layout.BuildButton(new List<FunButton> { 
                new FunButton("btnSchedule", "手动排程") });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
