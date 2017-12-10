stkSaleStockOutBcfRpt_AxceVcl = function () {
    Ax.vcl.LibVclRpt.apply(this, arguments);
};
var proto = stkSaleStockOutBcfRpt_AxceVcl.prototype = Object.create(Ax.vcl.LibVclRpt.prototype);
proto.constructor = stkSaleStockOutBcfRpt_AxceVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclRpt.prototype.vclHandler.apply(this, arguments);
    var table = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            var curPks = [];
            curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
            if (curPks.length > 0) {
                Ax.utils.LibVclSystemUtils.openBill("stk.SaleStockOut", BillTypeEnum.Bill, "销售出库单", BillActionEnum.Browse, undefined, curPks);
            }
    }
}