using Ax.Server.Models.ModelService;
using AxCRL.Bcf;
using AxCRL.Comm.Bill;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Data;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace Ax.Server.Models.Bcf
{
    public class Message
    {
        /// <summary>
        /// 获取异常消息
        /// </summary>
        /// <param name="userId">帐号</param>
        /// <param name="handle">句柄</param>
        /// <param name="info">分页参数</param>
        /// <returns></returns>
        public static Result GetAbnormalReport(string userId, string handle, PageModel info)
        {
            Result res = new Result();
            res.ReturnValue = true;
            LibHandle Handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
            Service.VerificationHandle(userId, handle, Handle, res);
            if (res.ReturnValue)
            {
                LibDataAccess access = new LibDataAccess();
                try
                {
                    string sql = string.Empty;
                    if (!string.IsNullOrEmpty(Handle.PersonId))
                    {
                        StringBuilder builder = new StringBuilder();
                        if (info.queryField != null)
                        {
                            switch (info.queryField[0].QueryChar)
                            {
                                case LibQueryChar.Equal:
                                    builder.AppendFormat("AND {0}{1}{2}", info.queryField[0].Name, "=", info.queryField[0].Value[0]);
                                    break;
                                case LibQueryChar.Region:
                                    builder.AppendFormat("AND {0}{1}{2} AND {3}{4}{5}", info.queryField[0].Name, ">=", info.queryField[0].Value[0], info.queryField[0].Name, "<=", info.queryField[0].Value[1]);
                                    break;
                                case LibQueryChar.GreaterOrEqual:
                                    builder.AppendFormat("AND {0}{1}{2}", info.queryField[0].Name, ">=", info.queryField[0].Value[0]);
                                    break;
                                case LibQueryChar.LessOrEqual:
                                    builder.AppendFormat("AND {0}{1}{2}", info.queryField[0].Name, "<=", info.queryField[0].Value[0]);
                                    break;
                                case LibQueryChar.GreaterThan:
                                    builder.AppendFormat("AND {0}{1}{2}", info.queryField[0].Name, ">", info.queryField[0].Value[0]);
                                    break;
                                case LibQueryChar.LessThan:
                                    builder.AppendFormat("AND {0}{1}{2}", info.queryField[0].Name, "<", info.queryField[0].Value[0]);
                                    break;
                                case LibQueryChar.UnequalTo:
                                    builder.AppendFormat("AND {0}{1}{2}", info.queryField[0].Name, "<>", info.queryField[0].Value[0]);
                                    break;
                            }
                        }
                        //StringBuilder buildString = new StringBuilder();
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        int beginNum = 0, endNum = 0;
                        sql = string.Format("SELECT COUNT(*) FROM COMABNORMALREPORT A LEFT JOIN COMABNORMALREPORTTYPEFLOW B ON B.TYPEID = A.TYPEID  WHERE A.TRANSMITLEVEL = B.TRANSMITLEVEL AND B.PERSONID = '{0}' AND (SELECT COUNT(BILLNO) FROM COMABNORMALTRACE C WHERE C.FROMBILLNO=A.BILLNO )={1} {2}", Handle.PersonId, LibSysUtils.ToInt32(info.SelectCondition), builder);
                        int totalCount = LibSysUtils.ToInt32(access.ExecuteScalar(sql));
                        int pageCount = 0;
                        if (totalCount / info.PageSize == 0)
                            pageCount = totalCount / info.PageSize;
                        else
                            pageCount = totalCount / info.PageSize + 1;
                        endNum = info.PageNo * info.PageSize;
                        beginNum = (info.PageNo - 1) * info.PageSize + 1;
                        DatabaseProviderFactory factory = new DatabaseProviderFactory(ConfigurationSourceFactory.Create());
                        Database dataBase = factory.Create("DefaultConnection");
                        if (dataBase.GetType().Name == "OracleDatabase")
                        {
                            sql = string.Empty;
                            sql = string.Format(@" SELECT DISTINCT  A.BILLNO,
                                                              A.BILLDATE,
                                                              D.ABNORMALTYPENAME,
                                                              A.FROMPERSONID,
                                                              C.PERSONNAME AS FROMPERSONNAME,
                                                              C.PHONENO AS FROMPHONENO,
                                                              A.ABNORMALDESC
                                                              FROM 
                                                              (
                                                              SELECT E.*,ROWNUM RN 
                                                              FROM (SELECT * FROM COMABNORMALREPORT F LEFT JOIN COMABNORMALREPORTTYPEFLOW B ON B.TYPEID = F.TYPEID  WHERE B.PERSONID={2} AND F.TRANSMITLEVEL = B.TRANSMITLEVEL AND (SELECT COUNT(BILLNO) FROM COMABNORMALTRACE A WHERE A.FROMBILLNO=F.BILLNO )={3}) E 
                                                              WHERE ROWNUM <= {1} ) A 
                                                             LEFT JOIN COMPERSON C ON C.PERSONID=A.FROMPERSONID 
                                                             LEFT JOIN COMABNORMALTYPE D ON D.ABNORMALTYPEID=A.ABNORMALTYPEID  
                                                             WHERE  RN>={0} {4}", beginNum, endNum, Handle.PersonId, LibSysUtils.ToInt32(info.SelectCondition), builder);
                        }
                        else
                        {
                            sql = string.Empty;
                            sql = string.Format("");
                        }
                        res.Info = access.ExecuteDataSet(sql);
                        //res.Info = access.ExecuteStoredProcedureReturnDataSet("GETABNORMALREPORT", ref dic, beginNum, endNum, Handle.PersonId, LibSysUtils.ToInt32(info.SelectCondition), builder);
                        res.pageModel.PageNo = info.PageNo;
                        res.pageModel.PageSize = info.PageSize;
                        res.pageModel.PageCount = pageCount;
                        res.pageModel.TotalCount = totalCount;
                        res.ReturnValue = true;
                    }
                    else
                    {
                        res.ReturnValue = false;
                        res.Message = "请重新登录！";
                    }


                }
                catch (Exception ex)
                {
                    res.ReturnValue = false;
                    res.Message = "查询失败！" + ex.Message;
                }
            }
            return res;
        }
        public static Result ExceptionTrack(string personId, string BillNo, DateTime PlanEndTime, string Solution, int DealwithState, string userId)
        {
            Result res = new Result();
            LibBcfData bcfData = (LibBcfData)LibBcfSystem.Default.GetBcfInstance("com.AbnormalTrace");
            LibEntryParam param = new LibEntryParam();
            DataSet dataSet = bcfData.AddNew(param);
            DataTable masterDt = dataSet.Tables[0];
            masterDt.BeginLoadData();
            DataRow masterRow = masterDt.Rows[0];
            try
            {
                masterRow.BeginEdit();
                try
                {
                    masterRow["TYPEID"] = "P001";
                    masterRow["FROMBILLNO"] = BillNo;
                    masterRow["PLANENDTIME"] = LibDateUtils.DateTimeToLibDateTime(PlanEndTime);
                    masterRow["SOLUTION"] = Solution;
                    masterRow["DEALWITHSTATE"] = DealwithState;
                    masterRow["DEALWITHPERSONID"] = personId;
                }
                catch
                {
                    throw;
                }
                finally
                {
                    masterRow.EndEdit();
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                masterDt.EndLoadData();
            }
            bcfData.InnerSave(BillAction.AddNew, null, dataSet);
            if (bcfData.ManagerMessage.IsThrow)
            {
                res.Message = "数据有误";
                res.ReturnValue = false;
            }
            else
            {
                res.ReturnValue = true;
                //Service.PushMessage(userId, PushType.Message);
            }
            return res;
        }

        public static Result ExceptionTrackBillNo(string BillNo, string userId, string handle)
        {
            Result res = new Result();
            res.ReturnValue = true;
            LibHandle Handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
            Service.VerificationHandle(userId, handle, Handle, res);
            if (res.ReturnValue)
            {
                LibDataAccess access = new LibDataAccess();
                try
                {
                    string sql = string.Empty;
                    if (!string.IsNullOrEmpty(Handle.PersonId))
                    {
                        sql = string.Format(@"SELECT DISTINCT A.BILLNO,A.BILLDATE,D.ABNORMALTYPENAME, A.FROMPERSONID,C.PERSONNAME AS FROMPERSONNAME,C.PHONENO AS FROMPHONENO,A.ABNORMALDESC FROM COMABNORMALREPORT A 
                    LEFT JOIN COMABNORMALREPORTTYPEFLOW B ON B.TYPEID = A.TYPEID 
                    LEFT JOIN COMPERSON C ON C.PERSONID=A.FROMPERSONID 
                    LEFT JOIN COMABNORMALTYPE D ON D.ABNORMALTYPEID=A.ABNORMALTYPEID  
                    WHERE A.TRANSMITLEVEL = B.TRANSMITLEVEL AND B.PERSONID = {0} AND A.BILLNO ={1}", LibStringBuilder.GetQuotString(Handle.PersonId), LibStringBuilder.GetQuotString(BillNo));
                        res.Info = access.ExecuteDataSet(sql);
                        res.ReturnValue = true;
                        res.Message = "成功！";
                    }
                    else
                    {
                        res.ReturnValue = false;
                        res.Message = "请重新登录！";
                    }


                }
                catch (Exception ex)
                {
                    res.ReturnValue = false;
                    res.Message = "查询失败！" + ex.Message;
                }
            }
            return res;
        }

        public static Result ExceptionBill(string BillNo, string userId, string handle)
        {
            Result res = new Result();
            res.ReturnValue = true;
            LibHandle Handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
            Service.VerificationHandle(userId, handle, Handle, res);
            if (res.ReturnValue)
            {
                LibDataAccess access = new LibDataAccess();
                try
                {
                    string sql = string.Empty;
                    if (!string.IsNullOrEmpty(Handle.PersonId))
                    {
                        sql = string.Format(@"SELECT A.BILLNO,
                                                A.PLANENDTIME,
                                                A.ABNORMALREASONID,
                                                B.ABNORMALREASONNAME,
                                                B.ABNORMALREASONTYPEID,
                                                C.ABNORMALREASONTYPENAME,
                                                A.PERSONID,
                                                D.PERSONNAME,
                                                A.DEALWITHPERSONID,
                                                E.PERSONNAME AS DEALWITHPERSONNAME,
                                                E.DEPTID AS DEALWITHDEPTID,
                                                F.DEPTNAME AS DEALWITHDEPTNAME,
                                                A.SOLUTION,
                                                A.DEALWITHSTATE
                                                FROM COMABNORMALTRACE A
                                                LEFT JOIN COMABNORMALREASON B ON B.ABNORMALREASONID=A.ABNORMALREASONID
                                                LEFT JOIN COMABNORMALREASONTYPE C ON C.ABNORMALREASONTYPEID=B.ABNORMALREASONTYPEID
                                                LEFT JOIN COMPERSON D ON D.PERSONID=A.PERSONID
                                                LEFT JOIN COMPERSON E ON E.PERSONID=A.DEALWITHPERSONID
                                                LEFT JOIN COMDEPT F ON F.DEPTID =E.DEPTID
                                                WHERE A.FROMBILLNO={0}", BillNo);
                        res.Info = access.ExecuteDataSet(sql);
                        res.ReturnValue = true;
                        res.Message = "成功！";
                    }
                    else
                    {
                        res.ReturnValue = false;
                        res.Message = "请重新登录！";
                    }


                }
                catch (Exception ex)
                {
                    res.ReturnValue = false;
                    res.Message = "查询失败！" + ex.Message;
                }
            }
            return res;
        }

        public static Result GetSelectList(string userId, string handle, PageModel info)
        {
            Result result = new Result();
            result.ReturnValue = true;
            LibHandle Handle = LibHandleCache.Default.IsExistsHandle(LibHandeleType.PC, userId);
            Service.VerificationHandle(userId, handle, Handle, result);
            if (result.ReturnValue)
            {
            }
            return result;
        }
    }
}