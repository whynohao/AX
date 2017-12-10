comSopInfoVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = comSopInfoVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comSopInfoVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
        if (e.dataInfo.tableIndex == 1) {
            if (e.dataInfo.fieldName == 'ATTRIBUTEDETAIL') {
                if (e.dataInfo.dataRow.get('ATTRIBUTEROWID') == '' ) {
                    e.dataInfo.cancel = true;
                    Ext.Msg.alert("提示", "不存在特征，不需要配置特征明细表!");
                }
            }
        }
        //case LibEventTypeEnum.Validating:
        //    if (e.dataInfo.tableIndex == 1) {
        //        if (e.dataInfo.fieldName == 'ATTRIBUTEROWID' || e.dataInfo.fieldName == 'ATTRITEMROWID') {
        //            if (e.dataInfo.dataRow.get('RELYITEM') == 0 || e.dataInfo.dataRow.get('RELYITEM') == 2) {
        //                e.dataInfo.cancel = true;
        //                Ext.Msg.alert("提示", "不需要填写特征!");
        //            }
        //        }
        //        if (e.dataInfo.fieldName == 'WORKPROCESSID') {
        //            if (e.dataInfo.dataRow.get('RELYITEM') == 0 || e.dataInfo.dataRow.get('RELYITEM') == 1) {
        //                e.dataInfo.cancel = true;
        //                Ext.Msg.alert("提示", "不需要填写工序!");
        //            }
        //        }
        //    }


    }
}