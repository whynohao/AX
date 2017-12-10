/// <reference path="../pls/plsProduceSendVcl.js" />
stkSTestRunPercentPasVcl = function () {
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
        if (sd.record.data['TARGETNUMBER' + idx] != 0) {
            var value = sd.record.data['ACTUALNUMBER' + idx] / sd.record.data['TARGETNUMBER' + idx];
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
var proto = stkSTestRunPercentPasVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = stkSTestRunPercentPasVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName != 'PRODUCTTYPEID' && e.dataInfo.fieldName != 'PRODUCTTYPENAME') {
                var i;
                switch (len) {
                    case 13: i = e.dataInfo.fieldName.substring(len - 1); break;
                    case 14: i = e.dataInfo.fieldName.substring(len - 2); break;
                }
                if (i < 13) {
                    //计算达成率
                    if (e.dataInfo.fieldName == 'TARGETNUMBER' + i) {
                        var actualQty = e.dataInfo.dataRow.data['ACTUALNUMBER' + i];
                        if (actualQty > 0 && e.dataInfo.value > 0) {
                            if (e.dataInfo.value >= actualQty) {
                                e.dataInfo.dataRow.set('REACHRATE' + i, actualQty / e.dataInfo.value);
                            }
                        }
                        else {
                            e.dataInfo.dataRow.set('REACHRATE' + i, 0);
                        }
                    }
                    else if (e.dataInfo.fieldName == 'ACTUALNUMBER' + i) {
                        var planQty = e.dataInfo.dataRow.data['TARGETNUMBER' + i];
                        if (planQty > 0 && e.dataInfo.value > 0) {
                            if (planQty >= e.dataInfo.value) {
                                e.dataInfo.dataRow.set('REACHRATE' + i, e.dataInfo.value / planQty);
                            }
                        }
                        else {
                            e.dataInfo.dataRow.set('REACHRATE' + i, 0);
                        }
                    }
                }
            }
            break;
    }


}