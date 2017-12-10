/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

plsMonthChangeRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = plsMonthChangeRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = plsMonthChangeRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'CHANGERATE', '变动率', function (rec) {
        return rec.get('YEAR') + '年 ' + '产品类别：' + rec.get('PRODUCTTYPENAME');
    });
};