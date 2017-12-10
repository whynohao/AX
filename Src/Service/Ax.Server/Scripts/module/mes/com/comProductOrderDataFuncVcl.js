comProductOrderDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = comProductOrderDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comProductOrderDataFuncVcl;

proto.winId = null;
proto.fromObj = null;
//赋值方法
proto.doSetParam = function (vclObj) {
    //判断参数是否为空,代表着是否被呼叫打开
    if (vclObj != undefined) {
        proto.winId = vclObj[0].winId;
        proto.fromObj = vclObj;

        //获取参数值
        proto.comProductId = vclObj[1];
        proto.conTractNo = vclObj[2];

        //给表头赋值
        var masterRow = this.dataSet.getTable(0).data.items[0];
        masterRow.set("COMPRODUCTID", proto.comProductId);
        masterRow.set("CONTRACTNO", proto.conTractNo);

        //重新加载数据
        this.forms[0].loadRecord(masterRow);

        var returnData = this.invorkBcf("GetProductOrderData", [proto.comProductId]);
        fillProductOrderDataFunc(this, returnData);
    }
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            //关闭
            if (e.dataInfo.fieldName == "BtnCloseDeTail") {
                this.win.close();

            }
            //查询
            if (e.dataInfo.fieldName == "BtnSelectDeTail") {
                var headTable = this.dataSet.getTable(0).data.items[0];
                if (headTable.data["COMPRODUCTID"] =="") {
                    Ext.Msg.alert("系统提示", "请先填写表头的投产编号信息！！");
                }
                else {
                    var returnData = this.invorkBcf("GetProductOrderData", [headTable.data["COMPRODUCTID"]]);
                    fillProductOrderDataFunc(this, returnData);
                }
            }
            //确认
            if (e.dataInfo.fieldName == "BtnLoadMaterial") {

            }
            break;
    }
}

//填充明细数据
function fillProductOrderDataFunc(This, returnData) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        This.deleteAll(1);
        var masterRow = This.dataSet.getTable(0).data.items[0];
        if (returnData !== undefined && returnData.length > 0) {
            for (var i = 0; i < returnData.length; i++) {
                var info = returnData[i];
                var newRow = This.addRow(masterRow, 1);
                newRow.set('BILLNO', info.BillNo);
                newRow.set('ROW_ID', info.RowId);
                newRow.set('ROWNO', info.RowNo);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('FIGURENO', info.FigureNo);
                newRow.set('SPECIFICATION', info.SpecIfication);
                newRow.set('TEXTUREID', info.Textureid);
                newRow.set('TEXTUREIDID', info.TextureIdId);
                newRow.set('MATSTYLE', info.Matstyle);
                newRow.set('MATERIALTYLE', info.Materialtyle);
                newRow.set('BASEQTY', info.BaseQty);
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