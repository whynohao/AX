/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：LibBcfData的分部类，避免单个类文件太大
 * 创建标识：Zhangkj 2017/06/06
 * 
 *
************************************************************************/
using AxCRL.Comm.Configs;
using AxCRL.Comm.Entity;
using AxCRL.Comm.Enums;
using AxCRL.Comm.Utils;
using AxCRL.Core.Comm;
using AxCRL.Core.Mail;
using AxCRL.Core.SysNews;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AxCRL.Bcf
{
    //LibBcfData的分部类，避免单个类文件太大
    public partial class LibBcfData
    {
        /// <summary>
        /// 发送操作的移动端推送消息
        /// </summary>
        public void SendApproveAppPush()
        {
            if (MicroServicesConfig.Instance.AppPush.Enabled == false)
                return;
            if (this.ApproveMailParam.To.Count > 0)
            {
                ThreadPool.QueueUserWorkItem(LibAppPushHelper.Push, new object[] { this.BillAction, new List<LibMailParam>() { this.ApproveMailParam } });
            }
            if (this.ApproveRowObj.MailParamList.Count > 0)
            {
                List<LibMailParam> list = new List<LibMailParam>();
                foreach (var item in this.ApproveRowObj.MailParamList)
                {
                    if (item.Value.MailParam.To.Count > 0)
                    {
                        list.Add(item.Value.MailParam);
                    }
                }
                ThreadPool.QueueUserWorkItem(LibAppPushHelper.Push, new object[] { this.BillAction, list });
            }
        }
        /// <summary>
        /// 发送微信消息
        /// </summary>
        public void SendWeixin()
        {
            if (this.ApproveMailParam.To.Count > 0)
            {
                ThreadPool.QueueUserWorkItem(LibSMSHelper.SendWeiXinMsgByMailList, new List<LibMailParam>() { this.ApproveMailParam });
            }
            if (this.ApproveRowObj.MailParamList.Count > 0)
            {
                List<LibMailParam> list = new List<LibMailParam>();
                foreach (var item in this.ApproveRowObj.MailParamList)
                {
                    if (item.Value.MailParam.To.Count > 0)
                    {
                        list.Add(item.Value.MailParam);
                    }
                }
                ThreadPool.QueueUserWorkItem(LibSMSHelper.SendWeiXinMsgByMailList, list);
            }
        }
        /// <summary>
        /// 发送短信消息
        /// </summary>
        public void SendSMS()
        {
            if (this.ApproveMailParam.To.Count > 0)
            {
                ThreadPool.QueueUserWorkItem(LibSMSHelper.SendMsgByMailList, new List<LibMailParam>() { this.ApproveMailParam });
            }
            if (this.ApproveRowObj.MailParamList.Count > 0)
            {
                List<LibMailParam> list = new List<LibMailParam>();
                foreach (var item in this.ApproveRowObj.MailParamList)
                {
                    if (item.Value.MailParam.To.Count > 0)
                    {
                        list.Add(item.Value.MailParam);
                    }
                }
                ThreadPool.QueueUserWorkItem(LibSMSHelper.SendMsgByMailList, list);
            }
        }

        #region 同步数据的相关方法      
          
        /// <summary>
        /// 同步数据到子站点
        /// </summary>       
        protected virtual void CallSynchroData()
        {
            if (this.DataSet.Tables.Contains(LibFuncPermission.SynchroDataSettingTableName) == false || CrossSiteHelper.ExistAxpSyncDataInfo == false ||
                CrossSiteHelper.ExistLinkSiteTable == false)
                return;
            if (this.IsCrossSiteCall)
                return;//本身已经是跨站点调用的不通过业务的数据变更触发数据同步，而是由跨站调用集中处理
            BcfSyncConfig syncConfig = this.Template.FuncPermission.SyncConfig;//同步配置
            DataTable syncDt = this.DataSet.Tables[LibFuncPermission.SynchroDataSettingTableName];
            syncDt.AcceptChanges();
            CrossSiteHelper.UpdateSyncDataSetting(syncDt);
            List<string> siteIdList = new List<string>();          
            DataRow[] syncRows = syncDt.Select(string.Format("ISSYNCTO = 1"));
            if (syncRows != null && syncRows.Length > 0)
            {
                foreach (DataRow row in syncRows)
                {
                    siteIdList.Add(LibSysUtils.ToString(row["SITEID"]));
                }
            }
            else
                return;//因为同步配置会在获取到Dataset时就填充好（上一层用户配置或者默认的可向其同步的子站点）， 如果从DataSet的同步信息从表配置中未筛选到同步信息行，则说明不需要同步了。

            LibSyncDataOpType opType = LibSyncDataOpType.Modify;
            //同步数据到站点
            Dictionary<string, LinkSiteInfo> linkSites = CrossSiteHelper.GetLinkSites(siteIdList.ToArray(), true);            
            if (linkSites != null && linkSites.Count > 0)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                Dictionary<string, object> mainRowPks = new Dictionary<string, object>();               
                string internalId = LibSysUtils.ToString(masterRow["INTERNALID"]);
                if (masterRow.RowState == DataRowState.Added || this.BillAction == BillAction.Delete)
                {
                    foreach (DataColumn column in this.DataSet.Tables[0].PrimaryKey)
                    {
                        mainRowPks[column.ColumnName] = masterRow[column.ColumnName];
                    }
                    if (this.BillAction == BillAction.Delete)
                        opType = LibSyncDataOpType.Delete;
                    else
                        opType = LibSyncDataOpType.AddNew;
                }
                else 
                {
                    foreach (DataColumn column in this.DataSet.Tables[0].PrimaryKey)
                    {
                        mainRowPks[column.ColumnName] = masterRow[column.ColumnName, DataRowVersion.Original];
                    }
                    opType = LibSyncDataOpType.Modify;
                }
                
                //移除虚表
                List<string> toRemoveTable = new List<string>();

                //子表或子子表中删除行的信息
                Dictionary<string, LibChangeRecord> subDeleteChanges = DataSetManager.GetChangeRecord(this.DataSet);
                if(subDeleteChanges!=null&& subDeleteChanges.Count > 0)
                {
                    //只保留子表的删除行信息
                    foreach(string tableName in subDeleteChanges.Keys)
                    {
                        if (tableName == this.DataSet.Tables[0].TableName)
                        {
                            subDeleteChanges[tableName].Remove.Clear();
                        }
                        subDeleteChanges[tableName].Add.Clear();
                        subDeleteChanges[tableName].Modif.Clear();
                    }
                }

                DataSet toSendDataset = this.DataSet.Copy();
                foreach(DataTable dt in toSendDataset.Tables)
                {
                    if (dt.ExtendedProperties.ContainsKey(TableProperty.IsVirtual) && (bool)dt.ExtendedProperties[TableProperty.IsVirtual])
                        toRemoveTable.Add(dt.TableName);
                    if (dt.TableName.Equals(this.DataSet.Tables[0].TableName) == false && syncConfig != null && syncConfig.NonSyncSubTables.Contains(dt.TableName))
                    {
                        //非主表且是不需要同步的子表,则清空其中的数据
                        dt.Clear();
                        if (subDeleteChanges.Keys.Contains(dt.TableName))
                            subDeleteChanges.Remove(dt.TableName);
                    }
                }
                toRemoveTable.ForEach(tableName => { toSendDataset.Tables.Remove(tableName); });
                toSendDataset.AcceptChanges();

                //存在可访问的从站，则合并获取从站的系统消息数据
                ExecuteBcfMethodParam callParams = new ExecuteBcfMethodParam()
                {
                    ProgId = this.ProgId,
                    MethodName = "SynchroData",
                    MethodParam = ExecuteBcfMethodParam.ConvertMethodParams(new object[] { this.BillAction, mainRowPks, subDeleteChanges, this.ExtendBcfParam, toSendDataset }),
                    TimeoutMillSecs = 30000, //设置超时时间，单位毫秒
                    IsSynchroDataCall = true
                };          
                try
                {
                    //阻塞模式调用远程方法
                    Dictionary<string, ExecuteBcfMethodResult> dicRets = CrossSiteHelper.CrossSiteBcfCall(this.Handle.Handle, linkSites.Keys.ToList(), callParams, this.DataAccess);                  
                    if (dicRets == null || dicRets.Keys.Count == 0)
                        return;
                    string retStr = string.Empty;
                    string syncInfo = string.Empty;
                    foreach (string key in dicRets.Keys)
                    {
                        syncInfo = string.Empty;
                        if (dicRets[key] != null && dicRets[key].Messages != null && dicRets[key].Messages.Count > 0)
                        {
                            dicRets[key].Messages.ForEach(msg =>
                            {
                                syncInfo += msg.Message + "\r\n";
                                msg.Message = string.Format("同步到站点:{0},消息:{1}", linkSites[key].ShortName, msg.Message);
                                this.ManagerMessage.AddMessage(msg);
                            });
                        }
                        CrossSiteHelper.AddSyncDataRecord(new SyncDataInfo()
                        {
                            ProgId = this.ProgId,
                            InternalId = internalId,
                            BillNo = (mainRowPks.Count > 0) ? Convert.ToString(mainRowPks.Values.First()) : "",
                            UserId = this.Handle.UserId,
                            SiteId = key,
                            SyncTime = DateTime.Now,
                            SyncOp = opType,
                            SyncState = dicRets[key].Messages.HasError() ? LibSyncDataState.SyncError : LibSyncDataState.Synced,
                            SyncInfo = syncInfo
                        });
                        
                    }
                }
                catch (Exception exp)
                {
                    LibCommUtils.AddOutput(@"Error\CrossSiteCall", string.Format("CallSynchroData error:{0}\r\nStacktrace:{1}", exp.Message, exp.StackTrace));
                    this.ManagerMessage.AddMessage(LibMessageKind.SysException, string.Format("同步数据到站点异常：{0}", exp.Message));
                }
            }
        }
        /// <summary>
        /// 同步数据。
        /// 可以由其他站点通过令牌信息跨站点调用
        /// </summary>
        /// <param name="billAction">业务操作类型</param>
        /// <param name="mainRowPks">主数据的主键值</param>
        /// <param name="subDeleteChanges">子表或子子表中删除行的信息</param>
        /// <param name="extendBcfParam">扩展参数信息</param>
        /// <param name="dataSet">数据集信息</param>
        public virtual void SynchroData(BillAction billAction, Dictionary<string, object> mainRowPks, Dictionary<string, LibChangeRecord> subDeleteChanges,
            Dictionary<string, object> extendBcfParam, DataSet dataSet)
        {
            if (mainRowPks == null || mainRowPks.Count == 0)
                return;
            object[] pks = mainRowPks.Values.ToArray();
            Dictionary<string, LibChangeRecord> changeRecords = null;
            Dictionary<string, string> extendParams = null;
            if (extendBcfParam != null && extendBcfParam.Keys.Count > 0)
            {
                extendParams = new Dictionary<string, string>();
                foreach (string key in extendBcfParam.Keys)
                {
                    extendParams[key] = JsonConvert.SerializeObject(extendBcfParam[key]);
                }
            }
            if (billAction == BillAction.Delete)
            {
                this.Delete(pks, extendParams);
            }
            else
            {
                BcfSyncConfig syncConfig = this.Template.FuncPermission.SyncConfig;//同步配置
                //先判断当前数据库中是否存在相关数据，并结合BillAction类型重新设定操作类型
                LibBcfData newBcf = (LibBcfData)LibBcfSystem.Default.GetBcfInstance(this.ProgId);
                newBcf.Handle = this.Handle;
                DataSet thisSiteData = newBcf.BrowseTo(pks);

                //处理修改状态的委托方法
                Action setModifyAction = delegate
                {
                    dataSet.AcceptChanges();
                    // 检查每一行与当前数据中的行的对应关系，如果当前不存在则设置为新增状态，否则设置为修改状态
                    DataTable thisDt = null;
                    DataRow thisRow = null;
                    List<object> keyValueList = new List<object>();//表主键值
                    Dictionary<string, object> dicValues = new Dictionary<string, object>();//数据行的各列的具体值临时存储
                    int tableCount = -1;
                    foreach (DataTable dt in dataSet.Tables)
                    {
                        tableCount++;//约定主表必须为第一个表
                        if (thisSiteData.Tables.Contains(dt.TableName) == false)
                            continue;//本站中不包含的数据表不进行处理                           
                        else
                        {
                            thisDt = thisSiteData.Tables[dt.TableName];
                        }
                        if (thisDt.PrimaryKey == null || thisDt.PrimaryKey.Length == 0)
                        {
                            continue;//没有主键无法比较数据也直接返回
                        }
                        int keyColumnCount = 0;
                        List<DataColumn> keyColList = new List<DataColumn>();
                        foreach (DataColumn col in thisDt.PrimaryKey)
                        {
                            if (dt.Columns.Contains(col.ColumnName))
                            {
                                keyColumnCount++;
                                keyColList.Add(dt.Columns[col.ColumnName]);
                            }
                        }
                        if (keyColumnCount != thisDt.PrimaryKey.Length)
                            continue;//主键不一致                       


                        foreach (DataRow row in dt.Rows)
                        {
                            keyValueList.Clear();
                            if (tableCount == 0)//对于主表，主键值使用传输过来的主键原始值
                                keyValueList = mainRowPks.Values.ToList();
                            else
                            {
                                foreach (DataColumn col in thisDt.PrimaryKey)
                                {
                                    if (dt.Columns.Contains(col.ColumnName))
                                    {
                                        if (mainRowPks.Keys.Contains(col.ColumnName))//对于其他表，主键值中包含的主表主键列使用传输过来的原始值
                                            keyValueList.Add(mainRowPks[col.ColumnName]);
                                        else
                                            keyValueList.Add(row[col.ColumnName]);
                                    }
                                }
                            }
                            if (keyValueList.Count == 0)
                                continue;
                            thisRow = thisDt.Rows.Find(keyValueList.ToArray());
                            if (thisRow == null)
                            {
                                row.SetAdded();//本地不存在的行新增
                                //检查不需要同步的数据列
                                if (syncConfig != null && syncConfig.NonSyncFields.Keys.Contains(thisDt.TableName) &&
                                syncConfig.NonSyncFields[thisDt.TableName].Count > 0)
                                {
                                    foreach(string fieldName in syncConfig.NonSyncFields[thisDt.TableName])
                                    {
                                        if (string.IsNullOrEmpty(fieldName) || thisDt.Columns.Contains(fieldName) == false 
                                        || dt.Columns.Contains(fieldName) == false)
                                            continue;
                                        if(thisDt.Columns[fieldName].AllowDBNull)
                                        {
                                            row[fieldName] = DBNull.Value;//对于不需要同步的列，新增时直接设置为空。
                                        }
                                    }
                                }
                            }
                            else
                            {
                                row.SetModified();
                                row.BeginEdit();
                                //通过重置数据，实现原始版本和当前版本不相同
                                try
                                {
                                    foreach (DataColumn col in dt.Columns)
                                    {
                                        dicValues[col.ColumnName] = row[col];
                                        if (keyColList.Contains(col) == false)
                                        {
                                            //非主键列直接设置为空，以便实现更改                                           
                                            row[col] = DBNull.Value;
                                        }
                                        else
                                        {
                                            if (mainRowPks.Keys.Contains(col.ColumnName))
                                                row[col] = mainRowPks[col.ColumnName];//子表中与主表中的主键列相同的，原始值设置为传输过来的值
                                            //子表中的其他主键列，一般为RowID、FormRowID等，都基本是只读的，不会变更。
                                        }
                                    }
                                    row.AcceptChanges();//先接受修改，然后再改回原来的值，这样实现历史版本和当前版本不一致。
                                    foreach (DataColumn col in dt.Columns)
                                    {                                      
                                        row[col] = dicValues[col.ColumnName];
                                    }
                                    //检查不需要同步的数据列
                                    if (syncConfig != null && syncConfig.NonSyncFields.Keys.Contains(thisDt.TableName) &&
                                    syncConfig.NonSyncFields[thisDt.TableName].Count > 0)
                                    {
                                        foreach (string fieldName in syncConfig.NonSyncFields[thisDt.TableName])
                                        {
                                            if (string.IsNullOrEmpty(fieldName) || thisDt.Columns.Contains(fieldName) == false
                                            || dt.Columns.Contains(fieldName) == false 
                                            || keyColList.Contains(dt.Columns[fieldName]) // 主键列必须同步
                                            )
                                                continue;
                                            if (thisDt.Columns[fieldName].ExtendedProperties.ContainsKey(FieldProperty.AllowEmpty)==false)
                                            {
                                                dt.Columns[fieldName].AllowDBNull = true;
                                                row[fieldName] = DBNull.Value;//对于不需要同步的列，直接设置为空。
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    row.EndEdit();
                                }
                            }
                        }
                        dt.PrimaryKey = keyColList.ToArray();  //因直接序列化过来的没有主键，需要重新设置。但要放到模拟实现了数据更改后再设置主键，不然会引起主键为空等错误。
                    }
                };
                if (billAction == BillAction.AddNew)
                {
                    //当前系统存在要添加的数据
                    if (thisSiteData != null && thisSiteData.Tables.Count > 0 && thisSiteData.Tables[0].Rows.Count > 0)
                    {
                        billAction = BillAction.Modif;
                        setModifyAction();
                    }
                }
                else if (billAction != BillAction.Browse)
                {
                    //除了新增、删除之外的其他操作，只要不是浏览且当前数据库中不存在，操作一律为修改状态
                    if (thisSiteData == null || thisSiteData.Tables.Count == 0 || thisSiteData.Tables[0].Rows.Count == 0)
                    {
                        billAction = BillAction.AddNew;
                    }
                    else
                    {
                        billAction = BillAction.Modif;
                        setModifyAction();
                    }
                }
                changeRecords = DataSetManager.GetChangeRecord(dataSet);
                if (billAction == BillAction.AddNew)
                {
                    this.Save(billAction, pks, changeRecords, extendParams);
                }
                else if (billAction == BillAction.Modif)
                {
                    //处理子表或子子表的删除行信息
                    LibChangeRecord tableChanges = null;
                    if (subDeleteChanges != null && subDeleteChanges.Count > 0)
                    {
                        foreach(string tableName in subDeleteChanges.Keys)
                        {
                            if (tableName == this.DataSet.Tables[0].TableName)
                                continue;
                            if (subDeleteChanges[tableName] == null || subDeleteChanges[tableName].Remove == null || subDeleteChanges[tableName].Remove.Count == 0)
                                continue;
                            if (changeRecords.ContainsKey(tableName) == false)
                                changeRecords[tableName] = subDeleteChanges[tableName];
                            else
                            {
                                tableChanges = changeRecords[tableName];
                                tableChanges.Remove = subDeleteChanges[tableName].Remove;                                                          
                            }                                
                        }
                    }
                    this.Edit(pks);//先编辑实现缓存中有该对象
                    this.Save(billAction, pks, changeRecords, extendParams);
                }
            }
        }
        #endregion
    }
}
