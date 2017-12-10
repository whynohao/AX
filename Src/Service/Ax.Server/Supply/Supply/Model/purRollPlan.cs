using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ax.Server.Supply.Model
{
    [Serializable]
    public class purRollPlan
    {
       
        string billNo = string.Empty;
        /// <summary>
        /// 订单编号
        /// </summary>
        public string BillNo
        {
            get { return billNo; }
            set { billNo = value; }
        }
        int row_Id = 0;
        /// <summary>
        /// 订单行标识
        /// </summary>
        public int Row_Id
        {
            get { return row_Id; }
            set { row_Id = value; }
        }
        string purBillNo = string.Empty;
        /// <summary>
        /// 采购订单号
        /// </summary>
        public string PurBillNo
        {
            get { return purBillNo; }
            set { purBillNo = value; }
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
        
        string _deliveryNoteNo = string.Empty;
        /// <summary>
        /// 发货单号
        /// </summary>
        public string DeliveryNoteNo
        {
            get { return _deliveryNoteNo; }
            set { _deliveryNoteNo = value; }
        }
       
        string _barcode = string.Empty;
        /// <summary>
        /// 批次号
        /// </summary>
        public string Barcode
        {
            get { return _barcode; }
            set { _barcode = value; }
        }
       
        string _purPersonId = string.Empty;
        /// <summary>
        /// 采购员编码
        /// </summary>
        public string PurPersonId
        {
            get { return _purPersonId; }
            set { _purPersonId = value; }
        }
        
        string _purPersonName = string.Empty;
        /// <summary>
        /// 采购员名称
        /// </summary>
        public string PurPersonName
        {
            get { return _purPersonName; }
            set { _purPersonName = value; }
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
        
        string _workId = string.Empty;
        /// <summary>
        /// 作业号
        /// </summary>
        public string WorkId
        {
            get { return _workId; }
            set { _workId = value; }
        }
        
        long _arriveDate = 0;
        /// <summary>
        /// 确认到货时间
        /// </summary>
        public long ArriveDate
        {
            get { return _arriveDate; }
            set { _arriveDate = value; }
        }
        
        int _arriveQuantity = 0;
        /// <summary>
        /// 确认到货数量
        /// </summary>
        public int ArriveQuantity
        {
            get { return _arriveQuantity; }
            set { _arriveQuantity = value; }
        }
        
        List<int> _planDate = null;
        /// <summary>
        /// 计划到货时间
        /// </summary>
        public List<int> PlanDate
        {
            get { return _planDate; }
            set { _planDate = value; }
        }
    }
}