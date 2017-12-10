/// <reference path="../../../ext/bootstrap.js" />
/// <reference path="../../../ext/ext-all.js" />
/// <reference path="../../../ext/locale/ext-lang-zh_CN.js" />
/// <reference path="../../../ext/ext.js" />

Ext.ns('Ax.vcl');

Ax.vcl.LibVclBase = function () {
    this.progId = '';
    this.billType;
    this.dataSet = new Ax.vcl.LibDataSet();
    this.subPanels = {};
    this.tpl;
    this.winId;
    this.entryParam;
    this.subGridScheme = {};
    this.summaryRenderer = {};
    this.funcView = new Ext.util.MixedCollection();
    this.sortRuler;
    // 根据某字段改变变身字体颜色
    this.fontColor;
    this.isConfirmClose = false;
};

Ax.vcl.LibVclBase.prototype = {
    constructor: Ax.vcl.LibVclBase,
    invorkBcf: function (methodName, methodParam, assistObj) {
        var relParam = [];
        if (methodParam) {
            for (var i = 0; i < methodParam.length; i++) {
                relParam.push(Ext.encode(methodParam[i]));
            }
        };
        var result;
        function call() {
            Ext.Ajax.request({
                url: 'billSvc/invorkBcf',
                jsonData: { param: { ProgId: this.progId, MethodName: methodName, MethodParam: relParam, Handle: UserHandle } },
                method: 'POST',
                async: false,
                timeout: 90000000,
                success: function (response) {
                    result = Ext.decode(response.responseText);
                    result = Ext.decode(result.ExecuteBcfMethodResult);
                    if (result.Messages.length > 0) {
                        var ex = [];
                        for (var i = 0; i < result.Messages.length; i++) {
                            var msgKind = result.Messages[i].MessageKind;
                            if (assistObj && !assistObj.hasError && (msgKind == 2 || msgKind == 3))
                                assistObj.hasError = true;
                            ex.push({ kind: msgKind, msg: result.Messages[i].Message });
                        }
                        Ax.utils.LibMsg.show(ex);
                    };
                },
                failure: function (response) {
                    if (assistObj)
                        assistObj.hasError = true;
                    Ax.utils.LibMsg.show([{ kind: LibMessageKind.SysException, msg: response.responseText }]);
                }
            });
        }
        call.apply(this);
        return result.Result;
    },
    doSetParam: function (paramList) {
    },
    getTpl: function () {
        this.tpl = this.invorkBcf('GetViewTemplate');
    },
    vclHandler: function (sender, e) {

    },
    beforeCheckField: function (tableIndex, fieldName, relSource, fieldValue) {

    },
    checkFieldValue: function (curRow, returnValue, tableIndex, fieldName) {
        if (returnValue) {
            for (p in returnValue) {
                if (!returnValue.hasOwnProperty(p))
                    continue;
                var value = returnValue[p] == null ? "" : returnValue[p];
                if (value !== undefined && value !== null) {
                    if (curRow.getField(p) != null || curRow.data.hasOwnProperty(p)) {
                        curRow.set(p, value);
                        var ctrl = Ext.getCmp(p + tableIndex + '_' + this.winId)
                        if (ctrl) {
                            if (ctrl.xtype == 'libSearchfield' || ctrl.xtype == 'libSearchfieldTree') {
                                var value = returnValue[ctrl.relName] == null ? "" : returnValue[ctrl.relName];
                                var valueid = returnValue[ctrl.name] == null ? "" : returnValue[ctrl.name];
                                ctrl.store.add({ Id: valueid, Name: value });
                                ctrl.select(valueid);
                            }
                            else
                                ctrl.setValue(value);
                        }
                    }
                }
            }
        }
    },
    afterCheckField: function (curRow, tableIndex, fieldName) {

    },
    getRelPk: function (fieldName, relPk, curTableIndex, curRow, curValue, curGrid) {
        var curPks = [];
        if (relPk && relPk != '') {
            var me = this;
            var relPks = relPk.split(';');
            if (relPks) {
                for (i = 0; i < relPks.length; i++) {
                    curPks.push(this.getParamFieldValue(me, relPks[i], curTableIndex, curRow, curGrid));
                }
            }
            else {
                curPks.push(this.getParamFieldValue(me, relPk, curTableIndex, curRow, curGrid));
            }
        }
        curPks.push(curValue);
        return curPks;
    },
    getParamFieldValue: function (vcl, field, curTableIndex, curRow, curGrid) {
        var me = vcl;
        var temps = field.split('.');
        var index = temps[0].charCodeAt() - 'A'.charCodeAt();
        var value;
        if (index == curTableIndex) {
            if (curGrid) {
                value = curRow.get(temps[1]);
            }
            else {
                var ctrl = Ext.getCmp(temps[1] + index + '_' + me.winId);
                if (ctrl)
                    value = ctrl.getValue(temps[1]);
            }
        } else {
            var grid = curGrid;
            if (grid.parentGrid) {
                do {
                    if (grid.parentGrid.tableIndex == index) {
                        value = grid.parentRow.get(temps[1]);
                        break;
                    }
                    grid = grid.parentGrid;
                } while (grid);
            } else {
                var ctrl = Ext.getCmp(temps[1] + index + '_' + me.winId)
                if (ctrl)
                    value = ctrl.getValue(temps[1]);
                else
                    value = me.dataSet.getTable(index).data.items[0].get(temps[1]);
            }
        }
        return value;
    },
    doCheckField: function (tableIndex, fieldName, relSource, relPk, curValue, curRow, curGrid) {
        var fieldValue = {};
        this.beforeCheckField(tableIndex, fieldName, relSource, fieldValue);
        var curPks = this.getRelPk(fieldName, relPk, tableIndex, curRow, curValue, curGrid);
        var returnValue = this.invorkBcf('CheckFieldValue', [tableIndex, fieldName, relSource, curPks, fieldValue]);
        this.checkFieldValue(curRow, returnValue, tableIndex, fieldName);
        this.afterCheckField(curRow, tableIndex, fieldName);
    },
    doF4: function (tableIndex, fieldName, progId, relPk, curValue, curRow, curGrid) {
        var entry = progId == this.progId ? this.entryParam : undefined;
        var isGrid = false;
        Ext.Ajax.request({
            url: 'billSvc/getBillType',
            method: 'POST',
            jsonData: {
                handle: UserHandle, progId: progId
            },
            async: false,
            timeout: 90000000,
            success: function (response) {
                var ret = Ext.decode(response.responseText);
                isGrid = ret.GetBillTypeResult == 2;
            }
        });
        if (curValue) {
            var curPks = this.getRelPk(fieldName, relPk, tableIndex, curRow, curValue, curGrid);
            if (isGrid)
                Ax.utils.BillManager.openGridByF4(progId, curPks);
            else
                Ax.utils.BillManager.openBillByF4(progId, BillActionEnum.Browse, curPks, entry);
        } else {
            if (isGrid)
                Ax.utils.BillManager.openGridByF4(progId, undefined);
            else
                Ax.utils.BillManager.openBillByF4(progId, BillActionEnum.AddNew, curPks, entry);
        }
    },
    formCallBackHandler: function (tag, param) {

    },
    saveDisplayScheme: function () {
        var needSave = false;
        var displayScheme = { ProgId: this.progId, GridScheme: {} };
        for (var i = 0; i < this.dataSet.dataList.length; i++) {
            if (this.dataSet.dataList[i].ownGrid) {
                var gridPanel = this.dataSet.dataList[i].ownGrid;
            }
            if (vcl.subGridScheme[i] || gridPanel) {
                if (!needSave)
                    needSave = true;
                var gridScheme = { GridFields: [] };

                var columns = gridPanel.headerCt.items.items;
                if (vcl.subGridScheme[i] || columns.length == 0) {
                    gridScheme = this.subGridScheme[i];
                } else {
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
                }
                if (gridScheme != undefined)
                    displayScheme.GridScheme[i] = gridScheme;

            }
        }
        if (needSave) {
            var call = function (displayScheme) {
                Ext.Ajax.request({
                    url: 'billSvc/saveDisplayScheme',
                    jsonData: { handle: UserHandle, progId: this.progId, entryParam: Ext.encode(this.entryParam), displayScheme: Ext.encode(displayScheme) },
                    method: 'POST',
                    async: false,
                    timeout: 90000000,
                    success: function (response) {
                        alert('方案保存成功。');
                    }
                });
            }
            call.apply(this, [displayScheme]);
        }
    },
    clearDisplayScheme: function () {
        function call() {
            Ext.Ajax.request({
                url: 'billSvc/clearDisplayScheme',
                jsonData: { handle: UserHandle, progId: this.progId, entryParam: Ext.encode(this.entryParam) },
                method: 'POST',
                async: false,
                timeout: 90000000,
                success: function (response) {
                    alert('方案删除成功。');
                }
            });
        }
        call.apply(this);
    },
    createChangeView: function (view, name, funcName, display) {
        var changeView;
        if (this.funcView.getCount() > 1) {
            var vcl = this;
            var menu = [];
            this.funcView.eachKey(function (key, item, idx, len) {
                if (key !== name) {
                    menu.push({
                        text: item.display,
                        handler: function () {
                            vcl.win.removeAll();
                            if (item.name == 'create')
                                vcl.win.add(view.createDefaultView(true));
                            else
                                vcl.win.add(view[item.name]());
                        }
                    });
                }
            });
            changeView = Ext.create(Ext.Action, {
                text: display,
                handler: function () {
                    vcl.win.removeAll();
                    if (funcName == 'create')
                        vcl.win.add(view.createDefaultView(true));
                    else
                        vcl.win.add(view[funcName]());
                },
                menu: menu
            });
        }
        return changeView;
    },
    print: function (progId, billNo) {
        //获取打印jpg路径
        debugger;
        var billNo = this.currentPk[0];
        var filePath = this.invorkBcf("Print", [[billNo]]);
        var LODOP = getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
        LODOP.ADD_PRINT_IMAGE(-20, 0, "100%", "100%", "<img src='" + filePath + "' />");
        LODOP.PRINT();
        return;

        var printTplNo = '';
        var fieldName = '';
        var Tplparam = '';
        var printTplJs = '';
        var printTplRowId = 0, printTplSubRowId = 0, tableIndex = 0;
        var printTemplateRuleList = null, billValueDic = null;
        var getPrintTemplateIds = this.invorkBcf("GetPrintTemplateIds", [billNo]);
        if (getPrintTemplateIds.length > 0) {
            for (var i = 0; i < getPrintTemplateIds.length; i++) {
                printTplNo = getPrintTemplateIds[i]["PrintTplNo"];
                printTplRowId = getPrintTemplateIds[i]["PrintTplRowId"];
                printTplSubRowId = getPrintTemplateIds[i]["PrintTplSubRowId"];
                var printTemplateDic = this.invorkBcf("GetPrintTemplateJs", [billNo, printTplNo, printTplRowId, printTplSubRowId]);
                printTplJs = printTemplateDic["TemplateJs"];
                if (printTplJs.length > 0) {
                    printTemplateRuleList = printTemplateDic["LabelTemplateRuleList"];
                    billValueDic = printTemplateDic["BillValueDic"];
                    for (var j = 0; j < printTemplateRuleList.length; j++) {
                        if (printTemplateRuleList[j]["TableIndex"] == 0) {
                            fieldName = printTemplateRuleList[j]["FieldName"];
                            Tplparam = printTemplateRuleList[j]["Tplparam"];
                            var reg = new RegExp(Tplparam, "g");
                            printTplJs = printTplJs.replace(reg, billValueDic[fieldName]);
                        }
                        else {
                        }
                    }
                    //this.lodop_print(printTplJs);
                    var LODOP = getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
                    eval(printTplJs);
                    LODOP.PRINT();
                    //LODOP.PRINT_DESIGN();
                }
            }
        }
    }
};

