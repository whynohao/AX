comExpressionVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comExpressionVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);

proto.constructor = comExpressionVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (this.isEdit) {
                if (e.dataInfo.fieldName == "BtnMatch") {
                    Ax.utils.LibVclSystemUtils.openDataFunc("com.ExpressionMatch", "匹配条件", [this]);
                }
                else if (e.dataInfo.fieldName == "BtnClear") {
                    //清空表头数据
                    Ext.getCmp('EXPRESSIONID0_' + this.winId).setValue("");
                    Ext.getCmp('EXPRESSIONNAME0_' + this.winId).setValue("");
                    Ext.getCmp('EXPRESSIONEXPLAIN0_' + this.winId).setValue("");
                    this.dataSet.getTable(0).data.items[0].set("JSON", "");
                    this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                    //清空表身数据
                    this.dataSet.getTable(1).removeAll();
                }
            }
            else {
                Ext.Msg.alert("警告", "匹配条件在修改下才可使用！");
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                //根据‘公式内容’填写明细表
                if (e.dataInfo.fieldName == "EXPRESSIONDESC") {
                    this.ExpressionDetail.call(this, e);
                    e.dataInfo.dataRow.set("EXPRESSIONDESC", e.dataInfo.value);
                }
                else if (e.dataInfo.fieldName != "EXPRESSIONID") {
                    this.forms[0].updateRecord(e.dataInfo.dataRow);
                }
            }
            break;
    }
};
proto.ExpressionDetail = function (e) {
    var s = e.dataInfo.value, a = 0, arr = [], len = s.length, reg = /\@/;
    for (var a = 0; reg.test(s.substr(a, len - a)) ; a++) {
        a += s.substr(a, len - a).indexOf('@');
        var l = len - a;//剩余待解析的字符串长度
        //因方法中只有加或减，判断+或-即可
        var plusSign = s.substr(a, l).indexOf('+');
        var minus = s.substr(a, l).indexOf('-');
        var b1 = plusSign > 0;
        var b2 = minus > 0;
        var sign = (b1 && b2 && (minus < plusSign ? minus : plusSign)) || b1 && plusSign || b2 && minus || l;
        var paramId = s.substr(a, sign);
        if (paramId.lastIndexOf('@') != paramId.indexOf('@')) {
            e.dataInfo.cancel = true;
            this.forms[0].loadRecord(e.dataInfo.dataRow);
            arr.length = 0;
            Ext.Msg.alert("警告", "存在变量之间无符号");
            break;
        }
        else if (arr.indexOf(paramId) < 0) {
            arr.push(paramId);
        }
    }
    if (arr.length) {
        try {
            var expressionId = this.dataSet.getTable(0).data.items[0].data["EXPRESSIONID"];
            var store = this.dataSet.getTable(1);
            Ext.suspendLayouts();
            store.suspendEvents();
            store.removeAll();
            var grid = Ext.getCmp(this.winId + 'COMEXPRESSIONDETAILGrid');
            for (var i = 0; i < arr.length; i++) {
                var newRow = this.addRowForGrid(grid);
                newRow.set("EXPRESSIONID", expressionId);
                newRow.set('ROW_ID', i + 1);
                newRow.set('ROWNO', i + 1);
                newRow.set('PARAMID', arr[i]);
            }
        } finally {
            store.resumeEvents();
            if (store.ownGrid && store.ownGrid.getView().store != null)
                store.ownGrid.reconfigure(store);
            Ext.resumeLayouts(true);
        }
    }
}

