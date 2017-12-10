/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：行审核的提交记录、待审核记录、送审和弃审列表等的查看窗口
 * 修改标识：Zhangkj 2017/03/28
 *
 ************************************************************************/
Ax.utils.LibApproveRowForm = function (curVcl, tableName, renderer, grid) {
  this.vcl = curVcl;
  this.approveGrid = grid;
  this.win = null;
  this.columnsRenderer = renderer;
  this.flowStoreList = new Ext.util.MixedCollection();
  var modelName = !curVcl.tpl.Tables[tableName].IsDynamic ? (curVcl.tpl.ProgId + tableName) : (curVcl.tpl.ProgId + tableName + curVcl.winId);
  var modelType = Ext.data.Model.schema.getEntity(modelName);
  this.preSubmitStore = Ext.create('Ext.data.Store', {
    model: modelType,
    proxy: {
      type: 'memory',
      reader: {
        type: 'json'
      }
    }
  });
  this.preCancelStore = Ext.create('Ext.data.Store', {
    model: modelType,
    proxy: {
      type: 'memory',
      reader: {
        type: 'json'
      }
    }
  });
  this.sendApproveStore = Ext.create('Ext.data.Store', {
    model: modelType,
    proxy: {
      type: 'memory',
      reader: {
        type: 'json'
      }
    }
  });
  this.waitApproveStore = Ext.create('Ext.data.Store', {
    model: modelType,
    proxy: {
      type: 'memory',
      reader: {
        type: 'json'
      }
    }
  });
  this.initApproveStore = function () {
    this.waitApproveStore.removeAll();
    this.sendApproveStore.removeAll();
    var dataRowList = this.approveGrid.getStore().data.items;
    var rowList = [];
    var mapList = new Ext.util.MixedCollection();
    var internalId = this.vcl.dataSet.getTable(0).data.items[0].get('INTERNALID');
    for (var r = 0; r < dataRowList.length; r++) {
      var state = dataRowList[r].get('AUDITSTATE');
      if (state == 1) {
        this.sendApproveStore.add(dataRowList[r]);
        var rowId = dataRowList[r].get('ROW_ID');
        rowList.push(rowId);
        mapList.add(rowId, dataRowList[r]);
      }
    }
    function getWaitApproveRow(internalId, rowList) {
      var list = this.vcl.invorkBcf('GetWaitApproveRow', [internalId, rowList]);
      for (var i = 0; i < list.length; i++) {
        this.waitApproveStore.add(mapList.get(list[i]));
      }
    };
    if (rowList.length > 0)
      getWaitApproveRow.call(this, internalId, rowList);
  };
  this.initApproveStore();
}
Ax.utils.LibApproveRowForm.prototype = {
  show: function (tag) {
    var me = this;
    function removeData(grid) {
      var records = grid.getView().getSelectionModel().getSelection();
      if (records.length > 0)
        grid.getStore().remove(records)
    };
    function locationRecord(grid) {
      var record = grid.getView().getSelectionModel().getLastSelected();
      if (record) {
        me.approveGrid.getView().getSelectionModel().select(record);
        me.win.close();
      }
    };

    function auditRow(grid, cancel, downLevel) {
      var records = grid.getView().getSelectionModel().getSelection();
      var changeFlow = [];
      rowList = {};
      var unPassLevel;
      if (downLevel !== undefined && downLevel >= 0) {
        unPassLevel = {};
      }
      for (var i = 0; i < records.length; i++) {
        var rowId = records[i].get('ROW_ID');
        var flowList = {};
        if (me.flowStoreList.containsKey(rowId)) {
          changeFlow.push(rowId);
          var flowStore = me.flowStoreList.get(rowId);
          flowStore.each(function (info) {
            var level = info.get('FLOWLEVEL');
            if (!flowList[level]) {
              flowList[level] = [];
            }
            flowList[level].push({
              FlowLevel: info.get('FLOWLEVEL'), PersonId: info.get('PERSONID'),
              PersonName: info.get('PERSONNAME'), Position: info.get('POSITION'),
              Independent: info.get('INDEPENDENT'), IsPass: info.get('ISPASS')
            })
          });
        }
        rowList[rowId] = flowList;
        if (unPassLevel)
          unPassLevel[rowId] = downLevel;
      }
      if (me.vcl.auditRow(cancel, rowList, unPassLevel)) {
        me.initApproveStore();
        for (var i = 0; i < changeFlow.length; i++) {
          me.flowStoreList.removeAtKey(changeFlow[i]);
        }
      }
    };

    function cancelAuditRow(grid, downLevel, reasonId) {
      var records = grid.getView().getSelectionModel().getSelection();
      rowList = {};
      var unPassLevel;
      if (downLevel !== undefined && downLevel >= 0) {
        unPassLevel = {};
      }
      for (var i = 0; i < records.length; i++) {
        var rowId = records[i].get('ROW_ID');
        rowList[rowId] = {};
        if (unPassLevel)
          unPassLevel[rowId] = downLevel;
      }
      if (me.vcl.cancelAuditRow(rowList, unPassLevel, reasonId)) {
        for (var i = records.length - 1; i >= 0; i--) {
          me.preCancelStore.remove(records[i]);
        }
        me.initApproveStore();
      }
    };
    var columns = Ext.decode(this.columnsRenderer);
    var tabPanel = Ext.create('Ext.tab.Panel', {
      items: [{
        title: '我的待审核记录',
        id: 'waitApprove',
        layout: 'fit',
        items: [{
          xtype: 'gridpanel',
          selType: 'checkboxmodel',
          multiSelect: true,
          dockedItems: [{
            xtype: 'toolbar',
            items: [{
              text: '审核通过',
              handler: function () {
                var grid = this.up('grid');
                auditRow(grid, true);
              }
            }, {
              text: '审核不通过',
              handler: function () {
                var grid = this.up('grid');
                Ax.utils.LibApproveRowWithdrawForm.show(grid, auditRow, false);
              }
            }, {
              text: '刷新',
              handler: function () {
                me.initApproveStore();
              }
            }, {
              text: '变更审核流程',
              handler: function () {
                var grid = this.up('grid');
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length == 1) {
                  var flowForm = new Ax.utils.LibApproveFlowForm(me.vcl, me, records[0], true);
                  flowForm.show();
                }
              }
            }, {
              text: '定位到记录',
              handler: function () {
                locationRecord(this.up('grid'));
              }
            }]
          }],
          columns: columns,
          store: this.waitApproveStore
        }]
      }, {
        title: '已送审列表',
        id: 'sentApprove',
        layout: 'fit',
        items: [{
          xtype: 'gridpanel',
          selType: 'checkboxmodel',
          multiSelect: true,
          dockedItems: [{
            xtype: 'toolbar',
            items: [{
              text: '撤回',
              handler: function () {
                var grid = this.up('grid');
                var records = grid.getView().getSelectionModel().getSelection();
                rowList = {};
                for (var i = 0; i < records.length; i++) {
                  var rowId = records[i].get('ROW_ID');
                  rowList[rowId] = {};
                }
                if (me.vcl.submitApproveRow(true, rowList)) {
                  me.initApproveStore();
                }
              }
            }, {
              text: '查看审核流程',
              handler: function () {
                var grid = this.up('grid');
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length == 1) {
                  var flowForm = new Ax.utils.LibApproveFlowForm(me.vcl, me, records[0], false);
                  flowForm.show();
                }
              }
              //}, {
              //    text: '发送审核提醒',
              //    handler: function () {

              //    }
            }, {
              text: '定位到记录',
              handler: function () {
                locationRecord(this.up('grid'));
              }
            }]
          }],
          columns: columns,
          store: this.sendApproveStore
        }]
      }, {
        title: '本次送审列表',
        id: 'sendApprove',
        layout: 'fit',
        items: [{
          xtype: 'gridpanel',
          selType: 'checkboxmodel',
          multiSelect: true,
          dockedItems: [{
            xtype: 'toolbar',
            items: [{
              text: '提交审核',
              handler: function () {
                var grid = this.up('grid');
                var records = grid.getView().getSelectionModel().getSelection();
                var changeFlow = [];
                rowList = {};
                for (var i = 0; i < records.length; i++) {
                  var rowId = records[i].get('ROW_ID');
                  var flowList = {};
                  if (me.flowStoreList.containsKey(rowId)) {
                    changeFlow.push(rowId);
                    var flowStore = me.flowStoreList.get(rowId);
                    flowStore.each(function (info) {
                      var level = info.get('FLOWLEVEL');
                      if (!flowList[level]) {
                        flowList[level] = [];
                      }
                      flowList[level].push({
                        FlowLevel: info.get('FLOWLEVEL'), PersonId: info.get('PERSONID'),
                        PersonName: info.get('PERSONNAME'), Position: info.get('POSITION'),
                        Independent: info.get('INDEPENDENT'), IsPass: info.get('ISPASS')
                      })
                    });
                  }
                  rowList[rowId] = flowList;
                }
                if (me.vcl.submitApproveRow(false, rowList)) {
                  for (var i = 0; i < changeFlow.length; i++) {
                    me.flowStoreList.removeAtKey(changeFlow[i]);
                  }
                  for (var i = records.length - 1; i >= 0; i--) {
                    me.preSubmitStore.remove(records[i]);
                  }
                  me.initApproveStore();
                }
              }
            }, {
              text: '撤销',
              handler: function () {
                removeData(this.up('grid'));
              }
            }, {
              text: '变更审核流程',
              handler: function () {
                var grid = this.up('grid');
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length == 1) {
                  var flowForm = new Ax.utils.LibApproveFlowForm(me.vcl, me, records[0], true);
                  flowForm.show();
                }
              }
            }, {
              text: '定位到记录',
              handler: function () {
                locationRecord(this.up('grid'));
              }
            }]
          }],
          columns: columns,
          store: this.preSubmitStore
        }]
      }, {
        title: '本次弃审列表',
        id: 'sendUnApprove',
        layout: 'fit',
        items: [{
          xtype: 'gridpanel',
          selType: 'checkboxmodel',
          multiSelect: true,
          dockedItems: [{
            xtype: 'toolbar',
            items: [{
              text: '确定弃审',
              handler: function () {
                var grid = this.up('grid');
                Ax.utils.LibApproveRowWithdrawForm.show(grid, cancelAuditRow, true);
              }
            }, {
              text: '撤销',
              handler: function () {
                removeData(this.up('grid'));
              }
            }, {
              text: '查看审核流程',
              handler: function () {
                var grid = this.up('grid');
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length == 1) {
                  var flowForm = new Ax.utils.LibApproveFlowForm(me.vcl, me, records[0], false);
                  flowForm.show();
                }
              }
            }, {
              text: '定位到记录',
              handler: function () {
                locationRecord(this.up('grid'));
              }
            }]
          }],
          columns: columns,
          store: this.preCancelStore
        }]
      }]
    });
    this.win = Ext.create('Ext.window.Window', {
      title: '行项审核列表',
      autoScroll: true,
      width: 800,
      height: 400,
      layout: 'fit',
      constrainHeader: true,
      minimizable: false,
      maximizable: false,
      modal: true,
      //closeAction: 'hide',
      items: [tabPanel]
    });
    if (tag && tag != null) {
      var tab = this.win.down('tabpanel').items.get(tag);
      if (tab)
        tab.show();
    }
    this.win.show();
  }
};

Ax.utils.LibApproveRowWithdrawForm = {
  show: function (grid, callback, isCancelAuditRow) {
    var me = this;
    var downLevel = Ext.create('Ext.form.field.Number', {
      fieldLabel: '退回层级',
      disabled: true,
      allowDecimals: false,
      minValue: 0
    });
    var isUseDefine = Ext.create('Ext.form.field.Checkbox', {
      labelWidth: 200,
      fieldLabel: '是否特别指定退回审核层级(指定为0时,表示退回到起始层级)',
      listeners: {
        'change': function (self, newValue) {
          if (newValue) {
            downLevel.setDisabled(false);
          } else {
            downLevel.setDisabled(true);
          }
        }
      }
    });
    var hidden = true;
    if (isCancelAuditRow) {
      var records = grid.getView().getSelectionModel().getSelection();
      for (var i = 0; i < records.length; i++) {
        if (2 == records[i].get('AUDITSTATE')) {
          hidden = false;
          break;
        }
      }
    }
    var changeReasonId = new Ax.ux.form.LibSearchField({
      name: 'REASONID',
      fieldLabel: '变更原因',
      xtype: 'libSearchfield',
      labelStyle: 'color:#a7392e',
      tableIndex: 0,
      relSource: { 'axp.ChangeDataReason': '' },
      relName: 'REASONNAME',
      relPk: '',
      selParams: [],
      hidden: hidden
    });
    var button = Ext.create('Ext.button.Button', {
      text: '确定',
      handler: function () {
        var reasonId = changeReasonId.getValue();
        if (hidden === false && isCancelAuditRow === true && (reasonId == null || reasonId == '')) {
          alert('变更原因不能为空。');
          return;
        }
        if (isUseDefine.getValue()) {
          var level = downLevel.getValue();
          if (isCancelAuditRow === true)
            callback(grid, level, reasonId);
          else
            callback(grid, false, level);
        } else {
          if (isCancelAuditRow === true)
            callback(grid, undefined, reasonId);
          else
            callback(grid, false);
        }
        me.win.close();
      }
    });
    var formPanel = Ext.create('Ext.form.Panel', {
      bodyPadding: 10,
      layout: { type: 'vbox', align: 'stretch' },
      items: [isUseDefine, downLevel, changeReasonId, button]
    });
    this.win = Ext.create('Ext.window.Window', {
      title: '审核退回',
      autoScroll: true,
      width: 300,
      height: 220,
      layout: 'fit',
      constrainHeader: true,
      minimizable: false,
      maximizable: false,
      modal: true,
      items: [formPanel]
    });
    this.win.show();
  }
};

Ax.utils.LibApproveRowVersionForm = {
  show: function (curVcl, tableName, curRow, index) {
    var storeList = new Ext.util.MixedCollection();
    var gridPanelList = [];
    function buildStore(tbName, isSub, idx) {
      var modelName = curVcl.tpl.ProgId + tbName + "version";
      modelType = Ext.data.Model.schema.getEntity(modelName);
      if (isSub === false) {
        if (modelType === undefined) {
          var fields = Ext.decode(curVcl.tpl.Tables[tableName].Fields);
          fields.unshift({ name: 'ReasonName' });
          fields.unshift({ name: 'ReasonId' });
          fields.unshift({ name: 'CreateTime', type: 'number' });
          fields.unshift({ name: 'Version' });
          modelType = Ext.define(modelName, {
            extend: 'Ext.data.Model',
            fields: fields,
            proxy: {
              type: 'memory',
              reader: {
                type: 'json'
              }
            }
          });
        }
      } else {
        if (modelType === undefined) {
          var fields = Ext.decode(curVcl.tpl.Tables[tbName].Fields);
          fields.unshift({ name: 'Version' });
          modelType = Ext.define(modelName, {
            extend: 'Ext.data.Model',
            fields: fields,
            proxy: {
              type: 'memory',
              reader: {
                type: 'json'
              }
            }
          });
        }
      }
      var store = Ext.create('Ext.data.Store', {
        model: modelType,
        proxy: {
          type: 'memory',
          reader: {
            type: 'json'
          }
        }
      });
      storeList.add(tbName, store);
      var columns;
      var displayName;
      if (isSub === true) {
        columns = Ext.decode(curVcl.tpl.Layout.SubBill[idx].Renderer);
        displayName = curVcl.tpl.Layout.SubBill[idx].DisplayName;
        columns.unshift({
          text: '历史版本',
          dataIndex: 'Version'
        });
      } else {
        columns = Ext.decode(curVcl.tpl.Layout.GridRange.Renderer);
        displayName = curVcl.tpl.Layout.GridRange.DisplayName;
        var createTime = new Ax.ux.form.LibDatecolumn({
          text: '时间',
          dataIndex: 'CreateTime',
          axT: 0,
          width: 150
        });
        columns.unshift({
          text: '(变更原因名称)',
          dataIndex: 'ReasonName',
          width: 150
        });
        columns.unshift({
          text: '变更原因',
          dataIndex: 'ReasonId',
          xtype: 'templatecolumn',
          tpl: '<tpl if=\"ReasonId != &quot;&quot; && ReasonId!=undefined && ReasonName != &quot;&quot; && ReasonName != undefined\">{ReasonId},{ReasonName}</tpl>'
        });
        columns.unshift(createTime);
        columns.unshift({
          text: '历史版本',
          dataIndex: 'Version'
        });
      }
      var gridPanel = Ext.create('Ext.grid.Panel', {
        xtype: 'gridpanel',
        columns: columns,
        store: store,
        displayName: displayName
      });
      gridPanelList.push(gridPanel);
    };
    buildStore(tableName, false);
    for (field in curVcl.tpl.Tables[tableName].SubTableMap) {
      if (!curVcl.tpl.Tables[tableName].SubTableMap.hasOwnProperty(field))
        continue;
      var tbName = curVcl.dataSet.getTable(curVcl.tpl.Tables[tableName].SubTableMap[field]).Name;
      buildStore(tbName, true, curVcl.tpl.Tables[tableName].SubTableMap[field]);
    }
    var masterRow = curVcl.dataSet.getTable(0).data.items[0];
    var list = curVcl.invorkBcf('GetApproveRowVersion', [masterRow.get('INTERNALID'), curRow.get('ROW_ID')]);
    if (list) {
      for (var i = 0; i < list.length; i++) {
        var billDataRow = list[i].BillDataRow;
        var curStore = storeList.get(tableName);
        var record = curStore.add(billDataRow.Data)[0];
        record.set('Version', list[i].Version);
        record.set('CreateTime', list[i].CreateTime);
        record.set('ReasonId', list[i].ReasonId);
        record.set('ReasonName', list[i].ReasonName);
        curStore.sync();
        if (billDataRow.SubData) {
          for (tbName in billDataRow.SubData) {
            if (!billDataRow.SubData.hasOwnProperty(tbName))
              continue;
            var subRowList = billDataRow.SubData[tbName];
            var subStore = storeList.get(tbName);
            for (var r = 0; r < subRowList.length; r++) {
              var subRecord = subStore.add(subRowList[r])[0];
              subRecord.set('Version', list[i].Version);
            }
            subStore.sync();
          }
        }
      }
    }
    var tabPanel = Ext.widget('tabpanel', {
      activeTab: 0,
      layout: 'fit',
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
    for (var i = 0; i < gridPanelList.length; i++) {
      addTab(gridPanelList[i], gridPanelList[i].displayName);
    }

    var win = Ext.create('Ext.window.Window', {
      title: '行项审核版本',
      autoScroll: true,
      width: 800,
      height: 400,
      layout: 'fit',
      constrainHeader: true,
      minimizable: false,
      maximizable: false,
      modal: true,
      items: [tabPanel]
    });
    win.show();
  }
};

