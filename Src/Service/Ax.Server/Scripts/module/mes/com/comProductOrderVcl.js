comProductOrderVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var attId = 0;
var This;
var proto = comProductOrderVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comProductOrderVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.AddRow:

            if (e.dataInfo.tableIndex == 1) {
                var btable = this.dataSet.getTable(1).data;
                var table = this.dataSet.getTable(0).data;
                var desc = btable.items[0].get("ATTRIBUTECODE");
                var tax = table.items[0].get("TAXRATE");
                e.dataInfo.dataRow.set("ATTRIBUTECODE", desc);
                e.dataInfo.dataRow.set("TAXRATE", tax);
               

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
                    This = this;
                    CreatAttForm_po(dataList, AttDicLst, e, FillDataRow_po);
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
                if (e.dataInfo.fieldName == "CONTRACTNO") {
                    var headTableRow = this.dataSet.getTable(0).data.items[0];
                    var returnstr = this.invorkBcf("SelectContractNo", [e.dataInfo.value]);
                    if (returnstr.length > 0) {                      
                        Ext.Msg.alert("提示", returnstr);
                        headTableRow.set("CONTRACTNO", "");
                    }
                    this.forms[0].loadRecord(headTable.data.items[0]);

                }
                if (e.dataInfo.fieldName == 'PROJECTID') {
                    var headTableRow = this.dataSet.getTable(0).data.items[0];
                    var bodyTable = this.dataSet.getTable(1);
                    var bodyTableOne = this.dataSet.getTable(2);
                    var bodySpareTable = this.dataSet.getTable(3);
                    var returnData = this.invorkBcf("GetProjectData", [headTableRow.data["PROJECTID"]]);
                    var returnSpareData = this.invorkBcf("GetSpareData", [headTableRow.data["PROJECTID"]]);
                    if (e.dataInfo.value == null) {
                        bodyTable.removeAll();
                        bodyTableOne.removeAll();
                        bodySpareTable.removeAll();
                    }
                    else {
                        if (returnData.length == 0) {
                            Ext.Msg.alert("提示", "项目单物料为空！");
                            return;
                        }
                        else {
                            fillProductOrder(this, returnData);
                            fillSpareOrder(this, returnSpareData);
                        }
                    }
                    var sum = 0;
                    var sun = 0;
                    for (var i = 0; i < bodyTable.data.length; i++) {
                        sun += bodyTable.data.items[i].get("TAXAMOUNT");
                        //console.info(sun);
                        sum += bodyTable.data.items[i].get("CPRICE") * bodyTable.data.items[i].get("DEALSQUANTITY");
                        //console.info(sum);
                    }
                    for (var i = 0; i < bodySpareTable.data.length; i++) {
                        sun += bodySpareTable.data.items[i].get("SALESPRICE");
                        //console.info(sun);
                        sum += bodySpareTable.data.items[i].get("PRICE") * bodyTable.data.items[i].get("QUANTITY");
                        //console.info(sum);
                    }
                    var headTable = this.dataSet.getTable(0);
                    headTable.data.items[0].set("OFFERAMOUNT", sun);
                    headTable.data.items[0].set("AMOUNT", sum);
                    this.forms[0].loadRecord(headTable.data.items[0]);
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
                        getPurchaseOrder_po(e, data);
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
                        getPurchaseOrder_po(e, data);
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
                        getPurchaseOrder_po(e, data);
                    }
                }
               
                if (e.dataInfo.fieldName == "TAXPRICE") {
                    if (e.dataInfo.value >= 0) {
                        //金额变更引起的其它字段的变更
                        ScmMoneyBcf.TaxPrice = e.dataInfo.value;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxPrice]);
                        getPurchaseOrder_po(e, data);
                    }
                }

                if (e.dataInfo.fieldName == "AMOUNT") {
                    if (e.dataInfo.value >= 0 && parseFloat(e.dataInfo.dataRow.data["DEALSQUANTITY"]) > 0) {
                        //金额变更引起的其它字段的变更
                        ScmMoneyBcf.Amount = e.dataInfo.value;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeAmount]);
                        getPurchaseOrder_po(e, data);
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
                        getPurchaseOrder_po(e, data);
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
                        getPurchaseOrder_po(e, data);
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
                        getPurchaseOrder_po(e, data);
                    }
                }
                if (e.dataInfo.fieldName == "BWAMOUNT") {
                    if (e.dataInfo.value >= 0) {
                        //本位币金额变更引起的其它字段的变更
                        ScmMoneyBcf.BWAmount = e.dataInfo.value;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeBWAmount]);
                        getPurchaseOrder_po(e, data);
                    }
                }
                if (e.dataInfo.fieldName == "BWTAXAMOUNT") {
                    if (e.dataInfo.value >= 0) {
                        //本位币含税金额变更引起的其它字段的变更
                        ScmMoneyBcf.BWTaxAmount = e.dataInfo.value;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeBWTaxAmount]);
                        getPurchaseOrder_po(e, data);

                    }
                }
                if (e.dataInfo.fieldName == "CPRICE") {
                    var bodyTable = this.dataSet.getTable(1);
                    var bodyTableDetail = this.dataSet.getTable(3);
                    var sum = e.dataInfo.value * e.dataInfo.dataRow.get("DEALSQUANTITY");
                    var bodyTableOne = this.dataSet.getTable(3);
                    for (var i = 0; i < bodyTable.data.length; i++) {
                        if (e.dataInfo.dataRow.get("ROW_ID") != bodyTable.data.items[i].get("ROW_ID")) {
                            sum += (bodyTable.data.items[i].get("CPRICE")* bodyTable.data.items[i].get("DEALSQUANTITY"));
                        }
                    }

                    for (var i = 0; i < bodyTableOne.data.length; i++) {

                        sum += (bodyTableOne.data.items[i].get("PRICE") * bodyTableOne.data.items[i].get("QUANTITY"));

                    }
                    var headTable = this.dataSet.getTable(0);
                    headTable.data.items[0].set("AMOUNT", sum);
                    this.forms[0].loadRecord(headTable.data.items[0]);
                }
                if (e.dataInfo.fieldName == "TAXPRICE") {
                    var bodyTable = this.dataSet.getTable(1);
                    var bodyTableOne = this.dataSet.getTable(3);
                    var sun = e.dataInfo.value * e.dataInfo.dataRow.get("DEALSQUANTITY");
                    for (var i = 0; i < bodyTable.data.length; i++) {
                        if (e.dataInfo.dataRow.get("ROW_ID") != bodyTable.data.items[i].get("ROW_ID")) {
                            sun += (bodyTable.data.items[i].get("TAXPRICE") * bodyTable.data.items[i].get("DEALSQUANTITY"));
                        }
                    }
                    
                    for (var i = 0; i < bodyTableOne.data.length; i++) {
                        
                        sun += (bodyTableOne.data.items[i].get("SALESPRICE") * bodyTableOne.data.items[i].get("QUANTITY"));
                        
                   }
                    var headTable = this.dataSet.getTable(0);
                    headTable.data.items[0].set("OFFERAMOUNT", sun);
                    this.forms[0].loadRecord(headTable.data.items[0]);
                }
                if (e.dataInfo.fieldName == "FACTORYNO") {
                    var bodyTable = this.dataSet.getTable(1);             
                    var sum = e.dataInfo.value;
                    var rowNo = (Number)(e.dataInfo.dataRow.get("ROWNO"));
                    var factoryNo = this.dataSet.getTable(2);
                    if (sum.indexOf("-") != -1 && (sum.split("-")[0].toString().length!=7 || sum.split("-")[1].toString().length!=3))
                    {
                        Ext.Msg.alert("提示", "填写的出厂编号格式不正确，请重新填写！");
                        var children = this.dataSet.getChildren(1, e.dataInfo.dataRow, 2);
                        if (children != undefined) {
                            if (children.length > 0) {
                                for (var j = 0; j < children.length; j++) {
                                    //this.deleteRow(2, children[i]);
                                    factoryNo.remove(children[j], true);
                                }
                            }
                        }
                        e.dataInfo.dataRow.set("FACTORYNO", "")
                        e.dataInfo.dataRow.set("FACTORYNODETAIL", 0)
                    }
                    else if (sum.indexOf("-") == -1 && sum.toString().length != 7) {
                        Ext.Msg.alert("提示", "填写的出厂编号格式不正确，请重新填写！");
                        var children = this.dataSet.getChildren(1, e.dataInfo.dataRow, 2);
                        if (children != undefined) {
                            if (children.length > 0) {
                                for (var j = 0; j < children.length; j++) {
                                    //this.deleteRow(2, children[i]);
                                    factoryNo.remove(children[j], true);
                                }
                            }
                        }
                        e.dataInfo.dataRow.set("FACTORYNO", "")
                        e.dataInfo.dataRow.set("FACTORYNODETAIL", 0)
                    }
                    else if (sum.indexOf("-") != -1 && (Number)(sum.split("-")[1]) - (Number)(sum.split('-')[0].substr(4, 3)) > 0) {
                        e.dataInfo.dataRow.set("FACTORYNODETAIL", 1)
                        var children = this.dataSet.getChildren(1, e.dataInfo.dataRow, 2);
                        if (children != undefined) {
                            if (children.length > 0) {
                                for (var j = 0; j < children.length; j++) {
                                    //this.deleteRow(2, children[i]);
                                    factoryNo.remove(children[j], true);
                                }
                            }
                        }
                        fillFactoryNoDetail(this, e.dataInfo.dataRow.data["ROW_ID"], sum, e.dataInfo.dataRow);
                        //sum += 1;

                        //fillFactoryNoDetail(this, bodyTable.data.items[i].get("ROW_ID"), sum);
                        // }

                        //this.forms[0].loadRecord(headTable.data.items[0]);
                    }
                    else if (sum.indexOf("-") == -1 && rowNo == 1)
                    {
                        this.deleteAll(2);
                            for (var i = 0; i < bodyTable.data.length; i++) {
                                bodyTable.data.items[i].set("FACTORYNO", sum)
                                bodyTable.data.items[i].set("FACTORYNODETAIL", 1)
                                fillFactoryNoDetailOne(this, bodyTable.data.items[i].get("ROW_ID"), sum, bodyTable.data.items[i]);
                                sum = (Number)(sum) + 1;
                            }                       
                    }
                
                        
                    else if (sum.indexOf("*") != -1) {
                        sun = sum.split('*')[1] - sum.split('*')[0] + e.dataInfo.dataRow.get("ROWNO");
                        var facNo = Number(e.dataInfo.value.split('*')[0])
                        for (var i = e.dataInfo.dataRow.get("ROWNO") - 1 ; i < sun; i++) {
                            bodyTable.data.items[i].set("FACTORYNO", facNo);
                            //if (sum.indexOf('-') != -1 && (Number)(sum.split('-')[1]) - (Number)(sum.split('-')[0].substr(4, 3)) > 0) {

                            var children = this.dataSet.getChildren(1, e.dataInfo.dataRow, 2);
                            if (children != undefined) {
                                if (children.length > 0) {
                                    for (var j = 0; j < children.length; j++) {
                                        //this.deleteRow(2, children[i]);
                                        factoryNo.remove(children[j], true);
                                    }
                                }
                            }
                            fillFactoryNoDetailOne(this, bodyTable.data.items[i].get("ROW_ID"), facNo, bodyTable.data.items[i]);
                            bodyTable.data.items[i].set("FACTORYNODETAIL", 1)
                            facNo += 1;

                            //fillFactoryNoDetail(this, bodyTable.data.items[i].get("ROW_ID"), sum);                               
                        }
                    }
                    else {
                        var children = this.dataSet.getChildren(1, e.dataInfo.dataRow, 2);
                        if (children != undefined) {
                            if (children.length > 0) {
                                for (var j = 0; j < children.length; j++) {
                                    //this.deleteRow(2, children[i]);
                                    factoryNo.remove(children[j], true);
                                }
                            }
                        }
                        fillFactoryNoDetail(this, e.dataInfo.dataRow.data["ROW_ID"], sum, e.dataInfo.dataRow);
                        e.dataInfo.dataRow.set("FACTORYNODETAIL", 1)
                    }
                    
                   this.forms[0].loadRecord(headTable.data.items[0]);
                }
            }
            if (e.dataInfo.tableIndex == 3) {
                if (e.dataInfo.fieldName == "PRICE") {
                    var bodyTable = this.dataSet.getTable(1);
                    var bodyTableOne = this.dataSet.getTable(3);
                    var sum = e.dataInfo.value * e.dataInfo.dataRow.get("QUANTITY");
                    for (var i = 0; i < bodyTable.data.length; i++) {
                        sum += (bodyTable.data.items[i].get("CPRICE") * bodyTable.data.items[i].get("DEALSQUANTITY"));
                    }

                    for (var i = 0; i < bodyTableOne.data.length; i++) {
                        if (e.dataInfo.dataRow.get("ROW_ID") != bodyTableOne.data.items[i].get("ROW_ID")) {
                            sum += (bodyTableOne.data.items[i].get("PRICE") * bodyTableOne.data.items[i].get("QUANTITY"));
                        }
                    }
                    var headTable = this.dataSet.getTable(0);
                    headTable.data.items[0].set("AMOUNT", sum);
                    this.forms[0].loadRecord(headTable.data.items[0]);
                }
                if (e.dataInfo.fieldName == "SALESPRICE") {
                    var bodyTable = this.dataSet.getTable(1);
                    var bodyTableOne = this.dataSet.getTable(3);
                    var sun = e.dataInfo.value * e.dataInfo.dataRow.get("QUANTITY");
                    for (var i = 0; i < bodyTable.data.length; i++) {
                        sun += bodyTable.data.items[i].get("TAXAMOUNT");

                    }
                    for (var i = 0; i < bodyTableOne.data.length; i++) {
                        if (e.dataInfo.dataRow.get("ROW_ID") != bodyTableOne.data.items[i].get("ROW_ID")) {
                            sun += (bodyTableOne.data.items[i].get("SALESPRICE")* bodyTableOne.data.items[i].get("QUANTITY"));
                        }
                    }
                    var headTable = this.dataSet.getTable(0);
                    headTable.data.items[0].set("OFFERAMOUNT", sun);
                    this.forms[0].loadRecord(headTable.data.items[0]);
                }
                if (e.dataInfo.fieldName == "QUANTITY") {
                    var bodyTable = this.dataSet.getTable(1);
                    var bodyTableOne = this.dataSet.getTable(3);
                    var sun = e.dataInfo.value * e.dataInfo.dataRow.get("SALESPRICE");
                    var sum = e.dataInfo.value * e.dataInfo.dataRow.get("PRICE");
                    for (var i = 0; i < bodyTable.data.length; i++) {
                        sun += bodyTable.data.items[i].get("TAXAMOUNT");
                        sum += (bodyTable.data.items[i].get("CPRICE") * bodyTable.data.items[i].get("DEALSQUANTITY"));

                    }
                    for (var i = 0; i < bodyTableOne.data.length; i++) {
                        if (e.dataInfo.dataRow.get("ROW_ID") != bodyTableOne.data.items[i].get("ROW_ID")) {
                            sun += (bodyTableOne.data.items[i].get("SALESPRICE") * bodyTableOne.data.items[i].get("QUANTITY"));
                        }
                        if (e.dataInfo.dataRow.get("ROW_ID") != bodyTableOne.data.items[i].get("ROW_ID")) {
                            sum += (bodyTableOne.data.items[i].get("PRICE") * bodyTableOne.data.items[i].get("QUANTITY"));
                        }
                    }
                    var headTable = this.dataSet.getTable(0);
                    headTable.data.items[0].set("OFFERAMOUNT", sun);
                    headTable.data.items[0].set("AMOUNT", sum);
                    this.forms[0].loadRecord(headTable.data.items[0]);
                }
            }
            break;
        case LibEventTypeEnum.ButtonClick:

            if (e.dataInfo.fieldName == "UpLoadDeatail") {
                This = this;
                if (this.isEdit) {
                    var panel = Ext.create('Ext.form.Panel', {
                        bodyPadding: 10,
                        frame: true,
                        renderTo: Ext.getBody(),
                        items: [{
                            xtype: 'filefield',
                            name: 'txtFile',
                            fieldLabel: '文件',
                            labelWidth: 50,
                            msgTarget: 'side',
                            allowBlank: false,
                            anchor: '100%',
                            buttonText: '选择...'
                        }],

                        buttons: [{
                            text: '导入',
                            handler: function () {
                                var form = this.up('form').getForm();
                                var judge = form.monitor.items.items[0].value;
                                if (judge.indexOf('\\') == -1) {
                                    path = judge;
                                }
                                else {
                                    path = form.monitor.items.items[0].value.split('\\')[2];
                                }
                                if (form.isValid()) {
                                    form.submit({
                                        url: '/fileTranSvc/upLoadFile',
                                        waitMsg: '正在导入文件...',
                                        success: function (fp, o) {
                                            var pathFill = This.invorkBcf('GetDetail', [path]);
                                            if (pathFill == null) {
                                                Ext.Msg.alert('错误', pathFill);
                                                return;
                                            }
                                            else {
                                                //取得最终结果并且填充进子表当中
                                                var detail = This.invorkBcf('Results', [pathFill]);
                                                ProductFeature(This, detail);
                                                
                                                win.close();
                                                This.forms[0].loadRecord(This.dataSet.getTable(0).data.items[0]);
                                            }

                                        },
                                        failure: function (fp, o) {
                                            Ext.Msg.alert('错误', '文件 "' + o.result.FileName + '" 导入失败.');
                                        }
                                    });
                                }
                            }
                        }]
                    });
                    win = Ext.create('Ext.window.Window', {
                        autoScroll: true,
                        width: 400,
                        height: 300,
                        layout: 'fit',
                        vcl: vcl,
                        constrainHeader: true,
                        minimizable: true,
                        maximizable: true,
                        items: [panel]
                    });
                    win.show();
                }
                else {
                    Ext.Msg.alert('错误', '只有在编辑状态下才能导入.');
                }
                //var detail = this.invorkBcf('Results');
                //AddFeature(this,detail);
            }
            else if (e.dataInfo.fieldName == "OutLoadDeatail") {
                var childTabale = this.dataSet.getTable(1).data;
                var list = [];
                var attid;
                for (var i = 0; i < childTabale.items.length; i++) {
                    list.push({ 'Feature': childTabale.items[i].data['ATTRIBUTEID'], 'FeatureName': childTabale.items[i].data['ATTRIBUTENAME'], 'Code': childTabale.items[i].data['ATTRIBUTECODE'] });
                }
                var fileName = this.invorkBcf('ParsingChild', [list]);
                if (fileName == null) {
                    Ext.Msg.alert('提示', '导出内容为空');
                }
                else {
                    if (fileName && fileName !== '') {
                        DesktopApp.IgnoreSkip = true;
                        try {
                            window.location.href = '/TempData/ExportData/' + fileName;
                        } finally {
                            DesktopApp.IgnoreSkip = false
                        }
                    }
                }
            }
               else if (e.dataInfo.fieldName == "DetailData") {
                //判断当前界面状态
                if (!this.isEdit) {
                    //获取界面数据
                    var contractNoName = this.dataSet.getTable(0).data.items[0].data['CONTRACTNO']; //获取合同号
                    var billNoName = this.dataSet.getTable(0).data.items[0].data['BILLNO']; //获取往来对象名称
                    //打开目标界面(progid,名称,[参数值])
                    Ax.utils.LibVclSystemUtils.openDataFunc("com.ProductOrderDetailDataFunc", "投产单生产计划", [this, contractNoName, billNoName]);
                }
                else {
                    Ext.Msg.alert("系统提示", "非编辑状态下才能使用数据加载按钮！");
                }
            }
            else if (e.dataInfo.fieldName == "ExprotExcel") {
                if (!this.isEdit) {
                    var headTableRow = this.dataSet.getTable(0).data.items[0];
                    var billNo = headTableRow.data["BILLNO"];
                    window.open("ExprotComProduceOrder.aspx?bill=" + billNo);
                }
                else { Ext.Msg.alert("系统提示", "编辑状态不能导出Excel！"); }
            }
            else if (e.dataInfo.fieldName == "CreatContract") {
                if (!this.isEdit) {
                    //生成合同评审单
                    var masterRow = this.dataSet.getTable(0).data.items[0];//表头
                    var billNo = masterRow.data["BILLNO"];
                    var bodyTableOne = this.dataSet.getTable(1);
                    var bodyTableTwo = this.dataSet.getTable(2);
                    if (bodyTableOne.data.length == 0 && bodyTableTwo.data.length == 0) {
                        Ext.Msg.alert("提示", "无数据可以生成！！");
                    }
                    else {

                        this.invorkBcf('CreatContract', [billNo]);
                        //if (!Ext.isEmpty(scheduleBillNo)) {
                        //    var obj = [];
                        //    obj.push(masterRow.data["BILLNO"]);
                        //    this.browseTo(obj);
                        //    masterRow.set("SCHEDULEBILLNO", scheduleBillNo);
                        //    Ext.Msg.alert("系统提示", "生成合同评审单号为：【" + scheduleBillNo + "】");
                        //    bodyGrid.getView().getSelectionModel().deselectAll();

                        //}
                        //else {
                        //    Ext.Msg.alert("系统提示", "生成报价单失败！");
                        //}
                    }
                }
                else
                    Ext.Msg.alert("系统提示", "编辑状态不能生成单据！");
            }


            break;
    }
}

//最新特征窗体
function CreatAttForm_po(dataList, AttDicLst, row, method) {

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

                fieldArray.push(CreatTextBox_po(AttDicLst[i].List[j], isRead));
            }
            else {
                fieldArray.push(CreatComBox_po(AttDicLst[i].List[j], isRead));
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
            if (This.isEdit) {

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
                    This.forms[0].loadRecord(This.dataSet.getTable(0).data.items[0]);
                }
                else {
                    Ext.Msg.alert("提示", '请维护特征！');
                }
            }
            if (yes) {
                thisWin.close();
                This.dataSet.getTable(0).data.items[0].set("ANDQUANTITY", quantity);
                This.forms[0].loadRecord(This.dataSet.getTable(0).data.items[0]);
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
        width: 900,
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
function CreatComBox_po(attData, isread) {

    var attlist = [];
    for (var i = 0; i < attData.AttrValueList.length; i++) {
        var data = { AttrCode: attData.AttrValueList[i]['AttrCode'], AttrValue: attData.AttrValueList[i]['AttrValue'] };
        attlist.push(data);
    };
    attlist.push({ AttrCode: "AddNew", AttrValue: "添加新选项" });
    //attlist.splice(attlist.length - 1, 0, { AttrCode: "AddNew1", AttrValue: "xinde" }); // 
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
        //editablle: true,
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
        margin: '5 10 5 10',
        columnWidth: .5,
        labelWidth: 60,
        listeners: {
            blur: function (f) {
                if (f.value == "AddNew") {
                    f.value = "";
                    if (!This.isEdit) {
                        Ext.Msg.alert("系统提示", "编辑状态才能新增特征项！");
                        return;
                    }
                    AttItemAddNewForm(f, attlist);
                }
            },

            render: function (field, p) {
                if (attData.Remarks.length > 0) {
                    Ext.QuickTips.init();
                    Ext.QuickTips.register({
                        target: field.el,
                        text: attData.Remarks
                    })
                }
            }
        }
    });
    return combox;
}
//动态特征 NumberField
function CreatTextBox_po(attData, isread) {
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
            listeners: {
                render: function (field, p) {
                    if (attData.Remarks.length > 0) {
                        Ext.QuickTips.init();
                        Ext.QuickTips.register({
                            target: field.el,
                            text: attData.Remarks
                        })
                    }
                }
            }
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
            listeners: {
                render: function (field, p) {
                    if (attData.Remarks.length > 0) {
                        Ext.QuickTips.init();
                        Ext.QuickTips.register({
                            target: field.el,
                            text: attData.Remarks
                        })
                    }
                }
            }
        });
    }

    return textbox;
}

//填充当前行特征信息
function FillDataRow_po(e, This, CodeDesc) {
    var quantity = 0;
    for (var i = 0; i < This.dataSet.getTable(1).data.length; i++) {
        quantity += (Number)(This.dataSet.getTable(1).data.items[i].get("DEALSQUANTITY"));
    }
    quantity = quantity - (Number)(e.dataInfo.dataRow.get("DEALSQUANTITY")) + (Number)(CodeDesc.Quantity);
    e.dataInfo.dataRow.set("ATTRIBUTECODE", CodeDesc.Code);
    e.dataInfo.dataRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
    e.dataInfo.dataRow.set("METERNO", CodeDesc.Meter);
    e.dataInfo.dataRow.set("PRODUCTSPEC", CodeDesc.MType);
    e.dataInfo.dataRow.set("PRODUCTNAME", CodeDesc.Valve);
    e.dataInfo.dataRow.set("DEALSQUANTITY", CodeDesc.Quantity);
    This.dataSet.getTable(0).data.items[0].set("ANDQUANTITY", quantity);
    return true;
}
function getPurchaseOrder_po(e, returnData) {
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
function fillFactoryNoDetail(This,row_id,sum,table)
{
    
    //Ext.suspendLayouts();
    //var formStore = This.dataSet.getTable(2);
    //formStore.suspendEvents();
    try {
        if (sum.indexOf("-") != -1) {
            var index = sum.split("-")[1] - sum.split("-")[0].substr(4,3)+1;
            var sun = (Number)(sum.split("-")[0]);
            for (var j = 0; j < index; j++) {
                var newRow = This.addRow(table, 2);
                newRow.set('PARENTROWID', row_id);
                newRow.set('ROW_ID', j + 1);
                newRow.set('ROWNO', j + 1);
                newRow.set('FACTORYNO', sun);
                sun += 1;
            }
        }
        else {
                for (var j = 0; j < 1; j++) {
                    var newRow = This.addRow(table, 2);
                    newRow.set('PARENTROWID', row_id);
                    newRow.set('ROW_ID', j+1);
                    newRow.set('ROWNO', j + 1);
                    newRow.set('FACTORYNO', sum);
                }            
        }
    }
    finally {
        //formStore.resumeEvents();
        //if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
        //    formStore.ownGrid.reconfigure(formStore);
        //Ext.resumeLayouts(true);
    }
}
function fillFactoryNoDetailOne(This, row_id, sum, table) {

    //Ext.suspendLayouts();
    //var formStore = This.dataSet.getTable(2);
    //formStore.suspendEvents();
    try {
            for (var j = 0; j < 1; j++) {
                var newRow = This.addRow(table, 2);
                newRow.set('PARENTROWID', row_id);
                newRow.set('ROW_ID', j + 1);
                newRow.set('ROWNO', j + 1);
                newRow.set('FACTORYNO', sum);
            }
        
    }
    finally {
        //formStore.resumeEvents();
        //if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
        //    formStore.ownGrid.reconfigure(formStore);
        //Ext.resumeLayouts(true);
    }
}
function fillProductOrder(This, returnData) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    var index = 0;
    index = This.invorkBcf('MaxFactoryNo', []);
    var indexOne = 0;
    formStore.suspendEvents();
    try {
        This.deleteAll(1);
        This.deleteAll(2);
        var masterRow = This.dataSet.getTable(0).data.items[0];
        if (returnData !== undefined && returnData.length > 0) {
            for (var i = 0; i < returnData.length; i++) {
                var info = returnData[i];
                var newRow = This.addRow(masterRow, 1);
                newRow.set('ROW_ID', info.RowId);
                newRow.set('ROWNO', info.RowNo);
                if (info.Quantity > 1) {
                    newRow.set('FACTORYNO', index + "-" + (index + info.Quantity - 1).toString().substr(4, 3));
                }
                else
                {
                    newRow.set('FACTORYNO', index);
                }
                newRow.set("FACTORYNODETAIL", 1);
                newRow.set('METERNO', info.MeterNo);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('FIGURENO', info.FigureNo);
                newRow.set('SPECIFICATION', info.SpecIfication);
                newRow.set('TEXTUREID', info.Textureid);
                newRow.set('MATERIALSPEC', info.MaterialSpec);
                newRow.set('DEALSQUANTITY', info.Quantity);
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
                newRow.set("TAXRATE", masterRow.get("TAXRATE"));
                newRow.set("PRICE", info.Price / (1 + masterRow.get("TAXRATE")));
                newRow.set("CPRICE", info.Amount);
                newRow.set("AMOUNT", info.Price / (1 + masterRow.get("TAXRATE")) * info.Quantity);
                newRow.set("TAXPRICE", info.Price);
                newRow.set("TAXAMOUNT", info.Price * info.Quantity);
                newRow.set("TAXES", info.Price / (1 + masterRow.get("TAXRATE")) * masterRow.get("TAXRATE") * info.Quantity);
                newRow.set("BWAMOUNT", info.Price / (1 + masterRow.get("TAXRATE")) * masterRow.get("STANDARDCOILRATE") * info.Quantity);
                newRow.set("BWTAXAMOUNT", (info.Price / (1 + masterRow.get("TAXRATE")) * masterRow.get("STANDARDCOILRATE") + info.Price / (1 + masterRow.get("TAXRATE")) * masterRow.get("STANDARDCOILRATE") * masterRow.get("TAXRATE")) * info.Quantity);
                newRow.set("BWTAXES", info.Price / (1 + masterRow.get("TAXRATE")) * masterRow.get("STANDARDCOILRATE") * masterRow.get("TAXRATE") * info.Quantity);
                var unitData = This.invorkBcf("GetData", [info.MaterialId, info.SktUnitId, info.SktUnitNo, 0, info.Quantity, info.UnitId, 0]);
                newRow.set("QUANTITY", unitData.Quantity);
                indexOne = index;
                for (var j = 0; j < info.Quantity; j++) {
                    var newRow = This.addRow(masterRow, 2);
                    newRow.set('PARENTROWID', info.RowId);
                    newRow.set('FACTORYNO', indexOne);
                    indexOne += 1;
                }
                index += info.Quantity;
                //var ScmMoneyBcf = {};
                //ScmMoneyBcf.Price = info.Price;
                //var data = This.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangePrice]);
                //getPurchaseOrder_po.call(This, e, data);
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
function fillSpareOrder(This, returnSpareData) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(3);
    var index = 0;
    index = This.invorkBcf('MaxFactoryNo', []);
    var indexOne = 0;
    formStore.suspendEvents();
    try {
        This.deleteAll(3);
        var masterRow = This.dataSet.getTable(0).data.items[0];
        if (returnSpareData !== undefined && returnSpareData.length > 0) {
            for (var i = 0; i < returnSpareData.length; i++) {
                var info = returnSpareData[i];
                var newRow = This.addRow(masterRow, 3);
                newRow.set('ROW_ID', i + 1);
                newRow.set('ROWNO', i + 1);
                newRow.set('ATTRIBUTEITEMID', info.SpareId);
                newRow.set('ATTRIBUTEITEMNAME', info.SpareName);
                newRow.set('SPARESPEC', info.SpareSpec);
                newRow.set('QUANTITY', info.Quantity);
                newRow.set('PRICE', info.Price);
                newRow.set('SALESPRICE', info.SalesPrice);
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



//添加子表的每行特征表示和特征描述
function ProductFeature(This, detail) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        This.deleteAll(1);
        var quantity = 0;
        var bool = true;
        var masterRow = This.dataSet.getTable(0).data.items[0];
        if (detail !== undefined && detail.length > 0) {
            //for (var i = 0; i < detail.length; i++) {
            //    if (detail[i].Quantity.indexOf("台") != -1) {
            //        Ext.Msg.alert("系统提示", "导入的投产单规格中的'数量（台）'字段不是数值，请检查投产单的规格！！");
            //        bool = false;
            //        return;
            //    }
            //}
            //if (bool) {
                for (var i = 0; i < detail.length; i++) {
                    var newRow = This.addRow(masterRow, 1);

                    newRow.set('ATTRIBUTECODE', detail[i].Code);
                    newRow.set('ATTRIBUTEDESC', detail[i].Desc);
                    newRow.set('ATTRIBUTEID', detail[i].Feature);
                    newRow.set('ATTRIBUTENAME', detail[i].FeatureName);
                    newRow.set('REMARK', "");
                    newRow.set('METERNO', detail[i].Meter);
                    newRow.set('PRODUCTSPEC', detail[i].MType);
                    newRow.set('TAXRATE', '0.17');
                    newRow.set('DEALSQUANTITY', detail[i].Quantity);
                    quantity += (Number)(detail[i].Quantity)
                    masterRow.set("ANDQUANTITY", quantity);
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


//添加新特征值
function AttItemAddNewForm(f, attlist) {
    //确认按钮
    var newAttId = This.invorkBcf('SelectAttId', [f.attId]);
    if (newAttId.indexOf("报错") > -1) {
        Ext.Msg.alert("系统提示", newAttId)
        return;
    }
    var btnSaleConfirm = new Ext.Button({
        width: 150,
        height: 35,
        text: "确定",
        type: 'submit',
        handler: function () {
            var yes = true;
            var thisWin = Ext.getCmp("attAddNewWin");
            var Panel = thisWin.items.items[0];
            var Id = Panel.items.items[0].value;
            var Name = Panel.items.items[1].value;
            if (Name == '') {
                Ext.Msg.alert("系统提示", "特征值不能为空！")
            }

            var res = This.invorkBcf('AttItemAddNew', [f.attId, Id, Name]);
            if (res == '成功') {
                var list = f.store.data;
                attlist.splice(attlist.length - 1, 0, { AttrCode: Id, AttrValue: Name }); // 
                f.store.removeAll();//先清空数据
                f.store.loadData(attlist);
                f.setValue(Id);
                thisWin.close();
            }
            else {
                Ext.Msg.alert("系统提示", res)
            }

        }
    })
    //取消按钮
    var btnSaleCancel = new Ext.Button({
        width: 150,
        height: 35,
        text: "取消",
        type: 'submit',
        handler: function () {
            f.setValue("");
            Ext.getCmp("attAddNewWin").close();
        }
    })
    //按钮Panle
    var btnSalePanel = new Ext.form.Panel({
        //layout: 'column',
        width: '100%',
        collapse: false,
        defaults: {
            margin: '0 40 0 80',//上右下左
            columnWidth: .5
        },
        items: [btnSaleConfirm, btnSaleCancel]
    })
    var classPanel = new Ext.form.Panel({
        layout: 'column',
        width: '100%',
        height: 50,
        items: [
               new Ext.form.TextField({
                   fieldLabel: "特征编码",
                   value: newAttId,
                   disabled: true,
                   maxLength: 50,
                   margin: '5 10 5 10',
                   columnWidth: .5,
                   labelWidth: 80,

               }),
                new Ext.form.TextField({
                    fieldLabel: "特征值",
                    value: "",
                    maxLength: 50,
                    margin: '5 10 5 10',
                    columnWidth: .5,
                    labelWidth: 80,
                }),

        ]
    })
    var Salewin = new Ext.create('Ext.window.Window', {
        id: "attAddNewWin",
        title: '特征项值新增',
        resizable: false,
        modal: true,
        width: 600,
        height: 140,
        autoScroll: true,
        defaults: {
            margin: '0 0 0 0',//上右下左
        },
        items: [classPanel, btnSalePanel],
    });

    Salewin.show();
}
