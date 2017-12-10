plsReturnOrderVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsReturnOrderVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsReturnOrderVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    var bodyTable = this.dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            e.dataInfo.cancel = true;
            break;

        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
            }

            break;
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "BtnInsert":
                    var BillNo = this.dataSet.getTable(0).data.items[0].data['BILLNO'];
                    var dt = this.dataSet.getTable(1);
                    var isExist = false;
                    for (var i = 0; i < dt.data.items.length; i++) {
                        if (BillNo == dt.data.items[i].data["BILLNO"]) {
                            isExist = true;
                            break;
                        }
                    }
                    if (BillNo == '' || BillNo == null) {
                        Ext.Msg.alert("提示", '请选择要退的订单！');
                        break;
                    }
                    if (isExist) {
                        Ext.Msg.alert("提示", '该订单已存在退单明细中！');
                        break;
                    }
                    var returnData = this.invorkBcf("GetData", [BillNo]);
                    InsertOrderReturnData.call(this, returnData);
                    Ext.getCmp("BILLNO0_" + this.winId).setValue(e.dataInfo.value);
                    Ext.getCmp("FROMBILLNO0_" + this.winId).setValue(e.dataInfo.value);
                    this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                    break;
                case "BtnClear":
                    this.deleteAll(1);
                    break;
                case "BtnBack":
                    var arr = [];
                    var dt = this.dataSet.getTable(1);
                    for (var i = 0; i < dt.data.items.length; i++) {
                        if (dt.data.items[i].data["ABNORMALREASONID"] == '') {
                            Ext.Msg.alert("提示", "销售订单号【" + dt.data.items[i].data["BILLNO"] + "】必须选择退单原因！");
                            return;
                        }
                        arr.push({
                            BillNo: dt.data.items[i].data["BILLNO"],
                            FromBillNo: dt.data.items[i].data["FROMBILLNO"],
                            WorkOrderDate: dt.data.items[i].data["STARTDATE"],
                            ReasonId: dt.data.items[i].data["ABNORMALREASONID"],
                            StateChange: 2,
                            IsStop: dt.data.items[i].data["ISSTOP"],
                            RecordFrom: 1,   // 表示手工添加的记录
                            OrderProgress: dt.data.items[i].data["ORDERPROGRESS"]
                        });

                    }
                    if (arr.length == 0) {
                        Ext.Msg.alert("提示", "没有需要退单的订单！");
                        break;
                    }
                    var returnData = this.invorkBcf("BackData", [arr]);
                    if (returnData.indexOf("成功") > -1) {
                        Ext.Msg.alert("提示", returnData);
                        this.deleteAll(1);
                    }
                    else {
                        Ext.Msg.alert("提示", returnData);
                    }
                    break;
                case "BtnConFirm":
                    var arr = [];
                    var dt = this.dataSet.getTable(1);
                    for (var i = 0; i < dt.data.items.length; i++) {
                        if (dt.data.items[i].data["ABNORMALREASONID"] == '') {
                            Ext.Msg.alert("提示", "销售订单号【" + dt.data.items[i].data["BILLNO"] + "】必须选择退单原因！");
                            return;
                        }
                        arr.push({
                            BillNo: dt.data.items[i].data["BILLNO"],
                            FromBillNo: dt.data.items[i].data["FROMBILLNO"],
                            WorkOrderDate: dt.data.items[i].data["STARTDATE"],
                            ReasonId: dt.data.items[i].data["ABNORMALREASONID"],
                            StateChange: 2,
                            IsStop: dt.data.items[i].data["ISSTOP"],
                            RecordFrom: 1,   // 表示手工添加的记录
                            OrderProgress: dt.data.items[i].data["ORDERPROGRESS"]
                        });

                    }
                    if (arr.length == 0) {
                        Ext.Msg.alert("提示", "没有需要退单的订单！");
                        break;
                    }
                    var returnData = this.invorkBcf("ConfirmBackData", [arr]);
                    if (returnData.indexOf("成功") > -1) {
                        Ext.Msg.alert("提示", returnData);
                        this.deleteAll(1);
                    }
                    else {
                        Ext.Msg.alert("提示", returnData);
                    }
                    break;
            }
            break;
    }
}

//填充数据到明细表中
function InsertOrderReturnData(returnData) {
    Ext.suspendLayouts();
    var formStore = this.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var info = returnData;
        if (info !== undefined) {
            var newRow = this.addRow(masterRow, 1);
            newRow.set('BILLNO', info.BillNo);
            newRow.set('FROMBILLNO', info.FromBillNo);
            newRow.set('SINGLEDATE', info.SingleDate);
            newRow.set('LASTESTDATE', info.LastestDate);
            newRow.set('CUSTOMERNAME', info.CustomerName);
            newRow.set('STARTDATE', info.WorkOrderDate);
            newRow.set('ABNORMALREASONID', masterRow.data["ABNORMALREASONID"]);
            newRow.set('ABNORMALREASONNAME', masterRow.data["ABNORMALREASONNAME"]);
            newRow.set("ORDERPROGRESS", info.OrderProgress);
        }
    }
    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}