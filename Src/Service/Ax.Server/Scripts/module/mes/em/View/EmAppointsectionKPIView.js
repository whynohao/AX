Ext.require('Ax.sys.sysKPIChart'); Ext.require('Ax.sys.sysKPIChart');

EmAppointsectionKPIView = function () {
    Ax.tpl.LibRptTpl.apply(this, arguments);
    this.vcl.funcView.add('createChart', { name: 'createChart', display: 'KPI图表' });
};
var proto = EmAppointsectionKPIView.prototype = Object.create(Ax.tpl.LibRptTpl.prototype);
proto.constructor = EmAppointsectionKPIView;

proto.createChart = function () {
    return EMsysKPIChart2.build(this, true);
};
EMsysKPIChart2 = {
    build: function (view, isRate) {
        if (view.vcl.dataSet.dataList) {
            view.vcl.dataSet.dataList[0].ownGrid = null;
        }
        var me = view;
        var vcl = view.vcl;
        var list = [];
        var fields = [];
        if (isRate === undefined)
            isRate = true;
        var dt = vcl.dataSet.getTable(0);

        /**
        * 去除数组重复元素
       **/
        function uniqueArray(data) {

            data = data || [];

            var a = {};

            for (var i = 0; i < data.length; i++) {

                var v = data[i];

                if (typeof (a[v]) == 'undefined') {

                    a[v] = 1;

                }
            };

            data.length = 0;

            for (var i in a) {

                data[data.length] = i;

            }

            return data;

        }

        /*
        **获取设备名称
        */
        function getEquipment(equipmentid) {
            var result="";
            dt.each(function (rec) {
                if (equipmentid == rec.get("EQUIPMENTID")) {
                    result = equipmentid +" "+ rec.get("EQUIPMENTNAME");
                    return result;
                }
            });
            return result;
        }

        //设备
        var equipments = [];
        var eqstemp = [];
        var sqsidx = 0;
        for (var i = 0; i < dt.getCount() ; i++) {
            eqstemp[sqsidx] = dt.data.items[i].data["EQUIPMENTID"];
            sqsidx++;
        }
        equipments = uniqueArray(eqstemp);

        for (var i = 0; i < equipments.length; i++) {
            fields.push({ name: 'data' + i, type: 'number' });
        }
        fields.push({ name: 'time', type: 'string' });

        //日期
        var timelists = [];
        var timeliststemp = [];
        var timelistidx = 0;
        for (var i = 0; i < dt.getCount() ; i++) {
            var date=dt.data.items[i].data["RPTDATE"]+"";
            timeliststemp[timelistidx] = date.substring(4, 6) + "-" + date.substring(6, 8);
                timelistidx++;
        }
        timelists = uniqueArray(timeliststemp);
        for (var i = 0; i < timelists.length; i++) {
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
        for (var k = 0; k < equipments.length; k++) {
            if (idx < 10) {
                var equipmentid = equipments[k];
                equipmentid = Ext.isEmpty(equipmentid) ? "设备" : equipmentid;
                var name = 'data' + idx;
                idx++;
                var oeerates = [];
                var oeeratecount = 0;
                for (var j = 0; j < dt.getCount() ; j++) {
                    if (dt.data.items[j].data["EQUIPMENTID"] == equipmentid) {
                        oeerates[oeeratecount] = dt.data.items[j].data["OEERATE"];
                        oeeratecount++;
                    }
                }
                for (var r = 0; r < timelists.length; r++) {
                    var value = oeerates[r];
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
                            var v = storeItem.get(item.series.yField);
                            var time = storeItem.get('time');
                            var html = "<div>设备：" + item.series.title + "<br/>日期：" + time + "<br/>OEE：" + parseFloat(v * 100).toFixed(2) + "%</div>";
                            this.setTitle(html);
                        }
                    },
                    title: getEquipment(equipmentid)
                });
            }
        }
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
                title: '日期（天）',
                minimum: 1,
                maximum: timelists.length,
                majorTickSteps: timelists.length - 2,
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
}