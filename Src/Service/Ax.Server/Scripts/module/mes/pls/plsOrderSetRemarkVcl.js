plsOrderSetRemarkVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsOrderSetRemarkVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsOrderSetRemarkVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    var bodyTable = this.dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            e.dataInfo.cancel = true;
            break;

        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "BtnInsert":
                    var BillNo = this.dataSet.getTable(0).data.items[0].data['BILLNO'];
                    var dt = this.dataSet.getTable(1);
                    var isExist = false;
                    if (BillNo == '' || BillNo == null) {
                        Ext.Msg.alert("提示", '请选择订单！');
                        break;
                    }
                    for (var i = 0; i < dt.data.items.length; i++) {
                        if (BillNo == dt.data.items[i].data["BILLNO"]) {
                            isExist = true;
                            break;
                        }
                    }

                    if (isExist) {
                        Ext.Msg.alert("提示", '该订单已存在明细中！');
                        break;
                    }

                    var returnData = this.invorkBcf("GetData", [BillNo]);
                    if (returnData.BillNo == "") {
                        Ext.Msg.alert("提示", '查询结果为空！');
                    }
                    else {
                        fillOrderWorkData.call(this, returnData);
                    }
                    break;
                case "BtnClear":
                    this.deleteAll(1);
                    break;
                case "BtnConFirm":
                    var arr = [];
                    var dt = this.dataSet.getTable(1);

                    for (var i = 0; i < dt.data.items.length; i++) {
                        if (dt.data.items[i].data["ADDREMARK"].length == '') {
                            Ext.Msg.alert("提示", "请维护【添加备注】！");
                            return;
                        }
                        arr.push({ BillNo: dt.data.items[i].data["BILLNO"], Remark: dt.data.items[i].data["REMARK"], AddRemark: dt.data.items[i].data["ADDREMARK"] });
                    }
                    if (arr.length == 0) {
                        Ext.Msg.alert("提示", "没有订单！");
                        break;
                    }
                    
                    var returnData = this.invorkBcf("ConfirmChange", [arr]);
                    if (returnData.indexOf("成功") > -1) {
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

function fillOrderWorkData(returnData) {
    Ext.suspendLayouts();
    var formStore = this.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var newRow = this.addRow(masterRow, 1);
        newRow.set('BILLNO', returnData.BillNo);
        newRow.set('STARTDATE', returnData.StartDate);
        newRow.set('WORKINDEX', returnData.WorkIndex);
        newRow.set('PRODUCELINEID', returnData.ProduceLineId);
        newRow.set('PRODUCELINENAME', returnData.ProduceLineName);
        newRow.set('FROMBILLNO', returnData.FromBillNo);
        newRow.set('WORKORDERNO', returnData.WorkOrderNo);
        newRow.set('REMARK', returnData.Remark);
        newRow.set('LOTNO', returnData.LotNo);
        newRow.set('SINGLEDATE', returnData.SingleDate);
    }
    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}