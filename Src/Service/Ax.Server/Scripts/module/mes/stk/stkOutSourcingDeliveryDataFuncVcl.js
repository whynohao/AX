stkOutSourcingDeliveryDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkOutSourcingDeliveryDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkOutSourcingDeliveryDataFuncVcl;
proto.winId = null;
proto.fromObj = null;

proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj;
    var mastHeadRow = proto.fromObj[0].dataSet.getTable(0).data.items[0];
    //var fromBillNo = mastHeadRow.get("FROMBILLNO");
    var contractsObjectId = mastHeadRow.get("CONTACTSOBJECTID");
    var contractsObjectName = mastHeadRow.get("CONTACTSOBJECTNAME");
    Ext.getCmp("CONTACTSOBJECTID0_" + vcl.winId).store.add({ Id: contractsObjectId, Name: contractsObjectName });
    Ext.getCmp('CONTACTSOBJECTID0_' + vcl.winId).setValue(contractsObjectId);
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            var bodyTable = this.dataSet.getTable(1).data.items;
            if (e.dataInfo.fieldName == "BtnSelect") {
                var contractCode = this.dataSet.getTable(0).data.items[0].data["CONTRACTCODE"];
                var fromBillNo = this.dataSet.getTable(0).data.items[0].data["FROMBILLNO"];
                var contractsObjectId = this.dataSet.getTable(0).data.items[0].data["CONTRACTSOBJECTID"];
                if (contractsObjectId == "") {
                    Ext.Msg.alert("提示", "往来单位不能为空！");
                    return;
                }
                else {
                    var records = this.invorkBcf('GetData', [contractCode, fromBillNo, contractsObjectId]);
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
        var grid = Ext.getCmp(this.winId + 'STKOUTSOURCINGDELIVERYDATAFUNCDETAILGrid');
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set("BILLNO", info.BILLNO);
                newRow.set("ROW_ID", info.ROW_ID);
                newRow.set("TASKNO", info.TASKNO);
                newRow.set("CONTRACTCODE", info.CONTRACTCODE);
                newRow.set("CONTRACTNO", info.CONTRACTNO);
                newRow.set("CONTACTOBJECTID", info.CONTACTOBJECTID);
                newRow.set("CONTACTSOBJECTNAME", info.CONTACTSOBJECTNAME);
                newRow.set("WORKPROCESSNO", info.WORKPROCESSNO);
                newRow.set("WORKPROCESSID", info.WORKPROCESSID);
                newRow.set("WORKPROCESSNAME", info.WORKPROCESSNAME);
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
                newRow.set("UNITID", info.UNITID);
                newRow.set("UNITNAME", info.UNITNAME);
                newRow.set("STKUNITID", info.STKUNITID);
                newRow.set("STKUNITNAME", info.STKUNITNAME);
                newRow.set("STKUNITNO", info.STKUNITNO);
                newRow.set("QUANTITY", info.QUANTITY);
                newRow.set("DEALQUANTITY", info.DEALQUANTITY);
                newRow.set("PRICE", info.PRICE);
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
        var grid = Ext.getCmp(proto.winId + 'STKOUTSOURCINGDELIVERYDETAILGrid');
        var table = proto.fromObj[0].dataSet.getTable(0);
        var billno = table.data.items[0].data["BILLNO"];
        var headTable = this.dataSet.getTable(0).data.items[0];
        var taxRate = headTable.data["TAXRATE"];
        var standardcoilRate = headTable.data["STANDARDCOILRATE"];
        var warehouseId = table.data.items[0].data["WAREHOUSEID"];
        var warehouseName = table.data.items[0].data["WAREHOUSENAME"];
        var warehousePersonId = table.data.items[0].data["WAREHOUSEPERSONID"];
        var warehousePersonName = table.data.items[0].data["WAREHOUSEPERSONNAME"];
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
                newRow.set("WORKPROCESSNO", info["WORKPROCESSNO"]);
                newRow.set("WORKPROCESSID", info["WORKPROCESSID"]);
                newRow.set("WORKPROCESSNAME", info["WORKPROCESSNAME"]);
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
                newRow.set("RECEIVEQTY", info["DEALQUANTITY"]);//到货数量
                newRow.set("DEALSUNITID", info["STKUNITID"]);
                newRow.set("DEALSUNITNAME", info["STKUNITNAME"]);
                newRow.set("DEALSUNITNO", info["STKUNITNO"]);
                newRow.set("QUANTITY", info["QUANTITY"]);
                newRow.set("PRICE", info["PRICE"]);
                newRow.set("AMOUNT", info["PRICE"] * info["DEALQUANTITY"]);
                newRow.set("TAXRATE", taxRate);
                newRow.set("TAXPRICE", info["PRICE"] * (1 + taxRate));
                newRow.set("TAXAMOUNT", info["PRICE"] * (1 + taxRate) * info["DEALQUANTITY"]);
                newRow.set("TAXES", info["PRICE"] * taxRate * info["DEALQUANTITY"]);
                newRow.set("BWAMOUNT", info["PRICE"] * info["DEALQUANTITY"] * standardcoilRate);
                newRow.set("BWTAXAMOUNT", info["PRICE"] * (1 + taxRate) * info["DEALQUANTITY"] * standardcoilRate);
                newRow.set("BWTAXES", info["PRICE"] * taxRate * info["DEALQUANTITY"] * standardcoilRate);
                newRow.set("WAREHOUSEID", warehouseId);
                newRow.set("WAREHOUSENAME", warehouseName);
                newRow.set("WAREHOUSEPERSONID", warehousePersonId);
                newRow.set("WAREHOUSEPERSONNAME", warehousePersonName);
                newRow.set("CANDEALSQTY", info["DEALQUANTITY"]);
                newRow.set("CANQTY", info["QUANTITY"]);
                newRow.set("FROMBILLNO", info["BILLNO"]);
                newRow.set("FROMROW_ID", info["ROW_ID"]);
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
    Ext.getCmp("CONTACTSOBJECTID0_" + proto.winId).store.add({ Id: headTable.data["CONTACTSOBJECTID"], Name: headTable.data["CONTACTSOBJECTNAME"] });
    Ext.getCmp('CURRENCYID0_' + proto.winId).store.add({ Id: headTable.data["CURRENCYID"], Name: headTable.data["CURRENCYNAME"] });
    Ext.getCmp("INVOICETYPEID0_" + proto.winId).store.add({ Id: headTable.data["INVOICETYPEID"], Name: headTable.data["INVOICETYPENAME"] });
    Ext.getCmp('PAYMENTTYPEID0_' + proto.winId).store.add({ Id: headTable.data["PAYMENTTYPEID"], Name: headTable.data["PAYMENTTYPENAME"] });
    //Ext.getCmp("PRODUCTORDER0_" + proto.winId).store.add({ Id: returnList[0].data["CONTRACTCODE"], Name: returnList[0].data["CONTRACTNO"] });
    Ext.getCmp('CONTACTSOBJECTID0_' + proto.winId).setValue(headTable.data["CONTACTSOBJECTID"]);
    Ext.getCmp('CURRENCYID0_' + proto.winId).setValue(headTable.data["CURRENCYID"]);
    Ext.getCmp('INVOICETYPEID0_' + proto.winId).setValue(headTable.data["INVOICETYPEID"]);
    Ext.getCmp('PAYMENTTYPEID0_' + proto.winId).setValue(headTable.data["PAYMENTTYPEID"]);
   // Ext.getCmp('PRODUCTORDER0_' + proto.winId).setValue(returnList[0].data["CONTRACTCODE"]);
    proto.fromObj[0].forms[0].updateRecord(table.data.items[0]);
}