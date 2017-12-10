Ext.require('Ax.sys.sysKPIChart');

stkReceiveResponceRateKpiView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkReceiveResponceRateKpiView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkReceiveResponceRateKpiView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ACTUALRATE', '仓管员收货响应及时率', function (rec) {
        return rec.get('FYEAR') + '年' + rec.get('FMONTH') + '月' + '仓管员:' + rec.get('PERSONNAME');
    }, 'day', true);
};