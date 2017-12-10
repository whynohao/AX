comMatDayStockAdjustView = function () {
    Ax.tpl.LibDataFuncTpl.apply(this, arguments);
    if (this.vcl.funcView.containsKey("default")) {
        this.vcl.funcView.get("default").name = "onReady";
    }
};
var proto = comMatDayStockAdjustView.prototype = Object.create(Ax.tpl.LibDataFuncTpl.prototype);
proto.constructor = comMatDayStockAdjustView;
proto.onReady = function (billAction, curPks, isF4, lookVersionObj, changeView) {
    var me = this;
    var vcl = this.vcl;
    vcl.forms = [];
    vcl.openFunc();
    var store = vcl.dataSet.getTable(0);

    var panel = Ext.create('Ext.form.Panel', {
        border: false,
        tableIndex: 0,
        margin: '6 2 6 2',
        items: Ext.decode(vcl.tpl.Layout.HeaderRange.Renderer)
    });
    vcl.forms.push(panel);
    panel.loadRecord(store.data.items[0]);

    var tabPanel = Ext.widget('tabpanel', {
        activeTab: 0,
        flex: vcl.tpl.Layout.GridRange == null ? 1 : undefined,
        defaults: {
            bodyPadding: 0
        }
    });
    function addTab(panel, displayName) {
        tabPanel.add({
            iconCls: 'tabs',
            layout: 'fit',
            items: panel,
            title: displayName
        });
    }

    var tabRange = vcl.tpl.Layout.TabRange;
    if (tabRange.length > 0) {
        var tableIndex = 1;
        for (var i = 0; i < tabRange.length; i++) {
            if (tabRange[i].BlockType == BlockTypeEnum.ControlGroup) {
                var tempPanel = Ext.create('Ext.form.Panel', {
                    border: false,
                    tableIndex: 0,
                    margin: '6 0 6 2',
                    defaultType: 'textfield',
                    items: Ext.decode(tabRange[i].Renderer)
                });
                tempPanel.loadRecord(store.data.items[0]);
                vcl.forms.push(tempPanel);
                addTab(tempPanel, tabRange[i].DisplayName);
            } else if (tabRange[i].BlockType == BlockTypeEnum.Grid) {
                var grid = Ax.tpl.GridManager.createGrid({
                    vcl: vcl,
                    parentRow: vcl.dataSet.getTable(0).data.items[0],
                    tableIndex: vcl.dataSet.tableMap.get(tabRange[i].Store),
                    curRange: tabRange[i]
                });
                addTab(grid, tabRange[i].DisplayName);
            }
        }
    };

    var gridPanel;
    if (vcl.tpl.Layout.GridRange != null) {
        gridPanel = Ax.tpl.GridManager.createGrid({
            vcl: vcl,
            parentRow: vcl.dataSet.getTable(0).data.items[0],
            tableIndex: vcl.dataSet.tableMap.get(vcl.tpl.Layout.GridRange.Store),
            curRange: vcl.tpl.Layout.GridRange,
            title: vcl.tpl.Layout.GridRange.DisplayName
        });
    };

    var funPanel;
    var inputAnchor = '100% 100%';
    if (vcl.tpl.Layout.ButtonRange != null) {
        inputAnchor = '100% 95%';
        funPanel = Ext.create('Ext.panel.Panel', {
            border: false,
            anchor: '100% 5%',
            margin: '2 4',
            layout: { type: 'hbox', align: 'stretch' },
            defaults: {
                margin: '0 10'
            },
            items: Ext.decode(vcl.tpl.Layout.ButtonRange.Renderer)
        });
    }

    var chart = this.createChart();
    var inputPanel = Ext.create('Ext.panel.Panel', {
        columnWidth: 0.8,
        layout: { type: 'vbox', align: 'stretch' },
        items: [chart, gridPanel] 
    });
    var materialGrid = this.createGrid();
    var panel = Ext.create('Ext.panel.Panel', {
        anchor: inputAnchor,
        layout: 'column',
        items: [materialGrid, inputPanel]
    })

    var toolBarAction = Ax.utils.LibToolBarBuilder.createDataFuncAction(vcl, 0);
    var mainWidth = document.body.clientWidth > 1210 ? document.body.clientWidth - 27 : 1210;
    var mainPanel = Ext.create('Ext.panel.Panel', {
        width: mainWidth,
        height: document.body.clientHeight - 80,
        layout: { type: 'anchor' },
        items: [panel, funPanel],
        border: false,
        tbar: Ax.utils.LibToolBarBuilder.createToolBar(toolBarAction)
    });
    return mainPanel;

};
proto.createChart = function () {
    var me = view;
    var vcl = view.vcl;
    //var dt = vcl.dataSet.getTable(1);
    //var list = [];
    var allFields = [{ name: 'time', type: 'number' }];
    var fields = [];
    fields.push({ name: 'needQty', type: 'number' });
    fields.push({ name: 'stockQuantity', type: 'number' });
    fields.push({ name: 'safeStockNum', type: 'number' });
    var modelName = vcl.progId + 'Chart';
    var modelType = Ext.data.Model.schema.getEntity(modelName);
    if (!modelType) {
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
    //var rowNum = dt.length;
    //for (var i = 0; i < rowNum; i++) {
    //    var date = dt.data.items[i].get("FDATE");
    //    var needQty = dt.data.items[i].get("NEEDQTY");
    //    var stockQuantity = dt.data.items[i].get("STOCKQUANTITY");
    //    var safeStockNum = dt.data.items[i].get("SAFESTOCKNUM");
    //    list.push(Ext.create(modelName, {
    //        time: date,
    //        needQty: needQty,
    //        stockQuantity: stockQuantity,
    //        safeStockNum: safeStockNum
    //    }));
    //}
    var list = [];
    var series = [];
    var idx = 0;
    var maxValue = 1;
    var minValue = 1000000;
    var name = 'needQty';
    var name1 = 'stockQuantity';
    var name2 = 'safeStockNum';
    series.push({
        type: 'line',
        highlight: {
            size: 10,//鼠标移到点上高亮效果，在这个折线图上效果不是很明显
            radius: 10
        },
        axis: 'left',//线的方向，是从左到右还是下到上，还有一种是bottom
        xField: 'time',//X轴对应的字段
        yField: name,//Y周对应的字段，一般一条线一个字段
        //showMarkers :false,
        markerConfig: {//标记的属性设置，'stroke-width'是标记的大小
            type: 'cross',//circle是圆形，如果是圆形的话有填充颜色
            size: 2,
            radius: 5,//半径
            'stroke-width': 2//粗细
        },
        smooth: true,
        //label: {
        //    display: 'rotate',//柱状图的话 insideStart | insideEnd | outside ; 饼图 outside | rotate 折现图，散图： "under" | "over" | "rotate"//在这里当隐藏线然后再点击显示线的时候会出现数字，其他时候不会rotate:半斜
        //    field: name,
        //    renderer: function (v) {
        //        return v;
        //    },
        //    'text-anchor': 'middle'
        //},
        tips: {//点击点显示出标签，里面有当前坐标的值的信息，
            trackMouse: true,//点击是否显示
            width: 50,//宽度
            height: 20,//高度
            renderer: function (storeItem, item) {
                this.setTitle(storeItem.get(name) == 0 ? '0' : storeItem.get(name));
            }
        },
        title: "需求量"
    },
      {
          type: 'line',
          highlight: {
              size: 10,
              radius: 10
          },
          axis: 'left',
          xField: 'time',
          yField: name1,
          showMarkers: true,
          smooth: true,
          markerConfig: {
              type: 'cross',
              size: 2,
              radius: 5,//半径
              'stroke-width': 2
          },
          //label: {
          //    display: 'outsideEnd',
          //    field: name1,
          //    renderer: Ext.util.Format.numberRenderer('0'),
          //    orientation: 'horizontal',
          //    color: '#333',
          //    'text-anchor': 'middle'
          //},
          tips: {
              trackMouse: false,
              width: 50,
              height: 20,
              renderer: function (storeItem, item) {
                  this.setTitle(storeItem.get(name1) == 0 ? '0' : storeItem.get(name1));
              }
          },
          title: "安全库存"
      },
          {
              type: 'line',
              highlight: {
                  size: 10,
                  radius: 10
              },
              axis: 'left',
              xField: 'time',
              yField: name2,
              showMarkers: true,
              smooth: true,
              markerConfig: {
                  type: 'cross',
                  size: 2,
                  radius: 5,//半径
                  'stroke-width': 2
              },
              //label: {
              //    display: 'outsideEnd',
              //    field: name1,
              //    renderer: Ext.util.Format.numberRenderer('0'),
              //    orientation: 'horizontal',
              //    color: '#333',
              //    'text-anchor': 'middle'
              //},
              tips: {
                  trackMouse: false,
                  width: 50,
                  height: 20,
                  renderer: function (storeItem, item) {
                      this.setTitle(storeItem.get(name2) == 0 ? '0' : storeItem.get(name2));
                  }
              },
              title: "调整后安全库存"
          });

    //dt.each(function (rec) {
    //    var value = rec.get(name);
    //    var value1 = rec.get(name1);
    //    var value2 = rec.get(name2);
    //    if (maxValue < value)
    //        maxValue = value;
    //    if (maxValue < value1)
    //        maxValue = value1;
    //    if (maxValue < value2)
    //        maxValue = value2;
    //    if (minValue > value)
    //        minValue = value;
    //    if (minValue > value1)
    //        minValue = value1;
    //    if (minValue > value2)
    //        minValue = value2;

    //});

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
        width: "100%",
        height: $(window).height() / 2,
        style: 'background:#FFF',//背景色
        animate: false,//动画  在这里效果不大
        store: dtChart,
        cls: 'x-panel-body-default',
        shadow: true,//是否有阴影 在这里效果明显，线变模糊变粗
        theme: 'Category1',//这么多线都有不同颜色主要是这个属性控制的
        legend: {//
            position: 'right'//可以左右底
        },
        axes: [{
            type: 'Numeric',
            minimum: 0,
            maximum: 200,
            position: 'left',
            fields: fields,
            //title: rateDisplay,
            //minorTickSteps: 1,
            grid: {
                //odd: {//基数 基数行
                //    opacity: 0.7,//透明度
                //    fill: '#FFFFFF',//色带填充的颜色
                //    //stroke: '#00FA9A',//色带边框的颜色
                //    //'stroke-width': 2//色带边框的宽度
                //},
                //even: {//偶数 偶数行
                //    opacity: 0.7,
                //    fill: '#DFFFDF',
                //    stroke: '#00FA9A',
                //    'stroke-width': 2
                //}
                odd: {
                    opacity: 1,
                    fill: '#ddd',
                    stroke: '#bbb',
                    'stroke-width': 0.5
                }
            },
            label: {
                renderer: function (v) {//坐标显示
                    return v;
                }
            }
        }, {
            type: 'Category',
            //minimum: 20160405,
            //maximum: 20160410,
            //majorTickSteps: 1,
            position: 'bottom',
            fields: ['time'],
            //title: timeStr == "month" ? '月份' : '日期（天）',
            //label: {
            //    renderer: function (v) {
            //        //if (hasAvg && v == rowNum)
            //        //    return timeStr == "month" ? '月平均' : '平均';
            //        //else
            //        //    return timeStr == "month" ? (v + '月') : v;
            //    }
            //    // font: '10px Arial',
            //}
        }],
        series: series
    });
    vcl.chart = chart;
    return chart;
}


Ext.define('Attribute', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'AttributeCode', type: 'string' },
        { name: 'AttributeDesc', type: 'string' },
    ]
});
proto.createGrid = function () {
    var me = view;
    var vcl = view.vcl;
    
    var attributeCombinationStore = Ext.create('Ext.data.Store', {
        model: 'Attribute',
        data: []
    });
    var grid = Ext.create('Ext.grid.Panel', {
        title: "物料特征",
        flex: 7,
        border: '10 5 10 5',
        height: 750,
        width: 400,
        store: attributeCombinationStore,
        scroll: 'both',
        columns: [
            { text: "特征标识", dataIndex: "AttributeCode" },
            { text: "特征描述", dataIndex: "AttributeDesc" }
        ],
        listeners: {
            itemclick: function (dataview, record, item, index, e) {
                var record = e.record;
                var ret = vcl.invorkBcf("GetMayDaySafeStock", [vcl.materialId, record.get("AttributeCode")]);
                vcl.fillData.call(vcl, ret);
                var dt = vcl.dataSet.getTable(1);
                var rowNum = dt.data.items.length;
                var list = [];
                var modelName = "com.MatDayStockAdjustChart";
                for (var i = 0; i < rowNum; i++) {
                    var date = dt.data.items[i].get("FDATE");
                    var needQty = dt.data.items[i].get("NEEDQTY");
                    var stockQuantity = dt.data.items[i].get("STOCKQUANTITY");
                    var safeStockNum = dt.data.items[i].get("SAFESTOCKNUM");
                    list.push(Ext.create(modelName, {
                        time: date,
                        needQty: needQty,
                        stockQuantity: stockQuantity,
                        safeStockNum: safeStockNum
                    }));
                }
                vcl.chart.store.loadData(list);
            }
        }
    });
    vcl.materialGrid = grid;
    return grid;
}
