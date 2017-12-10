finAROffsetDatafuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = finAROffsetDatafuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = finAROffsetDatafuncVcl;

proto.winId = null;
proto.fromObj = null
var isRed;
var contractNo;
var offsetType;
var contactsObject;
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj;
    var offsetRow = proto.fromObj[0].dataSet.getTable(0).data.items[0];
    isRed = proto.fromObj[2];
    contractNo = offsetRow.get("CONTRACTNO");
    offsetType = offsetRow.get("OFFSETTYPE");
    contactsObject = offsetRow.get("CONTACTSOBJECTID");
    contactsobjectName = offsetRow.get("CONTACTSOBJECTNAME");
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.data["ISRED"] = isRed;
    masterRow.data["CONTRACTNO"] = contractNo;
    masterRow.data["CONTACTSOBJECTID"] = contactsObject;
    masterRow.data["CONTACTSOBJECTNAME"] = contactsobjectName;
    masterRow.data["OFFSETTYPE"] = offsetType;
    this.forms[0].loadRecord(masterRow);
    //Ext.getCmp('ISRED0_' + vcl.winId).setValue(isRed);
    //Ext.getCmp('CONTRACTNO0_' + vcl.winId).setValue(contractNo);
    //Ext.getCmp('CONTACTSOBJECTID0_' + vcl.winId).setValue(contactsObject);
    //Ext.getCmp('INVOICETYPE0_' + vcl.winId).setValue(invoiceType);
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
                var offsetType = masterRow.get("OFFSETTYPE")
                var data = this.invorkBcf('GetInvoiceData', [billDateFrom, billDateTo, contactsobjectId, contractNo, isRed, offsetType]);
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
                            //BillType: selectItems[i].data["BILLTYPE"],
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
                if (records.length != 1) {
                    Ext.Msg.alert("系统提示", "只能选择一条数据！");
                    return;
                }
                else {
                    this.fillReturnData.call(this, records);
                    this.win.close();
                }
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            e.dataInfo.cancel = true;
            Ext.Msg.alert("系统提示", "不能新增行！");
            break;
    }
}

proto.fillData = function (returnData) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'FINAROFFSETDATAFUNCDETAILGrid');
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set("ROW_ID", i + 1);
                newRow.set("ROWNO", i + 1)
                newRow.set("BILLTYPE", offsetType);
                newRow.set("BILLNO", info.BillNo);
                newRow.set("BILLDATE", info.BillDate);
                newRow.set("CREATEINVOICEDATE", info.CreateInvoiceDate);
                newRow.set("INVOICENO", info.InvoiceNo);
                newRow.set("CONTRACTNO", info.ContractNo);
                newRow.set("PERSONID", info.PersonId);
                newRow.set("PERSONNAME", info.PersonName);
                newRow.set("DEPTID", info.DeptId);
                newRow.set("DEPTNAME", info.DeptName);
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
    //var curStore = proto.fromObj[0].dataSet.getTable(1);
    //curStore.suspendEvents();//关闭store事件
    try {
        //proto.fromObj[0].deleteAll(1);
        //var grid = Ext.getCmp(proto.winId + 'FINAROFFSETGrid');
        
        if(isRed==false)
        {
            Ext.getCmp('BLUEBILLNO0_' + proto.winId).setValue(returnList[0].BillNo);
            Ext.getCmp('BLUEINVOICENO0_' + proto.winId).setValue(returnList[0].InvoiceNo);
            Ext.getCmp('BLUEOFFSETACCOUNT0_' + proto.winId).setValue(returnList[0].RemainAmount);
        }
        else
        {
            Ext.getCmp('REDBILLNO0_' + proto.winId).setValue(returnList[0].BillNo);
            Ext.getCmp('REDINVOICENO0_' + proto.winId).setValue(returnList[0].InvoiceNo);
            Ext.getCmp('REDOFFSETACCOUNT0_' + proto.winId).setValue(returnList[0].RemainAmount);
           
        }
        //Ext.getCmp('CONTACTSOBJECTID0_' + proto.winId).setValue(returnList[0].ContactsObjectId);
        Ext.getCmp('CONTRACTNO0_' + proto.winId).setValue(returnList[0].ContractNo);
    }
    finally {
        //curStore.resumeEvents();//打开store事件
        //if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
        //    curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
        var masterRow = proto.fromObj[0].dataSet.getTable(0).data.items[0];
        proto.fromObj[0].forms[0].updateRecord(masterRow);
    }
}




