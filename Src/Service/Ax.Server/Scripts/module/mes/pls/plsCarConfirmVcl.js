plsCarConfirmVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsCarConfirmVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsCarConfirmVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    var bodyTable = this.dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
        case LibEventTypeEnum.BeforeDeleteRow:
            e.dataInfo.cancel = true;
            break;


        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
            }

            break;
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "BtnSelect":
                    var logId = this.dataSet.getTable(0).data.items[0].data['LOGISTICSCOMPANYID'];
                    var Date = this.dataSet.getTable(0).data.items[0].data['SENDDATE'];
                    if (logId == '') {
                        Ext.Msg.alert("提示", '请选择物流');
                        return;
                    }
                    if (Date <= 0) {
                        Ext.Msg.alert("提示", '请选择日期！');
                        return;
                    }
                  
                    var LogData = this.invorkBcf("GetLogData", [logId, Date]);
                    if (LogData.length == 0) {
                        Ext.Msg.alert("提示", '查询发货结果为空！');
                        this.deleteAll(1);
                        return;
                    }
                    else {
                        fillLogData.call(this, LogData);
                    }
                    var carData = this.invorkBcf("GetCarData", [logId]);
                    if (carData.length == 0) {
                        Ext.Msg.alert("提示", '查询车辆结果为空！');
                        this.deleteAll(2);
                        return;
                    }
                    else {
                        fillCarData.call(this, carData);
                    }

                    break;
                case "BtnConfirm":
                    var arr = [];
                    var Date = this.dataSet.getTable(0).data.items[0].data['SENDDATE'];
                    var logId = this.dataSet.getTable(0).data.items[0].data['LOGISTICSCOMPANYID'];
                    var dt = this.dataSet.getTable(2);
                    for (var i = 0; i < dt.data.items.length; i++) {
                        if (dt.data.items[i].data["QUANTITY"] > 0) {
                            arr.push({ CarId: dt.data.items[i].data["CARID"], Quantity: dt.data.items[i].data["QUANTITY"] });
                        }
                    }
                    if (arr.length == 0) {
                        Ext.Msg.alert("提示", "确认明细为空！");
                        return;
                    }
                    var returnData = this.invorkBcf("ConfirmData", [arr, Date, logId]);
                    if (returnData == "成功") {
                        Ext.Msg.alert("提示", "成功！");
                        this.deleteAll(1);
                        this.deleteAll(2);
                    }
                    else {
                        Ext.Msg.alert("提示", returnData);
                    }
                    break;
            }
            break;
    }
}

function fillLogData(returnData) {
    Ext.suspendLayouts();
    var formStore = this.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        this.deleteAll(1);
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                if (!info.Fix) {
                    var newRow = this.addRow(masterRow, 1);
                    newRow.set('ROW_ID', i + 1);
                    newRow.set('ROWNO', i + 1);
                    newRow.set('PLANPRODUCEDATE', info.PLANPRODUCEDATE);
                    newRow.set('SENDDATE', info.SENDDATE);
                    newRow.set('SALESORDERNO', info.SALESORDERNO);
                    newRow.set('SINGLEDATE', info.SINGLEDATE);
                    newRow.set('LASTESTDATE', info.LASTESTDATE);
                    newRow.set('LOGISTICSCOMPANYID', info.LOGISTICSCOMPANYID);
                    newRow.set('LOGISTICSCOMPANYNAME', info.LOGISTICSCOMPANYNAME);
                    newRow.set('CUSTOMERID', info.CUSTOMERID);
                    newRow.set('CUSTOMERNAME', info.CUSTOMERNAME);
                    newRow.set('BILLFROMID', info.BILLFROMID);
                    newRow.set('BILLFROMNAME', info.BILLFROMNAME);
                    newRow.set('LOTNO', info.LOTNO);
                    newRow.set('TREESPECIES', info.TREESPECIES);
                    newRow.set('PAINTTYPE', info.PAINTTYPE);
                    newRow.set('MATERIALTYPEID1', info.MATERIALTYPEID1);
                    newRow.set('MATERIALTYPEID2', info.MATERIALTYPEID2);
                    newRow.set('MATERIALTYPEID3', info.MATERIALTYPEID3);
                    newRow.set('MATERIALTYPEID4', info.MATERIALTYPEID4);
                    newRow.set('MATERIALTYPEID5', info.MATERIALTYPEID5);
                    newRow.set('MATERIALTYPEID6', info.MATERIALTYPEID6);
                    newRow.set('OTHERTYPEID', info.OTHERTYPEID);
                    newRow.set('QUANTITY', info.QUANTITY);
                    newRow.set('REMARK', info.REMARK);
                }
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


function fillCarData(returnData) {
    Ext.suspendLayouts();
    var formStore = this.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        this.deleteAll(2);
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                if (!info.Fix) {
                    var newRow = this.addRow(masterRow, 2);
                    newRow.set('ROW_ID', i + 1);
                    newRow.set('ROWNO', i + 1);
                    newRow.set('CARID', info.CARID);
                    newRow.set('CARNAME', info.CARNAME);
                }
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