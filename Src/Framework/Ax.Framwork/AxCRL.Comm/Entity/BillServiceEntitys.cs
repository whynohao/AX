/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：BillService中使用的相关实体。如果需要在框架的其他模块公用的，则放到Common项目中，解决循环引用问题
 * 创建标识：Zhangkj 2017/06/29
 * 
************************************************************************/
using AxCRL.Bcf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Entity
{
    /// <summary>
    /// 单点登录和跨系统访问的身份标识信息
    /// </summary>
    [DataContract]
    public class SSOInfo
    {
        /// <summary>
        /// 用户账号
        /// </summary>
        [DataMember]
        public string UserId { get; set; }
        /// <summary>
        /// 登录令牌
        /// </summary>
        [DataMember]
        public string Token { get; set; }
    }
    /// <summary>
    /// 单点登录并打开指定单据
    /// </summary>
    [DataContract]
    public class SSOOpenBillInfo : SSOInfo
    {
        /// <summary>
        /// 单据类型。BillType枚举的int值
        /// </summary>
        [DataMember]
        public int BillType { get; set; }
        /// <summary>
        /// 功能模块标识
        /// </summary>
        [DataMember]
        public string ProgId { get; set; }
        /// <summary>
        /// 主键值列表
        /// </summary>
        [DataMember]
        public object[] CurPks { get; set; }
        /// <summary>
        /// 入口参数
        /// </summary>
        [DataMember]
        public string EntryParam { get; set; }
        /// <summary>
        /// 信息Id。如果是通过系统消息中的链接打开新系统时有效
        /// </summary>
        [DataMember]
        public string InfoId { get; set; }
        /// <summary>
        /// 显示文本
        /// </summary>
        [DataMember]
        public string DisplayText { get; set; }
    }
    [DataContract]
    public class ExecuteBcfMethodResult
    {
        public ExecuteBcfMethodResult() { }
        public ExecuteBcfMethodResult(string message)
        {
            this.Messages.Add(new LibMessage() { Message = message });
        }

        private bool _Success = true;

        [DataMember]
        public bool Success
        {
            get { return _Success; }
            set { _Success = value; }
        }

        private object _Result;
        [DataMember]
        public object Result
        {
            get { return _Result; }
            set { _Result = value; }
        }

        private LibMessageList _Messages;
        [DataMember]
        public LibMessageList Messages
        {
            get
            {
                if (_Messages == null)
                    _Messages = new LibMessageList();
                return _Messages;
            }
            set { _Messages = value; }
        }
    }

    [DataContract]
    public class ExecuteBcfMethodParam
    {
        private string _ProgId;
        private string _MethodName;
        private string _Handle;
        /// <summary>
        /// 是否跨站点调用     
        /// </summary>
        private bool _IsCrossSiteCall;
        /// <summary>
        /// 是否同步数据调用
        /// </summary>
        private bool _IsSynchroDataCall;
        private string _UserId;
        private string _Token;
        /// <summary>
        /// 请求超时时间。单位为毫秒。
        /// 低于100会强制使用100。
        /// 默认为30秒
        /// </summary>
        private int _TimeoutMillSecs = 30 * 1000;
        [DataMember]
        public string Handle
        {
            get { return _Handle; }
            set { _Handle = value; }
        }
        private string[] _MethodParam;
        /// <summary>
        /// 方法参数Json序列化的字符串列表
        /// </summary>
        [DataMember]
        public string[] MethodParam
        {
            get { return _MethodParam; }
            set { _MethodParam = value; }
        }

        [DataMember]
        public string MethodName
        {
            get { return _MethodName; }
            set { _MethodName = value; }
        }

        [DataMember]
        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }
        /// <summary>
        /// 是否跨站点调用。
        /// 如果是跨站点调用，则Handle为单点登录管理站点中的用户Handle
        /// </summary>
        [DataMember]
        public bool IsCrossSiteCall
        {
            get { return _IsCrossSiteCall; }
            set { _IsCrossSiteCall = value; }
        }
        /// <summary>
        /// 是否为同步数据而调用
        /// </summary>
        [DataMember]
        public bool IsSynchroDataCall
        {
            get { return _IsSynchroDataCall; }
            set { _IsSynchroDataCall = value; }
        }
        /// <summary>
        /// 用户账号
        /// </summary>
        [DataMember]
        public string UserId
        {
            get { return _UserId; }
            set { _UserId = value; }
        }
        /// <summary>
        /// 访问令牌
        /// </summary>
        [DataMember]
        public string Token
        {
            get { return _Token; }
            set { _Token = value; }
        }
        /// <summary>
        ///  请求超时时间。单位为毫秒。
        /// 低于100会强制使用100。
        /// 默认为30秒
        /// </summary>
        [DataMember]
        public int TimeoutMillSecs
        {
            get { return _TimeoutMillSecs; }
            set
            {
                if (value < 100)
                    value = 100;
                _TimeoutMillSecs = value;
            }
        }
        private int _MaxCallLevel = 5;
        /// <summary>
        /// 最大调用层级
        /// </summary>
        [DataMember]
        public int MaxCallLevel
        {
            get { return _MaxCallLevel; }
            set
            {
                if (value > 10)
                    value = 10;
                _MaxCallLevel = value;
            }
        }
        private int _CurrentCallLevel = 0;
        /// <summary>
        /// 当前调用层级
        /// </summary>
        public int CurrentCallLevel
        {
            get { return _CurrentCallLevel; }
            set { _CurrentCallLevel = value; }
        }
        /// <summary>
        /// 将方法调用的参数列表转换为Json序列化格式
        /// </summary>
        /// <param name="objParams"></param>
        public static string[] ConvertMethodParams(object[] objParams)
        {
            string[] results = null;
            if (objParams == null || objParams.Length == 0)
                results = null;
            else
            {
                results = new string[objParams.Length];
                for (int index = 0; index < objParams.Length; index++)
                {
                    results[index] = JsonConvert.SerializeObject(objParams[index]);
                }
            }
            return results;
        }
    }
}
