plsProduceCollectPlanVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
};
var proto = plsProduceCollectPlanVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = plsProduceCollectPlanVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == 'PLANDETAILDATE') {
                    //alert(typeof e.dataInfo.dataRow.data.CUSTOMTYPE);
                    //e.dataInfo.cancel = true;
                    //e.dataInfo.dataRow.set(e.dataInfo.fieldName, e.dataInfo.oldValue);
                    if (String(e.dataInfo.dataRow.data.CUSTOMTYPE) != "4") {
                        Ext.Msg.alert("提示", "当定制类型为D时才能手动输入.");
                      
                    }
      
                }
              
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {

                if (e.dataInfo.fieldName == 'CUSTOMTYPE') {

                    var CurrentDate0 = String(e.dataInfo.dataRow.data.SALDATE);
                    var CurrentDate1 = CurrentDate0.substring(0, 4) + "-" + CurrentDate0.substring(4, 6) + "-" + CurrentDate0.substring(6, 8);

                    var a = AddDays(CurrentDate1, 1);
                    e.dataInfo.dataRow.set("PLANMATERIALDATE", a);
                    switch (e.dataInfo.value) {

                        case 0:
                            break;
                            //A类型
                        case 1:

                            var b = AddDays(CurrentDate1, 7);
                            e.dataInfo.dataRow.set("PLANDETAILDATE", b);
                            var c = AddDays(CurrentDate1, 8);
                            e.dataInfo.dataRow.set("PLANQUOTADATE", c);

                            break;
                        case 2:

                            var b = AddDays(CurrentDate1, 5);
                            e.dataInfo.dataRow.set("PLANDETAILDATE", b);
                            var c = AddDays(CurrentDate1, 6);
                            e.dataInfo.dataRow.set("PLANQUOTADATE", c)

                            break;
                        case 3:

                            var b = AddDays(CurrentDate1, 3);
                            e.dataInfo.dataRow.set("PLANDETAILDATE", b);
                            var c = AddDays(CurrentDate1, 4);
                            e.dataInfo.dataRow.set("PLANQUOTADATE", c)
                            break;
                        case 4:

                            var b = AddDays(CurrentDate1, 0);
                            e.dataInfo.dataRow.set("PLANDETAILDATE", b);
                            var c = AddDays(CurrentDate1, 1);
                            e.dataInfo.dataRow.set("PLANQUOTADATE", c)
                    }
                }
            }

            break;

    }
}


function AddDays(date, days) {
    var nd = new Date(date);
    nd = nd.valueOf();
    nd = nd + days * 24 * 60 * 60 * 1000;
    nd = new Date(nd);
    var y = nd.getFullYear();
    var m = nd.getMonth() + 1;
    var d = nd.getDate();
    if (m <= 9) m = "0" + m;
    if (d <= 9) d = "0" + d;
    var cdate = String(y) + String(m) + String(d);
    return parseInt(cdate);
}



