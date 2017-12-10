/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：SystemService中使用的相关实体。如果需要在框架的其他模块公用的，则放到Common项目中，解决循环引用问题
 * 创建标识：Zhangkj 2017/06/29
 * 
************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Entity
{
    [DataContract]
    public class LinkSiteInfo
    {
        [DataMember]
        public string SiteId { get; set; }
        [DataMember]
        public string SiteName { get; set; }
        [DataMember]
        public string ShortName { get; set; }
        [DataMember]
        public string SiteUrl { get; set; }
        /// <summary>
        /// 后端服务地址。如果为空表示与SiteUrl相同
        /// </summary>
        [DataMember]
        public string SvcUrl { get; set; }
        /// <summary>
        /// 是否从站
        /// true表示是当前站点的从站(下级站点)。False则表示是当前站点的上级站点
        /// </summary>
        [DataMember]
        public bool IsSlave { get; set; }
        /// <summary>
        /// 是否向其发送同步数据
        /// 当有需要同步的数据时调用约定的服务向外接站点发送同步请求或同步数据。
        /// </summary>
        [DataMember]
        public bool IsSendTo { get; set; }
    }
}
