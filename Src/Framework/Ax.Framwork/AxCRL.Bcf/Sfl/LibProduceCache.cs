using AxCRL.Comm.Utils;
using AxCRL.Data;
using AxCRL.Comm.Redis;
using AxCRL.Data.SqlBuilder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Bcf.Sfl
{

    public class LibProduceCache : MemoryCacheRedis
    {
        private static LibProduceCache _Default = null;
        private static object _LockObj = new object();
        private static ConcurrentDictionary<string, object> lockObjDic = null;

        public LibProduceCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static  LibProduceCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibProduceCache("LibProduceCache");
                            lockObjDic = new ConcurrentDictionary<string, object>();
                        }
                    }
                }
                return _Default;
            }
        }

        public override bool Remove(string billNo, string regionName = null)
        {
            return base.Remove(billNo);
        }
        /// <summary>
        /// 将object格式化成字节数组byte[]
        /// </summary>
        /// <param name="billNo">单据编号</param>
        /// <returns>字节数组</returns>

        public ProduceData GetProduceData(string billNo)
        {
            ProduceData produceData = null;
            string json= this.StringGet(billNo);
            if (!string.IsNullOrEmpty((json)))
            {
                produceData = JsonUtiler.Deserialize<ProduceData>(json);
            } 
            if (produceData == null)
            {
                object lockItem = lockObjDic.GetOrAdd(billNo, new object());
                lock (lockItem)
                {
                    json = this.StringGet(billNo);
                    if (!string.IsNullOrEmpty((json)))
                    {
                        produceData = JsonUtiler.Deserialize<ProduceData>(json);
                    }
                    //  produceData = this.Get< ProduceData>(billNo)  ;
                    if (produceData == null)
                    {
                        //CacheItemPolicy policy = new CacheItemPolicy();
                        //policy.SlidingExpiration = new TimeSpan(0, 60, 0); //60分钟内不访问自动剔除
                        LibBcfData ppWorkOrderBcf = (LibBcfData)LibBcfSystem.Default.GetBcfInstance("pp.WorkOrder");
                        DataSet ds = ppWorkOrderBcf.BrowseTo(new object[] { billNo });
                        produceData = new ProduceData(ds);
                        this.Set(billNo, produceData, new TimeSpan(0, 720, 0));
                        
                     //   byte[] binaryDataResult = GetBinaryFormatData(produceData);  
                     //   Default.StringSetBytes(billNo, binaryDataResult, new TimeSpan(0, 30, 0));
                        
                    }
                }
            }
            return produceData;
        }
    }

    public class LibPurCheckCache : MemoryCacheRedis
    {
        private static LibPurCheckCache _Default = null;
        private static object _LockObj = new object();
        private static ConcurrentDictionary<string, object> lockObjDic = null;

        public LibPurCheckCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static LibPurCheckCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibPurCheckCache("LibPurCheckCache");
                            lockObjDic = new ConcurrentDictionary<string, object>();
                        }
                    }
                }
                return _Default;
            }
        }

        public override bool Remove(string billNo, string regionName = null)
        {
            return base.Remove(billNo);
        }


        public PurCheckData GetPurCheckData(string billNo)
        {
             
            PurCheckData purCheckData = null;

            byte[] mybyte = this.StringGetBytes(billNo);
            if (mybyte != null)
            {
                purCheckData = (PurCheckData)RetrieveObjectex(mybyte);
            }


            if (purCheckData == null)
            {
                object lockItem = lockObjDic.GetOrAdd(billNo, new object());
                lock (lockItem)
                {
                    mybyte = this.StringGetBytes(billNo);
                    if (mybyte != null)
                    {
                        purCheckData = (PurCheckData)RetrieveObjectex(mybyte);
                    }
                    //  purCheckData = this.Get< PurCheckData>(billNo)  ;
                    if (purCheckData == null)
                    {
                        //CacheItemPolicy policy = new CacheItemPolicy();
                        //policy.SlidingExpiration = new TimeSpan(0, 60, 0); //60分钟内不访问自动剔除
                        LibBcfData ppWorkOrderBcf = (LibBcfData)LibBcfSystem.Default.GetBcfInstance("qc.PurQualityCheck");
                        DataSet ds = ppWorkOrderBcf.BrowseTo(new object[] { billNo });
                        purCheckData = new PurCheckData(ds);
                        this.Set(billNo, purCheckData, new TimeSpan(0, 720, 0));


                        //   byte[] binaryDataResult = GetBinaryFormatData(purCheckData);  
                        //   Default.StringSetBytes(billNo, binaryDataResult, new TimeSpan(0, 30, 0));
                    }
                }
            }
            return purCheckData;
        }
    }

    public class LibOutWareCheckCache : MemoryCacheRedis
    {
        private static LibOutWareCheckCache _Default = null;
        private static object _LockObj = new object();
        private static ConcurrentDictionary<string, object> lockObjDic = null;

        public LibOutWareCheckCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static  LibOutWareCheckCache Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibOutWareCheckCache("LibOutWareCheckCache");
                            lockObjDic = new ConcurrentDictionary<string, object>();
                        }
                    }
                }
                return _Default;
            }
        }

        public override bool Remove(string billNo, string regionName = null)
        {
            
            return base.Remove(billNo);
        }


        public OutWareCheckData GetOutWareCheckData(string billNo)
        {
            OutWareCheckData outWareCheckData = null;
            if (!string.IsNullOrEmpty(billNo))
                outWareCheckData = this.Get< OutWareCheckData>(billNo) ;
            if (outWareCheckData == null)
            {
                object lockItem = lockObjDic.GetOrAdd(billNo, new object());
                lock (lockItem)
                {
                    outWareCheckData = this.Get< OutWareCheckData>(billNo)  ;
                    if (outWareCheckData == null)
                    {
                        //CacheItemPolicy policy = new CacheItemPolicy();
                        //policy.SlidingExpiration = new TimeSpan(0, 60, 0); //60分钟内不访问自动剔除
                        LibBcfData ppWorkOrderBcf = (LibBcfData)LibBcfSystem.Default.GetBcfInstance("qc.OwQualityCheck");
                        DataSet ds = ppWorkOrderBcf.BrowseTo(new object[] { billNo });
                        outWareCheckData = new OutWareCheckData(ds);
                        this.Set(billNo, outWareCheckData, new TimeSpan(0, 60, 0));
                    }
                }
            }
            return outWareCheckData;
        }
    }
    [Serializable]
    public class ProduceData
    {
        private DataSet _WorkOrder;

        public DataSet WorkOrder
        {
            get { return _WorkOrder; }
            set { _WorkOrder = value; }
        }


        public ProduceData(DataSet workOrder)
        {
            this.WorkOrder = workOrder;
            InitPreWorkProcessNo();
        }

        private Dictionary<int, WorkProcessInfo> _WorkProcessNo;

        public Dictionary<int, WorkProcessInfo> WorkProcessNo
        {
            get
            {
                if (_WorkProcessNo == null)
                    _WorkProcessNo = new Dictionary<int, WorkProcessInfo>();
                return _WorkProcessNo;
            }
        }

        private Dictionary<string, IList<int>> _WsRelWorkProcessNo;

        public Dictionary<string, IList<int>> WsRelWorkProcessNo
        {
            get
            {
                if (_WsRelWorkProcessNo == null)
                    _WsRelWorkProcessNo = new Dictionary<string, IList<int>>();
                return _WsRelWorkProcessNo;
            }
        }


        private List<int> _FirstWorkProcessNo;

        public List<int> FirstWorkProcessNo
        {
            get {
                if (_FirstWorkProcessNo == null)
                {
                    _FirstWorkProcessNo = new List<int>();
                }
                return _FirstWorkProcessNo; }
            //set { _FirstWorkProcessNo = value; }
        }

        private int _LastWorkProcessNo;

        public int LastWorkProcessNo
        {
            get { return _LastWorkProcessNo; }
            set { _LastWorkProcessNo = value; }
        }

        private void InitPreWorkProcessNo()
        {
            int preWorkProcessNo = 0;
            DataView dataView = this.WorkOrder.Tables[2].DefaultView;
          
            dataView.Sort = "WORKPROCESSNO ASC";
            foreach (DataRowView curRow in dataView)
            {
               
                if (!LibSysUtils.ToBoolean(curRow["NEEDGATHER"]))
                    continue;
                int workProcessNo = LibSysUtils.ToInt32(curRow["WORKPROCESSNO"]);



                WorkProcessNo.Add(workProcessNo, new WorkProcessInfo(curRow.Row));
                WorkProcessNo[workProcessNo].NextWorkProcessNo = LibSysUtils.ToInt32(curRow["TRANSFERWORKPROCESSNO"]);
                WorkProcessNo[workProcessNo].DoWorkProcessNo = LibSysUtils.ToBoolean(curRow["DOWORKPROCESS"]);
                foreach (KeyValuePair<int, WorkProcessInfo> pair in WorkProcessNo)
                {
                    if (pair.Value.NextWorkProcessNo == workProcessNo)
                    {
                        if (!WorkProcessNo[workProcessNo].PreWorkProcessNo.Contains(pair.Key))
                        {
                            WorkProcessNo[workProcessNo].PreWorkProcessNo.Add(pair.Key);
                        }
                    }
                }
                if (WorkProcessNo[workProcessNo].PreWorkProcessNo.Count == 0)
                {
                    WorkProcessNo[workProcessNo].PreWorkProcessNo.Add(0);
                    this.FirstWorkProcessNo.Add(workProcessNo);
                }

                
                preWorkProcessNo = workProcessNo;
            }
            _LastWorkProcessNo = preWorkProcessNo;
            foreach (DataRow curRow in this.WorkOrder.Tables[3].Rows)
            {
                string workstationId = LibSysUtils.ToString(curRow["WORKSTATIONID"]);
                DataRow parentRow = this.WorkOrder.Tables[2].Rows.Find(new object[] { curRow["BILLNO"], curRow["PARENTROWID"] });
                if (parentRow != null)
                {
                    int workProcessNo = LibSysUtils.ToInt32(parentRow["WORKPROCESSNO"]);
                    if (WsRelWorkProcessNo.ContainsKey(workstationId))
                    {
                        if (!WsRelWorkProcessNo[workstationId].Contains(workProcessNo))
                            WsRelWorkProcessNo[workstationId].Add(workProcessNo);
                    }
                    else
                    {
                        WsRelWorkProcessNo.Add(workstationId, new List<int>() { workProcessNo });
                    }
                }
            }
        }


    }


    //[Serializable]
    //public class AxDataRow:DataRow
    //{
    //    protected internal AxDataRow(DataRowBuilder builder)
    //    {
           
    //    } 
    //}

    [Serializable]
    public class WorkProcessInfo
    {
        private List<int> _PreWorkProcessNo;

        private int _NextWorkProcessNo = 0;


        private bool _DoWorkProcessNo = true;

        public bool DoWorkProcessNo
        {
            get { return _DoWorkProcessNo; }
            set { _DoWorkProcessNo = value; }
        }
    
        private DataRow _DataRow;

        public DataRow DataRow
        {
            get { return _DataRow; }
            set { _DataRow = value; }
        }

        public int NextWorkProcessNo
        {
            get { return _NextWorkProcessNo; }
            set { _NextWorkProcessNo = value; }
        }

        public List<int> PreWorkProcessNo
        {
            get
            {
                if (_PreWorkProcessNo == null)
                {
                    _PreWorkProcessNo = new List<int>();
                }
                return _PreWorkProcessNo; }
            //set { _PreWorkProcessNo = value; }
        }

        public WorkProcessInfo( DataRow dataRow)
        {
            this._DataRow = dataRow;
        }
    }
    
    [Serializable]
    public class PurCheckData
    {
        private DataSet _PurCheckOrder;

        public DataSet PurCheckOrder
        {
            get { return _PurCheckOrder; }
            set { _PurCheckOrder = value; }
        }

        public PurCheckData(DataSet purCheckOrder)
        {
            this.PurCheckOrder = purCheckOrder;
        }
    }
   
    [Serializable]
    public class OutWareCheckData
    {
        private DataSet _OutWareCheckOrder;

        public DataSet OutWareCheckOrder
        {
            get { return _OutWareCheckOrder; }
            set { _OutWareCheckOrder = value; }
        }

        public OutWareCheckData(DataSet outWareCheckOrder)
        {
            this.OutWareCheckOrder = outWareCheckOrder;
        }
    }

}
