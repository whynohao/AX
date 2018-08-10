
Ext.define('Ax.ux.form.LibTimeField', {
    extend: 'Ext.form.field.Time',
    alias: 'widget.libTimeField',
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
    format: 'H:i:s',
    altFormats: 'His|gis|is|s',
    enableKeyEvents: true,
    initComponent: function () {
        debugger;
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
            arguments[0] = v.getHours() * 10000 + v.getMinutes() * 100 + v.getSeconds();
        };
        if (v === 0)
            arguments[0] = undefined;
        return this.callParent(arguments);
    },
    getValue: function () {
        var curGrid = this.up('grid');
        if (curGrid === undefined) {
            var win = this.up('window');
            if (win === undefined)
                win = this.up('[isVcl=true]');
            if (win === undefined || !win.vcl.saving)
                return this.callParent(arguments);
        }
        if (this.value === null)
            return 0;
        else
            return this.value;
    }
});