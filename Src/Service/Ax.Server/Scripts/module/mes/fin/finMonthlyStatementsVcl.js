/// <reference path="../../../ax/vcl/comm/LibVclData.js" />

finMonthlyStatementsVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = finMonthlyStatementsVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = finMonthlyStatementsVcl;
var returnList;
//调用datafuc的时候会调用此方法，可以初始化一些参数。

proto.doSetParam = function (vclObj) {
    var myDate = new Date();
    var year = myDate.getFullYear();
    Ext.getCmp('ACCOUNTYEAR0_' + this.winId).setValue(year);
    returnList = this.invorkBcf('GetData', [year]);
    this.FillDataFunc.call(this, returnList);
}


//dataFunc表身填充
proto.FillDataFunc = function (returnList) {
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
                this.FillDataFunc.call(this, returnList);
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            var grid = Ext.getCmp(this.winId + 'FINMONTHLYSTATEMENTSDETAILGrid');
            var records = grid.getView().getSelectionModel().getSelection();
            if (records.length <= 0) {
                Ext.Msg.alert("系统提示", "请选择数据！");
            }
            else if (records.length > 1) {
                Ext.Msg.alert("系统提示", "只能选择一条数据，进行月结");
            }
            else {
                var date = new Date();
                //if (records[0].data["ENDDATE"] < date.getDate()) {
                    var info =
                    {
                        AccountYear: records[0].data["ACCOUNTYEAR"],
                        Accountmonth: records[0].data["ACCOUNTMONTH"],
                        StartYear: records[0].data["STARTDATE"],
                        EndYear: records[0].data["ENDDATE"],
                        IsEnd: records[0].data["ISEND"] + 0
                    }
                //}
                //else
                //    Ext.Msg.alert("系统提示", "当前日期必须大于会计月日期！");
            }

            if (e.dataInfo.fieldName == "SettlemenMonth") {
                if (info.IsEnd == 0) {
                    this.invorkBcf('MonthlyEnd', [info, 0]);
                    var accountYear = this.dataSet.getTable(0).data.items[0].data['ACCOUNTYEAR'] == undefined
                        ? 0
                        : this.dataSet.getTable(0).data.items[0].data['ACCOUNTYEAR'];
                    returnList = this.invorkBcf('GetData', [accountYear]);
                    this.FillDataFunc.call(this, returnList);
                } else {
                    Ext.Msg.alert("系统提示", "当前月已经月结");
                }
            }
            if (e.dataInfo.fieldName == "InverseMonth") {
                if (info.IsEnd == 0) {
                    Ext.Msg.alert("系统提示", "当前月尚未月结，无法反月结");
                } else {
                    this.invorkBcf('MonthlyEnd', [info, 1]);
                    var accountYear = this.dataSet.getTable(0).data.items[0].data['ACCOUNTYEAR'] == undefined
                        ? 0
                        : this.dataSet.getTable(0).data.items[0].data['ACCOUNTYEAR'];
                    returnList = this.invorkBcf('GetData', [accountYear]);
                    this.FillDataFunc.call(this, returnList);
                }
            }
            break;
    }
};
