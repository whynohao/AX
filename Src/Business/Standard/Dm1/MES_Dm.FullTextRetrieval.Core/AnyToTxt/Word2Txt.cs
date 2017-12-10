/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：将Word文档转成txt
 * 创建标识：CHENQI 2016/12/06
 * 修改标识：
 * 修改描述：
************************************************************************/
using Jikon.MES_Dm.DMCommon;
using Microsoft.Office.Interop.Word;
using System;
using System.IO;

namespace MES_Dm.FullTextRetrieval.Core.AnyToTxt
{
    /// <summary>
    /// 将word转化成txt文件的类
    /// </summary>
    public class Word2Txt : IFile2TxtBase
    {
        /// <summary>
        /// 转化后文件的输出路径
        /// </summary>
        private string destFileName = string.Empty;

        /// <summary>
        /// 转化后的文件名
        /// </summary>
        public string NewFileName
        {
            get
            {
                return destFileName;
            }
        }
        /// <summary>
        /// 将word文档转成txt
        /// </summary>
        /// <param name="fileName">文档全路径</param>
        public void Convert(string fileName)
        {
            if (!File.Exists(fileName))
            {
                //打印日志
                Console.WriteLine("文件不存在");
                return;
            }
            FileInfo info = new FileInfo(fileName);
            if(info.Extension != ".doc" && info.Extension != ".docx")
            {
                //打印日志
                Console.WriteLine("文件格式错误");
                return;
            }
            destFileName = Path.Combine(DMCommonMethod.GetDMRootTempPath(), string.Format("_{0}.txt", CreateOnlyFileNameUtil.CreateOnlyFileName()));

            Application app = new Application();
            app.Visible = false;
            object obj = System.Reflection.Missing.Value;
            object inputFile = fileName;

            Document doc = app.Documents.Open(ref inputFile, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj);
            
            object outfile = destFileName;
            object fmt = WdSaveFormat.wdFormatEncodedText;
            doc.SaveAs(ref outfile, ref fmt, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj, ref obj);
            doc.Close(ref obj, ref obj, ref obj);
            app.Quit();
            app = null;
        }
        /// <summary>
        /// 删除转换的txt文本文件
        /// </summary>
        public void DeleteTxt()
        {
            if (File.Exists(destFileName))
            {
                File.Delete(destFileName);
            }
        }
    }
}
