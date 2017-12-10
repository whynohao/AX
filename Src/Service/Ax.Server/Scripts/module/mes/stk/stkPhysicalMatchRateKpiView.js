Ext.require('Ax.sys.sysKPIChart');

stkPhysicalMatchRateKpiView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkPhysicalMatchRateKpiView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkPhysicalMatchRateKpiView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ACTUALRATE', '实际配套率', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月' + '产品:' + rec.get('PLANITEMTYPENAME') + '班组:' + rec.get('WORKTEAMNAME') + '工段:' + rec.get('WORKSHOPSECTIONNAME');
    }, 'day', true);
};