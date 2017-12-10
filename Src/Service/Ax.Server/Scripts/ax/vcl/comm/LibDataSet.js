Ext.ns('Ax.vcl');

Ax.vcl.LibDataSet = function () {
    this.dataMap;
    this.dataList;
    this.tableMap;
    this.maxRowNo;
};

Ax.vcl.LibDataSet.prototype = {
    constructor: Ax.vcl.LibDataSet,
    getParentKey: function (pks, model) {
        var key = '';
        for (var l = 0; l < pks.length - 1; l++) {
            if (l != 0)
                key += '/t';
            key += model.get(pks[l]);
        }
        return key;
    },
    getKey: function (pks, model) {
        var key = '';
        for (var l = 0; l < pks.length; l++) {
            if (l != 0)
                key += '/t';
            key += model.get(pks[l]);
        }
        return key;
    },
    initMaxRowNo: function (newModel, key) {
        var rowNo = newModel.get('ROWNO');
        if (!this.maxRowNo.containsKey(key))
            this.maxRowNo.add(key, rowNo);
        else
            this.maxRowNo.replace(key, rowNo);
    },
    getTable: function (table) {
        var index;
        if (typeof table === 'number')
            index = table;
        else
            index = this.tableMap.get(table);
        return this.dataList[index];
    },
    FindRow: function (table, rowId) {
        var index;
        if (typeof table === 'number')
            index = table;
        else
            index = this.tableMap.get(table);
        return this.dataMap[index].get(rowId);
    },
    load: function (data, vcl, addNew) {
        var tpl = vcl.tpl;
        this.tableMap = new Ext.util.MixedCollection(), this.dataMap = [], this.maxRowNo = new Ext.util.MixedCollection();
        var i = 0;
        var empty = this.dataList === undefined || this.dataList.length == 0;
        if (empty)
            this.dataList = [];
        Ext.suspendLayouts();
        try {

            for (table in data) {
                if (!data.hasOwnProperty(table))
                    continue;
                if (empty) {
                    var modelName = !tpl.Tables[table].IsDynamic ? (tpl.ProgId + table) : (tpl.ProgId + table + vcl.winId);
                    var modelType = Ext.data.Model.schema.getEntity(modelName);
                    if (modelType === null) {
                        modelType = Ext.define(modelName, {
                            extend: 'Ext.data.Model',
                            fields: Ext.decode(tpl.Tables[table].Fields),
                            proxy: {
                                type: 'memory',
                                reader: {
                                    type: 'json'
                                }
                            }
                        });
                    }
                    this.dataList[i] = Ext.create('Ext.data.Store', {
                        model: modelType,
                        proxy: {
                            type: 'memory',
                            reader: {
                                type: 'json'
                            }
                        }
                    });
                }
                else {
                    this.dataList[i].removeAll(true);
                }
                this.dataList[i].suspendEvents();
                this.dataList[i].beginUpdate();
                try {
                    this.dataList[i]['Name'] = table;
                    var pks = tpl.Tables[table].Pk;
                    if (i == 0) {
                        this.dataMap[0] = new Ext.util.MixedCollection();
                        var maxRowId = 0;
                        for (var r = 0; r < data[table].length; r++) {
                            if (tpl.Tables[table].UsingRowId) {
                                var curRowId = data[table][r]["ROW_ID"];
                                if (maxRowId < curRowId)
                                    maxRowId = curRowId;
                            }
                            var newModel = this.dataList[i].add(data[table][r])[0];
                            var key = this.getKey(pks, newModel);
                            this.dataMap[0].add(key, newModel);
                            if (tpl.Tables[table].UsingRowNo) {
                                this.initMaxRowNo(newModel, '/t0');
                            }
                        }
                        if (tpl.Tables[table].UsingRowId)
                            this.dataList[i]['MaxRowId'] = maxRowId;
                    }
                    else {
                        var maxRowId = 0;
                        var parentIndex = tpl.Tables[table].ParentIndex;
                        this.dataMap[i] = new Ext.util.MixedCollection();
                        for (var r = 0; r < data[table].length; r++) {
                            var curRowId = data[table][r]["ROW_ID"];
                            if (maxRowId < curRowId)
                                maxRowId = curRowId;
                            var newModel = this.dataList[i].add(data[table][r])[0];
                            this.dataMap[i].add(curRowId, newModel);
                            var parent;
                            var key = this.getParentKey(pks, newModel);
                            if (parentIndex == 0) {
                                parent = this.dataMap[parentIndex].get(key);
                            } else {
                                parent = this.dataMap[parentIndex].get(newModel.get(pks[pks.length - 2]));
                            }
                            if (parent) {
                                if (!parent.hasOwnProperty('children'))
                                    parent['children'] = new Ext.util.MixedCollection();
                                if (!parent['children'].containsKey(i))
                                    parent['children'].add(i, []);
                                parent['children'].get(i).push(newModel);
                                if (tpl.Tables[table].UsingRowNo) {
                                    this.initMaxRowNo(newModel, (key + '/t' + i));
                                }
                            }
                        }
                        this.dataList[i]['MaxRowId'] = maxRowId;
                        this.dataList[i]['ParentIndex'] = parentIndex;
                    }
                    this.dataList[i]['Pks'] = pks;
                    this.tableMap.add(table, i);
                    if (vcl.sortRuler && vcl.sortRuler.hasOwnProperty(table)) {
                        this.dataList[i].sort(vcl.sortRuler[table]);
                    }
                    else {
                        if (tpl.Tables[table].UsingRowNo) {
                            this.dataList[i].sort([{ property: 'ROWNO', direction: 'ASC' }]);
                        }
                    }
                    if (!addNew)
                        this.dataList[i].sync();
                    i++;
                } finally {
                    var curStore = this.dataList[(i - 1)];
                    curStore.endUpdate();
                    curStore.resumeEvents();
                    if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
                        curStore.ownGrid.reconfigure(curStore);
                }
            }
        } finally {
            Ext.resumeLayouts(true);
        }
    },
    getParent: function (table, curRow) {
        var index, parent;
        if (typeof table === 'number')
            index = table;
        else
            index = this.tableMap.get(table);
        if (index != 0) {
            var parentIndex = this.dataList[index]['ParentIndex'];
            if (parentIndex == 0) {
                var key = this.getParentKey(this.dataList[parentIndex]['Pks'], curRow);
                parent = this.dataMap[parentIndex].get(key);
            }
            else {
                var pks = this.dataList[index]['Pks'];
                parent = this.dataMap[parentIndex].get(curRow.get(pks[pks.length - 2]));
            }
        }
        return parent;
    },
    getChildren: function (table, curRow, childTable) {
        var index, children = [], childrenIndex;
        if (typeof table === 'number')
            index = table;
        else
            index = this.tableMap.get(table);
        if (typeof childTable === 'number')
            childrenIndex = childTable;
        else
            childrenIndex = this.tableMap.get(childTable);
        if (index != this.dataList.length - 1) {
            if (curRow['children'] != undefined)
                children = curRow['children'].get(childrenIndex);
        }
        return children;
    },
    addData: function (table, newModel) {
        var index;
        if (typeof table === 'number')
            index = table;
        else
            index = this.tableMap.get(table);
        if (index == 0) {
            var key = this.getKey(this.dataList[index].Pks, newModel);
            this.dataMap[index].add(key, newModel);
        } else {
            this.dataMap[index].add(newModel.get('ROW_ID'), newModel);
            var parentIndex = this.dataList[index]['ParentIndex'];
            var parent;
            if (parentIndex == 0) {
                var key = this.getParentKey(this.dataList[index].Pks, newModel);
                parent = this.dataMap[parentIndex].get(key);
            } else {
                var pks = this.dataList[index].Pks;
                parent = this.dataMap[parentIndex].get(newModel.get(pks[pks.length - 2]));
            }
            if (parent) {
                if (!parent.hasOwnProperty('children'))
                    parent['children'] = new Ext.util.MixedCollection();
                if (!parent['children'].containsKey(index))
                    parent['children'].add(index, []);
                parent['children'].get(index).push(newModel);
            }
        }
    },
    deleteData: function (table, model) {
        var index;
        if (typeof table === 'number')
            index = table;
        else
            index = this.tableMap.get(table);
        if (index == 0) {
            var key = this.getKey(this.dataList[index].Pks, model);
            this.dataMap[index].removeAtKey(key);
        } else {
            this.dataMap[index].removeAtKey(model.get('ROW_ID'));
            var parentIndex = this.dataList[index]['ParentIndex'];
            var parent;
            if (parentIndex == 0) {
                var key = this.getParentKey(this.dataList[index].Pks, model);
                parent = this.dataMap[parentIndex].get(key);
            } else {
                var pks = this.dataList[index].Pks;
                parent = this.dataMap[parentIndex].get(model.get(pks[pks.length - 2]));
            }
            if (parent.hasOwnProperty('children') && parent['children'].containsKey(index)) {
                // parent['children'].get(index).pop(model);
                var items = parent['children'].get(index);
                var rowId = model.get('ROW_ID');
                for (var i = items.length - 1; i >= 0; i--) {
                    if (rowId == items[i].get('ROW_ID'))
                        items.splice(i, 1);
                }
            }
        }
    },
    getChanges: function () {
        var changeRecord = {};
        for (var t = 0; t < this.dataList.length; t++) {
            var store = this.dataList[t];
            store.clearFilter(); //表如果存在filter，提交时要清空
            var pks = this.dataList[t]['Pks'];
            var data = {
                add: [],
                modif: [],
                remove: []
            };
            var records = store.getNewRecords();
            if (records) {
                var length = records.length;
                for (var i = 0; i < length; i++) {
                    var temp = {};
                    Ext.apply(temp, records[i].data);
                    delete temp.id;
                    data.add.push(temp);
                }
            }
            records = store.getUpdatedRecords();
            if (records) {
                var length = records.length;
                for (var i = 0; i < length; i++) {
                    var temp = {};
                    for (var r = 0; r < pks.length; r++) {
                        temp['_' + pks[r]] = records[i].modified.hasOwnProperty(pks[r]) ? records[i].modified[pks[r]] : records[i].data[pks[r]];
                    }
                    for (p in records[i].modified) {
                        if (!records[i].modified.hasOwnProperty(p))
                            continue;
                        temp[p] = records[i].data[p];
                    }
                    data.modif.push(temp);
                }
            }
            records = store.getRemovedRecords();
            if (records) {
                var length = records.length;
                for (var i = 0; i < length; i++) {
                    var temp = {};
                    for (var r = 0; r < pks.length; r++) {
                        temp[pks[r]] = (records[i].modified != null && records[i].modified.hasOwnProperty(pks[r])) ? records[i].modified[pks[r]] : records[i].data[pks[r]];
                    }
                    data.remove.push(temp);
                }
            }
            changeRecord[store.Name] = data;
        }
        return changeRecord;
    },
    rejectChanges: function () {
        for (var i = this.dataList.length - 1; i >= 0 ; i--) {
            this.dataList[i].rejectChanges();
        }
        this.dataMap = [];
        for (var i = 0; i < this.dataList.length; i++) {
            var pks = this.dataList[i]['Pks'];
            if (i == 0) {
                this.dataMap[0] = new Ext.util.MixedCollection();
                this.dataMap[0].add('0', this.dataList[0].data.items[0]);
                for (var r = 0; r < this.dataList[i].data.items.length; r++) {
                    var record = this.dataList[i].data.items[r];
                    var key = this.getParentKey(pks, record);
                    this.dataMap[i].add(key, record);
                }
            }
            else {
                var maxRowId = 0;
                var parentIndex = this.dataList[i]['ParentIndex'];
                this.dataMap[i] = new Ext.util.MixedCollection();
                for (var r = 0; r < this.dataList[i].data.items.length; r++) {
                    var record = this.dataList[i].data.items[r];
                    var curRowId = record["ROW_ID"];
                    if (maxRowId < curRowId)
                        maxRowId = curRowId;
                    this.dataMap[i].add(curRowId, record);
                    var parent;
                    if (parentIndex == 0) {
                        var key = this.getParentKey(pks, record);
                        parent = this.dataMap[parentIndex].get(key);
                    } else {
                        parent = this.dataMap[parentIndex].get(record.get(pks[pks.length - 2]));
                    }
                    if (parent) {
                        if (!parent.hasOwnProperty('children'))
                            parent['children'] = new Ext.util.MixedCollection();
                        if (!parent['children'].containsKey(i))
                            parent['children'].add(i, []);
                        parent['children'].get(i).push(record);
                    }
                }
                this.dataList[i]['MaxRowId'] = maxRowId;
            }
        }
    }
};