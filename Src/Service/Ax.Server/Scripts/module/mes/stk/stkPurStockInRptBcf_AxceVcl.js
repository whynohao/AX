stkPurStockInRptBcf_AxceVcl = function () {
    Ax.vcl.LibVclRpt.apply(this, arguments);
};
var proto = stkPurStockInRptBcf_AxceVcl.prototype = Object.create(Ax.vcl.LibVclRpt.prototype);
proto.constructor = stkPurStockInRptBcf_AxceVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclRpt.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            var curPks = [];
            curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
            if (curPks.length > 0) {
                Ax.utils.LibVclSystemUtils.openBill('stk.PurStockIn', BillTypeEnum.Bill, "外购入库单", BillActionEnum.Browse, undefined, curPks);
            }
    }
}