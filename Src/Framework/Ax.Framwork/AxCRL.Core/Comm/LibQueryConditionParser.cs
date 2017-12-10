using AxCRL.Comm.Utils;
using AxCRL.Template;
using AxCRL.Template.DataSource;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Comm
{
    public static class LibQueryConditionParser
    {
        public static List<LibQueryField> GetQueryField(LibQueryCondition condition, string name)
        {
            List<LibQueryField> queryFieldList = new List<LibQueryField>();
            if (condition == null)
                return queryFieldList;
            foreach (var item in condition.QueryFields)
            {
                if (item.Name.CompareTo(name) == 0)
                {
                    queryFieldList.Add(item);
                }
            }
            return queryFieldList;
        }

        public static string GetQueryStr(LibQueryCondition condition, string name, LibDataType dataType, bool needAnd = false, string prefix = "A", string realName = "")
        {
            StringBuilder builder = new StringBuilder();
            List<LibQueryField> queryFieldList = new List<LibQueryField>();
            if (condition == null)
                return builder.ToString();
            foreach (var item in condition.QueryFields)
            {
                if (item.Name.CompareTo(name) == 0)
                {
                    queryFieldList.Add(item);
                }
            }
            int count = queryFieldList.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    BuildQueryStr(dataType, queryFieldList[i], builder, prefix, needAnd || !(i == count - 1), realName);
                }
            }
            if (condition.PowerQueryFieldDic.ContainsKey(name))
            {
                if(!needAnd)
                builder.Append(" and ");
                BuildPowserQueryStr(dataType, condition.PowerQueryFieldDic[name], builder, prefix, false, realName);
            }
            return builder.ToString();
        }

        public static string GetQueryFieldStr(LibDataType dataType, LibQueryField queryField, bool needAnd = false, string prefix = "A", string realName = "")
        {
            StringBuilder builder = new StringBuilder();
            BuildQueryStr(dataType, queryField, builder, prefix, needAnd, realName);
            return builder.ToString();
        }

        private static void BuildQueryStr(LibDataType dataType, LibQueryField queryField, StringBuilder builder, string prefix, bool needAnd, string realName)
        {
            bool needQuot = dataType == LibDataType.Text || dataType == LibDataType.NText;
            string addStr = needAnd ? "and " : string.Empty;
            string fieldName = string.IsNullOrEmpty(realName) ? queryField.Name : realName;
            if (!string.IsNullOrEmpty(prefix))
                prefix = string.Format("{0}.", prefix);
            switch (queryField.QueryChar)
            {
                case LibQueryChar.Equal:
                    if (queryField.Value.Count > 0)
                    {
                        if (needQuot)
                            builder.AppendFormat("{0}{1}={2} {3}", prefix, fieldName, LibStringBuilder.GetQuotObject(queryField.Value[0]), addStr);
                        else
                            builder.AppendFormat("{0}{1}={2} {3}", prefix, fieldName, queryField.Value[0], addStr);
                    }
                    break;
                case LibQueryChar.Contain:
                    if (queryField.Value.Count > 0)
                    {
                        builder.AppendFormat("{0}{1} like '%{2}%' {3}", prefix, fieldName, queryField.Value[0], addStr);
                    }
                    break;
                case LibQueryChar.Region:
                    if (queryField.Value.Count == 2)
                    {
                        if (needQuot)
                            builder.AppendFormat("{0}{1} between {2} and {3} {4}", prefix, fieldName, LibStringBuilder.GetQuotObject(queryField.Value[0]), LibStringBuilder.GetQuotObject(queryField.Value[1]), addStr);
                        else
                            builder.AppendFormat("{0}{1} between {2} and {3} {4}", prefix, fieldName, queryField.Value[0], queryField.Value[1], addStr);
                    }
                    break;
                case LibQueryChar.GreaterOrEqual:
                    if (queryField.Value.Count > 0)
                    {
                        if (needQuot)
                            builder.AppendFormat("{0}{1}>={2} {3}", prefix, fieldName, LibStringBuilder.GetQuotObject(queryField.Value[0]), addStr);
                        else
                            builder.AppendFormat("{0}{1}>={2} {3}", prefix, fieldName, queryField.Value[0], addStr);
                    }
                    break;
                case LibQueryChar.LessOrEqual:
                    if (queryField.Value.Count > 0)
                    {
                        if (needQuot)
                            builder.AppendFormat("{0}{1}<={2} {3}", prefix, fieldName, LibStringBuilder.GetQuotObject(queryField.Value[0]), addStr);
                        else
                            builder.AppendFormat("{0}{1}<={2} {3}", prefix, fieldName, queryField.Value[0], addStr);
                    }
                    break;
                case LibQueryChar.GreaterThan:
                    if (queryField.Value.Count > 0)
                    {
                        if (needQuot)
                            builder.AppendFormat("{0}{1}>{2} {3}", prefix, fieldName, LibStringBuilder.GetQuotObject(queryField.Value[0]), addStr);
                        else
                            builder.AppendFormat("{0}{1}>{2} {3}", prefix, fieldName, queryField.Value[0], addStr);
                    }
                    break;
                case LibQueryChar.LessThan:
                    if (queryField.Value.Count > 0)
                    {
                        if (needQuot)
                            builder.AppendFormat("{0}{1}<{2} {3}", prefix, fieldName, LibStringBuilder.GetQuotObject(queryField.Value[0]), addStr);
                        else
                            builder.AppendFormat("{0}{1}<{2} {3}", prefix, fieldName, queryField.Value[0], addStr);
                    }
                    break;
                case LibQueryChar.UnequalTo:
                    if (queryField.Value.Count > 0)
                    {
                        if (needQuot)
                            builder.AppendFormat("{0}{1}<>{2} {3}", prefix, fieldName, LibStringBuilder.GetQuotObject(queryField.Value[0]), addStr);
                        else
                            builder.AppendFormat("{0}{1}<>{2} {3}", prefix, fieldName, queryField.Value[0], addStr);
                    }
                    break;
                case LibQueryChar.Include:
                    if (queryField.Value.Count > 0)
                    {
                        if (needQuot)
                        {
                            StringBuilder tempBuilder = new StringBuilder();
                            string[] dest = queryField.Value[0].ToString().Split(',');
                            for (int i = 0; i < dest.Length; i++)
                            {
                                if (i == 0)
                                    tempBuilder.AppendFormat("{0}", LibStringBuilder.GetQuotString(dest[i]));
                                else
                                    tempBuilder.AppendFormat(",{0}", LibStringBuilder.GetQuotString(dest[i]));
                            }
                            builder.AppendFormat("{0}{1} in ({2}) {3}", prefix, fieldName, tempBuilder.ToString(), addStr);
                        }
                        else
                            builder.AppendFormat("{0}{1} in ({2}) {3}", prefix, fieldName, queryField.Value[0], addStr);
                    }
                    break;
            }
        }

        private static void BuildPowserQueryStr(LibDataType dataType, List<LibQueryField> queryFieldList, StringBuilder builder, string prefix, bool needAnd, string realName)
        {
            StringBuilder tempBuilder = new StringBuilder();
            for (int i = 0; i < queryFieldList.Count; i++)
            {
                tempBuilder.Append("(");
                if (i != 0)
                    tempBuilder.Append(" or ");
                BuildQueryStr(dataType, queryFieldList[i], tempBuilder, prefix, false, realName);
                tempBuilder.Append(")");
            }
            builder.Append(tempBuilder.ToString());
            if (needAnd)
                builder.Append("and ");
        }

        public static string GetQueryData(string progId, LibQueryCondition condition, string prefix = "A", bool useRelativeField = true)
        {
            if (condition == null)
                return string.Empty;
            StringBuilder builder = new StringBuilder();
            LibSqlModel sqlModel = LibSqlModelCache.Default.GetSqlModel(progId);
            if (sqlModel == null)
                throw new ArgumentNullException("sqlModel", "GetQueryData方法解析的progId取不到sqlModel");
            LibSqlModelTable table = (LibSqlModelTable)sqlModel.Tables[0];

           

            foreach (LibQueryField queryField in condition.QueryFields)
            {
                if (!table.Columns.Contains(queryField.Name))
                    continue;
                LibSqlModelColumn col = (LibSqlModelColumn)table.Columns[queryField.Name];
                if (col.ExtendedProperties.ContainsKey(FieldProperty.FieldType))
                {
                    FieldType fieldType = (FieldType)col.ExtendedProperties[FieldProperty.FieldType];
                    if (FieldType.Virtual == fieldType)
                        continue;
                    if (!useRelativeField && FieldType.Relative == fieldType)
                        continue;
                }
                LibDataType dataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                BuildQueryStr(dataType, queryField, builder, prefix, true, string.Empty);
                //加入权限
                if (condition.PowerQueryFieldDic.ContainsKey(queryField.Name))
                {
                    BuildPowserQueryStr(dataType, condition.PowerQueryFieldDic[queryField.Name], builder, prefix, true, string.Empty);
                }
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 4, 4);
            }
            return builder.ToString();
        }

        public static LibQueryCondition MergeQueryCondition(DataTable table, LibQueryCondition condition, Dictionary<string, List<LibQueryField>> powerQueryFieldDic)
        {
            if (powerQueryFieldDic == null || powerQueryFieldDic.Count == 0)
                return condition;
            if (condition == null || condition.QueryFields.Count == 0)
            {
                condition = new LibQueryCondition();
                foreach (var item in powerQueryFieldDic)
                {
                    foreach (var subItem in item.Value)
                    {
                        condition.QueryFields.Add(subItem);
                    }
                }
                return condition;
            }
            List<LibQueryField> addList = new List<LibQueryField>();
            List<string> removeList = new List<string>();
            //将权限（仅存在一个权限设定，即非or的情况）合并到当前用户的选择条件中
            foreach (var powerQuery in powerQueryFieldDic)
            {
                if (powerQuery.Value.Count == 1)
                {
                    bool exist = false;
                    LibQueryField other = powerQuery.Value[0];
                    foreach (var item in condition.QueryFields)
                    {
                        if (item.Name == powerQuery.Key)
                        {
                            exist = true;
                            DataColumn col = table.Columns[item.Name];
                            LibDataType dataType = (LibDataType)col.ExtendedProperties[FieldProperty.DataType];
                            switch (dataType)
                            {
                                case LibDataType.Text:
                                case LibDataType.NText:
                                case LibDataType.Binary:
                                    string curStr1 = item.Value[0].ToString();
                                    string otherStr1 = other.Value[0].ToString();
                                    string curStr2 = string.Empty;
                                    string otherStr2 = string.Empty;
                                    if (item.Value.Count == 2)
                                        curStr2 = item.Value[1].ToString();
                                    if (other.Value.Count == 2)
                                        otherStr2 = other.Value[1].ToString();
                                    MergeFieldQuery(addList, item, other, curStr1, otherStr1, curStr2, otherStr2);
                                    break;
                                case LibDataType.Int32:
                                    int curInt1 = LibSysUtils.ToInt32(item.Value[0]);
                                    int otherInt1 = LibSysUtils.ToInt32(other.Value[0]);
                                    int curInt2 = 0;
                                    int otherInt2 = 0;
                                    if (item.Value.Count == 2)
                                        curInt2 = LibSysUtils.ToInt32(item.Value[1]);
                                    if (other.Value.Count == 2)
                                        otherInt2 = LibSysUtils.ToInt32(other.Value[1]);
                                    MergeFieldQuery(addList, item, other, curInt1, otherInt1, curInt2, otherInt2);
                                    break;
                                case LibDataType.Int64:
                                    long curLong1 = LibSysUtils.ToInt64(item.Value[0]);
                                    long otherLong1 = LibSysUtils.ToInt64(other.Value[0]);
                                    long curLong2 = 0;
                                    long otherLong2 = 0;
                                    if (item.Value.Count == 2)
                                        curLong2 = LibSysUtils.ToInt64(item.Value[1]);
                                    if (other.Value.Count == 2)
                                        otherLong2 = LibSysUtils.ToInt64(other.Value[1]);
                                    MergeFieldQuery(addList, item, other, curLong1, otherLong1, curLong2, otherLong2);
                                    break;
                                case LibDataType.Numeric:
                                    decimal curDecimal1 = LibSysUtils.ToDecimal(item.Value[0]);
                                    decimal otherDecimal1 = LibSysUtils.ToDecimal(other.Value[0]);
                                    decimal curDecimal2 = 0;
                                    decimal otherDecimal2 = 0;
                                    if (item.Value.Count == 2)
                                        curDecimal2 = LibSysUtils.ToDecimal(item.Value[1]);
                                    if (other.Value.Count == 2)
                                        otherDecimal2 = LibSysUtils.ToDecimal(other.Value[1]);
                                    MergeFieldQuery(addList, item, other, curDecimal1, otherDecimal1, curDecimal2, otherDecimal2);
                                    break;
                                case LibDataType.Float:
                                    float curFloat1 = LibSysUtils.ToSingle(item.Value[0]);
                                    float otherFloat1 = LibSysUtils.ToSingle(other.Value[0]);
                                    float curFloat2 = 0;
                                    float otherFloat2 = 0;
                                    if (item.Value.Count == 2)
                                        curFloat2 = LibSysUtils.ToSingle(item.Value[1]);
                                    if (other.Value.Count == 2)
                                        otherFloat2 = LibSysUtils.ToSingle(other.Value[1]);
                                    MergeFieldQuery(addList, item, other, curFloat1, otherFloat1, curFloat2, otherFloat2);
                                    break;
                                case LibDataType.Double:
                                    double curDouble1 = LibSysUtils.ToDouble(item.Value[0]);
                                    double otherDouble1 = LibSysUtils.ToDouble(other.Value[0]);
                                    double curDouble2 = 0;
                                    double otherDouble2 = 0;
                                    if (item.Value.Count == 2)
                                        curDouble2 = LibSysUtils.ToDouble(item.Value[1]);
                                    if (other.Value.Count == 2)
                                        otherDouble2 = LibSysUtils.ToDouble(other.Value[1]);
                                    MergeFieldQuery(addList, item, other, curDouble1, otherDouble1, curDouble2, otherDouble2);
                                    break;
                                case LibDataType.Byte:
                                    byte curByte1 = LibSysUtils.ToByte(item.Value[0]);
                                    byte otherByte1 = LibSysUtils.ToByte(other.Value[0]);
                                    byte curByte2 = 0;
                                    byte otherByte2 = 0;
                                    if (item.Value.Count == 2)
                                        curByte2 = LibSysUtils.ToByte(item.Value[1]);
                                    if (other.Value.Count == 2)
                                        otherByte2 = LibSysUtils.ToByte(other.Value[1]);
                                    MergeFieldQuery(addList, item, other, curByte1, otherByte1, curByte2, otherByte2);
                                    break;
                                case LibDataType.Boolean:
                                    item.QueryChar = other.QueryChar;
                                    item.Value = other.Value;
                                    break;
                            }
                            break;
                        }
                    }
                    if (!exist)
                    {
                        condition.QueryFields.Add(other);
                        removeList.Add(powerQuery.Key);
                    }
                }
            }
            foreach (var item in addList)
            {
                condition.QueryFields.Add(item);
            }

            //仅添加合并后剩余的权限条件（仅剩下or条件的权限）
            foreach (var item in powerQueryFieldDic)
            {
                if (!removeList.Contains(item.Key))
                    condition.PowerQueryFieldDic.Add(item.Key, item.Value);
            }
            return condition;
        }

        private static void MergeFieldQuery<T>(List<LibQueryField> addList, LibQueryField item, LibQueryField other, T curValue1, T otherValue1, T curValue2, T otherValue2) where T : System.IComparable<T>
        {
            bool needReset = false;
            switch (item.QueryChar)
            {
                case LibQueryChar.Equal:
                    switch (other.QueryChar)
                    {
                        case LibQueryChar.Equal:
                            needReset = curValue1.CompareTo(otherValue1) != 0;
                            break;
                        case LibQueryChar.Contain:
                            needReset = !curValue1.ToString().Contains(otherValue1.ToString());
                            break;
                        case LibQueryChar.Region:
                            needReset = curValue1.CompareTo(otherValue1) < 0 || curValue2.CompareTo(otherValue2) > 0;
                            break;
                        case LibQueryChar.GreaterOrEqual:
                            needReset = curValue1.CompareTo(otherValue1) < 0;
                            break;
                        case LibQueryChar.LessOrEqual:
                            needReset = curValue1.CompareTo(otherValue1) > 0;
                            break;
                        case LibQueryChar.GreaterThan:
                            needReset = curValue1.CompareTo(otherValue1) <= 0;
                            break;
                        case LibQueryChar.LessThan:
                            needReset = curValue1.CompareTo(otherValue1) >= 0;
                            break;
                        case LibQueryChar.UnequalTo:
                            needReset = curValue1.CompareTo(otherValue1) == 0;
                            break;
                        case LibQueryChar.Include:
                            needReset = !otherValue1.ToString().Split(',').Contains(curValue1.ToString());
                            break;
                    }
                    break;
                case LibQueryChar.Contain:
                    switch (other.QueryChar)
                    {
                        case LibQueryChar.Equal:
                            needReset = true;
                            break;
                        case LibQueryChar.Contain:
                            needReset = !curValue1.ToString().Contains(otherValue1.ToString());
                            break;
                        case LibQueryChar.Region:
                        case LibQueryChar.GreaterOrEqual:
                        case LibQueryChar.LessOrEqual:
                        case LibQueryChar.GreaterThan:
                        case LibQueryChar.LessThan:
                        case LibQueryChar.UnequalTo:
                        case LibQueryChar.Include:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                    }
                    break;
                case LibQueryChar.Region:
                    switch (other.QueryChar)
                    {
                        case LibQueryChar.Equal:
                            needReset = true;
                            break;
                        case LibQueryChar.Contain:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                        case LibQueryChar.Region:
                            if (curValue1.CompareTo(otherValue1) < 0)
                                item.Value[0] = otherValue1;
                            else if (curValue2.CompareTo(otherValue2) > 0)
                                item.Value[1] = other.Value[1];
                            break;
                        case LibQueryChar.GreaterOrEqual:
                            needReset = curValue2.CompareTo(otherValue1) < 0;
                            if (!needReset && curValue1.CompareTo(otherValue1) < 0)
                            {
                                item.Value[0] = otherValue1;
                            }
                            break;
                        case LibQueryChar.LessOrEqual:
                            needReset = curValue1.CompareTo(otherValue1) > 0;
                            if (!needReset && curValue2.CompareTo(otherValue1) > 0)
                            {
                                item.Value[1] = otherValue1;
                            }
                            break;
                        case LibQueryChar.GreaterThan:
                            needReset = curValue2.CompareTo(otherValue1) <= 0;
                            if (!needReset && curValue1.CompareTo(otherValue1) <= 0)
                            {
                                item.Value[0] = otherValue1;
                            }
                            break;
                        case LibQueryChar.LessThan:
                            needReset = curValue1.CompareTo(otherValue1) >= 0;
                            if (!needReset && curValue2.CompareTo(otherValue1) >= 0)
                            {
                                item.Value[1] = otherValue1;
                            }
                            break;
                        case LibQueryChar.UnequalTo:
                            if (curValue1.CompareTo(otherValue1) <= 0 || curValue2.CompareTo(otherValue1) >= 0)
                            {
                                addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            }
                            break;
                        case LibQueryChar.Include:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                    }
                    break;
                case LibQueryChar.GreaterOrEqual:
                    switch (other.QueryChar)
                    {
                        case LibQueryChar.Equal:
                            needReset = true;
                            break;
                        case LibQueryChar.Contain:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                        case LibQueryChar.Region:
                            needReset = curValue1.CompareTo(otherValue2) > 0;
                            if (!needReset)
                            {
                                item.QueryChar = LibQueryChar.Region;
                                item.Value = other.Value;
                                item.Value[0] = curValue1;
                            }
                            break;
                        case LibQueryChar.GreaterOrEqual:
                            needReset = curValue1.CompareTo(otherValue1) < 0;
                            break;
                        case LibQueryChar.LessOrEqual:
                            needReset = curValue1.CompareTo(otherValue1) > 0;
                            if (!needReset)
                            {
                                item.QueryChar = LibQueryChar.Region;
                                item.Value = new object[] { curValue1, otherValue1 };
                            }
                            break;
                        case LibQueryChar.GreaterThan:
                            needReset = curValue1.CompareTo(otherValue1) <= 0;
                            break;
                        case LibQueryChar.LessThan:
                            needReset = curValue1.CompareTo(otherValue1) >= 0;
                            if (!needReset)
                            {
                                addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            }
                            break;
                        case LibQueryChar.UnequalTo:
                            if (curValue1.CompareTo(otherValue1) < 0)
                            {
                                addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            }
                            break;
                        case LibQueryChar.Include:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                    }
                    break;
                case LibQueryChar.LessOrEqual:
                    switch (other.QueryChar)
                    {
                        case LibQueryChar.Equal:
                            needReset = true;
                            break;
                        case LibQueryChar.Contain:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                        case LibQueryChar.Region:
                            needReset = curValue1.CompareTo(otherValue1) < 0;
                            if (!needReset)
                            {
                                item.QueryChar = LibQueryChar.Region;
                                item.Value = other.Value;
                                item.Value[1] = curValue1;
                            }
                            break;
                        case LibQueryChar.GreaterOrEqual:
                            needReset = curValue1.CompareTo(otherValue1) < 0;
                            if (!needReset)
                            {
                                item.QueryChar = LibQueryChar.Region;
                                item.Value = new object[] { otherValue1, curValue1 };
                            }
                            break;
                        case LibQueryChar.LessOrEqual:
                            needReset = curValue1.CompareTo(otherValue1) > 0;
                            break;
                        case LibQueryChar.GreaterThan:
                            needReset = curValue1.CompareTo(otherValue1) < 0;
                            if (!needReset)
                            {
                                addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            }
                            break;
                        case LibQueryChar.LessThan:
                            needReset = curValue1.CompareTo(otherValue1) >= 0;
                            break;
                        case LibQueryChar.UnequalTo:
                            if (curValue1.CompareTo(otherValue1) > 0)
                            {
                                addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            }
                            break;
                        case LibQueryChar.Include:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                    }
                    break;
                case LibQueryChar.GreaterThan:
                    switch (other.QueryChar)
                    {
                        case LibQueryChar.Equal:
                            needReset = true;
                            break;
                        case LibQueryChar.Contain:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                        case LibQueryChar.Region:
                            needReset = curValue1.CompareTo(otherValue2) >= 0;
                            if (!needReset)
                            {
                                addList.Add(new LibQueryField() { Name = other.Name, QueryChar = LibQueryChar.LessOrEqual, Value = new object[] { otherValue2 } });
                            }
                            break;
                        case LibQueryChar.GreaterOrEqual:
                            needReset = curValue1.CompareTo(otherValue1) <= 0;
                            break;
                        case LibQueryChar.LessOrEqual:
                            needReset = curValue1.CompareTo(otherValue1) >= 0;
                            if (!needReset)
                            {
                                addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            }
                            break;
                        case LibQueryChar.GreaterThan:
                            needReset = curValue1.CompareTo(otherValue1) < 0;
                            break;
                        case LibQueryChar.LessThan:
                            needReset = curValue1.CompareTo(otherValue1) > 0;
                            if (!needReset)
                            {
                                addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            }
                            break;
                        case LibQueryChar.UnequalTo:
                            if (curValue1.CompareTo(otherValue1) <= 0)
                            {
                                addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            }
                            break;
                        case LibQueryChar.Include:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                    }
                    break;
                case LibQueryChar.LessThan:
                    switch (other.QueryChar)
                    {
                        case LibQueryChar.Equal:
                            needReset = true;
                            break;
                        case LibQueryChar.Contain:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                        case LibQueryChar.Region:
                            needReset = curValue1.CompareTo(otherValue1) <= 0;
                            if (!needReset)
                            {
                                addList.Add(new LibQueryField() { Name = other.Name, QueryChar = LibQueryChar.GreaterOrEqual, Value = new object[] { otherValue1 } });
                            }
                            break;
                        case LibQueryChar.GreaterOrEqual:
                            needReset = curValue1.CompareTo(otherValue1) <= 0;
                            if (!needReset)
                            {
                                item.QueryChar = LibQueryChar.Region;
                                item.Value = new object[] { otherValue1, curValue1 };
                            }
                            break;
                        case LibQueryChar.LessOrEqual:
                            needReset = curValue1.CompareTo(otherValue1) >= 0;
                            break;
                        case LibQueryChar.GreaterThan:
                            needReset = curValue1.CompareTo(otherValue1) <= 0;
                            if (!needReset)
                            {
                                addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            }
                            break;
                        case LibQueryChar.LessThan:
                            needReset = curValue1.CompareTo(otherValue1) > 0;
                            break;
                        case LibQueryChar.UnequalTo:
                            if (curValue1.CompareTo(otherValue1) >= 0)
                            {
                                addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            }
                            break;
                        case LibQueryChar.Include:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                    }
                    break;
                case LibQueryChar.UnequalTo:
                    switch (other.QueryChar)
                    {
                        case LibQueryChar.Equal:
                            needReset = true;
                            break;
                        case LibQueryChar.Contain:
                        case LibQueryChar.Region:
                        case LibQueryChar.GreaterOrEqual:
                        case LibQueryChar.LessOrEqual:
                        case LibQueryChar.GreaterThan:
                        case LibQueryChar.LessThan:
                        case LibQueryChar.UnequalTo:
                        case LibQueryChar.Include:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                    }
                    break;
                case LibQueryChar.Include:
                    switch (other.QueryChar)
                    {
                        case LibQueryChar.Equal:
                            needReset = true;
                            break;
                        case LibQueryChar.Contain:
                        case LibQueryChar.Region:
                        case LibQueryChar.GreaterOrEqual:
                        case LibQueryChar.LessOrEqual:
                        case LibQueryChar.GreaterThan:
                        case LibQueryChar.LessThan:
                        case LibQueryChar.UnequalTo:
                            addList.Add(new LibQueryField() { Name = other.Name, QueryChar = other.QueryChar, Value = other.Value });
                            break;
                        case LibQueryChar.Include:
                            string[] array1 = curValue1.ToString().Split(',');
                            string[] array2 = otherValue1.ToString().Split(',');
                            StringBuilder builder = new StringBuilder();
                            foreach (string temp in array1)
                            {
                                if (array2.Contains(temp))
                                    builder.AppendFormat("{0},", temp);
                            }
                            if (builder.Length > 0)
                            {
                                builder.Remove(builder.Length - 1, 1);
                                item.Value[0] = builder.ToString();
                            }
                            else
                            {
                                needReset = true;
                            }
                            break;
                    }
                    break;
            }
            if (needReset)
            {
                item.QueryChar = other.QueryChar;
                item.Value = other.Value;
            }
        }
    }

    [DataContract]
    public class LibQueryCondition
    {
        private IList<LibQueryField> _QueryFields = null;
        [DataMember]
        public IList<LibQueryField> QueryFields
        {
            get
            {
                if (_QueryFields == null)
                    _QueryFields = new List<LibQueryField>();
                return _QueryFields;
            }
        }
        /// <summary>
        /// 是否包含子级数据
        /// </summary>
        [DataMember]
        public bool ContainsSub { get; set; }

        private Dictionary<string, List<LibQueryField>> _PowerQueryFieldDic = null;
        [DataMember]
        public Dictionary<string, List<LibQueryField>> PowerQueryFieldDic
        {
            get
            {
                if (_PowerQueryFieldDic == null)
                    _PowerQueryFieldDic = new Dictionary<string, List<LibQueryField>>();
                return _PowerQueryFieldDic;
            }
        }
        /// <summary>
        /// 检查数据是否符合当前条件。仅实现了部分比较符
        /// Zhangkj 20170329
        /// </summary>
        /// <param name="masterDic"></param>
        /// <returns></returns>
        public bool AccordOfThis(Dictionary<string, object> masterDic)
        {
            if (this.QueryFields != null && this.QueryFields.Count == 0)
                return true;
            if (masterDic == null || masterDic.Count == 0)
                return false;
            double dataValue = double.MinValue;
            double toCheckValue = double.MinValue;
            object dataObj = null;

            bool ret = true;
            foreach(LibQueryField field in this.QueryFields)
            {
                if (ret == false)
                    break;
                if (field == null || field.Value == null || field.Value.Count == 0 || field.Value[0] == null)
                    continue;
                if (masterDic.ContainsKey(field.Name) == false || masterDic[field.Name] == null)
                    return false;
                dataObj = masterDic[field.Name];
                switch (field.QueryChar)
                {
                    case LibQueryChar.Equal:
                        if (dataObj.Equals(field.Value[0]) == false)
                            ret = false;
                        break;
                    case LibQueryChar.Contain:
                        if (dataObj.ToString().Contains(field.Value[0].ToString()) == false)
                            ret = false;
                        break;
                    case LibQueryChar.GreaterOrEqual:
                        if (double.TryParse(field.Value[0].ToString(), out toCheckValue)
                                && double.TryParse(dataObj.ToString(), out dataValue))
                        {
                            if (dataValue >= toCheckValue)
                                ret = true;
                            else
                                ret = false;
                        }
                        else
                            ret = false;
                        break;
                    case LibQueryChar.GreaterThan:
                        if (double.TryParse(field.Value[0].ToString(), out toCheckValue)
                                && double.TryParse(dataObj.ToString(), out dataValue))
                        {
                            if (dataValue > toCheckValue)
                                ret = true;
                            else
                                ret = false;
                        }
                        else
                            ret = false;
                        break;
                    case LibQueryChar.Include:
                        ret = false;//包含先不实现
                        break;
                    case LibQueryChar.LessOrEqual:
                        if (double.TryParse(field.Value[0].ToString(), out toCheckValue)
                              && double.TryParse(dataObj.ToString(), out dataValue))
                        {
                            if (dataValue <= toCheckValue)
                                ret = true;
                            else
                                ret = false;
                        }
                        else
                            ret = false;
                        break;
                    case LibQueryChar.LessThan:
                        if (double.TryParse(field.Value[0].ToString(), out toCheckValue)
                              && double.TryParse(dataObj.ToString(), out dataValue))
                        {
                            if (dataValue < toCheckValue)
                                ret = true;
                            else
                                ret = false;
                        }
                        else
                            ret = false;
                        break;
                    case LibQueryChar.Region:
                        if (double.TryParse(dataObj.ToString(), out dataValue) && field.Value.Count > 1)
                        {
                            double firstCheckValue = double.MinValue;
                            double secondCheckValue = double.MinValue;
                            if (double.TryParse(field.Value[0].ToString(), out firstCheckValue)
                                && double.TryParse(field.Value[1].ToString(), out secondCheckValue))
                            {
                                if (dataValue >= firstCheckValue && dataValue <= secondCheckValue)
                                    ret = true;
                                else
                                    ret = false;
                            }
                            else
                                ret = false;
                        }
                        else
                            ret = false;
                        break;
                    case LibQueryChar.UnequalTo:
                        if (dataObj.Equals(field.Value[0]) == true)
                            ret = false;
                        break;
                }
            }
            return ret;
        }
    }
    [DataContract]
    public class LibQueryField
    {
        private string _Name;
        private LibQueryChar _QueryChar;
        private IList<object> _Value;
        [DataMember]
        public IList<object> Value
        {
            get
            {
                if (_Value == null)
                    _Value = new List<object>();
                return _Value;
            }
            set { _Value = value; }
        }
        [DataMember]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        [DataMember]
        public LibQueryChar QueryChar
        {
            get { return _QueryChar; }
            set { _QueryChar = value; }
        }
    }

    public enum LibQueryChar
    {
        /// <summary>
        /// 空
        /// </summary>
        None = 0,
        /// <summary>
        /// 等于
        /// </summary>
        Equal = 1,
        /// <summary>
        /// 包含
        /// </summary>
        Contain = 2,
        /// <summary>
        /// 区间
        /// </summary>
        Region = 3,
        /// <summary>
        /// 大于等于
        /// </summary>
        GreaterOrEqual = 4,
        /// <summary>
        /// 小于等于
        /// </summary>
        LessOrEqual = 5,
        /// <summary>
        /// 大于
        /// </summary>
        GreaterThan = 6,
        /// <summary>
        /// 小于
        /// </summary>
        LessThan = 7,
        /// <summary>
        /// 不等于
        /// </summary>
        UnequalTo = 8,
        /// <summary>
        ///  包括
        /// </summary>
        Include = 9,
    }
}
