MonPointFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = MonPointFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = MonPointFuncVcl;
proto.winId = null;
proto.fromObj = null;
proto.getType = 0;
proto.doSetParam = function (vclObj) {
    //console.log(arguments);
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj[0];
};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    //不允许手工添加行
    if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }
        //不允许手工删除行
    else if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }

    else if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo.tableIndex == 0) {
            if (e.dataInfo.fieldName == "EQUIPMENTID") {
                this.deleteAll(1);
                var equipmentId = this.dataSet.getTable(0).data.items[0].data['EQUIPMENTID'];
                if (equipmentId != "") {
                    var returnData = this.invorkBcf("GetData", [equipmentId]);
                    fillData.call(this, returnData);
                }
            }
        }
    }

    else if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        //全选按钮事件
        if (e.dataInfo.fieldName == "btnCheckAll") {
            var allItems = this.dataSet.getTable(1).data.items;
            for (var i = 0; i < allItems.length; i++) {
                allItems[i].set("ISCHOSE", "1");
            }
        }

        //取消按钮
        else if (e.dataInfo.fieldName == "btnCheckCancel") {
            var allItems = this.dataSet.getTable(1).data.items;
            for (var i = 0; i < allItems.length; i++) {
                allItems[i].set("ISCHOSE", "0");
            }
        }

        //确定按钮
        else if (e.dataInfo.fieldName == "btnCheck") {
            var grid = Ext.getCmp(this.winId + 'MONPOINTVALUEDETAILFUNCGrid');//通过ID找到对应的Grid
            var selectItems = this.dataSet.getTable(1).data.items;
            var checkItems = [];
            for (var i = 0; i < selectItems.length; i++) {
                if (selectItems[i].data["ISCHOSE"] == true) {
                    //获取打勾选中行,组成新的数组
                    checkItems.push({
                        EQUIPMENTID: selectItems[i].data["EQUIPMENTID"],
                        POINTID: selectItems[i].data["POINTID"],
                        POINTNAME: selectItems[i].data["POINTNAME"],
                    });
                }
            }

            if (checkItems.length <= 0) {
                Ext.Msg.alert("系统提示", "请选择载入的点位");
            }
            else {
                this.win.close();
                fillGetnoticeReturnData.call(this, checkItems);
            }
        }

    }
}

function fillData(returnData) {
    Ext.suspendLayouts();//关闭Ext布局
    var formStore = this.dataSet.getTable(1);//tableIndex是指当前grid所在的表索引，中间层第几个表，curStore是grid的数据源，在extjs中是指Store
    formStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var formBill = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var list = returnData['POINTID'];//一般是中间层返回来的数据，中间可能定义的是dictionary,在前段反序列化之后是对象
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(formBill, 1);//这个方法第一个参数是表头数据行，第二个参数是当前grid数据源store所属第几个表
                newRow.set('EQUIPMENTID', info.EQUIPMENTID);
                newRow.set('POINTID', info.POINTID);
                newRow.set('POINTNAME', info.POINTNAME);
            }
        }
    }
    finally {
        formStore.resumeEvents();//打开store事件
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);//打开Ext布局

    }
}


function fillGetnoticeReturnData(records) {

    var grid = Ext.getCmp(proto.winId + 'MONPOINTVALUEDETAILGrid');
    Ext.suspendLayouts();
    var fromStore = proto.fromObj.dataSet.getTable(1);
    fromStore.suspendEvents();//关闭store事件
    try {
        if (records !== undefined && records.length > 0) {
            for (var i = 0; i < records.length; i++) {
                var info = records[i];
                if (checkGetNotice(grid, info)) {
                    Ext.Msg.alert("系统提示", "所选点位已经在点位明细中,请重新选取！");
                }
                else {
                    var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
                    var newRow = proto.fromObj.addRow(masterRow, 1);
                    newRow.set('EQUIPMENTID', info["EQUIPMENTID"]);
                    newRow.set('POINTID', info["POINTID"]);
                    newRow.set('POINTNAME', info["POINTNAME"]);
                  
                }
            }
        }
    }
    finally {
        fromStore.resumeEvents();
        if (fromStore.ownGrid && fromStore.ownGrid.getView().store != null)
            fromStore.ownGrid.reconfigure(fromStore);
        Ext.resumeLayouts(true);
    }
}

//判断 点位是否已经在明细列表中
function checkGetNotice(grid, info) {
    var checkOk = false;
    var records = grid.store.data.items;
    //设备作业计划单明细 重复判断 
    for (var i = 0; i < records.length; i++) {
        if (records[i].get('POINTID') == info["POINTID"] && records[i].get('POINTNAME') == info["POINTNAME"]) {
            checkOk = true;
        }
    }
    return checkOk;
}


