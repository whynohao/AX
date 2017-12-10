/// <reference path="../../../ax/vcl/comm/LibVclGrid.js" />

purImpMatiralStockVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
};
var proto = purImpMatiralStockVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = purImpMatiralStockVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:// 移开光标 触发的事件
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == 'YEAR') {
                    if (e.dataInfo.dataRow.data.MATERIALID != "" && e.dataInfo.value != 0) {
                        for (var i = 0; i < dt.data.length - 1; i++) {
                            if (e.dataInfo.dataRow.data.MATERIALID == dt.data.items[i].data.MATERIALID && e.dataInfo.value == dt.data.items[i].data.YEAR) {
                                Ext.MessageBox.alert("错误提示框", "不可在同一年份出现重复物料，请去掉重复行！");
                                break;
                            }
                        }
                    }
                }
                else if (e.dataInfo.fieldName == 'MATERIALID') {
                    if (e.dataInfo.value != "" && e.dataInfo.dataRow.data.YEAR != 0) {
                        for (var i = 0; i < dt.data.length-1; i++) {
                            if (e.dataInfo.value == dt.data.items[i].data.MATERIALID && e.dataInfo.dataRow.data.YEAR == dt.data.items[i].data.YEAR) {
                                Ext.MessageBox.alert("错误提示框", "不可在同一年份出现重复物料，请去掉重复行！");
                                break;
                            }
                        }
                    }
                }
               
            }
            break;
      
    }
}

