Ext.require('Ax.sys.sysKPIChart');

stkInventoryReachedRateView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkInventoryReachedRateView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkInventoryReachedRateView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'INVENTORYREACHEDRATE', '库存达成率', function (rec) {
        return rec.get('YEAR') + '年' + '仓库:' + rec.get('WAREHOUSENAME');
    }, 'month', true);
};