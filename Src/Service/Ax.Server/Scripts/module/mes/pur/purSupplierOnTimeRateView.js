/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

purSupplierOnTimeRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = purSupplierOnTimeRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = purSupplierOnTimeRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ONTIMEDELIVERYRATE', '供应商月按时到货率', function (rec) {
        return rec.get('YEAR') + '年' + '供方名称:' + rec.get('SUPPLIERNAME');
    }, 'month', true,false );
};