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
    public class LibHYControlServer
    {
        private static LibHYControlServer _Default = null;
        private static object _LockObj = new object();
        private static HYProductScheduling _ProductScheduling = null;
        private static object _LockProductScheduling = new object();

        private LibHYControlServer()
        {

        }
        public static LibHYControlServer Default
        {
            get
            {
                if (_Default == null)
                {
                    lock (_LockObj)
                    {
                        if (_Default == null)
                        {
                            _Default = new LibHYControlServer();
                            _ProductScheduling = new HYProductScheduling();
                            string sql = "SELECT DISTINCT WORKORDERNO FROM PPMAINTENWORKRECORD WHERE STARTSTATE=1";
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
                        }
                    }
                }
                return _Default;
            }
        }

        private static void SetWorkProcessInfo(string billNo)
        {
            HYProduceData produceData = LibHYProduceCache.Default.GetProduceData(billNo);
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

        public HYProductScheduling GetProductScheduling()
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
                    _ProductScheduling.WorkOrderList.Add(billNo);
                    SetWorkProcessInfo(billNo);
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

    }

    public class HYProductScheduling
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

        
    }
}
