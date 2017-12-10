/// <reference path="../sys/sysKPIChart.js" />
Ext.require('Ax.sys.sysKPIChart');

plsPerCapitaOutPutView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = plsPerCapitaOutPutView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = plsPerCapitaOutPutView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'CAPITAOUTPUT', '人均产量', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月' + '产品类别：' + rec.get('TYPENAME') + '班组：' + rec.get('WORKTEAMNAME') + '工段：' + rec.get('WORKSHOPSECTIONNAME');
    }, 'day', true);
};