purPurchaseAdviceDailyRptVcl = function () {
    Ax.vcl.LibVclDailyRpt.apply(this, arguments);
};
var proto = purPurchaseAdviceDailyRptVcl.prototype = Object.create(Ax.vcl.LibVclDailyRpt.prototype);
proto.constructor = purPurchaseAdviceDailyRptVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDailyRpt.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == 'btnBulidPlan') {//采购建议生成采购计划
            var AdvicePlanInfo = [];
            for (var i = 0; i < table.data.items.length; i++) {
                var record = table.data.items[i];
                AdvicePlanInfo.push({
                    TASKNO: record.get("TASKNO"),
                    CREATEDATE: record.get("CREATEDATE"),
                    WORKNO: record.get("WORKNO"),
                    PERSONID: record.get("PERSONID"),
                    SUPPLIERID: record.get("SUPPLIERID"),
                    MATERIALID: record.get("MATERIALID"),
                    NEEDQUANTITY: record.get("NEEDQUANTITY"),
                    QUANTITY: record.get("QUANTITY"),
                    PLANARRIVEDATE: record.get("PLANARRIVEDATE"),
                    SHORTLEADTIME: record.get("SHORTLEADTIME"),
                    LEADTIMEUNIT: record.get("LEADTIMEUNIT"),
                    DEMANDDATE: record.get("DEMANDDATE")
                });
            }
          
            var data = this.invorkBcf('BulidAdvicePlan', [AdvicePlanInfo]);
        }
    } 
}