MonPointDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = MonPointDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = MonPointDataFuncVcl;
proto.winId = null;
proto.fromObj = null;
proto.getType = 0;
proto.doSetParam = function (vclObj) {
    //console.log(arguments);
    //proto.winId = vclObj[0].winId;
    //proto.fromObj = vclObj[0];
};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    //不允许手工添加行
    if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }
        //不允许手工删除行
    else if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }

        //下拉选一个点位,就在明细里加一行这个点位
    else if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo.tableIndex == 0) {
            if (e.dataInfo.fieldName == "POINTID") {
                var masterRow = this.dataSet.getTable(0).data.items[0];
                var grid = Ext.getCmp(this.winId + 'MONPOINTVALUEDETAILGrid');
                var bodyRow = this.addRow(masterRow, 1);
                if (!checkGetNotice(grid, e.dataInfo.value)) {
                    bodyRow.set("POINTID", e.dataInfo.value);
                    bodyRow.set("POINTNAME", masterRow.get("POINTNAME"));
                }
            }
        }
    }


    else if (e.libEventType == LibEventTypeEnum.ButtonClick) {

        if (e.dataInfo.fieldName == "btnCheckPoint") {
            if (this.isEdit) {
                Ax.utils.LibVclSystemUtils.openDataFunc("Mon.PointFunc", "选取点位", [this]);
            }
            else {
                Ext.Msg.alert("系统提示", "非编辑状态下不可操作！");
            }
        }

        else if (e.dataInfo.fieldName == "btnCheckAll") {
            if (this.isEdit) {
                Ax.utils.LibVclSystemUtils.openDataFunc("Mon.PointFunc", "选取点位", [this]);
            }
            else {
                Ext.Msg.alert("系统提示", "非编辑状态下不可操作！");
            }
        }
    }
}

//判断 点位是否已经在明细列表中
function checkGetNotice(grid, info) {
    var checkOk = false;
    var records = grid.store.data.items;
    //设备作业计划单明细 重复判断 
    for (var i = 0; i < records.length; i++) {
        if (records[i].get('POINTID') == info ) {
            checkOk = true;
        }
    }
    return checkOk;
}



