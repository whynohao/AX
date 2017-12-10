/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

StkFinalassemblyRevealView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = StkFinalassemblyRevealView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = StkFinalassemblyRevealView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'REACHRATE', '风冷总装累计泄漏率', function (rec) {
        return rec.get('YEAR') + '年' + '产品名称:' + rec.get('PRODUCTTYPENAME');
    }, 'month', true, false);
};