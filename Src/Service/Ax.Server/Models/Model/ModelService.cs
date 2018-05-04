using AxCRL.Core.Comm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ax.Ui.Models.ModelService
{
    public class ModelService
    {
    }
    /// <summary>
    /// 分页参数
    /// </summary>
    public class PageModel
    {
        /// <summary>
        /// 当前页数
        /// </summary>
        public int PageNo { set; get; }
        /// <summary>
        /// 分页显示数量
        /// </summary>
        public int PageSize { set; get; }
        /// <summary>
        /// 总页数
        /// </summary>
        public long PageCount { set; get; }
        /// <summary>
        /// 总记录数
        /// </summary>
        private long mTotalCount = 0;
        public long TotalCount
        {
            get { return mTotalCount; }
            set
            {
                mTotalCount = value;
                PageCount = mTotalCount > 0 ? (int)Math.Ceiling(mTotalCount / (double)PageSize) : 0;
            }
        }
        private List<object> DataItems = new List<object>();

        public List<long> TimeSection = new List<long>();

        public int SelectCondition { get; set; }

        /// <summary>
        /// 查询条件
        /// </summary>
        public QueryField[] queryField = null;

        public string ProgId { get; set; }
    }

    /// <summary>
    /// 方法返回结果
    /// </summary>
    public class Result
    {
        private bool _returnValue = false;
        private string _message = string.Empty;

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }

        public bool ReturnValue
        {
            get { return _returnValue; }
            set { _returnValue = value; }
        }

        public object Info;

        public PageModel pageModel = new PageModel();
    }
    /// <summary>
    /// 
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// 帐号
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Handle
        /// </summary>
        public string ValidateCode { get; set; }
        public string ClientId { get; set; }

        public string CodeId { get; set; }
        public string Code { get; set; }
    }

    /// <summary>
    /// 审核模型
    /// </summary>
    public class AuditModel
    {
        public string ProgId { get; set; }
        public string BillNo { get; set; }
        public int RowId { get; set; }
        public string UserId { get; set; }
        //public string ValidateCode { get; set; } 
        public bool IsPass { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// 报表模型
    /// </summary>
    public class ModelReport
    {
        public string ProgId { get; set; }
        public QueryField[] queryFieldList { get; set; }
    }

    /// <summary>
    /// 异常追踪模型
    /// </summary>
    public class ExceptionTrack
    {
        /// <summary>
        /// 人员编码
        /// </summary>
        public string PersonId { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public string BillNo { get; set; }
        /// <summary>
        /// 计划完成时间
        /// </summary>
        public DateTime PlanEndTime { get; set; }
        /// <summary>
        /// 解决措施
        /// </summary>
        public string Solution { get; set; }
        /// <summary>
        /// 处理方式
        /// </summary>
        public int DealwithState { get; set; }
    }

    /// <summary>
    /// 图片上传模型
    /// </summary>
    public class pictureUploadModel
    {
        private string _personPicture = string.Empty;

        public string PersonPicture
        {
            get { return _personPicture; }
            set { _personPicture = value; }
        }
        private string _fileExtension = string.Empty;

        public string FileExtension
        {
            get { return _fileExtension; }
            set { _fileExtension = value; }
        }
        private string _prisonId = string.Empty;

        public string PrisonId
        {
            get { return _prisonId; }
            set { _prisonId = value; }
        }
    }

    /// <summary>
    /// 报表查询条件Model
    /// </summary>
    public class QueryField
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 查询类别
        /// </summary>
        public LibQueryChar QueryChar { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public object[] Value { get; set; }
    }

    public class PersonInfo
    {
        private pictureUploadModel _setpicture = new pictureUploadModel();

        public pictureUploadModel Setpicture
        {
            get { return _setpicture; }
            set { _setpicture = value; }
        }
        private string _personPicture = string.Empty;

        public string PersonPicture
        {
            get { return _personPicture; }
            set { _personPicture = value; }
        }
        private string _phoneNo = string.Empty;

        public string PhoneNo
        {
            get { return _phoneNo; }
            set { _phoneNo = value; }
        }
        private string _userId = string.Empty;

        public string UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }
        private string _cornet = string.Empty;

        public string Cornet
        {
            get { return _cornet; }
            set { _cornet = value; }
        }
        private string _personId = string.Empty;

        public string PersonId
        {
            get { return _personId; }
            set { _personId = value; }
        }
        private string _personName = string.Empty;

        public string PersonName
        {
            get { return _personName; }
            set { _personName = value; }
        }
        private string _email = string.Empty;

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }
        private string _password = string.Empty;

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public SetPersonInfoState SetPersonInfoState { get; set; }
    }

    public enum SetPersonInfoState
    {
        PersonPicture = 0,
        phoneNo = 1,
        UserId = 2,
        Cornet = 3,
        PersonName = 4,
        Email = 5,
        Password = 6
    }
    public enum PushType
    {
        Message = 0,
        Approval = 1
    }

    public enum ValidateCodeType
    {
        Login = 0
    }


    public class FeedbackModel
    {
        public int MessageType { get; set; }
        public string Message { get; set; }
    }

    public class CountModel
    {
        public int AbnormalCount { get; set; }
        public int ApprovelCount { get; set; }
    }

    public class PictureCodeResult
    {
        public string PictureCode { get; set; }
        public string CodeID { get; set; }
    }


}