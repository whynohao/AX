comProjectVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
//组合品每个Panel唯一标识
var indexid = 0;
var attId = 0;
var This;
var proto = comProjectVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comProjectVcl;
var path;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "CreateInquiryBtn") {
            if (!this.isEdit) {
                //生成询价单
                var masterRow = this.dataSet.getTable(0).data.items[0];
                var contactsObjectId = masterRow.data["CONTACTSOBJECID"];
                var grid = Ext.getCmp(this.winId + 'COMMATERIALLITGrid');
                var records = grid.getView().getSelectionModel().getSelection();
                // var recordsAll = grid.getView().getForm();
                var masterTable = this.dataSet.getTable(3).data;
                if (records.length == 0) {
                    Ext.Msg.alert("友情提示", "请选择报价物料清单的清单！");
                }
                else {
                    var List = [];
                    for (var i = 0; i < records.length; i++) {
                        var record = records[i].data;
                        List.push({
                            BILLNO: record["BILLNO"],
                            ROW_ID: record["ROW_ID"],
                            ATTRIBUTEITEMID: record["ATTRIBUTEITEMID"],
                            MATERIALID: record["MATERIALID"],
                            MATERIALNAME: record["MATERIALNAME"],
                            MATERIALSPEC: record["MATERIALSPEC"],
                            QUANTITY: record["QUANTITY"]
                        });
                    }
                    var materialList = [];
                    for (var i = 0; i < masterTable.length; i++) {
                        materialList.push({
                            AttributeTypeId: masterTable.items[i].data["ATTRIBUTEITEMTYPEID"],
                            RowId: masterTable.items[i].data["PARENTROWID"],
                            AttributeDesc: masterTable.items[i].data["MATERIALSPEC"],
                            AttributeName: masterTable.items[i].data["ATTRIBUTEITEMID"],
                        })
                    }
                    var enquryBillNo = this.invorkBcf('CreatEnquiry', [List, contactsObjectId, materialList]);
                    if (!Ext.isEmpty(enquryBillNo)) {
                        var obj = [];
                        obj.push(masterRow.data["BILLNO"]);
                        this.browseTo(obj);
                        Ext.Msg.alert("系统提示", "询价单【" + enquryBillNo + "】的物料询价信息已更新");
                        masterRow.set("ENQUIRYBILLNO", enquryBillNo);
                        grid.getSelectionModel().deselectAll();
                    }
                    else {
                        Ext.Msg.alert("系统提示", "生成询价单失败！");
                    }
                }
            }
            else {
                Ext.Msg.alert("系统提示", "编辑状态不能生成单据！");
            }
        }
        else if (e.dataInfo.fieldName == "CreateSchedulBtn") {
            if (!this.isEdit) {
                //生成报价单
                var masterRow = this.dataSet.getTable(0).data.items[0];//表头
                var contactsObjectId = masterRow.data["CONTACTSOBJECID"];
                var masterTable = this.dataSet.getTable(2).data;//报价物料清单
                var bodyGrid = Ext.getCmp(this.winId + 'COMPROJECTDETAILGrid');//明细
                var bodyDetailGrid = Ext.getCmp(this.winId + 'COMSPAREPARTGrid');//备品备件
                var masterTableOne = this.dataSet.getTable(3).data;//阀体明细
                var records = bodyGrid.getView().getSelectionModel().getSelection();
                var recordsOne = bodyDetailGrid.getView().getSelectionModel().getSelection();
                if (records.length == 0 && recordsOne.length == 0) {
                    Ext.Msg.alert("提示", "请选择投标项目明细或者备品备件的清单");
                }
                else {
                    var List = [];
                    if (records != 0) {
                        for (var i = 0; i < records.length; i++) {
                            var record = records[i].data;
                            List.push({
                                BILLNO: record["BILLNO"],
                                ROW_ID: record["ROW_ID"],
                                MeterNo: record["METERNO"],
                                MATERIALID: record["MATERIALID"],
                                ATTRIBUTEITEMID: record["ATTRIBUTEITEMID"],
                                PRODUCTNAME: record["PRODUCTNAME"],
                                MATERIALNAME: record["MATERIALNAME"],
                                ATTRIBUTECODE: record["ATTRIBUTECODE"],
                                ATTRIBUTEDESC: record["ATTRIBUTEDESC"],
                                QUANTITY: record["QUANTITY"]
                            });
                        }
                    }
                    var ListOne = [];
                    if (recordsOne != 0) {
                        for (var i = 0; i < recordsOne.length; i++) {
                            var record = recordsOne[i].data;
                            ListOne.push({
                                BILLNO: record["BILLNO"],
                                ROW_ID: record["ROW_ID"],
                                //MATERIALID: record["MATERIALID"],
                                SPARENAME: record["ATTRIBUTEITEMNAME"],
                                SPARESPEC: record["SPARESPEC"],
                                SALESPRICE: record["SALESPRICE"],
                                PRICE: record["PRICE"],
                                QUANTITY: record["QUANTITY"]
                            });
                        }
                    }
                    var materialList = [];
                    for (var i = 0; i < masterTable.length; i++) {
                        materialList.push({
                            BillNo: masterTable.items[i].data["BILLNO"],
                            MaterialId: masterTable.items[i].data["MATERIALID"],
                            MaterialName: masterTable.items[i].data["MATERIALNAME"],
                            MaterialSpec: masterTable.items[i].data["MATERIALSPEC"],
                            Price: masterTable.items[i].data["PRICE"],
                            SalesPrice: masterTable.items[i].data["SALESPRICE"],
                            Row_Id: masterTable.items[i].data["ROW_ID"],
                            SpecId: masterTable.items[i].data["ATTRIBUTEITEMID"],
                            SpecName: masterTable.items[i].data["ATTRIBUTEITEMNAME"] + masterTable.items[i].data["MATERIALSPEC"]
                        })
                    }
                    var materialListe = [];
                    for (var i = 0; i < masterTableOne.length; i++) {
                        materialListe.push({
                            AttributeTypeId: masterTableOne.items[i].data["ATTRIBUTEITEMTYPEID"],
                            RowId: masterTableOne.items[i].data["PARENTROWID"],
                            AttributeDesc: masterTableOne.items[i].data["MATERIALSPEC"],
                            AttributeName: masterTableOne.items[i].data["ATTRIBUTEITEMID"],
                        })
                    }
                    var scheduleBillNo = this.invorkBcf('CreatSchedule', [List, contactsObjectId, materialList, materialListe, ListOne]);
                    if (!Ext.isEmpty(scheduleBillNo)) {
                        var obj = [];
                        obj.push(masterRow.data["BILLNO"]);
                        this.browseTo(obj);
                        masterRow.set("SCHEDULEBILLNO", scheduleBillNo);
                        Ext.Msg.alert("系统提示", "生成报价单单号为：【" + scheduleBillNo + "】");
                        bodyGrid.getView().getSelectionModel().deselectAll();

                    }
                    else {
                        Ext.Msg.alert("系统提示", "生成报价单失败！");
                    }
                }
            }
            else
                Ext.Msg.alert("系统提示", "编辑状态不能生成单据！");
        }
        else if (e.dataInfo.fieldName == "MaterialPost") {
            if (this.isEdit) {
                //生成报价物料清单
                var List = [];
                var bodyTable = this.dataSet.getTable(1);
                var sun = e.dataInfo.value;
                if (bodyTable.data.length == 0) {
                    Ext.Msg.alert("提示", "投标项目明细是没有数据的，请先填写明细数据！");
                    return;
                }
                else {
                    for (var i = 0; i < bodyTable.data.length; i++) {
                        if (bodyTable.data.items[i].get("QUANTITY") == 0) {
                            Ext.Msg.alert("提示", "投标项目明细中每行的数量必须要大于0");
                            return;
                        }
                        if (bodyTable.data.items[i].get("ATTRIBUTEDESC") == 0) {
                            Ext.Msg.alert("提示", "投标项目明细中特征描述不能为空");
                            return;
                        }
                        List.push({
                            AttributeCode: bodyTable.data.items[i].get("ATTRIBUTECODE"),
                            Number: bodyTable.data.items[i].get("QUANTITY"),
                            RowId: bodyTable.data.items[i].get("ROW_ID"),
                        });
                    }
                    var retrunList = this.invorkBcf('CreatMaterialPost', [List]);
                    FillMaterialData(this, retrunList, retrunList.DetailLsit);
                    Ext.Msg.alert("提示", "报价物料清单生成成功!");
                }
            }
            else {
                Ext.Msg.alert("系统提示", "在编辑状态才能生成报价物料清单！");
            }

        }
            //else if (e.dataInfo.fieldName == "PrintSpec") {
            //    var grid = Ext.getCmp(this.winId + 'COMPROJECTDETAILGrid');
            //    var records = grid.getView().getSelectionModel().getSelection();
            //    if (records.length == 0) {
            //        var bodyTable = this.dataSet.getTable(1);
            //        for (var i = 0; i < bodyTable.data.length; i++) {
            //            printSpec(this, bodyTable.data.items[i], true);
            //        }
            //    }
            //    else {
            //        for (var i = 0; i < records.length; i++) {
            //            printSpec(this, records[i], true);
            //        }
            //    }
            //    Ext.Msg.alert("提示","打印成功！");
            //}
            //else if (e.dataInfo.fieldName == "Preview") {
            //    var grid = Ext.getCmp(this.winId + 'COMPROJECTDETAILGrid');
            //    var records = grid.getView().getSelectionModel().getSelection();
            //    if (records.length == 0) {
            //        Ext.Msg.alert("提示","请选择打印的明细！");
            //        return;
            //    }

            //    for (var i = 0; i < records.length; i++) {
            //        printSpec(this, records[i], false);
            //    }
            //}

        else if (e.dataInfo.fieldName == "UpLoadDeatail") {
            This = this;
            if (this.isEdit) {
                var panel = Ext.create('Ext.form.Panel', {
                    bodyPadding: 10,
                    frame: true,
                    renderTo: Ext.getBody(),
                    items: [{
                        xtype: 'filefield',
                        name: 'txtFile',
                        fieldLabel: '文件',
                        labelWidth: 50,
                        msgTarget: 'side',
                        allowBlank: false,
                        anchor: '100%',
                        buttonText: '选择...'
                    }],

                    buttons: [{
                        text: '导入',
                        handler: function () {
                            var form = this.up('form').getForm();
                            var judge = form.monitor.items.items[0].value;
                            if (judge.indexOf('\\') == -1) {
                                path = judge;
                            }
                            else {
                                path = form.monitor.items.items[0].value.split('\\')[2];
                            }
                            if (form.isValid()) {
                                form.submit({
                                    url: '/fileTranSvc/upLoadFile',
                                    waitMsg: '正在导入文件...',
                                    success: function (fp, o) {
                                        var pathFill = This.invorkBcf('GetDetail', [path]);
                                        if (pathFill == null) {
                                            Ext.Msg.alert('错误', pathFill);
                                            return;
                                        }
                                        else {
                                            //取得最终结果并且填充进子表当中
                                            var detail = This.invorkBcf('Results', [pathFill]);
                                            AddFeature(This, detail);
                                            win.close();
                                        }

                                    },
                                    failure: function (fp, o) {
                                        Ext.Msg.alert('错误', '文件 "' + o.result.FileName + '" 导入失败.');
                                    }
                                });
                            }
                        }
                    }]
                });
                win = Ext.create('Ext.window.Window', {
                    autoScroll: true,
                    width: 400,
                    height: 300,
                    layout: 'fit',
                    vcl: vcl,
                    constrainHeader: true,
                    minimizable: true,
                    maximizable: true,
                    items: [panel]
                });
                win.show();
            }
            else {
                Ext.Msg.alert('错误', '只有在编辑状态下才能导入.');
            }
            //var detail = this.invorkBcf('Results');
            //AddFeature(this,detail);
        }
        else if (e.dataInfo.fieldName == "OutLoadDeatail") {
            var childTabale = this.dataSet.getTable(1).data;
            var list = [];
            var attid;
            for (var i = 0; i < childTabale.items.length; i++) {
                list.push({ 'Feature': childTabale.items[i].data['ATTRIBUTEID'], 'FeatureName': childTabale.items[i].data['ATTRIBUTENAME'], 'Code': childTabale.items[i].data['ATTRIBUTECODE'] });
            }
            var fileName = this.invorkBcf('ParsingChild', [list]);
            if (fileName == null) {
                Ext.Msg.alert('提示', '导出内容为空');
            }
            else {
                if (fileName && fileName !== '') {
                    DesktopApp.IgnoreSkip = true;
                    try {
                        window.location.href = '/TempData/ExportData/' + fileName;
                    } finally {
                        DesktopApp.IgnoreSkip = false
                    }
                }
            }
        }
        else if (e.dataInfo.fieldName == "ExcelSpce") {
            if (!this.isEdit) {
                var headTableRow = this.dataSet.getTable(0).data.items[0];
                var billNo = headTableRow.data["BILLNO"];
                window.open("ExprotComProject.aspx?billNo=" + billNo);
            }
            else { Ext.Msg.alert("系统提示", "编辑状态不能导出Excel！"); }
        }
    }

    else if (e.libEventType == LibEventTypeEnum.ColumnDbClick) {
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
                CreatAttForm(dataList, AttDicLst, e, FillDataRow);

            }
        }
    }
    else if (e.libEventType == LibEventTypeEnum.AddRow) {
        if (e.dataInfo.tableIndex == 1) {
            var btable = this.dataSet.getTable(1).data;

            var desc = btable.items[0].get("ATTRIBUTECODE");
            e.dataInfo.dataRow.set("ATTRIBUTECODE", desc);

        }
    }
    else if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo && e.dataInfo.tableIndex == 1) {
            if (e.dataInfo.fieldName == "ATTRIBUTEID") {
                var bodyTable = this.dataSet.getTable(1);
                var sum = e.dataInfo.value;
                for (var i = 0; i < bodyTable.data.length; i++) {
                    if (e.dataInfo.dataRow.get("ROW_ID") != bodyTable.data.items[i].get("ROW_ID")) {
                        if (bodyTable.data.items[i].get("ATTRIBUTEID") == e.dataInfo.dataRow.get("ATTRIBUTEID"))
                            e.dataInfo.dataRow.set("ATTRIBUTECODE", bodyTable.data.items[i].get("ATTRIBUTECODE"));
                    }
                }
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

//添加子表的每行特征表示和特征描述
function AddFeature(This, detail) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        This.deleteAll(1);
        var masterRow = This.dataSet.getTable(0).data.items[0];
        if (detail != undefined && detail.length > 0) {
            for (var i = 0; i < detail.length; i++) {
                var newRow = This.addRow(masterRow, 1);
                newRow.set('ATTRIBUTECODE', detail[i].Code);
                newRow.set('ATTRIBUTEDESC', detail[i].Desc);
                newRow.set('ATTRIBUTEID', detail[i].Feature);
                newRow.set('ATTRIBUTENAME', detail[i].FeatureName);
                newRow.set('REMARK', "");
                newRow.set('METERNO', detail[i].Meter);
                newRow.set('PRODUCTSPEC', detail[i].MType);
                newRow.set('PRODUCTNAME', detail[i].Valve);
                newRow.set('QUANTITY', detail[i].Quantity);
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
function CreatAttForm(dataList, AttDicLst, row, method) {

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

                fieldArray.push(CreatTextBox(AttDicLst[i].List[j], isRead));
            }
            else {
                fieldArray.push(CreatComBox(AttDicLst[i].List[j], isRead));
            }
        }


        var standardPanel = new Ext.form.FieldSet({
            id: 'Att' + attId + AttDicLst[i].AttrItemTypeId,
            layout: 'column',
            xtype: 'fieldset',
            title: "<lable><font size=3 ><B>" + AttDicLst[i].AttrItemTypeName + "</B></font></lable>",
            //collapsed: collapsed,
            collapsible: true,
            width: '98%',

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
    //确认按钮
    var btnSaleConfirm = new Ext.Button({
        width: 200,
        height: 35,
        text: "确定",
        type: 'submit',
        handler: function () {
            var yes = true;
            var thisWin = Ext.getCmp("attWin" + BillNo + Row_Id + MaterialId);
            if (This.isEdit) {

                var attPanel = thisWin.items.items[0].items.items[0];
                var unattPanel = thisWin.items.items[0].items.items[1];
                var attributeId = thisWin.attributeId;
                var materialId = thisWin.materialId;
                var attDic = [];
                var msg = '';

                for (var j = 0; j < thisWin.items.items[0].items.length; j++) {
                    var attPanel = thisWin.items.items[0].items.items[j];
                    for (var i = 0; i < attPanel.items.length; i++) {

                        if (attPanel.items.items[i].isRequired == 1 && (attPanel.items.items[i].value == null || attPanel.items.items[i].value == "")) {

                            Ext.Msg.alert("提示", '请填写【' + attPanel.items.items[i].fieldLabel + '】的值');
                            return false;
                        }
                        else {
                            //if (attPanel.items.items[i].id.indexOf("numberfield") >= 0 && attPanel.items.items[i].value <= 0) {
                            //    Ext.Msg.alert("提示", '标准特征项【' + attPanel.items.items[i].fieldLabel + '】的值必须大于0！');
                            //    return false;
                            //}
                            attDic.push({ AttributeId: attPanel.items.items[i].attId, AttrCode: attPanel.items.items[i].value })
                        }


                    }
                }

                if (attDic.length > 0) {
                    var CodeDesc = This.invorkBcf('GetAttrInfo', [materialId, attributeId, attDic]);
                    yes = method(row, This, CodeDesc);
                }
                else {
                    Ext.Msg.alert("提示", '请维护特征！');
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
        collapse: false,
        defaults: {
            margin: '5 0 0 140',//上右下左
            columnWidth: .5
        },
        items: [btnSaleConfirm, btnSaleCancel]
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
function CreatComBox(attData, isread) {

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
function CreatTextBox(attData, isread) {
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
function FillDataRow(e, This, CodeDesc) {
    e.dataInfo.dataRow.set("ATTRIBUTECODE", CodeDesc.Code);
    e.dataInfo.dataRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
    e.dataInfo.dataRow.set("METERNO", CodeDesc.Meter);
    e.dataInfo.dataRow.set("PRODUCTSPEC", CodeDesc.MType);
    e.dataInfo.dataRow.set("PRODUCTNAME", CodeDesc.Valve);
    e.dataInfo.dataRow.set("QUANTITY", CodeDesc.Quantity);
    return true;
}

function FillMaterialData(This, retrunList) {
    Ext.suspendLayouts();//关闭Ext布局
    var formStore = This.dataSet.getTable(2);//tableIndex是指当前grid所在的表索引，中间层第几个表，curStore是grid的数据源，在extjs中是指Store
    formStore.suspendEvents();//关闭store事件
    try {
        This.deleteAll(2);
        This.deleteAll(3);//删除当前grid的数据
        var masterRow = This.dataSet.getTable(0).data.items[0];//找到表头的数据
        var bodyRow = This.dataSet.getTable(2);
        var i = 0;
        if (retrunList !== undefined && retrunList.length > 0) {
            for (var i = 0; i < retrunList.length; i++) {
                var newRow = This.addRow(masterRow, 2);//这个方法第一个参数是表头数据行，第二个参数是当前grid数据源store所属第几个表
                //newRow.set('BILLNO', formBill.data["BILLNO"]);

                newRow.set('ROW_ID', i + 1);
                newRow.set('ROWNO', i + 1);
                newRow.set('ATTRIBUTEITEMTYPEID', retrunList[i].AttributeTypeId);
                newRow.set('ATTRIBUTEITEMTYPENAME', retrunList[i].AttributeTypeName);
                newRow.set('ATTRIBUTEITEMID', retrunList[i].AttributeName);
                newRow.set('ATTRIBUTEITEMNAME', retrunList[i].AttributeIdName);
                newRow.set('MATERIALSPEC', retrunList[i].AttributeDesc);
                newRow.set('QUANTITY', retrunList[i].Number);
                newRow.set('PRICE', retrunList[i].Price);
                newRow.set('SALESPRICE', retrunList[i].SalesPrice);
                newRow.set('FROMROWID', retrunList[i].RowId);
                if (retrunList[i]['DetailLsit']!=null && retrunList[i]['DetailLsit'].length > 0) {
                    newRow.set('ATTRIBUTEITEMSUB', retrunList[i].IsTrue);
                    for (var e = 0; e < retrunList[i]['DetailLsit'].length; e++) {
                        var newRow = This.addRow(masterRow, 3);
                        newRow.set('ATTRIBUTEITEMTYPEID', retrunList[i]['DetailLsit'][e].AttributeTypeId);
                        newRow.set('ATTRIBUTEITEMTYPENAME', retrunList[i]['DetailLsit'][e].AttributeTypeName);
                        newRow.set('ATTRIBUTEITEMID', retrunList[i]['DetailLsit'][e].AttributeId);
                        newRow.set('ATTRIBUTEITEMNAME', retrunList[i]['DetailLsit'][e].AttributeName);
                        newRow.set('MATERIALSPEC', retrunList[i]['DetailLsit'][e].MaterialSpec);
                        newRow.set('PARENTROWID', This.dataSet.getTable(2).data.items[i].get("ROW_ID"));
                    }
                }
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


//打印规格书
function printSpec(This, bodyRow, mode) {
    var TxtName = bodyRow.data["ATTRIBUTENAME"];
    var returnData = This.invorkBcf("GetTxt", [TxtName]);
    if (returnData == "") {
        Ext.Msg.alert("提示", '未找到相关规格书模板！');
        return;
    }
    var attList = This.invorkBcf("GetAttJson", ["", bodyRow.data["ATTRIBUTEID"], bodyRow.data["ATTRIBUTECODE"]]);
    for (var i = 0; i < attList.length; i++) {
        returnData = returnData.replace("@" + attList[i].AttributeItemId + "", attList[i].SelectValue);
    }
    var billDate = This.dataSet.getTable(0).data.items[0].data["BILLDATE"].toString();
    var date = billDate.substr(0, 4) + '-' + billDate.substr(4, 2) + '-' + billDate.substr(6, 2);

    returnData = returnData.replace("@Date", date);//日期
    returnData = returnData.replace("@EndUserName", This.dataSet.getTable(0).data.items[0].data["ENDUSERNAME"]);//最终用户
    returnData = returnData.replace("@IndexPage", bodyRow.data["ROWNO"]);//第几页
    returnData = returnData.replace("@AllPage", This.dataSet.getTable(1).data.items.length);//共几页
    returnData = returnData.replace("@Revision", This.dataSet.getTable(0).data.items[0].data["REVISION"]);//版次
    returnData = returnData.replace("@LastUpdateName", This.dataSet.getTable(0).data.items[0].data["LASTUPDATENAME"]);//编制
    returnData = returnData.replace("@ApprovrName", This.dataSet.getTable(0).data.items[0].data["APPROVRNAME"]);//校对 审核
    returnData = returnData.replace("@ApprovrName", This.dataSet.getTable(0).data.items[0].data["APPROVRNAME"]);//校对 审核
    returnData = returnData.replace("@DetailRemark", bodyRow.data["REMARK"]);//说明
    var LODOP = getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
    htmlStr = returnData;
    LODOP.ADD_PRINT_HTM(20, 5, "100%", "100%", htmlStr); //ADD_PRINT_HTM(Top,Left,Width,Height,strHtmlContent)6
    LODOP.ADD_PRINT_IMAGE(60, 35, 170, 40, "<img src=./Scripts/desk/images/中德.png width=170 height=40>");//商标

    LODOP.SET_PRINT_PAGESIZE(1, 0, 0, "A4"); //1---纵(正)向打印  2---横向打印 SET_PRINT_PAGESIZE(intOrient,intPageWidth,intPageHeight,strPageName)设定纸张大小

    if (mode) {
        LODOP.PRINT();
        //LODOP.PREVIEW();

    }
    else {
        LODOP.PREVIEW();
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
    var btnSaleConfirm = new Ext.Button({
        width: 150,
        height: 35,
        text: "确定",
        type: 'submit',
        handler: function () {
            var yes = true;
            var thisWin = Ext.getCmp("attAddNewWin");
            var Panel = thisWin.items.items[0];
            var Id = Panel.items.items[0].value;
            var Name = Panel.items.items[1].value;
            if (Name == '') {
                Ext.Msg.alert("系统提示", "特征值不能为空！")
            }

            var res = This.invorkBcf('AttItemAddNew', [f.attId, Id, Name]);
            if (res == '成功') {
                var list = f.store.data;
                attlist.splice(attlist.length - 1, 0, { AttrCode: Id, AttrValue: Name }); // 
                f.store.removeAll();//先清空数据
                f.store.loadData(attlist);
                f.setValue(Id);
                thisWin.close();
            }
            else {
                Ext.Msg.alert("系统提示", res)
            }

        }
    })
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
