emMaterialSafeStockVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
}
//grid用Ax.vcl.LibVclGrid,单据主数据用Ax.vcl.LibVclData,datafunc用Ax.vcl.LibVclGrid
var proto = emMaterialSafeStockVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = emMaterialSafeStockVcl;
proto.winId = "";
proto.doSetParam = function () {
    var allMaterialId = null;
    //调用后台方法获取备件数据
    var data = this.invorkBcf("GetMatearialStockPost", [allMaterialId]);
    FillData.call(this, data);
};
var ROW_ID = new Array();
var vcl = this.vcl;
proto.vclHandler = function (sender, e) {

    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {

        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "CheckAll") {//全选按钮

                //全选操作
                for (var i = 0; i < this.dataSet.getTable(1).data.length; i++) {
                    this.dataSet.getTable(1).data.items[i].set("ISSTOCK", 1);
                }
            }

            if (e.dataInfo.fieldName == "UnCheckAll") {//全不选按钮

                //全不选操作
                for (var i = 0; i < this.dataSet.getTable(1).data.length; i++) {
                    this.dataSet.getTable(1).data.items[i].set("ISSTOCK", 0);
                }
            }

            if (e.dataInfo.fieldName == "BtnLoad") {//查询按钮

                //获取表头订单号
                var materialId = this.dataSet.getTable(0).data.items[0].data["MATERIALID"];

                //调用后台方法获取备件数据
                var data = this.invorkBcf("GetMatearialStockPost", [materialId]);
                FillData.call(this, data);
            }

            if (e.dataInfo.fieldName == "BtnHander") {//处理按钮
                var istrue = false;
                if (this.dataSet.getTable(1).data.length != 0) {
                    var isEmpty = 1;
                    var isstockTable = this.dataSet.getTable(1).data.items;
                    for (var i = 0; i < this.dataSet.getTable(1).data.length; i++) {
                        if (isstockTable[i].data["ISSTOCK"] == 1) {
                            //存在被选中的明细
                            isEmpty = 0;
                            break;
                        }
                    }
                    if (isEmpty == 0) {
                        Ext.MessageBox.confirm("确认", "更改已选中备件的安全库存？", function (btn) {
                            if (btn == "yes") {
                                var bodyTable = this.dataSet.getTable(1).data.items;
                                var list = new Array();
                                for (var i = 0; i < bodyTable.length; i++) {
                                    if (bodyTable[i].data["ISSTOCK"] == 1) {
                                        list.push({//传入的数据
                                            MATERIALID: bodyTable[i].data["MATERIALID"],
                                            DYNAMICSAFESTOCK: bodyTable[i].data["DYNAMICSAFESTOCK"],
                                        });
                                    }
                                }
                                var handerSuccess = false;
                                //调用后台方法，将备件安全库存更新
                                handerSuccess = vcl.invorkBcf("HanderAbnormalMaterialStock", [list]);

                                if (handerSuccess == true) {
                                    Ext.Msg.alert("提示", "更改成功！");
                                    //this.dataSet.getTable(1).removeAll();
                                }
                            }
                        }, this);
                    }
                    else {
                        Ext.Msg.alert("提示", "请选择需更改的备件明细！");
                    }

                }
                else {
                    Ext.Msg.alert("提示", "暂无可处理项！");
                }


            }
            break;

        case LibEventTypeEnum.BeforeAddRow://不允许新增行
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow://不允许删除行
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
    }
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
                newRow.set("MATERIALID", info.MaterialId);
                newRow.set("MATERIALNAME", info.MaterialName);
                newRow.set("MATERIALSPEC", info.MaterialSpec);
                newRow.set("MATERIALTYPEID", info.MaterialTypeId);
                newRow.set("MATERIALTYPENAME", info.MaterialTypeName);
                newRow.set("SAFESTORAGENUM", info.SafeStorageNum);
                newRow.set("SAFETYFACTOR", info.SafetyFactor);
                newRow.set("DYNAMICSAFESTOCK", info.DynamicSafeStock);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}
