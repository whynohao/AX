/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

purStockTurnoverRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = purStockTurnoverRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = purStockTurnoverRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'TURNOVERRATE', '月存货周转率', function (rec) {
        return rec.get('YEAR') + '年 ';
    }, 'month', true,false);
};