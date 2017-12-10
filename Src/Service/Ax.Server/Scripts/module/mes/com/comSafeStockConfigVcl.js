comSafeStockConfigVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
}
var proto = comSafeStockConfigVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comSafeStockConfigVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.fieldName == 'STARTDATE') {
                if (e.dataInfo.value > e.dataInfo.dataRow.get('ENDDATE') && e.dataInfo.dataRow.get('ENDDATE')>0) {
                    e.dataInfo.cancel = true;
                    alert("开始日期不能大于结束日期！");
                }
            }
            if (e.dataInfo.fieldName == 'ENDDATE') {
                if (e.dataInfo.value < e.dataInfo.dataRow.get('STARTDATE')) {
                    e.dataInfo.cancel = true;
                    alert("结束日期不能小于开始日期！");
                }
            }
            this.forms[0].updateRecord(e.dataInfo.dataRow);
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == 'btnBuild') {//生成过账表
                var store = this.dataSet.getTable(1).data.items;
                var data = [];
                for (var i = 0; i < store.length; i++) {
                    var info = {};
                    if (store[i].get('MATERIALID'))
                        info.MATERIALID = store[i].get('MATERIALID');
                    else {
                        alert("物料不能为空！");
                        break;
                    }
                    if (store[i].get('STARTDATE') > 0)
                        info.STARTDATE = store[i].get('STARTDATE');
                    else {
                        alert("开始日期必须小于零！");
                        break;
                    }
                    if (store[i].get('ENDDATE') > 0)
                        info.ENDDATE = store[i].get('ENDDATE');
                    else {
                        alert("结束日期必须大于零！");
                        break;
                    }
                    if (store[i].get('SAFESTOCKQTY') > 0)
                        info.SAFESTOCKQTY = store[i].get('SAFESTOCKQTY');
                    else {
                        alert("安全库存必须大于零！");
                        break;
                    }
                    data.push(info);
                }
                if (data.length > 0)
                    this.invorkBcf('BtnBuild', [data]);
            }
            break;
    }
}