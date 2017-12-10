/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

ppWorkOrderVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = ppWorkOrderVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = ppWorkOrderVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.AddRow:
            if (e.dataInfo.tableIndex == 2) {
                var store = this.dataSet.getTable(2);
                var length = store.data.items.length;
                var maxWp = 0;
                for (var i = 0; i < length; i++) {
                    var workprocessNo = store.data.items[i].get('WORKPROCESSNO');
                    if (maxWp < workprocessNo)
                        maxWp = workprocessNo;
                }
                e.dataInfo.dataRow.set('WORKPROCESSNO', (maxWp + 5));
                GetTransferWorkprocessNo.call(this);
            }
            break;
        case LibEventTypeEnum.DeleteRow:
            if (e.dataInfo.tableIndex == 2)
                GetTransferWorkprocessNo.call(this);
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnManagerPerson") {
                var billNo = this.dataSet.getTable(0).data.items[0].data['BILLNO'];
                Ax.utils.LibVclSystemUtils.openDataFunc('pp.ManagerPerson', '工单人员维护', billNo);
            }
            else if (e.dataInfo.fieldName == "BtnBarcodePrint") {
                if (!this.isEdit) {
                    var frombillno = this.dataSet.getTable(0).data.items[0].data["BILLNO"];
                    Ax.utils.LibVclSystemUtils.openDataFunc('stk.BarcodePrintDataFunc', "条码打印", [2, frombillno, this.tpl.ProgId, this.tpl.DisplayText]);
                }
                else {
                    Ext.Msg.alert("提示", "编辑状态下不可操作！");
                }
            }
            else if (e.dataInfo.fieldName == "BtnProductionOut") {
                if (!this.isEdit) {
                    var workOrderNo = this.dataSet.getTable(0).data.items[0].data["BILLNO"];
                    var materialId = this.dataSet.getTable(0).data.items[0].data["MATERIALID"];                    
                    var currentState = this.dataSet.getTable(0).data.items[0].data["CURRENTSTATE"];
                    if (currentState != "2") {
                        Ext.Msg.alert("提示", "单据生效才能操作！");
                    } else {
                        var billNoList = this.invorkBcf('BuildProductionOut', [workOrderNo, materialId]);
                        if (billNoList != null) {
                            for (var i = 0; i < billNoList.length; i++) {
                                var curPks = [];
                                var billNo = billNoList[i];
                                curPks.push(billNo);
                                Ax.utils.LibVclSystemUtils.openBill('stk.ProductionOut', BillTypeEnum.Bill, "生产领料单", BillActionEnum.Browse, undefined, curPks);
                            }
                        }
                    }
                }
                else {
                    Ext.Msg.alert("提示", "编辑状态下不可操作！");
                }
            }
            break;
        case LibEventTypeEnum.Validating:
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "TECHROUTEID") {
                    var TechrouteId = e.dataInfo.dataRow.get('TECHROUTEID');
                    if (TechrouteId !== "") {
                        var data = this.invorkBcf('GetTechrouteDataInfo', [TechrouteId]);
                        TechrouteDataInfo.call(this, data);
                        var list = this.invorkBcf('GetMaterialCheckItemList', [TechrouteId]);
                        FillCheckItemList.call(this, list);
                    }
                }
            }
            else if (e.dataInfo.tableIndex == 2) {
                if (e.dataInfo.fieldName == 'WORKPROCESSNO') {
                    var store = this.dataSet.getTable(2);
                    var length = store.data.items.length;
                    for (var i = 0; i < length; i++) {
                        if (e.dataInfo.dataRow.get('ROW_ID') != store.data.items[i].get('ROW_ID')) {
                            if (e.dataInfo.value == store.data.items[i].get('WORKPROCESSNO')) {
                                store.data.items[i].set('WORKPROCESSNO', e.dataInfo.oldValve);
                                break;
                            }
                        }
                    }
                    e.dataInfo.dataRow.set('WORKPROCESSNO', e.dataInfo.value);
                    GetTransferWorkprocessNo.call(this);
                }
            }
            break;
    }
}

proto.checkFieldValue = function (curRow, returnValue, tableIndex, fieldName) {
    Ax.vcl.LibVclData.prototype.checkFieldValue.apply(this, arguments);
    if (tableIndex == 0) {  
        switch (fieldName) {
            case 'BOMID':
                getBOMInfo.call(this, returnValue);
                break;
        }
    }
};

//加载BOM明细
var getBOMInfo = function (returnValue) {

    Ext.suspendLayouts();
    var curStore = this.dataSet.getTable(2);
    curStore.suspendEvents();//关闭store事件
    try{
        var list = returnValue['BOMInfo'];
        this.dataSet.getTable(1).removeAll();
        if (list !== undefined && list.length > 0) {
            var grid = Ext.getCmp(this.winId + 'PPBOMDATAGrid');
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set('SUBMATERIALID', info.SubMaterialId);
                newRow.set('SUBMATERIALNAME', info.SubMaterialName);
                newRow.set('SUBMATERIALSPEC', info.SubMaterialSpec);
                newRow.set('UNITID', info.UnitId);
                newRow.set('UNITNAME', info.UnitName);
                newRow.set('ISKEY', info.IsKey);
                newRow.set('MATSTYLE', info.MatStyle);
                newRow.set('SUPPLYTYPE', info.SupplyType);
                newRow.set('BASEQTY', info.BaseQty);
                newRow.set('UNITQTY', info.UnitQty);
                newRow.set('PARENTMATID', info.ParentmatId);
                newRow.set('PARENTMATNAME', info.ParentmatName);
                newRow.set('BOMLEVEL', info.BOMLevel);
                newRow.set('ISSEMIFINISHED', info.IsSemifinished);
                newRow.set('TECHROUTEID', info.TechrouteId);
                newRow.set('TECHROUTENAME', info.TechrouteName);
                newRow.set('FROMROWID', info.FromrowId);
                newRow.set('WORKSHOPSECTIONID', info.WorkshopSectionId);
                newRow.set('WORKSHOPSECTIONNAME', info.WorkshopSectionName);
                newRow.set('WORKPROCESSNO', info.WorkProcessNo);
                newRow.set('WORKPROCESSID', info.WorkProcessId);
                newRow.set('WORKPROCESSNAME', info.WorkProcessName);
            }
        }
    } finally {
        curStore.resumeEvents();
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);
    }

}

//赋值工艺路线信息表
var TechrouteDataInfo = function (data) {
    Ext.suspendLayouts();
    var curStore = this.dataSet.getTable(2);
    curStore.suspendEvents();//关闭store事件
    try {
        var list = data;
        this.dataSet.getTable(3).removeAll();
        this.dataSet.getTable(2).removeAll();
        if (list !== undefined && list.length > 0) {
            var grid = Ext.getCmp(this.winId + 'PPTECHROUTEDATAGrid');
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set('FROMROWID', info.FromRowId);
                newRow.set('NEEDGATHER', info.NeedGather);
                newRow.set('WORKPROCESSNO', info.WorkProcessNo);
                newRow.set('WORKSHOPSECTIONID', info.WorkshopSectionId);
                newRow.set('WORKSHOPSECTIONNAME', info.WorkshopSectionName);
                newRow.set('WORKPROCESSID', info.WorkProcessId);
                newRow.set('WORKPROCESSNAME', info.WorkProcessName);
                newRow.set('WORKSTATIONCONFIGID', info.WorkstationConfigId);
                newRow.set('WORKSTATIONCONFIGNAME', info.WorkstationConfigName);
                newRow.set('TRANSFERWORKPROCESSNO', info.TransferWorkProcessNo);
                newRow.set('DOWORKPROCESS', info.DoWorkProcess);
                newRow.set('WORKSTATIONDETAIL', info.WorkstationDetail);
                newRow.set('ISOUTSOURCING', info.IsOutsourcing);
                //站点明细
                var workstationConfig = info.WorkstationConfig;
                if (info.WorkstationDetail && workstationConfig.length > 0) {
                    for (var j = 0; j < workstationConfig.length; j++) {
                        var subInfo = workstationConfig[j];
                        var subRow = this.addRow(newRow, 3);
                        subRow.set('WORKSTATIONID', subInfo.WorkstationId);
                        subRow.set('WORKSTATIONNAME', subInfo.WorkstationName);
                    }
                }
            }
        }
    } finally {
        curStore.resumeEvents();
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);
    }
}

//赋值工艺路线信息表
var FillCheckItemList = function (data) {
    Ext.suspendLayouts();
    var curStore = this.dataSet.getTable(5);
    curStore.suspendEvents();//关闭store事件
    try {
        var list = data;
        this.dataSet.getTable(5).removeAll();
        if (list !== undefined && list.length > 0) {
            var grid = Ext.getCmp(this.winId + 'PPWORKORDERCHECKITEMGrid');
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set('FROMROWID', info.FromRowId);
                newRow.set('WORKPROCESSID', info.WorkProcessId);
                newRow.set('WORKPROCESSNAME', info.WorkProcessName);
                newRow.set('CHECKITEMID', info.CheckItemId);
                newRow.set('CHECKITEMNAME', info.CheckItemName);
                newRow.set('CHECKITEMTYPE', info.CheckItemType);
                newRow.set('UPLIMIT', info.UpLimit);
                newRow.set('LOWLIMIT', info.LowLimit);
                newRow.set('STANDARD', info.Standard);
                newRow.set('CHECKSIGN', info.CheckSign);
                newRow.set('DEFECTID', info.DefectId);
                newRow.set('DEFECTNAME', info.DefectName);
                newRow.set('PICLOCATE', info.PicLocate);
            }
        }
    } finally {
        curStore.resumeEvents();
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);
    }
}


var GetTransferWorkprocessNo = function () {
    var items = this.dataSet.getTable(2).data.items;
    var list = [];
    for (var i = 0; i < items.length; i++) {
        list.push(items[i].get('WORKPROCESSNO'));
    }
    list.sort(function (a, b) { return a - b });
    list.push(0);
    for (var i = 0; i < list.length - 1; i++) {
        for (var j = 0; j < items.length; j++) {
            if (list[i] == items[j].get('WORKPROCESSNO')) {
                items[j].set('TRANSFERWORKPROCESSNO', list[i + 1]);
            }
        }
    }
}