comJudgeModuleVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = comJudgeModuleVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comJudgeModuleVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.fieldName == "MODULETYPEID") {
                var moduleTypeId = Ext.getCmp('MODULETYPEID0_' + this.winId).value;
                var returnData = this.invorkBcf("getModuleTypeDetial", [moduleTypeId]);
                fillData.call(this, returnData);
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1)
                e.dataInfo.cancel = true;
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1)
                e.dataInfo.cancel = true;
            break;
    }
}

function fillData(returnData) {
    Ext.suspendLayouts();//关闭ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        //this.deleteAll(1);//删除当前grid的数据
        this.dataSet.getTable(1).removeAll(); //删除要加载的Grid数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var datelist = returnData;//一般是中间层返回来的数据，中间可能定义的是dictionary,在前段反序列化之后是对象
        if (datelist != undefined && datelist.length > 0) {
            for (var i = 0; i < datelist.length; i++) {
                var info = datelist[i];       //RollingPlanDate
                var newRow = this.addRow(masterRow, 1);
                newRow.set('GRADEID', info.GradeId);
                newRow.set("GRADENAME", info.GradeName);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开ext布局
    }
}