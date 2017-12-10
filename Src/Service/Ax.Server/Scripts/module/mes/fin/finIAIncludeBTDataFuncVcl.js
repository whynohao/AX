finIAIncludeBTDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = finIAIncludeBTDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = finIAIncludeBTDataFuncVcl;

proto.doSetParam = function (vclObj) {
    var returnList = this.invorkBcf("GetData");
    this.FillDataFunc(returnList);
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    var bodyTable = this.dataSet.getTable(1);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            //不允许手工添加行
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            //不允许手工删除行
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "Confirm":
                    var list = [];
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        list.push({
                            BillTypeId: bodyTable.data.items[i].data["BILLTYPEID"],
                            IsAccount: bodyTable.data.items[i].data["ISACCOUNT"],
                        });
                    }
                    if (this.invorkBcf("SaveData", [list])) {
                        Ext.Msg.alert("提示", '保存成功！');
                    }
                    else {
                        Ext.Msg.alert("提示", '保存失败！');
                    }
                    break;
            }
            break;
    }
}

proto.FillDataFunc= function(returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'FINIAIBTDATAFUNCDETAILGrid');
        var list = returnList;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set("BILLTYPEID", info.BillTypeId);
                newRow.set("ISACCOUNT", info.IsAccount);
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