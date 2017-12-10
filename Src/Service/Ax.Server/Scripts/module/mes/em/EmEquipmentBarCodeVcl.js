EmEquipmentBarCodeVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = EmEquipmentBarCodeVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = EmEquipmentBarCodeVcl;

var btnSelect = 0;//记录全选还是不选

//界面加载
proto.doSetParam = function () {
    var returnList = this.invorkBcf("GetEquipmentInfo", [""]);
    FillEquipment.call(this, returnList);
};

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo.fieldName == "EQUIPMENTID") {
            var returnList = this.invorkBcf("GetEquipmentInfo", [this.dataSet.getTable(0).data.items[0].data["EQUIPMENTID"]]);
            FillEquipment.call(this, returnList);
        }
    }
    else if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "btnSelectAll") {
            var allItems = this.dataSet.getTable(1).data.items;
            if (btnSelect == 0) {
                for (var i = 0; i < allItems.length; i++) {
                    allItems[i].set("ISCHOSE", true);
                }
                btnSelect = 1;
            }
            else {
                for (var i = 0; i < allItems.length; i++) {
                    allItems[i].set("ISCHOSE", false);
                }
                btnSelect = 0;
            }
        }
        else if (e.dataInfo.fieldName == "btnBarCode") {
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var allItems = this.dataSet.getTable(1).data.items;
            if (masterRow.data["LABELTEMPLATEID"] != "") {
                for (var i = 0; i < allItems.length; i++) {
                    if (allItems[i].data["ISCHOSE"] == true) {
                        var code = this.invorkBcf("ReadPrintTemplateTxt", [masterRow.data["LABELTEMPLATEID"]]);
                        var LODOP = getLodop(document.getElementById('LODOP'), document.getElementById('LODOP_EM'));
                        eval(code.replace("@BarCode", allItems[i].data["EQUIPMENTID"]));
                        LODOP.PRINT();
                    }
                }
            }
            else
                Ext.Msg.alert("系统提示", "请选择条码模板");
        }
    }
}


function FillEquipment(returnList) {
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
                newRow.set("EQUIPMENTID", info.EquipmentId);
                newRow.set("EQUIPMENTNAME", info.EquipmentName);
                newRow.set("EQUIPMENTMODEL", info.EquipmentModel);
                newRow.set("FACTORYID", info.FactoryId);
                newRow.set("FACTORYNAME", info.FactoryName);
                newRow.set("PRODUCELINEID", info.ProduceLineId);
                newRow.set("PRODUCELINENAME", info.ProduceLineName);
                newRow.set("EQUIPMENTPOSITION", info.Localtion);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}
