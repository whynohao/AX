/// <reference path="../pls/plsProduceSendVcl.js" />



purNoInformRateVcl = function () {
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
        if (sd.record.data['INFORMNUMBER' + idx] != 0) {
            var value = sd.record.data['NOINFORMNUMBER' + idx] / sd.record.data['INFORMNUMBER' + idx];
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
var proto = purNoInformRateVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = purNoInformRateVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName != 'PERSONID' && e.dataInfo.fieldName != 'PERSONNAME') {
                var i;
                switch (len) {
                    
                    case 15: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 16: i = e.dataInfo.fieldName.substring(len - 2); break;
                    case 13: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 14: i = e.dataInfo.fieldName.substring(len - 2); break;

                }
                //#region 重新计算按时到货率
                if (i < 13) {
                  
                   
                    if (e.dataInfo.fieldName == 'INFORMNUMBER' + i) {
                        var Qty = e.dataInfo.dataRow.data['NOINFORMNUMBER' + i];
                        if (Qty > 0 && e.dataInfo.value > 0) {
                            if (e.dataInfo.value >= Qty) {
                                e.dataInfo.dataRow.set('ONTIMINFORMRATE' + i, Qty / e.dataInfo.value);
                            }
                           
                        }
                        else {
                            e.dataInfo.dataRow.set('ONTIMINFORMRATE' + i, 0);
                        }
                    }
                      
                    else if (e.dataInfo.fieldName == 'NOINFORMNUMBER' + i) {
                        var NoinformQty = e.dataInfo.dataRow.data['INFORMNUMBER' + i];
                        if (NoinformQty > 0 && e.dataInfo.value > 0) {
                            if (NoinformQty >= e.dataInfo.value) {
                                e.dataInfo.dataRow.set('ONTIMINFORMRATE' + i, e.dataInfo.value / NoinformQty);
                            }
                           
                        }
                        else {
                            e.dataInfo.dataRow.set('ONTIMINFORMRATE' + i, 0);
                        }

                    }
                }
                //#endregion

                //#region
                var drQty = 0;//当前行12个月的数量总值
                var drNoinformQty = 0;
                for (var k = 0; k < dt.data.length; k++) {
                    if (e.dataInfo.dataRow.data['PERSONID'] == dt.data.items[k].data.PERSONID) {
                       
                        for (var j = 1; j <= 12; j++) {
                            if (j == i) {//累加当前列的当前值
                                //NOINFORMNUMBER INFORMNUMBER ONTIMINFORMRATE
                                if (e.dataInfo.fieldName == 'INFORMNUMBER' + j) {
                                    drNoinformQty += dt.data.items[k].data['NOINFORMNUMBER' + j];//实际值的原始值
                                    drQty += e.dataInfo.value;//当前列计划值的当前值
                                }
                                else if (e.dataInfo.fieldName == 'INFORMNUMBER' + j) {
                                    drQty += dt.data.items[k].data['NOINFORMNUMBER' + j];//计划值的原始值
                                    drNoinformQty += e.dataInfo.value;//当前列实际值的当前值
                                }
                            }
                            else {
                                drQty += dt.data.items[k].data['INFORMNUMBER' + j];
                                drNoinformQty += dt.data.items[k].data['NOINFORMNUMBER' + j];
                            }
                        }
                        e.dataInfo.dataRow.set('INFORMNUMBER13', drQty);
                        e.dataInfo.dataRow.set('NOINFORMNUMBER13', drNoinformQty);
                        if (drQty > 0 && drNoinformQty > 0) {
                            if (drQty >= drNoinformQty) {
                                e.dataInfo.dataRow.set('ONTIMINFORMRATE13', drNoinformQty / drQty);
                            }
                           
                        }
                        else {
                            e.dataInfo.dataRow.set('ONTIMINFORMRATE13', 0);
                        }
                    }
                    //#endregion
                }
            }
            break;
    }


}