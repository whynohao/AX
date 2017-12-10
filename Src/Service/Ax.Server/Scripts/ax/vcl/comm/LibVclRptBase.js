/// <reference path="LibVclBase.js" />

Ax.vcl.LibVclRptBase = function () {
    Ax.vcl.LibVclBase.apply(this, arguments);
    this.win;
    this.loading = false;
    this.isEdit = false;
};
var proto = Ax.vcl.LibVclRptBase.prototype = Object.create(Ax.vcl.LibVclBase.prototype);
proto.constructor = Ax.vcl.LibVclRptBase;

proto.setDataSet = function (data) {
    this.loading = true;
    try {
        this.dataSet.load(data, this, false);
    } finally {
        this.loading = false;
    }
};

proto.exportData = function (queryCondition) {
    var fileName = this.invorkBcf('ExportData', [queryCondition]);
    return fileName;
};

proto.exportMainData = function (queryCondition) {
    var fileName = this.invorkBcf('ExportMainData', [queryCondition]);
    return fileName;
};

proto.vclHandler = function (sender, e) {
    switch (e.libEventType) {
        case LibEventTypeEnum.FormClosed:
            if (sender.isSubWin) {
                var curGrid = sender.down('grid');
                if (curGrid.parentFieldName) {
                    var ret = curGrid.store.data.items.length > 0;
                    if (ret != curGrid.parentRow.get(curGrid.parentFieldName))
                        curGrid.parentRow.set(curGrid.parentFieldName, ret);
                }
            }
            break;
    }
    Ax.vcl.LibVclBase.prototype.vclHandler.apply(this, arguments);
};
