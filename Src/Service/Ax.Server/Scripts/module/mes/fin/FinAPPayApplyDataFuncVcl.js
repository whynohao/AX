/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

finAPPayApplyDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = finAPPayApplyDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = finAPPayApplyDataFuncVcl;
var returnList;
//调用datafuc的时候会调用此方法，可以初始化一些参数。
proto.contactsobjectId = "";
proto.contactsobjectName = "";
proto.winId = "";
proto.fromObj = null;
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj[0];
    var masterRow = this.dataSet.getTable(0).data.items[0];
    this.forms[0].loadRecord(masterRow);
};


proto.fillData = function (returnList) {
    var grid = Ext.getCmp(proto.winId + 'FINAPPAYAPPLYDETAILGrid');
    var list = returnList;
    Ext.suspendLayouts();   
    var curStore = grid.getStore();
    var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
    var selectItems = this.dataSet.getTable(1).data.items;
    proto.fromObj.dataSet.dataList[1].removeAll();
    curStore.suspendEvents();
    try {
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                if (selectItems[i].data["ISCHOSE"] == true) {
                var newRow = proto.fromObj.addRow(masterRow, 1);
                newRow.set('CONTACTSOBJECTID', info.ContactsobjectId);
                newRow.set('CONTACTSOBJECTNAME', info.ContactsobjectName);
                newRow.set('PAYAMOUNT', info.PayAmount);
                newRow.set('DUEPAYAMOUNT', info.DuePayAmount);
                newRow.set('ACTUALPAYAMOUNT', info.ActualPayAmount);
                var finAPApplyDetailInfo = info.FinAPApplyDetailInfo;
                    if (finAPApplyDetailInfo.length > 0) {
                        for (var j = 0; j < finAPApplyDetailInfo.length; j++) {
                            newRow.set("PAYDETAIL", 1);
                            var subInfo = finAPApplyDetailInfo[j];
                            var subRow = proto.fromObj.addRow(newRow, 2);
                            subRow.set('PAYTYPE', subInfo.PayType);
                            subRow.set('FROMBILLNO', subInfo.FromBillNo);
                            subRow.set('INVOICENO', subInfo.InvoiceNo);
                            subRow.set('INVOICEDATE', subInfo.InvoiceDate);
                            subRow.set('PAYMENTDAYS', subInfo.PaymentDays);
                            subRow.set('ISDEADLINE', subInfo.IsDeadline);
                            subRow.set('PAYAMOUNT', subInfo.SubPayAmount);
                            subRow.set('VERIFICATIONACCOUNT', subInfo.VerificationAmount);
                            subRow.set('OFFSETACCOUNT', subInfo.OffsetAmount);
                        }
                    }
                }
            }
        }
    }
    finally {
        curStore.resumeEvents();
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);
    }
}

proto.FillDataFunc = function (returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
        this.dataSet.getTable(2).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var list = returnList;
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set('CONTACTSOBJECTID', info.ContactsobjectId);
                newRow.set('CONTACTSOBJECTNAME', info.ContactsobjectName);
                newRow.set('PAYAMOUNT', info.PayAmount);
                newRow.set('DUEPAYAMOUNT', info.DuePayAmount);
                var finAPApplyDetailInfo = info.FinAPApplyDetailInfo;
                if (finAPApplyDetailInfo.length > 0) {
                    for (var j = 0; j < finAPApplyDetailInfo.length; j++) {
                        newRow.set("PAYDETAIL", 1);
                        var subInfo = finAPApplyDetailInfo[j];
                        var subRow = this.addRow(newRow, 2);
                        subRow.set('CONTACTSOBJECTID', info.ContactsobjectId);
                        subRow.set('CONTACTSOBJECTNAME', info.ContactsobjectName);
                        subRow.set('PAYTYPE', subInfo.PayType);
                        subRow.set('FROMBILLNO', subInfo.FromBillNo);
                        subRow.set('INVOICENO', subInfo.InvoiceNo);
                        subRow.set('INVOICEDATE', subInfo.InvoiceDate);
                        subRow.set('PAYMENTDAYS', subInfo.PaymentDays);
                        subRow.set('ISDEADLINE', subInfo.IsDeadline);
                        subRow.set('PAYAMOUNT', subInfo.SubPayAmount);
                        subRow.set('VERIFICATIONACCOUNT', subInfo.VerificationAmount);
                        subRow.set('OFFSETACCOUNT', subInfo.OffsetAmount);
                    }
                }
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnLoadData") {
                if (returnList == undefined & returnList.length <= 0) {
                    alert("请先查询数据！");
                } else {
                    this.fillData.call(this, returnList);
                    this.win.close();
                }                            
            }
            else if (e.dataInfo.fieldName == "BtnQueryData") {
                
                var contactsobjectId = this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTID'];
                var isDue = this.dataSet.getTable(0).data.items[0].data['ISDUE'] == undefined ? 0 : this.dataSet.getTable(0).data.items[0].data['ISDUE'];
                returnList = this.invorkBcf('GetData', [contactsobjectId, isDue]);
                this.FillDataFunc.call(this, returnList);               
            }
            break;
        case LibEventTypeEnum.Validated://form的赋值之后在this.dataSet中还没有实时的数据写入，需要写入之后，才能取到值。          
            if (e.dataInfo.fieldName == "CONTACTSOBJECTID" || e.dataInfo.fieldName == "ISDUE" ) {
                e.dataInfo.curForm.updateRecord(e.dataInfo.dataRow);
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
            break;
    }
}; 
