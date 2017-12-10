finIAFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = finIAFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = finIAFuncVcl;


proto.doSetParam = function () {
    //设定会计年默认是当前年
    var headerRow = this.dataSet.getTable(0).data.items[0];
    var date = new Date();
    var accountYear = date.getFullYear();
    headerRow.set("ACCOUNTYEAR", accountYear);
    this.forms[0].loadRecord(headerRow);

    var iaExcuteRecords = this.invorkBcf("GetIaExcuteRecords", [false, accountYear]);
    this.fillIaExecuteRecordGrid.call(this, iaExcuteRecords);
}

proto.fillIaExecuteRecordGrid = function (iaExcuteRecords) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'FINIAFUNCDETAILGrid');
        var list = iaExcuteRecords;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set("ROW_ID", i + 1);
                newRow.set("ROWNO", i + 1)
                newRow.set("ACCOUNTYEAR", info.AccountYear);
                newRow.set("ACCOUNTMONTH", info.AccountMonth);
                newRow.set("DATEFROM", info.DateFrom);
                newRow.set("DATETO", info.DateTo);
                newRow.set("ISIA", info.IsIa);

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
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "IACHECK") {
                var grid = Ext.getCmp(this.winId + "FINIAFUNCDETAIL" + "Grid");
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length > 0) {
                    var record = records[0];
                    var obj = { AccountYear: record.get("ACCOUNTYEAR"), AccountMonth: record.get("ACCOUNTMONTH"), DateFrom: record.get("DATEFROM"), DateTo: record.get("DATETO"), IsIa: record.get("ISIA") }
                    var retObj = this.invorkBcf("IACheck", [obj]);
                    if (retObj.IsOk) { Ext.Msg.alert("提示", "核算检查没有问题，请点击存货核算！"); }
                    else {
                        Ext.Msg.alert("提示", retObj.Reason);
                    }
                }
                else {
                    Ext.Msg.alert("提示", "请选择一条数据！");
                }
            }
            else if (e.dataInfo.fieldName == "IADO") {
                var grid = Ext.getCmp(this.winId + "FINIAFUNCDETAIL" + "Grid");
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length > 0) {
                    var record = records[0];
                    var obj = { AccountYear: record.get("ACCOUNTYEAR"), AccountMonth: record.get("ACCOUNTMONTH"), DateFrom: record.get("DATEFROM"), DateTo: record.get("DATETO"), IsIa: record.get("ISIA") }
                    var retObj = this.invorkBcf("IADo", [obj]);
                    if (!retObj.IsOk) {
                        Ext.Msg.alert("提示", retObj.Reason);
                    }
                    else {
                        record.set("ISIA", 1);
                        Ext.Msg.alert("提示", "存货核算成功！");

                    }
                }
                else {
                    Ext.Msg.alert("提示", "请选择一条数据！");
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "ACCOUNTYEAR") {
                    var iaExcuteRecords = this.invorkBcf("GetIaExcuteRecords", [false, e.dataInfo.value]);
                    this.fillIaExecuteRecordGrid.call(this, iaExcuteRecords);
                }
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            //不允许手工添加行
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
    }
}