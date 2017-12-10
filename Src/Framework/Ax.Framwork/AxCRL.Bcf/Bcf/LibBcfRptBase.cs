using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;
using AxCRL.Core.Comm;
using AxCRL.Core.Permission;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AxCRL.Bcf
{
    public class LibBcfRptBase : LibBcfBase
    {

        [LibBusinessTaskAttribute(Name = "GetData", DisplayText = "报表查询")]
        public DataSet GetData(LibQueryCondition condition)
        {
            Dictionary<string, List<LibQueryField>> powerCondition = LibPermissionControl.Default.GetQueryCondition(this.Handle, this.ProgId);
            if (powerCondition != null)
            {
                condition = LibQueryConditionParser.MergeQueryCondition(this.DataSet.Tables[0], condition, powerCondition);
            }
            return InnerGetData(condition);
        }

        protected virtual DataSet InnerGetData(LibQueryCondition condition)
        {
            return this.DataSet;
        }


        public string ExportData(LibQueryCondition condition)
        {
            string fileName = string.Empty;
            bool ret = CheckHasPermission(FuncPermissionEnum.Export);
            if (ret)
            {
                this.GetData(condition);
                fileName = string.Format("{0}-{1}.xls", this.ProgId, LibDateUtils.GetCurrentDateTime());
                string filePath = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "ExportData", fileName);
                AxCRL.Core.Excel.LibExcelHelper libExcelHelper = new Core.Excel.LibExcelHelper();
                libExcelHelper.ExportToExcel(filePath, this.DataSet);
            }
            return fileName;
        }

        public string ExportDataX(DataSet data)
        {
            string fileName = string.Empty;
            this.DataSet.EnforceConstraints = false;
            DataTable dt = this.DataSet.Tables[0];
            DataTable subDt = this.DataSet.Tables[1];
            try
            {
                dt.BeginLoadData();
                subDt.BeginLoadData();
                foreach (DataRow currentRow in data.Tables[0].Rows)
                {
                    DataRow newRow = dt.NewRow();
                    newRow.BeginEdit();
                    foreach(DataColumn col in data.Tables[0].Columns)
                    {
                        newRow[col.ColumnName] = currentRow[col.ColumnName];
                    }
                    newRow.EndEdit();
                    dt.Rows.Add(newRow);
                }
                foreach (DataRow currentRow in data.Tables[1].Rows)
                {
                    DataRow newRow = subDt.NewRow();
                    newRow.BeginEdit();
                    foreach (DataColumn col in data.Tables[1].Columns)
                    {
                        newRow[col.ColumnName] = currentRow[col.ColumnName];
                    }
                    newRow.EndEdit();
                    subDt.Rows.Add(newRow);
                }
            }
            finally
            {
                dt.EndLoadData();
                subDt.EndLoadData();
                this.DataSet.EnforceConstraints = true;
            }
            fileName = string.Format("{0}-{1}.xls", this.ProgId, string.Format("{0}{1}", "UnitInfo", LibDateUtils.GetCurrentDateTime()));
            string filePath = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "ExportData", fileName);
            AxCRL.Core.Excel.LibExcelHelper libExcelHelper = new Core.Excel.LibExcelHelper();
            libExcelHelper.ExportToExcel(filePath, this.DataSet);
            //}
            return fileName;
        }

        public string ExportMainData(LibQueryCondition condition)
        {
            string fileName = string.Empty;
            bool ret = CheckHasPermission(FuncPermissionEnum.Export);
            if (ret)
            {
                this.GetData(condition);
                fileName = string.Format("{0}-{1}.xls", this.ProgId, LibDateUtils.GetCurrentDateTime());
                string filePath = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "ExportData", fileName);
                AxCRL.Core.Excel.LibExcelHelper libExcelHelper = new Core.Excel.LibExcelHelper();
                libExcelHelper.ExportToExcel(filePath, this.DataSet, new HashSet<int>() { 0 });
            }
            return fileName;
        }

        public virtual DataSet GetScheduleTaskData(string execTaskDataId)
        {
            string sql = string.Format("select RESULTDATA from AXAEXECTASKDATA where EXECTASKDATAID={0}", LibStringBuilder.GetQuotString(execTaskDataId));
            string data = LibSysUtils.ToString(this.DataAccess.ExecuteScalar(sql));
            if (!string.IsNullOrEmpty(data))
            {
                LibBillDataSerializeHelper.Deserialize(data, this.DataSet);
            }
            return this.DataSet;
        }

    }


}
