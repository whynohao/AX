// ******************************************************************************************
// * 文件名：RedisUtilExtension.cs
// * 功能描述：
// * 创建人：wangjun
// * 创建日期：2016年09月18日 16:00
// *******************************************************************************************

using System;
using StackExchange.Redis;

namespace AxCRL.Comm.Redis
{
    /// <summary>
    /// 扩展类
    /// </summary>
    internal static class RedisUtilExtension
    {
        static readonly string[] nix = new string[0];
        static readonly byte[][] nixBytes = new byte[0][];
        static readonly RedisValue[] nixRedisValues = new RedisValue[0];
        static readonly RedisKey[] nixRedisKeys = new RedisKey[0];


        /// <summary>
        /// 转换为字符串数组
        /// </summary>
        public static string[] ToStringArray(this RedisValue[] values)
        {
            if (values == null) return null;
            if (values.Length == 0) return nix;
            return RedisUtil.ConvertAll(values, x => (string)x);
        }

        /// <summary>
        /// 转换为字符串数组
        /// </summary>
        public static byte[][] ToBytesArray(this RedisValue[] values)
        {
            if (values == null) return null;
            if (values.Length == 0) return nixBytes;
            return RedisUtil.ConvertAll(values, x => (byte[])x);
        }

        /// <summary>
        /// 转换为Redis值域
        /// </summary> 
        public static RedisValue[] ToRedisValueArray(this string[] values)
        {
            if (values == null) return null;
            if (values.Length == 0) return nixRedisValues;
            return RedisUtil.ConvertAll(values, x => (RedisValue)x);
        }

        /// <summary>
        /// 转换为Redis值域
        /// </summary> 
        public static RedisValue[] ToRedisValueArray(this byte[][] values)
        {
            if (values == null) return null;
            if (values.Length == 0) return nixRedisValues;
            return RedisUtil.ConvertAll(values, x => (RedisValue)x);
        }

        /// <summary>
        /// 转换为Redis键
        /// </summary> 
        public static RedisKey[] ToRedisKeyArray(this string[] keys)
        {
            if (keys == null) return null;
            if (keys.Length == 0) return nixRedisKeys;
            return RedisUtil.ConvertAll(keys, x => (RedisKey)x);
        }

        /// <summary>
        /// 转换为键值
        /// </summary> 
        public static string[] ToStringKeyArray(this RedisKey[] keys)
        {
            if (keys == null) return null;
            if (keys.Length == 0) return nix;
            return RedisUtil.ConvertAll(keys, x => (string)x);
        }
    }
     
    internal class RedisUtil
    {
        /// <summary>
        /// 将输入类型数组转换为输出类型数组
        /// </summary>
        /// <typeparam name="TInput">输入类型</typeparam>
        /// <typeparam name="TOutput">输出类型</typeparam>
        /// <param name="source">源</param>
        /// <param name="selector">选择器</param>
        /// <returns></returns>
        public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] source, Func<TInput, TOutput> selector)
        {
            return Array.ConvertAll(source, item => selector(item));
        }
    }
}