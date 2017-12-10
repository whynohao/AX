stkBarcodePrintDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkBarcodePrintDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkBarcodePrintDataFuncVcl;

proto.printType = "";
proto.fromBillNo = "";
proto.fromProgId = "";
//赋值方法
proto.doSetParam = function (vclObj) {
    if (vclObj != undefined) {
        proto.printType = vclObj[0];
        proto.fromBillNo = vclObj[1];
        proto.fromProgId = vclObj[2];
        var masterRow = this.dataSet.getTable(0).data.items[0];
        masterRow.set("PRINTTYPE", proto.printType);
        masterRow.set("FROMBILLNO", proto.fromBillNo);
        masterRow.set("PROGID", proto.fromProgId);
        masterRow.set("PROGNAME", vclObj[3]);
        this.forms[0].loadRecord(masterRow);
        var returnData = this.invorkBcf("GetDetail", [proto.fromBillNo, "", proto.fromProgId]);
        //显示赋值
        this.fillData.call(this, returnData);
    }
};

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    //表身（取数据用allBodyRows[i].get("")）
    var allBodyRows = this.dataSet.getTable(1).data.items;


    if (e.libEventType === LibEventTypeEnum.BeforeAddRow) {
        if (e.dataInfo.tableIndex === 1) {
            e.dataInfo.cancel = true;
        }
    }

    if (e.libEventType === LibEventTypeEnum.Validated && e.dataInfo.tableIndex == 0) {
        if (e.dataInfo.fieldName === "PROGID") {
            this.dataSet.getTable(0).data.items[0].set("FROMBILLNO", "");//来源单号设空
            this.forms[0].loadRecord(e.dataInfo.dataRow);
        }
    }

    //用户自定义按钮
    if (e.libEventType === LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName === "BtnSelect") {
            var masterRow = this.dataSet.getTable(0).data.items[0];
            if (masterRow.get("FROMBILLNO") || masterRow.get("PROGID")) {
                var fromProgId = masterRow.get("PROGID");
                var billno = masterRow.get("FROMBILLNO");
                var materialid = masterRow.get("MATERIALID");
                //调用中间层方法
                var returnData = this.invorkBcf("GetDetail", [billno, materialid, fromProgId]);
                //显示赋值
                this.fillData.call(this, returnData);
            }
            else {
                Ext.Msg.alert("提示", '请选择来源单号和来源功能单');
                return;
            }
        }

        //全选
        if (e.dataInfo.fieldName == "BtnSelectAll") {
            for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                this.dataSet.getTable(1).data.items[i].set("SELECT", 1);
            }
        }
        //全反选
        if (e.dataInfo.fieldName == "BtnSelectNone") {
            for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                this.dataSet.getTable(1).data.items[i].set("SELECT", 0);
            }
        }

        if (e.dataInfo.fieldName == "BtnPrint") {
            var printtype = this.dataSet.getTable(0).data.items[0].get("PRINTTYPE");
            var fromProgId = this.dataSet.getTable(0).data.items[0].get("PROGID");
            var canprint = false;
            if (this.dataSet.getTable(1).data.items.length == 0) {
                Ext.Msg.alert("提示", '没有需要打印的数据');
                return;
            } else {
                for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                    if (this.dataSet.getTable(1).data.items[i].data["SELECT"] == 1) {
                        var printNum = this.dataSet.getTable(1).data.items[i].data["PRINTNUM"];
                        var printTimes = this.dataSet.getTable(1).data.items[i].data["PRINTTIMES"];
                        var barcodeType = this.dataSet.getTable(1).data.items[i].data["BARCODETYPE"];
                        var serialNumber = this.dataSet.getTable(1).data.items[i].data["SERIALNUMBER"];
                        if (printNum <= 0 || printTimes <= 0) {
                            Ext.Msg.alert("提示", '请输入打印数量或打印张次且不得小于0');
                            return;
                        } else if (!this.dataSet.getTable(1).data.items[i].get("LABELTEMPLATEID")) {
                            Ext.Msg.alert("提示", '请选择打印模板');
                            return;
                        } else if (barcodeType == 2 && !serialNumber) {
                            Ext.Msg.alert("提示", '打印出厂编码时，此物料的出厂编号不得为空');
                            return;
                        } else {
                            canprint = true;
                        }
                    }
                }
            }
            if (canprint) {
                for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                    if (this.dataSet.getTable(1).data.items[i].data["SELECT"] == 1) {
                        var printTimes = this.dataSet.getTable(1).data.items[i].data["PRINTTIMES"];
                        var printNum = this.dataSet.getTable(1).data.items[i].data["PRINTNUM"];
                        for (var j = 0; j < printNum; j++) {
                            var code = this.invorkBcf("ReadPrintTemplateTxt", [printtype, fromProgId, this.dataSet.getTable(1).data.items[i].data]);
                            var LODOP = getLodop(document.getElementById('LODOP'), document.getElementById('LODOP_EM'));
                            eval(code);
                            LODOP.PRINT();
                            if (printTimes > 1) {
                                for (var k = 0; k < printTimes - 1; k++) {
                                    eval(code);
                                    LODOP.PRINT();
                                }
                                break;
                            }
                        }
                    }
                }
            } else {
                Ext.Msg.alert("提示", '请选择需要打印的行');
                return;
            }
        }
    }



}

//查询数据，填充本DataFunc的GRID数据
proto.fillData = function (returnData) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    curStore.suspendEvents();//关闭store事件
    try {
        //删除当前grid的数据
        this.deleteAll(1);
        //获取采购订单单datafunc的grid
        var grid = Ext.getCmp(this.winId + 'STKBARCODEPRINTDETAILGrid');
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                //为grid添加行
                //采购询价单datatfunc中的所有字段
                var newRow = this.addRowForGrid(grid);

                newRow.set("FROMBILLNO", info.FromBillNo);
                newRow.set("FROMROWID", info.FromRowId);
                newRow.set("WORKORDERNO", info.WorkorderNo);
                newRow.set("SERIALNUMBER", info.SerialNumber);
                newRow.set("SUPPLIERID", info.SupplierId);
                newRow.set("SUPPLIERNAME", info.SupplierName);
                newRow.set("SUPPLIERNO", info.SupplierNo);
                newRow.set("MATERIALID", info.MaterialId);
                newRow.set("MATERIALNAME", info.MaterialName);
                newRow.set("SPECIFICATION", info.Specification);
                newRow.set("FIGURENO", info.FigureNo);
                newRow.set("MATERIALTYPEID", info.MaterialTypeId);
                newRow.set("MATERIALTYPENAME", info.MaterialTypeName);
                newRow.set("BARCODETYPE", info.BarcodeType);
                newRow.set("BATCHNO", info.BatchNo);
                newRow.set("SUBBATCHNO", info.SubBatchNo);
                newRow.set("PRINTNUM", info.PrintNum);
                newRow.set("PRINTTIMES", info.PrintTimes);
                newRow.set("NUMBER", info.Number);
                newRow.set("UNITID", info.UnitId);
                newRow.set("UNITNAME", info.UnitName);
                newRow.set("LABELTEMPLATEID", info.LabelTemplateId);
                newRow.set("LABELTEMPLATENAME", info.LabelTemplateName);
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