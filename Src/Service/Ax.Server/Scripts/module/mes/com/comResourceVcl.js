comResourceVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comResourceVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comResourceVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo.fieldName == 'PRODUCETYPE') {
            if (e.dataInfo.value != e.dataInfo.oldValue)
            {
                e.dataInfo.dataRow.set('PRODUCEUNITID',"" );
            }
        }
    }
}