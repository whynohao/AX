using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Comm.Utils
{
    public static class LibSysUtils
    {
        public static string ToString(object obj)
        {
            if (obj  is DBNull)
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
            if (obj  is DBNull)
                return false;
            else
                return Convert.ToBoolean(obj);
        }

        public static byte ToByte(object obj)
        {
            if (obj  is DBNull)
                return 0;
            else
                return Convert.ToByte(obj);
        }

        public static char ToChar(object obj)
        {
            if (obj  is DBNull)
                return char.MinValue;
            else
                return Convert.ToChar(obj);
        }

        public static decimal ToDecimal(object obj)
        {
            if (obj  is DBNull)
                return decimal.Zero;
            else
                return Convert.ToDecimal(obj);
        }

        public static double ToDouble(object obj)
        {
            if (obj  is DBNull)
                return 0;
            else
                return Convert.ToDouble(obj);
        }

        public static short ToInt16(object obj)
        {
            if (obj  is DBNull)
                return 0;
            else
                return Convert.ToInt16(obj);
        }

        public static sbyte ToSByte(object obj)
        {
            if (obj  is DBNull)
                return 0;
            else
                return Convert.ToSByte(obj);
        }

        public static Single ToSingle(object obj)
        {
            if (obj  is DBNull)
                return 0;
            else
                return Convert.ToSingle(obj);
        }

        public static UInt16 ToUInt16(object obj)
        {
            if (obj  is DBNull)
                return 0;
            else
                return Convert.ToUInt16(obj);
        }

        public static UInt32 ToUInt32(object obj)
        {
            if (obj  is DBNull)
                return 0;
            else
                return Convert.ToUInt32(obj);
        }

        public static UInt64 ToUInt64(object obj)
        {
            if (obj  is DBNull)
                return 0;
            else
                return Convert.ToUInt64(obj);
        }
    }
}
