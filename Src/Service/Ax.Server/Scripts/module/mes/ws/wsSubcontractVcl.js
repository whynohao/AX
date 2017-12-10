wsSubcontractVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = wsSubcontractVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = wsSubcontractVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var mastertable = this.dataSet.getTable(0);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (!this.isEdit) {
            if (e.dataInfo.fieldName == "BtnIn") {
                var record = mastertable.data.items[0];
                var list=[];
                var contractId = record.get("CONTRACTID");
                var currentState = record.get("CURRENTSTATE");
                if (currentState != "2") {
                    alert("单据未生效！");
                    return;
                }
                var items = this.dataSet.getTable(1).data.items;
                for (var i = 0; i < items.length; i++) {
                    var MaterialInfo = {};
                    MaterialInfo.BillNo = items[i].get("BILLNO");
                    MaterialInfo.RowId = items[i].get("ROW_ID");
                    MaterialInfo.MaterialId = items[i].get("MATERIALID");
                    MaterialInfo.Price = items[i].get("PRICE");
                    MaterialInfo.Quantity = items[i].get("QUANTITY");
                    MaterialInfo.UnitId = items[i].get("STKUNITID");
                    MaterialInfo.BatchNo = items[i].get("BATCHNO");
                    MaterialInfo.SubBatchNo = items[i].get("SUBBATCHNO");
                    MaterialInfo.CompleteNo = items[i].get("COMPLETENO");
                    MaterialInfo.MtoNo = items[i].get("MTONO");
                    MaterialInfo.Remark = items[i].get("REMARK");
                    list.push(MaterialInfo);
                }
                if (list.length > 0) {
                    this.invorkBcf('CreatePurIn', [contractId, list]);
                }
            }
            else if (e.dataInfo.fieldName == "BtnOut") {
                var record = mastertable.data.items[0];
                var list = [];
                var contractId = record.get("CONTRACTID");
                var currentState = record.get("CURRENTSTATE");
                var isFinish = record.get("ISFINISH");
                if (currentState != "2") {
                    alert("单据未生效！");
                    return;
                }
                if (currentState != "1") {
                    alert("单据未入库！");
                    return;
                }

                var items = this.dataSet.getTable(1).data.items;
                for (var i = 0; i < items.length; i++) {
                    var MaterialInfo = {};
                    MaterialInfo.BillNo = items[i].get("BILLNO");
                    MaterialInfo.RowId = items[i].get("ROW_ID");
                    MaterialInfo.MaterialId = items[i].get("MATERIALID");
                    MaterialInfo.Price = items[i].get("PRICE");
                    MaterialInfo.Quantity = items[i].get("QUANTITY");
                    MaterialInfo.UnitId = items[i].get("STKUNITID");
                    MaterialInfo.BatchNo = items[i].get("BATCHNO");
                    MaterialInfo.SubBatchNo = items[i].get("SUBBATCHNO");
                    MaterialInfo.CompleteNo = items[i].get("COMPLETENO");
                    MaterialInfo.MtoNo = items[i].get("MTONO");
                    MaterialInfo.Remark = items[i].get("REMARK");
                    list.push(MaterialInfo);
                }
                if (list.length > 0) {
                    this.invorkBcf('CreateOtherOut', [contractId, list]);
                }
            }
        }
        else { alert("编辑状态下不可操作！"); }
    }
}



