/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

StkMissionFirstPassRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = StkMissionFirstPassRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = StkMissionFirstPassRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'REACHRATE', '风冷螺杆一次试车合格率', function (rec) {
        return rec.get('YEAR') + '年' + '产品名称:' + rec.get('PRODUCTTYPENAME');
    }, 'month', true, false);
};