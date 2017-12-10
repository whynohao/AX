comStructTechRouteVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);

    //第一次打开
    this.firstLoad = true;

    //树
    this.tree;

    this.copyData = {};
};

var proto = comStructTechRouteVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comStructTechRouteVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.dataRow.data["ROW_ID"] == 0) {
                    e.dataInfo.cancel = true;
                    Ext.Msg.alert("提示", "根目录不能修改！");
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                var bindingRow = e.dataInfo.dataRow.bindingRow || getBindingRow.call(this, e);
                if (bindingRow) {
                    bindingRow.set(e.dataInfo.fieldName, e.dataInfo.value);
                    switch (e.dataInfo.fieldName) {
                        case "WORKPROCESSID":
                            e.dataInfo.dataRow.set("NODENAME", e.dataInfo.dataRow.data["WORKPROCESSNAME"]);
                            for (var fieldName in bindingRow.data) {
                                if (fieldName != "id")
                                    bindingRow.set(fieldName, e.dataInfo.dataRow.get(fieldName));
                            }
                            if (e.dataInfo.value == "") {
                                e.dataInfo.dataRow.set("ATTRIBUTEID", "");
                            }
                            break;
                    }
                }
            }
            break;
        case LibEventTypeEnum.ColumnDbClick:
            var temp;
            if (e.dataInfo.dataRow.data["MATCHTEXTBINARY"] != "") {
                var source = e.dataInfo.dataRow.data["MATCHTEXTBINARY"];
                temp = source.split(';');
            }
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "MATCHRULE") {
                    var dataRow = this.dataSet.getTable(0).data.items[0];
                    var attributeId = dataRow.get("ATTRIBUTEID");
                    var attrItems = this.invorkBcf('GetAttrDetail', [attributeId]);
                    var rowItemTable = [];
                    rowItemTable.push(dataRow.get("STRUCTTECHROUTENAME"));
                    rowItemTable.push(0);
                    rowItemTable.push(attrItems);
                    var dataRowItem = [];
                    dataRowItem.push(rowItemTable);
                    ChooseBomMatchItemValueForm(this, dataRowItem[0], e.dataInfo.dataRow, e, temp);
                }
            }
            break;
        case LibEventTypeEnum.FormClosed:
            break;
    }
}

//ISATTRDETAIL/将数据源转为树形的数据对象
proto.GetTreeStoreData1 = function (gen, fillStore, rowId, bomLevel) {//bomLevel 层级
    var obj = [];
    if (gen) {
        var items = this.GetTreeStoreData1(false, fillStore, rowId, 1);

        var charString = {};
        var record = this.dataSet.getTable(1).data.items[1];
        if (record != undefined) {
            for (var key in record.data) {
                if (key != "id")
                    charString[key] = "";
            }

            charString["expanded"] = true;//是否展开层级
            charString["children"] = items;//是否有下一层
            obj.push(charString);
        }
    }
    else {
        var models = GetModelCollection(fillStore, rowId);
        for (var i = 0; i < models.length; i++) {
            var record = models[i];
            var items = this.GetTreeStoreData1.call(this, false, fillStore, record.data['ROW_ID'], bomLevel + 1);
            record.data['ORDERNUM'] == 0 && (record.data['ORDERNUM'] = i + 1);
            record.data['BOMLEVEL'] == 0 && (record.data['BOMLEVEL'] = bomLevel);

            var charString = {};
            if (record != undefined) {
                for (var key in record.data) {
                    if (key != "id")
                        charString[key] = record.data[key];
                }
                charString["expanded"] = false;//是否展开层级
                charString["children"] = items;//是否有下一层
                obj.push(charString);
            }
        }
    }
    return obj;
}

//删除
proto.GetModelAndSonCollection = function (fillStore, rowid, modelCollection) {
    var items = fillStore.data.items;
    for (var i = 0; i < fillStore.data.length; i++) {
        if (items[i].data['PARENTROWID'] == rowid) {
            modelCollection.push(items[i]);
            this.GetModelAndSonCollection(fillStore, items[i].get("ROW_ID"), modelCollection);
        }
    }
}

//撤销
proto.cancel = function () {
    this.dataSet.rejectChanges();
    var masterRow = this.dataSet.getTable(0).data.items[0];
    for (var i = 0; i < this.forms.length; i++) {
        this.forms[i].loadRecord(masterRow);
    };
    this.tree.setRootNode(this.GetTreeStoreData1(true, this.dataSet.getTable(1), 0, 0)[0]);
};

//保存方案
proto.saveDisplayScheme = function () {
    var needSave = false;
    var displayScheme = { ProgId: this.progId, GridScheme: {} };
    for (var i = 0; i < this.dataSet.dataList.length; i++) {
        if (i == 1) {
            if (!needSave)
                needSave = true;
            var gridScheme = { GridFields: [] };
            var columns = this.tree.headerCt.items.items;
            if (columns.length == 0) {
                gridScheme = this.subGridScheme[i];
            } else {
                var buildBandCol = function (bandColumn, list) {
                    list.push({ Field: { Name: bandColumn.dataIndex, Width: bandColumn.width } });
                }
                for (var l = 0; l < columns.length; l++) {
                    //if (l == 0) {
                    //    columns[0].dataIndex = "ORDERNUM";
                    //    buildBandCol(columns[0], gridScheme.GridFields);
                    //}
                    //else {
                    if (columns.xtype == "rownumberer" || columns[l].hidden === true)
                        continue;
                    buildBandCol(columns[l], gridScheme.GridFields);
                    //}
                }
            }
            if (gridScheme != undefined)
                displayScheme.GridScheme[i] = gridScheme;
        }
        else {
            if (this.dataSet.dataList[i].ownGrid) {
                if (!needSave)
                    needSave = true;
                var gridScheme = { GridFields: [] };
                var gridPanel = this.dataSet.dataList[i].ownGrid;
                var columns = gridPanel.headerCt.items.items;
                if (columns.length == 0) {
                    gridScheme = this.subGridScheme[i];
                } else {
                    var buildBandCol = function (bandColumn, list) {
                        if (bandColumn.items.items.length > 0) {
                            var subList = [];
                            list.push({ Header: bandColumn.text, BandFields: subList });
                            for (var r = 0; r < bandColumn.items.items.length; r++) {
                                if (bandColumn.items.items[r].hidden === true)
                                    continue;
                                buildBandCol(bandColumn.items.items[r], subList);
                            }
                        }
                        else {
                            list.push({ Field: { Name: bandColumn.dataIndex, Width: bandColumn.width } });
                        }
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
        }
    }
    if (needSave) {
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
}

//刷新，多用户的时候，其中一个用户更改了数据，其他用户需要点击刷新才能刷新树
proto.browseTo = function (condition) {
    var data = this.invorkBcf("BrowseTo", [condition]);
    this.setDataSet(data, false);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    for (var i = 0; i < this.forms.length; i++) {
        this.forms[i].loadRecord(masterRow);
    };
    if (!this.firstLoad)
        this.tree.setRootNode(this.GetTreeStoreData1(true, this.dataSet.getTable(1), 0, 0)[0]);

};

//导入
proto.importData = function (fileName) {
    var assistObj = {};
    var data = this.invorkBcf('ImportData', [fileName, this.entryParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success) {
        this.restData(false, BillActionEnum.Browse, data);
        this.tree.setRootNode(this.GetTreeStoreData1(true, this.dataSet.getTable(1), 0, 0)[0]);
    }
    return success;
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
    var childrenRows = GetModelCollection(this.dataSet.getTable(1), dataRow.get("ROW_ID"));
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
    }
    else {
        currNode = parentNode;
        copyItem = currCopyItem;
        bindingRow = currNode.bindingRow || this.dataSet.FindRow(1, currNode.get("ROW_ID"));
        newRow.set("PARENTROWID", bindingRow.get("ROW_ID"));
        newRow.set("BOMLEVEL", bindingRow.get("BOMLEVEL") + 1);
    }
    for (var fieldName in copyItem.dataRow.data) {
        if (fieldName != "STRUCTBOMID" && fieldName != "PARENTROWID" && fieldName != "ROW_ID" && fieldName != "BOMLEVEL" && fieldName != "id") {
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

var getBindingRow = function (e) {
    var items = this.dataSet.dataList[1].data.items;
    var br;
    for (var i = 0; i < items.length; i++) {
        if (e.dataInfo.dataRow.data["ROW_ID"] == items[i].data["ROW_ID"]) {
            br = items[i];
            break;
        }
    }
    return br;
}

//取行数据
var GetModelCollection = function (fillStore, rowid) {
    var items = fillStore.data.items;
    var mc = [];
    for (var i = 0; i < fillStore.data.length; i++) {
        if (items[i].data['PARENTROWID'] == rowid) {
            mc.push(items[i]);
        }
    }
    return mc;
}


Ext.define('ChooseBomMatch', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MATERIALTYPENAME', type: 'string' },
        { name: 'ATTRIBUTEITEMID', type: 'string' },
        { name: 'ATTRIBUTEITEMNAME', type: 'string' }
    ]
});
//(点击物料选配规则时)加载特征
var ChooseBomMatchItemValueForm = function (me, attrItemTable, attrDetailRow, ex, json) {
    var tableData = [];
    var dic = new Array();
    if (json != undefined && json.length > 1) {
        dic = eval(json[1]);
    }
    if (dic.length > 0) {
        for (var i = 0; i < attrItemTable[2].length; i++) {
            var curdata = attrItemTable[2][i];
            if (dic[0].ATTRIBUTEITEMID == curdata["ATTRIBUTEITEMID"]) {
                tableData.push([attrItemTable[0], curdata["ATTRIBUTEITEMID"], curdata["ATTRIBUTEITEMNAME"]]);
            }
        }
    }
    else {
        for (var i = 0; i < attrItemTable[2].length; i++) {
            var curdata = attrItemTable[2][i];
            tableData.push([attrItemTable[0], curdata["ATTRIBUTEITEMID"], curdata["ATTRIBUTEITEMNAME"]]);
        }
    }
    var result;
    var myStore = Ext.create('Ext.data.Store', {
        model: 'ChooseBomMatch',
        data: tableData
    });
    if (!win) {
        var win = Ext.create("Ext.Window", {
            title: "选配特征项选择",
            width: 350,
            height: 400,
            layout: "fit",
            modal: true,
            closeAction: 'hide',
            items: {
                xtype: "grid",
                store: myStore,
                columns: [
                    { text: "上级物料", dataIndex: "MATERIALTYPENAME" },
                    { text: "特征项代码", dataIndex: "ATTRIBUTEITEMID" },
                    { text: "特征项名称", dataIndex: "ATTRIBUTEITEMNAME" }
                ],
                listeners: {
                    itemdblclick: function (dataview, record, item, index, e) {
                        result = e.record.data;
                        AttrItemDetail(me, result, attrDetailRow, ex, dic, attrItemTable[1]);
                        win.close();
                    }
                }
            },
            buttons: [
                {
                    text: '清空',
                    listeners: {
                        'click': function (dataview, record, item, index, e) {
                            if (me.isEdit) {
                                attrDetailRow.set("MATCHRULE", false);
                                attrDetailRow.set("MATCHTEXTBINARY", "");
                                var bindingRow = ex.dataInfo.dataRow.bindingRow || getBindingRow.call(me, ex);
                                if (bindingRow) {
                                    bindingRow.set("MATCHRULE", false);
                                    bindingRow.set("MATCHTEXTBINARY", "");
                                }
                                win.close();
                            } else {
                                Ext.Msg.alert("提示", "修改状态下才能清空");
                            }

                        }
                    }
                },
            ]

        });
    }
    win.show();
}

Ext.define('AttrItemDetail', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'ATTRCODE', type: 'string' },
        { name: 'ATTRVALUE', type: 'string' },
        {
            name: 'ROWID', type: 'int'
        }
    ]
});
//加载特征项
var AttrItemDetail = function (me, attrItemTable, attrDetailRow, e, dic, row) {
    var tableData = [];
    var curdata = me.invorkBcf('GetAttrItemDetail', [attrItemTable.ATTRIBUTEITEMID]);
    if (curdata.length == 0) {
        Ax.utils.LibVclSystemUtils.openDataFunc("com.AttributeMatch", "特征匹配", [me, attrItemTable, attrDetailRow, dic]);
    }
    else {
        for (var i = 0; i < curdata.length; i++) {
            var hasRowId = false;
            for (var j = 0; j < dic.length; j++) {
                if (curdata[i].Attrcode == dic[j].ATTRCODE) {
                    tableData.push([curdata[i].Attrcode, curdata[i].Attrvalue, dic[j].ROWID]);
                    hasRowId = true;
                    break;
                }
            }
            if (!hasRowId) {
                tableData.push([curdata[i].Attrcode, curdata[i].Attrvalue]);
            }
        }
        var cellEditing = Ext.create('Ext.grid.plugin.CellEditing', {
            clicksToEdit: 1
        });
        var result;
        var myStore = Ext.create('Ext.data.Store', {
            model: 'AttrItemDetail',
            data: tableData
        });
        if (!win) {
            var win = Ext.create("Ext.Window", {
                title: "特征项明细选择",
                width: 350,
                height: 400,
                layout: "fit",
                modal: true,
                closeAction: 'hide',
                items: {
                    xtype: "grid",
                    store: myStore,
                    plugins: [cellEditing
                    ],
                    columns: [
                        { text: "特征编码", dataIndex: "ATTRCODE" },
                        { text: "特征值", dataIndex: "ATTRVALUE" },
                        { text: "选配行", dataIndex: "ROWID", editor: me.isEdit }
                    ]
                },
                buttons: [
                    {
                        text: '确定',
                        listeners: {
                            'click': function (dataview, record, item) {
                                var structure = "";
                                var store = this.up("window").down("gridpanel").store;
                                var Conditions = new Array();
                                for (var i = 0; i < store.data.length; i++) {
                                    var rows = store.data.items[i];
                                    if (rows.data.ROWID != 0) {
                                        Conditions.push({
                                            ATTRIBUTEITEMID: attrItemTable.ATTRIBUTEITEMID,
                                            ATTRCODE: rows.data.ATTRCODE,
                                            ROWID: rows.data.ROWID,
                                        });
                                        structure = structure + "if(A == '" + rows.data.ATTRCODE + "') ret = " + rows.data.ROWID + " else ";
                                        //structure = structure + "if(A == '/:" + rows.data.ATTRCODE + "/') ret = /:" + rows.data.ROWID + "/ else ";
                                    }
                                }
                                var info = JSON.stringify(Conditions);
                                structure = structure + "ret = -1 | A = " + attrItemTable.ATTRIBUTEITEMID + ";" + info;
                                //structure = structure + "ret = -1 | A = /:" + attrItemTable.ATTRIBUTEITEMID;
                                attrDetailRow.set("MATCHRULE", true);
                                attrDetailRow.set("MATCHTEXTBINARY", structure);
                                var bindingRow = e.dataInfo.dataRow.bindingRow || getBindingRow.call(me, e);
                                if (bindingRow) {
                                    bindingRow.set("MATCHRULE", true);
                                    bindingRow.set("MATCHTEXTBINARY", structure);
                                }
                                win.hide();
                            }
                        }
                    },
                ]
            });
        }
        win.show();
    }
}

var getBindingRow = function (e) {
    var items = this.dataSet.dataList[1].data.items;
    var br;
    for (var i = 0; i < items.length; i++) {
        if (e.dataInfo.dataRow.data["ROW_ID"] == items[i].data["ROW_ID"]) {
            br = items[i];
            break;
        }
    }
    return br;
}
