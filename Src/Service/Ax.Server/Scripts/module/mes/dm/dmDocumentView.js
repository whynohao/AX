/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的文档的View自定义页面
 * 创建标识：Zhangkj 2016/12/05
 *
 *
 ************************************************************************/
dmDocumentView = function () {
  Ax.tpl.LibBillTpl.apply(this, arguments);
  if (this.vcl.funcView.containsKey("default")) {
    this.vcl.funcView.get("default").name = "onReady";
  }
  //只有第一列的双击才可以打开
  this.canExpend = true;
}
var proto = dmDocumentView.prototype = Object.create(Ax.tpl.LibBillTpl.prototype);
proto.constructor = dmDocumentView;
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
    if (!vcl.addNew())
      return;
    vcl.isEdit = true;
    dmToolBar.callAddNew({ noOpen: true });
  } else if (vcl.billAction == BillActionEnum.Edit) {
    vcl.browseTo(vcl.currentPk);
    dmToolBar.callEdit();
  } else {
    vcl.browseTo(vcl.currentPk);
  }
  //var progId = vcl.progId;
  var store = vcl.dataSet.getTable(0);

  //文档信息面板
  var panel = Ext.create('Ext.form.Panel', {
    border: false,
    tableIndex: 0,
    margin: '4 2 4 2',
    autoDestory: true,
    autoScroll: true,
    items: Ext.decode(vcl.tpl.Layout.HeaderRange.Renderer)
  });
  vcl.forms.push(panel);
  panel.loadRecord(store.data.items[0]);

  var tabPanel = Ext.create('Ext.tab.Panel', {
    border: false,
    activeTab: 0,
    flex: vcl.tpl.Layout.GridRange == null ? 1 : undefined,
    //tbar: dmToolBar,
    autoDestory: true,
    autoScroll: true,
    defaults: {
      bodyPadding: 0
    }
  });
  function addTab(panel, displayName) {
    tabPanel.add({
      iconCls: 'tabs',
      layout: 'fit',
      items: panel,
      title: displayName
    });
  }

  addTab(panel, "文档信息");//信息Panel作为第一个标签页
  //构造修订版管理的顶部工具栏
  function createModifyToolbar(modifyGrid) {
    //查看
    var view = Ext.create(Ext.Action, {
      text: '查看',
      handler: function () {
        var rows = modifyGrid.getSelectionModel().getSelection();
        if (rows.length == 0) {
          Ext.Msg.alert("提示", "请选中需要操作的修订版。");
          return;
        }

        var docId = rows[0].data.DOCID;
        var modifyId = rows[0].data.DOCMODIFYID;
        if (vcl.checkCan('', docId, DMPermissonEnum.Read)) {
          vcl.AddDocOpLog(docId, '阅读文档。修订版：' + modifyId);

          var docType = store.data.items[0].DOCTYPE;
          var fileWindow = Ext.create('DocumentManage.DMFileWindowForm', {});
          fileWindow.readOnly(docId, modifyId, docType);
        }
      }
    });
    //回退
    var fallback = Ext.create(Ext.Action, {
      text: '回退',
      handler: function () {
        var rows = modifyGrid.getSelectionModel().getSelection();
        if (rows.length == 0) {
          Ext.Msg.alert("提示", "请选中需要操作的修订版。");
          return;
        }
        var docId = rows[0].data.DOCID;              //文档编号
        var modifyId = rows[0].data.DOCMODIFYID;
        if (vcl.checkCan('', docId, DMPermissonEnum.Fallback)) {
          Ext.Msg.prompt("请确认回退修订版", "确定要回退至选定的修订版本吗?<br />操作说明:", function (btn, text) {
            if (btn == 'ok') {
              if(vcl.FallBackVersion(docId, modifyId, text))
              {
                Ext.Msg.alert('提示', '回退成功。');
              }
            }
            else { }
          }, this, false, "");
        }
      }
    });
    //下载
    var download = Ext.create(Ext.Action, {
      text: '下载',
      handler: function () {
        var rows = modifyGrid.getSelectionModel().getSelection();
        if (rows.length == 0) {
          Ext.Msg.alert("提示", "请选中需要操作的修订版。");
          return;
        }
        var docId = rows[0].data.DOCID;              //文档编号
        var modifyId = rows[0].data.DOCMODIFYID;
        if (vcl.checkCan('', docId, DMPermissonEnum.Download)) {

          vcl.AddDocOpLog(docId, '下载了文档。修订版:' + modifyId);

          var url = '/document/Download';
          url += '?docId=' + docId + '&userHandle=' + UserHandle + '&modifyVerId=' + modifyId;

          var win = Ext.create('Ext.window.Window', {
            autoScroll: true,
            width: 100,
            height: 20,
            layout: 'fit',
            constrainHeader: true,
            minimizable: true,
            maximizable: true,
            contentEl: Ext.DomHelper.append(document.body, {
              tag: 'iframe',
              style: "border 0px none;scrollbar:true",
              src: url,
              height: "100%",
              width: "100%"
            })
          });
          //win.show();//窗口不需要显示，内置的iframe因为地址指向了下载地址就能自动下载文件
        }
      }
    });
    //Action列表
    var items =[view, fallback, download,];//修订版的操作按钮
    var toolBaritems = [];
    for (var i = 0; i < items.length; i++) {
      if (items[i].initialConfig.menu) {
        toolBaritems.push(Ext.create('Ext.button.Split', items[i]));
      }
      else {
        toolBaritems.push(Ext.create('Ext.button.Button', items[i]));
      }
    }
    return toolBaritems;
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
      }
      else if (tabRange[i].BlockType == BlockTypeEnum.Grid) {
        if (tabRange[i].tableIndex == 1) {

        }
        var grid = Ax.tpl.GridManager.createGrid({
          vcl: vcl,
          parentRow: vcl.dataSet.getTable(0).data.items[0],
          tableIndex: vcl.dataSet.tableMap.get(tabRange[i].Store),
          curRange: tabRange[i],
          isEditGrid: tabRange[i].TableIndex==1,//仅“文档权限”表显示添加和删除按钮
        });
        if (tabRange[i].TableIndex != 4)
          addTab(grid, tabRange[i].DisplayName);
        else {
          //修订版管理Grid上面加上版本查看、回退、下载的按钮。
          var panelModify = Ext.create('Ext.form.Panel', {
            border: false,
            margin: '4 2 4 2',
            autoDestory: true,
            autoScroll: true,
            tbar: createModifyToolbar(grid),//构造修订版管理的顶部工具栏
            items: [grid]
          });
          addTab(panelModify, tabRange[i].DisplayName);
        }
      }
    }
  }
  function addValidityTab(me) {
    var tempPanel = Ext.create('Ext.form.Panel', {
      border: false,
      tableIndex: 0,
      margin: '4 0 4 2',
      defaultType: 'textfield',
      items: {
        xtype: 'container',
        layout: { type: 'table', columns: 4 },
        style: { marginTop: '6px', marginBottom: '6px' },
        defaults: { labelAlign: 'right' },
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
  function addFixTab(me) {
    var items;
    if (me.vcl.billType == BillTypeEnum.Master) {
      addValidityTab(me);
      items = {
        xtype: 'container',
        layout: { type: 'table', columns: 4 },
        style: { marginTop: '6px', marginBottom: '6px' },
        defaults: { labelAlign: 'right', readOnly: true, width: 300 },
        defaultType: 'libTextField',
        items: [{
          xtype: 'libSearchfield',
          labelAlign: 'right',
          fieldLabel: '创建人',
          relIndex: 0,
          relSource: { 'com.Person': '' },
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
          relSource: { 'com.Person': '' },
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
          relSource: { 'com.Person': '' },
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
        layout: { type: 'table', columns: 4 },
        style: { marginTop: '6px', marginBottom: '6px' },
        defaults: { labelAlign: 'right', readOnly: true, width: 300 },
        defaultType: 'libTextField',
        items: [{
          xtype: 'libSearchfield',
          labelAlign: 'right',
          fieldLabel: '创建人',
          relSource: { 'com.Person': '' },
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
          relSource: { 'com.Person': '' },
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
          relSource: { 'com.Person': '' },
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
          relSource: { 'com.Person': '' },
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
  addFixTab(this);

  var inputAnchor = '100% 100%';

  return tabPanel;
};
