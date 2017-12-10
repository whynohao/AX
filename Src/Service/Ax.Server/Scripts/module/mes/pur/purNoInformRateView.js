/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

purNoInformRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = purNoInformRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = purNoInformRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ONTIMINFORMRATE', '送货通知下达及时率', function (rec) {
        return rec.get('YEAR') + '年 ' + '采购员：' + rec.get('PERSONNAME');
    }, 'month', true);
};