stkDeliveryOrderVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = stkDeliveryOrderVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = stkDeliveryOrderVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);


    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            e.dataInfo.cancel = true;
            break;
        case LibEventTypeEnum.Validated:
            //var form = this.dataSet.getTable(0).data.items[0];
            //this.forms[0].updateRecord(form);//更新表头数据
            //e.dataInfo.cancel = true;

            if (e.dataInfo.fieldName == 'PRODUCTORDERID') {
                var headTableRow = this.dataSet.getTable(0).data.items[0];

                var returnData = this.invorkBcf("GetProjectData", [headTableRow.data["PRODUCTORDERID"]]);
                if (returnData.length == 0) {
                    Ext.Msg.alert("提示", "投产单为空！");
                    this.deleteall(1);
                    return;
                }
                fillDeliveryPlan(this, returnData);
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "LoadProjectDetail":
                    var headTableRow = this.dataSet.getTable(0).data.items[0];
                    if (headTableRow.data["PRODUCTORDERID"] == '') {
                        Ext.Msg.alert("提示", '请维护表头合同号！');
                    }
                    else {
                        var returnData = this.invorkBcf("GetProjectData", [headTableRow.data["PRODUCTORDERID"]]);
                        if (returnData.length == 0) {
                            Ext.Msg.alert("提示", "项目单物料为空！");
                            this.deleteall(1);
                            return;
                        }
                        fillDeliveryPlan(this, returnData);



                    }
                    break;
                case "CreateMark":
                    break;
            }
            break;
    }
}


function fillDeliveryPlan(This, returnData) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        This.deleteAll(1);
        var masterRow = This.dataSet.getTable(0).data.items[0];
        if (returnData !== undefined && returnData.length > 0) {
            for (var i = 0; i < returnData.length; i++) {
                var info = returnData[i];
                var newRow = This.addRow(masterRow, 1);
                newRow.set('ROW_ID', info.RowId);
                newRow.set('ROWNO', info.RowNo);
                newRow.set('METERNO', info.MeterNo);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('SPECIFICATION', info.SpecIfication);
                newRow.set('TEXTUREID', info.Textureid);
                newRow.set('MATERIALSPEC', info.MaterialSpec);
                newRow.set('UNITNAME', info.UnitName);
                newRow.set('QUANTITY', info.Quantity);

            }
        }
    }

    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}