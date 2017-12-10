comSaleBOMVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    this.originalValue = [];
    //第一次打开
    this.firstLoad = true;

    //树形结构
    this.tree;

    this.level = 0;  //某一层级

    this.levelCount = 0;  //总层级数

    this.copyData = {};
};
var attId = 0;
var proto = comSaleBOMVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comSaleBOMVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            switch (e.dataInfo.fieldName) {
                case "ATTRIBUTENAME":
                    if (this.billAction == BillActionEnum.Modif || this.billAction == BillActionEnum.AddNew) {
                        var MaterialId = e.dataInfo.dataRow.data["SUBMATERIALID"];
                        var AttributeId = e.dataInfo.dataRow.data["ATTRIBUTEID"];
                        var AttributeCode = e.dataInfo.dataRow.data["ATTRIBUTECODE"]
                        if (AttributeId != "") {
                            var returnData = this.invorkBcf('GetAttJson', [MaterialId, AttributeId, AttributeCode]);
                            var dataList = {
                                MaterialId: e.dataInfo.dataRow.data["SUBMATERIALID"],
                                AttributeId: e.dataInfo.dataRow.data["ATTRIBUTEID"],
                                AttributeDesc: e.dataInfo.dataRow.data["ATTRIBUTEDESC"],
                                AttributeCode: e.dataInfo.dataRow.data["ATTRIBUTECODE"],
                            };
                            this.CreatAttForm(dataList, returnData, this, e.dataInfo.dataRow, this.FillDataRow);
                        }
                        break;
                    }
                    else {
                        Ext.Msg.alert("提示", "修改状态下才可以修改特征！");
                    }
            }
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                if (this.tree.getSelectionModel().getSelected().items[0].get("BOMLEVEL") == 0) {
                    e.dataInfo.cancel = true;
                    Ext.Msg.alert("提示", "不能修改根节点！");
                }
            }
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName === "ISABNORMALCONFIRM") {
                    if (e.dataInfo.oldValue == 1 && e.dataInfo.value == 0) {
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("提示", "已经确认的订单BOM不能取消确认！");
                    }
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            //选择母件后给树形结构第一行赋值
            if (e.dataInfo.tableIndex === 0) {
                if (e.dataInfo.fieldName === "SUBROWID") {
                    var billno = e.dataInfo.dataRow.data["FROMBILLNO"];
                    var rowid = e.dataInfo.dataRow.data["FROMROWID"];
                    var subRowId = e.dataInfo.value;
                    var list = this.invorkBcf("GetMaterialFromSubRowId", [billno, rowid, subRowId]);
                    Ext.getCmp('MATERIALID0_' + this.winId).setValue(list[0].MaterialId);
                    Ext.getCmp('UNITID0_' + this.winId).setValue(list[0].UnitId);
                    Ext.getCmp('ATTRIBUTEID0_' + this.winId).setValue(list[0].AttributeId);
                    Ext.getCmp('ATTRIBUTECODE0_' + this.winId).setValue(list[0].AttributeCode);
                    Ext.getCmp('ATTRIBUTEDESC0_' + this.winId).setValue(list[0].AttributeDesc);
                    //给表身的物料赋值
                    this.tree.store.data.items[0].set("SUBMATERIALNAME", this.dataSet.getTable(0).data.items[0].data["MATERIALNAME"]);
                    this.tree.store.data.items[0].set("SUBMATERIALID", Ext.getCmp('MATERIALID0_' + this.winId).getValue());
                    var root = this.tree.getRootNode();
                    if (root.childNodes.length > 0) {
                        this.ChangeSubMaterial.call(this, root, root.get("SUBMATERIALID"));
                        for (var i = 0; i < root.childNodes.length; i++) {
                            root.childNodes[i].set("PMATERIALID", root.get("SUBMATERIALID"));
                            root.childNodes[i].set("PMATERIALNAME", root.get("SUBMATERIALNAME"));
                        }
                    }
                    this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                }
                if (e.dataInfo.fieldName === "MATERIALID") {
                    this.tree.store.data.items[0].set("SUBMATERIALNAME", this.dataSet.getTable(0).data.items[0].data["MATERIALNAME"]);
                    this.tree.store.data.items[0].set("SUBMATERIALID", Ext.getCmp('MATERIALID0_' + this.winId).getValue());
                    var root = this.tree.getRootNode();
                    if (root.childNodes.length > 0) {
                        this.ChangeSubMaterial.call(this, root, root.get("SUBMATERIALID"));
                        for (var i = 0; i < root.childNodes.length; i++) {
                            root.childNodes[i].set("PMATERIALID", root.get("SUBMATERIALID"));
                            root.childNodes[i].set("PMATERIALNAME", root.get("SUBMATERIALNAME"));
                        }
                    }
                }
                    //更改表头生产数量，明细中生产应用量做出变化
                else if (e.dataInfo.fieldName == "QUANTITY") {
                    this.GetProduceQty.call(this, this.tree.store.data.items[0], this.dataSet, e.dataInfo.value, e.dataInfo.dataRow.get("ISUNITEXPENDED"));
                }
                else if (e.dataInfo.fieldName == "ISUNITEXPENDED") {
                    this.GetProduceQty.call(this, this.tree.store.data.items[0], this.dataSet, e.dataInfo.dataRow.get("QUANTITY"), e.dataInfo.value);
                }
                this.forms[0].updateRecord(e.dataInfo.dataRow)
            }
            if (e.dataInfo.tableIndex == 1) {
                //绑定
                function getBindingRow() {
                    return this.dataSet.FindRow(1, e.dataInfo.dataRow.get("ROW_ID"));;
                }
                var bindingRow = e.dataInfo.dataRow.bindingRow || getBindingRow.call(this);
                bindingRow.set(e.dataInfo.fieldName, e.dataInfo.value);

                //子件变化后修改子节点的上级物料
                if (e.dataInfo.fieldName === "SUBMATERIALID") {
                    if (e.dataInfo.value != e.dataInfo.oldValue) {
                        this.DifFerence.call(this, e);
                    }
                    e.dataInfo.dataRow.set("NODENAME", e.dataInfo.dataRow.data["SUBMATERIALNAME"]);
                    e.dataInfo.dataRow.set("ATTRIBUTECODE", "");
                    e.dataInfo.dataRow.set("ATTRIBUTEDESC", "");
                    bindingRow.set("NODENAME", e.dataInfo.dataRow.data["SUBMATERIALNAME"]);
                    bindingRow.set("ATTRIBUTECODE", "");
                    bindingRow.set("ATTRIBUTEDESC", "");
                    var rootNode = this.tree.getSelectionModel().getSelected().items[0];
                    var PRowid = rootNode.get("ROW_ID");
                    var Pmaterialid;
                    var foundRow = this.dataSet.FindRow(1, PRowid);
                    if (foundRow) {
                        Pmaterialid = foundRow.get("SUBMATERIALID");
                    }

                    var Rowid = new Array();
                    var child = rootNode.childNodes
                    if (child.length > 0) {
                        for (var i = 0; i < child.length; i++) {
                            Rowid.push(child[i].get("ROW_ID"));
                            child[i].set("PMATERIALID", Pmaterialid);
                            child[i].set("PMATERIALNAME", rootNode.get("SUBMATERIALNAME"));
                        }
                    }
                    for (var i = 0; i < Rowid.length; i++) {
                        for (var j = 0; j < this.dataSet.getTable(1).data.items.length; j++) {
                            if (this.dataSet.getTable(1).data.items[j].data["ROW_ID"] == Rowid[i]) {
                                this.dataSet.getTable(1).data.items[j].set("PMATERIALID", Pmaterialid);
                                break;
                            }
                        }
                    }
                }

                if (e.dataInfo.fieldName == 'BASEQTY' || e.dataInfo.fieldName == 'UNITQTY' || e.dataInfo.fieldName == 'ATTRIBUTECODE' || e.dataInfo.fieldName == 'ATTRIBUTEDESC' || e.dataInfo.fieldName == 'PMATERIALID') {
                    if (e.dataInfo.value != e.dataInfo.oldValue) {
                        this.DifFerence.call(this, e);
                    }
                }
                //修改工艺路线和工艺路线行标识时，会判断表头的是否确认工艺路线的值
                if (e.dataInfo.fieldName === "SALETECHROUTEID" || e.dataInfo.fieldName === "SALETECHROUTEROWID") {
                    this.UpdateIsConfirm.call(this, e.dataInfo.dataRow, false);
                }
                //母件基数，单位用量,生产数量的联动
                if (e.dataInfo.fieldName === "BASEQTY") {
                    var quantity;
                    if (e.dataInfo.dataRow.get("PARENTROWID") == 0) {
                        quantity = Ext.getCmp('QUANTITY0_' + this.winId).getValue();
                    }
                    else {
                        var foundRow = this.dataSet.FindRow(1, e.dataInfo.dataRow.get("PARENTROWID"));
                        if (foundRow) {
                            quantity = foundRow.get("PRODUCEQTY");
                        }
                    }
                    var unitqty = e.dataInfo.dataRow.get("UNITQTY");
                    var baseqty = e.dataInfo.value;
                    if (quantity > 0) {
                        var produceqty = quantity * unitqty / baseqty;
                        //将结果填到生产应用量中
                        e.dataInfo.dataRow.set("PRODUCEQTY", produceqty);
                        var foundRow = this.dataSet.FindRow(1, e.dataInfo.dataRow.get("ROW_ID"));
                        if (foundRow) {
                            foundRow.set("PRODUCEQTY", produceqty);
                        }
                        //修改子节点的生产应用量
                        var Node = this.tree.getSelectionModel().getSelected().items[0];
                        this.ChangChildProduceQty.call(this, Node);
                    }
                }
                if (e.dataInfo.fieldName === "UNITQTY") {
                    var quantity;
                    if (e.dataInfo.dataRow.get("PARENTROWID") == 0) {
                        quantity = Ext.getCmp('QUANTITY0_' + this.winId).getValue();
                    }
                    else {
                        var foundRow = this.dataSet.FindRow(1, e.dataInfo.dataRow.get("PARENTROWID"));
                        if (foundRow) {
                            quantity = foundRow.get("PRODUCEQTY");
                        }
                    }
                    var baseqty = e.dataInfo.dataRow.get("BASEQTY");
                    var unitqty = e.dataInfo.value;
                    if (quantity > 0) {
                        var produceqty = quantity * unitqty / baseqty;
                        //将结果填到生产应用量中
                        e.dataInfo.dataRow.set("PRODUCEQTY", produceqty);
                        var foundRow = this.dataSet.FindRow(1, e.dataInfo.dataRow.get("ROW_ID"));
                        if (foundRow) {
                            foundRow.set("PRODUCEQTY", produceqty);
                        }
                        //修改子节点的生产应用量
                        var Node = this.tree.getSelectionModel().getSelected().items[0];
                        this.ChangChildProduceQty.call(this, Node);
                    }
                }
            }
            break;
    }
}

//重新生成订单BOM
proto.reCreateSaleBom = function () {
    var success = false;
    var saleOrderInfo = {};
    if (this.isEdit) {
        var masterRow = this.dataSet.getTable(0).data.items[0];
        saleOrderInfo.ReCreateSaleBom = true;
        saleOrderInfo.SaleBomId = masterRow.get("SALEBOMID");
        saleOrderInfo.AttributeCode = masterRow.get("ATTRIBUTECODE");
        saleOrderInfo.AttributeDesc = masterRow.get("ATTRIBUTEDESC");
        saleOrderInfo.AttributeId = masterRow.get("ATTRIBUTEID");
        saleOrderInfo.BomConfirm = masterRow.get("ISABNORMALCONFIRM");
        saleOrderInfo.DetailRemark = masterRow.get("REMARK");
        saleOrderInfo.IsAbnormal = masterRow.get("ISABNORMAL");
        saleOrderInfo.ProductDesc = masterRow.get("MATERIALSPEC");
        saleOrderInfo.ProductId = masterRow.get("MATERIALID");
        saleOrderInfo.ProductName = masterRow.get("MATERIALNAME");
        saleOrderInfo.ProductQty = masterRow.get("QUANTITY");
        saleOrderInfo.SaleOrderNo = masterRow.get("FROMBILLNO");
        saleOrderInfo.SaleOrderRowId = masterRow.get("FROMROWID");
        saleOrderInfo.SaleOrderSubRowId = masterRow.get("");
        saleOrderInfo.TotalRemark = masterRow.get("SUBROWID");
        success = this.invorkBcf("ReCreateSaleBom", [saleOrderInfo]);
    }
    else {
        Ext.Msg.alert("提示", "浏览状态才能重新生成订单BOM！");
    }
    if (success) {
        //重新刷新页面
        this.browseTo([saleOrderInfo['SaleBomId']]);
        this.edit(this.currentPk, {});
        Ext.Msg.alert("提示", "重新生成订单BOM成功！");
    }
    else {
        Ext.Msg.alert("提示", "重新生成订单BOM失败！");
    }
}
//修改状态
proto.edit = function (condition, assistObj) {
    this.billAction = BillActionEnum.Modif;
    return this.invorkBcf("Edit", [condition], assistObj);
};
//清空明细
proto.removeDetail = function () {
    if (this.isEdit) {
        this.dataSet.getTable(1).removeAll();
        this.tree.setRootNode(this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0));
    }
    else {
        Ext.Msg.alert("提示", "编辑状态才能清空订单BOM明细！");
    }
}
//修改特征
proto.changeAttribute = function () {
    if (this.isEdit) {
        var MaterialId = this.dataSet.getTable(0).data.items[0].data["MATERIALID"];
        var AttributeId = this.dataSet.getTable(0).data.items[0].data["ATTRIBUTEID"];
        var AttributeCode = this.dataSet.getTable(0).data.items[0].data["ATTRIBUTECODE"]
        if (AttributeId != "") {
            var returnData = this.invorkBcf('GetAttJson', [MaterialId, AttributeId, AttributeCode]);
            var dataList = {
                MaterialId: this.dataSet.getTable(0).data.items[0].data["MATERIALID"],
                AttributeId: this.dataSet.getTable(0).data.items[0].data["ATTRIBUTEID"],
                AttributeDesc: this.dataSet.getTable(0).data.items[0].data["ATTRIBUTEDESC"],
                AttributeCode: this.dataSet.getTable(0).data.items[0].data["ATTRIBUTECODE"],
            };
            this.CreatAttForm(dataList, returnData, this, this.dataSet.getTable(0).data.items[0], this.FillDataRow);
        }
    }
    else {
        Ext.Msg.alert("提示", "编辑状态才能修改特征！");
    }
}

//打印数据
proto.print = function (headTable, bodyTable) {
    headTable = vcl.dataSet.getTable(0).data.items[0];
    bodyTable = vcl.dataSet.getTable(1);

    var strHead = "<div align=center>"
    strHead += " <strong><font size = '5px'> 订单BOM </font></strong>"; "</div>";

    var strTableStartHtml = "<table border='1' width='100%' bordercolor='#336699' cellpadding='0' cellspacing='0' align='center'>";
    var strTableTheadHtml = "<thead style='height: 30px' bgcolor='#efefef'>";

    strTableTheadHtml += "<td nowrap align='center'><font size = '1px'>物料编码</font></td>";
    strTableTheadHtml += "<td nowrap align='center'><font size = '1px'>物料名称</font></td>";
    strTableTheadHtml += "<td nowrap align='center'><font size = '1px'>物料型号</font></td>";
    strTableTheadHtml += "<td nowrap align='center'><font size = '1px'>物料特征描述</font></td>";
    strTableTheadHtml += "<td nowrap align='center'><font size = '1px'>生产应用量</font></td>";
    strTableTheadHtml += "<td nowrap align='center'><font size = '1px'>生产领用量</font></td>";
    strTableTheadHtml += "<td nowrap align='center'><font size = '1px'>完成确认</font></td>";
    strTableTheadHtml += "</thead>";

    //构建表身
    var strTableTrHtml = "";
    for (var i = 0; i < bodyTable.data.length; i++) {
        var bodyItems = bodyTable.data.items[i];
        var SubMaterialId = bodyItems.get("SUBMATERIALID");
        var SubMaterialName = bodyItems.get("SUBMATERIALNAME");
        var SubmaterialSpec = bodyItems.get("SUBMATERIALSPEC");
        if (SubmaterialSpec == undefined) {
            SubmaterialSpec = "";
        }
        var AttributeDesc = bodyItems.get("ATTRIBUTEDESC");
        var ProduceQty = bodyItems.get("PRODUCEQTY");
        var Quantity = bodyItems.get("QUANTITY");
        var td = "<tr>";
        td += "<td nowrap align='center'><font size = '2px'>" + SubMaterialId + "</font></td>";
        td += "<td nowrap align='center'><font size = '2px'>" + SubMaterialName + "</font></td>";
        td += "<td nowrap align='center'><font size = '2px'>" + SubmaterialSpec + "</font></td>";
        td += "<td  align='center'><font size = '2px'>" + AttributeDesc + "</font></td>";
        td += "<td nowrap align='center'><font size = '2px'>" + ProduceQty + "</font></td>";
        td += "<td nowrap align='center'><font size = '2px'>" + Quantity + "</font></td>";
        td += "<td nowrap align='center'><font size = '2px'></font></td>";
        td += "<td nowrap align='center'><font size = '2px'></font></td>";//完成确认
        td += "</tr>";

        strTableTrHtml += td;
    }
    var strTableEndHtml = "</table>";
    var LODOP = getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
    var htmlStr = strHead + strTableStartHtml + strTableTheadHtml + strTableTrHtml + strTableEndHtml;

    LODOP.PRINT_INITA(0, 0, "100%", "100%", "打印控件功能演示_Ext");
    LODOP.ADD_PRINT_HTM(1, 1, "100%", "100%", htmlStr); //ADD_PRINT_HTM(Top,Left,Width,Height,strHtmlContent)
    LODOP.SET_PRINT_PAGESIZE(1, 0, 0, "A4"); //1---纵(正)向打印  2---横向打印 SET_PRINT_PAGESIZE(intOrient,intPageWidth,intPageHeight,strPageName)设定纸张大小
    LODOP.PREVIEW();
}

//填充当前行特征信息
proto.FillDataRow = function FillDataRow(e, This, CodeDesc) {
    e.dataInfo.dataRow.set("ATTRIBUTECODE", CodeDesc.Code);
    e.dataInfo.dataRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
    e.dataInfo.dataRow.set("ABNORMALDAY", CodeDesc.AbnormalDay);
    //设置异常天数
    //var masterRow = This.dataSet.getTable(0).data.items[0];
    //Ext.getCmp("ABNORMALDAY0_" + This.winId).setValue(CodeDesc.AbnormalDay);
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
                unstandard.push(This.CreatTextBox(returnData[i]));
            }
            else {
                standard.push(This.CreatTextBox(returnData[i]));
            }
        }
        else {
            if (returnData[i].Standard) {
                unstandard.push(This.CreatComBox(returnData[i]));
            }
            else {
                standard.push(This.CreatComBox(returnData[i]));
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
                    if (attPanel.items.items[i].value == null) {
                        //msg += '【' + attPanel.items.items[i].fieldLabel + '】';
                        continue;
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
                    var yes = This.FillDataRow(row, This, CodeDesc);
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
            title: '非标准特',
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
proto.CreatComBox = function (attData) {

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
proto.CreatTextBox = function (attData) {
    if (attData.ValueType == 0) {
        var textbox = new Ext.form.NumberField({
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
    }
    else {
        var textbox = new Ext.form.TextField({
            fieldLabel: attData.AttributeItemName,
            attId: attData.AttributeItemId,
            allowBlank: false,
            value: attData.DefaultValue,
            maxLength: 50,
            margin: '5 10 5 10',
            columnWidth: .5,
            labelWidth: 60,
        });
    }
    return textbox;
}

//填充当前行特征信息
proto.FillDataRow = function (row, This, CodeDesc) {
    row.set("ATTRIBUTECODE", CodeDesc.Code);
    row.set("ATTRIBUTEDESC", CodeDesc.Desc);
    //如果没有行标识，说明是表头，需要UPDATE刷新一下界面
    if (row.get("ROW_ID") != undefined) {
        var foundRow = This.dataSet.FindRow(1, row.get("ROW_ID"));
        if (foundRow);
        {
            foundRow.set("ATTRIBUTECODE", CodeDesc.Code);
            foundRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
        }
    }
    else {
        This.tree.store.data.items[0].set("ATTRIBUTECODE", CodeDesc.Code);
        This.tree.store.data.items[0].set("ATTRIBUTEDESC", CodeDesc.Desc);
        This.forms[0].loadRecord(This.dataSet.getTable(0).data.items[0]);
    }
    return true;
}

//是否确认物料的更新验证
proto.UpdateIsConfirm = function (curRow, isDelete) {
    var curTable = this.dataSet.getTable(1);
    var isConfirm = true;
    if (!this.ConfirmRowHaveTechRoute.call(this, curRow) && !isDelete) {
        isConfirm = false
    }
    else {
        for (var i = 1; i < curTable.data.items.length; i++) {
            if (curTable.data.items[i].get("ROW_ID") != curRow.get("ROW_ID")) {
                if (!this.ConfirmRowHaveTechRoute.call(this, curTable.data.items[i])) {
                    isConfirm = false;
                    break;
                }
            }
        }
    }
    this.dataSet.getTable(0).data.items[0].set("ISCONFIRM", isConfirm);
    this.forms[0].loadRecord(this.dataSet.getTable(0).data.items[0]);
}

proto.ConfirmRowHaveTechRoute = function (curRow) {
    return (curRow.get("SALETECHROUTEID") != "" && curRow.get("SALETECHROUTEROWID") != "")
}
//新增节点时，获取应带出的生产应用量
proto.GetProduceQty = function (curNode, curDataSet, dataInfoValue, dataIsExpended) {
    var parentQty = 0;
    for (var i = 0; i < curNode.childNodes.length; i++) {
        if (dataIsExpended) {
            parentQty = dataInfoValue;
        }
        else {
            if (curNode.childNodes[i].get("BOMLEVEL") == 1) {
                parentQty = dataInfoValue;
            }
            else {
                parentQty = curNode.get("PRODUCEQTY");
            }
        }
        curNode.childNodes[i].set("PRODUCEQTY", parentQty * curNode.childNodes[i].get('UNITQTY') / curNode.childNodes[i].get('BASEQTY'));
        curNode.childNodes[i].set("QUANTITY", parentQty * curNode.childNodes[i].get('UNITQTY') / curNode.childNodes[i].get('BASEQTY'));
        var foundRow = curDataSet.FindRow(1, curNode.childNodes[i].get("ROW_ID"));
        if (foundRow) {
            foundRow.set("PRODUCEQTY", curNode.childNodes[i].get("PRODUCEQTY"));
            foundRow.set("QUANTITY", curNode.childNodes[i].get("QUANTITY"));
        }
        this.GetProduceQty.call(this, curNode.childNodes[i], curDataSet, dataInfoValue, dataIsExpended);
    }
}
//新增节点时，获取应带出的生产应用量
proto.GetProduceQtyAddRow = function (newAddRow, curDataSet) {
    var parentQty = 0;
    var headRow = curDataSet.getTable(0).data.items[0]
    if (headRow.data["ISUNITEXPENDED"]) {
        parentQty = headRow.data["QUANTITY"];
    }
    else {
        if (newAddRow.get("BOMLEVEL") == 1) {
            parentQty = headRow.data["QUANTITY"];
        }
        else {
            var foundRow = vcl.dataSet.FindRow(1, newAddRow.get("PARENTROWID"));
            if (foundRow) {
                parentQty = foundRow.get("PRODUCEQTY");
                parentQty = foundRow.get("QUANTITY");
            }
        }
    }
    return parentQty * newAddRow.get('BASEQTY') / newAddRow.get('UNITQTY');
}
//当生产应用量改变时，更改下级节点的生产应用量
proto.ChangChildProduceQty = function (Node) {
    var child = Node.childNodes;
    var data = new Array();
    if (child.length > 0) {
        for (var i = 0; i < child.length; i++) {
            data.push({
                rowid: child[i].get("ROW_ID"),
                quantity: Node.get("PRODUCEQTY"),

            });
            unitqty = child[i].get("UNITQTY");
            baseqty = child[i].get("BASEQTY");
            quantity = Node.get("PRODUCEQTY");
            child[i].set("PRODUCEQTY", quantity * unitqty / baseqty);
            child[i].set("QUANTITY", quantity * unitqty / baseqty);
        }
    }
    for (var i = 0; i < data.length; i++) {
        //for (var j = 0; j < this.dataSet.getTable(1).data.items.length; j++) {
        //    if (this.dataSet.getTable(1).data.items[j].data["ROW_ID"] == data[i].rowid) {
        //        this.dataSet.getTable(1).data.items[j].set("PRODUCEQTY", data[i].quantity * unitqty / baseqty);
        //        break;
        //    }
        //}
        var foundRow = this.dataSet.FindRow(1, data[i].rowid);
        if (foundRow) {
            foundRow.set("PRODUCEQTY", data[i].quantity * unitqty / baseqty);
            foundRow.set("QUANTITY", data[i].quantity * unitqty / baseqty);
        }
    }
    for (var i = 0; i < child.length; i++) {
        this.ChangChildProduceQty.call(this, child[i]);
    }
}
//差异字段中是否包含此列名
proto.IsContain = function (modifiy, columnName) {
    var bool = false;
    for (var i = 0; i < modifiy.length; i++) {
        if (modifiy[i] === columnName) {
            bool = true;
            break;
        }
    }
    return bool;
}
//移动行后改变数据
proto.ChangeRow = function (data) {
    //改变父行标识和上级物料
    if (data.records[0].data["PARENTROWID"] != data.records[0].parentNode.data["ROW_ID"]) {
        this.ChangeSubMaterial.call(this, data.records[0], data.records[0].parentNode.data["SUBMATERIALID"]);
        data.records[0].set("PARENTROWID", data.records[0].parentNode.data["ROW_ID"]);
        data.records[0].set("PMATERIALID", data.records[0].parentNode.data["SUBMATERIALID"]);
        data.records[0].set("PMATERIALNAME", data.records[0].parentNode.data["SUBMATERIALNAME"]);
        var foundRow = this.dataSet.FindRow(1, data.records[0].data["ROW_ID"]);
        if (foundRow) {
            foundRow.set("PARENTROWID", data.records[0].parentNode.data["ROW_ID"]);
            foundRow.set("PMATERIALID", data.records[0].parentNode.data["SUBMATERIALID"]);
            foundRow.set("PMATERIALNAME", data.records[0].parentNode.data["SUBMATERIALNAME"]);
        }
    }

    //改变序号
    for (var i = 0; i < data.records[0].parentNode.childNodes.length; i++) {
        data.records[0].parentNode.childNodes[i].set("ORDERNUM", i + 1);
        var foundRow = this.dataSet.FindRow(1, data.records[0].parentNode.childNodes[i].get("ROW_ID"));
        if (foundRow) {
            foundRow.set("ORDERNUM", i + 1);
        }
    }

    //改变层级
    if (data.records[0].data["BOMLEVEL"] != data.records[0].parentNode.data["BOMLEVEL"] + 1) {
        data.records[0].set("BOMLEVEL", data.records[0].parentNode.data["BOMLEVEL"] + 1);
        var Rowid = new Array();
        var BomLevel = new Array();
        Rowid.push(data.records[0].data["ROW_ID"]);
        BomLevel.push(data.records[0].parentNode.data["BOMLEVEL"] + 1);
        ChangeChildLevel(data.records[0].childNodes);
        for (var i = 0; i < Rowid.length; i++) {
            var foundRow = this.dataSet.FindRow(1, Rowid[i]);
            if (foundRow) {
                foundRow.set("BOMLEVEL", BomLevel[i]);
            }
        }
    }
    function ChangeChildLevel(child) {
        if (child.length > 0) {
            for (var i = 0; i < child.length; i++) {
                Rowid.push(child[i].get("ROW_ID"));
                BomLevel.push(child[i].parentNode.get("BOMLEVEL") + 1);
                child[i].set("BOMLEVEL", child[i].parentNode.get("BOMLEVEL") + 1);
                if (child[i].childNodes.length > 0) {
                    ChangeChildLevel(child[i].childNodes);
                }
            }
        }
    }
};
//填充数据
proto.GetTreeStoreData = function (gen, fillStore, rowId) {
    var obj;
    var masterRow = this.dataSet.getTable(0).data.items[0];
    if (gen) {
        var isLeaf = false;
        var items = this.GetTreeStoreData(false, fillStore, rowId);
        if (items.length == 0)
            isLeaf = true;

        obj = {
            ROW_ID: 0,
            ORDERNUM:"",
            BOMLEVEL: 0,
            PARENTROWID: "",
            NODENAME: "",
            SUBMATERIALID: this.dataSet.getTable(0).data.items[0].data["MATERIALID"],
            SUBMATERIALNAME: this.dataSet.getTable(0).data.items[0].data["MATERIALNAME"],
            SUBMATERIALSPEC: this.dataSet.getTable(0).data.items[0].data["SUBMATERIALSPEC"],
            UNITID: this.dataSet.getTable(0).data.items[0].data["UNITID"],
            UNITNAME: this.dataSet.getTable(0).data.items[0].data["UNITNAME"],
            ISKEY: "",
            MATSTYLE: null,
            ATTRIBUTEID: this.dataSet.getTable(0).data.items[0].data["ATTRIBUTEID"],
            ATTRIBUTENAME: this.dataSet.getTable(0).data.items[0].data["ATTRIBUTENAME"],
            ATTRIBUTECODE: this.dataSet.getTable(0).data.items[0].data["ATTRIBUTECODE"],
            ATTRIBUTEDESC: this.dataSet.getTable(0).data.items[0].data["ATTRIBUTEDESC"],
            BASEQTY: "",
            UNITQTY: "",
            PRODUCEQTY: "",
            QUANTITY: "",
            DIFFERENCE: "",
            PMATERIALID: "",
            PMATERIALNAME: "",
            SALETECHROUTEID: "",
            SALETECHROUTENAME: "",
            SALETECHROUTEROWID: "",
            ISNONSTANDARDCONFIRM: "",
            PARENTROWID: "",
            WORKPROCESSNO: "",
            WORKSHOPSECTIONID: "",
            WORKSHOPSECTIONNAME: "",
            WORKPROCESSID: "",
            WORKPROCESSNAME: "",
            BUFFERNUM: "",
            ISNONSTANDARDCONFIRM: "",
            expanded: true,//展开
            leaf: isLeaf,
            children: items,
            disabled: true,
            //iconCls: 'tree-open'
        }
    }
    else {
        obj =[];
        var models = this.GetModelCollection(fillStore, rowId);
        for (var i = 0; i < models.length; i++) {
            var isLeaf = false;//是否有下一层级
            var isexpanded = false; //是否展开层级
            var record = models[i];
            this.level++;
            if (this.level > this.levelCount)
                this.levelCount = this.level;
            var items = this.GetTreeStoreData.call(this, false, fillStore, record.data['ROW_ID']);
            if (record.data['BOMLEVEL'] == 1) {
                isexpanded = true;
            }
            this.level--;

            var charString = {};
            for (var key in record.data) {
                charString[key] = record.data[key];
            }
            charString["PARENTROWID"] = rowId;
            charString["expanded"] = isexpanded;//是否展开层级
            charString["leaf"] = false;
            charString["children"] = items;//是否有下一层
            obj.push(charString);
        }
    }
    return obj;
}
//获取子集数据
proto.GetModelCollection = function (fillStore, rowid) {
    var storeItems = fillStore.data.items;
    var modelCollection = new Array();
    for (var i = 0; i < fillStore.data.length; i++) {
        if (storeItems[i].data['PARENTROWID'] == rowid) {
            modelCollection.push(storeItems[i]);
        }
    }
    return modelCollection;
}
//撤销按钮
proto.cancel = function () {

    this.dataSet.rejectChanges();
    var masterRow = this.dataSet.getTable(0).data.items[0];
    for (var i = 0; i < this.forms.length; i++) {
        this.forms[i].loadRecord(masterRow);
    };
    this.tree.setRootNode(this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0));
};
//刷新按钮
proto.browseTo = function (condition) {
    var data = this.invorkBcf("BrowseTo", [condition]);
    this.setDataSet(data, false);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    for (var i = 0; i < this.forms.length; i++) {
        this.forms[i].loadRecord(masterRow);
    };
    if (this.firstLoad == false) {
        this.tree.setRootNode(this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0));
    }

};
//保存方案
proto.saveDisplayScheme = function () {
    var displayScheme = { ProgId: this.progId, GridScheme: {} };
    for (var i = 0; i < this.dataSet.dataList.length; i++) {
        var gridScheme = { GridFields: [] };
        var columns = this.tree.headerCt.items.items;
        if (columns.length == 0) {
            gridScheme = this.subGridScheme[i];
        } else {
            var buildBandCol = function (bandColumn, list) {
                list.push({ Field: { Name: bandColumn.dataIndex, Width: bandColumn.width } });
            }
            for (var l = 0; l < columns.length; l++) {
                if (columns.xtype == "rownumberer" || columns[l].hidden === true)
                    continue;
                buildBandCol(columns[l], gridScheme.GridFields);
            }
        }
        if (gridScheme != undefined)
            displayScheme.GridScheme[i] = gridScheme;
    }
    var call = function (displayScheme) {
        Ext.Ajax.request({
            url: '/billSvc/saveDisplayScheme',
            jsonData: { handle: UserHandle, progId: this.progId, entryParam: Ext.encode(this.entryParam), displayScheme: Ext.encode(displayScheme) },
            method: 'POST',
            async: false,
            timeout: 90000000
        });
    }
    call.apply(this, [displayScheme]);
}
//导入
proto.importData = function (fileName) {
    var assistObj = {};
    var data = this.invorkBcf('ImportData', [fileName, this.entryParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success) {
        this.restData(false, BillActionEnum.Browse, data);
        this.tree.setRootNode(this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0));
    }
    return success;
}
//复制
proto.doSetParam = function (paramList) {
    this.tree.setRootNode(this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0));
}
//保存后将原始数据存入订单BOM记录表中
proto.doSave = function () {
    var assistObj = {};
    var saleBOMId = this.dataSet.getTable(0).data.items[0].get('SALEBOMID');
    var data = this.save(this.billAction, this.currentPk, assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success) {
        this.invorkBcf('PostLogSheet', [saleBOMId, this.originalValue]);
        this.restData(false, BillActionEnum.Browse, data);
    }
    return success;
};

//差异修改
//判断差异字段是否有值—>
//判断是否为新增行—>
//判断全局变量中是否含有此行
proto.DifFerence = function (e) {
    var row = e.dataInfo.dataRow;
    var list = this.originalValue;
    if (row.get('DIFFERENCE') == "") {
        var newList = {};
        newList.RowId = row.get('ROW_ID');
        newList.SubMaterialId = row.get('SUBMATERIALID');
        newList.BaseQty = row.get('BASEQTY');
        newList.UnitQty = row.get('UNITQTY');
        newList.AttributeCode = row.get('ATTRIBUTECODE');
        newList.AttributeDesc = row.get('ATTRIBUTEDESC');
        newList.PMaterialId = row.get('PMATERIALID');
        this.originalValue.push(newList);
        var modify = [];
        var c = {};
        c.Add = false;
        c.Modify = [e.dataInfo.fieldName];
        row.set('DIFFERENCE', JSON.stringify(c));
        var foundRow = this.dataSet.FindRow(1, row.data['ROW_ID']);
        if (foundRow) {
            foundRow.set("DIFFERENCE", JSON.stringify(c));
        }
        if (e.dataInfo.fieldName == 'SUBMATERIALID') {
            var rootNode = this.tree.getSelectionModel().getSelected().items[0];
            this.ChangeSubMaterial.call(this, rootNode, e.dataInfo.value);
        }
    }
    else {
        var b = true;//判断全局变量 this.originalValue 中是否有此行的值
        var c = JSON.parse(row.data['DIFFERENCE']);
        if (!c.Add) {
            if (list != undefined) {
                for (var i = 0; i < list.length; i++) {
                    if (list[i].RowId == row.get('ROW_ID')) {
                        b = false;
                        this.difference2.call(this, e, list[i], c);
                        break;
                    }
                }
            }
            if (b) {
                var data = this.invorkBcf('LogbookDt', [row.data['SALEBOMID'], row.data['ROW_ID']]);
                for (var i = 0; i < data.length; i++) {
                    if (data[i].RowId == row.data['ROW_ID']) {
                        this.difference2.call(this, e, data[i], c);
                    }
                    this.originalValue.push({
                        RowId: data[i].RowId,
                        SubMaterialId: data[i].SubMaterialId,
                        BaseQty: data[i].BaseQty,
                        UnitQty: data[i].UnitQty,
                        AttributeCode: data[i].AttributeCode,
                        AttributeDesc: data[i].AttributeDesc,
                        PMaterialId: data[i].PMaterialId

                    });
                }
            }
            row.set('DIFFERENCE', JSON.stringify(c));
            var foundRow = this.dataSet.FindRow(1, row.data['ROW_ID']);
            if (foundRow) {
                foundRow.set("DIFFERENCE", JSON.stringify(c));
            }
        }
    }
}
proto.difference2 = function (e, o, c) {
    var modify = [];
    var row = e.dataInfo.dataRow;
    if (e.dataInfo.fieldName == 'SUBMATERIALID') {
        if (e.dataInfo.value != o.SubMaterialId) {
            modify.push('SUBMATERIALID');
        }
        var rootNode = this.tree.getSelectionModel().getSelected().items[0];
        this.ChangeSubMaterial.call(this, rootNode, e.dataInfo.value);
    }
    else {
        if (row.get('SUBMATERIALID') != o.SubMaterialId) {
            modify.push('SUBMATERIALID');
        }
        var rootNode = this.tree.getSelectionModel().getSelected().items[0];
        this.ChangeSubMaterial.call(this, rootNode, e.dataInfo.value);
    }
    if (e.dataInfo.fieldName == 'BASEQTY') {
        if (e.dataInfo.value != o.BaseQty)
            modify.push('BASEQTY');
    }
    else {
        if (row.get('BASEQTY') != o.BaseQty)
            modify.push('BASEQTY');
    }
    if (e.dataInfo.fieldName == 'UNITQTY') {
        if (e.dataInfo.value != o.UnitQty)
            modify.push('UNITQTY');
    }
    else {
        if (row.get('UNITQTY') != o.UnitQty)
            modify.push('UNITQTY');
    }
    if (e.dataInfo.fieldName == 'ATTRIBUTECODE') {
        if (e.dataInfo.value != o.AttributeCode)
            modify.push('ATTRIBUTECODE');
    }
    else {
        if (row.get('ATTRIBUTECODE') != o.AttributeCode)
            modify.push('ATTRIBUTECODE');
    }
    if (e.dataInfo.fieldName == 'ATTRIBUTEDESC') {
        if (e.dataInfo.value != o.AttributeDesc)
            modify.push('ATTRIBUTEDESC');
    }
    else {
        if (row.get('ATTRIBUTEDESC') != o.AttributeDesc)
            modify.push('ATTRIBUTEDESC');
    }
    if (e.dataInfo.fieldName == 'PMATERIALID') {
        if (e.dataInfo.value != o.PMaterialId)
            modify.push('PMATERIALID');
    }
    else {
        if (row.get('PMATERIALID') != o.PMaterialId)
            modify.push('PMATERIALID');
    }
    c.Modify = modify;
}
//改变物料后更改子节点的different状态
proto.ChangeSubMaterial = function (rootNode, value) {
    var list = this.originalValue;
    var child = rootNode.childNodes
    if (child.length != 0) {
        for (var i = 0; i < child.length; i++) {
            if (value !== child[i].get("PMATERIALID")) {
                if (child[i].get('DIFFERENCE') == "") {
                    var newList = {};
                    newList.RowId = child[i].get('ROW_ID');
                    newList.SubMaterialId = child[i].get('SUBMATERIALID');
                    newList.BaseQty = child[i].get('BASEQTY');
                    newList.UnitQty = child[i].get('UNITQTY');
                    newList.AttributeCode = child[i].get('ATTRIBUTECODE');
                    newList.AttributeDesc = child[i].get('ATTRIBUTEDESC');
                    newList.PMaterialId = child[i].get('PMATERIALID');
                    this.originalValue.push(newList);
                    var modify = [];
                    var c = {};
                    c.Add = false;
                    c.Modify = ["PMATERIALID"];
                    child[i].set('DIFFERENCE', JSON.stringify(c));
                    var foundRow = this.dataSet.FindRow(1, child[i].data['ROW_ID']);
                    if (foundRow) {
                        foundRow.set("DIFFERENCE", JSON.stringify(c));
                    }
                }
                else {
                    var b = true;//判断全局变量 this.originalValue 中是否有此行的值
                    var c = JSON.parse(child[i].data['DIFFERENCE']);
                    //add为false
                    if (!c.Add) {
                        //数据存在前端集合中
                        if (list != undefined) {
                            for (var j = 0; j < list.length; j++) {
                                if (list[j].RowId == child[i].get('ROW_ID')) {
                                    b = false;
                                    this.MakeUpDifference.call(this, child[i], list[j], c, value);
                                    break;
                                }
                            }
                        }
                        //数据不在前端集合中
                        if (b) {
                            var data = this.invorkBcf('LogbookDt', [child[i].data['SALEBOMID'], child[i].data['ROW_ID']]);
                            for (var j = 0; j < data.length; j++) {
                                if (data[j].RowId == child[i].data['ROW_ID']) {
                                    this.MakeUpDifference.call(this, child[i], data[j], c, value);
                                }
                                this.originalValue.push({
                                    RowId: data[j].RowId,
                                    SubMaterialId: data[j].SubMaterialId,
                                    BaseQty: data[j].BaseQty,
                                    UnitQty: data[j].UnitQty,
                                    AttributeCode: data[j].AttributeCode,
                                    AttributeDesc: data[j].AttributeDesc,
                                    PMaterialId: data[j].PMaterialId

                                });
                            }
                        }
                        child[i].set('DIFFERENCE', JSON.stringify(c));
                        var foundRow = this.dataSet.FindRow(1, child[i].data['ROW_ID']);
                        if (foundRow) {
                            foundRow.set("DIFFERENCE", JSON.stringify(c));
                        }
                    }
                }
            }
        }
    }
}
proto.MakeUpDifference = function (node, o, c, value) {
    var modify = [];
    if (node.get('SUBMATERIALID') != o.SubMaterialId) {
        modify.push('SUBMATERIALID');
    }
    if (node.get('BASEQTY') != o.BaseQty) {
        modify.push('BASEQTY');
    }
    if (node.get('UNITQTY') != o.UnitQty) {
        modify.push('UNITQTY');
    }
    if (node.get('ATTRIBUTECODE') != o.AttributeCode) {
        modify.push('ATTRIBUTECODE');
    }
    if (node.get('ATTRIBUTEDESC') != o.AttributeDesc) {
        modify.push('ATTRIBUTEDESC');
    }
    if (value != o.PMaterialId) {
        modify.push('PMATERIALID');
    }
    c.Modify = modify;
}

//复制
proto.fillCopyData = function (dataRow, parentItem, isCurrentRow) {
    var copyItem = {};
    copyItem.dataRow = dataRow;
    copyItem.attributesDetail = new Array();
    copyItem.children = new Array();
    var attributeDetails = this.dataSet.getChildren(1, dataRow, 2);
    for (var i = 0; i < attributeDetails.length; i++) {
        var attributeDetailRow = attributeDetails[i];
        var attributesDetail = {};
        attributesDetail.dataRow = attributeDetailRow;
        copyItem.attributesDetail.push(attributesDetail);
    }
    if (isCurrentRow) {
        this.copyData = copyItem;
    }
    else {
        parentItem.children.push(copyItem);
    }
    var childrenRows = this.GetModelCollection(this.dataSet.getTable(1), dataRow.get("ROW_ID"));
    for (var i = 0; i < childrenRows.length; i++) {
        this.fillCopyData(childrenRows[i], copyItem, false);
    }

}

//粘贴
proto.pasteData = function (currNode, parentNode, currCopyItem, isCurrentNode) {
    var headerRow = this.dataSet.getTable(0).data.items[0];
    var newRow = this.addRow(headerRow, 1);
    var copyItem, bindingRow;
    if (isCurrentNode) {
        if (this.copyData.children) {
            copyItem = this.copyData;
        }
        else {
            var copyData = DesktopApp.copyData;
            if (copyData) {
                copyItem = copyData;
            }
        }
        bindingRow = currNode.bindingRow || this.dataSet.FindRow(1, currNode.get("ROW_ID"));
        newRow.set("PARENTROWID", bindingRow.get("PARENTROWID"));
        newRow.set("BOMLEVEL", bindingRow.get("BOMLEVEL"));
        newRow.set("PMATERIALID", currNode.parentNode.get("SUBMATERIALID"));
        newRow.set("PMATERIALNAME", currNode.parentNode.get("SUBMATERIALNAME"));
        newRow.set("DIFFERENCE", '{"Add":true}');
        newRow.set("ISNONSTANDARDCONFIRM", false);
    }
    else {
        currNode = parentNode;
        copyItem = currCopyItem;
        bindingRow = currNode.bindingRow || this.dataSet.FindRow(1, currNode.get("ROW_ID"));
        newRow.set("PARENTROWID", bindingRow.get("ROW_ID"));
        newRow.set("BOMLEVEL", bindingRow.get("BOMLEVEL") + 1);
        newRow.set("PMATERIALID", bindingRow.get("SUBMATERIALID"));
        newRow.set("PMATERIALNAME", bindingRow.get("SUBMATERIALNAME"));
        newRow.set("DIFFERENCE", '{"Add":true}');
        newRow.set("ISNONSTANDARDCONFIRM", false);
    }
    for (var fieldName in copyItem.dataRow.data) {
        if (fieldName != "SALEBOMID" && fieldName != "PARENTROWID" && fieldName != "ROW_ID" && fieldName != "BOMLEVEL" && fieldName != "PMATERIALID" && fieldName != "PMATERIALNAME" && fieldName != "DIFFERENCE" && fieldName != "ISNONSTANDARDCONFIRM" && fieldName != "id") {
            newRow.set(fieldName, copyItem.dataRow.get(fieldName));
        }
    }
    var newNode = {};
    for (var fieldName in newRow.data) {
        newNode[fieldName] = newRow.get(fieldName);
    }
    newNode.expanded = true;
    if (isCurrentNode) {
        if (currNode.parentNode == null) {
            newNode = currNode.appendChild(newNode);
        } else {
            newNode = currNode.parentNode.appendChild(newNode);
        }
    }
    else {
        newNode = currNode.appendChild(newNode);
    }
    for (var i = 0 ; i < copyItem.children.length; i++) {
        this.pasteData(currNode, newNode, copyItem.children[i], false);
    }
}


