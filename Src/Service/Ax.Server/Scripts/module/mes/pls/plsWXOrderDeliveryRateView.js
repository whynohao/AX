/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

plsWXOrderDeliveryRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = plsWXOrderDeliveryRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = plsWXOrderDeliveryRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'DELIVERYRATE', '按时交货率', function (rec) {
        return rec.get('YEAR') + '年 ' + '产品类别：' + rec.get('PRODUCTTYPENAME');
    });
};