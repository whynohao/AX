
Ext.define('Ax.ux.form.LibFieldControl', {
    extend: 'Ext.form.field.ComboBox',
    alias: 'widget.libFieldControl',
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
    initComponent: function () {
        this.id = this.name + this.tableIndex + '_' + DesktopApp.ActiveWindow;
        this.valueField = 'Id';
        this.displayField = 'Name';
        //this.displayTpl = Ext.create('Ext.XTemplate',
        //                  '<tpl for=".">',
        //                    '<tpl if="Id != &quot;&quot;">',
        //                      '{Id},{Name}',
        //                    '</tpl>',
        //                  '</tpl>'
        //);
        var fuzzyField = Ext.data.Model.schema.getEntity('FuzzyField');
        if (fuzzyField === null) {
            fuzzyField = Ext.define("FuzzyField", {
                extend: "Ext.data.Model",
                fields: [
                    { name: 'Id', type: 'string' },
                    { name: 'Name', type: 'string' }
                ]
            });
        }

        var fuzzyStore = Ext.create('Ext.data.Store', {
            model: fuzzyField,
            proxy: {
                type: 'memory',
                reader: {
                    type: 'json'
                }
            }
        });
        this.store = fuzzyStore;
        this.callParent();
    },
    listeners: {
        keydown: function (self, e, eOpts) {
            this.keydown(self, e, eOpts)
        },
        beforequery: function (queryPlan, eOpts) {
            var win = this.up('window');
            if (win === undefined)
                win = this.up('[isVcl=true]');
            if (win && !win.vcl.isEdit) {
                return false;
            }
        }
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
    doQuery: function (queryString, forceAll, rawQuery) {
        var me = this;
        var ret = Ext.form.field.ComboBox.prototype.doQuery.apply(this, arguments);
        function getFieldValue(name) {
            var dataInfo = Ax.Control.LibDataInfo.getDataInfo(me);
            var value = '';
            var reg = new RegExp('^[a-z]');
            if (reg.test(name) === true)
                value = name;
            else {
                var temps = name.split('.');
                var index = temps[0].charCodeAt() - 'A'.charCodeAt();
                if (me.tableIndex == index) {
                    value = dataInfo.dataRow.get(temps[1]);
                }
                else {
                    var grid = dataInfo.curGrid;
                    if (index != 0 && grid.parentGrid) {
                        do {
                            if (grid.parentGrid.tableIndex == index) {
                                value = grid.parentRow.get(temps[1]);
                                break;
                            }
                            grid = grid.parentGrid;
                        } while (grid);
                    } else {
                        if (me.win === undefined)
                            me.win = me.up('window');
                        if (me.win === undefined)
                            me.win = me.up('[isVcl=true]');
                        if (me.win && me.win.vcl.isEdit) {
                            var ctrl = Ext.getCmp(temps[1] + index + '_' + me.win.vcl.winId);
                            value = ctrl.getValue();
                        }
                    }

                }
            }
            return value;
        };
        var tableIdx = 0;
        if (me.relTableIndex && me.relTableIndex != '')
            tableIdx = getFieldValue(me.relTableIndex);
        Ext.Ajax.request({
            url: '/billSvc/selectFuncField',
            async: false,
            jsonData: { handle: UserHandle, progId: getFieldValue(me.relProgId), tableIndex: tableIdx },
            method: 'POST',
            timeout: 60000,
            success: function (response) {
                var ret = Ext.decode(response.responseText);
                var reuslt = Ext.decode(ret.SelectFuncFieldResult);
                me.store.loadData(reuslt);
            }
        });
        return ret;
    }
});