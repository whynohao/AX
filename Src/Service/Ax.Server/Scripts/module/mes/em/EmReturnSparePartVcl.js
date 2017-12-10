
EmReturnSparePartVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = EmReturnSparePartVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = EmReturnSparePartVcl;

function getInfo(returnValue) {
    var list = returnValue['EmReturnSparePartInfo'];
    this.dataSet.getTable(1).removeAll(); //删除要加载的Grid数据
    if (list !== undefined && list.length > 0) {
        var grid = Ext.getCmp(this.winId + 'EMRETURNSPAREPARTDETAILGrid'); //要加载数据的表名字 + Grid
        for (var i = 0; i < list.length; i++) {
            var info = list[i];
            var newRow = this.addRowForGrid(grid);
            newRow.set('ROW_ID', info.Row_id);
            newRow.set('MATERIALID', info.MaterialId);
            newRow.set('MATERIALNAME', info.MaterialName);
            newRow.set('QUANTITY', info.Quantity);
        }
    }
};

proto.checkFieldValue = function (curRow, returnValue, tableIndex, fieldName) {
    Ax.vcl.LibVclData.prototype.checkFieldValue.apply(this, arguments);
    if (tableIndex == 0) {
        switch (fieldName) {
            case 'FROMBILLNO':
                getInfo.call(this, returnValue);
                break;
        }
    }
};
