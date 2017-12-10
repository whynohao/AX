/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：App应用类别与其支持的BillAction种类的对应关系配置
 * 创建标识：Zhangkj 2017/06/05
 * 
 *
************************************************************************/
using AxCRL.Comm.Configs;
using AxCRL.Comm.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Bcf.Configs
{
    /// <summary>
    /// App应用类别与其支持的BillAction种类的对应关系配置
    /// </summary>
    public class AppTypeBillActionConfig : ConfigBase
    {
        protected static AppTypeBillActionConfig _Instance = null;
        public static AppTypeBillActionConfig Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = ReadConfig<AppTypeBillActionConfig>();
                    if (_Instance == null)
                    {
                        _Instance = new AppTypeBillActionConfig();
                        _Instance.SaveConfig();
                    }
                }
                return _Instance;
            }
        }
        /// <summary>
        /// App类型和支持的表单操作集合的对象关系字典
        /// 为了保证字典数据不被外部修改，访问属性设置为protected。非公开属性进行Json序列化需要标记JsonProperty
        /// </summary>
        [JsonProperty]
        protected Dictionary<AppType, List<BillAction>> _DicTypeActions = new Dictionary<AppType, List<BillAction>>();
        protected object _lockObj = new object();
        /// <summary>
        /// 查询AppType对应的app支持的单据BillAction种类
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public BillAction[] QueryCan(AppType type)
        {
            lock (_lockObj)
            {
                if (_DicTypeActions.ContainsKey(type))
                    return _DicTypeActions[type].ToArray();
                else
                    return null;
            }
        }
        /// <summary>
        /// 查找支持指定BillAction的AppType类型
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public List<AppType> QueryCan(BillAction action)
        {
            List<AppType> list = new List<AppType>();
            lock (_lockObj)
            {
                foreach(AppType key in _DicTypeActions.Keys)
                {
                    if (_DicTypeActions[key] != null && _DicTypeActions[key].Contains(action))
                        list.Add(key);
                }
            }
            return list;
        }

        private AppTypeBillActionConfig()
        {
            lock (_lockObj)
            {
                _DicTypeActions = new Dictionary<AppType, List<BillAction>>();
                _DicTypeActions[AppType.LeaderMobile] = new List<BillAction>() {
                     BillAction.AddNew,
                     BillAction.ApprovePassRow,
                     BillAction.ApproveUnPassRow,
                     BillAction.AuditPass,
                     BillAction.AuditUnPass,
                     BillAction.Browse,
                     BillAction.CancelApproveRow,
                     BillAction.CancelAudit,
                     BillAction.CancelEndCase,
                     BillAction.CancelInvalid,
                     BillAction.CancelRelease,
                     BillAction.Delete,
                     BillAction.EndCase,
                     BillAction.Invalid,
                     BillAction.Modif,
                     BillAction.Release,
                     BillAction.SaveToDraft,
                     BillAction.SubmitApproveRow,
                     BillAction.SubmitAudit,
                     BillAction.SubmitDraft,
                     BillAction.WithdrawApproveRow,
                     BillAction.WithdrawAudit
                };
                _DicTypeActions[AppType.PDA] = new List<BillAction>() {
                    BillAction.AddNew,
                     BillAction.ApprovePassRow,
                     BillAction.ApproveUnPassRow,
                     BillAction.AuditPass,
                     BillAction.AuditUnPass,
                     BillAction.Browse,
                     BillAction.CancelApproveRow,
                     BillAction.CancelAudit,
                     BillAction.CancelEndCase,
                     BillAction.CancelInvalid,
                     BillAction.CancelRelease,
                     BillAction.Delete,
                     BillAction.EndCase,
                     BillAction.Invalid,
                     BillAction.Modif,
                     BillAction.Release,
                     BillAction.SaveToDraft,
                     BillAction.SubmitApproveRow,
                     BillAction.SubmitAudit,
                     BillAction.SubmitDraft,
                     BillAction.WithdrawApproveRow,
                     BillAction.WithdrawAudit
                };
            }
        }

    }
}
