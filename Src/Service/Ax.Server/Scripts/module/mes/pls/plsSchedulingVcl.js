/// <reference path="../../../ax/vcl/comm/LibVclDataFunc.js" />


plsSchedulingVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsSchedulingVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsSchedulingVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    this.forms[0].updateRecord(masterRow);

    if (e.libEventType == LibEventTypeEnum.BeforeAddRow || e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
        e.dataInfo.cancel = true;
    }
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "btnExe") {
            var data = vcl.invorkBcf('ExecScheduling', [masterRow.data["SCHEDULINGDATE"], masterRow.data["COMLOGISTICSCOMPANYID"], masterRow.data["PRODUCELINEID"]]);
            var table = this.dataSet.getTable(1);
            table.removeAll();
            for (var i = 0; i < data.length; i++) {
                table.add(data[i]);
            }
        }
        else if (e.dataInfo.fieldName == "btnShow") {
            var data = vcl.invorkBcf('ShowResult', []);
            var table = this.dataSet.getTable(1);
            table.removeAll();
            for (var i = 0; i < data.length; i++) {
                table.add(data[i]);
            }
        } else if (e.dataInfo.fieldName == "btnClear") {
            vcl.invorkBcf('ClearScheduling', [masterRow.data["SCHEDULINGDATE"], masterRow.data["PRODUCELINEID"]]);
            var data = vcl.invorkBcf('ShowResult', []);
            var table = this.dataSet.getTable(1);
            table.removeAll();
            for (var i = 0; i < data.length; i++) {
                table.add(data[i]);
            }
        } else if (e.dataInfo.fieldName == "btnLogCount") {

            var returnData = this.invorkBcf("GetLogData", []);
            this.deleteAll(2);
            if (returnData.length > 0) {
                Ext.suspendLayouts();
                var formStore = this.dataSet.getTable(1);
                formStore.suspendEvents();
                try {

                    var masterRow = this.dataSet.getTable(0).data.items[0];
                    for (var i = 0; i < returnData.length; i++) {
                        var LogRow = this.addRow(masterRow, 2);
                        LogRow.set('LOGISTICSCOMPANYID', returnData[i].LogCompany);
                        LogRow.set('LOGISTICSCOMPANYNAME', returnData[i].LogCompanyName);
                        LogRow.set('TYPE311', returnData[i].Quan311);
                        LogRow.set('TYPE312', returnData[i].Quan312);
                        LogRow.set('TYPE314', returnData[i].Quan314);
                        LogRow.set('TYPE315', returnData[i].Quan315);
                        LogRow.set('TYPE316', returnData[i].Quan316);
                        LogRow.set('TYPE31501', returnData[i].Quan31501);
                        LogRow.set('TYPE31502', returnData[i].Quan31502);
                        LogRow.set('TYPEOTHER', returnData[i].QuanOther);
                    }

                }
                finally {
                    formStore.resumeEvents();
                    if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
                        formStore.ownGrid.reconfigure(formStore);
                    Ext.resumeLayouts(true);
                }
            }
            else {
                Ext.Msg.alert("提示", '统计结果为空！');
            }

        } else if (e.dataInfo.fieldName == "btnImport") {
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
                    text: '上传',
                    handler: function () {
                        var form = this.up('form').getForm();
                        if (form.isValid()) {
                            form.submit({
                                url: '/fileTranSvc/upLoadFile',
                                waitMsg: '正在上传文件...',
                                success: function (fp, o) {
                                    //if (vcl.invorkBcf('AddFileTask', [o.result.FileName]) == true) {
                                        Ext.Msg.alert('提示', '文件 "' + o.result.FileName + '" 上传成功.');
                                    //}
                                },
                                failure: function (fp, o) {
                                    Ext.Msg.alert('错误', '文件 "' + o.result.FileName + '" 上传失败.');
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
        }
    }

}