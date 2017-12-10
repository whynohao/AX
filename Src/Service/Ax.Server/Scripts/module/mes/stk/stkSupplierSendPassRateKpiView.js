Ext.require('Ax.sys.sysKPIChart');

stkSupplierSendPassRateKpiView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkSupplierSendPassRateKpiView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkSupplierSendPassRateKpiView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ACTUALRATE', '供应商送货合格率', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月' + '供应商:' + rec.get('SUPPLIERNAME') + '物料:' + rec.get('MATERIALNAME');
    }, 'day', true);
};