/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：一般简单的枚举
 * 创建标识：Zhangkj 2017/06/13
 * 
 *
************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Enums
{
    /// <summary>
    /// 消息通知渠道类型
    /// </summary>
    public enum NoticeChanelType
    {
        /// <summary>
        /// 所有类型
        /// </summary>
        [Description("所有")]
        All = 0,
        /// <summary>
        ///  系统内置消息
        /// </summary>
        [Description("内置消息")]
        SysNews,       
        [Description("邮件")]
        Mail,       
        [Description("短信")]
        SMS,
        [Description("微信")]       
        Weixin,
        [Description("移动端推送")]
        AppPush
    }
    public enum LibSyncDataOpType
    {
        [Description("新增")]
        AddNew = 0,
        [Description("修改")]
        Modify = 1,
        [Description("删除")]
        Delete = 2,
    }
    /// <summary>
    /// 同步数据的状态
    /// </summary>
    public enum LibSyncDataState
    {
        [Description("未同步")]
        NoneSync = 0,       
        [Description("同步异常")]
        SyncError = 1,
        [Description("已同步")]
        Synced = 2,
    }
}
