finARSaleInvoiceVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    me = this;
};
var proto = finARSaleInvoiceVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finARSaleInvoiceVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch(e.libEventType)
    {
        case LibEventTypeEnum.Validating:

            break;
        case LibEventTypeEnum.Validated:
            vcl = me;
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var bodyTable = this.dataSet.getTable(1);
            var standardCoilRate = masterRow.get("STANDARDCOILRATE");
            if (e.dataInfo.tableIndex == 0) {
                if(e.dataInfo.fieldName=="STANDARDCOILRATE")
                {
                    for(var i=0;i<bodyTable.data.items.length;i++)
                    {
                        var detailData = bodyTable.data.items[i];
                        var conversion =this.GetData(detailData, standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 2, 1]);
                        this.SetData(detailData,data);
                    }
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "FROMBILLTYPE") {
                    this.deleteAll(1);
                }
                if (e.dataInfo.fieldName == "CUSTOMERID")
                {
                    //bodyTable.deleteAll(1);
                    var contactsObjectData= this.invorkBcf('GetContactsObjectData', [e.dataInfo.value]);
                    Ext.getCmp("CONTACTSOBJECTID0_" + vcl.winId).store.add({ Id: contactsObjectData.ContactsObjectId, Name: contactsObjectData.ContactsobjectName });
                    Ext.getCmp('INVOICETYPEID0_' + vcl.winId).store.add({ Id: contactsObjectData.InvoiceTypeId, Name: contactsObjectData.InvoiceTypeName })
                    Ext.getCmp('CURRENCYID0_' + vcl.winId).store.add({ Id: contactsObjectData.CurrencyId, Name: contactsObjectData.CurrencyName })
                    Ext.getCmp('CONTACTSOBJECTID0_' + vcl.winId).setValue(contactsObjectData.ContactsObjectId);
                    Ext.getCmp('INVOICETYPEID0_' + vcl.winId).setValue(contactsObjectData.InvoiceTypeId);
                    Ext.getCmp('CURRENCYID0_' + vcl.winId).setValue(contactsObjectData.CurrencyId);
                    Ext.getCmp('EXPRESSSYMBOL0_' + vcl.winId).setValue(contactsObjectData.ExpressSymbol);
                    Ext.getCmp('ISSTANDARDCOIL0_' + vcl.winId).setValue(contactsObjectData.IsStandardCoil);
                    Ext.getCmp('STANDARDCOILRATE0_' + vcl.winId).setValue(contactsObjectData.StandardCoilRate);
                    Ext.getCmp('TAXRATE0_' + vcl.winId).setValue(contactsObjectData.TaxRate);
                    this.forms[0].updateRecord(masterRow);
                    for(var i=0;i<bodyTable.data.items.length;i++)
                    {
                        var bodyRow = bodyTable.data.items[i];
                        bodyRow.set("TAXRATE", contactsObjectData.TaxRate);
                        var conversion = this.GetData(bodyRow, standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 1, 1]);
                        data.StandardCoilRate = contactsObjectData.StandardCoilRate;
                        //this.SetData(bodyRow, data);
                        //var conversion = this.GetData(bodyRow, standardCoilRate, e);

                        data = this.invorkBcf('CalculateAmount', [data, 2, 1]);
                        this.SetData(bodyRow, data);
                    }
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "INVOICETYPEID") {
                    var invoiceTypeData = this.invorkBcf('GetInvoiceTypeData', [e.dataInfo.value]);
                    Ext.getCmp('TAXRATE0_' + vcl.winId).setValue(invoiceTypeData.TaxRate);
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        var bodyRow = bodyTable.data.items[i];
                        bodyRow.set("TAXRATE", invoiceTypeData.TaxRate);
                        var conversion = this.GetData(bodyRow, standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 1, 1]);
                        this.SetData(bodyRow, data);
                    }
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "CONTACTSOBJECTID") {
                    var contactsObjectData = this.invorkBcf('GetContactsObjectData', [e.dataInfo.value]);
                    Ext.getCmp('INVOICETYPEID0_' + vcl.winId).store.add({ Id: contactsObjectData.InvoiceTypeId, Name: contactsObjectData.InvoiceTypeName })
                    Ext.getCmp('CURRENCYID0_' + vcl.winId).store.add({ Id: contactsObjectData.CurrencyId, Name: contactsObjectData.CurrencyName })
                    Ext.getCmp('INVOICETYPEID0_' + vcl.winId).setValue(contactsObjectData.InvoiceTypeId);
                    Ext.getCmp('CURRENCYID0_' + vcl.winId).setValue(contactsObjectData.CurrencyId);
                    Ext.getCmp('EXPRESSSYMBOL0_' + vcl.winId).setValue(contactsObjectData.ExpressSymbol);
                    Ext.getCmp('ISSTANDARDCOIL0_' + vcl.winId).setValue(contactsObjectData.IsStandardCoil);
                    Ext.getCmp('STANDARDCOILRATE0_' + vcl.winId).setValue(contactsObjectData.StandardCoilRate);
                    Ext.getCmp('TAXRATE0_' + vcl.winId).setValue(contactsObjectData.TaxRate);
                    this.forms[0].updateRecord(masterRow);
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        var bodyRow = bodyTable.data.items[i];
                        bodyRow.set("TAXRATE", contactsObjectData.TaxRate);
                        var conversion = this.GetData(bodyRow, standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 1, 1]);
                        data.StandardCoilRate = contactsObjectData.StandardCoilRate;
                        //this.SetData(bodyRow, data);
                        //var conversion = this.GetData(bodyRow, standardCoilRate, e);
                        data = this.invorkBcf('CalculateAmount', [data, 2, 1]);
                        this.SetData(bodyRow, data);
                    }
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "TAXRATE") {
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        var bodyRow = bodyTable.data.items[i];
                        bodyRow.set("TAXRATE", e.dataInfo.value);
                        var conversion = this.GetData(bodyRow, standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 1, 1]);
                        this.SetData(bodyRow, data);
                    }
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
              
            }
            if (e.dataInfo.tableIndex == 1){
                if(e.dataInfo.fieldName=="TAXRATE")
                {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 1, 1]);
                    this.SetData(e.dataInfo.dataRow,data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if(e.dataInfo.fieldName=="INVOICESALQTY")
                {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 3, 1]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "NOTAXPRICE") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 5, 1]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "TAXPRICE") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 4, 1]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "AMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 6, 1]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "TAXAMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 7, 1]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "BWAMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 8, 1]);
                    SetData(e.dataInfo.dataRow, data);
                    SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "BWTAXAMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 9, 1]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "INVOICESALQTY") {
                    e.dataInfo.dataRow.set("INVOICEQTY", e.dataInfo.value);
                }
            }
            this.forms[0].updateRecord(masterRow);
            break;
        case LibEventTypeEnum.ButtonClick:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var contactsobjectId = masterRow.get('CUSTOMERID');
            var contractNo = masterRow.get("CONTRACTNO");
            if (e.dataInfo.fieldName == "BtnLoadData")
            {
                if(this.isEdit)
                {
                    if (contactsobjectId == '' || contactsobjectId==null)
                    {
                        Ext.Msg.alert("系统提示", "往来单位不能为空");
                        return;
                    }
                    if (contractNo == '' || contractNo == null)
                    {
                        Ext.Msg.alert("系统提示", "合同号不能为空");
                        return;
                    }
                    this.forms[0].updateRecord(masterRow);
                    Ax.utils.LibVclSystemUtils.openDataFunc("fin.ARSalInvoiceDataFunc", "应收数据导入", [this, "FINARSALDATAFUNCDETAIL"]);
                    
                }
                else
                {
                    Ext.Msg.alert("系统提示", "非编辑状态，不可操作！");
                }
            }
            break;
        case LibEventTypeEnum.DeleteRow:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var bodyTable = this.dataSet.getTable(1);
            this.SumAmount(bodyTable, masterRow);
            //this.forms[0].loadRecord(masterRow);
            //this.forms[0].updateRecord(masterRow);
            break;
        case LibEventTypeEnum.BeforeAddRow:
            e.dataInfo.cancel = true;
            Ext.Msg.alert("系统提示", "不能新增行！");
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
    if (e.dataInfo.fieldName== "TAXRATE"){
        taxRate = e.dataInfo.value;
    }
    else{
        taxRate=detailData.get("TAXRATE");
    }
    if (e.dataInfo.fieldName == "INVOICESALQTY") {
        quantity = e.dataInfo.value;
    }
    else {
        quantity =detailData.get("INVOICESALQTY");
    }
   
    if (e.dataInfo.fieldName== "NOTAXPRICE") {
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

proto.SetData=function(detailData,data)
{
    detailData.set("TAXPRICE", data.TaxPrice);
    detailData.set("TAXRATE", data.TaxRate);
    detailData.set("NOTAXPRICE", data.Price);
    detailData.set("AMOUNT", data.Amount);
    detailData.set("TAXAMOUNT", data.TaxAmount);
    detailData.set("TAX", data.Tax);
    detailData.set("BWAMOUNT", data.BwAmount);
    detailData.set("BWTAXAMOUNT", data.BwTaxAmount);
    detailData.set("BWTAX", data.BwTax);
    detailData.set("INVOICESALQTY", data.Quantity);
}

proto.SumAmount = function (bodyTable, masterRow)
{
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
    //masterRow.set("BWTOTALAMOUNT", bwTotalAmount);
    //masterRow.set("BWTOTALTAXAMOUNT", bwTotalTaxAmount);
    //masterRow.set("BWTOTALTAX", bwTotalTax);
    //masterRow.set("TOTALAMOUNT", totalAmount);
    //masterRow.set("TOTALTAXAMOUNT", totalTaxAmount);
    //masterRow.set("TOTALTAX", totalTax);
    vcl = me;
    Ext.getCmp('BWTOTALAMOUNT0_' + vcl.winId).setValue(bwTotalAmount);
    Ext.getCmp('BWTOTALTAXAMOUNT0_' + vcl.winId).setValue(bwTotalTaxAmount);
    Ext.getCmp('BWTOTALTAX0_' + vcl.winId).setValue(bwTotalTax);
    Ext.getCmp('TOTALAMOUNT0_' + vcl.winId).setValue(totalAmount);
    Ext.getCmp('TOTALTAXAMOUNT0_' + vcl.winId).setValue(totalTaxAmount);
    Ext.getCmp('TOTALTAX0_' + vcl.winId).setValue(totalTax);
    //masterRow.data["BWTOTALAMOUNT"] = bwTotalAmount;
    //masterRow.data["BWTOTALTAXAMOUNT"] = bwTotalTaxAmount;
    //masterRow.data["BWTOTALTAX"] = bwTotalTax;
    //masterRow.data["TOTALAMOUNT"] = totalAmount;
    //masterRow.data["TOTALTAXAMOUNT"] = totalTaxAmount;
    //masterRow.data["TOTALTAX"] = totalTax;
}


