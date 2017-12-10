AxpValidityCalcVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = AxpValidityCalcVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = AxpValidityCalcVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "btnSchedule") {
            this.invorkBcf('funSchedule');
        }
    }
}