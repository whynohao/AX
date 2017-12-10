plsOrderInsertVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsOrderInsertVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsOrderInsertVcl;


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
                case "BtnSelect":
                    var BillNo = this.dataSet.getTable(0).data.items[0].data['BILLNO'];
                    if (BillNo == '' || BillNo == null) {
                        Ext.Msg.alert("提示", "请选择订单！");
                        return;
                    }
                    var isExist = false;
                    for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                        if (BillNo == this.dataSet.getTable(1).data.items[i].data["BILLNO"]) {
                            isExist = true;
                            break;
                        }
                    }
                    if (isExist) {
                        Ext.Msg.alert("提示", '该订单已存在订单明细中！');
                        Ext.getCmp("BILLNO0_" + this.winId).setValue(e.dataInfo.value);
                        this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                        break;
                    }
                    var returnData = this.invorkBcf("GetData", [BillNo]);
                    fillOrderInsertData.call(this, returnData);
                    Ext.getCmp("BILLNO0_" + this.winId).setValue(e.dataInfo.value);
                    this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                    break;
                case "BtnInsert":
                    var INSERTDATE = this.dataSet.getTable(0).data.items[0].data['INSERTDATE'];
                    if (INSERTDATE <= 0) {
                        Ext.Msg.alert("提示", "请选择生产日期！");
                        return;
                    }
                    var list = [];
                    for (var i = 0; i < this.dataSet.getTable(1).data.items.length ; i++) {
                        list.push(this.dataSet.getTable(1).data.items[i].data['BILLNO']);
                    }
                    if (list.length == 0) {
                        Ext.Msg.alert("提示", "请选择要插入的订单！");

                    }
                    else {
                        var returnData = this.invorkBcf("InsertData", [list, INSERTDATE]);
                        Ext.Msg.alert("提示", returnData.MessageBox);
                        if (returnData.IsSuccess) {
                            this.deleteAll(1);
                            Ext.getCmp("BILLNO0_" + this.winId).setValue('');
                            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                        }
                    }
                    break;
            }
            break;
    }
}

function fillOrderInsertData(returnData) {
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
            newRow.set('LOTNO', info.LotNo);
            newRow.set('SINGLEDATE', info.SingleDate);
            newRow.set('LASTESTDATE', info.LastestDate);
            newRow.set('CUSTOMERNAME', info.CustomerName);
        }

    }
    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}