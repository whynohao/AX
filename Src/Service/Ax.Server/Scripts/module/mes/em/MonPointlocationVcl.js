MonPointlocationVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = MonPointlocationVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = MonPointlocationVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                var fieldName = e.dataInfo.fieldName;
                var masterRow = this.dataSet.getTable(0).data.items[0];
                if (this.billAction == BillActionEnum.Modif) {
                    if (fieldName == "POINTID") {
                        this.forms[0].loadRecord(masterRow);
                        Ext.Msg.alert("系统提示", "点位代码不允许修改！");
                        //子表代码 都还原成原值
                        var store1 = this.dataSet.getTable(1).data;
                        var length1 = store1.items.length;
                        for (var i = 0; i < length1; i++) {
                            store1.items[i].set("POINTID", oldvalue);
                        }
                    }
                }

                if (fieldName == "POINTNAME") {
                    this.forms[0].updateRecord(masterRow);
                }

                if (fieldName == "MAXVALUE") {
                    var max = e.dataInfo.value;
                    var oldvalue = e.dataInfo.oldValue
                    var min = masterRow.data["MINVALUE"];
                    if (max <= min) {
                        e.dataInfo.dataRow.set("MAXVALUE", oldvalue);
                        this.forms[0].updateRecord(masterRow);
                        Ext.Msg.alert("系统提示", "点位监控最大值 " + max + "必须 大于 最小值 " + min + "！");
                    }
                }
                if (fieldName == "MINVALUE") {
                    var min = e.dataInfo.value;
                    var oldvalue = e.dataInfo.oldValue
                    var max = masterRow.data["MAXVALUE"];
                    if (max <= min) {
                        e.dataInfo.dataRow.set("MINVALUE", oldvalue);
                        this.forms[0].updateRecord(masterRow);
                        Ext.Msg.alert("系统提示", "点位监控最大值 " + max + "必须 大于 最小值 " + min + "！");
                    }
                }
            }
            break;

            //按钮直接新增记录体 后反填到记录体下拉框中
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "AddNewRecord") {
                if (this.isEdit) {
                    var masterRow = this.dataSet.getTable(0).data.items[0];
                    var PointId = masterRow.get("POINTID");
                    var PointName = masterRow.get("POINTNAME");

                    if (PointId != null && PointId != '' && PointName != null && PointName != '') {
                        if (this.invorkBcf("CheckRecord", [PointId])) {
                            var returnData = this.invorkBcf("AddNewRecord", [PointId, PointName]);

                            var list = returnData['record'];
                            var ctr1 = Ext.getCmp("RECORDID0_" + vcl.winId);
                            ctr1.store.add({ Id: list[0].RECORDID, Name: list[0].RECORDNAME });
                            ctr1.select(list[0].RECORDID);
                            this.forms[0].updateRecord(masterRow);
                        }
                        else { Ext.Msg.alert("系统提示", "已经存 " + PointId + " 的记录体或 "+PointId+" 点位的命名不符合以字母打头至少包含两位字符！"); }
                    }
                    else {
                        Ext.Msg.alert("系统提示", "请先填写点位代码和点位名称！");
                    }
                }
                else {
                    Ext.Msg.alert("系统提示", "非编辑状态下不可操作！");
                }
            }
            break;

        //明细不允许手工新增行
        case LibEventTypeEnum.BeforeAddRow:
            e.dataInfo.cancel = true;
            break;

        //明细不允许手工删除行
        case LibEventTypeEnum.BeforeDeleteRow:
            e.dataInfo.cancel = true;
            break;
    }
}