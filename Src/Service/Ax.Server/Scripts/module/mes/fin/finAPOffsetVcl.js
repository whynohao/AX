finAPOffsetVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    me = this;;
};
var proto = finAPOffsetVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finAPOffsetVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
    case LibEventTypeEnum.ButtonClick:
        if (this.isEdit) {
            var contactsobjectId = this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTID'];
            var contactsobjectName = this.dataSet.getTable(0).data.items[0].data['CONTACTSOBJECTNAME'];
            var InvoiceType = this.dataSet.getTable(0).data.items[0].data['BILLTYPE'];
            var isred = 0;
            if (e.dataInfo.fieldName == "BtnLoadBlue") {
                if (contactsobjectId == "") {
                    alert("往来单位不能为空！");
                } else {
                    isred = 0;
                    Ax.utils.LibVclSystemUtils.openDataFunc("fin.APOffsetDataFunc",
                        "蓝单明细",
                        [contactsobjectId, contactsobjectName, InvoiceType, isred, this]);
                }
            }
            if (e.dataInfo.fieldName == "BtnLoadRed") {
                if (contactsobjectId == "") {
                    alert("往来单位不能为空！");
                } else {
                    isred = 1;
                    Ax.utils.LibVclSystemUtils.openDataFunc("fin.APOffsetDataFunc",
                        "红单明细",
                        [contactsobjectId, contactsobjectName, InvoiceType, isred, this]);
                }
            }
        } else {
            Ext.Msg.alert("系统提示", "非编辑状态，不可操作！");
        }
        case LibEventTypeEnum.Validated:
            vcl = me;
            if (e.dataInfo.fieldName == "CONTACTSOBJECTID") {
                Ext.getCmp('BLUEBILLNO0_' + vcl.winId).setValue("");
                Ext.getCmp('BLUEINVOICENO0_' + vcl.winId).setValue("");
                Ext.getCmp('REDBILLNO0_' + vcl.winId).setValue("");
                Ext.getCmp('REDINVOICENO0_' + vcl.winId).setValue("");
                Ext.getCmp('BLUEOFFSETAMOUNT0_' + vcl.winId).setValue(0);
                Ext.getCmp('REDOFFSETAMOUNT0_' + vcl.winId).setValue(0);
            }
            if (e.dataInfo.fieldName == "BILLTYPE") {
                Ext.getCmp('BLUEBILLNO0_' + vcl.winId).setValue("");
                Ext.getCmp('BLUEINVOICENO0_' + vcl.winId).setValue("");
                Ext.getCmp('REDBILLNO0_' + vcl.winId).setValue("");
                Ext.getCmp('REDINVOICENO0_' + vcl.winId).setValue("");
                Ext.getCmp('BLUEOFFSETAMOUNT0_' + vcl.winId).setValue(0);
                Ext.getCmp('REDOFFSETAMOUNT0_' + vcl.winId).setValue(0);
            }
            break;
    }
} 

