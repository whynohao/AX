using AxCRL.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml.Serialization;
using AxCRL.Core.Comm;
using Newtonsoft.Json;
using AxCRL.Bcf.Sfl;
using System.Configuration;
using System.IO;
using AxCRL.Data;
using AxCRL.Comm.Utils;
using System.Text;
using AxCRL.Bcf;
using System.Runtime.Caching;

namespace Ax.Ui
{
    /// <summary>
    /// WsService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://ax.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    [XmlInclude(typeof(AxCRL.Bcf.Sfl.LoginInfo))]
    [XmlInclude(typeof(WorkstationInfo))]
    [XmlInclude(typeof(ExecCodeEnum))]
    [XmlInclude(typeof(WorkOrderInfo))]
    [XmlInclude(typeof(WorkstationConfig))]
    [XmlInclude(typeof(Badness))]
    [XmlInclude(typeof(LabelTemplateDetail))]
    [XmlInclude(typeof(BarcodeFixCode))]
    [XmlInclude(typeof(WorkstationType))]
    [XmlInclude(typeof(ScanBarcode))]
    [XmlInclude(typeof(LinkBarcode))]
    [XmlInclude(typeof(PrintBarcodeInfo))]
    [XmlInclude(typeof(BarcodeCheckResult))]
    [XmlInclude(typeof(BarcodeData))]
    [XmlInclude(typeof(WorkOrderInfo_Ws))]
    [XmlInclude(typeof(STKLoginInfo))]
    [XmlInclude(typeof(STKInWareInfo))]
    [XmlInclude(typeof(STKOutWareInfo))]
    [XmlInclude(typeof(STKSplitBox))]
    [XmlInclude(typeof(STKBarCodeCheckResult))]
    [XmlInclude(typeof(STKInWareInfoList))]
    [XmlInclude(typeof(STKCheckResult))]
    [XmlInclude(typeof(STKOutWareInfoList))]
    [XmlInclude(typeof(STKSplitBoxList))]
    [XmlInclude(typeof(STKInWareMain))]
    [XmlInclude(typeof(STKBillType))]
    [XmlInclude(typeof(STKOutWareMain))]
    [XmlInclude(typeof(LabelTemplateInfo))]
    [XmlInclude(typeof(LabelTemplateInfo_Ws))]
    [XmlInclude(typeof(PrintSpecialBarcodeInfo))]
    [XmlInclude(typeof(BatchPrintInfo))]
    [XmlInclude(typeof(MastersBarcodeByLinkBarcodeInfo_ws))]
    [XmlInclude(typeof(BatchBarcode_Ws))]
    [XmlInclude(typeof(Maintenance))]
    [XmlInclude(typeof(CheckOrder))]
    [XmlInclude(typeof(ComData))]
    [XmlInclude(typeof(CheckSolution))]
    [XmlInclude(typeof(mainBarCode))]
    [XmlInclude(typeof(List<CheckOrder>))]
    [XmlInclude(typeof(List<ComData>))]
    [XmlInclude(typeof(List<WorkOrderDetail>))]
    #region[HY新增类型]
    [XmlInclude(typeof(List<TenWorkRecord>))]
    [XmlInclude(typeof(ProduceBcfData))]
    //[XmlInclude(typeof(ChangeType))]
    //[XmlInclude(typeof(HYWorkStationConfig))]
    [XmlInclude(typeof(AbnormalReport))]
    [XmlInclude(typeof(List<AbnormalReport>))]
    [XmlInclude(typeof(List<Abnormal>))]
    [XmlInclude(typeof(List<Paint>))]
    [XmlInclude(typeof(List<PaintCar>))]
    [XmlInclude(typeof(List<Storage>))]
    [XmlInclude(typeof(List<BandingInfo>))]
    [XmlInclude(typeof(List<OpcConfig>))]
    #endregion
    public partial class AxWsService : System.Web.Services.WebService
    {

        [WebMethod]
        public ExecuteWsMethodResult ExecuteWsMethod_Ws(ExecuteWsMethodParam_Ws param)
        {
            WsService ws = new WsService();
            return ws.ExecuteWsMethod_Ws(param);
        }

        [WebMethod]
        public DataSet GetRpt_Ws(ExecuteWsMethodParam_Ws param)
        {
            WsService ws = new WsService();
            return ws.GetRpt_Ws(param);
        }

        [WebMethod]
        public DataSet GetVisualData_Ws(string progId, string jsonString)
        {
            LibQueryCondition libQueryCondition = null;
            try
            {
                libQueryCondition = (LibQueryCondition)JsonConvert.DeserializeObject(jsonString, typeof(LibQueryCondition));
            }
            catch
            {

            }
            WsService ws = new WsService();
            ExecuteWsMethodParam_Ws param = new ExecuteWsMethodParam_Ws();
            param.ProgId = progId;
            param.MethodName = "BrowseTo";
            if (libQueryCondition != null)
            {
                param.MethodParam = new object[] { libQueryCondition };
            }
            else
            {
                param.MethodParam = new object[] { new LibQueryCondition() };
            }
            return ws.GetRpt_Ws(param);
        }

        [WebMethod]
        public DataSet GetVisualData_Ws_T(string progId)
        {
            WsService ws = new WsService();
            ExecuteWsMethodParam_Ws param = new ExecuteWsMethodParam_Ws();
            param.ProgId = progId;
            param.MethodName = "LiveUpdate";
            return ws.GetRpt_Ws(param);
        }

        #region [二开]
        [WebMethod]
        public List<CheckOrderInfo> GetSpecMatList(int pagestart, int pagesize)
        {
            LibWsGatherBcf ws = new LibWsGatherBcf();
            return ws.GetSpecMatList(pagestart, pagesize);
        }

        [WebMethod]
        public List<CheckOrderInfo> GetSpecMatDetailList(string billno, string supplierid, string receivedate, string sqlwhere)
        {
            LibWsGatherBcf ws = new LibWsGatherBcf();
            return ws.GetSpecMatDetailList(billno, supplierid, receivedate, sqlwhere);
        }
        [WebMethod]
        public bool StartCheck(string billno, string supplierid, string receivedate)
        {
            LibWsGatherBcf ws = new LibWsGatherBcf();
            return ws.StartCheck(billno, supplierid, receivedate);
        }
        [WebMethod]
        public bool EndCheck(CheckOrderInfo[] checkorder)
        {
            LibWsGatherBcf ws = new LibWsGatherBcf();
            return ws.EndCheck(checkorder);
        }
        [WebMethod]
        public bool CheckAll(string billno, string supplierid, string receivedate)
        {
            LibWsGatherBcf ws = new LibWsGatherBcf();
            return ws.CheckAll(billno, supplierid, receivedate);
        }
        #endregion

        #region WinForm更新
        /***************************WinForm更新******************************/
        /// <summary>
        /// 获取版本号
        /// </summary>
        /// <returns>更新版本号</returns>
        [WebMethod]
        public string GetVersion()
        {
            return ConfigurationManager.AppSettings["version"];
        }
        /// <summary>
        /// 获取下载地址
        /// </summary>
        /// <returns>下载地址</returns>
        [WebMethod]
        public string GetUrl()
        {
            return ConfigurationManager.AppSettings["url"] + "/" + ConfigurationManager.AppSettings["directory"] + "/";
        }
        /// <summary>
        /// 获取下载zip压缩包
        /// </summary>
        /// <returns>下载zip压缩包</returns>
        [WebMethod]
        public string[] GetZips()
        {
            string folder = ConfigurationManager.AppSettings["extendPath"] + "\\VERSION\\" + ConfigurationManager.AppSettings["directory"];
            string[] zips = Directory.GetFileSystemEntries(folder);
            for (int i = 0; i < zips.Length; i++)
            {
                zips[i] = Path.GetFileName(zips[i]);
            }
            return zips;
        }
        #endregion

        #region 更新缓存
        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public void RemoveProduceCache(string billNo)
        {
            LibProduceCache.Default.Remove(billNo);
            LibWsControlServer.Default.RemoveWorkOrder(billNo);
        }

        /// <summary>
        /// 新增缓存
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public void AddProduceCache(string billNo)
        {
            LibWsControlServer.Default.AddWorkOrder(billNo);
        }
        #endregion

        /// <summary>
        /// 删除批号信息【退单\加急做批号剔除】
        /// </summary>
        /// <param name="workOrderNo">派工单号</param>
        /// <param name="fromSaleBillNo">销售订单号</param>
        /// <param name="produceLineId"></param>
        [WebMethod]
        public void RemoveFromSaleBillNo(string fromSaleBillNo)
        {
            //通过销售订单号查找派工单号
            string sql = string.Format(@"SELECT DISTINCT F.BILLNO
                                          FROM WORKORDER A
                                         INNER JOIN WORKORDERDETAIL B
                                            ON A.BILLNO = B.BILLNO
                                         INNER JOIN PLSSALESORDER C
                                            ON B.FROMBILLNO = C.BILLNO
                                         INNER JOIN PLSPRODUCEMONTHPLANDETAIL D
                                            ON D.WORKORDERBILLNO = B.BILLNO
                                         INNER JOIN PLSPRODUCEDAYPLANDETAIL E
                                            ON E.FROMBILLNO = D.BILLNO
                                           AND E.FROMROWID = D.ROW_ID
                                         INNER JOIN PPWORKORDER F
                                            ON E.PWORKORDERNO = F.BILLNO
                                         WHERE A.PARENTBILLNO IS NULL AND C.BILLNO = {0} ", LibStringBuilder.GetQuotString(fromSaleBillNo));
            LibDataAccess dataAccess = new LibDataAccess();
            StringBuilder builder = new StringBuilder();
            Int32 index = 1;
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string workOrderNo = LibSysUtils.ToString(reader["BILLNO"]);
                    HYProduceData hyProduceData = LibHYProduceCache.Default.GetProduceData(workOrderNo);
                    DataTable dt = hyProduceData.TenWorkRecord.Tables[0];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string saleBillNo = LibSysUtils.ToString(dt.Rows[i]["FROMSALEBILLNO"]);
                        if (saleBillNo == fromSaleBillNo)
                        {
                            dt.Rows[i].Delete();
                        }
                    }
                    dt.AcceptChanges();
                    if (index == 1)
                    {
                        builder.AppendFormat(" AND (WORKORDERNO = {0}", LibStringBuilder.GetQuotObject(reader["BILLNO"]));
                    }
                    else
                    {
                        builder.AppendFormat(" OR WORKORDERNO = {0}", LibStringBuilder.GetQuotObject(reader["BILLNO"]));
                    }
                    index++;
                }
            }
            if (builder.Length > 0)
            {
                builder.Append(")");
                sql = string.Format(@"DELETE FROM PPTENWORKRECORD WHERE 1=1 {0} AND FROMSALEBILLNO = '{1}'", builder.ToString(), fromSaleBillNo);
                LibDBTransaction trans = dataAccess.BeginTransaction();
                try
                {
                    int result = dataAccess.ExecuteNonQuery(sql);
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                }
            }
        }

        /// <summary>
        /// 新增批号信息
        /// </summary>
        /// <param name="workOrderNo">作业单号</param>
        /// <param name="lotNo">批号</param>
        /// <param name="dataAccess">事务，保证同一个事务</param>
        [WebMethod]
        public void AddGatherData(string pPWorkOrderNo, string saleBillNo)
        {
            HYProduceData produceData = LibHYProduceCache.Default.GetProduceData(pPWorkOrderNo);
            if (produceData != null)
            {
                LibBcfGrid ppWorkOrderBcf = (LibBcfGrid)LibBcfSystem.Default.GetBcfInstance("pp.TenWorkRecord");
                LibQueryCondition lb = new LibQueryCondition();
                lb.QueryFields.Add(new AxCRL.Core.Comm.LibQueryField()
                {
                    Name = "WORKORDERNO",
                    QueryChar = LibQueryChar.Equal,
                    Value = new List<object> { pPWorkOrderNo }
                });
                lb.QueryFields.Add(new AxCRL.Core.Comm.LibQueryField()
                {
                    Name = "FROMSALEBILLNO",
                    QueryChar = LibQueryChar.Equal,
                    Value = new List<object> { saleBillNo }
                });
                DataSet ds = ppWorkOrderBcf.BrowseTo(lb);
                if (ds.Tables.Count > 0)
                {
                    produceData.TenWorkRecord.EnforceConstraints = false;
                    try
                    {
                        DataTable dt = produceData.TenWorkRecord.Tables[0];
                        dt.BeginLoadData();
                        try
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                DataRow row = dt.NewRow();
                                row.BeginEdit();
                                try
                                {
                                    foreach (DataColumn column in ds.Tables[0].Columns)
                                    {
                                        row[column.ColumnName] = dr[column.ColumnName];
                                    }
                                }
                                finally
                                {
                                    row.EndEdit();
                                }
                                dt.Rows.Add(row);
                            }
                        }
                        finally
                        {
                            dt.EndLoadData();
                        }
                    }
                    finally
                    {
                        produceData.TenWorkRecord.EnforceConstraints = true;
                    }
                }
            }
        }



    }
}
