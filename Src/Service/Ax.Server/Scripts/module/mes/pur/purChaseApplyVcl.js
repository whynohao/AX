purChaseApplyVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var attId = 0;
var proto = purChaseApplyVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = purChaseApplyVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {

        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "PRICE") {
                    if (e.dataInfo.value >= 0) {
                        e.dataInfo.dataRow.set("AMOUNT", parseFloat(e.dataInfo.value) * parseFloat(e.dataInfo.dataRow.data["DEALSQUANTITY"]));
                    }
                    else {
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("系统提示", "单价不可小于0");
                    }
                }
                if (e.dataInfo.fieldName == "AMOUNT") {
                    if (e.dataInfo.value >= 0 && parseFloat(e.dataInfo.dataRow.data["AMOUNT"]) > 0) {
                        e.dataInfo.dataRow.set("PRICE", parseFloat(e.dataInfo.value) / parseFloat(e.dataInfo.dataRow.data["DEALSQUANTITY"]));
                    }
                    else {
                        e.dataInfo.dataRow.set("PRICE", 0);
                        e.dataInfo.dataRow.set("QUANTITY", 0);
                        e.dataInfo.dataRow.set("AMOUNT", 0);
                    }
                    if (e.dataInfo.value < 0) {
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("系统提示", "金额不可小于0");
                    }
                }
                if (e.dataInfo.fieldName == "MATERIALID") {
                    //if (e.dataInfo.value != e.dataInfo.oldValue) {
                    //    var hasdealsuqntity = e.dataInfo.dataRow.data["HASDEALSUQNTITY"];
                    //    //判断是否被采购订单引用 引用后不能修改
                    //    if (parseFloat(hasdealsuqntity) > 0) {
                    //        Ext.Msg.alert("系统提示", "物料已被采购订单引用，不能修改！");
                    //        e.dataInfo.cancel = true;
                    //        e.dataInfo.dataRow.set("MATERIALID", e.dataInfo.oldValue);
                    //    }
                    //}
                    if (e.dataInfo.value.length > 0) {
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
                //基本数量=交易数量*换算数量比
                if (e.dataInfo.fieldName == "DEALSQUANTITY") {
                    if (e.dataInfo.value >= 0) {
                        if (parseFloat(e.dataInfo.value) < parseFloat(e.dataInfo.dataRow.data["HASDEALSUQNTITY"]) > 0) {
                            Ext.Msg.alert("系统提示", "交易数量不能小于已执行数量");
                            e.dataInfo.cancel = true;
                        }
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
                        e.dataInfo.dataRow.set("AMOUNT", parseFloat(e.dataInfo.value) * parseFloat(e.dataInfo.dataRow.data["PRICE"]));//计算金额
                    }
                    else {
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("系统提示", "交易数量不可小于0");
                    }
                }
                //交易单位
                if (e.dataInfo.fieldName == "DEALSUNITID") {
                    if (e.dataInfo.value.length > 0) {
                        //alert(x);
                        e.dataInfo.dataRow.set("DEALSUNITNO", "");//设交易单位标识为空
                        var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.value, e.dataInfo.dataRow.data["DEALSUNITNO"], 0, e.dataInfo.dataRow.get("DEALSQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 0]);
                        e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);//交易单位变更引发数量变化
                    }
                }
                //交易单位标识
                if (e.dataInfo.fieldName == "DEALSUNITNO") {
                    var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.data["DEALSUNITID"], e.dataInfo.value, 0, e.dataInfo.dataRow.get("DEALSQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 0]);
                    e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);//交易单位标识变更引发数量变化
                }
                if (e.dataInfo.fieldName == "QUANTITY") {
                    if (e.dataInfo.value >= 0) {
                        var unitData = this.invorkBcf("GetData", [e.dataInfo.dataRow.get("MATERIALID"), e.dataInfo.dataRow.get("DEALSUNITID"), e.dataInfo.dataRow.get("DEALSUNITNO"), e.dataInfo.value, e.dataInfo.dataRow.get("DEALSQUANTITY"), e.dataInfo.dataRow.get("UNITID"), 1]);
                        e.dataInfo.dataRow.set("DEALSQUANTITY", unitData.ConverQuantity);
                        if (unitData.ErrorType == 1) {
                            Ext.Msg.alert("提示","通过物料，基础单位，交易单位无法在对应物料表中找到明细！");
                        }
                        else if (unitData.ErrorType == 2) {
                            Ext.Msg.alert("提示","物料明细表中启动了浮动，数量超出范围！");
                        }

                        //对比最小批量返回新数量、最小批量、最小批量倍数
                        var infoList = this.invorkBcf('GetQuantity', [e.dataInfo.dataRow.data["DEALSQUANTITY"], e.dataInfo.dataRow.data["MATERIALID"]]);
                        var info = infoList[0];
                        if (e.dataInfo.dataRow.data["DEALSQUANTITY"] != info.DEALSQUANTITY) {
                            Ext.Msg.alert("提示", "物料" + e.dataInfo.dataRow.data["MATERIALNAME"] + "的采购最小批量是" + info.PURCHASEQTY + "，最小批量倍数是" + info.BATCHTIMES + " ，数量" + e.dataInfo.dataRow.data["DEALSQUANTITY"] + "不符合采购标准，系统会讲数量更改为" + info.DEALSQUANTITY + "，请知悉！");
                            e.dataInfo.dataRow.set("DEALSQUANTITY", info.DEALSQUANTITY);
                        }
                        //交易数量变更引起的其它字段的变更
                        e.dataInfo.dataRow.set("AMOUNT", parseFloat(info.DEALSQUANTITY) * parseFloat(e.dataInfo.dataRow.data["PRICE"]));//计算金额
                    }
                    else {
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("系统提示", "基本数量不可小于0");
                    }
                }
            }
            break;

        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                //循环统计总数量 
                var items = this.dataSet.getTable(1).data.items;
                var amount = 0;
                for (var i = 0; i < items.length; i++) {
                    var floatAmount = items[i].data["DEALSQUANTITY"];
                    //如果更新了金额字段 加入当前金额
                    if (e.dataInfo.fieldName == "DEALSQUANTITY" && parseInt(e.dataInfo.dataRow.data["ROW_ID"]) == items[i].data["ROW_ID"]) {
                        floatAmount = e.dataInfo.value;
                    }
                    amount += parseFloat(floatAmount);
                }
                Ext.getCmp("ALLNUMS0_" + this.winId).setValue(amount);
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            var grid = Ext.getCmp(this.winId + 'PURCHASEAPPLYDETAILGrid'); //要加载数据的表名字 + Grid
            var records = grid.getView().getSelectionModel().getSelection();//选中行
            if (records.length > 0) {
                for (var i = 0; i < records.length; i++) {
                    if (parseFloat(records[i].data["HASDEALSUQNTITY"]) > 0) {
                        Ext.Msg.alert("系统提示", "行标识为：" + records[i].data["ROW_ID"] + "的数据已被采购订单引用，不能删除！");
                        e.dataInfo.cancel = true;
                    }
                }
            }
            break;
        case LibEventTypeEnum.DeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                //循环统计数量 
                var items = this.dataSet.getTable(1).data.items;
                var amount = 0;
                for (var i = 0; i < items.length; i++) {
                    if (parseFloat(items[i].data["DEALSQUANTITY"]) > 0) {
                        amount += parseFloat(items[i].data["DEALSQUANTITY"]);
                    }
                }
                Ext.getCmp("ALLNUMS0_" + this.winId).setValue(amount);
            }
            break;

        case LibEventTypeEnum.ColumnDbClick:
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
            //if (e.dataInfo.fieldName == "ATTACHMENTSRC") {
            //    var table = this.dataSet.getTable(1);
            //    Ax.utils.LibAttachmentForm.show(vcl, table.data.items[0], table.Name);
            //}
            break;
    }

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
