/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

finIAMonthlyStatementsVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = finIAMonthlyStatementsVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = finIAMonthlyStatementsVcl;
var returnList;
//调用datafuc的时候会调用此方法，可以初始化一些参数。



proto.doSetParam = function (vclObj) {
    var myDate = new Date();
    var year = myDate.getFullYear();
    Ext.getCmp('ACCOUNTYEAR0_' + this.winId).setValue(year);
    returnList = this.invorkBcf('GetData', [year]);
    FillDataFunc.call(this, returnList);
}
//dataFunc表身填充
function FillDataFunc(returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var list = returnList;
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set('ACCOUNTYEAR', info.AccountYear);
                newRow.set('ACCOUNTMONTH', info.Accountmonth);
                newRow.set('STARTDATE', info.StartYear);
                newRow.set('ENDDATE', info.EndYear);
                newRow.set('ISEND', info.IsEnd);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated://form的赋值之后在this.dataSet中还没有实时的数据写入，需要写入之后，才能取到值。          
            if (e.dataInfo.fieldName == "ACCOUNTYEAR") {
                //e.dataInfo.updateRecord(e.dataInfo.dataRow);
                var accountYear = this.dataSet.getTable(0).data.items[0].data['ACCOUNTYEAR'] == undefined ? 0 : this.dataSet.getTable(0).data.items[0].data['ACCOUNTYEAR'];
                returnList = this.invorkBcf('GetData', [accountYear]);
                FillDataFunc.call(this, returnList);
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            //月结
            if (e.dataInfo.fieldName == "SettlemenMonth") {
                var grid = Ext.getCmp(this.winId + "FINMONTHLYSTATEMENTSDETAIL" + "Grid");
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length == 1) {
                    var record = records[0];
                    var obj = { AccountYear: record.get("ACCOUNTYEAR"), AccountMonth: record.get("ACCOUNTMONTH"), DateFrom: record.get("DATEFROM"), DateTo: record.get("DATETO") }
                    var retObj = this.invorkBcf('SettlemenMonth', [obj]);
                    if (retObj.IsOk) {
                        record.set("ISEND", 1);
                        Ext.Msg.alert("提示", "月结没有问题！");
                    }
                    else {
                        Ext.Msg.alert("提示", retObj.Reason);
                    }
                }
                else
                    Ext.Msg.alert("提示", "请选择一行进行月结");
            }
            //反月结
            if (e.dataInfo.fieldName == "InverseMonth") {
                var grid = Ext.getCmp(this.winId + "FINMONTHLYSTATEMENTSDETAIL" + "Grid");
                var records = grid.getView().getSelectionModel().getSelection();
                if (records.length == 1) {
                    var record = records[0];
                    if (record.get("ISEND") == 1) {
                        var obj = { AccountYear: record.get("ACCOUNTYEAR"), AccountMonth: record.get("ACCOUNTMONTH"), DateFrom: record.get("DATEFROM"), DateTo: record.get("DATETO") };
                        var retObj = this.invorkBcf('InverseMonth', [obj]);
                        if (retObj.IsOk) {
                            record.set("ISEND", 0);
                            Ext.Msg.alert("提示", "反月结没有问题！");
                        }
                        else {
                            Ext.Msg.alert("提示", retObj.Reason);
                        }
                    }
                    else
                        Ext.Msg.alert("提示", "选中行没有月结，不能进行反月结");
                }
                else
                    Ext.Msg.alert("提示", "请选择一行进行反月结");
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            //不允许手工添加行
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
    }
};
