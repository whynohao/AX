using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxCRL.Bcf;
using AxCRL.Template;
using AxCRL.Comm;
using System.Data;
using AxCRL.Template.DataSource;
using AxCRL.Template.Layout;
using AxCRL.Template.ViewTemplate;
using AxCRL.Comm.Define;
using AxCRL.Comm.Utils;

namespace MES_Com.MasterDataBcf
{
    [ProgId(ProgId = "com.Calendar", ProgIdType = ProgIdType.Bcf)]
    public class ComCalendarBcf : LibBcfData
    {
        protected override LibTemplate RegisterTemplate()
        {
            return new ComCalendarBcfTemplate("com.Calendar");
        }

        protected override void BeforeUpdate()
        {
            DataRow masterRow = this.DataSet.Tables[0].Rows[0];
            bool isWorkSaturday = LibSysUtils.ToBoolean(masterRow["ISWORKSATURDAY"]);
            bool isWorkSunday = LibSysUtils.ToBoolean(masterRow["ISWORKSUNDAY"]);
            foreach (DataRow curRow in this.DataSet.Tables[1].Rows)
            {
                if (curRow.RowState == DataRowState.Deleted)
                    continue;
                int workMark = 0;
                int workDayCount = 0;
                int year = LibSysUtils.ToInt32(curRow["YEAR"]);
                int month = LibSysUtils.ToInt32(curRow["MONTH"]);
                int days = DateTime.DaysInMonth(year, month);
                Dictionary<int, bool> specialDic = new Dictionary<int, bool>();
                DataRow[] childRows = curRow.GetChildRows("COMCALENDARDETAIL_COMHOLIDAYDETAIL");
                foreach (DataRow subRow in childRows)
                {
                    int dayNum = LibDateUtils.LibDateToDateTime(LibSysUtils.ToInt32(subRow["HOLIDAYDATE"])).Day;
                    specialDic.Add(dayNum, LibSysUtils.ToBoolean(subRow["ISWORK"]));
                }
                for (int i = 0; i < days; i++)
                {
                    int date = i + 1;
                    if (specialDic.ContainsKey(date))
                    {
                        if (specialDic[date])
                        {
                            workMark += (int)Math.Pow(2, i);
                            workDayCount++;
                        }
                        continue;
                    }
                    DayOfWeek week = new DateTime(year, month, i + 1).DayOfWeek;
                    if (week != DayOfWeek.Saturday && week != DayOfWeek.Sunday)
                    {
                        workMark += (int)Math.Pow(2, i);
                        workDayCount++;
                    }
                    else
                    {
                        if ((week == DayOfWeek.Saturday && isWorkSaturday == true) || (week == DayOfWeek.Sunday && isWorkSunday == true))
                        {
                            workMark += (int)Math.Pow(2, i);
                            workDayCount++;
                        }
                    }
                }
                curRow.BeginEdit();
                try
                {
                    curRow["WEEKDAY"] = workDayCount;
                    curRow["NONWORKDAY"] = days - workDayCount;
                    curRow["WORKMARK"] = workMark;
                }
                finally
                {
                    curRow.EndEdit();
                }
            }
        }
    }

    public class ComCalendarBcfTemplate : LibTemplate
    {
        private const string mainTableName = "COMCALENDAR";
        private const string subTableName = "COMCALENDARDETAIL";
        private const string holidayTableName = "COMHOLIDAYDETAIL";

        public ComCalendarBcfTemplate(string progId) : base(progId, BillType.Master, "行事历") { }

        protected override void BuildDataSet()
        {
            this.DataSet = new DataSet();

            string primaryName = "CALENDARID";
            DataTable mainTable = new DataTable(mainTableName);
            DataSourceHelper.AddColumn(new DefineField(mainTable, primaryName, "行事历代码", FieldSize.Size20) { AllowCopy = false, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(mainTable, "CALENDARNAME", "行事历名称", FieldSize.Size50) { AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(mainTable, "ISWORKSATURDAY", "周六上班") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddColumn(new DefineField(mainTable, "ISWORKSUNDAY", "周日上班") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddFixColumn(mainTable, this.BillType);
            mainTable.PrimaryKey = new DataColumn[] { mainTable.Columns[primaryName] };
            this.DataSet.Tables.Add(mainTable);

            DataTable subTable = new DataTable(subTableName);
            DataSourceHelper.AddColumn(new DefineField(subTable, primaryName, "行事历代码", FieldSize.Size20) { AllowCopy = false, AllowEmpty = false });
            DataSourceHelper.AddRowId(subTable);
            DataSourceHelper.AddRowNo(subTable);
            DataSourceHelper.AddColumn(new DefineField(subTable, "YEAR", "年份") { DataType = LibDataType.Int32, AllowEmpty = false, ControlType = LibControlType.Number });
            DataSourceHelper.AddColumn(new DefineField(subTable, "MONTH", "月份") { DataType = LibDataType.Int32, ControlType = LibControlType.Number });
            DataSourceHelper.AddColumn(new DefineField(subTable, "WEEKDAY", "工作日") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(subTable, "NONWORKDAY", "非工作日") { DataType = LibDataType.Int32, ControlType = LibControlType.Number, ReadOnly = true });
            DataSourceHelper.AddColumn(new DefineField(subTable, "HOLIDAYDETAIL", "节假日明细") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo, ReadOnly = true, SubTableIndex = 2 });
            DataSourceHelper.AddColumn(new DefineField(subTable, "WORKMARK", "工作日标识") { DataType = LibDataType.Int32, ReadOnly = true });
            subTable.PrimaryKey = new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["ROW_ID"] };
            subTable.ExtendedProperties.Add(TableProperty.AllowEmpt, false);
            this.DataSet.Tables.Add(subTable);
            this.DataSet.Relations.Add(new DataRelation(string.Format("{0}_{1}", mainTableName, subTableName), mainTable.Columns[primaryName], subTable.Columns[primaryName]));

            DataTable holidayDetailTable = new DataTable(holidayTableName);
            DataSourceHelper.AddColumn(new DefineField(holidayDetailTable, primaryName, "行事历代码", FieldSize.Size20) { AllowEmpty = false, AllowCopy = false });
            DataSourceHelper.AddRowId(holidayDetailTable, "PARENTROWID", "父行标识");
            DataSourceHelper.AddRowId(holidayDetailTable);
            DataSourceHelper.AddRowNo(holidayDetailTable);
            DataSourceHelper.AddColumn(new DefineField(holidayDetailTable, "HOLIDAYDATE", "日期") { DataType = LibDataType.Int32, ControlType = LibControlType.Date, AllowEmpty = false });
            DataSourceHelper.AddColumn(new DefineField(holidayDetailTable, "ISWORK", "是否上班") { DataType = LibDataType.Boolean, ControlType = LibControlType.YesNo });
            DataSourceHelper.AddRemark(holidayDetailTable);
            holidayDetailTable.PrimaryKey = new DataColumn[] { holidayDetailTable.Columns[primaryName], holidayDetailTable.Columns["PARENTROWID"], holidayDetailTable.Columns["ROW_ID"] };
            this.DataSet.Tables.Add(holidayDetailTable);
            this.DataSet.Relations.Add(string.Format("{0}_{1}", subTableName, holidayTableName), new DataColumn[] { subTable.Columns[primaryName], subTable.Columns["ROW_ID"] }, new DataColumn[] { holidayDetailTable.Columns[primaryName], holidayDetailTable.Columns["PARENTROWID"] });
        }

        protected override void DefineViewTemplate(DataSet dataSet)
        {
            LibBillLayout layout = new LibBillLayout(this.DataSet);
            layout.HeaderRange = layout.BuildControlGroup(0, string.Empty, new List<string>() { "CALENDARID", "CALENDARNAME", "ISWORKSATURDAY", "ISWORKSUNDAY" });
            layout.GridRange = layout.BuildGrid(1, "工作明细", new List<string> { "ROW_ID", "ROWNO", "CALENDARID", "YEAR", "MONTH", "WEEKDAY", "NONWORKDAY", "HOLIDAYDETAIL", "WORKMARK" });
            layout.SubBill.Add(2, layout.BuildGrid(2, "节假日明细"));
            this.ViewTemplate = new LibBillTpl(this.DataSet, layout);
        }
    }
}
