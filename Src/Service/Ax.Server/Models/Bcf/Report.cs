using Ax.Ui.Models.ModelService;
using AxCRL.Bcf;
using AxCRL.Comm.Bill;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Ax.Ui.Models.Bcf
{
    public class Report
    {
        public static Result GetReportData(string userId, string handle, string progId, QueryField[] queryField = null)
        {
            Result res = new Result();
            res.ReturnValue = true;
            LibHandle Handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
            Service.VerificationHandle(userId, handle, Handle, res);
            try
            {
                if (res.ReturnValue)
                {
                    DataSet ds = new DataSet();
                    if (!string.IsNullOrEmpty(progId))
                    {
                        if (progId.Equals("pls.LogisticsSendRpt"))
                        {
                            LibBcfDailyRpt bcf = (LibBcfDailyRpt)LibBcfSystem.Default.GetBcfInstance(progId);
                            if (bcf == null)
                            {
                                res.ReturnValue = false;
                                res.Message = "该报表不存在!";
                            }
                            else
                            {
                                LibQueryCondition condition = new LibQueryCondition();
                                if (queryField != null)
                                {
                                    foreach (var item in queryField)
                                    {
                                        condition.QueryFields.Add(new AxCRL.Core.Comm.LibQueryField() { Name = item.Name, QueryChar = item.QueryChar, Value = item.Value });
                                    }
                                }
                                else
                                {
                                    condition = null;
                                }
                                bcf.GetData(condition);
                                ds = SetReportData(progId, bcf.DataSet);
                                res.ReturnValue = true;
                                res.Info = ds;
                            }
                        }
                        else
                        {
                            LibBcfRpt bcf = (LibBcfRpt)LibBcfSystem.Default.GetBcfInstance(progId);
                            if (bcf == null)
                            {
                                res.ReturnValue = false;
                                res.Message = "该报表不存在!";
                            }
                            else
                            {
                                LibQueryCondition condition = new LibQueryCondition();
                                if (queryField != null)
                                {
                                    foreach (var item in queryField)
                                    {
                                        condition.QueryFields.Add(new AxCRL.Core.Comm.LibQueryField() { Name = item.Name, QueryChar = item.QueryChar, Value = item.Value });
                                    }
                                }
                                else
                                {
                                    condition = null;
                                }
                                bcf.GetData(condition);
                                ds = SetReportData(progId, bcf.DataSet);
                                res.ReturnValue = true;
                                res.Info = ds;
                            }
                        }

                    }
                    else
                    {
                        res.ReturnValue = false;
                        res.Message = "ProgId不存在！";
                    }
                }
            }
            catch (Exception ex)
            {
                res.ReturnValue = false;
                res.Message = ex.Message;
            }
            return res;
        }


        private static DataSet SetReportData(string progId, DataSet bcfDataSet)
        {
            DataSet newDataSet = new DataSet();
            switch (progId)
            {
                case "pls.WorkLogRpt":
                    newDataSet = SetWorkLogRpt(newDataSet, bcfDataSet);
                    break;
                case "pls.OrderScheduleRpt":
                    newDataSet = SetOrderScheduleRpt(newDataSet, bcfDataSet);
                    break;
                case "pls.OrderUrgentRpt":
                    newDataSet = SetOrderUrgentRpt(newDataSet, bcfDataSet);
                    break;
                case "Stk.AbnormalRpt":
                    newDataSet = SetAbnormalRpt(newDataSet, bcfDataSet);
                    break;
                default:
                    newDataSet = bcfDataSet;
                    break;
            }
            return newDataSet;
        }

        private static DataSet SetWorkLogRpt(DataSet newDataSet, DataSet bcfDataSet)
        {
            Dictionary<int, WorkLogRptModel> dic = new Dictionary<int, WorkLogRptModel>();
            DataTable table = new DataTable();
            table.Columns.Add("STARTDATE", typeof(int));
            table.Columns.Add("STARTONTIMERATE", typeof(decimal));
            table.Columns.Add("ENDONTIMERATE", typeof(decimal));
            newDataSet.Tables.Add(table);
            foreach (DataRow item in bcfDataSet.Tables[0].Rows)
            {
                decimal startOnTimeCount = 0;
                decimal endOnTimeCount = 0;
                int key = LibSysUtils.ToInt32(item["STARTDATE"]);
                if (dic.ContainsKey(key))
                {
                    if (LibSysUtils.ToInt32(item["PLANSTARTTIME"]) > LibSysUtils.ToInt32(item["ACTSTARTTIME"]))
                    {
                        dic[key].StartOnTimeCount++;
                    }
                    if (LibSysUtils.ToInt32(item["PLANENDTIME"]) > LibSysUtils.ToInt32(item["ACTENDTIME"]))
                    {
                        dic[key].EndOnTimeCount++;
                    }
                    dic[key].TotalCount++;
                }
                else
                {
                    if (LibSysUtils.ToInt32(item["PLANSTARTTIME"]) > LibSysUtils.ToInt32(item["ACTSTARTTIME"]))
                    {
                        startOnTimeCount++;
                    }
                    if (LibSysUtils.ToInt32(item["PLANENDTIME"]) > LibSysUtils.ToInt32(item["ACTENDTIME"]))
                    {
                        endOnTimeCount++;
                    }
                    dic.Add(key, new WorkLogRptModel() { StartOnTimeCount = startOnTimeCount, EndOnTimeCount = endOnTimeCount, TotalCount = 1 });

                }
            }
            foreach (KeyValuePair<int, WorkLogRptModel> item in dic)
            {
                DataRow newDataRow = newDataSet.Tables[0].NewRow();
                newDataRow["STARTDATE"] = item.Key;
                newDataRow["STARTONTIMERATE"] = item.Value.StartOnTimeCount / item.Value.TotalCount;
                newDataRow["ENDONTIMERATE"] = item.Value.EndOnTimeCount / item.Value.TotalCount;
                newDataSet.Tables[0].Rows.Add(newDataRow);
            }
            return newDataSet;
        }

        private static DataSet SetOrderScheduleRpt(DataSet newDataSet, DataSet bcfDataSet)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            DataTable table = new DataTable();
            table.Columns.Add("LASTESTDATE", typeof(int));
            table.Columns.Add("ORDERCOUNT", typeof(int));
            //newDataSet.Tables[0].Columns.Add("LASTESTDATE", typeof(int));
            //newDataSet.Tables[0].Columns.Add("ORDERCOUNT", typeof(int));
            newDataSet.Tables.Add(table);
            foreach (DataRow item in bcfDataSet.Tables[0].Rows)
            {
                int key = LibSysUtils.ToInt32(item["LASTESTDATE"]);
                if (dic.ContainsKey(key))
                {

                    dic[key]++;
                }
                else
                {
                    dic.Add(key, 1);
                }
            }
            foreach (KeyValuePair<int, int> item in dic)
            {
                DataRow newDataRow = newDataSet.Tables[0].NewRow();
                newDataRow["LASTESTDATE"] = item.Key;
                newDataRow["ORDERCOUNT"] = item.Value;
                newDataSet.Tables[0].Rows.Add(newDataRow);
            }
            return newDataSet;
        }

        private static DataSet SetOrderUrgentRpt(DataSet newDataSet, DataSet bcfDataSet)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            DataTable table = new DataTable();
            table.Columns.Add("DEALDATE", typeof(int));
            table.Columns.Add("ORDERCOUNT", typeof(int));
            newDataSet.Tables.Add(table);
            newDataSet.Tables.Add(table);
            foreach (DataRow item in bcfDataSet.Tables[0].Rows)
            {
                int key = LibSysUtils.ToInt32(item["DEALDATE"]);
                if (dic.ContainsKey(key))
                {

                    dic[key]++;
                }
                else
                {
                    dic.Add(key, 1);
                }
            }
            foreach (KeyValuePair<int, int> item in dic)
            {
                DataRow newDataRow = newDataSet.Tables[0].NewRow();
                newDataRow["DEALDATE"] = item.Key;
                newDataRow["ORDERCOUNT"] = item.Value;
                newDataSet.Tables[0].Rows.Add(newDataRow);
            }
            return newDataSet;
        }

        private static DataSet SetAbnormalRpt(DataSet newDataSet, DataSet bcfDataSet)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            DataTable table = new DataTable();
            table.Columns.Add("ABNORMALPROTOTYPE", typeof(int));
            table.Columns.Add("ORDERCOUNT", typeof(int));
            newDataSet.Tables.Add(table);
            foreach (DataRow item in bcfDataSet.Tables[0].Rows)
            {
                int key = LibSysUtils.ToInt32(item["ABNORMALPROTOTYPE"]);
                if (dic.ContainsKey(key))
                {
                    dic[key]++;
                }
                else
                {
                    dic.Add(key, 1);
                }
            }
            foreach (KeyValuePair<int, int> item in dic)
            {
                DataRow newDataRow = newDataSet.Tables[0].NewRow();
                newDataRow["ABNORMALPROTOTYPE"] = item.Key;
                newDataRow["ORDERCOUNT"] = item.Value;
                newDataSet.Tables[0].Rows.Add(newDataRow);
            }
            return newDataSet;
        }

        public class WorkLogRptModel
        {
            private decimal totalCount = 0;

            public decimal TotalCount
            {
                get { return totalCount; }
                set { totalCount = value; }
            }
            private decimal startOnTimeCount = 0;

            public decimal StartOnTimeCount
            {
                get { return startOnTimeCount; }
                set { startOnTimeCount = value; }
            }
            private decimal endOnTimeCount = 0;

            public decimal EndOnTimeCount
            {
                get { return endOnTimeCount; }
                set { endOnTimeCount = value; }
            }
        }

    }
}