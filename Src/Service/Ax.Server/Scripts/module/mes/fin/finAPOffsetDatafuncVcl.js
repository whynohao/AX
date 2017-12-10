/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

finAPOffsetDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = finAPOffsetDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = finAPOffsetDataFuncVcl;
var returnList;
//调用datafuc的时候会调用此方法，可以初始化一些参数。

proto.doSetParam = function (vclObj) {
    proto.contactsobjectId = vclObj[0];
    proto.contactsobjectName = vclObj[1];
    proto.InvoiceType = vclObj[2];
    proto.isRed = vclObj[3];
    proto.winId = vclObj[4].winId;
    proto.fromObj = vclObj[4];
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set("CONTACTSOBJECTID", proto.contactsobjectId); 
    masterRow.set("CONTACTSOBJECTNAME", proto.contactsobjectName);
    masterRow.set("INVOICETYPE", proto.InvoiceType);
    masterRow.set("ISRED", proto.isRed);
    this.forms[0].loadRecord(masterRow);
};

//回填到采购发票子表
proto.fillData = function (returnList) {

    var redBillno =  Ext.getCmp('REDBILLNO0_' + proto.winId );
    var redOffsetAmount = Ext.getCmp('REDOFFSETAMOUNT0_' + proto.winId);
    var redInvoice = Ext.getCmp('REDINVOICENO0_' + proto.winId);
    var blueBillno = Ext.getCmp('BLUEBILLNO0_' + proto.winId);
    var blueOffsetAmount = Ext.getCmp('BLUEOFFSETAMOUNT0_' + proto.winId);
    var blueInvoice = Ext.getCmp('BLUEINVOICENO0_' + proto.winId);
    var list = returnList;
    Ext.suspendLayouts();
    //var curStore = grid.getStore();
    var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
    var selectItems = this.dataSet.getTable(1).data.items;
    //curStore.suspendEvents();
    try {
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                if (selectItems[i].data["ISCHOSE"] == true) {
                    //var newRow = proto.fromObj.addRow(masterRow, 1);
                    if (info.IsRed == 0) {
                        blueBillno.setValue(info.BillNo);
                        blueInvoice.setValue(info.InvoiceNo);
                        blueOffsetAmount.setValue(info.RemainAmount);
                    }
                    else {
                        redBillno.setValue(info.BillNo);
                        redInvoice.setValue(info.InvoiceNo);
                        redOffsetAmount.setValue(info.RemainAmount);
                    }
                    break;
                }
            }
        }
    } finally {
        proto.fromObj.forms[0].updateRecord(masterRow);
        //proto.fromObj.forms[0].loadRecord(masterRow);
        Ext.resumeLayouts(true);
    }
}

//dataFunc表身填充
proto.FillDataFunc = function (returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var list = returnList;
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set('BILLTYPE', info.BillType);
                newRow.set('BILLNO', info.BillNo);
                newRow.set('BILLDATE', info.PurChaseOrderNo);
                newRow.set('CONTACTSOBJECTID', info.ContactsObjectId);
                newRow.set('CONTACTSOBJECTNAME', info.ContactsObjectName);
                newRow.set('CREATEINVOICEDATE', info.CreateInvoiceDate);
                newRow.set('INVOICENO', info.InvoiceNo);
                newRow.set('PERSONID', info.PersonId);
                newRow.set('PERSONNAME', info.PersonName);
                newRow.set('APAMOUNT', info.ApAmount);
                newRow.set('REMAINAMOUNT', info.RemainAmount);
                newRow.set('VERIFICATIONAMOUNT', info.VerificationAmount);
                newRow.set('OFFSETAMOUNT', info.OffsetAmount);
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

                var contactsobjectId = proto.contactsobjectId;
                var InvoiceType = proto.InvoiceType;
                var isRed = proto.isRed;
                var billDateFrom = this.dataSet.getTable(0).data.items[0].data['BILLDATEFROM'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['BILLDATEFROM'];
                var billDateTo = this.dataSet.getTable(0).data.items[0].data['BILLDATETO'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['BILLDATETO'];                                      
                returnList = this.invorkBcf('GetInvoiceData', [billDateFrom, billDateTo, contactsobjectId, isRed, InvoiceType]);
                this.FillDataFunc.call(this, returnList);
            }
            break;
        case LibEventTypeEnum.Validated://form的赋值之后在this.dataSet中还没有实时的数据写入，需要写入之后，才能取到值。          
            if (e.dataInfo.fieldName == "BILLDATEFROM" || e.dataInfo.fieldName == "BILLDATETO"  ) {
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
