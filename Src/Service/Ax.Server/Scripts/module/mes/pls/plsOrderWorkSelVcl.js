plsOrderWorkSelVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsOrderWorkSelVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsOrderWorkSelVcl;


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
                case "BtnSelect":
                    var billNo = this.dataSet.getTable(0).data.items[0].data['BILLNO'];
                    if (billNo == '') {
                        break;
                    }
                    var returnData = this.invorkBcf("GetData", [billNo]);
                    if (returnData.length == 0) {
                        Ext.Msg.alert("提示", '查询结果为空！');
                        this.deleteAll(1);
                    }
                    else {
                        fillOrderWorkData.call(this, returnData);
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
        this.deleteAll(1);
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set('BILLNO', info.WorkOrderNo);
                newRow.set('STARTDATE', info.StartDate);
                newRow.set('ENDDATE', info.EndDate);
                newRow.set('WORKINDEX', info.WorkIndex);
                newRow.set('PRODUCELINEID', info.ProduceLineId);
                newRow.set('PRODUCELINENAME', info.ProduceLineName);
                newRow.set('ORDERSTATE', info.OrderState);
                newRow.set('SINGLEDATE', info.SingleDate);
                newRow.set('LASTESTDATE', info.LastestDate);
                newRow.set('LOGISTICSCOMPANYID', info.LogisticsCompanyId);
                newRow.set('LOGISTICSCOMPANYNAME', info.LogisticsCompanyName);
                newRow.set('CUSTOMERID', info.CustomerId);
                newRow.set('CUSTOMERNAME', info.CustomerName);
                newRow.set('BILLFROMID', info.BillFromId);
                newRow.set('BILLFROMNAME', info.BillFromName);
                newRow.set('TREESPECIES', info.Treespecies);
                newRow.set('PAINTTYPE', info.PaintType);
                newRow.set('MATERIALTYPEID1', info.Quan311);
                newRow.set('MATERIALTYPEID2', info.Quan312);
                newRow.set('MATERIALTYPEID3', info.Quan316);
                newRow.set('MATERIALTYPEID4', info.Quan314);
                newRow.set('MATERIALTYPEID5', info.Quan31501);
                newRow.set('MATERIALTYPEID6', info.Quan31502);
                newRow.set('OTHERTYPEID', info.QuanOther);
                newRow.set('REMARK', info.ReMark);
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