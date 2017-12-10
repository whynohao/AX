using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AxCRL.Bcf.Sfl;
using Ax.Server.Supply.Model;
using System.Text;
using System.Data;
using AxCRL.Comm.Utils;

namespace Ax.Server.Supply.Bcf
{
    public class getSupplyTraceData : LibWsBcf
    {
        private static getSupplyTraceData _GetTraceData = null;
        private static object lockObj = new object();

        private getSupplyTraceData() { }

        public static getSupplyTraceData DefautDate
        {
            get
            {
                if (_GetTraceData == null)
                {
                    lock (lockObj)
                    {
                        if (_GetTraceData == null)
                        {
                            _GetTraceData = new getSupplyTraceData();
                        }
                    }
                }
                return _GetTraceData;
            }

        }


        public List<purSupplyTrace> GetListTrace(string purBillNo,string materialId,Int64 deliveryTime,string supplyUserId)
        {
            List<purSupplyTrace> traceList = new List<purSupplyTrace>();
            StringBuilder builder = new StringBuilder();
            StringBuilder sqlWhere = new StringBuilder();
            string supplierId = string.Empty;
            supplierId = supplyLogin.DefautDate.getSupplIer(supplyUserId);
            sqlWhere.AppendFormat(" A.SUPPLYUSERID = '{0}' AND A.SUPPLIERID = '{1}' ", supplyUserId, supplierId);

            #region 构建Sql语句
            if (purBillNo != "")
            {
                if (materialId != "")
                {
                    if (deliveryTime != 0)
                    {
                        sqlWhere.AppendFormat(" AND PURBILLNO = '{0}' AND MATERIALID = '{1}' AND DELIVERYTIME = '{2}'",purBillNo,materialId,deliveryTime);
                    }
                    else
                    {
                        sqlWhere.AppendFormat(" AND PURBILLNO = '{0}' AND MATERIALID = '{1}' ", purBillNo, materialId);
                    }
                }
                else
                {
                    if (deliveryTime != 0)
                    {
                        sqlWhere.AppendFormat(" AND PURBILLNO = '{0}'  AND DELIVERYTIME = '{1}'", purBillNo,  deliveryTime);
                    }
                    else
                    {
                        sqlWhere.AppendFormat(" AND PURBILLNO = '{0}' ", purBillNo);
                    }
                }
            }
            else
            {
                if (materialId != "")
                {
                    if (deliveryTime != 0)
                    {
                        sqlWhere.AppendFormat("  AND MATERIALID = '{0}' AND DELIVERYTIME = '{1}'",  materialId, deliveryTime);
                    }
                    else
                    {
                        sqlWhere.AppendFormat("  AND MATERIALID = '{0}' ", materialId);
                    }
                }
                else
                {
                    if (deliveryTime != 0)
                    {
                        sqlWhere.AppendFormat("  AND DELIVERYTIME = '{0}'", deliveryTime);
                    }
                }
            } 
            #endregion

            builder.AppendFormat(@"SELECT   A.PURBILLNO ,
                                            A.PERSONID ,
                                            B.PERSONNAME ,
                                            A.SUPPLIERID ,
                                            C.SUPPLIERNAME ,
                                            A.SUPPLYUSERID ,
                                            D.PERSONNAME AS SUPPLYUSERNAME ,
                                            D.PHONENO ,
                                            A.MATERIALID ,
                                            E.MATERIALNAME ,
                                            A.QUANTITY ,
                                            A.DELIVERYTIME ,
                                            A.CYCLETIME ,
                                            A.PLANSTARTTIME ,
                                            A.REALSTARTTIME ,
                                            A.PLANENDTIME ,
                                            A.REALENDTIME ,
                                            A.PLANINWARETIME ,
                                            A.REALINWARETIME ,
                                            A.PLANSENDTIME ,
                                            A.REALSENDTIME ,
                                            A.STOCKQTY ,
                                            A.INWAYQTY
                                    FROM    PURSUPPLYTRACE A
                                            LEFT JOIN COMPERSON B ON B.PERSONID = A.PERSONID
                                            LEFT JOIN COMPERSON D ON D.PERSONID = A.SUPPLYUSERID
                                            LEFT JOIN COMSUPPLIER C ON C.SUPPLIERID = A.SUPPLIERID
                                            LEFT JOIN COMMATERIAL E ON E.MATERIALID = A.MATERIALID WHERE {0}", sqlWhere);

            using (IDataReader orderReader = this.DataAccess.ExecuteDataReader(builder.ToString()))
            {
                while (orderReader.Read())
                {
                    purSupplyTrace PurSupplyTrace = new purSupplyTrace();
                    //主键
                    PurSupplyTrace.PurBillNo = LibSysUtils.ToString(orderReader["PURBILLNO"]);
                    PurSupplyTrace.PersonId = LibSysUtils.ToString(orderReader["PERSONID"]);
                    PurSupplyTrace.PersonName = LibSysUtils.ToString(orderReader["PERSONNAME"]);
                    PurSupplyTrace.SupplierId = LibSysUtils.ToString(orderReader["SUPPLIERID"]);
                    PurSupplyTrace.SupplierName = LibSysUtils.ToString(orderReader["SUPPLIERNAME"]);
                    PurSupplyTrace.SupplyUserId = LibSysUtils.ToString(orderReader["SUPPLYUSERID"]);
                    PurSupplyTrace.SupplyUserName = LibSysUtils.ToString(orderReader["SUPPLYUSERNAME"]);
                    PurSupplyTrace.SupplyUserTel = LibSysUtils.ToString(orderReader["PHONENO"]);
                    //主键
                    PurSupplyTrace.MaterialId = LibSysUtils.ToString(orderReader["MATERIALID"]);
                    PurSupplyTrace.MaterialName = LibSysUtils.ToString(orderReader["MATERIALNAME"]);
                    PurSupplyTrace.Quantity = LibSysUtils.ToInt32(orderReader["QUANTITY"]);
                    PurSupplyTrace.DeliveryTime = LibSysUtils.ToInt64(orderReader["DELIVERYTIME"]);
                    PurSupplyTrace.CycleTime = LibSysUtils.ToInt32(orderReader["CYCLETIME"]);
                    PurSupplyTrace.PlanStartTime = LibSysUtils.ToInt64(orderReader["PLANSTARTTIME"]);
                    PurSupplyTrace.RealStartTime = LibSysUtils.ToInt64(orderReader["REALSTARTTIME"]);
                    PurSupplyTrace.PlanEndTime = LibSysUtils.ToInt64(orderReader["PLANENDTIME"]);
                    PurSupplyTrace.RealEndTime = LibSysUtils.ToInt64(orderReader["REALENDTIME"]);
                    PurSupplyTrace.PlanInWareTime = LibSysUtils.ToInt64(orderReader["PLANINWARETIME"]);
                    PurSupplyTrace.RealInWareTime = LibSysUtils.ToInt64(orderReader["REALINWARETIME"]);
                    PurSupplyTrace.PlanSendTime = LibSysUtils.ToInt64(orderReader["PLANSENDTIME"]);
                    PurSupplyTrace.RealSendTime = LibSysUtils.ToInt64(orderReader["REALSENDTIME"]);
                    PurSupplyTrace.StockQty = LibSysUtils.ToInt32(orderReader["STOCKQTY"]);
                    PurSupplyTrace.InWareQty = LibSysUtils.ToInt32(orderReader["INWAYQTY"]);
                    traceList.Add(PurSupplyTrace);
                }
            }

            return traceList;
        }
        /// <summary>
        /// 登录获取追踪订单数据
        /// </summary>
        /// <param name="supplyUserId"></param>
        /// <returns></returns>
        public List<purSupplyTrace> GetListTrace(string supplyUserId)
        {
            List<purSupplyTrace> traceList = new List<purSupplyTrace>();
            StringBuilder builder = new StringBuilder();
            string supplierId = string.Empty;
            supplierId = supplyLogin.DefautDate.getSupplIer(supplyUserId);
            builder.AppendFormat(@"SELECT   A.PURBILLNO ,
                                            A.PERSONID ,
                                            B.PERSONNAME ,
                                            A.SUPPLIERID ,
                                            C.SUPPLIERNAME ,
                                            A.SUPPLYUSERID ,
                                            D.PERSONNAME AS SUPPLYUSERNAME ,
                                            D.PHONENO ,
                                            A.MATERIALID ,
                                            E.MATERIALNAME ,
                                            A.QUANTITY ,
                                            A.DELIVERYTIME ,
                                            A.CYCLETIME ,
                                            A.PLANSTARTTIME ,
                                            A.REALSTARTTIME ,
                                            A.PLANENDTIME ,
                                            A.REALENDTIME ,
                                            A.PLANINWARETIME ,
                                            A.REALINWARETIME ,
                                            A.PLANSENDTIME ,
                                            A.REALSENDTIME ,
                                            A.STOCKQTY ,
                                            A.INWAYQTY
                                    FROM    PURSUPPLYTRACE A
                                            LEFT JOIN COMPERSON B ON B.PERSONID = A.PERSONID
                                            LEFT JOIN COMPERSON D ON D.PERSONID = A.SUPPLYUSERID
                                            LEFT JOIN COMSUPPLIER C ON C.SUPPLIERID = A.SUPPLIERID
                                            LEFT JOIN COMMATERIAL E ON E.MATERIALID = A.MATERIALID WHERE A.SUPPLYUSERID = '{0}' AND A.SUPPLIERID = '{1}'", supplyUserId, supplierId);

            using (IDataReader orderReader = this.DataAccess.ExecuteDataReader(builder.ToString()))
            {
                while (orderReader.Read())
                {
                    purSupplyTrace PurSupplyTrace = new purSupplyTrace();
                    //主键
                    PurSupplyTrace.PurBillNo = LibSysUtils.ToString(orderReader["PURBILLNO"]);
                    PurSupplyTrace.PersonId = LibSysUtils.ToString(orderReader["PERSONID"]);
                    PurSupplyTrace.PersonName = LibSysUtils.ToString(orderReader["PERSONNAME"]);
                    PurSupplyTrace.SupplierId = LibSysUtils.ToString(orderReader["SUPPLIERID"]);
                    PurSupplyTrace.SupplierName = LibSysUtils.ToString(orderReader["SUPPLIERNAME"]);
                    PurSupplyTrace.SupplyUserId = LibSysUtils.ToString(orderReader["SUPPLYUSERID"]);
                    PurSupplyTrace.SupplyUserName = LibSysUtils.ToString(orderReader["SUPPLYUSERNAME"]);
                    PurSupplyTrace.SupplyUserTel = LibSysUtils.ToString(orderReader["PHONENO"]);
                    //主键
                    PurSupplyTrace.MaterialId = LibSysUtils.ToString(orderReader["MATERIALID"]);
                    PurSupplyTrace.MaterialName = LibSysUtils.ToString(orderReader["MATERIALNAME"]);
                    PurSupplyTrace.Quantity = LibSysUtils.ToInt32(orderReader["QUANTITY"]);
                    PurSupplyTrace.DeliveryTime = LibSysUtils.ToInt64(orderReader["DELIVERYTIME"]);
                    PurSupplyTrace.CycleTime = LibSysUtils.ToInt32(orderReader["CYCLETIME"]);
                    PurSupplyTrace.PlanStartTime = LibSysUtils.ToInt64(orderReader["PLANSTARTTIME"]);
                    PurSupplyTrace.RealStartTime = LibSysUtils.ToInt64(orderReader["REALSTARTTIME"]);
                    PurSupplyTrace.PlanEndTime = LibSysUtils.ToInt64(orderReader["PLANENDTIME"]);
                    PurSupplyTrace.RealEndTime = LibSysUtils.ToInt64(orderReader["REALENDTIME"]);
                    PurSupplyTrace.PlanInWareTime = LibSysUtils.ToInt64(orderReader["PLANINWARETIME"]);
                    PurSupplyTrace.RealInWareTime = LibSysUtils.ToInt64(orderReader["REALINWARETIME"]);
                    PurSupplyTrace.PlanSendTime = LibSysUtils.ToInt64(orderReader["PLANSENDTIME"]);
                    PurSupplyTrace.RealSendTime = LibSysUtils.ToInt64(orderReader["REALSENDTIME"]);
                    PurSupplyTrace.StockQty = LibSysUtils.ToInt32(orderReader["STOCKQTY"]);
                    PurSupplyTrace.InWareQty = LibSysUtils.ToInt32(orderReader["INWAYQTY"]);
                    traceList.Add(PurSupplyTrace);
                }
            }

            return traceList;
        }

        public bool savePurTraceData(purSupplyTrace PurSupplyTrace)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(@"UPDATE PURSUPPLYTRACE 
                                    SET 
                                    CYCLETIME = {0},
                                    PLANSTARTTIME = {1},
                                    PLANENDTIME = {2},
                                    PLANINWARETIME = {3},
                                    PLANSENDTIME = {4},
                                    STOCKQTY = {5},
                                    REALSTARTTIME = {6},
                                    REALENDTIME = {7},
                                    REALINWARETIME = {8},
                                    REALSENDTIME = {9},
                                    INWAYQTY={10} 
                                    WHERE 
                                    PURBILLNO = '{11}' 
                                    AND 
                                    MATERIALID = '{12}' "
                                    , PurSupplyTrace.CycleTime,
                                    PurSupplyTrace.PlanStartTime,
                                    PurSupplyTrace.PlanEndTime,
                                    PurSupplyTrace.PlanInWareTime,
                                    PurSupplyTrace.PlanSendTime,
                                    PurSupplyTrace.StockQty,
                                    PurSupplyTrace.RealStartTime,
                                    PurSupplyTrace.RealEndTime,
                                    PurSupplyTrace.RealInWareTime,
                                    PurSupplyTrace.RealSendTime,
                                    PurSupplyTrace.InWareQty,
                                    PurSupplyTrace.PurBillNo,
                                    PurSupplyTrace.MaterialId
                                    );
            int BackRow = LibSysUtils.ToInt32(this.DataAccess.ExecuteNonQuery(builder.ToString()));
            if (BackRow > 0)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
    }
}