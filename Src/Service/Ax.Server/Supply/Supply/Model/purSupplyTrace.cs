using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ax.Server.Supply.Model
{
    [Serializable]
    public class purSupplyTrace
    {
        string _purBillNo = string.Empty;
        /// <summary>
        /// 采购单号
        /// </summary>
        public string PurBillNo
        {
            get { return _purBillNo; }
            set { _purBillNo = value; }
        }
        string _personId = string.Empty;
        /// <summary>
        /// 采购人员编码
        /// </summary>
        public string PersonId
        {
            get { return _personId; }
            set { _personId = value; }
        }
        string _personName = string.Empty;
        /// <summary>
        /// 采购人员名称
        /// </summary>
        public string PersonName
        {
            get { return _personName; }
            set { _personName = value; }
        }
        string _supplierId = string.Empty;
        /// <summary>
        /// 供应商编码
        /// </summary>
        public string SupplierId
        {
            get { return _supplierId; }
            set { _supplierId = value; }
        }
        string _supplierName = string.Empty;
        /// <summary>
        /// 供应商名称
        /// </summary>
        public string SupplierName
        {
            get { return _supplierName; }
            set { _supplierName = value; }
        }

        string _supplyUserId = string.Empty;
        /// <summary>
        /// 供应商人员编码
        /// </summary>
        public string SupplyUserId
        {
            get { return _supplyUserId; }
            set { _supplyUserId = value; }
        }
        string _supplyUserName = string.Empty;
        /// <summary>
        /// 供应商人员名称
        /// </summary>
        public string SupplyUserName
        {
            get { return _supplyUserName; }
            set { _supplyUserName = value; }
        }
        string _supplyUserTel = string.Empty;
        /// <summary>
        /// 供应商人员电话
        /// </summary>
        public string SupplyUserTel
        {
            get { return _supplyUserTel; }
            set { _supplyUserTel = value; }
        }
        string _materialId = string.Empty;
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialId
        {
            get { return _materialId; }
            set { _materialId = value; }
        }
        string _materialName = string.Empty;
        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName
        {
            get { return _materialName; }
            set { _materialName = value; }
        }
        int _quantity = 0;
        /// <summary>
        /// 订单数量
        /// </summary>
        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }
        long _deliveryTime = 0;
        /// <summary>
        /// 要求送达时间
        /// </summary>
        public long DeliveryTime
        {
            get { return _deliveryTime; }
            set { _deliveryTime = value; }
        }
        int _cycleTime = 0;
        /// <summary>
        /// 生产周期
        /// </summary>
        public int CycleTime
        {
            get { return _cycleTime; }
            set { _cycleTime = value; }
        }
        long _planStartTime = 0;
        /// <summary>
        /// 计划生产开始时间
        /// </summary>
        public long PlanStartTime
        {
            get { return _planStartTime; }
            set { _planStartTime = value; }
        }
        long _realStartTime = 0;
        /// <summary>
        /// 实际生产开始时间
        /// </summary>
        public long RealStartTime
        {
            get { return _realStartTime; }
            set { _realStartTime = value; }
        }
        long _planEndTime = 0;
        /// <summary>
        /// 计划生产结束时间
        /// </summary>
        public long PlanEndTime
        {
            get { return _planEndTime; }
            set { _planEndTime = value; }
        }
        long _realEndTime = 0;
        /// <summary>
        /// 实际生产结束时间
        /// </summary>
        public long RealEndTime
        {
            get { return _realEndTime; }
            set { _realEndTime = value; }
        }
        long _planInWareTime = 0;
        /// <summary>
        /// 计划到货时间
        /// </summary>
        public long PlanInWareTime
        {
            get { return _planInWareTime; }
            set { _planInWareTime = value; }
        }
        long _realInWareTime = 0;
        /// <summary>
        /// 实际到货时间
        /// </summary>
        public long RealInWareTime
        {
            get { return _realInWareTime; }
            set { _realInWareTime = value; }
        }
        long _planSendTime = 0;
        /// <summary>
        /// 计划装车时间
        /// </summary>
        public long PlanSendTime
        {
            get { return _planSendTime; }
            set { _planSendTime = value; }
        }
        long _realSendTime = 0;
        /// <summary>
        /// 实际装车时间
        /// </summary>
        public long RealSendTime
        {
            get { return _realSendTime; }
            set { _realSendTime = value; }
        }
        int _stockQty = 0;
        /// <summary>
        /// 库存数量
        /// </summary>
        public int StockQty
        {
            get { return _stockQty; }
            set { _stockQty = value; }
        }
        int _inWareQty = 0;
        /// <summary>
        /// 在途数量
        /// </summary>
        public int InWareQty
        {
            get { return _inWareQty; }
            set { _inWareQty = value; }
        }
    }
}