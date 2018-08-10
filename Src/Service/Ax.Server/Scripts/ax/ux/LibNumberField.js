
Ext.define('Ax.ux.form.LibNumberField', {
    extend: 'Ext.form.field.Number',
    alias: 'widget.libNumberField',
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
    enableKeyEvents: true,
    autoStripChars: true,
    initState: true,
    selectOnFocus: true,
    stepValue:1,//微调时的步进值 Zhangkj 20170227
    initComponent: function () {
        this.id = this.name + this.tableIndex + '_' + DesktopApp.ActiveWindow;
        //var formaterCache = DesktopApp.FormaterCache;
        this.doForamt = function (self) {
            if (self.formatField) {
                //数据载入时，初始化decimalPrecision
                var dataInfo = Ax.Control.LibDataInfo.getDataInfo(self);
                var temps = self.formatField.split('.');
                var index = temps[0].charCodeAt() - 'A'.charCodeAt();
                var value;
                if (self.tableIndex == index) {
                    value = dataInfo.dataRow.get(temps[1]);
                }
                else {
                    var grid = dataInfo.curGrid;
                    if (grid!=undefined&&grid.parentGrid) {
                        do {
                            if (grid.parentGrid.tableIndex == index) {
                                unitId = grid.parentRow.get(temps[1]);
                                break;
                            }
                            grid = grid.parentGrid;
                        } while (grid);
                    } else {
                        if (self.win === undefined)
                            self.win = Ext.getCmp(DesktopApp.ActiveWindow);
                        if (self.win && self.win.vcl!=undefined)
                            value = self.win.vcl.dataSet.getTable(index).data.items[0].get(temps[1]);
                    }
                }
                if (value !== undefined && value != '') {
                    var decimalPrecision = formaterCache.getUnitData(value);
                    if (decimalPrecision !== undefined && this.decimalPrecision != decimalPrecision) {
                        this.decimalPrecision = decimalPrecision;
                    }
                }
            } else if (this.numType) {
                switch (this.numType) {
                    case 1:
                        this.decimalPrecision = 4;
                        break;
                    case 2:
                        this.decimalPrecision = formaterCache.getPriceData();
                        break;
                    case 3:
                        this.decimalPrecision = formaterCache.getAmountData();
                        break;
                    case 4:
                        this.decimalPrecision = formaterCache.getTaxRateData();
                        break;
                }
            }
        };
        this.callParent();
    },
    listeners: {
        keydown: function (self, e, eOpts) {
            if (!this.readOnly) {
                if (this.win === undefined)
                    this.win = self.up('window');
                if (this.win === undefined)
                    this.win = self.up('[isVcl=true]');
                if (this.win && this.win.vcl !== undefined && !this.win.vcl.isEdit) {
                    e.stopEvent();
                } else {
                    this.doForamt(this);
                }
            }
        }
    },
    onSpinUp: function () {
        this.setValue(Number(this.getValue()) + this.stepValue);
    },
    onSpinDown: function () {
        this.setValue(Number(this.getValue()) - this.stepValue);
    },
    onBlur: function (e) {
        var me = this;
        if (!me.readOnly) {
            if (me.up('form')) {
                if (!me.validating(me)) {
                    me.focus(false, true);
                    return;
                }
            }
        }
        me.callParent([e]);
    },
    setRawValue: function (v) {
        if (v != '' && this.initState) {
            this.doForamt(this);
            this.initState = false;
        }
        if (v && this.allowDecimals) {
            v = typeof v == 'number' ? v : parseFloat(String(v).replace(this.decimalSeparator, "."));
            arguments[0] = isNaN(v) ? '' : v.toFixed(this.decimalPrecision).replace(".", this.decimalSeparator);
        }
        return this.callParent(arguments);
    },
    setValue: function (v) {
        if (v != '' && this.initState) {
            if (this.win === undefined)
                this.win = this.up('window');
            if (this.win === undefined)
                this.win = this.up('[isVcl=true]');
            if (this.win && this.win.vcl !== undefined && this.win.vcl.isEdit) {
                this.doForamt(this);
                this.initState = false;
            }
        }
        return this.callParent(arguments);
    }
});
