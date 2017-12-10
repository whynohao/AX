
Ax.tpl.LibBillTreeTpl = function (vcl) {
    this.vcl = vcl;
};

Ax.tpl.LibBillTreeTpl.prototype = {
    constructor: Ax.tpl.LibBillTreeTpl,
    create: function () {
        var store = Ext.create('Ext.data.TreeStore', {
            root: {
                expanded: true,
                children: [
                    { text: "detention", leaf: true },
                    {
                        text: "homework", expanded: true, children: [
                          { text: "book report", leaf: true },
                          { text: "algebra", leaf: true }
                        ]
                    },
                    { text: "buy lottery tickets", leaf: true }
                ]
            }
        });
        var treePanel = Ext.create('Ext.tree.Panel', {
            store: store,
            rootVisible: false
            //columns: 
        });
        var splitter = Ext.create('Ext.resizer.Splitter', {
            border: 5,
            style: {
                borderColor: '#eae4e4',
                borderStyle: 'solid'
            }
        });
        var tabPanel = Ext.create('Ext.tab.Panel', {
            flex: 8
        });

        var mainPanel = Ext.create('Ext.panel.Panel', {
            width: mainWidth,
            height: document.body.clientHeight - 80,
            layout: { type: 'hbox', align: 'stretch' },
            items: [treePanel, splitter, tabPanel],
            border: false
        });
        return mainPanel;
    }
}