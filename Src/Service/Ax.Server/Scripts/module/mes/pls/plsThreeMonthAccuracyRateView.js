/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

plsThreeMonthAccuracyRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = plsThreeMonthAccuracyRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = plsThreeMonthAccuracyRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ACCURACYRATE', '达成率', function (rec) {
        return rec.get('YEAR') + '年 ' + '产品类别：' + rec.get('PRODUCTTYPENAME');
    });
};