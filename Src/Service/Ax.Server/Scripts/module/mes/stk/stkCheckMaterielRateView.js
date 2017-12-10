Ext.require('Ax.sys.sysKPIChart');

stkCheckMaterielRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkCheckMaterielRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkCheckMaterielRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ONTIMECHECKRATE', '物料检验及时率', function (rec) {
        return rec.get('YEAR') + '年' + rec.get('MONTH') + '月' + '检验员:' + rec.get('PERSONNAME');
    }, 'day', true);
};