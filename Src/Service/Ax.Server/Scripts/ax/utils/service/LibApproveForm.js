//在GridPanel的afterRender方法上加上一段代码，通过增加一个配置项cellTip(true/false)来控制是否启用Tooltip功能。
//而且整个GridPanel只使用一个Ext.ToolTip实例，在显示的时候将内容替换为单元格的内容，这样做还有一个好处是能保留原来内容的所有格式。
//此段代码只需要加载一次，就会对所有的GridPanel产生作用，使用时，只需要在GridPanel里增加一个配置 cellTip:true 即可实现ToolTip功能：
Ext.override(Ext.grid.GridPanel, {
  afterRender: Ext.Function.createSequence(Ext.grid.GridPanel.prototype.afterRender,
    function () {
      if (!this.cellTip) {
        return;
      }
      try {
        var view = this.getView();
        this.tip = new Ext.ToolTip({
          target: view.el,
          delegate: '.x-grid-cell-inner',
          trackMouse: true,
          autoScroll: true,
          width: 150,
          height: 100,
          renderTo: document.body,
          listeners: {
            beforeshow: function updateTipBody(tip) {
              if (Ext.isEmpty(tip.triggerElement.innerText) || tip.triggerElement.innerText.length < 5) {
                return false;
              }
              if (tip.triggerElement.innerText.length > 400) {
                tip.showDelay = 3000;
                tip.width = 400;
                tip.height = 300;
              }
              else if (tip.triggerElement.innerText.length > 200) {
                tip.showDelay = 2500;
                tip.width = 350;
                tip.height = 250;
              }
              else if (tip.triggerElement.innerText.length > 100) {
                tip.showDelay = 2000;
                tip.width = 300;
                tip.height = 200;
              }
              else if (tip.triggerElement.innerText.length > 50) {
                tip.showDelay = 1500;
                tip.width = 300;
                tip.height = 150;
              }
              else if (tip.triggerElement.innerText.length > 20) {
                tip.showDelay = 1000;
                tip.width = 250;
                tip.height = 150;
              }
              else {
                tip.showDelay = 500;
                tip.width = 150;
                tip.height = 100;
              }

              var html = tip.triggerElement.innerText.replace(new RegExp('【br】', 'gm'), '<br>');
              // var html = tip.triggerElement.innerText.replaceAll('[br]', '<br>');
              tip.body.dom.innerHTML = html;
            }
          }
        });
      } catch (e) {
        console.log(e);
      }
    })
});
Ax.utils.LibApproveForm = function (curVcl,showConfig) {
  this.vcl = curVcl;
  this.win = null;
  if (showConfig)
    this.showConfig = showConfig;//可以选择查看审核流配置信息
  else
    this.showConfig = false;//默认点击时是根据提交人的信息查看具体审核流程
  this.nextLevel = 1;
  var modelName = "LibApproveBillFlowModel";
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
        { name: 'AUDITOPINION' },
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
  this.masterRow = this.vcl.dataSet.getTable(0).data.items[0].data;
  var flowData = this.vcl.invorkBcf('GetApproveFlowForBill', [this.masterRow, this.showConfig]);
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
      }
    }
  }
}
Ax.utils.LibApproveForm.prototype = {
  show: function () {
    var me = this;
    var funButton = [{
      xtype: 'toolbar',
      items: [{
        iconCls: 'gridAdd',
        handler: function () {
          me.store.add({ FLOWLEVEL: me.nextLevel, PERSONID: '', PERSONNAME: '', POSITION: '', INDEPENDENT: false, ISPASS: false, AUDITOPINION:'' });
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
          var flowList = {};
          if (me.store.data.items.length<=0) {
            Ext.Msg.alert("提示", "审核流程不能为空");
          }
          if (me.vcl.billAction==2) {
            Ext.Msg.alert("提示", "请先保存单据再变更审核流程");
          }
          me.store.each(function (record) {
            var personId = record.get('PERSONID');
            if (personId == '') {
              error = true;
              Ext.Msg.alert("提示", "审核人不能为空");
              return false;
            }
            var level = record.get('FLOWLEVEL');
            if (level <= curLevel) {
              error = true;
              Ext.Msg.alert("提示", "审核层级不能低于等于当前层级");
              return false;
            }
            var independent = record.get('INDEPENDENT');
            if (temp.containsKey(level))
              temp.get(level).push({ personId: personId, independent: independent });
            else
              temp.add(level, [{ personId: personId, independent: independent }])

            if (!flowList[level]) {
              flowList[level] = [];
            }
            flowList[level].push({
              FlowLevel: level, PersonId: record.get('PERSONID'),
              PersonName: record.get('PERSONNAME'), Position: record.get('POSITION'),
              Independent: record.get('INDEPENDENT'), IsPass: record.get('ISPASS')
            })
          });
          if (!error) {
            temp.sortByKey();
            var preKey = -1;
            temp.eachKey(function (key) {
              if (preKey == -1) {
                preKey = key;
                if (preKey != 1) {
                  error = true;
                  Ext.Msg.alert("提示", "最小层级必须从1开始");
                  return false;
                }
              } else if (++preKey != key) {
                error = true;
                Ext.Msg.alert("提示", "层级中间不能有断层");
                return false;
              }
              var dataObj = temp.get(key);
              if (dataObj.length == 1 && dataObj[0].independent) {
                error = true;
                Ext.Msg.alert("提示", "同一层级存在多人时才能勾选独立决策权");
                return false;
              }
              var hash = {};
              for (var i = 0; i < dataObj.length; i++) {
                if (hash[dataObj[i].personId]) {
                  error = true;
                  Ext.Msg.alert("提示", "同一层级不能出现相同的人");
                  return false;
                }
                hash[dataObj[i].personId] = true;
              }
            });
            if (!error) {
              var assistObj = {};
              me.vcl.invorkBcf('UpdateApproveFlow', [me.masterRow['INTERNALID'], me.masterRow['FLOWLEVEL'], flowList], assistObj);
              var success = (assistObj.hasError === undefined || !assistObj.hasError);
              if (success) {
                Ext.Msg.alert("提示", "变更成功。");
                me.win.close();
              }
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
              Ext.Msg.alert("提示", "已存在相关审核流程记录,不能进行当前操作。");
              return false;
            }
          });
          if (!error) {
            var masterRow = me.vcl.dataSet.getTable(0).data.items[0].data;
            var flowData = me.vcl.invorkBcf('GetDefaultFlow', [masterRow]);
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
        //}, {
        //    text: '发送审核提醒',
        //    handler: function () {

        //    }
      }]
    }];
    var id = Ext.id();
    DesktopApp.ActiveWindow = id;
    var me = this;
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
        editor: {
          xtype: 'libNumberField',
          tableIndex: 0,
          allowDecimals: false,
          minValue: 1
        }
      }, {
        text: '审核人代码',
        dataIndex: 'PERSONID',
        tableIndex: 0,
        xtype: 'templatecolumn',
        tpl: '<tplif=\"PERSONID!=&quot;&quot;&&PERSONID!=undefined&&PERSONNAME!=&quot;&quot;&&PERSONNAME!=undefined\">{PERSONID},{PERSONNAME}</tpl>',
        editor: {
          xtype: 'libSearchfield',
          tableIndex: 0,
          relSource: { 'com.Person': '' },
          relName: 'PERSONNAME',
          relPk: '',
          selParams: [],
          selectFields: 'A.POSITION'
        }
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
        editor: {
          xtype: 'libCheckboxField',
          tableIndex: 0
        }
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
          renderer : function(value){
            return '<span data-qtip="'+value+'">'+value+'</span>';
          }
        }
      ],
      store: this.store
    });
    this.win = Ext.create('Ext.window.Window', {
      id: id,
      title: '单据审核流程',
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


Ax.utils.LibApproveWithdrawForm = {
  show: function (curVcl) {
    var me = this;
    var vcl = curVcl;
    var downLevel = Ext.create('Ext.form.field.Number', {
      fieldLabel: '退回层级',
      disabled: true,
      allowDecimals: false,
      minValue: 0,
      text:0,
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
    var hidden = curVcl.dataSet.getTable(0).data.items[0].get('AUDITSTATE') != 2;
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
        var success = false;
        var reasonId = changeReasonId.getValue();
        if (hidden === false && (reasonId == null || reasonId == '')) {
          Ext.Msg.alert("提示", "变更原因不能为空。");
          return;
        }
        if (isUseDefine.getValue()) {
          var level = downLevel.getValue();
          if (level == undefined || level <= -1)
            level = 0;//默认打回到第0层
          success = vcl.doCancelAudit(level, reasonId);
        } else {
          success = vcl.doCancelAudit(0, reasonId);//默认打回到第0层
        }
        if (success === true)
          me.win.close();
      }
    });
    var formPanel = Ext.create('Ext.form.Panel', {
      bodyPadding: 10,
      layout: { type: 'vbox', align: 'stretch' },
      items: [isUseDefine, downLevel,changeReasonId, button]
    });
    this.win = Ext.create('Ext.window.Window', {
      title: '审核退回',
      autoScroll: true,
      width: 300,
      height: 200,
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

Ax.utils.LibApproveConfirmForm = {
  show: function (curVcl) {
    var me = this;
    var vcl = curVcl;
    var downLevel = Ext.create('Ext.form.field.Number', {
      fieldLabel: '退回层级',
      disabled: true,
      allowDecimals: false,
      minValue: 0
    });
    var auditOpinion = Ext.create('Ext.form.field.TextArea', {
      fieldLabel: '审核意见',
      height:75,
      maxLength: 100 // 最大长度100
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
    var approveRadioGroup = Ext.create('Ext.form.RadioGroup', {
      fieldLabel: '审核结果',
      labelAlign: 'top',
      columns: 2,
      margin: '0 0 20 0',
      items: [
        { boxLabel: '审核通过', name: 'ret', inputValue: 'pass', checked: true },
        { boxLabel: '审核不通过', name: 'ret', inputValue: 'unPass' },
      ]
    });
    var button = Ext.create('Ext.button.Button', {
      text: '确定',
      handler: function () {
        var items = approveRadioGroup.getValue();
        var opinion = auditOpinion.getValue();
        var success = false;
        if (items['ret'] == 'pass') {
          success = vcl.doAudit(true, -1, opinion);
        } else {
          if (isUseDefine.getValue()) {
            var level = downLevel.getValue();
            if (level == undefined || level <= -1)
              level = 0;//默认打回到第0层
            success = vcl.doAudit(false, level, opinion);
          } else {
            success = vcl.doAudit(false, 0, opinion);//默认打回到第0层
          }
        }
        if (success === true)
          me.win.close();
      }
    });
    var formPanel = Ext.create('Ext.form.Panel', {
      bodyPadding: 10,
      layout: { type: 'vbox', align: 'stretch' },
      items: [approveRadioGroup, isUseDefine, downLevel, auditOpinion, button]
    });
    this.win = Ext.create('Ext.window.Window', {
      title: '审核确认',
      autoScroll: true,
      width: 300,
      height: 285,
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


Ax.utils.LibApproveVersionForm = {
  show: function (curVcl) {
    var me = this;
    var vcl = curVcl;
    var modelName = "LibApproveVersionModel";
    var modelType = Ext.data.Model.schema.getEntity(modelName);
    if (modelType === null) {
      modelType = Ext.define(modelName, {
        extend: 'Ext.data.Model',
        fields: [{ name: 'Version' },
          { name: 'CreateTime', type: 'number' },
          { name: 'ReasonId' },
          { name: 'ReasonName' },
          { name: 'InternalId' }],
        proxy: {
          type: 'memory',
          reader: {
            type: 'json'
          }
        }
      });
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
    var internalId = vcl.dataSet.getTable(0).data.items[0].get('INTERNALID');
    var list = vcl.invorkBcf('GetBillVersionList', [internalId]);
    if (list != undefined) {
      for (var i = 0; i < list.length; i++) {
        store.add(list[i]);
      }
    }
    var createTime = new Ax.ux.form.LibDatecolumn({
      text: '时间',
      dataIndex: 'CreateTime',
      axT: 0,
      width: 150
    });
    var gridPanel = Ext.create('Ext.grid.Panel', {
      columns: [{
        xtype: 'rownumberer',
        width: 50
      }, {
        text: '历史版本',
        dataIndex: 'Version'
      }, createTime, {
        text: '变更原因',
        dataIndex: 'ReasonId',
        xtype: 'templatecolumn',
        tpl: '<tpl if=\"ReasonId != &quot;&quot; && ReasonId!=undefined && ReasonName != &quot;&quot; && ReasonName != undefined\">{ReasonId},{ReasonName}</tpl>'
      }, {
        text: '(变更原因名称)',
        dataIndex: 'ReasonName',
        width: 150
      }, {
        text: '(内码)',
        dataIndex: 'InternalId',
        width: 150,
        hidden: true
      }, {
        text: '浏览',
        xtype: 'actioncolumn',
        width: 50,
        flex: 1,
        align: 'center',
        items: [{
          iconCls: 'icon-grid',
          altText: '浏览',
          tooltip: '浏览',
          handler: function (view, recordIndex, cellIndex, item, e, record, row) {
            if (record) {
              var createTime = record.get('CreateTime');
              var internalId = record.get('InternalId');
              Ax.utils.BillManager.openVersionBill(vcl.progId, vcl.currentPk, { InternalId: internalId, VersionTime: createTime, Vcl: vcl });
            }
          }
        }]
      }],
      store: store
    });
    this.win = Ext.create('Ext.window.Window', {
      title: '审核确认',
      autoScroll: true,
      width: 650,
      height: 300,
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


