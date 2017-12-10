EmOverHaulPlanVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = EmOverHaulPlanVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = EmOverHaulPlanVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);

    if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }

    if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo.tableIndex == 0) {
            var masterRow = this.dataSet.getTable(0).data.items[0];
            this.forms[0].updateRecord(masterRow);
        }
    }

    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:

            if (e.libEventType == LibEventTypeEnum.ButtonClick) {
                if (this.isEdit) {
                    if (e.dataInfo.fieldName == "Load") {
                        //alert("非编辑状态下不可操作");
                        //dataFunc 主表表名
                        var gridName = "EMOVERHAULPLANFUNC";
                        ////第一个参数:是datafunc 的progId
                        Ax.utils.LibVclSystemUtils.openDataFunc("Em.OverHaulPlanDataFunc", "来源单信息", [this, gridName]);
                    }
                }
                else {
                    alert("非编辑状态下不可操作");
                }
            }
    }
}