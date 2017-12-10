/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

plsOnTimeCompleteRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = plsOnTimeCompleteRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = plsOnTimeCompleteRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'COMPLETERATE', '按时完成率', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月 ' + '机型:' + rec.get('MODEL') + ' 工段:' + rec.get('WORKSHOPSECTIONNAME') + ' 班组:' + rec.get('WORKTEAMNAME');
    }, 'day');
};