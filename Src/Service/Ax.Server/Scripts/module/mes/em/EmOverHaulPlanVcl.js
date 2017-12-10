EmOverHaulPlanVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = EmOverHaulPlanVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = EmOverHaulPlanVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);

    if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }
    else if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo.tableIndex == 0) {
            var masterRow = this.dataSet.getTable(0).data.items[0];
            this.forms[0].updateRecord(masterRow);
            JudgeTaskstate.call(this, e);
        }
        if (e.dataInfo.tableIndex == 1) {
            var fieldName = e.dataInfo.fieldName;
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var bodyDt = this.dataSet.getTable(1);
            var bodyDtLength = bodyDt.data.items.length;
            var starttime = masterRow.data["STARTTIME"];
            var endtime = masterRow.data["ENDTIME"];
            if (fieldName == "PLANSTARTTIME") {
                var rowid=e.dataInfo.dataRow.data["ROW_ID"]
                var mintime = e.dataInfo.value;
                for (var k = 0; k < bodyDtLength; k++) {
                    var planstarttimne = bodyDt.data.items[k].data["PLANSTARTTIME"];
                    if (mintime > planstarttimne && bodyDt.data.items[k].data["ROW_ID"] != rowid) {
                        mintime = planstarttimne;
                    }
                }
                Ext.getCmp('STARTTIME0_' + this.winId).setValue(mintime);
            }
            else if (fieldName == "PLANENDTIME") {
                var rowid = e.dataInfo.dataRow.data["ROW_ID"]
                if (fieldName == "PLANENDTIME") {
                    var rowid = e.dataInfo.dataRow.data["ROW_ID"]
                    var maxtime = e.dataInfo.value;
                    for (var k = 0; k < bodyDtLength; k++) {
                        var planendtime = bodyDt.data.items[k].data["PLANENDTIME"];
                        if (maxtime < planendtime && bodyDt.data.items[k].data["ROW_ID"] != rowid) {
                            maxtime = planendtime;
                        }
                    }
                    Ext.getCmp('ENDTIME0_' + this.winId).setValue(maxtime);
                }
                else if (fieldName == "PERSONGROUPID") {
                    if (e.dataInfo.value != e.dataInfo.oldValue) {
                        e.dataInfo.dataRow.set("PERSONID", "");
                        e.dataInfo.dataRow.set("PERSONNAME", "");
                        if (Ext.isEmpty(e.dataInfo.value)) {
                            e.dataInfo.dataRow.set("PERSONGROUPNAME", "");
                        }
                    }
                }
                this.forms[0].updateRecord(masterRow);
                
            }
        }
    }
    else if (e.libEventType == LibEventTypeEnum.ButtonClick) {

        if (e.dataInfo.fieldName == "Load") {
            if (this.isEdit) {
                var gridName = "EMOVERHAULPLANFUNC";
                ////第一个参数:是datafunc 的progId
                Ax.utils.LibVclSystemUtils.openDataFunc("Em.OverHaulPlanDataFunc", "来源单信息", [this]);
            }
            else {
                Ext.Msg.alert("系统提示", "非编辑状态下不可操作！");
            }
        }
        if (e.dataInfo.fieldName == "SelectDetial") {
            //弹出查看明细dataFunc
            var grid = Ext.getCmp(this.winId + 'EMOVERHAULPLANDETAILGrid'); //要加载数据的表名字 + Grid
            var records = grid.getView().getSelectionModel().getSelection();
            if (records.length != 1) {
                Ext.Msg.alert("系统提示", "请选择一条数据查看明细");
            }
            else {
                var frombillno = records[0].data["FROMBILLNO"];
                var fromrowid = records[0].data["FROMROWID"];
                var overhaulplanbillno = records[0].data["BILLNO"];
                var ishaveDetial = this.invorkBcf("IsOverHaulHaveDetial", [frombillno, fromrowid, overhaulplanbillno]);
                if (ishaveDetial == true) {
                    Ax.utils.LibVclSystemUtils.openDataFunc("Em.OverHaulPlanDetail", "明细信息", [this]);
                }
                else {
                    Ext.Msg.alert("提示", "请下达当前检修计划并生效当前单据后再查看明细");
                }
            }
        }
    }
    else if (e.libEventType == LibEventTypeEnum.Validating) {
        var fieldName = e.dataInfo.fieldName;
        var bodyDt = this.dataSet.getTable(1);
        var bodyDtLength = bodyDt.data.items.length;
        if (e.dataInfo.tableIndex == 0) {
            //判断表头手动输入时间的合理性
            if (fieldName == "STARTTIME") {
                if (bodyDtLength > 0) {
                    var isprimtStarttime = true;
                    for (var i = 0; i < bodyDtLength; i++) {
                        if (e.dataInfo.value > bodyDt.data.items[i].data["PLANSTARTTIME"]) {
                            isprimtStarttime = false;
                        }
                    }
                    if (isprimtStarttime == false) {
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("系统提示", "当前检修计划单的开始时间和结束时间取值范围偏小，请重新调整！");
                    }
                }
            }
            else if (fieldName == "ENDTIME") {
                if (bodyDtLength > 0) {
                    var isprimtEndtime = true;
                    for (var i = 0; i < bodyDtLength; i++) {
                        if (e.dataInfo.value < bodyDt.data.items[i].data["PLANENDTIME"]) {
                            isprimtEndtime = false;
                        }
                    }
                    if (isprimtEndtime == false) {
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("系统提示", "当前检修计划单的开始时间和结束时间取值范围偏小，请重新调整！");
                    }
                }
            }
        }
        if (e.dataInfo.tableIndex == 1) {
            if (fieldName == "PLANSTARTTIME" || fieldName == "PLANENDTIME") {
                if (e.dataInfo.dataRow.get("TASKSTATE") == 0) {
                    if (fieldName == "PLANSTARTTIME") {
                        //var masterRowStartTime=masterRow.data.items.data["STARTTIME"];
                        var planendtime = e.dataInfo.dataRow.get("PLANENDTIME");
                        if (planendtime < e.dataInfo.value) {
                            e.dataInfo.cancel = true;
                            Ext.Msg.alert("系统提示", "计划开始时间不能晚于计划结束时间");
                        }
                    } else if (fieldName == "PLANENDTIME") {
                        var planstarttime = e.dataInfo.dataRow.get("PLANSTARTTIME");
                        if (e.dataInfo.value < planstarttime) {
                            e.dataInfo.cancel = true;
                            Ext.Msg.alert("系统提示", "计划开始时间不能晚于计划结束时间");
                        }
                    }
                }
                else {
                    e.dataInfo.cancel = true;
                    Ext.Msg.alert("系统提示", "作业已经执行不能修改计划开始时间和计划结束时间");
                }
            }
            else if (fieldName == "EQUIPMENTID") {
                var taskstate = e.dataInfo.dataRow.get("TASKSTATE");
                if (taskstate > 0)//作业状态大于0时，表示已开始或已完成
                {
                    e.dataInfo.cancel = true;
                    Ext.Msg.alert("系统提示", "该行作业计划已经执行，不能修改设备");
                }
            } else if (fieldName == "PERSONGROUPID") {
                var rowtype = e.dataInfo.dataRow.get("ROWTYPE");
                if (rowtype == 1) {//如果为合并项，则不允许修改人员组编码字段
                    e.dataInfo.cancel = true;
                    Ext.Msg.alert("系统提示", "此行数据不允许修改人员组！");
                }
            }
        }
    }
    else if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow)
        if (e.dataInfo.tableIndex == 1) {
            if (e.dataInfo.dataRow.get("TASKSTATE") > 0) {//作业状态大于0时，表示已开始或已完成
                Ext.Msg.alert("系统提示", "要删除的作业已经执行，不能删除作业！");
                e.dataInfo.cancel = true;
            }
        }
}

//判断是否可以改变计划状态
function JudgeTaskstate(e) {
    if (e.dataInfo.fieldName == "PLANSTATE") {
        var rows = this.dataSet.getTable(1).data.items;
        var count = 0;
        for (var i = 0; i < rows.length; i++) {
            if (rows[i].get("TASKSTATE") != 0)
                count++;
        }
        if (count > 0) {
            e.dataInfo.dataRow.cancel = true;
            Ext.Msg.alert("系统提示", "存在已经执行的作业，不能修改计划状态");
        }
    }
}