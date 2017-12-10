comcontractReviewVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var attId = 0;
var This;
var proto = comcontractReviewVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comcontractReviewVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
                e.dataInfo.cancel = true;           
            break;
        case LibEventTypeEnum.BeforeDeleteRow:

            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.fieldName == "ATTRIBUTENAME") {
                var AttributeId = e.dataInfo.dataRow.get("ATTRIBUTEID");
                var code = e.dataInfo.dataRow.data["ATTRIBUTECODE"];
                if (AttributeId != "") {
                    var AttDicLst = this.invorkBcf('GetAtt', ["", AttributeId, code]);
                    var dataList = {
                        MaterialId: e.dataInfo.dataRow.data["MATERIALID"],
                        AttributeId: AttributeId,
                        AttributeDesc: "",
                        AttributeCode: "",
                        BillNo: e.dataInfo.dataRow.data["BILLNO"],
                        Row_Id: e.dataInfo.dataRow.data["ROW_ID"]

                    };
                    This = this;
                    CreatAttForm_por(dataList, AttDicLst, e, FillDataRow_por);

                }
            }
            break;

    }
}

//最新特征窗体
function CreatAttForm_por(dataList, AttDicLst, row, method) {

    var MaterialId = dataList.MaterialId;
    var AttributeId = dataList.AttributeId;
    var AttributeName = dataList.AttributeName;
    var AttributeCode = dataList.AttributeCode;
    var BillNo = dataList.BillNo;
    var Row_Id = dataList.Row_Id;
    var standard = [];
    var unstandard = [];
    var isRead;
    if (AttDicLst.length == 0) {
        Ext.Msg.alert("提示", '该产品不存在特征或特征无法获取！');
        return;
    }
    var panelList = [];
    var collapsed = false;
    for (var i = 0; i < AttDicLst.length; i++) {
        var fieldArray = [];

        for (var j = 0; j < AttDicLst[i].List.length; j++) {
            if (AttDicLst[i].List[j].IsRead == 0) {
                isRead = false;
            }
            else {
                isRead = true;
            }
            if (AttDicLst[i].List[j].Dynamic) {

                fieldArray.push(CreatTextBox_por(AttDicLst[i].List[j], isRead));
            }
            else {
                fieldArray.push(CreatComBox_por(AttDicLst[i].List[j], isRead));
            }
        }


        var standardPanel = new Ext.form.FieldSet({
            id: 'Att' + attId + AttDicLst[i].AttrItemTypeId,
            layout: 'column',
            xtype: 'fieldset',
            title: "<lable><font size=3 ><B>" + AttDicLst[i].AttrItemTypeName + "</B></font></lable>",
            //collapsed: collapsed,
            collapsible: true,
            width: '96%',

            defaulType: 'combobox',
            margin: '5 10 5 10',
            autoScroll: true,
            items: fieldArray,
            listeners: {
                //collapse: function (a, b) {
                //    //Ext.getCmp('no'+ a.id).expand();
                //},
                //expand: function (a, b) {
                //    Ext.getCmp('no' + a.id).collapse(true);
                //}
            }
        });
        //collapsed = true;
        panelList.push(standardPanel);


    }


    //标准特征Panel
    var attPanel = new Ext.form.Panel({

    })

    //非标准特征Panel
    var unattPanel = new Ext.form.Panel({

    })
    //取消按钮
    var btnSaleCancel = new Ext.Button({
        width: 200,
        height: 35,
        text: "关闭窗口",
        type: 'submit',
        handler: function () {
            Ext.getCmp("attWin" + BillNo + Row_Id + MaterialId).close();
        }
    })
    //按钮Panle
    var btnSalePanel = new Ext.form.Panel({
        //layout: 'column',
        width: '100%',
        collapse: false,
        defaults: {
            margin: '5 0 0 320',//上右下左
            columnWidth: .5
        },
        items: [btnSaleCancel]
    })


    var classPanel = new Ext.form.Panel({
        layout: 'column',
        width: '100%',
        autoScroll: true,
        height: 460,
        items: panelList
    })

    var Salewin = new Ext.create('Ext.window.Window', {
        id: "attWin" + BillNo + Row_Id + MaterialId,
        title: '特征信息',
        resizable: false,
        //closeAction: "close",
        modal: true,
        width: 900,
        height: 550,//330
        materialId: MaterialId,//物料ID
        attributeId: AttributeId,//特征ID
        autoScroll: true,
        //layout: 'column',
        items: [classPanel, btnSalePanel],
    });
    attId++;
    Salewin.show();
    for (var i = 0; i < AttDicLst.length; i++) {
        if (AttDicLst[i].Remarks != "") {
            Ext.QuickTips.register({
                target: 'Remarks' + AttDicLst[i].AttributeItemId + '-labelEl',//给填写了备注的特征项元素注册提示信息  
                text: AttDicLst[i].Remarks
            })
        }
    }
}

//非动态特征 combox
function CreatComBox_por(attData, isread) {

    var attlist = [];
    for (var i = 0; i < attData.AttrValueList.length; i++) {
        var data = { AttrCode: attData.AttrValueList[i]['AttrCode'], AttrValue: attData.AttrValueList[i]['AttrValue'] };
        attlist.push(data);
    };
    attlist.push({ AttrCode: "AddNew", AttrValue: "添加新选项" });
    //attlist.splice(attlist.length - 1, 0, { AttrCode: "AddNew1", AttrValue: "xinde" }); // 
    var Store = Ext.create("Ext.data.Store", {
        fields: ["AttrCode", "AttrValue"],
        data: attlist
    });

    var color = "black";
    if (attData.IsRequired == 1) {
        color = "red";
    }
    var combox = new Ext.form.ComboBox({
        mode: 'local',
        forceSelection: true,
        //editablle: true,
        triggerAction: 'all',
        displayField: 'AttrValue',
        fieldLabel: "<lable style='color: " + color + ";'>" + attData.AttributeItemName + "</lable>",
        isRequired: attData.IsRequired,
        attId: attData.AttributeItemId,//特征项ID
        value: attData.DefaultValue,//特征项的值
        valueField: 'AttrCode',
        disabled: isread,
        fields: ['AttrCode', 'AttrValue'],
        store: Store,
        margin: '5 10 5 10',
        columnWidth: .5,
        labelWidth: 60,
        listeners: {
            blur: function (f) {
                if (f.value == "AddNew") {
                    f.value = "";
                    if (!This.isEdit) {
                        Ext.Msg.alert("系统提示", "编辑状态才能新增特征项！");
                        return;
                    }
                    AttItemAddNewForm(f, attlist);
                }
            },

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
    return combox;
}
//动态特征 NumberField
function CreatTextBox_por(attData, isread) {
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
            disabled: isread,
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
            disabled: isread,
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

//填充当前行特征信息
function FillDataRow_por(e, This, CodeDesc) {
    e.dataInfo.dataRow.set("ATTRIBUTECODE", CodeDesc.Code);
    e.dataInfo.dataRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
    e.dataInfo.dataRow.set("METERNO", CodeDesc.Meter);
    e.dataInfo.dataRow.set("PRODUCTSPEC", CodeDesc.MType);
    return true;
}
function getPurchaseOrder_por(e, returnData) {
    e.dataInfo.dataRow.set("DEALSQUANTITY", returnData["DealsQuantity"]); //交易数量
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




//添加子表的每行特征表示和特征描述
function ProductFeature(This, detail) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        This.deleteAll(1);
        var masterRow = This.dataSet.getTable(0).data.items[0];
        if (detail !== undefined && detail.length > 0) {
            for (var i = 0; i < detail.length; i++) {
                var newRow = This.addRow(masterRow, 1);
                newRow.set('ATTRIBUTECODE', detail[i].Code);
                newRow.set('ATTRIBUTEDESC', detail[i].Desc);
                newRow.set('ATTRIBUTEID', detail[i].Feature);
                newRow.set('ATTRIBUTENAME', detail[i].FeatureName);
                newRow.set('REMARK', "");
                newRow.set('METERNO', detail[i].Meter);
                newRow.set('PRODUCTSPEC', detail[i].MType);
                newRow.set('TAXRATE', '0.17');
            }


        }
    }
    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}


//添加新特征值
function AttItemAddNewForm(f, attlist) {
    //确认按钮
    var newAttId = This.invorkBcf('SelectAttId', [f.attId]);
    if (newAttId.indexOf("报错") > -1) {
        Ext.Msg.alert("系统提示", newAttId)
        return;
    }
    //取消按钮
    var btnSaleCancel = new Ext.Button({
        width: 150,
        height: 35,
        text: "取消",
        type: 'submit',
        handler: function () {
            f.setValue("");
            Ext.getCmp("attAddNewWin").close();
        }
    })
    //按钮Panle
    var btnSalePanel = new Ext.form.Panel({
        //layout: 'column',
        width: '100%',
        collapse: false,
        defaults: {
            margin: '0 40 0 80',//上右下左
            columnWidth: .5
        },
        items: [btnSaleConfirm, btnSaleCancel]
    })
    var classPanel = new Ext.form.Panel({
        layout: 'column',
        width: '100%',
        height: 50,
        items: [
               new Ext.form.TextField({
                   fieldLabel: "特征编码",
                   value: newAttId,
                   disabled: true,
                   maxLength: 50,
                   margin: '5 10 5 10',
                   columnWidth: .5,
                   labelWidth: 80,

               }),
                new Ext.form.TextField({
                    fieldLabel: "特征值",
                    value: "",
                    maxLength: 50,
                    margin: '5 10 5 10',
                    columnWidth: .5,
                    labelWidth: 80,
                }),

        ]
    })
    var Salewin = new Ext.create('Ext.window.Window', {
        id: "attAddNewWin",
        title: '特征项值新增',
        resizable: false,
        modal: true,
        width: 600,
        height: 140,
        autoScroll: true,
        defaults: {
            margin: '0 0 0 0',//上右下左
        },
        items: [classPanel, btnSalePanel],
    });

    Salewin.show();
}
