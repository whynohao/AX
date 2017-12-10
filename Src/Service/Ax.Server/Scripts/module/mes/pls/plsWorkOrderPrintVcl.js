plsWorkOrderPrintVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsWorkOrderPrintVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsWorkOrderPrintVcl;
proto.winId = null;
proto.fromObj = null;
proto.getType = 0;
proto.doSetParam = function (vclObj) {
    //console.log(arguments);
    //proto.winId = vclObj[0].winId;
    //proto.fromObj = vclObj[0];
};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    //不允许手工添加行
    if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }
        //不允许手工删除行
    else if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }

    else if (e.libEventType == LibEventTypeEnum.ButtonClick) {

        //保存
        if (e.dataInfo.fieldName == "btnSave") {
            var data = this.dataSet.getTable(1);
            if (data.data.items.length == 0) {
                Ext.Msg.alert("提示", "当前列表为空！");
            }
            else {
                var list = [];
                for (var i = 0; i < data.data.items.length; i++) {
                    var row = data.data.items[i];
                    list.push(
                    {
                        Billno: row.data["BILLNO"],
                        Billno: row.data["BILLNO"],
                        Customerid: row.data["CUSTOMERID"],
                        Customername: row.data["CUSTOMERNAME"],
                        Singledate: row.data["SINGLEDATE"],
                        Frombillno: row.data["FROMBILLNO"],
                        Attributedesc: row.data["ATTRIBUTEDESC"],
                        Materialtype: row.data["MATERIALTYPE"],
                        Menquantity: row.data["MENQUANTITY"],
                        Mentaoquantity: row.data["MENTAOQUANTITY"],
                        Guimenquantity: row.data["GUIMENQUANTITY"],
                        Tongzibanquantity: row.data["TONGZIBANQUANTITY"],
                        Tielianquantity: row.data["TIELIANQUANTITY"],
                        Tijiaoxianquantity: row.data["TIJIAOXIANQUANTITY"],
                        Otherquantity: row.data["OTHERQUANTITY"],
                        Customerdate: row.data["CUSTOMERDATE"],
                        Lotno: row.data["LOTNO"],
                        Pmcorderdate: row.data["PMCORDERDATE"],
                        Pmcgetbilldate: row.data["PMCGETBILLDATE"]
                    })
                }
                this.invorkBcf("SetWorkOrder", [list]);
                Ext.Msg.alert("提示", "保存成功！");
            }

        }
            //查询
        else if (e.dataInfo.fieldName == "btnSelect") {

            //获取编号
            var BillNo = this.dataSet.getTable(0).data.items[0].data["BILLNO"];
            if (BillNo == '') {
                Ext.Msg.alert("提示", "请输入作业单号！");
            }
            else {
                //调用后台方法获取异常备料过账表数据
                var data = this.invorkBcf("GetWorkOrder", [BillNo]);
                FillWorkOrder.call(this, data);
                Ext.Msg.alert("提示", "查询成功！");
            }
        }
    }
}

function FillWorkOrder(returnData) {
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
                newRow.set("BILLNO", info.Billno);
                newRow.set("CUSTOMERID", info.Customerid);
                newRow.set("CUSTOMERNAME", info.Customername);
                newRow.set("SINGLEDATE", info.Singledate);
                newRow.set("FROMBILLNO", info.Frombillno);
                newRow.set("ATTRIBUTEDESC", info.Attributedesc);
                newRow.set("MATERIALTYPE", info.Materialtype);
                newRow.set("MENQUANTITY", info.Menquantity);
                newRow.set("MENTAOQUANTITY", info.Mentaoquantity);
                newRow.set("GUIMENQUANTITY", info.Guimenquantity);
                newRow.set("TONGZIBANQUANTITY", info.Tongzibanquantity);
                newRow.set("TIELIANQUANTITY", info.Tielianquantity);
                newRow.set("TIJIAOXIANQUANTITY", info.Tijiaoxianquantity);
                newRow.set("OTHERQUANTITY", info.Otherquantity);
                newRow.set("CUSTOMERDATE", info.Customerdate);
                newRow.set("LOTNO", info.Lotno);
                newRow.set("PMCORDERDATE", info.Pmcorderdate);
                newRow.set("PMCGETBILLDATE", info.Pmcgetbilldate);

            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

