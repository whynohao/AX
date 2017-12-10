comChangeRequestVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var This;
var proto = comChangeRequestVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comChangeRequestVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "TYPEID") {
                    var headTableRow = this.dataSet.getTable(0).data.items[0];
                    var bodyTable = this.dataSet.getTable(1);
                    var returnData = this.invorkBcf("GetChangeRequestData", [headTableRow.data["TYPEID"]]);
                    if (e.dataInfo.value == null) {
                        bodyTable.removeAll();
                    }
                    else {
                        if (returnData.length == 0) {
                            Ext.Msg.alert("提示", "变更申请单类型明细为空！");
                            return;
                        }
                        else {
                            fillChangeRequest(this, returnData);
                        }
                    }
                }
            }
            break;
    }
}
function fillChangeRequest(This, returnData) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        This.deleteAll(1);
        var masterRow = This.dataSet.getTable(0).data.items[0];
        if (returnData !== undefined && returnData.length > 0) {
            for (var i = 0; i < returnData.length; i++) {
                var info = returnData[i];
                var newRow = This.addRow(masterRow, 1);
                newRow.set('ROW_ID', i + 1);
                newRow.set('ROWNO', i + 1);
                newRow.set('DEPTID', info.DeptId + "," + info.DeptName);
                newRow.set('PERSONID', info.PersonId + "," + info.PersonName);
            }
        }
    }
    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}