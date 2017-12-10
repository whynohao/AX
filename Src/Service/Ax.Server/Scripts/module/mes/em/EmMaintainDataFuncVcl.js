EmMaintainDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = EmMaintainDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = EmMaintainDataFuncVcl;

proto.winId = null;
proto.fromObj = null;
var records = null;
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj[0];
    var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
    var equipmentid = masterRow.get("EQUIPMENTID");
    var equipmentname = masterRow.get("EQUIPMENTNAME");
    var emfaultid = masterRow.get("EMFAULTID");
    var thisMasterRow = this.dataSet.getTable(0).data.items[0];
    thisMasterRow.set("EQUIPMENTID", equipmentid);
    thisMasterRow.set("EQUIPMENTNAME", equipmentname);
    proto.fromObj.forms[0].loadRecord(masterRow);
    var returnList = this.invorkBcf("GetTask", [equipmentid, emfaultid]);
    FillDataFunc.call(this, returnList);
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            //不允许手工添加行
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            //不允许手工删除行
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            var fieldName = e.dataInfo.fieldName;
            if (fieldName == "btnSure") {
                var grid = Ext.getCmp(this.winId + 'TASKDETIALGrid'); //dataFun子表
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length == 1) {
                    FillMaintainMasterRow.call(this, records);
                    this.win.close();
                }
                else {
                    Ext.Msg.alert("系统提示","请选择一行数据");
                }
            }
            break;
    }
}

//dataFunc表身填充
function FillDataFunc(returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var list = returnList;
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set("BILLNO", info.Billno);
                newRow.set("ROWNO", i + 1);
                newRow.set("ROWID", info.Rowid);
                newRow.set("PLANYEAR", info.Planyear);
                newRow.set("PLANMONTH", info.Planmonth);
                newRow.set("REPAIRBILLNO", info.Repairbillno);
                newRow.set("EMFAULTID", info.Emfaultid);
                newRow.set("EMFAULTNAME", info.Emfaultname);
                newRow.set("ISANALYSIS", info.Isanalysis);
                newRow.set("TASKID", info.Taskid);
                newRow.set("TASKNAME", info.TaskName);
                newRow.set("PLANSTARTTIME", info.Planstarttime);
                newRow.set("PLANENDTIME", info.Planendtime);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}
//维修单表头填充
function FillMaintainMasterRow(returnData) {      
    var list = returnData;
    var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
    var ctr1 = Ext.getCmp("TASKID0_" + proto.winId);
    ctr1.store.add({ Id: list[0].data["TASKID"], Name: list[0].data["TASKNAME"] });
    ctr1.select(list[0].data["TASKID"]);
    var ctr2 = Ext.getCmp("EMFAULTID0_" + proto.winId);
    ctr2.store.add({ Id: list[0].data["EMFAULTID"], Name: list[0].data["EMFAULTNAME"] });
    ctr2.select(list[0].data["EMFAULTID"]);
    proto.fromObj.forms[0].updateRecord(masterRow);
    masterRow.set("ISANALYSIS", list[0].data["ISANALYSIS"]);
    masterRow.set("FROMBILLNO", list[0].data["BILLNO"])
    masterRow.set("FROMROWID", list[0].data["ROWID"]);
    proto.fromObj.forms[0].loadRecord(masterRow);
       
    
}