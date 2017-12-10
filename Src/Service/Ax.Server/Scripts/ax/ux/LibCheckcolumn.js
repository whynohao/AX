
Ext.define('Ax.ux.form.LibCheckcolumn', {
    extend: 'Ext.grid.column.CheckColumn',
    alias: 'widget.libCheckcolumn',
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
    initComponent: function () {
        this.listeners = {
            beforecheckchange: function (self, rowIndex, checked, eOpts) {
                if (this.readOnly)
                    return false;
                var win = self.up('window');
                if (win === undefined)
                    win = self.up('[isVcl=true]');
                //win.vcl === undefined 在单据清单的情况
                if (win && (win.vcl === undefined || !win.vcl.isEdit)) {
                    return false;
                }
            }
        };
        this.callParent();
    }
});