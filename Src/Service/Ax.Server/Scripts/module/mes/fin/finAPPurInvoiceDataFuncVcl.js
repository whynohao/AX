/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

finAPPurInvoiceDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = finAPPurInvoiceDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = finAPPurInvoiceDataFuncVcl;
var returnList;
//调用datafuc的时候会调用此方法，可以初始化一些参数。
proto.contactsobjectId = "";
proto.contactsobjectName = "";
proto.winId = "";
proto.fromObj = null;
proto.doSetParam = function (vclObj) {
    proto.contactsobjectId = vclObj[0];
    proto.contactsobjectName = vclObj[1];
    proto.fromBillType = vclObj[2];
    proto.isRed = vclObj[3];
    proto.taxRate = vclObj[4];
    proto.standardcoilRate = vclObj[5];
    proto.winId = vclObj[6].winId;
    proto.fromObj = vclObj[6];
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set("CONTACTSOBJECTID", proto.contactsobjectId);
    masterRow.set("CONTACTSOBJECTNAME", proto.contactsobjectName);
    masterRow.set("FROMBILLTYPE", proto.fromBillType);
    this.forms[0].loadRecord(masterRow);
};

//回填到采购发票子表
proto.fillData = function (returnList) {
    var grid = Ext.getCmp(proto.winId + 'FINAPPURINVOICEDETAILGrid');
    var list = returnList;
    Ext.suspendLayouts();
    var curStore = grid.getStore();
    var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
    var selectItems = this.dataSet.getTable(1).data.items;
    proto.fromObj.dataSet.dataList[1].removeAll();
    curStore.suspendEvents();
    try {
        if (list !== undefined && list.length > 0) {

            var bwTotalAmount = 0;
            var bwTotalTaxAmount = 0;
            var bwTotalTax = 0;
            var totalAmount = 0;
            var totalTaxAmount = 0;
            var totalTax = 0;

            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                if (selectItems[i].data["ISCHOSE"] == true) {

                    //金额联动逻辑
                    var conversion = {
                        TaxRate: proto.taxRate,
                        StandardcoilRate: proto.standardcoilRate,
                        Quantity: info.InVoicePurQty,
                        taxPrice: 0,
                        Price: info.NoPurTaxPrice,
                        Amount: 0,
                        TaxAmount: 0,
                        Tax: 0,
                        BwAmount: 0,
                        BwTaxAmount: 0,
                        BwTax: 0
                    };
                    var priceData = this.invorkBcf('CalculateAmount', [conversion, 5, 0]);
                    //填充表身数据
                    var newRow = proto.fromObj.addRow(masterRow, 1);
                    newRow.set('PURCHASESTOCKINNO', info.PurChaseStockInNo);
                    newRow.set('PURCHASESTOCKINROWID', info.PurChaseStockInRowId);
                    newRow.set('PURCHASEORDERNO', info.PurChaseOrderNo);
                    newRow.set('PURCHASEORDERROWID', info.PurChaseOrderRowId);
                    newRow.set('CONTRACTNO', info.ContractNo);
                    newRow.set('MATERIALID', info.MaterialId);
                    newRow.set('MATERIALNAME', info.MaterialName);
                    newRow.set('MATERIALSPEC', info.MaterialSpec);
                    newRow.set('ATTRIBUTEID', info.AttributeId);
                    newRow.set('ATTRIBUTENAME', info.AttributeName);
                    newRow.set('ATTRIBUTECODE', info.AttributeCode);
                    newRow.set('ATTRIBUTEDESC', info.AttributeDesc);
                    newRow.set('UNITID', info.UnitId);
                    newRow.set('UNITNAME', info.UnitName);
                    newRow.set('DEALUNITID', info.DealUnitId);
                    newRow.set('DEALUNITNAME', info.DealUnitName);
                    newRow.set('DEALUNITNO', info.DealunitNo);
                    newRow.set('WAREINPURQTY', info.WareInPurQty);
                    newRow.set('WAREINQTY', info.WareInQty);
                    newRow.set('INVOICEPURQTY', info.InVoicePurQty);
                    newRow.set('INVOICEQTY', info.InVoiceQty);
                    newRow.set('PURTAXPRICE', info.PurTaxPrice);
                    newRow.set('NOPURTAXPRICE', info.NoPurTaxPrice);
                    newRow.set('NOINVOICEPURQTY', info.NoInVoicePurQty);
                    newRow.set('NOINVOICEQTY', info.NoInVoiceQty);
                    newRow.set('FIGURENO', info.FigureNo);
                    newRow.set('TEXTUREID', info.TextureId);
                    //金额联动字段
                    newRow.set('TAXPRICE', priceData.TaxPrice);//实际含税单价
                    newRow.set('NOTAXPRICE', priceData.Price);//实际不含税单价
                    newRow.set('AMOUNT', priceData.Amount);
                    newRow.set('TAXAMOUNT', priceData.TaxAmount);
                    newRow.set('TAX', priceData.Tax);
                    newRow.set('BWAMOUNT', priceData.BwAmount);
                    newRow.set('BWTAXAMOUNT', priceData.BwTaxAmount);
                    newRow.set('BWTAX', priceData.BwTax);
                    newRow.set('TAXRATE', priceData.TaxRate);//税率
                    //表头金额联动
                    bwTotalAmount += priceData.BwAmount;
                    bwTotalTaxAmount += priceData.BwTaxAmount;
                    bwTotalTax += priceData.BwTax;
                    totalAmount += priceData.Amount;
                    totalTaxAmount += priceData.TaxAmount;
                    totalTax += priceData.Tax;
                }
            }
            Ext.getCmp('BWTOTALAMOUNT0_' + proto.winId).setValue(bwTotalAmount);
            Ext.getCmp('BWTOTALTAXAMOUNT0_' + proto.winId).setValue(bwTotalTaxAmount);
            Ext.getCmp('BWTOTALTAX0_' + proto.winId).setValue(bwTotalTax);
            Ext.getCmp('TOTALAMOUNT0_' + proto.winId).setValue(totalAmount);
            Ext.getCmp('TOTALTAXAMOUNT0_' + proto.winId).setValue(totalTaxAmount);
            Ext.getCmp('TOTALTAX0_' + proto.winId).setValue(totalTax);
            proto.fromObj.forms[0].updateRecord(masterRow);
        }
    } finally {
        curStore.resumeEvents();
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
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
                newRow.set('PURCHASESTOCKINNO', info.PurChaseStockInNo);
                newRow.set('PURCHASESTOCKINROWID', info.PurChaseStockInRowId);
                newRow.set('PURCHASEORDERNO', info.PurChaseOrderNo);
                newRow.set('PURCHASEORDERROWID', info.PurChaseOrderRowId);
                newRow.set('CONTRACTNO', info.ContractNo);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('MATERIALSPEC', info.MaterialSpec);
                newRow.set('FIGURENO', info.FigureNo);
                newRow.set('TEXTUREID', info.TextureId);
                newRow.set('ATTRIBUTEID', info.AttributeId);
                newRow.set('ATTRIBUTENAME', info.AttributeName);
                newRow.set('ATTRIBUTECODE', info.AttributeCode);
                newRow.set('ATTRIBUTEDESC', info.AttributeDesc);
                newRow.set('UNITID', info.UnitId);
                newRow.set('UNITNAME', info.UnitName);
                newRow.set('DEALUNITID', info.DealUnitId);
                newRow.set('DEALUNITNAME', info.DealUnitName);
                newRow.set('DEALUNITNO', info.DealunitNo);
                newRow.set('WAREINPURQTY', info.WareInPurQty);
                newRow.set('WAREINQTY', info.WareInQty);
                newRow.set('INVOICEPURQTY', info.InVoicePurQty);
                newRow.set('INVOICEQTY', info.InVoiceQty);
                newRow.set('NOINVOICEPURQTY', info.NoInVoicePurQty);
                newRow.set('NOINVOICEQTY', info.NoInVoiceQty);
                newRow.set('PURTAXPRICE', info.PurTaxPrice);
                newRow.set('NOPURTAXPRICE', info.NoPurTaxPrice);
                newRow.set('TAXPRICE', info.TaxPrice);
                newRow.set('NOTAXPRICE', info.NoTaxPrice);
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
                    // this.mathPrice.call(this, returnList);
                    this.fillData.call(this, returnList);
                    this.win.close();
                }
            }
            else if (e.dataInfo.fieldName == "BtnQueryData") {

                var contactsobjectId = proto.contactsobjectId;
                var fromBillType = proto.fromBillType;
                var billDateFrom = this.dataSet.getTable(0).data.items[0].data['BILLDATEFROM'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['BILLDATEFROM'];
                var billDateTo = this.dataSet.getTable(0).data.items[0].data['BILLDATETO'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['BILLDATETO'];
                var materialId = this.dataSet.getTable(0).data.items[0].data['MATERIALID'] == undefined ? "" : this.dataSet.getTable(0).data.items[0].data['MATERIALID'];
                returnList = this.invorkBcf('GetData', [billDateFrom, billDateTo, contactsobjectId, materialId, fromBillType, proto.isRed]);
                this.FillDataFunc.call(this, returnList);
            }
            break;
        case LibEventTypeEnum.Validated://form的赋值之后在this.dataSet中还没有实时的数据写入，需要写入之后，才能取到值。          
            if (e.dataInfo.fieldName == "BILLDATEFROM" || e.dataInfo.fieldName == "BILLDATETO" || e.dataInfo.fieldName == "FROMBILLTYPE" || e.dataInfo.fieldName == "FROMBILLNO" || e.dataInfo.fieldName == "MATERIALID") {
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
