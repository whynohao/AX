/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：部门岗位任职信息的缓存
 * 创建标识：Zhangkj 2017/03/23
 * 
************************************************************************/
using AxCRL.Comm.Redis;
using AxCRL.Comm.Utils;
using AxCRL.Data;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Cache
{
    /// <summary>
    /// 部门中的岗位任职信息的缓存
    /// </summary>
    public class LibDeptDutyPersonCache : MemoryCacheRedis
    {
        /// <summary>
        /// 是否具有按部门岗位审核的功能
        /// Zhangkj 20170324
        /// </summary>
        public readonly static bool HasAduitOfDuty = false;
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static LibDeptDutyPersonCache()
        {
            //初始即查找是否具有按部门岗位审核的相关字段
            LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel("com.Dept");
            if (sqlModel != null && sqlModel.Tables.Count > 1 && sqlModel.Tables[1].Columns.Contains("DUTYID"))
            {
                HasAduitOfDuty = true;
            }
        }

        private static LibDeptDutyPersonCache _Default = null;
        private static object _LockObj = new object();

        public LibDeptDutyPersonCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static LibDeptDutyPersonCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                            _Default = new LibDeptDutyPersonCache("LibDeptDutyPersonCache");
                    }
                }
                return _Default;
            }
        }
        /// <summary>
        /// 从缓存中查找对象，如果没有则从数据库查找并存到缓存中
        /// </summary>
        /// <param name="deptId_p"></param>
        /// <returns></returns>
        public DeptDutyPersonInfo GetCacheItem(string deptId_p)
        {
            string key = deptId_p;

            DeptDutyPersonInfo value = this.Get<DeptDutyPersonInfo>(key);

            if (value == null)
            {
                //检查部门Bcf中是否有岗位任职从表，有则说明存在部门岗位信息              
                if (HasAduitOfDuty == false)
                    return null;

                LibDataAccess dataAccess = new LibDataAccess();              
                string sql = string.Format("Select B.DEPTID,B.DEPTNAME,B.SUPERDEPTID, " +
                                       " E.DUTYID,E.DUTYNAME,E.DUTYLEVEL," +
                                       " A.PERSONORDER,C.PERSONID,C.PERSONNAME" +
                                       " From COMDEPT B "+ //以部门为主查询表，无论其他是否有，部门都要查到
                                       " left join COMDEPTDUTYPERSON  A on B.DEPTID=A.DEPTID " +
                                       " left join COMPERSON C on C.PERSONID=A.PERSONID " +
                                       " left join COMDUTY E on A.DUTYID=E.DUTYID " +//职务表
                                       " Where B.DEPTID={0} ",
                                       LibStringBuilder.GetQuotString(deptId_p));
                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        string deptId = LibSysUtils.ToString(reader["DEPTID"]);
                        string deptName = LibSysUtils.ToString(reader["DEPTNAME"]);
                        string parentDeptId = LibSysUtils.ToString(reader["SUPERDEPTID"]);

                        string dutyId = LibSysUtils.ToString(reader["DUTYID"]);
                        string dutyName = LibSysUtils.ToString(reader["DUTYNAME"]);
                        int dutyLevel = LibSysUtils.ToInt32(reader["DUTYLEVEL"]);

                        string personId = LibSysUtils.ToString(reader["PERSONID"]);
                        string personName = LibSysUtils.ToString(reader["PERSONNAME"]);
                        //对于一个部门中同一岗位下有多个人时，可按照序号从小到大表示职位在部门内的从高到低顺序，例如第一副经理、第二副经理
                        int personOrder = LibSysUtils.ToInt32(reader["PERSONORDER"]);

                        if (value == null)
                            value = new DeptDutyPersonInfo() { DeptId = deptId, DeptName = deptName, ParentDeptId = parentDeptId };
                        if (string.IsNullOrEmpty(dutyId))
                            continue;
                        DutyPeronInfo dutyPerson = value.DutyPersons.Find(item =>
                        {
                            if (item.DutyId.Equals(dutyId))
                                return true;
                            else
                                return false;
                        });
                        if (dutyPerson == null)
                        {
                            dutyPerson = new DutyPeronInfo() { DutyId = dutyId, DutyName = dutyName, DutyLevel = dutyLevel, PersonOrder = personOrder };
                            value.DutyPersons.Add(dutyPerson);                           
                        }
                        if (personOrder < dutyPerson.PersonOrder)
                            dutyPerson.PersonOrder = personOrder;
                        //先按照职级从小到大排序，对于职级相同的不同职位再按位序从大到小排序。方便查找时先找职级低，位序低的人
                        value.DutyPersons = (from item in value.DutyPersons
                                             orderby item.DutyLevel ascending, item.PersonOrder descending
                                             select item).ToList();
                        if (string.IsNullOrEmpty(personId))
                            continue;
                        PersonInfo personInfo = dutyPerson.PersonList.Find(person => {
                            if (person.PersonId.Equals(personId))
                                return true;
                            else
                                return false;
                        });
                        if (personInfo == null)
                        {
                            personInfo = new PersonInfo()
                            {
                                PersonId = personId,
                                PersonName = personName,
                                PersonOrder = personOrder,
                                AsBelongDeptId = deptId,
                                AsBelongDeptName = deptName,
                                AsBeOfficeDutyId = dutyId,
                                AsBeOfficeDutyName = dutyName
                            };
                            dutyPerson.PersonList.Add(personInfo);
                            dutyPerson.PersonList = dutyPerson.PersonList.OrderByDescending(p => p.PersonOrder).ToList();//序号从大到小排列，表示先找位序低的人
                        }
                    }
                }

                if (value != null)
                {                    
                    //180分钟内不访问自动剔除
                    this.Set(key, value, new TimeSpan(0, 180, 0));
                }
            }
            return value;
        }

        public void RemoveCacheItem(string deptId)
        {
            if (string.IsNullOrEmpty(deptId))
                return;
            string key = deptId;
            this.Remove(key);
        }

        /// <summary>
        /// 获取指定部门下的指定岗位的任职人员
        /// </summary>
        /// <param name="deptId"></param>
        /// <param name="gparams"></param>
        /// <param name="execDescs"></param>
        /// <returns></returns>
        public PersonInfo GetDutyPerson(string deptId,GetDutyPersonParams gparams, ref List<string> execDescs)
        {
            if (execDescs == null)
                execDescs = new List<string>();
            DeptDutyPersonInfo dept = this.GetCacheItem(deptId);
            if (dept == null)
            {
                execDescs.Add("待查找的部门代码为空!");
                return null;
            }                
            else
                return dept.GetDutyPerson(gparams,ref execDescs);
        }
        /// <summary>
        /// 查找指定人员在所有部门中的最高任职岗位。如果没有任何任职则返回Null
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public DutyPeronInfo GetHighestDuty(string personId)
        {
            DutyPeronInfo dutyInfo = null;
            if (HasAduitOfDuty == false || string.IsNullOrEmpty(personId))
                return null;
            LibDataAccess dataAccess = new LibDataAccess();
            //审核配置表中包含了岗位相关字段，则多查找相关字段的信息 Zhangkj 20170323
            string sql = string.Format(" Select A.* From COMDUTY A " +
                                       "  Where A.DUTYID in (  " +
                                       "    Select distinct C.DUTYID From COMPERSON B " +
                                       "    inner join COMDEPTDUTYPERSON C on B.PERSONID = C.PERSONID " +
                                       "    where B.PERSONID = {0} " +
                                       "  ) " +
                                       " order by A.DUTYLEVEL desc",
                                   LibStringBuilder.GetQuotString(personId));
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    string dutyId = LibSysUtils.ToString(reader["DUTYID"]);
                    string dutyName = LibSysUtils.ToString(reader["DUTYNAME"]);
                    int dutyLevel = LibSysUtils.ToInt32(reader["DUTYLEVEL"]);

                    dutyInfo = new DutyPeronInfo() { DutyId = dutyId, DutyName = dutyName, DutyLevel = dutyLevel };
                    break;//只取按职级排序的第一个
                }
            }
            return dutyInfo;
        }
    }
    /// <summary>
    /// 获取岗位执行人员时需要的参数
    /// </summary>
    public class GetDutyPersonParams
    {
        /// <summary>
        /// 岗位Id
        /// </summary>
        public string DutyId { get; set; }
        /// <summary>
        /// 岗位级别
        /// </summary>
        public int DutyLevel { get; set; }
        /// <summary>
        /// 岗位名称
        /// </summary>
        public string DutyName { get; set; }
        /// <summary>
        /// 是否岗位上溯
        /// </summary>
        public bool IsDutyUp { get; set; }
        /// <summary>
        /// 是否部门上溯
        /// </summary>
        public bool IsDeptUp { get; set; }
        /// <summary>
        /// 是否不能是本人
        /// </summary>
        public bool IsNotSelf { get; set; }
        /// <summary>
        /// 是否必须比本人职级高
        /// </summary>
        public bool IsMustHighLevel { get; set; }
        /// <summary>
        /// 提交人PersonId
        /// </summary>
        public string SubmitPersonId { get; set; }
        /// <summary>
        /// 提交人的最高岗位职级
        /// </summary>
        public int SubmitPersonMostHighLevel { get; set; }

        /// <summary>
        /// 执行过程所处的层级和步骤说明
        /// </summary>
        public string ProcStepDesc { get; set; }
    }
    /// <summary>
    /// 部门中的岗位任职信息
    /// </summary>
    public class DeptDutyPersonInfo
    {
        private string _DeptId = string.Empty;
        /// <summary>
        /// 部门代码
        /// </summary>
        public string DeptId
        {
            get { return _DeptId; }
            set { _DeptId = value; }
        }

        private string _ParentDeptId = string.Empty;
        /// <summary>
        /// 上级部门代码
        /// </summary>
        public string ParentDeptId
        {
            get { return _ParentDeptId; }
            set { _ParentDeptId = value; }
        }

        private string _DeptName = string.Empty;
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DeptName
        {
            get { return _DeptName; }
            set { _DeptName = value; }
        }

        private List<DutyPeronInfo> _DutyPersons = new List<DutyPeronInfo>();
        /// <summary>
        /// 部门下的每个岗位的任职人员信息表
        /// 按职级的从低到高排序，再按在同一部门中任职序号从高到低排序
        /// </summary>
        public List<DutyPeronInfo> DutyPersons
        {
            get { return _DutyPersons; }
            set { _DutyPersons = value; }
        }
        /// <summary>
        /// 根据岗位Id查找部门下岗位的任职人员
        /// </summary>
        /// <param name="gparams">查找时的参数</param>
        /// <param name="execDescs">执行中的特殊说明</param>
        /// <returns></returns>
        public PersonInfo GetDutyPerson(GetDutyPersonParams gparams,ref List<string> execDescs)
        {
            if (execDescs == null)
                execDescs = new List<string>();
            if (gparams == null)
            {
                execDescs.Add("查找参数集合为空。");
                return null;
            }               
            string dutyId = gparams.DutyId;
            int dutyLevel = gparams.DutyLevel;
            bool isDutyUp = gparams.IsDutyUp;
            bool isDeptUp = gparams.IsDeptUp;
            bool isNotSelf = gparams.IsNotSelf;
            bool isMustHighLevel = gparams.IsMustHighLevel;
            string submitPersonId = gparams.SubmitPersonId;
            int submitPersonMostHighLevel = gparams.SubmitPersonMostHighLevel;

            if (string.IsNullOrEmpty(dutyId))
            {
                execDescs.Add("待查找的岗位代码为空。");
                return null;
            }
            PersonInfo person = null;
            int index = 0;
            DutyPeronInfo dutyPerson = null;

            int dutyLevelUpIndex = int.MinValue;//比需要的职位级别高的职位的索引
            //先查找有没有完全匹配的岗位任职人员
            while (index < this.DutyPersons.Count)
            {
                if (dutyLevelUpIndex == int.MinValue && this.DutyPersons[index].DutyLevel >= dutyLevel)
                    dutyLevelUpIndex = index;
                if (this.DutyPersons[index].DutyId.Equals(dutyId))
                {
                    if (isMustHighLevel == false)
                    {
                        dutyPerson = this.DutyPersons[index];
                        break;
                    }                        
                    else
                    {
                        //如果审核人职级必须要比提交人职级高，则需要继续判断职级
                        if (this.DutyPersons[index].DutyLevel > submitPersonMostHighLevel)
                        {
                            dutyPerson = this.DutyPersons[index];
                            break;
                        }
                        execDescs.Add(string.Format("设定的岗位:{0}级别没有比提交人的最高职级高。{1}", gparams.DutyName, gparams.ProcStepDesc));
                    }                   
                }
                index++;
            }
            person = GetPerson(dutyPerson, submitPersonId, isNotSelf,ref execDescs);
            if (person == null)
            {
                if (isDutyUp)
                {
                    execDescs.Add(string.Format("目标岗位:{2}。在部门:{0}中执行了岗位上溯。{1}", this.DeptName, gparams.ProcStepDesc, gparams.DutyName));
                    //先岗位上溯
                    if (dutyLevelUpIndex != int.MinValue)//dutyLevelUpIndex=int.MinValue表示当前部门完全没有岗位人员，或者没有比需要的岗位高的
                    {
                        index = dutyLevelUpIndex;//从当前部门中比目标岗位级别高的岗位开始查找                    
                        while (index < this.DutyPersons.Count)
                        {                            
                            dutyPerson = this.DutyPersons[index];
                            person = GetPerson(dutyPerson, submitPersonId, isNotSelf,ref execDescs);
                            if (person != null)
                                return person;
                            index++;//继续查找本部门下更高岗位
                        }
                    }                  
                    //本部门的岗位上溯未找到审核人,则从上级部门查找
                    if (isDeptUp)
                    {
                        execDescs.Add(string.Format("目标岗位:{2}。在部门:{0}中执行了部门上溯。{1}", this.DeptName, gparams.ProcStepDesc, gparams.DutyName));
                        if (string.IsNullOrEmpty(this.ParentDeptId) == false)
                        {
                            DeptDutyPersonInfo parnetDeptInfo = LibDeptDutyPersonCache.Default.GetCacheItem(this.ParentDeptId);
                            if (parnetDeptInfo != null)
                                return parnetDeptInfo.GetDutyPerson(gparams, ref execDescs);
                            else
                            {
                                execDescs.Add(string.Format("部门:{0}的上级部门(代码为:{1})的任职信息为空。{2}", this.DeptName, this.ParentDeptId, gparams.ProcStepDesc));
                                return null;
                            }                                
                        }
                        else
                        {
                            execDescs.Add(string.Format("部门:{0}的上级部门为空。{1}", this.DeptName, gparams.ProcStepDesc));
                            return null;
                        }
                    }
                    else
                    {
                        execDescs.Add(string.Format("此步骤:{0}设置为不允许部门上溯。部门:{1}。", gparams.ProcStepDesc, this.DeptName));
                        return null;//不允许部门上溯或不存在上级部门，且本部门又未找到
                    }                       
                }
                else
                {
                    execDescs.Add(string.Format("此步骤:{0}设置为不允许岗位上溯。部门:{1}。", gparams.ProcStepDesc, this.DeptName));
                    return null;//不允许岗位上溯，而且没找到
                }                    
            }
            else
                return person;
        }
        /// <summary>
        /// 从岗位任职表中查找第一个任职人员
        /// </summary>
        /// <param name="dutyPerson"></param>
        /// <param name="submitPersonId"></param>
        /// <param name="isNotSelf">是否不能是本人</param>
        /// <param name="execDescs"></param>
        /// <returns></returns>
        private PersonInfo GetPerson(DutyPeronInfo dutyPerson, string submitPersonId,bool isNotSelf,ref List<string> execDescs)
        {
            if (dutyPerson == null || dutyPerson.PersonList == null || dutyPerson.PersonList.Count == 0)
                return null;
            if (isNotSelf == false)
                return dutyPerson.PersonList.First();
            foreach(PersonInfo info in dutyPerson.PersonList)
            {
                if (info == null)
                    continue;
                if (info.PersonId.Equals(submitPersonId) == false)
                {
                    return info;
                }
                else
                {
                    if (isNotSelf)
                    {
                        if (execDescs == null)
                            execDescs = new List<string>();
                        execDescs.Add(string.Format("部门:{0}中的岗位:{1}任职人之一是提交人本人,不能作为审核人。", this.DeptName, dutyPerson.DutyName));
                        continue;
                    }
                    else
                        return info;                    
                }
            }
            return null;            
        }
    }
    /// <summary>
    /// 岗位任职信息
    /// </summary>
    public class DutyPeronInfo
    {
        private string _DutyId = string.Empty;
        /// <summary>
        /// 岗位代码
        /// </summary>
        public string DutyId
        {
            get { return _DutyId; }
            set { _DutyId = value; }
        }
        private string _DutyName = string.Empty;
        /// <summary>
        /// 岗位名称
        /// </summary>
        public string DutyName
        {
            get { return _DutyName; }
            set { _DutyName = value; }
        }

        private int _DutyLevel = 0;
        /// <summary>
        /// 岗位等级
        /// </summary>
        public int DutyLevel
        {
            get { return _DutyLevel; }
            set { _DutyLevel = value; }
        }
        private int _PersonOrder = 0;
        /// <summary>
        /// 职级相同的不同高位也按照此序号,从小到大表示位序从高到低
        /// </summary>
        public int PersonOrder
        {
            get { return _PersonOrder; }
            set { _PersonOrder = value; }
        }

        private List<PersonInfo> _PersonList = new List<PersonInfo>();
        /// <summary>
        /// 岗位下的任职人员。同级岗位下的人员按Order顺序从小到大排列，表示位序从高到低
        /// </summary>
        public List<PersonInfo> PersonList
        {
            get { return _PersonList;  }
            set { _PersonList = value; }
        }
    }
    /// <summary>
    /// 人员信息
    /// </summary>
    public class PersonInfo
    {
        private string _PersonId = string.Empty;
        /// <summary>
        /// 人员代码
        /// </summary>
        public string PersonId
        {
            get { return _PersonId; }
            set { _PersonId = value; }
        }
        private string _PersonName = string.Empty;
        /// <summary>
        /// 人员名称
        /// </summary>
        public string PersonName
        {
            get { return _PersonName; }
            set { _PersonName = value; }
        }
        private int _PersonOrder = 0;
        /// <summary>
        /// 岗位任职表中相同岗位的高低序号，对于一个部门中同一岗位下有多个人时，可按照序号从小到大表示职位在部门内的从高到低顺序，例如第一副经理、第二副经理
        /// </summary>
        public int PersonOrder
        {
            get { return _PersonOrder; }
            set { _PersonOrder = value; }
        }

        private string _AsBelongDeptId = string.Empty;
        /// <summary>
        /// 以在此Id标识的部门的某个岗位的任职来作为审核人
        /// </summary>
        public string AsBelongDeptId
        {
            get { return _AsBelongDeptId; }
            set { _AsBelongDeptId = value; }
        }
        private string _AsBelongDeptName = string.Empty;
        /// <summary>
        /// 以在此部门的某个岗位的任职来作为审核人
        /// </summary>
        public string AsBelongDeptName
        {
            get { return _AsBelongDeptName; }
            set { _AsBelongDeptName = value; }
        }

        private string _AsBeOfficeDutyId = string.Empty;
        /// <summary>
        /// 以某个岗位的任职人员来作为审核人
        /// </summary>
        public string AsBeOfficeDutyId
        {
            get { return _AsBeOfficeDutyId; }
            set { _AsBeOfficeDutyId = value; }
        }
        private string _AsBeOfficeDutyName = string.Empty;
        /// <summary>
        /// 以某个岗位的任职人员来作为审核人
        /// </summary>
        public string AsBeOfficeDutyName
        {
            get { return _AsBeOfficeDutyName; }
            set { _AsBeOfficeDutyName = value; }
        }
    }
}
