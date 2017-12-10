comFacProduceLineFlowVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
};
var proto = comFacProduceLineFlowVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = comFacProduceLineFlowVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    //switch (e.libEventType) {
    //    case LibEventTypeEnum.AddRow:
    //        var guid = Ax.utils.LibVclSystemUtils.newGuid();
    //        e.dataInfo.dataRow.set("RECORDID", guid);
    //        break;
    //}
}