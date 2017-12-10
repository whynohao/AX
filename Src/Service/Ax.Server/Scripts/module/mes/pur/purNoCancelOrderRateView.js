Ext.require('Ax.sys.sysKPIChart');

purNoCancelOrderRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = purNoCancelOrderRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = purNoCancelOrderRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ONRATE', '未及时取消采购订单数量所占百分比', function (rec) {
        return rec.get('YEAR') + '年' + '采购员:' + rec.get('PERSONNAME');
    }, 'month', true);
};