
Ext.define('Ax.ux.form.LibComboboxField', {
    extend: 'Ext.form.field.ComboBox',
    alias: 'widget.libComboboxField',
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
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
            if (win && win.vcl !== undefined && !win.vcl.isEdit) {
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
    }
});