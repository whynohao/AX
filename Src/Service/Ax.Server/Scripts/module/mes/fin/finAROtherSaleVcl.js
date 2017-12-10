finAROtherSaleVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    me = this;
};
var proto = finAROtherSaleVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finAROtherSaleVcl;
var attId = 0;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var bodyTable = this.dataSet.getTable(1);
            var standardCoilRate = masterRow.get("STANDARDCOILRATE")
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "STANDARDCOILRATE") {
                    for (var i = 0; i < bodyTable.data.items.length; i++) {

                        var detailData = bodyTable.data.items[i];
                        var conversion = this.GetData(detailData, standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 2, 1]);
                        this.SetData(detailData, data);
                    }
                    SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "CONTRACTNO")
                {
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        var detailData = bodyTable.data.items[i];
                        detailData.set("CONTRACTNO", e.dataInfo.value);
                    }
                }
                if (e.dataInfo.fieldName == "CUSTOMERID") {
                    var contactsObjectData = this.invorkBcf('GetContactsObjectData', [e.dataInfo.value]);
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
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "TAXRATE") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 1, 1]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "INVOICESALQTY") {
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
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "BWTAXAMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 9, 1]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
            }
            this.forms[0].updateRecord(masterRow);
            break;
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.tableIndex == 1) {
                bodyRow = e.dataInfo.dataRow;
            }
            if (e.dataInfo.fieldName == "ATTRIBUTENAME") {
                var MaterialId = e.dataInfo.dataRow.data["PRODUCTID"];
                var AttributeId = e.dataInfo.dataRow.data["ATTRIBUTEID"];
                var AttributeCode = e.dataInfo.dataRow.data["ATTRIBUTECODE"];
                if (AttributeCode == undefined)
                {
                    AttributeCode = "";
                }

                if (AttributeId != "") {
                    var returnData = this.invorkBcf('GetAttJson', [MaterialId, AttributeId, AttributeCode]);
                    var dataList = {
                        MaterialId: e.dataInfo.dataRow.data["PRODUCTID"],
                        AttributeId: e.dataInfo.dataRow.data["ATTRIBUTEID"],
                        AttributeDesc: e.dataInfo.dataRow.data["ATTRIBUTEDESC"],
                        AttributeCode: e.dataInfo.dataRow.data["ATTRIBUTECODE"],
                        BillNo: e.dataInfo.dataRow.data["BILLNO"],
                        Row_Id: e.dataInfo.dataRow.data["ROW_ID"]
                    };
                    this.CreatAttForm(dataList, returnData, this, e, this.FillDataRow);
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
        case LibEventTypeEnum.AddRow:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var bodyTable = this.dataSet.getTable(1);
            e.dataInfo.dataRow.set("TAXRATE", masterRow.get("TAXRATE"));
            e.dataInfo.dataRow.set("CONTRACTNO", masterRow.get("CONTRACTNO"));
    }

    proto.GetData=function(detailData, standardCoilRate, e) {
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
        if (e.dataInfo.fieldName == "INVOICESALQTY") {
            quantity = e.dataInfo.value;
        }
        else {
            quantity = detailData.get("INVOICESALQTY");
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

    proto.SetData=function(detailData, data) {
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

    proto.SumAmount=function(bodyTable, masterRow) {
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
}

//最新特征窗体
proto.CreatAttForm = function (dataList, returnData, This, row, method) {
    var MaterialId = dataList.MaterialId;
    var AttributeId = dataList.AttributeId;
    var AttributeName = dataList.AttributeName;
    var AttributeCode = dataList.AttributeCode;
    var BillNo = dataList.BillNo;
    var Row_Id = dataList.Row_Id;
    var standard = [];
    var unstandard = [];
    var isRead;
    if (returnData.length == 0) {
        Ext.Msg.alert("提示", '该产品不存在特征或特征无法获取！');
        return;
    }
    for (var i = 0; i < returnData.length; i++) {
        if (returnData[i].IsRead == 0) {
            isRead = false;
        }
        else {
            isRead = true;
        }
        if (returnData[i].Dynamic) {
            if (returnData[i].Standard) {

                unstandard.push(this.CreatTextBox(returnData[i], isRead));
            }
            else {
                standard.push(this.CreatTextBox(returnData[i], isRead));
            }
        }
        else {
            if (returnData[i].Standard) {
                unstandard.push(this.CreatComBox(returnData[i], isRead));
            }
            else {
                standard.push(this.CreatComBox(returnData[i], isRead));
            }
        }
    }
    //标准特征Panel
    var attPanel = new Ext.form.Panel({

    })
    //非标准特征Panel
    var unattPanel = new Ext.form.Panel({

    })
    //确认按钮
    var btnSaleConfirm = new Ext.Button({
        width: 200,
        height: 35,
        text: "确定",
        type: 'submit',
        handler: function () {
            var yes = true;
            var thisWin = Ext.getCmp("attWin" + BillNo + Row_Id + MaterialId);
            if (This.billAction == BillActionEnum.Modif || This.billAction == BillActionEnum.AddNew) {

                var attPanel = thisWin.items.items[0];
                var unattPanel = thisWin.items.items[1];
                var attributeId = thisWin.attributeId;
                var materialId = thisWin.materialId;
                var attDic = [];
                var msg = '';
                for (var i = 0; i < attPanel.items.length; i++) {
                    if (returnData[0].IsRequired == 1 && attPanel.items.items[i].value == null) {

                        Ext.Msg.alert("提示", '请填写【' + attPanel.items.items[i].fieldLabel + '】的值');
                        return false;
                    }
                    else {
                        if (attPanel.items.items[i].id.indexOf("numberfield") >= 0 && attPanel.items.items[i].value <= 0) {
                            Ext.Msg.alert("提示", '标准特征项【' + attPanel.items.items[i].fieldLabel + '】的值必须大于0！');
                            return false;
                        }
                        attDic.push({ AttributeId: attPanel.items.items[i].attId, AttrCode: attPanel.items.items[i].value })
                    }

                }
                if (msg.length > 0) {
                    Ext.Msg.alert("提示", '请维护标准特征项中' + msg + '的值！');
                    return false;
                }
                for (var i = 0; i < unattPanel.items.length; i++) {
                    if (unattPanel.items.items[i].value != null) {
                        attDic.push({ AttributeId: unattPanel.items.items[i].attId, AttrCode: unattPanel.items.items[i].value })
                    }
                }
                if (attDic.length > 0) {
                    var CodeDesc = This.invorkBcf('GetAttrInfo', [materialId, attributeId, attDic]);
                    yes = method(row, This, CodeDesc);
                }
                else {
                    Ext.Msg.alert("提示", '请维护标准特征！');
                }
            }
            if (yes) {
                thisWin.close();
            }

        }
    })
    //取消按钮
    var btnSaleCancel = new Ext.Button({
        width: 200,
        height: 35,
        text: "取消",
        type: 'submit',
        handler: function () {
            Ext.getCmp("attWin" + BillNo + Row_Id + MaterialId).close();
        }
    })
    //按钮Panle
    var btnSalePanel = new Ext.form.Panel({
        layout: 'column',
        width: '100%',
        defaults: {
            margin: '10 40 0 40',
            columnWidth: .5
        },
        items: [btnSaleConfirm, btnSaleCancel]
    })

    var Salewin = new Ext.create('Ext.window.Window', {
        id: "attWin" + BillNo + Row_Id + MaterialId,
        title: '特征信息',
        resizable: false,
        //closeAction: "close",
        modal: true,
        width: 600,
        height: 330,
        materialId: MaterialId,//物料ID
        attributeId: AttributeId,//特征ID
        autoScroll: true,
        layout: 'column',
        items: [{
            id: 'Att' + this.attId,
            layout: 'column',
            xtype: 'fieldset',
            title: '标准特征',
            //collapsed: true,
            collapsible: true,
            width: '96%',
            height: 200,

            defaulType: 'combobox',
            margin: '5 10 5 10',
            autoScroll: true,
            items: standard,
            listeners: {
                collapse: function (a, b) {
                    //Ext.getCmp('no'+ a.id).expand();
                },
                expand: function (a, b) {
                    Ext.getCmp('no' + a.id).collapse(true);
                }
            },
        }, {
            id: 'noAtt' + this.attId,
            layout: 'column',
            xtype: 'fieldset',
            //collapsed: true,
            collapsible: true,
            width: '96%',
            height: 200,
            margin: '5 10 0 10',
            title: '非标准特征',
            autoScroll: true,
            items: unstandard,
            listeners: {
                collapse: function (a, b) {
                    //Ext.getCmp(a.id.substr(2, a.id.length - 2)).expand();
                },
                expand: function (a, b) {
                    Ext.getCmp(a.id.substr(2, a.id.length - 2)).collapse(true);
                }
            }
        }, btnSalePanel],
    });
    this.attId++;
    Salewin.show();
    Salewin.items.items[1].collapse(true);
}
//非动态特征 combox
proto.CreatComBox = function (attData, isread) {

    var attlist = [];
    for (var i = 0; i < attData.AttrValueList.length; i++) {
        var data = { AttrCode: attData.AttrValueList[i]['AttrCode'], AttrValue: attData.AttrValueList[i]['AttrValue'] };
        attlist.push(data);
    }
    var Store = Ext.create("Ext.data.Store", {
        fields: ["AttrCode", "AttrValue"],
        data: attlist
    });
    var combox = new Ext.form.ComboBox({
        mode: 'local',
        forceSelection: true,
        triggerAction: 'all',
        displayField: 'AttrValue',
        fieldLabel: attData.AttributeItemName,
        attId: attData.AttributeItemId,//特征项ID
        value: attData.DefaultValue,//特征项的值
        valueField: 'AttrCode',
        disabled: isread,
        fields: ['AttrCode', 'AttrValue'],
        store: Store,

        //editable: true,
        //text: attData.DefaultValue,
        margin: '5 10 5 10',
        columnWidth: .5,
        labelWidth: 60,
    });
    return combox;
}
//动态特征 NumberField
proto.CreatTextBox = function (attData, isread) {
    if (attData.ValueType == 0) {
        var textbox = new Ext.form.NumberField({
            fieldLabel: attData.AttributeItemName,
            attId: attData.AttributeItemId,
            allowDecimals: true, // 允许小数点
            allowNegative: false, // 允许负数
            allowBlank: false,
            disabled: isread,
            value: attData.DefaultValue,
            maxLength: 50,
            margin: '5 10 5 10',
            columnWidth: .5,
            labelWidth: 60,
        });

    }
    else {
        var textbox = new Ext.form.TextField({
            fieldLabel: attData.AttributeItemName,
            attId: attData.AttributeItemId,
            allowBlank: false,
            value: attData.DefaultValue,
            maxLength: 50,
            margin: '5 10 5 10',
            disabled: isread,
            columnWidth: .5,
            labelWidth: 60,
        });
    }

    return textbox;
}

//填充当前行特征信息
proto.FillDataRow = function (e, This, CodeDesc) {
    e.dataInfo.dataRow.set("ATTRIBUTECODE", CodeDesc.Code);
    e.dataInfo.dataRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
    e.dataInfo.dataRow.set("ABNORMALDAY", CodeDesc.AbnormalDay);
    //设置异常天数
    //var masterRow = This.dataSet.getTable(0).data.items[0];
    //Ext.getCmp("ABNORMALDAY0_" + This.winId).setValue(CodeDesc.AbnormalDay);
    return true;
}