/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

StkFinHeatExchangerRevealView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = StkFinHeatExchangerRevealView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = StkFinHeatExchangerRevealView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'REACHRATE', '风冷螺杆翅片换热器泄漏率', function (rec) {
        return rec.get('YEAR') + '年' + '产品名称:' + rec.get('PRODUCTTYPENAME');
    }, 'month', true, false);
};