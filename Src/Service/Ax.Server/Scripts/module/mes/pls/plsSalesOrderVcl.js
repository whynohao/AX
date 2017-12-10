plsSalesOrderVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    this.dataRow;
}
Ext.tip.QuickTipManager.init();
//组合品每个Panel唯一标识
var indexid = 0;
var attId = 0;
//组合品总Panel
var newPanel;
var BIZATTR = '1';

var proto = plsSalesOrderVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = plsSalesOrderVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var headStore = this.dataSet.getTable(0).data.items[0];
    var parent = this;

    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.fieldName == "SALESORDERDETAILSUB")
                e.dataInfo.cancel = true;
            switch (e.dataInfo.fieldName) {
                case "ATTRIBUTENAME":
                    var MaterialId = e.dataInfo.dataRow.data["MATERIALID"];
                    var AttributeId = e.dataInfo.dataRow.data["ATTRIBUTEID"];
                    var AttributeCode = e.dataInfo.dataRow.data["ATTRIBUTECODE"]
                    var MaterialId = e.dataInfo.dataRow.data["MATERIALID"];

                    if (MaterialId == "") {
                        AttributeId = this.invorkBcf("GetDefaultAttr");
                    }
                    if (AttributeId != "") {
                        var returnData = this.invorkBcf('GetAttJson', [MaterialId, AttributeId, AttributeCode]);
                        var dataList = {
                            MaterialId: e.dataInfo.dataRow.data["MATERIALID"],
                            AttributeId: AttributeId,
                            AttributeDesc: e.dataInfo.dataRow.data["ATTRIBUTEDESC"],
                            AttributeCode: e.dataInfo.dataRow.data["ATTRIBUTECODE"],
                            BillNo: e.dataInfo.dataRow.data["BILLNO"],
                            Row_Id: e.dataInfo.dataRow.data["ROW_ID"]

                        };
                        CreatAttForm(dataList, returnData, this, e, FillDataRow);

                    }
                    break;
                case "MATERIALNAME":
                    if (this.billAction == BillActionEnum.Modif || this.billAction == BillActionEnum.AddNew) {
                        var thisWin = Ext.getCmp("win" + e.dataInfo.dataRow.data["BILLNO"] + e.dataInfo.dataRow.data["ROW_ID"] + e.dataInfo.dataRow.data["MATERIALID"]);
                        if (thisWin != undefined) {
                            thisWin.show();
                        }
                        //将子子表反填为组合品窗体用store
                        var store = [];

                        var childTable = this.dataSet.getTable(2);
                        if (childTable.data.items.length > 0) {
                            newPanel = new Ext.form.Panel({
                                layout: 'vbox',
                                margin: '0 0 0 0',
                                width: '100%',
                                height: 250,
                                autoScroll: true,
                            });



                            //每条明细对应一个data
                            var data;
                            for (var i = 0; i < childTable.data.items.length; i++) {
                                var datarow = childTable.data.items[i];
                                if (datarow.data["PARENTROWID"] == e.dataInfo.dataRow.data["ROW_ID"]) {
                                    data = {
                                        AttributeId: datarow.data["ATTRIBUTECODE"],
                                        MaterialId: datarow.data["MATERIALID"],
                                        MaterialtypeName: datarow.data["MATERIALTYPENAME"],
                                        MaterialtypeId: datarow.data["MATERIALTYPEID"],
                                        MaterialName: datarow.data["MATERIALNAME"],
                                        BillNo: e.dataInfo.dataRow.data["BILLNO"],
                                        RowId: e.dataInfo.dataRow.data["ROW_ID"],
                                        Quantity: datarow.data["QUANTITY"],
                                        IsNotAdd: false,
                                        AttributeCode: datarow.data["ATTRIBUTECODE"],
                                        AttributeDesc: datarow.data["ATTRIBUTEDESC"]
                                    }

                                    store.push(data);
                                }
                            }
                        }
                        if (store.length == 0) {
                            var returnData = this.invorkBcf('GetData', [e.dataInfo.dataRow.data["MATERIALID"]]);
                            if (returnData.length != 0) {

                                newPanel = new Ext.form.Panel({
                                    layout: 'vbox',
                                    margin: '0 0 0 0',
                                    width: '100%',
                                    height: 250,
                                    autoScroll: true,
                                })
                                ResetPanel(returnData, e, this);
                                FormWin(returnData, e, this);
                            }

                        }
                        else {
                            ResetPanel(store, e, this);
                            FormWin(store, e, this);
                        }
                        break;
                    }
            }

        case LibEventTypeEnum.Validating:

            switch (e.dataInfo.fieldName) {
                case "ISCONFIRM":
                    e.dataInfo.dataRow.set("ISCONFIRM", e.dataInfo.dataRow['ISCONFIRM']);
                    break;
                case "LASTESTDATE":
                    if (e.dataInfo.dataRow.data["CUSTOMERDATE"] > 0) {
                        e.dataInfo.cancel = true;
                    }
            }

            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                switch (e.dataInfo.fieldName) {
                    case "TYPEID":
                        BIZATTR = e.dataInfo.dataRow.data["BIZATTR"];
                        if (BIZATTR == '1' && Ext.getCmp("CUSTOMERDATE0_" + this.winId).value == '0') {

                            CalculateLastestdate(this);
                        }
                        if (BIZATTR == '0' || Ext.getCmp("CUSTOMERDATE0_" + this.winId).value != '0') {
                            Ext.getCmp("BIZATTR0_" + this.winId).setValue('0');
                            Ext.getCmp("ORDERDATES0_" + this.winId).setValue('0');
                        }
                        this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                        break;
                    case "CUSTOMERDATE":
                        if (BIZATTR == '1') {
                            if (e.dataInfo.value != "") {
                                Ext.getCmp("BIZATTR0_" + this.winId).setValue('0');
                                Ext.getCmp("ORDERDATES0_" + this.winId).setValue('0');
                                Ext.getCmp("LASTESTDATE0_" + this.winId).setValue(e.dataInfo.value);
                                this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                            }
                            if (e.dataInfo.value == "") {
                                Ext.getCmp("BIZATTR0_" + this.winId).setValue(BIZATTR);
                                Ext.getCmp("LASTESTDATE0_" + this.winId).setValue(e.dataInfo.value);
                                this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                            }
                        }
                        else {
                            Ext.getCmp("LASTESTDATE0_" + this.winId).setValue(e.dataInfo.value);
                            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                        }
                        break;
                    case "ORDERDATES":
                        if (e.dataInfo.dataRow.data["BIZATTR"] != '0') {

                            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                            CalculateLastestdate(this);
                        }
                        else {
                            Ext.getCmp("ORDERDATES0_" + this.winId).setValue('0');
                        }
                        break;
                    case "SINGLEDATE":
                        if (e.dataInfo.dataRow.data["CUSTOMERDATE"] == '') {
                            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                            CalculateLastestdate(this);
                        }
                        break;
                    case "SENDMODEL":
                        if (e.dataInfo.value == 0) {
                            Ext.getCmp("PLANDATE0_" + this.winId).setValue(0);
                            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                        }
                        else {
                            Ext.getCmp("PLANDATE0_" + this.winId).setValue(e.dataInfo.dataRow.data["LASTESTDATE"]);
                            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                        }
                        break;
                    case "PLANDATE":
                        //if (e.dataInfo.value != "") {
                        //    Ext.getCmp("SENDMODEL0_" + this.winId).setValue(1);
                        //    this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                        //}
                        if (e.dataInfo.dataRow.data["SENDMODEL"] == 0) {
                            Ext.getCmp("PLANDATE0_" + this.winId).setValue(0);
                            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                        }
                        break;


                }
            }
            else if (e.dataInfo.tableIndex == 1) {
                switch (e.dataInfo.fieldName) {
                    case "MATERIALID":
                        if (e.dataInfo.value != e.dataInfo.oldValue && e.dataInfo.oldValue != "") {
                            if (e.dataInfo.dataRow.data["ATTRIBUTEID"] == "") {
                                e.dataInfo.dataRow.set("ATTRIBUTENAME", "");
                            }
                            e.dataInfo.dataRow.set("SALEBOMID", "");
                            e.dataInfo.dataRow.set("SALEBOMNAME", "");
                            e.dataInfo.dataRow.set("ISEXCEPTION", false);
                            e.dataInfo.dataRow.set("ABNORMALDAY", 0);
                            e.dataInfo.dataRow.set("ATTRIBUTECODE", "");
                            e.dataInfo.dataRow.set("ATTRIBUTEDESC", "");
                        }

                        var materialId = e.dataInfo.value;
                        e.dataInfo.dataRow.set("MATERIALID", materialId);
                        var datarow = e.dataInfo.dataRow;
                        var materialName = datarow.data["MATERIALNAME"];
                        var unitId = datarow.data["UNITID"];
                        var unitName = datarow.data["UNITNAME"];
                        var quantity = datarow.data["QUANTITY"];

                        if (materialId != '') {
                            var thisWin = Ext.getCmp("win" + e.dataInfo.dataRow.data["BILLNO"] + e.dataInfo.dataRow.data["ROW_ID"] + e.dataInfo.dataRow.data["MATERIALID"]);
                            if (thisWin == undefined) {
                                var returnData = this.invorkBcf('GetData', [materialId]);
                                if (returnData.length != 0) {

                                    newPanel = new Ext.form.Panel({
                                        layout: 'vbox',
                                        margin: '0 0 0 0',
                                        width: '100%',
                                        height: 250,
                                        autoScroll: true,
                                    })
                                    indexid = 0;
                                    ResetPanel(returnData, e, this);
                                    FormWin(returnData, e, this);

                                }
                                else {
                                    var BodyTable = this.dataSet.getTable(2);
                                    for (var i = 0; i < BodyTable.data.items.length; i++) {
                                        if (BodyTable.data.items[i].data["PARENTROWID"] == e.dataInfo.dataRow.data["ROW_ID"]) {
                                            BodyTable.remove(BodyTable.data.items[i]);
                                            i--;
                                        }
                                    }
                                    e.dataInfo.dataRow.set("SALESORDERDETAILSUB", false);
                                }
                            }
                            else {

                                thisWin.show();
                                newPanel = thisWin.items.items[0];
                            }
                        }
                        break;
                }
            }
    }

}

//填充当前行特征信息
function FillDataRow(e, This, CodeDesc) {
    e.dataInfo.dataRow.set("ATTRIBUTECODE", CodeDesc.Code);
    e.dataInfo.dataRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
    e.dataInfo.dataRow.set("ABNORMALDAY", CodeDesc.AbnormalDay);
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
    panel.day = CodeDesc.AbnormalDay;
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
                //unstandard.push(CheckBox(returnData[i]));//复选框
                unstandard.push(CreatTextBox(returnData[i], isRead));
                
            }
            else {
                //standard.push(CheckBox(returnData[i]));
                standard.push(CreatTextBox(returnData[i], isRead));
                
            }
        }
        else {
            if (returnData[i].Standard) {
                //unstandard.push(CheckBox(returnData[i]));
                unstandard.push(CreatComBox(returnData[i], isRead));
                
            }
            else {
                //standard.push(CheckBox(returnData[i]));
                standard.push(CreatComBox(returnData[i], isRead));
                
            }
        }
    }
 
    //标准特征Panel
    var attPanel = new Ext.form.Panel({
        
    });
    
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

                var attPanel = thisWin.items.items[0].items.items[0];
                var unattPanel = thisWin.items.items[0].items.items[1];
                var attributeId = thisWin.attributeId;
                var materialId = thisWin.materialId;
                var attDic = [];
                var msg = '';
                for (var i = 0; i < attPanel.items.length; i++) {
                    if (returnData[i].IsRequired == 1 && (attPanel.items.items[i].value == null || attPanel.items.items[i].value == "")) {
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
        //layout: 'column',
        width: '100%',
        collapse:false,
        defaults: {
            margin: '20 40 0 300',//上右下左
            columnWidth: .5
        },
        items: [btnSaleConfirm, btnSaleCancel]
    })
    var classPanel = new Ext.form.Panel({
        layout: 'column',
        width: '100%',
        height: 560,
        items: [{
            id: 'Att' + attId,
            layout: 'column',
            xtype: 'fieldset',
            title: '标准特征',
            //collapsed: true,
            collapsible: true,
            width: '96%',
            height: 530,//200

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
            //collapse: true,
            collapsible: true,
            width: '96%',
            height: 500,
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
        }]
    })
    var Salewin = new Ext.create('Ext.window.Window', {
        id: "attWin" + BillNo + Row_Id + MaterialId,
        title: '特征信息',
        resizable: false,
        //closeAction: "close",
        modal: true,
        width: 1400,
        height: 670,//330
        materialId: MaterialId,//物料ID
        attributeId: AttributeId,//特征ID
        autoScroll: true,
        //layout: 'column',
        items: [classPanel, btnSalePanel],
    });
    attId++;
    Salewin.show();
    Salewin.items.items[1].collapse(true);
    for (var i = 0; i < returnData.length; i++) {
        if (returnData[i].Remarks!="") {
        Ext.QuickTips.register({
            target: 'Remarks' + returnData[i].AttributeItemId + '-labelEl',//给填写了备注的特征项元素注册提示信息  
            text: returnData[i].Remarks
        })
    }
}
}
//function CheckBox(attData)
//{
//    var checkbox = new Ext.form.Checkbox({
//        // xtype: "checkboxfield",
//        id: "checkbox"+attData.AttributeItemId,
//        name: "checkbox",
//        //checked: true，
//        //fieldLabel: "爱好",
//        //boxLabel: "散步"
//    });

//    return checkbox;
//}
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
        id: "Remarks" + attData.AttributeItemId,
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
            id: "Remarks" + attData.AttributeItemId,
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
            id: "Remarks" + attData.AttributeItemId,
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
                allowDecimals: true, // 允许小数点
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
    if (single > 0) {
        var lastestDate = This.invorkBcf('GetLastest', [single, order]);
        Ext.getCmp("LASTESTDATE0_" + This.winId).setValue(lastestDate);
    }
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
