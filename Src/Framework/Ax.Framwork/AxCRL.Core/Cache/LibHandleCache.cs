using AxCRL.Comm.Utils;
using AxCRL.Core.Comm;
using AxCRL.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using AxCRL.Comm.Redis;
using System.Timers;
using AxCRL.Comm.Runtime;
using System.Threading;
using AxCRL.Data.SqlBuilder;
using System.Data;

namespace AxCRL.Core.Cache
{
    public class LibHandleCache : MemoryCacheRedis
    {
        #region Zhangkj20161219 增加新用户登录的委托事件
        public delegate void NewLoginCall(LibHandle newHandle);
        public static event NewLoginCall NewLogin;
        /// <summary>
        /// 触发新用户登录的事件处理
        /// </summary>
        /// <param name="newHandle"></param>
        protected static void RaiseNewLogin(LibHandle newHandle)
        {
            try
            {
                if (NewLogin != null)
                {
                    NewLogin(newHandle);
                }
            }
            catch (Exception exp)
            {
                //记录异常日志
                string info = string.Format("LibHandleCache.RaiseNewLogin Error:{0}", exp.ToString());
                string path = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output", "Error", string.Format("LibHandleCache_{0}.txt", DateTime.Now.Ticks));
                using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                    {
                        sw.Write(info);
                    }
                }
            }
        }
        #endregion

        private static string _SystemHandle = string.Empty;
        private static int _MaxUserCount = -1;
        private static LibHandleCache _Default = null;
        private static object _LockObj = new object();
        /// <summary>
        /// 检查Token有效性的计时器
        /// </summary>
        private System.Timers.Timer checkTokenValidTimer = new System.Timers.Timer(5 * 1000);

        public LibHandleCache(string name, NameValueCollection config = null)
            : base(name)
        {
            if (EnvProvider.Default.IsSSOManageSite)
                checkTokenValidTimer.Elapsed += CheckTokenValidTimer_Elapsed;
        }
        /// <summary>
        /// 检查Token有效期的定时执行方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckTokenValidTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {                
                IEnumerator<string> enumerator = _Default.GetKeys();
                DateTime now = DateTime.Now;
                while (enumerator.MoveNext())
                {
                    LibHandle handle = this.Get<LibHandle>(enumerator.Current);
                    if (handle != null)
                    {
                        TimeSpan ts = now - handle.TokenValidTime;
                        if (ts.TotalMinutes > EnvProvider.Default.TokenValidMinutes)
                        {
                            handle.SetNewToken();
                            this.Set(handle.Handle, handle);//修改Token后重新放入Redis缓存
                        }                            
                    }
                    Thread.Sleep(30);
                }
            }
            catch
            {
                //to do log
            }
        }

        public int MaxUserCount
        {
            get { return LibHandleCache._MaxUserCount; }
        }

        public static LibHandleCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibHandleCache("LibHandleCache");
                            LibHandle systemHandle = new LibHandle(LibHandeleType.PC, "System", "System", "System", string.Empty);
                            _SystemHandle = systemHandle.Handle;
                            _Default.Set(_SystemHandle, systemHandle); //系统handle重来不过期
                            LibDataAccess dataAccess = new LibDataAccess();
                            _MaxUserCount = LibSysUtils.ToInt32(dataAccess.ExecuteScalar("Select MAXUSERCOUNT From AXPPURCHASESPEC"));
                        }
                    }
                }
                return _Default;
            }
        }
        public int GetUserCount()
        {
            int countUser = 1;
            AxCRL.Data.SqlBuilder.SqlBuilder builder = new AxCRL.Data.SqlBuilder.SqlBuilder("axp.User");
            string sql = builder.GetQuerySql(0, "A.USERID,A.PERSONID,A.PERSONNAME,A.ROLEID,A.WALLPAPER,A.WALLPAPERSTRETCH",
                string.Format("A.ISUSE={0}", 1));
            LibDataAccess dataAccess = new LibDataAccess();

            System.Data.DataTable tbUser = dataAccess.ExecuteDataSet(sql).Tables[0];

            IEnumerator<string> enumerator = _Default.GetKeys();
            Dictionary<string, bool> dicUser = new Dictionary<string, bool>();
            while (enumerator.MoveNext())
            {
                LibHandle handle = this.Get<LibHandle>(enumerator.Current);
                if (handle != null)
                {
                    if (!String.IsNullOrWhiteSpace(handle.UserId))
                        dicUser[handle.UserId] = true;
                }
            }

            foreach (KeyValuePair<string, bool> kvp in dicUser)
            {
                foreach (System.Data.DataRow dr in tbUser.Rows)
                {
                    if (kvp.Key.Equals(dr["USERID"].ToString(), StringComparison.CurrentCultureIgnoreCase))
                        ++countUser;
                }
            }
            return countUser;
        }
        public LibHandle GetSystemHandle()
        {
            return GetHandle(_SystemHandle, LibHandeleType.PC, "System", "System", "System", string.Empty);
        }


        public LibHandle IsExistsHandle(LibHandeleType handleType, string userId, bool checkHandleType = true)
        {
            LibHandle destHandle = null;
            IEnumerator<string> enumerator = _Default.GetKeys();
            while (enumerator.MoveNext())
            {
                LibHandle handle = this.Get<LibHandle>(enumerator.Current);
                if (handle != null && string.Compare(userId, handle.UserId, false) == 0
                    &&
                    (checkHandleType == false || handleType == handle.Type)
                    )
                {
                    destHandle = handle;
                    break;
                }
            }
            return destHandle;
        }

        public LibHandle GetCurrentHandle(string handle)
        {
            LibHandle libHandle = null;
            if (!string.IsNullOrEmpty(handle))
            {
                libHandle = this.Get<LibHandle>(handle);
            }
            return libHandle;
        }
        public LibHandle GetHandle(string handle, LibHandeleType handleType, string userId, string personId, string personName, string roleId)
        {
            return GetHandle(handle, handleType, userId, personId, personName, roleId, string.Empty);
        }
        public LibHandle GetHandle(string handle, LibHandeleType handleType, string userId, string personId, string personName, string roleId, string loginIp)
        {
            LibHandle libHandle = null;
            if (!string.IsNullOrEmpty(handle))
            {
                libHandle = this.Get<LibHandle>(handle);
            }
            if (libHandle == null && !string.IsNullOrEmpty(userId))
            {
                //CacheItemPolicy policy = new CacheItemPolicy();
                //policy.SlidingExpiration = ; //20分钟内不访问自动剔除,前端每15分钟定时取一下handle表示在用
                libHandle = new LibHandle(handleType, userId, personId, personName, roleId);

                libHandle.LogIp = loginIp;//Zhangkj20161219 增加LoginIp

                //policy.RemovedCallback += CacheEntryRemovedCallback;
                this.Set(libHandle.Handle, libHandle);

                RaiseNewLogin(libHandle);
            }
            return libHandle;
        }
        /// <summary>
        /// 构造跨站点调用的临时登录
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public LibHandle GetCrossCallHandle(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;
            LibHandle handle = new LibHandle(LibHandeleType.CrossSiteCall, userId, "");  
            SqlBuilder builder = new SqlBuilder("axp.User");
            string sql = builder.GetQuerySql(0, "A.PERSONID,A.PERSONNAME,A.ROLEID,A.WALLPAPER,A.WALLPAPERSTRETCH", string.Format("A.USERID={0} And A.ISUSE=1", LibStringBuilder.GetQuotString(userId)));
            LibDataAccess dataAccess = new LibDataAccess();
            bool exist = false;      
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                if (reader.Read())
                {
                    handle.PersonId = LibSysUtils.ToString(reader[0]);
                    handle.PersonName = LibSysUtils.ToString(reader[1]);
                    handle.RoleId = LibSysUtils.ToString(reader[2]);
                    exist = true;
                }
            }
            if (exist)
            {
                this.Set(handle.Handle, handle);
                return handle;
            }                
            else
                return null;
        }        

        //public void CacheEntryRemovedCallback(CacheEntryRemovedArguments arguments)
        //{
        //    string path = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output", "Error", "Handle", string.Format("{0}.txt", DateTime.Now.Ticks));
        //    using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
        //    {
        //        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
        //        {
        //            sw.Write(string.Format("handle:{0},remove time:{1}", arguments.CacheItem.Key, LibDateUtils.GetCurrentDateTime()));
        //        }
        //    }
        //}

        public bool RemoveHandle(string handle)
        {
            return this.Remove(handle);
        }

        public override bool Remove(string key, string regionName = null)
        {
            LibHandle handle = this.Get<LibHandle>(key);
            if (base.Remove(key))
            {
                try
                {
                    LibDataAccess dataAccess = new LibDataAccess();
                    dataAccess.ExecuteStoredProcedure("axpInsertUserLogin", handle.UserId, LibDateUtils.DateTimeToLibDateTime(handle.CreateTime), handle.Type, LibDateUtils.GetCurrentDateTime());
                }
                catch
                {
                    //即使有错也不抛出
                }
                return true;
            }
            return false;
        }
    }
}
