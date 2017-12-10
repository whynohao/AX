finCostShareLaborCostVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};

var attId = 0;
var proto = finCostShareLaborCostVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finCostShareLaborCostVcl;
proto.parentRow = [];
proto.subRecords = [];

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var bodyTable = this.dataSet.getTable(1).data.items;
            if (e.dataInfo.fieldName == "BtnLoadData") {
                var workProcessId = masterRow.data["WORKPROCESSID"];
                var billdate = masterRow.data["BILLDATE"];
                if (billdate == 0 || billdate == '' || workProcessId == '')
                    Ext.Msg.alert("系统提示", "均摊工序和单据日期不能为空");
                else {
                    var records = this.invorkBcf('GetDataList', [workProcessId, billdate]);
                    this.fillData(records, this)
                }
            }
            else if (e.dataInfo.fieldName == "BtnLoadReturnData") {
                var totalAmount = masterRow.data["TOTALAMOUNT"];
                var totalTime = masterRow.data["TOTALSHARETIME"];
                var timeType = masterRow.data["TIMETYPE"];
                for (var i = 0; i < bodyTable.length; i++) {
                    var row = bodyTable[i];
                    if (timeType == 0)
                        row.set("SHAREAMONUT", row.data["USEDTIME"] / totalTime * totalAmount);
                    else if (timeType == 1)
                        row.set("SHAREAMONUT", row.data["USEDTIME"] / (totalTime * 60) * totalAmount);
                }
            }
            break;
    }
}
proto.fillData = function (iaExcuteRecords) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'FINCOSTSHARELABORCOSTDETIALGrid');
        var list = iaExcuteRecords;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set("ROW_ID", i + 1);
                newRow.set("ROWNO", i + 1)
                newRow.set("CONTRACTCODE", info.CONTRACTCODE);
                newRow.set("CONTRACTNO", info.CONTRACTNO);
                newRow.set("FACTORYNO", info.FACTORYNO);
                newRow.set("MATERIALID", info.MATERIALID);
                newRow.set("MATERIALNAME", info.MATERIALNAME);
                newRow.set("SPECIFICATION", info.SPECIFICATION);
                newRow.set("NEEDCHECK", info.NEEDCHECK);
                newRow.set("ATTRIBUTEID", info.ATTRIBUTEID);
                newRow.set("ATTRIBUTENAME", info.ATTRIBUTENAME);
                newRow.set("ATTRIBUTECODE", info.ATTRIBUTECODE);
                newRow.set("ATTRIBUTEDESC", info.ATTRIBUTEDESC);
                newRow.set("USEDTIME", info.USEDTIME);
                newRow.set("RECORD", info.RECORD);
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