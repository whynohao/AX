// ******************************************************************************************
// * 文件名：JsonUtil.cs
// * 功能描述：
// * 创建人：wangjun
// * 创建日期：2016年09月07日 下午 2:06
// *******************************************************************************************

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;

namespace AxCRL.Comm.Redis
{
    /// <summary>
    /// 序列化、反序列化操作类
    /// </summary>
    public class JsonUtiler
    {
        /// <summary>
        /// 序列化有DataContract标记的类
        /// </summary>
        /// <typeparam name="T">序列化类型</typeparam>
        /// <param name="t">类型值</param>
        /// <returns></returns>
        public static string Serialize<T>(T t)
        {
            try
            {
                return t == null ? null : JsonConvert.SerializeObject(t);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 反序列化有DataContract标记的类
        /// </summary>
        /// <typeparam name="T">序列化类型</typeparam>
        /// <param name="value">序列化内容</param>
        /// <returns></returns>
        public static T Deserialize<T>(string value)
        {
            try
            {
                return string.IsNullOrEmpty(value) ? default(T) : JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// 序列化有DataContract标记的类
        /// </summary>
        /// <typeparam name="T">序列化类型</typeparam>
        /// <param name="t">类型值</param>
        /// <returns></returns>
        public static string SerializeDataContract<T>(T t) where T : class
        {
            try
            {
                if (t == null) return null;
                using (var ms = new MemoryStream())
                {
                    var s = new DataContractSerializer(typeof(T));
                    s.WriteObject(ms, t);
                    byte[] array = ms.ToArray();
                    return Encoding.UTF8.GetString(array, 0, array.Length);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 反序列化有DataContract标记的类
        /// </summary>
        /// <typeparam name="T">序列化类型</typeparam>
        /// <param name="value">序列化内容</param>
        /// <returns></returns>
        public static T DeserializeDataContract<T>(string value) where T : class
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return default(T);
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(value)))
                {
                    var s = new DataContractSerializer(typeof(T));
                    return (T)s.ReadObject(ms);
                }
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// 序列化成字节流
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <returns></returns>
        public static byte[] SerializeBinary(object obj)
        {
            try
            {
                if (obj == null) return null;
                var formatter = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, obj);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms.GetBuffer();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 字节流反序列化成对象
        /// </summary>
        /// <typeparam name="T">反序列化后的对象</typeparam>
        /// <param name="bytes">要反序列化的字节流</param>
        /// <returns></returns>
        public static T DeserializeBinary<T>(byte[] bytes)
        {
            try
            {
                if (bytes == null || bytes.Length == 0) return default(T);
                var formatter = new BinaryFormatter() { Binder = new ExBinder() };
                using (var ms = new MemoryStream(bytes))
                {
                    return (T)formatter.Deserialize(ms);
                }
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }

    public class ExBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            var ass = Assembly.GetExecutingAssembly();
            return ass.FullName.Equals(assemblyName) ? ass.GetType() : Assembly.Load(assemblyName).GetType(typeName);
        }
    }
}