using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jikon.MES_Dm.DMCommon
{
    /// <summary>
    /// 目录类型枚举
    /// </summary>
    public enum DirTypeEnum
    {
        /// <summary>
        /// 公共目录
        /// </summary>
        Public = 0,
        /// <summary>
        /// 个人目录
        /// </summary>
        Private = 1,

        /// <summary>
        /// 公共目录的根目录，功能选项类型
        /// </summary>
        PublicRoot = 10,
        /// <summary>
        /// 个人目录的根目录，功能选项类型
        /// </summary>
        PrivateRoot = 11,
        ///我常用的文档，功能选项类型  
        MyNormal = 12,
        /// <summary>
        /// 回收站目录，功能选项类型
        /// </summary>
        Recycle = 13,
    }
    /// <summary>
    /// 文档文件的操作类型
    /// </summary>
    public enum DocOpTypeENum
    {
        /// <summary>
        /// 未知或未设置
        /// </summary>
        UnknownOrUnset = -1,
        /// <summary>
        /// 上传文档
        /// </summary>
        Upload = 0,
        /// <summary>
        /// 新建文档
        /// </summary>
        AddNew = 1,
        /// <summary>
        /// 编辑文档
        /// </summary>
        Edit = 2,
        /// <summary>
        /// 替换
        /// </summary>
        Replace = 3,
        /// <summary>
        /// 通过表单附件上传
        /// </summary>
        UploadBillAttachment = 4,
    }
}
