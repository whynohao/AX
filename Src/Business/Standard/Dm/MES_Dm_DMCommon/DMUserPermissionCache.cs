/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的用户权限缓存类。
 * 创建标识：Zhangkj 2016/12/14
 * 
************************************************************************/
using AxCRL.Core.Cache;
using AxCRL.Comm.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Jikon.MES_Dm.DMCommon
{
    public class DMUserPermissionCache: MemoryCacheRedis
    {
        private static DMUserPermissionCache _Default = null;
        private static object _LockObj = new object();
        private static ConcurrentDictionary<string, object> lockObjDic = null;

        public DMUserPermissionCache(string name, NameValueCollection config = null)
            : base(name)
        {
            LibHandleCache.NewLogin += LibHandleCache_NewLogin;//注册新用户登录的事件
        }

        private void LibHandleCache_NewLogin(AxCRL.Core.Comm.LibHandle newHandle)
        {
            if (newHandle == null || string.IsNullOrEmpty(newHandle.PersonId))
                return;
            lock (_LockObj)
            {
                this.Remove(newHandle.PersonId);               
            }
        }
        public static DMUserPermissionCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new DMUserPermissionCache("LibDMRolePermissionCache");
                            lockObjDic = new ConcurrentDictionary<string, object>();
                        }
                    }
                }
                return _Default;
            }
        }

        public object RemoveCacheItem(string personId)
        {
            return this.Remove(personId);
        }

        public DMUserPermission GetCacheItem(string personId)
        {
            DMUserPermission userPermission = null;
            object lockItem = lockObjDic.GetOrAdd(personId, new object());
            lock (lockItem)
            {
                userPermission = this.Get<DMUserPermission>(personId);
                if (userPermission == null)
                {
                    userPermission = new DMUserPermission(personId);
                    userPermission.RefreshUserPower();//获取用户操作权限
                    //CacheItemPolicy policy = new CacheItemPolicy();
                    //policy.SlidingExpiration = new TimeSpan(0, 30, 0); //30分钟内不访问自动剔除
                    this.Set(personId, userPermission, new TimeSpan(0, 30, 0));
                }
            }
            return userPermission;
        }
    }
}
