/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：全文检索服务的实现类
 * 创建标识：CHENQI 2016/11/10
 * 修改标识：
 * 修改描述：
************************************************************************/
using MES_Dm.FullTextRetrieval.Core.HightLight;
using MES_Dm.FullTextRetrieval.Core.Index;
using MES_Dm.FullTextRetrieval.Core.Model;
using System;
using System.IO;
using System.Collections.Generic;
using MES_Dm.FullTextRetrieval.Core.Util;
using AxCRL.Data;
using AxCRL.Comm.Utils;
using MES_Dm.FullTextRetrieval.Core.AnyToTxt;
using System.Data;
using Jikon.MES_Dm.DMCommon;
using AxCRL.Core.Comm;
using AxCRL.Core.Cache;

namespace MES_Dm.FullTextRetrieval.Core
{
    /// <summary>
    /// 全文检索服务接口的实现类
    /// </summary>
    public class AxIndexer : IIndexer
    {
        public static void Init() {
            FullIndexHelper.NewDocArrivedToFullIndex += FullIndexHelper_NewDocArrivedToFullIndex;
            FullIndexHelper.NeedDeleteDocFullIndex += FullIndexHelper_NeedDeleteDocFullIndex;
        }

        private static AxIndexer _Default;
        private static object lockObj = new object();

        private static void FullIndexHelper_NeedDeleteDocFullIndex(string docId, int modifyVerId)
        {
            Default.DeleteIndex(docId);
        }

        private static void FullIndexHelper_NewDocArrivedToFullIndex(string docId, string docType, int modifyVerId, bool isFullNew, string fullPath)
        {
            Default.DeleteIndex(docId);
            Default.AddIndex(docId);
        }

        private AxIndexer()
        {

        }

        private static AxIndexer Default{
            get
            {
                if(_Default == null)
                {
                    lock(lockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new AxIndexer();
                        }
                    }
                }
                return _Default;
            }
        }

        /// <summary>
        /// 初始化所有索引
        /// </summary>
        /// <param>人员权限</param>
        /// <returns></returns>
        public bool InitIndexs()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 读取文件文本内容
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="extName"></param>
        /// <returns></returns>
        private string ReadContent(string fileName, string extName)
        {
            IFile2TxtBase file2Txt = null;
            string content = string.Empty;
            if (extName.ToLower() == ".doc" || extName.ToLower() == ".docx")
            {
                file2Txt = new Word2Txt();
            }
            else if(extName.ToLower() == ".ppt" || extName.ToLower() == ".pptx")
            {
                file2Txt = new PPT2Txt();
            }
            else if(extName.ToLower() == ".xls"|| extName.ToLower() == ".xlsx")
            {
                file2Txt = new Excel2Txt();
            }
            else if(extName.ToLower() == ".txt")
            {
                string str = File.ReadAllText(fileName, FileEncoding.GetType(fileName));
                return str;
            }
            
            if(file2Txt != null)
            {
                file2Txt.Convert(fileName);
                content = File.ReadAllText(file2Txt.NewFileName, FileEncoding.GetType(file2Txt.NewFileName));
                file2Txt.DeleteTxt();
                return content;
            }

            throw new Exception("无法识别的文件格式");
        }

        /// <summary>
        /// 根据文件创建索引
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //private bool CreateIndex(string fileName)
        //{
        //    if (File.Exists(fileName))
        //    {
        //        LibDataAccess dataAccess = new LibDataAccess();
                
        //        FileInfo info = new FileInfo(fileName);
        //        AbstractFileBase fileBase = new TextFileInfo();
        //        #region 根据文件路径查找
        //        string sql = string.Format("select DOCID from DMDOCUMENT where SAVEPATH = {0}", LibStringBuilder.GetQuotString(fileName));
        //        fileBase.FileId = dataAccess.ExecuteScalar(sql).ToString();
        //        object ret = dataAccess.ExecuteScalar(string.Format("select ISFULLINDEX from DMDOCUMENT where DOCID = {0}", LibStringBuilder.GetQuotString(fileBase.FileId)));
        //        if (LibSysUtils.ToInt32(ret) == 1)
        //        {
        //            return true;
        //        }
        //        #endregion
        //        //fileBase.FileName = info.Name;
        //        //fileBase.FilePath = info.DirectoryName;
        //        //fileBase.UpLoadPersonId = "zhangsan";
        //        //fileBase.CreateTime = DateTime.Now.ToString();
        //        fileBase.Content = ReadContent(fileName, info.Extension.Trim());
                
        //        IIndexManager indexManager = new IndexManagerFactory().Create();
        //        return indexManager.CreateIndex(fileBase);
        //    }
        //    if (Directory.Exists(fileName))
        //    {
        //        foreach (var item in new DirectoryInfo(fileName).GetFiles())
        //        {
        //            CreateIndex(item.FullName);
        //        }
        //        foreach (var item in new DirectoryInfo(fileName).GetDirectories())
        //        {
        //            CreateIndex(item.FullName);
        //        }
        //    }
        //    return true;
        //}

        /// <summary>
        /// 为文件添加索引
        /// </summary>
        /// <param>文件标识符</param>
        /// <param>人员权限</param>
        /// <returns></returns>
        public bool AddIndex(string fileId)
        {
            AbstractFileBase fileBase = new TextFileInfo();
            fileBase.FileId = fileId;
            #region 根据文件ID查找文件路径
            DirLinkAddress dirlink = new DirLinkAddress(fileId);
            string fileName = dirlink.GetDocFullPath(-1);
            #endregion
            //fileBase.FileName = info.Name;
            //fileBase.FilePath = info.DirectoryName;
            //fileBase.UpLoadPersonId = "zhangsan";
            //fileBase.CreateTime = DateTime.Now.ToString();
            fileBase.Content = ReadContent(fileName, dirlink.DocType);
            
            IIndexManager indexManager = new IndexManagerFactory().Create();
            return indexManager.CreateIndex(fileBase);
        }

        /// <summary>
        /// 删除文件的索引
        /// </summary>
        /// <param>文件标识符</param>
        /// <param>人员权限</param>
        /// <returns></returns>
        public bool DeleteIndex(string fileId)
        {
            AbstractFileBase fileBase = new TextFileInfo();
            fileBase.FileId = fileId;

            IIndexManager indexManager = new IndexManagerFactory().Create();
            return indexManager.DeleteIndex(fileBase);
        }

        /// <summary>
        /// 删除所有索引
        /// </summary>
        /// <param>人员权限</param>
        /// <returns></returns>
        public bool DestoryIndexs()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 查询索引
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param>人员权限</param>
        /// <returns></returns
        public ResultSet SearchIndex(string key, int pageNum, string userHandle, string lastFileId, string nextFileId)
        {
            LibHandle handle = LibHandleCache.Default.GetCurrentHandle(userHandle) as LibHandle;
            //页大小
            int pageSize = 8;
            IHightLighter hightLighter = new HightLighterFactory().Create();
            IIndexManager indexManager = new IndexManagerFactory().Create();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("content", key);
            SearchResult result = null;
            if (string.IsNullOrEmpty(lastFileId) && string.IsNullOrEmpty(nextFileId))//第一次查询
            {
                result = (SearchResult)indexManager.SearchIndex(dic, pageNum, pageSize, handle);
            }
            else if(string.IsNullOrEmpty(lastFileId))//上一页
            {
                result = (SearchResult)indexManager.SearchPrevIndex(dic, pageNum, pageSize, handle, nextFileId);
            }
            else if (string.IsNullOrEmpty(nextFileId))//下一页
            {
                result = (SearchResult)indexManager.SearchNextIndex(dic, pageNum, pageSize, handle, lastFileId);
            }
            ResultSet resultSet = new ResultSet();
            resultSet.SearchTime = result.SearchTime;
            resultSet.PageNum = pageNum;
            resultSet.ResultsCount = result.TotalHits;
            resultSet.PageCount = (resultSet.ResultsCount - 1) / pageSize + 1;
            DirLinkAddress dirlink;
            foreach (var item in result.Docs)
            {
                AbstractFileBase fileBase = hightLighter.InitHightLight(dic, item);
                FileInfoItem info = new FileInfoItem()
                {
                    FileId = fileBase.FileId,
                    Contents = fileBase.Content
                };
                dirlink = new DirLinkAddress(fileBase.FileId);
                info.FileName = dirlink.DocName;
                info.DirId = dirlink.DirID;
                info.DirType = dirlink.DirType;
                info.Path = dirlink.DirNameLink;
                resultSet.FileInfoItems.Add(info);
            }
            if (resultSet.FileInfoItems.Count == 0)
            {
                return null;
            }
            return resultSet;
        }
    }
}
