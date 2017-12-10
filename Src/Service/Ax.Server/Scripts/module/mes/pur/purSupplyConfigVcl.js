purSupplyConfigVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = purSupplyConfigVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = purSupplyConfigVcl;

var mark = false;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var supplierId = masterRow.get("SUPPLIERID");
            var table = this.dataSet.getTable(1);
            if (e.dataInfo.fieldName == "BtnSaveData") {
                var BuildMatingInfo = [];
                if (table.data.items.length > 0) {
                    for (var i = 0; i < table.data.items.length; i++) {
                        var record = table.data.items[i];
                        BuildMatingInfo.push({
                            MATERIALID: record.get("MATERIALID"),
                            MATERIALNAME: record.get("MATERIALNAME"),
                            PRICE: record.get("PRICE"),
                            VMIQUANTITY: record.get("VMIQUANTITY"),
                            VMIROLLDAY: record.get("VMIROLLDAY"),
                            NEEDCHECK: record.get("NEEDCHECK"),
                            ISUSECHECK: record.get("ISUSECHECK"),
                            PURCHASELEADTIME: record.get("PURCHASELEADTIME"),
                            INSTOCKLEADTIME: record.get("INSTOCKLEADTIME"),
                            SHORTLEADTIME: record.get("SHORTLEADTIME"),
                            LEADTIMEUNIT: record.get("LEADTIMEUNIT"),
                            CHECKTYPE: record.get("CHECKTYPE"),
                            WORKSTATIONCONFIGID: record.get("WORKSTATIONCONFIGID")
                        });
                    }
                    mark = this.invorkBcf('SaveDataPost', [supplierId, BuildMatingInfo]);
                    if (mark) {
                        var store = this.dataSet.getTable(1);
                        store.removeAll();
                        masterRow.set("SUPPLIERID", "");
                        masterRow.set("SUPPLIERNAME", "");
                        this.forms[0].loadRecord(masterRow);
                        Ext.Msg.alert('提示', '配置成功！');
                    }
                }

            }
            break;
        case LibEventTypeEnum.Validated:
            var Row = this.dataSet.getTable(0).data.items[0];
            this.forms[0].updateRecord(Row);
            if (e.dataInfo.tableIndex == 1) {
                var store = this.dataSet.getTable(e.dataInfo.tableIndex);
                var length = store.data.items.length;
                if (e.dataInfo.fieldName == "PURCHASELEADTIME") {
                    var inStockLeadTime = e.dataInfo.dataRow.get("INSTOCKLEADTIME");
                    var purChaseLeadTime = e.dataInfo.value;
                    if (purChaseLeadTime > 0 && inStockLeadTime > 0) {
                        var sub = parseInt(purChaseLeadTime) + parseInt(inStockLeadTime);
                        e.dataInfo.dataRow.set("SHORTLEADTIME", sub);
                    }
                }
                else if (e.dataInfo.fieldName == "INSTOCKLEADTIME") {
                    var purChaseLeadTime = e.dataInfo.dataRow.get("PURCHASELEADTIME");
                    var inStockLeadTime = e.dataInfo.value;
                    if (purChaseLeadTime > 0 && inStockLeadTime > 0) {
                        var sub = parseInt(purChaseLeadTime) + parseInt(inStockLeadTime);
                        e.dataInfo.dataRow.set("SHORTLEADTIME", sub);
                    }
                }

            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                var supplierId = this.dataSet.getTable(0).data.items[0].data["SUPPLIERID"];
                if (Ext.isEmpty(supplierId)) {
                    e.dataInfo.cancel = true;
                    Ext.Msg.alert('提示', '供应商不能为空！');
                }
            }
        case LibEventTypeEnum.Validating:
            var store = this.dataSet.getTable(e.dataInfo.tableIndex);
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var length = store.data.items.length;
            switch (e.dataInfo.fieldName) {
                case "MATERIALID":
                   
                    if (length > 0) {
                        for (var i = 0; i < length; i++) {
                            debugger;
                            if (store.data.items[i].get("MATERIALID") == e.dataInfo.value && store.data.items[i].get("ROW_ID") != e.dataInfo.dataRow.data["ROW_ID"]) {
                                debugger;
                                Ext.Msg.alert('提示', '物料 "' + e.dataInfo.value + '"已经存在于该列表！');
                                e.dataInfo.cancel = true;
                            }
                        }
                    }
                    break;
                case "SUPPLIERID":
                    if (e.dataInfo.value != e.dataInfo.oldValue && e.dataInfo.oldValue != "") {
                        Ext.Msg.confirm('提示', '是否确认修改供应商？', function (button) {
                            if (button == "yes") {
                                store.removeAll();
                            }
                            else if (button == "no") {
                                e.dataInfo.cancel = true;
                            }
                        }, this);
                    }
                    break;

            }

    }
}