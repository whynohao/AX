comTestStructBomRptView = function () {
    Ax.tpl.LibDataFuncTpl.apply(this, arguments);
    if (this.vcl.funcView.containsKey("default")) {
        this.vcl.funcView.get("default").name = "onReady";
    }
};
var proto = comTestStructBomRptView.prototype = Object.create(Ax.tpl.LibDataFuncTpl.prototype);
proto.constructor = comTestStructBomRptView;
proto.onReady = function (billAction, curPks, isF4, lookVersionObj, changeView) {
    var me = this;
    var vcl = this.vcl;
    vcl.forms = [];
    vcl.openFunc();
    var store = vcl.dataSet.getTable(0);

    var panel = Ext.create('Ext.form.Panel', {
        border: false,
        tableIndex: 0,
        margin: '6 2 6 2',
        items: Ext.decode(vcl.tpl.Layout.HeaderRange.Renderer)
    });
    vcl.forms.push(panel);
    panel.loadRecord(store.data.items[0]);

    var tabPanel = Ext.widget('tabpanel', {
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
                    margin: '6 0 6 2',
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
    function createPrintBtn() {
        var btnRefresh = Ext.create(Ext.Action, {
            text: '打印',
            handler: function () {
                var headTable = vcl.dataSet.getTable(0).data.items[0];
                var bodyTable = vcl.dataSet.getTable(1);
                vcl.print.call(this, headTable, bodyTable);
            }
        });
        return btnRefresh;
    }
    var bodyPanel;
    if (vcl.tpl.Layout.GridRange != null) {
        bodyPanel = Ext.create('Ext.panel.Panel', {
            items: this.createTree.call(this, {
                curRange: vcl.tpl.Layout.GridRange,
                title: vcl.tpl.Layout.GridRange.DisplayName
            }),
            layout: "fit",
            border: false,
            region: "center",
            tbar: [
                createPrintBtn.call()
            ]
        });

    };

    var inputAnchor = '100% 100%';
    var sidePanel = Ext.create('Ext.panel.Panel', {
        layout: { type: 'vbox', align: "stretch" },
        headerPosition: "bottom",
        border: false,
        items: [panel, tabPanel],
        collapsible: true,
        splitterResize: false,
        collapseMode: 'mini',
        region: 'north'
    });
    var inputPanel = Ext.create('Ext.panel.Panel', {
        anchor: inputAnchor,
        layout: { type: 'border' },
        items: [sidePanel, bodyPanel],
        border: false
    });

    var toolBarAction = Ax.utils.LibToolBarBuilder.createDataFuncAction(vcl, 0);
    var mainWidth = document.body.clientWidth > 1210 ? document.body.clientWidth - 27 : 1210;
    var mainPanel = Ext.create('Ext.panel.Panel', {
        width: mainWidth,
        height: document.body.clientHeight - 80,
        layout: { type: 'anchor' },
        items: [inputPanel],
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
    if (destColumns[0].dataIndex == "MATERIALNAME") {
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
        var obj;
        var store = Ext.create('Ext.data.TreeStore', {
            model: modelName,
            root: obj,
        });

        return store;
    }
    var treePanel = Ext.create('Ext.tree.Panel', {
        title: title,
        header: false,
        //height: 400,
        animate: true,
        renderTo: Ext.getBody(),
        iconCls: 'gridAdd',
        store: GetLoading(),
        defaults: { width: 'auto', forceFit: false, sortable: true },
        width: '100%',
        tableDetail: tableDetail,
        selModel: { mode: 'MULTI' },
        id: vcl.winId + vcl.dataSet.getTable(1).Name + 'treepanel',
        tableIndex: 1,
        parentRow: masterRow,
        columns: destColumns,
        plugins: [cellEditing
        ],
        viewConfig: {
            plugins: {
                ptype: 'treeviewdragdrop',
                dragText: '{0} 选中节点',
                allowContainerDrop: true,
                allowParentInsert: true,
                containerScroll: true,
                sortOnDrop: true,

            }
        },
        listeners: {
            beforeedit: function (self) {
                if (vcl.isEdit === false)
                    return false;
            },
            celldblclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
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
            'itemcontextmenu': function (view, record, item, index, e, eOpts) {
                e.preventDefault();
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
                       }
                    ]
                }).showAt(e.getXY());
            }
        }
    });
    vcl.tree = treePanel;
    vcl.firstLoad = false;
    return treePanel;
}

