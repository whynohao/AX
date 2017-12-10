/// <reference path="../pls/plsProduceSendVcl.js" />


stkMaterielturnoverRateVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
    this.summaryRenderer.TotalName = function (v, sd, f) {//v代表当前列汇总值，sd包含各列汇总值，f代表当前列字段名
        return '<span style="color:darkred;font-weight:bold;">总计：</span>';
    }
};
var proto = stkMaterielturnoverRateVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = stkMaterielturnoverRateVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName != 'YEAR') {
                var i;
                switch (len) {
                    case 13: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 14: i = e.dataInfo.fieldName.substring(len - 2); break;
                    case 17: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 18: i = e.dataInfo.fieldName.substring(len - 2); break;
                }
                if (i < 13) {
                    //修改数量后移开光标，重新计算比率
                    if (e.dataInfo.fieldName == 'PLANTURNOVERRATE' + i) {
                        var actualQty = e.dataInfo.dataRow.data['TURNOVERRATE' + i];
                        if (actualQty > 0 && e.dataInfo.value > 0) {
                            if (e.dataInfo.value >= actualQty) {
                                e.dataInfo.dataRow.set('RATE' + i, actualQty / e.dataInfo.value);
                            }

                        }
                        else {
                            e.dataInfo.dataRow.set('RATE' + i, 0);
                        }
                    }
                        //修改数量后移开光标，重新计算比率
                    else if (e.dataInfo.fieldName == 'TURNOVERRATE' + i) {
                        var planQty = e.dataInfo.dataRow.data['PLANTURNOVERRATE' + i];
                        if (planQty > 0 && e.dataInfo.value > 0) {
                            if (planQty >= e.dataInfo.value) {
                                e.dataInfo.dataRow.set('RATE' + i, e.dataInfo.value / planQty);
                            }

                        }
                        else {
                            e.dataInfo.dataRow.set('RATE' + i, 0);
                        }

                    }
                }
            }
            break;
    }


}