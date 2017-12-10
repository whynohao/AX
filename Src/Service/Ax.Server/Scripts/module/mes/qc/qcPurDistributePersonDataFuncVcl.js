qcPurDistributePersonDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};

var proto = qcPurDistributePersonDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = qcPurDistributePersonDataFuncVcl;
proto.winId = null;
proto.fromObj = null;
proto.getType = 0;

proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj;
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
        if (e.dataInfo.fieldName == "Countersign") {            //点击确定按钮触发
            var billNo = [];
            var rowId = [];
            for (var i = 4; i < proto.fromObj[2].length; i+=2) {
                billNo.push(proto.fromObj[2][i]);
                rowId.push(proto.fromObj[2][i + 1]);
            }
            var grid = Ext.getCmp(this.winId + 'PURDSPSDFCDETAILGrid');
            var personId = [];
            var personName = [];
            for (var i = 0; i < grid.items.items[0].dataSource.data.items.length; i++)
            {
                if (grid.items.items[0].dataSource.data.items[i].data["ISSELECT"])
                {
                    personId.push(grid.items.items[0].dataSource.data.items[i].data["PERSONID"]);
                }
            }
            if (personId.length != 0) {
                returnData = this.invorkBcf("PostData", [billNo, rowId, personId]);
                var list = [];
                for (var i = 0; i < 4; i++) {
                    list.push(proto.fromObj[2][i]);
                }
                returnData = this.invorkBcf("FillData", [list]);
                this.refreshData.call(this, returnData);
                this.win.close();
            } else { alert("还未分配人员,请选择人员进行分配!") }
        }
    }
    if (e.libEventType == LibEventTypeEnum.Validated)
    {
        if (e.dataInfo.fieldName == "DEPTID") {
            var deptId = this.dataSet.getTable(0).data.items[0].data["DEPTID"];
            returnData = this.invorkBcf("GainData", [deptId]);
            this.fillData.call(this, returnData);
        }
    }
}

proto.fillData = function (returnData) {             //载入查询的所有数据，载入到datafunc上
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'PURDSPSDFCDETAILGrid');
        //var list = returnData['stockoutdetail'];
        var list = returnData;
        //var masterRow = this.dataSet.getTable(0).data.items[0];
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set('PERSONID', info.PersonId);
                newRow.set('PERSONNAME', info.PersonName);
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

proto.refreshData = function (returnData) {             //载入查询的所有数据，载入到datafunc上
    Ext.suspendLayouts();//关闭Ext布局
    var grid = Ext.getCmp(proto.winId + 'PURDSTASKDFCDETAILGrid');
    var curStore = proto.fromObj[0].dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        proto.fromObj[0].deleteAll(1);//删除当前grid的数据
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
                newRow.set('SUBBATCHNO', info.SubBatchNo);
                newRow.set('ATTRIBUTECODE', info.AttributeCode);
                newRow.set('ATTRIBUTEDESC', info.AttributeDesc);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('CHECKTYPE', info.CheckType);
                newRow.set('QUANTITY', info.Quantity);
                newRow.set('CHECKNUM', info.Quantity);
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