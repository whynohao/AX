Ext.ns('Ax.Control')

Ext.define('Ax.ux.LibEventHelper', {
    alias: 'widget.libEventHelper',
    validating: function (self) {
        var ret = true;
        if (this.win === undefined)
            this.win = self.up('window');
        if (this.win === undefined)
            this.win = self.up('[isVcl=true]');
        if (this.win && this.win.vcl) {
            var dataInfo = Ax.Control.LibDataInfo.getDataInfo(self);
            if (dataInfo.value != dataInfo.oldValue) {
                self.win.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.Validating, dataInfo: dataInfo });
                ret = !dataInfo.cancel;
                if (ret) {
                    dataInfo.dataRow.set(dataInfo.fieldName, dataInfo.value);
                    self.win.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.Validated, dataInfo: dataInfo });
                } else {
                    self.setValue(dataInfo.oldValue);
                }
            }
            else
            {
                if (dataInfo.curForm)
                    dataInfo.curForm.loadRecord(dataInfo.dataRow);
            }
        }
        return ret;
    },
    keydown: function (self, e, eOpts) {
        if (!this.readOnly) {
            var win = self.up('window');
            if (win === undefined)
                win = self.up('[isVcl=true]');
            if (win.vcl && (!win.vcl.isEdit && win.vcl.proxy !== true)) {
                e.stopEvent();
            }
        }
    }
});

Ax.Control.LibDataInfo = {
    getDataInfo: function (self) {
        var dataRow, tableIndex, curForm, curGrid;
        curGrid = self.up('grid') || self.up("treepanel");
        if (curGrid) {
            dataRow = self.dataRow || curGrid.getSelectionModel().getLastSelected();
        }
        else {
            curForm = self.up('form');
            if (curForm) {
                dataRow = curForm.getRecord();
            }
        }
        var oldValue;
        if (dataRow)
            oldValue = dataRow.get(self.name);
        var dataInfo = {
            cancel: false,
            value: self.getValue(),
            oldValue: oldValue,
            fieldName: self.name,
            tableIndex: self.tableIndex,
            dataRow: dataRow,
            curForm: curForm,
            curGrid: curGrid
        };
        return dataInfo;
    },
    getDataInfoForGrid: function (self, td, cellIndex, record, tr, rowIndex, e) {
        var name = self.panel.columnManager.columns[cellIndex].dataIndex;
        var dataInfo = {
            cancel: false,
            value: record.get(name),
            fieldName: name,
            tableIndex: self.panel.tableIndex,
            dataRow: record,
            curGrid: self.panel
        };
        return dataInfo;
    }
};