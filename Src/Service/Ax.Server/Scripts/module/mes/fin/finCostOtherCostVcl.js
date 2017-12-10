
finCostOtherCostVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    me = this;
};

var proto = finCostOtherCostVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finCostOtherCostVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "AMONUT") {
                    var bodyTable = this.dataSet.getTable(1);
                    var totalAmount = 0;
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        totalAmount += bodyTable.data.items[i].get("AMONUT");
                    }
                    Ext.getCmp('TOTALAMOUNT0_' + vcl.winId).setValue(totalAmount);
                }
            }
            break;
    }
}