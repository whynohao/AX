plsProductiveDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = plsProductiveDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = plsProductiveDataFuncVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            
            break;
        case LibEventTypeEnum.BeforeAddRow:

            e.dataInfo.cancel = true;

            break;
        case LibEventTypeEnum.BeforeDeleteRow:

            //e.dataInfo.cancel = true;

            break;
        case LibEventTypeEnum.ButtonClick:
            //关闭
            if (e.dataInfo.fieldName == "BtnCloseProductive") {
                this.win.close();

            }
            //查询
            if (e.dataInfo.fieldName == "BtnSelectProductive") {
                var headTable = this.dataSet.getTable(0).data.items[0];

                if ((headTable.data["COMPRODUCTID"] == null || headTable.data["COMPRODUCTID"] == "") && headTable.data["STARTDATE"] == 0 && headTable.data["ENDDATE"] == 0) {
                    Ext.Msg.alert("系统提示", "表头上至少有一个字段作为查询条件！！");
                }
                else {
                    var lst = [];
                    var dt = this.dataSet.getTable(1);
                    for (var i = 0; i < dt.data.items.length; i++) {
                        var row = dt.data.items[i];
                        lst.push(
                            {
                                RecordId: row.data["RECORDID"],
                            });

                    }
                    var returnData = this.invorkBcf("GetProductiveData", [headTable.data["COMPRODUCTID"], headTable.data["STARTDATE"], headTable.data["ENDDATE"],lst]);
                    fillProductiveDataFunc(this, returnData);
                } 
            }
           // 修改
            if (e.dataInfo.fieldName == "BtnLoadProductive") {
                var lst = [];
                var bool = true;
                var msg;
                var dt = this.dataSet.getTable(1);
                for (var i = 0; i < dt.data.items.length; i++) {
                    var row = dt.data.items[i];
                    lst.push(
                        {
                            RecordId: row.data["RECORDID"],
                            NeedQty: row.data["NEEDQTY"],
                            ProduceQty: row.data["PRODUCEQTY"],
                            ProductQty: row.data["PRODUCTQTY"],
                        });

                }
                for (var i = 0; i < lst.length; i++) {
                        if (lst[i].ProduceQty < lst[i].NeedQty) {
                            msg = "修改的数据中生产数量不能小于需求数量！！";
                            bool = false;
                            break;
                        }
                        if (lst[i].ProductQty > 0) {
                            if (lst[i].ProduceQty >= lst[i].NeedQty && lst[i].ProduceQty < lst[i].ProductQty) {
                                msg = "修改的数据中生产数量数量不能小于投产数量！！";
                                bool = false;
                                break;
                            }
                        }
                }
                if (bool) {
                    if (lst.length > 0) {
                        var returnData = this.invorkBcf("SaveProductive", [lst]);
                        Ext.Msg.alert("提示", returnData);

                    }
                    else {
                        Ext.Msg.alert("提示", '没有要修改的行！');
                    }
                }
                else {
                    Ext.Msg.alert("提示", msg);
                }
            }
            if (e.dataInfo.fieldName == "BtnClearProductive") {
               
                var dt = this.dataSet.getTable(1);
                dt.removeAll();
            }
            if (e.dataInfo.fieldName == "BtnCreatProductive") {
                var hearTable=this.dataSet.getTable(0);
                var bodyTable = this.dataSet.getTable(1);
                var lst = [];
                var bool = true;
                var msg;
                if (hearTable.data.items[0].data["STARSDATE"] != "") {
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        var row = bodyTable.data.items[i];                      
                        if (row.data["ISCHOSE"]) {
                            lst.push(
                                {
                                    RecordId: row.data["RECORDID"],
                                    Materialid: row.data["MATERIALID"],
                                    Materialname: row.data["MATERIALNAME"],
                                    SpecIfication: row.data["SPECIFICATION"],
                                    Textureid: row.data["TEXTUREID"],
                                    FigureNo: row.data["FIGURENO"],
                                    FromBillno: row.data["FROMBILLNO"],
                                    ContractNo: row.data["CONTRACTNO"],
                                    NeedQty: row.data["NEEDQTY"],
                                    ProduceQty: row.data["PRODUCEQTY"],
                                    ProductQty: row.data["PRODUCTQTY"],
                                    Date: row.data["DATE"],
                                    BomId: row.data["BOMID"],
                                    BomRowId: row.data["BOMROWID"],
                                    //BomLevel: row.data["BOMLEVEL"]
                                });
                        }
                    }
                    if (lst.length > 1)
                    {
                        msg = "只能选择一条数据去生成任务单！";
                        bool = false;
                    }
                    else
                    {
                        for (var i = 0; i < lst.length; i++) {
                            if (i > 0) {
                                if (lst[i].Materialid != lst[i - 1].Materialid) {
                                    msg = "选择生成的数据中的物料ID不一致！";
                                    bool = false;
                                    break;
                                }
                                else if (lst[i].Date != lst[i - 1].Date) {
                                    msg = "选择生成的数据中的日期不一致!"
                                    bool = false;
                                    break;
                                }
                            }
                            if (hearTable.data.items[0].data["PRODUCTQTY"] == 0) {
                                msg = "表头的投产数量不能为0"
                                bool = false;
                                break;
                            }
                            if ((lst[i].ProduceQty - lst[i].ProductQty) < hearTable.data.items[0].data["PRODUCTQTY"]) {
                                msg = "表头的投产数量不能大于明细的生产数量减去投产数量！！"
                                bool = false;
                                break;
                            }

                        }
                    }
                    if (lst.length > 0) {
                        if (bool) {
                            var returnData = this.invorkBcf("CraetData", [lst, hearTable.data.items[0].data["STARSDATE"], hearTable.data.items[0].data["PRODUCTQTY"]]);
                            Ext.Msg.alert("提示", returnData);
                        }
                        else
                        {
                            Ext.Msg.alert("提示", msg);
                        }
                    }
                    else {
                        Ext.Msg.alert("提示", '没有要生成的行！');
                    }
                }
                else {
                    Ext.Msg.alert("提示", '请先维护表头的开工日期！！！');
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            //if (e.dataInfo.tableIndex == 1) {
            //    if (e.dataInfo.fieldName == "PRODUCEQTY") {
            //        var bodyTable = this.dataSet.getTable(1);
            //        for (var i = 0; i < bodyTable.data.length; i++) {
            //            if (bodyTable.data.items[i].get("PARENTROWID") == e.dataInfo.dataRow.get("ROW_ID")) {
            //                bodyTable.data.items[i].set('MATSTYLE', e.dataInfo.value);
            //            }
            //        }
            //        this.forms[0].loadRecord(headTable.data.items[0]);
            //    }
            //}
            //if (e.dataInfo.tableIndex == 0) {
            //    if (e.dataInfo.fieldName == "DATE") {
            //        var bodyTable = this.dataSet.getTable(2);
            //        for (var i = 0; i < bodyTable.data.length; i++) {

            //            bodyTable.data.items[i].set('DELIVERYDATE', e.dataInfo.value);

            //        }
            //        this.forms[0].loadRecord(headTable.data.items[0]);
            //    }
            //}
            break;
    }
}

//填充明细数据
function fillProductiveDataFunc(This, returnData) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        //This.deleteAll(1);
        var masterRow = This.dataSet.getTable(0).data.items[0];
        var bodyRow = This.dataSet.getTable(1);
        var bool = true;
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = This.addRow(masterRow, 1);
                    newRow.set('RECORDID', info.RecordId);
                    newRow.set('MATERIALID', info.MaterialId);
                    newRow.set('MATERIALNAME', info.MaterialName);
                    newRow.set('SPECIFICATION', info.SpecIfication);
                    newRow.set('TEXTUREID', info.Textureid);
                    newRow.set('FIGURENO', info.FigureNo);
                    newRow.set('FROMBILLNO', info.FromBillno);
                    newRow.set('CONTRACTNO', info.ContractNo);
                    newRow.set('NEEDQTY', info.NeedQty);
                    newRow.set('PRODUCEQTY', info.ProduceQty);
                    newRow.set('PRODUCTQTY', info.ProductQty);
                    newRow.set('DATE', info.Date);
                    newRow.set('BOMID', info.BomId);
                    newRow.set('BOMROWID', info.BomRowId);
                    newRow.set('INVENTORYQTY', info.InventoryQty);
                    //newRow.set('BOMLEVEL', info.BomLevel);
                
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