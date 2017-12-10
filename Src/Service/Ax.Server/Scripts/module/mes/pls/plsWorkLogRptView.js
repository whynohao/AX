plsWorkLogRptView = function () {
    Ax.tpl.LibRptTpl.apply(this, arguments);
    this.vcl.funcView.add('proto.createChart', { name: 'createChart', display: '图表' });

};

var proto = plsWorkLogRptView.prototype = Object.create(Ax.tpl.LibRptTpl.prototype);
proto.constructor = plsWorkLogRptView;

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
        //点位实时 报表 如果显示图形的数量大于20的时候,先取20个监控值显示
        if (rowNum > 0) {
            var checkDate = dt.data.items[0].data["STARTDATE"].toString();
            var lineName = dt.data.items[0].data["PRODUCELINENAME"].toString();
            checkDate = checkDate.substr(0, 4) + '年' + checkDate.substr(4, 2) + '月' + checkDate.substr(6, 2) + '日';
            thisTitle = checkDate + ' ' + lineName + ' ' + '作业准时率报表';
        }
        
        for (var i = 0 ; i < rowNum ; i++) {
           
            list.push({
                'WorkIndex': dt.data.items[i].data["WORKINDEX"],
                'PlanBeat': dt.data.items[i].data["PLANBEAT"],
                'ActBeat': dt.data.items[i].data["ACTBEAT"],
            });
        }

        this.myDataStore = Ext.create('Ext.data.JsonStore', {
            fields: ['WorkIndex', 'PlanBeat', 'ActBeat'],
            data: list
        });

        var colors = ['#31B09F', '#FF901A'];
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
            shadow: true,
            style: 'background: #fff;',
            legend: { position: 'right',  labelFont: '10px Helvetica' },
            store: this.myDataStore,
            insetPadding: 35,

            items: [
            {
                type: 'text', text: thisTitle, font: '22px Helvetica',
                width: 100, height: 40,
                x: 450, y: 12
            }],

            axes: [{
                title: '节拍/分钟',
                type: 'Numeric',
                position: 'left',
                grid: true,
                fields: ['PlanBeat', 'ActBeat'],
                minimum: 0,
                label: {
                    font: '15px Helvetica',
                }


            },
                {
                    title: '十单顺序号',
                    type: 'Category',
                    position: 'bottom',
                    grid: true,
                    fields: 'WorkIndex',
                    label: {
                        font: '15px Helvetica',
                    }
                }],

            series: [{
                type: 'column',
                axis: 'left',
                xField: 'WorkIndex',
                yField: ['PlanBeat', 'ActBeat'],
                title: ['计划节拍/分钟', '实际节拍/分钟'],
                stacked: false,
                //style: { 'stroke-width': 4 },
                label: {
                    display: 'insideEnd',
                    font: '18px Helvetica',
                    field: ['PlanBeat', 'ActBeat'],
                    renderer: Ext.util.Format.numberRenderer('0'),
                    orientation: 'horizontal',
                    color: '#333',
                    'text-anchor': 'middle'
                },
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

