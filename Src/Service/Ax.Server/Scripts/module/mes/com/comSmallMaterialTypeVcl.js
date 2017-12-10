comSmallMaterialTypeVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = comSmallMaterialTypeVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comSmallMaterialTypeVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo.tableIndex == 0) {
            if (e.dataInfo.fieldName == "MATERIALTYPEID") {
                e.dataInfo.dataRow.set("MCLASSID", "");
                e.dataInfo.dataRow.set("MCLASSNAME", "");
                Ext.getCmp('MCLASSID0_' + this.winId).setValue("");
            }
        }
    }
}