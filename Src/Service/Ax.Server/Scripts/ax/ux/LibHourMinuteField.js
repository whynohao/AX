
Ext.define('Ax.ux.form.LibHourMinuteField', {
    extend: 'Ext.form.field.Time',
    alias: 'widget.libHourMinuteField',
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
    format: 'H:i',
    altFormats: 'Hi|Gi|i',
    enableKeyEvents: true,
    increment: 1,
    pickerMaxHeight: 100,
    initComponent: function () {
        this.id = this.name + this.tableIndex + '_' + DesktopApp.ActiveWindow;
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
    setValue: function (v) {
        if (v != undefined && typeof v !== 'number') {
            arguments[0] = v.getHours() * 100 + v.getMinutes();
        };
        if (v === 0)
            arguments[0] = undefined;
        return Ext.form.field.Time.prototype.setValue.apply(this, arguments);
    },
    getValue: function () {
        var v = this.value;
        if (this.rawValue == '')
            this.value = 0;
        if (v != undefined && typeof v !== 'number') {
            this.value = v.getHours() * 100 + v.getMinutes();
        };
        return this.value;
    },
    doQuery: function (queryString, forceAll, rawQuery) {
        if (queryString.length <= 4)
            return false;
        return Ext.form.field.Time.prototype.doQuery.apply(this, arguments);
    },
    getErrors: function (value) {
        return '';
    }
});