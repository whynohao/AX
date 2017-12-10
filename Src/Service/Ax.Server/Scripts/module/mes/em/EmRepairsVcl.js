EmRepairsVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = EmRepairsVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = EmRepairsVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var fieldName = e.dataInfo.fieldName;
            var mastrow = this.dataSet.getTable(0).data.items[0];
            if (e.dataInfo.tableIndex == 0) {
                if (fieldName == "EQUIPMENTID") {
                    Ext.getCmp('EMFAULTID0_' + this.winId).setValue("");
                    if (e.dataInfo.dataRow.data["EQUIPMENTID"] == null) {
                        Ext.getCmp('OPERATIONID0_' + this.winId).setValue("");
                    }
                }
            }
            this.forms[0].updateRecord(mastrow);
            break;
    }
}