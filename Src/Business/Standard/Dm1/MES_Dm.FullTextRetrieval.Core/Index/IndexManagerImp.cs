/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：管理索引
 * 创建标识：CHENQI 2016/12/01
 * 修改标识：
 * 修改描述：
************************************************************************/
using AxCRL.Data;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using MES_Dm.FullTextRetrieval.Core.Model;
using MES_Dm.FullTextRetrieval.Core.Util;
using PanGu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using AxCRL.Comm.Utils;
using Jikon.MES_Dm.DMCommon;
using AxCRL.Core.Comm;
using System.Collections;

namespace MES_Dm.FullTextRetrieval.Core.Index
{
    public class IndexManagerImp : IIndexManager
    {
        public IndexManagerImp()
        {
            //初始化PanGu的配置
            PanGu.Segment.Init(DocumentEnvironment.PanGuXml);
        }

        public PanGuAnalyzer PanGuAnalyzer
        {
            get
            {
                return new PanGuAnalyzer();
            }
        }

        public string IndexDic
        {
            get
            {
                return DocumentEnvironment.IndexDir;
            }
        }
        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public bool CreateIndex(AbstractFileBase fileInfo)
        {
            try
            {
                IndexWriter writer = new IndexWriter(FSDirectory.Open(new System.IO.DirectoryInfo(IndexDic)), PanGuAnalyzer, IndexWriter.MaxFieldLength.LIMITED);

                //FileStream fs = File.OpenRead(Path.Combine(fileInfo.FilePath, fileInfo.FileName));

                //1.创建Document对象
                Document doc = new Document();

                //2.给doc对象添加Field
                doc.Add(new Field("fileId", fileInfo.FileId, Field.Store.YES, Field.Index.NOT_ANALYZED));
                //doc.Add(new Field("fileName", fileInfo.FileName, Field.Store.YES, Field.Index.ANALYZED));
                //doc.Add(new Field("filePath", fileInfo.FilePath, Field.Store.YES, Field.Index.ANALYZED));
                //doc.Add(new Field("createTime", fileInfo.CreateTime, Field.Store.YES, Field.Index.ANALYZED));
                //doc.Add(new Field("upLoadPersonId", fileInfo.UpLoadPersonId, Field.Store.YES, Field.Index.ANALYZED));
                //doc.Add(new Field("content", new StreamReader(fs, Encoding.UTF8)));
                doc.Add(new Field("content", fileInfo.Content, Field.Store.YES, Field.Index.ANALYZED));

                //将doc对象写入索引文件
                writer.AddDocument(doc);

                writer.Optimize();
                writer.Commit();
                writer.Close();

                //修改数据库的
                LibDataAccess dataAccess = new LibDataAccess();
                dataAccess.ExecuteNonQuery(string.Format("update DMDOCUMENT set ISFULLINDEX = 1 where DOCID = {0}", LibStringBuilder.GetQuotString(fileInfo.FileId)));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 删除索引
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public bool DeleteIndex(AbstractFileBase fileInfo)
        {
            try
            {
                IndexWriter writer = new IndexWriter(FSDirectory.Open(new System.IO.DirectoryInfo(IndexDic)), PanGuAnalyzer, IndexWriter.MaxFieldLength.LIMITED);
                writer.DeleteDocuments(new Term("fileId", fileInfo.FileId));
                //这两句一定要执行  
                writer.Optimize();
                writer.Close();

                LibDataAccess dataAccess = new LibDataAccess();
                dataAccess.ExecuteNonQuery(string.Format("update DMDOCUMENT set ISFULLINDEX = 0 where DOCID = {0}", LibStringBuilder.GetQuotString(fileInfo.FileId)));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 处理关键字为索引格式
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        private string GetKeyWordsSplitBySpace(string keywords)
        {
            PanGuTokenizer ktTokenizer = new PanGuTokenizer();
            StringBuilder result = new StringBuilder();
            ICollection<WordInfo> words = ktTokenizer.SegmentToWordInfos(keywords);
            foreach (WordInfo word in words)
            {
                if (word == null)
                {
                    continue;
                }
                result.AppendFormat("{0}^{1}.0 ", word.Word, (int)Math.Pow(3, word.Rank));
            }
            return result.ToString().Trim();
        }

        public object SearchNextIndex(Dictionary<string, string> dic, int pageIndex, int pageSize, LibHandle handle, string prevLastFileId)
        {
            BooleanQuery bQuery = new BooleanQuery();
            foreach (var item in dic)
            {
                QueryParser parse = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, item.Key, PanGuAnalyzer);
                Query query = parse.Parse(GetKeyWordsSplitBySpace(item.Value));
                parse.SetDefaultOperator(QueryParser.Operator.AND);
                bQuery.Add(query, BooleanClause.Occur.MUST);
            }

            IndexSearcher search = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(IndexDic)), true);
            Stopwatch stopwatch = Stopwatch.StartNew();
            //SortField构造函数第三个字段true为降序,false为升序
            Sort sort = new Sort(new SortField("fileId", SortField.DOC, true));
            TopDocs docs = search.Search(bQuery, null, 1000, sort);
            stopwatch.Stop();
            SearchResult doclist = new SearchResult();
            if (docs != null && docs.totalHits > 0)
            {
                doclist.SearchTime = stopwatch.ElapsedMilliseconds;
                doclist.TotalHits = 0;
                List<AbstractFileBase> fileList = new List<AbstractFileBase>();
                List<string> docIds = new List<string>();
                for (int i = (pageIndex - 1) * pageSize; i < docs.totalHits; i++)
                {
                    Document doc = search.Doc(docs.scoreDocs[i].doc);
                    string fileId = doc.Get("fileId").ToString();
                    if (string.Compare(fileId, prevLastFileId) > 0)
                    {
                        continue;
                    }
                    docIds.Add(fileId);
                    fileList.Add(new TextFileInfo()
                    {
                        FileId = fileId,
                        Content = doc.Get("content").ToString(),
                    });
                    #region 每循环100次，或最后一次循环
                    if (((i - (pageIndex - 1) * pageSize) % 100 == 0 && i != 0) || i == docs.totalHits - 1)
                    {
                        List<string> allowDocIds = DMPermissionControl.Default.FilterDocIds(handle, DMFuncPermissionEnum.Read, docIds);
                        if (doclist.Docs.Count < pageSize)
                        {
                            foreach (AbstractFileBase file in fileList)
                            {
                                if (allowDocIds.Contains(file.FileId))
                                {

                                    if (doclist.Docs.Count < pageSize)
                                    {
                                        doclist.Docs.Add(file);
                                        if (doclist.Docs.Count == pageSize)
                                        {
                                            return doclist;
                                        }
                                    }
                                }
                            }
                        }
                        docIds.Clear();
                        fileList.Clear();
                    }
                    #endregion
                }
            }
            return doclist;
        }

        public object SearchIndex(Dictionary<string, string> dic, int pageIndex, int pageSize, LibHandle handle)
        {
            BooleanQuery bQuery = new BooleanQuery();
            foreach (var item in dic)
            {
                QueryParser parse = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, item.Key, PanGuAnalyzer);
                Query query = parse.Parse(GetKeyWordsSplitBySpace(item.Value));
                parse.SetDefaultOperator(QueryParser.Operator.AND);
                bQuery.Add(query, BooleanClause.Occur.MUST);
            }

            IndexSearcher search = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(IndexDic)), true);
            Stopwatch stopwatch = Stopwatch.StartNew();
            //SortField构造函数第三个字段true为降序,false为升序
            Sort sort = new Sort(new SortField("fileId", SortField.DOC, true));
            TopDocs docs = search.Search(bQuery, null, 1000, sort);
            stopwatch.Stop();
            SearchResult doclist = new SearchResult();
            if (docs != null && docs.totalHits > 0)
            {
                doclist.SearchTime = stopwatch.ElapsedMilliseconds;
                doclist.TotalHits = 0;
                List<AbstractFileBase> fileList = new List<AbstractFileBase>();
                List<string> docIds = new List<string>();
                for (int i = (pageIndex - 1) * pageSize; i < docs.totalHits; i++)
                {
                    Document doc = search.Doc(docs.scoreDocs[i].doc);
                    string fileId = doc.Get("fileId").ToString();
                    docIds.Add(fileId);
                    fileList.Add(new TextFileInfo()
                    {
                        FileId = fileId,
                        Content = doc.Get("content").ToString(),
                    });
                    #region 每循环100次，或最后一次循环
                    if (((i - (pageIndex - 1) * pageSize) % 100 == 0 && i != 0) || i == docs.totalHits - 1)
                    {
                        List<string> allowDocIds = DMPermissionControl.Default.FilterDocIds(handle, DMFuncPermissionEnum.Read, docIds);
                        if (doclist.Docs.Count < pageSize)
                        {
                            foreach (AbstractFileBase file in fileList)
                            {
                                if (allowDocIds.Contains(file.FileId))
                                {

                                    if (doclist.Docs.Count < pageSize)
                                    {
                                        doclist.Docs.Add(file);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        doclist.TotalHits += allowDocIds.Count;
                        docIds.Clear();
                        fileList.Clear();
                    }
                    #endregion
                }

            }
            return doclist;
        }

        public object SearchPrevIndex(Dictionary<string, string> dic, int pageIndex, int pageSize, LibHandle handle, string lastFileId)
        {
            BooleanQuery bQuery = new BooleanQuery();
            foreach (var item in dic)
            {
                QueryParser parse = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, item.Key, PanGuAnalyzer);
                Query query = parse.Parse(GetKeyWordsSplitBySpace(item.Value));
                parse.SetDefaultOperator(QueryParser.Operator.AND);
                bQuery.Add(query, BooleanClause.Occur.MUST);
            }

            IndexSearcher search = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(IndexDic)), true);
            Stopwatch stopwatch = Stopwatch.StartNew();
            //SortField构造函数第三个字段true为降序,false为升序
            Sort sort = new Sort(new SortField("fileId", SortField.DOC, true));
            TopDocs docs = search.Search(bQuery, null, 1000, sort);
            stopwatch.Stop();
            SearchResult doclist = new SearchResult();
            if (docs != null && docs.totalHits > 0)
            {
                doclist.SearchTime = stopwatch.ElapsedMilliseconds;
                doclist.TotalHits = 0;
                List<AbstractFileBase> fileList = new List<AbstractFileBase>();
                List<string> docIds = new List<string>();
                Queue docsQueue = new Queue();
                for (int i = (pageIndex - 1) * pageSize; i < docs.totalHits; i++)
                {
                    Document doc = search.Doc(docs.scoreDocs[i].doc);
                    string fileId = doc.Get("fileId").ToString();
                    docIds.Add(fileId);
                    fileList.Add(new TextFileInfo()
                    {
                        FileId = fileId,
                        Content = doc.Get("content").ToString(),
                    });
                    #region 每循环100次，或最后一次循环
                    if (((i - (pageIndex - 1) * pageSize) % 100 == 0 && i != 0) || i == docs.totalHits - 1)
                    {
                        List<string> allowDocIds = DMPermissionControl.Default.FilterDocIds(handle, DMFuncPermissionEnum.Read, docIds);
                        foreach (AbstractFileBase file in fileList)
                        {
                            if (allowDocIds.Contains(file.FileId))
                            {

                                if (string.Compare(file.FileId, lastFileId) <= 0)
                                {
                                    foreach (object o in docsQueue.ToArray())
                                    {
                                        doclist.Docs.Add((AbstractFileBase)o);
                                    }
                                    return doclist;
                                }
                                else
                                {
                                    if (docsQueue.Count >= pageSize)
                                    {
                                        docsQueue.Dequeue();
                                    }
                                    docsQueue.Enqueue(file);
                                }
                            }
                        }
                        docIds.Clear();
                        fileList.Clear();
                    }
                    #endregion
                }

            }
            return doclist;
        }
    }

    public class SearchResult
    {
        private List<AbstractFileBase> docs;
        private int totalHits;
        private long searchTime;
        
        /// <summary>
        /// 查找到的记录的总条数
        /// </summary>
        public int TotalHits
        {
            get
            {
                return totalHits;
            }

            set
            {
                totalHits = value;
            }
        }
        /// <summary>
        /// 查询的时间
        /// </summary>
        public long SearchTime
        {
            get
            {
                return searchTime;
            }

            set
            {
                searchTime = value;
            }
        }
        /// <summary>
        /// 查出来的doc对象
        /// </summary>
        public List<AbstractFileBase> Docs
        {
            get
            {
                if(docs == null)
                {
                    docs = new List<AbstractFileBase>();
                }
                return docs;
            }
        }
    }
}
