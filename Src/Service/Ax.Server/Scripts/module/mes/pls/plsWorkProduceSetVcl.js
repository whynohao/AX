plsWorkProduceSetVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsWorkProduceSetVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsWorkProduceSetVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    var bodyTable = this.dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            e.dataInfo.cancel = true;
            break;


        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
            }

            break;
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "BtnSelect":
                    var producelineId = masterRow.data['PRODUCELINEID'];
                    var producelineName = masterRow.data['PRODUCELINENAME'];
                    var planProduceDate = masterRow.data['PLANPRODUCEDATE'];

                    if (planProduceDate == '') {
                        Ext.Msg.alert("提示", '请维护计划生产日期！');
                        break;
                    }
                    var returnData = this.invorkBcf("GetData", [producelineId, producelineName, planProduceDate]);
                    if (returnData.length == 0) {
                        Ext.Msg.alert("提示", '查询结果为空！');
                    }
                    fillWorkOrderSetData.call(this, returnData);
                    break;

                case "BtnReset":
                    this.deleteAll(1);
                    break;
                case "BtnSelectAll":
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        bodyTable.data.items[i].set("CHANGE", true);
                    }
                    break;

                case "BtnHandel":
                    var count = [];
                    var PlanDate = 0;
                    var endDate = 0;
                    var Producedate = masterRow.data['PRODUCEDATE'];
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        if (bodyTable.data.items[i].data["CHANGE"] == true) {
                            count.push({
                                BillNo: bodyTable.data.items[i].data["BILLNO"],
                                ProAdvanceDays: bodyTable.data.items[i].data["PROADVANCEDAYS"]
                            });

                        }
                    }
                    //if (PlanDate >= Producedate) {
                    //    Ext.Msg.alert("提示", "【计划生产日期】必须大于【计划开始日期】");
                    //    break;
                    //}
                    if (count.length == 0) {
                        Ext.Msg.alert("提示", '没有需要调整的作业单！');
                        break;
                    }

                    var returnData = this.invorkBcf("SetData", [Producedate, count]);
                    if (returnData == "成功") {
                        Ext.Msg.alert("提示", returnData);
                        this.deleteAll(1);
                    }
                    else {
                        Ext.Msg.alert("提示", returnData);
                    }

                    break;

                case "BtnCancel":
                    var count = [];
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        if (bodyTable.data.items[i].data["CHANGE"] == true) {
                            count.push(bodyTable.data.items[i].data["BILLNO"])
                        }
                    }
                    if (count.length == 0) {
                        Ext.Msg.alert("提示", '没有需要取消的作业单！');
                        break;
                    }
                    var returnData = this.invorkBcf("CancelData", [count]);
                    if (returnData == "成功") {
                        Ext.Msg.alert("提示", returnData);
                        this.deleteAll(1);
                    }
                    else {
                        Ext.Msg.alert("提示", returnData);
                    }

                    break;
            }
            break;
    }
}

function fillWorkOrderSetData(returnData) {
    Ext.suspendLayouts();
    var formStore = this.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        this.deleteAll(1);
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set('PRODUCELINEID', info.ProducelineId);
                newRow.set('PRODUCELINENAME', info.ProducelineName);
                newRow.set('WORKINDEX', info.WorkIndex);
                newRow.set('BILLNO', info.BillNo);
                newRow.set('STARTDATE', info.StartDate);
                newRow.set('ENDDATE', info.EndDate);
                newRow.set('PROADVANCEDAYS', info.ProAdvanceDays);
            }
        }
    }
    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}