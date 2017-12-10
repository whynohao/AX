MonEquipmentOperationVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
};
var proto = MonEquipmentOperationVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = MonEquipmentOperationVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    //不允许手工添加行
    if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
        if (e.dataInfo.tableIndex == 0) {
            e.dataInfo.cancel = true;
        }
    }
        //不允许手工删除行
    else if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
        if (e.dataInfo.tableIndex == 0) {
            e.dataInfo.cancel = true;
        }
    }

}