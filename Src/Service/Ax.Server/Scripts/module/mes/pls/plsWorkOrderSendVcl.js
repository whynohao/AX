plsWorkOrderSendVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
}
var proto = plsWorkOrderSendVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsWorkOrderSendVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo.fieldName == 'DATE' || e.dataInfo.fieldName == "PRODUCELINEID") {
            e.dataInfo.dataRow.set('DATE', e.dataInfo.value);
        }
    }
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == 'btnLoad') {//载入作业单
            var date = this.dataSet.getTable(0).data.items[0].get('DATE');
            var produceLineId = this.dataSet.getTable(0).data.items[0].get("PRODUCELINEID");
            if (date) {
                var data = this.invorkBcf('BtnLoad', [date, produceLineId]);
                this.funBtnLoad.call(this,data);
            }
        }
        else if (e.dataInfo.fieldName == 'btnBuild') {//生成月计划
            var store = this.dataSet.getTable(1).data.items;
            var workOrderNoList = [];
            for (var i = 0;i < store.length;i++) {
                workOrderNoList.push(store[i].get('WORKORDERNO'));
            }
            this.invorkBcf('BtnBuild', [workOrderNoList]);
        }
        else if (e.dataInfo.fieldName == "btnDelete") {
            this.invorkBcf('DeleteProduceLineWorkShopSectionDayTimeRecord', []);
        }
        else if (e.dataInfo.fieldName == "btnQuota") {
            var store = this.dataSet.getTable(1).data.items;
            var workOrderNoList = [];
            for (var i = 0; i < store.length; i++) {
                workOrderNoList.push(store[i].get('WORKORDERNO'));
            }
            this.invorkBcf('QuotaCalc', [workOrderNoList]);
        }
        else if (e.dataInfo.fieldName == "BtnDeleteQuota") {
            var date = this.dataSet.getTable(0).data.items[0].get('DATE');
            this.invorkBcf("BtnDeleteQuota", [date]);
        }
    }
}

//加载明细表
proto.funBtnLoad = function (data) {
    Ext.suspendLayouts();//关闭Ext布局
    //  console.log(this.dataSet);
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        var list = data;
        this.dataSet.getTable(1).removeAll();
        if (list !== undefined && list.length > 0) {
            var masterRow = this.dataSet.getTable(0).data.items[0];
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set('WORKORDERNO', info.WORKORDERNO);
                newRow.set('PRIORITY', info.PRIORITY);
                newRow.set('PRODUCELINEID', info.PRODUCELINEID);
                newRow.set('PRODUCELINENAME', info.PRODUCELINENAME);
                newRow.set('PLANSTARTDATE', info.PLANSTARTDATE);
                newRow.set('PLANENDDATE', info.PLANENDDATE);
                newRow.set('ISMIXED', info.ISMIXED);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}