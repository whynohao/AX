/// <reference path="../pls/plsProduceSendVcl.js" />
stkInventoryReachedRateVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
    this.summaryRenderer.TotalRateFun = function (v, sd, f) {
        var idx = '';
        var regx = /^[0-9]*$/;
        for (var i = this.dataIndex.length - 1; i >= 0; i--) {
            var v = this.dataIndex[i];
            if (regx.test(v)) {
                if (idx.length == 0)
                    idx = v;
                else
                    idx = Ext.String.insert(idx, v, 0);
            }
            else
                break;
        }
        if (sd.record.data['OBJECTIVE' + idx] != 0 && sd.record.data['PHYSICALINVENTORY' + idx] != 0) {
            var value = sd.record.data['OBJECTIVE' + idx] / sd.record.data['PHYSICALINVENTORY' + idx];
            if (value >= 0) {
                return '<span style="color:blue;">' + (value * 100).toFixed(2) + '%</span>';
            }
            else {
                return '<span style="color:red;">' + (value * 100).toFixed(2) + '%</span>';
            }
        }
        else
            return '<span style="color:blue;">' + (0).toFixed(2) + '%</span>';
    }
    this.summaryRenderer.TotalName = function (v, sd, f) {//v代表当前列汇总值，sd包含各列汇总值，f代表当前列字段名
        return '<span style="color:darkred;font-weight:bold;">总计：</span>';
    }
};
var proto = stkInventoryReachedRateVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = stkInventoryReachedRateVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName != 'WAREHOUSEID' && e.dataInfo.fieldName != 'WAREHOUSENAME') {
                var i;
                switch (len) {

                    case 10: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 11: i = e.dataInfo.fieldName.substring(len - 2); break;
                    case 18: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 19: i = e.dataInfo.fieldName.substring(len - 2); break;

                }
                //#region 重新计算按时收货率
                if (i < 13) {
                    //修改送货数量后移开光标，重新计算按时收货率
                    if (e.dataInfo.fieldName == 'OBJECTIVE' + i) {
                        var actualQty = e.dataInfo.dataRow.data['PHYSICALINVENTORY' + i];
                        if (actualQty > 0 && e.dataInfo.value > 0) {

                            e.dataInfo.dataRow.set('INVENTORYREACHEDRATE' + i, e.dataInfo.value / actualQty);

                        }
                        else {
                            e.dataInfo.dataRow.set('INVENTORYREACHEDRATE' + i, 0);
                        }
                    }
                        //修改实际数量后移开光标，重新计算按时收货率
                    else if (e.dataInfo.fieldName == 'PHYSICALINVENTORY' + i) {
                        var planQty = e.dataInfo.dataRow.data['OBJECTIVE' + i];
                        if (planQty > 0 && e.dataInfo.value > 0) {

                            e.dataInfo.dataRow.set('INVENTORYREACHEDRATE' + i, planQty / e.dataInfo.value);

                        }
                        else {
                            e.dataInfo.dataRow.set('INVENTORYREACHEDRATE' + i, 0);
                        }

                    }
                }
                //#endregion

                //#region
                var drPlanQty = 0;//当前行12个月的数量总值
                var drActualQty = 0;//当前行12个月的按时收货数量量总值
                for (var k = 0; k < dt.data.length; k++) {
                    if (e.dataInfo.dataRow.data['WAREHOUSEID'] == dt.data.items[k].data.WAREHOUSEID) {
                        //每次都要重新计算总共的值 以免存在多列同时修改的情况
                        for (var j = 1; j <= 12; j++) {
                            if (j == i) {//累加当前列的当前值

                                if (e.dataInfo.fieldName == 'OBJECTIVE' + j) {
                                    drActualQty += dt.data.items[k].data['PHYSICALINVENTORY' + j];//实际值的原始值
                                    drPlanQty += e.dataInfo.value;//当前列目标值的当前值
                                }
                                else if (e.dataInfo.fieldName == 'PHYSICALINVENTORY' + j) {
                                    drPlanQty += dt.data.items[k].data['OBJECTIVE' + j];//目标值的原始值
                                    drActualQty += e.dataInfo.value;//当前列实际值的当前值
                                }
                            }
                            else {
                                drPlanQty += dt.data.items[k].data['OBJECTIVE' + j];
                                drActualQty += dt.data.items[k].data['PHYSICALINVENTORY' + j];
                            }
                        }
                        e.dataInfo.dataRow.set('OBJECTIVE13', drPlanQty);
                        e.dataInfo.dataRow.set('PHYSICALINVENTORY13', drActualQty);
                        if (drPlanQty > 0 && drActualQty > 0) {

                            e.dataInfo.dataRow.set('INVENTORYREACHEDRATE13', drPlanQty / drActualQty);


                        }
                        else {
                            e.dataInfo.dataRow.set('INVENTORYREACHEDRATE13', 0);
                        }
                    }
                    //#endregion
                }
            }
            break;
    }


}