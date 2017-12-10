plsProduceDayPlanVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = plsProduceDayPlanVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = plsProduceDayPlanVcl;
var workBillNo = "";

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeDeleteRow:
            var curRow = e.dataInfo.dataRow;
            var pWorkOrderNo = curRow.get("PWORKORDERNO");
            var start = this.invorkBcf('CheckStartState', [pWorkOrderNo]);
            if (start) {
                e.dataInfo.cancel = true;
                Ext.Msg.alert("提示", "该明细对应的派工单已经开工，不能删除！");
            }
            break;
    }
};


