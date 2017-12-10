Ext.require('Ax.sys.sysKPIChart');

purSupplierArrivalRateKpiView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = purSupplierArrivalRateKpiView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = purSupplierArrivalRateKpiView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ACTUALRATE', '供应商送货合格率', function (rec) {
        return rec.get('FYEAR') + '年' + '供应商:' + rec.get('SUPPLIERNAME');
    }, 'month', true);
};