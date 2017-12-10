plsLogPostRptView = function () {
    Ax.tpl.LibRptTpl.apply(this, arguments);
    this.vcl.funcView.add('proto.createChart', { name: 'createChart', display: '图表' });
    this.vcl.funcView.add('proto.createDayChart', { name: 'createDayChart', display: '当日图表' });
};

var proto = plsLogPostRptView.prototype = Object.create(Ax.tpl.LibRptTpl.prototype);
proto.constructor = plsLogPostRptView;
proto.createChart = function () {
    return this.LogPostChart.build(this);
};

proto.createDayChart = function () {
    return this.LogPostChart.buildDay(this);
};

proto.LogPostChart = {
    build: function (view) {
        if (view.vcl.dataSet.dataList) {
            view.vcl.dataSet.dataList[0].ownGrid = null;
        }
        var me = view;
        var vcl = view.vcl;
        var dt = vcl.dataSet.getTable(0);
        var retList = [];
        for (var i = 0; i < dt.data.items.length; i++) {
            var date = dt.data.items[i].data["PLANDATE"].toString();
            retList.push({
                PlanDate: date,
                OrderProgress: dt.data.items[i].data["ORDERPROGRESS"],
                Rate: (dt.data.items[i].data["TIMERATE"] * 100).toFixed(2),
            });
        }
        var returnDate = vcl.invorkBcf('GetAllData', [retList]);
        var list = returnDate.list;
        var result = [];
        for (var i = 0; i < list.length; i++) {
            var count = 0;
            if (list[i].生产 == 0) {
                delete list[i].生产;
                count++;
            }
            if (list[i].入库 == 0) {
                delete list[i].入库;
                count++;
            }
            if (list[i].发货 == 0) {
                delete list[i].发货;
                count++;
            }
            if (count != 3) {
                result.push(list[i]);
            }
        }
        var xField = [];
        var yField = [];
        var allField = [];
        for (var i = 0; i < returnDate.xField.length; i++) {
            xField.push(returnDate.xField[i]);
            allField.push(returnDate.xField[i]);
        }
        for (var i = 0; i < returnDate.yField.length; i++) {
            yField.push(returnDate.yField[i]);
            allField.push(returnDate.yField[i]);
        }
        this.myDataStore = Ext.create('Ext.data.JsonStore', {
            fields: allField,
            data: result
        });

        //自定义一个样式 (颜色)
        var colors = ['#000000'];
        Ext.define('Ext.chart.theme.MyFancy', {
            extend: 'Ext.chart.theme.Base',
            constructor: function (config) {
                this.callParent([Ext.apply({
                    colors: colors
                }, config)]);
            }
        });
        var chart = Ext.create('Ext.chart.Chart', {
            style: 'background:#fff',
            animate: true,
            store: this.myDataStore,
            shadow: true,
            //theme: 'MyFancy',
            legend: {
                position: 'right'
            },
            axes: [{
                type: 'Numeric',
                minimum: 0,
                maximum: 100,
                position: 'left',
                fields: yField,
                title: '准时率 %',
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
                fields: xField,
                title: '日期'
            }],
            series: [{
                type: 'column',
                highlight: {
                    size: 5,
                    radius: 5
                },
                axis: ' ',
                //style: { width: 200 },
                smooth: true,
                fill: true,
                xField: xField,
                yField: yField,
                //markerConfig: {
                //    type: 'column',
                //    size: 4,
                //    radius: 4,
                //    'stroke-width': 0
                //},
                tips: {
                    trackMouse: true,
                    width: 200,
                    height: 25,
                    renderer: function (storeItem, item) {

                        this.setTitle(storeItem.data["Date"] + "," + item.yField + ':' + item.value[1] + '%');
                    }
                },
                label: {
                    display: 'insideEnd',
                    field: yField,
                    renderer: Ext.util.Format.numberRenderer('0'),
                    orientation: 'horizontal',
                    'font-size': 15,
                    color: '#FFFFFF',
                    'text-anchor': 'right'
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

        var mainPanel = Ext.create('Ext.panel.Panel', {
            minHeight: 400,
            minWidth: 550,
            hidden: false,
            maximizable: true,
            layout: { type: 'fit' },
            items: chart,
            border: false,
            tbar: Ax.utils.LibToolBarBuilder.createToolBar([select, refresh, vcl.createChangeView(me, 'create', 'createChart', '图表'), savepic])
        });
        return mainPanel;

    }
    ,

    buildDay: function (view) {

        var me = view;
        var vcl = view.vcl;
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
        var returnDate = vcl.invorkBcf('GetDayData', []);
        var color = ['primary', 'yellow', 'blue', 'green', 'gray'];
        var mainDiv = '<div style="width:100%;height:100%;margin:30px;padding:30px;border:0;overflow:hidden;">';
        var contentDiv = '<div class="row">';
        for (var i = 0; i < returnDate.length; i++) {
            contentDiv = contentDiv + '    <div class="col-lg-3 col-md-6">' +
                                        '        <div class="panel panel-' + color[i % color.length] + '">' +
                                        '            <div class="panel-heading">' +
                                        '                <div class="row">' +
                                        '                    <div class="col-xs-9 text-left" style="margin-left:20px;">' +
                                        '                        <div style="font-size:25px;">' + returnDate[i].OrderPrgress + '</div>' +
                                        '                    </div>' +
                                        '                </div>' +
                                        '            </div>' +
                                        '                <div class="panel-footer"  style="font-size:20px;">' +
                                        '                   <div class="row" style="margin-left:8px;margin-right:8px;margin-top:6px;">' +
                                        '                    <span class="col-lg-8">计划批次：</span>' +
                                        '                    <span class="col-lg-4"><i class="fa fa-arrow-circle-right">' + returnDate[i].PlanQuan + '</i></span>' +
                                        '                   </div>' +
                                        '                   <div class="row"  style="margin-left:8px;margin-right:8px;margin-top:6px;">' +
                                        '                    <span class="col-lg-8">准时批次：</span>' +
                                        '                    <span class="col-lg-4"><i class="fa fa-arrow-circle-right">' + returnDate[i].ActQuan + '</i></span>' +
                                        '                   </div>' +
                                        '                      <div class="row"  style="margin-left:8px;margin-right:8px;margin-top:6px;">' +
                                        '                    <span class="col-lg-8">准时率：</span>' +
                                        '                    <span class="col-lg-4"><i class="fa fa-arrow-circle-right">' + (returnDate[i].Rate * 100).toFixed(2) + '%</i></span>' +
                                        '                   </div>' +
                                        '                    <div class="clearfix"></div>' +
                                        '                </div>' +
                                        '        </div>' +
                                        '    </div>';
        }

        var mainDiv = mainDiv + contentDiv + "</div>";

        var mainPanel = Ext.create('Ext.panel.Panel', {
            minHeight: 400,
            minWidth: 550,
            hidden: false,
            maximizable: true,
            layout: { type: 'fit' },
            //items: chart,
            html: mainDiv,
            border: false,
            tbar: Ax.utils.LibToolBarBuilder.createToolBar([select, refresh, vcl.createChangeView(me, 'create', 'createChart', '图表'), savepic])
        });
        return mainPanel;

    }
};



