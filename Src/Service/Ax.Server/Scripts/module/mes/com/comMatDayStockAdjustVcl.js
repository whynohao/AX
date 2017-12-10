comMatDayStockAdjustVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
    this.chart;
    this.materialGrid;
    this.materialId;
};
var proto = comMatDayStockAdjustVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comMatDayStockAdjustVcl;
proto.doSetParam = function (data) {
    this.materialId = data[0];
    var attributeDescs = this.invorkBcf("GetAttributeCombinations", data);
    var attributeCombinationData = [];
    for (var i = 0; i < attributeDescs.length; i++) {
        var curdata = attributeDescs[i];
        attributeCombinationData.push([curdata["AttributeCode"], curdata["AttributeDesc"]]);
    }
    this.materialGrid.store.loadData(attributeCombinationData);
    console.log(this);
}
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BTNSAFESTOCKADJUST") {
                var bodyData = this.dataSet.getTable(1).data.items;
                var data = new Array();
                for (var i = 0; i < bodyData.length; i++) {
                    data.push({ FDate: bodyData[i].get("FDATE"), MaterialId: bodyData[i].get("MATERIALID"), AttributeCode: bodyData[i].get("ATTRIBUTECODE"), SafeStockNum: bodyData[i].get("SAFESTOCKNUM") });
                }
                this.invorkBcf("SaveMatSafeStockAdjust", [data]);
                this.win.close();
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.fieldName == "SAFESTOCKNUM") {
                e.dataInfo.dataRow.set("SAFESTOCKNUM", e.dataInfo.value);
                var dt = this.dataSet.getTable(1);
                var rowNum = dt.data.items.length;
                var list = [];
                var modelName = "com.MatDayStockAdjustChart";
                for (var i = 0; i < rowNum; i++) {
                    var date = dt.data.items[i].get("FDATE");
                    var needQty = dt.data.items[i].get("NEEDQTY");
                    var stockQuantity = dt.data.items[i].get("STOCKQUANTITY");
                    var safeStockNum = dt.data.items[i].get("SAFESTOCKNUM");
                    list.push(Ext.create(modelName, {
                        time: date,
                        needQty: needQty,
                        stockQuantity: stockQuantity,
                        safeStockNum: safeStockNum
                    }));
                }
                this.chart.store.loadData(list);
            }
            break;
    }
}
proto.fillData = function (data) {
    Ext.suspendLayouts();//关闭ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);
        var masterRow = this.dataSet.getTable(0).data.items[0];
        masterRow.set("GUID", "123");
        this.dataSet.dataMap[0].add("123", masterRow);
        for (var i = 0 ; i < data.length; i++) {
            var newRow = this.addRow(masterRow, 1);
            newRow.set("GUID", "123");
            newRow.set("FDATE", data[i].FDate);
            newRow.set("MATERIALID", data[i].MaterialId);
            newRow.set("MATERIALNAME", data[i].MaterialName);
            newRow.set("MATERIALSPEC", data[i].MaterialSpec);
            newRow.set("ATTRIBUTEID", data[i].AttributeId);
            newRow.set("ATTRIBUTENAME", data[i].AttributeName);
            newRow.set("ATTRIBUTECODE", data[i].AttributeCode);
            newRow.set("ATTRIBUTEDESC", data[i].AttributeDesc);
            newRow.set("FLOATRATE", data[i].FloatRate);
            newRow.set("UPPERLIMIT", data[i].UpperLimit);
            newRow.set("LOWERLIMIT", data[i].LowerLimit);
            newRow.set("NEEDQTY", data[i].NeedQty);
            newRow.set("STOCKQUANTITY", data[i].StockQuantity);
            newRow.set("SAFESTOCKNUM", data[i].SafeStockNum);
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开ext布局
    }
}
