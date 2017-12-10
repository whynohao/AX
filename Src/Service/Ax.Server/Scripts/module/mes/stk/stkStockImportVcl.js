/// <reference path="../../../ax/vcl/comm/LibVclDataFunc.js" />


stkStockImportVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkStockImportVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkStockImportVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        var masterRow = this.dataSet.getTable(0).data.items[0];
        this.forms[0].updateRecord(masterRow);
        var typeId = masterRow.get('FROMTYPEID');
        var typeName = masterRow.get('FROMTYPENAME');
        var table = this.dataSet.getTable(1);
        if (e.dataInfo.fieldName == "btnLoad") {
            if (typeId == '') {
                alert('仓库不能为空。');
            } else {
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
                                        var data = vcl.invorkBcf('LoadData', [typeId, typeName, o.result.FileName]);
                                        table.removeAll();
                                        for (var i = 0; i < data.length; i++) {
                                            table.add(data[i]);
                                        };
                                        win.close();
                                        Ext.Msg.alert('提示', '已成功导入库存到' + typeName + '仓库,界面上展示的是合并后的结果');
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
        else if (e.dataInfo.fieldName == "btnBuild") {
            if (table.data.items.length > 0) {
                var bulidInfo = {};
                bulidInfo.Warehouseid = typeId;
                bulidInfo.Data = {};
                for (var i = 0; i < table.data.items.length; i++) {
                    var record = table.data.items[i];
                    if (bulidInfo.Data[i] === undefined) {
                        bulidInfo.Data[i] = {
                            Materialid: record.get('MATERIALID'),
                            Warehouseid: record.get('WAREHOUSEID'),
                            Stockstateid: record.get('STOCKSTATEID'),
                            Quantity: record.get('QUANTITY')
                        };
                    }
                }
                this.invorkBcf('ImportData', [bulidInfo]);
                Ext.Msg.alert('提示','已成功导入库存到' + typeName + '仓库,请前往库存结余帐查看！');
            }
        }
    }
}