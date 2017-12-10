using AxCRL.Comm.Utils;
using AxCRL.Data;
using AxCRL.Comm.Redis;
using AxCRL.Parser;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AxCRL.Comm.Runtime;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using AxCRL.Core.Comm;

namespace AxCRL.Core.Cache
{
    public class LibApproveFlowCache : MemoryCacheRedis
    {
        private static LibApproveFlowCache _Default = null;
        private static object _LockObj = new object();

        public LibApproveFlowCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static   LibApproveFlowCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                            _Default = new LibApproveFlowCache("LibApproveFlowCache");
                    }
                }
                return _Default;
            }
        }

        public LibApproveFlow GetCacheItem(string progId, bool isApproveRow = false)
        {
            string key = isApproveRow ? string.Format("{0}_Row", progId) : progId;

            LibApproveFlow value = this.Get< LibApproveFlow>(key) ;
       
            if (value == null)
            {              
                //检查审核过程（Sub表）中是否有DutyId字段，如有则说明是数据库表是新的   
                bool isExistDuty = false;                
                LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel("axp.ApproveFlow");
                if (sqlModel != null && sqlModel.Tables.Count > 2 && sqlModel.Tables[2].Columns.Contains("DUTYID"))
                {
                    isExistDuty = true;
                }
                LibDataAccess dataAccess = new LibDataAccess();
                if (isExistDuty == false)
                {
                    string sql = string.Format("Select D.USECONDITION,A.FLOWLEVEL,A.PERSONID,C.PERSONNAME,C.POSITION,A.INDEPENDENT,A.ROWNO " +
                                           " From AXPAPPROVEFLOWSUB A inner join AXPAPPROVEFLOW B on B.APPROVEFLOWID=A.APPROVEFLOWID " +
                                           " inner join AXPAPPROVEFLOWDETAIL D on D.APPROVEFLOWID=A.APPROVEFLOWID and D.ROW_ID=A.PARENTROWID " +
                                           " left join COMPERSON C on C.PERSONID=A.PERSONID Where B.PROGID={0} and B.ISAPPROVEROW={1}",
                                           LibStringBuilder.GetQuotString(progId), isApproveRow ? 1 : 0);                    
                    using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            if (value == null)
                                value = new LibApproveFlow();
                            string useCondition = LibSysUtils.ToString(reader["USECONDITION"]);
                            int flowLevel = LibSysUtils.ToInt32(reader["FLOWLEVEL"]);
                            string personId = LibSysUtils.ToString(reader["PERSONID"]);
                            string personName = LibSysUtils.ToString(reader["PERSONNAME"]);
                            string position = LibSysUtils.ToString(reader["POSITION"]);
                            bool independent = LibSysUtils.ToBoolean(reader["INDEPENDENT"]);

                            int  subRowNO= LibSysUtils.ToInt32(reader["ROWNO"]);

                            if (!value.ApproveFlowList.ContainsKey(useCondition))
                                value.ApproveFlowList.Add(useCondition, new LibApproveFlowItem());
                            LibApproveFlowItem flowItem = value.ApproveFlowList[useCondition];
                            flowItem.UseCondition = useCondition;
                            if (!flowItem.FlowInfoDic.ContainsKey(flowLevel))
                                flowItem.FlowInfoDic.Add(flowLevel, new List<LibApproveFlowInfo>());
                            List<LibApproveFlowInfo> flowInfoList = flowItem.FlowInfoDic[flowLevel];
                            flowInfoList.Add(new LibApproveFlowInfo()
                            {

                                PersonId = personId,
                                PersonName = personName,
                                Position = position,
                                Independent = independent,

                                FlowLevel = flowLevel,
                                FlowProcRowNo = subRowNO
                            });
                        }
                    }
                }
                else
                {
                    //审核配置表中包含了岗位相关字段，则多查找相关字段的信息 Zhangkj 20170323
                    string sql = string.Format("Select D.USECONDITION,A.FLOWLEVEL,A.PERSONID,C.PERSONNAME,C.POSITION,A.INDEPENDENT,A.ROWNO, " +
                                           " B.CANEDITWHENDOING,B.CANEDITWHENDONE,B.CANDELETEWHENDONE," +//主表增加的审核中和审核通过后的是否可编辑删除配置项
                                           " D.SORTORDER," +//审核流程从表中增加的排序号
                                           " A.DEPTID,F.DEPTNAME,A.DEPTIDCOLUMN,A.DUTYID,E.DUTYNAME,E.DUTYLEVEL,A.ISDUTYUP,A.ISDEPTUP,A.CANJUMP,A.NOTSELF,A.MUSTHIGHLEVEL,A.ISSAMEDEFAULT " +//Sub中增加的部门、岗位、及相关配置项字段
                                           " From AXPAPPROVEFLOWSUB A inner join AXPAPPROVEFLOW B on B.APPROVEFLOWID=A.APPROVEFLOWID " +
                                           " inner join AXPAPPROVEFLOWDETAIL D on D.APPROVEFLOWID=A.APPROVEFLOWID and D.ROW_ID=A.PARENTROWID " +
                                           " left join COMPERSON C on C.PERSONID=A.PERSONID " +
                                           " left join COMDUTY E on A.DUTYID=E.DUTYID " +//职务表
                                           " left join COMDEPT F on A.DEPTID=F.DEPTID " +//部门表
                                           " Where B.PROGID={0} and B.ISAPPROVEROW={1}",
                                           LibStringBuilder.GetQuotString(progId), isApproveRow ? 1 : 0);                    
                    using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            if (value == null)
                                value = new LibApproveFlow();
                            string useCondition = LibSysUtils.ToString(reader["USECONDITION"]);
                            int flowLevel = LibSysUtils.ToInt32(reader["FLOWLEVEL"]);
                            string personId = LibSysUtils.ToString(reader["PERSONID"]);
                            string personName = LibSysUtils.ToString(reader["PERSONNAME"]);
                            string position = LibSysUtils.ToString(reader["POSITION"]);
                            bool independent = LibSysUtils.ToBoolean(reader["INDEPENDENT"]);

                            int subRowNO = LibSysUtils.ToInt32(reader["ROWNO"]);

                            bool canEditWhenDoing = LibSysUtils.ToBoolean(reader["CANEDITWHENDOING"]);
                            bool canEditWhenDone = LibSysUtils.ToBoolean(reader["CANEDITWHENDONE"]);
                            bool canDeleteWhenDone = LibSysUtils.ToBoolean(reader["CANDELETEWHENDONE"]);
                            int sortOrder= LibSysUtils.ToInt32(reader["SORTORDER"]);
                            string deptId= LibSysUtils.ToString(reader["DEPTID"]);
                            string deptName = LibSysUtils.ToString(reader["DEPTNAME"]);
                            string deptIdColumn = LibSysUtils.ToString(reader["DEPTIDCOLUMN"]);
                            string dutyId = LibSysUtils.ToString(reader["DUTYID"]);
                            string dutyName = LibSysUtils.ToString(reader["DUTYNAME"]);
                            int dutyLevel= LibSysUtils.ToInt32(reader["DUTYLEVEL"]);
                            bool isDutyUp = LibSysUtils.ToBoolean(reader["ISDUTYUP"]);
                            bool isDeptUp = LibSysUtils.ToBoolean(reader["ISDEPTUP"]);
                            bool canJump = LibSysUtils.ToBoolean(reader["CANJUMP"]);
                            bool netSelf = LibSysUtils.ToBoolean(reader["NOTSELF"]);
                            bool mustHighLevel = LibSysUtils.ToBoolean(reader["MUSTHIGHLEVEL"]);
                            bool isSameDefalut= LibSysUtils.ToBoolean(reader["ISSAMEDEFAULT"]);

                            value.CanEditWhenDoing = canEditWhenDoing;
                            value.CanEditWhenDone = canEditWhenDone;
                            value.CanDeleteWhenDone = canDeleteWhenDone;

                            if (!value.ApproveFlowList.ContainsKey(useCondition))
                                value.ApproveFlowList.Add(useCondition, new LibApproveFlowItem());
                            LibApproveFlowItem flowItem = value.ApproveFlowList[useCondition];
                            flowItem.SortOrder = sortOrder;
                            flowItem.UseCondition = useCondition;

                            if (!flowItem.FlowInfoDic.ContainsKey(flowLevel))
                                flowItem.FlowInfoDic.Add(flowLevel, new List<LibApproveFlowInfo>());
                            List<LibApproveFlowInfo> flowInfoList = flowItem.FlowInfoDic[flowLevel];
                            flowInfoList.Add(new LibApproveFlowInfo()
                            {
                                PersonId = personId,
                                PersonName = personName,
                                Position = position,
                                Independent = independent,

                                FlowLevel = flowLevel,
                                FlowProcRowNo = subRowNO,

                                DeptId = deptId,
                                DeptName = deptName,
                                DeptIdColumn = deptIdColumn,
                                DutyId = dutyId,
                                DutyName = dutyName,
                                DutyLevel = dutyLevel,
                                IsDutyUp = isDutyUp,
                                IsDeptUp = isDutyUp,
                                CanJump = canJump,
                                NotSelf = netSelf,
                                MustHighLevel = mustHighLevel,
                                IsSameDefault = isSameDefalut,

                                ExecuteDesc = (string.IsNullOrEmpty(deptIdColumn) ? "" : "动态字段:" + deptIdColumn) //初始执行信息的提示
                            });
                        }
                    }
                }

                if (value != null && value.ApproveFlowList.Count > 0)
                {
                    //对审核流程按序号从小到大排序 Zhangkj 20170323
                    List<LibApproveFlowItem> list = (from item in value.ApproveFlowList.Values
                                                     orderby item.SortOrder ascending
                                                     select item).ToList();
                    value.ApproveFlowList = new Dictionary<string, LibApproveFlowItem>();
                    for(int i = 0; i < list.Count; i++)
                    {
                        if (value.ApproveFlowList.ContainsKey(list[i].UseCondition) == false)
                        {
                            value.ApproveFlowList.Add(list[i].UseCondition, list[i]);
                        }
                        else
                        {
                            //正常情况下不会有相同的使用条件（配置时已约束），如果有，则替换掉
                            value.ApproveFlowList[list[i].UseCondition] = list[i];
                        }
                    }

                    //180分钟内不访问自动剔除
                    this.Set(key, value, new TimeSpan(0, 180, 0));
                }
            }
            return value;
        }

        public void RemoveCacheItem(string progId, bool isApproveRow = false)
        {
            string key = isApproveRow ? string.Format("{0}_Row", progId) : progId;
            this.Remove(key);
        }

    }
    /// <summary>
    /// 单据审核配置信息
    /// </summary>
    public class LibApproveFlow
    {
        private bool _CanEditWhenDoing = EnvProvider.Default.Default_CanEditWhenAuditing;
        /// <summary>
        /// 审核中是否可修改
        /// </summary>
        public bool CanEditWhenDoing
        {
            get { return _CanEditWhenDoing; }
            set { _CanEditWhenDoing = value; }
        }

        private bool _CanEditWhenDone = EnvProvider.Default.Default_CanEditWhenAudited;
        /// <summary>
        /// 审核通过后是否可修改
        /// </summary>
        public bool CanEditWhenDone
        {
            get { return _CanEditWhenDone; }
            set { _CanEditWhenDone = value; }
        }

        private bool _CanDeleteWhenDone = EnvProvider.Default.Default_CanDeleteWhenAudited;
        /// <summary>
        /// 审核通过后是否可删除
        /// </summary>
        public bool CanDeleteWhenDone
        {
            get { return _CanDeleteWhenDone; }
            set { _CanDeleteWhenDone = value; }
        }

        private Dictionary<string, LibApproveFlowItem> _ApproveFlowList = null;
        /// <summary>
        /// 条件与具体审核流程的对应关系字典。
        /// 需要按照SortOrder的顺序从小到大排列
        /// </summary>
        public Dictionary<string, LibApproveFlowItem> ApproveFlowList
        {
            get
            {
                if (_ApproveFlowList == null)
                    _ApproveFlowList = new Dictionary<string, LibApproveFlowItem>();
                return _ApproveFlowList;
            }
            set
            {
                this._ApproveFlowList = value;
            }
        }
    }
    /// <summary>
    /// 审核流程信息，一个审核流程包含多级多个审核过程
    /// </summary>
    public class LibApproveFlowItem
    {
        private int _SortOrder = 0;
        /// <summary>
        /// 排序号
        /// </summary>
        public int SortOrder
        {
            get { return _SortOrder; }
            set { _SortOrder = value; }
        }

        private string _UseCondition = string.Empty;
        /// <summary>
        /// 使用条件
        /// </summary>
        public string UseCondition
        {
            get { return _UseCondition; }
            set { _UseCondition = value; }
        }

        private SortedList<int, List<LibApproveFlowInfo>> _FlowInfoDic = null;

        public SortedList<int, List<LibApproveFlowInfo>> FlowInfoDic
        {
            get
            {
                if (_FlowInfoDic == null)
                    _FlowInfoDic = new SortedList<int, List<LibApproveFlowInfo>>();
                return _FlowInfoDic;
            }
        }
    }
    /// <summary>
    /// 具体审核过程信息
    /// </summary>
    [Serializable]
    public class LibApproveFlowInfo
    {
        private string _submitterId;
        private string _PersonId;
        private string _PersonName;
        private string _Position;
        private bool _Independent = false;
        private bool _IsPass = false;
        private int _AuditState = 0;
        private string _AuditOpinion;

        public string AuditOpinion
        {
            get { return _AuditOpinion; }
            set { _AuditOpinion = value; }
        }
        public int AuditState
        {
            get { return _AuditState; }
            set { _AuditState = value; }
        }
        /// <summary>
        /// 提交人Id
        /// </summary>
        public string SubmitterId
        {
            get { return _submitterId; }
            set { _submitterId = value; }
        }
        public bool IsPass
        {
            get { return _IsPass; }
            set { _IsPass = value; }
        }

        public bool Independent
        {
            get { return _Independent; }
            set { _Independent = value; }
        }
        /// <summary>
        /// 审核人职位
        /// </summary>
        public string Position
        {
            get { return _Position; }
            set { _Position = value; }
        }
        /// <summary>
        /// 审核人名称
        /// </summary>
        public string PersonName
        {
            get { return _PersonName; }
            set { _PersonName = value; }
        }
        /// <summary>
        /// 审核人Id
        /// </summary>
        public string PersonId
        {
            get { return _PersonId; }
            set { _PersonId = value; }
        }

        private string _DutyId = string.Empty;
        /// <summary>
        /// 审核过程指定的岗位Id
        /// </summary>
        public string DutyId
        {
            get { return _DutyId; }
            set { _DutyId = value; }
        }
        private string _DutyName = string.Empty;
        /// <summary>
        /// 审核过程指定的岗位的名称
        /// </summary>
        public string DutyName
        {
            get { return _DutyName; }
            set { _DutyName = value; }
        }
        private int _DutyLevel = 0;
        /// <summary>
        /// 审核过程中指定的岗位的级别
        /// </summary>
        public int DutyLevel
        {
            get { return _DutyLevel; }
            set { _DutyLevel = value; }
        }
        private string _DeptId = string.Empty;
        /// <summary>
        /// 审核过程中指定的部门Id
        /// </summary>
        public string DeptId
        {
            get { return _DeptId; }
            set { _DeptId = value; }
        }
        private string _DeptName = string.Empty;
        /// <summary>
        /// 审核过程中指定的部门的名称
        /// </summary>
        public string DeptName
        {
            get { return _DeptName; }
            set { _DeptName = value; }
        }
        private string _DeptIdColumn = string.Empty;
        /// <summary>
        /// 审核过程中指定的动态部门字段名称
        /// 执行时会根据此配置从单据的主表中相应字段查找部门Id
        /// </summary>
        public string DeptIdColumn
        {
            get { return _DeptIdColumn; }
            set { _DeptIdColumn = value; }
        }

        private bool _IsDutyUp = false;
        /// <summary>
        /// 岗位是否可上溯
        /// </summary>
        public bool IsDutyUp
        {
            get { return _IsDutyUp; }
            set { _IsDutyUp = value; }
        }
        private bool _IsDeptUp = false;
        /// <summary>
        /// 部门是否可上溯。
        /// </summary>
        public bool IsDeptUp
        {
            get { return _IsDeptUp; }
            set { _IsDeptUp = value; }
        }
        private bool _CanJump = false;
        /// <summary>
        /// 是否可跳过
        /// 如果为true，则在根据配置无法找到确定的审核执行人时可跳过此审核过程
        /// </summary>
        public bool CanJump
        {
            get { return _CanJump; }
            set { _CanJump = value; }
        }
        private bool _NotSelf = false;
        /// <summary>
        /// 审核人是否不能是提交人自己
        /// </summary>
        public bool NotSelf
        {
            get { return _NotSelf; }
            set { _NotSelf = value; }
        }
        private bool _MustHighLevel = false;
        /// <summary>
        /// 审核人是否必须是职级比自己高的人。仅对于通过提交人直属组织树查找到岗位有效，也就是未通过“部门”和“动态部门字段”确定部门，而是通过提交人的所属部门查找到的岗位
        /// 如果为true，则找到的审核人的岗位等级要比自己的高
        /// </summary>
        public bool MustHighLevel
        {
            get { return _MustHighLevel; }
            set { _MustHighLevel = value; }
        }
        private bool _IsSameDefault = false;
        /// <summary>
        /// 同人默认”配置：如果本次审核过程的审核执行人与之前的审核过程中审核执行人是同一个人，则默认为本次审核过程与之前的一致。
        /// 默认为true，即同一个审核执行人对同一个单据的同一次提交审核一次给出意见即可
        /// </summary>
        public bool IsSameDefault
        {
            get { return _IsSameDefault; }
            set { _IsSameDefault = value; }
        }

        /// <summary>
        /// 审核过程所在的层级
        /// </summary>
        public int FlowLevel { get; set; }
        /// <summary>
        /// 审核过程所在的具体行号
        /// </summary>
        public int FlowProcRowNo { get; set; }

        /// <summary>
        /// 执行时是否是跳过的审核过程
        /// </summary>
        public bool IsJump { get; set; }
        private string _JumpReason = string.Empty;
        /// <summary>
        /// 跳过过程的原因
        /// </summary>
        public string JumpReason
        {
            get { return _JumpReason; }
            set { _JumpReason = value; }
        }

        private string _ExecuteDesc = string.Empty;
        /// <summary>
        /// 流程执行说明。
        /// 具体的审核过程，根据配置来进行审核执行时的流程特殊说明。例如，配置为经理岗位审核，如因为对应部门没有经理，而又跳过了，则会说明哪个部门没有经理岗位，岗位进行了上溯等。
        /// </summary>
        public string ExecuteDesc
        {
            get { return _ExecuteDesc; }
            set { _ExecuteDesc = value; }
        }

        private string _AuditTaskId = string.Empty;
        /// <summary>
        /// 流程执行过程的主键
        /// </summary>
        public string AuditTaskId
        {            
            get { return _AuditTaskId; }
            set { _AuditTaskId = value; }
        }
    }

    public class LibApprovePersonInfo
    {
        private string _personId = string.Empty;

        public string PersonId
        {
            get { return _personId; }
            set { _personId = value; }
        }
        private string _personName = string.Empty;

        public string PersonName
        {
            get { return _personName; }
            set { _personName = value; }
        }
        private string _position = string.Empty;

        public string Position
        {
            get { return _position; }
            set { _position = value; }
        }
    }

    public class LibApproveFlowParser
    {
        public static bool Parse(string condition, DataRow masterRow, DataRow bodyRow)
        {
            return ParseCore(condition, masterRow, bodyRow, null, null);
        }

        public static bool Parse(string condition, Dictionary<string, object> masterDic, Dictionary<string, object> bodyDic)
        {
            return ParseCore(condition, null, null, masterDic, bodyDic);
        }

        private static bool ParseCore(string condition, DataRow masterRow, DataRow bodyRow, Dictionary<string, object> masterDic, Dictionary<string, object> bodyDic)
        {
            bool result = false;
            try
            {
                //先使用QueryFields尝试
                if ((masterDic == null || masterDic.Count == 0) && masterRow != null)
                {
                    masterDic = new Dictionary<string, object>();
                    if (masterRow.Table != null)
                    {
                        foreach (DataColumn column in masterRow.Table.Columns)
                        {
                            masterDic.Add(column.ColumnName, masterRow[column.ColumnName]);
                        }
                    }
                }
                if(masterDic!=null && masterDic.Count > 0)
                {
                    LibQueryCondition queryCondition = JsonUtiler.Deserialize<LibQueryCondition>(condition);
                    if (queryCondition != null)
                    {
                        if (queryCondition.AccordOfThis(masterDic))
                            return true;
                    }
                }               
            }
            catch (Exception)
            {
                //to do log
            }
           

            Memory memory = new Memory();
            //匹配类似表达式"[A.QTY]>=10 && ([A.MaterialId]=='13212321' || ([A.RANGEID]=='AAA' && [A.SSID]=='9999')) && (([A.DD]=='xxx' || [A.DD]=='xxx1' || [A.DD]=='xxx2') || [A.ZZ]>=0)";
            string pattern = @"[[][A-Z]\.\w+[]]";
            MatchCollection matchList = Regex.Matches(condition, pattern);
            HashSet<string> temp = new HashSet<string>();
            foreach (var item in matchList)
            {
                string field = item.ToString();
                if (temp.Contains(field))
                    continue;
                temp.Add(field);
                string copyField = field;
                field = field.Remove(0, 1);
                field = field.Remove(field.Length - 1, 1);
                int tableIndex = (int)field[0] - (int)'A';
                string fieldName = field.Substring(2, field.Length - 2);
                object value = tableIndex == 0 ? (masterRow == null ? masterDic[fieldName] : masterRow[fieldName]) : (bodyRow == null ? bodyDic[fieldName] : bodyRow[fieldName]);
                memory.AddObject(fieldName, value);
                condition = condition.Replace(copyField, fieldName);
            }
            Script.Execute("if(" + condition + "){ret=true;}else{ret=false;}", memory);
            result = LibSysUtils.ToBoolean(memory["ret"].value);
            return result;
        }
    }




}
