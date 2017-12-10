comControlConfigVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}

var proto = comControlConfigVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comControlConfigVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            // 编辑状态下执行选择图标 按钮事件，调用内部方法，获取所有CPS组件图标以供用户选择
            if (e.dataInfo.fieldName == 'BtnSearchIcon') {
                if (this.isEdit) {
                    var grid = Ext.getCmp(this.winId + 'COMCONTROLCONFIGDETAILGrid');
                    var records = grid.getView().getSelectionModel().getSelection();
                    // 选中一行状态下执行操作
                    if (records.length == 1) {
                        var returnData = this.invorkBcf('GetCPSIconDic', []);
                        this.fillData.call(this, returnData, function (name) {
                            records[0].set('RELICON', name);
                        })
                    }
                    else {
                        Ext.Msg.alert('系统提示', '请选择一行数据');
                    }
                } else {
                    Ext.Msg.alert('系统提示', '编辑状态才能使用 选择图标 按钮！');
                }
            }
            if (e.dataInfo.fieldName == 'BtnUploadIcon') {
                proto.uploadIcon.call(this)
            }
            break
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "PROGID") {
                    if (e.dataInfo.oldValue != e.dataInfo.value) {
                        e.dataInfo.dataRow.set("RELICON", "");
                    }
                }
            }
            break
    }
}

//填充图标路径集合数据并显示出窗体，返回选择的图标值
proto.fillData = function (list, callback) {
    // 获得所有图标
    var panel = this.GetIconPanel.call(this, list)

    var width = document.body.clientWidth * 0.4
    var height = document.body.clientHeight * 0.5
    // 显示弹框，按照当前视图的比例大小显示
    var viewport = Ext.create('Ext.window.Window', {
        title: '图标选择',
        titleAlign: 'center',
        align: 'middle',
        closable: true,
        draggable: true,
        width: width,
        height: height,
        autoScroll: true,
        plain: true,
        modal: true,
        layout: 'fit',
        items: [panel]
    })
    viewport.show()
    $('div[name=\'cpsicon\']').unbind('click').bind('click', function () {
        var name = $(this).find('div.cps-sel-span').html()
        callback(name)
        viewport.close()
    })
}

//根据图标路径集合得到图标面板
proto.GetIconPanel = function (list) {
    var html = '<div class="cps-table" style="width:100%;height:100%;">'
    for (var i = 0; i < list.length; i++) {
        var iconUrl = list[i]
        var name = iconUrl.substr(iconUrl.lastIndexOf('/') + 1)
        html = html + '<div class="cps-sel-div" name="cpsicon"  style="float:left;" title="' + name + '">'
        html = html + '<div class="cps-sel-icon"><img src="' + iconUrl + '" /></div>'
        html = html + '<div class="cps-sel-span">' + name + '</div></div>'
    }
    html = html + '</div>'

    var panel = Ext.create('Ext.panel.Panel', {
        anchor: '100% 100%',
        border: true,
        style: {
            background: 'white'
        },
        html: html
    })
    return panel
}

//上传文件
proto.uploadIcon = function () {
    // 上传文件panel，用于选择文件和产检文件
    var iconPanel = Ext.create('Ext.form.Panel', {
        bodyPadding: 10,
        items: [{
            xtype: 'filefield',
            name: 'uploadFileField',
            id: 'uploadFileField',
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
                var form = this.up('form').getForm()
                if (form.isValid()) {
                    var tempStrArrays = Ext.getCmp('uploadFileField').getValue().split('\\')
                    var realFileName = tempStrArrays[tempStrArrays.length - 1]
                    var file = form.getFields().items[0]
                    var thisMe = this
                    Ext.Ajax.request({
                        url: '/CpsModule/CheckIcon',
                        params: {
                            RealFileName: realFileName,
                        },
                        success: function (response) {
                            if (response.responseText == 'false') {
                                //检查文件大小
                                if (proto.checkFile.call(this, file) == false)
                                    return
                                form.submit({
                                    url: '/fileTranSvc/upLoadDoc',
                                    waitMsg: '正在上传文件...',
                                    success: function (fp, o) {
                                        Ext.Ajax.request({
                                            url: '/CpsModule/MoveIcon',
                                            params: {
                                                FileName: o.result.FileName,
                                                RealFileName: realFileName,
                                            },
                                            success: function (response) {
                                                Ext.Msg.alert('提示', '图标上传成功.')
                                                thisMe.up('window').close()
                                            },
                                            failure: function () {
                                                Ext.Msg.alert('错误', '图标上传失败.')
                                            }
                                        })
                                    },
                                    failure: function (fp, o) {
                                        Ext.Msg.alert('错误', '图标上传失败.')
                                    }
                                })
                            } else {
                                file.setRawValue('')
                                Ext.Msg.alert('错误', '图标已存在，请修改图标名称.')
                            }
                        },
                        failure: function () {
                            Ext.Msg.alert('错误', '图标已存在，请修改图标名称.')
                        }
                    })
                }
            }
        }]
    })
    // 显示上传文件框
    var win = Ext.create('Ext.window.Window', {
        title: '上传图标',
        titleAlign: 'center',
        align: 'middle',
        autoScroll: true,
        closable: true,
        draggable: true,
        width: 400,
        height: 200,
        layout: 'fit',
        modal: true,
        items: [iconPanel]
    })
    win.show()
}
//检查文件
proto.checkFile = function (file) {
    //验证文件的正则
    var img_reg = /\.([jJ][pP][gG]){1}$|\.([gG][iI][fF]){1}$|\.([pP][nN][gG]){1}$/
    if (!img_reg.test(file.value)) {
        Ext.Msg.alert('提示', '文件类型错误,请选择图片文件(jpg/gif/png)')
        file.setRawValue('')
        return false
    }
    //取控件DOM对象
    var field = document.getElementById('uploadFileField')
    //取控件中的input元素
    var inputs = field.getElementsByTagName('input')
    var fileInput = null
    var il = inputs.length
    //取出input 类型为file的元素
    for (var i = 0; i < il; i++) {
        if (inputs[i].type == 'file') {
            fileInput = inputs[i]
            break
        }
    }
    if (fileInput != null) {
        var fileSize = proto.getFileSize.call(this, fileInput)
        //允许上传不大于10M的文件
        if (fileSize > 1024 * 5) {
            Ext.Msg.alert('提示', '文件太大，请选择小于5M的文件！')
            file.setRawValue('')
            return false
        }
    }
    return true
}
//计算文件大小，返回文件大小值，单位K
proto.getFileSize = function (target) {
    var isIE = /msie/i.test(navigator.userAgent) && !window.opera
    var fs = 0
    if (isIE && !target.files) {
        var filePath = target.value
        var fileSystem = new ActiveXObject('Scripting.FileSystemObject')
        var file = fileSystem.GetFile(filePath)
        fs = file.Size
    } else if (target.files && target.files.length > 0) {
        fs = target.files[0].size
    } else {
        fs = 0
    }
    if (fs > 0) {
        fs = fs / 1024
    }
    return fs
}
