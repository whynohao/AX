using AxCRL.Bcf;
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
using AxCRL.Services.Entity;
using AxCRL.Services.ServiceMethods;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class BillService : IBillService
    {
        public string ExecuteBcfMethod(ExecuteBcfMethodParam param)
        {
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();
            string crossLoginCallHandle = string.Empty;
            bool isCrossCall = false;
            try
            {
                if (string.IsNullOrEmpty(param.ProgId))
                {
                    result.Messages.Add(new LibMessage() { MessageKind = LibMessageKind.Error, Message = string.Format("功能标识为空。") });
                    return JsonConvert.SerializeObject(result);
                    //throw new ArgumentNullException("ProgId", "ProgId is empty.");
                }
                LibHandle libHandle = null;
                if (string.IsNullOrEmpty(param.Token) == false && string.IsNullOrEmpty(param.UserId) == false)
                {
                    //远程Bcf方法调用，先检查用户账户是否存在
                    if (CrossSiteHelper.CheckUserExist(param.UserId) == false)
                    {
                        result.Messages.Add(new LibMessage() { MessageKind = LibMessageKind.Error, Message = string.Format("站点不存在操作账户:{0}。", param.UserId) });
                        return JsonConvert.SerializeObject(result);
                    }
                    //验证令牌信息
                    if (CrossSiteHelper.CheckSSOLoginState(new SSOInfo() { UserId = param.UserId, Token = param.Token }) == false)
                    {
                        result.Messages.Add(new LibMessage() { MessageKind = LibMessageKind.Error, Message = string.Format("跨站点访问令牌信息校验错误。") });
                        return JsonConvert.SerializeObject(result);
                    }

                    //检查本地是否已经登录
                    libHandle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.Unknown, param.UserId, false);//是否有任何形式的登录
                    if (libHandle == null)
                    {
                        //如果没登录则构造登录信息LibHandle(crossCallLogin,不影响新登录，主要为了同样的创建人、修改人等信息)。使用该crossCallLogin LibHandle作为当前Handle
                        libHandle = LibHandleCache.Default.GetCrossCallHandle(param.UserId);
                        if (libHandle != null)
                        {
                            crossLoginCallHandle = libHandle.Handle;
                        }
                    }
                    if (libHandle != null)
                    {
                        isCrossCall = true;
                    }
                    param.CurrentCallLevel++;//累加调用层级
                    if (param.CurrentCallLevel < param.MaxCallLevel)
                    {
                        // 向子站点发送调用请求
                        // to do
                    }
                }
                else
                {
                    libHandle = LibHandleCache.Default.GetCurrentHandle(param.Handle) as LibHandle;
                }
                if (libHandle == null)
                {
                    //throw new Exception("该账户在未登录。");
                    result.Messages.Add(new LibMessage() { MessageKind = LibMessageKind.Error, Message = string.Format("该账户在未登录。") });
                    return JsonConvert.SerializeObject(result);
                }
                else
                {
                    if (ProgIdHost.Instance.ProgIdRef.ContainsKey(param.ProgId))
                    {
                        BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[param.ProgId];
                        string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                        Assembly assembly = Assembly.LoadFrom(path);
                        Type t = assembly.GetType(info.ClassName);
                        LibBcfBase destObj = (LibBcfBase)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                        object[] destParam = null;
                        bool isContinue = true;
                        try
                        {
                            MethodInfo methodInfo = t.GetMethod(param.MethodName);
                            if (methodInfo == null)
                            {
                                isContinue = false;
                                result.Messages.Add(new LibMessage() { MessageKind = LibMessageKind.SysException, Message = string.Format("调用了未实现的处理过程,请联系管理员。") });
                            }
                            else
                                destParam = RestoreParamFormat(t, param.MethodName, param.MethodParam);
                        }
                        catch (Exception exp)
                        {
                            isContinue = false;
                            throw exp;
                            //result.Messages.Add(new LibMessage() { MessageKind = LibMessageKind.SysException, Message = string.Format("查找待调用的处理过程和参数时出现异常,请联系管理员。") });
                        }
                        if (isContinue)
                        {
                            destObj.Handle = libHandle;
                            if (isCrossCall)
                            {
                                destObj.IsCrossSiteCall = true;
                                destObj.IsSynchroDataCall = param.IsSynchroDataCall;
                            }
                            result.Result = t.InvokeMember(param.MethodName, BindingFlags.InvokeMethod, null, destObj, destParam);
                            result.Messages = destObj.ManagerMessage.MessageList;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                result.Messages.Add(new LibMessage() { MessageKind = LibMessageKind.SysException, Message = string.Format("异常信息:{0}{1}异常堆栈:{2}", message, Environment.NewLine, ex.StackTrace) });
            }
            finally
            {
                if (string.IsNullOrEmpty(crossLoginCallHandle) == false)
                {
                    //移除创建的跨站点访问临时登录
                    try
                    {
                        LibHandleCache.Default.RemoveHandle(crossLoginCallHandle);
                    }
                    catch { }
                }

            }
            return JsonConvert.SerializeObject(result);
        }

        public string ExecuteBcfMethodRpt(ExecuteBcfMethodParam param)
        {
            if (string.IsNullOrEmpty(param.ProgId))
            {
                throw new ArgumentNullException("ProgId", "ProgId is empty.");
            }
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();
            LibHandle libHandle = LibHandleCache.Default.GetSystemHandle();
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            else
            {
                if (ProgIdHost.Instance.ProgIdRef.ContainsKey(param.ProgId))
                {
                    BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[param.ProgId];
                    string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                    Assembly assembly = Assembly.LoadFrom(path);
                    Type t = assembly.GetType(info.ClassName);
                    LibBcfBase destObj = (LibBcfBase)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                    object[] destParam = RestoreParamFormat(t, param.MethodName, param.MethodParam);
                    destObj.Handle = libHandle;
                    result.Result = t.InvokeMember(param.MethodName, BindingFlags.InvokeMethod, null, destObj, destParam);
                    result.Messages = destObj.ManagerMessage.MessageList;
                }
            }
            return JsonConvert.SerializeObject(result);
        }

        private object[] RestoreParamFormat(Type destType, string method, string[] param)
        {
            object[] destParam = null;
            ParameterInfo[] paramInfo = destType.GetMethod(method).GetParameters();
            int length = paramInfo.Length;
            if (length > 0)
            {
                destParam = new object[length];
                for (int i = 0; i < param.Length; i++)
                {
                    destParam[i] = JsonConvert.DeserializeObject(param[i], paramInfo[i].ParameterType);
                }
            }
            return destParam;
        }


        private string BuildQuery(string progId, BillType billType, bool usingAudit, LibQueryCondition condition, BillListingTimeFilter timeFilter, int filter)
        {
            StringBuilder builder = new StringBuilder();

            #region 处理清单页具有树形分类的情况
            if (condition != null)
            {
                //处理清单页具有树形分类的情况
                LibBcfBase bcfObj = LibBcfSystem.Default.GetBcfInstance(progId);
                TreeListingConfig treeListingConfig = null;
                if (bcfObj == null || bcfObj.Template == null || (bcfObj.Template.BillType != BillType.Bill && bcfObj.Template.BillType != BillType.Master) ||
                    bcfObj.Template.FuncPermission == null || bcfObj.Template.FuncPermission.TreeListing == null)
                {
                    condition.ContainsSub = false;//如果没有启用清单页树形分类则强制将此属性设置为False
                }
                else
                {
                    treeListingConfig = bcfObj.Template.FuncPermission.TreeListing;
                }
                if (treeListingConfig != null && condition.ContainsSub && condition.QueryFields != null && condition.QueryFields.Count > 0)
                {
                    condition.QueryFields.First();
                    //如果有且查询该树形分类列条件中的条件为相等，
                    AxCRL.Core.Comm.LibQueryField field = condition.QueryFields.First(item =>
                    {
                        if (item.Name == treeListingConfig.ColumnName && item.QueryChar == LibQueryChar.Equal && item.Value != null && item.Value.Count > 0)
                            return true;
                        else
                            return false;
                    });
                    if (field != null)
                    {
                        //包含子级的键值，同时将LibQueryChar修改为Include
                        DataColumn column = bcfObj.DataSet.Tables[0].Columns[treeListingConfig.ColumnName];
                        LibDataType dataType = column.ExtendedProperties.ContainsKey(FieldProperty.DataType) ? (LibDataType)column.ExtendedProperties[FieldProperty.DataType] : LibDataTypeConverter.ConvertToLibType(column.DataType);

                        RelativeSourceCollection relSources = (RelativeSourceCollection)bcfObj.DataSet.Tables[0].Columns[treeListingConfig.ColumnName].ExtendedProperties[FieldProperty.RelativeSource];
                        if (relSources != null && relSources.Count > 0 && relSources[0] != null && string.IsNullOrEmpty(relSources[0].RelSource) == false)
                        {
                            string relProgId = relSources[0].RelSource;//只使用第一个
                            LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel(relProgId);
                            if (sqlModel != null && sqlModel.Tables != null && sqlModel.Tables.Count > 0 && sqlModel.Tables[0] != null)
                            {
                                List<object> subIds = bcfObj.GetSubDataIds(dataType, field.Value[0], sqlModel.Tables[0].TableName, treeListingConfig.RelativeIdColumn, treeListingConfig.RelativeParentColumn);
                                if (subIds != null && subIds.Count > 0)
                                {
                                    subIds.Add(field.Value[0]);
                                    field.QueryChar = LibQueryChar.Include;
                                    field.Value[0] = string.Join(",", subIds);
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            if (condition != null && condition.QueryFields.Count != 0)
            {
                builder.Append(LibQueryConditionParser.GetQueryData(progId, condition));
            }
            if (timeFilter != BillListingTimeFilter.None)
            {
                long dateTime = 0;
                switch (timeFilter)
                {
                    case BillListingTimeFilter.LatestWeek:
                        dateTime = LibDateUtils.AddDayToLibDateTime(DateTime.Now, -7);
                        break;
                    case BillListingTimeFilter.LatestMonth:
                        dateTime = LibDateUtils.AddDayToLibDateTime(DateTime.Now, -30);
                        break;
                    case BillListingTimeFilter.LatestQuarter:
                        dateTime = LibDateUtils.AddDayToLibDateTime(DateTime.Now, -90);
                        break;
                }
                if (builder.Length > 0)
                    builder.AppendFormat(" And A.CREATETIME >= {0}", dateTime);
                else
                    builder.AppendFormat("A.CREATETIME >= {0}", dateTime);
            }
            if (filter != 0)
            {
                StringBuilder tempBuilder = new StringBuilder();
                int state = 0;
                if ((filter & (int)BillListingFilter.Draft) == (int)BillListingFilter.Draft)
                {
                    tempBuilder.Append("A.CURRENTSTATE=0 Or ");
                    state++;
                }
                if ((filter & (int)BillListingFilter.UnRelease) == (int)BillListingFilter.UnRelease)
                {
                    tempBuilder.Append("A.CURRENTSTATE=1 Or ");
                    state++;
                }
                if ((filter & (int)BillListingFilter.Release) == (int)BillListingFilter.Release)
                {
                    tempBuilder.Append("A.CURRENTSTATE=2 Or ");
                    state++;
                }
                if ((filter & (int)BillListingFilter.Invalid) == (int)BillListingFilter.Invalid)
                {
                    tempBuilder.Append("A.CURRENTSTATE=3 Or ");
                    state++;
                }
                if ((filter & (int)BillListingFilter.EndCase) == (int)BillListingFilter.EndCase)
                {
                    tempBuilder.Append("A.CURRENTSTATE=4 Or ");
                    state++;
                }
                if (state != 0 && (state != 5 || (state != 3 && billType == BillType.Master)))
                {
                    tempBuilder.Remove(tempBuilder.Length - 3, 3);
                    if (builder.Length > 0)
                        builder.AppendFormat(" And ({0})", tempBuilder.ToString());
                    else
                        builder.AppendFormat("({0})", tempBuilder.ToString());
                }
                if (usingAudit)
                {
                    tempBuilder.Length = 0;
                    state = 0;
                    if ((filter & (int)BillListingFilter.Audit) == (int)BillListingFilter.Audit)
                    {
                        tempBuilder.Append("A.AUDITSTATE=2 ");
                        state++;
                    }
                    if ((filter & (int)BillListingFilter.UnAudit) == (int)BillListingFilter.UnAudit)
                    {
                        tempBuilder.Append("A.AUDITSTATE<>2 ");
                        state++;
                    }
                    if (state != 0 && state != 2)
                    {
                        if (builder.Length > 0)
                            builder.AppendFormat(" And {0}", tempBuilder.ToString());
                        else
                            builder.AppendFormat("{0}", tempBuilder.ToString());
                    }
                }
                if (billType == BillType.Master)
                {
                    tempBuilder.Length = 0;
                    state = 0;
                    if ((filter & (int)BillListingFilter.UnValidity) == (int)BillListingFilter.UnValidity)
                    {
                        tempBuilder.Append("A.ISVALIDITY=0 ");
                        state++;
                    }
                    if ((filter & (int)BillListingFilter.Validity) == (int)BillListingFilter.Validity)
                    {
                        tempBuilder.Append("A.ISVALIDITY=1 ");
                        state++;
                    }
                    if (state != 0 && state != 2)
                    {
                        if (builder.Length > 0)
                            builder.AppendFormat(" And {0}", tempBuilder.ToString());
                        else
                            builder.AppendFormat("{0}", tempBuilder.ToString());
                    }
                }
            }
            return builder.ToString();
        }

        private string GetEntryParam(LibEntryParam entryParam)
        {
            StringBuilder builder = new StringBuilder();
            if (entryParam != null && entryParam.ParamStore.Count > 0)
            {
                foreach (var item in entryParam.ParamStore)
                {
                    if (item.Value.GetType() == typeof(System.String))
                        builder.AppendFormat(" A.{0}={1} AND", item.Key, LibStringBuilder.GetQuotObject(item.Value));
                    else
                        builder.AppendFormat(" A.{0}={1} AND", item.Key, item.Value);
                }
                if (builder.Length > 0)
                    builder.Remove(builder.Length - 3, 3);
            }
            return builder.ToString();
        }


        private LibGridScheme GetListingGridScheme(LibHandle handle, string progId, LibEntryParam entryParam)
        {
            LibGridScheme gridScheme = null;
            StringBuilder builder = new StringBuilder();
            if (entryParam != null)
            {
                foreach (var item in entryParam.ParamStore)
                {
                    builder.AppendFormat("{0}", item.Value);
                }
            }
            string schemeName = Path.Combine(handle.UserId, string.Format("{0}{1}List.bin", progId, builder.ToString()));
            LibDisplayScheme displayScheme = null;
            string path = Path.Combine(EnvProvider.Default.MainPath, "Scheme", "ShowScheme", schemeName);
            if (File.Exists(path))
            {
                LibBinaryFormatter formatter = new LibBinaryFormatter();
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    displayScheme = (LibDisplayScheme)formatter.Deserialize(fs);
                }
            }
            if (displayScheme != null)
            {
                gridScheme = displayScheme.GridScheme[0];
            }
            return gridScheme;
        }

        private string GetQueryCondition(LibHandle libHandle, string progId, BillType billType, bool usingAudit, BillListingQuery listingQuery, LibEntryParam entryParam = null)
        {
            string query = BuildQuery(progId, billType, usingAudit, listingQuery.Condition, listingQuery.TimeFilter, listingQuery.Filter);
            if (entryParam != null)
            {
                string temp = GetEntryParam(entryParam);
                if (!string.IsNullOrEmpty(temp))
                    query = string.IsNullOrEmpty(query) ? temp : string.Format("{0} AND {1}", query, temp);
            }
            string browseCondition = LibPermissionControl.Default.GetShowCondition(libHandle, progId, libHandle.PersonId);
            if (!string.IsNullOrEmpty(browseCondition))
            {
                query = string.IsNullOrEmpty(query) ? browseCondition : string.Format("{0} AND {1}", query, browseCondition);
            }

            return query;
        }

        private void FillQueryData(LibHandle libHandle, string progId, string query, DataTable table, int pageSize, int pageCount)
        {
            LibDataAccess dataAccess = new LibDataAccess();
            SqlBuilder builder = new SqlBuilder(progId);
            string sql = builder.GetQuerySql(0, "A.*", query, "A.CREATETIME DESC");

            if (pageSize <= 0)
            {
                pageSize = 50;
            }
            sql = sql.Replace("Select", string.Format("Select Top {0}", pageSize * 4));
            //if (pageSize > 0)
            //{
            //    sql = sql.Replace("Select", "Select row_number() over (order by  A.CREATETIME DESC) ROWNUMFORPAGE,");
            //    sql = string.Format("select t.* from({0})t where ROWNUMFORPAGE > {1} and ROWNUMFORPAGE <= {2}", sql, pageSize * (pageCount - 1), pageSize * pageCount);
            //}
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                table.BeginLoadData();
                try
                {
                    while (reader.Read())
                    {
                        if ((LibCurrentState)LibSysUtils.ToInt32(reader["CURRENTSTATE"]) == LibCurrentState.Draft && string.Compare(libHandle.PersonId, LibSysUtils.ToString(reader["CREATORID"])) != 0)
                            continue;
                        //if (--maxCount < 0)
                        //    break;
                        DataRow newRow = table.NewRow();
                        newRow.BeginEdit();
                        try
                        {
                            int count = reader.FieldCount;
                            for (int i = 0; i < count; i++)
                            {
                                string name = reader.GetName(i);
                                if (table.Columns.Contains(name))
                                {
                                    if (!Convert.IsDBNull(reader[i]))
                                        newRow[name] = reader[i];
                                }
                            }
                        }
                        finally
                        {
                            newRow.EndEdit();
                        }
                        table.Rows.Add(newRow);
                    }
                }
                finally
                {
                    table.EndLoadData();
                }
            }
        }
        /// <summary>
        /// 查询功能Bcf清单页指定分类树节点的下级节点列表
        /// </summary>
        /// <param name="treeListingQuery"></param>
        /// <returns></returns>
        public List<TreeListingNode> GetBillTreeListing(TreeListingQuery treeListingQuery)
        {
            if (treeListingQuery == null)
                throw new ArgumentException("参数为空。");
            if (string.IsNullOrEmpty(treeListingQuery.Handle) || string.IsNullOrEmpty(treeListingQuery.ProgId))
                throw new Exception("参数为空。");
            string handle = treeListingQuery.Handle;
            string progId = treeListingQuery.ProgId;
            BillListingResult result = null;
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            List<TreeListingNode> listNode = new List<TreeListingNode>();
            LibBcfBase bcfObj = LibBcfSystem.Default.GetBcfInstance(progId);
            if (bcfObj == null || bcfObj.Template == null || (bcfObj.Template.BillType != BillType.Bill && bcfObj.Template.BillType != BillType.Master) ||
                bcfObj.Template.FuncPermission == null || bcfObj.Template.FuncPermission.TreeListing == null)
                return listNode;
            else
            {
                TreeListingConfig config = bcfObj.Template.FuncPermission.TreeListing;
                try
                {
                    RelativeSourceCollection relSources = (RelativeSourceCollection)bcfObj.DataSet.Tables[0].Columns[config.ColumnName].ExtendedProperties[FieldProperty.RelativeSource];
                    if (relSources.Count == 0 || relSources[0] == null || string.IsNullOrEmpty(relSources[0].RelSource))
                        return listNode;
                    string relProgId = relSources[0].RelSource;//只使用第一个
                    //to do
                    if (string.IsNullOrEmpty(relProgId))
                        return listNode;
                    LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel(relProgId);
                    if (sqlModel == null)
                        return listNode;
                    bool containCurrentState = sqlModel.Tables[0].Columns.Contains("CURRENTSTATE");//当前状态，包括审核等
                    bool containISVALIDITY = sqlModel.Tables[0].Columns.Contains("ISVALIDITY");//是否有效

                    DataColumn queryColumn = sqlModel.Tables[0].Columns[config.RelativeParentColumn];
                    PropertyCollection propertyList = queryColumn.ExtendedProperties;
                    LibDataType dataType = propertyList.ContainsKey(FieldProperty.DataType) ? (LibDataType)propertyList[FieldProperty.DataType] : LibDataTypeConverter.ConvertToLibType(queryColumn.DataType);
                    bool needQuot = dataType == LibDataType.Text || dataType == LibDataType.NText;
                    string sql = string.Format("select {0} as Id,{1} as Name from {2} where {3} = {4}",
                        config.RelativeIdColumn, config.RelativeNameColumn, sqlModel.Tables[0].TableName, config.RelativeParentColumn,
                        (needQuot) ? LibStringBuilder.GetQuotObject(treeListingQuery.NodeId) : treeListingQuery.NodeId);
                    if (containCurrentState)
                        sql += string.Format(" and ( CURRENTSTATE = {0} or ( CURRENTSTATE = {1} and CREATORID = {2}) )",
                            (int)LibCurrentState.Release, (int)LibCurrentState.Draft, LibStringBuilder.GetQuotString(libHandle.PersonId));
                    if (containISVALIDITY)
                        sql += " and ISVALIDITY = 1 ";
                    if (string.IsNullOrEmpty(config.OrderBy) == false)
                        sql += " order by " + config.OrderBy;
                    else if (sqlModel.Tables[0].Columns.Contains("CREATETIME"))
                        sql += " order by CREATETIME Asc";
                    LibDataAccess dataAccess = new LibDataAccess();
                    using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                    {
                        string id = string.Empty;
                        string name = string.Empty;
                        while (reader.Read())
                        {
                            try
                            {
                                id = LibSysUtils.ToString(reader["Id"]);
                                name = LibSysUtils.ToString(reader["Name"]);
                                listNode.Add(new TreeListingNode()
                                {
                                    Id = id,
                                    DisplayName = string.Format("{0}{1}{2}",
                                     config.NodeShowId ? id : "",
                                     config.NodeShowJoinChar ? config.NodeJoinChar : "",
                                     config.NodeShowName ? name : "")
                                });
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    LibCommUtils.AddOutput(@"Error\BillService",
                        string.Format("查询功能Bcf清单页指定分类树节点的下级节点列表,出现异常。\r\nMessage:{0}\r\nStackTrace:{1}", exp.Message, exp.StackTrace));
                }
            }
            return listNode;
        }
        public string GetBillListing(BillListingQuery listingQuery)
        {
            string handle = listingQuery.Handle;
            string progId = listingQuery.ProgId;
            BillListingResult result = null;
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            else
            {
                if (ProgIdHost.Instance.ProgIdRef.ContainsKey(progId))
                {
                    BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[progId];
                    string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                    Assembly assembly = Assembly.LoadFrom(path);
                    Type t = assembly.GetType(info.ClassName);
                    LibBcfData destObj = (LibBcfData)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                    DataTable table = destObj.DataSet.Tables[0];
                    LibEntryParam entryParam = null;
                    if (!string.IsNullOrEmpty(listingQuery.EntryParam))
                    {
                        entryParam = JsonConvert.DeserializeObject(listingQuery.EntryParam, typeof(LibEntryParam)) as LibEntryParam;
                    }
                    LibQueryField pageConditionField = new LibQueryField();

                    try
                    {
                        //zhangkj 20161214 增加获取列表数据前的模块自定义处理
                        destObj.BeforeFillList(libHandle, table, listingQuery, entryParam);
                    }
                    catch (Exception exp)
                    {
                        string ss = string.Format("BeforeFillList,Error:{0}", exp.ToString());
                        destObj.ManagerMessage.AddMessage(LibMessageKind.Error, ss);
                        throw exp;
                    }
                    //listingQuery.Condition.AddPageCondition(listingQuery.PageSize, listingQuery.PageCount);
                    string queryCondition = GetQueryCondition(libHandle, progId, destObj.Template.BillType, destObj.UsingAudit, listingQuery, entryParam);

                    FillQueryData(libHandle, progId, queryCondition, table, listingQuery.PageSize, listingQuery.PageCount);
                    try
                    {
                        //zhangkj 20161201 增加获取列表数据后的模块自定义处理
                        destObj.AfterFillList(libHandle, table, listingQuery, entryParam);
                    }
                    catch (Exception exp)
                    {
                        string ss = string.Format("AfterFillList,Error:{0}", exp.ToString());
                        destObj.ManagerMessage.AddMessage(LibMessageKind.Error, ss);
                        throw exp;
                    }

                    IList<string> columns = new List<string>();
                    StringBuilder filterBuilder = new StringBuilder();
                    foreach (DataColumn item in table.Columns)
                    {
                        columns.Add(item.ColumnName);
                        filterBuilder.Append("{");
                        filterBuilder.AppendFormat("key:'{0}',value:'{1}'", item.ColumnName, item.Caption);
                        filterBuilder.Append("},");
                    }
                    string filterField = string.Empty;
                    if (filterBuilder.Length > 0)
                    {
                        filterBuilder.Remove(filterBuilder.Length - 1, 1);
                        filterField = string.Format("[{0}]", filterBuilder.ToString());
                    }
                    //处理清单字段
                    string renderer = string.Empty;
                    LibGridScheme gridScheme = GetListingGridScheme(libHandle, progId, entryParam);//先找自定义的GridScheme
                    try
                    {
                        if (gridScheme == null)//如果没有自定义的
                            //zhangkj 20161201 获取功能模块自定义的GridScheme
                            gridScheme = destObj.GetDefinedGridScheme(libHandle, entryParam);
                    }
                    catch (Exception exp)
                    {
                        string ss = exp.ToString();
                        destObj.ManagerMessage.AddMessage(LibMessageKind.Error, ss);
                        throw exp;
                    }
                    if (gridScheme == null)
                    {
                        List<LayoutField> layoutFields = new List<LayoutField>();
                        int num = 0;
                        foreach (var item in columns)
                        {
                            if (num > 15)
                                layoutFields.Add(new LayoutField(table.Columns[item], 0) { Hidden = true });
                            else
                                layoutFields.Add(new LayoutField(table.Columns[item], 0));
                            num++;
                        }
                        renderer = JsBuilder.BuildGrid(layoutFields, true);
                    }
                    else
                    {
                        LibBillLayout layout = new LibBillLayout(destObj.DataSet);
                        LibGridLayoutBlock gridBlock = layout.BuildGrid(0, "", null, true);
                        gridBlock.GridScheme = gridScheme;
                        renderer = gridBlock.CreateRenderer();
                    }
                    destObj.Template.GetViewTemplate(destObj.DataSet);
                    AxCRL.Template.TableDetail tableDetail = destObj.Template.ViewTemplate.Tables[table.TableName];
                    result = new BillListingResult(table, renderer, tableDetail.Fields, tableDetail.Pk, filterField);

                }

            }
            return JsonConvert.SerializeObject(result);
        }

        //检查人员所具备的权限
        public List<int> CheckAllPermission(string handle, string progId)
        {
            List<int> permissionList = new List<int>();
            bool ret;
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == LibHandleCache.Default.GetSystemHandle())
            {
                return permissionList;
            }
            else
            {
                string roleId = libHandle.RoleId;
                LibRolePermission rolePermission = LibRolePermissionCache.Default.GetCacheItem(roleId);
                if (rolePermission.IsUnlimited)
                {
                    return permissionList;
                }
                else if (rolePermission.PermissionDic.ContainsKey(progId))
                {
                    LibPermission permission = rolePermission.PermissionDic[progId];
                    for (int i = 0; i < 16; i++)
                    {
                        if ((permission.OperateMark & (int)Math.Pow(2, i)) == (int)Math.Pow(2, i))
                            permissionList.Add((int)Math.Pow(2, i));
                    }
                }
            }
            return permissionList;
        }
        public string BatchExecBcfMethod(ExecuteBcfMethodParam param, IList<string[]> batchParams)
        {
            if (string.IsNullOrEmpty(param.ProgId))
            {
                throw new ArgumentNullException("ProgId", "ProgId is empty.");
            }
            IList<object[]> errorPks = new List<object[]>();
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(param.Handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            else
            {
                if (ProgIdHost.Instance.ProgIdRef.ContainsKey(param.ProgId))
                {
                    BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[param.ProgId];
                    string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                    Assembly assembly = Assembly.LoadFrom(path);
                    Type t = assembly.GetType(info.ClassName);
                    //List<string> exportFiles = null;
                    //bool isExportData = param.MethodName == "ExportData";
                    //if (isExportData)
                    //    exportFiles = new List<string>();
                    foreach (string[] item in batchParams)
                    {
                        LibBcfBase destObj = (LibBcfBase)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                        object[] destParam = RestoreParamFormat(t, param.MethodName, item);
                        destObj.Handle = libHandle;
                        result.Result = t.InvokeMember(param.MethodName, BindingFlags.InvokeMethod, null, destObj, destParam);
                        LibMessageList msgList = ((LibBcfBase)destObj).ManagerMessage.MessageList;
                        if (msgList.Count > 0)
                        {
                            StringBuilder strBuilder = new StringBuilder();
                            int count = 0;
                            object[] pk = (object[])destParam[0];
                            errorPks.Add(pk);
                            foreach (var col in destObj.DataSet.Tables[0].PrimaryKey)
                            {
                                strBuilder.AppendFormat("{0}为{1}", col.Caption, pk[count]);
                                count++;
                            }
                            result.Messages.Add(new LibMessage() { Message = string.Format("对{0}的操作，出现错误:", strBuilder.ToString()) });
                            result.Messages.AddRange(msgList);
                        }
                        //else
                        //{
                        //    if (isExportData)
                        //    {
                        //        string file = LibSysUtils.ToString(result.Result);
                        //        if (!string.IsNullOrEmpty(file))
                        //            exportFiles.Add(file);
                        //    }
                        //}
                    }
                    //if (isExportData)
                    //    result.Result = ZipExcelField(param.ProgId, exportFiles);
                    //else
                    result.Result = errorPks;
                    if (errorPks.Count == 0)
                        result.Messages.Add(new LibMessage() { Message = "操作成功。", MessageKind = LibMessageKind.Info });
                }
            }
            return JsonConvert.SerializeObject(result);
        }


        private string ZipExcelField(string progId, List<string> fileList)
        {
            if (fileList.Count == 0)
                return string.Empty;
            string fileName = string.Format("{0}-{1}.zip", progId, LibDateUtils.GetCurrentDateTime());
            string filePath = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "ExportData", fileName);
            using (FileStream zipToOpen = new FileStream(filePath, FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    foreach (string file in fileList)
                    {
                        ZipArchiveEntry readmeEntry = archive.CreateEntry(file);
                        using (Stream destStream = readmeEntry.Open())
                        {
                            using (FileStream fileStream = new FileStream(System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "ExportData", file), FileMode.Open, FileAccess.Read))
                            {
                                fileStream.CopyTo(destStream);
                            }
                        }
                    }
                }
            }
            return fileName;
        }


        private void FillRelationTable(DataSet dataSet, DataTable curTable, DataRow curRow, DataSet destDataSet)
        {
            foreach (DataRelation relation in curTable.ChildRelations)
            {
                DataRow[] childRowList = curRow.GetChildRows(relation);
                DataTable subTable = dataSet.Tables[relation.ChildTable.TableName];
                DataTable childTable = destDataSet.Tables[relation.ChildTable.TableName];
                childTable.BeginLoadData();
                try
                {
                    foreach (DataRow childRow in childRowList)
                    {
                        childTable.ImportRow(childRow);
                        FillRelationTable(dataSet, subTable, childRow, destDataSet);
                    }
                }
                finally
                {
                    childTable.EndLoadData();
                }
            }
        }

        public string ExportAllData(string handle, string progId, string pkStr, BillListingQuery listingQuery = null)
        {
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            string query = string.Empty;
            if (listingQuery != null && ProgIdHost.Instance.ProgIdRef.ContainsKey(progId))
            {
                BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[progId];
                string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                Assembly assembly = Assembly.LoadFrom(path);
                Type t = assembly.GetType(info.ClassName);
                LibBcfData destObj = (LibBcfData)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                LibEntryParam entryParam = null;
                if (!string.IsNullOrEmpty(listingQuery.EntryParam))
                {
                    entryParam = JsonConvert.DeserializeObject(listingQuery.EntryParam, typeof(LibEntryParam)) as LibEntryParam;
                }
                query = GetQueryCondition(libHandle, progId, destObj.Template.BillType, destObj.UsingAudit, listingQuery, entryParam);
            }
            LibDataAccess dataAccess = new LibDataAccess();
            SqlBuilder builder = new SqlBuilder(progId);
            string sql = builder.GetQuerySql(0, pkStr, query);
            IList<object[]> batchParams = new List<object[]>();
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    int count = reader.FieldCount;
                    object[] item = new object[count];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        item[i] = reader[i];
                    }
                    batchParams.Add(item);
                }
            }
            return BatchExportData(handle, progId, batchParams);
        }

        public string BatchExportData(string handle, string progId, IList<object[]> batchParams)
        {
            string fileName = string.Empty;
            if (string.IsNullOrEmpty(progId))
            {
                throw new ArgumentNullException("ProgId", "ProgId is empty.");
            }
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            else if (batchParams.Count > 0)
            {
                LibBcfData bcf = LibBcfSystem.Default.GetBcfInstance(progId) as LibBcfData;
                if (bcf != null)
                {
                    bcf.Handle = libHandle;
                    int idx = 0;
                    int count = batchParams.Count;
                    DataSet curDataSet = bcf.BrowseTo(batchParams[idx]);
                    DataSet destDataSet = curDataSet.Clone();
                    destDataSet.EnforceConstraints = false;
                    try
                    {
                        do
                        {
                            if (idx != 0)
                            {
                                bcf = LibBcfSystem.Default.GetBcfInstance(progId) as LibBcfData;
                                curDataSet = bcf.BrowseTo(batchParams[idx]);
                            }
                            if (curDataSet != null)
                            {
                                for (int i = 0; i < curDataSet.Tables.Count; i++)
                                {
                                    DataTable curTable = curDataSet.Tables[i];
                                    if (curTable.ExtendedProperties.ContainsKey(TableProperty.IsVirtual) && (bool)curTable.ExtendedProperties[TableProperty.IsVirtual])
                                    {
                                        if (destDataSet.Tables.Contains(curTable.TableName))
                                            destDataSet.Tables.Remove(curTable.TableName);
                                        continue;//虚表不导出
                                    }
                                    DataTable destTable = destDataSet.Tables[i];
                                    destTable.BeginLoadData();
                                    try
                                    {
                                        foreach (DataRow curRow in curTable.Rows)
                                        {
                                            destTable.ImportRow(curRow);
                                        }
                                    }
                                    finally
                                    {
                                        destTable.EndLoadData();
                                    }
                                }
                            }
                            curDataSet.Clear();
                            idx++;
                        } while (idx < count);
                    }
                    finally
                    {
                        destDataSet.EnforceConstraints = true;
                    }
                    fileName = string.Format("{0}-{1}.xls", progId, LibDateUtils.GetCurrentDateTime());
                    string filePath = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "ExportData", fileName);
                    AxCRL.Core.Excel.LibExcelHelper libExcelHelper = new Core.Excel.LibExcelHelper();
                    libExcelHelper.ExportToExcel(filePath, destDataSet);
                }
            }
            return fileName;
        }

        public string BatchImportData(string handle, string progId, string fileName, string entryParam = null)
        {
            if (string.IsNullOrEmpty(progId))
            {
                throw new ArgumentNullException("ProgId", "ProgId is empty.");
            }
            IList<object[]> errorPks = new List<object[]>();
            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            else
            {
                if (ProgIdHost.Instance.ProgIdRef.ContainsKey(progId))
                {
                    BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[progId];
                    string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                    Assembly assembly = Assembly.LoadFrom(path);
                    Type t = assembly.GetType(info.ClassName);
                    LibBcfBase destObj = (LibBcfBase)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                    destObj.Handle = libHandle;
                    LibEntryParam entryParamObj = JsonConvert.DeserializeObject(LibSysUtils.ToString(entryParam), typeof(LibEntryParam)) as LibEntryParam;
                    result.Result = t.InvokeMember("GetBatchImportData", BindingFlags.InvokeMethod, null, destObj, new object[] { fileName, entryParamObj });
                    DataSet dataSet = result.Result as DataSet;
                    if (dataSet != null)
                    {
                        int i = 0;
                        foreach (DataRow curRow in dataSet.Tables[0].Rows)
                        {
                            i++;
                            DataSet destDataSet = dataSet.Clone();
                            destDataSet.EnforceConstraints = false;
                            try
                            {
                                destDataSet.Tables[0].BeginLoadData();
                                try
                                {
                                    destDataSet.Tables[0].ImportRow(curRow);
                                }
                                finally
                                {
                                    destDataSet.Tables[0].EndLoadData();
                                }
                                FillRelationTable(dataSet, dataSet.Tables[0], curRow, destDataSet);
                                //将同步配置添加到数据集中
                                if (dataSet.Tables.Contains(LibFuncPermission.SynchroDataSettingTableName))
                                {
                                    DataTable syncSettingDt = dataSet.Tables[LibFuncPermission.SynchroDataSettingTableName];
                                    DataTable destSyncSettingDt = destDataSet.Tables[LibFuncPermission.SynchroDataSettingTableName];
                                    if (syncSettingDt != null && destSyncSettingDt != null)
                                    {
                                        foreach (DataRow row in syncSettingDt.Rows)
                                        {
                                            destSyncSettingDt.BeginLoadData();
                                            try
                                            {
                                                destSyncSettingDt.ImportRow(row);
                                            }
                                            finally
                                            {
                                                destSyncSettingDt.EndLoadData();
                                            }
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                destDataSet.EnforceConstraints = true;
                            }

                            //destObj = (LibBcfBase)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                            if (!destObj.ManagerMessage.IsThrow)
                            {
                                destObj.Handle = libHandle;
                                try
                                {
                                    t.InvokeMember("ImportDataSet", BindingFlags.InvokeMethod, null, destObj, new object[] { destDataSet, entryParamObj });
                                }
                                catch (Exception ex)
                                {
                                    destObj.ManagerMessage.MessageList.Add(new LibMessage() { Message = ex.InnerException.ToString() });
                                }
                            }
                        }
                        if (destObj.ManagerMessage.IsThrow)
                        {
                            LibMessageList msgList = destObj.ManagerMessage.MessageList;
                            result.Messages.Add(new LibMessage() { Message = string.Format("出现错误:") });
                            result.Messages.AddRange(msgList);
                        }
                        else if (destObj.ManagerMessage.Count > 0)
                        {
                            LibMessageList msgList = destObj.ManagerMessage.MessageList;
                            result.Messages.Add(new LibMessage() { Message = string.Format("提示:") });
                            result.Messages.AddRange(msgList);
                        }
                        if (result.Messages.Count == 0)
                            result.Messages.Add(new LibMessage() { Message = "操作成功。", MessageKind = LibMessageKind.Info });
                    }
                }
            }
            result.Result = null;
            return JsonConvert.SerializeObject(result);
        }


        public int GetFormatUnit(string unitId)
        {
            return LibSysUtils.ToInt32(LibFormatUnitCache.Default.GetFormatData(unitId));
        }


        public string SelectFuncField(string handle, string progId, int tableIndex = 0)
        {
            StringBuilder builder = new StringBuilder();
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            if (ProgIdHost.Instance.ProgIdRef.ContainsKey(progId))
            {
                BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[progId];
                string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                Assembly assembly = Assembly.LoadFrom(path);
                Type t = assembly.GetType(info.ClassName);
                LibBcfBase destObj = (LibBcfBase)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                destObj.Handle = libHandle;
                if (destObj.DataSet.Tables.Count > tableIndex)
                {
                    DataTable table = destObj.DataSet.Tables[tableIndex];
                    int i = 0;
                    foreach (DataColumn col in table.Columns)
                    {
                        if (i == 0)
                            builder.Append("{Id:'" + col.ColumnName + "',Name:'" + col.Caption + "'}");
                        else
                            builder.Append(",{Id:'" + col.ColumnName + "',Name:'" + col.Caption + "'}");
                        i++;
                    }
                }
            }
            return string.Format("[{0}]", builder.ToString());
        }

        public string SelectQueryField(string handle, string progId)
        {
            List<LibQueryField> list = new List<LibQueryField>();
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            if (ProgIdHost.Instance.ProgIdRef.ContainsKey(progId))
            {
                BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[progId];
                string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                Assembly assembly = Assembly.LoadFrom(path);
                Type t = assembly.GetType(info.ClassName);
                LibBcfBase destObj = (LibBcfBase)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                destObj.Handle = libHandle;
                DataTable table = destObj.DataSet.Tables[0];
                foreach (DataColumn col in table.Columns)
                {
                    if (col.ExtendedProperties.ContainsKey(FieldProperty.AllowCondition) && !(bool)col.ExtendedProperties[FieldProperty.AllowCondition])
                        continue;
                    string contorlJs = col.DataType == typeof(bool) ? string.Empty : JsBuilder.BuildField(new LayoutField(col, 0));
                    list.Add(new LibQueryField()
                    {
                        DataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType],
                        DisplayText = col.Caption,
                        Field = col.ColumnName,
                        ControlJs = contorlJs
                    });
                }
            }
            return JsonConvert.SerializeObject(list);
        }


        private void SaveDisplaySchemeCore(string handle, string progId, string entryParam, string displayScheme, bool isBillListing)
        {
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            else
            {
                string path = Path.Combine(EnvProvider.Default.MainPath, "Scheme", "ShowScheme", libHandle.UserId);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                StringBuilder builder = new StringBuilder();
                if (!string.IsNullOrEmpty(entryParam))
                {
                    LibEntryParam entryParamObj = JsonConvert.DeserializeObject(entryParam, typeof(LibEntryParam)) as LibEntryParam;
                    if (entryParamObj != null)
                    {
                        foreach (var item in entryParamObj.ParamStore)
                        {
                            builder.Append(item.Value);
                        }
                    }
                }
                if (isBillListing)
                    path = Path.Combine(path, string.Format("{0}{1}List.bin", progId, builder.ToString()));
                else
                    path = Path.Combine(path, string.Format("{0}{1}.bin", progId, builder.ToString()));
                LibDisplayScheme displaySchemeObj = JsonConvert.DeserializeObject(displayScheme, typeof(LibDisplayScheme)) as LibDisplayScheme;
                if (displaySchemeObj != null)
                {
                    LibBinaryFormatter formatter = new LibBinaryFormatter();
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        formatter.Serialize(fs, displaySchemeObj);
                    }
                }
            }
        }

        private void ClearDisplaySchemeCore(string handle, string progId, string entryParam, bool isBillListing)
        {
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            else
            {
                string path = Path.Combine(EnvProvider.Default.MainPath, "Scheme", "ShowScheme", libHandle.UserId);
                StringBuilder builder = new StringBuilder();
                if (!string.IsNullOrEmpty(entryParam))
                {
                    LibEntryParam entryParamObj = JsonConvert.DeserializeObject(entryParam, typeof(LibEntryParam)) as LibEntryParam;
                    if (entryParamObj != null)
                    {
                        foreach (var item in entryParamObj.ParamStore)
                        {
                            builder.Append(item.Value);
                        }
                    }
                }
                if (isBillListing)
                    path = Path.Combine(path, string.Format("{0}{1}List.bin", progId, builder.ToString()));
                else
                    path = Path.Combine(path, string.Format("{0}{1}.bin", progId, builder.ToString()));
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        public void SaveDisplayScheme(string handle, string progId, string entryParam, string displayScheme)
        {
            SaveDisplaySchemeCore(handle, progId, entryParam, displayScheme, false);
        }

        public void ClearDisplayScheme(string handle, string progId, string entryParam)
        {
            ClearDisplaySchemeCore(handle, progId, entryParam, false);
        }

        public void SaveBillListingScheme(string handle, string progId, string entryParam, string displayScheme)
        {
            SaveDisplaySchemeCore(handle, progId, entryParam, displayScheme, true);
        }

        public void ClearBillListingScheme(string handle, string progId, string entryParam)
        {
            ClearDisplaySchemeCore(handle, progId, entryParam, true);
        }

        public IList<FuzzyResult> FuzzySearchField(string handle, string relSource, string query, string condition, int tableIndex = 0)
        {
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            IList<FuzzyResult> list = new List<FuzzyResult>();
            SqlBuilder sqlBuilder = new SqlBuilder(relSource);
            string sql = sqlBuilder.GetFuzzySql(tableIndex, query, condition);
            LibDataAccess dataAccess = new LibDataAccess();
            int count = 0;
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    if (reader.FieldCount == 1)
                        list.Add(new FuzzyResult(LibSysUtils.ToString(reader[0]), string.Empty));
                    else
                        list.Add(new FuzzyResult(LibSysUtils.ToString(reader[0]), LibSysUtils.ToString(reader[1])));
                    count++;
                    if (count == 30)
                        break;
                }
            }
            return list;
        }


        public string CheckFieldValue(string handle, string fields, string relSource, string curPk, string condition, int tableIndex = 0)
        {
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            if (string.IsNullOrEmpty(relSource) || string.IsNullOrEmpty(fields) || string.IsNullOrEmpty(curPk))
                return null;
            Dictionary<string, object> returnValue = new Dictionary<string, object>();
            SqlBuilder sqlBuilder = new SqlBuilder(relSource);
            LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel(relSource);  //  LibSqlModelCache.Default.Get< LibSqlModel>(relSource);
            string colName = sqlModel.Tables[tableIndex].PrimaryKey[sqlModel.Tables[tableIndex].PrimaryKey.Length - 1].ColumnName;
            char prefix = (char)(tableIndex + (int)'A');
            string sql = sqlBuilder.GetQuerySql(tableIndex, fields, string.Format("{0}.{1}={2} {3}", prefix, colName, LibStringBuilder.GetQuotString(curPk), condition));
            LibDataAccess dataAccess = new LibDataAccess();
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    int count = reader.FieldCount;
                    for (int i = 0; i < count; i++)
                    {
                        string name = reader.GetName(i);
                        if (!returnValue.ContainsKey(name))
                            returnValue.Add(name, reader[i]);
                    }
                }
            }
            //增加对axp.FuncList的特殊处理
            if (relSource == "axp.FuncList" && string.IsNullOrEmpty(curPk) == false)
            {
                //增加是否启用清单页分类树的配置
                LibBcfBase bcfObj = LibBcfSystem.Default.GetBcfInstance(curPk);
                if (bcfObj == null || bcfObj.Template == null || (bcfObj.Template.BillType != BillType.Bill && bcfObj.Template.BillType != BillType.Master) ||
                    bcfObj.Template.FuncPermission == null || bcfObj.Template.FuncPermission.TreeListing == null)
                {
                    returnValue["ENABLETREELISTING"] = false;
                    returnValue["TREECOLUMNNAME"] = string.Empty;
                }
                else
                {
                    returnValue["ENABLETREELISTING"] = true;
                    returnValue["TREECOLUMNNAME"] = bcfObj.Template.FuncPermission.TreeListing.ColumnName;
                }
            }
            return JsonConvert.SerializeObject(returnValue);
        }
        /// <summary>
        /// 获取系统消息(本站点及下级站点)
        /// </summary>
        /// <param name="handle">y</param>
        /// <param name="startTime"></param>
        /// <param name="onlyUnRead"></param>
        /// <returns></returns>
        public List<LibNews> GetMyNews(string handle, long startTime, bool onlyUnRead)
        {
            List<LibNews> listNews = GetMyNewsThisSite(handle, startTime, onlyUnRead);
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            Dictionary<string, LinkSiteInfo> canLoginSlaveSites = CrossSiteHelper.GetCanLoginSlaveSites(libHandle.UserId);
            if (canLoginSlaveSites != null && canLoginSlaveSites.Count > 0)
            {
                //存在可访问的从站，则合并获取从站的系统消息数据
                ExecuteBcfMethodParam callParams = new ExecuteBcfMethodParam()
                {
                    ProgId = "axp.UserNews",
                    MethodName = "GetMyNews",
                    MethodParam = ExecuteBcfMethodParam.ConvertMethodParams(new object[] { startTime, onlyUnRead }),
                    TimeoutMillSecs = 30000 //设置超时时间，单位毫秒
                };
                try
                {
                    Dictionary<string, ExecuteBcfMethodResult> dicRets = CrossSiteHelper.CrossSiteBcfCall(handle, canLoginSlaveSites.Keys.ToList(), callParams);
                    if (dicRets != null)
                    {
                        ExecuteBcfMethodResult result = null;
                        foreach (string siteId in dicRets.Keys)
                        {
                            result = dicRets[siteId];
                            if (result == null)
                                continue;
                            if (result.Messages != null || result.Messages.Count > 0)
                            {
                                bool hasError = result.Messages.Any(msg =>
                                  {
                                      return msg.MessageKind == LibMessageKind.Error || msg.MessageKind == LibMessageKind.SysException;
                                  });
                                if (hasError)
                                    continue;
                            }
                            string resultStr = result.Result.ToString();
                            List<LibNews> subNews = JsonConvert.DeserializeObject<List<LibNews>>(resultStr);
                            if (subNews != null && subNews.Count > 0)
                            {
                                subNews.ForEach(news =>
                                {
                                    news.SourceSiteId = siteId;
                                    if (canLoginSlaveSites.ContainsKey(siteId) && canLoginSlaveSites[siteId] != null)
                                    {
                                        news.SourceSiteName = canLoginSlaveSites[siteId].ShortName;
                                        news.SourceSiteFullName = canLoginSlaveSites[siteId].SiteName;
                                        news.SourceSiteUrl = canLoginSlaveSites[siteId].SiteUrl;
                                    }
                                });
                                listNews.AddRange(subNews);
                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    LibCommUtils.AddOutput(@"Error\CrossSiteCall", string.Format("GetMyNews error:{0}\r\nStacktrace:{1}", exp.Message, exp.StackTrace));
                }
            }
            return listNews;
        }
        /// <summary>
        /// 获取系统消息(仅本站点)
        /// </summary>
        /// <param name="handle">y</param>
        /// <param name="startTime"></param>
        /// <param name="onlyUnRead"></param>
        /// <returns></returns>
        public List<LibNews> GetMyNewsThisSite(string handle, long startTime, bool onlyUnRead)
        {
            List<LibNews> list = new List<LibNews>();
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            long beginTime = startTime;
            if (startTime == 0)
            {
                startTime = (long)LibDateUtils.AddDayToLibDate(LibDateUtils.GetCurrentDate(), -7) * 1000000;
            }
            string sql = string.Format("select A.NEWSID,A.TITLE,A.MAINCONTENT,A.INFOID,D.PERSONNAME,A.CREATETIME,B.PROGID,C.PROGNAME,C.BILLTYPE,B.RESULTDATA,A.ISREAD " +
                "from AXPUSERNEWS A left join AXAEXECTASKDATA B on B.EXECTASKDATAID=A.INFOID left join AXPFUNCLIST C on C.PROGID=B.PROGID " +
                "left join COMPERSON D on D.PERSONID=A.PERSONID " +
                "where A.USERID={0} and A.CREATETIME>{1} {2}", LibStringBuilder.GetQuotString(libHandle.UserId), startTime, onlyUnRead ? " and ISREAD=0" : "");
            LibDataAccess dataAccess = new LibDataAccess();
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    LibNews news = new LibNews();
                    news.NewsId = LibSysUtils.ToString(reader["NEWSID"]);
                    news.Title = LibSysUtils.ToString(reader["TITLE"]);
                    news.MainContent = LibSysUtils.ToString(reader["MAINCONTENT"]);
                    news.InfoId = LibSysUtils.ToString(reader["INFOID"]);
                    news.PersonName = LibSysUtils.ToString(reader["PERSONNAME"]);
                    news.IsRead = LibSysUtils.ToBoolean(reader["ISREAD"]);
                    long createTime = LibSysUtils.ToInt64(reader["CREATETIME"]);
                    news.CreateDate = LibDateUtils.GetLibTimePart(createTime, LibDateTimePartEnum.Date);
                    news.CreateTime = LibDateUtils.GetLibTimePart(createTime, LibDateTimePartEnum.Time);
                    news.ProgId = LibSysUtils.ToString(reader["PROGID"]);
                    news.DisplayText = LibSysUtils.ToString(reader["PROGNAME"]);
                    news.BillType = (BillType)LibSysUtils.ToInt32(reader["BILLTYPE"]);
                    if (news.BillType == BillType.Bill || news.BillType == BillType.Master)
                    {
                        string resultData = LibSysUtils.ToString(reader["RESULTDATA"]);
                        string[] data = resultData.Split(';');
                        if (data.Length > 0)
                        {
                            news.CurPks = data[0];
                            if (data.Length == 2)
                                news.EntryParam = data[1];
                        }
                    }
                    list.Add(news);
                }
            }
            return list;
        }

        public int GetUnreadNews(string handle, long startTime, bool onlyUnRead)
        {
            List<LibNews> list = new List<LibNews>();
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            long beginTime = startTime;
            if (startTime == 0)
            {
                startTime = (long)LibDateUtils.AddDayToLibDate(LibDateUtils.GetCurrentDate(), -7) * 1000000;
            }
            string sql = string.Format("select count(*) " +
                "from AXPUSERNEWS A left join AXAEXECTASKDATA B on B.EXECTASKDATAID=A.INFOID left join AXPFUNCLIST C on C.PROGID=B.PROGID " +
                "left join COMPERSON D on D.PERSONID=A.PERSONID " +
                "where A.USERID={0} and A.CREATETIME>{1} {2}", LibStringBuilder.GetQuotString(libHandle.UserId), startTime, onlyUnRead ? " and ISREAD=0" : "");
            LibDataAccess dataAccess = new LibDataAccess();
            int unread = LibSysUtils.ToInt32(dataAccess.ExecuteScalar(sql));
            return unread;
        }

        public string SetMyNewsReadState(string handle, string[] newsList)
        {
            SetMyNewsReadStateThisSite(handle, newsList);
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                return "用户句柄无效,需重新登录。";
            }
            if (newsList == null && newsList.Length == 0)
                return string.Empty;
            Dictionary<string, LinkSiteInfo> canLoginSlaveSites = CrossSiteHelper.GetCanLoginSlaveSites(libHandle.UserId);
            if (canLoginSlaveSites != null && canLoginSlaveSites.Count > 0)
            {
                //存在可访问的从站，则合并获取从站的系统消息数据
                ExecuteBcfMethodParam callParams = new ExecuteBcfMethodParam()
                {
                    ProgId = "axp.UserNews",
                    MethodName = "SetMyNewsReadState",
                    MethodParam = ExecuteBcfMethodParam.ConvertMethodParams(new object[] { newsList }),
                    TimeoutMillSecs = 30000 //设置超时时间，单位毫秒
                };

                try
                {
                    Dictionary<string, ExecuteBcfMethodResult> dicRets = CrossSiteHelper.CrossSiteBcfCall(handle, canLoginSlaveSites.Keys.ToList(), callParams);
                    if (dicRets == null || dicRets.Keys.Count == 0)
                        return string.Empty;
                    string retStr = string.Empty;
                    foreach (string key in dicRets.Keys)
                    {
                        if (dicRets[key] != null && dicRets[key].Messages != null && dicRets[key].Messages.HasError())
                            retStr += canLoginSlaveSites[key].ShortName + ",";
                    }
                    if (retStr.Length > 0)
                    {
                        retStr = retStr.Substring(retStr.Length - 1);
                        retStr = string.Format("联动执行子站点:{0}的操作时出现错误，请联系管理员。", retStr);
                    }
                    return retStr;
                }
                catch (Exception exp)
                {
                    LibCommUtils.AddOutput(@"Error\CrossSiteCall", string.Format("SetMyNewsReadState error:{0}\r\nStacktrace:{1}", exp.Message, exp.StackTrace));
                    return string.Format("联动执行子站点操作时出现异常,请联系管理员。");
                }
            }
            else
                return string.Empty;
        }
        /// <summary>
        /// 标记本站点的指定系统消息为已读
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="newsList"></param>
        public void SetMyNewsReadStateThisSite(string handle, string[] newsList)
        {
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            if (newsList != null && newsList.Length > 0)
            {
                List<string> sqlList = new List<string>();
                foreach (string newsId in newsList)
                {
                    if (!string.IsNullOrEmpty(newsId))
                    {
                        sqlList.Add(string.Format("update AXPUSERNEWS set ISREAD=1 where NEWSID={0}", LibStringBuilder.GetQuotString(newsId)));
                    }
                }
                if (sqlList.Count > 0)
                {
                    LibDataAccess dataAccess = new LibDataAccess();
                    dataAccess.ExecuteNonQuery(sqlList);
                }
            }
        }

        /// <summary>
        /// 获取所有功能清单
        /// </summary>
        public IList<FuzzyResult> GetSelectData(string handle, string relSource, string query, string condition, int tableIndex = 0)
        {
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            IList<FuzzyResult> list = new List<FuzzyResult>();
            SqlBuilder sqlBuilder = new SqlBuilder(relSource);
            string sql = sqlBuilder.GetFuzzySql(tableIndex, query, condition);
            LibDataAccess dataAccess = new LibDataAccess();
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    if (reader.FieldCount == 1)
                        list.Add(new FuzzyResult(LibSysUtils.ToString(reader[0]), string.Empty));
                    else
                        list.Add(new FuzzyResult(LibSysUtils.ToString(reader[0]), LibSysUtils.ToString(reader[1])));
                }
            }
            return list;
        }
        /// <summary>
        /// 获取已发布的功能
        /// </summary>
        public List<FuncInfo> GetPublishFunc()
        {
            string sql = "select * from AXPFUNCPUBLISH";
            List<FuncInfo> FuncList = new List<FuncInfo>();
            LibDataAccess dataAccess = new LibDataAccess();
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    FuncInfo funcInfo = new FuncInfo()
                    {
                        MenuItem = LibSysUtils.ToString(reader["MENUITEM"]),
                        ProgId = LibSysUtils.ToString(reader["PROGID"]),
                        ProgName = LibSysUtils.ToString(reader["PROGNAME"]),
                        BillType = LibSysUtils.ToInt32(reader["BILLTYPE"]),
                        EntryParam = LibSysUtils.ToString(reader["ENTRYPARAM"]),
                        PublishDate = LibSysUtils.ToString(reader["PUBLISHDATE"]),
                    };
                    FuncList.Add(funcInfo);
                }
            }
            return FuncList;
        }
        /// <summary>
        /// 删除发布的功能
        /// </summary>
        public void DeleteFuncPublish(string handle, string ProgId, string EntryParam)
        {
            CheckAdmin(handle);
            LibDataAccess dataAccess = new LibDataAccess();
            string sql = string.Empty;
            sql = string.Format("DELETE FROM AXPFUNCPUBLISH WHERE PROGID='{0}' AND ENTRYPARAM='{1}'", ProgId, EntryParam);
            dataAccess.ExecuteNonQuery(sql);
        }
        /// /// <summary>
        /// 设置发布功能
        /// </summary>
        public int SetFuncPublish(string funcInfoJson)
        {
            int result = 0;
            string sql = string.Empty;
            FuncInfo funcInfo = JsonConvert.DeserializeObject<FuncInfo>(funcInfoJson);
            funcInfo.PublishDate = DateTime.Now.ToShortDateString();
            LibDataAccess dataAccess = new LibDataAccess();
            sql = string.Format(@"SELECT COUNT(*) FROM AXPFUNCPUBLISH WHERE PROGID='{0}' AND ENTRYPARAM='{1}'", funcInfo.ProgId, funcInfo.EntryParam);
            int count = LibSysUtils.ToInt32(dataAccess.ExecuteScalar(sql));
            if (count == 0)
            {
                sql = string.Format(@"INSERT INTO AXPFUNCPUBLISH  
                               (MENUITEM ,PROGID ,PROGNAME ,
                                BILLTYPE ,ENTRYPARAM ,PUBLISHDATE)     
                                VALUES ('{0}','{1}','{2}',{3},'{4}','{5}')", funcInfo.MenuItem, funcInfo.ProgId, funcInfo.ProgName,
                                funcInfo.BillType, funcInfo.EntryParam, funcInfo.PublishDate);
                dataAccess.ExecuteNonQuery(sql);
                result = 0;
            }
            else
            {
                result = 1;
            }
            return result;
        }
        /// <summary>
        /// 获取入口参数信息
        /// </summary>
        public string GetEntryParam(string handle, string progId)
        {
            string ret = string.Empty;

            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            if (!string.IsNullOrEmpty(progId))
            {
                LibBcfBase bcf = LibBcfSystem.Default.GetBcfInstance(progId);
                IList<string> entryParam = bcf.Template.FuncPermission.EntryParam;
                if (entryParam.Count > 0)
                {
                    DataTable masterTable = bcf.DataSet.Tables[0];
                    List<LayoutField> list = new List<LayoutField>();
                    foreach (string param in entryParam)
                    {
                        LayoutField field = new LayoutField(masterTable.Columns[param], 0);
                        list.Add(field);
                    }
                    ret = JsonConvert.SerializeObject(list);
                }
            }
            return ret;
        }
        /// /// <summary>
        /// 检查是否管理员权限
        /// </summary>
        public bool CheckAdmin(string handle)
        {
            bool isAdmin = false;
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            if (libHandle.UserId == "admin")
            {
                isAdmin = true;
            }
            return isAdmin;
        }




        public bool CanUseFunc(string handle, string progId)
        {
            bool canUse = false;
            if (string.IsNullOrEmpty(progId))
                throw new ArgumentNullException("progId");
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            if (!string.IsNullOrEmpty(libHandle.RoleId))
            {
                canUse = LibPermissionControl.Default.CanUse(libHandle, progId);
            }
            else
                canUse = true; //暂时不对未设置角色的用户控制其权限，后续去掉
            return canUse;
        }

        private class AttributeDescInfo
        {
            private Dictionary<string, string> _Data;
            private int _Len;
            private string _Name;

            public string Name
            {
                get { return _Name; }
                set { _Name = value; }
            }

            public int Len
            {
                get { return _Len; }
                set { _Len = value; }
            }
            public Dictionary<string, string> Data
            {
                get
                {
                    if (_Data == null)
                        _Data = new Dictionary<string, string>();
                    return _Data;
                }
            }
        }

        public LibAttrInfo GetAttributeDesc(string attrId, string attrCode)
        {
            LibAttrInfo attrInfo = new LibAttrInfo();
            string sql = string.Format("select C.RELATIONMARK,C.INTERVALMARK,C.ATTRIBUTELEN,A.ATTRIBUTEITEM,A.ATTRIBUTECODELEN,B.ATTRCODE,B.ATTRVALUE " +
            "from COMATTRIBUTEDETAIL A inner join COMATTRIBUTE C on C.ATTRIBUTEID=A.ATTRIBUTEID left join COMATTRIBUTEVALUE B on B.ATTRIBUTEID=A.ATTRIBUTEID and " +
            "B.PARENTROWID=A.ROW_ID where A.ATTRIBUTEID={0} Order by A.RowNo,B.RowNo", LibStringBuilder.GetQuotString(attrId));
            int relationMark = 0, intervalMark = 0, attrubuteLen = 0;
            LibDataAccess dataAccess = new LibDataAccess();
            List<AttributeDescInfo> list = new List<AttributeDescInfo>();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                int idx = 0;
                while (reader.Read())
                {
                    if (idx == 0)
                    {
                        relationMark = LibSysUtils.ToInt32(reader["RELATIONMARK"]);
                        intervalMark = LibSysUtils.ToInt32(reader["INTERVALMARK"]);
                        attrubuteLen = LibSysUtils.ToInt32(reader["ATTRIBUTELEN"]);
                    }
                    string attrItem = LibSysUtils.ToString(reader["ATTRIBUTEITEM"]);
                    string code = LibSysUtils.ToString(reader["ATTRCODE"]);
                    if (!dic.ContainsKey(attrItem))
                    {
                        dic.Add(attrItem, code);
                        list.Add(new AttributeDescInfo() { Name = attrItem, Len = LibSysUtils.ToInt32(reader["ATTRIBUTECODELEN"]) });
                    }
                    AttributeDescInfo info = list[list.Count - 1];
                    if (!info.Data.ContainsKey(code))
                    {
                        info.Data.Add(code, LibSysUtils.ToString(reader["ATTRVALUE"]));
                    }
                }
            }
            string relationMarkStr = relationMark == 0 ? ":" : "=";
            int index = 0;
            foreach (AttributeDescInfo item in list)
            {
                string itemCode = string.Empty;
                if (attrCode.Length >= index + item.Len)
                {
                    itemCode = attrCode.Substring(index, item.Len);
                }
                string value = string.Empty;
                if (string.IsNullOrEmpty(itemCode) || !item.Data.ContainsKey(itemCode))
                {
                    itemCode = dic[item.Name];
                }
                value = item.Data[itemCode];
                switch (intervalMark)
                {
                    case 0:
                        attrInfo.AttrDesc += '【' + item.Name + relationMarkStr + value + '】';
                        break;
                    case 1:
                        attrInfo.AttrDesc += item.Name + relationMarkStr + value + ';';
                        break;
                    case 2:
                        attrInfo.AttrDesc += item.Name + relationMarkStr + value + '\n';
                        break;
                }
                attrInfo.AttrCode += itemCode;
                index += item.Len;
            }
            return attrInfo;
        }

        public string GetAttributeControl(string attrId, string attrCode)
        {
            LibAttrControl control = new LibAttrControl();
            string sql = string.Format("select C.RELATIONMARK,C.INTERVALMARK,A.ATTRIBUTEITEM,A.ATTRIBUTECODELEN,B.ATTRCODE,B.ATTRVALUE " +
                "from COMATTRIBUTEDETAIL A inner join COMATTRIBUTE C on C.ATTRIBUTEID=A.ATTRIBUTEID left join COMATTRIBUTEVALUE B on B.ATTRIBUTEID=A.ATTRIBUTEID and " +
                "B.PARENTROWID=A.ROW_ID where A.ATTRIBUTEID={0} Order by A.RowNo,B.RowNo", LibStringBuilder.GetQuotString(attrId));
            DataSet ds = new DataSet();
            DataTable table = new DataTable();
            ds.Tables.Add(table);
            string preAttrItem = string.Empty;
            int preAttrItemLen = 0;
            string itemCode = string.Empty;
            LibTextOptionCollection keyValueOption = null;
            List<string> list = new List<string>();
            DefineField defineField = null;
            LibDataAccess dataAccess = new LibDataAccess();
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                int idx = 0;
                int index = 0;
                while (reader.Read())
                {
                    if (idx == 0)
                    {
                        control.RelationMark = LibSysUtils.ToInt32(reader["RELATIONMARK"]);
                        control.IntervalMark = LibSysUtils.ToInt32(reader["INTERVALMARK"]);
                    }
                    string attrItem = LibSysUtils.ToString(reader["ATTRIBUTEITEM"]);
                    if (attrItem != preAttrItem)
                    {
                        if (keyValueOption != null && keyValueOption.Count > 0)
                        {
                            if (string.IsNullOrEmpty(itemCode))
                                defineField.DefaultValue = keyValueOption[0].Key;
                            else
                                defineField.DefaultValue = itemCode;
                            defineField.KeyValueOption = keyValueOption;
                        }
                        if (!string.IsNullOrEmpty(itemCode))
                            itemCode = string.Empty;
                        int len = LibSysUtils.ToInt32(reader["ATTRIBUTECODELEN"]);
                        if (attrCode.Length >= index + len)
                        {
                            itemCode = attrCode.Substring(index, len);
                        }
                        index += len;
                        if (defineField != null)
                        {
                            DataSourceHelper.AddColumn(defineField);
                            list.Add(defineField.Name);
                        }
                        defineField = new DefineField(table, attrItem, attrItem);
                        keyValueOption = new LibTextOptionCollection();
                        defineField.DataType = LibDataType.Text;
                        defineField.ControlType = LibControlType.KeyValueOption;
                        keyValueOption.Add(new LibTextOption(LibSysUtils.ToString(reader["ATTRCODE"]), LibSysUtils.ToString(reader["ATTRVALUE"])));
                        preAttrItem = attrItem;
                        preAttrItemLen = len;
                    }
                    else
                    {
                        keyValueOption.Add(new LibTextOption(LibSysUtils.ToString(reader["ATTRCODE"]), LibSysUtils.ToString(reader["ATTRVALUE"])));
                    }
                }
                if (keyValueOption != null && keyValueOption.Count > 0)
                {
                    if (string.IsNullOrEmpty(itemCode))
                        defineField.DefaultValue = keyValueOption[0].Key;
                    else
                        defineField.DefaultValue = itemCode;
                    defineField.KeyValueOption = keyValueOption;
                }
                if (defineField != null)
                {
                    DataSourceHelper.AddColumn(defineField);
                    list.Add(defineField.Name);
                }
            }
            if (table.Columns.Count > 0)
            {
                LibBillLayout layout = new LibBillLayout(ds);
                LibControlLayoutBlock block = layout.BuildControlGroup(0, "", list);
                control.Renderer = block.CreateRenderer();
                TableDetail tableDetail = new TableDetail(table);
                control.Fields = tableDetail.Fields;
                control.NewRowObj = tableDetail.NewRowObj;
            }
            return JsonConvert.SerializeObject(control);
        }


        public BillType GetBillType(string progId)
        {
            LibDataAccess dataAccess = new LibDataAccess();
            return (BillType)LibSysUtils.ToInt32(dataAccess.ExecuteScalar(string.Format("select BILLTYPE from AXPFUNCLIST where PROGID={0}", LibStringBuilder.GetQuotString(progId)), false));
        }


        public string GetEntryRender(string handle, string progId)
        {
            string ret = string.Empty;
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            if (!string.IsNullOrEmpty(progId))
            {
                LibBcfBase bcf = LibBcfSystem.Default.GetBcfInstance(progId);
                IList<string> entryParam = bcf.Template.FuncPermission.EntryParam;
                if (entryParam.Count > 0)
                {
                    DataTable masterTable = bcf.DataSet.Tables[0];
                    List<LayoutField> list = new List<LayoutField>();
                    foreach (string param in entryParam)
                    {
                        LayoutField field = new LayoutField(masterTable.Columns[param], 0);
                        list.Add(field);
                    }
                    ret = JsBuilder.BuildControlGroup(list);
                }
            }
            return ret;
        }


        public LoadAttachInfo LoadAttachInfo(string handle, string attachmentSrc, string progId, List<Dictionary<string, object>> data)
        {
            LoadAttachInfo loadAttachInfo = new LoadAttachInfo();
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            LibDataAccess dataAccess = new LibDataAccess();
            if (!string.IsNullOrEmpty(attachmentSrc))
            {
                //string sql = string.Format("select A.ATTACHMENTNAME,A.ORDERID,A.CANUSE,B.FILENAME,C.PERSONNAME,B.CREATETIME from AXPATTACHMENTRECORD A left join AXPATTACHMENTRECORDDETAIL B on A.BELONGTOID=B.BELONGTOID and " +
                //    "A.ORDERID=B.ORDERID left join COMPERSON C on C.PERSONID=B.PERSONID where A.BELONGTOID={0} Order by A.ORDERNUM,B.CREATETIME", LibStringBuilder.GetQuotString(attachmentSrc));
                //Zhangkj 20170106 修改 增加对应到文档库中的DOCID，DIRID

                //string checkDocTable = string.Empty;//检查是否有文档表
                //if (dataAccess.DatabaseType == LibDatabaseType.SqlServer)
                //{
                //    checkDocTable = string.Format("select count(*) from sys.objects where name = '{0}'", "DMDOCUMENT");
                //}
                //else
                //{
                //    checkDocTable = string.Format("select count(*) from user_tables where table_name='{0}'", "DMDOCUMENT");
                //}
                //bool isExistDocTable = LibSysUtils.ToInt32(dataAccess.ExecuteScalar(checkDocTable)) > 0;

                bool isExistDocTable = false;
                LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel("dm.Document");
                if (sqlModel != null && sqlModel.Tables.Count > 0 && sqlModel.Tables[0].Columns.Contains("DOCID"))
                {
                    isExistDocTable = true;
                }

                string sql = string.Empty;
                if (isExistDocTable)
                {
                    //有文档表DMDOCUMENT才可以联表查询获取文档目录等信息
                    sql = string.Format("select A.ATTACHMENTNAME,A.ORDERID,A.CANUSE,A.DOCID,D.DIRID,B.FILENAME,C.PERSONNAME,B.CREATETIME from AXPATTACHMENTRECORD A left join AXPATTACHMENTRECORDDETAIL B on A.BELONGTOID=B.BELONGTOID and " +
                        "A.ORDERID=B.ORDERID left join COMPERSON C on C.PERSONID=B.PERSONID " +
                        " left join DMDOCUMENT D on D.DOCID=A.DOCID " +
                        " where A.BELONGTOID={0} Order by A.ORDERNUM,B.CREATETIME", LibStringBuilder.GetQuotString(attachmentSrc));
                }
                else
                {
                    sql = string.Format("select A.ATTACHMENTNAME,A.ORDERID,A.CANUSE,B.FILENAME,C.PERSONNAME,B.CREATETIME from AXPATTACHMENTRECORD A left join AXPATTACHMENTRECORDDETAIL B on A.BELONGTOID=B.BELONGTOID and " +
                    "A.ORDERID=B.ORDERID left join COMPERSON C on C.PERSONID=B.PERSONID where A.BELONGTOID={0} Order by A.ORDERNUM,B.CREATETIME", LibStringBuilder.GetQuotString(attachmentSrc));
                }


                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    int preOrderId = 0;
                    LibAttachInfo preInfo = null;
                    while (reader.Read())
                    {
                        string attachName = LibSysUtils.ToString(reader["ATTACHMENTNAME"]);
                        int orderId = LibSysUtils.ToInt32(reader["ORDERID"]);
                        string docId = string.Empty;
                        string dirId = string.Empty;
                        if (isExistDocTable)
                        {
                            docId = LibSysUtils.ToString(reader["DOCID"]);
                            dirId = LibSysUtils.ToString(reader["DIRID"]);
                        }

                        if (loadAttachInfo.MaxOrderId < orderId)
                            loadAttachInfo.MaxOrderId = orderId;
                        if (!LibSysUtils.ToBoolean(reader["CANUSE"]))
                            continue;
                        if (preOrderId != orderId)
                        {
                            preOrderId = orderId;
                            if (preInfo != null)
                                preInfo.FileName = preInfo.HistoryList[preInfo.HistoryList.Count - 1].FileName;
                            LibAttachInfo info = new LibAttachInfo();
                            info.AttachName = attachName;
                            info.OrderId = orderId;
                            info.DocId = docId;//Zhangkj 20170106 添加
                            info.DirId = dirId;

                            loadAttachInfo.AttachList.Add(info);
                            preInfo = info;
                        }
                        LibAttachHistory history = new LibAttachHistory();
                        history.FileName = LibSysUtils.ToString(reader["FILENAME"]);
                        history.Info = string.Format("上传人:{0} 时间:{1}", reader["PERSONNAME"], new DateTime(LibSysUtils.ToInt64(reader["CREATETIME"])).ToLocalTime());
                        preInfo.HistoryList.Add(history);
                    }
                    if (preInfo != null)
                        preInfo.FileName = preInfo.HistoryList[preInfo.HistoryList.Count - 1].FileName;
                }
            }
            else if (!string.IsNullOrEmpty(progId) && data != null && data.Count > 0)
            {
                string sql = string.Format("select A.ATTACHMENTTPLID,A.USECONDITION,A.ROW_ID from AXPATTACHMENTTPLDETAIL A inner join AXPATTACHMENTTPL B on B.ATTACHMENTTPLID=A.ATTACHMENTTPLID  where B.PROGID={0}", LibStringBuilder.GetQuotString(progId));
                int rowId = 0;
                string attachmentTplId = string.Empty;
                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        if (string.IsNullOrEmpty(attachmentTplId))
                            attachmentTplId = LibSysUtils.ToString(reader["ATTACHMENTTPLID"]);
                        string condition = LibSysUtils.ToString(reader["USECONDITION"]);
                        if (string.IsNullOrEmpty(condition))
                        {
                            rowId = LibSysUtils.ToInt32(reader["ROW_ID"]);
                        }
                        else
                        {
                            if (LibParseHelper.Parse(condition, data))
                            {
                                rowId = LibSysUtils.ToInt32(reader["ROW_ID"]);
                                break;
                            }
                        }
                    }
                }
                if (rowId > 0)
                {
                    sql = string.Format("select ROW_ID,ATTACHMENTNAME from AXPATTACHMENTTPLSUB where ATTACHMENTTPLID={0} and PARENTROWID={1} ORDER BY ROWNO",
                        LibStringBuilder.GetQuotString(attachmentTplId), rowId);
                    using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            LibAttachInfo info = new LibAttachInfo();
                            info.AttachName = LibSysUtils.ToString(reader["ATTACHMENTNAME"]);
                            info.OrderId = LibSysUtils.ToInt32(reader["ROW_ID"]);
                            loadAttachInfo.AttachList.Add(info);
                        }
                    }
                }
            }
            return loadAttachInfo;
        }


        public CompanyParam GetCompanyFormat(string companyId)
        {
            CompanyParam param = new CompanyParam();
            string sql = string.Empty;
            if (string.IsNullOrEmpty(companyId))
            {
                sql = "select PRICEPRECISION,AMOUNTPRECISION,TAXRATEPRECISION from AXPCOMPANYPARAM";
            }
            else
            {
                sql = string.Format("select PRICEPRECISION,AMOUNTPRECISION,TAXRATEPRECISION from AXPCOMPANYPARAM where ORGID={0}", LibStringBuilder.GetQuotString(companyId));
            }
            LibDataAccess dataAccess = new LibDataAccess();
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql, false))
            {
                if (reader.Read())
                {
                    param.Price = LibSysUtils.ToInt32(reader["PRICEPRECISION"]);
                    param.Amount = LibSysUtils.ToInt32(reader["AMOUNTPRECISION"]);
                    param.TaxRate = LibSysUtils.ToInt32(reader["TAXRATEPRECISION"]);
                }
            }
            return param;
        }

        public IList<FuzzyResult> GetRptFields(string progId)
        {
            List<FuzzyResult> list = new List<FuzzyResult>();
            string msg = string.Empty;
            List<List<DefineField>> fieldList = BcfTemplateMethods.GetBcfDefineFields(progId, out msg);
            if (string.IsNullOrEmpty(msg) == false || fieldList == null || fieldList.Count == 0 || fieldList[0] == null || fieldList[0].Count == 0)
                return list;
            foreach (DefineField field in fieldList[0])
            {
                if (field == null)
                    continue;
                list.Add(new FuzzyResult(field.Name, field.DisplayName));
            }
            return list;
        }
    }
}
