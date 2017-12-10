using AxCRL.Bcf;
using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Data.SqlBuilder;
using AxCRL.Services.ServiceMethods;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axp.SystemBcf
{
    [ProgId(ProgId = "axp.ApproveFlow", ProgIdType = ProgIdType.Bcf,VclPath = @"/Scripts/module/mes/axp/axpApproveFlowVcl.js")]
    public class AxpApproveFlowBcf : LibBcfData
    {
        /// <summary>
        /// 根据功能标识返回对应的主表中的所有列，以FuzzyResult的形式返回
        /// Zhangkj 20170321
        /// </summary>
        /// <param name="progId"></param>
        /// <returns></returns>
        public IList<FuzzyResult> GetDynamicFields(string progId)
        {
            List<FuzzyResult> list = new List<FuzzyResult>();
            string msg = string.Empty;
            List<List<DefineField>> fieldList = BcfTemplateMethods.GetBcfDefineFields(progId, out msg);
            if (string.IsNullOrEmpty(msg) == false || fieldList == null || fieldList.Count == 0 || fieldList[0] == null || fieldList[0].Count == 0)
                return list;
            foreach(DefineField field in fieldList[0])
            {
                if (field == null)
                    continue;
                list.Add(new FuzzyResult(field.Name, field.DisplayName));
            }           
            return list;
        }
        /// <summary>
        /// 根据部门标识和关键词，查找部门任职表中的岗位信息，返回IdName形式的数组供前端展示选择
        /// Zhangkj 20170323
        /// </summary>
        /// <param name="deptId"></param>
        /// <param name="queryKey"></param>
        /// <returns></returns>
        public IList<FuzzyResult> SearchDutyIdNameFromDept(string deptId,string queryKey)
        {
            List<FuzzyResult> list = new List<FuzzyResult>();
            try
            {
                if (string.IsNullOrEmpty(deptId))
                    return list;
                string querySql = string.Format("Select distinct A.DUTYID,B.DUTYNAME from COMDEPTDUTYPERSON A left join COMDUTY B on A.DUTYID = B.DUTYID " +
                    " where A.DEPTID = {0} and ( A.DUTYID like {1} or B.DUTYNAME like {1} ) ", LibStringBuilder.GetQuotString(deptId),
                    LibStringBuilder.GetQuotString(string.Format("%{0}%", queryKey))
                    );
                int count = 0;
                using (IDataReader reader = this.DataAccess.ExecuteDataReader(querySql))
                {
                    while (reader.Read())
                    {
                        list.Add(new FuzzyResult(LibSysUtils.ToString(reader[0]), LibSysUtils.ToString(reader[1])));
                        count++;
                        if (count == 30)
                            break;
                    }
                }
            }
            catch(Exception exp)
            {
                // to do Log
            }
            return list;
        }
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpApproveFlowBcfTemplate("axp.ApproveFlow");
        }

        protected override void BeforeUpdate()
        {
            base.BeforeUpdate();
            int bodyRowCount = 0;
            int subRowCount = 0;

            //检查行审核的启用是否有效
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            if (LibSysUtils.ToBoolean(masterRow["ISAPPROVEROW"]))
            {
                string progId = LibSysUtils.ToString(masterRow["PROGID"]);
                LibBcfData bcfData = (LibBcfData)LibBcfSystem.Default.GetBcfInstance(progId);
                if (bcfData != null && bcfData.Template.FuncPermission.UsingApproveRow == false)
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("审核流程:功能{0}的实现中未启用行审核，审核流不能配置为行审核。", progId));
                }
            }
            List<string> useConditionList = new List<string>();
            string useCondition = string.Empty;
            foreach (DataRow curRow in this.DataSet.Tables[1].Rows)
            {
                if (curRow.RowState == DataRowState.Deleted)
                    continue;
                useCondition = LibSysUtils.ToString(curRow["USECONDITION"]);
                if (useConditionList.Contains(useCondition) == false)
                    useConditionList.Add(useCondition);
                else
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("审核流程:不能使用相同的使用条件。行号:{0}", curRow["ROWNO"]));
                }

                bodyRowCount++;
                SortedList<int, HashSet<string>> tempList = new SortedList<int, HashSet<string>>();
                HashSet<int> tempHashSet = new HashSet<int>();
                DataRow[] childRows = curRow.GetChildRows(this.DataSet.Relations[1]);
                if (childRows != null)
                {
                    subRowCount = 0;
                    foreach (DataRow childRow in childRows)
                    {
                        if (childRow.RowState == DataRowState.Deleted)
                            continue;
                        subRowCount++;
                        int flowLevel = LibSysUtils.ToInt32(childRow["FLOWLEVEL"]);
                        if (!tempList.ContainsKey(flowLevel))
                            tempList.Add(flowLevel, new HashSet<string>());
                        HashSet<string> hashSet = tempList[flowLevel];
                        string personId = LibSysUtils.ToString(childRow["PERSONID"]);
                        string deptId = LibSysUtils.ToString(childRow["DEPTID"]);
                        string dutyId= LibSysUtils.ToString(childRow["DUTYID"]);
                        string deptColumnName= LibSysUtils.ToString(childRow["DEPTIDCOLUMN"]); 
                        if(string.IsNullOrEmpty(personId)&& string.IsNullOrEmpty(dutyId))
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("审核流程明细:具体审核人和岗位必须要指定一个。行号:{0}", childRow["ROWNO"]));
                        }
                        if (string.IsNullOrEmpty(personId)==false && string.IsNullOrEmpty(dutyId) ==false)
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("审核流程明细:具体审核人和岗位不能同时指定。行号:{0}", childRow["ROWNO"]));
                        }
                        if (string.IsNullOrEmpty(personId) == false)
                        {
                            //设定了具体审核人就不用再设置部门（或动态部门字段）
                            if(string.IsNullOrEmpty(deptId)==false||
                                string.IsNullOrEmpty(deptColumnName) == false)
                            {
                                this.ManagerMessage.AddMessage(LibMessageKind.Warn, string.Format("审核流程明细:设定了具体审核人,再设置部门(或动态部门字段)无意义。行号:{0}", childRow["ROWNO"]));
                            }
                            if (hashSet.Contains(personId))
                                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("审核流程明细:同一层级不能出现相同的人。行号:{0}", childRow["ROWNO"]));
                            else
                                hashSet.Add(personId);
                            bool independent = LibSysUtils.ToBoolean(childRow["INDEPENDENT"]);
                            if (independent && !tempHashSet.Contains(flowLevel))
                                tempHashSet.Add(flowLevel);
                        }
                        else
                        {                           
                            if(string.IsNullOrEmpty(deptId)==false&&string.IsNullOrEmpty(deptColumnName)==false)
                                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("审核流程明细:部门和动态部门字段只能设置一个。行号:{0}", childRow["ROWNO"]));
                        }

                    }
                    if (subRowCount == 0)
                    {
                        this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("审核流程:行号为{0}的流程必须配置至少一个审核过程。", curRow["ROWNO"]));
                    }
                    int preLevel = -1;
                    foreach (var item in tempList)
                    {
                        if (preLevel == -1)
                        {
                            preLevel = item.Key;
                            if (preLevel != 1)
                                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("审核流程明细:最小层级必须从1开始。"));
                        }
                        else if (++preLevel != item.Key)
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("审核流程明细:层级中间不能有断层。"));
                            break;
                        }
                        if (item.Value.Count == 1 && tempHashSet.Contains(item.Key))
                        {
                            this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("审核流程明细:同一层级存在多人时才能勾选独立决策权。"));
                        }
                    }
                }
            }
            if (bodyRowCount == 0)
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, string.Format("必须配置至少一个审核流程。"));
            }
        }

        protected override void AfterUpdate()
        {
            base.AfterUpdate();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            string sql = string.Format("select PROGID from AXPAPPROVEFLOW where APPROVEFLOWID<>{0} and PROGID={1} and ISAPPROVEROW={2}",
                LibStringBuilder.GetQuotObject(masterRow["APPROVEFLOWID"]), LibStringBuilder.GetQuotObject(masterRow["PROGID"]), LibSysUtils.ToBoolean(masterRow["ISAPPROVEROW"]) ? 1 : 0);
            if (!string.IsNullOrEmpty(LibSysUtils.ToString(this.DataAccess.ExecuteScalar(sql))))
            {
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "当前功能已存在审核主数据，不能重复建立");
            }
            //清除缓存
            if (this.BillAction != AxCRL.Bcf.BillAction.AddNew && this.BillAction != AxCRL.Bcf.BillAction.SaveToDraft && this.BillAction != AxCRL.Bcf.BillAction.SubmitDraft)
            {
                LibApproveFlowCache.Default.RemoveCacheItem(LibSysUtils.ToString(masterRow["PROGID", DataRowVersion.Original]), LibSysUtils.ToBoolean(masterRow["ISAPPROVEROW", DataRowVersion.Original]));
            }
        }

        protected override void AfterDelete()
        {
            base.AfterDelete();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            LibApproveFlowCache.Default.RemoveCacheItem(LibSysUtils.ToString(masterRow["PROGID"]), LibSysUtils.ToBoolean(masterRow["ISAPPROVEROW"]));
        }
    }

    public class AxpApproveFlowBcfTemplate : LibTemplate
    {
        private const string masterTableName = "AXPAPPROVEFLOW";
        private const string bodyTableName = "AXPAPPROVEFLOWDETAIL";
        private const string subTableName = "AXPAPPROVEFLOWSUB";

        public AxpApproveFlowBcfTemplate(string progId)
            : base(progId, BillType.Master, "单据审核流配置")
        {

        }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "APPROVEFLOWID";
            DataTable masterTable = new DataTable(masterTableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "审核流配置代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "APPROVEFLOWNAME", "审核流配置名称", FieldSize.Size50) { AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PROGID", "功能代码", FieldSize.Size50)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection(){
                    new RelativeSource("axp.FuncList"){
                           RelFields = new RelFieldCollection(){
                           new RelField("PROGNAME", LibDataType.NText,FieldSize.Size50,"功能名称")
                      }  
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISAPPROVEROW", "行记录审核") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });

            //Zhangkj 20170322
            DataSourceHelper.AddColumn(new DefineField(masterTable, "CANEDITWHENDOING", "审核中可修改") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "CANEDITWHENDONE", "审核后可修改") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "CANDELETEWHENDONE", "审核后可删除") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = false });


            DataSourceHelper.AddFixColumn(masterTable, this.BillType);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns[primaryName] };
            this.DataSet.Tables.Add(masterTable);

            DataTable bodyTable = new DataTable(bodyTableName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, primaryName, "审核流配置代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "USECONDITION", "使用条件", FieldSize.Size500) { ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "USECONDITIONDESC", "使用条件说明", FieldSize.Size200));

            //审核流程的排序号  Zhangkj 20170322
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "SORTORDER", "序号") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterOrEqualThanZero, Precision = 0, DefaultValue = 0, AllowEmpty = false });

            DataSourceHelper.AddColumn(new DefineField(bodyTable, "FLOWDETAIL", "审核流程明细") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true, SubTableIndex = 2 });
            DataSourceHelper.AddRemark(bodyTable);
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", masterTableName, bodyTableName), masterTable.Columns[primaryName], bodyTable.Columns[primaryName]);

            DataTable subTable = new DataTable(subTableName);
            DataSourceHelper.AddColumn(new DefineField(subTable, primaryName, "审核流配置代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(subTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(subTable);
            DataSourceHelper.AddRowNo(subTable);
            DataSourceHelper.AddColumn(new DefineField(subTable, "FLOWLEVEL", "审核层级") { DefaultValue = 1, DataType = LibDataType.Int32, ControlType = LibControlType.Number, Precision = 0, QtyLimit = LibQtyLimit.GreaterThanZero });
            DataSourceHelper.AddColumn(new DefineField(subTable, "PERSONID", "审核人代码", FieldSize.Size20)
            {
                //AllowEmpty = false, //具体审核人和岗位两个只要有一个填写了就可以
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() { 
                 new RelativeSource("com.Person"){  RelFields = new RelFieldCollection()
                     { new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"审核人名称"),
                       new RelField("POSITION", LibDataType.NText,FieldSize.Size50,"职位")}}  
                }
            });

            Dictionary<string, bool> orderBys = new Dictionary<string, bool>();
            orderBys.Add("SORTORDER", true);
            DataSourceHelper.AddColumn(new DefineField(subTable, "DEPTID", "部门代码", FieldSize.Size20)
            {  
                RelativeSource = new RelativeSourceCollection()
                {
                     new RelativeSource("com.Dept")
                     {
                         RelFields = new RelFieldCollection()
                         {
                             new RelField("DEPTNAME", LibDataType.NText,FieldSize.Size50,"部门名称")
                         },
                         ContainsSub = true,
                         ExpandAll = true,//默认全部展开
                         SearchFilterCount = 200,//筛选200条
                         ParentColumnName = "SUPERDEPTID", //在关联的表中表示父数据的列
                         OrderbyColumns = orderBys
                     }
                },
                ControlType = LibControlType.IdNameTree //以树形结构展示,需要在RelativeSource属性后设置，否则会重置为IdName
            });
            //动态部门字段
            DataSourceHelper.AddColumn(new DefineField(subTable, "DEPTIDCOLUMN", "动态部门字段", FieldSize.Size400)
            {
                ControlType = LibControlType.KeyValueOption,
                KeyValueOption = new LibTextOptionCollection()
                {
                    new LibTextOption() { Key="",Value=""}
                }
            });
            DataSourceHelper.AddColumn(new DefineField(subTable, "DUTYID", "岗位代码", FieldSize.Size20)
            {               
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                     new RelativeSource("com.Duty")
                     {  RelFields = new RelFieldCollection()
                        {
                            new RelField("DUTYNAME", LibDataType.NText,FieldSize.Size50,"岗位名称"),
                            new RelField("DUTYLEVEL", LibDataType.NText,FieldSize.Size50,"职务级别"),
                        }
                     }
                }
            });
            //启用了岗位上溯则会在找不到岗位时查找本部门的更高级岗位。默认为false
            DataSourceHelper.AddColumn(new DefineField(subTable, "ISDUTYUP", "岗位上溯") { DefaultValue = true, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            //启用了部门上溯则会在找不到岗位时查找上级部门中的岗位。默认为true
            DataSourceHelper.AddColumn(new DefineField(subTable, "ISDEPTUP", "部门上溯") { DefaultValue = true, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            //审核人不能是提交人本人。默认为true
            DataSourceHelper.AddColumn(new DefineField(subTable, "NOTSELF", "非本人") { DefaultValue = true, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            //审核人必须是职级比自己高的人。仅对于通过提交人直属组织树查找到岗位有效，也就是未通过“部门”和“动态部门字段”确定部门，而是通过提交人的所属部门查找到的岗位
            DataSourceHelper.AddColumn(new DefineField(subTable, "MUSTHIGHLEVEL", "职级高") { DefaultValue = true, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            //是否可跳过。当根据配置找不到具体执行人时，如果可跳过则跳过此审核过程，否则认为找不到执行人、审核流配置有误
            DataSourceHelper.AddColumn(new DefineField(subTable, "CANJUMP", "可跳过") { DefaultValue = false, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            //同人默认”配置：如果本次审核过程的审核执行人与之前的审核过程中审核执行人是同一个人，则默认为本次审核过程与之前的一致。默认为true，即同一个审核执行人对同一个单据的同一次提交审核一次给出意见即可
            DataSourceHelper.AddColumn(new DefineField(subTable, "ISSAMEDEFAULT", "同人默认") { DefaultValue = true, DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });

            DataSourceHelper.AddColumn(new DefineField(subTable, "INDEPENDENT", "独立决策权") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddRemark(subTable);
            subTable.PrimaryKey = new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"], subTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(subTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", bodyTableName, subTableName), new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] }, new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["PARENTROWID"] });
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "APPROVEFLOWID", "APPROVEFLOWNAME", "PROGID", "ISAPPROVEROW", "CANEDITWHENDOING", "CANEDITWHENDONE", "CANDELETEWHENDONE" });
            layout.GridRange = layout.BuildGrid(1, "单据审核流配置规则");
            layout.SubBill.Add(2, layout.BuildGrid(2, "审核流程明细"));
            layout.ButtonRange = layout.BuildButton(new List<FunButton>() { new FunButton("btnCondition", "设置使用条件") });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
