MonPointRegionVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = MonPointRegionVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = MonPointRegionVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    if (e.libEventType == LibEventTypeEnum.Validated) {
        var fieldName = e.dataInfo.fieldName;
        var masterRow = this.dataSet.getTable(0).data.items[0];
        if (e.dataInfo.tableIndex == 0) {
            if (this.billAction == BillActionEnum.Modif) {
                if (fieldName == "RECORDID") {
                    var oldvalue = e.dataInfo.oldValue
                    if (oldvalue != null && oldvalue != '') {
                        e.dataInfo.dataRow.set("RECORDID", oldvalue);
                        this.forms[0].loadRecord(masterRow);
                        Ext.Msg.alert("系统提示", "记录体代码不允许修改！");
                    }
                }

                if (fieldName == "RECORDMODEL") {
                    //this.forms[0].loadRecord(masterRow);
                    var recordModel = masterRow.get("RECORDMODEL");
                    var RcordId = masterRow.get("RECORDID");
                    var check = this.invorkBcf("CheckRecordMolde", [RcordId]);
                    if (!check ) {
                        Ext.Msg.alert("系统提示", "记录体有多个点位关联不允许修改记录模式！");
                        e.dataInfo.dataRow.set("RECORDMODEL", "0");
                        this.forms[0].loadRecord(masterRow);
                    }
                }
            }
        }
    }

    //if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
    //    if (e.dataInfo.tableIndex == 1) {
    //        e.dataInfo.cancel = true;
    //    }
    //}
}

