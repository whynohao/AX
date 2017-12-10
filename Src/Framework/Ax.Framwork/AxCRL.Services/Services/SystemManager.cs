using AxCRL.Bcf;
using AxCRL.Bcf.ScheduleTask;
using AxCRL.Comm.Define;
using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Core;
using AxCRL.Data;
using AxCRL.Data.SqlBuilder;
using AxCRL.Services;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace AxCRL.Services
{
    [Inspector.CrossDomainInspectorBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [GlobalExceptionHandlerBehaviour(typeof(GlobalExceptionHandler))]
    public class SystemManager : ISystemManager
    {
        public void SystemUpgrade()
        {
            LibSqlModelCache.Default.RemoveAll();//升级数据库时需要将SqlModel的缓存清空

            //重新生成ProgId
            Dictionary<string, Assembly> assemblyDic = new Dictionary<string, Assembly>();
            ProgIdConfigListingManager.BuildListing(EnvProvider.Default.MainPath, EnvProvider.Default.ExtendPath, assemblyDic);
            //在本地构建SqlModel文件,升级数据库表、创建单据存储过程
            ProgIdHost.Instance.Run();
            List<string> updateBusinessTaskList = new List<string>() { "DELETE FROM AXPBUSINESSTASK" };
            foreach (var item in ProgIdHost.Instance.ProgIdRef)
            {
                BcfServerInfo info = item.Value;
                if (!assemblyDic.ContainsKey(info.DllName))
                    continue;
                Assembly assembly = assemblyDic[info.DllName];
                Type destType = assembly.GetType(info.ClassName);
                if (destType == null)
                    continue;
                MethodInfo[] methodInfo = destType.GetMethods();
                if (methodInfo != null && methodInfo.Length > 0)
                {
                    foreach (MethodInfo subItem in methodInfo)
                    {
                        if (subItem.IsDefined(typeof(LibBusinessTaskAttribute)))
                        {
                            LibBusinessTaskAttribute attr = (LibBusinessTaskAttribute)subItem.GetCustomAttribute(typeof(LibBusinessTaskAttribute));
                            updateBusinessTaskList.Add(string.Format("insert into AXPBUSINESSTASK(PROGID,BUSINESSTASKID,BUSINESSTASKNAME) values({0},{1},{2})", LibStringBuilder.GetQuotString(item.Key),
                                LibStringBuilder.GetQuotString(attr.Name), LibStringBuilder.GetQuotString(attr.DisplayText)));
                        }
                    }
                }
                throw new Exception("test");
                //if (!destType.IsSubclassOf(typeof(LibBcfData)) && !destType.IsSubclassOf(typeof(LibBcfGrid)))
                //    continue;
                try
                {
                    LibBcfBase destObj = destType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null) as LibBcfBase;
                    if (destObj != null)
                        SaveSqlModel(item.Key, destObj.DataSet);
                }
                catch (Exception exp)
                {
                    throw new Exception(string.Format("UpdateError(SaveSqlModel):\r\n{0}  {1}  {2}  {3}。", item.Key, item.Value.ClassName, item.Value.DllName, exp.ToString()));
                }

            }
            ILibDbSchema schemaHelper = null;
            LibDataAccess dataAccess = new LibDataAccess();
            LibDatabaseType databaseType = dataAccess.DatabaseType;
            if (LibDatabaseType.Oracle == databaseType)
                schemaHelper = new LibOracleDbSchema();
            else
                schemaHelper = new LibSqlServerDbSchema();
            List<string> updateFunList = new List<string>() { "DELETE FROM AXPFUNCLIST" };
            List<string> updateFunButtonList = new List<string>() { "DELETE FROM AXPFUNCBUTTON" };
            List<string> updateBrowseStoreProcedureList = new List<string>();
            foreach (var item in ProgIdHost.Instance.ProgIdRef)
            {
                BcfServerInfo info = item.Value;
                if (!assemblyDic.ContainsKey(info.DllName))
                    continue;
                Assembly assembly = assemblyDic[info.DllName];
                try
                {
                    Type destType = assembly.GetType(info.ClassName);
                    LibBcfBase destObj = destType.InvokeMember(null, BindingFlags.CreateInstance, null, null, null) as LibBcfBase;
                    if (destObj != null)
                    {

                        LibFuncPermission funcPermission = destObj.Template.FuncPermission;
                        updateFunList.Add(GetSqlForFunList(destObj.ProgId, destObj.Template.DisplayText, funcPermission.ConfigPack, funcPermission.CanMenu, funcPermission.KeyCode, funcPermission.Permission, destObj.Template.BillType, LibSysUtils.ToString(funcPermission.ProgTag)));
                        Dictionary<string, string> dic = destObj.Template.GetViewTemplate(destObj.DataSet).Layout.GetButtonList();
                        if (dic != null)
                        {
                            foreach (var button in dic)
                            {
                                updateFunButtonList.Add(string.Format("insert into AXPFUNCBUTTON(PROGID,BUTTONID,BUTTONNAME) values('{0}','{1}','{2}')", destObj.ProgId, button.Key, button.Value));
                            }
                        }

                        if (!destType.IsSubclassOf(typeof(LibBcfData)) && !destType.IsSubclassOf(typeof(LibBcfGrid)))
                            continue;
                        schemaHelper.DicUniqueDataSql = destObj.GetUniqueDataSqlForUpdate(dataAccess.DatabaseType);//添加对于唯一性字段数据的提前处理Sql
                        schemaHelper.UpdateTables(destObj.DataSet, false);
                        //schemaHelper.CreateTables(destObj.DataSet);
                        SqlBuilder sqlBuilder = new SqlBuilder(item.Key);
                        string sql;
                        if (LibDatabaseType.Oracle == databaseType)
                            sql = sqlBuilder.BuildBrowseStoreProcedureByOracle(destObj.Template.BillType);
                        else
                            sql = sqlBuilder.BuildBrowseStoreProcedure(destObj.Template.BillType);
                        if (!string.IsNullOrEmpty(sql))
                        {
                            string name = destObj.Template.ProgId.Replace('.', '_');
                            if (LibDatabaseType.SqlServer == databaseType)
                            {
                                updateBrowseStoreProcedureList.Add(string.Format("IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID({0}) AND type in (N'P', N'PC')) DROP PROCEDURE {1}", LibStringBuilder.GetQuotString(name), name));
                            }
                            updateBrowseStoreProcedureList.Add(sql);
                        }
                    }
                }
                catch (Exception exp)
                {
                    throw new Exception(string.Format("UpdateError(UpdateTable):\r\n{0}  {1}  {2}  {3}\r\n。", item.Key, item.Value.ClassName, item.Value.DllName, exp.ToString()));
                }
            }

            //创建浏览存储过程
            foreach (string sql in updateBrowseStoreProcedureList)
            {
                try
                {
                    dataAccess.ExecuteNonQuery(sql, false);
                }
                catch (Exception exp)
                {
                    throw new Exception(string.Format("UpdateError(updateBrowseStoreProcedureList):\r\n{0} \r\n{1}。", sql, exp.ToString()));
                }
            }

            //更新功能清单
            dataAccess.ExecuteNonQuery(updateFunList, false);
            dataAccess.ExecuteNonQuery(updateFunButtonList, false);
            //更新业务任务表
            dataAccess.ExecuteNonQuery(updateBusinessTaskList, false);
            //创建存储过程
            CreateStoredProcedure(Path.Combine(EnvProvider.Default.MainPath, "StoredProcedure"), dataAccess);
            CreateStoredProcedure(Path.Combine(EnvProvider.Default.ExtendPath, "StoredProcedure"), dataAccess);
            this.InitData();
        }

        private void CreateStoredProcedure(string path, LibDataAccess dataAccess)
        {
            if (Directory.Exists(path))
            {
                string extendName = dataAccess.DatabaseType == LibDatabaseType.Oracle ? "*.prc" : "*.sql";
                string[] files = Directory.GetFiles(path, extendName, SearchOption.AllDirectories);
                if (files != null)
                {
                    foreach (string item in files)
                    {
                        string sql = string.Empty;
                        using (FileStream fs = new FileStream(item, FileMode.Open))
                        {
                            using (TextReader reader = new StreamReader(fs))
                            {
                                sql = reader.ReadToEnd();
                            }
                        }
                        string name = Path.GetFileNameWithoutExtension(item);
                        if (dataAccess.DatabaseType == LibDatabaseType.SqlServer)
                        {
                            dataAccess.ExecuteNonQuery(string.Format("IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID({0}) AND type in (N'P', N'PC')) DROP PROCEDURE {1} ", LibStringBuilder.GetQuotString(name), name));
                            dataAccess.ExecuteNonQuery(string.Format("IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID({0}) AND type in (N'V')) DROP VIEW {1} ", LibStringBuilder.GetQuotString(name), name));
                        }
                        if (!string.IsNullOrEmpty(sql))
                            dataAccess.ExecuteNonQuery(sql, false);
                    }
                }
            }
        }

        private void SaveSqlModel(string name, DataSet dataSet)
        {
            string preFix = name.Substring(0, name.IndexOf('.'));
            string path = Path.Combine(EnvProvider.Default.MainPath, "SqlModel", preFix);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            LibSqlModel libDataSet = new LibSqlModel();
            libDataSet.CloneDataSet(dataSet);
            using (FileStream fs = new FileStream(Path.Combine(path, string.Format("{0}.bin", name)), FileMode.Create, FileAccess.Write))
            {
                LibBinaryFormatter formatter = new LibBinaryFormatter();
                formatter.Serialize(fs, libDataSet);
            }
        }

        private string GetSqlForFunList(string progId, string progName, string configPack, bool canMenu, string keyCode, int permission, BillType billType, string progTag)
        {
            return string.Format(@"INSERT INTO axpFuncList (PROGID,PROGNAME,CONFIGPACK,CANMENU,KEYCODE,PERMISSION,BILLTYPE,PROGTAG) VALUES({0},{1},{2},{3},{4},{5},{6},{7})",
                LibStringBuilder.GetQuotString(progId), LibStringBuilder.GetQuotString(progName), LibStringBuilder.GetQuotString(configPack), Convert.ToInt32(canMenu),
                LibStringBuilder.GetQuotString(keyCode), permission, (int)billType, LibStringBuilder.GetQuotString(progTag));
        }

        private void InitData()
        {
            //授权规格
            LibDataAccess dataAccess = new LibDataAccess();
            decimal count = LibSysUtils.ToDecimal((dataAccess.ExecuteScalar("select count(*) from AXPPURCHASESPEC")));
            if (count == 0)
            {
                dataAccess.ExecuteNonQuery("insert into AXPPURCHASESPEC(PURCHASERID,PURCHASERNAME,MAXUSERCOUNT,MAXWORKSTATIONCOUNT) values('ax','ax',1000,-1)");
            }
            bool existsINTERNALID = false;
            LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel("axp.User");
            if (sqlModel != null && sqlModel.Tables.Count > 0 && sqlModel.Tables[0].Columns.Contains("INTERNALID"))
            {
                existsINTERNALID = true;
            }
            string userId = LibSysUtils.ToString(dataAccess.ExecuteScalar("select USERID from AXPUSER where USERID='admin'"));
            if (string.IsNullOrEmpty(userId))
            {
                //考虑默认账户 使用admin               
                if (existsINTERNALID == false)
                {
                    dataAccess.ExecuteNonQuery("insert into AXPUSER(USERID,USERPASSWORD,ISUSE) values('admin','admin',1)");
                }
                else
                {
                    //如果AXPUser不再是Grid数据，则会有内码字段
                    dataAccess.ExecuteNonQuery("insert into AXPUSER(USERID,USERPASSWORD,ISUSE,INTERNALID,CREATORID,CURRENTSTATE) values('admin','admin',1,'" + Guid.NewGuid().ToString() + "','(NotSet)',2)");
                }
            }
            if (existsINTERNALID)
            {
                // 升级时如果发现系统账户(axp.User)的数据表存在内码字段（即类型已修改为主数据），则需要修改所有内码为空的账户信息，为其生成新的Guid
                if (dataAccess.DatabaseType == LibDatabaseType.SqlServer)
                {
                    dataAccess.ExecuteNonQuery("update AXPUSER set INTERNALID = NEWID(),CURRENTSTATE = 2 where INTERNALID=''");
                }
                else if (dataAccess.DatabaseType == LibDatabaseType.Oracle)
                {
                    dataAccess.ExecuteNonQuery("update AXPUSER set INTERNALID = sys_guid(),CURRENTSTATE = 2 where INTERNALID=''");
                }
            }
        }
        private ILibDbSchema GetSchemaHelper(LibDataAccess dataAccess)
        {
            LibDatabaseType databaseType = dataAccess.DatabaseType;
            if (LibDatabaseType.Oracle == databaseType)
                return new LibOracleDbSchema();
            else
                return new LibSqlServerDbSchema();
        }

        public void OpenScheduleTask()
        {
            LibScheduleTaskHost.Default.InitTask();
        }
    }

    public class GlobalExceptionHandler : IErrorHandler
    {
        /// <summary>
        /// 测试log4net
        /// </summary>
        #region IErrorHandler Members
        /// <summary>
        /// HandleError
        /// </summary>
        /// <param name="ex">ex</param>
        /// <returns>true</returns>
        public bool HandleError(Exception ex)
        {
            return true;
        }
        /// <summary>
        /// ProvideFault
        /// </summary>
        /// <param name="ex">ex</param>
        /// <param name="version">version</param>
        /// <param name="msg">msg</param>
        public void ProvideFault(Exception ex, MessageVersion version, ref Message msg)
        {
            var newEx = new FaultException(string.Format("WCF接口出错 {0}", ex.TargetSite.Name));
            MessageFault msgFault = newEx.CreateMessageFault();
            msg = Message.CreateMessage(version, msgFault, newEx.Action);
        }
        #endregion
    }

    public class GlobalExceptionHandlerBehaviourAttribute : Attribute, IServiceBehavior
    {
        private readonly Type _errorHandlerType;

        public GlobalExceptionHandlerBehaviourAttribute(Type errorHandlerType)
        {
            _errorHandlerType = errorHandlerType;
        }

        #region IServiceBehavior Members

        public void Validate(ServiceDescription description,
                             ServiceHostBase serviceHostBase)
        {
        }

        public void AddBindingParameters(ServiceDescription description,
                                         ServiceHostBase serviceHostBase,
                                         Collection<ServiceEndpoint> endpoints,
                                         BindingParameterCollection parameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription description,
                                          ServiceHostBase serviceHostBase)
        {
            var handler =
                (IErrorHandler)Activator.CreateInstance(_errorHandlerType);

            foreach (ChannelDispatcherBase dispatcherBase in
                serviceHostBase.ChannelDispatchers)
            {
                var channelDispatcher = dispatcherBase as ChannelDispatcher;
                if (channelDispatcher != null)
                    channelDispatcher.ErrorHandlers.Add(handler);
            }
        }

        #endregion
    }
}
