plsMasterPlan1Vcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = plsMasterPlan1Vcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = plsMasterPlan1Vcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            e.dataInfo.cancel = true;
            break;


        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                var headTable = this.dataSet.getTable(0);
                if (e.dataInfo.fieldName == "FROMBILLNO") {
                    var headTableRow = this.dataSet.getTable(0).data.items[0];
                    var bodyTable = this.dataSet.getTable(1);
                    if (e.dataInfo.value == null) {
                        bodyTable.removeAll();
                    }
                    else {
                        var returnData = this.invorkBcf("GetProductData", [headTableRow.data["FROMBILLNO"]]);
                        if (returnData.length == 0) {
                            Ext.Msg.alert("提示", "投产单为空！");
                            return;
                        }
                        fillMasterPlan(this, returnData);
                    }
                }
                if (e.dataInfo.fieldName == "FINALDATE") {
                    var headTableRow = this.dataSet.getTable(0).data.items[0];
                    var bodyTable = this.dataSet.getTable(1);
                    if (e.dataInfo.value < headTableRow.data["PARTSDATE"])
                    {
                        Ext.Msg.alert("提示", "总装日期要大于部件日期！");
                        headTableRow.set("FINALDATE", "");
                    }
                    else
                    {
                        for (var i = 0; i < bodyTable.data.length; i++) {
                            if (bodyTable.data.items[i].data["FINALDATE"] == 0)
                                bodyTable.data.items[i].set("FINALDATE", headTableRow.data["FINALDATE"]);
                        }
                    }
                }
                if (e.dataInfo.fieldName == "PARTSDATE") {
                    var headTableRow = this.dataSet.getTable(0).data.items[0];
                    var bodyTable = this.dataSet.getTable(1);
                    if (e.dataInfo.value > headTableRow.data["FINALDATE"]) {
                        Ext.Msg.alert("提示", "部件日期要小于总装日期！");
                        headTableRow.set("PARTSDATE", "");
                    }
                    else if (e.dataInfo.value < headTableRow.data["PARTDATE"]) {
                        Ext.Msg.alert("提示", "部件日期要大于零件日期！");
                        headTableRow.set("PARTSDATE", "");
                    }
                    else {
                        for (var i = 0; i < bodyTable.data.length; i++) {
                            if (bodyTable.data.items[i].data["PARTSDATE"] == 0)
                                bodyTable.data.items[i].set("PARTSDATE", headTableRow.data["PARTSDATE"]);
                        }
                    }
                }
                if (e.dataInfo.fieldName == "PARTDATE") {
                    var headTableRow = this.dataSet.getTable(0).data.items[0];
                    var bodyTable = this.dataSet.getTable(1);
                    if (e.dataInfo.value > headTableRow.data["PARTSDATE"]) {
                        Ext.Msg.alert("提示", "零件日期要小于部件日期！");
                        headTableRow.set("PARTDATE", "");
                    }
                    else if (e.dataInfo.value < headTableRow.data["MATERIALDATE"]) {
                        Ext.Msg.alert("提示", "零件日期要大于原料日期！");
                        headTableRow.set("PARTDATE", "");
                    }
                    else {
                        for (var i = 0; i < bodyTable.data.length; i++) {
                            if (bodyTable.data.items[i].data["PARTDATE"] == 0)
                                bodyTable.data.items[i].set("PARTDATE", headTableRow.data["PARTDATE"]);
                        }
                    }
                }
                if (e.dataInfo.fieldName == "MATERIALDATE") {
                    var headTableRow = this.dataSet.getTable(0).data.items[0];
                    var bodyTable = this.dataSet.getTable(1);
                    if (e.dataInfo.value > headTableRow.data["PARTDATE"]) {
                        Ext.Msg.alert("提示", "原料日期要小于零件日期！");
                        headTableRow.set("MATERIALDATE", "");
                    }
                    else {
                        for (var i = 0; i < bodyTable.data.length; i++) {
                            if (bodyTable.data.items[i].data["MATERIALDATE"] == 0)
                                bodyTable.data.items[i].set("MATERIALDATE", headTableRow.data["MATERIALDATE"]);
                        }
                    }
                }
                this.forms[0].loadRecord(headTable.data.items[0]);
            }
            break;
    }
}
function fillMasterPlan(This, returnData) {
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
                newRow.set('FROMBILLNO', info.FromBillNo);
                newRow.set('CONTRACTNO', info.ContractNo);
                newRow.set('FROMROWID', info.FromRowId);
                newRow.set('FACTORYNO', info.FactoryNo);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('FIGURENO', info.FigureNo);
                newRow.set('TEXTUREID', info.Textureid);
                newRow.set('SPECIFICATION', info.SpecIfication);
                newRow.set('QTY', info.Quantity);
                newRow.set('MATSTYLE', info.Matstyle);
                newRow.set('BOMID', info.BomId);
                newRow.set('FINALDATE', masterRow.data["FINALDATE"]);
                newRow.set('PARTSDATE', masterRow.data["PARTSDATE"]);
                newRow.set('PARTDATE', masterRow.data["PARTDATE"]);
                newRow.set('MATERIALDATE', masterRow.data["MATERIALDATE"]);

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

