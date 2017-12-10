finIATempEstiCostVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    this.dataRow;
};
var proto = finIATempEstiCostVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finIATempEstiCostVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var curState = this.dataSet.getTable(0).data.items[0].get("CURRENTSTATE");
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            //不允许手工添加行
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (this.isEdit) {
                if (e.dataInfo.fieldName == "AccountsPayableLoad") {
                    var bodyRows = this.dataSet.getTable(1).data.items;
                    var recoders = [];
                    for (var i = 0; i < bodyRows.length; i++) {
                        var row = bodyRows[i];
                        recoders.push({
                            PurChaseStockInNo: row.data["PURCHASESTOCKINNO"],
                            PurChaseStockInRowId: row.data["PURCHASESTOCKINROWID"],
                            TempestiPrice: row.data["TEMPESTIPRICE"]
                        });
                    }
                    var accountYear = this.dataSet.getTable(0).data.items[0].data['ACCOUNTYEAR'];
                    var accountMonth = this.dataSet.getTable(0).data.items[0].data['ACCOUNTMONTH'];
                    var data = this.invorkBcf('GetData', [accountYear, accountMonth, recoders]);
                    this.fillData(data);
                }
            }
            else {
                Ext.Msg.alert("系统提示", "单据只有在修改状态才能载入数据！");
            }
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                switch (e.dataInfo.fieldName) {
                    case "TEMPESTIPRICE":
                        if (e.dataInfo.value < 0) {
                            Ext.Msg.alert("系统提示", "暂估单价须大于等于0！");
                            e.dataInfo.cancel = true;
                        }
                        break;
                }
            }
            else {
                if (e.dataInfo.fieldName == "BILLDATE") {
                    if (e.dataInfo.dataRow.data["CURRENTSTATE"] == 2) {
                        Ext.Msg.alert("系统提示", "生效暂估单不能修改日期！");
                        e.dataInfo.cancel = true;
                    }
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "TEMPESTIPRICE") {
                    var price = e.dataInfo.value;
                    var quantity = e.dataInfo.dataRow.data["NOINVOICEPURQTY"];
                    e.dataInfo.dataRow.set("TEMPESTIAMOUNT", quantity * price);
                }
            }
            break;

    }
}

proto.fillData = function (returnData) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'FINIATEMPESTICOSTDETAILGrid');
        var list = returnData;
        var billNo = this.dataSet.getTable(0).data.items[0].get("BILLNO");
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set("BILLNO", billNo);
                newRow.set("ROW_ID", i + 1);
                newRow.set("ROWNO", i + 1);
                newRow.set("PURCHASESTOCKINNO", info.PurChaseStockInNo);
                newRow.set("PURCHASESTOCKINROWID", info.PurChaseStockInRowId);
                newRow.set("PURCHASEORDERNO", info.PurChaseOrderNo);
                newRow.set("PURCHASEORDERROWID", info.PurChaseOrderRowId);
                newRow.set("MATERIALID", info.MaterialId);
                newRow.set("MATERIALNAME", info.MaterialName);
                newRow.set("MATERIALSPEC", info.MaterialSpec);
                newRow.set("UNITID", info.UnitId);
                newRow.set("UNITNAME", info.UnitName);
                newRow.set("PURUNITID", info.PurUnitId);
                newRow.set("PURUNITNAME", info.PurUnitName);
                newRow.set("NOINVOICEPURQTY", info.NoInVoicePurQty);
                newRow.set("NOINVOICEQTY", info.NoInVoiceQty);
                newRow.set("PURPRICE", info.PurPrice);
                newRow.set("PURAMOUNT", info.PurPrice * info.NoInVoiceQty);
                newRow.set("TEMPESTIPRICE", info.TempestiPrice);
                newRow.set("TEMPESTIAMOUNT", info.TempestiPrice * info.NoInVoiceQty);
                newRow.set("ATTRIBUTECODE", info.AttributeCode);
                newRow.set("ATTRIBUTEDESC", info.AttributeDesc);
                newRow.set("CONTACTSOBJECTID", info.ContactsObjectId);
            }
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
};