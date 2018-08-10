using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Utils
{
    public static class LibDateUtils
    {
        #region 字符串转 DateTime
        public static DateTime StrToDateTime(string strDate)
        {
            return DateTime.Parse(strDate);
        }
        #endregion

        #region 取前一个的日期和后一个的时间，将两个日期字符串合并
        public static DateTime ToDateTime(object strDate, object strTime)
        {
            DateTime date = DateTime.Parse(LibSysUtils.ToString(strDate));
            DateTime time = DateTime.Parse(LibSysUtils.ToString(strTime));
            DateTime datetime = DateTime.Parse(string.Format("{0} {1}", date.ToString("yyyy-MM-dd"), time.ToString("HH:mm:ss")));
            return datetime;
        }

        public static DateTime ToDateTime(string strDate, string strTime)
        {
            DateTime date = DateTime.Parse(strDate);
            DateTime time = DateTime.Parse(strTime);
            DateTime datetime = DateTime.Parse(string.Format("{0} {1}", date.ToString("yyyy-MM-dd"), time.ToString("HH:mm:ss")));
            return datetime;
        }
        #endregion


        public static DateTime Now()
        {
            return DateTime.Now;
        }

        public static long GetCurrentDateTime()
        {
            return DateTimeToLibDateTime(DateTime.Now);
        }

        public static int GetCurrentDate()
        {
            return DateTimeToLibDate(DateTime.Now);
        }

        public static string GetSpecialDate()
        {
            StringBuilder builder = new StringBuilder(6);
            DateTime dateTime = DateTime.Now;
            if (dateTime.Day <= 9)
                builder.AppendFormat("0{0}", dateTime.Day);
            else
                builder.Append(dateTime.Day);
            if (dateTime.Month <= 9)
                builder.AppendFormat("0{0}", dateTime.Month);
            else
                builder.Append(dateTime.Month);
            builder.Append(dateTime.Year.ToString().Remove(0, 2));
            return builder.ToString();
        }

        public static string GetDateForABYear()
        {
            StringBuilder builder = new StringBuilder(6);
            DateTime dateTime = DateTime.Now;
            string tempYear = dateTime.Year.ToString().Remove(0, 2);
            builder.Append((char)(int.Parse(tempYear[0].ToString()) - 1 + (int)'A'));
            builder.Append((char)(int.Parse(tempYear[1].ToString()) - 1 + (int)'A'));
            if (dateTime.Month <= 9)
                builder.AppendFormat("0{0}", dateTime.Month);
            else
                builder.Append(dateTime.Month);
            if (dateTime.Day <= 9)
                builder.AppendFormat("0{0}", dateTime.Day);
            else
                builder.Append(dateTime.Day);
            return builder.ToString();
        }

        public static long DateTimeToLibDateTime(DateTime dateTime)
        {
            long libDate = dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day;
            long libDateTime = libDate * 1000000L + (long)dateTime.Hour * 10000L + (long)dateTime.Minute * 100L + (long)dateTime.Second;
            return libDateTime;
        }
        public static int DateTimeToLibDate(DateTime dateTime)
        {
            int libDate = dateTime.Year * 10000 + dateTime.Month * 100 + dateTime.Day;
            return libDate;
        }

        public static DateTime LibDateToDateTime(long libDate)
        {
            string temp = libDate.ToString();
            DateTime dateTime = new DateTime(int.Parse(temp.Substring(0, 4)), int.Parse(temp.Substring(4, 2)), int.Parse(temp.Substring(6, 2)), int.Parse(temp.Substring(8, 2)), int.Parse(temp.Substring(10, 2)), int.Parse(temp.Substring(12, 2)));
            return dateTime;
        }
        public static DateTime LibDateToDateTime(int libDate)
        {
            string temp = libDate.ToString();
            DateTime dateTime = new DateTime(int.Parse(temp.Substring(0, 4)), int.Parse(temp.Substring(4, 2)), int.Parse(temp.Substring(6, 2)));
            return dateTime;
        }

        public static long AddDayToLibDateTime(DateTime dateTime, double value)
        {
            return (long)DateTimeToLibDate(dateTime.AddDays(value)) * 1000000L;
        }
        public static int AddDayToLibDate(DateTime dateTime, double value)
        {
            return (int)DateTimeToLibDate(dateTime.AddDays(value));
        }

        public static int AddDayToLibDate(int date, double value)
        {
            DateTime curDate = LibDateUtils.LibDateToDateTime(date);
            return (int)DateTimeToLibDate(curDate.AddDays(value));
        }


        public static int GetLibTimePart(long libTime, LibDateTimePartEnum timePart)
        {
            int value = 0;
            switch (timePart)
            {
                case LibDateTimePartEnum.Year:
                    value = int.Parse(libTime.ToString().Substring(0, 4));
                    break;
                case LibDateTimePartEnum.Month:
                    value = int.Parse(libTime.ToString().Substring(4, 2));
                    break;
                case LibDateTimePartEnum.Day:
                    value = int.Parse(libTime.ToString().Substring(6, 2));
                    break;
                case LibDateTimePartEnum.Hour:
                    value = int.Parse(libTime.ToString().Substring(8, 2));
                    break;
                case LibDateTimePartEnum.Minute:
                    value = int.Parse(libTime.ToString().Substring(10, 2));
                    break;
                case LibDateTimePartEnum.Second:
                    value = int.Parse(libTime.ToString().Substring(12, 2));
                    break;
                case LibDateTimePartEnum.Date:
                    value = int.Parse(libTime.ToString().Substring(0, 8));
                    break;
                case LibDateTimePartEnum.Time:
                    value = int.Parse(libTime.ToString().Substring(8, 6));
                    break;
            }
            return value;
        }

        public static int GetLibDatePart(int libDate, LibDateTimePartEnum timePart)
        {
            int value = 0;
            switch (timePart)
            {
                case LibDateTimePartEnum.Year:
                    value = int.Parse(libDate.ToString().Substring(0, 4));
                    break;
                case LibDateTimePartEnum.Month:
                    value = int.Parse(libDate.ToString().Substring(4, 2));
                    break;
                case LibDateTimePartEnum.Day:
                    value = int.Parse(libDate.ToString().Substring(6, 2));
                    break;
            }
            return value;
        }
    }

    public enum LibDateTimePartEnum
    {
        Year = 0,
        Month = 1,
        Day = 3,
        Hour = 4,
        Minute = 5,
        Second = 6,
        Date = 7,
        Time = 8
    }

}
