comCheckItemVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comCheckItemVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comCheckItemVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.Validating) {
        if (e.dataInfo.fieldName == "DEFECTID")
        {
            var store = this.dataSet.getTable(e.dataInfo.tableIndex);
            var length = store.data.items.length;
            var DefectId = e.dataInfo.value;
            for (var i = 0; i < length-1; i++) {
                if (DefectId == store.data.items[i].data["DEFECTID"]) {
                    alert("缺陷已存在");
                    e.dataInfo.cancel = true;
                }
            }
        }
    }
}
