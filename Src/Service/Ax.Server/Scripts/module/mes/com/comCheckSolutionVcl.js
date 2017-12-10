comCheckSolutionVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comCheckSolutionVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comCheckSolutionVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.Validating) {
        if (e.dataInfo.tableIndex == 2) {
            if (e.dataInfo.fieldName == "UPLIMIT") {
                var lowlimit = e.dataInfo.dataRow.get("LOWLIMIT");
                var standard = e.dataInfo.dataRow.get("STANDARD");
                var uplimit = e.dataInfo.value;
                if (standard != 0 && uplimit != 0) {
                    alert("已存在标准值，此上限值无效");
                    e.dataInfo.cancel = true;
                } else if (lowlimit > uplimit) {
                    alert("上限值不能小于下限值");
                    e.dataInfo.cancel = true;
                }
            }
            if (e.dataInfo.fieldName == "LOWLIMIT") {
                var uplimit = e.dataInfo.dataRow.get("UPLIMIT");
                var standard = e.dataInfo.dataRow.get("STANDARD");
                var lowlimit = e.dataInfo.value;
                if (standard != 0 && lowlimit != 0) {
                    alert("已存在标准值，此下限值无效");
                    e.dataInfo.cancel = true;
                } else if (lowlimit > uplimit) {
                    alert("下限值不能大于上限值");
                    e.dataInfo.cancel = true;
                }
            }
            if (e.dataInfo.fieldName == "STANDARD") {
                var uplimit = e.dataInfo.dataRow.get("UPLIMIT");
                var lowlimit = e.dataInfo.dataRow.get("LOWLIMIT");
                var standard = e.dataInfo.value;
                if (standard != 0 && (uplimit != 0 || lowlimit != 0)) {
                    alert("已存在上限值-下限值范围，此标准值无效");
                    e.dataInfo.cancel = true;
                }
            }
        }
    }
}
