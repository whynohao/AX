/// <reference path="../pls/plsProduceSendVcl.js" />
stkWorkFullRateVcl = function () {
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
        if (sd.record.data['WORKNUMBER' + idx] != 0) {
            var value = sd.record.data['FULLWORKNUMBER' + idx] / sd.record.data['WORKNUMBER' + idx];
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
var proto = stkWorkFullRateVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = stkWorkFullRateVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName != 'PRODUCTTYPEID' && e.dataInfo.fieldName != 'PRODUCTTYPENAME') {
                var i;
                switch (len) {

                    case 11: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 12: i = e.dataInfo.fieldName.substring(len - 2); break;
                    case 15: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 16: i = e.dataInfo.fieldName.substring(len - 2); break;

                }
                //#region 重新计算齐套率
                if (i < 32) {
                    //修改数量后移开光标，重新计算齐套率
                    if (e.dataInfo.fieldName == 'WORKNUMBER' + i) {
                        var Qty = e.dataInfo.dataRow.data['FULLWORKNUMBER' + i];
                        if (Qty > 0 && e.dataInfo.value > 0) {
                            if (e.dataInfo.value >= Qty) {
                                e.dataInfo.dataRow.set('FULLWORKRATE' + i, Qty / e.dataInfo.value);
                            }

                        }
                        else {
                            e.dataInfo.dataRow.set('FULLWORKRATE' + i, 0);
                        }
                    }
                        //修改齐套数量后移开光标，重新计算齐套率
                    else if (e.dataInfo.fieldName == 'FULLWORKNUMBER' + i) {
                        var FullQty = e.dataInfo.dataRow.data['WORKNUMBER' + i];
                        if (FullQty > 0 && e.dataInfo.value > 0) {
                            if (FullQty >= e.dataInfo.value) {
                                e.dataInfo.dataRow.set('FULLWORKRATE' + i, e.dataInfo.value / FullQty);
                            }

                        }
                        else {
                            e.dataInfo.dataRow.set('FULLWORKRATE' + i, 0);
                        }

                    }
                }
                //#endregion

                //#region

                var drQty = 0;//当前行31天的作业数量总值
                var drActualQty = 0;//当前31天的齐套数量
                for (var k = 0; k < dt.data.length; k++) {
                    if (e.dataInfo.dataRow.data['PRODUCTTYPEID'] == dt.data.items[k].data.PRODUCTTYPEID) {
                        //每次都要重新计算总共的值 以免存在多列同时修改的情况
                        for (var j = 1; j <= 31; j++) {
                            if (j == i) {//累加当前列的当前值

                                if (e.dataInfo.fieldName == 'WORKNUMBER' + j) {
                                    drActualQty += dt.data.items[k].data['FULLWORKNUMBER' + j];
                                    drQty += e.dataInfo.value;
                                }
                                else if (e.dataInfo.fieldName == 'FULLWORKNUMBER' + j) {
                                    drQty += dt.data.items[k].data['WORKNUMBER' + j];
                                    drActualQty += e.dataInfo.value;
                                }
                            }
                            else {
                                drQty += dt.data.items[k].data['WORKNUMBER' + j];
                                drActualQty += dt.data.items[k].data['FULLWORKNUMBER' + j];
                            }
                        }
                        e.dataInfo.dataRow.set('WORKNUMBER32', drQty);
                        e.dataInfo.dataRow.set('FULLWORKNUMBER32', drActualQty);
                        if (drQty > 0 && drActualQty > 0) {
                            if (drQty >= drActualQty) {
                                e.dataInfo.dataRow.set('FULLWORKRATE32', drActualQty / drQty);
                            }

                        }
                        else {
                            e.dataInfo.dataRow.set('FULLWORKRATE32', 0);
                        }
                    }
                    //#endregion
                }
            }
            break;
    }


}