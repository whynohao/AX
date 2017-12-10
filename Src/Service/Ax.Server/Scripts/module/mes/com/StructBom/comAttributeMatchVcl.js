comAttributeMatchVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
    this.SYMBOL = new Array();
    this.SYMBOL[0] = "";
    this.SYMBOL[1] = ">";
    this.SYMBOL[2] = "<";
    this.SYMBOL[3] = "==";
    this.SYMBOL[4] = ">=";
    this.SYMBOL[5] = "<=";
}
var proto = comAttributeMatchVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comAttributeMatchVcl;
proto.winId = null;
proto.fromObj = null;
proto.attrItemTable = null;
proto.attrDetailRow = null;
proto.dic = new Array();
proto.doSetParam = function (vclObj, attrItemTable, attrDetailRow, dic) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj[0];
    proto.attrItemTable = vclObj[1];
    proto.attrDetailRow = vclObj[2];
    proto.dic = vclObj[3];
    //var JsonString = proto.fromObj.dataSet.getTable(0).data.items[0].data["JSON"];
    if (proto.dic != "") {
        var Json = eval(proto.dic);
        this.fillData.call(this, Json);
    }
    this.win.width = document.body.clientWidth * 0.5;
    this.win.height = document.body.clientHeight * 0.5;
    this.win.modal = true;
};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "btnSure") {
                var Conditions = new Array();
                var formula = "";
                for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                    var dataRow = this.dataSet.getTable(1).data.items[i];
                    Conditions.push({
                        ATTRIBUTEITEMID: proto.attrItemTable.ATTRIBUTEITEMID,
                        SYMBOL1: dataRow.data["SYMBOL1"],
                        CONDITION1: dataRow.data["CONDITION1"],
                        SYMBOL2: dataRow.data["SYMBOL2"],
                        CONDITION2: dataRow.data["CONDITION2"],
                        VALUE: dataRow.data["VALUE"],
                    });
                    if (dataRow.data["SYMBOL1"] != 0) {
                        formula = formula + "if(A" + this.SYMBOL[dataRow.data["SYMBOL1"]] + dataRow.data["CONDITION1"];
                        if (dataRow.data["SYMBOL2"] != 0) {
                            formula = formula + " && A" + this.SYMBOL[dataRow.data["SYMBOL2"]] + dataRow.data["CONDITION2"];
                        }
                        formula = formula + ") ret = " + dataRow.data["VALUE"] + " else ";
                    }
                }
                if (formula != "") {
                    formula = formula + " ret = -1| A = " + proto.attrItemTable.ATTRIBUTEITEMID + ";" + JSON.stringify(Conditions);
                    proto.attrDetailRow.set("MATCHRULE", true);
                    proto.attrDetailRow.set("MATCHTEXTBINARY", formula);
                    var foundRow = proto.fromObj.dataSet.FindRow(1, proto.attrDetailRow.get("ROW_ID"));
                    if (foundRow) {
                        foundRow.set("MATCHRULE", true);
                        foundRow.set("MATCHTEXTBINARY", formula);
                    }
                    this.win.close();
                }
                else {
                    alert("条件不能都为空，起码要保证有一个条件");
                }
            }
            break;

    }
}

proto.fillData = function (Json) {
    var masterRow = this.dataSet.getTable(0).data.items[0];
    var list = Json;
    if (list !== undefined && list.length > 0) {
        for (var i = 0; i < list.length; i++) {
            var info = list[i];
            var newRow = this.addRow(masterRow, 1);
            newRow.set('SYMBOL1', info.SYMBOL1);
            newRow.set('CONDITION1', info.CONDITION1);
            newRow.set('SYMBOL2', info.SYMBOL2);
            newRow.set('CONDITION2', info.CONDITION2);
            newRow.set('VALUE', info.VALUE);
        }
    }
}
