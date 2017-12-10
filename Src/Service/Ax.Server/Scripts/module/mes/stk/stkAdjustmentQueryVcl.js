/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

stkAdjustmentQueryVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkAdjustmentQueryVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkAdjustmentQueryVcl;

//调用datafuc的时候会调用此方法，可以初始化一些参数。
proto.companyId = "";
//proto.contactsObjectName = "";
proto.winId = "";
proto.fromObj = null;
proto.doSetParam = function (vclObj) {
    proto.companyId = vclObj[0];
    //proto.contactsObjectId = vclObj[1];
    proto.winId = vclObj[1].winId;
    proto.fromObj = vclObj[1];
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set("COMPANYID", proto.companyId);
    //masterRow.set("CONTACTOBJECTID", proto.contactsObjectId);
    this.forms[0].loadRecord(masterRow);
};
function fillStkAdjustmentQuery(returnData) {
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
                newRow.set('FROMBILLNO', info.FrombillNo);//来源出入库单据记录单号
                newRow.set('FROMROWID', info.FromRowId);// 来源行标识
                newRow.set('WAREHOUSEID', info.WarehouseId);// 仓库
                newRow.set('WAREHOUSENAME', info.WarehouseName);// 仓库名称
                newRow.set('ORGID', info.OrgId);//隶属公司
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
                newRow.set('STKATTR', info.StkAttr);// 存货属性  空
                newRow.set('STKSTATE', info.StkState);// 库存状态  空
                newRow.set('STKSTATENAME', info.StkStateName);// 库存状态名称  空
                newRow.set('BATCHNO', info.BatchNo);// 批号
                newRow.set('SUBBATCHNO', info.SubBatchNo);// 小批号
                newRow.set('COMPLETENO', info.CompleteNo);// 完工标识号  空
                newRow.set('MTONO', info.MTONo);// MTO号
                newRow.set('STORAGEID', info.StorageId);// 库位  空
                newRow.set('STORAGENAME', info.StorageName);// 库位名称  空
                newRow.set('RESERVEDNO1', info.ReservedNo1);// 预留一  空
                newRow.set('RESERVEDNO2', info.ReservedNo2);// 预留二  空
                newRow.set('QUANTITY', info.Quantity);// 数量
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
    debugger;
    var grid = Ext.getCmp(proto.winId + 'STKADJUSTMENTDETAILGrid')
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
                //masterRow.set('FROMBILLNO', info.FrombillNo);
                var ctrl3 = Ext.getCmp("FROMBILLNO0_" + proto.winId);
                ctrl3.store.add({ Id: info.FrombillNo, Name: "" });//, Name: "" 
                ctrl3.select(info.FrombillNo);
                // console.info(proto.winId);
                // console.info(ctrl3);
                proto.fromObj.forms[0].updateRecord(masterRow);

                var newRow = proto.fromObj.addRow(masterRow, 1);
                //newRow.set('TASKNO', info.TaskNo);//任务号
                newRow.set('FROMBILLNO', info.FrombillNo);//来源出入库单据记录单号
                newRow.set('FROMROWID', info.FromRowId);// 来源行标识
                newRow.set('WAREHOUSEID', info.WarehouseId);// 仓库
                newRow.set('WAREHOUSENAME', info.WarehouseName);// 仓库名称 
                newRow.set('COMPANYID', info.OrgId);//隶属公司
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
                newRow.set('STKATTR', info.StkAttr);// 存货属性  空
                newRow.set('STKSTATE', info.StkState);// 库存状态  空
                newRow.set('STKSTATENAME', info.StkStateName);// 库存状态名称  空
                newRow.set('BATCHNO', info.BatchNo);// 批号
                newRow.set('SUBBATCHNO', info.SubBatchNo);// 小批号
                newRow.set('COMPLETENO', info.CompleteNo);// 完工标识号  空
                newRow.set('MTONO', info.MTONo);// MTO号
                newRow.set('STORAGEID', info.StorageId);// 库位  空
                newRow.set('STORAGENAME', info.StorageName);// 库位名称  空
                newRow.set('RESERVEDNO1', info.ReservedNo1);// 预留一  空
                newRow.set('RESERVEDNO2', info.ReservedNo2);// 预留二  空
                newRow.set('QUANTITY', info.Quantity);// 数量
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

//判断出入库单据记录单中是否已经存在当前返填写过去的物料
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
                var companyId = this.dataSet.getTable(0).data.items[0].data["COMPANYID"];//公司ID
                var contactObjectId = this.dataSet.getTable(0).data.items[0].data["CONTACTOBJECTID"];//往来单位ID
                var fromBillNo = this.dataSet.getTable(0).data.items[0].data["FROMBILLNO"];//出入库单据单号
                //var fromRowId = this.dataSet.getTable(0).data.items[0].data["FROMROWID"];//出入库单据单行标识
                var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'];//物料ID
                var materialTypeId = this.dataSet.getTable(0).data.items[0].data['MATERIALTYPEID'];//物料类别ID
                if (companyId == "" || companyId == undefined) {
                    alert("公司不能为空！");
                    return;
                }
                    //if (contactObjectId == "" || contactObjectId == undefined) {
                    //    alert("往来单位不能为空！");
                    //    return;
                    //}
                    //else if (materialId == "" && materialTypeId == "") {
                    //    alert("物料与物料类别请选择其中一个进行查询！");
                    //    return;
                    //}
                else {
                    var companyId = proto.companyId;
                    var contactObjectId = this.dataSet.getTable(0).data.items[0].data["CONTACTOBJECTID"] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data["CONTACTOBJECTID"];
                    var fromBillNo = this.dataSet.getTable(0).data.items[0].data["FROMBILLNO"] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data["FROMBILLNO"];
                    //var fromRowId = this.dataSet.getTable(0).data.items[0].data['FROMROWID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['FROMROWID'];
                    //var checkBillNo = this.dataSet.getTable(0).data.items[0].data['CHECKBILLNO'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['CHECKBILLNO'];
                    var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['MATERIALID'];
                    var materialTypeId = this.dataSet.getTable(0).data.items[0].data['MATERIALTYPEID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['MATERIALTYPEID'];
                    //if (fromRowId == 0) {
                    //    fromRowId = "";
                    //}
                    //this.win.close();
                    var data = this.invorkBcf('GetData', [companyId, contactObjectId, fromBillNo, materialId, materialTypeId]);
                    fillStkAdjustmentQuery.call(this, data);
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
                        //TaskNo: bodyTable.data.items[i].data["TASKNO"],//任务号
                        FrombillNo: bodyTable.data.items[i].data["FROMBILLNO"],//来源销售出库单号
                        FromRowId: bodyTable.data.items[i].data["FROMROWID"],// 来源行标识
                        WarehouseId: bodyTable.data.items[i].data["WAREHOUSEID"],// 仓库
                        WarehouseName: bodyTable.data.items[i].data["WAREHOUSENAME"],// 仓库名称  
                        OrgId: bodyTable.data.items[i].data["ORGID"],// 仓库名称 
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
                        StkAttr: bodyTable.data.items[i].data["STKATTR"],// 存货属性  空
                        StkState: bodyTable.data.items[i].data["STKSTATE"],// 库存状态  空
                        StkStateName: bodyTable.data.items[i].data["STKSTATENAME"],// 库存状态名称  空
                        BatchNo: bodyTable.data.items[i].data["BATCHNO"],// 批号
                        SubBatchNo: bodyTable.data.items[i].data["SUBBATCHNO"],// 小批号
                        CompleteNo: bodyTable.data.items[i].data["COMPLETENO"],// 完工标识号  空
                        MTONo: bodyTable.data.items[i].data["MTONO"],// MTO号
                        StorageId: bodyTable.data.items[i].data["STORAGEID"],// 库位  空
                        StorageName: bodyTable.data.items[i].data["STORAGENAME"],// 库位名称  空
                        ReservedNo1: bodyTable.data.items[i].data["RESERVEDNO1"],// 预留一  空
                        ReservedNo2: bodyTable.data.items[i].data["RESERVEDNO2"],// 预留二  空
                        Quantity: bodyTable.data.items[i].data["QUANTITY"],// 数量
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
