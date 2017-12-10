/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

stkProcessStockInQueryVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkProcessStockInQueryVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkProcessStockInQueryVcl;

//调用datafuc的时候会调用此方法，可以初始化一些参数。
proto.winId = "";
proto.fromObj = null;
proto.personId = "";
proto.personName = "";
proto.doSetParam = function (vclObj) {
    debugger;
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj[0];
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
        var list = returnData['ProcessStockIn'];
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set('TASKNO', info.TASKNO);
                newRow.set('WORKNO', info.WORKNO);
                newRow.set('PPWORKORDER', info.PPWORKORDER);
                newRow.set('WORKSHOPSECTIONID', info.WORKSHOPSECTIONID);
                newRow.set('WORKSHOPSECTIONNAME', info.WORKSHOPSECTIONNAME);
                newRow.set('MATERIALID', info.MATERIALID);
                newRow.set('MATERIALNAME', info.MATERIALNAME);
                newRow.set('UNITID', info.UNITID);
                newRow.set('UNITNAME', info.UNITNAME);
                newRow.set('WAREHOUSEPERSONID', info.WAREHOUSEPERSONID);
                newRow.set('WAREHOUSEPERSONNAME', info.WAREHOUSEPERSONNAME);
                newRow.set('STOCKINQUANTITY', info.STOCKINQUANTITY);
                newRow.set('ACTUALSTOCKINQTY', info.ACTUALSTOCKINQTY);
                newRow.set('STARTTIME', info.STARTTIME);
                newRow.set('ENDTIME', info.ENDTIME);
                newRow.set('RECEIVEPERSONID', info.RECEIVEPERSONID);
                newRow.set('RECEIVEPERSONNAME', info.RECEIVEPERSONNAME);
                newRow.set('SPECIFICATION', info.Specification);
                newRow.set('TEXTUREID', info.TextureId);
                newRow.set('FIGURENO', info.FigureNo);
                newRow.set('MATERIALSPEC', info.Materialspec);
                newRow.set('ATTRIBUTEID', info.AttributeId);
                newRow.set('ATTRIBUTENAME', info.AttributeName);
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
    var grid = Ext.getCmp(proto.winId + 'STKPROCESSSTOCKINDETAILGrid')
    var list = returnData;
    Ext.suspendLayouts();
    var curStore = grid.getStore();
    var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
    curStore.suspendEvents();
    try {
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                if (checkGetNotice(grid, info)) {
                    continue;
                }
                var newRow = proto.fromObj.addRow(masterRow, 1);
                newRow.set('TASKNO', info.TASKNO);
                newRow.set('MATERIALID', info.MATERIALID);
                newRow.set('MATERIALNAME', info.MATERIALNAME);
                newRow.set('UNITID', info.UNITID);
                newRow.set('UNITNAME', info.UNITNAME);
                newRow.set('QUANTITY', info.QUANTITY);
                newRow.set('SPECIFICATION', info.specification);
                newRow.set('TEXTUREID', info.textureId);
                newRow.set('FIGURENO', info.figureNo);
                newRow.set('MATERIALSPEC', info.materialspec);
                newRow.set('ATTRIBUTEID', info.attributeId);
                newRow.set('ATTRIBUTENAME', info.attributeName);
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

//判断在制品入库单中是否已经存在当前返填写过去的任务
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
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    var bodyTable = this.dataSet.getTable(1);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            //查询
            if (e.dataInfo.fieldName == "BTNSELECT") {
                var workshopsectionId = this.dataSet.getTable(0).data.items[0].data["WORKSHOPSECTIONID"];//工段ID
                var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'];//物料ID
                var personId = this.dataSet.getTable(0).data.items[0].data['PERSONID'];//接收人
                if (workshopsectionId == "" && materialId == "" && personId == "") {
                    alert("请至少填写一项查询条件！");
                    return;
                }
                else {
                    var workshopsectionId = workshopsectionId == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['WORKSHOPSECTIONID'];
                    var materialId = materialId == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['MATERIALID'];
                    var personId = personId == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['PERSONID'];
                    var data = this.invorkBcf('GetData', [workshopsectionId, materialId, personId]);
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
                    var stockinquantity = bodyTable.data.items[i].data["STOCKINQUANTITY"];
                    var actualstockinqty = bodyTable.data.items[i].data["ACTUALSTOCKINQTY"];
                    list.push({
                        TASKNO: bodyTable.data.items[i].data["TASKNO"],
                        WORKSHOPSECTIONID: bodyTable.data.items[i].data["WORKSHOPSECTIONID"],
                        MATERIALID: bodyTable.data.items[i].data["MATERIALID"],
                        MATERIALNAME: bodyTable.data.items[i].data["MATERIALNAME"],
                        UNITID: bodyTable.data.items[i].data["UNITID"],
                        UNITNAME: bodyTable.data.items[i].data["UNITNAME"],
                        QUANTITY: stockinquantity - actualstockinqty, //需入库-实际入库
                        specification: bodyTable.data.items[i].data["SPECIFICATION"],
                        textureId: bodyTable.data.items[i].data["TEXTUREID"],
                        figureNo: bodyTable.data.items[i].data["FIGURENO"],
                        materialspec: bodyTable.data.items[i].data["MATERIALSPEC"],
                        attributeId: bodyTable.data.items[i].data["ATTRIBUTEID"],
                        attributeName: bodyTable.data.items[i].data["ATTRIBUTENAME"],
                        UNITID: bodyTable.data.items[i].data["UNITID"],
                        UNITNAME: bodyTable.data.items[i].data["UNITNAME"]
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
