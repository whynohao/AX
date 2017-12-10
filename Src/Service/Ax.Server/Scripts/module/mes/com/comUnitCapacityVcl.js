comUnitCapacityVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);

}
var proto = comUnitCapacityVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = comUnitCapacityVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);

    switch (e.libEventType) {
        case LibEventTypeEnum.AddRow:
            //36位随机码
            var guid = Ax.utils.LibVclSystemUtils.newGuid();
            e.dataInfo.dataRow.set("GUID", guid);
            break;
    }

    //if (e.libEventType == LibEventTypeEnum.Validated) {
    //    if (e.dataInfo.fieldName == "WORKPROCESSID") {
    //        var oldvalue = e.dataInfo.oldValue;
    //        var masterRow = this.dataSet.getTable(0).data.items[0];
    //        if (e.dataInfo.dataRow.get("SALETECHROUTEID") != "") {
    //            //e.dataInfo.cancel = ture;
    //            e.dataInfo.dataRow.set("WORKPROCESSID", oldvalue);
    //            this.forms[0].loadRecord(masterRow);
    //        }
    //    }
    //}
}