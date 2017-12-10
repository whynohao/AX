finAPPaymentVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    me = this;
};
var proto = finAPPaymentVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finAPPaymentVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "ACCEPTANCESTARTTIME") {
                    if(masterRow.get("ACCEPTANCEENDTIME")>0)
                    {
                        if (e.dataInfo.value <= masterRow.get("ACCEPTANCEENDTIME")) {
                            var len = this.getMonthNumber(e.dataInfo.value,masterRow.get("ACCEPTANCEENDTIME"));
                            Ext.getCmp('ACCEPTANCEDATE0_' + this.winId).setValue(len * 30);
                        }
                        else
                        {
                            Ext.Msg.alert("系统提示", "开始时间不能大于结束时间");
                            e.dataInfo.cancel = true;
                        }
                        
                    }
                }
                if (e.dataInfo.fieldName == "ACCEPTANCEENDTIME") {
                    if (masterRow.get("ACCEPTANCESTARTTIME") > 0) {
                        if (e.dataInfo.value >= masterRow.get("ACCEPTANCESTARTTIME")) {
                            var len = this.getMonthNumber(masterRow.get("ACCEPTANCESTARTTIME"),e.dataInfo.value);
                            Ext.getCmp('ACCEPTANCEDATE0_' + this.winId).setValue(len * 30);
                        }
                        else
                        {
                            Ext.Msg.alert("系统提示", "开始时间不能大于结束时间");
                            e.dataInfo.cancel = true;
                        }
                    }
                }
                this.forms[0].updateRecord(masterRow);
            }
            break;
    }
}

proto.getMonthNumber=function(date1, date2) {
    //默认格式为"20030303",根据自己需要改格式和方法
    var year1 = date1.toString().substr(0, 4);
    var year2 = date2.toString().substr(0, 4);
    var month1 = date1.toString().substr(4, 2);
    var month2 = date2.toString().substr(4, 2);

    var len = (year2 - year1) * 12 + (month2 - month1);
    return len;
}
