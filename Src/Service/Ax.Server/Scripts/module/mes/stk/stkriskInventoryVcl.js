/// <reference path="../pls/plsProduceSendVcl.js" />


stkRiskInventoryVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
    this.summaryRenderer.TotalName = function (v, sd, f) {//v代表当前列汇总值，sd包含各列汇总值，f代表当前列字段名
        return '<span style="color:darkred;font-weight:bold;">总计：</span>';
    }
};
var proto = stkRiskInventoryVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = stkRiskInventoryVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName != 'YEAR') {
                var i;
                switch (len) {
                    case 15: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 16: i = e.dataInfo.fieldName.substring(len - 2); break;
                    case 13: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 14: i = e.dataInfo.fieldName.substring(len - 2); break;
                }
                if (i < 15) {
                    //修改数量后移开光标，重新计算风险比例
                    if (e.dataInfo.fieldName == 'RESIDUEQUANTITY' + i) {
                        var actualQty = e.dataInfo.dataRow.data['RISKQUANTITY' + i];
                        if (actualQty > 0 && e.dataInfo.value > 0) {
                            e.dataInfo.dataRow.set('RISKQUANTITYRATE' + i, actualQty / e.dataInfo.value);
                        }
                        else {
                            e.dataInfo.dataRow.set('RISKQUANTITYRATE' + i, 0);
                        }
                    }
                        //修改数量后移开光标，重新计算风险比例
                    else if (e.dataInfo.fieldName == 'RISKQUANTITY' + i) {
                        var planQty = e.dataInfo.dataRow.data['RESIDUEQUANTITY' + i];
                        if (planQty > 0 && e.dataInfo.value > 0) {
                            e.dataInfo.dataRow.set('RISKQUANTITYRATE' + i, e.dataInfo.value / planQty);
                        }
                        else {
                            e.dataInfo.dataRow.set('RISKQUANTITYRATE' + i, 0);
                        }

                    }
                }
            }
            break;
    }


}