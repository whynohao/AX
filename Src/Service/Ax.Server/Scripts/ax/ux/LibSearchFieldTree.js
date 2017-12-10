/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：具有下拉树的ComboBox
 * 创建标识：Zhangkj 2017/03/15
 *
************************************************************************/
Ext.define('Ax.ux.form.LibSearchFieldTree', {
    extend: 'Ax.ux.form.LibSearchField',
    alias: 'widget.libSearchfieldTree',
    queryMode: 'local',
    listHeight: 200,
    treeRootData: null,
    hasDoQuery: false,//是否已经执行查询
    initComponent: function () {
        this.callParent();
        this.treeRootData = {
            Id: '0',
            Name: "",
            ContainsKeyField: '',
            expanded: true
        };
        this.tplId = Ext.id();
        var me = this;
        this.listConfig = {
            loadingText: '搜索中...',
            emptyText: '',//没有数据的提示信息显示在树上
            tpl: '<div><div id="' + this.tplId + '" style="height:' + this.listHeight + 'px;overflow:hidden;"></div></div>',
            tpl: Ext.create('Ext.XTemplate', '<div><div id="' + this.tplId + '" style="height:' + this.listHeight + 'px;overflow:hidden;">',
                '<tpl if="this.isEmptyData()">',
                                   '没有匹配的数据',
                                 '</tpl>',
                '</div></div>',
                {
                    isEmptyData: function () {
                        if (me.hasDoQuery && (!me.treeRootData || !me.treeRootData.children || me.treeRootData.children.length == 0))
                            return true;
                        return false;
                    },
                    highlight: function (v) {
                        query = this.field.lastQuery;
                        if (Ext.isEmpty(query)) {
                            return v;
                        } else {
                            //Zhangkj 20170206 需要高亮的对象可能为空
                            if (v) {
                                return v.replace(new RegExp(query, 'gi'), function (m) {
                                    return "<font color='red'>" + m + "</font>";
                                });
                            }
                        }
                    }
                }
            )
        };
        this.createTree();

        this.on({
            expand: function (self) {
                this.createTree();//基类中执行查询时会将下列列表的控件清空，需要重新创建
                if (!me.tree.rendered && me.tplId) {
                    me.tree.render(me.tplId);
                    //me.tree.render(me.id);
                }
                this.tree.show();
                //检查是否为editor的子控件
                if (me.isEditorComponent && me.container) {
                    if (me.container.component && me.container.component.__proto__) {
                        //me.clearValue();//置空
                        //me.updateValue();
                        this.oldContainerOnFocusLeave = me.container.component.__proto__.onFocusLeave;
                        me.container.component.__proto__.onFocusLeave = Ext.emptyFn//置空编辑器容器的失去焦点事件处理函数                           
                    }
                }
            },
            collapse: function (self) {
                var me = self;
                if (!me.readOnly) {
                    if (me.up('form')) {
                        if (!me.validating(me)) {
                            me.focus(false, true);
                            return;
                        }
                    }
                    else {
                        self.setValue(self.value);
                        if (self.rawValue == "") {
                            self.setValue(self.rawValue);
                        }
                    }
                }
            }
        });
    },
    //还原容器的OnFocusLeave事件处理函数
    restoreContainerOnFocusLeave: function () {
        var me = this;
        if (me.isEditorComponent && me.container) {
            if (me.container.component && me.container.component.__proto__ && me.oldContainerOnFocusLeave) {
                me.container.component.__proto__.onFocusLeave = me.oldContainerOnFocusLeave;//还原编辑器容器的失去焦点事件处理函数
                me.oldContainerOnFocusLeave = null;
            }
        }
    },
    isCollapseWhenTreeBlur: function (self, e, eOpts) {
        if (!e.relatedTarget) {
            return true;
        }
        //console.log('have focus to ' + e.relatedTarget.id);
        var el, orgEl;
        if (e.relatedTarget.id != "" && Ext.getCmp(e.relatedTarget.id)) {
            el = Ext.getCmp(e.relatedTarget.id);//Component
            if (el) {
                var tree = el.up('treepanel');
                if (tree && tree.id == this.tree.id) {
                    return false;
                }
                else
                    return true;
            }
            else
                return true;
        }
        else {
            el = Ext.get(e.relatedTarget);//Element    
            orgEl = el;
            while (el) {
                if (el.id == this.tree.items.items[0].id) {
                    return false;
                }
                el = el.getParent();
            }
            return true;
        }
    },
    onFocusLeave: function (e, nocheck) {
        if (nocheck) {
            if (this.rawValue == "") {
                this.setValue(this.rawValue);
            }
            this.restoreContainerOnFocusLeave();//还原容器的OnFocusLeave事件处理函数
            this.collapse();
            this.callParent([e]);
            return;
        }
        else {
            var isCollapse = this.isCollapseWhenTreeBlur(this, e, null);
            if (isCollapse) {
                if (this.rawValue == "") {
                    this.setValue(this.rawValue);
                }
                this.restoreContainerOnFocusLeave();//还原容器的OnFocusLeave事件处理函数
                this.collapse();
                this.callParent([e]);
                return;
            }
        }
    },
    collapse: function () {
        //折叠前还原容器的OnFocusLeave事件处理函数
        this.restoreContainerOnFocusLeave();
        this.callParent();
    },
    collapseIf: function (e) {
        for (var i = 0; i < e.event.path.length; i++) {
            if (e.event.path[i].id == this.tplId)
                return;//如果焦点仍然在下拉列表的div控件区域内，则不关闭
        }
        this.restoreContainerOnFocusLeave();//还原容器的OnFocusLeave事件处理函数
        this.callParent([e]);
    },
    onExpand: function () {

        var doc = Ext.getDoc();
        var me = this;
        //this.hideListeners.args[0].touchstart=
        // log(this.hideListeners);
        //doc.events.mousedown.removeListener(me.collapseIf, me);
        //doc.events.mousedown.removeListener(me.collapseIf, me);//touchstart翻译成PC上的事件是mousedown
        //doc.events.mousedown.clearListeners();//touchstart翻译成PC上的事件是mousedown
        //doc.events.mousewheel.removeListener(me.collapseIf, me);
    },
    createTree() {
        var me = this;
        if (this.tree) {
            try {
                this.tree.hide();
                //this.tree.destroy();
                this.tree = null;

            } catch (e) {
                console.log(e);
            }
        }
        this.tree = new Ext.tree.TreePanel({
            displayField: 'Name',
            valueField: 'Id',
            rootVisible: false,
            root: this.treeRootData,
            listeners: {
                beforeitemexpand: function (node, eOpts) {
                },
                itemclick: function (view, record) {
                    me.collapse();//对于控件集成到grid的列中时，因为会在组件外部套一个editor容器，必须先collapse然后再设置值
                    me.setValue(record);
                    me.select(record.data.Id);
                    me.restoreContainerOnFocusLeave();//还原容器的OnFocusLeave事件处理函数
                },
                //itemclick: me.treeItemClick,
                click: function (node) {
                    this.treeClk(node);
                },
                focus: function (self, The, eOpts) {
                    console.log(The);
                }
            }
        });
        this.tree.autoScroll = true;
        this.tree.height = 180;
        this.tree.containerScroll = false;
    },
    doQuery: function (queryString, forceAll, rawQuery) {
        this.hasDoQuery = false;
        var queryResult = Ax.ux.form.LibSearchField.prototype.doQuery.apply(this, arguments);
        //var queryResult = this.callParent([queryString, forceAll, rawQuerye]);    
        if (queryResult) {
            if (!this.treeRootData) {
                this.treeRootData = {
                    Id: '0',
                    Name: "",
                    ContainsKeyField: '',
                    expanded: true,
                    children: []
                };
            }
            if (!this.treeRootData.children)
                this.treeRootData.children = [];
            this.treeRootData.children = [];
            if (this.remoteData) {
                if (this.remoteData.length > 0 && this.remoteData[0].TotalList && this.remoteData[0].TotalList.length > 0)
                    this.store.loadData(this.remoteData[0].TotalList);
                for (var i = 0; i < this.remoteData.length; i++) {
                    this.treeRootData.children.push(this.remoteData[i]);
                }
            }
            //this.treeRootData = this.remoteData;
            this.expand();
        }
        this.hasDoQuery = true;
        return queryResult
    }
});