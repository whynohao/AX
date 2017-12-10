MonPointRegionView = function () {
    Ax.tpl.LibBillTpl.apply(this, arguments);
    this.vcl.funcView.add('createMain', { name: 'createMain', display: '区域配置图表' });
};
var proto = MonPointRegionView.prototype = Object.create(Ax.tpl.LibBillTpl.prototype);
proto.constructor = MonPointRegionView;

proto.createMain = function () { 
    var me = this;
    var vcl = this.vcl;

    vcl.proxy = true;
    vcl.isEdit = false;//切换回默认视图之后，默认视图是否为修改状态

    var headTable = me.vcl.dataSet.getTable(0).data.items[0].data;
    var bodyTable = me.vcl.dataSet.getTable(1).data;//取明细表

    var addCount = 0;//新增的节点数量
    var deleteCount = 0;//删除的节点数量
    var oriCount = 0;//原有的节点数量

    oriCount = bodyTable.length;

    var addBtn = Ext.create(Ext.Action, {
        text: ' 新增节点  ',
        width:100,
        handler: function () {
            {
                //var MonPointStr = me.panel.getForm().findField('PointRegionId').getRawValue();
                //var MonPoint = MonPointStr.split(",");
                var panelTop = Ext.getCmp('panelTId');
                var MonPointStr = panelTop.getForm().findField('PointRegionId').getRawValue();
                var MonPoint = MonPointStr.split(",");
                var panelCenter = Ext.getCmp('panelCId');
                var hasInPanel = false;//判断点位是否已经在画布中

                //判断布局中是否已经有了该区域
                var nodes = proto.Main.scene.getDisplayedNodes();
                if (nodes.length == 0 && MonPoint[0] != "") {//没有节点且点位不为空
                    var node = new JTopo.Node(MonPoint[1]);
                    node.pointId = MonPoint[0];
                    node.pointName = MonPoint[1];
                    node.setLocation(20, 20);
                    proto.Main.scene.add(node);
                    addCount++;

                }
                else if (nodes.length != 0 && MonPoint[0] != "") {
                    for (var i = 0; i < nodes.length; i++) {
                        if (nodes[i].pointId == MonPoint[0]) {
                            hasInPanel = true;
                            alert(MonPoint[1] + " 点位已经在画布中");
                        }
                    }
                    if (hasInPanel == false) {//不在画布中则添加该点位
                        var node = new JTopo.Node(MonPoint[1]);
                        node.pointId = MonPoint[0];
                        node.pointName = MonPoint[1];
                        node.setLocation(20, 20);
                        proto.Main.scene.add(node);
                        addCount++;
                    }
                }
                else if (MonPoint[0] == "") {
                    alert("请选择点位之后再增加");
                }
            }
        }
    });
    var deleteBtn = Ext.create(Ext.Action, {
        text: ' 删除节点 ',
        width: 100,
        handler: function () {
            var panelTop = Ext.getCmp('panelTId');
            var AreaIdTxt = panelTop.getForm().findField('AreaIDTxt').getValue();//获取区域代码text的值
            var node = proto.Main.scene.selectedElements[0];
            if (node == undefined) {
                alert("请选中节点再移除");
            }
            else {
                proto.Main.scene.remove(node);
                deleteCount++;
                alert("已移除 " + node.pointName + " 点位");
            }
        }
    });
    var saveBtn = Ext.create(Ext.Action, {
        width: 100,
        text: ' 保存节点 ',
        handler: function () {
            var panelTop = Ext.getCmp('panelTId');
            var nodes = proto.Main.scene.getDisplayedNodes();//获取布局中的所有节点
            var nodesIdList = new Array();
            var nodesNameList = new Array();
            var nodesX = new Array();
            var nodesY = new Array();

            if (addCount + oriCount - deleteCount != nodes.length) {
                alert("总节点数量与当前画布中的节点数量不相等（请查看是否有节点在画布外）");
            }
            else if (addCount + oriCount - deleteCount == nodes.length) {
                if (nodes.length == 0) {
                    alert("画布中没有点位!");
                }
                else if (nodes.length > 0) {
                    for (var i = 0; i < nodes.length; i++) {
                        if (nodesY[i] > headTable["POINTREGIONHEIGHT"] || nodesX[i] > headTable["POINTREGIONWIDTH"] || nodesX[i] < 0 || nodesY[i] < 0) {
                            alert(nodes[i].pointId + "点位超出区域范围");
                        }
                        else {
                            nodesIdList[i] = nodes[i].pointId;
                            nodesNameList[i] = nodes[i].pointName;
                            nodesX[i] = nodes[i].x;
                            nodesY[i] = nodes[i].y;
                        }
                    }
                    var AreaWidth = headTable["POINTREGIONWIDTH"];
                    var AreaHeight = headTable["POINTREGIONHEIGHT"];
                    var AreaIdTxt = panelTop.getForm().findField('AreaIDTxt').getValue();//获取区域代码text的值
                    var AreaNameTxt = panelTop.getForm().findField('AreaNameTxt').getValue();//获取区域名称text的值
                    var saveSuccess = me.vcl.invorkBcf('SavePointFromView', [AreaIdTxt, AreaNameTxt,AreaWidth,AreaHeight, nodesIdList, nodesNameList, nodesX, nodesY]);//调用后台的新增方法
                    if (saveSuccess == true) {
                        var alertString = "保存成功!新增"+addCount+"个节点，删除"+deleteCount+"个节点，共保存"+(addCount+oriCount-deleteCount)+"个节点"
                        alert(alertString);
                    }
                }
            }
        }
    });

    var changBtn = Ext.create(Ext.Action, {
        width: 100,
        text: ' 切换图表 ',
        menu: [
        { text: '默认视图' },
        { text: '区域点位配置表' },
        ]
    });

    var mainWidth = document.body.clientWidth > 1210 ? document.body.clientWidth - 10 : 1210;
    var mainPanel = Ext.create('Ext.panel.Panel', {
        width: mainWidth,
        height: document.body.clientHeight - 80,
        layout: { type: 'border' },
        //tbar: Ax.utils.LibToolBarBuilder.createToolBar([addBtn,deleteBtn,saveBtn, vcl.createChangeView(me, 'create', 'createMain', ' 图表 ')]),
        tbar: Ax.utils.LibToolBarBuilder.createToolBar([addBtn, deleteBtn, saveBtn, vcl.createChangeView(me, 'create', 'createMain', ' 图表 ')]),
        items: [me.Main.createTop(me), me.Main.createCenter(mainWidth,me)],
        border: false,
    });
    return mainPanel;
};

proto.Main = {
    createTop: function (me) {
        var headTable = me.vcl.dataSet.getTable(0).data.items[0].data;

        var panel = Ext.create("Ext.form.Panel",
            {
                id: 'panelTId',
                border: false,
                headerPosition:'left',
                collapsible: false,//可折叠
                autoScroll: true,//自动创建滚动条
                region: 'north',
                //title:"参数设置",
                width: 100,
                height: 40,

                //buttonAlign:'center',
                defaultType: 'textfield',
                defaoults: {
                    //anchor: '100%',
                },
                fieldDefaults: {
                    //labelWidth: 80,
                    labelWidth:60,
                    labelAlign: "left",
                    flex: 1,
                    margin: "5 5 5 20"
                },
                items: [
                    {
                        xtype: "container",
                        layout: "column",
                        items: [
                            //width: '15%',
                            { xtype: "textfield", id: "AreaIDTxt", text: "", columnWidth: 0.20, fieldLabel: '区域代码', labelStyle: 'font-size:12px;color:#a7392e;horizontal-align:left;margin: 5,5,5,0' },

                            { xtype: "textfield", id: "AreaNameTxt", text: "", columnWidth: 0.20, fieldLabel: '区域名称', labelStyle: 'font-size:12px;color:#a7392e;horizontal-align:left;margin: 5,5,5,0' },

                            { xtype: 'libSearchfield', name: 'PointRegionId', columnWidth: 0.20, fieldLabel: '点位', labelStyle: 'color:#a7392e', relSource: { 'Mon.Pointlocation': '' }, relName: '', relPk: '', selParams: [], tableIndex: 0, selectFields: 'A.POINTID,A.POINTNAME' },

                            //添加按钮
                            //{
                            //    xtype: "button", name: 'addBtn', text: "增加点位", width: '10%', margin: 20,
                            //    handler: function () {
                            //        var MonPointStr = panel.getForm().findField('PointRegionId').getRawValue();
                            //        var MonPoint = MonPointStr.split(",");
                            //        var panelCenter = Ext.getCmp('panelCId');
                            //        var hasInPanel = false;//判断点位是否已经在画布中

                            //        //判断布局中是否已经有了该区域
                            //        var nodes = proto.Main.scene.getDisplayedNodes();
                            //        if (nodes.length == 0 && MonPoint[0] != "") {//没有节点且点位不为空
                            //            var node = new JTopo.Node(MonPoint[1]);
                            //            node.pointId = MonPoint[0];
                            //            node.pointName = MonPoint[1];
                            //            node.setLocation(20, 20);
                            //            proto.Main.scene.add(node);
                            //        }
                            //        else if (nodes.length != 0 && MonPoint[0] != "") {
                            //            for (var i = 0; i < nodes.length; i++) {
                            //                if (nodes[i].pointId == MonPoint[0]) {
                            //                    hasInPanel = true;
                            //                    alert(MonPoint[1] + " 点位已经在画布中");
                            //                }
                            //            }
                            //            if (hasInPanel == false) {//不在画布中则添加该点位
                            //                var node = new JTopo.Node(MonPoint[1]);
                            //                node.pointId = MonPoint[0];
                            //                node.pointName = MonPoint[1];
                            //                node.setLocation(20, 20);
                            //                proto.Main.scene.add(node);
                            //            }
                            //        }
                            //        else if (MonPoint[0] == "") {
                            //            alert("请选择点位之后再增加");
                            //        }
                            //    }
                            //},

                            //删除按钮
                            //{
                            //    xtype: 'button', name: 'deleteBtn', id: 'deleteBtnId', text: '移除点位', width: '10%', margin: 20, handler: function () {
                            //        var AreaIdTxt = panel.getForm().findField('AreaIDTxt').getValue();//获取区域代码text的值
                            //        var node = proto.Main.scene.selectedElements[0];
                            //        if (node.pointId == "") {
                            //            alert("请选中节点再移除");
                            //        }
                            //        else {
                            //            proto.Main.scene.remove(node);
                            //            alert("已移除 " + node.pointName + " 点位");
                            //        }
                            //    }
                            //},

                            //保存按钮
                            //{
                            //    xtype: "button", name: 'saveBtn', text: "保存点位", width: '10%', margin: 20, handler: function () {
                            //        var nodes = proto.Main.scene.getDisplayedNodes();//获取布局中的所有节点
                            //        var nodesIdList = new Array();
                            //        var nodesNameList = new Array();
                            //        var nodesX = new Array();
                            //        var nodesY = new Array();
                            //        if (nodes.length == 0) {
                            //            alert("画布中没有点位!");
                            //        }
                            //        else if (nodes.length > 0) {
                            //            for (var i = 0; i < nodes.length; i++) {
                            //                if (nodesY[i] > headTable["POINTREGIONHEIGHT"] || nodesX[i] > headTable["POINTREGIONWIDTH"]) {
                            //                    alert(nodes[i].pointId + "点位超出区域范围");
                            //                }
                            //                else {
                            //                    nodesIdList[i] = nodes[i].pointId;
                            //                    nodesNameList[i] = nodes[i].pointName;
                            //                    nodesX[i] = nodes[i].x;
                            //                    nodesY[i] = nodes[i].y;
                            //                }
                            //            }
                            //            var AreaIdTxt = panel.getForm().findField('AreaIDTxt').getValue();//获取区域代码text的值
                            //            var AreaNameTxt = panel.getForm().findField('AreaNameTxt').getValue();//获取区域名称text的值
                            //            var saveSuccess = me.vcl.invorkBcf('SavePointFromView', [AreaIdTxt, AreaNameTxt, nodesIdList,nodesNameList, nodesX, nodesY]);//调用后台的新增方法
                            //            if (saveSuccess==true) {
                            //                alert("保存成功!");
                            //            }
                            //        }
                            //    }
                            //},
                        ],
                    }
                ]
            });

        if (headTable["POINTREGIONID"] == "") {

        }
        else {
            Ext.getCmp("AreaIDTxt").setValue(headTable["POINTREGIONID"]);
            Ext.getCmp("AreaNameTxt").setValue(headTable["POINTREGIONNAME"]);
            var AreaIdTxt = Ext.getCmp("AreaIDTxt");
            var AreaNameTxt = Ext.getCmp("AreaNameTxt");
            AreaIdTxt.readOnly = true;
            AreaNameTxt.readOnly = true;
        }

        return panel;
    },

    createCenter: function (w, me) {
        var headTable = me.vcl.dataSet.getTable(0).data.items[0].data;//取主表
        var headTable1 = me.vcl.dataSet.getTable(0).data;//取主表
        var bodyTable = me.vcl.dataSet.getTable(1).data;//取明细表
        var panelCenter = new Ext.panel.Panel({
            id: 'panelCId',
            region: 'center',
            active: 0,
            items: [{
                title: '区域点位',
                authHeight: false,
                authWidth:false,
                closable: false,//是否可关闭
                html: '<canvas width="' + w + '" height="700" id="areaConfiguration"></canvas>',
                listeners: {
                    afterRender: function () {

                        var canvas = document.getElementById('areaConfiguration');
                        var stage = new JTopo.Stage(canvas);
                        //显示工具栏
                        showJTopoToobar(stage);
                        //stage.mode = 'edit';
                        //proto.Main.stage.mode = "drag";
                        proto.Main.scene = new JTopo.Scene(stage);
                        proto.Main.scene.width = headTable["POINTREGIONWIDTH"];
                        proto.Main.scene.height = headTable["POINTREGIONHEIGHT"];

                        var areaFileGUIdString = headTable["INTERNALID"];
                        if (headTable['IMGSRC'] == "") {
                            proto.Main.scene.background = './img/bg.jpg';//添加背景图
                        }
                        else {
                            proto.Main.scene.background = "../UserPicture/Mon.PointRegion/" + areaFileGUIdString + '/' + headTable["IMGSRC"];//添加背景图
                            //proto.Main.scene.background = './img/bg.jpg';//添加背景图
                        }
                        for (var i = 0; i < bodyTable.length; i++) {
                            var node = new JTopo.Node(bodyTable.items[i].data["POINTNAME"]);
                            //if (bodyTable.items[i].data["LOCATIONX"] >= 0 || bodyTable.items[i].data["LOCATIONY"]>=0) {
                                node.setLocation(bodyTable.items[i].data["LOCATIONX"], bodyTable.items[i].data["LOCATIONY"]);
                                node.pointId = bodyTable.items[i].data["POINTID"];
                                node.pointName = bodyTable.items[i].data["POINTNAME"];
                                proto.Main.scene.add(node);

                                //node.mousedrag(function (event) {
                                //    console.log("拖拽");
                                //});
                                //proto.Main.scene.mousedrag(function (event)
                                //{
                                //    console.log("scene拖拽");
                                //})
                            //}
                        }
                    }
                }
            },
            ]
        });
        return panelCenter;
    }
}




