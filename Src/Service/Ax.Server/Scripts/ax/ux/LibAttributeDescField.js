
Ext.define('Ax.ux.form.LibAttributeDescField', {
    extend: 'Ext.form.field.TextArea',
    alias: 'widget.libAttributeDescField',
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
    enableKeyEvents : true,
    enterIsSpecial : true,
    initComponent: function () {
        this.id = this.name + this.tableIndex + '_' + DesktopApp.ActiveWindow;
        this.callParent();
    },
    listeners: {
        keydown: function (self, e, eOpts) {
            this.keydown(self, e, eOpts)
        },
        keyup: function (self, e, eOpts) {
            if (e.keyCode == e.F4) {
                var win = self.up('window');
                if (win === undefined)
                    win = self.up('[isVcl=true]');
                if (win && win.vcl) {
                    var dataInfo = Ax.Control.LibDataInfo.getDataInfo(self);
                    var attrId = dataInfo.dataRow.get(self.attrField);
                    if (attrId && attrId != '')
                        Ax.utils.LibAttributeForm.show(win.vcl, self, dataInfo);
                }
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