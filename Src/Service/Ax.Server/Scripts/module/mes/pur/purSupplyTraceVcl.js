/// <reference path="../../../ax/vcl/comm/LibVclGrid.js" />

purSupplyTraceVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
};
var proto = purSupplyTraceVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = purSupplyTraceVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:// 移开光标 触发的事件
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == 'PUBILLNO') {
                    if (e.dataInfo.dataRow.data.MATERIALID != "" && e.dataInfo.dataRow.data.WORKNO != "" && e.dataInfo.value != "") {
                        for (var i = 0; i < dt.data.length - 1; i++) {
                            if (e.dataInfo.dataRow.data.MATERIALID == dt.data.items[i].data.MATERIALID && e.dataInfo.dataRow.data.WORKNO == dt.data.items[i].data.WORKNO && e.dataInfo.value == dt.data.items[i].data.PUBILLNO) {
                                Ext.MessageBox.alert("错误提示框", "不可在同一作业号,同一订单中出现重复物料，请去掉重复行！");
                                break;
                            }
                        }
                    }
                }
                else if (e.dataInfo.fieldName == 'MATERIALID') {
                    if (e.dataInfo.value != "" && e.dataInfo.dataRow.data.PUBILLNO != "" && e.dataInfo.dataRow.data.WORKNO != "") {
                        for (var i = 0; i < dt.data.length - 1; i++) {
                            if (e.dataInfo.value == dt.data.items[i].data.MATERIALID && e.dataInfo.dataRow.data.WORKNO == dt.data.items[i].data.WORKNO && e.dataInfo.dataRow.data.PUBILLNO == dt.data.items[i].data.PUBILLNO) {
                                Ext.MessageBox.alert("错误提示框", "不可在同一作业号,同一订单中出现重复物料，请去掉重复行！");
                                break;
                            }
                        }
                    }
                }
                else if (e.dataInfo.fieldName == 'WORKNO') {
                    if (e.dataInfo.value != "" && e.dataInfo.dataRow.data.PUBILLNO != "" && e.dataInfo.dataRow.data.MATERIALID != "") {
                        for (var i = 0; i < dt.data.length - 1; i++) {
                            if (e.dataInfo.value == dt.data.items[i].data.WORKNO && e.dataInfo.dataRow.data.MATERIALID == dt.data.items[i].data.MATERIALID && e.dataInfo.dataRow.data.PUBILLNO == dt.data.items[i].data.PUBILLNO) {
                                Ext.MessageBox.alert("错误提示框", "不可在同一作业号,同一订单中出现重复物料，请去掉重复行！");
                                break;
                            }
                        }
                    }
                }
            }
            break;
    }
}

