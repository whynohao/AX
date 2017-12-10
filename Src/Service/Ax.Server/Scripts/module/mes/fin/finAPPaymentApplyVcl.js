
finAPPaymentApplyVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = finAPPaymentApplyVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finAPPaymentApplyVcl;
var returnList;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (this.isEdit) {
                if (e.dataInfo.fieldName == "BtnLoadData") {
                        Ax.utils.LibVclSystemUtils.openDataFunc('fin.APPayApplyDataFunc', '载入来源单', [this]);

                }
            }
            else {
                alert("单据只有在修改状态才能载入数据！");
            }
            break; 
        //case LibEventTypeEnum.BeforeAddRow:
        //    if (e.dataInfo.tableIndex == 1) {
        //      e.dataInfo.cancel = true;
        //    }
            //break;
        case LibEventTypeEnum.Validated:
            var headTable = this.dataSet.getTable(0);
            var bodyTable = this.dataSet.getTable(1);
            if (e.dataInfo.fieldName == "APPLYAMOUNT") { 
                var amount = 0;
                e.dataInfo.dataRow.set("APPLYAMOUNT", e.dataInfo.value);
                for (var i = 0; i < bodyTable.data.length; i++) {
                    amount += bodyTable.data.items[i].data["APPLYAMOUNT"];
                }
                headTable.data.items[0].set('APPLYTOTALACCOUNT', amount);
                headTable.data.items[0].set('APPROVEACCOUNT', amount);
                e.dataInfo.dataRow.set('CHECKAMOUNT', e.dataInfo.value);
                e.dataInfo.dataRow.set('APPROVEAMOUNT', e.dataInfo.value);
                this.forms[0].loadRecord(headTable.data.items[0]);
            }
            if (e.dataInfo.fieldName == "CHECKAMOUNT") {
                var amount = 0;
                e.dataInfo.dataRow.set("CHECKAMOUNT", e.dataInfo.value);
                for (var i = 0; i < bodyTable.data.length; i++) {
                    amount += bodyTable.data.items[i].data["CHECKAMOUNT"];
                }
                headTable.data.items[0].set('APPROVEACCOUNT', amount);
                e.dataInfo.dataRow.set('APPROVEAMOUNT', e.dataInfo.value);
                this.forms[0].loadRecord(headTable.data.items[0]);
            }
            if (e.dataInfo.fieldName == "APPROVEAMOUNT") {
                var amount = 0;
                headTable.data.items[0].set('APPROVEAMOUNT', amount);
                if (e.dataInfo.value > e.dataInfo.dataRow.data["APPLYAMOUNT"]) {
                    Ext.Msg.alert('提示', '确认审核金额大于应付金额');
                }
                for (var i = 0; i < bodyTable.data.length; i++) {
                    amount += bodyTable.data.items[i].data["APPROVEAMOUNT"];
                }
                headTable.data.items[0].set('APPROVEACCOUNT', amount);
                this.forms[0].loadRecord(headTable.data.items[0]);
            }
            if (e.dataInfo.fieldName == "CONTACTSOBJECTID") {

                var contactsobjectId = e.dataInfo.value;
                var isDue = false;
                returnList = this.invorkBcf('GetData', [contactsobjectId, isDue]);
                var Row = e.dataInfo.dataRow;
                this.FillData(returnList, Row);
            }
            break;
        case LibEventTypeEnum.DeleteRow:
            var headTable = this.dataSet.getTable(0);
            var bodyTable = this.dataSet.getTable(1);
            var applyAmount = 0;
            var approveAmount = 0;
            for (var i = 0; i < bodyTable.data.length; i++) {
                applyAmount += bodyTable.data.items[i].data["APPLYAMOUNT"];
            }
            for (var i = 0; i < bodyTable.data.length; i++) {
                approveAmount += bodyTable.data.items[i].data["APPROVEAMOUNT"];
            }
            headTable.data.items[0].set('APPLYTOTALACCOUNT', applyAmount);
            headTable.data.items[0].set('APPROVEACCOUNT', approveAmount);
            this.forms[0].loadRecord(headTable.data.items[0]);
            break;
    }
}

proto.FillData = function (returnList, Row) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        var list = returnList;
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                 Row.set('CONTACTSOBJECTID', info.ContactsobjectId);
                Row.set('CONTACTSOBJECTNAME', info.ContactsobjectName);
                Row.set('PAYAMOUNT', info.PayAmount);
                Row.set('DUEPAYAMOUNT', info.DuePayAmount);
                Row.set('ACTUALPAYAMOUNT', info.ActualPayAmount);
                var finAPApplyDetailInfo = info.FinAPApplyDetailInfo;
                if (finAPApplyDetailInfo.length > 0) {
                    for (var j = 0; j < finAPApplyDetailInfo.length; j++) {
                        Row.set("PAYDETAIL", 1);
                        var subInfo = finAPApplyDetailInfo[j];
                        var subRow = this.addRow(Row, 2);
                        subRow.set('PAYTYPE', subInfo.PayType);
                        subRow.set('FROMBILLNO', subInfo.FromBillNo);
                        subRow.set('INVOICENO', subInfo.InvoiceNo);
                        subRow.set('INVOICEDATE', subInfo.InvoiceDate);
                        subRow.set('PAYMENTDAYS', subInfo.PaymentDays);
                        subRow.set('ISDEADLINE', subInfo.IsDeadline);
                        subRow.set('PAYAMOUNT', subInfo.SubPayAmount);
                        subRow.set('VERIFICATIONACCOUNT', subInfo.VerificationAmount);
                        subRow.set('OFFSETACCOUNT', subInfo.OffsetAmount);
                    }
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

