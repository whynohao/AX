stkOutSourcingInVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = stkOutSourcingInVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = stkOutSourcingInVcl;

proto.vclHandler = function (sender, e) {

    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);

    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "CONTRACTCODE") {
                    var bodyTable = this.dataSet.getTable(1).data.items;
                    var masterRow = this.dataSet.getTable(0).data.items[0].data;
                    for (var i = 0; i < bodyTable.length; i++) {
                        var row = bodyTable[i];
                        row.set("CONTRACTCODE", e.dataInfo.value);
                        row.set("CONTRACTNO", masterRow["CONTRACTNO"]);
                    }
                }
                if (e.dataInfo.fieldName == "CONTACTOBJECTID") {
                    var bodyTable = this.dataSet.getTable(1).data.items;
                    var masterRow = this.dataSet.getTable(0).data.items[0].data;
                    for (var i = 0; i < bodyTable.length; i++) {
                        var row = bodyTable[i];
                        row.set("CONTACTOBJECTID", e.dataInfo.value);
                        row.set("CONTACTSOBJECTNAME", masterRow["CONTACTSOBJECTNAME"]);
                    }
                }
            }
            break;
        case LibEventTypeEnum.AddRow:
            var masterRow = this.dataSet.getTable(0).data.items[0].data;
            e.dataInfo.dataRow.set("CONTRACTCODE", masterRow["CONTRACTCODE"]);
            e.dataInfo.dataRow.set("CONTRACTNO", masterRow["CONTRACTNO"]);
            e.dataInfo.dataRow.set("CONTACTOBJECTID", masterRow["CONTACTOBJECTID"]);
            e.dataInfo.dataRow.set("CONTACTSOBJECTNAME", masterRow["CONTACTSOBJECTNAME"]);
            break;
        case LibEventTypeEnum.ButtonClick:
            if (this.isEdit) {
                if (e.dataInfo.fieldName == "BtnLoadData") {
                    //var fromBillNo = this.dataSet.getTable(0).data.items[0].data['FROMBILLNO'];
                    //var contractCode = this.dataSet.getTable(0).data.items[0].data['CONTRACTCODE'];

                    Ax.utils.LibVclSystemUtils.openDataFunc('stk.OutSourcingOutDataFunc', '载入来源单', [this]);
                }
            }
            else {
                Ext.Msg.alert("单据只有在修改状态才能载入数据！");
            }
            break;
    }
}
