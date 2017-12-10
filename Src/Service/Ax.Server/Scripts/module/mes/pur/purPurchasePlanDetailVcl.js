purPurchasePlanDetailVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = purPurchasePlanDetailVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = purPurchasePlanDetailVcl;

 
proto.winId = null;
proto.fromObj = null;
var records = null;
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj[0];
    var grid = Ext.getCmp(proto.winId + 'PURPURCHASEPLANDETAILGrid'); //要加载数据的表名字 + Grid
    records = grid.getView().getSelectionModel().getSelection(); 
    //表头填充
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set("BILLNO", records[0].data["BILLNO"]);
    masterRow.set("ROW_ID", records[0].data["ROW_ID"]);
    masterRow.set("PLANQUANTITY", records[0].data["QUANTITY"]);
    masterRow.set("PLANARRIVEDATE", records[0].data["PLANARRIVEDATE"]);
    masterRow.set("SPLITNUM", 1); 
    //表身填充
    var list = this.invorkBcf("GetDetial", [records[0].data["BILLNO"], records[0].data["ROW_ID"],1]);
    FillData.call(this, list);
    this.isEdit = true;
    this.forms[0].loadRecord(masterRow);
};

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    this.forms[0].updateRecord(masterRow);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "SPLITNUM") {
                    if (e.dataInfo.value>0) {
                        var list = this.invorkBcf("GetDetial", [masterRow.data["BILLNO"], masterRow.data["ROW_ID"], e.dataInfo.value]);
                        FillData.call(this, list);
                    }
                }
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnLoadData") { 
                table = this.dataSet.getTable(1);
                for (var i = 0; i < table.data.items.length; i++) {
                    var record=table.data.items[i];
                    if (record.get["QUANTITY"] == 0 || record.get["PLANARRIVEDATE"] == 0) {
                        Ext.Msg.alert("系统提示", "计划数量必须大于0，计划到货日期不能为空！");
                        return;
                    }
                    if (record.get["QUANTITY"] < record.get["INSTOCKQTY"])
                    {
                        Ext.Msg.alert("系统提示", "拆分号为" + record.get["SPLITNO"] + "的计划数量不能小于已入库数！");
                        return;
                    }
                }
                this.win.close();
                fillGetStockReturnData(table);
            }
            break; 
    } 
}


function FillData(list) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据   
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set("BILLNO", info.BillNo);
                newRow.set("ROW_ID", info.RowId);
                newRow.set("ROWNO", info.RowNo);
                newRow.set("SPLITNO", info.SplitNo)
                newRow.set("FROMBILLNO", info.FromBillNo)
                newRow.set("FROMTYPE", info.FromType);
                newRow.set("WORKNO", info.WorkNo)
                newRow.set("PERSONID", info.PersonId);
                newRow.set("PERSONNAME", info.PersonName);
                newRow.set("MATERIALID", info.MaterialId);
                newRow.set("MATERIALNAME", info.MaterialName);
                newRow.set("MATERIALSPEC", info.MaterialName);
                newRow.set("UNITID", info.UnitId);
                newRow.set("UNITNAME", info.UnitName);
                newRow.set("QUANTITY", info.Quantity);
                newRow.set("PLANARRIVEDATE", info.PlanArriveDate);
                newRow.set("INSTOCKQTY", info.InQuantity);
                newRow.set("NONSTOCKQTY", info.NonQuantity);
                newRow.set("PURCHASEORDER", info.PurChaseOrder);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

function fillGetStockReturnData(dt)
{
    var grid = Ext.getCmp(proto.winId + 'PURPURCHASEPLANDETAILGrid');
    Ext.suspendLayouts();
    var curStore = grid.getStore();
    curStore.suspendEvents(); 
    try {
        if (records !== undefined && records.length > 0) {
            for (var i = 0; i < dt.data.items.length; i++) { 
                var info = table.data.items[i];
                if (checkGetStock(grid, info)) {
                    continue;
                } 
                var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
                var newRow = proto.fromObj.addRow(masterRow, 1); 
                newRow.set("SPLITNO", info.get("ROW_ID"));
                newRow.set("FROMBILLNO", info.get("FROMBILLNO"));
                newRow.set("FROMTYPE", info.get("FROMTYPE"));
                newRow.set("WORKNO", info.get("WORKNO"));
                newRow.set("PERSONID", info.get("PERSONID"));
                newRow.set("PERSONNAME", info.get("PERSONNAME"));
                newRow.set("MATERIALID", info.get("MATERIALID"));
                newRow.set("MATERIALNAME", info.get("MATERIALNAME"));
                newRow.set("MATERIALSPEC", info.get("MATERIALSPEC"));
                newRow.set("UNITID", info.get("UNITID"));
                newRow.set("UNITNAME", info.get("UNITNAME"));
                newRow.set("QUANTITY", info.get("QUANTITY"));
                newRow.set("INSTOCKQTY", info.get("INSTOCKQTY"));
                newRow.set("NONSTOCKQTY", info.get("NONSTOCKQTY"));
                newRow.set("CANCELQTY", info.get("0"));
                newRow.set("PLANARRIVEDATE", info.get("PLANARRIVEDATE"));
                newRow.set("DEMANDDATE", info.get("DEMANDDATE")); 
            }
        }
    } finally {
        curStore.resumeEvents();
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);
    }

}


checkGetStock = function (grid, info)
{
    var k = 0;
    var records = grid.store.data.items;
    for (var i = 0; i < records.length; i++) { 
        if (records[i].get("ROW_ID") == info.get("ROW_ID") && records[i].get("SPLITNO") == info.get("SPLITNO")) {
            records[i].set("QUANTITY", info.get("QUANTITY"));
            records[i].set("PLANARRIVEDATE", info.get("PLANARRIVEDATE")); 
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