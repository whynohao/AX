plsMonthOutputAccuracyRateVcl = function () {
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
        if (sd.record.data['PLANQUANTITY' + idx] != 0) {
            var value = sd.record.data['ACTUALQUANTITY' + idx] / sd.record.data['PLANQUANTITY' + idx];
            if (value >= 0) {
                if (sd.record.data['PLANQUANTITY' + idx] >= sd.record.data['ACTUALQUANTITY' + idx]) {
                    return '<span style="color:blue;">' + (value * 100).toFixed(2) + '%</span>';
                }
                else {
                    return '<span style="color:blue;">' + ((2 - value) * 100).toFixed(2) + '%</span>';
                }

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
}
var proto = plsMonthOutputAccuracyRateVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = plsMonthOutputAccuracyRateVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName != 'PRODUCTTYPEID' && e.dataInfo.fieldName != 'PRODUCTTYPENAME') {
                var i;
                switch (len) {
                    case 13: i = e.dataInfo.fieldName.substring(len - 1); break;//PLANQUANTITY1~ PLANQUANTITY9
                    case 14: i = e.dataInfo.fieldName.substring(len - 2); break;//PLANQUANTITY10~ PLANQUANTITY13
                    case 15: i = e.dataInfo.fieldName.substring(len - 1); break;//ACTUALQUANTITY1~ACTUALQUANTITY9
                    case 16: i = e.dataInfo.fieldName.substring(len - 2); break;//ACTUALQUANTITY10~ACTUALQUANTITY13

                }
                //#region 重新计算准确率
                if (i < 13) {
                    //修改计划数量后移开光标，重新计算准确率
                    if (e.dataInfo.fieldName == 'PLANQUANTITY' + i) {
                        var actualQty = e.dataInfo.dataRow.data['ACTUALQUANTITY' + i];
                        if (actualQty > 0 && e.dataInfo.value > 0) {
                            if (e.dataInfo.value >= actualQty) {
                                e.dataInfo.dataRow.set('ACCURACYRATE' + i, actualQty / e.dataInfo.value);
                            }
                            else {
                                e.dataInfo.dataRow.set('ACCURACYRATE' + i, 2 - actualQty / e.dataInfo.value);
                            }
                        }
                        else {
                            e.dataInfo.dataRow.set('ACCURACYRATE' + i, 0);
                        }
                    }
                        //修改实际数量后移开光标，重新计算准确率
                    else if (e.dataInfo.fieldName == 'ACTUALQUANTITY' + i) {
                        var planQty = e.dataInfo.dataRow.data['PLANQUANTITY' + i];
                        if (planQty > 0 && e.dataInfo.value > 0) {
                            if (planQty >= e.dataInfo.value) {
                                e.dataInfo.dataRow.set('ACCURACYRATE' + i, e.dataInfo.value / planQty);
                            }
                            else {
                                e.dataInfo.dataRow.set('ACCURACYRATE' + i, 2 - e.dataInfo.value / planQty);
                            }
                        }
                        else {
                            e.dataInfo.dataRow.set('ACCURACYRATE' + i, 0);
                        }

                    }
                }
                //#endregion

                //#region
                var drPlanQty = 0;//当前行12个月的计划量总值
                var drActualQty = 0;//当前行12个月的实际量总值
                for (var k = 0; k < dt.data.length; k++) {
                    if (e.dataInfo.dataRow.data['PRODUCTTYPEID'] == dt.data.items[k].data.PRODUCTTYPEID) {
                        //每次都要重新计算总共的值 以免存在多列同时修改的情况
                        for (var j = 1; j <= 12; j++) {
                            if (j == i) {//累加当前列的当前值
                                if (e.dataInfo.fieldName == 'PLANQUANTITY' + j) {
                                    drPlanQty += e.dataInfo.value;//当前列计划值的当前值
                                    drActualQty += dt.data.items[k].data['ACTUALQUANTITY' + j];//实际值的原始值
                                }
                                else if (e.dataInfo.fieldName == 'ACTUALQUANTITY' + j) {
                                    drPlanQty += dt.data.items[k].data['PLANQUANTITY' + j];//计划值的原始值
                                    drActualQty += e.dataInfo.value;//当前列实际值的当前值
                                }
                            }
                            else {
                                drPlanQty += dt.data.items[k].data['PLANQUANTITY' + j];
                                drActualQty += dt.data.items[k].data['ACTUALQUANTITY' + j];
                            }
                        }
                        e.dataInfo.dataRow.set('PLANQUANTITY13', drPlanQty);
                        e.dataInfo.dataRow.set('ACTUALQUANTITY13', drActualQty);
                        if (drPlanQty > 0 && drActualQty > 0) {
                            if (drPlanQty >= drActualQty) {
                                e.dataInfo.dataRow.set('ACCURACYRATE13', drActualQty / drPlanQty);
                            }
                            else {
                                e.dataInfo.dataRow.set('ACCURACYRATE13', 2 - drActualQty / drPlanQty);
                            }
                        }
                        else {
                            e.dataInfo.dataRow.set('ACCURACYRATE13', 0);
                        }
                    }
                    //#endregion
                }
            }
            break;
    }


}