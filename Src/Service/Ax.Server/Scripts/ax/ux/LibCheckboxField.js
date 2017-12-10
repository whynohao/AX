
Ext.define('Ax.ux.form.LibCheckboxField', {
    extend: 'Ext.form.field.Checkbox',
    alias: 'widget.libCheckboxField',
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
    initComponent: function () {
        this.id = this.name + this.tableIndex + '_' + DesktopApp.ActiveWindow;
        this.callParent();
    },
    listeners: {
        keydown: function (self, e, eOpts) {
            this.keydown(self, e, eOpts)
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
    setValue: function (checked) {
        var win = this.up('window');
        if (win === undefined)
            win = this.up('[isVcl=true]');
        if (win && win.vcl !== undefined) {
            if (win.vcl.isEdit || win.vcl.proxy === true) {
                this.oldValue = this.getValue();
                return this.callParent(arguments);
            }
        } else {
            return this.callParent(arguments);
        }
    }
});