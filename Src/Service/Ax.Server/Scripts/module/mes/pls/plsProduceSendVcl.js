plsProduceSendVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsProduceSendVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsProduceSendVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var typeId = masterRow.get('FROMTYPEID');
        var workShopSectionId = masterRow.get('WORKSHOPSECTIONID');
        var planDate = masterRow.get('PLANDATE');
        var produceLineId = masterRow.get("PRODUCELINEID");
        var bodyTable = this.dataSet.getTable(1);
        if (e.dataInfo.fieldName == "btnLoad") {
            if (typeId == '') {
                Ext.Msg.alert('月计划单据类型不能为空。');
            }
            else {
                Ext.suspendLayouts();
                bodyTable.suspendEvents();
                var data = this.invorkBcf('GetMonthPlanData', [typeId, planDate,produceLineId, workShopSectionId]);
                bodyTable.removeAll();
                try {
                    for (var i = 0; i < data.length; i++) {
                        var newRow = this.addRow(masterRow, 1);
                        newRow.set('ROW_ID', i + 1);
                        newRow.set('ROWNO', i + 1);
                        newRow.set('FROMTYPEID', data[i]["FROMTYPEID"]);
                        newRow.set('FROMBILLNO', data[i]["FROMBILLNO"]);
                        newRow.set('FROMROWID', data[i]["FROMROWID"]);
                        newRow.set('WORKORDERBILLNO', data[i]["WORKORDERBILLNO"]);
                        newRow.set('MATERIALID', data[i]["MATERIALID"]);
                        newRow.set('MATERIALNAME', data[i]["MATERIALNAME"]);
                        newRow.set('MATERIALSPEC', data[i]["MATERIALSPEC"]);
                        newRow.set('UNITID', data[i]["UNITID"]);
                        newRow.set('UNITNAME', data[i]["UNITNAME"]);
                        newRow.set('QUANTITY', data[i]["QUANTITY"]);
                        newRow.set('STARTTIME', data[i]["STARTTIME"]);
                        newRow.set('ENDTIME', data[i]["ENDTIME"]);
                        newRow.set('WORKORDER', data[i]["WORKORDER"]);
                        newRow.set('ISMIXED', data[i]["ISMIXED"]);
                        newRow.set('WORKSHOPSECTIONID', data[i]["WORKSHOPSECTIONID"]);
                        newRow.set('WORKSHOPSECTIONNAME', data[i]["WORKSHOPSECTIONNAME"]);
                        newRow.set("PRODUCELINEID", data[i]["PRODUCELINEID"]);
                        newRow.set("PRODUCELINENAME", data[i]["PRODUCELINENAME"]);
                    }
                } finally {
                    bodyTable.resumeEvents();
                    if (bodyTable.ownGrid && bodyTable.ownGrid.getView().store != null)
                        bodyTable.ownGrid.reconfigure(bodyTable);
                    Ext.resumeLayouts(true);
                }
            }
        } else if (e.dataInfo.fieldName == "btnBuild") {
            if (bodyTable.data.items.length > 0) {
                var bulidInfo = {};
                var curStore = this.dataSet.getTable(1);
                bulidInfo.PlanDate = planDate;
                bulidInfo.TypeId = typeId;
                bulidInfo.Data = {};
                for (var i = 0; i < bodyTable.data.items.length; i++) {
                    var record = bodyTable.data.items[i];
                    var workshopSectionId = record.get('WORKSHOPSECTIONID');
                    var produceLineId = record.get("PRODUCELINEID");
                    var workshopSectionName = record.get("WORKSHOPSECTIONNAME");
                    var produceLineName = record.get("PRODUCELINENAME");
                    var key = produceLineId + "/t" + workshopSectionId + "/t" + produceLineName + "/t" + workshopSectionName;
                    if (bulidInfo.Data[key] === undefined) {
                        bulidInfo.Data[key] = {};
                    }
                    var info = bulidInfo.Data[key];
                    var workOrderNo = record.get('WORKORDERBILLNO');
                    var workIndex = record.get("WORKORDER");
                    if (record.get('STARTTIME') > record.get('ENDTIME')) {
                        alert("计划结束时间小于计划开始时间，请调整！");
                        return;
                    }
                    info[workOrderNo] = {
                        WorkOrder: workIndex,
                        WorkOrderBillNo: workOrderNo,
                        FromBillNo: record.get('FROMBILLNO'),
                        ProducelineId: record.get('PRODUCELINEID'),
                        FromRowId: record.get('FROMROWID'),
                        MaterialId: record.get('MATERIALID'),
                        StartTime: record.get('STARTTIME'),
                        EndTime: record.get('ENDTIME')
                    };
                }
                this.invorkBcf('BuildPlan', [bulidInfo]);
            }
        }
    }
}