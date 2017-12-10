
axpScheduleTaskVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = axpScheduleTaskVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = axpScheduleTaskVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) { 
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.fieldName == 'PROGID') {
                var masterRow = this.dataSet.getTable(0).data.items[0];
                masterRow.set('EXECCONDITION', '');
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == 'btnCondition') {
                this.forms[0].updateRecord(masterRow);
                var masterRow = this.dataSet.getTable(0).data.items[0];
                var execCondition = masterRow.get('EXECCONDITION');
                var execProgId = masterRow.get('PROGID');
                if (execProgId) {
                    if (execCondition.length > 0)
                        execCondition = Ext.decode(execCondition).QueryFields;
                    else
                        execCondition = undefined;
                    Ax.utils.LibQueryForm.createForm(this, execProgId, execCondition);
                }
                else
                    alert('请先选择执行功能。');
            }
            break;
    }
}

proto.formCallBackHandler = function (tag, param) {
    Ax.vcl.LibVclDataBase.prototype.formCallBackHandler.apply(this, arguments);
    if (tag == "SYSTEM_QUERY") {
        if (this.isEdit) {
            var masterRow = this.dataSet.getTable(0).data.items[0];
            masterRow.set('EXECCONDITION', Ext.encode({ QueryFields: param.condition }));
        }
    }
};
