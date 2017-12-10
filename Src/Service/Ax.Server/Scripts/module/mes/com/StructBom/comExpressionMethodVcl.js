comExpressionMethodVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
}
var proto = comExpressionMethodVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comExpressionMethodVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == 'btnTest') {
            this.invorkBcf('Test', ["aaa", "EXP00002", ["aaa", "2145"]]);
        }
    }
}