using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxCRL.Services;
using AxCRL.Data.SqlBuilder;
using AxCRL.Data;
using System.Data;
using AxCRL.Comm.Utils;
using Ax.Server.Supply.Model;

namespace Ax.Server.Supply.Bcf
{
    public class supplyLogin
    {
        private static supplyLogin _GetLogin = null;
        private static object lockObj = new object();

        private supplyLogin() { }

        public static supplyLogin DefautDate
        {
            get
            {
                if (_GetLogin == null)
                {
                    lock (lockObj)
                    {
                        if (_GetLogin == null)
                        {
                            _GetLogin = new supplyLogin();
                        }
                    }
                }
                return _GetLogin;
            }

        }

        public supplyLoginModel login(string userId, string password)
        {
            supplyLoginModel supplyLoginModel = new supplyLoginModel();
            SqlBuilder builder = new SqlBuilder("axp.User");
            string sql = builder.GetQuerySql(0, "A.PERSONID,A.PERSONNAME", string.Format("A.USERID={0} And A.USERPASSWORD={1} And A.ISUSE=1", LibStringBuilder.GetQuotString(userId), LibStringBuilder.GetQuotString(password)));
            LibDataAccess dataAccess = new LibDataAccess();
            string roleId = string.Empty;
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                if (reader.Read())
                {
                    supplyLoginModel.PersonId = LibSysUtils.ToString(reader[0]);
                    supplyLoginModel.LoginSuccess = true;
                }
            }
            return supplyLoginModel;
        }
        public string getSupplIer(string supplyUserId)
        {
            string supplierId = string.Empty;
             LibDataAccess dataAccess = new LibDataAccess();
            string sql = string.Format("SELECT SUPPLIERID FROM COMPERSON WHERE PERSONID = '{0}'",supplyUserId);
            supplierId = LibSysUtils.ToString(dataAccess.ExecuteScalar(sql));
            return supplierId;
        }
    }
}