comMaterialTypeVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = comMaterialTypeVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comMaterialTypeVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.fieldName == 'PARENTTYPEID') {
                var parentindex = this.invorkBcf('GetParentIndex', [ e.dataInfo.value, e.dataInfo.dataRow.get('SELFINDEX')]);
                e.dataInfo.dataRow.set('PARENTINDEX', parentindex);
                e.dataInfo.dataRow.get('PARENTINDEX');
            }
            break;
        //case LibEventTypeEnum.ButtonClick:
        //    if (e.dataInfo.fieldName == "BtnReCreateIndex") {
        //        this.invorkBcf('BtnReCreateIndex', []);
        //    }
        //    break;
    }
}