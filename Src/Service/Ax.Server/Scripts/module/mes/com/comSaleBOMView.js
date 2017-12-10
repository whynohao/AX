comSaleBOMView = function () {
    Ax.tpl.LibBillTpl.apply(this, arguments);
    if (this.vcl.funcView.containsKey("default")) {
        this.vcl.funcView.get("default").name = "onReady";
    }
};
var proto = comSaleBOMView.prototype = Object.create(Ax.tpl.LibBillTpl.prototype);
proto.constructor = comSaleBOMView;
proto.onReady = function (billAction, curPks, isF4, lookVersionObj, changeView) {
    var me = this;
    var vcl = me.vcl;
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
        activeTab: 2,
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
            }
        }
    }
    var bodyPanel;
    var levelComBo;
    if (vcl.tpl.Layout.GridRange != null) {
        bodyPanel = Ext.create('Ext.panel.Panel', {
            items: this.createTree.call(this, {
                curRange: vcl.tpl.Layout.GridRange,
                title: vcl.tpl.Layout.GridRange.DisplayName
            }),
            layout: "fit",
            border: false,
            region: "center",
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
    addFixTab(this);

    var inputAnchor = '100% 100%';

    var btnRemoveDetail = Ext.create(Ext.Action, {
        text: '清空明细',
        handler: function () {
            success = vcl.removeDetail();
        }
    });
    var btnReCreateSaleBom = Ext.create(Ext.Action, {
        text: '重新分解',
        handler: function () {
            success = vcl.reCreateSaleBom();
        }
    });
    var btnChangeAttribute = Ext.create(Ext.Action, {
        text: '特征修改',
        handler: function () {
            success = vcl.changeAttribute();
        }
    });
    
    toolBarAction.push(btnRemoveDetail);
    toolBarAction.push(btnReCreateSaleBom);
    toolBarAction.push(btnChangeAttribute);
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
    var funPanel;
    var inputAnchor = '100% 100%';
    if (vcl.tpl.Layout.ButtonRange != null) {
        inputAnchor = '100% 90%';
        funPanel = Ext.create('Ext.panel.Panel', {
            border: false,
            anchor: '100% 10%',
            margin: '2 4',
            layout: { type: 'hbox', align: 'stretch' },
            defaults: {
                margin: '0 10'
            },
            items: Ext.decode(vcl.tpl.Layout.ButtonRange.Renderer)
        });
    }

    var inputPanel = Ext.create('Ext.panel.Panel', {
        anchor: inputAnchor,
        layout: { type: 'border' },
        items: [sidePanel, bodyPanel],
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
proto.createTree = function (gridInfo) {
    //创建树状图模型
    var me = this;
    var vcl = me.vcl;
    var destColumns = Ext.decode(vcl.tpl.Layout.GridRange.Renderer);
    for (var i = 0; i < destColumns.length; i++) {
        switch (destColumns[i].dataIndex) {
            case "SUBMATERIALID":
                destColumns[i].renderer = function (sprite, record, attributes, index, store) {
                    var diffrence = (new Function("return " + record.record.get("DIFFERENCE")))();
                    var v;
                    if (record.record.get("SUBMATERIALID") != "") {
                        v = '<span style="color:black;">' + record.record.get("SUBMATERIALID") + "," + record.record.get("SUBMATERIALNAME") + '</span>';
                        if (diffrence !== undefined && diffrence.Modify !== undefined) {
                            if (vcl.IsContain(diffrence.Modify, "SUBMATERIALID")) {
                                v = '<span style="color:red;">' + record.record.get("SUBMATERIALID") + "," + record.record.get("SUBMATERIALNAME") + '</span>';
                            }
                        }
                    }
                    return v;
                }
                break;
            case "BASEQTY":
                destColumns[i].renderer = function (sprite, record, attributes, index, store) {
                    var diffrence = (new Function("return " + record.record.get("DIFFERENCE")))();
                    var v;
                    v = '<span style="color:black;">' + record.record.get("BASEQTY") + '</span>';
                    if (diffrence !== undefined && diffrence.Modify !== undefined) {
                        if (vcl.IsContain(diffrence.Modify, "BASEQTY")) {
                            v = '<span style="color:red;">' + record.record.get("BASEQTY") + '</span>';
                        }
                    }
                    return v;
                }
                break;
            case "UNITQTY":
                destColumns[i].renderer = function (sprite, record, attributes, index, store) {
                    var diffrence = (new Function("return " + record.record.get("DIFFERENCE")))();
                    var v;
                    v = '<span style="color:black;">' + record.record.get("UNITQTY") + '</span>';
                    if (diffrence !== undefined && diffrence.Modify !== undefined) {
                        if (vcl.IsContain(diffrence.Modify, "UNITQTY")) {
                            v = '<span style="color:red;">' + record.record.get("UNITQTY") + '</span>';
                        }
                    }
                    return v;
                }
                break;
            case "ATTRIBUTECODE":
                destColumns[i].renderer = function (sprite, record, attributes, index, store) {
                    var diffrence = (new Function("return " + record.record.get("DIFFERENCE")))();
                    var v;
                    if (record.record.get("ATTRIBUTECODE") != "") {
                        v = '<span style="color:black;">' + record.record.get("ATTRIBUTECODE") + '</span>';
                        if (diffrence !== undefined && diffrence.Modify !== undefined) {
                            if (vcl.IsContain(diffrence.Modify, "ATTRIBUTECODE")) {
                                v = '<span style="color:red;">' + record.record.get("ATTRIBUTECODE") + '</span>';
                            }
                        }
                    }
                    return v;
                }
                break;
            case "ATTRIBUTEDESC":
                destColumns[i].renderer = function (sprite, record, attributes, index, store) {
                    var diffrence = (new Function("return " + record.record.get("DIFFERENCE")))();
                    var v;
                    if (record.record.get("ATTRIBUTEDESC") != "") {
                        v = '<span style="color:black;">' + record.record.get("ATTRIBUTEDESC") + '</span>';
                        if (diffrence !== undefined && diffrence.Modify !== undefined) {
                            if (vcl.IsContain(diffrence.Modify, "ATTRIBUTEDESC")) {
                                v = '<span style="color:red;">' + record.record.get("ATTRIBUTEDESC") + '</span>';
                            }
                        }
                    }
                    return v;
                }
                break;
            case "PMATERIALID":
                destColumns[i].renderer = function (sprite, record, attributes, index, store) {
                    var diffrence = (new Function("return " + record.record.get("DIFFERENCE")))();
                    var v;
                    if (record.record.get("PMATERIALID") != "") {
                        v = '<span style="color:black;">' + record.record.get("PMATERIALID") + "," + record.record.get("PMATERIALNAME") + '</span>';
                        if (diffrence !== undefined && diffrence.Modify !== undefined) {
                            if (vcl.IsContain(diffrence.Modify, "PMATERIALID")) {
                                v = '<span style="color:red;">' + record.record.get("PMATERIALID") + "," + record.record.get("PMATERIALNAME") + '</span>';
                            }
                        }
                    }
                    return v;
                }
                break;
            default:
                break;
        }
    }
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
            root: obj,
        });

        return store;
    }
    var treePanel = Ext.create('Ext.tree.Panel', {
        title: title,
        header: true,
        animate: true,
        rootVisible: true,
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
        plugins: [cellEditing],
        viewConfig: {
            plugins: {
                ptype: 'treeviewdragdrop',
                dragText: '{0} 选中节点',
                allowContainerDrop: true,
                allowParentInsert: true,
                containerScroll: true,
                sortOnDrop: true,

            },
            getRowClass: function (record, rowIndex, rowParams, store) {
                var bacClass = "white";
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
                beforedrop: function (node, data, overModel, dropPosition, dropHandler, eOpts) {
                    if (vcl.isEdit === false) {
                        Ext.Msg.alert("提示", "修改状态下才可以移动节点!");
                        return false;
                    }
                    else {
                        var index = 1;
                        for (var i = 0; i < data.records[0].parentNode.childNodes.length; i++) {
                            if (data.records[0].parentNode.childNodes[i].data["ROW_ID"] != data.records[0].data["ROW_ID"]) {
                                data.records[0].parentNode.childNodes[i].set("ORDERNUM", index);
                                index++;
                            }
                        }
                    }
                },
                drop: function (node, data, overModel, dropPosition, eOpts) {
                    vcl.ChangeRow(data);
                },
            }
        },
        listeners: {
            beforeedit: function (self) {
                if (vcl.isEdit === false)
                    return false;
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
                            return vcl.dataSet.FindRow(1, items[i].get("ROW_ID"));
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
                if (vcl.isEdit) {
                    var menu = new Ext.menu.Menu({
                        items: [
                            {
                                text: "新增子节点", iconCls: 'gridAdd',
                                handler: function () {
                                    record.expand();
                                    masterRow.store = vcl.dataSet.getTable(1);
                                    var newRow = vcl.addRow(masterRow, 1);
                                    newRow.set("PARENTROWID", record.get("ROW_ID"));
                                    newRow.set("PMATERIALID", record.get("SUBMATERIALID"));
                                    newRow.set("PMATERIALNAME", record.get("PMATERIALNAME"));
                                    newRow.set("DIFFERENCE", "{\"Add\":true}");
                                    newRow.set("BASEQTY", 1);
                                    newRow.set("UNITQTY", 1);
                                    newRow.set("PRODUCEQTY", vcl.GetProduceQtyAddRow.call(vcl, newRow, vcl.dataSet));
                                    newRow.set("QUANTITY", vcl.GetProduceQtyAddRow.call(vcl, newRow, vcl.dataSet))
                                    newRow.set("ISNONSTANDARDCONFIRM", 1);
                                    var bomLevel;
                                    var orderNum;
                                    orderNum = record.childNodes[record.childNodes.length - 1].data["ORDERNUM"] + 1;
                                    if (record.get("BOMLEVEL") == 0) {
                                        bomLevel = 1;
                                    }
                                    else {
                                        bomLevel = record.get("BOMLEVEL") + 1;
                                    }
                                    newRow.set("BOMLEVEL", bomLevel);
                                    newRow.set("ORDERNUM", orderNum);
                                    var newNode = {
                                        SALEBOMID: newRow.get('SALEBOMID'),
                                        ROW_ID: newRow.get('ROW_ID'),
                                        PARENTROWID: record.get("ROW_ID"),
                                        BOMLEVEL: bomLevel,
                                        ORDERNUM:orderNum,
                                        SUBMATERIALID: "",
                                        SUBMATERIALNAME: "",
                                        SUBMATERIALSPEC: "",
                                        NODENAME: '',
                                        UNITID: "",
                                        UNITNAME: "",
                                        ISKEY: "",
                                        MATSTYLE: "",
                                        ATTRIBUTEID: "",
                                        ATTRIBUTENAME: "",
                                        ATTRIBUTECODE: "",
                                        ATTRIBUTEDESC: "",
                                        BASEQTY: newRow.get('BASEQTY'),
                                        UNITQTY: newRow.get('UNITQTY'),
                                        PRODUCEQTY: newRow.get('PRODUCEQTY'),//生产应用量
                                        QUANTITY: newRow.get('QUANTITY'),//生产应用量
                                        PMATERIALID: record.get("SUBMATERIALID"),
                                        PMATERIALNAME: record.get("SUBMATERIALNAME"),
                                        SALETECHROUTEID: "",
                                        SALETECHROUTENAME: "",
                                        SALETECHROUTEROWID: "",
                                        WORKPROCESSNO: "",
                                        WORKSHOPSECTIONID: "",
                                        WORKSHOPSECTIONNAME: "",
                                        WORKPROCESSID: "",
                                        WORKPROCESSNAME: "",
                                        BUFFERNUM: "",
                                        DIFFERENCE: newRow.get("DIFFERENCE"),
                                        ISNONSTANDARDCONFIRM: newRow.get("ISNONSTANDARDCONFIRM"),
                                        expanded: true,//展开
                                        leaf: false,
                                        //iconCls: 'leaf'
                                    }
                                    newNode = record.appendChild(newNode);
                                    newNode.bindingRow = newRow;
                                    vcl.dataSet.getTable(0).data.items[0].set("ISCONFIRM", false);
                                    vcl.forms[0].loadRecord(vcl.dataSet.getTable(0).data.items[0]);
                                }
                            }, {
                                text: "删除子节点", iconCls: 'gridDelete',
                                handler: function () {
                                    //找到所有子节点的ID
                                    function GetRowids(child) {
                                        if (child.length > 0) {
                                            for (var i = 0; i < child.length; i++) {
                                                rowids.push(child[i].data["ROW_ID"]);
                                                if (child[i].childNodes.length > 0) {
                                                    GetRowids(child[i].childNodes);
                                                }
                                            }
                                        }
                                    }
                                    if (record.data["ROW_ID"] != "") {
                                        vcl.UpdateIsConfirm.call(vcl, record, true);
                                        var rowids = new Array();
                                        rowids.push(record.data["ROW_ID"]);
                                        if (record.childNodes.length > 0) {
                                            GetRowids(record.childNodes);
                                        }
                                        for (var i = 0; i < rowids.length; i++) {
                                            var foundRow = vcl.dataSet.FindRow(1, rowids[i]);
                                            if (foundRow) {
                                                vcl.dataSet.getTable(1).data.remove(foundRow);
                                            }
                                        }
                                        var index = 1;
                                        for (var i = 0; i < record.parentNode.childNodes.length; i++) {
                                            if (record.parentNode.childNodes[i].data["ROW_ID"] != record.data["ROW_ID"]) {
                                                record.parentNode.childNodes[i].set("ORDERNUM", index);
                                                index++;
                                            }
                                        }
                                        record.remove();
                                        
                                    }
                                }
                            }, {
                                text: "复制节点", iconCls: 'gridCopy',
                                handler: function () {
                                    function getBindingRow() {
                                        var bindingRow = vcl.dataSet.FindRow(1, record.get("ROW_ID"));
                                        return bindingRow;
                                    }
                                    if (record.data["ROW_ID"] == 0) {
                                        Ext.Msg.alert("提示", "根节点不能复制!");
                                    } else {
                                        var bindingRow = record.bindingRow || getBindingRow();
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
                                    vcl.pasteData(record, null, null, true);
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
                           }
                        ]
                    }).showAt(e.getXY());
                }
            },
            beforeitemdblclick: function () {
                return vcl.canExpend;
            }
        }
    });
    vcl.tree = treePanel;
    vcl.firstLoad = false;
    return treePanel;
}
