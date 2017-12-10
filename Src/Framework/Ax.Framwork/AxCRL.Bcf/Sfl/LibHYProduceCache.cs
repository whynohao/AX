using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using AxCRL.Comm.Utils;
using AxCRL.Core.Comm;
using AxCRL.Comm.Redis;

namespace AxCRL.Bcf.Sfl
{
    public class LibHYProduceCache : MemoryCacheRedis
    {
        private static LibHYProduceCache _default;
        private static readonly object LockObj = new object();
        private static ConcurrentDictionary<string, object> _lockObjDic;

        public LibHYProduceCache(string name, NameValueCollection config = null)
            : base(name)
        {
        }

        public static  LibHYProduceCache Default
        {
            get
            {
                if (_default == null)
                {
                    lock (LockObj)
                    {
                        if (_default == null)
                        {
                            _default = new LibHYProduceCache("LibHYProduceCache");
                            _lockObjDic = new ConcurrentDictionary<string, object>();
                        }
                    }
                }
                return _default;
            }
        }

        public override bool Remove(string billNo, string regionName = null)
        { 
            return base.Remove(billNo);
        }


        public HYProduceData GetProduceData(string billNo)
        {
            HYProduceData produceData = null;
            if (!string.IsNullOrEmpty(billNo))
                produceData = this.Get< HYProduceData>(billNo)   ;
            if (produceData == null)
            {
                object lockItem = _lockObjDic.GetOrAdd(billNo, new object());
                lock (lockItem)
                {
                    produceData = this.Get< HYProduceData>(billNo)  ;
                    if (produceData == null)
                    {
                        //CacheItemPolicy policy = new CacheItemPolicy();
                        //policy.SlidingExpiration = new TimeSpan(0, 720, 0); //720分钟内不访问自动剔除
                        LibBcfGrid ppWorkOrderBcf = (LibBcfGrid)LibBcfSystem.Default.GetBcfInstance("pp.TenWorkRecord");
                        LibQueryCondition lb = new LibQueryCondition();
                        lb.QueryFields.Add(new LibQueryField() {Name = "WORKORDERNO",QueryChar=LibQueryChar.Equal, Value = new List<object>{ billNo} });
                        DataSet ds = ppWorkOrderBcf.BrowseTo(lb);
                        //DataSet ds = ppWorkOrderBcf.BrowseTo(new object[] { billNo });
                        produceData = new HYProduceData(ds);
                        this.Set(billNo, produceData, new TimeSpan(0, 720, 0));
                    }
                }
            }
            return produceData;
        }
    }
    
    public class HYProduceData
    {
        public DataSet TenWorkRecord { get; set; }

        public HYProduceData(DataSet ds)
        {
            this.TenWorkRecord = ds;
        }
    }

    public class TenWorkRecord
    {
        //任务号

        public string TaskNo { get; set; }

        //派工单号

        public string WorkOrderNo { get; set; }


        //作业单号

        public string PlsWorkOrderNo { get; set; }


        //来源月计划单号

        public string FromBillNo { get; set; }


        //来源月计划行标识

        public int FromRowId { get; set; }

        //关联条码

        public string LinkBarcode { get; set; }

        //主条码

        public string Barcode { get; set; }

        //物料类别

        public string MaterialType { get; set; }

        //物料编号

        public string MaterialId { get; set; }

        //物料名称

        public string MaterialName { get; set; }

        //单位数量

        public int UnitNum { get; set; }

        //条码规则编号

        public string BarcodeRuleId { get; set; }

        //主要特征

        public string MainAttributeDesc { get; set; }


        //特征标识

        public string AttributeId { get; set; }


        //特征描述

        public string AttributeDesc { get; set; }

        //工序编号

        public string WorkProcessId { get; set; }

        //工序号

        public int WorkProcessNo { get; set; }

        //转入工序号

        public int NextWorkProcessNo { get; set; }

        //生产线

        public string ProduceLineId { get; set; }

        //生产线名称

        public string ProduceLineName { get; set; }

        public int States { get; set; }

        //协同状态

        public int CooprateState { get; set; }

        //工作站点

        public string WorkstationId { get; set; }

        //十单顺序号

        public int OrderNum { get; set; }

        //开始时间

        public Int64 StartTime { get; set; }

        //完成时间

        public Int64 FinishTime { get; set; }

        //缺陷标识

        public int DefectState { get; set; }

        //订单状态

        public int SaleOrderState { get; set; }

        //十单开始

        public int StartState { get; set; }

        //扫码顺序号

        public int ScanNum { get; set; }

        //库位编号

        public string StorageId { get; set; }

        //部件物料编码

        public string SubMaterialId { get; set; }

        //部件物料名称

        public string SubMaterialName { get; set; }

        //人员编号

        public string PersonId { get; set; }

        //人员名称

        public string PersonName { get; set; }

        //十单日期

        public int OrderDate { get; set; }

        //大类

        public string ProductType { get; set; }

        //生产单号

        public string LotNo { get; set; }

        //组号

        public string GroupNo { get; set; }

        //客户名称

        public string CustomerName { get; set; }

        //产品尺寸

        public string ProductSize { get; set; }

        //树种

        public string TreeType { get; set; }

        //油漆颜色

        public string Color { get; set; }

        //位置

        public string Location { get; set; }

        //下单时间

        public string Time { get; set; }

        //合计

        public string Total { get; set; }

        //订单编号

        public string SaleBillNo { get; set; }

        //模板编号

        public string LabelTemplateId { get; set; }

        //订单序号

        public int OrderIndex { get; set; }

        //订单数量

        public int OrderQuantity { get; set; }

        //备注信息

        public string Remark { get; set; }

        //报工数量

        public int FinishNum { get; set; }

        //来源类型

        public int FromType { get; set; }

        //来源销售订单

        public string FromSaleBillNo { get; set; }

        public TenWorkRecord()
        {
            FromSaleBillNo = string.Empty;
            FromType = 0;
            FinishNum = 0;
            Remark = string.Empty;
            OrderQuantity = 0;
            OrderIndex = 0;
            LabelTemplateId = string.Empty;
            SaleBillNo = string.Empty;
            Total = string.Empty;
            Time = string.Empty;
            Location = string.Empty;
            Color = string.Empty;
            TreeType = string.Empty;
            ProductSize = string.Empty;
            CustomerName = string.Empty;
            GroupNo = string.Empty;
            LotNo = string.Empty;
            ProductType = string.Empty;
            OrderDate = 0;
            PersonName = string.Empty;
            PersonId = string.Empty;
            SubMaterialName = string.Empty;
            SubMaterialId = string.Empty;
            StorageId = string.Empty;
            ScanNum = 0;
            StartState = 0;
            SaleOrderState = 0;
            DefectState = 0;
            FinishTime = 0;
            StartTime = 0;
            OrderNum = 0;
            WorkstationId = string.Empty;
            CooprateState = 0;
            States = 0;
            ProduceLineName = string.Empty;
            ProduceLineId = string.Empty;
            NextWorkProcessNo = 0;
            WorkProcessNo = 0;
            WorkProcessId = string.Empty;
            AttributeDesc = string.Empty;
            AttributeId = string.Empty;
            MainAttributeDesc = string.Empty;
            BarcodeRuleId = string.Empty;
            UnitNum = 0;
            MaterialName = string.Empty;
            MaterialId = string.Empty;
            MaterialType = string.Empty;
            Barcode = string.Empty;
            LinkBarcode = string.Empty;
            FromRowId = 0;
            FromBillNo = string.Empty;
            PlsWorkOrderNo = string.Empty;
            WorkOrderNo = string.Empty;
            TaskNo = string.Empty;
        }

        
    }
}
