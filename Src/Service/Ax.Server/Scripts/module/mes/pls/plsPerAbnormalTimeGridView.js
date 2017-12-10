/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

plsPerAbnormalTimeGridView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = plsPerAbnormalTimeGridView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = plsPerAbnormalTimeGridView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'OCCUPYRATE', '异常时间占比', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月' + '产品类别：' + rec.get('TYPENAME') + '工段：' + rec.get('WORKSHOPSECTIONNAME') + '班组：' + rec.get('WORKTEAMNAME');
    }, 'day', true,false);
};