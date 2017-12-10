qcPurDistributeTaskDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};

var proto = qcPurDistributeTaskDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = qcPurDistributeTaskDataFuncVcl;
proto.winId = null;
proto.fromObj = null;
proto.getType = 0;
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj;
    var returnlist = proto.fromObj[2];
    this.fillData.call(this, returnlist);
};



proto.vclHandler = function (sender, e) {
    
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }
    if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        var returnData;
        if (e.dataInfo.fieldName == "Select") {            //点击查询按钮触发
            var list = [];
            list.push(this.dataSet.getTable(0).data.items[0].data["PURCHASEORDER"]);
            list.push(this.dataSet.getTable(0).data.items[0].data["SUPPLIERID"]);
            list.push(this.dataSet.getTable(0).data.items[0].data["BILLDATE"]);
            list.push(this.dataSet.getTable(0).data.items[0].data["MATERIALID"]);
            returnData = this.invorkBcf("GetData", [list]);
            this.fillData.call(this, returnData);
        }
        if (e.dataInfo.fieldName == "Distribute") {            //点击分发按钮触发
            var grid = Ext.getCmp(this.winId + 'PURDSTASKDFCDETAILGrid');
            var records = grid.getView().getSelectionModel().getSelection();
            if (records.length == 0) {
                alert("请选择要分发的明细！");
            }
            else {
                var paramList = [];
                paramList.push(this.dataSet.getTable(0).data.items[0].data["PURCHASEORDER"]);
                paramList.push(this.dataSet.getTable(0).data.items[0].data["SUPPLIERID"]);
                paramList.push(this.dataSet.getTable(0).data.items[0].data["BILLDATE"]);
                paramList.push(this.dataSet.getTable(0).data.items[0].data["MATERIALID"]);
                for(var i=0;i<records.length;i++){
                    var billNo = records[i].data["BILLNO"];
                    var rowId = records[i].data["ROW_ID"];                    
                    paramList.push(billNo);
                    paramList.push(rowId);
                }
                gridName = "PURDSTASKDFCDETAIL";
                Ax.utils.LibVclSystemUtils.openDataFunc("qc.PurDistributePersonDataFunc", "质检单分发人员明细", [this, gridName,paramList ]);
            }
        }
    }
}

proto.fillData = function (returnData) {             //载入查询的所有数据，载入到datafunc上
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'PURDSTASKDFCDETAILGrid');
        //var list = returnData['stockoutdetail'];
        var list = returnData;
        //var masterRow = this.dataSet.getTable(0).data.items[0];
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set('BILLNO', info.BillNo);
                newRow.set('ROW_ID', info.Row_Id);
                newRow.set('FROMBILLNO', info.FromBillNo);
                newRow.set('FROMROWID', info.FromRowId);
                newRow.set('BATCHNO', info.BatchNo);
                newRow.set('SUBBATCHNO',info.SubBatchNo);
                newRow.set('ATTRIBUTECODE', info.AttributeCode);
                newRow.set('ATTRIBUTEDESC', info.AttributeDesc);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('CHECKTYPE', info.CheckType);
                newRow.set('QUANTITY', info.Quantity);
                newRow.set('CHECKNUM', info.CheckNum);
            }
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}