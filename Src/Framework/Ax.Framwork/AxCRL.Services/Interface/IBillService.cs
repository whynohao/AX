using AxCRL.Bcf;
using AxCRL.Comm.Bill;
using AxCRL.Comm.Entity;
using AxCRL.Core.Comm;
using AxCRL.Services.Entity;
using AxCRL.Template;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Services
{
    [ServiceContract]
    public interface IBillService
    {
        /// <summary>
        /// 调用中间层方法
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "invorkBcf", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string ExecuteBcfMethod(ExecuteBcfMethodParam param);
        /// <summary>
        /// 调用报表方法（看板调用，创建syshandle）
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "invorkBcfRpt", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string ExecuteBcfMethodRpt(ExecuteBcfMethodParam param);
        /// <summary>
        /// 批次调用中间层方法
        /// </summary>
        /// <param name="param"></param>
        /// <param name="batchParams"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "invorkBatchBcf", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string BatchExecBcfMethod(ExecuteBcfMethodParam param, IList<string[]> batchParams);
        /// <summary>
        /// 批次导入
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <param name="fileName"></param>
        [OperationContract]
        [WebInvoke(UriTemplate = "invorkBatchImport", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string BatchImportData(string handle, string progId, string fileName, string entryParam = null);
        /// <summary>
        /// 批次导出
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <param name="batchParams"></param>
        [OperationContract]
        [WebInvoke(UriTemplate = "invorkBatchExport", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string BatchExportData(string handle, string progId, IList<object[]> batchParams);
        /// <summary>
        /// 导出全部数据
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <param name="pkStr"></param>
        [OperationContract]
        [WebInvoke(UriTemplate = "exportAllData", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string ExportAllData(string handle, string progId, string pkStr, BillListingQuery listingQuery = null);
        /// <summary>
        /// 查询单据数据清单
        /// </summary>
        /// <param name="listingQuery"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "getBillListing", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string GetBillListing(BillListingQuery listingQuery);
        /// <summary>
        /// 查询功能Bcf清单页指定分类树节点的下级节点列表
        /// </summary>
        /// <param name="treeListingQuery"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "getBillTreeListing", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        List<TreeListingNode> GetBillTreeListing(TreeListingQuery treeListingQuery);
        /// <summary>
        /// 取格式化单位
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "getFormatUnit?unitId={unitId}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        int GetFormatUnit(string unitId);
        /// <summary>
        /// 取格式化单位
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(UriTemplate = "getCompanyFormat?companyId={companyId}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        CompanyParam GetCompanyFormat(string companyId);
        /// <summary>
        /// 查询功能字段
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "selectFuncField", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string SelectFuncField(string handle, string progId, int tableIndex = 0);
        /// <summary>
        /// 查询表头字段
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "selectQueryField", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string SelectQueryField(string handle, string progId);
        /// <summary>
        /// 保存方案
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <param name="entryParam"></param>
        /// <param name="displayScheme"></param>
        [OperationContract]
        [WebInvoke(UriTemplate = "saveDisplayScheme", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void SaveDisplayScheme(string handle, string progId, string entryParam, string displayScheme);
        /// <summary>
        /// 删除方案
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <param name="entryParam"></param>
        [OperationContract]
        [WebInvoke(UriTemplate = "clearDisplayScheme", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void ClearDisplayScheme(string handle, string progId, string entryParam);
        /// <summary>
        /// 保存清单方案
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <param name="entryParam"></param>
        /// <param name="displayScheme"></param>
        [OperationContract]
        [WebInvoke(UriTemplate = "saveBillListingScheme", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void SaveBillListingScheme(string handle, string progId, string entryParam, string displayScheme);
        /// <summary>
        /// 删除清单方案
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <param name="entryParam"></param>
        [OperationContract]
        [WebInvoke(UriTemplate = "clearBillListingScheme", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void ClearBillListingScheme(string handle, string progId, string entryParam);
        /// <summary>
        /// 模糊查询
        /// </summary>
        /// <param name="relSource"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "fuzzySearchField", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        IList<FuzzyResult> FuzzySearchField(string handle, string relSource, string query, string condition, int tableIndex, string selectSql = "");
        /// <summary>
        /// 查询关联字段
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="fields"></param>
        /// <param name="relSource"></param>
        /// <param name="curPk"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "checkFieldValue", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string CheckFieldValue(string handle, string fields, string relSource, string curPk, string condition, int tableIndex);
        /// <summary>
        /// 获取用户消息
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="startTime"></param>
        /// <param name="onlyUnRead"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "getMyNews", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        List<LibNews> GetMyNews(string handle, long startTime, bool onlyUnRead);
        /// <summary>
        /// 获取用户消息数量
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="startTime"></param>
        /// <param name="onlyUnRead"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "getUnreadNews", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        int GetUnreadNews(string handle, long startTime, bool onlyUnRead);
        /// <summary>
        /// 设置消息的状态
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="newsList"></param>
        [OperationContract]
        [WebInvoke(UriTemplate = "setMyNewsReadState", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string SetMyNewsReadState(string handle, string[] newsList);
        /// <summary>
        /// 能使用该功能
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "canUseFunc", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool CanUseFunc(string handle, string progId);
        /// <summary>
        /// 能使用该功能
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "checkAllPermission", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        List<int> CheckAllPermission(string handle, string progId);
        /// <summary>
        /// 获取特征控件排版
        /// </summary>
        /// <param name="attrId"></param>
        /// <param name="attrCode"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "getAttrControl", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string GetAttributeControl(string attrId, string attrCode);
        /// <summary>
        /// 获取特征描述
        /// </summary>
        /// <param name="attrId"></param>
        /// <param name="attrCode"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "getAttrDesc", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        LibAttrInfo GetAttributeDesc(string attrId, string attrCode);
        /// <summary>
        /// 获取单据类别
        /// </summary>
        /// <param name="progId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "getBillType", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        BillType GetBillType(string progId);

        /// <summary>
        /// 获取入库参数渲染内容
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "getEntryRender", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string GetEntryRender(string handle, string progId);
        /// <summary>
        /// 载入附件信息
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="attachmentSrc"></param>
        /// <param name="progId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "loadAttachInfo", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        LoadAttachInfo LoadAttachInfo(string handle, string attachmentSrc, string progId, List<Dictionary<string, object>> data);

        [OperationContract]
        [WebInvoke(UriTemplate = "getRptFields", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        IList<FuzzyResult> GetRptFields(string progId);


        /// <summary>
        /// 获取发布的功能
        /// </summary>  
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "getPublishFunc", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        List<FuncInfo> GetPublishFunc();
        /// <summary>
        /// 刪除发布的功能
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="ProgId"></param>
        /// <param name="EntryParam"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "deleteFuncPublish", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void DeleteFuncPublish(string handle, string ProgId, string EntryParam);
        /// <summary>
        /// 发布的功能添加到数据库中
        /// </summary>
        /// <param name="funcInfoJson"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "setFuncPublish", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        int SetFuncPublish(string funcInfoJson);

        /// <summary>
        /// 获取所有Funclist数据
        /// </summary>
        /// <param name="relSource"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "getSelectData", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        IList<FuzzyResult> GetSelectData(string handle, string relSource, string query, string condition, int tableIndex);
        /// <summary>
        /// 是否管理员
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "checkAdmin", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        bool CheckAdmin(string handle);
        /// <summary>
        /// 获取入口参数
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="progId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "getEntryParam", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        string GetEntryParam(string handle, string progId);

    }

    [DataContract]
    public class FuncInfo
    {
        private string _menuItem;
        private string _progId;
        private string _progName;
        private int _billType;
        private string _entryParam;
        private string _publishDate;

        [DataMember]
        public string MenuItem
        {
            get
            {
                return _menuItem;
            }

            set
            {
                _menuItem = value;
            }
        }
        [DataMember]
        public string ProgId
        {
            get
            {
                return _progId;
            }

            set
            {
                _progId = value;
            }
        }
        [DataMember]
        public string ProgName
        {
            get
            {
                return _progName;
            }

            set
            {
                _progName = value;
            }
        }
        [DataMember]
        public int BillType
        {
            get
            {
                return _billType;
            }

            set
            {
                _billType = value;
            }
        }
        [DataMember]
        public string EntryParam
        {
            get
            {
                return _entryParam;
            }

            set
            {
                _entryParam = value;
            }
        }
        [DataMember]
        public string PublishDate
        {
            get
            {
                return _publishDate;
            }

            set
            {
                _publishDate = value;
            }
        }
    }

    [DataContract]
    public class BillListingResult
    {
        public BillListingResult(System.Data.DataTable data, string columns, string fields, string[] pk, string filterField)
        {
            this._Data = data;
            this._Columns = columns;
            this._Fields = fields;
            this._Pk = pk;
            this._FilterField = filterField;
        }
        private System.Data.DataTable _Data;
        [DataMember]
        public System.Data.DataTable Data
        {
            get { return _Data; }
            set { _Data = value; }
        }
        private string _Columns;
        [DataMember]
        public string Columns
        {
            get { return _Columns; }
            set { _Columns = value; }
        }
        private string _Fields;
        [DataMember]
        public string Fields
        {
            get { return _Fields; }
            set { _Fields = value; }
        }
        private string[] _Pk;
        [DataMember]
        public string[] Pk
        {
            get { return _Pk; }
            set { _Pk = value; }
        }
        private string _FilterField;
        [DataMember]
        public string FilterField
        {
            get { return _FilterField; }
            set { _FilterField = value; }
        }
    }

    [DataContract]
    public class BillListingQuery
    {
        private string _Handle;
        [DataMember]
        public string Handle
        {
            get { return _Handle; }
            set { _Handle = value; }
        }
        private string _ProgId;
        [DataMember]
        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }
        private int _pageCount;
        [DataMember]
        public int PageCount
        {
            get { return _pageCount; }
            set { _pageCount = value; }
        }
        private int _pageSize;
        [DataMember]
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }
        //private string _FieldName = string.Empty;
        //[DataMember]
        //public string FieldName
        //{
        //    get { return _FieldName; }
        //    set { _FieldName = value; }
        //}
        //private CompareOperater _Compare = CompareOperater.Contain;
        //[DataMember]
        //public CompareOperater Compare
        //{
        //    get { return _Compare; }
        //    set { _Compare = value; }
        //}
        //private string _Query = string.Empty;
        //[DataMember]
        //public string Query
        //{
        //    get { return _Query; }
        //    set { _Query = value; }
        //}
        private LibQueryCondition _Condition;
        [DataMember]
        public LibQueryCondition Condition
        {
            get { return _Condition; }
            set { _Condition = value; }
        }

        private BillListingTimeFilter _TimeFilter = 0;
        [DataMember]
        public BillListingTimeFilter TimeFilter
        {
            get { return _TimeFilter; }
            set { _TimeFilter = value; }
        }
        private int _Filter = 0;
        [DataMember]
        public int Filter
        {
            get { return _Filter; }
            set { _Filter = value; }
        }
        private string _EntryParam;
        [DataMember]
        public string EntryParam
        {
            get { return _EntryParam; }
            set { _EntryParam = value; }
        }
    }
    [DataContract]
    public enum CompareOperater
    {
        Contain = 0,
        Equal = 1,
        GreaterThanEqual = 2,
        LessThanEqual = 3,
        GreaterThan = 4,
        LessThan = 5
    }
    [DataContract]
    public enum BillListingTimeFilter
    {
        None = 0,
        LatestWeek = 1,
        LatestMonth = 2,
        LatestQuarter = 3
    }

    public enum BillListingFilter
    {
        Draft = 1,
        UnRelease = 2,
        Release = 4,
        Invalid = 8,
        EndCase = 16,
        UnAudit = 32,
        Audit = 64,
        UnValidity = 128,
        Validity = 256
    }

    [DataContract]
    public class LibQueryField
    {
        private string _Field;
        private string _DisplayText;
        private LibDataType _DataType;
        private string _ControlJs;

        public LibQueryField()
        {

        }
        public LibQueryField(string field, string displayText, LibDataType dataType)
        {
            this._Field = field;
            this._DisplayText = displayText;
            this._DataType = dataType;
        }
        [DataMember]
        public LibDataType DataType
        {
            get { return _DataType; }
            set { _DataType = value; }
        }
        [DataMember]
        public string DisplayText
        {
            get { return _DisplayText; }
            set { _DisplayText = value; }
        }
        [DataMember]
        public string Field
        {
            get { return _Field; }
            set { _Field = value; }
        }
        [DataMember]
        public string ControlJs
        {
            get { return _ControlJs; }
            set { _ControlJs = value; }
        }
    }
    [DataContract]
    public class LibNews
    {
        private string _NewsId;
        private string _Title;
        private string _PersonName;
        private int _CreateTime;
        private int _CreateDate;
        private string _InfoId;
        private string _MainContent;
        private string _ProgId;
        private string _DisplayText;
        private BillType _BillType;
        private string _CurPks;
        private string _EntryParam;
        private bool _IsRead;
        [DataMember]
        public string NewsId
        {
            get { return _NewsId; }
            set { _NewsId = value; }
        }
        [DataMember]
        public bool IsRead
        {
            get { return _IsRead; }
            set { _IsRead = value; }
        }
        [DataMember]
        public string EntryParam
        {
            get { return _EntryParam; }
            set { _EntryParam = value; }
        }
        [DataMember]
        public string CurPks
        {
            get { return _CurPks; }
            set { _CurPks = value; }
        }

        [DataMember]
        public BillType BillType
        {
            get { return _BillType; }
            set { _BillType = value; }
        }

        [DataMember]
        public string DisplayText
        {
            get { return _DisplayText; }
            set { _DisplayText = value; }
        }

        [DataMember]
        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }

        [DataMember]
        public string MainContent
        {
            get { return _MainContent; }
            set { _MainContent = value; }
        }
        [DataMember]
        public string InfoId
        {
            get { return _InfoId; }
            set { _InfoId = value; }
        }
        [DataMember]
        public int CreateDate
        {
            get { return _CreateDate; }
            set { _CreateDate = value; }
        }
        [DataMember]
        public int CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
        [DataMember]
        public string PersonName
        {
            get { return _PersonName; }
            set { _PersonName = value; }
        }
        [DataMember]
        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }
        private string _SourceSiteId = string.Empty;
        /// <summary>
        /// 来源站点代码
        /// </summary>
        [DataMember]
        public string SourceSiteId
        {
            get { return _SourceSiteId; }
            set { _SourceSiteId = value; }
        }
        private string _SourceSiteName = string.Empty;
        /// <summary>
        /// 来源站点简称
        /// </summary>
        [DataMember]
        public string SourceSiteName
        {
            get { return _SourceSiteName; }
            set { _SourceSiteName = value; }
        }
        private string _SourceSiteFullName = string.Empty;
        /// <summary>
        /// 来源站点全称
        /// </summary>
        [DataMember]
        public string SourceSiteFullName
        {
            get { return _SourceSiteFullName; }
            set { _SourceSiteFullName = value; }
        }
        private string _SourceSiteUrl = string.Empty;
        /// <summary>
        /// 来源站点Url
        /// </summary>
        [DataMember]
        public string SourceSiteUrl
        {
            get { return _SourceSiteUrl; }
            set { _SourceSiteUrl = value; }
        }
    }

    [DataContract]
    public class LibAttrInfo
    {
        private string _AttrDesc;
        private string _AttrCode;
        [DataMember]
        public string AttrCode
        {
            get { return _AttrCode; }
            set { _AttrCode = value; }
        }
        [DataMember]
        public string AttrDesc
        {
            get { return _AttrDesc; }
            set { _AttrDesc = value; }
        }
    }

    [DataContract]
    public class LibAttrControl
    {
        private string _Renderer;
        private string _Fields;
        private string _NewRowObj;
        private int _RelationMark;
        private int _IntervalMark;
        [DataMember]
        public int IntervalMark
        {
            get { return _IntervalMark; }
            set { _IntervalMark = value; }
        }

        [DataMember]
        public int RelationMark
        {
            get { return _RelationMark; }
            set { _RelationMark = value; }
        }

        [DataMember]
        public string NewRowObj
        {
            get { return _NewRowObj; }
            set { _NewRowObj = value; }
        }
        [DataMember]
        public string Fields
        {
            get { return _Fields; }
            set { _Fields = value; }
        }
        [DataMember]
        public string Renderer
        {
            get { return _Renderer; }
            set { _Renderer = value; }
        }
    }

    /// <summary>
    /// 附件信息
    /// </summary>
    [DataContract]
    public class LibAttachInfo
    {
        private int _OrderId;
        private string _AttachName;
        private string _FileName;
        private List<LibAttachHistory> _HistoryList;
        private string _DocId;
        private string _DirId;
        /// <summary>
        /// 标识
        /// </summary>
        [DataMember]
        public int OrderId
        {
            get { return _OrderId; }
            set { _OrderId = value; }
        }

        /// <summary>
        /// 历史
        /// </summary>
        [DataMember]
        public List<LibAttachHistory> HistoryList
        {
            get
            {
                if (_HistoryList == null)
                    _HistoryList = new List<LibAttachHistory>();
                return _HistoryList;
            }
        }
        /// <summary>
        /// 附件名
        /// </summary>
        [DataMember]
        public string AttachName
        {
            get { return _AttachName; }
            set { _AttachName = value; }
        }
        /// <summary>
        /// 文件名
        /// </summary>
        [DataMember]
        public string FileName
        {
            get { return _FileName; }
            set { _FileName = value; }
        }
        /// <summary>
        /// 对应到文档库中的DOCID
        /// </summary>
        [DataMember]
        public string DocId
        {
            get { return _DocId; }
            set { _DocId = value; }
        }
        /// <summary>
        /// 对应到文档库中的DIRID
        /// </summary>
        [DataMember]
        public string DirId
        {
            get { return _DirId; }
            set { _DirId = value; }
        }
    }
    /// <summary>
    /// 附件历史信息
    /// </summary>
    [DataContract]
    public class LibAttachHistory
    {
        private string _FileName;
        private string _Info;
        /// <summary>
        /// 信息
        /// </summary>
        [DataMember]
        public string Info
        {
            get { return _Info; }
            set { _Info = value; }
        }
        /// <summary>
        /// 文件名
        /// </summary>
        [DataMember]
        public string FileName
        {
            get { return _FileName; }
            set { _FileName = value; }
        }
    }
    /// <summary>
    /// 附件列表加载信息
    /// </summary>
    [DataContract]
    public class LoadAttachInfo
    {
        private List<LibAttachInfo> _AttachList;
        private int _MaxOrderId;
        /// <summary>
        /// 最大OrderId
        /// </summary>
        [DataMember]
        public int MaxOrderId
        {
            get { return _MaxOrderId; }
            set { _MaxOrderId = value; }
        }
        /// <summary>
        /// 附件列表
        /// </summary>
        [DataMember]
        public List<LibAttachInfo> AttachList
        {
            get
            {
                if (_AttachList == null)
                    _AttachList = new List<LibAttachInfo>();
                return _AttachList;
            }
        }
    }
    [DataContract]
    public class CompanyParam
    {
        private int _Price = 4;
        private int _Amount = 4;
        private int _TaxRate = 4;
        [DataMember]
        public int TaxRate
        {
            get { return _TaxRate; }
            set { _TaxRate = value; }
        }
        [DataMember]
        public int Amount
        {
            get { return _Amount; }
            set { _Amount = value; }
        }
        [DataMember]
        public int Price
        {
            get { return _Price; }
            set { _Price = value; }
        }
    }

}
