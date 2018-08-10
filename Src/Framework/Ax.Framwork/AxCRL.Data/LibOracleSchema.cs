using AxCRL.Comm.Utils;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Data
{

    public class LibOracleDbSchema : ILibDbSchema
    {
        /// <summary>
        /// 对于具有唯一性约束的数据的处理的Sql语句.
        /// Key是数据表名，Value是Sql语句
        /// </summary>
        public Dictionary<string, string> DicUniqueDataSql { get; set; }
        private LibOracleSchema _DbSchema = null;
        /// <summary>
        /// 构建数据库架构的接口
        /// </summary>
        private LibOracleSchema DbSchema
        {
            get
            {
                if (_DbSchema == null)
                    _DbSchema = new LibOracleSchema();
                return _DbSchema;
            }
        }

        /// <summary>
        /// 通过DataSet创建数据库表，注意此方法先删除相关表，再重新创建表
        /// </summary>
        /// <param name="dataSet"></param>
        public void CreateTables(DataSet dataSet)
        {
            foreach (DataTable table in dataSet.Tables)
            {
                bool isVirtual = table.ExtendedProperties.ContainsKey(TableProperty.IsVirtual) ? (bool)table.ExtendedProperties[TableProperty.IsVirtual] : false;
                if (!isVirtual)
                {
                    //删除表
                    DbSchema.DropTable(table.TableName);
                    //创建表
                    DbSchema.CreateTable(table);
                }
            }
        }
        /// <summary>
        /// 通过DataSet更新数据库表
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="isDelete">是否删除已不存在的字段</param>
        public void UpdateTables(DataSet dataSet, bool isDelete)
        {
            foreach (DataTable table in dataSet.Tables)
            {
                bool isVirtual = table.ExtendedProperties.ContainsKey(TableProperty.IsVirtual) ? (bool)table.ExtendedProperties[TableProperty.IsVirtual] : false;
                if (!isVirtual)
                {
                    if (this.DicUniqueDataSql != null && this.DicUniqueDataSql.ContainsKey(table.TableName))
                        DbSchema.UniqueDataSql = this.DicUniqueDataSql[table.TableName];
                    DbSchema.UpdateTable(table, isDelete);
                }
            }
        }
        /// <summary>
        /// 检测数据库中对象是否存在
        /// </summary>
        /// <param name="name">对象名称</param>
        /// <param name="type">对象类别</param>
        /// <returns></returns>
        public bool ExistsObject(string name, DBObjectType type)
        {
            LibDataAccess dataAccess = new LibDataAccess();
            string sql = "";
            switch (type)
            {
                case DBObjectType.Table:
                    sql = string.Format(@"select count(TABLE_NAME) FROM all_tables WHERE TABLE_NAME = '{0}'",
                        name);
                    break;

                case DBObjectType.Column:
                    string[] nameArrar = name.Split('.');
                    sql = string.Format(@"select column_name from user_tab_columns where table_name=upper('{0}') and column_name=upper('{0}')", nameArrar[0], nameArrar[1]);
                    break;

                default:
                    break;
            }

            string _count = dataAccess.ExecuteScalar(sql).ToString();

            return _count == "0" ? false : true;

        }
        private class LibOracleSchema
        {
            /// <summary>
            /// 对于具有唯一性约束的数据的处理的Sql语句.
            /// </summary>
            public string UniqueDataSql { get; set; }
            public const string BIGINT = "INTEGER";
            public const string BINARY = "NCLOB";
            public const string BIT = "INTEGER";
            public const string DECIMAL = "NUMBER";
            public const string FLOAT = "NUMBER";
            public const string INT = "INTEGER";
            public const string NVARCHAR = "NVARCHAR2";
            public const string TINYINT = "INTEGER";
            public const string VARCHAR = "VARCHAR2";
            public const string DateTime = "datetime";
            public const string Date = "date";
            public const string Time = "time";

            /// <summary>
            /// Initialize using connection.
            /// </summary>
            public LibOracleSchema()
            {
            }


            /// <summary>
            /// Drop the table from the database.
            /// </summary>
            /// <param name="tableName"></param>
            public void DropTable(string tableName)
            {
                string drop = string.Format(@"declare num number; 
                                        begin
                                        select count(TABLE_NAME) into num FROM all_tables WHERE TABLE_NAME = '{0}';
                                        if num>0 then
                                        execute immediate 'drop table {0}';
                                        end if;
                                        end;", tableName);
                LibDataAccess dataAccess = new LibDataAccess();
                dataAccess.ExecuteNonQuery(drop);
            }


            private string GetPkStr(DataTable table)
            {
                StringBuilder pkBuilder = new StringBuilder();
                int length = table.PrimaryKey.Length;
                for (int p = 0; p < length; p++)
                {
                    if (p != 0)
                        pkBuilder.Append(',');
                    pkBuilder.Append(table.PrimaryKey[p].ColumnName);
                }
                return pkBuilder.ToString();
            }

            private string GetFieldInfo(DataColumn col, bool addQuto = false)
            {
                string fieldStr = string.Empty;
                string name = col.ColumnName;
                int size = col.MaxLength;
                string defaultStr = string.Empty;
                //string constraint = string.Format("CONSTRAINT NN_{0}_{1} NOT NULL", shortTableName, name);
                LibDataType libDataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                switch (libDataType)
                {
                    case LibDataType.Text:
                        if (!string.IsNullOrEmpty(LibSysUtils.ToString(col.DefaultValue)))
                            defaultStr = string.Format(" DEFAULT '{0}'", addQuto ? string.Format("'{0}'", col.DefaultValue) : col.DefaultValue); //外层execute immediate已经有一个单引号，所以这里是有2个单引号转义
                        fieldStr = string.Format("{0} {1}({2}){3}", name, LibOracleSchema.VARCHAR, size, defaultStr);
                        break;
                    case LibDataType.NText:
                        if (!string.IsNullOrEmpty(LibSysUtils.ToString(col.DefaultValue)))
                            defaultStr = string.Format(" DEFAULT '{0}'", addQuto ? string.Format("'{0}'", col.DefaultValue) : col.DefaultValue);
                        fieldStr = string.Format("{0} {1}({2}){3}", name, LibOracleSchema.NVARCHAR, size, defaultStr);
                        break;
                    case LibDataType.Int32:
                        defaultStr = string.Format(" DEFAULT {0}", col.DefaultValue);
                        fieldStr = string.Format("{0} {1}{2}", name, LibOracleSchema.INT, defaultStr);
                        break;
                    case LibDataType.Int64:
                        defaultStr = string.Format(" DEFAULT {0}", col.DefaultValue);
                        fieldStr = string.Format("{0} {1}{2}", name, LibOracleSchema.BIGINT, defaultStr);
                        break;
                    case LibDataType.Numeric:
                        defaultStr = string.Format(" DEFAULT {0}", col.DefaultValue);
                        fieldStr = string.Format("{0} {1}{2}", name, LibOracleSchema.DECIMAL, defaultStr);
                        break;
                    case LibDataType.Float:
                    case LibDataType.Double:
                        defaultStr = string.Format(" DEFAULT {0}", col.DefaultValue);
                        fieldStr = string.Format("{0} {1}{2}", name, LibOracleSchema.FLOAT, defaultStr);
                        break;
                    case LibDataType.Byte:
                        defaultStr = string.Format(" DEFAULT {0}", col.DefaultValue);
                        fieldStr = string.Format("{0} {1}{2}", name, LibOracleSchema.TINYINT, defaultStr);
                        break;
                    case LibDataType.Boolean:
                        if (LibSysUtils.ToBoolean(col.DefaultValue))
                            defaultStr = " DEFAULT 1";
                        else
                            defaultStr = " DEFAULT 0";
                        fieldStr = string.Format("{0} {1}{2}", name, LibOracleSchema.BIT, defaultStr);
                        break;
                    case LibDataType.Binary:
                        if (!string.IsNullOrEmpty(LibSysUtils.ToString(col.DefaultValue)))
                            defaultStr = string.Format(" DEFAULT '{0}'", addQuto ? string.Format("'{0}'", col.DefaultValue) : col.DefaultValue);
                        fieldStr = string.Format("{0} {1}{2}", name, LibOracleSchema.BINARY, defaultStr);
                        break;
                    default:
                        break;
                }
                return fieldStr;
            }

            public void CreateTable(DataTable table)
            {
                string tableName = table.TableName;
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("CREATE TABLE {0}(", tableName);
                foreach (DataColumn col in table.Columns)
                {
                    FieldType fieldType = col.ExtendedProperties.ContainsKey(FieldProperty.FieldType) ? (FieldType)col.ExtendedProperties[FieldProperty.FieldType] : FieldType.None;
                    if (fieldType == FieldType.None)
                    {
                        builder.AppendFormat("{0},", GetFieldInfo(col));
                    }
                }
                string pkStr = GetPkStr(table);
                builder.AppendLine(string.Format("constraints PK_{0} primary key({1})", tableName, pkStr));
                builder.Append(")");
                LibDataAccess dataAccess = new LibDataAccess();
                dataAccess.ExecuteNonQuery(builder.ToString());
                List<string> sqlList = new List<string>();
                DBIndexCollection dbIndexes = table.ExtendedProperties.ContainsKey(TableProperty.DBIndex) ? (DBIndexCollection)table.ExtendedProperties[TableProperty.DBIndex] : null;
                if (dbIndexes != null)
                {
                    foreach (DBIndex item in dbIndexes)
                    {
                        sqlList.Add(CreateIndexSql(item, tableName));
                    }
                }
                if (sqlList.Count > 0)
                {
                    foreach (var sql in sqlList)
                    {
                        dataAccess.ExecuteNonQuery(sql);
                    }
                }
            }

            private string CreateIndexSql(DBIndex item, string tableName)
            {
                StringBuilder strBuilder = new StringBuilder();
                foreach (DBIndexField indexField in item.DbIndexFields)
                {
                    strBuilder.AppendFormat("{0} {1},", indexField.Name, indexField.IndexOrderWay.ToString());
                }
                strBuilder.Remove(strBuilder.Length - 1, 1);
                return string.Format("create {0} index {1} on {2}({3})", item.IsUnique ? "unique" : string.Empty, item.Name, tableName, strBuilder.ToString());
            }


            private string ReturnOracleType(LibDataType dataType)
            {
                string dbType = string.Empty;
                switch (dataType)
                {
                    case LibDataType.Text:
                        dbType = LibOracleSchema.VARCHAR;
                        break;
                    case LibDataType.NText:
                        dbType = LibOracleSchema.NVARCHAR;
                        break;
                    case LibDataType.Int32:
                        dbType = "NUMBER"; //conn.getschema时候为NUMBER
                        break;
                    case LibDataType.Int64:
                        dbType = "NUMBER";
                        break;
                    case LibDataType.Numeric:
                        dbType = LibOracleSchema.DECIMAL;
                        break;
                    case LibDataType.Float:
                        dbType = LibOracleSchema.FLOAT;
                        break;
                    case LibDataType.Double:
                        dbType = LibOracleSchema.FLOAT;
                        break;
                    case LibDataType.Byte:
                        dbType = "NUMBER";
                        break;
                    case LibDataType.Boolean:
                        dbType = "NUMBER";
                        break;
                    case LibDataType.Binary:
                        dbType = LibOracleSchema.BINARY;
                        break;
                }
                return dbType;
            }

            private Dictionary<string, DbFieldInfo> GetDbFieldInfo(LibDataAccess dataAccess, string tableName)
            {
                Dictionary<string, DbFieldInfo> dic = new Dictionary<string, DbFieldInfo>();
                using (IDataReader reader = dataAccess.ExecuteDataReader(string.Format("select column_name,data_default,char_length from all_tab_columns where table_name='{0}'", tableName)))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            if (!dic.ContainsKey(LibSysUtils.ToString(reader[0])))
                            {
                                DbFieldInfo info = new DbFieldInfo(LibSysUtils.ToString(reader[1]), LibSysUtils.ToInt32(reader[2]));
                                dic.Add(LibSysUtils.ToString(reader[0]), info);
                            }
                        }
                    } while (reader.NextResult());
                }
                return dic;
            }

            private bool HasFieldChanged(DataColumn col, string dataType, Dictionary<string, DbFieldInfo> defaultDic)
            {
                bool hasDiff = false;
                LibDataType tempDataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                string dbType = ReturnOracleType(tempDataType);
                string curDefaultValue = string.Empty;
                bool isText = tempDataType == LibDataType.Text || tempDataType == LibDataType.NText || tempDataType == LibDataType.Binary;
                DbFieldInfo dbFieldInfo = defaultDic[col.ColumnName];
                if (isText)
                    curDefaultValue = LibSysUtils.ToString(col.DefaultValue);
                else if (tempDataType == LibDataType.Boolean)
                {
                    if ((bool)col.DefaultValue)
                        curDefaultValue = "1";
                }
                else
                {
                    curDefaultValue = col.DefaultValue.ToString();
                }
                if (string.Compare(dbType, dataType, true) != 0 || (isText && col.MaxLength != dbFieldInfo.Length))
                {
                    hasDiff = true;
                }
                else
                {
                    string defaultValue = string.Empty;
                    defaultValue = dbFieldInfo.DefaultValue;
                    if (curDefaultValue != defaultValue)
                    {
                        hasDiff = true;
                    }
                }
                return hasDiff;
            }


            public void UpdateTable(DataTable table, bool isDelete)
            {
                LibDataAccess dataAccess = new LibDataAccess();
                decimal count = (decimal)dataAccess.ExecuteScalar(string.Format("SELECT COUNT(*) FROM all_tables WHERE TABLE_NAME = {0}", LibStringBuilder.GetQuotString(table.TableName)));
                if (count == 0)
                {
                    CreateTable(table);
                }
                else
                {
                    DataTable dtSchema = null;
                    using (DbConnection conn = dataAccess.CreateConnection())
                    {
                        conn.Open();
                        dtSchema = conn.GetSchema("Columns", new string[] { null, table.TableName });
                    }
                    if (dtSchema != null)
                    {
                        Dictionary<string, DbFieldInfo> defaultDic = GetDbFieldInfo(dataAccess, table.TableName);
                        //对删除的非聚集索引进行处理
                        StringBuilder tempBuilder = new StringBuilder();
                        List<string> deleteIndexList = new List<string>();
                        List<string> addIndexList = new List<string>();
                        DBIndexCollection dbIndex = table.ExtendedProperties[TableProperty.DBIndex] as DBIndexCollection;
                        CompareDBIndex(table.TableName, dbIndex, addIndexList, deleteIndexList);
                        if (deleteIndexList.Count > 0)
                        {
                            foreach (var item in deleteIndexList)
                            {
                                dataAccess.ExecuteNonQuery(item);
                            }
                        }
                        //再对列进行处理
                        Dictionary<string, bool> dic = new Dictionary<string, bool>();
                        foreach (DataColumn col in table.Columns)
                        {
                            FieldType fieldType = FieldType.None;
                            if (col.ExtendedProperties.ContainsKey(FieldProperty.FieldType))
                                fieldType = (FieldType)col.ExtendedProperties[FieldProperty.FieldType];
                            if (fieldType == FieldType.None)
                            {
                                dic.Add(col.ColumnName, false);
                            }
                        }
                        foreach (DataRow row in dtSchema.Rows)
                        {
                            string columnName = row["COLUMN_NAME"].ToString();
                            bool isFind = false;
                            foreach (DataColumn col in table.Columns)
                            {
                                if (!dic.ContainsKey(col.ColumnName))
                                    continue;
                                if (string.Compare(col.ColumnName, columnName, true) == 0)
                                {
                                    StringBuilder strBuilder = new StringBuilder();
                                    string dataType = row["DATATYPE"].ToString();
                                    bool hasDiff = HasFieldChanged(col, dataType, defaultDic);
                                    if (hasDiff)
                                    {
                                        tempBuilder.AppendLine(string.Format("execute immediate 'alter table {0} modify({1})';", table.TableName, GetFieldInfo(col, true)));
                                    }
                                    isFind = true;
                                    dic[col.ColumnName] = true;
                                    break;
                                }
                            }
                            if (isDelete && !isFind)
                            {
                                tempBuilder.AppendLine(string.Format("execute immediate 'alter table {0} drop column {1}';", table.TableName, columnName));
                            }
                        }
                        foreach (var item in dic)
                        {
                            if (!item.Value)
                            {
                                DataColumn col = table.Columns[item.Key];
                                tempBuilder.AppendLine(string.Format("execute immediate 'alter table {0} add({1})';", table.TableName, GetFieldInfo(col, true)));
                            }
                        }
                        if (tempBuilder.Length > 0)
                        {
                            StringBuilder testBuild = new StringBuilder();
                            testBuild.AppendLine("begin");
                            testBuild.Append(tempBuilder.ToString());
                            testBuild.AppendLine("end;");
                            dataAccess.ExecuteNonQuery(testBuild.ToString());
                        }
                        //如果主键异动，先删除聚集索引
                        bool isPkChange = IsPkChange(table.TableName, table.PrimaryKey);
                        //先执行对于具有唯一性约束的字段的数据更新处理Sql
                        if (string.IsNullOrEmpty(this.UniqueDataSql) == false && (isPkChange || addIndexList.Count > 0))
                        {
                            dataAccess.ExecuteNonQuery(this.UniqueDataSql);
                        }
                        //对主键的进行标识
                        if (isPkChange)
                        {
                            string pkStr = GetPkStr(table);
                            if (!string.IsNullOrEmpty(LibSysUtils.ToString(dataAccess.ExecuteScalar(string.Format("select constraint_name from dba_constraints where constraint_name = 'PK_{0}'", table.TableName)))))
                                dataAccess.ExecuteNonQuery(string.Format("alter table {0} drop constraint PK_{0}", table.TableName));
                            //一般情况下如果删除主键约束，索引会自动删除。但是目前有存在未删除的情况，所以确保删除
                            if (!string.IsNullOrEmpty(LibSysUtils.ToString(dataAccess.ExecuteScalar(string.Format("select * from  user_ind_columns where INDEX_NAME='PK_{0}'", table.TableName)))))
                                dataAccess.ExecuteNonQuery(string.Format("drop index PK_{0}", table.TableName));
                            dataAccess.ExecuteNonQuery(string.Format("alter table {0} add constraint PK_{0} primary key ({1})", table.TableName, pkStr));
                        }
                        //对表的新增非聚集索引进行处理
                        foreach (var item in addIndexList)
                        {
                            dataAccess.ExecuteNonQuery(item);
                        }
                    }
                }
            }

            private void CompareDBIndex(string tableName, DBIndexCollection modelIndexs,
                                 List<string> addIndexs, List<string> deleteIndexs)
            {
                Dictionary<string, DBIndexInfo> dbIndexs = GetDBIndex(tableName);
                if (modelIndexs != null)
                {
                    foreach (DBIndex item in modelIndexs)
                    {
                        string indexName = item.Name.ToUpper();
                        if (dbIndexs.ContainsKey(indexName))
                        {
                            //已存在的索引，判断有否修改
                            DBIndexInfo dbIndexInfo = dbIndexs[indexName];
                            dbIndexInfo.IsExist = true;
                            bool hasChange = false;
                            if (dbIndexInfo.IndexFields.Count != item.DbIndexFields.Count ||
                                dbIndexInfo.IsUnique != item.IsUnique)
                            {
                                hasChange = true;
                            }
                            else
                            {
                                for (int i = 0; i < item.DbIndexFields.Count; i++)
                                {
                                    if (string.Compare(item.DbIndexFields[i].Name, dbIndexInfo.IndexFields[i].Name, true) != 0 ||
                                        item.DbIndexFields[i].IndexOrderWay != dbIndexInfo.IndexFields[i].IndexOrderWay)
                                    {
                                        hasChange = true;
                                        break;
                                    }
                                }
                            }
                            if (hasChange)
                            {
                                //索引变更，则先删除，再新增
                                deleteIndexs.Add(string.Format("drop index {0}", item.Name));
                                addIndexs.Add(CreateIndexSql(item, tableName));
                            }

                        }
                        else
                        {
                            //新增的索引
                            addIndexs.Add(CreateIndexSql(item, tableName));
                        }
                    }
                }
                foreach (KeyValuePair<string, DBIndexInfo> item in dbIndexs)
                {
                    if (!item.Value.IsExist)
                    {
                        //模型已不存在次索引，需要删除
                        deleteIndexs.Add(string.Format("drop index {0}", item.Key));
                    }
                }
            }

            /// <summary>
            /// 获取表的索引
            /// </summary>
            /// <param name="tableName"></param>
            /// <returns></returns>
            private Dictionary<string, DBIndexInfo> GetDBIndex(string tableName)
            {
                Dictionary<string, DBIndexInfo> dic = new Dictionary<string, DBIndexInfo>();
                string sql = string.Format("select t.INDEX_NAME as IndexName,t.DESCEND as IsDesc,t.COLUMN_NAME as ColumnName,i.uniqueness as IsUnique from user_ind_columns t,user_indexes i where t.index_name = i.index_name and " +
                                           "t.table_name='{0}'", tableName);
                string pkIndex = string.Format("PK_{0}", tableName);
                LibDataAccess dataAccess = new LibDataAccess();
                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        string idxName = reader["IndexName"].ToString();
                        if (string.Compare(pkIndex, idxName, true) == 0)
                            continue;
                        bool isUnique = Convert.ToString(reader["IsUnique"]) == "UNIQUE";
                        IndexOrderWay orderWay = Convert.ToString(reader["IsDesc"]) == "ASC" ? IndexOrderWay.ASC : IndexOrderWay.DESC;
                        string colName = reader["ColumnName"].ToString();
                        if (!dic.ContainsKey(idxName))
                        {
                            DBIndexInfo indexs = new DBIndexInfo();
                            indexs.IsUnique = isUnique;
                            dic.Add(idxName, indexs);
                        }
                        DBIndexInfo dbIndexs = dic[idxName];
                        DBIndexField indexFiled = new DBIndexField(colName, orderWay);
                        dbIndexs.IndexFields.Add(indexFiled);
                    }
                }
                return dic;
            }

            private class DBIndexInfo
            {
                private bool _IsExist = false;
                private bool _IsUnique;
                private DBIndexFieldCollection _IndexFields;

                public DBIndexFieldCollection IndexFields
                {
                    get
                    {
                        if (_IndexFields == null)
                            _IndexFields = new DBIndexFieldCollection();
                        return _IndexFields;
                    }
                }
                public bool IsUnique
                {
                    get { return _IsUnique; }
                    set { _IsUnique = value; }
                }
                /// <summary>
                /// 标识是否在模型上依然存在
                /// </summary>
                public bool IsExist
                {
                    get { return _IsExist; }
                    set { _IsExist = value; }
                }
            }

            /// <summary>
            /// 判断主键是否有异动
            /// </summary>
            /// <param name="tableName"></param>
            /// <param name="pks"></param>
            /// <returns></returns>
            private bool IsPkChange(string tableName, DataColumn[] pks)
            {
                bool change = false;
                LibDataAccess dataAccess = new LibDataAccess();
                HashSet<string> dbPks = new HashSet<string>();
                using (IDataReader reader = dataAccess.ExecuteDataReader(string.Format("select cu.COLUMN_NAME from user_cons_columns cu, user_constraints au where cu.constraint_name = au.constraint_name and au.constraint_type = 'P' and au.table_name = '{0}'", tableName)))
                {
                    while (reader.Read())
                    {
                        string colName = reader["COLUMN_NAME"].ToString();
                        dbPks.Add(colName);
                    }
                }
                if (pks.Length == dbPks.Count)
                {
                    foreach (var item in pks)
                    {
                        if (!dbPks.Contains(item.ColumnName))
                        {
                            change = true;
                            break;
                        }
                    }
                }
                else
                {
                    change = true;
                }
                //主键是否变更
                return change;
            }

            private class DbFieldInfo
            {
                public int Length { get; set; }
                public string DefaultValue { get; set; }
                public DbFieldInfo(string defaultValue, int length)
                {
                    DefaultValue = defaultValue;
                    Length = length;
                }
            }

        }
    }

}
