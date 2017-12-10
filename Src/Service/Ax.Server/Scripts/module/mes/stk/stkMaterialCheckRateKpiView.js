Ext.require('Ax.sys.sysKPIChart');

stkMaterialCheckRateKpiView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkMaterialCheckRateKpiView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkMaterialCheckRateKpiView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'CHECKRATE', '检验效率', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月' + '检验员:' + rec.get('PERSONNAME');
    }, 'day', true);
};