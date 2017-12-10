/// <reference path="../../../ext/bootstrap.js" />
/// <reference path="../../../ext/ext-all.js" />
/// <reference path="../../../ext/locale/ext-lang-zh_CN.js" />
/// <reference path="../../../ext/ext.js" />


Ext.ns('Ax.tpl')

Ax.tpl.LibVisualTpl = function (vcl) {
    Ax.tpl.LibTplBase.apply(this, arguments);
};

var proto = Ax.tpl.LibVisualTpl.prototype = Object.create(Ax.tpl.LibTplBase.prototype);
proto.constructor = Ax.tpl.LibVisualTpl;

proto.create = function () {
    var toolBarAction = Ax.utils.LibToolBarBuilder.createVisualAction(this.vcl, 0);
    var mainPanel = Ext.create('Ext.panel.Panel', {
        anchor: '100% 100%',
        html: "<iframe width=100% height=100%  scrolling='yes' border=0 id='" + this.vcl.frameId + "'/>",
        border: false,
        tbar: Ax.utils.LibToolBarBuilder.createToolBar(toolBarAction),
        renderTo: Ext.getBody()
    });
    this.vcl.showData();
    return mainPanel;
};