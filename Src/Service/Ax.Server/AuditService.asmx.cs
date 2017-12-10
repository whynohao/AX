using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using AxCRL.Bcf;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Services;
using System.Data;
using AxCRL.Comm.Utils;
using AxCRL.Data;

namespace Ax.Server
{
    /// <summary>
    /// AuditService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class AuditService : System.Web.Services.WebService
    {

        //审核
        [WebMethod]
        public string Audit(string progId, string billNo, int rowId, string userId, string password, bool isPass)
        {


            //模拟用户登录
            SystemService server = new SystemService();
            LoginInfo loginInfo = server.Login(userId, password, false);
            if (loginInfo.PersonId == null)
            {
                return "登录失败";
            }
            //根据progid构建bcf
            LibBcfData bcf = (LibBcfData)LibBcfSystem.Default.GetBcfInstance(progId);
            bcf.Handle = LibHandleCache.Default.GetHandle(string.Empty, LibHandeleType.None, userId, loginInfo.PersonId, loginInfo.PersonName, "");

            //根据rowid判断是行审核还是单据审核
            if (rowId>0)
            {
                Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> dic =
                    new Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>>();
                dic.Add(rowId,new SortedList<int, List<LibApproveFlowInfo>>( ));
                bcf.AuditRow(new object[] { billNo }, isPass, dic, new Dictionary<int, int>());
            }
            else
            {
                bcf.Audit(new object[] { billNo }, isPass, new Dictionary<string, LibChangeRecord>(), -1, null);
 
            }

            StringBuilder sb = new StringBuilder();

            //根据messagelist判断是否操作成功
            if (bcf.ManagerMessage.IsThrow)
            {
                foreach (LibMessage item in bcf.ManagerMessage.MessageList)
                {
                    sb.Append(item.Message);
                }
                return sb.ToString();
            }
            else
            {

                return "审核成功";
            }


        }

        //送审
        [WebMethod]
        public string SubmitAudit(string billNo, string progId)
        {
            LibBcfData bcf = (LibBcfData)LibBcfSystem.Default.GetBcfInstance(progId);
            bcf.SubmitAudit(new object[] { billNo }, false, new Dictionary<string, LibChangeRecord>(), null);
            StringBuilder sb = new StringBuilder();
            if (bcf.ManagerMessage.IsThrow)
            {
                foreach (LibMessage item in bcf.ManagerMessage.MessageList)
                {
                    sb.Append(item.Message);
                }
                return sb.ToString();
            }
            else
            {

                return "送审成功";
            }
        }

        //获取审核消息
        [WebMethod]

        public DataSet GetMyNews(string userId, string password)
        {
            SystemService server = new SystemService();
            LibDataAccess access = new LibDataAccess();
            string sql = string.Format("SELECT count(*) FROM AXPUSER  WHERE USERID ='{0}' AND USERPASSWORD ='{1}'", userId, password);
            int count = LibSysUtils.ToInt32(access.ExecuteScalar(sql));
            if (count > 0)
            {
                sql = string.Format("SELECT  NEWSID ,USERID ,TITLE ,MAINCONTENT ,INFOID ,A.CREATETIME ,PERSONID ,ISREAD ,EXECTASKDATAID ,B.CREATETIME ,PROGID ,RESULTDATA FROM AXPUSERNEWS A LEFT JOIN AXAEXECTASKDATA B ON A.INFOID = B.EXECTASKDATAID WHERE A.USERID = {0} AND A.ISREAD = 0", LibStringBuilder.GetQuotString(userId));

                return access.ExecuteDataSet(sql);
            }
            else
            {
                return new DataSet();
            }

        }

    }
}
