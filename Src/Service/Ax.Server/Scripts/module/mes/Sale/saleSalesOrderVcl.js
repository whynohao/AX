saleSalesOrderVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
//组合品每个Panel唯一标识
var indexid = 0;
var attId = 0;
var proto = saleSalesOrderVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = saleSalesOrderVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (!this.isEdit) {
            if (e.dataInfo.fieldName == "CreateInquiryBtn") {
                //生成询价单
                var masterRow = this.dataSet.getTable(0).data.items[0];
                var contactsObjectId = masterRow.data["CONTACTSOBJECID"];
                var grid = Ext.getCmp(this.winId + 'COMMATERIALLITGrid');
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length == 0) {
                    alert("请选择载入的明细！");
                }
                else {
                    var List = [];
                    for (var i = 0; i < records.length; i++) {
                        var record = records[i].data;
                        List.push({
                            BILLNO: record["BILLNO"],
                            ROW_ID: record["ROW_ID"],
                            MATERIALID: record["MATERIALID"],
                            MATERIALNAME: record["MATERIALNAME"],
                            MATERIALSPEC: record["MATERIALSPEC"],
                            QUANTITY: record["QUANTITY"]
                        });
                    }
                    var enquryBillNo = this.invorkBcf('CreatEnquiry', [List, contactsObjectId]);
                    if (!Ext.isEmpty(enquryBillNo)) {
                        var obj = [];
                        obj.push(masterRow.data["BILLNO"]);
                        this.browseTo(obj);
                        Ext.Msg.alert("系统提示", "生成询价单【" + enquryBillNo + "】");
                    }
                    else {
                        Ext.Msg.alert("系统提示", "生成询价单失败！");
                    }
                }

            }
            else if (e.dataInfo.fieldName == "CreateSchedulBtn") {
                //生成报价单
                var masterRow = this.dataSet.getTable(0).data.items[0];
                var contactsObjectId = masterRow.data["CONTACTSOBJECID"];
                var grid = Ext.getCmp(this.winId + 'COMMATERIALLITGrid');
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length == 0) {
                    alert("请选择载入的明细！");
                }
                else {
                    var List = [];
                    for (var i = 0; i < records.length; i++) {
                        var record = records[i].data;
                        List.push({
                            BILLNO: record["BILLNO"],
                            ROW_ID: record["ROW_ID"],
                            MATERIALID: record["MATERIALID"],
                            MATERIALNAME: record["MATERIALNAME"],
                            MATERIALSPEC: record["MATERIALSPEC"],
                            QUANTITY: record["QUANTITY"],
                            PRICE: record["PRICE"]
                        });
                    }
                    var scheduleBillNo = this.invorkBcf('CreatSchedule', [List, contactsObjectId]);
                    if (!Ext.isEmpty(scheduleBillNo)) {
                        var obj = [];
                        obj.push(masterRow.data["BILLNO"]);
                        this.browseTo(obj);
                        Ext.Msg.alert("系统提示", "生成报价单单【" + scheduleBillNo + "】");
                    }
                    else {
                        Ext.Msg.alert("系统提示", "生成报价单失败！");
                    }
                }
            }
        }
        else {
            if (e.dataInfo.fieldName == "MaterialPost") {
                //生成报价物料清单
                var List = [];
                var bodyTable = this.dataSet.getTable(1);
                var sun = e.dataInfo.value;
                for (var i = 0; i < bodyTable.data.length; i++) {
                    List.push({
                        AttributeCode: bodyTable.data.items[i].get("ATTRIBUTECODE"),
                        Number: bodyTable.data.items[i].get("QUANTITY")
                    });
                }
                var retrunList = this.invorkBcf('CreatMaterialPost', [List]);
                FillMaterialData(this, retrunList);
            }
            else
                Ext.Msg.alert("系统提示", "编辑状态不能生成单据！");
        }
    }
    else if (e.libEventType == LibEventTypeEnum.ColumnDbClick) {
        if (e.dataInfo.fieldName == "ATTRIBUTEID") {
            var AttributeId = this.invorkBcf("GetDefaultAttr");

            if (AttributeId != "") {
                var returnData = this.invorkBcf('GetAttJson', ["", AttributeId, ""]);
                var dataList = {
                    MaterialId: e.dataInfo.dataRow.data["MATERIALID"],
                    AttributeId: AttributeId,
                    AttributeDesc: "",
                    AttributeCode: "",
                    BillNo: e.dataInfo.dataRow.data["BILLNO"],
                    Row_Id: e.dataInfo.dataRow.data["ROW_ID"]

                };
                CreatAttForm(dataList, returnData, this, e, FillDataRow);

            }


        }
    }
}
browseTo = function (condition) {
    var data = this.invorkBcf("BrowseTo", [condition]);
    this.setDataSet(data, false);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    for (var i = 0; i < this.forms.length; i++) {
        this.forms[i].loadRecord(masterRow);
    };
};
//填充组合品窗口的特征信息
function FillCombineForm(panel, This, CodeDesc) {
    for (var i = 0; i < newPanel.items.items.length  ; i++) {
        if (newPanel.items.items[i].materialId == panel.materialId && newPanel.items.items[i].attributeCode == CodeDesc.Code &&

newPanel.items.items[i].id != panel.id) {
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

function FillMaterialData(This, retrunList) {
    Ext.suspendLayouts();//关闭Ext布局
    var formStore = This.dataSet.getTable(2);//tableIndex是指当前grid所在的表索引，中间层第几个表，curStore是grid的数据源，在extjs中是指Store
    formStore.suspendEvents();//关闭store事件
    console.info(retrunList);
    console.info(formStore);
    try {
        This.deleteAll(2);//删除当前grid的数据
        var masterRow = This.dataSet.getTable(0).data.items[0];//找到表头的数据
        if (retrunList !== undefined && retrunList.length > 0) {
            for (var i = 0; i < retrunList.length; i++) {
                var newRow = This.addRow(masterRow, 2);//这个方法第一个参数是表头数据行，第二个参数是当前grid数据源store所属第几个表
                //newRow.set('BILLNO', formBill.data["BILLNO"]);
                //newRow.set('ROW_ID', i + 1);
                //newRow.set('ROWNO', i + 1);
                newRow.set('MATERIALSPEC', retrunList[i].AttributeDesc);
                newRow.set('QUANTITY', retrunList[i].Number);
                newRow.set('PRICE', retrunList[i].Price);
            }
        }
    }
    finally {
        formStore.resumeEvents();//打开store事件
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}
