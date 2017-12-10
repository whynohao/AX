ppSubcontracteOrderVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = ppSubcontracteOrderVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = ppSubcontracteOrderVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);

    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var masterRow = this.dataSet.getTable(0).data.items[0];
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
                if (e.dataInfo.fieldName == "PPWORKORDER") {
                    var ppworkOrderNo = this.dataSet.getTable(0).data.items[0].get("PPWORKORDER");
                    if (ppworkOrderNo != "") {
                        this.loadBarcode(ppworkOrderNo, this);
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
    }
}
proto.loadBarcode = function (ppworkOrderNo) {
    var returnData = this.invorkBcf("GetPPWorkOrderBarcode", [ppworkOrderNo]);
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'PPSUBCONTRACTEORDERDETAILGrid');
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set("BARCODE", info.BARCODE);
                newRow.set("MATERIALID", info.MATERIALID);
                newRow.set("MATERIALNAME", info.MATERIALNAME);
                newRow.set("SPECIFICATION", info.SPECIFICATION);
                newRow.set("TEXTUREID", info.TEXTUREID);
                newRow.set("FIGURENO", info.FIGURENO);
                newRow.set("MATERIALSPEC", info.MATERIALSPEC);
                newRow.set("NEEDCHECK", info.NEEDCHECK);
                newRow.set("ATTRIBUTEID", info.ATTRIBUTEID);
                newRow.set("ATTRIBUTENAME", info.ATTRIBUTENAME);
                newRow.set("ATTRIBUTECODE", info.ATTRIBUTECODE);
                newRow.set("ATTRIBUTEDESC", info.ATTRIBUTEDESC);
                newRow.set("UNITID", info.UNITID);
                newRow.set("UNITNAME", info.UNITNAME);
                newRow.set("DEALSUNITID", info.UNITID);
                newRow.set("DEALSUNITNAME", info.UNITNAME);
                newRow.set("QUANTITY", info.QUANTITY);
                newRow.set("DEALSQUANTITY", info.QUANTITY);
                newRow.set("FROMBILLNO", info.FROMBILLNO);
                newRow.set("FROMROW_ID", info.FROMROW_ID);
                newRow.set("DEALSQUANTITY", 1);
                newRow.set("QUANTITY", 1);

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