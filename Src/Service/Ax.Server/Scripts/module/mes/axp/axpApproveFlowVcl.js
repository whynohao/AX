/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：单据审核流配置的vcl脚本
 * 创建标识：Zhangkj 2017/03/21
 * 
 *
************************************************************************/
axpApproveFlowVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = axpApproveFlowVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = axpApproveFlowVcl;
//根据功能代码查找主表下的所有字段并设置到指定的comboBox下拉列表的选项中
proto.getFields = function (progId) {
    if (!progId || progId == '') {
        return;
    }
    var data = this.invorkBcf('GetDynamicFields', [progId]);
    this.axpApproveFlowVclOptionData = data;

};
//为动态部门列生成需要的Option下拉列表属性
proto.createColumnCombo = function (comboBoxCell) {
    var data = this.axpApproveFlowVclOptionData;
    //tpl
    comboBoxCell.allowBlank = true;
    if (comboBoxCell.tpl) {
        comboBoxCell.tpl.html = '<tpl if=\"DEPTIDCOLUMN == &quot;&quot;\">(空)</tpl>';//空行
        for (var i = 0; i < data.length; i++) {
            comboBoxCell.tpl.html += '<tpl if=\"DEPTIDCOLUMN == &quot;' + data[i].Id + '&quot;\">' + data[i].Name + '</tpl>';
        }
    }
    //store    
    if (!comboBoxCell.editor || !comboBoxCell.editor.store) {
        //create editor 
        //comboBoxCell.editor = Ext.create('Ax.ux.form.LibComboboxField', {
        comboBoxCell.editor = new Ax.ux.form.LibComboboxField({
            tableIndex: 2,
            queryMode: 'local', editable: false,
            displayField: 'value',
            valueField: 'key',
            store: Ext.create('Ext.data.Store', { fields: ['key', 'value'], data: [] })
        });
    }
    var storeData = [];
    storeData.push({ key: '', value: '(空)' });//空行
    for (var i = 0; i < data.length; i++) {
        storeData.push({ key: data[i].Id, value: data[i].Name });
    }
    comboBoxCell.editor.store.loadData(storeData);
    //filter
    if (comboBoxCell.filter) {
        comboBoxCell.filter.options = [['', '(空)']];//空行
        for (var i = 0; i < data.length; i++) {
            comboBoxCell.filter.options.push([data[i].Id, data[i].Name]);
        }
    }
};
proto.checkDeptFields = function (curGrid, masterRow) {
    var filterColumns = Ext.Array.filter(curGrid.columns, function (item) {
        //过滤,查找动态部门字段列
        if (item && item.dataIndex && item.dataIndex == 'DEPTIDCOLUMN') {
            return true;
        } else {
            return false;
        }
    }, this);//this表示作用域
    if (!filterColumns || filterColumns.length == 0)
        return;
    var nameColumn = filterColumns[0];
    var progId = masterRow.get('PROGID');
    if (!progId || progId == '') {
        return;
    }
    if (!this.axpApproveFlowVclOptionData || this.axpApproveFlowVclOldProgId != progId) {
        //console.log(nameColumn);                   
        this.axpApproveFlowVclOldProgId = progId;
        this.getFields(progId);
    }
    this.createColumnCombo(nameColumn);
};
//在subGrid窗口显示前触发
proto.beforeSubWinGridShow = function (tabIndex, gridPanel) {
    if (tabIndex == 2) {
        //为动态部门字段的控件combobox 添加下拉列表选项
        var masterRow = this.dataSet.getTable(0).data.items[0];
        this.checkDeptFields(gridPanel, masterRow);
    }
}
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == 'btnCondition') {
                if (this.isEdit) {
                    var dataRow = this.dataSet.getTable(1).ownGrid.getSelectionModel().getLastSelected();
                    if (dataRow) {
                        var execCondition = dataRow.get('USECONDITION');
                        if (execCondition.length > 0)
                            execCondition = Ext.decode(execCondition).QueryFields;
                        else
                            execCondition = undefined;
                        var execProgId = this.dataSet.getTable(0).data.items[0].data["PROGID"];
                        if (Ext.isEmpty(execProgId)) {
                            Ext.Msg.alert("系统提示", "请填写表头功能代码！");
                        }
                        else {
                            //Ax.utils.LibQueryForm.createForm(this, execProgId, execCondition);
                            this.createForm(this, execProgId, execCondition);
                        }
                    }
                    else
                        Ext.Msg.alert("系统提示", "请先选择需设置的行！");
                }
                else {
                    Ext.Msg.alert("系统提示", "编辑状态才能使用设置使用条件按钮！");
                }
            }
            break;
    }
}
//自定义的libSearchField后台数据查询
proto.customFuzzySearch = function (libSearchField, tableIndex, name, realRelSource, relName, queryString, curPks, selConditionParam, currentPk) {
    var isDoDeptQueryDuty = false;
    var deptId;
    var data = null;
    if (tableIndex != 2 || name != "DUTYID")
        isDoDeptQueryDuty = false;
    else {
        var curGrid = libSearchField.up('grid');
        if (curGrid) {
            var record = curGrid.getSelectionModel().getLastSelected();
            if (record) {
                var curRow;
                if (Ext.typeOf(record) == 'array' && record.length > 0)
                    curRow = record[0].data;
                else
                    curRow = record.data;
                if (curRow && curRow.DEPTID) {
                    isDoDeptQueryDuty = true;
                    deptId = curRow.DEPTID
                }
            }
        }
    }
    if (isDoDeptQueryDuty == false)
        data = this.invorkBcf('FuzzySearchField', [tableIndex, name, realRelSource, relName, queryString, curPks, selConditionParam, currentPk]);
    else {
        //调用自定义的查找指定部门下的任职岗位的后台Bcf方法
        data = this.invorkBcf('SearchDutyIdNameFromDept', [deptId, queryString]);
    }
    return data;
}


proto.formCallBackHandler = function (tag, param) {
    Ax.vcl.LibVclDataBase.prototype.formCallBackHandler.apply(this, arguments);
    if (tag == "SYSTEM_QUERY") {
        if (this.isEdit) {
            var dataRow = this.dataSet.getTable(1).ownGrid.getSelectionModel().getLastSelected();
            if (dataRow) {
                dataRow.set('USECONDITION', Ext.encode({ QueryFields: param.condition }));
            }
        }
    }
}

proto.createForm = function (vcl, progId, defaultCondition, callback) {
    var id = Ext.id();
    DesktopApp.ActiveWindow = id;
    var progId = progId || vcl.progId;
    var selectFields = [];
    function getFields() {
        Ext.Ajax.request({
            url: '/billSvc/selectQueryField',
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
            { "id": 4, "name": "大于等于" },
            { "id": 5, "name": "小于等于" },
            { "id": 6, "name": "大于" },
            { "id": 7, "name": "小于" },
            { "id": 8, "name": "不等于" }
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
    var textStore = Ext.create('Ext.data.Store', {
        fields: ['id', 'name'],
        data: [
            { "id": 0, "name": "空" },
            { "id": 1, "name": "等于" },
            { "id": 2, "name": "不等于" }
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
                        store: textStore,
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
