comFindCheckSolutionDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = comFindCheckSolutionDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comFindCheckSolutionDataFuncVcl;

proto.winIdFrom = null;
proto.fromFromObj = null;
proto.checkName = null;
//赋值方法
proto.doSetParam = function (vclObj) {
    proto.winIdFrom = vclObj[0].winId;
    proto.fromFromObj = vclObj;
    proto.fromFromObj[0].fromObj = vclObj[1];
    proto.checkName = vclObj[2];
    proto.figureNo = vclObj[3];
    proto.workProcessId = vclObj[4];
    var getData = this.invorkBcf("GetCheckSolution", [proto.checkName, proto.figureNo, proto.workProcessId]);
    this.fillData.call(this,getData);
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);

    if (e.dataInfo && e.dataInfo.tableIndex == 0) {
        //新增行
        //表身不可手工新增
        if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
            e.dataInfo.cancel = true;
            return;
        }
        //删除行
        //表身不可手工删除
        if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
            e.dataInfo.cancel = true;
            return;
        }
    }
    //用户自定义按钮
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        //全选
        if (e.dataInfo.fieldName == "BtnSelectAll") {
            for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                this.dataSet.getTable(1).data.items[i].set("ISCHOSE", 1);
            }
        }
        //全反选
        if (e.dataInfo.fieldName == "BtnSelectNone") {
            for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                this.dataSet.getTable(1).data.items[i].set("ISCHOSE", 0);
            }
        }

        if (e.dataInfo.fieldName == "BtnConfirm") {
            var list = [];
            var recordData;
            //表身行项
            var selectItems = this.dataSet.getTable(0).data.items;

            for (var i = 0; i < selectItems.length; i++) {
                //如果打勾
                if (selectItems[i].data["ISCHOSE"] == true) {
                    list.push(selectItems[i].data["CHECKSTID"]);
                }
            }
            if (list.length > 0) {

                recordData = this.invorkBcf("FindData", [list]);
            }
            //否则执行生成通知单
            if (recordData.length > 0) {

                //proto.fromObj[0].forms[0].updateRecord(proto.fromObj[0].dataSet.getTable(0).data.items[0]);
                //调用fillReturnData方法
                this.fillReturnData.call(this, recordData);
                this.win.close();
            } else {
                Ext.Msg.alert("系统提示", "请选择载入的明细！");
                return;
            }
        }
        if (e.dataInfo.fieldName == "BtnReturn") {
            this.win.close();
        }
    }
}

//查询数据，填充本DataFunc的GRID数据
proto.fillData = function (getData) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(0);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(0);
        //获取采购询价单datafunc的grid
        var grid = Ext.getCmp(this.winId + 'COMFINDCHECKSOLUTIONDATAFUNCGrid');
        var list = getData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                //为grid添加行

                var newRow = this.addRowForGrid(grid);
                newRow.set("CHECKSTID", info.CheckStId);
                newRow.set("CHECKSTNAME", info.CheckStName);                
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

//返回值赋值
//将选中的行记录数据填回明细中
proto.fillReturnData = function (recordData) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = proto.fromFromObj[0].dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        var grid = Ext.getCmp(proto.winIdFrom + 'CHECKSOLUTIONDATAFUNCDETAILGrid');
        //入库通知单子表赋值
        for (var i = 0; i < recordData.length; i++) {
            //recordData的行赋给info
            var info = recordData[i];
            var deleteRowNo = this.checkExist(grid, info);
            if (deleteRowNo >= 0) {
                curStore.removeAt(deleteRowNo);
            }
            var newRow = proto.fromFromObj[0].addRowForGrid(grid);
            newRow.set("ISCHOSE", true);
            newRow.set("CHECKITEMNAME", info.CheckItemName);
            newRow.set("PICLOCATE", info.PicLocate);
            newRow.set("CHECKITEMTYPE", info.CheckItemType);
            newRow.set("UPLIMIT", info.UpLimit);
            newRow.set("LOWLIMIT", info.LowLimit);
            newRow.set("STANDARD", info.Standard);
            newRow.set("DEFECTID", info.DefectId);
            newRow.set("DEFECTNAME", info.DefectName);
            newRow.set("ROUGHNESS", info.Roughness);
            newRow.set("CHARONE", info.CharOne);
            newRow.set("LIMITONE", info.LimitOne);
            newRow.set("TOWARDONE", info.TowardOne);
            newRow.set("CHARTWO", info.CharTwo);
            newRow.set("LIMITTWO", info.LimitTwo);
            newRow.set("TOWARDTWO", info.TowardTwo);
            newRow.set("CHARTHREE", info.CharThree);
            newRow.set("LIMITTHREE", info.LimitThree);
            newRow.set("TOWARDTHREE", info.TowardThree);
            newRow.set("REMARK", info.Remark);
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

proto.checkExist = function (grid, info) {
    var records = grid.store.data.items;
    for (var i = 0; i < records.length; i++) {
        if (records[i].get('CHECKITEMNAME') == info.CheckItemName) {
            return i;
        }
    }
    return -1;

}

