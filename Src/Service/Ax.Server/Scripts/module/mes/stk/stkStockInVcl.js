stkStockInVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = stkStockInVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = stkStockInVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.dataInfo && e.dataInfo.tableIndex >= 1) {
        if (e.libEventType == LibEventTypeEnum.AddRow) {
            if (e.dataInfo.tableIndex == 1) {
                var masterRow = this.dataSet.getTable(0).data.items[0];
                this.forms[0].updateRecord(masterRow);
                var warehouseId = masterRow.get('WAREHOUSEID');
                var warehouseName = masterRow.get('WAREHOUSENAME');
                var stockstateId = masterRow.get('STOCKSTATEID');
                var stockstateName = masterRow.get('STOCKSTATENAME');
                var workNo = masterRow.get('WORKNO');
                e.dataInfo.dataRow.set("WAREHOUSEID", warehouseId);
                e.dataInfo.dataRow.set("WAREHOUSENAME", warehouseName);
                e.dataInfo.dataRow.set("STOCKSTATEID", stockstateId);
                e.dataInfo.dataRow.set("STOCKSTATENAME", stockstateName);
                e.dataInfo.dataRow.set("WORKNO", workNo);
            }
        }
    }
};