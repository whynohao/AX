

plsLogisticsPlanView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createMain', { name: 'createMain', display: '甘特图' });
    //    if (this.vcl.funcView.containsKey("default")) {
    //    this.vcl.funcView.get("default").name = "createMain";
    //}
};
var proto = plsLogisticsPlanView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = plsLogisticsPlanView;

Ext.define('plsLogisticsPlanModel', {
    extend: 'Sch.model.Resource',
    fields: ['Quantity', 'DeliveryTime']
});


proto.createMain = function () {
    var me = this;
    var vcl = this.vcl;
    var myDate = new Date();

    var date = new Date().toLocaleDateString().split('/');
    var tree = creatTree(me, vcl, getDate(date));

    var D = Ext.Date;
    //tree.eventStore.filter("StartDate", new Date());
    tree.setTimeSpan(D.add(myDate, D.HOUR, 8), D.add(myDate, D.HOUR, 18));

    return tree;
}

function creatTree(me, vcl, date) {
    var myDate = new Date();
    var time = myDate.toLocaleDateString();//当前日期
    var T = getDate(myDate.toLocaleDateString().split('/'));
    var nextweek = new Date(myDate.getTime() + 1000 * 60 * 60 * 24 * 7).toLocaleDateString();//下一周时间
    var T1 = getDate(new Date(myDate.getTime() + 1000 * 60 * 60 * 24 * 7).toLocaleDateString().split('/'));
    var dt = vcl.invorkBcf("selectdt", []);
    var dt1 = vcl.invorkBcf("selectdt1", []);
    var count = dt.length//主表行数
    var count1 = dt1.length//子表行数
    var x = 1;


    //this.win.vcl.proxy
    vcl.proxy = true;
    vcl.isEdit = true;
    var mainWidth = document.body.clientWidth > 1210 ? document.body.clientWidth - 10 : 1210;

    var newRoot = creatLogisticsPlanObj(dt, dt1, vcl);
    var resourceStore = Ext.create('Sch.data.ResourceTreeStore', {
        model: 'plsLogisticsPlanModel',
        root: newRoot
    })

    var data = creatLogisticsPlandata(dt, dt1, vcl);
    var eventStore = Ext.create('Sch.data.EventStore', {
        data: data
    });

    var tree = Ext.create('Sch.panel.SchedulerTree', {
        width: '100%',
        height: 600,
        //renderTo: 'CustomerGantt-Div',
        rowHeight: 32,
        //左边grid Store
        resourceStore: resourceStore,
        //右边时间 store
        eventStore: eventStore,
        viewPreset: 'weekAndDay',  //日程表上方菜单栏格式
        //startDate: new Date(time),
        //endDate: new Date(time + 7), //取7天范围
        layout: { type: 'hbox', align: 'stretch' },
        // 初始化Gird
        lockedGridConfig: {
            resizeHandles: 'e',
            resizable: { pinned: true },
            width: 400
        },
        listeners: {
            //边框不能拉
            //beforeeventresizefinalize: function (s, dragContext) {
            //    dragContext.finalize(false);
            //}
            //,
            beforedragcreate: function (s, r) {
                return false;
            },
            beforeeventresize: function (s, r) {
                return false;
            },
            //模块拖动事件
            aftereventdrop: function (s, r) {
                var ID = r[0].data.ResourceId.toString(); //ID
                var Name = r[0].data.Name.toString();  //Name
                var MATERIALID = ID.substr(ID.indexOf('@') + 1, ID.indexOf('#') - ID.indexOf('@') - 1);//物料ID
                var RowNo = ID.substr(ID.indexOf('#') + 1, ID.indexOf('$') - ID.indexOf('#') - 1);//行号
                var FROMROWID = ID.substring(ID.indexOf('$') + 1);//来源行标识
                var BillNo = ID.substr(0, 14);//订单号
                //获取拖动后的日期  转换日期格式
                var startyear = r[0].data.StartDate.getFullYear().toString();
                var startmonth = (r[0].data.StartDate.getMonth() + 1).toString();
                if (startmonth == "1" || startmonth == "2" || startmonth == "3" || startmonth == "4" || startmonth == "5" || startmonth == "6" || startmonth == "7" || startmonth == "8" || startmonth == "9") {
                    startmonth = "0" + startmonth;
                }
                var startday = r[0].data.StartDate.getDate().toString();
                if (startday == "1" || startday == "2" || startday == "3" || startday == "4" || startday == "5" || startday == "6" || startday == "7" || startday == "8" || startday == "9") {
                    startday = "0" + startday;
                }
                var startdata = startyear + startmonth + startday;

                var endyear = r[0].data.EndDate.getFullYear().toString();
                var endmonth = (r[0].data.EndDate.getMonth() + 1).toString();
                if (endmonth == "1" || endmonth == "2" || endmonth == "3" || endmonth == "4" || endmonth == "5" || endmonth == "6" || endmonth == "7" || endmonth == "8" || endmonth == "9") {
                    endmonth = "0" + endmonth;
                }
                var endday = r[0].data.EndDate.getDate().toString();
                if (endday == "1" || endday == "2" || endday == "3" || endday == "4" || endday == "5" || endday == "6" || endday == "7" || endday == "8" || endday == "9") {
                    endday = "0" + endday;
                }
                var enddata = endyear + endmonth + endday;
                //如果存在行号，说明是子表中的数据（主表的日程表不做操作）
                if (RowNo != "") {
                    for (var i = 0; i < count1; i++) {  //子表循环
                        //订单号，来源行标识和行号相同
                        if (dt1[i].SalesorderNo == BillNo && dt1[i].RowNo == RowNo && dt1[i].FromrowId == FROMROWID) {
                            //更新计划发货时间
                            dt1[i].plsLogisticsPlanView = enddata;
                            //this.forms[1].updateRecord(this.dataSet.getTable(1).data.items[i]);
                            //把改动信息传到后台字典
                            vcl.invorkBcf("storage", [BillNo, FROMROWID, RowNo, enddata, x]);
                            x++;
                        }
                    }
                }



            }
            ,
            beforeeventdropfinalize: function (s, dragContext) {
                if (dragContext.resourceRecord !== dragContext.newResource) {

                    Ext.Msg.show({
                        title: "提示!",
                        modal: true,
                        msg: '不能拖到其他行！',
                        icon: Ext.Msg.Info,
                        fn: function (btn) {
                            dragContext.finalize(false);
                        },
                        buttons: Ext.Msg.YES,
                    });
                    return false;
                }
            }
        },

        // 初始化时间表
        schedulerConfig: {
            scroll: true,
            columnLines: false,
            flex: 1
        },
        //创建时间轴时，默认显示在时间轴上的字符串设置
        onEventCreated: function (newFlight) {
            //newFlight.set('Name', 'New departure');
        },
        columnLines: true,
        rowLines: true,
        columns: [
           {
               xtype: 'treecolumn', //this is so we know which column will show the tree
               text: '订单编号',
               width: 200,
               sortable: true,
               dataIndex: 'Name'
           },
           {
               text: '计划发货数量',
               width: 100,
               sortable: true,
               dataIndex: 'Quantity'
           },
           {
               text: '要求送达时间',
               width: 100,
               sortable: true,
               dataIndex: 'DeliveryTime'
           }
        ],
        tbar: [
                {
                    //左上角时间选择控件
                    //id: 'span3',
                    enableToggle: true,
                    text: '选择时间',
                    toggleGroup: 'span',
                    scope: tree,
                    menu: Ext.create('Ext.menu.DatePicker', {
                        handler: function (dp, date) {
                            var D = Ext.Date;

                            tree.eventStore.filter("StartDate", date);
                            //转到所选时间页
                            tree.setTimeSpan(D.add(date, D.HOUR, 8), D.add(date, D.HOUR, 18));
                            return tree;
                        },
                        scope: tree,
                    })
                },
  //Ax.utils.LibToolBarBuilder.createToolBar([vcl.createChangeView(me, 'createMain', 'createMain', '甘特图')]),
                {
                    //保存按钮
                    xtype: 'button', text: '保存', handler: function () {
                        x--;
                        //保存方法
                        vcl.invorkBcf("update", [x]);
                        x = 1;
                        Ext.Msg.show({
                            title: "提示!",
                            modal: true,
                            msg: '保存成功！',
                            icon: Ext.Msg.Info,
                            buttons: Ext.Msg.YES,
                        });
                    }
                },
                '->',
                //前进后退按钮
                 {
                     iconCls: 'icon-prev',
                     scale: 'medium',
                     scope: tree,
                     handler: function () {
                         tree.shiftPrevious();
                     }
                 },
                {
                    iconCls: 'icon-next',
                    scale: 'medium',
                    scope: tree,
                    handler: function () {
                        tree.shiftNext();
                    }
                },
        ],
        viewConfig: {
            getRowClass: function (r) {
                if (r.get('Id') === 3 || r.parentNode.get('Id') === 3) {
                    return 'some-grouping-class';
                }

                if (r.get('Id') === 9 || r.parentNode.get('Id') === 9) {
                    return 'some-other-grouping-class';
                }
            }
        },
        //插件
        plugins: [
           Ext.create("Sch.plugin.EventEditor", {
               height: 190,
               width: 280,

               //按钮中心对齐
               buttonAlign: 'center',
               deleteText: '删除',
               saveText: '保存',
               cancelText: '取消',
               durationUnit: Sch.util.Date.DAY,
               durationText: "天",
               listeners: {
                   beforeeventsave: function (s, r) {
                       //var Num = s.fieldsPanelConfig.items[0].items[0].items[2].value;
                       //var date = r.data.StartDate.toLocaleDateString().split('/');
                       //var newDate = getDate(date);
                       //addtaskData(r, data, newDate, Num);
                       //return false;
                   },
                   beforeeventdelete: function (s, r) {
                       return false;
                   },
                   scope: this
               },
               // panel with form fields
               //字段面板配置
               fieldsPanelConfig: {

                   xtype: 'container',

                   //布局
                   layout: 'card',
                   items: [
                 {
                     xtype: 'form',

                     layout: 'hbox',

                     style: 'background:#fff',
                     cls: 'editorpanel',
                     border: false,

                     items: [{
                         padding: 10,

                         style: 'background:#fff',
                         border: false,

                         flex: 2,

                         layout: 'anchor',

                         defaults: {
                             anchor: '100%'
                         },

                         items: [
                      new Ext.form.TextField({
                          id: 'SalesorderNo',
                          name: 'SalesorderNo',
                          readOnly: true,
                          fieldLabel: '订单号'
                      }),
                       new Ext.form.TextField({
                           id: 'CustomerName',
                           name: 'CustomerName',
                           readOnly: true,
                           fieldLabel: '客户'
                       }),
                       new Ext.form.TextField({
                           id: 'SendareaName',
                           name: 'SendareaName',
                           readOnly: true,
                           fieldLabel: '发货区域'
                       }),
                         ]
                     }]
                 }]



               }

           }),
        ]

    })


    return tree;
};
//日期转YYYYMMDD
function getDate(date) {
    var year = date[0];
    var month = date[1];
    if (month.length == 1) {
        month = '0' + month;
    }
    var day = date[2];
    if (day.length == 1) {
        day = '0' + day;
    }
    return year + month + day;
}

function creatLogisticsPlanObj(dt, dt1, vcl) {
    var newObj = [];//左边树状图root
    var b = true;
    var list = [];
    for (var i = 0; i < dt.length; i++) {     //主表循环
        var Children = [];
        for (var j = 0; j < dt1.length; j++) {    //子表循环
            //若来源行标识，销售订单号相同
            if (dt[i].FromrowId == dt1[j].FromrowId && dt[i].SalesorderNo == dt1[j].SalesorderNo) {
                var r = dt1[j].PlanlogisticsDate.toString().replace(/^(\d{4})(\d{2})(\d{2})$/, "$1-$2-$3");//计划发货时间
                Children.push({
                    //@和#是作为订单号，物料ID和行号之间的分隔符
                    Id: dt[i].SalesorderNo + "@" + dt[i].MaterialId + "#" + dt1[j].RowNo + "$" + dt1[j].FromrowId,
                    //Name: dt1[j]["ROWNO"],
                    Name: dt[i].MaterialId + ','+ dt[i].MaterialName,
                    leaf: true,
                    iconCls: 'sch-gate',
                    Quantity: dt1[j].PlansendNum,   //计划发货数量
                    DeliveryTime: r
                })
            }
        }

        //判断主表是不是同一个单号，相同单号就不再创建
        for (var k = 0; k < list.length; k++) {
            if (list[k] == dt[i].SalesorderNo) {
                b = false;
                break;
            }
        }
        //创建主表
        if (b) {
            var m = dt[i].SalesorderNo + "@" + dt[i].MaterialId + "#" + "$";//@和#是作为订单号，物料ID和行号之间的分隔符
            newObj.push({
                Id: m,
                Name: dt[i].SalesorderNo,
                iconCls: 'sch-gates-bundle',
                expanded: false,
                children: Children
            })
            list.push(dt[i].SalesorderNo);
            b = true;
        }
            //把子表加到对应主表里
        else {
            for (var s = 0; s < newObj.length; s++) {
                if (newObj[s].Name == dt[i].SalesorderNo) {
                    for (var y = 0; y < Children.length; y++) {
                        newObj[s].children.push(Children[y]);
                    }
                    b = true;
                    break;
                }
            }
        }
    }
    var newRoot = { Id: 0, children: newObj };
    return newRoot;
}

function creatLogisticsPlandata(dt, dt1, vcl) {
    var data = [];     //右边日程表data
    var datalist = [];
    var c = true;
    for (var i = 0; i < dt.length; i++) {   //主表循环
        for (var j = 0; j < dt1.length; j++) {   //子表循环
            //若来源行标识，销售订单号相同
            if (dt[i].FromrowId == dt1[j].FromrowId && dt[i].SalesorderNo == dt1[j].SalesorderNo) {
                var plantime = dt1[j].PlanlogisticsDate.toString().replace(/^(\d{4})(\d{2})(\d{2})$/, "$1-$2-$3") + " 23:59";//计划发货时间
                var plantime1 = dt1[j].PlanlogisticsDate.toString().replace(/^(\d{4})(\d{2})(\d{2})$/, "$1-$2-$3") + " 00:00";//计划发货时间前一天
                data.push({
                    //@和#是作为订单号，物料ID和行号之间的分隔符
                    ResourceId: dt[i].SalesorderNo + "@" + dt[i].MaterialId + "#" + dt1[j].RowNo + "$" + dt1[j].FromrowId,
                    Name: dt[i].MaterialName,
                    SalesorderNo: dt[i].SalesorderNo,
                    CustomerName: dt[i].CustomerName,
                    SendareaName: dt[i].SendareaName,
                    StartDate: plantime1,
                    EndDate: plantime
                })
            }
        }

        //判断主表是不是同一个单号，相同单号就不再创建
        //for (var x = 0; x < datalist.length; x++) {
        //    if (datalist[x] == dt[i].SalesorderNo) {
        //        c = false;
        //        break;
        //    }
        //}
        //创建主表
        //if (c) {
        //    var headID = dt[i].SalesorderNo + "@" + dt[i].MaterialId + "#" + "$";//@和#是作为订单号，物料ID和行号之间的分隔符
        //    var headtime = dt[i].LastestDate.toString().replace(/^(\d{4})(\d{2})(\d{2})$/, "$1-$2-$3") + " 23:59";//最迟发货日期
        //    var headtime1 = dt[i].LastestDate.toString().replace(/^(\d{4})(\d{2})(\d{2})$/, "$1-$2-$3") + " 00:00";//最迟发货日期
        //    data.push({
        //        ResourceId: headID,
        //        Name: dt[i].MaterialName,
        //        SalesorderNo: dt[i].SalesorderNo,
        //        CustomerName: dt[i].CustomerName,
        //        SendareaName: dt[i].SendareaName,
        //        StartDate: headtime1,
        //        EndDate: headtime
        //    })
        //    datalist.push(dt[i].SalesorderNo);
        //    c = true;
        //}

    }
    return data;
}