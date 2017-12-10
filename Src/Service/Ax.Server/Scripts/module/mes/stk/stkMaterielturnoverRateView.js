Ext.require('Ax.sys.sysKPIChart');

stkMaterielturnoverRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkMaterielturnoverRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkMaterielturnoverRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'RATE', '比率', function (rec) {
        return rec.get('YEAR') + '年';
    }, 'month', true,false);
};