using AxCRL.Comm.Utils;
using AxCRL.Parser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AxCRL.Core.Comm
{
    public static class LibParseHelper
    {
        public static bool Parse(string condition, List<Dictionary<string, object>> dataList)
        {
            return ParseCore(condition, null, dataList);
        }

        public static bool Parse(string condition, List<DataRow> dataRowList)
        {
            return ParseCore(condition, dataRowList, null);
        }

        private static bool ParseCore(string condition, List<DataRow> dataRowList, List<Dictionary<string, object>> dataList)
        {
            bool result = false;
            Memory memory = new Memory();
            //匹配类似表达式"[A.QTY]>=10 && ([A.MaterialId]=='13212321' || ([A.RANGEID]=='AAA' && [A.SSID]=='9999')) && (([A.DD]=='xxx' || [A.DD]=='xxx1' || [A.DD]=='xxx2') || [A.ZZ]>=0)";
            string pattern = @"[[][A-Z]\.\w+[]]";
            MatchCollection matchList = Regex.Matches(condition, pattern);
            HashSet<string> temp = new HashSet<string>();
            foreach (var item in matchList)
            {
                string field = item.ToString();
                if (temp.Contains(field))
                    continue;
                temp.Add(field);
                string copyField = field;
                field = field.Remove(0, 1);
                field = field.Remove(field.Length - 1, 1);
                int tableIndex = (int)field[0] - (int)'A';
                string fieldName = field.Substring(2, field.Length - 2);
                object value = null;
                if (dataRowList != null)
                {
                    if (dataRowList.Count < tableIndex + 1)
                        return result;
                    if (!dataRowList[tableIndex].Table.Columns.Contains(fieldName))
                        return result;
                    value = dataRowList[tableIndex][fieldName];
                }
                else if (dataList != null)
                {
                    if (dataList.Count < tableIndex + 1)
                        return result;
                    if (!dataList[tableIndex].ContainsKey(fieldName))
                        return result;
                    value = dataList[tableIndex][fieldName];
                }
                memory.AddObject(fieldName, value);
                condition = condition.Replace(copyField, fieldName);
            }
            Script.Execute("if(" + condition + "){ret=true;}else{ret=false;}", memory);
            result = LibSysUtils.ToBoolean(memory["ret"].value);
            return result;
        }
    }
}
