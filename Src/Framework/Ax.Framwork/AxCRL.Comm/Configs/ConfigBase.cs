/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：配置类文件的基类
 * 创建标识：Zhangkj 2017/06/05
 * 
 *
************************************************************************/
using AxCRL.Comm.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Configs
{
    /// <summary>
    /// 配置类文件的基类
    /// </summary>
    public abstract class ConfigBase
    {       
        /// <summary>
        /// 获取类型对应的配置文件名称。默认以"类名.json"为配置文件名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetConfigFileName(Type type)
        {
            string fullName = type.FullName;
            string[] segs = fullName.Split(new char[] { '.' });
            string _ConfigFileName = string.Format("{0}.json", segs[segs.Length - 1]);//类名作为配置文件名
            return _ConfigFileName;
        }       
        /// <summary>
        /// 默认存储在AxPath下的Config目录下
        /// </summary>
        public static string SaveDir
        {
            get
            {
                string _SaveDir = Path.Combine(EnvProvider.Default.MainPath, "Config");
                return _SaveDir;
            }
        }
        /// <summary>
        /// 读取配置
        /// </summary>
        /// <returns></returns>
        public static T ReadConfig<T>() where T:ConfigBase
        {
            try
            {
                string path = Path.Combine(SaveDir, GetConfigFileName(typeof(T)));
                string jsonString = string.Empty;
                if (File.Exists(path))
                {
                    using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                    {
                        jsonString = sr.ReadToEnd();
                    }
                    return JsonConvert.DeserializeObject<T>(jsonString);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exp)
            {
                string info = exp.Message;
                // to do log
                return null;
            }
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        public void SaveConfig()
        {
            try
            {
                if (Directory.Exists(SaveDir) == false)
                    Directory.CreateDirectory(SaveDir);
                string path = Path.Combine(SaveDir, GetConfigFileName(this.GetType()));
                if (File.Exists(path))
                {
                    //删除之前的
                    File.Delete(path);
                }
                string jsonString = JsonConvert.SerializeObject(this);
                using (FileStream fs = new FileStream(path, FileMode.CreateNew))
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(jsonString);
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Flush();
                }
            }
            catch (Exception exp)
            {
                string info = exp.Message;
                // to do log                
            }
        }
    }
}
