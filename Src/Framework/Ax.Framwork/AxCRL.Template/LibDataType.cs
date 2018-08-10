using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Template
{
    /// <summary>
    /// 平台数据类型枚举定义
    /// </summary>
    public enum LibDataType
    {
        Text = 0,
        NText = 1,
        Int32 = 2,
        Int64 = 3,
        Numeric = 4,
        Float = 5,
        Double = 6,
        Byte = 7,
        Boolean = 8,
        Binary = 9,
        DateTime = 10,
        Date = 11,
        Time = 12,
    }
    /// <summary>
    /// 平台控件类型
    /// </summary>
    public enum LibControlType
    {
        Id = 0,
        IdName = 1,
        Text = 2,
        NText = 3,
        Quantity = 4,
        Number = 5,
        TextOption = 6,
        Double = 7,
        YesNo = 8,
        Date = 10,
        DateTime = 11,
        HourMinute = 12,
        Time = 13,
        FieldControl = 14,
        Rate = 15,
        AttributeCodeField = 16,
        AttributeDescField = 17,
        KeyValueOption = 18,
        Price = 19,
        Amount = 20,
        TaxRate = 21,
        /// <summary>
        /// 以树形结构展示IdName，要求RelativeSource具有父子结构
        /// </summary>
        IdNameTree = 22,
        FieldOption = 23,
        HtmlEditor = 24,
        Image = 25,
    }

    public static class LibDataTypeConverter
    {
        public static Type ConvertType(LibDataType libDataType)
        {
            Type t = null;

            switch (libDataType)
            {
                case LibDataType.Text:
                case LibDataType.NText:
                    t = typeof(string);
                    break;

                case LibDataType.Int32:
                    t = typeof(int);
                    break;

                case LibDataType.Int64:
                    t = typeof(long);
                    break;

                case LibDataType.Numeric:
                    t = typeof(decimal);
                    break;

                case LibDataType.Float:
                    t = typeof(float);
                    break;

                case LibDataType.Double:
                    t = typeof(double);
                    break;

                case LibDataType.Byte:
                    t = typeof(byte);
                    break;

                case LibDataType.Boolean:
                    t = typeof(bool);
                    break;

                case LibDataType.Binary:
                    t = typeof(string);
                    break;
                case LibDataType.DateTime:
                    t = typeof(string);
                    break;
                case LibDataType.Date:
                    t = typeof(string);
                    break;
                case LibDataType.Time:
                    t = typeof(string);
                    break;
                default:
                    break;
            }

            return t;
        }
        /// <summary>
        /// 将Type类型转换为LibType类型
        /// 无法区分NText、Text、Binary，Type为String都转换为LibDataType.NText
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static LibDataType ConvertToLibType(Type type)
        {
            LibDataType libType = LibDataType.Text;

            if (type == typeof(string))
            { libType = LibDataType.NText; }//无法区分NText、Text、Binary

            else
                if (type == typeof(int))
                { libType = LibDataType.Int32; }

                else
                    if (type == typeof(long))
                    { libType = LibDataType.Int64; }

                    else
                        if (type == typeof(decimal))
                        { libType = LibDataType.Numeric; }

                        else
                            if (type == typeof(float))
                            { libType = LibDataType.Float; }

                            else
                                if (type == typeof(double))
                                { libType = LibDataType.Double; }

                                else
                                    if (type == typeof(byte))
                                    { libType = LibDataType.Byte; }

                                    else
                                        if (type == typeof(bool))
                                        { libType = LibDataType.Boolean; }

            return libType;
        }
    }
}
