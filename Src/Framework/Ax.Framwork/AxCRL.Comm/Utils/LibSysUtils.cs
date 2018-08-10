using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AxCRL.Comm.Utils
{
    public static class LibSysUtils
    {
        #region 文件路径组合
        public static string Combine(params string[] paths)
        {
            if (paths.Length == 0)
            {
                throw new ArgumentException("please input path");
            }
            else
            {
                StringBuilder builder = new StringBuilder();
                string spliter = "\\";
                string firstPath = paths[0];
                if (firstPath.StartsWith("HTTP", StringComparison.OrdinalIgnoreCase))
                {
                    spliter = "/";
                }
                if (!firstPath.EndsWith(spliter))
                {
                    firstPath = firstPath + spliter;
                }
                builder.Append(firstPath);
                for (int i = 1; i < paths.Length; i++)
                {
                    string nextPath = paths[i];
                    if (nextPath.StartsWith("/") || nextPath.StartsWith("\\"))
                    {
                        nextPath = nextPath.Substring(1);
                    }
                    if (i != paths.Length - 1)//not the last one
                    {
                        if (nextPath.EndsWith("/") || nextPath.EndsWith("\\"))
                        {
                            nextPath = nextPath.Substring(0, nextPath.Length - 1) + spliter;
                        }
                        else
                        {
                            nextPath = nextPath + spliter;
                        }
                    }
                    builder.Append(nextPath);
                }
                return builder.ToString();
            }
        }
        #endregion

        public static string ToString(object obj)
        {
            if (obj is DBNull)
                return string.Empty;
            else
                return Convert.ToString(obj);
        }

        public static int ToInt32(object obj)
        {
            if (obj is DBNull)
                return 0;
            else
                return Convert.ToInt32(obj);
        }

        public static long ToInt64(object obj)
        {
            if (obj is DBNull)
                return 0L;
            else
                return Convert.ToInt64(obj);
        }

        public static bool ToBoolean(object obj)
        {
            if (obj is DBNull)
                return false;
            else
                return Convert.ToBoolean(obj);
        }

        public static byte ToByte(object obj)
        {
            if (obj is DBNull)
                return 0;
            else
                return Convert.ToByte(obj);
        }

        public static char ToChar(object obj)
        {
            if (obj is DBNull)
                return char.MinValue;
            else
                return Convert.ToChar(obj);
        }

        public static decimal ToDecimal(object obj)
        {
            if (obj is DBNull)
                return decimal.Zero;
            else
                return Convert.ToDecimal(obj);
        }

        public static double ToDouble(object obj)
        {
            if (obj is DBNull)
                return 0;
            else
                return Convert.ToDouble(obj);
        }

        public static short ToInt16(object obj)
        {
            if (obj is DBNull)
                return 0;
            else
                return Convert.ToInt16(obj);
        }

        public static sbyte ToSByte(object obj)
        {
            if (obj is DBNull)
                return 0;
            else
                return Convert.ToSByte(obj);
        }

        public static Single ToSingle(object obj)
        {
            if (obj is DBNull)
                return 0;
            else
                return Convert.ToSingle(obj);
        }

        public static UInt16 ToUInt16(object obj)
        {
            if (obj is DBNull)
                return 0;
            else
                return Convert.ToUInt16(obj);
        }

        public static UInt32 ToUInt32(object obj)
        {
            if (obj is DBNull)
                return 0;
            else
                return Convert.ToUInt32(obj);
        }

        public static UInt64 ToUInt64(object obj)
        {
            if (obj is DBNull)
                return 0;
            else
                return Convert.ToUInt64(obj);
        }

        public static string HtmlDecode(string value)
        {
            return HttpUtility.HtmlDecode(value);
        }

        public static string UrlDecode(object value)
        {
            return HttpUtility.UrlDecode(ToString(value));
        }

    }
}
