using AxCRL.Comm.Bill;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Core.Server;
using AxCRL.Data;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AxCRL.Bcf
{
    public abstract class LibBcfDataBase : LibBcfBase
    {
        private Dictionary<string, Type> _RegisterBcfParamType;
        private Dictionary<string, object> _extendBcfParam;


        protected Dictionary<string, Type> RegisterBcfParamType
        {
            get
            {
                if (_RegisterBcfParamType == null)
                    _RegisterBcfParamType = new Dictionary<string, Type>();
                return _RegisterBcfParamType;
            }
        }

        protected Dictionary<string, object> ExtendBcfParam
        {
            get
            {
                if (_extendBcfParam == null)
                    _extendBcfParam = new Dictionary<string, object>();
                return _extendBcfParam;
            }
        }
        public LibBcfDataBase()
        {

        }
        protected virtual void CheckImportData(DataSet DataSet)
        {

        }
        protected virtual void AfterImportDataInTable(DataTable table)
        {

        }
        protected virtual bool AfterImportDataInRow(DataRow curRow, Dictionary<string, object> otherValueList)
        {
            return true;
        }

        protected virtual void AfterExportData()
        {

        }

        protected virtual void CreateDynamicFieldRelation(LibEntryParam entryParam)
        {

        }

        protected virtual DataSet ChangeTableStructure(LibEntryParam entryParam)
        {
            return null;
        }

        private class DynamicTableInfo
        {
            public int RowId { get; set; }
            public DataTable Table { get; set; }
            public DataTable ParentTable { get; set; }
            public DataColumn[] ParentColumn { get; set; }
            public DataColumn[] PKColumn { get; set; }
            public DynamicTableInfo(DataTable table)
            {
                this.Table = table;
                DataRelation dataRelation = table.ParentRelations[0];
                this.ParentTable = dataRelation.ParentTable;
                this.ParentColumn = dataRelation.ParentColumns;
                this.PKColumn = table.PrimaryKey;
                this.RowId = 1;
            }
        }

        protected virtual DataSet FillDataDynamicTable(DataSet destDataSet)
        {
            destDataSet.EnforceConstraints = false;
            List<DynamicTableInfo> dyTableList = new List<DynamicTableInfo>();
            try
            {
                foreach (DataTable curTable in destDataSet.Tables)
                {
                    if (curTable.TableName.IndexOf("DYTABLE") != 0)
                        continue;
                    dyTableList.Add(new DynamicTableInfo(curTable));
                    curTable.BeginLoadData();
                }
                foreach (DataTable sourceTable in this.DataSet.Tables)
                {
                    if (!sourceTable.ExtendedProperties.ContainsKey(TableProperty.DynamicFieldRelaion))
                        continue;
                    LibDynamicFildRelation dyRelation = (LibDynamicFildRelation)sourceTable.ExtendedProperties[TableProperty.DynamicFieldRelaion];
                    DataTable destTable = destDataSet.Tables[sourceTable.TableName];
                    destTable.BeginLoadData();
                    try
                    {
                        foreach (DataRow row in sourceTable.Rows)
                        {
                            if (row.RowState == DataRowState.Deleted)
                                continue;
                            DataRow newRow = destTable.NewRow();
                            newRow.BeginEdit();
                            try
                            {
                                foreach (DataColumn col in sourceTable.Columns)
                                {
                                    newRow[col.ColumnName] = row[col];
                                }
                                Dictionary<int, Dictionary<string, LibDynamicFildInfo>> dic = dyRelation.DynamicFildRelation;
                                if (dic != null)
                                {
                                    Dictionary<string, DataRow> subRowMap = new Dictionary<string, DataRow>();
                                    foreach (var dyTableInfo in dyTableList)
                                    {
                                        DataRow newSubRow = dyTableInfo.Table.NewRow();
                                        newSubRow.BeginEdit();
                                        if (dyTableInfo.ParentTable.TableName == sourceTable.TableName)
                                        {
                                            for (int i = 0; i < dyTableInfo.ParentColumn.Length; i++)
                                            {
                                                newSubRow[dyTableInfo.PKColumn[i]] = newRow[dyTableInfo.ParentColumn[i]];
                                            }
                                        }
                                        else
                                        {
                                            DataRow parentRow = subRowMap[dyTableInfo.ParentTable.TableName];
                                            for (int i = 0; i < dyTableInfo.ParentColumn.Length; i++)
                                            {
                                                newSubRow[dyTableInfo.PKColumn[i]] = parentRow[dyTableInfo.ParentColumn[i]];
                                            }
                                        }
                                        newSubRow["ROW_ID"] = dyTableInfo.RowId;
                                        newSubRow["ROWNO"] = 1;
                                        dyTableInfo.RowId++;
                                        dyTableInfo.Table.Rows.Add(newSubRow);
                                        subRowMap.Add(dyTableInfo.Table.TableName, newSubRow);
                                    }
                                    foreach (var item in dic)
                                    {
                                        DataRow[] childRows = row.GetChildRows(string.Format("{0}_{1}", sourceTable.TableName, this.DataSet.Tables[item.Key].TableName));
                                        if (childRows != null)
                                        {
                                            foreach (DataRow childRow in childRows)
                                            {
                                                foreach (var subItem in item.Value)
                                                {
                                                    string colName = LibSysUtils.ToString(childRow[subItem.Value.MapToField]);
                                                    if (string.IsNullOrEmpty(subItem.Value.ParentMapToField))
                                                    {
                                                        if (destTable.Columns.Contains(colName))
                                                            newRow[colName] = childRow[subItem.Value.FieldForValue];
                                                    }
                                                    else
                                                    {
                                                        string tableName = string.Format("DYTABLE{0}", subItem.Value.ParentForValue);
                                                        if (subRowMap.ContainsKey(tableName))
                                                        {
                                                            if (subRowMap[tableName].Table.Columns.Contains(colName))
                                                                subRowMap[tableName][colName] = childRow[subItem.Value.FieldForValue];
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    foreach (var dyTableInfo in dyTableList)
                                    {
                                        subRowMap[dyTableInfo.Table.TableName].EndEdit();
                                    }
                                }
                            }
                            finally
                            {
                                newRow.EndEdit();
                            }
                            destTable.Rows.Add(newRow);
                        }
                    }
                    finally
                    {
                        destTable.EndLoadData();
                    }
                }
            }
            finally
            {
                foreach (var item in dyTableList)
                {
                    item.Table.EndLoadData();
                }
                destDataSet.EnforceConstraints = true;
            }
            return destDataSet;
        }

        protected virtual void AddNewDynamicRow(DataRow curRow, LibDynamicFildRelation dynamicRelation, Dictionary<string, object> changeData)
        {
            if (curRow != null)
            {
                Dictionary<int, Dictionary<string, LibDynamicFildInfo>> dic = dynamicRelation.DynamicFildRelation;
                if (dic != null)
                {
                    Dictionary<string, DataRow> mapDic = new Dictionary<string, DataRow>();
                    foreach (var item in dic)
                    {
                        DataTable table = this.DataSet.Tables[item.Key];
                        DataColumn[] subPkCol = table.PrimaryKey;
                        int rowId = 0;
                        foreach (var subItem in item.Value)
                        {
                            bool isStartField = subItem.Key.Length > 5 && (subItem.Key.Substring(subItem.Key.Length - 5, 5) == "START");
                            if (isStartField)
                            {
                                string endField = subItem.Key.Substring(0, subItem.Key.Length - 5);
                                if (mapDic.ContainsKey(endField))
                                {
                                    DataRow mapRow = mapDic[endField];
                                    curRow.BeginEdit();
                                    try
                                    {
                                        string colName = subItem.Key;
                                        mapRow[subItem.Value.MapToField] = colName;
                                        if (changeData.ContainsKey(colName))
                                            mapRow[subItem.Value.FieldForValue] = changeData[colName];
                                        if (subItem.Value.ParentMapToField != null)
                                            mapRow[subItem.Value.ParentMapToField] = LibSysUtils.ToString(subItem.Value.ParentForValue);
                                    }
                                    finally
                                    {
                                        curRow.EndEdit();
                                    }
                                }
                            }
                            else
                            {
                                DataRow newRow = table.NewRow();
                                newRow.BeginEdit();
                                try
                                {
                                    //赋值主键
                                    for (int i = 0; i < curRow.Table.PrimaryKey.Length; i++)
                                    {
                                        newRow[subPkCol[i].ColumnName] = curRow[curRow.Table.PrimaryKey[i].ColumnName];
                                    }
                                    newRow["ROW_ID"] = ++rowId;
                                    newRow["ROWNO"] = rowId;
                                    //赋值其他字段
                                    string colName = subItem.Key;
                                    newRow[subItem.Value.MapToField] = colName;
                                    if (changeData.ContainsKey(colName))
                                        newRow[subItem.Value.FieldForValue] = changeData[colName];
                                    if (subItem.Value.ParentMapToField != null)
                                        newRow[subItem.Value.ParentMapToField] = LibSysUtils.ToString(subItem.Value.ParentForValue);
                                    if (!mapDic.ContainsKey(colName))
                                        mapDic.Add(colName, newRow);
                                }
                                finally
                                {
                                    newRow.EndEdit();
                                }
                                table.Rows.Add(newRow);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void ModifDynamicPk(DataRow curRow, LibDynamicFildRelation dynamicRelation)
        {
            if (curRow != null)
            {
                Dictionary<int, Dictionary<string, LibDynamicFildInfo>> dic = dynamicRelation.DynamicFildRelation;
                if (dic != null)
                {
                    foreach (var item in dic)
                    {
                        DataRow[] subRow = curRow.GetChildRows(string.Format("{0}_{1}", curRow.Table.TableName, this.DataSet.Tables[item.Key].TableName));
                        DataColumn[] subPkCol = this.DataSet.Tables[item.Key].PrimaryKey;
                        for (int i = 0; i < subRow.Length; i++)
                        {
                            subRow[i].BeginEdit();
                            try
                            {
                                for (int r = 0; r < curRow.Table.PrimaryKey.Length; r++)
                                {
                                    subRow[i][subPkCol[r].ColumnName] = curRow[curRow.Table.PrimaryKey[r].ColumnName];
                                }
                            }
                            finally
                            {
                                subRow[i].EndEdit();
                            }
                        }
                    }
                }
            }
        }

        protected virtual void ModifDynamicRow(DataRow curRow, LibDynamicFildRelation dynamicRelation, Dictionary<string, object> changeData)
        {
            if (curRow != null)
            {
                Dictionary<int, Dictionary<string, LibDynamicFildInfo>> dic = dynamicRelation.DynamicFildRelation;
                if (dic != null)
                {
                    foreach (var item in dic)
                    {
                        DataRow[] subRow = curRow.GetChildRows(string.Format("{0}_{1}", curRow.Table.TableName, this.DataSet.Tables[item.Key].TableName));
                        for (int i = 0; i < subRow.Length; i++)
                        {
                            subRow[i].BeginEdit();
                            try
                            {
                                foreach (var subItem in changeData)
                                {
                                    if (!item.Value.ContainsKey(subItem.Key))
                                        continue;
                                    LibDynamicFildInfo dyFiledInfo = item.Value[subItem.Key];
                                    if (LibSysUtils.ToString(subRow[i][dyFiledInfo.MapToField]) == subItem.Key)
                                    {
                                        string srcName = dyFiledInfo.FieldForValue;
                                        subRow[i][srcName] = subItem.Value;
                                    }
                                }
                            }
                            finally
                            {
                                subRow[i].EndEdit();
                            }
                        }
                    }
                }
            }
        }

        protected virtual void DeleteDynamicRow(DataRow curRow, LibDynamicFildRelation dynamicRelation)
        {
            if (curRow != null) //动态列所在实际表的记录将标识删除
            {
                Dictionary<int, Dictionary<string, LibDynamicFildInfo>> dic = dynamicRelation.DynamicFildRelation;
                if (dic != null)
                {
                    foreach (var item in dic)
                    {
                        DataRow[] subRow = curRow.GetChildRows(string.Format("{0}_{1}", curRow.Table.TableName, this.DataSet.Tables[item.Key].TableName));
                        for (int i = subRow.Length - 1; i >= 0; i--)
                        {
                            subRow[i].Delete();
                        }
                    }
                }
            }
        }
    }

    public static class DataSetManager
    {

        public static void GetDataSet(DataSet dataSet, LibDataAccess dataAccess, string key)
        {
            dataAccess.ExecuteDataTables(dataSet, LibCommUtils.GetStoredProcedureName(key));
        }

        public static void GetDataSet(DataSet dataSet, LibDataAccess dataAccess, string key, object[] pks, LibHandle userHandle)
        {
            dataAccess.ExecuteDataTables(dataSet, LibCommUtils.GetStoredProcedureName(key), pks);
            //附加同步数据到各站点的配置及历史记录
            //if (dataSet.Tables.Contains(LibFuncPermission.SynchroDataSettingTableName) && LibTemplate.HasAxpLinkSite)
            if (dataSet.Tables.Contains(LibFuncPermission.SynchroDataSettingTableName))
            {
                string progId = key;
                string internalId = string.Empty;
                if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Columns.Contains("INTERNALID") && dataSet.Tables[0].Rows.Count > 0)
                    internalId = LibSysUtils.ToString(dataSet.Tables[0].Rows[0]["INTERNALID"]);

                DataTable dt = dataSet.Tables[LibFuncPermission.SynchroDataSettingTableName];
                if (dt != null && string.IsNullOrEmpty(progId) == false && userHandle != null)
                {
                    CrossSiteHelper.FillSyncDataSetting(userHandle, dt, progId);
                }
                dt = dataSet.Tables[LibFuncPermission.SynchroDataHisTableName];
                if (dt != null && string.IsNullOrEmpty(progId) == false && userHandle != null)
                {
                    CrossSiteHelper.FillSyncDataHistory(userHandle, dt, progId, internalId);
                }
            }
        }

        public static Dictionary<string, LibChangeRecord> GetChangeRecord(DataSet dataSet)
        {
            Dictionary<string, LibChangeRecord> changeRecord = new Dictionary<string, LibChangeRecord>();
            foreach (DataTable table in dataSet.Tables)
            {
                LibChangeRecord rec = new LibChangeRecord();
                changeRecord.Add(table.TableName, rec);
                foreach (DataRow curRow in table.Rows)
                {
                    Dictionary<string, object> valDic = new Dictionary<string, object>();
                    switch (curRow.RowState)
                    {
                        case DataRowState.Added:
                            foreach (DataColumn col in table.Columns)
                            {
                                valDic.Add(col.ColumnName, curRow[col]);
                            }
                            rec.Add.Add(valDic);
                            break;
                        case DataRowState.Deleted:
                            foreach (DataColumn col in table.PrimaryKey)
                            {
                                valDic.Add(col.ColumnName, curRow[col, DataRowVersion.Original]);
                            }
                            rec.Remove.Add(valDic);
                            break;
                        case DataRowState.Modified:
                            foreach (DataColumn col in table.PrimaryKey)
                            {
                                valDic.Add(string.Format("_{0}", col.ColumnName), curRow[col, DataRowVersion.Original]);
                            }
                            foreach (DataColumn col in table.Columns)
                            {
                                if (!curRow[col].Equals(curRow[col, DataRowVersion.Original]))
                                    valDic.Add(col.ColumnName, curRow[col]);
                            }
                            rec.Modif.Add(valDic);
                            break;
                    }
                }
            }
            return changeRecord;
        }


        public static void ChangeDataHandle(DataSet dataSet, Dictionary<string, LibChangeRecord> changeRecord, LibManagerMessage messageList,
            Action<DataRow, LibDynamicFildRelation, Dictionary<string, object>> addAction = null, Action<DataRow, LibDynamicFildRelation> modifPkAciton = null,
            Action<DataRow, LibDynamicFildRelation, Dictionary<string, object>> modifAciton = null, Action<DataRow, LibDynamicFildRelation> deleteAction = null)
        {
            if (changeRecord != null && changeRecord.Count > 0)
            {
                try
                {
                    dataSet.EnforceConstraints = false;
                    DataTable dynamicHostTable = null;
                    LibDynamicFildRelation dynamicRelation = null;
                    try
                    {
                        for (int index = 0; index < dataSet.Tables.Count; index++)
                        {
                            DataTable curTable = dataSet.Tables[index];
                            if (curTable.ExtendedProperties.ContainsKey(TableProperty.DynamicFieldRelaion))
                            {
                                dynamicRelation = curTable.ExtendedProperties[TableProperty.DynamicFieldRelaion] as LibDynamicFildRelation;
                                dynamicHostTable = curTable;
                            }
                            if (!changeRecord.ContainsKey(curTable.TableName))
                                continue;
                            LibChangeRecord record = changeRecord[curTable.TableName];
                            curTable.BeginLoadData();
                            try
                            {
                                foreach (var subItem in record.Add)
                                {
                                    Dictionary<string, object> dyDic = null;
                                    DataRow newRow = curTable.NewRow();
                                    newRow.BeginEdit();
                                    try
                                    {
                                        foreach (var data in subItem)
                                        {
                                            if (dynamicRelation == null)
                                            {
                                                if (curTable.Columns.Contains(data.Key) && data.Value != null)
                                                    newRow[data.Key] = data.Value;
                                            }
                                            else
                                            {
                                                if (curTable.Columns.Contains(data.Key))//存在动态列的情况
                                                    newRow[data.Key] = data.Value;
                                                else
                                                {
                                                    if (dyDic == null)
                                                        dyDic = new Dictionary<string, object>();
                                                    dyDic.Add(data.Key, data.Value);
                                                }
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        newRow.EndEdit();
                                    }
                                    curTable.Rows.Add(newRow);
                                    if (addAction != null && dynamicRelation != null && dyDic != null)
                                    {
                                        addAction(newRow, dynamicRelation, dyDic);
                                    }
                                }
                                foreach (var subItem in record.Modif)
                                {
                                    int length = curTable.PrimaryKey.Length;
                                    object[] keys = new object[length];
                                    int i = 0;
                                    bool isPkChange = false;
                                    foreach (var temp in curTable.PrimaryKey)
                                    {
                                        if (dynamicRelation != null && !isPkChange && subItem.ContainsKey(temp.ColumnName))
                                            isPkChange = true;
                                        keys[i] = subItem[string.Format("_{0}", temp.ColumnName)];
                                        i++;
                                    }
                                    DataRow curRow = curTable.Rows.Find(keys);
                                    if (isPkChange && (modifPkAciton != null && dynamicRelation != null))
                                        modifPkAciton(curRow, dynamicRelation);
                                    Dictionary<string, object> dyDic = null;
                                    curRow.BeginEdit();
                                    try
                                    {
                                        foreach (var data in subItem)
                                        {
                                            if (data.Key[0] == '_')
                                                continue;
                                            if (dynamicRelation == null)
                                            {
                                                if (data.Value != null && curTable.Columns.Contains(data.Key))
                                                    curRow[data.Key] = data.Value;
                                            }
                                            else
                                            {
                                                if (curTable.Columns.Contains(data.Key)) //存在动态列的情况
                                                    curRow[data.Key] = data.Value;
                                                else
                                                {
                                                    if (dyDic == null)
                                                        dyDic = new Dictionary<string, object>();
                                                    dyDic.Add(data.Key, data.Value);
                                                }
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        curRow.EndEdit();
                                    }
                                    if (modifAciton != null && dynamicRelation != null && dyDic != null)
                                        modifAciton(curRow, dynamicRelation, dyDic);
                                }
                                foreach (var subItem in record.Remove)
                                {
                                    int length = curTable.PrimaryKey.Length;
                                    object[] keys = new object[length];
                                    int i = 0;
                                    foreach (var temp in curTable.PrimaryKey)
                                    {
                                        keys[i] = subItem[temp.ColumnName];
                                        i++;
                                    }
                                    DataRow curRow = curTable.Rows.Find(keys);
                                    if (curRow != null)//从其他站点同步过来的主键可能找不到对应行
                                    {
                                        if (deleteAction != null && dynamicRelation != null)
                                            deleteAction(curRow, dynamicRelation);
                                        curRow.Delete();
                                    }
                                }
                            }
                            finally
                            {
                                curTable.EndLoadData();
                            }
                        }
                        if (dynamicHostTable != null)
                        {
                            //处理动态表，将动态表的数据更新到对应实体表的记录
                            foreach (KeyValuePair<string, LibChangeRecord> item in changeRecord)
                            {
                                if (item.Key.IndexOf("DYTABLE") != 0)
                                    continue;
                                foreach (var subItem in item.Value.Add)
                                {
                                    DataRow parentRow = dynamicHostTable.Rows.Find(new object[] { subItem["BILLNO"], subItem["PARENTROWID"] });
                                    subItem.Remove("BILLNO");
                                    subItem.Remove("PARENTROWID");
                                    modifAciton(parentRow, dynamicRelation, subItem);
                                }
                                foreach (var subItem in item.Value.Modif)
                                {
                                    object billNO = subItem.ContainsKey("BILLNO") ? subItem["BILLNO"] : subItem["_BILLNO"];
                                    DataRow parentRow = dynamicHostTable.Rows.Find(new object[] { billNO, subItem["_PARENTROWID"] });
                                    subItem.Remove("BILLNO");
                                    subItem.Remove("PARENTROWID");
                                    modifAciton(parentRow, dynamicRelation, subItem);
                                }
                            }
                        }
                    }
                    finally
                    {
                        try
                        {
                            dataSet.EnforceConstraints = true;
                        }
                        catch
                        {
                            foreach (DataTable dt in dataSet.Tables)
                            {
                                foreach (DataRow dr in dt.Rows)
                                {
                                    if (dr.HasErrors)
                                        messageList.AddMessage(LibMessageKind.Error, dr.RowError);
                                }
                            }
                            throw;
                        }
                    }
                }
                catch
                {
                    dataSet.RejectChanges();
                    throw;
                }
            }
        }

        public static void SubmitData(DataSet dataSet, LibDataAccess dataAccess)
        {
            List<string> list = new List<string>();
            int i = 0;
            foreach (DataTable table in dataSet.Tables)
            {
                bool isVirtual = table.ExtendedProperties.ContainsKey(TableProperty.IsVirtual) ? (bool)table.ExtendedProperties[TableProperty.IsVirtual] : false;
                if (isVirtual) continue;
                foreach (DataRow curRow in table.Rows)
                {
                    string sql = null;
                    switch (curRow.RowState)
                    {
                        case DataRowState.Added:
                            sql = GetInsertSql(table, curRow);
                            break;
                        case DataRowState.Deleted:
                            sql = GetDeleteSql(table, curRow);
                            break;
                        case DataRowState.Modified:
                            sql = GetUpdateSql(table, curRow);
                            break;
                    }
                    if (!string.IsNullOrEmpty(sql))
                        list.Add(sql);
                }
                i++;
            }
            if (list.Count > 0)
                dataAccess.ExecuteNonQuery(list);
        }

        #region [构建数据表更新语法]

        private static string GetUpdateSql(DataTable table, DataRow curRow)
        {
            string tableName = table.TableName;
            string sql = string.Empty;
            StringBuilder valueBuilder = new StringBuilder();
            StringBuilder whereBuilder = new StringBuilder();
            foreach (var item in table.PrimaryKey)
            {
                if (item.DataType == typeof(string))
                    whereBuilder.AppendFormat(" {0}={1} And", item.ColumnName, LibStringBuilder.GetQuotString(LibSysUtils.ToString(curRow[item.ColumnName, DataRowVersion.Original])));
                else
                    whereBuilder.AppendFormat(" {0}={1} And", item.ColumnName, curRow[item.ColumnName, DataRowVersion.Original]);
            }
            foreach (DataColumn item in table.Columns)
            {
                string name = item.ColumnName;
                FieldType fieldType = item.ExtendedProperties.ContainsKey(FieldProperty.FieldType) ? (FieldType)item.ExtendedProperties[FieldProperty.FieldType] : FieldType.None;
                if (fieldType != FieldType.None)
                    continue;
                LibDataType DataType = (LibDataType)item.ExtendedProperties[FieldProperty.DataType];
                switch (DataType)
                {
                    case LibDataType.Text:
                    case LibDataType.NText:
                    case LibDataType.Binary:
                        string sValue = LibSysUtils.ToString(curRow[name]);
                        if (sValue != LibSysUtils.ToString(curRow[name, DataRowVersion.Original]))
                            valueBuilder.AppendFormat("{0}={1},", name, LibStringBuilder.GetQuotString(sValue));
                        break;
                    case LibDataType.Int32:
                        if (LibSysUtils.ToInt32(curRow[name]) != LibSysUtils.ToInt32(curRow[name, DataRowVersion.Original]))
                            valueBuilder.AppendFormat("{0}={1},", name, curRow[name]);
                        break;
                    case LibDataType.Int64:
                        if (LibSysUtils.ToInt64(curRow[name]) != LibSysUtils.ToInt64(curRow[name, DataRowVersion.Original]))
                            valueBuilder.AppendFormat("{0}={1},", name, curRow[name]);
                        break;
                    case LibDataType.Numeric:
                        if (LibSysUtils.ToDecimal(curRow[name]) != LibSysUtils.ToDecimal(curRow[name, DataRowVersion.Original]))
                            valueBuilder.AppendFormat("{0}={1},", name, curRow[name]);
                        break;
                    case LibDataType.Float:
                        if (LibSysUtils.ToSingle(curRow[name]) != LibSysUtils.ToSingle(curRow[name, DataRowVersion.Original]))
                            valueBuilder.AppendFormat("{0}={1},", name, curRow[name]);
                        break;
                    case LibDataType.Double:
                        if (LibSysUtils.ToDouble(curRow[name]) != LibSysUtils.ToDouble(curRow[name, DataRowVersion.Original]))
                            valueBuilder.AppendFormat("{0}={1},", name, curRow[name]);
                        break;
                    case LibDataType.Byte:
                        if (LibSysUtils.ToByte(curRow[name]) != LibSysUtils.ToByte(curRow[name, DataRowVersion.Original]))
                            valueBuilder.AppendFormat("{0}={1},", name, curRow[name]);
                        break;
                    case LibDataType.Boolean:
                        if (LibSysUtils.ToBoolean(curRow[name]) != LibSysUtils.ToBoolean((curRow[name, DataRowVersion.Original])))
                            valueBuilder.AppendFormat("{0}={1},", name, LibSysUtils.ToInt32(curRow[name]));
                        break;
                    case LibDataType.DateTime:
                        if (LibSysUtils.ToString(curRow[name]) != LibSysUtils.ToString((curRow[name, DataRowVersion.Original])))
                            valueBuilder.AppendFormat("{0}='{1}',", name, LibSysUtils.ToString(curRow[name]));
                        break;
                    case LibDataType.Date:
                        if (LibSysUtils.ToString(curRow[name]) != LibSysUtils.ToString((curRow[name, DataRowVersion.Original])))
                            valueBuilder.AppendFormat("{0}='{1}',", name, LibSysUtils.ToString(curRow[name]));
                        break;
                    case LibDataType.Time:
                        if (LibSysUtils.ToString(curRow[name]) != LibSysUtils.ToString((curRow[name, DataRowVersion.Original])))
                            valueBuilder.AppendFormat("{0}='{1}',", name, LibSysUtils.ToString(curRow[name]));
                        break;
                    default:
                        break;
                }

            }
            if (valueBuilder.Length > 0)
            {
                valueBuilder.Remove(valueBuilder.Length - 1, 1);
                whereBuilder.Remove(whereBuilder.Length - 3, 3);
                sql = string.Format("Update {0} set {1} where {2}", tableName, valueBuilder.ToString(), whereBuilder.ToString());
            }
            return sql;
        }

        private static string GetDeleteSql(DataTable table, DataRow curRow)
        {
            string tableName = table.TableName;
            string sql = string.Empty;
            StringBuilder whereBuilder = new StringBuilder();
            foreach (var item in table.PrimaryKey)
            {
                if (item.DataType == typeof(string))
                    whereBuilder.AppendFormat(" {0}={1} And", item.ColumnName, LibStringBuilder.GetQuotString(LibSysUtils.ToString(curRow[item.ColumnName, DataRowVersion.Original])));
                else
                    whereBuilder.AppendFormat(" {0}={1} And", item.ColumnName, curRow[item.ColumnName, DataRowVersion.Original]);
            }
            whereBuilder.Remove(whereBuilder.Length - 3, 3);
            sql = string.Format("Delete {0} where {1}", tableName, whereBuilder.ToString());
            return sql;
        }

        private static string GetInsertSql(DataTable table, DataRow curRow)
        {
            string tableName = table.TableName;
            string sql = string.Empty;
            StringBuilder fieldBuilder = new StringBuilder();
            StringBuilder valueBuilder = new StringBuilder();
            foreach (DataColumn item in table.Columns)
            {
                FieldType fieldType = item.ExtendedProperties.ContainsKey(FieldProperty.FieldType) ? (FieldType)item.ExtendedProperties[FieldProperty.FieldType] : FieldType.None;
                if (fieldType != FieldType.None)
                    continue;
                string name = item.ColumnName;
                fieldBuilder.AppendFormat("{0},", name);
                object value = null;
                if (item.DataType == typeof(string))
                    value = LibStringBuilder.GetQuotString(LibSysUtils.ToString(curRow[name]));
                else if (item.DataType == typeof(bool))
                    value = LibSysUtils.ToInt32(curRow[name]);
                else
                    value = curRow[name];
                valueBuilder.AppendFormat("{0},", value);
            }
            fieldBuilder.Remove(fieldBuilder.Length - 1, 1);
            valueBuilder.Remove(valueBuilder.Length - 1, 1);
            sql = string.Format("Insert Into {0}({1}) Values({2})", tableName, fieldBuilder.ToString(), valueBuilder.ToString());
            return sql;
        }

        #endregion

    }

    public static class CheckDataHelper
    {
        public static void CheckData(LibBcfDataBase curBcf)
        {
            DataSet dataSet = curBcf.DataSet;
            foreach (DataTable table in dataSet.Tables)
            {
                bool allowEmpt = true;
                if (table.ExtendedProperties.ContainsKey(TableProperty.AllowEmpt))
                    allowEmpt = Convert.ToBoolean(table.ExtendedProperties[TableProperty.AllowEmpt]);
                HashSet<string> repeatHashSet = null;
                List<string> notRepeat = null;
                if (table.ExtendedProperties.ContainsKey(TableProperty.NotRepeat))
                {
                    repeatHashSet = new HashSet<string>();
                    notRepeat = (List<string>)table.ExtendedProperties[TableProperty.NotRepeat];
                }
                if (table.DefaultView.Count == 0)
                {
                    if (!allowEmpt)
                    {
                        string tableDisplayText = table.ExtendedProperties.ContainsKey(TableProperty.DisplayText) ?
                            LibSysUtils.ToString(table.ExtendedProperties[TableProperty.DisplayText]) : table.TableName;
                        curBcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("表{0}不能为空", tableDisplayText));
                    }
                }
                else
                {
                    Dictionary<string, LibDataType> colDataType = new Dictionary<string, LibDataType>();
                    Dictionary<string, LibControlType> colControlType = new Dictionary<string, LibControlType>();
                    List<string> checkEmpty = new List<string>();
                    Dictionary<string, LibQtyLimit> checkQtyLimit = new Dictionary<string, LibQtyLimit>();
                    foreach (DataColumn col in table.Columns)
                    {
                        if (col.ExtendedProperties.ContainsKey(FieldProperty.AllowEmpty))
                        {
                            if (!(bool)col.ExtendedProperties[FieldProperty.AllowEmpty])
                            {
                                checkEmpty.Add(col.ColumnName);
                                if (!colDataType.ContainsKey(col.ColumnName))
                                {
                                    colDataType.Add(col.ColumnName, (LibDataType)((int)col.ExtendedProperties[FieldProperty.DataType]));
                                    colControlType.Add(col.ColumnName, (LibControlType)((int)col.ExtendedProperties[FieldProperty.ControlType]));
                                }
                            }
                        }
                        if (col.ExtendedProperties.ContainsKey(FieldProperty.QtyLimit))
                        {
                            checkQtyLimit.Add(col.ColumnName, (LibQtyLimit)((int)col.ExtendedProperties[FieldProperty.QtyLimit]));
                            if (!colDataType.ContainsKey(col.ColumnName))
                            {
                                colDataType.Add(col.ColumnName, (LibDataType)((int)col.ExtendedProperties[FieldProperty.DataType]));
                            }
                        }
                    }
                    foreach (DataRow curRow in table.Rows)
                    {
                        if (curRow.RowState == DataRowState.Deleted)
                            continue;
                        if (notRepeat != null)
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (string colName in notRepeat)
                            {
                                builder.AppendFormat("{0}/t", curRow[colName]);
                            }
                            string key = builder.ToString();
                            if (repeatHashSet.Contains(key))
                            {
                                builder.Length = 0;
                                foreach (string colName in notRepeat)
                                {
                                    builder.AppendFormat("{0},", table.Columns[colName].Caption);
                                }
                                builder.Remove(builder.Length - 1, 1);
                                curBcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("行{0}相关字段{1}与其他记录重复。", LibSysUtils.ToInt32(curRow["ROWNO"]), builder.ToString()));
                            }
                            else
                                repeatHashSet.Add(key);
                        }

                        foreach (string name in checkEmpty)
                        {
                            LibDataType dataType = colDataType[name];
                            LibControlType controlType = colControlType[name];
                            if (dataType == LibDataType.Text || dataType == LibDataType.NText)
                            {
                                if (string.IsNullOrEmpty(LibSysUtils.ToString(curRow[name])))
                                    curBcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("字段{0}不能为空。", table.Columns[name].Caption));
                            }
                            else if (controlType == LibControlType.Date)
                            {
                                if (LibSysUtils.ToInt32(curRow[name]) == 0)
                                    curBcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("字段{0}不能为空。", table.Columns[name].Caption));
                            }
                        }
                        foreach (KeyValuePair<string, LibQtyLimit> item in checkQtyLimit)
                        {
                            string name = item.Key;
                            bool right = true;
                            switch (item.Value)
                            {
                                case LibQtyLimit.GreaterThanZero:
                                    switch (colDataType[name])
                                    {
                                        case LibDataType.Int32:
                                            right = LibSysUtils.ToInt32(curRow[name]) > 0;
                                            break;
                                        case LibDataType.Int64:
                                            right = LibSysUtils.ToInt64(curRow[name]) > 0;
                                            break;
                                        case LibDataType.Numeric:
                                            right = LibSysUtils.ToDecimal(curRow[name]) > 0;
                                            break;
                                        case LibDataType.Float:
                                            right = LibSysUtils.ToSingle(curRow[name]) > 0;
                                            break;
                                        case LibDataType.Double:
                                            right = LibSysUtils.ToDouble(curRow[name]) > 0;
                                            break;
                                        case LibDataType.Byte:
                                            right = LibSysUtils.ToByte(curRow[name]) > 0;
                                            break;
                                    }
                                    if (!right)
                                        curBcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("字段{0}必须大于0。", table.Columns[name].Caption));
                                    break;
                                case LibQtyLimit.LessThanZero:
                                    switch (colDataType[name])
                                    {
                                        case LibDataType.Int32:
                                            right = LibSysUtils.ToInt32(curRow[name]) < 0;
                                            break;
                                        case LibDataType.Int64:
                                            right = LibSysUtils.ToInt64(curRow[name]) < 0;
                                            break;
                                        case LibDataType.Numeric:
                                            right = LibSysUtils.ToDecimal(curRow[name]) < 0;
                                            break;
                                        case LibDataType.Float:
                                            right = LibSysUtils.ToSingle(curRow[name]) < 0;
                                            break;
                                        case LibDataType.Double:
                                            right = LibSysUtils.ToDouble(curRow[name]) < 0;
                                            break;
                                        case LibDataType.Byte:
                                            right = LibSysUtils.ToByte(curRow[name]) < 0;
                                            break;
                                    }
                                    if (!right)
                                        curBcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("字段{0}必须小于0。", table.Columns[name].Caption));
                                    break;
                                case LibQtyLimit.GreaterOrEqualThanZero:
                                    switch (colDataType[name])
                                    {
                                        case LibDataType.Int32:
                                            right = LibSysUtils.ToInt32(curRow[name]) >= 0;
                                            break;
                                        case LibDataType.Int64:
                                            right = LibSysUtils.ToInt64(curRow[name]) >= 0;
                                            break;
                                        case LibDataType.Numeric:
                                            right = LibSysUtils.ToDecimal(curRow[name]) >= 0;
                                            break;
                                        case LibDataType.Float:
                                            right = LibSysUtils.ToSingle(curRow[name]) >= 0;
                                            break;
                                        case LibDataType.Double:
                                            right = LibSysUtils.ToDouble(curRow[name]) >= 0;
                                            break;
                                        case LibDataType.Byte:
                                            right = LibSysUtils.ToByte(curRow[name]) >= 0;
                                            break;
                                    }
                                    if (!right)
                                        curBcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("字段{0}必须大于等于0。", table.Columns[name].Caption));
                                    break;
                                case LibQtyLimit.LessOrEqualThanZero:
                                    switch (colDataType[name])
                                    {
                                        case LibDataType.Int32:
                                            right = LibSysUtils.ToInt32(curRow[name]) <= 0;
                                            break;
                                        case LibDataType.Int64:
                                            right = LibSysUtils.ToInt64(curRow[name]) <= 0;
                                            break;
                                        case LibDataType.Numeric:
                                            right = LibSysUtils.ToDecimal(curRow[name]) <= 0;
                                            break;
                                        case LibDataType.Float:
                                            right = LibSysUtils.ToSingle(curRow[name]) <= 0;
                                            break;
                                        case LibDataType.Double:
                                            right = LibSysUtils.ToDouble(curRow[name]) <= 0;
                                            break;
                                        case LibDataType.Byte:
                                            right = LibSysUtils.ToByte(curRow[name]) <= 0;
                                            break;
                                    }
                                    if (!right)
                                        curBcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("字段{0}必须小于等于0。", table.Columns[name].Caption));
                                    break;
                                case LibQtyLimit.ZeroBetweenHundred:
                                    int value32 = LibSysUtils.ToInt32(curRow[name]);
                                    right = value32 >= 0 && value32 <= 100;
                                    if (!right)
                                        curBcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("字段{0}必须大于等于0小于等于100。", table.Columns[name].Caption));
                                    break;
                                case LibQtyLimit.UnequalToZero:
                                    switch (colDataType[name])
                                    {
                                        case LibDataType.Int32:
                                            right = LibSysUtils.ToInt32(curRow[name]) != 0;
                                            break;
                                        case LibDataType.Int64:
                                            right = LibSysUtils.ToInt64(curRow[name]) != 0;
                                            break;
                                        case LibDataType.Numeric:
                                            right = LibSysUtils.ToDecimal(curRow[name]) != 0;
                                            break;
                                        case LibDataType.Float:
                                            right = LibSysUtils.ToSingle(curRow[name]) != 0;
                                            break;
                                        case LibDataType.Double:
                                            right = LibSysUtils.ToDouble(curRow[name]) != 0;
                                            break;
                                        case LibDataType.Byte:
                                            right = LibSysUtils.ToByte(curRow[name]) != 0;
                                            break;
                                    }
                                    if (!right)
                                        curBcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("字段{0}必须不等于0。", table.Columns[name].Caption));
                                    break;
                                case LibQtyLimit.ZeroBetweenOne:
                                    int tempValue = LibSysUtils.ToInt32(curRow[name]);
                                    right = tempValue >= 0 && tempValue <= 1;
                                    if (!right)
                                        curBcf.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("字段{0}必须大于等于0小于等于1。", table.Columns[name].Caption));
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }

}
