qcOwQualityCheckDataFuncVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}

var proto = qcOwQualityCheckDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = qcOwQualityCheckDataFuncVcl;
proto.winId = null;
proto.fromObj = null;
proto.getType = 0;
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj[0];
};

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        var returnData = this.invorkBcf("GetData");
        if (e.dataInfo.fieldName == "Select") {            //点击查询按钮触发
            this.fillData.call(this, returnData);
        }
        if (e.dataInfo.fieldName == "Load") {            //点击载入按钮触发
            var grid = Ext.getCmp(this.winId + 'OWQUALITYCHECKDATAFUNCDETAILGrid');
            var records = grid.getView().getSelectionModel().getSelection();
            if (records.length <= 0) {
                alert("请选择载入的明细！");
            }
            else {
                fillGetnoticeReturnData.call(this, records, returnData);
                this.win.close();
            }
        }
    }
}

proto.fillData = function(returnData) {             //载入查询的所有数据，载入到datafunc上
    //Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    var table = this.dataSet.getTable(2);
    //curStore.suspendEvents();//关闭store事件
    //table.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'OWQUALITYCHECKDATAFUNCDETAILGrid');
        var list = returnData['stockoutdetail'];
        var masterRow = this.dataSet.getTable(0).data.items[0];
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set('BILLNO', masterRow["BILLNO"]);
                newRow.set('ROW_ID', this.dataSet.dataList[1].MaxRowId);
                newRow.set('FROMBILLNO', info.FromBillNo);
                newRow.set('FROMROWID', info.FromRowId);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('ATTRIBUTECODE', info.AttributeCode);
                newRow.set('ATTRIBUTEDESC', info.AttributeDesc);
                newRow.set('WORKSTATIONCONFIGID', info.WorkStationConfigId);
                newRow.set('QUANTITY', info.Quantity);
                newRow.set('CHECKNUM', info.Quantity);
                newRow.set('ISUSECHECK', info.IsUseCheck);
                newRow.set('ISCONFIG', info.IsConfig);
                newRow.set('BATCHNO', info.BatchNo);
                newRow.set('SUBBATCHNO',info.SubBatchNo);
                for (var j = 0; j < info.Dic.length; j++) {
                    if (info.Dic[j].CheckStId != "") {
                        this.deleteAll(2);
                        var subRow = this.addRow(newRow, 2);
                        subRow.set('CHECKSTID', info.Dic[j].CheckStId);
                        subRow.set('CHECKSTNAME', info.Dic[j].CheckStName);
                        newRow.set('CHECKSTDETAIL', true);
                    }
                }
            }
        }
    }
    finally {
        //curStore.resumeEvents();//打开store事件
        //table.resumeEvents();//打开store事件
        //if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
        //    curStore.ownGrid.reconfigure(curStore);
        //if (table.ownGrid && table.ownGrid.getView().store != null)
        //    table.ownGrid.reconfigure(table);
        //Ext.resumeLayouts(true);//打开Ext布局
    }
}

function fillGetnoticeReturnData(records, returnData) {
    var grid = Ext.getCmp(proto.winId + 'OWQUALITYCHECKDETAILGrid');    //数据进入出库单明细
    //Ext.suspendLayouts();
    var fromStore = proto.fromObj.dataSet.getTable(1);
    var table = proto.fromObj.dataSet.getTable(2);
    //fromStore.suspendEvents();//关闭store事件
    //table.suspendEvents();//关闭store事件
    try {
        if (records !== undefined && records.length > 0) {
            for (var i = 0; i < records.length; i++) {
                var info = records[i];
                if (!checkGetNotice(grid, info)) {
                    var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
                    var newRow = proto.fromObj.addRow(masterRow, 1);
                    //newRow.set('BILLNO', masterRow.data["BILLNO"]);
                    //newRow.set('ROW_ID', proto.fromObj.dataSet.dataList[1].MaxRowId);
                    newRow.set('FROMBILLNO', info.data["FROMBILLNO"])
                    newRow.set('FROMROWID', info.data["FROMROWID"]);
                    newRow.set('MATERIALID', info.data["MATERIALID"]);
                    newRow.set('MATERIALNAME', info.data["MATERIALNAME"]);
                    newRow.set('ATTRIBUTECODE', info.data["ATTRIBUTECODE"]);
                    newRow.set('ATTRIBUTEDESC', info.data["ATTRIBUTEDESC"]);
                    newRow.set('WORKSTATIONCONFIGID', info.data["WORKSTATIONCONFIGID"]);
                    newRow.set('QUANTITY', info.data["QUANTITY"]);
                    newRow.set('CHECKNUM', info.data["QUANTITY"]);
                    newRow.set('ISUSECHECK', info.data["ISUSECHECK"]);
                    newRow.set('ISCONFIG', info.data["ISCONFIG"]);
                    newRow.set('BATCHNO', info.data["BATCHNO"]);
                    newRow.set('SUBBATCHNO', info.data["SUBBATCHNO"]);
                    var list = returnData['stockoutdetail'];
                    for (var j = 0; j < list.length; j++) {
                        if (info.data["FROMBILLNO"] == list[j].FromBillNo && info.data["FROMROWID"] == list[j].FromRowId) {
                            for (var k = 0; k < list[j].Dic.length; k++) {
                                if (list[j].Dic[k].CheckStId != "") {
                                    var subRow = proto.fromObj.addRow(newRow, 2);
                                    subRow.set('CHECKSTID', list[j].Dic[k].CheckStId);
                                    subRow.set('CHECKSTNAME', list[j].Dic[k].CheckStName);
                                    newRow.set('CHECKSTDETAIL', true);
                                }
                            }
                        }
                    }
                }
            }
        }
    } finally {
        //fromStore.resumeEvents();
        //if (fromStore.ownGrid && fromStore.ownGrid.getView().store != null)
        //    fromStore.ownGrid.reconfigure(fromStore);
        //table.resumeEvents();
        //if (table.ownGrid && table.ownGrid.getView().store != null)
        //    table.ownGrid.reconfigure(table);
        //Ext.resumeLayouts(true);
    }
}

function checkGetNotice(grid, info) {           //判断出库明细表是否已经存在当前填写过去的明细项
    var k = 0;
    var main = proto.fromObj.dataSet.getTable(1);
    for (var j = 0; j < main.data.items.length; j++) {
        if (main.data.items[j].get('FROMBILLNO') == info.data["FROMBILLNO"] && main.data.items[j].get('FROMROWID') == info.data["FROMROWID"]) {
            k = 1;
        }
    }
    if (k == 1) {
        return true;
    }
    else {
        return false
    }

}
