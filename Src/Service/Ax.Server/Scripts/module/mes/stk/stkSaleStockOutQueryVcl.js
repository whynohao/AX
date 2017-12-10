/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

stkSaleStockOutQueryVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkSaleStockOutQueryVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkSaleStockOutQueryVcl;

var btnSelectAll = 0;//记录全选还是不选
//调用datafuc的时候会调用此方法，可以初始化一些参数。
proto.winId = "";
var winId = "";
proto.fromObj = null;
fromObj = null;
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    winId = vclObj[0].winId;
    proto.fromObj = vclObj[0];
    fromObj = vclObj[0];
    var masterRow = this.dataSet.getTable(0).data.items[0];
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
                newRow.set('CONTRACTNO', info.ContractNo);//合同号
                newRow.set('SALEORDERNO', info.SaleOrderNo);//来源生产领料出库单号
                newRow.set('SALEORDERROWID', info.SaleOrderRowId);// 来源行标识
                newRow.set('WAREHOUSEID', info.WarehouseId);// 仓库 空
                newRow.set('WAREHOUSENAME', info.WarehouseName);// 仓库名称  空
                newRow.set('CONTACTOBJECTID', info.ContactObjectId);// 往来单位
                newRow.set('CONTACTSOBJECTNAME', info.ContactsObjectName);// 往来单位名称
                newRow.set('MATERIALID', info.MaterialId);// 物料ID
                newRow.set('MATERIALNAME', info.MaterialName);// 物料名称
                newRow.set('ATTRIBUTEID', info.AttributeId);// 特征
                newRow.set('ATTRIBUTENAME', info.AttributeName);// 特征名称
                newRow.set('ATTRIBUTECODE', info.AttributeCode);// 特征标识
                newRow.set('ATTRIBUTEDESC', info.AttributeDesc);// 特征描述
                newRow.set('SPECIFICATION', info.Specification);// 规格
                newRow.set('MATERIALSPEC', info.MaterialSpec);// 物料描述
                newRow.set('TEXTUREID', info.TextureId);// 标识
                newRow.set('FIGURENNO', info.FigurenNo);// 图号
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
    var grid = Ext.getCmp(winId + 'STKSALESTOCKOUTDETAILGrid')
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
                //newRow.set('TASKNO', info.TaskNo);//任务号
                newRow.set('SALEORDERNO', info.SaleOrderNo);//销售订单
                newRow.set('SALEORDERROWID', info.SaleOrderRowId);// 销售订单行标识
                newRow.set('WAREHOUSEID', info.WarehouseId);// 仓库 空
                newRow.set('WAREHOUSENAME', info.WarehouseName);// 仓库名称  空
                newRow.set('CONTACTOBJECTID', info.ContactObjectId);// 往来单位
                newRow.set('CONTACTSOBJECTNAME', info.ContactsObjectName);// 往来单位名称
                newRow.set('MATERIALID', info.MaterialId);// 物料ID
                newRow.set('MATERIALNAME', info.MaterialName);// 物料名称
                newRow.set('ATTRIBUTEID', info.AttributeId);// 特征
                newRow.set('ATTRIBUTENAME', info.AttributeName);// 特征名称
                newRow.set('ATTRIBUTECODE', info.AttributeCode);// 特征标识
                newRow.set('ATTRIBUTEDESC', info.AttributeDesc);// 特征描述
                newRow.set('SPECIFICATION', info.Specification);// 规格
                newRow.set('MATERIALSPEC', info.MaterialSpec);// 物料描述
                newRow.set('TEXTUREID', info.TextureId);// 标识
                newRow.set('FIGURENO', info.FigurenNo);// 图号
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

//判断销售出库库单中是否已经存在当前返填写过去的物料
function checkGetNotice(grid, info) {
    var k = 0;
    debugger;
    var records = grid.store.data.items;
    for (var i = 0; i < records.length; i++) {
        if (records[i].get('SALEORDERNO') == info.SaleOrderNo && records[i].get('SALEORDERROWID') == info.SaleOrderRowId &&
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

//判断是否选了来源单号相同的单据
function checkFromBillNo(list, info) {
    var k = 0;
    for (var i = 0; i < list.length; i++) {
        var listitem = list[i];
        if (info.SaleOrderNo != listitem.SaleOrderNo) {
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
                var fromBillNo = this.dataSet.getTable(0).data.items[0].data["FROMBILLNO"];//生产领料出库单号
                var fromRowId = this.dataSet.getTable(0).data.items[0].data["FROMROWID"];//采购收货单行标识
                var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'];//物料ID
                var materialTypeId = this.dataSet.getTable(0).data.items[0].data['MATERIALTYPEID'];//物料类别ID
                var fromBillNo = this.dataSet.getTable(0).data.items[0].data["FROMBILLNO"] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data["FROMBILLNO"]
                var fromRowId = this.dataSet.getTable(0).data.items[0].data['FROMROWID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['FROMROWID'];
                var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['MATERIALID'];
                var materialTypeId = this.dataSet.getTable(0).data.items[0].data['MATERIALTYPEID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['MATERIALTYPEID'];
                //if (fromRowId == 0) {
                //    fromRowId = "";
                //}
                //this.win.close();
                var data = this.invorkBcf('GetData', [fromBillNo, fromRowId, materialId, materialTypeId]);
                fillData.call(this, data);
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
                        list.push({
                            //TaskNo: bodyTable.data.items[i].data["TASKNO"],//任务号
                            ContractNo: bodyTable.data.items[i].data["CONTRACTNO"],//合同号
                            SaleOrderNo: bodyTable.data.items[i].data["SALEORDERNO"],//销售订单
                            SaleOrderRowId: bodyTable.data.items[i].data["SALEORDERROWID"],// 销售订单行标识
                            WarehouseId: bodyTable.data.items[i].data["WAREHOUSEID"],// 仓库 空
                            WarehouseName: bodyTable.data.items[i].data["WAREHOUSENAME"],// 仓库名称  空
                            MaterialId: bodyTable.data.items[i].data["MATERIALID"],// 物料ID
                            MaterialName: bodyTable.data.items[i].data["MATERIALNAME"],// 物料名称
                            AttributeId: bodyTable.data.items[i].data["ATTRIBUTEID"],// 特征
                            AttributeName: bodyTable.data.items[i].data["ATTRIBUTENAME"],// 特征名称
                            AttributeCode: bodyTable.data.items[i].data["ATTRIBUTECODE"],// 特征标识
                            AttributeDesc: bodyTable.data.items[i].data["ATTRIBUTEDESC"],// 特征描述
                            Specification: bodyTable.data.items[i].data["SPECIFICATION"],// 规格
                            MaterialSpec: bodyTable.data.items[i].data["MATERIALSPEC"],// 物料描述
                            TextureId: bodyTable.data.items[i].data["TEXTUREID"],// 标识
                            FigurenNo: bodyTable.data.items[i].data["FIGURENO"],// 图号
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
                }
                if (list.length > 0) {
                    var info = list[0];
                    if (checkFromBillNo(list, info)) {
                
                        //来源单
                        var FrombillNo = info.SaleOrderNo;
                        //合同号
                        var ContractNo = info.ContractNo;

                        var masterRow = fromObj.dataSet.getTable(0).data.items[0];
                        var masterFromBillNo = masterRow.get('SALEORDERNO');

                        if (masterFromBillNo != FrombillNo) {
                            fromObj.dataSet.getTable(1).removeAll();
                        }

                        fromObj.dataSet.getTable(0).data.items[0].set("SALEORDERNO", FrombillNo);
                        fromObj.dataSet.getTable(0).data.items[0].set("PRODUCTCONTRACTNO", ContractNo);
                        fromObj.forms[0].loadRecord(masterRow);
                        var field = Ext.getCmp('SALEORDERNO0_' + proto.winId);
                        field.store.add({ Id: FrombillNo, Name: '' });
                        field.select(FrombillNo);
                        //fromObj.forms[0].updateRecord(fromObj.dataSet.getTable(0).data.items[0]);

                        this.win.close();
                        fillStockInData.call(this, list);
                    }
                    else {
                        Ext.Msg.alert("提示", "请选择来源投产单号相同的数据");
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
