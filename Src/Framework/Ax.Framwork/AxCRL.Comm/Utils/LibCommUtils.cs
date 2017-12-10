using AxCRL.Comm.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AxCRL.Comm.Utils
{
    public static class LibCommUtils
    {
        public static string GetStoredProcedureName(string progId)
        {
            return progId.Replace('.', '_');
        }

        public static ulong GetInternalId()
        {
            int refCounter = Environment.TickCount;
            Random random = new Random(refCounter);
            ulong result;
            Monitor.Enter(random);
            try
            {
                uint time = ((uint)DateTime.UtcNow.GetHashCode()) << 2;
                do
                {
                    result = ((((ulong)random.Next()) << 32) + (time & 0xff000000 + ((time & 0x003fc000) << 2) + ((time & 0xff0) << 4)) + (uint)(refCounter++ & 0xff));
                } while (result < 100);
            }
            finally
            {
                Monitor.Exit(random);
            }
            return result;
        }
        /// <summary>
        /// 输出信息到Output目录
        /// </summary>
        /// <param name="classTypePath">输出信息类型对应的文件目录,形式类似Error\Excel</param>
        /// <param name="msg">要输出的消息</param>
        public static void AddOutput(string classTypePath,string msg)
        {
            try
            {
                if (string.IsNullOrEmpty(classTypePath) || string.IsNullOrEmpty(msg))
                    return;
                string path= Path.Combine(EnvProvider.Default.MainPath, "Output", classTypePath);
                if (Directory.Exists(path) == false)
                    Directory.CreateDirectory(path);
                path = Path.Combine(path, string.Format("{0}.txt", DateTime.Now.Ticks));
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.Write(msg);
                    }
                }
            }
            catch { }            
        }
        
    }
}
