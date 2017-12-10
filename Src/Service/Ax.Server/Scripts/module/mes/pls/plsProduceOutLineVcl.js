
plsProduceOutLineVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = plsProduceOutLineVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = plsProduceOutLineVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated: //grid一行的合计
            if (e.dataInfo.tableIndex == 1) {

                var varFV = e.dataInfo.fieldName;

                if (varFV == "JANPRODUCE" || 
                    varFV == "FEBPRODUCE" || 
                    varFV == "MARPRODUCE" || 
                    varFV == "APRPRODUCE" || 
                    varFV == "MAYPRODUCE" || 
                    varFV == "JUNEPRODUCE" ||
                    varFV == "JULPRODUCE" || 
                    varFV == "AUGPRODUCE" || 
                    varFV == "SEPTPRODUCE" ||
                    varFV == "OCTPRODUCE" || 
                    varFV == "NOVPRODUCE" || 
                    varFV == "DECPRODUCE")
                {
                    var value = e.dataInfo.value - e.dataInfo.oldValue;
                    value += e.dataInfo.dataRow.get("TOTALPRODUCE");
                    e.dataInfo.dataRow.set('TOTALPRODUCE', value);
                }
                if(varFV == "JANOUTVALUE" ||
                   varFV == "FEBOUTVALUE" || 
                   varFV == "MAROUTVALUE" || 
                   varFV == "APROUTVALUE" || 
                   varFV == "MAYOUTVALUE" || 
                   varFV == "JUNEOUTVALUE" || 
                   varFV == "JULOUTVALUE" || 
                   varFV == "AUGOUTVALUE" || 
                   varFV == "SEPTOUTVALUE" || 
                   varFV == "OCTOUTVALUE" || 
                   varFV == "NOVOUTVALUE" ||
                   varFV == "DECOUTVALUE")
                {
                    var value = e.dataInfo.value - e.dataInfo.oldValue;
                    value += e.dataInfo.dataRow.get("TOTALOUTVALUE");
                    e.dataInfo.dataRow.set('TOTALOUTVALUE', value);
                }
            }
            break;
    }
}
