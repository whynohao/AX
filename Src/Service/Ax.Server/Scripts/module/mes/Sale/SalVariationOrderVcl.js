SalVariationOrderVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var attId = 0;
var proto = SalVariationOrderVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = SalVariationOrderVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.AddRow:

            if (e.dataInfo.tableIndex == 1) {
                var btable = this.dataSet.getTable(1).data;

                var desc = btable.items[0].get("ATTRIBUTECODE");
                e.dataInfo.dataRow.set("ATTRIBUTECODE", desc);


            }
            break;
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.fieldName == "ATTRIBUTENAME") {
                var AttributeId = e.dataInfo.dataRow.get("ATTRIBUTEID");
                var code = e.dataInfo.dataRow.data["ATTRIBUTECODE"];
                if (AttributeId != "") {
                    var AttDicLst = this.invorkBcf('GetAtt', ["", AttributeId, code]);
                    var dataList = {
                        MaterialId: e.dataInfo.dataRow.data["MATERIALID"],
                        AttributeId: AttributeId,
                        AttributeDesc: "",
                        AttributeCode: "",
                        BillNo: e.dataInfo.dataRow.data["BILLNO"],
                        Row_Id: e.dataInfo.dataRow.data["ROW_ID"]

                    };
                    CreatAttForm(dataList, AttDicLst, this, e, FillDataRow);

                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                //更改汇率计算
                if (e.dataInfo.fieldName == "STANDARDCOILRATE") {
                    var items = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < items.length; i++) {
                        var ScmMoneyBcf = {}; //金额、数量换算
                        ScmMoneyBcf.DealsQuantity = this.dataSet.getTable(1).data.items[i].get("DEALSQUANTITY");//交易数量
                        ScmMoneyBcf.TaxRate = this.dataSet.getTable(1).data.items[i].get("TAXRATE"); //税率;
                        ScmMoneyBcf.Price = this.dataSet.getTable(1).data.items[i].get("PRICE"); //单价
                        ScmMoneyBcf.TaxPrice = this.dataSet.getTable(1).data.items[i].get("TAXPRICE"); //含税单价
                        ScmMoneyBcf.Amount = this.dataSet.getTable(1).data.items[i].get("AMOUNT"); //金额
                        ScmMoneyBcf.TaxAmount = this.dataSet.getTable(1).data.items[i].get("TAXAMOUNT"); //含税金额
                        ScmMoneyBcf.Taxes = this.dataSet.getTable(1).data.items[i].get("TAXES"); //含税金额
                        ScmMoneyBcf.BWAmount = this.dataSet.getTable(1).data.items[i].get("BWAMOUNT"); //本位币金额
                        ScmMoneyBcf.BWTaxAmount = this.dataSet.getTable(1).data.items[i].get("BWTAXAMOUNT"); //本位币含税金额
                        ScmMoneyBcf.BWTaxes = this.dataSet.getTable(1).data.items[i].get("BWTAXES"); //本位币税额                      

                        //交易数量变更引起的其它字段的变更
                        ScmMoneyBcf.StandardcoilRate = e.dataInfo.value;//汇率
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeStandardcoilRate]);
                        if (data != null) {
                            this.dataSet.getTable(1).data.items[i].set("BWAMOUNT", data["BWAmount"]);//本币金额
                            this.dataSet.getTable(1).data.items[i].set("BWTAXAMOUNT", data["BWTaxAmount"]);//本币含税金额
                            this.dataSet.getTable(1).data.items[i].set("BWTAXES", data["BWTaxes"]);
                        }

                    }
                }
                if (e.dataInfo.fieldName == "CURRENCYID") {
                    var items = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < items.length; i++) {
                        var ScmMoneyBcf = {}; //金额、数量换算
                        ScmMoneyBcf.DealsQuantity = this.dataSet.getTable(1).data.items[i].get("DEALSQUANTITY");//交易数量
                        ScmMoneyBcf.TaxRate = this.dataSet.getTable(1).data.items[i].get("TAXRATE"); //税率;
                        ScmMoneyBcf.Price = this.dataSet.getTable(1).data.items[i].get("PRICE"); //单价
                        ScmMoneyBcf.TaxPrice = this.dataSet.getTable(1).data.items[i].get("TAXPRICE"); //含税单价
                        ScmMoneyBcf.Amount = this.dataSet.getTable(1).data.items[i].get("AMOUNT"); //金额
                        ScmMoneyBcf.TaxAmount = this.dataSet.getTable(1).data.items[i].get("TAXAMOUNT"); //含税金额
                        ScmMoneyBcf.Taxes = this.dataSet.getTable(1).data.items[i].get("TAXES"); //含税金额
                        ScmMoneyBcf.BWAmount = this.dataSet.getTable(1).data.items[i].get("BWAMOUNT"); //本位币金额
                        ScmMoneyBcf.BWTaxAmount = this.dataSet.getTable(1).data.items[i].get("BWTAXAMOUNT"); //本位币含税金额
                        ScmMoneyBcf.BWTaxes = this.dataSet.getTable(1).data.items[i].get("BWTAXES"); //本位币税额                      

                        //交易数量变更引起的其它字段的变更
                        ScmMoneyBcf.StandardcoilRate = this.dataSet.getTable(0).data.items[0].get("STANDARDCOILRATE");//汇率
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeStandardcoilRate]);
                        if (data != null) {
                            this.dataSet.getTable(1).data.items[i].set("BWAMOUNT", data["BWAmount"]);//本币金额
                            this.dataSet.getTable(1).data.items[i].set("BWTAXAMOUNT", data["BWTaxAmount"]);//本币含税金额
                            this.dataSet.getTable(1).data.items[i].set("BWTAXES", data["BWTaxes"]);
                        }

                    }
                }
                if (e.dataInfo.fieldName == "TAXRATE") {
                    var items = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < items.length; i++) {
                        var ScmMoneyBcf = {}; //金额、数量换算
                        items[i].set("TAXRATE", this.dataSet.getTable(0).data.items[0].get("TAXRATE"));
                        ScmMoneyBcf.DealsQuantity = this.dataSet.getTable(1).data.items[i].get("DEALSQUANTITY");//交易数量
                        ScmMoneyBcf.Price = this.dataSet.getTable(1).data.items[i].get("PRICE"); //单价
                        ScmMoneyBcf.StandardcoilRate = this.dataSet.getTable(0).data.items[0].get("STANDARDCOILRATE"); //汇率
                        ScmMoneyBcf.TaxPrice = this.dataSet.getTable(1).data.items[i].get("TAXPRICE"); //含税单价
                        ScmMoneyBcf.Amount = this.dataSet.getTable(1).data.items[i].get("AMOUNT"); //金额
                        ScmMoneyBcf.TaxAmount = this.dataSet.getTable(1).data.items[i].get("TAXAMOUNT"); //含税金额
                        ScmMoneyBcf.Taxes = this.dataSet.getTable(1).data.items[i].get("TAXES"); //税额
                        ScmMoneyBcf.BWAmount = this.dataSet.getTable(1).data.items[i].get("BWAMOUNT"); //本位币金额
                        ScmMoneyBcf.BWTaxAmount = this.dataSet.getTable(1).data.items[i].get("BWTAXAMOUNT"); //本位币含税金额
                        ScmMoneyBcf.BWTaxes = this.dataSet.getTable(1).data.items[i].get("BWTAXES"); //本位币税额                      

                        //交易数量变更引起的其它字段的变更
                        ScmMoneyBcf.TaxRate = e.dataInfo.value;//税率
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxRate]);
                        if (data != null) {
                            this.dataSet.getTable(1).data.items[i].set("TAXPRICE", data["TaxPrice"]);//含税单价
                            this.dataSet.getTable(1).data.items[i].set("TAXAMOUNT", data["TaxAmount"]);//含税金额
                            this.dataSet.getTable(1).data.items[i].set("TAXES", data["Taxes"]);//税额
                            this.dataSet.getTable(1).data.items[i].set("BWTAXAMOUNT", data["BWTaxAmount"]);//本币含税金额
                            this.dataSet.getTable(1).data.items[i].set("BWTAXES", data["BWTaxes"]);//本位币税额 
                        }

                    }
                }
                if (e.dataInfo.fieldName == "INVOICETYPEID") {
                    var items = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < items.length; i++) {
                        var ScmMoneyBcf = {}; //金额、数量换算
                        items[i].set("TAXRATE", this.dataSet.getTable(0).data.items[0].get("TAXRATE"));
                        ScmMoneyBcf.DealsQuantity = this.dataSet.getTable(1).data.items[i].get("DEALSQUANTITY");//交易数量
                        ScmMoneyBcf.Price = this.dataSet.getTable(1).data.items[i].get("PRICE"); //单价
                        ScmMoneyBcf.StandardcoilRate = this.dataSet.getTable(0).data.items[0].get("STANDARDCOILRATE"); //汇率
                        ScmMoneyBcf.TaxPrice = this.dataSet.getTable(1).data.items[i].get("TAXPRICE"); //含税单价
                        ScmMoneyBcf.Amount = this.dataSet.getTable(1).data.items[i].get("AMOUNT"); //金额
                        ScmMoneyBcf.TaxAmount = this.dataSet.getTable(1).data.items[i].get("TAXAMOUNT"); //含税金额
                        ScmMoneyBcf.Taxes = this.dataSet.getTable(1).data.items[i].get("TAXES"); //税额
                        ScmMoneyBcf.BWAmount = this.dataSet.getTable(1).data.items[i].get("BWAMOUNT"); //本位币金额
                        ScmMoneyBcf.BWTaxAmount = this.dataSet.getTable(1).data.items[i].get("BWTAXAMOUNT"); //本位币含税金额
                        ScmMoneyBcf.BWTaxes = this.dataSet.getTable(1).data.items[i].get("BWTAXES"); //本位币税额                      

                        //交易数量变更引起的其它字段的变更
                        ScmMoneyBcf.TaxRate = this.dataSet.getTable(0).data.items[0].get("TAXRATE");//税率
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxRate]);
                        if (data != null) {
                            this.dataSet.getTable(1).data.items[i].set("TAXPRICE", data["TaxPrice"]);//含税单价
                            this.dataSet.getTable(1).data.items[i].set("TAXAMOUNT", data["TaxAmount"]);//含税金额
                            this.dataSet.getTable(1).data.items[i].set("TAXES", data["Taxes"]);//税额
                            this.dataSet.getTable(1).data.items[i].set("BWTAXAMOUNT", data["BWTaxAmount"]);//本币含税金额
                            this.dataSet.getTable(1).data.items[i].set("BWTAXES", data["BWTaxes"]);//本位币税额 
                        }

                    }
                }
                if (e.dataInfo.fieldName == "FROMTYPE" || e.dataInfo.fieldName == "RELATIONCODE") {
                    this.forms[0].updateRecord(e.dataInfo.dataRow);
                }
                if (e.dataInfo.fieldName == 'FROMBILLNO') {
                    var headTableRow = this.dataSet.getTable(0).data.items[0];
                    var bodyTable = this.dataSet.getTable(1);
                    var returnData = this.invorkBcf("GetProjectData", [headTableRow.data["FROMBILLNO"]]);
                    if (e.dataInfo.value == null) {
                        bodyTable.removeAll();
                    }
                    else {
                        if (returnData.length == 0) {
                            Ext.Msg.alert("提示", "项目单物料为空！");
                            return;
                        }
                        fillProductOrder(this, returnData);
                    }
                }

            }
            if (e.dataInfo.tableIndex == 1) {
                var ScmMoneyBcf = {}; //金额、数量换算
                ScmMoneyBcf.DealsQuantity = e.dataInfo.dataRow.get("DEALSQUANTITY");//交易数量
                ScmMoneyBcf.TaxRate = e.dataInfo.dataRow.get("TAXRATE"); //税率;
                ScmMoneyBcf.Price = e.dataInfo.dataRow.get("PRICE"); //单价
                ScmMoneyBcf.TaxPrice = e.dataInfo.dataRow.get("TAXPRICE"); //含税单价
                ScmMoneyBcf.Amount = e.dataInfo.dataRow.get("AMOUNT"); //金额
                ScmMoneyBcf.TaxAmount = e.dataInfo.dataRow.get("TAXAMOUNT"); //含税金额
                ScmMoneyBcf.Taxes = e.dataInfo.dataRow.get("TAXES"); //含税金额
                ScmMoneyBcf.BWAmount = e.dataInfo.dataRow.get("BWAMOUNT"); //本位币金额
                ScmMoneyBcf.BWTaxAmount = e.dataInfo.dataRow.get("BWTAXAMOUNT"); //本位币含税金额
                ScmMoneyBcf.BWTaxes = e.dataInfo.dataRow.get("BWTAXES"); //本位币税额
                ScmMoneyBcf.StandardcoilRate = masterRow.get("STANDARDCOILRATE"); //汇率

                if (e.dataInfo.fieldName == "MATERIALID") {
                    if (e.dataInfo.value.length > 0) {
                        var hasdealsuqntity = e.dataInfo.dataRow.data["HASDEALSUQNTITY"];
                        var returnData = this.invorkBcf('GetUnitJson', [e.dataInfo.value]);
                        var list = returnData;//一般是中间层返回来的数据
                        if (list != undefined && list.length > 0) {
                            var info = list[0];
                            e.dataInfo.dataRow.set("DEALSUNITID", info.UNITID);
                            e.dataInfo.dataRow.set("DEALSUNITNO", info.UNITNO);
                            e.dataInfo.dataRow.set("DEALSUNITNAME", info.UNITNAME);
                        }
                    }
                }


                //交易单位
                if (e.dataInfo.fieldName == "DEALSUNITID") {
                    if (e.dataInfo.value.length > 0) {
                        e.dataInfo.dataRow.set("DEALSUNITNO", "");//设交易单位标识为空
                        var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.value, e.dataInfo.dataRow.data["DEALSUNITNO"], 0, e.dataInfo.dataRow.get("DEALSQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 0]);
                        e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                    }
                }
                //交易单位标识
                if (e.dataInfo.fieldName == "DEALSUNITNO") {
                    var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.data["DEALSUNITID"], e.dataInfo.value, 0, e.dataInfo.dataRow.get("DEALSQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 0]);
                    e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                }
                if (e.dataInfo.fieldName == "QUANTITY") {
                    if (e.dataInfo.value >= 0) {
                        var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO"), e.dataInfo.value, e.dataInfo.dataRow.get("DEALSQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 1]);
                        e.dataInfo.dataRow.set("DEALSQUANTITY", unitData.ConverQuantity);
                        if (unitData.ErrorType == 1) {
                            alert("通过物料，基础单位，交易单位无法在对应物料表中找到明细！");
                        }
                        else if (unitData.ErrorType == 2) {
                            alert("物料明细表中启动了浮动，数量超出范围！");
                        }

                        //对比最小批量返回新数量、最小批量、最小批量倍数
                        var infoList = this.invorkBcf('GetQuantity', [e.dataInfo.dataRow.data["DEALSQUANTITY"], e.dataInfo.dataRow.data["MATERIALID"]]);
                        var info = infoList[0];
                        if (e.dataInfo.dataRow.data["DEALSQUANTITY"] != info.DEALSQUANTITY) {
                            Ext.Msg.alert("提示", "物料" + e.dataInfo.dataRow.data["MATERIALNAME"] + "的采购最小批量是" + info.PURCHASEQTY + "，最小批量倍数是" + info.BATCHTIMES + " ，数量" + e.dataInfo.dataRow.data["DEALSQUANTITY"] + "不符合采购标准，系统会讲数量更改为" + info.DEALSQUANTITY + "，请知悉！");
                            e.dataInfo.dataRow.set("DEALSQUANTITY", info.DEALSQUANTITY);
                        }
                        //交易数量变更引起的其它字段的变更
                        ScmMoneyBcf.DealsQuantity = info.DEALSQUANTITY;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeDealsQuantity]);
                        getPurchaseOrder.call(this, e, data);
                    }

                }

                if (e.dataInfo.fieldName == "DEALSQUANTITY") {
                    if (e.dataInfo.value >= 0) {
                        //对比最小批量返回新数量、最小批量、最小批量倍数
                        var infoList = this.invorkBcf('GetQuantity', [e.dataInfo.value, e.dataInfo.dataRow.data["MATERIALID"]]);
                        var info = infoList[0];
                        if (e.dataInfo.value != info.DEALSQUANTITY) {
                            Ext.Msg.alert("提示", "物料" + e.dataInfo.dataRow.data["MATERIALNAME"] + "的采购最小批量是" + info.PURCHASEQTY + "，最小批量倍数是" + info.BATCHTIMES + " ，数量" + e.dataInfo.value + "不符合采购标准，系统会讲数量更改为" + info.DEALSQUANTITY + "，请知悉！");
                            Ext.getCmp('DEALSQUANTITY1_' + this.winId).setValue(info.DEALSQUANTITY);
                        }
                        //金额变更引起的其它字段的变更
                        ScmMoneyBcf.DealsQuantity = info.DEALSQUANTITY;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeDealsQuantity]);
                        getPurchaseOrder.call(this, e, data);

                        //交易数量变更引发数量变化
                        var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO"), 0, info.DEALSQUANTITY, e.dataInfo.dataRow.get("UNITID"), 0]);
                        e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                    }
                }
                if (e.dataInfo.fieldName == "PRICE") {
                    if (e.dataInfo.value >= 0) {
                        //单价变更引起的其它字段的变更
                        ScmMoneyBcf.Price = e.dataInfo.value;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangePrice]);
                        getPurchaseOrder.call(this, e, data);
                    }
                }

                if (e.dataInfo.fieldName == "TAXPRICE") {
                    if (e.dataInfo.value >= 0) {
                        //金额变更引起的其它字段的变更
                        ScmMoneyBcf.TaxPrice = e.dataInfo.value;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxPrice]);
                        getPurchaseOrder.call(this, e, data);

                    }
                }

                if (e.dataInfo.fieldName == "AMOUNT") {
                    if (e.dataInfo.value >= 0 && parseFloat(e.dataInfo.dataRow.data["DEALSQUANTITY"]) > 0) {
                        //金额变更引起的其它字段的变更
                        ScmMoneyBcf.Amount = e.dataInfo.value;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeAmount]);
                        getPurchaseOrder.call(this, e, data);
                    }
                    else {
                        e.dataInfo.cancel = true;
                    }
                }

                if (e.dataInfo.fieldName == "TAXAMOUNT") {
                    if (e.dataInfo.value >= 0 && parseFloat(e.dataInfo.dataRow.data["DEALSQUANTITY"]) > 0) {
                        //金额变更引起的其它字段的变更
                        ScmMoneyBcf.TaxAmount = e.dataInfo.value;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxAmount]);
                        getPurchaseOrder.call(this, e, data);
                    }
                    else {
                        e.dataInfo.cancel = true;
                    }
                }

                if (e.dataInfo.fieldName == "TAXES") {
                    if (e.dataInfo.value >= 0 && parseFloat(e.dataInfo.dataRow.data["DEALSQUANTITY"]) > 0) {
                        //税额变更引起的其它字段的变更
                        ScmMoneyBcf.Taxes = e.dataInfo.value;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxes]);
                        getPurchaseOrder.call(this, e, data);
                    }
                    else {
                        e.dataInfo.cancel = true;
                    }

                }

                if (e.dataInfo.fieldName == "TAXRATE") {
                    if (e.dataInfo.value >= 0) {
                        //税率额变更引起的其它字段的变更
                        ScmMoneyBcf.TaxRate = e.dataInfo.value;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxRate]);
                        getPurchaseOrder.call(this, e, data);

                    }
                }
                if (e.dataInfo.fieldName == "BWAMOUNT") {
                    if (e.dataInfo.value >= 0) {
                        //本位币金额变更引起的其它字段的变更
                        ScmMoneyBcf.BWAmount = e.dataInfo.value;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeBWAmount]);
                        getPurchaseOrder.call(this, e, data);

                    }
                }
                if (e.dataInfo.fieldName == "BWTAXAMOUNT") {
                    if (e.dataInfo.value >= 0) {
                        //本位币含税金额变更引起的其它字段的变更
                        ScmMoneyBcf.BWTaxAmount = e.dataInfo.value;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeBWTaxAmount]);
                        getPurchaseOrder.call(this, e, data);

                    }
                }
            }
    }
}
//填充组合品窗口的特征信息
function FillCombineForm(panel, This, CodeDesc) {
    for (var i = 0; i < newPanel.items.items.length  ; i++) {
        if (newPanel.items.items[i].materialId == panel.materialId && newPanel.items.items[i].attributeCode == CodeDesc.Code &&

newPanel.items.items[i].id != panel.id) {
            Ext.Msg.alert("提示", '该行与第' + (i + 1) + '行重复！');
            return false;
        }
    }
    panel.items.items[5].setValue(CodeDesc.Code);
    panel.items.items[6].setValue(CodeDesc.Desc);
    panel.attributeCode = CodeDesc.Code;
    panel.attributeDesc = CodeDesc.Desc;
    panel.day = CodeDesc.AbnormalDay;
    return true;

}

//最新特征窗体
function CreatAttForm(dataList, AttDicLst, This, row, method) {

    var MaterialId = dataList.MaterialId;
    var AttributeId = dataList.AttributeId;
    var AttributeName = dataList.AttributeName;
    var AttributeCode = dataList.AttributeCode;
    var BillNo = dataList.BillNo;
    var Row_Id = dataList.Row_Id;
    var standard = [];
    var unstandard = [];
    var isRead;
    if (AttDicLst.length == 0) {
        Ext.Msg.alert("提示", '该产品不存在特征或特征无法获取！');
        return;
    }
    var panelList = [];
    var collapsed = false;
    for (var i = 0; i < AttDicLst.length; i++) {
        var fieldArray = [];

        for (var j = 0; j < AttDicLst[i].List.length; j++) {
            if (AttDicLst[i].List[j].IsRead == 0) {
                isRead = false;
            }
            else {
                isRead = true;
            }
            if (AttDicLst[i].List[j].Dynamic) {

                fieldArray.push(CreatTextBox(AttDicLst[i].List[j], isRead));
            }
            else {
                fieldArray.push(CreatComBox(AttDicLst[i].List[j], isRead));
            }
        }


        var standardPanel = new Ext.form.FieldSet({
            id: 'Att' + attId + AttDicLst[i].AttrItemTypeId,
            layout: 'column',
            xtype: 'fieldset',
            title: "<lable><font size=3 ><B>" + AttDicLst[i].AttrItemTypeName + "</B></font></lable>",
            //collapsed: collapsed,
            collapsible: true,
            width: '96%',

            defaulType: 'combobox',
            margin: '5 10 5 10',
            autoScroll: true,
            items: fieldArray,
            listeners: {
                //collapse: function (a, b) {
                //    //Ext.getCmp('no'+ a.id).expand();
                //},
                //expand: function (a, b) {
                //    Ext.getCmp('no' + a.id).collapse(true);
                //}
            }
        });
        //collapsed = true;
        panelList.push(standardPanel);


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

                var attPanel = thisWin.items.items[0].items.items[0];
                var unattPanel = thisWin.items.items[0].items.items[1];
                var attributeId = thisWin.attributeId;
                var materialId = thisWin.materialId;
                var attDic = [];
                var msg = '';

                for (var j = 0; j < thisWin.items.items[0].items.length; j++) {
                    var attPanel = thisWin.items.items[0].items.items[j];
                    for (var i = 0; i < attPanel.items.length; i++) {

                        if (attPanel.items.items[i].isRequired == 1 && (attPanel.items.items[i].value == null || attPanel.items.items[i].value == "")) {

                            Ext.Msg.alert("提示", '请填写【' + attPanel.items.items[i].fieldLabel + '】的值');
                            return false;
                        }
                        else {
                            //if (attPanel.items.items[i].id.indexOf("numberfield") >= 0 && attPanel.items.items[i].value <= 0) {
                            //    Ext.Msg.alert("提示", '标准特征项【' + attPanel.items.items[i].fieldLabel + '】的值必须大于0！');
                            //    return false;
                            //}
                            attDic.push({ AttributeId: attPanel.items.items[i].attId, AttrCode: attPanel.items.items[i].value })
                        }


                    }
                }

                if (attDic.length > 0) {
                    var CodeDesc = This.invorkBcf('GetAttrInfo', [materialId, attributeId, attDic]);
                    yes = method(row, This, CodeDesc);
                }
                else {
                    Ext.Msg.alert("提示", '请维护特征！');
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
        //layout: 'column',
        width: '100%',
        collapse: false,
        defaults: {
            margin: '5 0 0 140',//上右下左
            columnWidth: .5
        },
        items: [btnSaleConfirm, btnSaleCancel]
    })


    var classPanel = new Ext.form.Panel({
        layout: 'column',
        width: '100%',
        autoScroll: true,
        height: 460,
        items: panelList
    })

    var Salewin = new Ext.create('Ext.window.Window', {
        id: "attWin" + BillNo + Row_Id + MaterialId,
        title: '特征信息',
        resizable: false,
        //closeAction: "close",
        modal: true,
        width: 850,
        height: 550,//330
        materialId: MaterialId,//物料ID
        attributeId: AttributeId,//特征ID
        autoScroll: true,
        //layout: 'column',
        items: [classPanel, btnSalePanel],
    });
    attId++;
    Salewin.show();
    for (var i = 0; i < AttDicLst.length; i++) {
        if (AttDicLst[i].Remarks != "") {
            Ext.QuickTips.register({
                target: 'Remarks' + AttDicLst[i].AttributeItemId + '-labelEl',//给填写了备注的特征项元素注册提示信息  
                text: AttDicLst[i].Remarks
            })
        }
    }
}

//非动态特征 combox
function CreatComBox(attData, isread) {

    var attlist = [];
    for (var i = 0; i < attData.AttrValueList.length; i++) {
        var data = { AttrCode: attData.AttrValueList[i]['AttrCode'], AttrValue: attData.AttrValueList[i]['AttrValue'] };
        attlist.push(data);
    }
    var Store = Ext.create("Ext.data.Store", {
        fields: ["AttrCode", "AttrValue"],
        data: attlist
    });

    var color = "black";
    if (attData.IsRequired == 1) {
        color = "red";
    }
    var combox = new Ext.form.ComboBox({
        mode: 'local',
        forceSelection: true,
        triggerAction: 'all',
        displayField: 'AttrValue',
        fieldLabel: "<lable style='color: " + color + ";'>" + attData.AttributeItemName + "</lable>",
        isRequired: attData.IsRequired,
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
function CreatTextBox(attData, isread) {
    var color = "black";
    if (attData.IsRequired == 1) {
        color = "red";
    }
    if (attData.ValueType == 0) {
        var textbox = new Ext.form.NumberField({
            fieldLabel: "<lable style='color: " + color + ";'>" + attData.AttributeItemName + "</lable>",
            attId: attData.AttributeItemId,
            allowDecimals: true, // 允许小数点
            allowNegative: false, // 允许负数
            allowBlank: true,
            disabled: isread,
            isRequired: attData.IsRequired,
            value: attData.DefaultValue,
            maxLength: 50,
            margin: '5 10 5 10',
            columnWidth: .5,
            labelWidth: 60,
        });

    }
    else {
        var textbox = new Ext.form.TextField({
            fieldLabel: "<lable style='color: " + color + ";'>" + attData.AttributeItemName + "</lable>",
            attId: attData.AttributeItemId,
            allowBlank: true,
            value: attData.DefaultValue,
            isRequired: attData.IsRequired,
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
function FillDataRow(e, This, CodeDesc) {
    e.dataInfo.dataRow.set("ATTRIBUTECODE", CodeDesc.Code);
    e.dataInfo.dataRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
    // e.dataInfo.dataRow.set("ABNORMALDAY", CodeDesc.AbnormalDay);
    //设置异常天数
    //var masterRow = This.dataSet.getTable(0).data.items[0];
    //Ext.getCmp("ABNORMALDAY0_" + This.winId).setValue(CodeDesc.AbnormalDay);
    return true;
}
function getPurchaseOrder(e, returnData) {
    e.dataInfo.dataRow.set("DEALSQUANTITY", returnData["DealsQuantity"]); //交易数量
    e.dataInfo.dataRow.set("TAXRATE", returnData["TaxRate"]); //税率
    e.dataInfo.dataRow.set("TAXPRICE", returnData["TaxPrice"]); //含税单价
    e.dataInfo.dataRow.set("PRICE", returnData["Price"]); //单价
    e.dataInfo.dataRow.set("TAXES", returnData["Taxes"]); //税额
    e.dataInfo.dataRow.set("AMOUNT", returnData["Amount"]); //金额
    e.dataInfo.dataRow.set("TAXAMOUNT", returnData["TaxAmount"]); //含税金额
    e.dataInfo.dataRow.set("BWAMOUNT", returnData["BWAmount"]); //本币金额
    e.dataInfo.dataRow.set("BWTAXAMOUNT", returnData["BWTaxAmount"]); //本币含税金额
    e.dataInfo.dataRow.set("BWTAXES", returnData["BWTaxes"]); //本币税额
};
function fillProductOrder(This, returnData) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        This.deleteAll(1);
        var masterRow = This.dataSet.getTable(0).data.items[0];
        if (returnData !== undefined && returnData.length > 0) {
            for (var i = 0; i < returnData.length; i++) {
                var info = returnData[i];
                var newRow = This.addRow(masterRow, 1);
                newRow.set('FACTORYNO', info.FactoryNo);
                newRow.set('FIGURENO', info.FigureNo);
                newRow.set('METERNO', info.MeterNo);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('SPECIFICATION', info.SpecIfication);
                newRow.set('TEXTUREID', info.Textureid);
                newRow.set('MATERIALSPEC', info.MaterialSpec);
                newRow.set('DEALSQUANTITY', info.DealsQuantity);
                newRow.set('UNITID', info.UnitId);
                newRow.set('DEALSUNITNO', info.SktUnitNo);
                newRow.set('UNITNAME', info.UnitName);
                newRow.set('DEALSUNITNAME', info.SktUnitName);
                newRow.set('DEALSUNITID', info.SktUnitId);
                //newRow.set('FACTORYNO', info.FactoryNo);
                newRow.set("ATTRIBUTEID", info.AttributeId);
                newRow.set("ATTRIBUTENAME", info.AttributeName);
                newRow.set("ATTRIBUTECODE", info.AttributeCode);
                newRow.set("ATTRIBUTEDESC", info.AttributeDesc);
                newRow.set("TAXRATE", info.TaxRate);
                newRow.set("PRICE", info.Price);
                newRow.set("AMOUNT", info.Amount);
                newRow.set("TAXPRICE", info.TaxPrice);
                newRow.set("TAXAMOUNT", info.TaxAmount);
                newRow.set("TAXES", info.Taxes);
                newRow.set("BWAMOUNT", info.BwAmount);
                newRow.set("BWTAXAMOUNT", info.BwTaxAmount);
                newRow.set("BWTAXES", info.BwTaxes);
                newRow.set("QUANTITY", info.Quantity);
                newRow.set("ROW_ID", info.Row_Id);
                newRow.set("ROWNO", info.RowNo);
                //var ScmMoneyBcf = {};
                //ScmMoneyBcf.Price = info.Price;
                //var data = This.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangePrice]);
                //getPurchaseOrder.call(This, e, data);
            }
        }
    }
    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}