stkPurStockInVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = stkPurStockInVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = stkPurStockInVcl;
var bodyRow;
var attId = 0;
//proto.companyId = "CGRK001";
//proto.doSetParam = function () {

//    var ctrl3 = Ext.getCmp("TYPEID0_" + this.winId);
//    ctrl3.store.add({ Id: proto.companyId, Name: "一般采购入库" });
//    ctrl3.select(proto.companyId);
//    this.forms[0].loadRecord(masterRow);
//};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (this.isEdit) {
                if (e.dataInfo.fieldName == "BtnLoadData") {
                    var contactObjectId = this.dataSet.getTable(0).data.items[0].data['CONTACTOBJECTID'];
                    var contactsObjectName = this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTNAME']
                    //if (contactObjectId == "") {
                    //    alert("往来单位不能为空！");
                    //}
                    //else {
                    Ax.utils.LibVclSystemUtils.openDataFunc('stk.PurStockInQuery', '采购入库单数据查询', [contactObjectId, contactsObjectName, this]);
                    //}
                }
            }
            else {
                alert("单据只有在修改状态才能载入采购入库数据！");
            }
            break;
            if (e.dataInfo.fieldName == "btnLoad") {
                var BillNo = this.dataSet.getTable(0).data.items[0].data['FROMBILLNO']; //获取来源单号
                if (BillNo.length > 0) {
                    var list = this.invorkBcf("GetBillDetail", [BillNo]);
                    FillData.call(this, list);
                }
                else {
                    Ext.Msg.alert("提示", "请选择来源单号！");
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo && e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.value != null) {
                    switch (e.dataInfo.fieldName) {
                        case 'MATERIALID'://仓库改变带出库存状态
                            if (e.dataInfo.dataRow.get("WAREHOUSEID") != "") {
                                var stkState = this.invorkBcf("GetStkState", [e.dataInfo.dataRow.get("WAREHOUSEID")]);
                                e.dataInfo.dataRow.set("STKSTATE", stkState);
                            }
                            break;
                    }

                    //修改来源行标识，带出应收数量
                    if (e.dataInfo.fieldName == "FROMROWID") {
                        var receivableQty = this.invorkBcf("GetReceivableQty", [e.dataInfo.dataRow.get("FROMBILLNO"), e.dataInfo.value]);
                        e.dataInfo.dataRow.set("RECEIVABLEQUANTITY", receivableQty);
                    }

                    var fname = e.dataInfo.fieldName;
                    if ($.inArray(fname, mylist) > -1) {
                        getInventroyQty(e.dataInfo.dataRow);
                    }
                }
            }
            if (e.dataInfo.tableIndex == 0) {
                //更改汇率计算
                if (e.dataInfo.fieldName == "STANDARDCOILRATE") {
                    var items = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < items.length; i++) {
                        var ScmMoneyBcf = {}; //金额、数量换算
                        ScmMoneyBcf.DealsQuantity = this.dataSet.getTable(1).data.items[i].get("DEALQUANTITY");//交易数量
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

                if (e.dataInfo.fieldName == "PERSONID") {
                    //选择仓管员带出仓库
                    if (e.dataInfo.value != null && e.dataInfo.value != "") {
                        var warehouse = this.invorkBcf("SynWareHouse", [e.dataInfo.dataRow.get("PERSONID")]);
                        if (warehouse.WAREHOUSEID != null) {
                            var field = Ext.getCmp('WAREHOUSEID0_' + this.winId);
                            field.store.add({ Id: warehouse.WAREHOUSEID, Name: warehouse.WAREHOUSENAME });
                            field.select(warehouse.WAREHOUSEID);
                        }
                    }
                }

                if (e.dataInfo.fieldName == "PRODUCTORDER") {
                    if (e.dataInfo.value != null && e.dataInfo.value != "") {
                        var masterRow = this.dataSet.getTable(0).data.items[0];
                        masterRow.set("PRODUCTCONTRACTNO", e.dataInfo.dataRow.data.CONTRACTNO);
                        this.forms[0].loadRecord(masterRow);
                        for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                            this.dataSet.getTable(1).data.items[i].set("PRODUCTCONTRACTNO", e.dataInfo.dataRow.data.CONTRACTNO);
                        }
                    }
                }
                if (e.dataInfo.fieldName == "PRODUCTCONTRACTNO" ) {
                    var contractNo = Ext.getCmp("PRODUCTCONTRACTNO0_" + this.winId).rawValue;
                    for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                        this.dataSet.getTable(1).data.items[i].set("PRODUCTCONTRACTNO", contractNo);
                    }
                }
                if (e.dataInfo.fieldName == "BATCHNO") {
                    var batchNo = Ext.getCmp("BATCHNO0_" + this.winId).rawValue;
                    for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                        this.dataSet.getTable(1).data.items[i].set("BATCHNO", batchNo);
                    }
                }
            }
            if (e.dataInfo.tableIndex == 1) {
                var ScmMoneyBcf = {}; //金额、数量换算
                ScmMoneyBcf.DealsQuantity = e.dataInfo.dataRow.get("DEALQUANTITY");//交易数量
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
                if (e.dataInfo.fieldName == "STKUNITID") {
                    if (e.dataInfo.value.length > 0) {
                        e.dataInfo.dataRow.set("STKUNITNO", "");//设交易单位标识为空
                        var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.value, e.dataInfo.dataRow.data["STKUNITNO"], 0, e.dataInfo.dataRow.get("DEALQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 0]);
                        e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                    }
                }
                //交易单位标识
                if (e.dataInfo.fieldName == "STKUNITNO") {
                    var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.data["STKUNITID"], e.dataInfo.value, 0, e.dataInfo.dataRow.get("DEALQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 0]);
                    e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                }
                if (e.dataInfo.fieldName == "QUANTITY") {
                    if (e.dataInfo.value >= 0) {
                        debugger;
                        var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("STKUNITID"), e.dataInfo.dataRow.get("STKUNITNO"), e.dataInfo.value, e.dataInfo.dataRow.get("DEALQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 1]);
                        e.dataInfo.dataRow.set("DEALQUANTITY", unitData.ConverQuantity);
                        if (unitData.ErrorType == 1) {
                            alert("通过物料，基础单位，交易单位无法在对应物料表中找到明细！");
                        }
                        else if (unitData.ErrorType == 2) {
                            alert("物料明细表中启动了浮动，数量超出范围！");
                        }

                        ////交易数量改变重新计算
                        //对比最小批量返回新数量
                        var infoList = this.invorkBcf('GetQuantity', [e.dataInfo.dataRow.data["DEALQUANTITY"], e.dataInfo.dataRow.data["MATERIALID"]]);
                        var info = infoList[0];
                        if (e.dataInfo.dataRow.data["DEALQUANTITY"] != info.DEALSQUANTITY) {
                            Ext.Msg.alert("提示", "物料" + e.dataInfo.dataRow.data["MATERIALNAME"] + "的采购最小批量是" + info.PURCHASEQTY + "，最小批量倍数是" + info.BATCHTIMES + " ，数量" + e.dataInfo.dataRow.data["DEALQUANTITY"] + "不符合采购标准，系统会讲数量更改为" + info.DEALSQUANTITY + "，请知悉！");
                            e.dataInfo.dataRow.set("DEALQUANTITY", info.DEALSQUANTITY);
                        }
                        //交易数量变更引起的其它字段的变更
                        ScmMoneyBcf.DealsQuantity = info.DEALSQUANTITY;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeDealsQuantity]);
                        getStkPurStockInPurchaseOrder.call(this, e, data);

                    }
                }

                if (e.dataInfo.fieldName == "DEALQUANTITY") {
                    if (e.dataInfo.value >= 0) {
                        //对比最小批量返回新数量、最小批量、最小批量倍数
                        var infoList = this.invorkBcf('GetQuantity', [e.dataInfo.value, e.dataInfo.dataRow.data["MATERIALID"]]);
                        var info = infoList[0];
                        if (e.dataInfo.value != info.DEALSQUANTITY) {
                            Ext.Msg.alert("提示", "物料" + e.dataInfo.dataRow.data["MATERIALNAME"] + "的采购最小批量是" + info.PURCHASEQTY + "，最小批量倍数是" + info.BATCHTIMES + " ，数量" + e.dataInfo.value + "不符合采购标准，系统会讲数量更改为" + info.DEALSQUANTITY + "，请知悉！");
                            Ext.getCmp('DEALQUANTITY1_' + this.winId).setValue(info.DEALSQUANTITY);
                        }
                        //金额变更引起的其它字段的变更
                        ScmMoneyBcf.DealsQuantity = info.DEALSQUANTITY;
                        var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeDealsQuantity]);
                        getStkPurStockInPurchaseOrder.call(this, e, data);

                        //交易数量变更引发数量变化
                        var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("STKUNITID"), e.dataInfo.dataRow.get("STKUNITNO"), 0, info.DEALSQUANTITY, e.dataInfo.dataRow.get("UNITID"), 0]);
                        e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);

                    }
                }
                //修改实收数量，计算未入库数=应收数量-实收数量
                if (e.dataInfo.fieldName == "QUANTITY") {
                    if (e.dataInfo.dataRow.get("RECEIVABLEQUANTITY") - e.dataInfo.value < 0) {
                        e.dataInfo.dataRow.set("NOTSTOCKQUANTITY", 0);
                    }
                    else {
                        e.dataInfo.dataRow.set("NOTSTOCKQUANTITY", e.dataInfo.dataRow.get("RECEIVABLEQUANTITY") - e.dataInfo.value);
                    }
                }
                //修改实收交易数量，计算未入库数=应收数量-实收数量
                if (e.dataInfo.fieldName == "DEALQUANTITY") {
                    if (e.dataInfo.dataRow.get("RECEIVABLEQUANTITY") - e.dataInfo.dataRow.get("QUANTITY") < 0) {
                        e.dataInfo.dataRow.set("NOTSTOCKQUANTITY", 0);
                    }
                    else {
                        e.dataInfo.dataRow.set("NOTSTOCKQUANTITY", e.dataInfo.dataRow.get("RECEIVABLEQUANTITY") - e.dataInfo.dataRow.get("QUANTITY"));
                    }
                }
            }
            if (e.dataInfo && e.dataInfo.tableIndex == 2) {
                debugger;
                if (e.dataInfo.value != null) {
                    switch (e.dataInfo.fieldName) {
                        case 'SQUANTITY'://仓储数量改变
                            //单位换算引发数量变化
                            var unitData = this.invorkBcf("GetDatas", [e.dataInfo.curGrid.parentRow.get("MATERIALID"), e.dataInfo.dataRow.get("STKUNITID"), e.dataInfo.dataRow.get("STKUNITNO"), 0, e.dataInfo.value, e.dataInfo.curGrid.parentRow.get("UNITID"), 0]);
                            e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                            break;
                        case 'QUANTITY'://基本数量改变
                            //单位换算引发数量变化
                            var unitData = this.invorkBcf("GetDatas", [e.dataInfo.curGrid.parentRow.get("MATERIALID"), e.dataInfo.dataRow.get("STKUNITID"), e.dataInfo.dataRow.get("STKUNITNO"), e.dataInfo.value, 0, e.dataInfo.curGrid.parentRow.get("UNITID"), 1]);
                            e.dataInfo.dataRow.set("SQUANTITY", unitData.ConverQuantity);
                            if (unitData.ErrorType == 1) {
                                alert("通过物料，基础单位，交易单位无法在对应物料表中找到明细！");
                            }
                            else if (unitData.ErrorType == 2) {
                                alert("物料明细表中启动了浮动，数量超出范围！");
                            }
                            break;
                        case 'STKUNITNO'://修改仓储标识
                            var unitData = this.invorkBcf("GetDatas", [e.dataInfo.curGrid.parentRow.get("MATERIALID"), e.dataInfo.dataRow.get("STKUNITID"), e.dataInfo.value, 0, e.dataInfo.dataRow.get("SQUANTITY"), e.dataInfo.curGrid.parentRow.get("UNITID"), 0]);
                            e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                            break;
                        case 'STKUNITID'://修改仓储单位
                            if (e.dataInfo.value != e.dataInfo.dataRow.data["STKUNITID"]) {
                                e.dataInfo.dataRow.set("STKUNITNO", null);
                                e.dataInfo.dataRow.set("SQUANTITY", 1);
                                e.dataInfo.dataRow.set("QUANTITY", 1);

                            }
                            break;
                    }
                }
            }
            if (e.dataInfo && e.dataInfo.tableIndex == 0) {
                var contactobjectid = e.dataInfo.dataRow.get('CONTACTOBJECTID');
                var contactobjectname = e.dataInfo.dataRow.get('CONTACTSOBJECTNAME');
                if (e.dataInfo.value != null) {
                    if (e.dataInfo.fieldName == "CONTACTOBJECTID") {
                        //填充子表往来单位
                        for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                            this.dataSet.getTable(1).data.items[i].set("CONTACTOBJECTID", contactobjectid);
                            this.dataSet.getTable(1).data.items[i].set("CONTACTSOBJECTNAME", contactobjectname);
                        }
                        //填充表头财务信息
                        //var contactobjectData = this.invorkBcf("GetContactData", [contactobjectid]);
                        //this.dataSet.getTable(0).data.items[0].set("INVOICETYPENAME", contactobjectData.InvoiceTypeName);
                        //this.dataSet.getTable(0).data.items[0].set("TAXRATE", contactobjectData.TaxRate);
                        //this.dataSet.getTable(0).data.items[0].set("CURRENCYNAME", contactobjectData.CurrencyName);
                        //this.dataSet.getTable(0).data.items[0].set("STANDARDCOILRATE", contactobjectData.StandardcoilRate);
                        //this.dataSet.getTable(0).data.items[0].set("PAYMENTTYPENAME", contactobjectData.PaymenttypeName);
                        //this.forms[0].loadRecord(this.dataSet.getTable(0).data.items[0]);
                    }
                }
            }
            if (e.dataInfo && e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.value != null) {
                    switch (e.dataInfo.fieldName) {
                        case 'MATERIALID'://物料
                            var endTimeOfQuality = this.invorkBcf('GetEndTimeOfQuality', [e.dataInfo.value, e.dataInfo.dataRow.data["STARTTIMEOFQUALITY"]]);
                            e.dataInfo.dataRow.set("ENDTIMEOFQUALITY", endTimeOfQuality);
                            break;
                        case 'STARTTIMEOFQUALITY'://保质起始日期
                            var endTimeOfQuality = this.invorkBcf('GetEndTimeOfQuality', [e.dataInfo.dataRow.data["MATERIALID"], e.dataInfo.value]);
                            e.dataInfo.dataRow.set("ENDTIMEOFQUALITY", endTimeOfQuality);
                            break;
                    }
                }
            }
            break;
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.tableIndex == 1) {
                bodyRow = e.dataInfo.dataRow;
            }
            if (e.dataInfo.fieldName == "ATTRIBUTENAME") {
                var MaterialId = e.dataInfo.dataRow.data["MATERIALID"];
                var AttributeId = e.dataInfo.dataRow.data["ATTRIBUTEID"];
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
            break;
        case LibEventTypeEnum.FormClosed:
            if (bodyRow != undefined) {
                if (bodyRow.data["QUANTITY"] == 0 || bodyRow.data["QUANTITY"] == "") {
                    var detailTable = this.dataSet.getTable(1);
                    var unitTable = this.dataSet.getTable(2);
                    var count = 0;
                    for (var i = 0 ; i < unitTable.data.length; i++) {
                        count += unitTable.data.items[i].data["QUANTITY"];
                    }
                    bodyRow.set('QUANTITY', count);
                }
            }

            break;
        case LibEventTypeEnum.AddRow:
            if (e.dataInfo.tableIndex == 1) {
                var masterRow = this.dataSet.getTable(0).data.items[0];
                this.forms[0].updateRecord(masterRow);
                var contactobjectid = masterRow.get('CONTACTOBJECTID');
                var contactobjectname = masterRow.get('CONTACTSOBJECTNAME');
                var BatchNo = masterRow.get('BATCHNO');
                e.dataInfo.dataRow.set("CONTACTOBJECTID", contactobjectid);
                e.dataInfo.dataRow.set("CONTACTSOBJECTNAME", contactobjectname);
                e.dataInfo.dataRow.set("PRODUCTCONTRACTNO", this.dataSet.getTable(0).data.items[0].data["PRODUCTCONTRACTNO"]);
                e.dataInfo.dataRow.set("BATCHNO", BatchNo);
            }
            break;
    }
}

function getStkPurStockInPurchaseOrder(e, returnData) {
    e.dataInfo.dataRow.set("DEALQUANTITY", returnData["DealsQuantity"]); //交易数量
    //e.dataInfo.dataRow.set("TAXRATE", returnData["TaxRate"]); //税率
    //e.dataInfo.dataRow.set("TAXPRICE", returnData["TaxPrice"]); //含税单价
    //e.dataInfo.dataRow.set("PRICE", returnData["Price"]); //单价
    //e.dataInfo.dataRow.set("TAXES", returnData["Taxes"]); //税额
    //e.dataInfo.dataRow.set("AMOUNT", returnData["Amount"]); //金额
    //e.dataInfo.dataRow.set("TAXAMOUNT", returnData["TaxAmount"]); //含税金额
    //e.dataInfo.dataRow.set("BWAMOUNT", returnData["BWAmount"]); //本币金额
    //e.dataInfo.dataRow.set("BWTAXAMOUNT", returnData["BWTaxAmount"]); //本币含税金额
    //e.dataInfo.dataRow.set("BWTAXES", returnData["BWTaxes"]); //本币税额
};
//填充当前物料信息
function FillData(list) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据   
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set("FROMBILLNO", info.Frombillno);
                newRow.set("FROMROWID", info.Fromrowid);
                newRow.set("MATERIALID", info.Materialid);
                newRow.set("MATERIALNAME", info.Materialname);
                newRow.set("MATERIALTYPEID", info.Materialtypeid);
                newRow.set("MATERIALTYPENAME", info.Materialtypename);
                newRow.set("AMOUNT", info.Amount);
                newRow.set("PRICE", info.Price);
                newRow.set("TAXRATE", info.Taxrate);
                newRow.set("UNITPRICE", info.Unitprice)
                newRow.set("UNITAMOUNT", info.Unitamount);
                newRow.set("TAXAMOUNT", info.Taxamount);
                newRow.set("NEEDCHECK", info.Needcheck);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

//填充当前行特征信息
function FillDataRow(e, This, CodeDesc) {
    e.dataInfo.dataRow.set("ATTRIBUTECODE", CodeDesc.Code);
    e.dataInfo.dataRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
    //e.dataInfo.dataRow.set("ABNORMALDAY", CodeDesc.AbnormalDay);
    //设置异常天数
    //var masterRow = This.dataSet.getTable(0).data.items[0];
    //Ext.getCmp("ABNORMALDAY0_" + This.winId).setValue(CodeDesc.AbnormalDay);
    return true;
}

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
    if (returnData.length == 0) {
        Ext.Msg.alert("提示", '该产品不存在特征或特征无法获取！');
        return;
    }
    for (var i = 0; i < returnData.length; i++) {
        if (returnData[i].Dynamic) {
            if (returnData[i].Standard) {
                unstandard.push(CreatTextBox(returnData[i]));
            }
            else {
                standard.push(CreatTextBox(returnData[i]));
            }
        }
        else {
            if (returnData[i].Standard) {
                unstandard.push(CreatComBox(returnData[i]));
            }
            else {
                standard.push(CreatComBox(returnData[i]));
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
    var btnConfirm = new Ext.Button({
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
                    if (attPanel.items.items[i].value == null) {
                        msg += '【' + attPanel.items.items[i].fieldLabel + '】';
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
            }
            if (yes) {
                thisWin.close();
            }

        }
    })
    //取消按钮
    var btnCancel = new Ext.Button({
        width: 200,
        height: 35,
        text: "取消",
        type: 'submit',
        handler: function () {
            Ext.getCmp("attWin" + BillNo + Row_Id + MaterialId).close();
        }
    })
    //按钮Panle
    var btnPanel = new Ext.form.Panel({
        layout: 'column',
        width: '100%',
        defaults: {
            margin: '10 40 0 40',
            columnWidth: .5
        },
        items: [btnConfirm, btnCancel]
    })

    var win = new Ext.create('Ext.window.Window', {
        id: "attWin" + BillNo + Row_Id + MaterialId,
        title: '特征信息',
        resizable: false,
        //closeAction: "close",
        modal: true,
        width: 600,
        height: 330,
        materialId: MaterialId,
        attributeId: AttributeId,
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
        }, btnPanel],
    });
    attId++;
    win.show();
    win.items.items[1].collapse(true);
}

//非动态特征 combox
function CreatComBox(attData) {

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
        attId: attData.AttributeItemId,
        valueField: 'AttrCode',
        fields: ['AttrCode', 'AttrValue'],
        store: Store,
        value: attData.DefaultValue,
        //editable: true,
        //text: attData.DefaultValue,
        margin: '5 10 5 10',
        columnWidth: .5,
        labelWidth: 60,
    });
    return combox;
}

//动态特征 NumberField
function CreatTextBox(attData) {
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

//重置按钮
function ResetPanel(returnData, e, This) {

    for (var i = 0; i < returnData.length ; i++) {
        var AttributeCode = "";
        var AttributeDesc = "";
        if (returnData[i]["AttributeCode"] != undefined) {
            AttributeCode = returnData[i]["AttributeCode"];
            AttributeDesc = returnData[i]["AttributeDesc"];
        }

        var thisRow = {
            AttributeId: returnData[i]["AttributeId"],
            MaterialId: returnData[i]['MaterialId'],
            MaterialtypeName: returnData[i]['MaterialtypeName'],
            MaterialtypeId: returnData[i]['MaterialtypeId'],
            MaterialName: returnData[i]['MaterialName'],
            BillNo: e.dataInfo.dataRow.data["BILLNO"],
            RowId: e.dataInfo.dataRow.data["ROW_ID"],
            Quantity: returnData[i]['Quantity'],
            IsNotAdd: true,
            AttributeCode: AttributeCode,
            AttributeDesc: AttributeDesc,

        }
        var panel = AddPanel(thisRow, e, This)
        newPanel.add(panel);
    }
}
