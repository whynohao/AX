using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
//using AxCRL.Template.DataSource;
using AxCRL.Comm.Utils;

namespace AxCRL.Comm.Redis
{

    public class MemoryCacheRedis
    {

        public MemoryCacheRedis(string name)
        {
            Name = name;
            //RemoveAll();//初始化清空上次缓存数据
        }

        //
        // 摘要:
        //     获取缓存的名称。
        //
        // 返回结果:
        //     缓存的名称。
        private string Name
        { get; set; }
        private string getKey(string key)
        {
            if (key.StartsWith(Name + ":"))
            {
                return key;
            }

            return Name + ":" + key;

        }
        /// <summary>
        /// 设置string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyvalue"></param>
        /// <param name="expiry"></param>
        /// <param name="regionName"></param>
        public void StringSet(string key,string  keyvalue, TimeSpan? expiry = null, string regionName = null)
        {
            RedisManager.Default.StringSet(getKey(key), keyvalue, expiry);
        }
        /// <summary>
        /// 获得string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="regionName"></param>
        /// <returns></returns>
        public string StringGet(string key, string regionName = null)
        {
            return RedisManager.Default.StringGet(getKey(key));
        }
        /// <summary>
        /// 获得bytes
        /// </summary>
        /// <param name="key"></param>
        /// <param name="regionName"></param>
        /// <returns></returns>
        public byte[] StringGetBytes(string key, string regionName = null)
        {
            return RedisManager.Default.StringGetBytes(getKey(key));
        }
        /// <summary>
        /// 设置bytes
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyvalue"></param>
        /// <param name="expiry"></param>
        /// <param name="regionName"></param>
        public void StringSetBytes(string key, byte[] keyvalue, TimeSpan? expiry = null, string regionName = null)
        {
            RedisManager.Default.StringGetSetBytes(getKey(key), keyvalue);
            RedisManager.Default.KeyExpire(getKey(key), expiry); 
        }


        /// <summary>
        /// 设置相应主键对应实体值，以JSON方式存储同时设置有效时间
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <param name="regionName"></param>
        public void Set<T>(string key, T value, TimeSpan? expiry = null, string regionName = null)
        {
            RedisManager.Default.Set(getKey(key), value, expiry);
        }


        /// <summary>
        /// 设置相应主键对应实体值，以JSON方式存储同时设置有效截止时间
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="datetime"></param>
        /// <param name="regionName"></param>
        public void Set<T>(string key, T value, DateTime datetime, string regionName = null)
        {
            RedisManager.Default.Set(getKey(key), value);
            RedisManager.Default.KeyExpire(getKey(key), datetime);
        }
        /// <summary>
        /// 获得key值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="regionName"></param>
        /// <returns></returns>
        public T Get<T>(string key, string regionName = null)
        {
            RedisManager.Default.KeyExpire(getKey(key), RedisManager.Default.KeyTimeToLive(getKey(key)));
            return RedisManager.Default.Get<T>(getKey(key));
        }


        public static byte[] GetBinaryFormatData(object dsOriginal)
        {
            byte[] binaryDataResult = null;
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter brFormatter = new BinaryFormatter();
            //dsOriginal.RemotingFormat 为远程处理期间使用的DataSet 获取或设置 SerializtionFormat        
            //SerializationFormat.Binary      将字符串比较方法设置为使用严格的二进制排序顺序
            //dsOriginal.RemotingFormat = SerializationFormat.Binary;
            brFormatter.Serialize(memStream, dsOriginal);
            memStream.Position = 0;
            binaryDataResult = memStream.ToArray();
            memStream.Close();
            memStream.Dispose();
            return binaryDataResult;
        }
        /// <summary>
        /// 将byte[]字节数组反序列化成object对象
        /// </summary>
        /// <param name="binaryData">字节数组</param>
        /// <returns>object对象</returns>
        public static object RetrieveObjectex(byte[] binaryData)
        {
            if (binaryData == null)
                return null;
            MemoryStream memStream = new MemoryStream(binaryData);
            memStream.Position = 0;
            BinaryFormatter brFormatter = new BinaryFormatter();
            Object obj = brFormatter.Deserialize(memStream);
            memStream.Close();
            memStream.Dispose();
            return obj;
        }
        /// <summary>
        /// 将DataSet序列化为binary
        /// </summary>
        /// <param name="dsOriginal">object对象</param>
        /// <returns>字节数组</returns>
        public static byte[] GetBinaryFormatData(DataSet dsOriginal)
        { 
            byte[] binaryDataResult = null;
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter(); 
            dsOriginal.RemotingFormat = SerializationFormat.Binary;
            formatter.Serialize(memStream, dsOriginal);
            memStream.Position = 0;
            binaryDataResult = memStream.ToArray(); 
             memStream.Close();
             memStream.Dispose();
             return binaryDataResult;
        }

        /// <summary>
        /// 将byte[]字节数组反序列化成DataSet对象
        /// </summary>
        /// <param name="binaryData">字节数组</param>
        /// <returns>object对象</returns>
        public static object RetrieveObject(byte[] binaryData)
        { 
            if (binaryData == null)
                return null;
            MemoryStream memStream = new MemoryStream(binaryData);
            memStream.Position = 0;
            BinaryFormatter brFormatter = new BinaryFormatter();
            DataSet ds = (DataSet)brFormatter.Deserialize(memStream);  
            memStream.Close();
            memStream.Dispose();
            return ds;
        }




        /// <summary>
        /// 设置相应主键对应实体值，以JSON方式存储同时设置有效时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <param name="regionName"></param>
        public void Set(string key, DataSet value, TimeSpan? expiry = null, string regionName = null)
        {  
            byte[] bytes = GetBinaryFormatData(value); 
            StringSetBytes(key, bytes, expiry); 
        }


        /// <summary>
        /// 设置相应主键对应实体值，以JSON方式存储同时设置有效截止时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="datetime"></param>
        /// <param name="regionName"></param>
        public void Set (string key, DataSet value, DateTime datetime, string regionName = null)
        {
            byte[] bytes = GetBinaryFormatData(value);
            StringSetBytes(key, bytes);
            RedisManager.Default.KeyExpire(getKey(key), datetime);

        }
        /// <summary>
        /// 获得key值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="regionName"></param>
        /// <returns></returns>
        public DataSet  Get(string key, string regionName = null)
        { 
            RedisManager.Default.KeyExpire(getKey(key), RedisManager.Default.KeyTimeToLive(getKey(key))); 
            byte[] bytes = StringGetBytes(key);
            DataSet ds = (DataSet)RetrieveObject(bytes);
            return ds;
        }
        /// <summary>
        /// 删除key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="regionName"></param>
        /// <returns></returns>
        public virtual bool Remove(string key, string regionName = null)
        {
            return RedisManager.Default.KeyDelete(getKey(key));
        }

        /// <summary>
        /// 删除所有的key
        /// </summary>
        /// <returns></returns>
        public bool RemoveAll()
        {
            IEnumerator<string> enumerator = this.GetKeys();
            while (enumerator.MoveNext())
            {
                string key = enumerator.Current;
                Remove(key);
            }
            return true;
        }



        /// <summary>
        /// 设置有效时间间隔
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <param name="regionName"></param>
        /// <returns></returns>
        public bool KeyExpire(string key, TimeSpan? expiry = null, string regionName = null)
        {
            return RedisManager.Default.KeyExpire(getKey(key), expiry); 
        }




        /// <summary>
        /// 模糊查询获取
        /// </summary>
        public IEnumerator<string> GetKeys()
        {
            return GetKeys(Name + ":*");
        }
        /// <summary>
        /// 模糊查询获取
        /// </summary>
        public IEnumerator<string> GetKeys(string Pattern)
        {
            foreach (var ep in RedisManager.Default.Multiplexer.GetEndPoints())
            {
                var server = RedisManager.Default.Multiplexer.GetServer(ep);
                var keys = server.Keys(database:RedisManager.Default.Database, pattern: Pattern).ToArray();
                foreach (RedisKey rk in keys)
                {
                    string returnstr = rk.ToString();
                    if (returnstr.Contains(Name+":"))
                    {
                        returnstr= returnstr.Substring(Name.Length + 1, returnstr.Length - Name.Length - 1);
                    } 
                    yield return returnstr;
                }
            }
        }

        /// <summary>
        /// 获得这个缓存下面的数量
        /// </summary>
        public long GetCount()
        {
            foreach (var ep in RedisManager.Default.Multiplexer.GetEndPoints())
            {
               var server = RedisManager.Default.Multiplexer.GetServer(ep);
               return  server.Keys(database: RedisManager.Default.Database,pattern: Name +"*").ToArray().Count(); 
            }
            return 0;
        }


    }
}



