comComtractNoDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = comComtractNoDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comComtractNoDataFuncVcl;
proto.doSetParam = function (vclObj) {
    //判断参数是否为空,代表着是否被呼叫打开
    
        //this.fromVcl = vclObj[0];
        //this.fromObj = vclObj[1];
        //this.fromMethod = vclObj[2];
        //this.parms = vclObj[3];
        //给表头赋值
        var myDate = new Date();
        var masterRow = this.dataSet.getTable(0).data.items[0];
        masterRow.set("YEAR", myDate.getFullYear().toString().substr(2,2));
        this.forms[0].loadRecord(masterRow);
    
}
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:

            break;
        case LibEventTypeEnum.BeforeAddRow:

            e.dataInfo.cancel = true;

            break;
        case LibEventTypeEnum.BeforeDeleteRow:

            //e.dataInfo.cancel = true;

            break;
        case LibEventTypeEnum.ButtonClick:
            //关闭
            if (e.dataInfo.fieldName == "BtnCloseProductive") {
                this.win.close();

            }
            //查询
            if (e.dataInfo.fieldName == "BtnSelectContract") {
                var headTable = this.dataSet.getTable(0).data.items[0];
                if (headTable.data["YEAR"] == 0) {
                    Ext.Msg.alert("系统提示", "年份值必须大于0！！");
                    break;
                }
                else if (!(headTable.data["CONTRACTNOSTART"] > 0 && headTable.data["CONTRACTNOEND"] > 0)) {
                    Ext.Msg.alert("系统提示", "请维护开始合同号和结束合同号！！");
                }
                else {
                    var lst = [];
                    var dt = this.dataSet.getTable(1);
                    for (var i = 0; i < dt.data.items.length; i++) {
                        var row = dt.data.items[i];
                        lst.push(
                            {
                                RecordId: row.data["RECORDID"],
                            });

                    }
                    var returnData = this.invorkBcf("GetContractNoData", [headTable.data["CONTRACTNOSTART"], headTable.data["CONTRACTNOEND"], headTable.data["YEAR"]]);
                    fillContractNoDataFunc(this, returnData);
                }
            }
            if (e.dataInfo.fieldName == "BtnClearProductive") {

                var dt = this.dataSet.getTable(1);
                dt.removeAll();
            }            
        case LibEventTypeEnum.Validated:
            
            break;
    }
}

//填充明细数据
function fillContractNoDataFunc(This, returnData) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        This.deleteAll(1);
        var masterRow = This.dataSet.getTable(0).data.items[0];
        var bodyRow = This.dataSet.getTable(1);
        var bool = true;
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = This.addRow(masterRow, 1);
                newRow.set('CONTRACTNO', info.ContractNo);
               
                //newRow.set('BOMLEVEL', info.BomLevel);

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