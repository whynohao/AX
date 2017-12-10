


Ax.utils.LibQueryForm = {
  createForm: function (vcl, progId, defaultCondition, callback) {
    var id = Ext.id();
    DesktopApp.ActiveWindow = id;
    var progId = progId || vcl.progId;
    var selectFields = [];
    function getFields() {
      Ext.Ajax.request({
        url: 'billSvc/selectQueryField',
        method: 'POST',
        jsonData: { handle: UserHandle, progId: progId },
        async: false,
        timeout: 60000,
        success: function (response) {
          var ret = Ext.decode(response.responseText);
          selectFields = Ext.decode(ret.SelectQueryFieldResult);
        }
      });
    };
    getFields();
    function setDefaultValue(dataObj, defaultCondition, name) {
      var idx = -1;
      if (defaultCondition) {
        for (var i = 0; i < defaultCondition.length; i++) {
          if (defaultCondition[i]['Name'] == name) {
            idx = i;
            break;
          }
        }
      }
      if (idx != -1) {
        dataObj[name] = defaultCondition[idx]['QueryChar'];
        var count = defaultCondition[idx]['Value'].length;
        dataObj[name + 'Begin'] = defaultCondition[idx]['Value'][0];
        if (count == 2)
          dataObj[name + 'End'] = defaultCondition[idx]['Value'][1];
      } else {
        dataObj[field.Field] = 0;
        switch (field.DataType) {
          case 0:
          case 1:
            dataObj[field.Field + 'Begin'] = '';
            dataObj[field.Field + 'End'] = '';
            break;
          case 2:
          case 3:
          case 7:
          case 4:
          case 5:
          case 6:
          case 8:
            dataObj[field.Field + 'Begin'] = 0;
            dataObj[field.Field + 'End'] = 0;
            break
        }
      }
    }
    var selectItems = [];
    var fields = [];
    var dataObj = {};
    var store = Ext.create('Ext.data.Store', {
      fields: ['id', 'name'],
      data: [
        { "id": 0, "name": "空" },
        { "id": 1, "name": "等于" },
        { "id": 2, "name": "包含" },
        { "id": 3, "name": "区间" },
        { "id": 4, "name": "大于等于" },
        { "id": 5, "name": "小于等于" },
        { "id": 6, "name": "大于" },
        { "id": 7, "name": "小于" },
        { "id": 8, "name": "不等于" },
        { "id": 9, "name": "包括" }
      ]
    });
    var boolStore = Ext.create('Ext.data.Store', {
      fields: ['id', 'name'],
      data: [
        { "id": 0, "name": "空" },
        { "id": 1, "name": "是" },
        { "id": 2, "name": "否" }
      ]
    });
    for (var i = 0; i < selectFields.length; i++) {
      var field = selectFields[i];
      switch (field.DataType) {
        case 0:
        case 1:
          selectItems.push(Ext.create('Ext.Panel', {
            layout: {
              type: 'hbox',
              align: 'stretch'
            },
            defaults: {
              margin: '4 4',
              tableIndex:0
            },
            items: [{
              fieldLabel: field.DisplayText,
              xtype: 'libComboboxField',
              name: field.Field,
              labelAlign: 'right',
              displayField: 'name',
              valueField: 'id',
              store: store,
              value: 0,
              flex: 1
            }, Ext.apply(Ext.decode(field.ControlJs), {
              fieldLabel: '',
              readOnly: false,
              name: field.Field + 'Begin',
              flex: 1
            })
              , {
                xtype: 'label',
                text: 'To'
              }, Ext.apply(Ext.decode(field.ControlJs), {
                fieldLabel: '',
                readOnly: false,
                name: field.Field + 'End',
                flex: 1
              })]
          }));
          fields.push(field.Field);
          fields.push(field.Field + 'Begin');
          fields.push(field.Field + 'End');
          setDefaultValue(dataObj, defaultCondition, field.Field);
          break;
        case 2:
        case 3:
        case 7:
          selectItems.push(Ext.create('Ext.Panel', {
            layout: {
              type: 'hbox',
              align: 'stretch'
            },
            defaults: {
              margin: '4 4',
              tableIndex: 0
            },
            items: [{
              fieldLabel: field.DisplayText,
              xtype: 'libComboboxField',
              name: field.Field,
              labelAlign: 'right',
              displayField: 'name',
              valueField: 'id',
              store: store,
              value: 0,
              flex: 1
            }, Ext.apply(Ext.decode(field.ControlJs), {
              fieldLabel: '',
              readOnly: false,
              name: field.Field + 'Begin',
              flex: 1
            }), {
              xtype: 'label',
              text: 'To'
            }, Ext.apply(Ext.decode(field.ControlJs), {
              fieldLabel: '',
              readOnly: false,
              name: field.Field + 'End',
              flex: 1
            })]
          }));
          fields.push(field.Field);
          fields.push(field.Field + 'Begin');
          fields.push(field.Field + 'End');
          setDefaultValue(dataObj, defaultCondition, field.Field);
          break;
        case 4:
        case 5:
        case 6:
          selectItems.push(Ext.create('Ext.Panel', {
            layout: {
              type: 'hbox',
              align: 'stretch'
            },
            defaults: {
              margin: '4 4',
              tableIndex: 0
            },
            items: [{
              fieldLabel: field.DisplayText,
              xtype: 'libComboboxField',
              name: field.Field,
              labelAlign: 'right',
              displayField: 'name',
              valueField: 'id',
              store: store,
              value: 0,
              flex: 1
            }, Ext.apply(Ext.decode(field.ControlJs), {
              fieldLabel: '',
              readOnly: false,
              name: field.Field + 'Begin',
              flex: 1
            }), {
              xtype: 'label',
              text: 'To'
            }, Ext.apply(Ext.decode(field.ControlJs), {
              fieldLabel: '',
              readOnly: false,
              name: field.Field + 'End',
              flex: 1
            })]
          }));
          fields.push(field.Field);
          fields.push(field.Field + 'Begin');
          fields.push(field.Field + 'End');
          setDefaultValue(dataObj, defaultCondition, field.Field);
          break;
        case 8:
          selectItems.push(Ext.create('Ext.Panel', {
            layout: {
              type: 'hbox',
              align: 'stretch'
            },
            defaults: {
              margin: '4 4',
              tableIndex: 0
            },
            items: [{
              fieldLabel: field.DisplayText,
              xtype: 'libComboboxField',
              name: field.Field,
              labelAlign: 'right',
              displayField: 'name',
              valueField: 'id',
              store: boolStore,
              value: 0,
              flex: 1
            }, {
              flex: 1
            }, {
              xtype: 'label',
              margin: '4 10'
            }, {
              flex: 1
            }]
          }));
          fields.push(field.Field);
          dataObj[field.Field] = 0;
          if (defaultCondition !== undefined) {
            for (var r = 0; r < defaultCondition.length; r++) {
              if (defaultCondition[r]['Name'] == field.Field) {
                if (defaultCondition[r]['Value'][0] == 0)
                  dataObj[field.Field] = 2;
                else
                  dataObj[field.Field] = 1;
                break;
              }
            }
          }
          break
        default:
          break;
      }
    }
    var conditionStore = Ext.create('Ext.data.Store', {
      fields: fields,
      data: [dataObj]
    });
    var form = Ext.create('Ext.form.Panel', {
      flex: 1,
      layout: { type: 'vbox', align: 'stretch' },
      items: selectItems,
      autoScroll: true,
      store: conditionStore
    });
    form.loadRecord(conditionStore.data.items[0]);

    var win = Ext.create('Ext.window.Window', {
      title: '查询窗',
      id:id,
      autoScroll: true,
      width: 600,
      height: document.body.clientHeight * 0.9,
      layout: { type: 'vbox', align: 'stretch' },
      constrainHeader: true,
      minimizable: false,
      maximizable: false,
      modal: true,
      items: [form, {
        xtype: 'button',
        text: '确定',
        margin: '4 4',
        handler: function () {
          form.updateRecord(conditionStore.data.items[0]);
          var record = conditionStore.data.items[0].data;
          var queryCondition = [];
          for (var i = 0; i < selectFields.length; i++) {
            var field = selectFields[i];
            var queryChar = record[field.Field];
            if (queryChar == 0)
              continue;
            if (field.DataType == 8) {
              var tempVal = queryChar == 2 ? 0 : 1;
              queryCondition.push({
                Name: field.Field,
                QueryChar: 1,
                Value: [tempVal]
              });
            } else {
              if (queryChar == 3) {
                queryCondition.push({
                  Name: field.Field,
                  QueryChar: queryChar,
                  Value: [record[field.Field + "Begin"], record[field.Field + "End"]]
                });
              } else {
                queryCondition.push({
                  Name: field.Field,
                  QueryChar: queryChar,
                  Value: [record[field.Field + "Begin"]]
                });
              }
            }
          }
          vcl.formCallBackHandler("SYSTEM_QUERY", { condition: queryCondition });
          if (callback)
            callback();
          win.close();
        }
      }]
    });
    win.show();
    return win;
  }
};


Ax.utils.LibEntryParam = {
  createForm: function (vcl, progId, defaultParam) {
    var entryStr;
    Ext.Ajax.request({
      url: 'billSvc/getEntryRender',
      jsonData: { handle: UserHandle, progId: progId },
      method: 'POST',
      async: false,
      success: function (response) {
        var temp = Ext.decode(response.responseText);
        entryStr = temp["GetEntryRenderResult"];
      },
      failure: function () {
        Ext.Msg.show({
          title: '错误',
          msg: '载入失败！返回登录页！',
          buttons: Ext.Msg.YES,
          icon: Ext.Msg.INFO,
          fn: function (buttonId) {
            if (buttonId === 'yes') {
              if(window.DesktopApp.router){
                window.DesktopApp.router.push('/')
              }
            }
          }
        });
      }
    });
    if (!entryStr)
      return;
    var entry = Ext.decode(entryStr);
    var fields = [];
    for (var i = 0; i < entry[0].items.length; i++) {
      var item = entry[0].items[i];
      switch (item.xtype) {
        case 'libTextAreaField':
        case 'libTextField':
        case 'libSearchfield':
        case 'libSearchfieldTree':
          fields.push({ name: item.name, type: 'string' });
          break;
        case 'libNumberField':
        case 'libComboboxField':
        case 'libDateField':
        case 'libDatetimefield':
        case 'libHourMinuteField':
        case 'libTimeField':
          fields.push({ name: item.name, type: 'number' });
          break;
        case 'libCheckboxField':
          fields.push({ name: item.name, type: 'bool' });
          break;
      }
    }
    var modelName = progId + '_entry';
    var modelType = Ext.data.Model.schema.getEntity(modelName);
    if (modelType === null) {
      modelType = Ext.define(modelName, {
        extend: "Ext.data.Model",
        fields: fields
      });
    }
    var rec = Ext.create(modelName);
    var form = Ext.create('Ext.form.Panel', {
      border: false,
      tableIndex: 0,
      margin: '4 2 4 2',
      items: entry
    });

    var win = Ext.create('Ext.window.Window', {
      title: '入口参数设置',
      autoScroll: true,
      layout: { type: 'vbox', align: 'stretch' },
      constrainHeader: true,
      minimizable: false,
      maximizable: false,
      modal: true,
      items: [form, {
        xtype: 'button',
        text: '确定',
        margin: '4 4',
        handler: function () {
          form.updateRecord(rec);
          var entryParam = { ParamStore: {} };
          var emptyCount = 0;
          for (var i = 0; i < fields.length; i++) {
            var name = fields[i].name;
            if (!Ext.isEmpty(rec.get(name)))
              entryParam.ParamStore[name] = rec.get(name);
            else
              emptyCount += 1;
          }
          if (emptyCount == fields.length){
            Ext.Msg.alert("系统提示","请输入入口参数");
          }else{
            vcl.formCallBackHandler("ENTRY_PARAM", { condition: entryParam });
            win.close();
          }
        }
      }]
    });
    win.show();
    return win;
  }
};
