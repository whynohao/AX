Ext.require('Ax.sys.sysKPIChart');

stkRiskInventoryView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = stkRiskInventoryView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = stkRiskInventoryView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'RISKQUANTITYRATE', '风险库存比例', function (rec) {
        return rec.get('YEAR') + '年';
    }, 'month', true,false);
};