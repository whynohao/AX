comBomView = function () {
    Ax.tpl.LibBillTpl.apply(this, arguments);
    if (this.vcl.funcView.containsKey("default")) {
        this.vcl.funcView.get("default").name = "onReady";
    }
    //只有第一列的双击才可以打开
    this.canExpend = true;
}
var proto = comBomView.prototype = Object.create(Ax.tpl.LibBillTpl.prototype);
proto.constructor = comBomView;
proto.onReady = function (billAction, curPks, isF4, lookVersionObj, changeView) {
    var me = this;
    var vcl = this.vcl;
    vcl.forms = [];
    if (changeView !== true) {
        vcl.billAction = billAction;
        if (curPks)
            vcl.currentPk = curPks;
    }
    if (vcl.dataSet.dataList && vcl.dataSet.dataList[1]) {
        vcl.dataSet.getTable(1).ownGrid = null;
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
            if (changeView !== true) {
                vcl.browseTo(vcl.currentPk);
            }
            toolBarAction = Ax.utils.LibToolBarBuilder.createBillAction(vcl, 0, isF4);
        }
    }
    //var progId = vcl.progId;
    var store = vcl.dataSet.getTable(0);

    var panel = Ext.create('Ext.form.Panel', {
        border: false,
        tableIndex: 0,
        margin: '4 2 4 2',
        items: Ext.decode(vcl.tpl.Layout.HeaderRange.Renderer)
    });
    vcl.forms.push(panel);
    panel.loadRecord(store.data.items[0]);

    var inputAnchor = '100% 100%';
    if (vcl.tpl.Layout.ButtonRange != null) {
        inputAnchor = '100% 100%';
        funPanel = Ext.create('Ext.panel.Panel', {
            border: false,
            style: 'border-top: 1px solid black',
            layout: { type: 'hbox', align: 'stretch' },
            defaults: {
                margin: '4 4'
            },
            items: Ext.decode(vcl.tpl.Layout.ButtonRange.Renderer)
        });
    }


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

    //加载grid明细
    function addGridTab() {
        var tabRange = vcl.tpl.Layout.TabRange;
        if (tabRange.length > 0) {
            var tableIndex = 1;
            for (var i = 0; i < tabRange.length; i++) {
                if (tabRange[i].BlockType == BlockTypeEnum.Grid) {
                    var grid = Ax.tpl.GridManager.createGrid({
                        vcl: vcl,
                        parentRow: vcl.dataSet.getTable(0).data.items[0],
                        tableIndex: vcl.dataSet.tableMap.get(tabRange[i].Store),
                        curRange: tabRange[i]
                    });
                    addTab(grid, tabRange[i].DisplayName);
                }
            }
        }
    }

    var bodyPanel;
    if (vcl.tpl.Layout.GridRange != null) {
        bodyPanel = Ext.create('Ext.panel.Panel', {
            items: me.createTree.call(me, {
                curRange: vcl.tpl.Layout.GridRange,
                title: vcl.tpl.Layout.GridRange.DisplayName
            }),
            layout: "fit",
            border: false,
            region: "center"
        });
    };
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
    addTab(bodyPanel, "物料清单");
    addGridTab();
    addFixTab(this);
    var inputAnchor = '100% 100%';
    var sidePanel = Ext.create('Ext.panel.Panel', {
        anchor: inputAnchor,
        layout: { type: 'vbox', align: "stretch" },
        headerPosition: "bottom",
        border: false,
        items: [panel, funPanel, tabPanel],
        collapsible: false,
        splitterResize: false,
        collapseMode: 'mini',
        region: 'north'
    });

    var mainWidth = document.body.clientWidth > 1100 ? document.body.clientWidth - 27 : 1100;
    var mainPanel = Ext.create('Ext.panel.Panel', {
        width: mainWidth,
        height: document.body.clientHeight - 395,
        layout: { type: 'anchor' },
        items: [sidePanel],
        border: false,
        tbar: Ax.utils.LibToolBarBuilder.createToolBar(toolBarAction)
    });
    return mainPanel;
};
proto.createTree = function (gridInfo) {
    //创建树状图模型
    var me = this;
    var vcl = me.vcl;
    var destColumns = Ext.decode(vcl.tpl.Layout.GridRange.Renderer);
    var treeWidth = 'auto';
    if (destColumns[0].dataIndex == "NODENAME") {
        var treeWidth = destColumns[0].width;
        destColumns.shift();
    }
    var treeColumn = {
        text: '树形结构',
        xtype: 'treecolumn',
        dataIndex: 'NODENAME',
        width: treeWidth
    };
    destColumns.splice(0, 0, treeColumn);
    var curRange = gridInfo.curRange, title = gridInfo.title;
    var tableDetail = vcl.tpl.Tables[vcl.dataSet.getTable(1).Name];
    var masterRow = vcl.dataSet.getTable(0).data.items[0];
    var cellEditing = Ext.create('Ext.grid.plugin.CellEditing', {
        clicksToEdit: 1
    });
    //数据加载
    function GetLoading() {
        var modelName = "treeModel";
        var modelType = Ext.data.Model.schema.getEntity(modelName);
        if (!modelType) {
            Ext.define('treeModel', {
                extend: 'Ext.data.Model',
                idProperty: 'postid',
            });
        }
        var obj = vcl.GetTreeStoreData(true, vcl.dataSet.getTable(1), 0, 0, masterRow);
        var store = Ext.create('Ext.data.TreeStore', {
            model: modelName,
            root: obj[0]
        })
        return store;
    }
    var treePanel = Ext.create('Ext.tree.Panel', {
        title: title,
        header: false,
        height: document.body.clientHeight - 325,
        renderTo: Ext.getBody(),
        store: GetLoading(),
        defaults: { width: 'auto', forceFit: false, sortable: true },//forceFit设置为false才不会出现遮蔽文本框的问题
        width: '100%',
        tableDetail: tableDetail,
        selModel: { mode: 'MULTI' },
        id: vcl.winId + vcl.dataSet.getTable(1).Name + 'treepanel',
        tableIndex: 1,
        columns: destColumns,
        plugins: [cellEditing
        ],
        columns: {
            items: destColumns,
        },
        viewConfig: {
            plugins: {
                ptype: 'treeviewdragdrop',
                dragText: '{0} 选中节点',
                allowContainerDrop: true,
                allowParentInsert: true,
                containerScroll: true,
                sortOnDrop: true
            },
            listeners: {
                beforedrop: function (node, data, overModel, dropPosition, dropHandler, eOpts) {
                    if (vcl.isEdit === false) {
                        Ext.Msg.alert("提示", "修改状态下才可以移动节点!");
                        return false;
                    }
                },
                drop: function (node, data, overModel, dropPosition, eOpts) {
                    function setNodeLevel(node) {
                        if (node.childNodes.length > 0) {
                            var childNodes = node.childNodes;
                            for (var i = 0; i < childNodes.length; i++) {
                                childNodes[i].set(level, node.get(level) + 1);
                                var bindingRow = childNodes[i].bindingRow;
                                if (bindingRow) {
                                    bindingRow.set(level, childNodes[i].get(level) + 1);
                                }
                                else {
                                    for (var j = 0; j < items.length; j++) {
                                        if (items[j].get(rowId) == childNodes[i].get(rowId)) {
                                            items[j].set(level, childNodes[i].get(level));
                                            break;
                                        }
                                    }
                                }
                                setNodeLevel(childNodes[i]);
                            }
                        }
                    }
                    var items = vcl.dataSet.getTable(1).data.items;
                    var parentRowId = "PARENTROWID", rowId = "ROW_ID", level = "BOMLEVEL", orderNum = "ORDERNUM";
                    //改变树形以及ax数据源的父行标识字段
                    for (var i = 0; i < data.records.length; i++) {
                        var currNode = data.records[i];
                        var parentNode = currNode.parentNode;
                        var sameLevelNodes = parentNode.childNodes;
                        if (currNode.get(parentRowId) != parentNode.get(rowId)) {
                            currNode.set(parentRowId, parentNode.get(rowId));
                            if (currNode.bindingRow) {
                                currNode.bindingRow.set(parentRowId, parentNode.get(rowId))
                            }
                            else {
                                for (var j = 0; j < items.length; j++) {
                                    if (items[j].data[rowId] == currNode.get(rowId)) {
                                        items[j].set(parentRowId, currNode.get(parentRowId));
                                        break;
                                    }
                                }
                            }
                        }
                        //改变树形以及ax数据源的层级、序号字段
                        for (var k = 0; k < sameLevelNodes.length; k++) {
                            sameLevelNodes[k].set(level, parentNode.get(level) + 1);
                            sameLevelNodes[k].set(orderNum, k + 1);
                            var bindingRow = sameLevelNodes[k].bindingRow;
                            if (bindingRow) {
                                bindingRow.set(level, sameLevelNodes[k].get(level));
                                bindingRow.set(orderNum, sameLevelNodes[k].get(orderNum));
                            }
                            else {
                                for (var l = 0; l < items.length; l++) {
                                    if (items[l].get(rowId) == sameLevelNodes[k].get(rowId)) {
                                        items[l].set(level, sameLevelNodes[k].get(level));
                                        items[l].set(orderNum, k + 1);
                                        break;
                                    }
                                }
                            }
                            setNodeLevel(sameLevelNodes[k]);
                        }
                    }
                }

            },
            getRowClass: function (record, rowIndex, rowParams, store) {
                var retClass = "bom_treeBackground";
                if (record.get("MATCHTYPE") == 1) {
                    retClass = "bom_Semi-finishedProductMatchClass";
                }
                else if (record.get("MATCHTYPE") == 2) {
                    retClass = "bom_MaterialMatchClass";
                }
                else if (record.get("ISAUTOMATCHMAT")) {
                    retClass = "bom_AutoMaterialClass";
                }
                else if (record.get("MATERIALTYLE") == 3) {
                    retClass = "bom_AutoVirtualpartClass";
                }
                else if (record.get("MATERIALTYLE") == 2) {
                    retClass = "bom_AutoMatstyleClass";
                }
                return retClass;
            }
        },
        listeners: {
            beforeedit: function (obj, e) {
                if (vcl.isEdit === false)
                    return false;
            },
            edit: function (obj, e) {
                var me = this;
                var dataInfo = {
                    cancel: false,
                    value: e.value,
                    oldValue: e.originalValue,
                    fieldName: e.field,
                    tableIndex: 1,
                    dataRow: e.record,
                    curForm: null,
                    curGrid: me
                };
                if (e.field == "ISPROCESS") {
                    var self = Ext.getCmp(e.field + 1 + '_' + vcl.winId);
                    vcl.vclHandler(self, { libEventType: LibEventTypeEnum.Validated, dataInfo: dataInfo });
                }

                if (dataInfo.value != dataInfo.oldValue) {
                    var self = Ext.getCmp(e.field + 1 + '_' + vcl.winId);
                    vcl.vclHandler(self, { libEventType: LibEventTypeEnum.Validated, dataInfo: dataInfo });
                }
            },
            celldblclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                vcl.canExpend = true;
                if (cellIndex != 0) {
                    vcl.canExpend = false;
                }
                var dataInfo = Ax.Control.LibDataInfo.getDataInfoForGrid(self, td, cellIndex, record, tr, rowIndex, e);
                vcl.vclHandler(self, { libEventType: LibEventTypeEnum.ColumnDbClick, dataInfo: dataInfo });
                if (!dataInfo.cancel) {
                    var subIndex = treePanel.tableDetail.SubTableMap[dataInfo.fieldName];
                    if (subIndex) {
                        function getBindingRow() {
                            var items = vcl.dataSet.getTable(1).data.items;
                            var bindingRow;
                            for (var i = 0; i < items.length; i++) {
                                if (record.get("ROW_ID") == items[i].get("ROW_ID")) {
                                    bindingRow = items[i];
                                    break;
                                }
                            }
                            return bindingRow;
                        }
                        var dataRow = dataInfo.dataRow.bindingRow || getBindingRow() || record;
                        Ax.tpl.GridManager.callSubBill(vcl, dataRow, dataInfo.curGrid, subIndex, dataInfo.fieldName);
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
            itemcontextmenu: function (view, record, item, index, e, eOpts) {
                e.preventDefault();
                e.stopEvent();
                var currNode = record;
                if (vcl.isEdit) {
                    var menu = new Ext.menu.Menu({
                        items: [{
                            text: "新增子节点", iconCls: 'gridAdd',
                            handler: function () {
                                masterRow.store = vcl.dataSet.getTable(1);
                                var newRow = vcl.addRow(masterRow, 1);
                                newRow.set("PARENTROWID", record.get("ROW_ID"));
                                var bomLevel;
                                if (record.get("BOMLEVEL") == undefined) {
                                    bomLevel = 1;
                                }
                                else {
                                    bomLevel = record.get("BOMLEVEL") + 1;
                                }
                                newRow.set("BOMLEVEL", bomLevel);
                                newRow.set("ORDERNUM", record.childNodes.length + 1);

                                var newNode = {};
                                for (var fieldName in newRow.data) {
                                    newNode[fieldName] = newRow.get(fieldName);
                                }
                                newNode["expanded"] = false;
                                newNode["leaf"] = false;

                                record.data['expanded'] = true;
                                newNode = record.appendChild(newNode);
                                newNode.bindingRow = newRow;
                                var data = currNode.childNodes;//当前层级的所有数据
                                for (var i = 0; i < data.length; i++) {
                                    var foundRow = vcl.dataSet.FindRow(1, data[i].get("ROW_ID"));
                                    if (foundRow) {
                                        foundRow.set("ORDERNUM", i + 1);
                                    }
                                    data[i].set("ORDERNUM", i + 1);
                                }
                            }
                        }, {
                            text: "删除子节点", iconCls: 'gridDelete',
                            handler: function () {
                                function getBindingRow() {
                                    var bindingRow = vcl.dataSet.FindRow(1, currNode.get("ROW_ID"));
                                    return bindingRow;
                                }
                                var bindingRow = getBindingRow();
                                var deleteRows = new Array();
                                deleteRows.push(bindingRow);
                                vcl.GetModelAndSonCollection(vcl.dataSet.getTable(1), bindingRow.get("ROW_ID"), deleteRows);
                                for (var j = deleteRows.length - 1; j >= 0; j--) {
                                    var record = deleteRows[j];
                                    items = vcl.dataSet.getTable(2).data.items;
                                    for (var i = items.length - 1; i >= 0; i--) {
                                        if (record.get("ROW_ID") == items[i].get("PARENTROWID")) {
                                            //vcl.dataSet.getTable(2).remove(items[i]);
                                            vcl.deleteRow(2, items[i]);
                                        }
                                    }
                                    vcl.deleteRow(1, record);
                                }
                                var data = currNode.parentNode.childNodes;//当前层级的所有数据
                                currNode.remove();
                                for (var i = 0; i < data.length; i++) {
                                    var foundRow = vcl.dataSet.FindRow(1, data[i].get("ROW_ID"));
                                    if (foundRow) {
                                        foundRow.set("ORDERNUM", i + 1);
                                    }
                                    data[i].set("ORDERNUM", i + 1);
                                }
                            }
                        }, {
                            text: "复制节点", iconCls: 'gridCopy',
                            handler: function () {
                                function getBindingRow() {
                                    var bindingRow = vcl.dataSet.FindRow(1, currNode.get("ROW_ID"));
                                    return bindingRow;
                                }
                                if (currNode.data["ROW_ID"] == 0) {
                                    Ext.Msg.alert("提示", "根节点不能复制!");
                                } else {
                                    var bindingRow = currNode.bindingRow || getBindingRow();
                                    //首先获得复制节点当前的对象，然后去找该行的子行以及子子行
                                    vcl.fillCopyData(bindingRow, null, true);
                                    if (vcl.copyData) {
                                        DesktopApp.copyData = vcl.copyData;
                                    }
                                }
                            }
                        }, {
                            text: "粘贴节点", iconCls: 'gridPaste',
                            handler: function () {
                                vcl.pasteData(currNode, null, null, true);
                            }
                        }, {
                            text: "展开节点", iconCls: 'gridAdd',
                            handler: function () {
                                record.expand(false);
                                expendTreeForLevel.call(this, record.childNodes);

                                //展开到某一层级
                                function expendTreeForLevel(record) {
                                    for (var i = 0; i < record.length; i++) {
                                        record[i].expand(false);
                                        if (record[i].childNodes.length > 0) {
                                            expendTreeForLevel(record[i].childNodes);
                                        }
                                    }
                                }
                            }
                        }, {
                            text: "收缩节点", iconCls: 'gridDelete',
                            handler: function () {
                                record.collapse();
                                expendTreeForLevel.call(this, record.childNodes);

                                //展开到某一层级
                                function expendTreeForLevel(record) {
                                    for (var i = 0; i < record.length; i++) {
                                        record[i].collapse();
                                        if (record[i].childNodes.length > 0) {
                                            expendTreeForLevel(record[i].childNodes);
                                        }
                                    }
                                }
                            }
                        }, {
                            text: "打开引用BOM", iconCls: 'gridOpen',
                            handler: function () {
                                var struecBom = vcl.getBomID(currNode.data["MATERIALID"]);
                                if (struecBom[0] != undefined) {
                                    var entryParam = '{"ParamStore":{"BOMTYPEID":"' + struecBom[1] + '"}}';
                                    var curPks = [];
                                    curPks.push(struecBom[0]);
                                    Ax.utils.LibVclSystemUtils.openBill('com.Bom', 0, struecBom[2], BillActionEnum.Browse, Ext.decode(entryParam), curPks);
                                } else {
                                    Ext.Msg.alert("提示", "当前物料不是虚拟件或通用件，无法打开");
                                }
                            }
                        }]
                    }).showAt(e.getXY());
                } else {
                    var menu = new Ext.menu.Menu({
                        items: [
                            {
                                text: "展开节点", iconCls: 'gridAdd',
                                handler: function () {
                                    record.expand(false);
                                    expendTreeForLevel.call(this, record.childNodes);

                                    //展开到某一层级
                                    function expendTreeForLevel(record) {
                                        for (var i = 0; i < record.length; i++) {
                                            record[i].expand(false);
                                            if (record[i].childNodes.length > 0) {
                                                expendTreeForLevel(record[i].childNodes);
                                            }
                                        }
                                    }
                                }
                            }, {
                                text: "收缩节点", iconCls: 'gridDelete',
                                handler: function () {
                                    record.collapse();
                                    expendTreeForLevel.call(this, record.childNodes);

                                    //展开到某一层级
                                    function expendTreeForLevel(record) {
                                        for (var i = 0; i < record.length; i++) {
                                            record[i].collapse();
                                            if (record[i].childNodes.length > 0) {
                                                expendTreeForLevel(record[i].childNodes);
                                            }
                                        }
                                    }
                                }
                            }, {
                                text: "打开引用BOM", iconCls: 'gridOpen',
                                handler: function () {
                                    var struecBom = vcl.getSaleBomID(currNode.data["MATERIALID"]);
                                    if (struecBom[0] != undefined) {
                                        var entryParam = '{"ParamStore":{"STRUCTBOMTYPEID":"' + struecBom[1] + '"}}';
                                        var curPks = [];
                                        curPks.push(struecBom[0]);
                                        Ax.utils.LibVclSystemUtils.openBill('com.StructBom', 0, struecBom[2], BillActionEnum.Browse, Ext.decode(entryParam), curPks);
                                    } else {
                                        Ext.Msg.alert("提示", "当前物料不是虚拟件或通用件，无法打开");
                                    }
                                }
                            }]
                    }).showAt(e.getXY());
                }
            },
            beforeitemdblclick: function () {
                return vcl.canExpend;
            },
            validateedit: function (editor, context, eOpts) {
                var me = this;
                var dataInfo = {
                    cancel: false,
                    value: context.value,
                    oldValue: context.originalValue,
                    fieldName: context.field,
                    tableIndex: 1,
                    dataRow: context.record,
                    curForm: null,
                    curGrid: me
                };
                if (dataInfo.value != dataInfo.oldValue) {
                    var self = Ext.getCmp(context.field + 1 + '_' + vcl.winId);
                    vcl.vclHandler(self, { libEventType: LibEventTypeEnum.Validating, dataInfo: dataInfo });
                }
                return !dataInfo.cancel;
            }
        }
    });
    vcl.tree = treePanel;
    vcl.firstLoad = false;
    return treePanel;
}