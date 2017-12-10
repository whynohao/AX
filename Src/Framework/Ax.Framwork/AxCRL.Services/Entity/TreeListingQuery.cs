using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Services.Entity
{
    /// <summary>
    /// 清单页树形分类查询参数
    /// </summary>
    [DataContract]
    public class TreeListingQuery
    {
        /// <summary>
        /// 用户会话标识
        /// </summary>
        [DataMember]
        public string Handle { get; set; }
        /// <summary>
        /// 功能标识
        /// 例如获取人员清单页的树形分类时，此参数为com.Person
        /// </summary>
        [DataMember]
        public string ProgId { get; set; }
        /// <summary>
        /// 树节点上对应的键值。将依据此属性查询下级节点。
        /// 为空表示查询一级节点
        /// </summary>
        [DataMember]
        public object NodeId { get; set; }
    }
    /// <summary>
    /// 清单页树形分类节点
    /// </summary>
    [DataContract]
    public class TreeListingNode
    {
        /// <summary>
        /// 树节点对应的键值
        /// </summary>
        [DataMember]
        public string Id { get; set; }
        /// <summary>
        /// 树节点的显示名称
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }
    }
}
