using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Data;
using AxCRL.Comm.Redis;
using AxCRL.Data.SqlBuilder;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Cache
{
    public class LibParamCache : MemoryCacheRedis
    {
        private static LibParamCache _Default = null;
        private static object _LockObj = new object();

        public LibParamCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static  LibParamCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                            _Default = new LibParamCache("LibParamCache");
                    }
                }
                return _Default;
            }
        }

        private string BuildCacheKey(string progId, object[] pks)
        {
            string key = string.Empty;
            if (pks.Length > 1)
            {
                StringBuilder strBuilder = new StringBuilder();
                foreach (var item in pks)
                {
                    strBuilder.AppendFormat("/t{0}", item);
                }
                key = string.Format("{0}{1}", progId, strBuilder.ToString());
            }
            else
                key = string.Format("{0}/t{1}", progId, pks[0]);
            return key;
        }


        public bool RemoveCacheItem(string progId, object[] pks)
        { 
            string key = BuildCacheKey(progId, pks); 
            return this.Remove(key);
        }

        public object GetValueByName(string progId, object[] pks, string name)
        {
            object value = null;
            string key = BuildCacheKey(progId, pks);
            Dictionary<string, object> destObj = this.Get<Dictionary<string, object>>(key);
            if (destObj == null)
            {
                destObj = new Dictionary<string, object>();
                LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel(progId);
                DataColumnCollection columns = sqlModel.Tables[0].Columns;
                //说明缓存不存在则需创建
                StringBuilder whereBuilder = new StringBuilder();
                for (int i = 0; i < sqlModel.Tables[0].PrimaryKey.Length; i++)
                {
                    if (i != 0)
                        whereBuilder.AppendFormat(" AND ");
                    if (pks[i].GetType() == typeof(string))
                        whereBuilder.AppendFormat("A.{0}={1}", sqlModel.Tables[0].PrimaryKey[i].ColumnName, LibStringBuilder.GetQuotObject(pks[i]));
                    else
                        whereBuilder.AppendFormat("A.{0}={1}", sqlModel.Tables[0].PrimaryKey[i].ColumnName, pks[i]);
                }
                SqlBuilder sqlBuilder = new SqlBuilder(progId);
                string sql = sqlBuilder.GetQuerySql(0, "A.*", whereBuilder.ToString());
                //TODO固定字段应排除
                LibDataAccess dataAccess = new LibDataAccess();
                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    if (reader.Read())
                    {
                        int count = reader.FieldCount;
                        for (int i = 0; i < count; i++)
                        {
                            string columnName = reader.GetName(i);
                            object columnValue = reader.GetValue(i);
                            if (dataAccess.DatabaseType == LibDatabaseType.Oracle && columnValue.GetType() == typeof(decimal))
                            {   //如果是oracle 数值类型都是number
                                LibDataType dataType = (LibDataType)((int)columns[columnName].ExtendedProperties[FieldProperty.DataType]);
                                switch (dataType)
                                {
                                    case LibDataType.Int32:
                                        columnValue = decimal.ToInt32((decimal)columnValue);
                                        break;
                                    case LibDataType.Int64:
                                        columnValue = decimal.ToInt64((decimal)columnValue);
                                        break;
                                    case LibDataType.Float:
                                        columnValue = decimal.ToSingle((decimal)columnValue);
                                        break;
                                    case LibDataType.Double:
                                        columnValue = decimal.ToDouble((decimal)columnValue);
                                        break;
                                    case LibDataType.Byte:
                                        columnValue = decimal.ToByte((decimal)columnValue);
                                        break;
                                    case LibDataType.Boolean:
                                        columnValue = (decimal)columnValue == decimal.Zero ? false : true;
                                        break;
                                }
                            }
                            destObj.Add(columnName, columnValue);
                        }
                    }
                }
                if (destObj.Count > 0)
                {
                    //CacheItemPolicy policy = new CacheItemPolicy();
                    //policy.SlidingExpiration = new TimeSpan(0, 180, 0); //30分钟内不访问自动剔除
                    _Default.Set(key, destObj, new TimeSpan(0, 180, 0));
                }
            }
            destObj.TryGetValue(name, out value);
            return value;
        }
    }
}
