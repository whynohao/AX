/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

stkProductionOutQueryVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkProductionOutQueryVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkProductionOutQueryVcl;

//调用datafuc的时候会调用此方法，可以初始化一些参数。
proto.winId = "";
winId = "";
proto.fromObj = null;
fromObj = null;
proto.personId = "";
proto.personName = "";
proto.doSetParam = function (vclObj) {
    debugger;
    proto.winId = vclObj[0].winId;
    winId = vclObj[0].winId;
    proto.fromObj = vclObj[0];
    fromObj = vclObj[0];
    proto.personId = vclObj[1];
    proto.personName = vclObj[2];
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set("PERSONID", proto.personId);
    masterRow.set("PERSONNAME", proto.personName);
    this.forms[0].loadRecord(masterRow);
};
function fillData(returnData) {
    Ext.suspendLayouts();
    var formStore = this.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        this.deleteAll(1);
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var list = returnData['StockInfoList'];
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set('TASKNO', info.TASKNO);
                newRow.set('WORKNO', info.WORKNO);
                newRow.set('PPWORKORDER', info.PPWORKORDER);
                newRow.set('PRODUCELINEID', info.PRODUCELINEID);
                newRow.set('PRODUCELINENAME', info.PRODUCELINENAME);
                newRow.set('WORKSHOPSECTIONID', info.WORKSHOPSECTIONID);
                newRow.set('WORKSHOPSECTIONNAME', info.WORKSHOPSECTIONNAME);
                newRow.set('WORKPROCESSID', info.WORKPROCESSID);
                newRow.set('MATERIALID', info.MATERIALID);
                newRow.set('MATERIALNAME', info.MATERIALNAME);
                newRow.set('MATERIALSPEC', info.MATERIALSPEC);
                newRow.set('UNITID', info.UNITID);
                newRow.set('UNITNAME', info.UNITNAME);
                newRow.set('WAREHOUSEPERSONID', info.WAREHOUSEPERSONID);
                newRow.set('WAREHOUSEPERSONNAME', info.WAREHOUSEPERSONNAME);
                newRow.set('ATTRIBUTECODE', info.ATTRIBUTECODE);
                newRow.set('ATTRIBUTEDESC', info.ATTRIBUTEDESC);
                newRow.set('PRODUCTID', info.PRODUCTID);
                newRow.set('PRODUCTNAME', info.PRODUCTNAME);
                newRow.set('CREATETIME', info.CREATETIME);
                newRow.set('STOCKQUANTITY', info.STOCKQUANTITY);
                newRow.set('ACTUALSTOCKQTY', info.ACTUALSTOCKQTY);
                newRow.set('DELIVERQUANTITY', info.DELIVERQUANTITY);
                newRow.set('ACTUALDELIVERQTY', info.ACTUALDELIVERQTY);
                newRow.set('STOCKSTARTTIME', info.STOCKSTARTTIME);
                newRow.set('STOCKENDTIME', info.STOCKENDTIME);
                newRow.set('STOCKRECEIVETIME', info.STOCKRECEIVETIME);
                newRow.set('ACTUALSTOCKSTARTTIME', info.ACTUALSTOCKSTARTTIME);
                newRow.set('ACTUALSTOCKENDTIME', info.ACTUALSTOCKENDTIME);
                newRow.set('SENDSTARTTIME', info.SENDSTARTTIME);
                newRow.set('SENDENDTIME', info.SENDENDTIME);
                newRow.set('ACTUALSENDSTARTTIME', info.ACTUALSENDSTARTTIME);
                newRow.set('ACTUALSENDENDTIME', info.ACTUALSENDENDTIME);
                newRow.set('PERSONID', info.PERSONID);
                newRow.set('PERSONNAME', info.PERSONNAME);
                newRow.set('SENDPERSONID', info.SENDPERSONID);
                newRow.set('SENDPERSONNAME', info.SENDPERSONNAME);
                newRow.set('FROMBILLNO', info.FROMBILLNO);
                newRow.set('FROMROWID', info.FROMROWID);
                newRow.set('ACTUALRECEIVEQTY', info.ACTUALRECEIVEQTY);
                newRow.set('SENDTYPE', info.SENDTYPE);
                newRow.set('MANAGERID', info.MANAGERID);
                newRow.set('MANAGERNAME', info.MANAGERNAME);
                newRow.set('SENDMANAGERID', info.SENDMANAGERID);
                newRow.set('SENDMANAGERNAME', info.SENDMANAGERNAME);
                newRow.set('FROMTYPE', info.FROMTYPE);
                newRow.set('ORDERDATE', info.ORDERDATE);
                newRow.set('LOTNO', info.LOTNO);
                newRow.set('GROUPNO', info.GROUPNO);
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

function fillStockInData(returnData) {
    var grid = Ext.getCmp(winId + 'STKPRODUCTIONOUTDETAILGrid')
    var list = returnData;
    Ext.suspendLayouts();
    var curStore = grid.getStore();
    var masterRow = fromObj.dataSet.getTable(0).data.items[0];
    curStore.suspendEvents();
    try {
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                if (checkGetNotice(grid, info)) {
                    continue;
                }
                var newRow = fromObj.addRow(masterRow, 1);
                newRow.set('TASKNO', info.TASKNO);
                newRow.set('MATERIALID', info.MATERIALID);
                newRow.set('MATERIALNAME', info.MATERIALNAME);
                newRow.set('UNITID', info.UNITID);
                newRow.set('UNITNAME', info.UNITNAME);
                newRow.set('ATTRIBUTECODE', info.ATTRIBUTECODE);
                newRow.set('ATTRIBUTEDESC', info.ATTRIBUTEDESC);
                newRow.set('QUANTITY', info.QUANTITY);
            }
        }
    }
    finally {
        curStore.resumeEvents();
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);
    }
}

//判断调拨单中是否已经存在当前返填写过去的任务
function checkGetNotice(grid, info) {
    var k = 0;
    var records = grid.store.data.items;
    for (var i = 0; i < records.length; i++) {
        if (records[i].get('TASKNO') == info.TASKNO) {
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

proto.vclHandler = function (sender, e) {
    debugger;
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    var bodyTable = this.dataSet.getTable(1);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            //查询
            if (e.dataInfo.fieldName == "BTNSELECT") {
                var producelineId = this.dataSet.getTable(0).data.items[0].data["PRODUCELINEID"];//生产线ID
                var workshopsectionId = this.dataSet.getTable(0).data.items[0].data["WORKSHOPSECTIONID"];//工段ID
                var workprocessId = this.dataSet.getTable(0).data.items[0].data["WORKPROCESSID"];//工序ID
                var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'];//物料ID
                var personId = this.dataSet.getTable(0).data.items[0].data['PERSONID'];//备料执行人
                if (producelineId == "" && workshopsectionId == "" && workprocessId == "" && materialId == "" && personId == "") {
                    alert("请至少填写一项查询条件！");
                    return;
                }
                else {
                    var producelineId = producelineId == undefined ? "" : this.dataSet.getTable(0).data.items[0].data["PRODUCELINEID"]
                    var workshopsectionId = workshopsectionId == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['WORKSHOPSECTIONID'];
                    var workprocessId = workprocessId == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['WORKPROCESSID'];
                    var materialId = materialId == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['MATERIALID'];
                    var personId = personId == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['PERSONID'];
                    var data = this.invorkBcf('GetData', [producelineId, workshopsectionId, workprocessId, materialId, personId]);
                    fillData.call(this, data);
                }
                break;
            }
            //重置
            if (e.dataInfo.fieldName == "BTNRESET") {
                this.deleteAll(1)
                break;
            }
            //加载
            if (e.dataInfo.fieldName == "BTNSAVE") {
                var list = [];
                for (var i = 0; i < bodyTable.data.items.length; i++) {
                    //var stockquantity = bodyTable.data.items[i].data["STOCKQUANTITY"]; 
                    //var actualstockqty = bodyTable.data.items[i].data["ACTUALSTOCKQTY"]; 
                    var deliverQuantity = bodyTable.data.items[i].data["DELIVERQUANTITY"];
                    var actualDeliverQty = bodyTable.data.items[i].data["ACTUALDELIVERQTY"];
                    list.push({
                        TASKNO: bodyTable.data.items[i].data["TASKNO"],
                        WORKSHOPSECTIONID: bodyTable.data.items[i].data["WORKSHOPSECTIONID"],
                        MATERIALID: bodyTable.data.items[i].data["MATERIALID"],
                        MATERIALNAME: bodyTable.data.items[i].data["MATERIALNAME"],
                        UNITID: bodyTable.data.items[i].data["UNITID"],
                        UNITNAME: bodyTable.data.items[i].data["UNITNAME"],
                        ATTRIBUTECODE: bodyTable.data.items[i].data["ATTRIBUTECODE"],
                        ATTRIBUTEDESC: bodyTable.data.items[i].data["ATTRIBUTEDESC"],
                        QUANTITY: deliverQuantity - actualDeliverQty //需派料-实际派料
                    });
                }
                this.win.close();
                fillStockInData.call(this, list);
                break;
            }
            break;
        //case LibEventTypeEnum.Validated://form的赋值之后在this.dataSet中还没有实时的数据写入，需要写入之后，才能取到值。          
        //    if (e.dataInfo.fieldName == "WAREHOUSEID" || e.dataInfo.fieldName == "STORAGEID" || e.dataInfo.fieldName == "MATERIALID" || e.dataInfo.fieldName == "MATERIALTYPEID" || e.dataInfo.fieldName == "STKSTATE" || e.dataInfo.fieldName == "STKATTR" || e.dataInfo.fieldName == "BATCHNO" || e.dataInfo.fieldName == "SUBBATCHNO") {
        //        e.dataInfo.curForm.updateRecord(e.dataInfo.dataRow);
        //    }
        //    break;
    }
};
