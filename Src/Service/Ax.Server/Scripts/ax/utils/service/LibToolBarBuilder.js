Ext.ns('Ax.utils');

Ax.utils.BillManager = {
    openBillByF4: function (progId, billAction, curPks, entryParam) {
        var vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, BillTypeEnum.Bill);
        var view = Ax.utils.LibVclSystemUtils.getView(progId, BillTypeEnum.Bill, vcl);
        vcl.entryParam = entryParam;
        vcl.getTpl();
        vcl.billType = vcl.tpl.BillType;
        var id = Ext.id();
        DesktopApp.ActiveWindow = id;
        vcl.winId = id;
        var win = Ext.create('Ext.window.Window', {
            id: id,
            progName: vcl.tpl.DisplayText,
            autoScroll: true,
            width: document.body.clientWidth * 0.8,
            height: document.body.clientHeight * 0.9,
            constrainHeader: true,
            minimizable: true,
            maximizable: true,
            items: view[vcl.funcView.get("default").name](billAction, curPks, true),
            vcl: vcl,
            listeners: {
                close: function (self, eOpts) {
                    this.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.FormClosed });
                },
                beforeclose: function (self, eOpts) {
                    this.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.FormClosing });
                }
            }
        });
        var masterRow = vcl.dataSet.getTable(0).data.items[0];
        var showTxt = Ax.utils.LibVclSystemUtils.getCurrentStateText(masterRow.get('CURRENTSTATE'));
        if (vcl.tpl.ShowAuditState) {
            var auditTxt = Ax.utils.LibVclSystemUtils.getAuditStateText(masterRow.get('AUDITSTATE'), masterRow.get('FLOWLEVEL'));
            win.setTitle(win.progName + '【' + showTxt + '】' + '【' + auditTxt + '】');
        } else
            win.setTitle(win.progName + '【' + showTxt + '】');
        if (vcl.billType == BillTypeEnum.Master) {
            if (!masterRow.get('ISVALIDITY'))
                win.setTitle(win.title + '【已失效】');
        }
        win.minimize = function (e) {
            this.hide();
        }
        vcl.win = win;
        win.show();
    },
    openVersionBill: function (progId, curPks, lookVersionObj) {
        var vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, BillTypeEnum.Bill);
        var view = Ax.utils.LibVclSystemUtils.getView(progId, BillTypeEnum.Bill, vcl);
        vcl.entryParam = lookVersionObj.Vcl.entryParam;
        vcl.getTpl();
        vcl.billType = vcl.tpl.BillType;
        var id = Ext.id();
        DesktopApp.ActiveWindow = id;
        vcl.winId = id;
        var win = Ext.create('Ext.window.Window', {
            id: id,
            progName: vcl.tpl.DisplayText,
            autoScroll: true,
            width: document.body.clientWidth * 0.8,
            height: document.body.clientHeight * 0.9,
            constrainHeader: true,
            minimizable: true,
            maximizable: true,
            items: view[vcl.funcView.get("default").name](BillActionEnum.Browse, curPks, false, lookVersionObj),
            vcl: vcl,
            listeners: {
                close: function (self, eOpts) {
                    this.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.FormClosed });
                },
                beforeclose: function (self, eOpts) {
                    this.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.FormClosing });
                }
            }
        });
        var masterRow = vcl.dataSet.getTable(0).data.items[0];
        var showTxt = Ax.utils.LibVclSystemUtils.getCurrentStateText(masterRow.get('CURRENTSTATE'));
        if (vcl.tpl.ShowAuditState) {
            var auditTxt = Ax.utils.LibVclSystemUtils.getAuditStateText(masterRow.get('AUDITSTATE'), masterRow.get('FLOWLEVEL'));
            win.setTitle(win.progName + '【' + showTxt + '】' + '【' + auditTxt + '】');
        } else
            win.setTitle(win.progName + '【' + showTxt + '】');
        if (vcl.billType == BillTypeEnum.Master) {
            if (!masterRow.get('ISVALIDITY'))
                win.setTitle(win.title + '【已失效】');
        }
        vcl.win = win;
        win.minimize = function (e) {
            this.hide();
        }
        win.show();
    },
    openRpt: function (progId, displayText, execTaskDataId) {
        var vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, BillTypeEnum.Rpt);
        var view = Ax.utils.LibVclSystemUtils.getView(progId, BillTypeEnum.Rpt, vcl);
        vcl.billType = BillTypeEnum.Rpt;
        vcl.getTpl();
        var toolBarAction = Ax.utils.LibToolBarBuilder.createRptAction(vcl, 0);
        var id = Ext.id();
        DesktopApp.ActiveWindow = id;
        vcl.winId = id;
        var win = Ext.create('Ext.window.Window', {
            id: id,
            title: displayText,
            id: progId,
            autoScroll: true,
            width: document.body.clientWidth * 0.8,
            height: document.body.clientHeight * 0.9,
            layout: 'fit',
            vcl: vcl,
            tbar: Ax.utils.LibToolBarBuilder.createToolBar(toolBarAction),
            constrainHeader: true,
            minimizable: true,
            maximizable: true,
            items: view[vcl.funcView.get("default").name](displayText, execTaskDataId),
            listeners: {
                close: function (self, eOpts) {
                    this.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.FormClosed });
                },
                beforeclose: function (self, eOpts) {
                    this.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.FormClosing });
                }
            }
        });
        vcl.win = win;
        win.minimize = function (e) {
            this.hide();
        }
        win.show();
    },
    openGridByF4: function (progId, curPks) {
        var vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, BillTypeEnum.Grid);
        var view = Ax.utils.LibVclSystemUtils.getView(progId, BillTypeEnum.Grid, vcl);
        vcl.billType = BillTypeEnum.Grid;
        vcl.getTpl();
        var id = Ext.id()
        DesktopApp.ActiveWindow = id
        vcl.winId = id
        var win = Ext.create('Ext.window.Window', {
            id: id,
            title: vcl.tpl.DisplayText,
            autoScroll: true,
            width: document.body.clientWidth * 0.8,
            height: document.body.clientHeight * 0.9,
            layout: 'fit',
            vcl: vcl,
            constrainHeader: true,
            minimizable: true,
            maximizable: true,
            items: view[vcl.funcView.get("default").name](),
            listeners: {
                close: function (self, eOpts) {
                    this.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.FormClosed });
                },
                beforeclose: function (self, eOpts) {
                    this.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.FormClosing });
                }
            }

        });
        // if (curPks) {
        //   vcl.browseByPK(curPks);
        // }
        win.minimize = function (e) {
            this.hide();
        }
        vcl.win = win;
        win.show();
    }
};

Ax.utils.LibToolBarBuilder = {
    createBillListingAction: function (progId, billType, displayText, permission, gridPanel, filterCbo, compareCbo, compareTxt, dateCbo, filterFun, entryParam) {
        var items;
        var queryCondition;
        function getBillListing() {
            var filter = filterFun();
            var pageSize = 50;// 每页多少条
            Ext.Ajax.request({
                url: '/billSvc/getBillListing',
                method: 'POST',
                jsonData: { listingQuery: { Handle: UserHandle, ProgId: progId, Condition: queryCondition, TimeFilter: dateCbo.getValue(), Filter: filter, EntryParam: entryParam, PageSize: pageSize } },
                async: false,
                timeout: 90000000,
                success: function (response) {
                    var ret = Ext.decode(response.responseText);
                    var listingData = Ext.decode(ret.GetBillListingResult);
                    var curStore = gridPanel.getStore();
                    var proxy = curStore.getProxy();
                    proxy.setData(listingData.Data);
                    curStore.loadPage(1);
                }
            });
        }
        var proxyVcl = {
            proxy: true,
            progId: progId,
            formCallBackHandler: function (tag, param) {
                if (tag == 'SYSTEM_QUERY') {
                    queryCondition = { QueryFields: param.condition };
                    getBillListing();
                }
            }
        };
        //var permissionList = checkAllPermission();
        var permissionList = [];
        var select = Ext.create(Ext.Action, {
            text: '查询',
            iconCls: 'fa fa-search',
            handler: function () {
                var query;
                if (queryCondition) {
                    query = queryCondition.QueryFields;
                }
                Ax.utils.LibQueryForm.createForm(proxyVcl, progId, query);
            }
        });
        var refresh = Ext.create(Ext.Action, {
            text: '刷新',
            iconCls: 'fa fa-refresh',
            handler: function () {
                getBillListing();
            }
        });

        function getSelectPk() {
            var curPks = [];
            var firstRecord = gridPanel.getView().getSelectionModel().getSelection()[0];
            if (firstRecord) {
                var count = gridPanel.Pks.length;
                for (var i = 0; i < count; i++) {
                    var pkStr = gridPanel.Pks[i];
                    var value = firstRecord.get(pkStr);
                    curPks.push(value);
                }
            }
            return curPks;
        };

        //浏览
        var browse = Ext.create(Ext.Action, {
            text: '浏览',
            iconCls: 'fa fa-eye',
            hidden: contains(permissionList, 2),
            handler: function () {
                var curPks = getSelectPk();
                if (curPks.length > 0)
                    Ax.utils.LibVclSystemUtils.openBill(progId, billType, displayText, BillActionEnum.Browse, entryParam != '' ? Ext.decode(entryParam) : undefined, curPks, undefined);
                else
                    Ext.Msg.alert("提示", "请先选择一行数据");
            }
        });
        //新增
        var addNew = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            iconCls: 'fa fa-plus',
            hidden: contains(permissionList, 4),
            menuActiveCls: 'fa fa-plus',
            text: '新增',
            handler: function () {
                Ax.utils.LibVclSystemUtils.openBill(progId, billType, displayText, BillActionEnum.AddNew, entryParam != '' ? Ext.decode(entryParam) : undefined, undefined, undefined);
            },
            menu: [{
                text: '复制',
                //cls: 'fa fa-copy',
                handler: function () {
                    var curPks = getSelectPk();
                    if (curPks.length > 0)
                        Ax.utils.LibVclSystemUtils.openBill(progId, billType, displayText, BillActionEnum.AddNew, entryParam != '' ? Ext.decode(entryParam) : undefined, curPks, undefined);
                }
            }]
        });

        //修改
        var edit = Ext.create(Ext.Action, {
            text: '修改',
            iconCls: 'fa fa-edit',
            hidden: contains(permissionList, 8),
            handler: function () {
                var curPks = [];
                var firstRecord = gridPanel.getView().getSelectionModel().getSelection()[0];
                if (firstRecord) {
                    var count = gridPanel.Pks.length;
                    for (var i = 0; i < count; i++) {
                        var pkStr = gridPanel.Pks[i];
                        var value = firstRecord.get(pkStr);
                        curPks.push(value);
                    }
                    Ax.utils.LibVclSystemUtils.openBill(progId, billType, displayText, BillActionEnum.Edit, entryParam != '' ? Ext.decode(entryParam) : undefined, curPks, undefined);
                }
                else
                    Ext.Msg.alert("提示", "请先选择一行数据");
            }
        });
        //删除
        var deleteBill = Ext.create(Ext.Action, {
            text: '删除',
            iconCls: 'fa fa-trash',
            hidden: contains(permissionList, 16),
            handler: function () {
                Ext.Msg.confirm('系统提示', '确定要删除吗？', function (btn) {
                    if (btn === 'yes') {
                        invorkBatchBcf('Delete', undefined, function (errPks) {
                            var records = gridPanel.getView().getSelectionModel().getSelection()
                            if (records.length > 0) {
                                var count = gridPanel.Pks.length
                                for (var i = records.length - 1; i >= 0; i--) {
                                    var pk = []
                                    for (var r = 0; r < count; r++) {
                                        var pkStr = gridPanel.Pks[r]
                                        var value = records[i].get(pkStr)
                                        pk.push(value)
                                    }
                                    if (errPks.length > 0) {
                                        for (var m = errPks.length - 1; m >= 0; m--) {
                                            var same = true
                                            for (var n = 0; n < count; n++) {
                                                if (errPks[m][n] != pk[n]) {
                                                    same = false
                                                    errPks.pop(errPks[m])
                                                    break
                                                }
                                            }
                                            if (!same)
                                                gridPanel.store.remove(records, true)
                                        }
                                    } else {
                                        gridPanel.store.remove(records, true)
                                    }
                                }
                            }
                        })
                    }
                })
            }
        })

        //送审
        var submitAudit = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            text: '送审',
            iconCls: 'fa fa-send',
            hidden: contains(permissionList, 2),
            handler: function () {
                invorkBatchBcf("SubmitAudit", false);
            },
            menu: [{
                text: '撤回',
                handler: function () {
                    invorkBatchBcf("SubmitAudit", true);
                }
            }]
        });
        //审核
        var audit = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            iconCls: 'fa fa-check-circle',
            text: '审核通过',
            hidden: contains(permissionList, 128),
            handler: function () {
                invorkBatchBcf("Audit", true);
            },
            menu: [{
                text: '审核不通过',
                handler: function () {
                    invorkBatchBcf("Audit", false);
                }
            }, {
                text: '弃审',
                handler: function () {
                    invorkBatchBcf("CancelAudit");
                }
            }]
        });
        //生效
        var release = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            text: '生效',
            iconCls: 'fa fa-check-square',
            hidden: contains(permissionList, 32),
            handler: function () {
                invorkBatchBcf("TakeRelease", false);
            },
            menu: [{
                text: '取消生效',
                handler: function () {
                    invorkBatchBcf("TakeRelease", true);
                }
            }]
        });
        //作废
        var invalid = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            text: '作废',
            iconCls: 'fa fa-file-excel-o',
            hidden: contains(permissionList, 2048),
            handler: function () {
                invorkBatchBcf("Invalid", false);
            },
            menu: [{
                text: '取消作废',
                handler: function () {
                    invorkBatchBcf("Invalid", true);
                }
            }]
        });
        //结案
        var endCase = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            text: '结案',
            iconCls: 'fa fa-file-zip-o',
            hidden: contains(permissionList, 512),
            handler: function () {
                invorkBatchBcf("EndCase", false);
            },
            menu: [{
                text: '取消结案',
                handler: function () {
                    invorkBatchBcf("EndCase", true);
                }
            }]
        });
        var returnListing = Ext.create(Ext.Action, {
            text: '清单',
            //handler: function () {
            //    getBillListing(); //1);
            //},
            iconCls: 'fa fa-file-text',
            menu: [{
                text: '首页',
                handler: function () {
                    gridPanel.store.loadPage(1);
                    //MoveFirstOrLastPage(false);
                }
            }, {
                text: '上页',
                handler: function () {
                    //MovePage(false);
                    gridPanel.store.previousPage();
                }
            }, {
                text: '下页',
                handler: function () {
                    //MovePage(true);
                    var lastPage = Math.ceil(gridPanel.store.getTotalCount() / gridPanel.store.getPageSize());
                    if (gridPanel.store.currentPage < lastPage)
                        gridPanel.store.nextPage();
                }
            }, {
                text: '末页',
                handler: function () {
                    //MoveFirstOrLastPage(true);
                    var lastPage = Math.ceil(gridPanel.store.getTotalCount() / gridPanel.store.getPageSize());
                    gridPanel.store.loadPage(lastPage);
                }
            }]
        });
        //导入
        var importData = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            text: '导入',
            iconCls: 'fa fa-download',
            hidden: contains(permissionList, 8192),
            handler: function () {
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
                                        function call() {
                                            Ext.Ajax.request({
                                                url: '/billSvc/invorkBatchImport',
                                                jsonData: { handle: UserHandle, progId: progId, fileName: o.result.FileName, entryParam: entryParam },
                                                method: 'POST',
                                                async: false,
                                                timeout: 90000000,
                                                success: function (response) {
                                                    var result = Ext.decode(response.responseText);
                                                    result = Ext.decode(result.BatchImportDataResult);
                                                    if (result.Messages.length > 0) {
                                                        var ex = [];
                                                        for (var i = 0; i < result.Messages.length; i++) {
                                                            var msgKind = result.Messages[i].MessageKind;
                                                            ex.push({ kind: msgKind, msg: result.Messages[i].Message });
                                                        }
                                                        Ax.utils.LibMsg.show(ex);
                                                    };
                                                },
                                                failure: function (response) {
                                                    Ext.Msg.alert("提示", "方法调用失败");
                                                }
                                            });
                                        }
                                        call.apply(this);
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
                    constrainHeader: true,
                    minimizable: true,
                    maximizable: true,
                    items: [panel]
                });
                win.show();
            }
        });
        //导出
        var exportData = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            text: '导出',
            iconCls: 'fa fa-upload',
            hidden: contains(permissionList, 16384),
            handler: function () {
                var batchParams = [];
                var records = gridPanel.getView().getSelectionModel().getSelection();
                if (records.length > 0) {
                    var batchMethod = [];
                    var count = gridPanel.Pks.length;
                    for (var i = 0; i < records.length; i++) {
                        var pk = [];
                        for (var r = 0; r < count; r++) {
                            var pkStr = gridPanel.Pks[r];
                            var value = records[i].get(pkStr);
                            pk.push(value);
                        }
                        batchParams.push(pk);
                    }
                    call.apply(this);
                }
                else {
                    Ext.Msg.alert("提示", "未选择数据");
                }
                function call() {
                    Ext.Ajax.request({
                        url: '/billSvc/invorkBatchExport',
                        jsonData: { handle: UserHandle, progId: progId, batchParams: batchParams },
                        method: 'POST',
                        async: false,
                        timeout: 90000000,
                        success: function (response) {
                            var result = Ext.decode(response.responseText);
                            var fileName = result.BatchExportDataResult;
                            if (fileName && fileName !== '') {
                                DesktopApp.IgnoreSkip = true;
                                try {
                                    window.location.href = '/TempData/ExportData/' + fileName;
                                } finally {
                                    DesktopApp.IgnoreSkip = false
                                }
                            }
                        },
                        failure: function (response) {
                            Ext.Msg.alert("提示", "方法调用失败");
                        }
                    });
                }
            },
            menu: [{
                text: '导出全部',
                handler: function () {
                    var pkStr = '';
                    var filter = filterFun();
                    var count = gridPanel.Pks.length;
                    for (var r = 0; r < count; r++) {
                        var str = r == 0 ? 'A.' + gridPanel.Pks[r] : ',A.' + gridPanel.Pks[r];
                        pkStr += str;
                    }
                    function call() {
                        Ext.Ajax.request({
                            url: '/billSvc/exportAllData',
                            jsonData: { handle: UserHandle, progId: progId, pkStr: pkStr, listingQuery: { Handle: UserHandle, ProgId: progId, Condition: queryCondition, TimeFilter: dateCbo.getValue(), Filter: filter, EntryParam: entryParam } },
                            method: 'POST',
                            async: false,
                            timeout: 90000000,
                            success: function (response) {
                                var result = Ext.decode(response.responseText);
                                var fileName = result.ExportAllDataResult;
                                if (fileName && fileName !== '') {
                                    window.location.href = '/TempData/ExportData/' + fileName;
                                }
                            },
                            failure: function (response) {
                                Ext.Msg.alert("提示", "方法调用失败");
                            }
                        });
                    }
                    call.apply(this);
                }
            }]
        });
        //打印
        var print = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            text: '打印',
            iconCls: 'fa fa-print',
            hidden: contains(permissionList, 32768),
            handler: function () {

            }
        });
        // 设置查询方案
        var setSearchScheme = Ext.create(Ext.Action, {
            text: '设置查询方案',
            iconCls: 'fa fa-save',
            handler: function () {
                Ax.utils.LibVclSystemUtils.openBill('axp.RptSearchField', BillTypeEnum.Bill, '报表查询参数设置', BillActionEnum.AddNew, undefined, undefined, { progId: progId, progName: displayText });
            }
        });

        var saveDisplayScheme = Ext.create(Ext.Action, {
            text: '保存方案',
            iconCls: 'fa fa-save',
            handler: function () {
                var displayScheme = { ProgId: this.progId, GridScheme: {} };
                if (gridPanel) {
                    var gridScheme = { GridFields: [] };
                    var columns = gridPanel.headerCt.items.items;
                    var buildBandCol = function (bandColumn, list) {
                        if (bandColumn.items.items.length > 0) {
                            var subList = [];
                            list.push({ Header: bandColumn.text, BandFields: subList });
                            for (var r = 0; r < bandColumn.items.items.length; r++) {
                                if (bandColumn.items.items[r].hidden === true)
                                    continue;
                                buildBandCol(bandColumn.items.items[r], subList);
                            }
                        }
                        else {
                            list.push({ Field: { Name: bandColumn.dataIndex, Width: bandColumn.width } });
                        }
                    }
                    for (var l = 0; l < columns.length; l++) {
                        if (columns.xtype == "rownumberer" || columns[l].hidden === true)
                            continue;
                        buildBandCol(columns[l], gridScheme.GridFields);
                    }
                    if (my.gridScheme != undefined)
                        displayScheme.GridScheme[0] = gridScheme;
                    Ext.Ajax.request({
                        url: '/billSvc/saveBillListingScheme',
                        method: 'POST',
                        jsonData: {
                            handle: UserHandle, progId: progId, entryParam: entryParam, displayScheme: Ext.encode(displayScheme)
                        },
                        async: false,
                        timeout: 90000000,
                        success: function (response) {
                            alert('方案保存成功。');
                        }
                    });
                }
            },
            menu: [{
                text: '删除方案',
                handler: function () {
                    Ext.Ajax.request({
                        url: '/billSvc/clearBillListingScheme',
                        method: 'POST',
                        jsonData: {
                            handle: UserHandle, progId: progId, entryParam: entryParam
                        },
                        async: false,
                        timeout: 90000000,
                        success: function (response) {
                            alert('方案删除成功。');
                        }
                    });
                }
            }]
        });
        function checkAllPermission() {
            var CheckAllPermission;
            Ext.Ajax.request({
                url: 'billSvc/checkAllPermission',
                method: 'POST',
                jsonData: {
                    handle: UserHandle, progId: progId
                },
                async: false,
                timeout: 90000000,
                success: function (response) {
                    var ret = Ext.decode(response.responseText)
                    CheckAllPermission = ret.CheckAllPermissionResult;
                },
                failure: function (response) {
                    CheckAllPermission = undefined;
                    console.log('权限按钮方法调用失败')
                    // Ext.Msg.alert("提示", "权限按钮方法调用失败");
                }
            })
            return CheckAllPermission;
        }
        function contains(arr, obj) {
            if (arr == undefined)
                return false;

            var i = arr.length;
            if (i == 0)
                return false;

            while (i--) {
                if (arr[i] === obj) {
                    return false;
                }
            }
            return true;
        }
        function invorkBatchBcf(methodName, cancel, callback) {
            var batchParams = [];
            var records = gridPanel.getView().getSelectionModel().getSelection();
            if (records.length > 0) {
                var batchMethod = [];
                var count = gridPanel.Pks.length;
                for (var i = 0; i < records.length; i++) {
                    var pk = [];
                    for (var r = 0; r < count; r++) {
                        var pkStr = gridPanel.Pks[r];
                        var value = records[i].get(pkStr);
                        pk.push(value);
                    }
                    if (cancel)
                        batchParams.push([Ext.encode(pk), Ext.encode(cancel)]);
                    else
                        batchParams.push([Ext.encode(pk)]);
                }
                call.apply(this);
            }
            else {
                Ext.Msg.alert("提示", "请先选择一行数据");
            }
            function call() {
                Ext.Ajax.request({
                    url: '/billSvc/invorkBatchBcf',
                    jsonData: { param: { ProgId: progId, MethodName: methodName, Handle: UserHandle }, batchParams: batchParams },
                    method: 'POST',
                    async: false,
                    timeout: 90000000,
                    success: function (response) {
                        var result = Ext.decode(response.responseText);
                        result = Ext.decode(result.BatchExecBcfMethodResult);
                        if (callback) {
                            callback(result.Result);
                        }
                        if (result.Messages.length > 0) {
                            var ex = [];
                            for (var i = 0; i < result.Messages.length; i++) {
                                var msgKind = result.Messages[i].MessageKind;
                                ex.push({ kind: msgKind, msg: result.Messages[i].Message });
                            }
                            Ax.utils.LibMsg.show(ex);
                        };
                    },
                    failure: function (response) {
                        Ext.Msg.alert("提示", "方法调用失败");
                    }
                });
            }
        };

        if (billType == BillTypeEnum.Master) {
            items = [select, refresh, browse, addNew, edit, deleteBill, submitAudit, audit, returnListing, importData, exportData, print, saveDisplayScheme, setSearchScheme];
        } else {
            items = [select, refresh, browse, addNew, edit, deleteBill, release, invalid, submitAudit, audit, endCase, returnListing, importData, exportData, print, saveDisplayScheme, setSearchScheme];
        }

        return items;
    },
    createBillAction: function (vcl, permission, isF4) {
        function checkAllPermission() {
            var CheckAllPermission;
            Ext.Ajax.request({
                url: 'billSvc/checkAllPermission',
                method: 'POST',
                jsonData: {
                    handle: UserHandle, progId: vcl.progId
                },
                async: false,
                timeout: 90000000,
                success: function (response) {
                    var ret = Ext.decode(response.responseText)
                    CheckAllPermission = ret.CheckAllPermissionResult;
                },
                failure: function (response) {
                    CheckAllPermission = undefined;
                    // Ext.Msg.alert("提示", "权限按钮方法调用失败");
                }
            })
            return CheckAllPermission;
        }
        function contains(arr, obj) {
            if (arr == undefined)
                return false;

            var i = arr.length;
            if (i == 0)
                return false;

            while (i--) {
                if (arr[i] === obj) {
                    return false;
                }
            }
            return true;
        }
        //var permissionList = checkAllPermission();
        var permissionList = [];
        //新增
        var addNew = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            iconCls: 'fa fa-plus',
            text: '新增',
            hidden: contains(permissionList, 4),
            handler: function () {
                if (arguments[0]["noOpen"] === undefined)
                    Ax.utils.LibVclSystemUtils.openBill(vcl.progId, vcl.billType, vcl.win.progName, BillActionEnum.AddNew, vcl.entryParam, undefined, undefined);
                else
                    setAction(vcl.billType, vcl.isEdit, true);
            },
            menu: [{
                text: '复制',
                iconCls: 'fa fa-copy',
                handler: function () {
                    Ax.utils.LibVclSystemUtils.openBill(vcl.progId, vcl.billType, vcl.win.progName, BillActionEnum.AddNew, vcl.entryParam, vcl.currentPk, undefined);
                }
            }]
        });

        //修改
        var edit = Ext.create(Ext.Action, {
            text: '修改',
            iconCls: 'fa fa-edit',
            hidden: contains(permissionList, 8),
            handler: function () {
                debugger;
                var success = true;
                billAction = vcl.billAction;
                if (vcl.isEdit) {
                    success = vcl.doSave();
                    if (success) {
                        if (billAction == 2 || billAction == 3) {
                            if (vcl.entryParam == undefined || vcl.entryParam == "") {
                                if (Ext.getCmp(vcl.progId.replace(".", '')) != undefined) {
                                    Ext.getCmp(vcl.progId.replace(".", '')).toolBarAction[1].execute();
                                }
                            }
                            else {
                                var entryParam = "";
                                if (vcl.entryParam != "") {
                                    var jsonString = JSON.stringify(vcl.entryParam.ParamStore);
                                    jsonString = jsonString.substring(jsonString.indexOf(':') + 1, jsonString.indexOf('}'));
                                    var temp = jsonString.split(",");
                                    for (var i = 0; i < temp.length; i++) {
                                        entryParam = entryParam + temp[i].substring(temp[i].indexOf(":") + 1);
                                    }
                                }
                                Ext.getCmp(vcl.progId.replace(".", '') + entryParam.replace(new RegExp('"', 'gm'), '')).toolBarAction[1].execute();
                            }
                        }
                    }
                }
                else {
                    success = vcl.doEdit();
                }
                if (success)
                    setAction(vcl.billType, vcl.isEdit, false);
            }
        });
        //删除
        var deleteBill = Ext.create(Ext.Action, {
            text: '删除',
            iconCls: 'fa fa-trash',
            hidden: contains(permissionList, 16),
            handler: function () {
                var myWin = this;
                Ext.Msg.confirm("系统提示", "确定要删除吗？", function (btn) {
                    if (btn == "yes") {
                        var success = vcl.doDelete(vcl.currentPk);
                        if (success) {
                            if (vcl.entryParam == undefined) {
                                if (Ext.getCmp(vcl.progId.replace(".", '')) != undefined) {
                                    Ext.getCmp(vcl.progId.replace(".", '')).toolBarAction[1].execute();
                                }
                            }
                            else {
                                if (vcl.entryParam != "") {
                                    var entryParam = "";
                                    var jsonString = JSON.stringify(vcl.entryParam.ParamStore);
                                    jsonString = jsonString.substring(jsonString.indexOf(':') + 1, jsonString.indexOf('}'));
                                    var temp = jsonString.split(",");
                                    for (var i = 0; i < temp.length; i++) {
                                        entryParam = entryParam + temp[i].substring(temp[i].indexOf(":") + 1);
                                    }
                                }
                                Ext.getCmp(vcl.progId.replace(".", '') + vcl.entryParam.replace(new RegExp('"', 'gm'), '')).toolBarAction[1].execute();
                            }
                        }
                        if (success)
                            myWin.up('window').close();
                    }
                });
            }
        });

        //送审
        var submitAudit = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            text: '送审',
            iconCls: 'fa fa-send',
            handler: function () {
                var success = vcl.doSubmitAudit(false);
                if (success)
                    setAction(vcl.billType, vcl.isEdit, false);
            },
            menu: [{
                text: '撤回',
                handler: function () {
                    var success = vcl.doSubmitAudit(true);
                    if (success)
                        setAction(vcl.billType, vcl.isEdit, false);
                }
            }]
        });
        //审核
        var audit = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            text: '审核',
            iconCls: 'fa fa-check-circle',
            hidden: contains(permissionList, 128),
            handler: function () {
                var success = Ax.utils.LibApproveConfirmForm.show(vcl);
                if (success)
                    setAction(vcl.billType, vcl.isEdit, false);
            },
            menu: [{
                text: '弃审',
                handler: function () {
                    var success = Ax.utils.LibApproveWithdrawForm.show(vcl);
                    if (success)
                        setAction(vcl.billType, vcl.isEdit, false);
                }
            }]
        });
        //生效
        var release = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            iconCls: 'fa fa-check-square',
            hidden: contains(permissionList, 32),
            text: '生效',
            handler: function () {
                var success = vcl.doTakeRelease(false);
                if (success)
                    setAction(vcl.billType, vcl.isEdit, false);
            },
            menu: [{
                text: '取消生效',
                handler: function () {
                    var success = vcl.doTakeRelease(true);
                    if (success)
                        setAction(vcl.billType, vcl.isEdit, false);
                }
            }]
        });
        //作废
        var invalid = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            text: '作废',
            iconCls: 'fa fa-file-excel-o',
            hidden: contains(permissionList, 2048),
            handler: function () {
                var success = vcl.doInvalid(false);
                if (success)
                    setAction(vcl.billType, vcl.isEdit, false);
            },
            menu: [{
                text: '取消作废',
                handler: function () {
                    if (vcl.doInvalid(true))
                        setAction(vcl.billType, vcl.isEdit, false);
                }
            }]
        });
        //结案
        var endCase = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            iconCls: 'fa fa-file-zip-o',
            hidden: contains(permissionList, 512),
            text: '结案',
            handler: function () {
                if (vcl.doEndCase(false))
                    setAction(vcl.billType, vcl.isEdit, false);
            },
            menu: [{
                text: '取消结案',
                handler: function () {
                    if (vcl.doEndCase(true))
                        setAction(vcl.billType, vcl.isEdit, false);
                }
            }]
        });

        var saveToDraft = Ext.create(Ext.Action, {
            text: '存为草稿',
            iconCls: 'fa fa-file-word',
            hidden: true,
            handler: function () {
                vcl.doSaveToDraft();
                setAction(vcl.billType, vcl.isEdit, false);
            }
        });

        var submitDraft = Ext.create(Ext.Action, {
            text: '提交',
            iconCls: 'fa fa-hand-o-up',
            handler: function () {
                if (vcl.doSubmitDraft())
                    setAction(vcl.billType, false, false)
            }
        });

        var cancel = Ext.create(Ext.Action, {
            text: '撤销',
            iconCls: 'fa fa-hand-o-down',
            handler: function () {
                if (vcl.billAction == BillActionEnum.AddNew) {
                    this.up('window').close();
                } else {
                    vcl.cancel(vcl.billAction);
                    vcl.isEdit = false;
                    setAction(vcl.billType, vcl.isEdit, false);
                    vcl.billAction = BillActionEnum.Browse;
                }
            }
        });

        var returnListing = Ext.create(Ext.Action, {
            text: '清单',
            iconCls: 'fa fa-file-text',
            hidden: true,
            handler: function () {
                var funcMenu = Ext.getCmp('billMenu-win');
                if (funcMenu) {
                    funcMenu.show();
                    funcMenu.down('tabpanel').setActiveTab(vcl.win.progName);
                }
            },
            menu: [{
                text: '首页',
                handler: function () {
                    moveFirstOrLast(false);
                }
            }, {
                text: '上页',
                handler: function () {
                    moveBill(false);
                }
            }, {
                text: '下页',
                handler: function () {
                    moveBill(true);
                }
            }, {
                text: '末页',
                handler: function () {
                    moveFirstOrLast(true);
                }
            }]
        });


        var refresh = Ext.create(Ext.Action, {
            text: '刷新',
            iconCls: 'fa fa-refresh',
            handler: function () {
                vcl.browseTo(vcl.currentPk);
                setAction(vcl.billType, false, false)
            }
        });

        //导入
        var importData = Ext.create(Ext.Action, {
            text: '导入',
            iconCls: 'fa fa-download',
            hidden: contains(permissionList, 8192),
            handler: function () {
                if (vcl.billAction != BillActionEnum.AddNew) {
                    Ext.Msg.alert('提示', '新增状态下才可以导入。');
                    return;
                }
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
                                        if (vcl.importData(o.result.FileName) == true) {
                                            Ext.Msg.alert('提示', '文件 "' + o.result.FileName + '" 导入成功.');
                                            setAction(vcl.billType, vcl.isEdit, false);
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
            }
        });
        //导出
        var exportData = Ext.create(Ext.Action, {
            text: '导出',
            iconCls: 'fa fa-upload',
            hidden: contains(permissionList, 16384),
            handler: function () {
                var fileName = vcl.exportData();
                if (fileName && fileName !== '') {
                    DesktopApp.IgnoreSkip = true;
                    try {
                        window.location.href = '/TempData/ExportData/' + fileName;
                    } finally {
                        DesktopApp.IgnoreSkip = false
                    }
                }
            }
        });

        //打印
        var print = Ext.create(Ext.Action, {
            text: '打印',
            iconCls: 'fa fa-print',
            hidden: contains(permissionList, 32768),
            handler: function () {
                vcl.print(vcl.progId);
            }
        });

        var checkUsed = Ext.create(Ext.Action, {
            text: '被用',
            handler: function () {

            }
        });

        var flow = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            text: '审核流程',
            iconCls: 'fa fa-stack-overflow',
            handler: function () {
                var approveForm = new Ax.utils.LibApproveForm(vcl);
                approveForm.show();
            },
            menu: [
              {
                  text: '单据版本',
                  handler: function () {
                      Ax.utils.LibApproveVersionForm.show(vcl);
                  }
              },
              {
                  text: '流程配置',
                  handler: function () {
                      var approveForm = new Ax.utils.LibApproveForm(vcl, true);//可以选择查看审核流配置信息
                      approveForm.show();
                  }
              }
            ]
        });

        var attachment = Ext.create(Ext.Action, {
            text: '附件',
            iconCls: 'fa fa-folder',
            handler: function () {
                var table = vcl.dataSet.getTable(0);
                Ax.utils.LibAttachmentForm.show(vcl, table.data.items[0], table.Name);
            }
        });

        var saveDisplayScheme = Ext.create(Ext.Action, {
            xtype: 'splitbutton',
            text: '保存方案',
            iconCls: 'fa fa-save',
            handler: function () {
                vcl.saveDisplayScheme();
            },
            menu: [{
                text: '删除方案',
                handler: function () {
                    vcl.clearDisplayScheme();
                }
            }]
        });

        function moveFirstOrLast(isLast) {
            var newPk = [];
            var entryParam = "";
            var jsonString;
            var temp;
            if (vcl.entryParam != undefined) {
                jsonString = JSON.stringify(vcl.entryParam.ParamStore);
                jsonString = jsonString.substring(jsonString.indexOf(':') + 1, jsonString.indexOf('}'));
                temp = jsonString.split(",");
                for (var i = 0; i < temp.length; i++) {
                    entryParam = entryParam + temp[i].substring(temp[i].indexOf(":") + 1);
                }
            }
            var tab = Ext.getCmp(vcl.progId.replace(".", '') + entryParam.replace(new RegExp('"', 'gm'), ''));
            if (tab) {
                var grid = tab.down('gridpanel');
                var length = grid.store.data.items.length;
                if (length > 0) {
                    var pkStr = vcl.dataSet.getTable(0).Pks;
                    var index = isLast ? length - 1 : 0;
                    for (var l = 0; l < pkStr.length; l++) {
                        newPk.push(grid.store.data.items[index].get(pkStr[l]));
                    }
                }
            }
            if (newPk.length > 0) {
                vcl.approveRowForm = null;
                vcl.currentPk = newPk;
                vcl.browseTo(vcl.currentPk);
            }
        };


        function moveBill(isNext) {
            var newPk = [];
            var isFirst, isLast;
            var entryParam = "";
            var jsonString;
            var temp;
            if (vcl.entryParam != undefined) {
                jsonString = JSON.stringify(vcl.entryParam.ParamStore);
                jsonString = jsonString.substring(jsonString.indexOf(':') + 1, jsonString.indexOf('}'));
                temp = jsonString.split(",");
                for (var i = 0; i < temp.length; i++) {
                    entryParam = entryParam + temp[i].substring(temp[i].indexOf(":") + 1);
                }
            }
            var tab = Ext.getCmp(vcl.progId.replace(".", '') + entryParam.replace(new RegExp('"', 'gm'), ''));
            if (tab) {
                var grid = tab.down('gridpanel');
                var length = grid.store.data.items.length;
                var pkStr = vcl.dataSet.getTable(0).Pks;
                var index = getCurrentIndex(grid);
                var newRecord;
                if (index != -1) {
                    if (isNext)
                        newRecord = grid.store.data.items[++index];
                    else
                        newRecord = grid.store.data.items[--index];
                    if (newRecord) {
                        for (var l = 0; l < pkStr.length; l++) {
                            newPk.push(newRecord.get(pkStr[l]));
                        }
                        isFirst = index == 0;
                        isLast = (index + 1) == length;
                    }
                    else {
                        //如果找不到，说明清单数据已变化，默认定位到清单第一笔数据
                        moveFirstOrLast(false);
                    }
                }
            }
            if (newPk.length > 0) {
                vcl.approveRowForm = null;
                vcl.currentPk = newPk;
                vcl.browseTo(vcl.currentPk);
                if (vcl.picture) {
                    var masterRow = vcl.dataSet.getTable(0).data.items[0];
                    var imgSrc = masterRow.get('IMGSRC');
                    if (imgSrc !== undefined && imgSrc != '') {
                        vcl.picture.setSrc(imgSrc = '/UserPicture/' + vcl.progId + '/' + masterRow.get('INTERNALID') + '/' + imgSrc);
                    } else {
                        vcl.picture.setSrc('');
                    }
                }
            }
        }

        function getCurrentIndex(grid) {
            var length = grid.store.data.items.length;
            var pkStr = vcl.dataSet.getTable(0).Pks;
            var index = -1;
            for (var i = 0; i < length; i++) {
                var find = true;
                var record = grid.store.data.items[i];
                for (var r = 0; r < pkStr.length; r++) {
                    if (vcl.currentPk[r] != record.get(pkStr[r])) {
                        find = false;
                        break;
                    }
                }
                if (find) {
                    index = i;
                    break;
                }
            }
            return index;
        }

        function setAction(billType, isEdit, isAddNew) {
            var record = vcl.dataSet.getTable(0).data.items[0];
            var currentState = record.get('CURRENTSTATE');
            var auditState = record.get('AUDITSTATE');
            if (isEdit) {
                addNew.setDisabled(true);
                edit.setText("保存");
                if (isAddNew === true)
                    saveToDraft.setDisabled(false);
                else
                    saveToDraft.setDisabled(true);
                cancel.setDisabled(false);
                if (isF4 !== true) {
                    //firstBill.setDisabled(true);
                    //preBill.setDisabled(true);
                    //nextBill.setDisabled(true);
                    //lastBill.setDisabled(true);
                    returnListing.setDisabled(true);
                }
                refresh.setDisabled(true);
                deleteBill.setDisabled(true);
                audit.setDisabled(true);
                submitDraft.setDisabled(true);
                submitAudit.setDisabled(true);
                if (billType == BillTypeEnum.Bill) {
                    release.setDisabled(true);
                    invalid.setDisabled(true);
                    endCase.setDisabled(true);
                }

                //debugger;
                //vcl.form.every(function (item) {
                //    vcl.form.items.each(function (item) {
                //        if (item.xtype == "tabpanel") {
                //            item.items.each(function (childItem) {
                //                if (childItem.xtype == "panel") {
                //                    childItem.items.each(function (childItemDetial) {
                //                        childItemDetial.setReadOnly(true);
                //                    })
                //                }
                //            })
                //        }
                //        else if (item.xtype == "panel") {

                //        }
                //        else {
                //            item.setReadOnly(true);
                //            if (item.onEdit != undefined) {
                //                item.onEdit();
                //            }
                //        }
                //    });
                //});
                //var data =vcl.form panel.items.items["0"].items;
                //data.each(function (item) {
                //    if (item.xtype == "tabpanel") {
                //        item.items.each(function (childItem) {
                //            if (childItem.xtype == "panel") {
                //                childItem.items.each(function (childItemDetial) {
                //                    childItemDetial.setReadOnly(true);
                //                })
                //            }
                //        })
                //    }
                //    else if (item.xtype == "panel") {

                //    }
                //    else {
                //        item.setReadOnly(true);
                //    }
                //});
            } else {
                addNew.setDisabled(false);
                edit.setText("修改");
                saveToDraft.setDisabled(true);
                cancel.setDisabled(true);
                if (isF4 !== true) {
                    //firstBill.setDisabled(false);
                    //preBill.setDisabled(false);
                    //nextBill.setDisabled(false);
                    //lastBill.setDisabled(false);
                    returnListing.setDisabled(false);
                }
                refresh.setDisabled(false);
                deleteBill.setDisabled(false);
                if ((currentState == 1 || currentState == 2) && (auditState == 1 || auditState == 2))
                    audit.setDisabled(false);
                else
                    audit.setDisabled(true);

                if (currentState == 0)
                    submitDraft.setDisabled(false);
                else
                    submitDraft.setDisabled(true);
                if ((currentState == 1 || currentState == 2) && auditState != 2)
                    submitAudit.setDisabled(false);
                else
                    submitAudit.setDisabled(true);
                if (billType == BillTypeEnum.Bill) {
                    if (currentState == 0 || currentState == 3 || currentState == 4)
                        release.setDisabled(true);
                    else
                        release.setDisabled(false);
                    if (currentState == 0 || currentState == 1 || currentState == 4)
                        invalid.setDisabled(true);
                    else
                        invalid.setDisabled(false);
                    if (currentState == 0 || currentState == 1 || currentState == 3)
                        endCase.setDisabled(true);
                    else
                        endCase.setDisabled(false);
                }
            }
        };

        if (vcl.billType == BillTypeEnum.Master) {
            if (isF4 === true)
                items = [addNew, edit, saveToDraft, submitDraft, cancel, refresh, deleteBill, submitAudit, audit, flow, importData, exportData, print, attachment, saveDisplayScheme];
            else
                items = [addNew, edit, saveToDraft, submitDraft, cancel, returnListing, refresh, deleteBill, submitAudit, audit, flow, importData, exportData, print, attachment, saveDisplayScheme];
        } else {
            if (isF4 === true)
                items = [addNew, edit, saveToDraft, submitDraft, cancel, refresh, submitDraft, deleteBill, release, invalid, submitAudit, audit, flow, endCase, importData, exportData, print, attachment, saveDisplayScheme];
            else
                items = [addNew, edit, saveToDraft, submitDraft, cancel, returnListing, refresh, submitDraft, deleteBill, release, invalid, submitAudit, audit, flow, endCase, importData, exportData, print, attachment, saveDisplayScheme];
        }
        var changeView = vcl.createChangeView(view, 'default', 'create', '默认视图')
        if (changeView)
            items.push(changeView);
        setAction(vcl.billType, vcl.isEdit, vcl.billAction == BillActionEnum.AddNew);
        if (vcl.billAction != BillActionEnum.AddNew) {
            var tab = Ext.getCmp(vcl.progId);
            if (tab) {
                var grid = tab.down('gridpanel');
                var index = getCurrentIndex(grid)
                if (index != -1) {
                    var isFirst = index == 0;
                    var isLast = (index + 1) == grid.store.data.items.length;
                }
            };
        }
        return items;
    },
    createGridAction: function (view, permission) {
        var vcl = view.vcl;
        //查询
        var select = Ext.create(Ext.Action, {
            text: '查询',
            iconCls: 'fa fa-search',
            handler: function () {
                Ax.utils.LibQueryForm.createForm(vcl);
            }
        });

        //修改
        var edit = Ext.create(Ext.Action, {
            text: '修改',
            iconCls: 'fa fa-edit',
            handler: function () {
                debugger;
                var success = true;
                if (vcl.isEdit) {
                    success = vcl.doSave();
                }
                else {
                    success = vcl.doEdit(vcl.queryCondition);
                }
                if (success)
                    setAction(vcl.isEdit);
            }
        });

        //撤销
        var cancel = Ext.create(Ext.Action, {
            text: '撤销',
            iconCls: 'fa fa-hand-o-down',
            handler: function () {
                vcl.cancel(vcl.billAction);
                vcl.isEdit = false;
                setAction(vcl.isEdit);
                vcl.billAction = BillActionEnum.Browse;
            }
        });
        //刷新
        var refresh = Ext.create(Ext.Action, {
            text: '刷新',
            iconCls: 'fa fa-refresh',
            handler: function () {
                vcl.browseTo(vcl.queryCondition);
            }
        });

        //导入
        var importData = Ext.create(Ext.Action, {
            text: '导入',
            iconCls: 'fa fa-download',
            handler: function () {
                if (this.isEdit == false) {
                    Ext.Msg.alert('提示', '浏览状态下才可以导入。');
                    return;
                }
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
                                        if (vcl.importData(o.result.FileName) == true)
                                            Ext.Msg.alert('提示', '文件 "' + o.result.FileName + '" 已导入成功.');
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
            }
        });
        //导出
        var exportData = Ext.create(Ext.Action, {
            text: '导出',
            iconCls: 'fa fa-upload',
            handler: function () {
                var fileName = vcl.exportData(vcl.queryCondition);
                if (fileName && fileName !== '') {
                    DesktopApp.IgnoreSkip = true;
                    try {
                        window.location.href = '/TempData/ExportData/' + fileName;
                    } finally {
                        DesktopApp.IgnoreSkip = false
                    }
                }
            }
        });

        //打印
        var print = Ext.create(Ext.Action, {
            text: '打印',
            iconCls: 'fa fa-print',
            handler: function () {

            }
        });

        var saveDisplayScheme = Ext.create(Ext.Action, {
            text: '保存方案',
            iconCls: 'fa fa-save',
            handler: function () {
                vcl.saveDisplayScheme();
            },
            menu: [{
                text: '删除方案',
                handler: function () {
                    vcl.clearDisplayScheme();
                }
            }]
        });

        /*var setSearchScheme = Ext.create(Ext.Action, {
          text: '设置查询方案',
          iconCls: 'fa fa-save',
          handler: function () {
            Ax.utils.LibVclSystemUtils.openBill('axp.RptSearchField', BillTypeEnum.Bill, '报表查询参数设置', BillActionEnum.AddNew, undefined, undefined, {progId: vcl.progId, progName: vcl.tpl.DisplayText});
          }
        });
        */

        function setAction(isEdit) {
            if (isEdit) {
                edit.setText("保存");
                cancel.setDisabled(false);
                refresh.setDisabled(true);
                select.setDisabled(true);
            } else {
                edit.setText("修改");
                cancel.setDisabled(true);
                refresh.setDisabled(false);
                select.setDisabled(false);
            }
        };
        var items = [select, edit, cancel, refresh, importData, exportData, print, saveDisplayScheme];
        var changeView = vcl.createChangeView(view, 'default', 'create', '默认视图')
        if (changeView)
            items.push(changeView);
        setAction(vcl.isEdit);
        return items;
    },
    createRptAction: function (vcl, permission) {
        //查询
        var select = Ext.create(Ext.Action, {
            text: '查询',
            iconCls: 'fa fa-search',
            handler: function () {
                Ax.utils.LibQueryForm.createForm(vcl);
            }
        });
        //刷新
        var refresh = Ext.create(Ext.Action, {
            text: '刷新',
            iconCls: 'fa fa-refresh',
            handler: function () {
                vcl.showRpt(vcl.queryCondition);
            }
        });
        //导出
        var exportData = Ext.create(Ext.Action, {
            text: '导出',
            iconCls: 'fa fa-upload',
            handler: function () {
                var fileName = vcl.exportData(vcl.queryCondition);
                if (fileName && fileName !== '') {
                    window.location.href = '/TempData/ExportData/' + fileName;
                }
            }
        });
        //导出
        var exportMainData = Ext.create(Ext.Action, {
            text: '导出主表',
            iconCls: 'fa fa-cloud-upload',
            handler: function () {
                var fileName = vcl.exportMainData(vcl.queryCondition);
                if (fileName && fileName !== '') {
                    DesktopApp.IgnoreSkip = true;
                    try {
                        window.location.href = '/TempData/ExportData/' + fileName;
                    } finally {
                        DesktopApp.IgnoreSkip = false
                    }
                }
            }
        });

        //打印
        var print = Ext.create(Ext.Action, {
            text: '打印',
            iconCls: 'fa fa-print',
            handler: function () {
                vcl.print(vcl.progId);
            }
        });
        var saveDisplayScheme = Ext.create(Ext.Action, {
            text: '保存方案',
            iconCls: 'fa fa-save',
            handler: function () {
                vcl.saveDisplayScheme();
            },
            menu: [{
                text: '删除方案',
                handler: function () {
                    vcl.clearDisplayScheme();
                }
            }]
        });
        var setSearchScheme = Ext.create(Ext.Action, {
            text: '设置查询方案',
            iconCls: 'fa fa-save',
            handler: function () {
                if (vcl.setSearchScheme)
                    vcl.setSearchScheme();
                else
                    Ext.Msg.alert('提示', '未实现该功能！');
            }
        });
        var items = [select, refresh, exportMainData, exportData, print, saveDisplayScheme, setSearchScheme];
        var changeView = vcl.createChangeView(view, 'default', 'create', '默认视图')
        if (changeView)
            items.push(changeView);
        return items;
    },
    createDataFuncAction: function (vcl, permission) {
        var saveDisplayScheme = Ext.create(Ext.Action, {
            text: '保存方案',
            iconCls: 'fa fa-save',
            handler: function () {
                vcl.saveDisplayScheme();
            },
            menu: [{
                text: '删除方案',
                handler: function () {
                    vcl.clearDisplayScheme();
                }
            }]
        });
        var items = [saveDisplayScheme];
        var changeView = vcl.createChangeView(view, 'default', 'create', '默认视图')
        if (changeView)
            items.push(changeView);
        return items;
    },
    createVisualAction: function (vcl, permission) {
        //查询
        var select = Ext.create(Ext.Action, {
            text: '查询',
            iconCls: 'fa fa-search',
            handler: function () {
                Ax.utils.LibQueryForm.createForm(vcl);
            }
        });
        //刷新
        var refresh = Ext.create(Ext.Action, {
            text: '刷新',
            iconCls: 'fa fa-refresh',
            handler: function () {
                vcl.showData();
            }
        });
        var items = [select, refresh];
        var changeView = vcl.createChangeView(view, 'default', 'create', '默认视图')
        if (changeView)
            items.push(changeView);
        return items;
    },
    createToolBar: function (actions) {
        if (actions === undefined)
            return;
        var items = [];
        for (var i = 0; i < actions.length; i++) {
            if (actions[i].initialConfig.menu)
                items.push(Ext.create('Ext.button.Split', actions[i]));
            else
                items.push(Ext.create('Ext.button.Button', actions[i]));
        }
        var tbar = Ext.create('Ext.toolbar.Toolbar', {
            xtype: 'container',
            autoScroll: true,
            border: false,
            items: items
        });
        return tbar;
    }
};
