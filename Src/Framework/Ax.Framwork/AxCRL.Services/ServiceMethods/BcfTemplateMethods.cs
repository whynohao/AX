/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：Bcf的模板信息访问的方法类
 * 创建标识：Zhangkj 2017/02/07
 * 
************************************************************************/
using AxCRL.Bcf;
using AxCRL.Comm.Runtime;
using AxCRL.Core;
using AxCRL.Template.DataSource;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Services.ServiceMethods
{
    /// <summary>
    /// Bcf模板信息访问提供类，包含一些访问Bcf模板信息需要的公共静态方法。可供WebAPI、WebService、Wcf服务等使用
    /// </summary>
    public class BcfTemplateMethods
    {
        /// <summary>
        /// 根据功能标识返回该功能标识下的所有DataTable的DefineField信息
        /// 第一层List的顺序是功能下的DataTable顺序，第二层是某个DataTable下的各个DefineField信息
        /// </summary>
        /// <param name="progId">功能标识</param>
        /// <param name="errorMsg">错误信息。如果成功则为string.Empty</param>
        /// <returns></returns>
        public static List<List<DefineField>> GetBcfDefineFields(string progId,out string errorMsg)
        {
            errorMsg = string.Empty;
            try
            {
                List<List<DefineField>> listResult = null;
                if (ProgIdHost.Instance.ProgIdRef.ContainsKey(progId))
                {
                    BcfServerInfo info = ProgIdHost.Instance.ProgIdRef[progId];
                    string path = Path.Combine(EnvProvider.Default.MainPath, "Bcf", info.DllName);
                    Assembly assembly = Assembly.LoadFrom(path);
                    Type t = assembly.GetType(info.ClassName);
                    LibBcfBase destObj = (LibBcfBase)t.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
                    if (destObj == null)
                    {
                        errorMsg = string.Format("异常信息:未能构造Bcf实例对象。");
                        return null;
                    }
                    if (destObj.DataSet == null || destObj.DataSet.Tables == null || destObj.DataSet.Tables.Count == 0
                        || destObj.DataSet.Tables[0].Columns == null || destObj.DataSet.Tables[0].Columns.Count == 0)
                    {
                        errorMsg = string.Format("异常信息:Bcf实例对象DataSet的Column列为空。");
                        return null;
                    }
                    listResult = new List<List<DefineField>>();
                    foreach (DataTable table in destObj.DataSet.Tables)
                    {
                        listResult.Add(new List<DefineField>());
                        foreach (DataColumn column in table.Columns)
                        {
                            listResult.Last().Add(DataSourceHelper.ConvertToDefineField(column));
                        }
                    }
                }
                else
                {
                    errorMsg = string.Format("异常信息:不可识别的功能标识号:{0}", progId);
                    return null;
                }
                return listResult;
            }
            catch (Exception exp)
            {
                string message = exp.InnerException == null ? exp.Message : exp.InnerException.Message;
                errorMsg = string.Format("异常信息:{0}{1}异常堆栈:{2}", message, Environment.NewLine, exp.StackTrace);               
                return null;
            }
        }
        /// <summary>
        /// 移动端的ShowScheme文件夹名称
        /// </summary>
        public static readonly string MobileSchemePath = "MobileSchemePath";
        /// <summary>
        /// 获取移动端需要的功能模块ShowScheme对应的Json文件
        /// Json文件以UTF8格式保存
        /// </summary>
        /// <param name="progId">功能标识</param>
        /// <param name="errorMsg">错误信息。如果成功则为string.Empty</param>
        /// <returns></returns>
        public static string GetBcfMobileShowScheme(string progId,out string errorMsg)
        {
            errorMsg = string.Empty;
            string jsonString = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(progId))
                {
                    errorMsg = "功能标识号为Null或空。";
                    return string.Empty;
                }
                string schemeName = string.Format("{0}.json", progId);
                string directoryPath = Path.Combine(EnvProvider.Default.MainPath, "Scheme", "ShowScheme", MobileSchemePath);
                if (Directory.Exists(directoryPath)==false)
                {
                    Directory.CreateDirectory(directoryPath);
                }
                string path = Path.Combine(directoryPath, schemeName);
                if (File.Exists(path))
                {
                    using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                    {
                        jsonString = sr.ReadToEnd();
                    }                  
                }
                else
                {
                    errorMsg = string.Format("功能标识号对应的ShowScheme Json文件不存在:{0}", path);
                    return string.Empty;
                }
                return jsonString;
            }
            catch (Exception exp)
            {
                string message = exp.InnerException == null ? exp.Message : exp.InnerException.Message;
                errorMsg = string.Format("异常信息:{0}{1}异常堆栈:{2}", message, Environment.NewLine, exp.StackTrace);
                return string.Empty;
            }            
        }
    }
}
