/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的功能权限枚举，参照AX原FuncPermissionEnum
 * 创建标识：Zhangkj 2016/12/14
 * 
************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jikon.MES_Dm.DMCommon
{
    /// <summary>
    /// 文档管理模块的功能权限项枚举
    /// </summary>
    public enum DMFuncPermissionEnum
    {
        /// <summary>
        /// 功能模块的使用权限，仅为了与原AX保持一致
        /// </summary>
        Use = 1,
        /// <summary>
        /// 浏览权限。对文档来说仅能浏览文档信息，文档的内容还需要具有Read权限
        /// </summary>
        Browse = 2,
        Add = 4,
        Edit = 8,
        Delete = 16,
        Release = 32,
        CancelRelease = 64,
        Audit = 128,
        CancelAudit = 256,
        EndCase = 512,
        CancelEndCase = 1024,
        Invalid = 2048,
        CancelInvalid = 4096,
        /// <summary>
        /// 为目录批量导入文档
        /// </summary>
        Import = 8192,
        /// <summary>
        /// 导出目录下的有下载权限的文档
        /// </summary>
        Export = 16384,
        Print = 32768,
        #region 以下项为针对文档管理新增的权限项  Zhangkj20161214
        /// <summary>
        /// 阅读权限
        /// </summary>
        Read = 0x10000,     
        /// <summary>
        /// 上传权限
        /// </summary>
        Upload = 0x20000,     
        /// <summary>
        /// 下载权限
        /// </summary>
        Download = 0x40000,     
        /// <summary>
        /// 移动权限，如将文件从一个目录移动到另外一个目录
        /// </summary>
        Move = 0x80000,     
        /// <summary>
        /// 设定版本权限，如设定文件的版本号
        /// </summary>
        SetVersion = 0x100000,    
        /// <summary>
        /// 订阅权限
        /// </summary>
        Subscribe = 0x200000,    
        /// <summary>
        /// 借出权限
        /// </summary>
        Lend = 0x400000,    
        /// <summary>
        /// 链接（发送链接）权限
        /// </summary>
        Link = 0x800000,    
        /// <summary>
        /// 关联（发送链接）权限
        /// </summary>
        Associate = 0x1000000,    
        /// <summary>
        /// 评论 权限
        /// </summary>
        Comment = 0x2000000,    
        /// <summary>
        /// 重命名 权限，如重命名文件的名字
        /// </summary>
        Rename = 0x4000000,    
        /// <summary>
        /// 替换 权限，如使用新文件替换旧的文件
        /// </summary>
        Replace = 0x8000000,    
        /// <summary>
        /// 管理 权限，如对一个具体对象（如文件目录）的所有权限（子目录的增删改、子目录下的文件所有权限等等、目录和文件的权限设置等）
        /// 如果权限项中没有对应的功能权限，而又需要做控制时可使用管理权限来检查
        /// </summary>
        Manage = 0x10000000,   
        /// <summary>
        /// 回退权限。可对文档的修订版进行回退，使之成为最新版
        /// </summary>
        Fallback= 0x20000000, 
        #endregion
    }

}
