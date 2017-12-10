/// <reference path="../../../ext/bootstrap.js" />
/// <reference path="../../../ext/ext-all.js" />
/// <reference path="../../../ext/locale/ext-lang-zh_CN.js" />
/// <reference path="../../../ext/ext.js" />


Ext.ns('Ax.tpl')

Ax.tpl.LibGridTpl = function (vcl) {
    Ax.tpl.LibTplBase.apply(this, arguments);
};

var proto = Ax.tpl.LibGridTpl.prototype = Object.create(Ax.tpl.LibTplBase.prototype);
proto.constructor = Ax.tpl.LibGridTpl;

proto.createDefaultView = function () {
    var vcl = this.vcl;
    return this.create(true);
}

proto.create = function (changeView) {
    var vcl = this.vcl;
    if (changeView !== true)
        vcl.browseTo();
    var store = vcl.dataSet.getTable(0);
    var grid = Ax.tpl.GridManager.createGrid({
        vcl: vcl,
        tableIndex: 0,
        curRange: vcl.tpl.Layout.GridRange
    });
    var funPanel = Ext.create('Ext.panel.Panel', {
        border: false,
        margin: '10 10',
        layout: { type: 'hbox', align: 'stretch' },
        defaults: {
            margin: '0 10'
        },
        items: vcl.tpl.Layout.ButtonRange != null ? Ext.decode(vcl.tpl.Layout.ButtonRange.Renderer) : []
    });
    var toolBarAction = Ax.utils.LibToolBarBuilder.createGridAction(this, 0);
    var mainWidth = document.body.clientWidth > 1210 ? document.body.clientWidth - 27 : 1210;
    var mainPanel = Ext.create('Ext.panel.Panel', {
        width: mainWidth,
        height: document.body.clientHeight - 80,
        layout: { type: 'vbox', align: 'stretch' },
        items: [grid, funPanel],
        border: false,
        tbar: Ax.utils.LibToolBarBuilder.createToolBar(toolBarAction)
    });
    return mainPanel;
};
