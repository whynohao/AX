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
    /// <summary>
    ///  根据DataSet创建表结构
    /// </summary>
    public class LibSqlServerDbSchema : ILibDbSchema
    {
        /// <summary>
        /// 对于具有唯一性约束的数据的处理的Sql语句.
        /// Key是数据表名，Value是Sql语句
        /// </summary>
        public Dictionary<string, string> DicUniqueDataSql { get; set; }
        private LibDbSchema _DbSchema = null;
        /// <summary>
        /// 构建数据库架构的接口
        /// </summary>
        private LibDbSchema DbSchema
        {
            get
            {
                if (_DbSchema == null)
                    _DbSchema = new LibDbSchema();
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
                    TableSchema scheam = new TableSchema(table);
                    DbSchema.CreateTable(scheam);
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
                    TableSchema scheam = new TableSchema(table);
                    if (this.DicUniqueDataSql != null && this.DicUniqueDataSql.ContainsKey(table.TableName))
                        DbSchema.UniqueDataSql = this.DicUniqueDataSql[table.TableName];
                    DbSchema.UpdateTable(scheam, isDelete);
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
                    sql = string.Format(@"SELECT name FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}]') 
                            AND type in (N'U')", name);
                    break;
                case DBObjectType.Column:
                    string[] nameArrar = name.Split('.');
                    sql = string.Format(@"SELECT name FROM sys.syscolumns WHERE name='{0}' and id=(SELECT MAX(id) FROM sys.objects WHERE object_id = OBJECT_ID(N'[{1}]') 
                            AND type in (N'U'))", nameArrar[1], nameArrar[0]);
                    break;
                default:
                    break;
            }

            string _name = dataAccess.ExecuteScalar(sql) as string;

            return string.IsNullOrEmpty(_name) ? false : true;

        }
        /// <summary>
        /// 控制数据库架构
        /// </summary>
        private class LibDbSchema
        {
            /// <summary>
            /// 对于具有唯一性约束的数据的处理的Sql语句.
            /// </summary>
            public string UniqueDataSql { get; set; }

            public const string BIGINT = "bigint";
            public const string BINARY = "nvarchar(max)";
            public const string BIT = "bit";
            public const string DECIMAL = "decimal";
            public const string FLOAT = "float";
            public const string INT = "int";
            public const string NVARCHAR = "nvarchar";
            public const string TINYINT = "tinyint";
            public const string VARCHAR = "varchar";

            /// <summary>
            /// Initialize using connection.
            /// </summary>
            public LibDbSchema()
            {
            }


            /// <summary>
            /// Drop the table from the database.
            /// </summary>
            /// <param name="tableName"></param>
            public void DropTable(string tableName)
            {
                string checkDelete = GetDropTable(tableName, false);
                LibDataAccess dataAccess = new LibDataAccess();
                dataAccess.ExecuteNonQuery(checkDelete);
            }


            /// <summary>
            /// Creates a DROP TABLE statement preceeded
            /// by an optional corresponding IF EXISTS statement.
            /// </summary>
            /// <param name="tableName">Name of table to use.</param>
            /// <param name="includeGo">True to generate IF EXISTS statement.</param>
            /// <returns>Generated string.</returns>
            public string GetDropTable(string tableName, bool includeGo)
            {
                // IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
                // DROP TABLE [dbo].[Categories]
                string checkDelete = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + tableName + "]') AND type in (N'U'))";
                checkDelete += Environment.NewLine + "DROP TABLE [dbo].[" + tableName + "]";
                if (includeGo)
                    checkDelete += Environment.NewLine + "go";
                return checkDelete;
            }

            public void CreateTable(TableSchema schema)
            {
                StringBuilder pkStr = new StringBuilder();
                foreach (string item in schema.PRIMARY_KEY)
                {
                    pkStr.AppendFormat("[{0}] ASC,", item);
                }
                StringBuilder columnStr = new StringBuilder();
                StringBuilder defaultValueStr = new StringBuilder();
                StringBuilder indexStr = new StringBuilder();
                foreach (ColumnSchema item in schema.ColumnSchemaList)
                {
                    columnStr.AppendLine(string.Format("{0},", item.ColumnTypeStr));
                    if (!string.IsNullOrEmpty(item.DefaultValueStr))
                    {
                        defaultValueStr.AppendLine(string.Format("ALTER TABLE [dbo].[{0}] ADD {1} FOR [{2}]", schema.Name, item.DefaultValueStr, item.Name));
                    }
                }
                foreach (string item in schema.DBIndexStr)
                {
                    indexStr.AppendLine(item);
                }
                if (columnStr.Length > 0)
                    columnStr.Remove(columnStr.Length - 1, 1);
                if (pkStr.Length > 0)
                    pkStr.Remove(pkStr.Length - 1, 1);
                StringBuilder strBuilder = new StringBuilder();
                //strBuilder.AppendLine(string.Format("USE [{0}]", InitialCatalog));
                strBuilder.AppendLine("SET ANSI_NULLS ON ");
                strBuilder.AppendLine("SET QUOTED_IDENTIFIER ON ");
                strBuilder.AppendLine("SET ANSI_PADDING ON ");
                strBuilder.AppendLine(string.Format("CREATE TABLE [dbo].[{0}]({1}", schema.Name, columnStr));
                strBuilder.AppendLine(string.Format("CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ", schema.Name));
                strBuilder.AppendLine(string.Format("({0}) ", pkStr));//主键
                strBuilder.AppendLine("WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF," +
                                      " ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY]");
                strBuilder.AppendLine("SET ANSI_PADDING OFF ");
                //添加默认值
                strBuilder.Append(defaultValueStr.ToString());
                //添加索引
                strBuilder.Append(indexStr);
                LibDataAccess dataAccess = new LibDataAccess();
                dataAccess.ExecuteNonQuery(strBuilder.ToString());
            }


            public void UpdateTable(TableSchema schema, bool isDelete)
            {
                DataTable table = null;
                LibDataAccess dataAccess = new LibDataAccess();
                int count = (int)dataAccess.ExecuteScalar(string.Format("select count(*) from sysobjects where id = object_id({0})", LibStringBuilder.GetQuotString(schema.Name)));
                if (count == 0)
                {
                    CreateTable(schema);
                }
                else
                {
                    using (DbConnection conn = dataAccess.CreateConnection())
                    {
                        conn.Open();
                        table = conn.GetSchema("Columns", new string[] { null, null, schema.Name, null });
                    }
                    if (table != null)
                    {
                        List<string> sql = new List<string>();
                        List<string> sqlSecond = new List<string>();//第二批执行的sql语句，一般在数据列加完以后执行唯一约束、主键等设置
                        //如果主键异动，先删除聚集索引
                        bool isPkChange = IsPkChange(schema.Name, schema.PRIMARY_KEY);
                        if (isPkChange)
                        {
                            sql.Add(GetDropPkConstraintSql(schema.Name));
                        }
                        //对删除的非聚集索引进行处理
                        List<string> deleteIndexList = new List<string>();
                        List<string> addIndexList = new List<string>();
                        CompareDBIndex(schema.Name, schema.DBIndexs, addIndexList, deleteIndexList);
                        foreach (var item in deleteIndexList)
                        {
                            sql.Add(item);
                        }
                        //再对列进行处理
                        Dictionary<string, bool> dic = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                        foreach (ColumnSchema col in schema.ColumnSchemaList)
                        {
                            dic.Add(col.Name, false);
                        }
                        foreach (DataRow row in table.Rows)
                        {
                            string columnName = row["COLUMN_NAME"].ToString();
                            string dataType = row["DATA_TYPE"].ToString();
                            int size = LibSysUtils.ToInt32(row["CHARACTER_MAXIMUM_LENGTH"]);
                            int digit = LibSysUtils.ToInt32(row["NUMERIC_SCALE"]);
                            string defualtValue = LibSysUtils.ToString(row["COLUMN_DEFAULT"]);
                            bool isFind = false;
                            foreach (ColumnSchema col in schema.ColumnSchemaList)
                            {
                                if (string.Compare(col.Name, columnName, true) == 0)
                                {
                                    StringBuilder strBuilder = new StringBuilder();
                                    if (string.Compare(col.ColumnType, dataType, true) != 0 ||
                                        (col.Size > 0 && col.Size != size) || col.Digit != digit)
                                    {
                                        foreach (var item in schema.PRIMARY_KEY)
                                        {
                                            if (string.Compare(col.Name, item, true) == 0)
                                            {
                                                if (!isPkChange)
                                                {
                                                    isPkChange = true;
                                                    sql.Add(GetDropPkConstraintSql(schema.Name));
                                                }
                                                break;
                                            }
                                        }
                                        //删除默认值约束
                                        strBuilder.Append(GetDropDefaultValueCheckSql(schema.Name, columnName));
                                        //修改字段
                                        strBuilder.AppendLine("ALTER TABLE [dbo].[{0}] ALTER column {1} ");
                                        //新建默认值约束
                                        strBuilder.AppendFormat("ALTER TABLE [dbo].[{0}] ADD {1} FOR [{2}]", schema.Name, col.DefaultValueStr, columnName);
                                        sql.Add(string.Format(strBuilder.ToString(), schema.Name, col.ColumnTypeStr));
                                    }
                                    else if (string.Compare(string.Format(col.DefaultValue), defualtValue) != 0)
                                    {
                                        foreach (var item in schema.PRIMARY_KEY)
                                        {
                                            if (string.Compare(col.Name, item, true) == 0)
                                            {
                                                if (!isPkChange)
                                                {
                                                    isPkChange = true;
                                                    sql.Add(GetDropPkConstraintSql(schema.Name));
                                                }
                                                break;
                                            }
                                        }
                                        //删除默认值约束
                                        strBuilder.Append(GetDropDefaultValueCheckSql(schema.Name, columnName));
                                        //新建默认值约束
                                        strBuilder.AppendFormat("ALTER TABLE [dbo].[{0}] ADD {1} FOR [{2}]", schema.Name, col.DefaultValueStr, columnName);
                                        sql.Add(string.Format(strBuilder.ToString(), schema.Name, columnName));
                                    }
                                    isFind = true;
                                    dic[columnName] = true;
                                    break;
                                }
                            }
                            if (isDelete && !isFind)
                            {
                                //原先存在。现在不存在，则考虑删除
                                //有默认约束依赖该字段，先删除默认约束，再删除字段
                                //删除默认值约束
                                StringBuilder strBuilder = new StringBuilder();
                                strBuilder.Append(GetDropDefaultValueCheckSql(schema.Name, columnName));
                                //删除字段
                                strBuilder.AppendLine(string.Format("ALTER TABLE [dbo].[{0}] DROP [{1}] {2}", schema.Name, columnName));
                                sql.Add(strBuilder.ToString());
                            }
                        }
                        foreach (var item in dic)
                        {
                            if (!item.Value)
                            {
                                StringBuilder strBuilder = new StringBuilder();
                                ColumnSchema col = schema.ColumnSchemaList.Find(c => c.Name == item.Key);
                                //新增列
                                strBuilder.AppendLine(string.Format("ALTER TABLE [dbo].[{0}] ADD {1}", schema.Name, col.ColumnTypeStr));
                                //新增列默认值约束
                                strBuilder.Append(col.DefaultValueStr);
                                sql.Add(strBuilder.ToString());
                            }
                        }
                        //第二批执行的是索引、主键、唯一约束等
                        //先添加对于具有唯一性约束的字段的数据更新处理Sql，以便其先执行
                        if (string.IsNullOrEmpty(this.UniqueDataSql) == false && (isPkChange || addIndexList.Count > 0))
                        {
                            sqlSecond.Add(this.UniqueDataSql);
                        }
                        //对主键的进行标识
                        if (isPkChange)
                        {
                            sqlSecond.Add(GetAddPkConstraintSql(schema.Name, schema.PRIMARY_KEY));
                        }
                        //对表的新增非聚集索引进行处理
                        foreach (var item in addIndexList)
                        {
                            sqlSecond.Add(item);
                        }
                        if (sql.Count > 0 || sqlSecond.Count > 0)
                        {
                            StringBuilder sqlBuilder = new StringBuilder();
                            foreach (var item in sql)
                            {
                                sqlBuilder.Append(item);
                            }
                            StringBuilder sqlBuilderSecond = new StringBuilder();
                            foreach (var item in sqlSecond)
                            {
                                sqlBuilderSecond.Append(item);
                            }
                            //提交更新
                            LibDBTransaction tran = dataAccess.BeginTransaction();
                            try
                            {
                                if (sqlBuilder.Length > 0)
                                    dataAccess.ExecuteNonQuery(sqlBuilder.ToString());
                                //第二批执行的是索引、主键、唯一约束等
                                if (sqlBuilderSecond.Length > 0)
                                    dataAccess.ExecuteNonQuery(sqlBuilderSecond.ToString());
                                tran.Commit();
                            }
                            catch (Exception exp)
                            {
                                LibCommUtils.AddOutput(@"Update", string.Format("升级数据表异常。\r\nsql:{0}\r\n异常:{1}\r\nStackTrace:{2}",
                                    sqlBuilder.ToString(), exp.Message, exp.StackTrace));
                                tran.Rollback();
                            }
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
                        if (dbIndexs.ContainsKey(item.Name))
                        {
                            //已存在的索引，判断有否修改
                            DBIndexInfo dbIndexInfo = dbIndexs[item.Name];
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
                                deleteIndexs.Add(GetDropNonclusteredIndexSql(tableName, item.Name));
                                addIndexs.Add(GetAddNonclusteredIndexSql(item, tableName));
                            }

                        }
                        else
                        {
                            //新增的索引
                            addIndexs.Add(GetAddNonclusteredIndexSql(item, tableName));
                        }
                    }
                }
                foreach (KeyValuePair<string, DBIndexInfo> item in dbIndexs)
                {
                    if (!item.Value.IsExist)
                    {
                        //模型已不存在次索引，需要删除
                        deleteIndexs.Add(GetDropNonclusteredIndexSql(tableName, item.Key));
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
                string sql = string.Format("SELECT idx.name as IndexName,idx.is_unique as IsUnique,idxCol.is_descending_key as IsDesc,col.name as ColumnName " +
                            "FROM  sys.indexes idx    JOIN sys.index_columns idxCol  " +
                            "ON (idx.object_id = idxCol.object_id  " +
                            "AND idx.index_id = idxCol.index_id) " +
                            "JOIN sys.tables tab ON (idx.object_id = tab.object_id) " +
                            "JOIN sys.columns col ON (idx.object_id = col.object_id " +
                            "AND idxCol.column_id = col.column_id) " +
                            "where tab.name='{0}' and idx.is_primary_key<>1 " +
                            "order by idx.index_id ", tableName);
                LibDataAccess dataAccess = new LibDataAccess();
                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        string idxName = reader["IndexName"].ToString();
                        bool isUnique = Convert.ToBoolean(reader["IsUnique"]);
                        IndexOrderWay orderWay = Convert.ToBoolean(reader["IsDesc"]) ? IndexOrderWay.DESC : IndexOrderWay.ASC;
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
            private bool IsPkChange(string tableName, string[] pks)
            {
                bool change = false;
                HashSet<string> dbPks = new HashSet<string>();
                LibDataAccess dataAccess = new LibDataAccess();
                using (IDataReader reader = dataAccess.ExecuteDataReader(string.Format("EXEC sp_pkeys @table_name='{0}'", tableName)))
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
                        if (!dbPks.Contains(item))
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

            private string GetDropNonclusteredIndexSql(string tableName, string dbIndexName)
            {
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.AppendLine("IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'{1}')");
                strBuilder.AppendLine("DROP INDEX [{1}] ON [dbo].[{0}] WITH ( ONLINE = OFF )");
                return string.Format(strBuilder.ToString(), tableName, dbIndexName);
            }

            /// <summary>
            /// 增加非聚集索引
            /// </summary>
            /// <param name="newDBIndex"></param>
            /// <param name="tableName"></param>
            /// <returns></returns>
            private string GetAddNonclusteredIndexSql(DBIndex newDBIndex, string tableName)
            {
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.AppendLine("CREATE {0} NONCLUSTERED INDEX [{1}] ON [dbo].[{2}] ");
                strBuilder.AppendLine("({3})");
                strBuilder.AppendLine("WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, " +
                                      "SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF," +
                                      " ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]");
                StringBuilder indexFieldStr = new StringBuilder();
                foreach (var item in newDBIndex.DbIndexFields)
                {
                    indexFieldStr.AppendFormat("[{0}] {1},", item.Name, item.IndexOrderWay == IndexOrderWay.ASC ? "ASC" : "DESC");
                }
                indexFieldStr.Remove(indexFieldStr.Length - 1, 1);
                return string.Format(strBuilder.ToString(), newDBIndex.IsUnique ? "UNIQUE" : string.Empty, newDBIndex.Name, tableName, indexFieldStr.ToString());
            }

            /// <summary>
            /// 删除聚集索引
            /// </summary>
            /// <param name="tableName"></param>
            /// <returns></returns>
            private string GetDropPkConstraintSql(string tableName)
            {
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.AppendLine("IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'PK_{0}')");
                strBuilder.AppendLine("ALTER TABLE [dbo].{0} DROP CONSTRAINT [PK_{0}]");
                return string.Format(strBuilder.ToString(), tableName);
            }

            private string GetAddPkConstraintSql(string tableName, string[] pks)
            {
                StringBuilder pkStr = new StringBuilder();
                foreach (string item in pks)
                {
                    pkStr.AppendFormat("[{0}] ASC,", item);
                }
                if (pkStr.Length > 0)
                    pkStr.Remove(pkStr.Length - 1, 1);
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.AppendLine("ALTER TABLE [dbo].[{0}] ADD  CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ");
                strBuilder.AppendLine("(");
                strBuilder.AppendLine(pkStr.ToString());
                strBuilder.AppendLine(")");
                strBuilder.AppendLine("WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]");
                return string.Format(strBuilder.ToString(), tableName);
            }
            /// <summary>
            /// 删除列的默认值约束
            /// </summary>
            /// <param name="tableName"></param>
            /// <param name="columnName"></param>
            /// <returns></returns>
            private string GetDropDefaultValueCheckSql(string tableName, string columnName)
            {
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.AppendLine("IF  EXISTS (SELECT * FROM dbo.sysobjects " +
                                      "WHERE id = OBJECT_ID(N'[DF_{0}_{1}]') AND type = 'D')");
                strBuilder.AppendLine("BEGIN");
                strBuilder.AppendLine("ALTER TABLE [dbo].[{0}] DROP CONSTRAINT [DF_{0}_{1}]");
                strBuilder.AppendLine("END");
                return string.Format(strBuilder.ToString(), tableName, columnName);
            }
        }

        private class TableSchema
        {            
            private string _Name;
            private string[] _PRIMARY_KEY;
            private DBIndexCollection _DBIndexs = null;
            private List<ColumnSchema> _ColumnSchemaList = null;
            private List<string> _DBIndexStr;

            public TableSchema()
            {

            }

            private string[] GetPkStr(DataColumn[] columns)
            {
                string[] ret = new string[columns.Length];
                for (int i = 0; i < columns.Length; i++)
                {
                    ret[i] = columns[i].ColumnName;
                }
                return ret;
            }

            public TableSchema(DataTable table)
            {
                this.Name = table.TableName;
                if (table.PrimaryKey == null || table.PrimaryKey.Length == 0)
                    throw new Exception(string.Format("表{0}的主键为空!", this.Name));

                this._PRIMARY_KEY = GetPkStr(table.PrimaryKey);
                DBIndexs = table.ExtendedProperties.ContainsKey(TableProperty.DBIndex) ? (DBIndexCollection)table.ExtendedProperties[TableProperty.DBIndex] : null;
                if (DBIndexs != null)
                {
                    foreach (DBIndex item in this.DBIndexs)
                    {
                        StringBuilder strBuilder = new StringBuilder();
                        foreach (DBIndexField indexField in item.DbIndexFields)
                        {
                            strBuilder.AppendFormat("{0} {1},", indexField.Name, indexField.IndexOrderWay.ToString());
                        }
                        strBuilder.Remove(strBuilder.Length - 1, 1);
                        this.DBIndexStr.Add(string.Format("CREATE {0} NONCLUSTERED INDEX [{1}] ON [dbo].[{2}] ({3})" +
                                            "WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, " +
                                            "SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF," +
                                            " ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)" +
                                            " ON [PRIMARY]", item.IsUnique ? "UNIQUE" : string.Empty,
                                            item.Name, this.Name, strBuilder.ToString()));

                    }
                }
                foreach (DataColumn item in table.Columns)
                {
                    FieldType fieldType = item.ExtendedProperties.ContainsKey(FieldProperty.FieldType) ? (FieldType)item.ExtendedProperties[FieldProperty.FieldType] : FieldType.None;
                    if (fieldType == FieldType.None)
                        this.ColumnSchemaList.Add(new ColumnSchema(item, this.Name));
                }
            }

            public List<ColumnSchema> ColumnSchemaList
            {
                get
                {
                    if (_ColumnSchemaList == null)
                        _ColumnSchemaList = new List<ColumnSchema>();
                    return _ColumnSchemaList;
                }
            }

            public List<string> DBIndexStr
            {
                get
                {
                    if (_DBIndexStr == null)
                        _DBIndexStr = new List<string>();
                    return _DBIndexStr;
                }
            }

            /// <summary>
            /// 主键
            /// </summary>
            public string[] PRIMARY_KEY
            {
                get { return _PRIMARY_KEY; }
                set { _PRIMARY_KEY = value; }
            }
            /// <summary>
            /// 表名
            /// </summary>
            public string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }

            public DBIndexCollection DBIndexs
            {
                get { return _DBIndexs; }
                set { _DBIndexs = value; }
            }
        }

        private class ColumnSchema
        {
            private string _DefaultValueStr;
            private string _ColumnTypeStr;
            private string _Name;

            public string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }
            private string _DefaultValue;

            public string DefaultValue
            {
                get { return _DefaultValue; }
                set { _DefaultValue = value; }
            }
            private string _ColumnType;

            public string ColumnType
            {
                get { return _ColumnType; }
                set { _ColumnType = value; }
            }
            private int _Size;

            public int Size
            {
                get { return _Size; }
                set { _Size = value; }
            }
            private int _Digit;

            public int Digit
            {
                get { return _Digit; }
                set { _Digit = value; }
            }

            public ColumnSchema()
            {

            }

            public ColumnSchema(DataColumn field, string tableName)
            {
                this.Name = field.ColumnName;
                this.Size = field.MaxLength;
                LibDataType libDataType = (LibDataType)field.ExtendedProperties[FieldProperty.DataType];
                string defaultValueForamt = "CONSTRAINT [DF_{0}_{1}]  DEFAULT {2} ";
                switch (libDataType)
                {
                    case LibDataType.Text:
                        this.ColumnType = LibDbSchema.VARCHAR;
                        this.DefaultValue = string.Format("('{0}')", LibSysUtils.ToString(field.DefaultValue));
                        this._ColumnTypeStr = string.Format("[{0}] [{1}]({2}) NOT NULL ", this.Name, LibDbSchema.VARCHAR, this.Size);
                        this._DefaultValueStr = string.Format(defaultValueForamt, tableName, this.Name, this.DefaultValue);
                        break;
                    case LibDataType.NText:
                        this.ColumnType = LibDbSchema.NVARCHAR;
                        this.DefaultValue = string.Format("('{0}')", LibSysUtils.ToString(field.DefaultValue));
                        this._ColumnTypeStr = string.Format("[{0}] [{1}]({2}) NOT NULL", this.Name, LibDbSchema.NVARCHAR, this.Size);
                        this._DefaultValueStr = string.Format(defaultValueForamt, tableName, this.Name, this.DefaultValue);
                        break;
                    case LibDataType.Int32:
                        this.ColumnType = LibDbSchema.INT;
                        this.DefaultValue = string.Format("(({0}))", LibSysUtils.ToInt32(field.DefaultValue));
                        this._ColumnTypeStr = string.Format("[{0}] [{1}] NOT NULL", this.Name, LibDbSchema.INT);
                        this._DefaultValueStr = string.Format(defaultValueForamt, tableName, this.Name, this.DefaultValue);
                        break;
                    case LibDataType.Int64:
                        this.ColumnType = LibDbSchema.BIGINT;
                        this.DefaultValue = string.Format("(({0}))", LibSysUtils.ToInt64(field.DefaultValue));
                        this._ColumnTypeStr = string.Format("[{0}] [{1}] NOT NULL", this.Name, LibDbSchema.BIGINT);
                        this._DefaultValueStr = string.Format(defaultValueForamt, tableName, this.Name, this.DefaultValue);
                        break;
                    case LibDataType.Numeric:
                        this.ColumnType = LibDbSchema.DECIMAL;
                        this.DefaultValue = string.Format("(({0}))", LibSysUtils.ToDecimal(field.DefaultValue));
                        this._ColumnTypeStr = string.Format("[{0}] [{1}](18,9) NOT NULL", this.Name, LibDbSchema.DECIMAL);
                        this._DefaultValueStr = string.Format(defaultValueForamt, tableName, this.Name, this.DefaultValue);
                        break;
                    case LibDataType.Float:
                    case LibDataType.Double:
                        this.ColumnType = LibDbSchema.FLOAT;
                        this.DefaultValue = string.Format("(({0}))", LibSysUtils.ToSingle(field.DefaultValue));
                        this._ColumnTypeStr = string.Format("[{0}] [{1}] NOT NULL", this.Name, LibDbSchema.FLOAT);
                        this._DefaultValueStr = string.Format(defaultValueForamt, tableName, this.Name, this.DefaultValue);
                        break;
                    case LibDataType.Byte:
                        this.ColumnType = LibDbSchema.TINYINT;
                        this.DefaultValue = string.Format("(({0}))", LibSysUtils.ToByte(field.DefaultValue));
                        this._ColumnTypeStr = string.Format("[{0}] [{1}] NOT NULL", this.Name, LibDbSchema.TINYINT);
                        this._DefaultValueStr = string.Format(defaultValueForamt, tableName, this.Name, this.DefaultValue);
                        break;
                    case LibDataType.Boolean:
                        this.ColumnType = LibDbSchema.BIT;
                        this.DefaultValue = string.Format("(({0}))", LibSysUtils.ToInt32(field.DefaultValue));
                        this._ColumnTypeStr = string.Format("[{0}] [{1}] NOT NULL", this.Name, LibDbSchema.BIT);
                        this._DefaultValueStr = string.Format(defaultValueForamt, tableName, this.Name, this.DefaultValue);
                        break;
                    case LibDataType.Binary:
                        this.ColumnType = LibDbSchema.BINARY;
                        this.DefaultValue = string.Format("('{0}')", LibSysUtils.ToString(field.DefaultValue));
                        this._ColumnTypeStr = string.Format("[{0}] {1} NULL", this.Name, LibDbSchema.BINARY);
                        this._DefaultValueStr = string.Format(defaultValueForamt, tableName, this.Name, this.DefaultValue);
                        break;
                    default:
                        break;
                }
            }

            /// <summary>
            /// 默认值
            /// </summary>
            public string DefaultValueStr
            {
                get { return _DefaultValueStr; }
                set { _DefaultValueStr = value; }
            }
            /// <summary>
            /// 数据类型
            /// </summary>
            public string ColumnTypeStr
            {
                get { return _ColumnTypeStr; }
                set { _ColumnTypeStr = value; }
            }
        }

    }

}
