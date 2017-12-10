/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

plsQuarterforecastVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = plsQuarterforecastVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = plsQuarterforecastVcl;



proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1 && e.dataInfo.dataRow != null) {
                var value = e.dataInfo.value;
                if (e.dataInfo.fieldName == "FIRSTLOCALSALES") {
                    value += e.dataInfo.dataRow.get("FIRSTEXPORTSALES");
                    e.dataInfo.dataRow.set('FIRSTTOTALSALES', value);
                }
                if (e.dataInfo.fieldName == "FIRSTEXPORTSALES") {
                    value += e.dataInfo.dataRow.get("FIRSTLOCALSALES");
                    e.dataInfo.dataRow.set('FIRSTTOTALSALES', value);
                }

                if (e.dataInfo.fieldName == "SECONDLOCALSALES") {
                    value += e.dataInfo.dataRow.get("SECONDEXPORTSALES");
                    e.dataInfo.dataRow.set('SECONDTOTALSALES', value);
                }
                if (e.dataInfo.fieldName == "SECONDEXPORTSALES") {
                    value += e.dataInfo.dataRow.get("SECONDLOCALSALES");
                    e.dataInfo.dataRow.set('SECONDTOTALSALES', value);
                }

                if (e.dataInfo.fieldName == "THIRDLOCALSALES") {
                    value += e.dataInfo.dataRow.get("THIRDEXPORTSALES");
                    e.dataInfo.dataRow.set('THIRDTOTALSALES', value);
                }
                if (e.dataInfo.fieldName == "THIRDEXPORTSALES") {
                    value += e.dataInfo.dataRow.get("THIRDLOCALSALES");
                    e.dataInfo.dataRow.set('THIRDTOTALSALES', value);
                }

                if (e.dataInfo.fieldName == "SUMLOCALSALES") {
                    value += e.dataInfo.dataRow.get("SUMEXPORTSALES");
                    e.dataInfo.dataRow.set('SUMTOTALSALES', value);
                }
                if (e.dataInfo.fieldName == "SUMEXPORTSALES") {
                    value += e.dataInfo.dataRow.get("SUMLOCALSALES");
                    e.dataInfo.dataRow.set('SUMTOTALSALES', value);
                }
                //var value = e.dataInfo.value - e.dataInfo.oldValue;
                //value += e.dataInfo.dataRow.get("COUNTPRODUCE");
                //e.dataInfo.dataRow.set('COUNTPRODUCE', value);
            }
            break;
    }
}