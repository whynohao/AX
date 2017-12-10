/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

stkAllocationQueryVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkAllocationQueryVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkAllocationQueryVcl;

var btnSelectAll = 0;//记录全选还是不选
//调用datafuc的时候会调用此方法，可以初始化一些参数。
proto.winId = "";
winId = "";
proto.fromObj = null;
fromObj = null;
proto.personId = "";
proto.personName = "";
proto.doSetParam = function (vclObj) {
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
function fillStkAllocationQueryData(returnData) {
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
                newRow.set('BILLNO', info.BillNo);
                newRow.set('ROW_ID', info.Row_Id);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('QUANTITY', info.Quantity);
                newRow.set('SPECIFICATION', info.Specification);
                newRow.set('TEXTUREID', info.TextureId);
                newRow.set('FIGURENO', info.FigureNo);
                newRow.set('MATERIALSPEC', info.Materialspec);
                newRow.set('ATTRIBUTEID', info.AttributeId);
                newRow.set('ATTRIBUTENAME', info.AttributeName);
                newRow.set('UNITID', info.UnitId);
                newRow.set('UNITNAME', info.UnitName);
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
    var grid = Ext.getCmp(winId + 'STKALLOCATIONDETAILGrid')
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
                newRow.set('FROMBILLNO', info.billNo);
                newRow.set('FROMROWID', info.row_Id);
                newRow.set('MATERIALID', info.materialId);
                newRow.set('MATERIALNAME', info.materialName);
                newRow.set('QUANTITY', info.quantity);
                newRow.set('SPECIFICATION', info.specification);
                newRow.set('TEXTUREID', info.textureId);
                newRow.set('FIGURENO', info.figureNo);
                newRow.set('MATERIALSPEC', info.materialspec);
                newRow.set('ATTRIBUTEID', info.attributeId);
                newRow.set('ATTRIBUTENAME', info.attributeName);
                newRow.set('UNITID', info.unitId);
                newRow.set('UNITNAME', info.unitName);
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
        if (records[i].get('FROMBILLNO') == info.billNo && records[i].get('FROMROWID') == info.row_Id && records[i].get('MATERIALID') == info.materialId) {
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

//判断是否选了来源单号相同的单据
function checkFromBillNo(list, info) {
    var k = 0;
    for (var i = 0; i < list.length; i++) {
        var listitem = list[i];
        if (info.billNo != listitem.billNo) {
            k = 1;
            break;
        }
    }
    if (k == 1) {
        return false;
    }
    else {
        return true;
    }
}


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    var bodyTable = this.dataSet.getTable(1);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            //查询
            if (e.dataInfo.fieldName == "BTNSELECT") {
                var billNo = this.dataSet.getTable(0).data.items[0].data['PRODUCTORDERNO'];//来源投产单
                var projectId = this.dataSet.getTable(0).data.items[0].data['PROJECTID'];//项目
                var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'];//物料ID
                var personId = this.dataSet.getTable(0).data.items[0].data['PERSONID'];//备料执行人
                if (projectId == "" && materialId == "" && personId == "") {
                    alert("请至少填写一项查询条件！");
                    return;
                }
                else {
                    var billNo = billNo == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['PRODUCTORDERNO'];
                    var projectId = projectId == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['PROJECTID'];
                    var materialId = materialId == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['MATERIALID'];
                    var personId = personId == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['PERSONID'];
                    var data = this.invorkBcf('GetData', [materialId, projectId, billNo]);
                    fillStkAllocationQueryData.call(this, data);
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
                    if (bodyTable.data.items[i].data["ISCHOSE"] == true) {
                        var quantity = bodyTable.data.items[i].data["QUANTITY"];
                        list.push({
                            billNo: bodyTable.data.items[i].data["BILLNO"],
                            row_Id: bodyTable.data.items[i].data["ROW_ID"],
                            materialId: bodyTable.data.items[i].data["MATERIALID"],
                            materialName: bodyTable.data.items[i].data["MATERIALNAME"],
                            quantity: quantity,
                            specification: bodyTable.data.items[i].data["SPECIFICATION"],
                            textureId: bodyTable.data.items[i].data["TEXTUREID"],
                            figureNo: bodyTable.data.items[i].data["FIGURENO"],
                            materialspec: bodyTable.data.items[i].data["MATERIALSPEC"],
                            attributeId: bodyTable.data.items[i].data["ATTRIBUTEID"],
                            attributeName: bodyTable.data.items[i].data["ATTRIBUTENAME"],
                            unitId: bodyTable.data.items[i].data["UNITID"],
                            unitName: bodyTable.data.items[i].data["UNITNAME"]
                        });
                    }
                }
                if (list.length > 0) {
                    var info = list[0];
                    if (checkFromBillNo(list, info)) {
                        var FrombillNo = info.billNo;
                        var returnNoAndId = this.invorkBcf('GetContractNo', [FrombillNo]);
                        var returnContractNo = returnNoAndId[0];
                        var contactsObjectId = returnNoAndId[1];
                        //往来对象
                       
                        var contactsObjectName = returnNoAndId[2];
                        fromObj.dataSet.getTable(0).data.items[0].set("CONTACTOBJECTID", contactsObjectId);
                        var field = Ext.getCmp('CONTACTOBJECTID0_' + proto.winId);
                        field.store.add({ Id: contactsObjectId, Name: contactsObjectName });
                        field.select(contactsObjectId);

                        //来源单
                        var masterRow = fromObj.dataSet.getTable(0).data.items[0];
                        var masterFromBillNo = masterRow.get('FROMBILLNO');

                        if (masterFromBillNo != FrombillNo) {
                            fromObj.dataSet.getTable(1).removeAll();
                        }
                        
                        Ext.getCmp('PRODUCTCONTRACTNO0_' + proto.winId).setValue(returnContractNo);
                        fromObj.dataSet.getTable(0).data.items[0].set("FROMBILLNO", FrombillNo);
                        var field = Ext.getCmp('FROMBILLNO0_' + proto.winId);
                        field.store.add({ Id: FrombillNo, Name: '' });
                        field.select(FrombillNo);
                        fromObj.forms[0].updateRecord(fromObj.dataSet.getTable(0).data.items[0]);

                        this.win.close();
                        fillStockInData.call(this, list);
                    }
                    else {
                        Ext.Msg.alert("提示","请选择来源投产单号相同的数据");
                    }
                }
                else {
                    Ext.Msg.alert("提示", "请选择数据");
                }                
                break;
            }
            //全选
            if (e.dataInfo.fieldName == "BTNSELECTALL") {
                var allItems = this.dataSet.getTable(1).data.items;
                if (btnSelectAll == 0) {
                    for (var i = 0; i < allItems.length; i++) {
                        allItems[i].set("ISCHOSE", true);
                    }
                    btnSelectAll = 1;
                }
                else {
                    for (var i = 0; i < allItems.length; i++) {
                        allItems[i].set("ISCHOSE", false);
                    }
                    btnSelectAll = 0;
                }
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) { e.dataInfo.cancel = true; Ext.Msg.alert("提示", "不能新增"); }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) { e.dataInfo.cancel = true; Ext.Msg.alert("提示", "不能删除"); }
            break;
        //case LibEventTypeEnum.Validated://form的赋值之后在this.dataSet中还没有实时的数据写入，需要写入之后，才能取到值。          
        //    if (e.dataInfo.fieldName == "WAREHOUSEID" || e.dataInfo.fieldName == "STORAGEID" || e.dataInfo.fieldName == "MATERIALID" || e.dataInfo.fieldName == "MATERIALTYPEID" || e.dataInfo.fieldName == "STKSTATE" || e.dataInfo.fieldName == "STKATTR" || e.dataInfo.fieldName == "BATCHNO" || e.dataInfo.fieldName == "SUBBATCHNO") {
        //        e.dataInfo.curForm.updateRecord(e.dataInfo.dataRow);
        //    }
        //    break;
    }
};
