using AxCRL.Comm;
using AxCRL.Comm.Bill;
using AxCRL.Comm.Entity;
using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Core;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Core.Permission;
using AxCRL.Data;
using AxCRL.Data.SqlBuilder;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;


namespace AxCRL.Bcf
{
    public class LibBcfBase : ILibBcfBase, IPart
    {
        private LibDataAccess _DataAccess = null;
        private LibManagerMessage _ManagerMessage = null;
        private LibTemplate _Template;
        private DataSet _DataSet;
        private string _ProgId;
        private LibHandle _Handle;
        /// <summary>
        /// 是否跨站点调用     
        /// </summary>
        private bool _IsCrossSiteCall;
        /// <summary>
        /// 是否同步数据调用
        /// </summary>
        private bool _IsSynchroDataCall;

        public LibHandle Handle
        {
            get { return _Handle; }
            set { _Handle = value; }
        }

        public LibManagerMessage ManagerMessage
        {
            get
            {
                if (_ManagerMessage == null)
                    _ManagerMessage = new LibManagerMessage();
                return _ManagerMessage;
            }
        }
        public LibDataAccess DataAccess
        {
            get
            {
                if (_DataAccess == null)
                    _DataAccess = new LibDataAccess();
                return _DataAccess;
            }
            set
            {
                this._DataAccess = value;
            }
        }
        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }

        public DataSet DataSet
        {
            get { return _DataSet; }
            set { _DataSet = value; }
        }
        public LibTemplate Template
        {
            get { return _Template; }
            set { _Template = value; }
        }
        /// <summary>
        /// 是否跨站点调用
        /// </summary>
        public bool IsCrossSiteCall
        {
            get { return _IsCrossSiteCall; }
            set { _IsCrossSiteCall = value; }
        }
        /// <summary>
        /// 是否为同步数据而调用
        /// </summary>
        public bool IsSynchroDataCall
        {
            get { return _IsSynchroDataCall; }
            set { _IsSynchroDataCall = value; }
        }

        public LibBcfBase()
        {
            this.Template = RegisterTemplate();
            this.DataSet = this.Template.DataSet;
            this.ProgId = this.Template.ProgId;
        }

        public LibBcfBase(LibDataAccess dataAccess)
        {
            this._DataAccess = dataAccess;
            this.Template = RegisterTemplate();
            this.DataSet = this.Template.DataSet;
            this.ProgId = this.Template.ProgId;
        }
        // 注册模型
        protected virtual LibTemplate RegisterTemplate()
        {
            return null;
        }
        //权限检查
        protected bool CheckHasPermission(FuncPermissionEnum permission)
        {
            bool ret = (this.Template.FuncPermission.Permission & (int)permission) == (int)permission;
            if (ret)
            {
                if (LibHandleCache.Default.GetSystemHandle() != this.Handle)
                {
                    ret = LibPermissionControl.Default.HasPermission(this.Handle, this.ProgId, permission);
                    if (!ret)
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前人员不具备操作权限。");
                    }
                }
            }
            else
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("由于功能{0}不具备当前操作条件，因此当前操作不能执行。", this.Template.DisplayText));
            }
            return ret;
        }
        //按钮权限
        public bool CheckHasButtonPermission(string id)
        {
            bool ret = LibPermissionControl.Default.HasButtonPermission(this.Handle, this.ProgId, id);
            if (!ret)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前人员不具备操作权限。");
            }
            return ret;
        }
        // 视图模型
        public virtual LibViewTemplate GetViewTemplate(LibEntryParam entryParam = null)
        {
            if (entryParam != null)
            {
                this.DataSet.ExtendedProperties.Add("entryParam", entryParam);
            }
            LibViewTemplate template = this.Template.GetViewTemplate(this.DataSet);
            template.ProgId = this.Template.ProgId;
            template.DisplayText = this.Template.DisplayText;
            template.BillType = this.Template.BillType;
            StringBuilder builder = new StringBuilder();
            if (entryParam != null)
            {
                foreach (var item in entryParam.ParamStore)
                {
                    builder.AppendFormat("{0}", item.Value);
                }
            }
            template.Layout.SchemeName = Path.Combine(this.Handle.UserId, string.Format("{0}{1}.bin", this.ProgId, builder.ToString()));
            return template;
        }

        //字段检查
        public Dictionary<string, object> CheckFieldValue(int tableIndex, string fieldName, string relSource, object[] curPks, Dictionary<string, object> fieldValue)
        {
            Dictionary<string, object> returnValue = new Dictionary<string, object>();
            RelativeSourceCollection relSources = (RelativeSourceCollection)this.DataSet.Tables[tableIndex].Columns[fieldName].ExtendedProperties[FieldProperty.RelativeSource];
            int groupIndex = -1;
            bool isFind = false;
            StringBuilder builder = new StringBuilder();
            Dictionary<string, string> asNameDic = new Dictionary<string, string>();
            foreach (RelativeSource item in relSources)
            {
                if (string.Compare(relSource, item.RelSource, true) == 0)
                {
                    isFind = true;
                    groupIndex = item.GroupIndex;
                }
                if (isFind)
                {
                    if (item.GroupIndex == groupIndex)
                    {
                        StringBuilder selectBuilder = new StringBuilder();
                        char prefix = (char)(item.TableIndex + (int)'A');
                        foreach (RelField relField in item.RelFields)
                        {
                            string name;
                            if (string.IsNullOrEmpty(relField.AsName))
                                name = relField.Name;
                            else
                            {
                                name = relField.AsName;
                                asNameDic.Add(relField.Name, relField.AsName);
                            }
                            returnValue.Add(name, "");
                            selectBuilder.AppendFormat("{0}.{1},", prefix, relField.Name);
                        }
                        foreach (SetValueField relField in item.SetValueFields)
                        {
                            string name;
                            if (string.IsNullOrEmpty(relField.AsName))
                                name = relField.Name;
                            else
                            {
                                name = relField.AsName;
                                asNameDic.Add(relField.Name, relField.AsName);
                            }
                            returnValue.Add(name, "");
                            selectBuilder.AppendFormat("{0}.{1},", prefix, relField.Name);
                        }
                        if (selectBuilder.Length > 0)
                        {
                            selectBuilder.Remove(selectBuilder.Length - 1, 1);
                            SqlBuilder sqlBuilder = new SqlBuilder(item.RelSource);
                            string sql = sqlBuilder.GetQuerySql(item.TableIndex, selectBuilder.ToString(), GetRelWhere(item.RelSource, item.TableIndex, prefix, curPks));
                            builder.AppendLine(sql);
                        }
                    }
                    else
                        break;
                }
            }
            if (builder.Length > 0)
            {
                LibDataAccess dataAccess = new LibDataAccess();
                using (IDataReader reader = dataAccess.ExecuteDataReader(builder.ToString()))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            int count = reader.FieldCount;
                            for (int i = 0; i < count; i++)
                            {
                                string name = reader.GetName(i);
                                if (asNameDic.ContainsKey(name))
                                    name = asNameDic[name];
                                if (returnValue.ContainsKey(name))
                                {
                                    returnValue[name] = reader[i] == null ? "" : reader[i];
                                }
                            }
                        }
                    } while (reader.NextResult());
                }
            }
            CheckFieldReturn(tableIndex, fieldName, curPks, fieldValue, returnValue);
            return returnValue;
        }

        private string GetRelWhere(string relSource, int tableIndex, char prefix, object[] curPks)
        {
            StringBuilder whereBuilder = new StringBuilder();
            LibSqlModel relModel = LibSqlModelCache.Default.GetSqlModel(relSource);
            DataColumn[] cols = relModel.Tables[tableIndex].PrimaryKey;
            for (int i = 0; i < curPks.Length; i++)
            {
                DataColumn pk = cols[i];
                if (i != 0)
                    whereBuilder.Append(" AND ");
                LibDataType dataType = (LibDataType)pk.ExtendedProperties[FieldProperty.DataType];
                if (dataType == LibDataType.Text)
                    whereBuilder.AppendFormat("{0}.{1} = {2}", prefix, pk.ColumnName, LibStringBuilder.GetQuotObject(curPks[i]));
                else
                    whereBuilder.AppendFormat("{0}.{1} = {2}", prefix, pk.ColumnName, LibSysUtils.ToString(curPks[i]) == "" ? 0 : curPks[i]);
            }
            return whereBuilder.ToString();
        }
        //检查字段返回前
        protected virtual void CheckFieldReturn(int tableIndex, string fieldName, object[] curPk, Dictionary<string, object> fieldKeyAndValue, Dictionary<string, object> returnValue)
        {

        }
        /// <summary>
        /// 获取升级时对于具有唯一性约束的字段的处理Sql
        /// 可以返回更新数据列的内容设置唯一性数据。
        /// 此方法会在升级数据表，数据列添加完后但唯一性约束（主键、索引、唯一约束等）添加前执行
        /// 可以由多条语句组成
        /// </summary>      
        /// <param name="dbType">数据库类型</param>
        /// <returns>数据表名称与对应的Sql语句的字典</returns>
        public virtual Dictionary<string, string> GetUniqueDataSqlForUpdate(LibDatabaseType dbType)
        {
            Dictionary<string, string> dicSqls = new Dictionary<string, string>();
            if (this.DataSet.Tables.Count == 0 || this.DataSet.Tables[0].Columns.Contains("INTERNALID") == false ||
                this.DataSet.Tables[0].Columns.Contains("CURRENTSTATE") == false)
                return dicSqls;
            if (dbType == LibDatabaseType.SqlServer)
            {
                // BillType类型从grid改为Master，需要处理INTERNALID字段
                dicSqls.Add(this.DataSet.Tables[0].TableName,
                    string.Format("Update {0} set INTERNALID = NEWID(),CURRENTSTATE = 2 where INTERNALID=''", this.DataSet.Tables[0].TableName));
            }
            else if (dbType == LibDatabaseType.Oracle)
            {
                dicSqls.Add(this.DataSet.Tables[0].TableName,
                    string.Format("Update {0} set INTERNALID = sys_guid(),CURRENTSTATE = 2 where INTERNALID=''", this.DataSet.Tables[0].TableName));
            }
            return dicSqls;
        }

        public IList<FuzzyResult> FuzzySearchField(int tableIndex, string fieldName, string relSource, string relName,
            string query, object[] curPks = null, Dictionary<string, object> selConditionParam = null, string[] currentPks = null)
        {
            IList<FuzzyResult> list = new List<FuzzyResult>();
            RelativeSourceCollection relSources = (RelativeSourceCollection)this.DataSet.Tables[tableIndex].Columns[fieldName].ExtendedProperties[FieldProperty.RelativeSource];
            RelativeSource curRelSource = null;
            foreach (RelativeSource item in relSources)
            {
                if (string.Compare(relSource, item.RelSource, true) == 0)
                {
                    curRelSource = item;
                    break;
                }
            }
            if (curRelSource == null)
                return list;
            SqlBuilder sqlBuilder = new SqlBuilder(relSource);
            StringBuilder builder = new StringBuilder();
            LibBcfBase bcfBase = LibBcfSystem.Default.GetBcfInstance(relSource);
            BillType billType = bcfBase.Template.BillType;
            //if (relSource.Split(new string[] { "axp" }, StringSplitOptions.None).Length == 1 && (billType == BillType.Bill || billType == BillType.Master))
            //{
            //    builder.AppendFormat("And A.CURRENTSTATE=2");
            //}
            if (curPks != null && curPks.Length > 0)
            {
                builder.AppendFormat(" And {0}", GetRelWhere(relSource, curRelSource.TableIndex, 'A', curPks));
            }
            string selCondition = string.Empty;
            if (curRelSource.SelConditions.Count > 0)
            {
                foreach (SelCondition item in curRelSource.SelConditions)
                {
                    builder.AppendFormat(" And {0}", item.Condition);
                }
                selCondition = builder.ToString();
                selCondition = selCondition.Replace("CURRENT_PERSON", LibStringBuilder.GetQuotObject(this.Handle.PersonId));
                if (selConditionParam != null && selConditionParam.Count > 0)
                {
                    LibSqlModel model = LibSqlModelCache.Default.GetSqlModel(this.ProgId);
                    if (model != null)
                    {
                        foreach (KeyValuePair<string, object> item in selConditionParam)
                        {
                            string[] temp = item.Key.Split('.');
                            int index = (int)temp[0][0] - (int)'A';
                            DataColumn col = model.Tables[index].Columns[temp[1]];
                            LibDataType dataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                            if (dataType == LibDataType.Text)
                                selCondition = selCondition.Replace(string.Format("@{0}", item.Key), LibStringBuilder.GetQuotObject(item.Value));
                            else
                                selCondition = selCondition.Replace(string.Format("@{0}", item.Key), item.Value.ToString());
                        }
                    }
                }
            }
            else
            {
                selCondition = builder.ToString();
            }
            string powerStr = LibPermissionControl.Default.GetShowCondition(this.Handle, relSource, this.Handle.PersonId);
            if (!string.IsNullOrEmpty(powerStr))
            {
                selCondition = string.Format("{0} and {1}", selCondition, powerStr);
            }
            if (curRelSource.ContainsSub == false && string.IsNullOrEmpty(curRelSource.ParentColumnName) == false
                && currentPks != null && currentPks.Length > 0 && string.IsNullOrEmpty(currentPks[0]) == false)
            {
                //对于父子结构数据，如果不包含子数据且指定了父列外键列的名称，则添加额外的查询条件 Zhangkj 20170316
                DataColumn keyColumn = this.DataSet.Tables[tableIndex].PrimaryKey[0];
                string keyColumnName = this.DataSet.Tables[tableIndex].PrimaryKey[0].ColumnName;//目前仅支持单主键
                string dataId = currentPks[0];
                LibDataType dataType = keyColumn.ExtendedProperties.ContainsKey(FieldProperty.DataType) ? (LibDataType)keyColumn.ExtendedProperties[FieldProperty.DataType] : LibDataTypeConverter.ConvertToLibType(keyColumn.DataType);
                List<object> subIds = this.GetSubDataIds(dataType, dataId, this.DataSet.Tables[tableIndex].TableName, keyColumnName, curRelSource.ParentColumnName, true);
                if (subIds != null && subIds.Count > 0)
                {
                    bool needQuot = dataType == LibDataType.Text || dataType == LibDataType.NText;
                    List<object> quotSubIds = new List<object>();
                    foreach (object obj in subIds)
                    {
                        quotSubIds.Add((needQuot) ? LibStringBuilder.GetQuotObject(obj) : obj);
                    }
                    selCondition = string.Format("{0} and A.{1} not in ({2})", selCondition, keyColumnName, string.Join(",", quotSubIds));
                }
            }
            string sql = sqlBuilder.GetFuzzySql(curRelSource.TableIndex, relSources, query, selCondition, curRelSource.ParentColumnName);
            LibDataAccess dataAccess = new LibDataAccess();
            int count = 0;
            int filterCount = curRelSource.SearchFilterCount;
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    if (reader.FieldCount == 1)
                        list.Add(new FuzzyResult(LibSysUtils.ToString(reader[0]), string.Empty));
                    else if (reader.FieldCount == 2)
                        list.Add(new FuzzyResult(LibSysUtils.ToString(reader[0]), LibSysUtils.ToString(reader[1])));
                    else if (reader.FieldCount == 3)
                    {
                        FuzzyResult fuzzyResult = new FuzzyResult(LibSysUtils.ToString(reader[0]), LibSysUtils.ToString(reader[1]));
                        fuzzyResult.ContainsKeyField = LibSysUtils.ToString(reader[2]);//将除Id Name列以外的包含查询关键字的列的内容
                        list.Add(fuzzyResult);
                    }
                    else if (reader.FieldCount == 4)
                    {
                        FuzzyResult fuzzyResult = new FuzzyResult(LibSysUtils.ToString(reader[0]), LibSysUtils.ToString(reader[1]));
                        fuzzyResult.ContainsKeyField = LibSysUtils.ToString(reader[2]);//将除Id Name列以外的包含查询关键字的列的内容
                        fuzzyResult.ParentId = LibSysUtils.ToString(reader[3]);//树形结构的父数据Id
                        if (curRelSource.ExpandAll)
                        {
                            fuzzyResult.TreeNodeExpanded = true;
                        }
                        list.Add(fuzzyResult);
                    }
                    count++;
                    if (count == filterCount)
                        break;
                }
            }
            if (list.Count > 1)
            {

                LibControlType controlType = (LibControlType)this.DataSet.Tables[tableIndex].Columns[fieldName].ExtendedProperties[FieldProperty.ControlType];
                if (controlType == LibControlType.IdNameTree && string.IsNullOrEmpty(curRelSource.ParentColumnName) == false)
                {
                    //处理树形结构数据
                    List<FuzzyResult> newList = list.ToList();//先全部放入

                    List<FuzzyResult> tempList = null;
                    using (MemoryStream stream = new MemoryStream())
                    {
                        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        formatter.Serialize(stream, list.ToList());
                        stream.Position = 0;
                        tempList = formatter.Deserialize(stream) as List<FuzzyResult>;//深度复制一份
                    }

                    //查找是其他节点的子节点的进行处理
                    int index = 0;
                    while (index < list.Count)
                    {
                        FuzzyResult child = list[index];
                        FuzzyResult parent = (from re in list
                                              where re != null && re.Id.Equals(child.ParentId)
                                              select re).FirstOrDefault();
                        if (parent != default(FuzzyResult))
                        {
                            newList.Remove(child);
                            parent.Children.Add(child);
                        }
                        index++;
                    }
                    newList[0].TotalList = tempList;
                    list = newList;
                }
            }

            return list;
        }
        /// <summary>
        /// 获取指定数据的子数据主键值列表。
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="dataId">数据主键标识</param>
        /// <param name="tabName">数据表名称</param>
        /// <param name="keyIdColumnName">主键列名</param>
        /// <param name="parenetIdColumnName">关联到父数据的数据列名称</param>
        /// <param name="isContainsSelf">是否包含自身，默认为false</param>
        /// <returns></returns>
        public List<object> GetSubDataIds(LibDataType dataType, object dataId, string tabName, string keyIdColumnName, string parenetIdColumnName, bool isContainsSelf = false)
        {
            List<object> list = new List<object>();
            bool needQuot = dataType == LibDataType.Text || dataType == LibDataType.NText;
            if (string.IsNullOrEmpty(tabName.Trim()) || string.IsNullOrEmpty(keyIdColumnName.Trim()) || string.IsNullOrEmpty(parenetIdColumnName.Trim()))
                return list;
            try
            {
                //从本级数据开始到最底级目录。第一行为本级数据（dataId标识的数据），下面的是按层级的子级数据
                string sqlFindSub = "";
                string tempTableName = string.Format("{0}_{1}", "temp", DateTime.Now.Ticks);
                if (this.DataAccess.DatabaseType == LibDatabaseType.SqlServer)
                    sqlFindSub = string.Format(" with {0} as  " +
                          "   ( " +
                          "   select a.{1},a.{2} from {3} a where {1} = {4} " +
                          "   union all " +
                          "   select k.{1},k.{2} from {3} k inner " +
                          "                               join {0} t on t.{1} = k.{2} " +
                          "   ) select * from  " + tempTableName,
                          tempTableName, keyIdColumnName.Trim(), parenetIdColumnName.Trim(), tabName.Trim(), (needQuot) ? LibStringBuilder.GetQuotObject(dataId) : dataId);
                else
                {
                    //Oracle的递归查询待测试
                    sqlFindSub = string.Format("select {0},{1} " +
                          " from {2} " +
                          " START WITH {0} = {3} " +
                          " CONNECT BY PRIOR {0} =  {1} ", keyIdColumnName.Trim(), parenetIdColumnName.Trim(), tabName.Trim(), (needQuot) ? LibStringBuilder.GetQuotObject(dataId) : dataId);
                }
                DataTable subDirDt = null;
                DataSet ds2 = this.DataAccess.ExecuteDataSet(sqlFindSub);
                if (ds2 != null && ds2.Tables.Count > 0)
                    subDirDt = ds2.Tables[0];
                if (subDirDt != null && subDirDt.Rows.Count > 0)
                {
                    DataRow row = null;
                    //正序，从当前数据向下级数据方向
                    object id = null;
                    for (int i = (isContainsSelf ? 0 : 1); i < subDirDt.Rows.Count; i++)
                    {
                        row = subDirDt.Rows[i];
                        if (needQuot)
                            id = LibSysUtils.ToString(row[keyIdColumnName.Trim()]);
                        else
                        {
                            if (row[keyIdColumnName.Trim()] != DBNull.Value)
                                id = row[keyIdColumnName.Trim()];
                        }
                        if (list.Contains(id) == false)
                            list.Add(id);
                    }
                }
            }
            catch (Exception exp)
            {
                //to do log
                throw exp;
            }
            return list;
        }
        public List<PrintTemplateIds> GetPrintTemplateIds(string billNo)
        {
            int num = 0;
            string progId = this.ProgId;
            string printTplId = string.Empty;
            StringBuilder builder = new StringBuilder();
            SqlBuilder sqlBuilder = new SqlBuilder("axp.PrintTpl");
            SqlBuilder sqlBillBuilder = new SqlBuilder(this.ProgId);
            DataTable dt = new DataTable();
            List<PrintTemplateIds> printTemplateIds = new List<PrintTemplateIds>();
            builder.Append(sqlBuilder.GetQuerySql(0, "A.PRINTTPLID", string.Format("A.PROGID = {0}", LibStringBuilder.GetQuotString(progId))));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(builder.ToString()))
            {
                while (reader.Read())
                {
                    printTplId = LibSysUtils.ToString(reader["PRINTTPLID"]);
                }
            }
            if (printTplId == string.Empty)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "打印模板尚未设置！");
            }
            else
            {
                builder.Clear();
                builder.Append(sqlBillBuilder.GetQuerySql(0, "A.*", string.Format("A.BILLNO = {0}", LibStringBuilder.GetQuotString(billNo))));
                this.DataAccess.ExecuteDataTable(builder.ToString(), dt);//将单据数据放入dt中
                builder.Clear();
                builder.Append(sqlBuilder.GetQuerySql(1, "B.ROW_ID,B.USECONDITION", string.Format(" B.PRINTTPLID = {0} ", LibStringBuilder.GetQuotString(printTplId))));
                string sql = builder.ToString();
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(builder.ToString(), true))
                {
                    while (reader.Read())
                    {
                        List<DataRow> dataRowList = new List<DataRow> { dt.Rows[0] };
                        if (LibSysUtils.ToString(reader["USECONDITION"]) != string.Empty)
                        {
                            if (LibParseHelper.Parse(LibSysUtils.ToString(reader["USECONDITION"]), dataRowList))//判断是否匹配到条件符合的打印模板明细
                            {
                                num = LibSysUtils.ToInt32(reader["ROW_ID"]);
                            }
                        }
                    }
                }
                if (num == 0)
                {
                    builder.Clear();
                    builder.Append(sqlBuilder.GetQuerySql(1, "B.ROW_ID", string.Format(" B.PRINTTPLID = {0} AND B.USECONDITION = '' ", LibStringBuilder.GetQuotString(printTplId))));
                    using (IDataReader reader = this.DataAccess.ExecuteDataReader(builder.ToString()))
                    {
                        while (reader.Read())
                        {
                            if (LibSysUtils.ToInt32(reader["ROW_ID"]) == 0)
                            {
                                this.ManagerMessage.AddMessage(LibMessageKind.Error, "打印模板明细中没有设置默认模板！");
                            }
                            else
                            {
                                num = LibSysUtils.ToInt32(reader["ROW_ID"]);
                            }
                        }
                    }
                }
                if (num > 0)
                {
                    builder.Clear();
                    builder.Append(sqlBuilder.GetQuerySql(2, "C.PRINTTPLID,C.PARENTROWID,C.ROW_ID", string.Format(" C.PRINTTPLID = {0} AND C.PARENTROWID = {1} ", LibStringBuilder.GetQuotString(printTplId), num), " C.ROW_ID ASC "));
                    using (IDataReader reader = this.DataAccess.ExecuteDataReader(builder.ToString(), true))
                    {
                        while (reader.Read())
                        {
                            PrintTemplateIds printTemplateId = new PrintTemplateIds();
                            printTemplateId.PrintTplNo = LibSysUtils.ToString(reader["PRINTTPLID"]);
                            printTemplateId.PrintTplRowId = LibSysUtils.ToInt32(reader["PARENTROWID"]);
                            printTemplateId.PrintTplSubRowId = LibSysUtils.ToInt32(reader["ROW_ID"]);
                            printTemplateIds.Add(printTemplateId);
                        }
                    }
                }
            }
            return printTemplateIds;
        }

        public Dictionary<string, object> GetPrintTemplateJs(string billNo, string printTplNo, int printTplRowId, int printTplSubRowId)
        {
            List<LabelTemplateRule> LabelTemplateRuleList = new List<LabelTemplateRule>();
            StringBuilder builder = new StringBuilder(), builderVal = new StringBuilder();
            SqlBuilder sqlBuilder = new SqlBuilder("axp.PrintTpl");
            string printTplJs = string.Empty, sql = string.Empty;
            Dictionary<string, object> dic = new Dictionary<string, object>(), billValueDic = new Dictionary<string, object>();
            builder.Append(sqlBuilder.GetQuerySql(2, "C.TPLJS", string.Format(" C.PRINTTPLID = {0} AND C.PARENTROWID = {1} AND C.ROW_ID = {2} ", LibStringBuilder.GetQuotString(printTplNo), printTplRowId, printTplSubRowId)));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(builder.ToString()))
            {
                while (reader.Read())
                {
                    printTplJs = LibSysUtils.ToString(reader["TPLJS"]);
                }
            }
            builder.Clear();
            builder.Append(sqlBuilder.GetQuerySql(3, "D.FIELDNAME,D.TABLEINDEX,D.TPLPARAM", string.Format(" D.PRINTTPLID = {0} AND D.GRANDFATHERROWID = {1} AND D.PARENTROWID= {2} ", LibStringBuilder.GetQuotString(printTplNo), printTplRowId, printTplSubRowId)));
            Dictionary<string, PropertyCollection> fieldCollection = new Dictionary<string, PropertyCollection>();
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(builder.ToString()))
            {
                while (reader.Read())
                {
                    LabelTemplateRule labelTemplateRule = new LabelTemplateRule();
                    labelTemplateRule.FieldName = LibSysUtils.ToString(reader["FIELDNAME"]);
                    labelTemplateRule.TableIndex = LibSysUtils.ToInt32(reader["TABLEINDEX"]);
                    labelTemplateRule.Tplparam = LibSysUtils.ToString(reader["TPLPARAM"]);
                    LabelTemplateRuleList.Add(labelTemplateRule);
                    builderVal.AppendFormat("{0}.{1},", (char)(labelTemplateRule.TableIndex + (int)'A'), labelTemplateRule.FieldName);
                    if (!fieldCollection.ContainsKey(labelTemplateRule.FieldName))
                    {
                        fieldCollection.Add(labelTemplateRule.FieldName, this.DataSet.Tables[labelTemplateRule.TableIndex].Columns[labelTemplateRule.FieldName].ExtendedProperties);
                    }
                }
            }
            if (builderVal.Length > 0)
            {
                builderVal.Remove(builderVal.Length - 1, 1);
                sql = new SqlBuilder(this.ProgId).GetQuerySql(0, builderVal.ToString(), string.Format(" A.BILLNO={0} ", LibStringBuilder.GetQuotString(billNo)));
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql, true))
                {
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string name = reader.GetName(i);
                            PropertyCollection propertyList = fieldCollection[name];
                            LibControlType controlType = (LibControlType)propertyList[FieldProperty.ControlType];
                            switch (controlType)
                            {
                                case LibControlType.TextOption:
                                    billValueDic.Add(name, ((string[])propertyList[FieldProperty.Option])[LibSysUtils.ToInt32(reader[i])]);
                                    break;
                                case LibControlType.YesNo:
                                    if (LibSysUtils.ToBoolean(reader[i]))
                                        billValueDic.Add(name, "是");
                                    else
                                        billValueDic.Add(name, "否");
                                    break;
                                case LibControlType.Date:
                                    int date = LibSysUtils.ToInt32(reader[i]);
                                    if (date == 0)
                                        billValueDic.Add(name, string.Empty);
                                    else
                                        billValueDic.Add(name, LibDateUtils.LibDateToDateTime(date).ToLongDateString());
                                    break;
                                case LibControlType.DateTime:
                                    long dateTime = LibSysUtils.ToInt64(reader[i]);
                                    if (dateTime == 0)
                                        billValueDic.Add(name, string.Empty);
                                    else
                                        billValueDic.Add(name, LibDateUtils.LibDateToDateTime(dateTime).ToLocalTime());
                                    break;
                                case LibControlType.Rate:
                                    double rate = LibSysUtils.ToDouble(reader[i]);
                                    billValueDic.Add(name, string.Format("{0}%", rate * 100));
                                    break;
                                case LibControlType.KeyValueOption:
                                    billValueDic.Add(name, ((LibTextOptionCollection)propertyList[FieldProperty.KeyValueOption])[LibSysUtils.ToInt32(reader[i])].Value);
                                    break;
                                default:
                                    billValueDic.Add(name, reader[i]);
                                    break;
                            }
                        }
                    }
                }
            }
            dic.Add("TemplateJs", printTplJs);
            dic.Add("LabelTemplateRuleList", LabelTemplateRuleList);
            dic.Add("BillValueDic", billValueDic);
            return dic;
        }

        #region 打印

        public string ExportDataFile()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(this.DataSet);
            string telFilePath = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "PrintTel", string.Format("{0}.frx", ProgId.Replace(".", string.Empty)));
            using (StreamWriter sw = new StreamWriter(telFilePath, false))
            {
                sw.Write(json);
            }
            return telFilePath;
        }
        #endregion


        private List<string> FindFilterFieldsByAdmin()
        {
            List<string> list = new List<string>();
            string sql = string.Format("select SEARCHFIELD from AXPRPTSEARCHFIELDDETAIL where RPTSEARCHID in (select top 1 RPTSEARCHID from AXPRPTSEARCHFIELD A where A.PROGID = {0} and A.USERID = 'admin' and A.ISON = 1 ORDER BY CREATETIME DESC)", LibStringBuilder.GetQuotString(this.ProgId));

            using (IDataReader dr = DataAccess.ExecuteDataReader(sql))
            {
                while (dr.Read())
                {
                    list.Add(dr[0].ToString());
                }
            }
            return list;
        }
        private List<string> CanFilterFields(string userId = "")
        {
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(userId))
            {
                return FindFilterFieldsByAdmin();
            }
            else
            {
                string sql = string.Format("select SEARCHFIELD from AXPRPTSEARCHFIELDDETAIL where RPTSEARCHID in (select top 1 RPTSEARCHID from AXPRPTSEARCHFIELD A where A.PROGID = {0} and A.USERID = {1} and A.ISON = 1 ORDER BY CREATETIME DESC)", LibStringBuilder.GetQuotString(this.ProgId), LibStringBuilder.GetQuotString(userId));
                using (IDataReader dr = DataAccess.ExecuteDataReader(sql))
                {
                    while (dr.Read())
                    {
                        list.Add(dr[0].ToString());
                    }
                }
            }
            if (list.Count <= 0)
            {
                list = FindFilterFieldsByAdmin();
            }
            return list;
        }

        public string GetFilterLayoutJs(string userId = "")
        {
            List<string> fileds = this.CanFilterFields(userId);
            if (fileds == null || fileds.Count == 0)
            {
                return string.Empty;
            }
            List<FilterField> filterFiledList = new List<FilterField>();
            foreach (var item in fileds)
            {
                DataTable dt = this.DataSet.Tables[0];
                if (dt.Columns[item] == null)
                {
                    throw new Exception("字段不存在");
                }
                DefineField df = DataSourceHelper.ConvertToDefineField(dt.Columns[item]);
                filterFiledList.Add(new FilterField(df.ControlType, df.Name, df.DisplayName));
            }
            if (this.Template.BillType == BillType.Rpt)
            {
                return JsBuilder.BuildFilterFieldJs(filterFiledList);
            }

            else
            {
                // 主数据、单据、Grid
                return JsBuilder.BuildFilterFieldJsForBillListing(filterFiledList);
            }
        }

        public List<FilterField> GetFilterLayoutJs1(string userId = "")
        {
            List<string> fileds = this.CanFilterFields(userId);
            if (fileds == null || fileds.Count == 0)
            {
                return null;
            }
            List<FilterField> filterFiledList = new List<FilterField>();
            foreach (var item in fileds)
            {
                DataTable dt = this.DataSet.Tables[0];
                if (dt.Columns[item] == null)
                {
                    throw new Exception("字段不存在");
                }
                DefineField df = DataSourceHelper.ConvertToDefineField(dt.Columns[item]);
                filterFiledList.Add(new FilterField(df.ControlType, df.Name, df.DisplayName));
            }
            if (this.Template.BillType == BillType.Rpt)
            {
                //return JsBuilder.BuildFilterFieldJs(filterFiledList);
            }

            else
            {
                // 主数据、单据、Grid
                //return JsBuilder.BuildFilterFieldJsForBillListing(filterFiledList);
            }
            return filterFiledList;
        }
    }

    [DataContract]
    [Serializable]
    public class FuzzyResult
    {
        private object _Id;
        [DataMember]
        public object Id
        {
            get { return _Id; }
            set { _Id = value; }
        }
        private object _Name;
        [DataMember]
        public object Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private object _ContainsKeyField = string.Empty;
        /// <summary>
        /// 包含待查询内容的指定列的信息。
        /// 列不固定
        /// </summary>
        [DataMember]
        public object ContainsKeyField
        {
            get { return _ContainsKeyField; }
            set { _ContainsKeyField = value; }
        }

        private bool _TreeNodeExpanded = false;
        /// <summary>
        /// 作为树形结构的节点是否展开。默认为false
        /// </summary>
        [DataMember(Name = "expanded")]
        public bool TreeNodeExpanded
        {
            get { return _TreeNodeExpanded; }
            set { _TreeNodeExpanded = value; }
        }

        private string _parentId = string.Empty;
        /// <summary>
        /// 当返回的数据类型为树形数据时，表示父数据的Id
        /// </summary>
        public string ParentId
        {
            get { return _parentId; }
            set { _parentId = value; }
        }

        private List<FuzzyResult> _children = new List<FuzzyResult>();
        /// <summary>
        /// 实现树形结构时使用
        /// </summary>
        [DataMember(Name = "children")]
        public List<FuzzyResult> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        private List<FuzzyResult> _totalList = new List<FuzzyResult>();
        [DataMember]
        public List<FuzzyResult> TotalList
        {
            get { return _totalList; }
            set { _totalList = value; }
        }

        public FuzzyResult()
        {

        }
        public FuzzyResult(object id, object name)
        {
            this._Id = id;
            this._Name = name;
        }
    }
    public class LabelTemplateRule
    {
        // Properties
        public string FieldName { get; set; }
        public int TableIndex { get; set; }
        public string Tplparam { get; set; }
    }
    public class PrintTemplateIds
    {
        private string _printTplNo = string.Empty;

        public string PrintTplNo
        {
            get { return _printTplNo; }
            set { _printTplNo = value; }
        }
        private int _printTplRowId = 0;

        public int PrintTplRowId
        {
            get { return _printTplRowId; }
            set { _printTplRowId = value; }
        }
        private int _printTplSubRowId = 0;

        public int PrintTplSubRowId
        {
            get { return _printTplSubRowId; }
            set { _printTplSubRowId = value; }
        }
    }




}
