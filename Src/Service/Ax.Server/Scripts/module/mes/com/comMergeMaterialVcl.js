/// <reference path="../../../ax/vcl/comm/LibVclDataFunc.js" />


comMergeMaterialVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = comMergeMaterialVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comMergeMaterialVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "btnImport") {
            var panel = Ext.create('Ext.form.Panel', {
                bodyPadding: 10,
                frame: true,
                renderTo: Ext.getBody(),
                items: [{
                    xtype: 'filefield',
                    name: 'txtFile',
                    fieldLabel: '文件',
                    labelWidth: 50,
                    msgTarget: 'side',
                    allowBlank: false,
                    anchor: '100%',
                    buttonText: '选择...'
                }],

                buttons: [{
                    text: '导入',
                    handler: function () {
                        var form = this.up('form').getForm();
                        if (form.isValid()) {
                            form.submit({
                                url: '/fileTranSvc/upLoadFile',
                                waitMsg: '正在导入文件...',
                                success: function (fp, o) {
                                    if (vcl.invorkBcf('AddMergeTask', [o.result.FileName]) == true) {
                                        Ext.Msg.alert('提示', '文件 "' + o.result.FileName + '" 导入成功.');
                                    }
                                },
                                failure: function (fp, o) {
                                    Ext.Msg.alert('错误', '文件 "' + o.result.FileName + '" 导入失败.');
                                }
                            });
                        }
                    }
                }]
            });
            win = Ext.create('Ext.window.Window', {
                autoScroll: true,
                width: 400,
                height: 300,
                layout: 'fit',
                vcl: vcl,
                constrainHeader: true,
                minimizable: true,
                maximizable: true,
                items: [panel]
            });
            win.show();
        } else if (e.dataInfo.fieldName == "btnShow") {
            var data = vcl.invorkBcf('ShowResult', []);
            var table = this.dataSet.getTable(1);
            table.removeAll();
            for (var i = 0; i < data.length; i++) {
                table.add(data[i]);
            }
        } else if (e.dataInfo.fieldName == "btnMergeMaterial") {
            vcl.invorkBcf('MergeMaterial', []);
            var data = vcl.invorkBcf('ShowResult', []);
            var table = this.dataSet.getTable(1);
            table.removeAll();
            for (var i = 0; i < data.length; i++) {
                table.add(data[i]);
            }
        }
    }
}