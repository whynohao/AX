// ******************************************************************************************
// * 文件名：RedisManager.Async.cs
// * 功能描述：
// * 创建人：wangjun
// * 创建日期：2016年09月18日 10:49
// *******************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace AxCRL.Comm.Redis
{
    /// <summary>
    /// 异步
    /// </summary>
    partial class RedisManager
    {
        private Task SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler,
            CommandFlags flags = CommandFlags.None)
        {
            var s = GetSubscriber();
            return s.SubscribeAsync(channel, handler, flags);
        } 

        private Task UnsubscribeAllAsync(CommandFlags flags = CommandFlags.None)
        {
            var s = GetSubscriber();
            return s.UnsubscribeAllAsync( flags);
        }

        private Task UnsubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler = null,
            CommandFlags flags = CommandFlags.None)
        {
            var s = GetSubscriber();
            return s.UnsubscribeAsync(channel, handler, flags);
        }
         
        public Task SubscribeAsync(string channel, Action<string, string> handler,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            Action<RedisChannel, RedisValue> h = (redisChannel, value) => handler(redisChannel, value);
            return SubscribeAsync(channel, h, (CommandFlags)flags);
        }
        public Task UnsubscribeAsync(string channel, Action<string, string> handler,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            Action<RedisChannel, RedisValue> h = (redisChannel, value) => handler(redisChannel, value);
            return UnsubscribeAsync(channel, h, (CommandFlags)flags);
        }

        public Task UnsubscribeAllAsync(RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return UnsubscribeAllAsync( (CommandFlags)flags);
        }

        public Task<TimeSpan> PingAsync()
        {
            var db = GetDatabase(Database);
            return db.PingAsync(CommandFlags.None);
        }

        public bool TryWait(Task task)
        {
            var db = GetDatabase(Database);
            return db.TryWait(task);
        }

        public void Wait(Task task)
        {
            var db = GetDatabase(Database);
            db.Wait(task);
        }

        public T Wait<T>(Task<T> task)
        {
            var db = GetDatabase(Database);
            return db.Wait<T>(task);
        }

        public void WaitAll(params Task[] tasks)
        {
            var db = GetDatabase(Database);
            db.WaitAll(tasks);
        }

        public TimeSpan Ping()
        {
            var db = GetDatabase(Database);
            return db.Ping(CommandFlags.None);
        }

        private Task<RedisValue> DebugObjectAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.DebugObjectAsync(key, flags);
        }

        public Task<string> DebugObjectAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => DebugObjectAsync(key, (CommandFlags)flags).Result);
        }

        private Task<long> HashDecrementAsync(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashDecrementAsync(key, hashField, value, flags);
        }

        private Task<double> HashDecrementAsync(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashDecrementAsync(key, hashField, value, flags);
        }

        public Task<long> HashDecrementAsync(string key, string hashField, long value = 1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => HashDecrementAsync(key, hashField, value, (CommandFlags)flags).Result);
        }

        public Task<long> HashDecrementAsync(string key, byte[] hashField, long value = 1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => HashDecrementAsync(key, hashField, value, (CommandFlags)flags).Result);
        }
        public Task<double> HashDecrementAsync(string key, string hashField, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<double>(() => HashDecrementAsync(key, hashField, value, (CommandFlags)flags).Result);
        }

        public Task<double> HashDecrementAsync(string key, byte[] hashField, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<double>(() => HashDecrementAsync(key, hashField, value, (CommandFlags)flags).Result);
        }

        private Task<bool> HashDeleteAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashDeleteAsync(key, hashField, flags);
        }

        public Task<bool> HashDeleteAsync(string key, string hashField, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => HashDeleteAsync(key, hashField, (CommandFlags)flags).Result);
        }
        public Task<bool> HashDeleteAsync(string key, byte[] hashField, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => HashDeleteAsync(key, hashField, (CommandFlags)flags).Result);
        }

        private Task<long> HashDeleteAsync(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashDeleteAsync(key, hashFields, flags);
        }

        public Task<long> HashDeleteAsync(string key, string[] hashFields, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => HashDeleteAsync(key, hashFields.ToRedisValueArray(), (CommandFlags)flags).Result);
        }

        public Task<long> HashDeleteAsync(string key, byte[][] hashFields, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => HashDeleteAsync(key, hashFields.ToRedisValueArray(), (CommandFlags)flags).Result);
        }

        private Task<bool> HashExistsAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashExistsAsync(key, hashField, flags);
        }

        public Task<bool> HashExistsAsync(string key, string hashField, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => HashExistsAsync(key, hashField, (CommandFlags)flags).Result);
        }

        public Task<bool> HashExistsAsync(string key, byte[] hashField, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => HashExistsAsync(key, hashField, (CommandFlags)flags).Result);
        }

        private Task<HashEntry[]> HashGetAllAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashGetAllAsync(key, flags);
        }

        public Task<Dictionary<string, string>> HashGetAllAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return
                new Task<Dictionary<string, string>>(
                    () => HashGetAllAsync(key, (CommandFlags)flags).Result.ToStringDictionary());
        }

        public Task<Dictionary<string, byte[]>> HashGetAllBytesAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return
                new Task<Dictionary<string, byte[]>>(
                    () => HashGetAllAsync(key, (CommandFlags)flags).Result.ToDictionary<HashEntry, string, byte[]>(t => t.Name, t => t.Value));
        }

        private Task<RedisValue> HashGetAsync(RedisKey key, RedisValue hashField, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashGetAsync(key, hashField, flags);
        }

        private Task<RedisValue[]> HashGetAsync(RedisKey key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashGetAsync(key, hashFields, flags);
        }

        public Task<string> HashGetAsync(string key, string hashField, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => HashGetAsync(key, hashField, (CommandFlags)flags).Result);
        }

        public Task<byte[]> HashGetBytesAsync(string key, string hashField, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[]>(() => HashGetAsync(key, hashField, (CommandFlags)flags).Result);
        }

        public Task<string[]> HashGetAsync(string key, string[] hashFields, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => HashGetAsync(key, hashFields.ToRedisValueArray(), (CommandFlags)flags).Result.ToStringArray());
        }

        public Task<byte[][]> HashGetAsync(string key, byte[][] hashFields, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => HashGetAsync(key, hashFields.ToRedisValueArray(), (CommandFlags)flags).Result.ToBytesArray());
        }

        private Task<long> HashIncrementAsync(RedisKey key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashIncrementAsync(key, hashField, value, flags);
        }

        private Task<double> HashIncrementAsync(RedisKey key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashIncrementAsync(key, hashField, value, flags);
        }

        public Task<long> HashIncrementAsync(string key, string hashField, long value = 1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => HashIncrementAsync(key, hashField, value, (CommandFlags)flags).Result);
        }
        public Task<double> HashIncrementAsync(string key, string hashField, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<double>(() => HashIncrementAsync(key, hashField, value, (CommandFlags)flags).Result);
        }
        public Task<long> HashIncrementAsync(string key, byte[] hashField, long value = 1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => HashIncrementAsync(key, hashField, value, (CommandFlags)flags).Result);
        }
        public Task<double> HashIncrementAsync(string key, byte[] hashField, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<double>(() => HashIncrementAsync(key, hashField, value, (CommandFlags)flags).Result);
        }

        private Task<RedisValue[]> HashKeysAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashKeysAsync(key, flags);
        }

        public Task<string[]> HashKeysAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => HashKeysAsync(key, (CommandFlags)flags).Result.ToStringArray());
        }

        public Task<byte[][]> HashKeysBytesAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => HashKeysAsync(key, (CommandFlags)flags).Result.ToBytesArray());
        }

        private Task<long> HashLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashLengthAsync(key, flags);
        }

        public Task<long> HashLengthAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => HashLengthAsync(key, (CommandFlags)flags).Result);
        }

        private Task HashSetAsync(RedisKey key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashSetAsync(key, hashFields, flags);
        }

        public Task HashSetAsync(string key, Dictionary<string, string> hashFields, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task(() => HashSetAsync(key, hashFields.Select(s => new HashEntry(s.Key, s.Value)).ToArray(), (CommandFlags)flags));
        }

        public Task HashSetAsync(string key, Dictionary<string, byte[]> hashFields, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task(() => HashSetAsync(key, hashFields.Select(s => new HashEntry(s.Key, s.Value)).ToArray(), (CommandFlags)flags));
        }

        private Task<bool> HashSetAsync(RedisKey key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashSetAsync(key, hashField, value, when, flags);
        }

        public Task<bool> HashSetAsync(string key, string hashField, string value, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => HashSetAsync(key, hashField, value, (When)when, (CommandFlags)flags).Result);
        }

        public Task<bool> HashSetAsync(string key, byte[] hashField, byte[] value, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => HashSetAsync(key, hashField, value, (When)when, (CommandFlags)flags).Result);
        }

        private Task<RedisValue[]> HashValuesAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HashValuesAsync(key, flags);
        }

        public Task<string[]> HashValuesAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => HashValuesAsync(key, (CommandFlags)flags).Result.ToStringArray());
        }

        public Task<byte[][]> HashValuesBytesAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => HashValuesAsync(key, (CommandFlags)flags).Result.ToBytesArray());
        }

        private Task<bool> HyperLogLogAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HyperLogLogAddAsync(key, value, flags);
        }

        public Task<bool> HyperLogLogAddAsync(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => HyperLogLogAddAsync(key, value, (CommandFlags)flags).Result);
        }

        public Task<bool> HyperLogLogAddAsync(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => HyperLogLogAddAsync(key, value, (CommandFlags)flags).Result);
        }

        private Task<bool> HyperLogLogAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HyperLogLogAddAsync(key, values, flags);
        }

        public Task<bool> HyperLogLogAddAsync(string key, string[] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => HyperLogLogAddAsync(key, values.ToRedisValueArray(), (CommandFlags)flags).Result);
        }

        public Task<bool> HyperLogLogAddAsync(string key, byte[][] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => HyperLogLogAddAsync(key, values.ToRedisValueArray(), (CommandFlags)flags).Result);
        }

        private Task<long> HyperLogLogLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HyperLogLogLengthAsync(key, flags);
        }

        public Task<long> HyperLogLogLengthAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => HyperLogLogLengthAsync(key, (CommandFlags)flags).Result);
        }

        private Task<long> HyperLogLogLengthAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HyperLogLogLengthAsync(keys, flags);
        }

        public Task<long> HyperLogLogLengthAsync(string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => HyperLogLogLengthAsync(keys.ToRedisKeyArray(), (CommandFlags)flags).Result);
        }

        private Task HyperLogLogMergeAsync(RedisKey destination, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HyperLogLogMergeAsync(destination, first, second, flags);
        }

        public Task HyperLogLogMergeAsync(string destination, string first, string second, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task(() => HyperLogLogMergeAsync(destination, first, second, (CommandFlags)flags));
        }

        private Task HyperLogLogMergeAsync(RedisKey destination, RedisKey[] sourceKeys, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.HyperLogLogMergeAsync(destination, sourceKeys, flags);
        }

        public Task HyperLogLogMergeAsync(string destination, string[] sourceKeys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task(() => HyperLogLogMergeAsync(destination, sourceKeys.ToRedisKeyArray(), (CommandFlags)flags));
        }

        private Task<EndPoint> IdentifyEndpointAsync(RedisKey key = new RedisKey(), CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.IdentifyEndpointAsync(key, flags);
        }

        public Task<EndPoint> IdentifyEndpointAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<EndPoint>(() => IdentifyEndpointAsync(key, (CommandFlags)flags).Result);
        }

        private bool IsConnected(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.IsConnected(key, flags);
        }

        public bool IsConnected(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return IsConnected(key, (CommandFlags)flags);
        }

        private Task<bool> KeyDeleteAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyDeleteAsync(key, flags);
        }

        private Task<long> KeyDeleteAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyDeleteAsync(keys, flags);
        }

        public Task<bool> KeyDeleteAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => KeyDeleteAsync(key, (CommandFlags)flags).Result);
        }

        public Task<long> KeyDeleteAsync(string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => KeyDeleteAsync(keys.ToRedisKeyArray(), (CommandFlags)flags).Result);
        }

        private Task<byte[]> KeyDumpAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyDumpAsync(key, flags);
        }

        public Task<byte[]> KeyDumpAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[]>(() => KeyDumpAsync(key, (CommandFlags)flags).Result);
        }

        private Task<bool> KeyExistsAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyExistsAsync(key, flags);
        }

        public Task<bool> KeyExistsAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => KeyExistsAsync(key, (CommandFlags)flags).Result);
        }

        private Task<bool> KeyExpireAsync(RedisKey key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyExpireAsync(key, expiry, flags);
        }

        public Task<bool> KeyExpireAsync(string key, TimeSpan? expiry, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => KeyExpireAsync(key, expiry, (CommandFlags)flags).Result);
        }

        private Task<bool> KeyExpireAsync(RedisKey key, DateTime? expiry, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyExpireAsync(key, expiry, flags);
        }

        public Task<bool> KeyExpireAsync(string key, DateTime? expiry, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => KeyExpireAsync(key, expiry, (CommandFlags)flags).Result);
        }

        private Task KeyMigrateAsync(RedisKey key, EndPoint toServer, int toDatabase = 0, int timeoutMilliseconds = 0,
            MigrateOptions migrateOptions = MigrateOptions.None, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyMigrateAsync(key, toServer, toDatabase, timeoutMilliseconds, migrateOptions, flags);
        }

        public Task KeyMigrateAsync(string key, EndPoint toServer, int toDatabase = 0, int timeoutMilliseconds = 0, RedisMigrateOptions migrateOptions = RedisMigrateOptions.None, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task(() => KeyMigrateAsync(key, toServer, toDatabase, timeoutMilliseconds, (MigrateOptions)migrateOptions, (CommandFlags)flags));
        }

        private Task<bool> KeyMoveAsync(RedisKey key, int database, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyMoveAsync(key, database, flags);
        }

        public Task<bool> KeyMoveAsync(string key, int database, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => KeyMoveAsync(key, database, (CommandFlags)flags).Result);
        }

        private Task<bool> KeyPersistAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyPersistAsync(key, flags);
        }

        public Task<bool> KeyPersistAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => KeyPersistAsync(key, (CommandFlags)flags).Result);
        }

        private Task<RedisKey> KeyRandomAsync(CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyRandomAsync(flags);
        }

        public Task<string> KeyRandomAsync(RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => KeyRandomAsync((CommandFlags)flags).Result);
        }

        private Task<bool> KeyRenameAsync(RedisKey key, RedisKey newKey, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyRenameAsync(key, newKey, when, flags);
        }

        public Task<bool> KeyRenameAsync(string key, string newKey, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => KeyRenameAsync(key, newKey, (When)when, (CommandFlags)flags).Result);
        }

        private Task KeyRestoreAsync(RedisKey key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyRestoreAsync(key, value, expiry, flags);
        }

        public Task KeyRestoreAsync(string key, byte[] value, TimeSpan? expiry = null, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task(() => KeyRestoreAsync(key, value, expiry, (CommandFlags)flags));
        }

        private Task<TimeSpan?> KeyTimeToLiveAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyTimeToLiveAsync(key, flags);
        }

        public Task<TimeSpan?> KeyTimeToLiveAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<TimeSpan?>(() => KeyTimeToLiveAsync(key, (CommandFlags)flags).Result);
        }

        private Task<RedisType> KeyTypeAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.KeyTypeAsync(key, flags);
        }

        private Task<RedisValue> ListGetByIndexAsync(RedisKey key, long index, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListGetByIndexAsync(key, index, flags);
        }

        public Task<string> ListGetByIndexAsync(string key, long index, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => ListGetByIndexAsync(key, index, (CommandFlags)flags).Result);
        }

        public Task<byte[]> ListGetByIndexBytesAsync(string key, long index, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[]>(() => ListGetByIndexAsync(key, index, (CommandFlags)flags).Result);
        }

        private Task<long> ListInsertAfterAsync(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListInsertAfterAsync(key, pivot, value, flags);
        }

        public Task<long> ListInsertAfterAsync(string key, string pivot, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListInsertAfterAsync(key, pivot, value, (CommandFlags)flags).Result);
        }

        public Task<long> ListInsertAfterAsync(string key, byte[] pivot, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListInsertAfterAsync(key, pivot, value, (CommandFlags)flags).Result);
        }

        private Task<long> ListInsertBeforeAsync(RedisKey key, RedisValue pivot, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListInsertBeforeAsync(key, pivot, value, flags);
        }

        public Task<long> ListInsertBeforeAsync(string key, string pivot, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListInsertBeforeAsync(key, pivot, value, (CommandFlags)flags).Result);
        }

        public Task<long> ListInsertBeforeAsync(string key, byte[] pivot, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListInsertBeforeAsync(key, pivot, value, (CommandFlags)flags).Result);
        }

        private Task<RedisValue> ListLeftPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListLeftPopAsync(key, flags);
        }

        public Task<string> ListLeftPopAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => ListLeftPopAsync(key, (CommandFlags)flags).Result);
        }

        public Task<byte[]> ListLeftPopBytesAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[]>(() => ListLeftPopAsync(key, (CommandFlags)flags).Result);
        }

        private Task<long> ListLeftPushAsync(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListLeftPushAsync(key, value, when, flags);
        }

        private Task<long> ListLeftPushAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListLeftPushAsync(key, values, flags);
        }

        public Task<long> ListLeftPushAsync(string key, string value, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListLeftPushAsync(key, value, (When)when, (CommandFlags)flags).Result);
        }

        public Task<long> ListLeftPushAsync(string key, byte[] value, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListLeftPushAsync(key, value, (When)when, (CommandFlags)flags).Result);
        }

        public Task<long> ListLeftPushAsync(string key, string[] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListLeftPushAsync(key, values.ToRedisValueArray(), (CommandFlags)flags).Result);
        }

        public Task<long> ListLeftPushAsync(string key, byte[][] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListLeftPushAsync(key, values.ToRedisValueArray(), (CommandFlags)flags).Result);
        }

        private Task<long> ListLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListLengthAsync(key, flags);
        }

        public Task<long> ListLengthAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListLengthAsync(key, (CommandFlags)flags).Result);
        }

        private Task<RedisValue[]> ListRangeAsync(RedisKey key, long start = 0, long stop = -1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListRangeAsync(key, start, stop, flags);
        }

        public Task<string[]> ListRangeAsync(string key, long start = 0, long stop = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => ListRangeAsync(key, start, stop, (CommandFlags)flags).Result.ToStringArray());
        }

        public Task<byte[][]> ListRangeBytesAsync(string key, long start = 0, long stop = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => ListRangeAsync(key, start, stop, (CommandFlags)flags).Result.ToBytesArray());
        }

        private Task<long> ListRemoveAsync(RedisKey key, RedisValue value, long count = 0, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListRemoveAsync(key, value, count, flags);
        }

        public Task<long> ListRemoveAsync(string key, string value, long count = 0, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListRemoveAsync(key, value, count, (CommandFlags)flags).Result);
        }

        public Task<long> ListRemoveAsync(string key, byte[] value, long count = 0, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListRemoveAsync(key, value, count, (CommandFlags)flags).Result);
        }

        private Task<RedisValue> ListRightPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListRightPopAsync(key, flags);
        }

        public Task<string> ListRightPopAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => ListRightPopAsync(key, (CommandFlags)flags).Result);
        }

        public Task<byte[]> ListRightPopBytesAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[]>(() => ListRightPopAsync(key, (CommandFlags)flags).Result);
        }

        private Task<RedisValue> ListRightPopLeftPushAsync(RedisKey source, RedisKey destination, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListRightPopLeftPushAsync(source, destination, flags);
        }

        public Task<string> ListRightPopLeftPushAsync(string source, string destination, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => ListRightPopLeftPushAsync(source, destination, (CommandFlags)flags).Result);
        }

        public Task<byte[]> ListRightPopLeftPushBytesAsync(string source, string destination, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[]>(() => ListRightPopLeftPushAsync(source, destination, (CommandFlags)flags).Result);
        }

        private Task<long> ListRightPushAsync(RedisKey key, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListRightPushAsync(key, value, when, flags);
        }

        private Task<long> ListRightPushAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListRightPushAsync(key, values, flags);
        }

        public Task<long> ListRightPushAsync(string key, string value, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListRightPushAsync(key, value, (When)when, (CommandFlags)flags).Result);
        }

        public Task<long> ListRightPushAsync(string key, byte[] value, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListRightPushAsync(key, value, (When)when, (CommandFlags)flags).Result);
        }

        public Task<long> ListRightPushAsync(string key, string[] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListRightPushAsync(key, values.ToRedisValueArray(), (CommandFlags)flags).Result);
        }

        public Task<long> ListRightPushAsync(string key, byte[][] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => ListRightPushAsync(key, values.ToRedisValueArray(), (CommandFlags)flags).Result);
        }

        private Task ListSetByIndexAsync(RedisKey key, long index, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListSetByIndexAsync(key, index, value, flags);
        }

        public Task ListSetByIndexAsync(string key, long index, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task(() => ListSetByIndexAsync(key, index, value, (CommandFlags)flags));
        }

        public Task ListSetByIndexAsync(string key, long index, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task(() => ListSetByIndexAsync(key, index, value, (CommandFlags)flags));
        }

        private Task ListTrimAsync(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ListTrimAsync(key, start, stop, flags);
        }

        public Task ListTrimAsync(string key, long start, long stop, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task(() => ListTrimAsync(key, start, stop, (CommandFlags)flags));
        }

        private Task<bool> LockExtendAsync(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.LockExtendAsync(key, value, expiry, flags);
        }

        public Task<bool> LockExtendAsync(string key, string value, TimeSpan expiry, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => LockExtendAsync(key, value, expiry, (CommandFlags)flags).Result);
        }

        public Task<bool> LockExtendAsync(string key, byte[] value, TimeSpan expiry, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => LockExtendAsync(key, value, expiry, (CommandFlags)flags).Result);
        }

        private Task<RedisValue> LockQueryAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.LockQueryAsync(key, flags);
        }

        public Task<string> LockQueryAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => LockQueryAsync(key, (CommandFlags)flags).Result);
        }

        public Task<byte[]> LockQueryBytesAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[]>(() => LockQueryAsync(key, (CommandFlags)flags).Result);
        }

        private Task<bool> LockReleaseAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.LockReleaseAsync(key, value, flags);
        }

        public Task<bool> LockReleaseAsync(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => LockReleaseAsync(key, value, (CommandFlags)flags).Result);
        }

        public Task<bool> LockReleaseAsync(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => LockReleaseAsync(key, value, (CommandFlags)flags).Result);
        }

        private Task<bool> LockTakeAsync(RedisKey key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.LockTakeAsync(key, value, expiry, flags);
        }

        public Task<bool> LockTakeAsync(string key, string value, TimeSpan expiry, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => LockTakeAsync(key, value, expiry, (CommandFlags)flags).Result);
        }

        public Task<bool> LockTakeAsync(string key, byte[] value, TimeSpan expiry, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => LockTakeAsync(key, value, expiry, (CommandFlags)flags).Result);
        }

        private Task<long> PublishAsync(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.PublishAsync(channel, message, flags);
        }

        public Task<long> PublishAsync(string channel, string message, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => PublishAsync(channel, message, (CommandFlags)flags).Result);
        }

        public Task<long> PublishAsync(string channel, byte[] message, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => PublishAsync(channel, message, (CommandFlags)flags).Result);
        }

        private Task<RedisResult> ScriptEvaluateAsync(string script, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ScriptEvaluateAsync(script, keys, values, flags);
        }

        private Task<RedisResult> ScriptEvaluateAsync(byte[] hash, RedisKey[] keys = null, RedisValue[] values = null, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ScriptEvaluateAsync(hash, keys, values, flags);
        }

        private Task<RedisResult> ScriptEvaluateAsync(LuaScript script, object parameters = null, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ScriptEvaluateAsync(script, parameters, flags);
        }

        private Task<RedisResult> ScriptEvaluateAsync(LoadedLuaScript script, object parameters = null, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.ScriptEvaluateAsync(script, parameters, flags);
        }

        private Task<bool> SetAddAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetAddAsync(key, value, flags);
        }

        private Task<long> SetAddAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetAddAsync(key, values, flags);
        }

        public Task<bool> SetAddAsync(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => SetAddAsync(key, value, (CommandFlags)flags).Result);
        }

        public Task<bool> SetAddAsync(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => SetAddAsync(key, value, (CommandFlags)flags).Result);
        }

        private Task<long> SetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetCombineAndStoreAsync(operation, destination, first, second, flags);
        }

        private Task<long> SetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetCombineAndStoreAsync(operation, destination, keys, flags);
        }

        public Task<long> SetCombineAndStoreAsync(RedisSetOperation operation, string destination, string first, string second, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SetCombineAndStoreAsync((SetOperation)operation, destination, first, second, (CommandFlags)flags).Result);
        }

        public Task<long> SetCombineAndStoreAsync(RedisSetOperation operation, string destination, string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SetCombineAndStoreAsync((SetOperation)operation, destination, keys.ToRedisKeyArray(), (CommandFlags)flags).Result);
        }

        private Task<RedisValue[]> SetCombineAsync(SetOperation operation, RedisKey first, RedisKey second, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetCombineAsync(operation, first, second, flags);
        }

        private Task<RedisValue[]> SetCombineAsync(SetOperation operation, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetCombineAsync(operation, keys, flags);
        }

        public Task<string[]> SetCombineAsync(RedisSetOperation operation, string first, string second, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => SetCombineAsync((SetOperation)operation, first, second, (CommandFlags)flags).Result.ToStringArray());
        }

        public Task<byte[][]> SetCombineBytesAsync(RedisSetOperation operation, string first, string second, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => SetCombineAsync((SetOperation)operation, first, second, (CommandFlags)flags).Result.ToBytesArray());
        }

        public Task<string[]> SetCombineAsync(RedisSetOperation operation, string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => SetCombineAsync((SetOperation)operation, keys.ToRedisKeyArray(), (CommandFlags)flags).Result.ToStringArray());
        }

        public Task<byte[][]> SetCombineBytesAsync(RedisSetOperation operation, string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => SetCombineAsync((SetOperation)operation, keys.ToRedisKeyArray(), (CommandFlags)flags).Result.ToBytesArray());
        }

        private Task<bool> SetContainsAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetContainsAsync(key, value, flags);
        }

        public Task<bool> SetContainsAsync(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => SetContainsAsync(key, value, (CommandFlags)flags).Result);
        }

        public Task<bool> SetContainsAsync(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => SetContainsAsync(key, value, (CommandFlags)flags).Result);
        }

        private Task<long> SetLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetLengthAsync(key, flags);
        }

        public Task<long> SetLengthAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SetLengthAsync(key, (CommandFlags)flags).Result);
        }

        private Task<RedisValue[]> SetMembersAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetMembersAsync(key, flags);
        }

        public Task<string[]> SetMembersAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => SetMembersAsync(key, (CommandFlags)flags).Result.ToStringArray());
        }

        public Task<byte[][]> SetMembersBytesAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => SetMembersAsync(key, (CommandFlags)flags).Result.ToBytesArray());
        }

        private Task<bool> SetMoveAsync(RedisKey source, RedisKey destination, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetMoveAsync(source, destination, value, flags);
        }

        public Task<bool> SetMoveAsync(string source, string destination, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => SetMoveAsync(source, destination, value, (CommandFlags)flags).Result);
        }

        public Task<bool> SetMoveAsync(string source, string destination, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => SetMoveAsync(source, destination, value, (CommandFlags)flags).Result);
        }

        private Task<RedisValue> SetPopAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetPopAsync(key, flags);
        }

        public Task<string> SetPopAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => SetPopAsync(key, (CommandFlags)flags).Result);
        }

        public Task<byte[]> SetPopBytesAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[]>(() => SetPopAsync(key, (CommandFlags)flags).Result);
        }

        private Task<RedisValue> SetRandomMemberAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetRandomMemberAsync(key, flags);
        }

        private Task<RedisValue[]> SetRandomMembersAsync(RedisKey key, long count, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetRandomMembersAsync(key, count, flags);
        }

        public Task<string> SetRandomMemberAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => SetRandomMemberAsync(key, (CommandFlags)flags).Result);
        }

        public Task<byte[]> SetRandomMemberBytesAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[]>(() => SetRandomMemberAsync(key, (CommandFlags)flags).Result);
        }

        public Task<string[]> SetRandomMembersAsync(string key, long count, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => SetRandomMembersAsync(key, count, (CommandFlags)flags).Result.ToStringArray());
        }

        public Task<byte[][]> SetRandomMembersBytesAsync(string key, long count, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => SetRandomMembersAsync(key, count, (CommandFlags)flags).Result.ToBytesArray());
        }

        private Task<bool> SetRemoveAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetRemoveAsync(key, value, flags);
        }

        private Task<long> SetRemoveAsync(RedisKey key, RedisValue[] values, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SetRemoveAsync(key, values, flags);
        }

        public Task<bool> SetRemoveAsync(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => SetRemoveAsync(key, value, (CommandFlags)flags).Result);
        }

        public Task<bool> SetRemoveAsync(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => SetRemoveAsync(key, value, (CommandFlags)flags).Result);
        }

        public Task<long> SetRemoveAsync(string key, string[] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SetRemoveAsync(key, values.ToRedisValueArray(), (CommandFlags)flags).Result);
        }

        public Task<long> SetRemoveAsync(string key, byte[][] values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SetRemoveAsync(key, values.ToRedisValueArray(), (CommandFlags)flags).Result);
        }

        private Task<long> SortAndStoreAsync(RedisKey destination, RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending,
            SortType sortType = SortType.Numeric, RedisValue @by = new RedisValue(), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortAndStoreAsync(destination, key, skip, take, order, sortType, @by, get, flags);
        }

        public Task<long> SortAndStoreAsync(string destination, string key, long skip = 0, long take = -1, RedisOrder order = RedisOrder.Ascending, RedisSortType sortType = RedisSortType.Numeric,
            string @by = null, string[] get = null, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortAndStoreAsync(destination, key, skip, take, (Order)order, (SortType)sortType, @by, get.ToRedisValueArray(), (CommandFlags)flags).Result);
        }
        public Task<long> SortAndStoreAsync(string destination, string key, long skip = 0, long take = -1, RedisOrder order = RedisOrder.Ascending, RedisSortType sortType = RedisSortType.Numeric,
          byte[] @by = null, byte[][] get = null, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortAndStoreAsync(destination, key, skip, take, (Order)order, (SortType)sortType, @by, get.ToRedisValueArray(), (CommandFlags)flags).Result);
        }

        private Task<RedisValue[]> SortAsync(RedisKey key, long skip = 0, long take = -1, Order order = Order.Ascending, SortType sortType = SortType.Numeric,
            RedisValue @by = new RedisValue(), RedisValue[] get = null, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortAsync(key, skip, take, order, sortType, @by, get, flags);
        }

        public Task<string[]> SortAsync(string key, long skip = 0, long take = -1, RedisOrder order = RedisOrder.Ascending, RedisSortType sortType = RedisSortType.Numeric,
            string @by = null, string[] get = null, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => SortAsync(key, skip, take, (Order)order, (SortType)sortType, @by, get.ToRedisValueArray(), (CommandFlags)flags).Result.ToStringArray());
        }
        public Task<byte[][]> SortAsync(string key, long skip = 0, long take = -1, RedisOrder order = RedisOrder.Ascending, RedisSortType sortType = RedisSortType.Numeric,
          byte[] @by = null, byte[][] get = null, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => SortAsync(key, skip, take, (Order)order, (SortType)sortType, @by, get.ToRedisValueArray(), (CommandFlags)flags).Result.ToBytesArray());
        }

        private Task<bool> SortedSetAddAsync(RedisKey key, RedisValue member, double score, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetAddAsync(key, member, score, flags);
        }

        private Task<long> SortedSetAddAsync(RedisKey key, SortedSetEntry[] values, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetAddAsync(key, values, flags);
        }

        public Task<bool> SortedSetAddAsync(string key, string member, double score, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => SortedSetAddAsync(key, member, score, (CommandFlags)flags).Result);
        }
        public Task<bool> SortedSetAddAsync(string key, byte[] member, double score, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => SortedSetAddAsync(key, member, score, (CommandFlags)flags).Result);
        }

        public Task<long> SortedSetAddAsync(string key, Dictionary<string, double> values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(()=> SortedSetAddAsync(key, values.Select(value => new SortedSetEntry(value.Key, value.Value)).ToArray(), (CommandFlags)flags).Result);
        }

        public Task<long> SortedSetAddAsync(string key, Dictionary<byte[], double> values, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortedSetAddAsync(key, values.Select(value => new SortedSetEntry(value.Key, value.Value)).ToArray(), (CommandFlags)flags).Result);
        }

        private Task<long> SortedSetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey first, RedisKey second,
            Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetCombineAndStoreAsync(operation, destination, first, second, aggregate, flags);
        }

        private Task<long> SortedSetCombineAndStoreAsync(SetOperation operation, RedisKey destination, RedisKey[] keys,
            double[] weights = null, Aggregate aggregate = Aggregate.Sum, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetCombineAndStoreAsync(operation, destination, keys, weights, aggregate, flags);
        }

        public Task<long> SortedSetCombineAndStoreAsync(RedisSetOperation operation, string destination, string first, string second, RedisAggregate aggregate = RedisAggregate.Sum, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortedSetCombineAndStoreAsync((SetOperation)operation, destination, first, second, (Aggregate)aggregate, (CommandFlags)flags).Result);
        }

        public Task<long> SortedSetCombineAndStoreAsync(RedisSetOperation operation, string destination, string[] keys, double[] weights = null, RedisAggregate aggregate = RedisAggregate.Sum, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortedSetCombineAndStoreAsync((SetOperation)operation, destination, keys.ToRedisKeyArray(), weights, (Aggregate)aggregate, (CommandFlags)flags).Result);
        }

        private Task<double> SortedSetDecrementAsync(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetDecrementAsync(key, member, value, flags);
        }

        public Task<double> SortedSetDecrementAsync(string key, string member, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<double>(() => SortedSetDecrementAsync(key, member, value, (CommandFlags)flags).Result);
        }

        public Task<double> SortedSetDecrementAsync(string key, byte[] member, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<double>(() => SortedSetDecrementAsync(key, member, value, (CommandFlags)flags).Result);
        }

        private Task<double> SortedSetIncrementAsync(RedisKey key, RedisValue member, double value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetIncrementAsync(key, member, value, flags);
        }

        public Task<double> SortedSetIncrementAsync(string key, string member, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<double>(() => SortedSetIncrementAsync(key, member, value, (CommandFlags)flags).Result);
        }

        public Task<double> SortedSetIncrementAsync(string key, byte[] member, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<double>(() => SortedSetIncrementAsync(key, member, value, (CommandFlags)flags).Result);
        }

        private Task<long> SortedSetLengthAsync(RedisKey key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetLengthAsync(key, min, max, exclude, flags);
        }

        public Task<long> SortedSetLengthAsync(string key, double min = double.NegativeInfinity, double max = double.PositiveInfinity, RedisExclude exclude = RedisExclude.None,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortedSetLengthAsync(key, min, max, (Exclude)exclude, (CommandFlags)flags).Result);
        }

        private Task<long> SortedSetLengthByValueAsync(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetLengthByValueAsync(key, min, max, exclude, flags);
        }

        public Task<long> SortedSetLengthByValueAsync(string key, string min, string max, RedisExclude exclude = RedisExclude.None,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortedSetLengthByValueAsync(key, min, max, (Exclude)exclude, (CommandFlags)flags).Result);
        }

        public Task<long> SortedSetLengthByValueAsync(string key, byte[] min, byte[] max, RedisExclude exclude = RedisExclude.None,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortedSetLengthByValueAsync(key, min, max, (Exclude)exclude, (CommandFlags)flags).Result);
        }


        private Task<RedisValue[]> SortedSetRangeByRankAsync(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRangeByRankAsync(key, start, stop, order, flags);
        }

        public Task<string[]> SortedSetRangeByRankAsync(string key, long start = 0, long stop = -1, RedisOrder order = RedisOrder.Ascending,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => SortedSetRangeByRankAsync(key, start, stop, (Order)order, (CommandFlags)flags).Result.ToStringArray());
        }

        public Task<byte[][]> SortedSetRangeByRankBytesAsync(string key, long start = 0, long stop = -1, RedisOrder order = RedisOrder.Ascending,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => SortedSetRangeByRankAsync(key, start, stop, (Order)order, (CommandFlags)flags).Result.ToBytesArray());
        }

        private Task<SortedSetEntry[]> SortedSetRangeByRankWithScoresAsync(RedisKey key, long start = 0, long stop = -1, Order order = Order.Ascending,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRangeByRankWithScoresAsync(key, start, stop, order, flags);
        }

        public Task<Dictionary<string, double>> SortedSetRangeByRankWithScoresAsync(string key, long start = 0, long stop = -1, RedisOrder order = RedisOrder.Ascending,
            RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<Dictionary<string, double>>(() => SortedSetRangeByRankWithScoresAsync(key, start, stop, (Order)order, (CommandFlags)flags).Result.ToDictionary<SortedSetEntry, string, double>(t => t.Element, t => t.Score));
        }

        private Task<RedisValue[]> SortedSetRangeByScoreAsync(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity, Exclude exclude = Exclude.None,
            Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRangeByScoreAsync(key, start, stop, exclude, order, skip, take, flags);
        }

        public Task<string[]> SortedSetRangeByScoreAsync(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            RedisExclude exclude = RedisExclude.None, RedisOrder order = RedisOrder.Ascending, long skip = 0, long take = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => SortedSetRangeByScoreAsync(key, start, stop, (Exclude)exclude, (Order)order, skip, take, (CommandFlags)flags).Result.ToStringArray());
        }

        public Task<byte[][]> SortedSetRangeByScoreBytesAsync(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            RedisExclude exclude = RedisExclude.None, RedisOrder order = RedisOrder.Ascending, long skip = 0, long take = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => SortedSetRangeByScoreAsync(key, start, stop, (Exclude)exclude, (Order)order, skip, take, (CommandFlags)flags).Result.ToBytesArray());
        }

        private Task<SortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(RedisKey key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            Exclude exclude = Exclude.None, Order order = Order.Ascending, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRangeByScoreWithScoresAsync(key, start, stop, exclude, order, skip, take, flags);
        }

        public Task<Dictionary<string, double>> SortedSetRangeByScoreWithScoresAsync(string key, double start = double.NegativeInfinity, double stop = double.PositiveInfinity,
            RedisExclude exclude = RedisExclude.None, RedisOrder order = RedisOrder.Ascending, long skip = 0, long take = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<Dictionary<string, double>>(() => SortedSetRangeByScoreWithScoresAsync(key, start, stop, (Exclude)exclude, (Order)order, skip, take, (CommandFlags)flags).Result.ToDictionary<SortedSetEntry, string, double>(t => t.Element, t => t.Score));
        }

        private Task<RedisValue[]> SortedSetRangeByValueAsync(RedisKey key, RedisValue min = new RedisValue(), RedisValue max = new RedisValue(),
            Exclude exclude = Exclude.None, long skip = 0, long take = -1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRangeByValueAsync(key, min, max, exclude, skip, take, flags);
        }

        public Task<string[]> SortedSetRangeByValueAsync(string key, string min = null, string max = null, RedisExclude exclude = RedisExclude.None, long skip = 0, long take = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => SortedSetRangeByValueAsync(key, min, max, (Exclude)exclude, skip, take, (CommandFlags)flags).Result.ToStringArray());
        }

        public Task<byte[][]> SortedSetRangeByValueAsync(string key, byte[] min = null, byte[] max = null, RedisExclude exclude = RedisExclude.None, long skip = 0, long take = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => SortedSetRangeByValueAsync(key, min, max, (Exclude)exclude, skip, take, (CommandFlags)flags).Result.ToBytesArray());
        }

        private Task<long?> SortedSetRankAsync(RedisKey key, RedisValue member, Order order = Order.Ascending, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRankAsync(key, member, order, flags);
        }

        public Task<long?> SortedSetRankAsync(string key, string member, RedisOrder order = RedisOrder.Ascending, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long?>(() => SortedSetRankAsync(key, member, (Order)order, (CommandFlags)flags).Result);
        }

        public Task<long?> SortedSetRankAsync(string key, byte[] member, RedisOrder order = RedisOrder.Ascending, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long?>(() => SortedSetRankAsync(key, member, (Order)order, (CommandFlags)flags).Result);
        }

        private Task<bool> SortedSetRemoveAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRemoveAsync(key, member, flags);
        }

        private Task<long> SortedSetRemoveAsync(RedisKey key, RedisValue[] members, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRemoveAsync(key, members, flags);
        }

        public Task<bool> SortedSetRemoveAsync(string key, string member, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => SortedSetRemoveAsync(key, member, (CommandFlags)flags).Result);
        }

        public Task<bool> SortedSetRemoveAsync(string key, byte[] member, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => SortedSetRemoveAsync(key, member, (CommandFlags)flags).Result);
        }

        public Task<long> SortedSetRemoveAsync(string key, string[] members, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(()=> SortedSetRemoveAsync(key, members.ToRedisValueArray(), (CommandFlags)flags).Result); 
        }

        public Task<long> SortedSetRemoveAsync(string key, byte[][] members, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortedSetRemoveAsync(key, members.ToRedisValueArray(), (CommandFlags)flags).Result);
        }

        private Task<long> SortedSetRemoveRangeByRankAsync(RedisKey key, long start, long stop, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRemoveRangeByRankAsync(key, start, stop, flags);
        }

        public Task<long> SortedSetRemoveRangeByRankAsync(string key, long start, long stop, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortedSetRemoveRangeByRankAsync(key, start, stop, (CommandFlags)flags).Result);
        }

        private Task<long> SortedSetRemoveRangeByScoreAsync(RedisKey key, double start, double stop, Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRemoveRangeByScoreAsync(key, start, stop, exclude, flags);
        }

        public Task<long> SortedSetRemoveRangeByScoreAsync(string key, double start, double stop, RedisExclude exclude = RedisExclude.None, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortedSetRemoveRangeByScoreAsync(key, start, stop, (Exclude)exclude, (CommandFlags)flags).Result);
        }

        private Task<long> SortedSetRemoveRangeByValueAsync(RedisKey key, RedisValue min, RedisValue max, Exclude exclude = Exclude.None,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetRemoveRangeByValueAsync(key, min, max, exclude, flags);
        }

        public Task<long> SortedSetRemoveRangeByValueAsync(string key, string min, string max, RedisExclude exclude = RedisExclude.None, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortedSetRemoveRangeByValueAsync(key, min, max, (Exclude)exclude, (CommandFlags)flags).Result);
        }

        public Task<long> SortedSetRemoveRangeByValueAsync(string key, byte[] min, byte[] max, RedisExclude exclude = RedisExclude.None, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => SortedSetRemoveRangeByValueAsync(key, min, max, (Exclude)exclude, (CommandFlags)flags).Result);
        }

        private Task<double?> SortedSetScoreAsync(RedisKey key, RedisValue member, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.SortedSetScoreAsync(key, member, flags);
        }

        public Task<double?> SortedSetScoreAsync(string key, string member, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<double?>(() => SortedSetScoreAsync(key, member, (CommandFlags)flags).Result);
        }

        public Task<double?> SortedSetScoreAsync(string key, byte[] member, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<double?>(() => SortedSetScoreAsync(key, member, (CommandFlags)flags).Result);
        }

        private Task<long> StringAppendAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringAppendAsync(key, value, flags);
        }

        public Task<long> StringAppendAsync(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => StringAppendAsync(key, value, (CommandFlags)flags).Result);
        }

        public Task<long> StringAppendAsync(string key, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => StringAppendAsync(key, value, (CommandFlags)flags).Result);
        }

        private Task<long> StringBitCountAsync(RedisKey key, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringBitCountAsync(key, start, end, flags);
        }

        public Task<long> StringBitCountAsync(string key, long start = 0, long end = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => StringBitCountAsync(key, start, end, (CommandFlags)flags).Result);
        }

        private Task<long> StringBitOperationAsync(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second = new RedisKey(),
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringBitOperationAsync(operation, destination, first, second, flags);
        }

        public Task<long> StringBitOperationAsync(RedisBitwise opration, string destination, string first, string second = null, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => StringBitOperationAsync((Bitwise)opration, destination, first, second, (CommandFlags)flags).Result);
        }

        private Task<long> StringBitOperationAsync(Bitwise operation, RedisKey destination, RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringBitOperationAsync(operation, destination, keys, flags);
        }

        public Task<long> StringBitOperationAsync(RedisBitwise opration, string destination, string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => StringBitOperationAsync((Bitwise)opration, destination, keys.ToRedisKeyArray(), (CommandFlags)flags).Result);
        }

        private Task<long> StringBitPositionAsync(RedisKey key, bool bit, long start = 0, long end = -1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringBitPositionAsync(key, bit, start, end, flags);
        }

        public Task<long> StringBitPositionAsync(string key, bool bit, long start = 0, long end = -1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => StringBitPositionAsync(key, bit, start, end, (CommandFlags)flags).Result);
        }

        private Task<long> StringDecrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringDecrementAsync(key, value, flags);
        }

        private Task<double> StringDecrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringDecrementAsync(key, value, flags);
        }

        public Task<long> StringDecrementAsync(string key, long value = 1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => StringDecrementAsync(key, value, (CommandFlags)flags).Result);
        }
        public Task<double> StringDecrementAsync(string key, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<double>(() => StringDecrementAsync(key, value, (CommandFlags)flags).Result);
        }

        private Task<RedisValue> StringGetAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringGetAsync(key, flags);
        }

        public Task<RedisValue> StringGetAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<RedisValue>(() => StringGetAsync(key, (CommandFlags)flags).Result);
        }

        private Task<RedisValue[]> StringGetAsync(RedisKey[] keys, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringGetAsync(keys, flags);
        }

        public Task<string[]> StringGetAsync(string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string[]>(() => StringGetAsync(keys.ToRedisKeyArray(), (CommandFlags)flags).Result.ToStringArray());
        }

        public Task<byte[][]> StringGetBytesAsync(string[] keys, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[][]>(() => StringGetAsync(keys.ToRedisKeyArray(), (CommandFlags)flags).Result.ToBytesArray());
        }

        private Task<bool> StringGetBitAsync(RedisKey key, long offset, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringGetBitAsync(key, offset, flags);
        }

        public Task<bool> StringGetBitAsync(string key, long offset, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => StringGetBitAsync(key, offset, (CommandFlags)flags).Result);
        }

        private Task<RedisValue> StringGetRangeAsync(RedisKey key, long start, long end, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringGetRangeAsync(key, start, end, flags);
        }

        public Task<string> StringGetRangeAsync(string key, long start, long end, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => StringGetRangeAsync(key, start, end, (CommandFlags)flags).Result);
        }

        private Task<RedisValue> StringGetSetAsync(RedisKey key, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringGetSetAsync(key, value, flags);
        }

        public Task<string> StringGetSetAsync(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => StringGetSetAsync(key, value, (CommandFlags)flags).Result);
        }

        public Task<byte[]> StringGetSetBytesAsync(string key, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[]>(() => StringGetSetAsync(key, value, (CommandFlags)flags).Result);
        }

        private Task<RedisValueWithExpiry> StringGetWithExpiryAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringGetWithExpiryAsync(key, flags);
        }

        private Task<long> StringIncrementAsync(RedisKey key, long value = 1, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringIncrementAsync(key, value, flags);
        }

        private Task<double> StringIncrementAsync(RedisKey key, double value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringIncrementAsync(key, value, flags);
        }

        public Task<long> StringIncrementAsync(string key, long value = 1, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => StringIncrementAsync(key, value, (CommandFlags)flags).Result);
        }

        public Task<double> StringIncrementAsync(string key, double value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<double>(() => StringIncrementAsync(key, value, (CommandFlags)flags).Result);
        }

        private Task<long> StringLengthAsync(RedisKey key, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringLengthAsync(key, flags);
        }

        public Task<long> StringLengthAsync(string key, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<long>(() => StringLengthAsync(key, (CommandFlags)flags).Result);
        }

        private Task<bool> StringSetAsync(RedisKey key, RedisValue value, TimeSpan? expiry = null, When when = When.Always,
            CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringSetAsync(key, value, expiry, when, flags);
        }

        public Task<bool> StringSetAsync(string key, string value, TimeSpan? expiry = null, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => StringSetAsync(key, value, expiry, (When)when, (CommandFlags)flags).Result);
        }

        public Task<bool> StringSetAsync(string key, byte[] value, TimeSpan? expiry = null, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => StringSetAsync(key, value, expiry, (When)when, (CommandFlags)flags).Result);
        }

        private Task<bool> StringSetAsync(KeyValuePair<RedisKey, RedisValue>[] values, When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringSetAsync(values, when, flags);
        }

        public Task<bool> StringSetAsync(KeyValuePair<string, string>[] values, RedisWhen when = RedisWhen.Always, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => StringSetAsync(values.Select(s => new KeyValuePair<RedisKey, RedisValue>(s.Key, s.Value)).ToArray(), (When)when, (CommandFlags)flags).Result);
        }

        private Task<bool> StringSetBitAsync(RedisKey key, long offset, bool bit, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringSetBitAsync(key, offset, bit, flags);
        }

        public Task<bool> StringSetBitAsync(string key, long offset, bool bit, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<bool>(() => StringSetBitAsync(key, offset, bit, (CommandFlags)flags).Result);
        }

        private Task<RedisValue> StringSetRangeAsync(RedisKey key, long offset, RedisValue value, CommandFlags flags = CommandFlags.None)
        {
            var db = GetDatabase(Database);
            return db.StringSetRangeAsync(key, offset, value, flags);
        }

        public Task<string> StringSetRangeAsync(string key, long offset, string value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<string>(() => StringSetRangeAsync(key, offset, value, (CommandFlags)flags).Result);
        }

        public Task<byte[]> StringSetRangeBytesAsync(string key, long offset, byte[] value, RedisCommandFlags flags = RedisCommandFlags.None)
        {
            return new Task<byte[]>(() => StringSetRangeAsync(key, offset, value, (CommandFlags)flags).Result);
        }
    }
}