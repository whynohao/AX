/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：调用推送API服务的推送结果
 * 创建标识：Zhangkj 2017/06/06
 * 
 *
************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AxSRL.SMS.Entity
{
    /// <summary>
    /// 调用推送API服务的推送结果
    /// </summary>
    [DataContract]
    public class PushResult
    {
        /// <summary>
        /// 调用推送接口是否有错误
        /// </summary>
        [DataMember]
        public bool IsCallPushError { get; set; }
        /// <summary>
        /// 推送结果消息
        /// </summary>
        [DataMember]
        public string ResultMessage { get; set; }
    }
}