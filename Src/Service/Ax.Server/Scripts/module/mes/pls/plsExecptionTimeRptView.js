//plsDayFinishQuantityRptView
plsExecptionTimeRptView = function () {
    Ax.tpl.LibRptTpl.apply(this, arguments);
    //this.vcl.funcView.add('proto.createChart', { name: 'createChart', display: '图表' });
    if (this.vcl.funcView.containsKey("default")) {
        this.vcl.funcView.get("default").name = "createChart";
    }
};

var proto = plsExecptionTimeRptView.prototype = Object.create(Ax.tpl.LibRptTpl.prototype);
proto.constructor = plsExecptionTimeRptView;

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
            var rowNum = dt.getCount();
            var time = 0;
            var index = 0;
            var lotno = '0';
            var groupNo = '0';
            var orderDtae = 0;
            var quaryDate = 0;
            var color = '0';
            debugger
                
            var thisTitle = '';
            //点位实时 报表 如果显示图形的数量大于20的时候,先取20个监控值显示
            if (rowNum > 0) {
                // var checkDate = dt.data.items[0].data["YEAR"].toString() + ;
                //var lineName = dt.data.items[0].data["EQUIPMENTID"].toString();
                // checkDate = dt.data.items[0].data["YEAR"].toString() + '年' + dt.data.items[0].data["MONTH"].toString() + '月'
                thisTitle = '4分厂异常处理时间报表';

            }
            debugger
                if (rowNum > 0) {
                    for (var min = 0; min < rowNum; min++) {
                        if (dt.data.items[min].data["EXECPTIONTIME"] == '未结束' || parseInt(dt.data.items[min].data["EXECPTIONTIME"])>300)
                        { 
                            time = 300;                          
                        }
                        else
                        {
                            time = parseInt(dt.data.items[min].data["EXECPTIONTIME"]);
                            color ='#00ff00'
                        }
                        list.push({
                            'INDEX': parseInt(dt.data.items[min].data["INDEX"]),
                            'TASKNO': dt.data.items[min].data["TASKNO"],
                            'LOTNO': dt.data.items[min].data["LOTNO"],
                            'GROUPNO': dt.data.items[min].data["GROUPNO"],
                            'EXECPTIONTIME': time,
                            'COLOR': color,
                            'week': dt.data.items[min].data["EXECPTIONTIME"],
                            'ORDERDATE': parseInt(dt.data.items[min].data["ORDERDATE"]),
                            'QUARYDATE': parseInt(dt.data.items[min].data["QUARYDATE"]),
                        });
                    }                
                }
           
            debugger
            this.myDataStore = Ext.create('Ext.data.JsonStore', {
                fields: ['INDEX', 'TASKNO', "LOTNO", "GROUPNO", 'EXECPTIONTIME', 'ORDERDATE', 'QUARYDATE','COLOR'],
                data: list
                
            });

            debugger
        //    var chart = Ext.create('Ext.chart.Chart', {
        //        store: this.myDataStore,
        //        style: 'background:#fff',
        //        shadow: false,
        //        minmum: 0,
        //        axes: [{
        //            type: 'Numeric',
        //            position: 'left',
        //            fields: 'EXECPTIONTIME',
        //            displayfields: '异常时间',
        //            title: '异常时间(min)',
        //            maximum: 300,
        //            minimum: 0,
        //            minorTickSteps: 1,
        //        },
        //            {
        //                type: 'Category',
        //                position: 'buttom',
        //                fields: 'TASKNO',
        //                title: '任务号',
        //            }],

        //        series: [{
        //            type: 'column',                 //类型：条（柱状图）
        //            axis: 'left',
        //            xField: 'TASKNO',              //X轴：自定义
        //            yField: 'EXECPTIONTIME',   //Y轴：自定义
        //            //markerConfig: {
        //            //    type: 'cross',
        //            //    size: 4,
        //            //    radius: 4,
        //            //    'stroke-width': 0
        //            //},
        //            title: '任务号',
        //            tips: {
        //                trackMouse: true,
        //                width: 130,
        //                height: 80,
        //                renderer: function (storeItem, item) {
        //                    this.setTitle('十单日期:' + storeItem.get('ORDERDATE') + '<br/>十单号:' + storeItem.get('INDEX') + '<br/>生产单号:' + storeItem.get('LOTNO') + '<br/>组号:' + storeItem.get('GROUPNO') + '<br/>异常发生日期:' + storeItem.get('QUARYDATE') + '<br/>处理时间:' + storeItem.get('EXECPTIONTIME'));
        //                }
        //            },
        //        },
        //        ]
        //    });

        //}
            var chart = Ext.create('Ext.chart.Chart', {
                store: this.myDataStore,
                style: 'background:#fff',
                theme: 'Category2',
                animate: true,
                shadow: true,
                insetPadding: 40,
               
                axes: [{
                    type: 'Numeric',
                    position: 'bottom',
                    fields: ['EXECPTIONTIME'],
                    displayfields: '异常时间',
                    minimum: 0,
                    title: '异常时间(min)',
                    maximum: 300,
                    minimum: 0,
                    minorTickSteps: 1,                  
                },
                    {
                        type: 'Category',
                        position: 'left',
                        fields: 'TASKNO',
                        title: '任务号',
                    }],

                series: [{
                    type: 'bar',                 //类型：条（柱状图）
                    //highlight: {
                    //    size: 7,
                    //    radius: 7,
                    //},
                    
                    highlight: true,
                    axis: 'bottom',              //轴：底部、末端
                    xField: 'TASKNO',              //X轴：自定义
                    yField: 'EXECPTIONTIME',   //Y轴：自定义
                    //markerConfig: {
                    //    type: 'cross',
                    //    size: 4,
                    //    radius: 4,
                    //    'stroke-width': 0
                    //},
                    title: '组号',
                    label: {//设置每个标签的属性
                        display: 'insideEnd', //标签的位置
                        color: '#333',
                        'text-anchor': 'middle',
                        field: ['week'],
                        orientation: 'horizontal',
                        fill: '#000',
                        font: '17px Arial'
                    },
                    renderer: function (sprite, record, attr, index, store) {
                        if (record.get('week') == '未结束') {
                            var color = 'RED';
                        }
                        else {
                            var color = 'GREEN';
                        }
                     return Ext.apply(attr, {
                        fill: color
                    });
                },
                   
                    tips: {
                        trackMouse: true,
                                        width: 200,
                                        height: 130,
                                        renderer: function (storeItem, item) {
                                            this.setTitle('十单日期:' + storeItem.get('ORDERDATE') + '<br/>十单号:' + storeItem.get('INDEX') + '<br/>生产单号:' + storeItem.get('LOTNO') + '<br/>组号:' + storeItem.get('GROUPNO') + '<br/>异常发生日期:' + storeItem.get('QUARYDATE') + '<br/>处理时间:' + storeItem.get('week'));
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
            tbar: Ax.utils.LibToolBarBuilder.createToolBar([select, refresh, savepic])
        });
        return mainPanel;
    }
};