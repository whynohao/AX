/// <reference path="../../../ext/bootstrap.js" />
/// <reference path="../../../ext/ext-all.js" />
/// <reference path="../../../ext/locale/ext-lang-zh_CN.js" />
/// <reference path="../../../ext/ext.js" />


Ext.ns('Ax.tpl')

Ax.tpl.LibRptTpl = function (vcl) {
  Ax.tpl.LibTplBase.apply(this, arguments);
};

var proto = Ax.tpl.LibRptTpl.prototype = Object.create(Ax.tpl.LibTplBase.prototype);
proto.constructor = Ax.tpl.LibRptTpl;

proto.createDefaultView = function () {
  var vcl = this.vcl;
  return this.create(this.vcl.win.title);
}

proto.create =  function (displayText,execTaskDataId) {
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
  if (vcl.dataSet && vcl.dataSet.dataList) {
    vcl.dataSet.dataList[0].ownGrid = null;
  }
  // 2017/6/27 自定义查询组件
  var jsStr = vcl.invorkBcf('GetFilterLayoutJs', [window.UserId])
  try{
    var filterPanel = eval(jsStr)
  }catch(e){
    filterPanel = undefined
  }
  // end
  if (execTaskDataId) {
    vcl.showTaskRpt(execTaskDataId);
  } else {
    vcl.showRpt(this.vcl.queryCondition);
  }
  var store = vcl.dataSet.getTable(0);
  var grid = Ax.tpl.GridManager.createGrid({
    vcl: vcl,
    tableIndex: 0,
    curRange: vcl.tpl.Layout.GridRange,
    isEditGrid: false,
    store: store
  });
  var toolBarAction = Ax.utils.LibToolBarBuilder.createRptAction(vcl, 0);
  var items;
  if (filterPanel)
    items = [headerPanel,filterPanel, grid];
  else
    items = [headerPanel, grid];
  var mainPanel = Ext.create('Ext.panel.Panel', {
    layout: { type: 'vbox', align: 'stretch' },
    items: items,
    border: false,
    tbar: Ax.utils.LibToolBarBuilder.createToolBar(toolBarAction)
  });
  return mainPanel;
};
