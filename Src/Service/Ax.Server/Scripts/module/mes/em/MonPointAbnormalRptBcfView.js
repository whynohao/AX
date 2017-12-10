MonPointAbnormalRptBcfView = function () {
    Ax.tpl.LibRptTpl.apply(this, arguments);
    this.vcl.funcView.add('proto.createChart', { name: 'createChart', display: '图表' });
};

var proto = MonPointAbnormalRptBcfView.prototype = Object.create(Ax.tpl.LibRptTpl.prototype);
proto.constructor = MonPointAbnormalRptBcfView;

proto.createChart = function () {
    return this.sysKPIChart.build(this);
};

proto.sysKPIChart = {
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
            checkDate = dt.data.items[0].data["DATETIME"].toString();
        }
        checkDate = checkDate.substr(0, 4) + '年' + checkDate.substr(4, 2) + '月' + checkDate.substr(6, 2) + '日';
        for (var i = 0; i <= rowNum - 1; i++) {
            list.push({
                'pointId': dt.data.items[i].data["POINTNAME"],
                'abnormalcount': dt.data.items[i].data["ABNORMALCOUNT"],
                'normalcount': dt.data.items[i].data["NORMALCOUNT"]
            });
        }


        this.myDataStore = Ext.create('Ext.data.JsonStore', {
            fields: ['pointId', 'abnormalcount', 'normalcount'],
            data: list
        });

        //自定义一个样式 (颜色)
        var colors = ['#ff0000', '#00ff00'];
        Ext.define('Ext.chart.theme.MyFancy', {
            extend: 'Ext.chart.theme.Base',
            constructor: function (config) {
                this.callParent([Ext.apply({
                    colors: colors
                }, config)]);
            }
        });

        var chart = Ext.create('Ext.chart.Chart', {
            xtype: 'chart',
            width: '1000',
            height: 410,
            padding: '10 0 0 0',
            theme: 'MyFancy',
            animate: true,
            shadow: false,
            style: 'background: #fff;',
            legend: { position: 'bottom', boxStrokeWidth: 0, labelFont: '12px Helvetica' },
            store: this.myDataStore,
            insetPadding: 40,

            items: [
            {
                type: 'text', text: '点位异常报表', font: '22px Helvetica',
                width: 100, height: 30,
                x: 450, y: 12
            },
             {
                 type: 'text', text: checkDate, font: '16px Helvetica',
                 width: 100, height: 30,
                 x: 650, y: 10
             }
            ],
            //axes属性的坐标轴是一个数组，一般指定两个，每个坐标轴有type, position, fields和title等属性需要设置
            axes: [{
                type: 'Numeric',//类型
                position: 'left',//位置
                grid: true,
                fields: ['abnormalcount'],//对应store的字段
                minimum: 0,
                maximum: 10
                //title:'标题'
            },
                {
                    type: 'Category',
                    position: 'bottom',
                    grid: true,
                    fields: ['pointId'],
                    label: { rotate: { degrees: -45 } }
                }],
            //series属性的曲线，可以有多条。每一条有type, xField, yField等常用属性。
            series: [{
                type: 'column',
                axis: 'left',
                title: ['异常数', '正常数'],
                xField: 'pointId',
                yField: ['abnormalcount', 'normalcount'],
                stacked: true,
                style: { opacity: 0.80, width:50 },
                highlight: { fill: '#000', 'stroke-width': 1000, stroke: '#fff' },
                tips: {
                    trackMouse: true,
                    style: 'background: #FFF',
                    height: 20,
                    renderer: function (storeItem, item) {
                        var browser = item.series.title[Ext.Array.indexOf(item.series.yField, item.yField)];
                        this.setTitle(storeItem.get('pointId') + ' 的 ' + browser + ': ' + storeItem.get(item.yField));
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

        var savepic = Ext.create(Ext.Action, {
            text: '保存图形',
            handler: function () {
                Ext.MessageBox.confirm('确认下载', '你要下载该报表为一张图片吗?', function (choice) {
                    if (choice == 'yes') {
                        chart.save({
                            type: 'image/png'
                        });
                    }
                });
            }
        });

        var mainWidth = document.body.clientWidth > 1210 ? document.body.clientWidth - 10 : 1210;
        var mainPanel = Ext.create('Ext.panel.Panel', {
            width: mainWidth,
            height: document.body.clientHeight - 80,
            layout: { type: 'fit' },
            items: chart,
            border: false,
            tbar: Ax.utils.LibToolBarBuilder.createToolBar([select, refresh, vcl.createChangeView(me, 'create', 'createChart', '图表'), savepic])
        });
        return mainPanel;

    }
};



