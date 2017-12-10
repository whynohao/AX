comSimRelationMapVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = comSimRelationMapVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comSimRelationMapVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "simulation") {//模拟生成过账表
                var dataList = getDataList(this);

                var data = this.invorkBcf('AbnormalSaleBom', [dataList]);
                if (data > 0)
                    Ax.utils.LibVclSystemUtils.openDailyRpt("com.SimAbnormalBomTechTrackRpt", "模拟异常订单BOM工艺确认跟踪报表", data);


            } else if (e.dataInfo.fieldName == "deleteSaleBom") {
                Ext.Msg.confirm('提示', '是否确认删除模拟订单BOM?', function (button) {
                    if (button == "yes") {
                        var data = this.invorkBcf('DeleteSaleBom', []);

                        Ext.Msg.alert("提示", "删除成功");
                    }
                    else if (button == "no") {
                        e.dataInfo.cancel = true;
                    }
                }, this);

            } else if (e.dataInfo.fieldName == "chooseYse") {
                updateSimulation(true);

            } else if (e.dataInfo.fieldName == "chooseNo") {
                updateSimulation(false);

            } else if (e.dataInfo.fieldName == "importData") {
                setWin().show();
            } else if (e.dataInfo.fieldName == "exportData") {
                var dataList = getDataList(this);

                var fileName = vcl.invorkBcf('GetExportData', ["comSimRelationMap", dataList]);
                if (fileName && fileName !== '') {
                    DesktopApp.IgnoreSkip = true;
                    try {
                        window.location.href = '/TempData/ExportData/' + fileName;
                    } finally {
                        DesktopApp.IgnoreSkip = false
                    }
                }
            }
            break;

    }
}

//修改 模拟状态 ：是 or 否
var updateSimulation = function (boolean) {
    var grid = Ext.getCmp(vcl.winId + "COMSIMRELATIONMAP" + "Grid");
    var selected = grid.getView().getSelectionModel().getSelection();
    for (var i = 0; i < selected.length; i++) {
        selected[i].set("SIMULATION", boolean);
    }
}

//创建导入窗口
var setWin = function () {
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
                            var data = vcl.invorkBcf('SetImportData', [o.result.FileName]);
                            vcl.dataSet.getTable(0).removeAll()
                            var masterRow = vcl.dataSet.getTable(0).data.items[0];
                            for (var i = 0; i < data.COMSIMRELATIONMAP.length; i++) {
                                var info = data.COMSIMRELATIONMAP[i];
                                var newRow = vcl.addRow(masterRow, 0);
                                for (var key in info) {
                                    newRow.set(key, info[key]);
                                }
                            }
                            win.close();
                        },
                        failure: function (fp, o) {
                            Ext.Msg.alert('错误', '文件 "' + o.result.FileName + '" 导入失败.');
                        }
                    });
                }
            }
        }]
    });
    var win = Ext.create('Ext.window.Window', {
        autoScroll: true,
        width: 400,
        height: 300,
        layout: 'fit',
        constrainHeader: true,
        minimizable: true,
        maximizable: true,
        items: [panel]
    });
    return win;
}

//获取数据
var getDataList = function (me) {
    var dataList = new Array();
    for (var i = 0; i < me.dataSet.getTable(0).data.items.length; i++) {
        var dataRow = me.dataSet.getTable(0).data.items;
        var charString = [];
        for (var key in dataRow[i].data) {
            charString = charString + key + ":" + dataRow[i].data[key] + "/t";
        }
        charString = charString.toString();
        charString = charString.substring(0, charString.length - 1);
        dataList.push(charString);

    }
    return dataList;
}


