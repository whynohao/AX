plsOrderFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsOrderFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsOrderFuncVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    var bodyTable = this.dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
        case LibEventTypeEnum.BeforeDeleteRow:
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
                    var startDate = this.dataSet.getTable(0).data.items[0].data['STARTDATE'];
                    var endDate = this.dataSet.getTable(0).data.items[0].data['ENDDATE'];
                    var Billno = this.dataSet.getTable(0).data.items[0].data['BILLNO'];
                    //if (startDate == '') {
                    //    Ext.Msg.alert("提示", '请维护开始日期！');
                    //    break;
                    //}
                    //if (endDate == '') {
                    //    Ext.Msg.alert("提示", '请维护结束日期！');
                    //    break;
                    //}
                    if (startDate > endDate) {
                        Ext.Msg.alert("提示", '开始日期不能大于结束日期！');
                        break;
                    }
                    var returnData = this.invorkBcf("GetData", [startDate, endDate, Billno]);
                    if (returnData.length ==0) {
                        Ext.Msg.alert("提示", '查询结果为空！');
                        this.deleteAll(1);
                        this.deleteAll(2);
                    }
                    else {
                        fillOrderFuncData.call(this, returnData);
                    }
                  
                    break;
            }
            break;
    }
}

function fillOrderFuncData(returnData) {
    Ext.suspendLayouts();
    var formStore = this.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        this.deleteAll(1);
        this.deleteAll(2);
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var rowId = 1;
                if (!info.Fix) {
                    var newRow = this.addRow(masterRow, 1);
                    newRow.set('BILLNO', info.BillNo);
                    newRow.set('ROW_ID', rowId);
                    newRow.set('SINGLEDATE', info.SingleDate);
                    newRow.set('LOTNO', info.LotNo);
                    newRow.set('LASTESTDATE', info.LastestDate);
                    newRow.set('PAINTTYPE', info.PaintTypeId);
                    newRow.set('PAINTCOLOR', info.PaintColor);
                    newRow.set('TREESPECIES', info.TreeSpecies);
                    newRow.set('BILLFROMNAME', info.BillFromName);
                    newRow.set('BILLFROMID', info.BillFromId);
                    if (list[i].remarkList !== undefined && list[i].remarkList.length > 0) {
                        newRow.set('REMARKDETAILSUB', true);
                        for (var j = 0; j < list[i].remarkList.length; j++) {
                            var remarkDetail = list[i].remarkList[j];
                            var remarkRow = this.addRow(newRow, 2);
                            remarkRow.set('BILLNO', info.BillNo);
                            remarkRow.set('PARENTROWID', rowId);
                            remarkRow.set('REMARK', remarkDetail);
                        }
                    }
                    if (list[i].Detail !== undefined && list[i].Detail.length > 0) {
                        newRow.set('SALESORDERDETAILSUB', true);
                        for (var j = 0; j < list[i].Detail.length; j++) {
                            var info = list[i].Detail[j];
                            var childRow = this.addRow(newRow, 3);
                            childRow.set('BILLNO', info.BillNo);
                            childRow.set('ROW_ID', info.RowId);
                            childRow.set('GROUPNO', info.GroupNo);
                            childRow.set('MATERIALID', info.MaterialId);
                            childRow.set('MATERIALNAME', info.MaterialName);
                            childRow.set('MATERIALTYPEID', info.MaterialTypeId);
                            childRow.set('MATERIALTYPENAME', info.MaterialTypeName);
                            childRow.set('ATTRIBUTECODE', info.AttributeCode);
                            childRow.set('ATTRIBUTEDESC', info.AttributeDesc);
                            childRow.set('QUANTITY', info.Quantity);
                        }
                    }
                }
                rowId++;
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