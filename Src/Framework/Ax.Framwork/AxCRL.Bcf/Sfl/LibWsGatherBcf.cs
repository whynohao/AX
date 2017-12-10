using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Server;
using AxCRL.Data;
using AxCRL.Data.SqlBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AxCRL.Comm.Bill;
namespace AxCRL.Bcf.Sfl
{
    public class LibWsGatherBcf : LibWsBcf
    {
        #region[工作站方法]
        #region zz winform
        public LabelTemplateInfo_Ws GetLabelTemplateInfo_Ws(string billno)
        {
            return new LabelTemplateInfo_Ws { LabelTemplateInfo = this.GetLabelTemplateInfo(billno).ToList<LabelTemplateInfo>() };
        }
        public MastersBarcodeByLinkBarcodeInfo_ws GetMastersBarcodeByLinkBarcode_ws(string barcode, bool isPackage)
        {
            return new MastersBarcodeByLinkBarcodeInfo_ws { MastersBarcodeByLinkBarcodeInfo = this.GetMastersBarcodeByLinkBarcode(barcode, isPackage) };
        }
        public BatchBarcode_Ws GetBatchBarcode_Ws(string barcodeRuleId, string billNo, int printNum, bool isMaster)
        {
            return new BatchBarcode_Ws { BatchBarcodeInfo = this.GetBatchBarcode(barcodeRuleId, billNo, printNum, isMaster).ToList<string>() };
        }
        #endregion
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="terminalId">设备编号</param>
        /// <param name="userId">人员账号</param>
        /// <returns>登录信息</returns>
        public LoginInfo Login(string terminalId, string userId)
        {
            if (string.IsNullOrEmpty(terminalId))
                throw new ArgumentNullException("设备标识");
            if (string.IsNullOrEmpty(terminalId))
                throw new ArgumentNullException("用户账号");
            LoginInfo info = new LoginInfo();
            SqlBuilder sqlBuilder = new SqlBuilder("com.Workstation");
            string sql = sqlBuilder.GetQuerySql(0, "A.WORKSTATIONID,A.WORKSTATIONNAME", string.Format("A.TERMINALID={0}", LibStringBuilder.GetQuotString(terminalId)));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                if (reader.Read())
                {
                    info.WorkstationId = LibSysUtils.ToString(reader[0]);
                    info.WorkstationName = LibSysUtils.ToString(reader[1]);
                }
            }
            sqlBuilder = new SqlBuilder("axp.User");
            sql = sqlBuilder.GetQuerySql(0, "A.PERSONID,A.PERSONNAME", string.Format("A.USERID={0} AND A.ISUSE=1", LibStringBuilder.GetQuotString(userId)));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                if (reader.Read())
                {
                    info.PersonId = LibSysUtils.ToString(reader[0]);
                    info.PersonName = LibSysUtils.ToString(reader[1]);
                }
            }
            if (string.IsNullOrEmpty(info.WorkstationId))
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "工作站点未设定");
            if (string.IsNullOrEmpty(info.PersonId))
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "人员账号错误");
            return info;
        }

        /// <summary>
        /// 更换人员
        /// </summary>
        /// <param name="userId">人员账号</param>
        /// <returns>登录信息</returns>
        public LoginInfo ChangePerson(string userId)
        {
            LoginInfo info = new LoginInfo();
            SqlBuilder sqlBuilder = new SqlBuilder("axp.User");
            string sql = sqlBuilder.GetQuerySql(0, "A.PERSONID,A.PERSONNAME", string.Format("A.USERID={0} AND A.ISUSE=1", LibStringBuilder.GetQuotString(userId)));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                if (reader.Read())
                {
                    info.PersonId = LibSysUtils.ToString(reader[0]);
                    info.PersonName = LibSysUtils.ToString(reader[1]);
                }
            }
            return info;
        }

        /// <summary>
        /// 获取派工单
        /// </summary>
        /// <param name="workstationId">站点编号</param>
        /// <returns></returns>
        public WorkOrderInfo_Ws ChooseWorkOrderList_Ws(string workstationId)
        {
            WorkOrderInfo_Ws ret = new WorkOrderInfo_Ws { WorkOrderInfo = ChooseWorkOrderList(workstationId) };
            return ret;
        }

        /// <summary>
        /// 获取派工单
        /// </summary>
        /// <param name="workstationId">站点编号</param>
        /// <returns></returns>
        public List<WorkOrderInfo> ChooseWorkOrderList(string workstationId)
        {
            List<WorkOrderInfo> workOrderList = new List<WorkOrderInfo>();
            ProductScheduling productScheduling = LibWsControlServer.Default.GetProductScheduling();
            StringBuilder builder = new StringBuilder();
            int i = 0;
            if (productScheduling.WsRelWorkOrder.ContainsKey(workstationId))
            {
                foreach (string item in productScheduling.WsRelWorkOrder[workstationId])
                {
                    if (i != 0)
                        builder.AppendFormat(",{0}", LibStringBuilder.GetQuotString(item));
                    else
                        builder.Append(LibStringBuilder.GetQuotString(item));
                    i++;
                }
                if (builder.Length > 0)
                {
                    SqlBuilder sqlBuilder = new SqlBuilder("pp.WorkOrder");
                    string sql = sqlBuilder.GetQuerySql(0, "A.BILLNO,A.MATERIALID,A.MATERIALNAME,A.MATERIALSPEC,A.QUANTITY,A.UNITID,A.UNITNAME,A.CUSTOMERID,A.CUSTOMERNAME,A.NOTICE,A.COMBINENUM,A.PACKAGENUM,A.PRODUCELINEID,A.PRODUCELINENAME", string.Format("A.BILLNO in ({0}) and A.WORKORDERSTATE = 1 AND A.CURRENTSTATE = 2", builder.ToString()), "A.BILLDATE DESC");
                    using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            workOrderList.Add(new WorkOrderInfo()
                            {
                                Remark = string.Format(@"单号:{0},
                                                         物料:{1},数量:{2}
                                                         客户:{3}", LibSysUtils.ToString(reader["BILLNO"]), LibSysUtils.ToString(reader["MATERIALNAME"]), LibSysUtils.ToDecimal(reader["QUANTITY"]), LibSysUtils.ToString(reader["CUSTOMERNAME"])),
                                BillNo = LibSysUtils.ToString(reader["BILLNO"]),
                                MaterialId = LibSysUtils.ToString(reader["MATERIALID"]),
                                MaterialName = LibSysUtils.ToString(reader["MATERIALNAME"]),
                                MaterialSpec = LibSysUtils.ToString(reader["MATERIALSPEC"]),
                                Quantity = LibSysUtils.ToDecimal(reader["QUANTITY"]),
                                UnitId = LibSysUtils.ToString(reader["UNITID"]),
                                UnitName = LibSysUtils.ToString(reader["UNITNAME"]),
                                CustomerId = LibSysUtils.ToString(reader["CUSTOMERID"]),
                                CustomerName = LibSysUtils.ToString(reader["CUSTOMERNAME"]),
                                Notice = LibSysUtils.ToString(reader["NOTICE"]),
                                CombineNum = LibSysUtils.ToDecimal(reader["COMBINENUM"]),
                                PackageNum = LibSysUtils.ToDecimal(reader["PACKAGENUM"]),
                                ProduceLineId = LibSysUtils.ToString(reader["PRODUCELINEID"]),
                                ProduceLineName = LibSysUtils.ToString(reader["PRODUCELINENAME"])
                            });
                        }
                    }
                }
            }
            return workOrderList;
        }

        /// <summary>
        /// 获取站点信息
        /// </summary>
        /// <param name="billNo">派工单号</param>
        /// <param name="workstationId">站点编号</param>
        /// <returns></returns>
        public WorkstationInfo GetWorkstationInfo(string billNo, string workstationId)
        {
            WorkstationInfo info = null;
            ProduceData produceData = LibProduceCache.Default.GetProduceData(billNo);
            if (produceData != null)
            {
                foreach (DataRow curRow in produceData.WorkOrder.Tables[3].Rows)
                {
                    if (String.CompareOrdinal(LibSysUtils.ToString(curRow["WORKSTATIONID"]), workstationId) == 0)
                    {
                        DataRow parentRow = produceData.WorkOrder.Tables[2].Rows.Find(new object[] { curRow["BILLNO"], curRow["PARENTROWID"] });
                        int workProcessNo = LibSysUtils.ToInt32(parentRow["WORKPROCESSNO"]);
                        info = GetWorkstationInfo(produceData.WorkProcessNo[workProcessNo].DataRow);
                        break;
                    }
                }
            }
            return info;
        }

        /// <summary>
        /// 获取站点配置
        /// </summary>
        /// <param name="configId">站点配置编号</param>
        /// <param name="masterRow">派工单表头</param>
        /// <returns></returns>
        private WorkstationConfig GetWorkstationConfig(string configId, DataRow masterRow)
        {
            WorkstationConfig config = new WorkstationConfig();
            SqlBuilder sqlBuilder = new SqlBuilder("com.WorkstationConfig");
            string sql = sqlBuilder.GetQuerySql(0, "A.WORKSTATIONTYPE,A.ALLOWCHANGEDATA,A.NEEDTAKEBADNESS,A.ISCOMBINE,A.SCANANY,A.ISACCURATECHECK",
                string.Format("A.WORKSTATIONCONFIGID={0}", LibStringBuilder.GetQuotString(configId)));
            Dictionary<int, ScanBarcode> tempDic = new Dictionary<int, ScanBarcode>();
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    config.WorkstationType = (WorkstationType)LibSysUtils.ToInt32(reader["WORKSTATIONTYPE"]);
                    config.AllowChangeData = LibSysUtils.ToBoolean(reader["ALLOWCHANGEDATA"]);
                    config.NeedTakeBadness = LibSysUtils.ToBoolean(reader["NEEDTAKEBADNESS"]);
                    config.IsCombine = LibSysUtils.ToBoolean(reader["ISCOMBINE"]);
                    config.ScanAny = LibSysUtils.ToBoolean(reader["SCANANY"]);
                    config.IsAccurateCheck = LibSysUtils.ToBoolean(reader["ISACCURATECHECK"]);
                }
            }
            sql = sqlBuilder.GetQuerySql(1, "B.ROW_ID,B.BARCODETYPEID,B.BARCODETYPENAME,B.BARCODERULEID,B.ISFROM,B.ISMASTER,B.BARCODERULEID<A.BARCODELENGTH,B.BARCODETYPEID<A.CHECKMATERIAL",
                string.Format("B.WORKSTATIONCONFIGID={0}", LibStringBuilder.GetQuotString(configId)), "B.ROWNO ASC");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    ScanBarcode scanBarcode = new ScanBarcode()
                    {
                        BarcodeTypeId = LibSysUtils.ToString(reader["BARCODETYPEID"]),
                        BarcodeTypeName = LibSysUtils.ToString(reader["BARCODETYPENAME"]),
                        BarcodeLength = LibSysUtils.ToInt32(reader["BARCODELENGTH"]),
                        CheckMaterial = LibSysUtils.ToBoolean(reader["CHECKMATERIAL"]),
                        IsMaster = LibSysUtils.ToBoolean(reader["ISMASTER"]),
                        IsFrom = LibSysUtils.ToBoolean(reader["ISFROM"])
                    };
                    string barcodeRuleId = LibSysUtils.ToString(reader["BARCODERULEID"]);
                    if (!string.IsNullOrEmpty(barcodeRuleId))
                    {
                        BarcodeRule barcodeRule = LibBarcodeRuleCache.Default.GetBarcodeRule(barcodeRuleId);
                        if (barcodeRule != null)
                        {
                            foreach (var item in barcodeRule.Items.Where(item => item.SectionType == BarcodeRuleSectionType.None))
                            {
                                scanBarcode.BarcodeFixCode.Add(new BarcodeFixCode(item.Start, item.Value));
                            }
                        }
                    }
                    config.ScanBarcode.Add(scanBarcode);
                    int rowId = LibSysUtils.ToInt32(reader["ROW_ID"]);
                    if (!tempDic.ContainsKey(rowId))
                        tempDic.Add(rowId, scanBarcode);
                }
            }
            sql = sqlBuilder.GetQuerySql(3, "D.PARENTROWID,D.FIELDNAME,D.PRINTNUM,D.BARCODERULEID,D.AUTOBUILD,D.ISMASTER,D.BARCODETYPEID,D.BARCODETYPENAME",
                string.Format("D.WORKSTATIONCONFIGID={0}", LibStringBuilder.GetQuotString(configId)), "D.ROWNO ASC");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    int parentRowId = LibSysUtils.ToInt32(reader["PARENTROWID"]);
                    if (tempDic.ContainsKey(parentRowId))
                    {
                        string name = LibSysUtils.ToString(reader["FIELDNAME"]);
                        string labelTemplateId = string.Empty;
                        if (!string.IsNullOrEmpty(name))
                            labelTemplateId = LibSysUtils.ToString(masterRow[name]);
                        tempDic[parentRowId].LabelTemplateDetail.Add(new LabelTemplateDetail(labelTemplateId, LibSysUtils.ToString(reader["BARCODETYPEID"]),
                            LibSysUtils.ToString(reader["BARCODETYPENAME"]), LibSysUtils.ToInt32(reader["PRINTNUM"]), LibSysUtils.ToString(reader["BARCODERULEID"]), LibSysUtils.ToBoolean(reader["AUTOBUILD"]),
                            LibSysUtils.ToBoolean(reader["ISMASTER"])));
                    }
                }
            }
            Dictionary<string, Dictionary<string, CheckItem>> ckDic = new Dictionary<string, Dictionary<string, CheckItem>>();
            sql = string.Format(@"SELECT DISTINCT A.CHECKSTID,B.CHECKSTNAME,C.CHECKITEMID,D.CHECKITEMNAME,D.ISFILL,D.CHECKITEMTYPE,E.UPLIMIT,E.LOWLIMIT,E.STANDARD,E.DEFECTID,F.DEFECTNAME FROM COMWSCONFIGCSDETAIL A LEFT JOIN CHECKSOLUTION B ON A.CHECKSTID = B.CHECKSTID
                                    LEFT JOIN CHECKSOLUTIONDETAIL C ON B.CHECKSTID = C.CHECKSTID
                                    LEFT JOIN CHECKITEM D ON C.CHECKITEMID = D.CHECKITEMID
                                    LEFT JOIN CHECKSTBADNESS E ON C.CHECKSTID = E.CHECKSTID AND C.ROW_ID = E.PARENTROWID
                                    LEFT JOIN COMDEFECT F ON E.DEFECTID = F.DEFECTID
                                    WHERE A.WORKSTATIONCONFIGID =  '{0}'", LibSysUtils.ToString(configId));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string checkStid = LibSysUtils.ToString(reader["CHECKSTID"]);
                    string checkStname = LibSysUtils.ToString(reader["CHECKSTNAME"]);
                    string ckKey = checkStid + "-" + checkStname;
                    string checkItemId = LibSysUtils.ToString(reader["CHECKITEMID"]);
                    CheckItemBadness ckBadness = new CheckItemBadness
                    {
                        UpLimit = LibSysUtils.ToDecimal(reader["UPLIMIT"]),
                        LowLimit = LibSysUtils.ToDecimal(reader["LOWLIMIT"]),
                        Standard = LibSysUtils.ToDecimal(reader["STANDARD"]),
                        BadnessId = LibSysUtils.ToString(reader["DEFECTID"]),
                        BadnessName = LibSysUtils.ToString(reader["DEFECTNAME"])
                    };
                    if (!ckDic.ContainsKey(ckKey))
                    {
                        Dictionary<string, CheckItem> dic = new Dictionary<string, CheckItem>();
                        CheckItem checkItem = new CheckItem
                        {
                            CheckItemId = checkItemId,
                            CheckItemName = LibSysUtils.ToString(reader["CHECKITEMNAME"]),
                            CheckItemType = LibSysUtils.ToInt32(reader["CHECKITEMTYPE"]),
                            IsFill = LibSysUtils.ToBoolean(reader["ISFILL"])
                        };
                        checkItem.CheckItemBadness.Add(ckBadness);
                        dic.Add(checkItemId, checkItem);
                        ckDic.Add(ckKey, dic);
                    }
                    else
                    {
                        if (!ckDic[ckKey].ContainsKey(checkItemId))
                        {
                            CheckItem checkItem = new CheckItem
                            {
                                CheckItemId = checkItemId,
                                CheckItemName = LibSysUtils.ToString(reader["CHECKITEMNAME"]),
                                CheckItemType = LibSysUtils.ToInt32(reader["CHECKITEMTYPE"]),
                                IsFill = LibSysUtils.ToBoolean(reader["ISFILL"])
                            };
                            checkItem.CheckItemBadness.Add(ckBadness);
                            ckDic[ckKey].Add(checkItemId, checkItem);
                        }
                        else
                        {
                            ckDic[ckKey][checkItemId].CheckItemBadness.Add(ckBadness);
                        }

                    }
                }
            }
            foreach (KeyValuePair<string, Dictionary<string, CheckItem>> pair in ckDic)
            {
                CheckSolution ck = new CheckSolution
                {
                    CheckSID = pair.Key.Split('-')[0],
                    CheckSName = pair.Key.Split('-')[1]
                };
                foreach (KeyValuePair<string, CheckItem> itempair in pair.Value)
                {
                    ck.CheckItem.Add(itempair.Value);
                }
                config.CheckSolution.Add(ck);
            }
            return config;
        }

        /// <summary>
        /// 获取派工单信息
        /// </summary>
        /// <param name="billNo">派工单号</param>
        /// <returns></returns>
        public WorkOrderInfo GetWorkOrderInfo(string billNo)
        {
            WorkOrderInfo info = new WorkOrderInfo();
            ProduceData produceData = LibProduceCache.Default.GetProduceData(billNo);
            if (produceData != null)
            {
                DataRow masterRow = produceData.WorkOrder.Tables[0].Rows[0];
                info.BillNo = billNo;
                info.CustomerId = LibSysUtils.ToString(masterRow["CUSTOMERID"]);
                info.CustomerName = LibSysUtils.ToString(masterRow["CUSTOMERNAME"]);
                info.MaterialId = LibSysUtils.ToString(masterRow["MATERIALID"]);
                info.MaterialName = LibSysUtils.ToString(masterRow["MATERIALNAME"]);
                info.MaterialSpec = LibSysUtils.ToString(masterRow["MATERIALSPEC"]);
                info.UnitId = LibSysUtils.ToString(masterRow["UNITID"]);
                info.UnitName = LibSysUtils.ToString(masterRow["UNITNAME"]);
                info.Quantity = LibSysUtils.ToDecimal(masterRow["QUANTITY"]);
                info.Notice = LibSysUtils.ToString(masterRow["NOTICE"]);
                info.CombineNum = LibSysUtils.ToDecimal(masterRow["COMBINENUM"]);
                info.PackageNum = LibSysUtils.ToDecimal(masterRow["PACKAGENUM"]);
                info.ProduceLineId = LibSysUtils.ToString(masterRow["PRODUCELINEID"]);
                info.ProduceLineName = LibSysUtils.ToString(masterRow["PRODUCELINENAME"]);
            }
            return info;
        }

        /// <summary>
        /// 条码插入
        /// </summary>
        /// <param name="barcodeData">条码信息</param>
        /// <param name="checkSolution">检测方案</param>
        /// <param name="isPackage">是否包装</param>
        /// <param name="changeData">条码变更数据</param>
        /// <param name="isCheck">是否检测</param>
        /// <param name="isCheckDetail">是否精确检测</param>
        public void WriteBarcodeData(BarcodeData barcodeData, CheckSolution checkSolution, bool isPackage = false, bool changeData = false, bool isCheck = false, bool isCheckDetail = false)
        {
            long currentTime = LibDateUtils.GetCurrentDateTime();
            LibDBTransaction trans = DataAccess.BeginTransaction();
            try
            {
                ProduceData produceData = LibProduceCache.Default.GetProduceData(barcodeData.BillNo);
                DataSet ds = produceData.WorkOrder;
                int checkOrderType = LibSysUtils.ToInt32(ds.Tables[0].Rows[0]["ISFROMPLAN"]);
                string sqlPro;
                if (changeData)
                {
                    //先删除现有条码数据
                    sqlPro = GetTableName(TableType.DeleteBarcode, checkOrderType);
                    DataAccess.ExecuteStoredProcedure(sqlPro, barcodeData.Barcode, barcodeData.BillNo, barcodeData.WorkProcessNo, isPackage);
                    //删除主码对应的关联码之前在该工序中存在的记录。主要是针对电芯捆绑，几个条码关联的，以及包装工序
                    if (barcodeData.LinkBarcodeList.Count > 0)
                    {
                        sqlPro = GetTableName(TableType.DeleteLinkBarcode, checkOrderType);
                        foreach (LinkBarcode t in barcodeData.LinkBarcodeList)
                        {
                            DataAccess.ExecuteStoredProcedure(sqlPro, t.Barcode, barcodeData.BillNo, barcodeData.WorkProcessNo);
                        }
                    }
                }
                sqlPro = GetTableName(TableType.WriteBarcode, checkOrderType);
                DataAccess.ExecuteStoredProcedure(sqlPro, barcodeData.Barcode, barcodeData.WorkProcessNo, barcodeData.BillNo, currentTime, barcodeData.IsPass,
                    barcodeData.BarcodeTypeId, barcodeData.BarcodeTypeName, barcodeData.WorkstationId, barcodeData.WorkstationName, barcodeData.WorkshopSectionId, barcodeData.WorkshopSectionName,
                    barcodeData.WorkProcessId, barcodeData.WorkProcessName, barcodeData.ProduceLineId, barcodeData.ProduceLineName, barcodeData.CreatorId, barcodeData.CreatorName, 1);
                if (checkOrderType > 1)
                {
                    UpdateCheckOrderNum(barcodeData.BillNo, barcodeData.IsPass, false, 0, 0, barcodeData.WorkProcessNo, LibDateUtils.GetCurrentDateTime());
                }
                if (isCheckDetail)
                {
                    sqlPro = GetTableName(TableType.WriteCheckItem, checkOrderType);
                    foreach (CheckItem item in checkSolution.CheckItem)
                    {
                        DataAccess.ExecuteStoredProcedure(sqlPro, barcodeData.Barcode, barcodeData.WorkProcessNo, barcodeData.BillNo, currentTime, item.IsPass,
                        checkSolution.CheckSID, checkSolution.CheckSName, item.CheckItemId, item.CheckItemName, item.CheckItemType, item.CheckValue, barcodeData.CreatorId, barcodeData.CreatorName);
                    }
                }
                if (barcodeData.BandnessList.Count > 0)
                {
                    sqlPro = GetTableName(TableType.WriteBarcodeBad, checkOrderType);
                    HashSet<string> set = new HashSet<string>();
                    foreach (var item in barcodeData.BandnessList.Where(item => !set.Contains(item.BadnessId)))
                    {
                        set.Add(item.BadnessId);
                        DataAccess.ExecuteStoredProcedure(sqlPro, barcodeData.Barcode, barcodeData.WorkProcessNo, barcodeData.BillNo, currentTime, item.BadnessId, item.BadnessName);
                    }
                }
                if (barcodeData.LinkBarcodeList.Count > 0)
                {
                    sqlPro = GetTableName(TableType.WriteLinkBarcode, checkOrderType);
                    HashSet<string> set = new HashSet<string>();
                    foreach (var item in barcodeData.LinkBarcodeList.Where(item => !set.Contains(item.Barcode)))
                    {
                        set.Add(item.Barcode);
                        if (isPackage)
                            DataAccess.ExecuteStoredProcedure("wsWritePackageBarcode", barcodeData.Barcode, item.Barcode, barcodeData.WorkProcessNo, currentTime, item.BarcodeTypeId, item.BarcodeTypeName);
                        else
                            DataAccess.ExecuteStoredProcedure(sqlPro, barcodeData.Barcode, item.Barcode, barcodeData.WorkProcessNo, barcodeData.BillNo, currentTime, item.BarcodeTypeId, item.BarcodeTypeName);
                    }
                }
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 通过关联码查找与之关联的主码
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <param name="isPackage">是否包装</param>
        /// <returns></returns>
        public List<MastersBarcodeByLinkBarcodeInfo> GetMastersBarcodeByLinkBarcode(string barcode, bool isPackage)
        {
            string sql;
            List<MastersBarcodeByLinkBarcodeInfo> listInfo = new List<MastersBarcodeByLinkBarcodeInfo>();
            if (isPackage)
            {
                sql = string.Format("SELECT PACKAGEBARCODE,BARCODE,BARCODETYPENAME FROM WSPACKAGERECORD WHERE BARCODE = '{0}' OR PACKAGEBARCODE = '{0}' ", barcode);
            }
            else
            {
                sql = string.Format("SELECT BARCODE,LINKBARCODE,BARCODETYPENAME FROM WSLINKBARCODE WHERE (LINKBARCODE = '{0}' OR BARCODE = '{0}') ", barcode);
            }
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    MastersBarcodeByLinkBarcodeInfo info = new MastersBarcodeByLinkBarcodeInfo();
                    info.LinkBarcode = reader.GetString(1);
                    info.LinkBarcodeType = reader.GetString(2);
                    info.Masterbarcode = reader.GetString(0);
                    listInfo.Add(info);
                }
            }
            foreach (MastersBarcodeByLinkBarcodeInfo info in listInfo)
            {
                sql = string.Format("SELECT DISTINCT BARCODETYPENAME FROM WSBARCODE WHERE BARCODE = '{0}'", info.Masterbarcode);
                info.MasterBarcodeType = LibSysUtils.ToString(this.DataAccess.ExecuteScalar(sql));
            }
            return listInfo;
        }

        /// <summary>
        /// 获取站点信息
        /// </summary>
        /// <param name="curRow">当前行</param>
        /// <returns></returns>
        private WorkstationInfo GetWorkstationInfo(DataRow curRow)
        {
            WorkstationInfo info = new WorkstationInfo
            {
                WorkProcessId = LibSysUtils.ToString(curRow["WORKPROCESSID"]),
                WorkProcessName = LibSysUtils.ToString(curRow["WORKPROCESSNAME"]),
                WorkshopSectionId = LibSysUtils.ToString(curRow["WORKSHOPSECTIONID"]),
                WorkshopSectionName = LibSysUtils.ToString(curRow["WORKSHOPSECTIONNAME"]),
                WorkProcessNo = LibSysUtils.ToInt32(curRow["WORKPROCESSNO"]),
                WorkstationConfigId = LibSysUtils.ToString(curRow["WORKSTATIONCONFIGID"]),
                WorkstationConfigName = LibSysUtils.ToString(curRow["WORKSTATIONCONFIGNAME"])
            };
            info.BadnessSetting = GetBadnessSetting(info.WorkstationConfigId);
            info.WorkstationConfig = GetWorkstationConfig(info.WorkstationConfigId, curRow.Table.DataSet.Tables[0].Rows[0]);
            return info;
        }

        /// <summary>
        /// 自动切换站点
        /// </summary>
        /// <param name="result">条码检测结果</param>
        /// <param name="billNo">派工单号</param>
        /// <param name="workstationId">站点编号</param>
        /// <param name="isFirst">是否首道工序</param>
        /// <param name="produceData">缓存派工单数据</param>
        /// <param name="workProcessNo">工序号</param>
        /// <param name="checkMaterial">查看物料是否属于同一组(超威)</param>
        /// <param name="isCheckExistLinkBarcode">是否需要检查存在关联条码</param>
        private void AutoChangeWorkstation(BarcodeCheckResult result, string billNo, string workstationId, bool isFirst, ref ProduceData produceData, ref int workProcessNo, ref bool checkMaterial, ref bool isCheckExistLinkBarcode)
        {
            result.WorkOrderInfo = GetWorkOrderInfo(billNo);
            produceData = LibProduceCache.Default.GetProduceData(billNo);
            if (isFirst)
            {
                workProcessNo = produceData.FirstWorkProcessNo[0];
            }
            else
            {
                foreach (DataRow curRow in produceData.WorkOrder.Tables[3].Rows)
                {
                    if (String.CompareOrdinal(LibSysUtils.ToString(curRow["WORKSTATIONID"]), workstationId) == 0)
                    {
                        DataRow parentRow = produceData.WorkOrder.Tables[2].Rows.Find(new object[] { curRow["BILLNO"], curRow["PARENTROWID"] });
                        workProcessNo = LibSysUtils.ToInt32(parentRow["WORKPROCESSNO"]);
                        break;
                    }
                }
            }
            result.WorkstationInfo = GetWorkstationInfo(produceData.WorkProcessNo[workProcessNo].DataRow);
            if (result.WorkstationInfo != null)
            {
                checkMaterial = result.WorkstationInfo.WorkstationConfig.ScanBarcode[0].CheckMaterial;
                isCheckExistLinkBarcode = !result.WorkstationInfo.WorkstationConfig.ScanBarcode[0].IsMaster;
            }
        }

        /// <summary>
        /// 自动切换派工单
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <param name="workstationId">站点编号</param>
        /// <param name="execCode">返回值类型</param>
        /// <param name="isFirst">是否首道工序</param>
        /// <returns>派工单号</returns>
        private string AutoChangeWorkOrder(string barcode, string workstationId, ref ExecCodeEnum execCode, ref bool isFirst)
        {
            string billNo = string.Empty;
            ProductScheduling productScheduling = LibWsControlServer.Default.GetProductScheduling();
            bool isExisted = false;
            string[] tableName = new string[] { "WSBARCODE", "WSIQCBARCODE", "WSOQCBARCODE" };
            if (productScheduling.WsRelWorkOrder.ContainsKey(workstationId))
            {
                foreach (string t in tableName)
                {
                    if (!isExisted)
                    {
                        using (IDataReader reader = this.DataAccess.ExecuteDataReader(string.Format("select distinct BILLNO from {1} where BARCODE={0}", LibStringBuilder.GetQuotString(barcode), t)))
                        {
                            int num = 0;
                            while (reader.Read())
                            {
                                if (!isExisted)
                                {
                                    isExisted = true;
                                }
                                string tempBillNo = LibSysUtils.ToString(reader["BILLNO"]);
                                if (productScheduling.WsRelWorkOrder[workstationId].Contains(tempBillNo))
                                {
                                    num++;
                                    billNo = tempBillNo;
                                }
                                if (num > 1)
                                {
                                    //仅有一笔工单满足条件的情况下，才赋值，否则让用户选择工单
                                    billNo = string.Empty;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(billNo))
                {
                    if (productScheduling.FirstWs.Contains(workstationId))
                    {
                        IList<string> bills = productScheduling.WsRelWorkOrder[workstationId];
                        //为第一道工序，仅存在一个可以使用的工单，则自动做切换
                        if (bills.Count == 1)
                        {
                            //工单和站点（工序工段信息）要切换
                            billNo = bills[0];
                            isFirst = true;
                        }
                        else
                        {
                            //为第一道工序,存在多个可用工单,则通知前端出现工单选择窗口
                            execCode = ExecCodeEnum.MoreWorkOrder;
                        }
                    }
                    else
                    {
                        //如果不为第一道工序，仅有一笔可用工单则赋值
                        IList<string> bills = productScheduling.WsRelWorkOrder[workstationId];
                        if (bills.Count == 1)
                            billNo = bills[0];
                        else
                            execCode = ExecCodeEnum.MoreWorkOrder;
                    }
                }
                else
                {
                    IList<string> bills = productScheduling.WsRelWorkOrder[workstationId];
                    if (!bills.Contains(billNo))
                    {
                        if (productScheduling.FirstWs.Contains(workstationId))
                        {
                            bills = productScheduling.WsRelWorkOrder[workstationId];
                            if (!bills.Contains(billNo))
                            {   //是第一道工序，且当前站点对应的工单不等于条码上道工序所在的工单，且生效工单仅存在1个，则取工单。
                                //工单存在多个则让用户自己手动选择
                                if (bills.Count == 1)
                                {
                                    billNo = bills[0];
                                    isFirst = true;
                                }
                                else
                                {
                                    billNo = string.Empty;
                                    execCode = ExecCodeEnum.MoreWorkOrder;
                                }
                            }
                        }
                        else
                        {
                            billNo = string.Empty;
                            if (bills.Count == 1)
                                billNo = bills[0];
                            else
                                execCode = ExecCodeEnum.MoreWorkOrder;
                        }
                    }
                }
            }
            if (execCode == ExecCodeEnum.MoreWorkOrder)
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("请先选择工单"));
            else if (string.IsNullOrEmpty(billNo))
            {
                execCode = ExecCodeEnum.NotFindWorkOrder;
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("没有可用工单"));
            }
            return billNo;
        }

        /// <summary>
        /// 切换到其他站点
        /// </summary>
        /// <param name="result">条码检测结果</param>
        /// <param name="billNo">派工单号</param>
        /// <param name="workstationId">站点编号</param>
        /// <param name="barcode">条码</param>
        /// <param name="produceData">缓存派工单数据</param>
        /// <param name="sqlPro">存储过程</param>
        /// <param name="execCode">返回值类型</param>
        /// <param name="workProcessNo">工序号</param>
        /// <param name="checkMaterial">查看物料是否属于同一组(超威)</param>
        /// <param name="isCheckExistLinkBarcode">是否需要检查存在关联条码</param>
        /// <returns></returns>
        private bool AutoChangeMoreWorkstation(BarcodeCheckResult result, string billNo, string workstationId, string barcode,
            ProduceData produceData, string sqlPro, ref ExecCodeEnum execCode, ref int workProcessNo, ref bool checkMaterial, ref bool isCheckExistLinkBarcode)
        {
            //当前工序是否在当前工单下存在另外的工序号
            IList<int> curWsRelWorkProcessNo = produceData.WsRelWorkProcessNo[workstationId];
            bool isFind = false;
            if (curWsRelWorkProcessNo.Count > 1)
            {
                foreach (int item in curWsRelWorkProcessNo)
                {
                    if (item <= workProcessNo)
                        continue;
                    Dictionary<string, object> outputData;
                    this.DataAccess.ExecuteStoredProcedure(sqlPro, out outputData, billNo, barcode, item, produceData.WorkProcessNo[item].PreWorkProcessNo[0], 0, string.Empty);
                    execCode = (ExecCodeEnum)LibSysUtils.ToInt32(outputData["EXECCODE_VAL"]);
                    if (execCode == ExecCodeEnum.Success)
                    {
                        //当前站点信息进行切换
                        result.WorkstationInfo = GetWorkstationInfo(produceData.WorkProcessNo[item].DataRow);
                        workProcessNo = result.WorkstationInfo.WorkProcessNo;
                        checkMaterial = result.WorkstationInfo.WorkstationConfig.ScanBarcode[0].CheckMaterial;
                        isCheckExistLinkBarcode = !result.WorkstationInfo.WorkstationConfig.ScanBarcode[0].IsMaster;
                        isFind = true;
                        break;
                    }
                }
            }
            return isFind;
        }

        /// <summary>
        /// 对于没有找到前道数据尝试切换派工单
        /// </summary>
        /// <param name="preBillNo">前一个派工单号</param>
        /// <param name="preWorkProcessNo">前一个工序号</param>
        /// <param name="result">条码检测结果</param>
        /// <param name="billNo">派工单号</param>
        /// <param name="workstationId">站点编号</param>
        /// <param name="barcode">条码</param>
        /// <param name="produceData">缓存派工单数据</param>
        /// <param name="sqlPro">存储过程</param>
        /// <param name="execCode">返回值类型</param>
        /// <param name="workProcessNo">工序号</param>
        /// <param name="checkMaterial">查看物料是否属于同一组(超威)</param>
        /// <param name="isCheckExistLinkBarcode">是否需要检查存在关联条码</param>
        private void AutoChangeWorkOrderForNotFindPreData(string preBillNo, List<int> preWorkProcessNo, BarcodeCheckResult result, string billNo, string workstationId, string barcode,
            ProduceData produceData, string sqlPro, ref ExecCodeEnum execCode, ref int workProcessNo, ref bool checkMaterial, ref bool isCheckExistLinkBarcode)
        {
            //当前条码在前道工序未存在记录，则判断此条码在数据库里的工单号是否和当前工单号一致，如果不一致说明已经是另外的工单了
            if (string.Compare(billNo, preBillNo, StringComparison.OrdinalIgnoreCase) == 0)
            {
                //判断当前工序在当前工单下是否存在另外的工序角色，如果存在，则依次用不同的角色测试上道工序是否存在，如果存在则切换当前站点的信息。
                bool isFind = AutoChangeMoreWorkstation(result, billNo, workstationId, barcode, produceData, sqlPro, ref execCode, ref workProcessNo, ref checkMaterial, ref isCheckExistLinkBarcode);
                if (!isFind)
                {
                    //不存在另外的工序角色
                    execCode = ExecCodeEnum.NotDataInPreWP;
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}在前道工序无记录。", billNo, barcode));
                }
            }
            else
            {
                //preBillNo为空，表示当前条码在数据库中不存在或者存在多笔单号
                if (string.IsNullOrEmpty(preBillNo))
                {
                    if (!preWorkProcessNo.Contains(0)) //如果为0说明是当前工单的第一道工序，则不需要做特殊处理
                    {
                        //如果在当前工单下不是第一道工序，则要判断是否在其他的工单下是第一道工序
                        ProductScheduling productScheduling = LibWsControlServer.Default.GetProductScheduling();
                        if (productScheduling.FirstWs.Contains(workstationId))
                        {
                            IList<string> bills = productScheduling.WsRelWorkOrder[workstationId];
                            //为第一道工序，仅存在一个可以使用的工单，则自动做切换
                            if (bills.Count == 1)
                            {
                                //工单和站点（工序工段信息）要切换
                                result.WorkOrderInfo = GetWorkOrderInfo(bills[0]);
                                produceData = LibProduceCache.Default.GetProduceData(bills[0]);
                                result.WorkstationInfo = GetWorkstationInfo(produceData.WorkProcessNo[produceData.FirstWorkProcessNo[0]].DataRow);
                            }
                            else
                            {
                                //为第一道工序,存在多个可用工单,则通知前端出现工单选择窗口
                                result.NeedWorkOrder = true;
                                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}在前道工序无记录。", billNo, barcode));
                            }
                        }
                        else
                        {
                            //没有设定为可用工单的第一道工序，则表示是单前工单，且在前道工序无记录
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}在前道工序无记录。", billNo, barcode));
                        }
                    }
                }
                else
                {
                    //已近是另外的工单自动切换站点上下文。如果数据库还不存在此条码的信息，则判断当前可用工单中是否存在当前站点为第一个站点的工单，
                    //如果有存在则，做为第一个站点记录数据，如果当前站点对应多个工单号，则让用户选择工单，如果为一个工单则自动切换为新工单
                    ProductScheduling productScheduling = LibWsControlServer.Default.GetProductScheduling();
                    if (productScheduling.WsRelWorkOrder[workstationId].Contains(preBillNo))
                    {
                        result.WorkOrderInfo = GetWorkOrderInfo(preBillNo);
                        produceData = LibProduceCache.Default.GetProduceData(preBillNo);
                        if (produceData != null && produceData.WsRelWorkProcessNo.ContainsKey(workstationId))
                        {
                            bool isFind = false;

                            //重置在新工单下，当前站点的工序号
                            IList<int> curWsRelWorkProcessNo = produceData.WsRelWorkProcessNo[workstationId];
                            foreach (int item in curWsRelWorkProcessNo)
                            {
                                workProcessNo = item;
                                if (produceData.WorkProcessNo.ContainsKey(workProcessNo))
                                {
                                    preWorkProcessNo = produceData.WorkProcessNo[workProcessNo].PreWorkProcessNo;
                                    object[] obj = CheckPreWs(produceData, workProcessNo, preBillNo, barcode, preWorkProcessNo, sqlPro);
                                    execCode = (ExecCodeEnum)obj[0];
                                    switch (execCode)
                                    {
                                        case ExecCodeEnum.Success:
                                            result.WorkstationInfo = GetWorkstationInfo(produceData.WorkProcessNo[item].DataRow);
                                            isFind = true; //站点对应此工序号
                                            break;
                                        case ExecCodeEnum.Existed:
                                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}在此工序已存在记录。", preBillNo, barcode));
                                            break;
                                        case ExecCodeEnum.NotPassInPreWP:
                                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}在前道工序为不通过。", preBillNo, barcode));
                                            break;
                                        case ExecCodeEnum.NotDataInPreWP:
                                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}在前道工序无记录。", preBillNo, barcode));
                                            break;
                                        case ExecCodeEnum.None:
                                            execCode = ExecCodeEnum.CheckSqlError;
                                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("{0}的检查未能执行,请检查是否出现异常。", barcode));
                                            break;
                                    }
                                }
                                if (isFind)
                                    break;

                            }
                            if (!isFind)
                            {
                                if (this.ManagerMessage.Count <= 0)
                                {
                                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}在前道工序无记录。", preBillNo, barcode));
                                }
                            }
                        }

                        else
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("获取不到工单{0}的数据。", preBillNo));
                            execCode = ExecCodeEnum.NotFindWorkOrder;
                        }
                    }
                    else
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}在前道工序无记录。", billNo, barcode));
                    }
                }
            }
        }

        /// <summary>
        /// 检查关联条码是否被使用
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <param name="workProcessNo">工序号</param>
        /// <param name="isPackage">是否包装</param>
        /// <param name="isChangeData">是否数据变更</param>
        /// <param name="sqlPro">存储过程标识</param>
        /// <param name="execCode">返回值类型</param>
        /// <returns></returns>
        private bool CheckLinkBarcodeUsed(string barcode, int workProcessNo, bool isPackage, bool isChangeData, string sqlPro, ref ExecCodeEnum execCode)
        {
            //检查条码是否已经使用过
            bool exist = false;
            Dictionary<string, object> outputData = null;
            this.DataAccess.ExecuteStoredProcedure(sqlPro, out outputData, barcode, workProcessNo, isPackage, false);
            exist = LibSysUtils.ToBoolean(outputData["EXIST_VAL"]);
            if (!exist || isChangeData) return exist;
            execCode = ExecCodeEnum.Existed;
            this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前条码在此工序已存在记录");
            return exist;
        }

        /// <summary>
        /// 条码检查
        /// </summary>
        /// <param name="workstationId">站点编号</param>
        /// <param name="barcode">条码</param>
        /// <param name="workProcessNo">工序号</param>
        /// <param name="billNo">派工单号</param>
        /// <param name="isCheckExistLinkBarcode">是否需要检查存在关联条码</param>
        /// <param name="isRealBarcode">是否主码</param>
        /// <param name="checkMaterial">查看物料是否属于同一组(超威)</param>
        /// <param name="classifyBarcode">组码</param>
        /// <param name="changeData">变更数据</param>
        /// <param name="isPackage">是否包装</param>
        /// <returns></returns>
        public virtual BarcodeCheckResult CheckBarcodeData(string workstationId, string barcode,
            int workProcessNo, string billNo, bool isCheckExistLinkBarcode = false, bool isRealBarcode = true, bool checkMaterial = false,
            string classifyBarcode = "", bool changeData = false, bool isPackage = false)
        {
            BarcodeCheckResult result = new BarcodeCheckResult();
            ExecCodeEnum execCode = ExecCodeEnum.None;
            ProduceData produce = LibProduceCache.Default.GetProduceData(billNo);
            DataSet ds = produce.WorkOrder;
            string sqlPro;
            int checkOrderType = 0;
            if (!string.IsNullOrEmpty(billNo))
            {
                checkOrderType = LibSysUtils.ToInt32(ds.Tables[0].Rows[0]["ISFROMPLAN"]);
            }
            if (!isRealBarcode)
            {
                //表明扫入的是其他关联码，此时应该查找到真正的barcode
                Dictionary<string, object> outputData = null;
                sqlPro = GetTableName(TableType.FindRealBarcode, checkOrderType);
                this.DataAccess.ExecuteStoredProcedure(sqlPro, out outputData, barcode, string.Empty);
                string realBarcode = LibSysUtils.ToString(outputData["REALBARCODE_VAL"]);
                if (string.IsNullOrEmpty(realBarcode))
                {
                    execCode = ExecCodeEnum.NotFindRelBarcode;
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}找不到关联条码。", billNo, barcode));
                }
                else
                {
                    barcode = realBarcode;
                    result.RealBarcode = realBarcode;
                }
            }
            if (execCode == ExecCodeEnum.None)
            {
                ProduceData produceData = null;
                if (string.IsNullOrEmpty(billNo))
                {
                    //没有选择单号，则先自动查找匹配的工单，如果找到则自动切换站点信息
                    bool isFirst = false;
                    billNo = AutoChangeWorkOrder(barcode, workstationId, ref execCode, ref isFirst);
                    if (execCode == ExecCodeEnum.MoreWorkOrder)
                        result.NeedWorkOrder = true;
                    if (!string.IsNullOrEmpty(billNo))
                    {
                        AutoChangeWorkstation(result, billNo, workstationId, isFirst, ref produceData, ref workProcessNo, ref checkMaterial, ref isCheckExistLinkBarcode);
                        produce = LibProduceCache.Default.GetProduceData(billNo);
                        ds = produce.WorkOrder;
                        checkOrderType = LibSysUtils.ToInt32(ds.Tables[0].Rows[0]["ISFROMPLAN"]);
                    }
                }
                if (!string.IsNullOrEmpty(billNo) && produceData == null)
                    produceData = LibProduceCache.Default.GetProduceData(billNo);
                if (produceData != null && produceData.WorkProcessNo.ContainsKey(workProcessNo))
                {
                    //检查上道工序是否通过，以及本道工序是否已存在记录
                    List<int> preWorkProcessNo = produceData.WorkProcessNo[workProcessNo].PreWorkProcessNo;
                    sqlPro = GetTableName(TableType.CheckBarcode, checkOrderType);
                    object[] obj = CheckPreWs(produceData, workProcessNo, billNo, barcode, preWorkProcessNo, sqlPro);
                    execCode = (ExecCodeEnum)obj[0];
                    string preBillNo = LibSysUtils.ToString(obj[1]);
                    switch (execCode)
                    {
                        case ExecCodeEnum.Existed:
                            if (changeData)
                            {
                                //数据变更情况线下 检查当前工序存在 且还未流入后续工序的记录
                                int nextWorkProcessNo = produceData.WorkProcessNo[workProcessNo].NextWorkProcessNo;
                                if (nextWorkProcessNo != 0)
                                {
                                    Dictionary<string, object> outputDic = null;
                                    sqlPro = GetTableName(TableType.CheckWPNextExist, checkOrderType);
                                    this.DataAccess.ExecuteStoredProcedure(sqlPro, out outputDic, barcode, billNo, nextWorkProcessNo, false);
                                    bool exist = LibSysUtils.ToBoolean(outputDic["EXIST_VAL"]);
                                    if (exist)
                                        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}在此工序的下道工序{2}已存在记录。", billNo, barcode, nextWorkProcessNo));
                                    else
                                        result.AllowChangeData = true;
                                }
                                else
                                    result.AllowChangeData = true;
                            }
                            else
                            {
                                sqlPro = GetTableName(TableType.CheckBarcode, checkOrderType);
                                bool isFind = AutoChangeMoreWorkstation(result, billNo, workstationId, barcode, produceData, sqlPro, ref execCode, ref workProcessNo, ref checkMaterial, ref isCheckExistLinkBarcode);
                                if (isFind)
                                    execCode = ExecCodeEnum.None;
                                else
                                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}在此工序已存在记录。", billNo, barcode));
                            }
                            break;
                        case ExecCodeEnum.NotPassInPreWP:
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}在前道工序为不通过。", billNo, barcode));
                            break;
                        case ExecCodeEnum.NotDataInPreWP:
                            sqlPro = GetTableName(TableType.CheckBarcode, checkOrderType);
                            AutoChangeWorkOrderForNotFindPreData(preBillNo, preWorkProcessNo, result, billNo, workstationId, barcode, produceData, sqlPro, ref execCode, ref workProcessNo, ref checkMaterial, ref isCheckExistLinkBarcode);
                            break;
                        case ExecCodeEnum.None:
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("{0}的检查未能执行,请检查是否出现异常。", barcode));
                            execCode = ExecCodeEnum.CheckSqlError;
                            break;
                        default:
                            if (execCode == ExecCodeEnum.Success && changeData)
                            {
                                sqlPro = GetTableName(TableType.CheckExistLinkBarcode, checkOrderType);
                                if (!isCheckExistLinkBarcode || (isCheckExistLinkBarcode && !CheckLinkBarcodeUsed(barcode, workProcessNo, isPackage, changeData, sqlPro, ref execCode)))
                                {
                                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("条码{0}在本道工序无记录无需变更", barcode));
                                    execCode = ExecCodeEnum.NotDataChange;
                                }
                            }
                            break;
                    }
                    //else if (execCode == ExecCodeEnum.Success && !changeData)
                    //{
                    //    if (CheckLinkXTBarCode(barcode, workProcessNo, billNo))
                    //    {
                    //        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("工单{0}下{1}在本道工序已绑定。", billNo, barcode));
                    //        execCode = ExecCodeEnum.NotFindMatBarcode;
                    //    }
                    //}

                }
            }
            if (!changeData && (execCode == ExecCodeEnum.None || execCode == ExecCodeEnum.Success))
            {
                if (isCheckExistLinkBarcode)
                {
                    sqlPro = GetTableName(TableType.CheckExistLinkBarcode, checkOrderType);
                    CheckLinkBarcodeUsed(barcode, workProcessNo, isPackage, changeData, sqlPro, ref execCode);
                }
                if (result.WorkstationInfo == null && checkMaterial)
                {
                    string curClassifyBarcode = CheckExistsMaterialBarcode(barcode, result, ref execCode);
                    CheckMaterialBarcode(barcode, classifyBarcode, curClassifyBarcode, result, ref execCode);
                    if (execCode == ExecCodeEnum.None || execCode == ExecCodeEnum.Success)
                        result.ClassifyBarcode = curClassifyBarcode;
                }
                else if (result.WorkstationInfo != null && result.WorkstationInfo.WorkstationConfig != null &&
                    result.WorkstationInfo.WorkstationConfig.ScanBarcode.Count > 0)
                {
                    ScanBarcode scanBarcode = result.WorkstationInfo.WorkstationConfig.ScanBarcode[0];
                    if (scanBarcode.CheckMaterial)
                    {
                        result.ClassifyBarcode = CheckExistsMaterialBarcode(barcode, result, ref execCode);
                    }
                }
            }
            if (execCode == ExecCodeEnum.None || result.AllowChangeData)
                execCode = ExecCodeEnum.Success;
            result.ExecCode = execCode;
            return result;
        }

        /// <summary>
        /// 对多个前道工序进行检测
        /// </summary>
        /// <param name="produceData">缓存派工单数据</param>
        /// <param name="workProcessNo">工序号</param>
        /// <param name="billNo">派工单号</param>
        /// <param name="barcode">条码</param>
        /// <param name="preWorkProcessNo">上道工序号</param>
        /// <param name="sqlPro">存储过程标识</param>
        /// <returns></returns>
        public object[] CheckPreWs(ProduceData produceData, int workProcessNo, string billNo, string barcode, List<int> preWorkProcessNo, string sqlPro)
        {
            object[] obj = new object[2];
            ExecCodeEnum execCode = ExecCodeEnum.Success;
            string preBillNo = string.Empty;
            foreach (int preWs in preWorkProcessNo)
            {
                Dictionary<string, object> outputData = null;
                this.DataAccess.ExecuteStoredProcedure(sqlPro, out outputData, billNo, barcode, workProcessNo, preWs, 0, string.Empty);
                execCode = (ExecCodeEnum)LibSysUtils.ToInt32(outputData["EXECCODE_VAL"]);
                preBillNo = LibSysUtils.ToString(outputData["OUTBILLNO_VAL"]);
                if (execCode != ExecCodeEnum.NotDataInPreWP)
                {
                    break;
                }
            }
            if (execCode == ExecCodeEnum.NotDataInPreWP)
            {
                GetResult(produceData, preWorkProcessNo, barcode, ref  execCode, workProcessNo, billNo, sqlPro);
                if (execCode == ExecCodeEnum.Existed)
                {
                    execCode = ExecCodeEnum.Success;
                }
            }
            obj[0] = execCode;
            obj[1] = preBillNo;
            return obj;
        }

        /// <summary>
        /// 判断是否为前道工序无记录
        /// </summary>
        /// <param name="produceData">缓存派工单数据</param>
        /// <param name="preWorkProcessNo">上道工序号</param>
        /// <param name="barcode">条码</param>
        /// <param name="execCode">返回值类型</param>
        /// <param name="workProcessNo"></param>
        /// <param name="billNo"></param>
        /// <param name="sqlPro"></param>
        private void GetResult(ProduceData produceData, List<int> preWorkProcessNo, string barcode, ref ExecCodeEnum execCode, int workProcessNo, string billNo, string sqlPro)
        {
            if (preWorkProcessNo.Count == 1)
            {
                if (preWorkProcessNo[0] == 0)
                {
                    execCode = ExecCodeEnum.Success;
                }
                else
                {
                    bool isdo = produceData.WorkProcessNo[preWorkProcessNo[0]].DoWorkProcessNo;
                    if (!isdo)
                    {
                        GetResult(produceData, produceData.WorkProcessNo[preWorkProcessNo[0]].PreWorkProcessNo, barcode, ref execCode, workProcessNo, billNo, sqlPro);
                    }
                    else
                    {
                        if (produceData.WorkProcessNo[workProcessNo].PreWorkProcessNo.Contains(preWorkProcessNo[0]))
                            return;
                        ExecCodeEnum result = ExecCodeEnum.None;
                        Dictionary<string, object> outputData = null;
                        this.DataAccess.ExecuteStoredProcedure(sqlPro, out outputData, billNo, barcode, produceData.WorkProcessNo[preWorkProcessNo[0]].NextWorkProcessNo, preWorkProcessNo[0], 0, string.Empty);
                        result = (ExecCodeEnum)LibSysUtils.ToInt32(outputData["EXECCODE_VAL"]);
                        execCode = result;
                    }
                }
            }
            else
            {
                foreach (int item in preWorkProcessNo.Where(item => !produceData.WorkProcessNo[item].DoWorkProcessNo))
                {
                    GetResult(produceData, produceData.WorkProcessNo[item].PreWorkProcessNo, barcode, ref execCode, workProcessNo, billNo, sqlPro);
                    if (execCode != ExecCodeEnum.NotDataInPreWP)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 查看电芯条码是否存在
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <param name="result">条码检测结果</param>
        /// <param name="execCode">返回值类型</param>
        /// <returns></returns>
        private string CheckExistsMaterialBarcode(string barcode, BarcodeCheckResult result, ref ExecCodeEnum execCode)
        {
            Dictionary<string, object> outputData = null;
            this.DataAccess.ExecuteStoredProcedure("wsCheckMaterialBarcode", out outputData, barcode, string.Empty);
            var classifyBarcode = LibSysUtils.ToString(outputData["CLASSIFYBARCODE_VAL"]);
            if (!string.IsNullOrEmpty(classifyBarcode)) return classifyBarcode;
            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("条码{0}在系统中找不到记录", barcode));
            execCode = ExecCodeEnum.NotFindMatBarcode;
            return classifyBarcode;
        }

        /// <summary>
        /// 检查电芯条码是否符合条件
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <param name="classifyBarcode">组码</param>
        /// <param name="curClassifyBarcode">当前组号</param>
        /// <param name="result">条码检测结果</param>
        /// <param name="execCode">返回值类型</param>
        protected virtual void CheckMaterialBarcode(string barcode, string classifyBarcode, string curClassifyBarcode, BarcodeCheckResult result, ref ExecCodeEnum execCode)
        {
            if (string.IsNullOrEmpty(curClassifyBarcode)) return;
            if (string.CompareOrdinal(classifyBarcode, curClassifyBarcode) == 0) return;
            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("条码{0}不能用于当前站点", barcode));
            execCode = ExecCodeEnum.NotUseMatBarcode;
        }

        /// <summary>
        /// 检查是否存在关联条码
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <param name="isFrom">是否有来源</param>
        /// <param name="billNo">派工单号</param>
        /// <param name="workProcessNo">工序号</param>
        /// <param name="workstationId">站点编号</param>
        /// <returns></returns>
        public bool CheckExistLinkBarcode(string barcode, bool isFrom, string billNo, int workProcessNo, string workstationId)
        {
            //检查条码是否已经使用过
            bool exist = false;
            if (isFrom)
            {
                ProduceData produceData = LibProduceCache.Default.GetProduceData(billNo);
                List<int> preWorkProcessNo = produceData.WorkProcessNo[workProcessNo].PreWorkProcessNo;
                BarcodeCheckResult bc = CheckBarcodeData(workstationId, barcode, workProcessNo, billNo);
                if (bc.ExecCode != ExecCodeEnum.Success)
                {
                    exist = true;
                }
            }
            else
            {
                ProduceData produce = LibProduceCache.Default.GetProduceData(billNo);
                DataSet ds = produce.WorkOrder;
                int checkOrderType = LibSysUtils.ToInt32(ds.Tables[0].Rows[0]["ISFROMPLAN"]);
                var sqlPro = GetTableName(TableType.CheckExistBarcode, checkOrderType);
                Dictionary<string, object> outputData = null;
                this.DataAccess.ExecuteStoredProcedure(sqlPro, out outputData, barcode, false);
                exist = LibSysUtils.ToBoolean(outputData["EXIST_VAL"]);
            }
            return exist;
        }

        /// <summary>
        /// 获取批次打印信息
        /// </summary>
        /// <param name="configId">站点配置编号</param>
        /// <param name="billNo">派工单号</param>
        /// <returns></returns>
        public BatchPrintInfo GetBatchPrintInfo(string configId, string billNo)
        {
            if (string.IsNullOrEmpty(billNo))
                throw new Exception("必须先选择一个工单。");
            BatchPrintInfo info = new BatchPrintInfo { LabelTemplateData = GetLabelTemplateInfo(billNo) };
            HashSet<string> hashSet = new HashSet<string>();
            SqlBuilder sqlBuilder = new SqlBuilder("com.WorkstationConfig");
            string sql = sqlBuilder.GetQuerySql(1, "B.BARCODERULEID,B.BARCODERULENAME,B.ISMASTER", string.Format("B.WORKSTATIONCONFIGID={0}", LibStringBuilder.GetQuotString(configId)), "B.ROWNO ASC");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string barcodeRuleId = LibSysUtils.ToString(reader["BARCODERULEID"]);
                    if (hashSet.Contains(barcodeRuleId)) continue;
                    info.BarcodeRuleData.Add(new BarcodeRuleInfo() { BarcodeRuleId = barcodeRuleId, BarcodeRuleName = LibSysUtils.ToString(reader["BARCODERULENAME"]), IsMaster = LibSysUtils.ToBoolean(reader["ISMASTER"]) });
                    hashSet.Add(barcodeRuleId);
                }
            }
            sql = sqlBuilder.GetQuerySql(2, "C.BARCODERULEID,C.BARCODERULENAME,C.ISMASTER",
                string.Format("C.WORKSTATIONCONFIGID={0}", LibStringBuilder.GetQuotString(configId)), "C.ROWNO ASC");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string barcodeRuleId = LibSysUtils.ToString(reader["BARCODERULEID"]);
                    if (hashSet.Contains(barcodeRuleId)) continue;
                    info.BarcodeRuleData.Add(new BarcodeRuleInfo() { BarcodeRuleId = barcodeRuleId, BarcodeRuleName = LibSysUtils.ToString(reader["BARCODERULENAME"]), IsMaster = LibSysUtils.ToBoolean(reader["ISMASTER"]) });
                    hashSet.Add(barcodeRuleId);
                }
            }
            return info;
        }

        /// <summary>
        /// 获取打印信息
        /// </summary>
        /// <param name="billNo">派工单号</param>
        /// <returns></returns>
        public List<LabelTemplateInfo> GetLabelTemplateInfo(string billNo)
        {
            if (string.IsNullOrEmpty(billNo))
                throw new Exception("必须先选择一个工单。");
            List<LabelTemplateInfo> list = new List<LabelTemplateInfo>();
            ProduceData produceData = LibProduceCache.Default.GetProduceData(billNo);
            DataRow dataRow = produceData.WorkOrder.Tables[0].Rows[0];
            for (int i = 1; i <= 8; i++)
            {
                string id = string.Format("LABELTEMPLATEID{0}", i);
                if (string.IsNullOrEmpty(LibSysUtils.ToString(dataRow[id]).Trim()))
                    continue;
                string name = string.Format("LABELTEMPLATENAME{0}", i);
                list.Add(new LabelTemplateInfo() { LabelTemplateId = LibSysUtils.ToString(dataRow[id]), LabelTemplateName = LibSysUtils.ToString(dataRow[name]) });
            }
            return list;
        }

        /// <summary>
        /// 获取批次条码
        /// </summary>
        /// <param name="barcodeRuleId">条码规则编号</param>
        /// <param name="billNo">派工单号</param>
        /// <param name="printNum">打印数量</param>
        /// <param name="isMaster">是否传递码</param>
        /// <returns></returns>
        public IList<string> GetBatchBarcode(string barcodeRuleId, string billNo, int printNum, bool isMaster)
        {
            IList<string> list = new List<string>();
            ProduceData produce = LibProduceCache.Default.GetProduceData(billNo);
            DataSet ds = produce.WorkOrder;
            int checkOrderType = LibSysUtils.ToInt32(ds.Tables[0].Rows[0]["ISFROMPLAN"]);
            string progId = isMaster ? GetTableName(TableType.MainProgId, checkOrderType) : GetTableName(TableType.LinkProgId, checkOrderType);
            string fieldName = isMaster ? "BARCODE" : "LINKBARCODE";
            ProduceData produceData = LibProduceCache.Default.GetProduceData(billNo);
            list = LibBarcodeServer.Default.GetBatchBarcode(progId, fieldName, barcodeRuleId, new List<DataRow>() { produceData.WorkOrder.Tables[0].Rows[0] }, printNum);
            return list;
        }

        /// <summary>
        /// 获取打印的条码信息
        /// </summary>
        /// <param name="needFindLinkBarcode">是否需要查找关联条码</param>
        /// <param name="barcode">条码</param>
        /// <param name="labelTemplateId">条码模板编号</param>
        /// <param name="barcodeTypeId">条码类型编号</param>
        /// <param name="barcodeRuleId">条码规则编号</param>
        /// <param name="autoBuild">是否自动生成</param>
        /// <param name="isMaster">是否传递码</param>
        /// <param name="billNo">派工单号</param>
        /// <returns></returns>
        public PrintBarcodeInfo GetPrintBarcodeInfo(bool needFindLinkBarcode, string barcode, string labelTemplateId, string barcodeTypeId, string barcodeRuleId, bool autoBuild, bool isMaster, string billNo)
        {
            ProduceData produce = LibProduceCache.Default.GetProduceData(billNo);
            DataSet ds = produce.WorkOrder;
            int checkOrderType = LibSysUtils.ToInt32(ds.Tables[0].Rows[0]["ISFROMPLAN"]);
            //取打印模板 需要查找关联条码的取到条码 如果需要生成条码的要产生条码
            PrintBarcodeInfo info = new PrintBarcodeInfo();
            if (needFindLinkBarcode)
            {
                if (autoBuild)
                {
                    string progId = isMaster ? GetTableName(TableType.MainProgId, checkOrderType) : GetTableName(TableType.LinkProgId, checkOrderType);
                    string fieldName = isMaster ? "BARCODE" : "LINKBARCODE";
                    ProduceData produceData = LibProduceCache.Default.GetProduceData(billNo);
                    int serialLen = 0;
                    info.Barcode = LibBarcodeServer.Default.GetBarcode(progId, fieldName, barcodeRuleId, new List<DataRow>() { produceData.WorkOrder.Tables[0].Rows[0] }, ref serialLen);
                    info.SerialLen = serialLen;
                }
                else
                {
                    Dictionary<string, object> outputData = null;
                    var sqlPro = GetTableName(TableType.SelectLinkBarcode, checkOrderType);
                    this.DataAccess.ExecuteStoredProcedure(sqlPro, out outputData, barcode, barcodeTypeId, string.Empty);
                    info.Barcode = LibSysUtils.ToString(outputData["LINKBARCODE_VAL"]);
                }

            }
            if (!string.IsNullOrEmpty(labelTemplateId))
            {
                info.LabelTemplateJs = GetLabelTemplateJs(labelTemplateId, billNo);
            }
            return info;
        }

        /// <summary>
        /// 获取需要打印的特殊条码的信息
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <param name="barcodeRuleId">条码规则编号</param>
        /// <param name="billNo">派工单号</param>
        /// <returns></returns>
        public PrintSpecialBarcodeInfo GetSpecialPrintBarcodeInfo(string barcode, string barcodeRuleId, string billNo)
        {
            PrintSpecialBarcodeInfo info = new PrintSpecialBarcodeInfo();
            int packageLen = 0;
            ProduceData produceData = LibProduceCache.Default.GetProduceData(billNo);
            BarcodeRule barcodeRule = LibBarcodeRuleCache.Default.GetBarcodeRule(barcodeRuleId);
            info.SerialLen = GetPrefix(barcodeRule);
            string sql = string.Format("select count(packagebarcode) from WSPACKAGERECORD where PACKAGEBARCODE = '{0}'", barcode);
            packageLen = LibSysUtils.ToInt32(this.DataAccess.ExecuteScalar(sql));
            info.PackageLen = packageLen;
            return info;
        }

        /// <summary>
        /// 获取流水号长度
        /// </summary>
        /// <param name="barcodeRule">条码规则信息</param>
        /// <returns></returns>
        private int GetPrefix(BarcodeRule barcodeRule)
        {
            int serialLen = 0;
            foreach (BarcodeRuleItem item in barcodeRule.Items)
            {
                switch (item.SectionType)
                {
                    case BarcodeRuleSectionType.SerialNum:
                        serialLen = item.Length;
                        break;
                    default:
                        break;
                }
            }
            return serialLen;
        }

        /// <summary>
        /// 读取打印模板
        /// </summary>
        /// <param name="name">条码模板编号</param>
        /// <returns></returns>
        private string ReadPrintTemplateTxt(string name)
        {
            string labelTemplateJs = string.Empty;
            string path = Path.Combine(EnvProvider.Default.MainPath, "Resource", "PrintTpl", string.Format("{0}.txt", name));
            if (!File.Exists(path)) return labelTemplateJs;
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    labelTemplateJs = sr.ReadToEnd();
                }
            }
            return labelTemplateJs;
        }

        /// <summary>
        /// 获取打印模板信息
        /// </summary>
        /// <param name="labelTemplateId">条码模板编号</param>
        /// <param name="billNo">派工单</param>
        /// <returns></returns>
        public string GetLabelTemplateJs(string labelTemplateId, string billNo)
        {
            string labelTemplateJs = ReadPrintTemplateTxt(labelTemplateId);
            List<LabelTemplateRule> list = new List<LabelTemplateRule>();
            SqlBuilder sqlBuilder = new SqlBuilder("com.LabelTemplate");
            StringBuilder builder = new StringBuilder();
            StringBuilder selectBuilder = new StringBuilder();
            builder.Append(sqlBuilder.GetQuerySql(1, "B.LTPARAMTYPE,B.LTPARAMNAME,B.LTPARAMVALUE,B.FIELDNAME", string.Format("B.LABELTEMPLATEID = {0}", LibStringBuilder.GetQuotString(labelTemplateId))));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(builder.ToString()))
            {
                while (reader.Read())
                {
                    LabelTemplateRule rule = new LabelTemplateRule()
                    {
                        LtParamType = (LtParamType)LibSysUtils.ToInt32(reader["LTPARAMTYPE"]),
                        LtParamName = LibSysUtils.ToString(reader["LTPARAMNAME"]),
                        LtParamValue = LibSysUtils.ToString(reader["LTPARAMVALUE"]),
                        FieldName = LibSysUtils.ToString(reader["FIELDNAME"])
                    };
                    list.Add(rule);
                    if (rule.LtParamType == LtParamType.Field)
                    {
                        selectBuilder.AppendFormat("A.{0},", rule.FieldName);
                    }
                }
            }
            Dictionary<string, object> workOrderField = new Dictionary<string, object>();
            if (selectBuilder.Length > 0)
            {
                selectBuilder.Remove(selectBuilder.Length - 1, 1);
                sqlBuilder = new SqlBuilder("pp.WorkOrder");
                string sql = sqlBuilder.GetQuerySql(0, selectBuilder.ToString(), string.Format("A.BILLNO={0}", LibStringBuilder.GetQuotString(billNo)));
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            workOrderField.Add(reader.GetName(i), reader[i]);
                        }
                    }
                }
            }

            foreach (LabelTemplateRule item in list)
            {
                switch (item.LtParamType)
                {
                    case LtParamType.Image:
                        labelTemplateJs = labelTemplateJs.Replace(item.LtParamName, string.Format("<img src='../Content/images/{0}'/>", item.LtParamValue));
                        break;
                    case LtParamType.Field:
                        labelTemplateJs = labelTemplateJs.Replace(item.LtParamName, string.Format("{0}", workOrderField[item.FieldName]));
                        break;
                    case LtParamType.Date:
                        DateTime dateTime = DateTime.Now;
                        string dateStr = (dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day).ToString();
                        labelTemplateJs = labelTemplateJs.Replace(item.LtParamName, dateStr);
                        break;
                }
            }
            return labelTemplateJs;
        }

        /// <summary>
        /// 检验数量
        /// </summary>
        /// <param name="isPurCheck">是否采购质检</param>
        /// <param name="checkNum">检验数量</param>
        /// <param name="qualifiedNum">合格数量</param>
        /// <param name="unQualifiedNum">不合格数量</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="dealWay">处理方式</param>
        /// <param name="billNo">派工单号</param>
        /// <param name="rowId">行号</param>
        /// <returns></returns>
        public bool CheckNum(int isPurCheck, decimal checkNum, int qualifiedNum, int unQualifiedNum, long startTime, long endTime, int dealWay, string billNo, int rowId)
        {
            bool result = true;
            List<string> lists = new List<string>();
            StringBuilder sqlBuilder = new StringBuilder();
            string sql;
            if (isPurCheck == 2)
            {
                string isHaveStartTime = string.Format(@"SELECT CHECKSTARTTIME FROM PURSUPPLYARRIVALINFO
                                    WHERE FROMBILLNO = (select FROMBILLNO from STKDELIVERYNOTEDETAIL 
                                                        WHERE BILLNO =(select FROMBILLNO from PURQUALITYCHECKDETAIL  where BILLNO={0} AND ROW_ID={1}) AND
                                                        ROW_ID=(select FROMROWID from PURQUALITYCHECKDETAIL  where BILLNO={0} AND ROW_ID={1}))
                                    AND FROMROWID =(select FROMROWID from STKDELIVERYNOTEDETAIL 
                                                    WHERE BILLNO =(select FROMBILLNO from PURQUALITYCHECKDETAIL  where BILLNO={0} AND ROW_ID={1}) AND
                                                    ROW_ID=(select FROMROWID from PURQUALITYCHECKDETAIL  where BILLNO={0} AND ROW_ID={1}))", LibStringBuilder.GetQuotObject(billNo), rowId);
                long existStartTime = LibSysUtils.ToInt64(this.DataAccess.ExecuteScalar(isHaveStartTime));
                string billExsitStartTime = string.Format(@"SELECT STARTTIME FROM PURQUALITYCHECKDETAIL WHERE BILLNO={0} AND ROW_ID={1}", LibStringBuilder.GetQuotObject(billNo), rowId);
                long billStartTime = LibSysUtils.ToInt64(this.DataAccess.ExecuteScalar(billExsitStartTime));
                string informationsql;
                if (checkNum == qualifiedNum + unQualifiedNum)
                {
                    if (billStartTime == 0)
                    {
                        sql = "UPDATE PURQUALITYCHECKDETAIL SET ";
                        sqlBuilder.AppendFormat("QUALIFIEDNUM={0},UNQUALIFIEDNUM={1},CHECKNUM={8},QUALIFIEDRATE={7},DEALWAY={2},STARTTIME={3},ENDTIME={4},ISFINISHED=1 WHERE BILLNO={5} AND ROW_ID={6}", qualifiedNum, unQualifiedNum, dealWay, startTime, endTime, LibStringBuilder.GetQuotObject(billNo), rowId, Math.Round(qualifiedNum / checkNum, 4), checkNum);
                    }
                    else
                    {
                        sql = "UPDATE PURQUALITYCHECKDETAIL SET ";
                        sqlBuilder.AppendFormat("CHECKNUM={0},QUALIFIEDNUM={1},UNQUALIFIEDNUM={2},DEALWAY={3},ENDTIME={4},QUALIFIEDRATE={5},ISFINISHED=1 WHERE BILLNO={6} AND ROW_ID={7}", checkNum, qualifiedNum, unQualifiedNum, dealWay, endTime, Math.Round(qualifiedNum / checkNum, 4), LibStringBuilder.GetQuotObject(billNo), rowId);
                    }
                    if (existStartTime == 0)
                    {
                        informationsql = string.Format(@"UPDATE PURSUPPLYARRIVALINFO SET CHECKSTARTTIME={0},
                        CHECKENDTIME={1},CHECKGOODQTY=RECEIVEQTY-CHECKBADQTY-{2},CHECKBADQTY={2} 
                        WHERE FROMBILLNO = (select FROMBILLNO from STKDELIVERYNOTEDETAIL  where BILLNO={3} AND ROW_ID={4}) 
                        AND FROMROWID = (select FROMROWID from STKDELIVERYNOTEDETAIL  where BILLNO={3} AND ROW_ID={4})"
                            , startTime, endTime, unQualifiedNum, LibStringBuilder.GetQuotObject(billNo), rowId);
                        lists.Add(informationsql);
                    }
                    else
                    {
                        informationsql = string.Format(@"UPDATE PURSUPPLYARRIVALINFO SET CHECKENDTIME={0},CHECKGOODQTY=RECEIVEQTY-CHECKBADQTY-{1},CHECKBADQTY=CHECKBADQTY+{1} 
                        WHERE FROMBILLNO = (select FROMBILLNO from STKDELIVERYNOTEDETAIL 
                                            WHERE BILLNO =(select FROMBILLNO from PURQUALITYCHECKDETAIL  where BILLNO={2} AND ROW_ID={3}) AND
                                            ROW_ID=(select FROMROWID from PURQUALITYCHECKDETAIL  where BILLNO={2} AND ROW_ID={3}))
                        AND FROMROWID =(select FROMROWID from STKDELIVERYNOTEDETAIL 
                                        WHERE BILLNO =(select FROMBILLNO from PURQUALITYCHECKDETAIL  where BILLNO={2} AND ROW_ID={3}) AND
                                        ROW_ID=(select FROMROWID from PURQUALITYCHECKDETAIL  where BILLNO={2} AND ROW_ID={3}))",
                            endTime, unQualifiedNum, LibStringBuilder.GetQuotObject(billNo), rowId);
                        lists.Add(informationsql);
                    }
                }
                else
                {
                    if (billStartTime == 0)
                    {
                        sql = "UPDATE PURQUALITYCHECKDETAIL SET ";
                        sqlBuilder.AppendFormat("CHECKNUM={0},STARTTIME={1} WHERE BILLNO={2} AND ROW_ID={3}", checkNum, startTime, LibStringBuilder.GetQuotObject(billNo), rowId);
                    }
                    else
                    {
                        sql = "UPDATE PURQUALITYCHECKDETAIL SET ";
                        sqlBuilder.AppendFormat("CHECKNUM={0} WHERE BILLNO={1} AND ROW_ID={2}", checkNum, LibStringBuilder.GetQuotObject(billNo), rowId);
                    }
                    if (existStartTime == 0)
                    {
                        informationsql = string.Format(@"UPDATE PURSUPPLYARRIVALINFO SET CHECKSTARTTIME={0}
                        WHERE FROMBILLNO = (select FROMBILLNO from STKDELIVERYNOTEDETAIL 
                                            WHERE BILLNO =(select FROMBILLNO from PURQUALITYCHECKDETAIL  where BILLNO={1} AND ROW_ID={2}) AND
                                            ROW_ID=(select FROMROWID from PURQUALITYCHECKDETAIL  where BILLNO={1} AND ROW_ID={2}))
                        AND FROMROWID =(select FROMROWID from STKDELIVERYNOTEDETAIL 
                                        WHERE BILLNO =(select FROMBILLNO from PURQUALITYCHECKDETAIL  where BILLNO={1} AND ROW_ID={2}) AND
                                        ROW_ID=(select FROMROWID from PURQUALITYCHECKDETAIL  where BILLNO={1} AND ROW_ID={2}))"
                            , startTime, LibStringBuilder.GetQuotObject(billNo), rowId);
                        lists.Add(informationsql);
                    }
                }
            }
            else
            {
                sql = string.Format("UPDATE OWQUALITYCHECKDETAIL SET ");
                sqlBuilder.AppendFormat("QUALIFIEDNUM={0},UNQUALIFIEDNUM={1},DEALWAY={2},STARTTIME={3},ENDTIME={4},ISFINISHED=1 WHERE BILLNO={5} AND ROW_ID={6}", qualifiedNum, unQualifiedNum, dealWay, startTime, endTime, LibStringBuilder.GetQuotObject(billNo), rowId);
            }
            sql += sqlBuilder;
            lists.Add(sql);
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                this.DataAccess.ExecuteNonQuery(lists, false);
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 获取服务器时间
        /// </summary>
        /// <param name="sort">时间格式</param>
        /// <returns></returns>
        public string GetTime(int sort)
        {
            string time = string.Empty;
            switch (sort)
            {
                case 1:
                    time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                case 2:
                    time = DateTime.Now.ToString("yyyyMMddHHmmss");
                    break;
                case 3:
                    time = DateTime.Now.ToString("yyyy-MM-dd");
                    break;
                case 4:
                    time = DateTime.Now.ToString("yyyyMMdd");
                    break;
                case 5:
                    time = DateTime.Now.ToString("HH:mm:ss");
                    break;
                case 6:
                    time = DateTime.Now.ToString("HHmmss");
                    break;
            }
            return time;
        }

        /// <summary>
        /// 获取缺陷
        /// </summary>
        /// <param name="workstationConfigId">站点配置编号</param>
        /// <returns></returns>
        public List<Badness> GetBadnessSetting(string workstationConfigId)
        {
            List<Badness> list = new List<Badness>();
            string sql = string.Format(@"SELECT DISTINCT
                                            C.DEFECTID ,
                                            D.DEFECTNAME
                                            FROM    COMWSCONFIGCSDETAIL A
                                            LEFT JOIN CHECKSOLUTIONDETAIL B ON A.CHECKSTID = B.CHECKSTID
                                            LEFT JOIN CHECKITEMDETAIL C ON B.CHECKITEMID = C.CHECKITEMID
                                            LEFT JOIN COMDEFECT D ON C.DEFECTID = D.DEFECTID
                                            WHERE   A.WORKSTATIONCONFIGID = '{0}'", workstationConfigId);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    list.Add(new Badness(LibSysUtils.ToString(reader["DEFECTID"]), LibSysUtils.ToString(reader["DEFECTNAME"])));
                }
            }
            return list;
        }

        /// <summary>
        /// 获取存储过程名称
        /// </summary>
        /// <param name="tableType">操作类型</param>
        /// <param name="checkOrderType">质检单类型</param>
        /// <returns></returns>
        public string GetTableName(TableType tableType, int checkOrderType)
        {
            string tableName = string.Empty;
            switch (tableType)
            {
                case TableType.DeleteBarcode:
                    tableName = checkOrderType == 2 ? "wsIQCDeleteBarcodeData" : checkOrderType == 3 ? "wsOQCDeleteBarcodeData" : "wsDeleteBarcodeData";
                    break;
                case TableType.DeleteLinkBarcode:
                    tableName = checkOrderType == 2 ? "wsIQCDeleteLinkBarcodeData" : checkOrderType == 3 ? "wsOQCDeleteLinkBarcodeData" : "wsDeleteLinkBarcodeData";
                    break;
                case TableType.WriteBarcode:
                    tableName = checkOrderType == 2 ? "wsIQCWriteBarcodeData" : checkOrderType == 3 ? "wsOQCWriteBarcodeData" : "wsWriteBarcodeData";
                    break;
                case TableType.WriteCheckItem:
                    tableName = checkOrderType == 2 ? "wsIQCWriteCheckItemData" : checkOrderType == 3 ? "wsOQCWriteCheckItemData" : "wsWriteCheckItemData";
                    break;
                case TableType.WriteLinkBarcode:
                    tableName = checkOrderType == 2 ? "wsIQCWriteLinkBarcode" : checkOrderType == 3 ? "wsOQCWriteLinkBarcode" : "wsWriteLinkBarcode";
                    break;
                case TableType.WriteBarcodeBad:
                    tableName = checkOrderType == 2 ? "wsIQCWriteBarcodeBadData" : checkOrderType == 3 ? "wsOQCWriteBarcodeBadData" : "wsWriteBarcodeBadData";
                    break;
                case TableType.FindRealBarcode:
                    tableName = checkOrderType == 2 ? "wsIQCFindRealBarcode" : checkOrderType == 3 ? "wsOQCFindRealBarcode" : "wsFindRealBarcode";
                    break;
                case TableType.CheckBarcode:
                    tableName = checkOrderType == 2 ? "wsIQCCheckBarcode" : checkOrderType == 3 ? "wsOQCCheckBarcode" : "wsCheckBarcode";
                    break;
                case TableType.CheckWPNextExist:
                    tableName = checkOrderType == 2 ? "wsIQCCheckWPNextExistBarcode" : checkOrderType == 3 ? "wsOQCCheckWPNextExistBarcode" : "wsCheckWPNextExistBarcode";
                    break;
                case TableType.CheckExistLinkBarcode:
                    tableName = checkOrderType == 2 ? "wsIQCCheckExistLinkBarcode" : checkOrderType == 3 ? "wsOQCCheckExistLinkBarcode" : "wsCheckExistLinkBarcode";
                    break;
                case TableType.CheckExistBarcode:
                    tableName = checkOrderType == 2 ? "wsIQCCheckExistBarcode" : checkOrderType == 3 ? "wsOQCCheckExistBarcode" : "wsCheckExistBarcode";
                    break;
                case TableType.SelectLinkBarcode:
                    tableName = checkOrderType == 2 ? "wsIQCSelectLinkBarcode" : checkOrderType == 3 ? "wsOQCSelectLinkBarcode" : "wsSelectLinkBarcode";
                    break;
                case TableType.CheckBarcodeCK:
                    tableName = checkOrderType == 2 ? "wsIQCCheckBarcodeCK" : "wsOQCCheckBarcodeCK";
                    break;
                case TableType.MainProgId:
                    tableName = checkOrderType == 2 ? "ws.IQCBarCode" : checkOrderType == 3 ? "ws.OQCBarCode" : "ws.Barcode";
                    break;
                case TableType.LinkProgId:
                    tableName = checkOrderType == 2 ? "ws.IQCLinkBarcode" : checkOrderType == 3 ? "ws.OQCLinkBarcode" : "ws.LinkBarcode";
                    break;
                default:
                    break;
            }
            return tableName;
        }
        #endregion

        #region[仓库]
        public STKLoginInfo STKLogin(string UserId, string UserPassWord)
        {
            if (string.IsNullOrEmpty(UserId))
                throw new ArgumentNullException("用户账号");
            if (string.IsNullOrEmpty(UserPassWord))
                throw new ArgumentNullException("用户密码");
            STKLoginInfo info = new STKLoginInfo();
            SqlBuilder sqlBuilder = new SqlBuilder("axp.User");
            string sql = sqlBuilder.GetQuerySql(0, "A.PERSONID,A.PERSONNAME", string.Format("A.USERID={0} AND A.USERPASSWORD = {1} AND A.ISUSE=1", LibStringBuilder.GetQuotString(UserId), LibStringBuilder.GetQuotString(UserPassWord)));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                if (reader.Read())
                {
                    info.PersonId = LibSysUtils.ToString(reader[0]);
                    info.PersonName = LibSysUtils.ToString(reader[1]);
                }
            }
            if (string.IsNullOrEmpty(info.PersonId))
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "人员账号或密码错误");
            return info;
        }

        public STKInWareInfoList GetSTKInWareInfo(string billno)
        {
            STKInWareInfoList stkinwarelist = new STKInWareInfoList();
            SqlBuilder sqlbuilder = new SqlBuilder("stk.InWare");
            string sql = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(billno))
                {
                    sql = sqlbuilder.GetQuerySql(1, "B.ROW_ID,B.SALBILLNO,B.FROMBILLNO,B.BILLNO,B.CUSTOMERID,B.CUSTOMERNAME,B.STORAGEID,B.STORAGENAME,B.LIBRARYID,B.LIBRARYNAME,B.MATERIALID,B.MATERIALNAME,B.WORKPROCESSNO,B.WORKPROCESSID,B.WORKPROCESSNAME,B.WORKSHOPSECTIONID,B.WORKSHOPSECTIONNAME", string.Format(" B.BILLNO = '{0}'", billno));
                    using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            STKInWareInfo stkinwareinfo = new STKInWareInfo();
                            stkinwareinfo.Row_Id = LibSysUtils.ToInt32(reader["ROW_ID"]);
                            stkinwareinfo.SalBillNo = LibSysUtils.ToString(reader["SALBILLNO"]);
                            stkinwareinfo.WorkOrderNo = LibSysUtils.ToString(reader["FROMBILLNO"]);
                            stkinwareinfo.BillNo = LibSysUtils.ToString(reader["BILLNO"]);
                            stkinwareinfo.CustomerId = LibSysUtils.ToString(reader["CUSTOMERID"]);
                            stkinwareinfo.CustomerName = LibSysUtils.ToString(reader["CUSTOMERNAME"]);
                            stkinwareinfo.StorageId = LibSysUtils.ToString(reader["STORAGEID"]);
                            stkinwareinfo.StorageName = LibSysUtils.ToString(reader["STORAGENAME"]);
                            stkinwareinfo.LibraryId = LibSysUtils.ToString(reader["LIBRARYID"]);
                            stkinwareinfo.LibraryName = LibSysUtils.ToString(reader["LIBRARYNAME"]);
                            stkinwareinfo.MaterialId = LibSysUtils.ToString(reader["MATERIALID"]);
                            stkinwareinfo.MaterialName = LibSysUtils.ToString(reader["MATERIALNAME"]);
                            stkinwareinfo.WorkProcessNo = LibSysUtils.ToString(reader["WORKPROCESSNO"]);
                            stkinwareinfo.WorkProcessId = LibSysUtils.ToString(reader["WORKPROCESSID"]);
                            stkinwareinfo.WorkProcessName = LibSysUtils.ToString(reader["WORKPROCESSNAME"]);
                            stkinwareinfo.WorkShopSectionId = LibSysUtils.ToString(reader["WORKSHOPSECTIONID"]);
                            stkinwareinfo.WorkShopSectionName = LibSysUtils.ToString(reader["WORKSHOPSECTIONNAME"]);
                            stkinwarelist.stkInWareInfoList.Add(stkinwareinfo);
                        }
                    }
                }
                else
                {
                    sql = sqlbuilder.GetQuerySql(0, "A.BILLNO,A.BILLDATE,A.TYPENAME", string.Format(" A.CURRENTSTATE=2 "));
                    using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            STKInWareMain stkinwaremain = new STKInWareMain();
                            stkinwaremain.BillNo = LibSysUtils.ToString(reader["BILLNO"]);
                            stkinwaremain.BillType = LibSysUtils.ToString(reader["TYPENAME"]);
                            stkinwaremain.BillDate = LibSysUtils.ToString(reader["BILLDATE"]);
                            stkinwarelist.stkInWareMainList.Add(stkinwaremain);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, e.ToString());
            }
            return stkinwarelist;
        }
        public STKCheckResult STKCheckBarCode(string billno, string barcode, string workorderno, int other, string StoreProduceName)
        {
            STKCheckResult stkCheck = new STKCheckResult();
            try
            {
                Dictionary<string, object> outputData = null;
                this.DataAccess.ExecuteStoredProcedure(StoreProduceName, out outputData, billno, workorderno, barcode, other, 0, string.Empty, string.Empty);
                STKBarCodeCheckResult result = (STKBarCodeCheckResult)LibSysUtils.ToInt32(outputData["EXECCODE_VAL"]);
                stkCheck = new STKCheckResult();
                stkCheck.stkCheck = result;
                stkCheck.stkMainInfo.BarCodeTypeId = LibSysUtils.ToString(outputData["EXECBARCODETYPEID"]);
                stkCheck.stkMainInfo.BarCodeTypeName = LibSysUtils.ToString(outputData["EXECBARCODETYPENAME"]);
            }
            catch (Exception e)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, e.ToString());
            }
            return stkCheck;
        }

        public int STKPost(object obj, STKCheckResult stkbarcodecheckresult, string barcode, STKBillType billType, string PersonId)
        {
            int IsPack = 0;
            int result = -1;
            Dictionary<string, object> outputData = null;
            try
            {
                switch (billType)
                {
                    case STKBillType.InWare:
                        STKInWareInfo objInWare = (STKInWareInfo)obj;
                        switch (stkbarcodecheckresult.stkCheck)
                        {
                            case STKBarCodeCheckResult.PassIsPack:
                                IsPack = 1; break;
                            case STKBarCodeCheckResult.PassNotPack:
                                IsPack = 0;
                                break;
                            default:
                                break;
                        }

                        this.DataAccess.ExecuteStoredProcedure("stkInWareInsert", objInWare.BillNo, objInWare.Row_Id, objInWare.MaterialId, objInWare.StorageId, LibSysUtils.ToString(objInWare.LibraryId), barcode, objInWare.WorkOrderNo, objInWare.SalBillNo, objInWare.CustomerId, IsPack, LibSysUtils.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss")), 0, 0, PersonId);
                        result = 1;
                        break;
                    case STKBillType.OutWare:
                        STKOutWareInfo objOutWare = (STKOutWareInfo)obj;
                        this.DataAccess.ExecuteStoredProcedure("stkOutWareUpdate", objOutWare.BillNo, objOutWare.Row_Id, barcode, LibSysUtils.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss")), PersonId);
                        result = 1;
                        break;
                    case STKBillType.SplitBox:
                        STKOutWareInfo objSplitBox = (STKOutWareInfo)obj;
                        this.DataAccess.ExecuteStoredProcedure("stkSplitBoxUpdate", objSplitBox.BillNo, barcode, LibSysUtils.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss")), PersonId);
                        result = 1;
                        break;
                    case STKBillType.TurnOrder:
                        string postbarcode = string.Empty;
                        postbarcode = barcode;
                        Dictionary<string, object> objTurnOrder = JsonConvert.DeserializeObject<Dictionary<string, object>>(obj.ToString());

                        if (LibSysUtils.ToInt32(objTurnOrder["IsPackage"]) == 1)
                        {
                            postbarcode = LibSysUtils.ToString(objTurnOrder["PackageBarcode"]);
                        }

                        this.DataAccess.ExecuteStoredProcedure("stkTurnOrderUpdate", out outputData, barcode, postbarcode, LibSysUtils.ToString(objTurnOrder["CustomerID"]), LibSysUtils.ToString(objTurnOrder["InSaleBillNo"]), LibSysUtils.ToString(objTurnOrder["StorageId"]), LibSysUtils.ToString(objTurnOrder["LibraryId"]));
                        result = 1;
                        break;
                    default: break;
                }
            }
            catch (Exception e)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, e.ToString());
            }

            return result;
        }
        //public STKCheckResult STKCheckBarCode(string barcode, string workorderno, string StoreProduceName)
        //{
        //    STKCheckResult stkCheck = new STKCheckResult();
        //    try
        //    {
        //        Dictionary<string, object> outputData = null;
        //        this.DataAccess.ExecuteStoredProcedure(StoreProduceName, out outputData, workorderno, barcode, 0, string.Empty, string.Empty);
        //        STKBarCodeCheckResult result = (STKBarCodeCheckResult)LibSysUtils.ToInt32(outputData["EXECCODE_VAL"]);
        //        stkCheck = new STKCheckResult();
        //        stkCheck.stkCheck = result;
        //        stkCheck.stkMainInfo.BarCodeTypeId = LibSysUtils.ToString(outputData["EXECBARCODETYPEID"]);
        //        stkCheck.stkMainInfo.BarCodeTypeName = LibSysUtils.ToString(outputData["EXECBARCODETYPENAME"]);
        //    }
        //    catch (Exception e)
        //    {
        //        this.ManagerMessage.AddMessage(LibMessageKind.Error, e.ToString());
        //    }
        //    return stkCheck;
        //}

        //public int STKPost(object obj, STKCheckResult stkbarcodecheckresult, string barcode, STKBillType billType)
        //{
        //    int IsPack = 0;
        //    int result = -1;
        //    Dictionary<string, object> outputData = null;
        //    try
        //    {
        //        switch (billType)
        //        {
        //            case STKBillType.InWare:
        //                STKInWareInfo objInWare = (STKInWareInfo)obj;
        //                switch (stkbarcodecheckresult.stkCheck)
        //                {
        //                    case STKBarCodeCheckResult.PassIsPack:
        //                        IsPack = 1; break;
        //                    case STKBarCodeCheckResult.PassNotPack:
        //                        IsPack = 0;
        //                        break;
        //                    default:
        //                        break;
        //                }

        //                this.DataAccess.ExecuteStoredProcedure("stkInWareInsert", objInWare.BillNo, objInWare.Row_Id, objInWare.MaterialId, objInWare.StorageId, objInWare.LibraryId, barcode, objInWare.WorkOrderNo, objInWare.SalBillNo, objInWare.CustomerId, IsPack, LibSysUtils.ToInt64(DateTime.Now.ToString("yyMMddHHmmss")), 0, 0);
        //                result = 1;
        //                break;
        //            case STKBillType.OutWare:
        //                STKOutWareInfo objOutWare = (STKOutWareInfo)obj;
        //                this.DataAccess.ExecuteStoredProcedure("stkOutWareUpdate", objOutWare.BillNo, barcode, LibSysUtils.ToInt64(DateTime.Now.ToString("yyMMddHHmmss")));
        //                result = 1;
        //                break;
        //            case STKBillType.SplitBox:
        //                STKSplitBox objSplitBox = (STKSplitBox)obj;
        //                this.DataAccess.ExecuteStoredProcedure("stkSplitBoxUpdate", objSplitBox.BillNo, barcode, LibSysUtils.ToInt64(DateTime.Now.ToString("yyMMddHHmmss")));
        //                result = 1;
        //                break;
        //            case STKBillType.TurnOrder:
        //                string postbarcode = string.Empty;
        //                postbarcode = barcode;
        //                Dictionary<string, object> objTurnOrder = JsonConvert.DeserializeObject<Dictionary<string, object>>(obj.ToString());

        //                if (LibSysUtils.ToInt32(objTurnOrder["IsPackage"]) == 1)
        //                {
        //                    postbarcode = LibSysUtils.ToString(objTurnOrder["PackageBarcode"]);
        //                }

        //                this.DataAccess.ExecuteStoredProcedure("stkTurnOrderUpdate", out outputData, barcode, postbarcode, LibSysUtils.ToString(objTurnOrder["CustomerID"]), LibSysUtils.ToString(objTurnOrder["InSaleBillNo"]));
        //                result = 1;
        //                break;
        //            default: break;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        this.ManagerMessage.AddMessage(LibMessageKind.Error, e.ToString());
        //    }

        //    return result;
        //}

        public STKOutWareInfoList GetSTKOutWareInfo(string billno)
        {

            STKOutWareInfoList stkoutwarelist = new STKOutWareInfoList();
            SqlBuilder sqlbuilder = new SqlBuilder("stk.OutWare");
            string sql = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(billno))
                {
                    sql = sqlbuilder.GetQuerySql(0, "A.TYPENAME,A.BILLNO,A.BILLDATE,A.PLANOUTNUM,A.HASOUTNUM", string.Format(" A.CURRENTSTATE = 2 "));
                    using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            STKOutWareMain stkoutwaremain = new STKOutWareMain();
                            stkoutwaremain.BillNo = LibSysUtils.ToString(reader["BILLNO"]);
                            stkoutwaremain.BillType = LibSysUtils.ToString(reader["TYPENAME"]);
                            stkoutwaremain.BillDate = LibSysUtils.ToString(reader["BILLDATE"]);
                            stkoutwaremain.PlanOutNum = LibSysUtils.ToInt32(reader["PLANOUTNUM"]);
                            stkoutwaremain.HasOutNum = LibSysUtils.ToInt32(reader["HASOUTNUM"]);
                            stkoutwarelist.stkOutWareMainList.Add(stkoutwaremain);
                        }
                    }
                }
                else
                {
                    sql = sqlbuilder.GetQuerySql(1, "B.BILLNO,B.ROW_ID,B.SALBILLNO,B.MATERIALID,B.MATERIALNAME,B.CUSTOMERID,B.CUSTOMERNAME,B.OUTSALBILLNO,B.SPLITSALBILLNO,B.SPLITBOX,B.TURNORDER,B.STORAGEID,B.STORAGENAME,B.LIBRARYID,B.LIBRARYNAME", string.Format(" B.BILLNO = '{0}'", billno));
                    using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            STKOutWareInfo stkoutwareinfo = new STKOutWareInfo();
                            stkoutwareinfo.BillNo = LibSysUtils.ToString(reader["BILLNO"]);
                            stkoutwareinfo.Row_Id = LibSysUtils.ToInt32(reader["ROW_ID"]);
                            stkoutwareinfo.SalBillNo = LibSysUtils.ToString(reader["SALBILLNO"]);
                            stkoutwareinfo.MaterialId = LibSysUtils.ToString(reader["MATERIALID"]);
                            stkoutwareinfo.MaterialName = LibSysUtils.ToString(reader["MATERIALNAME"]);
                            stkoutwareinfo.CustomerId = LibSysUtils.ToString(reader["CUSTOMERID"]);
                            stkoutwareinfo.CustomerName = LibSysUtils.ToString(reader["CUSTOMERNAME"]);
                            stkoutwareinfo.OutSalBillNo = LibSysUtils.ToString(reader["OUTSALBILLNO"]);
                            stkoutwareinfo.SplitSalBillNo = LibSysUtils.ToString(reader["SPLITSALBILLNO"]);
                            stkoutwareinfo.SplitBox = LibSysUtils.ToInt32(reader["SPLITBOX"]);
                            stkoutwareinfo.TurnOrder = LibSysUtils.ToInt32(reader["TURNORDER"]);
                            stkoutwareinfo.StorageId = LibSysUtils.ToString(reader["STORAGEID"]);
                            stkoutwareinfo.StorageName = LibSysUtils.ToString(reader["STORAGENAME"]);
                            stkoutwareinfo.LibraryId = LibSysUtils.ToString(reader["LIBRARYID"]);
                            stkoutwareinfo.LibraryName = LibSysUtils.ToString(reader["LIBRARYNAME"]);
                            stkoutwarelist.stkOutWareInfoList.Add(stkoutwareinfo);
                        }

                    }
                }
            }
            catch (Exception e)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, e.ToString());
            }
            return stkoutwarelist;
        }



        public string GetSTKLabelTemplateJs(string labelTemplateId)
        {
            string labelTemplateJs = ReadPrintTemplateTxt(labelTemplateId);
            List<LabelTemplateRule> list = new List<LabelTemplateRule>();
            SqlBuilder sqlBuilder = new SqlBuilder("com.LabelTemplate");
            StringBuilder builder = new StringBuilder();
            StringBuilder selectBuilder = new StringBuilder();
            builder.Append(sqlBuilder.GetQuerySql(1, "B.LTPARAMTYPE,B.LTPARAMNAME,B.LTPARAMVALUE,B.FIELDNAME", string.Format("B.LABELTEMPLATEID = {0}", LibStringBuilder.GetQuotString(labelTemplateId))));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(builder.ToString()))
            {
                while (reader.Read())
                {
                    LabelTemplateRule rule = new LabelTemplateRule()
                    {
                        LtParamType = (LtParamType)LibSysUtils.ToInt32(reader["LTPARAMTYPE"]),
                        LtParamName = LibSysUtils.ToString(reader["LTPARAMNAME"]),
                        LtParamValue = LibSysUtils.ToString(reader["LTPARAMVALUE"]),
                        FieldName = LibSysUtils.ToString(reader["FIELDNAME"])
                    };
                    list.Add(rule);
                    if (rule.LtParamType == LtParamType.Field)
                    {
                        selectBuilder.AppendFormat("A.{0},", rule.FieldName);
                    }
                }
            }
            foreach (LabelTemplateRule item in list)
            {
                switch (item.LtParamType)
                {
                    case LtParamType.Image:
                        labelTemplateJs = labelTemplateJs.Replace(item.LtParamName, string.Format("<img src='../Content/images/{0}'/>", item.LtParamValue));
                        break;
                    case LtParamType.Field:
                        break;
                    case LtParamType.Date:
                        DateTime dateTime = DateTime.Now;
                        string dateStr = (dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day).ToString();
                        labelTemplateJs = labelTemplateJs.Replace(item.LtParamName, dateStr);
                        break;
                }
            }
            return labelTemplateJs;
        }

        public STKCheckResult STKCheckTurnOrderBarCode(string barcode, string outSaleBillNo, string inSaleBillNo, string mateialId)
        {
            STKCheckResult stkCheck = new STKCheckResult();
            try
            {
                PrintStorageBarcodeInfo info = new PrintStorageBarcodeInfo();
                int serialLen = 0;
                string barcodeTypeID = string.Empty;
                string barcodeTypeName = string.Empty;
                bool isPackage = false;
                Dictionary<string, object> outputData = null;
                this.DataAccess.ExecuteStoredProcedure("STKCheckTurnOrderBarcode", out outputData, outSaleBillNo, barcode, 0, string.Empty, string.Empty, 0);
                STKBarCodeCheckResult execCode = (STKBarCodeCheckResult)LibSysUtils.ToInt32(outputData["EXECCODE_VAL"]);
                barcodeTypeID = LibSysUtils.ToString(outputData["BARCODETYPEID_VAL"]);
                barcodeTypeName = LibSysUtils.ToString(outputData["BARCODETYPENAME_VAL"]);
                switch (execCode)
                {
                    case STKBarCodeCheckResult.NotFindBarcode:
                        stkCheck.stkCheck = STKBarCodeCheckResult.NotFindBarcode;
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("条码{0}没有记录，尚未入库！", barcode));
                        break;
                    case STKBarCodeCheckResult.NotRightSalBillNo:
                        stkCheck.stkCheck = STKBarCodeCheckResult.NotRightSalBillNo;
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("条码{0}不在订单{1}中！", barcode, outSaleBillNo));
                        break;
                    case STKBarCodeCheckResult.HasOutWare:
                        stkCheck.stkCheck = STKBarCodeCheckResult.HasOutWare;
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("条码{0}已出库！", barcode, outSaleBillNo));
                        break;
                    default:
                        stkCheck.stkCheck = STKBarCodeCheckResult.IsPass;
                        if (LibSysUtils.ToInt32(outputData["ISPACKAGE_VALUE"]) == 1)
                        {
                            if (outSaleBillNo != inSaleBillNo)
                            {
                                string billNo = string.Empty;
                                string sql = string.Format("select count(barcode) from wspackagerecord where packagebarcode ='{0}'", barcode);
                                info.PackageNum = LibSysUtils.ToString(this.DataAccess.ExecuteScalar(sql));
                                string barcodeRuleID = GetBarcodeRuleId(inSaleBillNo, mateialId, ref billNo);
                                if (billNo != "")
                                {
                                    ProduceData produceData = LibProduceCache.Default.GetProduceData(billNo);
                                    info.Color = LibSysUtils.ToString(produceData.WorkOrder.Tables[0].Rows[0]["COLOR"]);
                                    info.CrossWeight = LibSysUtils.ToString(produceData.WorkOrder.Tables[0].Rows[0]["GROSSWEIGHT"]);
                                    info.NetWeight = LibSysUtils.ToString(produceData.WorkOrder.Tables[0].Rows[0]["NETWEIGHT"]);
                                    info.LabelTemplateID = LibSysUtils.ToString(produceData.WorkOrder.Tables[0].Rows[0]["LABELTEMPLATEID5"]);
                                    info.CellSpec = LibSysUtils.ToString(produceData.WorkOrder.Tables[0].Rows[0]["CELLSPEC"]);
                                    isPackage = true;
                                    string progId = "ws.Barcode";
                                    string fieldName = "BARCODE";
                                    DataTable dt = new DataTable();
                                    DataColumn datacolumn = new DataColumn("SALBILLNO");//添加表头
                                    dt.Columns.Add(datacolumn);
                                    DataRow dr = dt.NewRow();
                                    dr["SALBILLNO"] = inSaleBillNo;
                                    dt.Rows.Add(dr);
                                    info.Barcode = LibBarcodeServer.Default.GetBarcode(progId, fieldName, barcodeRuleID, new List<DataRow>() { dt.Rows[0] }, ref serialLen);
                                }
                                else
                                {
                                    stkCheck.stkCheck = STKBarCodeCheckResult.IsNotWorkOrderBillNO;
                                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("找不到派工单，请仔细检查销售订单和物料编号！", barcode, outSaleBillNo));
                                }
                            }
                            else
                            {
                                info.Barcode = barcode;
                            }
                        }
                        else
                        {
                            info.Barcode = barcode;
                        }
                        break;
                }
                stkCheck.stkMainInfo.BarCodeTypeId = barcodeTypeID;
                stkCheck.stkMainInfo.BarCodeTypeName = barcodeTypeName;
                stkCheck.stkMainInfo.IsPackage = isPackage;
                stkCheck.stkMainInfo.PackageBarcode = info.Barcode;
                stkCheck.stkMainInfo.SerialLen = serialLen;
                stkCheck.stkMainInfo.Color = info.Color;
                stkCheck.stkMainInfo.CellSpec = info.CellSpec;
                stkCheck.stkMainInfo.CrossWeight = info.CrossWeight;
                stkCheck.stkMainInfo.NetWeight = info.NetWeight;
                stkCheck.stkMainInfo.LabelTemplateID = info.LabelTemplateID;
                stkCheck.stkMainInfo.Barcode = info.Barcode;
            }
            catch (Exception e)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, e.ToString());
            }
            return stkCheck;
        }

        public string GetBarcodeRuleId(string salbillNo, string materialId, ref string billNo)
        {
            int i = 0;

            SqlBuilder builder = new SqlBuilder("pp.WorkOrder");
            string sql = builder.GetQuerySql(0, "A.BILLNO", string.Format("A.SALBILLNO = '{0}' AND A.MATERIALID = '{1}'", salbillNo, materialId), "A.PRODUCELINECONFIGID DESC");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    if (i == 0)
                    {
                        billNo = LibSysUtils.ToString(reader["BILLNO"]);
                    }
                }
            }
            int workProcessNo = 0;
            string workStartionConfigId = string.Empty;
            sql = builder.GetQuerySql(2, "C.WORKPROCESSNO,C.WORKSTATIONCONFIGID", string.Format("C.BILLNO ='{0}'", billNo));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    if (LibSysUtils.ToInt32(reader["WORKPROCESSNO"]) > workProcessNo)
                    {
                        workProcessNo = LibSysUtils.ToInt32(reader["WORKPROCESSNO"]);
                        workStartionConfigId = LibSysUtils.ToString(reader["WORKSTATIONCONFIGID"]);
                    }

                }
            }
            builder = new SqlBuilder("com.WorkstationConfig");
            string barcodeRuleId = string.Empty;
            sql = builder.GetQuerySql(2, "C.BARCODERULEID",
                string.Format("C.WORKSTATIONCONFIGID={0}", LibStringBuilder.GetQuotString(workStartionConfigId)), "C.ROWNO ASC");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    barcodeRuleId = LibSysUtils.ToString(reader["BARCODERULEID"]);
                }
            }
            return barcodeRuleId;
        }
        public STKSplitBoxList GetSTKSplitBox(string info)
        {

            STKSplitBoxList stksplitboxlist = new STKSplitBoxList();
            SqlBuilder sqlbuilder = new SqlBuilder("stk.SplitBox");
            string sql = sqlbuilder.GetQuerySql(0, "A.SALBILLNO,A.BILLNO,A.OUTWARENO", string.Format(""));
            try
            {
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        STKSplitBox stksplitbox = new STKSplitBox();
                        stksplitbox.SalBillNo = LibSysUtils.ToString(reader["SALBILLNO"]);
                        stksplitbox.BillNo = LibSysUtils.ToString(reader["BILLNO"]);
                        stksplitbox.OutWareNo = LibSysUtils.ToString(reader["OUTWARENO"]);
                        stksplitboxlist.stkSplitBoxList.Add(stksplitbox);
                    }
                }
            }
            catch (Exception e)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, e.ToString());
            }
            return stksplitboxlist;
        }


        #endregion

        #region[维修]
        private readonly List<string> _mainBarCode = new List<string>();
        private string _markBarCode = string.Empty;
        private string _sqlCycle = string.Empty;

        /// <summary>
        /// 检验是否存在不良工序，并取出其他相应数据
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <returns></returns>
        public Maintenance MainCheckBarCode(string barcode)
        {
            int processNo = 0;
            string barCode = string.Empty;
            string billNo = string.Empty;
            long createTime = 0;
            Maintenance maintenance = new Maintenance();
            string sqlCheck = string.Format(@"SELECT DISTINCT
                                                B.BARCODE ,
                                                B.ISPASS ,
                                                A.BILLNO ,
                                                B.WORKPROCESSNO ,
                                                C.DEFECTNAME,
                                                A.CREATETIME
                                                FROM    WSLINKBARCODE A
                                                LEFT JOIN WSBARCODE B ON A.BARCODE = B.BARCODE AND A.BILLNO = B.BILLNO AND A.WORKPROCESSNO = B.WORKPROCESSNO
                                                LEFT JOIN WSBARCODEBADRECORD C ON B.WORKPROCESSNO = C.WORKPROCESSNO
                                                AND B.BARCODE = C.BARCODE
                                                AND B.BILLNO = C.BILLNO
                                                WHERE   ( A.BARCODE = '{0}'
                                                OR A.LINKBARCODE = '{0}'
                                                )
                                                AND B.ISPASS = 0
                                                UNION
                                                SELECT DISTINCT
                                                A.BARCODE ,
                                                A.ISPASS ,
                                                A.BILLNO ,
                                                B.WORKPROCESSNO ,
                                                B.DEFECTNAME,
                                                A.CREATETIME
                                                FROM    WSBARCODE A
                                                LEFT JOIN WSBARCODEBADRECORD B ON A.BARCODE = B.BARCODE
                                                AND A.WORKPROCESSNO = B.WORKPROCESSNO
                                                AND A.BILLNO = B.BILLNO
                                                WHERE   A.BARCODE = '{0}'
                                                AND A.ISPASS = 0", barcode);
            using (IDataReader reader = DataAccess.ExecuteDataReader(sqlCheck))
            {

                while (reader.Read())
                {
                    maintenance.comDect.Add(LibSysUtils.ToString(reader["DEFECTNAME"]));
                    processNo = LibSysUtils.ToInt32(reader["WORKPROCESSNO"]);
                    barCode = LibSysUtils.ToString(reader["BARCODE"]);
                    billNo = LibSysUtils.ToString(reader["BILLNO"]);
                    createTime = LibSysUtils.ToInt64(reader["CREATETIME"]);
                }
            }
            if (maintenance.comDect.Count > 0)
            {
                sqlCheck = string.Format(@"SELECT COUNT(BARCODE) FROM WSREPAIRBARCODE WHERE BARCODE = '{0}' AND WORKPROCESSNO = {1} AND BILLNO = '{2}' AND CREATETIME = {3}", barCode, processNo, billNo, createTime);
                int isFinishBad = LibSysUtils.ToInt32(DataAccess.ExecuteScalar(sqlCheck));
                if (isFinishBad <= 0)
                {
                    maintenance.IsPass = true;
                }
            }
            if (maintenance.IsPass)
            {
                sqlCheck = string.Format(@"SELECT DISTINCT A.BARCODE,
                                                                A.BARCODETYPEID,
                                                                A.BARCODETYPENAME,
                                                                A.BILLNO,
                                                                B.MATERIALID,
                                                                B.QUANTITY,
                                                                C.MATERIALNAME,
                                                                C.MATERIALSPEC,
                                                                D.UNITNAME,
                                                                E.CUSTOMERNAME,
                                                                F.WORKPROCESSNO,
                                                                G.WORKPROCESSNAME
                                                                FROM WSBARCODE A
                                                                LEFT JOIN PPWORKORDER B
                                                                ON A.BILLNO = B.BILLNO
                                                                LEFT JOIN COMMATERIAL C
                                                                ON B.MATERIALID = C.MATERIALID
                                                                LEFT JOIN COMUNIT D
                                                                ON C.UNITID = D.UNITID
                                                                LEFT JOIN COMCUSTOMER E
                                                                ON B.CUSTOMERID = E.CUSTOMERID
                                                                LEFT JOIN PPTECHROUTEDATA F
                                                                ON B.BILLNO = F.BILLNO
                                                                LEFT JOIN COMWORKPROCESS G
                                                                ON F.WORKPROCESSID = G.WORKPROCESSID
                                                                WHERE A.BARCODE = '{0}'
                                                                AND A.BILLNO = '{1}' AND F.NEEDGATHER = 1
                                                                ORDER BY F.WORKPROCESSNO", barCode, billNo);
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sqlCheck))
                {
                    while (reader.Read())
                    {
                        maintenance.UnitName = LibSysUtils.ToString(reader["UNITNAME"]);
                        maintenance.Quantity = LibSysUtils.ToDecimal(reader["QUANTITY"]);
                        maintenance.CustomerName = LibSysUtils.ToString(reader["CUSTOMERNAME"]);
                        maintenance.MaterialId = LibSysUtils.ToString(reader["MATERIALID"]);
                        maintenance.MaterialName = LibSysUtils.ToString(reader["MATERIALNAME"]);
                        maintenance.MaterialSpec = LibSysUtils.ToString(reader["MATERIALSPEC"]);
                        if (LibSysUtils.ToInt32(reader["WORKPROCESSNO"]) == processNo)
                        {
                            maintenance.BillNo = LibSysUtils.ToString(reader["BILLNO"]);
                            maintenance.BarCode = LibSysUtils.ToString(reader["BarCode"]);
                            maintenance.BarCodeTypeId = LibSysUtils.ToString(reader["BARCODETYPEID"]);
                            maintenance.BarCodeTypeName = LibSysUtils.ToString(reader["BARCODETYPENAME"]);
                            maintenance.WorkprocessNo = LibSysUtils.ToInt32(reader["WORKPROCESSNO"]);
                            maintenance.WorkprocessName = LibSysUtils.ToString(reader["WORKPROCESSNAME"]);
                            break;
                        }
                    }
                }
                sqlCheck = @"SELECT BADNESSID,BADNESSNAME FROM COMBADNESS";
                using (IDataReader reader = DataAccess.ExecuteDataReader(sqlCheck))
                {
                    while (reader.Read())
                    {
                        Badness badness = new Badness
                        {
                            BadnessId = LibSysUtils.ToString(reader["BADNESSID"]),
                            BadnessName = LibSysUtils.ToString(reader["BADNESSNAME"])
                        };
                        maintenance.BadnessList.Add(badness);
                    }
                }
            }
            return maintenance;
        }

        /// <summary>
        /// 获取条码类型
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <returns></returns>
        public string GetBarCodeType(string barcode)
        {
            string[] ret = new string[2];
            string sql = string.Format("SELECT DISTINCT BARCODETYPEID,BARCODETYPENAME FROM WSBARCODE WHERE BARCODE = '{0}'", barcode);
            string sqlLink = string.Format("SELECT DISTINCT BARCODETYPEID,BARCODETYPENAME FROM WSLINKBARCODE WHERE LINKBARCODE = '{0}'", barcode);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    ret[0] = LibSysUtils.ToString(reader["BARCODETYPEID"]);
                    ret[1] = LibSysUtils.ToString(reader["BARCODETYPENAME"]);
                }
            }
            if (string.IsNullOrEmpty(ret[0]))
            {
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sqlLink))
                {
                    while (reader.Read())
                    {
                        ret[0] = LibSysUtils.ToString(reader["BARCODETYPEID"]);
                        ret[1] = LibSysUtils.ToString(reader["BARCODETYPENAME"]);
                    }
                }
            }
            string result = ret[0] + ',' + ret[1];
            return result;

        }

        /// <summary>
        /// 检验条码是否符合规则
        /// </summary>
        /// <param name="length">长度</param>
        /// <param name="barcode">条码</param>
        /// <param name="barcodetypeId">条码类型</param>
        /// <returns></returns>
        public bool CheckBarCodeType(int length, string barcode, string barcodetypeId)
        {
            bool checkResult = false;
            string sql = string.Format(@"SELECT *
                                        FROM (SELECT COUNT(BARCODE) AS BARCODE
                                        FROM WSLINKBARCODE
                                        WHERE LINKBARCODE = '{0}'
                                        UNION
                                        SELECT COUNT(BARCODE) AS BARCODE
                                        FROM WSBARCODE
                                        WHERE BARCODE = '{0}') A
                                        ORDER BY BARCODE DESC", barcode);
            int result = LibSysUtils.ToInt32(DataAccess.ExecuteScalar(sql));
            if (result > 0) return false;
            if (barcode.Length == length && length > 0)
            {
                List<string> ruleId = new List<string>();
                sql = string.Format(@"select DISTINCT A.BARCODETYPEID, A.BARCODERULEID, B.BARCODERULEID AS RULEID
                                            FROM COMBARCODETYPE A
                                            LEFT JOIN COMBARCODERULEUSECONDITION B
                                            ON A.BARCODETYPEID = B.BARCODETYPEID
                                            WHERE A.BARCODETYPEID = '{0}'
                                            ORDER BY A.BARCODETYPEID", barcodetypeId);
                using (IDataReader reader = DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        if (!ruleId.Contains(LibSysUtils.ToString(reader["BARCODERULEID"])) && !string.IsNullOrEmpty(LibSysUtils.ToString(reader["BARCODERULEID"])))
                        {
                            ruleId.Add(LibSysUtils.ToString(reader["BARCODERULEID"]));
                        }
                        if (!ruleId.Contains(LibSysUtils.ToString(reader["RULEID"])) && !string.IsNullOrEmpty(LibSysUtils.ToString(reader["RULEID"])))
                        {
                            ruleId.Add(LibSysUtils.ToString(reader["RULEID"]));
                        }
                    }
                }
                Dictionary<string, List<BarcodeFixCode>> checkCondtion = new Dictionary<string, List<BarcodeFixCode>>();
                foreach (string id in ruleId)
                {
                    List<BarcodeFixCode> lbf = new List<BarcodeFixCode>();
                    BarcodeRule barcodeRule = LibBarcodeRuleCache.Default.GetBarcodeRule(id);
                    if (barcodeRule != null)
                    {
                        lbf.AddRange(from item in barcodeRule.Items where item.SectionType == BarcodeRuleSectionType.None select new BarcodeFixCode(item.Start, item.Value));
                    }
                    if (lbf.Count > 0)
                    {
                        checkCondtion.Add(id, lbf);
                    }
                }
                if (checkCondtion.Count == 0)
                {
                    checkResult = true;
                }
                foreach (KeyValuePair<string, List<BarcodeFixCode>> dic in checkCondtion)
                {
                    for (int i = 0; i < dic.Value.Count; i++)
                    {
                        if (barcode.Substring(dic.Value[i].Start, dic.Value[i].Value.Length) != dic.Value[i].Value)
                        {
                            break;
                        }
                        if (i == dic.Value.Count - 1)
                        {
                            checkResult = true;
                        }
                    }
                }
            }
            return checkResult;
        }

        /// <summary>
        /// 维修过账
        /// </summary>
        /// <param name="mainbarCode">条码信息</param>
        /// <param name="personId">人员编号</param>
        /// <param name="personName">人员名称</param>
        /// <param name="currentIspass">是否通过</param>
        public void MaintenancePost(mainBarCode mainbarCode, string personId, string personName, bool currentIspass)
        {

            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                long repairTime = LibDateUtils.GetCurrentDateTime();
                DataAccess.ExecuteStoredProcedure("MaintenceInsertHistory", mainbarCode.processInformation.BarCode, mainbarCode.processInformation.BillNo, mainbarCode.processInformation.WorkprocessNo, mainbarCode.processInformation.Createtime);
                DataAccess.ExecuteStoredProcedure("MaintenceRepair", mainbarCode.processInformation.BarCode, mainbarCode.processInformation.BillNo, mainbarCode.processInformation.WorkprocessNo, repairTime, personId, personName, mainbarCode.isOK, currentIspass);
                HashSet<string> set = new HashSet<string>();
                foreach (ActualChangeBarCode changebarcode in mainbarCode.changeBarCode.Where(changebarcode => !set.Contains(changebarcode.oldBarCode)))
                {
                    DataAccess.ExecuteStoredProcedure("MainChangeBarCode", mainbarCode.processInformation.BarCode, mainbarCode.processInformation.BillNo, mainbarCode.workProcessNo, changebarcode.oldBarCode, changebarcode.newBarCode, repairTime);
                    set.Add(changebarcode.oldBarCode);
                }
                foreach (Badness badness in mainbarCode.badnessList.Where(badness => !set.Contains(badness.BadnessId)))
                {
                    DataAccess.ExecuteStoredProcedure("MaintenceBadness", mainbarCode.processInformation.BarCode, mainbarCode.processInformation.BillNo, mainbarCode.workProcessNo, badness.BadnessId, badness.BadnessName, repairTime);
                    set.Add(badness.BadnessId);
                }
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;

            }


        }

        /// <summary>
        /// 获取条码生命周期数据
        /// </summary>
        /// <param name="barCode">条码</param>
        /// <returns></returns>
        public List<LifeCycle> GetLifeCycle(string barCode)
        {
            List<LifeCycle> list = new List<LifeCycle>();
            List<string> tempBarCode = new List<string>();
            Dictionary<string, object> outputData;
            DataAccess.ExecuteStoredProcedure("WsBarCodeLifeCycleCheck", out outputData, barCode, 0, string.Empty);
            LifeCycleType lifecycletype = (LifeCycleType)LibSysUtils.ToInt32(outputData["RETURN_VAL"]);
            switch (lifecycletype)
            {
                //不存在此条码
                case LifeCycleType.NotExisted:
                    ManagerMessage.AddMessage(LibMessageKind.Error, "该条码不存在！");
                    break;
                //主码非唛头
                case LifeCycleType.MainBarCode:
                    _mainBarCode.Add(barCode);
                    tempBarCode.Add(barCode);
                    if (string.IsNullOrEmpty(_markBarCode))
                    {
                        _sqlCycle = string.Format("SELECT PACKAGEBARCODE FROM WSPACKAGERECORD WHERE BARCODE = '" + tempBarCode[0] + "'");
                        for (int i = 1; i < tempBarCode.Count; i++)
                        {
                            _sqlCycle += " OR BARCODE = '" + tempBarCode[i] + "'";
                        }
                        using (IDataReader reader = DataAccess.ExecuteDataReader(_sqlCycle))
                        {
                            while (reader.Read())
                            {
                                _markBarCode = reader[0].ToString();
                                _mainBarCode.Add(reader[0].ToString());
                                break;
                            }
                        }
                    }
                    _sqlCycle = string.Format("SELECT DISTINCT BARCODE FROM WSLINKBARCODE WHERE LINKBARCODE = '" + tempBarCode[0] + "'");
                    for (int i = 1; i < tempBarCode.Count; i++)
                    {
                        _sqlCycle += " OR LINKBARCODE = '" + tempBarCode[i] + "'";
                    }
                    using (IDataReader reader = DataAccess.ExecuteDataReader(_sqlCycle))
                    {

                        while (reader.Read())
                        {
                            string code = reader[0].ToString();
                            if (!_mainBarCode.Contains(code))
                            {
                                tempBarCode.Add(code);
                            }
                        }

                    }
                    _sqlCycle = string.Format("SELECT DISTINCT LINKBARCODE FROM WSLINKBARCODE WHERE BARCODE = '" + tempBarCode[0] + "'");
                    for (int i = 1; i < tempBarCode.Count; i++)
                    {
                        _sqlCycle += " OR BARCODE = '" + tempBarCode[i] + "'";
                    }
                    using (IDataReader reader = DataAccess.ExecuteDataReader(_sqlCycle))
                    {

                        //TempBarCode.Clear();
                        while (reader.Read())
                        {
                            string code = reader[0].ToString();
                            if (!tempBarCode.Contains(code))
                            {
                                tempBarCode.Add(code);
                            }
                        }

                    }
                    GetBarCode(tempBarCode);
                    list = GetCollection(_mainBarCode);
                    break;
                //关联码
                case LifeCycleType.LinkBarCode:
                    _mainBarCode.Add(barCode);
                    string linkBarCode = LibSysUtils.ToString(outputData["REBARCODE"]);
                    tempBarCode.Add(linkBarCode);
                    GetBarCode(tempBarCode);
                    list = GetCollection(_mainBarCode);
                    break;
                //唛头
                case LifeCycleType.MarkBarCode:
                    _markBarCode = barCode;
                    _mainBarCode.Add(barCode);
                    _sqlCycle = string.Format("SELECT DISTINCT BARCODE FROM WSPACKAGERECORD WHERE PACKAGEBARCODE = '{0}'", barCode);
                    using (IDataReader reader = DataAccess.ExecuteDataReader(_sqlCycle))
                    {
                        while (reader.Read())
                        {
                            tempBarCode.Add(reader[0].ToString());
                        }
                    }
                    GetBarCode(tempBarCode);
                    list = GetCollection(_mainBarCode);
                    break;
            }
            return list;
        }

        /// <summary>
        /// 获取关联条码的关联数据
        /// </summary>
        /// <param name="tempCount">关联条码</param>
        public void GetBarCode(List<string> tempCount)
        {
            while (true)
            {
                for (int i = 0; i < tempCount.Count; i++)
                {
                    if (!_mainBarCode.Contains(tempCount[i]))
                    {
                        _mainBarCode.Add(tempCount[i]);
                    }
                    else
                    {
                        tempCount.RemoveAt(i);
                    }
                }
                if (tempCount.Count > 0)
                {
                    _sqlCycle = string.Format("SELECT DISTINCT BARCODE FROM WSBARCODE WHERE BARCODE = '" + tempCount[0] + "'");
                    for (int i = 1; i < tempCount.Count; i++)
                    {
                        _sqlCycle += " OR BARCODE = '" + tempCount[i] + "'";
                    }
                    using (IDataReader reader = DataAccess.ExecuteDataReader(_sqlCycle))
                    {
                        tempCount.Clear();
                        while (reader.Read())
                        {
                            tempCount.Add(reader[0].ToString());
                        }
                    }
                }
                if (tempCount.Count > 0)
                {
                    if (string.IsNullOrEmpty(_markBarCode))
                    {
                        _sqlCycle = string.Format("SELECT PACKAGEBARCODE FROM WSPACKAGERECORD WHERE BARCODE = '" + tempCount[0] + "'");
                        for (int i = 1; i < tempCount.Count; i++)
                        {
                            _sqlCycle += " OR BARCODE = '" + tempCount[i] + "'";
                        }
                        using (IDataReader reader = DataAccess.ExecuteDataReader(_sqlCycle))
                        {
                            while (reader.Read())
                            {
                                _markBarCode = reader[0].ToString();
                                _mainBarCode.Add(reader[0].ToString());
                                break;
                            }
                        }
                    }
                    _sqlCycle = string.Format("SELECT DISTINCT BARCODE FROM WSLINKBARCODE WHERE LINKBARCODE = '" + tempCount[0] + "'");
                    for (int i = 1; i < tempCount.Count; i++)
                    {
                        _sqlCycle += " OR LINKBARCODE = '" + tempCount[i] + "'";
                    }
                    using (IDataReader reader = DataAccess.ExecuteDataReader(_sqlCycle))
                    {
                        while (reader.Read())
                        {
                            string code = reader[0].ToString();
                            if (!_mainBarCode.Contains(code))
                            {
                                tempCount.Add(code);
                            }
                        }
                    }
                    _sqlCycle = string.Format("SELECT DISTINCT LINKBARCODE FROM WSLINKBARCODE WHERE BARCODE = '" + tempCount[0] + "'");
                    for (int i = 1; i < tempCount.Count; i++)
                    {
                        _sqlCycle += " OR BARCODE = '" + tempCount[i] + "'";
                    }
                    using (IDataReader reader = DataAccess.ExecuteDataReader(_sqlCycle))
                    {
                        //TempCount.Clear();
                        while (reader.Read())
                        {
                            string code = reader[0].ToString();
                            if (!tempCount.Contains(code))
                            {
                                tempCount.Add(code);
                            }
                        }
                    }
                    continue;
                }
                break;
            }
        }

        /// <summary>
        /// 获取全部的条码数据
        /// </summary>
        /// <param name="collection">所有关联的条码集合</param>
        /// <returns></returns>
        public List<LifeCycle> GetCollection(List<string> collection)
        {
            List<LifeCycle> lifecyclelist = new List<LifeCycle>();
            if (_mainBarCode.Count > 0)
            {
                _sqlCycle = string.Format("SELECT DISTINCT BARCODE,BILLNO,CREATETIME,ISPASS,PRODUCELINEID,PRODUCELINENAME,BARCODETYPEID,BARCODETYPENAME,WORKPROCESSNO,WORKPROCESSID,WORKPROCESSNAME,CREATORID,CREATORNAME FROM(SELECT DISTINCT BARCODE,BILLNO,CREATETIME,ISPASS,PRODUCELINEID,PRODUCELINENAME,BARCODETYPEID,BARCODETYPENAME,WORKPROCESSNO,WORKPROCESSID,WORKPROCESSNAME,CREATORID,CREATORNAME FROM WSBARCODE WHERE BARCODE = '" + _mainBarCode[0] + "'");
                for (int i = 1; i < _mainBarCode.Count; i++)
                {
                    _sqlCycle += " OR BARCODE = '" + _mainBarCode[i] + "'";
                }
                _sqlCycle += " union ";
                _sqlCycle += string.Format("SELECT DISTINCT A.LINKBARCODE as BARCODE,A.BILLNO AS BILLNO,A.CREATETIME AS CREATETIME,B.ISPASS AS ISPASS,B.PRODUCELINEID AS PRODUCELINEID,B.PRODUCELINENAME AS PRODUCELINENAME,A.BARCODETYPEID AS BARCODETYPEID,A.BARCODETYPENAME AS BARCODETYPENAME,B.WORKPROCESSNO AS WORKPROCESSNO,B.WORKPROCESSID AS WORKPROCESSID,B.WORKPROCESSNAME AS WORKPROCESSNAME,B.CREATORID AS CREATORID,B.CREATORNAME AS CREATORNAME FROM WSLINKBARCODE A,WSBARCODE B WHERE A.BILLNO = B.BILLNO AND A.WORKPROCESSNO = B.WORKPROCESSNO AND A.BARCODE = B.BARCODE AND  (A.LINKBARCODE = '" + _mainBarCode[0] + "'");
                for (int i = 1; i < _mainBarCode.Count; i++)
                {
                    _sqlCycle += " OR A.LINKBARCODE = '" + _mainBarCode[i] + "'";
                }
                _sqlCycle += ")";
                _sqlCycle += ") C  order by PRODUCELINEID,WORKPROCESSNO ";
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(_sqlCycle))
                {
                    while (reader.Read())
                    {
                        LifeCycle lifecycle = new LifeCycle();
                        lifecycle.BarCode = LibSysUtils.ToString(reader["BARCODE"]);
                        lifecycle.BillNo = LibSysUtils.ToString(reader["BILLNO"]);
                        lifecycle.ProduceLineId = LibSysUtils.ToString(reader["PRODUCELINEID"]);
                        lifecycle.ProduceLineName = LibSysUtils.ToString(reader["PRODUCELINENAME"]);
                        lifecycle.WorkProcessNo = LibSysUtils.ToInt32(reader["WORKPROCESSNO"]);
                        lifecycle.WorkProcessId = LibSysUtils.ToString(reader["WORKPROCESSID"]);
                        lifecycle.WorkProcessName = LibSysUtils.ToString(reader["WORKPROCESSNAME"]);
                        lifecycle.CreatorId = LibSysUtils.ToString(reader["CREATORID"]);
                        lifecycle.CreatorName = LibSysUtils.ToString(reader["CREATORNAME"]);
                        lifecycle.CreateTime = LibSysUtils.ToInt64(reader["CREATETIME"]);
                        lifecycle.BarCodeTypeId = LibSysUtils.ToString(reader["BARCODETYPEID"]);
                        lifecycle.BarCodeTypeName = LibSysUtils.ToString(reader["BARCODETYPENAME"]);
                        lifecycle.Pass = LibSysUtils.ToInt32(reader["ISPASS"]);
                        lifecyclelist.Add(lifecycle);
                    }
                }
            }
            //lifecyclelist.Sort((left, right) =>
            //{
            //    if (left.WorkProcessNo > right.WorkProcessNo)
            //        return 1;
            //    else if (left.WorkProcessNo == right.WorkProcessNo)
            //        return 0;
            //    else
            //        return -1;
            //});
            //lifecyclelist.Sort((left, right) =>
            //{
            //    if (left.ProduceLineId.CompareTo(right.ProduceLineId)>0)
            //        return 1;
            //    else if (left.ProduceLineId == right.ProduceLineId)
            //        return 0;
            //    else
            //        return -1;
            //});
            return lifecyclelist;
        }
        #endregion

        #region[返工]
        /// <summary>
        /// 获取条码的生命周期数据
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <param name="billNo">派工单</param>
        /// <param name="workProcess">工序号</param>
        /// <returns></returns>
        public List<WorkOrderDetail> GetWorkOrderDetail(string barcode, string billNo, int workProcess)
        {
            List<string> defectList = new List<string>();
            List<WorkOrderDetail> workOrderDetailList = new List<WorkOrderDetail>();
            string sql = string.Format(@"SELECT DISTINCT
                                                    B.BARCODE ,
                                                    B.ISPASS ,
                                                    A.BILLNO ,
                                                    B.WORKPROCESSNO ,
                                                    C.DEFECTNAME
                                                    FROM    WSLINKBARCODE A
                                                    LEFT JOIN WSBARCODE B ON A.BARCODE = B.BARCODE
                                                    LEFT JOIN WSBARCODEBADRECORD C ON B.WORKPROCESSNO = C.WORKPROCESSNO
                                                    AND B.BARCODE = C.BARCODE
                                                    AND B.BILLNO = C.BILLNO
                                                    WHERE   ( A.BARCODE = '{0}'
                                                    OR A.LINKBARCODE = '{0}'
                                                    )
                                                    AND B.ISPASS = 0
                                                    UNION
                                                    SELECT DISTINCT
                                                    A.BARCODE ,
                                                    A.ISPASS ,
                                                    A.BILLNO ,
                                                    B.WORKPROCESSNO ,
                                                    B.DEFECTNAME
                                                    FROM    WSBARCODE A
                                                    LEFT JOIN WSBARCODEBADRECORD B ON A.BARCODE = B.BARCODE
                                                    AND A.WORKPROCESSNO = B.WORKPROCESSNO
                                                    AND A.BILLNO = B.BILLNO
                                                    WHERE   A.BARCODE = '{0}'
                                                    AND A.ISPASS = 0", barcode);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string defect = LibSysUtils.ToString(reader["DEFECTNAME"]);
                    if (!defectList.Contains(defect))
                    {
                        defectList.Add(defect);
                    }
                }
            }
            if (defectList.Count <= 0) return workOrderDetailList;
            {
                sql = string.Format(@"SELECT DISTINCT
                                            A.BARCODE ,
                                            A.ISPASS,
                                            A.BARCODETYPEID ,
                                            A.BARCODETYPENAME ,
                                            A.BILLNO ,
                                            A.WORKPROCESSNO ,
                                            G.WORKPROCESSNAME
                                    FROM    WSBARCODE A
                                            LEFT JOIN COMWORKPROCESS G ON A.WORKPROCESSID = G.WORKPROCESSID
                                    WHERE   A.BARCODE = '{0}'
                                            AND A.BILLNO = '{1}'
                                    ORDER BY A.WORKPROCESSNO", barcode, billNo);
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        WorkOrderDetail workOrderDetail = new WorkOrderDetail();
                        int wProcessNo = LibSysUtils.ToInt32(reader["WORKPROCESSNO"]);
                        bool isPass = LibSysUtils.ToBoolean(reader["ISPASS"]);
                        if (isPass)
                        {
                            if (wProcessNo == workProcess)
                            {
                                workOrderDetail.IsCheck = true;
                            }
                            workOrderDetail.BillNo = LibSysUtils.ToString(reader["BILLNO"]);
                            workOrderDetail.WorkprocessNo = wProcessNo;
                            workOrderDetail.WorkprocessName = LibSysUtils.ToString(reader["WORKPROCESSNAME"]);
                            workOrderDetailList.Add(workOrderDetail);
                        }
                        else
                        {
                            if (wProcessNo == workProcess)
                            {
                                workOrderDetail.IsCheck = true;
                            }
                            workOrderDetail.BillNo = LibSysUtils.ToString(reader["BILLNO"]);
                            workOrderDetail.WorkprocessNo = wProcessNo;
                            workOrderDetail.WorkprocessName = LibSysUtils.ToString(reader["WORKPROCESSNAME"]);
                            workOrderDetailList.Add(workOrderDetail);
                            if (wProcessNo >= workProcess) continue;
                            workOrderDetailList.Clear();
                            break;
                        }
                    }
                }
            }
            return workOrderDetailList;
        }

        /// <summary>
        /// 返工到某道工序
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="reworkProcessNo"></param>
        /// <returns></returns>
        public bool ReworkProcess(string barcode, int reworkProcessNo)
        {
            try
            {
                List<LifeCycle> lifecycleList = GetLifeCycle(barcode);
                foreach (LifeCycle life in lifecycleList.Where(life => life.WorkProcessNo >= reworkProcessNo))
                {
                    this.DataAccess.ExecuteStoredProcedure("MaintenceInsertHistorySpecial", life.BarCode, life.BillNo, life.WorkProcessNo);
                }
            }
            catch { return false; }
            return true;
        }
        #endregion

        #region[检测方法]
        /// <summary>
        /// 获取质检单信息
        /// </summary>
        /// <param name="type">质检单类型</param>
        /// <returns></returns>
        public List<CheckOrder> ChooseCheckOrderList(int type)
        {
            List<CheckOrder> checkOrderList = new List<CheckOrder>();
            ProductScheduling productScheduling = LibWsControlServer.Default.GetProductScheduling();
            string sql;
            string sqlWhere = string.Empty;
            if (type == 2)
            {
                if (productScheduling.PurCheckOrderList.Count > 0)
                {
                    sqlWhere += " WHERE A.BILLNO IN (";
                    sqlWhere = productScheduling.PurCheckOrderList.Aggregate(sqlWhere, (current, billNo) => current + (billNo + ","));
                    sqlWhere = sqlWhere.Substring(0, sqlWhere.LastIndexOf(','));
                    sqlWhere += ") AND (A.ISCONFIG = 1 OR A.ISUSECHECK = 1) AND A.ISFINISHED = 0 AND A.WORKORDERNO = ''";
                }
                sql = @"SELECT A.BILLNO,A.ROW_ID,A.MATERIALID,B.MATERIALNAME,A.ATTRIBUTEDESC,A.CHECKTYPE,A.WORKSTATIONCONFIGID,A.CHECKNUM,A.ISUSECHECK,A.ISCONFIG FROM PURQUALITYCHECKDETAIL A
LEFT JOIN COMMATERIAL B ON A.MATERIALID = B.MATERIALID ";

            }
            else
            {
                if (productScheduling.OutWareCheckOrderList.Count > 0)
                {
                    sqlWhere += " WHERE A.BILLNO IN (";
                    sqlWhere = productScheduling.OutWareCheckOrderList.Aggregate(sqlWhere, (current, billNo) => current + (billNo + ","));
                    sqlWhere = sqlWhere.Substring(0, sqlWhere.LastIndexOf(','));
                    sqlWhere += ") AND (A.ISCONFIG = 1 OR A.ISUSECHECK = 1)  AND A.ISFINISHED = 0 AND A.WORKORDERNO = ''";
                }
                sql = @"SELECT A.BILLNO,A.ROW_ID,A.MATERIALID,B.MATERIALNAME,A.ATTRIBUTEDESC,A.CHECKTYPE,A.WORKSTATIONCONFIGID,A.CHECKNUM,A.ISUSECHECK,A.ISCONFIG FROM OWQUALITYCHECKDETAIL A
LEFT JOIN COMMATERIAL B ON A.MATERIALID = B.MATERIALID ";
            }
            if (sqlWhere.Length <= 0) return checkOrderList;
            sql += sqlWhere;
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    checkOrderList.Add(new CheckOrder()
                    {
                        BillNo = LibSysUtils.ToString(reader["BILLNO"]),
                        RowId = LibSysUtils.ToInt32(reader["ROW_ID"]),
                        MaterialId = LibSysUtils.ToString(reader["MATERIALID"]),
                        MaterialName = LibSysUtils.ToString(reader["MATERIALNAME"]),
                        AttributeDesc = LibSysUtils.ToString(reader["ATTRIBUTEDESC"]),
                        CheckType = GetCheckType(LibSysUtils.ToInt32(reader["CHECKTYPE"])),
                        WorkStationConfigId = LibSysUtils.ToString(reader["WORKSTATIONCONFIGID"]),
                        CheckNum = LibSysUtils.ToDecimal(reader["CHECKNUM"]),
                        IsUserCheck = LibSysUtils.ToBoolean(reader["ISUSECHECK"]),
                        IsConfig = LibSysUtils.ToBoolean(reader["ISCONFIG"]),
                        CheckOrderType = type
                    });
                }
            }
            return checkOrderList;
        }

        /// <summary>
        /// 过账质检数据
        /// </summary>
        /// <param name="type">质检单类型</param>
        /// <param name="isOnlyCheck">是否只启用数量检验</param>
        /// <param name="day">时间范围</param>
        /// <param name="totalNum">显示数据的上限</param>
        /// <returns></returns>
        public List<CheckOrder> ChooseOnlyCheckOrderList(int type, int isOnlyCheck, int day, int totalNum)
        {
            DateTime time = DateTime.Now;
            long billStartTime = LibSysUtils.ToInt64(time.AddDays(-day).ToString("yyyyMMdd"));
            long startTime = LibSysUtils.ToInt64(time.AddDays(-day).ToString("yyyyMMdd") + "000000");

            List<CheckOrder> checkOrderList = new List<CheckOrder>();
            ProductScheduling productScheduling = LibWsControlServer.Default.GetProductScheduling();
            string sql;
            string sqlWhere = string.Empty;
            if (type == 2)
            {
                if (isOnlyCheck == 1)
                {
                    if (productScheduling.PurCheckOrderList.Count > 0)
                    {
                        sqlWhere += " WHERE A.BILLNO IN (";
                        sqlWhere = productScheduling.PurCheckOrderList.Aggregate(sqlWhere, (current, billNo) => current + (billNo + ","));
                        sqlWhere = sqlWhere.Substring(0, sqlWhere.LastIndexOf(','));
                        sqlWhere += ") AND A.ISCONFIG = 0 AND A.ISUSECHECK = 0 AND A.ISFINISHED = 0 AND A.WORKORDERNO = '' AND C.BILLDATE>" + billStartTime + " AND  B.MATERIALTYPEID !='21242' ";
                    }
                }
                else
                {
                    sqlWhere = " WHERE A.ISFINISHED=1 AND A.DEALWAY=0 AND A.STARTTIME>" + startTime + " AND  B.MATERIALTYPEID !='21242' ";
                }
                sql = @"SELECT A.BILLNO,A.ROW_ID,A.MATERIALID,B.MATERIALNAME,A.ATTRIBUTEDESC,A.CHECKTYPE,A.WORKSTATIONCONFIGID,A.QUANTITY,A.CHECKNUM,A.QUALIFIEDNUM,A.UNQUALIFIEDNUM,A.ISUSECHECK,A.ISCONFIG FROM PURQUALITYCHECKDETAIL A
LEFT JOIN COMMATERIAL B ON A.MATERIALID = B.MATERIALID LEFT JOIN PURQUALITYCHECK C ON C.BILLNO=A.BILLNO ";

            }
            else
            {
                if (isOnlyCheck == 1)
                {
                    if (productScheduling.OutWareCheckOrderList.Count > 0)
                    {
                        sqlWhere += " WHERE A.BILLNO IN (";
                        sqlWhere = productScheduling.OutWareCheckOrderList.Aggregate(sqlWhere, (current, billNo) => current + (billNo + ","));
                        sqlWhere = sqlWhere.Substring(0, sqlWhere.LastIndexOf(','));
                        sqlWhere += ") AND A.ISCONFIG = 0 AND A.ISUSECHECK = 0  AND A.ISFINISHED = 0 AND A.WORKORDERNO = '' AND C.BILLDATE>" + billStartTime + "  AND  B.MATERIALTYPEID !='21242'  ";
                    }
                }
                else
                {
                    sqlWhere = " WHERE A.ISFINISHED=1 AND A.DEALWAY=0 AND A.STARTTIME>" + startTime + " ";
                }
                sql = @"SELECT A.BILLNO,A.ROW_ID,A.MATERIALID,B.MATERIALNAME,A.ATTRIBUTEDESC,A.CHECKTYPE,A.WORKSTATIONCONFIGID,A.QUANTITY,A.CHECKNUM,A.QUALIFIEDNUM,A.UNQUALIFIEDNUM,A.ISUSECHECK,A.ISCONFIG FROM OWQUALITYCHECKDETAIL A
LEFT JOIN COMMATERIAL B ON A.MATERIALID = B.MATERIALID LEFT JOIN OWQUALITYCHECK C ON C.BILLNO=A.BILLNO ";
            }
            if (sqlWhere.Length > 0)
            {
                sql += sqlWhere;
                int dataNum = 1;
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        if (dataNum <= totalNum)
                        {
                            checkOrderList.Add(new CheckOrder()
                            {
                                BillNo = LibSysUtils.ToString(reader["BILLNO"]),
                                RowId = LibSysUtils.ToInt32(reader["ROW_ID"]),
                                MaterialId = LibSysUtils.ToString(reader["MATERIALID"]),
                                MaterialName = LibSysUtils.ToString(reader["MATERIALNAME"]),
                                AttributeDesc = LibSysUtils.ToString(reader["ATTRIBUTEDESC"]),
                                CheckType = GetCheckType(LibSysUtils.ToInt32(reader["CHECKTYPE"])),
                                WorkStationConfigId = LibSysUtils.ToString(reader["WORKSTATIONCONFIGID"]),
                                Quantity = LibSysUtils.ToDecimal(reader["QUANTITY"]),
                                CheckNum = LibSysUtils.ToDecimal(reader["CHECKNUM"]),
                                QualifiedNum = LibSysUtils.ToDecimal(reader["QUALIFIEDNUM"]),
                                UnQualifiedNum = LibSysUtils.ToDecimal(reader["UNQUALIFIEDNUM"]),
                                IsUserCheck = LibSysUtils.ToBoolean(reader["ISUSECHECK"]),
                                IsConfig = LibSysUtils.ToBoolean(reader["ISCONFIG"]),
                                CheckOrderType = type
                            });
                            dataNum++;
                        }
                        else { return checkOrderList; }
                    }
                }
            }
            return checkOrderList;
        }

        /// <summary>
        /// 过账质检数据(重载)
        /// </summary>
        /// <param name="type">质检单类型</param>
        /// <param name="isOnlyCheck">是否只启用数量检验</param>
        /// <param name="day">时间范围</param>
        /// <param name="totalNum">显示数据的上限</param>
        /// <param name="conditionList">人员编号集合</param>
        /// <returns></returns>
        public List<CheckOrder> ChooseOnlyCheckOrderList(int type, int isOnlyCheck, int day, int totalNum, List<string> conditionList)
        {
            DateTime time = DateTime.Now;
            long billStartTime = LibSysUtils.ToInt64(time.AddDays(-day).ToString("yyyyMMdd"));
            long startTime = LibSysUtils.ToInt64(time.AddDays(-day).ToString("yyyyMMdd") + "000000");
            List<CheckOrder> checkOrderList = new List<CheckOrder>();
            ProductScheduling productScheduling = LibWsControlServer.Default.GetProductScheduling();
            string sql;
            string sqlWhere = string.Empty;
            string conditionSql = string.Empty;
            if (type == 2)
            {
                switch (isOnlyCheck)
                {
                    case 1:
                        if (productScheduling.PurCheckOrderList.Count > 0)
                        {
                            sqlWhere += " WHERE B.BILLNO IN (";
                            sqlWhere = productScheduling.PurCheckOrderList.Aggregate(sqlWhere, (current, billNo) => current + (billNo + ","));
                            sqlWhere = sqlWhere.Substring(0, sqlWhere.LastIndexOf(','));
                            sqlWhere += ") AND B.ISCONFIG = 0 AND B.ISUSECHECK = 0 AND B.ISFINISHED = 0 AND B.WORKORDERNO = '' AND C.BILLDATE>" + billStartTime + " ";
                        }
                        sql = @"SELECT B.BILLNO,B.ROW_ID,B.MATERIALID,D.MATERIALNAME,E.SUPPLIERNAME,B.ATTRIBUTEDESC,B.CHECKTYPE,B.WORKSTATIONCONFIGID,
                                        B.QUANTITY,B.CHECKNUM,B.QUALIFIEDNUM,B.UNQUALIFIEDNUM,B.ISUSECHECK,B.ISCONFIG,G.PURCHASEORDER
                                        FROM PURQUALITYCHECKPS A LEFT JOIN PURQUALITYCHECKDETAIL B ON B.BILLNO=A.BILLNO AND B.ROW_ID=A.PARENTROWID
                                        LEFT JOIN PURQUALITYCHECK C ON C.BILLNO=B.BILLNO
                                        LEFT JOIN COMMATERIAL D ON D.MATERIALID=B.MATERIALID
                                        LEFT JOIN COMSUPPLIER E ON E.SUPPLIERID=C.SUPPLIERID 
                                        LEFT JOIN STKDELIVERYNOTEDETAIL F ON F.BILLNO=B.FROMBILLNO AND F.ROW_ID=B.FROMROWID
                                        LEFT JOIN PURPURCHASEPLANDETAIL G ON G.BILLNO=F.FROMBILLNO AND G.ROW_ID=F.FROMROWID ";
                        break;
                    case 2:
                        sqlWhere = " WHERE B.ISFINISHED=1 AND B.DEALWAY=0 AND B.STARTTIME>" + startTime + " ";
                        sql = @"SELECT B.BILLNO,B.ROW_ID,B.MATERIALID,D.MATERIALNAME,E.SUPPLIERNAME,B.ATTRIBUTEDESC,B.CHECKTYPE,B.WORKSTATIONCONFIGID,
                                                 B.QUANTITY,B.CHECKNUM,B.QUALIFIEDNUM,B.UNQUALIFIEDNUM,B.ISUSECHECK,B.ISCONFIG,G.PURCHASEORDER 
                                          FROM PURQUALITYCHECKDETAIL B LEFT JOIN PURQUALITYCHECK C ON C.BILLNO=B.BILLNO 
                                          LEFT JOIN COMMATERIAL D ON D.MATERIALID=B.MATERIALID  
                                          LEFT JOIN COMSUPPLIER E ON E.SUPPLIERID=C.SUPPLIERID
                                          LEFT JOIN STKDELIVERYNOTEDETAIL F ON F.BILLNO=B.FROMBILLNO AND F.ROW_ID=B.FROMROWID
                                          LEFT JOIN PURPURCHASEPLANDETAIL G ON G.BILLNO=F.FROMBILLNO AND G.ROW_ID=F.FROMROWID ";
                        break;
                    default:
                        sqlWhere = " WHERE (B.ISCONFIG=1 OR B.ISUSECHECK=1) AND B.ISFINISHED=0 AND B.WORKORDERNO = '' AND C.BILLDATE>" + billStartTime + " ";
                        sql = @"SELECT B.BILLNO,B.ROW_ID,B.MATERIALID,D.MATERIALNAME,E.SUPPLIERNAME,B.ATTRIBUTEDESC,B.CHECKTYPE,B.WORKSTATIONCONFIGID,
                                                 B.QUANTITY,B.CHECKNUM,B.QUALIFIEDNUM,B.UNQUALIFIEDNUM,B.ISUSECHECK,B.ISCONFIG,G.PURCHASEORDER 
                                          FROM PURQUALITYCHECKDETAIL B LEFT JOIN PURQUALITYCHECK C ON C.BILLNO=B.BILLNO 
                                          LEFT JOIN COMMATERIAL D ON D.MATERIALID=B.MATERIALID  
                                          LEFT JOIN COMSUPPLIER E ON E.SUPPLIERID=C.SUPPLIERID
                                          LEFT JOIN STKDELIVERYNOTEDETAIL F ON F.BILLNO=B.FROMBILLNO AND F.ROW_ID=B.FROMROWID
                                          LEFT JOIN PURPURCHASEPLANDETAIL G ON G.BILLNO=F.FROMBILLNO AND G.ROW_ID=F.FROMROWID ";
                        break;
                }
                conditionSql = conditionList.Aggregate(conditionSql, (current, condition) => current + condition);
            }
            else
            {
                if (isOnlyCheck == 1)
                {
                    if (productScheduling.OutWareCheckOrderList.Count > 0)
                    {
                        sqlWhere += " WHERE A.BILLNO IN (";
                        sqlWhere = productScheduling.OutWareCheckOrderList.Aggregate(sqlWhere, (current, billNo) => current + (billNo + ","));
                        sqlWhere = sqlWhere.Substring(0, sqlWhere.LastIndexOf(','));
                        sqlWhere += ") AND A.ISCONFIG = 0 AND A.ISUSECHECK = 0  AND A.ISFINISHED = 0 AND A.WORKORDERNO = '' AND C.BILLDATE>" + billStartTime + " ";
                    }
                }
                else if (isOnlyCheck == 2)
                {
                    sqlWhere = " WHERE A.ISFINISHED=1 AND A.DEALWAY=0 AND A.STARTTIME>" + startTime + " ";
                }
                else
                {
                    sqlWhere = " WHERE (A.ISCONFIG=1 OR A.ISUSECHECK=1) AND A.ISFINISHED=0 AND A.WORKORDERNO='' AND C.BILLDATE>" + billStartTime + " ";
                }
                sql = @"SELECT A.BILLNO,A.ROW_ID,A.MATERIALID,B.MATERIALNAME,A.ATTRIBUTEDESC,A.CHECKTYPE,A.WORKSTATIONCONFIGID,A.QUANTITY,A.CHECKNUM,A.QUALIFIEDNUM,A.UNQUALIFIEDNUM,A.ISUSECHECK,A.ISCONFIG FROM OWQUALITYCHECKDETAIL A
LEFT JOIN COMMATERIAL B ON A.MATERIALID = B.MATERIALID LEFT JOIN OWQUALITYCHECK C ON C.BILLNO=A.BILLNO ";
            }
            if (sqlWhere.Length <= 0) return checkOrderList;
            sql = sql + sqlWhere + conditionSql;
            int dataNum = 1;
            using (IDataReader reader = DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    if (dataNum <= totalNum)
                    {
                        CheckOrder info = new CheckOrder
                        {
                            BillNo = LibSysUtils.ToString(reader["BILLNO"]),
                            RowId = LibSysUtils.ToInt32(reader["ROW_ID"]),
                            SupplierName = LibSysUtils.ToString(reader["SUPPLIERNAME"]),
                            MaterialId = LibSysUtils.ToString(reader["MATERIALID"]),
                            MaterialName = LibSysUtils.ToString(reader["MATERIALNAME"]),
                            AttributeDesc = LibSysUtils.ToString(reader["ATTRIBUTEDESC"]),
                            CheckType = GetCheckType(LibSysUtils.ToInt32(reader["CHECKTYPE"])),
                            WorkStationConfigId = LibSysUtils.ToString(reader["WORKSTATIONCONFIGID"]),
                            Quantity = LibSysUtils.ToDecimal(reader["QUANTITY"]),
                            CheckNum = LibSysUtils.ToDecimal(reader["CHECKNUM"]),
                            QualifiedNum = LibSysUtils.ToDecimal(reader["QUALIFIEDNUM"]),
                            UnQualifiedNum = LibSysUtils.ToDecimal(reader["UNQUALIFIEDNUM"]),
                            IsUserCheck = LibSysUtils.ToBoolean(reader["ISUSECHECK"]),
                            IsConfig = LibSysUtils.ToBoolean(reader["ISCONFIG"]),
                            CheckOrderType = type
                        };
                        if (type == 2)
                        {
                            info.PurchaseOrder = LibSysUtils.ToString(reader["PurchaseOrder"]);
                        }
                        checkOrderList.Add(info);
                        dataNum++;
                    }
                    else { return checkOrderList; }
                }
            }
            return checkOrderList;
        }

        /// <summary>
        /// 更新处理方式
        /// </summary>
        /// <param name="checkOrderType">质检单类型</param>
        /// <param name="dealWay">处理方式</param>
        /// <param name="billNo">质检单号</param>
        /// <param name="rowId">行标识</param>
        /// <returns></returns>
        public bool CheckDealWay(int checkOrderType, int dealWay, string billNo, int rowId)
        {
            bool result = true;
            List<string> lists = new List<string>();
            string sql;
            if (checkOrderType == 2)
            {
                sql = string.Format(@"UPDATE PURQUALITYCHECKDETAIL SET DEALWAY={0} WHERE BILLNO={1} AND ROW_ID={2}", dealWay, LibStringBuilder.GetQuotObject(billNo), rowId);
            }
            else
            {
                sql = string.Format(@"UPDATE OWQUALITYCHECKDETAIL SET DEALWAY={0} WHERE BILLNO={1} AND ROW_ID={2}", dealWay, LibStringBuilder.GetQuotObject(billNo), rowId);
            }
            lists.Add(sql);
            LibDBTransaction trans = DataAccess.BeginTransaction();
            try
            {
                DataAccess.ExecuteNonQuery(lists, false);
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 获取质检类型
        /// </summary>
        /// <param name="checkType">质检类型标识0：抽检 1：全检 2：免检</param>
        /// <returns></returns>
        public string GetCheckType(int checkType)
        {
            string result;
            switch (checkType)
            {
                case 0: result = "抽检"; break;
                case 1: result = "全检"; break;
                case 2: result = "免检"; break;
                default: result = ""; break;
            }
            return result;
        }

        /// <summary>
        /// 获取站点信息
        /// </summary>
        /// <param name="configId">站点配置编号</param>
        /// <returns></returns>
        public WorkstationInfo GetWorkstation(string configId)
        {
            WorkstationInfo workstationInfo = new WorkstationInfo
            {
                BadnessSetting = GetBadnessSetting(configId),
                WorkstationConfig = GetCheckConfig(configId)
            };
            return workstationInfo;
        }

        /// <summary>
        /// 获取检测站点配置
        /// </summary>
        /// <param name="configId">站点配置编号</param>
        /// <returns></returns>
        public WorkstationConfig GetCheckConfig(string configId)
        {
            WorkstationConfig config = new WorkstationConfig();
            SqlBuilder sqlBuilder = new SqlBuilder("com.WorkstationConfig");
            string sql = sqlBuilder.GetQuerySql(0, "A.WORKSTATIONTYPE,A.ALLOWCHANGEDATA,A.NEEDTAKEBADNESS,A.ISCOMBINE,A.SCANANY,A.ISACCURATECHECK",
                string.Format("A.WORKSTATIONCONFIGID={0}", LibStringBuilder.GetQuotString(configId)));
            Dictionary<int, ScanBarcode> tempDic = new Dictionary<int, ScanBarcode>();
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    config.WorkstationType = (WorkstationType)LibSysUtils.ToInt32(reader["WORKSTATIONTYPE"]);
                    config.AllowChangeData = LibSysUtils.ToBoolean(reader["ALLOWCHANGEDATA"]);
                    config.NeedTakeBadness = LibSysUtils.ToBoolean(reader["NEEDTAKEBADNESS"]);
                    config.IsCombine = LibSysUtils.ToBoolean(reader["ISCOMBINE"]);
                    config.ScanAny = LibSysUtils.ToBoolean(reader["SCANANY"]);
                    config.IsAccurateCheck = LibSysUtils.ToBoolean(reader["ISACCURATECHECK"]);
                }
            }
            sql = sqlBuilder.GetQuerySql(1, "B.ROW_ID,B.BARCODETYPEID,B.BARCODETYPENAME,B.BARCODERULEID,B.ISFROM,B.ISMASTER,B.BARCODERULEID<A.BARCODELENGTH,B.BARCODETYPEID<A.CHECKMATERIAL",
                string.Format("B.WORKSTATIONCONFIGID={0}", LibStringBuilder.GetQuotString(configId)), "B.ROWNO ASC");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    ScanBarcode scanBarcode = new ScanBarcode()
                    {
                        BarcodeTypeId = LibSysUtils.ToString(reader["BARCODETYPEID"]),
                        BarcodeTypeName = LibSysUtils.ToString(reader["BARCODETYPENAME"]),
                        BarcodeLength = LibSysUtils.ToInt32(reader["BARCODELENGTH"]),
                        CheckMaterial = LibSysUtils.ToBoolean(reader["CHECKMATERIAL"]),
                        IsMaster = LibSysUtils.ToBoolean(reader["ISMASTER"]),
                        IsFrom = LibSysUtils.ToBoolean(reader["ISFROM"])
                    };
                    string barcodeRuleId = LibSysUtils.ToString(reader["BARCODERULEID"]);
                    if (!string.IsNullOrEmpty(barcodeRuleId))
                    {
                        BarcodeRule barcodeRule = LibBarcodeRuleCache.Default.GetBarcodeRule(barcodeRuleId);
                        if (barcodeRule != null)
                        {
                            foreach (var item in barcodeRule.Items.Where(item => item.SectionType == BarcodeRuleSectionType.None))
                            {
                                scanBarcode.BarcodeFixCode.Add(new BarcodeFixCode(item.Start, item.Value));
                            }
                        }
                    }
                    config.ScanBarcode.Add(scanBarcode);
                    int rowId = LibSysUtils.ToInt32(reader["ROW_ID"]);
                    if (!tempDic.ContainsKey(rowId))
                        tempDic.Add(rowId, scanBarcode);
                }
            }
            sql = sqlBuilder.GetQuerySql(3, "D.PARENTROWID,D.FIELDNAME,D.PRINTNUM,D.BARCODERULEID,D.AUTOBUILD,D.ISMASTER,D.BARCODETYPEID,D.BARCODETYPENAME",
                string.Format("D.WORKSTATIONCONFIGID={0}", LibStringBuilder.GetQuotString(configId)), "D.ROWNO ASC");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    int parentRowId = LibSysUtils.ToInt32(reader["PARENTROWID"]);
                    if (!tempDic.ContainsKey(parentRowId)) continue;
                    string name = LibSysUtils.ToString(reader["FIELDNAME"]);
                    string labelTemplateId = string.Empty;
                    if (!string.IsNullOrEmpty(name))
                        labelTemplateId = "A001";
                    tempDic[parentRowId].LabelTemplateDetail.Add(new LabelTemplateDetail(labelTemplateId, LibSysUtils.ToString(reader["BARCODETYPEID"]),
                        LibSysUtils.ToString(reader["BARCODETYPENAME"]), LibSysUtils.ToInt32(reader["PRINTNUM"]), LibSysUtils.ToString(reader["BARCODERULEID"]), LibSysUtils.ToBoolean(reader["AUTOBUILD"]),
                        LibSysUtils.ToBoolean(reader["ISMASTER"])));
                }
            }
            Dictionary<string, Dictionary<string, CheckItem>> ckDic = new Dictionary<string, Dictionary<string, CheckItem>>();
            sql = string.Format(@"SELECT DISTINCT A.CHECKSTID,B.CHECKSTNAME,C.CHECKITEMID,D.CHECKITEMNAME,D.ISFILL,D.CHECKITEMTYPE,E.UPLIMIT,E.LOWLIMIT,E.STANDARD,E.DEFECTID,F.DEFECTNAME FROM COMWSCONFIGCSDETAIL A LEFT JOIN CHECKSOLUTION B ON A.CHECKSTID = B.CHECKSTID
                                    LEFT JOIN CHECKSOLUTIONDETAIL C ON B.CHECKSTID = C.CHECKSTID
                                    LEFT JOIN CHECKITEM D ON C.CHECKITEMID = D.CHECKITEMID
                                    LEFT JOIN CHECKSTBADNESS E ON C.CHECKSTID = E.CHECKSTID AND C.ROW_ID = E.PARENTROWID
                                    LEFT JOIN COMDEFECT F ON E.DEFECTID = F.DEFECTID
                                    WHERE A.WORKSTATIONCONFIGID =  '{0}'", LibSysUtils.ToString(configId));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string checkStid = LibSysUtils.ToString(reader["CHECKSTID"]);
                    string checkStname = LibSysUtils.ToString(reader["CHECKSTNAME"]);
                    string ckKey = checkStid + "-" + checkStname;
                    string checkItemId = LibSysUtils.ToString(reader["CHECKITEMID"]);
                    CheckItemBadness ckBadness = new CheckItemBadness
                    {
                        UpLimit = LibSysUtils.ToDecimal(reader["UPLIMIT"]),
                        LowLimit = LibSysUtils.ToDecimal(reader["LOWLIMIT"]),
                        Standard = LibSysUtils.ToDecimal(reader["STANDARD"]),
                        BadnessId = LibSysUtils.ToString(reader["DEFECTID"]),
                        BadnessName = LibSysUtils.ToString(reader["DEFECTNAME"])
                    };
                    if (!ckDic.ContainsKey(ckKey))
                    {
                        Dictionary<string, CheckItem> dic = new Dictionary<string, CheckItem>();
                        CheckItem checkItem = new CheckItem
                        {
                            CheckItemId = checkItemId,
                            CheckItemName = LibSysUtils.ToString(reader["CHECKITEMNAME"]),
                            CheckItemType = LibSysUtils.ToInt32(reader["CHECKITEMTYPE"]),
                            IsFill = LibSysUtils.ToBoolean(reader["ISFILL"])
                        };
                        checkItem.CheckItemBadness.Add(ckBadness);
                        dic.Add(checkItemId, checkItem);
                        ckDic.Add(ckKey, dic);
                    }
                    else
                    {
                        if (!ckDic[ckKey].ContainsKey(checkItemId))
                        {
                            CheckItem checkItem = new CheckItem
                            {
                                CheckItemId = checkItemId,
                                CheckItemName = LibSysUtils.ToString(reader["CHECKITEMNAME"]),
                                CheckItemType = LibSysUtils.ToInt32(reader["CHECKITEMTYPE"]),
                                IsFill = LibSysUtils.ToBoolean(reader["ISFILL"])
                            };
                            checkItem.CheckItemBadness.Add(ckBadness);
                            ckDic[ckKey].Add(checkItemId, checkItem);
                        }
                        else
                        {
                            ckDic[ckKey][checkItemId].CheckItemBadness.Add(ckBadness);
                        }

                    }
                }
            }
            foreach (KeyValuePair<string, Dictionary<string, CheckItem>> pair in ckDic)
            {
                CheckSolution ck = new CheckSolution
                {
                    CheckSID = pair.Key.Split('-')[0],
                    CheckSName = pair.Key.Split('-')[1]
                };
                foreach (KeyValuePair<string, CheckItem> itempair in pair.Value)
                {
                    ck.CheckItem.Add(itempair.Value);
                }
                config.CheckSolution.Add(ck);
            }
            return config;
        }

        /// <summary>
        /// 条码检查
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <param name="billNo">单据编号</param>
        /// <param name="checkOrderType">质检单类型</param>
        /// <returns></returns>
        public BarcodeCheckResult CheckBarcodeCK(string barcode, string billNo, int checkOrderType)
        {
            BarcodeCheckResult barcodeCheckResult = new BarcodeCheckResult();
            ExecCodeEnum execCode = ExecCodeEnum.Success;
            if (!string.IsNullOrEmpty(billNo))
            {
                Dictionary<string, object> outputData;
                var sqlPro = GetTableName(TableType.CheckBarcodeCK, checkOrderType);
                DataAccess.ExecuteStoredProcedure(sqlPro, out outputData, billNo, barcode, 0);
                execCode = (ExecCodeEnum)LibSysUtils.ToInt32(outputData["EXECCODE_VAL"]);
                if (execCode == ExecCodeEnum.Existed)
                {
                    ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("{0}在质检单{1}下已经过检验", barcode, billNo));
                }
            }
            else
            {
                execCode = ExecCodeEnum.MoreWorkOrder;
                ManagerMessage.AddMessage(LibMessageKind.Error, "请选择质检单");
            }
            barcodeCheckResult.ExecCode = execCode;
            return barcodeCheckResult;
        }

        /// <summary>
        /// 写入检验数据
        /// </summary>
        /// <param name="barcodeData">条码信息</param>
        /// <param name="checkSolution">检测方案</param>
        /// <param name="checkOrderType">质检单类型</param>
        /// <param name="isCheck">是否检验</param>
        /// <param name="isCheckDetail">是否精确检测</param>
        public void WriteBarcodeDataCK(BarcodeData barcodeData, CheckSolution checkSolution, int checkOrderType, bool isCheck = false, bool isCheckDetail = false)
        {
            long currentTime = LibDateUtils.GetCurrentDateTime();
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {

                UpdateCheckOrderNum(barcodeData.BillNo, barcodeData.IsPass, true, barcodeData.WorkProcessNo, checkOrderType, 0, LibDateUtils.GetCurrentDateTime());
                string sqlPro = GetTableName(TableType.WriteBarcode, checkOrderType);
                DataAccess.ExecuteStoredProcedure(sqlPro, barcodeData.Barcode, barcodeData.WorkProcessNo, barcodeData.BillNo, currentTime, barcodeData.IsPass,
                    barcodeData.BarcodeTypeId, barcodeData.BarcodeTypeName, barcodeData.WorkstationId, barcodeData.WorkstationName, barcodeData.WorkshopSectionId, barcodeData.WorkshopSectionName,
                    barcodeData.WorkProcessId, barcodeData.WorkProcessName, barcodeData.ProduceLineId, barcodeData.ProduceLineName, barcodeData.CreatorId, barcodeData.CreatorName, 0);

                if (isCheckDetail)
                {
                    sqlPro = GetTableName(TableType.WriteCheckItem, checkOrderType);
                    foreach (CheckItem item in checkSolution.CheckItem)
                    {
                        DataAccess.ExecuteStoredProcedure(sqlPro, barcodeData.Barcode, barcodeData.WorkProcessNo, barcodeData.BillNo, currentTime, item.IsPass,
                        checkSolution.CheckSID, checkSolution.CheckSName, item.CheckItemId, item.CheckItemName, item.CheckItemType, item.CheckValue, barcodeData.CreatorId, barcodeData.CreatorName);
                    }
                }
                if (barcodeData.BandnessList.Count > 0)
                {
                    sqlPro = GetTableName(TableType.WriteBarcodeBad, checkOrderType);
                    HashSet<string> set = new HashSet<string>();
                    foreach (var item in barcodeData.BandnessList)
                    {
                        if (set.Contains(item.BadnessId))
                            continue;
                        set.Add(item.BadnessId);
                        DataAccess.ExecuteStoredProcedure(sqlPro, barcodeData.Barcode, barcodeData.WorkProcessNo, barcodeData.BillNo, currentTime, item.BadnessId, item.BadnessName);
                    }
                }
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 创建派工单
        /// </summary>
        /// <param name="producelineId">生产线编号</param>
        /// <param name="techrouteId">工艺路线编号</param>
        /// <param name="typeId">派工单单据类型</param>
        /// <param name="checkOrder">质检单信息</param>
        /// <param name="fromId">来源类型</param>
        /// <returns></returns>
        public string CreateWorkOrder(string producelineId, string techrouteId, string typeId, CheckOrder checkOrder, int fromId)
        {
            try
            {
                List<int> rowList = new List<int>();
                this.ManagerMessage.MessageList.Clear();
                LibBcfData bcfData = (LibBcfData)LibBcfSystem.Default.GetBcfInstance("pp.WorkOrder");
                LibEntryParam entryParam = new LibEntryParam();
                entryParam.ParamStore.Add("TYPEID", typeId);
                DataSet ds = bcfData.AddNew(entryParam);
                DataRow masterRow = ds.Tables[0].Rows[0];
                string billNo = LibSysUtils.ToString(masterRow["BILLNO"]);
                masterRow["MATERIALID"] = checkOrder.MaterialId;
                masterRow["QUANTITY"] = checkOrder.CheckNum;
                masterRow["FROMBILLNO"] = checkOrder.BillNo;
                masterRow["FROMROWID"] = checkOrder.RowId;
                masterRow["ISFROMPLAN"] = fromId;
                masterRow["PRODUCELINEID"] = producelineId;
                masterRow["TECHROUTEID"] = techrouteId;
                masterRow["WORKORDERSTATE"] = 1;
                string sql = string.Format(@"SELECT A.ROW_ID,A.WORKPROCESSNO,A.WORKPROCESSID,A.WORKSHOPSECTIONID,A.WORKSTATIONCONFIGID,A.NEEDGATHER,A.TRANSFERWORKPROCESSNO,A.DOWORKPROCESS,C.WORKSTATIONID FROM COMTECHROUTEDETAIL A
LEFT JOIN COMPRODUCELINECONFIG B
ON A.WORKPROCESSID = B.WORKPROCESSID 
LEFT JOIN COMPRODUCELINESTATION C
ON B.RECORDID = C.RECORDID   WHERE B.PRODUCELINEID = '{0}' AND A.TECHROUTEID = '{1}' ORDER BY A.ROW_ID", producelineId, techrouteId);
                DataTable detail = ds.Tables[2];
                DataTable sub = ds.Tables[3];
                int i = 0;
                int j = 1;
                ds.EnforceConstraints = false;
                try
                {
                    detail.BeginLoadData();
                    sub.BeginLoadData();
                    try
                    {
                        using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                        {
                            while (reader.Read())
                            {
                                int fromRowId = LibSysUtils.ToInt32(reader["ROW_ID"]);
                                string workstationId = LibSysUtils.ToString(reader["WORKSTATIONID"]);
                                if (!rowList.Contains(fromRowId))
                                {
                                    i++;
                                    DataRow newRow = detail.NewRow();
                                    newRow["BILLNO"] = billNo;
                                    newRow["ROW_ID"] = i;
                                    newRow["ROWNO"] = i;
                                    newRow["NEEDGATHER"] = LibSysUtils.ToBoolean(reader["NEEDGATHER"]);
                                    newRow["FROMROWID"] = LibSysUtils.ToInt32(reader["ROW_ID"]);
                                    newRow["WORKPROCESSNO"] = LibSysUtils.ToInt32(reader["WORKPROCESSNO"]);
                                    newRow["WORKSHOPSECTIONID"] = LibSysUtils.ToString(reader["WORKSHOPSECTIONID"]);
                                    newRow["WORKPROCESSID"] = LibSysUtils.ToString(reader["WORKPROCESSID"]);
                                    newRow["WORKSTATIONCONFIGID"] = LibSysUtils.ToString(reader["WORKSTATIONCONFIGID"]);
                                    newRow["TRANSFERWORKPROCESSNO"] = LibSysUtils.ToInt32(reader["TRANSFERWORKPROCESSNO"]);
                                    newRow["DOWORKPROCESS"] = LibSysUtils.ToBoolean(reader["DOWORKPROCESS"]);
                                    if (!string.IsNullOrEmpty(workstationId))
                                    {
                                        newRow["WORKSTATIONDETAIL"] = 1;
                                    }
                                    detail.Rows.Add(newRow);
                                }
                                if (!string.IsNullOrEmpty(workstationId))
                                {
                                    DataRow subRow = sub.NewRow();
                                    subRow["BILLNO"] = billNo;//PARENTROWID
                                    subRow["PARENTROWID"] = i;
                                    subRow["ROW_ID"] = j;
                                    subRow["ROWNO"] = j;
                                    subRow["WORKSTATIONID"] = workstationId;
                                    sub.Rows.Add(subRow);
                                    j++;
                                }
                            }
                        }
                    }
                    finally
                    {
                        detail.EndLoadData();
                        sub.EndLoadData();
                    }
                }
                finally
                {
                    ds.EnforceConstraints = true;
                }
                bcfData.InnerSave(BillAction.AddNew, null, ds);
                if (checkOrder.CheckOrderType == 2)
                {
                    sql = string.Format("UPDATE PURQUALITYCHECKDETAIL SET WORKORDERNO = '{0}' WHERE BILLNO = '{1}' AND ROW_ID = {2}", billNo, checkOrder.BillNo, checkOrder.RowId);
                }
                else
                {
                    sql = string.Format("UPDATE OWQUALITYCHECKDETAIL SET WORKORDERNO = '{0}' WHERE BILLNO = '{1}' AND ROW_ID = {2}", billNo, checkOrder.BillNo, checkOrder.RowId);
                }
                DataAccess.ExecuteNonQuery(sql);
                if (!bcfData.ManagerMessage.IsThrow) return billNo;
                foreach (LibMessage msg in bcfData.ManagerMessage.MessageList)
                {
                    ManagerMessage.AddMessage(msg);
                }
                return billNo;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 获取主数据
        /// </summary>
        /// <param name="progId">ProgId</param>
        /// <param name="where">SQL条件</param>
        /// <returns></returns>
        public List<ComData> GetComData(string progId, string where)
        {
            List<ComData> comList = new List<ComData>();
            string sql = string.Empty;
            switch (progId)
            {
                case "com.TechRoute": sql = "SELECT TECHROUTEID AS ID,TECHROUTENAME AS NAME FROM COMTECHROUTE";
                    if (!string.IsNullOrEmpty(where))
                    {
                        sql += " WHERE MATERIALID = '" + where + "'";
                    }
                    ; break;
                case "com.ProduceLineConfig":
                    sql = "SELECT PRODUCELINEID AS ID,PRODUCELINENAME AS NAME FROM COMPRODUCELINE WHERE ISTECHROUTELINE =1";
                    break;
                case "pp.WorkOrderType":
                    sql = "SELECT TYPEID AS ID,TYPENAME AS NAME FROM PPWORKORDERTYPE";
                    break;
                default: break;
            }
            if (!string.IsNullOrEmpty(sql))
            {
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        comList.Add(new ComData() { Id = LibSysUtils.ToString(reader["ID"]), Name = LibSysUtils.ToString(reader["NAME"]) });
                    }
                }
            }
            return comList;
        }

        /// <summary>
        /// 更新检验数量
        /// </summary>
        /// <param name="orderNo">单据编号</param>
        /// <param name="isPass">是否通过</param>
        /// <param name="isCheckOrder">是否检验</param>
        /// <param name="rowId">行标识</param>
        /// <param name="checkOrderType">质检单类型</param>
        /// <param name="workProcessNo">工序号</param>
        /// <param name="nodate">时间</param>
        public void UpdateCheckOrderNum(string orderNo, bool isPass, bool isCheckOrder, int rowId, int checkOrderType, int workProcessNo, long nodate)
        {
            if (!isCheckOrder)
            {
                ProduceData produceData = LibProduceCache.Default.GetProduceData(orderNo);
                DataSet ds = produceData.WorkOrder;
                orderNo = LibSysUtils.ToString(ds.Tables[0].Rows[0]["FROMBILLNO"]);
                rowId = LibSysUtils.ToInt32(ds.Tables[0].Rows[0]["FROMROWID"]);
                checkOrderType = LibSysUtils.ToInt32(ds.Tables[0].Rows[0]["ISFROMPLAN"]);
                if (produceData.LastWorkProcessNo != workProcessNo && isPass)
                {
                    if (!CheckIsFinished(orderNo, rowId, true, checkOrderType))
                    {
                        throw new Exception();
                    }
                    return;
                }
            }
            if (string.IsNullOrEmpty(orderNo)) return;
            Dictionary<string, object> outputData;
            DataAccess.ExecuteStoredProcedure("wsCheckOrderNum", out outputData, orderNo, rowId, isPass, checkOrderType, nodate, 0);
            ExecCodeEnum execCode = (ExecCodeEnum)LibSysUtils.ToInt32(outputData["EXECCODE_VAL"]);
            if (execCode == ExecCodeEnum.NotDataInPreWP)
            {
                ManagerMessage.AddMessage(LibMessageKind.Error, "该张派工单对于的质检记录已完成，请重新选择其他派工单");
            }
            else if (execCode == ExecCodeEnum.NotPassInPreWP)
            {
                ManagerMessage.AddMessage(LibMessageKind.Error, "该张派工单对于的质检记录不存在，请重新选择其他派工单");
            }
        }

        /// <summary>
        /// 检查当前质检数据是否完成
        /// </summary>
        /// <param name="billNo">单据编号</param>
        /// <param name="rowId">行标识</param>
        /// <param name="isCheckOrder">是否质检单</param>
        /// <param name="checkOrderType">质检单类型</param>
        /// <returns></returns>
        public bool CheckIsFinished(string billNo, int rowId, bool isCheckOrder, int checkOrderType)
        {
            bool workorder = false;
            if (!isCheckOrder)
            {
                ProduceData produceData = LibProduceCache.Default.GetProduceData(billNo);
                DataSet ds = produceData.WorkOrder;
                billNo = LibSysUtils.ToString(ds.Tables[0].Rows[0]["FROMBILLNO"]);
                rowId = LibSysUtils.ToInt32(ds.Tables[0].Rows[0]["FROMROWID"]);
                checkOrderType = LibSysUtils.ToInt32(ds.Tables[0].Rows[0]["ISFROMPLAN"]);
                isCheckOrder = true;
                workorder = true;
            }
            if (!string.IsNullOrEmpty(billNo))
            {
                Dictionary<string, object> outputData;
                DataAccess.ExecuteStoredProcedure("wsCheckIsFinished", out outputData, billNo, rowId, isCheckOrder, checkOrderType, 0);
                ExecCodeEnum execCode = (ExecCodeEnum)LibSysUtils.ToInt32(outputData["EXECCODE_VAL"]);
                if (execCode == ExecCodeEnum.NotDataInPreWP)
                {
                    ManagerMessage.AddMessage(LibMessageKind.Error,
                        !workorder ? "该条质检记录已完成，请重新选择质检记录" : "该张派工单对于的质检记录已完成，请重新选择其他派工单");
                }
                else if (execCode == ExecCodeEnum.NotPassInPreWP)
                {
                    ManagerMessage.AddMessage(LibMessageKind.Error,
                        !workorder ? "该条质检记录不存在，请重新选择质检记录" : "该张派工单对于的质检记录不存在，请重新选择其他派工单");
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
            return false;
        }
        #endregion

        #region [二开]
        /// <summary>
        /// 从采购质检单获取 单据编号、接收时间、供应商名称
        /// </summary>
        /// <param name="pagestart">返回的起始位置</param>
        /// <param name="pagesize">每次返回的条数</param>
        /// <returns>放有单据编号、接收时间、供应商名称的list</returns>
        public List<CheckOrderInfo> GetSpecMatList(int pagestart, int pagesize)
        {
            List<CheckOrderInfo> checkOrder = new List<CheckOrderInfo>();
            List<CheckOrderInfo> ReturnList = new List<CheckOrderInfo>();
            string sql = @"SELECT DISTINCT A.BILLNO,
                            A.RECEIVEDATE,
                            A.SUPPLIERID,
                            C.SUPPLIERNAME,
                            B.STARTTIME
                            FROM PURQUALITYCHECK A
                            LEFT JOIN PURQUALITYCHECKDETAIL B
                            ON A.BILLNO = B.BILLNO
                            LEFT JOIN COMSUPPLIER C
                            ON A.SUPPLIERID = C.SUPPLIERID
                            LEFT JOIN COMMATERIAL D
                            ON D.MATERIALID = B.MATERIALID
                            WHERE B.ISFINISHED = 0
                            AND D.MATERIALTYPEID IN ('15001', '15002')
                            GROUP BY A.BILLNO,
                            A.RECEIVEDATE,
                            C.SUPPLIERNAME,
                            A.SUPPLIERID,
                            B.STARTTIME";    //15001、15002 只有玻璃和雕花件能够载入
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                int i = 0;
                while (reader.Read())
                {
                    CheckOrderInfo checkdetail = new CheckOrderInfo();
                    checkdetail.BillNo = LibSysUtils.ToString(reader["BILLNO"]);
                    checkdetail.Receivedate = LibSysUtils.ToString(reader["RECEIVEDATE"]);
                    checkdetail.SuppierId = LibSysUtils.ToString(reader["SUPPLIERID"]);
                    checkdetail.SupplierName = LibSysUtils.ToString(reader["SUPPLIERNAME"]);
                    checkdetail.Starttime = LibSysUtils.ToInt64(reader["STARTTIME"]);
                    checkdetail.RowId = i++;
                    checkOrder.Add(checkdetail);
                }
            }
            foreach (CheckOrderInfo item in checkOrder)
            {
                if (item.RowId >= pagestart && item.RowId <= (pagestart + pagesize))
                {
                    ReturnList.Add(item);
                }
                if (ReturnList.Count > 0)
                {
                    ReturnList[pagestart].Count = checkOrder.Count;
                }
            }
            return ReturnList;
        }

        /// <summary>
        /// 根据采购质检单号、供应商ID、接收时间查询 采购质检单明细
        /// </summary>
        /// <param name="billno">采购质检单号</param>
        /// <param name="supplierid">供应商编号</param>
        /// <param name="receivedate">接收时间</param>
        /// <param name="sqlwhere"></param>
        /// <returns>采购质检单明细</returns>
        public List<CheckOrderInfo> GetSpecMatDetailList(string billno, string supplierid, string receivedate, string sqlwhere)
        {
            List<CheckOrderInfo> ReturnList = new List<CheckOrderInfo>();
            string sql = string.Format(@"SELECT A.MATERIALID,
                                           B.MATERIALNAME,
                                           A.ATTRIBUTEDESC,
                                           A.QUANTITY,
                                           A.BATCHNO,
                                           A.BILLNO,
                                           A.ROW_ID,
                                           A.STARTTIME,
                                           E.BILLNO AS FROMBILLNO,
                                           E.ROW_ID AS FROMROWID
                                      FROM PURQUALITYCHECKDETAIL A
                                      LEFT JOIN COMMATERIAL B
                                        ON A.MATERIALID = B.MATERIALID
                                      LEFT JOIN PURQUALITYCHECK C
                                        ON A.BILLNO = C.BILLNO
                                      LEFT JOIN STKDELIVERYNOTEDETAIL D
                                      ON A.FROMBILLNO = D.BILLNO AND A.FROMROWID = D.ROW_ID
                                      LEFT JOIN PURPURCHASEPLANDETAIL E
                                      ON D.FROMBILLNO = E.BILLNO AND D.FROMROWID = E.ROW_ID 
                                     WHERE A.ISFINISHED = 0
                                       AND A.BILLNO = {0}
                                       AND C.SUPPLIERID = {1}
                                       AND C.RECEIVEDATE = {2}
                                       AND E.PURCHASEORDER LIKE '%{3}%'
                                       ", LibStringBuilder.GetQuotString(billno), LibStringBuilder.GetQuotString(supplierid), Convert.ToInt64(receivedate), sqlwhere);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    CheckOrderInfo checkdetail = new CheckOrderInfo();
                    checkdetail.BillNo = LibSysUtils.ToString(reader["BILLNO"]);
                    checkdetail.RowId = LibSysUtils.ToInt32(reader["ROW_ID"]);
                    checkdetail.MaterialId = LibSysUtils.ToString(reader["MATERIALID"]);
                    checkdetail.MaterialName = LibSysUtils.ToString(reader["MATERIALNAME"]);
                    checkdetail.Quantity = LibSysUtils.ToDecimal(reader["QUANTITY"]);
                    checkdetail.AttributeDesc = LibSysUtils.ToString(reader["ATTRIBUTEDESC"]);
                    checkdetail.BatchNo = LibSysUtils.ToString(reader["BATCHNO"]);
                    checkdetail.QualifiedNum = LibSysUtils.ToDecimal(reader["QUANTITY"]);
                    checkdetail.Starttime = LibSysUtils.ToInt64(reader["STARTTIME"]);
                    checkdetail.UnQualifiedNum = 0;
                    checkdetail.FromBillNo = LibSysUtils.ToString(reader["FROMBILLNO"]);//采购计划 单号
                    checkdetail.FromRowId = LibSysUtils.ToInt32(reader["FROMROWID"]);//采购计划单 行标识
                    ReturnList.Add(checkdetail);
                }
            }
            return ReturnList;
        }

        /// <summary>
        /// 开始时修改采购质检单和供应商到货信息表
        /// </summary>
        /// <param name="billno"></param>
        /// <param name="supplierid"></param>
        /// <param name="receivedate"></param>
        /// <returns></returns>
        public bool StartCheck(string billno, string supplierid, string receivedate)
        {
            List<CheckOrderInfo> getAllOrder = new List<CheckOrderInfo>();
            getAllOrder = GetSpecMatDetailList(billno, supplierid, receivedate, "");
            List<string> sqlList = new List<string>();
            foreach (CheckOrderInfo item in getAllOrder)
            {
                sqlList.Add(string.Format("UPDATE PURSUPPLYARRIVALINFO A SET CHECKSTARTTIME = {0} WHERE A.FROMBILLNO = {1} AND A.FROMROWID = {2}", LibDateUtils.GetCurrentDateTime(), LibStringBuilder.GetQuotString(item.FromBillNo), item.FromRowId));
                sqlList.Add(string.Format("UPDATE PURQUALITYCHECKDETAIL A SET A.STARTTIME = {0} WHERE BILLNO = {1} AND A.ROW_ID = {2}", LibDateUtils.GetCurrentDateTime(), LibStringBuilder.GetQuotString(item.BillNo), item.RowId));
            }
            try
            {
                int A = DataAccess.ExecuteNonQuery(sqlList, false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 结束时修改采购质检单和供应商到货信息表
        /// </summary>
        /// <param name="checkorder"></param>
        public bool EndCheck(CheckOrderInfo[] checkorder)
        {
            List<string> sqlList = new List<string>();
            foreach (CheckOrderInfo item in checkorder)
            {
                sqlList.Add(string.Format("UPDATE PURSUPPLYARRIVALINFO A SET A.CHECKGOODQTY = {0},A.CHECKBADQTY ={1},A.CHECKENDTIME = {2} WHERE A.FROMBILLNO = {3} AND A.FROMROWID = {4}", item.QualifiedNum, item.UnQualifiedNum, LibDateUtils.GetCurrentDateTime(), LibStringBuilder.GetQuotString(item.FromBillNo), item.FromRowId));
                sqlList.Add(string.Format("UPDATE PURQUALITYCHECKDETAIL A SET A.QUALIFIEDNUM = {0},A.UNQUALIFIEDNUM = {1},A.ENDTIME = {2},A.ISFINISHED = 1，A.DEALWAY = {3} WHERE BILLNO = {4} AND A.ROW_ID = {5}", item.QualifiedNum, item.UnQualifiedNum, LibDateUtils.GetCurrentDateTime(), item.DealWay, LibStringBuilder.GetQuotString(item.BillNo), item.RowId));
            }
            try
            {
                DataAccess.ExecuteNonQuery(sqlList, false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 确认该质检单全部内容
        /// </summary>
        /// <returns></returns>
        public bool CheckAll(string billno, string supplierid, string receivedate)
        {
            List<CheckOrderInfo> getAllOrder = new List<CheckOrderInfo>();
            List<string> sqllist = new List<string>();
            getAllOrder = GetSpecMatDetailList(billno, supplierid, receivedate, "");
            foreach (CheckOrderInfo item in getAllOrder)
            {
                sqllist.Add(string.Format(@" UPDATE PURQUALITYCHECKDETAIL A SET A.QUALIFIEDNUM = A.CHECKNUM,
                                        A.UNQUALIFIEDNUM = 0, A.ENDTIME = {0},
                                        A.ISFINISHED = 1，A.DEALWAY = 1 WHERE A.ISFINISHED = 0
                                        AND BILLNO = {1} AND ROW_ID = {2} ", LibDateUtils.GetCurrentDateTime(), LibStringBuilder.GetQuotString(item.BillNo), item.RowId));

                sqllist.Add(string.Format(@"UPDATE PURSUPPLYARRIVALINFO A
                                        SET A.CHECKGOODQTY = A.RECEIVEQTY, A.CHECKBADQTY = 0,A.CHECKENDTIME = {0}
                                        WHERE A.FROMBILLNO = {1}
                                        AND A.FROMROWID = {2} AND A.CHECKENDTIME = 0", LibDateUtils.GetCurrentDateTime(), LibStringBuilder.GetQuotString(item.FromBillNo), item.FromRowId));
            }
            try
            {
                int A = DataAccess.ExecuteNonQuery(sqllist, false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region[HY]

        /// <summary>
        /// 获取派工单编号
        /// </summary>
        /// <param name="userId">人员账号</param>
        /// <param name="workstationId">设备站点</param>
        /// <param name="oldBillNo">当前派工单号</param>
        /// <returns>下一个派工单号</returns>
        public string HYLogin(string userId, string workstationId, string oldBillNo)
        {
            string workOrderNo = string.Empty;
            List<string> billNoList = new List<string>();
            ProductScheduling wsPS = LibWsControlServer.Default.GetProductScheduling();
            HYProductScheduling hyPS = LibHYControlServer.Default.GetProductScheduling();
            string sql = string.Format(@"SELECT DISTINCT A.WORKORDERNO,
                                                        B.SENDSTARTTIME,
                                                        A.ORDERDATE,
                                                        A.FROMTYPE,
                                                        A.ORDERNUM
                                                        FROM PPMAINTENWORKRECORD A
                                                        LEFT JOIN PPTENWORKRECORD B
                                                        ON A.WORKORDERNO = B.WORKORDERNO AND A.FIRSTWORKPROCESSNO = B.WORKPROCESSNO
                                                        WHERE A.STARTSTATE = 1
                                                        ORDER BY B.SENDSTARTTIME, A.ORDERDATE, A.FROMTYPE, A.ORDERNUM");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    billNoList.Add(LibSysUtils.ToString(reader["WORKORDERNO"]));
                }
            }
            if (wsPS.WsRelWorkOrder.ContainsKey(workstationId))
            {
                int index = 0;
                if (billNoList.Contains(oldBillNo))
                {
                    for (int i = 0; i < billNoList.Count; i++)
                    {
                        if (billNoList[i] == oldBillNo)
                        {
                            index = i;
                        }
                    }
                }
                for (int i = index; i < billNoList.Count; i++)
                {
                    if (billNoList[i] == oldBillNo)
                        continue;
                    foreach (string billNo in wsPS.WsRelWorkOrder[workstationId])
                    {
                        if (billNoList[i] == billNo)
                        {
                            workOrderNo = billNo;
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(workOrderNo))
                    {
                        break;
                    }
                }
            }

            return workOrderNo;
        }

        /// <summary>
        /// 获取站点配置
        /// </summary>
        /// <param name="billNo">派工单号</param>
        /// <param name="workstationId">站点编号</param>
        /// <returns>站点配置（转入、协同工序等）</returns>
        public HYWorkStationConfig GetStationConfig(string billNo, string workstationId)
        {
            HYWorkStationConfig hc = new HYWorkStationConfig();
            string workstationConfigId = string.Empty;
            string producelineId = string.Empty;
            //string TechrouteId = string.Empty;
            string workProcessId = string.Empty;
            ProduceData produceData = LibProduceCache.Default.GetProduceData(billNo);
            if (produceData != null)
            {
                producelineId = LibSysUtils.ToString(produceData.WorkOrder.Tables[0].Rows[0]["PRODUCELINEID"]);
                //TechrouteId = LibSysUtils.ToString(produceData.WorkOrder.Tables[0].Rows[0]["TECHROUTEID"]);
                foreach (DataRow curRow in produceData.WorkOrder.Tables[3].Rows)
                {
                    if (string.Compare(LibSysUtils.ToString(curRow["WORKSTATIONID"]), workstationId, false) == 0)
                    {
                        DataRow parentRow = produceData.WorkOrder.Tables[2].Rows.Find(new object[] { curRow["BILLNO"], curRow["PARENTROWID"] });
                        int workProcessNo = LibSysUtils.ToInt32(parentRow["WORKPROCESSNO"]);
                        workstationConfigId = LibSysUtils.ToString(produceData.WorkProcessNo[workProcessNo].DataRow["WORKSTATIONCONFIGID"]);
                        workProcessId = LibSysUtils.ToString(produceData.WorkProcessNo[workProcessNo].DataRow["WORKPROCESSID"]);
                        break;
                    }
                }
            }
            SqlBuilder sqlBuilder = new SqlBuilder("com.WorkstationConfig");
            string sql = sqlBuilder.GetQuerySql(0, "A.ISWHOLE,A.ISAUTO,A.ISPOINT,A.ISMAINPRINT,A.ISLINKPRINT,A.PRINTCOUNT,A.ISCHANGEORDER,A.ISSINGLE",
                string.Format("A.WORKSTATIONCONFIGID={0}", LibStringBuilder.GetQuotString(workstationConfigId)));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    hc.IsWhole = LibSysUtils.ToBoolean(reader["ISWHOLE"]);
                    hc.IsAuto = LibSysUtils.ToBoolean(reader["ISAUTO"]);
                    hc.IsPoint = LibSysUtils.ToBoolean(reader["ISPOINT"]);
                    hc.IsMainPrint = LibSysUtils.ToBoolean(reader["ISMAINPRINT"]);
                    hc.IsLinkPrint = LibSysUtils.ToBoolean(reader["ISLINKPRINT"]);
                    hc.PrintCount = LibSysUtils.ToInt32(reader["PRINTCOUNT"]);
                    hc.IsChangeOrder = LibSysUtils.ToBoolean(reader["ISCHANGEORDER"]);
                    hc.IsSingle = LibSysUtils.ToBoolean(reader["ISSINGLE"]);
                }
            }
            sql = string.Format(@"SELECT B.WORKSTATIONID
                                FROM COMPRODUCELINECONFIG A
                                LEFT JOIN COMPRODUCELINESTATION B
                                ON A.RECORDID = B.RECORDID
                                WHERE A.PRODUCELINEID = '{0}'
                                AND B.TRANSFERWORKSTATIONID = '{1}'", producelineId, workstationId);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    hc.IsTransferWorkStationId = true;
                    hc.TransferWorkStationId = LibSysUtils.ToString(reader["WORKSTATIONID"]);
                }
            }
            sql = string.Format(@"SELECT A.BEATTIME, B.CPWORKPROCESSID
                                FROM COMPRODUCELINECONFIG A
                                LEFT JOIN COMPLCOOPERATE B
                                ON A.RECORDID = B.RECORDID
                                WHERE A.PRODUCELINEID = '{0}'
                                AND A.WORKPROCESSID = '{1}'", producelineId, workProcessId);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    if (hc.BeatTime == 0)
                    {
                        hc.BeatTime = LibSysUtils.ToInt32(reader["BEATTIME"]);
                    }
                    hc.CooperateWorkProcess.Add(LibSysUtils.ToString(reader["CPWORKPROCESSID"]));
                }
            }
            sql = string.Format(@"SELECT A.WORKPROCESSID
                                FROM COMPRODUCELINECONFIG A
                                LEFT JOIN COMPLCOOPERATE B
                                ON A.RECORDID = B.RECORDID
                                WHERE A.PRODUCELINEID = '{0}'
                                AND B.CPWORKPROCESSID = '{1}'
                                ", producelineId, workProcessId);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    hc.IsCooperateWorkProcess = true;
                    hc.CooperateMainWorkProcess = LibSysUtils.ToString(reader["WORKPROCESSID"]);
                }
            }

            return hc;
        }

        /// <summary>
        /// 获取当前站点数据
        /// </summary>
        /// <param name="workProcessNo">工序编号</param>
        /// <param name="billNo">派工单号</param>
        /// <param name="workstationId">站点编号</param>
        /// <param name="config">站点配置</param>
        /// <returns>缓存中的当前派工单符合当前站点的任务数据</returns>
        public List<TenWorkRecord> GetTenWorkRecord(int workProcessNo, string billNo, string workstationId, HYWorkStationConfig config)
        {
            List<string> taskNoList = new List<string>();
            //List<int> workProcessList = new List<int>();
            List<TenWorkRecord> tp = new List<TenWorkRecord>();
            HYProduceData hyProduce = LibHYProduceCache.Default.GetProduceData(billNo);
            ProduceData produce = LibProduceCache.Default.GetProduceData(billNo);
            if (hyProduce == null || produce == null)
                return tp;
            if (produce.FirstWorkProcessNo.Contains(workProcessNo))
            {
                if (!config.IsTransferWorkStationId && config.IsCooperateWorkProcess)
                {
                    DataRow[] dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSID = {0}  AND STATES <> 0 AND SALEORDERSTATE = 0 ", LibStringBuilder.GetQuotString(config.CooperateMainWorkProcess)), "STARTTIME ");
                    for (int i = 0; i < dataRows.Length; i++)
                    {
                        if (!taskNoList.Contains(LibSysUtils.ToString(dataRows[i]["TASKNO"])))
                        {
                            taskNoList.Add(LibSysUtils.ToString(dataRows[i]["TASKNO"]));
                        }
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    if (taskNoList.Count > 0)
                    {
                        stringBuilder.Append(" TASKNO IN ( ");
                        foreach (string taskNo in taskNoList)
                        {
                            stringBuilder.Append("'");
                            stringBuilder.Append(taskNo);
                            stringBuilder.Append("',");
                        }
                        string sqlWhere = stringBuilder.ToString().Substring(0, stringBuilder.ToString().LastIndexOf(','));
                        sqlWhere += " )";
                        if (config.IsPoint)
                        {
                            dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO = {0} AND COOPERATESTATE = 1  AND (STATES = 0 OR STATES = 1)  AND {1} AND WORKSTATIONID = {2} AND SALEORDERSTATE = 0", workProcessNo, sqlWhere, LibStringBuilder.GetQuotString(workstationId)));
                        }
                        else
                        {
                            dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO = {0} AND COOPERATESTATE = 1  AND (STATES = 0 OR STATES = 1)  AND {1} AND SALEORDERSTATE = 0", workProcessNo, sqlWhere));
                        }
                        foreach (string taskNo in taskNoList)
                        {
                            for (int i = 0; i < dataRows.Length; i++)
                            {
                                if (LibSysUtils.ToString(dataRows[i]["TASKNO"]) == taskNo)
                                {
                                    tp.Add(GetWorkRecord(dataRows[i]));
                                }
                            }
                        }
                    }
                }
                else if (config.IsTransferWorkStationId && config.IsCooperateWorkProcess)
                {
                    DataRow[] dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSID = {0} AND WORKSTATIONID = {1}  AND STATES <> 0  AND SALEORDERSTATE = 0", LibStringBuilder.GetQuotString(config.CooperateMainWorkProcess), LibStringBuilder.GetQuotString(config.TransferWorkStationId)), "STARTTIME ");
                    for (int i = 0; i < dataRows.Length; i++)
                    {
                        if (!taskNoList.Contains(LibSysUtils.ToString(dataRows[i]["TASKNO"])))
                        {
                            taskNoList.Add(LibSysUtils.ToString(dataRows[i]["TASKNO"]));
                        }
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    if (taskNoList.Count > 0)
                    {
                        stringBuilder.Append(" TASKNO IN ( ");
                        foreach (string taskNo in taskNoList)
                        {
                            stringBuilder.Append("'");
                            stringBuilder.Append(taskNo);
                            stringBuilder.Append("',");
                        }
                        string sqlWhere = stringBuilder.ToString().Substring(0, stringBuilder.ToString().LastIndexOf(','));
                        sqlWhere += " )";
                        if (config.IsPoint)
                        {
                            dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO = {0} AND COOPERATESTATE = 1  AND (STATES = 0 OR STATES = 1)  AND {1} AND WORKSTATIONID = {2} AND SALEORDERSTATE = 0", workProcessNo, sqlWhere, LibStringBuilder.GetQuotString(workstationId)));
                        }
                        else
                        {
                            dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO = {0} AND COOPERATESTATE = 1  AND (STATES = 0 OR STATES = 1)  AND {1} AND SALEORDERSTATE = 0", workProcessNo, sqlWhere));
                        }
                        foreach (string taskNo in taskNoList)
                        {
                            for (int i = 0; i < dataRows.Length; i++)
                            {
                                if (LibSysUtils.ToString(dataRows[i]["TASKNO"]) == taskNo)
                                {
                                    tp.Add(GetWorkRecord(dataRows[i]));
                                }
                            }
                        }
                    }
                }
                else
                {
                    DataRow[] dataRows = null;
                    if (config.IsPoint)
                    {
                        dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO = {0} AND WORKSTATIONID = {1}  AND (STATES = 0 OR STATES = 1)   AND SALEORDERSTATE = 0 ", workProcessNo, LibStringBuilder.GetQuotString(workstationId)), "ORDERINDEX,SCANNUM,TASKNO");
                    }
                    else
                    {
                        dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO = {0}  AND (STATES = 0 OR STATES = 1) AND SALEORDERSTATE = 0 ", workProcessNo), "ORDERINDEX,SCANNUM,TASKNO");
                    }
                    for (int i = 0; i < dataRows.Length; i++)
                    {
                        tp.Add(GetWorkRecord(dataRows[i]));
                    }
                }
                return tp;
            }
            else
            {
                if (config.IsTransferWorkStationId)
                {
                    int count = 0;
                    List<string> actualTaskNoList = new List<string>();
                    StringBuilder stringBuilder = new StringBuilder();
                    string sqlWhere = string.Empty;
                    WorkProcessInfo wp = produce.WorkProcessNo[workProcessNo];
                    foreach (int workNo in wp.PreWorkProcessNo)
                    {
                        actualTaskNoList.Clear();
                        DataRow[] dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO ={0} AND WORKSTATIONID = {1}  AND STATES = 5 AND SALEORDERSTATE = 0", workNo, LibStringBuilder.GetQuotString(config.TransferWorkStationId)), "FINISHTIME ");
                        for (int i = 0; i < dataRows.Length; i++)
                        {
                            if (count > 0)
                            {
                                if (taskNoList.Contains(LibSysUtils.ToString(dataRows[i]["TASKNO"])))
                                {
                                    actualTaskNoList.Add(LibSysUtils.ToString(dataRows[i]["TASKNO"]));
                                }
                            }
                            else
                            {
                                if (!taskNoList.Contains(LibSysUtils.ToString(dataRows[i]["TASKNO"])))
                                {
                                    actualTaskNoList.Add(LibSysUtils.ToString(dataRows[i]["TASKNO"]));
                                }
                            }
                        }
                        taskNoList.Clear();
                        foreach (string taskNo in actualTaskNoList)
                        {
                            if (!taskNoList.Contains(taskNo))
                            {
                                taskNoList.Add(taskNo);
                            }
                        }
                        count++;
                    }
                    if (taskNoList.Count > 0)
                    {
                        actualTaskNoList.Clear();
                        DataRow[] dataRows = null;
                        if (config.IsChangeOrder)
                        {
                            dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO = 5 AND STATES = 5 AND SALEORDERSTATE = 0"), "STARTTIME ");
                            foreach (DataRow row in dataRows)
                            {
                                string taskNo = LibSysUtils.ToString(row["TASKNO"]);
                                if (!actualTaskNoList.Contains(taskNo))
                                {
                                    actualTaskNoList.Add(taskNo);
                                }
                            }
                            for (int i = actualTaskNoList.Count - 1; i >= 0; i--)
                            {
                                if (!taskNoList.Contains(actualTaskNoList[i]))
                                {
                                    actualTaskNoList.RemoveAt(i);
                                }
                            }
                            taskNoList = actualTaskNoList;
                        }
                        stringBuilder.Clear();
                        stringBuilder.Append(" TASKNO IN ( ");
                        foreach (string taskNo in taskNoList)
                        {
                            stringBuilder.Append("'");
                            stringBuilder.Append(taskNo);
                            stringBuilder.Append("',");
                        }
                        sqlWhere = stringBuilder.ToString().Substring(0, stringBuilder.ToString().LastIndexOf(','));
                        sqlWhere += " )";
                        if (config.IsPoint)
                        {
                            dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO = {0} AND (STATES = 0 OR STATES = 1) AND {1} AND WORKSTATIONID = {2} AND SALEORDERSTATE = 0", workProcessNo, sqlWhere, LibStringBuilder.GetQuotString(workstationId)));
                        }
                        else
                        {
                            dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO = {0} AND (STATES = 0 OR STATES = 1) AND {1} AND SALEORDERSTATE = 0", workProcessNo, sqlWhere));
                        }
                        if (dataRows != null && dataRows.Length > 0)
                        {
                            foreach (string taskNo in taskNoList)
                            {
                                for (int i = 0; i < dataRows.Length; i++)
                                {
                                    if (LibSysUtils.ToString(dataRows[i]["TASKNO"]) == taskNo)
                                    {
                                        tp.Add(GetWorkRecord(dataRows[i]));
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    int count = 0;
                    List<string> actualTaskNoList = new List<string>();
                    StringBuilder stringBuilder = new StringBuilder();
                    string sqlWhere = string.Empty;
                    WorkProcessInfo wp = produce.WorkProcessNo[workProcessNo];
                    foreach (int workNo in wp.PreWorkProcessNo)
                    {
                        actualTaskNoList.Clear();
                        DataRow[] dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO ={0}  AND STATES = 5  AND SALEORDERSTATE = 0", workNo), "FINISHTIME");
                        for (int i = 0; i < dataRows.Length; i++)
                        {
                            if (count > 0)
                            {
                                if (taskNoList.Contains(LibSysUtils.ToString(dataRows[i]["TASKNO"])))
                                {
                                    actualTaskNoList.Add(LibSysUtils.ToString(dataRows[i]["TASKNO"]));
                                }
                            }
                            else
                            {
                                if (!taskNoList.Contains(LibSysUtils.ToString(dataRows[i]["TASKNO"])))
                                {
                                    actualTaskNoList.Add(LibSysUtils.ToString(dataRows[i]["TASKNO"]));
                                }
                            }
                        }
                        taskNoList.Clear();
                        foreach (string taskNo in actualTaskNoList)
                        {
                            if (!taskNoList.Contains(taskNo))
                            {
                                taskNoList.Add(taskNo);
                            }
                        }
                        count++;
                    }

                    if (taskNoList.Count > 0)
                    {
                        actualTaskNoList.Clear();
                        DataRow[] dataRows = null;
                        if (config.IsChangeOrder)
                        {
                            dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO = 5 AND STATES = 5 AND SALEORDERSTATE = 0"), "STARTTIME ");
                            foreach (DataRow row in dataRows)
                            {
                                string taskNo = LibSysUtils.ToString(row["TASKNO"]);
                                if (!actualTaskNoList.Contains(taskNo))
                                {
                                    actualTaskNoList.Add(taskNo);
                                }
                            }
                            for (int i = actualTaskNoList.Count - 1; i >= 0; i--)
                            {
                                if (!taskNoList.Contains(actualTaskNoList[i]))
                                {
                                    actualTaskNoList.RemoveAt(i);
                                }
                            }
                            taskNoList = actualTaskNoList;
                        }
                        stringBuilder.Clear();
                        stringBuilder.Append(" TASKNO IN ( ");
                        foreach (string taskNo in taskNoList)
                        {
                            stringBuilder.Append("'");
                            stringBuilder.Append(taskNo);
                            stringBuilder.Append("',");
                        }
                        sqlWhere = stringBuilder.ToString().Substring(0, stringBuilder.ToString().LastIndexOf(','));
                        sqlWhere += " )";
                        if (config.IsPoint)
                        {
                            dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO = {0} AND (STATES = 0 OR STATES = 1) AND {1} AND WORKSTATIONID = {2} AND SALEORDERSTATE = 0", workProcessNo, sqlWhere, LibStringBuilder.GetQuotString(workstationId)));
                        }
                        else
                        {
                            dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO = {0} AND (STATES = 0 OR STATES = 1) AND {1} AND SALEORDERSTATE = 0", workProcessNo, sqlWhere));
                        }
                        if (dataRows != null && dataRows.Length > 0)
                        {
                            foreach (string taskNo in taskNoList)
                            {
                                for (int i = 0; i < dataRows.Length; i++)
                                {
                                    if (LibSysUtils.ToString(dataRows[i]["TASKNO"]) == taskNo)
                                    {
                                        tp.Add(GetWorkRecord(dataRows[i]));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return tp;
        }

        /// <summary>
        /// 检查异常数据
        /// </summary>
        /// <param name="produceData">单个任务对象</param>
        /// <returns>是否为异常任务</returns>
        public bool CheckAbnormal(ProduceBcfData produceData)
        {
            HYProduceData hyProduce = LibHYProduceCache.Default.GetProduceData(produceData.BillNo);
            if (hyProduce == null)
                return false;
            DataRow[] dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format(" TASKNO = '{0}'", produceData.TaskNo));
            foreach (string link in produceData.LinkBarcode)
            {
                foreach (DataRow row in dataRows)
                {
                    string sourTaskNo = LibSysUtils.ToString(row["TASKNO"]);
                    string sourLinkBarcode = LibSysUtils.ToString(row["LINKBARCODE"]);
                    int workprocessNo = LibSysUtils.ToInt32(row["WORKPROCESSNO"]);
                    int saleOrderState = LibSysUtils.ToInt32(row["SALEORDERSTATE"]);
                    string workstationId = LibSysUtils.ToString(row["WORKSTATIONID"]);
                    if (saleOrderState == 0)
                    {
                        if (sourTaskNo == produceData.TaskNo && sourLinkBarcode == link && produceData.WorkProcessNo == workprocessNo && produceData.WorkstationId == workstationId)
                        {
                            if (LibSysUtils.ToInt32(row["STATES"]) == 6)
                            {
                                //this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前任务为异常任务,请做下线处理");
                                return true;
                            }
                        }
                    }
                    else
                    {
                        //if (LibSysUtils.ToInt32(row["STATES"]) == 6)
                        //{
                        //this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前任务为异常任务,请做下线处理");
                        return true;
                        //}
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 修改任务状态
        /// </summary>
        /// <param name="produceData">单个任务信息</param>
        public void UpdateTenWorkRecord(ProduceBcfData produceData)
        {
            long Time = LibDateUtils.GetCurrentDateTime();
            HYProduceData hyProduce = LibHYProduceCache.Default.GetProduceData(produceData.BillNo);
            if (hyProduce == null)
                return;
            foreach (string link in produceData.LinkBarcode)
            {
                foreach (DataRow row in hyProduce.TenWorkRecord.Tables[0].Rows)
                {
                    string sourTaskNo = LibSysUtils.ToString(row["TASKNO"]);
                    string sourLinkBarcode = LibSysUtils.ToString(row["LINKBARCODE"]);
                    int workprocessNo = LibSysUtils.ToInt32(row["WORKPROCESSNO"]);
                    if (sourTaskNo == produceData.TaskNo && sourLinkBarcode == link && produceData.WorkProcessNo == workprocessNo)
                    {
                        switch (produceData.ChangeType)
                        {
                            //锁定 过账协同工序列表
                            case ChangeType.Select:
                                row["STATES"] = 1;
                                row["STARTTIME"] = Time;
                                row["WORKSTATIONID"] = produceData.WorkstationId;
                                row["PERSONID"] = produceData.PersonId;
                                row["PERSONNAME"] = produceData.PersonName;
                                break;
                            //case ChangeType.Stop: row["STATES"] = 2; break;
                            //完成 过账完成时间和完成标识
                            case ChangeType.Finish:
                                row["STATES"] = 5;
                                row["FINISHTIME"] = Time;
                                break;
                            case ChangeType.Defect:
                                row["STATES"] = 6;
                                break;
                            case ChangeType.Back:
                                row["STATES"] = 0;
                                row["STARTTIME"] = 0;
                                row["WORKSTATIONID"] = produceData.Config.IsPoint ? produceData.WorkstationId : "";
                                row["PERSONID"] = "";
                                row["PERSONNAME"] = "";
                                break;
                            case ChangeType.Single:
                                row["FINISHNUM"] = LibSysUtils.ToInt32(row["FINISHNUM"]) + 1;
                                break;
                        }
                    }
                }
            }
            switch (produceData.ChangeType)
            {
                //锁定 过账协同工序列表
                case ChangeType.Select:
                    UpdateSelect(produceData, hyProduce, Time);
                    break;
                //case ChangeType.Stop: row["STATES"] = 2; break;
                //完成 过账完成时间和完成标识
                case ChangeType.Finish:
                    UpdateFinish(produceData, hyProduce, Time);
                    break;
                case ChangeType.Defect:
                    UpdateDefect(produceData, hyProduce);
                    break;
                case ChangeType.Back:
                    UpdateBack(produceData, hyProduce);
                    break;
                case ChangeType.Single:
                    UpdateSingle(produceData, hyProduce);
                    break;
            }
        }

        /// <summary>
        /// 锁定
        /// </summary>
        /// <param name="produceData">单个任务信息</param>
        /// <param name="hyProduce">当前派工单内的缓存数据</param>
        /// <param name="startTime">开始时间</param>
        private void UpdateSelect(ProduceBcfData produceData, HYProduceData hyProduce, long startTime)
        {
            List<string> sqlList = new List<string>();
            foreach (string linkBarcode in produceData.LinkBarcode)
            {
                sqlList.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STATES = 1,STARTTIME = {3},WORKSTATIONID = {5},PERSONID = {6},PERSONNAME = {7} WHERE WORKORDERNO = {0} AND TASKNO = {1} AND  LINKBARCODE = {2} AND WORKPROCESSNO = {4}", LibStringBuilder.GetQuotString(produceData.BillNo), LibStringBuilder.GetQuotString(produceData.TaskNo),
                LibStringBuilder.GetQuotString(linkBarcode), startTime, produceData.WorkProcessNo,
                LibStringBuilder.GetQuotString(produceData.WorkstationId), LibStringBuilder.GetQuotString(produceData.PersonId)
                , LibStringBuilder.GetQuotString(produceData.PersonName)));
            }
            ProduceData produce = LibProduceCache.Default.GetProduceData(produceData.BillNo);
            DataRow[] dataRowArray = null;
            if (produceData.WorkProcessNo == produce.FirstWorkProcessNo[0])
            {
                dataRowArray = hyProduce.TenWorkRecord.Tables[0].Select(string.Format(@"STATES = 1 AND WORKPROCESSNO = {0}", produceData.WorkProcessNo));
                if (dataRowArray.Length == produceData.LinkBarcode.Count)
                {
                    sqlList.Add(string.Format("UPDATE PPMAINTENWORKRECORD SET STARTTIME = {1} WHERE WORKORDERNO = {0}", LibStringBuilder.GetQuotString(produceData.BillNo),LibDateUtils.GetCurrentDateTime()));
                }
            }
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                int result = this.DataAccess.ExecuteNonQuery(sqlList);
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("任务锁定失败"));
            }

        }

        /// <summary>
        /// 将当前任务状态更新为-完成  4号车间不需要判断下道工序是否堵塞
        /// </summary>
        /// <param name="produceData">单个任务信息</param>
        /// <param name="hyProduce">当前派工单内的缓存数据</param>
        /// <param name="finishTime">结束时间</param>
        private void UpdateFinish(ProduceBcfData produceData, HYProduceData hyProduce, long finishTime)
        {
            List<string> sqlList = new List<string>();
            foreach (string linkBarcode in produceData.LinkBarcode)
            {
                sqlList.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STATES = 5,FINISHTIME = {3} WHERE WORKORDERNO = {0} AND TASKNO = {1} AND                          LINKBARCODE = {2} AND WORKPROCESSNO = {4}", LibStringBuilder.GetQuotString(produceData.BillNo), LibStringBuilder.GetQuotString(produceData.TaskNo),
                LibStringBuilder.GetQuotString(linkBarcode), finishTime, produceData.WorkProcessNo));
            }
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                int result = this.DataAccess.ExecuteNonQuery(sqlList);

                trans.Commit();
                ProduceData produce = LibProduceCache.Default.GetProduceData(produceData.BillNo);
                if (produceData.WorkProcessNo == produce.LastWorkProcessNo)
                {
                    DataRow[] dataRowArray = null;
                    dataRowArray = hyProduce.TenWorkRecord.Tables[0].Select(string.Format(@"STATES <> 5 AND WORKPROCESSNO = {0}", produceData.WorkProcessNo));
                    if (dataRowArray.Length == 0)
                    {
                        UpdateTenState(produceData.BillNo, LibSysUtils.ToString(hyProduce.TenWorkRecord.Tables[0].Rows[0]["PLSWORKORDERNO"]),2);
                    }
                }
            }
            catch
            {
                trans.Rollback();
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("任务完成失败"));
            }
        }

        /// <summary>
        /// 将当前任务状态更新为-异常
        /// </summary>
        /// <param name="produceData">单个任务信息</param>
        /// <param name="hyProduce">当前派工单内的缓存数据</param>
        private void UpdateDefect(ProduceBcfData produceData, HYProduceData hyProduce)
        {
            List<string> sqlList = new List<string>();
            foreach (string linkBarcode in produceData.LinkBarcode)
            {
                sqlList.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STATES = 6 WHERE WORKORDERNO = {0} AND TASKNO = {1} AND                          LINKBARCODE = {2} AND WORKPROCESSNO = {3}", LibStringBuilder.GetQuotString(produceData.BillNo), LibStringBuilder.GetQuotString(produceData.TaskNo),
                LibStringBuilder.GetQuotString(linkBarcode), produceData.WorkProcessNo));
            }
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                int result = this.DataAccess.ExecuteNonQuery(sqlList);
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("任务完成失败"));
            }
        }

        /// <summary>
        /// 撤销当前任务
        /// </summary>
        /// <param name="produceData">单个任务信息</param>
        /// <param name="hyProduce">当前派工单内的缓存数据</param>
        private void UpdateBack(ProduceBcfData produceData, HYProduceData hyProduce)
        {
            List<string> sqlList = new List<string>();
            foreach (string linkBarcode in produceData.LinkBarcode)
            {
                sqlList.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STATES = 0,STARTTIME = {3},WORKSTATIONID = '{5}',PERSONID = '{6}',PERSONNAME = '{7}' WHERE WORKORDERNO = {0} AND TASKNO = {1} AND  LINKBARCODE = {2} AND WORKPROCESSNO = {4}", LibStringBuilder.GetQuotString(produceData.BillNo), LibStringBuilder.GetQuotString(produceData.TaskNo), LibStringBuilder.GetQuotString(linkBarcode), 0, produceData.WorkProcessNo,
                produceData.Config.IsPoint ? produceData.WorkstationId : "", ""
                , ""));
            }
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                int result = this.DataAccess.ExecuteNonQuery(sqlList);
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("任务撤销失败"));
            }
        }

        /// <summary>
        /// 单个报工
        /// </summary>
        /// <param name="produceData">单个任务信息</param>
        /// <param name="hyProduce">当前派工单内的缓存数据</param>
        private void UpdateSingle(ProduceBcfData produceData, HYProduceData hyProduce)
        {
            List<string> sqlList = new List<string>();
            foreach (string linkBarcode in produceData.LinkBarcode)
            {
                sqlList.Add(string.Format(@"UPDATE PPTENWORKRECORD SET FINISHNUM = FINISHNUM +1 WHERE WORKORDERNO = {0} AND TASKNO = {1} AND  LINKBARCODE = {2} AND WORKPROCESSNO = {4}", LibStringBuilder.GetQuotString(produceData.BillNo), LibStringBuilder.GetQuotString(produceData.TaskNo), LibStringBuilder.GetQuotString(linkBarcode), 0, produceData.WorkProcessNo,
                produceData.Config.IsPoint ? produceData.WorkstationId : "", ""
                , ""));
            }
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                int result = this.DataAccess.ExecuteNonQuery(sqlList);
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("单个报工失败"));
            }
        }

        /// <summary>
        /// 协同发放
        /// </summary>
        /// <param name="produceData">单个任务信息</param>
        public void UpdateCooprateWorkProcess(ProduceBcfData produceData)
        {
            List<string> sqlList = new List<string>();
            HYProduceData hyProduce = LibHYProduceCache.Default.GetProduceData(produceData.BillNo);
            if (hyProduce == null)
                return;
            if (produceData.Config.CooperateWorkProcess.Count > 0)
            {
                foreach (string workProcess in produceData.Config.CooperateWorkProcess)
                {
                    foreach (DataRow row in hyProduce.TenWorkRecord.Tables[0].Rows)
                    {
                        string sourTaskNo = LibSysUtils.ToString(row["TASKNO"]);
                        string sourWorkProcessId = LibSysUtils.ToString(row["WORKPROCESSID"]);
                        if (sourTaskNo == produceData.TaskNo && sourWorkProcessId == workProcess)
                        {
                            row["COOPERATESTATE"] = 1;
                            string sql = string.Format(@"UPDATE PPTENWORKRECORD SET COOPERATESTATE = 1 WHERE WORKORDERNO = {0} AND TASKNO = {1}
                                         AND WORKPROCESSID = {2}", LibStringBuilder.GetQuotString(produceData.BillNo), LibStringBuilder.GetQuotString(produceData.TaskNo), LibStringBuilder.GetQuotString(workProcess));
                            if (!sqlList.Contains(sql))
                            {
                                sqlList.Add(sql);
                            }
                        }
                    }
                }
            }
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                int result = this.DataAccess.ExecuteNonQuery(sqlList);
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("协同失败"));
            }
        }

        /// <summary>
        /// 用DataRow填充TenWorkRecord对象
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <returns></returns>
        private TenWorkRecord GetWorkRecord(DataRow row)
        {
            TenWorkRecord tr = new TenWorkRecord();
            tr.TaskNo = LibSysUtils.ToString(row["TASKNO"]);
            tr.WorkOrderNo = LibSysUtils.ToString(row["WORKORDERNO"]);
            tr.PlsWorkOrderNo = LibSysUtils.ToString(row["PLSWORKORDERNO"]);
            tr.FromBillNo = LibSysUtils.ToString(row["FROMBILLNO"]);
            tr.FromRowId = LibSysUtils.ToInt32(row["FROMROWID"]);
            tr.LinkBarcode = LibSysUtils.ToString(row["LINKBARCODE"]);
            tr.Barcode = LibSysUtils.ToString(row["BARCODE"]);
            tr.MaterialType = LibSysUtils.ToString(row["MATERIALTYPE"]);
            tr.MaterialId = LibSysUtils.ToString(row["MATERIALID"]);
            tr.MaterialName = LibSysUtils.ToString(row["MATERIALNAME"]);
            tr.UnitNum = LibSysUtils.ToInt32(row["UNITNUM"]);
            tr.BarcodeRuleId = LibSysUtils.ToString(row["BARCODERULEID"]);
            tr.MainAttributeDesc = LibSysUtils.ToString(row["MAINATTRIBUTEDESC"]);
            tr.AttributeId = LibSysUtils.ToString(row["ATTRIBUTEID"]);
            tr.AttributeDesc = LibSysUtils.ToString(row["ATTRIBUTEDESC"]);
            tr.WorkProcessId = LibSysUtils.ToString(row["WORKPROCESSID"]);
            tr.WorkProcessNo = LibSysUtils.ToInt32(row["WORKPROCESSNO"]);
            tr.NextWorkProcessNo = LibSysUtils.ToInt32(row["NEXTWORKPROCESSNO"]);
            tr.ProduceLineId = LibSysUtils.ToString(row["PRODUCELINEID"]);
            tr.ProduceLineName = LibSysUtils.ToString(row["PRODUCELINENAME"]);
            tr.States = LibSysUtils.ToInt32(row["STATES"]);
            tr.CooprateState = LibSysUtils.ToInt32(row["COOPERATESTATE"]);
            tr.WorkstationId = LibSysUtils.ToString(row["WORKSTATIONID"]);
            tr.OrderNum = LibSysUtils.ToInt32(row["ORDERNUM"]);
            tr.StartTime = LibSysUtils.ToInt64(row["STARTTIME"]);
            tr.FinishTime = LibSysUtils.ToInt64(row["FINISHTIME"]);
            tr.DefectState = LibSysUtils.ToInt32(row["DEFECTSTATE"]);
            tr.StartState = LibSysUtils.ToInt32(row["STARTSTATE"]);
            tr.ScanNum = LibSysUtils.ToInt32(row["SCANNUM"]);
            tr.StorageId = LibSysUtils.ToString(row["STORAGEID"]);
            tr.OrderDate = LibSysUtils.ToInt32(row["ORDERDATE"]);
            tr.SubMaterialId = LibSysUtils.ToString(row["SUBMATERIALID"]);
            tr.SubMaterialName = LibSysUtils.ToString(row["SUBMATERIALNAME"]);
            tr.PersonId = LibSysUtils.ToString(row["PERSONID"]);
            tr.PersonName = LibSysUtils.ToString(row["PERSONNAME"]);
            tr.ProductType = LibSysUtils.ToString(row["PRODUCTTYPE"]);
            tr.LotNo = LibSysUtils.ToString(row["LOTNO"]);
            tr.GroupNo = LibSysUtils.ToString(row["GROUPNO"]);
            tr.CustomerName = LibSysUtils.ToString(row["CUSTOMERNAME"]);
            tr.ProductSize = LibSysUtils.ToString(row["PRODUCTSIZE"]);
            tr.TreeType = LibSysUtils.ToString(row["TREETYPE"]);
            tr.Color = LibSysUtils.ToString(row["COLOR"]);
            tr.Location = LibSysUtils.ToString(row["LOCATION"]);
            tr.Time = LibSysUtils.ToString(row["TIME"]);
            tr.Total = LibSysUtils.ToString(row["TOTAL"]);
            tr.SaleBillNo = LibSysUtils.ToString(row["SALEBILLNO"]);
            tr.LabelTemplateId = LibSysUtils.ToString(row["LABELTEMPLATEID"]);
            tr.SaleOrderState = LibSysUtils.ToInt32(row["SALEORDERSTATE"]);
            tr.OrderIndex = LibSysUtils.ToInt32(row["ORDERINDEX"]);
            tr.OrderQuantity = LibSysUtils.ToInt32(row["ORDERQUANTITY"]);
            tr.Remark = LibSysUtils.ToString(row["REMARK"]);
            tr.FinishNum = LibSysUtils.ToInt32(row["FINISHNUM"]);
            tr.FromType = LibSysUtils.ToInt32(row["FROMTYPE"]);
            tr.FromSaleBillNo = LibSysUtils.ToString(row["FROMSALEBILLNO"]);
            return tr;
        }

        /// <summary>
        /// 用DataReader填充TenWorkRecord对象
        /// </summary>
        /// <param name="row">IDataReader</param>
        /// <returns></returns>
        private TenWorkRecord GetWorkRecord(IDataReader row)
        {
            TenWorkRecord tr = new TenWorkRecord();
            tr.TaskNo = LibSysUtils.ToString(row["TASKNO"]);
            tr.WorkOrderNo = LibSysUtils.ToString(row["WORKORDERNO"]);
            tr.PlsWorkOrderNo = LibSysUtils.ToString(row["PLSWORKORDERNO"]);
            tr.FromBillNo = LibSysUtils.ToString(row["FROMBILLNO"]);
            tr.FromRowId = LibSysUtils.ToInt32(row["FROMROWID"]);
            tr.LinkBarcode = LibSysUtils.ToString(row["LINKBARCODE"]);
            tr.Barcode = LibSysUtils.ToString(row["BARCODE"]);
            tr.MaterialType = LibSysUtils.ToString(row["MATERIALTYPE"]);
            tr.MaterialId = LibSysUtils.ToString(row["MATERIALID"]);
            tr.MaterialName = LibSysUtils.ToString(row["MATERIALNAME"]);
            tr.UnitNum = LibSysUtils.ToInt32(row["UNITNUM"]);
            tr.BarcodeRuleId = LibSysUtils.ToString(row["BARCODERULEID"]);
            tr.MainAttributeDesc = LibSysUtils.ToString(row["MAINATTRIBUTEDESC"]);
            tr.AttributeId = LibSysUtils.ToString(row["ATTRIBUTEID"]);
            tr.AttributeDesc = LibSysUtils.ToString(row["ATTRIBUTEDESC"]);
            tr.WorkProcessId = LibSysUtils.ToString(row["WORKPROCESSID"]);
            tr.WorkProcessNo = LibSysUtils.ToInt32(row["WORKPROCESSNO"]);
            tr.NextWorkProcessNo = LibSysUtils.ToInt32(row["NEXTWORKPROCESSNO"]);
            tr.ProduceLineId = LibSysUtils.ToString(row["PRODUCELINEID"]);
            tr.ProduceLineName = LibSysUtils.ToString(row["PRODUCELINENAME"]);
            tr.States = LibSysUtils.ToInt32(row["STATES"]);
            tr.CooprateState = LibSysUtils.ToInt32(row["COOPERATESTATE"]);
            tr.WorkstationId = LibSysUtils.ToString(row["WORKSTATIONID"]);
            tr.OrderNum = LibSysUtils.ToInt32(row["ORDERNUM"]);
            tr.StartTime = LibSysUtils.ToInt64(row["STARTTIME"]);
            tr.FinishTime = LibSysUtils.ToInt64(row["FINISHTIME"]);
            tr.DefectState = LibSysUtils.ToInt32(row["DEFECTSTATE"]);
            tr.StartState = LibSysUtils.ToInt32(row["STARTSTATE"]);
            tr.ScanNum = LibSysUtils.ToInt32(row["SCANNUM"]);
            tr.StorageId = LibSysUtils.ToString(row["STORAGEID"]);
            tr.OrderDate = LibSysUtils.ToInt32(row["ORDERDATE"]);
            tr.SubMaterialId = LibSysUtils.ToString(row["SUBMATERIALID"]);
            tr.SubMaterialName = LibSysUtils.ToString(row["SUBMATERIALNAME"]);
            tr.PersonId = LibSysUtils.ToString(row["PERSONID"]);
            tr.PersonName = LibSysUtils.ToString(row["PERSONNAME"]);
            tr.ProductType = LibSysUtils.ToString(row["PRODUCTTYPE"]);
            tr.LotNo = LibSysUtils.ToString(row["LOTNO"]);
            tr.GroupNo = LibSysUtils.ToString(row["GROUPNO"]);
            tr.CustomerName = LibSysUtils.ToString(row["CUSTOMERNAME"]);
            tr.ProductSize = LibSysUtils.ToString(row["PRODUCTSIZE"]);
            tr.TreeType = LibSysUtils.ToString(row["TREETYPE"]);
            tr.Color = LibSysUtils.ToString(row["COLOR"]);
            tr.Location = LibSysUtils.ToString(row["LOCATION"]);
            tr.Time = LibSysUtils.ToString(row["TIME"]);
            tr.Total = LibSysUtils.ToString(row["TOTAL"]);
            tr.SaleBillNo = LibSysUtils.ToString(row["SALEBILLNO"]);
            tr.LabelTemplateId = LibSysUtils.ToString(row["LABELTEMPLATEID"]);
            tr.SaleOrderState = LibSysUtils.ToInt32(row["SALEORDERSTATE"]);
            tr.OrderIndex = LibSysUtils.ToInt32(row["ORDERINDEX"]);
            tr.OrderQuantity = LibSysUtils.ToInt32(row["ORDERQUANTITY"]);
            tr.Remark = LibSysUtils.ToString(row["REMARK"]);
            tr.FinishNum = LibSysUtils.ToInt32(row["FINISHNUM"]);
            tr.FromType = LibSysUtils.ToInt32(row["FROMTYPE"]);
            tr.FromSaleBillNo = LibSysUtils.ToString(row["FROMSALEBILLNO"]);
            return tr;
        }

        /// <summary>
        /// 异常保存
        /// </summary>
        /// <param name="proAbnormalReport">异常数据</param>
        /// <returns>是否保存成功</returns>
        public string SaveAbnormalByType(AbnormalReport proAbnormalReport)
        {
            string returnstring;
            string sql =
                string.Format(
                    @"SELECT BILLNO FROM COMABNORMALREPORT WHERE FROMMARK = '{0}' AND (DEALWITHSTATE=0 OR DEALWITHSTATE = 1)  AND ABNORMALTYPEID <> 'PP003'", proAbnormalReport.FromMark);
            string billNo = LibSysUtils.ToString(this.DataAccess.ExecuteScalar(sql));
            if (!string.IsNullOrEmpty(billNo))
            {
                returnstring = "False";
                return returnstring;
            }
            LibEntryParam entryParam = new LibEntryParam();
            entryParam.ParamStore.Add("TYPEID", proAbnormalReport.TypeId);
            //entryParam.ParamStore.Add("ABNORMALID", proAbnormalReport.AbnormalId);
            DataSet dataset = null;
            LibBcfData bcfData = (LibBcfData)LibBcfSystem.Default.GetBcfInstance("com.AbnormalReport");
            if (proAbnormalReport.BillNo.Length == 0)
            {
                proAbnormalReport.StartTime = LibDateUtils.DateTimeToLibDateTime(DateTime.Now);
                dataset = bcfData.AddNew(entryParam);
                //填充数据
                FillData(dataset, proAbnormalReport);
                dataset = bcfData.InnerSave(BillAction.AddNew, null, dataset);
                proAbnormalReport.BillNo = LibSysUtils.ToString(dataset.Tables[0].Rows[0]["BILLNO"]);
                BuildAbnormalTrace(proAbnormalReport);
            }
            else
            {
                dataset = bcfData.Edit(new object[] { proAbnormalReport.BillNo });
                //填充数据
                FillData(dataset, proAbnormalReport);
                dataset = bcfData.InnerSave(BillAction.Modif, new object[] { dataset.Tables[0].Rows[0]["BILLNO"] }, dataset);
            }

            if (bcfData.ManagerMessage.IsThrow)
            {
                returnstring = "False";
            }
            else
            {
                returnstring = "True";
            }
            return returnstring;
        }

        /// <summary>
        /// 构建异常追踪单
        /// </summary>
        /// <param name="abnormalReport">异常数据</param>
        /// <returns>异常追踪单数据集合</returns>
        public DataTable BuildAbnormalTrace(AbnormalReport abnormalReport)
        {
            string sql = string.Format("select BILLNO  from COMABNORMALTRACE where FROMBILLNO={0}",
                  LibSysUtils.ToString(abnormalReport.BillNo));
            string billNo = string.Empty;
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                if (reader.Read())
                {
                    billNo = LibSysUtils.ToString(reader["BILLNO"]);
                }
            }

            string abnormalType = LibSysUtils.ToString(abnormalReport.TypeId);
            LibEntryParam entryParam = new LibEntryParam();
            string typeid = abnormalReport.TypeId;
            typeid = typeid.Insert(typeid.Length - 1, "T");
            entryParam.ParamStore.Add("TYPEID", typeid);
            DataSet dataSet = null;
            LibBcfData bcfData = (LibBcfData)LibBcfSystem.Default.GetBcfInstance("com.AbnormalTrace");
            if (string.IsNullOrEmpty(billNo))
            {
                dataSet = bcfData.AddNew(entryParam);
                FillTraceData(dataSet, abnormalReport);
                dataSet = bcfData.InnerSave(BillAction.AddNew, null, dataSet);
            }
            else
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("已存在相应的异常追踪单{0},不能重复生成。", billNo));
                return null;
            }
            if (bcfData.ManagerMessage.IsThrow)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("异常追踪单{0}生成出错，错误信息为:", billNo));
                foreach (LibMessage msg in bcfData.ManagerMessage.MessageList)
                {
                    this.ManagerMessage.AddMessage(msg);
                }

                return null;
            }
            else
            {
                string newBillNo = LibSysUtils.ToString(dataSet.Tables[0].Rows[0]["BILLNO"]);
                this.ManagerMessage.AddMessage(LibMessageKind.Info, string.Format("异常报告单{0}生成成功，异常追踪单号为:{1}", billNo, newBillNo));

                return dataSet.Tables[0];
            }
        }

        /// <summary>
        /// 填充异常追踪单数据
        /// </summary>
        /// <param name="ds">异常追踪单DataSet结构</param>
        /// <param name="abnormalReport">异常数据</param>
        private void FillTraceData(DataSet ds, AbnormalReport abnormalReport)
        {
            DataRow masterRow = ds.Tables[0].Rows[0];
            masterRow.BeginEdit();
            try
            {
                masterRow["FROMBILLNO"] = LibSysUtils.ToString(abnormalReport.BillNo);
                masterRow["PLANENDTIME"] = LibDateUtils.GetCurrentDateTime();
                masterRow["ABNORMALREASONID"] = "";
                masterRow["PERSONID"] = LibSysUtils.ToString(abnormalReport.PersonId);
                masterRow["DEALWITHPERSONID"] = LibSysUtils.ToString(abnormalReport.PersonId); ;
                masterRow["SOLUTION"] = "无";
                masterRow["DEALWITHSTATE"] = 0;
            }
            finally
            {
                masterRow.EndEdit();
            }
        }

        /// <summary>
        /// 填充数据
        /// </summary>
        /// <param name="dataset">异常报告单DataSet结构</param>
        /// <param name="proAbnormalReport">异常数据</param>
        private void FillData(DataSet dataset, AbnormalReport proAbnormalReport)
        {
            DataRow masterRow = dataset.Tables[0].Rows[0];
            masterRow.BeginEdit();
            try
            {
                masterRow["TYPEID"] = LibSysUtils.ToString(proAbnormalReport.TypeId);
                masterRow["ABNORMALTYPEID"] = LibSysUtils.ToString(proAbnormalReport.AbnormalTypeId);
                masterRow["ABNORMALID"] = LibSysUtils.ToString(proAbnormalReport.AbnormalId);
                masterRow["ABNORMALDESC"] = LibSysUtils.ToString(proAbnormalReport.AbnormalDesc);
                masterRow["PERSONID"] = LibSysUtils.ToString(proAbnormalReport.PersonId);
                masterRow["DEPTID"] = LibSysUtils.ToString(proAbnormalReport.DeptId);
                masterRow["FROMPERSONID"] = LibSysUtils.ToString(proAbnormalReport.FromPersonId);
                masterRow["FROMDEPTID"] = LibSysUtils.ToString(proAbnormalReport.FromDeptId);
                masterRow["AFFECTPERSONNUM"] = LibSysUtils.ToDecimal(proAbnormalReport.AffectPersonNum);
                masterRow["AFFECTPRODUCESTATE"] = LibSysUtils.ToInt32(proAbnormalReport.AffectProduceState);
                masterRow["AFFECTTIME"] = LibSysUtils.ToDouble(proAbnormalReport.AffectTime);
                masterRow["STARTTIME"] = LibSysUtils.ToInt64(proAbnormalReport.StartTime);
                masterRow["ENDTIME"] = LibSysUtils.ToInt64(proAbnormalReport.EndTime);
                masterRow["DEALWITHSTATE"] = LibSysUtils.ToInt32(proAbnormalReport.DealWithState);
                masterRow["FROMMARK"] = LibSysUtils.ToString(proAbnormalReport.FromMark);
                masterRow["ABNORMALPROTOTYPE"] = LibSysUtils.ToInt32(proAbnormalReport.abnormalPrptoType);
                masterRow["ISSYSTEMBUILD"] = 1;
            }
            finally
            {
                masterRow.EndEdit();
            }
        }

        /// <summary>
        /// 获取可发放的明细信息
        /// </summary>
        /// <param name="sqlWhere">SQL条件</param>
        /// <returns>可发放任务的明细集合</returns>
        public List<TenWorkRecord> GetMessage(string sqlWhere)
        {
            List<TenWorkRecord> sendMessageList = new List<TenWorkRecord>();
            string sql = string.Format(@"SELECT DISTINCT *
                                        FROM PPTENWORKRECORD
                                        WHERE  {0}
                                        ORDER BY ORDERDATE,ORDERNUM,WORKORDERNO,ORDERINDEX,SCANNUM", string.IsNullOrEmpty(sqlWhere) ? " 1=1                                           " : sqlWhere);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    sendMessageList.Add(GetWorkRecord(reader));
                }
            }
            return sendMessageList;
        }

        /// <summary>
        /// 获取发放信息的主清单列表
        /// </summary>
        /// <param name="sqlWhere">SQL条件</param>
        /// <returns>发放清单</returns>
        public List<TenWorkRecord> GetMainMessage(string sqlWhere)
        {
            List<TenWorkRecord> sendMessageList = new List<TenWorkRecord>();
            string sql = string.Format(@"SELECT ORDERDATE,
                                                        ORDERNUM,
                                                        PLSWORKORDERNO,
                                                        WORKORDERNO,
                                                        PRODUCELINEID,
                                                        PRODUCELINENAME,
                                                        FROMTYPE
                                                        FROM PPMAINTENWORKRECORD
                                                        WHERE STARTSTATE = 0
                                                        AND {0}   ORDER BY PRODUCELINEID, ORDERDATE, ORDERNUM", string.IsNullOrEmpty(sqlWhere) ? " 1=1                                           " : sqlWhere);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    sendMessageList.Add(new TenWorkRecord() { 
                        OrderDate = LibSysUtils.ToInt32(reader["ORDERDATE"]),
                        OrderNum = LibSysUtils.ToInt32(reader["ORDERNUM"]),
                        PlsWorkOrderNo = LibSysUtils.ToString(reader["PLSWORKORDERNO"]),
                        WorkOrderNo = LibSysUtils.ToString(reader["WORKORDERNO"]),
                        ProduceLineId = LibSysUtils.ToString(reader["PRODUCELINEID"]),
                        ProduceLineName = LibSysUtils.ToString(reader["PRODUCELINENAME"]),
                        FromType = LibSysUtils.ToInt32(reader["FROMTYPE"])
                        //ScanNum = LibSysUtils.ToInt32(reader["TASKNUM"])
                    });
                }
            }
            return sendMessageList;
        }

        /// <summary>
        /// 获取门套提前打的数据
        /// </summary>
        /// <param name="sqlWhere">SQL条件</param>
        /// <returns>门套提前打码的数据集合</returns>
        public List<TenWorkRecord> GetSpecialMessage(string sqlWhere)
        {
            //AND MATERIALTYPE <> '{0}'
            //AND PRODUCELINEID = '{1}'
            List<TenWorkRecord> tenList = new List<TenWorkRecord>();
            string sql = string.Format(@"SELECT DISTINCT C.ORDERDATE,
                                                    C.ORDERNUM,
                                                    C.PLSWORKORDERNO,
                                                    C.WORKORDERNO,
                                                    C.PRODUCELINEID,
                                                    C.PRODUCELINENAME,
                                                    C.FROMTYPE
                                                    FROM (SELECT DISTINCT A.ORDERDATE,
                                                    A.ORDERNUM,
                                                    A.PLSWORKORDERNO,
                                                    A.WORKORDERNO,
                                                    A.PRODUCELINEID,
                                                    A.PRODUCELINENAME,
                                                    A.FROMTYPE,
                                                    B.STATES
                                                    FROM PPMAINTENWORKRECORD A
                                                    LEFT JOIN PPTENWORKRECORD B
                                                    ON A.WORKORDERNO = B.WORKORDERNO
                                                    WHERE {0}
                                                    AND A.STARTSTATE <> 3
                                                    AND B.WORKPROCESSNO = A.FIRSTWORKPROCESSNO
                                                    AND B.NONSTANDARD = 1) C
                                                    WHERE C.STATES <> 5
                                                    ORDER BY C.PRODUCELINEID, C.ORDERDATE, C.ORDERNUM", sqlWhere);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    tenList.Add(new TenWorkRecord()
                    {
                        OrderDate = LibSysUtils.ToInt32(reader["ORDERDATE"]),
                        OrderNum = LibSysUtils.ToInt32(reader["ORDERNUM"]),
                        PlsWorkOrderNo = LibSysUtils.ToString(reader["PLSWORKORDERNO"]),
                        WorkOrderNo = LibSysUtils.ToString(reader["WORKORDERNO"]),
                        ProduceLineId = LibSysUtils.ToString(reader["PRODUCELINEID"]),
                        ProduceLineName = LibSysUtils.ToString(reader["PRODUCELINENAME"]),
                        FromType = LibSysUtils.ToInt32(reader["FROMTYPE"])
                        //ScanNum = LibSysUtils.ToInt32(reader["TASKNUM"])
                    });
                }
            }
            return tenList;
        }

        /// <summary>
        /// 获取主数据信息
        /// </summary>
        /// <param name="sqlWhere">SQL条件</param>
        /// <param name="ProgId">ProgId</param>
        /// <returns>数据集合【KEY,VALUE】</returns>
        public List<string> comList(string sqlWhere,string ProgId)
        {
            List<string> comList = new List<string>();
            string sql = string.Empty;
            string comId = string.Empty;
            string comName = string.Empty;
            switch (ProgId)
            { 
                case "com.Workstation":
                    sql = string.Format("SELECT WORKSTATIONID,WORKSTATIONNAME FROM COMWORKSTATION WHERE {0} ORDER BY WORKSTATIONID",
                string.IsNullOrEmpty(sqlWhere) ? " 1 = 1" : sqlWhere);
                    comId = "WORKSTATIONID";comName = "WORKSTATIONNAME";
                    break;
                case "com.ProduceLine":
                    sql = string.Format("SELECT PRODUCELINEID,PRODUCELINENAME FROM COMPRODUCELINE WHERE {0} ORDER BY PRODUCELINEID",
                string.IsNullOrEmpty(sqlWhere) ? " 1 = 1" : sqlWhere);
                    comId = "PRODUCELINEID";comName = "PRODUCELINENAME";
                    break;
            }
            if (!string.IsNullOrEmpty(sql))
            {
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        comList.Add(LibSysUtils.ToString(reader[comId]) + "," + LibSysUtils.ToString(reader[comName]));
                    }
                }
            }

            return comList;
        }

        /// <summary>
        /// 发放明细数据
        /// </summary>
        /// <param name="taskNoList">任务号集合</param>
        /// <param name="plsWorkOrderNo">作业单号</param>
        /// <param name="workOrderNo">派工单号</param>
        /// <param name="workstationId">站点编号</param>
        /// <returns>是否发放成功</returns>
        public bool SendDataDetail(ProduceBcfData taskNoList, string plsWorkOrderNo, string workOrderNo, string workstationId)
        {
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                int workprocessNo = 0;
                List<string> sqlList = new List<string>();
                if (taskNoList.LinkBarcode.Count > 0)
                {
                    string sql = string.Format(@"SELECT MIN(WORKPROCESSNO) FROM PPTENWORKRECORD WHERE PLSWORKORDERNO = '{0}' AND 
                                         WORKORDERNO = '{1}'", plsWorkOrderNo, workOrderNo);
                    workprocessNo = LibSysUtils.ToInt32(this.DataAccess.ExecuteScalar(sql));
                    foreach (string taskNo in taskNoList.LinkBarcode)
                    {
                        sqlList.Add(string.Format(@"UPDATE PPTENWORKRECORD SET WORKSTATIONID = '{4}' WHERE PLSWORKORDERNO = '{0}' AND WORKORDERNO = '{1}' AND TASKNO = '{2}' AND WORKPROCESSNO = {3}", plsWorkOrderNo, workOrderNo, taskNo, workprocessNo, workstationId));
                    }
                    int result = this.DataAccess.ExecuteNonQuery(sqlList);
                    if (result > 0)
                    {
                        trans.Commit();
                        return true;
                    }

                }
                trans.Commit();
                return false;
            }
            catch (Exception )
            {
                trans.Rollback();
                return false;
            }

        }

        /// <summary>
        /// 发放十单数据
        /// </summary>
        /// <param name="plsWorkOrderNo">作业单号</param>
        /// <param name="workOrderNo">派工单号</param>
        /// <returns>是否发放成功</returns>
        public bool SendTenData(string plsWorkOrderNo, string workOrderNo)
        {
            List<string> sqlList = new List<string>();
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                int wCount = 0;
                string sql = string.Format(@" SELECT COUNT(B.TASKNO)
                                                        FROM PPMAINTENWORKRECORD A
                                                        LEFT JOIN PPTENWORKRECORD B
                                                        ON A.WORKORDERNO = B.WORKORDERNO
                                                        AND A.FIRSTWORKPROCESSNO = B.WORKPROCESSNO
                                                        WHERE A.PLSWORKORDERNO = '{0}'
                                                        AND A.WORKORDERNO = '{1}'
                                                        AND B.WORKSTATIONID IS NULL
                                                        ", plsWorkOrderNo, workOrderNo);
                wCount = LibSysUtils.ToInt32(this.DataAccess.ExecuteScalar(sql));
                if (wCount == 0)
                {
                    sqlList.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STARTSTATE =1,SENDSTARTTIME = {2} WHERE PLSWORKORDERNO = '{0}' AND 
                                         WORKORDERNO = '{1}'", plsWorkOrderNo, workOrderNo,LibDateUtils.GetCurrentDateTime()));
                    sqlList.Add(string.Format("UPDATE PPWORKORDER SET STARTSTATE = 1 WHERE BILLNO = '{0}'",workOrderNo));
                    sqlList.Add(string.Format("UPDATE PPMAINTENWORKRECORD SET STARTSTATE = 1 WHERE WORKORDERNO = '{0}'", workOrderNo));
                    int result = this.DataAccess.ExecuteNonQuery(sqlList);
                    if (result > 0)
                    {
                        trans.Commit();
                        return true;
                    }
                }
                trans.Commit();
                return false;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
        }

        /// <summary>
        /// 添加赋码数据缓存
        /// </summary>
        /// <param name="billNo">派工单号</param>
        /// <returns></returns>
        public bool CommitMemaryData(string billNo)
        {
            if (!string.IsNullOrEmpty(billNo))
            {
                LibHYControlServer.Default.AddWorkOrder(billNo);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 根据扫入的条码获取派工单号
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <returns>单个产成品的任务集合</returns>
        public List<TenWorkRecord> GetScanBillNo(string barcode)
        {
            List<TenWorkRecord> tenWorkRecordList = new List<TenWorkRecord>();
            string sql = string.Format(@"SELECT *
                                        FROM PPTENWORKRECORD
                                        WHERE TASKNO =
                                        (SELECT DISTINCT TASKNO FROM PPTENWORKRECORD WHERE BARCODE = '{0}')
                                        AND WORKPROCESSNO =
                                        (SELECT MAX(WORKPROCESSNO)
                                        FROM PPTENWORKRECORD
                                        WHERE TASKNO = (SELECT DISTINCT TASKNO
                                        FROM PPTENWORKRECORD
                                        WHERE BARCODE = '{0}'))
                                        UNION ALL
                                        SELECT *
                                        FROM PPTENWORKRECORD
                                        WHERE TASKNO = (SELECT DISTINCT TASKNO
                                        FROM PPTENWORKRECORD
                                        WHERE LINKBARCODE = '{0}')
                                        AND WORKPROCESSNO =
                                        (SELECT MAX(WORKPROCESSNO)
                                        FROM PPTENWORKRECORD
                                        WHERE TASKNO = (SELECT DISTINCT TASKNO
                                        FROM PPTENWORKRECORD
                                        WHERE LINKBARCODE = '{0}'))
", barcode);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    tenWorkRecordList.Add(GetWorkRecord(reader));
                }
            }

            return tenWorkRecordList;
        }

        /// <summary>
        /// 获取关联码
        /// </summary>
        /// <param name="billNo">派工单号</param>
        /// <param name="taskNo">任务号</param>
        /// <param name="workProcessNo">工序号</param>
        /// <returns>关联条码集合【KEY,KEY,KEY】</returns>
        public string UpdateWsRecord(string billNo, string taskNo, int workProcessNo)
        {
            List<string> linkBarcode = new List<string>();
            HYProduceData hyProduce = LibHYProduceCache.Default.GetProduceData(billNo);
            ProduceData produce = LibProduceCache.Default.GetProduceData(billNo);
            if (hyProduce == null || produce == null)
                return string.Empty;
            if (!produce.FirstWorkProcessNo.Contains(workProcessNo))
            {
                StringBuilder stringBuilder = new StringBuilder();
                WorkProcessInfo wp = produce.WorkProcessNo[workProcessNo];
                foreach (int workNo in wp.PreWorkProcessNo)
                {
                    stringBuilder.Append(workNo);
                    stringBuilder.Append(",");
                }
                string sqlWhere = stringBuilder.ToString().Substring(0, stringBuilder.ToString().LastIndexOf(','));


                DataRow[] dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSNO in ({0}) AND TASKNO = {1}  AND STATES = 5 ", sqlWhere, LibStringBuilder.GetQuotString(taskNo)), "WORKPROCESSNO ");
                for (int i = 0; i < dataRows.Length; i++)
                {
                    if (!linkBarcode.Contains(LibSysUtils.ToString(dataRows[i]["BARCODE"])))
                    {
                        linkBarcode.Add(LibSysUtils.ToString(dataRows[i]["BARCODE"]));
                    }
                }
                stringBuilder.Clear();
                foreach (string link in linkBarcode)
                {
                    stringBuilder.Append(link);
                    stringBuilder.Append(",");
                }
                return stringBuilder.ToString().Substring(0, stringBuilder.ToString().LastIndexOf(','));
            }
            return string.Empty;
        }

        /// <summary>
        /// 是否检验完毕
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <returns></returns>
        public bool IsHasFinished(string barcode)
        {
            string sql = string.Format(@"SELECT COUNT(BARCODE) FROM PPTENWORKRECORD WHERE BARCODE = '{0}' AND STATES <> 0", barcode);
            decimal result = (decimal)this.DataAccess.ExecuteScalar(sql);
            if (result > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 更新十单过账表的缺陷数据
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <returns></returns>
        public bool UpdateDefectData(string barcode)
        {
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                string sql = string.Format(@"UPDATE PPTENWORKRECORD SET STATES = 5 ,FINISHTIME = {1} WHERE BARCODE = '{0}'", barcode, LibDateUtils.GetCurrentDateTime());
                int result = (int)this.DataAccess.ExecuteNonQuery(sql);
                if (result > 0)
                {
                    trans.Commit();
                    return true;
                }
                return false;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
        }

        /// <summary>
        /// 获取模板信息
        /// </summary>
        /// <param name="ten">单个部件信息</param>
        /// <param name="config">站点配置</param>
        /// <returns>打印模板明细（0:模板信息 1：参数信息【PARAMNAME:PARAMVALUE】）</returns>
        public List<string> GetLabTemplateInfo(TenWorkRecord ten, HYWorkStationConfig config)
        {
            List<string> printInfo = new List<string>();
            string labelTemplateJs = ReadPrintTemplateTxt(ten.LabelTemplateId);
            List<LabelTemplateRule> list = new List<LabelTemplateRule>();
            SqlBuilder sqlBuilder = new SqlBuilder("com.LabelTemplate");
            StringBuilder builder = new StringBuilder();
            StringBuilder selectBuilder = new StringBuilder();
            builder.Append(sqlBuilder.GetQuerySql(1, "B.LTPARAMTYPE,B.LTPARAMNAME,B.LTPARAMVALUE,B.FIELDNAME", string.Format("B.LABELTEMPLATEID = {0}", LibStringBuilder.GetQuotString(ten.LabelTemplateId))));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(builder.ToString()))
            {
                while (reader.Read())
                {
                    LabelTemplateRule rule = new LabelTemplateRule()
                    {
                        LtParamType = (LtParamType)LibSysUtils.ToInt32(reader["LTPARAMTYPE"]),
                        LtParamName = LibSysUtils.ToString(reader["LTPARAMNAME"]),
                        LtParamValue = LibSysUtils.ToString(reader["LTPARAMVALUE"]),
                        FieldName = LibSysUtils.ToString(reader["FIELDNAME"])
                    };
                    list.Add(rule);
                    if (rule.LtParamType == LtParamType.Field)
                    {
                        selectBuilder.AppendFormat("A.{0},", rule.FieldName);
                    }
                }
            }
            Dictionary<string, object> workOrderField = new Dictionary<string, object>();
            if (selectBuilder.Length > 0)
            {
                selectBuilder.Remove(selectBuilder.Length - 1, 1);
                sqlBuilder = new SqlBuilder("pp.TenWorkRecord");
                string sql = string.Empty;
                if (!config.IsMainPrint)
                {
                    sql = sqlBuilder.GetQuerySql(0, selectBuilder.ToString(), string.Format("A.WORKORDERNO={0} AND WORKPROCESSID = {1} AND LINKBARCODE = {2}", LibStringBuilder.GetQuotString(ten.WorkOrderNo), LibStringBuilder.GetQuotString(ten.WorkProcessId), LibStringBuilder.GetQuotString(ten.LinkBarcode)));
                }
                else
                {
                    sql = sqlBuilder.GetQuerySql(0, selectBuilder.ToString(), string.Format("A.WORKORDERNO={0} AND WORKPROCESSID = {1} AND BARCODE = {2}", LibStringBuilder.GetQuotString(ten.WorkOrderNo), LibStringBuilder.GetQuotString(ten.WorkProcessId), LibStringBuilder.GetQuotString(ten.Barcode)));
                }
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (!workOrderField.ContainsKey(reader.GetName(i)))
                            {
                                workOrderField.Add(reader.GetName(i), reader[i]);
                            }
                        }
                    }
                }
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (LabelTemplateRule item in list)
            {
                switch (item.LtParamType)
                {
                    case LtParamType.Image:
                        stringBuilder.Append("【"+item.LtParamName+":"+ string.Format("<img src='../Content/images/{0}'/>", item.LtParamValue)+"】");
                        break;
                    case LtParamType.Field:
                        stringBuilder.Append("【" + item.LtParamName + ":" + string.Format("{0}", workOrderField[item.FieldName]) + "】");
                        break;
                    case LtParamType.Date:
                        DateTime dateTime = DateTime.Now;
                        string dateStr = (dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day).ToString();
                        stringBuilder.Append("【" + item.LtParamName + ":" + dateStr + "】");
                        break;
                }
            }
            printInfo.Add(labelTemplateJs);
            printInfo.Add(LibSysUtils.ToString(stringBuilder));
            return printInfo;
        }

        /// <summary>
        /// 判断当前工单是否完成
        /// </summary>
        /// <param name="billNo">派工单号</param>
        /// <param name="workstationId">站点编号</param>
        /// <param name="workProcessNo">工序号</param>
        /// <param name="config">站点配置</param>
        /// <returns></returns>
        public string IsWorkOrderFinish(string billNo, string workstationId, int workProcessNo, HYWorkStationConfig config)
        {
            string actualBillNo = string.Empty;
            List<string> billNoList = new List<string>();
            HYProduceData hyProduce = LibHYProduceCache.Default.GetProduceData(billNo);
            ProduceData produce = LibProduceCache.Default.GetProduceData(billNo);
            ProductScheduling wsPS = LibWsControlServer.Default.GetProductScheduling();
            DataRow[] dataRows = null;
            if (hyProduce == null || produce == null || wsPS == null)
                return actualBillNo;
            if (produce.FirstWorkProcessNo.Contains(workProcessNo))
            {
                if (config.IsPoint)
                {
                    dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKORDERNO = {0}  AND WORKPROCESSNO = {1} AND WORKSTATIONID = {2}  AND STATES = 0 ", LibStringBuilder.GetQuotString(billNo), workProcessNo, LibStringBuilder.GetQuotString(workstationId)), "TASKNO ");
                }
                else
                {
                    dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKORDERNO = {0} AND WORKPROCESSNO = {1} AND STATES = 0 ", LibStringBuilder.GetQuotString(billNo), workProcessNo), "TASKNO ");
                }
            }
            else
            {
                WorkProcessInfo workProcess = produce.WorkProcessNo[workProcessNo];
                foreach (int preWorkProcessNo in workProcess.PreWorkProcessNo)
                {
                    if (config.IsPoint)
                    {
                        dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKORDERNO = {0} AND  WORKPROCESSNO = {1} AND WORKSTATIONID = {2}  AND STATES = 0 ", LibStringBuilder.GetQuotString(billNo), preWorkProcessNo, LibStringBuilder.GetQuotString(workstationId)), "TASKNO ");
                    }
                    else
                    {
                        dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKORDERNO = {0} AND WORKPROCESSNO = {1} AND STATES = 0 ", LibStringBuilder.GetQuotString(billNo), preWorkProcessNo), "TASKNO ");
                    }
                    if (dataRows.Count() > 0)
                    {
                        break;
                    }
                }
            }
            if (dataRows.Count() == 0)
            {
                string sql = string.Format(@" SELECT DISTINCT WORKORDERNO FROM  (SELECT WORKORDERNO FROM PPMAINTENWORKRECORD WHERE STARTSTATE = 1 ORDER BY ORDERDATE,ORDERNUM)A");
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        billNoList.Add(LibSysUtils.ToString(reader["WORKORDERNO"]));
                    }
                }
                if (wsPS.WsRelWorkOrder.ContainsKey(workstationId))
                {
                    for (int i = 0; i < billNoList.Count; i++)
                    {
                        foreach (string workOrderNo in wsPS.WsRelWorkOrder[workstationId])
                        {
                            if (billNoList[i] == workOrderNo)
                            {
                                if (billNo != workOrderNo)
                                {
                                    actualBillNo = workOrderNo;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return billNo;
            }
            return actualBillNo;
        }

        /// <summary>
        /// 处理异常单
        /// </summary>
        /// <param name="taskNo">任务号</param>
        /// <returns></returns>
        public bool DealWithAbnormal(string taskNo)
        {
            string sql = string.Format(@"SELECT BILLNO FROM COMABNORMALREPORT WHERE FROMMARK = '{0}' AND ENDTIME = 0", taskNo + "-R");
            string billNo = LibSysUtils.ToString(this.DataAccess.ExecuteScalar(sql));
            List<string> sqlList = new List<string>();
            long finishTime = LibDateUtils.GetCurrentDateTime();
            if (!string.IsNullOrEmpty(billNo))
            {
                sqlList.Add(string.Format(@"UPDATE COMABNORMALREPORT SET DEALWITHSTATE = 2,ENDTIME = {0} WHERE BILLNO = '{1}'", finishTime, billNo));
                sqlList.Add(string.Format(@"UPDATE COMABNORMALTRACE SET DEALWITHSTATE = 2,SOLUTION = '已处理' WHERE FROMBILLNO = '{0}'", billNo));
                LibDBTransaction trans = this.DataAccess.BeginTransaction();
                try
                {
                    int result = this.DataAccess.ExecuteNonQuery(sqlList);
                    if (result > 0)
                    {
                        trans.Commit();
                        return true;
                    }
                }
                catch
                {
                    trans.Rollback();
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 当前派工单在当前工序、站点是否完成
        /// </summary>
        /// <param name="config">站点配置</param>
        /// <param name="billNo">派工单号</param>
        /// <param name="workProcessId">工序编号</param>
        /// <param name="workstationId">站点编号</param>
        /// <returns>是否完成</returns>
        public bool IsWorkPrcessFinish(HYWorkStationConfig config, string billNo, string workProcessId, string workstationId)
        {
            HYProduceData hyProduce = LibHYProduceCache.Default.GetProduceData(billNo);
            if (hyProduce == null)
                return true;
            List<string> taskNoList = new List<string>();
            if (hyProduce != null)
            {
                DataRow[] dataRows = null;
                if (!config.IsTransferWorkStationId)
                {
                    if (config.IsPoint)
                    {
                        dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSID = {0} AND WORKSTATIONID = {1}  AND STATES <>5  AND (SALEORDERSTATE = 0 OR SALEORDERSTATE = 1)", LibStringBuilder.GetQuotString(workProcessId), LibStringBuilder.GetQuotString(workstationId)));
                    }
                    else
                    {
                        dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSID = {0}  AND STATES <>5 AND (SALEORDERSTATE = 0 OR SALEORDERSTATE = 1)", LibStringBuilder.GetQuotString(workProcessId)));
                    }
                    if (dataRows.Count() > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKSTATIONID = {0} AND (SALEORDERSTATE = 0 OR SALEORDERSTATE = 1)", LibStringBuilder.GetQuotString(config.TransferWorkStationId)), "STARTTIME ");
                    for (int i = 0; i < dataRows.Length; i++)
                    {
                        if (!taskNoList.Contains(LibSysUtils.ToString(dataRows[i]["TASKNO"])))
                        {
                            taskNoList.Add(LibSysUtils.ToString(dataRows[i]["TASKNO"]));
                        }
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    if (taskNoList.Count > 0)
                    {
                        stringBuilder.Append(" TASKNO IN ( ");
                        foreach (string taskNo in taskNoList)
                        {
                            stringBuilder.Append("'");
                            stringBuilder.Append(taskNo);
                            stringBuilder.Append("',");
                        }
                        string sqlWhere = stringBuilder.ToString().Substring(0, stringBuilder.ToString().LastIndexOf(','));
                        sqlWhere += " )";
                        if (config.IsPoint)
                        {
                            dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSID = {0} AND STATES <>5 AND {1} AND WORKSTATIONID = {2}", LibStringBuilder.GetQuotString(workProcessId), sqlWhere, LibStringBuilder.GetQuotString(workstationId)));
                        }
                        else
                        {
                            dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("WORKPROCESSID = {0} AND STATES <>5 AND {1}", LibStringBuilder.GetQuotString(workProcessId), sqlWhere));
                        }
                        if (dataRows.Count() > 0)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 获取异常
        /// </summary>
        /// <param name="abnormalTypeId">异常类型【KEY，KEY】</param>
        /// <returns>具体的异常类型信息</returns>
        public List<Abnormal> GetAbnormal(string abnormalTypeId)
        {
            List<Abnormal> abnormalList = new List<Abnormal>();
            if (string.IsNullOrEmpty(abnormalTypeId)) return abnormalList;
            string sqlWhere = " A.ABNORMALTYPEID IN (";
            foreach (var typeId in abnormalTypeId.Split(','))
            {
                sqlWhere += "'" + typeId + "'";
                sqlWhere += ",";
            }
            sqlWhere = sqlWhere.Substring(0, sqlWhere.LastIndexOf(",", StringComparison.Ordinal));
            sqlWhere += ")";
            string sql = string.Format(@"SELECT A.*,B.DEPTNAME,C.ABNORMALTYPENAME
                                        FROM COMABNORMAL A 
                                        LEFT JOIN COMDEPT B ON A.DEPTID = B.DEPTID  
                                        LEFT JOIN COMABNORMALTYPE C ON A.ABNORMALTYPEID = C.ABNORMALTYPEID
                                        WHERE {0} ORDER BY A.ABNORMALTYPEID", sqlWhere);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    Abnormal abnormal = new Abnormal();
                    abnormal.AbnormalId = LibSysUtils.ToString(reader["ABNORMALID"]);
                    abnormal.AbnormalName = LibSysUtils.ToString(reader["ABNORMALNAME"]);
                    abnormal.AbnormalTypeId = LibSysUtils.ToString(reader["ABNORMALTYPEID"]);
                    abnormal.AbnormalTypeName = LibSysUtils.ToString(reader["ABNORMALTYPENAME"]);
                    abnormal.Bizattr = LibSysUtils.ToInt32(reader["BIZATTR"]);
                    abnormal.ChangeType = LibSysUtils.ToInt32(reader["CHANGETYPE"]);
                    abnormal.DeptId = LibSysUtils.ToString(reader["DEPTID"]);
                    abnormal.DeptName = LibSysUtils.ToString(reader["DEPTNAME"]);
                    abnormal.PersonId = LibSysUtils.ToString(reader["PERSONID"]);
                    abnormalList.Add(abnormal);
                }
            }
            return abnormalList;
        }

        /// <summary>
        /// 获取异常数据（异常结束界面数据源）
        /// </summary>
        /// <param name="sqlWhere">SQL条件</param>
        /// <returns></returns>
        public List<Abnormal> GetAbnormalDate(string sqlWhere)
        {
            List<string> taskNoList = new List<string>();
            List<Abnormal> abnormalList = new List<Abnormal>();
            string sql = string.Format(@"SELECT DISTINCT A.*, B.WORKORDERNO, B.LOTNO, B.GROUPNO, B.SALEORDERSTATE
                                                    FROM (SELECT A.BILLNO,
                                                    A.BILLDATE,
                                                    A.STARTTIME,
                                                    A.DEPTID,
                                                    B.DEPTNAME,
                                                    A.PERSONID,
                                                    C.PERSONNAME,
                                                    A.ABNORMALDESC,
                                                    A.FROMMARK,
                                                    SUBSTR(A.FROMMARK, 0, 18) AS TASKNO,
                                                    A.AFFECTPRODUCESTATE
                                                    FROM COMABNORMALREPORT A
                                                    LEFT JOIN COMDEPT B
                                                    ON B.DEPTID = A.DEPTID
                                                    LEFT JOIN COMPERSON C
                                                    ON C.PERSONID = A.PERSONID
                                                    WHERE {0}) A
                                                    LEFT JOIN PPTENWORKRECORD B
                                                    ON A.TASKNO = B.TASKNO", sqlWhere);
            try
            {
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        Abnormal abnormal = new Abnormal();
                        abnormal.BillNo = LibSysUtils.ToString(reader["BILLNO"]);
                        abnormal.BillDate = LibSysUtils.ToInt32(reader["BILLDATE"]);
                        abnormal.AbnormalStartTime = LibSysUtils.ToInt64(reader["STARTTIME"]);
                        abnormal.DeptId = LibSysUtils.ToString(reader["DEPTID"]);
                        abnormal.DeptName = LibSysUtils.ToString(reader["DEPTNAME"]);
                        abnormal.PersonId = LibSysUtils.ToString(reader["PERSONID"]);
                        abnormal.PersonName = LibSysUtils.ToString(reader["PERSONNAME"]); ;
                        abnormal.AbnormalDesc = LibSysUtils.ToString(reader["ABNORMALDESC"]);
                        abnormal.FromMark = LibSysUtils.ToString(reader["FROMMARK"]);
                        abnormal.AffectProduceState = LibSysUtils.ToInt32(reader["AFFECTPRODUCESTATE"]);
                        abnormal.LotNo = LibSysUtils.ToString(reader["LOTNO"]);
                        abnormal.GroupNo = LibSysUtils.ToString(reader["GROUPNO"]);
                        abnormal.SaleOrderState = LibSysUtils.ToInt32(reader["SALEORDERSTATE"]);
                        abnormal.WorkOrderNo = LibSysUtils.ToString(reader["WORKORDERNO"]);
                        abnormal.TaskNo = LibSysUtils.ToString(reader["TASKNO"]);
                        string taskNo = abnormal.FromMark.Split('-')[0];
                        abnormalList.Add(abnormal);
                    }
                }
            }
            catch (Exception)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error,string.Format("异常数据查询有误"));
            }
            
            return abnormalList;
        }

        /// <summary>
        /// 异常数据处理（异常结束、跳单、跳十单）
        /// </summary>
        /// <param name="produceBcfData">单个任务信息</param>
        /// <returns>是否处理完成</returns>
        public bool UpdateAbnormalData(ProduceBcfData produceBcfData)
        {
            bool abnormalResult = false;
            List<string> sql = new List<string>();
            HYProduceData hyProduce = LibHYProduceCache.Default.GetProduceData(produceBcfData.BillNo);
            if (hyProduce == null)
                return abnormalResult;
            switch (produceBcfData.TenAbnormal)
            {
                //单个锁定，一般异常
                case TenAbnormType.SingleLock:
                    sql.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STATES = 6 WHERE WORKORDERNO = {0} AND TASKNO = {1} AND WORKPROCESSNO = {2} AND WORKSTATIONID = {3}", LibStringBuilder.GetQuotString(produceBcfData.BillNo), LibStringBuilder.GetQuotString(produceBcfData.TaskNo), produceBcfData.WorkProcessNo, LibStringBuilder.GetQuotString(produceBcfData.WorkstationId)));
                    break;
                //异常锁定,紧急异常
                case TenAbnormType.AbnormalLock:
                    sql.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STATES = 6 WHERE WORKORDERNO = {0} AND TASKNO = {1} AND STATES <> 5", LibStringBuilder.GetQuotString(produceBcfData.BillNo), LibStringBuilder.GetQuotString(produceBcfData.TaskNo)));
                    break;
                //跳单【生产批号】
                case TenAbnormType.LotNoLock:
                    sql.Add(string.Format(@"UPDATE PPTENWORKRECORD SET SALEORDERSTATE = 1 WHERE WORKORDERNO = {0} AND LOTNO = {1} ", LibStringBuilder.GetQuotString(produceBcfData.BillNo), LibStringBuilder.GetQuotString(produceBcfData.LotNo)));
                    break;
                //单个解锁
                case TenAbnormType.SingleUnLock:
                    sql.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STATES = 0 WHERE WORKORDERNO = {0} AND TASKNO = {1} AND WORKPROCESSNO = {2} AND WORKSTATIONID = {3}", LibStringBuilder.GetQuotString(produceBcfData.BillNo), LibStringBuilder.GetQuotString(produceBcfData.TaskNo), produceBcfData.WorkProcessNo, LibStringBuilder.GetQuotString(produceBcfData.WorkstationId)));
                    sql.Add(string.Format(@"UPDATE COMABNORMALTRACE
                    SET DEALWITHSTATE = 2, SOLUTION = '已解决'
                    WHERE FROMBILLNO = (SELECT BILLNO
                    FROM COMABNORMALREPORT
                    WHERE TYPEID = 'PP1'
                    AND FROMMARK = '{0}'
                    AND DEALWITHSTATE = 0)", produceBcfData.TaskNo + "-P"));
                    sql.Add(string.Format(@"UPDATE COMABNORMALREPORT
                    SET DEALWITHSTATE = 2, ENDTIME = {0}
                    WHERE TYPEID = 'PP1'
                    AND FROMMARK = '{1}'
                    AND DEALWITHSTATE = 0", LibDateUtils.GetCurrentDateTime(), produceBcfData.TaskNo + "-P"));
                    break;
                //异常解锁
                case TenAbnormType.AbnormalUnLock:
                    sql.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STATES = 0,STARTTIME = 0,DEFECTSTATE = 1 WHERE WORKORDERNO = {0} AND TASKNO = {1} AND STATES <> 5", LibStringBuilder.GetQuotString(produceBcfData.BillNo), LibStringBuilder.GetQuotString(produceBcfData.TaskNo)));
                    sql.Add(string.Format(@"UPDATE COMABNORMALTRACE
                    SET DEALWITHSTATE = 2, SOLUTION = '已解决'
                    WHERE FROMBILLNO = (SELECT BILLNO
                    FROM COMABNORMALREPORT
                    WHERE TYPEID = 'PP1'
                    AND FROMMARK = '{0}'
                    AND DEALWITHSTATE = 0)", produceBcfData.TaskNo + "-P"));
                    sql.Add(string.Format(@"UPDATE COMABNORMALREPORT
                    SET DEALWITHSTATE = 2, ENDTIME = {0}
                    WHERE TYPEID = 'PP1'
                    AND FROMMARK = '{1}'
                    AND DEALWITHSTATE = 0", LibDateUtils.GetCurrentDateTime(), produceBcfData.TaskNo + "-P"));
                    break;
                //跳单解锁
                case TenAbnormType.LotNoUnLock:
                    sql.Add(string.Format(@"UPDATE PPTENWORKRECORD SET SALEORDERSTATE = 0 WHERE WORKORDERNO = {0} AND LOTNO = {1} ", LibStringBuilder.GetQuotString(produceBcfData.BillNo), LibStringBuilder.GetQuotString(produceBcfData.LotNo)));
                    sql.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STATES = 0,STARTTIME = 0,DEFECTSTATE=2 WHERE WORKORDERNO = {0} AND LOTNO = {1} AND STATES <> 5", LibStringBuilder.GetQuotString(produceBcfData.BillNo), LibStringBuilder.GetQuotString(produceBcfData.LotNo)));
                    sql.Add(string.Format(@"UPDATE COMABNORMALTRACE
                    SET DEALWITHSTATE = 2, SOLUTION = '已解决'
                    WHERE FROMBILLNO = (SELECT BILLNO
                    FROM COMABNORMALREPORT
                    WHERE TYPEID = 'PP1'
                    AND FROMMARK = '{0}'
                    AND DEALWITHSTATE = 0)", produceBcfData.TaskNo + "-P"));
                    sql.Add(string.Format(@"UPDATE COMABNORMALREPORT
                    SET DEALWITHSTATE = 2, ENDTIME = {0}
                    WHERE TYPEID = 'PP1'
                    AND FROMMARK = '{1}'
                    AND DEALWITHSTATE = 0", LibDateUtils.GetCurrentDateTime(), produceBcfData.TaskNo + "-P"));
                    break;
                case TenAbnormType.LotNoOutLock:
                    sql.Add(string.Format(@"UPDATE PPTENWORKRECORD SET SALEORDERSTATE = 2 WHERE WORKORDERNO = {0} AND LOTNO = {1} ", LibStringBuilder.GetQuotString(produceBcfData.BillNo), LibStringBuilder.GetQuotString(produceBcfData.LotNo)));
                    break;
                case TenAbnormType.LotNoOutUnLock:
                    sql.Add(string.Format(@"UPDATE PPTENWORKRECORD SET SALEORDERSTATE = 0 WHERE WORKORDERNO = {0} AND LOTNO = {1} ", LibStringBuilder.GetQuotString(produceBcfData.BillNo), LibStringBuilder.GetQuotString(produceBcfData.LotNo)));
                    sql.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STATES = 0,STARTTIME = 0,DEFECTSTATE = 3 WHERE WORKORDERNO = {0} AND LOTNO = {1} AND STATES <> 5", LibStringBuilder.GetQuotString(produceBcfData.BillNo), LibStringBuilder.GetQuotString(produceBcfData.LotNo)));
                    sql.Add(string.Format(@"UPDATE COMABNORMALTRACE
                    SET DEALWITHSTATE = 2, SOLUTION = '已解决'
                    WHERE FROMBILLNO = (SELECT BILLNO
                    FROM COMABNORMALREPORT
                    WHERE TYPEID = 'PP1'
                    AND FROMMARK = '{0}'
                    AND DEALWITHSTATE = 0)", produceBcfData.TaskNo + "-P"));
                    sql.Add(string.Format(@"UPDATE COMABNORMALREPORT
                    SET DEALWITHSTATE = 2, ENDTIME = {0}
                    WHERE TYPEID = 'PP1'
                    AND FROMMARK = '{1}'
                    AND DEALWITHSTATE = 0", LibDateUtils.GetCurrentDateTime(), produceBcfData.TaskNo + "-P"));
                    break;
            }
            if (sql.Count > 0)
            {
                LibDBTransaction trans = this.DataAccess.BeginTransaction();
                try
                {
                    int result = this.DataAccess.ExecuteNonQuery(sql);
                    if (result > 0)
                    {
                        trans.Commit();
                        abnormalResult = true;
                    }
                    else
                    {
                        trans.Rollback();
                        return abnormalResult;
                    }
                }
                catch
                {
                    trans.Rollback();
                    return abnormalResult;
                }
            }
            foreach (DataRow row in hyProduce.TenWorkRecord.Tables[0].Rows)
            {
                string taskNo = LibSysUtils.ToString(row["TASKNO"]);
                string workOrderNo = LibSysUtils.ToString(row["WORKORDERNO"]);
                int workProcessNo = LibSysUtils.ToInt32(row["WORKPROCESSNO"]);
                string workstationId = LibSysUtils.ToString(row["WORKSTATIONID"]);
                string lotNo = LibSysUtils.ToString(row["LOTNO"]);
                int states = LibSysUtils.ToInt32(row["STATES"]);
                switch (produceBcfData.TenAbnormal)
                {
                    case TenAbnormType.SingleLock:

                        if (taskNo == produceBcfData.TaskNo && workProcessNo == produceBcfData.WorkProcessNo && workOrderNo == produceBcfData.BillNo && workstationId == produceBcfData.WorkstationId)
                        {
                            row["STATES"] = 6;
                        }
                        break;
                    case TenAbnormType.AbnormalLock:
                        if (taskNo == produceBcfData.TaskNo && workOrderNo == produceBcfData.BillNo && states != 5)
                        {
                            row["STATES"] = 6;
                        }
                        break;
                    case TenAbnormType.LotNoLock:
                        if (workOrderNo == produceBcfData.BillNo && lotNo == produceBcfData.LotNo)
                        {
                            row["SALEORDERSTATE"] = 1;
                        }
                        break;
                    case TenAbnormType.SingleUnLock:
                        if (taskNo == produceBcfData.TaskNo && workProcessNo == produceBcfData.WorkProcessNo && workOrderNo == produceBcfData.BillNo && workstationId == produceBcfData.WorkstationId)
                        {
                            row["STATES"] = 0;
                        }
                        break;
                    case TenAbnormType.AbnormalUnLock:
                        if (taskNo == produceBcfData.TaskNo && workOrderNo == produceBcfData.BillNo && states != 5)
                        {
                            row["STATES"] = 0;
                            row["STARTTIME"] = 0;
                            row["DEFECTSTATE"] = 1;
                            //row["COOPERATESTATE"] = 0;
                        }
                        break;
                    case TenAbnormType.LotNoUnLock:
                        if (workOrderNo == produceBcfData.BillNo && lotNo == produceBcfData.LotNo)
                        {
                            row["SALEORDERSTATE"] = 0;
                            if (LibSysUtils.ToInt32(row["STATES"]) != 5)
                            {
                                row["STATES"] = 0;
                                row["STARTTIME"] = 0;
                                row["DEFECTSTATE"] = 2;
                                //row["COOPERATESTATE"] = 0;
                            }
                        }
                        break;
                    case TenAbnormType.LotNoOutLock:
                        if (workOrderNo == produceBcfData.BillNo && lotNo == produceBcfData.LotNo)
                        {
                            row["SALEORDERSTATE"] = 2;
                        }
                        break;
                    case TenAbnormType.LotNoOutUnLock:
                        if (workOrderNo == produceBcfData.BillNo && lotNo == produceBcfData.LotNo)
                        {
                            row["SALEORDERSTATE"] = 0;
                            if (LibSysUtils.ToInt32(row["STATES"]) != 5)
                            {
                                row["STATES"] = 0;
                                row["STARTTIME"] = 0;
                                row["DEFECTSTATE"] = 3;
                                //row["COOPERATESTATE"] = 0;
                            }
                        }
                        break;
                }
            }
            return abnormalResult;
        }

        /// <summary>
        /// 获取未扫码数据
        /// </summary>
        /// <param name="produceLineId">产线编号</param>
        /// <returns>漏扫任务集合</returns>
        public List<TenWorkRecord> GetUnScanData(string produceLineId)
        {
            List<TenWorkRecord> unScanList = new List<TenWorkRecord>();
            string sql = string.Format(@"SELECT DISTINCT A.ORDERDATE,
                                                    A.ORDERNUM,
                                                    A.FROMTYPE,
                                                    A.PRODUCELINEID,
                                                    A.PRODUCELINENAME,
                                                    B.LOTNO,
                                                    B.GROUPNO,
                                                    B.TASKNO,
                                                    B.LINKBARCODE,
                                                    B.UNITNUM,
                                                    B.FINISHNUM,
                                                    B.ATTRIBUTEDESC,
                                                    B.MATERIALNAME,
                                                    B.LABELTEMPLATEID,
                                                    B.WORKORDERNO,
                                                    B.BARCODE,
                                                    B.WORKPROCESSID,
                                                    A.PLSWORKORDERNO
                                                    FROM PPMAINTENWORKRECORD A
                                                    LEFT JOIN PPTENWORKRECORD B
                                                    ON A.WORKORDERNO = B.WORKORDERNO
                                                    WHERE A.STARTSTATE = 1
                                                    AND A.PRODUCELINEID = {0}
                                                    AND B.WORKPROCESSNO = A.LASTWORKPROCESSNO
                                                    AND B.STATES <> 5
                                                    ORDER BY A.ORDERDATE, A.ORDERNUM, B.LOTNO, B.GROUPNO", LibStringBuilder.GetQuotString(produceLineId));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    TenWorkRecord ten = new TenWorkRecord();
                    ten.PlsWorkOrderNo = LibSysUtils.ToString(reader["PLSWORKORDERNO"]);
                    ten.OrderDate = LibSysUtils.ToInt32(reader["ORDERDATE"]);
                    ten.OrderNum = LibSysUtils.ToInt32(reader["ORDERNUM"]);
                    ten.FromType = LibSysUtils.ToInt32(reader["FROMTYPE"]);
                    ten.ProduceLineId = LibSysUtils.ToString(reader["PRODUCELINEID"]);
                    ten.ProduceLineName = LibSysUtils.ToString(reader["PRODUCELINENAME"]);
                    ten.LotNo = LibSysUtils.ToString(reader["LOTNO"]);
                    ten.GroupNo = LibSysUtils.ToString(reader["GROUPNO"]);
                    ten.TaskNo = LibSysUtils.ToString(reader["TASKNO"]);
                    ten.LinkBarcode = LibSysUtils.ToString(reader["LINKBARCODE"]);
                    ten.UnitNum = LibSysUtils.ToInt32(reader["UNITNUM"]);
                    ten.FinishNum = LibSysUtils.ToInt32(reader["FINISHNUM"]);
                    ten.AttributeDesc = LibSysUtils.ToString(reader["ATTRIBUTEDESC"]);
                    ten.MaterialName = LibSysUtils.ToString(reader["MATERIALNAME"]);
                    ten.LabelTemplateId = LibSysUtils.ToString(reader["LABELTEMPLATEID"]);
                    ten.WorkOrderNo = LibSysUtils.ToString(reader["WORKORDERNO"]);
                    ten.Barcode = LibSysUtils.ToString(reader["BARCODE"]);
                    ten.WorkProcessId = LibSysUtils.ToString(reader["WORKPROCESSID"]);
                    unScanList.Add(ten);
                }
            }
            return unScanList;
        }

        /// <summary>
        /// 获取十单任务号数据
        /// </summary>
        /// <param name="taskNo">任务号</param>
        /// <returns>单个任务信息</returns>
        public TenWorkRecord GetTenData(string taskNo)
        {
            TenWorkRecord ten = new TenWorkRecord();
            string sql = string.Format(@"SELECT DISTINCT WORKORDERNO,TASKNO,LOTNO,SALEORDERSTATE FROM PPTENWORKRECORD WHERE TASKNO = '{0}'", taskNo);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    ten.WorkOrderNo = LibSysUtils.ToString(reader["WORKORDERNO"]);
                    ten.TaskNo = LibSysUtils.ToString(reader["TASKNO"]);
                    ten.LotNo = LibSysUtils.ToString(reader["LOTNO"]);
                    ten.SaleOrderState = LibSysUtils.ToInt32(reader["SALEORDERSTATE"]);
                }
            }
            return ten;
        }

        /// <summary>
        /// 读取缓存数据
        /// </summary>
        /// <param name="sqlWhere">SQL条件</param>
        /// <param name="billNo">派工单号</param>
        /// <returns>缓存集合</returns>
        public List<TenWorkRecord> GetCacheData(string sqlWhere, string billNo)
        {
            List<TenWorkRecord> tenList = new List<TenWorkRecord>();
            HYProduceData hyProduce = LibHYProduceCache.Default.GetProduceData(billNo);
            if (hyProduce == null)
                return tenList;
            DataRow[] dataRows = hyProduce.TenWorkRecord.Tables[0].Select(string.Format("{0}", string.IsNullOrEmpty(sqlWhere) ? " 1=1 " : sqlWhere), "ORDERINDEX,SCANNUM");
            foreach (DataRow row in dataRows)
            {
                tenList.Add(GetWorkRecord(row));
            }
            return tenList;
        }

        /// <summary>
        /// 更新缓存数据
        /// </summary>
        /// <param name="ten">单条部件信息</param>
        /// <returns></returns>
        public bool UpdateCacheData(TenWorkRecord ten)
        {
            bool result = false;
            List<string> sqlList = new List<string>();
            HYProduceData hyProduce = LibHYProduceCache.Default.GetProduceData(ten.WorkOrderNo);
            if (hyProduce == null)
                return result;
            DataTable dt = hyProduce.TenWorkRecord.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                string workOrderNo = LibSysUtils.ToString(row["WORKORDERNO"]);
                string taskNo = LibSysUtils.ToString(row["TASKNO"]);
                string workProcessId = LibSysUtils.ToString(row["WORKPROCESSID"]);
                string linkBarcode = LibSysUtils.ToString(row["LINKBARCODE"]);
                if (workOrderNo == ten.WorkOrderNo && taskNo == ten.TaskNo && workProcessId == ten.WorkProcessId && linkBarcode == ten.LinkBarcode)
                {
                    row.BeginEdit();
                    row["STATES"] = ten.States;
                    row["STARTTIME"] = ten.StartTime;
                    row["FINISHTIME"] = ten.FinishTime;
                    row["STARTSTATE"] = ten.StartState;
                    row["WORKSTATIONID"] = ten.WorkstationId;
                    row["COOPERATESTATE"] = ten.CooprateState;
                    row["ORDERNUM"] = ten.OrderNum;
                    row["ORDERDATE"] = ten.OrderDate;
                    row["SALEORDERSTATE"] = ten.SaleOrderState;
                    row["LABELTEMPLATEID"] = ten.LabelTemplateId;
                    row.EndEdit();
                    sqlList.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STATES = {0},STARTTIME = {1},FINISHTIME = {2},STARTSTATE = {3},
                    WORKSTATIONID = '{4}',COOPERATESTATE = {5},ORDERNUM = {6},ORDERDATE = {7},SALEORDERSTATE = {8},LABELTEMPLATEID = '{9}'
                     WHERE WORKORDERNO = '{10}' AND TASKNO = '{11}' AND WORKPROCESSID = '{12}' AND LINKBARCODE = '{13}' ",
                     ten.States, ten.StartTime, ten.FinishTime, ten.StartState, ten.WorkstationId, ten.CooprateState, ten.OrderNum, ten.OrderDate
                     , ten.SaleOrderState, ten.LabelTemplateId, ten.WorkOrderNo, ten.TaskNo, ten.WorkProcessId, ten.LinkBarcode));
                    result = true;
                }
            }
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                int updateCount = this.DataAccess.ExecuteNonQuery(sqlList);
                if (updateCount <= 0)
                {
                    trans.Rollback();
                    result = false;
                }
                else
                {
                    trans.Commit();
                }
            }
            catch
            {
                trans.Rollback();
                result = false;
                return result;
            }
            return result;
        }

        /// <summary>
        /// 获取可完结/可撤销/可结束【内部使用】的信息
        /// </summary>
        /// <param name="sqlWhere">SQL条件</param>
        /// <param name="finishType">0：可完结 1：可撤销 2：可结束</param>
        /// <returns>任务清单集合</returns>
        public List<TenWorkRecord> GetFinishMessage(string sqlWhere, int finishType)
        {
            List<TenWorkRecord> finishMessageList = new List<TenWorkRecord>();
            string sql = string.Empty;
            switch (finishType)
            {
                case 0:
                    sql = string.Format(@"SELECT DISTINCT A.ORDERDATE,
                                                        A.ORDERNUM,
                                                        A.PLSWORKORDERNO,
                                                        A.WORKORDERNO,
                                                        A.PRODUCELINEID,
                                                        A.PRODUCELINENAME
                                                        FROM PPMAINTENWORKRECORD A
                                                        LEFT JOIN PPTENWORKRECORD B
                                                        ON A.WORKORDERNO = B.WORKORDERNO
                                                        WHERE A.STARTSTATE = 1
                                                        AND A.WORKSHOPSECTIONID = '{0}'
                                                        AND B.WORKPROCESSNO = A.LASTWORKPROCESSNO
                                                        AND A.WORKORDERNO NOT IN (SELECT DISTINCT C.WORKORDERNO
                                                        FROM PPMAINTENWORKRECORD C
                                                        LEFT JOIN PPTENWORKRECORD D
                                                        ON C.WORKORDERNO = D.WORKORDERNO
                                                        WHERE C.STARTSTATE = 1
                                                        AND C.WORKSHOPSECTIONID = '{0}'
                                                        AND D.WORKPROCESSNO = C.LASTWORKPROCESSNO
                                                        AND D.STATES <> 5)
                                                        ORDER BY ORDERDATE, ORDERNUM", string.IsNullOrEmpty(sqlWhere) ? " 1=1 " : sqlWhere); break;
                case 1:
                    sql = string.Format(@"SELECT DISTINCT A.ORDERDATE,
                                                        A.ORDERNUM,
                                                        A.PLSWORKORDERNO,
                                                        A.WORKORDERNO,
                                                        A.PRODUCELINEID,
                                                        A.PRODUCELINENAME
                                                        FROM PPMAINTENWORKRECORD A
                                                        LEFT JOIN PPTENWORKRECORD B
                                                        ON A.WORKORDERNO = B.WORKORDERNO
                                                        WHERE A.STARTSTATE = 1
                                                        AND A.WORKSHOPSECTIONID = '{0}'
                                                        AND B.WORKPROCESSNO = A.FIRSTWORKPROCESSNO
                                                        AND A.WORKORDERNO NOT IN (SELECT DISTINCT C.WORKORDERNO
                                                        FROM PPMAINTENWORKRECORD C
                                                        LEFT JOIN PPTENWORKRECORD D
                                                        ON C.WORKORDERNO = D.WORKORDERNO
                                                        WHERE C.STARTSTATE = 1
                                                        AND C.WORKSHOPSECTIONID = '{0}'
                                                        AND D.WORKPROCESSNO = C.FIRSTWORKPROCESSNO
                                                        AND D.STATES = 5)
                                                        ORDER BY ORDERDATE, ORDERNUM", string.IsNullOrEmpty(sqlWhere) ? " 1=1 " : sqlWhere);
                    break;
                case 2:
                    sql = string.Format(@" SELECT DISTINCT A.ORDERDATE,
                                                        A.ORDERNUM,
                                                        A.PLSWORKORDERNO,
                                                        A.WORKORDERNO,
                                                        A.PRODUCELINEID,
                                                        A.PRODUCELINENAME
                                                        FROM PPMAINTENWORKRECORD A
                                                        LEFT JOIN PPTENWORKRECORD B
                                                        ON A.WORKORDERNO = B.WORKORDERNO
                                                        WHERE A.STARTSTATE = 1
                                                        AND A.WORKSHOPSECTIONID = '{0}'
                                                        ORDER BY ORDERDATE, ORDERNUM", string.IsNullOrEmpty(sqlWhere) ? " 1=1 " : sqlWhere);
                    break;
            }
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    finishMessageList.Add(new TenWorkRecord()
                    {
                        OrderDate = LibSysUtils.ToInt32(reader["ORDERDATE"]),
                        OrderNum = LibSysUtils.ToInt32(reader["ORDERNUM"]),
                        PlsWorkOrderNo = LibSysUtils.ToString(reader["PLSWORKORDERNO"]),
                        WorkOrderNo = LibSysUtils.ToString(reader["WORKORDERNO"]),
                        ProduceLineId = LibSysUtils.ToString(reader["PRODUCELINEID"]),
                        ProduceLineName = LibSysUtils.ToString(reader["PRODUCELINENAME"])
                        //ScanNum = LibSysUtils.ToInt32(reader["SCANNUM"])
                    });
                }
            }
            return finishMessageList;
        }

        /// <summary>
        /// 移除完工/撤销未开始的十单
        /// </summary>
        /// <param name="workOrderNo">派工单号</param>
        /// <param name="plsWorkOrderNo">作业单号</param>
        /// <param name="finishType">0：可完结 1：可撤销 2：可结束</param>
        /// <returns>是否移除成功</returns>
        public bool UpdateTenState(string workOrderNo, string plsWorkOrderNo, int finishType)
        {
            List<string> sqlList = new List<string>();
            //string sql = string.Empty;
            HYProduceData produceData = LibHYProduceCache.Default.GetProduceData(workOrderNo);
            if (produceData != null)
            {
                switch (finishType)
                {
                    case 0:
                        sqlList.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STARTSTATE = 3,SENDFINISHTIME = {2} WHERE PLSWORKORDERNO = '{0}' AND WORKORDERNO = '{1}'",
                                plsWorkOrderNo, workOrderNo,LibDateUtils.GetCurrentDateTime()));
                        sqlList.Add(string.Format("UPDATE PPWORKORDER SET STARTSTATE = 4 WHERE BILLNO = '{0}'", workOrderNo));
                        sqlList.Add(string.Format("UPDATE PPMAINTENWORKRECORD SET STARTSTATE = 3,FINISHTIME = {1} WHERE WORKORDERNO = '{0}'", workOrderNo,LibDateUtils.GetCurrentDateTime()));
                        break;
                    case 1:
                        sqlList.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STARTSTATE = 0,SENDSTARTTIME = 0 WHERE PLSWORKORDERNO = '{0}' AND WORKORDERNO = '{1}'",
                                plsWorkOrderNo, workOrderNo));
                        sqlList.Add(string.Format("UPDATE PPWORKORDER SET STARTSTATE = 0 WHERE BILLNO = '{0}'", workOrderNo));
                        sqlList.Add(string.Format("UPDATE PPMAINTENWORKRECORD SET STARTSTATE = 0,STARTTIME = 0 WHERE WORKORDERNO = '{0}'", workOrderNo));
                        break;
                    case 2:
                        sqlList.Add(string.Format(@"UPDATE PPTENWORKRECORD SET STARTSTATE = 3,SENDFINISHTIME = {2} WHERE PLSWORKORDERNO = '{0}' AND WORKORDERNO = '{1}'",
                                plsWorkOrderNo, workOrderNo,LibDateUtils.GetCurrentDateTime()));
                        sqlList.Add(string.Format("UPDATE PPWORKORDER SET STARTSTATE = 4 WHERE BILLNO = '{0}'", workOrderNo));
                        sqlList.Add(string.Format("UPDATE PPMAINTENWORKRECORD SET STARTSTATE = 3,FINISHTIME = {1} WHERE WORKORDERNO = '{0}'", workOrderNo, LibDateUtils.GetCurrentDateTime()));
                        break;
                }

                LibDBTransaction trans = this.DataAccess.BeginTransaction();
                try
                {
                    int result = this.DataAccess.ExecuteNonQuery(sqlList);
                    if (result > 0)
                    {
                        trans.Commit();
                        LibHYProduceCache.Default.Remove(workOrderNo);
                        LibHYControlServer.Default.RemoveWorkOrder(workOrderNo);
                        if (finishType!=1)
                        {
                            LibProduceCache.Default.Remove(workOrderNo);
                            LibWsControlServer.Default.RemoveWorkOrder(workOrderNo);
                        }
                        return true;
                    }
                }
                catch
                {
                    trans.Rollback();
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 备料齐套验证
        /// </summary>
        /// <param name="ppWorkOrderNo">派工单号</param>
        /// <param name="materialType">0：通用件 1：非标专有件 2：衍生品</param>
        /// <returns></returns>
        public List<string> ArriveMatKit(string ppWorkOrderNo, int materialType)
        {
            List<string> workOrderNoList = new List<string>();
            string sqlWhere = string.Empty;
            string sql = string.Empty;
            if (ppWorkOrderNo.Length > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("AND A.PPWORKORDER IN (");
                foreach (string workOrderNo in ppWorkOrderNo.Split(','))
                {
                    if (!string.IsNullOrEmpty(workOrderNo))
                    {
                        builder.AppendFormat(" {0},", LibStringBuilder.GetQuotString(workOrderNo));
                    }
                }
                sqlWhere = builder.ToString().Substring(0, builder.ToString().LastIndexOf(','));
                sqlWhere += ")";
                LibDataAccess dataAccess = new LibDataAccess();
                
                switch (materialType)
                {
                    case 0:
                        sql = string.Format("SELECT A.PPWORKORDER,COUNT(A.TASKNO) AS TASKNUM FROM PLSMATERIALSTOCKTASK A WHERE (A.DELIVERQUANTITY - A.ACTUALDELIVERQTY) > 0 {0} GROUP BY A.PPWORKORDER", sqlWhere);
                        break;
                    case 1:
                        sql = string.Format("SELECT A.PPWORKORDER,COUNT(A.TASKNO) AS TASKNUM FROM PLSSPECIALMATSTOCKTASK A WHERE MATERIALTYPE = 1 AND (A.DELIVERQUANTITY - A.ACTUALDELIVERQTY) > 0 {0} GROUP BY A.PPWORKORDER", sqlWhere);
                        break;
                    case 2:
                        sql = string.Format("SELECT A.PPWORKORDER,COUNT(A.TASKNO) AS TASKNUM FROM PLSSPECIALMATSTOCKTASK A WHERE  MATERIALTYPE = 0 AND (A.DELIVERQUANTITY - A.ACTUALDELIVERQTY) > 0 {0} GROUP BY A.PPWORKORDER", sqlWhere);
                        break;
                    default:
                        break;
                }
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        int taskNum = LibSysUtils.ToInt32(reader["TASKNUM"]);
                        string orderNo = LibSysUtils.ToString(reader["PPWORKORDER"]);
                        if (taskNum == 0)
                        {
                            if (!workOrderNoList.Contains(orderNo))
                            {
                                workOrderNoList.Add(orderNo);
                            }
                        }
                    }
                }
            }
            return workOrderNoList;
        }

        /// <summary>
        /// 获取重复打码信息
        /// </summary>
        /// <param name="orderDate">十单日期</param>
        /// <param name="orderNum">十单顺序号</param>
        /// <param name="lotNo">生产批号</param>
        /// <param name="groupNo">组号</param>
        /// <param name="produceLineId">产线编号</param>
        /// <returns></returns>
        public List<TenWorkRecord> GetPrintInfo(int orderDate,int orderNum,string lotNo,string groupNo,string produceLineId)
        {
            List<TenWorkRecord> tenList = new List<TenWorkRecord>();
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("WHERE 1 = 1");
            if (orderDate != 0) stringBuilder.Append(string.Format(@" AND ORDERDATE = {0}",orderDate));
            if (orderNum != 0) stringBuilder.Append(string.Format(@" AND ORDERNUM = {0}", orderNum));
            if (!string.IsNullOrEmpty(lotNo)) stringBuilder.Append(string.Format(@" AND LOTNO = '{0}'", lotNo));
            if (!string.IsNullOrEmpty(groupNo)) stringBuilder.Append(string.Format(@" AND GROUPNO = '{0}'", groupNo));
            if (!string.IsNullOrEmpty(produceLineId)) stringBuilder.Append(string.Format(@" AND PRODUCELINEID = '{0}'", produceLineId));
            string sql = string.Format(@"SELECT * FROM PPTENWORKRECORD {0} AND WORKPROCESSNO IN (SELECT MAX(WORKPROCESSNO) FROM PPTENWORKRECORD {0})",LibSysUtils.ToString(stringBuilder));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    tenList.Add(GetWorkRecord(reader));
                }
            }
            return tenList;
        }

        /// <summary>
        /// 判断当前未完成的十单个数
        /// </summary>
        /// <param name="Num"></param>
        /// <param name="produceLineId">产线编号</param>
        /// <returns></returns>
        public List<TenWorkRecord> GetNotFinishOrder(int Num,string produceLineId)
        {
            List<TenWorkRecord> tenList = new List<TenWorkRecord>();
            string sql = string.Format(@"SELECT DISTINCT WORKORDERNO,ORDERDATE,ORDERNUM FROM PPMAINTENWORKRECORD WHERE PRODUCELINEID = '{0}' AND STARTSTATE = 1", produceLineId);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    tenList.Add(new TenWorkRecord() { 
                    WorkOrderNo = LibSysUtils.ToString(reader["WORKORDERNO"]),
                    OrderDate = LibSysUtils.ToInt32(reader["ORDERDATE"]),
                    OrderNum = LibSysUtils.ToInt32(reader["ORDERNUM"])
                    });
                }
            }
            return tenList;
        }

        /// <summary>
        /// 获取权限信息
        /// </summary>
        /// <param name="userId">人员编号</param>
        /// <returns>010101011111101</returns>
        public string GetPermission(string userId)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string sql = string.Format(@"SELECT * FROM AXPERMISSION WHERE PERSONID = '{0}'",userId);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    for (int i = 1; i < reader.FieldCount; i++)
                    {
                        stringBuilder.Append(LibSysUtils.ToString(reader[i]));
                    }
                }
            }
            return LibSysUtils.ToString(stringBuilder); ;
        }

        /// <summary>
        /// 获取控制参数信息
        /// </summary>
        /// <param name="paramContrilId">参数编号</param>
        /// <returns>0：是否启用 1：参数值</returns>
        public List<string> GetParam(string paramContrilId)
        {
            List<string> paramList = new List<string>();
            string sql = string.Format(@"SELECT  ISUSE,PARAMVALUE FROM AXPARAMCONTROL WHERE PARAMCONTRILID = '{0}'", paramContrilId);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    paramList.Add(LibSysUtils.ToString(reader["ISUSE"]));
                    paramList.Add(LibSysUtils.ToString(reader["PARAMVALUE"]));
                }
            }
            return paramList;
        }

        /// <summary>
        /// 修改参数
        /// </summary>
        /// <param name="paramContrilId">参数编号</param>
        /// <param name="isUse">是否开启</param>
        /// <param name="paramValue">参数值</param>
        /// <returns>是否修改完成</returns>
        public bool UpdateParam(string paramContrilId, int isUse, string paramValue)
        {
            bool updateResult = false;
            string sql = string.Format(@"UPDATE AXPARAMCONTROL SET ISUSE = '{0}',PARAMVALUE = '{1}' WHERE PARAMCONTRILID = '{2}'", isUse, paramValue, paramContrilId);
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                int result = this.DataAccess.ExecuteNonQuery(sql);
                trans.Commit();
                if (result > 0) updateResult = true;
                return updateResult;
            }
            catch (Exception )
            {
                trans.Rollback();
                return updateResult;
            }
        }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="workstationId">站点编号</param>
        /// <returns></returns>
        public List<string> GetCacheBillNo(string workstationId)
        {
            ProductScheduling wsPS = LibWsControlServer.Default.GetProductScheduling();
            List<string> billNoList = new List<string>();
            if (!string.IsNullOrEmpty(workstationId))
            {
                billNoList.AddRange(wsPS.WsRelWorkOrder[workstationId]);
            }
            else
            {
                billNoList.AddRange(wsPS.WorkOrderList);
            }
            return billNoList;
        }

        /// <summary>
        /// 获取缓存中的赋码派工单号
        /// </summary>
        /// <returns></returns>
        public List<string> GetBarcodeBillNo()
        {
            List<string> billNoList = new List<string>();
            string sql = string.Format(@"SELECT DISTINCT A.WORKORDERNO,
                                                        B.SENDSTARTTIME,
                                                        A.ORDERDATE,
                                                        A.FROMTYPE,
                                                        A.ORDERNUM
                                                        FROM PPMAINTENWORKRECORD A
                                                        LEFT JOIN PPTENWORKRECORD B
                                                        ON A.WORKORDERNO = B.WORKORDERNO AND A.FIRSTWORKPROCESSNO = B.WORKORDERNO
                                                        WHERE A.STARTSTATE = 1
                                                        ORDER BY B.SENDSTARTTIME, A.ORDERDATE, A.FROMTYPE, A.ORDERNUM");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    billNoList.Add(LibSysUtils.ToString(reader["WORKORDERNO"]));
                }
            }
            return billNoList;
        }

        /// <summary>
        /// 配置条件
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, SpecialMat> GetAdvanceMatDic()
        {
            Dictionary<string, SpecialMat> specialMatDic = new Dictionary<string, SpecialMat>();
            string sql = string.Format(@"SELECT DISTINCT   A.MATERIALID,
                                                    A.COMPLETENESS,
                                                    A.MATERIALID AS A_MATERIALID,
                                                    A.WORKSHOPSECTIONID,
                                                    B.MATERIALID AS B_MATERIALID,
                                                    B.PRODUCT,
                                                    B.ATTRIBUTEID,
                                                    B.ATTRIBUTEROWID,
                                                    B.ATTRIBUTEITEMID,
                                                    B.ATTRIBUTEITEMROWID,
                                                    C.ATTRCODE,
                                                    C.ATTRVALUE,
                                                    A.FACTORYID,
                                                    A.SPECIALMATTYPE,
                                                    B.WORKSHOPSECTIONID AS B_WORKSHOPSECTIONID,
                                                    B.FACTORYID AS B_FACTORYID,
                                                    B.SPECIALMATTYPE AS B_SPECIALMATTYPE,
                                                    B.OPERATOR,
                                                    B.OPERVALUE
                                      FROM COMMATERIAL A
                                      LEFT JOIN COMMATERIALMATINGNAME B
                                        ON A.MATERIALID = B.MATERIALID
                                      LEFT JOIN COMATTRIBUTEITEMDETAIL C
                                        ON B.ATTRIBUTEITEMID = C.ATTRIBUTEITEMID
                                       AND B.ATTRIBUTEITEMROWID = C.ROW_ID
                                           WHERE  (A.COMPLETENESS = 1 AND A.SPECIALMATTYPE = {0})  OR (B.MATERIALID IS NOT NULL AND  B.SPECIALMATTYPE = {0})", 1);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string materialId = LibSysUtils.ToString(reader["A_MATERIALID"]);
                    if (!specialMatDic.ContainsKey(materialId))
                    {
                        specialMatDic.Add(materialId, new SpecialMat() { IsSpecial = LibSysUtils.ToBoolean(reader["COMPLETENESS"]), WorkShopSectionId = LibSysUtils.ToString(reader["WORKSHOPSECTIONID"]), FactoryId = LibSysUtils.ToString(reader["FACTORYID"]), SpecialMatType = (SpecialMatType)LibSysUtils.ToInt32(reader["SPECIALMATTYPE"]) });
                        SpecialMat specialMat = specialMatDic[materialId];
                        if (!string.IsNullOrEmpty(LibSysUtils.ToString(reader["B_MATERIALID"])))
                        {
                            specialMat.SpecialMatDetail.Add(new SpecialMatDetail()
                            {
                                ProductId = LibSysUtils.ToString(reader["PRODUCT"]),
                                AttributeId = LibSysUtils.ToString(reader["ATTRIBUTEID"]),
                                AttributeRowId = LibSysUtils.ToInt32(reader["ATTRIBUTEROWID"]),
                                AttributeItemId = LibSysUtils.ToString(reader["ATTRIBUTEITEMID"]),
                                AttributeItemRowId = LibSysUtils.ToInt32(reader["ATTRIBUTEITEMROWID"]),
                                AttributeValue = LibSysUtils.ToString(reader["ATTRVALUE"]),
                                AttributeCode = LibSysUtils.ToString(reader["ATTRCODE"]),
                                FactoryId = LibSysUtils.ToString(reader["B_FACTORYID"]),
                                WorkShopSectionId = LibSysUtils.ToString(reader["B_WORKSHOPSECTIONID"]),
                                SpecialMatType = (SpecialMatType)LibSysUtils.ToInt32(reader["B_SPECIALMATTYPE"]),
                                Operator = LibSysUtils.ToInt32(reader["OPERATOR"]),
                                OperValue = LibSysUtils.ToDecimal(reader["OPERVALUE"])
                            });
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(LibSysUtils.ToString(reader["B_MATERIALID"])))
                        {
                            SpecialMat specialMat = specialMatDic[materialId];
                            specialMat.SpecialMatDetail.Add(new SpecialMatDetail()
                            {
                                ProductId = LibSysUtils.ToString(reader["PRODUCT"]),
                                AttributeId = LibSysUtils.ToString(reader["ATTRIBUTEID"]),
                                AttributeRowId = LibSysUtils.ToInt32(reader["ATTRIBUTEROWID"]),
                                AttributeItemId = LibSysUtils.ToString(reader["ATTRIBUTEITEMID"]),
                                AttributeItemRowId = LibSysUtils.ToInt32(reader["ATTRIBUTEITEMROWID"]),
                                AttributeValue = LibSysUtils.ToString(reader["ATTRVALUE"]),
                                AttributeCode = LibSysUtils.ToString(reader["ATTRCODE"]),
                                FactoryId = LibSysUtils.ToString(reader["B_FACTORYID"]),
                                WorkShopSectionId = LibSysUtils.ToString(reader["B_WORKSHOPSECTIONID"]),
                                SpecialMatType = (SpecialMatType)LibSysUtils.ToInt32(reader["B_SPECIALMATTYPE"]),
                                Operator = LibSysUtils.ToInt32(reader["OPERATOR"]),
                                OperValue = LibSysUtils.ToDecimal(reader["OPERVALUE"])
                            });
                        }
                    }
                }
            }
            return specialMatDic;
        }

        /// <summary>
        /// 获取Opc站点配置
        /// </summary>
        /// <param name="workstationId">设备编号</param>
        /// <returns></returns>
        public List<OpcConfig> GetOpcConfig(string workstationId)
        {
            List<string> paramNameList = new List<string>();
            List<OpcConfig> opcConfigList = new List<OpcConfig>();
            string sql = string.Format(@"SELECT B.PARAMNAME,B.DEFAULTVALUE,B.CURRENTVALUE, C.MATERIALNAME,                                                                                          C.PARAMVALUE,C.CURRENTVALUE AS DETAILVALUE
                                                        FROM COMOPCWORKSTATION A
                                                        LEFT JOIN COMOPCCONFIG B
                                                        ON A.OPCCONFIGID = B.OPCCONFIGID
                                                        LEFT JOIN COMOPCCONFIGDETAIL C
                                                        ON B.OPCCONFIGID = C.OPCCONFIGID
                                                        WHERE A.TERMINALID = '{0}'", workstationId);
            using (IDataReader reader = DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string paramName = LibSysUtils.ToString(reader["PARAMNAME"]);
                    if (!paramNameList.Contains(paramName))
                    {
                        OpcConfig opcConfig = new OpcConfig()
                        {
                            ParamName = paramName,
                            DefaultValue = LibSysUtils.ToInt32(reader["DEFAULTVALUE"]),
                            CurrentValue = LibSysUtils.ToInt32(reader["CURRENTVALUE"]),
                            ConfigDetail = new List<OpcConfigDetail>(){new OpcConfigDetail()
                            {
                                MaterialName = LibSysUtils.ToString(reader["MATERIALNAME"]),
                                ParamValue = LibSysUtils.ToInt32(reader["PARAMVALUE"]),
                                CurrentValue = LibSysUtils.ToInt32(reader["DETAILVALUE"])
                            }}
                        };
                        opcConfigList.Add(opcConfig);
                        paramNameList.Add(paramName);
                    }
                    else
                    {
                        foreach (var pair in opcConfigList.Where(pair => pair.ParamName == paramName))
                        {
                            pair.ConfigDetail.Add(new OpcConfigDetail
                            {
                                MaterialName = LibSysUtils.ToString(reader["MATERIALNAME"]),
                                ParamValue = LibSysUtils.ToInt32(reader["PARAMVALUE"]),
                                CurrentValue = LibSysUtils.ToInt32(reader["DETAILVALUE"])
                            });
                        }
                    }
                }
            }
            return opcConfigList;
        }

        /// <summary>
        /// 删除批号信息【退单\加急做批号剔除】
        /// </summary>
        /// <param name="workOrderNo">派工单号</param>
        /// <param name="fromSaleBillNo">销售订单号</param>
        public void RemoveLotNo(string workOrderNo, string fromSaleBillNo)
        {
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
            string sql = string.Format(@"DELETE FROM PPTENWORKRECORD WHERE WORKORDERNO = '{0}' AND FROMSALEBILLNO = '{1}'",workOrderNo,fromSaleBillNo);
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                int result = this.DataAccess.ExecuteNonQuery(sql);
                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
            }
        }

        /// <summary>
        /// 查询缓存明细
        /// </summary>
        /// <param name="workOrderNo"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public bool IsLiveCacheData(string workOrderNo, int dataType)
        {
            bool result = false;
            switch (dataType)
            {
                case 0:
                    ProduceData produceData = LibProduceCache.Default.GetProduceData(workOrderNo);
                    if (produceData != null)
                    {
                        DataTable dt = produceData.WorkOrder.Tables[3];
                        if (dt.Rows.Count > 0) result = true;
                    }
                    break;
                case 1:
                    HYProduceData hyProduceData = LibHYProduceCache.Default.GetProduceData(workOrderNo);
                    if (hyProduceData != null)
                    {
                        DataTable dt = hyProduceData.TenWorkRecord.Tables[0];
                        if (dt.Rows.Count > 0) result = true;
                    }
                    break;
            }
            return result;
        }

        #endregion

    }

    #region[工作站模型]

    #region zz winform
    public class LabelTemplateInfo_Ws
    {
        // Fields
        private List<LabelTemplateInfo> _LabelTemplateInfo;

        public List<LabelTemplateInfo> LabelTemplateInfo
        {
            get { return _LabelTemplateInfo; }
            set { _LabelTemplateInfo = value; }
        }


    }

    public class MastersBarcodeByLinkBarcodeInfo_ws
    {
        // Fields
        private List<MastersBarcodeByLinkBarcodeInfo> _mastersBarcodeByLinkBarcodeInfo;

        public List<MastersBarcodeByLinkBarcodeInfo> MastersBarcodeByLinkBarcodeInfo
        {
            get { return _mastersBarcodeByLinkBarcodeInfo; }
            set { _mastersBarcodeByLinkBarcodeInfo = value; }
        }

    }

    public class BatchBarcode_Ws
    {
        // Fields
        private List<string> _batchBarcodeInfo;

        public List<string> BatchBarcodeInfo
        {
            get { return _batchBarcodeInfo; }
            set { _batchBarcodeInfo = value; }
        }

    }

    #endregion

    /// <summary>
    /// 模板变量类型
    /// </summary>
    public enum LtParamType
    {
        /// <summary>
        /// 图片
        /// </summary>
        Image = 0,
        /// <summary>
        /// 工单字段
        /// </summary>
        Field = 1,
        /// <summary>
        /// 时间
        /// </summary>
        Date = 2
    }

    /// <summary>
    /// 模板明细
    /// </summary>
    public class LabelTemplateRule
    {
        /// <summary>
        /// 变量类型
        /// </summary>
        public LtParamType LtParamType { get; set; }

        /// <summary>
        /// 变量名称
        /// </summary>
        public string LtParamName { get; set; }

        /// <summary>
        /// 变量值
        /// </summary>
        public string LtParamValue { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName { get; set; }
    }

    /// <summary>
    /// 打印信息
    /// </summary>
    public class PrintBarcodeInfo
    {
        public PrintBarcodeInfo()
        {
            LabelTemplateJs = string.Empty;
            Barcode = string.Empty;
            SerialLen = 0;
        }

        public int SerialLen { get; set; }

        public string Barcode { get; set; }

        public string LabelTemplateJs { get; set; }
    }

    /// <summary>
    /// 特殊打印条码信息
    /// </summary>
    public class PrintSpecialBarcodeInfo
    {
        public PrintSpecialBarcodeInfo()
        {
            PackageLen = 0;
            SerialLen = 0;
        }

        public int SerialLen { get; set; }

        public int PackageLen { get; set; }
    }

    /// <summary>
    /// 操作类型
    /// </summary>
    public enum TableType
    {
        MainProgId = 0,
        LinkProgId = 1,
        DeleteBarcode = 2,
        DeleteLinkBarcode = 3,
        WriteBarcode = 4,
        WriteCheckItem = 5,
        WriteLinkBarcode = 6,
        WriteBarcodeBad = 7,
        FindRealBarcode = 8,
        CheckBarcode = 9,
        CheckWPNextExist = 10,
        CheckExistLinkBarcode = 11,
        CheckExistBarcode = 12,
        SelectLinkBarcode = 13,
        CheckBarcodeCK = 14
    }

    /// <summary>
    /// 返回值类型
    /// </summary>
    public enum ExecCodeEnum
    {
        None = 0,
        Success = 1,
        CheckSqlError = 2,
        NotFindWorkOrder = 1000,
        NotDataInPreWP = 1001,
        NotPassInPreWP = 1002,
        Existed = 1003,
        NotFindRelBarcode = 1004,
        NotFindMatBarcode = 1005,
        NotUseMatBarcode = 1006,
        MoreWorkOrder = 1007,
        NotDataChange = 1008
    }

    /// <summary>
    /// 条码数据
    /// </summary>
    
    public class BarcodeData
    {
        /// <summary>
        /// 条码
        /// </summary>
        
        public string Barcode { get; set; }

        /// <summary>
        /// 工序号
        /// </summary>
        
        public int WorkProcessNo { get; set; }

        /// <summary>
        /// 是否通过
        /// </summary>
        
        public bool IsPass { get; set; }

        /// <summary>
        /// 工作站点
        /// </summary>
        
        public string WorkstationId { get; set; }

        /// <summary>
        /// 工作站点名称
        /// </summary>
        public string WorkstationName { get; set; }

        /// <summary>
        /// 工段
        /// </summary>
        
        public string WorkshopSectionId { get; set; }

        /// <summary>
        /// 工段名称
        /// </summary>
        
        public string WorkshopSectionName { get; set; }

        /// <summary>
        /// 工序
        /// </summary>
        
        public string WorkProcessId { get; set; }

        /// <summary>
        /// 工序名称
        /// </summary>
        
        public string WorkProcessName { get; set; }

        /// <summary>
        /// 生产线
        /// </summary>
        
        public string ProduceLineId { get; set; }

        /// <summary>
        /// 生产线名称
        /// </summary>
        
        public string ProduceLineName { get; set; }

        /// <summary>
        /// 派工单号
        /// </summary>
        
        public string BillNo { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        
        public string CreatorId { get; set; }

        /// <summary>
        /// 创建人名称
        /// </summary>
        
        public string CreatorName { get; set; }

        /// <summary>
        /// 条码类型
        /// </summary>
        
        public string BarcodeTypeId { get; set; }

        /// <summary>
        /// 条码类型名称
        /// </summary>
        
        public string BarcodeTypeName { get; set; }

        private List<Badness> _bandnessList;

        /// <summary>
        /// 缺陷集合
        /// </summary>
        
        public List<Badness> BandnessList
        {
            get
            {
                if (_bandnessList == null)
                    _bandnessList = new List<Badness>();
                return _bandnessList;
            }
        }

        private List<LinkBarcode> _linkBarcodeList;

        /// <summary>
        /// 关联条码集合
        /// </summary>
        
        public List<LinkBarcode> LinkBarcodeList
        {
            get
            {
                if (_linkBarcodeList == null)
                    _linkBarcodeList = new List<LinkBarcode>();
                return _linkBarcodeList;
            }
        }
    }

    /// <summary>
    /// 关联条码
    /// </summary>
    public class LinkBarcode
    {
        /// <summary>
        /// 条码
        /// </summary>
        public string Barcode { get; set; }

        /// <summary>
        /// 条码类型
        /// </summary>
        public string BarcodeTypeId { get; set; }

        /// <summary>
        /// 条码类型名称
        /// </summary>
        public string BarcodeTypeName { get; set; }

        public LinkBarcode(string barcode, string barcodeTypeId, string barcodeTypeName)
        {
            this.Barcode = barcode;
            this.BarcodeTypeId = barcodeTypeId;
            this.BarcodeTypeName = barcodeTypeName;
        }
        public LinkBarcode()
        {

        }
    }

    /// <summary>
    /// 不良原因
    /// </summary>
    public class Badness
    {
        /// <summary>
        /// 缺陷
        /// </summary>
        public string BadnessId { get; set; }

        /// <summary>
        /// 缺陷名称
        /// </summary>
        public string BadnessName { get; set; }

        public Badness(string badnessId, string badnessName)
        {
            this.BadnessId = badnessId;
            this.BadnessName = badnessName;
        }

        public Badness()
        {

        }
    }

    /// <summary>
    /// 站点配置信息
    /// </summary>
    
    public class WorkstationConfig
    {
        public WorkstationConfig()
        {
            WorkstationType = WorkstationType.None;
            AllowChangeData = true;
            NeedTakeBadness = false;
            IsCombine = false;
            ScanAny = false;
            IsAccurateCheck = false;
        }


        private List<CheckSolution> _checkSolution;

        /// <summary>
        /// 检测方案
        /// </summary>
        
        public List<CheckSolution> CheckSolution
        {
            get { return _checkSolution ?? (_checkSolution = new List<CheckSolution>()); }
        }

        /// <summary>
        /// 是否精确检测
        /// </summary>
        
        public bool IsAccurateCheck { get; set; }

        /// <summary>
        /// 扫任意码关联
        /// </summary>
        
        public bool ScanAny { get; set; }

        private List<ScanBarcode> _ScanBarcode;

        /// <summary>
        /// 扫码明细
        /// </summary>
        
        public List<ScanBarcode> ScanBarcode
        {
            get
            {
                if (_ScanBarcode == null)
                    _ScanBarcode = new List<ScanBarcode>();
                return _ScanBarcode;
            }
        }

        /// <summary>
        /// 是否多件组合
        /// </summary>
        
        public bool IsCombine { get; set; }

        /// <summary>
        /// 启用缺陷
        /// </summary>
        
        public bool NeedTakeBadness { get; set; }

        /// <summary>
        /// 变更条码
        /// </summary>
        
        public bool AllowChangeData { get; set; }

        /// <summary>
        /// 站点类型
        /// </summary>
        
        public WorkstationType WorkstationType { get; set; }
    }

    /// <summary>
    /// 站点类别枚举
    /// </summary>
    public enum WorkstationType
    {
        /// <summary>
        /// 一般采集
        /// </summary>
        None = 0,
        /// <summary>
        /// 包装
        /// </summary>
        Package = 1,
        /// <summary>
        /// 自动化采集
        /// </summary>
        Auto = 2,
        /// <summary>
        /// 检测
        /// </summary>
        Check = 3
    }

    /// <summary>
    /// 站点扫入的条码
    /// </summary>
    public class ScanBarcode
    {
        private List<BarcodeFixCode> _barcodeFixCode;
        private List<LabelTemplateDetail> _labelTemplateDetail;

        public ScanBarcode()
        {
            CheckMaterial = false;
            IsMaster = false;
            IsFrom = false;
        }

        /// <summary>
        /// 是否有来源
        /// </summary>
        public bool IsFrom { get; set; }

        /// <summary>
        /// 是否传递条码
        /// </summary>
        public bool IsMaster { get; set; }

        public bool CheckMaterial { get; set; }

        /// <summary>
        /// 打印明细
        /// </summary>
        public List<LabelTemplateDetail> LabelTemplateDetail
        {
            get
            {
                if (_labelTemplateDetail == null)
                    _labelTemplateDetail = new List<LabelTemplateDetail>();
                return _labelTemplateDetail;
            }
        }

        /// <summary>
        /// 条码固定编码
        /// </summary>
        public List<BarcodeFixCode> BarcodeFixCode
        {
            get
            {
                if (_barcodeFixCode == null)
                    _barcodeFixCode = new List<BarcodeFixCode>();
                return _barcodeFixCode;
            }
        }
        /// <summary>
        /// 条码长度
        /// </summary>
        public int BarcodeLength { get; set; }

        /// <summary>
        /// 条码类型名称
        /// </summary>
        public string BarcodeTypeName { get; set; }

        /// <summary>
        /// 条码类型
        /// </summary>
        public string BarcodeTypeId { get; set; }
    }

    /// <summary>
    /// 条码固定编码
    /// </summary>
    public class BarcodeFixCode
    {
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 开始位置
        /// </summary>
        public int Start { get; set; }

        public BarcodeFixCode(int start, string value)
        {
            this.Start = start;
            this.Value = value;
        }

        public BarcodeFixCode()
        {

        }
    }

    /// <summary>
    /// 打印明细
    /// </summary>
    
    public class LabelTemplateDetail
    {
        /// <summary>
        /// 条码模板
        /// </summary>
        
        public string LabelTemplateJs { get; set; }

        /// <summary>
        /// 条码类型名称
        /// </summary>
        
        public string BarcodeTypeName { get; set; }

        /// <summary>
        /// 条码类型
        /// </summary>
        
        public string BarcodeTypeId { get; set; }

        /// <summary>
        /// 是否传递条码
        /// </summary>
        
        public bool IsMaster { get; set; }

        /// <summary>
        /// 条码规则
        /// </summary>
        
        public string BarcodeRuleId { get; set; }

        /// <summary>
        /// 是否自动生成
        /// </summary>
        
        public bool AutoBuild { get; set; }

        /// <summary>
        /// 打印数量
        /// </summary>
        
        public int PrintNum { get; set; }

        /// <summary>
        /// 条码模板
        /// </summary>
        
        public string LabelTemplateId { get; set; }

        public LabelTemplateDetail(string labelTemplateId, string barcodeTypeId, string barcodeTypeName, int printNum, string barcodeRuleId, bool autoBuild, bool isMaster)
        {
            LabelTemplateJs = string.Empty;
            this.LabelTemplateId = labelTemplateId;
            this.PrintNum = printNum;
            this.BarcodeRuleId = barcodeRuleId;
            this.AutoBuild = autoBuild;
            this.IsMaster = isMaster;
            this.BarcodeTypeId = barcodeTypeId;
            this.BarcodeTypeName = barcodeTypeName;
        }

        public LabelTemplateDetail()
        {
            LabelTemplateId = string.Empty;
            BarcodeRuleId = string.Empty;
            AutoBuild = false;
            PrintNum = 0;
            IsMaster = false;
            BarcodeTypeId = string.Empty;
            BarcodeTypeName = string.Empty;
            LabelTemplateJs = string.Empty;
        }
    }

    /// <summary>
    /// 站点信息
    /// </summary>
    
    public class WorkstationInfo
    {
        public WorkstationInfo()
        {
            WorkshopSectionId = string.Empty;
            WorkshopSectionName = string.Empty;
            WorkProcessId = string.Empty;
            WorkProcessName = string.Empty;
            WorkProcessNo = 0;
            WorkstationConfigId = string.Empty;
            WorkstationConfigName = string.Empty;
        }

        /// <summary>
        /// 缺陷集合
        /// </summary>
        
        public List<Badness> BadnessSetting { get; set; }

        /// <summary>
        /// 站点配置实体
        /// </summary>
        
        public WorkstationConfig WorkstationConfig { get; set; }

        /// <summary>
        /// 站点配置名称
        /// </summary>
        
        public string WorkstationConfigName { get; set; }

        /// <summary>
        /// 站点配置
        /// </summary>
        
        public string WorkstationConfigId { get; set; }

        /// <summary>
        /// 工序号
        /// </summary>
        
        public int WorkProcessNo { get; set; }

        /// <summary>
        /// 工序名称
        /// </summary>
        
        public string WorkProcessName { get; set; }

        /// <summary>
        /// 工序
        /// </summary>
        
        public string WorkProcessId { get; set; }

        /// <summary>
        /// 工段名称
        /// </summary>
        
        public string WorkshopSectionName { get; set; }

        /// <summary>
        /// 工段
        /// </summary>
        
        public string WorkshopSectionId { get; set; }
    }

    /// <summary>
    /// 工单信息
    /// </summary>
    
    public class WorkOrderInfo
    {
        /// <summary>
        /// 备注
        /// </summary>
        
        public string Remark { get; set; }

        /// <summary>
        /// 包装规格数
        /// </summary>
        
        public decimal PackageNum { get; set; }

        /// <summary>
        /// 多件组合数
        /// </summary>
        
        public decimal CombineNum { get; set; }

        /// <summary>
        /// 母件规格
        /// </summary>
        
        public string MaterialSpec { get; set; }

        /// <summary>
        /// 通知
        /// </summary>
        
        public string Notice { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        
        public string CustomerName { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        
        public string CustomerId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        
        public decimal Quantity { get; set; }

        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        
        public string UnitId { get; set; }

        /// <summary>
        /// 母件名称
        /// </summary>
        
        public string MaterialName { get; set; }

        /// <summary>
        /// 母件
        /// </summary>
        
        public string MaterialId { get; set; }

        /// <summary>
        /// 派工单号
        /// </summary>
        
        public string BillNo { get; set; }

        /// <summary>
        /// 生产线名称
        /// </summary>
        
        public string ProduceLineName { get; set; }

        /// <summary>
        /// 生产线
        /// </summary>
        
        public string ProduceLineId { get; set; }

        public WorkOrderInfo()
        {
            ProduceLineId = string.Empty;
            ProduceLineName = string.Empty;
            BillNo = string.Empty;
            MaterialId = string.Empty;
            MaterialName = string.Empty;
            UnitId = string.Empty;
            UnitName = string.Empty;
            Quantity = decimal.Zero;
            CustomerId = string.Empty;
            CustomerName = string.Empty;
            Notice = string.Empty;
            MaterialSpec = string.Empty;
            CombineNum = decimal.Zero;
            PackageNum = decimal.Zero;
            Remark = string.Empty;
        }
    }

    /// <summary>
    /// 条码检测结果
    /// </summary>
    
    public class BarcodeCheckResult
    {
        public BarcodeCheckResult()
        {
            ExecCode = ExecCodeEnum.None;
            NeedWorkOrder = false;
            RealBarcode = string.Empty;
            AllowChangeData = false;
        }

        /// <summary>
        /// 是否允许更改条码
        /// </summary>
        
        public bool AllowChangeData { get; set; }

        /// <summary>
        /// 传递吗
        /// </summary>
        
        public string RealBarcode { get; set; }

        /// <summary>
        /// 组码
        /// </summary>
        
        public string ClassifyBarcode { get; set; }

        /// <summary>
        /// 是否切换派工单
        /// </summary>
        
        public bool NeedWorkOrder { get; set; }

        /// <summary>
        /// 派工单信息
        /// </summary>
        
        public WorkOrderInfo WorkOrderInfo { get; set; }

        /// <summary>
        /// 站点信息
        /// </summary>
        
        public WorkstationInfo WorkstationInfo { get; set; }

        /// <summary>
        /// 条码检测返回值
        /// </summary>
        
        public ExecCodeEnum ExecCode { get; set; }
    }

    /// <summary>
    /// 登录信息
    /// </summary>
    
    public class LoginInfo
    {
        /// <summary>
        /// 人员名称
        /// </summary>
        
        public string PersonName { get; set; }

        /// <summary>
        /// 人员
        /// </summary>
        
        public string PersonId { get; set; }

        /// <summary>
        /// 站点
        /// </summary>
        
        public string WorkstationId { get; set; }

        /// <summary>
        /// 站点名称
        /// </summary>
        
        public string WorkstationName { get; set; }
    }

    /// <summary>
    /// 派工单集合
    /// </summary>
    
    public class WorkOrderInfo_Ws
    {
        /// <summary>
        /// 派工单集合
        /// </summary>
        
        public List<WorkOrderInfo> WorkOrderInfo { get; set; }
    }

    /// <summary>
    /// 检测方案
    /// </summary>
    
    public class CheckSolution
    {
        public CheckSolution()
        {
            CheckSName = string.Empty;
            CheckSID = string.Empty;
        }

        /// <summary>
        /// 检测方案名称
        /// </summary>
        
        public string CheckSName { get; set; }

        /// <summary>
        /// 检测方案
        /// </summary>
        
        public string CheckSID { get; set; }

        private List<CheckItem> checkItem;

        /// <summary>
        /// 检测项集合
        /// </summary>
        
        public List<CheckItem> CheckItem
        {
            get { return checkItem ?? (checkItem = new List<CheckItem>()); }
        }
    }

    /// <summary>
    /// 检测项
    /// </summary>
    public class CheckItem
    {
        public CheckItem()
        {
            CheckItemId = string.Empty;
            CheckItemName = string.Empty;
            CheckItemType = 0;
            IsFill = false;
            CheckValue = string.Empty;
            IsPass = true;
        }

        /// <summary>
        /// 是否通过
        /// </summary>
        public bool IsPass { get; set; }

        /// <summary>
        /// 检测结果值
        /// </summary>
        public string CheckValue { get; set; }

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsFill { get; set; }

        /// <summary>
        /// 检测项类型
        /// </summary>
        public int CheckItemType { get; set; }

        /// <summary>
        /// 检测项名称
        /// </summary>
        public string CheckItemName { get; set; }

        /// <summary>
        /// 检测项
        /// </summary>
        public string CheckItemId { get; set; }

        private List<CheckItemBadness> checkItemBadness;

        /// <summary>
        /// 检测项缺陷集合
        /// </summary>
        public List<CheckItemBadness> CheckItemBadness
        {
            get { return checkItemBadness ?? (checkItemBadness = new List<CheckItemBadness>()); }
        }
    }

    /// <summary>
    /// 检测缺陷
    /// </summary>
    public class CheckItemBadness
    {
        public CheckItemBadness()
        {
            UpLimit = decimal.Zero;
            LowLimit = decimal.Zero;
            Standard = decimal.Zero;
            BadnessId = string.Empty;
            BadnessName = string.Empty;
        }

        /// <summary>
        /// 缺陷名称
        /// </summary>
        public string BadnessName { get; set; }

        /// <summary>
        /// 缺陷
        /// </summary>
        public string BadnessId { get; set; }

        /// <summary>
        /// 标准值
        /// </summary>
        public decimal Standard { get; set; }

        /// <summary>
        /// 下限
        /// </summary>
        public decimal LowLimit { get; set; }

        /// <summary>
        /// 上限
        /// </summary>
        public decimal UpLimit { get; set; }
    }

    /// <summary>
    /// 关联码关联主码类
    /// </summary>
    public class MastersBarcodeByLinkBarcodeInfo
    {
        /// <summary>
        /// 关联条码
        /// </summary>
        public string LinkBarcode { set; get; }

        /// <summary>
        /// 主码
        /// </summary>
        public string Masterbarcode { set; get; }

        /// <summary>
        /// 主码类型
        /// </summary>
        public string MasterBarcodeType { set; get; }

        /// <summary>
        /// 关联码类型
        /// </summary>
        public string LinkBarcodeType { set; get; }
    }

    /// <summary>
    /// 模板信息
    /// </summary>
    public class LabelTemplateInfo
    {
        /// <summary>
        /// 模板
        /// </summary>
        public string LabelTemplateId { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        public string LabelTemplateName { get; set; }
    }

    /// <summary>
    /// 条码规则信息
    /// </summary>
    public class BarcodeRuleInfo
    {
        /// <summary>
        /// 是否传递吗
        /// </summary>
        public bool IsMaster { get; set; }

        /// <summary>
        /// 条码规则
        /// </summary>
        public string BarcodeRuleId { get; set; }

        /// <summary>
        /// 条码规则名称
        /// </summary>
        public string BarcodeRuleName { get; set; }
    }

    /// <summary>
    /// 批次打印信息
    /// </summary>
    public class BatchPrintInfo
    {
        private List<LabelTemplateInfo> _LabelTemplateData;
        private List<BarcodeRuleInfo> _BarcodeTypeData;

        /// <summary>
        /// 批次打印模板信息集合
        /// </summary>
        public List<LabelTemplateInfo> LabelTemplateData
        {
            get { return _LabelTemplateData ?? (_LabelTemplateData = new List<LabelTemplateInfo>()); }
            set { _LabelTemplateData = value; }
        }

        /// <summary>
        /// 批次打印条码规则集合
        /// </summary>
        public List<BarcodeRuleInfo> BarcodeRuleData
        {
            get { return _BarcodeTypeData ?? (_BarcodeTypeData = new List<BarcodeRuleInfo>()); }
            set { _BarcodeTypeData = value; }
        }
    }
    #endregion

    #region[仓库模型]
    public class STKTurnOrderInfo
    {
        private string _BillNo = string.Empty;
        private string _OutWareNo = string.Empty;
        private string _OutSaleBillNo = string.Empty;
        private string _InSaleBillNo = string.Empty;
        private string _CustomerID = string.Empty;
        private string _CustomerName = string.Empty;
        private string _CellSpec = string.Empty;
        private decimal _PackageNum = decimal.Zero;
        private decimal _CrossWeight = decimal.Zero;
        private decimal _NetWeight = decimal.Zero;
        private string _Color = string.Empty;
        private string _BarcodeRuleID = string.Empty;
        private string _BarcodeRuleName = string.Empty;
        private string _LabelTemplateID = string.Empty;
        private string _LabelTemplateName = string.Empty;
        public string BillNo
        {
            get { return _BillNo; }
            set { _BillNo = value; }
        }
        public string OutWareNo
        {
            get { return _OutWareNo; }
            set { _OutWareNo = value; }
        }
        public string OutSaleBillNo
        {
            get { return _OutSaleBillNo; }
            set { _OutSaleBillNo = value; }
        }
        public string InSaleBillNo
        {
            get { return _InSaleBillNo; }
            set { _InSaleBillNo = value; }
        }
        public string CustomerID
        {
            get { return _CustomerID; }
            set { _CustomerID = value; }
        }
        public string CustomerName
        {
            get { return _CustomerName; }
            set { _CustomerName = value; }
        }
        public string CellSpec
        {
            get { return _CellSpec; }
            set { _CellSpec = value; }
        }
        public decimal PackageNum
        {
            get { return _PackageNum; }
            set { _PackageNum = value; }
        }
        public decimal CrossWeight
        {
            get { return _CrossWeight; }
            set { _CrossWeight = value; }
        }
        public decimal NetWeight
        {
            get { return _NetWeight; }
            set { _NetWeight = value; }
        }
        public string Color
        {
            get { return _Color; }
            set { _Color = value; }
        }
        public string BarcodeRuleID
        {
            get { return _BarcodeRuleID; }
            set { _BarcodeRuleID = value; }
        }
        public string BarcodeRuleName
        {
            get { return _BarcodeRuleName; }
            set { _BarcodeRuleName = value; }
        }
        public string LabelTemplateID
        {
            get { return _LabelTemplateID; }
            set { _LabelTemplateID = value; }
        }
        public string LabelTemplateName
        {
            get { return _LabelTemplateName; }
            set { _LabelTemplateName = value; }
        }
    }
    public class STKTurnOrderInfoList
    {
        private List<STKTurnOrderInfo> _stkTurnOrderList = null;
        public List<STKTurnOrderInfo> StkTurnOrderList
        {
            get
            {
                if (_stkTurnOrderList == null)
                    _stkTurnOrderList = new List<STKTurnOrderInfo>();
                return _stkTurnOrderList;

            }
            set { _stkTurnOrderList = value; }
        }
    }
    public class PrintStorageBarcodeInfo
    {
        private string _barcode;
        private string _barCodeTypeId;
        private string _barCodeTypeName;
        private string _PackageBarcode;
        private bool _isPackage;
        private int _SerialLen;
        private string _labelTemplateID;
        private string _Color;
        private string _cellSpec;
        private string _crossWeight;
        private string _NetWeight;
        private string _packageNum;
        public string Barcode
        {
            get { return _barcode; }
            set { _barcode = value; }
        }
        public string BarCodeTypeId
        {
            get { return _barCodeTypeId; }
            set { _barCodeTypeId = value; }
        }

        public string BarCodeTypeName
        {
            get { return _barCodeTypeName; }
            set { _barCodeTypeName = value; }
        }
        public bool IsPackage
        {
            get { return _isPackage; }
            set { _isPackage = value; }
        }
        public string PackageBarcode
        {
            get { return _PackageBarcode; }
            set { _PackageBarcode = value; }
        }
        public string LabelTemplateID
        {
            get { return _labelTemplateID; }
            set { _labelTemplateID = value; }
        }
        public int SerialLen
        {
            get { return _SerialLen; }
            set { _SerialLen = value; }
        }
        public string Color
        {
            get { return _Color; }
            set { _Color = value; }
        }
        public string CellSpec
        {
            get { return _cellSpec; }
            set { _cellSpec = value; }
        }
        public string CrossWeight
        {
            get { return _crossWeight; }
            set { _crossWeight = value; }
        }
        public string NetWeight
        {
            get { return _NetWeight; }
            set { _NetWeight = value; }
        }
        public string PackageNum
        {
            get { return _packageNum; }
            set { _packageNum = value; }
        }
    }

    public class STKMainInfo
    {
        private string _barCodeTypeId;
        private string _barCodeTypeName;
        private string _PackageBarcode;
        private bool _isPackage;
        private int _SerialLen;
        private string _labelTemplateID;
        private string _Color;
        private string _cellSpec;
        private string _crossWeight;
        private string _NetWeight;
        private string _packageNum;
        private string _barcode;
        public string Barcode
        {
            set { _barcode = value; }
            get { return _barcode; }
        }
        public string BarCodeTypeId
        {
            get { return _barCodeTypeId; }
            set { _barCodeTypeId = value; }
        }
        public string BarCodeTypeName
        {
            get { return _barCodeTypeName; }
            set { _barCodeTypeName = value; }
        }
        public bool IsPackage
        {
            get { return _isPackage; }
            set { _isPackage = value; }
        }
        public string PackageBarcode
        {
            get { return _PackageBarcode; }
            set { _PackageBarcode = value; }
        }
        public int SerialLen
        {
            get { return _SerialLen; }
            set { _SerialLen = value; }
        }
        public string LabelTemplateID
        {
            get { return _labelTemplateID; }
            set { _labelTemplateID = value; }
        }
        public string Color
        {
            get { return _Color; }
            set { _Color = value; }
        }
        public string CellSpec
        {
            get { return _cellSpec; }
            set { _cellSpec = value; }
        }
        public string CrossWeight
        {
            get { return _crossWeight; }
            set { _crossWeight = value; }
        }
        public string NetWeight
        {
            get { return _NetWeight; }
            set { _NetWeight = value; }
        }
        public string PackageNum
        {
            get { return _packageNum; }
            set { _packageNum = value; }
        }
    }


    public class STKLoginInfo
    {
        private string _UserId;
        private string _PersonId;
        private string _PersonName;
        private string _UserPassWord;

        public string UserId
        {
            get { return _UserId; }
            set { _UserId = value; }
        }
        public string PersonId
        {
            get { return _PersonId; }
            set { _PersonId = value; }
        }
        public string PersonName
        {
            get { return _PersonName; }
            set { _PersonName = value; }
        }
        public string UserPassWord
        {
            get { return _UserPassWord; }
            set { _UserPassWord = value; }
        }
    }

    public class STKInWareInfo
    {
        private int _Row_Id;
        private string _WorkProcessNo;
        private string _WorkProcessId;
        private string _WorkProcessName;
        private string _WorkShopSectionId;
        private string _WorkShopSectionName;
        private string _SalBillNo;
        private string _WorkOrderNo;
        private string _BillNo;
        private string _CustomerId;
        private string _CustomerName;
        private string _StorageId;
        private string _StorageName;
        private string _LibraryId;
        private string _LibraryName;
        private string _MaterialId;
        private string _MaterialName;
        public int Row_Id
        {
            get { return _Row_Id; }
            set { _Row_Id = value; }
        }

        public string WorkProcessNo
        {
            get { return _WorkProcessNo; }
            set { _WorkProcessNo = value; }
        }
        public string WorkProcessId
        {
            get { return _WorkProcessId; }
            set { _WorkProcessId = value; }
        }
        public string WorkProcessName
        {
            get { return _WorkProcessName; }
            set { _WorkProcessName = value; }
        }
        public string WorkShopSectionId
        {
            get { return _WorkShopSectionId; }
            set { _WorkShopSectionId = value; }
        }
        public string WorkShopSectionName
        {
            get { return _WorkShopSectionName; }
            set { _WorkShopSectionName = value; }
        }
        public string SalBillNo
        {
            get { return _SalBillNo; }
            set { _SalBillNo = value; }
        }

        public string WorkOrderNo
        {
            get { return _WorkOrderNo; }
            set { _WorkOrderNo = value; }
        }
        public string BillNo
        {
            get { return _BillNo; }
            set { _BillNo = value; }
        }
        public string CustomerId
        {
            get { return _CustomerId; }
            set { _CustomerId = value; }
        }
        public string CustomerName
        {
            get { return _CustomerName; }
            set { _CustomerName = value; }
        }
        public string StorageId
        {
            get { return _StorageId; }
            set { _StorageId = value; }
        }
        public string StorageName
        {
            get { return _StorageName; }
            set { _StorageName = value; }
        }
        public string LibraryId
        {
            get { return _LibraryId; }
            set { _LibraryId = value; }
        }
        public string LibraryName
        {
            get { return _LibraryName; }
            set { _LibraryName = value; }
        }
        public string MaterialId
        {
            get { return _MaterialId; }
            set { _MaterialId = value; }
        }
        public string MaterialName
        {
            get { return _MaterialName; }
            set { _MaterialName = value; }
        }


    }

    public class STKInWareMain
    {
        private string _billNo;

        private string _billType;
        private string _billDate;

        public string BillNo
        {
            get { return _billNo; }
            set { _billNo = value; }
        }
        public string BillType
        {
            get { return _billType; }
            set { _billType = value; }
        }
        public string BillDate
        {
            get { return _billDate; }
            set { _billDate = value; }
        }
    }

    public class STKOutWareMain
    {
        private string _billNo;
        private string _billType;
        private string _billDate;
        private int _planOutNum;
        private int _hasOutNum;

        public string BillNo
        {
            get { return _billNo; }
            set { _billNo = value; }
        }
        public string BillType
        {
            get { return _billType; }
            set { _billType = value; }
        }
        public string BillDate
        {
            get { return _billDate; }
            set { _billDate = value; }
        }
        public int PlanOutNum
        {
            get { return _planOutNum; }
            set { _planOutNum = value; }
        }
        public int HasOutNum
        {
            get { return _hasOutNum; }
            set { _hasOutNum = value; }
        }
    }

    public class STKOutWareInfo
    {
        private int _Row_Id;
        private string _BillNo;
        private string _SalBillNo;
        private string _CustomerId;
        private string _CustomerName;
        private string _MaterialId;
        private string _MaterialName;
        private string _OutSalBillNo;
        private string _SplitSalBillNo;
        private int _SplitBox;
        private int _TurnOrder;
        private string _StorageId;
        private string _StorageName;
        private string _LibraryId;
        private string _LibraryName;

        public string LibraryName
        {
            get { return _LibraryName; }
            set { _LibraryName = value; }
        }

        public string LibraryId
        {
            get { return _LibraryId; }
            set { _LibraryId = value; }
        }
        public string StorageName
        {
            get { return _StorageName; }
            set { _StorageName = value; }
        }
        public string StorageId
        {
            get { return _StorageId; }
            set { _StorageId = value; }
        }
        public int Row_Id
        {
            get { return _Row_Id; }
            set { _Row_Id = value; }
        }
        public string SalBillNo
        {
            get { return _SalBillNo; }
            set { _SalBillNo = value; }
        }
        public string BillNo
        {
            get { return _BillNo; }
            set { _BillNo = value; }
        }
        public string CustomerId
        {
            get { return _CustomerId; }
            set { _CustomerId = value; }
        }
        public string CustomerName
        {
            get { return _CustomerName; }
            set { _CustomerName = value; }
        }
        public string MaterialId
        {
            get { return _MaterialId; }
            set { _MaterialId = value; }
        }
        public string MaterialName
        {
            get { return _MaterialName; }
            set { _MaterialName = value; }
        }
        public string OutSalBillNo
        {
            get { return _OutSalBillNo; }
            set { _OutSalBillNo = value; }
        }
        public string SplitSalBillNo
        {
            get { return _SplitSalBillNo; }
            set { _SplitSalBillNo = value; }
        }
        public int SplitBox
        {
            get { return _SplitBox; }
            set { _SplitBox = value; }
        }
        public int TurnOrder
        {
            get { return _TurnOrder; }
            set { _TurnOrder = value; }
        }
    }

    public class STKSplitBox
    {
        private string _SalBillNo;
        private string _BillNo;
        private string _OutWareNo;
        public string SalBillNo
        {
            get { return _SalBillNo; }
            set { _SalBillNo = value; }
        }
        public string BillNo
        {
            get { return _BillNo; }
            set { _BillNo = value; }
        }
        public string OutWareNo
        {
            get { return _OutWareNo; }
            set { _OutWareNo = value; }
        }
    }

    public class STKInWareInfoList
    {
        private List<STKInWareInfo> _stkInWareInfoList = null;

        private List<STKInWareMain> _stkInWareMainList = null;

        public List<STKInWareInfo> stkInWareInfoList
        {
            get
            {
                if (_stkInWareInfoList == null)
                {
                    _stkInWareInfoList = new List<STKInWareInfo>();
                }
                return _stkInWareInfoList;
            }
            set { _stkInWareInfoList = value; }

        }

        public List<STKInWareMain> stkInWareMainList
        {
            get
            {
                if (_stkInWareMainList == null)
                {
                    _stkInWareMainList = new List<STKInWareMain>();
                }
                return _stkInWareMainList;
            }
            set { _stkInWareMainList = value; }
        }
    }

    public class STKOutWareInfoList
    {
        private List<STKOutWareInfo> _stkOutWareInfoList = null;

        public List<STKOutWareInfo> stkOutWareInfoList
        {
            get
            {
                if (_stkOutWareInfoList == null)
                {
                    _stkOutWareInfoList = new List<STKOutWareInfo>();
                }
                return _stkOutWareInfoList;
            }
            set { _stkOutWareInfoList = value; }

        }

        private List<STKOutWareMain> _stkOutWareMainList = null;
        public List<STKOutWareMain> stkOutWareMainList
        {
            get
            {
                if (_stkOutWareMainList == null)
                {
                    _stkOutWareMainList = new List<STKOutWareMain>();
                }
                return _stkOutWareMainList;
            }
            set { _stkOutWareMainList = value; }

        }

    }

    public class STKSplitBoxList
    {
        private List<STKSplitBox> _stkSplitBoxList = null;

        public List<STKSplitBox> stkSplitBoxList
        {
            get
            {
                if (_stkSplitBoxList == null)
                {
                    _stkSplitBoxList = new List<STKSplitBox>();
                }
                return _stkSplitBoxList;
            }
            set { _stkSplitBoxList = value; }

        }
    }

    public enum STKBarCodeCheckResult
    {

        //条码格式错误
        ErrorType = 0,
        //已存在
        Existed = 1,
        //符合非包装码
        PassNotPack = 2,
        //符合是包装码
        PassIsPack = 3,


        //已出库
        HasOutWare = 4,
        //不是该销售订单
        NotRightSalBillNo = 5,
        //未入库
        NotInWare = 6,
        //允许出库
        CanOutWare = 7,


        //转单中 未找到该条码
        NotFindBarcode = 8,

        //可以转单
        IsPass = 9,
        //不允许拆箱
        IsNotSplitBox = 10,
        //不允许转单
        IsNotTurnOrder = 11,

        IsNotWorkOrderBillNO = 12


    }


    public enum STKBillType
    {
        InWare = 1,

        SplitBox = 2,

        OutWare = 3,

        TurnOrder = 4,

        Null = 5

    }
    public class STKCheckResult
    {
        private STKBarCodeCheckResult _stkCheck = STKBarCodeCheckResult.ErrorType;

        public STKBarCodeCheckResult stkCheck
        {
            get { return _stkCheck; }
            set { _stkCheck = value; }
        }

        private STKBillType _billType = STKBillType.Null;

        public STKBillType billType
        {
            get { return _billType; }
            set { _billType = value; }
        }

        private STKMainInfo _stkMainInfo = null;

        public STKMainInfo stkMainInfo
        {
            get
            {
                if (_stkMainInfo == null)
                {
                    _stkMainInfo = new STKMainInfo();
                }
                return _stkMainInfo;
            }
            set { _stkMainInfo = value; }
        }
    }

    public class TurnOrderPost
    {
        private int _IsPackage = 0;
        private string _CustomerID = string.Empty;
        private string _BarcodeRuleID = string.Empty;
        private string _InSaleBillNo = string.Empty;
        private string _PackageBarcode = string.Empty;
        public int IsPackage
        {
            set
            {
                _IsPackage = value;
            }
            get
            {
                return _IsPackage;
            }
        }
        public string CustomerID
        {
            set
            {
                _CustomerID = value;
            }
            get
            {
                return _CustomerID;
            }
        }
        public string BarcodeRuleID
        {
            set
            {
                _BarcodeRuleID = value;
            }
            get
            {
                return _BarcodeRuleID;
            }
        }
        public string InSaleBillNo
        {
            set
            {
                _InSaleBillNo = value;
            }
            get
            {
                return _InSaleBillNo;
            }
        }
        public string PackageBarcode
        {
            set
            {
                _PackageBarcode = value;
            }
            get
            {
                return _PackageBarcode;
            }
        }

    }
    #endregion

    #region[维修模型]
    /// <summary>
    /// 维修
    /// </summary>
    
    public class mainBarCode
    {
        /// <summary>
        /// 条码
        /// </summary>
        
        public string barcode { get; set; }

        /// <summary>
        /// 是否通过
        /// </summary>
        
        public bool isPass { get; set; }

        /// <summary>
        /// 条码类型
        /// </summary>
        
        public string barcodeTypeId { get; set; }

        /// <summary>
        /// 条码类型名称
        /// </summary>
        
        public string barcodeTypeName { get; set; }

        /// <summary>
        /// 是否能提交
        /// </summary>
        
        public bool canSubmit { get; set; }

        /// <summary>
        /// 是否维修完成
        /// </summary>
        
        public bool isOK { get; set; }

        /// <summary>
        /// 工序号
        /// </summary>
        
        public int workProcessNo { get; set; }

        /// <summary>
        /// 派工单对象
        /// </summary>
        
        public Maintenance processInformation
        {
            get;
            set;
        }

        /// <summary>
        /// 缺陷
        /// </summary>
        
        public List<Badness> badnessList
        {
            get;
            set;
        }

        /// <summary>
        /// 更换条码
        /// </summary>
        
        public List<ActualChangeBarCode> changeBarCode
        {
            get;
            set;
        }

    }

    /// <summary>
    /// 更换条码
    /// </summary>
    public class ActualChangeBarCode
    {
        /// <summary>
        /// 旧条码
        /// </summary>
        public string oldBarCode { get; set; }

        /// <summary>
        /// 条码类型
        /// </summary>
        public string barcodeTypeId { get; set; }

        /// <summary>
        /// 新条码
        /// </summary>
        public string newBarCode { get; set; }
    }

    /// <summary>
    /// 派工单信息集合
    /// </summary>
    
    public class Maintenance
    {
        public Maintenance()
        {
            IsPass = false;
            Createtime = 0;
        }

        private List<Badness> _badnessList;

        /// <summary>
        /// 不良原因
        /// </summary>
        
        public List<Badness> BadnessList
        {
            get
            {
                if (_badnessList == null)
                {
                    _badnessList = new List<Badness>();
                }
                return _badnessList;
            }
            set { _badnessList = value; }
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        
        public long Createtime { get; set; }

        /// <summary>
        /// 条码
        /// </summary>
        
        public string BarCode { get; set; }

        /// <summary>
        /// 派工单号
        /// </summary>
        
        public string BillNo { get; set; }

        /// <summary>
        /// 是否通过
        /// </summary>
        
        public bool IsPass { get; set; }

        /// <summary>
        /// 工序名称
        /// </summary>
        
        public string WorkprocessName { get; set; }

        /// <summary>
        /// 工序号
        /// </summary>
        
        public int WorkprocessNo { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        
        public string CustomerName { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        
        public decimal Quantity { get; set; }

        /// <summary>
        /// 单位名称
        /// </summary>
        
        public string UnitName { get; set; }

        /// <summary>
        /// 母件规格
        /// </summary>
        
        public string MaterialSpec { get; set; }

        /// <summary>
        /// 母件名称
        /// </summary>
        
        public string MaterialName { get; set; }

        /// <summary>
        /// 母件
        /// </summary>
        
        public string MaterialId { get; set; }

        private List<string> _badness;

        /// <summary>
        /// 缺陷
        /// </summary>
        
        public List<string> comDect
        {
            get
            {
                if (_badness == null)
                {
                    _badness = new List<string>();
                }
                return _badness;
            }
            set { _badness = value; }
        }

        /// <summary>
        /// 条码类型名称
        /// </summary>
        
        public string BarCodeTypeName { get; set; }

        /// <summary>
        /// 条码类型
        /// </summary>
        
        public string BarCodeTypeId { get; set; }
    }

    /// <summary>
    /// 条码的完成情况
    /// </summary>
    
    public class WorkOrderDetail
    {
        public WorkOrderDetail()
        {
            IsCheck = false;
        }

        /// <summary>
        /// 是否完成
        /// </summary>
        
        public bool IsCheck { get; set; }

        /// <summary>
        /// 工序名称
        /// </summary>
        
        public string WorkprocessName { get; set; }

        /// <summary>
        /// 工序号
        /// </summary>
        
        public int WorkprocessNo { get; set; }

        /// <summary>
        /// 派工单号
        /// </summary>
        
        public string BillNo { get; set; }
    }

    /// <summary>
    /// 条码生命周期记录
    /// </summary>
    
    public class LifeCycle
    {
        /// <summary>
        /// 条码
        /// </summary>
        
        public string BarCode { get; set; }

        /// <summary>
        /// 派工单号
        /// </summary>
        
        public string BillNo { get; set; }

        /// <summary>
        /// 是否通过
        /// </summary>
        
        public int Pass { get; set; }

        /// <summary>
        /// 生产线
        /// </summary>
        
        public string ProduceLineId { get; set; }

        /// <summary>
        /// 生产线名称
        /// </summary>
        
        public string ProduceLineName { get; set; }

        /// <summary>
        /// 工序号
        /// </summary>
        
        public int WorkProcessNo { get; set; }

        /// <summary>
        /// 工序
        /// </summary>
        
        public string WorkProcessId { get; set; }

        /// <summary>
        /// 工序名称
        /// </summary>
        
        public string WorkProcessName { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        
        public string CreatorId { get; set; }

        /// <summary>
        /// 操作人名称
        /// </summary>
        
        public string CreatorName { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        
        public long CreateTime { get; set; }

        /// <summary>
        /// 条码类型
        /// </summary>
        
        public string BarCodeTypeId { get; set; }

        /// <summary>
        /// 条码类型名称
        /// </summary>
        
        public string BarCodeTypeName { get; set; }
    }

    /// <summary>
    /// 条码类别
    /// </summary>
    public enum LifeCycleType
    {
        /// <summary>
        /// 主码但不是唛头
        /// </summary>
        MainBarCode = 0,
        /// <summary>
        /// 关联码
        /// </summary>
        LinkBarCode = 1,
        /// <summary>
        /// 不存在该条码
        /// </summary>
        NotExisted = 2,
        /// <summary>
        /// 唛头
        /// </summary>
        MarkBarCode = 3

    }
    #endregion

    #region[检测模型]
    /// <summary>
    /// 质检单
    /// </summary>
    public class CheckOrder
    {
        public CheckOrder()
        {
            CheckOrderType = 0;
            WorkStationConfigId = string.Empty;
            PurchaseOrder = string.Empty;
            IsConfig = false;
            IsUserCheck = false;
            UnQualifiedNum = decimal.Zero;
            QualifiedNum = decimal.Zero;
            CheckNum = decimal.Zero;
            Quantity = decimal.Zero;
            CheckType = string.Empty;
            AttributeDesc = string.Empty;
            MaterialName = string.Empty;
            MaterialId = string.Empty;
            SupplierName = string.Empty;
            RowId = 0;
            BillNo = string.Empty;
        }

        /// <summary>
        /// 质检单号
        /// </summary>
       
        public string BillNo { get; set; }

        /// <summary>
        /// 行标识
        /// </summary>
       
        public int RowId { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
       
        public string SupplierName { get; set; }

        /// <summary>
        /// 物料
        /// </summary>
       
        public string MaterialId { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
       
        public string MaterialName { get; set; }

        /// <summary>
        /// 特征描述
        /// </summary>
       
        public string AttributeDesc { get; set; }

        /// <summary>
        /// 检测方式
        /// </summary>
       
        public string CheckType { get; set; }

        /// <summary>
        /// 物料数量
        /// </summary>
       
        public decimal Quantity { get; set; }

        /// <summary>
        /// 检测数量
        /// </summary>
       
        public decimal CheckNum { get; set; }

        /// <summary>
        /// 合格数量
        /// </summary>
       
        public decimal QualifiedNum { get; set; }

        /// <summary>
        /// 不合格数量
        /// </summary>
       
        public decimal UnQualifiedNum { get; set; }

        /// <summary>
        /// 是否启用质检流程
        /// </summary>
       
        public bool IsUserCheck { get; set; }

        /// <summary>
        /// 是否启用站点检测
        /// </summary>
       
        public bool IsConfig { get; set; }

        /// <summary>
        /// 采购订单号
        /// </summary>
       
        public string PurchaseOrder { get; set; }

        /// <summary>
        /// 站点配置
        /// </summary>
       
        public string WorkStationConfigId { get; set; }

        /// <summary>
        /// 质检单类型
        /// </summary>
       
        public int CheckOrderType { get; set; }
    }

    /// <summary>
    /// 数据字典
    /// </summary>
    public class ComData
    {
        public ComData()
        {
            Name = string.Empty;
            Id = string.Empty;
        }

        /// <summary>
        /// ID
        /// </summary>
       
        public string Id { get; set; }

        /// <summary>
        /// NAME
        /// </summary>
       
        public string Name { get; set; }
    }

    #endregion

    #region [二开模型]

    public class CheckOrderInfo
    {
        private string _billNo = string.Empty;

        public string BillNo
        {
            get { return _billNo; }
            set { _billNo = value; }
        }

        private int _rowId = 0;

        public int RowId
        {
            get { return _rowId; }
            set { _rowId = value; }
        }
        private string _materialId = string.Empty;

        public string MaterialId
        {
            get { return _materialId; }
            set { _materialId = value; }
        }

        private string _materialName = string.Empty;

        public string MaterialName
        {
            get { return _materialName; }
            set { _materialName = value; }
        }
        private string _AttributeDesc = string.Empty;

        public string AttributeDesc
        {
            get { return _AttributeDesc; }
            set { _AttributeDesc = value; }
        }
        private string _checkType = string.Empty;

        public string CheckType
        {
            get { return _checkType; }
            set { _checkType = value; }
        }
        private decimal _quantity = decimal.Zero;

        public decimal Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }
        private decimal _checkNum = decimal.Zero;

        public decimal CheckNum
        {
            get { return _checkNum; }
            set { _checkNum = value; }
        }

        private bool _isUserCheck = false;

        public bool IsUserCheck
        {
            get { return _isUserCheck; }
            set { _isUserCheck = value; }
        }

        private bool _isConfig = false;

        public bool IsConfig
        {
            get { return _isConfig; }
            set { _isConfig = value; }
        }

        private string _workStationConfigId = string.Empty;

        public string WorkStationConfigId
        {
            get { return _workStationConfigId; }
            set { _workStationConfigId = value; }
        }

        private int _checkOrderType = 0;

        public int CheckOrderType
        {
            get { return _checkOrderType; }
            set { _checkOrderType = value; }
        }

        //供应商编码
        private string _SuppierId = string.Empty;

        public string SuppierId
        {
            get { return _SuppierId; }
            set { _SuppierId = value; }
        }

        //供应商名称
        private string _SupplierName = string.Empty;

        public string SupplierName
        {
            get { return _SupplierName; }
            set { _SupplierName = value; }
        }

        //到货日期 RECEIVEDATE
        private string _Receivedate = string.Empty;

        public string Receivedate
        {
            get { return _Receivedate; }
            set { _Receivedate = value; }
        }

        //批号
        private string _BatchNo = string.Empty;

        public string BatchNo
        {
            get { return _BatchNo; }
            set { _BatchNo = value; }
        }

        //合格数量
        private decimal _QualifiedNum = 0;

        public decimal QualifiedNum
        {
            get { return _QualifiedNum; }
            set { _QualifiedNum = value; }
        }

        //不合格数量
        private decimal _UnQualifiedNum = 0;

        public decimal UnQualifiedNum
        {
            get { return _UnQualifiedNum; }
            set { _UnQualifiedNum = value; }
        }

        //处理方式
        private int _DealWay = 1;

        public int DealWay
        {
            get { return _DealWay; }
            set { _DealWay = value; }
        }

        //采购单计划单单号
        private string _FromBillNo = string.Empty;

        public string FromBillNo
        {
            get { return _FromBillNo; }
            set { _FromBillNo = value; }
        }

        //采购计划单行标识
        private int _FromRowId = 0;

        public int FromRowId
        {
            get { return _FromRowId; }
            set { _FromRowId = value; }
        }

        //记录总条数
        private int count = 0;

        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        //开始时间
        private long starttime = 0;

        public long Starttime
        {
            get { return starttime; }
            set { starttime = value; }
        }
    }

    #endregion

    #region[HY模型]
    //数据处理模型
    public class ProduceBcfData
    {
        //派工单号
        public string BillNo { get; set; }

        //任务号
        public string TaskNo { get; set; }

        //工序号
        public int WorkProcessNo { get; set; }

        //是否超时
        public bool IsTimeOut { get; set; }

        //超时时长
        public int OutTime { get; set; }

        //人员编号
        public string PersonId { get; set; }

        //人员名称
        public string PersonName { get; set; }

        //站点编号
        public string WorkstationId { get; set; }

        private List<string> _linkBarcode;
        //任务明细
        public List<string> LinkBarcode
        {
            get { return _linkBarcode ?? (_linkBarcode = new List<string>()); }
            set { _linkBarcode = value; }
        }

        private List<Badness> _defectDetail;
        //缺陷
        public List<Badness> DefectDetail
        {
            get { return _defectDetail ?? (_defectDetail = new List<Badness>()); }
            set { _defectDetail = value; }
        }

        private HYWorkStationConfig _config;
        //站点配置
        public HYWorkStationConfig Config
        {
            get { return _config ?? (_config = new HYWorkStationConfig()); }
            set { _config = value; }
        }

        //任务当前处理状态
        public ChangeType ChangeType = ChangeType.Select;

        //批号
        public string LotNo { get; set; }

        //组号
        public string GroupNo { get; set; }

        //锁定状态
        public TenAbnormType TenAbnormal = TenAbnormType.SingleLock;

        public ProduceBcfData()
        {
            GroupNo = string.Empty;
            LotNo = string.Empty;
            WorkstationId = string.Empty;
            PersonName = string.Empty;
            PersonId = string.Empty;
            BillNo = string.Empty;
            TaskNo = string.Empty;
            OutTime = 0;
            IsTimeOut = false;
            WorkProcessNo = 0;
        }
    }
    //任务处理状态
    public enum ChangeType
    {
        //锁定
        Select = 0,
        //暂存
        Stop = 1,
        //完成
        Finish = 2,
        //异常
        Defect = 3,

        Back = 4,

        Single = 5

    }

    public enum TenAbnormType
    {
        //单个锁定
        SingleLock = 0,
        //异常锁定
        AbnormalLock = 1,
        //跳单
        LotNoLock = 2,
        //单个解锁
        SingleUnLock = 3,
        //异常解锁
        AbnormalUnLock = 4,
        //跳单解锁
        LotNoUnLock = 5,
        //十单锁定
        LotNoOutLock = 6,
        //十单解锁
        LotNoOutUnLock = 7
    }

    //站点配置
    public class HYWorkStationConfig
    {
        //是否单个报工
        public bool IsSingle { get; set; }

        //是否调整顺序
        public bool IsChangeOrder { get; set; }

        //打印次数
        public int PrintCount { get; set; }

        //关联码打印
        public bool IsLinkPrint { get; set; }

        //节拍计时
        public int BeatTime { get; set; }

        //是否整体载入
        public bool IsWhole { get; set; }

        //是否自动载入
        public bool IsAuto { get; set; }

        //是否指定站点
        public bool IsPoint { get; set; }

        //是否主码打印
        public bool IsMainPrint { get; set; }

        //是否协同工序
        public bool IsCooperateWorkProcess { get; set; }

        //协同主工序
        public string CooperateMainWorkProcess { get; set; }

        //是否为转入站点
        public bool IsTransferWorkStationId { get; set; }

        public string TransferWorkStationId { get; set; }

        private List<string> _cooperateWorkProcess;

        //协同工序
        public List<string> CooperateWorkProcess
        {
            get { return _cooperateWorkProcess ?? (_cooperateWorkProcess = new List<string>()); }
            set { _cooperateWorkProcess = value; }
        }

        public HYWorkStationConfig()
        {
            TransferWorkStationId = string.Empty;
            IsTransferWorkStationId = false;
            CooperateMainWorkProcess = string.Empty;
            IsCooperateWorkProcess = false;
            IsMainPrint = true;
            IsPoint = false;
            IsAuto = false;
            IsWhole = true;
            BeatTime = 0;
            PrintCount = 1;
            IsLinkPrint = false;
            IsChangeOrder = false;
            IsSingle = false;
        }
    }

    public class AbnormalReport
    {
        public AbnormalType abnormalPrptoType = AbnormalType.Produce;
        //异常报告单号
        public string BillNo { get; set; }

        //类型id
        public string TypeId { get; set; }

        //异常类型id

        public string AbnormalTypeId { get; set; }

        //异常id

        public string AbnormalId { get; set; }

        public string AbnormalName { get; set; }

        //责任部门

        public string DeptId { get; set; }

        //异常描述

        public string AbnormalDesc { get; set; }

        //责任人

        public string PersonId { get; set; }

        public string PersonName { get; set; }

        public string DestPhoneNo { get; set; }

        //报告人

        public string FromPersonId { get; set; }

        //报告人部门

        public string FromDeptId { get; set; }

        //影响生产

        public int AffectProduceState { get; set; }

        //影响人数

        public decimal AffectPersonNum { get; set; }

        //影响工时

        public double AffectTime { get; set; }

        //开始时间

        public long StartTime { get; set; }

        //结束时间

        public long EndTime { get; set; }

        //处理状态

        public int DealWithState { get; set; }

        //来源标识

        public string FromMark { get; set; }

        //系统创建ISSYSTEMBUILD
        public int IsSystemBuilD { get; set; }

        public AbnormalReport()
        {
            FromMark = string.Empty;
            AffectPersonNum = 0;
            FromDeptId = string.Empty;
            FromPersonId = string.Empty;
            DestPhoneNo = string.Empty;
            PersonName = string.Empty;
            PersonId = string.Empty;
            AbnormalDesc = string.Empty;
            DeptId = string.Empty;
            AbnormalName = string.Empty;
            AbnormalId = string.Empty;
            AbnormalTypeId = string.Empty;
            TypeId = string.Empty;
            BillNo = string.Empty;
        }

        
    }

    public enum AbnormalType
    {
        ReceiveTask = 0,
        Check = 1,
        StockIn = 2,
        TakeMat = 3,
        Send = 4,
        ReceiveMaterial = 5,
        Produce = 6

    }

    public class Abnormal
    {
        //异常
        public string AbnormalId { get; set; }

        //异常名称
        public string AbnormalName { get; set; }

        //异常类型
        public string AbnormalTypeId { get; set; }

        //异常类型名称
        public string AbnormalTypeName { get; set; }

        //业务属性
        public int Bizattr { get; set; }

        //所属类型
        public int ChangeType { get; set; }

        //部门编号
        public string DeptId { get; set; }

        //部分名称
        public string DeptName { get; set; }

        //异常单号
        public string BillNo { get; set; }

        //单据时间
        public int BillDate { get; set; }

        //异常开始时间
        public long AbnormalStartTime { get; set; }

        //人员编号
        public string PersonId { get; set; }

        //人员名称
        public string PersonName { get; set; }

        //异常描述
        public string AbnormalDesc { get; set; }

        //来源标识
        public string FromMark { get; set; }

        //任务号
        public string TaskNo { get; set; }

        //影响生产
        public int AffectProduceState { get; set; }

        //批号
        public string LotNo { get; set; }

        //组号
        public string GroupNo { get; set; }

        //订单状态
        public int SaleOrderState { get; set; }

        //派工单号
        public string WorkOrderNo { get; set; }

        public int IsRepairOut { get; set; }

        public Abnormal()
        {
            IsRepairOut = 0;
            WorkOrderNo = string.Empty;
            SaleOrderState = 0;
            GroupNo = string.Empty;
            LotNo = string.Empty;
            AffectProduceState = 0;
            TaskNo = string.Empty;
            FromMark = string.Empty;
            AbnormalDesc = string.Empty;
            PersonName = string.Empty;
            PersonId = string.Empty;
            AbnormalStartTime = 0;
            BillDate = 0;
            BillNo = string.Empty;
            DeptName = string.Empty;
            DeptId = string.Empty;
            ChangeType = 0;
            Bizattr = 0;
            AbnormalTypeName = string.Empty;
            AbnormalTypeId = string.Empty;
            AbnormalName = string.Empty;
            AbnormalId = string.Empty;
        }

        
    }

    public class Paint
    {
        //条码
        public string Barcode { get; set; }

        //创建时间
        public long Createtime { get; set; }

        //派工单号
        public string WorkOrderNo { get; set; }

        //十单日期
        public int OrderDate { get; set; }

        //十单顺序号
        public int OrderNum { get; set; }

        //生产单号
        public string LotNo { get; set; }


        //组号
        public string GroupNo { get; set; }

        //来源
        public int FromType { get; set; }

        //站点编号
        public string WorkstationId { get; set; }

        //开始时间
        public long Starttime { get; set; }

        //结束时间
        public long Endtime { get; set; }

        //创建人ID
        public string CreatorId { get; set; }

        //创建人姓名
        public string CreatorName { get; set; }

        //状态
        public int States { get; set; }

        //报工类型
        public string SubmitType { get; set; }

        //是否单个报工
        public int IsSingleWork { get; set; }

        //工件数量
        public int UnitNum { get; set; }

        //扫入数量
        public int FinishNum { get; set; }

        //异常ID
        public string AbnormalId { get; set; }

        //异常名称
        public string AbnormalName { get; set; }

        public Paint()
        {
            AbnormalName = string.Empty;
            AbnormalId = string.Empty;
            FinishNum = 0;
            UnitNum = 0;
            IsSingleWork = 0;
            SubmitType = string.Empty;
            States = 0;
            CreatorName = string.Empty;
            CreatorId = string.Empty;
            Endtime = 0;
            Starttime = 0;
            WorkstationId = string.Empty;
            FromType = 0;
            GroupNo = string.Empty;
            LotNo = string.Empty;
            OrderNum = 0;
            OrderDate = 0;
            WorkOrderNo = string.Empty;
            Createtime = 0;
            Barcode = string.Empty;
        }

        
    }

    //小车
    public class PaintCar
    {
        public string CarId { get; set; }

        public string CarName { get; set; }

        public int States { get; set; }

        public bool IsBand { get; set; }

        public bool IsRepairCar { get; set; }
    }

    //库位
    public class Storage
    {
        public string StorageId { get; set; }

        public bool IsBand { get; set; }
    }

    //绑定信息
    public class BandingInfo
    {
        //条码
        public string Barcode { get; set; }

        //小车编号
        public string CarId { get; set; }

        //小车名称
        public string CarName { get; set; }

        //库位编号
        public string StorageId { get; set; }

        //库位名称
        public string StorageName { get; set; }

        //创建时间
        public long CreateTime { get; set; }

        //油漆创建时间
        public long PaintCreateTime { get; set; }

        //绑定创建时间
        public long BandCreateTime { get; set; }

        //派工单号
        public string WorkOrderNo { get; set; }

        //十单日期
        public int OrderDate { get; set; }

        //十单顺序号
        public int OrderNum { get; set; }

        //生产单号
        public string LotNo { get; set; }


        //组号
        public string GroupNo { get; set; }

        //来源
        public int FromType { get; set; }

        //站点编号
        public string WorkstationId { get; set; }

        //开始时间
        public long Starttime { get; set; }

        //结束时间
        public long Endtime { get; set; }

        //库位是否绑定
        public int IsBand { get; set; }

        //库位是否绑定
        public int States { get; set; }

        //创建人ID
        public string CreatorId { get; set; }

        //创建人姓名
        public string CreatorName { get; set; }

        //是否单个报工
        public int IsSingleWork { get; set; }

        //工件数量
        public int UnitNum { get; set; }

        //扫入数量
        public int FinishNum { get; set; }

        public BandingInfo()
        {
            FinishNum = 0;
            UnitNum = 0;
            IsSingleWork = 0;
            CreatorName = string.Empty;
            CreatorId = string.Empty;
            States = 0;
            IsBand = 0;
            Endtime = 0;
            Starttime = 0;
            WorkstationId = string.Empty;
            FromType = 0;
            GroupNo = string.Empty;
            LotNo = string.Empty;
            OrderNum = 0;
            OrderDate = 0;
            WorkOrderNo = string.Empty;
            BandCreateTime = 0;
            PaintCreateTime = 0;
            CreateTime = 0;
            StorageName = string.Empty;
            StorageId = string.Empty;
            CarName = string.Empty;
            CarId = string.Empty;
            Barcode = string.Empty;
        }

        
    }

    /// <summary>
    /// 特殊件
    /// </summary>
    public enum SpecialMatType
    {
        /// <summary>
        /// 配品配件
        /// </summary>
        Product = 0,
        /// <summary>
        /// 提前生产件
        /// </summary>
        Advance = 1,
        /// <summary>
        /// 非标件
        /// </summary>
        NonStandardMat = 2
    }

    public class SpecialMat
    {
        public Boolean IsSpecial { get; set; }

        public string WorkShopSectionId { get; set; }

        public string FactoryId { get; set; }

        public SpecialMatType SpecialMatType { get; set; }

        private List<SpecialMatDetail> _specialMatDetail;

        public SpecialMat()
        {
            SpecialMatType = SpecialMatType.Product;
            FactoryId = string.Empty;
            WorkShopSectionId = string.Empty;
            IsSpecial = false;
        }

        public List<SpecialMatDetail> SpecialMatDetail
        {
            get { return _specialMatDetail ?? (_specialMatDetail = new List<SpecialMatDetail>()); }
            set
            {
                _specialMatDetail = value;
            }
        }
    }

    public class SpecialMatDetail
    {
        public SpecialMatDetail()
        {
            FactoryId = string.Empty;
            AttributeItemRowId = 0;
            AttributeItemId = string.Empty;
            AttributeRowId = 0;
            AttributeId = string.Empty;
            AttributeValue = string.Empty;
            AttributeCode = string.Empty;
            ProductId = string.Empty;
            WorkShopSectionId = string.Empty;
            SpecialMatType = SpecialMatType.Product;
            OperValue = 0;
            Operator = 0;
        }

        public Int32 Operator { get; set; }

        public decimal OperValue { get; set; }

        public string FactoryId { get; set; }

        public SpecialMatType SpecialMatType { get; set; }

        public string WorkShopSectionId { get; set; }

        public string ProductId { get; set; }

        public string AttributeCode { get; set; }

        public string AttributeValue { get; set; }

        public string AttributeId { get; set; }

        public Int32 AttributeRowId { get; set; }

        public string AttributeItemId { get; set; }

        public Int32 AttributeItemRowId { get; set; }
    }

    public class OpcConfig
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; }

        public List<OpcConfigDetail> ConfigDetail
        {
            get { return _opcConfigDetail ?? (_opcConfigDetail = new List<OpcConfigDetail>()); }
            set { _opcConfigDetail = value; }
        }

        /// <summary>
        /// 默认值
        /// </summary>
        public int DefaultValue { get; set; }

        /// <summary>
        /// 初始值
        /// </summary>
        public int CurrentValue { get; set; }

        /// <summary>
        /// 配置信息明细
        /// </summary>
        private List<OpcConfigDetail> _opcConfigDetail;

        public OpcConfig()
        {
            CurrentValue = 0;
            DefaultValue = 0;
            ParamName = string.Empty;
        }
    }

    public class OpcConfigDetail
    {
        /// <summary>
        /// 类型
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public int ParamValue { get; set; }

        /// <summary>
        /// 厨师值
        /// </summary>
        public int CurrentValue { get; set; }

        public OpcConfigDetail()
        {
            CurrentValue = 0;
            ParamValue = 0;
            MaterialName = string.Empty;
        }
    }

    #endregion
}
