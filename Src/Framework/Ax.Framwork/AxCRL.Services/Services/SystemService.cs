using AxCRL.Comm.Define;
using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Data;
using AxCRL.Data.SqlBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel.Activation;
using System.IO;
using AxCRL.Bcf;
using System.Threading;
using AxCRL.Core.Mail;
using AxCRL.Template.DataSource;
using AxCRL.Template;
using AxCRL.Services.Inspector;
using AxCRL.Comm.Entity;

namespace AxCRL.Services
{
    [CrossDomainInspectorBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class SystemService : ISystemService
    {
        public string GetJsPath()
        {
            ProgIdConfigListing progIdConfigListing = ProgIdConfigListingManager.GetProgIdListing(EnvProvider.Default.MainPath);
            return progIdConfigListing.GetMap();
        }


        [ParameterOperatorBehavior]
        public LoginInfo Login(string userId, string password, bool quitOther = false)
        {
            LoginInfo loginInfo = new LoginInfo();
            SqlBuilder builder = new SqlBuilder("axp.User");
            string sql = builder.GetQuerySql(0, "A.PERSONID,A.PERSONNAME,A.ROLEID,A.WALLPAPER,A.WALLPAPERSTRETCH", string.Format("A.USERID={0} And A.USERPASSWORD={1} And A.ISUSE=1", LibStringBuilder.GetQuotString(userId), LibStringBuilder.GetQuotString(password)));
            LibDataAccess dataAccess = new LibDataAccess();
            string roleId = string.Empty;
            bool exists = false;
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                if (reader.Read())
                {
                    loginInfo.PersonId = LibSysUtils.ToString(reader[0]);
                    loginInfo.PersonName = LibSysUtils.ToString(reader[1]);
                    roleId = LibSysUtils.ToString(reader[2]);
                    loginInfo.Wallpaper = LibSysUtils.ToString(reader[3]);
                    loginInfo.Stretch = LibSysUtils.ToBoolean(reader[4]);
                    exists = true;
                }
            }
            if (exists)
            {
                LibHandle handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
                if (handle != null)
                {
                    if (quitOther)
                        LibHandleCache.Default.RemoveHandle(handle.Handle);
                    else
                        loginInfo.IsUsed = true;
                }
                if (!loginInfo.IsUsed)
                {
                    long currentCount = LibHandleCache.Default.GetCount();
                    long maxUserCount = (long)LibHandleCache.Default.MaxUserCount;
                    if (maxUserCount != -1 && maxUserCount < currentCount)
                    {
                        loginInfo.IsOverUser = true;
                    }
                    else
                    {
                        string loginIp = string.Empty;
                        //Zhangkj20161219 增加LoginIp
                        System.ServiceModel.OperationContext context = System.ServiceModel.OperationContext.Current;
                        //对于非WCF的访问context为null
                        if (context != null)
                        {
                            System.ServiceModel.Channels.MessageProperties properties = context.IncomingMessageProperties;
                            System.ServiceModel.Channels.RemoteEndpointMessageProperty endpoint = properties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name] as System.ServiceModel.Channels.RemoteEndpointMessageProperty;
                            loginIp = endpoint.Address + ":" + endpoint.Port.ToString();
                        }
                        //创建新的Handle                        
                        handle = LibHandleCache.Default.GetHandle(string.Empty, LibHandeleType.PC, userId, loginInfo.PersonId, loginInfo.PersonName, roleId, loginIp);
                        if (handle != null)
                        {
                            loginInfo.Handle = handle.Handle;
                        }
                    }
                }

            }
            return loginInfo;
        }       
        /// <summary>
        /// 单点登录。
        /// 1.检查当前本站点系统账户中是否存在userId，如果否则不予登录
        /// 2.使用userId、loginToken等访问sso管理站点的CheckSSOLoginState方法检验是否已经SSO登录过。
        ///   如果未登录过则不予登录。
        /// 3.判断当前站点中userId是否已经登录过，如果是则使用对应的LibHandle信息返回，否则则新构造LibHande并返回。
        /// </summary>
        ///<param name="ssoInfo">单点登录信息</param>
        /// <returns></returns>
        public LoginInfo SSOLogin(SSOInfo ssoInfo)
        {
            LoginInfo loginInfo = new LoginInfo() { IsUsed = true, IsOverUser = false};

            SqlBuilder builder = new SqlBuilder("axp.User");
            string sql = builder.GetQuerySql(0, "A.PERSONID,A.PERSONNAME,A.ROLEID,A.WALLPAPER,A.WALLPAPERSTRETCH", string.Format("A.USERID={0} And A.ISUSE=1", LibStringBuilder.GetQuotString(ssoInfo.UserId)));
            LibDataAccess dataAccess = new LibDataAccess();
            string roleId = string.Empty;
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                if (reader.Read())
                {
                    loginInfo.PersonId = LibSysUtils.ToString(reader[0]);
                    loginInfo.PersonName = LibSysUtils.ToString(reader[1]);
                    roleId = LibSysUtils.ToString(reader[2]);
                    loginInfo.Wallpaper = LibSysUtils.ToString(reader[3]);
                    loginInfo.Stretch = LibSysUtils.ToBoolean(reader[4]);
                    loginInfo.IsUsed = false;
                }
                else
                {
                    return loginInfo;
                }
            }
            if (CheckToken(ssoInfo))
            {
                // 授权成功
                LibHandle handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, ssoInfo.UserId);
                if (handle == null)
                    handle = LibHandleCache.Default.GetHandle(string.Empty, LibHandeleType.PC, ssoInfo.UserId, loginInfo.PersonId, loginInfo.PersonName, roleId);
                loginInfo.Handle = handle.Handle;
                return loginInfo;
            }
            return loginInfo;
        }

        public LoginInfo AppLogin(string userId, string password, string clientId, int clientType, bool quitOther = false)
        {
            LoginInfo loginInfo = new LoginInfo();
            //检查是否具有 AXPUSERAPP数据表，用于判定是否支持移动端App登录
            LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel("axp.User");
            bool hasAXPUSERAPP = false;
            if (sqlModel != null && sqlModel.Tables.Count > 1 && sqlModel.Tables[1].TableName.Equals("AXPUSERAPP"))
            {
                hasAXPUSERAPP = true;
            }
            if (hasAXPUSERAPP == false)
            {
                return loginInfo;//如果没有需要的相关字段则直接返回
            }
           
            SqlBuilder builder = new SqlBuilder("axp.User");
            string sql = string.Format(@"SELECT 
                                        A.PERSONID,
                                        A.ROLEID,
                                        A.WALLPAPER,A.WALLPAPERSTRETCH,B.PERSONNAME,B.PHONENO,B.CORNET,B.HEADPORTRAIT,B.MAIL   
                                        FROM AXPUSER A LEFT JOIN COMPERSON B ON B.PERSONID=A.PERSONID   
                                        WHERE A.USERID={0} and A.USERPASSWORD={1} AND A.ISUSE=1", LibStringBuilder.GetQuotString(userId), LibStringBuilder.GetQuotString(password));
            //builder.GetQuerySql(0, "A.PERSONID,A.PERSONNAME,A.PHONENO,A.CORNET,A.HEADPORTRAIT,A.MAIL,A.ROLEID,A.WALLPAPER,A.WALLPAPERSTRETCH", string.Format("A.USERID={0} And A.USERPASSWORD={1} And A.ISUSE=1", LibStringBuilder.GetQuotString(userId), LibStringBuilder.GetQuotString(password)));
            LibDataAccess dataAccess = new LibDataAccess();
            string roleId = string.Empty;
            bool exists = false;
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                if (reader.Read())
                {
                    loginInfo.PersonId = LibSysUtils.ToString(reader["PERSONID"]);
                    loginInfo.PersonName = LibSysUtils.ToString(reader["PERSONNAME"]);
                    roleId = LibSysUtils.ToString(reader["ROLEID"]);
                    loginInfo.Wallpaper = LibSysUtils.ToString(reader["WALLPAPER"]);
                    loginInfo.Stretch = LibSysUtils.ToBoolean(reader["WALLPAPERSTRETCH"]);
                    //20170214 施卢威 增加头像 Email 短号信息 
                    loginInfo.Headportrait = LibSysUtils.ToString(reader["HEADPORTRAIT"]);
                    loginInfo.UserEMail = LibSysUtils.ToString(reader["MAIL"]);
                    loginInfo.Cornet = LibSysUtils.ToString(reader["CORNET"]);
                    loginInfo.UserPhone = LibSysUtils.ToString(reader["PHONENO"]);
                    exists = true;
                }
            }
            if (exists)
            {
                #region 帐号与登录设备关联
                string appSql = string.Empty;
                //查询帐号是否已有设备标识
                int isAPPClient = LibSysUtils.ToInt32(dataAccess.ExecuteScalar(string.Format("SELECT COUNT(*) from AXPUSERAPP WHERE USERID = '{0}' and CLIENTTYPE={1}", userId, clientType)));
                if (isAPPClient > 0)
                {
                    //更新设备标识信息
                    appSql = string.Format("UPDATE AXPUSERAPP SET  CLIENTID={1}    WHERE USERID={0} AND CLIENTTYPE={2}", LibStringBuilder.GetQuotString(userId), LibStringBuilder.GetQuotString(clientId), clientType);
                }
                else
                {
                    int curMaxRowId = LibSysUtils.ToInt32(dataAccess.ExecuteScalar(string.Format("SELECT Max(ROW_ID) from AXPUSERAPP WHERE USERID='{0}'", userId)));
                    //插入账户对应的App设备标识信息。
                    appSql = string.Format("insert into AXPUSERAPP(USERID,ROW_ID,ROWNO,CLIENTTYPE,CLIENTID) values('{0}',{1},{2},{3},'{4}')",
                                        userId, curMaxRowId + 1, curMaxRowId + 1, clientType, clientId, 1);
                }
                dataAccess.ExecuteNonQuery(appSql);

                #endregion

                LibHandle handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
                if (handle != null)
                {
                    if (quitOther)
                        LibHandleCache.Default.RemoveHandle(handle.Handle);
                    else
                        loginInfo.IsUsed = true;
                }
                if (!loginInfo.IsUsed)
                {
                    long currentCount = LibHandleCache.Default.GetCount();
                    long maxUserCount = (long)LibHandleCache.Default.MaxUserCount;
                    if (maxUserCount != -1 && maxUserCount < currentCount)
                    {
                        loginInfo.IsOverUser = true;
                    }
                    else
                    {
                        string loginIp = string.Empty;
                        //Zhangkj20161219 增加LoginIp
                        System.ServiceModel.OperationContext context = System.ServiceModel.OperationContext.Current;
                        //对于非WCF的访问context为null
                        if (context != null)
                        {
                            System.ServiceModel.Channels.MessageProperties properties = context.IncomingMessageProperties;
                            System.ServiceModel.Channels.RemoteEndpointMessageProperty endpoint = properties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name] as System.ServiceModel.Channels.RemoteEndpointMessageProperty;
                            loginIp = endpoint.Address + ":" + endpoint.Port.ToString();
                            handle = LibHandleCache.Default.GetHandle(string.Empty, LibHandeleType.PC, userId, loginInfo.PersonId, loginInfo.PersonName, roleId, loginIp);
                        }
                        else
                        {
                            handle = LibHandleCache.Default.GetHandle(string.Empty, LibHandeleType.PC, userId, loginInfo.PersonId, loginInfo.PersonName, roleId);
                        }
                        //创建新的Handle                        

                        if (handle != null)
                        {
                            loginInfo.Handle = handle.Handle;
                        }
                    }
                }

            }
            return loginInfo;
        }

        public void CheckLogin(string handle)
        {
            LibHandleCache.Default.GetHandle(handle, LibHandeleType.PC, string.Empty, string.Empty, string.Empty, string.Empty);
        }


        public void LoginOut(string handle)
        {
            LibHandleCache.Default.RemoveHandle(handle);
        }


        public SetPwdResult SetPassword(string handle, string oldPwd, string newPwd)
        {
            SetPwdResult result = new SetPwdResult();
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            string sql = string.Format("select USERID from AXPUSER where USERID={0} and USERPASSWORD={1}",
                LibStringBuilder.GetQuotString(libHandle.UserId), LibStringBuilder.GetQuotString(oldPwd));
            LibDataAccess dataAccess = new LibDataAccess();
            string userId = LibSysUtils.ToString(dataAccess.ExecuteScalar(sql));
            if (string.IsNullOrEmpty(userId))
            {
                result.Msg = "输入的旧密码与系统不匹配。";
                result.Success = false;
            }
            else
            {
                dataAccess.ExecuteNonQuery(string.Format("update AXPUSER set USERPASSWORD={2} where USERID={0} and USERPASSWORD={1}",
                    LibStringBuilder.GetQuotString(libHandle.UserId), LibStringBuilder.GetQuotString(oldPwd),
                    LibStringBuilder.GetQuotString(newPwd)));
                result.Success = true;
            }
            return result;
        }


        public string GetVisualHostName()
        {
            string ret = string.Empty;
            if (!string.IsNullOrEmpty(EnvProvider.Default.VisualHostName))
                ret = string.Format("{0}:{1}", EnvProvider.Default.VisualHostName, EnvProvider.Default.VisualPort);
            return ret;
        }

        public void SetWallpaper(string handle, string wallpaper, bool stretch)
        {
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            LibDataAccess dataAccess = new LibDataAccess();
            dataAccess.ExecuteNonQuery(string.Format("update AXPUSER set WALLPAPER={0},WALLPAPERSTRETCH={1} where USERID={2}", LibStringBuilder.GetQuotString(wallpaper), stretch ? 1 : 0, LibStringBuilder.GetQuotString(libHandle.UserId)));
        }


        public List<string> GetWallpapers(string handle)
        {
            List<string> wallpapers = new List<string>();
            LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(handle) as LibHandle;
            if (libHandle == null)
            {
                throw new Exception("用户句柄无效。");
            }
            string path = Path.Combine(EnvProvider.Default.RuningPath, "Wallpapers", libHandle.UserId);
            if (!Directory.Exists(path))
            {
                path = Path.Combine(EnvProvider.Default.RuningPath, "Wallpapers", "admin");
                if (!Directory.Exists(path))
                    path = string.Empty;
            }
            if (!string.IsNullOrEmpty(path))
            {
                DirectoryInfo info = new DirectoryInfo(path);
                FileInfo[] list = new string[] { "*.jpg", "*.png", "*.gif" }
                                 .SelectMany(i => info.GetFiles(i))
                                 .Distinct().ToArray();
                foreach (var item in list)
                {
                    wallpapers.Add(item.Name);
                }
            }
            return wallpapers;
        }


        public List<DeptInfo> GetDept()
        {
            List<DeptInfo> list = new List<DeptInfo>();
            LibDataAccess dataAccess = new LibDataAccess();
            using (IDataReader reader = dataAccess.ExecuteDataReader("select DEPTID,DEPTNAME from COMDEPT"))
            {
                while (reader.Read())
                {
                    list.Add(new DeptInfo()
                    {
                        DeptId = LibSysUtils.ToString(reader["DEPTID"]),
                        DeptName = LibSysUtils.ToString(reader["DEPTNAME"])
                    });
                }
            }
            return list;
        }


        public string Register(RegisterInfo info)
        {
            string error = string.Empty;
            LibDataAccess dataAccess = new LibDataAccess();
            string userId = LibSysUtils.ToString(dataAccess.ExecuteScalar(string.Format("select USERID from AXPUSER where USERID={0}",
                    LibStringBuilder.GetQuotString(info.inputId))));
            if (string.IsNullOrEmpty(userId))
            {
                string personId = LibSysUtils.ToString(dataAccess.ExecuteScalar(string.Format("select PERSONID from COMPERSON where PERSONNAME={0} and DEPTID={1}",
                    LibStringBuilder.GetQuotString(info.inputName), LibStringBuilder.GetQuotString(info.inputDept))));
                //如果遇到同部门同名的情况。建议客户手动创建人员主数据。
                if (string.IsNullOrEmpty(personId))
                {
                    LibBcfData bcfData = (LibBcfData)LibBcfSystem.Default.GetBcfInstance("com.Person");
                    DataSet dataSet = bcfData.AddNew(null);
                    DataRow masterRow = dataSet.Tables[0].Rows[0];
                    masterRow.BeginEdit();
                    try
                    {
                        if (string.IsNullOrEmpty(LibSysUtils.ToString(masterRow["PERSONID"])))
                        {
                            masterRow["PERSONID"] = personId = LibCommUtils.GetInternalId().ToString();
                        }
                        masterRow["PERSONNAME"] = info.inputName;
                        masterRow["GENDER"] = info.gender;
                        masterRow["DEPTID"] = info.inputDept;
                        masterRow["MAIL"] = info.inputEmail;
                        masterRow["PHONENO"] = info.inputPhone;
                    }
                    finally
                    {
                        masterRow.EndEdit();
                    }
                    dataSet = bcfData.InnerSave(BillAction.AddNew, new object[] { personId }, dataSet);
                    personId = LibSysUtils.ToString(dataSet.Tables[0].Rows[0]["PERSONID"]);
                }
                if (!string.IsNullOrEmpty(personId))
                {
                    string sql = string.Format("insert into AXPUSER(USERID,USERPASSWORD,PERSONID,ISUSE) values({0},{1},{2},0)",
                        LibStringBuilder.GetQuotString(info.inputId), LibStringBuilder.GetQuotString(info.inputPassword1),
                        LibStringBuilder.GetQuotString(personId));
                    dataAccess.ExecuteNonQuery(sql);
                }
            }
            else
            {
                error = "账号已注册";
            }
            return error;
        }


        public string RecoverPassword(string userId)
        {
            string error = string.Empty;
            if (string.Compare(userId, "admin", true) == 0)
            {
                error = "账户admin不允许重置密码";
                return error;
            }
            LibDataAccess dataAccess = new LibDataAccess();
            string id = string.Empty, personId = string.Empty, email = string.Empty;
            using (IDataReader reader = dataAccess.ExecuteDataReader(string.Format("select A.USERID,A.PERSONID,B.MAIL from AXPUSER A inner join COMPERSON B on B.PERSONID=A.PERSONID where A.USERID={0}",
                    LibStringBuilder.GetQuotString(userId))))
            {
                if (reader.Read())
                {
                    id = LibSysUtils.ToString(reader[0]);
                    personId = LibSysUtils.ToString(reader[1]);
                    email = LibSysUtils.ToString(reader[2]);
                }
            }
            if (string.IsNullOrEmpty(id))
            {
                error = "该账号未注册";
            }
            else if (string.IsNullOrEmpty(email))
            {
                error = "该账号未关联邮箱，请联系管理员";
            }
            else
            {
                Random random = new Random();
                string pw = string.Format("{0}{1}", userId, random.Next(1000, 9999));
                dataAccess.ExecuteNonQuery(string.Format("update AXPUSER set USERPASSWORD={0} where USERID={1}", LibStringBuilder.GetQuotString(pw), LibStringBuilder.GetQuotString(userId)));
                List<AxCRL.Core.Mail.LibMailParam> list = new List<AxCRL.Core.Mail.LibMailParam>();
                AxCRL.Core.Mail.LibMailParam param = new AxCRL.Core.Mail.LibMailParam();
                param.Content = string.Format("您的账号 {0} 密码已重置。新密码为{1}", userId, pw);
                param.MailKind = AxCRL.Core.Mail.LibMailKind.Info;
                param.Subject = "智慧工厂账号密码重置";
                param.To.Add(LibSysUtils.ToString(personId));
                list.Add(param);
                ThreadPool.QueueUserWorkItem(LibMailHelper.SendMail, list);
            }
            return error;
        }
        /// <summary>
        /// 检查指定账户是否已以某个loginToken形式登录(SSO管理站点才有用)
        /// </summary>
        /// <param name="ssoInfo">包含用户账号、访问令牌等</param>
        /// <returns></returns>
        public string CheckSSOLoginState(SSOInfo ssoInfo)
        {
            string ret = "101";
            if (!EnvProvider.Default.IsSSOManageSite)
                ret = "100";
            else
            {
                LibHandle handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, ssoInfo.UserId);
                if (handle != null && handle.Token == ssoInfo.Token)
                {
                    ret = "0";
                }
            }
            // 公钥加密
            ret = LibRSAHelper.Encrypt(ret);
            return ret;
        }
        /// <summary>
        /// 在跨站登陆前，访问获取Token
        /// </summary>
        /// <param name="userHandle">当前用户句柄</param>
        /// <returns></returns>
        public string GetToken(string userHandle)
        {
            LibHandle handle = LibHandleCache.Default.GetCurrentHandle(userHandle);
            if (handle == null)
            {
                return string.Empty;
            }

            if (EnvProvider.Default.IsSSOManageSite)
            {
                return handle.GetToCheckToken();
            }
            else
            {
                try
                {
                    string sql = string.Format("select USERPASSWORD from AXPUSER where USERID = {0}", LibStringBuilder.GetQuotString(handle.UserId));
                    LibDataAccess dataAccess = new LibDataAccess();
                    var pwd = dataAccess.ExecuteScalar(sql);
                    if (pwd == null)
                    {
                        return string.Empty;
                    }
                    string baseUrl = EnvProvider.Default.SSOManageSiteUrl;
                    string url = baseUrl + "/sysSvc/getTokenByUserId";
                    string errorInfo = "";
                    dynamic obj = LibNetUtils.HttpPostCall<dynamic>(url, new { userId = handle.UserId, pwd = pwd.ToString() }, out errorInfo);
                    if (string.IsNullOrEmpty(errorInfo))
                    {
                        string token = obj.GetTokenByUserIdResult.Value;
                        return token;
                    }
                    return string.Empty;
                }
                catch (Exception)
                {
                    return string.Empty;
                    //throw;
                }
            }
        }

        private bool CheckToken(SSOInfo ssoInfo)
        {
            if (EnvProvider.Default.IsSSOManageSite)
            {
                LibHandle handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, ssoInfo.UserId);
                if (handle != null)
                {
                    if (handle.Token == ssoInfo.Token)
                    {
                        return true;
                    }
                }
            }
            else
            {
                try
                {
                    string baseUrl = EnvProvider.Default.SSOManageSiteUrl;
                    string url = baseUrl + "/sysSvc/checkSSOLoginState";
                    string errorInfo = "";
                    dynamic obj = LibNetUtils.HttpPostCall<dynamic>(url, new { ssoInfo = ssoInfo}, out errorInfo);
                    if (string.IsNullOrEmpty(errorInfo))
                    {
                        string ret = LibRSAHelper.Decrypt(obj.CheckSSOLoginStateResult.Value);
                        if (ret == "0")
                        {
                            return true;
                        }
                    }
                }
                catch (Exception)
                {
                    //throw;
                }
            }
            return false;
        }

        /// <summary>
        /// 一般站点登录SSO管理站点(SSO管理站点才有用，前端不需要访问）
        /// </summary>
        /// <param name="userId">用户名</param>
        /// <param name="pwd">密码</param>
        /// <returns></returns>
        public string GetTokenByUserId(string userId, string pwd)
        {
            if (!EnvProvider.Default.IsSSOManageSite)
                return string.Empty;
            LoginInfo loginInfo = new LoginInfo();
            SqlBuilder builder = new SqlBuilder("axp.User");
            string sql = builder.GetQuerySql(0, "A.PERSONID,A.PERSONNAME,A.ROLEID,A.WALLPAPER,A.WALLPAPERSTRETCH", string.Format("A.USERID={0} And A.USERPASSWORD={1} And A.ISUSE=1", LibStringBuilder.GetQuotString(userId), LibStringBuilder.GetQuotString(pwd)));
            LibDataAccess dataAccess = new LibDataAccess();
            string roleId = string.Empty;
            bool exists = false;
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                if (reader.Read())
                {
                    loginInfo.PersonId = LibSysUtils.ToString(reader[0]);
                    loginInfo.PersonName = LibSysUtils.ToString(reader[1]);
                    roleId = LibSysUtils.ToString(reader[2]);
                    loginInfo.Wallpaper = LibSysUtils.ToString(reader[3]);
                    loginInfo.Stretch = LibSysUtils.ToBoolean(reader[4]);
                    exists = true;
                }
            }
            if (exists)
            {
                LibHandle handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
                if (handle != null)
                {
                    return handle.GetToCheckToken();
                }
                handle = LibHandleCache.Default.GetHandle(string.Empty, LibHandeleType.PC, userId, loginInfo.PersonId, loginInfo.PersonName, roleId);
                return handle.Token;
            }
            return string.Empty;
        }
        /// <summary>
        /// 获取用户可访问的站点
        /// </summary>
        /// <param name="userHandle"></param>
        /// <returns></returns>
        public List<LinkSiteInfo> GetLinkSites(string userHandle)
        {
            // 判断userHandle存不存在
            LibHandle handle = LibHandleCache.Default.GetCurrentHandle(userHandle);
            if (handle == null)
            {
                throw new Exception("用户句柄无效");
            }
            List<LinkSiteInfo> linkSiteInfoList = new List<LinkSiteInfo>();
            string sql = string.Format("select B.SITEID, B.SITENAME, B.SHORTNAME, B.SITEURL, B.SVCURL, B.ISSLAVE,B.ISSENDTO from AXPUSERSITE A join AXPLINKSITE B on A.SITEID = B.SITEID where A.USERID = {0}", LibStringBuilder.GetQuotString(handle.UserId));
            LibDataAccess dataAccess = new LibDataAccess();
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    var siteInfo = new LinkSiteInfo()
                    {
                        SiteId = LibSysUtils.ToString(reader["SITEID"]),
                        SiteName = LibSysUtils.ToString(reader["SITENAME"]),
                        ShortName = LibSysUtils.ToString(reader["SHORTNAME"]),
                        SiteUrl = LibSysUtils.ToString(reader["SITEURL"]),
                        SvcUrl = LibSysUtils.ToString(reader["SVCURL"]),
                        IsSlave = LibSysUtils.ToBoolean(reader["ISSLAVE"]),
                        IsSendTo = LibSysUtils.ToBoolean(reader["ISSENDTO"]),
                    };
                    linkSiteInfoList.Add(siteInfo);
                }
            }
            return linkSiteInfoList;
        }
    }
}
