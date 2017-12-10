EmAssignPersonLeaderVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = EmAssignPersonLeaderVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = EmAssignPersonLeaderVcl;

var btnSelect = 0;//记录全选还是不选

//界面加载
proto.doSetParam = function () {
    var returnList = this.invorkBcf("GetRepairListInfo", []);
    FillRepairData.call(this, returnList);
};

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        //全选按钮事件
        if (e.dataInfo.fieldName == "btnAssignTask") {
            var allItems = this.dataSet.getTable(1).data.items;
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var checkItems = [];
            var errorMessage = "";
            var flag = true;
            if (allItems.length > 0) {
                for (var i = 0; i < allItems.length; i++) {
                    if (allItems[i].data["ISCHOSE"] == true) {
                        if (allItems[i].data["PERSONLEADER"] != "") {
                            checkItems.push({
                                BillNo: allItems[i].data["REPAIRBILLNO"],
                                PersonLeader: allItems[i].data["PERSONLEADER"]
                            });
                        }
                        else {
                            errorMessage = "第" + allItems[i].data["ROWNO"] + "行，" + "维修主管不能为空！";
                            flag = false;
                            break;
                        }
                    }
                }
                if (flag) {
                    var mark = this.invorkBcf("AssignTask", [checkItems, masterRow.data["PERSONLEADER"]]);
                    if (mark) {
                        Ext.Msg.alert("系统提示", "指派成功");
                    }
                    else {
                        Ext.Msg.alert("系统提示", "指派失败");
                    }
                }
                else {
                    Ext.Msg.alert("系统提示", errorMessage);
                }
            }
            else
                Ext.Msg.alert("系统提示", "请选择需要修改的行");
        }
        else if (e.dataInfo.fieldName == "btnSelectAll") {
            var allItems = this.dataSet.getTable(1).data.items;
            if (btnSelect == 0) {
                for (var i = 0; i < allItems.length; i++) {
                    allItems[i].set("ISCHOSE", true);
                }
                btnSelect = 1;
            }
            else {
                for (var i = 0; i < allItems.length; i++) {
                    allItems[i].set("ISCHOSE", false);
                }
                btnSelect = 0;
            }
        }
    }
    else if (e.libEventType == LibEventTypeEnum.Validated) {
        var fieldName = e.dataInfo.fieldName;
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var allItems = this.dataSet.getTable(1).data.items;
        if (e.dataInfo.tableIndex == 0) {
            if (fieldName == "PERSONLEADER") {
                var count = 0;
                for (var i = 0; i < allItems.length; i++) {
                    if (allItems[i].data["ISCHOSE"] == true) {
                        count++;
                    }
                }
                if (count > 0) {
                    for (var i = 0; i < allItems.length; i++) {
                        if (allItems[i].data["ISCHOSE"] == true) {
                            allItems[i].set("PERSONLEADER", masterRow.data["PERSONLEADER"]);
                            allItems[i].set("PERSONLEADERNAME", masterRow.data["PERSONLEADERNAME"]);
                        }
                    }
                }
                else {
                    masterRow.data["PERSONLEADER"] = "";
                    this.forms[0].loadRecord(masterRow);
                    Ext.Msg.alert("系统提示", "请先选择要指派的报修单");
                }
            }
        }
    }
}

function FillRepairData(returnList) {
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
                newRow.set("REPAIRBILLNO", info.BillNo);
                newRow.set("ROWNO", i + 1);
                newRow.set("REPAIRDATE", info.RepairDate);
                newRow.set("REPAIRPERSONID", info.RepairPersonId);
                newRow.set("REPAIRPERSONNAME", info.RepairPersonName);
                newRow.set("DEPTID", info.DeptId);
                newRow.set("DEPTNAME", info.DeptName);
                newRow.set("EQUIPMENTID", info.EquipmentId);
                newRow.set("EQUIPMENTNAME", info.EquipmentName);
                newRow.set("EQUIPMENTMODEL", info.EquipmentModel);
                newRow.set("EMFAULTID", info.EmFaultId);
                newRow.set("EMFAULTNAME", info.EmFaultName);
                newRow.set("PERSONLEADER", info.PersonLeader);
                newRow.set("PERSONLEADERNAME", info.PersonLeaderName);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}