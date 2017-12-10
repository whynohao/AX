Ext.require('Ax.sys.sysKPIChart');

EmEquipmentYearKPIView = function () {
    Ax.tpl.LibRptTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = EmEquipmentYearKPIView.prototype = Object.create(Ax.tpl.LibRptTpl.prototype);
proto.constructor = EmEquipmentYearKPIView;
proto.createChart = function () {
    return EMsysKPIChart2.build(this, true, "OEERATE");
};
EMsysKPIChart = {
    build: function (view, isRate, rateField) {
        if (view.vcl.dataSet.dataList) {
            view.vcl.dataSet.dataList[0].ownGrid = null;
        }
        var me = view;
        var vcl = view.vcl;
        var timelists =vcl.invorkBcf("GetMonth", []);
        if (isRate === undefined)
            isRate = true;
        var dt = vcl.dataSet.getTable(0);
        var list = [];
        var allFields = [{ name: 'time' }];
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
        var rowNum = 12;
        for (var i = 0; i < rowNum; i++) {
            list.push(Ext.create(modelName, {
                time:timelists[i]
            }));
        }
        var series = [];
        var idx = 0;
        var maxValue = 1;
        dt.each(function (rec) {
            if (idx < 10) {
                var equipmentname = rec.get("EQUIPMENTNAME");
                var name = 'data' + idx;
                idx++;
                for (var r = 1; r <= rowNum; r++) {
                    var value = rec.get(rateField + r);
                    list[r].set(name, value);
                }
                series.push({
                    type: 'line',
                    highlight: {
                        size: 7,
                        radius: 7
                    },
                    axis: 'left',
                    xField: 'time',
                    smooth: true,
                    stacked: true,
                    yField: name,
                    markerConfig: {
                        type: 'circle',
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
                    title: Ext.isEmpty(equipmentname) ? "设备" : equipmentname
                });
            }
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
                maximum: maxValue,
                position: 'left',
                fields: fields,
                title: "综合使用效率（%）",
                minorTickSteps: 1,
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
                type: 'Category',
                position: 'bottom',
                fields: ['time'],
                title: '月份（月）',
                minimum: 1,
                maximum: rowNum,
                majorTickSteps:rowNum-2,
                label: {
                    renderer: function (v) {
                        return v;
                    },
                    rotate: {
                        degrees: 319//倾斜度
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
                vcl.showRpt(vcl.queryCondition);
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
            tbar: Ax.utils.LibToolBarBuilder.createToolBar([select, refresh, vcl.createChangeView(me, 'create', 'createChart', 'KPI图表')])
        });
        return mainPanel;
    }
};

EMsysKPIChart2 = {
    build: function (view, isRate, rateField) {
        if (view.vcl.dataSet.dataList) {
            view.vcl.dataSet.dataList[0].ownGrid = null;
        }
        var me = view;
        var vcl = view.vcl;
        var timelists = vcl.invorkBcf("GetMonth", []);
        var rowNum = timelists.length;
        var dt = vcl.dataSet.getTable(0);
        var list = [];
        var fields = [];
        var count = dt.getCount();
        for (var i = 0; i < count; i++) {
            fields.push({ name: 'data' + i, type: 'number' });
        }
        fields.push({ name: 'time', type: 'string' });

        for (var i = 0; i < rowNum; i++) {
            var data = {};
            if (fields.length > 0) {
                for (var m = 0; m < fields.length; m++) {
                    data[fields[m].name] = 0;
                }
            }
            data['time'] = timelists[i];
            list.push(data);
        }
        var series = [];
        var idx = 0;
        var maxValue = 1;
        dt.each(function (rec) {
            if (idx < 10) {
                var equipmentname = rec.get("EQUIPMENTNAME");
                equipmentname = Ext.isEmpty(equipmentname) ? "设备" : equipmentname;
                var name = 'data' + idx;
                idx++;
                for (var r = 0; r < rowNum; r++) {
                    var value = rec.get(rateField + (r+1));
                    list[r][name] = value;
                }
                series.push({
                    type: 'line',
                    highlight: {
                        size: 7,
                        radius: 7
                    },
                    axis: 'left',
                    xField: 'time',
                    smooth: true,
                    stacked: true,
                    yField: name,
                    markerConfig: {
                        type: 'circle',
                        size: 4,
                        radius: 4,
                        'stroke-width': 0
                    },
                    tips: {
                        trackMouse: true,
                        width: 200,
                        height: 60,
                        renderer: function (storeItem, item) {
                            var v = storeItem.get(name);
                            var time = storeItem.get('time');
                            var html = "<div>设备：" + equipmentname + "<br/>月份：" + time + "<br/>OEE：" + parseFloat(v * 100).toFixed(2) + "%</div>";
                            this.setTitle(html);
                        }
                    },
                    title: equipmentname
                });
            }
        });
        var dtChart = Ext.create('Ext.data.JsonStore', {
            fields: fields,
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
                maximum: maxValue,
                position: 'left',
                fields: fields,
                title: "综合使用效率（%）",
                minorTickSteps: 1,
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
                type: 'Category',
                position: 'bottom',
                fields: ['time'],
                title: '月份（月）',
                minimum: 1,
                maximum: rowNum,
                majorTickSteps: rowNum - 2,
                label: {
                    renderer: function (v) {
                        return v;
                    },
                    rotate: {
                        degrees: 319//倾斜度
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
                vcl.showRpt(vcl.queryCondition);
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
            tbar: Ax.utils.LibToolBarBuilder.createToolBar([select, refresh, vcl.createChangeView(me, 'create', 'createChart', 'KPI图表')])
        });
        return mainPanel;
    }
};