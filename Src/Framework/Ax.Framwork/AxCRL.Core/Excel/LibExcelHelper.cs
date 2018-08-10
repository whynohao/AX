using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using AxCRL.Comm.Utils;
using AxCRL.Template.DataSource;
using AxCRL.Template;
using System.Xml;
using System.IO;
using System.Diagnostics;
//using Microsoft.Office.Interop.Excel;
using System.Reflection;




namespace AxCRL.Core.Excel
{
    public class LibExcelHelper
    {
        public void ImportToDataSet(string filePath, DataSet dataSet, Action<System.Data.DataTable> actionTable, Func<DataRow, Dictionary<string, object>, bool> actionRow, bool dbField = false)
        {
            try
            {
                string connStr = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source='{0}'; Extended Properties='Excel 8.0;HDR=Yes;IMEX=2'", filePath);
                List<string> sqlList = new List<string>();
                foreach (System.Data.DataTable table in dataSet.Tables)
                {
                    string tableName = string.Empty;
                    if (dbField)
                        tableName = table.TableName;
                    else
                    {
                        if (table.ExtendedProperties.ContainsKey(TableProperty.DisplayText))
                            tableName = LibSysUtils.ToString(table.ExtendedProperties[TableProperty.DisplayText]);
                        else
                            tableName = table.TableName;
                    }
                    sqlList.Add(string.Format("select * from [{0}$]", tableName));
                }
                dataSet.EnforceConstraints = false;
                using (OleDbConnection conn = new OleDbConnection(connStr))
                {
                    conn.Open();
                    try
                    {
                        int i = 0;
                        foreach (string sql in sqlList)
                        {
                            System.Data.DataTable curTable = dataSet.Tables[i];
                            Dictionary<string, DataColumn> colMap = null;
                            if (!dbField)
                            {
                                colMap = new Dictionary<string, DataColumn>();
                                foreach (DataColumn col in curTable.Columns)
                                {
                                    try
                                    {
                                        colMap.Add(col.Caption, col);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw ex;
                                    }
                                }
                            }
                            curTable.BeginLoadData();
                            try
                            {
                                using (OleDbCommand command = new OleDbCommand(sql, conn))
                                {
                                    using (IDataReader reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            bool canAdd = true;
                                            Dictionary<string, object> otherValueList = null;
                                            DataRow newRow = curTable.NewRow();
                                            newRow.BeginEdit();
                                            try
                                            {
                                                bool allNull = true;
                                                for (int l = 0; l < reader.FieldCount; l++)
                                                {
                                                    string colName = reader.GetName(l).Trim();
                                                    if (!Convert.IsDBNull(reader[l]))
                                                    {
                                                        if (allNull)
                                                            allNull = false;
                                                        string realName = string.Empty;
                                                        if (dbField)
                                                        {
                                                            if (curTable.Columns.Contains(colName))
                                                            {
                                                                realName = colName;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (colMap.ContainsKey(colName))
                                                            {
                                                                realName = colMap[colName].ColumnName;
                                                            }
                                                        }
                                                        if (!string.IsNullOrEmpty(realName))
                                                        {
                                                            LibControlType controlType = (LibControlType)colMap[colName].ExtendedProperties[FieldProperty.ControlType];
                                                            if (controlType == LibControlType.DateTime || controlType == LibControlType.Date)
                                                            {
                                                                DateTime curTime;
                                                                DateTime.TryParse(LibSysUtils.ToString(reader[l]), out curTime);
                                                                if (controlType == LibControlType.Date)
                                                                {
                                                                    newRow[realName] = LibDateUtils.DateTimeToLibDate(curTime);
                                                                }
                                                                else if (controlType == LibControlType.DateTime)
                                                                {
                                                                    newRow[realName] = LibDateUtils.DateTimeToLibDateTime(curTime);
                                                                }
                                                            }
                                                            else if (controlType == LibControlType.HourMinute)
                                                            {
                                                                string hourtime = LibSysUtils.ToString(reader[l]);
                                                                hourtime = hourtime.Replace(":", "");
                                                                newRow[realName] = LibSysUtils.ToInt32(hourtime);
                                                            }
                                                            else if (controlType == LibControlType.Text || controlType == LibControlType.NText)
                                                            {
                                                                newRow[realName] = LibSysUtils.ToString(reader[l]).Trim();
                                                            }
                                                            else
                                                                newRow[realName] = reader[l];
                                                        }
                                                        else
                                                        {
                                                            if (otherValueList == null)
                                                                otherValueList = new Dictionary<string, object>();
                                                            if (!otherValueList.ContainsKey(colName))
                                                                otherValueList.Add(colName, reader[l]);
                                                        }
                                                    }
                                                }
                                                canAdd = !allNull; //全为null的行是空行不需要导入
                                                if (canAdd)
                                                    canAdd = actionRow(newRow, otherValueList);
                                            }
                                            finally
                                            {
                                                newRow.EndEdit();
                                            }
                                            if (canAdd)
                                                curTable.Rows.Add(newRow);
                                        }
                                    }
                                    actionTable(curTable);

                                }
                            }
                            finally
                            {
                                curTable.EndLoadData();
                            }
                            i++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Exception ex1 = ex;
                    }
                    finally
                    {
                        conn.Close();
                        dataSet.EnforceConstraints = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (dataSet.HasErrors)
                {
                    string errorData = string.Empty;
                    foreach (DataTable dt in dataSet.Tables)
                    {
                        foreach (DataRow dr in dt.GetErrors())
                        {
                            errorData += string.Format(" {0}",dr.RowError);
                        }
                    }
                    Exception dsEx = new Exception(errorData);
                    LibLog.WriteLog(dsEx);
                }

                LibLog.WriteLog(ex);
                throw;
            }
        }

        public void ExportToExcel(string filePath, DataSet dataSet, HashSet<int> tableIndex = null, bool dbField = false)
        {
            try
            {
                IList<string> dmlSqlList = new List<string>();
                IList<string> sqlList = new List<string>();
                for (int index = 0; index < dataSet.Tables.Count; index++)
                {
                    if (tableIndex != null && !tableIndex.Contains(index))
                        continue;
                    System.Data.DataTable table = dataSet.Tables[index];
                    //如果存在文本列名相同时则需要此结构
                    Dictionary<string, int> sameColDic = null;
                    if (!dbField)
                    {
                        sameColDic = new Dictionary<string, int>();
                    }
                    string columnStr = string.Empty;
                    StringBuilder columnDefineBuilder = new StringBuilder();
                    StringBuilder columnBuilder = new StringBuilder();
                    foreach (DataColumn col in table.Columns)
                    {
                        string name = dbField ? col.ColumnName : string.IsNullOrEmpty(col.Caption) ? col.ColumnName : col.Caption;
                        if (sameColDic.ContainsKey(name))
                        {
                            sameColDic[name]++;
                            name += sameColDic[name].ToString();
                        }
                        else
                            sameColDic.Add(name, 0);
                        columnBuilder.AppendFormat("{0},", name);
                        LibDataType dataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                        switch (dataType)
                        {
                            case LibDataType.Text:
                            case LibDataType.NText:
                                //columnDefineBuilder.AppendFormat("{0} String,", name);
                                columnDefineBuilder.AppendFormat("{0} memo,", name);
                                break;
                            case LibDataType.Int32:
                                LibControlType ctrlType = (LibControlType)col.ExtendedProperties[FieldProperty.ControlType];
                                if (ctrlType == LibControlType.Date)
                                    columnDefineBuilder.AppendFormat("{0} Date,", name);
                                else if (ctrlType == LibControlType.HourMinute)
                                    columnDefineBuilder.AppendFormat("{0} String,", name);
                                else
                                    columnDefineBuilder.AppendFormat("{0} Integer,", name);
                                break;
                            case LibDataType.Int64:
                                ctrlType = (LibControlType)col.ExtendedProperties[FieldProperty.ControlType];
                                if (ctrlType == LibControlType.DateTime)
                                    columnDefineBuilder.AppendFormat("{0} DateTime,", name);
                                else
                                    columnDefineBuilder.AppendFormat("{0} Long,", name);
                                break;
                            case LibDataType.Numeric:
                                columnDefineBuilder.AppendFormat("{0} Currency,", name);
                                break;
                            case LibDataType.Float:
                                columnDefineBuilder.AppendFormat("{0} Single,", name);
                                break;
                            case LibDataType.Double:
                                columnDefineBuilder.AppendFormat("{0} Double,", name);
                                break;
                            case LibDataType.Byte:
                                columnDefineBuilder.AppendFormat("{0} Integer,", name);
                                break;
                            case LibDataType.Boolean:
                                columnDefineBuilder.AppendFormat("{0} Integer,", name);
                                break;
                            case LibDataType.Binary:
                                columnDefineBuilder.AppendFormat("{0} memo,", name);
                                break;
                        }
                    }
                    if (columnBuilder.Length > 0)
                    {
                        columnBuilder.Remove(columnBuilder.Length - 1, 1);
                        columnDefineBuilder.Remove(columnDefineBuilder.Length - 1, 1);
                    }
                    columnStr = columnBuilder.ToString();
                    string tableName = string.Empty;
                    if (dbField)
                        tableName = table.TableName;
                    else
                    {
                        if (table.ExtendedProperties.ContainsKey(TableProperty.DisplayText))
                            tableName = LibSysUtils.ToString(table.ExtendedProperties[TableProperty.DisplayText]);
                        else
                            tableName = table.TableName;
                    }
                    dmlSqlList.Add(string.Format("CREATE TABLE {0} ({1})", tableName, columnDefineBuilder.ToString()));
                    foreach (DataRow curRow in table.Rows)
                    {
                        if (curRow.RowState == DataRowState.Deleted)
                            continue;
                        StringBuilder builder = new StringBuilder();
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            DataColumn col = table.Columns[i];
                            LibDataType dataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                            LibControlType ctrlType = (LibControlType)col.ExtendedProperties[FieldProperty.ControlType];
                            switch (dataType)
                            {
                                case LibDataType.Text:
                                case LibDataType.NText:
                                case LibDataType.Binary:
                                case LibDataType.Int64:
                                    if (dataType == LibDataType.Int64 && ctrlType == LibControlType.DateTime)
                                    {
                                        long dateTime = LibSysUtils.ToInt64(curRow[col]);
                                        if (dateTime != 0)
                                            builder.AppendFormat("{0},", LibStringBuilder.GetQuotObject(LibDateUtils.LibDateToDateTime(dateTime).ToString("yyyy-MM-dd HH:mm:ss")));
                                        else
                                            builder.Append("null,");
                                    }
                                    else
                                        builder.AppendFormat("{0},", LibStringBuilder.GetQuotObject(curRow[col]));
                                    break;
                                case LibDataType.Int32:
                                case LibDataType.Numeric:
                                case LibDataType.Float:
                                case LibDataType.Double:
                                case LibDataType.Byte:
                                    if (dataType == LibDataType.Int32 && ctrlType == LibControlType.Date)
                                    {
                                        int date = LibSysUtils.ToInt32(curRow[col]);
                                        if (date != 0)
                                            builder.AppendFormat("{0},", LibStringBuilder.GetQuotObject(LibDateUtils.LibDateToDateTime(date).ToLongDateString()));
                                        else
                                            builder.Append("null,");
                                    }
                                    else if (dataType == LibDataType.Int32 && ctrlType == LibControlType.HourMinute)
                                    {
                                        string time = LibSysUtils.ToString(curRow[col]);
                                        switch (time.Length)
                                        {
                                            case 1: time = "000" + time + "00"; break;
                                            case 2: time = "00" + time + "00"; break;
                                            case 3: time = "0" + time + "00"; break;
                                            case 4: time = time + "00"; break;
                                            default: time = time + "00"; break;
                                        }
                                        time = "20150101" + time;
                                        builder.AppendFormat("{0},", LibStringBuilder.GetQuotObject(LibDateUtils.LibDateToDateTime(LibSysUtils.ToInt64(time)).ToString("HH:mm")));
                                    }
                                    else
                                        builder.AppendFormat("{0},", curRow[col]);
                                    break;
                                case LibDataType.Boolean:
                                    builder.AppendFormat("{0},", LibSysUtils.ToBoolean(curRow[col.ColumnName]) ? 1 : 0);
                                    break;
                            }
                        }
                        if (builder.Length > 0)
                            builder.Remove(builder.Length - 1, 1);
                        sqlList.Add(string.Format("insert into {0}({1}) values({2})", tableName, columnBuilder, builder.ToString()));
                    }
                }

                string connStr = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source='{0}'; Extended Properties='Excel 8.0;HDR=Yes;IMEX=2,ReadOnly=False'", filePath);
                using (OleDbConnection conn = new OleDbConnection(connStr))
                {
                    conn.Open();
                    try
                    {
                        foreach (string sql in dmlSqlList)
                        {
                            using (OleDbCommand command = new OleDbCommand(sql, conn))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                        foreach (string sql in sqlList)
                        {
                            using (OleDbCommand command = new OleDbCommand(sql, conn))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

            }
            catch (Exception ex)
            {
                string path = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output", "Error", "Excel", string.Format("{0}.txt", DateTime.Now.Ticks));
                using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                    {
                        sw.Write(ex);
                    }
                }
                throw;
            }
        }

        public void ExportRadXMLData(string filePath, DataSet dataSet, HashSet<int> tableIndex = null, bool dbField = false)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            #region 文件名，路径
            string templateFile = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "ExcelModel", "List.xml");
            String bodyXML = File.ReadAllText(templateFile, Encoding.UTF8);
            #endregion
            StringBuilder names = new StringBuilder("");
            StringBuilder Worksheet = new StringBuilder("");
            for (int index = 0; index < dataSet.Tables.Count; index++)
            {
                StringBuilder headCols = new StringBuilder("");
                StringBuilder rows = new StringBuilder("");
                if (tableIndex != null && !tableIndex.Contains(index))
                    continue;
                System.Data.DataTable dt = dataSet.Tables[index];
                string tableName = string.Empty;
                if (dbField)
                    tableName = dt.TableName;
                else
                {
                    if (dt.ExtendedProperties.ContainsKey(TableProperty.DisplayText))
                        tableName = LibSysUtils.ToString(dt.ExtendedProperties[TableProperty.DisplayText]);
                    else
                        tableName = dt.TableName;
                }
                names.AppendLine(String.Format("<NamedRange ss:Name=\"{0}\" ss:RefersTo=\"={0}!R1C1:R{1}C{2}\"/>", tableName, (dt.Rows.Count + 1).ToString(), dt.Columns.Count.ToString()));
                //如果存在文本列名相同时则需要此结构
                Dictionary<string, int> sameColDic = null;
                if (!dbField)
                {
                    sameColDic = new Dictionary<string, int>();
                }
                #region 填充表头
                foreach (DataColumn col in dt.Columns)
                {
                    string name = dbField ? col.ColumnName : string.IsNullOrEmpty(col.Caption) ? col.ColumnName : col.Caption;
                    if (sameColDic.ContainsKey(name))
                    {
                        sameColDic[name]++;
                        name += sameColDic[name].ToString();
                    }
                    else
                        sameColDic.Add(name, 0);
                    headCols.Append(String.Format("<Cell><Data ss:Type=\"String\">{0}</Data><NamedCell ss:Name=\"{1}\"/></Cell>\r\n", name, tableName));
                }
                #endregion
                #region 表格具体内容
                string type = string.Empty;
                object value = string.Empty;
                foreach (DataRow curRow in dt.Rows)
                {
                    if (curRow.RowState == DataRowState.Deleted)
                        continue;
                    StringBuilder builder = new StringBuilder();
                    #region 填充行的格式
                    rows.Append("<Row>\r\n");
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        string style = string.Empty;
                        #region 填充的值和类型
                        DataColumn col = dt.Columns[i];
                        LibDataType dataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                        LibControlType ctrlType = (LibControlType)col.ExtendedProperties[FieldProperty.ControlType];
                        switch (dataType)
                        {
                            case LibDataType.Text:
                            case LibDataType.NText:
                            case LibDataType.Binary:
                            case LibDataType.Int64:
                                if (dataType == LibDataType.Int64 && ctrlType == LibControlType.DateTime)
                                {
                                    long dateTime = LibSysUtils.ToInt64(curRow[col]);
                                    if (dateTime != 0)
                                    {
                                        type = "DateTime";
                                        style = string.Format(" ss:StyleID=\"s23\"");
                                        value = LibDateUtils.LibDateToDateTime(dateTime).ToString("yyyy-MM-ddTHH:mm:ss");
                                    }
                                    else
                                    {
                                        type = "String";
                                        value = string.Empty;
                                    }
                                }
                                else
                                {
                                    type = "String";
                                    value = curRow[col];
                                }
                                break;
                            case LibDataType.Int32:
                            case LibDataType.Numeric:
                            case LibDataType.Float:
                            case LibDataType.Double:
                            case LibDataType.Byte:
                                if (dataType == LibDataType.Int32 && ctrlType == LibControlType.Date)
                                {
                                    int date = LibSysUtils.ToInt32(curRow[col]);
                                    if (date != 0)
                                    {
                                        type = "DateTime";
                                        value = string.Format("{0}T00:00:00.000", LibDateUtils.LibDateToDateTime(date).ToString("yyyy-MM-dd"));
                                        style = string.Format(" ss:StyleID=\"s23\"");
                                    }
                                    else
                                    {
                                        type = "String";
                                        value = string.Empty;
                                    }
                                }
                                else if (dataType == LibDataType.Int32 && ctrlType == LibControlType.HourMinute)
                                {
                                    type = "Number";
                                    string time = LibSysUtils.ToString(curRow[col]);
                                    switch (time.Length)
                                    {
                                        case 1: time = "000" + time + "00"; break;
                                        case 2: time = "00" + time + "00"; break;
                                        case 3: time = "0" + time + "00"; break;
                                        case 4: time = time + "00"; break;
                                        default: time = time + "00"; break;
                                    }
                                    time = "20150101" + time;
                                    value = LibStringBuilder.GetQuotObject(LibDateUtils.LibDateToDateTime(LibSysUtils.ToInt64(time)).ToString("HH:mm"));
                                }
                                else if (dataType == LibDataType.Numeric)
                                {
                                    type = "Number";
                                    style = string.Format(" ss:StyleID=\"s24\"");
                                    value = curRow[col];
                                }
                                else
                                {
                                    type = "Number";
                                    value = curRow[col];
                                }

                                break;
                            case LibDataType.Boolean:
                                type = "Number";
                                value = LibSysUtils.ToBoolean(curRow[col.ColumnName]) ? 1 : 0;
                                break;
                        }
                        #endregion
                        rows.Append(string.Format("<Cell{3}><Data ss:Type=\"{0}\">{1}</Data><NamedCell ss:Name=\"{2}\"/></Cell>\r\n", type, value, tableName, style));
                    }
                    rows.Append("</Row>\r\n");
                    #endregion
                }
                #endregion
                #region 构建表格模板
                Worksheet.AppendLine(string.Format("<Worksheet ss:Name=\"{0}\">\n<Table ss:ExpandedColumnCount=\"{1}\" ss:ExpandedRowCount=\"{2}\" x:FullColumns=\"1\" x:FullRows=\"1\" ss:DefaultRowHeight=\"12\">", tableName, dt.Columns.Count.ToString(), (dt.Rows.Count + 1).ToString()));
                Worksheet.AppendLine(string.Format(@"<Row>
{0}
</Row>
{1}
</Table>", headCols.ToString(), rows.ToString()));
                Worksheet.AppendLine("<WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">");
                Worksheet.AppendLine("<PageSetup>");
                Worksheet.AppendLine("<Header x:Data=\"&amp;A\"/>");
                Worksheet.AppendLine("<Footer x:Data=\"Page &amp;P\"/>");
                Worksheet.AppendLine(@"</PageSetup>
<Selected/>
<ProtectObjects>False</ProtectObjects>
<ProtectScenarios>False</ProtectScenarios>
</WorksheetOptions>
</Worksheet>");
                #endregion
            }
            #region 将数据替换到模板中
            DateTime datetime = DateTime.Now;
            bodyXML = bodyXML.Replace("{##Author##}", "Administrator");
            bodyXML = bodyXML.Replace("{##Created##}", datetime.ToString());
            bodyXML = bodyXML.Replace("{##Names##}", names.ToString());
            bodyXML = bodyXML.Replace("{##Worksheet##}", Worksheet.ToString());
            #endregion

            try
            {
                string path = filePath;
                using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                    {

                        sw.Write(bodyXML);
                    }
                }
                watch.Stop();
                string time = watch.ElapsedMilliseconds.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public System.Data.DataTable ExcelToTable(string filePath, string tableName)
        {
            System.Data.DataTable curTable = new System.Data.DataTable();
            try
            {
                string connStr = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source='{0}'; Extended Properties='Excel 8.0;HDR=Yes;IMEX=2'", filePath);
                using (OleDbConnection conn = new OleDbConnection(connStr))
                {
                    conn.Open();
                    try
                    {
                        using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(string.Format("select * from [{0}$];", tableName), conn))
                        {
                            dataAdapter.Fill(curTable);
                        }
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                string path = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output", "Error", "Excel", string.Format("{0}.txt", DateTime.Now.Ticks));
                using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                    {
                        sw.Write(ex);
                    }
                }
                throw;
            }
            return curTable;
        }
    }
}
