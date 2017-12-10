/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：具体行的行审核的审核流程查看和修改
 * 修改标识：Zhangkj 2017/03/28
 *
 ************************************************************************/
Ax.utils.LibApproveFlowForm = function (curVcl, approveRowForm, curRow, canChange,showConfig) {
  this.vcl = curVcl;
  this.win = null;
  this.approveRowForm = approveRowForm;
  this.curRow = curRow;
  this.canChange = canChange;
  this.curRowId = curRow.get('ROW_ID');
  this.nextLevel = 1;
  if (showConfig == undefined)
    showConfig = false;
  this.showConfig = showConfig;//是否仅显示流程配置
  if (this.approveRowForm != undefined && this.approveRowForm.flowStoreList.containsKey(this.curRowId)) {
    this.store = this.approveRowForm.flowStoreList.get(this.curRowId);
  } else {
    var modelName = "LibApproveFlowModel";
    var modelType = Ext.data.Model.schema.getEntity(modelName);
    if (modelType === null) {
      modelType = Ext.define(modelName, {
        extend: 'Ext.data.Model',
        fields: [{ name: 'FLOWLEVEL', type: 'number' },
          { name: 'PERSONID' },
          { name: 'PERSONNAME' },
          { name: 'POSITION' },
          { name: 'INDEPENDENT', type: 'boolean' },
          { name: 'ISPASS', type: 'boolean' },

          { name: 'DEPTID' },
          { name: 'DEPTNAME' },
          { name: 'DUTYID' },
          { name: 'DUTYNAME' },
          { name: 'ISJUMP', type: 'boolean' },
          { name: 'JUMPREASON' },
          { name: 'ISSAMEDEFAULT', type: 'boolean' },
          { name: 'EXECUTEDESC' },//执行过程的描述
        ],
        proxy: {
          type: 'memory',
          reader: {
            type: 'json'
          }
        }
      });
    }
    this.store = Ext.create('Ext.data.Store', {
      model: modelType,
      proxy: {
        type: 'memory',
        reader: {
          type: 'json'
        }
      }
    });
    var masterRow = this.vcl.dataSet.getTable(0).data.items[0].data;
    var flowData = this.vcl.invorkBcf('GetApproveFlow', [masterRow, this.curRow.data,this.showConfig]);
    if (flowData) {
      for (p in flowData) {
        if (!flowData.hasOwnProperty(p))
          continue;
        var detalis = flowData[p];
        for (var i = 0; i < detalis.length; i++) {
          var temp = detalis[i];
          if (temp.IsJump != undefined) {
            //返回结果包含跳过原因等字段
            this.store.add({
              FLOWLEVEL: p, PERSONID: temp.PersonId, PERSONNAME: temp.PersonName, POSITION: temp.Position,
              INDEPENDENT: temp.Independent, ISPASS: temp.IsPass, AUDITOPINION: temp.AuditOpinion,
              DEPTID: temp.DeptId, DEPTNAME: temp.DeptName, DUTYID: temp.DutyId, DUTYNAME: temp.DutyName, ISJUMP: temp.IsJump, JUMPREASON: temp.JumpReason, ISSAMEDEFAULT: temp.IsSameDefault, EXECUTEDESC: temp.ExecuteDesc
            });
          } else {
            this.store.add({
              FLOWLEVEL: p, PERSONID: temp.PersonId, PERSONNAME: temp.PersonName, POSITION: temp.Position,
              INDEPENDENT: temp.Independent, ISPASS: temp.IsPass, AUDITOPINION: temp.AuditOpinion
            });
          }
          //this.store.add({ FLOWLEVEL: p, PERSONID: temp.PersonId, PERSONNAME: temp.PersonName, POSITION: temp.Position, INDEPENDENT: temp.Independent, ISPASS: temp.IsPass });
        }
      }
    }
  }
}
Ax.utils.LibApproveFlowForm.prototype = {
  show: function () {
    var me = this;
    var funButton = me.canChange === true ? [{
        xtype: 'toolbar',
        items: [{
          iconCls: 'gridAdd',
          handler: function () {
            me.store.add({ FLOWLEVEL: me.nextLevel, PERSONID: '', PERSONNAME: '', POSITION: '', INDEPENDENT: false, ISPASS: false });
          }
        }, {
          iconCls: 'gridDelete',
          handler: function () {
            var records = gridPanel.getView().getSelectionModel().getSelection();
            if (records.length > 0)
              me.store.remove(records)
          }
        }, {
          text: '确认变更',
          handler: function () {
            var error = false;
            var curLevel = me.nextLevel - 1;
            var temp = new Ext.util.MixedCollection();
            me.store.each(function (record) {
              var personId = record.get('PERSONID');
              if (personId == '') {
                error = true;
                alert('审核人不能为空');
                return false;
              }
              var level = record.get('FLOWLEVEL');
              if (level <= curLevel) {
                error = true;
                alert('审核层级不能低于等于当前层级');
                return false;
              }
              var independent = record.get('INDEPENDENT');
              if (temp.containsKey(level))
                temp.get(level).push({ personId: personId, independent: independent });
              else
                temp.add(level, [{ personId: personId, independent: independent }])
            });
            if (!error) {
              temp.sortByKey();
              var preKey = -1;
              temp.eachKey(function (key) {
                if (preKey == -1) {
                  preKey = key;
                  if (preKey != 1) {
                    error = true;
                    alert('最小层级必须从1开始');
                    return false;
                  }
                } else if (++preKey != key) {
                  error = true;
                  alert('层级中间不能有断层');
                  return false;
                }
                var dataObj = temp.get(key);
                if (dataObj.length == 1 && dataObj[0].independent) {
                  error = true;
                  alert('同一层级存在多人时才能勾选独立决策权');
                  return false;
                }
                var hash = {};
                for (var i = 0; i < dataObj.length; i++) {
                  if (hash[dataObj[i].personId]) {
                    error = true;
                    alert('同一层级不能出现相同的人');
                    return false;
                  }
                  hash[dataObj[i].personId] = true;
                }
              });
              if (!error) {
                if (me.approveRowForm.flowStoreList.containsKey(me.curRowId)) {
                  me.approveRowForm.flowStoreList.replace(me.curRowId, me.store);
                } else {
                  me.approveRowForm.flowStoreList.add(me.curRowId, me.store);
                }
                me.win.close();
              }
            }
          }
        }, {
          text: '载入系统流程',
          handler: function () {
            var error = false;
            me.store.each(function (record) {
              var isPass = record.get('ISPASS');
              if (isPass) {
                error = true;
                alert('已存在相关审核流程记录,不能进行当前操作。');
                return false;
              }
            });
            if (!error) {
              var masterRow = me.vcl.dataSet.getTable(0).data.items[0].data;
              var flowData = me.vcl.invorkBcf('GetDefaultApproveFlow', [masterRow, me.curRow.data]);
              if (flowData) {
                me.store.removeAll();
                for (p in flowData) {
                  if (!flowData.hasOwnProperty(p))
                    continue;
                  var detalis = flowData[p];
                  for (var i = 0; i < detalis.length; i++) {
                    var temp = detalis[i];
                    if (temp.IsJump != undefined) {
                      //返回结果包含跳过原因等字段
                      me.store.add({
                        FLOWLEVEL: p, PERSONID: temp.PersonId, PERSONNAME: temp.PersonName, POSITION: temp.Position,
                        INDEPENDENT: temp.Independent, ISPASS: temp.IsPass, AUDITOPINION: temp.AuditOpinion,
                        DEPTID: temp.DeptId, DEPTNAME: temp.DeptName, DUTYID: temp.DutyId, DUTYNAME: temp.DutyName, ISJUMP: temp.IsJump, JUMPREASON: temp.JumpReason, ISSAMEDEFAULT: temp.IsSameDefault, EXECUTEDESC: temp.ExecuteDesc
                      });
                    } else {
                      me.store.add({
                        FLOWLEVEL: p, PERSONID: temp.PersonId, PERSONNAME: temp.PersonName, POSITION: temp.Position,
                        INDEPENDENT: temp.Independent, ISPASS: temp.IsPass, AUDITOPINION: temp.AuditOpinion
                      });
                    }
                    //me.store.add({ FLOWLEVEL: p, PERSONID: temp.PersonId, PERSONNAME: temp.PersonName, POSITION: temp.Position, INDEPENDENT: temp.Independent, ISPASS: temp.IsPass });
                  }
                }
              }
            }
          }
        }, {
          text: '取消',
          handler: function () {
            me.win.close();
          }
        }]
      }] : [];

    var id = Ext.id();
    DesktopApp.ActiveWindow = id;
    var gridPanel = Ext.create('Ext.grid.Panel', {
      cellTip: true,//添加这个配置项，实现鼠标悬停的提示效果
      plugins: [
        Ext.create('Ext.grid.plugin.CellEditing', {
          clicksToEdit: 1
        })
      ],
      dockedItems: funButton,
      columns: [{
        xtype: 'rownumberer',
        width: 50
      }, {
        text: '审核层级',
        dataIndex: 'FLOWLEVEL',
        tableIndex: 0,
        xtype: 'libNumbercolumn',
        editor: this.canChange === true ? {
            xtype: 'libNumberField',
            tableIndex: 0,
            allowDecimals: false,
            minValue: 1
          } : null
      }, {
        text: '审核人代码',
        dataIndex: 'PERSONID',
        tableIndex: 0,
        xtype: 'templatecolumn',
        tpl: '<tplif=\"PERSONID!=&quot;&quot;&&PERSONID!=undefined&&PERSONNAME!=&quot;&quot;&&PERSONNAME!=undefined\">{PERSONID},{PERSONNAME}</tpl>',
        editor: this.canChange === true ? {
            xtype: 'libSearchfield',
            tableIndex: 0,
            relSource: { 'com.Person': '' },
            relName: 'PERSONNAME',
            relPk: '',
            selParams: [],
            selectFields: 'A.POSITION'
          } : null
      }, {
        text: '(审核人名称)',
        dataIndex: 'PERSONNAME',
        tableIndex: 0
      }, {
        text: '(职位)',
        dataIndex: 'POSITION',
        tableIndex: 0
      }, {
        text: '独立决策权',
        dataIndex: 'INDEPENDENT',
        tableIndex: 0,
        xtype: 'libCheckcolumn',
        editor: this.canChange === true ? {
            xtype: 'libCheckboxField',
            tableIndex: 0
          } : null
      },
        {
          text: '(部门)',
          dataIndex: 'DEPTNAME',
          tableIndex: 0
        },
        {
          text: '(岗位)',
          dataIndex: 'DUTYNAME',
          tableIndex: 0
        },
        {
          text: '(已通过)',
          dataIndex: 'ISPASS',
          tableIndex: 0,
          xtype: 'libCheckcolumn'
        },
        {
          text: '(审核意见)',
          dataIndex: 'AUDITOPINION',
          tableIndex: 0
        },
        {
          text: '(同人默认)',
          dataIndex: 'ISSAMEDEFAULT',
          tableIndex: 0,
          xtype: 'libCheckcolumn'
        },
        {
          text: '(是否跳过)',
          dataIndex: 'ISJUMP',
          tableIndex: 0,
          xtype: 'libCheckcolumn'
        },
        {
          text: '(跳过原因)',
          dataIndex: 'JUMPREASON',
          tableIndex: 0
        },
        {
          text: '(执行说明)',
          dataIndex: 'EXECUTEDESC',
          tableIndex: 0,
          renderer: function (value) {
            return '<span data-qtip="' + value + '">' + value + '</span>';
          }
        }
      ],
      store: this.store
    });
    this.win = Ext.create('Ext.window.Window', {
      id: id,
      title: '行项审核流程',
      autoScroll: true,
      width: 800,
      height: 400,
      layout: 'fit',
      constrainHeader: true,
      minimizable: false,
      maximizable: false,
      modal: true,
      items: [gridPanel]
    });
    this.win.show();
  }
};
