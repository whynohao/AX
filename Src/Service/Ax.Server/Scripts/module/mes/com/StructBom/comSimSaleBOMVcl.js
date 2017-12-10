comSimSaleBOMVcl = function () {
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

var proto = comSimSaleBOMVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comSimSaleBOMVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            if (this.billAction == BillActionEnum.Modif || this.billAction == BillActionEnum.AddNew) {
                switch (e.dataInfo.fieldName) {
                    case "ATTRIBUTENAME":
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
                            this.CreatAttForm(dataList, returnData, this, e, this.FillDataRow);
                        }
                        break;
                }
            }
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                if (this.tree.getSelectionModel().getSelected().items[0].get("BOMLEVEL") == 0) {
                    e.dataInfo.cancel = true;
                    Ext.Msg.alert("不能修改根节点");
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
                    var list = this.invorkBcf("GetMaterialFromSubRowId", [billno, rowid,subRowId]);
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
                    this.GetProduceQty.call(this,this.tree.store.data.items[0], this.dataSet, e.dataInfo.value, e.dataInfo.dataRow.get("ISUNITEXPENDED"));
                }
                else if (e.dataInfo.fieldName == "ISUNITEXPENDED") {
                    this.GetProduceQty.call(this,this.tree.store.data.items[0], this.dataSet, e.dataInfo.dataRow.get("QUANTITY"), e.dataInfo.value);
                }
                this.forms[0].updateRecord(e.dataInfo.dataRow)
            }
            if (e.dataInfo.tableIndex == 1) {
               
                //绑定
                function getBindingRow() {
                    //var items = this.dataSet.getTable(1).data.items;
                    //var bindingRow;
                    //for (var i = 0; i < items.length; i++) {
                    //    if (e.dataInfo.dataRow.get("ROW_ID") == items[i].get("ROW_ID")) {
                    //        bindingRow = items[i];
                    //        break;
                    //    }
                    //}
                    return this.dataSet.FindRow(1, e.dataInfo.dataRow.get("ROW_ID"));;
                }
                var bindingRow = e.dataInfo.dataRow.bindingRow || getBindingRow.call(this);
                bindingRow.set(e.dataInfo.fieldName, e.dataInfo.value);

                //子件变化后修改子节点的上级物料
                if (e.dataInfo.fieldName === "SUBMATERIALID") {
                    if (e.dataInfo.value != e.dataInfo.oldValue) {
                        this.DifFerence.call(this, e);
                    }
                    var rootNode = this.tree.getSelectionModel().getSelected().items[0];
                    var PRowid = rootNode.get("ROW_ID");
                    var Pmaterialid;
                    //for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                    //    if (this.dataSet.getTable(1).data.items[i].data["ROW_ID"] == PRowid) {
                    //        Pmaterialid = this.dataSet.getTable(1).data.items[i].data["SUBMATERIALID"];
                    //        break;
                    //    }
                    //}
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
                    this.UpdateIsConfirm.call(this, e.dataInfo.dataRow,false);
                }
                //母件基数，单位用量,生产数量的联动
                if (e.dataInfo.fieldName === "BASEQTY") {
                    //if (Ext.getCmp('ISUNITEXPENDED0_' + this.winId).getValue() == false) {
                    var quantity;
                    if (e.dataInfo.dataRow.get("PARENTROWID") == 0) {
                        quantity = Ext.getCmp('QUANTITY0_' + this.winId).getValue();
                    }
                    else {
                        //for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                        //    if (this.dataSet.getTable(1).data.items[i].data["ROW_ID"] == e.dataInfo.dataRow.get("PARENTROWID")) {
                        //        quantity = this.dataSet.getTable(1).data.items[i].data["PRODUCEQTY"];
                        //        break;
                        //    }
                        //}
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
                        //for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                        //    if (this.dataSet.getTable(1).data.items[i].data["ROW_ID"] == e.dataInfo.dataRow.get("ROW_ID")) {
                        //        this.dataSet.getTable(1).data.items[i].set("PRODUCEQTY", produceqty);
                        //        break;
                        //    }
                        //}
                        var foundRow = this.dataSet.FindRow(1, e.dataInfo.dataRow.get("ROW_ID"));
                        if (foundRow) {
                            foundRow.set("PRODUCEQTY", produceqty);
                        }
                        //修改子节点的生产应用量
                        var Node = this.tree.getSelectionModel().getSelected().items[0];
                        this.ChangChildProduceQty.call(this, Node);
                    }
                    //}
                    //else {
                    //    var quantity = Ext.getCmp('QUANTITY0_' + this.winId).getValue();
                    //    var unitqty = e.dataInfo.dataRow.get("UNITQTY");
                    //    var baseqty = e.dataInfo.value;
                    //    if (quantity > 0) {
                    //        var produceqty = quantity * unitqty / baseqty;
                    //        //将结果填到生产应用量中
                    //        e.dataInfo.dataRow.set("PRODUCEQTY", produceqty);
                    //        //for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                    //        //    if (this.dataSet.getTable(1).data.items[i].data["ROW_ID"] == e.dataInfo.dataRow.get("ROW_ID")) {
                    //        //        this.dataSet.getTable(1).data.items[i].set("PRODUCEQTY", produceqty);
                    //        //        break;
                    //        //    }
                    //        //}
                    //        var foundRow = this.dataSet.FindRow(1, e.dataInfo.dataRow.get("ROW_ID"));
                    //        if (foundRow)
                    //        {
                    //            foundRow.set("PRODUCEQTY", produceqty);
                    //        }
                    //    }
                    //}
                }
                if (e.dataInfo.fieldName === "UNITQTY") {
                    //if (Ext.getCmp('ISUNITEXPENDED0_' + this.winId).getValue() == false) {
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
                    //}
                    //else {
                    //    var quantity = Ext.getCmp('QUANTITY0_' + this.winId).getValue();
                    //    var baseqty = e.dataInfo.dataRow.get("BASEQTY");
                    //    var unitqty = e.dataInfo.value;
                    //    if (quantity > 0) {
                    //        var produceqty = quantity * unitqty / baseqty;
                    //        //将结果填到生产应用量中
                    //        e.dataInfo.dataRow.set("PRODUCEQTY", produceqty);
                    //        //for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                    //        //    if (this.dataSet.getTable(1).data.items[i].data["ROW_ID"] == e.dataInfo.dataRow.get("ROW_ID")) {
                    //        //        this.dataSet.getTable(1).data.items[i].set("PRODUCEQTY", produceqty);
                    //        //        break;
                    //        //    }
                    //        //}
                    //        var foundRow = this.dataSet.FindRow(1,e.dataInfo.dataRow.get("ROW_ID"));
                    //        if (foundRow)
                    //        {
                    //            foundRow.set("PRODUCEQTY", produceqty);
                    //        }
                    //    }
                    //}
                }
            }
            break;
    }
}

proto.reCreateSaleBom = function () {
    var success = false;
    var saleOrderInfo = {};
    if (this.isEdit) {
        var masterRow = this.dataSet.getTable(0).data.items[0];
        saleOrderInfo.SaleBomId = masterRow.get("SALEBOMID");
        saleOrderInfo.ReCreateSaleBom = true;
        saleOrderInfo.AttributeCode = masterRow.get("ATTRIBUTECODE");
        saleOrderInfo.AttributeDesc = masterRow.get("ATTRIBUTEDESC");
        saleOrderInfo.AttributeId = masterRow.get("ATTRIBUTEID");
        saleOrderInfo.BomConfirm = masterRow.get("ISABNORMALCONFIRM");
        saleOrderInfo.DetailRemark = masterRow.get("REMARK");
        saleOrderInfo.GroupId = masterRow.get("GROUPNO");
        saleOrderInfo.IsAbnormal = masterRow.get("ISABNORMAL");
        saleOrderInfo.LotNo = masterRow.get("LOTNO");
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
        Ext.Msg.alert("提示", "编辑状态才能重新生成订单BOM！");
    }
    if (success) {
        //重新刷新页面
        this.browseTo([saleOrderInfo.SaleBomId]);
        Ext.Msg.alert("提示", "重新生成订单BOM成功！");
    }
}
proto.removeDetail = function () {
    if (this.isEdit) {
        this.dataSet.getTable(1).removeAll();
        this.tree.setRootNode(this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0));
    }
    else {
        Ext.Msg.alert("提示", "编辑状态才能清空订单BOM明细！");
    }
}
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
    //debugger;
    //headTable = this.dataSet.getTable(0).data.items[0];
    //bodyTable = this.dataSet.getTable(1);
    //var MaterialiId = headTable.data['MATERIALID'];
    //var MaterialiName = headTable.data['MATERIALNAME'];
    //var MaterialiSpec = headTable.data['MATERIALSPEC'];
    //var AttributeDesc = headTable.data['ATTRIBUTEDESC'];
    //var Quantity = headTable.data['QUANTITY'];
    //if (MaterialiSpec == "") {
    //    MaterialiSpec = "无";
    //}

    var strHead = "<div align=center>"
    strHead += " <strong><font size = '5px'> 订单BOM </font></strong>"; "</div>";
    //strHead += "</div>";
    //strHead += "<div >";
    //strHead += "<p>产品编码: " + MaterialiId + "  产品名称: " + MaterialiName + "  产品型号: " + MaterialiSpec + "  产品数量: " + Quantity + "</p>";
    //strHead += "<p>产品特征描述: " + AttributeDesc + "</p>";
    //strHead += "</div>";

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
        if (SubmaterialSpec==undefined) {
            SubmaterialSpec = "";
        }
        var AttributeDesc = bodyItems.get("ATTRIBUTEDESC");
        var ProduceQty = bodyItems.get("PRODUCEQTY");

        var td = "<tr>";
        td += "<td nowrap align='center'><font size = '2px'>" + SubMaterialId + "</font></td>";
        td += "<td nowrap align='center'><font size = '2px'>" + SubMaterialName + "</font></td>";
        td += "<td nowrap align='center'><font size = '2px'>" + SubmaterialSpec + "</font></td>";
        td += "<td  align='center'><font size = '2px'>" + AttributeDesc + "</font></td>";
        td += "<td nowrap align='center'><font size = '2px'>" + ProduceQty + "</font></td>";
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
//最新特征窗体
proto.CreatAttForm=function(dataList, returnData, This, row, FillDataRow) {
    var MaterialId = dataList.MaterialId;
    var AttributeId = dataList.AttributeId;
    var AttributeName = dataList.AttributeName;
    var AttributeCode = dataList.AttributeCode;
    var standard = [];
    var unstandard = [];
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
            var thisWin = Ext.getCmp("attWin" + row.dataInfo.dataRow.data["ROW_ID"] + MaterialId);
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
                var yes = FillDataRow(row, This, CodeDesc);
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
            Ext.getCmp("attWin" + row.data["ROW_ID"] + MaterialId).close();
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
        id: "attWin" + row.data["ROW_ID"] + MaterialId,
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
//填充当前行特征信息
proto.FillDataRow = function (e, This, CodeDesc) {
    e.dataInfo.dataRow.set("ATTRIBUTECODE", CodeDesc.Code);
    e.dataInfo.dataRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
    var foundRow = This.dataSet.FindRow(1, e.dataInfo.dataRow.get("ROW_ID"));
    if (foundRow);
    {
        foundRow.set("ATTRIBUTECODE", CodeDesc.Code);
        foundRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
    }
    //for (var i = 0; i < This.dataSet.getTable(1).data.items.length; i++) {
    //    if (This.dataSet.getTable(1).data.items[i].data["ROW_ID"] == e.dataInfo.dataRow.get("ROW_ID")) {
    //        This.dataSet.getTable(1).data.items[i].set("ATTRIBUTECODE", CodeDesc.Code);
    //        This.dataSet.getTable(1).data.items[i].set("ATTRIBUTEDESC", CodeDesc.Desc);
    //        break;
    //    }
    //}
    return true;
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
    return textbox;
}
//是否确认物料的更新验证
proto.UpdateIsConfirm = function (curRow,isDelete){
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
        var foundRow = curDataSet.FindRow(1, curNode.childNodes[i].get("ROW_ID"));
        if (foundRow) {
            foundRow.set("PRODUCEQTY", curNode.childNodes[i].get("PRODUCEQTY"));
        }
        this.GetProduceQty.call(this,curNode.childNodes[i], curDataSet, dataInfoValue, dataIsExpended);
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
                quantity: Node.get("PRODUCEQTY")
            });
            unitqty = child[i].get("UNITQTY");
            baseqty = child[i].get("BASEQTY");
            quantity = Node.get("PRODUCEQTY");
            child[i].set("PRODUCEQTY", quantity * unitqty / baseqty);
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
        if (foundRow)
        {
            foundRow.set("PRODUCEQTY", data[i].quantity * unitqty / baseqty);
        }
    }
    for (var i = 0; i < child.length; i++) {
        this.ChangChildProduceQty.call(this, child[i]);
    }    
}
//差异字段中是否包含此列名
proto.IsContain = function (modifiy,columnName) {
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
        this.ChangeSubMaterial.call(this, data.records[0].parentNode, data.records[0].parentNode.data["SUBMATERIALID"]);
        data.records[0].set("PARENTROWID", data.records[0].parentNode.data["ROW_ID"]);
        data.records[0].set("PMATERIALID", data.records[0].parentNode.data["SUBMATERIALID"]);
        data.records[0].set("PMATERIALNAME", data.records[0].parentNode.data["SUBMATERIALNAME"]);
        //for (var j = 0; j < this.dataSet.getTable(1).data.items.length; j++) {
        //    if (this.dataSet.getTable(1).data.items[j].data["ROW_ID"] == data.records[0].data["ROW_ID"]) {
        //        this.dataSet.getTable(1).data.items[j].set("PARENTROWID", data.records[0].parentNode.data["ROW_ID"]);
        //        this.dataSet.getTable(1).data.items[j].set("PMATERIALID", data.records[0].parentNode.data["SUBMATERIALID"]);
        //        this.dataSet.getTable(1).data.items[j].set("PMATERIALNAME", data.records[0].parentNode.data["SUBMATERIALNAME"]);
        //        break;
        //    }
        //}

        var foundRow = this.dataSet.FindRow(1, data.records[0].data["ROW_ID"]);
        if (foundRow)
        {
            foundRow.set("PARENTROWID", data.records[0].parentNode.data["ROW_ID"]);
            foundRow.set("PMATERIALID", data.records[0].parentNode.data["SUBMATERIALID"]);
            foundRow.set("PMATERIALNAME", data.records[0].parentNode.data["SUBMATERIALNAME"]);
        }
    }



    //改变序号
    for (var i = 0; i < data.records[0].parentNode.childNodes.length; i++) {
        data.records[0].parentNode.childNodes[i].set("ORDERNUM", i + 1);
        //for (var j = 0; j < this.dataSet.getTable(1).data.items.length; j++) {
        //    if (this.dataSet.getTable(1).data.items[j].data["ROW_ID"] == data.records[0].parentNode.childNodes[i].get("ROW_ID")) {
        //        this.dataSet.getTable(1).data.items[j].set("ORDERNUM", i + 1);
        //    }
        //}
        var foundRow = this.dataSet.FindRow(1, data.records[0].parentNode.childNodes[i].get("ROW_ID"));
        if (foundRow)
        {
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
            //for (var j = 0; j < this.dataSet.getTable(1).data.items.length; j++) {
            //    if (this.dataSet.getTable(1).data.items[j].data["ROW_ID"] == Rowid[i]) {
            //        this.dataSet.getTable(1).data.items[j].set("BOMLEVEL", BomLevel[i]);
            //        break;
            //    }
            //}
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
    if (gen) {
        var isLeaf = false;

        var items = this.GetTreeStoreData(false, fillStore, rowId);
        if (items.length == 0)
            isLeaf = true;
        obj = {
            ROW_ID: 0,
            ORDERNUM: "",
            BOMLEVEL: 0,
            PARENTROWID: "",
            NODENAME: "",
            SUBMATERIALID: this.dataSet.getTable(0).data.items[0].data["MATERIALID"],
            SUBMATERIALNAME: this.dataSet.getTable(0).data.items[0].data["MATERIALNAME"],
            SUBMATERIALSPEC: "",
            UNITID: "",
            UNITNAME: "",
            ISKEY: "",
            MATSTYLE: null,
            ATTRIBUTEID: "",
            ATTRIBUTENAME: "",
            ATTRIBUTECODE: this.dataSet.getTable(0).data.items[0].data["ATTRIBUTECODE"],
            ATTRIBUTEDESC: this.dataSet.getTable(0).data.items[0].data["ATTRIBUTEDESC"],
            BASEQTY: "",
            UNITQTY: "",
            PRODUCEQTY:"",
            DIFFERENCE:"",
            PMATERIALID: "",
            PMATERIALNAME:"",
            SALETECHROUTEID: "",
            SALETECHROUTENAME: "",
            SALETECHROUTEROWID: "",
            ISNONSTANDARDCONFIRM:"",
            PARENTROWID: "",
            WORKPROCESSNO: "",
            WORKSHOPSECTIONID: "",
            WORKSHOPSECTIONNAME: "",
            WORKPROCESSID: "",
            WORKPROCESSNAME: "",
            OUTPUTWORKSHOPSECITONID: "",
            OUTPUTWORKSHOPSECTIONNAME:"",
            BUFFERNUM: "",
            ISNONSTANDARDCONFIRM:"",
            expanded: true,//展开
            leaf: isLeaf,
            children: items,
            //disabled:true,
            //iconCls: 'tree-open'
        }
    }
    else {
        obj = new Array();
        var models = this.GetModelCollection(fillStore, rowId);
        for (var i = 0; i < models.length; i++) {
            var isLeaf = false;//是否有下一层级
            var isexpanded = false; //是否展开层级
            var record = models[i];
            this.level++;
            if (this.level > this.levelCount)
                this.levelCount = this.level;

            var items = this.GetTreeStoreData.call(this, false, fillStore, record.data['ROW_ID']);
            //var iconCls = 'tree';
            
            if (record.data['BOMLEVEL'] == 1) {
                isexpanded = true;
            }
            this.level--;

            if (record.data['ORDERNUM']==0) {
                record.data['ORDERNUM'] = i + 1;
            }

            obj.push({
                SALEBOMID: record.data['SALEBOMID'],
                ROW_ID: record.data['ROW_ID'],
                PARENTROWID: rowId,
                ORDERNUM: record.data['ORDERNUM'],
                BOMLEVEL: record.data['BOMLEVEL'],
                NODENAME: record.data["NODENAME"],
                SUBMATERIALID: record.data['SUBMATERIALID'],
                SUBMATERIALNAME: record.data['SUBMATERIALNAME'],
                SUBMATERIALSPEC: record.data['SUBMATERIALSPEC'],
                UNITID: record.data['UNITID'],
                UNITNAME: record.data['UNITNAME'],
                ISKEY: record.data['ISKEY'],
                MATSTYLE: record.data['MATSTYLE'],
                ATTRIBUTEID: record.data['ATTRIBUTEID'],
                ATTRIBUTENAME: record.data['ATTRIBUTENAME'],
                ATTRIBUTECODE: record.data['ATTRIBUTECODE'],
                ATTRIBUTEDESC: record.data['ATTRIBUTEDESC'],
                BASEQTY: record.data['BASEQTY'],
                UNITQTY: record.data['UNITQTY'],
                PRODUCEQTY: record.data['PRODUCEQTY'],
                DIFFERENCE: record.data['DIFFERENCE'],
                PMATERIALID: record.data['PMATERIALID'],
                PMATERIALNAME: record.data['PMATERIALNAME'],
                SALETECHROUTEID: record.data['SALETECHROUTEID'],
                SALETECHROUTENAME: record.data['SALETECHROUTENAME'],
                SALETECHROUTEROWID:record.data['SALETECHROUTEROWID'],
                WORKPROCESSNO: record.data['WORKPROCESSNO'],
                WORKSHOPSECTIONID: record.data['WORKSHOPSECTIONID'],
                WORKSHOPSECTIONNAME: record.data['WORKSHOPSECTIONNAME'],
                WORKPROCESSID: record.data['WORKPROCESSID'],
                WORKPROCESSNAME: record.data['WORKPROCESSNAME'],
                OUTPUTWORKSHOPSECITONID: record.data['OUTPUTWORKSHOPSECITONID'],
                OUTPUTWORKSHOPSECTIONNAME: record.data['OUTPUTWORKSHOPSECTIONNAME'],
                ISNONSTANDARDCONFIRM: record.data['ISNONSTANDARDCONFIRM'],
                BUFFERNUM: record.data['BUFFERNUM'],
                ISNONSTANDARDCONFIRM:record.data['ISNONSTANDARDCONFIRM'],
                expanded: isexpanded,//展开
                leaf: false,
                children: items,
            });
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
    if (this.firstLoad==false) {
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
    if (row.get('DIFFERENCE')=="") {
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
        //for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
        //    if (this.dataSet.getTable(1).data.items[i].data["ROW_ID"] == row.data['ROW_ID']) {
        //        this.dataSet.getTable(1).data.items[i].set("DIFFERENCE", JSON.stringify(c));
        //    }
        //}
        var foundRow = this.dataSet.FindRow(1, row.data['ROW_ID']);
        if (foundRow)
        {
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
            //for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
            //    if (this.dataSet.getTable(1).data.items[i].data["ROW_ID"] == row.data['ROW_ID']) {
            //        this.dataSet.getTable(1).data.items[i].set("DIFFERENCE", JSON.stringify(c));
            //    }
            //}
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
proto.ChangeSubMaterial = function (rootNode, value) {
    var list = this.originalValue;
    var child = rootNode.childNodes
    if (child.length != 0) {
        for (var i = 0; i < child.length; i++) {
            if (value !== child[i].get("PMATERIALID")) {
                if (child[i].get('DIFFERENCE')=="") {
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
                    //for (var j = 0; j < this.dataSet.getTable(1).data.items.length; j++) {
                    //    if (this.dataSet.getTable(1).data.items[j].data["ROW_ID"] == child[i].data['ROW_ID']) {
                    //        this.dataSet.getTable(1).data.items[j].set("DIFFERENCE", JSON.stringify(c));
                    //    }
                    //}
                    var foundRow = this.dataSet.FindRow(1, child[i].data['ROW_ID']);
                    if (foundRow)
                    {
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
                        //for (var j = 0; j < this.dataSet.getTable(1).data.items.length; j++) {
                        //    if (this.dataSet.getTable(1).data.items[j].data["ROW_ID"] == child[i].data['ROW_ID']) {
                        //        this.dataSet.getTable(1).data.items[j].set("DIFFERENCE", JSON.stringify(c));
                        //    }
                        //}
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
proto.MakeUpDifference = function (node, o, c,value) {
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
        attributesDetail.noMatchParamDetails = new Array();
        //直接找第4张是没有数据的，应该还是找第6张表
        var store = this.dataSet.getTable(6);
        for (var j = 0; j < store.data.items.length; j++) {
            var record = store.data.items[j];
            if (record.get("STRUCTBOMID") == attributeDetailRow.get("STRUCTBOMID") && record.get("GRANDPARENTROWID") == attributeDetailRow.get("PARENTROWID") && record.get("PARENTROWID") == attributeDetailRow.get("ROW_ID")) {
                attributesDetail.noMatchParamDetails.push(record);
            }
        }
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
    }
    else {
        currNode = parentNode;
        copyItem = currCopyItem;
        bindingRow = currNode.bindingRow || this.dataSet.FindRow(1, currNode.get("ROW_ID"));
        newRow.set("PARENTROWID", bindingRow.get("ROW_ID"));
        newRow.set("BOMLEVEL", bindingRow.get("BOMLEVEL") + 1);
    }
    newRow.set("BOMLEVEL", bindingRow.get("BOMLEVEL"));
    for (var fieldName in copyItem.dataRow.data) {
        if (fieldName != "STRUCTBOMID" && fieldName != "PARENTROWID" && fieldName != "ROW_ID" && fieldName != "id") {
            newRow.set(fieldName, copyItem.dataRow.get(fieldName));
        }
    }
    for (var i = 0 ; i < copyItem.attributesDetail.length; i++) {
        var attributesDetailRow = copyItem.attributesDetail[i];
        var attributesNewRow = this.addRow(newRow, 2);
        for (var fieldName in attributesDetailRow.dataRow.data) {
            if (fieldName != "STRUCTBOMID" && fieldName != "PARENTROWID" && fieldName != "ROW_ID" && fieldName != "id") {
                attributesNewRow.set(fieldName, attributesDetailRow.dataRow.get(fieldName));
            }
        }
        for (var k = 0; k < attributesDetailRow.noMatchParamDetails.length; k++) {
            var noMatchParamDetailRow = attributesDetailRow.noMatchParamDetails[k];
            var noMatchParamDetailNewRow = this.addRow(attributesNewRow, 3);
            for (var fieldName in noMatchParamDetailRow.data) {
                if (fieldName != "STRUCTBOMID" && fieldName != "GRANDPARENTROWID" && fieldName != "PARENTROWID" && fieldName != "ROW_ID" && fieldName != "id" && fieldName != "FROMTABLE") {
                    noMatchParamDetailNewRow.set(fieldName, noMatchParamDetailRow.get(fieldName));
                }
            }
            var store = this.dataSet.getTable(6);
            var newRow1 = Ext.decode(this.tpl.Tables[this.dataSet.getTable(6).Name].NewRowObj);
            newRow1["STRUCTBOMID"] = noMatchParamDetailNewRow.get("STRUCTBOMID");
            newRow1["GRANDPARENTROWID"] = noMatchParamDetailNewRow.get("GRANDPARENTROWID");
            newRow1["PARENTROWID"] = noMatchParamDetailNewRow.get("PARENTROWID");
            newRow1["ROW_ID"] = noMatchParamDetailNewRow.get("ROW_ID");
            newRow1["PARAMID"] = noMatchParamDetailNewRow.get("PARAMID");
            newRow1["PARAMNAME"] = noMatchParamDetailNewRow.get("PARAMNAME");
            newRow1["PARAMVALUE"] = noMatchParamDetailNewRow.get("PARAMVALUE");
            newRow1["PARAMVALUENAME"] = noMatchParamDetailNewRow.get("PARAMVALUENAME");
            var newModel = store.add(newRow1)[0];
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
