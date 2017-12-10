using AxCRL.Comm.Utils;
using AxCRL.Data;
using AxCRL.Comm.Redis;
using AxCRL.Data.SqlBuilder;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;

namespace AxCRL.Core.Cache
{
    public class LibBusinessTaskCache : MemoryCacheRedis
    {
        private static LibBusinessTaskCache _Default = null;
        private static object _LockObj = new object();

        public LibBusinessTaskCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static   LibBusinessTaskCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibBusinessTaskCache("LibBusinessTaskCache");
                            InitBusinessTask();
                        }
                    }
                }
                return _Default;
            }
        }

        public LibBusinessTask GetCacheItem(string taskId)
        {
            LibBusinessTask task = this.Get< LibBusinessTask>(taskId)  ;
            if (task == null)
            {
                task = GetBusinessTask(taskId);
                Default.Set(taskId, task);
            }
            return task;
        }

        public List<LibBusinessTask> GetCacheItemList()
        {
            List<LibBusinessTask> taskList = new List<LibBusinessTask>();
            IEnumerator<string> enumerator = Default.GetKeys();
            while (enumerator.MoveNext())
            {
                taskList.Add(this.Get< LibBusinessTask>(enumerator.Current.ToString()));
            }
            return taskList;
        }

        public void RemoveCacheItem(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
                return;
            //移除并重新获取
            this.Remove(taskId);
        }

        public void AddCacheItem(LibBusinessTask task)
        {
            Default.Set (task.TaskId, task);//时间是永久
        }

        private LibBusinessTask GetBusinessTask(string taskId)
        {
            SqlBuilder sqlBuilder = new SqlBuilder("axp.ScheduleTask");
            string sql = sqlBuilder.GetQuerySql(0, "A.TASKID,A.PROGID,A.BUSINESSTASKID,A.CALENDARID,A.ISJUSTWORKDAY,A.INTERVALTIME,A.EXECDATE,A.REPEATDATEMARK,A.EXECCONDITION,A.TITLE,A.MAINCONTENT,B.EXECTIME",
                string.Format("A.TASKID={0}", LibStringBuilder.GetQuotString(taskId)), "B.EXECTIME");
            LibDataAccess dataAccess = new LibDataAccess();
            LibBusinessTask task = null;
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
            {
                while (reader.Read())
                {
                    if (task == null)
                    {
                        task = new LibBusinessTask();
                        task.BusinessTaskId = LibSysUtils.ToString(reader["BUSINESSTASKID"]);
                        task.CalendarId = LibSysUtils.ToString(reader["CALENDARID"]);
                        task.ExecCondition = LibSysUtils.ToString(reader["ExecCondition"]);
                        task.ExecDate = LibSysUtils.ToInt32(reader["EXECDATE"]);
                        task.IntervalTime = LibSysUtils.ToInt32(reader["INTERVALTIME"]);
                        task.IsJustWorkDay = LibSysUtils.ToBoolean(reader["ISJUSTWORKDAY"]);
                        task.ProgId = LibSysUtils.ToString(reader["PROGID"]);
                        task.Title = LibSysUtils.ToString(reader["TITLE"]);
                        task.MainContent = LibSysUtils.ToString(reader["MAINCONTENT"]);
                        task.RepeatDateMark = LibSysUtils.ToInt32(reader["REPEATDATEMARK"]);
                        task.TaskId = taskId;
                    }
                    task.ExecTime.Add(LibSysUtils.ToInt32(reader["EXECTIME"]));
                }
            }
            sql = string.Format("select distinct A.TASKID,A.PERSONID,D.DEPTID,C.USERID from AXPSCHEDULETASKPERSON A " +
                 "inner join AXPSCHEDULETASK B on B.TASKID=A.TASKID left join COMPERSON D on D.PERSONID=A.PERSONID inner join AXPUSER C on C.PERSONID=A.PERSONID " +
                 "where B.TASKID={0}", LibStringBuilder.GetQuotString(taskId));
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql, false))
            {
                while (reader.Read())
                {
                    task.Liaison.Add(new LibBusinessTaskLiaison()
                    {
                        PersonId = LibSysUtils.ToString(reader["PERSONID"]),
                        DeptId = LibSysUtils.ToString(reader["DEPTID"]),
                        UserId = LibSysUtils.ToString(reader["USERID"])
                    });
                }
            }
            return task;
        }

        private static void InitBusinessTask()
        {
            Dictionary<string, LibBusinessTask> taskList = new Dictionary<string, LibBusinessTask>();
            int currentDate = LibDateUtils.GetCurrentDate();
            SqlBuilder sqlBuilder = new SqlBuilder("axp.ScheduleTask");
            string sql = sqlBuilder.GetQuerySql(0, "A.TASKID,A.PROGID,A.BUSINESSTASKID,A.CALENDARID,A.ISJUSTWORKDAY,A.INTERVALTIME,A.EXECDATE,A.REPEATDATEMARK,A.EXECCONDITION,A.TITLE,A.MAINCONTENT,B.EXECTIME",
                string.Format("A.CURRENTSTATE=2 and (A.VALIDITYSTARTDATE <= {0} and (A.VALIDITYENDDATE>{0} or A.VALIDITYENDDATE = 0))", currentDate), "A.TASKID,B.EXECTIME");
            LibDataAccess dataAccess = new LibDataAccess();
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql, false))
            {
                while (reader.Read())
                {
                    string taskId = LibSysUtils.ToString(reader["TASKID"]);
                    int execTime = LibSysUtils.ToInt32(reader["EXECTIME"]);
                    if (taskList.ContainsKey(taskId))
                    {
                        if (execTime > 0)
                            taskList[taskId].ExecTime.Add(execTime);
                    }
                    else
                    {
                        LibBusinessTask task = new LibBusinessTask();
                        task.BusinessTaskId = LibSysUtils.ToString(reader["BUSINESSTASKID"]);
                        task.CalendarId = LibSysUtils.ToString(reader["CALENDARID"]);
                        task.ExecCondition = LibSysUtils.ToString(reader["ExecCondition"]);
                        task.ExecDate = LibSysUtils.ToInt32(reader["EXECDATE"]);
                        task.IntervalTime = LibSysUtils.ToInt32(reader["INTERVALTIME"]);
                        task.IsJustWorkDay = LibSysUtils.ToBoolean(reader["ISJUSTWORKDAY"]);
                        task.ProgId = LibSysUtils.ToString(reader["PROGID"]);
                        task.Title = LibSysUtils.ToString(reader["TITLE"]);
                        task.MainContent = LibSysUtils.ToString(reader["MAINCONTENT"]);
                        task.RepeatDateMark = LibSysUtils.ToInt32(reader["REPEATDATEMARK"]);
                        task.TaskId = taskId;
                        if (execTime > 0)
                            task.ExecTime.Add(execTime);
                        taskList.Add(taskId, task);
                    }
                }
            }
            sql = string.Format("select distinct A.TASKID,A.PERSONID,D.DEPTID,C.USERID from AXPSCHEDULETASKPERSON A " +
                "inner join AXPSCHEDULETASK B on B.TASKID=A.TASKID left join COMPERSON D on D.PERSONID=A.PERSONID inner join AXPUSER C on C.PERSONID=A.PERSONID " +
                "where B.CURRENTSTATE=2 and (B.VALIDITYSTARTDATE <= {0} and (B.VALIDITYENDDATE>{0} or B.VALIDITYENDDATE = 0))", currentDate);
            using (IDataReader reader = dataAccess.ExecuteDataReader(sql, false))
            {
                while (reader.Read())
                {
                    string taskId = LibSysUtils.ToString(reader["TASKID"]);
                    if (taskList.ContainsKey(taskId))
                    {
                        taskList[taskId].Liaison.Add(new LibBusinessTaskLiaison()
                        {
                            PersonId = LibSysUtils.ToString(reader["PERSONID"]),
                            DeptId = LibSysUtils.ToString(reader["DEPTID"]),
                            UserId = LibSysUtils.ToString(reader["USERID"])
                        });
                    }
                }
            }
            foreach (var item in taskList)
            {
                _Default.Set(item.Key, item.Value);
            }
        }
    }
  
    public class LibBusinessTask
    {
        private string _TaskId;
        private string _ProgId;
        private string _BusinessTaskId;
        private string _CalendarId;
        private bool _IsJustWorkDay;
        private int _IntervalTime;
        private int _ExecDate;
        private int _RepeatDateMark;
        private string _ExecCondition;
        private List<int> _ExecTime;
        private List<LibBusinessTaskLiaison> _Liaison;
        private string _Title;
        private string _MainContent;
        private LibTaskType _TaskType = LibTaskType.None;
        private string _InternalId;

        public string InternalId
        {
            get { return _InternalId; }
            set { _InternalId = value; }
        }

        public LibTaskType TaskType
        {
            get { return _TaskType; }
            set { _TaskType = value; }
        }

        public string MainContent
        {
            get { return _MainContent; }
            set { _MainContent = value; }
        }

        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        public List<LibBusinessTaskLiaison> Liaison
        {
            get
            {
                if (_Liaison == null)
                    _Liaison = new List<LibBusinessTaskLiaison>();
                return _Liaison;
            }
        }

        public List<int> ExecTime
        {
            get
            {
                if (_ExecTime == null)
                    _ExecTime = new List<int>();
                return _ExecTime;
            }
        }

        public string ExecCondition
        {
            get { return _ExecCondition; }
            set { _ExecCondition = value; }
        }

        public int RepeatDateMark
        {
            get { return _RepeatDateMark; }
            set { _RepeatDateMark = value; }
        }

        public int ExecDate
        {
            get { return _ExecDate; }
            set { _ExecDate = value; }
        }

        public int IntervalTime
        {
            get { return _IntervalTime; }
            set { _IntervalTime = value; }
        }

        public bool IsJustWorkDay
        {
            get { return _IsJustWorkDay; }
            set { _IsJustWorkDay = value; }
        }

        public string CalendarId
        {
            get { return _CalendarId; }
            set { _CalendarId = value; }
        }

        public string BusinessTaskId
        {
            get { return _BusinessTaskId; }
            set { _BusinessTaskId = value; }
        }

        public string ProgId
        {
            get { return _ProgId; }
            set { _ProgId = value; }
        }

        public string TaskId
        {
            get { return _TaskId; }
            set { _TaskId = value; }
        }
    }

    public class LibBusinessTaskLiaison
    {
        private string _UserId;
        private string _PersonId;
        private string _DeptId;

        public string UserId
        {
            get { return _UserId; }
            set { _UserId = value; }
        }

        public string DeptId
        {
            get { return _DeptId; }
            set { _DeptId = value; }
        }

        public string PersonId
        {
            get { return _PersonId; }
            set { _PersonId = value; }
        }
    }

    public enum LibTaskType
    {
        None = 0,
        TempTask = 1
    }
}
