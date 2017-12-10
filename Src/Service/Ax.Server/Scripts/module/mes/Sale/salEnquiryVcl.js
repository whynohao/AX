salEnquiryVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = salEnquiryVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = salEnquiryVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo.value != null) {
            switch (e.dataInfo.fieldName) {
                case 'QUANTITY':
                    e.dataInfo.dataRow.set("AMOUNT", e.dataInfo.dataRow.get("PRICE") * e.dataInfo.value);
                    break;
                case 'PRICE':
                    e.dataInfo.dataRow.set("AMOUNT", e.dataInfo.dataRow.get("QUANTITY") * e.dataInfo.value);
                    break;
                case 'AMOUNT':
                    e.dataInfo.dataRow.set("PRICE",e.dataInfo.value/e.dataInfo.dataRow.get("QUANTITY"));
                    break;
                default:
                    break;
            }
        }
        else if (e.dataInfo && e.dataInfo.tableIndex == 0) {
            var fieldInfos = e.dataInfo.fieldName;
            if (fieldInfos == "TYPEID" || fieldInfos == "CONTACTSOBJECTID" || fieldInfos == "FROMBILLNO" || fieldInfos == "AMOUNT" || fieldInfos == "OFFERAMOUNT") {
                e.dataInfo.curForm.updateRecord(e.dataInfo.dataRow);
            }

        }
    }
}