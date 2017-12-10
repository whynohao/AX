
Ext.define('Ax.ux.form.LibAttributeCodeField', {
    extend: 'Ext.form.field.Text',
    alias: 'widget.libAttributeCodeField',
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
    enableKeyEvents: true,
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
    }
});