EmTaskPlanDetailVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = EmTaskPlanDetailVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = EmTaskPlanDetailVcl;

proto.winId = null;
proto.fromObj = null;
var records = null;
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj[0];
    var grid = Ext.getCmp(proto.winId + 'EMTASKPLANDETAILGrid'); //要加载数据的表名字 + Grid
    records = grid.getView().getSelectionModel().getSelection();
    //表头填充
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set("TASKID", records[0].data["TASKID"]);
    masterRow.set("TASKNAME", records[0].data["TASKNAME"]);
    masterRow.set("TASKATTR", records[0].data["TASKATTR"]);
    masterRow.set("EQUTYPEID", records[0].data["EQUTYPEID"]);
    masterRow.set("EQUTYPENAME", records[0].data["EQUTYPENAME"]);
    this.forms[0].loadRecord(masterRow);
    //表身填充
    
    var returnList = this.invorkBcf("GetDetial", [records[0].data["BILLNO"], records[0].data["ROW_ID"]]);
    FillData.call(this,returnList);
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
}

function FillData(returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var list = returnList["Key"];//一般是中间层返回来的数据，中间可能定义的是dictionary,在前段反序列化之后是对象
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set("ROWNO", i + 1);
                newRow.set("FROMBILLNO", info.FromBillno)
                newRow.set("FROMROWID", info.FromrowID);
                newRow.set("EQUIPMENTID", info.EquipmentID)
                newRow.set("EQUIPMENTNAME", info.EquipmentName);
                newRow.set("EQUIPMENTMODEL", info.EquipmentModel)
                newRow.set("PLANSTARTTIME", info.PlanstartTime);
                newRow.set("PLANENDTIME", info.PlanendTime);
                newRow.set("TASKSTATE", info.TaskState);
                newRow.set("STARTTIME", info.StartTime);
                newRow.set("ENDTIME", info.EndTime);
                newRow.set("PERSONGROUPID", info.PersongroupID);
                newRow.set("PERSONGROUPNAME", info.PersongroupName);
                newRow.set("PERSONID", info.PersonID);
                newRow.set("PERSONNAME", info.PersonName);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
        returnList = null;
    }
}
