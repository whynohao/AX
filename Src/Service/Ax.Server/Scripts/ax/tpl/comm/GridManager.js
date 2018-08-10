/// <reference path="../../ux/LibTimeField.js" />
Ext.ns('Ax.tpl')

//呼叫新窗口（子子表）
Ax.tpl.GridManager = {
    createGrid: function (gridInfo) {
        var curVcl = gridInfo.vcl, parentRow = gridInfo.parentRow, tableIndex = gridInfo.tableIndex,
          curRange = gridInfo.curRange, parentGrid = gridInfo.parentGrid, height = gridInfo.height,
          parentFieldName = gridInfo.parentFieldName, title = gridInfo.title, isEditGrid = gridInfo.isEditGrid;
        var vcl = curVcl;
        var buffer = false;
        if (vcl.billType == 4)
            buffer = true;
        var store = vcl.dataSet.getTable(tableIndex);
        if (parentFieldName) {
            var curPks = vcl.dataSet.getTable(tableIndex).Pks;
            var parentPks = vcl.dataSet.getTable(parentGrid.tableIndex).Pks;
            store.clearFilter();
            var filter = function (record) {
                var ret = true;
                for (var i = 0; i < parentPks.length; i++) {
                    if (record.get(curPks[i]) != parentRow.get(parentPks[i]))
                        ret = false;
                }
                return ret;
            }
            store.filterBy(function (record) {
                return filter(record);
            });
        }
        var tableDetail = vcl.tpl.Tables[vcl.dataSet.getTable(tableIndex).Name];
        var dockedItems;
        var selectAll = false;
        var select = Ext.create(Ext.Action, {
            text: "全选",
            width: 70,
            scope: this,
            handler: function () {
                if (!selectAll) {
                    grid.getSelectionModel().selectAll();
                    selectAll = true;
                    select.setText("全反选");
                }
                else {
                    grid.getSelectionModel().deselectAll();
                    selectAll = false;
                    select.setText("全选");
                }
            }
        });
        var btnClipboardId = Ext.id()
        var clipboardobj = null
        var selectedCell;
        var btnClipboard = Ext.create(Ext.Action, {
            text: '复制到剪切板',
            id: btnClipboardId,
            width: 120,
            scope: this,
            handler: function () {
            }
        })

        var copyButton = Ext.create(Ext.Action, {
            text: "复制",
            width: 70,
            scope: this,
            handler: function () {
                if (vcl.isEdit === false)
                    return;
                if (vcl.billType == BillTypeEnum.Grid && vcl.canAdd === undefined) {
                    vcl.canAdd = vcl.invorkBcf('HasAddRowPermission');
                }
                if (vcl.billType != BillTypeEnum.Grid || vcl.canAdd) {
                    var dataInfo = {
                        cancel: false,
                        value: null,
                        oldValue: null,
                        fieldName: null,
                        tableIndex: grid.tableIndex,
                        dataRow: null,
                        curForm: null,
                        curGrid: grid
                    };
                    vcl.vclHandler(self, {libEventType: LibEventTypeEnum.BeforeAddRow, dataInfo: dataInfo});
                    if (!dataInfo.cancel) {
                        grid.manualing = true;
                        try {
                            vcl.copyRowForGrid(grid);
                        } finally {
                            grid.manualing = false;
                        }
                    }
                }
            }
        });
        if (isEditGrid !== false) {
            var gridFuncItems = [{
                iconCls: 'gridAdd',
                scope: this,
                handler: function () {
                    gridAddNewData()
                }
            }, {
                iconCls: 'gridDelete',
                scope: this,
                handler: function () {
                    if (vcl.isEdit === false || grid.getSelectionModel().getLastSelected() == undefined)
                        return;
                    if (vcl.billType == BillTypeEnum.Grid && vcl.canDelete === undefined) {
                        vcl.canDelete = vcl.invorkBcf('HasDeleteRowPermission');
                    }
                    if (vcl.billType != BillTypeEnum.Grid || vcl.canDelete) {
                        var dataInfo = {
                            cancel: false,
                            value: null,
                            oldValue: null,
                            fieldName: null,
                            tableIndex: grid.tableIndex,
                            dataRow: grid.getSelectionModel().getLastSelected(),
                            curForm: null,
                            curGrid: grid
                        };
                        vcl.vclHandler(self, {libEventType: LibEventTypeEnum.BeforeDeleteRow, dataInfo: dataInfo});
                        if (!dataInfo.cancel) {
                            grid.manualing = true;
                            try {
                                vcl.deleteRowForGrid(grid);
                            } finally {
                                grid.manualing = false;
                            }
                        }
                    }
                }
            }]
            gridFuncItems.push(copyButton)
            gridFuncItems.push(select)
            gridFuncItems.push(btnClipboard)
            if (tableDetail.UsingApproveRow) {
                gridFuncItems.push({
                    text: '添加审核',
                    handler: function () {
                        if (vcl.isEdit === true)
                            return;
                        var records = grid.getView().getSelectionModel().getSelection();
                        if (records.length > 0) {
                            var dataList = [];
                            for (var i = 0; i < records.length; i++) {
                                var state = records[i].get('AUDITSTATE');
                                if (state == 0 || state == 3) {
                                    dataList.push(records[i]);
                                }
                            }
                            if (vcl.approveRowForm == null)
                                vcl.approveRowForm = new Ax.utils.LibApproveRowForm(vcl, store.Name, curRange.Renderer, grid);
                            vcl.approveRowForm.preSubmitStore.loadData(dataList, true);
                            vcl.approveRowForm.show("sendApprove");
                        }
                    }
                });
                gridFuncItems.push({
                    text: '添加弃审',
                    handler: function () {
                        if (vcl.isEdit === true)
                            return;
                        var records = grid.getView().getSelectionModel().getSelection();
                        if (records.length > 0) {
                            var dataList = [];
                            for (var i = 0; i < records.length; i++) {
                                var level = records[i].get('FLOWLEVEL');
                                if (level > 0) {
                                    dataList.push(records[i]);
                                }
                            }
                            if (vcl.approveRowForm == null)
                                vcl.approveRowForm = new Ax.utils.LibApproveRowForm(vcl, store.Name, curRange.Renderer, grid);
                            vcl.approveRowForm.preCancelStore.loadData(dataList, true);
                            vcl.approveRowForm.show("sendUnApprove");
                        }
                    }
                });
                gridFuncItems.push({
                    text: '行项审核列表',
                    handler: function () {
                        if (vcl.isEdit === true)
                            return;
                        if (vcl.approveRowForm == null)
                            vcl.approveRowForm = new Ax.utils.LibApproveRowForm(vcl, store.Name, curRange.Renderer, grid);
                        vcl.approveRowForm.show();
                    }
                });
                gridFuncItems.push({
                    text: '查看审核流程',
                    handler: function () {
                        var records = grid.getView().getSelectionModel().getSelection();
                        if (records.length == 1) {
                            var flowForm = new Ax.utils.LibApproveFlowForm(vcl, undefined, records[0], false);
                            flowForm.show();
                        }
                    }
                });
                gridFuncItems.push({
                    text: '查看流程配置',
                    handler: function () {
                        var records = grid.getView().getSelectionModel().getSelection();
                        if (records.length == 1) {
                            var flowForm = new Ax.utils.LibApproveFlowForm(vcl, undefined, records[0], false, true);//仅显示流程配置
                            flowForm.show();
                        }
                    }
                });
                gridFuncItems.push({
                    text: '版本',
                    handler: function () {
                        var records = grid.getView().getSelectionModel().getSelection();
                        if (records.length == 1) {
                            new Ax.utils.LibApproveRowVersionForm.show(vcl, store.Name, records[0], grid.tableIndex);
                        }
                    }
                });
            }
            if (tableDetail.UsingAttachment) {
                gridFuncItems.push({
                    text: '附件',
                    handler: function () {
                        var records = grid.getView().getSelectionModel().getSelection();
                        if (records.length == 1) {
                            var table = vcl.dataSet.getTable(grid.tableIndex);
                            Ax.utils.LibAttachmentForm.show(vcl, records[0], table.Name);
                        }
                    }
                });
            }

            //添加扩展的按钮
            if (gridInfo.buttons!=undefined) {
                Ext.Array.forEach(gridInfo.buttons,function(btn,index,array){
                    gridFuncItems.push(btn);
                });
            }

            dockedItems = [{
                xtype: 'toolbar',
                items: gridFuncItems
            }];
        }
        var destColumns = Ext.decode(curRange.Renderer);
        var colFunc = function (columns) {
            for (var i = 0; i < columns.length; i++) {
                if (columns[i].columns)
                    colFunc(columns[i].columns);
                else if (columns[i].hasOwnProperty('summaryRenderer')) {
                    columns[i].summaryRenderer = vcl.summaryRenderer[columns[i].summaryRenderer];
                }
            }
        };
        colFunc(destColumns);
        var cellEditing = Ext.create('Ext.grid.plugin.CellEditing', {
            clicksToEdit: 1
        });
        var getRowClipboard = function (dom) {
            var clipboardContent = ''
            var divs = $('td>div', dom)
            for (var i = 0; i < divs.length; i++) {
                var div = divs[i]
                if (div.firstChild && div.firstChild.className && div.firstChild.className.indexOf('x-grid-checkcolumn-checked') !== -1) {
                    clipboardContent += '1\t'
                    continue
                }
                if (div.firstChild && div.firstChild.className && div.firstChild.className.indexOf('x-grid-checkcolumn') !== -1) {
                    clipboardContent += '0\t'
                    continue
                }
                var content = ''
                if (div.innerHTML !== '&nbsp;') {
                    content = div.innerHTML
                }
                clipboardContent += content + '\t'
                continue
            }
            return clipboardContent
        }
        var gridViewDom = null
        var gridViewKeyPress = function (event) {
            var e = event || window.event || arguments.callee.caller.arguments[0]
            var ctrlKey = e.ctrlKey || e.metaKey
            // if (ctrlKey && e.keyCode === 67) {
            // }
            if (ctrlKey && e.keyCode === 40) {
                gridAddNewData()
            }
        }
        var gridAddNewData = function () {
            if (vcl.isEdit === false)
                return;
            if (vcl.billType == BillTypeEnum.Grid && vcl.canAdd === undefined) {
                vcl.canAdd = vcl.invorkBcf('HasAddRowPermission');
            }
            if (vcl.billType != BillTypeEnum.Grid || vcl.canAdd) {
                var dataInfo = {
                    cancel: false,
                    value: null,
                    oldValue: null,
                    fieldName: null,
                    tableIndex: grid.tableIndex,
                    dataRow: null,
                    curForm: null,
                    curGrid: grid
                };
                vcl.vclHandler(self, {libEventType: LibEventTypeEnum.BeforeAddRow, dataInfo: dataInfo});
                if (!dataInfo.cancel) {
                    grid.manualing = true;
                    try {
                        vcl.addRowForGrid(grid);
                        selectAll = false;
                        select.setText("全选");
                    } finally {
                        grid.manualing = false;
                    }
                }
            }
        }
        var grid = Ext.create('Ext.grid.Panel', {
            border: false,
            bufferedRenderer: buffer,
            collapsible: title === undefined ? false : true,
            title: title,
            margin: '6 2 2 2',
            plugins: [cellEditing,
              'gridfilters'
            ],
            features: [{
                ftype: 'summary',
                dock: 'bottom'
            }],
            flex: 1,
            dockedItems: dockedItems,
            tableDetail: tableDetail,
            store: store,
            // selType: 'cellmodel',
            selModel: {mode: 'MULTI'},
            id: vcl.winId + vcl.dataSet.getTable(tableIndex).Name + 'Grid',
            tableIndex: tableIndex,
            columns: destColumns,
            parentRow: parentRow,
            parentGrid: parentGrid,
            parentFieldName: parentFieldName,
            listeners: {
                cellclick (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
              const text = td.textContent
              const temp = text.split(',')
        selectedCell = temp[0]
    },
    afterrender: function (self, eOpts) {
        //clipboardobj = new Clipboard('#' + btnClipboardId, {
        //  text: function (trigger) {
        //    var records = grid.getView().getSelectionModel().getSelection();
        //    var clipboardContent = ''
        //    for (var i = 0; i < records.length; i++) {
        //      var record = records[i]
        //      var node = grid.view.getNode(record)
        //      var table = Ext.get(node)
        //      var row = table.query('tr')
        //      var rowContent = getRowClipboard(row)
        //      clipboardContent += rowContent + '\r\n'
        //    }
        //    return clipboardContent
        //  }
        //})
        $(document.body).bind({
            copy: function (e) { //copy事件
                var clipboardData = window.clipboardData //for IE
                var selectionObj = window.getSelection();
                var selectedText = selectionObj.toString();
                if (!clipboardData) { // for chrome
                    clipboardData = e.originalEvent.clipboardData
                }
                if(selectedText&&selectedText!=''){
                    clipboardData.setData('Text', selectedText)
                }
                else {
                    clipboardData.setData('Text', selectedCell)
                }
                return false//否则设不生效
            }
        })
    },
    beforedestroy: function (self, eOpts) {
        try {
            if (clipboardobj) {
                clipboardobj.destroy()
            }
            $(document.body).unbind("copy");
        } catch (e) {
            console.error(e)
        }
    },
    select: function (self, record, index, eOpts) {
        var curSelected = grid.getSelectionModel().selected.length;
        var totalCount = grid.store.data.length;
        if (curSelected == totalCount) {
            selectAll = true;
            select.setText("全反选");
        }
    },
    deselect: function (self, record, index, eOpts) {
        var curSelected = grid.getSelectionModel().selected.length;
        var totalCount = grid.store.data.length;
        if (curSelected < totalCount) {
            selectAll = false;
            select.setText("全选");
        }

    },
    beforeedit: function (self) {
        if (vcl.isEdit === false)
            return false;
    },
    celldblclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
        var dataInfo = Ax.Control.LibDataInfo.getDataInfoForGrid(self, td, cellIndex, record, tr, rowIndex, e);
        vcl.vclHandler(self, {libEventType: LibEventTypeEnum.ColumnDbClick, dataInfo: dataInfo});
        if (!dataInfo.cancel) {
            var subIndex = grid.tableDetail.SubTableMap[dataInfo.fieldName];
            if (subIndex) {
                Ax.tpl.GridManager.callSubBill(vcl, dataInfo.dataRow, dataInfo.curGrid, subIndex, dataInfo.fieldName);
            } else {
                var col = self.panel.columnManager.columns[cellIndex];
                if (col.attrField) {
                    var attrId = dataInfo.dataRow.get(col.attrField);
                    if (attrId && attrId != '')
                        Ax.utils.LibAttributeForm.show(vcl, col, dataInfo);
                } else if (col.relSource) {
                    var realRelSource;
                    if (col.realRelSource) {
                        realRelSource = col.realRelSource;
                    }
                    else {
                        var obj = {};
                        realRelSource = Ax.utils.LibVclSystemUtils.getRelSource(col, dataInfo, vcl, obj);
                        if (obj.hasRealRelSource) {
                            col.realRelSource = realRelSource;
                        }
                    }
                    vcl.doF4(dataInfo.tableIndex, dataInfo.fieldName, realRelSource, col.relPk, dataInfo.value, dataInfo.dataRow, dataInfo.curGrid);
                }
            }
        }
    },
    validateedit: function (editor, context, eOpts) {
        var dataInfo = {
            cancel: false,
            value: context.value,
            oldValue: context.originalValue,
            fieldName: context.field,
            tableIndex: tableIndex,
            dataRow: context.record,
            curForm: null,
            curGrid: grid
        };
        if (dataInfo.value != dataInfo.oldValue) {
            var self = Ext.getCmp(context.field + tableIndex + '_' + vcl.winId);
            vcl.vclHandler(self, {libEventType: LibEventTypeEnum.Validating, dataInfo: dataInfo});
        }
        return !dataInfo.cancel;
    },
    edit: function (editor, context, eOpts) {
        var dataInfo = {
            cancel: false,
            value: context.value,
            oldValue: context.originalValue,
            fieldName: context.field,
            tableIndex: tableIndex,
            dataRow: context.record,
            curForm: null,
            curGrid: grid
        };
        if (dataInfo.value != dataInfo.oldValue) {
            var self = Ext.getCmp(context.field + tableIndex + '_' + vcl.winId);
            vcl.vclHandler(self, {libEventType: LibEventTypeEnum.Validated, dataInfo: dataInfo});
        }
    }
},
viewConfig: {
    getRowClass: function (record, rowIndex, rowParams, store) {
        if (vcl.fontColor) {
            var bacClass = vcl.fontColor
            return bacClass
        }
    },
    listeners: {
            viewready: function (self, eOpts) {
                gridViewDom = grid.getView().el.dom
                gridViewDom.addEventListener('keydown', gridViewKeyPress)
            },
            beforedestroy: function (self, eOpts) {
                if (gridViewDom) {
                    gridViewDom.removeEventListener('keydown', gridViewKeyPress)
                }
            },
            afterRowAdd: function (store, records, index) {
                if (!vcl.loading && grid.manualing === true && records.length > 0) {
                    var rec = records[0]
                    var self = grid
                    var dataInfo = {
                        tableIndex: tableIndex,
                        dataRow: rec,
                        curGrid: self
                    }
                    vcl.vclHandler(self, {libEventType: LibEventTypeEnum.AddRow, dataInfo: dataInfo})
                }
            },
            afterRowDelete: function (store, records, index) {
                if (!vcl.loading && grid.manualing === true && records.length > 0) {
                    var rec = records[0];
                    var self = grid;
                    var dataInfo = {
                        tableIndex: tableIndex,
                        dataRow: rec,
                        curGrid: self
                    };
                    if (rec.children) {
                        rec.children.eachKey(function (key, item, index, len) {
                            vcl.dataSet.getTable(key).remove(item);
                        }, this)
                    }
                    vcl.vclHandler(self, {libEventType: LibEventTypeEnum.DeleteRow, dataInfo: dataInfo});
                }
            }
    }
}
})
store.ownGrid = grid
return grid
},
callSubBill: function (curVcl, curRow, curGrid, index, parentFieldName) {
    var vcl = curVcl;
    var key = '';
    var pks = vcl.dataSet.getTable(curGrid.tableIndex).Pks;
    for (var i = 0; i < pks.length; i++) {
        key += curRow.get(pks[i]);
    }
    key = '_' + key;

    var subBill = vcl.tpl.Layout.SubBill[index];
    var subPanel = Ax.tpl.GridManager.createGrid({
        vcl: vcl,
        parentRow: curRow,
        tableIndex: index,
        curRange: subBill,
        parentGrid: curGrid,
        parentFieldName: parentFieldName
    });
    if (vcl.subPanels[index]) {
        //subPanel.getStore().ownGrid.headerCt = vcl.subPanels[index];
    }
    else {
        vcl.subPanels[index] = {};

    }
    //if (vcl.dataSetSubTableOwnGrid[index]) {
    //    subPanel.getStore().ownGrid= vcl.dataSetSubTableOwnGrid[index];
    //}
    //else {
    //    vcl.dataSetSubTableOwnGrid[index] = new Object();
    //}
    var subWin = Ext.create('Ext.window.Window', {
        title: subBill.DisplayName,
        id: key,
        autoScroll: true,
        width: 900,
        height: 400,
        layout: 'fit',
        constrainHeader: true,
        minimizable: true,
        maximizable: true,
        items: subPanel,
        isSubWin: true,
        modal: true,
        vcl: vcl,
        listeners: {
            close: function (self, eOpts) {
                //vcl.subPanels[index]= subPanel.getStore().ownGrid.headerCt;
                //vcl.subPanels[index].length = 0;
                //for (var a = 0; a < subPanel.getStore().ownGrid.headerCt.items.items.length; a++) {
                //    vcl.subPanels[index].push(subPanel.getStore().ownGrid.headerCt.items.items);
                //}
                vcl.vclHandler(self, {libEventType: LibEventTypeEnum.FormClosed, dataInfo: {tableIndex: index}});
                subPanel.getStore().ownGrid = undefined;
                subPanel.getStore().clearFilter();
            },
            beforeclose: function (self, eOpts) {
                var gridScheme = {GridFields: []};
                var gridPanel = subPanel;
                var columns = gridPanel.headerCt.items.items;

                function buildBandCol (bandColumn, list) {
                    if (bandColumn.items.items.length > 0) {
                        var subList = [];
                        list.push({Header: bandColumn.text, BandFields: subList});
                        for (var r = 0; r < bandColumn.items.items.length; r++) {
                            if (bandColumn.items.items[r].hidden === true)
                                continue;
                            buildBandCol(bandColumn.items.items[r], subList);
                        }
                    }
                    else {
                        list.push({Field: {Name: bandColumn.dataIndex, Width: bandColumn.width}});
                    }
                }

                for (var l = 0; l < columns.length; l++) {
                    if (columns.xtype == "rownumberer")// || columns[l].hidden === true
                        continue;
                    buildBandCol(columns[l], gridScheme.GridFields);
                }
                //var aaaa = vcl.invorkBcf('GetViewTemplate');

            },
            beforeshow: function (self, eOpts) {
                //触发vcl的自定义方法 Zhangkj 20170321
                if (vcl.beforeSubWinGridShow)
                    vcl.beforeSubWinGridShow(index, subPanel);
            }
        }
    });
    subWin.show();
    DesktopApp.ActiveWindow = vcl.winId;
}
};

