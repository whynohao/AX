using AxCRL.Comm.Utils;
using AxCRL.Core.Comm;
using AxCRL.Data;
using AxCRL.Comm.Redis;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Cache
{
    public class LibRolePermissionCache : MemoryCacheRedis
    {
        private static LibRolePermissionCache _Default = null;
        private static object _LockObj = new object();
        private static ConcurrentDictionary<string, object> lockObjDic = null;

        public LibRolePermissionCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static  LibRolePermissionCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibRolePermissionCache("LibRolePermissionCache");
                            lockObjDic = new ConcurrentDictionary<string, object>();
                        }
                    }
                }
                return _Default;
            }
        }

        public object RemoveCacheItem(string roleId)
        {
            return this.Remove(roleId);
        }


        private void MergeQueryField(string progId, LibPermission curPermission, LibPermission otherPermission)
        {
            LibSqlModel sqlModel = null;
            LibSqlModelTable table = null;
            List<string> removeList = new List<string>();
            foreach (var item in curPermission.QueryFieldDic)
            {
                if (otherPermission.QueryFieldDic.ContainsKey(item.Key))
                {
                    LibQueryField other = otherPermission.QueryFieldDic[item.Key][0];
                    bool exist = false;
                    foreach (var subItem in item.Value)
                    {
                        if (table == null)
                        {
                            sqlModel = LibSqlModelCache.Default.GetSqlModel(progId);
                            if (sqlModel != null)
                            {
                                table = (LibSqlModelTable)sqlModel.Tables[0];
                            }
                        }
                        if (table != null)
                        {
                            LibSqlModelColumn col = (LibSqlModelColumn)table.Columns[item.Key];
                            LibDataType dataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                            exist = LibQueryConditionParser.GetQueryFieldStr(dataType, subItem).CompareTo(LibQueryConditionParser.GetQueryFieldStr(dataType, other)) == 0;
                            if (exist)
                                break;
                        }
                    }
                    if (!exist)
                    {
                        item.Value.Add(other);
                    }
                }
                else
                {
                    removeList.Add(item.Key);
                }
            }
            foreach (var item in removeList)
            {
                curPermission.QueryFieldDic.Remove(item);
            }
        }

        public LibRolePermission GetCacheItem(string roleId)
        {
            LibRolePermission rolePermission = null;
            object lockItem = lockObjDic.GetOrAdd(roleId, new object());
            lock (lockItem)
            {
                rolePermission = this.Get< LibRolePermission>(roleId)  ;
                if (rolePermission == null)
                {
                    rolePermission = new LibRolePermission();
                    rolePermission.RoleId = roleId;
                    string sql = string.Format("select distinct A.ISUNLIMITED,B.PERMISSIONGROUPID from AXPROLE A left join AXPROLEDETAIL B on B.ROLEID=A.ROLEID where A.ROLEID={0} and ISVALIDITY=1", LibStringBuilder.GetQuotString(roleId));
                    LibDataAccess dataAccess = new LibDataAccess();
                    List<string> groupList = new List<string>();
                    using (IDataReader reader = dataAccess.ExecuteDataReader(sql, false))
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            if (count == 0)
                                rolePermission.IsUnlimited = LibSysUtils.ToBoolean(reader["ISUNLIMITED"]);
                            string groupId = LibSysUtils.ToString(reader["PERMISSIONGROUPID"]);
                            if (!string.IsNullOrEmpty(groupId))
                                groupList.Add(groupId);
                            count++;
                        }
                    }
                    if (!rolePermission.IsUnlimited)
                    {
                        foreach (string groupId in groupList)
                        {
                            LibPermissionGroup group = LibPermissionGroupCache.Default.GetCacheItem(groupId);
                            if (group != null)
                            {
                                foreach (var item in group.PermissionDic)
                                {
                                    if (rolePermission.PermissionDic.ContainsKey(item.Key))
                                    {
                                        //进行宽松的权限控制
                                        LibPermission curPermission = rolePermission.PermissionDic[item.Key];
                                        //处理清单浏览条件
                                        if (!string.IsNullOrEmpty(curPermission.ShowCondition) && string.IsNullOrEmpty(item.Value.ShowCondition))
                                            curPermission.ShowCondition = string.Empty;
                                        else if (!string.IsNullOrEmpty(curPermission.ShowCondition) && !string.IsNullOrEmpty(item.Value.ShowCondition))
                                        {
                                            if (curPermission.ShowCondition.CompareTo(item.Value.ShowCondition) != 0)
                                            {
                                                curPermission.ShowCondition = string.Format("{0} or {1}", curPermission.ShowCondition, item.Value.ShowCondition);
                                                MergeQueryField(item.Key, curPermission, item.Value);
                                            }
                                        }
                                        //处理按钮权限
                                        if (curPermission.NoUseButton.Count > 0 && item.Value.NoUseButton.Count == 0)
                                            curPermission.NoUseButton.Clear();
                                        else if (curPermission.NoUseButton.Count > 0 && item.Value.NoUseButton.Count > 0)
                                        {
                                            List<string> removeList = new List<string>();
                                            foreach (string buttonId in curPermission.NoUseButton)
                                            {
                                                if (!item.Value.NoUseButton.Contains(buttonId))
                                                    removeList.Add(buttonId);
                                            }
                                            foreach (var buttonId in removeList)
                                            {
                                                curPermission.NoUseButton.Remove(buttonId);
                                            }
                                        }
                                        //处理操作权限
                                        if (curPermission.OperateMark != item.Value.OperateMark)
                                        {
                                            curPermission.OperateMark |= item.Value.OperateMark;
                                        }
                                        //处理字段权限
                                        if (curPermission.FieldPowerDic.Count > 0 && item.Value.FieldPowerDic.Count == 0)
                                            curPermission.FieldPowerDic.Clear();
                                        else if (curPermission.FieldPowerDic.Count > 0 && item.Value.FieldPowerDic.Count > 0)
                                        {
                                            List<int> remove = new List<int>();
                                            foreach (var subItem in curPermission.FieldPowerDic)
                                            {
                                                if (item.Value.FieldPowerDic.ContainsKey(subItem.Key))
                                                {
                                                    Dictionary<string, FieldPower> otherFieldPower = item.Value.FieldPowerDic[subItem.Key];
                                                    List<string> subRemove = new List<string>();
                                                    foreach (var fieldPowerItem in subItem.Value)
                                                    {
                                                        if (otherFieldPower.ContainsKey(fieldPowerItem.Key))
                                                        {
                                                            FieldPower other = otherFieldPower[fieldPowerItem.Key];
                                                            if (fieldPowerItem.Value.PowerOption == FieldPowerOption.CannotBrowse)
                                                            {
                                                                if (other.PowerOption == FieldPowerOption.CannotModify)
                                                                {
                                                                    fieldPowerItem.Value.PowerOption = other.PowerOption;
                                                                    fieldPowerItem.Value.Condition = other.Condition;
                                                                }
                                                                else
                                                                {
                                                                    fieldPowerItem.Value.Condition = string.Format("{0} && {1}", fieldPowerItem.Value.Condition, other.Condition);
                                                                }
                                                            }
                                                            else if (other.PowerOption == FieldPowerOption.CannotModify)
                                                            {
                                                                fieldPowerItem.Value.Condition = string.Format("{0} && {1}", fieldPowerItem.Value.Condition, other.Condition);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            subRemove.Add(fieldPowerItem.Key);
                                                        }
                                                    }
                                                    foreach (string fieldName in subRemove)
                                                    {
                                                        subItem.Value.Remove(fieldName);
                                                    }
                                                }
                                                else
                                                {
                                                    remove.Add(subItem.Key);
                                                }
                                            }
                                            foreach (var tableIndex in remove)
                                            {
                                                curPermission.FieldPowerDic.Remove(tableIndex);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        rolePermission.PermissionDic[item.Key] = (LibPermission)item.Value.Clone();
                                    }
                                }
                            }
                        }
                    }
                    //CacheItemPolicy policy = new CacheItemPolicy();
                    //policy.SlidingExpiration = new TimeSpan(0, 180, 0); //180分钟内不访问自动剔除
                    this.Set(roleId, rolePermission, new TimeSpan(0, 180, 0));
                }
            }
            return rolePermission;
        }
    }


    public class LibPermissionGroupCache : MemoryCacheRedis
    {
        private static LibPermissionGroupCache _Default = null;
        private static object _LockObj = new object();
        private static ConcurrentDictionary<string, object> lockObjDic = null;

        public LibPermissionGroupCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static  LibPermissionGroupCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibPermissionGroupCache("LibPermissionGroupCache");
                            lockObjDic = new ConcurrentDictionary<string, object>();
                        }
                    }
                }
                return _Default;
            }
        }

        public object RemoveCacheItem(string groupId)
        {
            string sql = string.Format("select distinct ROLEID from AXPROLEDETAIL where PERMISSIONGROUPID={0}", LibStringBuilder.GetQuotString(groupId));
            LibDataAccess dataAccess = new LibDataAccess();
            List<string> list = new List<string>();
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    list.Add(LibSysUtils.ToString(reader["ROLEID"]));
                }
            }
            foreach (string roleId in list)
            {
                LibRolePermissionCache.Default.RemoveCacheItem(roleId);
            }
            return this.Remove(groupId);
        }

        public LibPermissionGroup GetCacheItem(string groupId)
        {
            LibPermissionGroup permissionGroup = null;
            object lockItem = lockObjDic.GetOrAdd(groupId, new object());
            lock (lockItem)
            {
                permissionGroup = this.Get< LibPermissionGroup>(groupId)  ;
                if (permissionGroup == null)
                {
                    permissionGroup = GetPermissionGroupData(groupId);
                    if (permissionGroup != null)
                    {
                        //CacheItemPolicy policy = new CacheItemPolicy();
                        //policy.SlidingExpiration = new TimeSpan(0, 120, 0); //60分钟内不访问自动剔除
                        this.Set(groupId, permissionGroup, new TimeSpan(0, 120, 0));
                    }
                }
            }
            return permissionGroup;
        }

        private LibPermissionGroup GetPermissionGroupData(string groupId)
        {
            LibPermissionGroup groupData = null;
            string sql = string.Format("select PARENTGROUPID from AXPPERMISSIONGROUP where PERMISSIONGROUPID={0} and ISVALIDITY=1", LibStringBuilder.GetQuotString(groupId));
            LibDataAccess dataAccess = new LibDataAccess();
            string parentGroupId = LibSysUtils.ToString(dataAccess.ExecuteScalar(sql, false));
            if (!string.IsNullOrEmpty(parentGroupId))
            {
                groupData = GetPermissionGroupData(parentGroupId);
            }
            if (groupData == null)
                groupData = new LibPermissionGroup();
            sql = string.Format("select PROGID,SHOWCONDITION,OPERATEMARK from AXPPERMISSIONGROUPDETAIL where PERMISSIONGROUPID={0}", LibStringBuilder.GetQuotString(groupId));
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql, false))
            {
                while (reader.Read())
                {
                    string progId = LibSysUtils.ToString(reader["PROGID"]);
                    LibPermission permission = new LibPermission();
                    string showCondition = LibSysUtils.ToString(reader["SHOWCONDITION"]);
                    if (!string.IsNullOrEmpty(showCondition))
                    {
                        LibQueryCondition condition = JsonConvert.DeserializeObject(showCondition, typeof(LibQueryCondition)) as LibQueryCondition;
                        permission.ShowCondition = LibQueryConditionParser.GetQueryData(progId, condition);
                        if (!string.IsNullOrEmpty(permission.ShowCondition.Trim()))
                        {
                            foreach (var queryField in condition.QueryFields)
                            {
                                if (!permission.QueryFieldDic.ContainsKey(queryField.Name))
                                    permission.QueryFieldDic.Add(queryField.Name, new List<LibQueryField>() { queryField });
                            }
                            permission.ShowCondition = string.Format("({0})", permission.ShowCondition);
                        }
                    }
                    permission.OperateMark = LibSysUtils.ToInt32(reader["OPERATEMARK"]);
                    if (groupData.PermissionDic.ContainsKey(progId)) //对于继承关系的，直接用子覆盖父的权限
                    {
                        groupData.PermissionDic[progId] = permission;
                    }
                    else
                    {
                        groupData.PermissionDic.Add(progId, permission);
                    }
                }
            }
            sql = string.Format("select B.PROGID,A.TABLEINDEX,A.FIELDNAME,A.FIELDPOWER,A.USECONDITION from AXPFIELDPOWER A inner join AXPPERMISSIONGROUPDETAIL B on B.ROW_ID=A.PARENTROWID where A.PERMISSIONGROUPID={0}", LibStringBuilder.GetQuotString(groupId));
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql, false))
            {
                while (reader.Read())
                {
                    string progId = LibSysUtils.ToString(reader["PROGID"]);
                    if (groupData.PermissionDic.ContainsKey(progId))
                    {
                        int tableIndex = LibSysUtils.ToInt32(reader["TABLEINDEX"]);
                        string fieldName = LibSysUtils.ToString(reader["FIELDNAME"]);
                        if (!groupData.PermissionDic[progId].FieldPowerDic.ContainsKey(tableIndex))
                        {
                            groupData.PermissionDic[progId].FieldPowerDic.Add(tableIndex, new Dictionary<string, FieldPower>());
                        }
                        Dictionary<string, FieldPower> dic = groupData.PermissionDic[progId].FieldPowerDic[tableIndex];
                        if (!dic.ContainsKey(fieldName))
                        {
                            string useCondition = LibSysUtils.ToString(reader["USECONDITION"]);
                            if (!string.IsNullOrEmpty(useCondition))
                                useCondition = string.Format("({0})", useCondition);
                            dic.Add(fieldName, new FieldPower() { Condition = useCondition, PowerOption = (FieldPowerOption)LibSysUtils.ToInt32(reader["FIELDPOWER"]) });
                        }
                    }
                }
            }
            sql = string.Format("select B.PROGID,A.BUTTONID from AXPBUTTONPOWER A inner join AXPPERMISSIONGROUPDETAIL B on B.ROW_ID=A.PARENTROWID where A.PERMISSIONGROUPID={0} and A.CANUSE=0", LibStringBuilder.GetQuotString(groupId));
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql, false))
            {
                while (reader.Read())
                {
                    string progId = LibSysUtils.ToString(reader["PROGID"]);
                    if (groupData.PermissionDic.ContainsKey(progId))
                    {
                        string buttonId = LibSysUtils.ToString(reader["BUTTONID"]);
                        if (!groupData.PermissionDic[progId].NoUseButton.Contains(buttonId))
                        {
                            groupData.PermissionDic[progId].NoUseButton.Add(buttonId);
                        }
                    }
                }
            }
            return groupData;
        }
    }

     
    public class LibRolePermission
    {
        private string _RoleId;
        private bool _IsUnlimited = false;
        private Dictionary<string, LibPermission> _PermissionDic;

        public Dictionary<string, LibPermission> PermissionDic
        {
            get
            {
                if (_PermissionDic == null)
                    _PermissionDic = new Dictionary<string, LibPermission>();
                return _PermissionDic;
            }
        }

        public string RoleId
        {
            get { return _RoleId; }
            set { _RoleId = value; }
        }

        public bool IsUnlimited
        {
            get { return _IsUnlimited; }
            set { _IsUnlimited = value; }
        }
    }
    [Serializable]
    public class LibPermissionGroup
    {
        private Dictionary<string, LibPermission> _PermissionDic;

        public Dictionary<string, LibPermission> PermissionDic
        {
            get
            {
                if (_PermissionDic == null)
                    _PermissionDic = new Dictionary<string, LibPermission>();
                return _PermissionDic;
            }
        }
    }

    public class LibPermission : ICloneable
    {
        private string _ShowCondition = string.Empty;
        private int _OperateMark;
        private HashSet<string> _NoUseButton;
        private Dictionary<int, Dictionary<string, FieldPower>> _FieldPowerDic;
        private Dictionary<string, List<LibQueryField>> _QueryFieldDic = null;

        public Dictionary<string, List<LibQueryField>> QueryFieldDic
        {
            get
            {
                if (_QueryFieldDic == null)
                    _QueryFieldDic = new Dictionary<string, List<LibQueryField>>();
                return _QueryFieldDic;
            }
            set { _QueryFieldDic = value; }
        }

        public string ShowCondition
        {
            get { return _ShowCondition; }
            set { _ShowCondition = value; }
        }

        public Dictionary<int, Dictionary<string, FieldPower>> FieldPowerDic
        {
            get
            {
                if (_FieldPowerDic == null)
                    _FieldPowerDic = new Dictionary<int, Dictionary<string, FieldPower>>();
                return _FieldPowerDic;
            }
        }

        public int OperateMark
        {
            get { return _OperateMark; }
            set { _OperateMark = value; }
        }


        public HashSet<string> NoUseButton
        {
            get
            {
                if (_NoUseButton == null)
                    _NoUseButton = new HashSet<string>();
                return _NoUseButton;
            }
            set { _NoUseButton = value; }
        }

        public object Clone()
        {
            LibPermission newObj = new LibPermission();
            newObj.ShowCondition = this.ShowCondition;
            newObj.OperateMark = this.OperateMark;
            if (this.QueryFieldDic.Count > 0)
            {
                foreach (var item in this.QueryFieldDic)
                {
                    newObj.QueryFieldDic.Add(item.Key, item.Value);
                }
            }
            foreach (var item in this.FieldPowerDic)
            {
                newObj.FieldPowerDic.Add(item.Key, item.Value);
            }
            foreach (var item in this.NoUseButton)
            {
                newObj.NoUseButton.Add(item);
            }
            return newObj;
        }
    }

    public class FieldPower
    {
        private FieldPowerOption _PowerOption;
        private string _Condition;

        public string Condition
        {
            get { return _Condition; }
            set { _Condition = value; }
        }

        public FieldPowerOption PowerOption
        {
            get { return _PowerOption; }
            set { _PowerOption = value; }
        }
    }

    public enum FieldPowerOption
    {
        CannotBrowse = 0,
        CannotModify = 1
    }
}
