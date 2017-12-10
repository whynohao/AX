Ext.require('Ax.sys.sysKPIChart');

stkStockTurnoverRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkStockTurnoverRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkStockTurnoverRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'TURNOVERRATE', '月周转率', function (rec) {
        return rec.get('YEAR') + '年';
    }, 'month', true, false);
};