finARReceiptVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    me = this;
};
var proto = finARReceiptVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finARReceiptVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "COLLECTIONAMOUNT") {
                    var bwcollectionamount = e.dataInfo.value * e.dataInfo.dataRow.get("STANDARDCOILRATE");
                    var verificationamount = e.dataInfo.dataRow.get("VERIFICATIONAMOUNT");
                    var offsetamount = e.dataInfo.dataRow.get("OFFSETAMOUNT");
                    var remainamount = bwcollectionamount - verificationamount - offsetamount;
                    //if(remainamount>0)
                    //{
                    //e.dataInfo.dataRow.set("BWCOLLECTIONAMOUNT", bwcollectionamount);
                    //e.dataInfo.dataRow.set("REMAINAMOUNT", remainamount);
                    Ext.getCmp('BWCOLLECTIONAMOUNT0_' + me.winId).setValue(bwcollectionamount);
                    //Ext.getCmp('REMAINAMOUNT0_' + vcl.winId).setValue(remainamount);
                    //}
                    //else
                    //{
                    //    Ext.Msg.alert("系统提示", "收款金额不能小于核销金额和对冲金额之和");
                    //}
                }
                if (e.dataInfo.fieldName == "BWCOLLECTIONAMOUNT") {
                    var collectionamount = e.dataInfo.dataRow.get("STANDARDCOILRATE") == 0 ?
                        0 : e.dataInfo.value / e.dataInfo.dataRow.get("STANDARDCOILRATE");
                    var verificationamount = e.dataInfo.dataRow.get("VERIFICATIONAMOUNT");
                    var offsetamount = e.dataInfo.dataRow.get("OFFSETAMOUNT");
                    var remainamount = e.dataInfo.value - verificationamount - offsetamount;
                    //if (remainamount > 0) {
                    //e.dataInfo.dataRow.set("COLLECTIONAMOUNT", collectionamount);
                    //e.dataInfo.dataRow.set("REMAINAMOUNT", remainamount);
                    Ext.getCmp('COLLECTIONAMOUNT0_' + me.winId).setValue(collectionamount);
                    //Ext.getCmp('REMAINAMOUNT0_' + vcl.winId).setValue(remainamount);
                    //}
                    //else {
                    //    Ext.Msg.alert("系统提示", "收款金额不能小于核销金额和对冲金额之和");
                    //}
                }
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
                if (e.dataInfo.fieldName == "STANDARDCOILRATE") {
                    var bwcollectionamount = e.dataInfo.dataRow.get("BWCOLLECTIONAMOUNT");
                    var collectionamount = e.dataInfo.value == 0 ? 0 : bwcollectionamount / e.dataInfo.value;
                    Ext.getCmp('COLLECTIONAMOUNT0_' + me.winId).setValue(collectionamount);
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
