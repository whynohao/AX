/// <reference path="plsMaterialStockTaskVcl.js" />
//柱状图

plsOrderHandleRptView = function () {
    Ax.tpl.LibRptTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = plsOrderHandleRptView.prototype = Object.create(Ax.tpl.LibRptTpl.prototype);
proto.constructor = plsOrderHandleRptView;
proto.createChart = function () {
    return this.EMsysKPIChart.build(this);
};

proto.EMsysKPIChart = {
    build: function (view) {
        var max=0;
        var me = view;
        if (view.vcl.dataSet.dataList) {
            view.vcl.dataSet.dataList[0].ownGrid = null;
        }
        var vcl = view.vcl;
        var dt = view.vcl.dataSet.getTable(0);
        var items = dt.data.items;
        var date = items[0].data["DATE"].toString();
        var year = date.substring(0, 4);
        var month = date.substring(4, 6);
        var day = date.substring(6, 8);
        var rowNum = dt.getCount();

        var checkDate = "";
        if (rowNum > 0) {
            checkDate = "订单备注报表";
        };
        for (var i = 0; i <= rowNum - 1; i++) {
            if (items[i].data["ACCORDERQUANTITY"] > max)
                max = items[i].data["ACCORDERQUANTITY"];
            if (items[i].data["UNCONFIRMQUANTITY"] > max)
                max = items[i].data["UNCONFIRMQUANTITY"];
            if (items[i].data["BACKORDERQUANTITY"] > max)
                max = items[i].data["BACKORDERQUANTITY"];
            if (items[i].data["CONFIRMQUANTITY"] > max)
                max = items[i].data["CONFIRMQUANTITY"];
            if (items[i].data["REMARKQUANTITY"] > max)
                max = items[i].data["REMARKQUANTITY"];
        }
        var list = [];
        for (var i = 0; i <= rowNum - 1; i++) {
            list.push({
                DATE: items[i].data["DATE"],
                ACCORDERQUANTITY: items[i].data["ACCORDERQUANTITY"],
                UNCONFIRMQUANTITY: items[i].data["UNCONFIRMQUANTITY"],
                BACKORDERQUANTITY: items[i].data["BACKORDERQUANTITY"],
                CONFIRMQUANTITY: items[i].data["CONFIRMQUANTITY"],
                REMARKQUANTITY: items[i].data["REMARKQUANTITY"],
                X: year+'年'+month+'月'+day+'日',

            });
            if (list.length > 100) break;
        }

        //Ext数据源
        this.myDataStore = Ext.create('Ext.data.JsonStore', {
            fields: ['DATE', 'ACCORDERQUANTITY', 'BACKORDERQUANTITY','UNCONFIRMQUANTITY', 'CONFIRMQUANTITY', 'REMARKQUANTITY', 'X'],
            data: list
        });

        var chart = Ext.create('Ext.chart.Chart', {
            width: '100%',
            height: 410,
            padding: '10 0 0 0',
            //theme: 'Category1',
            animate: true,
            shadow: false,
            style: 'background: #fff;',
            legend: {
                position: 'bottom',
                boxStrokeWidth: 0,
                labelFont: '12px Helvetica'
            },
            store: this.myDataStore,
            insetPadding: 40,
            items: [{
                type: 'text',
                text: '订单报表',
                font: '30px Helvetica',
                width: 100,
                height: 30,
                x: 500, //the sprite x position
                y: 15  //the sprite y position
            }, {
                type: 'text',
                text: checkDate,
                font: '15px Helvetica',
                x: 12,
                y: 510
            }],
            axes: [{
                //type: 'Numeric',
                type: 'Numeric',
                position: 'left',
                fields: 'ACCORDERQUANTITY',
                grid: true,
                minimum: 0,
                maximum:max%2+max,
            }, {
                type: 'Category',
                position: 'bottom',
                fields: ['X'],
                grid: true,
                label: {
                    rotate: {
                        degrees: 0
                    }
                }
            }],
            series: [{
                type: 'column',
                axis: 'left',
                title: ['接单个数', '退单个数', '未确认个数', '已确认个数', '带备注个数'],
                xField: ['DATE'],
                yField: ['ACCORDERQUANTITY', 'BACKORDERQUANTITY', 'UNCONFIRMQUANTITY', 'CONFIRMQUANTITY', 'REMARKQUANTITY'],
                style: {
                    opacity: 0.80
                },
                highlight: {
                    fill: '#000',
                    'stroke-width': 1,
                    stroke: '#000'
                },
                tips: {
                    trackMouse: true,
                    style: 'background: #FFF',
                    height: 20,
                    renderer: function (storeItem, item) {
                        var browser = item.series.title[Ext.Array.indexOf(item.series.yField, item.yField)];
                        this.setTitle(browser + ' for ' + storeItem.get('DATE') + ': ' + storeItem.get(item.yField));
                    }
                },
                label: {
                    //calloutLine: true,
                    contrast: true,
                    display: 'insideEnd',
                    'text-anchor': 'middle',
                    field: ['ACCORDERQUANTITY', 'BACKORDERQUANTITY', 'UNCONFIRMQUANTITY', 'CONFIRMQUANTITY', 'REMARKQUANTITY'],
                    renderer: Ext.util.Format.numberRenderer('0'),
                    //orientation: 'vertical',//横向
                    font: '18px Arial',
                    color: '#333'
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
                vcl.browseTo(vcl.queryCondition);
                vcl.win.removeAll();
                vcl.win.add(me.createChart());
            }
        });
        var minWidth = list.length > 10 ? list.length * 40 : 400;
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

