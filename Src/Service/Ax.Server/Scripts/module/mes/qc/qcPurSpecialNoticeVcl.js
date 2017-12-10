qcPurSpecialNoticeVcl  = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = qcPurSpecialNoticeVcl .prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = qcPurSpecialNoticeVcl ;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var mastertable = this.dataSet.getTable(0);
    switch(e.libEventType)
    {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "TYPEID" || e.dataInfo.fieldName == "BILLDATE" || e.dataInfo.fieldName == "RECEIVEDATE" || e.dataInfo.fieldName == "SUPPLIERID" || e.dataInfo.fieldName == "PERSONID") {
                    this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                }
                if (e.dataInfo.fieldName == "RECEIVEDATE") {
                    var record = mastertable.data.items[0];
                    SupplierId = e.dataInfo.dataRow.get("SUPPLIERID");
                    PersonId = e.dataInfo.dataRow.get("PERSONID");
                    ReceiveDate = e.dataInfo.value;
                    if (SupplierId != "" && PersonId != "" &&ReceiveDate!="") {
                        var returnList = this.invorkBcf("GetPurQualityCheckData", [SupplierId,PersonId,ReceiveDate]);
                        fillData.call(this, returnList);
                    }
                }
                if (e.dataInfo.fieldName == "SUPPLIERID") {
                    var record = mastertable.data.items[0];
                    SupplierId = e.dataInfo.value;
                    PersonId = e.dataInfo.dataRow.get("PERSONID");
                    ReceiveDate = e.dataInfo.dataRow.get("RECEIVEDATE");
                    if (SupplierId != "" && PersonId != "" && ReceiveDate != "") {
                        var returnList = this.invorkBcf("GetPurQualityCheckData", [SupplierId, PersonId, ReceiveDate]);
                        fillData.call(this, returnList);
                    }
                }
                if (e.dataInfo.fieldName == "PERSONID") {
                    var record = mastertable.data.items[0];
                    SupplierId = e.dataInfo.dataRow.get("SUPPLIERID");
                    PersonId = e.dataInfo.value;
                    ReceiveDate = e.dataInfo.dataRow.get("RECEIVEDATE");
                    if (SupplierId != "" && PersonId != "" && ReceiveDate != "") {
                        var returnList = this.invorkBcf("GetPurQualityCheckData", [SupplierId, PersonId, ReceiveDate]);
                        fillData.call(this, returnList);
                    }
                }
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
    }
    
}

function fillData(returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'PURSPECIALNOTICEDETAILGrid');
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        if (returnList !== undefined && returnList.length > 0) {
            for (var i = 0; i < returnList.length; i++) {
                var info = returnList[i];
                var newRow = this.addRowForGrid(grid);//这个方法第一个参数是表头数据行，第二个参数是当前grid数据源store所属第几个表
                newRow.set('FROMBILLNO', info.FromBillNo);
                newRow.set('FROMROWID', info.FromRow_Id);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('CHECKTYPE', info.CheckType);
                newRow.set('WORKSTATIONCONFIGID', info.WorkStaionConfigId);
                newRow.set('QUANTITY', info.Quantity);
                newRow.set('CHECKNUM', info.CheckNum);
                newRow.set('ISUSECHECK', info.IsUseCheck);
                newRow.set('ISCONFIG', info.IsConfig);
                newRow.set('BATCHNO', info.BatchNo);
                newRow.set('SUBBATCHNO', info.SubBatchNo);
                newRow.set('QUALIFIEDNUM', info.QualifiedNum);
                newRow.set('UNQUALIFIEDNUM', info.UnqualifiedNum);
                newRow.set('STARTTIME', info.StartTime);
                newRow.set('ENDTIME', info.EndTime);
                
            }
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}