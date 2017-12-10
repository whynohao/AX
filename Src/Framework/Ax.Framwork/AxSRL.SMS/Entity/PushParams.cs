/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：调用推送API服务时的推送参数
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
    /// 调用推送API服务时的推送参数
    /// </summary>
    [DataContract]
    public class PushParams
    {
        /// <summary>
        /// 推送目标列表
        /// </summary>
        public List<PushTarget> Targets { get; set; }       
        /// <summary>
        /// 推送的提醒消息
        /// </summary>
        [DataMember]
        public NoticeMsg Message { get; set; }
    }
}