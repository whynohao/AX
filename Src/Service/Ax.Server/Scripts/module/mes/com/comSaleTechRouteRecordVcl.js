comSaleTechRouteRecordVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
};
var proto = comSaleTechRouteRecordVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = comSaleTechRouteRecordVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            e.dataInfo.cancel = true;
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            e.dataInfo.cancel = true;
            break;
    }
}