
finIAAmountAdjustVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    me = this;
};

var proto = finIAAmountAdjustVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finIAAmountAdjustVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "SetAttbute") {
                var records = this.dataSet.getTable(0).data.items[0].data;
                var MaterialId = records["MATERIALID"];
                var AttributeId = records["ATTRIBUTEID"];
                var AttributeCode = records["ATTRIBUTECODE"]
                var MaterialId = records["MATERIALID"];

                if (MaterialId == "") {
                    AttributeId = this.invorkBcf("GetDefaultAttr");
                }
                if (AttributeId != "") {
                    var returnData = this.invorkBcf('GetAttJson', [MaterialId, AttributeId, AttributeCode]);
                    var dataList = {
                        MaterialId: records["MATERIALID"],
                        AttributeId: AttributeId,
                        AttributeDesc: records["ATTRIBUTEDESC"],
                        AttributeCode: records["ATTRIBUTECODE"],
                        BillNo: records["BILLNO"],
                        Row_Id: records["ROW_ID"]

                    };
                    this.CreatAttForm(dataList, returnData, this, records, this.FillDataRow);

                }
            }
            break;
    }
}
//最新特征窗体
proto.CreatAttForm = function (dataList, returnData, This, row, method) {
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

                unstandard.push(this.CreatTextBox(returnData[i], isRead));
            }
            else {
                standard.push(this.CreatTextBox(returnData[i], isRead));
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
            id: 'Att' + this.attId,
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
            id: 'noAtt' + this.attId,
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
    this.attId++;
    Salewin.show();
    Salewin.items.items[1].collapse(true);
}
//非动态特征 combox
proto.CreatComBox = function (attData, isread) {

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
proto.CreatTextBox = function (attData, isread) {
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
proto.FillDataRow = function (e, This, CodeDesc) {
    //This.set("ATTRIBUTECODE", CodeDesc.Code);
    //records.set("ATTRIBUTEDESC", CodeDesc.Desc);
    Ext.getCmp("ATTRIBUTECODE0_" + This.winId).setValue(CodeDesc.Code);
    Ext.getCmp("ATTRIBUTEDESC0_" + This.winId).setValue(CodeDesc.Desc);
    // e.dataInfo.dataRow.set("ABNORMALDAY", CodeDesc.AbnormalDay);
    //设置异常天数
    //var masterRow = This.dataSet.getTable(0).data.items[0];
    //Ext.getCmp("ABNORMALDAY0_" + This.winId).setValue(CodeDesc.AbnormalDay);
    return true;
}