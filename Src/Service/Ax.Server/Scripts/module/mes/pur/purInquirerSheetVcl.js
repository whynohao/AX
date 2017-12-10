purInquirerSheetVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = purInquirerSheetVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = purInquirerSheetVcl;

//交易数量（用于子子表计算）
var dealsQty = 0;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    //表头
    var masterRow = this.dataSet.getTable(0).data.items[0];
    //表身
    var allBodyRow = this.dataSet.getTable(1).data.items;
    //点击按钮事件
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        //“数据加载”按钮
        if (e.dataInfo.fieldName == "BtnGetSaleInquiry") {
            if (this.isEdit) {
                Ax.utils.LibVclSystemUtils.openDataFunc("pur.InquirerSheetDataFunc", "取销售询价单", [this, "PURINQUIRERSHEETDATAFUNCDETAIL"]);
            }
            else {
                Ext.Msg.alert("系统提示", "编辑状态才能使用数据加载按钮！");
            }
        }
    }
    //其它事件（编辑状态）
    if (this.isEdit) {
        //新增行赋值（后加逻辑）
        if (e.libEventType == LibEventTypeEnum.AddRow) {
            //表身
            if (e.dataInfo.tableIndex == 1) {
                /*----预计到货日期默认为表头需求日期---*/
                this.forms[0].updateRecord(masterRow);
                //需求日期
                var demandDate = masterRow.get("DEMANDDATE");
                //预计到货日期
                e.dataInfo.dataRow.set("PREPAREDATE", demandDate);
                /*-------------------------------------*/
            }
        }
        //表头
        if (e.dataInfo && e.dataInfo.tableIndex == 0) {
            //修改数据ing
            if (e.libEventType == LibEventTypeEnum.Validating) {
                //修改需求日期自动修改表身行项预计到货日期
                if (e.dataInfo.fieldName == "DEMANDDATE") {
                    //需求日期
                    var demandDate = e.dataInfo.value;
                    //如果需求日期不为空
                    if (demandDate) {
                        //判断需求日期是否大于等于当前日期
                        var isBiggerThanNow = CompareDate(demandDate);
                        if (!isBiggerThanNow) {
                            Ext.Msg.alert("提示", '需求日期不可早于当前日期');
                            e.dataInfo.cancel = true;
                            return;
                        }
                        ////如果表身有行项
                        //if (this.dataSet.getTable(1).data.length > 0) {
                        //    for (var i = 0; i < this.dataSet.getTable(1).data.length; i++) {
                        //        //预计到货日期
                        //        allBodyRow[i].set("PREPAREDATE", demandDate);
                        //    }
                        //}
                    }
                }
            }
        }
        //数量、金额、单价、税率、税额、含税单价、含税金额、换算单位
        //表身
        if (e.dataInfo && e.dataInfo.tableIndex == 1) {
            //判断子子表是否有打勾项，有打勾则不能修改部分字段
            if (e.libEventType == LibEventTypeEnum.Validating) {
                //子子表
                var curRows = this.dataSet.getTable(2).data.items;
                for (var i = 0; i < curRows.length; i++) {
                    //行标识与子子表父行标识一致
                    if (curRows[i].data["PARENTROWID"] == e.dataInfo.dataRow.get("ROW_ID")) {
                        //有勾选项
                        if (curRows[i].data["ISSELECTED"] == true) {
                            if (e.dataInfo.fieldName == "TAXRATE" || e.dataInfo.fieldName == "DEALSQUANTITY" || e.dataInfo.fieldName == "QUANTITY" || e.dataInfo.fieldName == "PRICE" || e.dataInfo.fieldName == "TAXPRICE" || e.dataInfo.fieldName == "AMOUNT" || e.dataInfo.fieldName == "TAXAMOUNT") {
                                Ext.Msg.alert("提示", "采购询价单子子表（询价详情）中勾选确认了一项询价结果，无法修改部分字段的值，如需更改请将详情中的勾选去除");
                                e.dataInfo.cancel = true;
                                return;
                            }
                            if (e.dataInfo.fieldName == "CONTACTSOBJECTID") {
                                Ext.Msg.alert("提示", "采购询价单子子表（询价详情）中勾选确认了一项询价结果，无法修改部分字段的值，如需更改请将详情中的勾选去除");
                                e.dataInfo.dataRow.set("CONTACTSOBJECTID", e.dataInfo.oldValue);
                                e.dataInfo.cancel = true;
                                return;
                            }
                        }
                    }
                }

                //选择了物料
                if (e.dataInfo.fieldName == "MATERIALID" && e.dataInfo.value != "") {
                    /*---默认带出物料明细中的默认辅助单位------*/
                    var returnData = this.invorkBcf('GetUnitJson', [e.dataInfo.value]);
                    var list = returnData;//一般是中间层返回来的数据
                    if (list != undefined && list.length > 0) {
                        var info = list[0];
                        e.dataInfo.dataRow.set("DEALSUNITID", info.UNITID);
                        e.dataInfo.dataRow.set("DEALSUNITNO", info.UNITNO);
                        e.dataInfo.dataRow.set("DEALSUNITNAME", info.UNITNAME);
                    }
                    /*-------------------------------------*/
                }
                    //物料变为空后此三项也清空
                else if (e.dataInfo.fieldName == "MATERIALID" && e.dataInfo.value == "") {
                    e.dataInfo.dataRow.set("DEALSUNITID", "");
                    e.dataInfo.dataRow.set("DEALSUNITNO", "");
                    e.dataInfo.dataRow.set("DEALSUNITNAME", "");
                }
            }
            //修改后
            if (e.libEventType == LibEventTypeEnum.Validated) {
                this.forms[0].updateRecord(masterRow);
                //金额、数量换算
                var ScmMoneyBcf = {}; 
                //交易数量
                ScmMoneyBcf.DealsQuantity = e.dataInfo.dataRow.get("DEALSQUANTITY");
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

                //如果动作内容不为空
                if (e.dataInfo.value != null) {
                    switch (e.dataInfo.fieldName) {
                        //改变交易数量
                        case "DEALSQUANTITY":
                            ScmMoneyBcf.DealsQuantity = e.dataInfo.value;
                            //交易数量变更引发基本数量变化
                            var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO"), 0, e.dataInfo.value, e.dataInfo.dataRow.get("UNITID"), 0]);
                            e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                            //如果交易单位 == 基本单位，基本数量变为交易数量
                            if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("DEALSUNITID") == e.dataInfo.dataRow.get("UNITID")) {
                                e.dataInfo.dataRow.set("QUANTITY", e.dataInfo.value)
                            }
                            //金额换算引发金额、数量的变化
                            var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeDealsQuantity]);
                            //调用下面编写的方法
                            getPurchaseOrder.call(this, e, data);
                            break;
                        //改变物料（选择物料会带出交易单位）
                        case "MATERIALID":
                            if (e.dataInfo.value.length > 0) {
                                var unitData = this.invorkBcf("GetData", [e.dataInfo.value, e.dataInfo.dataRow.data["DEALSUNITID"], e.dataInfo.dataRow.data["DEALSUNITNO"], 0, e.dataInfo.dataRow.get("DEALSQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 0]);
                                //交易单位变更引发基本数量变化
                                e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                                //如果交易单位 == 基本单位，基本数量变为交易数量
                                if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("DEALSUNITID") == e.dataInfo.dataRow.get("UNITID")) {
                                    e.dataInfo.dataRow.set("QUANTITY", e.dataInfo.dataRow.get("DEALSQUANTITY"))
                                }
                            }
                            break;
                        //改变基本数量（基本数量会变动交易数量）
                        case "QUANTITY":
                            //如果交易单位，物料代码不为空
                            if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("MATERIALID")) {
                                //交易数量
                                var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO"), e.dataInfo.value, e.dataInfo.dataRow.get("DEALSQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 1]);
                                //交易数量变化
                                e.dataInfo.dataRow.set("DEALSQUANTITY", unitData.ConverQuantity);
                                //如果交易单位 == 基本单位，交易数量变为基本数量
                                if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("DEALSUNITID") == e.dataInfo.dataRow.get("UNITID")) {
                                    e.dataInfo.dataRow.set("DEALSQUANTITY", e.dataInfo.value)
                                }
                                ScmMoneyBcf.DealsQuantity = e.dataInfo.dataRow.get("DEALSQUANTITY");
                                //金额换算引发金额、数量的变化
                                var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeDealsQuantity]);
                                //调用下面编写的方法
                                getPurchaseOrder.call(this, e, data);
                            }
                            break;
                        //改变交易单位
                        case "DEALSUNITID":
                            if (e.dataInfo.value.length > 0) {
                                //设交易单位标识为空
                                e.dataInfo.dataRow.set("DEALSUNITNO", "");
                                var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.value, e.dataInfo.dataRow.data["DEALSUNITNO"], 0, e.dataInfo.dataRow.get("DEALSQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 0]);
                                //交易单位变更引发基本数量变化
                                e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                                //如果交易单位 == 基本单位，基本数量变为交易数量
                                if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("DEALSUNITID") == e.dataInfo.dataRow.get("UNITID")) {
                                    e.dataInfo.dataRow.set("QUANTITY", e.dataInfo.dataRow.get("DEALSQUANTITY"))
                                }
                            }
                            break;
                        //改变交易单位标识
                        case "DEALSUNITNO":
                                var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.data["DEALSUNITID"], e.dataInfo.value, 0, e.dataInfo.dataRow.get("DEALSQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 0]);
                                //交易单位标识变更引发基本数量变化
                                e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                            break;
                        //税率
                        case 'TAXRATE':
                            ScmMoneyBcf.TaxRate = e.dataInfo.value;
                            var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxRate]);
                            //调用下面编写的方法
                            getPurchaseOrder.call(this, e, data);
                            break;
                        //供应商（选择供应商会带出默认税率）
                        case 'CONTACTSOBJECTID':
                            ScmMoneyBcf.TaxRate = e.dataInfo.dataRow.get("TAXRATE");
                            var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxRate]);
                            //调用下面编写的方法
                            getPurchaseOrder.call(this, e, data);
                            break;
                        //单价
                        case 'PRICE': 
                            ScmMoneyBcf.Price = e.dataInfo.value;
                            //ScmMoneyBcf.TaxPrice = e.dataInfo.value * e.dataInfo.dataRow.get("DEALSQUANTITY") * ( 1 + e.dataInfo.dataRow.get("TAXRATE")) //含税单价
                            var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangePrice]);
                            //调用下面编写的方法
                            getPurchaseOrder.call(this, e, data);
                            break;
                        //含税单价
                        case 'TAXPRICE':
                            ScmMoneyBcf.TaxPrice = e.dataInfo.value;
                            var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxPrice]);
                            //调用下面编写的方法
                            getPurchaseOrder.call(this, e, data);
                            break;
                        //金额
                        case 'AMOUNT': 
                            ScmMoneyBcf.Amount = e.dataInfo.value;
                            var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeAmount]);
                            //调用下面编写的方法
                            getPurchaseOrder.call(this, e, data);
                            break;
                        //含税金额
                        case 'TAXAMOUNT':
                            ScmMoneyBcf.TaxAmount = e.dataInfo.value;
                            var data = this.invorkBcf('AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxAmount]);
                            //调用下面编写的方法
                            getPurchaseOrder.call(this, e, data);
                            break;
                    }
                }
            }
            //双击特征名称 → 弹出窗口选择
            if (e.libEventType == LibEventTypeEnum.ColumnDbClick) {
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
                if (e.dataInfo.fieldName == "INQUIRYDETAIL") {
                    //交易数量赋值
                    dealsQty = e.dataInfo.dataRow.data["DEALSQUANTITY"];
                    for (var i = 0; i < this.dataSet.getTable(2).data.items.length  ; i++) {
                        if (e.dataInfo.dataRow.data["ROW_ID"] == this.dataSet.getTable(2).data.items[i].data["PARENTROWID"]) {
                            //单价
                            var price = this.dataSet.getTable(2).data.items[i].data["PRICE"];
                            //含税单价
                            var taxPrice = price * (1 + this.dataSet.getTable(2).data.items[i].data["TAXRATE"]);
                            //含税金额
                            var taxAmount = taxPrice * dealsQty;
                            //税额
                            var taxes = taxAmount - price * dealsQty;

                            //赋值
                            this.dataSet.getTable(2).data.items[i].set("TAXPRICE", taxPrice);
                            this.dataSet.getTable(2).data.items[i].set("TAXAMOUNT", taxAmount);
                            this.dataSet.getTable(2).data.items[i].set("TAXES", taxes);
                        }
                    }
                }
            }
        }
        //子子表
        if (e.dataInfo && e.dataInfo.tableIndex == 2) {
            if (e.libEventType == LibEventTypeEnum.Validating) {
                //选中相同供应商时回退，提示
                if (e.dataInfo.fieldName == "CONTACTSOBJECTID" && e.dataInfo.value != "") {
                    //选择的供应商
                    var contactsObject = e.dataInfo.value;
                    //选择的行标识
                    var rowId = e.dataInfo.dataRow.get("ROW_ID");
                    //子子表
                    var subTable = this.dataSet.getTable(2);
                    for (var i = 0; i < subTable.data.items.length; i++) {
                        if (e.dataInfo.value != "" && subTable.data.items[i].data["CONTACTSOBJECTID"] == contactsObject && rowId != subTable.data.items[i].data["ROW_ID"]) {
                            e.dataInfo.dataRow.data["CONTACTSOBJECTID"] = null;
                            e.dataInfo.dataRow.data["CONTACTSOBJECTNAME"] = null;
                            e.dataInfo.dataRow.data["PAYMENTTYPE"] = null;
                            e.dataInfo.dataRow.data["INVOICETYPEID"] = null;
                            e.dataInfo.dataRow.data["INVOICETYPENAME"] = null;
                            e.dataInfo.cancel = true;
                            //提示
                            Ext.Msg.alert("提示", "询价明细中供应商不能重复");
                            break;
                        }
                    }
                }
                if (e.dataInfo.fieldName == "TAXAMOUNT") {
                    if (dealsQty == 0) {
                        //金额
                        e.dataInfo.dataRow.set("AMOUNT", 0);
                        //含税金额
                        e.dataInfo.dataRow.set("TAXAMOUNT", 0)
                        //税额
                        e.dataInfo.dataRow.set("TAXES", 0);
                        e.dataInfo.cancel = true;
                        //提示
                        Ext.Msg.alert("提示", "交易数量为0，相关金额应为0");
                    }
                }
            }
            //————————————子子表中的金额联动
            if (e.libEventType == LibEventTypeEnum.Validated) {
                //供应商
                if (e.dataInfo.fieldName == "CONTACTSOBJECTID") {
                    //税率
                    var taxRate = e.dataInfo.dataRow.data["TAXRATE"];
                    //单价
                    var price = e.dataInfo.dataRow.data["PRICE"];
                    //含税单价
                    e.dataInfo.dataRow.set("TAXPRICE", price * (taxRate + 1));
                    //含税金额
                    e.dataInfo.dataRow.set("TAXAMOUNT", price * (taxRate + 1) * dealsQty);
                    //税额
                    e.dataInfo.dataRow.set("TAXES", price * (taxRate + 1) * dealsQty - price * dealsQty);
                }
                //税率
                if (e.dataInfo.fieldName == "TAXRATE" && e.dataInfo.value >= 0) {
                    //单价
                    var price = e.dataInfo.dataRow.data["PRICE"];
                    //含税单价
                    e.dataInfo.dataRow.set("TAXPRICE", price * (e.dataInfo.value + 1));
                    //含税金额
                    e.dataInfo.dataRow.set("TAXAMOUNT", price * (e.dataInfo.value + 1) * dealsQty);
                    //税额
                    e.dataInfo.dataRow.set("TAXES", price * (e.dataInfo.value + 1) * dealsQty - dealsQty * price);
                }
                //单价
                if (e.dataInfo.fieldName == "PRICE" && e.dataInfo.value >= 0) {
                    //含税单价
                    e.dataInfo.dataRow.set("TAXPRICE", e.dataInfo.value * (e.dataInfo.dataRow.get("TAXRATE") + 1));
                    //含税金额
                    e.dataInfo.dataRow.set("TAXAMOUNT", e.dataInfo.value * (e.dataInfo.dataRow.get("TAXRATE") + 1) * dealsQty);
                    //税额
                    e.dataInfo.dataRow.set("TAXES", e.dataInfo.value * (e.dataInfo.dataRow.get("TAXRATE") + 1) * dealsQty - dealsQty * e.dataInfo.value);
                }
                //含税单价
                if (e.dataInfo.fieldName == "TAXPRICE" && e.dataInfo.value >= 0) {
                    //单价
                    e.dataInfo.dataRow.set("PRICE", e.dataInfo.value / (e.dataInfo.dataRow.get("TAXRATE") + 1));
                    //含税金额
                    e.dataInfo.dataRow.set("TAXAMOUNT", e.dataInfo.value * dealsQty);
                    //税额
                    e.dataInfo.dataRow.set("TAXES", e.dataInfo.value * dealsQty / (e.dataInfo.dataRow.get("TAXRATE") + 1));
                }
                //含税金额
                if (e.dataInfo.fieldName == "TAXAMOUNT" && e.dataInfo.value >= 0) {
                    if (dealsQty > 0) {
                        //含税单价
                        e.dataInfo.dataRow.set("TAXPRICE", e.dataInfo.value / dealsQty)
                        //单价
                        e.dataInfo.dataRow.set("PRICE", e.dataInfo.value / dealsQty / (1 + e.dataInfo.dataRow.get("TAXRATE")));
                        //金额
                        e.dataInfo.dataRow.set("AMOUNT", e.dataInfo.value / (1 + e.dataInfo.dataRow.get("TAXRATE")));
                        //税额
                        e.dataInfo.dataRow.set("TAXES", e.dataInfo.value - e.dataInfo.value / (1 + e.dataInfo.dataRow.get("TAXRATE")));
                    }
                }
            }
            //关闭之前检查是否是最多打了一个勾
            if (e.libEventType == LibEventTypeEnum.FormClosed) {
                //计数，统计打勾行
                var count = 0;
                var parentRowId;
                var hasError = true;
                //子子表
                var subTable = this.dataSet.getTable(2);
                for (var i = 0; i < subTable.data.items.length; i++) {
                    if (subTable.data.items[i].data["ISSELECTED"] == 1) {
                        count += 1;
                        parentRowId = subTable["ParentIndex"];
                    }
                }
                if (count > 1) {
                    Ext.Msg.alert("提示", "最多只能选中一个供应商，请重新选择！");
                    //清空所有勾
                    for (var i = 0; i < subTable.data.items.length; i++) {
                        //复选框清空只能用set，不能用"...... = 0"
                        //须用set，不能用=
                        subTable.data.items[i].set("ISSELECTED", 0);
                    }
                } else {
                    hasError = false;
                }

                //子子表打勾项回填
                if (!hasError) {
                    var selectItem = this.dataSet.getTable(2).data.items;
                    //数组
                    var records = [];
                    for (var i = 0; i < selectItem.length; i++) {
                        if (selectItem[i].data["ISSELECTED"] == true) {
                            //父行标识
                            var parentRowId = selectItem[i].data["PARENTROWID"];
                            records.push({
                                ContactsObjectId: selectItem[i].data["CONTACTSOBJECTID"],
                                ContactsObjectName: selectItem[i].data["CONTACTSOBJECTNAME"],
                                PaymentTypeId: selectItem[i].data["PAYMENTTYPEID"],
                                PaymentTypeName: selectItem[i].data["PAYMENTTYPENAME"],
                                InvoiceTypeId: selectItem[i].data["INVOICETYPEID"],
                                InvoiceTypeName: selectItem[i].data["INVOICETYPENAME"],
                                TaxRate: selectItem[i].data["TAXRATE"],
                                Price: selectItem[i].data["PRICE"],
                                Amount: selectItem[i].data["PRICE"] * dealsQty,
                                TaxPrice: selectItem[i].data["TAXPRICE"],
                                TaxAmount: selectItem[i].data["TAXAMOUNT"],
                                Taxes: selectItem[i].data["TAXES"],
                            });
                        }
                    }
                    //回填
                    for (var i = 0; i < allBodyRow.length; i++) {
                        if (allBodyRow[i].data["ROW_ID"] == parentRowId) {
                            allBodyRow[i].set("CONTACTSOBJECTID", records[0]["ContactsObjectId"]);
                            allBodyRow[i].set("CONTACTSOBJECTNAME", records[0]["ContactsObjectName"]);
                            allBodyRow[i].set("PAYMENTTYPEID", records[0]["PaymentTypeId"]);
                            allBodyRow[i].set("PAYMENTTYPENAME", records[0]["PaymentTypeName"]);
                            allBodyRow[i].set("INVOICETYPEID", records[0]["InvoiceTypeId"]);
                            allBodyRow[i].set("INVOICETYPENAME", records[0]["InvoiceTypeName"]);
                            allBodyRow[i].set("TAXRATE", records[0]["TaxRate"]);
                            allBodyRow[i].set("PRICE", records[0]["Price"]);
                            allBodyRow[i].set("AMOUNT", records[0]["Amount"]);
                            allBodyRow[i].set("TAXPRICE", records[0]["TaxPrice"]);
                            allBodyRow[i].set("TAXAMOUNT", records[0]["TaxAmount"]);
                            allBodyRow[i].set("TAXES", records[0]["Taxes"]);
                        }
                    }
                }
            }
        }
    }
}
//比较日期
function CompareDate(demandDate) {
    //当前日期
    var currentDate = new Date();
    var stringDate = Ext.Date.format(currentDate, "Ymd");
    var intDate = parseInt(stringDate);
    //如果需求日期 < 当前日期
    if (demandDate < intDate) {
        return false;
    }
    return true;
}
//计算数据的赋值
function getPurchaseOrder(e, returnData) {
    e.dataInfo.dataRow.set("DEALSQUANTITY", returnData["DealsQuantity"]); //交易数量
    e.dataInfo.dataRow.set("TAXRATE", returnData["TaxRate"]); //税率
    e.dataInfo.dataRow.set("TAXPRICE", returnData["TaxPrice"]); //含税单价
    e.dataInfo.dataRow.set("PRICE", returnData["Price"]); //单价
    e.dataInfo.dataRow.set("TAXES", returnData["Taxes"]); //税额
    e.dataInfo.dataRow.set("AMOUNT", returnData["Amount"]); //金额
    e.dataInfo.dataRow.set("TAXAMOUNT", returnData["TaxAmount"]); //含税金额
};
//改变的字段名种类的枚举
var ChangeTypeEnum =
{
    ChangeDealsQuantity: 1, //改变交易数量
    ChangeTaxRate: 2, //改变税率
    ChangePrice: 3, //改变单价
    ChangeTaxPrice: 4, //改变含税单价
    ChangeAmount: 5, //改变金额
    ChangeTaxAmount: 6 //改变含税金额
};
/*复制开始-----------------------------------------------------------------------------------------------------------------------------------------*/
var attId = 0;
//最新特征窗体（双击特征名称弹出的选择窗体）
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

//组合品Form
function FormWin(returnData, e, This) {
    var materialId = e.dataInfo.dataRow.data["MATERIALID"];
    var win = new Ext.create('Ext.window.Window', {
        id: "win" + e.dataInfo.dataRow.data["BILLNO"] + e.dataInfo.dataRow.data["ROW_ID"] + materialId,
        title: '产品表单',
        resizable: false,
        //closeAction: "hide",
        autoScroll: true,
        layout: "vbox",
        modal: true,
        width: 1250,
        height: 350,
        tbar: [
            {
                xtype: 'button', text: '确定', handler: function () {
                    thisWin = Ext.getCmp("win" + e.dataInfo.dataRow.data["BILLNO"] + e.dataInfo.dataRow.data["ROW_ID"] + materialId);
                    var masterRow = This.dataSet.getTable(0).data.items[0];
                    var thisPanel = thisWin.items.items[0];
                    var PanelItem = thisPanel.items.items;
                    var b = true;
                    if (PanelItem.length == 0) {
                        Ext.Msg.alert("提示", '该产品为组合件，请维护！');
                        return false;
                    }
                    for (var i = 0; i < PanelItem.length; i++) {
                        if (PanelItem[i].materialId == "" || PanelItem[i].materialId == undefined) {
                            Ext.Msg.alert("提示", '请维护完整产品！');
                            b = false;
                            break;
                        }
                        else if (PanelItem[i].items.items[3].existAtt) {
                            if (PanelItem[i].attributeCode == "" || PanelItem[i].attributeCode == undefined) {
                                Ext.Msg.alert("提示", '第' + (i + 1) + '行产品存在特征，请双击【特征标识】维护！');
                                b = false;
                                break;
                            }
                        }
                        if (PanelItem[i].quantity < 1) {
                            Ext.Msg.alert("提示", '第' + (i + 1) + '行数量必须大于0！');
                            b = false;
                            break;
                        }
                    }
                    if (b) {
                        var BodyTable = This.dataSet.getTable(2);
                        for (var i = 0; i < BodyTable.data.items.length; i++) {
                            if (BodyTable.data.items[i].data["PARENTROWID"] == e.dataInfo.dataRow.data["ROW_ID"]) {
                                BodyTable.remove(BodyTable.data.items[i]);
                                i--;
                            }
                        }
                        var AbnormalDay = 0;
                        for (var i = 0; i < PanelItem.length; i++) {

                            if (PanelItem[i].day > AbnormalDay) {
                                AbnormalDay = PanelItem[i].day;
                            }
                            var ReturnUnit = This.invorkBcf('GetUnit', [PanelItem[i].materialId]);
                            var UnitId = "";
                            var UnitName = "";

                            if (ReturnUnit.length != 0) {
                                UnitId = ReturnUnit[0].UnitId;
                                UnitName = ReturnUnit[0].UnitName;
                            }
                            var newRow = This.addRow(e.dataInfo.dataRow, 2);
                            newRow.set('MATERIALID', PanelItem[i].materialId);
                            newRow.set('MATERIALNAME', PanelItem[i].materialName);
                            newRow.set('MATERIALTYPEID', PanelItem[i].materialtypeId);
                            newRow.set('MATERIALTYPENAME', PanelItem[i].materialtypeName);
                            newRow.set('UNITID', UnitId);
                            newRow.set('UNITNAME', UnitName);
                            newRow.set('ATTRIBUTECODE', PanelItem[i].attributeCode);
                            newRow.set('ATTRIBUTEDESC', PanelItem[i].attributeDesc);
                            newRow.set('PARENTROWID', e.dataInfo.dataRow.data["ROW_ID"]);
                            newRow.set('QUANTITY', PanelItem[i].quantity);
                            newRow.set('ABNORMALDAY', PanelItem[i].day);

                        }

                        thisWin.close();
                        e.dataInfo.dataRow.set("SALESORDERDETAILSUB", true);
                        e.dataInfo.dataRow.set("ABNORMALDAY", AbnormalDay);
                    }

                }
            },
            {
                xtype: 'button', text: '重置', handler: function () {
                    for (var i = newPanel.items.items.length; i > 0 ; i--) {
                        newPanel.remove(newPanel.items.items[i - 1]);
                    }
                    ResetPanel(returnData, e, This);
                }
            }],
        items: [newPanel],

    });

    win.show();
}

//组合品明细Panel
function AddPanel(thisRow, e, This) {
    var AttributeCode = thisRow.AttributeCode;
    var AttributeDesc = thisRow.AttributeDesc;

    //判断是否存在特征
    var existAtt = true;
    if (thisRow.AttributeId == "") {
        existAtt = false;
    }
    indexid = indexid + 1;
    var testguid = "Panel_" + indexid;
    //新增按钮
    var btnAdd = new Ext.Button({
        margin: '10 5 10 0  ',
        columnWidth: 0.025,
        height: 30,
        text: "+",
        type: 'submit',
        flag: testguid + thisRow.BillNo + thisRow.RowId + indexid + e.dataInfo.dataRow.data["MATERIALID"],
        handler: function () {
            //复制当前行
            var AddRow = {
                AttributeId: "",
                MaterialId: "",
                MaterialtypeName: thisRow.MaterialtypeName,
                MaterialtypeId: thisRow.MaterialtypeId,
                MaterialName: "",
                BillNo: thisRow.BillNo,
                RowId: thisRow.RowId,
                Quantity: 1,
                IsNotAdd: false,
                AttributeCode: "",
                AttributeDesc: ""

            };
            var Apanel = AddPanel(AddRow, e, This);
            newPanel.add(Apanel);
            newPanel.doLayout();
        }
    })

    var id = btnAdd.id.substr(6, btnAdd.id.length - 6);
    //删除按钮
    var btnDel = new Ext.Button(
        {
            margin: '10 5 10 0  ',
            id: "btnDel" + id,
            columnWidth: 0.025,
            height: 30,
            text: "-",
            type: 'submit',
            flag: testguid + thisRow.BillNo + thisRow.RowId + indexid + e.dataInfo.dataRow.data["MATERIALID"],
            handler: function () {
                var panel = Ext.getCmp(this.flag);
                newPanel.remove(panel);
                newPanel.doLayout();
            }
        })

    var materialid = e.dataInfo.dataRow.data["MATERIALID"];
    //panel
    var formPanel = new Ext.form.FieldSet({
        layout: "column",
        id: testguid + thisRow.BillNo + thisRow.RowId + indexid + e.dataInfo.dataRow.data["MATERIALID"],

        margin: '5 0 0 10',
        defaults:
        {
            margin: '10 20 10 0  ',

            border: false,
            layout: 'form'
        },
        width: 1200,
        height: 50,
        items:
            [btnAdd, btnDel, {
                labelWidth: 60,
                columnWidth: 0.2,
                xtype: 'textfield',
                readOnly: true,
                fieldLabel: '产品类别',
                value: thisRow.MaterialtypeId + ',' + thisRow.MaterialtypeName,
                name: thisRow.MaterialtypeId,
            }, {
                id: "material" + id,
                columnWidth: 0.2,
                labelWidth: 35,
                name: testguid + thisRow.BillNo + thisRow.RowId + indexid + e.dataInfo.dataRow.data["MATERIALID"],
                fieldLabel: '产品',
                xtype: 'libSearchfield',
                relSource: { 'com.Material': '' },
                relName: 'MATERIALNAME',
                relPk: 'A.MATERIALID',
                selParams: ['A.UNITID'],
                get condition() {
                    return "A.MATERIALTYPEID = '" + thisRow.MaterialtypeId + "'" + " AND A.COMBINEDPARTS = '0'" + " AND A.MATERIALID <> '" + e.dataInfo.dataRow.data["MATERIALID"] + "'"
                },
                tableIndex: 0,
                selectFields: 'A.MATERIALID,A.MATERIALNAME',
                existAtt: existAtt,
                listeners: {
                    change: function (a, b, c, d) {
                        if (b == null) {
                            b = "";
                        }
                        Ext.getCmp(a.name).materialId = b;
                        Ext.getCmp(a.name).materialName = a.rawValue.split(',')[1];
                        var returnData = This.invorkBcf('GetAttIdName', [b]);
                        if (returnData.AttributeId == null) {
                            return;
                        }
                        if (returnData.AttributeId == "") {
                            this.existAtt = false;
                        }
                        else {
                            this.existAtt = true;
                        }

                        Ext.getCmp(a.name).items.items[5].setValue("");
                        Ext.getCmp(a.name).items.items[6].setValue("");
                        Ext.getCmp(a.name).attributeCode = "";
                        Ext.getCmp(a.name).attributeDesc = "";
                    }
                }
            }, {
                columnWidth: 0.15,
                flag: testguid + thisRow.BillNo + thisRow.RowId + indexid + e.dataInfo.dataRow.data["MATERIALID"],
                labelWidth: 35,
                id: "textQu" + id,
                xtype: 'numberfield',
                fieldLabel: '数量',
                value: thisRow.Quantity,
                allowDecimals: false, // 允许小数点
                allowNegative: false, // 允许负数
                listeners: {
                    change: function (a, b, c, d) {
                        //if (b<1) {
                        //    Ext.Msg.alert("提示", '数量必须大于0！');
                        //    a.value = 1;
                        //    Ext.getCmp(a.flag).quantity = 1;
                        //}
                        Ext.getCmp(a.flag).quantity = b;

                    }
                }

            }, {

                id: "textAc" + id,
                columnWidth: 0.2,
                flag: testguid + thisRow.BillNo + thisRow.RowId + indexid + e.dataInfo.dataRow.data["MATERIALID"],
                labelWidth: 60,
                xtype: 'textfield',
                fieldLabel: '特征标识',
                readOnly: true,
                value: AttributeCode,
            }, {

                id: "textAd" + id,
                columnWidth: 0.2,
                flag: testguid + thisRow.BillNo + thisRow.RowId + indexid + e.dataInfo.dataRow.data["MATERIALID"],
                margin: '10 10 10 0  ',
                labelWidth: 60,
                xtype: 'textfield',
                fieldLabel: '特征描述',
                readOnly: true,
                value: AttributeDesc,
            }],
        listeners: {
            //双击特征标识
            dblclick: {
                element: 'body',
                fn: function (a, b) {
                    if (b.id.substr(0, 6) == 'textAc') {
                        var panelId = b.offsetParent.id;
                        var thisPanel = Ext.getCmp(panelId);

                        var materialId = thisPanel.materialId;
                        var materialId = thisPanel.materialId;
                        if (materialId == '' || materialId == undefined) {
                            Ext.Msg.alert("提示", '请先维护产品！');
                            return;
                        }
                        if (!thisPanel.items.items[3].existAtt) {
                            Ext.Msg.alert("提示", '该产品无特征！');
                            return;
                        }
                        var returnData = This.invorkBcf('GetAttIdName', [materialId]);


                        var newData = This.invorkBcf('GetAttJson', [materialId, returnData.AttributeId, thisPanel.attributeCode]);

                        if (newData.length == 0) {
                            Ext.Msg.alert("提示", '无法获取该产品特征！');
                        }
                        var dataList = {
                            MaterialId: materialId,
                            AttributeId: returnData.AttributeId,
                            AttributeName: returnData.AttributeName,
                            AttributeCode: thisPanel.attributeCode,
                            BillNo: thisPanel.billNo,
                            Row_Id: thisPanel.rowId,

                        };
                        //呼出特征框
                        //CreatAtt(thisPanel, dataList, This, FormMethod);
                        CreatAttForm(dataList, newData, This, thisPanel, FillCombineForm);


                    }
                }
            }
        },

        //panel绑定字段
        materialName: thisRow.MaterialName,
        materialtypeId: thisRow.MaterialtypeId,
        materialtypeName: thisRow.MaterialtypeName,
        attributeCode: AttributeCode,
        attributeDesc: AttributeDesc,
        billNo: thisRow.BillNo,
        rowId: thisRow.RowId + indexid,
        quantity: thisRow.Quantity,
        day: 0,

    });
    //如果是手动新增行，物料为空
    if (thisRow.IsNotAdd) {
        var materialId = thisRow.MaterialId;
        var materialName = thisRow.MaterialName;
        if (materialId != "") {
            formPanel.items.items[3].rawValue = materialId + "," + materialName;
            formPanel.materialId = materialId;
            formPanel.materialName = materialName;
        }
    }
    return formPanel;
}


//计算最迟日期
function CalculateLastestdate(This) {
    var masterRow = This.dataSet.getTable(0).data.items[0];
    var single = masterRow.data["SINGLEDATE"];
    var order = masterRow.data["ORDERDATES"];
    var lastestDate = This.invorkBcf('GetLastest', [single, order]);
    Ext.getCmp("LASTESTDATE0_" + This.winId).setValue(lastestDate);
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
/*复制结束-----------------------------------------------------------------------------------------------------------------------------------------*/