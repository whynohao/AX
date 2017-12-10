stkProductStorageAssignVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
    this.dataRow;
};
var proto = stkProductStorageAssignVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkProductStorageAssignVcl;
//界面加载
proto.doSetParam = function () {
    var returnList = this.invorkBcf("GetProductStorageData", [0]);
    FillMaintainData.call(this, returnList);
};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "btnSelect") {
            var masterRow = this.dataSet.getTable(0).data.items[0];
            this.forms[0].updateRecord(masterRow);
            var returnList = this.invorkBcf("GetProductStorageData", [masterRow.data["TASKTYPE"]]);
            FillMaintainData.call(this, returnList);
        }
        else if (e.dataInfo.fieldName == "btnAssignTaskToPerson") {
            var allItems = this.dataSet.getTable(1).data.items;
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var taskType = masterRow.data["TASKTYPE"];
            var checkItems = [];
            if (allItems.length > 0) {
                for (var i = 0; i < allItems.length; i++) {
                    if (allItems[i].data["ISCHOSE"] == true) {
                        checkItems.push({
                            TaskNo: allItems[i].data["TASKNO"],
                            WorkNo: allItems[i].data["WORKNO"],
                            PPWorkOrder: allItems[i].data["PPWORKORDER"],
                            WorkShopSectionId: allItems[i].data["WORKSHOPSECTIONID"],
                            WorkShopSectionName: allItems[i].data["WORKSHOPSECTIONNAME"],
                            MaterialId: allItems[i].data["MATERIALID"],
                            MaterialName: allItems[i].data["MATERIALNAME"],
                            ProductId: allItems[i].data["PRODUCTID"],
                            ProductName: allItems[i].data["PRODUCTNAME"],
                            AttributeCode: allItems[i].data["ATTRIBUTECODE"],
                            AttributeDesc: allItems[i].data["ATTRIBUTEDESC"],
                            PlanEndTime: allItems[i].data["PLANENDTIME"],
                            StartTime: allItems[i].data["STARTTIME"],
                            EndTime: allItems[i].data["ENDTIME"],
                            PersonId: allItems[i].data["PERSONID"],
                            PersonName: allItems[i].data["PERSONNAME"],
                          
                        });
                    }
                }
            }
            var result = this.invorkBcf("AssignTasktoPerson", [taskType, checkItems]);
            if (result == "true") {
                alert("任务指派成功");
            }
        }
    }

}

function FillMaintainData(returnList) {
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
                newRow.set("TASKNO", info.TaskNo);
                newRow.set("WORKNO", info.WorkNo);
                newRow.set("PPWORKORDER", info.PPWorkOrder);
                newRow.set("WORKSHOPSECTIONID", info.WorkShopSectionId);
                newRow.set("WORKSHOPSECTIONNAME", info.WorkShopSectionName);
                newRow.set("MATERIALID", info.MaterialId);
                newRow.set("MATERIALNAME", info.MaterialName);
                newRow.set("PRODUCTID", info.ProductId);
                newRow.set("PRODUCTNAME", info.ProductName);
                newRow.set("ATTRIBUTECODE", info.AttributeCode);
                newRow.set("ATTRIBUTEDESC", info.AttributeDesc);
                newRow.set("PLANENDTIME", info.PlanEndTime);
                newRow.set("STARTTIME", info.StartTime);
                newRow.set("ENDTIME", info.EndTime);
                newRow.set("PERSONID", info.PersonId);
                newRow.set("PERSONNAME", info.PersonName);
           
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}