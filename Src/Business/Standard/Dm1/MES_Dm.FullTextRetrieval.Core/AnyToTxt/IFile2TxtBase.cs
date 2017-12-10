/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文件转换成txt文本文件的接口
 * 创建标识：CHENQI 2016/12/06
 * 修改标识：
 * 修改描述：
************************************************************************/
using System;

namespace MES_Dm.FullTextRetrieval.Core.AnyToTxt
{
    /// <summary>
    /// 文件转换的接口
    /// </summary>
    public interface IFile2TxtBase
    {
        string NewFileName
        {
            get;
        }

        void Convert(string fileName);

        void DeleteTxt();
    }
    /// <summary>
    /// 可以获得唯一文件名的类
    /// </summary>
    public sealed class CreateOnlyFileNameUtil
    {
        /// <summary>
        /// 获得唯一文件名
        /// </summary>
        /// <returns></returns>
        public static string CreateOnlyFileName()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
