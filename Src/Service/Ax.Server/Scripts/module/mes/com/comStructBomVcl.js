comStructBomVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);

    //第一次打开
    this.firstLoad = true;

    //树
    this.tree;

    this.copyData = {};
};
var proto = comStructBomVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comStructBomVcl;

proto.doSetParam = function () {

}
proto.getBindingRow = function (e) {
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
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            var text = {
                //2: "特征明细过账表不能手动添加！",
                3: "无选配特征变量明细表不能手动添加！",
                4: "结构BOM工艺路线选配公式变量明细表不能手动添加！"
            }[e.dataInfo.tableIndex] || false;
            if (text) {
                e.dataInfo.cancel = true;
                Ext.Msg.alert("提示", text);
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            var text = {
                //2: "特征明细过账表不能手动删除！",
                3: "无选配特征变量明细表不能手动删除！",
                4: "结构BOM工艺路线选配公式变量明细表不能手动删除！"
            }[e.dataInfo.tableIndex] || false;
            if (text) {
                e.dataInfo.cancel = true;
                Ext.Msg.alert("提示", text);
            }
            break;
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
                var bindingRow = e.dataInfo.dataRow.bindingRow || this.getBindingRow.call(this, e);
                if (bindingRow) {
                    bindingRow.set(e.dataInfo.fieldName, e.dataInfo.value);
                    switch (e.dataInfo.fieldName) {
                        case "SUBMATERIALID":
                            e.dataInfo.dataRow.set("NODENAME", e.dataInfo.dataRow.data["SUBMATERIALNAME"]);
                            for (var item in e.dataInfo.dataRow.data) {
                                if (bindingRow.data.hasOwnProperty(item)) {
                                    bindingRow.set(item, e.dataInfo.dataRow.get(item));
                                }
                            }

                            if (e.dataInfo.dataRow.data['ISATTRDETAIL']) {
                                var pid = e.dataInfo.dataRow.data["ROW_ID"];
                                var store = this.dataSet.getTable(6);
                                store.clearFilter();
                                var items = store.data.items;
                                for (var i = items.length - 1; i >= 0; i--)
                                    items[i].get("GRANDPARENTROWID") == pid && store.data.removeAt(i);
                                store = this.dataSet.getTable(3);
                                store.clearFilter();
                                items = store.data.items;
                                for (var i = items.length - 1; i >= 0; i--) {
                                    if (items[i].get("GRANDPARENTROWID") == pid) {
                                        this.deleteRow(3, items[i]);
                                    }
                                }
                                store = this.dataSet.getTable(2);
                                store.clearFilter();
                                items = store.data.items;
                                for (var i = items.length - 1; i >= 0; i--) {
                                    if (items[i].get("PARENTROWID") == pid) {
                                        this.deleteRow(2, items[i]);
                                    }
                                }
                            }
                            e.dataInfo.dataRow.set("ISATTRDETAIL", false);
                            bindingRow.set("ISATTRDETAIL", false);
                            if (e.dataInfo.value == "") {
                                e.dataInfo.dataRow.set("ATTRIBUTEID", "");
                            }
                            //新增特征明细
                            var attrDetail = this.invorkBcf('GetAttrDetail', [e.dataInfo.dataRow.data["ATTRIBUTEID"]]);
                            var i = 1;
                            for (var attrItem in attrDetail) {
                                var newRow = this.addRow(bindingRow, 2);
                                newRow.set('ATTRIBUTEITEMID', attrItem);
                                newRow.set('ATTRIBUTEITEMNAME', attrDetail[attrItem][0]);
                                newRow.set('ATTRIBUTECODELEN', attrDetail[attrItem][1]);
                                newRow.set('VALUECALTYPE', attrDetail[attrItem][2]);
                                i++;
                            }
                            e.dataInfo.dataRow.set("ISATTRDETAIL", i > 1);
                            bindingRow.set("ISATTRDETAIL", i > 1);

                            break;
                        case 'MATERIALTYPEID':
                            if (e.dataInfo.dataRow.data['ISATTRDETAIL']) {
                                var pid = e.dataInfo.dataRow.data["ROW_ID"];
                                var store = this.dataSet.getTable(6);
                                store.clearFilter();
                                var items = store.data.items;
                                for (var i = store.data.length - 1; i >= 0; i--)
                                    items[i].get("GRANDPARENTROWID") == pid && store.data.removeAt(i);
                                store = this.dataSet.getTable(3);
                                store.clearFilter();
                                items = store.data.items;
                                for (var i = items.length - 1; i >= 0; i--) {
                                    if (items[i].get("GRANDPARENTROWID") == pid) {
                                        this.deleteRow(3, items[i]);
                                    }
                                }
                                store = this.dataSet.getTable(2);
                                store.clearFilter();
                                items = store.data.items;
                                for (var i = items.length - 1; i >= 0; i--) {
                                    if (items[i].get("PARENTROWID") == pid) {
                                        this.deleteRow(2, items[i]);
                                    }
                                }
                            }
                            e.dataInfo.dataRow.set("ISATTRDETAIL", false);
                            bindingRow.set("ISATTRDETAIL", false);
                            if (e.dataInfo.value == "") {
                                e.dataInfo.dataRow.set("ATTRIBUTEID", "");
                            }
                            bindingRow.set("ATTRIBUTEID", e.dataInfo.dataRow.data["ATTRIBUTEID"]);
                            //新增特征明细
                            var attrDetail = this.invorkBcf('GetAttrDetail', [e.dataInfo.dataRow.data["ATTRIBUTEID"]]);
                            var i = 1;
                            for (var attrItem in attrDetail) {
                                var newRow = this.addRow(bindingRow, 2);
                                newRow.set('ATTRIBUTEITEMID', attrItem);
                                newRow.set('ATTRIBUTEITEMNAME', attrDetail[attrItem][0]);
                                newRow.set('ATTRIBUTECODELEN', attrDetail[attrItem][1]);
                                newRow.set('VALUECALTYPE', attrDetail[attrItem][2]);
                                i++;
                            }
                            e.dataInfo.dataRow.set("ISATTRDETAIL", i > 1);
                            bindingRow.set("ISATTRDETAIL", i > 1);
                            break;
                        case "TECHROUTEROWID":
                            bindingRow.set("WORKPROCESSID", e.dataInfo.dataRow.get("WORKPROCESSID"));
                            bindingRow.set("WORKSHOPSECTIONID", e.dataInfo.dataRow.get("WORKSHOPSECTIONID"));
                            break;
                    }
                }
            }
            else if (e.dataInfo.tableIndex == 2) {
                if (e.dataInfo.fieldName == "EXPRESSIONID") {
                    var store = this.dataSet.getTable(3);
                    store.clearFilter();
                    var items = store.data.items; b
                    var gpid = e.dataInfo.dataRow.data["PARENTROWID"];
                    var pid = e.dataInfo.dataRow.get("ROW_ID");
                    for (var i = items.length - 1; i >= 0; i--) {
                        if (items[i].get("GRANDPARENTROWID") == gpid && items[i].get("PARENTROWID") == pid) {
                            this.deleteRow(3, items[i]);
                        }
                    }
                    var i = 1;
                    var paramDetailDic = this.invorkBcf('GetParamDetail', [e.dataInfo.value]);
                    for (var paramItem in paramDetailDic) {
                        newRow = this.addRow(e.dataInfo.dataRow, 3);
                        newRow.set('PARAMID', paramItem);
                        newRow.set('PARAMNAME', paramDetailDic[paramItem]);
                        i++;
                    }
                    e.dataInfo.dataRow.set("ISPARAMDETAIL", i > 1);
                }
            }
            break;
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.tableIndex == 1) {
                var bindingRow = e.dataInfo.dataRow.bindingRow || this.getBindingRow.call(this, e);
                switch (e.dataInfo.fieldName) {
                    case "ISATTRDETAIL"://特征明细
                        if (this.isEdit) {
                            var attrDetail = this.invorkBcf('GetAttrDetail', [e.dataInfo.dataRow.data["ATTRIBUTEID"]]);
                            var attrDetailOld = this.dataSet.getChildren(1, bindingRow, 2);
                            for (var attrItem in attrDetail) {
                                var hasRowId = false;
                                for (var i = 0; i < attrDetailOld.length; i++) {
                                    if (attrItem == attrDetailOld[i].data["ATTRIBUTEITEMID"]) {
                                        hasRowId = true;
                                        break;
                                    }
                                }
                                if (!hasRowId) {
                                    var newRow = this.addRow(bindingRow, 2);
                                    newRow.set('ATTRIBUTEITEMID', attrItem);
                                    newRow.set('ATTRIBUTEITEMNAME', attrDetail[attrItem][0]);
                                    newRow.set('ATTRIBUTECODELEN', attrDetail[attrItem][1]);
                                    newRow.set('VALUECALTYPE', attrDetail[attrItem][2]);
                                }
                            }
                        }
                        break;
                    case "MATCHRULE"://物料选配变量明细表
                        var temp;
                        if (e.dataInfo.dataRow.data["MATCHTEXTBINARY"] != "") {
                            var source = e.dataInfo.dataRow.data["MATCHTEXTBINARY"];
                            temp = source.split(';');
                        }
                        var dataRowItem = [];
                        var curBomDetailRow = this.dataSet.FindRow(1, e.dataInfo.dataRow.data["ROW_ID"]);
                        var getParentRow = function (me, curBomDetailRow, dataRowItem, row) {
                            if (curBomDetailRow) {
                                var rowItemTable = [];
                                if (curBomDetailRow.get("PARENTROWID") != 0) {
                                    var attrItemTable = [];
                                    var store = me.dataSet.getTable(2);
                                    store.clearFilter();
                                    var childItems = store.data.items;
                                    for (var i = 0; i < childItems.length; i++) {
                                        if (childItems[i].get("PARENTROWID") == curBomDetailRow.get("PARENTROWID")) {
                                            attrItemTable.push(childItems[i].data);
                                        }
                                    }
                                    var curPks = me.dataSet.getTable(2).Pks;
                                    var parentPks = me.dataSet.getTable(3).Pks;
                                    var filter = function (record) {
                                        var ret = true;
                                        for (var i = 0; i < curPks.length - 1; i++) {
                                            if (record.get(curPks[i]) != e.dataInfo.dataRow.get(parentPks[i]))
                                                ret = false;
                                        }
                                        return ret;
                                    }
                                    store.filterBy(function (record) {
                                        return filter(record);
                                    });
                                    curBomDetailRow = me.dataSet.FindRow(1, curBomDetailRow.data["PARENTROWID"]);
                                    if (attrItemTable.length == 0) {
                                        getParentRow(me, curBomDetailRow, dataRowItem, 1);
                                    }
                                    else {
                                        rowItemTable.push(curBomDetailRow.get("NODENAME"));
                                        rowItemTable.push(curBomDetailRow.get("ROW_ID"));
                                        rowItemTable.push(attrItemTable);
                                        dataRowItem.push(rowItemTable);
                                        getParentRow(me, curBomDetailRow, dataRowItem);
                                    }
                                }
                                else {
                                    var dataRow = me.dataSet.getTable(0).data.items[0];
                                    var attributeId = dataRow.get("ATTRIBUTEID");
                                    var attrItems = me.invorkBcf('GetAttributeDetail', [attributeId]);
                                    rowItemTable.push(dataRow.get("STRUCTBOMNAME"));
                                    rowItemTable.push(0);
                                    rowItemTable.push(attrItems);
                                    dataRowItem.push(rowItemTable);
                                    me.ChooseBomRowAttribute(me, dataRowItem, e.dataInfo.dataRow, e, temp);
                                }
                            }
                        }
                        getParentRow(this, curBomDetailRow, dataRowItem, 0);

                    case "ISTECHROUTEMATDETAIL"://工艺路线选配明细表
                        this.dataRow = e.dataInfo.dataRow;
                        break;
                    case "ISEXPRESSIONDPARAMDETAIL"://工艺选配公式变量明细
                        this.dataRow = e.dataInfo.dataRow;
                        if (e.dataInfo.value || !e.dataInfo.dataRow.data["TECHEXPRESSIONID"])
                            break;
                        var pid = e.dataInfo.dataRow.data["ROW_ID"];
                        var masterRow = this.dataSet.getTable(0).data.items[0];
                        var structBomId = masterRow.get("STRUCTBOMID");
                        var store = this.dataSet.getTable(4);
                        var i = 1;
                        var paramDetailDic = this.invorkBcf('GetParamDetail', [e.dataInfo.dataRow.data["TECHEXPRESSIONID"]]);
                        for (var paramItem in paramDetailDic) {
                            newRow = this.addRow(this.dataRow, 4);
                            newRow.set('PARAMID', paramItem);
                            newRow.set('PARAMNAME', paramDetailDic[paramItem]);
                            i++;
                        }
                        e.dataInfo.dataRow.set("ISEXPRESSIONDPARAMDETAIL", i > 1);
                        bindingRow.set("ISEXPRESSIONDPARAMDETAIL", i > 1);
                        break;
                }
            }
            else if (e.dataInfo.tableIndex == 2) {
                if (e.dataInfo.fieldName == "ISPARAMDETAIL") {
                    this.dataRow = e.dataInfo.dataRow;
                }
            }
            else if (e.dataInfo.tableIndex == 3) {
                if (e.dataInfo.fieldName == "PARAMVALUE") {
                    var dataRowItem = [];
                    var curBomDetailRow = this.dataSet.FindRow(1, e.dataInfo.dataRow.data["GRANDPARENTROWID"]);
                    var getParentRow = function (me, curBomDetailRow, dataRowItem, row) {
                        if (curBomDetailRow) {
                            var rowItemTable = [];
                            if (curBomDetailRow.get("PARENTROWID") != 0) {
                                var attrItemTable = [];
                                var store = me.dataSet.getTable(2);
                                store.clearFilter();
                                var childItems = store.data.items;
                                for (var i = 0; i < childItems.length; i++) {
                                    if (childItems[i].get("PARENTROWID") == curBomDetailRow.get("PARENTROWID")) {
                                        attrItemTable.push(childItems[i].data);
                                    }
                                }
                                var curPks = me.dataSet.getTable(2).Pks;
                                var parentPks = me.dataSet.getTable(3).Pks;
                                var filter = function (record) {
                                    var ret = true;
                                    for (var i = 0; i < curPks.length - 1; i++) {
                                        if (record.get(curPks[i]) != e.dataInfo.dataRow.get(parentPks[i]))
                                            ret = false;
                                    }
                                    return ret;
                                }
                                store.filterBy(function (record) {
                                    return filter(record);
                                });
                                curBomDetailRow = me.dataSet.FindRow(1, curBomDetailRow.data["PARENTROWID"]);
                                if (attrItemTable.length == 0) {
                                    getParentRow(me, curBomDetailRow, dataRowItem, 1);
                                }
                                else {
                                    rowItemTable.push(curBomDetailRow.get("NODENAME"));
                                    rowItemTable.push(curBomDetailRow.get("ROW_ID"));
                                    rowItemTable.push(attrItemTable);
                                    dataRowItem.push(rowItemTable);
                                    getParentRow(me, curBomDetailRow, dataRowItem);
                                }
                            }
                            else {
                                var dataRow = me.dataSet.getTable(0).data.items[0];
                                var attributeId = dataRow.get("ATTRIBUTEID");
                                var attrItems = me.invorkBcf('GetAttributeDetail', [attributeId]);
                                rowItemTable.push(dataRow.get("STRUCTBOMNAME"));
                                rowItemTable.push(0);
                                rowItemTable.push(attrItems);
                                dataRowItem.push(rowItemTable);
                                me.ParentRowAttribute(me, dataRowItem, e.dataInfo.dataRow);
                            }
                        }
                    }
                    getParentRow(this, curBomDetailRow, dataRowItem, 0);
                }
            }
            break;
        case LibEventTypeEnum.FormClosed:
            var structBomId = this.dataSet.getTable(0).data.items[0].get("STRUCTBOMID");
            if (e.dataInfo.tableIndex == 3) {
                var gpid = this.dataRow.get("PARENTROWID");
                var pid = this.dataRow.get("ROW_ID");
                var store = this.dataSet.getTable(6);
                var items = store.data.items;
                for (var i = items.length - 1; i >= 0; i--) {
                    if (items[i].get("STRUCTBOMID") == structBomId && items[i].get("GRANDPARENTROWID") == gpid && items[i].get("PARENTROWID") == pid) {
                        store.remove(items[i]);
                    }
                }
                items = this.dataSet.getTable(3).data.items;
                for (var i = 0; i < items.length; i++) {
                    if (items[i].get("STRUCTBOMID") == structBomId && items[i].get("GRANDPARENTROWID") == gpid && items[i].get("PARENTROWID") == pid) {
                        var newRow = Ext.decode(this.tpl.Tables[this.dataSet.getTable(6).Name].NewRowObj);
                        newRow["STRUCTBOMID"] = structBomId;
                        newRow["GRANDPARENTROWID"] = gpid;
                        newRow["PARENTROWID"] = pid;
                        newRow["ROW_ID"] = items[i].get("ROW_ID");
                        newRow["PARAMID"] = items[i].get("PARAMID");
                        newRow["PARAMNAME"] = items[i].get("PARAMNAME");
                        newRow["PARAMVALUE"] = items[i].get("PARAMVALUE");
                        newRow["PARAMVALUENAME"] = items[i].get("PARAMVALUENAME");
                        var newModel = store.add(newRow)[0];
                    }
                }
            }
            else if (e.dataInfo.tableIndex == 4) {
                var gpid = this.dataRow.get("ROW_ID");
                var pid = 0;
                var store = this.dataSet.getTable(6);
                var items = store.data.items;
                for (var i = items.length - 1; i >= 0; i--) {
                    if (items[i].get("STRUCTBOMID") == structBomId && items[i].get("GRANDPARENTROWID") == gpid && items[i].get("PARENTROWID") == pid && items[i].get("FROMTABLE") == 4) {
                        store.remove(items[i]);
                    }
                }
                items = this.dataSet.getTable(4).data.items;
                for (var i = 0; i < items.length; i++) {
                    if (items[i].get("STRUCTBOMID") == structBomId && items[i].get("PARENTROWID") == gpid) {
                        var newRow = Ext.decode(this.tpl.Tables[this.dataSet.getTable(6).Name].NewRowObj);
                        newRow["STRUCTBOMID"] = structBomId;
                        newRow["GRANDPARENTROWID"] = gpid;
                        newRow["PARENTROWID"] = pid;
                        newRow["FROMTABLE"] = 4;
                        newRow["ROW_ID"] = items[i].get("ROW_ID");
                        newRow["PARAMID"] = items[i].get("PARAMID");
                        newRow["PARAMNAME"] = items[i].get("PARAMNAME");
                        newRow["PARAMVALUE"] = items[i].get("PARAMVALUE");
                        newRow["PARAMVALUENAME"] = items[i].get("PARAMVALUENAME");
                        var newModel = store.add(newRow)[0];
                    }
                }
            }
            else if (e.dataInfo.tableIndex == 5) {
                var b = false;
                var pid = this.dataRow.get("ROW_ID");
                var items = this.dataSet.getTable(5).data.items;
                for (var i = 0; i < items.length; i++) {
                    if (items[i].get("STRUCTBOMID") == structBomId && items[i].get("PARENTROWID") == pid) {
                        b = true;
                        break;
                    }
                }
                if (this.dataRow.get("ISTECHROUTEMATDETAIL") != b)
                    this.dataRow.set("ISTECHROUTEMATDETAIL", b);
                var items = this.dataSet.getTable(1).data.items;
                for (var i = 0; i < items.length; i++) {
                    if (pid == items[i].get("ROW_ID")) {
                        items[i].set("ISTECHROUTEMATDETAIL", b);
                        break;
                    }
                }
            }
            break;
    }
}

//（点击特征项变量明细中特征值时）呼出行号选择框
Ext.define('ParentRowAttribute', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'NODENAME', type: 'string' },
        { name: 'ROW_ID', type: 'int' },
    ]
});
proto.ParentRowAttribute = function (me, dataRowItem, attrDetailRow) {
    var tableData = [];
    for (var i = 0; i < dataRowItem.length; i++) {
        var curdata = dataRowItem[i];
        tableData.push([curdata[0], curdata[1]]);
    }
    var result;
    var myStore = Ext.create('Ext.data.Store', {
        model: 'ParentRowAttribute',
        data: tableData
    });
    if (!win) {
        var win = Ext.create("Ext.Window", {
            title: "当前行对应父级",
            width: 200,
            height: 400,
            layout: "fit",
            modal: true,
            closeAction: 'hide',
            items: {
                xtype: "grid",
                store: myStore,
                columns: [
                    { text: "节点名称", dataIndex: "NODENAME" },
                    { text: "行标识", dataIndex: "ROW_ID" }
                ],
                listeners: {
                    itemdblclick: function (dataview, record, item, index, e) {
                        var attrItemTable;
                        var row;//如果row等于0 则说明获取的特征是当前行的父级
                        for (var i = 0; i < dataRowItem.length; i++) {
                            if (dataRowItem[i][1] == record.get("ROW_ID")) {
                                attrItemTable = dataRowItem[i];
                                row = i;
                            }
                        }
                        if (row != 0) {
                            Ext.Msg.confirm('提示', '非父层级是否编辑?', function (button) {
                                if (button == "yes") {
                                    me.ChooseBomAttributeItemValueForm(me, attrItemTable, attrDetailRow, row)
                                    win.hide();
                                }
                            }, me);
                        } else {
                            me.ChooseBomAttributeItemValueForm(me, attrItemTable, attrDetailRow, row)
                            win.hide();
                        }

                    }
                }
            }

        });
    }
    win.show();
}

//（点击特征项变量明细中特征值时）呼出上级特征项选择窗口
Ext.define('ChooseBomAttrModel', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'ATTRIBUTEITEMID', type: 'string' },
        { name: 'ATTRIBUTEITEMNAME', type: 'string' },
        { name: 'ATTRCODE', type: 'string' }
    ]
});
proto.ChooseBomAttributeItemValueForm = function (me, attrItemTable, attrDetailRow, row) {
    var tableData = [];
    for (var i = 0; i < attrItemTable[2].length; i++) {
        var curdata = attrItemTable[2][i];
        tableData.push([curdata["ATTRIBUTEITEMID"], curdata["ATTRIBUTEITEMNAME"], curdata["ATTRCODE"]]);
    }
    var result;
    var myStore = Ext.create('Ext.data.Store', {
        model: 'ChooseBomAttrModel',
        data: tableData
    });
    if (!win) {
        var win = Ext.create("Ext.Window", {
            title: "上级特征项选择",
            width: 400,
            height: 400,
            layout: "fit",
            modal: true,
            closeAction: 'hide',
            items: {
                xtype: "grid",
                store: myStore,
                columns: [
                    { text: "特征项", dataIndex: "ATTRIBUTEITEMID" },
                    { text: "特征项名称", dataIndex: "ATTRIBUTEITEMNAME" }
                ],
                listeners: {
                    itemdblclick: function (dataview, record, item, index, e) {
                        if (me.isEdit) {
                            result = e.record.data;
                            if (row == -1) {
                                attrDetailRow.set("PARAMVALUE", "A|" + result.ATTRIBUTEITEMID);
                                //特征项说明赋值
                                attrDetailRow.set("PARAMVALUENAME", result.ATTRIBUTEITEMNAME);
                            } else {
                                attrDetailRow.set("PARAMVALUE", "B|" + attrItemTable[1] + "|" + result.ATTRIBUTEITEMID);
                                //特征项说明赋值
                                attrDetailRow.set("PARAMVALUENAME", result.ATTRIBUTEITEMNAME);
                            }
                            win.hide();
                        }
                        else {
                            Ext.Msg.alert("提示", "修改状态下才能修改");
                        }

                    }
                }
            }

        });
    }
    win.show();
}

//（点击特征项变量明细中特征值时）呼出行号选择框
Ext.define('ChooseBomRowAttribute', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'NODENAME', type: 'string' },
        { name: 'ROW_ID', type: 'int' },
    ]
});
proto.ChooseBomRowAttribute = function (me, dataRowItem, attrDetailRow, ex, temp) {
    var tableData = [];
    for (var i = 0; i < dataRowItem.length; i++) {
        var curdata = dataRowItem[i];
        tableData.push([curdata[0], curdata[1]]);
    }
    var result;
    var myStore = Ext.create('Ext.data.Store', {
        model: 'ChooseBomRowAttribute',
        data: tableData
    });
    if (!win) {
        var win = Ext.create("Ext.Window", {
            title: "当前行对应父级",
            width: 200,
            height: 400,
            layout: "fit",
            modal: true,
            closeAction: 'hide',
            items: {
                xtype: "grid",
                store: myStore,
                columns: [
                    { text: "节点名称", dataIndex: "NODENAME" },
                    { text: "行标识", dataIndex: "ROW_ID" }
                ],
                listeners: {
                    itemdblclick: function (dataview, record, item, index, e) {
                        var attrItemTable;
                        var row;//如果row等于0 则说明获取的特征是当前行的父级
                        for (var i = 0; i < dataRowItem.length; i++) {
                            if (dataRowItem[i][1] == record.get("ROW_ID")) {
                                attrItemTable = dataRowItem[i];
                                row = i;
                            }
                        }
                        if (row != 0) {
                            Ext.Msg.confirm('提示', '非父层级是否编辑?', function (button) {
                                if (button == "yes") {
                                    me.ChooseBomMatchItemValueForm(me, attrItemTable, attrDetailRow, ex, temp, row)
                                    win.hide();
                                }
                            }, me);
                        } else {
                            me.ChooseBomMatchItemValueForm(me, attrItemTable, attrDetailRow, ex, temp, row)
                            win.hide();
                        }

                    }
                }
            }

        });
    }
    win.show();
}
//(点击物料选配规则时)呼出
Ext.define('ChooseBomMatch1', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MATERIALTYPENAME', type: 'string' },
        { name: 'ATTRIBUTEITEMID', type: 'string' },
        { name: 'ATTRIBUTEITEMNAME', type: 'string' }
    ]
});
proto.ChooseBomMatchItemValueForm = function (me, attrItemTable, attrDetailRow, ex, json, row) {
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
        model: 'ChooseBomMatch1',
        data: tableData
    });
    if (!win) {
        var win = Ext.create("Ext.Window", {
            title: "选配上级特征项选择",
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
                        me.Attributeitemdetail(me, result, attrDetailRow, ex, dic, attrItemTable[1]);
                        win.close();
                    }
                }
            },
            buttons: [
                {
                    text: '清空',
                    listeners: {
                        'click': function (dataview, record, item, index, e) {
                            if (this.isEdit) {
                                attrDetailRow.set("MATCHRULE", false);
                                attrDetailRow.set("MATCHTEXTBINARY", "");
                                var bindingRow = ex.dataInfo.dataRow.bindingRow || me.getBindingRow.call(me, ex);
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

Ext.define('AttributeitemdetailMatch', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'ATTRCODE', type: 'string' },
        { name: 'ATTRVALUE', type: 'string' },
        {
            name: 'ROWID', type: 'int'
        }
    ]
});
proto.Attributeitemdetail = function (me, attrItemTable, attrDetailRow, e, dic, row) {
    var tableData = [];
    var curdata = this.invorkBcf('GetAttributeitemDetail', [attrItemTable.ATTRIBUTEITEMID]);
    if (curdata.length == 0) {
        Ax.utils.LibVclSystemUtils.openDataFunc("com.AttributeMatch", "特征匹配", [this, attrItemTable, attrDetailRow, dic]);
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
            model: 'AttributeitemdetailMatch',
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
                                        structure = structure + "if(B == '" + rows.data.ATTRCODE + "') ret = " + rows.data.ROWID + " else ";
                                        //structure = structure + "if(A == '/:" + rows.data.ATTRCODE + "/') ret = /:" + rows.data.ROWID + "/ else ";
                                    }
                                }
                                var info = JSON.stringify(Conditions);
                                structure = structure + "ret = -1 | B = " + attrItemTable.ATTRIBUTEITEMID + "/t" + row + ";" + info;
                                //structure = structure + "ret = -1 | A = /:" + attrItemTable.ATTRIBUTEITEMID;
                                attrDetailRow.set("MATCHRULE", true);
                                attrDetailRow.set("MATCHTEXTBINARY", structure);
                                var bindingRow = e.dataInfo.dataRow.bindingRow || me.getBindingRow.call(me, e);
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

//ISATTRDETAIL/将数据源转为树形的数据对象
proto.GetTreeStoreData = function (gen, fillStore, rowId, bomLevel) {//bomLevel 层级
    var obj = [];
    if (gen) {
        var items = this.GetTreeStoreData(false, fillStore, rowId, 1);

        var charString = {};
        var record = this.dataSet.getTable(1).data.items[0];
        if (record != undefined) {
            for (var key in record.data) {
                charString[key] = "";
            }

            charString["expanded"] = true;//是否展开层级
            charString["children"] = items;//是否有下一层
            obj.push(charString);
        }

    }
    else {
        var models = this.GetModelCollection(fillStore, rowId);
        for (var i = 0; i < models.length; i++) {
            var record = models[i];
            var items = this.GetTreeStoreData.call(this, false, fillStore, record.data['ROW_ID'], bomLevel + 1);
            record.data['ORDERNUM'] == 0 && (record.data['ORDERNUM'] = i + 1);
            record.data['BOMLEVEL'] == 0 && (record.data['BOMLEVEL'] = bomLevel);

            var charString = {};
            for (var key in record.data) {
                charString[key] = record.data[key];
            }
            charString["expanded"] = false;//是否展开层级
            charString["children"] = items;//是否有下一层
            obj.push(charString);

        }
    }
    return obj;
}

//取行数据
proto.GetModelCollection = function (fillStore, rowid) {
    var items = fillStore.data.items;
    var mc = [];
    for (var i = 0; i < fillStore.data.length; i++) {
        if (items[i].data['PARENTROWID'] == rowid) {
            mc.push(items[i]);
        }
    }
    return mc;
}

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
    this.tree.setRootNode(this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0)[0]);
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
                    if (columns.xtype == "rownumberer" || columns[l].hidden === true)
                        continue;
                    buildBandCol(columns[l], gridScheme.GridFields);
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
        this.tree.setRootNode(this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0)[0]);

};

//导入
proto.importData = function (fileName) {
    var assistObj = {};
    var data = this.invorkBcf('ImportData', [fileName, this.entryParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success) {
        this.restData(false, BillActionEnum.Browse, data);
        this.tree.setRootNode(this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0)[0]);
    }
    return success;
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
    for (var fieldName in copyItem.dataRow.data) {
        if (fieldName != "STRUCTBOMID" && fieldName != "PARENTROWID" && fieldName != "ROW_ID" && fieldName != "BOMLEVEL" && fieldName != "id") {
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

//通过物料ID  获取结构BOM ID
proto.getSaleBomID = function (materialID) {
    return this.invorkBcf('GetSaleBomID', [materialID]);
}

