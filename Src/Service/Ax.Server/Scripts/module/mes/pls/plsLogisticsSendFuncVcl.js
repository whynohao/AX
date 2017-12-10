plsLogisticsSendFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsLogisticsSendFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsLogisticsSendFuncVcl;


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
                var dt = this.dataSet.getTable(1).data.items;
                for (var i = 0; i < dt.length; i++) {
                    dt[i].set('CONFIRMDATE', e.dataInfo.dataRow.data["CONFIRMDATE"]);
                }
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "BtnSelect":
                    var startDate = this.dataSet.getTable(0).data.items[0].data['STARTDATE'];
                    var sendDate = this.dataSet.getTable(0).data.items[0].data['SENDDATE'];
                    var logId = this.dataSet.getTable(0).data.items[0].data['LOGISTICSCOMPANYID'];
                    var returnData = this.invorkBcf("GetData", [startDate, sendDate, logId]);
                    if (returnData.length == 0) {
                        Ext.Msg.alert("提示", '查询结果为空！');
                        this.deleteAll(1);
                    }
                    else {
                        fillLogisticsFuncData.call(this, returnData);
                    }

                    break;
                case "BtnConfirm":
                    var dt = this.dataSet.getTable(1).data.items;
                    var arr = [];
                    for (var i = 0; i < dt.length; i++) {
                        if (dt[i].data["ISCONFIRM"] == true) {
                            //if (dt[i].data["CONFIRMDATE"] == '') {
                            //    Ext.Msg.alert("提示", '请维护明细中所有选中项的【确认日期】！');
                            //    return;
                            //}
                            var changeTimes = dt[i].data["CHANGETIMES"];
                            if (changeTimes > 0) {
                                Ext.Msg.alert("提示", '订单批号【' + dt[i].data["LOTNO"] + '】不能再次调整！');
                                return;
                            }
                            if (dt[i].data["CONFIRMDATE"] != 0 && dt[i].data["CONFIRMDATE"] > dt[i].data["LASTESTDATE"]) {
                                Ext.Msg.alert("提示", '订单批号【' + dt[i].data["LOTNO"] + '】 【确认日期】不能晚于【最迟发货日期】！');
                                return;
                            }
                            var confirmDate = dt[i].data["CONFIRMDATE"];
                            if (confirmDate == "") {
                                confirmDate = dt[i].data["PLANLOGISTICSDATE"];
                            }
                            arr.push({
                                PLANPRODUCEDATE: dt[i].data["PLANPRODUCEDATE"],
                                PLANLOGISTICSDATE: dt[i].data["PLANLOGISTICSDATE"],
                                BILLNO: dt[i].data["BILLNO"],
                                SINGLEDATE: dt[i].data["SINGLEDATE"],
                                LOGISTICSCOMPANYID: dt[i].data["LOGISTICSCOMPANYID"],
                                LOGISTICSCOMPANYNAME: dt[i].data["LOGISTICSCOMPANYNAME"],
                                CUSTOMERID: dt[i].data["CUSTOMERID"],
                                CUSTOMERNAME: dt[i].data["CUSTOMERNAME"],
                                LOTNO: dt[i].data["LOTNO"],
                                TREESPECIES: dt[i].data["TREESPECIES"],
                                PAINTTYPE: dt[i].data["PAINTTYPE"],
                                MATERIALTYPEID1: dt[i].data["MATERIALTYPEID1"],
                                MATERIALTYPEID2: dt[i].data["MATERIALTYPEID2"],
                                MATERIALTYPEID3: dt[i].data["MATERIALTYPEID3"],
                                MATERIALTYPEID4: dt[i].data["MATERIALTYPEID4"],
                                MATERIALTYPEID5: dt[i].data["MATERIALTYPEID5"],
                                MATERIALTYPEID6: dt[i].data["MATERIALTYPEID6"],
                                OTHERTYPEID: dt[i].data["OTHERTYPEID"],
                                QUANTITY: dt[i].data["QUANTITY"],
                                REMARK: dt[i].data["REMARK"],
                                CONFIRMDATE: confirmDate,
                                CHANGETIMES: dt[i].data["CHANGETIMES"]

                            });
                        }
                    }
                    var returnData = this.invorkBcf("SetData", [arr]);
                    if (returnData == "成功") {
                        Ext.Msg.alert("提示", returnData);
                        this.deleteAll(1);
                    }
                    else {
                        Ext.Msg.alert("提示", returnData);
                    }
                    break;
                case "BtnClear":
                    this.deleteAll(1);
                    break;
                case "BtnSelectAll":
                    var dt = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < dt.length; i++) {
                        dt[i].set('ISCONFIRM', true);
                    }
                    break;
                case "BtnSelectNo":
                    var dt = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < dt.length; i++) {
                        dt[i].set('ISCONFIRM', false);
                    }
                    break;
            }
            break;
    }
}

function fillLogisticsFuncData(returnData) {
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
                if (!info.Fix) {
                    var newRow = this.addRow(masterRow, 1);
                    newRow.set('ISCONFIRM', true);
                    newRow.set('ROW_ID', i + 1);
                    newRow.set('ROWNO', i + 1);
                    newRow.set('PLANPRODUCEDATE', info.PLANPRODUCEDATE);
                    newRow.set('PLANLOGISTICSDATE', info.PLANLOGISTICSDATE);
                    newRow.set('BILLNO', info.BILLNO);
                    newRow.set('SINGLEDATE', info.SINGLEDATE);
                    newRow.set('LASTESTDATE', info.LASTESTDATE);
                    newRow.set('LOGISTICSCOMPANYID', info.LOGISTICSCOMPANYID);
                    newRow.set('LOGISTICSCOMPANYNAME', info.LOGISTICSCOMPANYNAME);
                    newRow.set('CUSTOMERID', info.CUSTOMERID);
                    newRow.set('CUSTOMERNAME', info.CUSTOMERNAME);
                    newRow.set('BILLFROMID', info.BILLFROMID);
                    newRow.set('BILLFROMNAME', info.BILLFROMNAME);
                    newRow.set('LOTNO', info.LOTNO);
                    newRow.set('TREESPECIES', info.TREESPECIES);
                    newRow.set('PAINTTYPE', info.PAINTTYPE);
                    newRow.set('MATERIALTYPEID1', info.MATERIALTYPEID1);
                    newRow.set('MATERIALTYPEID2', info.MATERIALTYPEID2);
                    newRow.set('MATERIALTYPEID3', info.MATERIALTYPEID3);
                    newRow.set('MATERIALTYPEID4', info.MATERIALTYPEID4);
                    newRow.set('MATERIALTYPEID5', info.MATERIALTYPEID5);
                    newRow.set('MATERIALTYPEID6', info.MATERIALTYPEID6);
                    newRow.set('OTHERTYPEID', info.OTHERTYPEID);
                    newRow.set('QUANTITY', info.QUANTITY);
                    newRow.set('REMARK', info.REMARK);
                    newRow.set('CHANGETIMES', info.CHANGETIMES);
                }
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