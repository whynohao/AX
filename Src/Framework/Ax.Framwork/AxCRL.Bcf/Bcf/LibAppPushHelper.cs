/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：移动端推送服务调用帮助类
 * 创建标识：Zhangkj 2017/06/06
 * 
 *
************************************************************************/
using AxCRL.Bcf.Configs;
using AxCRL.Comm.Enums;
using AxCRL.Comm.Utils;
using AxCRL.Core.Mail;
using AxCRL.Data;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxSRL.SMS;
using AxSRL.SMS.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Bcf
{
    /// <summary>
    /// 移动端推送服务调用帮助类
    /// </summary>
    public class LibAppPushHelper
    {
        /// <summary>
        /// 推送具体消息
        /// </summary>
        /// <param name="paramObj">object[],第一个对象是BillAction，第二个是LibMailParam的List数组</param>
        public static void Push(object paramObj)
        {
            try
            {
                object[] paramObjs = (object[])paramObj;
                if (paramObjs == null || paramObjs.Length < 2)
                    return;
                BillAction billAction = (BillAction)paramObjs[0];
                List<LibMailParam> list = (List<LibMailParam>)paramObjs[1];
                foreach (var item in list)
                {
                    PushCore(billAction, item);
                }
            }
            catch (Exception)
            {
                // to do log
            }
        }
        /// <summary>
        /// 推送消息到移动端App。
        /// </summary>
        /// <param name="billAction">本次消息对应的单据操作种类。不同的app支持的操作种类不一样。</param>
        /// <param name="paramObj"></param>
        public static void PushCore(BillAction billAction, LibMailParam paramObj)
        {
            LibMailParam param = paramObj as LibMailParam;
            try
            {
                List<PushTarget> listTarget = GetPushTarget(billAction, param.PersonId, param.To, param.CC);
                if (listTarget == null || listTarget.Count == 0)
                    return;
                NoticeMsg msg = new NoticeMsg()
                {
                    Message = param.Content,
                    Title = param.Subject
                };
                PushParams pushParams = new PushParams()
                {
                    Message = msg,
                    Targets = listTarget
                };
                //调用服务接口推送
                LibAppPushService.Push(pushParams);
            }
            catch
            {
                //throw;
            }
        }
        /// <summary>
        /// 根据单据操作类型和发送人列表获取推送目标
        /// </summary>
        /// <param name="billAction">表单操作类型</param>
        /// <param name="send">主要发送到的PERSONID</param>
        /// <param name="to">发送到的PERSONID列表</param>
        /// <param name="cc">抄送的人员PERSONID列表</param>
        /// <returns></returns>
        public static List<PushTarget> GetPushTarget(BillAction billAction, string send, IList<string> to, IList<string> cc)
        {
            //检查是否具有 AXPUSERAPP数据表
            LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel("axp.User");
            bool hasAXPUSERAPP = false;
            if (sqlModel != null && sqlModel.Tables.Count > 1 && sqlModel.Tables[1].TableName.Equals("AXPUSERAPP"))
            {
                hasAXPUSERAPP = true;
            }
            if (hasAXPUSERAPP == false)
            {
                return null;//如果没有需要的相关字段则直接返回
            }
            // 查找支持指定BillAction的AppType类型
            List<AppType> listType = AppTypeBillActionConfig.Instance.QueryCan(billAction);            
            string listTypeStr = string.Empty;
            if (listType == null || listType.Count == 0)
                return null;
            List<int> listTypeInt = new List<int>();
            listType.ForEach(type => {
                listTypeInt.Add((int)type);
            });
            listTypeStr = string.Join(",", listTypeInt);

            List<PushTarget> targetList = new List<PushTarget>();
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrEmpty(send))
                builder.AppendFormat("C.PERSONID={0} OR ", LibStringBuilder.GetQuotString(send));
            if (to != null)
            {
                foreach (string item in to)
                {
                    builder.AppendFormat("C.PERSONID={0} OR ", LibStringBuilder.GetQuotString(item));
                }
            }
            if (cc != null)
            {
                foreach (string item in cc)
                {
                    builder.AppendFormat("C.PERSONID={0} OR ", LibStringBuilder.GetQuotString(item));
                }
            }
           
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 3, 3);
                string sql = string.Format("select distinct A.CLIENTTYPE,A.CLIENTID from AXPUSERAPP A " +
                                           " left join AXPUSER B on A.USERID = B.USERID " +
                                           " left join COMPERSON C on B.PERSONID = C.PERSONID " +
                                           " where ( {0} ) and A.CLIENTTYPE in ({1})", builder.ToString(), listTypeStr);
                LibDataAccess dataAccess = new LibDataAccess();
                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        int appType = LibSysUtils.ToInt32(reader[0]);
                        string clientId = LibSysUtils.ToString(reader[1]);
                        if (string.IsNullOrEmpty(clientId) == false)
                        {
                            targetList.Add(new PushTarget()
                            {
                                AppType = appType,
                                ClientId = clientId
                            });
                        }
                        
                    }
                }
            }            
            return targetList;
        }
    }
}
