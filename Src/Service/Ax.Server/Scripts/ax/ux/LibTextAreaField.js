
Ext.define('Ax.ux.form.LibTextAreaField', {
    extend: 'Ext.form.field.TextArea',
    alias: 'widget.libTextAreaField',
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
    enableKeyEvents: true,
    grow: true,
    growMin: 0,
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