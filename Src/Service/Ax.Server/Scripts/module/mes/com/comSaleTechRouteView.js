comSaleTechRouteView = function () {
    Ax.tpl.LibBillTpl.apply(this, arguments);
    if (this.vcl.funcView.containsKey("default")) {
        this.vcl.funcView.get("default").name = "onReady";
    }
}
var proto = comSaleTechRouteView.prototype = Object.create(Ax.tpl.LibBillTpl.prototype);
proto.constructor = comSaleTechRouteView;
proto.onReady = function (billAction, curPks, isF4, lookVersionObj, changeView) {
    var me = this;
    var vcl = this.vcl;
    vcl.forms = [];
    if (changeView !== true) {
        vcl.billAction = billAction;
        if (curPks)
            vcl.currentPk = curPks;
    }
    var toolBarAction;
    if (lookVersionObj !== undefined) {
        vcl.browseToVersion(lookVersionObj.InternalId, lookVersionObj.VersionTime);
    } else {
        if (vcl.billAction == BillActionEnum.AddNew) {
            if (changeView !== true) {
                if (!vcl.addNew())
                    return;
                vcl.isEdit = true;
            }
            toolBarAction = Ax.utils.LibToolBarBuilder.createBillAction(vcl, 0, isF4);
            if (changeView !== true)
                toolBarAction[0].execute({ noOpen: true });
        } else if (vcl.billAction == BillActionEnum.Edit) {
            if (changeView !== true)
                vcl.browseTo(vcl.currentPk);
            toolBarAction = Ax.utils.LibToolBarBuilder.createBillAction(vcl, 0, isF4);
            if (changeView !== true)
                toolBarAction[1].execute();
        } else {
            if (changeView !== true)
                vcl.browseTo(vcl.currentPk);
            toolBarAction = Ax.utils.LibToolBarBuilder.createBillAction(vcl, 0, isF4);
        }
    }
    var progId = vcl.progId;
    var store = vcl.dataSet.getTable(0);

    var panel = Ext.create('Ext.form.Panel', {
        border: false,
        tableIndex: 0,
        margin: '4 2 4 2',
        items: Ext.decode(vcl.tpl.Layout.HeaderRange.Renderer)
    });
    vcl.forms.push(panel);
    panel.loadRecord(store.data.items[0]);

    var tabPanel = Ext.create('Ext.tab.Panel', {
        border: false,
        activeTab: 0,
        flex: vcl.tpl.Layout.GridRange == null ? 1 : undefined,
        defaults: {
            bodyPadding: 0
        }
    });
    function addTab(panel, displayName) {
        tabPanel.add({
            iconCls: 'tabs',
            layout: 'fit',
            items: panel,
            title: displayName
        });
    }

    var tabRange = vcl.tpl.Layout.TabRange;
    if (tabRange.length > 0) {
        var tableIndex = 1;
        for (var i = 0; i < tabRange.length; i++) {
            if (tabRange[i].BlockType == BlockTypeEnum.ControlGroup) {
                var tempPanel = Ext.create('Ext.form.Panel', {
                    border: false,
                    tableIndex: 0,
                    margin: '4 0 4 2',
                    defaultType: 'textfield',
                    items: Ext.decode(tabRange[i].Renderer)
                });
                tempPanel.loadRecord(store.data.items[0]);
                vcl.forms.push(tempPanel);
                addTab(tempPanel, tabRange[i].DisplayName);
            } else if (tabRange[i].BlockType == BlockTypeEnum.Grid) {
                var grid = Ax.tpl.GridManager.createGrid({
                    vcl: vcl,
                    parentRow: vcl.dataSet.getTable(0).data.items[0],
                    tableIndex: vcl.dataSet.tableMap.get(tabRange[i].Store),
                    curRange: tabRange[i]
                });
                addTab(grid, tabRange[i].DisplayName);
            }
        }
    };

    //有效期
    function addValidityTab(me) {
        var tempPanel = Ext.create('Ext.form.Panel', {
            border: false,
            tableIndex: 0,
            margin: '4 0 4 2',
            defaultType: 'textfield',
            items: {
                xtype: 'container',
                layout: { type: 'table', columns: 4 },
                style: { marginTop: '6px', marginBottom: '6px' },
                defaults: { labelAlign: 'right' },
                defaultType: 'libTextField',
                items: [{
                    xtype: 'libDateField',
                    height: 24,
                    width: 300,
                    colspan: 1,
                    fieldLabel: '有效期从',
                    name: 'VALIDITYSTARTDATE',
                    tableIndex: 0
                }, {
                    xtype: 'libDateField',
                    height: 24,
                    width: 300,
                    colspan: 1,
                    fieldLabel: '有效期至',
                    name: 'VALIDITYENDDATE',
                    tableIndex: 0
                }, {
                    xtype: 'libCheckboxField',
                    height: 24,
                    width: 300,
                    colspan: 1,
                    readOnly: true,
                    fieldLabel: '(是否有效)',
                    name: 'ISVALIDITY',
                    tableIndex: 0
                }]
            }
        });
        tempPanel.loadRecord(store.data.items[0]);
        me.vcl.forms.push(tempPanel);
        addTab(tempPanel, '有效期');
    };
    //加入系统页签和备注
    function addFixTab(me) {
        var items;
        if (me.vcl.billType == BillTypeEnum.Master) {
            addValidityTab(me);
            items = {
                xtype: 'container',
                layout: { type: 'table', columns: 4 },
                style: { marginTop: '6px', marginBottom: '6px' },
                defaults: { labelAlign: 'right', readOnly: true, width: 300 },
                defaultType: 'libTextField',
                items: [{
                    xtype: 'libSearchfield',
                    labelAlign: 'right',
                    fieldLabel: '创建人',
                    relIndex: 0,
                    relSource: { 'com.Person': '' },
                    relName: 'CREATORNAME',
                    name: 'CREATORID'
                }, {
                    xtype: 'libDatetimefield',
                    labelAlign: 'right',
                    fieldLabel: '创建时间',
                    name: 'CREATETIME'
                }, {
                    xtype: 'libSearchfield',
                    labelAlign: 'right',
                    fieldLabel: '审核人',
                    relSource: { 'com.Person': '' },
                    relIndex: 0,
                    relName: 'APPROVRNAME',
                    name: 'APPROVRID'
                }, {
                    xtype: 'libDatetimefield',
                    labelAlign: 'right',
                    fieldLabel: '审核时间',
                    name: 'APPROVALTIME'
                }, {
                    xtype: 'libSearchfield',
                    labelAlign: 'right',
                    fieldLabel: '最后修改人',
                    relSource: { 'com.Person': '' },
                    relIndex: 0,
                    relName: 'LASTUPDATENAME',
                    name: 'LASTUPDATEID'
                }, {
                    xtype: 'libDatetimefield',
                    labelAlign: 'right',
                    fieldLabel: '最后修改时间',
                    name: 'LASTUPDATETIME'
                }]
            };
        } else {
            items = {
                xtype: 'container',
                layout: { type: 'table', columns: 4 },
                style: { marginTop: '6px', marginBottom: '6px' },
                defaults: { labelAlign: 'right', readOnly: true, width: 300 },
                defaultType: 'libTextField',
                items: [{
                    xtype: 'libSearchfield',
                    labelAlign: 'right',
                    fieldLabel: '创建人',
                    relSource: { 'com.Person': '' },
                    relIndex: 0,
                    relName: 'CREATORNAME',
                    name: 'CREATORID'
                }, {
                    xtype: 'libDatetimefield',
                    labelAlign: 'right',
                    fieldLabel: '创建时间',
                    name: 'CREATETIME'
                }, {
                    xtype: 'libSearchfield',
                    labelAlign: 'right',
                    fieldLabel: '审核人',
                    relSource: { 'com.Person': '' },
                    relIndex: 0,
                    relName: 'APPROVRNAME',
                    name: 'APPROVRID'
                }, {
                    xtype: 'libDatetimefield',
                    labelAlign: 'right',
                    fieldLabel: '审核时间',
                    name: 'APPROVALTIME'
                }, {
                    xtype: 'libSearchfield',
                    labelAlign: 'right',
                    fieldLabel: '最后修改人',
                    relSource: { 'com.Person': '' },
                    relIndex: 0,
                    relName: 'LASTUPDATENAME',
                    name: 'LASTUPDATEID'
                }, {
                    xtype: 'libDatetimefield',
                    labelAlign: 'right',
                    fieldLabel: '最后修改时间',
                    name: 'LASTUPDATETIME'
                }, {
                    xtype: 'libSearchfield',
                    labelAlign: 'right',
                    fieldLabel: '结案人',
                    relSource: { 'com.Person': '' },
                    relIndex: 0,
                    relName: 'ENDCASENAME',
                    name: 'ENDCASEID'
                }, {
                    xtype: 'libDatetimefield',
                    labelAlign: 'right',
                    fieldLabel: '结案时间',
                    name: 'ENDCASETIME'
                }]
            };
        }
        var tempPanel = Ext.create('Ext.form.Panel', {
            border: false,
            tableIndex: 0,
            margin: '4 0 4 2',
            defaultType: 'textfield',
            items: items
        });
        tempPanel.loadRecord(store.data.items[0]);
        me.vcl.forms.push(tempPanel);
        addTab(tempPanel, '制单信息');

        tempPanel = Ext.create('Ext.form.Panel', {
            border: false,
            tableIndex: 0,
            margin: '4 0 4 2',
            defaultType: 'textfield',
            items: {
                xtype: 'container', layout: 'fit',
                style: {
                    marginTop: '6px',
                    marginRight: '50px',
                    marginBottom: '6px'
                },
                items: {
                    xtype: 'textareafield',
                    labelAlign: 'right',
                    grow: true,
                    name: 'REMARK',
                    fieldLabel: '备注'
                }
            }
        });
        tempPanel.loadRecord(store.data.items[0]);
        me.vcl.forms.push(tempPanel);
        addTab(tempPanel, '备注');
    };
    addFixTab(this);

    //明细自定义视图
    var gridPanel;
    if (vcl.tpl.Layout.GridRange != null) {
        gridPanel = me.createGrid.call(me, {
            vcl: vcl,
            parentRow: vcl.dataSet.getTable(0).data.items[0],
            tableIndex: vcl.dataSet.tableMap.get(vcl.tpl.Layout.GridRange.Store),
            curRange: vcl.tpl.Layout.GridRange,
            height: 600,
            title: vcl.tpl.Layout.GridRange.DisplayName
        });
    };

    var funPanel;
    var inputAnchor = '100% 100%';
    if (vcl.tpl.Layout.ButtonRange != null) {
        inputAnchor = '100% 95%';
        funPanel = Ext.create('Ext.panel.Panel', {
            border: false,
            anchor: '100% 5%',
            layout: { type: 'hbox', align: 'stretch' },
            defaults: {
                margin: '2 10'
            },
            items: Ext.decode(vcl.tpl.Layout.ButtonRange.Renderer)
        });
    }

    var toolBarItems = [];
    if (toolBarAction) {
        for (var i = 0; i < toolBarAction.length; i++) {
            toolBarItems.push(Ext.create(Ext.button.Button, toolBarAction[i]));
        }
    }
    var inputPanel = Ext.create('Ext.panel.Panel', {
        anchor: inputAnchor,
        layout: { type: 'vbox', align: 'stretch' },
        items: [panel, tabPanel, gridPanel],
        border: false
    });
    var mainWidth = document.body.clientWidth > 1210 ? document.body.clientWidth - 27 : 1210;
    var mainPanel = Ext.create('Ext.panel.Panel', {
        width: mainWidth,
        height: document.body.clientHeight - 80,
        layout: { type: 'anchor' },
        items: [inputPanel, funPanel],
        border: false,
        tbar: Ax.utils.LibToolBarBuilder.createToolBar(toolBarAction)
    });
    return mainPanel;
};

//明细表自定义视图
proto.createGrid = function (gridInfo) {
    var curVcl = gridInfo.vcl,
        parentRow = gridInfo.parentRow,
        tableIndex = gridInfo.tableIndex,
        curRange = gridInfo.curRange,
        parentGrid = gridInfo.parentGrid,
        height = gridInfo.height,
        parentFieldName = gridInfo.parentFieldName, title = gridInfo.title, isEditGrid = gridInfo.isEditGrid;
    var vcl = curVcl;
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
    if (isEditGrid !== false) {
        var gridFuncItems = [{
            iconCls: 'gridAdd',
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
                    vcl.vclHandler(self, { libEventType: LibEventTypeEnum.BeforeAddRow, dataInfo: dataInfo });
                    if (!dataInfo.cancel) {
                        grid.manualing = true;
                        try {
                            vcl.addRowForGrid(grid);
                        } finally {
                            grid.manualing = false;
                        }
                    }
                }
            }
        }, {
            iconCls: 'gridDelete',
            scope: this,
            handler: function () {
                if (vcl.isEdit === false)
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
                    vcl.vclHandler(self, { libEventType: LibEventTypeEnum.BeforeDeleteRow, dataInfo: dataInfo });
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
        }];
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
        dockedItems = [{
            xtype: 'toolbar',
            items: gridFuncItems
        }];
    }
    var destColumns = Ext.decode(curRange.Renderer);
    //修改单个单元格样式
    for (var i = 0; i < curRange.FieldList.length; i++) {
        if (curRange.FieldList[i] == 'WORKSHOPSECTIONID') {
            destColumns[i].renderer = function (v, m) {
                if (v) {
                    v = v + ',' + m.record.data['WORKSHOPSECTIONNAME'];
                    if (m.record.data['DIFFERENCE']) {
                        var s = JSON.parse(m.record.data['DIFFERENCE']);
                        if (!s.Add) {
                            for (var str in s.Modify) {
                                if (s.Modify[str] == 'WORKSHOPSECTIONID') {
                                    v = '<span style="color:red;">' + v + '</span>';
                                    break;
                                }
                            }
                        }
                    }
                }
                return v;
            }
        }
        else if (curRange.FieldList[i] == 'WORKPROCESSID') {
            destColumns[i].renderer = function (v, m) {
                if (v) {
                    v = v + ',' + m.record.data['WORKPROCESSNAME'];
                    if (m.record.data['DIFFERENCE']) {
                        var s = JSON.parse(m.record.data['DIFFERENCE']);
                        if (!s.Add) {
                            for (var str in s.Modify) {
                                if (s.Modify[str] == 'WORKPROCESSID') {
                                    v = '<span style="color:red;">' + v + '</span>';
                                    break;
                                }
                            }
                        }
                    }
                }
                return v;
            }
        }
        else if (curRange.FieldList[i] == 'WORKPROCESSNO') {
            destColumns[i].renderer = function (v, m) {
                if (v) {
                    if (m.record.data['DIFFERENCE']) {
                        var s = JSON.parse(m.record.data['DIFFERENCE']);
                        if (!s.Add) {
                            for (var str in s.Modify) {
                                if (s.Modify[str] == 'WORKPROCESSNO') {
                                    v = '<span style="color:red;">' + v + '</span>';
                                    break;
                                }
                            }
                        }
                    }
                }
                return v;
            }
        }
        else if (curRange.FieldList[i] == 'WORKSTATIONCONFIGID') {
            destColumns[i].renderer = function (v, m) {
                if (v) {
                    if (m.record.data['DIFFERENCE']) {
                        var s = JSON.parse(m.record.data['DIFFERENCE']);
                        if (!s.Add) {
                            for (var str in s.Modify) {
                                if (s.Modify[str] == 'WORKSTATIONCONFIGID') {
                                    v = '<span style="color:red;">' + v + '</span>';
                                    break;
                                }
                            }
                        }
                    }
                }
                return v;
            }
        }
        else if (curRange.FieldList[i] == 'WORKPROCESSPARAM') {
            destColumns[i].renderer = function (v, m) {
                if (v) {
                    if (m.record.data['DIFFERENCE']) {
                        var s = JSON.parse(m.record.data['DIFFERENCE']);
                        if (!s.Add) {
                            for (var str in s.Modify) {
                                if (s.Modify[str] == 'WORKPROCESSPARAM') {
                                    v = '<span style="color:red;">' + v + '</span>';
                                    break;
                                }
                            }
                        }
                    }
                }
                return v;
            }
        }
    }

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
    var grid = Ext.create('Ext.grid.Panel', {
        border: false,
        collapsible: title === undefined ? false : true,
        title: title,
        margin: '6 2 2 2',
        plugins: [cellEditing,
            'gridfilters'
        ],
        flex: 1,
        dockedItems: dockedItems,
        tableDetail: tableDetail,
        store: store,
        selModel: { mode: 'MULTI' },
        id: vcl.winId + vcl.dataSet.getTable(tableIndex).Name + 'Grid',
        tableIndex: tableIndex,
        columns: destColumns,
        parentRow: parentRow,
        parentGrid: parentGrid,
        parentFieldName: parentFieldName,
        listeners: {
            beforeedit: function (self) {
                if (vcl.isEdit === false)
                    return false;
            },
            celldblclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                var dataInfo = Ax.Control.LibDataInfo.getDataInfoForGrid(self, td, cellIndex, record, tr, rowIndex, e);
                vcl.vclHandler(self, { libEventType: LibEventTypeEnum.ColumnDbClick, dataInfo: dataInfo });
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
            }
        },
        viewConfig: {
            getRowClass: function (record, rowIndex, rowParams, store) {
                //根据差异字段修改行背景颜色
                var bacClass = "green";
                if (record.data.DIFFERENCE) {
                    var c = JSON.parse(record.data.DIFFERENCE);
                    if (c.Add) {
                        bacClass = 'yellow';
                    }
                    else if (!c.Add && c.Modify.length > 0) {
                        bacClass = 'red';
                    }
                }
                return bacClass;
            },
            listeners: {
                afterRowAdd: function (store, records, index) {
                    if (!vcl.loading && grid.manualing === true && records.length > 0) {
                        var rec = records[0];
                        var self = grid;
                        var dataInfo = {
                            tableIndex: tableIndex,
                            dataRow: rec,
                            curGrid: self
                        };
                        vcl.vclHandler(self, { libEventType: LibEventTypeEnum.AddRow, dataInfo: dataInfo });
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
                        vcl.vclHandler(self, { libEventType: LibEventTypeEnum.DeleteRow, dataInfo: dataInfo });
                    }
                }
            }
        }
    });
    store.ownGrid = grid;
    return grid;
}


