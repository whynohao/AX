Ext.require('Ax.sys.sysKPIChart');

stkPhysicalMatOfWorkKpiView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkPhysicalMatOfWorkKpiView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkPhysicalMatOfWorkKpiView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ACTUALRATE', '实际配套率', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月' + '产品:' + rec.get('PRODUCTTYPENAME') + '作业:' + rec.get('WORKNO') + '物料:' + rec.get('MATERIALNAME');
    }, 'day', true);
};