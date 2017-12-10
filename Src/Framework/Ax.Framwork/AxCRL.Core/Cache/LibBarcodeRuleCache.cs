using AxCRL.Comm.Utils;
using AxCRL.Data;
using AxCRL.Comm.Redis;
using AxCRL.Data.SqlBuilder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.Caching;

namespace AxCRL.Core.Cache
{

    public class LibBarcodeCache : MemoryCacheRedis
    {
        private static LibBarcodeCache _Default = null;
        private static object _LockObj = new object();
        private static ConcurrentDictionary<string, object> lockObjDic = null;

        public LibBarcodeCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static   LibBarcodeCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibBarcodeCache("LibBarcodeCache");
                            lockObjDic = new ConcurrentDictionary<string, object>();
                        }
                    }
                }
                return _Default;
            }
        }

        public void RemoveCacheByProgId(string progId)
        { 
            IList<string> list = new List<string>();
            IEnumerator<string> enumerator = this.GetKeys();
            while (enumerator.MoveNext())
            {
                string key = enumerator.Current;
                if (string.Compare(progId, key.Substring(0, key.IndexOf("/t")), true) == 0)
                {
                    list.Add(key);
                }
            }
            foreach (var item in list)
            {
                this.Remove(item);
            } 
        }

        public IList<string> GetBarcode(string progId, string fieldName, string prefix, int serialLen, int number = 1)
        {
            IList<string> list = new List<string>();
            string key = string.Format("{0}/t{1}", progId, prefix);
            object lockItem = lockObjDic.GetOrAdd(key, new object());
            lock (lockItem)
            {

                BarcodeValue barcodeValue = this.Get<BarcodeValue>(key);  

                if (barcodeValue == null)
                {
                    string curSerial = string.Empty;
                    int len = prefix.Length + serialLen;
                    SqlBuilder sqlBuilder = new SqlBuilder(progId);
                    string sql = sqlBuilder.GetQuerySql(0, string.Format("A.{0}", fieldName), string.Format("A.{0} Like '{1}%'", fieldName, prefix), string.Format("A.{0} DESC", fieldName));
                    LibDataAccess dataAccess = new LibDataAccess();
                    int serial = 0;
                    using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                    {
                        while (reader.Read())
                        {
                            string temp = reader.GetString(0);
                            if (temp.Length != len)
                                continue;
                            if (!int.TryParse(temp.Substring(prefix.Length, serialLen), out serial))
                                continue;
                            curSerial = temp;
                            break;
                        }
                    }
                    int value = 0;
                    if (!string.IsNullOrEmpty(curSerial))
                        int.TryParse(curSerial.Substring(prefix.Length, serialLen), out value);
                    barcodeValue = new BarcodeValue() { MaxValue = value };
                }
                if (barcodeValue.Unused.Count >= number)
                {
                    for (int i = 0; i < number; i++)
                    {
                        list.Add(barcodeValue.Unused.Dequeue());
                    }
                }
                else
                {
                    for (int i = 0; i < number; i++)
                    {
                        string maxValue = (++barcodeValue.MaxValue).ToString();
                        list.Add(string.Format("{0}{1}", prefix, maxValue.PadLeft(serialLen, '0')));
                    }
                  
                    this.Set(key, barcodeValue, new TimeSpan(0, 180, 0));
                }
            }
            return list;
        }

        public void ReturnBarcode(string progId, string prefix, string barcode)
        {
            string key = string.Format("{0}/t{1}", progId, prefix);
            object lockItem = lockObjDic.GetOrAdd(key, new object());
            lock (lockItem)
            {
                BarcodeValue barcodeValue = this.Get<BarcodeValue>(key)  ;
                if (barcodeValue != null)
                {
                    barcodeValue.Unused.Enqueue(barcode);
                }
            }
        }

    }
 
    public class BarcodeValue
    {
        private int _MaxValue = 0;
        private Queue<string> _Unused = null;

        public Queue<string> Unused
        {
            get
            {
                if (_Unused == null)
                    _Unused = new Queue<string>();
                return _Unused;
            }
        }

        public int MaxValue
        {
            get { return _MaxValue; }
            set { _MaxValue = value; }
        }

    }

    public class LibBarcodeRuleCache : MemoryCacheRedis
    {
        private static LibBarcodeRuleCache _Default = null;
        private static object _LockObj = new object();
        private static object _LockRuleObj = new object();

        public LibBarcodeRuleCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static  LibBarcodeRuleCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                            _Default = new LibBarcodeRuleCache("LibBarcodeRuleCache");
                    }
                }
                return _Default;
            }
        }

        public BarcodeRule GetBarcodeRule(string ruleId)
        {
            BarcodeRule codingRule = this.Get< BarcodeRule>(ruleId) ;
            if (codingRule == null)
            {
                lock (_LockRuleObj)
                {
                    codingRule = this.Get< BarcodeRule>(ruleId)  ;
                    if (codingRule == null)
                    {
                        string sql;
                        SqlBuilder sqlBuilder = new SqlBuilder("com.BarcodeRule");
                        sql = sqlBuilder.GetQuerySql(1, "B.ROW_ID,B.SECTIONTYPE,B.SECTIONLENGTH,B.TABLEINDEX,B.FIELDNAME,B.SECTIONVALUE", string.Format("A.BARCODERULEID={0} And A.VALIDITYSTARTDATE <= {1} And (A.VALIDITYENDDATE >= {1} or A.VALIDITYENDDATE = 0)", LibStringBuilder.GetQuotString(ruleId), LibDateUtils.GetCurrentDate()), "B.ROWNO ASC");
                        codingRule = new BarcodeRule();
                        Dictionary<int, int> listIndex = new Dictionary<int, int>();
                        LibDataAccess dataAccess = new LibDataAccess();
                        using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                        {
                            int startIndex = 0;
                            while (reader.Read())
                            {
                                BarcodeRuleItem rule = new BarcodeRuleItem();
                                rule.SectionType = (BarcodeRuleSectionType)LibSysUtils.ToInt32(reader["SECTIONTYPE"]);
                                rule.Start = startIndex;
                                rule.Length = LibSysUtils.ToInt32(reader["SECTIONLENGTH"]);
                                switch (rule.SectionType)
                                {
                                    case BarcodeRuleSectionType.None:
                                        rule.Value = LibSysUtils.ToString(reader["SECTIONVALUE"]);
                                        break;
                                    case BarcodeRuleSectionType.Dynamic:
                                        rule.TableIndex = LibSysUtils.ToInt32(reader["TABLEINDEX"]);
                                        rule.FieldName = LibSysUtils.ToString(reader["FIELDNAME"]);
                                        rule.Values.Add(rule.FieldName, LibSysUtils.ToString(reader["SECTIONVALUE"]));
                                        int rowId = LibSysUtils.ToInt32(reader["ROW_ID"]);
                                        if (!listIndex.ContainsKey(rowId))
                                            listIndex.Add(rowId, codingRule.Items.Count);
                                        break;
                                }
                                startIndex += rule.Length;
                                codingRule.Items.Add(rule);

                            }
                        }
                        sql = sqlBuilder.GetQuerySql(2, "C.PARENTROWID,C.FIELDVALUE,C.SECTIONVALUE", string.Format("A.BARCODERULEID={0} And A.VALIDITYSTARTDATE <= {1} And (A.VALIDITYENDDATE >= {1} or A.VALIDITYENDDATE = 0)", LibStringBuilder.GetQuotString(ruleId), LibDateUtils.GetCurrentDate()));
                        using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                        {
                            while (reader.Read())
                            {
                                int rowId = LibSysUtils.ToInt32(reader["PARENTROWID"]);
                                if (listIndex.ContainsKey(rowId))
                                {
                                    BarcodeRuleItem rule = codingRule.Items[listIndex[rowId]];
                                    string fieldValue = LibSysUtils.ToString(reader["FIELDVALUE"]);
                                    if (!rule.Values.ContainsKey(fieldValue))
                                        rule.Values.Add(fieldValue, LibSysUtils.ToString(reader["SECTIONVALUE"]));
                                }
                            }
                        } 
                        this.Set(ruleId, codingRule, new TimeSpan(0, 180, 0));
                    }
                }
            }
            return codingRule;
        }

        public override bool Remove(string key, string regionName = null)
        { 
            return base.Remove(key);
        }
    }

     
    public class BarcodeRule
    {

        private IList<BarcodeRuleItem> _Items;
        /// <summary>
        /// 规则明细
        /// </summary>
        public IList<BarcodeRuleItem> Items
        {
            get
            {
                if (_Items == null)
                    _Items = new List<BarcodeRuleItem>();
                return _Items;
            }
        }
    }


    public class BarcodeRuleItem
    {
        private int _TableIndex = 0;

        public int TableIndex
        {
            get { return _TableIndex; }
            set { _TableIndex = value; }
        }

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

        private int _Start;

        public int Start
        {
            get { return _Start; }
            set { _Start = value; }
        }

        private int _Length;

        public int Length
        {
            get { return _Length; }
            set { _Length = value; }
        }
        private BarcodeRuleSectionType _SectionType = BarcodeRuleSectionType.None;

        public BarcodeRuleSectionType SectionType
        {
            get { return _SectionType; }
            set { _SectionType = value; }
        }
    }
    public enum BarcodeRuleSectionType
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
        /// <summary>
        /// 16进制日期(7位）
        /// </summary>
        DateL16 = 8,
        /// <summary>
        /// 16进制日期(5位）
        /// </summary>
        DateS16 = 9
    }
}