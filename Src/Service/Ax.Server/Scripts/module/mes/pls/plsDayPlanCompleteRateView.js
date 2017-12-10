/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

plsDayPlanCompleteRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = plsDayPlanCompleteRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = plsDayPlanCompleteRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'COMPLETERATE', '达成率', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月 ' + '机型:' + rec.get('MODEL') + ' 工段:' + rec.get('WORKSHOPSECTIONNAME') + ' 班组:' + rec.get('WORKTEAMNAME');
    }, 'day');
};