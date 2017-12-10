/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

plsMonthOutputAccuracyRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = plsMonthOutputAccuracyRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = plsMonthOutputAccuracyRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ACCURACYRATE', '准确率', function (rec) {
        return rec.get('YEAR') + '年 ' + '产品类别：' + rec.get('PRODUCTTYPENAME');
    });
};