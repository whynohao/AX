/// <reference path="../pls/plsProduceSendVcl.js" />
stkCheckMaterielRateVcl = function () {
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
        if (sd.record.data['CHECKNUMBER' + idx] != 0) {
            var value = sd.record.data['ONTIMECHECKNUMBER' + idx] / sd.record.data['CHECKNUMBER' + idx];
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
var proto =stkCheckMaterielRateVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor =stkCheckMaterielRateVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName != 'PERSONID' && e.dataInfo.fieldName != 'PERSONNAME') {
                var i;
                switch (len) {
                   
                    case 12: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 13: i = e.dataInfo.fieldName.substring(len - 2); break;
                    case 18: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 19: i = e.dataInfo.fieldName.substring(len - 2); break;

                }
                //#region 重新计算检验及时率
                if (i < 32) {
                    //修改送货数量后移开光标，重新计算检验及时率
                    if (e.dataInfo.fieldName == 'CHECKNUMBER' + i) {
                        var actualQty = e.dataInfo.dataRow.data['ONTIMECHECKNUMBER' + i];
                        if (actualQty > 0 && e.dataInfo.value > 0) {
                            if (e.dataInfo.value >= actualQty) {
                                e.dataInfo.dataRow.set('ONTIMECHECKRATE' + i, actualQty / e.dataInfo.value);
                            }

                        }
                        else {
                            e.dataInfo.dataRow.set('ONTIMECHECKRATE' + i, 0);
                        }
                    }
                        //修改实际数量后移开光标，重新计算检验及时率
                    else if (e.dataInfo.fieldName == 'ONTIMECHECKNUMBER' + i) {
                        var planQty = e.dataInfo.dataRow.data['CHECKNUMBER' + i];
                        if (planQty > 0 && e.dataInfo.value > 0) {
                            if (planQty >= e.dataInfo.value) {
                                e.dataInfo.dataRow.set('ONTIMECHECKRATE' + i, e.dataInfo.value / planQty);
                            }

                        }
                        else {
                            e.dataInfo.dataRow.set('ONTIMECHECKRATE' + i, 0);
                        }

                    }
                }
                //#endregion

                //#region
                var drPlanQty = 0;//当前行12个月的数量总值
                var drActualQty = 0;//当前行12个月的按时检验数量量总值
                for (var k = 0; k < dt.data.length; k++) {
                    if (e.dataInfo.dataRow.data['PERSONID'] == dt.data.items[k].data.PERSONID) {
                        //每次都要重新计算总共的值 以免存在多列同时修改的情况
                        for (var j = 1; j <= 31; j++) {
                            if (j == i) {//累加当前列的当前值
                               
                                if (e.dataInfo.fieldName == 'CHECKNUMBER' + j) {
                                    drActualQty += dt.data.items[k].data['ONTIMECHECKNUMBER' + j];
                                    drPlanQty += e.dataInfo.value;
                                }
                                else if (e.dataInfo.fieldName == 'ONTIMECHECKNUMBER' + j) {
                                    drPlanQty += dt.data.items[k].data['CHECKNUMBER' + j];
                                    drActualQty += e.dataInfo.value;
                                }
                            }
                            else {
                                drPlanQty += dt.data.items[k].data['CHECKNUMBER' + j];
                                drActualQty += dt.data.items[k].data['ONTIMECHECKNUMBER' + j];
                            }
                        }
                        e.dataInfo.dataRow.set('CHECKNUMBER32', drPlanQty);
                        e.dataInfo.dataRow.set('ONTIMECHECKNUMBER32', drActualQty);
                        if (drPlanQty > 0 && drActualQty > 0) {
                            if (drPlanQty >= drActualQty) {
                                e.dataInfo.dataRow.set('ONTIMECHECKRATE32', drActualQty / drPlanQty);
                            }

                        }
                        else {
                            e.dataInfo.dataRow.set('ONTIMECHECKRATE32', 0);
                        }
                    }
                    //#endregion
                }
            }
            break;
    }


}