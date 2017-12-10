//柱状图

EmEquipmentOeeDayRptView = function () {
    Ax.tpl.LibRptTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = EmEquipmentOeeDayRptView.prototype = Object.create(Ax.tpl.LibRptTpl.prototype);
proto.constructor = EmEquipmentOeeDayRptView;
proto.createChart = function () {
    return this.EMsysKPIChart.build(this);
};
proto.EMsysKPIChart = {
    build: function (view) {
        if (view.vcl.dataSet.dataList) {
            view.vcl.dataSet.dataList[0].ownGrid = null;
        }
        var me = view;
        var vcl = view.vcl;
        var dt = vcl.dataSet.getTable(0);
        var list = [];
        var rowNum = dt.getCount();
        var checkDate = "";
        if (rowNum > 0) {
            checkDate = dt.data.items[0].data["RPTDATE"].toString();
        }
        checkDate = checkDate.substr(0, 4) + '年' + checkDate.substr(4, 2) + '月' + checkDate.substr(6, 2) + '日';

        for (var i = 0; i <= rowNum - 1; i++) {
            list.push({
                'equipmentName': dt.data.items[i].data["EQUIPMENTNAME"],
                'oeeRate': dt.data.items[i].data["OEERATE"]
            });
            if (list.length > 24) break;
        }

        //Ext数据源
        this.myDataStore = Ext.create('Ext.data.JsonStore', {
            fields: ['equipmentName', 'oeeRate'],
            data: list
        });

        //自定义一个样式 (颜色)
        //var colors = ['#ff0000'];

        //Ext.define('Ext.chart.theme.MyFancy', {
        //    extend: 'Ext.chart.theme.Base',
        //    constructor: function (config) {
        //        this.callParent(config);
        //    }
        //});

        var chart = Ext.create('Ext.chart.Chart', {
            xtype: 'chart',
            width: 1000,
            height: 410,
            padding: '10 0 0 0',
            theme: 'Category1',//'MyFancy',
            animate: true,
            shadow: false,
            style: 'background: #fff;',
            legend: { position: 'bottom', boxStrokeWidth: 0, labelFont: '12px Helvetica' },
            store: this.myDataStore,
            insetPadding: 40,
            //标题
            items: [
            {
                type: 'text', text: '设备OEE日报表', font: '22px Helvetica',
                width: 100, height: 30,
                x: 450, y: 12
            },
             {
                 type: 'text', text: checkDate, font: '16px Helvetica',
                 width: 100, height: 30,
                 x: 650, y: 12
             }
            ],
            //axes属性的坐标轴是一个数组，一般指定两个，每个坐标轴有type, position, fields和title等属性需要设置
            axes: [{
                type: 'Numeric',
                position: 'left',
                grid: true,
                fields: 'oeeRate',//对应store的字段
                minimum: 5,
                maximum: 10,
                title: '综合效率'
            },
            {
                type: 'Category',
                position: 'bottom',
                grid: true,
                fields: 'equipmentName',
                label: {
                    rotate: { degrees: -45 }
                    }
                }],
            //series属性的曲线，可以有多条。每一条有type, xField, yField等常用属性。
            series: [{
                type: 'column',
                axis: 'left',
                title: '设备综合效率',
                xField: 'equipmentName',
                yField: 'oeeRate',
                stacked: true,
                style: { opacity: 0.80, width: 30 },
                highlight: { fill: '#000', 'stroke-width': 1000, stroke: '#fff' },
                tips: {
                    trackMouse: true,
                    style: 'background: #FFF',
                    height: 20,
                    renderer: function (storeItem, item) {
                        var browser = item.series.title[Ext.Array.indexOf(item.series.yField, item.yField)];
                        this.setTitle(storeItem.get('equipmentName') + '综合效率' + storeItem.get('oeeRate'));
                    }
                }
            }]
        });


        var select = Ext.create(Ext.Action, {
            text: '查询',
            hidden: vcl.isEdit,
            handler: function () {
                Ax.utils.LibQueryForm.createForm(vcl, undefined, undefined, function () {
                    vcl.win.removeAll();
                    vcl.win.add(me.createChart());
                });
            }
        });
        var refresh = Ext.create(Ext.Action, {
            text: '刷新',
            hidden: vcl.isEdit,
            handler: function () {
                vcl.showRpt(vcl.queryCondition);
                vcl.win.removeAll();
                vcl.win.add(me.createChart());
            } 
        });
        var minWidth = list.length>10 ? list.length*40 : 400;
        var mainWidth = document.body.clientWidth > 1210 ? document.body.clientWidth - 10 : 1210;
        var mainPanel = Ext.create('Ext.panel.Panel', {
            width: mainWidth,
            minWidth: minWidth,
            height: document.body.clientHeight - 80,
            layout: { type: 'fit' },
            items: chart,
            border: false,
            tbar: Ax.utils.LibToolBarBuilder.createToolBar([select, refresh, vcl.createChangeView(me, 'create', 'createChart', '图表')])
        });
        return mainPanel;
    }
}