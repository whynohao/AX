intFieldMappingVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
}

var proto = intFieldMappingVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = intFieldMappingVcl;
var options;//只有点击查询数据和重置条件时才会修改该值

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validating:
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                this.forms[0].updateRecord(e.dataInfo.dataRow);
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "BTNSELECT"://查询数据
                    options = this.dataSet.getTable(0).data.items[0].data['OPTIONS'];//功能选项
                    var selectCondition = this.dataSet.getTable(0).data.items[0].data['SELECTCONDITION'];//查询条件
                    var selectNumber = this.dataSet.getTable(0).data.items[0].data['SELECTNUMBER'];//查询数量
                    var selectText = this.dataSet.getTable(0).data.items[0].data['SELECTTEXT'];//查询名称

                    //调用后台获取数据方法
                    var bodyData = this.invorkBcf("GetIdAndName", [options, selectCondition, selectNumber, selectText]);

                    //填充数据到明细表
                    FillData.call(this, bodyData);
                    break;
                case "BTNRESET"://重置条件
                    //表头清空
                    Ext.getCmp("OPTIONS0_" + this.winId).setValue(0);
                    Ext.getCmp("SELECTCONDITION0_" + this.winId).setValue(0);
                    Ext.getCmp("SELECTNUMBER0_" + this.winId).setValue(0);
                    Ext.getCmp("SELECTTEXT0_" + this.winId).setValue("");
                    this.forms[0].updateRecord(e.dataInfo.dataRow);

                    //将明细表也重新置空
                    var bodyTable = this.dataSet.getTable(1).data;
                    bodyTable.removeAll();
                    options = 0;

                    break;
                case "BTNSAVE"://保存数据
                    var bodyTable = this.dataSet.getTable(1).data.items;
                    var list = new Array();
                    for (var i = 0; i < bodyTable.length; i++) {
                        list.push({
                            'ID': bodyTable[i].data['ID'],
                            'NAME': bodyTable[i].data['NAME'],
                            'DELIVERYAREA': bodyTable[i].data['DELIVERYAREA']
                            });
                    }
                    //options = this.dataSet.getTable(0).data.items[0].data['OPTIONS'];
                    //调用后台保存二开字段方法
                    var returnResult = this.invorkBcf("SaveIdAndNameAndDeliveryArea", [list, options]);
                    if (returnResult==true) {
                        Ext.Msg.alert("提示", "维护成功");
                    }
                    break;
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
                    newRow.set("ROW_ID", i+1);
                    newRow.set("ROWNO", i+1);
                    newRow.set("ID", info.Id);
                    newRow.set("NAME", info.Name);
                    newRow.set("DELIVERYAREA", info.DeliveryArea);
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