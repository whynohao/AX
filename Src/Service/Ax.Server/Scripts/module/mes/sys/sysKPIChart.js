sysKPIChart = {
    build: function (view, rateField, rateDisplay, showTitleFn, timeStr, isRate, hasAvg) {
        var me = view;
        var vcl = view.vcl;
        if (timeStr === undefined)
            timeStr = 'month';
        if (isRate === undefined)
            isRate = true;
        if (hasAvg === undefined)
            hasAvg = true;
        var dt = vcl.dataSet.getTable(0);
        var list = [];
        var allFields = [{ name: 'time', type: 'number' }];
        var fields = [];
        var count = dt.getCount();
        for (var i = 0; i < count; i++) {
            fields.push({ name: 'data' + i, type: 'number' });
        }
        var modelName = vcl.progId + 'Chart';
        var modelType = Ext.data.Model.schema.getEntity(modelName);
        if (modelType === null) {
            modelType = Ext.define(modelName, {
                extend: 'Ext.data.Model',
                fields: allFields.concat(fields),
                proxy: {
                    type: 'memory',
                    reader: {
                        type: 'json'
                    }
                }
            });
        }
        var rowNum = timeStr == "month" ? 13 : 32;
        if (!hasAvg)
            rowNum--;
        for (var i = 1; i <= rowNum; i++) {
            list.push(Ext.create(modelName, {
                time: i
            }));
        }
        var series = [];
        var idx = 0;
        var maxValue = 1;
        dt.each(function (rec) {
            var name = 'data' + idx;
            idx++;
            for (var r = 0; r < rowNum; r++) {
                var value = rec.get(rateField + (r + 1));
                list[r].set(name, value);
                if (maxValue < value)
                    maxValue = value;
            }
            series.push({
                type: 'line',
                highlight: {
                    size: 7,
                    radius: 7
                },
                axis: 'left',
                xField: 'time',
                yField: name,
                markerConfig: {
                    type: 'cross',
                    size: 4,
                    radius: 4,
                    'stroke-width': 0
                },
                tips: {
                    trackMouse: true,
                    width: 40,
                    height: 20,
                    renderer: function (storeItem, item) {
                        var v = storeItem.get(name);
                        if (isRate)
                            this.setTitle(parseFloat(v * 100).toFixed(0) + "%");
                        else
                            this.setTitle(v);
                    }
                },
                title: showTitleFn(rec)
            });
        });
        var dtChart = Ext.create('Ext.data.Store', {
            model: modelType,
            proxy: {
                type: 'memory',
                reader: {
                    type: 'json'
                }
            },
            data: list
        });

        var chart = Ext.create('Ext.chart.Chart', {
            style: 'background:#fff',
            animate: true,
            store: dtChart,
            shadow: true,
            theme: 'Category1',
            legend: {
                position: 'right'
            },
            axes: [{
                type: 'Numeric',
                //minimum: 0,
                maximum: maxValue,
                position: 'left',
                fields: fields,
                title: rateDisplay,
                //minorTickSteps: 1,
                grid: {
                    odd: {
                        opacity: 1,
                        fill: '#ddd',
                        stroke: '#bbb',
                        'stroke-width': 0.5
                    }
                },
                label: {
                    renderer: function (v) {
                        if (isRate)
                            return parseFloat(v * 100).toFixed(0) + "%";
                        else
                            return v;
                    }
                }
            }, {
                type: 'Numeric',
                minimum: 1,
                maximum: rowNum,
                majorTickSteps: rowNum - 2,
                position: 'bottom',
                fields: ['time'],
                title: timeStr == "month" ? '月份' : '日期（天）',
                label: {
                    renderer: function (v) {
                        if (hasAvg && v == rowNum)
                            return timeStr == "month" ? '月平均' : '平均';
                        else
                            return timeStr == "month" ? (v + '月') : v;
                    }
                }
            }],
            series: series
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
        var mainWidth = document.body.clientWidth > 1210 ? document.body.clientWidth - 10 : 1210;
        var mainPanel = Ext.create('Ext.panel.Panel', {
            width: mainWidth,
            height: document.body.clientHeight - 80,
            layout: { type: 'fit' },
            items: chart,
            border: false,
            tbar: Ax.utils.LibToolBarBuilder.createToolBar([select, refresh, vcl.createChangeView(me, 'createChart', 'createChart', 'KPI图表')])
        });
        return mainPanel;
    }
};