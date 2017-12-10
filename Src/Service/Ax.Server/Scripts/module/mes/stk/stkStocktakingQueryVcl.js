/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

stkStocktakingQueryVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkStocktakingQueryVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkStocktakingQueryVcl;

//调用datafuc的时候会调用此方法，可以初始化一些参数。
proto.companyId = "";
proto.companyName = "";
proto.winId = "";
proto.fromObj = null;
proto.doSetParam = function (vclObj) {
    proto.companyId = vclObj[0];
    proto.companyName = vclObj[1];
    proto.winId = vclObj[2].winId;
    proto.fromObj = vclObj[2];
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set("COMPANYID", proto.companyId);
    masterRow.set("COMPANYNAME",proto.companyName);
    this.forms[0].loadRecord(masterRow);
};
function fillData(returnData) {
    var grid = Ext.getCmp(proto.winId + 'STKSTOCKTAKINGDETAILGrid')
    var list = returnData['StocktakingInfoList'];
    Ext.suspendLayouts();
    var curStore = grid.getStore();
    var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
    curStore.suspendEvents();
    try {
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                    if (checkGetNotice(grid, info))                     
                    {
                        continue;
                    }
                        var newRow = proto.fromObj.addRow(masterRow, 1);
                        //newRow.set('COMPANYID', info.CompanyId);
                        //newRow.set('COMPANYNAME',info.CompanyName);
                        newRow.set('MATERIALID', info.MaterialId);
                        newRow.set('MATERIALNAME', info.MaterialName);
                        newRow.set('CONTACTOBJECTID', info.ContactSobjectId);
                        newRow.set('CONTACTSOBJECTNAME', info.contactSobjectName);
                        newRow.set('UNITID', info.UnitId);
                        newRow.set('UNITNAME', info.UnitName);
                        newRow.set('WAREHOUSEID', info.WarehouseId);
                        newRow.set('WAREHOUSENAME', info.WarehouseName); 
                        newRow.set('COMPANYID', info.OrgId);
                        newRow.set('STORAGEID', info.StorageId);
                        newRow.set('STORAGENAME', info.StorageName);
                        newRow.set('BATCHNO', info.BatchNo);
                        newRow.set('SUBBATCHNO', info.SubBatchNo);
                        newRow.set('COMPLETENO', info.CompleteNo);
                        newRow.set('MTONO', info.MTONo);
                        newRow.set('RESERVEDNO1', info.ReservedNo1);
                        newRow.set('RESERVEDNO2', info.ReservedNo2);
                        newRow.set('ATTRIBUTECODE', info.AttributeCode);
                        newRow.set('STKSTATE', info.StkState);
                        newRow.set('STKATTR', info.StkAttr);
                        newRow.set('STKSTATENAME', info.StkStateName);
                        newRow.set('ACCOUNTQUANTITY', info.AccountQuantity);
                        newRow.set('STOCKQUANTITY', 0);
                        newRow.set('QUANTITY', 0);
                        var unitStocktakingInfo = info.UnitStocktakingInfo;
                        var serialStocktakingInfo = info.SerialStocktakingInfo;
                        if (unitStocktakingInfo && unitStocktakingInfo.length > 0) {
                            for (var r = 0; r < unitStocktakingInfo.length; r++) {
                                newRow.set("UNITDETAIL", 1);
                                var subInfo = unitStocktakingInfo[r];
                                var subRow = proto.fromObj.addRow(newRow, 2);
                                subRow.set('STKUNITID', subInfo.StockUnitId);
                                subRow.set('STKUNITNAME', subInfo.StockUnitName);
                                subRow.set('STKUNITNO', subInfo.StockUnitNo);
                                subRow.set('UNITACCOUNTSQUANTITY', subInfo.StockQuantity);
                                subRow.set('UNITACCOUNTQUANTITY', subInfo.Quantity);
                            }
                        }
                        if (serialStocktakingInfo && serialStocktakingInfo.length > 0) {
                            for (var j = 0; j < serialStocktakingInfo.length; j++) {
                                newRow.set("SERIALNODETAIL", 1);
                                var subInfo = serialStocktakingInfo[j];
                                var subRow = proto.fromObj.addRow(newRow, 3);
                                subRow.set('SERIALNO', subInfo.SerialNo);
                            }
                        }
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

//判断盘点单中是否已经存在当前返填写过去的物料
function checkGetNotice(grid, info) {
    var k = 0;
    var records = grid.store.data.items;
    for (var i = 0; i < records.length; i++) {
        if (records[i].get('WAREHOUSEID') == info.WarehouseId && records[i].get('MATERIALID') == info.MaterialId &&
        records[i].get('ATTRIBUTECODE') == info.AttributeCode && records[i].get('BATCHNO') == info.BatchNo &&
        records[i].get('SUBBATCHNO') == info.SubBatchNo && records[i].get('STORAGEID') == info.StorageId &&
        records[i].get('COMPLETENO') == info.CompleteNo && records[i].get('MTONO') == info.MTONo &&
        records[i].get('STKATTR') == info.StkAttr && records[i].get('STKSTATE') == info.StkState&&
        records[i].get('CONTACTOBJECTID') == info.ContactSobjectId) {
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
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnQueryData") {
                var warehouseId = this.dataSet.getTable(0).data.items[0].data["WAREHOUSEID"];
                var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'];
                var materialTypeId = this.dataSet.getTable(0).data.items[0].data['MATERIALTYPEID'];
                if (warehouseId == "" || warehouseId == undefined)
                {
                    alert("仓库不能为空！");
                    return;
                } 
                else if (materialId == "" && materialTypeId == "") {
                    alert("物料与物料类别请选择其中一个进行查询！");
                    return;
                }
                else {
                    var companyId = proto.companyId;
                    var warehouseId=this.dataSet.getTable(0).data.items[0].data["WAREHOUSEID"]==undefined?"":this.dataSet.getTable(0).data.items[0].data["WAREHOUSEID"]
                    var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['MATERIALID'];
                    var materialTypeId = this.dataSet.getTable(0).data.items[0].data['MATERIALTYPEID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['MATERIALTYPEID'];
                    var storageId = this.dataSet.getTable(0).data.items[0].data['STORAGEID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['STORAGEID'];
                    var batchNo = this.dataSet.getTable(0).data.items[0].data['BATCHNO'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['BATCHNO'];
                    var subBatchNo = this.dataSet.getTable(0).data.items[0].data['SUBBATCHNO'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['SUBBATCHNO'];
                    var stkState = this.dataSet.getTable(0).data.items[0].data['STKSTATE'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['STKSTATE'];
                    var stkAttr = this.dataSet.getTable(0).data.items[0].data['STKATTR'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['STKATTR'];
                    this.win.close();
                    var data = this.invorkBcf('GetData', [companyId,warehouseId, materialId, materialTypeId, storageId, batchNo, subBatchNo,stkState,stkAttr]);
                    fillData.call(this, data);
                }
            }
            break;
        case LibEventTypeEnum.Validated://form的赋值之后在this.dataSet中还没有实时的数据写入，需要写入之后，才能取到值。          
            if (e.dataInfo.fieldName == "WAREHOUSEID" || e.dataInfo.fieldName == "STORAGEID" || e.dataInfo.fieldName == "MATERIALID" || e.dataInfo.fieldName == "MATERIALTYPEID" || e.dataInfo.fieldName == "STKSTATE" || e.dataInfo.fieldName == "STKATTR" || e.dataInfo.fieldName == "BATCHNO" || e.dataInfo.fieldName == "SUBBATCHNO") {
                e.dataInfo.curForm.updateRecord(e.dataInfo.dataRow);
            }
            break;
    }
};
