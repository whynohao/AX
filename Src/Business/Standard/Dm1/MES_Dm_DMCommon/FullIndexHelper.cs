/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的全文索引委托事件帮助类。
 * 创建标识：Zhangkj 2016/12/28
 * 
************************************************************************/
using AxCRL.Comm.Runtime;
using AxCRL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jikon.MES_Dm.DMCommon
{
    public class FullIndexHelper
    {
        #region 全文索引相关
        /// <summary>
        /// 文档需要创建全文索引
        /// </summary>
        /// <param name="docId">文档编号</param>
        /// <param name="docType">文档类型，文档的扩展名，含.号</param>
        /// <param name="modifyVerId">修订版标识号</param>
        /// <param name="isFullNew">是否为全新的文档。新增和替换的认为是全新的，编辑的认为不是全新的</param>
        /// <param name="fullPath">文档在文件系统上的绝对路径</param>
        public delegate void NewDocArrivedToFullIndexCall(string docId, string docType, int modifyVerId, bool isFullNew, string fullPath);
        /// <summary>
        /// 有文档需要创建全新索引的事件。索引创建完毕后可通过调用DealFullIndexResult反馈结果
        /// </summary>
        public static event NewDocArrivedToFullIndexCall NewDocArrivedToFullIndex;
        /// <summary>
        /// 删除文档的全文索引。如果修订版号码小于等于0则表示删除所有修订版的索引，否则为删除指定修订版索引
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="modifyVerId"></param>
        public delegate void DeleteDocFullIndexCall(string docId, int modifyVerId);
        /// <summary>
        /// 需要删除文档的全文索引委托事件
        /// </summary>
        public static event DeleteDocFullIndexCall NeedDeleteDocFullIndex;

        /// <summary>
        /// 触发文档创建全文索引事件
        /// </summary>
        /// <param name="docId">文档编号</param>
        /// <param name="docType">文档类型，文档的扩展名，含.号</param>
        /// <param name="modifyVerId">修订版标识号</param>
        /// <param name="isFullNew">是否为全新的文档。新增和替换的认为是全新的，编辑的认为不是全新的</param>
        /// <param name="fullPath">文档在文件系统上的绝对路径</param>
        public static void RaiseNewDocArrivedToFullIndex(string docId, string docType, int modifyVerId, bool isFullNew, string fullPath)
        {
            try
            {
                if (NewDocArrivedToFullIndex != null)
                {
                    //异步调用事件
                    NewDocArrivedToFullIndex.BeginInvoke(docId, docType, modifyVerId, isFullNew, fullPath, null, null);
                    //Delegate[] delegAry = NewDocArrivedToFullIndex.GetInvocationList();
                    ////遍历委托列表
                    //foreach (NewDocArrivedToFullIndexCall deleg in delegAry)
                    //{
                    //    //异步调用委托
                    //    deleg.BeginInvoke(docId, docType, modifyVerId, isFullNew, fullPath, null, null);
                    //}
                }
            }
            catch (Exception exp)
            {
                //发生异常时记录日志
                DMCommonMethod.WriteLog("RaiseNewDocArrivedToFullIndex", string.Format("DocId:{0}\r\nDocType:{1}\r\nModifyVerId:{2}\r\nIsFullNew:{3}\r\nFullPath:{4}\r\nError:{5}",
                        docId, docType, modifyVerId, isFullNew, fullPath, exp.ToString()));
            }
            //if (NewDocArrivedToFullIndex != null)
            //{
            //    ThreadPool.QueueUserWorkItem(delegate {
            //        try
            //        {
            //            NewDocArrivedToFullIndex(docId, docType, modifyVerId, isFullNew, fullPath);
            //        }
            //        catch (Exception exp)
            //        {
            //            //发生异常时记录日志
            //            DMCommonMethod.WriteLog("RaiseNewDocArrivedToFullIndex", string.Format("DocId:{0}\r\nDocType:{1}\r\nModifyVerId:{2}\r\nIsFullNew:{3}\r\nFullPath:{4}\r\nError:{5}",
            //                    docId, docType, modifyVerId, isFullNew, fullPath, exp.ToString()));
            //        }
            //    });
            //}
        }
        /// <summary>
        /// 触发需要删除文档的全文索引事件
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="modifyVerId">默认为删除所有修订版索引。如果修订版号码小于等于0则表示删除所有修订版的索引，否则为删除指定修订版索引</param>
        public static void RaiseNeedDeleteDocFullIndex(string docId, int modifyVerId = -1)
        {
            try
            {
                if (NeedDeleteDocFullIndex != null)
                {
                    //异步调用事件
                    NeedDeleteDocFullIndex.BeginInvoke(docId, modifyVerId, null, null);
                    //Delegate[] delegAry = NeedDeleteDocFullIndex.GetInvocationList();
                    ////遍历委托列表
                    //foreach (DeleteDocFullIndexCall deleg in delegAry)
                    //{
                    //    //异步调用委托
                    //    deleg.BeginInvoke(docId, modifyVerId, null, null);
                    //}                    
                }
            }
            catch (Exception exp)
            {
                //发生异常时记录日志
                DMCommonMethod.WriteLog("RaiseNeedDeleteDocFullIndex", string.Format("DocId:{0}\r\nModifyVerId:{1}\r\nError:{2}",
                        docId, modifyVerId, exp.ToString()));
            }

            //if (NeedDeleteDocFullIndex != null)
            //{
            //    //异步调用事件
            //    ThreadPool.QueueUserWorkItem(delegate {
            //        try
            //        {
            //            NeedDeleteDocFullIndex(docId, modifyVerId);
            //        }
            //        catch (Exception exp)
            //        {
            //            //发生异常时记录日志
            //            DMCommonMethod.WriteLog("RaiseNeedDeleteDocFullIndex", string.Format("DocId:{0}\r\nModifyVerId:{1}\r\nError:{2}",
            //                    docId, modifyVerId, exp.ToString()));
            //        }
            //    });
            //}
        }
        /// <summary>
        /// 全文索引创建结果的处理
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="modifyVerId">修订版标识号</param>
        /// <param name="isSuccess"></param>
        public static void DealFullIndexResult(string docId, int modifyVerId, bool isSuccess)
        {
            try
            {
                LibDataAccess dataAccess = new LibDataAccess();
                LibDBTransaction trans = dataAccess.BeginTransaction();
                try
                {
                    string sql = "";
                    if (dataAccess.DatabaseType == LibDatabaseType.SqlServer)
                        sql = string.Format(
                            " update a                        " +
                            " set a.ISFULLINDEX = {2}         " +
                            " from DMDOCUMENT a               " +
                            " where a.DOCID = '{0}' and (select max(DOCMODIFYID) from DMDOCMODIFYHISTORY where DOCID = '{0}') = {1}",//修订号与最新修订号相同才设置索引结果
                            docId, modifyVerId, isSuccess ? 1 : 0);
                    else
                    {
                        //Oracle To do
                    }

                    if (string.IsNullOrEmpty(sql) == false)
                    {
                        dataAccess.ExecuteNonQuery(sql);
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
            catch (Exception exp)
            {
                //发生异常时记录日志
                DMCommonMethod.WriteLog("DealFullIndexResult", string.Format("DocId:{0}\r\nIsSuccess{1}\r\nnError:{2}",
                        docId, isSuccess, exp.ToString()));
            }
        }
        #endregion
    }
}
