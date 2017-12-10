Ext.require('Ax.sys.sysKPIChart');

stkMaterialMatOfWorkKpiView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkMaterialMatOfWorkKpiView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkMaterialMatOfWorkKpiView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ACTUALRATE', '作业号物料实际配套率', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月' + '产品类型:' + rec.get('PRODUCTTYPENAME') + '作业号:' + rec.get('WORKNO') + '物料:' + rec.get('MATERIALNAME');
    }, 'day', true);
};