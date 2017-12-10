plsSalesForecastVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
//grid用Ax.vcl.LibVclGrid,单据主数据用Ax.vcl.LibVclBase,datafunc用Ax.vcl.LibVclGrid
var proto = plsSalesForecastVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = plsSalesForecastVcl;

//proto.winId = null;
//proto.fromObj = null;
//proto.getType = null;

//proto.doSetParam = function (vclObj) {
//    proto.winId = vclObj[0].winId;
//};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    //switch (e.libEventType) {
    //    case LibEventTypeEnum.Validating:
    //        
    //        break;
    //    case LibEventTypeEnum.BeforeDeleteRow://删除行
    //        break;
    //}
};
