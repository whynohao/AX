/// <reference path="finARVerificateInvoiceFuncVcl.js" />
finARVerificateInvoiceFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = finARVerificateInvoiceFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = finARVerificateInvoiceFuncVcl;
proto.winId = null;
proto.fromObj = null;
var salInvoiceData = [];

proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj;
    var salInvoiceRow = proto.fromObj[0].dataSet.getTable(0).data.items[0];
    salInvoiceTableDetail = proto.fromObj[0].dataSet.getTable(2);
    for (var i = 0; i < salInvoiceTableDetail.data.items.length; i++) {
        var tableSalInvoiceDetailValue = salInvoiceTableDetail.data.items[i];
        salInvoiceData.push({
            ContactsObjectId: tableSalInvoiceDetailValue.data["CONTACTSOBJECTID"],
            ContactsObjectName: tableSalInvoiceDetailValue.data["CONTACTSOBJECTNAME"],
            BillType: tableSalInvoiceDetailValue.data["BILLTYPE"],
            BillNo: tableSalInvoiceDetailValue.data["FROMBILLNO"],
            InvoiceNo: tableSalInvoiceDetailValue.data["INVOICENO"],
            ContractNo: tableSalInvoiceDetailValue.data["CONTRACTNO"],
            RemainAmount: tableSalInvoiceDetailValue.data["REMAINAMOUNT"],
            VerificationAmount: tableSalInvoiceDetailValue.data["VERIFICATEAMOUNT"],
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
                var contractNo = masterRow.get("CONTRACTNO");
                var data = this.invorkBcf('GetInvoiceData', [billDateFrom, billDateTo, contactsobjectId, contractNo,salInvoiceData]);
                this.fillData.call(this, data);
            }
            if (e.dataInfo.fieldName == "BtnLoadData") {
                var selectItems = this.dataSet.getTable(1).data.items;
                var records = [];
                for (var i = 0; i < selectItems.length; i++) {
                    if (selectItems[i].data["ISCHOSE"] == true) {
                        records.push({
                            BillNo: selectItems[i].data["BILLNO"],
                            ContractNo: selectItems[i].data["CONTRACTNO"],
                            ContactsObjectId: selectItems[i].data["CONTACTSOBJECTID"],
                            ContactsObjectName: selectItems[i].data["CONTACTSOBJECTNAME"],
                            PersonId: selectItems[i].data["PERSONID"],
                            PersonName: selectItems[i].data["PERSONNAME"],
                            BillType: selectItems[i].data["BILLTYPE"],
                            InvoiceNo: selectItems[i].data["INVOICENO"],
                            BillDate: selectItems[i].data["BILLDATE"],
                            CreateInvoiceDate: selectItems[i].data["CREATEINVOICEDATE"],
                            DueAmount: selectItems[i].data["DUEAMOUNT"],
                            RemainAmount: selectItems[i].data["REMAINAMOUNT"],
                            VerificationAmount: selectItems[i].data["VERIFICATIONAMOUNT"],
                            OffsetAmount: selectItems[i].data["OFFSETAMOUNT"],
                        });
                    }
                }
                if (records.length == 0) {
                    Ext.Msg.alert("系统提示", "请选择明细数据！");
                    return;
                }
                else {
                    this.fillReturnData.call(this, records);
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
        var grid = Ext.getCmp(this.winId + 'FINARVERIFICATEINVOICEFUNCDETAILGrid');
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set("ROW_ID", i + 1);
                newRow.set("ROWNO", i + 1)
                newRow.set("BILLTYPE", info.BillType);
                newRow.set("BILLNO", info.BillNo);
                newRow.set("BILLDATE", info.BillDate);
                newRow.set("CREATEINVOICEDATE", info.CreateInvoiceDate);
                newRow.set("INVOICENO", info.InvoiceNo);
                newRow.set("CONTRACTNO", info.ContractNo);
                newRow.set("PERSONID", info.PersonId);
                newRow.set("PERSONNAME", info.PersonName);
                newRow.set("DUEAMOUNT", info.DueAmount);
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
        var grid = Ext.getCmp(proto.winId + 'FINARVERSHALLRECGrid');
        var table = proto.fromObj[0].dataSet.getTable(0);
        var billno = table.data.items[0].data["BILLNO"];
        var l = salInvoiceData.length;
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
            newRow.set("BILLDATE", info.BillDate);
            newRow.set("INVOICENO", info.InvoiceNo);
            newRow.set("CONTRACTNO", info.ContractNo);
            newRow.set("REMAINAMOUNT", info.RemainAmount);
            newRow.set("VERIFICATEAMOUNT", info.VerificationAmount);
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}


