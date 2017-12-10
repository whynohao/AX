finAPVerificateInvoiceFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = finAPVerificateInvoiceFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = finAPVerificateInvoiceFuncVcl;
proto.winId = null;
proto.fromObj = null;
var InvoiceData = [];
var PaymentData = [];
var returnList = [];

proto.doSetParam = function(vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj;
    var InvoiceRow = proto.fromObj[0].dataSet.getTable(0).data.items[0];
    var PaymentTableDetail = proto.fromObj[0].dataSet.getTable(1);
    var InvoiceTableDetail = proto.fromObj[0].dataSet.getTable(2);
    for (var i = 0; i < InvoiceTableDetail.data.items.length; i++) {
        var tableInvoiceDetailValue = InvoiceTableDetail.data.items[i];
        InvoiceData.push({
            ContactsObjectId: tableInvoiceDetailValue.data["CONTACTSOBJECTID"],
            ContactsObjectName: tableInvoiceDetailValue.data["CONTACTSOBJECTNAME"],
            BillType: tableInvoiceDetailValue.data["BILLTYPE"],
            BillNo: tableInvoiceDetailValue.data["FROMBILLNO"],
            InvoiceNo: tableInvoiceDetailValue.data["INVOICENO"],
            RemainAmount: tableInvoiceDetailValue.data["REMAINAMOUNT"],
            VerificationAmount: tableInvoiceDetailValue.data["VERIFICATEAMOUNT"],
        });
    }
    for (var i = 0; i < PaymentTableDetail.data.items.length; i++) {
        var tablePaymentDetailValue = PaymentTableDetail.data.items[i];
        PaymentData.push({
            ContactsObjectId: tablePaymentDetailValue.data["CONTACTSOBJECTID"],
            ApAmount: tablePaymentDetailValue.data["REMAINAMOUNT"],
        });

    }
}


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnQueryData") {
                var masterRow = this.dataSet.getTable(0).data.items[0];
                var billDateFrom = masterRow.get("BILLDATEFROM");
                var billDateTo = masterRow.get("BILLDATETO")
                var contactsobjectId = masterRow.get("CONTACTSOBJECTID");
                returnList = this.invorkBcf('GetInvoiceData', [billDateFrom, billDateTo, contactsobjectId, PaymentData]);
                this.fillData.call(this, returnList);
            }
            if (e.dataInfo.fieldName == "BtnLoadData") {

                var selectItems = this.dataSet.getTable(1).data.items;
                var records = [];
                for (var i = 0; i < returnList.length; i++) {
                    if (selectItems[i].data["ISCHOSE"] == true) {
                        records.push({
                            BillNo: returnList[i].BillNo,
                            ContactsObjectId: returnList[i].ContactsObjectId,
                            ContactsObjectName: returnList[i].ContactsObjectName,
                            PersonId: returnList[i].PersonId,
                            PersonName: returnList[i].PersonName,
                            BillType: returnList[i].BillType,
                            InvoiceNo: returnList[i].InvoiceNo,
                            BillDate: returnList[i].BillDate,
                            CreateInvoiceDate: returnList[i].CreateInvoiceDate,
                            ApAmount: returnList[i].ApAmount,
                            RemainAmount: returnList[i].RemainAmount,
                            VerificationAmount: returnList[i].VerificationAmount,
                            OffsetAmount: returnList[i].OffsetAmount,
                            PreVerificateAmount: returnList[i].PreVerificateAmount,
                            PreRemainAmount: returnList[i].PreRemainAmount
                        });
                    }
                }
                if (records == undefined & records.length <= 0) {
                    Ext.Msg.alert("系统提示", "请选择明细数据！");
                    return;
                }
                else {
                    this.fillReturnData.call(this, records);
                    this.fillPaymentData.call(this, records);
                    this.win.close();
                }
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
    }
}


proto.fillData = function (returnData) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'FINAPVERIFICATEINVOICEFUNCDETAILGrid');
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set("ROW_ID", i + 1);
                newRow.set("ROWNO", i + 1);
                newRow.set("BILLTYPE", info.BillType);
                newRow.set("BILLNO", info.BillNo);
                newRow.set("BILLDATE", info.BillDate);
                newRow.set("CREATEINVOICEDATE", info.CreateInvoiceDate);
                newRow.set("INVOICENO", info.InvoiceNo);
                newRow.set("PERSONID", info.PersonId);
                newRow.set("PERSONNAME", info.PersonName);
                newRow.set("APAMOUNT", info.ApAmount);
                newRow.set("REMAINAMOUNT", info.RemainAmount);
                newRow.set("VERIFICATIONAMOUNT", info.VerificationAmount);
                newRow.set("OFFSETAMOUNT", info.OffsetAmount);
                newRow.set("CONTACTSOBJECTID", info.ContactsObjectId);
                newRow.set("CONTACTSOBJECTNAME", info.ContactsObjectName);
            }
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}


proto.fillReturnData = function (returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = proto.fromObj[0].dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        //proto.fromObj[0].deleteAll(1);
        var grid = Ext.getCmp(proto.winId + 'FINAPVERINVOICEGrid');
        var table = proto.fromObj[0].dataSet.getTable(0);
        var billno = table.data.items[0].data["BILLNO"];
        var l = InvoiceData.length;
        for (var i = 0; i < returnList.length; i++) {
            var info = returnList[i];
            var newRow = proto.fromObj[0].addRowForGrid(grid);
            newRow.set("BILLNO", billno);
            newRow.set("ROW_ID", l + i + 1);
            newRow.set("ROWNO", l + i + 1);
            newRow.set("CONTACTSOBJECTID", info.ContactsObjectId);
            newRow.set("CONTACTSOBJECTNAME", info.ContactsObjectName);
            newRow.set("CREATEINVOICEDATE", info.CreateInvoiceDate);
            newRow.set("PERSONID", info.PersonId);
            newRow.set("PERSONNAME", info.PersonName);
            newRow.set("BILLTYPE", info.BillType);
            newRow.set("FROMBILLNO", info.BillNo);
            newRow.set("INVOICENO", info.InvoiceNo);
            newRow.set("REMAINAMOUNT", info.RemainAmount);
            newRow.set("VERIFICATEAMOUNT", info.PreVerificateAmount);
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

proto.fillPaymentData = function (paymentList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = proto.fromObj[0].dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        //proto.fromObj[0].deleteAll(1);
        var grid = Ext.getCmp(proto.winId + 'FINAPVERPAYMENTGrid');
        var table = proto.fromObj[0].dataSet.getTable(1);
        for (var i = 0; i < paymentList.length; i++) {          
            for (var j = 0; j <table.count.length; j++) {
                var list = paymentList[i];
                var row = table.data.items[j];
                if (list.ContactsObjectId == row.data["CONTACTSOBJECTID"]) {
                    if (row.data["REMAINAMOUNT"] > 0) {
                        if (row.data["REMAINAMOUNT"] >= list.PreVerificateAmount) {
                            row.set('REMAINAMOUNT', (row.data["REMAINAMOUNT"] - list.PreVerificateAmount));
                            row.set('VERIFICATEAMOUNT', (row.data["VERIFICATEAMOUNT"] + list.PreVerificateAmount));
                        } else {
                            row.set('VERIFICATEAMOUNT', (row.data["VERIFICATEAMOUNT"]+row.data["REMAINAMOUNT"]));
                            row.set('REMAINAMOUNT', 0);
                        }
                    }
                }
                proto.fromObj[0].forms[1].loadRecord(row);
            }
        }
    } finally {
        curStore.resumeEvents(); //打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true); //打开Ext布局
    }
}
 

