emLineAllMonthRptVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
};
var proto = emLineAllMonthRptVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = emLineAllMonthRptVcl;
proto.vclHandler = function (sender, e) {
    
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 0) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 0) {
                e.dataInfo.cancel = true;
            }
        //case LibEventTypeEnum.Validated:
        //    if (e.dataInfo.tableIndex == 0) {
        //        if(e.dataInfo.fieldName == "PRODUCELINEID")
        //        {
        //            var producelineid = e.dataInfo.value;
        //            var year = e.dataInfo.dataRow.get("CURRENTYEAR");
        //            debugger;
        //            var list = this.invorkBcf('GetMonthOeerate', [year, producelineid]);
        //            e.dataInfo.dataRow.set('OEERATE1', list.Oeerate1);
        //            e.dataInfo.dataRow.set('OEERATE2', list.Oeerate2);
        //            e.dataInfo.dataRow.set('OEERATE3', list.Oeerate3);
        //            e.dataInfo.dataRow.set('OEERATE4', list.Oeerate4);
        //            e.dataInfo.dataRow.set('OEERATE5', list.Oeerate5);
        //            e.dataInfo.dataRow.set('OEERATE6', list.Oeerate6);
        //            e.dataInfo.dataRow.set('OEERATE7', list.Oeerate7);
        //            e.dataInfo.dataRow.set('OEERATE8', list.Oeerate8);
        //            e.dataInfo.dataRow.set('OEERATE9', list.Oeerate9);
        //            e.dataInfo.dataRow.set('OEERATE10', list.Oeerate10);
        //            e.dataInfo.dataRow.set('OEERATE11', list.Oeerate11);
        //            e.dataInfo.dataRow.set('OEERATE12', list.Oeerate12);
        //        }
            //}
    }
    
}

   
