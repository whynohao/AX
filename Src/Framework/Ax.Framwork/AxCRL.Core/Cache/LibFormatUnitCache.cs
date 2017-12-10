using AxCRL.Comm.Utils;
using AxCRL.Data;
using AxCRL.Comm.Redis;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Cache
{
    public class LibFormatUnitCache : MemoryCacheRedis
    {
        private static LibFormatUnitCache _Default = null;
        private static object _LockObj = new object();

        public LibFormatUnitCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static  LibFormatUnitCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                            _Default = new LibFormatUnitCache("LibFormatUnitCache");
                    }
                }
                return _Default;
            }
        }

        public override bool Remove(string key, string regionName = null)
        {
           
            return base.Remove(key);
        }

        public object GetFormatData(string unitId)
        {
            object value = this.Get<object>(unitId);
            if (value == null)
            {
                //说明缓存不存在则需创建
                string sql = string.Format("Select RETAINDIGITS From COMUNIT Where UNITID={0}", LibStringBuilder.GetQuotString(unitId));
                LibDataAccess dataAccess = new LibDataAccess();
                value = dataAccess.ExecuteScalar(sql);
                if (value != null)
                { 
                    this.Set(unitId, value, new TimeSpan(0, 180, 0));
                }
            }
            return value;
        }
    }
}
