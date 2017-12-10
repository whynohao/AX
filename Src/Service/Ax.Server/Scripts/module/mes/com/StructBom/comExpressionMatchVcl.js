comExpressionMatchVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
    this.SYMBOL = new Array();
    this.SYMBOL[0] = "";
    this.SYMBOL[1] = ">";
    this.SYMBOL[2] = "<";
    this.SYMBOL[3] = "=";
    this.SYMBOL[4] = ">=";
    this.SYMBOL[5] = "<=";
    this.curRow;
}
var proto = comExpressionMatchVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comExpressionMatchVcl;
proto.winId = null;
proto.fromObj = null;
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj[0];
    var JsonString = proto.fromObj.dataSet.getTable(0).data.items[0].data["JSON"];
    if (JsonString != "") {
        var Json = eval("(" + JsonString + ")");
        this.fillData.call(this, Json);
    }
    else {
        var masterRow = this.dataSet.getTable(0).data.items[0];
        this.addRow(masterRow, 1);
    }
    this.win.width = document.body.clientWidth * 0.5;
    this.win.height = document.body.clientHeight * 0.5;
    this.win.modal = true;
};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.FormClosed:
            if (this.dataSet.getChildren(1, this.curRow, 2).length > 0) {
                this.curRow.set("CONTITIONDETAIL", 1);
            }
            else {
                this.curRow.set("CONTITIONDETAIL", 0);
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == 'btnSure') {
                var result = {};
                var formula = "";
                var explain = "";
                var matchType = this.dataSet.getTable(0).data.items[0].get("MATCHTYPE");
                result.result = new Array();
                result.matchType = matchType;
                if (matchType == 0) {

                    var item = {};
                    item.value = this.dataSet.getTable(1).data.items[0].get("VALUE");
                    item.conditionDetail = [];
                    result.result.push(item);
                    //if (this.dataSet.getTable(1).data.items[0].data["VALUE"] != "") {
                    //    formula = "ret=" + this.dataSet.getTable(1).data.items[0].data["VALUE"] + ";";
                    //    explain = "返回" + this.dataSet.getTable(1).data.items[0].data["VALUE"];
                    //}
                    if (item.value) {
                        formula = "ret=" + this.dataSet.getTable(1).data.items[0].data["VALUE"] + ";";
                        explain = "返回" + this.dataSet.getTable(1).data.items[0].data["VALUE"];
                    }
                }
                else {
                    var items = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < items.length; i++) {
                        var dataRow = items[i];
                        var item = {};
                        result.result.push(item);
                        item.value = dataRow.get("VALUE");
                        item.conditionDetail = new Array();
                        if (i >= 1) {
                            formula = formula+" else ";
                        }
                        var subRows = this.dataSet.getChildren(1, dataRow, 2);
                        for (var j = 0; j < subRows.length; j++) {
                            var subRow = subRows[j];
                            item.conditionDetail.push({ param: subRow.get("PARAM"), symbol1: subRow.get("SYMBOL1"), condition1: subRow.get("CONDITION1"), symbol2: subRow.get("SYMBOL2"), condition2: subRow.get("CONDITION2") });
                            if (j == 0) {
                                if (subRow.data["SYMBOL1"] != 0) {
                                    formula = formula + "if(" + subRow.get('PARAM') + this.SYMBOL[subRow.data["SYMBOL1"]] + subRow.data["CONDITION1"];
                                    explain = explain + "当" + subRow.get("PARAM") + this.SYMBOL[subRow.data["SYMBOL1"]] + subRow.data["CONDITION1"];
                                    if (subRow.data["SYMBOL2"] != 0) {
                                        formula = formula + " && " + subRow.get('PARAM') + this.SYMBOL[subRow.data["SYMBOL2"]] + subRow.data["CONDITION2"];
                                        explain = explain + "并且" + subRow.get('PARAM') + this.SYMBOL[subRow.data["SYMBOL2"]] + subRow.data["CONDITION2"];
                                    }
                                }
                            }
                            else {
                                if (subRow.data["SYMBOL1"] != 0) {
                                    formula = formula + " && " + subRow.get('PARAM') + this.SYMBOL[subRow.data["SYMBOL1"]] + subRow.data["CONDITION1"];
                                    explain = explain + " 并且 " + subRow.get("PARAM") + this.SYMBOL[subRow.data["SYMBOL1"]] + subRow.data["CONDITION1"];
                                    if (subRow.data["SYMBOL2"] != 0) {
                                        formula = formula + " && " + subRow.get('PARAM') + this.SYMBOL[subRow.data["SYMBOL2"]] + subRow.data["CONDITION2"];
                                        explain = explain + "并且" + subRow.get('PARAM') + this.SYMBOL[subRow.data["SYMBOL2"]] + subRow.data["CONDITION2"];
                                    }
                                }
                            }
                        }
                        if (formula)
                            formula = formula + ") ret = " + dataRow.get("VALUE") + ";";
                        if (explain)
                            explain = explain + "时 ,返回" + dataRow.data["VALUE"] + ";\n";

                    }
                    if (formula != "") {
                        formula = formula + " else ret = -1;";
                    }
                }
                if (formula != "") {
                    proto.fromObj.dataSet.getTable(0).data.items[0].set("EXPRESSIONDESC", formula);
                    proto.fromObj.dataSet.getTable(0).data.items[0].set("EXPRESSIONEXPLAIN", explain);
                    var params = [];
                    if (matchType == 1) {
                        var items = this.dataSet.getTable(2).data.items;
                        for (var i = 0; i < items.length; i++) {
                            var param = items[i].get("PARAM");
                            if (params.length > 0) {
                                var hasParam = false;
                                for (var j = 0; j < params.length; j++) {
                                    if (param == params[j]) {
                                        hasParam = true;
                                    }
                                }
                                if (!hasParam) {
                                    params.push(param);
                                }
                            }
                            else {
                                params.push(param);
                            }
                        }
                    }
                    else {
                        var value =  this.dataSet.getTable(1).data.items[0].get("VALUE");
                        var s = value, a = 0, params = [], len = s.length, reg = /\@/;
                        for (var a = 0; reg.test(s.substr(a, len - a)) ; a++) {
                            a += s.substr(a, len - a).indexOf('@');
                            var l = len - a;//剩余待解析的字符串长度
                            //因方法中只有加或减，判断+或-即可
                            var plus = s.substr(a, l).indexOf('+');
                            var minus = s.substr(a, l).indexOf('-');
                            var divide = s.substr(a, l).indexOf('/');
                            var multiply = s.substr(a, l).indexOf('*');
                            var array = [];
                            (plus > 0 && array.push(plus));
                            (minus > 0 && array.push(minus));
                            (divide > 0 && array.push(divide));
                            (multiply > 0 && array.push(multiply));
                            
                            
                            var sign = (array.length>0 && Math.min.apply(null, array) || l);
                            //var sign = (b1 && b2 && (minus < plus ? minus : plus)) || b1 && plus || b2 && minus || l;
                            var paramId = s.substr(a, sign);
                            if (paramId.lastIndexOf('@') != paramId.indexOf('@')) {
                                e.dataInfo.cancel = true;
                                this.forms[0].loadRecord(e.dataInfo.dataRow);
                                params.length = 0;
                                Ext.Msg.alert("警告", "存在变量之间无符号");
                                break;
                            }
                            else if (params.indexOf(paramId) < 0) {
                                params.push(paramId);
                            }
                        }
                    }
                    this.fillReturnData.call(this, result, params);
                    this.win.close();
                }
                else {
                    if (matchType == 0)
                        Ext.Msg.alert("提示", "缺失返回值");
                    else
                        Ext.Msg.alert("提示", "缺失返回值或者缺失条件明细");
                }
                break;
            }
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.fieldName == 'VALUE') {
                if (!toolWindow) {
                    var toolWindow = Ext.create("ToolWindow", { dataRow: e.dataInfo.dataRow, me: this, form: this.forms[0] });
                }
                toolWindow.show();
                //var expressionDessc = this.dataSet.getTable(0).data.items[0].get("EXPRESSIONDESC");
                //if (expressionDessc.length > 0) {
                //    expressionDessc = expressionDessc.substr(4, expressionDessc.length - 5);
                //}
                Ext.getCmp("tool").setValue(e.dataInfo.value);
            }
        else if (e.dataInfo.fieldName == "CONTITIONDETAIL") {
            this.curRow = e.dataInfo.dataRow;
        }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0 && e.dataInfo.fieldName == 'MATCHTYPE') {
                if (e.dataInfo.value == 0) {
                    this.dataSet.getTable(1).removeAll();
                    var masterRow = this.dataSet.getTable(0).data.items[0];
                    this.addRow(masterRow, 1);
                }
            }
            this.forms[0].updateRecord(masterRow);
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (this.dataSet.getTable(0).data.items[0].data["MATCHTYPE"] == 0) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (this.dataSet.getTable(0).data.items[0].data["MATCHTYPE"] == 0) {
                e.dataInfo.cancel = true;
            }
            break;

    }
}

proto.fillData = function (Json) {
    var masterRow = this.dataSet.getTable(0).data.items[0];
    var result = Json;
    var type = result.matchType;
    Ext.getCmp('MATCHTYPE0_' + this.winId).setValue(type);
    masterRow.set("MATCHTYPE", type);

    if (result !== undefined && result.result.length > 0) {
        for (var i = 0; i < result.result.length; i++) {
            var info = result.result[i];
            var newRow = this.addRow(masterRow, 1);
            //newRow.set('SYMBOL1', info.SYMBOL1);
            //newRow.set('CONDITION1', info.CONDITION1);
            //newRow.set('SYMBOL2', info.SYMBOL2);
            //newRow.set('CONDITION2', info.CONDITION2);
            newRow.set('VALUE', info.value);
            if (info.conditionDetail.length > 0) {
                newRow.set("CONTITIONDETAIL", 1);
            }
            for (var j = 0; j < info.conditionDetail.length; j++) {
                var conditionDetailInfo = info.conditionDetail[j];
                var subRow = this.addRow(newRow, 2);
                subRow.set("PARAM", conditionDetailInfo.param);
                subRow.set('SYMBOL1', conditionDetailInfo.symbol1);
                subRow.set('CONDITION1', conditionDetailInfo.condition1);
                subRow.set('SYMBOL2', conditionDetailInfo.symbol2);
                subRow.set('CONDITION2', conditionDetailInfo.condition2);
            }
        }
    }
}
proto.fillReturnData = function (returnData, params) {
    Ext.suspendLayouts();
    try {
        var list = returnData;
        var info = JSON.stringify(list)
        var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
        masterRow.set("JSON", info);
        if (proto.fromObj.dataSet.getTable(1).data.items.length == 0) {
            for (var i = 0; i < params.length; i++) {
                var newRow = proto.fromObj.addRow(masterRow, 1);
                newRow.set('PARAMID', params[i]);
            }
        }
        else {
            for (var i = 0; i < params.length; i++) {
                var notHaveParam = true;
                for (var j = 0; j < proto.fromObj.dataSet.getTable(1).data.items.length; j++) {
                    if (params[i] == proto.fromObj.dataSet.getTable(1).data.items[j].get("PARAMID")) {
                        notHaveParam = false;
                    }
                }
                if (notHaveParam) {
                    var newRow = proto.fromObj.addRow(masterRow, 1);
                    newRow.set('PARAMID', params[i]);
                }
            }
        }
        proto.fromObj.forms[0].loadRecord(masterRow);
    } finally {
        Ext.resumeLayouts(true);
    }
}


Ext.define('ToolWindow', {
    extend: 'Ext.window.Window',
    height: 400,
    width: 280,
    padding: '10 10 0 20',
    collapsible: true,
    title: '工具',
    frame: true,
    modal: true,
    collapsible: false,
    initComponent: function () {
        this.headRow,
        this.me,
        this.form,
        this.arr = [],
        Ext.apply(this, {
            items: [{
                xtype: 'panel', anchor: '100%', frame: false, height: "100%", collapsible: false, header: false,
                items: [
                 {
                     xtype: 'panel', height: 50, margin: '0 20 0 10', baseCls: 'my-panel-no-border', width: 280,
                     defaults: { width: 210, height: 30 }, title: '',
                     items: [
                        { xtype: 'textfield', id: 'tool' }
                     ]
                 },
                 {
                     xtype: 'panel', height: 40, margin: '0 20 0 10', baseCls: 'my-panel-no-border', width: 280, defaults: { width: 70, height: 30 }, title: '',
                     items: [
                         {
                             xtype: 'button', text: '变量', listeners: { click: { fn: this.varstr, scope: this } }
                         },
                        {
                            xtype: 'button', text: '加', listeners: { click: { fn: this.onButtonClick, scope: this } }
                        },
                         {
                             xtype: 'button', text: '减', listeners: { click: { fn: this.onButtonClick, scope: this } }
                         }]
                 },
                 {
                     xtype: 'panel', height: 40, margin: '0 20 0 10', baseCls: 'my-panel-no-border', width: 280, defaults: { width: 70, height: 30 }, title: '',
                     items: [
                         {
                             xtype: 'button', text: '(', listeners: { click: { fn: this.onButtonClick, scope: this } }
                         },
                        {
                            xtype: 'button', text: ')', listeners: { click: { fn: this.onButtonClick, scope: this } }
                        },
                         {
                             xtype: 'button', text: '除', listeners: { click: { fn: this.onButtonClick, scope: this } }
                         }]
                 },
                {
                    xtype: 'panel', height: 40, margin: '0 20 0 10', baseCls: 'my-panel-no-border', width: 280, defaults: { width: 70, height: 30 }, title: '',
                    items: [
                    {
                        xtype: 'button', text: '0', listeners: { click: { fn: this.onButtonClick, scope: this } }
                    },
                    {
                        xtype: 'button', text: '.', listeners: { click: { fn: this.onButtonClick, scope: this } }
                    },
                     {
                         xtype: 'button', text: '乘', listeners: { click: { fn: this.onButtonClick, scope: this } }
                     }
                    ]
                },
                {
                    xtype: 'panel', height: 40, margin: '0 20 0 10', baseCls: 'my-panel-no-border', width: 280, defaults: { width: 70, height: 30 }, title: '',
                    items: [
                    {
                        xtype: 'button', text: '1', listeners: { click: { fn: this.onButtonClick, scope: this } }
                    },
                    {
                        xtype: 'button', text: '2', listeners: { click: { fn: this.onButtonClick, scope: this } }
                    },
                     {
                         xtype: 'button', text: '3', listeners: { click: { fn: this.onButtonClick, scope: this } }
                     }
                    ]
                },
                {
                    xtype: 'panel', height: 40, margin: '0 20 0 10', baseCls: 'my-panel-no-border', width: 280, defaults: { width: 70, height: 30 }, title: '',
                    items: [
                    {
                        xtype: 'button', text: '4', listeners: { click: { fn: this.onButtonClick, scope: this } }
                    },
                    {
                        xtype: 'button', text: '5', listeners: { click: { fn: this.onButtonClick, scope: this } }
                    },
                     {
                         xtype: 'button', text: '6', listeners: { click: { fn: this.onButtonClick, scope: this } }
                     }
                    ]
                },
                {
                    xtype: 'panel', height: 40, margin: '0 20 0 10', baseCls: 'my-panel-no-border', width: 280, defaults: { width: 70, height: 30 }, title: '',
                    items: [
                    {
                        xtype: 'button', text: '7', listeners: { click: { fn: this.onButtonClick, scope: this } }
                    },
                    {
                        xtype: 'button', text: '8', listeners: { click: { fn: this.onButtonClick, scope: this } }
                    },
                     {
                         xtype: 'button', text: '9', listeners: { click: { fn: this.onButtonClick, scope: this } }
                     }
                    ]
                },
                {
                    xtype: 'panel', height: 40, margin: '0 20 0 10', baseCls: 'my-panel-no-border', width: 280, defaults: { width: 70, height: 30 }, title: '',
                    items: [
                    {
                        xtype: 'button', text: '撤销', listeners: { click: { fn: this.delstr, scope: this } }
                    },
                    {
                        xtype: 'button', text: '清空', listeners: { click: { fn: this.clearstr, scope: this } }
                    },
                    {
                        xtype: 'button', text: '确定', listeners: { click: { fn: this.affirm, scope: this } }
                    },

                    ]
                },
                ]
            }
            ]
        });
        this.callParent(arguments);
    },
    delstr: function () {//撤销
        var s = Ext.getCmp('tool').getValue();
        s = s.substr(0, s.length - 1);
        Ext.getCmp('tool').setValue(s);
    },
    clearstr: function () {//清空
        Ext.getCmp('tool').setValue();
    },
    onButtonClick: function (btn) {//+,-,1,2,3,4,5,6,7,8,9,0
        var s = Ext.getCmp('tool').getValue();
        var c = { "加": '+', "减": '-', "除": '/' ,"乘":'*'}[btn.text] || btn.text;
        s += c;
        Ext.getCmp('tool').setValue(s);
    },
    varstr: function () {
        var s = Ext.getCmp('tool').getValue();
        s += "@A";
        Ext.getCmp('tool').setValue(s);
    },
    affirm: function () {//确认
        var s = Ext.getCmp('tool').getValue(), a = 0, arr = [], len = s.length, reg = /\@/;
        this.dataRow.set("VALUE", s);
        //for (var a = 0; reg.test(s.substr(a, len - a)) ; a++) {
        //    a += s.substr(a, len - a).indexOf('@');
        //    var l = len - a;//剩余待解析的字符串长度
        //    //因方法中只有加或减，判断+或-即可
        //    var plusSign = s.substr(a, l).indexOf('+');
        //    var minus = s.substr(a, l).indexOf('-');
        //    var b1 = plusSign > 0, b2 = minus > 0;
        //    var sign = (b1 && b2 && (minus < plusSign ? minus : plusSign)) || b1 && plusSign || b2 && minus || l;
        //    var paramId = s.substr(a, sign);
        //    if (paramId.lastIndexOf('@') != paramId.indexOf('@')) {
        //        s = this.headRow.data["EXPRESSIONDESC"];
        //        arr.length = 0;
        //        Ext.Msg.alert("警告", "存在变量之间无符号");
        //        break;
        //    }
        //    else if (arr.indexOf(paramId) < 0) {
        //        arr.push(paramId);
        //    }
        //}
        //this.headRow.set("EXPRESSIONDESC", s == "" ? s : "ret=" + s + ";");
        //if (arr.length) {
        //    try {
        //        var store = this.me.dataSet.getTable(1);
        //        Ext.suspendLayouts();
        //        store.suspendEvents();
        //        store.removeAll();
        //        var grid = Ext.getCmp(this.me.winId + 'COMEXPRESSIONDETAILGrid');
        //        for (var i = 0; i < arr.length; i++) {
        //            var newRow = this.me.addRowForGrid(grid);
        //            newRow.set('ROW_ID', i + 1);
        //            newRow.set('ROWNO', i + 1);
        //            newRow.set('PARAMID', arr[i]);
        //        }
        //    } finally {
        //        store.resumeEvents();
        //        if (store.ownGrid && store.ownGrid.getView().store != null)
        //            store.ownGrid.reconfigure(store);
        //        Ext.resumeLayouts(true);
        //    }
        //}
        //this.form.loadRecord(this.headRow);
        this.close();
    }
});

