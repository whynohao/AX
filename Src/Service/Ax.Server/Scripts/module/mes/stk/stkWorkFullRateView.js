Ext.require('Ax.sys.sysKPIChart');

stkWorkFullRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkWorkFullRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkWorkFullRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'FULLWORKRATE', '齐套率', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月' + '产品类别:' + rec.get('PRODUCTTYPENAME');
    }, 'day', true);
};