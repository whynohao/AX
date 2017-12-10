plsDayPlanSendVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
}
var proto = plsDayPlanSendVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsDayPlanSendVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo.fieldName == 'PLANDATE') {
            e.dataInfo.dataRow.set('PLANDATE', e.dataInfo.value);
        }
    }
    if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
        Ext.Msg.alert("提示", "不能删除！");
        e.dataInfo.cancel = true;
    }
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == 'BtnLoad') {//载入日计划单
            var date = this.dataSet.getTable(0).data.items[0].get('PLANDATE');
            var produceLineId = this.dataSet.getTable(0).data.items[0].get("PRODUCELINEID");
            var workshopsectionId = this.dataSet.getTable(0).data.items[0].get("WORKSHOPSECTIONID");
            if (date) {
                var data = this.invorkBcf('BtnSelected', [date, produceLineId, workshopsectionId]);
                this.funBtnLoad.call(this, data);
            }
        }
        else if (e.dataInfo.fieldName == 'BtnAddBarcode') {//生成赋码数据
            var headList = [];
            var bodyList = [];
            var headTable = this.dataSet.getTable(0).data.items[0].data;
            headList.push({
                Plandate: headTable["PLANDATE"],
                ProducelineId: headTable["PRODUCELINEID"],
                ProducelineName: headTable["PRODUCELINENAME"],
                WorkshopsectionId: headTable["WORKSHOPSECTIONID"],
                TechrouteId: headTable["TECHROUTEID"],
                BarcoderuleId: headTable["BARCODERULEID"],
                IstechrouteLine: headTable["ISTECHROUTELINE"],
            });
            var bodyTable = this.dataSet.getTable(1).data.items;
            for (var i = 0; i < bodyTable.length; i++) {
                bodyList.push({
                    PworkorderNo: bodyTable[i].data["PWORKORDERNO"],
                    WorkorderBillNo: bodyTable[i].data["WORKORDERBILLNO"],
                    FrombillNo: bodyTable[i].data["FROMBILLNO"],
                    FromRowId: bodyTable[i].data["FROMROWID"],
                    Billno: bodyTable[i].data["BILLNO"],
                    RowId: bodyTable[i].data["ROW_ID"],
                    SendEndTime: bodyTable[i].data["SENDENDTIME"],
                    StockStartTime: bodyTable[i].data["STOCKSTARTTIME"],
                    StockEndTime: bodyTable[i].data["STOCKENDTIME"],
                });
            }
            if (bodyList.length != 0) {
                var data = this.invorkBcf('AddBarcodeData', [headList, bodyList]);
            }
        }
        else if (e.dataInfo.fieldName == 'BtnDeleteBarcode') {//删除赋码数this.dataSet.getTable(0)据
            var headList = [];
            var bodyList = [];
            var headTable = this.dataSet.getTable(0).data.items[0].data;
            headList.push({
                Plandate: headTable["PLANDATE"],
                ProducelineId: headTable["PRODUCELINEID"],
                ProducelineName: headTable["PRODUCELINENAME"],
                WorkshopsectionId: headTable["WORKSHOPSECTIONID"],
                TechrouteId: headTable["TECHROUTEID"],
                BarcoderuleId: headTable["BARCODERULEID"],
                IstechrouteLine: headTable["ISTECHROUTELINE"],
            });
            var bodyTable = this.dataSet.getTable(1).data.items;
            for (var i = 0; i < bodyTable.length; i++) {
                bodyList.push({
                    PworkorderNo: bodyTable[i].data["PWORKORDERNO"],
                    WorkorderBillNo: bodyTable[i].data["WORKORDERBILLNO"],
                    FrombillNo: bodyTable[i].data["FROMBILLNO"],
                    FromRowId: bodyTable[i].data["FROMROWID"],
                    Billno: bodyTable[i].data["BILLNO"],
                    RowId: bodyTable[i].data["ROW_ID"],
                    SendEndTime: bodyTable[i].data["SENDENDTIME"],
                    StockStartTime: bodyTable[i].data["STOCKSTARTTIME"],
                    StockEndTime: bodyTable[i].data["STOCKENDTIME"],
                });
            }
            if (bodyList.length != 0) {
                var data = this.invorkBcf('DeleteBarcode', [headList, bodyList]);
            }
        }
        else if (e.dataInfo.fieldName == 'BtnAddStockMat') {//生成备料派料数据
            var headList = [];
            var bodyList = [];
            var headTable = this.dataSet.getTable(0).data.items[0].data;
            headList.push({
                Plandate: headTable["PLANDATE"],
                ProducelineId: headTable["PRODUCELINEID"],
                ProducelineName: headTable["PRODUCELINENAME"],
                WorkshopsectionId: headTable["WORKSHOPSECTIONID"],
                TechrouteId: headTable["TECHROUTEID"],
                BarcoderuleId: headTable["BARCODERULEID"],
                IstechrouteLine: headTable["ISTECHROUTELINE"],
            });
            var bodyTable = this.dataSet.getTable(1).data.items;
            for (var i = 0; i < bodyTable.length; i++) {
                bodyList.push({
                    PworkorderNo: bodyTable[i].data["PWORKORDERNO"],
                    WorkorderBillNo: bodyTable[i].data["WORKORDERBILLNO"],
                    FrombillNo: bodyTable[i].data["FROMBILLNO"],
                    FromRowId: bodyTable[i].data["FROMROWID"],
                    Billno: bodyTable[i].data["BILLNO"],
                    RowId: bodyTable[i].data["ROW_ID"],
                    SendEndTime: bodyTable[i].data["SENDENDTIME"],
                    StockStartTime: bodyTable[i].data["STOCKSTARTTIME"],
                    StockEndTime: bodyTable[i].data["STOCKENDTIME"],
                });
            }
            if (bodyList.length != 0) {
                this.invorkBcf('AddStockMat', [headList, bodyList]);
            }
        }
        else if (e.dataInfo.fieldName == 'BtnDeleteStockMat') {//删除备料派料数据
            var bodyList = [];
            var bodyTable = this.dataSet.getTable(1).data.items;
            for (var i = 0; i < bodyTable.length; i++) {
                bodyList.push({
                    Billno: bodyTable[i].data["BILLNO"],
                    RowId: bodyTable[i].data["ROW_ID"],
                });
            }
            if (bodyList.length != 0) {
                this.invorkBcf('DeleteStockMat', [bodyList]);
            }
        }
    }
}

//加载明细表
proto.funBtnLoad = function (data) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {

        var list = data;
        this.dataSet.getTable(1).removeAll();
        if (list !== undefined && list.length > 0) {
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var ctrl = Ext.getCmp("TECHROUTEID0_" + this.winId);
            ctrl.store.add({ Id: data[0].TechrouteId, Name: data[0].TechrouteName });
            ctrl.select(data[0].TechrouteId);
            var ctrl2 = Ext.getCmp("BARCODERULEID0_" + this.winId);
            ctrl2.store.add({ Id: data[0].BarcoderuleId, Name: data[0].BarcoderuleName });
            ctrl2.select(data[0].BarcoderuleId);
            Ext.getCmp("ISTECHROUTELINE0_" + this.winId).setValue(data[0].IstechrouteLine);
            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set('PLANDATE', info.PlanDate);
                newRow.set('BILLNO', info.Billno);
                newRow.set('ROW_ID', info.RowId);
                newRow.set('FROMBILLNO', info.FrombillNo);
                newRow.set('FROMROWID', info.FromRowId);
                newRow.set('SENDENDTIME', info.SendEndTime);
                newRow.set('STOCKSTARTTIME', info.StockStartTime);
                newRow.set('STOCKENDTIME', info.StockEndTime);
                newRow.set('WORKORDERBILLNO', info.WorkorderBillNo);
                newRow.set('PWORKORDERNO', info.PworkorderNo);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}