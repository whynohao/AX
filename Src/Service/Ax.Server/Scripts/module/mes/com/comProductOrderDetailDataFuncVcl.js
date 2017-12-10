comProductOrderDetailDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = comProductOrderDetailDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comProductOrderDetailDataFuncVcl;
var attId = 0;
proto.winId = null;
proto.fromObj = null;
proto.objthis = null;
//赋值方法
proto.doSetParam = function (vclObj) {
    //判断参数是否为空,代表着是否被呼叫打开
    if (vclObj != undefined) {
        proto.winId = vclObj[0].winId;
        proto.fromObj = vclObj;
        proto.objthis = vclObj[0];

        //获取参数值
        proto.contractNo = vclObj[1];
        proto.billNo = vclObj[2];
        //给表头赋值
        var masterRow = this.dataSet.getTable(0).data.items[0];
        masterRow.set("CONTRACTNO", proto.contractNo);

        //重新加载数据
        this.forms[0].loadRecord(masterRow);

        var returnData = this.invorkBcf("GetProductOrderDetailData", [proto.billNo]);
        fillProductOrderDetailDataFunc(this, returnData);
    }
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.fieldName == "ATTRIBUTENAME") {
                var AttributeId = e.dataInfo.dataRow.get("ATTRIBUTEID");
                var code = e.dataInfo.dataRow.data["ATTRIBUTECODE"];
                if (AttributeId != "") {
                    var AttDicLst = this.invorkBcf('GetAttDataFunc', ["", AttributeId, code]);
                    var dataList = {
                        MaterialId: e.dataInfo.dataRow.data["MATERIALID"],
                        AttributeId: AttributeId,
                        AttributeDesc: "",
                        AttributeCode: "",
                        BillNo: e.dataInfo.dataRow.data["BILLNO"],
                        Row_Id: e.dataInfo.dataRow.data["ROW_ID"]

                    };
                    CreatAttForm_DataFunc(dataList, AttDicLst, e);
                }
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
          
                e.dataInfo.cancel = true;
            
                break;
        case LibEventTypeEnum.BeforeDeleteRow:

            e.dataInfo.cancel = true;

            break;
        case LibEventTypeEnum.ButtonClick:
            //关闭
            if (e.dataInfo.fieldName == "BtnCloseDeTail") {
                this.win.close();

            }
            //查询
            //if (e.dataInfo.fieldName == "BtnSelectDeTail") {
            //    var headTable = this.dataSet.getTable(0).data.items[0];
            //    if (headTable.data["COMPRODUCTID"] == "") {
            //        Ext.Msg.alert("系统提示", "请先填写表头的投产编号信息！！");
            //    }
            //    else {
            //        var returnData = this.invorkBcf("GetProductOrderData", [headTable.data["COMPRODUCTID"]]);
            //        fillProductOrderDataFunc(this, returnData);
            //    }
            //}
            //确认
            if (e.dataInfo.fieldName == "BtnLoadDeTail") {
                var masterTableOne = this.dataSet.getTable(2).data;//出厂编号明细
                var masterTableTwo = this.dataSet.getTable(1).data;//出明细
                var masterTable = this.dataSet.getTable(0).data.items[0].get("CONTRACTNO");//
                var materialListe = [];
                var Liste = [];
                for (var i = 0; i < masterTableOne.length; i++) {
                    if (masterTableOne.items[i].get("MATERIALID") == "" && masterTableOne.items[i].get("MATSTYLE") == "1")
                    {
                        Ext.Msg.alert("提示", "明细的第" + masterTableOne.items[i].get("PARENTROWID") + '行的出厂编码明细中没填物料，请维护出厂编号明细！');
                        return;
                    }
                }
                if (masterTableOne.length > 0) {
                    for (var i = 0; i < masterTableOne.length; i++) {
                        materialListe.push({
                            BillNo: masterTableOne.items[i].data["BILLNO"],
                            RowId: masterTableOne.items[i].data["PARENTROWID"],
                            MatStyle: masterTableOne.items[i].data["MATSTYLE"],
                            MaterialId: masterTableOne.items[i].data["MATERIALID"],
                            Date: masterTableOne.items[i].data["DELIVERYDATE"],
                            ContractNo: masterTable,
                            FactoryNo: masterTableOne.items[i].data["FACTORYNO"],
                        })
                    }
                    for (var i = 0; i < masterTableTwo.length; i++) {
                        Liste.push({
                            BillNo: masterTableTwo.items[i].data["BILLNO"],
                            RowId: masterTableTwo.items[i].data["ROW_ID"],
                            MatStyle: masterTableTwo.items[i].data["MATSTYLE"],
                        })
                    }
                    var bool = this.invorkBcf('GetDetailData', [materialListe,Liste]);
                    this.win.close();
                    if (bool = true)
                        Ext.Msg.alert("提示", '修改成功并更新到了投产单！！！');
                   // var billNo = this.dataSet.getTable(1).data.items[0].get("BILLNO");
                    var billType = proto.fromObj[0].dataSet.getTable(0).data.items[0].get("TYPEID");
                    //proto.forms[0].updateRecord(proto.fromObj[0].dataSet.getTable(2).data);
                    proto.objthis.browseTo([proto.billNo]);
                    //proto.browseTo(proto.billNo);
                    //setAction(proto.billType, false, false)
                }
                else
                {
                    Ext.Msg.alert("提示", '出厂编号明细没有数据，请维护投产单的出厂编号明细！！！');
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "MATSTYLE") {
                    var bodyTable = this.dataSet.getTable(2);
                    for (var i = 0; i < bodyTable.data.length; i++) {
                        if (bodyTable.data.items[i].get("PARENTROWID") == e.dataInfo.dataRow.get("ROW_ID"))
                        {
                            bodyTable.data.items[i].set('MATSTYLE', e.dataInfo.value);
                        }
                    }
                    this.forms[0].loadRecord(headTable.data.items[0]);
                }
            }
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "DATE") {
                    var bodyTable = this.dataSet.getTable(2);
                    for (var i = 0; i < bodyTable.data.length; i++) {
                       
                        bodyTable.data.items[i].set('DELIVERYDATE', e.dataInfo.value);
                        
                    }
                    this.forms[0].loadRecord(headTable.data.items[0]);
                }
            }
            break;
    }
}

//填充明细数据
function fillProductOrderDetailDataFunc(This, returnData) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        This.deleteAll(1);
        This.deleteAll(2);
        var masterRow = This.dataSet.getTable(0).data.items[0];
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = This.addRow(masterRow, 1);
                newRow.set('BILLNO', info.BillNo);
                newRow.set('ROW_ID', info.RowId);
                newRow.set('ROWNO', info.RowNo);
                newRow.set('FACTORYNO', info.FactoryNo);
                newRow.set('FACTORYNODETAIL', info.FactoryNoDetail);
                newRow.set('MATSTYLE', info.MatStyle);
                newRow.set('ATTRIBUTEID', info.AttributeId);
                newRow.set('ATTRIBUTENAME', info.AttributeName);
                newRow.set('ATTRIBUTECODE', info.AttributeCode);
                newRow.set('ATTRIBUTEDESC', info.AttributeDesc);
                newRow.set('DEALSQUANTITY', info.Quantity);
                if (list[i].detail !== undefined && list[i].detail.length > 0) {
                    newRow.set('FACTORYNODETAIL', true);
                    for (var j = 0; j < list[i].detail.length; j++) {
                        var DetailRow = This.addRow(newRow, 2);
                        DetailRow.set('BILLNO', list[i].detail[j].BillNo);
                        DetailRow.set('ROW_ID', list[i].detail[j].RowId);
                        DetailRow.set('PARENTROWID', list[i].detail[j].ParentRowId);
                        DetailRow.set('ROWNO', list[i].detail[j].RowNo);
                        DetailRow.set('FACTORYNO', list[i].detail[j].FactoryNo);
                        DetailRow.set('MATSTYLE', list[i].detail[j].MatStyle);
                        DetailRow.set('MATERIALID', list[i].detail[j].MaterialId);
                        DetailRow.set('DELIVERYDATE', list[i].detail[j].Date);
                    }
                }
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
function CreatAttForm_DataFunc(dataList, AttDicLst, row, method) {

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

                fieldArray.push(CreatTextBox_DataFunc(AttDicLst[i].List[j], isRead));
            }
            else {
                fieldArray.push(CreatComBox_DataFunc(AttDicLst[i].List[j], isRead));
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
    //退出按钮
    var btnSaleCancel = new Ext.Button({
        width: 200,
        height: 35,
        text: "退出",
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
            margin: '5 0 0 300',//上右下左
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
        width: 850,
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
function CreatComBox_DataFunc(attData, isread) {

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
function CreatTextBox_DataFunc(attData, isread) {
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