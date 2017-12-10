/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

plsMonthQtyAccuracyRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = plsMonthQtyAccuracyRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = plsMonthQtyAccuracyRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'FINISHRATE', '完成率', function (rec) {
        return rec.get('YEAR') + '年 ' + '产品类别：' + rec.get('PRODUCTTYPENAME');
    });
};