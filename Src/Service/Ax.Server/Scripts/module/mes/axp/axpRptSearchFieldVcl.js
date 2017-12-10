/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：单据审核流配置的vcl脚本
 * 创建标识：Chenq 2017/03/21
 *
 *
************************************************************************/
axpRptSearchFieldVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = axpRptSearchFieldVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = axpRptSearchFieldVcl;
//根据功能代码查找主表下的所有字段并设置到指定的comboBox下拉列表的选项中
proto.getFields = function (progId) {
    if (!progId || progId == '') {
        return;
    }
    var data = [];
    Ext.Ajax.request({
        url: '/billSvc/getRptFields',
        async: false,
        jsonData: { progId: progId },
        method: 'POST',
        timeout: 60000,
        success: function (res) {
            data = Ext.decode(res.responseText).GetRptFieldsResult
        }
    });
    return data;
};

proto.checkFields = function (curGrid, masterRow) {
    var filterColumns = Ext.Array.filter(curGrid.columns, function (item) {
        //过滤,查找动态部门字段列
        if (item && item.dataIndex && item.dataIndex == 'SEARCHFIELD') {
            return true;
        } else {
            return false;
        }
    }, this);//this表示作用域
    if (!filterColumns || filterColumns.length == 0)
        return;
    var nameColumn = filterColumns[0];
    var progId = masterRow.get('PROGID');
    if (!progId || progId == '') {
        if (!nameColumn.getEditor) {
            return;
        }
        var editor = nameColumn.getEditor();
        editor.doQuery = function () {
            if (this.win === undefined)
                this.win = this.up('window');
            if (this.win === undefined)
                this.win = this.up('[isVcl=true]');
            this.store.removeAll();//先清空数据
            var ret = Ext.form.field.ComboBox.prototype.doQuery.apply(this, arguments);
            this.remoteData = [];
            this.store.loadData([]);
            this.doAutoSelect();
            return ret;
        }
        return;
    }
    if (!this.axpApproveFlowVclOptionData || this.axpApproveFlowVclOldProgId != progId) {
        //console.log(nameColumn);
        this.axpApproveFlowVclOldProgId = progId;
        var data = this.getFields(progId);
        if (!nameColumn.getEditor) {
            return;
        }
        var editor = nameColumn.getEditor();
        editor.doQuery = function () {
            if (this.win === undefined)
                this.win = this.up('window');
            if (this.win === undefined)
                this.win = this.up('[isVcl=true]');
            this.store.removeAll();//先清空数据
            var ret = Ext.form.field.ComboBox.prototype.doQuery.apply(this, arguments);
            this.remoteData = data;
            this.store.loadData(data);
            this.doAutoSelect();
            return ret;
        }
    }
};

proto.doCheckField = function (tableIndex, fieldName, relSource, relPk, curValue, curRow, curGrid) {
    if (fieldName === 'SEARCHFIELD') {
        var fieldValue = {};
        this.beforeCheckField(tableIndex, fieldName, relSource, fieldValue);
        var curPks = this.getRelPk(fieldName, relPk, tableIndex, curRow, curValue, curGrid);
        var returnValue = '';
        this.checkFieldValue(curRow, returnValue, tableIndex, fieldName);
        this.afterCheckField(curRow, tableIndex, fieldName);
    }
    else {
        Ax.vcl.LibVclData.prototype.doCheckField.apply(this, arguments);
    }
}

proto.doSetParam = function(param){
    if (param) {
        var masterRow = this.dataSet.getTable(0).data.items[0];
        masterRow.set('PROGID', param.progId);
        masterRow.set('PROGNAME', param.progName);
    }
}

proto.afterShow = function (param) {
    var grid = Ext.getCmp(this.winId + this.dataSet.getTable(1).Name + 'Grid');
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set('USERID', window.UserId);
    if (param) {
        masterRow.set('PROGID', param.progId);
        masterRow.set('PROGNAME', param.progName);
        Ext.getCmp('PROGID0_' + this.winId).focus();
        Ext.getCmp('PROGID0_' + this.winId).blur();
    }
    this.checkFields(grid, masterRow);
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.fieldName === 'PROGID') {
                var grid = Ext.getCmp(this.winId + this.dataSet.getTable(1).Name + 'Grid');
                grid.store.reload();
                var masterRow = this.dataSet.getTable(0).data.items[0];
                this.checkFields(grid, masterRow);
            }
    }
}
