finAROffsetVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = finAROffsetVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finAROffsetVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnLoadBlue") {
                if (this.isEdit) {
                    var masterRow = this.dataSet.getTable(0).data.items[0];
                    var contactsObjected = masterRow.get("CONTACTSOBJECTID");
                    if (contactsObjected != "") {
                        Ax.utils.LibVclSystemUtils.openDataFunc("fin.AROffsetDatafunc", "蓝单明细", [this, "FINAROFFSETDATAFUNCDETAIL", false]);
                    }
                    else
                    {
                        Ext.Msg.alert("系统提示", "往来单位不能为空");
                    }
                }
                else {
                    Ext.Msg.alert("系统提示", "非编辑状态，不可操作！");
                }
            }
            if (e.dataInfo.fieldName == "BtnLoadRed") {
                if (this.isEdit) {
                    var masterRow = this.dataSet.getTable(0).data.items[0];
                    var contactsObjected = masterRow.get("CONTACTSOBJECTID");
                    if (contactsObjected != "") {
                        Ax.utils.LibVclSystemUtils.openDataFunc("fin.AROffsetDatafunc", "红单明细", [this, "FINAROFFSETDATAFUNCDETAIL", true]);
                    }
                    else
                    {
                        Ext.Msg.alert("系统提示", "往来单位不能为空");
                    }
                }
                else {
                    Ext.Msg.alert("系统提示", "非编辑状态，不可操作！");
                }
            }
    }
}

