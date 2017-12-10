purPurchasePlanVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};

var attId = 0;
var proto = purPurchasePlanVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = purPurchasePlanVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                var store = this.dataSet.getTable(e.dataInfo.tableIndex);
                var length = store.data.items.length;
                if (e.dataInfo.fieldName == "BILLSTATE") {
                    if (e.dataInfo.value == 1 && e.dataInfo.value != e.dataInfo.oldValue) {
                        var nonstockQTY = e.dataInfo.dataRow.get("NONSTOCKQTY");
                        e.dataInfo.dataRow.set("CANCELQTY", nonstockQTY);
                        e.dataInfo.dataRow.set("NONSTOCKQTY", 0);
                    }
                    else if (e.dataInfo.value == 0 && e.dataInfo.value != e.dataInfo.oldValue) {
                        var cancelQTY = e.dataInfo.dataRow.get("CANCELQTY");
                        e.dataInfo.dataRow.set("NONSTOCKQTY", cancelQTY);
                        e.dataInfo.dataRow.set("CANCELQTY", 0);
                    }
                }
                else if (e.dataInfo.fieldName == "QUANTITY") {
                    var instockQTY = e.dataInfo.dataRow.get("INSTOCKQTY");
                    var quantity = e.dataInfo.value;
                    var nonStockQTY = quantity - instockQTY;
                    e.dataInfo.dataRow.set("NONSTOCKQTY", nonStockQTY);
                }
                if (e.dataInfo.fieldName == "MATERIALID") {
                    if (e.dataInfo.value){
                        var materialId = e.dataInfo.value;
                        var contactsObjectId = e.dataInfo.dataRow.get("SUPPLIERID");
                        if (materialId && contactsObjectId) {
                            var list = this.invorkBcf("SelectMaterialSupplier", [materialId, contactsObjectId]);
                            if (list[0]) {
                                //if (list[0]["Price"]) {
                                //    e.dataInfo.dataRow.set("PRICE", list[0]["Price"]);
                                //}
                                if (list[0]["ShortLeadTime"]) {
                                    e.dataInfo.dataRow.set("SHORTLEADTIME", list[0]["ShortLeadTime"]);
                                }
                            }
                            else {
                                e.dataInfo.dataRow.set("SHORTLEADTIME", 0);
                            }
                        }
                    }
                    else {
                        e.dataInfo.dataRow.set("SHORTLEADTIME", 0);
                    }
                }
                if (e.dataInfo.fieldName == "SUPPLIERID") {
                    if (e.dataInfo.value) {
                        var materialId = e.dataInfo.dataRow.get("MATERIALID");
                        var contactsObjectId = e.dataInfo.value;
                        if (materialId && contactsObjectId) {
                            var list = this.invorkBcf("SelectMaterialSupplier", [materialId, contactsObjectId]);
                            if (list[0]) {
                                //if (list[0]["Price"]) {
                                //    e.dataInfo.dataRow.set("PRICE", list[0]["Price"]);
                                //}
                                if (list[0]["ShortLeadTime"]) {
                                    e.dataInfo.dataRow.set("SHORTLEADTIME", list[0]["ShortLeadTime"]);
                                }
                            }
                            else {
                                e.dataInfo.dataRow.set("SHORTLEADTIME", 0);
                            }
                        }
                    }
                    else {
                        e.dataInfo.dataRow.set("SHORTLEADTIME", 0);
                    }
                }
            }
            if (e.dataInfo.tableIndex == 0) {
                var bodyTable = this.dataSet.getTable(1);
                var masterRow = this.dataSet.getTable(0).data.items[0];
                if (e.dataInfo.fieldName = "SUPPLIERID") {
                    var supplierId = e.dataInfo.value;
                    var supplierName = e.dataInfo.dataRow.get("SUPPLIERNAME");
                    if (bodyTable.data.items.length > 0) {
                        if (e.dataInfo.value) {
                            for (var i = 0; i < bodyTable.data.items.length; i++) {
                                bodyTable.data.items[i].set("SUPPLIERID", supplierId);
                                bodyTable.data.items[i].set("SUPPLIERNAME", supplierName);

                                var materialId = this.dataSet.getTable(1).data.items[i].data["MATERIALID"];
                                var contactsObjectId = this.dataSet.getTable(1).data.items[i].data["SUPPLIERID"];
                                if (materialId && contactsObjectId) {
                                    var list = this.invorkBcf("SelectMaterialSupplier", [materialId, contactsObjectId]);
                                    if (list[0]) {
                                        //if (list[0]["Price"]) {
                                        //    this.dataSet.getTable(1).data.items[i].set("PRICE", list[0]["Price"]);
                                        //}
                                        if (list[0]["ShortLeadTime"]) {
                                            this.dataSet.getTable(1).data.items[i].set("SHORTLEADTIME", list[0]["ShortLeadTime"]);
                                        }
                                    }
                                    else {
                                        bodyTable.data.items[i].set("SHORTLEADTIME", 0);
                                    }
                                }
                            }
                        }
                        else {
                            for (var i = 0; i < bodyTable.data.items.length; i++) {
                                bodyTable.data.items[i].set("SUPPLIERID", "");
                                bodyTable.data.items[i].set("SUPPLIERNAME", "");
                                bodyTable.data.items[i].set("SHORTLEADTIME", 0);
                            }
                        }
                    }
                }
            }
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "QUANTITY") {
                    var instockQTY = e.dataInfo.dataRow.get("INSTOCKQTY");
                    var quantity = e.dataInfo.value;
                    if (quantity < instockQTY) {
                        Ext.Msg.alert("提示","计划数量不能小于已入库数量");
                        e.dataInfo.cancel = true;
                    }
                }
            }
            if (e.dataInfo.tableIndex == 0) {
                var bodyTable = this.dataSet.getTable(1);
                var masterRow = this.dataSet.getTable(0).data.items[0];
                this.forms[0].updateRecord(masterRow);
                if (e.dataInfo.fieldName == "PERSONID") {
                    var personId = masterRow.get('PERSONID');
                    var personName = masterRow.get('PERSONNAME');
                    if (bodyTable.data.items.length > 0) {
                        for (var i = 0; i < bodyTable.data.items.length; i++) {
                            bodyTable.data.items[i].set("PERSONID", personId);
                            bodyTable.data.items[i].set("PERSONNAME", personName);
                        }
                    }
                }
            }
            break;
        case LibEventTypeEnum.AddRow:
            if (e.dataInfo.tableIndex == 1) {
                var masterRow = this.dataSet.getTable(0).data.items[0];
                this.forms[0].updateRecord(masterRow);
                var personId = masterRow.get('PERSONID');
                var personName = masterRow.get('PERSONNAME');
                e.dataInfo.dataRow.set("PERSONID", personId);
                e.dataInfo.dataRow.set("PERSONNAME", personName);
                //e.dataInfo.dataRow.set("SPLITNO", e.dataInfo.dataRow.get("ROW_ID"));
                var supplierId = masterRow.get("SUPPLIERID");
                var supplierNam = masterRow.get("SUPPLIERNAME");
                e.dataInfo.dataRow.set("SUPPLIERID", supplierId);
                e.dataInfo.dataRow.set("SUPPLIERNAME", supplierNam);
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                var dt = this.dataSet.getTable(1);
                for (var i = 0; i < dt.data.items.length; i++) {
                    var info = dt.data.items[i];
                    if (info.data["PURCHASEORDER"] != "") {
                        Ext.Msg.alert("系统提示", "行标识为：" + info.data["ROW_ID"] + "的数据已下订单，不能删除！");
                        e.dataInfo.cancel = true;
                    }
                }
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "btnSplitPlan") {
                var grid = Ext.getCmp(this.winId + 'PURPURCHASEPLANDETAILGrid'); //要加载数据的表名字 + Grid
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length != 1) {
                    Ext.Msg.alert("系统提示", "请选择一条数据进行拆分");
                    return;
                }
                else {
                    if (records[0].data["BILLSTATE"] == 1) {//状态
                        Ext.Msg.alert("系统提示", "该条数据的状态为已完成，不能拆分！");
                        return;
                    }
                    else if (records[0].data["PURCHASEORDER"] != "") {
                        Ext.Msg.alert("系统提示", "该条数据已存在订单号，不能拆分！");
                        return;
                    }
                    Ax.utils.LibVclSystemUtils.openDataFunc("pur.PurchasePlanDetail", "拆分明细", [this]);
                }
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
            if (e.dataInfo.fieldName == "ATTACHMENTSRC") {
                var table = this.dataSet.getTable(1);
                Ax.utils.LibAttachmentForm.show(vcl, table.data.items[0], table.Name);
            }
            break;
    }

}

//function FillData(list) {
//    Ext.suspendLayouts();//关闭Ext布局
//    var curStore = this.dataSet.getTable(1);
//    curStore.suspendEvents();//关闭store事件
//    try {
//        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
//        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据   
//        if (list != undefined && list.length > 0) {
//            for (var i = 0; i < list.length; i++) {
//                var info = list[i];
//                var newRow = this.addRow(masterRow, 1);
//                newRow.set("FROMBILLNO", info.frombillno);
//                newRow.set("FROMROW_ID", info.fromrowid);
//                newRow.set("MATERIALID", info.materialid);
//                newRow.set("MATERIALNAME", info.materialname);
//                newRow.set("MATERIALSPEC", info.materialspec);
//                newRow.set("ATTRIBUTEID", info.attributeid);
//                newRow.set("ATTRIBUTENAME", info.attributename);
//                newRow.set("ATTRIBUTECODE", info.attributecode);
//                newRow.set("ATTRIBUTEDESC", info.attributedesc);
//                newRow.set("UNITID", info.unitid);
//                newRow.set("UNITNAME", info.unitname);
//                newRow.set("DEALSUNITID", info.dealsunitid);
//                newRow.set("DEALSUNITNO", info.dealsunitno);
//                newRow.set("ISCHECK", info.ischeck);
//                newRow.set("PREPAREDATE", info.preparedate);
//                newRow.set("MATERIALTYPEID", info.materialtypeid);
//                newRow.set("MATERIALTYPENAME", info.materialtypename);
//                newRow.set("DEALSUNITNAME", info.dealsunitname);
//                newRow.set("TEXTUREID", info.textureid);
//                newRow.set("SPECIFICATION", info.specification);
//                newRow.set("FIGURENO", info.figureno);
//                newRow.set("DEALSQUANTITY", info.dealsquantity);
//                newRow.set("QUANTITY", info.quantity);
//                newRow.set("PRICE", info.price);
//                newRow.set("AMOUNT", info.amount);
//                newRow.set("FROMTYPE", info.fromtype);
//            }
//        }
//        else {
//            Ext.Msg.alert("提示", "该来源单号没有可引用行！");
//        }
//    } finally {
//        curStore.resumeEvents();//打开store事件
//        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
//            curStore.ownGrid.reconfigure(curStore);
//        Ext.resumeLayouts(true);//打开Ext布局
//    }
//}

//填充本表的数据
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
