comAttributeFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = comAttributeFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comAttributeFuncVcl;

proto.doSetParam = function (vclObj) {
    attributeId = vclObj[0];
    proto.winId = vclObj[1].winId;
    proto.fromObj = vclObj[1];
    //var masterRow = this.dataSet.getTable(0).data.items[0];
    //this.forms[0].loadRecord(masterRow);
};
var attId = 0;
var attributeId;

proto.doSetParam = function () {
    //var masterRow = this.dataSet.getTable(0).data.items[0];
    this.fillData(this, "");
};

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.fieldName == "ATTRIBUTENAME") {
                var masterrow = e.dataInfo.dataRow;
                var MaterialId = masterrow.data["MATERIALID"];
                var AttributeId = masterrow.data["ATTRIBUTEID"];
                var AttributeCode = masterrow.data["ATTRIBUTECODE"]
                if (AttributeId != "") {
                    var returnData = this.invorkBcf('GetAttJson', [MaterialId, AttributeId, AttributeCode]);
                    var dataList = {
                        MaterialId: masterrow.data["MATERIALID"],
                        AttributeId: masterrow.data["ATTRIBUTEID"],
                        AttributeDesc: masterrow.data["ATTRIBUTEDESC"],
                        AttributeCode: masterrow.data["ATTRIBUTECODE"],
                        BillNo: masterrow.data["BILLNO"],
                        Row_Id: masterrow.data["ROW_ID"]
                    };
                    this.CreatAttForm(dataList, returnData, this, masterrow, this.FillDataRow);
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName = "MATERIALID") {
                    this.fillData(this, e.dataInfo.dataRow.get("MATERIALID"));
                    //this.forms[0].updateRecord(e.dataInfo.dataRow);
                }
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnSure") {
                var materialList = [];
                var bodyTable = this.dataSet.getTable(1).data.items;
                for (var i = 0; i < bodyTable.length; i++) {
                    materialList.push({
                        MaterialId: bodyTable[i].data["MATERIALID"],
                        MaterialName: bodyTable[i].data["MATERIALNAME"],
                        MaterialSpec: bodyTable[i].data["MATERIALSPEC"],
                        Specfication: bodyTable[i].data["SPECFICATION"],
                        FigureNo: bodyTable[i].data["FIGURENO"],
                        TexttureId: bodyTable[i].data["TEXTTUREID"],
                        AttributeId: bodyTable[i].data["ATTRIBUTEID"],
                        AttributeName: bodyTable[i].data["ATTRIBUTENAME"],
                        AttributeCode: bodyTable[i].data["ATTRIBUTECODE"],
                        AttributeDesc: bodyTable[i].data["ATTRIBUTEDESC"],
                    });
                }
                this.invorkBcf("SureMaterialAttribute", [materialList]);
                this.win.close();
            }
            break;
    }
}
proto.fillData = function (This, materialId) {
    var rerurnList = This.invorkBcf("GetData", ["1", materialId]);

    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var list = rerurnList;
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('MATERIALSPEC', info.MaterialSpec);
                newRow.set('SPECIFICATION', info.Specfication);
                newRow.set('TEXTUREID', info.TexttureId);
                newRow.set('FIGURENO', info.FigureNo);
                newRow.set('ATTRIBUTEID', info.AttributeId);
                newRow.set('ATTRIBUTENAME', info.AttributeName);
                newRow.set('ATTRIBUTECODE', info.AttributeCode);
                newRow.set('ATTRIBUTEDESC', info.AttributeDesc);
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
proto.FillDataRow = function (e, This, CodeDesc) {
    e.set("ATTRIBUTECODE", CodeDesc.Code);
    e.set("ATTRIBUTEDESC", CodeDesc.Desc);
    //This.forms[0].loadRecord(e);
    return true;
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
            //if (This.billAction == BillActionEnum.Modif || This.billAction == BillActionEnum.AddNew) {

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
            //}
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