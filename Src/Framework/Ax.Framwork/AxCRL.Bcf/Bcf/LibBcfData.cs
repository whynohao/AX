using AxCRL.Comm;
using AxCRL.Comm.Bill;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Server;
using AxCRL.Data;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.ViewTemplate;
using AxCRL.Core.Mail;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using AxCRL.Comm.Runtime;
using AxCRL.Data.SqlBuilder;
using AxCRL.Core.Comm;
using AxCRL.Core.SysNews;
using AxCRL.Core.Permission;
using AxSRL.SMS.SignalR;
using System.Runtime.Serialization.Formatters.Binary;
namespace AxCRL.Bcf
{
    public partial class LibBcfData : LibBcfDataBase
    {
        private BillAction _BillAction;
        private bool _UsingAudit = false;
        private string _CurrentCodingNo;
        private ApproveRowObject _ApproveRowObj = null;
        private List<string> _UpdateTaskSqlList = null;
        private LibMailParam _ApproveMailParam = null;
        private string _ChangeReasonId = string.Empty;

        public string ChangeReasonId
        {
            get { return _ChangeReasonId; }
            set { _ChangeReasonId = value; }
        }

        public bool UsingMail
        {
            get { return !string.IsNullOrEmpty(EnvProvider.Default.MailProvider.Host); }
        }

        public LibMailParam ApproveMailParam
        {
            get
            {
                if (_ApproveMailParam == null)
                {
                    _ApproveMailParam = new LibMailParam();
                    _ApproveMailParam.ProgId = this.ProgId;
                    _ApproveMailParam.PersonId = this.Handle.PersonId;
                    _ApproveMailParam.MailKind = LibMailKind.Approve;
                }
                return _ApproveMailParam;
            }
        }

        public List<string> UpdateTaskSqlList
        {
            get
            {
                if (_UpdateTaskSqlList == null)
                    _UpdateTaskSqlList = new List<string>();
                return _UpdateTaskSqlList;
            }
        }

        private ApproveRowObject ApproveRowObj
        {
            get
            {
                if (_ApproveRowObj == null)
                    _ApproveRowObj = new ApproveRowObject();
                return _ApproveRowObj;
            }
        }

        public string CurrentCodingNo
        {
            get { return _CurrentCodingNo; }
            set { _CurrentCodingNo = value; }
        }

        public bool UsingAudit
        {
            get
            {
                string sql = string.Format("select APPROVEFLOWID from AXPAPPROVEFLOW where PROGID='{0}' and ISAPPROVEROW=0 and CURRENTSTATE=2", this.ProgId);
                string flowId = LibSysUtils.ToString(DataAccess.ExecuteScalar(sql));
                _UsingAudit = !string.IsNullOrEmpty(flowId);
                return _UsingAudit;
            }
        }

        public BillAction BillAction
        {
            get { return _BillAction; }
            set { _BillAction = value; }
        }

        public LibBcfData()
        {

        }
        /// <summary>
        /// 是否具有按部门岗位审核的功能
        /// </summary>
        public readonly static bool HasAduitOfDuty = false;
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static LibBcfData()
        {
            //初始即查找是否具有按部门岗位审核的相关字段
            //LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel("axp.ApproveFlow");
            //if (sqlModel != null && sqlModel.Tables.Count > 2 && sqlModel.Tables[2].Columns.Contains("DUTYID"))
            //{
            //    HasAduitOfDuty = true;
            //}
        }

        #region [单据操作检查]


        protected virtual bool CheckBrowseTo(object[] pks)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Browse);
            return ret;
        }

        protected virtual bool CheckAddNew(LibEntryParam entryParam)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Add);
            return ret;
        }

        protected virtual bool CheckModif(object[] pks)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Edit);
            if (ret)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                LibApproveFlow flow = LibApproveFlowCache.Default.GetCacheItem(this.ProgId);
                if (flow == null)
                    return true;//如果未配置审核流，则直接返回可以修改。 Zhangkj 20170323
                LibAuditState auditState = (LibAuditState)masterRow["AUDITSTATE"];
                switch (auditState)
                {
                    case LibAuditState.Submit:
                        if (flow.CanEditWhenDoing || this.IsSynchroDataCall)
                            ret = true;
                        else
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, "此类单据已提交审核时,不能修改。");
                            ret = false;
                        }
                        break;
                    case LibAuditState.Pass:
                        if (flow.CanEditWhenDone || this.IsSynchroDataCall)
                            ret = true;
                        else
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, "此类单据已审核时,不能修改。");
                            ret = false;
                        }
                        break;
                }
            }
            return ret;
        }


        protected virtual bool CheckDelete()
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Delete);
            if (ret)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                LibCurrentState currentState = (LibCurrentState)masterRow["CURRENTSTATE"];
                if (this.IsSynchroDataCall == false && (currentState == LibCurrentState.EndCase || currentState == LibCurrentState.Invalid))
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据状态为结案或作废,不能删除。");
                    ret = false;
                }
                if (LibSysUtils.ToBoolean(masterRow["ISUSED"]) && this.IsSynchroDataCall == false)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "数据被其他单据引用，不能删除。");
                    ret = false;
                }
                LibApproveFlow flow = LibApproveFlowCache.Default.GetCacheItem(this.ProgId);
                if (flow == null)
                    return true;//如果未配置审核流，则直接返回可以删除。 Zhangkj 20170323
                LibAuditState auditState = (LibAuditState)masterRow["AUDITSTATE"];
                switch (auditState)
                {
                    case LibAuditState.Submit:
                        if (this.IsSynchroDataCall == false)
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据已提交审核,不能删除。");
                            ret = false;
                        }
                        break;
                    case LibAuditState.Pass:
                        if (flow.CanDeleteWhenDone == false && this.IsSynchroDataCall == false)
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, "此类单据已审核时,不能删除。");
                            ret = false;
                        }
                        break;
                    default:
                        int flowLevel = LibSysUtils.ToInt32(masterRow["FLOWLEVEL"]);
                        if (flowLevel > 0 && this.IsSynchroDataCall == false)
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据已触发审核流程,不能删除。");
                            ret = false;
                        }
                        break;
                }
            }
            return ret;
        }

        protected virtual bool CheckRelease(object[] pks)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Release);
            if (ret)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                LibCurrentState currentState = (LibCurrentState)masterRow["CURRENTSTATE"];
                if (currentState != LibCurrentState.UnRelease)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据状态为未生效才能做生效操作。");
                    ret = false;
                }
            }
            return ret;
        }

        protected virtual bool CheckInvalid(object[] pks)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Invalid);
            if (ret)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                LibCurrentState currentState = (LibCurrentState)masterRow["CURRENTSTATE"];
                if (currentState != LibCurrentState.Release)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据状态为生效才能作废。");
                    ret = false;
                }
            }
            return ret;
        }

        protected bool DoCheckAudit(object[] pks)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Audit);
            if (ret)
            {
                if (this.UsingAudit)
                {
                    DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                    LibCurrentState currentState = (LibCurrentState)masterRow["CURRENTSTATE"];
                    if (currentState == LibCurrentState.Draft || currentState == LibCurrentState.EndCase || currentState == LibCurrentState.Invalid)
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前单据状态,不能做此操作。");
                        ret = false;
                    }
                }
                else
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据未启用审核流。");
                    ret = false;

                }
            }
            return ret;
        }

        protected virtual bool CheckSubmitAudit(object[] pks)
        {
            bool ret = DoCheckAudit(pks);
            if (ret)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                LibAuditState auditState = (LibAuditState)masterRow["AUDITSTATE"];
                switch (auditState)
                {
                    case LibAuditState.Submit:
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据已提交审核,不能重复提交。");
                        ret = false;
                        break;
                    case LibAuditState.Pass:
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据已审核,不能重复提交。");
                        ret = false;
                        break;
                }
                if (ret && this.Template.FuncPermission.UsingApproveRow)
                {
                    DataTable curTable = GetApproveRowTable();
                    foreach (DataRow curRow in curTable.Rows)
                    {
                        if (curRow.RowState == DataRowState.Deleted)
                            continue;
                        if (curTable.Columns.Contains("AUDITSTATE") == false || curTable.Columns.Contains("FLOWLEVEL") == false)
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据启用行项审核,但表身没有行审核需要的相关字段。");
                            ret = false;
                            break;
                        }
                        else
                        {
                            if ((LibAuditState)LibSysUtils.ToInt32(curRow["AUDITSTATE"]) != LibAuditState.Pass)
                            {
                                this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据启用行项审核,表身中存在未审核通过的记录。");
                                ret = false;
                                break;
                            }
                        }
                    }
                }
                if (ret)
                {
                    string errorInfo = string.Empty;
                    //检查是否可提交时，对于审核流程仅是查看配置，可选参数showConfig设置为true
                    SortedList<int, List<LibApproveFlowInfo>> flowList = GetApproveFlowForBillByDataRow(masterRow, out errorInfo, true);
                    if (flowList == null)
                    {
                        if (string.IsNullOrEmpty(errorInfo))
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, "未找到具体审核过程。");
                        else
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, errorInfo);
                        ret = false;
                    }
                }
            }
            return ret;
        }



        protected virtual bool CheckWithdrawAudit(object[] pks)
        {
            bool ret = true;
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            LibAuditState auditState = (LibAuditState)masterRow["AUDITSTATE"];
            switch (auditState)
            {
                case LibAuditState.UnSubmit:
                case LibAuditState.UnPass:
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据未提交审核,无需撤回。");
                    ret = false;
                    break;
                case LibAuditState.Pass:
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据已审核,不能撤回。");
                    ret = false;
                    break;
            }
            return ret;
        }

        protected virtual bool CheckAudit(object[] pks)
        {
            bool ret = DoCheckAudit(pks);
            if (ret)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                LibAuditState auditState = (LibAuditState)masterRow["AUDITSTATE"];
                switch (auditState)
                {
                    case LibAuditState.UnSubmit:
                    case LibAuditState.UnPass:
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据还未提交审核,不能做此操作。");
                        ret = false;
                        break;
                    case LibAuditState.Pass:
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据已审核,不能重复审核。");
                        ret = false;
                        break;
                }
            }
            return ret;
        }



        protected virtual bool CheckCancelAudit(object[] pks)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.CancelAudit);
            if (ret)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                int flowLevel = LibSysUtils.ToInt32(masterRow["FLOWLEVEL"]);
                if (flowLevel == 0)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据审核层级为0，不能做此操作。");
                    ret = false;
                }
            }
            return ret;
        }

        #region [行项审核]

        protected virtual bool CheckSubmitAuditRow(DataRow curRow)
        {
            bool ret = true;
            if (curRow.Table != null && curRow.Table.Columns.Contains("AUDITSTATE") == false)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "行项无审核字段，请检查是否已启用行审核。");
                return false;
            }
            LibAuditState auditState = (LibAuditState)curRow["AUDITSTATE"];
            switch (auditState)
            {
                case LibAuditState.Submit:
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "行项已提交审核,不能重复提交。");
                    ret = false;
                    break;
                case LibAuditState.Pass:
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "行项已审核,不能重复提交。");
                    ret = false;
                    break;
            }
            return ret;
        }



        protected virtual bool CheckWithdrawAuditRow(DataRow curRow)
        {
            bool ret = true;
            LibAuditState auditState = (LibAuditState)curRow["AUDITSTATE"];
            switch (auditState)
            {
                case LibAuditState.UnSubmit:
                case LibAuditState.UnPass:
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "行项未提交审核,无需撤回。");
                    ret = false;
                    break;
                case LibAuditState.Pass:
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "行项已审核,不能撤回。");
                    ret = false;
                    break;
            }
            return ret;
        }

        protected virtual bool CheckAuditRow(DataRow curRow)
        {
            bool ret = true;
            LibAuditState auditState = (LibAuditState)curRow["AUDITSTATE"];
            switch (auditState)
            {
                case LibAuditState.UnSubmit:
                case LibAuditState.UnPass:
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "行项还未提交审核,不能做此操作。");
                    ret = false;
                    break;
                case LibAuditState.Pass:
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "行项已审核,不能重复审核。");
                    ret = false;
                    break;
            }
            return ret;
        }

        protected virtual bool CheckCancelAuditRow(DataRow curRow)
        {
            bool ret = true;
            int flowLevel = LibSysUtils.ToInt32(curRow["FLOWLEVEL"]);
            if (flowLevel == 0)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "行项审核层级为0，不能做此操作。");
                ret = false;
            }
            return ret;
        }

        #endregion

        protected virtual bool CheckEndCase(object[] pks)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.EndCase);
            if (ret)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                LibCurrentState currentState = (LibCurrentState)masterRow["CURRENTSTATE"];
                if (currentState != LibCurrentState.Release)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据状态为生效才能结案。");
                    ret = false;
                }
                if (this.UsingAudit)
                {
                    LibAuditState auditState = (LibAuditState)masterRow["AUDITSTATE"];
                    if (auditState != LibAuditState.Pass)
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据已启用审核流,需先审核后才能结案。");
                        ret = false;
                    }
                }
            }
            return ret;
        }

        protected virtual bool CheckCancelEndCase(object[] pks)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.CancelEndCase);
            if (ret)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                LibCurrentState currentState = (LibCurrentState)masterRow["CURRENTSTATE"];
                if (currentState != LibCurrentState.EndCase)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据状态结案才能做此操作。");
                    ret = false;
                }
            }
            return ret;
        }

        protected virtual bool CheckCancelInvalid(object[] pks)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.CancelInvalid);
            if (ret)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                LibCurrentState currentState = (LibCurrentState)masterRow["CURRENTSTATE"];
                if (currentState != LibCurrentState.Invalid)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据状态为非作废状态。");
                    ret = false;
                }
            }
            return ret;
        }

        protected virtual bool CheckCancelRelease(object[] pks)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.CancelRelease);
            if (ret)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                LibAuditState auditState = (LibAuditState)masterRow["AUDITSTATE"];
                if (auditState == LibAuditState.Pass)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据已审核,不能取消生效。");
                    ret = false;
                }
                else
                {
                    LibCurrentState currentState = (LibCurrentState)masterRow["CURRENTSTATE"];
                    if (currentState != LibCurrentState.Release)
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "单据状态为生效才能做此操作。");
                        ret = false;
                    }
                    else if (LibSysUtils.ToBoolean(masterRow["ISUSED"]))
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "数据被其他单据引用，不能取消生效。");
                        ret = false;
                    }
                }
            }
            return ret;
        }

        #endregion

        #region [浏览单据]

        public DataSet BrowseTo(object[] pks)
        {
            if (CheckBrowseTo(pks))
            {
                DataSetManager.GetDataSet(this.DataSet, this.DataAccess, this.ProgId, pks, this.Handle);
                this.AfterChangeData(this.DataSet);
                this.DataSet.AcceptChanges();
                if (this.Template.FuncPermission.UsingDynamicColumn)
                {
                    DoFillDataDynamicTable();
                }
            }
            return this.DataSet;
        }

        #endregion

        #region [单据新增]

        private void DoAddNew(LibEntryParam entryParam)
        {
            DataRow masterRow = this.DataSet.Tables[0].NewRow();
            masterRow.BeginEdit();
            try
            {
                if (entryParam != null && entryParam.ParamStore.Count > 0)
                {
                    foreach (var item in entryParam.ParamStore)
                    {
                        masterRow[item.Key] = item.Value;
                        //如果存在关联字段，则取关联字段值
                        if (masterRow.Table.Columns[item.Key].ExtendedProperties.ContainsKey(FieldProperty.RelativeSource))
                        {
                            RelativeSourceCollection relSourceList = (RelativeSourceCollection)masterRow.Table.Columns[item.Key].ExtendedProperties[FieldProperty.RelativeSource];
                            if (relSourceList.Count > 0)
                            {
                                object[] curPks = null;
                                RelativeSource relSource = relSourceList[0];
                                if (!string.IsNullOrEmpty(relSource.RelPK))
                                {
                                    string[] relPKs = relSource.RelPK.Split(';');
                                    int len = relPKs.Length;
                                    curPks = new object[len + 1];
                                    for (int i = 0; i < len; i++)
                                    {
                                        curPks[i] = masterRow[relPKs[i]];
                                    }
                                    curPks[len - 1] = item.Value;
                                }
                                else
                                {
                                    curPks = new object[1];
                                    curPks[0] = item.Value;
                                }
                                Dictionary<string, object> dic = this.CheckFieldValue(0, item.Key, relSource.RelSource, curPks, null);
                                foreach (var dest in dic)
                                {
                                    masterRow[dest.Key] = dest.Value;
                                }
                            }
                        }
                    }
                }
                //对主键赋值
                string fieldName = masterRow.Table.PrimaryKey[masterRow.Table.PrimaryKey.Length - 1].ColumnName;

                #region 实体编码
                string codingNo = string.Empty;
                try
                {
                    codingNo = LibCodingNoServer.Default.GetCodingNo(this.Template.BillType, this.ProgId, fieldName, masterRow, true, this.DataAccess);
                    if (!string.IsNullOrEmpty(codingNo))
                        masterRow[fieldName] = codingNo;
                }
                catch (Exception)
                {
                }
                #endregion

                //为系统字段赋值
                masterRow["CREATORID"] = this.Handle.PersonId;
                masterRow["CREATORNAME"] = this.Handle.PersonName;
                masterRow["CREATETIME"] = LibDateUtils.Now();
                if (this.Template.BillType == BillType.Bill && masterRow.Table.Columns.Contains("BILLDATE"))
                    masterRow["BILLDATE"] = LibDateUtils.GetCurrentDate();
            }
            finally
            {
                masterRow.EndEdit();
            }
            this.DataSet.Tables[0].Rows.Add(masterRow);
        }
        /// <summary>
        /// 设置单据状态
        /// </summary>
        /// <param name="masterRow">表头行</param>
        private void SetCurrentState(DataRow masterRow)
        {
            //为system handle则应该由调用方对current state赋值（考虑系统创建的数据不应该有草稿状态的，如果传入的是草稿状态多半是RD没有赋值，由于此判断为后期加入，为了兼容之前的项目，特殊判断为草稿状态时走机制默认赋值流程）
            if ((LibCurrentState)LibSysUtils.ToInt32(masterRow["CURRENTSTATE"]) == LibCurrentState.Draft || this.Handle != LibHandleCache.Default.GetSystemHandle())
            {
                if (this.Template.BillType == AxCRL.Template.BillType.Bill)
                {
                    if (!string.IsNullOrEmpty(this.Template.FuncPermission.BillTypeName))
                    {
                        int defaultCreateState = LibSysUtils.ToInt32(LibParamCache.Default.GetValueByName(this.Template.FuncPermission.BillTypeName, new object[] { masterRow["TYPEID"] }, "DEFAULTCREATESTATE"));
                        if (defaultCreateState == 0)
                            masterRow["CURRENTSTATE"] = (int)LibCurrentState.UnRelease;
                        else
                            masterRow["CURRENTSTATE"] = (int)LibCurrentState.Release;
                    }
                    else
                        masterRow["CURRENTSTATE"] = (int)LibCurrentState.UnRelease;
                }
                else
                {
                    //有启用审核流，则保存时为未生效，否则为生效
                    if (this.UsingAudit)
                        masterRow["CURRENTSTATE"] = (int)LibCurrentState.UnRelease;
                    else
                        masterRow["CURRENTSTATE"] = (int)LibCurrentState.Release;
                }
            }
        }


        private void CopyDataRow(DataRow curRow, DataRow copyRow)
        {
            curRow.BeginEdit();
            try
            {
                foreach (DataColumn col in copyRow.Table.Columns)
                {
                    if (col.ExtendedProperties.ContainsKey(FieldProperty.AllowCopy))
                    {
                        if ((bool)col.ExtendedProperties[FieldProperty.AllowCopy])
                            curRow[col.ColumnName] = copyRow[col];
                    }
                    else
                        curRow[col.ColumnName] = copyRow[col];
                }
            }
            finally
            {
                curRow.EndEdit();
            }
        }

        private void CopyDataSet(object[] copyPks)
        {
            DataSet copyDataSet = this.DataSet.Clone();
            DataSetManager.GetDataSet(copyDataSet, this.DataAccess, this.ProgId, copyPks, this.Handle);
            this.AfterChangeData(copyDataSet);
            this.DataSet.AcceptChanges();
            int i = 0;
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            DataColumn[] curPkCols = masterRow.Table.PrimaryKey;
            foreach (DataTable copyTable in copyDataSet.Tables)
            {
                if (copyTable.ExtendedProperties.ContainsKey(TableProperty.AllowCopy) && (bool)copyTable.ExtendedProperties[TableProperty.AllowCopy] == false)
                    continue;
                if (i == 0)
                {
                    CopyDataRow(masterRow, copyTable.Rows[0]);
                }
                else
                {
                    DataTable curTable = this.DataSet.Tables[copyTable.TableName];
                    curTable.BeginLoadData();
                    try
                    {
                        foreach (DataRow row in copyTable.Rows)
                        {
                            DataRow newRow = curTable.NewRow();
                            CopyDataRow(newRow, row);
                            //复制表头主键
                            foreach (DataColumn item in curPkCols)
                            {
                                newRow[item.ColumnName] = masterRow[item];
                            }
                            curTable.Rows.Add(newRow);
                        }
                    }
                    finally
                    {
                        curTable.EndLoadData();
                    }
                }
                i++;
            }
        }

        public DataSet AddNew(LibEntryParam entryParam, object[] copyPks = null)
        {
            this.BillAction = BillAction.AddNew;
            bool isCopy = copyPks != null;
            if (this.CheckAddNew(entryParam))
            {
                BeforeAddNew();
                DoAddNew(entryParam);
                if (isCopy)
                {
                    CopyDataSet(copyPks);
                }
                AfterAddNew();
            }
            if (this.Template.FuncPermission.UsingDynamicColumn && entryParam != null)
            {
                DataSet dataSet = this.ChangeTableStructure(entryParam);
                if (isCopy)
                    dataSet = FillDataDynamicTable(dataSet);
                return dataSet; //这里不直接this.DataSet = dataSet,应为后续保存调用InnerSave时，需再次装换结构
            }
            return this.DataSet;
        }

        protected virtual void BeforeAddNew()
        {

        }

        protected virtual void AfterAddNew()
        {
            //附加同步数据到各站点的配置及历史记录
            this.AddSyncTableData(this.DataSet);
        }
        /// <summary>
        /// 添加数据同步相关的数据表和数据
        /// </summary>
        private void AddSyncTableData(DataSet dataSet)
        {
            if (dataSet == null)
                return;
            //if (dataSet.Tables.Contains(LibFuncPermission.SynchroDataSettingTableName) && LibTemplate.HasAxpLinkSite)
            if (dataSet.Tables.Contains(LibFuncPermission.SynchroDataSettingTableName))
            {
                string progId = this.ProgId;
                string internalId = string.Empty;
                if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Columns.Contains("INTERNALID") && dataSet.Tables[0].Rows.Count > 0)
                    internalId = LibSysUtils.ToString(dataSet.Tables[0].Rows[0]["INTERNALID"]);

                DataTable dt = dataSet.Tables[LibFuncPermission.SynchroDataSettingTableName];
                if (dt != null && string.IsNullOrEmpty(progId) == false && this.Handle != null)
                {
                    CrossSiteHelper.FillSyncDataSetting(this.Handle, dt, progId);
                }
                dt = dataSet.Tables[LibFuncPermission.SynchroDataHisTableName];
                if (dt != null && string.IsNullOrEmpty(progId) == false && this.Handle != null)
                {
                    CrossSiteHelper.FillSyncDataHistory(this.Handle, dt, progId, internalId);
                }
            }
        }
        /// <summary>
        /// 前端调用BillService方法获取某个Bcf数据的主表(第0个表)列表数据前触发
        /// 可以做一些自定义的操作，如添加自定义的查询条件等
        /// </summary>
        /// <param name="libHandle">登录用户信息</param>
        /// <param name="table">第0张数据表</param>   
        /// <param name="listingQuery">查询条件</param>
        /// <param name="entryParam">入口参数</param>
        public virtual void BeforeFillList(LibHandle libHandle, DataTable table, object listingQuery, LibEntryParam entryParam)
        {

        }
        /// <summary>
        /// 前端调用BillService方法获取到某个Bcf数据的主表(第0个表)列表数据后触发
        /// 可以做一些自定义的操作，如添加自定义的数据行等
        /// </summary>
        /// <param name="libHandle">登录用户信息</param>
        /// <param name="table">第0张数据表</param>   
        /// <param name="listingQuery">查询条件</param>
        /// <param name="entryParam">入口参数</param>
        public virtual void AfterFillList(LibHandle libHandle, DataTable table, object listingQuery, LibEntryParam entryParam)
        {

        }
        /// <summary>
        /// 获取由功能模块确定的LibGridScheme
        /// zhangkj 20161201 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="entryParam"></param>
        /// <returns></returns>
        public virtual LibGridScheme GetDefinedGridScheme(LibHandle handle, LibEntryParam entryParam)
        {
            return null;
        }
        #endregion

        #region [单据修改]

        public DataSet Edit(object[] pks)
        {
            this.BillAction = BillAction.Modif;
            GetDataThenToCache(pks);
            if (this.CheckModif(pks))
            {
                if (this.Template.FuncPermission.UsingDynamicColumn)
                {
                    DoFillDataDynamicTable();
                }
            }
            else
            {
                string key = GetDataSetCacheKey(pks);
                LibBillDataCache.Default.Remove(key);
            }
            return this.DataSet;
        }


        #endregion

        #region [单据删除]

        private void DoDelete()
        {
            DataTable headerTable = this.DataSet.Tables[0];
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            string sql = string.Empty;
            StringBuilder whereBuilder = new StringBuilder();
            foreach (var item in headerTable.PrimaryKey)
            {
                if (item.DataType == typeof(string))
                    whereBuilder.AppendFormat(" {0}={1} And", item.ColumnName, LibStringBuilder.GetQuotString(LibSysUtils.ToString(masterRow[item.ColumnName])));
                else
                    whereBuilder.AppendFormat(" {0}={1} And", item.ColumnName, masterRow[item.ColumnName]);
            }
            whereBuilder.Remove(whereBuilder.Length - 3, 3);
            sql = string.Format("Delete {0} where {1}", "{0}", whereBuilder.ToString());
            List<string> deleteList = new List<string>();
            foreach (DataTable table in this.DataSet.Tables)
            {
                bool isVirtual = table.ExtendedProperties.ContainsKey(TableProperty.IsVirtual) ? (bool)table.ExtendedProperties[TableProperty.IsVirtual] : false;
                if (isVirtual) continue;
                deleteList.Add(string.Format(sql, table.TableName));
            }
            if (deleteList.Count > 0)
                this.DataAccess.ExecuteNonQuery(deleteList);
        }

        public DataSet Delete(object[] pks, Dictionary<string, string> extendParam = null)
        {
            this.BillAction = BillAction.Delete;
            if (extendParam != null && extendParam.Count != 0)
            {
                foreach (var item in extendParam)
                {
                    ExtendBcfParam[item.Key] = JsonConvert.DeserializeObject(extendParam[item.Key], RegisterBcfParamType[item.Key]);
                }
            }
            DataSetManager.GetDataSet(this.DataSet, this.DataAccess, this.ProgId, pks, this.Handle);
            this.DataSet.AcceptChanges();
            if (this.CheckDelete())
            {
                LibDBTransaction trans = this.DataAccess.BeginTransaction();
                try
                {
                    this.BeforeDelete();
                    this.DoDelete();
                    this.AfterDelete();
                    if (this.ManagerMessage.IsThrow)
                    {
                        trans.Rollback();
                        this.DataSet.RejectChanges();
                    }
                    else
                    {
                        if (this.Template.FuncPermission.UsingCache)
                            LibParamCache.Default.RemoveCacheItem(this.ProgId, pks);
                        trans.Commit();
                    }
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
                this.AfterCommintData();
            }
            return this.DataSet;
        }

        protected virtual void BeforeDelete()
        {

        }

        protected virtual void AfterDelete()
        {
            //同步数据
            if (this.Template.FuncPermission.UseSynchroData && this.ManagerMessage.IsThrow == false)
                CallSynchroData();
        }

        #endregion

        #region [单据保存]

        protected virtual void BeforeUpdate()
        {

        }

        protected virtual void AfterUpdate()
        {
            UpdateApproveDataVersion();
            //同步数据
            if (this.Template.FuncPermission.UseSynchroData && this.ManagerMessage.IsThrow == false)
                CallSynchroData();
        }

        private void UpdateApproveDataVersion()
        {
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            if (this.BillAction == BillAction.CancelAudit)
            {
                if ((LibAuditState)LibSysUtils.ToInt32(masterRow["AUDITSTATE", DataRowVersion.Original]) == LibAuditState.Pass &&
                (LibAuditState)LibSysUtils.ToInt32(masterRow["AUDITSTATE"]) != LibAuditState.Pass)
                {
                    string data = LibBillDataSerializeHelper.Serialize(this.DataSet);
                    this.DataAccess.ExecuteStoredProcedure("axpInsertApproveDataVersion", this.ProgId, masterRow["INTERNALID"],
                        0, LibDateUtils.GetCurrentDateTime(), this.ChangeReasonId, data);
                }
            }
            if (this.BillAction == BillAction.CancelApproveRow)
            {
                if (this.ApproveRowObj.Records.Count > 0)
                {
                    DataTable curTable = null;
                    DataSet dataSet = this.DataSet;
                    if (this.Template.FuncPermission.UsingDynamicColumn)
                    {
                        LibEntryParam entryParam = new LibEntryParam();
                        foreach (string colName in this.Template.FuncPermission.EntryParam)
                        {
                            entryParam.ParamStore.Add(colName, masterRow[colName]);
                        }
                        dataSet = this.ChangeTableStructure(entryParam);  //后面使用的dataSet不是中间层的DataSet，因为afterchangedata后还会调用ChangeTableStructure
                        dataSet = FillDataDynamicTable(dataSet);
                        for (int i = 1; i < this.DataSet.Tables.Count; i++)
                        {
                            if (this.DataSet.Tables[i].ExtendedProperties.ContainsKey(TableProperty.UsingApproveRow))
                            {
                                if ((bool)this.DataSet.Tables[i].ExtendedProperties[TableProperty.UsingApproveRow])
                                {
                                    curTable = dataSet.Tables[i];
                                    break;
                                }
                            }
                        }
                    }
                    long createTime = LibDateUtils.GetCurrentDateTime();
                    foreach (var curRow in this.ApproveRowObj.Records)
                    {
                        DataRow destRow = null;
                        if (curTable == null)
                        {
                            destRow = curRow;
                        }
                        else
                        {
                            object[] pks = new object[curTable.PrimaryKey.Length];
                            for (int i = 0; i < pks.Length; i++)
                            {
                                pks[i] = curRow[curTable.PrimaryKey[i].ColumnName];
                            }
                            destRow = curTable.Rows.Find(pks);
                        }
                        LibBillDataRow billDataRow = new LibBillDataRow();
                        foreach (DataColumn col in destRow.Table.Columns)
                        {
                            billDataRow.Data.Add(col.ColumnName, destRow[col]);
                        }
                        for (int i = 0; i < destRow.Table.ChildRelations.Count; i++)
                        {
                            DataRow[] childRows = destRow.GetChildRows(destRow.Table.ChildRelations[i]);
                            if (childRows != null && childRows.Length > 0)
                            {
                                DataTable childTable = childRows[0].Table;
                                billDataRow.SubData.Add(childTable.TableName, new List<Dictionary<string, object>>());
                                List<Dictionary<string, object>> list = billDataRow.SubData[childTable.TableName];
                                foreach (var childRow in childRows)
                                {
                                    Dictionary<string, object> dic = new Dictionary<string, object>();
                                    foreach (DataColumn col in childTable.Columns)
                                    {
                                        dic.Add(col.ColumnName, childRow[col]);
                                    }
                                    list.Add(dic);
                                }
                            }
                        }
                        string data = JsonConvert.SerializeObject(billDataRow);
                        this.DataAccess.ExecuteStoredProcedure("axpInsertApproveDataVersion", this.ProgId, masterRow["INTERNALID"], destRow["ROW_ID"], createTime,
                            this.ApproveRowObj.ChangeReasonId, data);
                    }
                }
            }
        }

        protected virtual void AfterCommintData()
        {

        }

        protected virtual void AfterChangeData(DataSet tables)
        {

        }

        private void AnewSetCodingNo(DataRow masterRow, bool isImportState)
        {
            string fieldName = masterRow.Table.PrimaryKey[masterRow.Table.PrimaryKey.Length - 1].ColumnName;
            CodingRule codingRule;
            bool addNew = isImportState;

            #region 编码
            string codingValue = string.Empty;
            try
            {
                codingValue = LibCodingNoServer.Default.GetCodingNo(this.Template.BillType, this.ProgId, fieldName, masterRow, addNew, out codingRule, this.DataAccess);
                if ((isImportState || codingRule.CreateOnSave) && !string.IsNullOrEmpty(codingValue))
                {
                    this.CurrentCodingNo = codingValue;
                    List<DataTable> dtList = new List<DataTable>();
                    try
                    {
                        foreach (DataTable table in this.DataSet.Tables)
                        {
                            if (this.DataSet.Tables.Count > 0)
                            {
                                table.BeginLoadData();
                                dtList.Add(table);
                                foreach (DataRow curRow in table.Rows)
                                {
                                    curRow.BeginEdit();
                                    try
                                    {
                                        curRow[fieldName] = codingValue;
                                    }
                                    finally
                                    {
                                        curRow.EndEdit();
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        foreach (var table in dtList)
                        {
                            table.EndLoadData();
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            #endregion
        }

        private void ReturnCodingNo(DataRow masterRow)
        {
            if (!string.IsNullOrEmpty(this.CurrentCodingNo))
                LibCodingNoServer.Default.ReturnCodingNo(this.Template.BillType, this.ProgId, masterRow, this.CurrentCodingNo);
        }


        private void SetSystemFieldValue(DataRow masterRow)
        {
            long currentTime = LibDateUtils.GetCurrentDateTime();

            masterRow.BeginEdit();
            try
            {
                if (this.BillAction == BillAction.AddNew || this.BillAction == BillAction.SubmitDraft)
                {
                    //设置单据状态
                    //this.SetCurrentState(masterRow);
                }
                if (this.BillAction == BillAction.AddNew || this.BillAction == BillAction.SaveToDraft)
                {
                    masterRow["CREATETIME"] = LibDateUtils.Now();
                    masterRow["INTERNALID"] = Guid.NewGuid().ToString();
                    masterRow["CREATORID"] = this.Handle.PersonId;
                    masterRow["CREATORNAME"] = this.Handle.PersonName;
                }
                //if (this.Template.BillType == BillType.Master)
                //{
                //    int validityStartDate = LibSysUtils.ToInt32(masterRow["VALIDITYSTARTDATE"]);
                //    int validityEndDate = LibSysUtils.ToInt32(masterRow["VALIDITYENDDATE"]);
                //    if (validityEndDate != 0 && validityStartDate > validityEndDate)
                //    {
                //        this.ManagerMessage.AddMessage(LibMessageKind.Error, "有效期开始日期大于有效期结束日期");
                //    }
                //    else
                //    {
                //        bool isValidity = LibSysUtils.ToBoolean(masterRow["ISVALIDITY"]);
                //        int curDate = LibDateUtils.GetCurrentDate();
                //        bool curIsValidity = (curDate >= validityStartDate && (curDate <= validityEndDate || validityEndDate == 0));
                //        if (isValidity != curIsValidity)
                //            masterRow["ISVALIDITY"] = curIsValidity;
                //    }
                //}
                switch (BillAction)
                {
                    case BillAction.Release:
                        masterRow["CURRENTSTATE"] = (int)LibCurrentState.Release;
                        break;
                    case BillAction.CancelRelease:
                        masterRow["CURRENTSTATE"] = (int)LibCurrentState.UnRelease;
                        break;
                    case BillAction.Invalid:
                        masterRow["CURRENTSTATE"] = (int)LibCurrentState.Invalid;
                        break;
                    case BillAction.CancelInvalid:
                        masterRow["CURRENTSTATE"] = (int)LibCurrentState.Release;
                        break;
                    case BillAction.AuditPass:
                    case BillAction.AuditUnPass:
                    case BillAction.CancelAudit:
                        masterRow["APPROVRID"] = this.Handle.PersonId;
                        masterRow["APPROVRNAME"] = this.Handle.PersonId;
                        masterRow["APPROVALTIME"] = currentTime;
                        break;
                    case BillAction.EndCase:
                        masterRow["CURRENTSTATE"] = (int)LibCurrentState.EndCase;
                        masterRow["ENDCASEID"] = this.Handle.PersonId;
                        masterRow["ENDCASENAME"] = this.Handle.PersonId;
                        masterRow["ENDCASETIME"] = currentTime;
                        break;
                    case BillAction.CancelEndCase:
                        masterRow["CURRENTSTATE"] = (int)LibCurrentState.Release;
                        masterRow["ENDCASEID"] = string.Empty;
                        masterRow["ENDCASENAME"] = string.Empty;
                        masterRow["ENDCASETIME"] = 0;
                        break;
                    case BillAction.SubmitAudit:
                        masterRow["AUDITSTATE"] = (int)LibAuditState.Submit;
                        if (masterRow.Table != null && masterRow.Table.Columns.Contains("SUMMITAUDITTIME"))
                        {
                            //提交审核时间
                            masterRow["SUMMITAUDITTIME"] = currentTime;
                        }
                        break;
                    case BillAction.WithdrawAudit:
                        masterRow["AUDITSTATE"] = (int)LibAuditState.UnSubmit;
                        break;
                }
                masterRow["LASTUPDATEID"] = this.Handle.PersonId;
                masterRow["LASTUPDATENAME"] = this.Handle.PersonName;
                masterRow["LASTUPDATETIME"] = currentTime;
            }
            finally
            {
                masterRow.EndEdit();
            }
        }

        public string ExportData(object[] pks)
        {
            string fileName = string.Empty;
            bool ret = CheckHasPermission(FuncPermissionEnum.Export);
            if (ret)
            {
                if (pks == null || pks.Length == 0)
                    return string.Empty;
                DataSetManager.GetDataSet(this.DataSet, this.DataAccess, this.ProgId, pks, this.Handle);
                this.AfterChangeData(this.DataSet);
                if (this.Template.FuncPermission.UsingDynamicColumn)
                    DoFillDataDynamicTable();
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < pks.Length; i++)
                {
                    builder.Append(pks[i]);
                }
                fileName = string.Format("{0}-{1}.xls", this.ProgId, string.Format("{0}{1}", builder.ToString(), LibDateUtils.GetCurrentDateTime()));
                string filePath = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "ExportData", fileName);
                AxCRL.Core.Excel.LibExcelHelper libExcelHelper = new Core.Excel.LibExcelHelper();
                libExcelHelper.ExportToExcel(filePath, this.DataSet);
            }
            return fileName;
        }

        public DataSet GetBatchImportData(string fileName, LibEntryParam entryParam = null)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Import);
            if (ret)
            {
                DataSet batchDataSet = null;
                if (this.Template.FuncPermission.UsingDynamicColumn)
                {
                    batchDataSet = this.ChangeTableStructure(entryParam).Clone();
                }
                else
                {
                    batchDataSet = this.DataSet.Clone();
                }
                this.AddSyncTableData(batchDataSet);
                AxCRL.Core.Excel.LibExcelHelper libExcelHelper = new Core.Excel.LibExcelHelper();
                libExcelHelper.ImportToDataSet(System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "ImportData", fileName), batchDataSet, this.AfterImportDataInTable, this.AfterImportDataInRow);
                return batchDataSet;
            }
            else
            {
                return this.DataSet;
            }
        }

        public DataSet ImportDataSet(DataSet dataSet, LibEntryParam entryParam = null)
        {
            if (this.Template.FuncPermission.UsingDynamicColumn)
            {
                Dictionary<String, LibChangeRecord> changeRecord = DataSetManager.GetChangeRecord(dataSet);
                this.CreateDynamicFieldRelation(entryParam);
                DataSetManager.ChangeDataHandle(this.DataSet, changeRecord, this.ManagerMessage, AddNewDynamicRow, ModifDynamicPk, ModifDynamicRow, DeleteDynamicRow);
            }
            else
            {
                this.DataSet = dataSet;
            }
            return DoImportData();
        }

        private DataSet DoImportData()
        {
            this.BillAction = BillAction.AddNew;
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                CheckImportData(this.DataSet);
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                AnewSetCodingNo(masterRow, true);
                SetSystemFieldValue(masterRow);
                this.BeforeUpdate();
                DataSetManager.SubmitData(this.DataSet, this.DataAccess);
                this.AfterUpdate();
                if (this.ManagerMessage.IsThrow && this.BillAction != BillAction.SaveToDraft)
                {
                    trans.Rollback();
                    ReturnCodingNo(masterRow);
                    this.DataSet.RejectChanges();
                }
                else
                {
                    trans.Commit();
                }
            }
            catch
            {
                trans.Rollback();
                ReturnCodingNo(this.DataSet.Tables[0].Rows[0]);
                this.DataSet.RejectChanges();
                throw;
            }
            this.AfterCommintData();
            this.AfterChangeData(this.DataSet);
            this.DataSet.AcceptChanges();
            return this.DataSet;
        }

        public DataSet ImportData(string fileName, LibEntryParam entryParam = null)
        {
            bool ret = CheckHasPermission(FuncPermissionEnum.Import);
            if (ret)
            {
                DataSet destDataSet = null;
                if (this.Template.FuncPermission.UsingDynamicColumn)
                {
                    destDataSet = this.ChangeTableStructure(entryParam);
                }
                else
                {
                    destDataSet = this.DataSet;
                }
                AxCRL.Core.Excel.LibExcelHelper libExcelHelper = new Core.Excel.LibExcelHelper();
                libExcelHelper.ImportToDataSet(System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.RuningPath, "TempData", "ImportData", fileName), destDataSet, this.AfterImportDataInTable, this.AfterImportDataInRow);
                if (this.Template.FuncPermission.UsingDynamicColumn)
                {
                    Dictionary<String, LibChangeRecord> changeRecord = DataSetManager.GetChangeRecord(destDataSet);
                    DataSetManager.ChangeDataHandle(this.DataSet, changeRecord, this.ManagerMessage, AddNewDynamicRow, ModifDynamicPk, ModifDynamicRow, DeleteDynamicRow);
                }
                DoImportData();
                if (this.Template.FuncPermission.UsingDynamicColumn)
                {
                    DoFillDataDynamicTable();
                }
            }
            return this.DataSet;
        }

        private void SetExtendBcfParam(Dictionary<string, string> extendParam)
        {
            if (extendParam != null && extendParam.Count != 0)
            {
                foreach (var item in extendParam)
                {
                    ExtendBcfParam[item.Key] = JsonConvert.DeserializeObject(extendParam[item.Key], RegisterBcfParamType[item.Key]);
                }
            }
        }

        public DataSet InnerSave(BillAction billAction, object[] pks, DataSet changeDataSet, Dictionary<string, string> extendParam = null)
        {
            this.BillAction = billAction;
            SetExtendBcfParam(extendParam);
            string cacheKey = GetDataSetCacheKey(pks);
            bool isAddNew = BillAction == BillAction.AddNew || BillAction == BillAction.SaveToDraft;
            if (!isAddNew)
            {
                LibBillDataCache.Default.AddBillData(cacheKey, changeDataSet);
                this.DataSet = (DataSet)LibBillDataCache.Default.Get(cacheKey);
            }
            //else
            //{
            //    //新增时，DataSet未缓存，所以必须重新构造LibDynamicFildRelation
            //    LibEntryParam entryParam = new LibEntryParam();
            //    DataRow masterRow = changeDataSet.Tables[this.DataSet.Tables[0].TableName].Rows[0];
            //    foreach (string colName in this.Template.FuncPermission.EntryParam)
            //    {
            //        entryParam.ParamStore.Add(colName, masterRow[colName]);
            //    }
            //    this.CreateDynamicFieldRelation(entryParam);
            //}
            if (this.Template.FuncPermission.UsingDynamicColumn)
            {
                Dictionary<String, LibChangeRecord> changeRecord = DataSetManager.GetChangeRecord(changeDataSet);
                DataSetManager.ChangeDataHandle(this.DataSet, changeRecord, this.ManagerMessage, AddNewDynamicRow, ModifDynamicPk, ModifDynamicRow, DeleteDynamicRow);
                LibBillDataCache.Default.AddBillData(cacheKey, this.DataSet);
            }
            this.DoSaveCore(isAddNew, cacheKey, pks);
            return this.DataSet;
        }

        public DataSet Save(BillAction billAction, object[] pks, Dictionary<string, LibChangeRecord> changeRecord, Dictionary<string, string> extendParam = null)
        {
            try
            {
                this.BillAction = billAction;
                SetExtendBcfParam(extendParam);
                string cacheKey = GetDataSetCacheKey(pks);
                bool isAddNew = BillAction == BillAction.AddNew || BillAction == BillAction.SaveToDraft;
                if (!isAddNew)
                {
                    this.DataSet = (DataSet)LibBillDataCache.Default.Get(cacheKey);

                    if (this.Template.FuncPermission.UsingDynamicColumn && this.Template.FuncPermission.EntryParam != null && this.Template.FuncPermission.EntryParam.Count > 0)
                    {
                        bool createDynamicField = false;
                        foreach (DataTable t in this.DataSet.Tables)
                        {
                            if (t.ExtendedProperties.ContainsKey(TableProperty.DynamicFieldRelaion))
                            {
                                createDynamicField = true;
                                break;
                            }
                        }

                        if (!createDynamicField)
                        {
                            LibEntryParam entryParam = new LibEntryParam();
                            foreach (string colName in this.Template.FuncPermission.EntryParam)
                            {
                                entryParam.ParamStore.Add(colName, this.DataSet.Tables[0].Rows[0][colName]);
                            }
                            this.CreateDynamicFieldRelation(entryParam);
                        }
                    }

                }
                else
                {
                    //新增时，DataSet未缓存，所以必须重新构造LibDynamicFildRelation
                    LibEntryParam entryParam = new LibEntryParam();
                    Dictionary<string, object> masterDic = changeRecord[this.DataSet.Tables[0].TableName].Add[0];
                    foreach (string colName in this.Template.FuncPermission.EntryParam)
                    {
                        entryParam.ParamStore.Add(colName, masterDic[colName]);
                    }
                    this.CreateDynamicFieldRelation(entryParam);
                }
                if (this.DataSet != null)
                {
                    DataSetManager.ChangeDataHandle(this.DataSet, changeRecord, this.ManagerMessage, AddNewDynamicRow, ModifDynamicPk, ModifDynamicRow, DeleteDynamicRow);
                    LibBillDataCache.Default.AddBillData(cacheKey, this.DataSet);
                    this.DoSaveCore(isAddNew, cacheKey, pks);
                }
            }
            catch (Exception e)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("{0}", e));
                if (this.ProgId == "com.StructBom")
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(changeRecord);
                    string path = Path.Combine(EnvProvider.Default.MainPath, "Resource", "BOMBadness", string.Format("{0}.txt", LibDateUtils.GetCurrentDateTime()));
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            json = "操作员:" + Handle.UserId + "\r\n" + "错误信息:" + e.Message + "\r\n" + json;
                            sw.Write(json);
                        }
                    }
                }
            }
            return this.DataSet;
        }


        private void CheckTypeCache(DataRow masterRow, object[] pks)
        {
            string key = string.Empty;
            if (masterRow.Table.Columns.Contains("DEFAULTCREATESTATE") && masterRow.RowState == DataRowState.Modified)
            {
                string progId = this.Template.ProgId;
                if (pks.Length > 1)
                {
                    StringBuilder strBuilder = new StringBuilder();
                    foreach (var item in pks)
                    {
                        strBuilder.AppendFormat("/t{0}", item);
                    }
                    key = string.Format("{0}{1}", progId, strBuilder.ToString());
                }
                else
                    key = string.Format("{0}/t{1}", progId, pks[0]);

                Dictionary<string, object> destObj = LibParamCache.Default.Get<Dictionary<string, object>>(key);
                if (destObj != null)
                {
                    destObj["DEFAULTCREATESTATE"] = LibSysUtils.ToInt32(masterRow["DEFAULTCREATESTATE"]);
                    LibParamCache.Default.Set(key, destObj, new TimeSpan(0, 180, 0));
                }
            }
        }
        private void DoSaveCore(bool isAddNew, string cacheKey, object[] pks)
        {
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            if (isAddNew)
                AnewSetCodingNo(masterRow, false);
            SetSystemFieldValue(masterRow);
            LibDBTransaction trans = this.DataAccess.BeginTransaction();
            try
            {
                this.DataSet.CaseSensitive = true;
                CheckDataHelper.CheckData(this);
                this.BeforeUpdate();
                DataSetManager.SubmitData(this.DataSet, this.DataAccess);
                CheckTypeCache(masterRow, pks);
                this.UpdateApproveTask();
                if (this.BillAction == BillAction.ApprovePassRow || this.BillAction == BillAction.CancelApproveRow)
                    this.DealWithApproveRow(this.ApproveRowObj.Records, this.BillAction == BillAction.ApprovePassRow);
                this.AfterUpdate();
                if (this.ManagerMessage.IsThrow && this.BillAction != BillAction.SaveToDraft)
                {
                    trans.Rollback();
                    if (isAddNew)
                    {
                        ReturnCodingNo(masterRow);
                        this.DataSet.RejectChanges();
                    }
                    else
                    {
                        this.DataSet.RejectChanges();
                        LibBillDataCache.Default.AddBillData(cacheKey, this.DataSet);
                    }
                }
                else
                {
                    if (!isAddNew)
                    {
                        if (this.Template.FuncPermission.UsingCache)
                            LibParamCache.Default.RemoveCacheItem(this.ProgId, pks);
                        LibBillDataCache.Default.Remove(cacheKey);
                    }
                    trans.Commit();
                }
            }
            catch
            {
                trans.Rollback();
                if (!isAddNew)
                {
                    this.DataSet.RejectChanges();
                    LibBillDataCache.Default.AddBillData(cacheKey, this.DataSet);
                }
                if (isAddNew && this.DataSet.Tables[0].Rows.Count > 0)
                {
                    ReturnCodingNo(this.DataSet.Tables[0].Rows[0]);
                    this.DataSet.RejectChanges();
                }
                throw;
            }
            if (NeedSendNotice())
                SendNotice();
            this.AfterCommintData();
            this.AfterChangeData(this.DataSet);
            this.DataSet.AcceptChanges();
            if (this.Template.FuncPermission.UsingDynamicColumn)
            {
                DoFillDataDynamicTable();
            }
        }

        private bool NeedSendNotice()
        {
            return this.BillAction == BillAction.AuditPass || this.BillAction == BillAction.AuditUnPass || this.BillAction == BillAction.CancelAudit ||
                this.BillAction == BillAction.SubmitAudit || this.BillAction == BillAction.WithdrawAudit ||
                this.BillAction == BillAction.ApprovePassRow || this.BillAction == BillAction.ApproveUnPassRow ||
                this.BillAction == BillAction.CancelApproveRow || this.BillAction == BillAction.SubmitApproveRow ||
                this.BillAction == BillAction.WithdrawApproveRow;
        }

        #endregion

        #region [生效及取消生效]

        public DataSet TakeRelease(object[] pks, bool cancel, Dictionary<string, LibChangeRecord> changeRecord, Dictionary<string, string> extendParam = null)
        {
            string cacheKey = GetDataThenToCache(pks);
            try
            {
                bool check = true;
                if (cancel)
                {
                    this.BillAction = BillAction.CancelRelease;
                    check = this.CheckCancelRelease(pks);
                }
                else
                {
                    this.BillAction = BillAction.Release;
                    check = this.CheckRelease(pks);
                }
                if (check)
                {
                    this.SetSystemFieldValue(this.DataSet.Tables[0].Rows[0]);
                    this.Save(this.BillAction, pks, changeRecord, extendParam);
                }
            }
            finally
            {
                LibBillDataCache.Default.Remove(cacheKey);
            }
            return this.DataSet;
        }

        #endregion

        #region [作废及取消作废]

        public DataSet Invalid(object[] pks, bool cancel, Dictionary<string, LibChangeRecord> changeRecord, Dictionary<string, string> extendParam = null)
        {
            string cacheKey = GetDataThenToCache(pks);
            try
            {
                bool check = true;
                if (cancel)
                {
                    this.BillAction = BillAction.CancelInvalid;
                    check = this.CheckCancelInvalid(pks);
                }
                else
                {
                    this.BillAction = BillAction.Invalid;
                    check = this.CheckInvalid(pks);
                }
                if (check)
                {
                    this.SetSystemFieldValue(this.DataSet.Tables[0].Rows[0]);
                    this.Save(this.BillAction, pks, changeRecord, extendParam);
                }
            }
            finally
            {
                LibBillDataCache.Default.Remove(cacheKey);
            }
            return this.DataSet;
        }

        #endregion

        #region [结案及取消结案]

        public DataSet EndCase(object[] pks, bool cancel, Dictionary<string, LibChangeRecord> changeRecord, Dictionary<string, string> extendParam = null)
        {
            string cacheKey = GetDataThenToCache(pks);
            try
            {
                bool check = true;
                if (cancel)
                {
                    this.BillAction = BillAction.CancelEndCase;
                    check = this.CheckCancelEndCase(pks);
                }
                else
                {
                    this.BillAction = BillAction.EndCase;
                    check = this.CheckEndCase(pks);
                }
                if (check)
                {
                    this.SetSystemFieldValue(this.DataSet.Tables[0].Rows[0]);
                    this.Save(this.BillAction, pks, changeRecord, extendParam);
                }
            }
            finally
            {
                LibBillDataCache.Default.Remove(cacheKey);
            }
            return this.DataSet;
        }


        #endregion

        #region [草稿提交]

        public DataSet SubmitDraft(object[] pks, Dictionary<string, LibChangeRecord> changeRecord, Dictionary<string, string> extendParam = null)
        {
            this.BillAction = BillAction.SubmitDraft;
            string cacheKey = GetDataThenToCache(pks);
            try
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                this.SetSystemFieldValue(masterRow);
                this.Save(this.BillAction, pks, changeRecord, extendParam);
            }
            finally
            {
                LibBillDataCache.Default.Remove(cacheKey);
            }
            return this.DataSet;
        }

        #endregion

        #region 打印
        public virtual DataSet Print(string[] pks)
        {
            DataSet ds = BrowseTo(pks);
            return ds;
        }
        #endregion

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="pks"></param>
        public void RemoveCacheBillData(object[] pks)
        {
            string cacheKey = GetDataSetCacheKey(pks);
            LibBillDataCache.Default.Remove(cacheKey);
        }

        private string GetDataSetCacheKey(object[] pks)
        {
            if (pks == null)
                return string.Empty;
            StringBuilder key = new StringBuilder();
            foreach (var item in pks)
            {
                key.AppendFormat("/t{0}", item);
            }
            return string.Format("{0}{1}", this.ProgId, key);
        }

        private string GetDataThenToCache(object[] pks)
        {
            string key = GetDataSetCacheKey(pks);
            DataSetManager.GetDataSet(this.DataSet, this.DataAccess, this.ProgId, pks, this.Handle);
            this.AfterChangeData(this.DataSet);
            this.DataSet.AcceptChanges();
            LibBillDataCache.Default.AddBillData(key, this.DataSet);
            return key;
        }

        public override LibViewTemplate GetViewTemplate(LibEntryParam entryParam = null)
        {
            if (this.Template.FuncPermission.UsingDynamicColumn && entryParam != null)
                this.DataSet = this.ChangeTableStructure(entryParam);
            LibBillTpl tpl = (LibBillTpl)base.GetViewTemplate(entryParam);
            //tpl.ShowAuditState = this.UsingAudit;
            return tpl;
        }

        private void DoFillDataDynamicTable()
        {
            LibEntryParam entryParam = new LibEntryParam();
            if (this.DataSet.Tables[0].Rows.Count > 0)
            {
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                foreach (string colName in this.Template.FuncPermission.EntryParam)
                {
                    entryParam.ParamStore.Add(colName, masterRow[colName]);
                }
                DataSet dataSet = this.ChangeTableStructure(entryParam);
                dataSet = FillDataDynamicTable(dataSet);
                this.DataSet = dataSet;
            }
        }

        #region [单据审核相关]

        #region [提交审核、撤回]

        private void FillFlowPerson(IList<string> list, string levelCondition)
        {
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            string sql = string.Format("select distinct PersonId from AXPAPPROVETASK where PROGID='{0}' and INTERNALID='{1}' and FROMROWID=0 and {2}", this.ProgId, masterRow["INTERNALID"], levelCondition);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string personId = LibSysUtils.ToString(reader[0]);
                    if (!string.IsNullOrEmpty(personId))
                        list.Add(personId);
                }
            }
        }

        private string FillFlowPerson(IList<string> list, string levelCondition, int level)
        {
            StringBuilder builder = new StringBuilder();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            string sql = string.Format("select distinct A.PersonId,B.PersonName,A.FLOWLEVEL from AXPAPPROVETASK A left join COMPERSON B on B.PERSONID=A.PERSONID where A.PROGID='{0}' and A.INTERNALID='{1}' and A.FROMROWID=0 and {2}", this.ProgId, masterRow["INTERNALID"], levelCondition);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string personId = LibSysUtils.ToString(reader[0]);
                    if (level == reader.GetInt32(2))
                        builder.AppendFormat("{0},", reader[1]);
                    if (!string.IsNullOrEmpty(personId) && !list.Contains(personId))
                        list.Add(personId);
                }
            }
            if (builder.Length > 0)
                builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        private void FillRowFlowPerson(int rowId, IList<string> list, string levelCondition)
        {
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            string sql = string.Format("select distinct SUBMITTERID from AXPAPPROVETASK where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2} and {3}", this.ProgId, masterRow["INTERNALID"], rowId, levelCondition);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string personId = LibSysUtils.ToString(reader[0]);
                    if (!string.IsNullOrEmpty(personId))
                        list.Add(personId);
                }
            }
        }

        private string FillRowFlowPerson(int rowId, IList<string> list, string levelCondition, int level)
        {
            StringBuilder builder = new StringBuilder();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            string sql = string.Format("select distinct A.PersonId,B.PersonName,A.FLOWLEVEL from AXPAPPROVETASK A left join COMPERSON B on B.PERSONID=A.PERSONID where A.PROGID='{0}' and A.INTERNALID='{1}' and A.FROMROWID={2} and {3}", this.ProgId, masterRow["INTERNALID"], rowId, levelCondition);
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string personId = LibSysUtils.ToString(reader[0]);
                    if (level == reader.GetInt32(2))
                        builder.AppendFormat("{0},", reader[1]);
                    if (!string.IsNullOrEmpty(personId) && !list.Contains(personId))
                        list.Add(personId);
                }
            }
            if (builder.Length > 0)
                builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        private string GetApproveRowTempKey(List<string> personList)
        {
            StringBuilder builder = new StringBuilder();
            personList.Sort();
            foreach (var item in personList)
            {
                builder.Append(item);
            }
            return builder.ToString();
        }

        public DataSet SubmitAudit(object[] pks, bool cancel, Dictionary<string, LibChangeRecord> changeRecord, Dictionary<string, string> extendParam = null)
        {
            ChatConnection chatConnection = new ChatConnection();
            string cacheKey = GetDataThenToCache(pks);
            try
            {
                string tableName = this.DataSet.Tables[0].TableName;
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                // 获取主键值
                string billNo = LibSysUtils.ToString(masterRow[this.DataSet.Tables[0].PrimaryKey[0]]);
                changeRecord = new Dictionary<string, LibChangeRecord>();
                changeRecord.Add(tableName, new LibChangeRecord());
                LibChangeRecord curChangeRecord = changeRecord[tableName];
                bool check = true;
                if (cancel)
                {
                    this.BillAction = BillAction.WithdrawAudit;
                    check = CheckWithdrawAudit(pks);
                    if (check)
                    {
                        Dictionary<string, object> temp = new Dictionary<string, object>();
                        foreach (DataColumn col in this.DataSet.Tables[0].PrimaryKey)
                        {
                            temp.Add(string.Format("_{0}", col.ColumnName), masterRow[col]);
                        }
                        temp.Add("AUDITSTATE", LibAuditState.UnSubmit);
                        curChangeRecord.Modif.Add(temp);
                        int curLevel = LibSysUtils.ToInt32(masterRow["FLOWLEVEL"]);
                        if (curLevel == 0)
                        {
                            this.UpdateTaskSqlList.Add(string.Format("delete AXPAPPROVETASK where PROGID='{0}' and INTERNALID='{1}' and FROMROWID=0", this.ProgId, masterRow["INTERNALID"]));
                        }
                        this.FillFlowPerson(this.ApproveMailParam.To, string.Format("(FLOWLEVEL={0} or FLOWLEVEL={1})", curLevel + 1, curLevel));
                        string keyStr = LibSysUtils.ToString(GetLastPkValue(masterRow));
                        string text = string.Format("《{0}》{1}已撤回", this.Template.DisplayText, keyStr);
                        this.ApproveMailParam.BillNo = keyStr;
                        this.ApproveMailParam.Subject = text;
                        this.ApproveMailParam.Content = text;
                    }
                }
                else
                {
                    this.BillAction = BillAction.SubmitAudit;
                    check = CheckSubmitAudit(pks);
                    if (check)
                    {
                        Dictionary<string, object> temp = new Dictionary<string, object>();
                        foreach (DataColumn col in this.DataSet.Tables[0].PrimaryKey)
                        {
                            temp.Add(string.Format("_{0}", col.ColumnName), masterRow[col]);
                        }
                        temp.Add("AUDITSTATE", LibAuditState.Submit);
                        curChangeRecord.Modif.Add(temp);
                        //任务表里不存在则新增
                        string internalId = LibSysUtils.ToString(masterRow["INTERNALID"]);
                        string sql = string.Format("select PERSONID,FLOWLEVEL from AXPAPPROVETASK where PROGID='{0}' and INTERNALID='{1}' and FROMROWID=0", this.ProgId, internalId);
                        int curLevel = LibSysUtils.ToInt32(masterRow["FLOWLEVEL"]);
                        bool existData = false;
                        using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                        {
                            while (reader.Read())
                            {
                                if (!existData)
                                    existData = true;
                                int flowLevel = reader.GetInt32(1);
                                string personId = reader.GetString(0);
                                if ((flowLevel == curLevel || flowLevel == curLevel + 1) && !string.IsNullOrEmpty(personId))
                                {
                                    ApproveMailParam.To.Add(personId);
                                    MessageInfo messageInfo = new MessageInfo(personId, this.Handle.PersonId + "向你提交了待审核内容");
                                    chatConnection.SendMessage(messageInfo);
                                }
                            }
                        }
                        //检查是否全是跳过的行
                        bool isFoundNotJump = false;
                        if (!existData)
                        {
                            string errorInfo = string.Empty;
                            SortedList<int, List<LibApproveFlowInfo>> flowList = GetApproveFlowForBillByDataRow(masterRow, out errorInfo);
                            if (flowList == null)
                            {
                                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("查找到的审核过程为空，原因:{0}", errorInfo));
                            }
                            else
                            {
                                LibApproveFlowInfo firstNotJumpFlowInfo = null;   //第一个非跳过审核过程    
                                foreach (var flowInfo in flowList)
                                {
                                    foreach (var info in flowInfo.Value)
                                    {
                                        if (info.IsJump == false)
                                        {
                                            firstNotJumpFlowInfo = info;
                                            isFoundNotJump = true;
                                            break;
                                        }
                                        MessageInfo messageInfo = new MessageInfo(info.PersonId, this.Handle.PersonId + "向你提交了待审核内容");
                                        chatConnection.SendMessage(messageInfo);
                                    }
                                    if (isFoundNotJump)
                                        break;
                                }
                                if (isFoundNotJump == false)
                                {
                                    //全是跳过行
                                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("本此单据的提交审核，全是跳过的审核过程。请检查审核流配置。"));
                                }
                                else
                                {
                                    foreach (var flowInfo in flowList)
                                    {
                                        foreach (var info in flowInfo.Value)
                                        {
                                            if (HasAduitOfDuty == false)
                                            {
                                                this.UpdateTaskSqlList.Add(string.Format("insert into AXPAPPROVETASK(PROGID,INTERNALID,FROMROWID,CURRENTLEVEL,FLOWLEVEL,SUBMITTERID,PERSONID,INDEPENDENT,AUDITSTATE,BILLNO) "
                                                     + "values('{0}','{1}',{2},{3},{4},'{5}','{6}',{7},0,'{8}')", this.ProgId, internalId, 0, 0, flowInfo.Key, this.Handle.PersonId, info.PersonId, info.Independent ? 1 : 0, billNo));
                                            }
                                            else
                                            {
                                                //插入更多字段 部门Id、岗位Id、是否跳过、跳过原因
                                                this.UpdateTaskSqlList.Add(string.Format(
                                                    "insert into AXPAPPROVETASK(AUDITTASKID,PROGID,INTERNALID,FROMROWID,CURRENTLEVEL,FLOWLEVEL,SUBMITTERID,PERSONID,INDEPENDENT,AUDITSTATE,BILLNO,DEPTID,DUTYID,ISJUMP,JUMPREASON,EXECUTEDESC) "
                                                     + "values('{14}','{0}','{1}',{2},{3},{4},'{5}','{6}',{7},0,'{8}', " +
                                                     " '{9}','{10}','{11}','{12}',{13})",
                                                    this.ProgId, internalId, 0, 0, flowInfo.Key, this.Handle.PersonId, info.PersonId, info.Independent ? 1 : 0, billNo,
                                                    info.DeptId, info.DutyId, info.IsJump, info.JumpReason, LibStringBuilder.GetQuotString(string.Format("{0}", info.ExecuteDesc)),
                                                    Guid.NewGuid().ToString() //使用Guid作为主键
                                                    ));
                                            }
                                            if ((flowInfo.Key == curLevel || flowInfo.Key == curLevel + 1) && !string.IsNullOrEmpty(info.PersonId))
                                            {
                                                ApproveMailParam.To.Add(info.PersonId);
                                            }
                                            if (firstNotJumpFlowInfo != null && string.IsNullOrEmpty(firstNotJumpFlowInfo.PersonId) == false)
                                            {
                                                //对第一个非跳过审核过程的执行人发送通知
                                                if (ApproveMailParam.To.Contains(firstNotJumpFlowInfo.PersonId) == false)
                                                    ApproveMailParam.To.Add(firstNotJumpFlowInfo.PersonId);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //// 修改当前审核级别之后的审核状态为0 //审核不通过时已处理
                            //this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AUDITSTATE=0 where PROGID='{0}' and INTERNALID='{1}' and FROMROWID=0 and FLOWLEVEL>{2}", this.ProgId, internalId, curLevel));

                            // 修改 SUBMITTERID 和 BILLNO
                            this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set SUBMITTERID='{0}', BILLNO='{1}' where PROGID='{2}' and INTERNALID='{3}' and FROMROWID=0", this.Handle.PersonId, billNo, this.ProgId, internalId));
                        }
                        if (isFoundNotJump == true || existData == true)
                        {
                            string keyStr = LibSysUtils.ToString(GetLastPkValue(masterRow));
                            string text = string.Format("《{0}》{1}已提交审核", this.Template.DisplayText, keyStr);
                            this.ApproveMailParam.BillNo = keyStr;
                            this.ApproveMailParam.Subject = text;
                            this.ApproveMailParam.Content = text;
                        }
                    }
                }
                if (check)
                {
                    this.SetSystemFieldValue(this.DataSet.Tables[0].Rows[0]);
                    this.Save(this.BillAction, pks, changeRecord, extendParam);
                }
            }
            finally
            {
                LibBillDataCache.Default.Remove(cacheKey);
            }
            return this.DataSet;
        }


        #endregion

        #region [审核、弃审]


        public DataSet Audit(object[] pks, bool isPass, string opinion, Dictionary<string, LibChangeRecord> changeRecord, int downLevel = -1, Dictionary<string, string> extendParam = null)
        {
            ChatConnection chatConnection = new ChatConnection();
            this.BillAction = isPass ? BillAction.AuditPass : BillAction.AuditUnPass;
            string cacheKey = GetDataThenToCache(pks);
            try
            {
                bool check = true;
                string tableName = this.DataSet.Tables[0].TableName;
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                changeRecord = new Dictionary<string, LibChangeRecord>();
                changeRecord.Add(tableName, new LibChangeRecord());
                LibChangeRecord curChangeRecord = changeRecord[tableName];
                string curPersonId = this.Handle.PersonId;
                string internalId = LibSysUtils.ToString(masterRow["INTERNALID"]);
                check = CheckAudit(pks);
                if (check)
                {
                    Dictionary<string, object> temp = new Dictionary<string, object>();
                    foreach (DataColumn col in this.DataSet.Tables[0].PrimaryKey)
                    {
                        temp.Add(string.Format("_{0}", col.ColumnName), masterRow[col]);
                    }
                    //从主表的数据行中取出审核层级的数据。初始时为0
                    int curLevel = LibSysUtils.ToInt32(masterRow["FLOWLEVEL"]);
                    bool isFind = false;
                    string errorInfo = string.Empty;
                    SortedList<int, List<LibApproveFlowInfo>> flowList = this.GetApproveFlowForBillByDataRow(masterRow, out errorInfo);
                    LibApproveFlowInfo theFoundFlowInfo = null;//被找到的审核过程。
                    if (flowList == null)
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("查找到的审核过程为空，原因:{0}", errorInfo));
                    }
                    else
                    {
                        //首先处理一个级别下只有跳过的审核过程的情况。因提交审核时已过滤了全是跳过行的情况，此处不用再考虑
                        bool isFoundNotJumpRow = false;
                        while (isFoundNotJumpRow == false)
                        {
                            foreach (var flowInfo in flowList[(curLevel + 1)])
                            {
                                if (flowInfo.IsJump == false)
                                {
                                    isFoundNotJumpRow = true;
                                    break;
                                }
                            }
                            if (isFoundNotJumpRow)
                                break;
                            if (flowList.Keys[flowList.Keys.Count - 1] == curLevel + 1)
                                break;//已经是最后一级
                            curLevel++;
                        }
                        foreach (var flowInfo in flowList[(curLevel + 1)])
                        {
                            if (string.Compare(flowInfo.PersonId, curPersonId, true) == 0)
                            {
                                isFind = true;
                                theFoundFlowInfo = flowInfo;
                                flowInfo.IsPass = isPass;
                                flowInfo.AuditState = isPass ? 1 : 2;
                                if (HasAduitOfDuty == false)
                                {
                                    this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AuditState={5},AUDITOPINION='{6}' where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2} and CURRENTLEVEL={3} and PERSONID='{4}'",
                                        this.ProgId, internalId, 0, curLevel, flowInfo.PersonId, flowInfo.AuditState, opinion));
                                }
                                else
                                {
                                    this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AuditState={1},AUDITOPINION='{2}' where AUDITTASKID='{0}'",
                                            flowInfo.AuditTaskId, flowInfo.AuditState, opinion));
                                }
                                break;
                            }
                        }
                    }
                    if (isFind)
                    {
                        bool isLastLevel = false;
                        string keyStr = LibSysUtils.ToString(GetLastPkValue(masterRow));
                        if (isPass)
                        {
                            //通过                          
                            bool isUpLevel = IsUpLevel(curLevel + 1, flowList);//判断是否进行到下一层级
                            if (isUpLevel)
                            {
                                int upLevel = curLevel + 1;
                                temp.Add("FLOWLEVEL", upLevel);
                                isLastLevel = flowList.Keys[flowList.Keys.Count - 1] == upLevel;
                                if (isLastLevel)
                                {
                                    temp.Add("AUDITSTATE", LibAuditState.Pass);
                                    if ((LibCurrentState)masterRow["CURRENTSTATE"] == LibCurrentState.UnRelease)
                                    {
                                        temp.Add("CURRENTSTATE", (int)LibCurrentState.Release);
                                    }
                                }
                                else
                                {
                                    while (IsUpLevel(upLevel + 1, flowList))//因后面的行可能是跳过行，先进行预先检查
                                    {
                                        upLevel++;
                                        isLastLevel = flowList.Keys[flowList.Keys.Count - 1] == upLevel;
                                        if (isLastLevel)
                                            break;
                                    }
                                    if ((LibAuditState)LibSysUtils.ToInt32(masterRow["AUDITSTATE"]) != LibAuditState.Submit)
                                        temp.Add("AUDITSTATE", LibAuditState.Submit);
                                }
                                if (isLastLevel)
                                {
                                    //再判断一次是否为最后一层
                                    if (temp.ContainsKey("AUDITSTATE") == false)
                                        temp.Add("AUDITSTATE", LibAuditState.Pass);
                                    else
                                    {
                                        temp["AUDITSTATE"] = LibAuditState.Pass;
                                    }
                                    if ((LibCurrentState)masterRow["CURRENTSTATE"] == LibCurrentState.UnRelease)
                                    {
                                        if (temp.ContainsKey("CURRENTSTATE") == false)
                                            temp.Add("CURRENTSTATE", (int)LibCurrentState.Release);
                                        else
                                            temp["CURRENTSTATE"] = (int)LibCurrentState.Release;
                                    }
                                }
                                this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set CURRENTLEVEL={2} where  PROGID='{0}' and INTERNALID='{1}' and FROMROWID=0", this.ProgId, internalId, upLevel));

                                string text = string.Empty;
                                if (isLastLevel)
                                {
                                    this.FillFlowPerson(ApproveMailParam.To, "FLOWLEVEL>0");
                                    text = string.Format("《{0}》{1}已审核通过", this.Template.DisplayText, keyStr);
                                }
                                else
                                {
                                    string flowLevelConditon = string.Empty;
                                    if (HasAduitOfDuty == false)
                                    {
                                        flowLevelConditon = string.Format("(A.FLOWLEVEL={0} or A.FLOWLEVEL={1} or A.FLOWLEVEL={2})", upLevel, curLevel, upLevel + 1);
                                    }
                                    else
                                    {
                                        flowLevelConditon = string.Format("(A.FLOWLEVEL={0} or A.FLOWLEVEL={1} or A.FLOWLEVEL={2}) and A.PERSONID != '{3}'", upLevel, curLevel, upLevel + 1, curPersonId);
                                    }
                                    string nextPersonStr = FillFlowPerson(ApproveMailParam.To, flowLevelConditon, upLevel + 1);
                                    text = string.Format("《{0}》{1}已在审核层级{2}上审核通过,请{3}进行第{4}层级的审核", this.Template.DisplayText, keyStr, upLevel, nextPersonStr, upLevel + 1);
                                }

                                if (isLastLevel)
                                {
                                    string creatorId = LibSysUtils.ToString(masterRow["CREATORID"]);
                                    if (!string.IsNullOrEmpty(creatorId) && !ApproveMailParam.To.Contains(creatorId))
                                        ApproveMailParam.To.Add(creatorId);
                                    MessageInfo messageInfo = new MessageInfo(creatorId, text);
                                    chatConnection.SendMessage(messageInfo);

                                }
                                this.ApproveMailParam.BillNo = keyStr;
                                this.ApproveMailParam.Subject = text;
                                this.ApproveMailParam.Content = text;
                            }
                            //同人默认的操作，同一个审核人如果在本步骤有同意的意见，则后续都是同意的 Zhangkj 20170327           
                            #region 同人默认的相关处理 Zhangkj 20170327
                            if (HasAduitOfDuty && isLastLevel == false)//必须是有部门岗位设置相关的字段时
                            {
                                //如果设置了同人默认，首先在内存中设置大于本级的同一个人的审核审核都同意
                                bool findOtherSamePersonRow = false;
                                foreach (var level in flowList.Keys)
                                {
                                    foreach (var flowInfo in flowList[level])
                                    {
                                        if (string.Compare(flowInfo.PersonId, curPersonId, true) == 0
                                            && flowInfo.AuditState != (int)LibAuditState.Pass
                                            && flowInfo.FlowLevel >= curLevel
                                            && flowInfo.IsSameDefault
                                            && flowInfo.IsJump == false
                                            && flowInfo != theFoundFlowInfo)
                                        {
                                            findOtherSamePersonRow = true;
                                            flowInfo.IsPass = isPass;
                                            flowInfo.AuditState = isPass ? 1 : 2;
                                            //this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AuditState={5},AUDITOPINION='{6}',EXECUTEDESC='{7}' " +
                                            //    " where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2} and CURRENTLEVEL={3} and PERSONID='{4}'",
                                            //    this.ProgId, internalId, 0, flowInfo.FlowLevel, flowInfo.PersonId, flowInfo.AuditState, opinion, "同人默认"));//同人默认
                                            this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AuditState={1},AUDITOPINION='{2}',EXECUTEDESC='{3}' " +
                                                " where AUDITTASKID='{0}'",
                                                flowInfo.AuditTaskId, flowInfo.AuditState, opinion, "同人默认"));//同人默认
                                        }
                                    }
                                }
                                if (findOtherSamePersonRow)//通过同人默认设置了一些行的状态
                                {
                                    if (isUpLevel)
                                        curLevel++;//如果需要晋级则判断下一层
                                    //查找如果执行了同人默认，单据的下一审核层级和审核状态应该是什么                          
                                    int maxUpLevel = int.MinValue;
                                    string text = string.Empty;
                                    while (true)
                                    {
                                        isUpLevel = IsUpLevel(curLevel + 1, flowList);
                                        if (isUpLevel)
                                        {
                                            int thisUpLevel = curLevel + 1;
                                            if (thisUpLevel > maxUpLevel)
                                                maxUpLevel = thisUpLevel;
                                            if (temp.ContainsKey("FLOWLEVEL") == false)
                                                temp.Add("FLOWLEVEL", thisUpLevel);
                                            else
                                                temp["FLOWLEVEL"] = thisUpLevel;
                                            isLastLevel = flowList.Keys[flowList.Keys.Count - 1] == thisUpLevel;

                                            if (isLastLevel)
                                            {
                                                if (temp.ContainsKey("AUDITSTATE") == false)
                                                    temp.Add("AUDITSTATE", LibAuditState.Pass);
                                                else
                                                    temp["AUDITSTATE"] = LibAuditState.Pass;
                                                if ((LibCurrentState)masterRow["CURRENTSTATE"] == LibCurrentState.UnRelease)
                                                {
                                                    if (temp.ContainsKey("CURRENTSTATE") == false)
                                                        temp.Add("CURRENTSTATE", (int)LibCurrentState.Release);
                                                    else
                                                        temp["CURRENTSTATE"] = (int)LibCurrentState.Release;
                                                }

                                                this.FillFlowPerson(ApproveMailParam.To, "FLOWLEVEL>0");
                                                text = string.Format("《{0}》{1}已审核通过。", this.Template.DisplayText, keyStr);

                                                string creatorId = LibSysUtils.ToString(masterRow["CREATORID"]);
                                                if (!string.IsNullOrEmpty(creatorId) && !ApproveMailParam.To.Contains(creatorId))
                                                    ApproveMailParam.To.Add(creatorId);
                                                break;//是最后一层了，终止循环
                                            }
                                            else
                                            {
                                                if ((LibAuditState)LibSysUtils.ToInt32(masterRow["AUDITSTATE"]) != LibAuditState.Submit)
                                                {
                                                    if (temp.ContainsKey("AUDITSTATE") == false)
                                                        temp.Add("AUDITSTATE", LibAuditState.Submit);
                                                    else
                                                        temp["AUDITSTATE"] = LibAuditState.Submit;
                                                }
                                                if (IsUpLevel(thisUpLevel + 1, flowList) == false)
                                                {
                                                    string flowLevelConditon = string.Format("(A.FLOWLEVEL={0} or A.FLOWLEVEL={1} or A.FLOWLEVEL={2}) and A.PERSONID != '{3}'", thisUpLevel, curLevel, thisUpLevel + 1, curPersonId);//对于当前用户就不再通知了
                                                    string nextPersonStr = FillFlowPerson(ApproveMailParam.To, flowLevelConditon, thisUpLevel + 1);
                                                    text = string.Format("《{0}》{1}已在审核层级{2}上审核通过,请{3}进行第{4}层级的审核。", this.Template.DisplayText, keyStr, thisUpLevel, nextPersonStr, thisUpLevel + 1);
                                                    break;//下一层有需要审核的人员
                                                }
                                                curLevel++;//如果需要进入下一层循环，而且又不是最后一层，则继续进入下一层检查。
                                            }
                                        }
                                        else
                                        {
                                            break;//不能进入下一层审核，则终止循环
                                        }
                                    }
                                    if (maxUpLevel != int.MinValue)
                                        this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set CURRENTLEVEL={2} where  PROGID='{0}' and INTERNALID='{1}' and FROMROWID=0", this.ProgId, internalId, maxUpLevel));
                                    if (string.IsNullOrEmpty(text) == false)
                                    {
                                        this.ApproveMailParam.Subject = text;
                                        this.ApproveMailParam.Content = text;
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            //不通过
                            temp.Add("AUDITSTATE", LibAuditState.UnPass);
                            if (downLevel == -1)
                                downLevel = curLevel;//-1 表示退回当前审核层级的上一层级。
                            if (curLevel < downLevel)
                            {
                                if (check)
                                    check = false;
                                this.ManagerMessage.AddMessage(LibMessageKind.Error, "退回层级大于当前审核层级。");
                            }
                            else
                            {
                                temp.Add("FLOWLEVEL", downLevel);
                                this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set CURRENTLEVEL={3} where  PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2}", this.ProgId, internalId, 0, downLevel));
                                //Zhangkj 20170327 还原在审核不通过时修改之前的审核状态，同时清空审核意见
                                this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AUDITSTATE=0,AUDITOPINION='' where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2} and FLOWLEVEL>{3}",
                                    this.ProgId, internalId, 0, downLevel));

                                if (HasAduitOfDuty == false)
                                {
                                    this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AUDITOPINION='{5}' where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2} and CURRENTLEVEL={3} and PERSONID='{4}'",
                                         this.ProgId, internalId, 0, curLevel, curPersonId, opinion));
                                }
                                else
                                {
                                    this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AUDITOPINION='{1}' where AUDITTASKID='{0}'",
                                         theFoundFlowInfo.AuditTaskId, opinion));
                                }


                                string flowLevelConditon = string.Format("(FLOWLEVEL<={0} and FLOWLEVEL>={1})", curLevel + 1, downLevel);
                                string text = string.Format("《{0}》{1}已在审核层级{2}上审核不通过", this.Template.DisplayText, keyStr, curLevel + 1);
                                this.FillFlowPerson(ApproveMailParam.To, flowLevelConditon);
                                if (downLevel == 0)
                                {
                                    string creatorId = LibSysUtils.ToString(masterRow["CREATORID"]);
                                    if (!string.IsNullOrEmpty(creatorId) && !ApproveMailParam.To.Contains(creatorId))
                                    {
                                        ApproveMailParam.To.Add(creatorId);
                                        MessageInfo messageInfo = new MessageInfo(creatorId, text);
                                        chatConnection.SendMessage(messageInfo);
                                    }
                                }
                                this.ApproveMailParam.BillNo = keyStr;
                                this.ApproveMailParam.Subject = text;
                                this.ApproveMailParam.Content = text;
                            }
                        }
                        curChangeRecord.Modif.Add(temp);

                    }
                    else
                    {
                        if (check)
                            check = false;
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "对于单据在所处的审核阶段，当前人员没有审核权限。");
                    }
                }
                if (check)
                {
                    this.SetSystemFieldValue(this.DataSet.Tables[0].Rows[0]);
                    this.Save(this.BillAction, pks, changeRecord, extendParam);
                }
            }
            finally
            {
                LibBillDataCache.Default.Remove(cacheKey);
            }
            return this.DataSet;
        }

        public DataSet CancelAudit(object[] pks, Dictionary<string, LibChangeRecord> changeRecord, int downLevel = -1, string reasonId = "", Dictionary<string, string> extendParam = null)
        {
            this.BillAction = BillAction.CancelAudit;
            string cacheKey = GetDataThenToCache(pks);
            try
            {
                string curPersonId = this.Handle.PersonId;
                string tableName = this.DataSet.Tables[0].TableName;
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                changeRecord = new Dictionary<string, LibChangeRecord>();
                changeRecord.Add(tableName, new LibChangeRecord());
                LibChangeRecord curChangeRecord = changeRecord[tableName];
                string internalId = LibSysUtils.ToString(masterRow["INTERNALID"]);
                bool check = CheckCancelAudit(pks);
                if (check)
                {
                    int curLevel = LibSysUtils.ToInt32(masterRow["FLOWLEVEL"]);
                    bool isFind = false;
                    string errorInfo = string.Empty;
                    SortedList<int, List<LibApproveFlowInfo>> flowList = GetApproveFlowForBillByDataRow(masterRow, out errorInfo);
                    if (flowList == null)
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("查找到的审核过程为空，原因:{0}", errorInfo));
                    }
                    else
                    {
                        foreach (var flowInfo in flowList[curLevel])
                        {
                            if (string.Compare(flowInfo.PersonId, curPersonId, true) == 0)
                            {
                                isFind = true;
                                break;
                            }
                        }
                    }
                    if (isFind)
                    {
                        Dictionary<string, object> temp = new Dictionary<string, object>();
                        foreach (DataColumn col in this.DataSet.Tables[0].PrimaryKey)
                        {
                            temp.Add(string.Format("_{0}", col.ColumnName), masterRow[col]);
                        }
                        if ((LibAuditState)LibSysUtils.ToInt32(masterRow["AUDITSTATE"]) == LibAuditState.Pass)
                        {
                            if (!string.IsNullOrEmpty(reasonId))
                                this.ChangeReasonId = reasonId;
                            Int64 defaultCreateState = 0;
                            if (this.Template.BillType == BillType.Bill)
                                defaultCreateState = (Int64)LibParamCache.Default.GetValueByName(this.Template.FuncPermission.BillTypeName, new object[] { masterRow["TYPEID"] }, "DEFAULTCREATESTATE");
                            int currentState = defaultCreateState == 0 && !LibSysUtils.ToBoolean(masterRow["ISUSED"]) ? (int)LibCurrentState.UnRelease : (int)LibCurrentState.Release;
                            temp.Add("CURRENTSTATE", currentState);
                        }
                        temp.Add("AUDITSTATE", LibAuditState.UnPass);
                        downLevel = -1;
                        if (downLevel == -1)
                            if (curLevel != 0)
                            {
                                downLevel = curLevel - 1;
                            }
                            else
                            {
                                downLevel = 0;
                            }

                        temp.Add("FLOWLEVEL", downLevel);
                        this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set CURRENTLEVEL={3} where  PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2}", this.ProgId, internalId, 0, downLevel));
                        this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AUDITSTATE=0 where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2} and FLOWLEVEL>{3}",
                            this.ProgId, internalId, 0, downLevel));
                        curChangeRecord.Modif.Add(temp);

                        string keyStr = LibSysUtils.ToString(GetLastPkValue(masterRow));
                        string flowLevelConditon = string.Format("(FLOWLEVEL<={0} and FLOWLEVEL>={1})", curLevel, downLevel);
                        string text = string.Format("《{0}》{1}已在审核层级{2}上弃审退回", this.Template.DisplayText, keyStr, curLevel);
                        this.FillFlowPerson(ApproveMailParam.To, flowLevelConditon);
                        if (downLevel == 0)
                        {
                            string creatorId = LibSysUtils.ToString(masterRow["CREATORID"]);
                            if (!string.IsNullOrEmpty(creatorId) && !ApproveMailParam.To.Contains(creatorId))
                            {
                                ApproveMailParam.To.Add(creatorId);
                            }
                        }
                        this.ApproveMailParam.BillNo = keyStr;
                        this.ApproveMailParam.Subject = text;
                        this.ApproveMailParam.Content = text;
                    }
                    else
                    {
                        if (check)
                            check = false;
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前人员没有弃审权限。");
                    }
                }
                if (check)
                {
                    this.SetSystemFieldValue(this.DataSet.Tables[0].Rows[0]);
                    this.Save(this.BillAction, pks, changeRecord, null);
                }
            }
            finally
            {
                LibBillDataCache.Default.Remove(cacheKey);
            }
            return this.DataSet;
        }

        #endregion

        private object GetLastPkValue(DataRow curRow)
        {
            if (curRow.Table.PrimaryKey.Length > 0)
                return curRow[curRow.Table.PrimaryKey[curRow.Table.PrimaryKey.Length - 1]];
            else
                return null;
        }

        private object[] GetPkCondition(DataRow curRow, int rowId)
        {
            DataTable curTable = curRow.Table;
            int len = curTable.PrimaryKey.Length;
            object[] pkList = new object[len + 1];
            for (int i = 0; i < len; i++)
            {
                pkList[i] = curRow[curTable.PrimaryKey[i]];
            }
            pkList[len] = rowId;
            return pkList;
        }
        /// <summary>
        /// 从单据的审核任务进度表中查找单据对应的审核条目
        /// </summary>
        /// <param name="masterRow"></param>
        /// <param name="errorInfo"></param>
        /// <param name="showConfig"></param>
        /// <returns></returns>
        private SortedList<int, List<LibApproveFlowInfo>> GetApproveFlowForBillByDataRow(DataRow masterRow, out string errorInfo, bool showConfig = false)
        {
            errorInfo = string.Empty;
            SortedList<int, List<LibApproveFlowInfo>> flowList = null;
            string internalId = LibSysUtils.ToString(masterRow["INTERNALID"]);
            flowList = GetApproveFlowFromDB(internalId);
            if (flowList == null)
            {
                flowList = GetDefaultFlowByDataRow(masterRow, out errorInfo, showConfig);
            }
            return flowList;
        }

        public SortedList<int, List<LibApproveFlowInfo>> GetApproveFlowForBill(Dictionary<string, object> masterDic, bool showConfig = false)
        {
            SortedList<int, List<LibApproveFlowInfo>> flowList = null;
            string internalId = LibSysUtils.ToString(masterDic["INTERNALID"]);
            if (showConfig == false)
            {
                flowList = GetApproveFlowFromDB(internalId);
            }
            if (showConfig == true || flowList == null || string.IsNullOrEmpty(internalId))
            {
                flowList = GetDefaultFlow(masterDic, showConfig);
            }
            return flowList;
        }
        /// <summary>
        /// 根据单据的内码，查找单据的审核过程信息
        /// </summary>
        /// <param name="internalId"></param>
        /// <returns></returns>
        private SortedList<int, List<LibApproveFlowInfo>> GetApproveFlowFromDB(string internalId)
        {
            SortedList<int, List<LibApproveFlowInfo>> flowList = null;
            if (HasAduitOfDuty == false)
            {
                string sql = string.Format("select A.FLOWLEVEL,A.SUBMITTERID,A.PERSONID,B.PERSONNAME," +
                                      "B.POSITION,A.INDEPENDENT,A.AUDITSTATE,A.AUDITOPINION  from AXPAPPROVETASK A inner join COMPERSON B on " +
                                      "B.PERSONID=A.PERSONID where A.PROGID={0} and A.INTERNALID={1} and A.FROMROWID=0 ORDER BY A.FROMROWID,A.FLOWLEVEL",
                                      LibStringBuilder.GetQuotString(this.ProgId), LibStringBuilder.GetQuotString(internalId));
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        if (flowList == null)
                            flowList = new SortedList<int, List<LibApproveFlowInfo>>();
                        int flowLevel = LibSysUtils.ToInt32(reader["FLOWLEVEL"]);
                        if (!flowList.ContainsKey(flowLevel))
                            flowList.Add(flowLevel, new List<LibApproveFlowInfo>());
                        List<LibApproveFlowInfo> infoList = flowList[flowLevel];
                        int auditState = LibSysUtils.ToInt32(reader["AUDITSTATE"]);
                        infoList.Add(new LibApproveFlowInfo()
                        {
                            SubmitterId = LibSysUtils.ToString(reader["SUBMITTERID"]),
                            PersonId = LibSysUtils.ToString(reader["PERSONID"]),
                            PersonName = LibSysUtils.ToString(reader["PERSONNAME"]),
                            Position = LibSysUtils.ToString(reader["POSITION"]),
                            Independent = LibSysUtils.ToBoolean(reader["INDEPENDENT"]),
                            AuditOpinion = LibSysUtils.ToString(reader["AUDITOPINION"]),
                            AuditState = auditState,
                            IsPass = auditState == 1
                        });
                    }
                }
            }
            else
            {
                //有按部门岗位审核的相关字段
                string sql = string.Format("select A.AUDITTASKID, A.FLOWLEVEL,A.SUBMITTERID,A.PERSONID,B.PERSONNAME," +
                                      " B.POSITION,A.INDEPENDENT,A.AUDITSTATE,A.AUDITOPINION," +
                                      " A.DEPTID,C.DEPTNAME,A.DUTYID,D.DUTYNAME,A.ISJUMP,A.JUMPREASON,A.ISSAMEDEFAULT,A.EXECUTEDESC " +
                                      " from AXPAPPROVETASK A " +
                                      " left join COMPERSON B on  B.PERSONID=A.PERSONID " +//left join 具体审核人可能为空（可能会跳过）
                                      " left join COMDEPT C on C.DEPTID = A.DEPTID " +
                                      " left join COMDUTY D on D.DUTYID = A.DUTYID " +
                                      " where A.PROGID={0} and A.INTERNALID={1} and A.FROMROWID=0 ORDER BY A.FROMROWID,A.FLOWLEVEL",
                                      LibStringBuilder.GetQuotString(this.ProgId), LibStringBuilder.GetQuotString(internalId));
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        if (flowList == null)
                            flowList = new SortedList<int, List<LibApproveFlowInfo>>();
                        int flowLevel = LibSysUtils.ToInt32(reader["FLOWLEVEL"]);
                        if (!flowList.ContainsKey(flowLevel))
                            flowList.Add(flowLevel, new List<LibApproveFlowInfo>());
                        List<LibApproveFlowInfo> infoList = flowList[flowLevel];
                        int auditState = LibSysUtils.ToInt32(reader["AUDITSTATE"]);
                        infoList.Add(new LibApproveFlowInfo()
                        {
                            SubmitterId = LibSysUtils.ToString(reader["SUBMITTERID"]),
                            PersonId = LibSysUtils.ToString(reader["PERSONID"]),
                            PersonName = LibSysUtils.ToString(reader["PERSONNAME"]),
                            Position = LibSysUtils.ToString(reader["POSITION"]),
                            Independent = LibSysUtils.ToBoolean(reader["INDEPENDENT"]),
                            AuditOpinion = LibSysUtils.ToString(reader["AUDITOPINION"]),
                            AuditState = auditState,
                            IsPass = auditState == 1,

                            FlowLevel = flowLevel,

                            DeptId = LibSysUtils.ToString(reader["DEPTID"]),
                            DeptName = LibSysUtils.ToString(reader["DEPTNAME"]),
                            DutyId = LibSysUtils.ToString(reader["DUTYID"]),
                            DutyName = LibSysUtils.ToString(reader["DUTYNAME"]),
                            IsJump = LibSysUtils.ToBoolean(reader["ISJUMP"]),
                            JumpReason = LibSysUtils.ToString(reader["JUMPREASON"]),
                            IsSameDefault = LibSysUtils.ToBoolean(reader["ISSAMEDEFAULT"]),

                            ExecuteDesc = LibSysUtils.ToString(reader["EXECUTEDESC"]),
                            AuditTaskId = LibSysUtils.ToString(reader["AUDITTASKID"])// 主键Guid
                        });
                    }
                }
            }

            return flowList;
        }

        public LibApprovePersonInfo GetApprovePersonInfo(string pk)
        {
            LibApprovePersonInfo approvePersonInfo = new LibApprovePersonInfo();
            string sql = string.Format("SELECT A.PERSONNAME,A.POSITION FROM COMPERSON A WHERE A.PERSONID={0}", LibStringBuilder.GetQuotString(pk));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    approvePersonInfo.PersonName = LibSysUtils.ToString(reader["PERSONNAME"]);
                    approvePersonInfo.Position = LibSysUtils.ToString(reader["POSITION"]);
                }
            }
            return approvePersonInfo;
        }
        /// <summary>
        /// 前端手动变更具体一个单据的审核流程
        /// </summary>
        /// <param name="internalId"></param>
        /// <param name="curLevel"></param>
        /// <param name="flowList"></param>
        public void UpdateApproveFlow(string internalId, int curLevel, SortedList<int, List<LibApproveFlowInfo>> flowList)
        {
            //增加权限判断，必须要有审核流配置权限的人才可以变更 Zhangkj 20170324       
            if (LibPermissionControl.Default.HasPermission(this.Handle, "axp.ApproveFlow", FuncPermissionEnum.Edit) == false)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前操作人员无权变更此单据的审核流程。");
                return;
            }

            string levelConditon = curLevel > 0 ? string.Format(" and FLOWLEVEL>{0}", curLevel) : string.Empty;
            List<string> sqlList = new List<string>();
            sqlList.Add(string.Format("delete AXPAPPROVETASK where PROGID='{0}' and INTERNALID='{1}' and FROMROWID=0 {2}",
                 this.ProgId, internalId, levelConditon));
            foreach (var flowInfo in flowList)
            {
                if (flowInfo.Key <= curLevel)
                    continue;
                foreach (var info in flowInfo.Value)
                {
                    if (HasAduitOfDuty == false)
                    {
                        sqlList.Add(string.Format("insert into AXPAPPROVETASK(PROGID,INTERNALID,FROMROWID,CURRENTLEVEL,FLOWLEVEL,SUBMITTERID,PERSONID,INDEPENDENT,AUDITSTATE) "
                         + "values('{0}','{1}',{2},{3},{4},'{5}','{6}',{7},0)", this.ProgId, internalId, 0, curLevel, flowInfo.Key, info.SubmitterId, info.PersonId, info.Independent ? 1 : 0));
                    }
                    else
                    {
                        sqlList.Add(string.Format("insert into AXPAPPROVETASK(AUDITTASKID,PROGID,INTERNALID,FROMROWID,CURRENTLEVEL,FLOWLEVEL,SUBMITTERID,PERSONID,INDEPENDENT,AUDITSTATE) "
                         + "values('{8}','{0}','{1}',{2},{3},{4},'{5}','{6}',{7},0)",
                         this.ProgId, internalId, 0, curLevel, flowInfo.Key, info.SubmitterId, info.PersonId, info.Independent ? 1 : 0,
                         Guid.NewGuid().ToString()//使用Guid作为主键
                         ));
                    }
                }
            }
            this.DataAccess.ExecuteNonQuery(sqlList);
        }
        /// <summary>
        /// 根据单据配置的审核流，寻找匹配的审核流程
        /// </summary>
        /// <param name="masterRow"></param>
        /// <param name="errorInfo"></param>
        /// <param name="showConfig"></param>
        /// <returns></returns>
        private SortedList<int, List<LibApproveFlowInfo>> GetDefaultFlowByDataRow(DataRow masterRow, out string errorInfo, bool showConfig = false)
        {
            errorInfo = string.Empty;
            SortedList<int, List<LibApproveFlowInfo>> ret = null;
            LibApproveFlow flow = LibApproveFlowCache.Default.GetCacheItem(this.ProgId);
            if (flow != null)
            {
                LibApproveFlowItem defalutItem = null;
                if (flow.ApproveFlowList.ContainsKey(string.Empty))
                    defalutItem = flow.ApproveFlowList[string.Empty];//如果有无条件的审核流程，则作为默认值
                bool isFind = false;
                foreach (var flowItem in flow.ApproveFlowList)
                {
                    if (string.IsNullOrEmpty(flowItem.Key))
                        continue;
                    if (LibApproveFlowParser.Parse(flowItem.Key, masterRow, null))
                    {
                        ret = flowItem.Value.FlowInfoDic;
                        isFind = true;
                        break;
                    }
                }
                if (!isFind && defalutItem != null)
                    ret = defalutItem.FlowInfoDic;//没有其他匹配的审核流程时使用无条件的默认流程
            }
            //如果仅想查看单据配置也是不用计算审核人的（showConfig==true），showConfig == false时说明才需要查看针对提交人的具体审核流程
            if (HasAduitOfDuty && showConfig == false)
            {
                //如果存在部门岗位审核相关字段，则根据配置计算每个步骤的审核人
                Dictionary<string, object> masterDic = new Dictionary<string, object>();
                foreach (DataColumn col in masterRow.Table.Columns)
                {
                    masterDic.Add(col.ColumnName, masterRow[col.ColumnName]);
                }
                ComputeAduitResult result = ComputeAduitExecutor(ret, this.Handle.PersonId, masterDic);
                if (result.IsCanSubmitAduit)
                    ret = result.FlowList;
                else
                {
                    errorInfo = result.ErrorInfo;
                    ret = null;
                }
            }
            return ret;
        }
        /// <summary>
        /// masterDic的参数是前台传递过来的键值对
        /// </summary>
        /// <param name="masterDic"></param>
        /// <param name="showConfig">是否仅显示配置</param>
        /// <returns></returns>
        public SortedList<int, List<LibApproveFlowInfo>> GetDefaultFlow(Dictionary<string, object> masterDic, bool showConfig = false)
        {
            SortedList<int, List<LibApproveFlowInfo>> ret = null;
            LibApproveFlow flow = LibApproveFlowCache.Default.GetCacheItem(this.ProgId);
            if (flow != null)
            {
                LibApproveFlowItem defalutItem = null;
                if (flow.ApproveFlowList.ContainsKey(string.Empty))
                    defalutItem = flow.ApproveFlowList[string.Empty];
                bool isFind = false;
                foreach (var flowItem in flow.ApproveFlowList)
                {
                    if (string.IsNullOrEmpty(flowItem.Key))
                        continue;
                    if (LibApproveFlowParser.Parse(flowItem.Key, masterDic, null))
                    {
                        ret = flowItem.Value.FlowInfoDic;
                        isFind = true;
                        break;
                    }
                }
                if (!isFind && defalutItem != null)
                    ret = defalutItem.FlowInfoDic;
            }
            //如果仅想查看单据配置也是不用计算审核人的（showConfig==true），showConfig == false时说明才需要查看针对提交人的具体审核流程
            if (HasAduitOfDuty && showConfig == false)
            {
                //如果存在部门岗位审核相关字段，则根据配置计算每个步骤的审核人
                ComputeAduitResult result = ComputeAduitExecutor(ret, this.Handle.PersonId, masterDic);
                if (result.IsCanSubmitAduit)
                    ret = result.FlowList;
                else
                {
                    ret = null;
                }
            }
            return ret;
        }
        #region 计算审核执行人
        /// <summary>
        /// 计算具体审核执行人的结果
        /// </summary>
        public class ComputeAduitResult
        {
            /// <summary>
            /// 是否可提交审核（如果发现某个审核过程无法执行则不可提交审核）
            /// </summary>
            public bool IsCanSubmitAduit { get; set; }

            /// <summary>
            /// 审核流的初始配置是否没有具体审核过程
            /// </summary>
            public bool IsConfigProcEmpty { get; set; }
            /// <summary>
            /// 审核过程的配置错误。违反了配置规则，例如具体审核人和岗位都为空
            /// </summary>
            public bool IsConfigProcError { get; set; }

            /// <summary>
            /// 是否计算得到审核人
            /// </summary>
            public bool IsComputedAduitor { get; set; }

            #region 待进一步优化检查的信息 Zhangkj 20170323
            /// <summary>
            /// 审核人Id标识的Person记录是否存在
            /// </summary>
            public bool IsAduitPersonExist { get; set; }
            /// <summary>
            /// 审核人Person是否拥有系统账户（如没有则无法执行审核）
            /// </summary>
            public bool IsAduitPersonHasAccount { get; set; }
            #endregion


            private SortedList<int, List<LibApproveFlowInfo>> _FlowList = null;
            /// <summary>
            /// 获取计算得到的包含了具体执行人的审核过程信息
            /// </summary>
            public SortedList<int, List<LibApproveFlowInfo>> FlowList
            {
                get { return _FlowList; }
            }

            private string _ErrorInfo = string.Empty;
            /// <summary>
            /// 错误信息
            /// </summary>
            public string ErrorInfo
            {
                get { return _ErrorInfo; }
                internal set { _ErrorInfo = value; }
            }

            private List<string> _ExecuteDesc = new List<string>();
            /// <summary>
            /// 审核流程在根据配置查找审核执行人的过程中的执行说明。例如过程中进行了岗位上溯、部门上溯、跳过等。
            /// </summary>
            public List<string> ExecuteDesc
            {
                get { return _ExecuteDesc; }
                set { _ExecuteDesc = value; }
            }

            /// <summary>
            /// 设置计算后的审核过程信息
            /// </summary>
            /// <param name="value"></param>
            internal void SetFlowList(SortedList<int, List<LibApproveFlowInfo>> value)
            {
                _FlowList = value;
            }

            public ComputeAduitResult()
            {
                IsCanSubmitAduit = false;
                IsComputedAduitor = false;
                IsAduitPersonExist = false;
                IsAduitPersonHasAccount = false;
                IsConfigProcEmpty = false;
            }
        }
        /// <summary>
        /// 根据审核流的配置，为每一个审核过程查找具体的审核执行人
        /// </summary>
        /// <param name="flowListConfig">匹配的单据审核流程配置</param>
        /// <param name="submitPersonId">提交人Id</param>
        /// <param name="masterDic">待审核的单据主表的键值对数据</param>
        /// <returns></returns>
        private ComputeAduitResult ComputeAduitExecutor(SortedList<int, List<LibApproveFlowInfo>> flowListConfig, string submitPersonId, Dictionary<string, object> masterDic)
        {
            SortedList<int, List<LibApproveFlowInfo>> flowList = null;
            ComputeAduitResult result = new ComputeAduitResult();
            if (flowListConfig == null)
            {
                result.IsConfigProcEmpty = true;
                return result;
            }
            //深度复制一份,避免对通用配置的修改
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, flowListConfig);
                stream.Position = 0;
                flowList = formatter.Deserialize(stream) as SortedList<int, List<LibApproveFlowInfo>>;//深度复制一份
            }
            //将键值对中的所有审核过程都取出来放到一个List中
            List<LibApproveFlowInfo> procList = new List<LibApproveFlowInfo>();
            foreach (int key in flowList.Keys)
            {
                foreach (LibApproveFlowInfo flowInfo in flowList[key])
                {
                    procList.Add(flowInfo);
                }
            }
            //按审核层级从小到大排序，再按是否指定了审核人排序（指定了的排前面）、再按是否指定了部门排序（指定了的排前面）
            procList = (from item in procList
                        orderby item.FlowLevel ascending, item.PersonId descending, item.DeptId descending
                        select item).ToList();

            if (procList.Count == 0)
            {
                result.IsConfigProcEmpty = true;
                return result;
            }
            //查找提交人的最大等级岗位
            DutyPeronInfo mostHighLevelInfo = LibDeptDutyPersonCache.Default.GetHighestDuty(submitPersonId);
            int mostHighLevel = int.MinValue;
            if (mostHighLevelInfo != null)
                mostHighLevel = mostHighLevelInfo.DutyLevel;

            bool isFindDeptIdByPersonDept = false;//是否是通过提交人的所属部门找到的部门Id
            string deptIdOfSubmitPerson = string.Empty;//提交人所在部门Id
            bool hasQueryDeptIdOfSubmitPerson = false;//是否已经查找过提交人所在部门Id
            #region 查找执行人
            //查找执行人
            Dictionary<string, List<string>> toCheckPersonIdDic = new Dictionary<string, List<string>>();//待检查的PersonId与其要审核的步骤的对应关系信息
            foreach (LibApproveFlowInfo proc in procList)
            {
                isFindDeptIdByPersonDept = false;//针对每个审核过程重置此参数
                if (proc == null)
                {
                    result.IsConfigProcEmpty = true;
                    return result;
                }
                if (string.IsNullOrEmpty(proc.PersonId) && string.IsNullOrEmpty(proc.DutyId))
                {
                    result.IsConfigProcError = true;
                    result.ErrorInfo = string.Format("审核流配置有误，具体审核人和岗位都为空。层级:{0},行号:{1}", proc.FlowLevel, proc.FlowProcRowNo);
                    return result;
                }
                if (string.IsNullOrEmpty(proc.PersonId) == false)
                {
                    result.IsComputedAduitor = true;
                    //记录下审核人的Id，待检查
                    if (toCheckPersonIdDic.ContainsKey(proc.PersonId) == false)
                        toCheckPersonIdDic.Add(proc.PersonId, new List<string>());

                    //设置部门Id和职位Id为指定审核人的所属部门和最高职位。
                    string querySql = string.Format("Select DEPTID from COMPERSON where PERSONID = {0} ", LibStringBuilder.GetQuotString(proc.PersonId));
                    proc.DeptId = LibSysUtils.ToString(this.DataAccess.ExecuteScalar(querySql));

                    DutyPeronInfo dutyPerson = LibDeptDutyPersonCache.Default.GetHighestDuty(proc.PersonId);
                    if (dutyPerson != null)
                        proc.DutyId = dutyPerson.DutyId;

                    toCheckPersonIdDic[proc.PersonId].Add(string.Format("层级:{0},行号:{1}", proc.FlowLevel, proc.FlowProcRowNo));
                    continue;
                }
                //到这一步说明具体审核人未指定，岗位已指定，检查部门是否为空
                string deptId = string.Empty;
                if (string.IsNullOrEmpty(proc.DeptId) == false)
                {
                    deptId = proc.DeptId;//直接配置了部门
                }
                else
                {
                    //从动态部门字段查找部门
                    if (string.IsNullOrEmpty(proc.DeptIdColumn) == false)
                    {
                        if (masterDic == null || masterDic.ContainsKey(proc.DeptIdColumn) == false || string.IsNullOrEmpty(masterDic[proc.DeptIdColumn].ToString()))
                        {
                            if (proc.CanJump)
                            {
                                proc.IsJump = true;
                                proc.JumpReason = string.Format("检查审核流配置:未根据动态部门字段找到审核部门。层级:{0},行号:{1}", proc.FlowLevel, proc.FlowProcRowNo);
                                continue;
                            }
                            else
                            {
                                result.IsConfigProcError = true;
                                result.ErrorInfo = string.Format("检查审核流配置:配置了动态部门字段，但未从表单中找到部门代码。层级:{0},行号:{1},字段:{2}", proc.FlowLevel, proc.FlowProcRowNo, proc.DeptIdColumn);
                                return result;
                            }
                        }
                        else
                        {
                            deptId = masterDic[proc.DeptIdColumn].ToString();//从表单中获取到部门Id
                        }
                    }
                    else
                    {
                        //从人员所在的部门查找
                        if (string.IsNullOrEmpty(submitPersonId) == false)
                        {
                            if (string.IsNullOrEmpty(deptIdOfSubmitPerson) && hasQueryDeptIdOfSubmitPerson == false)
                            {
                                string querySql = string.Format("Select DEPTID from COMPERSON where PERSONID = {0} ", LibStringBuilder.GetQuotString(submitPersonId));
                                deptIdOfSubmitPerson = LibSysUtils.ToString(this.DataAccess.ExecuteScalar(querySql));
                                hasQueryDeptIdOfSubmitPerson = true;
                            }
                            deptId = deptIdOfSubmitPerson;
                            if (string.IsNullOrEmpty(deptId))
                            {
                                if (proc.CanJump)
                                {
                                    proc.IsJump = true;
                                    proc.JumpReason = string.Format("提交人的所在部门为空，但可跳过。层级:{0},行号:{1}", proc.FlowLevel, proc.FlowProcRowNo);
                                    continue;
                                }
                                else
                                {
                                    result.IsComputedAduitor = false;
                                    result.ErrorInfo = string.Format("检查审核流配置:提交人的所在部门为空。层级:{0},行号:{1}", proc.FlowLevel, proc.FlowProcRowNo);
                                    return result;
                                }
                            }
                            isFindDeptIdByPersonDept = true;//通过提交人所属部门查找到的部门
                        }
                        else
                        {
                            if (proc.CanJump)
                            {
                                proc.IsJump = true;
                                proc.JumpReason = string.Format("提交人代码为空，但可跳过。层级:{0},行号:{1}", proc.FlowLevel, proc.FlowProcRowNo);
                                continue;
                            }
                            else
                            {
                                result.IsConfigProcError = true;
                                result.ErrorInfo = string.Format("检查审核流配置:提交人代码为空。层级:{0},行号:{1}", proc.FlowLevel, proc.FlowProcRowNo);
                                return result;
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(deptId) == false)
                {
                    //根据部门标识和岗位标识、以及是否岗位上溯、是否部门上溯，查找审核执行人
                    GetDutyPersonParams gparams = new GetDutyPersonParams()
                    {
                        DutyId = proc.DutyId,
                        DutyLevel = proc.DutyLevel,
                        DutyName = proc.DutyName,
                        IsDutyUp = proc.IsDutyUp,
                        IsDeptUp = proc.IsDeptUp,
                        IsNotSelf = proc.NotSelf,
                        IsMustHighLevel = proc.MustHighLevel && isFindDeptIdByPersonDept, //仅对于是通过提交人的所属部门找到的部门Id，而且配置为需要更高岗位执行审核时，才检查更高岗位
                        SubmitPersonId = submitPersonId,
                        SubmitPersonMostHighLevel = mostHighLevel,

                        ProcStepDesc = string.Format("层级:{0},行号:{1}", proc.FlowLevel, proc.FlowProcRowNo)
                    };
                    List<string> execDesc = new List<string>();
                    PersonInfo personInfo = LibDeptDutyPersonCache.Default.GetDutyPerson(deptId, gparams, ref execDesc);
                    if (personInfo == null)
                    {
                        if (result.ExecuteDesc != null && result.ExecuteDesc.Count > 0)
                            proc.ExecuteDesc = string.Join("【br】 ", result.ExecuteDesc);
                        if (proc.CanJump)
                        {
                            proc.IsJump = true;
                            proc.JumpReason = string.Format("未根据部门和岗位信息找到审核人，但可跳过。层级:{0},行号:{1}", proc.FlowLevel, proc.FlowProcRowNo);
                            continue;
                        }
                        else
                        {
                            result.IsComputedAduitor = false;
                            if (string.IsNullOrEmpty(proc.ExecuteDesc) == false)
                                result.ErrorInfo = string.Format("执行信息:{4}。检查审核流配置:未根据部门和岗位信息找到审核人。\r\n层级:{0},行号:{1},部门代码:{2},岗位名称:{3}",
                                                                proc.FlowLevel, proc.FlowProcRowNo, deptId, proc.DutyName, proc.ExecuteDesc);
                            else
                                result.ErrorInfo = string.Format("检查审核流配置:未根据部门和岗位信息找到审核人。\r\n层级:{0},行号:{1},部门代码:{2},岗位名称:{3}",
                                                                proc.FlowLevel, proc.FlowProcRowNo, deptId, proc.DutyName);
                            return result;
                        }
                    }
                    else
                    {
                        result.IsComputedAduitor = true;
                        result.ExecuteDesc = execDesc;//执行过程中的特殊说明
                        if (result.ExecuteDesc != null && result.ExecuteDesc.Count > 0)
                            proc.ExecuteDesc = string.Join("【br】 ", result.ExecuteDesc);

                        proc.PersonId = personInfo.PersonId;//记录下审核人
                        proc.PersonName = personInfo.PersonName;
                        proc.DeptId = personInfo.AsBelongDeptId;
                        proc.DeptName = personInfo.AsBelongDeptName;
                        proc.DutyId = personInfo.AsBeOfficeDutyId;
                        proc.DutyName = personInfo.AsBeOfficeDutyName;

                        //记录下审核人的Id，待检查
                        if (toCheckPersonIdDic.ContainsKey(personInfo.PersonId) == false)
                            toCheckPersonIdDic.Add(personInfo.PersonId, new List<string>());
                        toCheckPersonIdDic[personInfo.PersonId].Add(string.Format("层级:{0},行号:{1}", proc.FlowLevel, proc.FlowProcRowNo));
                        continue;
                    }
                }
                else
                {
                    if (proc.CanJump)
                    {
                        proc.IsJump = true;
                        proc.JumpReason = string.Format("未根据配置找到审核部门，但可跳过。层级:{0},行号:{1}", proc.FlowLevel, proc.FlowProcRowNo);
                        continue;
                    }
                    else
                    {
                        result.IsComputedAduitor = false;
                        result.ErrorInfo = string.Format("检查审核流配置:未根据配置找到审核部门。层级:{0},行号:{1}", proc.FlowLevel, proc.FlowProcRowNo);
                        return result;
                    }
                }
            }
            #endregion

            //to do 检查每个审核人Id对应的Person是否存在，
            //to do 检查是否有与Person关联的可用系统账户

            result.IsCanSubmitAduit = true;
            result.SetFlowList(flowList);
            return result;
        }

        #endregion

        #endregion

        #region [行项审核相关]

        /// <summary>
        /// 获取待审核的行，返回行号列表
        /// </summary>
        /// <param name="internalId"></param>
        /// <param name="rowList"></param>
        /// <returns></returns>
        public List<int> GetWaitApproveRow(string internalId, List<int> rowList)
        {
            List<int> list = new List<int>();
            string curPerson = this.Handle.PersonId;
            StringBuilder builder = new StringBuilder();
            foreach (int item in rowList)//前端只有未审核通过的行才会提交过来检查
            {
                builder.AppendFormat("A.FROMROWID={0} OR ", LibSysUtils.ToInt32(item));
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 3, 3);
                string sql = string.Empty;
                if (HasAduitOfDuty == false)
                {
                    sql = string.Format("select distinct A.FROMROWID from AXPAPPROVETASK A inner join COMPERSON B on " +
                                         "B.PERSONID=A.PERSONID where A.PROGID='{0}' and A.INTERNALID='{1}' and A.FLOWLEVEL=(A.CURRENTLEVEL+1) and A.AUDITSTATE=0 and A.PERSONID='{2}' and ({3})",
                                         this.ProgId, internalId, curPerson, builder.ToString());

                }
                else
                {
                    //因为有可能存在跳过的行，因此需要预先判断下当前需要进行审核的层级。
                    int toQueryFlowLevel = 1;//默认查找第一行
                    bool foundNotJumpProc = false;
                    int curLevel = 0;
                    int thisFound = 0;
                    string foundLevel = string.Format("select A.ISJUMP,A.FLOWLEVEL,A.PERSONID,A.CURRENTLEVEL from AXPAPPROVETASK A " +
                                        "where A.PROGID='{0}' and A.INTERNALID='{1}' and A.AUDITSTATE=0  and ({2}) and A.FLOWLEVEL=(A.CURRENTLEVEL+1) ",
                                       this.ProgId, internalId, builder.ToString());
                    while (foundNotJumpProc == false)
                    {
                        thisFound = 0;
                        using (IDataReader reader = this.DataAccess.ExecuteDataReader(foundLevel))
                        {
                            while (reader.Read())
                            {
                                thisFound++;
                                bool isJump = LibSysUtils.ToBoolean(reader["ISJUMP"]);
                                int flowLevel = LibSysUtils.ToInt32(reader["FLOWLEVEL"]);
                                string personId = LibSysUtils.ToString(reader["PERSONID"]);
                                curLevel = LibSysUtils.ToInt32(reader["CURRENTLEVEL"]);
                                if (isJump == false)
                                {
                                    toQueryFlowLevel = flowLevel;
                                    foundNotJumpProc = true;
                                    break;
                                }
                            }
                        }
                        if (foundNotJumpProc || thisFound == 0)
                            break;
                        curLevel++;
                        foundLevel = string.Format("select A.ISJUMP,A.FLOWLEVEL,A.PERSONID,A.CURRENTLEVEL from AXPAPPROVETASK A " +
                                        "where A.PROGID='{0}' and A.INTERNALID='{1}' and A.AUDITSTATE=0  and ({2}) and A.FLOWLEVEL={3} ",
                                       this.ProgId, internalId, builder.ToString(), curLevel + 1);
                    }

                    sql = string.Format("select distinct A.FROMROWID from AXPAPPROVETASK A inner join COMPERSON B on " +
                                         "B.PERSONID=A.PERSONID where A.PROGID='{0}' and A.INTERNALID='{1}' and A.FLOWLEVEL={4} and A.AUDITSTATE=0 and A.PERSONID='{2}' and ({3})",
                                         this.ProgId, internalId, curPerson, builder.ToString(), toQueryFlowLevel);
                }
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        list.Add(LibSysUtils.ToInt32(reader["FROMROWID"]));
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 从数据库中获取审核行的进度任务信息
        /// </summary>
        /// <param name="ret"></param>
        /// <param name="builder"></param>
        /// <param name="internalId"></param>
        private void GetApproveTaskData(Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> ret, StringBuilder builder, string internalId)
        {
            builder.Remove(builder.Length - 3, 3);
            if (HasAduitOfDuty == false)
            {
                //无部门岗位审核相关字段，则按原方法执行查询
                string sql = string.Format("select A.FROMROWID,A.FLOWLEVEL,A.SUBMITTERID,A.PERSONID,B.PERSONNAME," +
                                          "B.POSITION,A.INDEPENDENT,A.AUDITSTATE from AXPAPPROVETASK A inner join COMPERSON B on " +
                                          "B.PERSONID=A.PERSONID where A.PROGID={0} and A.INTERNALID={1} and ({2}) ORDER BY A.FROMROWID,A.FLOWLEVEL",
                                          LibStringBuilder.GetQuotString(this.ProgId), LibStringBuilder.GetQuotString(internalId),
                                          builder.ToString());
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        int rowId = LibSysUtils.ToInt32(reader["FROMROWID"]);
                        if (!ret.ContainsKey(rowId))
                            ret.Add(rowId, new SortedList<int, List<LibApproveFlowInfo>>());
                        SortedList<int, List<LibApproveFlowInfo>> flowList = ret[rowId];
                        int flowLevel = LibSysUtils.ToInt32(reader["FLOWLEVEL"]);
                        if (!flowList.ContainsKey(flowLevel))
                            flowList.Add(flowLevel, new List<LibApproveFlowInfo>());
                        List<LibApproveFlowInfo> infoList = flowList[flowLevel];
                        int auditState = LibSysUtils.ToInt32(reader["AUDITSTATE"]);
                        infoList.Add(new LibApproveFlowInfo()
                        {
                            SubmitterId = LibSysUtils.ToString(reader["SubmitterId"]),
                            PersonId = LibSysUtils.ToString(reader["PersonId"]),
                            PersonName = LibSysUtils.ToString(reader["PersonName"]),
                            Position = LibSysUtils.ToString(reader["Position"]),
                            Independent = LibSysUtils.ToBoolean(reader["Independent"]),
                            AuditState = auditState,
                            IsPass = auditState == 1

                        });
                    }
                }
            }
            else
            {
                //有部门岗位审核相关字段
                string sql = string.Format("select A.AUDITTASKID,A.FROMROWID,A.FLOWLEVEL,A.SUBMITTERID,A.PERSONID,B.PERSONNAME," +
                                          " B.POSITION,A.INDEPENDENT,A.AUDITSTATE,A.AUDITOPINION, " +
                                          " A.DEPTID,C.DEPTNAME,A.DUTYID,D.DUTYNAME,A.ISJUMP,A.JUMPREASON,A.ISSAMEDEFAULT,A.EXECUTEDESC " +
                                          " from AXPAPPROVETASK A " +
                                          " left join COMPERSON B on  B.PERSONID=A.PERSONID " +//left join 具体审核人可能为空（可能会跳过）
                                         " left join COMDEPT C on C.DEPTID = A.DEPTID " +
                                         " left join COMDUTY D on D.DUTYID = A.DUTYID " +
                                          " where A.PROGID={0} and A.INTERNALID={1} and ({2}) ORDER BY A.FROMROWID,A.FLOWLEVEL",
                                          LibStringBuilder.GetQuotString(this.ProgId), LibStringBuilder.GetQuotString(internalId),
                                          builder.ToString());
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        int rowId = LibSysUtils.ToInt32(reader["FROMROWID"]);
                        if (!ret.ContainsKey(rowId))
                            ret.Add(rowId, new SortedList<int, List<LibApproveFlowInfo>>());
                        SortedList<int, List<LibApproveFlowInfo>> flowList = ret[rowId];
                        int flowLevel = LibSysUtils.ToInt32(reader["FLOWLEVEL"]);
                        if (!flowList.ContainsKey(flowLevel))
                            flowList.Add(flowLevel, new List<LibApproveFlowInfo>());
                        List<LibApproveFlowInfo> infoList = flowList[flowLevel];
                        int auditState = LibSysUtils.ToInt32(reader["AUDITSTATE"]);
                        infoList.Add(new LibApproveFlowInfo()
                        {
                            SubmitterId = LibSysUtils.ToString(reader["SUBMITTERID"]),
                            PersonId = LibSysUtils.ToString(reader["PERSONID"]),
                            PersonName = LibSysUtils.ToString(reader["PERSONNAME"]),
                            Position = LibSysUtils.ToString(reader["POSITION"]),
                            Independent = LibSysUtils.ToBoolean(reader["INDEPENDENT"]),
                            AuditOpinion = LibSysUtils.ToString(reader["AUDITOPINION"]),
                            AuditState = auditState,
                            IsPass = auditState == 1,

                            FlowLevel = flowLevel,

                            DeptId = LibSysUtils.ToString(reader["DEPTID"]),
                            DeptName = LibSysUtils.ToString(reader["DEPTNAME"]),
                            DutyId = LibSysUtils.ToString(reader["DUTYID"]),
                            DutyName = LibSysUtils.ToString(reader["DUTYNAME"]),
                            IsJump = LibSysUtils.ToBoolean(reader["ISJUMP"]),
                            JumpReason = LibSysUtils.ToString(reader["JUMPREASON"]),
                            IsSameDefault = LibSysUtils.ToBoolean(reader["ISSAMEDEFAULT"]),

                            ExecuteDesc = LibSysUtils.ToString(reader["EXECUTEDESC"]),
                            AuditTaskId = LibSysUtils.ToString(reader["AUDITTASKID"])// 主键Guid
                        });
                    }
                }
            }
        }
        /// <summary>
        /// 查看审核流程,前端Bcf调用
        /// </summary>
        /// <param name="masterDic"></param>
        /// <param name="bodyDic"></param>
        /// <param name="showConfig">是否是显示配置</param>
        /// <returns></returns>
        public SortedList<int, List<LibApproveFlowInfo>> GetApproveFlow(Dictionary<string, object> masterDic, Dictionary<string, object> bodyDic, bool showConfig = false)
        {
            SortedList<int, List<LibApproveFlowInfo>> curList = null;
            string errorInfo = string.Empty;
            Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> ret = GetApproveFlowList(masterDic, new List<Dictionary<string, object>>() { bodyDic }, out errorInfo, showConfig);
            if (string.IsNullOrEmpty(errorInfo) == false)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "获取审核流程出现错误:" + errorInfo);
                return null;
            }
            int rowId = LibSysUtils.ToInt32(bodyDic["ROW_ID"]);
            if (ret != null && ret.ContainsKey(rowId))
                curList = ret[rowId];
            return curList;
        }

        public SortedList<int, List<LibApproveFlowInfo>> GetApproveFlowByDataRow(DataRow masterRow, DataRow bodyRow, out string errorInfo, bool showConfig = false)
        {
            errorInfo = string.Empty;
            SortedList<int, List<LibApproveFlowInfo>> curList = null;
            Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> ret = GetApproveFlowListByDataRow(masterRow, new List<DataRow>() { bodyRow }, out errorInfo, showConfig);
            int rowId = LibSysUtils.ToInt32(bodyRow["ROW_ID"]);
            if (ret != null && ret.ContainsKey(rowId))
                curList = ret[rowId];
            return curList;
        }

        public Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> GetApproveFlowList(Dictionary<string, object> masterDic, List<Dictionary<string, object>> bodyDic, out string errorInfo, bool showConfig = false)
        {
            errorInfo = string.Empty;
            Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> ret = new Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>>();
            string internalId = LibSysUtils.ToString(masterDic["INTERNALID"]);
            StringBuilder builder = new StringBuilder();
            foreach (Dictionary<string, object> item in bodyDic)
            {
                builder.AppendFormat("A.FROMROWID={0} OR ", LibSysUtils.ToInt32(item["ROW_ID"]));
            }
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            if (builder.Length > 0)
            {
                if (showConfig == false)
                    GetApproveTaskData(ret, builder, internalId);
                foreach (Dictionary<string, object> item in bodyDic)
                {
                    int rowId = LibSysUtils.ToInt32(item["ROW_ID"]);
                    if (!ret.ContainsKey(rowId))
                        list.Add(item);
                }
            }
            if (list.Count > 0 || showConfig)
            {
                FillDefaultApproveFlow(ret, masterDic, list, out errorInfo, showConfig);
            }
            return ret;
        }

        private void FillDefaultApproveFlow(Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> ret, Dictionary<string, object> masterDic, List<Dictionary<string, object>> bodyDic,
            out string errorInfo, bool showConfig = false)
        {
            errorInfo = string.Empty;
            LibApproveFlow flow = LibApproveFlowCache.Default.GetCacheItem(this.ProgId, true);
            if (flow != null)
            {
                LibApproveFlowItem defalutItem = null;
                if (flow.ApproveFlowList.ContainsKey(string.Empty))
                    defalutItem = flow.ApproveFlowList[string.Empty];
                foreach (Dictionary<string, object> subItem in bodyDic)
                {
                    int rowId = LibSysUtils.ToInt32(subItem["ROW_ID"]);
                    bool isFind = false;
                    foreach (var flowItem in flow.ApproveFlowList)
                    {
                        if (string.IsNullOrEmpty(flowItem.Key))
                            continue;
                        if (LibApproveFlowParser.Parse(flowItem.Key, masterDic, subItem))
                        {
                            ret.Add(rowId, flowItem.Value.FlowInfoDic);
                            isFind = true;
                            break;
                        }
                    }
                    if (!isFind && defalutItem != null)
                        ret.Add(rowId, defalutItem.FlowInfoDic);
                }
            }
            //如果仅想查看单据配置也是不用计算审核人的（showConfig==true），showConfig == false时说明才需要查看针对提交人的具体审核流程
            if (HasAduitOfDuty && showConfig == false && ret != null)
            {
                //如果存在部门岗位审核相关字段，则根据配置计算每个步骤的审核人              
                ComputeAduitResult result = null;
                foreach (Dictionary<string, object> subItem in bodyDic)
                {
                    int rowId = LibSysUtils.ToInt32(subItem["ROW_ID"]);
                    if (ret.ContainsKey(rowId))//针对每行进行检查
                    {
                        result = ComputeAduitExecutor(ret[rowId], this.Handle.PersonId, masterDic);
                        if (result.IsCanSubmitAduit)
                            ret[rowId] = result.FlowList;
                        else
                        {
                            errorInfo = result.ErrorInfo;
                            ret = null;
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 获取默认的审核流程
        /// </summary>
        /// <param name="masterDic"></param>
        /// <param name="bodyDic"></param>
        /// <param name="showConfig"></param>
        /// <returns></returns>
        public SortedList<int, List<LibApproveFlowInfo>> GetDefaultApproveFlow(Dictionary<string, object> masterDic, Dictionary<string, object> bodyDic, bool showConfig = false)
        {
            SortedList<int, List<LibApproveFlowInfo>> curList = null;
            Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> ret = new Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>>();
            string errorInfo = string.Empty;
            FillDefaultApproveFlow(ret, masterDic, new List<Dictionary<string, object>>() { bodyDic }, out errorInfo, showConfig);
            if (string.IsNullOrEmpty(errorInfo) == false)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "获取审核过程出现错误：" + errorInfo);
                return null;
            }
            int rowId = LibSysUtils.ToInt32(bodyDic["ROW_ID"]);
            if (ret != null && ret.ContainsKey(rowId))
                curList = ret[rowId];
            return curList;
        }

        public Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> GetApproveFlowListByDataRow(DataRow masterRow, List<DataRow> bodyRowList, out string errorInfo, bool showConfig = false)
        {
            errorInfo = string.Empty;
            Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> ret = new Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>>();
            string internalId = LibSysUtils.ToString(masterRow["INTERNALID"]);
            StringBuilder builder = new StringBuilder();
            List<DataRow> listOfNeedFindFlowRows = new List<DataRow>();

            foreach (DataRow item in bodyRowList)
            {
                builder.AppendFormat("A.FROMROWID={0} OR ", LibSysUtils.ToInt32(item["ROW_ID"]));
            }
            if (builder.Length > 0)
            {
                if (showConfig == false)
                {
                    GetApproveTaskData(ret, builder, internalId);
                }
                foreach (DataRow item in bodyRowList)
                {
                    int rowId = LibSysUtils.ToInt32(item["ROW_ID"]);
                    if (!ret.ContainsKey(rowId))
                        listOfNeedFindFlowRows.Add(item);
                }
            }
            if (listOfNeedFindFlowRows.Count > 0 || showConfig)//有需要查找审核流配置的行，或者显示配置时，读取配置
            {
                LibApproveFlow flow = LibApproveFlowCache.Default.GetCacheItem(this.ProgId, true);
                if (flow != null)
                {
                    LibApproveFlowItem defalutItem = null;
                    if (flow.ApproveFlowList.ContainsKey(string.Empty))
                        defalutItem = flow.ApproveFlowList[string.Empty];
                    foreach (var subItem in listOfNeedFindFlowRows)
                    {
                        int rowId = LibSysUtils.ToInt32(subItem["ROW_ID"]);
                        bool isFind = false;
                        foreach (var flowItem in flow.ApproveFlowList)
                        {
                            if (string.IsNullOrEmpty(flowItem.Key))
                                continue;
                            if (LibApproveFlowParser.Parse(flowItem.Key, masterRow, subItem))
                            {
                                ret.Add(rowId, flowItem.Value.FlowInfoDic);
                                isFind = true;
                                break;
                            }
                        }
                        if (!isFind && defalutItem != null)
                            ret.Add(rowId, defalutItem.FlowInfoDic);
                    }
                }
                //如果仅想查看单据配置也是不用计算审核人的（showConfig==true），showConfig == false时说明才需要查看针对提交人的具体审核流程
                if (HasAduitOfDuty && showConfig == false)
                {
                    //如果存在部门岗位审核相关字段，则根据配置计算每个步骤的审核人
                    Dictionary<string, object> masterDic = new Dictionary<string, object>();
                    foreach (DataColumn col in masterRow.Table.Columns)
                    {
                        masterDic.Add(col.ColumnName, masterRow[col.ColumnName]);
                    }
                    ComputeAduitResult result = null;
                    foreach (var subItem in listOfNeedFindFlowRows)
                    {
                        int rowId = LibSysUtils.ToInt32(subItem["ROW_ID"]);
                        if (ret.ContainsKey(rowId))//针对每行进行检查
                        {
                            result = ComputeAduitExecutor(ret[rowId], this.Handle.PersonId, masterDic);
                            if (result.IsCanSubmitAduit)
                                ret[rowId] = result.FlowList;
                            else
                            {
                                errorInfo = result.ErrorInfo;
                                ret = null;
                                break;
                            }
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 更新审核任务
        /// </summary>
        private void UpdateApproveTask()
        {
            if (this.UpdateTaskSqlList.Count > 0)
            {
                this.DataAccess.ExecuteNonQuery(this.UpdateTaskSqlList);
            }
            if (this.ApproveRowObj.UpdateTaskSqlList.Count > 0)
            {
                this.DataAccess.ExecuteNonQuery(this.ApproveRowObj.UpdateTaskSqlList);
            }
        }

        private void SendSysNews()
        {
            string data = string.Empty;
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            StringBuilder pkBuilder = new StringBuilder();
            foreach (DataColumn col in this.DataSet.Tables[0].PrimaryKey)
            {
                LibDataType dataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                if (dataType == LibDataType.Text || dataType == LibDataType.NText)
                    pkBuilder.AppendFormat("'{0}',", masterRow[col]);
                else
                    pkBuilder.AppendFormat("{0},", masterRow[col]);
            }
            pkBuilder.Remove(pkBuilder.Length - 1, 1);
            data = string.Format("[{0}]", pkBuilder.ToString());
            if (this.Template.FuncPermission.EntryParam.Count > 0)
            {
                StringBuilder entryBuilder = new StringBuilder();
                entryBuilder.Append("{ParamStore:{");
                foreach (string entryParam in this.Template.FuncPermission.EntryParam)
                {
                    DataColumn col = this.DataSet.Tables[0].Columns[entryParam];
                    LibDataType dataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                    if (dataType == LibDataType.Text || dataType == LibDataType.NText)
                        entryBuilder.AppendFormat("{0}:'{1}',", entryParam, masterRow[col]);
                    else
                        entryBuilder.AppendFormat("{0}:{1},", entryParam, masterRow[col]);
                }
                entryBuilder.Remove(entryBuilder.Length - 1, 1);
                entryBuilder.Append("}}");
                data += string.Format(";{0}", entryBuilder.ToString());
            }
            //this.ApproveMailParam主要是整单审核时的信息通知人
            if (this.ApproveMailParam.To.Count > 0)
            {
                LibSysNews news = new LibSysNews();
                news.Content = this.ApproveMailParam.Content;
                news.Data = data;
                news.PersonId = this.ApproveMailParam.PersonId;
                news.ProgId = this.ProgId;
                news.Title = this.ApproveMailParam.Subject;
                news.UserList = this.ApproveMailParam.To;
                ThreadPool.QueueUserWorkItem(LibSysNewsHelper.SendNews, new List<LibSysNews>() { news });
            }
            //this.ApproveRowObj.MailParamList主要是行审核时的信息通知人
            if (this.ApproveRowObj.MailParamList.Count > 0)
            {
                List<LibSysNews> sysNewsList = new List<LibSysNews>();
                foreach (var item in this.ApproveRowObj.MailParamList)
                {
                    if (item.Value.MailParam.To.Count > 0)
                    {
                        LibSysNews news = new LibSysNews();
                        news.Content = item.Value.MailParam.Content;
                        news.Data = data;
                        news.PersonId = item.Value.MailParam.PersonId;
                        news.ProgId = this.ProgId;
                        news.Title = item.Value.MailParam.Subject;
                        news.UserList = item.Value.MailParam.To;
                        sysNewsList.Add(news);
                    }
                }
                ThreadPool.QueueUserWorkItem(LibSysNewsHelper.SendNews, sysNewsList);
            }
        }

        private void SendNotice()
        {
            if (this.ApproveRowObj.MailParamList.Count > 0)
            {
                foreach (var item in this.ApproveRowObj.MailParamList)
                {
                    if (item.Value.MailParam.To.Count > 0)
                    {
                        StringBuilder builder = new StringBuilder();
                        foreach (var rowId in item.Value.RowIdList)
                        {
                            builder.AppendFormat("{0},", rowId);
                        }
                        if (builder.Length > 0)
                            builder.Remove(builder.Length - 1, 1);
                        item.Value.MailParam.Content = string.Format(item.Value.MailParam.Content, builder.ToString());
                    }
                }
            }
            SendSysNews();
            if (UsingMail)
            {
                if (this.ApproveMailParam.To.Count > 0)
                {
                    ThreadPool.QueueUserWorkItem(LibMailHelper.SendMail, new List<LibMailParam>() { this.ApproveMailParam });
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
                    ThreadPool.QueueUserWorkItem(LibMailHelper.SendMail, list);
                }
            }
            //发送移动端推送消息 Zhangkj 20170606
            SendApproveAppPush();
            //发送短信和微信 Zhangkj 20170612
            SendSMS();
            SendWeixin();
        }

        /// <summary>
        /// 处理行项审核通过\审核不通过
        /// </summary>
        /// <param name="records"></param>
        /// <param name="isPass"></param>
        protected virtual void DealWithApproveRow(List<DataRow> records, bool isPass)
        {

        }

        private DataTable GetApproveRowTable()
        {
            DataTable curTable = null;
            for (int i = 1; i < this.DataSet.Tables.Count; i++)
            {
                if (this.DataSet.Tables[i].ExtendedProperties.ContainsKey(TableProperty.UsingApproveRow))
                {
                    if ((bool)this.DataSet.Tables[i].ExtendedProperties[TableProperty.UsingApproveRow])
                    {
                        curTable = this.DataSet.Tables[i];
                        break;
                    }
                }
            }
            return curTable;
        }

        public DataSet SubmitApproveRow(object[] pks, bool cancel, Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> rowList)
        {
            string cacheKey = GetDataThenToCache(pks);
            try
            {
                bool check = true;
                DataTable curTable = GetApproveRowTable();
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                Dictionary<string, LibChangeRecord> changeRecord = new Dictionary<string, LibChangeRecord>();
                changeRecord.Add(curTable.TableName, new LibChangeRecord());
                LibChangeRecord curChangeRecord = changeRecord[curTable.TableName];
                string internalId = LibSysUtils.ToString(masterRow["INTERNALID"]);

                // 获取主键值
                string billNo = LibSysUtils.ToString(masterRow[this.DataSet.Tables[0].PrimaryKey[0]]);
                if (cancel)
                {
                    this.BillAction = BillAction.WithdrawApproveRow;
                    foreach (var item in rowList)
                    {
                        DataRow curRow = curTable.Rows.Find(GetPkCondition(masterRow, item.Key));
                        if (curRow != null)
                        {
                            check = CheckWithdrawAuditRow(curRow);
                            if (check)
                            {
                                Dictionary<string, object> temp = new Dictionary<string, object>();
                                foreach (DataColumn col in curTable.PrimaryKey)
                                {
                                    temp.Add(string.Format("_{0}", col.ColumnName), curRow[col]);
                                }
                                temp.Add("AUDITSTATE", LibAuditState.UnSubmit);
                                curChangeRecord.Modif.Add(temp);
                                int rowId = LibSysUtils.ToInt32(curRow["ROW_ID"]);
                                int curLevel = LibSysUtils.ToInt32(masterRow["FLOWLEVEL"]);
                                if (curLevel == 0)
                                {
                                    this.UpdateTaskSqlList.Add(string.Format("delete AXPAPPROVETASK where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2}", this.ProgId, masterRow["INTERNALID"], rowId));
                                }
                                List<string> personList = new List<string>();
                                this.FillRowFlowPerson(rowId, personList, string.Format("(FLOWLEVEL={0} or FLOWLEVEL={1})", curLevel + 1, curLevel));
                                string key = GetApproveRowTempKey(personList);
                                if (!this.ApproveRowObj.MailParamList.ContainsKey(key))
                                {
                                    this.ApproveRowObj.MailParamList.Add(key, new ApproveRowMail(this.ProgId, this.Handle.PersonId));
                                    LibMailParam param = this.ApproveRowObj.MailParamList[key].MailParam;
                                    param.To = personList;
                                    string keyStr = LibSysUtils.ToString(GetLastPkValue(masterRow));
                                    param.BillNo = keyStr;
                                    param.Subject = string.Format("《{0}》{1}已撤回行项审核", this.Template.DisplayText, keyStr);
                                    param.Content = string.Format("《{0}》{1}已撤回行项审核，撤回的行标识号为{2}", this.Template.DisplayText, keyStr, "{0}");
                                }
                                this.ApproveRowObj.MailParamList[key].RowIdList.Add(rowId);
                            }
                        }
                    }
                }
                else
                {
                    this.BillAction = BillAction.SubmitApproveRow;
                    StringBuilder builder = new StringBuilder();
                    HashSet<int> hashSet = new HashSet<int>();
                    foreach (var item in rowList)
                    {
                        builder.AppendFormat("FROMROWID={0} OR ", item.Key);
                        hashSet.Add(item.Key);
                    }
                    if (builder.Length > 0)
                    {
                        builder.Remove(builder.Length - 3, 3);
                        string sql = string.Format("select distinct FROMROWID from AXPAPPROVETASK where PROGID='{0}' and INTERNALID='{1}' and ({2})",
                                                             this.ProgId, internalId, builder.ToString());
                        using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
                        {
                            while (reader.Read())
                            {
                                int rowId = LibSysUtils.ToInt32(reader["FROMROWID"]);
                                if (hashSet.Contains(rowId))
                                    hashSet.Remove(rowId);
                            }
                        }
                    }
                    foreach (var item in rowList)
                    {
                        bool changeFlow = item.Value != null && item.Value.Count > 0;//判断某个行是否重新指定了审核流程
                        //检查审核流程的变更权限
                        //增加权限判断，必须要有审核流配置权限的人才可以变更 Zhangkj 20170324       
                        if (changeFlow && LibPermissionControl.Default.HasPermission(this.Handle, "axp.ApproveFlow", FuncPermissionEnum.Edit) == false)
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前操作人员无权变更此单据行审核的审核流程。行号:" + item.Key.ToString());
                            break;
                        }
                        DataRow curRow = curTable.Rows.Find(GetPkCondition(masterRow, item.Key));
                        if (curRow != null)
                        {
                            check = CheckSubmitAuditRow(curRow);
                            if (check)
                            {
                                Dictionary<string, object> temp = new Dictionary<string, object>();
                                foreach (DataColumn col in curTable.PrimaryKey)
                                {
                                    temp.Add(string.Format("_{0}", col.ColumnName), curRow[col]);
                                }
                                temp.Add("AUDITSTATE", LibAuditState.Submit);
                                if (curTable.Columns.Contains("SUMMITAUDITTIME"))
                                {
                                    //Zhangkj 20170609 增加行审核提交时间
                                    temp.Add("SUMMITAUDITTIME", LibDateUtils.GetCurrentDateTime());
                                }
                                curChangeRecord.Modif.Add(temp);
                            }
                            SortedList<int, List<LibApproveFlowInfo>> flowList = null;
                            int curLevel = LibSysUtils.ToInt32(curRow["FLOWLEVEL"]);
                            bool isFoundNotJump = false;//是否找到了不能跳过的行
                            if (hashSet.Contains(item.Key))
                            {
                                string errorInfo = string.Empty;
                                flowList = changeFlow ? item.Value : GetApproveFlowByDataRow(masterRow, curRow, out errorInfo);
                                if (flowList == null || (changeFlow == false && string.IsNullOrEmpty(errorInfo) == false))
                                {
                                    if (string.IsNullOrEmpty(errorInfo) == false)
                                        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("查找到的审核过程为空,错误消息:{0}。", errorInfo));
                                    else
                                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "查找到的审核过程为空。");
                                    break;
                                }
                                else
                                {
                                    //检查全是跳过的行
                                    foreach (var flowInfo in flowList)
                                    {
                                        foreach (var info in flowInfo.Value)
                                        {
                                            if (info.IsJump == false)
                                            {
                                                isFoundNotJump = true;
                                                break;
                                            }
                                        }
                                        if (isFoundNotJump)
                                            break;
                                    }
                                    if (isFoundNotJump == false)
                                    {
                                        //全是跳过行
                                        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("本次提交审核中的行号:{0}，全是跳过的审核过程。请检查审核流配置。", item.Key));
                                        break;
                                    }
                                    foreach (var flowInfo in flowList)
                                    {
                                        foreach (var info in flowInfo.Value)
                                        {
                                            if (HasAduitOfDuty == false)
                                            {
                                                this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("insert into AXPAPPROVETASK(PROGID,INTERNALID,FROMROWID,CURRENTLEVEL,FLOWLEVEL,SUBMITTERID,PERSONID,INDEPENDENT,AUDITSTATE,BILLNO) "
                                            + "values('{0}','{1}',{2},{3},{4},'{5}','{6}',{7},0,'{8}')", this.ProgId, internalId, item.Key, 0, flowInfo.Key, this.Handle.PersonId, info.PersonId, info.Independent ? 1 : 0, billNo));
                                            }
                                            else
                                            {
                                                //插入更多字段 部门Id、岗位Id、是否跳过、跳过原因
                                                this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format(
                                                    "insert into AXPAPPROVETASK(AUDITTASKID,PROGID,INTERNALID,FROMROWID,CURRENTLEVEL,FLOWLEVEL,SUBMITTERID,PERSONID,INDEPENDENT,AUDITSTATE,BILLNO,DEPTID,DUTYID,ISJUMP,JUMPREASON,EXECUTEDESC) "
                                                     + "values('{14}','{0}','{1}',{2},{3},{4},'{5}','{6}',{7},0,'{8}', " +
                                                     " '{9}','{10}','{11}','{12}',{13})",
                                                    this.ProgId, internalId, item.Key, 0, flowInfo.Key, this.Handle.PersonId, info.PersonId, info.Independent ? 1 : 0, billNo,
                                                    info.DeptId, info.DutyId, info.IsJump, info.JumpReason, LibStringBuilder.GetQuotString(string.Format("{0}", info.ExecuteDesc)),
                                                    Guid.NewGuid().ToString() //使用Guid作为主键
                                                    ));
                                            }
                                        }
                                    }
                                }
                            }
                            else if (changeFlow)
                            {
                                //重新指定审核流程时不会有跳过的行，因此不用再检查
                                string levelConditon = curLevel > 0 ? string.Format(" and FLOWLEVEL>{0}", curLevel) : string.Empty;
                                this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("delete AXPAPPROVETASK where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2} {3}",
                                     this.ProgId, internalId, item.Key, levelConditon));
                                flowList = item.Value;
                                foreach (var flowInfo in flowList)
                                {
                                    if (flowInfo.Key <= curLevel)
                                        continue;
                                    foreach (var info in flowInfo.Value)
                                    {
                                        if (HasAduitOfDuty == false)
                                        {
                                            this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("insert into AXPAPPROVETASK(PROGID,INTERNALID,FROMROWID,CURRENTLEVEL,FLOWLEVEL,SUBMITTERID,PERSONID,INDEPENDENT,AUDITSTATE,BILLNO) "
                                            + "values('{0}','{1}',{2},{3},{4},'{5}','{6}',{7},0,'{8}')", this.ProgId, internalId, item.Key, curLevel, flowInfo.Key, this.Handle.PersonId, info.PersonId, info.Independent ? 1 : 0, billNo));
                                        }
                                        else
                                        {
                                            // 插入更多字段 部门Id、岗位Id、是否跳过、跳过原因
                                            this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format(
                                                "insert into AXPAPPROVETASK(AUDITTASKID,PROGID,INTERNALID,FROMROWID,CURRENTLEVEL,FLOWLEVEL,SUBMITTERID,PERSONID,INDEPENDENT,AUDITSTATE,BILLNO,DEPTID,DUTYID,ISJUMP,JUMPREASON,EXECUTEDESC) "
                                                    + "values('{14}','{0}','{1}',{2},{3},{4},'{5}','{6}',{7},0,'{8}', " +
                                                    " '{9}','{10}','{11}','{12}',{13})",
                                                this.ProgId, internalId, item.Key, 0, flowInfo.Key, this.Handle.PersonId, info.PersonId, info.Independent ? 1 : 0, billNo,
                                                info.DeptId, info.DutyId, info.IsJump, info.JumpReason, LibStringBuilder.GetQuotString(string.Format("{0}", info.ExecuteDesc)),
                                                Guid.NewGuid().ToString() //使用Guid作为主键
                                                ));
                                        }
                                    }
                                }
                            }
                            int rowId = LibSysUtils.ToInt32(curRow["ROW_ID"]);
                            List<string> personList = new List<string>();
                            if (flowList == null)
                            {
                                this.FillRowFlowPerson(rowId, personList, string.Format("(FLOWLEVEL={0} or FLOWLEVEL={1})", curLevel + 1, curLevel));
                            }
                            else
                            {
                                int upLevel = curLevel + 1;
                                foreach (var flowInfo in flowList)
                                {
                                    if (flowInfo.Key > upLevel)
                                        break;
                                    if (flowInfo.Key == curLevel || flowInfo.Key == upLevel)
                                    {
                                        foreach (var info in flowInfo.Value)
                                        {
                                            if (!string.IsNullOrEmpty(info.PersonId))
                                                personList.Add(info.PersonId);
                                        }
                                    }
                                }
                            }

                            //// 修改当前审核级别之后的审核状态为0  审核不通过时已处理
                            //this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AUDITSTATE=0 where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2} and FLOWLEVEL>{3}", this.ProgId, internalId, rowId, curLevel));


                            string key = GetApproveRowTempKey(personList);
                            if (!this.ApproveRowObj.MailParamList.ContainsKey(key))
                            {
                                this.ApproveRowObj.MailParamList.Add(key, new ApproveRowMail(this.ProgId, this.Handle.PersonId));
                                LibMailParam param = this.ApproveRowObj.MailParamList[key].MailParam;
                                param.To = personList;
                                string keyStr = LibSysUtils.ToString(GetLastPkValue(masterRow));
                                param.BillNo = keyStr;
                                param.Subject = string.Format("《{0}》{1}已提交行项审核", this.Template.DisplayText, keyStr);
                                param.Content = string.Format("《{0}》{1}已提交行项审核,提交的行标识号为{2}", this.Template.DisplayText, keyStr, "{0}");
                            }
                            this.ApproveRowObj.MailParamList[key].RowIdList.Add(rowId);
                        }
                    }
                }
                if (check)
                {
                    this.SetSystemFieldValue(this.DataSet.Tables[0].Rows[0]);
                    this.Save(this.BillAction, pks, changeRecord, null);
                }
            }
            finally
            {
                LibBillDataCache.Default.Remove(cacheKey);
            }
            return this.DataSet;
        }
        /// <summary>
        /// 判断是否进行到下一层级
        /// </summary>
        /// <param name="nextLevel"></param>
        /// <param name="flowList"></param>
        /// <returns></returns>
        private bool IsUpLevel(int nextLevel, SortedList<int, List<LibApproveFlowInfo>> flowList)
        {
            bool upLevel = false;
            bool allPass = true;
            foreach (var flowInfo in flowList[nextLevel])
            {
                if (flowInfo.Independent && flowInfo.IsPass)
                {
                    upLevel = true;
                    break;
                }
                if (!flowInfo.IsPass && flowInfo.IsJump == false && allPass)
                    allPass = false;
            }
            if (!upLevel && allPass)
                upLevel = true;
            return upLevel;
        }

        public DataSet AuditRow(object[] pks, bool isPass, Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> rowList, Dictionary<int, int> unPassLevel)
        {
            this.BillAction = isPass ? BillAction.ApprovePassRow : BillAction.ApproveUnPassRow;
            string cacheKey = GetDataThenToCache(pks);
            try
            {
                bool check = true;
                DataTable curTable = GetApproveRowTable();
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];

                // 获取主键值
                string billNo = LibSysUtils.ToString(masterRow[this.DataSet.Tables[0].PrimaryKey[0]]);
                Dictionary<string, LibChangeRecord> changeRecord = new Dictionary<string, LibChangeRecord>();
                changeRecord.Add(curTable.TableName, new LibChangeRecord());
                LibChangeRecord curChangeRecord = changeRecord[curTable.TableName];
                string curPersonId = this.Handle.PersonId;
                string internalId = LibSysUtils.ToString(masterRow["INTERNALID"]);
                foreach (var item in rowList)
                {
                    bool changeFlow = item.Value != null && item.Value.Count > 0;
                    //检查审核流程的变更权限
                    //增加权限判断，必须要有审核流配置权限的人才可以变更 Zhangkj 20170324       
                    if (changeFlow && LibPermissionControl.Default.HasPermission(this.Handle, "axp.ApproveFlow", FuncPermissionEnum.Edit) == false)
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前操作人员无权变更此单据行审核的审核流程。行号:" + item.Key.ToString());
                        break;
                    }

                    DataRow curRow = curTable.Rows.Find(GetPkCondition(masterRow, item.Key));
                    if (curRow != null)
                    {
                        check = CheckAuditRow(curRow);
                        if (check)
                        {
                            Dictionary<string, object> temp = new Dictionary<string, object>();
                            foreach (DataColumn col in curTable.PrimaryKey)
                            {
                                temp.Add(string.Format("_{0}", col.ColumnName), curRow[col]);
                            }
                            SortedList<int, List<LibApproveFlowInfo>> flowList = null;

                            string errorInfo = string.Empty;
                            if (changeFlow)
                            {
                                flowList = item.Value;
                            }
                            else
                            {
                                flowList = this.GetApproveFlowByDataRow(masterRow, curRow, out errorInfo);
                            }
                            LibApproveFlowInfo theFoundFlowInfo = null;//被找到的审核过程。
                            if (flowList == null || (changeFlow == false && string.IsNullOrEmpty(errorInfo) == false))
                            {
                                if (string.IsNullOrEmpty(errorInfo) == false)
                                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("查找到的审核过程为空,错误消息:{0}。", errorInfo));
                                else
                                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "查找到的审核过程为空。");
                                break;
                            }
                            int curLevel = LibSysUtils.ToInt32(curRow["FLOWLEVEL"]);
                            //首先处理一个级别下只有跳过的审核过程的情况。因提交审核时已过滤了全是跳过行的情况，此处不用再考虑
                            bool isFoundNotJumpRow = false;
                            while (isFoundNotJumpRow == false)
                            {
                                foreach (var flowInfo in flowList[(curLevel + 1)])
                                {
                                    if (flowInfo.IsJump == false)
                                    {
                                        isFoundNotJumpRow = true;
                                        break;
                                    }
                                }
                                if (isFoundNotJumpRow)
                                    break;
                                if (flowList.Keys[flowList.Keys.Count - 1] == curLevel + 1)
                                    break;//已经是最后一级
                                curLevel++;
                            }
                            bool isFind = false;
                            foreach (var flowInfo in flowList[(curLevel + 1)])
                            {
                                if (string.Compare(flowInfo.PersonId, curPersonId, true) == 0)
                                {
                                    isFind = true;
                                    flowInfo.IsPass = isPass;
                                    flowInfo.AuditState = isPass ? 1 : 2;
                                    theFoundFlowInfo = flowInfo;
                                    if (HasAduitOfDuty == false)
                                    {
                                        this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AuditState={5} where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2} and CURRENTLEVEL={3} and PERSONID='{4}'",
                                       this.ProgId, internalId, item.Key, curLevel, flowInfo.PersonId, flowInfo.AuditState));
                                    }
                                    else
                                    {
                                        this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AuditState={1} where AUDITTASKID='{0}'",
                                       flowInfo.AuditTaskId, flowInfo.AuditState));
                                    }
                                    break;
                                }
                            }
                            if (isFind)
                            {
                                if (isPass)
                                {
                                    bool isUpLevel = IsUpLevel(curLevel + 1, flowList);
                                    if (isUpLevel)
                                    {
                                        int upLevel = curLevel + 1;
                                        temp.Add("FLOWLEVEL", upLevel);
                                        bool isLastLevel = flowList.Keys[flowList.Keys.Count - 1] == upLevel;

                                        if (isLastLevel)
                                        {
                                            temp.Add("AUDITSTATE", LibAuditState.Pass);
                                            if (curTable != null && curTable.Columns.Contains("APPROVRID"))
                                            {
                                                temp.Add("APPROVRID", this.Handle.PersonId);
                                                temp.Add("APPROVRNAME", this.Handle.PersonName);
                                                temp.Add("APPROVALTIME", LibDateUtils.GetCurrentDateTime());
                                            }
                                            this.ApproveRowObj.Records.Add(curRow);
                                        }
                                        else
                                        {
                                            while (IsUpLevel(upLevel + 1, flowList))//因后面的行可能是跳过行，先进行预先检查
                                            {
                                                upLevel++;
                                                isLastLevel = flowList.Keys[flowList.Keys.Count - 1] == upLevel;
                                                if (isLastLevel)
                                                    break;
                                            }
                                            if ((LibAuditState)LibSysUtils.ToInt32(curRow["AUDITSTATE"]) != LibAuditState.Submit)
                                                temp.Add("AUDITSTATE", LibAuditState.Submit);
                                        }
                                        if (isLastLevel)
                                        {
                                            //再判断一次是否为最后一层
                                            if (temp.ContainsKey("AUDITSTATE") == false)
                                            {
                                                temp.Add("AUDITSTATE", LibAuditState.Pass);
                                                if (curTable != null && curTable.Columns.Contains("APPROVRID"))
                                                {
                                                    temp.Add("APPROVRID", this.Handle.PersonId);
                                                    temp.Add("APPROVRNAME", this.Handle.PersonName);
                                                    temp.Add("APPROVALTIME", LibDateUtils.GetCurrentDateTime());
                                                }
                                            }
                                            else
                                            {
                                                temp["AUDITSTATE"] = LibAuditState.Pass;
                                                if (curTable != null && curTable.Columns.Contains("APPROVRID"))
                                                {
                                                    temp["APPROVRID"] = this.Handle.PersonId;
                                                    temp["APPROVRNAME"] = this.Handle.PersonName;
                                                    temp["APPROVALTIME"] = LibDateUtils.GetCurrentDateTime();
                                                }
                                            }
                                            if (this.ApproveRowObj.Records.Contains(curRow) == false)
                                                this.ApproveRowObj.Records.Add(curRow);
                                        }

                                        this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set CURRENTLEVEL={3} where  PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2}", this.ProgId, internalId, item.Key, upLevel));

                                        int rowId = LibSysUtils.ToInt32(curRow["ROW_ID"]);
                                        string flowLevelConditon = string.Empty;
                                        List<string> personList = new List<string>();
                                        List<string> submitterLIst = new List<string>();
                                        string text = string.Empty;
                                        string content = string.Empty;
                                        string keyStr = LibSysUtils.ToString(GetLastPkValue(masterRow));
                                        if (isLastLevel)
                                        {
                                            flowLevelConditon = "FLOWLEVEL>0";
                                            text = string.Format("《{0}》{1}行项已审核通过", this.Template.DisplayText, keyStr);
                                            content = string.Format("《{0}》{1}行项已审核通过,审核通过的行标识号为{2}", this.Template.DisplayText, keyStr, "{0}");
                                            if (changeFlow)
                                            {
                                                foreach (var flowInfo in flowList)
                                                {
                                                    foreach (var info in flowInfo.Value)
                                                    {
                                                        if (!string.IsNullOrEmpty(info.SubmitterId))
                                                            personList.Add(info.SubmitterId);
                                                    }
                                                }
                                            }
                                            else
                                                this.FillRowFlowPerson(rowId, personList, flowLevelConditon);
                                        }
                                        else
                                        {
                                            string nextPersonStr = string.Empty;
                                            if (changeFlow)
                                            {
                                                StringBuilder builder = new StringBuilder();
                                                foreach (var flowInfo in flowList)
                                                {
                                                    if (flowInfo.Key > upLevel)
                                                        break;
                                                    if (flowInfo.Key == curLevel || flowInfo.Key == upLevel || flowInfo.Key == upLevel + 1)
                                                    {
                                                        foreach (var info in flowInfo.Value)
                                                        {
                                                            if (flowInfo.Key == upLevel + 1)
                                                            {
                                                                builder.AppendFormat("{0},", info.PersonName);
                                                            }
                                                            if (!string.IsNullOrEmpty(info.PersonId) && !personList.Contains(info.PersonId))
                                                                personList.Add(info.PersonId);
                                                        }
                                                    }

                                                }
                                                if (builder.Length > 1)
                                                    builder.Remove(builder.Length - 1, 1);
                                                nextPersonStr = builder.ToString();
                                            }
                                            else
                                            {
                                                if (HasAduitOfDuty == false)
                                                {
                                                    flowLevelConditon = string.Format("(A.FLOWLEVEL={0} or A.FLOWLEVEL={1} or A.FLOWLEVEL={2})", upLevel, curLevel, upLevel + 1);
                                                }
                                                else
                                                {
                                                    flowLevelConditon = string.Format("(A.FLOWLEVEL={0} or A.FLOWLEVEL={1} or A.FLOWLEVEL={2}) and A.PERSONID != '{3}'", upLevel, curLevel, upLevel + 1, curPersonId);
                                                }

                                                nextPersonStr = FillRowFlowPerson(rowId, personList, flowLevelConditon, upLevel + 1);
                                            }
                                            text = string.Format("《{0}》{1}行项已在审核层级{2}上审核通过", this.Template.DisplayText, keyStr, upLevel);
                                            content = string.Format("《{0}》{1}行项已审核通过,审核通过的行标识号为{2},请{3}进行第{4}层级的审核", this.Template.DisplayText, keyStr, "{0}", nextPersonStr, upLevel + 1);
                                        }
                                        if (isLastLevel)
                                        {
                                            string creatorId = LibSysUtils.ToString(masterRow["CREATORID"]);
                                            if (!string.IsNullOrEmpty(creatorId) && !personList.Contains(creatorId))
                                                personList.Add(creatorId);
                                        }
                                        string key = this.GetApproveRowTempKey(personList);
                                        if (!this.ApproveRowObj.MailParamList.ContainsKey(key))
                                        {
                                            this.ApproveRowObj.MailParamList.Add(key, new ApproveRowMail(this.ProgId, this.Handle.PersonId));
                                            LibMailParam parem = this.ApproveRowObj.MailParamList[key].MailParam;
                                            parem.To = personList;
                                            parem.BillNo = keyStr;
                                            parem.Subject = text;
                                            parem.Content = content;
                                        }
                                        this.ApproveRowObj.MailParamList[key].RowIdList.Add(rowId);

                                        //同人默认的操作，同一个审核人如果在本步骤有同意的意见，则后续都是同意的 Zhangkj 20170328           
                                        #region 同人默认的相关处理 Zhangkj 20170328
                                        if (HasAduitOfDuty && isLastLevel == false)//必须是有部门岗位设置相关的字段时
                                        {
                                            personList = new List<string>();
                                            //如果设置了同人默认，首先在内存中设置大于本级的同一个人的审核审核都同意
                                            bool findOtherSamePersonRow = false;
                                            foreach (var level in flowList.Keys)
                                            {
                                                foreach (var flowInfo in flowList[level])
                                                {
                                                    if (string.Compare(flowInfo.PersonId, curPersonId, true) == 0
                                                        && flowInfo.AuditState != (int)LibAuditState.Pass
                                                        && flowInfo.FlowLevel >= curLevel
                                                        && flowInfo.IsSameDefault
                                                        && flowInfo.IsJump == false
                                                        && flowInfo != theFoundFlowInfo)
                                                    {
                                                        findOtherSamePersonRow = true;
                                                        flowInfo.IsPass = isPass;
                                                        flowInfo.AuditState = isPass ? 1 : 2;
                                                        this.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AuditState={1},EXECUTEDESC='{2}' " +
                                                            " where AUDITTASKID='{0}'",
                                                            flowInfo.AuditTaskId, flowInfo.AuditState, "同人默认"));//同人默认
                                                    }
                                                }
                                            }
                                            if (findOtherSamePersonRow)//通过同人默认设置了一些行的状态
                                            {
                                                if (isUpLevel)
                                                    curLevel++;//如果需要晋级则判断下一层
                                                //查找如果执行了同人默认，单据的下一审核层级和审核状态应该是什么                          
                                                int maxUpLevel = int.MinValue;
                                                string thisText = string.Empty;
                                                while (true)
                                                {
                                                    isUpLevel = IsUpLevel(curLevel + 1, flowList);
                                                    if (isUpLevel)
                                                    {
                                                        int thisUpLevel = curLevel + 1;
                                                        if (thisUpLevel > maxUpLevel)
                                                            maxUpLevel = thisUpLevel;
                                                        if (temp.ContainsKey("FLOWLEVEL") == false)
                                                            temp.Add("FLOWLEVEL", thisUpLevel);
                                                        else
                                                            temp["FLOWLEVEL"] = thisUpLevel;
                                                        isLastLevel = flowList.Keys[flowList.Keys.Count - 1] == thisUpLevel;

                                                        if (isLastLevel)
                                                        {
                                                            if (temp.ContainsKey("AUDITSTATE") == false)
                                                                temp.Add("AUDITSTATE", LibAuditState.Pass);
                                                            else
                                                            {
                                                                temp["AUDITSTATE"] = LibAuditState.Pass;
                                                            }
                                                            if (this.ApproveRowObj.Records.Contains(curRow) == false)
                                                                this.ApproveRowObj.Records.Add(curRow);

                                                            flowLevelConditon = "FLOWLEVEL>0";
                                                            thisText = string.Format("《{0}》{1}行项已审核通过", this.Template.DisplayText, keyStr);
                                                            content = string.Format("《{0}》{1}行项已审核通过,审核通过的行标识号为{2}", this.Template.DisplayText, keyStr, "{0}");
                                                            if (changeFlow)
                                                            {
                                                                foreach (var flowInfo in flowList)
                                                                {
                                                                    foreach (var info in flowInfo.Value)
                                                                    {
                                                                        if (!string.IsNullOrEmpty(info.SubmitterId))
                                                                            personList.Add(info.SubmitterId);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                                this.FillRowFlowPerson(rowId, personList, flowLevelConditon);

                                                            string creatorId = LibSysUtils.ToString(masterRow["CREATORID"]);
                                                            if (!string.IsNullOrEmpty(creatorId) && !personList.Contains(creatorId))
                                                                personList.Add(creatorId);

                                                            break;//是最后一层了，终止循环
                                                        }
                                                        else
                                                        {
                                                            if ((LibAuditState)LibSysUtils.ToInt32(curRow["AUDITSTATE"]) != LibAuditState.Submit)
                                                            {
                                                                if (temp.ContainsKey("AUDITSTATE") == false)
                                                                    temp.Add("AUDITSTATE", LibAuditState.Submit);
                                                                else
                                                                    temp["AUDITSTATE"] = LibAuditState.Submit;
                                                            }
                                                            if (IsUpLevel(thisUpLevel + 1, flowList) == false)
                                                            {
                                                                flowLevelConditon = string.Format("(A.FLOWLEVEL={0} or A.FLOWLEVEL={1} or A.FLOWLEVEL={2}) and A.PERSONID != '{3}'", thisUpLevel, curLevel, thisUpLevel + 1, curPersonId);//对于当前用户就不再通知了
                                                                string nextPersonStr = FillRowFlowPerson(rowId, personList, flowLevelConditon, thisUpLevel + 1);
                                                                thisText = string.Format("《{0}》{1}行项已在审核层级{2}上审核通过", this.Template.DisplayText, keyStr, thisUpLevel);
                                                                content = string.Format("《{0}》{1}行项已审核通过,审核通过的行标识号为{2},请{3}进行第{4}层级的审核", this.Template.DisplayText, keyStr, "{0}", nextPersonStr, thisUpLevel + 1);
                                                                break;//下一层有需要审核的人员
                                                            }
                                                            curLevel++;//如果需要进入下一层循环，而且又不是最后一层，则继续进入下一层检查。
                                                        }
                                                    }
                                                    else
                                                    {
                                                        break;//不能进入下一层审核，则终止循环
                                                    }
                                                }
                                                if (maxUpLevel != int.MinValue)
                                                    this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set CURRENTLEVEL={3} where  PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2}", this.ProgId, internalId, item.Key, maxUpLevel));
                                                if (string.IsNullOrEmpty(thisText) == false)
                                                {
                                                    key = this.GetApproveRowTempKey(personList);
                                                    if (!this.ApproveRowObj.MailParamList.ContainsKey(key))
                                                    {
                                                        this.ApproveRowObj.MailParamList.Add(key, new ApproveRowMail(this.ProgId, this.Handle.PersonId));
                                                    }
                                                    LibMailParam parem = this.ApproveRowObj.MailParamList[key].MailParam;
                                                    parem.To = personList;
                                                    parem.BillNo = keyStr;
                                                    parem.Subject = thisText;
                                                    parem.Content = content;

                                                    if (this.ApproveRowObj.MailParamList[key].RowIdList.Contains(rowId) == false)
                                                        this.ApproveRowObj.MailParamList[key].RowIdList.Add(rowId);
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                    if (changeFlow)
                                    {
                                        int flowLevel = curLevel + 1;
                                        this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("delete AXPAPPROVETASK where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2} {3}",
                                            this.ProgId, internalId, item.Key, string.Format(" and FLOWLEVEL>{0}", flowLevel)));
                                        foreach (var flowInfo in flowList)
                                        {
                                            if (flowInfo.Key <= flowLevel)
                                                continue;
                                            foreach (var info in flowInfo.Value)
                                            {
                                                if (HasAduitOfDuty == false)
                                                {
                                                    this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("insert into AXPAPPROVETASK(PROGID,INTERNALID,FROMROWID,CURRENTLEVEL,FLOWLEVEL,SUBMITTERID,PERSONID,INDEPENDENT,AuditState,BILLNO) "
                                                     + "values('{0}','{1}',{2},{3},{4},'{5}','{6}',{7},0,'{8}')", this.ProgId, internalId, item.Key, temp["FLOWLEVEL"], flowInfo.Key, info.SubmitterId, info.PersonId, info.Independent ? 1 : 0, billNo));
                                                }
                                                else
                                                {
                                                    this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("insert into AXPAPPROVETASK(AUDITTASKID,PROGID,INTERNALID,FROMROWID,CURRENTLEVEL,FLOWLEVEL,SUBMITTERID,PERSONID,INDEPENDENT,AuditState,BILLNO) "
                                                     + "values('{9}','{0}','{1}',{2},{3},{4},'{5}','{6}',{7},0,'{8}')", this.ProgId, internalId, item.Key, temp["FLOWLEVEL"], flowInfo.Key, info.SubmitterId, info.PersonId, info.Independent ? 1 : 0, billNo,
                                                     Guid.NewGuid().ToString()//使用Guid作为主键
                                                     ));
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    temp.Add("AUDITSTATE", LibAuditState.UnPass);
                                    this.ApproveRowObj.Records.Add(curRow);
                                    int downLevel = curLevel;
                                    if (unPassLevel != null && unPassLevel.ContainsKey(item.Key))
                                    {
                                        downLevel = unPassLevel[item.Key];
                                        if (curLevel < downLevel)
                                        {
                                            if (check)
                                                check = false;
                                            this.ManagerMessage.AddMessage(LibMessageKind.Error, "退回层级大于当前审核层级。");
                                        }
                                        else
                                        {
                                            temp.Add("FLOWLEVEL", downLevel);
                                            this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set CURRENTLEVEL={3} where  PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2}", this.ProgId, internalId, item.Key, downLevel));
                                            //Zhangkj 20170327 还原在审核不通过时修改之前的审核状态，同时清空审核意见
                                            this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AuditState=0,AUDITOPINION='' where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2} and FLOWLEVEL>{3}",
                                                this.ProgId, internalId, item.Key, downLevel));
                                        }
                                    }
                                    if (!this.ManagerMessage.IsThrow)
                                    {
                                        int rowId = LibSysUtils.ToInt32(curRow["ROW_ID"]);
                                        List<string> personList = new List<string>();
                                        string flowLevelConditon = string.Format("(FLOWLEVEL<={0} and FLOWLEVEL>={1})", curLevel + 1, downLevel);
                                        this.FillFlowPerson(personList, flowLevelConditon);
                                        if (downLevel == 0)
                                        {
                                            string creatorId = LibSysUtils.ToString(masterRow["CREATORID"]);
                                            if (!string.IsNullOrEmpty(creatorId) && !ApproveMailParam.To.Contains(creatorId))
                                                personList.Add(creatorId);
                                        }
                                        string key = this.GetApproveRowTempKey(personList);
                                        if (!this.ApproveRowObj.MailParamList.ContainsKey(key))
                                        {
                                            this.ApproveRowObj.MailParamList.Add(key, new ApproveRowMail(this.ProgId, this.Handle.PersonId));
                                            LibMailParam parem = this.ApproveRowObj.MailParamList[key].MailParam;
                                            parem.To = personList;
                                            string keyStr = LibSysUtils.ToString(GetLastPkValue(masterRow));
                                            parem.BillNo = keyStr;
                                            parem.Subject = string.Format("《{0}》{1}行项已在审核层级{2}上审核不通过", this.Template.DisplayText, keyStr, curLevel + 1);
                                            parem.Content = string.Format("《{0}》{1}行项已在审核层级{2}上审核不通过,审核不通过的行标识号为{3}", this.Template.DisplayText, keyStr, curLevel + 1, "{0}");
                                        }
                                        this.ApproveRowObj.MailParamList[key].RowIdList.Add(rowId);
                                    }
                                }
                                curChangeRecord.Modif.Add(temp);
                            }
                            else
                            {
                                if (check)
                                    check = false;
                                this.ManagerMessage.AddMessage(LibMessageKind.Error, "对于提交行所处的审核阶段,当前人员没有审核权限。行号:" + item.Key.ToString());
                            }
                        }
                    }
                }
                if (check)
                {
                    this.SetSystemFieldValue(this.DataSet.Tables[0].Rows[0]);
                    this.Save(this.BillAction, pks, changeRecord, null);
                }
            }
            finally
            {
                LibBillDataCache.Default.Remove(cacheKey);
            }
            return this.DataSet;
        }

        public DataSet CancelAuditRow(object[] pks, Dictionary<int, SortedList<int, List<LibApproveFlowInfo>>> rowList, Dictionary<int, int> unPassLevel, string reasonId)
        {
            this.BillAction = BillAction.CancelApproveRow;
            string cacheKey = GetDataThenToCache(pks);
            try
            {
                bool check = true;
                string curPersonId = this.Handle.PersonId;
                DataTable curTable = GetApproveRowTable();
                DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                Dictionary<string, LibChangeRecord> changeRecord = new Dictionary<string, LibChangeRecord>();
                changeRecord.Add(curTable.TableName, new LibChangeRecord());
                LibChangeRecord curChangeRecord = changeRecord[curTable.TableName];
                string internalId = LibSysUtils.ToString(masterRow["INTERNALID"]);
                if (!string.IsNullOrEmpty(reasonId))
                    this.ApproveRowObj.ChangeReasonId = reasonId;
                foreach (var item in rowList)
                {
                    DataRow curRow = curTable.Rows.Find(GetPkCondition(masterRow, item.Key));
                    if (curRow != null)
                    {
                        check = CheckCancelAuditRow(curRow);
                        if (check)
                        {
                            bool isFind = false;
                            int curLevel = LibSysUtils.ToInt32(curRow["FLOWLEVEL"]);
                            string errorInfo = string.Empty;
                            SortedList<int, List<LibApproveFlowInfo>> flowList = GetApproveFlowByDataRow(masterRow, curRow, out errorInfo);
                            if (flowList == null || string.IsNullOrEmpty(errorInfo) == false)
                            {
                                if (string.IsNullOrEmpty(errorInfo) == false)
                                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("查找到的审核过程为空,错误消息:{0}。", errorInfo));
                                else
                                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "查找到的审核过程为空。");
                                break;
                            }
                            foreach (var flowInfo in flowList[curLevel])
                            {
                                if (string.Compare(flowInfo.PersonId, curPersonId, true) == 0)
                                {
                                    isFind = true;
                                    break;
                                }
                            }
                            if (isFind)
                            {
                                Dictionary<string, object> temp = new Dictionary<string, object>();
                                foreach (DataColumn col in curTable.PrimaryKey)
                                {
                                    temp.Add(string.Format("_{0}", col.ColumnName), curRow[col]);
                                }
                                if ((LibAuditState)LibSysUtils.ToInt32(curRow["AUDITSTATE"]) == LibAuditState.Pass)
                                {
                                    this.ApproveRowObj.Records.Add(curRow);
                                }
                                temp.Add("AUDITSTATE", LibAuditState.UnPass);
                                int downLevel = curLevel - 1;
                                if (unPassLevel != null && unPassLevel.ContainsKey(item.Key))
                                {
                                    downLevel = unPassLevel[item.Key];
                                }
                                temp.Add("FLOWLEVEL", downLevel);
                                this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set CURRENTLEVEL={3} where  PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2}", this.ProgId, internalId, item.Key, downLevel));
                                this.ApproveRowObj.UpdateTaskSqlList.Add(string.Format("update AXPAPPROVETASK set AuditState=0 where PROGID='{0}' and INTERNALID='{1}' and FROMROWID={2} and FLOWLEVEL>{3}",
                                    this.ProgId, internalId, item.Key, downLevel));
                                curChangeRecord.Modif.Add(temp);

                                int rowId = LibSysUtils.ToInt32(curRow["ROW_ID"]);
                                List<string> personList = new List<string>();
                                string flowLevelConditon = string.Format("(FLOWLEVEL<={0} and FLOWLEVEL>={1})", curLevel, downLevel);
                                this.FillRowFlowPerson(rowId, personList, flowLevelConditon);
                                if (downLevel == 0)
                                {
                                    string creatorId = LibSysUtils.ToString(masterRow["CREATORID"]);
                                    if (!string.IsNullOrEmpty(creatorId) && !ApproveMailParam.To.Contains(creatorId))
                                        personList.Add(creatorId);
                                }
                                string key = this.GetApproveRowTempKey(personList);
                                if (!this.ApproveRowObj.MailParamList.ContainsKey(key))
                                {
                                    this.ApproveRowObj.MailParamList.Add(key, new ApproveRowMail(this.ProgId, this.Handle.PersonId));
                                    LibMailParam param = this.ApproveRowObj.MailParamList[key].MailParam;
                                    param.To = personList;
                                    string keyStr = LibSysUtils.ToString(GetLastPkValue(masterRow));
                                    param.BillNo = keyStr;
                                    param.Subject = string.Format("《{0}》{1}行项已在审核层级{2}上弃审退回", this.Template.DisplayText, keyStr, curLevel);
                                    param.Content = string.Format("《{0}》{1}行项已在审核层级{2}上弃审退回，弃审的行标识号为{3}", this.Template.DisplayText, keyStr, curLevel, "{0}");
                                }
                                this.ApproveRowObj.MailParamList[key].RowIdList.Add(rowId);
                            }
                            else
                            {
                                if (check)
                                    check = false;
                                this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前人员没有弃审权限。");
                            }
                        }
                    }
                }
                if (check)
                {
                    this.SetSystemFieldValue(this.DataSet.Tables[0].Rows[0]);
                    this.Save(this.BillAction, pks, changeRecord, null);
                }
            }
            finally
            {
                LibBillDataCache.Default.Remove(cacheKey);
            }
            return this.DataSet;
        }


        private class ApproveRowMail
        {
            private List<int> _RowIdList;
            private LibMailParam _MailParam;

            public ApproveRowMail(string progId, string personId)
            {
                _MailParam = new LibMailParam();
                _MailParam.ProgId = progId;
                _MailParam.PersonId = personId;
                _MailParam.MailKind = LibMailKind.Approve;
            }

            public LibMailParam MailParam
            {
                get { return _MailParam; }
                set { _MailParam = value; }
            }

            public List<int> RowIdList
            {
                get
                {
                    if (_RowIdList == null)
                        _RowIdList = new List<int>();
                    return _RowIdList;
                }
            }
        }

        private class ApproveRowObject
        {
            private string _ChangeReasonId = string.Empty;
            private List<DataRow> _Records = null;
            private List<string> _UpdateTaskSqlList = null;
            private Dictionary<string, ApproveRowMail> _MailParamList = null;


            public string ChangeReasonId
            {
                get { return _ChangeReasonId; }
                set { _ChangeReasonId = value; }
            }
            /// <summary>
            /// 主要用于行审核相关的消息通知
            /// </summary>
            public Dictionary<string, ApproveRowMail> MailParamList
            {
                get
                {
                    if (_MailParamList == null)
                        _MailParamList = new Dictionary<string, ApproveRowMail>();
                    return _MailParamList;
                }
            }
            /// <summary>
            /// 需更新审核任务的语句集合
            /// </summary>
            public List<string> UpdateTaskSqlList
            {
                get
                {
                    if (_UpdateTaskSqlList == null)
                        _UpdateTaskSqlList = new List<string>();
                    return _UpdateTaskSqlList;
                }
            }
            /// <summary>
            /// 需考虑过账的记录
            /// </summary>
            public List<DataRow> Records
            {
                get
                {
                    if (_Records == null)
                        _Records = new List<DataRow>();
                    return _Records;
                }
            }
        }

        #endregion

        #region [审核变更相关]


        public List<LibBillDataVersion> GetBillVersionList(string internalId)
        {
            List<LibBillDataVersion> list = new List<LibBillDataVersion>();
            SqlBuilder builder = new SqlBuilder("axp.ApproveDataVersion");
            string sql = builder.GetQuerySql(0, "A.CREATETIME,A.REASONID,A.REASONNAME", string.Format("A.INTERNALID={0} and A.FROMROWID=0",
                LibStringBuilder.GetQuotString(internalId)), "A.CREATETIME");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql, false))
            {
                int i = 0;
                while (reader.Read())
                {
                    LibBillDataVersion versionData = new LibBillDataVersion();
                    versionData.InternalId = internalId;
                    versionData.CreateTime = LibSysUtils.ToInt64(reader["CREATETIME"]);
                    versionData.ReasonId = LibSysUtils.ToString(reader["REASONID"]);
                    versionData.ReasonName = LibSysUtils.ToString(reader["REASONNAME"]);
                    versionData.Version = string.Format("V{0}", ++i);
                    list.Add(versionData);
                }
            }
            return list;
        }

        public DataSet BrowseToVersion(string internalId, long createTime)
        {
            string sql = string.Format("select VERSIONDATA from AXPAPPROVEDATAVERSION where INTERNALID={0} and CREATETIME={1}", LibStringBuilder.GetQuotString(internalId), createTime);
            string versionData = LibSysUtils.ToString(this.DataAccess.ExecuteScalar(sql, false));
            if (!string.IsNullOrEmpty(versionData))
            {
                LibBillDataSerializeHelper.Deserialize(versionData, this.DataSet);
            }
            this.DataSet.AcceptChanges();
            if (this.Template.FuncPermission.UsingDynamicColumn)
            {
                DoFillDataDynamicTable();
            }
            return this.DataSet;
        }

        public List<LibBillDataRowVersion> GetApproveRowVersion(string internalId, int rowId)
        {
            List<LibBillDataRowVersion> list = new List<LibBillDataRowVersion>();
            SqlBuilder builder = new SqlBuilder("axp.ApproveDataVersion");
            string sql = builder.GetQuerySql(0, "A.CREATETIME,A.REASONID,A.REASONNAME,A.VERSIONDATA", string.Format("A.INTERNALID={0} and A.FROMROWID={1}",
                LibStringBuilder.GetQuotString(internalId), rowId), "A.CREATETIME");
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql))
            {
                int i = 0;
                while (reader.Read())
                {
                    LibBillDataRowVersion newVersion = new LibBillDataRowVersion();
                    newVersion.InternalId = internalId;
                    newVersion.RowId = rowId;
                    newVersion.CreateTime = LibSysUtils.ToInt64(reader["CREATETIME"]);
                    newVersion.ReasonId = LibSysUtils.ToString(reader["REASONID"]);
                    newVersion.ReasonName = LibSysUtils.ToString(reader["REASONNAME"]);
                    newVersion.Version = string.Format("V{0}", ++i);
                    newVersion.BillDataRow = JsonConvert.DeserializeObject(LibSysUtils.ToString(reader["VERSIONDATA"]), typeof(LibBillDataRow)) as LibBillDataRow;
                    list.Add(newVersion);
                }
            }
            return list;
        }
        #endregion


    }
    /// <summary>
    /// 异动记录
    /// </summary>
    public class LibChangeRecord
    {
        private IList<Dictionary<string, object>> _Add = null;
        public IList<Dictionary<string, object>> Add
        {
            get
            {
                if (_Add == null)
                    _Add = new List<Dictionary<string, object>>();
                return _Add;
            }
            set { _Add = value; }
        }
        private IList<Dictionary<string, object>> _Modif = null;
        public IList<Dictionary<string, object>> Modif
        {
            get
            {
                if (_Modif == null)
                    _Modif = new List<Dictionary<string, object>>();
                return _Modif;
            }
            set { _Modif = value; }
        }
        private IList<Dictionary<string, object>> _Remove = null;
        public IList<Dictionary<string, object>> Remove
        {
            get
            {
                if (_Remove == null)
                    _Remove = new List<Dictionary<string, object>>();
                return _Remove;
            }
            set { _Remove = value; }
        }
    }
    /// <summary>
    /// 单据操作
    /// </summary>
    public enum BillAction
    {
        /// <summary>
        /// 浏览
        /// </summary>
        Browse = 0,
        /// <summary>
        /// 另存为草稿
        /// </summary>
        SaveToDraft = 1,
        /// <summary>
        /// 新增
        /// </summary>
        AddNew = 2,
        /// <summary>
        /// 修改
        /// </summary>
        Modif = 3,
        /// <summary>
        /// 删除
        /// </summary>
        Delete = 4,
        /// <summary>
        /// 生效
        /// </summary>
        Release = 5,
        /// <summary>
        /// 作废
        /// </summary>
        Invalid = 6,
        /// <summary>
        /// 审核通过
        /// </summary>
        AuditPass = 7,
        /// <summary>
        /// 审核不通过
        /// </summary>
        AuditUnPass = 8,
        /// <summary>
        /// 弃审
        /// </summary>
        CancelAudit = 9,
        /// <summary>
        /// 结案
        /// </summary>
        EndCase = 10,
        /// <summary>
        /// 取消结案
        /// </summary>
        CancelEndCase = 11,
        /// <summary>
        /// 草稿提交
        /// </summary>
        SubmitDraft = 12,
        /// <summary>
        /// 取消作废
        /// </summary>
        CancelInvalid = 13,
        /// <summary>
        /// 取消生效
        /// </summary>
        CancelRelease = 14,
        /// <summary>
        /// 提交审核
        /// </summary>
        SubmitAudit = 15,
        /// <summary>
        /// 撤回提交的审核
        /// </summary>
        WithdrawAudit = 16,
        /// <summary>
        /// 审核行项通过
        /// </summary>
        ApprovePassRow = 17,
        /// <summary>
        /// 审核行项不通过
        /// </summary>
        ApproveUnPassRow = 18,
        /// <summary>
        /// 弃审行项
        /// </summary>
        CancelApproveRow = 19,
        /// <summary>
        /// 提交审核行项
        /// </summary>
        SubmitApproveRow = 20,
        /// <summary>
        /// 撤回提交审核的行
        /// </summary>
        WithdrawApproveRow = 21,
    }
    /// <summary>
    /// 单据状态
    /// </summary>
    public enum LibCurrentState
    {
        Draft = 0,
        UnRelease,
        Release,
        Invalid,
        EndCase
    }

    /// <summary>
    /// 审核状态
    /// </summary>
    public enum LibAuditState
    {
        /// <summary>
        /// 未提交
        /// </summary>
        UnSubmit = 0,
        /// <summary>
        /// 已提交
        /// </summary>
        Submit = 1,
        /// <summary>
        /// 已审核
        /// </summary>
        Pass = 2,
        /// <summary>
        /// 未通过
        /// </summary>
        UnPass = 3,
    }

    public class LibBillDataVersion
    {
        private string _ReasonId;
        private string _ReasonName;
        private long _CreateTime;
        private string _InternalId;
        private string _Version;

        public string ReasonName
        {
            get { return _ReasonName; }
            set { _ReasonName = value; }
        }

        public string Version
        {
            get { return _Version; }
            set { _Version = value; }
        }

        public string InternalId
        {
            get { return _InternalId; }
            set { _InternalId = value; }
        }

        public long CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }

        public string ReasonId
        {
            get { return _ReasonId; }
            set { _ReasonId = value; }
        }
    }

    public class LibBillDataRowVersion
    {
        private string _ReasonId;
        private string _ReasonName;
        private long _CreateTime;
        private string _InternalId;
        private string _Version;
        private int _RowId;
        private LibBillDataRow _BillDataRow;

        public string ReasonName
        {
            get { return _ReasonName; }
            set { _ReasonName = value; }
        }

        public int RowId
        {
            get { return _RowId; }
            set { _RowId = value; }
        }

        public LibBillDataRow BillDataRow
        {
            get { return _BillDataRow; }
            set { _BillDataRow = value; }
        }

        public string Version
        {
            get { return _Version; }
            set { _Version = value; }
        }

        public string InternalId
        {
            get { return _InternalId; }
            set { _InternalId = value; }
        }

        public long CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }

        public string ReasonId
        {
            get { return _ReasonId; }
            set { _ReasonId = value; }
        }
    }

    public class LibBillDataRow
    {
        private Dictionary<string, object> _Data;
        private Dictionary<string, List<Dictionary<string, object>>> _SubData;

        public Dictionary<string, List<Dictionary<string, object>>> SubData
        {
            get
            {
                if (_SubData == null)
                    _SubData = new Dictionary<string, List<Dictionary<string, object>>>();
                return _SubData;
            }
        }

        public Dictionary<string, object> Data
        {
            get
            {
                if (_Data == null)
                    _Data = new Dictionary<string, object>();
                return _Data;
            }
        }
    }

    #region [过账]
    /// <summary>
    /// 过账方式
    /// </summary>
    public enum PostAccountWay
    {
        /// <summary>
        /// 不过账
        /// </summary>
        None = 0,
        /// <summary>
        /// 正过账
        /// </summary>
        Positive = 1,
        /// <summary>
        /// 反过账
        /// </summary>
        Reverse = 2,
        /// <summary>
        /// 差异过账
        /// </summary>
        Diff = 3
    }

    /// <summary>
    /// 过账状态
    /// </summary>
    public enum PostAccountState
    {
        /// <summary>
        /// 未生效
        /// </summary>
        UnRelease = 0,
        /// <summary>
        /// 生效
        /// </summary>
        Release = 1,
    }


    /// <summary>
    /// 过账帮助静态类
    /// </summary>
    public static class PostAccountHelper
    {
        /// <summary>
        /// 返回过账方式
        /// </summary>
        /// <param name="postAccountState">当前过账状态</param>
        /// <param name="masterRow">表头行</param>
        /// <param name="isDelete"></param>
        /// <returns>过账方式</returns>
        public static PostAccountWay GetPostAccountWay(PostAccountState postAccountState, DataRow masterRow, bool isDelete = false)
        {
            PostAccountWay postAccountWay = PostAccountWay.None;
            LibCurrentState curState = (LibCurrentState)LibSysUtils.ToInt32(masterRow["CurrentState"]);
            if (isDelete)
            {
                if (postAccountState == PostAccountState.UnRelease)
                {
                    if (curState == LibCurrentState.Draft || curState == LibCurrentState.Invalid)
                        postAccountWay = PostAccountWay.None;
                    else
                        postAccountWay = PostAccountWay.Reverse;
                }
                else
                {
                    if (curState == LibCurrentState.Draft || curState == LibCurrentState.UnRelease || curState == LibCurrentState.Invalid)
                        postAccountWay = PostAccountWay.None;
                    else
                        postAccountWay = PostAccountWay.Reverse;
                }
            }
            else
            {
                LibCurrentState orgState = masterRow.HasVersion(DataRowVersion.Original) ? (LibCurrentState)LibSysUtils.ToInt32(masterRow["CurrentState", DataRowVersion.Original]) : LibCurrentState.Draft;
                int org, cur;
                if (postAccountState == PostAccountState.UnRelease)
                {
                    org = (orgState == LibCurrentState.Draft || orgState == LibCurrentState.Invalid) ? 0 : 1;
                    cur = (curState == LibCurrentState.Draft || curState == LibCurrentState.Invalid) ? 0 : 1;
                }
                else
                {
                    org = (orgState == LibCurrentState.Draft || orgState == LibCurrentState.UnRelease || orgState == LibCurrentState.Invalid) ? 0 : 1;
                    cur = (curState == LibCurrentState.Draft || curState == LibCurrentState.UnRelease || curState == LibCurrentState.Invalid) ? 0 : 1;
                }
                if (org == 1 && cur == 1)
                    postAccountWay = PostAccountWay.Diff;
                else if (org == 0 && cur == 1)
                    postAccountWay = PostAccountWay.Positive;
                else if (org == 0 && cur == 0)
                    postAccountWay = PostAccountWay.None;
                else if (org == 1 && cur == 0)
                    postAccountWay = PostAccountWay.Reverse;
            }
            return postAccountWay;
        }
    }

    #endregion
}
