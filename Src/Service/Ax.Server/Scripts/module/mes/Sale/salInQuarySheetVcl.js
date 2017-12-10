salInQuarySheetVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = salInQuarySheetVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = salInQuarySheetVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "EnquiryOut") {
            if (!this.isEdit) {
                var headTableRow = this.dataSet.getTable(0).data.items[0];
                var billNo = headTableRow.data["BILLNO"];

                window.open("ExportSalEnquiry.aspx?billNo=" + billNo);
                if (field == null) { Ext.alert.alert("导出提示", "导出的数据为空！"); }

            }
            else { Ext.Msg.alert("系统提示", "编辑状态不能生成单据！"); }
        }
    }
}