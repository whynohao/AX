Ext.require('Ax.sys.sysKPIChart');

stkPhysicalMatOfSectionKpiView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkPhysicalMatOfSectionKpiView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkPhysicalMatOfSectionKpiView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ACTUALRATE', '实际配套率', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月' + '产品:' + rec.get('PRODUCTTYPENAME') + '班组:' + rec.get('WORKTEAMNAME') + '工段:' + rec.get('WORKSHOPSECTIONNAME') + '物料:' + rec.get('MATERIALNAME');
    }, 'day', true);
};