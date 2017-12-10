stkProcessStockInVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = stkProcessStockInVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = stkProcessStockInVcl;
var bodyRow;
var attId = 0;
proto.vclHandler = function (sender, e) {

    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);

    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.value != null) {
                switch (e.dataInfo.fieldName) {
                    case 'MATERIALID'://仓库改变带出库存状态
                        if (e.dataInfo.dataRow.get("WAREHOUSEID") != "") {
                            var stkState = this.invorkBcf("GetStkState", [e.dataInfo.dataRow.get("WAREHOUSEID")]);
                            e.dataInfo.dataRow.set("STKSTATE", stkState);
                        }
                        break;
                    case 'PRODUCTORDER'://投产单带出合同号
                        if (e.dataInfo.value != null && e.dataInfo.value != "") {
                            var masterRow = this.dataSet.getTable(0).data.items[0];
                            masterRow.set("PRODUCTCONTRACTNO", e.dataInfo.dataRow.data.CONTRACTNO);
                            this.forms[0].loadRecord(masterRow);
                        }
                        break;
                }
            }
            if (e.dataInfo && e.dataInfo.tableIndex == 0) {
                switch (e.dataInfo.fieldName) {
                    case 'PERSONID'://选择仓管员带出仓库
                        if (e.dataInfo.value != null && e.dataInfo.value != "") {
                            var warehouse = this.invorkBcf("SynWareHouse", [e.dataInfo.dataRow.get("PERSONID")]);
                            if (warehouse.WAREHOUSEID != null) {
                                var field = Ext.getCmp('WAREHOUSEID0_' + this.winId);
                                field.store.add({ Id: warehouse.WAREHOUSEID, Name: warehouse.WAREHOUSENAME });
                                field.select(warehouse.WAREHOUSEID);
                            }
                        }
                        break;
                }
            }
            if (e.dataInfo && e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.value != null) {
                    var fname = e.dataInfo.fieldName;
                    if ($.inArray(fname, mylist) > -1) {
                        getInventroyQty(e.dataInfo.dataRow);
                    }
                }
            }
        case LibEventTypeEnum.ButtonClick:
            if (this.isEdit) {
                if (e.dataInfo.fieldName == "BtnLoadData") {
                    var personId = this.dataSet.getTable(0).data.items[0].data['PERSONID'];
                    var personName = this.dataSet.getTable(0).data.items[0].data['PERSONNAME'];
                    if (personId == "") {
                        alert("请先填写人员！");
                    }
                    else {
                        Ax.utils.LibVclSystemUtils.openDataFunc('stk.ProcessStockInQuery', '在制品入库单数据查询', [this, personId, personName]);
                    }
                }
            }
            else {
                alert("单据只有在修改状态才能载入在制品入库单数据！");
            }
            if (e.dataInfo && e.dataInfo.tableIndex == 2) {
                if (e.dataInfo.value != null) {
                    switch (e.dataInfo.fieldName) {
                        case 'SQUANTITY'://仓储数量改变
                            //单位换算引发数量变化
                            var unitData = this.invorkBcf("GetData", [e.dataInfo.curGrid.parentRow.get("MATERIALID"), e.dataInfo.dataRow.get("STKUNITID"), e.dataInfo.dataRow.get("STKUNITNO"), 0, e.dataInfo.value, e.dataInfo.curGrid.parentRow.get("UNITID"), 0]);
                            e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                            break;
                        case 'QUANTITY'://基本数量改变
                            //单位换算引发数量变化
                            var unitData = this.invorkBcf("GetData", [e.dataInfo.curGrid.parentRow.get("MATERIALID"), e.dataInfo.dataRow.get("STKUNITID"), e.dataInfo.dataRow.get("STKUNITNO"), e.dataInfo.value, 0, e.dataInfo.curGrid.parentRow.get("UNITID"), 1]);
                            e.dataInfo.dataRow.set("SQUANTITY", unitData.ConverQuantity);
                            if (unitData.ErrorType == 1) {
                                alert("通过物料，基础单位，交易单位无法在对应物料表中找到明细！");
                            }
                            else if (unitData.ErrorType == 2) {
                                alert("物料明细表中启动了浮动，数量超出范围！");
                            }
                            break;
                        case 'STKUNITID'://修改仓储单位
                            if (e.dataInfo.value != e.dataInfo.dataRow.data["STKUNITID"]) {
                                e.dataInfo.dataRow.set("STKUNITNO", null);
                                e.dataInfo.dataRow.set("SQUANTITY", 1);
                                e.dataInfo.dataRow.set("QUANTITY", 1);
                            }
                            break;
                        case 'STKUNITNO'://修改仓储单位标识
                            var unitData = this.invorkBcf("GetData", [e.dataInfo.curGrid.parentRow.get("MATERIALID"), e.dataInfo.dataRow.get("STKUNITID"), e.dataInfo.value, 0, e.dataInfo.dataRow.get("SQUANTITY"), e.dataInfo.curGrid.parentRow.get("UNITID"), 0]);
                            e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
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
                    this.forms[0].loadRecord(bodyRow);
                }
            }
            break;
    }
};

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