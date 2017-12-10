using AxCRL.Comm.Utils;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Data.SqlBuilder
{
    public class SqlBuilder : ISqlBuilder
    {
        private string _ProgId;

        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }
        private LibSqlModel _SqlModel = null;

        public LibSqlModel SqlModel
        {
            get
            {
                if (_SqlModel == null)
                    _SqlModel = LibSqlModelCache.Default.GetSqlModel(this.ProgId);
                return _SqlModel;
            }
        }


        public SqlBuilder(string progId)
        {
            this._ProgId = progId;
        }

        private string[] GetPkStr(DataTable table)
        {
            DataColumn[] primaryColumns = table.PrimaryKey;
            string[] ret = new string[primaryColumns.Length];
            for (int i = 0; i < primaryColumns.Length; i++)
            {
                ret[i] = primaryColumns[i].ColumnName;
            }
            return ret;
        }

        private string GetWhereSql(object[] pks, char prefix, bool isStoreProcedure = false)
        {
            if (pks == null)
                return string.Empty;
            StringBuilder whereBuilder = new StringBuilder();
            string[] fields = GetPkStr(this.SqlModel.Tables[0]);
            int length = fields.Length;
            for (int i = 0; i < length; i++)
            {
                object value = pks[i];
                if (isStoreProcedure)
                {
                    whereBuilder.AppendFormat("{0}.{1}={2}", prefix, fields[i], value);
                }
                else
                {
                    TypeCode typeCode = Type.GetTypeCode(value.GetType());
                    if (typeCode == TypeCode.String)
                        whereBuilder.AppendFormat("{0}.{1}={2}", prefix, fields[i], LibStringBuilder.GetQuotObject(value));
                    else if (typeCode == TypeCode.Boolean)
                        whereBuilder.AppendFormat("{0}.{1}={2}", prefix, fields[i], (int)value);
                    else
                        whereBuilder.AppendFormat("{0}.{1}={2}", prefix, fields[i], value);
                }
                if (i != length - 1)
                    whereBuilder.Append(" And ");
            }
            return whereBuilder.ToString();
        }

        public string BuildBrowseStoreProcedure(BillType billType)
        {
            StringBuilder builder = new StringBuilder();
            object[] paramList = null;
            string paramStr = string.Empty;
            bool hasCondition = billType == BillType.Bill || billType == BillType.Master;
            if (hasCondition)
            {
                DataColumn[] primaryKeys = this.SqlModel.Tables[0].PrimaryKey;
                int length = primaryKeys.Length;
                paramList = new object[length];
                StringBuilder paramBuild = new StringBuilder();
                for (int i = 0; i < length; i++)
                {
                    if (i != 0)
                        paramBuild.Append(',');
                    paramList[i] = string.Format("@{0}", primaryKeys[i].ColumnName);
                    Type dataType = primaryKeys[i].DataType;
                    int size = (int)primaryKeys[i].MaxLength;
                    if (dataType == typeof(string))
                        paramBuild.AppendLine(string.Format("{0} varchar({1}) ", paramList[i], size));
                    else if (dataType == typeof(int))
                        paramBuild.AppendLine(string.Format("{0} int ", paramList[i]));
                    else if (dataType == typeof(long))
                        paramBuild.AppendLine(string.Format("{0} bigint ", paramList[i]));
                }
                paramStr = paramBuild.ToString();
            }
            for (int i = 0; i < this.SqlModel.Tables.Count; i++)
            {
                LibSqlParser parser = new LibSqlParser() { SqlModel = SqlModel };
                char prefix = parser.GetTablePrefix(i);
                bool isVirtual = this.SqlModel.Tables[i].ExtendedProperties.ContainsKey(TableProperty.IsVirtual) ? (bool)this.SqlModel.Tables[i].ExtendedProperties[TableProperty.IsVirtual] : false;
                if (!isVirtual)
                {
                    string sort = string.Empty;
                    if (this.SqlModel.Tables[i].Columns.Contains("ROWNO"))
                        sort = string.Format("{0}.ROWNO", prefix);
                    builder.AppendLine(parser.ParesSelectSql(i, string.Format("{0}.*", prefix), GetWhereSql(paramList, prefix, true), sort, string.Empty, false));
                }
            }
            string sql = @"CREATE PROCEDURE {0} 
                         {1}
                         AS 
                         BEGIN 
                         SET NOCOUNT ON; 
                         {2} 
                         END";
            sql = string.Format(sql, string.Format("{0}", this.ProgId.Replace('.', '_')), paramStr, builder.ToString());
            return sql;
        }

        public string BuildBrowseStoreProcedureByOracle(BillType billType)
        {
            StringBuilder builder = new StringBuilder();
            object[] paramList = null;
            string paramStr = string.Empty;
            StringBuilder paramBuild = new StringBuilder();
            bool hasCondition = billType == BillType.Bill || billType == BillType.Master;
            if (hasCondition)
            {
                DataColumn[] primaryKeys = this.SqlModel.Tables[0].PrimaryKey;
                int length = primaryKeys.Length;
                paramList = new object[length];
                for (int i = 0; i < length; i++)
                {
                    if (i != 0)
                        paramBuild.Append(',');
                    paramList[i] = string.Format("{0}_val", primaryKeys[i].ColumnName);
                    Type dataType = primaryKeys[i].DataType;
                    int size = (int)primaryKeys[i].MaxLength;
                    if (dataType == typeof(string))
                        paramBuild.AppendFormat("{0} in VARCHAR2", paramList[i], size);
                    else if (dataType == typeof(int))
                        paramBuild.AppendFormat("{0} in INTEGER", paramList[i]);
                    else if (dataType == typeof(long))
                        paramBuild.AppendFormat("{0} in NUMBER", paramList[i]);
                }
            }
            StringBuilder cursorBuilder = new StringBuilder();
            int index = 0;
            for (int i = 0; i < this.SqlModel.Tables.Count; i++)
            {
                LibSqlParser parser = new LibSqlParser() { SqlModel = SqlModel };
                char prefix = parser.GetTablePrefix(i);
                bool isVirtual = this.SqlModel.Tables[i].ExtendedProperties.ContainsKey(TableProperty.IsVirtual) ? (bool)this.SqlModel.Tables[i].ExtendedProperties[TableProperty.IsVirtual] : false;
                if (!isVirtual)
                {
                    string sort = string.Empty;
                    if (this.SqlModel.Tables[i].Columns.Contains("ROWNO"))
                        sort = string.Format("{0}.ROWNO", prefix);
                    if (index > 0 || (index == 0 && paramBuild.Length > 0))
                        cursorBuilder.Append(",");
                    index++;
                    cursorBuilder.AppendFormat("cur_out_{0} out SYS_REFCURSOR", index);
                    builder.AppendLine(string.Format("open cur_out_{0} for", index));
                    builder.AppendLine(string.Format("{0};", parser.ParesSelectSql(i, string.Format("{0}.*", prefix), GetWhereSql(paramList, prefix, true), sort, string.Empty, false)));
                }
            }
            paramStr = string.Format("({0}{1})", paramBuild.ToString(), cursorBuilder.ToString());
            StringBuilder spBuilder = new StringBuilder();
            string sql = @"create or replace procedure {0}{1} is 
                         begin 
                         {2} end {0};";
            sql = string.Format(sql, string.Format("{0}", this.ProgId.Replace('.', '_')), paramStr, builder.ToString());
            return sql;
        }

        public string GetQueryAllSql(object[] pks)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.SqlModel.Tables.Count; i++)
            {
                LibSqlParser parser = new LibSqlParser() { SqlModel = SqlModel };
                char prefix = parser.GetTablePrefix(i);
                bool isVirtual = this.SqlModel.Tables[i].ExtendedProperties.ContainsKey(TableProperty.IsVirtual) ? (bool)this.SqlModel.Tables[i].ExtendedProperties[TableProperty.IsVirtual] : false;
                if (!isVirtual)
                    builder.AppendLine(parser.ParesSelectSql(i, string.Format("{0}.*", prefix), GetWhereSql(pks, prefix), string.Empty, string.Empty, false));
            }
            return builder.ToString();
        }

        public string GetQueryAllSql(string condition)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.SqlModel.Tables.Count; i++)
            {
                LibSqlParser parser = new LibSqlParser() { SqlModel = SqlModel };
                char prefix = parser.GetTablePrefix(i);
                bool isVirtual = this.SqlModel.Tables[i].ExtendedProperties.ContainsKey(TableProperty.IsVirtual) ? (bool)this.SqlModel.Tables[i].ExtendedProperties[TableProperty.IsVirtual] : false;
                if (!isVirtual)
                    builder.AppendLine(parser.ParesSelectSql(i, string.Format("{0}.*", prefix), condition, string.Empty, string.Empty, false));
            }
            return builder.ToString();
        }

        public string GetFuzzySql(int tableIndex, string query, string condition = "")
        {
            LibSqlParser parser = new LibSqlParser() { SqlModel = SqlModel };
            string id = SqlModel.Tables[tableIndex].PrimaryKey[SqlModel.Tables[tableIndex].PrimaryKey.Length - 1].ColumnName;
            char prefix = parser.GetTablePrefix(tableIndex);
            string ret = string.Empty;
            if (tableIndex == 0)
            {
                string name = id.Replace("ID", "NAME");
                if (id == name)
                    ret = parser.ParesSelectSql(tableIndex, string.Format("{0}.{1}", prefix, id), string.Format("({0}.{1} LIKE {2} OR {0}.{3} LIKE {2}) {4}", prefix, id, LibStringBuilder.GetQuotString(string.Format("%{0}%", query)), name, condition), string.Empty, string.Empty, false);
                else
                    ret = parser.ParesSelectSql(tableIndex, string.Format("{0}.{1},{0}.{2}", prefix, id, name), string.Format("({0}.{1} LIKE {2} OR {0}.{3} LIKE {2}) {4}", prefix, id, LibStringBuilder.GetQuotString(string.Format("%{0}%", query)), name, condition), string.Empty, string.Empty, false);
            }
            else
            {   //表身关联是ROW_ID字段
                ret = parser.ParesSelectSql(tableIndex, string.Format("{0}.{1}", prefix, id), string.Format("{0}.{1} LIKE {2} {3}", prefix, id, LibStringBuilder.GetQuotString(string.Format("%{0}%", query)), condition), string.Empty, string.Empty, false);
            }
            return ret;
        }

        public string GetFuzzySql(int tableIndex, RelativeSourceCollection relSources, string query, string condition = "", string parentColumnName = "")
        {
            if (parentColumnName == null)
                parentColumnName = string.Empty;
            LibSqlParser parser = new LibSqlParser() { SqlModel = SqlModel };
            string id = SqlModel.Tables[tableIndex].PrimaryKey[SqlModel.Tables[tableIndex].PrimaryKey.Length - 1].ColumnName;
            char prefix = parser.GetTablePrefix(tableIndex);
            string ret = string.Empty;

            RelativeSource curRelSource = null;//找到当前对应的RelativeSource
            foreach (RelativeSource item in relSources)
            {
                if (string.Compare(this.ProgId, item.RelSource, true) == 0)
                {
                    curRelSource = item;
                    break;
                }
            }
            //添加Orderby 字段
            string orderBySql = string.Empty;
            if (curRelSource.OrderbyColumns != null && curRelSource.OrderbyColumns.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string key in curRelSource.OrderbyColumns.Keys)
                {
                    builder.Append(string.Format(" A.{0} {1} ,", key, (curRelSource.OrderbyColumns[key] ? "asc" : "desc")));
                }
                builder.Remove(builder.Length - 1, 1);
                orderBySql = " order by " + builder.ToString();
            }
            //是否需要查询其他列的信息
            bool isQueryOtherValue = string.IsNullOrEmpty(query) == false && curRelSource.IdNameFilterFields != null && curRelSource.IdNameFilterFields.Count > 0;
            string name = string.Empty;
            string tableName = SqlModel.Tables[tableIndex].TableName;
            if (tableIndex == 0)
            {
                if (id.IndexOf("ID") < 0)
                {
                    if (curRelSource.RelFields.Count > 0)
                    {
                        name = curRelSource.RelFields[0].Name;
                    }
                    else
                    {
                        name = id;
                    }
                }
                else
                {
                    name = id.Replace("ID", "NAME");
                }
                if (id == name)
                    ret = parser.ParesSelectSql(tableIndex, string.Format("{0}.{1}", prefix, id), string.Format("({0}.{1} LIKE {2} OR {0}.{3} LIKE {2}) {4}", prefix, id, LibStringBuilder.GetQuotString(string.Format("%{0}%", query)), name, condition), string.Empty, string.Empty, false);
                else
                    ret = parser.ParesSelectSql(tableIndex, string.Format("{0}.{1},{0}.{2},'' as OTHERVALUE" 
                        + ((string.IsNullOrEmpty(parentColumnName.Trim())) ? "": ",{0}.{3} "),
                        prefix, id, name, parentColumnName.Trim()), string.Format("({0}.{1} LIKE {2} OR {0}.{3} LIKE {2}) {4}", prefix, id, LibStringBuilder.GetQuotString(string.Format("%{0}%", query)), name, condition), string.Empty, string.Empty, false);
            }
            else
            {   //表身关联是ROW_ID字段
                if (curRelSource.RelFields.Count == 0)
                {
                    ret = parser.ParesSelectSql(tableIndex, string.Format("{0}.{1}", prefix, id), string.Format("{0}.{1} LIKE {2} {3}", prefix, id, LibStringBuilder.GetQuotString(string.Format("%{0}%", query)), condition), string.Empty, string.Empty, false);
                }
                else
                {
                    name = curRelSource.RelFields[0].Name;
                    ret = parser.ParesSelectSql(tableIndex, string.Format("{0}.{1},{0}.{2},'' as OTHERVALUE"
                        + ((string.IsNullOrEmpty(parentColumnName.Trim())) ? "" : ",{0}.{3} "),
                        prefix, id, name, parentColumnName.Trim()), string.Format("{0}.{1} LIKE {2} {3}", prefix, id, LibStringBuilder.GetQuotString(string.Format("%{0}%", query)), condition), string.Empty, string.Empty, false);
                }
            }
            if (string.IsNullOrEmpty(orderBySql) == false)
                ret += orderBySql;//添加Orderby 语句
                 
            if (isQueryOtherValue)
            {
                //需要检索除Id Name以外的其他指定列，暂时只是第一个RelSource中的IdName有效 Zhangkj 20170124
                StringBuilder stringBuilder = new StringBuilder();
                foreach (RelField field in curRelSource.IdNameFilterFields)
                {
                    if (field == null)
                        continue;
                    stringBuilder.Append(string.Format(" UNION SELECT {0}.{1},{0}.{2},CAST({0}.{3} AS nvarchar(500)) AS OTHERVALUE" + 
                        (string.IsNullOrEmpty(parentColumnName.Trim()) ? "" : ",{0}.{6} " )+ " FROM {4} AS {0} WHERE {0}.{3} LIKE {5} ",
                        prefix, id, name, field.Name, tableName, LibStringBuilder.GetQuotString(string.Format("%{0}%", query)), parentColumnName.Trim()));
                }
                if (string.IsNullOrEmpty(orderBySql) == false)
                    stringBuilder.Append(orderBySql);
                ret += stringBuilder.ToString();               
            }   
            return ret;
        }

        public string GetQuerySql(int tableIndex, string selectFields, string where)
        {
            LibSqlParser parser = new LibSqlParser() { SqlModel = SqlModel };
            string ret = parser.ParesSelectSql(tableIndex, selectFields, where, string.Empty, string.Empty, false);
            return ret;
        }

        public string GetQuerySql(int tableIndex, string selectFields, string where, string sortCondition)
        {
            LibSqlParser parser = new LibSqlParser() { SqlModel = SqlModel };
            string ret = parser.ParesSelectSql(tableIndex, selectFields, where, sortCondition, string.Empty, false);
            return ret;
        }

        public string GetQuerySql(int tableIndex, string selectFields, string where, string sortCondition, string groupCondition, bool distinct)
        {
            LibSqlParser parser = new LibSqlParser() { SqlModel = SqlModel };
            string ret = parser.ParesSelectSql(tableIndex, selectFields, where, sortCondition, groupCondition, distinct);
            return ret;
        }

    }
    /// <summary>
    /// 查询语法解析器
    /// </summary>
    public class LibSqlParser
    {
        private Dictionary<string, string> _TableMapPrefix = null;
        private Dictionary<string, Dictionary<int, string>> _TableMapAssist = null;
        private LibSqlModel _SqlModel = null;
        private string _CurrMaxPrefix = "A";
        private int _CurrTableIndex = 0;
        private StringBuilder _JoinBuilder = null;
        private HashSet<string> _OnHashSet = null;
        private readonly List<char> compareStr = new List<char>() { '=', '>', '<' };
        private string _MainTableName;
        /// <summary>
        /// 主表
        /// </summary>
        public string MainTableName
        {
            get { return _MainTableName; }
            set { _MainTableName = value; }
        }

        /// <summary>
        /// 存储on语句
        /// </summary>
        public HashSet<string> OnHashSet
        {
            get
            {
                if (_OnHashSet == null)
                    _OnHashSet = new HashSet<string>();
                return _OnHashSet;
            }
        }

        public StringBuilder JoinBuilder
        {
            get
            {
                if (_JoinBuilder == null)
                    _JoinBuilder = new StringBuilder();
                return _JoinBuilder;
            }
        }

        public LibSqlModel SqlModel
        {
            get { return _SqlModel; }
            set { _SqlModel = value; }
        }

        public Dictionary<string, string> TableMapPrefix
        {
            get
            {
                if (_TableMapPrefix == null)
                    _TableMapPrefix = new Dictionary<string, string>();
                return _TableMapPrefix;
            }
        }

        public Dictionary<string, Dictionary<int, string>> TableMapAssist
        {
            get
            {
                if (_TableMapAssist == null)
                    _TableMapAssist = new Dictionary<string, Dictionary<int, string>>();
                return _TableMapAssist;
            }
        }

        /// <summary>
        /// 解析查询语法
        /// </summary>
        /// <param name="tableIndex">主表索引</param>
        /// <param name="selectFields">查询字符串</param>
        /// <param name="where">查询条件</param>
        /// <param name="sortCondition">排序条件</param>
        /// <param name="groupCondition">分组条件</param>
        /// <param name="distinct">唯一性</param>
        /// <returns>返回Sql语句</returns>
        public string ParesSelectSql(int tableIndex, string selectFields, string where, string sortCondition, string groupCondition, bool distinct)
        {
            string header = distinct ? "Select Distinct " : "Select ";
            StringBuilder builder = new StringBuilder(header);
            //确立查询主表
            ConfirmMainTable(tableIndex);
            //解析查询字段并且拼接Join语句
            builder.Append(ParseSelectField(selectFields));
            //解析查询条件
            builder.Append(ParseWhere(where));
            //解析分组条件
            builder.Append(ParseGroupBy(groupCondition));
            //解析排序条件
            builder.Append(ParseOrderBy(sortCondition));
            return builder.ToString();
        }

        private void AddTableMapAssist(string tableName, int fieldIdx, string prefix)
        {
            if (!TableMapAssist.ContainsKey(tableName))
            {
                TableMapAssist.Add(tableName, new Dictionary<int, string>());
            }
            if (TableMapAssist[tableName].ContainsKey(fieldIdx))
            {
                TableMapAssist[tableName][fieldIdx] = prefix;
            }
            else
            {
                TableMapAssist[tableName].Add(fieldIdx, prefix);
            }
        }

        /// <summary>
        /// 解析Join语句
        /// </summary>
        /// <param name="sqlModel">数据语法源模型</param>
        /// <param name="prefix">表前缀</param>
        /// <param name="name">字段名</param>
        /// <param name="realTableName">存在于数据库的表名</param>
        /// <param name="realFieldName">存在于数据库的字段名</param>
        /// <returns>返回caseWhen</returns>
        private string ParseFieldCore(LibSqlModel sqlModel, char prefix, string name, out string realTableName, out string realFieldName)
        {
            string caseWhen = null;
            int tempTableIndex = GetTablePrefix(prefix);
            List<FieldRelation> fieldRelations = sqlModel.GetFieldRelation(tempTableIndex, name);
            FieldRelation last = null;
            StringBuilder caseWhenBuilder = null;
            string currPrefix = string.Empty;
            if (fieldRelations != null)
            {
                foreach (FieldRelation fieldRelation in fieldRelations)
                {
                    last = fieldRelation;
                    if (!string.IsNullOrEmpty(fieldRelation.Join))
                    {
                        DataTable table = sqlModel.Tables[tempTableIndex];
                        Dictionary<string, FieldAddr> fieldAddres = (Dictionary<string, FieldAddr>)table.ExtendedProperties[TableProperty.FieldAddrDic];
                        FieldAddr fieldAddr;

                        fieldAddres.TryGetValue(name, out fieldAddr);
                        string onStr = fieldRelation.GetOnStr();//获取on语句
                        if (OnHashSet.Contains(onStr)) //如果之前存在，则说明join语句已经加入了，不需要执行后面的操作
                        {
                            Dictionary<int, string> tempDic;
                            TableMapAssist.TryGetValue(fieldRelation.TableName, out tempDic);
                            if (tempDic != null && fieldAddr != null)
                            {
                                if (tempDic.ContainsKey(fieldAddr.FieldIndex))
                                {
                                    currPrefix = tempDic[fieldAddr.FieldIndex];
                                    continue;
                                }
                                else
                                {
                                    currPrefix = _CurrMaxPrefix;
                                    if (!TableMapPrefix.ContainsKey(fieldRelation.TableName))
                                    {
                                        TableMapPrefix.Add(fieldRelation.TableName, _CurrMaxPrefix);
                                        AddTableMapAssist(fieldRelation.TableName, fieldAddr.FieldIndex, _CurrMaxPrefix);
                                    }
                                    else if (string.Compare(MainTableName, fieldRelation.TableName, true) != 0)
                                    {
                                        TableMapPrefix[fieldRelation.TableName] = _CurrMaxPrefix;
                                        AddTableMapAssist(fieldRelation.TableName, fieldAddr.FieldIndex, _CurrMaxPrefix);
                                    }
                                    _CurrMaxPrefix = InternalAdd(_CurrMaxPrefix);
                                    OnHashSet.Add(onStr);
                                }
                            }
                            else
                            {
                                currPrefix = TableMapPrefix[fieldRelation.TableName];
                                continue;
                            }
                        }
                        else
                        {//如果不存在则是新的join语句
                            currPrefix = _CurrMaxPrefix;
                            if (!TableMapPrefix.ContainsKey(fieldRelation.TableName))
                            {
                                TableMapPrefix.Add(fieldRelation.TableName, _CurrMaxPrefix);
                                AddTableMapAssist(fieldRelation.TableName, fieldAddr.FieldIndex, _CurrMaxPrefix);
                            }
                            else if (string.Compare(MainTableName, fieldRelation.TableName, true) != 0)
                            {
                                TableMapPrefix[fieldRelation.TableName] = _CurrMaxPrefix;
                                AddTableMapAssist(fieldRelation.TableName, fieldAddr.FieldIndex, _CurrMaxPrefix);
                            }
                            _CurrMaxPrefix = InternalAdd(_CurrMaxPrefix);
                            OnHashSet.Add(onStr);
                        }
                    }
                    else
                    {
                        //非关联字段情况
                        if (TableMapPrefix.ContainsKey(fieldRelation.TableName))
                            currPrefix = TableMapPrefix[fieldRelation.TableName];
                        else
                        {
                            currPrefix = _CurrMaxPrefix;
                            TableMapPrefix.Add(fieldRelation.TableName, _CurrMaxPrefix);
                            _CurrMaxPrefix = InternalAdd(_CurrMaxPrefix);
                            //存在A<B表达式的情况，这个时候传人的不是本SqlModel
                            //如果取的不是当前的表，比如关联了父表，则要增加Join语句
                            if (this.SqlModel == sqlModel && _CurrTableIndex != tempTableIndex)
                            {
                                StringBuilder tempBuilder = new StringBuilder();
                                string joinStr, temp;
                                if (_CurrTableIndex > tempTableIndex)
                                {
                                    int parentIndex = tempTableIndex;
                                    joinStr = " Inner Join ";
                                    DataColumn[] parentPk = this.SqlModel.Tables[parentIndex].PrimaryKey;
                                    temp = TableMapPrefix[fieldRelation.TableName];
                                    for (int i = 0; i < parentPk.Length; i++)
                                    {
                                        tempBuilder.AppendFormat("{0}.{1}=A.{2} And ", temp, parentPk[i].ColumnName, this.SqlModel.Tables[_CurrTableIndex].PrimaryKey[i]);
                                    }
                                }
                                else
                                {
                                    int parentIndex = _CurrTableIndex;
                                    joinStr = " Left Join ";
                                    DataColumn[] parentPk = this.SqlModel.Tables[parentIndex].PrimaryKey;
                                    temp = TableMapPrefix[fieldRelation.TableName];
                                    for (int i = 0; i < parentPk.Length; i++)
                                    {
                                        tempBuilder.AppendFormat("{0}.{1}=A.{2} And ", temp, this.SqlModel.Tables[tempTableIndex].PrimaryKey[i], parentPk[i].ColumnName);
                                    }
                                }
                                tempBuilder.Remove(tempBuilder.Length - 4, 4);
                                JoinBuilder.AppendFormat(" {0} {1} {2} on {3} ", joinStr, fieldRelation.TableName, temp, tempBuilder.ToString());
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(fieldRelation.Join))
                    {
                        StringBuilder onBuilder = new StringBuilder();
                        foreach (FieldOwn[] fieldOwn in fieldRelation.On)
                        {
                            //存在RelPK关联的是父表的字段，这个时候要产生父表Inner join的语法
                            string tableName = fieldOwn[0].TableName;
                            if (!TableMapPrefix.ContainsKey(tableName))
                            {
                                if (this.SqlModel.Tables.Contains(tableName))
                                {
                                    BuildParentTableJoinSql(tableName);
                                    //TableMapPrefix.Add(fieldOwn[0].TableName, _CurrMaxPrefix);
                                    //_CurrMaxPrefix = InternalAdd(_CurrMaxPrefix);
                                    //StringBuilder tempBuilder = new StringBuilder();
                                    //DataColumn[] parentPk = this.SqlModel.Tables[fieldOwn[0].TableName].PrimaryKey;
                                    //string temp = TableMapPrefix[fieldOwn[0].TableName];
                                    //for (int i = 0; i < parentPk.Length; i++)
                                    //{
                                    //    tempBuilder.AppendFormat("{0}.{1}=A.{2} And ", temp, parentPk[i].ColumnName, this.SqlModel.Tables[_CurrTableIndex].PrimaryKey[i]);
                                    //}
                                    //tempBuilder.Remove(tempBuilder.Length - 4, 4);
                                    //JoinBuilder.AppendFormat(" Inner Join {0} {1} on {2} ", fieldOwn[0].TableName, temp, tempBuilder.ToString());
                                }
                            }
                            onBuilder.AppendFormat("{0}.{1}={2}.{3} And ", currPrefix, fieldOwn[1].Name,
                                                                           TableMapPrefix[tableName], fieldOwn[0].Name);
                        }
                        onBuilder.Remove(onBuilder.Length - 4, 4);
                        JoinBuilder.AppendFormat(" {0} {1} {2} on {3} ", fieldRelation.Join, fieldRelation.TableName,
                            currPrefix, onBuilder.ToString());
                    }
                    string groupCondition = fieldRelation.GroupCondition;
                    if (!string.IsNullOrEmpty(groupCondition))
                    {
                        if (caseWhenBuilder == null)
                            caseWhenBuilder = new StringBuilder("Case ");
                        int index = groupCondition.IndexOf('.');
                        Dictionary<char, string> dic = new Dictionary<char, string>();
                        while (index != -1)
                        {
                            char groupPrefix = groupCondition[index - 1];
                            if (!dic.ContainsKey(groupPrefix))
                            {
                                string tableName = this.SqlModel.Tables[GetTablePrefix(groupPrefix)].TableName;
                                if (!TableMapPrefix.ContainsKey(tableName))
                                {
                                    BuildParentTableJoinSql(tableName);
                                }
                                string tempPrefix = TableMapPrefix[tableName];
                                dic.Add(groupPrefix, tempPrefix);
                            }
                            groupCondition = groupCondition.Insert(index, dic[groupPrefix]);
                            groupCondition = groupCondition.Remove(index - 1, 1);
                            index = groupCondition.IndexOf('.', index + 1);
                        }
                        caseWhenBuilder.AppendFormat("When {0} Then {1}.{2} ", groupCondition, TableMapPrefix[fieldRelation.TableName], fieldRelation.Name);
                    }
                }
                if (caseWhenBuilder != null)
                {
                    caseWhenBuilder.AppendFormat("Else Null End As {0}", name);
                    caseWhen = caseWhenBuilder.ToString();
                }
            }
            realTableName = currPrefix;
            if (last == null)
                realFieldName = string.Empty;
            else
                realFieldName = last.Name;
            return caseWhen;
        }

        private void BuildParentTableJoinSql(string tableName)
        {
            TableMapPrefix.Add(tableName, _CurrMaxPrefix);
            _CurrMaxPrefix = InternalAdd(_CurrMaxPrefix);
            StringBuilder tempBuilder = new StringBuilder();
            DataColumn[] parentPk = this.SqlModel.Tables[tableName].PrimaryKey;
            string temp = TableMapPrefix[tableName];
            for (int i = 0; i < parentPk.Length; i++)
            {
                tempBuilder.AppendFormat("{0}.{1}=A.{2} And ", temp, parentPk[i].ColumnName, this.SqlModel.Tables[_CurrTableIndex].PrimaryKey[i]);
            }
            tempBuilder.Remove(tempBuilder.Length - 4, 4);
            JoinBuilder.AppendFormat(" Inner Join {0} {1} on {2} ", tableName, temp, tempBuilder.ToString());
        }
        /// <summary>
        /// 确立查询主表
        /// </summary>
        /// <param name="tableIndex">表索引</param>
        private void ConfirmMainTable(int tableIndex)
        {
            _CurrTableIndex = tableIndex;
            MainTableName = this.SqlModel.Tables[tableIndex].TableName;
            TableMapPrefix.Add(MainTableName, _CurrMaxPrefix);
            JoinBuilder.AppendFormat(" From {0} {1} ", MainTableName, _CurrMaxPrefix);
            _CurrMaxPrefix = InternalAdd(_CurrMaxPrefix);
        }
        /// <summary>
        /// 处理*号的查询字段
        /// </summary>
        /// <param name="selectFields">查询字段</param>
        /// <returns></returns>
        private string ParseStar(string selectFields)
        {
            //if (selectFields == "*")
            //    selectFields = GetAllSelectField();
            //else
            //{
            //处理类似A.*的查询字段
            int index = selectFields.IndexOf('*');
            Dictionary<string, string> store = index == -1 ? null : new Dictionary<string, string>();
            while (index > -1)
            {
                string key = selectFields.Substring(index - 2, 3);
                char prefix = key[0];
                int tableIndex = GetTablePrefix(prefix);
                string value = GetTableAllField(this.SqlModel.Tables[tableIndex], prefix);
                store.Add(key, value);
                index = selectFields.IndexOf('*', (index + 1));
            }
            if (store != null)
            {
                int length = store.Count;
                foreach (KeyValuePair<string, string> item in store)
                {
                    selectFields = selectFields.Replace(item.Key, item.Value);
                }
            }
            //}
            return selectFields;
        }

        /// <summary>
        /// 解析查询字段和拼接Join语句
        /// </summary>
        /// <param name="selectFields">查询字段</param>
        /// <returns>返回查询字段和拼接好的Join语句</returns>
        private string ParseSelectField(string selectFields)
        {
            selectFields = selectFields.Trim();
            selectFields = ParseStar(selectFields);
            //确定查询字段和Join表
            StringBuilder builder = new StringBuilder();
            string[] fieldList = selectFields.Split(',');
            int count = fieldList.Length;
            foreach (string field in fieldList)
            {
                int indexArrow = field.IndexOf('<');
                int length = field.Length;
                int index = field.IndexOf('.');
                if (index < 0)//Zhangkj 20170206 因增加了固定值列，不存在的.,如Select '' as XXX
                {
                    builder.Append(field);
                    count--;
                    if (count > 0)
                        builder.Append(",");                   
                    continue;
                }
                int prefixIndex = index - 1;
                char prefix = field[prefixIndex];
                if (indexArrow == -1)
                {   //处理无箭头关联的字段
                    int fieldStart = index + 1;
                    int fieldEnd = length - index - 1;
                    string name;
                    if (field[length - 1] == ')')
                        name = field.Substring(fieldStart, --fieldEnd);
                    else
                        name = field.Substring(fieldStart, fieldEnd);
                    string asName = string.Empty;
                    int asIndex = name.IndexOf(" as ");
                    if (asIndex == -1)
                        asIndex = name.IndexOf(" AS ");
                    if (asIndex > 0)
                    {
                        asName = name.Substring(asIndex + 3, name.Length - asIndex - 3).Trim();
                        name = name.Substring(0, asIndex).Trim();
                    }
                    string tablePrefix, fieldName, caseWhen;
                    caseWhen = ParseFieldCore(this.SqlModel, prefix, name, out tablePrefix, out fieldName);
                    if (string.IsNullOrEmpty(caseWhen))
                    {
                        bool add; //加入查询字段
                        for (int i = 0; i < length; i++)
                        {
                            if (i == prefixIndex)
                                builder.Append(tablePrefix);
                            else
                            {
                                add = i != fieldStart;
                                if (add)
                                    builder.Append(field[i]);
                                else
                                {
                                    if (string.Compare(name, fieldName, true) == 0)
                                    {
                                        if (string.IsNullOrEmpty(asName))
                                            builder.Append(fieldName);
                                        else
                                            builder.AppendFormat("{0} as {1}", fieldName, asName);
                                    }
                                    else //存在别名
                                    {
                                        if (string.IsNullOrEmpty(asName))
                                            builder.AppendFormat("{0} as {1}", fieldName, name);
                                        else
                                            builder.AppendFormat("{0} as {1}", fieldName, asName);
                                    }
                                    i = fieldEnd + 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        builder.Append(caseWhen);
                    }
                }
                else
                {
                    //处理箭头关联字段
                    string currName = field.Substring(index + 1, indexArrow - index - 1);
                    string selectName;
                    char selectNamePrefix;
                    int fieldEnd = length - indexArrow - 1;
                    selectNamePrefix = field[indexArrow + 1];
                    if (field[length - 1] == ')')
                        selectName = field.Substring(indexArrow + 3, fieldEnd - 3);
                    else
                        selectName = field.Substring(indexArrow + 3, fieldEnd - 2);
                    DataTable table = this.SqlModel.Tables[GetTablePrefix(prefix)];
                    FieldAddr addr = ((Dictionary<string, FieldAddr>)table.ExtendedProperties[TableProperty.FieldAddrDic])[currName];
                    if (addr.RelSourceIndex == -1)
                    {
                        RelativeSourceCollection relSourceColl = table.Columns[addr.FieldIndex].ExtendedProperties[FieldProperty.RelativeSource] as RelativeSourceCollection;
                        if (relSourceColl != null)
                        {
                            foreach (RelativeSource source in relSourceColl)
                            {
                                string tablePrefix, fieldName;
                                LibSqlModel relSourceModel = LibSqlModelCache.Default.GetSqlModel(source.RelSource);
                                ParseFieldCore(relSourceModel, selectNamePrefix, selectName, out tablePrefix, out fieldName);
                                if (!string.IsNullOrEmpty(fieldName))
                                {
                                    //加入查询字段
                                    if (string.Compare(selectName, fieldName, true) == 0)
                                        builder.AppendFormat("{0}.{1}", tablePrefix, fieldName);
                                    else //存在别名
                                        builder.AppendFormat("{0}.{1} as {2}", tablePrefix, fieldName, selectName);
                                    //这种情况下，需要设置关联语法
                                    StringBuilder tempBuilder = new StringBuilder();
                                    DataColumn[] parentPk = relSourceModel.Tables[source.TableIndex].PrimaryKey;
                                    string temp = TableMapPrefix[relSourceModel.Tables[source.TableIndex].TableName];

                                    List<string> curPk = new List<string>();
                                    if (!string.IsNullOrEmpty(source.RelPK))
                                    {
                                        string[] relPks = source.RelPK.Split(';');
                                        if (relPks == null)
                                            curPk.Add(source.RelPK.Split('.')[1]);
                                        else
                                        {
                                            for (int i = 0; i < relPks.Length; i++)
                                                curPk.Add(relPks[i].Split('.')[1]);
                                        }
                                    }
                                    curPk.Add(currName);
                                    for (int i = 0; i < parentPk.Length; i++)
                                    {
                                        tempBuilder.AppendFormat("{0}.{1}=A.{2} And ", temp, parentPk[i].ColumnName, this.SqlModel.Tables[_CurrTableIndex].Columns[curPk[i]]);
                                    }
                                    tempBuilder.Remove(tempBuilder.Length - 4, 4);
                                    if (JoinBuilder.ToString().IndexOf(tempBuilder.ToString()) == -1) //TODO 后续需修改
                                        JoinBuilder.AppendFormat(" Left Join {0} {1} on {2} ", relSourceModel.Tables[source.TableIndex].TableName, temp, tempBuilder.ToString());
                                    break;
                                }
                            }
                        }
                    }
                }
                if (--count != 0)
                    builder.Append(',');
            }
            //添加Join语句
            builder.Append(JoinBuilder.ToString());
            return builder.ToString();
        }

        private string GetTableAllField(DataTable table, char preTableIndex)
        {
            StringBuilder builder = new StringBuilder();
            foreach (DataColumn defineField in table.Columns)
            {
                FieldType fieldType = FieldType.None;
                if (defineField.ExtendedProperties.ContainsKey(FieldProperty.FieldType))
                    fieldType = (FieldType)defineField.ExtendedProperties[FieldProperty.FieldType];
                if (fieldType == FieldType.None)
                {
                    builder.AppendFormat("{0}.{1},", preTableIndex, defineField.ColumnName);
                    RelativeSourceCollection relColl = null;
                    if (defineField.ExtendedProperties.ContainsKey(FieldProperty.RelativeSource))
                        relColl = (RelativeSourceCollection)defineField.ExtendedProperties[FieldProperty.RelativeSource];
                    if (relColl != null)
                    {
                        foreach (RelativeSource relativeSource in relColl)
                        {
                            foreach (RelField relField in relativeSource.RelFields)
                            {
                                builder.AppendFormat("{0}.{1},", preTableIndex, string.IsNullOrEmpty(relField.AsName) ? relField.Name : relField.AsName);
                            }
                        }
                    }
                }
            }

            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        /// <summary>
        /// 解析查询条件
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns></returns>
        private string ParseWhere(string where)
        {
            JoinBuilder.Length = 0;
            if (string.IsNullOrEmpty(where))
                return where;
            where = where.Trim();
            //查找是否存在LIKE关键字
            int start = 0;
            bool isFind = true;
            List<int> likeList = new List<int>();
            while (isFind)
            {
                int index = where.IndexOf("LIKE", start, StringComparison.CurrentCultureIgnoreCase);
                isFind = index >= 0;
                if (isFind)
                    likeList.Add(index);
                start = index + 4;
            }
            bool hasLike = likeList.Count > 0;
            int length = where.Length;
            //key为where条件里的X.Field名，value为真实情况的RealX.RealField名。
            Dictionary<string, string> store = new Dictionary<string, string>();
            StringBuilder tempBuilder = new StringBuilder();
            bool findExists = false;
            //把Exists的语句提取出来
            List<string> existsList = null;
            StringBuilder existsStr = new StringBuilder();
            StringBuilder existsBuilder = new StringBuilder(3);//ts[ 
            for (int i = 0; i < length; i++)
            {
                char c = where[i];
                if (findExists)
                {
                    existsStr.Append(c);
                    if (c == ']')
                    {
                        findExists = false;
                        existsList.Add(existsStr.ToString());
                        existsStr.Length = 0;
                    }
                    continue; //跳过下面计算逻辑
                }

                existsBuilder.Append(c);
                if (existsBuilder.Length == 3)
                {
                    if (string.Compare(existsBuilder.ToString(), "ts[", true) == 0)
                    {
                        //为Exists
                        findExists = true;
                        if (existsList == null)
                            existsList = new List<string>();
                        existsStr.Append(c);
                        existsBuilder.Length = 0;
                    }
                    else
                        existsBuilder.Remove(0, 1); //移除第一个字符
                }
                if ((compareStr.Contains(c) && !compareStr.Contains(where[i - 1])) || (hasLike && likeList.Contains(i)))
                {
                    char prefix = 'A';
                    string selectName = null;
                    if (tempBuilder.Length > 0)
                        tempBuilder.Length = 0;
                    for (int l = i - 1; l >= 0; l--)
                    {
                        char temp = where[l];
                        if (compareStr.Contains(temp))
                        {
                            selectName = null;
                            break;
                        }
                        if (temp == '.')
                        {
                            selectName = tempBuilder.ToString().Trim();
                            prefix = where[l - 1];
                            break;
                        }
                        tempBuilder.Insert(0, temp);
                    }
                    if (!string.IsNullOrEmpty(selectName))
                    {
                        string selectFullName = string.Format("{0}.{1}", prefix, selectName);
                        if (!store.ContainsKey(selectFullName))
                        {
                            string tablePrefix, fieldName;
                            ParseFieldCore(this.SqlModel, prefix, selectName, out tablePrefix, out fieldName);
                            fieldName = string.Format("{0}.{1}", tablePrefix, fieldName);
                            store.Add(selectFullName, fieldName);
                        }
                    }
                }
            }
            if (existsList != null)
            {
                foreach (string item in existsList)
                {
                    //解析exists语句
                    store.Add(item, ParseExists(item));
                }
            }
            foreach (KeyValuePair<string, string> item in store)
            {
                if (item.Key != item.Value)
                    where = where.Replace(item.Key, item.Value);
            }
            //添加Join语句
            StringBuilder builder = new StringBuilder();
            builder.Append(JoinBuilder.ToString());
            builder.Append(" Where ");
            builder.Append(where);
            return builder.ToString();
        }

        private string ParseExists(string existsStr)
        {
            //Exists[ProgId,0,B.Field=A.Field] DBTableIndex为0时可以省略  B为里面的表，A为外面的表
            //Exists[ProgId,0,1,B.Field=A.Field] 
            StringBuilder builder = new StringBuilder();
            bool isStart = false;
            string progId = string.Empty;
            bool isDBTableIndex = false;
            int tableIndex = 0;
            int dbTableIndex = 0;
            StringBuilder tempBuilder = new StringBuilder();
            foreach (char c in existsStr)
            {
                if (isStart)
                {
                    if (c == ',')
                    {
                        if (string.IsNullOrEmpty(progId))
                        {
                            progId = tempBuilder.ToString();
                            tempBuilder.Length = 0;
                        }
                        else
                        {
                            if (isDBTableIndex)
                            {
                                int.TryParse(tempBuilder.ToString(), out dbTableIndex);
                                tempBuilder.Length = 0;
                            }
                            else
                            {
                                int.TryParse(tempBuilder.ToString(), out tableIndex);
                                tempBuilder.Length = 0;
                                isDBTableIndex = true;
                            }
                        }
                    }
                    else
                    {
                        if (c == ']')
                            break;
                        tempBuilder.Append(c);
                    }
                }
                else
                {
                    if (c == '[')
                        isStart = true;
                }
            }
            string condition = tempBuilder.ToString();
            LibSqlModel dataSource = LibSqlModelCache.Default.GetSqlModel(progId);
            string tableName = dataSource.Tables[tableIndex].TableName;
            if (!TableMapPrefix.ContainsKey(tableName))
            {
                TableMapPrefix.Add(tableName, _CurrMaxPrefix);
                _CurrMaxPrefix = InternalAdd(_CurrMaxPrefix);
            }
            string prefix = TableMapPrefix[tableName];
            condition = condition.Replace("B.", string.Format("{0}.", prefix));
            builder.Append("(Select * ");
            builder.AppendFormat("From {0} {1} ", tableName, prefix);
            builder.AppendFormat("Where {0})", condition);
            return builder.ToString();
        }

        /// <summary>
        /// 解析排序字段
        /// </summary>
        /// <param name="sortCondition">排序条件</param>
        /// <returns></returns>
        private string ParseOrderBy(string sortCondition)
        {
            if (string.IsNullOrEmpty(sortCondition))
                return sortCondition;
            StringBuilder builder = new StringBuilder(" Order by ");
            builder.Append(AdjustExpression(sortCondition));
            return builder.ToString();
        }

        private string AdjustExpression(string expression)
        {
            int length = expression.Length;
            bool isFieldName = false;
            StringBuilder fieldBuilder = new StringBuilder();
            char prefix = 'A';
            Dictionary<string, string> store = new Dictionary<string, string>();
            for (int i = 0; i < length; i++)
            {
                char c = expression[i];
                if (isFieldName)
                {
                    if (c == ' ' || c == ',' || c == ')')
                    {
                        FindRealField(prefix, fieldBuilder.ToString(), store);
                        isFieldName = false;
                        fieldBuilder.Length = 0;
                    }
                    else
                        fieldBuilder.Append(c);
                }
                if (c == '.')
                {
                    prefix = expression[i - 1];
                    isFieldName = true;
                }
            }
            if (fieldBuilder.Length > 0)
            {
                FindRealField(prefix, fieldBuilder.ToString(), store);
            }
            foreach (KeyValuePair<string, string> item in store)
            {
                expression = expression.Replace(item.Key, item.Value);
            }
            return expression;
        }

        private void FindRealField(char prefix, string name, Dictionary<string, string> store)
        {
            string fullName = string.Format("{0}.{1}", prefix, name);
            if (!store.ContainsKey(fullName))
            {
                string tablePrefix, fieldName;
                ParseFieldCore(this.SqlModel, prefix, name, out tablePrefix, out fieldName);
                store.Add(fullName, string.Format("{0}.{1}", tablePrefix, fieldName));
            }
        }
        /// <summary>
        /// 解析分组语句
        /// </summary>
        /// <param name="groupCondition">分组语句</param>
        /// <returns></returns>
        private string ParseGroupBy(string groupCondition)
        {
            if (string.IsNullOrEmpty(groupCondition))
                return groupCondition;
            StringBuilder builder = new StringBuilder(" Group by ");
            builder.Append(AdjustExpression(groupCondition));
            return builder.ToString();
        }

        /// <summary>
        /// 递增表前缀
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string InternalAdd(string str)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = str.Length - 1; i >= 0; i--)
            {
                char last = str[i];
                int num;
                bool success = int.TryParse(last.ToString(), out num);
                if (success)
                {
                    if (num == 9)
                        builder.Insert(0, 'A');
                    else
                    {
                        builder.Insert(0, ++num);
                        break;
                    }
                }
                else
                {
                    if (last == 'Z')
                        builder.Insert(0, 0);
                    else
                    {
                        builder.Insert(0, (char)((int)last + 1));
                        break;
                    }
                }
            }
            return string.Format("{0}{1}", str.Substring(0, str.Length - builder.Length), builder.ToString());
        }

        public char GetTablePrefix(int tableIndex)
        {
            return (char)(tableIndex + (int)'A');
        }

        public int GetTablePrefix(char prefix)
        {
            return ((int)prefix - (int)'A');
        }

    }

}
