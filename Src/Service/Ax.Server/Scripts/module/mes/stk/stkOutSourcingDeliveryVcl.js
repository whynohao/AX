stkOutSourcingDeliveryVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = stkOutSourcingDeliveryVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = stkOutSourcingDeliveryVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (this.isEdit) {
                if (e.dataInfo.fieldName == "BtnGetData") {
                    Ax.utils.LibVclSystemUtils.openDataFunc('stk.OutSourcingDeliveryDataFunc', '载入来源单', [this]);
                }
                else if (e.dataInfo.fieldName == "BtnCreateDelivery") {
                    var bodyTable = this.dataSet.getTable[1].data.items;
                    this.forms[0].loadRecord(masterRow);
                    var backData;
                    
                   
                    var wareHouse = this.dataSet.getTable(0).data.items[0].data["WAREHOUSEID"];//仓库
                    var contactObjectId = this.dataSet.getTable(0).data.items[0].data["CONTACTSOBJECTID"];//往来单位ID
                    var fromTypeId = this.dataSet.getTable(0).data.items[0].data["TYPEID"];
                    var currencyId = this.dataSet.getTable(0).data.items[0].data["CURRENCYID"];
                    var paymentTypeId = this.dataSet.getTable(0).data.items[0].data["PAYMENTTYPEID"];//结算方式
                    var invoiceTypeId = this.dataSet.getTable(0).data.items[0].data["INVOICETYPEID"];//发票类型
                    var currentState = this.dataSet.getTable(0).data.items[0].data["CURRENTSTATE"];
                    var productOrder = this.dataSet.getTable(0).data.items[0].data["PRODUCTORDER"];
                    if (contactObjectId == "" || contactObjectId == undefined) {
                        Ext.Msg.alert("提示", "往来单位不能为空！");
                    }
                        //else if (fromBillNo == "" || fromBillNo == undefined) {
                        //    Ext.Msg.alert("提示", "单据编号不能为空！");

                        //}
                    else if (currentState != "2") {
                        Ext.Msg.alert("提示", "单据未生效！");
                    }
                    else if (auditState != "2") {
                        Ext.Msg.alert("提示", "单据未审核通过！");
                    }

                    else {
                        var records = [];
                        for (var i = 0; i < bodyTable.length; i++) {
                            var row=bodyTable[i].data;
                            records.push({
                                CONTACTOBJECTID: contactObjectId,
                                CURRENCYID: this.dataSet.getTable(0).data.items[0].data["CURRENCYID"],
                                INVOICETYPEID: this.dataSet.getTable(0).data.items[0].data["INVOICETYPEID"],
                                PAYMENTTYPEID: this.dataSet.getTable(0).data.items[0].data["PAYMENTTYPEID"],
                                WAREHOUSEID: this.dataSet.getTable(0).data.items[0].data["WAREHOUSEID"],
                                PERSONID: this.dataSet.getTable(0).data.items[0].data["WAREHOUSEPERSONID"],
                                CONTRACTCODE: this.dataSet.getTable(0).data.items[0].data["PRODUCTORDER"],
                                TASKNO: row["TASKNO"],
                                FROMBILLNO: row["BILLNO"],
                                FROMROWID: row["ROW_ID"],
                                MATERIALID: row["MATERIALID"],
                                BATCHNO: row["BATCHNO"],
                                SUBBATCHNO: row["SUBBATCHNO"],
                                COMPLETENO: row["COMPLETENO"],
                                MTONO: row["MTONO"],
                                DEALQUANTITY: row["RECEIVEQTY"],
                                STKUNITID: row["DEALSUNITID"],
                                STKUNITNO: row["DEALSUNITNO"],
                                QUANTITY: row["QUANTITY"],
                                PRICE: row["PRICE"],
                                AMOUNT: row["AMOUNT"],
                                TAXRATE: row["TAXRATE"],
                                TAXPRICE: row["TAXPRICE"],
                                TAXAMOUNT: row["TAXAMOUNT"],
                                TAXES: row["TAXES"],
                                BWAMOUNT: row["BWAMOUNT"],
                                BWTAXAMOUNT: row["BWTAXAMOUNT"],
                                BWTAXES: row["BWTAXES"]
                            });
                        }
                        backData = this.invorkBcf('BuildOutSourcingStockIn', [records]);
                    }

                    if (backData != null) {
                        var curPks = [];
                        var isExist = backData.IsExist;
                        curPks.push(backData.BillNo);

                        //var typeId = backData.TypeId;
                        //var entryParam = '{"ParamStore":{"TYPEID":"' + typeId + '"}}';
                        if (isExist) {
                            Ext.Msg.show({
                                title: '提示',
                                msg: '已存在相应单据【' + backData.BillNo + '】，是否打开单据？',
                                buttons: Ext.Msg.YESNO,
                                icon: Ext.Msg.QUESTION,
                                fn: function (h) {
                                    if (h == "yes") {
                                        Ax.utils.LibVclSystemUtils.openBill('stk.PurStockIn', BillTypeEnum.Bill, "外购入库单", BillActionEnum.Browse, undefined, curPks);
                                    }
                                }
                            })
                        }
                        else {
                            Ax.utils.LibVclSystemUtils.openBill('stk.PurStockIn', BillTypeEnum.Bill, "外购入库单", BillActionEnum.Browse, undefined, curPks);
                        }
                    }

                }
                else {
                    Ext.Msg.alert("提示", "单据只有在修改状态才能载入数据！");
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "STANDARDCOILRATE" && e.dataInfo.value) {
                    var items = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < items.length; i++) {
                        var ScmMoneyBcf = {}; //金额、数量换算
                        ScmMoneyBcf.DealsQuantity = this.dataSet.getTable(1).data.items[i].get("DEALSUNITNO");//交易数量
                        ScmMoneyBcf.TaxRate = this.dataSet.getTable(1).data.items[i].get("TAXRATE"); //税率;
                        ScmMoneyBcf.Price = this.dataSet.getTable(1).data.items[i].get("PRICE"); //单价
                        ScmMoneyBcf.TaxPrice = this.dataSet.getTable(1).data.items[i].get("TAXPRICE"); //含税单价
                        ScmMoneyBcf.Amount = this.dataSet.getTable(1).data.items[i].get("AMOUNT"); //金额
                        ScmMoneyBcf.TaxAmount = this.dataSet.getTable(1).data.items[i].get("TAXAMOUNT"); //含税金额
                        ScmMoneyBcf.Taxes = this.dataSet.getTable(1).data.items[i].get("TAXES"); //含税金额
                        ScmMoneyBcf.BWAmount = this.dataSet.getTable(1).data.items[i].get("BWAMOUNT"); //本位币金额
                        ScmMoneyBcf.BWTaxAmount = this.dataSet.getTable(1).data.items[i].get("BWTAXAMOUNT"); //本位币含税金额
                        ScmMoneyBcf.BWTaxes = this.dataSet.getTable(1).data.items[i].get("BWTAXES"); //本位币税额                      

                        //汇率变更引起的其它字段的变更
                        ScmMoneyBcf.StandardcoilRate = e.dataInfo.value;//汇率
                        var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeStandardcoilRate]);
                        if (data != null) {
                            this.dataSet.getTable(1).data.items[i].set("BWAMOUNT", data["BWAmount"]);//本币金额
                            this.dataSet.getTable(1).data.items[i].set("BWTAXAMOUNT", data["BWTaxAmount"]);//本币含税金额
                            this.dataSet.getTable(1).data.items[i].set("BWTAXES", data["BWTaxes"]);//本位币税额
                        }
                    }
                    this.forms[0].updateRecord(masterRow);//更新表头
                }
                //更改表头仓库，表身联动
                if (e.dataInfo.fieldName == "WAREHOUSEID") {
                    var bodyTable = this.dataSet.getTable(1).data.items;
                    var masterRow = this.dataSet.getTable(0).data.items[0].data;
                    for (var i = 0; i < bodyTable.length; i++) {
                        var row = bodyTable[i];
                        row.set("WAREHOUSEID", e.dataInfo.value);
                        row.set("WAREHOUSENAME", masterRow["WAREHOUSENAME"]);
                    }
                }
                //更改表头仓管员，表身联动
                if (e.dataInfo.fieldName == "WAREHOUSEPERSONID") {
                    var bodyTable = this.dataSet.getTable(1).data.items;
                    var masterRow = this.dataSet.getTable(0).data.items[0].data;
                    for (var i = 0; i < bodyTable.length; i++) {
                        var row = bodyTable[i];
                        row.set("WAREHOUSEPERSONID", e.dataInfo.value);
                        row.set("WAREHOUSEPERSONNAME", masterRow["WAREHOUSEPERSONNAME"]);
                    }
                }
                if (e.dataInfo.fieldName == "PRODUCTCONTRACTNO") {
                    var bodyTable = this.dataSet.getTable(1).data.items;
                    var masterRow = this.dataSet.getTable(0).data.items[0].data;
                    for (var i = 0; i < bodyTable.length; i++) {
                        var row = bodyTable[i];
                        row.set("CONTRACTCODE", e.dataInfo.value);
                        row.set("CONTRACTNO", masterRow["CONTRACTNO"]);
                    }
                }
            }
            else if (e.dataInfo.tableIndex == 1) {
                this.forms[0].updateRecord(masterRow);
                //金额、数量换算
                var ScmMoneyBcf = {};
                //交易数量
                ScmMoneyBcf.DealsQuantity = e.dataInfo.dataRow.get("DEALSUNITNO") - e.dataInfo.dataRow.get("REJECTIONQTY");
                //税率
                ScmMoneyBcf.TaxRate = e.dataInfo.dataRow.get("TAXRATE");
                //单价
                ScmMoneyBcf.Price = e.dataInfo.dataRow.get("PRICE");
                //含税单价
                ScmMoneyBcf.TaxPrice = e.dataInfo.dataRow.get("TAXPRICE");
                //金额
                ScmMoneyBcf.Amount = e.dataInfo.dataRow.get("AMOUNT");
                //含税金额
                ScmMoneyBcf.TaxAmount = e.dataInfo.dataRow.get("TAXAMOUNT");
                //本位币金额
                ScmMoneyBcf.BWAmount = e.dataInfo.dataRow.get("BWAMOUNT");
                //本位币含税金额
                ScmMoneyBcf.BWTaxAmount = e.dataInfo.dataRow.get("BWTAXAMOUNT");
                //本位币税额  
                ScmMoneyBcf.BWTaxes = e.dataInfo.dataRow.get("BWTAXES");
                //汇率
                ScmMoneyBcf.StandardcoilRate = masterRow.data["STANDARDCOILRATE"];
                //到货数量原值
                ScmMoneyBcf.OldDealsQuantity = e.dataInfo.dataRow.get("DEALSUNITNO");

                //如果动作内容不为空
                if (e.dataInfo.value != null) {
                    switch (e.dataInfo.fieldName) {
                        //改变交易数量
                        case "DEALSUNITNO":
                            ////对比最小批量返回新数量、最小批量、最小批量倍数
                            //var infoList = this.invorkBcf('GetQuantity', [e.dataInfo.value, e.dataInfo.dataRow.data["MATERIALID"]]);
                            //var info = infoList[0];
                            //if (e.dataInfo.value != info.DEALSUNITNO) {
                            //    Ext.Msg.alert("提示", "物料" + e.dataInfo.dataRow.data["MATERIALNAME"] + "的采购最小批量是" + info.PURCHASEQTY + "，最小批量倍数是" + info.BATCHTIMES + " ，数量" + e.dataInfo.value + "不符合采购标准，系统会讲数量更改为" + info.DEALSUNITNO + "，请知悉！");
                            //    Ext.getCmp('DEALSUNITNO1_' + this.winId).setValue(info.DEALSUNITNO);
                            //}

                            //交易数量变更引发基本数量变化
                            ScmMoneyBcf.DealsQuantity = e.dataInfo.value - e.dataInfo.dataRow.get("REJECTIONQTY");
                            ScmMoneyBcf.OldDealsQuantity = e.dataInfo.value;
                            var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO"), 0, e.dataInfo.value, e.dataInfo.dataRow.get("UNITID"), 0]);
                            e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                            //如果交易单位 == 基本单位，基本数量变为交易数量
                            if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("DEALSUNITID") == e.dataInfo.dataRow.get("UNITID")) {
                                e.dataInfo.dataRow.set("QUANTITY", e.dataInfo.value)
                            }
                            //金额换算引发金额、数量的变化
                            var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeDealsQuantity]);
                            //调用下面编写的方法
                            DeliverSetPrice.call(this, e, data);
                            break;
                            //改变物料（选择物料会带出交易单位）
                        case "MATERIALID":
                            if (e.dataInfo.value.length > 0) {
                                var unitData = this.invorkBcf("GetData", [e.dataInfo.value, e.dataInfo.dataRow.data["DEALSUNITID"], e.dataInfo.dataRow.data["DEALSUNITNO"], 0, e.dataInfo.dataRow.get("DEALSUNITNO"), e.dataInfo.dataRow.get("UNITID"), 0]);
                                //交易单位变更引发基本数量变化
                                e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                                //如果交易单位 == 基本单位，基本数量变为交易数量
                                if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("DEALSUNITID") == e.dataInfo.dataRow.get("UNITID")) {
                                    e.dataInfo.dataRow.set("QUANTITY", e.dataInfo.dataRow.get("DEALSUNITNO"))
                                }
                            }
                            break;
                            //改变基本数量（基本数量会变动交易数量）
                        case "QUANTITY":
                            //如果交易单位，物料代码不为空
                            if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("MATERIALID")) {
                                //交易数量
                                var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO"), e.dataInfo.value, e.dataInfo.dataRow.get("DEALSUNITNO"), e.dataInfo.dataRow.get("UNITID"), 1]);
                                //交易数量变化
                                e.dataInfo.dataRow.set("DEALSUNITNO", unitData.ConverQuantity);
                                if (unitData.ErrorType == 1) {
                                    Ext.Msg.alert("提示", "通过物料，基础单位，交易单位无法在对应物料表中找到明细！");
                                }
                                else if (unitData.ErrorType == 2) {
                                    Ext.Msg.alert("提示", "物料明细表中启动了浮动，数量超出范围！");
                                }
                                //如果交易单位 == 基本单位，交易数量变为基本数量
                                if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("DEALSUNITID") == e.dataInfo.dataRow.get("UNITID")) {
                                    e.dataInfo.dataRow.set("DEALSUNITNO", e.dataInfo.value)
                                }
                                //--交易数量改变重新计算
                                ////对比最小批量返回新数量、最小批量、最小批量倍数
                                //var infoList = this.invorkBcf('GetQuantity', [e.dataInfo.dataRow.data["DEALSUNITNO"], e.dataInfo.dataRow.data["MATERIALID"]]);
                                //var info = infoList[0];
                                //if (e.dataInfo.dataRow.data["DEALSUNITNO"] != info.DEALSUNITNO) {
                                //    Ext.Msg.alert("提示", "物料" + e.dataInfo.dataRow.data["MATERIALNAME"] + "的采购最小批量是" + info.PURCHASEQTY + "，最小批量倍数是" + info.BATCHTIMES + " ，数量" + e.dataInfo.dataRow.data["DEALSUNITNO"] + "不符合采购标准，系统会讲数量更改为" + info.DEALSUNITNO + "，请知悉！");
                                //    e.dataInfo.dataRow.set("DEALSUNITNO", info.DEALSUNITNO);
                                //}

                                //金额换算引发金额、数量的变化
                                ScmMoneyBcf.DealsQuantity = e.dataInfo.dataRow.get("DEALSUNITNO") - e.dataInfo.dataRow.get("REJECTIONQTY");
                                ScmMoneyBcf.OldDealsQuantity = e.dataInfo.dataRow.get("DEALSUNITNO");
                                var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeDealsQuantity]);
                                //调用下面编写的方法
                                DeliverSetPrice.call(this, e, data);

                                //获取单位换算比
                                var unitRate = this.invorkBcf("GetUnitRate", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO")]);
                                //复原交易数量
                                e.dataInfo.dataRow.set("DEALSUNITNO", e.dataInfo.value * unitRate);
                            }
                            break;
                            //改变交易单位
                        case "DEALSUNITID":
                            if (e.dataInfo.value.length > 0) {
                                //设交易单位标识为空
                                e.dataInfo.dataRow.set("DEALSUNITNO", "");
                                var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.value, e.dataInfo.dataRow.data["DEALSUNITNO"], 0, e.dataInfo.dataRow.get("DEALSUNITNO"), e.dataInfo.dataRow.get("UNITID"), 0]);
                                //交易单位变更引发基本数量变化
                                e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                                //如果交易单位 == 基本单位，基本数量变为交易数量
                                if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("DEALSUNITID") == e.dataInfo.dataRow.get("UNITID")) {
                                    e.dataInfo.dataRow.set("QUANTITY", e.dataInfo.dataRow.get("DEALSUNITNO"))
                                }
                            }
                            break;
                            //改变交易单位标识
                        case "DEALSUNITNO":
                            var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.data["DEALSUNITID"], e.dataInfo.value, 0, e.dataInfo.dataRow.get("DEALSUNITNO"), e.dataInfo.dataRow.get("UNITID"), 0]);
                            //交易单位标识变更引发基本数量变化
                            e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                            break;
                            //税率
                        case 'TAXRATE':
                            ScmMoneyBcf.TaxRate = e.dataInfo.value;
                            var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxRate]);
                            //调用下面编写的方法
                            DeliverSetPrice.call(this, e, data);
                            break;
                            //供应商（选择供应商会带出默认税率）
                            //case 'CONTACTSOBJECTID':
                            //    ScmMoneyBcf.TaxRate = e.dataInfo.dataRow.get("TAXRATE");
                            //    var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxRate]);
                            //    //调用下面编写的方法
                            //    DeliverSetPrice.call(this, e, data);
                            //    break;
                            //单价
                        case 'PRICE':
                            ScmMoneyBcf.Price = e.dataInfo.value;
                            //ScmMoneyBcf.TaxPrice = e.dataInfo.value * e.dataInfo.dataRow.get("DEALSUNITNO") * ( 1 + e.dataInfo.dataRow.get("TAXRATE")) //含税单价
                            var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangePrice]);
                            //调用下面编写的方法
                            DeliverSetPrice.call(this, e, data);
                            break;
                            //含税单价
                        case 'TAXPRICE':
                            ScmMoneyBcf.TaxPrice = e.dataInfo.value;
                            var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxPrice]);
                            //调用下面编写的方法
                            DeliverSetPrice.call(this, e, data);
                            break;
                            //金额
                        case 'AMOUNT':
                            ScmMoneyBcf.Amount = e.dataInfo.value;
                            var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeAmount]);
                            //调用下面编写的方法
                            DeliverSetPrice.call(this, e, data);
                            break;
                            //含税金额
                        case 'TAXAMOUNT':
                            ScmMoneyBcf.TaxAmount = e.dataInfo.value;
                            var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxAmount]);
                            //调用下面编写的方法
                            DeliverSetPrice.call(this, e, data);
                            break;
                            //本位金额
                        case "BWAMOUNT":
                            if (e.dataInfo.value >= 0) {
                                //本位币金额变更引起的其它字段的变更
                                ScmMoneyBcf.BWAmount = e.dataInfo.value;
                                var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeBWAmount]);
                                DeliverSetPrice.call(this, e, data);
                            }
                            break;
                            //本位含税金额
                        case "BWTAXAMOUNT":
                            if (e.dataInfo.value >= 0) {
                                //本位币含税金额变更引起的其它字段的变更
                                ScmMoneyBcf.BWTaxAmount = e.dataInfo.value;
                                var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeBWTaxAmount]);
                                DeliverSetPrice.call(this, e, data);
                            }
                            break;
                            //拒收数
                        case "REJECTIONQTY":
                            if (e.dataInfo.value >= 0) {
                                //原来的交易数量
                                var oldDealsQuantity = e.dataInfo.dataRow.get("DEALSUNITNO");
                                //拒收数变更引发交易数量变化
                                ScmMoneyBcf.DealsQuantity = oldDealsQuantity - e.dataInfo.value;
                                ScmMoneyBcf.OldDealsQuantity = oldDealsQuantity;
                                //金额换算引发金额、数量的变化
                                var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeDealsQuantity]);
                                //调用下面编写的方法
                                DeliverSetPrice.call(this, e, data);
                            }
                            break;
                    }
                }
            }
            break;
        case LibEventTypeEnum.AddRow:
            var masterRow = this.dataSet.getTable(0).data.items[0].data;
            e.dataInfo.dataRow.set("CONTRACTCODE", masterRow["CONTRACTCODE"]);
            e.dataInfo.dataRow.set("CONTRACTNO", masterRow["CONTRACTNO"]);
            e.dataInfo.dataRow.set("WAREHOUSEID", masterRow["WAREHOUSEID"]);
            e.dataInfo.dataRow.set("WAREHOUSENAME", masterRow["WAREHOUSENAME"]);
            e.dataInfo.dataRow.set("WAREHOUSEPERSONID", masterRow["WAREHOUSEPERSONID"]);
            e.dataInfo.dataRow.set("WAREHOUSEPERSONNAME", masterRow["WAREHOUSEPERSONNAME"]);
            break;
    }
}
function DeliverSetPrice(e, returnData) {
    e.dataInfo.dataRow.set("RECEIVEQTY", returnData["DealsQuantity"]); //交易数量
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
//改变的字段名种类的枚举
var ChangeTypeEnum =
{
    ChangeDealsQuantity: 1, //改变交易数量
    ChangeTaxRate: 2, //改变税率
    ChangePrice: 3, //改变单价
    ChangeTaxPrice: 4, //改变含税单价
    ChangeAmount: 5, //改变金额
    ChangeTaxAmount: 6, //改变含税金额
    ChangeTaxes: 7,//改变税额
    ChangeStandardcoilRate: 8,//改变汇率
    ChangeBWAmount: 9,//改变本币金额
    ChangeBWTaxAmount: 10//改变本币含税金额
};