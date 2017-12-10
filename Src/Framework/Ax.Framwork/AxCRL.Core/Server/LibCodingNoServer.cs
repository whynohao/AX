using AxCRL.Comm.Utils;
using AxCRL.Core.Cache;
using AxCRL.Data;
using AxCRL.Template;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Core.Server
{
    public class LibCodingNoServer
    {
        private static LibCodingNoServer _Default = null;
        private static object _LockObj = new object();

        private LibCodingNoServer()
        {

        }
        public static LibCodingNoServer Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                            _Default = new LibCodingNoServer();
                    }
                }
                return _Default;
            }
        }

        private string GetPrefix(CodingRule codingRule, DataRow masterRow, ref int serialLen)
        {
            StringBuilder prefix = new StringBuilder();
            foreach (CodingRuleItem item in codingRule.Items)
            {
                switch (item.SectionType)
                {
                    case SectionType.None:
                        prefix.Append(item.Value);
                        break;
                    case SectionType.DateL:
                        prefix.Append(LibDateUtils.GetCurrentDate());
                        break;
                    case SectionType.DateL1:
                        prefix.Append(LibDateUtils.GetCurrentDate().ToString().Remove(0, 2).Remove(4, 2));
                        break;
                    case SectionType.DateS:
                        prefix.Append(LibDateUtils.GetCurrentDate().ToString().Remove(0, 2));
                        break;
                    case SectionType.DateS1:
                        prefix.Append(LibDateUtils.GetSpecialDate());
                        break;
                    case SectionType.DateAB:
                        prefix.Append(LibDateUtils.GetDateForABYear());
                        break;
                    case SectionType.Dynamic:
                        if (masterRow != null)
                        {
                            string fieldValue = LibSysUtils.ToString(masterRow[item.FieldName]);
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
                        }
                        break;
                    case SectionType.SerialNum:
                        serialLen = item.Length;
                        break;
                    default:
                        break;
                }
            }
            return prefix.ToString();
        }

        public string GetCodingNo(BillType billType, string progId, string fieldName, DataRow masterRow, bool addNew, LibDataAccess dataAccess)
        {
            CodingRule codingRule;
            return GetCodingNo(billType, progId, fieldName, masterRow, addNew, out codingRule, dataAccess);
        }

        public string GetCodingNo(BillType billType, string progId, string fieldName, DataRow masterRow, bool addNew, out CodingRule codingRule, LibDataAccess dataAccess)
        {
            string codingNo = string.Empty;
            codingRule = LibCodingRuleCache.Default.GetCodingRule(billType, progId);
            if ((!addNew && codingRule.CreateOnSave) || (addNew && !codingRule.CreateOnSave))
            {
                if (codingRule.Items.Count > 0)
                {
                    int serialLen = 0;
                    string prefix = GetPrefix(codingRule, masterRow, ref serialLen);
                    codingNo = LibCodingNoCache.Default.GetCodingNo(progId, fieldName, prefix, serialLen, dataAccess);
                }
            }
            return codingNo;
        }

        public void ReturnCodingNo(BillType billType, string progId, DataRow masterRow, string codingNo)
        {
            CodingRule codingRule = LibCodingRuleCache.Default.GetCodingRule(billType, progId);
            int serialLen = 0;
            string prefix = GetPrefix(codingRule, masterRow, ref serialLen);
            LibCodingNoCache.Default.ReturnCodingNo(progId, prefix, codingNo);
        }

        //public IList<string> GetCodingNoList(BillType billType, string progId, string fieldName, int count, DataRow masterRow)
        //{
        //    throw new NotImplementedException();
        //}
    }

    public class CodingRule
    {
        private bool _IsSetRule = false;
        /// <summary>
        /// 是否有设定规则
        /// </summary>
        public bool IsSetRule
        {
            get { return _IsSetRule; }
            set { _IsSetRule = value; }
        }

        private bool _CreateOnSave = false;
        /// <summary>
        /// 在保存时才能创建，因为用到了表头的动态字段
        /// </summary>
        public bool CreateOnSave
        {
            get { return _CreateOnSave; }
            set { _CreateOnSave = value; }
        }

        private IList<CodingRuleItem> _Items;
        /// <summary>
        /// 规则明细
        /// </summary>
        public IList<CodingRuleItem> Items
        {
            get
            {
                if (_Items == null)
                    _Items = new List<CodingRuleItem>();
                return _Items;
            }
        }
    }


    public class CodingRuleItem
    {
        private string _FieldName;

        public string FieldName
        {
            get { return _FieldName; }
            set { _FieldName = value; }
        }
        private Dictionary<string, string> _Values;

        public Dictionary<string, string> Values
        {
            get
            {
                if (_Values == null)
                    _Values = new Dictionary<string, string>();
                return _Values;
            }
        }
        private string _Value;

        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }
        private int _Length;

        public int Length
        {
            get { return _Length; }
            set { _Length = value; }
        }
        private SectionType _SectionType = SectionType.None;

        public SectionType SectionType
        {
            get { return _SectionType; }
            set { _SectionType = value; }
        }
    }
    public enum SectionType
    {
        /// <summary>
        /// 固定值
        /// </summary>
        None = 0,
        /// <summary>
        /// 流水号
        /// </summary>
        SerialNum = 1,
        /// <summary>
        /// 动态段
        /// </summary>
        Dynamic = 2,
        /// <summary>
        /// 日期（yyyymmdd）
        /// </summary>
        DateL = 3,
        /// <summary>
        /// 日期（yymmdd）
        /// </summary>
        DateS = 4,
        /// <summary>
        /// 日期（ddmmyy）
        /// </summary>
        DateS1 = 5,
        /// <summary>
        /// 日期（ABmmdd）
        /// </summary>
        DateAB = 6,
        /// <summary>
        /// 日期（yymm）
        /// </summary>
        DateL1 = 7,
    }
}
