comAbnormalTraceTypeVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = comAbnormalTraceTypeVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comAbnormalTraceTypeVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == 'btnCondition') {
                if (this.isEdit) {
                    var dataRow = this.dataSet.getTable(1).ownGrid.getSelectionModel().getLastSelected();
                    if (dataRow) {
                        var execProgId = this.progId.replace('Type', '');
                        this.createForm(this, execProgId, "");
                    }
                    else
                        Ext.Msg.alert("系统提示", "请先选择行记录！");
                }
                else {
                    Ext.Msg.alert("系统提示", "编辑状态下才能设置条件！");
                }
            }
            break;
    }
}

proto.formCallBackHandler = function (tag, queryCondition) {
    Ax.vcl.LibVclDataBase.prototype.formCallBackHandler.apply(this, arguments);
    if (tag == "SYSTEM_QUERY") {
        if (this.isEdit) {
            var dataRow = this.dataSet.getTable(1).ownGrid.getSelectionModel().getLastSelected();
            if (dataRow) {
                dataRow.set('USECONDITION', queryCondition);
            }
        }
    }
}

proto.createForm = function (vcl, progId, defaultCondition, callback) {
    var id = Ext.id();
    DesktopApp.ActiveWindow = id;
    var progId = progId || vcl.progId;
    var allFields = [];
    function getFields() {
        Ext.Ajax.request({
            url: '/billSvc/selectQueryField',
            method: 'POST',
            jsonData: { handle: UserHandle, progId: progId },
            async: false,
            timeout: 60000,
            success: function (response) {
                var ret = Ext.decode(response.responseText);
                allFields = Ext.decode(ret.SelectQueryFieldResult);
            }
        });
    };
    getFields();
    var selectFields = [];
    //筛选显示的查询字段
    for (var i = 0; i < allFields.length; i++) {
        var selectField = allFields[i];
        selectFields.push(selectField);
    }

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
            dataObj[name + 'Begin'] = defaultCondition[idx]['Value'];
        } else {
            dataObj[field.Field] = 0;
            switch (field.DataType) {
                case 0:
                case 1:
                    dataObj[field.Field + 'Begin'] = '';
                    break;
                case 2:
                case 3:
                case 7:
                case 4:
                case 5:
                case 6:
                case 8:
                    dataObj[field.Field + 'Begin'] = 0;
                    break;
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
            { "id": 1, "name": "等于" }//,
            //{ "id": 2, "name": "包含" },
            //{ "id": 3, "name": "区间" },
            //{ "id": 4, "name": "大于等于" },
            //{ "id": 5, "name": "小于等于" },
            //{ "id": 6, "name": "大于" },
            //{ "id": 7, "name": "小于" },
            //{ "id": 8, "name": "不等于" },
            //{ "id": 9, "name": "包括" }
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
                    })]
                }));
                fields.push(field.Field);
                fields.push(field.Field + 'Begin');
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
                    })]
                }));
                fields.push(field.Field);
                fields.push(field.Field + 'Begin');
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
                    })]
                }));
                fields.push(field.Field);
                fields.push(field.Field + 'Begin');
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
                break;
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
        id: id,
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
                var queryCondition = '';
                for (var i = 0; i < selectFields.length; i++) {
                    var field = selectFields[i];
                    var queryChar = record[field.Field];
                    if (queryChar == 0)
                        continue;

                    if (queryCondition != '') {
                        Ext.Msg.alert("只支持单字段条件");
                        break;
                    }
                    queryCondition = '[A.' + field.Field + ']==' + "'" + record[field.Field + "Begin"] + "'";

                }
                console.log(queryCondition);
                vcl.formCallBackHandler("SYSTEM_QUERY", queryCondition);
                if (callback)
                    callback();
                win.close();
            }
        }]
    });
    win.show();
    return win;
}
