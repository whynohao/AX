using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Core.Comm;
using AxCRL.Core.SysNews;
using AxCRL.Data;
using AxCRL.Data.SqlBuilder;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AxCRL.Bcf.ScheduleTask
{
    public class LibScheduleTaskHost
    {
        private static LibScheduleTaskHost _Default = null;
        private static object _LockObj = new object();
        private static ConcurrentDictionary<string, LibTask> _TaskList = null;
        private static ConcurrentDictionary<string, string> _TaskMap = null;

        public static ConcurrentDictionary<string, string> TaskMap
        {
            get { return _TaskMap; }
        }

        public static ConcurrentDictionary<string, LibTask> TaskList
        {
            get
            {
                return _TaskList;
            }
        }

        private LibScheduleTaskHost()
        {
            _TaskList = new ConcurrentDictionary<string, LibTask>();
            _TaskMap = new ConcurrentDictionary<string, string>();
        }

        public static LibScheduleTaskHost Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                            _Default = new LibScheduleTaskHost();
                    }
                }
                return _Default;
            }
        }

        public void InitTask()
        {
            if (EnvProvider.Default.ScheduleTaskOpened) //控制仅可初始化一次
                return;
            List<LibBusinessTask> taskList = LibBusinessTaskCache.Default.GetCacheItemList();
            foreach (LibBusinessTask task in taskList)
            {
                AddTask(task);
            }
            taskList = InitTempTask();
            foreach (LibBusinessTask task in taskList)
            {
                if (!TaskMap.ContainsKey(task.TaskId)) //考虑到排程任务后面才打开，已经存在了相关临时业务任务
                    AddTask(task);
            }
            EnvProvider.Default.ScheduleTaskOpened = true;
        }

        public List<LibBusinessTask> InitTempTask()
        {
            List<LibBusinessTask> list = new List<LibBusinessTask>();
            LibDataAccess dataAccess = new LibDataAccess();
            using (IDataReader reader = dataAccess.ExecuteDataReader("select TASKID,PROGID,BUSINESSTASKID,EXECDATE,EXECTIME,EXECCONDITION,INTERNALID from AXPBUSINESSTEMPTASK", false))
            {
                while (reader.Read())
                {
                    LibBusinessTask task = new LibBusinessTask()
                    {
                        TaskType = LibTaskType.TempTask,
                        TaskId = LibSysUtils.ToString(reader["TASKID"]),
                        ProgId = LibSysUtils.ToString(reader["PROGID"]),
                        BusinessTaskId = LibSysUtils.ToString(reader["BUSINESSTASKID"]),
                        ExecDate = LibSysUtils.ToInt32(reader["EXECDATE"]),
                        ExecCondition = LibSysUtils.ToString(reader["EXECCONDITION"]),
                        InternalId = LibSysUtils.ToString(reader["INTERNALID"])
                    };
                    task.ExecTime.Add(LibSysUtils.ToInt32(reader["EXECTIME"]));
                    list.Add(task);
                }
            }
            return list;
        }


        public void AddTask(LibBusinessTask task, bool needAddDB = false)
        {
            int curDate = LibDateUtils.GetCurrentDate();
            if (string.IsNullOrEmpty(task.ProgId) || string.IsNullOrEmpty(task.BusinessTaskId))
                return;
            //如果指定的日期大于当前日期，无需执行
            if (task.TaskType == LibTaskType.None && task.ExecDate != 0 && task.ExecDate > curDate)
                return;
            //未指定有效执行日期则跳过
            if (task.ExecDate == 0 && task.RepeatDateMark == 0)
                return;
            //无设置执行时间点则跳过
            if (task.IntervalTime == 0 && task.ExecTime.Count == 0)
                return;
            //初始化Timer
            LibTaskParam param = new LibTaskParam(task);
            string key = Guid.NewGuid().ToString();
            param.Task = new LibTask(new Timer(ExecBusinessTask, param, param.GetTaskDueTime(), Timeout.InfiniteTimeSpan));
            TaskList.TryAdd(key, param.Task);
            TaskMap.TryAdd(task.TaskId, key);
            if (task.TaskType == LibTaskType.TempTask && needAddDB)
            {
                LibDataAccess dataAccess = new LibDataAccess();
                dataAccess.ExecuteNonQuery(string.Format("insert into AXPBUSINESSTEMPTASK(TASKID,PROGID,BUSINESSTASKID,EXECDATE,EXECTIME,EXECCONDITION,INTERNALID) values({0},{1},{2},{3},{4},{5},{6})",
                    LibStringBuilder.GetQuotString(task.TaskId), LibStringBuilder.GetQuotString(task.ProgId), LibStringBuilder.GetQuotString(task.BusinessTaskId), task.ExecDate, task.ExecTime[0],
                    LibStringBuilder.GetQuotString(task.ExecCondition), LibStringBuilder.GetQuotString(task.InternalId)), false);
            }
        }

        public void DeleteTask(string taskId)
        {
            string key = string.Empty; ;
            if (TaskMap.TryGetValue(taskId, out key))
            {
                LibTask task = null;
                if (TaskList.TryGetValue(key, out task))
                {
                    task.Timer.Dispose();
                    TaskList.TryRemove(key, out task);
                    TaskMap.TryRemove(taskId, out key);
                }
            }
        }

        private void ExecBusinessTask(object obj)
        {
            LibTaskParam param = (LibTaskParam)obj;
            LibBusinessTask taskDefine = param.TaskDefine;
            //系统当天是否可以执行
            if (IsExecOfDay(taskDefine))
            {
                try
                {
                    param.Task.TaskState = TaskRunState.Running;
                    LibBcfBase bcf = LibBcfSystem.Default.GetBcfInstance(taskDefine.ProgId);
                    bcf.Handle = LibHandleCache.Default.GetSystemHandle();
                    Type type = bcf.GetType();
                    object[] destParam = RestoreParamFormat(type, taskDefine.BusinessTaskId, new string[] { taskDefine.ExecCondition });
                    object result = bcf.GetType().InvokeMember(taskDefine.BusinessTaskId, BindingFlags.InvokeMethod, null, bcf, destParam);
                    switch (bcf.Template.BillType)
                    {
                        case AxCRL.Template.BillType.Master:
                        case AxCRL.Template.BillType.Bill:
                            break;
                        case AxCRL.Template.BillType.Grid:
                            break;
                        case AxCRL.Template.BillType.DataFunc:
                            break;
                        case AxCRL.Template.BillType.Rpt:
                        case AxCRL.Template.BillType.DailyRpt:
                            DataSet dataSet = result as DataSet;
                            if (dataSet != null)
                            {
                                LibSysNews news = new LibSysNews();
                                news.Content = taskDefine.MainContent;
                                news.Data = LibBillDataSerializeHelper.Serialize(dataSet);
                                news.PersonId = "SYSTEM";
                                news.ProgId = taskDefine.ProgId;
                                news.Title = taskDefine.Title;
                                foreach (LibBusinessTaskLiaison item in taskDefine.Liaison)
                                {
                                    news.UserList.Add(item.UserId);
                                }
                                LibSysNewsHelper.SendNews(news, false);
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    //将错误输出
                    string path = System.IO.Path.Combine(AxCRL.Comm.Runtime.EnvProvider.Default.MainPath, "Output", "Error", "ScheduleTask", string.Format("{0}.txt", DateTime.Now.Ticks));
                    OutputInfo(path, ex.ToString());
                }
                finally
                {
                    param.Task.TaskState = TaskRunState.Wait;
                }
            }
            bool canNextExec = true;
            if (taskDefine.ExecDate != 0)
            {
                int curDate = LibDateUtils.GetCurrentDate();
                if (taskDefine.ExecDate > curDate)
                {
                    param.Task.Timer.Dispose();
                    canNextExec = false;
                    param.Task.TaskState = TaskRunState.Stop;
                }
                else if (taskDefine.ExecDate == curDate && taskDefine.ExecTime.Count > 0)
                {
                    int curTime = LibDateUtils.GetLibTimePart(LibDateUtils.GetCurrentDateTime(), LibDateTimePartEnum.Time);
                    if (taskDefine.ExecTime[taskDefine.ExecTime.Count - 1] < curTime)
                    {
                        param.Task.Timer.Dispose();
                        canNextExec = false;
                        param.Task.TaskState = TaskRunState.Stop;
                    }
                }
            }
            if (canNextExec)
            {
                param.Task.Timer.Change(param.GetTaskDueTime(), Timeout.InfiniteTimeSpan);
            }
            else if (taskDefine.TaskType == LibTaskType.TempTask)
            {
                //删除临时任务
                LibDataAccess dataAccess = new LibDataAccess();
                dataAccess.ExecuteNonQuery(string.Format("delete AXPBUSINESSTEMPTASK where TASKID={0}", LibStringBuilder.GetQuotString(taskDefine.TaskId)), false);
            }
        }

        private int GetNextIndex(int count, int index)
        {
            if (index > count)
                return 0;
            else
                return index;
        }

        private object[] RestoreParamFormat(Type destType, string method, string[] param)
        {
            object[] destParam = null;
            ParameterInfo[] paramInfo = destType.GetMethod(method).GetParameters();
            int length = paramInfo.Length;
            if (length > 0)
            {
                destParam = new object[length];
                for (int i = 0; i < param.Length; i++)
                {
                    destParam[i] = JsonConvert.DeserializeObject(param[i], paramInfo[i].ParameterType);
                }
            }
            return destParam;
        }



        private void OutputInfo(string path, string info)
        {
            using (System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                {
                    sw.Write(info);
                }
            }
        }


        private bool IsExecOfDay(LibBusinessTask task)
        {
            bool needExec = false;
            int curDate = LibDateUtils.GetCurrentDate();
            if (task.ExecDate == 0)
            {
                if (task.RepeatDateMark != 0)
                {
                    needExec = IsExecOfDay(task.RepeatDateMark, curDate);
                    if (task.IsJustWorkDay)
                    {
                        string calendarId = task.CalendarId;
                        if (!string.IsNullOrEmpty(calendarId))
                        {
                            CalendarData calendarData = new CalendarData(calendarId);
                            needExec = IsWorkDay(curDate, calendarData);
                        }
                    }
                }
            }
            else
            {
                needExec = task.ExecDate == curDate;
            }
            return needExec;
        }

        private bool IsExecOfDay(int repeatDateMark, int execDate)
        {
            DayOfWeek dayOfWeek = LibDateUtils.LibDateToDateTime(execDate).DayOfWeek;
            int dayOfWeekValue = 0;
            switch (dayOfWeek)
            {
                case DayOfWeek.Friday:
                    dayOfWeekValue = (int)Math.Pow(2, 4);
                    break;
                case DayOfWeek.Monday:
                    dayOfWeekValue = (int)Math.Pow(2, 0);
                    break;
                case DayOfWeek.Saturday:
                    dayOfWeekValue = (int)Math.Pow(2, 5);
                    break;
                case DayOfWeek.Sunday:
                    dayOfWeekValue = (int)Math.Pow(2, 6);
                    break;
                case DayOfWeek.Thursday:
                    dayOfWeekValue = (int)Math.Pow(2, 3);
                    break;
                case DayOfWeek.Tuesday:
                    dayOfWeekValue = (int)Math.Pow(2, 1);
                    break;
                case DayOfWeek.Wednesday:
                    dayOfWeekValue = (int)Math.Pow(2, 2);
                    break;
            }
            bool isExec = (repeatDateMark & dayOfWeekValue) != 0;
            return isExec;
        }

        private bool IsWorkDay(int curDate, CalendarData calendarData)
        {
            bool isWork = false;
            int year = LibDateUtils.GetLibDatePart(curDate, LibDateTimePartEnum.Year);
            if (calendarData.WorkDayList.ContainsKey(year))
            {
                int month = LibDateUtils.GetLibDatePart(curDate, LibDateTimePartEnum.Month);
                int day = LibDateUtils.GetLibDatePart(curDate, LibDateTimePartEnum.Day);
                int calendar = calendarData.WorkDayList[year][month];
                int temp = (int)Math.Pow(2, day);
                isWork = (calendar & temp) != 0;
            }
            else
                isWork = true;
            return isWork;
        }

        private class CalendarData
        {
            private Dictionary<int, int[]> _WorkDayList;

            public Dictionary<int, int[]> WorkDayList
            {
                get
                {
                    if (_WorkDayList == null)
                        _WorkDayList = new Dictionary<int, int[]>();
                    return _WorkDayList;
                }
            }

            public CalendarData(string calendarId)
            {
                if (string.IsNullOrEmpty(calendarId))
                    return;
                LibDataAccess dataAccess = new LibDataAccess();
                SqlBuilder sqlBuilder = new SqlBuilder("com.Calendar");
                string sql = sqlBuilder.GetQuerySql(1, "B.YEAR,B.MONTH,B.WORKMARK", string.Format("B.CALENDARID={0}", LibStringBuilder.GetQuotString(calendarId)));
                using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                {
                    while (reader.Read())
                    {
                        int year = LibSysUtils.ToInt32(reader[1]);
                        if (!WorkDayList.ContainsKey(year))
                            WorkDayList.Add(year, new int[12]);
                        int[] items = WorkDayList[year];
                        int month = LibSysUtils.ToInt32(reader["MONTH"]);
                        items[month - 1] = LibSysUtils.ToInt32(reader["WORKMARK"]);
                    }
                }
            }
        }

    }

    public class LibTask
    {
        private Timer _Timer;
        private TaskRunState _TaskState;

        public TaskRunState TaskState
        {
            get { return _TaskState; }
            set { _TaskState = value; }
        }

        public Timer Timer
        {
            get { return _Timer; }
            set { _Timer = value; }
        }

        public LibTask(Timer timer)
        {
            Timer = timer;
            TaskState = TaskRunState.Wait;
        }
    }

    public enum TaskRunState
    {
        Wait = 0,
        Running = 1,
        Stop = 2
    }

    public class LibTaskParam
    {
        private LibBusinessTask _TaskDefine;
        private LibTask _Task;

        public LibTask Task
        {
            get { return _Task; }
            set { _Task = value; }
        }
        public LibBusinessTask TaskDefine
        {
            get { return _TaskDefine; }
            set { _TaskDefine = value; }
        }

        public LibTaskParam(LibBusinessTask taskDefine)
        {
            TaskDefine = taskDefine;
        }

        public TimeSpan GetTaskDueTime()
        {
            TimeSpan dueTime = TimeSpan.Zero;
            long currentDateTime = LibDateUtils.GetCurrentDateTime();
            int curDate = LibDateUtils.GetLibTimePart(currentDateTime, LibDateTimePartEnum.Date);
            int currentTime = LibDateUtils.GetLibTimePart(currentDateTime, LibDateTimePartEnum.Time);
            if (TaskDefine.ExecTime.Count > 0) //指定时间点执行情况
            {
                for (int i = 0; i < TaskDefine.ExecTime.Count; i++)
                {
                    int execTime = TaskDefine.ExecTime[i];
                    if (execTime < 9999)
                        execTime *= 100;
                    if (currentTime < execTime)
                    {
                        long endTime = (long)curDate * 1000000 + execTime;
                        dueTime = LibDateUtils.LibDateToDateTime(endTime) - LibDateUtils.LibDateToDateTime(currentDateTime);
                        break;
                    }
                    if (TaskDefine.ExecTime.Count == i + 1)
                    {
                        if (TaskDefine.TaskType == LibTaskType.None)
                        {
                            long startTime = (long)curDate * 1000000 + execTime;
                            long endTime = (long)LibDateUtils.AddDayToLibDate(LibDateUtils.LibDateToDateTime(curDate), 1) * 1000000 + TaskDefine.ExecTime[0] * 100;
                            dueTime = LibDateUtils.LibDateToDateTime(endTime) - LibDateUtils.LibDateToDateTime(startTime);
                        }
                    }
                }
            }
            else if (TaskDefine.IntervalTime != 0)//间隔时间执行情况
            {
                dueTime = new TimeSpan(0, TaskDefine.IntervalTime, 0);
            }
            return dueTime;
        }
    }

}
