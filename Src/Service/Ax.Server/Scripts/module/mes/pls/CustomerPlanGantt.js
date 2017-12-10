var obj;
obj = {
    Id: 0,
    children: [{
        Id: 3,
        Name: 'Gates 1 - 5',
        iconCls: 'sch-gates-bundle',
        expanded: true,

        children: [
            {
                Id: 4,

                Name: 'Gate 1',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 100,
                DeliveryTime: 20160119
            },
            {
                Id: 5,

                Name: 'Gate 2',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 45,
                DeliveryTime: 20160119
            },
            {
                Id: 6,

                Name: 'Gate 3',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 45,
                DeliveryTime: 20160119
            },
            {
                Id: 7,

                Name: 'Gate 4',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 65,
                DeliveryTime: 20160119
            },
            {
                Id: 8,

                Name: 'Gate 5',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 75,
                DeliveryTime: 20160119
            }
        ]
    }, {
        Id: 9,
        Name: 'Gates 6 - 10',
        iconCls: 'sch-gates-bundle',
        expanded: true,

        children: [
            {
                Id: 10,

                Name: 'Gate 6',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 77,
                DeliveryTime: 20160119
            },
            {
                Id: 11,

                Name: 'Gate 7',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 85,
                DeliveryTime: 20160119
            },
            {
                Id: 12,

                Name: 'Gate 8',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 95,
                DeliveryTime: 20160119
            },
            {
                Id: 13,

                Name: 'Gate 9',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 55,
                DeliveryTime: 20160119
            },
            {
                Id: 14,

                Name: 'Gate 10',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 15,
                DeliveryTime: 20160119
            }
        ]
    }, {
        Id: 16,
        Name: 'Gates 1 - 5',
        iconCls: 'sch-gates-bundle',

        children: [
            {
                Id: 17,

                Name: 'Gate 1',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 15,
                DeliveryTime: 20160119
            },
            {
                Id: 18,

                Name: 'Gate 2',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 45,
                DeliveryTime: 20160119
            },
            {
                Id: 19,

                Name: 'Gate 3',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 45,
                DeliveryTime: 20160119
            },
            {
                Id: 20,

                Name: 'Gate 4',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 65,
                DeliveryTime: 20160119
            },
            {
                Id: 21,

                Name: 'Gate 5',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 70,
                DeliveryTime: 20160119
            }
        ]
    }, {
        Id: 22,
        Name: 'Gates 6 - 10',
        iconCls: 'sch-gates-bundle',

        children: [
            {
                Id: 23,

                Name: 'Gate 6',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 80,
                DeliveryTime: 20160119
            },
            {
                Id: 24,

                Name: 'Gate 7',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 120,
                DeliveryTime: 20160119
            },
            {
                Id: 25,

                Name: 'Gate 8',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 125,
                DeliveryTime: 20160119
            },
            {
                Id: 26,

                Name: 'Gate 9',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 100,
                DeliveryTime: 20160119
            },
            {
                Id: 27,

                Name: 'Gate 10',
                leaf: true,
                iconCls: 'sch-gate',
                Quantity: 100,
                DeliveryTime: 20160119
            }
        ]
    }



                // eof Terminal B

        // eof Kastrup
    ]
    // eof top level
};

Ext.onReady(function () {
    Ext.define('Model', {
        extend: 'Sch.model.Resource',
        fields: ['Quantity', 'DeliveryTime']
    });
    var resourceStore = Ext.create('Sch.data.ResourceTreeStore', {
        model: 'Model',
        root: obj
    })

    var eventStore = Ext.create('Sch.data.EventStore', {
        data: [
            // Grouping tasks
            { ResourceId: 3, Name: 'Summary', StartDate: "2011-12-02 08:20", EndDate: "2011-12-02 11:25" },
            { ResourceId: 3, Name: 'Summary', StartDate: "2011-12-02 12:10", EndDate: "2011-12-02 13:50" },
            { ResourceId: 3, Name: 'Summary', StartDate: "2011-12-02 14:30", EndDate: "2011-12-02 16:10" },

            { ResourceId: 6, Name: 'London 895', StartDate: "2011-12-02 08:20", EndDate: "2011-12-02 09:50" },
            { ResourceId: 4, Name: 'Moscow 167', StartDate: "2011-12-02 09:10", EndDate: "2011-12-02 10:40" },
            { ResourceId: 5, Name: 'Berlin 291', StartDate: "2011-12-02 09:25", EndDate: "2011-12-02 11:25" },
            { ResourceId: 7, Name: 'Brussel 107', StartDate: "2011-12-02 12:10", EndDate: "2011-12-02 13:50" },
            { ResourceId: 8, Name: 'Krasnodar 101', StartDate: "2011-12-02 14:30", EndDate: "2011-12-02 16:10" },

            { ResourceId: 17, Name: 'Split 811', StartDate: "2011-12-02 16:10", EndDate: "2011-12-02 18:30" },
            { ResourceId: 18, Name: 'Rome 587', StartDate: "2011-12-02 13:15", EndDate: "2011-12-02 14:25" },
            { ResourceId: 24, Name: 'Praga 978', StartDate: "2011-12-02 16:40", EndDate: "2011-12-02 18:00" },
            { ResourceId: 25, Name: 'Stockholm 581', StartDate: "2011-12-02 11:10", EndDate: "2011-12-02 12:30" },

            { ResourceId: 10, Name: 'Copenhagen 111', StartDate: "2011-12-02 16:10", EndDate: "2011-12-02 18:30" },
            { ResourceId: 11, Name: 'Gothenburg 233', StartDate: "2011-12-02 13:15", EndDate: "2011-12-02 14:25" },
            { ResourceId: 12, Name: 'New York 231', StartDate: "2011-12-02 16:40", EndDate: "2011-12-02 18:00" },
            { ResourceId: 13, Name: 'Paris 321', StartDate: "2011-12-02 11:10", EndDate: "2011-12-02 12:30" }
        ]
    });


    var tree = Ext.create('Sch.panel.SchedulerTree', {
        width: '100%',
        height: 600,
        renderTo: 'CustomerGantt-Div',
        rowHeight: 32,

        //左边grid Store
        resourceStore: resourceStore,
        //右边时间 store
        eventStore: eventStore,

        viewPreset: 'hourAndDay',
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
           {
               text: '要求送达时间',
               width: 150,
               sortable: true,
               dataIndex: 'DeliveryTime'
           }
        ],
        tbar: [
                {
                    id: 'span3',
                    enableToggle: true,
                    text: '选择时间',
                    toggleGroup: 'span',
                    scope: tree,
                    menu: Ext.create('Ext.menu.DatePicker', {
                        handler: function (dp, date) {
                            var D = Ext.Date;
                            tree.setTimeSpan(D.add(date, D.HOUR, 8), D.add(date, D.HOUR, 18));
                        },
                        scope: tree
                    })
                },
                '->',
                {
                    iconCls: 'icon-next',
                    scale: 'medium',
                    scope: tree,
                    handler: function () {
                        tree.shiftNext();
                    }
                }
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
        plugins: [
           Ext.create("Sch.plugin.EventEditor", {
               height: 190,
               width: 280,

               //timeConfig: {
               //    minValue: '08:00',
               //    maxValue: '18:00'
               //},

               // dateConfig : {
               // },
               //            
               // durationUnit : Sch.util.Date.DAY,
               // durationConfig : {
               // minValue : 1,
               // maxValue : 10
               // },

               buttonAlign: 'center',
               deleteText: '删除',
               saveText: '保存',
               cancelText: '取消',

               // panel with form fields
               fieldsPanelConfig: {
                   xtype: 'container',

                   layout: 'card',

                   //items: [
                   //// form for "Meeting" EventType
                   //{
                   //    EventType: 'Meeting',

                   //    xtype: 'form',

                   //    layout: 'hbox',

                   //    style: 'background:#fff',
                   //    cls: 'editorpanel',
                   //    border: false,

                   //    items: [{
                   //        padding: 10,

                   //        style: 'background:#fff',
                   //        border: false,

                   //        flex: 2,

                   //        layout: 'anchor',

                   //        defaults: {
                   //            anchor: '100%'
                   //        },

                   //        items: [this.titleField = new Ext.form.TextField({

                   //            // doesn't work in "defaults" for now (4.0.1)
                   //            labelAlign: 'top',

                   //            name: 'Title',
                   //            fieldLabel: '任务'
                   //        }),

                   //        this.locationField = new Ext.form.TextField({

                   //            // doesn't work in "defaults" for now (4.0.1)
                   //            labelAlign: 'top',

                   //            name: 'Location',
                   //            fieldLabel: '位置'
                   //        })]
                   //    }]

                   //},
                   //// eof form for "Meeting" EventType

                   //// form for "Appointment" EventType
                   //{
                   //    EventType: 'Appointment',

                   //    xtype: 'form',

                   //    style: 'background:#fff',
                   //    cls: 'editorpanel',
                   //    border: false,

                   //    padding: 10,

                   //    layout: {
                   //        type: 'vbox',
                   //        align: 'stretch'
                   //    },

                   //    items: [new Ext.form.TextField({

                   //        // doesn't work in "defaults" for now (4.0.1)
                   //        labelAlign: 'top',

                   //        name: 'Location',
                   //        fieldLabel: '位置'
                   //    }), {
                   //        xtype: 'combo',

                   //        store: ["Dental", "Medical"],

                   //        labelAlign: 'top',

                   //        name: 'Type',
                   //        fieldLabel: '类型'
                   //    }]
                   //}
                   //// eof form for "Appointment" EventType
                   //]
               }
               // eof panel with form fields

           }),
        ]
        //headerConfig: {
        //    bottom: {
        //        unit: "WEEK",
        //        increment: 1,
        //        renderer: function () {
        //            return Sch.util.HeaderRenderers.dateNumber.apply(tree, arguments);
        //        }
        //    },
        //    middle: {
        //        unit: "WEEK",
        //        dateFormat: 'D d M Y',
        //        align: 'left'
        //    }
        //}
    })
})