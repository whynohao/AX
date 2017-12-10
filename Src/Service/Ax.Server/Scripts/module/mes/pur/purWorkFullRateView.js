Ext.require('Ax.sys.sysKPIChart');

purWorkFullRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = purWorkFullRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = purWorkFullRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'FULLWORKRATE', '月份作业信息齐套率', function (rec) {
        return rec.get('YEAR') + '年' + '采购员:' + rec.get('PERSONNAME');
    }, 'month', true);
};