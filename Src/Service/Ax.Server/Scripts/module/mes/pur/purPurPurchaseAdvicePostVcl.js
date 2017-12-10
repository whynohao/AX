purPurPurchaseAdvicePostVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
};

var proto = purPurPurchaseAdvicePostVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = purPurPurchaseAdvicePostVcl;
proto.winId = null;
proto.fromObj = null;
proto.getType = 0;



proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);

    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "btnBulidPlan") {
            var grid = Ext.getCmp(this.winId + 'PURPURPURCHASEADVICEPOSTGrid');
            var records = grid.getView().getSelectionModel().getSelection();
            if (records.length == 0) {
                alert("请选择载入的明细！");
            }
            else {
                var List = [];
                for (var i = 0; i < records.length; i++) {
                    var record = records[i].data;
                    List.push({
                        CREATEDATE: record["CREATEDATE"],
                        SINGLEDATE: record["MATERIALID"],
                        LOTNO: record["MATSTYLE"],
                        MATERIALID: record["ATTRIBUTECODE"],
                        ATTRIBUTECODE: record["ATTRIBUTEDESC"],
                        ATTRIBUTEDESC: record["NEEDQUANTITY"],
                        NEEDQUANTITY: record[""],
                        PLANSTARTDATE: record[""]
                    });
                }
                gridName = "PURPURCHASEADVICEDATAFUNCDETAIL";
                Ax.utils.LibVclSystemUtils.openDataFunc("pur.PurchaseAdviceDataFunc", "任务分配");
            }
        }
    }
}
