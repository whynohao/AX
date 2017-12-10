using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Server
{
    public class LibBarcodeServer
    {
        private static LibBarcodeServer _Default = null;
        private static object _LockObj = new object();

        private LibBarcodeServer()
        {

        }
        public static LibBarcodeServer Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                            _Default = new LibBarcodeServer();
                    }
                }
                return _Default;
            }
        }

        private string GetPrefix(BarcodeRule barcodeRule, List<DataRow> rowList, ref int serialLen)
        {
            StringBuilder prefix = new StringBuilder();
            foreach (BarcodeRuleItem item in barcodeRule.Items)
            {
                switch (item.SectionType)
                {
                    case BarcodeRuleSectionType.None:
                        prefix.Append(item.Value);
                        break;
                    case BarcodeRuleSectionType.DateL:
                        prefix.Append(LibDateUtils.GetCurrentDate());
                        break;
                    case BarcodeRuleSectionType.DateS:
                        prefix.Append(LibDateUtils.GetCurrentDate().ToString().Remove(0, 2));
                        break;
                    case BarcodeRuleSectionType.DateS1:
                        prefix.Append(LibDateUtils.GetSpecialDate());
                        break;
                    case BarcodeRuleSectionType.DateAB:
                        prefix.Append(LibDateUtils.GetDateForABYear());
                        break;
                    case BarcodeRuleSectionType.DateL16:
                        prefix.Append(Convert.ToString(LibDateUtils.GetCurrentDate(), 16));
                        break;
                    case BarcodeRuleSectionType.DateS16:
                        prefix.Append(Convert.ToString(int.Parse(LibDateUtils.GetCurrentDate().ToString().Remove(0, 2)), 16));
                        break;
                    case BarcodeRuleSectionType.Dynamic:
                        DataRow curRow = rowList[item.TableIndex];
                        string fieldValue = LibSysUtils.ToString(curRow[item.FieldName]);
                        if (item.Values.ContainsKey(fieldValue))
                            prefix.Append(item.Values[fieldValue]);
                        else
                        {
                            string value = item.Values[item.FieldName];
                            //规则：如果没有设定字段为其他值时的固定字符。则默认使用字段当前值，不足用0补位
                            if (string.IsNullOrEmpty(value))
                            {
                                if (fieldValue.Length == item.Length)
                                    prefix.Append(fieldValue);
                                else if (fieldValue.Length > item.Length)
                                    prefix.Append(fieldValue.Substring(0, item.Length));
                                else
                                    prefix.Append(fieldValue.PadRight(item.Length, '0'));
                            }
                            else
                                prefix.Append(value);
                        }
                        break;
                    case BarcodeRuleSectionType.SerialNum:
                        serialLen = item.Length;
                        break;
                    default:
                        break;
                }
            }
            return prefix.ToString();
        }


        public string GetBarcode(string progId, string fieldName, string ruleId, List<DataRow> rowList, ref int serialLen)
        {
            string barcode = string.Empty;
            BarcodeRule barcodeRule = LibBarcodeRuleCache.Default.GetBarcodeRule(ruleId);
            string prefix = GetPrefix(barcodeRule, rowList, ref serialLen);
            IList<string> list = LibBarcodeCache.Default.GetBarcode(progId, fieldName, prefix, serialLen);
            if (list.Count > 0)
                barcode = list[0];
            return barcode;
        }

        public IList<string> GetBatchBarcode(string progId, string fieldName, string ruleId, List<DataRow> rowList, int number)
        {
            BarcodeRule barcodeRule = LibBarcodeRuleCache.Default.GetBarcodeRule(ruleId);
            int serialLen = 0;
            string prefix = GetPrefix(barcodeRule, rowList, ref serialLen);
            IList<string> list = LibBarcodeCache.Default.GetBarcode(progId, fieldName, prefix, serialLen, number);
            return list;
        }

        public void ReturnCodingNo(string progId, string ruleId, List<DataRow> rowList, string barcode)
        {
            BarcodeRule barcodeRule = LibBarcodeRuleCache.Default.GetBarcodeRule(ruleId);
            int serialLen = 0;
            string prefix = GetPrefix(barcodeRule, rowList, ref serialLen);
            LibCodingNoCache.Default.ReturnCodingNo(progId, prefix, barcode);
        }

    }
}
