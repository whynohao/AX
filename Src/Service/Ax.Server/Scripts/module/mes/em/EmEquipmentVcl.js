EmEquipmentVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = EmEquipmentVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = EmEquipmentVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            this.forms[0].updateRecord(masterRow);
            var fieldName = e.dataInfo.fieldName;
            if (e.dataInfo.tableIndex == 0) {
                if (fieldName == "EQUTYPEID" || fieldName == "OPERATIONID") {
                    LoadOptionData.call(this,fieldName, e);
                }
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            e.dataInfo.cancel = true;
            break;
    }
}

function LoadOptionData(fieldName, e) {
    var data;
    var newthis = this;
    var equtypeid = e.dataInfo.dataRow.get("EQUTYPEID");
    var operationid = e.dataInfo.dataRow.get("OPERATIONID");


    if (!Ext.isEmpty(operationid)) {
        switch (fieldName) {
            case "EQUTYPEID"://设备类型
                data = this.invorkBcf('ReturnEmTaskList', ["0", equtypeid]);
                break;
            case "OPERATIONID"://设备运维
                data = this.invorkBcf('ReturnEmTaskList', ["1", operationid]);
                break;
        }
        var list = data;
        var len = this.dataSet.getTable(1).data.length;
        if (list.length > 0) {
            if (len > 0) {
                Ext.Msg.confirm("系统提示", "变更设备运维会影响设备作业明细信息，确认变更吗？", function (btn) {
                    if (btn == "yes") {
                        FillEquipmentData.call(newthis, list);
                    }
                    else {
                        e.dataInfo.dataRow.set("OPERATIONID", e.dataInfo.oldValue);
                        e.dataInfo.curForm.loadRecord(e.dataInfo.dataRow);
                    }
                })
            }
            else
                FillEquipmentData.call(this, list);
        }
    }
    else {
        var len = this.dataSet.getTable(1).data.length;
        if (len > 0) {
            Ext.Msg.confirm("系统提示", "确定要清空设备的作业明细信息吗？", function (btn) {
                if (btn == "yes")
                    newthis.dataSet.getTable(1).removeAll();
                else {
                    e.dataInfo.dataRow.set("OPERATIONID", e.dataInfo.oldValue);
                    newthis.forms[0].loadRecord(e.dataInfo.dataRow);
                }
            })
        }
    }
        
}

function FillEquipmentData(list) {
    console.log(list);
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        //var grid = Ext.getCmp(this.winId + "EQUIPMENTDETAILGrid");
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set("TASKID", info.TaskId);
                newRow.set("TASKNAME", info.TaskName);
                newRow.set("NEEDCEASE", info.NeedCease);
                newRow.set("STARTTIME", info.StartTime);
                newRow.set("EXECCYCLE", info.ExecCycle);
                newRow.set("TIMEUNIT", info.TimeUnit);
                newRow.set("ISUSE", info.IsUse);
                newRow.set("ISMANYPERSON", info.IsManyPerson);
                newRow.set("PERSONGROUPID", info.PersonGroupId);
                newRow.set("PERSONGROUPNAME", info.PersonGroupName);
                newRow.set("PERSONID", info.PersonId);
                newRow.set("PERSONNAME", info.PersonName);
            }
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}