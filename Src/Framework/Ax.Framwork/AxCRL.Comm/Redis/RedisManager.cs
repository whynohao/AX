// ******************************************************************************************
// * 文件名：RedisManager.cs
// * 功能描述：
// * 创建人：wangjun
// * 创建日期：2016年09月13日 16:19
// *******************************************************************************************

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using StackExchange.Redis;
using AxCRL.Comm.Runtime;

namespace AxCRL.Comm.Redis
{
    /// <summary>
    /// Redis操作类
    /// </summary>
    public partial class RedisManager
    {
        private readonly object _lockObj = new object();
        private ConnectionMultiplexer _multiplexer;
        private static RedisManager _default;

        private RedisManager()
        {
        }

        public static RedisManager Default
        {
            get
            {
                return _default ?? (_default = new RedisManager() { Database = EnvProvider.Default.RedisDbIndex });
            }
        }

        /// <summary>
        /// 单实例
        /// </summary>
        internal ConnectionMultiplexer Multiplexer
        {
            get
            {
                if (_multiplexer == null)
                {
                    lock (_lockObj)
                    {
                        if (_multiplexer == null)
                            _multiplexer = GetConnectionMultiplexer();
                    }
                }
                return _multiplexer;
            }
        }

        /// <summary>
        /// 获取或设置数据库索引，根据配置连接中数据库顺序获取
        /// </summary>.
        /// 


        public int Database { get; set; }

        /// <summary>
        /// 根据配置文件获取数据库连接地址
        /// </summary>
        /// <returns></returns>
        private ConnectionMultiplexer GetConnectionMultiplexer()
        {
            try
            {
                //var config = ConfigurationManager.OpenExeConfiguration("D:\\cps\\AX\\AxSolution\\Ax.Server\\web.config");
                //var strings = config.ConnectionStrings.ConnectionStrings["RedisConnection"];
                var strings = ConfigurationManager.ConnectionStrings["RedisConnection"];
                return ConnectionMultiplexer.Connect(strings == null ? "127.0.0.1:6379" : strings.ConnectionString);
            }
            catch (Exception)
            {
                return ConnectionMultiplexer.Connect( "127.0.0.1:6379" );
            }
        }

        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <param name="db">数据库索引</param>
        /// <param name="asyncState">异步内容</param>
        /// <returns></returns>
        private IDatabase GetDatabase(int db = -1, object asyncState = null)
        {
            return Multiplexer.GetDatabase(db, asyncState);
        }

        /// <summary>
        /// 获取订阅接口
        /// </summary>
        /// <returns></returns>
        private ISubscriber GetSubscriber()
        {
            return Multiplexer.GetSubscriber();
        }

        /// <summary>
        /// 根据KEY获取相应存储的实体的JSON字符串，并序列化成相应实体对象
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="key">主键</param>
        /// <returns></returns>
        public T Get<T>(dynamic key)
        {
            if (string.IsNullOrWhiteSpace(key)) return default(T);
            var value = StringGet(key);
            if (value == null)
                return default(T);
            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        ///  设置相应主键对应实体值，以JSON方式存储
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool Set<T>(dynamic key, T value, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null) return false;
            var s = JsonConvert.SerializeObject(value);
            return StringSet(key, s, expiry);




        }

        /// <summary>
        /// 批量操作
        /// </summary>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        public IBatch CreateBatch(object asyncState = null)
        {
            var db = GetDatabase(Database);
            return db.CreateBatch(asyncState);
        } 
     
        private bool IsConnected(RedisChannel channel = default(RedisChannel))
        {
            var s = GetSubscriber();
            return s.IsConnected(channel);
        }

        /// <summary>
        /// 频道是否连接
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool IsConnected(string channel)
        {
            return IsConnected((RedisChannel)channel);
        }

        private void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler,
            CommandFlags flags = CommandFlags.None)
        {
            var s = GetSubscriber();
            s.Subscribe(channel, handler, flags);
        }

        private void Unsubscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler = null,
            CommandFlags flags = CommandFlags.None)
        {
            var s = GetSubscriber();
            s.Unsubscribe(channel, handler, flags);
        }

        private void UnsubscribeAll(CommandFlags flags = CommandFlags.None)
        {
            var s = GetSubscriber();
            s.UnsubscribeAll(flags);
        }

        public void Subscribe(string channel, Action<string, string> handler, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            Action<RedisChannel, RedisValue> h = (redisChannel, value) => handler(redisChannel, value);
            Subscribe(channel, h, (CommandFlags)flags);
        }

        public void Unsubscribe(string channel, Action<string, string> handler, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            Action<RedisChannel, RedisValue> h = (redisChannel, value) => handler(redisChannel, value);
            Unsubscribe(channel, h, (CommandFlags)flags);
        }

        public void UnsubscribeAll(RedisCommandFlags flags = RedisCommandFlags.None)
        {
            UnsubscribeAll((CommandFlags)flags);
        }

        /// <summary>
        /// 根据主键从源实例向目标实例转换
        /// </summary>
        /// <param name="key"></param>
        /// <param name="toServer"></param>
        /// <param name="toDatabase"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <param name="migrateOptions"></param>
        /// <param name="flags"></param>
        private void KeyMigrate(RedisKey key, EndPoint toServer, int toDatabase = 0, int timeoutMilliseconds = 0,
            MigrateOptions migrateOptions = MigrateOptions.None, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            db.KeyMigrate(key, toServer, toDatabase, timeoutMilliseconds, migrateOptions, flags);
        }

        /// <summary>
        /// 根据主键从源实例向目标实例转换
        /// </summary>
        /// <param name="key"></param>
        /// <param name="toServer"></param>
        /// <param name="toDatabase"></param>
        /// <param name="timeoutMilliseconds"></param>
        /// <param name="migrateOptions"></param>
        /// <param name="flags"></param>
        public void KeyMigrate(string key, EndPoint toServer, int toDatabase = 0, int timeoutMilliseconds = 0, RedisMigrateOptions migrateOptions = RedisMigrateOptions.None, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            KeyMigrate(key, toServer, toDatabase, timeoutMilliseconds, (MigrateOptions)migrateOptions, (CommandFlags)flags);
        }

        /// <summary>
        /// 创建事务
        /// </summary>
        /// <param name="asyncState"></param>
        /// <returns></returns>
        public ITransaction CreateTransaction(object asyncState = null)
        {
            var db = GetDatabase(Database);
            return db.CreateTransaction(asyncState);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private long HashDecrement(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashDecrement(key, hashField, value, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private double HashDecrement(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashDecrement(key, hashField, value, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public long HashDecrement(string key, string hashField, long value = 1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashDecrement(key, hashField, value, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public double HashDecrement(string key, string hashField, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashDecrement(key, hashField, value, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public long HashDecrement(string key, byte[] hashField, long value = 1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashDecrement(key, hashField, value, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public double HashDecrement(string key, byte[] hashField, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashDecrement(key, hashField, value, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private bool HashDelete(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashDelete(key, hashField, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashFields"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private long HashDelete(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashDelete(key, hashFields, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool HashDelete(string key, string hashField, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashDelete(key, hashField, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool HashDelete(string key, byte[] hashField, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashDelete(key, hashField, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashFields"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public long HashDelete(string key, string[] hashFields, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            var values = hashFields.ToRedisValueArray();
            return HashDelete(key, values, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashFields"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public long HashDelete(string key, byte[][] hashFields, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            var values = hashFields.ToRedisValueArray();
            return HashDelete(key, values, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private bool HashExists(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashExists(key, hashField, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool HashExists(string key, string hashField, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashExists(key, hashField, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool HashExists(string key, byte[] hashField, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashExists(key, hashField, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private RedisValue HashGet(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashGet(key, hashField, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashFields"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private RedisValue[] HashGet(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashGet(key, hashFields, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public string HashGet(string key, string hashField, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashGet(key, hashField, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public byte[] HashGet(string key, byte[] hashField, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashGet(key, hashField, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashFields"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public string[] HashGet(string key, string[] hashFields, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            var values = hashFields.ToRedisValueArray();
            var result = HashGet(key, values, (CommandFlags)flags);
            return result.Select(s => (string)s).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashFields"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public byte[][] HashGet(string key, byte[][] hashFields, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            var values = hashFields.ToRedisValueArray();
            var result = HashGet(key, values, (CommandFlags)flags);
            return result.Select(s => (byte[])s).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private HashEntry[] HashGetAll(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashGetAll(key, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public Dictionary<string, string> HashGetAll(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            var entries = HashGetAll(key, (CommandFlags)flags);
            return entries.ToStringDictionary();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public Dictionary<string, byte[]> HashGetAllBytes(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            var entries = HashGetAll(key, (CommandFlags)flags);
            return entries.ToDictionary<HashEntry, string, byte[]>(t => t.Name, t => t.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private long HashIncrement(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashIncrement(key, hashField, value, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private double HashIncrement(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashIncrement(key, hashField, value, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public long HashIncrement(string key, string hashField, long value = 1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashIncrement(key, hashField, value, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public double HashIncrement(string key, string hashField, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashIncrement(key, hashField, value, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public long HashIncrement(string key, byte[] hashField, long value = 1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashIncrement(key, hashField, value, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public double HashIncrement(string key, byte[] hashField, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashIncrement(key, hashField, value, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private RedisValue[] HashKeys(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashKeys(key, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public string[] HashKeys(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            var values = HashKeys(key, (CommandFlags)flags);
            return values.ToStringArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private long HashLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashLength(key, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public long HashLength(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashLength(key, (CommandFlags)flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pattern"></param>
        /// <param name="pageSize"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private IEnumerable<HashEntry> HashScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
        {
            var db = GetDatabase(Database);
            return db.HashScan(key, pattern, pageSize, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pattern"></param>
        /// <param name="pageSize"></param>
        /// <param name="cursor"></param>
        /// <param name="pageOffset"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private IEnumerable<HashEntry> HashScan(RedisKey key, RedisValue pattern = new RedisValue(), int pageSize = 10, long cursor = 0,
            int pageOffset = 0, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashScan(key, pattern, pageSize, cursor, pageOffset, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pattern"></param>
        /// <param name="pageSize"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, string>> HashScan(string key, string pattern, int pageSize, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            var values = HashScan(key, pattern, pageSize, (CommandFlags)flags);
            return values.Select(s => new KeyValuePair<string, string>(s.Name, s.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pattern"></param>
        /// <param name="pageSize"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, byte[]>> HashScanBytes(string key, byte[] pattern, int pageSize, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            var values = HashScan(key, pattern, pageSize, (CommandFlags)flags);
            return values.Select(s => new KeyValuePair<string, byte[]>(s.Name, s.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pattern"></param>
        /// <param name="pageSize"></param>
        /// <param name="cursor"></param>
        /// <param name="pageOffset"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, string>> HashScan(string key, string pattern,
            int pageSize, long cursor, int pageOffset, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            var values = HashScan(key, pattern, pageSize, cursor, pageOffset, (CommandFlags)flags);
            return values.Select(s => new KeyValuePair<string, string>(s.Name, s.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pattern"></param>
        /// <param name="pageSize"></param>
        /// <param name="cursor"></param>
        /// <param name="pageOffset"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, byte[]>> HashScanBytes(string key, byte[] pattern,
            int pageSize, long cursor, int pageOffset, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            var values = HashScan(key, pattern, pageSize, cursor, pageOffset, (CommandFlags)flags);
            return values.Select(s => new KeyValuePair<string, byte[]>(s.Name, s.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashFields"></param>
        /// <param name="flags"></param>
        private void HashSet(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            db.HashSet(key, hashFields, flags);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <param name="value"></param>
        /// <param name="when"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private bool HashSet(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashSet(key, hashField, value, when, flags);
        }

        public void HashSet(string key, Dictionary<string, string> hashFields, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            HashSet(key, hashFields.Select(s => new HashEntry(s.Key, s.Value)).ToArray(), (CommandFlags)flags);
        }

        public void HashSet(string key, Dictionary<string, byte[]> hashFields, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            HashSet(key, hashFields.Select(s => new HashEntry(s.Key, s.Value)).ToArray(), (CommandFlags)flags);
        }

        public bool HashSet(string key, string hashField, string value, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashSet(key, hashField, value, (When)when, (CommandFlags)flags);
        }

        public bool HashSet(string key, byte[] hashField, byte[] value, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashSet(key, hashField, value, (When)when, (CommandFlags)flags);
        }

        private RedisValue[] HashValues(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashValues(key, flags);
        }

        public string[] HashValues(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashValues(key, (CommandFlags)flags).ToStringArray();
        }

        public byte[][] HashValuesBytes(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return HashValues(key, (CommandFlags)flags).ToBytesArray();
        }

        private bool KeyDelete(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyDelete(key, flags);
        }

        private long KeyDelete(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyDelete(keys, flags);
        }

        public bool KeyDelete(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return KeyDelete(key, (CommandFlags)flags);
        }

        public long KeyDelete(string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return KeyDelete(keys.ToRedisKeyArray(), (CommandFlags)flags);
        }

        private byte[] KeyDump(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyDump(key, flags);
        }

        public byte[] KeyDump(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return KeyDump(key, (CommandFlags)flags);
        }

        private bool KeyExists(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyExists(key, flags);
        }

        public bool KeyExists(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return KeyExists(key, (CommandFlags)flags);
        }

        private bool KeyExpire(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyExpire(key, expiry, flags);
        }

        private bool KeyExpire(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyExpire(key, expiry, flags);
        }

        public bool KeyExpire(string key, TimeSpan? expiry, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return KeyExpire(key, expiry, (CommandFlags)flags);
        }

        public bool KeyExpire(string key, DateTime? expiry, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return KeyExpire(key, expiry, (CommandFlags)flags);
        }

        private bool KeyMove(RedisKey key, int database, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyMove(key, database, flags);
        }

        public bool KeyMove(string key, int database, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return KeyMove(key, database, (CommandFlags)flags);
        }

        private bool KeyPersist(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyPersist(key, flags);
        }

        public bool KeyPersist(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return KeyPersist(key, (CommandFlags)flags);
        }

        private RedisKey KeyRandom(CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyRandom(flags);
        }

        public string KeyRandom(RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return KeyRandom((CommandFlags)flags);
        }

        private bool KeyRename(RedisKey key, RedisKey newKey, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyRename(key, newKey, when, flags);
        }

        public bool KeyRename(string key, string newKey, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return KeyRename(key, newKey, (When)when, (CommandFlags)flags);
        }

        private void KeyRestore(RedisKey key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            db.KeyRestore(key, value, expiry, flags);
        }

        public void KeyRestore(string key, byte[] value, TimeSpan? expiry = null, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            KeyRestore(key, value, expiry, (CommandFlags)flags);
        }

        private TimeSpan? KeyTimeToLive(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyTimeToLive(key, flags);
        }

        public TimeSpan? KeyTimeToLive(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return KeyTimeToLive(key, (CommandFlags)flags);
        }

        private RedisValue ListGetByIndex(RedisKey key, long index, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListGetByIndex(key, index, flags);
        }

        public string ListGetByIndex(string key, long index, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListGetByIndex(key, index, (CommandFlags)flags);
        }

        public byte[] ListGetBytesByIndex(string key, long index, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListGetByIndex(key, index, (CommandFlags)flags);
        }

        private long ListInsertAfter(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListInsertAfter(key, pivot, value, flags);
        }

        public long ListInsertAfter(string key, string pivot, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListInsertAfter(key, pivot, value, (CommandFlags)flags);
        }

        public long ListInsertAfter(string key, byte[] pivot, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListInsertAfter(key, pivot, value, (CommandFlags)flags);
        }

        private long ListInsertBefore(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListInsertBefore(key, pivot, value, flags);
        }

        public long ListInsertBefore(string key, string pivot, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListInsertBefore(key, pivot, value, (CommandFlags)flags);
        }

        public long ListInsertBefore(string key, byte[] pivot, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListInsertBefore(key, pivot, value, (CommandFlags)flags);
        }

        private RedisValue ListLeftPop(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListLeftPop(key, flags);
        }

        public string ListLeftPop(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListLeftPop(key, (CommandFlags)flags);
        }

        public byte[] ListBytesLeftPop(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListLeftPop(key, (CommandFlags)flags);
        }

        private long ListLeftPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListLeftPush(key, value, when, flags);
        }

        private long ListLeftPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListLeftPush(key, values, flags);
        }

        public long ListLeftPush(string key, string value, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListLeftPush(key, value, (When)when, (CommandFlags)flags);
        }

        public long ListLeftPush(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListLeftPush(key, value, When.Always, (CommandFlags)flags);
        }

        public long ListLeftPush(string key, string[] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListLeftPush(key, values.ToRedisValueArray(), (CommandFlags)flags);
        }

        public long ListLeftPush(string key, byte[][] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListLeftPush(key, values.ToRedisValueArray(), (CommandFlags)flags);
        }

        private long ListLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListLength(key, flags);
        }

        public long ListLength(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListLength(key, (CommandFlags)flags);
        }

        private RedisValue[] ListRange(RedisKey key, long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListRange(key, start, stop, flags);
        }

        public string[] ListRange(string key, long start = 0, long stop = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListRange(key, start, stop, (CommandFlags)flags).ToStringArray();
        }

        public byte[][] ListBytesRange(string key, long start = 0, long stop = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListRange(key, start, stop, (CommandFlags)flags).Select(s => (byte[])s).ToArray();
        }

        private long ListRemove(RedisKey key, RedisValue value, long count = 0, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListRemove(key, value, count, flags);
        }

        public long ListRemove(string key, string value, long count = 0, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListRemove(key, value, count, (CommandFlags)flags);
        }

        public long ListRemove(string key, byte[] value, long count = 0, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListRemove(key, value, count, (CommandFlags)flags);
        }

        private RedisValue ListRightPop(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListRightPop(key, flags);
        }

        public string ListRightPop(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListRightPop(key, (CommandFlags)flags);
        }

        public byte[] ListBytesRightPop(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListRightPop(key, (CommandFlags)flags);
        }

        private RedisValue ListRightPopLeftPush(RedisKey source, RedisKey destination, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListRightPopLeftPush(source, destination, flags);
        }

        public string ListRightPopLeftPush(string source, string destination, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListRightPopLeftPush(source, destination, (CommandFlags)flags);
        }

        public byte[] ListBytesRightPopLeftPush(string source, string destination, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListRightPopLeftPush(source, destination, (CommandFlags)flags);
        }

        private long ListRightPush(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListRightPush(key, value, when, flags);
        }

        public long ListRightPush(string key, string value, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListRightPush(key, value, (When)when, (CommandFlags)flags);
        }

        public long ListRightPush(string key, byte[] value, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListRightPush(key, value, (When)when, (CommandFlags)flags);
        }

        private long ListRightPush(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListRightPush(key, values, flags);
        }

        public long ListRightPush(string key, string[] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListRightPush(key, values.ToRedisValueArray(), (CommandFlags)flags);
        }

        public long ListBytesRightPush(string key, byte[][] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return ListRightPush(key, values.ToRedisValueArray(), (CommandFlags)flags);
        }

        private void ListSetByIndex(RedisKey key, long index, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            db.ListSetByIndex(key, index, value, flags);
        }

        public void ListSetByIndex(string key, long index, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            ListSetByIndex(key, index, value, (CommandFlags)flags);
        }

        public void ListSetByIndex(string key, long index, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            ListSetByIndex(key, index, value, (CommandFlags)flags);
        }

        private void ListTrim(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            db.ListTrim(key, start, stop, flags);
        }

        public void ListTrim(string key, long start, long stop, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            ListTrim(key, start, stop, (CommandFlags)flags);
        }

        private bool LockExtend(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.LockExtend(key, value, expiry, flags);
        }

        public bool LockExtend(string key, string value, TimeSpan expiry, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return LockExtend(key, value, expiry, (CommandFlags)flags);
        }

        public bool LockExtend(string key, byte[] value, TimeSpan expiry, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return LockExtend(key, value, expiry, (CommandFlags)flags);
        }

        private RedisValue LockQuery(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.LockQuery(key, flags);
        }

        public string LockQuery(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return LockQuery(key, (CommandFlags)flags);
        }

        public byte[] LockQueryBytes(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return LockQuery(key, (CommandFlags)flags);
        }

        private bool LockRelease(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.LockRelease(key, value, flags);
        }

        public bool LockRelease(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return LockRelease(key, value, (CommandFlags)flags);
        }

        public bool LockRelease(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return LockRelease(key, value, (CommandFlags)flags);
        }

        private bool LockTake(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.LockTake(key, value, expiry, flags);
        }

        public bool LockTake(string key, string value, TimeSpan expiry, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return LockTake(key, value, expiry, (CommandFlags)flags);
        }

        public bool LockTake(string key, byte[] value, TimeSpan expiry, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return LockTake(key, value, expiry, (CommandFlags)flags);
        }

        private long Publish(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.Publish(channel, message, flags);
        }

        public long Publish(string channel, string message, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return Publish(channel, message, (CommandFlags)flags);
        }

        public long Publish(string channel, byte[] message, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return Publish(channel, message, (CommandFlags)flags);
        }

        private bool SetAdd(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetAdd(key, value, flags);
        }

        private long SetAdd(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetAdd(key, values, flags);
        }

        public bool SetAdd(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetAdd(key, value, (CommandFlags)flags);
        }

        public bool SetAdd(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetAdd(key, value, (CommandFlags)flags);
        }

        public long SetAdd(string key, string[] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetAdd(key, values.ToRedisValueArray(), (CommandFlags)flags);
        }

        public long SetAdd(string key, byte[][] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetAdd(key, values.ToRedisValueArray(), (CommandFlags)flags);
        }

        private RedisValue[] SetCombine(SetOperation operation, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetCombine(operation, first, second, flags);
        }

        private RedisValue[] SetCombine(SetOperation operation, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetCombine(operation, keys, flags);
        }

        public string[] SetCombine(RedisSetOperation operation, string first, string second, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetCombine((SetOperation)operation, first, second, (CommandFlags)flags).ToStringArray();
        }

        public byte[][] SetCombineBytes(RedisSetOperation operation, string first, string second, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetCombine((SetOperation)operation, first, second, (CommandFlags)flags).ToBytesArray();
        }

        public string[] SetCombine(RedisSetOperation operation, string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetCombine((SetOperation)operation, keys.ToRedisKeyArray(), (CommandFlags)flags).ToStringArray();
        }

        public byte[][] SetCombineBytes(RedisSetOperation operation, string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetCombine((SetOperation)operation, keys.ToRedisKeyArray(), (CommandFlags)flags).ToBytesArray();
        }

        private long SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetCombineAndStore(operation, destination, first, second, flags);
        }

        private long SetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetCombineAndStore(operation, destination, keys, flags);
        }

        public long SetCombineAndStore(RedisSetOperation operation, string destination, string first, string second, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetCombineAndStore((SetOperation)operation, destination, first, second, (CommandFlags)flags);
        }

        public long SetCombineAndStore(RedisSetOperation operation, string destination, string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetCombineAndStore((SetOperation)operation, destination, keys.ToRedisKeyArray(), (CommandFlags)flags);
        }

        private bool SetContains(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetContains(key, value, flags);
        }

        public bool SetContains(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetContains(key, value, (CommandFlags)flags);
        }

        public bool SetContains(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetContains(key, value, (CommandFlags)flags);
        }

        private long SetLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetLength(key, flags);
        }

        public long SetLength(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetLength(key, (CommandFlags)flags);
        }

        private RedisValue[] SetMembers(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetMembers(key, flags);
        }

        public string[] SetMembers(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetMembers(key, (CommandFlags)flags).ToStringArray();
        }

        public byte[][] SetMembersBytes(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetMembers(key, (CommandFlags)flags).ToBytesArray();
        }

        private bool SetMove(RedisKey source, RedisKey destination, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetMove(source, destination, value, flags);
        }

        public bool SetMove(string source, string destination, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetMove(source, destination, value, (CommandFlags)flags);
        }

        public bool SetMove(string source, string destination, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetMove(source, destination, value, (CommandFlags)flags);
        }

        private RedisValue SetPop(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetPop(key, flags);
        }

        public string SetPop(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetPop(key, (CommandFlags)flags);
        }

        public byte[] SetPopBytes(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetPop(key, (CommandFlags)flags);
        }

        private RedisValue SetRandomMember(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetRandomMember(key, flags);
        }

        private RedisValue[] SetRandomMembers(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetRandomMembers(key, count, flags);
        }

        public string SetRandomMember(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetRandomMember(key, (CommandFlags)flags);
        }

        public byte[] SetRandomMemberBytes(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetRandomMember(key, (CommandFlags)flags);
        }

        public string[] SetRandomMembers(string key, long count, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetRandomMembers(key, count, (CommandFlags)flags).ToStringArray();
        }

        public byte[][] SetRandomMembersBytes(string key, long count, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetRandomMembers(key, count, (CommandFlags)flags).ToBytesArray();
        }

        private bool SetRemove(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetRemove(key, value, flags);
        }

        private long SetRemove(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetRemove(key, values, flags);
        }

        public bool SetRemove(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetRemove(key, value, (CommandFlags)flags);
        }

        public bool SetRemove(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetRemove(key, value, (CommandFlags)flags);
        }

        public long SetRemove(string key, string[] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetRemove(key, values.ToRedisValueArray(), (CommandFlags)flags);
        }

        public long SetRemove(string key, byte[][] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetRemove(key, values.ToRedisValueArray(), (CommandFlags)flags);
        }

        private IEnumerable<RedisValue> SetScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
        {
            var db = GetDatabase(Database);
            return db.SetScan(key, pattern, pageSize, flags);
        }

        private IEnumerable<RedisValue> SetScan(RedisKey key, RedisValue pattern = new RedisValue(), int pageSize = 10, long cursor = 0,
            int pageOffset = 0, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetScan(key, pattern, pageSize, cursor, pageOffset, flags);
        }

        public IEnumerable<string> SetScan(string key, string pattern, int pageSize, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetScan(key, pattern, pageSize, (CommandFlags)flags).Select(s => (string)s);
        }

        public IEnumerable<byte[]> SetScanBytes(string key, string pattern, int pageSize, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetScan(key, pattern, pageSize, (CommandFlags)flags).Select(s => (byte[])s);
        }

        public IEnumerable<string> SetScan(string key, string pattern, int pageSize, long cursor, int pageOffset, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetScan(key, pattern, pageSize, cursor, pageOffset, (CommandFlags)flags).Select(s => (string)s);
        }

        public IEnumerable<byte[]> SetScanBytes(string key, string pattern, int pageSize, long cursor, int pageOffset, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SetScan(key, pattern, pageSize, cursor, pageOffset, (CommandFlags)flags).Select(s => (byte[])s);
        }

        private RedisValue[] Sort(RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric,
            RedisValue @by = new RedisValue(), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.Sort(key, skip, take, order, sortType, @by, get, flags);
        }

        public string[] Sort(string key, long skip = 0, long take = -1, RedisOrder order = RedisOrder.Ascending, RedisSortType sortType = RedisSortType.Numeric,
            string @by = null, string[] get = null, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return Sort(key, skip, take, (Order)order, (SortType)sortType, @by, get.ToRedisValueArray(), (CommandFlags)flags).ToStringArray();
        }

        public byte[][] SortBytes(string key, long skip = 0, long take = -1, RedisOrder order = RedisOrder.Ascending, RedisSortType sortType = RedisSortType.Numeric,
            byte[] @by = null, byte[][] get = null, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return Sort(key, skip, take, (Order)order, (SortType)sortType, @by, get.ToRedisValueArray(), (CommandFlags)flags).ToBytesArray();
        }


        private long SortAndStore(RedisKey destination, RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending,
            SortType sortType = SortType.Numeric, RedisValue @by = new RedisValue(), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortAndStore(destination, key, skip, take, order, sortType, @by, get, flags);
        }

        public long SortAndStore(string destination, string key, long skip = 0, long take = -1, RedisOrder order = RedisOrder.Ascending, RedisSortType sortType = RedisSortType.Numeric,
            string @by = null, string[] get = null, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortAndStore(destination, key, skip, take, (Order)order, (SortType)sortType, @by, get.ToRedisValueArray(), (CommandFlags)flags);
        }

        public long SortAndStore(string destination, string key, long skip = 0, long take = -1, RedisOrder order = RedisOrder.Ascending, RedisSortType sortType = RedisSortType.Numeric,
          byte[] @by = null, byte[][] get = null, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortAndStore(destination, key, skip, take, (Order)order, (SortType)sortType, @by, get.ToRedisValueArray(), (CommandFlags)flags);
        }

        private bool SortedSetAdd(RedisKey key, RedisValue member, double score, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetAdd(key, member, score, flags);
        }

        private long SortedSetAdd(RedisKey key, SortedSetEntry[] values, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetAdd(key, values, flags);
        }

        public bool SortedSetAdd(string key, string member, double score, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetAdd(key, member, score, (CommandFlags)flags);
        }

        public bool SortedSetAdd(string key, byte[] member, double score, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetAdd(key, member, score, (CommandFlags)flags);
        }

        public long SortedSetAdd(string key, Dictionary<string, double> values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetAdd(key, values.Select(value => new SortedSetEntry(value.Key, value.Value)).ToArray(), (CommandFlags)flags);
        }

        public long SortedSetAdd(string key, Dictionary<byte[], double> values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetAdd(key, values.Select(value => new SortedSetEntry(value.Key, value.Value)).ToArray(), (CommandFlags)flags);
        }

        private long SortedSetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second,
            Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetCombineAndStore(operation, destination, first, second, aggregate, flags);
        }

        private long SortedSetCombineAndStore(SetOperation operation, RedisKey destination, RedisKey[] keys, double[] weights = null,
            Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetCombineAndStore(operation, destination, keys, weights, aggregate, flags);
        }

        public long SortedSetCombineAndStore(RedisSetOperation operation, string destination, string first, string second, RedisAggregate aggregate = RedisAggregate.Sum, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetCombineAndStore((SetOperation)operation, destination, first, second, (Aggregate)aggregate, (CommandFlags)flags);
        }

        public long SortedSetCombineAndStore(RedisSetOperation operation, string destination, string[] keys, double[] weights = null, RedisAggregate aggregate = RedisAggregate.Sum, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetCombineAndStore((SetOperation)operation, destination, keys.ToRedisKeyArray(), weights, (Aggregate)aggregate, (CommandFlags)flags);
        }

        private double SortedSetDecrement(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetDecrement(key, member, value, flags);
        }

        public double SortedSetDecrement(string key, string member, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetDecrement(key, member, value, (CommandFlags)flags);
        }

        public double SortedSetDecrement(string key, byte[] member, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetDecrement(key, member, value, (CommandFlags)flags);
        }

        private double SortedSetIncrement(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetIncrement(key, member, value, flags);
        }

        public double SortedSetIncrement(string key, string member, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetIncrement(key, member, value, (CommandFlags)flags);
        }

        public double SortedSetIncrement(string key, byte[] member, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetIncrement(key, member, value, (CommandFlags)flags);
        }

        private long SortedSetLength(RedisKey key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetLength(key, min, max, exclude, flags);
        }

        public long SortedSetLength(string key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, RedisExclude exclude = RedisExclude.None,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetLength(key, min, max, (Exclude)exclude, (CommandFlags)flags);
        }

        private long SortedSetLengthByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetLengthByValue(key, min, max, exclude, flags);
        }

        public long SortedSetLengthByValue(string key, string min, string max, RedisExclude exclude = RedisExclude.None,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetLengthByValue(key, min, max, (Exclude)exclude, (CommandFlags)flags);
        }

        public long SortedSetLengthByValue(string key, byte[] min, byte[] max, RedisExclude exclude = RedisExclude.None,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetLengthByValue(key, min, max, (Exclude)exclude, (CommandFlags)flags);
        }

        private RedisValue[] SortedSetRangeByRank(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRangeByRank(key, start, stop, order, flags);
        }

        public string[] SortedSetRangeByRank(string key, long start = 0, long stop = -1, RedisOrder order = RedisOrder.Ascending,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRangeByRank(key, start, stop, (Order)order, (CommandFlags)flags).ToStringArray();
        }

        public byte[][] SortedSetRangeByRankBytes(string key, long start = 0, long stop = -1, RedisOrder order = RedisOrder.Ascending,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRangeByRank(key, start, stop, (Order)order, (CommandFlags)flags).ToBytesArray();
        }

        private SortedSetEntry[] SortedSetRangeByRankWithScores(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRangeByRankWithScores(key, start, stop, order, flags);
        }

        public Dictionary<string, double> SortedSetRangeByRankWithScores(string key, long start = 0, long stop = -1, RedisOrder order = RedisOrder.Ascending,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRangeByRankWithScores(key, start, stop, (Order)order, (CommandFlags)flags).ToDictionary<SortedSetEntry, string, double>(t => t.Element, t => t.Score);
        }

        public Dictionary<byte[], double> SortedSetRangeByRankWithScoresBytes(string key, long start = 0, long stop = -1, RedisOrder order = RedisOrder.Ascending,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRangeByRankWithScores(key, start, stop, (Order)order, (CommandFlags)flags).ToDictionary<SortedSetEntry, byte[], double>(t => t.Element, t => t.Score);
        }

        private RedisValue[] SortedSetRangeByScore(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRangeByScore(key, start, stop, exclude, order, skip, take, flags);
        }

        public string[] SortedSetRangeByScore(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            RedisExclude exclude = RedisExclude.None, RedisOrder order = RedisOrder.Ascending, long skip = 0, long take = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRangeByScore(key, start, stop, (Exclude)exclude, (Order)order, skip, take, (CommandFlags)flags).ToStringArray();
        }

        public byte[][] SortedSetRangeByScoreBytes(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            RedisExclude exclude = RedisExclude.None, RedisOrder order = RedisOrder.Ascending, long skip = 0, long take = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRangeByScore(key, start, stop, (Exclude)exclude, (Order)order, skip, take, (CommandFlags)flags).ToBytesArray();
        }

        private SortedSetEntry[] SortedSetRangeByScoreWithScores(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRangeByScoreWithScores(key, start, stop, exclude, order, skip, take, flags);
        }

        public Dictionary<string, double> SortedSetRangeByScoreWithScores(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            RedisExclude exclude = RedisExclude.None, RedisOrder order = RedisOrder.Ascending, long skip = 0, long take = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRangeByScoreWithScores(key, start, stop, (Exclude)exclude, (Order)order, skip, take, (CommandFlags)flags).ToDictionary<SortedSetEntry, string, double>(t => t.Element, t => t.Score);
        }

        public Dictionary<byte[], double> SortedSetRangeByScoreWithScoresBytes(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            RedisExclude exclude = RedisExclude.None, RedisOrder order = RedisOrder.Ascending, long skip = 0, long take = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRangeByScoreWithScores(key, start, stop, (Exclude)exclude, (Order)order, skip, take, (CommandFlags)flags).ToDictionary<SortedSetEntry, byte[], double>(t => t.Element, t => t.Score);
        }

        private RedisValue[] SortedSetRangeByValue(RedisKey key, RedisValue min = new RedisValue(), RedisValue max = new RedisValue(),
            Exclude exclude = Exclude.None, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRangeByValue(key, min, max, exclude, skip, take, flags);
        }

        public string[] SortedSetRangeByValue(string key, string min = null, string max = null, RedisExclude exclude = RedisExclude.None, long skip = 0, long take = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRangeByValue(key, min, max, (Exclude)exclude, skip, take, (CommandFlags)flags).ToStringArray();
        }
        public byte[][] SortedSetRangeByValue(string key, byte[] min = null, byte[] max = null, RedisExclude exclude = RedisExclude.None, long skip = 0, long take = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRangeByValue(key, min, max, (Exclude)exclude, skip, take, (CommandFlags)flags).ToBytesArray();
        }

        private long? SortedSetRank(RedisKey key, RedisValue member, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRank(key, member, order, flags);
        }

        public long? SortedSetRank(string key, string member, RedisOrder order = RedisOrder.Ascending, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRank(key, member, (Order)order, (CommandFlags)flags);
        }

        public long? SortedSetRank(string key, byte[] member, RedisOrder order = RedisOrder.Ascending, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRank(key, member, (Order)order, (CommandFlags)flags);
        }

        private bool SortedSetRemove(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRemove(key, member, flags);
        }

        public bool SortedSetRemove(string key, string member, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRemove(key, member, (CommandFlags)flags);
        }

        public bool SortedSetRemove(string key, byte[] member, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRemove(key, member, (CommandFlags)flags);
        }

        private long SortedSetRemove(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRemove(key, members, flags);
        }

        public long SortedSetRemove(string key, string[] members, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRemove(key, members.ToRedisValueArray(), (CommandFlags)flags);
        }

        public long SortedSetRemove(string key, byte[][] members, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRemove(key, members.ToRedisValueArray(), (CommandFlags)flags);
        }

        private long SortedSetRemoveRangeByRank(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRemoveRangeByRank(key, start, stop, flags);
        }

        public long SortedSetRemoveRangeByRank(string key, long start, long stop, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRemoveRangeByRank(key, start, stop, (CommandFlags)flags);
        }

        private long SortedSetRemoveRangeByScore(RedisKey key, double start, double stop, Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRemoveRangeByScore(key, start, stop, exclude, flags);
        }

        public long SortedSetRemoveRangeByScore(string key, double start, double stop, RedisExclude exclude = RedisExclude.None, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRemoveRangeByScore(key, start, stop, (Exclude)exclude, (CommandFlags)flags);
        }

        private long SortedSetRemoveRangeByValue(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRemoveRangeByValue(key, min, max, exclude, flags);
        }

        public long SortedSetRemoveRangeByValue(string key, string min, string max, RedisExclude exclude = RedisExclude.None, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRemoveRangeByValue(key, min, max, (Exclude)exclude, (CommandFlags)flags);
        }

        public long SortedSetRemoveRangeByValue(string key, byte[] min, byte[] max, RedisExclude exclude = RedisExclude.None, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetRemoveRangeByValue(key, min, max, (Exclude)exclude, (CommandFlags)flags);
        }

        private IEnumerable<SortedSetEntry> SortedSetScan(RedisKey key, RedisValue pattern, int pageSize, CommandFlags flags)
        {
            var db = GetDatabase(Database);
            return db.SortedSetScan(key, pattern, pageSize, flags);
        }

        public IEnumerable<KeyValuePair<string, double>> SortedSetScan(string key, string pattern, int pageSize, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return
                SortedSetScan(key, pattern, pageSize, (CommandFlags)flags)
                    .Select(s => new KeyValuePair<string, double>(s.Element, s.Score));
        }

        public IEnumerable<KeyValuePair<byte[], double>> SortedSetScan(string key, byte[] pattern, int pageSize, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return
                SortedSetScan(key, pattern, pageSize, (CommandFlags)flags)
                    .Select(s => new KeyValuePair<byte[], double>(s.Element, s.Score));
        }

        private IEnumerable<SortedSetEntry> SortedSetScan(RedisKey key, RedisValue pattern = new RedisValue(), int pageSize = 10, long cursor = 0,
            int pageOffset = 0, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetScan(key, pattern, pageSize, cursor, pageOffset, flags);
        }

        public IEnumerable<KeyValuePair<string, double>> SortedSetScan(string key, string pattern = null, int pageSize = 10, long cursor = 0,
            int pageOffset = 0, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return
                SortedSetScan(key, pattern, pageSize, cursor, pageOffset, (CommandFlags)flags)
                    .Select(s => new KeyValuePair<string, double>(s.Element, s.Score));
        }

        public IEnumerable<KeyValuePair<byte[], double>> SortedSetScanBytes(string key, byte[] pattern = null, int pageSize = 10, long cursor = 0,
            int pageOffset = 0, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return
                SortedSetScan(key, pattern, pageSize, cursor, pageOffset, (CommandFlags)flags)
                    .Select(s => new KeyValuePair<byte[], double>(s.Element, s.Score));
        }

        private double? SortedSetScore(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetScore(key, member, flags);
        }

        public double? SortedSetScore(string key, string member, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetScore(key, member, (CommandFlags)flags);
        }

        public double? SortedSetScore(string key, byte[] member, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return SortedSetScore(key, member, (CommandFlags)flags);
        }

        private long StringBitCount(RedisKey key, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringBitCount(key, start, end, flags);
        }

        public long StringBitCount(string key, long start = 0, long end = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringBitCount(key, start, end, (CommandFlags)flags);
        }

        private long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second = new RedisKey(),
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringBitOperation(operation, destination, first, second, flags);
        }

        public long StringBitOperation(RedisBitwise opration, string destination, string first, string second = null, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringBitOperation((Bitwise)opration, destination, first, second, (CommandFlags)flags);
        }

        private long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringBitOperation(operation, destination, keys, flags);
        }

        public long StringBitOperation(RedisBitwise opration, string destination, string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringBitOperation((Bitwise)opration, destination, keys.ToRedisKeyArray(), (CommandFlags)flags);
        }

        private long StringBitPosition(RedisKey key, bool bit, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringBitPosition(key, bit, start, end, flags);
        }

        public long StringBitPosition(string key, bool bit, long start = 0, long end = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringBitPosition(key, bit, start, end, (CommandFlags)flags);
        }

        private long StringDecrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringDecrement(key, value, flags);
        }

        private double StringDecrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringDecrement(key, value, flags);
        }

        public long StringDecrement(string key, long value = 1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringDecrement(key, value, (CommandFlags)flags);
        }

        public double StringDecrement(string key, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringDecrement(key, value, (CommandFlags)flags);
        }

        /// <summary>
        /// 根据KEY获取值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private RedisValue StringGet(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringGet(key, flags);
        }

        private RedisValue[] StringGet(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringGet(keys, flags);
        }

        public string StringGet(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringGet(key, (CommandFlags)flags);
        }

        public byte[] StringGetBytes(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringGet(key, (CommandFlags)flags);
        }

        public string[] StringGet(string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringGet(keys.ToRedisKeyArray(), (CommandFlags)flags).ToStringArray();
        }

        public byte[][] StringGetBytes(string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringGet(keys.ToRedisKeyArray(), (CommandFlags)flags).ToBytesArray();
        }

        private bool StringGetBit(RedisKey key, long offset, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringGetBit(key, offset, flags);
        }

        public bool StringGetBit(string key, long offset, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringGetBit(key, offset, (CommandFlags)flags);
        }

        /// <summary>
        /// 根据KEY获取从起始位置到结束位置的值
        /// </summary>
        /// <param name="key">主键</param>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private RedisValue StringGetRange(RedisKey key, long start, long end, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringGetRange(key, start, end, flags);
        }

        public string StringGetRange(string key, long start, long end, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringGetRange(key, start, end, (CommandFlags)flags);
        }

        public byte[] StringGetRangeBytes(string key, long start, long end, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringGetRange(key, start, end, (CommandFlags)flags);
        }

        private RedisValue StringGetSet(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringGetSet(key, value, flags);
        }

        public string StringGetSet(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringGetSet(key, value, (CommandFlags)flags);
        }

        public byte[] StringGetSetBytes(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringGetSet(key, value, (CommandFlags)flags);
        }

        private RedisValueWithExpiry StringGetWithExpiry(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringGetWithExpiry(key, flags);
        }

        private long StringIncrement(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringIncrement(key, value, flags);
        }

        private double StringIncrement(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringIncrement(key, value, flags);
        }

        public long StringIncrement(string key, long value = 1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringIncrement(key, value, (CommandFlags)flags);
        }

        public double StringIncrement(string key, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringIncrement(key, value, (CommandFlags)flags);
        }

        private long StringLength(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringLength(key, flags);
        }

        public long StringLength(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringLength(key, (CommandFlags)flags);
        }

        /// <summary>
        /// 存储KEY对应值，并设置失效时间
        /// </summary>
        /// <param name="key">主键</param>
        /// <param name="value">值</param>
        /// <param name="expiry">失效时间</param>
        /// <param name="when"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private bool StringSet(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringSet(key, value, expiry, when, flags);
        }

        public bool StringSet(string key, string value, TimeSpan? expiry = null, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringSet(key, value, expiry, (When)when, (CommandFlags)flags);
        }

        public bool StringSet(string key, byte[] value, TimeSpan? expiry = null, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringSet(key, value, expiry, (When)when, (CommandFlags)flags);
        }

        /// <summary>
        /// 批量存储值
        /// </summary>
        /// <param name="values">键值集合</param>
        /// <param name="when"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        private bool StringSet(KeyValuePair<RedisKey, RedisValue>[] values, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringSet(values, when, flags);
        }

        public bool StringSet(KeyValuePair<string, string>[] values, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringSet(values.Select(s => new KeyValuePair<RedisKey, RedisValue>(s.Key, s.Value)).ToArray(), (When)when, (CommandFlags)flags);
        }

        public bool StringSet(KeyValuePair<string, byte[]>[] values, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringSet(values.Select(s => new KeyValuePair<RedisKey, RedisValue>(s.Key, s.Value)).ToArray(), (When)when, (CommandFlags)flags);
        }

        private bool StringSetBit(RedisKey key, long offset, bool bit, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringSetBit(key, offset, bit, flags);
        }

        public bool StringSetBit(string key, long offset, bool bit, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringSetBit(key, offset, bit, (CommandFlags)flags);
        }

        private RedisValue StringSetRange(RedisKey key, long offset, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringSetRange(key, offset, value, flags);
        }

        public string StringSetRange(string key, long offset, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringSetRange(key, offset, value, (CommandFlags)flags);
        }

        public byte[] StringSetRange(string key, long offset, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringSetRange(key, offset, value, (CommandFlags)flags);
        }

        /// <summary>
        /// 追加值，若存在主键，则将值加到最后，若不存在主键，则创建主键并加入空值
        /// </summary>
        /// <param name="key">主键</param>
        /// <param name="value">值</param>
        /// <param name="flags">行为值</param>
        /// <returns></returns>
        private long StringAppend(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringAppend(key, value, flags);
        }

        public long StringAppend(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringAppend(key, value, (CommandFlags)flags);
        }

        public long StringAppend(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return StringAppend(key, value, (CommandFlags)flags);
        }
    }
}