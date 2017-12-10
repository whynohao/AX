finAccountPeriodFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = finAccountPeriodFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = finAccountPeriodFuncVcl;
var num = 0;

proto.doSetParam = function () {
    this.fillData.call(this, 0);
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "DATEFROM") {
                    var headRecord = this.dataSet.getTable(0).data.items[0];
                    var yearFrom = headRecord.data["ACCOUNTYEAR"];
                    var mark = true;
                    var recoders = this.dataSet.getTable(1).data.items;
                    var month = e.dataInfo.dataRow.data["ACCOUNTMONTH"];
                    for (var i = 0; i < recoders.length; i++) {
                        if (month == 1) {
                            if (this.invorkBcf('CheckFirstDate', [yearFrom, e.dataInfo.value]) != e.dataInfo.value) {
                                mark = false;
                            }
                            break;
                        }
                        else if (recoders[i].data["ACCOUNTMONTH"] < month && recoders[i].data["DATEFROM"] >= e.dataInfo.value) {
                            mark = false;
                            break;
                        }
                    }
                    if (!mark) {
                        Ext.Msg.alert("提示", "开始时间不能小于上一个会计月的开始时间！");
                        e.dataInfo.cancel = true;
                    }
                }
            }
            else if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "ACCOUNTYEAR") {
                    if (e.dataInfo.value <= 0)
                        e.dataInfo.cancel = true;
                }
                else if (e.dataInfo.fieldName == "ACCOUNTMONTH") {
                    if (num == 0) {
                        if (e.dataInfo.value <= 0 || e.dataInfo.value > 12)
                            e.dataInfo.cancel = true;
                    }
                    else {
                        if (e.dataInfo.value <= 0 || e.dataInfo.value > 13)
                            e.dataInfo.cancel = true;
                    }
                }
            }
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "ISNATURALMONTH") {
                    num = e.dataInfo.value;
                    this.fillData.call(this, num);
                }
                else if (e.dataInfo.fieldName == "ACCOUNTYEAR") {
                    this.fillData.call(this, num);
                }
            }
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "DATEFROM") {
                    var month = e.dataInfo.dataRow.data["ACCOUNTMONTH"];
                    var changRow = [];
                    var recoders = this.dataSet.getTable(1).data.items;

                    for (var i = 0; i < recoders.length; i++) {
                        if (recoders[i].data["ACCOUNTMONTH"] >= month) {
                            changRow.push({
                                AccountMonth: recoders[i].data["ACCOUNTMONTH"],
                                DateFrom: recoders[i].data["DATEFROM"]
                            });
                        }
                    }
                    if (changRow.length != 1) {
                        var returnList = this.invorkBcf('ChangeDateFrom', [changRow, num]);
                        for (var i = 0; i < recoders.length; i++) {
                            for (var j = 0; j < returnList.length; j++) {
                                if (recoders[i].data["ACCOUNTMONTH"] == returnList[j].AccountMonth)
                                    recoders[i].set("DATEFROM", returnList[j].DateFrom);
                            }
                        }
                    }

                }
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnSure") {
                var headRecord = this.dataSet.getTable(0).data.items[0];
                var yearFrom = headRecord.data["ACCOUNTYEAR"];
                var monthFrom = headRecord.data["ACCOUNTMONTH"];
                var recoders = this.dataSet.getTable(1).data.items;
                var changRow = [];
                for (var i = 0; i < recoders.length; i++) {
                    changRow.push({
                        AccountMonth: recoders[i].data["ACCOUNTMONTH"],
                        DateFrom: recoders[i].data["DATEFROM"]
                    });
                }
                var retun = this.invorkBcf('Sure', [changRow, yearFrom, monthFrom]);
                if (retun.IsOK)
                    Ext.Msg.alert("提示", "保存成功");
                else
                    Ext.Msg.alert("提示", retun.Message);
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
    }
}
proto.fillData = function (monthNum) {
    if (monthNum == 0)
        monthNum = 12;
    else
        monthNum = 13;
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'FINACCOUNTPERIODDETAILFUNCGrid');
        var headerRow = this.dataSet.getTable(0).data.items[0];
        var year = headerRow.data["ACCOUNTYEAR"];
        var records = this.invorkBcf('GetData', [year, monthNum])

        for (var i = 0; i < records.length; i++) {
            var newRow = this.addRowForGrid(grid);
            newRow.set("ACCOUNTMONTH", records[i].AccountMonth);
            newRow.set("DATEFROM", records[i].DateFrom);
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}