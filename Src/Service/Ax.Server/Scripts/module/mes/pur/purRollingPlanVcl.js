purRollingPlanVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = purRollingPlanVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = purRollingPlanVcl;



proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {

        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnQueryData") {
                var supplierId = this.dataSet.getTable(0).data.items[0].data['SUPPLIERID'];
                var personId = this.dataSet.getTable(0).data.items[0].data['PERSONID'];
                var returnData = this.invorkBcf('GetRollPlan', [supplierId, personId]);
                fillData.call(this, returnData);
            }
            else if (e.dataInfo.fieldName == "BtnSaveData") {
                var table = this.dataSet.getTable(1);
                var BuildMatingInfo = [];
                if (table.data.items.length > 0) {
                    for (var i = 0; i < table.data.items.length; i++) {
                        var maxDate = this.invorkBcf('getPlanDate', []);
                        var record = table.data.items[i];
                        var obj = {
                            SUPPLIERID: record.get("SUPPLIERID"),
                            PERSONID: record.get("PERSONID"),
                            FROMBILLNO: record.get("FROMBILLNO"),
                            FROMROWID: record.get("FROMROWID"),
                            ORDERDATE: record.get("ORDERDATE"),
                            MATERIALID: record.get("MATERIALID"),
                            WORKNO: record.get("WORKNO"),
                            SUPPLYUSERID: record.get("SUPPLYUSERID"),
                            SUPPLYUSERNAME: record.get("SUPPLYUSERNAME"),
                            ATTRIBUTECODE: record.get("ATTRIBUTECODE"),
                            ATTRIBUTEDESC: record.get("ATTRIBUTEDESC"),
                            PURCHASEORDER: record.get("PURCHASEORDER"),
                            QUANTITY: record.get("QUANTITY")
                        };
                        for (var j = 0; j < maxDate; j++) {
                            obj["DAYNUM" + j] = record.get("DAYNUM" + j);
                        }
                        BuildMatingInfo.push(obj);
                    }

                    var mark = this.invorkBcf('SaveDataPost', [BuildMatingInfo]);
                    if (mark) {
                        var masterRow = this.dataSet.getTable(0).data.items[0];
                        var store = this.dataSet.getTable(1);
                        store.removeAll();
                        masterRow.set("SUPPLIERID", "");
                        masterRow.set("PERSONID", "");
                        this.forms[0].updateRecord(masterRow);
                        //this.forms[0].loadRecord(store);
                        Ext.Msg.alert('提示', '保存成功！');
                    }
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.fieldName == "FROMROWID") {
                var fromBillNo = e.dataInfo.dataRow.get("FROMBILLNO");
                var fromRowId = e.dataInfo.value;
                var dayNum = this.invorkBcf('GetRollPlanDate', [fromBillNo, fromRowId])
                if (dayNum > 0) {
                    e.dataInfo.dataRow.set("DAYNUM" + dayNum, 1);
                }
            }
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 0) {
                var store = this.dataSet.getTable(1);
                var masterRow = this.dataSet.getTable(0).data.items[0];
                var length = store.data.items.length;
                if (e.dataInfo.fieldName == "SUPPLIERID" || e.dataInfo.fieldName == "PERSONID") {
                    var fieldName;
                    if (e.dataInfo.fieldName == "SUPPLIERID")
                        fieldName = "供应商";
                    else
                        fieldName = "采购员";
                    if (e.dataInfo.value != e.dataInfo.oldValue && !Ext.isEmpty(e.dataInfo.oldValue)) {
                        Ext.Msg.confirm('提示', '是否确认修改' + fieldName + '？修改后现有子表将清空！', function (button) {
                            if (button == "yes") {
                                store.removeAll();
                            }
                            else if (button == "no") {
                                e.dataInfo.cancel = true;
                            }
                        }, this);
                    }
                }
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            if (Ext.isEmpty(masterRow.data["SUPPLIERID"]) || Ext.isEmpty(masterRow.data["PERSONID"])) {
                e.dataInfo.cancel = true;
                Ext.Msg.alert('提示', '供应商或采购员未选择，无法新增数据！');
            }
            break;
        case LibEventTypeEnum.AddRow:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            e.dataInfo.dataRow.set("SUPPLIERID", masterRow.data["SUPPLIERID"]);
            e.dataInfo.dataRow.set("SUPPLIERNAME", masterRow.data["SUPPLIERNAME"]);
            e.dataInfo.dataRow.set("PERSONID", masterRow.data["PERSONID"]);
            e.dataInfo.dataRow.set("PERSONNAME", masterRow.data["PERSONNAME"]);

            break;

    }
}
function fillData(returnData) {

    Ext.suspendLayouts();//关闭ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        //this.deleteAll(1);//删除当前grid的数据
        this.dataSet.getTable(1).removeAll(); //删除要加载的Grid数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var datelist = returnData;//一般是中间层返回来的数据，中间可能定义的是dictionary,在前段反序列化之后是对象
        if (datelist != undefined && datelist.length > 0) {
            for (var i = 0; i < datelist.length; i++) {
                var info = datelist[i];       //RollingPlanDate
                var newRow = this.addRow(masterRow, 1);
                newRow.set('SUPPLIERID', info.SupplierId);
                newRow.set('SUPPLIERNAME',info.SupplierName)
                newRow.set("PERSONID", info.PersonId);
                newRow.set('PERSONNAME',info.PersonName)
                newRow.set('FROMBILLNO', info.FromBillNo);
                newRow.set("FROMROWID", info.FromRowId);
                newRow.set("ORDERDATE", info.OrderDate);
                newRow.set("MATERIALID", info.MaterialId);
                newRow.set("MATERIALNAME",info.MaterialName)
                newRow.set("SUPPLYUSERID", info.SupplyUserId);
                newRow.set("SUPPLYUSERNAME", info.SupplyUserName);
                newRow.set("ATTRIBUTECODE", info.Attributecode);
                newRow.set("ATTRIBUTEDESC", info.Attributedesc);
                newRow.set("PURCHASEORDER", info.PurChaseOrder);
                newRow.set("QUANTITY", info.Quantity);
                var PlanArriveDate = info.PlanArriveDate; //dic
                for (var j = 0; j < PlanArriveDate.length; j++) {
                    var value = PlanArriveDate[j];
                    newRow.set("DAYNUM" + value, 1);
                }
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开ext布局
    }
}