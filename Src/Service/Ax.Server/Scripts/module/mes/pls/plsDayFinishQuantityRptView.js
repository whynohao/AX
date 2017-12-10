//plsDayFinishQuantityRptView
plsDayFinishQuantityRptView = function () {
    Ax.tpl.LibRptTpl.apply(this, arguments);
    //this.vcl.funcView.add('proto.createChart', { name: 'createChart', display: '图表' });
    if (this.vcl.funcView.containsKey("default")) {
        this.vcl.funcView.get("default").name = "createChart";
    }
};

var proto = plsDayFinishQuantityRptView.prototype = Object.create(Ax.tpl.LibRptTpl.prototype);
proto.constructor = plsDayFinishQuantityRptView;

proto.createChart = function () {
    return this.createThisChart.build(this);
};

proto.createThisChart = {
    build: function (view) {
        var me = view;
        var vcl = view.vcl;
        if (view.vcl.dataSet.dataList) {
            view.vcl.dataSet.dataList[0].ownGrid = null;
            var doShowRpt = function (field, value, eOpts) {
                vcl.currentDate = value;
                vcl.showRpt(this.vcl.queryCondition);
            };
            var dt = vcl.dataSet.getTable(0);
            var list = [];
            var listhour = [];
            var hour = 0;
            var quantity = 0;
            var outquantity = 0;
            var efficinecy = 0;
            var efficinecyand = 0;
            var rowNum = dt.getCount();
            debugger

            var thisTitle = '';
            //点位实时 报表 如果显示图形的数量大于20的时候,先取20个监控值显示
            if (rowNum > 0) {
                // var checkDate = dt.data.items[0].data["YEAR"].toString() + ;
                //var lineName = dt.data.items[0].data["EQUIPMENTID"].toString();
                // checkDate = dt.data.items[0].data["YEAR"].toString() + '年' + dt.data.items[0].data["MONTH"].toString() + '月'
                thisTitle = '4分厂效率报表';

            }
            debugger
            for (var i = 0 ; i < 24; i++) {
                hour = parseInt(i);
                if (rowNum > 0) {
                    for (var min = 0; min < rowNum; min++) {
                        if (parseInt(dt.data.items[min].data["STARTTIME"]) == hour) {
                            outquantity = parseInt(dt.data.items[min].data["OUTQUANTITY"]);
                            quantity = parseInt(dt.data.items[min].data["QUANTITY"]);
                            efficinecy = parseInt(parseFloat(dt.data.items[min].data["EFFICIENCY"]) * 1000) / 10;
                            efficinecyand = parseInt(parseFloat(dt.data.items[min].data["EFFICIENCYAND"]) * 1000) / 10;
                            break;
                        }
                        else {
                            outquantity = 0;
                            quantity = 0;
                            efficinecy = 0;

                        }
                    }
                }
                list.push({
                    'week': i + "时",
                    'weeks': (i + 1) + "时",
                    'OUTQUANTITY': outquantity,
                    'QUANTITY': quantity,
                    'EFFICIENCY': efficinecy,
                    'EFFICIENCYAND': efficinecyand,
                });
            }
            debugger
            this.myDataStore = Ext.create('Ext.data.JsonStore', {
                fields: ['week', 'OUTQUANTITY', "QUANTITY", "EFFICIENCY", 'EFFICIENCYAND'],
                data: list
            });

            debugger
            var chart = Ext.create('Ext.chart.Chart', {
                store: this.myDataStore,
                style: 'background:#fff',
                theme: 'Category2',
                animate: true,
                shadow: true,
                insetPadding: 40,
                legend: {
                    position: 'top'
                },
                axes: [{
                    type: 'Numeric',
                    position: 'left',
                    fields: ['QUANTITY', 'OUTQUANTITY'],
                    displayfields: '数量',
                    minimum: 0,
                    grid: true,
                    title: '数量',
                    maximum: 100,
                    minimum: 0,
                    minorTickSteps: 1,
                    grid: {
                        odd: {
                            opacity: 1,
                            fill: '#ddd',
                            stroke: '#bbb',
                            'stroke-width': 0.5
                        }
                    }
                },
                {
                    type: 'Numeric',
                    position: 'right',
                    fields: ['EFFICIENCY'],
                    displayfields: '效率(%)',
                    title: '效率(%)',
                    maximum: 100,
                    minimum: 0,
                    minorTickSteps: 1
                },

                    {
                        type: 'Category',
                        position: 'bottom',
                        fields: 'week',
                        title: '时间',
                    }],

                series: [{
                    type: 'column',                 //类型：条（柱状图）
                    //highlight: {
                    //    size: 7,
                    //    radius: 7,
                    //},

                    highlight: true,
                    axis: 'left',              //轴：底部、末端
                    xField: 'week',              //X轴：自定义
                    yField:  'QUANTITY',   //Y轴：自定义
                    //markerConfig: {
                    //    type: 'cross',
                    //    size: 4,
                    //    radius: 4,
                    //    'stroke-width': 0
                    //},
                    stacked: true,
                    title: '理论数量',
                   
                    label: {//设置每个标签的属性
                        display: 'insideEnd', //标签的位置
                        color: '#fffff',
                        'text-anchor': 'middle',
                        field: 'QUANTITY',
                        orientation: 'horizontal',
                        fill: '#000',
                        font: '15px Arial'
                    },
                    tips: {
                        trackMouse: true,
                        width: 130,
                        height: 50,
                        renderer: function (storeItem, item) {
                            this.setTitle('时段:' + (storeItem.get('week') + '-' + storeItem.get('weeks')) + '<br/>完成数量:' + storeItem.get('OUTQUANTITY') + '<br/>理论数量:' + storeItem.get('QUANTITY'));
                        }
                    },
                },
                {
                    type: 'column',                 //类型：条（柱状图）
                    //highlight: {
                    //    size: 7,
                    //    radius: 7,
                    //},

                    highlight: true,
                    axis: 'left',              //轴：底部、末端
                    xField: 'week',              //X轴：自定义
                    yField:  'OUTQUANTITY',   //Y轴：自定义
                    //markerConfig: {
                    //    type: 'cross',
                    //    size: 4,
                    //    radius: 4,
                    //    'stroke-width': 0
                    //},
                    stacked: true,
                    title:  '实际数量',

                    label: {//设置每个标签的属性
                        display: 'insideEnd', //标签的位置
                        color: '#fffff',
                        'text-anchor': 'middle',
                        field:  'OUTQUANTITY',
                        orientation: 'horizontal',
                        fill: '#000',
                        font: '15px Arial'
                    },
                    tips: {
                        trackMouse: true,
                        width: 130,
                        height: 50,
                        renderer: function (storeItem, item) {
                            this.setTitle('时段:' + (storeItem.get('week') + '-' + storeItem.get('weeks')) + '<br/>完成数量:' + storeItem.get('OUTQUANTITY') + '<br/>理论数量:' + storeItem.get('QUANTITY'));
                        }
                    },
                },
               {
                   type: 'line',                 //类型：条（柱状图）
                   highlight: {
                       size: 7,
                       radius: 7,
                   },

                   highlight: true,
                   axis: 'right',              //轴：底部、末端
                   xField: 'week',              //X轴：自定义
                   yField: 'EFFICIENCY',   //Y轴：自定义
                   markerConfig: {
                       type: 'circle',
                       size: 4,
                       radius: 4,
                       'stroke-width': 1
                   },
                   title: '阶段效率(%)',

                   label: {//设置每个标签的属性
                       display: 'over', //标签的位置
                       color: '#23238E',
                       'text-anchor': 'middle',
                       field: ['EFFICIENCY'],
                       orientation: 'horizontal',
                       fill: '#000',
                       font: '15px Arial'
                   },
                   tips: {
                       trackMouse: true,
                       width: 130,
                       height: 50,
                       renderer: function (storeItem, item) {
                           this.setTitle('时段:' + (storeItem.get('week') + '-' + storeItem.get('weeks')) + '<br/>阶段效率:' + storeItem.get('EFFICIENCY') + "%");
                       }
                   },
               },
               {
                   type: 'line',                 //类型：条（柱状图）
                   highlight: {
                       size: 7,
                       radius: 7,
                   },

                   highlight: true,
                   axis: 'right',              //轴：底部、末端
                   xField: 'week',              //X轴：自定义
                   yField: 'EFFICIENCYAND',   //Y轴：自定义
                   markerConfig: {
                       type: 'circle',
                       size: 4,
                       radius: 4,
                       'stroke-width': 1
                   },
                   title: '总效率(%)',
                   label: {//设置每个标签的属性
                       display: 'under', //标签的位置
                       color: '#FFCC00',
                       'text-anchor': 'middle',
                       field: ['EFFICIENCYAND'],
                       orientation: 'horizontal',
                       fill: '#000',
                       font: '15px Arial'
                   },
                   tips: {
                       trackMouse: true,
                       width: 130,
                       height: 50,
                       renderer: function (storeItem, item) {
                           this.setTitle('时段:' + (storeItem.get('week') + '-' + storeItem.get('weeks')) + '<br/>总效率:' + storeItem.get('EFFICIENCYAND') + "%");
                       }
                   },
               },
                ]
            });

        }
       
        debugger
       
     

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
            //tbar: Ax.utils.LibToolBarBuilder.createToolBar([select, refresh, vcl.createChangeView(me, 'create', 'createChart', '图表'), savepic])
            tbar: Ax.utils.LibToolBarBuilder.createToolBar([select, refresh,savepic])
        });
        return mainPanel;
    }
};