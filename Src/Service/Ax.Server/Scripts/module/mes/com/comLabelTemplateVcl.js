/// <reference path="../../../ax/vcl/comm/LibVclData.js" />


comLabelTemplateVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = comLabelTemplateVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comLabelTemplateVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        //Ext.util.Cookies.set('LabelTemplateJs',Ext.encode( Ext.getCmp('LABELTEMPLATEJS0_' + vcl.winId).getValue()));
		DesktopApp.LabelTemplateJs = Ext.encode( Ext.getCmp('LABELTEMPLATEJS0_' + vcl.winId).getValue());
        var me = this;
        var panel = Ext.create('Ext.panel.Panel', {
            border: false,
            html: "<iframe id='Lab' scrolling='auto' frameborder='0' width='100%' height='100%' src=../../../Scripts/view/com/Print.html> </iframe>"
        });
        var win = Ext.create('Ext.window.Window', {
            title: '打印标签设计',
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
                    if (DesktopApp.LabelTemplateJsAfter) {
                        var labelTemplateJs = Ext.decode(DesktopApp.LabelTemplateJsAfter);
                        Ext.getCmp('LABELTEMPLATEJS0_' + vcl.winId).setValue(labelTemplateJs);
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