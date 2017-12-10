/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：将PPT转成txt
 * 创建标识：CHENQI 2016/12/06
 * 修改标识：
 * 修改描述：
************************************************************************/
using Jikon.MES_Dm.DMCommon;
using Microsoft.Office.Interop.PowerPoint;
using System;
using System.IO;

namespace MES_Dm.FullTextRetrieval.Core.AnyToTxt
{
    /// <summary>
    /// 将PPT转化成txt文件的类
    /// </summary>
    public class PPT2Txt : IFile2TxtBase
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
        /// 将PPT转成txt
        /// </summary>
        /// <param name="fileName"></param>
        public void Convert(string fileName)
        {
            if (!File.Exists(fileName))
            {
                //打印日志
                //Console.WriteLine("文件不存在");
                DMCommonMethod.WriteLog("文档管理", fileName + "文件不存在");
                return;
            }
            FileInfo info = new FileInfo(fileName);
            if (info.Extension != ".ppt" && info.Extension != ".pptx")
            {
                //打印日志
                //Console.WriteLine("文件格式错误");
                DMCommonMethod.WriteLog("文档管理", fileName + "文件格式错误");
                return;
            }
            destFileName = Path.Combine(DMCommonMethod.GetDMRootTempPath(), string.Format("_{0}.txt", CreateOnlyFileNameUtil.CreateOnlyFileName()));

            try
            {
                FileStream fs = new FileStream(destFileName, FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(fs);

                Application pa = new Application();
                Presentation pp = pa.Presentations.Open(fileName, Microsoft.Office.Core.MsoTriState.msoTrue, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse);
                //开始读取每一页
                foreach (Slide slide in pp.Slides)
                {
                    //开始读取每一个数据块
                    foreach (Shape shape in slide.Shapes)
                    {
                        try
                        {
                            if (shape.TextFrame.TextRange != null)//如果是文字,文字处理
                            {
                                string text = shape.TextFrame.TextRange.Text.Trim();
                                sw.Write(text);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "指定的值超出了范围")
                            {
                                continue;
                            }
                        }
                    }
                    sw.WriteLine();
                }
                sw.Close();
            }
            catch (Exception ex)
            {

                DMCommonMethod.WriteLog("文档管理", fileName + "文件转换错误\n"+ex.Message);
            }
        }
        /// <summary>
        /// 删除转换后的txt
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
