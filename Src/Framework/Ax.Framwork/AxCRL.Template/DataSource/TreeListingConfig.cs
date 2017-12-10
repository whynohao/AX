/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：清单页树形分类的配置
 * 创建标识：Zhangkj 2017/06/21
 * 
************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template.DataSource
{
    /// <summary>
    /// 清单页树形分类的配置
    /// </summary>
    public class TreeListingConfig
    {
        /// <summary>
        /// 作为分类树层级分类的主表列名。例如人员表中的DEPTID
        /// 启用分类层级的节点必须是有关联的RelativeSource。例如人员的DEPTID关联到部门
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 关联到的对象的主键列名。例如人员表部门的关联对象com.Dept对应的主键为DEPTID
        /// </summary>
        public string RelativeIdColumn { get; set; }
        /// <summary>
        /// 关联到的对象的名称列。例如人员表部门的关联对象com.Dept对应的主键为DEPTNAME
        /// </summary>
        public string RelativeNameColumn { get; set; }

        /// <summary>
        /// 关联到的对象的表示父对象引用的数据列的列名。
        /// 例如人员表部门的关联对象com.Dept，其中表示父节点的是上级部门标识SUPERDEPTID
        /// </summary>
        public string RelativeParentColumn { get; set; }

        /// <summary>
        /// 分类树节点上每个同级节点下子节点的排序字段名称及排序条件。
        /// 例如人员关联的com.Dept，同级部门下按部门级别排序，则设置为DEPTLEVEL asc等
        /// 如果为空且数据表有创建时间列，则按创建时间升序排列
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// 树节点上显示Id信息
        /// </summary>
        public bool NodeShowId { get; set; }
        /// <summary>
        /// 树节点上显示名称信息
        /// </summary>
        public bool NodeShowName { get; set; }
        /// <summary>
        /// 树节点上显示连接符
        /// </summary>
        public bool NodeShowJoinChar { get; set; }
        /// <summary>
        /// 树节点上显示的Id和Name的连接符
        /// </summary>
        public string NodeJoinChar { get; set; }

        public TreeListingConfig()
        {
            NodeShowName = true;
            NodeJoinChar = ",";
        }
    }
}
