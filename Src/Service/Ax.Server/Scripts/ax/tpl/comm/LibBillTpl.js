/// <reference path="../../../ext/bootstrap.js" />
/// <reference path="../../../ext/ext-all.js" />
/// <reference path="../../../ext/locale/ext-lang-zh_CN.js" />
/// <reference path="../../../ext/ext.js" />


Ext.ns('Ax.tpl')

var BlockTypeEnum = {
  ControlGroup: 0,
  Grid: 1,
  Func: 2,
  TreeView: 3,
  TreeGrid: 4
}

Ax.tpl.LibTplBase = function (vcl) {
  this.vcl = vcl;
  this.vcl.funcView.add('default', {name: 'create', display: '默认视图'});
};


Ax.tpl.LibBillTpl = function (vcl) {
  Ax.tpl.LibTplBase.apply(this, arguments);
};

var proto = Ax.tpl.LibBillTpl.prototype = Object.create(Ax.tpl.LibTplBase.prototype);
proto.constructor = Ax.tpl.LibBillTpl;

proto.createDefaultView = function () {
  var vcl = this.vcl;
  return this.create(vcl.billAction, vcl.currentPk, false, undefined, true);
}

proto.create = function (billAction, curPks, callback, isF4, lookVersionObj, changeView) {
  var me = this;
  var vcl = this.vcl;
  vcl.forms = [];
  if (changeView !== true) {
    vcl.billAction = billAction;
    if (curPks)
      vcl.currentPk = curPks;
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
        toolBarAction[0].execute({noOpen: true});
    } else if (vcl.billAction == BillActionEnum.Edit) {
      if (changeView !== true)
        vcl.browseTo(vcl.currentPk);
      toolBarAction = Ax.utils.LibToolBarBuilder.createBillAction(vcl, 0, isF4);
      if (changeView !== true)
        toolBarAction[1].execute();
    } else {
      if (changeView !== true)
        vcl.browseTo(vcl.currentPk);
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
          margin: '4 0 4 2',
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

  function addImageTab () {
    var masterRow = vcl.dataSet.getTable(0).data.items[0];
    var imgSrc = masterRow.get('IMGSRC');
    if (imgSrc == undefined)
      return;
    if (imgSrc != '')
      imgSrc = '/UserPicture/' + vcl.progId + '/' + masterRow.get('INTERNALID') + '/' + imgSrc;
    var btnPanel = Ext.create('Ext.panel.Panel', {
      border: false,
      anchor: '100% 10%',
      layout: {type: 'hbox', align: 'stretch'},
      items: [{
        xtype: 'button',
        width: 100,
        margin: '4 0 4 2',
        text: '图片上传',
        handler: function () {
          if (vcl.isEdit !== false) {
            Ext.Msg.alert('提示', '浏览状态下才可以导入。');
            return;
          }
          var panel = Ext.create('Ext.form.Panel', {
            bodyPadding: 10,
            frame: true,
            renderTo: Ext.getBody(),
            items: [{
              xtype: 'filefield',
              name: 'txtFile',
              fieldLabel: '图片',
              labelWidth: 50,
              msgTarget: 'side',
              allowBlank: false,
              anchor: '100%',
              buttonText: '选择...'
            }],

            buttons: [{
              text: '导入',
              handler: function () {
                var form = this.up('form').getForm();
                if (form.isValid()) {
                  form.submit({
                    url: '/fileTranSvc/upLoadUserPicture',
                    waitMsg: '正在导入图片...',
                    success: function (fp, o) {
                      var masterRow = vcl.dataSet.getTable(0).data.items[0];
                      var fileName = o.result.FileName;
                      var internalId = masterRow.get('INTERNALID');
                      Ext.Ajax.request({
                        url: 'fileTranSvc/moveUserPicture',
                        method: 'POST',
                        jsonData: {
                          progId: vcl.progId,
                          internalId: internalId,
                          fileName: fileName
                        },
                        async: false,
                        timeout: 90000000,
                        success: function (response) {
                          var ret = Ext.decode(response.responseText);
                          imgSrc = '/UserPicture/' + vcl.progId + '/' + internalId + '/' + ret.MoveUserPictureResult;
                          vcl.picture.setSrc(imgSrc);
                          masterRow.set('IMGSRC', ret.MoveUserPictureResult),
                            Ext.Msg.alert('提示', '图片已导入成功.');
                          win.close();
                        }
                      });
                    },
                    failure: function (fp, o) {
                      Ext.Msg.alert('错误', '图片导入失败.');
                    }
                  });
                }
              }
            }]
          });
          win = Ext.create('Ext.window.Window', {
            autoScroll: true,
            width: 400,
            height: 300,
            layout: 'fit',
            constrainHeader: true,
            minimizable: true,
            maximizable: true,
            items: [panel]
          });
          win.show();
        }
      }, {
        xtype: 'button',
        width: 100,
        margin: '4 0 4 2',
        text: '图片清除',
        handler: function () {
          var masterRow = vcl.dataSet.getTable(0).data.items[0];
          var img = masterRow.get('IMGSRC');
          if (img && img != '') {
            Ext.Ajax.request({
              url: 'fileTranSvc/removeUserPicture',
              method: 'POST',
              jsonData: {
                progId: vcl.progId,
                internalId: masterRow.get('INTERNALID'),
                fileName: img
              },
              async: false,
              timeout: 90000000,
              success: function (response) {
                vcl.picture.setSrc('');
              }
            });
          }
        }
      }]
    });
    vcl.picture = Ext.create('Ext.Img', {
      src: imgSrc
    });
    var imgPanel = Ext.create('Ext.panel.Panel', {
      border: 1,
      autoScroll: true,
      anchor: '100% 90%',
      items: vcl.picture
    });
    var tabPanel = Ext.create('Ext.panel.Panel', {
      border: false,
      margin: '4 0 4 2',
      layout: {type: 'anchor'},
      items: [imgPanel, btnPanel]
    });
    addTab(tabPanel, '图片信息');
  };

  function addValidityTab (me) {
    var tempPanel = Ext.create('Ext.form.Panel', {
      border: false,
      tableIndex: 0,
      margin: '4 0 4 2',
      defaultType: 'textfield',
      items: {
        xtype: 'container',
        layout: {type: 'table', columns: 4},
        style: {marginTop: '6px', marginBottom: '6px'},
        defaults: {labelAlign: 'right'},
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
  function addFixTab (me) {
    var items;
    if (me.vcl.billType == BillTypeEnum.Master) {
      addValidityTab(me);
      items = {
        xtype: 'container',
        layout: {type: 'table', columns: 4},
        style: {marginTop: '6px', marginBottom: '6px'},
        defaults: {labelAlign: 'right', readOnly: true, width: 300},
        defaultType: 'libTextField',
        items: [{
          xtype: 'libSearchfield',
          labelAlign: 'right',
          fieldLabel: '创建人',
          relIndex: 0,
          relSource: {'com.Person': ''},
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
          relSource: {'com.Person': ''},
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
          relSource: {'com.Person': ''},
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
        layout: {type: 'table', columns: 4},
        style: {marginTop: '6px', marginBottom: '6px'},
        defaults: {labelAlign: 'right', readOnly: true, width: 300},
        defaultType: 'libTextField',
        items: [{
          xtype: 'libSearchfield',
          labelAlign: 'right',
          fieldLabel: '创建人',
          relSource: {'com.Person': ''},
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
          relSource: {'com.Person': ''},
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
          relSource: {'com.Person': ''},
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
          relSource: {'com.Person': ''},
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

  addImageTab();
  addFixTab(this);

  var gridPanel;
  if (vcl.tpl.Layout.GridRange != null) {
    gridPanel = Ax.tpl.GridManager.createGrid({
      vcl: vcl,
      parentRow: vcl.dataSet.getTable(0).data.items[0],
      tableIndex: vcl.dataSet.tableMap.get(vcl.tpl.Layout.GridRange.Store),
      curRange: vcl.tpl.Layout.GridRange,
      height: 600,
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

  var toolBarItems = [];
  if (toolBarAction) {
    for (var i = 0; i < toolBarAction.length; i++) {
      toolBarItems.push(Ext.create(Ext.button.Button, toolBarAction[i]));
    }
  }
  var inputPanel = Ext.create('Ext.panel.Panel', {
    anchor: inputAnchor,
    layout: {type: 'vbox', align: 'stretch'},
    items: [panel, funPanel, tabPanel, gridPanel],
    border: false
  });
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
