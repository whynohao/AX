/// <reference path="LibVclDataBase.js" />

Ax.vcl.LibVclGrid = function () {
    Ax.vcl.LibVclDataBase.apply(this, arguments);
    this.isEdit = false;
    this.queryCondition = {};
    this.canAdd;
    this.canDelete;
};
var proto = Ax.vcl.LibVclGrid.prototype = Object.create(Ax.vcl.LibVclDataBase.prototype);
proto.constructor = Ax.vcl.LibVclGrid;

proto.doEdit = function (queryCondition) {
    var assistObj = {};
    var data = this.invorkBcf("Edit", [queryCondition], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(true, BillActionEnum.Modif, data);
    return success;
};

proto.doSave = function () {
    var assistObj = {};
  
    var data = this.save(undefined, undefined, assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
};

proto.restData = function (isEdit, billAction, data) {
    this.setDataSet(data, false);
    this.isEdit = isEdit;
    this.billAction = billAction;
};

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataBase.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.FormClosing:
            if (this.isEdit)
                this.invorkBcf("RemoveCacheBillData", []);
            break;
    }
};

proto.importData = function (fileName) {
    var assistObj = {};
    var data = this.invorkBcf('ImportData', [fileName], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
}

proto.exportData = function (queryCondition) {
    var fileName = this.invorkBcf('ExportData', [queryCondition]);
    return fileName;
};

proto.formCallBackHandler = function (tag, param) {
    Ax.vcl.LibVclDataBase.prototype.formCallBackHandler.apply(this, arguments);
    if (tag == "SYSTEM_QUERY") {
        this.queryCondition = { QueryFields: param.condition };
        this.browseTo(this.queryCondition);
    }
};

proto.browseByPK = function (curPks) {
    var assistObj = {};
    var data = this.invorkBcf("BrowseByPK", [curPks], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
};