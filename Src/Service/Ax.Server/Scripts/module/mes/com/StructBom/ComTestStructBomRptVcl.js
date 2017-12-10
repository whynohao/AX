comTestStructBomRptVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
    this.tree;
}
var proto = comTestStructBomRptVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comTestStructBomRptVcl;
proto.doSetParam = function (data) {
    if (data.length > 0) {
        this.fillData.call(this, data);
        var obj = this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0);
        this.tree.setRootNode(obj);
    }
   
}


proto.fillData = function (data) {
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set("GUID", "123");
    this.dataSet.dataMap[0].add("123", masterRow);
    for (var i = 0 ; i < data.length; i++) {
        var newRow = this.addRow(masterRow, 1);
        newRow.set("GUID", "123");
        newRow.set("ROW_ID", i + 1);
        newRow.set("ROWID", data[i].RowId);
        newRow.set("PARENTROWID", data[i].ParentrowId);
        newRow.set("NODENAME", data[i].NodeName);
        newRow.set("MATERIALID", data[i].SubMaterialId);
        newRow.set("MATERIALNAME", data[i].SubMaterialName);
        newRow.set("ATTRIBUTECODE", data[i].AttributeCode);
        newRow.set("ATTRIBUTEDESC", data[i].AttributeDesc);
        newRow.set("BASEQTY", data[i].Baseqty);
        newRow.set("UNITQTY", data[i].Unitqty);
        newRow.set("EXCEPTION", data[i].Exception);
    }
}
//填充数据
proto.GetTreeStoreData = function (gen, fillStore, rowId) {
    var obj;
    if (gen) {
        var isLeaf = false;
        var items = this.GetTreeStoreData(false, fillStore, rowId);
        if (items.length == 0)
            isLeaf = true;
        obj = {
            ROWID: 0,
            PARENTROWID: 0,
            NODENAME:"",
            MATERIALID: "",
            MATERIALNAME: "",
            ATTRIBUTECODE: "",
            ATTRIBUTEDESC: "",
            BASEQTY: "",
            UNITQTY:"",
            EXCEPTION:"",
            expanded: true,//展开
            leaf: false,
            disabled: true,
            children: items,
        }
    } else {
        obj = new Array();
        var models = this.GetModelCollection(fillStore, rowId);
        for (var i = 0; i < models.length; i++) {
            var isLeaf = false;//是否有下一层级
            var isexpanded = false; //是否展开层级
            var record = models[i];
            var items = this.GetTreeStoreData.call(this, false, fillStore, record.data['ROWID']);
            obj.push({
                ROWID: record.data['ROWID'],
                PARENTROWID: record.data['PARENTROWID'],
                NODENAME:record.data['NODENAME'],
                MATERIALID: record.data['MATERIALID'],
                MATERIALNAME: record.data['MATERIALNAME'],
                ATTRIBUTECODE: record.data['ATTRIBUTECODE'],
                ATTRIBUTEDESC: record.data['ATTRIBUTEDESC'],
                BASEQTY: record.data['BASEQTY'],
                UNITQTY: record.data['UNITQTY'],
                EXCEPTION: record.data['EXCEPTION'],
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
//打印数据
proto.print = function (headTable, bodyTable) {
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
        var SubMaterialId = bodyItems.get("MATERIALID");
        var SubMaterialName = bodyItems.get("MATERIALNAME");
        var SubmaterialSpec = bodyItems.get("MATERIALSPEC");
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