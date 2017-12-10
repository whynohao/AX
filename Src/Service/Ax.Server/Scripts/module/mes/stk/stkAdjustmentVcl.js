

stkAdjustmentVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = stkAdjustmentVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = stkAdjustmentVcl;
var attId = 0;


function getInOutRecord(masterRow, dataSet) {
    Ext.suspendLayouts();//关闭Ext布局 
    try {
        for (var i = 0; i < dataSet.STKADJUSTMENTDETAIL.length; i++) {
            var newRow = this.addRow(masterRow, 1);
            record = dataSet.STKADJUSTMENTDETAIL[i];
            newRow.set('FROMROWID', record.FROMROWID);
            newRow.set('MATERIALID', record.MATERIALID);
            newRow.set('MATERIALNAME', record.MATERIANAME);
            newRow.set('WAREHOUSEID', record.WAREHOUSEID);
            newRow.set('WAREHOUSENAME', record.WAREHOUSENAME);
            newRow.set('BATCHNO', record.BATCHNO);
            newRow.set('SUBBATCHNO', record.SUBBATCHNO);
            newRow.set('MTONO', record.MTONO);
            newRow.set('COMPLETENO', record.COMPLETENO);
            newRow.set('STKATTR', record.STKATTR);
            newRow.set('STKSTATE', record.STKSTATE);
            newRow.set('STORAGEID', record.STORAGEID);
            newRow.set('STORAGENAME', record.STORAGENAME);
            newRow.set('OLDQUANTITY', record.OLDQUANTITY);
            newRow.set('UNITDETAIL', record.OLDQUANTITY);
            newRow.set('SERIALNODETAIL', record.OLDQUANTITY);
            newRow.set('ATTRIBUTECODE', record.ATTRIBUTECODE);
            newRow.set('ATTRIBUTEDESC', record.ATTRIBUTEDESC);
            newRow.set('UNITID', record.UNITID);
            newRow.set('UNITNAME', record.UNITNAME);
            newRow.set('CONTACTOBJECTID', record.CONTACTOBJECTID);
            newRow.set('CONTACTSOBJECTNAME', record.CONTACTSOBJECTNAME);
        }
    }
    finally {
        Ext.resumeLayouts(true);//打开Ext布局
    }
};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (this.isEdit) {
                if (e.dataInfo.fieldName == "BtnLoadData") {
                    var companyId = this.dataSet.getTable(0).data.items[0].data['COMPANYID'];
                    //var contactsObjectId = this.dataSet.getTable(0).data.items[0].data['CONTACTOBJECTID'];
                    if (companyId == "") {
                        alert("公司不能为空！");
                    }
                    //else if (contactsObjectId == "") {
                    //    alert("往来单位不能为空！");
                    //}
                    else {
                        this.deleteAll(1)
                        Ax.utils.LibVclSystemUtils.openDataFunc('stk.AdjustmentQuery', '调整单数据查询', [companyId, this]);
                    }
                }
            }
            else {
                alert("单据只有在修改状态才能载入调整单数据！");
            }
            break;
        case LibEventTypeEnum.Validated:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            this.forms[0].updateRecord(masterRow);
            if (e.dataInfo.tableIndex == 0) {
                var fieldName = e.dataInfo.fieldName;
                var billno = e.dataInfo.dataRow.get('BILLNO');
                if (fieldName == "FROMBILLNO") {
                    var fromBillNo = e.dataInfo.value;
                    var dataSet = this.invorkBcf('GetInOutRecordInfo', [fromBillNo, billno]);
                    this.dataSet.getTable(1).removeAll();
                    this.dataSet.getTable(2).removeAll();
                    this.dataSet.getTable(3).removeAll();
                    for (var i = 0; i < dataSet.STKADJUSTMENTDETAIL.length; i++) {
                        var newRow = this.addRow(masterRow, 1);
                        record = dataSet.STKADJUSTMENTDETAIL[i];
                        newRow.set('FROMBILLNO', record.FROMBILLNO);
                        newRow.set('FROMROWID', record.FROMROWID);
                        newRow.set('STKTYPE', record.STKTYPE);
                        newRow.set('MATERIALID', record.MATERIALID);
                        newRow.set('MATERIALNAME', record.MATERIALNAME);
                        newRow.set('WAREHOUSEID', record.WAREHOUSEID);
                        newRow.set('WAREHOUSENAME', record.WAREHOUSENAME);
                        newRow.set('COMPANYID', record.COMPANYID);
                        newRow.set('COMPANYNAME', record.COMPANYNAME);
                        newRow.set('BATCHNO', record.BATCHNO);
                        newRow.set('SUBBATCHNO', record.SUBBATCHNO);
                        newRow.set('MTONO', record.MTONO);
                        newRow.set('COMPLETENO', record.COMPLETENO);
                        newRow.set('STKATTR', record.STKATTR);
                        newRow.set('STKSTATE', record.STKSTATE);
                        newRow.set('STKSTATENAME', record.STKSTATENAME);
                        newRow.set('STORAGEID', record.STORAGEID);
                        newRow.set('STORAGENAME', record.STORAGENAME);
                        newRow.set('OLDQUANTITY', record.OLDQUANTITY);
                        newRow.set('UNITDETAIL', record.UNITDETAIL);
                        newRow.set('SERIALNODETAIL', record.SERIALNODETAIL);
                        newRow.set('ATTRIBUTECODE', record.ATTRIBUTECODE);
                        newRow.set('ATTRIBUTEDESC', record.ATTRIBUTEDESC);
                        newRow.set('UNITID', record.UNITID);
                        newRow.set('CONTACTOBJECTID', record.CONTACTOBJECTID);
                        newRow.set('CONTACTSOBJECTNAME', record.CONTACTSOBJECTNAME);
                    }
                }
                else if (fieldName == "STKMOVETYPE") {
                    if (e.dataInfo.value != null) {
                        var detailtable = this.dataSet.getTable(1);
                        for (var i = 0 ; i < detailtable.data.items.length; i++) {
                            detailtable.data.items[i].set("QUANTITY", 0);
                            detailtable.data.items[i].set("AFTERADJUSTQTY", 0);
                        }
                    }
                }
            }
            else if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.value != null) {
                    switch (e.dataInfo.fieldName) {
                        case 'MATERIALID'://仓库改变带出库存状态
                            if (e.dataInfo.dataRow.get("WAREHOUSEID") != "") {
                                var stkState = this.invorkBcf("GetStkState", [e.dataInfo.dataRow.get("WAREHOUSEID")]);
                                e.dataInfo.dataRow.set("STKSTATE", stkState);
                            }
                            break;
                    }
                }
            }
            else if (e.dataInfo.tableIndex == 2) {
                if (e.dataInfo.value != null) {
                    switch (e.dataInfo.fieldName) {
                        case 'STKUNITID'://仓储单位改变
                            //检查仓储单位是否重复出现
                            var subTable = this.dataSet.getTable(2);
                            var existCnt = 0;
                            var stkunitno = e.dataInfo.dataRow.get("STKUNITNO");
                            if (stkunitno != "") {
                                for (var i = 0; i < subTable.data.items.length; i++) {
                                    if (e.dataInfo.value == subTable.data.items[i].data.STKUNITID && stkunitno == subTable.data.items[i].data.STKUNITNO) {
                                        existCnt += 1;
                                    }
                                }
                            }
                            if (existCnt > 0) {
                                Ext.Msg.alert('提示', '仓储单位："' + e.dataInfo.value + '",单位标识："' + stkunitno + '"已经存在于该列表！');
                                return;
                            }
                            //单位换算引发数量变化
                            if (e.dataInfo.value != e.dataInfo.dataRow.data["STKUNITID"]) {
                                e.dataInfo.dataRow.set("STKUNITNO", null);
                                e.dataInfo.dataRow.set("SQUANTITY", 1);
                                e.dataInfo.dataRow.set("QUANTITY", 1);
                            }
                            break;
                        case 'STKUNITNO':
                            var subTable = this.dataSet.getTable(2);
                            var existCnt = 0;
                            var stkunitid = e.dataInfo.dataRow.get("STKUNITID")
                            if (stkunitid != "") {
                                for (var i = 0; i < subTable.data.items.length; i++) {
                                    if (e.dataInfo.value == subTable.data.items[i].data.STKUNITNO && stkunitid == subTable.data.items[i].data.STKUNITID) {
                                        existCnt += 1;
                                    }
                                }
                            }
                            if (existCnt > 1) {
                                Ext.Msg.alert('提示', '仓储单位："' + stkunitid + '",单位标识："' + e.dataInfo.value + '"已经存在于该列表！');
                                return;
                            }
                            var unitData = this.invorkBcf("GetData", [e.dataInfo.curGrid.parentRow.get("MATERIALID"), e.dataInfo.dataRow.get("STKUNITID"), e.dataInfo.value, 0, e.dataInfo.dataRow.get("SQUANTITY"), e.dataInfo.curGrid.parentRow.get("UNITID"), 0]);
                            e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                            break;
                        case 'QUANTITY'://调整基本数量改变 
                            var unitData = this.invorkBcf("GetData", [e.dataInfo.curGrid.parentRow.get("MATERIALID"), e.dataInfo.dataRow.get("STKUNITID"), e.dataInfo.dataRow.get("STKUNITNO"), e.dataInfo.value, 0, e.dataInfo.curGrid.parentRow.get("UNITID"), 1]);
                            e.dataInfo.dataRow.set("SQUANTITY", unitData.ConverQuantity);
                            break;
                        case 'SQUANTITY'://调整仓储数量改变
                            var unitData = this.invorkBcf("GetData", [e.dataInfo.curGrid.parentRow.get("MATERIALID"), e.dataInfo.dataRow.get("STKUNITID"), e.dataInfo.dataRow.get("STKUNITNO"), 0, e.dataInfo.value, e.dataInfo.curGrid.parentRow.get("UNITID"), 0]);
                            e.dataInfo.dataRow.set("QUANTITY", unitData.Quantity);
                            break;
                    }
                }
            }
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                var masterRow = this.dataSet.getTable(0).data.items[0];
                var oldquantity = e.dataInfo.dataRow.get("OLDQUANTITY");
                var stkmovetype = masterRow.data["STKMOVETYPE"];
                var stktype = e.dataInfo.dataRow.get("STKTYPE");
                switch (e.dataInfo.fieldName) {
                    case 'QUANTITY':
                        if (stkmovetype == "" || stkmovetype == undefined) {
                            Ext.Msg.alert('提示', '请先选择库存移动类型！');
                            e.dataInfo.cancel = true;
                            return;
                        } else {
                            if (stkmovetype == 405) {
                                if (stktype == 0) {
                                    e.dataInfo.dataRow.set("AFTERADJUSTQTY", oldquantity + e.dataInfo.value);
                                } else {
                                    e.dataInfo.dataRow.set("AFTERADJUSTQTY", oldquantity - e.dataInfo.value);
                                }
                            }
                            else {
                                if (stktype == 0) {
                                    e.dataInfo.dataRow.set("AFTERADJUSTQTY", oldquantity - e.dataInfo.value);
                                }
                                else {
                                    e.dataInfo.dataRow.set("AFTERADJUSTQTY", oldquantity + e.dataInfo.value);
                                }
                            }
                        }
                        break;
                    case 'AFTERADJUSTQTY':
                        if (stkmovetype == "" || stkmovetype == undefined) {
                            Ext.Msg.alert('提示', '请先选择库存移动类型！');
                            e.dataInfo.cancel = true;
                            return;
                        } else {
                            if (stkmovetype == 405) {
                                if (stktype == 0) {
                                    e.dataInfo.dataRow.set("QUANTITY", e.dataInfo.value - oldquantity);
                                } else {
                                    e.dataInfo.dataRow.set("QUANTITY", oldquantity - e.dataInfo.value);
                                }
                            }
                            else {
                                if (stktype == 0) {
                                    e.dataInfo.dataRow.set("AFTERADJUSTQTY", oldquantity - e.dataInfo.value);
                                }
                                else {
                                    e.dataInfo.dataRow.set("AFTERADJUSTQTY", e.dataInfo.value - oldquantity);
                                }
                            }
                        }
                        break;
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
