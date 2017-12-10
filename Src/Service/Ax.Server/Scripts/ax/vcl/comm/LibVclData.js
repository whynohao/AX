/// <reference path="LibVclDataBase.js" />

var BillActionEnum = {
    Browse: 0,
    SaveToDraft: 1,
    AddNew: 2,
    Modif: 3,
    Delete: 4,
    Release: 5,
    Invalid: 6,
    AuditPass: 7,
    AuditUnPass: 8,
    CancelAudit: 9,
    EndCase: 10,
    CancelEndCase: 11,
    SubmitDraft: 12,
    CancelInvalid: 13,
    CancelRelease: 14,
    SubmitAudit: 15,
    WithdrawAudit: 16,
    ApprovePassRow: 17,
    ApproveUnPassRow: 18,
    CancelApproveRow: 19,
    SubmitApproveRow: 20,
    WithdrawApproveRow: 21
};

Ax.vcl.LibVclData = function () {
    Ax.vcl.LibVclDataBase.apply(this, arguments);
    this.currentPk = [];
    this.latestPk = new Ext.util.MixedCollection();
    this.billAction;
    this.isEdit = false;
    this.approveRowForm = null;
};
var proto = Ax.vcl.LibVclData.prototype = Object.create(Ax.vcl.LibVclDataBase.prototype);
proto.constructor = Ax.vcl.LibVclData;

proto.customFuzzySearchTemplate = Ext.emptyFn;
proto.customFuzzySearch = Ext.emptyFn;

proto.getTpl = function () {
    this.tpl = this.invorkBcf('GetViewTemplate', [this.entryParam]);
};

proto.setDataSet = function (data,condition, isAdd) {
    Ax.vcl.LibVclDataBase.prototype.setDataSet.apply(this, arguments);
    if (this.win) {
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var showTxt = Ax.utils.LibVclSystemUtils.getCurrentStateText(masterRow.get('CURRENTSTATE'));
        var curPk = "";
        if (condition != false) {
            curPk = condition[0];
        }
        if (vcl.progId != "com.Material" && vcl.progId!="stk.AccountDetail") {
            if (vcl.tpl.ShowAuditState) {
                var auditTxt = Ax.utils.LibVclSystemUtils.getAuditStateText(masterRow.get('AUDITSTATE'), masterRow.get('FLOWLEVEL'));
                this.win.setTitle(curPk + this.win.progName + '【' + showTxt + '】' + '【' + auditTxt + '】');
            } else
                this.win.setTitle(curPk + this.win.progName + '【' + showTxt + '】');
            if (vcl.billType == BillTypeEnum.Master) {
                if (!masterRow.get('ISVALIDITY')) {
                    const title = this.win.title + '【已失效】'
                    this.win.setTitle(title)
                }
            }
        }
    }
};

proto.restData = function (isEdit, billAction, data) {
    this.setDataSet(data, false);
    this.currentPk = [];
    this.latestPk = new Ext.util.MixedCollection();
    var headTable = this.dataSet.getTable(0);
    var pkStr = headTable.Pks;
    for (var i = 0; i < pkStr.length; i++) {
        var value = headTable.data.items[0].get(pkStr[i]);
        this.currentPk.push(value);
        this.latestPk.add(pkStr[i], value);
    }
    for (var i = 0; i < this.forms.length; i++) {
        var form = this.forms[i];
        var items = form.query("libSearchfield");
        if (items) {
            for (var j = 0; j < items.length; j++) {
                var item = items[j];
                item.store.add({ Id: headTable.data.items[0].get(item.name), Name: headTable.data.items[0].get(item.relName) });
                item.select(headTable.data.items[0].get(item.Name));
            }
        }
        form.loadRecord(headTable.data.items[0]);
    };
    this.isEdit = isEdit;
    this.billAction = billAction;
};

proto.addNew = function () {
    var assistObj = {};
    var data, ret;
    if (this.currentPk.length > 0)
        data = this.invorkBcf("AddNew", [this.entryParam, this.currentPk], assistObj);
    else
        data = this.invorkBcf("AddNew", [this.entryParam], assistObj);
    ret = (assistObj.hasError === undefined || !assistObj.hasError);
    if (ret) {
        this.setDataSet(data, true);
        var lenght = this.forms.length;
        for (var i = 0; i < lenght; i++) {
            var record = this.dataSet.getTable(0).data.items[0];
            this.forms[i].loadRecord(record);
        };
        var pks = this.tpl.Tables[this.dataSet.getTable(0).Name].Pk;
        var masterRow = this.dataSet.getTable(0).data.items[0];
        if (masterRow) {
            for (var i = 0; i < pks.length; i++) {
                this.latestPk.add(pks[i], masterRow.get(pks[i]));
            }
        }
    }
    return ret;
};

proto.doDelete = function (condition) {
    var assistObj = {};
    this.invorkBcf("Delete", [condition], assistObj);
    return (assistObj.hasError === undefined || !assistObj.hasError);
};


proto.doEdit = function (vuexStore) {
    var assistObj = {};
    var data = this.edit(this.currentPk, assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success){
        this.restData(true, this.billAction, data);
        if (vuexStore&&vcl.win&&vuexStore!==true) {
            var windowtab = {
                id: vcl.win.id,
                name: vcl.win.title,
            }
            Ax.utils.LibVclSystemUtils.setWindowtab(vuexStore, storeTypes, windowtab, 1)
        }
    }
    return success;
};

proto.doSave = function (vuexStore) {
    var assistObj = {};
    var data = this.save(this.billAction, this.currentPk, assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success) {
        this.restData(false, BillActionEnum.Browse, data);
        if (vuexStore&&vuexStore!==true) {
            var windowtab = {
                id: vcl.win.id,
                name: vcl.win.title,
            }
            Ax.utils.LibVclSystemUtils.setWindowtab(vuexStore, storeTypes, windowtab, 1)
        }
    }
    return success;
};

proto.doSaveToDraft = function () {
    this.billAction = BillActionEnum.SaveToDraft;
    var data = this.save(this.billAction, this.currentPk);
    this.restData(false, BillActionEnum.Browse, data);
};

proto.doSubmitDraft = function () {
    this.billAction = BillActionEnum.SubmitDraft;
    this.setExtendParam();
    var assistObj = {};
    var data = this.invorkBcf("SubmitDraft", [this.currentPk, undefined, this.extendParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
};

proto.doAudit = function (isPass, downLevel,opinion) {
    if (isPass)
        this.billAction = BillActionEnum.AuditPass;
    else
        this.billAction = BillActionEnum.AuditUnPass;
    this.setExtendParam();
    var assistObj = {};
    var data = this.invorkBcf("Audit", [this.currentPk, isPass, opinion, undefined, downLevel, this.extendParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
};

proto.doCancelAudit = function (downLevel, reasonId) {
    this.billAction = BillActionEnum.CancelAudit;
    this.setExtendParam();
    var assistObj = {};
    var data = this.invorkBcf("CancelAudit", [this.currentPk, undefined, downLevel, reasonId, this.extendParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
};


proto.doSubmitAudit = function (cancel) {
    if (cancel)
        this.billAction = BillActionEnum.WithdrawAudit;
    else
        this.billAction = BillActionEnum.SubmitAudit;
    this.setExtendParam();
    var assistObj = {};
    var data = this.invorkBcf("SubmitAudit", [this.currentPk, cancel, undefined, this.extendParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
};


proto.doTakeRelease = function (cancel) {
    if (cancel)
        this.billAction = BillActionEnum.Release;
    else
        this.billAction = BillActionEnum.CancelRelease;
    this.setExtendParam();
    var assistObj = {};
    var data = this.invorkBcf("TakeRelease", [this.currentPk, cancel, undefined, this.extendParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
};


proto.doInvalid = function (cancel) {
    if (cancel)
        this.billAction = BillActionEnum.CancelInvalid;
    else
        this.billAction = BillActionEnum.Invalid;
    this.setExtendParam();
    var assistObj = {};
    var data = this.invorkBcf("Invalid", [this.currentPk, cancel, undefined, this.extendParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
};

proto.doEndCase = function (cancel) {
    if (cancel)
        this.billAction = BillActionEnum.CancelEndCase;
    else
        this.billAction = BillActionEnum.EndCase;
    this.setExtendParam();
    var assistObj = {};
    var data = this.invorkBcf("EndCase", [this.currentPk, cancel, undefined, this.extendParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
};

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataBase.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.FormClosing:
            if (this.isEdit)
                this.invorkBcf("RemoveCacheBillData", [this.currentPk]);
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex != 0) {
                if (e.dataInfo.dataRow.get('AUDITSTATE') == 2) {
                    alert('行项已审核不能删除');
                    e.dataInfo.cancel = true;
                }
                else if (e.dataInfo.dataRow.get('AUDITSTATE') == 1) {
                    alert('行项已提交审核不能删除');
                    e.dataInfo.cancel = true;
                }
            }
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex != 0) {
                if (e.dataInfo.dataRow.get('AUDITSTATE') == 2) {
                    alert('行项已审核不能修改');
                    e.dataInfo.cancel = true;
                }
            }
            break;
    }
};

proto.importData = function (fileName) {
    var assistObj = {};
    var data = this.invorkBcf('ImportData', [fileName,this.entryParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
}

proto.exportData = function () {
    var fileName = this.invorkBcf('ExportData', [this.currentPk]);
    return fileName;
};

proto.submitApproveRow = function (cancel, rowList) {
    if (cancel)
        this.billAction = BillActionEnum.WithdrawApproveRow;
    else
        this.billAction = BillActionEnum.SubmitApproveRow;
    var assistObj = {};
    var data = this.invorkBcf("SubmitApproveRow", [this.currentPk, cancel, rowList], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
}

proto.auditRow = function (cancel, rowList, unPassLevel) {
    if (cancel)
        this.billAction = BillActionEnum.ApproveUnPassRow;
    else
        this.billAction = BillActionEnum.ApprovePassRow;
    var assistObj = {};
    var data = this.invorkBcf("AuditRow", [this.currentPk, cancel, rowList, unPassLevel], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
}

proto.cancelAuditRow = function (rowList, unPassLevel, reasonId) {
    this.billAction = BillActionEnum.CancelApproveRow;
    var assistObj = {};
    var data = this.invorkBcf("CancelAuditRow", [this.currentPk, rowList, unPassLevel, reasonId], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        this.restData(false, BillActionEnum.Browse, data);
    return success;
};

proto.browseToVersion = function (condition, versionTime) {
    var data = this.invorkBcf("BrowseToVersion", [condition, versionTime]);
    this.setDataSet(data, false);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    for (var i = 0; i < this.forms.length; i++) {
        this.forms[i].loadRecord(masterRow);
    };
}
