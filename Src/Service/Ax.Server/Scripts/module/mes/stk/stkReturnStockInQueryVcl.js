/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

stkReturnStockInQueryVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkReturnStockInQueryVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkReturnStockInQueryVcl;

//调用datafuc的时候会调用此方法，可以初始化一些参数。
proto.personId = "";
proto.personName = "";
proto.winId = "";
proto.fromObj = null;
proto.doSetParam = function (vclObj) {
    proto.personId = vclObj[0];
    proto.personName = vclObj[1];
    proto.winId = vclObj[2].winId;
    proto.fromObj = vclObj[2];
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
                //newRow.set('TASKNO', info.TaskNo);//任务号
                newRow.set('FROMBILLNO', info.FrombillNo);//来源销售出库单号
                newRow.set('FROMROWID', info.FromRowId);// 来源行标识
                newRow.set('WAREHOUSEID', info.WarehouseId);// 仓库
                newRow.set('WAREHOUSENAME', info.WarehouseName);// 仓库名称
                newRow.set('COMPANYID', info.CompanyId);// 隶属公司
                newRow.set('COMPANYNAME', info.CompanyName);// 公司名称
                newRow.set('CONTACTOBJECTID', info.ContactObjectId);// 往来单位
                newRow.set('CONTACTSOBJECTNAME', info.ContactsObjectName);// 往来单位名称
                newRow.set('MATERIALID', info.MaterialId);// 物料ID
                newRow.set('MATERIALNAME', info.MaterialName);// 物料名称
                newRow.set('ATTRIBUTEID', info.AttributeId);// 特征
                newRow.set('ATTRIBUTENAME', info.AttributeName);// 特征名称
                newRow.set('ATTRIBUTECODE', info.AttributeCode);// 特征标识
                newRow.set('ATTRIBUTEDESC', info.AttributeDesc);// 特征描述
                newRow.set('UNITID', info.UnitId);// 基本单位
                newRow.set('UNITNAME', info.UnitName);// 基本单位名称
                newRow.set('STKATTR', info.StkAttr);// 存货属性
                newRow.set('STKSTATE', info.StkState);// 库存状态
                newRow.set('STKSTATENAME', info.StkStateName);// 库存状态名称 
                newRow.set('BATCHNO', info.BatchNo);// 批号
                newRow.set('SUBBATCHNO', info.SubBatchNo);// 小批号
                newRow.set('COMPLETENO', info.CompleteNo);// 完工标识号
                newRow.set('MTONO', info.MTONo);// MTO号
                newRow.set('STORAGEID', info.StorageId);// 库位
                newRow.set('STORAGENAME', info.StorageName);// 库位名称
                newRow.set('RESERVEDNO1', info.ReservedNo1);// 预留一  空
                newRow.set('RESERVEDNO2', info.ReservedNo2);// 预留二  空
                newRow.set('BORROWQUANTITY', info.BorrowQuantity);// 借用数量
                newRow.set('NEEDRETURNQUANTITY', info.NeedReturnQuantity);// 待归还数量
                newRow.set('BORROWTIME', info.BorrowTime);//借料日期
                newRow.set('PRICE', info.Price);// 单价
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
    var grid = Ext.getCmp(proto.winId + 'STKRETURNSTOCKINDETAILGrid')
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
                newRow.set('FROMBILLNO', info.FrombillNo);//来源借料单号
                newRow.set('FROMROWID', info.FromRowId);// 来源行标识
                newRow.set('WAREHOUSEID', info.WarehouseId);// 仓库
                newRow.set('WAREHOUSENAME', info.WarehouseName);// 仓库名称
                newRow.set('COMPANYID', info.CompanyId);// 隶属公司
                newRow.set('COMPANYNAME', info.CompanyName);// 公司名称
                newRow.set('CONTACTOBJECTID', info.ContactObjectId);// 往来单位
                newRow.set('CONTACTSOBJECTNAME', info.ContactsObjectName);// 往来单位名称
                newRow.set('MATERIALID', info.MaterialId);// 物料ID
                newRow.set('MATERIALNAME', info.MaterialName);// 物料名称
                newRow.set('ATTRIBUTEID', info.AttributeId);// 特征
                newRow.set('ATTRIBUTENAME', info.AttributeName);// 特征名称
                newRow.set('ATTRIBUTECODE', info.AttributeCode);// 特征标识
                newRow.set('ATTRIBUTEDESC', info.AttributeDesc);// 特征描述
                newRow.set('UNITID', info.UnitId);// 基本单位
                newRow.set('UNITNAME', info.UnitName);// 基本单位名称
                newRow.set('STKATTR', info.StkAttr);// 存货属性
                newRow.set('STKSTATE', info.StkState);// 库存状态
                newRow.set('STKSTATENAME', info.StkStateName);// 库存状态名称
                newRow.set('BATCHNO', info.BatchNo);// 批号
                newRow.set('SUBBATCHNO', info.SubBatchNo);// 小批号
                newRow.set('COMPLETENO', info.CompleteNo);// 完工标识号
                newRow.set('MTONO', info.MTONo);// MTO号
                newRow.set('STORAGEID', info.StorageId);// 库位  空
                newRow.set('STORAGENAME', info.StorageName);// 库位名称
                newRow.set('RESERVEDNO1', info.ReservedNo1);// 预留一  空
                newRow.set('RESERVEDNO2', info.ReservedNo2);// 预留二  空
                newRow.set('BORROWQUANTITY', info.BorrowQuantity);// 借用数量
                newRow.set('NEEDRETURNQUANTITY', info.NeedReturnQuantity);// 待归还数量
                newRow.set('BORROWTIME', info.BorrowTime);//借料日期
                newRow.set('PRICE', info.Price);// 单价
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

//判断销售退货入库单中是否已经存在当前返填写过去的物料
function checkGetNotice(grid, info) {
    var k = 0;
    var records = grid.store.data.items;
    for (var i = 0; i < records.length; i++) {
        if (records[i].get('FROMBILLNO') == info.FrombillNo && records[i].get('FROMROWID') == info.FromRowId &&
        records[i].get('CONTACTOBJECTID') == info.ContactObjectId && records[i].get('MATERIALID') == info.MaterialId &&
        records[i].get('UNITID') == info.UnitId && records[i].get('BATCHNO') == info.BatchNo &&
        records[i].get('SUBBATCHNO') == info.SubBatchNo && records[i].get('MTONO') == info.MTONo &&
        records[i].get('PRICE') == info.Price) {
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
                var personId = this.dataSet.getTable(0).data.items[0].data["PERSONID"];//还料人员ID
                if (personId == "" || personId == undefined) {
                    alert("还料人员不能为空！");
                    return;
                }
                else {
                    var personId = proto.personId;
                    var data = this.invorkBcf('GetData', [personId]);
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
                    list.push({
                        FrombillNo: bodyTable.data.items[i].data["FROMBILLNO"],//来源借料单号
                        FromRowId: bodyTable.data.items[i].data["FROMROWID"],// 来源行标识
                        WarehouseId: bodyTable.data.items[i].data["WAREHOUSEID"],// 仓库 空
                        WarehouseName: bodyTable.data.items[i].data["WAREHOUSENAME"],// 仓库名称
                        CompanyId: bodyTable.data.items[i].data["COMPANYID"],// 隶属公司
                        CompanyName: bodyTable.data.items[i].data["COMPANYNAME"],// 公司名称
                        ContactObjectId: bodyTable.data.items[i].data["CONTACTOBJECTID"],// 往来单位
                        ContactsObjectName: bodyTable.data.items[i].data["CONTACTSOBJECTNAME"],// 往来单位名称
                        MaterialId: bodyTable.data.items[i].data["MATERIALID"],// 物料ID
                        MaterialName: bodyTable.data.items[i].data["MATERIALNAME"],// 物料名称
                        AttributeId: bodyTable.data.items[i].data["ATTRIBUTEID"],// 特征
                        AttributeName: bodyTable.data.items[i].data["ATTRIBUTENAME"],// 特征名称
                        AttributeCode: bodyTable.data.items[i].data["ATTRIBUTECODE"],// 特征标识
                        AttributeDesc: bodyTable.data.items[i].data["ATTRIBUTEDESC"],// 特征描述
                        UnitId: bodyTable.data.items[i].data["UNITID"],// 基本单位
                        UnitName: bodyTable.data.items[i].data["UNITNAME"],// 基本单位名称
                        StkAttr: bodyTable.data.items[i].data["STKATTR"],// 存货属性
                        StkState: bodyTable.data.items[i].data["STKSTATE"],// 库存状态
                        StkStateName: bodyTable.data.items[i].data["STKSTATENAME"],// 库存状态名称
                        BatchNo: bodyTable.data.items[i].data["BATCHNO"],// 批号
                        SubBatchNo: bodyTable.data.items[i].data["SUBBATCHNO"],// 小批号
                        CompleteNo: bodyTable.data.items[i].data["COMPLETENO"],// 完工标识号
                        MTONo: bodyTable.data.items[i].data["MTONO"],// MTO号
                        StorageId: bodyTable.data.items[i].data["STORAGEID"],// 库位
                        StorageName: bodyTable.data.items[i].data["STORAGENAME"],// 库位名称
                        ReservedNo1: bodyTable.data.items[i].data["RESERVEDNO1"],// 预留一  空
                        ReservedNo2: bodyTable.data.items[i].data["RESERVEDNO2"],// 预留二  空
                        BorrowQuantity: bodyTable.data.items[i].data["BORROWQUANTITY"],// 借用数量
                        NeedReturnQuantity: bodyTable.data.items[i].data["NEEDRETURNQUANTITY"],// 待归还数量
                        BorrowTime: bodyTable.data.items[i].data["BORROWTIME"],// 单价
                        Price: bodyTable.data.items[i].data["PRICE"],// 单价
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
