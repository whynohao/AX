/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

stkPurStockInQueryVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkPurStockInQueryVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkPurStockInQueryVcl;

var btnSelectAll = 0;//记录全选还是不选
//调用datafuc的时候会调用此方法，可以初始化一些参数。
proto.contactObjectId = "";
proto.contactsObjectName = "";
proto.winId = "";
proto.fromObj = null;
var winId = "";
var fromObj = null;
proto.doSetParam = function (vclObj) {
    proto.contactObjectId = vclObj[0];
    proto.contactsObjectName = vclObj[1];
    proto.winId = vclObj[2].winId;
    winId = vclObj[2].winId;
    proto.fromObj = vclObj[2];
    fromObj = vclObj[2];
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set("CONTACTOBJECTID", proto.contactObjectId);
    masterRow.set("CONTACTSOBJECTNAME", proto.contactsObjectName);
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
                newRow.set('ISCHOSE', 1);//选中
                newRow.set('FROMBILLNO', info.FrombillNo);//来源收货单号
                newRow.set('FROMROWID', info.FromRowId);// 来源行标识
                newRow.set('WAREHOUSEID', info.WarehouseId);// 仓库 空
                newRow.set('WAREHOUSENAME', info.WarehouseName);// 仓库名称  空
                newRow.set('WAREHOUSEPERSONID', info.WarehousePersonId);// 仓库管理员ID
                newRow.set('WAREHOUSEPERSONNAME', info.WarehousePersonName);// 仓库管理员名称
                newRow.set('COMPANYID', info.CompanyId);// 仓库管理员名称
                newRow.set('COMPANYNAME', info.CompanyName);// 仓库管理员名称
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
                newRow.set('STKUNITID', info.StkUnitId);//交易单位
                newRow.set('STKUNITNAME', info.StkUnitName);//交易单位名称
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
                newRow.set('QUANTITY', info.Quantity);// 应收数量
                newRow.set('CANDEALSQTY', info.Candealsqty);//交易数量
                newRow.set('REJECTIONQTY', info.RejectionQty);// 拒收数量
                newRow.set('PRICE', info.Price);// 单价
                newRow.set('ENDTIMEOFQUALITY', info.EndTimeOfQuality);//质保结束日期
                newRow.set('REMARK', info.Remark);//备注
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
    var grid = Ext.getCmp(winId + 'STKPURSTOCKINDETAILGrid')
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
                newRow.set('FROMBILLNO', info.FrombillNo);//来源收货单号
                newRow.set('FROMROWID', info.FromRowId);// 来源行标识
                newRow.set('WAREHOUSEID', info.WarehouseId);// 仓库 空
                newRow.set('WAREHOUSENAME', info.WarehouseName);// 仓库名称  空
                newRow.set('COMPANYID', info.CompanyId);// 仓库所属单位
                newRow.set('COMPANYNAME', info.CompanyName);// 仓库所属单位名称
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
                newRow.set('STKUNITID', info.StkUnitId);//交易单位
                newRow.set('STKUNITNAME', info.StkUnitName);//交易单位名称
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
                newRow.set('QUANTITY', info.Quantity);// 实收数量
                newRow.set('INVENTORYQTY', info.InventoryQty);// 实时库存数量
                newRow.set('DEALQUANTITY', info.Candealsqty);//交易数量
                newRow.set('REJECTIONQUANTITY', info.RejectionQty);// 实收数量
                newRow.set('RECEIVABLEQUANTITY', info.Quantity);//应收数量
                newRow.set('PRICE', info.Price);// 单价
                newRow.set('ENDTIMEOFQUALITY', info.EndTimeOfQuality);//质保结束日期
                newRow.set('REMARK', info.Remark);//备注
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

//判断采购收货单中是否已经存在当前返填写过去的物料
function checkGetNotice(grid, info) {
    var k = 0;
    var records = grid.store.data.items;
    for (var i = 0; i < records.length; i++) {
        if (records[i].get('FROMBILLNO') == info.FrombillNo && records[i].get('FROMROWID') == info.FromRowId &&
        records[i].get('CONTACTOBJECTID') == info.ContactObjectId && records[i].get('MATERIALID') == info.MaterialId &&
        records[i].get('UNITID') == info.UnitId && records[i].get('BATCHNO') == info.BatchNo &&
        records[i].get('SUBBATCHNO') == info.SubBatchNo && records[i].get('MTONO') == info.MTONo &&
        records[i].get('QUANTITY') == info.Quantity) {
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
        if (info.FrombillNo != listitem.FrombillNo) {
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
                var contactObjectId = this.dataSet.getTable(0).data.items[0].data["CONTACTOBJECTID"];//往来单位ID
                var fromBillNo = this.dataSet.getTable(0).data.items[0].data["FROMBILLNO"];//采购收货单号
                //var fromRowId = this.dataSet.getTable(0).data.items[0].data["FROMROWID"];//采购收货单行标识
                var checkBillNo = this.dataSet.getTable(0).data.items[0].data["CHECKBILLNO"];//质检单号
                var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'];//物料ID
                var materialTypeId = this.dataSet.getTable(0).data.items[0].data['MATERIALTYPEID'];//物料类别ID
                if (contactObjectId == "" || contactObjectId == undefined) {
                    Ext.Msg.alert("提示", "往来单位不能为空！");
                    break;
                }
                else if (fromBillNo == "" || fromBillNo == undefined) {
                    Ext.Msg.alert("提示", "采购通知单号不能为空！");
                    break;
                }
                else {
                    var contactObjectId = this.dataSet.getTable(0).data.items[0].data["CONTACTOBJECTID"];
                    var fromBillNo = this.dataSet.getTable(0).data.items[0].data["FROMBILLNO"] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data["FROMBILLNO"]
                    //var fromRowId = this.dataSet.getTable(0).data.items[0].data['FROMROWID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['FROMROWID'];
                    var checkBillNo = this.dataSet.getTable(0).data.items[0].data['CHECKBILLNO'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['CHECKBILLNO'];
                    var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['MATERIALID'];
                    var materialTypeId = this.dataSet.getTable(0).data.items[0].data['MATERIALTYPEID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['MATERIALTYPEID'];
                    //if (fromRowId == 0) {
                    //    fromRowId = "";
                    //}
                    //this.win.close();
                    var data = this.invorkBcf('GetData', [contactObjectId, fromBillNo, checkBillNo, materialId, materialTypeId]);
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
                debugger;
                var list = [];
                for (var i = 0; i < bodyTable.data.items.length; i++) {
                    if (bodyTable.data.items[i].data["ISCHOSE"] == true) {
                        list.push({
                                FrombillNo: bodyTable.data.items[i].data["FROMBILLNO"],//来源收货单号
                                FromRowId: bodyTable.data.items[i].data["FROMROWID"],// 来源行标识
                                WarehouseId: bodyTable.data.items[i].data["WAREHOUSEID"],// 仓库 空
                                WarehouseName: bodyTable.data.items[i].data["WAREHOUSENAME"],// 仓库名称  空
                                WarehousePersonId: bodyTable.data.items[i].data["WAREHOUSEPERSONID"],// 仓管员ID
                                WarehousePersonName: bodyTable.data.items[i].data["WAREHOUSEPERSONNAME"],// 仓管员名称
                                CompanyId: bodyTable.data.items[i].data["COMPANYID"],// 仓管员名称
                                CompanyName: bodyTable.data.items[i].data["COMPANYNAME"],// 仓管员名称
                                ContactObjectId: bodyTable.data.items[i].data["CONTACTOBJECTID"],// 往来单位
                                ContactsObjectName: bodyTable.data.items[i].data["CONTACTSOBJECTNAME"],// 往来单位名称
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
                                StkUnitId: bodyTable.data.items[i].data["STKUNITID"],//交易单位
                                StkUnitName: bodyTable.data.items[i].data["STKUNITNAME"],//交易单位名称
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
                                Quantity: bodyTable.data.items[i].data["QUANTITY"],// 应收数量 = 可入库数量
                                Candealsqty: bodyTable.data.items[i].data["CANDEALSQTY"],// 交易数量
                                RejectionQty: bodyTable.data.items[i].data["REJECTIONQTY"],// 拒收数量
                                Price: bodyTable.data.items[i].data["PRICE"],// 单价
                                EndTimeOfQuality: bodyTable.data.items[i].data["ENDTIMEOFQUALITY"],//质保结束日期
                                Remark: bodyTable.data.items[i].data["REMARK"]//备注
                        });
                }
            }
                if (list.length > 0) {
                    var info = list[0];
                    if (checkFromBillNo(list, info)) {

                        //往来对象
                        var contactsObjectId = info.ContactObjectId;
                        var contactsObjectName = info.ContactsObjectName;
                        fromObj.dataSet.getTable(0).data.items[0].set("CONTACTOBJECTID", contactsObjectId);
                        var field = Ext.getCmp('CONTACTOBJECTID0_' + proto.winId);
                        field.store.add({ Id: contactsObjectId, Name: contactsObjectName });
                        field.select(contactsObjectId);

                        //来源单
                        var FrombillNo = info.FrombillNo;

                        var masterRow = fromObj.dataSet.getTable(0).data.items[0];
                        var masterFromBillNo = masterRow.get('FROMBILLNO');

                        if (masterFromBillNo != FrombillNo) {
                            fromObj.dataSet.getTable(1).removeAll();
                    }


                        fromObj.dataSet.getTable(0).data.items[0].set("FROMBILLNO", FrombillNo);
                        var field = Ext.getCmp('FROMBILLNO0_' + proto.winId);
                        field.store.add({ Id: FrombillNo, Name: '' });
                        field.select(FrombillNo);

                        //仓库和仓管员
                        //masterRow.set("WAREHOUSEID", info.WarehouseId);
                        fromObj.dataSet.getTable(0).data.items[0].set("PERSONID", info.WarehousePersonId);
                        fromObj.dataSet.getTable(0).data.items[0].set("PERSONNAME", info.WarehousePersonName);
                        fromObj.forms[0].loadRecord(masterRow);
                        var field = Ext.getCmp('WAREHOUSEID0_' + proto.winId);
                        field.store.add({ Id: info.WarehouseId, Name: info.WarehouseName });
                        field.select(info.WarehouseId);

                        this.win.close();
                        fillStockInData.call(this, list);
                    }
                    else {
                        Ext.Msg.alert("提示", "请选择来源单号相同的数据");
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
                //case LibEventTypeEnum.Validated:
                //    if (e.dataInfo.fieldName == "CONTACTOBJECTID") {
                //        if (proto.contactObjectId != "" && proto.contactObjectId != null) {

                //            Ext.Msg.alert("前面表头已经选择了往来单位");
                //        }
                //    }
                //break;
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
