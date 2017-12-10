/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

StkLowLevelMistakeView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = StkLowLevelMistakeView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = StkLowLevelMistakeView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'REACHRATE', '风冷螺杆总装台均低级错误', function (rec) {
        return rec.get('YEAR') + '年' + '产品名称:' + rec.get('PRODUCTTYPENAME');
    }, 'month', true, false);
};