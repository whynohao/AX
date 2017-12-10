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
    public class getRollPlanData : LibWsBcf
    {
        private static getRollPlanData _GetRollPlanData = null;
        private static object lockObj = new object();

        private getRollPlanData() { }

        public static getRollPlanData DefautDate
        {
            get
            {
                if (_GetRollPlanData == null)
                {
                    lock (lockObj)
                    {
                        if (_GetRollPlanData == null)
                        {
                            _GetRollPlanData = new getRollPlanData();
                        }
                    }
                }
                return _GetRollPlanData;
            }

        }

        /// <summary>
        /// 获取滚动计划查询结果
        /// </summary>
        /// <param name="supplyUserId"></param>
        /// <param name="materialId"></param>
        /// <param name="personId"></param>
        /// <param name="purchaseOrder"></param>
        /// <param name="planDate"></param>
        /// <returns>list</returns>
        public List<purRollPlan> GetListRollPlan(string materialId, string purchaseOrder, string supplyUserId)
        {
            List<purRollPlan> rollplanlist = new List<purRollPlan>();
            StringBuilder builder = new StringBuilder();
            string supplierId = string.Empty;
            supplierId = supplyLogin.DefautDate.getSupplIer(supplyUserId);
            StringBuilder sqlWhere = new StringBuilder();
             sqlWhere.AppendFormat(" AND B.SUPPLYUSERID = '{0}' AND B.SUPPLIERID = '{1}' ", supplyUserId, supplierId);

            #region 构建SQL条件
            if (materialId != "")
            {
                if (purchaseOrder != "")
                {
                    sqlWhere.AppendFormat(" AND MATERIALID = '{0}' AND PURCHASEORDER = '{1}'", materialId, purchaseOrder);
                }
                else
                {
                    sqlWhere.AppendFormat(" AND MATERIALID = '{0}'", materialId);
                }
            }
            else
            {
                if (purchaseOrder != "")
                {
                    sqlWhere.AppendFormat(" AND PURCHASEORDER = '{0}'", purchaseOrder);

                }
            }
            #endregion

            builder.AppendFormat(@"SELECT  B.BILLNO,
                                    B.ROW_ID,
                                    B.PURCHASEORDER ,
                                    B.PERSONID ,
                                    D.PERSONNAME ,
                                    B.SUPPLIERID,
                                    F.SUPPLIERNAME,
                                    B.SUPPLYUSERID ,
                                    G.PERSONNAME AS SUPPLYUSERNAME ,
                                    G.PHONENO,
                                    B.MATERIALID,
                                    E.MATERIALNAME,
                                    A.PLANDATE,
                                    A.DELIVERYNOTENO,
                                    A.BARCODE,
                                    A.ARRIVEDATE,
                                    A.ARRIVEQUANTITY,
                                    B.WORKNO
                            FROM    dbo.PURSUPPLYROLLPLANPOST A
                                    LEFT JOIN dbo.PURPURCHASEPLANDETAIL B ON A.FROMBILLNO = B.BILLNO
                                                                             AND A.FROMROWID = B.ROW_ID
                                    LEFT JOIN dbo.PURPURCHASEPLAN C ON B.BILLNO = C.BILLNO
                                    LEFT JOIN dbo.COMPERSON D ON B.PERSONID = D.PERSONID
                                    LEFT JOIN dbo.COMMATERIAL E ON B.MATERIALID = E.MATERIALID
                                    LEFT JOIN dbo.COMSUPPLIER F ON F.SUPPLIERID=B.SUPPLIERID
                                    LEFT JOIN dbo.COMPERSON G ON G.PERSONID = B.PERSONID
                            WHERE   C.CURRENTSTATE = 2
                                    AND B.BILLSTATE = 0 
                                    {0} ORDER BY B.PURCHASEORDER", sqlWhere);
            using (IDataReader orderReader = this.DataAccess.ExecuteDataReader(builder.ToString()))
            {
                SortedDictionary<purRollPlan, List<int>> dic = new SortedDictionary<purRollPlan, List<int>>(new purRollPlanCompare());
                while (orderReader.Read())
                {
                    purRollPlan key = new purRollPlan()
                    {
                        BillNo = LibSysUtils.ToString(orderReader["BILLNO"]),
                        Row_Id = LibSysUtils.ToInt32(orderReader["ROW_ID"]),
                        PurBillNo = LibSysUtils.ToString(orderReader["PURCHASEORDER"]),
                        PurPersonId = LibSysUtils.ToString(orderReader["PERSONID"]),
                        PurPersonName = LibSysUtils.ToString(orderReader["PERSONNAME"]),
                        SupplierId = LibSysUtils.ToString(orderReader["SUPPLIERID"]),
                        SupplierName = LibSysUtils.ToString(orderReader["SUPPLIERNAME"]),
                        SupplyUserId = LibSysUtils.ToString(orderReader["SUPPLYUSERID"]),
                        SupplyUserName = LibSysUtils.ToString(orderReader["SUPPLYUSERNAME"]),
                        MaterialId = LibSysUtils.ToString(orderReader["MATERIALID"]),
                        MaterialName = LibSysUtils.ToString(orderReader["MATERIALNAME"]),
                        DeliveryNoteNo = LibSysUtils.ToString(orderReader["DELIVERYNOTENO"]),
                        Barcode = LibSysUtils.ToString(orderReader["BARCODE"]),
                        ArriveDate = LibSysUtils.ToInt64(orderReader["ARRIVEDATE"]),
                        ArriveQuantity = LibSysUtils.ToInt32(orderReader["ARRIVEQUANTITY"]),
                        SupplyUserTel = LibSysUtils.ToString(orderReader["PHONENO"]),
                        WorkId = LibSysUtils.ToString(orderReader["WORKNO"]),
                    };
                    if (dic.Keys.Contains(key))
                    {
                        dic[key].Add(LibSysUtils.ToInt32(orderReader["PLANDATE"]));
                    }
                    else
                    {
                        dic.Add(key, new List<int>() { LibSysUtils.ToInt32(orderReader["PLANDATE"]) });
                    }
                }
                foreach (KeyValuePair<purRollPlan, List<int>> item in dic)
                {
                    item.Key.PlanDate = item.Value;
                    rollplanlist.Add(item.Key);
                }

            }
            return rollplanlist;
        }
        /// <summary>
        /// 登录后获取滚动计划数据
        /// </summary>
        /// <param name="supplyUserId"></param>
        /// <returns></returns>
        public List<purRollPlan> GetListRollPlan(string supplyUserId)
        {
            List<purRollPlan> rollplanlist = new List<purRollPlan>();
            StringBuilder builder = new StringBuilder();
            string supplierId = string.Empty;
            supplierId = supplyLogin.DefautDate.getSupplIer(supplyUserId);
            builder.AppendFormat(@"SELECT  B.BILLNO,
                                    B.ROW_ID,
                                    B.PURCHASEORDER ,
                                    B.PERSONID ,
                                    D.PERSONNAME ,
                                    B.SUPPLIERID,
                                    F.SUPPLIERNAME,
                                    B.SUPPLYUSERID ,
                                    D.PERSONNAME AS SUPPLYUSERNAME ,
                                    D.PHONENO,
                                    B.MATERIALID,
                                    E.MATERIALNAME,
                                    A.PLANDATE,
                                    A.DELIVERYNOTENO,
                                    A.BARCODE,
                                    A.ARRIVEDATE,
                                    A.ARRIVEQUANTITY,
                                    B.WORKNO
                            FROM    dbo.PURSUPPLYROLLPLANPOST A
                                    LEFT JOIN dbo.PURPURCHASEPLANDETAIL B ON A.FROMBILLNO = B.BILLNO
                                                                             AND A.FROMROWID = B.ROW_ID
                                    LEFT JOIN dbo.PURPURCHASEPLAN C ON B.BILLNO = C.BILLNO
                                    LEFT JOIN dbo.COMPERSON D ON B.PERSONID = D.PERSONID
                                    LEFT JOIN dbo.COMMATERIAL E ON B.MATERIALID = E.MATERIALID
                                    LEFT JOIN dbo.COMSUPPLIER F ON F.SUPPLIERID = B.SUPPLIERID
                            WHERE   C.CURRENTSTATE = 2
                                    AND B.BILLSTATE = 0 
                                    AND B.SUPPLYUSERID = '{0}'
                                    AND B.SUPPLIERID = '{1}'
                                    ORDER BY B.PURCHASEORDER", supplyUserId, supplierId);
            using (IDataReader orderReader = this.DataAccess.ExecuteDataReader(builder.ToString()))
            {
                SortedDictionary<purRollPlan, List<int>> dic = new SortedDictionary<purRollPlan, List<int>>(new purRollPlanCompare());
                while (orderReader.Read())
                {
                    purRollPlan key = new purRollPlan()
                    {
                        BillNo=LibSysUtils.ToString(orderReader["BILLNO"]),
                        Row_Id=LibSysUtils.ToInt32(orderReader["ROW_ID"]),
                        PurBillNo = LibSysUtils.ToString(orderReader["PURCHASEORDER"]),
                        PurPersonId = LibSysUtils.ToString(orderReader["PERSONID"]),
                        PurPersonName = LibSysUtils.ToString(orderReader["PERSONNAME"]),
                        SupplierId = LibSysUtils.ToString(orderReader["SUPPLIERID"]),
                        SupplierName = LibSysUtils.ToString(orderReader["SUPPLIERNAME"]),
                        SupplyUserId = LibSysUtils.ToString(orderReader["SUPPLYUSERID"]),
                        SupplyUserName = LibSysUtils.ToString(orderReader["SUPPLYUSERNAME"]),
                        MaterialId = LibSysUtils.ToString(orderReader["MATERIALID"]),
                        MaterialName = LibSysUtils.ToString(orderReader["MATERIALNAME"]),
                        DeliveryNoteNo = LibSysUtils.ToString(orderReader["DELIVERYNOTENO"]),
                        Barcode = LibSysUtils.ToString(orderReader["BARCODE"]),
                        ArriveDate = LibSysUtils.ToInt64(orderReader["ARRIVEDATE"]),
                        ArriveQuantity = LibSysUtils.ToInt32(orderReader["ARRIVEQUANTITY"]),
                        SupplyUserTel = LibSysUtils.ToString(orderReader["PHONENO"]),
                        WorkId = LibSysUtils.ToString(orderReader["WORKNO"]),
                    };
                    if (dic.Keys.Contains(key))
                    {
                        dic[key].Add(LibSysUtils.ToInt32(orderReader["PLANDATE"]));
                    }
                    else
                    {
                        dic.Add(key, new List<int>() { LibSysUtils.ToInt32(orderReader["PLANDATE"]) });
                    }
                }
                foreach (KeyValuePair<purRollPlan, List<int>> item in dic)
                {
                    item.Key.PlanDate = item.Value;
                    rollplanlist.Add(item.Key);
                }
            }
            return rollplanlist;


        }
        /// <summary>
        /// 比较器
        /// </summary>
        public class purRollPlanCompare : IComparer<purRollPlan>
        {
            public int Compare(purRollPlan x, purRollPlan y)
            {
                if (x.BillNo.CompareTo(y.BillNo) != 0)
                {
                    return x.BillNo.CompareTo(y.BillNo);
                }
                else if (x.Row_Id.CompareTo(y.Row_Id) != 0)
                {
                    return x.Row_Id.CompareTo(y.Row_Id);
                }
                else if (x.PurBillNo.CompareTo(y.PurBillNo) != 0)
                {
                    return x.PurBillNo.CompareTo(y.PurBillNo);
                }
                else if (x.WorkId.CompareTo(y.WorkId) != 0)
                {
                    return x.WorkId.CompareTo(y.WorkId);
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 获取同步天数
        /// </summary>
        /// <param name="supplierId"></param>
        /// <returns>同步天数</returns>
        public int getRollPlanCopyDate(string supplierId)
        {
            string sql = string.Empty;
            sql = string.Format("SELECT COPYDATE FROM AXPCOMPANYPARAM WHERE ORGID = {0}", LibStringBuilder.GetQuotString(supplierId));
            int copyDateNumber = 0;
            copyDateNumber = LibSysUtils.ToInt32(this.DataAccess.ExecuteScalar(sql));
            if (copyDateNumber > 0)
                return copyDateNumber;
            else
                return 3;
        }
        /// <summary>
        /// 保存滚动计划数据
        /// </summary>
        /// <param name="SaveRollPlanData"></param>
        /// <returns></returns>
        public bool saveRollPlanData(saveRollPlan SaveRollPlanData)
        {
            Dictionary<string, object> outPutValueDic = new Dictionary<string, object>();
            int planDate = LibSysUtils.ToInt32(SaveRollPlanData.ARRIVEDATE / 1000000);
            this.DataAccess.ExecuteStoredProcedure("PueSupplyRollPlanSave",
                out outPutValueDic,
                SaveRollPlanData.BILLNO, 
                SaveRollPlanData.ROW_ID, 
                SaveRollPlanData.DELIVERYNOTENO,
                SaveRollPlanData.BARCODE, 
                planDate, 
                SaveRollPlanData.ARRIVEDATE, 
                SaveRollPlanData.ARRIVEQUANTITY, 
                0);
            if (LibSysUtils.ToInt32(outPutValueDic["OUTPUTVALUE"]) != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}