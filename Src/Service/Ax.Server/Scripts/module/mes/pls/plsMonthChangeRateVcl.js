//plsMonthChangeRateVcl.js
plsMonthChangeRateVcl = function () {
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
            var value = sd.record.data['CHANGEQUANTITY' + idx] / sd.record.data['PLANQUANTITY' + idx];
            if (value >= 0) {
                return '<span style="color:blue;">' + (value * 100).toFixed(2) + '%</span>';
            }
            else {
                return '<span style="color:red;">' + (value * 100).toFixed(2) + '%</span>';
            }
        }
        else
            return '<span style="color:green;">' + (0).toFixed(2) + '%</span>';
    }
    this.summaryRenderer.TotalName = function (v, sd, f) {//v代表当前列汇总值，sd包含各列汇总值，f代表当前列字段名
        return '<span style="color:darkred;font-weight:bold;">总计：</span>';
    }
}
var proto = plsMonthChangeRateVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = plsMonthChangeRateVcl;
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
                    case 15: i = e.dataInfo.fieldName.substring(len - 1); break;//CHANGEQUANTITY1~CHANGEQUANTITY9
                    case 16: i = e.dataInfo.fieldName.substring(len - 2); break;//CHANGEQUANTITY10~CHANGEQUANTITY13

                }
                //#region 重新计算变动率
                if (i < 13) {
                    //修改计划台数后移开光标，重新计算变动率
                    if (e.dataInfo.fieldName == 'PLANQUANTITY' + i) {
                        var actualQty = e.dataInfo.dataRow.data['CHANGEQUANTITY' + i];
                        if (actualQty > 0 && e.dataInfo.value > 0) {
                            e.dataInfo.dataRow.set('CHANGERATE' + i, actualQty / e.dataInfo.value);
                        }
                        else {
                            e.dataInfo.dataRow.set('CHANGERATE' + i, 0);
                        }
                    }
                        //修改变动台数后移开光标，重新计算变动率
                    else if (e.dataInfo.fieldName == 'CHANGEQUANTITY' + i) {
                        var planQty = e.dataInfo.dataRow.data['PLANQUANTITY' + i + ''];
                        if (planQty > 0 && e.dataInfo.value > 0) {
                            e.dataInfo.dataRow.set('CHANGERATE' + i, e.dataInfo.value / planQty);
                        }
                        else {
                            e.dataInfo.dataRow.set('CHANGERATE' + i, 0);
                        }

                    }
                }
                //#endregion

                var drPlanQty = 0;//当前行12个月的总计划台数
                var drActualQty = 0;//当前行12个月的总变动台数
                for (var k = 0; k < dt.data.length; k++) {
                    if (e.dataInfo.dataRow.data['PRODUCTTYPEID'] == dt.data.items[k].data.PRODUCTTYPEID) {
                        //每次都要重新计算总共的值 以免存在多列同时修改的情况
                        for (var j = 1; j <= 12; j++) {
                            if (j == i) {//累加当前列的当前值
                                if (e.dataInfo.fieldName == 'PLANQUANTITY' + j) {
                                    drPlanQty += e.dataInfo.value;//当前列计划台数的当前值
                                    drActualQty += dt.data.items[k].data['CHANGEQUANTITY' + j];//变动台数的原始值
                                }
                                else if (e.dataInfo.fieldName == 'CHANGEQUANTITY' + j) {
                                    drPlanQty += dt.data.items[k].data['PLANQUANTITY' + j];//计划值的原始值
                                    drActualQty += e.dataInfo.value;//当前列变动台数的当前值
                                }
                            }
                            else {
                                drPlanQty += dt.data.items[k].data['PLANQUANTITY' + j];
                                drActualQty += dt.data.items[k].data['CHANGEQUANTITY' + j];
                            }
                        }
                        e.dataInfo.dataRow.set('PLANQUANTITY13', drPlanQty);
                        e.dataInfo.dataRow.set('CHANGEQUANTITY13', drActualQty);
                        if (drPlanQty > 0 && drActualQty > 0) {
                            e.dataInfo.dataRow.set('CHANGERATE13', drActualQty / drPlanQty);
                        }
                        else {
                            e.dataInfo.dataRow.set('CHANGERATE13', 0);
                        }
                    }
                }
            }
            break;
    }


}