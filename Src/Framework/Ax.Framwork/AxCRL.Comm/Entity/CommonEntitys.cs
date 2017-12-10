/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：通用实体
 * 创建标识：Zhangkj 2017/07/03
 * 
************************************************************************/
using AxCRL.Comm.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Entity
{
    /// <summary>
    /// 同步数据的信息
    /// </summary>
    public class SyncDataInfo
    {
        public string InfoId { get; set; }
        public string ProgId { get; set; }
        public string ProgName { get; set; }
        public string InternalId { get; set; }
        public string BillNo { get; set; }
        public string UserId { get; set; }
        public string PersonName { get; set; }
        public bool IsSyncTo { get; set; }
        public string SiteId { get; set; }
        public string ShortName { get; set; }
        public DateTime SyncTime { get; set; }
        public LibSyncDataOpType SyncOp { get; set; }
        public LibSyncDataState SyncState { get; set; }
        public string SyncInfo { get; set; }
    }
}
