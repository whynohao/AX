comCheckSolutionDataFuncVcl= function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = comCheckSolutionDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comCheckSolutionDataFuncVcl;

proto.winId = null;
proto.fromObj = null;
//赋值方法
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj;
    proto.checkName = vclObj[1];
    proto.figureNo = vclObj[2];
    proto.workProcessId = vclObj[3];
    proto.workProcessName = vclObj[4];
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set("CHECKSTNAME", proto.checkName);
    masterRow.set("FIGURENO", proto.figureNo);
    masterRow.set("WORKPROCESSID", proto.workProcessId);
    masterRow.set("WORKPROCESSNAME", proto.workProcessName);
    this.forms[0].loadRecord(masterRow);
}

proto.vclHandler = function(sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    //表头（取数据用masterRow.get("")）
    var masterRow = this.dataSet.getTable(0).data.items[0];

    if (e.dataInfo && e.dataInfo.tableIndex == 1) {
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
        //查询
        if (e.dataInfo.fieldName == "BtnFind") {
            if (masterRow.get("CHECKSTNAME") || masterRow.get("FIGURENO") || masterRow.get("WORKPROCESSID")) {
                //datafunc表头检测方案名称
                var checkName = masterRow.get("CHECKSTNAME");
                var figureNo = masterRow.get("FIGURENO");
                var workProcessId = masterRow.get("WORKPROCESSID");
                //var getData = this.invorkBcf("GetCheckSolution", [checkName]);
                Ax.utils.LibVclSystemUtils.openDataFunc("com.FindCheckSolutionDataFunc", "检测方案查询清单", [this, proto.fromObj, checkName, figureNo, workProcessId]);

            }
            else {
                Ext.Msg.alert("提示", '请输入查询条件');
                return;
            }
        }
        if (e.dataInfo.fieldName == "BtnOK") {

            //获取datafunc表身的grid
            var grid = Ext.getCmp(this.winId + 'CHECKSOLUTIONAXCEDETAILGrid');
            //表身行项
            var selectItems = this.dataSet.getTable(1).data.items;
            //数组，用于存储数据
            var records = [];
            //循环所有行项
            for (var i = 0; i < selectItems.length; i++) {
                //如果打勾
                if (selectItems[i].data["ISCHOSE"] == true) {
                    //将行项对象加入数组
                    records.push({
                        CheckItemName: selectItems[i].data["CHECKITEMNAME"],
                        PicLocate: selectItems[i].data["PICLOCATE"],
                        CheckItemType: selectItems[i].data["CHECKITEMTYPE"],
                        UpLimit: selectItems[i].data["UPLIMIT"],
                        LowLimit: selectItems[i].data["LOWLIMIT"],
                        Standard: selectItems[i].data["STANDARD"],
                        DefectId: selectItems[i].data["DEFECTID"],
                        DefectName: selectItems[i].data["DEFECTNAME"],
                        Roughness: selectItems[i].data["ROUGHNESS"],
                        CharOne: selectItems[i].data["CHARONE"],
                        LimitOne: selectItems[i].data["LIMITONE"],
                        TowardOne: selectItems[i].data["TOWARDONE"],
                        CharTwo: selectItems[i].data["CHARTWO"],
                        LimitTwo: selectItems[i].data["LIMITTWO"],
                        TowardTwo: selectItems[i].data["TOWARDTWO"],
                        CharThree: selectItems[i].data["CHARTHREE"],
                        LimitThree: selectItems[i].data["LIMITTHREE"],
                        TowardThree: selectItems[i].data["TOWARDTHREE"],
                        Reamrk: selectItems[i].data["REMARK"]
                    });
                }
            }
            //datatfunc中未选择任何行就点击生成通知单则提示
            if (records.length == 0) {
                Ext.Msg.alert("系统提示", "请选择载入的明细！");
                return;
            }
            if (records.length > 0) {
                //调用fillReturnData方法
                this.fillReturnData.call(this, records);
                this.win.close();
            }
        }
    }    
}



//返回值赋值
//将选中的行记录数据填回明细中
proto.fillReturnData = function (records) {

    Ext.suspendLayouts();//关闭Ext布局
    var curStore = proto.fromFromObj[0].fromObj[0].dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        var grid = Ext.getCmp(proto.fromFromObj[0].fromObj[0].winId + 'CHECKSOLUTIONAXCEDETAILGrid');

        //入库通知单子表赋值
        for (var i = 0; i < records.length; i++) {
            //records的行赋给info
            var info = records[i];
            var deleteRowNo = this.checkExist(grid, info);
            if (deleteRowNo >= 0) {
                curStore.removeAt(deleteRowNo);
            }
            var newRow = proto.fromFromObj[0].fromObj[0].addRowForGrid(grid);
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

proto.checkExist=function(grid, info) {
    var records = grid.store.data.items;
    for (var i = 0; i < records.length; i++) {
        if (records[i].get('CHECKITEMNAME') == info.CheckItemName) {
            return i;
        }
    }
    return -1;

}

