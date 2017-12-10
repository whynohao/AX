/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

StkSMissionFirstPassRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = StkSMissionFirstPassRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = StkSMissionFirstPassRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'REACHRATE', '水冷螺杆一次试车合格率', function (rec) {
        return rec.get('YEAR') + '年' + '产品名称:' + rec.get('PRODUCTTYPENAME');
    }, 'month', true, false);
};