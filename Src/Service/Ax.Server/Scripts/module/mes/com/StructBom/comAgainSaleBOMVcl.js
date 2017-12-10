comAgainSaleBOMVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = comAgainSaleBOMVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comAgainSaleBOMVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.fieldName == 'DATE') {
                e.dataInfo.dataRow.set('DATE', e.dataInfo.value);
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
            if (e.dataInfo.fieldName === "AgainSaleBOM") {//模拟生成过账表

                var data = this.invorkBcf("againSaleBom", [this.dataSet.getTable(0).data.items[0].get('DATE'), this.dataSet.getTable(0).data.items[0].get('MATERIALID')]);

            }
            else if (e.dataInfo.fieldName === "ReCreateSaleBomByBatch") {
                var data = this.invorkBcf("ReCreateSaleBomByBatch", [this.dataSet.getTable(0).data.items[0].get('DATE'), this.dataSet.getTable(0).data.items[0].get('PRODUCTRULEID')]);
            }
            break;

    }
}