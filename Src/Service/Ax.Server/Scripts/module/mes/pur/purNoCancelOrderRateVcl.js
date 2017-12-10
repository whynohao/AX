/// <reference path="../pls/plsProduceSendVcl.js" />
purNoCancelOrderRateVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
    this.summaryRenderer.TotalName = function (v, sd, f) {//v代表当前列汇总值，sd包含各列汇总值，f代表当前列字段名
        return '<span style="color:darkred;font-weight:bold;">总计：</span>';
    }
};
var proto = purNoCancelOrderRateVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = purNoCancelOrderRateVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName != 'PERSONID' && e.dataInfo.fieldName != 'PERSONNAME') {
                var i;
                switch (len) {
                    case 18: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 19: i = e.dataInfo.fieldName.substring(len - 2); break;
                }

                //修改1-12月的数量 需要重新计算所有占比
                var sumEvery = 0;
                for (var k = 0; k < dt.data.length; k++) {
                        var value= dt.data.items[k].data['CANCELORDERNUMBER' + i];
                        sumEvery += value;
                }
                sumEvery += e.dataInfo.value - e.dataInfo.oldValue;
                if (sumEvery > 0) {
                    
                    for (var j = 0; j < dt.data.length; j++) {
                        if (e.dataInfo.dataRow.data['PERSONID'] == dt.data.items[j].data.PERSONID) {
                            e.dataInfo.dataRow.set('ONRATE' + i, e.dataInfo.value / sumEvery);
                        }
                        else {
                            dt.data.items[j].set('ONRATE' + i, dt.data.items[j].data['CANCELORDERNUMBER' + i] / sumEvery);
                        }
                    }
                }
                else {
                    dt.data.items[j].set('ONRATE' + i, 0);
                }

                //随着1-12月某个数量的改变 所有行的总计数量发生改变 需要重新计算所有占比
                var sumTotal = 0;
                for (var k = 0; k < dt.data.length; k++) {
                    var drPlanQty = 0;//当前行12个月的数量总值
                    if (e.dataInfo.dataRow.data['PERSONID'] == dt.data.items[k].data.PERSONID) {
                        for (var j = 1; j <= 12; j++) {
                            if (j == i) {//累加当前列的当前值
                                if (e.dataInfo.fieldName == 'CANCELORDERNUMBER' + j) {
                                    drPlanQty += e.dataInfo.value;
                                }
                            }
                            else {
                                drPlanQty += dt.data.items[k].data['CANCELORDERNUMBER' + j];
                            }
                        }
                        e.dataInfo.dataRow.set('CANCELORDERNUMBER13', drPlanQty);
                        sumTotal += drPlanQty;
                    }
                    else {
                        sumTotal += dt.data.items[k].data['CANCELORDERNUMBER13'];
                    }
                }

                if (sumTotal > 0) {

                    for (var j = 0; j < dt.data.length; j++) {
                        if (e.dataInfo.dataRow.data['PERSONID'] == dt.data.items[j].data.PERSONID) {
                            e.dataInfo.dataRow.set('ONRATE13', e.dataInfo.dataRow.data['CANCELORDERNUMBER13'] / sumTotal);
                        }
                        else {
                            dt.data.items[j].set('ONRATE13', dt.data.items[j].data['CANCELORDERNUMBER13'] / sumTotal);
                        }
                    }
                }
                else {
                    dt.data.items[j].set('ONRATE13', 0);
                }
            }
            break;
    }


}