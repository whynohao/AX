/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：将Excel转成txt
 * 创建标识：CHENQI 2016/12/06
 * 修改标识：
 * 修改描述：
************************************************************************/
using Jikon.MES_Dm.DMCommon;
using System;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace MES_Dm.FullTextRetrieval.Core.AnyToTxt
{
    /// <summary>
    /// 将Excel文件转成txt的转换类
    /// </summary>
    public class Excel2Txt : IFile2TxtBase
    {
        /// <summary>
        /// 转换后的输入路径
        /// </summary>
        private string destFileName = string.Empty;
        /// <summary>
        /// 转换后的文件名
        /// </summary>
        public string NewFileName
        {
            get
            {
                return destFileName;
            }
        }

        /// <summary>
        /// 将Excel转成txt
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
            if (info.Extension != ".xls" && info.Extension != ".xlsx")
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

                var conn = new OleDbConnection();
                conn.ConnectionString = String.Format(@"provider=Microsoft.ACE.OLEDB.12.0;extended properties='excel 12.0 Macro;hdr=yes';data source={0}", fileName);
                conn.Open();
                DataTable sheetTb = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                foreach (DataRow sheet in sheetTb.Rows)
                {
                    string tableName = sheet["TABLE_NAME"].ToString();

                    string sql = String.Format("select * from [{0}]", tableName);
                    OleDbDataAdapter da = new OleDbDataAdapter(sql, conn);

                    var ds = new DataSet();
                    da.Fill(ds);

                    var tb1 = ds.Tables[0];

                    if (tb1.Rows.Count == 0)
                    {
                        continue; // 空表
                    }
                    if (tb1.Rows.Count == 1 && tb1.Columns.Count == 1)
                    {
                        if (tb1.Rows[0][0] == DBNull.Value)
                        {
                            continue; // 空表
                        }
                    }

                    int[] colMaxLen = new int[tb1.Columns.Count];

                    foreach (DataRow row in tb1.Rows)
                    {
                        for (int j = 0; j < tb1.Columns.Count; ++j)
                        {
                            DataColumn col = tb1.Columns[j];
                            string content = row[j].ToString();

                            bool hasYinhao = false;
                            if (-1 != content.IndexOf("\r") || -1 != content.IndexOf("\n"))
                            {
                                hasYinhao = true;
                            }
                            string fmt;
                            fmt = String.Format("{0}{1}0{2}{3}{4}", hasYinhao ? "\"" : "",
                            "{", "}", hasYinhao ? "\"" : "", j + 1 == tb1.Columns.Count ? "" : "\t");
                            sw.Write(fmt, row[j]);
                        }
                        sw.WriteLine();
                    }
                }
                sw.Close();
                conn.Close();
            }
            catch (Exception ex)
            {

                DMCommonMethod.WriteLog("文档管理", fileName + "文件转换失败\n,"+ex.Message);
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
