Ext.require('Ax.sys.sysKPIChart');

StkContainerExpandView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = StkContainerExpandView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = StkContainerExpandView;

proto.createChart = function () {
    return sysKPIChart.build(this, 'REACHRATE', '螺杆机容器累计胀接泄漏率', function (rec) {
        return rec.get('YEAR') + '年' + '产品名称:' + rec.get('PRODUCTTYPENAME');
    }, 'month', true,false);
};