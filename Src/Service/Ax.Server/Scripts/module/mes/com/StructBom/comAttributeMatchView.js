comAttributeMatchView = function () {
    Ax.tpl.LibDataFuncTpl.apply(this, arguments);
    if (this.vcl.funcView.containsKey("default")) {
        this.vcl.funcView.get("default").name = "onReady";
    }
};
var proto = comAttributeMatchView.prototype = Object.create(Ax.tpl.LibDataFuncTpl.prototype);
proto.constructor = comAttributeMatchView;
proto.onReady = function () {
    var me = this;
    var vcl = this.vcl;
    vcl.forms = [];
    vcl.openFunc();
    var store = vcl.dataSet.getTable(0);
    var gridPanel;
    if (vcl.tpl.Layout.GridRange != null) {
        gridPanel = Ax.tpl.GridManager.createGrid({
            vcl: vcl,
            parentRow: vcl.dataSet.getTable(0).data.items[0],
            tableIndex: vcl.dataSet.tableMap.get(vcl.tpl.Layout.GridRange.Store),
            curRange: vcl.tpl.Layout.GridRange,
            title: vcl.tpl.Layout.GridRange.DisplayName
        });
        gridPanel.header = false;
    };

    var funPanel;
    var inputAnchor = '100% 100%';
    if (vcl.tpl.Layout.ButtonRange != null) {
        inputAnchor = '100% 95%';
        funPanel = Ext.create('Ext.panel.Panel', {
            border: false,
            anchor: '100% 5%',
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
        layout: { type: 'vbox', align: 'stretch' },
        items: [gridPanel],
        border: false
    });
    var mainPanel = Ext.create('Ext.panel.Panel', {
        layout: { type: 'anchor' },
        items: [inputPanel, funPanel],
        border: false,
    });
    return mainPanel;
};