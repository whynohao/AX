/// <reference path="../pls/plsProduceSendVcl.js" />
purAffectWorkHoursVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
    this.summaryRenderer.TotalName = function (v, sd, f) {//v代表当前列汇总值，sd包含各列汇总值，f代表当前列字段名
        return '<span style="color:darkred;font-weight:bold;">总计：</span>';
    }
};
var proto = purAffectWorkHoursVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = purAffectWorkHoursVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName != 'PERSONID' && e.dataInfo.fieldName != 'PERSONNAME') {
                var i;
                switch (len)
                {
                    case 12: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 13: i = e.dataInfo.fieldName.substring(len - 2); break;
                }
                //#region 
                var drPlanQty = 0;//当前行12个月的数量总值
                for (var k = 0; k < dt.data.length; k++) {
                    if (e.dataInfo.dataRow.data['PERSONID'] == dt.data.items[k].data.PERSONID) {
                        for (var j = 1; j <= 12; j++) {
                            if (j == i) {//累加当前列的当前值
                                if (e.dataInfo.fieldName == 'AFFECTHOURS' + j) {
                                    drPlanQty += e.dataInfo.value;//当前列计划值的当前值
                                }
                            }
                            else {
                                drPlanQty += dt.data.items[k].data['AFFECTHOURS' + j];
                            }
                        }
                        e.dataInfo.dataRow.set('AFFECTHOURS13', drPlanQty)
                    }
                }
            }
            break;
    }


}