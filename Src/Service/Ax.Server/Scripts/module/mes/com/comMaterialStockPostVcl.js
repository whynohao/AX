comMaterialStockPostVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);

}
var proto = comMaterialStockPostVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = comMaterialStockPostVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);

    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow://不允许新增行
            if (e.dataInfo.tableIndex == 0) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow://不允许删除行
            if (e.dataInfo.tableIndex == 0) {
                e.dataInfo.cancel = true;
            }
            break;
    }


}