EmTaskPlanVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = EmTaskPlanVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = EmTaskPlanVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            this.forms[0].updateRecord(masterRow);
            var fieldName = e.dataInfo.fieldName;
            if (e.dataInfo.tableIndex == 0) {
                if (fieldName == "PLANMONTH") {
                    var month = e.dataInfo.value;
                    if (month > 12) {
                        e.dataInfo.dataRow.set("PLANMONTH", 12);
                    }
                    else if (month < 1) {
                        e.dataInfo.dataRow.set("PLANMONTH", 1);
                    }
                    var year = e.dataInfo.dataRow.get("PLANYEAR");
                    var deptid = e.dataInfo.dataRow.get("DEPTID");
                    IsExistSameData.call(this, year, month, deptid);
                }
                if (fieldName == "PLANYEAR") {
                    var year = e.dataInfo.value;
                    if (year.toString().length != 4) {
                        var curdate = new Date();
                        e.dataInfo.dataRow.set("PLANYEAR", Ext.Date.format(curdate, "Y"));
                    }
                    var month = e.dataInfo.dataRow.get("PLANMONTH");
                    var deptid = e.dataInfo.dataRow.get("DEPTID");
                    IsExistSameData.call(this, year, month, deptid);
                }
                if (fieldName == "DEPTID") {
                    var deptid = e.dataInfo.value;
                    var year = e.dataInfo.dataRow.get("PLANYEAR");
                    var month = e.dataInfo.dataRow.get("PLANMONTH");
                    IsExistSameData.call(this, year, month, deptid);
                }
                e.dataInfo.curForm.loadRecord(e.dataInfo.dataRow);
            }
            else if (e.dataInfo.tableIndex == 1) {
                if (fieldName == "PERSONGROUPID") {
                    if (e.dataInfo.value != e.dataInfo.oldValue) {
                        e.dataInfo.dataRow.set("PERSONID", "");
                        e.dataInfo.dataRow.set("PERSONNAME", "");
                        if (Ext.isEmpty(e.dataInfo.value)) {
                            e.dataInfo.dataRow.set("PERSONGROUPNAME", "");
                        }
                    }
                }
            }
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                var isauto = e.dataInfo.dataRow.get("ISAUTO");
                //var rowtype = e.dataInfo.dataRow.get("ROWTYPE");
                var fieldName = e.dataInfo.fieldName;
                if (fieldName == "ROWTYPE" || fieldName == "EQUIPMENTID" || fieldName == "EQUTYPEID" || fieldName == "TASKID" || fieldName == "NEEDCEASE") {
                    if (isauto == true) {
                        e.dataInfo.cancel = true;
                        var name;
                        switch (fieldName) {
                            case "ROWTYPE":
                                name = "行项类型";
                                break;
                            case "EQUIPMENTID":
                                name = "设备";
                                break;
                            case "EQUTYPEID":
                                name = "设备类型";
                                break;
                            case "TASKID":
                                name = "作业";
                                break;
                            case "NEEDCEASE":
                                name = "需要停机";
                                break;
                        }
                        alert("该行数据由系统生成，不能修改字段【" + name + "】的值");
                        //Ext.Msg.alert("系统提示", "该行数据由系统生成，不能修改字段【" + name + "】的值");                        
                    }
                    else {
                        if (fieldName == "EQUIPMENTID")
                        {
                            var taskstate = e.dataInfo.dataRow.get("TASKSTATE");
                            if (taskstate > 0)//作业状态大于0时，表示已开始或已完成
                            {
                                e.dataInfo.cancel = true;
                                Ext.Msg.alert("系统提示", "该行作业计划已经执行，不能修改设备");
                            }
                        }
                    }
                }
                if (fieldName == "PLANSTARTTIME" || fieldName == "PLANENDTIME") {
                    if (e.dataInfo.dataRow.get("TASKSTATE") != 0) {//作业状态为2时，表示已经完成
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("系统提示", "该作业已经执行，不能修改计划开始时间和计划结束时间");
                    }
                    else {
                        if (!Ext.isEmpty(e.dataInfo.dataRow.get("RELATIONBILLNO"))) {
                            e.dataInfo.cancel = true;
                            Ext.Msg.alert("系统提示", "已经存在检修计划单，修改计划开始时间和计划结束时间操作无效");
                        }
                    }
                }
                if (fieldName == "PERSONGROUPID") {
                    var rowtype = e.dataInfo.dataRow.get("ROWTYPE");
                    if (rowtype == 1) {//如果为合并项，则不允许修改人员组编码字段
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("系统提示", "此行数据不允许修改人员组！");
                    }
                }
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            var fieldName = e.dataInfo.fieldName;
            if (fieldName == "btnScheduling") {
                this.invorkBcf('KK', []);
            }
            else if (fieldName == "btndetail") {
                var grid = Ext.getCmp(this.winId + 'EMTASKPLANDETAILGrid'); //要加载数据的表名字 + Grid
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length != 1) {
                    Ext.Msg.alert("系统提示", "请选择一条数据查看明细");
                }                
                else {
                    var billno = records[0].data["BILLNO"];
                    var rowid = records[0].data["ROW_ID"];
                    var ishaveDetial = this.invorkBcf("IsHaveDetial", [billno, rowid]);
                    if (ishaveDetial == true) {
                        Ax.utils.LibVclSystemUtils.openDataFunc("Em.TaskPlanDetail", "明细查看", [this]);
                    }
                    else {
                        Ext.Msg.alert("系统提示","当前选择数据不存在明细");
                    }
                }
            }
            else if (fieldName == "btnrelationbillno") { 
                if (this.isEdit) {
                    Ext.Msg.alert("系统提示", "请先保存当前单据，并确认生效再进行此操作");
                }
                else {
                    var currentstate = this.dataSet.getTable(0).data.items[0].data["CURRENTSTATE"];
                    var billno = this.dataSet.getTable(0).data.items[0].data["BILLNO"];
                    if (currentstate == 2) {
                        var returnbillno = this.invorkBcf("ProduceOverHaulPlan", [billno]);//返回的检修计划单单号
                        if (!Ext.isEmpty(returnbillno)) {
                            if (returnbillno != "error") {
                                Ext.Msg.alert("系统提示", "已经成功生成检修计划单，检修计划单号为【" + returnbillno + "】");
                                //生成检修计划单后直接弹出对应的检修计划单
                                //Ax.utils.LibVclSystemUtils.openBill("Em.OverHaulPlan", 1, "检修计划单", BillActionEnum.Modif, this.entryParam, [billno]);
                            }
                            else {
                                Ext.Msg.alert("系统提示", "请先填写基础数据计划检修单单据类型！");
                            }                
                        }
                        else {
                            Ext.Msg.alert("系统提示", "没有可以生成检修计划的数据");
                        }
                    }
                    else {
                        Ext.Msg.alert("系统提示", "请生效当前单据，再进行此操作");
                    }
                }
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.dataRow.get("ISAUTO") == true) {
                    Ext.Msg.alert("系统提示", "系统生成的作业不允许删除！");
                    e.dataInfo.cancel = true;
                }
                else {
                    if (e.dataInfo.dataRow.get("TASKSTATE") > 0) {//作业状态大于0时，表示已开始或已完成
                        Ext.Msg.alert("系统提示", "要删除的作业已经执行，不能删除作业！");
                        e.dataInfo.cancel = true;
                    }
                }
            }
            break;
    }
}

function IsExistSameData(year,month,deptid) {
    if (!Ext.isEmpty(year) && !Ext.isEmpty(month) && !Ext.isEmpty(deptid)) {
        var billno = this.invorkBcf("IsExistSameData", [year, month, deptid]);
        if (!Ext.isEmpty(billno))
            Ext.Msg.alert("系统提示", "已经存在相同年份、月份、管理部门的设备作业计划单【" + billno + "】");
    }
}