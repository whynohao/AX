//plsMonthChangeRateVcl.js
stkStockSummaryKpiVcl = function () {
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
        if (sd.record.data['MATERIALNUM' + idx] != 0) {
            var value = sd.record.data['HASMATERIALNUM' + idx] / sd.record.data['MATERIALNUM' + idx];
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
var proto = stkStockSummaryKpiVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = stkStockSummaryKpiVcl;
