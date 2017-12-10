//EmSingleDetailRptView
EmSingleDetailRptView = function () {
    Ax.tpl.LibRptTpl.apply(this, arguments);
    this.vcl.funcView.add('proto.createChart', { name: 'createChart', display: '图表' });
};

var proto = EmSingleDetailRptView.prototype = Object.create(Ax.tpl.LibRptTpl.prototype);
proto.constructor = EmSingleDetailRptView;

proto.createChart = function () {
    return this.createThisChart.build(this);
};

proto.createThisChart = {
    build: function (view) {
        if (view.vcl.dataSet.dataList) {
            view.vcl.dataSet.dataList[0].ownGrid = null;
        }

        var me = view;
        var vcl = view.vcl;
        var dt = vcl.dataSet.getTable(0);
        var list = [];
        
        var rowNum = dt.getCount();
        

        var thisTitle = '';

        if (rowNum > 0) {
            thisTitle = '设备分析报表';
            for (var i = 1 ; i < 6 ; i++) {
                list.push({
                    'week': "第" + i + "周",
                    'MTBF': parseFloat(dt.data.items[4].data["WEEK" + i]),
                    'MTTR': parseFloat(dt.data.items[5].data["WEEK" + i])
                });
            }
        }
        this.myDataStore = Ext.create('Ext.data.JsonStore', {
            fields: ['week','MTBF','MTTR'],
            data: list
        });
       // this.myDataStore.loadData();
        var chart = Ext.create('Ext.chart.Chart', {
            width: '1000',
            height: 410,
            padding: '10 0 0 0',
            store: this.myDataStore,
            style: 'background:#fff',
            animate: true,
            shadow: true,
            theme: 'Category1',
            insetPadding: 35,
            legend: {
                position: 'right', labelFont: '10px Helvetica'
            },
            items: [
            {
                type: 'text', text: thisTitle, font: '22px Helvetica',
                width: 100, height: 40,
                x: 700, y: 12
            }],
            axes: [{
                type: 'Numeric',
                minimum: 0,
                position: 'left',
                fields: ['MTBF', 'MTTR'],
                title: '小时',
                minorTickSteps: 1,
                grid: {
                    odd: {
                        opacity: 1,
                        fill: '#ddd',
                        stroke: '#bbb',
                        'stroke-width': 0.5
                    }
                }
            }, {
                type: 'Category',
                position: 'bottom',
                fields: ['week'],
                label: {
                    font: '15px Helvetica',
                },
                title: '周'
            }],
            series: [{
                type: 'line',
                highlight: {
                    size: 7,
                    radius: 7
                },
                axis: 'left',
                xField: 'week', 
                yField: 'MTBF',
                smooth: true,
                markerConfig: {
                    type: 'cross',
                    size: 4,
                    radius: 4,
                    'stroke-width': 0
                },
                label: {
                    font: '15px Helvetica',
                },
                tips: {
                    trackMouse: true,
                    width: 130,
                    height: 50,
                    renderer: function (storeItem, item) {
                        this.setTitle('周:' + storeItem.get('week') + '<br/>MTBF:' + storeItem.get('MTBF'));
                    }
                }
            }, {
                type: 'line',
                highlight: {
                    size: 7,
                    radius: 7
                },
                axis: 'left',
                xField: 'week',
                yField: 'MTTR',
                smooth: true,
                markerConfig: {                   
                    type: 'circle',
                    size: 4,
                    radius: 4,
                    'stroke-width': 0
                },
                label: {
                    font: '15px Helvetica',
                },
                tips: {
                trackMouse: true,
                width: 130,
            height: 50,
            renderer: function (storeItem, item) {
                this.setTitle('周:' + storeItem.get('week') + '<br/>MTTR:' + storeItem.get('MTTR'));
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