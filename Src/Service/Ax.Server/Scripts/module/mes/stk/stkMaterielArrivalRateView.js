Ext.require('Ax.sys.sysKPIChart');

stkMaterielArrivalRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkMaterielArrivalRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkMaterielArrivalRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ONTIMEDELIVERYRATE', '仓管员按时到货率', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月' + '仓管员:' + rec.get('PERSONNAME');
    }, 'day', true);
};