/// <reference path="../../../ax/vcl/comm/LibVclData.js" />


axpPrintTplVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = axpPrintTplVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = axpPrintTplVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ColumnDbClick) {
        if (e.dataInfo.fieldName == 'ISTPL') {
            Ext.util.Cookies.set('TplJs', Ext.encode(e.dataInfo.dataRow.get('TPLJS')));
            var me = this;
            var panel = Ext.create('Ext.panel.Panel', {
                border: false,
                html: "<iframe scrolling='auto' frameborder='0' width='100%' height='100%' src=/Desk/PrintTpl> </iframe>"
            });
            var win = Ext.create('Ext.window.Window', {
                title: '打印模板设计',
                height: 600,
                width: 800,
                modal: true,
                vcl: this,
                autoScroll: true,
                constrainHeader: true,
                layout: 'fit',
                items: [panel],
                listeners: {
                    beforeclose: function () {
                        var tplJs = Ext.util.Cookies.get('TplJsAfter');
                        if (tplJs) {
                            var js = Ext.decode(tplJs);
                            e.dataInfo.dataRow.set('TPLJS', js);
                            e.dataInfo.dataRow.set('ISTPL', js.length > 0);
                        }
                        else {
                            if (this.confirmed == true) {
                                this.configed = false;
                                return true;
                            }
                            Ext.Msg.confirm('提示', '模板还没保存，是否关闭？', function (button) {
                                if (button == "yes") {
                                    this.confirmed = true;
                                    this.close();
                                }
                            }, this);
                            return false;
                        }
                    }
                }
            }).show();
        }
    }
}