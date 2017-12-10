
axpPermissionGroupVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = axpPermissionGroupVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = axpPermissionGroupVcl;

proto.setDefaultPower = function (curRow, returnValue) {
    var ret = returnValue['OperatePowerData'];
    var operateData = this.dataSet.getChildren(1, curRow, 2);
    var buttonPowerData = this.dataSet.getChildren(1, curRow, 4);
    if (operateData) {
        var table = this.dataSet.getTable(2);
        for (var i = operateData.length - 1; i >= 0; i--) {
            table.remove(operateData[i]);
            this.dataSet.deleteData(2, operateData[i]);
        }
    }
    if (buttonPowerData) {
        var table = this.dataSet.getTable(4);
        for (var i = buttonPowerData.length - 1; i >= 0; i--) {
            table.remove(buttonPowerData[i]);
            this.dataSet.deleteData(4, buttonPowerData[i]);
        }
    }
    var hasData = false;
    if (ret) {
        for (p in ret) {
            if (!ret.hasOwnProperty(p))
                continue;
            var value = ret[p];
            if (value !== undefined) {
                var newRow = this.addRow(curRow, 2);
                newRow.set('OPERATEPOWERID', p);
                newRow.set('OPERATEPOWERNAME', value.DisplayText);
                newRow.set('CANUSE', value.CanUse);
                if (!hasData)
                    hasData = true;
            }
        }
    }
    curRow.set('ISOPERATEPOWER', hasData);
    hasData = false;
    ret = returnValue['ButtonPowerData'];
    if (ret) {
        for (p in ret) {
            if (!ret.hasOwnProperty(p))
                continue;
            var value = ret[p];
            if (value !== undefined) {
                var newRow = this.addRow(curRow, 4);
                newRow.set('BUTTONID', p);
                newRow.set('BUTTONNAME', value);
                newRow.set('CANUSE', true);
                if (!hasData)
                    hasData = true;
            }
        }
    }
    curRow.set('ISBUTTONPOWER', hasData);
};


proto.checkFieldValue = function (curRow, returnValue, tableIndex, fieldName) {
    Ax.vcl.LibVclData.prototype.checkFieldValue.apply(this, arguments);
    if (tableIndex == 1) {
        switch (fieldName) {
            case 'PROGID':
                this.setDefaultPower(curRow, returnValue);
                break;
        }
    }
};

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 2) {
                e.dataInfo.cancel = true;
                alert('不能新增。');
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 2) {
                e.dataInfo.cancel = true;
                alert('不能删除。');
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case 'btnCondition':
                    var dataRow = this.dataSet.getTable(1).ownGrid.getSelectionModel().getLastSelected();
                    if (dataRow) {
                        var execCondition = dataRow.get('SHOWCONDITION');
                        var execProgId = dataRow.get('PROGID');
                        if (execProgId) {
                            if (execCondition.length > 0)
                                execCondition = Ext.decode(execCondition).QueryFields;
                            else
                                execCondition = undefined;
                            Ax.utils.LibQueryForm.createForm(this, execProgId, execCondition);
                        }
                        else
                            alert('请先选择功能。');
                    }
                    else
                        alert('请先选择行记录。');
                    break;
                case 'btnLoad':
                    break;
                default:
                    break;
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.fieldName == 'PROGID') {
                var dataRow = this.dataSet.getTable(1).ownGrid.getSelectionModel().getLastSelected();
                if (dataRow) {
                    dataRow.set('SHOWCONDITION', '');
                    dataRow.set('HASSHOWCONDITION', false);
                }
            }
            break;
    }
}


proto.formCallBackHandler = function (tag, param) {
    Ax.vcl.LibVclDataBase.prototype.formCallBackHandler.apply(this, arguments);
    if (tag == "SYSTEM_QUERY") {
        if (this.isEdit) {
            //var dataRow = this.dataSet.getTable(1).ownGrid.getSelectionModel().getLastFocugetLastSelectedsed();
            var dataRow = this.dataSet.getTable(1).ownGrid.getSelectionModel().getLastSelected();
            if (dataRow) {
                dataRow.set('SHOWCONDITION', Ext.encode({ QueryFields: param.condition }));
                dataRow.set('HASSHOWCONDITION', true);
            }
        }
    }
};
