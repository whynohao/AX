using AxCRL.Comm.Redis;
using System;
using System.Collections.Specialized;
using System.Data;



namespace AxCRL.Core.Cache
{


    public class LibBillDataCache :MemoryCacheRedis
    {
        private static LibBillDataCache _Default = null;
        private static object _LockObj = new object();

        public LibBillDataCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static  LibBillDataCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                            _Default = new LibBillDataCache("LibBillDataCache");
                    }
                }
                return _Default;
            }
        }

        public void AddBillData(string key, DataSet dataSet)
        { 
            this.Set(key, dataSet, new TimeSpan(0, 180, 0));
        }

        public override bool Remove(string key, string regionName = null)
        { 
            return  base.Remove(key);
        }
    }







}
