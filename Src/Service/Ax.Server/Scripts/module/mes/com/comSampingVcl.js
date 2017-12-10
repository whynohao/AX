comSampingVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comSampingVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comSampingVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var masterTable = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1 || e.dataInfo.tableIndex == 2) {
                if (e.dataInfo.fieldName == "OPERATORLIMIT") {
                    var operatorUp = e.dataInfo.dataRow.get('OPERATORUP');
                    if (e.dataInfo.value == 1 && (operatorUp == 1 || operatorUp == 2 || operatorUp == 3)) {
                        Ext.Msg.alert("提示", "当【上限运算符】不为“无”时，【下限运算符】不得为“等于”！");
                        e.dataInfo.cancel = true;
                    } else if (e.dataInfo.value == 2 && operatorUp == 3) {
                        Ext.Msg.alert("提示", "当【上限运算符】为“等于”时，【下限运算符】不得为“大于”！");
                        e.dataInfo.cancel = true;
                    } else if (e.dataInfo.value == 3 && operatorUp == 3) {
                        Ext.Msg.alert("提示", "当【上限运算符】为“等于”时，【下限运算符】不得为“大于等于”！");
                        e.dataInfo.cancel = true;
                    }
                } else if (e.dataInfo.fieldName == "OPERATORUP") {
                    var operatorLimit = e.dataInfo.dataRow.get('OPERATORLIMIT');
                    if (e.dataInfo.value == 1 && operatorLimit == 1) {
                        Ext.Msg.alert("提示", "当【下限运算符】为“等于”时，【上限运算符】不得为“小于”！");
                        e.dataInfo.cancel = true;
                    } else if (e.dataInfo.value == 2 && operatorLimit == 1) {
                        Ext.Msg.alert("提示", "当【下限运算符】为“等于”时，【上限运算符】不得为“小于等于”！");
                        e.dataInfo.cancel = true;
                    } else if (e.dataInfo.value == 3 && (operatorLimit == 1 || operatorLimit == 2 || operatorLimit == 3)) {
                        Ext.Msg.alert("提示", "当【下限运算符】不为“无”时，【上限运算符】不得为“等于”！");
                        e.dataInfo.cancel = true;
                    }
                } else if (e.dataInfo.fieldName == "LIMITNUM") {
                    var upNum = e.dataInfo.dataRow.get('UPNUM');
                    var operatorLimit = e.dataInfo.dataRow.get('OPERATORLIMIT');
                    var operatorUp = e.dataInfo.dataRow.get('OPERATORUP');
                    if (e.dataInfo.value > upNum && operatorUp != 0) {
                        Ext.Msg.alert("提示", "当【上限运算符】不为“无”时，【下限数量】不得大于【上限数量】！");
                        e.dataInfo.cancel = true;
                    }
                    if (operatorLimit != 0) {
                        if (e.dataInfo.value < 0) {
                            Ext.Msg.alert("提示", "当【下限运算符】不为“无”时，【下限数量】大于等于0！");
                            e.dataInfo.cancel = true;
                        }
                    } else if (e.dataInfo.value != 0) {
                        Ext.Msg.alert("提示", "当【下限运算符】为“无”时，【下限数量】只能为0！");
                        e.dataInfo.cancel = true;
                    }
                } else if (e.dataInfo.fieldName == "UPNUM") {
                    var limitNum = e.dataInfo.dataRow.get('LIMITNUM');
                    var operatorUp = e.dataInfo.dataRow.get('OPERATORUP');
                    if (operatorUp != 0) {
                        if (e.dataInfo.value < limitNum || e.dataInfo.value <= 0) {
                            Ext.Msg.alert("提示", "当【上限运算符】不为“无”时，【上限数量】不得小于【下限数量】且大于0！");
                            e.dataInfo.cancel = true;
                        }
                    } else if (e.dataInfo.value != 0) {
                        Ext.Msg.alert("提示", "当【上限运算符】为“无”时，【上限数量】只能为0！");
                        e.dataInfo.cancel = true;
                    }
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1 || e.dataInfo.tableIndex == 2) {
                if (e.dataInfo.fieldName == "OPERATORLIMIT") {
                    if (e.dataInfo.value == 0) {
                        e.dataInfo.dataRow.set("LIMITNUM", 0);
                    } else if (e.dataInfo.value == 1) {
                        e.dataInfo.dataRow.set("OPERATORUP", 0);
                        e.dataInfo.dataRow.set("UPNUM", 0);
                    }
                } else if (e.dataInfo.fieldName == "OPERATORUP") {
                    if (e.dataInfo.value == 0) {
                        e.dataInfo.dataRow.set("UPNUM", 0);
                    } else if (e.dataInfo.value == 3) {
                        e.dataInfo.dataRow.set("OPERATORLIMIT", 0);
                        e.dataInfo.dataRow.set("LIMITNUM", 0);
                    }
                }
            }
            break;
    }
}