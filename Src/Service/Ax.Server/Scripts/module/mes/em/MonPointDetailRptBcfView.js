MonPointDetailRptBcfView = function () {
    Ax.tpl.LibRptTpl.apply(this, arguments);
    this.vcl.funcView.add('proto.createChart', { name: 'createChart', display: '图表' });
    
};

var proto = MonPointDetailRptBcfView.prototype = Object.create(Ax.tpl.LibRptTpl.prototype);
proto.constructor = MonPointDetailRptBcfView;

proto.createChart = function () {
    return this.sysKPIChart.build(this);
};

//点位实时 报表 如果显示图形的数量大于20的时候,先取20个监控值显示
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
        var pointName = '';
        //点位实时 报表 如果显示图形的数量大于20的时候,先取20个监控值显示
        if (rowNum > 20)
        {
            rowNum = 20;
            pointName = dt.data.items[0].data["POINTNAME"];
        }

        for (var i = 0 ; i <= rowNum - 1 ; i++) {
            var temDate = dt.data.items[i].data["DATETIME"].toString();
            temDate = temDate.substr(0, 4) + '-' + temDate.substr(4, 2) + '-' + temDate.substr(6, 2) + '  ' + temDate.substr(8, 2) + ':' + temDate.substr(10, 2) + ':' + temDate.substr(12, 2);
            list.push({
                'pointId': dt.data.items[i].data["POINTNAME"],
                'value': dt.data.items[i].data["POINTVALUE"],
                'time': temDate//dt.data.items[i].data["DATETIME"]
            });
        }

        this.myDataStore = Ext.create('Ext.data.JsonStore', {
            fields: ['pointId', 'value', 'time'],
            data: list
        });

        var colors = ['#00ff00', '#ff0000'];
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
                type: 'text', text: '点位值报表', font: '22px Helvetica',
                width: 100, height: 30,
                x: 450, y: 12
            },
            {
                type: 'text', text: pointName, font: '12px Helvetica',
              
                x: 650, y: 12
            }
            ],

            axes: [{
                type: 'Numeric',
                position: 'left',
                grid: true,
                fields: 'value',
                minimum: 0  
            },
                {
                    type: 'Category',
                    position: 'bottom',
                    //reverse:true,
                    grid: true,
                    fields: 'time',
                    label: { rotate: { degrees: -45 } }
                }],

            series: [{
                type: 'line',
                axis: 'left',
                xField: 'time',
                yField: 'value',
                title:'点位',
                stacked: true,
                style: { 'stroke-width': 4 },
                highlight: {
                    fill: '#000',
                    radius: 5,
                    'stroke-width': 2,
                    stroke: '#fff'
                },
                tips: {
                    trackMouse: true,
                    style: 'background: #FFF',
                    height: 20,
                    showDelay: 0,
                    dismissDelay: 0,
                    hideDelay: 0,
                    renderer: function (storeItem, item) {
                        this.setTitle(storeItem.get('pointId') + ' 在 ' + storeItem.get('time') + '值为: ' + storeItem.get('value'));
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



