plsLogisticsPlanSetVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
}

var proto = plsLogisticsPlanSetVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsLogisticsPlanSetVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "BtnLoad":
                    //调用后台获取数据方法
                    var sendDate = this.dataSet.getTable(0).data.items[0].data['SENDDATE'];
                    var lotNo = this.dataSet.getTable(0).data.items[0].data['LOTNO'];;
                    var bodyData = this.invorkBcf("GetLoginsticsPlan", [sendDate, lotNo]);

                    //填充数据到明细表
                    FillData.call(this, bodyData);
                    break;
                case "BtnHander":
                    var bodyTable = this.dataSet.getTable(1).data.items;
                    if (bodyTable.length == 0) {
                        Ext.Msg.alert("提示", "暂无可处理项！");
                    }
                    else {
                        var list = new Array();
                        for (var i = 0; i < bodyTable.length; i++) {
                            list.push({
                                'SENDDATE': bodyTable[i].data['SENDDATE'],
                                'REALSENDDATE': bodyTable[i].data['REALSENDDATE'],
                                'LOTNO': bodyTable[i].data['LOTNO'],
                                'ROW_ID': bodyTable[i].data['ROW_ID'],
                                'SALEBILLNO': bodyTable[i].data['SALEBILLNO'],
                            });
                        }
                        //调用后台方法
                        var returnResult = this.invorkBcf("HanderLotingsticsPlan", [list]);
                        if (returnResult == true) {
                            Ext.Msg.alert("提示", "维护成功");
                            this.dataSet.getTable(1).removeAll();
                        }
                    }
                    break;
            }
    }

    function FillData(returnData) {
        Ext.suspendLayouts();//关闭Ext布局
        var curStore = this.dataSet.getTable(1);
        curStore.suspendEvents();//关闭store事件
        try {
            var myDate = new Date();
            var nowDate = getDate(myDate.toLocaleDateString().split('/'));
            this.dataSet.getTable(1).removeAll();
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var list = returnData;
            if (list != undefined && list.length > 0) {
                for (var i = 0; i < list.length; i++) {
                    var info = list[i];
                    var newRow = this.addRow(masterRow, 1);
                    newRow.set("ROW_ID", i + 1);
                    newRow.set("ROWNO", i + 1);
                    newRow.set("SENDDATE", info.SendDate);
                    newRow.set("LOTNO", info.LotNo);
                    newRow.set("SALEBILLNO", info.SaleBillNo);
                    if (info.RealSendDate <= 0) {
                        newRow.set("REALSENDDATE", nowDate);//默认处理当天为实际发货日期
                    }
                    else {
                        newRow.set("REALSENDDATE", info.RealSendDate);//有实际发货日期，则将日期带到表中
                    }

                }
            }
        } finally {
            curStore.resumeEvents();//打开store事件
            if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
                curStore.ownGrid.reconfigure(curStore);
            Ext.resumeLayouts(true);//打开Ext布局
        }
    }
}

//日期转YYYYMMDD
function getDate(date) {
    var year = date[0];
    var month = date[1];
    if (month.length == 1) {
        month = '0' + month;
    }
    var day = date[2];
    if (day.length == 1) {
        day = '0' + day;
    }
    return year + month + day;
}