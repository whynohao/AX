purChaseOrderVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var attId = 0;
var proto = purChaseOrderVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = purChaseOrderVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
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
                    ALLCount.call(this, e); //统计
                }
                if (e.dataInfo.fieldName == "TAXRATE") {
                    var items = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < items.length; i++) {
                        var ScmMoneyBcf = {}; //金额、数量换算
                        items[i].set("TAXRATE", this.dataSet.getTable(0).data.items[0].get("TAXRATE"));
                        ScmMoneyBcf.Amount = this.dataSet.getTable(1).data.items[i].get("AMOUNT"); //金额
                        ScmMoneyBcf.DealsQuantity = this.dataSet.getTable(1).data.items[i].get("DEALSQUANTITY");//交易数量
                        ScmMoneyBcf.StandardcoilRate = this.dataSet.getTable(0).data.items[0].get("STANDARDCOILRATE"); //汇率
                        ScmMoneyBcf.Price = this.dataSet.getTable(1).data.items[i].get("PRICE"); //单价   
                        //税率变更引起的其它字段的变更
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
                    ALLCount.call(this, e); //统计
                }

                if (e.dataInfo.fieldName == "CONTACTSOBJECTID") {
                    if (this.dataSet.getTable(0).data.items[0].get("TAXRATE") > 0) {
                        var items = this.dataSet.getTable(1).data.items;
                        for (var i = 0; i < items.length; i++) {
                            var ScmMoneyBcf = {}; //金额、数量换算
                            items[i].set("TAXRATE", this.dataSet.getTable(0).data.items[0].get("TAXRATE"));
                            ScmMoneyBcf.Amount = this.dataSet.getTable(1).data.items[i].get("AMOUNT"); //金额
                            ScmMoneyBcf.DealsQuantity = this.dataSet.getTable(1).data.items[i].get("DEALSQUANTITY");//交易数量
                            ScmMoneyBcf.TaxRate = this.dataSet.getTable(0).data.items[0].get("TAXRATE"); //税率
                            ScmMoneyBcf.StandardcoilRate = this.dataSet.getTable(0).data.items[0].get("STANDARDCOILRATE"); //汇率
                            ScmMoneyBcf.Price = this.dataSet.getTable(1).data.items[i].get("PRICE"); //单价     
                            //交易数量变更引起的其它字段的变更                           
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
                    ALLCount.call(this, e); //统计
                }
                if (e.dataInfo.fieldName == "INVOICETYPEID") {
                    if (this.dataSet.getTable(0).data.items[0].get("TAXRATE") > 0) {
                        var items = this.dataSet.getTable(1).data.items;
                        for (var i = 0; i < items.length; i++) {
                            var ScmMoneyBcf = {}; //金额、数量换算
                            items[i].set("TAXRATE", this.dataSet.getTable(0).data.items[0].get("TAXRATE"));
                            ScmMoneyBcf.Amount = this.dataSet.getTable(1).data.items[i].get("AMOUNT"); //金额
                            ScmMoneyBcf.DealsQuantity = this.dataSet.getTable(1).data.items[i].get("DEALSQUANTITY");//交易数量
                            ScmMoneyBcf.TaxRate = this.dataSet.getTable(0).data.items[0].get("TAXRATE"); //税率
                            ScmMoneyBcf.StandardcoilRate = this.dataSet.getTable(0).data.items[0].get("STANDARDCOILRATE"); //汇率
                            ScmMoneyBcf.Price = this.dataSet.getTable(1).data.items[i].get("PRICE"); //单价   
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
                    ALLCount.call(this, e); //统计
                }
                if (e.dataInfo.fieldName == "FROMTYPE") {
                    masterRow.set("RELATIONCODE", "");//来源单号设空
                    //更新表头数据
                    this.forms[0].updateRecord(masterRow);
                    this.forms[0].loadRecord(masterRow);
                }

                if (e.dataInfo.fieldName == "PREPAREDATE") {
                    var items = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < items.length; i++) {
                        items[i].set("PREPAREDATE", this.dataSet.getTable(0).data.items[0].get("PREPAREDATE"));
                    }
                }

                if (e.dataInfo.fieldName == "BILLDATE") {
                    var lastdate = parseInt(changeTodateTwoLastYear(e.dataInfo.value));
                    masterRow.set("YXQEND", lastdate);//有效期至
                    Ext.getCmp('YXQEND0_' + this.winId).setValue(lastdate);//有效期至
                    this.forms[0].updateRecord(masterRow);
                    this.forms[0].loadRecord(masterRow);
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

                        ////交易数量改变重新计算
                        //对比最小批量返回新数量
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

                        //交易数量变更引发数量变化
                        var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO"), 0, info.DEALSQUANTITY, e.dataInfo.dataRow.get("UNITID"), 0]);
                        e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);

                        //金额变更引起的其它字段的变更
                        ScmMoneyBcf.DealsQuantity = info.DEALSQUANTITY;
                        ScmMoneyBcf.Quantity = unitData.Quantity;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeDealsQuantity]);
                        getPurchaseOrder.call(this, e, data);

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
                if (e.dataInfo.fieldName == "MATERIALID") {
                    if (e.dataInfo.dataRow.data["FROMBILLNO"] != "") {
                        Ext.Msg.alert("提示", "该物料有来源订单,不允许修改！");
                        e.dataInfo.cancel = true;
                    }
                    var contactsobjectId = this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTID'];
                    var returnData = this.invorkBcf('GetUnitPriceJson', [e.dataInfo.value, contactsobjectId]);
                    var list = returnData;//一般是中间层返回来的数据
                    if (list != undefined && list.length > 0) {
                        var info = list[0];
                        e.dataInfo.dataRow.set("DEALSUNITID", info.UNITID);
                        e.dataInfo.dataRow.set("DEALSUNITNO", info.UNITNO);
                        e.dataInfo.dataRow.set("DEALSUNITNAME", info.UNITNAME);
                        e.dataInfo.dataRow.set("PRICE", info.PRICE);
                        if (info.PRICE >= 0) {
                            //单价变更引起的其它字段的变更
                            ScmMoneyBcf.Price = info.PRICE;
                            var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangePrice]);
                            getPurchaseOrder.call(this, e, data);
                        }
                    }
                }

                //循环统计数值
                var items = this.dataSet.getTable(1).data.items;
                var dealsquantity = 0;
                var quantity = 0;
                var amount = 0;
                var taxamount = 0;
                var taxes = 0;
                var bwamount = 0;
                var bwtaxamount = 0;
                var bwtaxes = 0;
                for (var i = 0; i < items.length; i++) {
                    var floatDealsQuantity = items[i].data["DEALSQUANTITY"];
                    var floatQuantity = items[i].data["QUANTITY"];
                    var floatAmount = items[i].data["AMOUNT"];
                    var floatTaxAmount = items[i].data["TAXAMOUNT"];
                    var floatTaxes = items[i].data["TAXES"];
                    var floatBwAmount = items[i].data["BWAMOUNT"];
                    var floatBwTaxAmount = items[i].data["BWTAXAMOUNT"];
                    var floatBwTaxes = items[i].data["BWTAXES"];
                    //交易数量
                    if (e.dataInfo.fieldName == "DEALSQUANTITY" && parseInt(e.dataInfo.dataRow.data["ROW_ID"]) == items[i].data["ROW_ID"]) {
                        floatDealsQuantity = e.dataInfo.value;
                    }
                    //基本数量
                    if (e.dataInfo.fieldName == "QUANTITY" && parseInt(e.dataInfo.dataRow.data["ROW_ID"]) == items[i].data["ROW_ID"]) {
                        floatQuantity = e.dataInfo.value;
                    }
                    //金额
                    if (e.dataInfo.fieldName == "AMOUNT" && parseInt(e.dataInfo.dataRow.data["ROW_ID"]) == items[i].data["ROW_ID"]) {
                        floatAmount = e.dataInfo.value;
                    }
                    //含税金额
                    if (e.dataInfo.fieldName == "TAXAMOUNT" && parseInt(e.dataInfo.dataRow.data["ROW_ID"]) == items[i].data["ROW_ID"]) {
                        floatTaxAmount = e.dataInfo.value;
                    }
                    //税额
                    if (e.dataInfo.fieldName == "TAXES" && parseInt(e.dataInfo.dataRow.data["ROW_ID"]) == items[i].data["ROW_ID"]) {
                        floatTaxes = e.dataInfo.value;
                    }
                    //本位币金额
                    if (e.dataInfo.fieldName == "BWAMOUNT" && parseInt(e.dataInfo.dataRow.data["ROW_ID"]) == items[i].data["ROW_ID"]) {
                        floatBwAmount = e.dataInfo.value;
                    }
                    //本位币含税金额
                    if (e.dataInfo.fieldName == "BWTAXAMOUNT" && parseInt(e.dataInfo.dataRow.data["ROW_ID"]) == items[i].data["ROW_ID"]) {
                        floatBwTaxAmount = e.dataInfo.value;
                    }
                    //本位币税额
                    if (e.dataInfo.fieldName == "BWTAXES" && parseInt(e.dataInfo.dataRow.data["ROW_ID"]) == items[i].data["ROW_ID"]) {
                        floatBwTaxes = e.dataInfo.value;
                    }
                    dealsquantity += parseFloat(floatDealsQuantity);
                    quantity += parseFloat(floatQuantity);
                    amount += parseFloat(floatAmount);
                    taxamount += parseFloat(floatTaxAmount);
                    taxes += parseFloat(floatTaxes);
                    bwamount += parseFloat(floatBwAmount);
                    bwtaxamount += parseFloat(floatBwTaxAmount);
                    bwtaxes += parseFloat(floatBwTaxes);
                }
                Ext.getCmp("ALLDEALSQUANTITYS0_" + this.winId).setValue(dealsquantity);
                Ext.getCmp("ALLQUANTITYS0_" + this.winId).setValue(quantity);
                Ext.getCmp("ALLAMOUNTS0_" + this.winId).setValue(amount);
                Ext.getCmp("ALLTAXAMOUNTS0_" + this.winId).setValue(taxamount);
                Ext.getCmp("ALLTAXES0_" + this.winId).setValue(taxes);
                Ext.getCmp("ALLBWAMOUNTS0_" + this.winId).setValue(bwamount);
                Ext.getCmp("ALLBWTAXAMOUNTS0_" + this.winId).setValue(bwtaxamount);
                Ext.getCmp("ALLBWTAXES0_" + this.winId).setValue(bwtaxes);
                //更新表头数据
                this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                this.forms[0].loadRecord(this.dataSet.getTable(0).data.items[0]);

            }
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 0) {
                //表头完结提示
                if (e.dataInfo.fieldName == "ISEND") {
                    if (e.dataInfo.value != e.dataInfo.oldValue && e.dataInfo.value == true) {
                        Ext.Msg.confirm('提示', '完结后订单无法修改，是否确认完结？', function (button) {
                            if (button == "yes") {
                                this.confirmed = true;
                            }
                            else if (button == "no") {
                                this.dataSet.getTable(0).data.items[0].set("ISEND", e.dataInfo.oldValue);
                                Ext.getCmp('ISEND0_' + this.winId).setValue(e.dataInfo.oldValue);
                            }
                        }, this);
                    }

                }
                if (e.dataInfo.fieldName == "FROMTYPE") {
                    if (masterRow.get("RELATIONCODE") != "") {
                        var items = this.dataSet.getTable(1).data.items;
                        for (var i = 0; i < items.length; i++) {
                            if (this.dataSet.getTable(1).data.items[i].get("FROMBILLNO") == masterRow.get("RELATIONCODE")) {
                                Ext.Msg.confirm('提示', '单据明细中有该来源单号明细，是否确认更改来源？更改将清空所有明细。', function (button) {
                                    if (button == "yes") {
                                        this.confirmed = true;
                                        this.dataSet.getTable(1).removeAll();
                                    }
                                    else if (button == "no") {
                                        this.dataSet.getTable(0).data.items[0].set("FROMTYPE", e.dataInfo.oldValue);
                                        Ext.getCmp('FROMTYPE0_' + this.winId).setValue(e.dataInfo.oldValue);
                                    }
                                }, this);
                            }
                        }
                    }
                }
                if (e.dataInfo.fieldName == "RELATIONCODE") {
                    if (e.dataInfo.oldValue != "") {
                        var items = this.dataSet.getTable(1).data.items;
                        for (var i = 0; i < items.length; i++) {
                            if (this.dataSet.getTable(1).data.items[i].get("FROMBILLNO") == e.dataInfo.oldValue) {
                                Ext.Msg.confirm('提示', '单据明细中有该来源单号明细，是否确认更改来源？更改将清空所有明细。', function (button) {
                                    if (button == "yes") {
                                        this.confirmed = true;
                                        this.dataSet.getTable(1).removeAll();
                                    }
                                    else if (button == "no") {
                                        this.dataSet.getTable(0).data.items[0].set("RELATIONCODE", e.dataInfo.oldValue);
                                        Ext.getCmp('RELATIONCODE0_' + this.winId).setValue(e.dataInfo.oldValue);
                                    }
                                }, this);
                            }
                        }
                    }
                }

            }
            break;
        case LibEventTypeEnum.DeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                ALLCount.call(this, e); //统计
            }
            break;
        case LibEventTypeEnum.AddRow:
            if (e.dataInfo.tableIndex == 1) {
                var masterRow = this.dataSet.getTable(0).data.items[0];
                this.forms[0].updateRecord(masterRow);
                var taxrate = masterRow.get('TAXRATE');
                e.dataInfo.dataRow.set("TAXRATE", taxrate);//赋值表头税率
                var preparedate = masterRow.get('PREPAREDATE');
                e.dataInfo.dataRow.set("PREPAREDATE", preparedate);//赋值表头预计到货日期
            }
            break;

        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnLoadData") {
                var relationCode = this.dataSet.getTable(0).data.items[0].data['RELATIONCODE']; //获取来源单号
                var fromType = this.dataSet.getTable(0).data.items[0].data['FROMTYPE']; //获取来源类型
                if (fromType != 0)
                    fromType = parseFloat(fromType) - 1;

                var taxRate = this.dataSet.getTable(0).data.items[0].data['TAXRATE']; //获取税率
                var standardcoilRate = this.dataSet.getTable(0).data.items[0].data['STANDARDCOILRATE']; //获取汇率
                var contactsobjectId = this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTID']; //获取供应商
                if (this.isEdit) {
                    Ax.utils.LibVclSystemUtils.openDataFunc('pur.ChaseOrderDataFunc', '选择来源数据查询', [relationCode, fromType, taxRate, standardcoilRate, contactsobjectId, this]);
                }
                else {
                    Ext.Msg.alert("系统提示", "编辑状态才能使用数据加载按钮！");
                }
            }
            else if (e.dataInfo.fieldName == "BtnTrace") {
                var masterRow = this.dataSet.getTable(0).data.items[0].data;
                var record = [];
                record.push({
                    CURRENTSTATE: masterRow["CURRENTSTATE"],
                    BILLNO: masterRow["BILLNO"],
                    PERSONID: masterRow["PERSONID"],
                    CONTRACTCODE: masterRow["CONTRACTCODE"],
                    CONTACTSOBJECTID: masterRow["CONTACTSOBJECTID"]
                });

                var data = this.invorkBcf('BuildOrderTracing', [record]);
                if (data != null) {
                    var curPks = [];
                    curPks.push(data[0].BILLNO);
                    var typeId = data[0].TYPEID;
                    var entryParam = '{"ParamStore":{"TYPEID":"' + typeId + '"}}';
                    Ax.utils.LibVclSystemUtils.openBill('pur.ChaseOrderTracing', 1, "采购订单追踪", BillActionEnum.Browse, Ext.decode(entryParam), curPks);
                }
            }
            else if (e.dataInfo.fieldName == "BtnChange") {
                var headTable = this.dataSet.getTable(0).data.items[0].data;
                var bodyTable = this.dataSet.getTable(1).data.items;
                var head = [];
                var body = [];
                head.push({
                    CurrentState:headTable["CURRENTSTATE"],
                    BILLNO: headTable["BILLNO"],
                    PERSONID: headTable["PERSONID"],
                    CONTRACTCODE: headTable["CONTRACTCODE"],
                    CONTACTSOBJECTID: headTable["CONTACTSOBJECTID"],
                    TRANSPORTWAYID: headTable["TRANSPORTWAYID"],
                    BILLDATE: headTable["BILLDATE"],
                    ISEND: headTable["ISEND"]
                });
                for (var i = 0; i < bodyTable.length; i++) {
                    body.push({
                        BILLNO: bodyTable[i].data["BILLNO"],
                        ROW_ID: bodyTable[i].data["ROW_ID"],
                        MATERIALID: bodyTable[i].data["MATERIALID"],
                        MATERIALTYPEID: bodyTable[i].data["MATERIALTYPEID"],
                        ISCHECK: bodyTable[i].data["ISCHECK"],
                        QUALITYREQUIRE: bodyTable[i].data["QUALITYREQUIRE"],
                        DEALSQUANTITY: bodyTable[i].data["DEALSQUANTITY"],
                        DEALSUNITID: bodyTable[i].data["DEALSUNITID"],
                        DEALSUNITNO: bodyTable[i].data["DEALSUNITNO"],
                        PRICE: bodyTable[i].data["PRICE"],
                        AMOUNT: bodyTable[i].data["AMOUNT"],
                        PREPAREDATE: bodyTable[i].data["PREPAREDATE"],
                        QUANTITY: bodyTable[i].data["QUANTITY"],
                        UNITID: bodyTable[i].data["UNITID"],
                        TAXRATE: bodyTable[i].data["TAXRATE"],
                        TAXPRICE: bodyTable[i].data["TAXPRICE"],
                        TAXAMOUNT: bodyTable[i].data["TAXAMOUNT"],
                        TAXES: bodyTable[i].data["TAXES"],
                        MTONO: bodyTable[i].data["MTONO"],
                        ISEND: bodyTable[i].data["ISEND"],
                        REMARK: bodyTable[i].data["REMARK"]
                    });
                }
                var data = this.invorkBcf('BuildOrderChange', [head, body]);
                if (data != null) {
                    var curPks = [];
                    curPks.push(data[0].BILLNO);
                    var typeId = data[0].TYPEID;
                    var entryParam = '{"ParamStore":{"TYPEID":"' + typeId + '"}}';
                    Ax.utils.LibVclSystemUtils.openBill('pur.ChaseOrderChange', 1, "采购订单变更单", BillActionEnum.Browse, Ext.decode(entryParam), curPks);
                }
            }
            else if (e.dataInfo.fieldName == "PrintPurchase") {
                printChaseOrder(this, false, false);

            }
            else if (e.dataInfo.fieldName == "PrintOutSource") {
                printChaseOrder(this, false, true);
            }
            else if (e.dataInfo.fieldName == "UpLoadDeatail") {
                var This = this;
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
                                                proto.FillData.call(This, detail);
                                                win.close();
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
            break;

        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.fieldName == "ATTRIBUTENAME") {
                var MaterialId = e.dataInfo.dataRow.data["MATERIALID"];
                var AttributeId = e.dataInfo.dataRow.data["ATTRIBUTEID"];
                console.log(AttributeId);
                var AttributeCode = e.dataInfo.dataRow.data["ATTRIBUTECODE"]
                if (AttributeId != "") {
                    var returnData = this.invorkBcf('GetAttJson', [MaterialId, AttributeId, AttributeCode]);
                    var dataList = {
                        MaterialId: e.dataInfo.dataRow.data["MATERIALID"],
                        AttributeId: e.dataInfo.dataRow.data["ATTRIBUTEID"],
                        AttributeDesc: e.dataInfo.dataRow.data["ATTRIBUTEDESC"],
                        AttributeCode: e.dataInfo.dataRow.data["ATTRIBUTECODE"],
                        BillNo: e.dataInfo.dataRow.data["BILLNO"],
                        Row_Id: e.dataInfo.dataRow.data["ROW_ID"]
                    };
                    CreatAttForm(dataList, returnData, this, e, FillDataRow);
                }
            }

    }

}
//统计所有数
function ALLCount(e) {
    //循环统计 
    var items = this.dataSet.getTable(1).data.items;
    var dealsquantity = 0;
    var quantity = 0;
    var amount = 0;
    var taxamount = 0;
    var taxes = 0;
    var bwamount = 0;
    var bwtaxamount = 0;
    var bwtaxes = 0;
    for (var i = 0; i < items.length; i++) {
        var floatDealsQuantity = items[i].data["DEALSQUANTITY"];
        var floatQuantity = items[i].data["QUANTITY"];
        var floatAmount = items[i].data["AMOUNT"];
        var floatTaxAmount = items[i].data["TAXAMOUNT"];
        var floatTaxes = items[i].data["TAXES"];
        var floatBwAmount = items[i].data["BWAMOUNT"];
        var floatBwTaxAmount = items[i].data["BWTAXAMOUNT"];
        var floatBwTaxes = items[i].data["BWTAXES"];
        dealsquantity += parseFloat(floatDealsQuantity);
        quantity += parseFloat(floatQuantity);
        amount += parseFloat(floatAmount);
        taxamount += parseFloat(floatTaxAmount);
        taxes += parseFloat(floatTaxes);
        bwamount += parseFloat(floatBwAmount);
        bwtaxamount += parseFloat(floatBwTaxAmount);
        bwtaxes += parseFloat(floatBwTaxes);
    }
    Ext.getCmp("ALLDEALSQUANTITYS0_" + this.winId).setValue(dealsquantity);
    Ext.getCmp("ALLQUANTITYS0_" + this.winId).setValue(quantity);
    Ext.getCmp("ALLAMOUNTS0_" + this.winId).setValue(amount);
    Ext.getCmp("ALLTAXAMOUNTS0_" + this.winId).setValue(taxamount);
    Ext.getCmp("ALLTAXES0_" + this.winId).setValue(taxes);
    Ext.getCmp("ALLBWAMOUNTS0_" + this.winId).setValue(bwamount);
    Ext.getCmp("ALLBWTAXAMOUNTS0_" + this.winId).setValue(bwtaxamount);
    Ext.getCmp("ALLBWTAXES0_" + this.winId).setValue(bwtaxes);
    //更新表头数据
    this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
    this.forms[0].loadRecord(this.dataSet.getTable(0).data.items[0]);
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

    ALLCount.call(this, e); //统计
};

//最新特征窗体
function CreatAttForm(dataList, returnData, This, row, method) {

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

                unstandard.push(CreatTextBox(returnData[i], isRead));
            }
            else {
                standard.push(CreatTextBox(returnData[i], isRead));
            }
        }
        else {
            if (returnData[i].Standard) {
                unstandard.push(CreatComBox(returnData[i], isRead));
            }
            else {
                standard.push(CreatComBox(returnData[i], isRead));
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
            id: 'Att' + attId,
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
            id: 'noAtt' + attId,
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
    attId++;
    Salewin.show();
    Salewin.items.items[1].collapse(true);
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
function FillDataRow(e, This, CodeDesc) {
    e.dataInfo.dataRow.set("ATTRIBUTECODE", CodeDesc.Code);
    e.dataInfo.dataRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
    // e.dataInfo.dataRow.set("ABNORMALDAY", CodeDesc.AbnormalDay);
    //设置异常天数
    //var masterRow = This.dataSet.getTable(0).data.items[0];
    //Ext.getCmp("ABNORMALDAY0_" + This.winId).setValue(CodeDesc.AbnormalDay);
    return true;
}

//填充组合品窗口的特征信息
function FillCombineForm(panel, This, CodeDesc) {
    for (var i = 0; i < newPanel.items.items.length  ; i++) {
        if (newPanel.items.items[i].materialId == panel.materialId && newPanel.items.items[i].attributeCode == CodeDesc.Code && newPanel.items.items[i].id != panel.id) {
            Ext.Msg.alert("提示", '该行与第' + (i + 1) + '行重复！');
            return false;
        }
    }
    panel.items.items[5].setValue(CodeDesc.Code);
    panel.items.items[6].setValue(CodeDesc.Desc);
    panel.attributeCode = CodeDesc.Code;
    panel.attributeDesc = CodeDesc.Desc;
    return true;
}



function printChaseOrder(This, isPaint, isOut) {


    //构建表身
    var strTableTrHtml = "";
    var strTableEndHtml = "</table>";
    var strTableStartHtml = "<style>  .table-a table{border-right:1px solid #000;border-bottom:1px solid #000 ; table-layout:fixed;} .table-a table th{border-left:1px solid #000;border-top:1px solid #000}  .table-a table td{border-left:1px solid #000;border-top:1px solid #000; } </style> "
    strTableStartHtml += "<div class='table-a'><table border='0'  id='theTable' width='100%' cellpadding='0' cellspacing='0' align='center' >";
    var strTableTheadHtml = "<thead style='height: 38px ' face='宋体' bgcolor='#FFFFFF'>";

    strTableTheadHtml += "<th nowrap align='center'style='width:30px' ><font size = '2px' face='宋体'>序号</font></th>";
    strTableTheadHtml += "<th nowrap align='center' style='width:100px'><font size = '2px' face='宋体'>品名</font></th>";
    strTableTheadHtml += "<th nowrap align='center' style='width:250px'><font size = '2px' face='宋体'>规格型号</font></th>";//nowrap自动换行
    strTableTheadHtml += "<th nowrap align='center' style='width:80px'><font size = '2px' face='宋体'>数量</font></th>";
    strTableTheadHtml += "<th nowrap align='center' style='width:100px'><font size = '2px' face='宋体'>单价</font></th>";
    strTableTheadHtml += "<th nowrap align='center' style='width:100px'><font size = '2px' face='宋体'>总价</font></th>";
    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>交货日期</font></th>";
    strTableTheadHtml += "</thead>";
    var bodyTable = This.dataSet.getTable(1);
    var headTable = This.dataSet.getTable(0);
    var Suppliers = This.invorkBcf('GetSuppliers', [headTable.data.items[0].data["CONTACTSOBJECTID"]]);



    var ProductOrders = This.invorkBcf('GetGetProductOrder', [headTable.data.items[0].data["CONTRACTCODE"]]);//投产单打印相关
    var PhoneAndFax = changeUndefined(Suppliers[4]);//供方电话/传真
    PhoneAndFax = PhoneAndFax.length > 0 ? PhoneAndFax + changeUndefined(Suppliers[5]) : changeUndefined(Suppliers[5])
    //第三页
    //var tdStyle = "style='border-left:1px solid #000;border-top:1px solid #000'";
    var strTableWGFHtml = "<style>  .table-c table{border-right:1px solid #000;border-bottom:1px solid #000}   .table-c table td{border-left:1px solid #000;border-top:1px solid #000} </style> "
    strTableWGFHtml += "<table  width='120%' border='0' cellpadding='0' cellspacing='0'  style='font-size: 9pt;line-height:30px'><tr><td colspan='5'rowspan='3'x:str><img src=./Scripts/img/images/zhongdePrint.png height=60 width=60></td><td colspan='17'rowspan='2'x:str  align='center' valign='bottom' style='padding-top:20px;'><font size = '4pt' face='黑体'>浙江中德阀门自控科技有限公司</font></td><td></td><td></td><td></td><td></td><td></td><td></td></tr><tr><td></td><td></td><td></td><td></td><td></td><td></td></tr><tr><td colspan='17'x:str align='center'>合同附件清单(合同编号：" + headTable.data.items[0].data["BILLNO"] + ")对应：" + changeUndefined(ProductOrders[0]) + ",交货期：" + changeUndefined(ProductOrders[1]) + "到货</td><td></td><td></td><td></td><td></td><td></td><td></td></tr><tr><td colspan='7'>合同日期：" + changeTodateTwo(headTable.data.items[0].data["BILLDATE"]) + "</td><td></td><td></td><td></td><td colspan='6'x:str>合同编号：" + headTable.data.items[0].data["BILLNO"] + "</td><td></td><td></td><td></td><td></td><td></td><td colspan='7'x:str>合同号：" + headTable.data.items[0].data["PRODUCTCONTRACTNO"] + "</td></tr><tr><td colspan='7'x:str>需方：浙江中德自控科技股份有限公司</td><td></td><td></td><td></td><td colspan='5'x:str>联系人：" + headTable.data.items[0].data["PERSONNAME"] + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td colspan='7'x:str>电话/传真：0572-6660050/0572-6660133</td></tr><tr><td colspan='7'x:str>供方：" + headTable.data.items[0].data["CONTACTSOBJECTNAME"] + "</td><td></td><td></td><td></td><td colspan='5'x:str>联系人：" + changeUndefined(Suppliers[0]) + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td colspan='7'x:str>电话/传真：" + PhoneAndFax + "</td></tr><tr><td colspan='24' x:str>备注：" + headTable.data.items[0].data["REMARK"] + "</td></tr></table>"
    strTableWGFHtml += "<div class='table-c'><table   border='0'  width='120%'  cellpadding='0' cellspacing='0' style='font-size: 8pt;'>";
    strTableWGFHtml += "<tr ><td  rowspan='2' >序号</td><td rowspan='2'>产品名称</td><td rowspan='2'>特征项</td><td rowspan='2'>口径</td><td rowspan='2'>压力</td><td colspan='11'align='center'>阀门参数</td><td colspan='4'align='center'>工况条件</td><td colspan='6'align='center'>执行机构参数</td><td rowspan='2'>数量</td><td rowspan='2'>单价（含税）</td><td rowspan='2'>总价（含税）</td></tr>";
    strTableWGFHtml += "<tr ><td>结构形式</td><td>连接形式</td><td>法兰标准</td><td>阀体材质</td><td>阀芯材质</td><td>阀座材质</td><td>阀杆材质</td><td>填料材质</td><td>上阀盖型式</td><td>结构长度</td><td>密封等级</td><td>气源/电源故障要求</td><td>介质</td><td>最大压差</td><td>设计/操作温度</td><td>执行机构型式</td><td>执行机构型号</td><td>手操</td><td>信号源（最大/最小）Mpa</td><td>行程</td><td>时间</td></tr>";

    var count = 0, amount = 0, no = 0;
    var wgcount = 0, wgamount = 0, wgno = 0;
    for (var i = 0; i < bodyTable.data.items.length; i++) {

        //ps:2017-4-28 16:58:08
        var attributedesc = bodyTable.data.items[i].data["ATTRIBUTEDESC"];//特征描述
        var contractno, factoryno, wgattributedesc;//定义合同号、出厂编号、外购阀特征标识
        if (isOut) {
            if (attributedesc != "") {
                var arr = changeAttribute(attributedesc);
                for (var a = 0; a < arr.length; a++) {
                    var list = arr[a].split(":");
                    if (list[0] == "合同号")
                        contractno = list[1];
                    if (list[0] == "出厂编号")
                        factoryno = list[1];
                }
                if (contractno != undefined && factoryno != undefined)
                    wgattributedesc = This.invorkBcf('GetProductOrderAttributedesc', [contractno, factoryno]);
            }
        }

        //是否外购阀
        if (wgattributedesc != undefined) {
            wgno++;
            //公称通径, 公称压力, 阀门型式, 阀门连接形式, 阀门连接标准, 阀体材质及阀盖材质, 阀芯材质, 阀座材质, 阀杆材质, 填料材质, 阀盖 / 支架类型, 结构长度, 泄漏等级, 电源故障时阀门状态, 介质名称, 最大关闭差压, 设计温度 / 操作温度, 执行机构型式, 执行机构型号, 执行机构手轮装置, 最大气源压力 / 最小气源压力, 阀体行程, 关闭时间
            var attribute0, attribute1, attribute2, attribute3, attribute4, attribute5, attribute6, attribute7, attribute8, attribute9, attribute10, attribute11, attribute12, attribute13, attribute14, attribute15, attribute16, attribute17, attribute18, attribute19, attribute20, attribute21, attribute22, attribute23 = "";
            var arr = changeAttribute(wgattributedesc);
            attribute0 = contractno + "/" + factoryno;
            for (var b = 0; b < arr.length; b++) {
                var list = arr[b].split(":");
                if (list[0] == "公称通径")
                    attribute1 = list[1];
                if (list[0] == "公称压力")
                    attribute2 = list[1];
                if (list[0] == "阀门型式")
                    attribute3 = list[1];
                if (list[0] == "阀门连接形式")
                    attribute4 = list[1];
                if (list[0] == "阀门连接标准")
                    attribute5 = list[1];
                if (list[0] == "阀体材质及阀盖材质")
                    attribute6 = list[1];
                if (list[0] == "阀芯材质")
                    attribute7 = list[1];
                if (list[0] == "阀座材质")
                    attribute8 = list[1];
                if (list[0] == "阀杆材质")
                    attribute9 = list[1];
                if (list[0] == "填料材质")
                    attribute10 = list[1];
                if (list[0] == "阀盖/支架类型")
                    attribute11 = list[1];
                if (list[0] == "结构长度")
                    attribute12 = list[1];
                if (list[0] == "泄漏等级")
                    attribute13 = list[1];
                if (list[0] == "电源故障时阀门状态")
                    attribute14 = list[1];
                if (list[0] == "介质名称")
                    attribute15 = list[1];
                if (list[0] == "最大关闭差压")
                    attribute16 = list[1];
                if (list[0] == "设计温度")
                    attribute17 = list[1];
                if (list[0] == "操作温度")
                    attribute17 = attribute17 == null ? list[1] : attribute17 + "/" + list[1];
                if (list[0] == "执行机构型式")
                    attribute18 = list[1];
                if (list[0] == "执行机构型号")
                    attribute19 = list[1];
                if (list[0] == "执行机构手轮装置")
                    attribute20 = list[1];
                if (list[0] == "最大气源压力")
                    attribute21 = list[1];
                if (list[0] == "最小气源压力")
                    attribute21 = attribute20 == null ? list[1] : attribute20 + "/" + list[1];
                if (list[0] == "阀体行程")
                    attribute22 = list[1];
                if (list[0] == "关闭时间")
                    attribute23 = list[1];
            }
            wgcount += bodyTable.data.items[i].data["DEALSQUANTITY"];
            wgamount += bodyTable.data.items[i].data["BWTAXAMOUNT"];
            strTableWGFHtml += "<tr><td border='thin'>" + wgno + "</td><td>" + bodyTable.data.items[i].data["MATERIALNAME"] + "</td><td>" + changeUndefined(attribute0) + "</td><td>" + changeUndefined(attribute1) + "</td><td>" + changeUndefined(attribute2) + "</td><td>" + changeUndefined(attribute3) + "</td><td>" + changeUndefined(attribute4) + "</td><td>" + changeUndefined(attribute5) + "</td><td>" + changeUndefined(attribute6) + "</td><td>" + changeUndefined(attribute7) + "</td><td>" + changeUndefined(attribute8) + "</td><td>" + changeUndefined(attribute9) + "</td><td>" + changeUndefined(attribute10) + "</td><td>" + changeUndefined(attribute11) + "</td><td>" + changeUndefined(attribute12) + "</td><td>" + changeUndefined(attribute13) + "</td><td>" + changeUndefined(attribute14) + "</td><td>" + changeUndefined(attribute15) + "</td><td>" + changeUndefined(attribute16) + "</td><td>" + changeUndefined(attribute17) + "</td><td>" + changeUndefined(attribute18) + "</td><td>" + changeUndefined(attribute19) + "</td><td>" + changeUndefined(attribute20) + "</td><td>" + changeUndefined(attribute21) + "</td><td>" + changeUndefined(attribute22) + "</td><td>" + changeUndefined(attribute23) + "</td><td>" + bodyTable.data.items[i].data["DEALSQUANTITY"] + "</td><td>" + (parseFloat(bodyTable.data.items[i].data["BWTAXAMOUNT"]) / parseFloat(bodyTable.data.items[i].data["DEALSQUANTITY"])).toFixed(4) + "</td><td>" + changeMoney(bodyTable.data.items[i].data["BWTAXAMOUNT"], 2) + "</td></tr>"
        }
        else {
            no++;
            count += bodyTable.data.items[i].data["DEALSQUANTITY"];
            amount += bodyTable.data.items[i].data["BWTAXAMOUNT"];
            strTableTheadHtml += "<tr>";
            strTableTheadHtml += "<td nowrap align='center' ><font size='2px' face='宋体'>" + no + "</font></td>";
            strTableTheadHtml += "<td nowrap align='center' style='WORD-WRAP: break-word' width='100'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["MATERIALNAME"] + "</font></td>";
            strTableTheadHtml += "<td nowrap align='center' style='WORD-WRAP: break-word' width='245'><font size='2px' face='宋体'>" + changeStr(bodyTable.data.items[i].data["SPECIFICATION"], bodyTable.data.items[i].data["TEXTUREID"], bodyTable.data.items[i].data["FIGURENO"]) + "</font></td>";
            strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["DEALSQUANTITY"] + "</font></td>";
            strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + (parseFloat(bodyTable.data.items[i].data["BWTAXAMOUNT"]) / parseFloat(bodyTable.data.items[i].data["DEALSQUANTITY"])).toFixed(4) + "</font></td>";
            strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + changeMoney(bodyTable.data.items[i].data["BWTAXAMOUNT"], 2) + "</font></td>";
            strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + changeTodate(bodyTable.data.items[i].data["PREPAREDATE"]) + "</font></td>";
        }
    }

    //@dai 数量、金额的数字合计 先注释 如有需求取消注释 2017-5-2 17:28:11
    strTableWGFHtml += "<tr style='height: 30px' face='宋体' bgcolor='#FFFFFF'>";
    strTableWGFHtml += "<td nowrap align='center'  colspan =30><font size = '3px' face='宋体'>合计：" + changeNumMoneyToChinese(wgamount.toFixed(2)) + "</font></td>";
    strTableWGFHtml += "</tr>";
    strTableWGFHtml += "</table></div>";

    strTableTheadHtml += "<thead style='height: 30px' face='宋体' bgcolor='#FFFFFF'>";
    strTableTheadHtml += "<td nowrap align='center'><font size = '2px' face='宋体'>总计</font></td>";
    strTableTheadHtml += "<td nowrap align='center' colspan =2><font size = '2px' face='宋体'></font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>" + count + "</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'></font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>¥" + changeMoney(amount, 2) + "</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'></font></td>";
    strTableTheadHtml += "</thead>";
    strTableTheadHtml += "<thead style='height: 30px' face='宋体' bgcolor='#FFFFFF'>";
    strTableTheadHtml += "<td nowrap align='left'  colspan =7><font size = '3px' face='宋体'>金额总计（人民币大写）：" + changeNumMoneyToChinese(amount.toFixed(2)) + "</font></td>";
    strTableTheadHtml += "</thead>";
    strTableTheadHtml += "<thead style='height: 30px' face='宋体' bgcolor='#FFFFFF' bordercolor='#FFFFFF'>";
    strTableTheadHtml += "<td nowrap align='left'  colspan =7><font size = '2px' face='宋体'>说明：为了配合我司智能工厂管理，供应商送货单上必须附对应的采购订单号，否则拒收。</font></td>";
    strTableTheadHtml += "</thead>";
    strTableTheadHtml += "</table></div>";


    var strTableFootHtml = "<div style='line-height:15px;font-size:12px;border:solid #000000 2px;'>";
    strTableFootHtml += "<div style='float:left;width:49%;'>";
    strTableFootHtml += "<p style='text-align:center'>需方（订货单位）</p>";
    strTableFootHtml += "<p>单位名称：  浙江中德自控科技股份有限公司</p>";
    strTableFootHtml += "<p>联 系 人：  " + headTable.data.items[0].data["PERSONNAME"] + "</p>";
    strTableFootHtml += "<p>地    址：  长兴县太湖街道长兴大道659号</p>"
    strTableFootHtml += "<p>开户银行：  中国工商银行长兴县支行 </p>"
    strTableFootHtml += "<p>帐    号：  1205270019200139686</p>"
    strTableFootHtml += "<p>税务登记号：330522668348736</p>"
    strTableFootHtml += "<p>电    话：  0572-6660050</p>"
    strTableFootHtml += "<p>传    真：  0572-6660133</p>"
    strTableFootHtml += "</div>"

    strTableFootHtml += "<div style='float:left;width:50%;border-left:solid #000000 1px;'>";
    strTableFootHtml += "<p style='text-align:center'>供方（供货单位）</p>";
    strTableFootHtml += "<p>单位名称：  " + headTable.data.items[0].data["CONTACTSOBJECTNAME"] + "</p>";
    strTableFootHtml += "<p>联 系 人：  " + changeUndefined(Suppliers[0]) + "</p>";
    strTableFootHtml += "<p>地    址：  " + changeUndefined(Suppliers[1]) + "</p>"
    strTableFootHtml += "<p>开户银行：  " + changeUndefined(Suppliers[2]) + "</p>"
    strTableFootHtml += "<p>帐    号：  " + changeUndefined(Suppliers[3]) + "</p>"
    strTableFootHtml += "<p>税务登记号： </p>"
    strTableFootHtml += "<p>电    话：  " + changeUndefined(Suppliers[4]) + "</p>"
    strTableFootHtml += "<p>传    真：  " + changeUndefined(Suppliers[5]) + "</p>"
    strTableFootHtml += "</div>";
    strTableFootHtml += "<div style='clear:both'></div>";
    strTableFootHtml += "</div>";

    var strListHtml = "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>一、产品名称、商标、型号、厂家、数量、金额、供货时间及数量</font></p>"
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'><strong>详见附件清单</strong></font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;height:30px;line-height:15px;padding-left:60px'><span style='float:left'><font size = '1px' face='宋体'>二、质量要求技术标准:</font></span><span style='float:left'><font size = '1px' face='宋体'>1.产品合格率100%，乙方提供" + headTable.data.items[0].data["YFZL"] + "等相关资料。<br/>2.质保期" + headTable.data.items[0].data["ZBQ"] + "，质保期内若出现产品质量问题，乙方必须在24小时之内无偿响应。</font></span></p>"
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>三、交（提）货地点、时间、方式: 乙方负责在甲乙双方确认的期限内将产品送至" + headTable.data.items[0].data["SHDZ"] + "。</font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>四、运输方式及到达站港和费用负担: 乙方负责运费。</font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>五、合理损耗及计算方法: 无</font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>六、包装标准、包装物的供应与回收:  标准包装，不回收。</font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>七、验收标准: " + headTable.data.items[0].data["YSBZ"] + "。</font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>八、随机备品、配件工具数量及供应办法:" + headTable.data.items[0].data["GYBF"] + "</font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>九、结算方式及期限: " + headTable.data.items[0].data["JSFS"] + "。</font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>十、如需提供担保，另立合同担保书，作为本合同附件:无。</font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>十一、违约责任:按合同法有关规定执行。</font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>十二、解决合同纠纷的方式:本合同在履行过程中发生争议，由当事人双方协商解决，协商未果交由浙江长兴仲裁委仲裁。</font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>十三、其它约定事项: " + headTable.data.items[0].data["QTSX"] + "。</font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>十四、合同传真件及与本合同有关的往来邮件与合同本体具有同等法律效力。</font></p>";
    strListHtml += "<p style='border-bottom:#000000 solid 1px;line-height:15px;padding-left:60px'><font size = '1px' face='宋体'>十五、合同附件：" + headTable.data.items[0].data["HTFJ"] + "</font></p>";
    strListHtml += "<div style='line-height:15px;font-size:12px;border:solid #000000 2px;'>";
    strListHtml += "<div style='float:left;width:49%;'>";
    strListHtml += "<p style='text-align:center'>需 (甲)　　　　　　　方</p>"
    strListHtml += "<p>单位名称（章）：浙江中德自控科技股份有限公司</p>"
    strListHtml += "<p>单&nbsp;&nbsp;&nbsp;&nbsp;位&nbsp;&nbsp;&nbsp;&nbsp;地&nbsp;&nbsp;&nbsp;&nbsp;址：浙江省湖州市长兴县太湖街道长兴大道69号</p>"
    strListHtml += "<p>法定代表人：</p>"
    strListHtml += "<p>委托代理人：" + headTable.data.items[0].data["PERSONNAME"] + "</p>"
    strListHtml += "<p>电　　　话： 0572-666 0050</p>"
    strListHtml += "<p>传　　　真： 0572-666 0133</p>"
    strListHtml += "<p>开户银　行： 工行湖州分行长兴县支行</p>"
    strListHtml += "<p>账　　　号： 1205270019200130000</p>"

    strListHtml += "</div>"
    strListHtml += "<div style='float:left;width:50%;border-left:solid #000000 1px;'>";
    strListHtml += "<p style='text-align:center'>供 (乙)　　　　　　　方</p>"
    strListHtml += "<p>单位名称（章）：" + headTable.data.items[0].data["CONTACTSOBJECTNAME"] + "</p>"
    strListHtml += "<p>单&nbsp;&nbsp;&nbsp;&nbsp;位&nbsp;&nbsp;&nbsp;&nbsp;地&nbsp;&nbsp;&nbsp;&nbsp;址：" + changeUndefined(Suppliers[1]) + "</p>"
    strListHtml += "<p>法定代表人：" + changeUndefined(Suppliers[6]) + "</p>"
    strListHtml += "<p>委托代理人：" + changeUndefined(Suppliers[0]) + "</p>"
    strListHtml += "<p>电　　　话： " + changeUndefined(Suppliers[4]) + "</p>"
    strListHtml += "<p>传　　　真： " + changeUndefined(Suppliers[5]) + "</p>"
    strListHtml += "<p>开户银　行： " + changeUndefined(Suppliers[2]) + "</p>"
    strListHtml += "<p>账　　　号： " + changeUndefined(Suppliers[3]) + "</p>"
    strListHtml += "</div>";
    strListHtml += "<div style='clear:both'></div>";
    strListHtml += "</div>";
    strListHtml += "<div style='clear:both;text-align:right;line-height:15px;'><font size = '1px' face='宋体'>有效期限：" + changeTodateTwo(headTable.data.items[0].data["BILLDATE"]) + " 至 " + changeTodateTwo(headTable.data.items[0].data["YXQEND"]) + "</font></div>";



    var LODOP = getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
    //第一页头
    LODOP.ADD_PRINT_TEXT(20, 300, 500, 40, "产 品 购 销 合 同");
    LODOP.SET_PRINT_STYLEA(0, "FontName", "黑体");
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 18);
    LODOP.ADD_PRINT_IMAGE(10, 40, "60", "60", "<img src=./Scripts/img/images/zhongdePrint.png height=60 width=60>");//商标
    LODOP.ADD_PRINT_TEXT(90, 25, 370, 20, "需方(甲方)： 浙江中德自控科技股份有限公司 ");
    LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
    LODOP.ADD_PRINT_TEXT(120, 25, 370, 20, "供方(乙方)：  " + headTable.data.items[0].data["CONTACTSOBJECTNAME"]);
    LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
    if (headTable.data.items[0].data["PRODUCTCONTRACTNO"] != '') {
        LODOP.ADD_PRINT_TEXT(150, 25, 370, 20, "投产单号： " + headTable.data.items[0].data["PRODUCTCONTRACTNO"]);
    }
    LODOP.ADD_PRINT_TEXT(80, 550, 370, 20, "合同编号：  " + headTable.data.items[0].data["BILLNO"]);
    LODOP.ADD_PRINT_TEXT(110, 550, 370, 20, "签订地点： 浙江长兴");
    LODOP.ADD_PRINT_TEXT(140, 550, 370, 20, "签订时间： " + changeTodateTwo(headTable.data.items[0].data["BILLDATE"]));
    LODOP.ADD_PRINT_HTM(170, "2%", "96%", "100%", strListHtml); //ADD_PRINT_HTM(Top,Left,Width,Height,strHtmlContent)
    LODOP.NewPageA();
    if (wgno == 0) {
        //第二页
        LODOP.ADD_PRINT_TEXT(40, 250, 500, 40, "浙江中德阀门自控科技有限公司");
        LODOP.SET_PRINT_STYLEA(0, "FontName", "黑体");
        LODOP.SET_PRINT_STYLEA(0, "FontSize", 18);
        LODOP.ADD_PRINT_IMAGE(10, 40, "60", "60", "<img src=./Scripts/img/images/zhongdePrint.png height=60 width=60>");//商标
        LODOP.ADD_PRINT_TEXT(80, 350, 550, 30, "合同附件清单");
        LODOP.SET_PRINT_STYLEA(0, "FontSize", 15);
        LODOP.ADD_PRINT_TEXT(100, 550, 370, 20, "合同编号：  " + headTable.data.items[0].data["BILLNO"]);
        LODOP.ADD_PRINT_TEXT(120, 25, 370, 20, "订单日期：  " + changeTodateTwo(headTable.data.items[0].data["BILLDATE"]));
        LODOP.ADD_PRINT_TEXT(120, 250, 370, 20, "供方名称：  " + headTable.data.items[0].data["CONTACTSOBJECTNAME"]);
        LODOP.ADD_PRINT_TEXT(140, 25, 370, 20, "备注： " + headTable.data.items[0].data["REMARK"]);
        var htmlStr = strTableStartHtml + strTableTheadHtml + strTableEndHtml;
        LODOP.ADD_PRINT_HTM(160, 1, "100%", "100%", htmlStr); //ADD_PRINT_HTM(Top,Left,Width,Height,strHtmlContent)
        LODOP.ADD_PRINT_HTM(750, 1, "100%", "100%", strTableFootHtml);
        LODOP.NewPageA();
    }
        //第三页
    else if (wgno > 0) {
        LODOP.ADD_PRINT_HTM(0, 0, "100%", "100%", strTableWGFHtml);
        LODOP.SET_PRINT_STYLEA(0, "AngleOfPageInside", -90);
    }
    LODOP.SET_PRINT_PAGESIZE(1, 0, 0, "A4"); //1---纵(正)向打印  2---横向打印 SET_PRINT_PAGESIZE(intOrient,intPageWidth,intPageHeight,strPageName)设定纸张大小
    if (isPaint) {
        LODOP.PRINT();
    }
    else {
        LODOP.PREVIEW();
    }
}


//规格型号合并三字段加逗号分隔 
function changeStr(str1, str2, str3) {
    var rstr = "";
    if (str1 != "")
        rstr += str1;
    if (rstr != "" && str2 != "")
        rstr += "," + str2;
    if (rstr != "" && str3 != "")
        rstr += "," + str3;
    return rstr;
}

proto.FillData = function (returnList) {
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
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('SPECIFICATION', info.Specifiaction);
                newRow.set('TEXTUREID', info.TextureId);
                newRow.set('FIGURENO', info.FigureNo);
                newRow.set('MATERIALSPEC', info.MaterialSpec);
                //newRow.set('NEEDCHECK', info.NeedCheck);
                newRow.set('MATERIALTYPEID', info.MaterialTypeId);
                newRow.set('MATERIALTYPENAME', info.MaterialTypeName);
                newRow.set('UNITID', info.UnitId);
                newRow.set('UNITNAME', info.UnitName);
                newRow.set('DEALSUNITID', info.DealsUnitId);
                newRow.set('DEALSUNITNAME', info.DealsUnitName);
                newRow.set('DEALSUNITNO', info.DealsUnitNo);
                newRow.set('ATTRIBUTEID', info.AttributeId);
                newRow.set('ATTRIBUTENAME', info.AttributeName);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}