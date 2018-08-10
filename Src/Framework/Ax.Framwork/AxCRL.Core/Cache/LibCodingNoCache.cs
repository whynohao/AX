using AxCRL.Comm.Utils;
using AxCRL.Core.Server;
using AxCRL.Data;
using AxCRL.Comm.Redis;
using AxCRL.Data.SqlBuilder;
using AxCRL.Template;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;

namespace AxCRL.Core.Cache
{
    public class LibCodingNoCache : MemoryCacheRedis
    {
        private static LibCodingNoCache _Default = null;
        private static object _LockObj = new object();
        private static ConcurrentDictionary<string, object> lockObjDic = null;

        public LibCodingNoCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static  LibCodingNoCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibCodingNoCache("LibCodingNoCache");
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
                string key = enumerator.Current.ToString();
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

        public string GetCodingNo(string progId, string fieldName, string prefix, int serialLen, LibDataAccess dataAccess)
        {
            string maxNo = string.Empty;
            string key = string.Format("{0}/t{1}", progId, prefix);
            object lockItem = lockObjDic.GetOrAdd(key, new object());
            lock (lockItem)
            {
                CodingNoValue codingNoValue = this.Get< CodingNoValue>(key)  ;
                if (codingNoValue == null)
                {
                    string curSerial = string.Empty;
                    int len = prefix.Length + serialLen;
                    SqlBuilder sqlBuilder = new SqlBuilder(progId);
                    string sql = sqlBuilder.GetQuerySql(0, string.Format("A.{0}", fieldName), string.Format("A.{0} Like '{1}%'", fieldName, prefix), string.Format("A.{0} DESC", fieldName));
                    if (dataAccess == null) //在单据的保存中，有可能产生新的单据，dataAccess必须是原始事务的dataAccess，否则可能出现死锁
                        dataAccess = new LibDataAccess();
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
                    codingNoValue = new CodingNoValue() { MaxValue = value };
                }
                if (codingNoValue.Unused.Count > 0)
                {
                    maxNo = codingNoValue.Unused.Dequeue();
                    this.Set(key, codingNoValue, new TimeSpan(0, 180, 0));//Zhangkj 20170303 修改Unused值后需要Set回Redis缓存
                }
                else
                {
                    codingNoValue.MaxValue++; 
                    this.Set(key, codingNoValue, new TimeSpan(0, 180, 0));
                    maxNo = string.Format("{0}{1}", prefix, codingNoValue.MaxValue.ToString().PadLeft(serialLen, '0'));
                }
            }
            return maxNo;
        }

        public void ReturnCodingNo(string progId, string prefix, string codingNo)
        {
            string key = string.Format("{0}/t{1}", progId, prefix);
            object lockItem = lockObjDic.GetOrAdd(key, new object());
            lock (lockItem)
            {
                CodingNoValue codingNoValue = this.Get< CodingNoValue>(key)  ;
                if (codingNoValue != null)
                {
                    codingNoValue.Unused.Enqueue(codingNo);
                }
                //增加了新的ReturnCode时，需要将值Set进Redis缓存。 原先使用MemoryCahche不需要此操作 Zhangkj 20170303
                this.Set<CodingNoValue>(key, codingNoValue, new TimeSpan(0, 180, 0));
            }
        }

    }
    
    public class CodingNoValue
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
            set
            {
                //Zhangkj 20170303 为了实现Redis缓存的反序列化而增加的Set方法
                this._Unused = value;
            }
        }

        public int MaxValue
        {
            get { return _MaxValue; }
            set { _MaxValue = value; }
        }

    }


    public class LibCodingRuleCache : MemoryCacheRedis
    {
        private static LibCodingRuleCache _Default = null;
        private static object _LockObj = new object();
        private static object _LockRuleObj = new object();

        public LibCodingRuleCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static  LibCodingRuleCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                            _Default = new LibCodingRuleCache("LibCodingRuleCache");
                    }
                }
                return _Default;
            }
        }

        public CodingRule GetCodingRule(BillType billType, string progId)
        {
            CodingRule codingRule = this.Get< CodingRule>(progId) ;
            if (codingRule == null)
            {
                lock (_LockRuleObj)
                {
                    codingRule = this.Get< CodingRule>(progId)  ;
                    if (codingRule == null)
                    {
                        codingRule = new CodingRule();
                        Dictionary<int, int> listIndex = new Dictionary<int, int>();
                        SqlBuilder sqlBuilder = new SqlBuilder("com.CodingRule");
                        //string sql = sqlBuilder.GetQuerySql(1, "B.ROW_ID,B.SECTIONTYPE,B.SECTIONLENGTH,B.FIELDNAME,B.SECTIONVALUE", string.Format("A.PROGID = {0} And A.VALIDITYSTARTDATE <= {1} And (A.VALIDITYENDDATE >= {1} or A.VALIDITYENDDATE = 0)", LibStringBuilder.GetQuotString(progId), LibDateUtils.GetCurrentDate()), "B.ROWNO ASC");
                        string sql = sqlBuilder.GetQuerySql(1, "B.ROW_ID,B.SECTIONTYPE,B.SECTIONLENGTH,B.FIELDNAME,B.SECTIONVALUE", string.Format("A.PROGID = {0}", LibStringBuilder.GetQuotString(progId), LibDateUtils.GetCurrentDate()), "B.ROWNO ASC");
                        LibDataAccess dataAccess = new LibDataAccess();
                        using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                        {
                            while (reader.Read())
                            {
                                CodingRuleItem rule = new CodingRuleItem();
                                rule.SectionType = (SectionType)LibSysUtils.ToInt32(reader["SECTIONTYPE"]);
                                rule.Length = LibSysUtils.ToInt32(reader["SECTIONLENGTH"]);
                                switch (rule.SectionType)
                                {
                                    case SectionType.None:
                                        rule.Value = LibSysUtils.ToString(reader["SECTIONVALUE"]);
                                        break;
                                    case SectionType.Dynamic:
                                        rule.FieldName = LibSysUtils.ToString(reader["FIELDNAME"]);
                                        rule.Values.Add(rule.FieldName, LibSysUtils.ToString(reader["SECTIONVALUE"]));
                                        int rowId = LibSysUtils.ToInt32(reader["ROW_ID"]);
                                        if (!listIndex.ContainsKey(rowId))
                                            listIndex.Add(rowId, codingRule.Items.Count);
                                        if (!codingRule.CreateOnSave)
                                            codingRule.CreateOnSave = true;
                                        break;
                                }
                                codingRule.Items.Add(rule);
                            }
                        }
                        sql = sqlBuilder.GetQuerySql(2, "C.PARENTROWID,C.FIELDVALUE,C.SECTIONVALUE", string.Format("A.PROGID = {0}", LibStringBuilder.GetQuotString(progId)));
                        using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                        {
                            while (reader.Read())
                            {
                                int rowId = LibSysUtils.ToInt32(reader["PARENTROWID"]);
                                if (listIndex.ContainsKey(rowId))
                                {
                                    CodingRuleItem rule = codingRule.Items[listIndex[rowId]];
                                    string fieldValue = LibSysUtils.ToString(reader["FIELDVALUE"]);
                                    if (!rule.Values.ContainsKey(fieldValue))
                                        rule.Values.Add(fieldValue, LibSysUtils.ToString(reader["SECTIONVALUE"]));
                                }
                            }
                        }
                        //如果为单据，默认产生编码规则 日期+6位流水码
                        if (codingRule.Items.Count == 0)
                        {
                            if (billType == BillType.Bill)
                            {
                                codingRule.Items.Add(new CodingRuleItem() { SectionType = SectionType.DateL });
                                codingRule.Items.Add(new CodingRuleItem() { SectionType = SectionType.SerialNum, Length = 6 });
                            }
                        }
                        else
                            codingRule.IsSetRule = true;
                        if (codingRule.Items.Count > 0)
                        { 
                            this.Set(progId, codingRule, new TimeSpan(0, 180, 0));
                        }
                    }
                }
            }
            return codingRule;
        }
        /// <summary>
        /// 将编码规则设定为createOnSave
        /// </summary>
        /// <param name="billType"></param>
        /// <param name="progId"></param>
        /// <param name="createOnSave">可选参数，默认为true</param>
        public void SetCreateOnSave(BillType billType, string progId, bool createOnSave = true)
        {
            CodingRule rule = GetCodingRule(billType, progId);
            if (rule != null)
                rule.CreateOnSave = createOnSave;
            //使用Redis缓存，需要将修改后的值Set回去
            this.Set<CodingRule>(progId, rule, new TimeSpan(0, 180, 0));
        }

        public override bool Remove(string key, string regionName = null)
        { 
            return   base.Remove(key); 
        }
    }
}
