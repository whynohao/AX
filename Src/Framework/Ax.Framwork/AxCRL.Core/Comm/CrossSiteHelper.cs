/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：跨站点数据交互帮助类
 * 创建标识：Zhangkj 2017/06/16
 * 
************************************************************************/
using AxCRL.Bcf;
using AxCRL.Comm.Entity;
using AxCRL.Comm.Enums;
using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Data;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AxCRL.Core.Comm
{
    /// <summary>
    /// 跨站点数据交互帮助类
    /// </summary>
    public class CrossSiteHelper
    {
        /// <summary>
        /// 是否存在外接站点相关的数据表
        /// </summary>
        public static readonly bool ExistLinkSiteTable = LibSqlModelCache.Default.Contains("axp.LinkSite");
        /// <summary>
        /// 是否存在用户可访问的外接站点相关数据表
        /// </summary>
        public static readonly bool ExistUserLinkSiteTable = LibSqlModelCache.Default.Contains("axp.User", "AXPUSERSITE");
        /// <summary>
        /// 是否存在同步数据配置及历史的数据表
        /// </summary>
        public static readonly bool ExistAxpSyncDataInfo = LibSqlModelCache.Default.Contains("axp.SyncDataHistory") && LibSqlModelCache.Default.Contains("axp.SyncDataSetting");        
        /// <summary>
        /// 获取用户可访问的下级站点代码列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>站点代码与站点信息的字典</returns>
        public static Dictionary<string, LinkSiteInfo> GetCanLoginSlaveSites(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;
            Dictionary<string, LinkSiteInfo> siteIds = new Dictionary<string, LinkSiteInfo>();
            try
            {
                if (ExistLinkSiteTable == false || ExistUserLinkSiteTable == false)
                    return null;

                string sql = string.Format("select distinct B.* from AXPUSERSITE A " +
                    " left join AXPLINKSITE B on A.SITEID = B.SITEID" +
                    " where A.USERID = {0} and ISSLAVE = 1", LibStringBuilder.GetQuotString(userId));
                LibDataAccess dataAccess = new LibDataAccess();
                string name = string.Empty;
                string id = string.Empty;
                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        id = LibSysUtils.ToString(reader["SITEID"]);
                        name = LibSysUtils.ToString(reader["SHORTNAME"]);
                        if (string.IsNullOrEmpty(id) == false)
                        {
                            siteIds[id] = new LinkSiteInfo()
                            {
                                SiteId = id,
                                SiteName = LibSysUtils.ToString(reader["SITENAME"]),
                                ShortName = LibSysUtils.ToString(reader["SHORTNAME"]),
                                SiteUrl = LibSysUtils.ToString(reader["SITEURL"]),
                                SvcUrl = LibSysUtils.ToString(reader["SVCURL"]),
                                IsSlave = LibSysUtils.ToBoolean(reader["ISSLAVE"]),
                                IsSendTo = LibSysUtils.ToBoolean(reader["ISSENDTO"]),
                            };
                        }
                    }
                }
            }
            catch
            {
                //to do log
            }
            return siteIds;
        }
        /// <summary>
        /// 获取站点代码对应的子站点信息。如果站点代码为空则表示获取所有子站点信息
        /// </summary>
        /// <param name="siteIds">站点代码列表</param>
        /// <param name="checkNeedSendTo"></param>
        /// <returns>站点代码与站点信息的字典</returns>
        public static Dictionary<string, LinkSiteInfo> GetLinkSites(string[] siteIds = null, bool checkNeedSendTo = false)
        {
            string siteIdStr = string.Empty;
            if (siteIds != null && siteIds.Length > 0)
            {
                foreach (string siteId in siteIds)
                {
                    if (string.IsNullOrEmpty(siteId))
                        continue;
                    siteIdStr += LibStringBuilder.GetQuotString(siteId) + ",";
                }
                if (string.IsNullOrEmpty(siteIdStr) == false)
                    siteIdStr = siteIdStr.Substring(0, siteIdStr.Length - 1);
            }
            Dictionary<string, LinkSiteInfo> siteInfos = new Dictionary<string, LinkSiteInfo>();
            try
            {
                if (ExistLinkSiteTable == false || ExistUserLinkSiteTable == false)
                    return null;
                string sql = string.Format("select * from AXPLINKSITE ");
                if (string.IsNullOrEmpty(siteIdStr) == false)
                    sql += " where SITEID in (" + siteIdStr + ")";
                LibDataAccess dataAccess = new LibDataAccess();
                string name = string.Empty;
                string id = string.Empty;
                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        id = LibSysUtils.ToString(reader["SITEID"]);
                        name = LibSysUtils.ToString(reader["SHORTNAME"]);
                        if (string.IsNullOrEmpty(id) == false)
                        {
                            siteInfos[id] = new LinkSiteInfo()
                            {
                                SiteId = id,
                                SiteName = LibSysUtils.ToString(reader["SITENAME"]),
                                ShortName = LibSysUtils.ToString(reader["SHORTNAME"]),
                                SiteUrl = LibSysUtils.ToString(reader["SITEURL"]),
                                SvcUrl = LibSysUtils.ToString(reader["SVCURL"]),
                                IsSlave = LibSysUtils.ToBoolean(reader["ISSLAVE"]),
                                IsSendTo = LibSysUtils.ToBoolean(reader["ISSENDTO"]),
                            };
                        }
                    }
                }
            }
            catch
            {
                //to do log
            }
            if (checkNeedSendTo)
            {
                if (siteInfos != null && siteInfos.Count > 0)
                {
                    siteInfos = (from item in siteInfos.Values
                                 where item.IsSendTo
                                 select item).ToDictionary(t => t.SiteId, t => t);//筛选需要同步到的站点
                }
            }
            return siteInfos;
        }
        /// <summary>
        /// 获取站点代码对应的站点Url
        /// </summary>
        /// <param name="linkSiteIds"></param>
        /// <returns>返回站点代码与站点Url的对应关系字典</returns>
        public static Dictionary<string, string> GetSiteUrls(List<string> linkSiteIds)
        {
            if (linkSiteIds == null || linkSiteIds.Count == 0)
                return null;
            Dictionary<string, string> dicSites = new Dictionary<string, string>();
            try
            {
                if (ExistLinkSiteTable == false)
                    return null;
                string siteIdsStr = string.Empty;
                linkSiteIds.ForEach(item =>
                {
                    siteIdsStr += LibStringBuilder.GetQuotString(item) + ",";
                });
                siteIdsStr = siteIdsStr.Remove(siteIdsStr.Length - 1);
                string sql = string.Format("select distinct SITEID,SITEURL,SVCURL from AXPLINKSITE where  SITEID in ({0})", siteIdsStr);
                LibDataAccess dataAccess = new LibDataAccess();
                string url = string.Empty;
                string id = string.Empty;
                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        id = LibSysUtils.ToString(reader["SITEID"]);
                        url = LibSysUtils.ToString(reader["SVCURL"]);
                        if (string.IsNullOrEmpty(url))
                            url = LibSysUtils.ToString(reader["SITEURL"]);
                        if (string.IsNullOrEmpty(url) == false)
                        {
                            dicSites[id] = url;
                        }
                    }
                }
            }
            catch
            {
                //to do log
            }
            return dicSites;
        }

        /// <summary>
        /// 向多个目标站点发起跨站Bcf方法调用请求。
        /// 此方法执行时会阻塞
        /// 如果有向多个站点请求执行，会同时（并发线程）向多个站点发起请求，请求完毕后再汇总执行结果返回
        /// 如果参数有误会抛出异常
        /// </summary>
        /// <param name="handle">当前用户的标识Handle</param>
        /// <param name="linkSiteIds">目标站点代码列表</param>
        /// <param name="callParams">包含SSO令牌信息、请求方法、请求参数等callParams</param>
        /// <param name="dataAccess">可选参数：数据库访问器。如果调用时使用了数据库事务，需要将开启了事务的数据库访问器传递进来，避免在本方法中查询数据库时因事务锁表而死锁。</param>
        /// <returns>返回执行结果字典，键值为目标站点Id，值为执行结果</returns>
        public static Dictionary<string, ExecuteBcfMethodResult> CrossSiteBcfCall(string handle, List<string> linkSiteIds, ExecuteBcfMethodParam callParams, LibDataAccess dataAccess = null)
        {
            if (string.IsNullOrEmpty(handle) || linkSiteIds == null || linkSiteIds.Count == 0 || callParams == null)
                throw new ArgumentNullException("handle、linkSiteIds、callParams", "检查参数时发现有空参数。");
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("该账户未登录。");
            }
            callParams.IsCrossSiteCall = true;
            callParams.UserId = libHandle.UserId;
            callParams.Token = GetToken(libHandle, 30 * 1000, dataAccess);
            if (string.IsNullOrEmpty(callParams.Token))
                throw new Exception("获取跨站访问令牌信息失败。");

            Dictionary<string, string> urls = GetSiteUrls(linkSiteIds);
            if (urls == null || urls.Count == 0)
                throw new Exception("查找到的站点Url为空。");
            Dictionary<string, ExecuteBcfMethodResult> dicResults = new Dictionary<string, ExecuteBcfMethodResult>();
            string errorInfo = string.Empty;
            List<Task> tasks = new List<Task>();
            foreach (string siteId in urls.Keys)
            {
                var url = urls[siteId];
                if (string.IsNullOrEmpty(url))
                    continue;
                url = string.Format("{0}/billSvc/invorkBcf", url);
                var task = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var sendObj = new { param = callParams };//参数名必须一致
                        dynamic ret = LibNetUtils.HttpPostCall<dynamic>(url, sendObj, out errorInfo, callParams.TimeoutMillSecs);
                        if (ret != null)
                        {
                            ExecuteBcfMethodResult result = JsonConvert.DeserializeObject<ExecuteBcfMethodResult>((string)ret.ExecuteBcfMethodResult);
                            lock (dicResults)
                            {
                                dicResults.Add(siteId, result);
                            }
                        }
                        else if (string.IsNullOrEmpty(errorInfo) == false)
                        {
                            ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();
                            result.Messages.Add(new LibMessage()
                            {
                                MessageKind = LibMessageKind.Error,
                                Message = string.Format("执行跨站请求出现异常:{0}", errorInfo)
                            });
                            lock (dicResults)
                            {
                                dicResults.Add(siteId, result);
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        LibCommUtils.AddOutput(@"Error\CrossSiteCall", string.Format("CheckSSOLoginState error:{0}\r\nStacktrace:{1}", exp.Message, exp.StackTrace));
                        ExecuteBcfMethodResult result = new ExecuteBcfMethodResult();
                        result.Messages.Add(new LibMessage()
                        {
                            MessageKind = LibMessageKind.Error,
                            Message = string.Format("执行跨站请求出现异常:{0}", exp.Message)
                        });
                        lock (dicResults)
                        {
                            dicResults.Add(siteId, result);
                        }
                    }
                });
                tasks.Add(task);
            }
            //循环检查等待所有task都执行完毕
            int waitMillSecs = callParams.TimeoutMillSecs + 1000;//多等一秒
            int waitCount = 0;
            //等待超时时间到来，或者全部已经执行完毕
            while (waitCount < waitMillSecs)
            {
                bool isAllFinished = tasks.All(t =>
                {
                    return t.IsCompleted;
                });
                if (isAllFinished)
                    break;
                waitCount += 100;
                Thread.Sleep(100);
            }
            return dicResults;
        }
        /// <summary>
        /// 向多个目标站点发起跨站Bcf方法调用请求。
        /// 此方法执行时将方法请求放入消息队列，放入成功即返回。
        /// 如果返回的字符串为空表示放入成功，否则表示错误提示。
        /// 如果参数有误会抛出异常
        /// </summary>
        /// <param name="handle">当前用户的标识Handle</param>
        /// <param name="linkSiteIds">目标站点代码列表</param>
        /// <param name="callParams">包含SSO令牌信息、请求方法、请求参数等callParams</param>
        /// <returns>如果返回的字符串为空表示放入队列成功，否则表示错误提示。</returns>
        public static string CrossSiteBcfCallQueue(string handle, List<string> linkSiteIds, ExecuteBcfMethodParam callParams)
        {
            return string.Empty;
        }
        /// <summary>
        /// 向多个目标站点发起跨站服务方法调用请求。此方法执行时会阻塞
        /// 如果参数有误会抛出异常
        /// 
        /// </summary>
        /// <param name="handle">当前用户的标识Handle</param>
        /// <param name="listSiteIds">目标站点代码列表</param>
        /// <param name="serviceUrl">服务地址（不包括基地址）</param>
        /// <param name="jsonData">要请求的方法参数的Json序列化数据</param>
        /// <returns>返回执行结果字典，键值为目标站点Id，值为执行结果</returns>
        public static Dictionary<string, object> CrossSiteSvcCall(string handle, List<string> listSiteIds, string serviceUrl, string jsonData)
        {
            return null;
        }
        /// <summary>
        /// 向多个目标站点发起跨站服务方法调用请求。
        /// 此方法执行时将方法请求放入消息队列，放入成功即返回。
        /// 如果返回的字符串为空表示放入成功，否则表示错误提示。
        /// 如果参数有误会抛出异常
        /// </summary>
        /// <param name="handle">当前用户的标识Handle</param>
        /// <param name="listSiteIds">目标站点代码列表</param>
        /// <param name="serviceUrl">服务地址（不包括基地址）</param>
        /// <param name="jsonData">要请求的方法参数的Json序列化数据</param>
        /// <returns>返回执行结果字典，键值为目标站点Id，值为执行结果</returns>
        public static string CrossSiteSvcCallQueue(string handle, List<string> listSiteIds, string serviceUrl, string jsonData)
        {
            return null;
        }
        /// <summary>
        /// 检查用户账户是否存在
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool CheckUserExist(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;
            try
            {
                LibDataAccess dataAccess = new LibDataAccess();
                int count = LibSysUtils.ToInt32(dataAccess.ExecuteScalar(string.Format("select count(*) from AXPUSER where USERID={0} AND ISUSE=1",
                      LibStringBuilder.GetQuotString(userId))));
                return count > 0;
            }
            catch(Exception exp)
            {
                LibCommUtils.AddOutput("CrossSiteCall", string.Format("error:{0}\r\nStacktrace:{1}", exp.Message, exp.StackTrace));
                return false;
            }
        }
        /// <summary>
        /// 获取指定用户的SSO令牌信息。如果本站点不是SSO管理站点，则从管理站点获取
        /// </summary>
        /// <param name="userHandle"></param>
        /// <param name="timeOutMs"></param>
        /// <param name="dataAccess">可选参数：数据库访问器。如果调用时使用了数据库事务，需要将开启了事务的数据库访问器传递进来，避免在本方法中查询数据库时因事务锁表而死锁。</param>
        /// <returns></returns>
        public static string GetToken(LibHandle userHandle, int timeOutMs = 30 * 1000, LibDataAccess dataAccess = null)
        {
            if (userHandle == null || string.IsNullOrEmpty(userHandle.UserId))
                return string.Empty;
            if (EnvProvider.Default.IsSSOManageSite)
            {
                return userHandle.GetToCheckToken();
            }
            if (string.IsNullOrEmpty(EnvProvider.Default.SSOManageSiteUrl))
                return string.Empty;
            try
            {
                string url = string.Format("{0}/sysSvc/getTokenByUserId", EnvProvider.Default.SSOManageSiteUrl);
                string password = string.Empty;
                string sql = string.Format("select USERPASSWORD from AXPUSER where USERID={0} And ISUSE=1", LibStringBuilder.GetQuotString(userHandle.UserId));
                if (dataAccess == null)
                    password = LibSysUtils.ToString((new LibDataAccess()).ExecuteScalar(sql));
                else
                    password = LibSysUtils.ToString(dataAccess.ExecuteScalar(sql));
                var postP = new
                {
                    userId = userHandle.UserId,
                    pwd = password
                };
                string errorInfo = string.Empty;
                dynamic result = LibNetUtils.HttpPostCall<dynamic>(url, postP, out errorInfo, timeOutMs);

                if (string.IsNullOrEmpty(errorInfo) == false || result == null)
                    return string.Empty;
                else
                {
                    return (string)result.GetTokenByUserIdResult;
                }
            }
            catch (Exception exp)
            {
                LibCommUtils.AddOutput("CrossSiteCall", string.Format("error:{0}\r\nStacktrace:{1}", exp.Message, exp.StackTrace));
                return string.Empty;
            }
        }
        /// <summary>
        /// 向SSOManage站点发起校验令牌信息的请求
        /// </summary>
        /// <param name="ssoInfo">包含令牌信息的参数</param>
        /// <param name="timeOutMs">超时时间，单位为毫秒，默认为5000毫秒</param>
        /// <returns>如果验证成功返回true，否则返回false</returns>
        public static bool CheckSSOLoginState(SSOInfo ssoInfo, int timeOutMs = 5000)
        {
            if (ssoInfo == null || string.IsNullOrEmpty(ssoInfo.UserId) || string.IsNullOrEmpty(ssoInfo.Token))
                return false;
            if (EnvProvider.Default.IsSSOManageSite)
            {
                LibHandle handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, ssoInfo.UserId);
                if (handle != null)
                {
                    return handle.Token == ssoInfo.Token;
                }
                return false;
            }
            if (string.IsNullOrEmpty(EnvProvider.Default.SSOManageSiteUrl))
                return false;
            try
            {
                string url = string.Format("{0}/sysSvc/CheckSSOLoginState", EnvProvider.Default.SSOManageSiteUrl);
                var postP = new
                {
                    ssoInfo = ssoInfo
                };
                string errorInfo = string.Empty;
                dynamic result = LibNetUtils.HttpPostCall<dynamic>(url, postP, out errorInfo, timeOutMs);

                if (string.IsNullOrEmpty(errorInfo) == false || result == null)
                    return false;
                else
                {
                    // 私钥解密              
                    string ret = LibRSAHelper.Decrypt((string)result.CheckSSOLoginStateResult);//对于直接返回基本类型的接口调用，结果会包装成方法名+Result
                    return ret == "0";
                }
            }
            catch (Exception exp)
            {
                LibCommUtils.AddOutput("CrossSiteCall", string.Format("CheckSSOLoginState error:{0}\r\nStacktrace:{1}", exp.Message, exp.StackTrace));
                return false;
            }

        }
        #region 同步配置与同步历史
        /// <summary>
        /// 填充指定用户对于指定功能模块数据的同步到其他站点的配置,如果没有则构造配置
        /// </summary>
        /// <param name="userHandle"></param>
        /// <param name="dt"></param>
        /// <param name="progId"></param>
        public static void FillSyncDataSetting(LibHandle userHandle, DataTable dt, string progId)
        {
            if (userHandle == null || dt == null || string.IsNullOrEmpty(progId) || ExistAxpSyncDataInfo == false || ExistLinkSiteTable == false)
                return;
            //构建当前用户对所有站点的同步选项
            Dictionary<string, LinkSiteInfo> linkSites = GetLinkSites(null, true);
            if (linkSites == null || linkSites.Count == 0)
                return;
            string siteIdStrs = string.Empty;
            foreach (string key in linkSites.Keys)
            {
                siteIdStrs += LibStringBuilder.GetQuotString(key) + ",";
            }
            siteIdStrs = siteIdStrs.Substring(0, siteIdStrs.Length - 1);
            try
            {
                string sql = string.Format("select A.*,C.PERSONNAME,D.SHORTNAME from AXPSYNCDATASETTING A " +
                       " left join AXPUSER B on A.USERID = B.USERID " +
                       " left join COMPERSON C on B.PERSONID = C.PERSONID " +
                       " left join AXPLINKSITE D on A.SITEID = D.SITEID " +
                       " where A.PROGID = {0} and A.USERID = {1} and A.SITEID in ({2})" +
                       " order by A.SITEID asc",// 站点代码升序
                       LibStringBuilder.GetQuotString(progId), LibStringBuilder.GetQuotString(userHandle.UserId), siteIdStrs);
                LibDataAccess dataAccess = new LibDataAccess();
                dataAccess.ExecuteDataTable(sql, dt);
                DataRow[] rows = null;
                if (dt.Rows.Count < linkSites.Count)
                {
                    if (linkSites != null && linkSites.Count > 0)
                    {
                        dt.BeginLoadData();
                        try
                        {
                            DataRow newRow = null;
                            foreach (string key in linkSites.Keys)
                            {
                                rows = dt.Select(string.Format("SITEID = {0}", LibStringBuilder.GetQuotString(key)));
                                if(rows==null|| rows.Length == 0)
                                {
                                    //对于没有配置的站点，增加默认配置 并保存到数据库
                                    newRow = dt.NewRow();
                                    newRow.BeginEdit();
                                    Guid guid = Guid.NewGuid();
                                    newRow["SETTINGID"] = guid.ToString();
                                    newRow["PROGID"] = progId;
                                    newRow["USERID"] = userHandle.UserId;
                                    newRow["ISSYNCTO"] = linkSites[key].IsSlave ? 1 : 0;//默认仅向子站点同步
                                    newRow["SITEID"] = key;
                                    newRow["SHORTNAME"] = linkSites[key].ShortName;
                                    newRow.EndEdit();
                                    dt.Rows.Add(newRow);

                                    string insertSql = string.Format("insert into AXPSYNCDATASETTING(SETTINGID,PROGID,USERID,ISSYNCTO,SITEID) "
                                                     + "values({0},{1},{2},{3},{4})",
                                                     LibStringBuilder.GetQuotString(guid.ToString()),
                                                     LibStringBuilder.GetQuotString(progId),
                                                     LibStringBuilder.GetQuotString(userHandle.UserId),
                                                     linkSites[key].IsSlave ? 1 : 0,//默认仅向子站点同步
                                                     LibStringBuilder.GetQuotString(key)
                                                     );
                                    dataAccess.ExecuteNonQuery(insertSql);
                                }
                            }
                        }
                        finally
                        {
                            dt.EndLoadData();
                        }
                    }
                }
                       
                dt.AcceptChanges();
            }
            catch (Exception exp)
            {
                LibCommUtils.AddOutput(@"Error\CrossSiteCall", string.Format("error:{0}\r\nStacktrace:{1}", exp.Message, exp.StackTrace));
            }
        }
        /// <summary>
        /// 将同步数据的配置更新到数据库
        /// </summary>
        /// <param name="dt"></param>
        public static void UpdateSyncDataSetting(DataTable dt)
        {
            if (dt == null || ExistAxpSyncDataInfo == false || ExistLinkSiteTable == false)
                return;
            List<string> sqlList = new List<string>();
            string setting = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                setting = LibSysUtils.ToString(row["SETTINGID"]);
                if (string.IsNullOrEmpty(setting))
                    continue;
                sqlList.Add(string.Format("update AXPSYNCDATASETTING set ISSYNCTO = {0} where SETTINGID = {1}",
                    LibSysUtils.ToBoolean(row["ISSYNCTO"]) ? 1 : 0, LibStringBuilder.GetQuotString(setting)));
            }
            LibDataAccess dataAccess = new LibDataAccess();
            dataAccess.ExecuteNonQuery(sqlList);
        }
        /// <summary>
        /// 填充对于指定功能模块数据的同步到其他站点的历史记录
        /// </summary>
        /// <param name="userHandle"></param>
        /// <param name="dt"></param>
        /// <param name="progId"></param>
        /// <param name="internalId">内码</param>
        public static void FillSyncDataHistory(LibHandle userHandle, DataTable dt, string progId, string internalId)
        {
            if (userHandle == null || dt == null || string.IsNullOrEmpty(progId) || ExistAxpSyncDataInfo == false || ExistLinkSiteTable == false)
                return;
            try
            {
                if (string.IsNullOrEmpty(internalId) == false)
                {
                    string sql = string.Format("select A.*,C.PERSONNAME,D.SHORTNAME from AXPSYNCDATAHISTORY A " +
                        " left join AXPUSER B on A.USERID = B.USERID " +
                        " left join COMPERSON C on B.PERSONID = C.PERSONID " +
                        " left join AXPLINKSITE D on A.SITEID = D.SITEID " +
                        " where A.PROGID = {0} and A.INTERNALID = {1} " +
                        " order by A.SYNCTIME desc",// 时间降序
                        LibStringBuilder.GetQuotString(progId), LibStringBuilder.GetQuotString(internalId));
                    LibDataAccess dataAccess = new LibDataAccess();
                    dataAccess.ExecuteDataTable(sql, dt, true, 200);//取前200条
                }

                dt.AcceptChanges();
            }
            catch (Exception exp)
            {
                LibCommUtils.AddOutput(@"Error\CrossSiteCall", string.Format("error:{0}\r\nStacktrace:{1}", exp.Message, exp.StackTrace));
            }
        }
        /// <summary>
        /// 添加同步信息到同步信息记录表中
        /// </summary>
        /// <param name="syncInfo"></param>
        /// <returns></returns>
        public static bool AddSyncDataRecord(SyncDataInfo syncInfo)
        {
            try
            {
                if (syncInfo == null||
                    string.IsNullOrEmpty(syncInfo.ProgId)||
                    string.IsNullOrEmpty(syncInfo.InternalId) ||
                    string.IsNullOrEmpty(syncInfo.UserId))
                    return false;
                string sql = string.Format("insert into AXPSYNCDATAHISTORY(INFOID,PROGID,INTERNALID,BILLNO,USERID,SITEID,SYNCTIME,SYNCOP,SYNCSTATE,SYNCINFO) "
                                                     + "values({0},{1},{2},{3},{4},{5},{6},{7},{8},{9})",
                                                     LibStringBuilder.GetQuotString(Guid.NewGuid().ToString()),
                                                     LibStringBuilder.GetQuotString(syncInfo.ProgId),
                                                     LibStringBuilder.GetQuotString(syncInfo.InternalId),
                                                     LibStringBuilder.GetQuotString(syncInfo.BillNo),
                                                     LibStringBuilder.GetQuotString(syncInfo.UserId),
                                                     LibStringBuilder.GetQuotString(syncInfo.SiteId),
                                                     LibDateUtils.DateTimeToLibDateTime(syncInfo.SyncTime),
                                                     (int)syncInfo.SyncOp,
                                                     (int)syncInfo.SyncState,
                                                      LibStringBuilder.GetQuotString(syncInfo.SyncInfo)
                                                     );
                LibDataAccess dataAccess = new LibDataAccess();
                int count = dataAccess.ExecuteNonQuery(sql);
                return count > 0;
            }
            catch (Exception exp)
            {
                LibCommUtils.AddOutput("CrossSiteCall", string.Format("error:{0}\r\nStacktrace:{1}", exp.Message, exp.StackTrace));
                return false;
            }
        }
        /// <summary>
        /// 根据信息唯一标识InfoId更新数据同步的历史结果
        /// </summary>
        /// <param name="syncInfo"></param>
        /// <returns></returns>
        public static bool UpdateSyncDataRecord(SyncDataInfo syncInfo)
        {
            try
            {
                if (syncInfo == null || string.IsNullOrEmpty(syncInfo.InfoId))
                    return false;
                string sql = string.Format("update AXPSYNCDATAHISTORY set SYNCSTATE={0},SYNCINFO={1} where INFOID={2}",
                    (int)syncInfo.SyncState,
                    LibStringBuilder.GetQuotString(syncInfo.SyncInfo),
                     LibStringBuilder.GetQuotString(syncInfo.InfoId)
                    );
                LibDataAccess dataAccess = new LibDataAccess();
                int count = dataAccess.ExecuteNonQuery(sql);
                return count > 0;
            }
            catch (Exception exp)
            {
                LibCommUtils.AddOutput("CrossSiteCall", string.Format("error:{0}\r\nStacktrace:{1}", exp.Message, exp.StackTrace));
                return false;
            }
        }
        #endregion
    }
}
