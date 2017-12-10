stkOutSourcingOutDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkOutSourcingOutDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkOutSourcingOutDataFuncVcl;
proto.winId = null;
proto.fromObj = null;

proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj;
    var mastHeadRow = proto.fromObj[0].dataSet.getTable(0).data.items[0];
    var fromBillNo = mastHeadRow.get("FROMBILLNO");
    var contractCode = mastHeadRow.get("CONTRACTCODE");
    var contractNo = mastHeadRow.get("CONTRACTNO");
    Ext.getCmp("CONTRACTCODE0_" + vcl.winId).store.add({ Id: contractCode, Name: contractNo });
    Ext.getCmp('FROMBILLNO0_' + vcl.winId).store.add({ Id: fromBillNo })
    Ext.getCmp('CONTRACTCODE0_' + vcl.winId).setValue(contractCode);
    Ext.getCmp('FROMBILLNO0_' + vcl.winId).setValue(fromBillNo);
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            var bodyTable = this.dataSet.getTable(1).data.items;
            if (e.dataInfo.fieldName == "BtnSelect") {
                var fromBillNo = this.dataSet.getTable(0).data.items[0].data['FROMBILLNO'];
                var contractCode = this.dataSet.getTable(0).data.items[0].data['CONTRACTCODE'];
                var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'];

                if (fromBillNo == '') {
                    Ext.Msg.alert("提示", "派工单号不能为空！");
                    return;
                }
                else {
                    var records = this.invorkBcf('GetData', [contractCode, fromBillNo, materialId]);
                    this.fillData(records, this);
                }
            }
            else if (e.dataInfo.fieldName == "BtnSelectAll") {
                for (var i = 0; i < bodyTable.length; i++) {
                    bodyTable[i].set("ISCHOOSE", 1);
                }
            }
            else if (e.dataInfo.fieldName == "BtnSelectNone") {
                for (var i = 0; i < bodyTable.length; i++) {
                    bodyTable[i].set("ISCHOOSE", 0);
                }
            }
            else if (e.dataInfo.fieldName == "BtnLoad") {
                this.fillReturnData(bodyTable, this);
                this.win.close();
                //var chooseRecords = [];
                //for (var i = 0; i < bodyTable.length; i++) {
                //    var row=bodyTable[i].data;
                //    if (row["ISCHOOSE"] == 1) {
                //        chooseRecords.push({

                //        });
                //    }
                //}
            }
            break;
    }
}
proto.fillData = function (returnData) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'STKOUTSOURCINGOUTDATAFUNCDETAILGrid');
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                //newRow.set("ROW_ID", i + 1);
                //newRow.set("ROWNO", i + 1)
                newRow.set("TASKNO", info.TASKNO);
                newRow.set("CONTRACTCODE", info.CONTRACTCODE);
                newRow.set("CONTRACTNO", info.CONTRACTNO);
                newRow.set("FROMBILLNO", info.FROMBILLNO);
                newRow.set("FACTORYNO", info.FACTORYNO);
                newRow.set("MATERIALID", info.MATERIALID);
                newRow.set("MATERIALNAME", info.MATERIALNAME);
                newRow.set("SPECIFICATION", info.SPECIFICATION);
                newRow.set("TEXTUREID", info.TEXTUREID);
                newRow.set("FIGURENO", info.FIGURENO);
                newRow.set("MATERIALSPEC", info.MATERIALSPEC);
                newRow.set("NEEDCHECK", info.NEEDCHECK);
                newRow.set("ATTRIBUTEID", info.ATTRIBUTEID);
                newRow.set("ATTRIBUTENAME", info.ATTRIBUTENAME);
                newRow.set("ATTRIBUTECODE", info.ATTRIBUTECODE);
                newRow.set("ATTRIBUTEDESC", info.ATTRIBUTEDESC);
                newRow.set("WORKPROCESSNO", info.WORKPROCESSNO);
                newRow.set("WORKPROCESSID", info.WORKPROCESSID);
                newRow.set("WORKPROCESSNAME", info.WORKPROCESSNAME);
                newRow.set("QUANTITY", info.QUANTITY);
                newRow.set("WAREOUTQYU", info.WAREOUTQYU);
                newRow.set("WAREINQYU", info.WAREINQYU);
                newRow.set("UNITID", info.UNITID);
                newRow.set("UNITNAME", info.UNITNAME);
            }
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}
proto.fillReturnData = function (returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = proto.fromObj[0].dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        proto.fromObj[0].deleteAll(1);
        var grid = Ext.getCmp(proto.winId + 'STKOUTSOURCINGOUTDETAILGrid');
        var table = proto.fromObj[0].dataSet.getTable(0);
        var billno = table.data.items[0].data["BILLNO"];
        var warehouseId = table.data.items[0].data["WAREHOUSEID"];
        var warehouseName = table.data.items[0].data["WAREHOUSENAME"];
        var stkstate = table.data.items[0].data["STKSTATE"];
        var stkstateName = table.data.items[0].data["STKSTATENAME"];
        var l = 0;
        for (var i = 0; i < returnList.length; i++) {
            var info = returnList[i].data;
            if (info["ISCHOOSE"] == 1) {
                var newRow = proto.fromObj[0].addRowForGrid(grid);
                newRow.set("BILLNO", billno);
                newRow.set("ROW_ID", l + 1);
                newRow.set("ROWNO", l + 1);
                newRow.set("TASKNO", info["TASKNO"]);
                newRow.set("CONTRACTCODE", info["CONTRACTCODE"]);
                newRow.set("CONTRACTNO", info["CONTRACTNO"]);
                newRow.set("WAREHOUSEID", warehouseId);
                newRow.set("WAREHOUSENAME", warehouseName);
                newRow.set("STKSTATE", stkstate);
                newRow.set("STKSTATENAME", stkstateName);
                //newRow.set("WORKPROCESSNO", info["WORKPROCESSNO"]);
                //newRow.set("WORKPROCESSID", info["WORKPROCESSID"]);
                //newRow.set("WORKPROCESSNAME", info["WORKPROCESSNAME"]);
                newRow.set("MATERIALID", info["MATERIALID"]);
                newRow.set("MATERIALNAME", info["MATERIALNAME"]);
                newRow.set("SPECIFICATION", info["SPECIFICATION"]);
                newRow.set("TEXTUREID", info["TEXTUREID"]);
                newRow.set("FIGURENO", info["FIGURENO"]);
                newRow.set("MATERIALSPEC", info["MATERIALSPEC"]);
                newRow.set("NEEDCHECK", info["NEEDCHECK"]);
                newRow.set("ATTRIBUTEID", info["ATTRIBUTEID"]);
                newRow.set("ATTRIBUTENAME", info["ATTRIBUTENAME"]);
                newRow.set("ATTRIBUTECODE", info["ATTRIBUTECODE"]);
                newRow.set("ATTRIBUTEDESC", info["ATTRIBUTEDESC"]);
                newRow.set("QUANTITY", info["QUANTITY"]);
                newRow.set("DEALQUANTITY", info["QUANTITY"]);
                newRow.set("UNITID", info["UNITID"]);
                newRow.set("UNITNAME", info["UNITNAME"]);
                newRow.set("STKUNITID", info["UNITID"]);
                newRow.set("STKUNITNAME", info["UNITNAME"]);
                l++;

                var newRow1 = proto.fromObj[0].addRowForGrid(grid);
                newRow1.set("BILLNO", billno);
                newRow1.set("ROW_ID", l + 1);
                newRow1.set("ROWNO", l + 1);
                newRow1.set("TASKNO", info["TASKNO"]);
                newRow1.set("CONTRACTCODE", info["CONTRACTCODE"]);
                newRow1.set("CONTRACTNO", info["CONTRACTNO"]);
                newRow1.set("WAREHOUSEID", warehouseId);
                newRow1.set("WAREHOUSENAME", warehouseName);
                newRow1.set("STKSTATE", stkstate);
                newRow1.set("STKSTATENAME", stkstateName);
                newRow1.set("WORKPROCESSNO", info["WORKPROCESSNO"]);
                newRow1.set("WORKPROCESSID", info["WORKPROCESSID"]);
                newRow1.set("WORKPROCESSNAME", info["WORKPROCESSNAME"]);
                //newRow1.set("MATERIALID", info["MATERIALID"]);
                //newRow1.set("MATERIALNAME", info["MATERIALNAME"]);
                //newRow1.set("SPECIFICATION", info["SPECIFICATION"]);
                //newRow1.set("TEXTUREID", info["TEXTUREID"]);
                //newRow1.set("FIGURENO", info["FIGURENO"]);
                //newRow1.set("MATERIALSPEC", info["MATERIALSPEC"]);
                //newRow1.set("NEEDCHECK", info["NEEDCHECK"]);
                //newRow1.set("ATTRIBUTEID", info["ATTRIBUTEID"]);
                //newRow1.set("ATTRIBUTENAME", info["ATTRIBUTENAME"]);
                //newRow1.set("ATTRIBUTECODE", info["ATTRIBUTECODE"]);
                //newRow1.set("ATTRIBUTEDESC", info["ATTRIBUTEDESC"]);
                newRow1.set("QUANTITY", info["QUANTITY"]);
                newRow1.set("DEALQUANTITY", info["QUANTITY"]);
                newRow1.set("UNITID", info["UNITID"]);
                newRow1.set("UNITNAME", info["UNITNAME"]);
                newRow1.set("STKUNITID", info["UNITID"]);
                newRow1.set("STKUNITNAME", info["UNITNAME"]);
                l++;
            }
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
    Ext.getCmp("CONTRACTCODE0_" + proto.winId).store.add({ Id: returnList[0].data["CONTRACTCODE"], Name: returnList[0].data["CONTRACTNO"] });
    Ext.getCmp('FROMBILLNO0_' + proto.winId).store.add({ Id: returnList[0].data["FROMBILLNO"], Name: '' });
    Ext.getCmp('CONTRACTCODE0_' + proto.winId).setValue(returnList[0].data["CONTRACTCODE"]);
    Ext.getCmp('FROMBILLNO0_' + proto.winId).setValue(returnList[0].data["FROMBILLNO"]);
    proto.fromObj[0].forms[0].updateRecord(table.data.items[0]);
}