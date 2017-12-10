/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的目录的View自定义页面
 * 创建标识：Zhangkj 2016/12/05
 *
 *
 ************************************************************************/
dmDirectoryView = function () {
  Ax.tpl.LibBillTpl.apply(this, arguments);
  if (this.vcl.funcView.containsKey("default")) {
    this.vcl.funcView.get("default").name = "onReady";
  }
  //只有第一列的双击才可以打开
  this.canExpend = true;
}
var proto = dmDirectoryView.prototype = Object.create(Ax.tpl.LibBillTpl.prototype);
proto.constructor = dmDirectoryView;
proto.onReady = function (billAction, curPks, dirType, dmToolBar) {
  var me = this;

  var vcl = this.vcl;

  vcl.forms = [];
  vcl.billAction = billAction;
  if (curPks && curPks.length > 0 && curPks[0])
    vcl.currentPk = curPks;

  if (vcl.dataSet.dataList && vcl.dataSet.dataList[1]) {
    vcl.dataSet.getTable(1).ownGrid = null;
  }

  if (vcl.billAction == BillActionEnum.AddNew) {
    //if (!vcl.addNew())
    //    return;
    //vcl.isEdit = true;
    //dmToolBar.callAddNew({ noOpen: true });
    if (vcl.addNew() == true) {
      vcl.isEdit = true;
      dmToolBar.callAddNew({noOpen: true});
    }
    else {
      if (vcl.currentPk && vcl.currentPk.length > 0 && vcl.currentPk[0])
        vcl.browseTo(vcl.currentPk);
    }
  } else if (vcl.billAction == BillActionEnum.Edit) {
    vcl.browseTo(vcl.currentPk);
    dmToolBar.callEdit();
  } else {
    if (vcl.currentPk && vcl.currentPk.length > 0 && vcl.currentPk[0])
      vcl.browseTo(vcl.currentPk);
  }

  //var progId = vcl.progId;
  var storeDataLength = 0;
  var store = null;
  if (vcl.dataSet.dataList) {
    store = vcl.dataSet.getTable(0);
    storeDataLength = store.data.items.length;
  }

  //if (storeDataLength == 0)
  //    return null;//未获取到数据则直接返回

  //目录信息面板
  var panel = Ext.create('Ext.form.Panel', {
    border: false,
    tableIndex: 0,
    margin: '4 2 4 2',
    autoDestory: true,
    autoScroll: true,
    items: Ext.decode(vcl.tpl.Layout.HeaderRange.Renderer)
  });
  vcl.forms.push(panel);
  if (storeDataLength > 0)
    panel.loadRecord(store.data.items[0]);

  var tabPanel = Ext.create('Ext.tab.Panel', {
    border: false,
    activeTab: 0,
    flex: vcl.tpl.Layout.GridRange == null ? 1 : undefined,
    //tbar: Ax.utils.LibToolBarBuilder.createToolBar(toolBarAction),
    //tbar: dmToolBar,
    autoDestory: true,
    autoScroll: false,
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

  addTab(panel, "目录信息");//目录信息Panel作为第一个标签页

  if (storeDataLength > 0) {
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
        else if (tabRange[i].BlockType == BlockTypeEnum.Grid) {
          if (dirType == DirTypeEnum.Private && tabRange[i].TableIndex == 1)
            continue;//私有目录不显示目录权限页
          var grid = Ax.tpl.GridManager.createGrid({
            vcl: vcl,
            parentRow: vcl.dataSet.getTable(0).data.items[0],
            tableIndex: vcl.dataSet.tableMap.get(tabRange[i].Store),
            curRange: tabRange[i],
          });
          addTab(grid, tabRange[i].DisplayName);
        }
      }
    }
  }
  function addValidityTab (me) {
    var tempPanel = Ext.create('Ext.form.Panel', {
      border: false,
      tableIndex: 0,
      margin: '4 0 4 2',
      defaultType: 'textfield',
      autoScroll: true,
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
    if (storeDataLength > 0)
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
      autoScroll: true,
      tableIndex: 0,
      margin: '4 0 4 2',
      defaultType: 'textfield',
      items: items
    });
    if (storeDataLength > 0)
      tempPanel.loadRecord(store.data.items[0]);
    me.vcl.forms.push(tempPanel);
    addTab(tempPanel, '制单信息');

    tempPanel = Ext.create('Ext.form.Panel', {
      border: false,
      tableIndex: 0,
      margin: '4 0 4 2',
      autoScroll: true,
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
    if (storeDataLength > 0)
      tempPanel.loadRecord(store.data.items[0]);
    me.vcl.forms.push(tempPanel);
    addTab(tempPanel, '备注');
  };
  addFixTab(this);

  var inputAnchor = '100% 100%';

  return tabPanel;
};

