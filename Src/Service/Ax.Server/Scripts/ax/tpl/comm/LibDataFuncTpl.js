/// <reference path="../../../ext/bootstrap.js" />
/// <reference path="../../../ext/ext-all.js" />
/// <reference path="../../../ext/locale/ext-lang-zh_CN.js" />
/// <reference path="../../../ext/ext.js" />


Ext.ns('Ax.tpl')

Ax.tpl.LibDataFuncTpl = function (vcl) {
  Ax.tpl.LibTplBase.apply(this, arguments);
};

var proto = Ax.tpl.LibDataFuncTpl.prototype = Object.create(Ax.tpl.LibTplBase.prototype);
proto.constructor = Ax.tpl.LibDataFuncTpl;

proto.create = function () {
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

  function addTab (panel, displayName) {
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
  }
  ;

  var gridPanel;
  if (vcl.tpl.Layout.GridRange != null) {
    gridPanel = Ax.tpl.GridManager.createGrid({
      vcl: vcl,
      selType: 'checkboxmodel',
      parentRow: vcl.dataSet.getTable(0).data.items[0],
      tableIndex: vcl.dataSet.tableMap.get(vcl.tpl.Layout.GridRange.Store),
      curRange: vcl.tpl.Layout.GridRange,
      title: vcl.tpl.Layout.GridRange.DisplayName
    });
  }
  ;

  var funPanel;
  var inputAnchor = '100% 100%';
  if (vcl.tpl.Layout.ButtonRange != null) {
    inputAnchor = '100% 100%';
    funPanel = Ext.create('Ext.panel.Panel', {
      border: false,
      style: 'border-top: 1px solid black',
      layout: {type: 'hbox', align: 'stretch'},
      defaults: {
        margin: '4 4'
      },
      items: Ext.decode(vcl.tpl.Layout.ButtonRange.Renderer)
    });
  }

  var inputPanel = Ext.create('Ext.panel.Panel', {
    anchor: inputAnchor,
    layout: {type: 'vbox', align: 'stretch'},
    items: [panel, funPanel, tabPanel, gridPanel],
    border: false
  });

  var toolBarAction = Ax.utils.LibToolBarBuilder.createDataFuncAction(vcl, 0);
  var mainWidth = document.body.clientWidth > 1210 ? document.body.clientWidth - 27 : 1210;
  var mainPanel = Ext.create('Ext.panel.Panel', {
    width: mainWidth,
    height: document.body.clientHeight,
    layout: {type: 'anchor'},
    items: [inputPanel],
    border: false,
    tbar: Ax.utils.LibToolBarBuilder.createToolBar(toolBarAction)
  });
  return mainPanel;
};
