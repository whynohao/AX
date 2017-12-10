/// <reference path="LibVclBase.js" />

var BillTypeEnum = {
    Master: 0,
    Bill: 1,
    Grid: 2,
    DataFunc: 3,
    Rpt: 4,
    DailyRpt: 5,
    Visual: 6
};

Ax.vcl.LibVclDataBase = function () {
    Ax.vcl.LibVclBase.apply(this, arguments);
    this.forms = [];
    this.saving = false;
    this.extendParam;
    this.win;
    this.loading = false;
};
var proto = Ax.vcl.LibVclDataBase.prototype = Object.create(Ax.vcl.LibVclBase.prototype);
proto.constructor = Ax.vcl.LibVclDataBase;

proto.vclHandler = function (sender, e) {
    var dataInfo = e.dataInfo;
    var tableIndex = dataInfo ? dataInfo.tableIndex : 0;
    var fieldName = dataInfo ? dataInfo.fieldName : '';
    switch (e.libEventType) {
        case LibEventTypeEnum.Validating:
            if (tableIndex != 0) {
                if (fieldName == 'ROWNO') {
                    var parentRow = dataInfo.curGrid.parentRow;
                    var parentIndex = this.dataSet.getTable(dataInfo.tableIndex)['ParentIndex'];
                    var parentTable = this.dataSet.getTable(parentIndex);
                    var key = this.dataSet.getKey(parentTable.Pks, parentRow);
                    var maxRowNo = this.dataSet.maxRowNo.get((key + '/t' + dataInfo.tableIndex));
                    if (dataInfo.value > maxRowNo) {
                        if (dataInfo.oldValue == maxRowNo) {
                            dataInfo.cancel = true;
                        }
                        else {
                            dataInfo.dataRow.set('ROWNO', maxRowNo);
                            dataInfo.value = maxRowNo;
                        }
                    }
                } else {
                    if (sender) {
                        var className = sender.$className;
                        switch (className) {
                            case 'ax.ux.form.LibSearchField':
                                if (dataInfo.value != '' && sender.store.find('Id', dataInfo.value, 0, false, false, true) == -1) {
                                    dataInfo.cancel = true;
                                }
                                break;
                        }
                    }
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (dataInfo.tableIndex == 0) {
                if (Ext.Array.contains(this.dataSet.getTable(0).Pks, e.dataInfo.fieldName)) {
                    if (this.latestPk)
                        this.latestPk.replace(e.dataInfo.fieldName, e.dataInfo.value);
                    var updatePk = function (pks, model, i) {
                        var key = this.dataSet.getKey(pks, model);
                        var data = this.dataSet.dataMap[i].get(key);
                        this.dataSet.dataMap[i].remove(key);
                        var parentPk;
                        if (i == 0)
                            parentPk = "";
                        else
                            parentPk = this.dataSet.getParentKey(pks, model);
                        var maxRowNoKey = (parentPk + '/t' + i);
                        var maxRowNo = this.dataSet.maxRowNo.get(maxRowNoKey);
                        model.set(e.dataInfo.fieldName, e.dataInfo.value);
                        var newKey = this.dataSet.getKey(pks, model);
                        this.dataSet.dataMap[i].add(newKey, data);
                        this.dataSet.maxRowNo.remove(maxRowNoKey);
                        var newParentPk;
                        if (i == 0)
                            newParentPk = "";
                        else
                            newParentPk = this.dataSet.getParentKey(pks, model);
                        this.dataSet.maxRowNo.add((newParentPk + '/t' + i), maxRowNo);
                    }
                    if (e.dataInfo.curGrid) {
                        var findSubRow = function (key, row) {
                            var pks = this.dataSet.dataList[key].Pks;
                            updatePk.call(this, pks, row, key);
                            if (row['children']) {
                                row['children'].eachKey(function (key, item) {
                                    for (var i = 0; i < item.length; i++) {
                                        findSubRow.call(this, key, item[i]);
                                    }
                                });
                            }
                        }
                        findSubRow.call(this, 0, e.dataInfo.dataRow);
                    } else {
                        for (var i = 1; i < this.dataSet.dataList.length; i++) {
                            var pks = this.dataSet.dataList[i].Pks;
                            if (i != 1)
                                this.dataSet.dataList[i].clearFilter();
                            for (var r = 0; r < this.dataSet.dataList[i].data.items.length; r++) {
                                var model = this.dataSet.dataList[i].data.items[r];
                                updatePk.call(this, pks, model, i);
                            }
                        }
                        updatePk.call(this, this.dataSet.dataList[0].Pks, e.dataInfo.dataRow, 0);
                    }
                }
            } else {
                if (dataInfo.fieldName == 'ROWNO') {
                    this.updateRowNo(this, dataInfo.curGrid, dataInfo.dataRow, dataInfo.value, dataInfo.oldValue);
                }
            }

            if (sender) {
                var className = sender.$className;
                switch (className) {
                    case 'Ax.ux.form.LibAttributeCodeField':
                        var attrId = dataInfo.dataRow.get(sender.attrField);
                        if (attrId && attrId != '') {
                            if (dataInfo.value && dataInfo.value != '') {
                                var ajaxCall = function () {
                                    Ext.Ajax.request({
                                        url: '/billSvc/getAttrDesc',
                                        method: 'POST',
                                        jsonData: {
                                            attrId: attrId, attrCode: dataInfo.value
                                        },
                                        async: false,
                                        timeout: 90000000,
                                        success: function (response) {
                                            var ret = Ext.decode(response.responseText);
                                            ret = ret.GetAttributeDescResult;
                                            dataInfo.value = ret.AttrCode;
                                            dataInfo.dataRow.set(fieldName, ret.AttrCode);
                                            dataInfo.dataRow.set(sender.attrDesc, ret.AttrDesc);
                                            if (dataInfo.curForm)
                                                dataInfo.curForm.loadRecord(dataInfo.dataRow);
                                            //else {
                                            //    var ctrl = Ext.getCmp(me.tpl.Layout.IDMap[fieldName + tableIndex]);
                                            //    ctrl.setValue(ret.AttrCode);
                                            //}
                                        }
                                    });
                                }
                                ajaxCall();
                            } else {
                                dataInfo.dataRow.set(fieldName, '');
                                dataInfo.dataRow.set(sender.attrDesc, '');
                                if (dataInfo.curForm)
                                    dataInfo.curForm.loadRecord(dataInfo.dataRow);
                            }
                        }
                        break;
                    case 'Ax.ux.form.LibSearchField':
                    case 'Ax.ux.form.LibSearchFieldTree':
                        //赋值relative name
                        //var index = sender.store.find('Id', dataInfo.value);
                        //if (index != -1)
                        //    dataInfo.dataRow.set(sender.relName, sender.store.data.items[index].get('Name'));
                        //else
                        //    dataInfo.dataRow.set(sender.relName, '');
                        if (sender.win === undefined)
                            sender.win = sender.up('window');
                        if (sender.win === undefined)
                            sender.win = sender.up('[isVcl=true]');

                        var realRelSource;
                        if (sender.realRelSource) {
                            realRelSource = sender.realRelSource;
                        }
                        else {
                            var obj = {};
                            if (sender.xcontainer)
                                realRelSource = Ax.utils.LibVclSystemUtils.getRelSource(sender, dataInfo, sender.xcontainer.vcl, obj);//Zhangkj 20170206 修改为使用LibVclSystemUtils
                            else
                                realRelSource = Ax.utils.LibVclSystemUtils.getRelSource(sender, dataInfo, sender.win.vcl, obj);//Zhangkj 20170206 修改为使用LibVclSystemUtils
                            if (obj.hasRealRelSource) {
                                sender.realRelSource = realRelSource;
                            }
                        }
                        if (sender.win.vcl.proxy === true) {
                            if (sender.selectFields) {
                                var fn = function (realRelSource) {
                                    var condition = typeof this.condition == "function" ? this.condition() : this.condition ? 'and ' + this.condition : '';
                                    var returnValue;
                                    Ext.Ajax.request({
                                        url: '/billSvc/checkFieldValue',
                                        jsonData: { handle: UserHandle, fields: sender.selectFields, relSource: realRelSource, curPk: dataInfo.value, condition: condition, tableIndex: sender.tableIndex },
                                        method: 'POST',
                                        async: false,
                                        timeout: 90000000,
                                        success: function (response) {
                                            var result = Ext.decode(response.responseText);
                                            returnValue = Ext.decode(result.CheckFieldValueResult);
                                        }
                                    });
                                    if (returnValue) {
                                        for (p in returnValue) {
                                            if (!returnValue.hasOwnProperty(p))
                                                continue;
                                            var value = returnValue[p];
                                            if (value !== undefined && value !== null) {
                                                if (dataInfo.dataRow && dataInfo.dataRow.getField(p) != null) {
                                                    dataInfo.dataRow.set(p, value);
                                                    var ctrl = Ext.getCmp(p + e.dataInfo.tableIndex + '_' + sender.win.getId());
                                                    if (ctrl) {
                                                        if (ctrl.xtype == 'libSearchfield' || ctrl.xtype == 'libSearchfieldTree') {
                                                            ctrl.store.add({ Id: returnValue[ctrl.name], Name: returnValue[ctrl.relName] });
                                                            ctrl.select(returnValue[ctrl.name]);
                                                        }
                                                        else
                                                            ctrl.setValue(value);
                                                    }
                                                }
                                            }
                                        }
                                    } else {
                                        dataInfo.dataRow.set(sender.relName, '');
                                    }
                                }
                                fn.call(sender, realRelSource);
                            }
                        } else {
                            sender.win.vcl.doCheckField(dataInfo.tableIndex, dataInfo.fieldName, realRelSource, sender.relPk, dataInfo.value, dataInfo.dataRow, dataInfo.curGrid);
                        }
                        if (dataInfo.curForm)
                            dataInfo.curForm.loadRecord(dataInfo.dataRow);
                        break;
                }
            }
            break;
        case LibEventTypeEnum.FormClosed:
            if (sender.isSubWin) {
                var curGrid = sender.down('grid');
                if (curGrid.parentFieldName) {
                    var ret = curGrid.store.data.items.length > 0;
                    if (ret != curGrid.parentRow.get(curGrid.parentFieldName))
                        curGrid.parentRow.set(curGrid.parentFieldName, ret);
                }
            }
            break;
    }
    Ax.vcl.LibVclBase.prototype.vclHandler.apply(this, arguments);
};


proto.setDataSet = function (data, isAdd) {
    this.loading = true;
    try {
        this.dataSet.load(data, this, isAdd);
    } finally {
        this.loading = false;
    }
};

proto.browseTo = function (condition) {
    var data = this.invorkBcf("BrowseTo", [condition]);
    this.setDataSet(data,condition, false);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    for (var i = 0; i < this.forms.length; i++) {
        this.forms[i].loadRecord(masterRow);
    };
};

proto.edit = function (condition, assistObj) {
    this.billAction = BillActionEnum.Modif;
    return this.invorkBcf("Edit", [condition], assistObj);
};


proto.cancel = function () {
    this.dataSet.rejectChanges();
    var masterRow = this.dataSet.getTable(0).data.items[0];
    for (var i = 0; i < this.forms.length; i++) {
        this.forms[i].loadRecord(masterRow);
    };
};

proto.setExtendParam = function () {
};

proto.save = function (billAction, condition, assistObj) {
    this.setExtendParam();
    this.saving = true;
    var destData;
    try {
        var masterRow = this.dataSet.getTable(0).data.items[0];
        for (var i = 0; i < this.forms.length; i++) {
            this.forms[i].updateRecord(masterRow);
        };
        var changeRecord = this.dataSet.getChanges();
        var params = billAction === undefined ? [changeRecord, this.extendParam] : [billAction, condition, changeRecord, this.extendParam];
        destData = this.invorkBcf("Save", params, assistObj);
    }
    finally {
        this.saving = false;
    }
    return destData;
};


proto.addRow = function (parentRow, tableIndex) {
    var store = this.dataSet.getTable(tableIndex);
    var newRow = Ext.decode(this.tpl.Tables[this.dataSet.getTable(tableIndex).Name].NewRowObj);
    if (tableIndex != 0) {
        var pks = parentRow.store.Pks;
        var curPks = store.Pks;
        for (var i = 0; i < pks.length; i++) {
            newRow[curPks[i]] = parentRow.get(pks[i]);
        }
        newRow['ROW_ID'] = ++store['MaxRowId'];
        if (newRow.hasOwnProperty("ROWNO")) {
            var key = this.dataSet.getKey(pks, parentRow);
            var maxRowNo = this.dataSet.maxRowNo.get((key + '/t' + tableIndex)) || 0;
            var rowNo = maxRowNo + 1;
            this.dataSet.maxRowNo.replace((key + '/t' + tableIndex), rowNo);
            newRow['ROWNO'] = rowNo;
        }
    }
    var newModel = store.add(newRow)[0];
    this.dataSet.addData(tableIndex, newModel);
    return newModel;
};

proto.addRowForGrid = function (grid) {
    var tableIndex = grid.tableIndex;
    var newRow = Ext.decode(grid.tableDetail.NewRowObj);
    if (tableIndex != 0) {
        var table = this.dataSet.getTable(tableIndex);
        newRow['ROW_ID'] = ++table['MaxRowId'];
        var parentIndex = table['ParentIndex'];
        var parentTable = this.dataSet.getTable(parentIndex);
        var parent;
        var pks = parentTable.Pks;
        var curPks = table.Pks;
        if (grid.parentGrid === undefined) {
            parent = parentTable.data.items[0];
            for (var i = 0; i < pks.length; i++) {
                newRow[curPks[i]] = this.latestPk.get(pks[i]);
            }
        }
        else {
            parent = grid.parentRow;
            for (var i = 0; i < pks.length; i++) {
                newRow[curPks[i]] = parent.get(pks[i]);
            }
        }
        if (grid.tableDetail.UsingRowNo) {
            var key = this.dataSet.getKey(pks, parent);
            var maxRowNo = this.dataSet.maxRowNo.get((key + '/t' + tableIndex)) || 0;
            var rowNo = maxRowNo + 1;
            this.dataSet.maxRowNo.replace((key + '/t' + tableIndex), rowNo);
            newRow['ROWNO'] = rowNo;
        }
    }
    else {
        if (grid.tableDetail.UsingRowId) {
            var table = this.dataSet.getTable(tableIndex);
            newRow['ROW_ID'] = ++table['MaxRowId'];
        }
        if (grid.tableDetail.UsingRowNo) {
            var key = "";
            var maxRowNo = this.dataSet.maxRowNo.get((key + '/t' + tableIndex)) || 0;
            var rowNo = maxRowNo + 1;
            this.dataSet.maxRowNo.replace((key + '/t' + tableIndex), rowNo);
            newRow['ROWNO'] = rowNo;
        }
    }
    var newModel = grid.store.add(newRow)[0];
    var index = grid.store.data.length - 1;
    grid.getView().scrollRowIntoView(index);
    this.dataSet.addData(tableIndex, newModel);
    return newModel;
};

proto.copyRowForGrid = function (grid) {
    var select = grid.getSelectionModel().getSelected();
    if (select.length != 1) {
        Ext.Msg.alert("提示", "请选择一行数据进行复制");
        return;
    }
    var tableIndex = grid.tableIndex;
    var newRow = new Object();
    $.extend(newRow, select.items[0].data);
    delete newRow.id;
    if (newRow.AUDITSTATE != undefined) {
      newRow.AUDITSTATE = 0
      newRow.FLOWLEVEL = 0
    }
    if (tableIndex != 0) {
        var table = this.dataSet.getTable(tableIndex);
        newRow['ROW_ID'] = ++table['MaxRowId'];
        var parentIndex = table['ParentIndex'];
        var parentTable = this.dataSet.getTable(parentIndex);
        var parent;
        var pks = parentTable.Pks;
        var curPks = table.Pks;
        if (grid.parentGrid === undefined) {
            parent = parentTable.data.items[0];
            for (var i = 0; i < pks.length; i++) {
                newRow[curPks[i]] = this.latestPk.get(pks[i]);
            }
        }
        else {
            parent = grid.parentRow;
            for (var i = 0; i < pks.length; i++) {
                newRow[curPks[i]] = parent.get(pks[i]);
            }
        }
        if (grid.tableDetail.UsingRowNo) {
            var key = this.dataSet.getKey(pks, parent);
            var maxRowNo = this.dataSet.maxRowNo.get((key + '/t' + tableIndex)) || 0;
            var rowNo = maxRowNo + 1;
            this.dataSet.maxRowNo.replace((key + '/t' + tableIndex), rowNo);
            newRow['ROWNO'] = rowNo;
        }
    }
    else {
        if (grid.tableDetail.UsingRowId) {
            var table = this.dataSet.getTable(tableIndex);
            newRow['ROW_ID'] = ++table['MaxRowId'];
        }
        if (grid.tableDetail.UsingRowNo) {
            var key = "";
            var maxRowNo = this.dataSet.maxRowNo.get((key + '/t' + tableIndex)) || 0;
            var rowNo = maxRowNo + 1;
            this.dataSet.maxRowNo.replace((key + '/t' + tableIndex), rowNo);
            newRow['ROWNO'] = rowNo;
        }
    }
    var newModel = grid.store.add(newRow)[0];
    var index = grid.store.data.length - 1;
    grid.getView().scrollRowIntoView(index);
    this.dataSet.addData(tableIndex, newModel);
    return newModel;
};

proto.deleteRow = function (tableIndex, curRow) {
    var store = this.dataSet.getTable(tableIndex);
    store.remove(curRow);
    this.dataSet.deleteData(tableIndex, curRow);
};

proto.deleteAll = function (tableIndex) {
    var store = this.dataSet.getTable(tableIndex);
    for (var i = 0; i < store.data.items; i++) {
        this.dataSet.deleteData(tableIndex, store.data.items[i]);
    }
    store.removeAll();
};

proto.deleteRowForGrid = function (grid) {
    var tableIndex = grid.tableIndex;
    var selection = grid.getView().getSelectionModel().getSelection();
    if (selection) {
        var parentRow = grid.parentRow;
        var key = '';
        if (parentRow) {
            var parentIndex = this.dataSet.getTable(grid.tableIndex)['ParentIndex'];
            var parentTable = this.dataSet.getTable(parentIndex);
            key = this.dataSet.getKey(parentTable.Pks, parentRow);
        }
        var maxRowNoKey = key + '/t' + tableIndex;
        var minDelRowNo = this.dataSet.maxRowNo.get(maxRowNoKey);
        var count = selection.length;
        var delRowNo = [];
        for (var i = 0; i < count; i++) {
            if (grid.tableDetail.UsingRowNo) {
                var rowNo = selection[0].get('ROWNO');
                delRowNo.push(rowNo);
                if (minDelRowNo > rowNo)
                    minDelRowNo = rowNo;
            }
            grid.store.remove(selection[i]);
            this.dataSet.deleteData(tableIndex, selection[i]);
        }
        if (grid.tableDetail.UsingRowNo) {
            var items = grid.store.data.items;
            for (var i = 0; i < items.length; i++) {
                var rowNo = items[i].get('ROWNO');
                if (rowNo > minDelRowNo) {
                    var temp = 0;
                    for (var r = 0; r < delRowNo.length; r++) {
                        if (rowNo > delRowNo[r])
                            temp++;
                    }
                    items[i].set('ROWNO', rowNo - temp);
                }
            }
            this.dataSet.maxRowNo.replace(maxRowNoKey, this.dataSet.maxRowNo.get(maxRowNoKey) - delRowNo.length);
        }

    }
};


proto.updateRowNo = function (vcl, grid, curRow, updateRowNo, oldValue) {
    if (grid.tableDetail.UsingRowNo) {
        if (updateRowNo == oldValue)
            return;
        var curRowId = curRow.get('ROW_ID');
        var items = grid.store.data.items;
        var maxRowNo = items.length;
        var less = updateRowNo < oldValue;
        var model;
        var dic = new Array();
        for (var i = 0; i < items.length; i++) {
            var rowNo = items[i].get('ROWNO');
            if (less) {
                if (rowNo > oldValue || rowNo < updateRowNo)
                    continue
                if (rowNo >= updateRowNo && curRowId != items[i].get('ROW_ID')) {
                    model = {
                        ROW_ID: items[i].get('ROW_ID'),
                        ROWNO: rowNo + 1
                    }
                    dic.push(model);
                }
            } else {
                if (rowNo < oldValue || rowNo > updateRowNo)
                    continue
                if (rowNo <= updateRowNo && curRowId != items[i].get('ROW_ID')) {
                    model = {
                        ROW_ID: items[i].get('ROW_ID'),
                        ROWNO: rowNo - 1
                    }
                    dic.push(model);
                }
                if (maxRowNo < rowNo && curRowId == items[i].get('ROW_ID')) {
                    model = {
                        ROW_ID: items[i].get('ROW_ID'),
                        ROWNO: maxRowNo
                    }
                    dic.push(model);
                }
            }
        }
        for (var i = 0; i < dic.length; i++) {
            for (var j = 0; j < items.length; j++) {
                if (dic[i].ROW_ID == items[j].get('ROW_ID')) {
                    items[j].set('ROWNO', (dic[i].ROWNO));
                    break;
                }
            }
        }
    }
};
