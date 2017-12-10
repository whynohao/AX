/// <reference path="../../../ext/bootstrap.js" />
/// <reference path="../../../ext/ext-all.js" />
/// <reference path="../../../ext/locale/ext-lang-zh_CN.js" />
/// <reference path="../../../ext/ext.js" />


Ext.ns('Ax.tpl')

Ax.tpl.LibDailyRptTpl = function (vcl) {
    Ax.tpl.LibTplBase.apply(this, arguments);
};

var proto = Ax.tpl.LibDailyRptTpl.prototype = Object.create(Ax.tpl.LibTplBase.prototype);
proto.constructor = Ax.tpl.LibDailyRptTpl;

proto.createDefaultView = function () {
    var vcl = this.vcl;
    return this.create(this.vcl.win.title);
}
proto.create = function (displayText) {
    var vcl = this.vcl;
    var headerPanel = Ext.create('Ext.panel.Panel', {
        layout: { type: 'hbox', align: 'middle', pack: 'center' },
        items: [{
            xtype: 'label',
            text: displayText,
            baseCls: 'rptTitle',
            margin: '10 0'
        }]
    });
    var dateControl = Ext.create('Ext.form.field.Date', {
        fieldLabel: '日期',
        labelAlign: 'right',
        margin: '0 10',
        value: vcl.currentDate
    });
    if (vcl.dataSet && vcl.dataSet.dataList) {
        vcl.dataSet.dataList[0].ownGrid = null;
    }
    var doShowRpt = function (field, value, eOpts) {
        vcl.currentDate = value;
        vcl.showRpt(this.vcl.queryCondition);
    };
    dateControl.on("select", doShowRpt, this);
    var conditionPanel = Ext.create('Ext.form.Panel', {
        items: [dateControl]
    });
    vcl.showRpt();
    var store = vcl.dataSet.getTable(0);
    var grid = Ax.tpl.GridManager.createGrid({
        vcl: vcl,
        tableIndex: 0,
        curRange: vcl.tpl.Layout.GridRange,
        isEditGrid: false,
        store: store
    });
    var toolBarAction = Ax.utils.LibToolBarBuilder.createRptAction(vcl, 0);
    var mainPanel = Ext.create('Ext.panel.Panel', {
        layout: { type: 'vbox', align: 'stretch' },
        items: [headerPanel, conditionPanel, grid],
        border: false,
        tbar: Ax.utils.LibToolBarBuilder.createToolBar(toolBarAction)
    });
    return mainPanel;
};