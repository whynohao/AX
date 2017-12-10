Ext.require('Ax.sys.sysKPIChart');

purMaterielArrivalRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = purMaterielArrivalRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = purMaterielArrivalRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'ONTIMEDELIVERYRATE', '按时到货率', function (rec) {
        return rec.get('YEAR') + '年' + '采购员:' + rec.get('PERSONNAME');
    },'month',true);
};