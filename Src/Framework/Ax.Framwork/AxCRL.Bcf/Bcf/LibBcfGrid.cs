using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Core.Permission;
using AxCRL.Data;
using AxCRL.Data.SqlBuilder;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Bcf
{
    public class LibBcfGrid : LibBcfDataBase
    {
        public DataSet BrowseTo(LibQueryCondition condition)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Browse);
            if (ret)
            {
                GetQueryData(condition);
                this.AfterChangeData(this.DataSet);
                this.DataSet.AcceptChanges();
            }
            return this.DataSet;
        }

        public DataSet Edit(LibQueryCondition condition)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Edit);
            if (ret)
            {
                GetQueryData(condition);
                this.AfterChangeData(this.DataSet);
                this.DataSet.AcceptChanges();
                LibBillDataCache.Default.AddBillData(this.ProgId, this.DataSet);
            }
            return this.DataSet;
        }

        public DataSet BrowseByPK(object[] pks)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Browse);
            if (ret)
            {
                LibQueryCondition condition = new LibQueryCondition();
                DataColumn[] colums = this.DataSet.Tables[0].PrimaryKey;
                for (int i = 0; i < colums.Length; i++)
                {
                    DataColumn col = colums[i];
                    LibQueryField queryField = new LibQueryField();
                    queryField.Name = col.ColumnName;
                    queryField.QueryChar = LibQueryChar.Equal;
                    queryField.Value.Add(pks[i]);
                    condition.QueryFields.Add(queryField);
                }
                GetQueryData(condition);
                this.AfterChangeData(this.DataSet);
                this.DataSet.AcceptChanges();
            }
            return this.DataSet;
        }

        public bool HasAddRowPermission()
        {
            return CheckHasPermission(FuncPermissionEnum.Add);
        }
        public bool HasDeleteRowPermission()
        {
            return CheckHasPermission(FuncPermissionEnum.Delete);
        }

        protected virtual void BeforeUpdate()
        {

        }

        protected virtual void AfterUpdate()
        {

        }

        protected virtual void AfterCommintData()
        {

        }

        protected virtual void AfterChangeData(DataSet tables)
        {

        }

        public string ExportData(LibQueryCondition condition)
        {
            string fileName = string.Empty;
            bool ret = CheckHasPermission(FuncPermissionEnum.Export);
            if (ret)
            {
                if (condition == null || condition.QueryFields.Count == 0)
                    DataSetManager.GetDataSet(this.DataSet, this.DataAccess, this.ProgId);
                else
                    GetQueryData(condition);
                this.AfterChangeData(this.DataSet);
                fileName = string.Format("{0}-{1}.xls", this.ProgId, LibDateUtils.GetCurrentDateTime());
                string filePath = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "ExportData", fileName);
                AxCRL.Core.Excel.LibExcelHelper libExcelHelper = new Core.Excel.LibExcelHelper();
                libExcelHelper.ExportToExcel(filePath, this.DataSet);
                AfterExportData();
            }
            return fileName;
        }

        public DataSet ImportData(string fileName)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Import);
            if (ret)
            {
                AxCRL.Core.Excel.LibExcelHelper libExcelHelper = new Core.Excel.LibExcelHelper();
                libExcelHelper.ImportToDataSet(System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "ImportData", fileName), this.DataSet, this.AfterImportDataInTable, this.AfterImportDataInRow);
                LibDBTransaction trans = this.DataAccess.BeginTransaction();
                try
                {
                    CheckImportData(this.DataSet);
                    CheckDataHelper.CheckData(this);
                    this.BeforeUpdate();
                    DataSetManager.SubmitData(this.DataSet, this.DataAccess);
                    this.AfterUpdate();
                    if (this.ManagerMessage.IsThrow)
                    {
                        trans.Rollback();
                        this.DataSet.RejectChanges();
                    }
                    else
                    {
                        trans.Commit();
                    }
                }
                catch
                {
                    trans.Rollback();
                    this.DataSet.RejectChanges();
                    throw;
                }
                this.AfterCommintData();
                this.AfterChangeData(this.DataSet);
                this.DataSet.AcceptChanges();
            }
            return this.DataSet;
        }

        public DataSet Save(Dictionary<string, LibChangeRecord> changeRecord, Dictionary<string, string> extendParam = null)
        {
            if (extendParam != null && extendParam.Count != 0)
            {
                foreach (var item in extendParam)
                {
                    ExtendBcfParam[item.Key] = JsonConvert.DeserializeObject(extendParam[item.Key], RegisterBcfParamType[item.Key]);
                }
            }
            this.DataSet = (DataSet)LibBillDataCache.Default.Get(this.ProgId);
            DataSetManager.ChangeDataHandle(this.DataSet, changeRecord, this.ManagerMessage);
            LibBillDataCache.Default.AddBillData(this.ProgId, this.DataSet);
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                CheckDataHelper.CheckData(this);
                this.BeforeUpdate();
                DataSetManager.SubmitData(this.DataSet, this.DataAccess);
                this.AfterUpdate();
                if (this.ManagerMessage.IsThrow)
                {
                    trans.Rollback();
                    this.DataSet.RejectChanges();
                    LibBillDataCache.Default.AddBillData(this.ProgId, this.DataSet);
                }
                else
                {
                    trans.Commit();
                    LibBillDataCache.Default.Remove(this.ProgId);
                }
            }
            catch
            {
                trans.Rollback();
                this.DataSet.RejectChanges();
                LibBillDataCache.Default.AddBillData(this.ProgId, this.DataSet);
                throw;
            }
            this.AfterCommintData();
            this.AfterChangeData(this.DataSet);
            this.DataSet.AcceptChanges();
            return this.DataSet;
        }


        /// <summary>
        /// 移除缓存
        /// </summary>     
        public void RemoveCacheBillData()
        {
            LibBillDataCache.Default.Remove(this.ProgId);
        }

        private string GetFilterCondition()
        {
            string filter = string.Empty;
            if (this.DataSet.Tables[0].ExtendedProperties.ContainsKey(TableProperty.FilterSetting))
            {
                FilterSetting filterSetting = this.DataSet.Tables[0].ExtendedProperties[TableProperty.FilterSetting] as FilterSetting;
                if (filterSetting != null)
                {
                    DataTable table = this.DataSet.Tables[0];
                    if (table.Columns.Contains(filterSetting.Name))
                    {
                        if (table.Columns[filterSetting.Name].ExtendedProperties.ContainsKey(FieldProperty.ControlType))
                        {
                            //TODO需考虑节假日的情况，例如周五看到明天的数据，应该是看到周一的数据
                            LibControlType controlType = (LibControlType)table.Columns[filterSetting.Name].ExtendedProperties[FieldProperty.ControlType];
                            if (controlType == LibControlType.Date)
                            {
                                int currentDate = LibDateUtils.GetCurrentDate();
                                if (filterSetting.Day == 0)
                                {
                                    filter = string.Format("A.{0} = {1}", filterSetting.Name, currentDate);
                                }
                                else
                                {
                                    int otherDate = LibDateUtils.AddDayToLibDate(currentDate, filterSetting.Day);
                                    if (currentDate < otherDate)
                                        filter = string.Format("A.{0} >= {1} and A.{0} <= {2}", filterSetting.Name, currentDate, otherDate);
                                    else
                                        filter = string.Format("A.{0} >= {1} and A.{0} <= {2}", filterSetting.Name, otherDate, currentDate);
                                }
                            }
                            else if (controlType == LibControlType.DateTime)
                            {
                                int currentDate = LibDateUtils.GetCurrentDate();
                                if (filterSetting.Day == 0)
                                {
                                    filter = string.Format("A.{0} >= {1}000000 and A.{0}<={2}999999", filterSetting.Name, currentDate);
                                }
                                else
                                {
                                    int otherDate = LibDateUtils.AddDayToLibDate(currentDate, filterSetting.Day);
                                    if (currentDate < otherDate)
                                        filter = string.Format("A.{0} >= {1}000000 and A.{0} <= {2}999999", filterSetting.Name, currentDate, otherDate);
                                    else
                                        filter = string.Format("A.{0} >= {1}000000 and A.{0} <= {2}999999", filterSetting.Name, otherDate, currentDate);
                                }
                            }
                        }
                    }
                }
            }
            return filter;
        }

        private void GetQueryData(LibQueryCondition condition)
        {
            string conditionStr = string.Empty;
            if (condition != null && condition.QueryFields.Count != 0)
            {
                conditionStr = LibQueryConditionParser.GetQueryData(this.ProgId, condition);
            }
            //else
            //{  //当用户不选择查询条件的时候，使用默认的过滤条件
            //    conditionStr = GetFilterCondition();
            //}
            string powerStr = LibPermissionControl.Default.GetShowCondition(this.Handle, this.ProgId, this.Handle.PersonId);
            if (!string.IsNullOrEmpty(powerStr))
            {
                conditionStr = LibStringBuilder.JoinStringList(new List<string> { conditionStr, powerStr }, "and");
            }
            if (string.IsNullOrEmpty(conditionStr))
            {
                if (condition != null)
                    DataSetManager.GetDataSet(this.DataSet, this.DataAccess, this.ProgId);
            }
            else
            {
                SqlBuilder sqlBuilder = new SqlBuilder(this.ProgId);
                List<string> sqlList = new List<string>();
                LibDataAccess dataAccess = new LibDataAccess();
                for (int i = 0; i < this.DataSet.Tables.Count; i++)
                {
                    string sql = sqlBuilder.GetQuerySql(0, string.Format("{0}.*", (char)(i + (int)'A')), conditionStr);
                    dataAccess.ExecuteDataTable(sql, this.DataSet.Tables[i]);
                }
            }
        }

    }
}
