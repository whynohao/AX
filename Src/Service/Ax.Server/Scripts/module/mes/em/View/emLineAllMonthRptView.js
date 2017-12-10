Ext.require('Ax.sys.sysKPIChart');

emLineAllMonthRptView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = emLineAllMonthRptView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = emLineAllMonthRptView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'OEERATE', '年度生产线综合使用效率', function (rec) {
        return rec.get('CURRENTYEAR') + '年' + '产线:' + rec.get('PRODUCELINENAME');
    }, 'month', true,false);
};