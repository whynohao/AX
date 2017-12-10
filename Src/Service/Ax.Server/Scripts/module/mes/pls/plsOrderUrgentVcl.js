plsOrderUrgentVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsOrderUrgentVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsOrderUrgentVcl;


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
                    var LastDate = this.dataSet.getTable(0).data.items[0].data['LASTDATE'];
                    var dt = this.dataSet.getTable(1);
                    var isExist = false;
                    for (var i = 0; i < dt.data.items.length; i++) {
                        if (BillNo == dt.data.items[i].data["BILLNO"]) {
                            isExist = true;
                            break;
                        }
                    }
                    if (BillNo == '' || BillNo == null) {
                        Ext.Msg.alert("提示", '请选择要加急的订单！');
                        break;
                    }
                    if (isExist) {
                        Ext.Msg.alert("提示", '该订单已存在订单加急明细中！');
                        break;
                    }
                    var returnData = this.invorkBcf("GetData", [BillNo]);
                    InsertOrderUrgentData.call(this, returnData);
                    Ext.getCmp("BILLNO0_" + this.winId).setValue(e.dataInfo.value);
                    Ext.getCmp("FROMBILLNO0_" + this.winId).setValue(e.dataInfo.value);
                    this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                    break;
                case "BtnClear":
                    this.deleteAll(1);
                    break;
                case "BtnUrgent":
                    var arr = [];
                    var dt = this.dataSet.getTable(1);
                    for (var i = 0; i < dt.data.items.length; i++) {
                        if (dt.data.items[i].data["LASTDATE"] <= dt.data.items[i].data["SINGLEDATE"]) {
                            Ext.Msg.alert("提示", "销售订单号【" + dt.data.items[i].data["BILLNO"] + "】最迟发货日期必须大于接单日期！");
                            return;
                        }
                        if (dt.data.items[i].data["LASTDATE"] >= dt.data.items[i].data["LASTESTDATE"]) {
                            Ext.Msg.alert("提示", "销售订单号【" + dt.data.items[i].data["BILLNO"] + "】最迟发货日期必须小于原计划最迟发货日期！");
                            return;
                        }
                        if (dt.data.items[i].data["ABNORMALREASONID"] == '') {
                            Ext.Msg.alert("提示", "销售订单号【" + dt.data.items[i].data["BILLNO"] + "】必须选择加急类型！");
                            return;
                        }
                        arr.push({
                            BillNo: dt.data.items[i].data["BILLNO"],
                            FromBillNo: dt.data.items[i].data["FROMBILLNO"],
                            SingleDate: dt.data.items[i].data["SINGLEDATE"],
                            LastestDate: dt.data.items[i].data["LASTESTDATE"],
                            LastDate: dt.data.items[i].data["LASTDATE"],
                            ReasonId: dt.data.items[i].data["ABNORMALREASONID"],
                            RecordFrom: 1,   // 表示手工添加的记录
                            OrderProgress: dt.data.items[i].data["ORDERPROGRESS"]
                        });

                    }
                    if (arr.length == 0) {
                        Ext.Msg.alert("提示", "没有需要加急的订单！");
                        return;
                    }
                    var returnData = this.invorkBcf("UrgentData", [arr]);
                    if (returnData == "成功") {
                        Ext.Msg.alert("提示", "成功！");
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
function InsertOrderUrgentData(returnData) {
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
            newRow.set('LASTDATE', masterRow.data["LASTDATE"]);
            newRow.set('ORDERPROGRESS', info.OrderProgress);
        }
    }
    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}