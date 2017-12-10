
finARSalInvoiceDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = finARSalInvoiceDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = finARSalInvoiceDataFuncVcl;
proto.winId = null;
proto.fromObj = null;
var salInvoiceData = [];
var isRed;
var taxRate;
proto.doSetParam=function(vclObj)
{
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj;
    salInvoiceData = [];
    var salInvoiceRow = proto.fromObj[0].dataSet.getTable(0).data.items[0];
    salInvoiceTableDetail = proto.fromObj[0].dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    isRed = salInvoiceRow.get("ISRED");
    taxRate = salInvoiceRow.get("TAXRATE");
    var contactsobjectId = salInvoiceRow.get("CUSTOMERID");
    var contactsobjectName = salInvoiceRow.get("CUSTOMERNAME");
    var fromBillType = salInvoiceRow.get("FROMBILLTYPE");
    var contractNo = salInvoiceRow.get("CONTRACTNO");
    masterRow.data["CONTACTSOBJECTID"] = contactsobjectId;
    masterRow.data["CONTACTSOBJECTNAME"] = contactsobjectName;
    masterRow.data["FROMBILLTYPE"] = fromBillType;
    masterRow.data["CONTRACTNO"] = contractNo;
    this.forms[0].loadRecord(masterRow);
    for (var i = 0; i < salInvoiceTableDetail.data.items.length; i++) {
        var tableSalInvoiceDetailValue = salInvoiceTableDetail.data.items[i];
         salInvoiceData.push({
             SaleOrderNo: tableSalInvoiceDetailValue.data["SALESTOCKOUTNO"],
             SaleOrderRowId: tableSalInvoiceDetailValue.data["SALESTOCKOUTROWID"],
             WareOutNo: tableSalInvoiceDetailValue.data["SALEORDERNO"],
             WareOutRowId: tableSalInvoiceDetailValue.data["SALEORDERROWID"],
             ContractNo: tableSalInvoiceDetailValue.data["CONTRACTNO"],
             MaterialId: tableSalInvoiceDetailValue.data["PRODUCTID"],
             MaterialName: tableSalInvoiceDetailValue.data["PRODUCTNAME"],
             AttributeId: tableSalInvoiceDetailValue.data["ATTRIBUTEID"],
             AttributeName: tableSalInvoiceDetailValue.data["ATTRIBUTENAME"],
             MaterialSpec: tableSalInvoiceDetailValue.data["PRODUCTSPEC"],
             AttributeCode: tableSalInvoiceDetailValue.data["ATTRIBUTECODE"],
             AttributeDesc: tableSalInvoiceDetailValue.data["ATTRIBUTEDESC"],
             UnitId: tableSalInvoiceDetailValue.data["UNITID"],
             UnitName: tableSalInvoiceDetailValue.data["UNITNAME"],
             DealUnitId: tableSalInvoiceDetailValue.data["DEALUNITID"],
             DealUnitName: tableSalInvoiceDetailValue.data["DEALUNITNAME"],
             DealUnitNo: tableSalInvoiceDetailValue.data["DEALUNITNO"],
             InvoiceSalQty: tableSalInvoiceDetailValue.data["INVOICESALQTY"],
             InvoiceQty: tableSalInvoiceDetailValue.data["INVOICEQTY"],
             SalTaxPrice: tableSalInvoiceDetailValue.data["SALETAXPRICE"],
        });
    }

}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnQueryData") {
                var masterRow = this.dataSet.getTable(0).data.items[0];
                var billDateFrom = masterRow.get("BILLDATEFROM");
                var billDateTo = masterRow.get("BILLDATETO")
                var contactsobjectId = masterRow.get("CONTACTSOBJECTID");
                var fromBillType = masterRow.get("FROMBILLTYPE");
                var contractNo = masterRow.get("CONTRACTNO");
                var materialId = masterRow.get("MATERIALID");
                var data = this.invorkBcf('GetData', [billDateFrom, billDateTo, contactsobjectId, fromBillType, contractNo, materialId,isRed,salInvoiceData]);
                this.fillData.call(this, data);
            }
            if (e.dataInfo.fieldName == "BtnLoadData") {
                var selectItems = this.dataSet.getTable(1).data.items;
                var records = [];
                for(var i=0;i<selectItems.length;i++)
                {
                    if(selectItems[i].data["ISCHOSE"]==true)
                    {
                        records.push({
                            WareOutNo: selectItems[i].data["WAREOUTNO"],
                            WareOutRowId: selectItems[i].data["WAREOUTROWID"],
                            SaleOrderNo: selectItems[i].data["SALEORDERNO"],
                            SaleOrderRowId: selectItems[i].data["SALEORDERROWID"],
                            ContractNo: selectItems[i].data["CONTRACTNO"],
                            MaterialId: selectItems[i].data["MATERIALID"],
                            MaterialName: selectItems[i].data["MATERIALNAME"],
                            AttributeId: selectItems[i].data["ATTRIBUTEID"],
                            AttributeName: selectItems[i].data["ATTRIBUTENAME"],
                            MaterialSpec: selectItems[i].data["MATERIALSPEC"],
                            AttributeCode: selectItems[i].data["ATTRIBUTECODE"],
                            AttributeDesc: selectItems[i].data["ATTRIBUTEDESC"],
                            UnitId: selectItems[i].data["UNITID"],
                            UnitName: selectItems[i].data["UNITNAME"],
                            DealUnitId: selectItems[i].data["DEALUNITIID"],
                            DealUnitName: selectItems[i].data["DEALUNITNAME"],
                            DealUnitNo: selectItems[i].data["DEALUNITNO"],
                            SalTaxPrice: selectItems[i].data["SALETAXPRICE"],
                            SaleNoTaxPrice: selectItems[i].data["SALENOTAXPRICE"],
                            WareoutSaleQty: selectItems[i].data["WAREOUTSALQTY"],
                            WareOutQty: selectItems[i].data["WAREOUTQTY"],
                            InvoiceSalQty: selectItems[i].data["INVOICESALQTY"],
                            NoInvoiceSalQty: selectItems[i].data["NOINVOICESALQTY"],
                            InvoiceQty: selectItems[i].data["INVOICEQTY"],
                            NoinvoiceQty: selectItems[i].data["NOINVOICEQTY"],
                            HasRedSalQty: selectItems[i].data["HASREDSALQTY"],
                            HasReddQty: selectItems[i].data["HASREDDQTY"],
                        });
                    }
                }
                if(records.length==0)
                {
                    Ext.Msg.alert("系统提示", "请选择明细数据！");
                    return;
                }
                else
                {
                    this.fillReturnData.call(this,records);
                    this.win.close();
                }
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
                Ext.Msg.alert("系统提示", "不能新增行！");
            }
            break;
        //case LibEventTypeEnum.BeforeDeleteRow:
        //    if (e.dataInfo.tableIndex == 1) {
        //        e.dataInfo.cancel = true;
               
        //    }
        

    }
}

proto.fillData = function (returnData) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'FINARSALDATAFUNCDETAILGrid');
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set("ROW_ID", i + 1);
                newRow.set("ROWNO", i + 1)
                newRow.set("WAREOUTNO", info.WareOutNo);
                newRow.set("WAREOUTROWID", info.WareOutRowId);
                newRow.set("SALEORDERNO", info.SaleOrderNo);
                newRow.set("SALEORDERROWID", info.SaleOrderRowId);
                newRow.set("CONTRACTNO", info.ContractNo);
                newRow.set("MATERIALID", info.MaterialId);
                newRow.set("MATERIALNAME", info.MaterialName);
                newRow.set("ATTRIBUTEID", info.AttributeId);
                newRow.set("ATTRIBUTENAME", info.AttributeName);
                newRow.set("MATERIALSPEC", info.MaterialSpec);
                newRow.set("ATTRIBUTECODE", info.AttributeCode);
                newRow.set("ATTRIBUTEDESC", info.AttributeDesc);
                newRow.set("UNITID", info.UnitId);
                newRow.set("UNITNAME", info.UnitName);
                newRow.set("DEALUNITIID", info.DealUnitId);
                newRow.set("DEALUNITNAME", info.DealUnitName);
                newRow.set("DEALUNITNO", info.DealUnitNo);
                newRow.set("WAREOUTSALQTY", info.WareoutSaleQty);
                newRow.set("WAREOUTQTY", info.WareOutQty);
                newRow.set("INVOICESALQTY", info.InvoiceSalQty);
                newRow.set("NOINVOICESALQTY", info.NoInvoiceSalQty);
                newRow.set("INVOICEQTY", info.InvoiceQty);
                newRow.set("NOINVOICEQTY", info.NoinvoiceQty);
                newRow.set("SALETAXPRICE", info.SalTaxPrice);
                newRow.set("SALENOTAXPRICE", info.SaleNoTaxPrice);
                newRow.set("HASREDSALQTY", info.HasRedSalQty);
                newRow.set("HASREDDQTY", info.HasReddQty);
                newRow.set("FIGURENO", info.FigureNo);
                newRow.set("TEXTUREID", info.TextureId);
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

proto.fillReturnData = function (returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = proto.fromObj[0].dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        //proto.fromObj[0].deleteAll(1);
        var grid = Ext.getCmp(proto.winId + 'FINARSALEINVOICEDETAILGrid');
        var table = proto.fromObj[0].dataSet.getTable(0);
        var billno = table.data.items[0].data["BILLNO"];
        var l=salInvoiceData.length;
        for (var i = 0; i < returnList.length; i++) {
            var info = returnList[i];
            var newRow = proto.fromObj[0].addRowForGrid(grid);
            newRow.set("BILLNO", billno);
            newRow.set("ROW_ID", l+i+ 1);
            newRow.set("ROWNO", l+i+ 1);
            newRow.set("SALESTOCKOUTNO", info.WareOutNo);
            newRow.set("SALESTOCKOUTROWID", info.WareOutRowId);
            newRow.set("SALEORDERNO", info.SaleOrderNo);
            newRow.set("SALEORDERROWID", info.SaleOrderRowId);
            newRow.set("CONTRACTNO", info.ContractNo);
            newRow.set("PRODUCTID", info.MaterialId);
            newRow.set("PRODUCTNAME", info.MaterialName);
            newRow.set("ATTRIBUTEID", info.AttributeId);
            newRow.set("ATTRIBUTENAME", info.AttributeName);
            newRow.set("PRODUCTSPEC", info.MaterialSpec);
            newRow.set("ATTRIBUTECODE", info.AttributeCode);
            newRow.set("ATTRIBUTEDESC", info.AttributeDesc);
            newRow.set("UNITID", info.UnitId);
            newRow.set("UNITNAME", info.UnitName);
            newRow.set("DEALUNITID", info.DealUnitId);
            newRow.set("DEALUNITNAME", info.DealUnitName);
            newRow.set("DEALUNITNO", info.DealUnitNo);
            newRow.set("TAXRATE", taxRate);
            if (!isRed)
            {
                newRow.set("INVOICESALQTY", info.NoInvoiceSalQty);
                newRow.set("INVOICEQTY", info.NoInvoiceSalQty);
            }
            else
            {
                newRow.set("INVOICESALQTY", info.InvoiceSalQty - info.HasRedSalQty);
                newRow.set("INVOICEQTY", info.InvoiceSalQty - info.HasRedSalQty);
            }
            newRow.set("SALETAXPRICE", info.SalTaxPrice);
            newRow.set("SALENOTAXPRICE", info.SaleNoTaxPrice);
            newRow.set("FIGURENO",info.FigureNo);
            newRow.set("TEXTUREID", info.TextureId);
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}


