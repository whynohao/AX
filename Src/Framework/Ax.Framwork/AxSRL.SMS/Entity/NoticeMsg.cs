/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：推送时的提醒消息
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
    /// 推送时的提醒消息
    /// </summary>
    [DataContract]
    public class NoticeMsg
    {
        /// <summary>
        /// 要发送的消息标题
        /// </summary>
        [DataMember]
        public string Title { get; set; }
        /// <summary>
        /// 要发送的消息内容
        /// </summary>
        [DataMember]
        public string Message { get; set; }
        public NoticeMsg()
        {
            Title = "消息";
            Message = "";
        }
    }
}