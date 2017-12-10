comMaterialProcessConfigVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);

}
var proto = comMaterialProcessConfigVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = comMaterialProcessConfigVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);

    switch (e.libEventType) {
        case LibEventTypeEnum.AddRow:
            //36位随机码
            var guid = Ax.utils.LibVclSystemUtils.newGuid();
            e.dataInfo.dataRow.set("GUID", guid);
            break;
    }
}