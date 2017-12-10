stkOutDeliveryNoteVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = stkOutDeliveryNoteVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = stkOutDeliveryNoteVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    //表头
    var masterRow = this.dataSet.getTable(0).data.items[0];
    //表身
    var allBodyRow = this.dataSet.getTable(1).data.items;

    //编辑状态
    if (this.isEdit) {
        //表头
        if (e.dataInfo && e.dataInfo.tableIndex == 0) {
            if (e.libEventType == LibEventTypeEnum.Validating) {
                //汇率
                if (e.dataInfo.fieldName == "STANDARDCOILRATE") {
                    //把汇率删了不填内容则默认撤销更改
                    if (e.dataInfo.value == "") {
                        e.dataInfo.cancel = true;
                        return;
                    }
                    //引了来源单，无法修改汇率
                    var hasFromBillNo = false;
                    if (e.dataInfo.dataRow.get("RELATIONCODE") != "") {
                        e.dataInfo.cancel = true;
                        hasFromBillNo = true;
                    }
                    if (hasFromBillNo == true) {
                        Ext.Msg.alert("提示", '该单据有行数据来源于采购订单，请勿更改，以免影响过账');
                    }
                }
                //来源单号
                if (e.dataInfo.fieldName == "RELATIONCODE" && e.dataInfo.dataRow.get("CURRENTSTATE") == 2) {
                    Ext.Msg.alert("提示", '已经保存的通知单来源单号无法修改，请新建入库通知单');
                    e.dataInfo.cancel = true;
                    return;
                }
            }
            if (e.libEventType == LibEventTypeEnum.Validated) {
                //变更汇率
                if (e.dataInfo.fieldName == "STANDARDCOILRATE" && e.dataInfo.value) {
                    var items = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < items.length; i++) {
                        var ScmMoneyBcf = {}; //金额、数量换算
                        ScmMoneyBcf.DealsQuantity = this.dataSet.getTable(1).data.items[i].get("RECEIVEQTY");//交易数量
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
                    var wareHouseId = Ext.getCmp("WAREHOUSEID0_" + this.winId).rawValue;
                    var wareHouseName = e.dataInfo.dataRow.get("WAREHOUSENAME");
                    for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                        if (wareHouseId) {
                            wareHouseId = wareHouseId.split(",")[0];
                            this.dataSet.getTable(1).data.items[i].set("WAREHOUSEID", wareHouseId);
                        } else {
                            this.dataSet.getTable(1).data.items[i].set("WAREHOUSEID", wareHouseId);
                        }
                        this.dataSet.getTable(1).data.items[i].set("WAREHOUSENAME", wareHouseName);
                    }
                }
                //更改表头仓管员，表身联动
                if (e.dataInfo.fieldName == "WAREHOUSEPERSONID") {
                    var wareHousePersonId = Ext.getCmp("WAREHOUSEPERSONID0_" + this.winId).rawValue;
                    var wareHousePersonName = e.dataInfo.dataRow.get("WAREHOUSEPERSONNAME");
                    for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                        if (wareHousePersonId) {
                            wareHousePersonId = wareHousePersonId.split(",")[0];
                            this.dataSet.getTable(1).data.items[i].set("WAREHOUSEPERSONID", wareHousePersonId);
                        } else {
                            this.dataSet.getTable(1).data.items[i].set("WAREHOUSEPERSONID", wareHousePersonId);
                        }
                        this.dataSet.getTable(1).data.items[i].set("WAREHOUSEPERSONNAME", wareHousePersonName);
                    }
                }
                if (e.dataInfo.fieldName == "PRODUCTCONTRACTNO") {
                    var contractNo = Ext.getCmp("PRODUCTCONTRACTNO0_" + this.winId).rawValue;
                    for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                        this.dataSet.getTable(1).data.items[i].set("PRODUCTCONTRACTNO", contractNo);
                    }
                }

                if (e.dataInfo && e.dataInfo.tableIndex == 0) {
                    switch (e.dataInfo.fieldName) {
                        case 'PRODUCTORDER'://投产单带出合同号
                            if (e.dataInfo.value != null && e.dataInfo.value != "") {
                                var masterRow = this.dataSet.getTable(0).data.items[0];
                                masterRow.set("PRODUCTCONTRACTNO", e.dataInfo.dataRow.data.CONTRACTNO);
                                this.forms[0].loadRecord(masterRow);
                                for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                                    this.dataSet.getTable(1).data.items[i].set("PRODUCTCONTRACTNO", e.dataInfo.dataRow.data.CONTRACTNO);
                                }
                            }
                            break;
                    }
                }
            }
        }
        //表身
        if (e.dataInfo && e.dataInfo.tableIndex == 1) {
            //表身不可手工新增
            //if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
            //    e.dataInfo.cancel = true;
            //    return;
            //}
            if (e.libEventType == LibEventTypeEnum.AddRow) {
                //表身
                /*----税率默认为表头往来单位税率---*/
                this.forms[0].updateRecord(masterRow);
                //获得税率
                var taxRate = this.invorkBcf("getTaxRate", [this.dataSet.getTable(0).data.items[0].data["CONTACTSOBJECTID"]]);
                //税率
                e.dataInfo.dataRow.set("TAXRATE", taxRate);
                //投产单编号
                e.dataInfo.dataRow.set("PRODUCTCONTRACTNO", this.dataSet.getTable(0).data.items[0].data["PRODUCTCONTRACTNO"]);
                /*-------------------------------------*/
                //表身默认表头仓库仓管员
                for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                    var wareHouseId = Ext.getCmp("WAREHOUSEID0_" + this.winId).rawValue;
                    if (wareHouseId) {
                        wareHouseId = wareHouseId.split(",")[0];
                        this.dataSet.getTable(1).data.items[i].set("WAREHOUSEID", wareHouseId);
                    } else {
                        this.dataSet.getTable(1).data.items[i].set("WAREHOUSEID", wareHouseId);
                    }
                    this.dataSet.getTable(1).data.items[i].set("WAREHOUSENAME", masterRow.get("WAREHOUSENAME"));

                    var wareHousePersonId = Ext.getCmp("WAREHOUSEPERSONID0_" + this.winId).rawValue;
                    if (wareHousePersonId) {
                        wareHousePersonId = wareHousePersonId.split(",")[0];
                        this.dataSet.getTable(1).data.items[i].set("WAREHOUSEPERSONID", wareHousePersonId);
                    } else {
                        this.dataSet.getTable(1).data.items[i].set("WAREHOUSEPERSONID", wareHousePersonId);
                    }
                    this.dataSet.getTable(1).data.items[i].set("WAREHOUSEPERSONNAME", masterRow.get("WAREHOUSEPERSONNAME"));
                }
            }
            //Validating
            if (e.libEventType == LibEventTypeEnum.Validating) {
                //物料
                if (e.dataInfo.fieldName == "MATERIALID") {
                    for (var i = 0; i < allBodyRow.length; i++) {
                        //已交易入库数
                        var hasDealsQty = this.dataSet.getTable(1).data.items[i].data["HASDEALSQTY"];
                        if (hasDealsQty > 0) {
                            break;
                        }
                    }
                    //已交易入库数 > 0，被下游单据引用，无法更改物料
                    if (hasDealsQty > 0) {
                        Ext.Msg.alert("提示", '该通知单已被采购入库单引用，已交易入库数大于0，不能修改物料');
                        e.dataInfo.cancel = true;
                        return;
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
                //拒收数
                if (e.dataInfo.fieldName == "REJECTIONQTY") {
                    if (e.dataInfo.value >= 0) {
                        if (e.dataInfo.value > e.dataInfo.dataRow.get("RECEIVEQTY")) {
                            Ext.Msg.alert("提示", '拒收数不可超过到货数量');
                            e.dataInfo.cancel = true;
                            return;
                        }
                            //可入库交易数、可入库基本数
                        else {
                            //获取单位换算比
                            var unitRate = this.invorkBcf("GetUnitRate", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO")]);
                            e.dataInfo.dataRow.set("CANDEALSQTY", e.dataInfo.dataRow.get("RECEIVEQTY") - e.dataInfo.value);
                            e.dataInfo.dataRow.set("CANQTY", (e.dataInfo.dataRow.get("RECEIVEQTY") - e.dataInfo.value) / unitRate);
                        }
                    }
                    else {
                        Ext.Msg.alert("提示", '拒收数不可小于0');
                        e.dataInfo.cancel = true;
                        return;
                    }
                }
                //到货数量
                if (e.dataInfo.fieldName == "RECEIVEQTY") {
                    if (e.dataInfo.value >= 0) {
                        //来源单号
                        //var fromBillNo = masterRow.get("RELATIONCODE");
                        //调用中间层方法（表头fromBillNo为空时返回的returnData长度为0，不会进入for循环）
                        //var returnData = this.invorkBcf("GetPurChaseNoticeFromOrder", [fromBillNo]);
                        //for (var i = 0; i < returnData.length; i++) {
                        //来源行标识相同
                        //if (e.dataInfo.dataRow.get("FROMROW_ID") == returnData[i].FromRow_Id) {
                        //采购订单交易数量（已经过 入库通知交易数量和已入库交易数量 的减法计算）（即可执行交易数量）
                        //var purchaseOrderDealsQty = returnData[i].DealsQuantity;

                        //if (purchaseOrderDealsQty < e.dataInfo.value) {
                        //    Ext.Msg.alert("提示", '到货数量超过了采购订单中的可执行交易数量');
                        //    e.dataInfo.cancel = true;
                        //    return;
                        //}
                        //else {
                        //获取单位换算比
                        var unitRate = this.invorkBcf("GetUnitRate", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO")]);
                        e.dataInfo.dataRow.set("CANDEALSQTY", e.dataInfo.value - e.dataInfo.dataRow.get("REJECTIONQTY"));
                        e.dataInfo.dataRow.set("CANQTY", (e.dataInfo.value - e.dataInfo.dataRow.get("REJECTIONQTY")) / unitRate);
                        //}
                        //}
                        //}
                        //无来源单号的行（手动新增的行）
                        if (e.dataInfo.dataRow.get("FROMBILLNO") == "") {
                            //获取单位换算比
                            var unitRate = this.invorkBcf("GetUnitRate", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO")]);
                            e.dataInfo.dataRow.set("CANDEALSQTY", e.dataInfo.value - e.dataInfo.dataRow.get("REJECTIONQTY"));
                            e.dataInfo.dataRow.set("CANQTY", (e.dataInfo.value - e.dataInfo.dataRow.get("REJECTIONQTY")) / unitRate);
                        }
                    }
                    else {
                        Ext.Msg.alert("提示", '交易数量必须大于0');
                        e.dataInfo.cancel = true;
                        return;
                    }
                }
                //基本数量
                if (e.dataInfo.fieldName == "QUANTITY") {
                    if (e.dataInfo.value >= 0) {
                        //获取单位换算比
                        var unitRate = this.invorkBcf("GetUnitRate", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO")]);
                        //交易数量
                        //var dealsQuantity = e.dataInfo.value * unitRate;
                        //来源单号
                        //var fromBillNo = masterRow.get("RELATIONCODE");
                        //调用中间层方法
                        //var returnData = this.invorkBcf("GetPurChaseNoticeFromOrder", [fromBillNo]);
                        //for (var i = 0; i < returnData.length; i++) {
                        //    if (e.dataInfo.dataRow.get("FROMROW_ID") == returnData[i].FromRow_Id) {
                        //        //采购订单交易数量
                        //        var purchaseOrderDealsQty = returnData[i].DealsQuantity;

                        //        if (purchaseOrderDealsQty < dealsQuantity) {
                        //            Ext.Msg.alert("提示", '修改基本数量过大，导致到货数量超过了采购订单中的可执行交易数量');
                        //            e.dataInfo.cancel = true;
                        //            return;
                        //        } else {
                        e.dataInfo.dataRow.set("CANDEALSQTY", e.dataInfo.value * unitRate - e.dataInfo.dataRow.get("REJECTIONQTY"));
                        e.dataInfo.dataRow.set("CANQTY", (e.dataInfo.value * unitRate - e.dataInfo.dataRow.get("REJECTIONQTY")) / unitRate);
                        //}
                        //}
                        //}
                        //该行的来源单号为空
                        if (e.dataInfo.dataRow.get("FROMBILLNO") == "") {
                            e.dataInfo.dataRow.set("CANDEALSQTY", e.dataInfo.value * unitRate - e.dataInfo.dataRow.get("REJECTIONQTY"));
                            e.dataInfo.dataRow.set("CANQTY", (e.dataInfo.value * unitRate - e.dataInfo.dataRow.get("REJECTIONQTY")) / unitRate);
                        }
                    } else {
                        Ext.Msg.alert("提示", '基本数量必须大于0');
                        e.dataInfo.cancel = true;
                        return;
                    }
                }
                //引了来源单，无法修改部分字段
                //物料、交易单位、单价、金额、税率、含税单价、本位币金额、本位币含税金额
                // if (e.dataInfo.fieldName == "MATERIALID" || e.dataInfo.fieldName == "DEALSUNITID" || e.dataInfo.fieldName == "DEALSUNITNO" || e.dataInfo.fieldName == "PRICE" || e.dataInfo.fieldName == "AMOUNT" || e.dataInfo.fieldName == "TAXRATE" || e.dataInfo.fieldName == "TAXPRICE" || e.dataInfo.fieldName == "BWAMOUNT" || e.dataInfo.fieldName == "BWTAXAMOUNT") {
                if (e.dataInfo.fieldName == "MATERIALID" || e.dataInfo.fieldName == "DEALSUNITID" || e.dataInfo.fieldName == "DEALSUNITNO") {
                    if (e.dataInfo.dataRow.get("FROMBILLNO") != "" || e.dataInfo.dataRow.get("FROMBILLNO") != 0) {
                        Ext.Msg.alert("提示", '该数据来源于采购订单，请勿更改，以免影响过账');
                        e.dataInfo.cancel = true;
                        if (e.dataInfo.fieldName == "MATERIALID") {
                            if (e.dataInfo.value == "" || e.dataInfo.value != e.dataInfo.oldValue) {
                                e.dataInfo.dataRow.set("MATERIALID", e.dataInfo.oldValue);
                            }
                        }
                        if (e.dataInfo.fieldName == "DEALSUNITID") {
                            if (e.dataInfo.value == "" || e.dataInfo.value != e.dataInfo.oldValue) {
                                e.dataInfo.dataRow.set("DEALSUNITID", e.dataInfo.oldValue);
                            }
                        }
                        if (e.dataInfo.fieldName == "DEALSUNITNO") {
                            if (e.dataInfo.value == "" || e.dataInfo.value != e.dataInfo.oldValue) {
                                e.dataInfo.dataRow.set("DEALSUNITNO", e.dataInfo.oldValue);
                            }
                        }
                        return;
                    }
                }
                //条码号默认为送货单号
                if (e.dataInfo.fieldName == "DELIVERYNOTENO") {
                    e.dataInfo.dataRow.set("BARCODE", e.dataInfo.value);
                }
            }
            //Validated
            if (e.libEventType == LibEventTypeEnum.Validated) {
                this.forms[0].updateRecord(masterRow);
                //金额、数量换算
                var ScmMoneyBcf = {};
                //交易数量
                ScmMoneyBcf.DealsQuantity = e.dataInfo.dataRow.get("RECEIVEQTY") - e.dataInfo.dataRow.get("REJECTIONQTY");
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
                ScmMoneyBcf.OldDealsQuantity = e.dataInfo.dataRow.get("RECEIVEQTY");

                //如果动作内容不为空
                if (e.dataInfo.value != null) {
                    switch (e.dataInfo.fieldName) {
                        //改变交易数量
                        case "RECEIVEQTY":
                            ////对比最小批量返回新数量、最小批量、最小批量倍数
                            //var infoList = this.invorkBcf('GetQuantity', [e.dataInfo.value, e.dataInfo.dataRow.data["MATERIALID"]]);
                            //var info = infoList[0];
                            //if (e.dataInfo.value != info.RECEIVEQTY) {
                            //    Ext.Msg.alert("提示", "物料" + e.dataInfo.dataRow.data["MATERIALNAME"] + "的采购最小批量是" + info.PURCHASEQTY + "，最小批量倍数是" + info.BATCHTIMES + " ，数量" + e.dataInfo.value + "不符合采购标准，系统会讲数量更改为" + info.RECEIVEQTY + "，请知悉！");
                            //    Ext.getCmp('RECEIVEQTY1_' + this.winId).setValue(info.RECEIVEQTY);
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
                                var unitData = this.invorkBcf("GetData", [e.dataInfo.value, e.dataInfo.dataRow.data["DEALSUNITID"], e.dataInfo.dataRow.data["DEALSUNITNO"], 0, e.dataInfo.dataRow.get("RECEIVEQTY"), e.dataInfo.dataRow.get("UNITID"), 0]);
                                //交易单位变更引发基本数量变化
                                e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                                //如果交易单位 == 基本单位，基本数量变为交易数量
                                if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("DEALSUNITID") == e.dataInfo.dataRow.get("UNITID")) {
                                    e.dataInfo.dataRow.set("QUANTITY", e.dataInfo.dataRow.get("RECEIVEQTY"))
                                }
                            }
                            break;
                            //改变基本数量（基本数量会变动交易数量）
                        case "QUANTITY":
                            //如果交易单位，物料代码不为空
                            if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("MATERIALID")) {
                                //交易数量
                                var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO"), e.dataInfo.value, e.dataInfo.dataRow.get("RECEIVEQTY"), e.dataInfo.dataRow.get("UNITID"), 1]);
                                //交易数量变化
                                e.dataInfo.dataRow.set("RECEIVEQTY", unitData.ConverQuantity);
                                if (unitData.ErrorType == 1) {
                                    Ext.Msg.alert("提示", "通过物料，基础单位，交易单位无法在对应物料表中找到明细！");
                                }
                                else if (unitData.ErrorType == 2) {
                                    Ext.Msg.alert("提示", "物料明细表中启动了浮动，数量超出范围！");
                                }
                                //如果交易单位 == 基本单位，交易数量变为基本数量
                                if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("DEALSUNITID") == e.dataInfo.dataRow.get("UNITID")) {
                                    e.dataInfo.dataRow.set("RECEIVEQTY", e.dataInfo.value)
                                }
                                //--交易数量改变重新计算
                                ////对比最小批量返回新数量、最小批量、最小批量倍数
                                //var infoList = this.invorkBcf('GetQuantity', [e.dataInfo.dataRow.data["RECEIVEQTY"], e.dataInfo.dataRow.data["MATERIALID"]]);
                                //var info = infoList[0];
                                //if (e.dataInfo.dataRow.data["RECEIVEQTY"] != info.RECEIVEQTY) {
                                //    Ext.Msg.alert("提示", "物料" + e.dataInfo.dataRow.data["MATERIALNAME"] + "的采购最小批量是" + info.PURCHASEQTY + "，最小批量倍数是" + info.BATCHTIMES + " ，数量" + e.dataInfo.dataRow.data["RECEIVEQTY"] + "不符合采购标准，系统会讲数量更改为" + info.RECEIVEQTY + "，请知悉！");
                                //    e.dataInfo.dataRow.set("RECEIVEQTY", info.RECEIVEQTY);
                                //}

                                //金额换算引发金额、数量的变化
                                ScmMoneyBcf.DealsQuantity = e.dataInfo.dataRow.get("RECEIVEQTY") - e.dataInfo.dataRow.get("REJECTIONQTY");
                                ScmMoneyBcf.OldDealsQuantity = e.dataInfo.dataRow.get("RECEIVEQTY");
                                var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeDealsQuantity]);
                                //调用下面编写的方法
                                DeliverSetPrice.call(this, e, data);

                                //获取单位换算比
                                var unitRate = this.invorkBcf("GetUnitRate", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO")]);
                                //复原交易数量
                                e.dataInfo.dataRow.set("RECEIVEQTY", e.dataInfo.value * unitRate);
                            }
                            break;
                            //改变交易单位
                        case "DEALSUNITID":
                            if (e.dataInfo.value.length > 0) {
                                //设交易单位标识为空
                                e.dataInfo.dataRow.set("DEALSUNITNO", "");
                                var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.value, e.dataInfo.dataRow.data["DEALSUNITNO"], 0, e.dataInfo.dataRow.get("RECEIVEQTY"), e.dataInfo.dataRow.get("UNITID"), 0]);
                                //交易单位变更引发基本数量变化
                                e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                                //如果交易单位 == 基本单位，基本数量变为交易数量
                                if (e.dataInfo.dataRow.get("DEALSUNITID") && e.dataInfo.dataRow.get("DEALSUNITID") == e.dataInfo.dataRow.get("UNITID")) {
                                    e.dataInfo.dataRow.set("QUANTITY", e.dataInfo.dataRow.get("RECEIVEQTY"))
                                }
                            }
                            break;
                            //改变交易单位标识
                        case "DEALSUNITNO":
                            var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.data["DEALSUNITID"], e.dataInfo.value, 0, e.dataInfo.dataRow.get("RECEIVEQTY"), e.dataInfo.dataRow.get("UNITID"), 0]);
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
                        case 'CONTACTSOBJECTID':
                            ScmMoneyBcf.TaxRate = e.dataInfo.dataRow.get("TAXRATE");
                            var data = this.invorkBcf('DeliveryNote_AmountNumConvert', [ScmMoneyBcf, ChangeTypeEnum.ChangeTaxRate]);
                            //调用下面编写的方法
                            DeliverSetPrice.call(this, e, data);
                            break;
                            //单价
                        case 'PRICE':
                            ScmMoneyBcf.Price = e.dataInfo.value;
                            //ScmMoneyBcf.TaxPrice = e.dataInfo.value * e.dataInfo.dataRow.get("RECEIVEQTY") * ( 1 + e.dataInfo.dataRow.get("TAXRATE")) //含税单价
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
                                var oldDealsQuantity = e.dataInfo.dataRow.get("RECEIVEQTY");
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
            }
        }
    }
    //自定义按钮
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "BtnGetData") {
            if (this.isEdit) {
                var contactsObjectid = this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTID']; //获取往来对象编码
                var contactsObjectname = this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTNAME']; //获取往来对象名称
                Ax.utils.LibVclSystemUtils.openDataFunc("stk.DeliveryNoteDataFunc", "选择订单", [this, "STKDELIVERYNOTEDATAFUNCDETAIL", contactsObjectid, contactsObjectname]);
            }
            else {
                Ext.Msg.alert("系统提示", "编辑状态才能使用数据加载按钮！");
            }
        }
        else if (e.dataInfo.fieldName == "BtnCreateDelivery") {
            if (!this.isEdit) {
                this.forms[0].loadRecord(masterRow);
                var backData;
                var wareHouse = this.dataSet.getTable(0).data.items[0].data["WAREHOUSEID"];//仓库
                var contactObjectId = this.dataSet.getTable(0).data.items[0].data["CONTACTSOBJECTID"];//往来单位ID
                var fromBillNo = this.dataSet.getTable(0).data.items[0].data["BILLNO"];//采购收货单号
                var fromTypeId = this.dataSet.getTable(0).data.items[0].data["TYPEID"];
                var currencyId = this.dataSet.getTable(0).data.items[0].data["CURRENCYID"];
                var paymentTypeId = this.dataSet.getTable(0).data.items[0].data["PAYMENTTYPEID"];//结算方式
                var invoiceTypeId = this.dataSet.getTable(0).data.items[0].data["INVOICETYPEID"];//发票类型
                var currentState = this.dataSet.getTable(0).data.items[0].data["CURRENTSTATE"];
                var auditState = this.dataSet.getTable(0).data.items[0].data["AUDITSTATE"];
                var productOrder = this.dataSet.getTable(0).data.items[0].data["PRODUCTORDER"];
                var productContractNo = this.dataSet.getTable(0).data.items[0].data["PRODUCTCONTRACTNO"];
                if (contactObjectId == "" || contactObjectId == undefined) {
                    Ext.Msg.alert("提示", "往来单位不能为空！");
                }
                else if (fromBillNo == "" || fromBillNo == undefined) {
                    Ext.Msg.alert("提示", "单据编号不能为空！");

                }
                else if (currentState != "2") {
                    Ext.Msg.alert("提示", "单据未生效！");
                }
                else if (auditState != "2") {
                    Ext.Msg.alert("提示", "单据未审核通过！");
                }

                else {

                    backData = this.invorkBcf('BuildPurStockIn', [wareHouse, contactObjectId, fromBillNo, fromTypeId, currencyId, paymentTypeId, invoiceTypeId, productOrder, productContractNo]);
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
            else { alert("编辑状态下不可操作！"); }

        }

        else if (e.dataInfo.fieldName == "BtnCreateQuality") {
            if (!this.isEdit) {
                this.forms[0].loadRecord(masterRow);
                var backData;
                var wareHouse = this.dataSet.getTable(0).data.items[0].data["WAREHOUSEID"];//仓库
                var contactObjectId = this.dataSet.getTable(0).data.items[0].data["CONTACTSOBJECTID"];//往来单位ID
                var deliverylNo = this.dataSet.getTable(0).data.items[0].data["BILLNO"];//采购收货单号
                var fromTypeId = this.dataSet.getTable(0).data.items[0].data["TYPEID"];
                var billDate = this.dataSet.getTable(0).data.items[0].data["BILLDATE"];
                var currentState = this.dataSet.getTable(0).data.items[0].data["CURRENTSTATE"];
                var auditState = this.dataSet.getTable(0).data.items[0].data["AUDITSTATE"];
                var flowlevel = this.dataSet.getTable(0).data.items[0].data["FLOWLEVEL"];

                if (currentState != "1") {
                    Ext.Msg.alert("提示", "未生效单据才能操作！");
                }
                else if (auditState != "1" || flowlevel != '0') {
                    Ext.Msg.alert("提示", "采购人员提交审核的单据才能操作！");
                }

                else {

                    backData = this.invorkBcf('BuildQualityCheck', [wareHouse, contactObjectId, deliverylNo, fromTypeId, billDate]);
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
                                    Ax.utils.LibVclSystemUtils.openBill('qc.PurQualityCheck', BillTypeEnum.Bill, "采购质检单", BillActionEnum.Browse, undefined, curPks);
                                }
                            }
                        })
                    }
                    else {
                        Ax.utils.LibVclSystemUtils.openBill('qc.PurQualityCheck', BillTypeEnum.Bill, "采购质检单", BillActionEnum.Browse, undefined, curPks);
                    }
                }
            }
            else { Ext.Msg.alert("提示", "编辑状态下不可操作！"); }

        }
        else if (e.dataInfo.fieldName == "BtnBarcodePrint") {
            if (!this.isEdit) {
                var frombillno = this.dataSet.getTable(0).data.items[0].data["BILLNO"];
                Ax.utils.LibVclSystemUtils.openDataFunc('stk.BarcodePrintDataFunc', "条码打印", [0, frombillno, this.tpl.ProgId, this.tpl.DisplayText]);
            }
            else {
                Ext.Msg.alert("提示", "编辑状态下不可操作！");
            }
        }
    }
}

//计算数据的赋值
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

//选择来源单号时的填充数据方法
proto.fillNoticeData = function (returnData) {
    Ext.suspendLayouts();//关闭Ext布局
    //子表
    var bodyTable = this.dataSet.getTable(1);
    //表头
    var masterRow = this.dataSet.getTable(0).data.items[0];
    bodyTable.suspendEvents();//关闭store事件
    try {
        //删除当前grid的数据
        this.deleteAll(1);
        //获取采购询价单datafunc的grid
        var grid = Ext.getCmp(this.winId + 'PURCHASENOTICEDETAILGrid');
        var n = 1;
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            //子表赋值
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                //未完结的行
                if (info.IsEnd == false) {
                    //为grid添加行
                    //通知单表身字段
                    var newRow = this.addRowForGrid(grid);
                    newRow.set("ROWNO", n);
                    n++;
                    newRow.set("MATERIALID", info.MaterialId);
                    newRow.set("MATERIALNAME", info.MaterialName);
                    newRow.set("SPECIFICATION", info.Specification);
                    newRow.set("ISCHECK", info.IsCheck);
                    newRow.set("FIGURENO", info.FigureNo);
                    newRow.set("TEXTUREID", info.TextureId);
                    newRow.set("MATERIALTYPEID", info.MaterialTypeId);
                    newRow.set("MATERIALTYPENAME", info.MaterialTypeName);
                    newRow.set("MATERIALSPEC", info.MaterialSpec)
                    newRow.set("ATTRIBUTEID", info.AttributeId);
                    newRow.set("ATTRIBUTENAME", info.AttributeName);
                    newRow.set("ATTRIBUTECODE", info.AttributeCode);
                    newRow.set("ATTRIBUTEDESC", info.AttributeDesc);
                    newRow.set("PREPAREDATE", info.PrepareDate);
                    //从采购订单过账表中获取已入库交易数量
                    var receiptinDealsQuantity = this.invorkBcf("GetReceiptinDealsQuantity", [masterRow.get("RELATIONCODE"), info.FromRow_Id, info.MaterialId]);
                    newRow.set("RECEIVEQTY", info.DealsQuantity - receiptinDealsQuantity);
                    newRow.set("DEALSUNITID", info.DealsUnitId);
                    newRow.set("DEALSUNITNAME", info.DealsUnitName);
                    newRow.set("DEALSUNITNO", info.DealsUnitNo);
                    //获取单位换算比
                    var unitRate = this.invorkBcf("GetUnitRate", [info.MaterialId, info.DealsUnitId, info.DealsUnitNo])
                    newRow.set("QUANTITY", (info.DealsQuantity - receiptinDealsQuantity) / unitRate);

                    newRow.set("UNITID", info.UnitId);
                    newRow.set("UNITNAME", info.UnitName);
                    newRow.set("TAXRATE", info.TaxRate);
                    newRow.set("PRICE", info.Price);
                    newRow.set("TAXPRICE", info.Price * (info.TaxRate + 1));
                    //金额联动计算
                    newRow.set("AMOUNT", info.Price * info.DealsQuantity);
                    newRow.set("TAXAMOUNT", info.Price * (info.TaxRate + 1) * info.DealsQuantity);
                    newRow.set("AMOUNT", info.Price * info.DealsQuantity);
                    newRow.set("TAXES", info.TaxRate * info.Price * info.DealsQuantity);
                    newRow.set("BWAMOUNT", info.Price * info.DealsQuantity * masterRow.get("STANDARDCOILRATE"));
                    newRow.set("BWTAXAMOUNT", info.Price * (info.TaxRate + 1) * info.DealsQuantity * masterRow.get("STANDARDCOILRATE"));
                    newRow.set("BWTAXES", info.TaxRate * info.Price * info.DealsQuantity * masterRow.get("STANDARDCOILRATE"));
                    //可入库交易数、可入库基本数
                    newRow.set("CANDEALSQTY", info.DealsQuantity - receiptinDealsQuantity);
                    newRow.set("CANQTY", (info.DealsQuantity - receiptinDealsQuantity) / unitRate);

                    newRow.set("FROMBILLNO", masterRow.get("RELATIONCODE"));
                    newRow.set("FROMROW_ID", info.FromRow_Id);
                }
            }
        }
    }
    finally {
        bodyTable.resumeEvents();//打开store事件
        if (bodyTable.ownGrid && bodyTable.ownGrid.getView().store != null)
            bodyTable.ownGrid.reconfigure(bodyTable);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

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