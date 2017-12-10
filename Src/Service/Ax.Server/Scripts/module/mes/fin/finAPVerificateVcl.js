/// <reference path="finAPVerificateVcl.js" />
finAPVerificateVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    me = this;
};
var proto = finAPVerificateVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finAPVerificateVcl;
var paymentData;
var verificateData;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnLoadPayData") {
                if (this.isEdit) {
                    Ax.utils.LibVclSystemUtils.openDataFunc("fin.APVerificatePaymentFunc", "付款单明细", [this, "FINAPVERIFICATPAYMENTFUNCDETAIL"]);
                }
                else {
                    Ext.Msg.alert("系统提示", "非编辑状态，不可操作！");
                }
            }
            if (e.dataInfo.fieldName == "BtnLoadInvoiceData") {
                if (this.isEdit) {

                    Ax.utils.LibVclSystemUtils.openDataFunc("fin.APVerificateInvoiceFunc",
                        "发票明细",
                        [this, "FINAPVERIFICATEINVOICEFUNCDETAIL"]);
                } else {
                    Ext.Msg.alert("系统提示", "非编辑状态，不可操作！");
                }
            }
            if (e.dataInfo.fieldName == "BtnAutoVerificate") {
                    if (this.isEdit) {
                        var table = this.dataSet.getTable(1);
                        verificateData = this.dataSet.getTable(0).data.items[0].get("VERIFICATEAMOUNT");
                        paymentData = this.dataSet.getTable(0).data.items[0].get("PAYAMOUNT");
                        var invoiceInfo = [];
                        if (table.data.items.length > 0) {
                            for (var i = 0; i < table.data.items.length; i++) {
                                var record = table.data.items[i];
                                invoiceInfo.push({
                                    FromBillNo: record.get("FROMBILLNO"),
                                    ContactsObjectId: record.get("CONTACTSOBJECTID"),
                                    ContactsObjectName: record.get("CONTACTSOBJECTNAME"),
                                    BillType: record.get("BILLTYPE"),
                                    InvoiceNo: record.get("INVOICENO"),
                                    CreateInvoiceDate: record.get("CREATEINVOICEDATE"),
                                    RemainAmount: record.get("REMAINAMOUNT"),
                                    VerificationAmount: record.get("VERIFICATEAMOUNT")
                                });
                            }
                           var data =  this.invorkBcf('CalVerAmount', [paymentData, verificateData, invoiceInfo]);
                           this.FillDataFunc(data);
                        }

                    }
                    else {
                        Ext.Msg.alert("系统提示", "非编辑状态，不可操作！");
                    }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "FINAPPAYMENTNO") {
                    var finAPPaymentNo = this.dataSet.getTable(0).data.items[0].data['FINAPPAYMENTNO'] == undefined
                        ? ""
                        : this.dataSet.getTable(0).data.items[0].data['FINAPPAYMENTNO'];
                    paymentData = this.invorkBcf('GetPaymentData', [finAPPaymentNo]);
                    Ext.getCmp('PAYAMOUNT0_' + me.winId).setValue(paymentData);

                    var contactsObjectId = this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTID'] == undefined
                        ? ""
                        : this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTID'];
                    var invoiceData = this.invorkBcf('GetInvoiceData', [contactsObjectId, paymentData]);
                    this.FillDataFunc(invoiceData);

                }
                if (e.dataInfo.fieldName == "CONTACTSOBJECTID") {
                    Ext.getCmp('FINAPPAYMENTNO0_' + me.winId).setValue("");
                    Ext.getCmp('PAYMENTDATE0_' + me.winId).setValue(0);
                    Ext.getCmp('PAYAMOUNT0_' + me.winId).setValue(0);
                    this.deleteAll(1);
                }
            }
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "VERIFICATEAMOUNT") {
                    var dt = this.dataSet.getTable(1);
                    var amount = 0;
                    for (var i = 0; i < dt.data.length; i++) {
                        amount += dt.data.items[i].data['VERIFICATEAMOUNT'];
                    }
                    verificateData = amount;
                    Ext.getCmp('VERIFICATEAMOUNT0_' + me.winId).setValue(amount);
                }
            }
            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        //case LibEventTypeEnum.BeforeDeleteRow:
        //    if (e.dataInfo.tableIndex == 1) {
        //        e.dataInfo.cancel = true;
        //    }
    }
}

proto.FillDataFunc =  function (returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var amount = 0;
        var list = returnList;
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
               // newRow.set("BILLNO", BillNo);
                newRow.set("ROW_ID",  i + 1);
                newRow.set("ROWNO", i + 1);
                newRow.set("CONTACTSOBJECTID", info.ContactsObjectId);
                newRow.set("CONTACTSOBJECTNAME", info.ContactsObjectName);
                newRow.set("CREATEINVOICEDATE", info.CreateInvoiceDate);          
                newRow.set("BILLTYPE", info.BillType);
                newRow.set("FROMBILLNO", info.FromBillNo);
                newRow.set("INVOICENO", info.InvoiceNo);
                newRow.set("REMAINAMOUNT", info.RemainAmount);
                newRow.set("VERIFICATEAMOUNT", info.VerificationAmount);
                amount += info.VerificationAmount;
            }
            Ext.getCmp('VERIFICATEAMOUNT0_' + me.winId).setValue(amount);
            this.forms[0].updateRecord(masterRow);
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
} 