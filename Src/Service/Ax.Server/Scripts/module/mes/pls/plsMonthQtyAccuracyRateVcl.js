//plsMonthQtyAccuracyRateVcl
plsMonthQtyAccuracyRateVcl = function () {
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
        if (sd.record.data['PLANQTY' + idx] != 0) {
            var value = sd.record.data['ACTUALQTY' + idx] / sd.record.data['PLANQTY' + idx];
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
}
var proto = plsMonthQtyAccuracyRateVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = plsMonthQtyAccuracyRateVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName != 'PRODUCTTYPEID' && e.dataInfo.fieldName != 'PRODUCTTYPENAME') {
                var i;
                switch (len) {
                    case 8: i = e.dataInfo.fieldName.substring(len - 1); break;//PLANQTY1~ PLANQTY9
                    case 9: i = e.dataInfo.fieldName.substring(len - 2); break;//PLANQTY10~ PLANQTY13
                    case 10: i = e.dataInfo.fieldName.substring(len - 1); break;//ACTUALQTY1~ACTUALQTY9
                    case 11: i = e.dataInfo.fieldName.substring(len - 2); break;//ACTUALQTY10~ACTUALQTY13

                }
                //#region 重新计算准确率
                if (i < 13) {
                    //修改计划数量后移开光标，重新计算完成率
                    if (e.dataInfo.fieldName == 'PLANQTY' + i) {
                        var actualQty = e.dataInfo.dataRow.data['ACTUALQTY' + i];
                        if (actualQty > 0 && e.dataInfo.value > 0) {
                            e.dataInfo.dataRow.set('FINISHRATE' + i, actualQty / e.dataInfo.value);
                        }
                        else {
                            e.dataInfo.dataRow.set('FINISHRATE' + i, 0);
                        }
                    }
                        //修改完成数量后移开光标，重新计算完成率
                    else if (e.dataInfo.fieldName == 'ACTUALQTY' + i) {
                        var planQty = e.dataInfo.dataRow.data['PLANQTY' + i + ''];
                        if (planQty > 0 && e.dataInfo.value > 0) {
                            e.dataInfo.dataRow.set('FINISHRATE' + i, e.dataInfo.value / planQty);
                        }
                        else {
                            e.dataInfo.dataRow.set('FINISHRATE' + i, 0);
                        }

                    }
                }
                //#endregion

                var drPlanQty = 0;//当前行12个月的计划量总值
                var drActualQty = 0;//当前行12个月的完成量总值
                for (var k = 0; k < dt.data.length; k++) {
                    if (e.dataInfo.dataRow.data['PRODUCTTYPEID'] == dt.data.items[k].data.PRODUCTTYPEID) {
                        //每次都要重新计算总共的值 以免存在多列同时修改的情况
                        for (var j = 1; j <= 12; j++) {
                            if (j == i) {//累加当前列的当前值
                                if (e.dataInfo.fieldName == 'PLANQTY' + j) {
                                    drPlanQty += e.dataInfo.value;//当前列计划量的当前值
                                    drActualQty += dt.data.items[k].data['ACTUALQTY' + j];//完成量的原始值
                                }
                                else if (e.dataInfo.fieldName == 'ACTUALQTY' + j) {
                                    drPlanQty += dt.data.items[k].data['PLANQTY' + j];//计划量的原始值
                                    drActualQty += e.dataInfo.value;//当前列完成量的当前值
                                }
                            }
                            else {
                                drPlanQty += dt.data.items[k].data['PLANQTY' + j];
                                drActualQty += dt.data.items[k].data['ACTUALQTY' + j];
                            }
                        }
                        e.dataInfo.dataRow.set('PLANQTY13', drPlanQty);
                        e.dataInfo.dataRow.set('ACTUALQTY13', drActualQty);
                        if (drPlanQty > 0 && drActualQty > 0) {
                            e.dataInfo.dataRow.set('FINISHRATE13', drActualQty / drPlanQty);
                        }
                        else {
                            e.dataInfo.dataRow.set('FINISHRATE13', 0);
                        }
                    }
                }
            }
            break;
    }


}