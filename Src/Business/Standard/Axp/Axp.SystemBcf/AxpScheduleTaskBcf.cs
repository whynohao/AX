using AxCRL.Bcf;
using AxCRL.Bcf.ScheduleTask;
using AxCRL.Comm.Define;
using AxCRL.Comm.Runtime;
using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Data.SqlBuilder;
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
    [ProgId(ProgId = "axp.ScheduleTask", ProgIdType = ProgIdType.Bcf, VclPath = @"/Scripts/module/mes/axp/axpScheduleTaskVcl.js")]
    public class AxpScheduleTaskBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new AxpScheduleTaskBcfTemplate("axp.ScheduleTask");
        }

        protected override void BeforeUpdate()
        {
            base.BeforeUpdate();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            bool isJustWorkDay = LibSysUtils.ToBoolean(masterRow["ISJUSTWORKDAY"]);
            if (isJustWorkDay && string.IsNullOrEmpty(LibSysUtils.ToString(masterRow["CALENDARID"])))
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "勾选仅工作日时，行事历不能为空。");
            int repeatDateMark = 0;
            if (LibSysUtils.ToBoolean(masterRow["ISMONDAY"]))
                repeatDateMark += (int)Math.Pow(2, 0);
            if (LibSysUtils.ToBoolean(masterRow["ISTUESDAY"]))
                repeatDateMark += (int)Math.Pow(2, 1);
            if (LibSysUtils.ToBoolean(masterRow["ISWEDNESDAY"]))
                repeatDateMark += (int)Math.Pow(2, 2);
            if (LibSysUtils.ToBoolean(masterRow["ISTHURSDAY"]))
                repeatDateMark += (int)Math.Pow(2, 3);
            if (LibSysUtils.ToBoolean(masterRow["ISFRIDAY"]))
                repeatDateMark += (int)Math.Pow(2, 4);
            if (LibSysUtils.ToBoolean(masterRow["ISSATURDAY"]))
                repeatDateMark += (int)Math.Pow(2, 5);
            if (LibSysUtils.ToBoolean(masterRow["ISSUNDAY"]))
                repeatDateMark += (int)Math.Pow(2, 6);
            masterRow["REPEATDATEMARK"] = repeatDateMark;
            int execDate = LibSysUtils.ToInt32(masterRow["EXECDATE"]);
            if (repeatDateMark != 0 && execDate != 0)
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "指定执行日期，就不能勾选相关重复执行日期。");
            if (isJustWorkDay && execDate != 0)
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "指定执行日期，就不能勾选仅工作日执行。");
            if (repeatDateMark == 0 && execDate == 0)
                this.ManagerMessage.AddMessage(LibMessageKind.Error, "未指定执行日期。");
            HashSet<int> timeHashSet = new HashSet<int>();
            foreach (DataRow curRow in this.DataSet.Tables[1].Rows)
            {
                if (curRow.RowState == DataRowState.Deleted)
                    continue;
                int execTime = LibSysUtils.ToInt32(curRow["EXECTIME"]);
                if (timeHashSet.Contains(execTime))
                {
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "存在多个相同的执行时间点。");
                    break;
                }
                else timeHashSet.Add(execTime);
            }
            int intervalTime = LibSysUtils.ToInt32(masterRow["INTERVALTIME"]);
            if (intervalTime == 0)
            {
                if (timeHashSet.Count == 0)
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "需指定执行时间点。");
            }
            else
            {
                if (timeHashSet.Count > 0)
                    this.ManagerMessage.AddMessage(LibMessageKind.Error, "已指定了执行时间点，不能同时指定间隔时间。");
            }
        }

        protected override void AfterUpdate()
        {
            base.AfterUpdate();
            if (!this.ManagerMessage.IsThrow)
            {
                if (EnvProvider.Default.ScheduleTaskOpened)
                {
                    DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                    string taskId = LibSysUtils.ToString(masterRow["TASKID"]);
                    LibBusinessTask task = null;
                    PostAccountWay way = PostAccountHelper.GetPostAccountWay(PostAccountState.Release, masterRow);
                    switch (way)
                    {
                        case PostAccountWay.Diff:
                            string oldTaskId = LibSysUtils.ToString(masterRow["TASKID", DataRowVersion.Original]);
                            LibBusinessTaskCache.Default.RemoveCacheItem(oldTaskId);
                            LibScheduleTaskHost.Default.DeleteTask(oldTaskId);
                            task = AddTask();
                            LibScheduleTaskHost.Default.AddTask(task);
                            break;
                        case PostAccountWay.Positive:
                            task = AddTask();
                            LibScheduleTaskHost.Default.AddTask(task);
                            break;
                        case PostAccountWay.Reverse:
                            LibBusinessTaskCache.Default.RemoveCacheItem(LibSysUtils.ToString(masterRow["TASKID"]));
                            LibScheduleTaskHost.Default.DeleteTask(taskId);
                            break;
                    }
                }
            }
        }

        private LibBusinessTask AddTask()
        {
            LibBusinessTask task = new LibBusinessTask();
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            task.BusinessTaskId = LibSysUtils.ToString(masterRow["BUSINESSTASKID"]);
            task.CalendarId = LibSysUtils.ToString(masterRow["CALENDARID"]);
            task.ExecCondition = LibSysUtils.ToString(masterRow["ExecCondition"]);
            task.ExecDate = LibSysUtils.ToInt32(masterRow["EXECDATE"]);
            task.IntervalTime = LibSysUtils.ToInt32(masterRow["INTERVALTIME"]);
            task.IsJustWorkDay = LibSysUtils.ToBoolean(masterRow["ISJUSTWORKDAY"]);
            task.ProgId = LibSysUtils.ToString(masterRow["PROGID"]);
            task.Title = LibSysUtils.ToString(masterRow["TITLE"]);
            task.MainContent = LibSysUtils.ToString(masterRow["MAINCONTENT"]);
            task.RepeatDateMark = LibSysUtils.ToInt32(masterRow["REPEATDATEMARK"]);
            task.TaskId = LibSysUtils.ToString(masterRow["TASKID"]);
            foreach (DataRow curRow in this.DataSet.Tables[1].Rows)
            {
                if (curRow.RowState == DataRowState.Deleted)
                    continue;
                task.ExecTime.Add(LibSysUtils.ToInt32(curRow["EXECTIME"]));
            }
            string sql = string.Format("select distinct A.TASKID,A.PERSONID,D.DEPTID,C.USERID from AXPSCHEDULETASKPERSON A " +
            "inner join AXPSCHEDULETASK B on B.TASKID=A.TASKID left join COMPERSON D on D.PERSONID=A.PERSONID inner join AXPUSER C on C.PERSONID=A.PERSONID " +
            "where B.TASKID={0}", LibStringBuilder.GetQuotString(task.TaskId));
            using (IDataReader reader = this.DataAccess.ExecuteDataReader(sql, false))
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
            LibBusinessTaskCache.Default.AddCacheItem(task);
            return task;
        }

        protected override void AfterDelete()
        {
            base.AfterDelete();
            if (!this.ManagerMessage.IsThrow)
            {
                if (EnvProvider.Default.ScheduleTaskOpened)
                {
                    DataRow masterRow = this.DataSet.Tables[0].Rows[0];
                    string taskId = LibSysUtils.ToString(masterRow["TASKID"]);
                    LibBusinessTaskCache.Default.RemoveCacheItem(taskId);
                    LibScheduleTaskHost.Default.DeleteTask(taskId);
                }
            }
        }
    }

    public class AxpScheduleTaskBcfTemplate : LibTemplate
    {
        private const string tableName = "AXPSCHEDULETASK";
        private const string bodyTableName = "AXPSCHEDULETASKDETAIL";
        private const string subTableName = "AXPSCHEDULETASKPERSON";

        public AxpScheduleTaskBcfTemplate(string progId)
            : base(progId, BillType.Master, "排程任务设置")
        { }


        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();
            string primaryName = "TASKID";
            DataTable masterTable = new DataTable(tableName);
            DataSourceHelper.AddColumn(new DefineField(masterTable, primaryName, "任务代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "TASKNAME", "任务名称", FieldSize.Size50) { DataType = LibDataType.NText, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "PROGID", "功能代码", FieldSize.Size50)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                SelectSql = "Select A.PROGID as Id,A.PROGNAME as Name From AXPFUNCLIST A ",
                SelectFields = "PROGNAME",
                RelativeSource = new RelativeSourceCollection(){
                    new RelativeSource("axp.FuncList"){
                           RelFields = new RelFieldCollection(){
                           new RelField("PROGNAME", LibDataType.NText,FieldSize.Size50,"功能名称")
                      }  
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "BUSINESSTASKID", "业务任务", FieldSize.Size100)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("axp.BusinessTask")
                    {
                        RelPK = "A.PROGID",
                        RelFields = new RelFieldCollection()
                        {
                          new RelField("BUSINESSTASKNAME", LibDataType.NText,FieldSize.Size50,"业务任务名称"){ DataType = LibDataType.NText}
                        }
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "CALENDARID", "行事历", FieldSize.Size20)
            {
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection()
                {
                    new RelativeSource("com.Calendar")
                    {
                        RelFields = new RelFieldCollection()
                        {
                          new RelField("CALENDARNAME", LibDataType.NText,FieldSize.Size50,"行事历名称"){ DataType = LibDataType.NText}
                        }
                    }
                }
            });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISJUSTWORKDAY", "仅工作日") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "EXECDATE", "指定执行日期") { DataType = LibDataType.Int32, ControlType = LibControlType.Date });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "INTERVALTIME", "间隔执行分钟") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, QtyLimit = LibQtyLimit.GreaterOrEqualThanZero });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISMONDAY", "周一") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISTUESDAY", "周二") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISWEDNESDAY", "周三") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISTHURSDAY", "周四") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISFRIDAY", "周五") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISSATURDAY", "周六") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "ISSUNDAY", "周日") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, DefaultValue = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "REPEATDATEMARK", "重复日期标识") { DataType = LibDataType.Int32, ReadOnly = true, ControlType = LibControlType.Number });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "EXECCONDITION", "执行条件") { DataType = LibDataType.Binary, ControlType = LibControlType.Text, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "TITLE", "发送信息标题", FieldSize.Size200) { DataType = LibDataType.NText, ControlType = LibControlType.NText, ColumnSpan = 4 });
            DataSourceHelper.AddColumn(new DefineField(masterTable, "MAINCONTENT", "发送信息", FieldSize.Size500) { DataType = LibDataType.NText, ControlType = LibControlType.NText, ColumnSpan = 4, RowSpan = 2 });
            DataSourceHelper.AddFixColumn(masterTable, this.BillType);
            masterTable.PrimaryKey = new DataColumn[] { masterTable.Columns[primaryName] };
            this.DataSet.Tables.Add(masterTable);

            DataTable bodyTable = new DataTable(bodyTableName);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, primaryName, "任务代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(bodyTable);
            DataSourceHelper.AddRowNo(bodyTable);
            DataSourceHelper.AddColumn(new DefineField(bodyTable, "EXECTIME", "执行时间点") { DataType = LibDataType.Int32, ControlType = LibControlType.HourMinute });
            DataSourceHelper.AddRemark(bodyTable);
            bodyTable.PrimaryKey = new DataColumn[] { bodyTable.Columns[primaryName], bodyTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(bodyTable);
            this.DataSet.Relations.Add(new DataRelation(string.Format("{0}_{1}", tableName, bodyTableName), masterTable.Columns[primaryName], bodyTable.Columns[primaryName]));

            DataTable subTable = new DataTable(subTableName);
            DataSourceHelper.AddColumn(new DefineField(subTable, primaryName, "任务代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(subTable);
            DataSourceHelper.AddRowNo(subTable);
            DataSourceHelper.AddColumn(new DefineField(subTable, "PERSONID", "人员代码", FieldSize.Size20)
            {
                AllowEmpty = false,
                ControlType = LibControlType.IdName,
                RelativeSource = new RelativeSourceCollection() { 
                 new RelativeSource("com.Person"){  RelFields = new RelFieldCollection()
                     { new RelField("PERSONNAME", LibDataType.NText,FieldSize.Size50,"人员名称"),
                       new RelField("DEPTID", LibDataType.Text,FieldSize.Size20,"部门"){ ControlType = LibControlType.IdName},
                       new RelField("DEPTNAME",LibDataType.NText,FieldSize.Size50,"部门名称"){ControlType = LibControlType.NText}}}  
                }
            });
            DataSourceHelper.AddRemark(subTable);
            subTable.PrimaryKey = new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(subTable);
            this.DataSet.Relations.Add(new DataRelation(string.Format("{0}_{1}", tableName, subTable), masterTable.Columns[primaryName], subTable.Columns[primaryName]));
        }


        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "TASKID", "TASKNAME", "PROGID", "BUSINESSTASKID", "CALENDARID", "ISJUSTWORKDAY", "INTERVALTIME", "EXECDATE", "TITLE", "MAINCONTENT" });
            layout.TabRange.Add(layout.BuildControlGroup(0, "重复", new List<string>() { "ISMONDAY", "ISTUESDAY", "ISWEDNESDAY", "ISTHURSDAY", "ISFRIDAY", "ISSATURDAY", "ISSUNDAY" }));
            layout.TabRange.Add(layout.BuildGrid(1, "指定执行时间点", null));
            layout.TabRange.Add(layout.BuildGrid(2, "结果接收人员明细", null));
            layout.ButtonRange = layout.BuildButton(new List<FunButton>() { new FunButton("btnCondition", "设置执行条件") });
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
