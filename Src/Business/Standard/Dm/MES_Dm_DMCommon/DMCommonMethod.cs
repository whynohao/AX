/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的通用处理类，完成一些通用操作
 * 创建标识：Zhangkj 2016/12/13
 * 
************************************************************************/
using AxCRL.Comm.Runtime;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jikon.MES_Dm.DMCommon
{
    /// <summary>
    /// 文档模块通用处理类
    /// </summary>
    public class DMCommonMethod
    {
        /// <summary>
        /// 获得文档库的根目录
        /// </summary>
        /// <param name="dirType">目录类型</param>
        /// <returns></returns>
        public static string GetDMRootPath(DirTypeEnum dirType)
        {
            if (Directory.Exists(EnvProvider.Default.DocumentsPath) == false)
                Directory.CreateDirectory(EnvProvider.Default.DocumentsPath);
            string path = Path.Combine(EnvProvider.Default.DocumentsPath, (dirType == DirTypeEnum.Public ? "" : "my"));//根据是否为私有类型在路径下增加my
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
            return path;
        }
        /// <summary>
        /// 获得文档库的临时文件夹目录
        /// </summary>
        /// <returns></returns>
        public static string GetDMRootTempPath()
        {
            if (Directory.Exists(EnvProvider.Default.DocumentsPath) == false)
                Directory.CreateDirectory(EnvProvider.Default.DocumentsPath);
            string path = Path.Combine(EnvProvider.Default.DocumentsPath,"Temp");
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
            return path;
        }
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="kind">信息种类</param>
        /// <param name="msg"></param>
        public static void WriteLog(string kind,string msg)
        {
            try
            {
                if (kind == null)
                    kind = string.Empty;
                string logPath = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output", "Log", "DM");
                if (Directory.Exists(logPath) == false)
                {
                    Directory.CreateDirectory(logPath);
                }
                logPath = Path.Combine(logPath, string.Format("{0}_{1}.txt", kind, DateTime.Now.Ticks));
                using (System.IO.FileStream fs = new System.IO.FileStream(logPath, System.IO.FileMode.Create))
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                    {
                        sw.Write(msg);
                        sw.Flush();
                    }
                }
            }
            catch
            {
                //发生异常不再处理
            }
            
        }
        /// <summary>
        /// 检查用户是否具有指定文件的下周权限，如有则返回待下载的临时文件地址
        /// </summary>
        /// <param name="userHandle"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        public static string CheckAndGetDownloadTempFile(string userHandle, string docId, int modifyId, out string downloadName)
        {
            downloadName = string.Empty;
            try
            {
                LibHandle libHandle = LibHandleCache.Default.GetCurrentHandle(userHandle) as LibHandle;
                if (libHandle == null)
                {
                    return string.Empty;//用户句柄无效
                }
                if (DMPermissionControl.Default.HasPermission(libHandle, string.Empty, docId, DMFuncPermissionEnum.Download) == false)
                    return string.Empty;
                DirLinkAddress dirlink = new DirLinkAddress(docId);
                downloadName = dirlink.DocName;
                //复制一份到临时目录
                string tempPath = Path.Combine(GetDMRootTempPath(), string.Format("{0}_{1}", DateTime.Now.Ticks.ToString(), downloadName));
                string docFullPath = dirlink.GetDocFullPath(modifyId);
                if (File.Exists(docFullPath))
                {
                    File.Copy(docFullPath, tempPath);
                    return tempPath;
                }
                return string.Empty;
            }
            catch (Exception exp)
            {
                WriteLog("DMCommonMethod.CheckAndGetDownloadTempFile", string.Format("DocId:{0},Error:{1}", docId, exp.ToString()));
                return string.Empty;
            }            
        }
    }
}
