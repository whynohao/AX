qcPurQualityCheckDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = qcPurQualityCheckDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = qcPurQualityCheckDataFuncVcl;

proto.winId = null;
proto.fromObj = null;
//赋值方法
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj;
    var returnData = vclObj[1];
    this.fillData.call(this, returnData);
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);

    if (e.dataInfo && e.dataInfo.tableIndex == 1) {
        //新增行
        //表身不可手工新增
        if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
            e.dataInfo.cancel = true;
            return;
        }
        //删除行
        //表身不可手工删除
        if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
            e.dataInfo.cancel = true;
            return;
        }
    }
    //用户自定义按钮
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {                
        if (e.dataInfo.fieldName == "BtnOK") {
            //表头（取数据用masterRow.get("")）
            var masterRow = this.dataSet.getTable(0).data.items[0];
            //表身行项
            var selectItems = this.dataSet.getTable(1).data.items;
            //数组，用于存储数据
            var records = [];
            var abnormalDesc="";
            //循环所有行项
            for (var i = 0; i < selectItems.length; i++) {
                //如果打勾
                if (selectItems[i].data["ISCHOSE"] == true) {
                    //将行项对象加入数组
                    var desc = "";
                    desc = '质检单【' + selectItems[i].data["BILLNO"] + '】批号【' + selectItems[i].data["BATCHNO"]
                           + '】物料【' + selectItems[i].data["MATERIALNAME"] + '】质检不合格数量【'
                           + selectItems[i].data["UNQUALIFIEDNUM"] + '】' + '\n';
                    abnormalDesc += desc;                  
                }
            }
            if (abnormalDesc.length>0) {
                records.push({
                    AbnormalTypeId: masterRow.data["TYPEID"],
                    AbnormalPrototype: masterRow.data["ABNORMALPROTOTYPE"],
                    AbnormalId: masterRow.data["ABNORMALID"],
                    PersonId: masterRow.data["PERSONID"],
                    FromPersonId: masterRow.data["FROMPERSONID"],
                    AbnormalDesc: abnormalDesc

                });
            }            
            //datatfunc中未选择任何行就点击生成通知单则提示
            if (records.length == 0) {
                Ext.Msg.alert("系统提示", "请选择存在不合格数量的明细！");
                return;
            }
            if (records.length > 0) {
                //调用fillReturnData方法
                this.invorkBcf('CreateAbnormal', [records[0]]);
                this.win.close();
            }
        }
        if (e.dataInfo.fieldName == "BtnReturn") {
            this.win.close();
        }
    }
}



proto.fillData = function (returnData) {             //载入查询的所有数据，载入到datafunc上
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        //this.deleteAll(1);//删除当前grid的数据
        var grid = Ext.getCmp(this.winId + 'PURQUANTITYCHECKDATAFUNCDETAILGrid');
        //var list = returnData['stockoutdetail'];
        var list = returnData;
        //var masterRow = this.dataSet.getTable(0).data.items[0];
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRowForGrid(grid);
                newRow.set('ISCHOSE', info.IsChose);
                newRow.set('BILLNO', info.BillNo);
                newRow.set('ROW_ID', info.Row_Id);
                newRow.set('ROWNO', info.RowNo);
                newRow.set('FROMBILLNO', info.FromBillNo);
                newRow.set('FROMROWID', info.FromRowId);
                newRow.set('PURCHASEORDER', info.PurChaseOrder);
                newRow.set('BATCHNO', info.BatchNo);
                newRow.set('SUBBATCHNO', info.SubBatchNo);
                newRow.set('ATTRIBUTECODE', info.AttributeCode);
                newRow.set('ATTRIBUTEDESC', info.AttributeDesc);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('CHECKTYPE', info.CheckType);
                newRow.set('QUANTITY', info.Quantity);
                newRow.set('CHECKNUM', info.CheckNum);
                newRow.set('QUALIFIEDNUM', info.QualifiedNum);
                newRow.set('UNQUALIFIEDNUM', info.UnQualifiedNum);
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