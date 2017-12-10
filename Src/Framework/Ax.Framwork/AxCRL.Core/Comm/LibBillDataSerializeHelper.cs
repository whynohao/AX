using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Comm
{
    public static class LibBillDataSerializeHelper
    {
        public static string Serialize(DataSet dataSet)
        {
            Dictionary<string, List<Dictionary<string, object>>> billData = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (DataTable table in dataSet.Tables)
            {
                List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
                foreach (DataRow dataRow in table.Rows)
                {
                    if (dataRow.RowState == DataRowState.Deleted)
                        continue;
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    foreach (DataColumn col in table.Columns)
                    {
                        dic.Add(col.ColumnName, dataRow[col]);
                    }
                    list.Add(dic);
                }
                billData.Add(table.TableName, list);
            }
            return JsonConvert.SerializeObject(billData);
        }

        public static void Deserialize(string data, DataSet dataSet)
        {
            Dictionary<string, List<Dictionary<string, object>>> destObj = JsonConvert.DeserializeObject(data, typeof(Dictionary<string, List<Dictionary<string, object>>>)) as Dictionary<string, List<Dictionary<string, object>>>;
            if (destObj != null)
            {
                dataSet.EnforceConstraints = false;
                try
                {
                    foreach (DataTable curTable in dataSet.Tables)
                    {
                        string tableName = curTable.TableName;
                        if (destObj.ContainsKey(tableName))
                        {
                            List<Dictionary<string, object>> list = destObj[tableName];
                            if (list.Count > 0)
                            {
                                curTable.BeginLoadData();
                                try
                                {
                                    foreach (Dictionary<string, object> item in list)
                                    {
                                        DataRow newRow = curTable.NewRow();
                                        newRow.BeginEdit();
                                        try
                                        {
                                            foreach (KeyValuePair<string, object> subItem in item)
                                            {
                                                if (curTable.Columns.Contains(subItem.Key))
                                                {
                                                    newRow[subItem.Key] = subItem.Value;
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            newRow.EndEdit();
                                        }
                                        curTable.Rows.Add(newRow);
                                    }
                                }
                                finally
                                {
                                    curTable.EndLoadData();
                                }
                            }
                        }
                    }
                }
                finally
                {
                    dataSet.EnforceConstraints = true;
                }
            }
        }
    }
}
