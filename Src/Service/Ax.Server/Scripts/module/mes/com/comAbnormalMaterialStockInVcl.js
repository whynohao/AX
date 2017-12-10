comAbnormalMaterialStockInVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
}
//grid用Ax.vcl.LibVclGrid,单据主数据用Ax.vcl.LibVclData,datafunc用Ax.vcl.LibVclGrid
var proto = comAbnormalMaterialStockInVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comAbnormalMaterialStockInVcl;
proto.winId = "";
proto.doSetParam = function () {

};
var ROW_ID = new Array();
var vcl = this.vcl;
proto.vclHandler = function (sender, e) {

    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {

        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnLoad") {//查询按钮

                //获取表头订单号
                var fromBillNo = this.dataSet.getTable(0).data.items[0].data["FROMBILLNO"];
                if (fromBillNo == "" || fromBillNo == null || fromBillNo == 0) {
                    Ext.Msg.alert("提示", "销售单号不能为空！");
                }
                else {

                    //调用后台方法获取异常备料过账表数据
                    var data = this.invorkBcf("GetMatearialStockPost", [fromBillNo]);
                    FillData.call(this, data);
                    Ext.Msg.alert("提示", "查询成功！");
                }
            }
            if (e.dataInfo.fieldName == "BtnHander") {//处理按钮
                var istrue = false;
                if (this.dataSet.getTable(1).data.length != 0) {
                    Ext.MessageBox.confirm("确认", "物料是否已入库？", function (btn) {
                        if (btn == "yes") {
                            var bodyTable = this.dataSet.getTable(1).data.items;
                            var list = new Array();
                            for (var i = 0; i < bodyTable.length; i++) {
                                list.push({//传入的数据
                                    ROW_ID: bodyTable[i].data["ROW_ID"],
                                    ENDDATE: bodyTable[i].data["ENDDATE"],
                                    HANDERWAY: bodyTable[i].data["HANDERWAY"],
                                    FROMBILLNO: bodyTable[i].data["FROMBILLNO"],
                                    MATERIALID: bodyTable[i].data["MATERIALID"],
                                    ATTRBUTECODE: bodyTable[i].data["ATTRBUTECODE"],
                                    STARTDATE: bodyTable[i].data["STARTDATE"],
                                    ABNORMALDAY: bodyTable[i].data["ABNORMALDAY"],
                                    FINISHDATE: bodyTable[i].data["FINISHDATE"],
                                });
                            }
                            var handerSuccess = false;
                            //调用后台方法，将处理方式和处理结果更新到异常备料过账表中
                            handerSuccess = vcl.invorkBcf("HanderAbnormalMaterialStock", [list]);

                            if (handerSuccess == true) {
                                Ext.Msg.alert("提示", "处理成功！");
                                this.dataSet.getTable(1).removeAll();
                            }
                        }
                    }, this);
                }
                else {
                    Ext.Msg.alert("提示", "暂无可处理项！");
                }


            }
            break;

        case LibEventTypeEnum.BeforeAddRow://只能删除行，不允许新增行
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
    }


    function FillData(returnData) {
        Ext.suspendLayouts();//关闭Ext布局
        //  console.log(this.dataSet);
        var curStore = this.dataSet.getTable(1);
        curStore.suspendEvents();//关闭store事件
        try {
            this.dataSet.getTable(1).removeAll();
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var list = returnData;
            if (list != undefined && list.length > 0) {
                for (var i = 0; i < list.length; i++) {
                    var info = list[i];
                    var newRow = this.addRow(masterRow, 1);
                    newRow.set("HANDERWAY", info.HanderWay);
                    newRow.set("FROMBILLNO", info.FromBillNo);
                    newRow.set("MATERIALID", info.MaterialId);
                    newRow.set("MATERIALNAME", info.MaterialName);
                    newRow.set("ATTRIBUTEID", info.AttributeId);
                    newRow.set("ATTRIBUTENAME", info.AttributeName);
                    newRow.set("ATTRBUTECODE", info.AttrbuteCode);
                    newRow.set("ATTRBUTEDESC", info.AttrbuteDesc);
                    newRow.set("QUANTITY", info.Quantity);
                    newRow.set("ABNORMALDAY", info.AbnormalDay);
                    newRow.set("ENDDATE", info.EndDate);
                    newRow.set("STARTDATE", info.StartDate);
                    newRow.set("FINISHDATE", info.FinishDate);
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

