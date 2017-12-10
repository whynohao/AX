
Ext.define('Ax.ux.form.LibNumbercolumn', {
    extend: 'Ext.grid.column.Number',
    alias: 'widget.libNumbercolumn',
    initComponent: function () {
        this.callParent();
        this.renderer = function (value, metaData, record, rowIndex, colIndex, store, view) {
            var formaterCache = DesktopApp.FormaterCache;
            var decimalSeparator = '.';
            if (this.formatField) {
                //数据载入时，初始化decimalPrecision
                var temps = this.formatField.split('.');
                var index = temps[0].charCodeAt() - 'A'.charCodeAt();
                var unitId;
                if (this.tableIndex == index) {
                    unitId = record.get(temps[1]);
                }
                else {
                    var grid = view;
                    if (grid.parentGrid) {
                        do {
                            if (grid.parentGrid.tableIndex == index) {
                                unitId = grid.parentRow.get(temps[1]);
                                break;
                            }
                            grid = grid.parentGrid;
                        } while (grid);
                    } else {
                        if (this.win === undefined)
                            this.win = Ext.getCmp(DesktopApp.ActiveWindow);
                        if (this.win)
                            unitId = this.win.vcl.dataSet.getTable(index).data.items[0].get(temps[1]);
                    }
                }
                if (unitId !== undefined && unitId != '') {
                    var decimalPrecision = formaterCache.getUnitData(unitId);
                    var v = value;
                    v = typeof v == 'number' ? v : parseFloat(String(v).replace(decimalSeparator, "."));
                    value = isNaN(v) ? '' : v.toFixed(decimalPrecision).replace(".", decimalSeparator);
                }
            } else if (this.numType) {
                var decimalPrecision;
                switch (this.numType) {
                    case 1:
                        decimalPrecision = 4;
                        break;
                    case 2:
                        decimalPrecision = formaterCache.getPriceData();
                        break;
                    case 3:
                        decimalPrecision = formaterCache.getAmountData();
                        break;
                    case 4:
                        decimalPrecision = formaterCache.getTaxRateData();
                        break;
                }
                var number = 0;
                if (value != "" && (parseInt(value)!=value)) {
                    number = value.toString().split(".")[1].length
                }
                var v = value;
                v = typeof v == 'number' ? v : parseFloat(String(v).replace(decimalSeparator, "."));
                value = isNaN(v) ? '' : v.toFixed(decimalPrecision).replace(".", decimalSeparator);
                if (this.numType == 1 || this.numType == 4) {
                    if (value >= 0) {
                        return '<span style="color:blue;">' + (value * 100).toFixed(decimalPrecision < 2 ? 0 : decimalPrecision - 2).replace(".", decimalSeparator) + '%</span>';
                    } else {
                        return '<span style="color:red;">' + (value * 100).toFixed(decimalPrecision < 2 ? 0 : decimalPrecision - 2).replace(".", decimalSeparator) + '%</span>';
                    }
                }
                if (number != decimalPrecision) {
                    var dataIndex = metaData.column.dataIndex;
                    window.setTimeout(function () {
                        if (dataIndex) {
                            record.set(dataIndex, value);
                        }
                    }, 0)
                }
            }
            return value;
        }
    }
});
