
comAttributeVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comAttributeVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comAttributeVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.AddRow:
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                Ext.Msg.confirm('提示', '是否确认删除?', function (button) {
                    if (button == "yes") {
                        vcl.deleteRowForGrid(e.dataInfo.curGrid);
                    }
                    else if (button == "no") {
                        e.dataInfo.cancel = true;
                    }
                }, this);
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "Synchronous") {//同步
                if (!vcl.isEdit) {
                    Ext.Msg.confirm('提示', '是否确认同步结构BOM?', function (button) {
                        if (button == "yes") {
                            var addRow = [];
                            var dataRow = this.dataSet.getTable(1).data.items;
                            for (var i = 0; i < dataRow.length; i++) {
                                var add = {
                                    ATTRIBUTEID: dataRow[i].data["ATTRIBUTEID"],
                                    ROW_ID: dataRow[i].data["ROW_ID"],
                                    ROWNO: dataRow[i].data["ROWNO"],
                                    VALUECALTYPE: dataRow[i].data["VALUECALTYPE"],
                                    ATTRIBUTEITEMID: dataRow[i].data["ATTRIBUTEITEMID"],
                                    ATTRIBUTEITEMNAME: dataRow[i].data["ATTRIBUTEITEMNAME"],
                                    ATTRIBUTECODELEN: dataRow[i].data["ATTRIBUTECODELEN"]
                                }
                                addRow.push(add);
                            }
                            var data = this.invorkBcf('GetSynchronous', [addRow]);
                            Ext.Msg.alert("提示", "同步结构BOM成功");
                        }
                        else if (button == "no") {
                            e.dataInfo.cancel = true;
                        }
                    }, this);
                }
                else {
                    Ext.Msg.alert("提示", "修改状态下，不能同步结构BOM");

                }
            }
            if (e.dataInfo.fieldName == "IndividualAdjustment") {
                //个别更改
                var attributeId = this.dataSet.getTable(0).data.items[0].get("ATTRIBUTEID");
                Ax.utils.LibVclSystemUtils.openDataFunc("com.AttributeFunc",
                       "物料维护特征界面",
                       [attributeId, this]);
            }
            if (e.dataInfo.fieldName == "UnifiedAdjustment") {
                //统一更改
                var attributeId = this.dataSet.getTable(0).data.items[0].get("ATTRIBUTEID");
                var grid = Ext.getCmp(this.winId + "COMATTRIBUTEDETAIL" + "Grid");
                var records = grid.getView().getSelectionModel().getSelection();
                var returnData = [];
                var attributeItem = [];
                if (records.length > 0) {
                    for (var i = 0; i < records.length; i++) {
                        attributeItem.push(records[i].get("ATTRIBUTEITEMID"));
                    }
                    returnData = this.invorkBcf('GetAttrInfo', [attributeItem]);
                    this.CreatAttForm(attributeId, returnData, this);
                }
                else
                    Ext.Msg.alert("提示", "请至少选择一条特征项进行统一修改");
            }
            break;
    }
}
proto.CreatAttForm = function (attributeId, returnData, This) {
    var standard = [];
    for (var i = 0; i < returnData.length; i++) {
        if (returnData[i].Dynamic) {
            standard.push(CreatTextBox(returnData[i]));
        }
        else {
            standard.push(CreatComBox(returnData[i]));
        }
    }

    //确认按钮
    var btnConfirm = new Ext.Button({
        width: 200,
        height: 35,
        text: "确定",
        type: 'submit',
        handler: function () {
            var yes = true;
            var thisWin = Ext.getCmp("attWin");
            var attPanel = thisWin.items.items[0];
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
            if (attDic.length > 0) {

                yes = This.invorkBcf('UnifiedAdjustment', [attributeId, attDic]);
            }
            if (yes) {
                Ext.Msg.alert("提示", '调整成功');
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
            Ext.getCmp("attWin").close();
        }
    })
    //按钮Panle
    var btnPanel = new Ext.form.Panel({
        layout: 'column',
        width: '100%',
        margin: '5 10 5 10',
        defaults: {
            margin: '10 40 0 40',
            columnWidth: .5
        },
        height: 60,
        items: [btnConfirm, btnCancel]
    })

    var win = new Ext.create('Ext.window.Window', {
        id: "attWin",
        title: '特征信息',
        resizable: false,
        //closeAction: "close",
        modal: true,
        width: 600,
        height: 330,
        autoScroll: true,
        layout: 'column',
        items: [{
            id: 'Att',
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
            //listeners: {
            //    collapse: function (a, b) {
            //        //Ext.getCmp('no'+ a.id).expand();
            //    },
            //    expand: function (a, b) {
            //        Ext.getCmp('no' + a.id).collapse(true);
            //    }
            //},
        }, btnPanel],
    });
    //attId++;
    win.show();
    //win.items.items[1].collapse(true);
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
    var textbox;
    if (attData.ValueType == 0)
        textbox = new Ext.form.NumberField({
            fieldLabel: attData.AttributeItemName,
            attId: attData.AttributeItemId,
            allowDecimals: false, // 允许小数点
            allowNegative: false, // 允许负数
            allowBlank: false,
            value: attData.DefaultValue,
            maxLength: 50,
            margin: '5 10 5 10',
            columnWidth: .5,
            labelWidth: 60,
        });
    else
        textbox = new Ext.form.TextField({
            fieldLabel: attData.AttributeItemName,
            attId: attData.AttributeItemId,
            allowBlank: false,
            value: attData.DefaultValue,
            margin: '5 10 5 10',
            columnWidth: .5,
            labelWidth: 60,
        });
    return textbox;
}