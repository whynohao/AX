
finAPPurInvoiceVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
     me = this;
};

var proto = finAPPurInvoiceVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finAPPurInvoiceVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (this.isEdit) {
                if (e.dataInfo.fieldName == "BtnLoadData") {
                    var contactsobjectId = this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTID'];
                    var contactsobjectName = this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTNAME'];
                    var fromBillType = this.dataSet.getTable(0).data.items[0].data['FROMBILLTYPE'];
                    var isred = this.dataSet.getTable(0).data.items[0].data['ISRED'];
                    var taxRate = this.dataSet.getTable(0).data.items[0].data['TAXRATE'];
                    var standardcoilRate = this.dataSet.getTable(0).data.items[0].data['STANDARDCOILRATE'];
                    if (contactsobjectId == "") {
                        alert("往来单位不能为空！");
                    }
                    else {
                        Ax.utils.LibVclSystemUtils.openDataFunc('fin.APPurInvoiceDataFunc', '载入来源单', [contactsobjectId, contactsobjectName, fromBillType, isred, taxRate, standardcoilRate, this]);
                    }
                }
            }
            else {
                alert("单据只有在修改状态才能载入数据！");
            }
            break;     
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.Validated:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var bodyTable = this.dataSet.getTable(1);
            var standardCoilRate = masterRow.get("STANDARDCOILRATE")
            if (e.dataInfo.tableIndex == 0) {
                if(e.dataInfo.fieldName=="STANDARDCOILRATE")
                {
                    for(var i=0;i<bodyTable.data.items.length;i++)
                    {
                        var detailData = bodyTable.data.items[i];
                        var conversion = this.GetData(detailData, standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 2, 0]);
                        this.SetData(detailData,data);
                    }
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }

                if (e.dataInfo.fieldName == "FROMBILLTYPE") {
                    Ext.getCmp('BWTOTALAMOUNT0_' + me.winId).setValue(0);
                    Ext.getCmp('BWTOTALTAXAMOUNT0_' + me.winId).setValue(0);
                    Ext.getCmp('BWTOTALTAX0_' + me.winId).setValue(0);
                    Ext.getCmp('TOTALAMOUNT0_' + me.winId).setValue(0);
                    Ext.getCmp('TOTALTAXAMOUNT0_' + me.winId).setValue(0);
                    Ext.getCmp('TOTALTAX0_' + me.winId).setValue(0);
                    me.deleteAll(1);
                }
                if (e.dataInfo.fieldName == "CONTACTSOBJECTID") {
                    Ext.getCmp('BWTOTALAMOUNT0_' + me.winId).setValue(0);
                    Ext.getCmp('BWTOTALTAXAMOUNT0_' + me.winId).setValue(0);
                    Ext.getCmp('BWTOTALTAX0_' + me.winId).setValue(0);
                    Ext.getCmp('TOTALAMOUNT0_' + me.winId).setValue(0);
                    Ext.getCmp('TOTALTAXAMOUNT0_' + me.winId).setValue(0);
                    Ext.getCmp('TOTALTAX0_' + me.winId).setValue(0);
                    me.deleteAll(1);
                }
                if (e.dataInfo.fieldName == "INVOICETYPEID") {
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        bodyTable.data.items[i].set("TAXRATE", masterRow.get("TAXRATE"));
                        var conversion = this.GetData(bodyTable.data.items[i], standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 1, 0]);
                        this.SetData(bodyTable.data.items[i], data);
                    }
                    this.SumAmount(bodyTable, masterRow);
                }
                if (e.dataInfo.fieldName == "TAXRATE") {
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        bodyTable.data.items[i].set("TAXRATE", e.dataInfo.value);
                        var conversion = this.GetData(bodyTable.data.items[i], standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 1, 0]);
                        this.SetData(bodyTable.data.items[i], data);
                    }
                    this.SumAmount(bodyTable, masterRow);
                }
            }
            if (e.dataInfo.tableIndex == 1){
                if(e.dataInfo.fieldName=="TAXRATE")
                {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 1, 0]);
                    this.SetData(e.dataInfo.dataRow,data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "INVOICEPURQTY")
                {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 3, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "NOTAXPRICE") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 5, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "TAXPRICE") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 4, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "AMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 6, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "TAXAMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 7, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "BWAMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 8, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "BWTAXAMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 9, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "INVOICEPURQTY") {
                    e.dataInfo.dataRow.set("INVOICEQTY", e.dataInfo.value);
                }
                this.forms[0].updateRecord(masterRow);
            } 
            break;
        case LibEventTypeEnum.DeleteRow:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var bodyTable = this.dataSet.getTable(1);
            this.SumAmount(bodyTable, masterRow);
            break;
    }
}


proto.GetData = function (detailData, standardCoilRate, e) {
    var taxRate;
    var price;
    var quantity;
    var taxPrice;
    var amount;
    var taxAmount;
    var tax;
    var bwAmount;
    var bwTaxAmount;
    var bwTax;
    var standardcoilrate;
    if (e.dataInfo.fieldName == "TAXRATE") {
        taxRate = e.dataInfo.value;
    }
    else {
        taxRate = detailData.get("TAXRATE");
    }
    if (e.dataInfo.fieldName == "INVOICEPURQTY") {
        quantity = e.dataInfo.value;
    }
    else {
        quantity = detailData.get("INVOICEPURQTY");
    }

    if (e.dataInfo.fieldName == "NOTAXPRICE") {
        price = e.dataInfo.value;
    }
    else {
        price = detailData.get("NOTAXPRICE");
    }
    if (e.dataInfo.fieldName == "TAXPRICE") {
        taxPrice = e.dataInfo.value;
    }
    else {
        taxPrice = detailData.get("TAXPRICE");
    }
    if (e.dataInfo.fieldName == "AMOUNT") {
        amount = e.dataInfo.value;
    }
    else {
        amount = detailData.get("AMOUNT");
    }
    if (e.dataInfo.fieldName == "TAXAMOUNT") {
        taxAmount = e.dataInfo.value;
    }
    else {
        taxAmount = detailData.get("TAXAMOUNT");
    }
    if (e.dataInfo.fieldName == "TAX") {
        tax = e.dataInfo.value;
    }
    else {
        tax = detailData.get("TAX");
    }
    if (e.dataInfo.fieldName == "BWAMOUNT") {
        bwAmount = e.dataInfo.value;
    }
    else {
        bwAmount = detailData.get("BWAMOUNT");
    }
    if (e.dataInfo.fieldName == "BWTAXAMOUNT") {
        bwTaxAmount = e.dataInfo.value;
    }
    else {
        bwTaxAmount = detailData.get("BWTAXAMOUNT");
    }
    if (e.dataInfo.fieldName == "BWTAX") {
        bwTax = e.dataInfo.value;
    }
    else {
        bwTax = detailData.get("BWTAX");
    }
    if (e.dataInfo.fieldName == "STANDARDCOILRATE") {
        standardcoilrate = e.dataInfo.value;
    }
    else {
        standardcoilrate = standardCoilRate;
    }

    var conversion = {
        TaxRate: taxRate,
        StandardcoilRate: standardcoilrate,
        Quantity: quantity,
        taxPrice: taxPrice,
        Price: price,
        Amount: amount,
        TaxAmount: taxAmount,
        Tax: tax,
        BwAmount: bwAmount,
        BwTaxAmount: bwTaxAmount,
        BwTax: bwTax
    };
    return conversion;
}

proto.SetData = function (detailData, data) {
    detailData.set("TAXPRICE", data.TaxPrice);
    detailData.set("TAXRATE", data.TaxRate);
    detailData.set("NOTAXPRICE", data.Price);
    detailData.set("AMOUNT", data.Amount);
    detailData.set("TAXAMOUNT", data.TaxAmount);
    detailData.set("TAX", data.Tax);
    detailData.set("BWAMOUNT", data.BwAmount);
    detailData.set("BWTAXAMOUNT", data.BwTaxAmount);
    detailData.set("BWTAX", data.BwTax);
    detailData.set("INVOICEPURQTY", data.Quantity);
}

proto.SumAmount = function (bodyTable, masterRow) {
    var bwTotalAmount = 0;
    var bwTotalTaxAmount = 0;
    var bwTotalTax = 0;
    var totalAmount = 0;
    var totalTaxAmount = 0;
    var totalTax = 0;
    for (var i = 0; i < bodyTable.data.items.length; i++) {
        var detailData = bodyTable.data.items[i];
        bwTotalAmount = detailData.get("BWAMOUNT") + bwTotalAmount;
        bwTotalTaxAmount = detailData.get("BWTAXAMOUNT") + bwTotalTaxAmount;
        bwTotalTax = detailData.get("BWTAX") + bwTotalTax;
        totalAmount = detailData.get("AMOUNT") + totalAmount;
        totalTaxAmount = detailData.get("TAXAMOUNT") + totalTaxAmount;
        totalTax = detailData.get("TAX") + totalTax;
    }
    vcl = me;
    Ext.getCmp('BWTOTALAMOUNT0_' + vcl.winId).setValue(bwTotalAmount);
    Ext.getCmp('BWTOTALTAXAMOUNT0_' + vcl.winId).setValue(bwTotalTaxAmount);
    Ext.getCmp('BWTOTALTAX0_' + vcl.winId).setValue(bwTotalTax);
    Ext.getCmp('TOTALAMOUNT0_' + vcl.winId).setValue(totalAmount);
    Ext.getCmp('TOTALTAXAMOUNT0_' + vcl.winId).setValue(totalTaxAmount);
    Ext.getCmp('TOTALTAX0_' + vcl.winId).setValue(totalTax);
} 


