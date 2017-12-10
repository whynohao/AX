purOrderPushDelieveryFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = purOrderPushDelieveryFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = purOrderPushDelieveryFuncVcl;

proto.doSetParam = function () {
    var records = this.invorkBcf("GetChaseOrderDetail", ['', '', '']);
    this.fillData(records);
}
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            var headTable = this.dataSet.getTable(0).data.items[0].data;
            if (e.dataInfo.fieldName == 'BtnSelect') {
                if (headTable["CONTACTSOBJECTID"] == '') {
                    Ext.Msg.alert("提示", "请先选择一个供应商");
                    return;
                }
                var type = headTable["FROMTYPE"];
                if (type == 0)
                    var records = this.invorkBcf("GetChaseOrderDetail", [headTable["CONTACTSOBJECTID"], headTable["CONTRACTCODE"], headTable["PERSONID"]]);
                else if (type == 1)
                    var records = this.invorkBcf("GetPPSubcontracteOrderDetial", [headTable["CONTACTSOBJECTID"], headTable["CONTRACTCODE"], headTable["PERSONID"]]);
                this.fillData(records);
            }
            else if (e.dataInfo.fieldName == 'BtnPushDeliveryNote') {
                var record = [];
                //   = Ext.getCmp(this.winId + 'PURORDERPUSHDELIEVERYFUNCDEATILGrid').getSelectionModel().selected.items;
                var grid = this.dataSet.getTable(1).data.items;
                for (var i = 0; i < grid.length; i++) {
                    if (grid[i].data["ISCHOSE"] == 1) {
                        record.push({
                            CONTRACTCODE: grid[i].data["CONTRACTCODE"],
                            CONTRACTNO: grid[i].data["CONTRACTNO"],
                            CONTACTSOBJECTID: grid[i].data["CONTACTSOBJECTID"],
                            PERSONID: grid[i].data["PERSONID"],
                            DEPTID: grid[i].data["DEPTID"],
                            CURRENCYID: grid[i].data["CURRENCYID"],
                            INVOICETYPEID: grid[i].data["INVOICETYPEID"],
                            TRANSPORTWAYID: grid[i].data["TRANSPORTWAYID"],
                            MATERIALID: grid[i].data["MATERIALID"],
                            BILLNO: grid[i].data["BILLNO"],
                            ROW_ID: grid[i].data["ROW_ID"],
                            PLANARRIVEDATE: grid[i].data["PLANARRIVEDATE"],
                            DEALSQUANTITY: grid[i].data["DEALSQUANTITY"],
                            DEALSUNITID: grid[i].data["DEALSUNITID"],
                            QUANTITY: grid[i].data["QUANTITY"],
                            PRICE: grid[i].data["PRICE"],
                            AMOUNT: grid[i].data["AMOUNT"],
                            TAXRATE: grid[i].data["TAXRATE"],
                            TAXES: grid[i].data["TAXES"],
                            TAXAMOUNT: grid[i].data["TAXAMOUNT"],
                            TAXPRICE: grid[i].data["TAXPRICE"],
                            BWAMOUNT: grid[i].data["BWAMOUNT"],
                            BWTAXAMOUNT: grid[i].data["BWTAXAMOUNT"],
                            BWTAXES: grid[i].data["BWTAXES"],
                            PAYMENTTYPEID: grid[i].data["PAYMENTTYPEID"],
                            UNITID: grid[i].data["UNITID"],
                        });
                    }
                }
                var type = headTable["FROMTYPE"];
                var billno = this.invorkBcf("PushDelieveryNote", [record, type]);
                if (billno != '') {
                    var curPks = [];
                    curPks.push(billno);
                    Ax.utils.LibVclSystemUtils.openBill("stk.DeliveryNote", BillTypeEnum.Bill, "收料通知单", BillActionEnum.Browse, undefined, curPks);
                }
                else
                    Ext.Msg.alert("提示", "下推失败");
            }
            break;

    }
}
proto.fillData = function (records) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据   
        if (records != undefined && records.length > 0) {
            for (var i = 0; i < records.length; i++) {
                var info = records[i];
                if (info.DEALSQUANTITY > 0) {
                    var newRow = this.addRow(masterRow, 1);
                    newRow.set("BILLNO", info.BILLNO);
                    newRow.set("ROW_ID", info.ROW_ID);
                    newRow.set("CONTRACTCODE", info.CONTRACTCODE);
                    newRow.set("CONTRACTNO", info.CONTRACTNO);
                    newRow.set("PERSONID", info.PERSONID);
                    newRow.set("PERSONNAME", info.PERSONNAME);
                    newRow.set("CONTACTSOBJECTID", info.CONTACTSOBJECTID);
                    newRow.set("CONTACTSOBJECTNAME", info.CONTACTSOBJECTNAME);
                    newRow.set("PLANARRIVEDATE", info.PLANARRIVEDATE);
                    newRow.set("MATERIALID", info.MATERIALID);
                    newRow.set("MATERIALNAME", info.MATERIALNAME);
                    newRow.set("SPECIFICATION", info.SPECIFICATION);
                    newRow.set("TEXTUREID", info.TEXTUREID);
                    newRow.set("FIGURENO", info.FIGURENO);
                    newRow.set("MATERIALSPEC", info.MATERIALSPEC);
                    newRow.set("NEEDCHECK", info.NEEDCHECK);
                    newRow.set("ATTRIBUTEID", info.ATTRIBUTEID);
                    newRow.set("ATTRIBUTENAME", info.ATTRIBUTENAME);
                    newRow.set("ATTRIBUTECODE", info.ATTRIBUTECODE);
                    newRow.set("ATTRIBUTEDESC", info.ATTRIBUTEDESC);
                    newRow.set("DEALSQUANTITY", info.DEALSQUANTITY);
                    newRow.set("DEALSUNITID", info.DEALSUNITID);
                    newRow.set("DEALSUNITNAME", info.DEALSUNITNAME);
                    newRow.set("DEALSUNITNO", info.DEALSUNITNO);
                    newRow.set("QUANTITY", info.QUANTITY);
                    newRow.set("UNITID", info.UNITID);
                    newRow.set("UNITNAME", info.UNITNAME);
                    newRow.set("PRICE", info.PRICE);
                    newRow.set("AMOUNT", info.PRICE * info.DEALSQUANTITY);
                    newRow.set("TAXRATE", info.TAXRATE);
                    newRow.set("TAXPRICE", info.TAXPRICE);
                    newRow.set("TAXAMOUNT", info.TAXPRICE * info.DEALSQUANTITY);
                    newRow.set("TAXES", info.PRICE * info.DEALSQUANTITY * info.TAXRATE);
                    newRow.set("BWAMOUNT", info.PRICE * info.DEALSQUANTITY * info.STANDARDCOILRATE);
                    newRow.set("BWTAXAMOUNT", info.TAXPRICE * info.DEALSQUANTITY * info.STANDARDCOILRATE);
                    newRow.set("BWTAXES", info.PRICE * info.DEALSQUANTITY * info.TAXRATE * info.STANDARDCOILRATE);
                    newRow.set("TRANSPORTWAYID", info.TRANSPORTWAYID);
                    newRow.set("TRANSPORTWAYNAME", info.TRANSPORTWAYNAME);
                    newRow.set("DEPTID", info.DEPTID);
                    newRow.set("DEPTNAME", info.DEPTNAME);
                    newRow.set("CURRENCYID", info.CURRENCYID);
                    newRow.set("CURRENCYNAME", info.CURRENCYNAME);
                    newRow.set("PAYMENTTYPEID", info.PAYMENTTYPEID);
                    newRow.set("PAYMENTTYPENAME", info.PAYMENTTYPENAME);
                    newRow.set("INVOICETYPEID", info.INVOICETYPEID);
                    newRow.set("INVOICETYPENAME", info.INVOICETYPENAME);
                }
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}