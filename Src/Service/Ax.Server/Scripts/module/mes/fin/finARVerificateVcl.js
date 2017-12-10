finARVerificateVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    me = this;
};
var proto = finARVerificateVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finARVerificateVcl;
var amount = 0;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            //    if (e.dataInfo.fieldName == "BtnLoadReceiptData") {
            //        if (this.isEdit) {
            //            Ax.utils.LibVclSystemUtils.openDataFunc("fin.ARVerificateReceiptFunc", "收款单明细", [this, "FINARVERIFICATERECEIPTFUNCDETAIL"]);
            //        }
            //        else {
            //            Ext.Msg.alert("系统提示", "非编辑状态，不可操作！");
            //        }
            //    }
            //    if (e.dataInfo.fieldName == "BtnLoadInvoiceData") {
            //        if (this.isEdit) {

            //            Ax.utils.LibVclSystemUtils.openDataFunc("fin.ARVerificateInvoiceFunc", "发票明细", [this, "FINARVERIFICATEINVOICEFUNCDETAIL"]);
            //        }
            //        else {
            //            Ext.Msg.alert("系统提示", "非编辑状态，不可操作！");
            //        }
            //    }
            if (e.dataInfo.fieldName == "BtnAutoVerificate") {
                if (this.isEdit) {
                    var masterRow = this.dataSet.getTable(0).data.items[0];
                    var bodyTable = this.dataSet.getTable(1);
                    var remainAmount = masterRow.get("REMAINAMOUNT") - amount;
                    if (remainAmount > 0) {
                        for (i = 0; i < bodyTable.data.items.length; i++) {
                            var bodyRow = bodyTable.data.items[i];
                            if (bodyRow.get("VERIFICATEAMOUNT") == 0) {
                                if (remainAmount > bodyRow.get("REMAINAMOUNT")) {
                                    bodyRow.set("VERIFICATEAMOUNT", bodyRow.get("REMAINAMOUNT"));
                                    amount = amount + bodyRow.get("REMAINAMOUNT");
                                    remainAmount = remainAmount - bodyRow.get("REMAINAMOUNT");
                                }
                                else {
                                    bodyRow.set("VERIFICATEAMOUNT", remainAmount);
                                    remainAmount = 0;
                                    amount = amount +bodyRow.get("VERIFICATEAMOUNT");
                                }
                            }
                        }
                    }
                    Ext.getCmp('VERIFICATEAMOUNT0_' + me.winId).setValue(amount);

                }
                else {
                    Ext.Msg.alert("系统提示", "非编辑状态，不可操作！");
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "RECEIPTNO") {
                    var masterRow = this.dataSet.getTable(0).data.items[0];
                    var receiptNo = masterRow.get("RECEIPTNO");
                    var data = this.invorkBcf('GetInvoiceData', [receiptNo]);
                    this.fillData.call(this, data);
                }
                if (e.dataInfo.fieldName == "CONTACTSOBJECTID") {
                    Ext.getCmp('RECEIPTNO0_' + me.winId).setValue("");
                    Ext.getCmp('RECEIPTDATE0_' + me.winId).setValue(0);
                    Ext.getCmp('FUNDTYPE0_' + me.winId).select(0);
                    Ext.getCmp('RECEIPTAMOUNT0_' + me.winId).setValue(0);
                    Ext.getCmp('REMAINAMOUNT0_' + me.winId).setValue(0);
                    Ext.getCmp('CONTRACTNO0_' + me.winId).setValue("");
                    this.deleteAll(1);
                }
            }
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "VERIFICATEAMOUNT") {
                    var masterRow = this.dataSet.getTable(0).data.items[0];
                    amount = amount + e.dataInfo.value - e.dataInfo.oldValue;
                    var dt = this.dataSet.getTable(1);
                    var totalAmount = 0;
                    for (var i = 0; i < dt.data.length; i++) {
                        totalAmount += dt.data.items[i].data['VERIFICATEAMOUNT'];
                    }
                    Ext.getCmp('VERIFICATEAMOUNT0_' + this.winId).setValue(totalAmount);
                }
            }
            this.forms[0].updateRecord(masterRow);
            break;
        case LibEventTypeEnum.FormClosing:
            amount = 0;
            break;


    }
}

proto.fillData = function (returnData) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'FINARVERIFICATEDETAILGrid');
        var table = this.dataSet.getTable(0);
        var masterRow = table.data.items[0];
        var verificateAmount = 0;
        var billno = table.data.items[0].data["BILLNO"];
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set("BILLNO", billno);
                newRow.set("ROW_ID", i + 1);
                newRow.set("ROWNO", i + 1);
                newRow.set("CREATEINVOICEDATE", info.CreateInvoiceDate);
                newRow.set("BILLTYPE", info.BillType);
                newRow.set("FROMBILLNO", info.BillNo);
                newRow.set("BILLDATE", info.BillDate);
                newRow.set("INVOICENO", info.InvoiceNo);
                newRow.set("CONTRACTNO", info.ContractNo);
                newRow.set("REMAINAMOUNT", info.RemainAmount);
                verificateAmount = verificateAmount + info.VerificationAmount
            }
        }
        Ext.getCmp('VERIFICATEAMOUNT0_' + me.winId).setValue(verificateAmount);
        this.forms[0].updateRecord(masterRow);
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

