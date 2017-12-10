comIDLConfigVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comIDLConfigVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);

proto.constructor = comIDLConfigVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.fieldName == "EXCLUDEATTRIBUTEITEM") {
                if (!(e.dataInfo.dataRow.data["FIELDNAME"] == "ATTRIBUTECODE" || e.dataInfo.dataRow.data["FIELDNAME"] == "ATTRIBUTEDESC")) {
                    e.dataInfo.cancel = true;
                }
            }
            if (e.dataInfo.fieldName == "CONTAINATTRIBUTEITEM") {
                if (!(e.dataInfo.dataRow.data["FIELDNAME"] == "ATTRIBUTECODE")) {
                    e.dataInfo.cancel = true;
                }
            }
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "PROGID") {
                    if (e.dataInfo.value != "com.SaleBOM") {
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("提示", "指定配置暂不支持此功能");
                    }
                }
                if (e.dataInfo.fieldName == "FIELDNAME") {
                    var progId = e.dataInfo.dataRow.get("PROGID");
                    var tableIndex = e.dataInfo.dataRow.get("TABLEINDEX");
                    var fieldId = e.dataInfo.value;
                    this.invorkBcf('FindLibDataType', [progId, tableIndex, fieldId]);
                }
            }
            break;
    }
};


