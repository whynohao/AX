plsMasterProducePlanView = function () {
    Ax.tpl.LibGridTpl.apply(this, arguments);
    this.vcl.funcView.add('createMain', { name: 'createMain', display: '甘特图' });
    //if (this.vcl.funcView.containsKey("default")) {
    //    this.vcl.funcView.get("default").name = "createMain";
    //}
};
var proto = plsMasterProducePlanView.prototype = Object.create(Ax.tpl.LibGridTpl.prototype);
proto.constructor = plsMasterProducePlanView;


Ext.define('MasterProduceModel', {
    extend: 'Sch.model.Resource',
    fields: ['Quantity']                
});

var GetDate;

//var billNo ;
//var materialId;

proto.createMain = function () {


    var me = this;
    var vcl = this.vcl;
    var date = new Date().toLocaleDateString().split('/');
    var newDate = new Date();
    var tree = ProducePlanTree(me, vcl, getDate(date));
    var D = Ext.Date;
    //tree.eventStore.filter("StartDate", date);
    tree.setTimeSpan(D.add(newDate, D.HOUR, 8), D.add(newDate, D.HOUR, 18));

  
    return tree;



};
function ProducePlanTree(me, vcl, date) {
    //this.win.vcl.proxy
    vcl.proxy = true;
    vcl.isEdit = true;

    var taskData = [];
    var data = vcl.invorkBcf('GetSelData', []);
    //data = vcl.dataSet.getTable(0).data.items;
    var newobj = creatObj(data, vcl);
    var resourceStore = Ext.create('Sch.data.ResourceTreeStore', {
        model: 'MasterProduceModel',
        root: newobj
    })


    var eventStore = Ext.create('Sch.data.EventStore', {
        data: creatList(newobj)
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
        viewPreset: 'weekAndDay',
        //startDate: new Date(time),
        //endDate: new Date(time + 7), //取7天范围
        layout: { type: 'hbox', align: 'stretch' },
        // 初始化Gird
        lockedGridConfig: {
            resizeHandles: 'e',
            resizable: { pinned: true },
            width: 300
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
               text: '数量',
               width: 50,
               sortable: true,
               dataIndex: 'Quantity'
           },
        ],

        tbar: [{
            //id: 'span3',
            enableToggle: true,
            text: '选择时间',
            toggleGroup: 'span',
            scope: tree,
            menu: Ext.create('Ext.menu.DatePicker', {
                handler: function (dp, date) {
                    var D = Ext.Date;








                    var data = vcl.invorkBcf('GetSelData', [getDate(date.toLocaleDateString().split('/'))]);
                    //tree = creatTree(me, vcl, getDate(date.toLocaleDateString().split('/')));
                    //var data = vcl.invorkBcf('GetSelData', [getDate(date.toLocaleDateString().split('/'))]);
                     //newobj = creatObj(data, vcl);

                    //var resourceStore = Ext.create('Sch.data.ResourceTreeStore', {
                    //    model: 'MasterProduceModel',
                    //    root: newobj
                    //})
                    //var Date = Date.substr(0, 4) + '-' + Date.substr(4, 2) + '-' + Date.substr(6, 2) + '00:00';
                    tree.eventStore.filter("StartDate", date);
                    //var eventStore = Ext.create('Sch.data.EventStore', {
                    //    data: creatList(newobj)
                    //});
                    //tree.resourceStore.root.removeAll();
                    //tree.eventStore.data.removeAll();
                    //tree.resourceStore.loadData(newobj, true);
                    //tree.eventStore.loadData(creatList(newobj), true);
                    //tree.resourceStore.clearFilter();





                    //tree.eventStore.filter("StartDate", date);
                    //tree.eventStore.filter("StartDate", [date.addDays(1)]);
                    //curDate =   new Date().addDays(1);
                    tree.setTimeSpan(D.add(date, D.HOUR, 8), D.add(date, D.HOUR, 18));
                    return tree;
                },
                scope: tree
            })
        }, {
            xtype: 'button', text: '保存', handler: function () {
                if (vcl.invorkBcf('GetData', [taskData]) == 1) {
                    Ext.Msg.show({
                        title: "提示!",
                        modal: true,
                        msg: '保存成功！',
                        icon: Ext.Msg.Info,
                        buttons: Ext.Msg.YES,
                    });
                    taskData = [];
                }
                else {
                    Ext.Msg.show({
                        title: "提示!",
                        modal: true,
                        msg: '保存失败！',
                        icon: Ext.Msg.Info,
                        buttons: Ext.Msg.YES,
                    });
                }
            }
        },
        '->', {
            iconCls: 'icon-prev',
            scale: 'medium',
            scope: tree,
            handler: function () {
                tree.shiftPrevious();
            }
        }, {
            iconCls: 'icon-next',
            scale: 'medium',
            scope: tree,
            handler: function () {
                tree.shiftNext();
            }
        },
        ],


        listeners: {
            aftereventdrop: function (s, r) {

                var date = r[0].data.StartDate.toLocaleDateString().split('/');
                var newDate = getDate(date);
                var billNo = r[0].data['ResourceId'].split(',')[0];
                var materialId = r[0].data['ResourceId'].split(',')[1];
                for (var i = 0; i < data.length; i++) {
                    if (data[i].BillNo == billNo && (data[i].EndMaterialId == materialId || data[i].HalfMaterialId == materialId)) {
                        if (data[i].PlanDate == GetDate) {
                            data[i].PlanDate = newDate;
                            var isExist = false;

                            var index;
                            for (var j = 0; j < taskData.length; j++) {
                                if (taskData[j].taskId == data[i].TaskId) {
                                    isExist = true;
                                    index = j;
                                    break;
                                }
                            }
                            if (isExist) {
                                taskData[index].taskDate = newDate;
                            }
                            else {
                                taskData.push({ taskId: data[i].TaskId, taskDate: newDate, taskNum: r[0].data.Name });
                            }
                            break;
                        }
                    }
                }
            },


            beforedragcreate:function (s, r) {
                return false;
            },

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
                    //var window = Ext.WindowManager.getActive();
                    //window.el.setZIndex(1100000);
                    return false;
                }
            },
            beforeeventresize: function (s, r) {
                return false;
            },
            beforeeventdrag: function (s, r) {
                var date = r.data.StartDate.toLocaleDateString().split('/');
                GetDate = getDate(date);
                billNo = r.data.ResourceId.split(',')[0];
                materialId = r.data.ResourceId.split(',')[1];
            },

            scope: this
        },

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
        plugins: [
           Ext.create("Sch.plugin.EventEditor", {
               height: 200,
               width: 300,

               buttonAlign: 'center',
               deleteText: '删除',
               saveText: '保存',
               cancelText: '取消',
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

               // 信息框
               fieldsPanelConfig: {
                   xtype: 'container',
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
                                    id: 'BillNo',
                                    name: 'BillNo',
                                    readOnly: true,
                                    fieldLabel: '订单号'
                                }),
                                 new Ext.form.TextField({
                                     id: 'Material',
                                     name: 'Material',
                                     readOnly: true,
                                     fieldLabel: '产品'
                                 }),
                                 new Ext.form.Number({
                                     id: 'Quantity',
                                     name: 'Name',
                                     readOnly: true,
                                     fieldLabel: '数量'
                                 }),
                           ]
                       }]
                   }]
               }
           }),
        ]
    })
    
    return tree;
}
Date.prototype.addDays = function (number) {
    return new Date(this.getTime() + 24 * 60 * 60 * 1000  * number)
    //alert("Date" + adjustDate.getFullYear() + "-" + adjustDate.getMonth() + "-" + adjustDate.getDate());
    
}




//resourceStore
function creatObj(data, vcl) {
    var MATERIALID;
    var MATERIALNAME;
    var Id;
    var newobj = {
        children: []
    };

    for (var i = 0; i < data.length; i++) {
        var b = false;
        var index;
        for (var j = 0; j < newobj.children.length; j++) {
            if (newobj.children[j].Name == data[i].BillNo) {
                b = true;
                index = j;
                break;
            }
        }
        if (data[i].EndMaterialId == "") {
            MATERIALID = data[i].HalfMaterialId;
            MATERIALNAME = data[i].HalfMaterialName;
        }
        else {
            MATERIALID = data[i].EndMaterialId;
            MATERIALNAME = data[i].EndMaterialName;
        }

        if (b) {
            var c = false;
            var Mindex;

            for (var k = 0; k < newobj.children[index].children.length; k++) {
                if (newobj.children[index].children[k].Name == MATERIALID + "," + MATERIALNAME) {
                    c = true;
                    Mindex = k;
                    break;
                }
            }
            if (c) {
                newobj.children[index].children[Mindex].children.push({
                    Name: MATERIALID + ',' + MATERIALNAME,
                    Quantity: data[i].ProduceNum,
                    DeliveryTime: data[i].PlanDate
                });
                newobj.children[index].children[Mindex].Quantity = parseInt(newobj.children[index].children[Mindex].Quantity) + parseInt(data[i].ProduceNum);

            } else {
                var newMchild = [{
                    Id: data[i].BillNo + ',' + MATERIALID,
                    Name: MATERIALID + ',' + MATERIALNAME,
                    iconCls: 'sch-gate',
                    leaf: true,
                    Quantity: data[i].ProduceNum,
                    children: [{
                        Name: MATERIALID + ',' + MATERIALNAME,
                        Quantity: data[i].ProduceNum,
                        DeliveryTime: data[i].PlanDate
                    }
                    ]
                }];
                newobj.children[index].children.push(newMchild[0]);

            }
        }
        else {
            var newchild = {
                Name: data[i].BillNo,
                iconCls: 'sch-gates-bundle',
                expanded: false,
                children: []
            };

            newchild.children.push({
                Id: data[i].BillNo + ',' + MATERIALID,
                Name: MATERIALID + ',' + MATERIALNAME,
                iconCls: 'sch-gate',
                leaf: true,
                Quantity: data[i].ProduceNum,
                children: [{
                    Name: MATERIALID + ',' + MATERIALNAME,
                    Quantity: data[i].ProduceNum,
                    DeliveryTime: data[i].PlanDate
                }
                ]
            })
            newobj.children.push(newchild);
        }
    }
    return newobj;
}

//eventStore
function creatList(newobj) {
    var MList = [];
    for (var i = 0; i < newobj.children.length; i++) {
        for (var j = 0; j < newobj.children[i].children.length; j++) {

            var BodyItems = newobj.children[i].children[j];
            var MId = BodyItems.Id;
            for (var k = 0; k < BodyItems.children.length; k++) {
                var Date = BodyItems.children[k]["DeliveryTime"].toString();
                var StartDate = Date.substr(0, 4) + '-' + Date.substr(4, 2) + '-' + Date.substr(6, 2) + '00:00';
                var EndDate = Date.substr(0, 4) + '-' + Date.substr(4, 2) + '-' + Date.substr(6, 2) + '23:59';
                MList.push({
                    ResourceId: MId, Name: BodyItems.children[k]["Quantity"], StartDate: StartDate, EndDate: EndDate, Material: BodyItems.children[k]["Name"], BillNo: newobj.children[i]["Name"]
                })
            }
        }
    }
    return MList;
}

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
