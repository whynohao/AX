salEmarketSetVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = salEmarketSetVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = salEmarketSetVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    var bodyTable = this.dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
        case LibEventTypeEnum.BeforeDeleteRow:
            e.dataInfo.cancel = true;
            break;


        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
            }
            else {
                if (e.dataInfo.fieldName == 'PRICE' || e.dataInfo.fieldName == 'SALESPRICE') {
                    if (e.dataInfo.value < 0) {
                        Ext.Msg.alert("提示", '价格不能小于0！');
                        return;
                    }
                }

                e.dataInfo.dataRow.set("ISEDIT", true);
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "BtnSel":
                    var Materialid = this.dataSet.getTable(0).data.items[0].data['MATERIALID'];
                    var Materialname = this.dataSet.getTable(0).data.items[0].data['MATERIALNAME'];
                    var Typeid = this.dataSet.getTable(0).data.items[0].data['ATTRIBUTEITEMTYPEID'];
                    var Itemid = this.dataSet.getTable(0).data.items[0].data['ATTRIBUTEITEMID'];
                    var Materialspec = this.dataSet.getTable(0).data.items[0].data['MATERIALSPEC'];
                    if (Materialid == '' && Materialname == '' && Typeid == '' && Itemid == '' && Materialspec == '') {
                        Ext.Msg.alert("提示", '至少输入一个查询条件！');
                        return;
                    }

                    var returnData = this.invorkBcf("GetData", [Materialid, Materialname, Typeid, Itemid, Materialspec]);
                    if (returnData.length == 0) {
                        Ext.Msg.alert("提示", '查询结果为空！');
                        this.deleteAll(1);
                        this.deleteAll(2);
                    }
                    else {
                        fillFuncData.call(this, returnData);
                    }

                    break;
                case "BtnSave":
                    var lst = [];
                    var dt = this.dataSet.getTable(1);
                    for (var i = 0; i < dt.data.items.length; i++) {
                        var row = dt.data.items[i];
                        if (row.data["ISEDIT"]) {
                            lst.push(
                                {
                                    Taskid: row.data["TASKID"],
                                    Materialid: row.data["MATERIALID"],
                                    Materialname: row.data["MATERIALNAME"],
                                    Materialspec: row.data["MATERIALSPEC"],
                                    Attributeitemtypeid: row.data["ATTRIBUTEITEMTYPEID"],
                                    Attributeitemid: row.data["ATTRIBUTEITEMID"],
                                    Price: row.data["PRICE"],
                                    Salesprice: row.data["SALESPRICE"],
                                    Expirydate: row.data["EXPIRYDATE"],
                                    OldPrice: row.data["OLDPRICE"],
                                    OldSalesprice: row.data["OLDSALESPRICE"],
                                    OldExpirydate: row.data["OLDEXPIRYDATE"]
                                });
                        }
                    }
                    if (lst.length > 0) {
                        var returnData = this.invorkBcf("SaveData", [lst]);
                        Ext.Msg.alert("提示", returnData);

                    }
                    else {
                        Ext.Msg.alert("提示", '没有要保存的行！');
                    }
                    break;
                case "BtnDelete":
                    this.deleteAll(1);
                    this.deleteAll(2);
                    break;
            }
            break;
    }
}

function fillFuncData(returnData) {
    Ext.suspendLayouts();
    var formStore = this.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        this.deleteAll(1);
        this.deleteAll(2);
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var rowId = 1;

                var newRow = this.addRow(masterRow, 1);
                newRow.set('TASKID', info.Taskid);
                newRow.set('ROW_ID', rowId);
                newRow.set('MATERIALID', info.Materialid);
                newRow.set('MATERIALNAME', info.Materialname);
                newRow.set('ATTRIBUTEITEMTYPEID', info.Attributeitemtypeid);
                newRow.set('ATTRIBUTEITEMTYPENAME', info.Attributeitemtypename);
                newRow.set('ATTRIBUTEITEMID', info.Attributeitemid);
                newRow.set('ATTRIBUTEITEMNAME', info.Attributeitemname);
                newRow.set('MATERIALSPEC', info.Materialspec);
                newRow.set('PRICE', info.Price);
                newRow.set('SALESPRICE', info.Salesprice);
                newRow.set('EXPIRYDATE', info.Expirydate);
                newRow.set('OLDPRICE', info.Price);
                newRow.set('OLDSALESPRICE', info.Salesprice);
                newRow.set('OLDEXPIRYDATE', info.Expirydate);
                if (list[i].detail !== undefined && list[i].detail.length > 0) {
                    newRow.set('ATTRIBUTEITEMSUB', true);
                    for (var j = 0; j < list[i].detail.length; j++) {
                        var DetailRow = this.addRow(newRow, 2);
                        DetailRow.set('TASKID', list[i].detail[j].Taskid);
                        DetailRow.set('ROW_ID', list[i].detail[j].RowId);
                        DetailRow.set('PARENTROWID', rowId);
                        DetailRow.set('ATTRIBUTEITEMTYPEID', list[i].detail[j].Attributeitemtypeid);
                        DetailRow.set('ATTRIBUTEITEMTYPENAME', list[i].detail[j].Attributeitemtypename);
                        DetailRow.set('ATTRIBUTEITEMID', list[i].detail[j].Attributeitemid);
                        DetailRow.set('ATTRIBUTEITEMNAME', list[i].detail[j].Attributeitemname);
                        DetailRow.set('MATERIALSPEC', list[i].detail[j].Materialspec);
                    }
                }
                rowId++;
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