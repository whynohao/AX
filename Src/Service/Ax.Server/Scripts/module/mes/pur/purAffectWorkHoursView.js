/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

purAffectWorkHoursView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = purAffectWorkHoursView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = purAffectWorkHoursView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'AFFECTHOURS', '影响工时(分钟)', function (rec) {
        return rec.get('YEAR') + '年 ' + '采购员：' + rec.get('PERSONNAME');
    }, 'month', false);
};