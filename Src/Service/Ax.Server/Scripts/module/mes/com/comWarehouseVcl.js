comWarehouseVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comWarehouseVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comWarehouseVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                //天数不能小于0
                if (e.dataInfo.fieldName == "ISDEFAULT") {
                    var masterRow = this.dataSet.getTable(0).data.items[0];
                    var bodyRows = this.dataSet.getTable(1).data.items;
                    if (e.dataInfo.value == 1) {
                        for (var i = 0; i < bodyRows.length; i++) {
                            if (bodyRows[i].get("ROW_ID") != e.dataInfo.dataRow.get("ROW_ID")) {
                                bodyRows[i].set("ISDEFAULT", 0);
                            }
                        }
                        masterRow.set("PERSONID", e.dataInfo.dataRow.get("PERSONID"));
                        masterRow.set("PERSONNAME", e.dataInfo.dataRow.get("PERSONNAME"));
                        this.forms[0].loadRecord(masterRow);
                    } else {
                        masterRow.set("PERSONID", "");
                        masterRow.set("PERSONNAME", "");
                        this.forms[0].loadRecord(masterRow);
                    }
                }
            }
            break;
    }
}