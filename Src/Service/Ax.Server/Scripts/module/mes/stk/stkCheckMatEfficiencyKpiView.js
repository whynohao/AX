Ext.require('Ax.sys.sysKPIChart');

stkCheckMatEfficiencyKpiView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkCheckMatEfficiencyKpiView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkCheckMatEfficiencyKpiView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'CHECKRATE', '物料检验效率', function (rec) {
        return rec.get('FYEAR') + '年' + rec.get('FMONTH') + '月' + '仓管员:' + rec.get('PERSONNAME');
    }, 'day', true);
};