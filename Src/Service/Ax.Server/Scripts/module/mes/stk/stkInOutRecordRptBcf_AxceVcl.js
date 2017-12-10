stkInOutRecordRptBcf_AxceVcl = function () {
    Ax.vcl.LibVclRpt.apply(this, arguments);
};
var proto = stkInOutRecordRptBcf_AxceVcl.prototype = Object.create(Ax.vcl.LibVclRpt.prototype);
proto.constructor = stkInOutRecordRptBcf_AxceVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclRpt.prototype.vclHandler.apply(this, arguments);
    var table = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            var curPks = [];
            var progId;
            var displayName;
            switch (e.dataInfo.dataRow.data["BILLTYPE"] * 1) {
                case 100:
                case 101:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.PurStockIn';
                    displayName = "采购入库单";
                    break;
                case 110:
                case 111:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.PurStockOut';
                    displayName = "采购退货单";
                    break;
                case 210:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.SaleStockOut';
                    displayName = "销售出库单";
                    break;
                case 200:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.SaleBackStockIn';
                    displayName = "销售退货入库单";
                    break;
                case 310:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.ProductionOut';
                    displayName = "生产领料出库单";
                    break;
                case 301:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.ProcessStockIn';
                    displayName = "在制品入库单";
                    break;
                case 300:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.ProductBackStockIn';
                    displayName = "生产退料入库单";
                    break;
                case 400:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.OtherStockIn';
                    displayName = "其他入库单";
                    break;
                case 410:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.OtherStockOut';
                    displayName = "其他出库单";
                    break;
                case 402:
                case 412:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.Allocation';
                    displayName = "调拨单";
                    break;
                case 403:
                case 413:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.Stocktaking';
                    displayName = "盘盈单";
                    break;
                case 405:
                case 415:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.Adjustment';
                    displayName = "调整单";
                    break;
                case 406:
                case 407:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.SparePartIn';
                    displayName = "备品备件入库单";
                    break;
                case 416:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.SparePartOut';
                    displayName = "备品备件出库单";
                    break;
                case 500:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.ReturnStockIn';
                    displayName = "还料单";
                    break;
                case 510:
                    curPks.push(e.dataInfo.dataRow.data["BILLNO"]);
                    progId = 'stk.BorrowStockOut';
                    displayName = "借料单";
                    break;
            }
            if (curPks.length > 0) {
                Ax.utils.LibVclSystemUtils.openBill(progId, BillTypeEnum.Bill, displayName, BillActionEnum.Browse, undefined, curPks);
            }
    }
}