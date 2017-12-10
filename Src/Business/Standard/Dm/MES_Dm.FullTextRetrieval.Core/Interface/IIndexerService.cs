/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：全文检索的服务接口
 * 创建标识：CHENQI 2016/12/01
 * 修改标识：
 * 修改描述：
************************************************************************/
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Runtime.Serialization;
using Jikon.MES_Dm.DMCommon;

namespace MES_Dm.FullTextRetrieval.Core
{
    /// <summary>
    /// 全文索引的服务接口
    /// </summary>
    [ServiceContract]
    public interface IIndexer
    {
        /// <summary>
        /// 初始化所有索引
        /// </summary>
        /// <param>人员权限</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "initIndexs", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool InitIndexs();

        /// <summary>
        /// 为文件或文件夹添加索引
        /// </summary>
        /// <param>文件标识符</param>
        /// <param>人员权限</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "addIndex")]
        bool AddIndex(string fileId);

        /// <summary>
        /// 删除文件或文件夹的索引
        /// </summary>
        /// <param>文件标识符</param>
        /// <param>人员权限</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "deleteIndex")]
        bool DeleteIndex(string fileId);

        /// <summary>
        /// 删除所有索引
        /// </summary>
        /// <param>人员权限</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "destoryIndexs")]
        bool DestoryIndexs();

        /// <summary>
        /// 查询索引
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="pageNum">第几页</param>
        /// <param>人员权限</param>
        /// <param name="fileId">最后一个查到的id，第一次查的时候为''</param>
        /// <returns></returns
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "searchIndex")]
        ResultSet SearchIndex(string key, int pageNum, string userHandle, string lastFileId, string nextFileId);
    }

    [DataContract]
    public class FileInfoItem
    {
        /// <summary>
        /// 文件名
        /// </summary>
        [DataMember]
        public string FileName
        {
            set;
            get;
        }

        /// <summary>
        /// 创建者
        /// </summary>
        //[DataMember]
        //public string Creator
        //{
        //    set;
        //    get;
        //}
        /// <summary>
        /// 所在目录
        /// </summary>
        [DataMember]
        public string Path
        {
            set;
            get;
        }
        /// <summary>
        /// 创建时间
        /// </summary>
        //[DataMember]
        //public string CreateTime
        //{
        //    set;
        //    get;
        //}
        /// <summary>
        /// 概要内容
        /// </summary>
        [DataMember]
        public string Contents
        {
            set;
            get;
        }
        /// <summary>
        /// 文件标志符
        /// </summary>
        [DataMember]
        public string FileId
        {
            get;
            set;
        }
        [DataMember]
        public string DirId
        {
            get;
            set;
        }
        [DataMember]
        public DirTypeEnum DirType
        {
            set;
            get;
        }
    }

    /// <summary>
    /// 查询返回的结果
    /// </summary>
    [DataContract]
    public class ResultSet
    {
        private List<FileInfoItem> fileInfoItems;

        /// <summary>
        /// 当前页码
        /// </summary>
        [DataMember]
        public int PageNum
        {
            get;
            set;
        }
        /// <summary>
        /// 共查找到的记录数
        /// </summary>
        [DataMember]
        public int ResultsCount
        {
            get;
            set;
        }
        /// <summary>
        /// 总的页数
        /// </summary>
        [DataMember]
        public int PageCount
        {
            get;
            set;
        }
        /// <summary>
        /// 查询所花的时间
        /// </summary>
        [DataMember]
        public long SearchTime
        {
            get;
            set;
        }

        /// <summary>
        /// 保存具体信息的数组
        /// </summary>
        [DataMember]
        public List<FileInfoItem> FileInfoItems
        {
            get
            {
                if (fileInfoItems == null)
                {
                    fileInfoItems = new List<FileInfoItem>();
                }
                return fileInfoItems;
            }
        }
    }
}
