comPersonGroupVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = comPersonGroupVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comPersonGroupVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            this.forms[0].updateRecord(masterRow);
            var fieldName = e.dataInfo.fieldName;
            if (e.dataInfo.tableIndex == 0) {                
                if (fieldName == "GROUPATTRIBUTE"||fieldName == "ISDEFAULT") {
                    MessageConfirm.call(this, e);
                }
            }
            break;
    }
}

function MessageConfirm(e) {
    var isdefault = e.dataInfo.dataRow.get("ISDEFAULT");
    var groupattribute = e.dataInfo.dataRow.get("GROUPATTRIBUTE");

    if (isdefault == 1) {
        var persongroupid = this.invorkBcf('CheckDefault', [isdefault, groupattribute]);
        if (!Ext.isEmpty(persongroupid)) {
            Ext.Msg.confirm("系统提示", "该业务属性下已经存在缺省人员组【" + persongroupid + "】，是否替换？", function (btn) {
                switch (btn) {
                    case "yes":

                        break;
                    case "no":
                        e.dataInfo.dataRow.set("ISDEFAULT", "0");
                        e.dataInfo.curForm.loadRecord(e.dataInfo.dataRow);
                        break;
                }
            });
        }
    }
}