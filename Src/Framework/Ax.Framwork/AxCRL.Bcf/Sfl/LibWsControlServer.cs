using AxCRL.Comm.Utils;
using AxCRL.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Bcf.Sfl
{
    public class LibWsControlServer
    {
        private static LibWsControlServer _Default = null;
        private static object _LockObj = new object();
        private static ProductScheduling _ProductScheduling = null;
        private static object _LockProductScheduling = new object();

        private LibWsControlServer()
        {

        }
        public static LibWsControlServer Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibWsControlServer();
                            _ProductScheduling = new ProductScheduling();
                            string sql = "SELECT BILLNO FROM PPWORKORDER WHERE CURRENTSTATE=2 AND WORKORDERSTATE=1";
                            LibDataAccess dataAccess = new LibDataAccess();
                            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                            {
                                while (reader.Read())
                                {
                                    _ProductScheduling.WorkOrderList.Add(reader.GetString(0));
                                }
                            }
                            foreach (string billNo in _ProductScheduling.WorkOrderList)
                            {
                                SetWorkProcessInfo(billNo);
                            }
                            sql = "SELECT BILLNO FROM PURQUALITYCHECK WHERE CURRENTSTATE = 2";
                            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                            {
                                while (reader.Read())
                                {
                                    _ProductScheduling.PurCheckOrderList.Add(reader.GetString(0));
                                }
                            }
                            foreach (string billNo in _ProductScheduling.PurCheckOrderList)
                            {
                                SetPurCheckInfo(billNo);
                            }
                            sql = "SELECT BILLNO FROM OWQUALITYCHECK WHERE CURRENTSTATE = 2";
                            using (IDataReader reader = dataAccess.ExecuteDataReader(sql))
                            {
                                while (reader.Read())
                                {
                                    _ProductScheduling.OutWareCheckOrderList.Add(reader.GetString(0));
                                }
                            }
                            foreach (string billNo in _ProductScheduling.PurCheckOrderList)
                            {
                                SetOutWareCheckInfo(billNo);
                            }
                        }
                    }
                }
                return _Default;
            }
        }

        private static Boolean SetWorkProcessInfo(string billNo)
        {
            Boolean ret = false;
            ProduceData produceData = LibProduceCache.Default.GetProduceData(billNo);
            if (produceData != null)
            {
                foreach (DataRow curRow in produceData.WorkOrder.Tables[3].Rows)
                {
                    ret = true;
                    DataRow parentRow = produceData.WorkOrder.Tables[2].Rows.Find(new object[] { curRow["BILLNO"], curRow["PARENTROWID"] });
                    if (!LibSysUtils.ToBoolean(parentRow["NEEDGATHER"]))
                        continue;
                    string workstationId = LibSysUtils.ToString(curRow["WORKSTATIONID"]);
                    if (!_ProductScheduling.WsRelWorkOrder.ContainsKey(workstationId))
                        _ProductScheduling.WsRelWorkOrder.Add(workstationId, new List<string>() { billNo });
                    if (!_ProductScheduling.WsRelWorkOrder[workstationId].Contains(billNo))
                        _ProductScheduling.WsRelWorkOrder[workstationId].Add(billNo);
                    if (produceData.FirstWorkProcessNo.Contains(LibSysUtils.ToInt32(parentRow["WORKPROCESSNO"])) && !_ProductScheduling.FirstWs.Contains(workstationId))
                        _ProductScheduling.FirstWs.Add(workstationId);
                }
            }
            return ret;
        }
        private static Boolean SetPurCheckInfo(string billNo)
        {
            Boolean ret = false;
            PurCheckData produceData = LibPurCheckCache.Default.GetPurCheckData(billNo);
            if (produceData != null)
            {
                ret = true;
            }
            return ret;
        }
        private static Boolean SetOutWareCheckInfo(string billNo)
        {
            Boolean ret = false;
            OutWareCheckData produceData = LibOutWareCheckCache.Default.GetOutWareCheckData(billNo);
            if (produceData != null)
            {
                ret = true;
            }
            return ret;
        }

        private static void ClearWorkProcessInfo(string billNo)
        {
            foreach (var item in _ProductScheduling.WsRelWorkOrder)
            {
                IList<string> billNoList = item.Value;
                for (int i = billNoList.Count - 1; i >= 0; i--)
                {
                    if (string.Compare(billNoList[i], billNo, true) == 0)
                    {
                        item.Value.RemoveAt(i);
                        if (_ProductScheduling.FirstWs.Contains(item.Key))
                            _ProductScheduling.FirstWs.Remove(item.Key);
                    }
                }
            }
        }

        public ProductScheduling GetProductScheduling()
        {
            lock (_LockProductScheduling)
            {
                return _ProductScheduling;
            }
        }

        public void AddWorkOrder(string billNo)
        {
            lock (_LockProductScheduling)
            {
                if (!_ProductScheduling.WorkOrderList.Contains(billNo))
                {
                    if (SetWorkProcessInfo(billNo))
                        _ProductScheduling.WorkOrderList.Add(billNo);
                }
            }
        }
        public void AddPurCheckOrder(string billNo)
        {
            lock (_LockProductScheduling)
            {
                if (!_ProductScheduling.PurCheckOrderList.Contains(billNo))
                {
                    if(SetPurCheckInfo(billNo))
                    _ProductScheduling.PurCheckOrderList.Add(billNo);
                    
                }
            }
        }
        public void AddOutWareCheckOrder(string billNo)
        {
            lock (_LockProductScheduling)
            {
                if (!_ProductScheduling.OutWareCheckOrderList.Contains(billNo))
                {
                    if (SetOutWareCheckInfo(billNo))
                    _ProductScheduling.OutWareCheckOrderList.Add(billNo);
                }
            }
        }

        public void RemoveWorkOrder(string billNo)
        {
            lock (_LockProductScheduling)
            {
                if (_ProductScheduling.WorkOrderList.Contains(billNo))
                {
                    _ProductScheduling.WorkOrderList.Remove(billNo);
                    ClearWorkProcessInfo(billNo);
                }
            }
        }
        public void RemovePurCheck(string billNo)
        {
            lock (_LockProductScheduling)
            {
                if (_ProductScheduling.PurCheckOrderList.Contains(billNo))
                {
                    _ProductScheduling.PurCheckOrderList.Remove(billNo);
                }
            }
        }
        public void RemoveOutWareCheck(string billNo)
        {
            lock (_LockProductScheduling)
            {
                if (_ProductScheduling.OutWareCheckOrderList.Contains(billNo))
                {
                    _ProductScheduling.OutWareCheckOrderList.Remove(billNo);
                }
            }
        }

    }

    public class ProductScheduling
    {
        private HashSet<string> _FirstWs;

        public HashSet<string> FirstWs
        {
            get
            {
                if (_FirstWs == null)
                    _FirstWs = new HashSet<string>();
                return _FirstWs;
            }
        }

        private Dictionary<string, IList<string>> _WsRelWorkOrder;

        public Dictionary<string, IList<string>> WsRelWorkOrder
        {
            get
            {
                if (_WsRelWorkOrder == null)
                    _WsRelWorkOrder = new Dictionary<string, IList<string>>();
                return _WsRelWorkOrder;
            }
        }

        private IList<string> _WorkOrderList;

        public IList<string> WorkOrderList
        {
            get
            {
                if (_WorkOrderList == null)
                    _WorkOrderList = new List<string>();
                return _WorkOrderList;
            }
        }

        private IList<string> _PurCheckOrderList;

        public IList<string> PurCheckOrderList
        {
            get
            {
                if (_PurCheckOrderList == null)
                {
                    _PurCheckOrderList = new List<string>();

                }
                return _PurCheckOrderList;
            }

        }

        private IList<string> _OutWareCheckOrderList;

        public IList<string> OutWareCheckOrderList
        {
            get
            {
                if (_OutWareCheckOrderList == null)
                {
                    _OutWareCheckOrderList = new List<string>();

                }
                return _OutWareCheckOrderList;
            }
        }
    }
}
