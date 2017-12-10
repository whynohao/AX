axpBillMoveConfigVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);

}
var proto = axpBillMoveConfigVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = axpBillMoveConfigVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);

    //var SRCPROGID;
    //var OBJPROGID;
    if (e.libEventType == LibEventTypeEnum.AddRow) {
        //36位随机码
        //debugger
        if (e.dataInfo.tableIndex == 0) {
            var guid = Ax.utils.LibVclSystemUtils.newGuid();
            e.dataInfo.dataRow.set("GUID", guid);
        }
        ////源单代码,目的单代码 赋值
        //if (e.dataInfo.tableIndex == 1) {
        //        e.dataInfo.dataRow.set("SRCPROGID", SRCPROGID);
        //        e.dataInfo.dataRow.set("OBJPROGID", OBJPROGID);
        //    //this.forms[1].loadRecord(masterRow);
        //}
    }

    if (e.libEventType == LibEventTypeEnum.Validating) {
        //debugger
        //if (e.dataInfo.tableIndex == 0) {
        //    //源单代码,目的单代码 取值
        //    this.updateRowNo(e.dataInfo);
        //    debugger;
        //    if (e.dataInfo.fieldName == "SRCPROGID" || e.dataInfo.fieldName == "OBJPROGID") {
        //        SRCPROGID = e.dataInfo.dataRow.get("SRCPROGID");
        //        OBJPROGID = e.dataInfo.dataRow.get("OBJPROGID");
        //    }
        //}
    }

}