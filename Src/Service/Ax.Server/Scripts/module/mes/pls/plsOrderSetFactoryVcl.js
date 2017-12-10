plsOrderSetFactoryVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsOrderSetFactoryVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsOrderSetFactoryVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    var bodyTable = this.dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            e.dataInfo.cancel = true;
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 0) {
                2
                switch (e.dataInfo.fieldName) {
                    case "FACTORYID":
                        for (var i = 0; i < bodyTable.data.items.length; i++) {
                            bodyTable.data.items[i].set("FACTORYID", e.dataInfo.value);
                            bodyTable.data.items[i].set("FACTORYNAME", e.dataInfo.dataRow.data["FACTORYNAME"]);
                        }
                        break;
                }
            }
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "BtnInsert":
                    var start = this.dataSet.getTable(0).data.items[0].data['STARTDATE'];
                    var end = this.dataSet.getTable(0).data.items[0].data['ENDDATE'];
                   

                    var returnData = this.invorkBcf("GetData", [start, end]);
                    if (returnData.BillNo == "") {
                        Ext.Msg.alert("提示", '查询结果为空！');
                    }
                    else {
                        fillOrderWorkData.call(this, returnData);
                    }
                    break;

                case "BtnConfirm":
                    var arr = [];
                    var dt = this.dataSet.getTable(1);

                    for (var i = 0; i < dt.data.items.length; i++) {
                        if (dt.data.items[i].data["FACTORYID"] != '') {
                            arr.push({ BillNo: dt.data.items[i].data["BILLNO"], FactoryId: dt.data.items[i].data["FACTORYID"] });
                        }
                    }
                    if (arr.length == 0) {
                        Ext.Msg.alert("提示", "没有订单！");
                        break;
                    }
                    var returnData = this.invorkBcf("ConfirmFac", [arr]);
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
    this.deleteAll(1);
    try {
        var masterRow = this.dataSet.getTable(0).data.items[0];
        for (var i = 0; i < returnData.length; i++) {

            var newRow = this.addRow(masterRow, 1);
            newRow.set('BILLNO', returnData[i].BillNo);
            newRow.set('REMARK', returnData[i].Remark);
            newRow.set('FROMBILLNO', returnData[i].FromBillNo);
            newRow.set('PAINTTYPE', returnData[i].PaintType);
            newRow.set('PAINTCOLOR', returnData[i].PaintColor);
            newRow.set('TREESPECIES', returnData[i].TreeSpecies);
            newRow.set('BILLFROMID', returnData[i].BillFromId);
            newRow.set('BILLFROMNAME', returnData[i].BillFromName);
            newRow.set('LASTESTDATE', returnData[i].LastestDate);
            newRow.set('LOTNO', returnData[i].LotNo);
            newRow.set('SINGLEDATE', returnData[i].SingleDate);
        }
       
    }
    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}