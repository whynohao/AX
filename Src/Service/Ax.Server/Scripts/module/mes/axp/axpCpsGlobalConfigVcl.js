axpCpsGlobalConfigVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);

}
var proto = axpCpsGlobalConfigVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = axpCpsGlobalConfigVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);

    if (e.libEventType == LibEventTypeEnum.AddRow) {
        //36位随机码
        if (e.dataInfo.tableIndex == 0) {
            var guid = Ax.utils.LibVclSystemUtils.newGuid();
            e.dataInfo.dataRow.set("CONFIGID", guid);
        }

    }



}