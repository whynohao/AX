/// <reference path="LibVclRptBase.js" />

Ax.vcl.LibVclRpt = function () {
    Ax.vcl.LibVclRptBase.apply(this, arguments);
};
var proto = Ax.vcl.LibVclRpt.prototype = Object.create(Ax.vcl.LibVclRptBase.prototype);
proto.constructor = Ax.vcl.LibVclRpt;

proto.showRpt = function (condition) {
    var data = this.invorkBcf("GetData", [condition]);
    this.setDataSet(data, false);
};

proto.showTaskRpt = function (execTaskDataId) {
    var data = this.invorkBcf("GetScheduleTaskData", [execTaskDataId]);
    this.setDataSet(data, false);
};

proto.formCallBackHandler = function (tag, param) {
    Ax.vcl.LibVclBase.prototype.formCallBackHandler.apply(this, arguments);
    if (tag == "SYSTEM_QUERY") {
        this.queryCondition = { QueryFields: param.condition };
        this.showRpt(this.queryCondition);
    }
};
proto.setSearchScheme = function (){
  Ax.utils.LibVclSystemUtils.openBill('axp.RptSearchField', BillTypeEnum.Bill, '报表查询参数设置', BillActionEnum.AddNew, undefined, undefined, {progId: this.progId, progName: this.tpl.DisplayText});
};
