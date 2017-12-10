/// <reference path="LibVclRptBase.js" />

Ax.vcl.LibVclDailyRpt = function () {
    Ax.vcl.LibVclRptBase.apply(this, arguments);
    this.currentDate = new Date();
};
var proto = Ax.vcl.LibVclDailyRpt.prototype = Object.create(Ax.vcl.LibVclRptBase.prototype);
proto.constructor = Ax.vcl.LibVclDailyRpt;

proto.showRpt = function (condition) {
    var tempDate = this.currentDate.getFullYear() * 10000 + (this.currentDate.getMonth() + 1) * 100 + this.currentDate.getDate();
    var data = this.invorkBcf("GetDailyData", [tempDate, condition]);
    this.setDataSet(data, false);
};

proto.formCallBackHandler = function (tag, param) {
    Ax.vcl.LibVclBase.prototype.formCallBackHandler.apply(this, arguments);
    if (tag == "SYSTEM_QUERY") {
        this.queryCondition = { QueryFields: param.condition };
        this.showRpt(this.queryCondition);
    }
};