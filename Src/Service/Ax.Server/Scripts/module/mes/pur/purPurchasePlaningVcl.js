purPurchasePlaningVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};

var attId = 0;
var proto = purPurchasePlaningVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = purPurchasePlaningVcl;
proto.parentRow = [];
proto.subRecords = [];

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {

        case LibEventTypeEnum.FormClosed:
            if (e.dataInfo != undefined) {
                if (e.dataInfo.tableIndex == 3) {
                    var bodyTale = this.dataSet.getTable(1).data.items;
                    var totalTable = this.dataSet.getTable(2).data.items;
                    var subTable = this.dataSet.getTable(3).data.items;
                    var records = [];

                    var a = [];
                    a = this.invorkBcf("ClearSubTableList", [this.subRecords, subTable[0].data["PARENTROWID"], subTable[0].data["BILLNO"]]);
                    this.subRecords = a;

                    for (var i = 0; i < subTable.length; i++) {
                        records.push({
                            BILLNO: subTable[i].data["BILLNO"],
                            ROW_ID: subTable[i].data["ROW_ID"],
                            PARENTROWID: subTable[i].data["PARENTROWID"],
                            MATERIALID: subTable[i].data["MATERIALID"],
                            MATERIALNAME: subTable[i].data["MATERIALNAME"],
                            SPECIFICATION: subTable[i].data["SPECIFICATION"],
                            TEXTUREID: subTable[i].data["TEXTUREID"],
                            FIGURENO: subTable[i].data["FIGURENO"],
                            UNITID: subTable[i].data["UNITID"],
                            UNITNAME: subTable[i].data["UNITNAME"],
                            ATTRIBUTEID: subTable[i].data["ATTRIBUTEID"],
                            ATTRIBUTENAME: subTable[i].data["ATTRIBUTENAME"],
                            ATTRIBUTECODE: subTable[i].data["ATTRIBUTECODE"],
                            ATTRIBUTEDESC: subTable[i].data["ATTRIBUTEDESC"],
                            SUPPLIERID: subTable[i].data["SUPPLIERID"],
                            SUPPLIERNAME: subTable[i].data["SUPPLIERNAME"],
                            QUANTITY: subTable[i].data["QUANTITY"],
                            INSTOCKQTY: subTable[i].data["INSTOCKQTY"],
                            SUPPLYUSERID: subTable[i].data["SUPPLYUSERID"],
                            SUPPLYUSERNAME: subTable[i].data["SUPPLYUSERNAME"],
                            SHORTLEADTIME: subTable[i].data["SHORTLEADTIME"],
                            PLANARRIVEDATE: subTable[i].data["PLANARRIVEDATE"],
                            ISURGENT: subTable[i].data["ISURGENT"]
                        });
                        this.subRecords.push({
                            BILLNO: subTable[i].data["BILLNO"],
                            ROW_ID: subTable[i].data["ROW_ID"],
                            PARENTROWID: subTable[i].data["PARENTROWID"],
                            MATERIALID: subTable[i].data["MATERIALID"],
                            MATERIALNAME: subTable[i].data["MATERIALNAME"],
                            SPECIFICATION: subTable[i].data["SPECIFICATION"],
                            TEXTUREID: subTable[i].data["TEXTUREID"],
                            FIGURENO: subTable[i].data["FIGURENO"],
                            UNITID: subTable[i].data["UNITID"],
                            UNITNAME: subTable[i].data["UNITNAME"],
                            ATTRIBUTEID: subTable[i].data["ATTRIBUTEID"],
                            ATTRIBUTENAME: subTable[i].data["ATTRIBUTENAME"],
                            ATTRIBUTECODE: subTable[i].data["ATTRIBUTECODE"],
                            ATTRIBUTEDESC: subTable[i].data["ATTRIBUTEDESC"],
                            SUPPLIERID: subTable[i].data["SUPPLIERID"],
                            SUPPLIERNAME: subTable[i].data["SUPPLIERNAME"],
                            QUANTITY: subTable[i].data["QUANTITY"],
                            INSTOCKQTY: subTable[i].data["INSTOCKQTY"],
                            SUPPLYUSERID: subTable[i].data["SUPPLYUSERID"],
                            SUPPLYUSERNAME: subTable[i].data["SUPPLYUSERNAME"],
                            SHORTLEADTIME: subTable[i].data["SHORTLEADTIME"],
                            PLANARRIVEDATE: subTable[i].data["PLANARRIVEDATE"],
                            ISURGENT: subTable[i].data["ISURGENT"]
                        });
                    }

                    var sumList = this.invorkBcf("ChangeSubTable", [records]);
                    for (var i = 0; i < sumList.length; i++) {
                        for (var j = 0; j < bodyTale.length; j++) {
                            if (bodyTale[j].data["ROW_ID"] == sumList[i].ParentRowId)
                                bodyTale[j].set("INSTOCKQTY", sumList[i].SumQty);
                        }
                    }
                    this.fillTotalData(records, this);
                }
            }
            else
                this.invorkBcf("RemoveCache", []);
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1 || e.dataInfo.tableIndex == 2) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.AddRow:
            if (e.dataInfo.tableIndex == 3) {
                e.dataInfo.dataRow.set("MATERIALID", this.parentRow[0].materialId);
                e.dataInfo.dataRow.set("MATERIALNAME", this.parentRow[0].materialName);
                e.dataInfo.dataRow.set("SPECIFICATION", this.parentRow[0].specification);
                e.dataInfo.dataRow.set("UNITID", this.parentRow[0].unitId);
                e.dataInfo.dataRow.set("ATTRIBUTENAME", this.parentRow[0].attributeName);
                e.dataInfo.dataRow.set("TEXTUREID", this.parentRow[0].textureId);
                e.dataInfo.dataRow.set("FIGURENO", this.parentRow[0].figureNo);
                e.dataInfo.dataRow.set("ATTRIBUTECODE", this.parentRow[0].attributeCode);
                e.dataInfo.dataRow.set("ATTRIBUTEDESC", this.parentRow[0].attributeDesc);
                e.dataInfo.dataRow.set("QUANTITY", this.parentRow[0].quantity);
            }
            break;
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.tableIndex == 1 && e.dataInfo.fieldName == 'SUPPLYDETAIL') {
                var row = e.dataInfo.dataRow;
                this.parentRow = [];
                this.parentRow.push({
                    billNo: row.data["BILLNO"],
                    parentRowId: row.data["ROW_ID"],
                    materialId: row.data["MATERIALID"],
                    materialName: row.data["MATERIALNAME"],
                    specification: row.data["SPECIFICATION"],
                    unitId: row.data["UNITID"],
                    attributeName: row.data["ATTRIBUTENAME"],
                    textureId: row.data["TEXTUREID"],
                    figureNo: row.data["FIGURENO"],
                    attributeCode: row.data["ATTRIBUTECODE"],
                    attributeDesc: row.data["ATTRIBUTEDESC"],
                    quantity: row.data["QUANTITY"]
                });
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            var headTable = this.dataSet.getTable(0).data.items[0];
            //计划下推至订单
            if (e.dataInfo.fieldName == 'btnPushChaseOrder') {
                if (!this.isEdit) {
                    if (headTable.data["BILLSTATE"] == 0) {
                        var totalTable = this.dataSet.getTable(2).data.items;
                        var billNo = headTable.data["BILLNO"];
                        var message = this.invorkBcf("PushChaseOrder", [billNo]);
                        if (message = '下推成功')
                            Ext.Msg.alert("系统提示", message);
                    }

                    else
                        Ext.Msg.alert("系统提示", "一个计划只能下推一次");
                }
                else
                    Ext.Msg.alert("系统提示", "请保存后在下推订单");
            }
                //时间齐套
            else if (e.dataInfo.fieldName == 'btnDateMating') {
                var billState = headTable.get("BILLSTATE");
                if (billState == 0) {
                    //var record = {
                    //    PurPersonId: headTable.get("PERSONID"),
                    //    ProductOrderNo: '',
                    //    IsValve: headTable.get("ISVALVE"),
                    //    IsActator: headTable.get("ISACTUATOR"),
                    //    IsAttachment: this.dataSet.getTable(0).data.items[0].get("ISATTACHMENT"),
                    //    IsStandard: this.dataSet.getTable(0).data.items[0].get("ISSTANDARD"),
                    //};
                    var result = this.invorkBcf("GetMRPDetial", []);
                    this.fillData(result, 0, this);
                }
                else
                    Ext.Msg.alert("系统提示", "已下推的计划不能更改");
            }
            //加载数据
            if (e.dataInfo.fieldName == 'btnNextPage') {
                var pageNum = headTable.get("CURRENTPAGE") + 1;
                var result = this.invorkBcf("PageTurn", [pageNum]);
                this.fillData(result, 1, this);
                if (result.length > 0) {
                    headTable.set("CURRENTPAGE", pageNum);
                    this.forms[0].updateRecord(headTable);
                }
            }
                //统一分配供应商
            else if (e.dataInfo.fieldName == 'btnAllocationSpullier') {
                var supplierId = headTable.get("CONTACTSOBJECTID");
                if (supplierId != "") {
                    var list = [];
                    list.push({
                        SUPPLIERID: supplierId,
                        SUPPLIERNAME: headTable.get("CONTACTSOBJECTNAME")
                    });
                    var grid = Ext.getCmp(this.winId + 'PURPURCHASEPLANINGDETAILGrid');
                    var records = grid.getView().getSelectionModel().getSelection();
                    for (var i = 0; i < records.length; i++) {
                        var row = records[i];
                        this.parentRow = [];
                        this.parentRow.push({
                            billNo: row.data["BILLNO"],
                            parentRowId: row.data["ROW_ID"],
                            materialId: row.data["MATERIALID"],
                            materialName: row.data["MATERIALNAME"],
                            specification: row.data["SPECIFICATION"],
                            unitId: row.data["UNITID"],
                            attributeName: row.data["ATTRIBUTENAME"],
                            textureId: row.data["TEXTUREID"],
                            figureNo: row.data["FIGURENO"],
                            attributeCode: row.data["ATTRIBUTECODE"],
                            attributeDesc: row.data["ATTRIBUTEDESC"],
                            quantity: row.data["QUANTITY"]
                        });
                        this.fillSubData(row, list, 0, this);
                    }
                }
                else
                    Ext.Msg.alert("系统提示", "请先选择供应商");
            }
                //统一分配到货时间
            else if (e.dataInfo.fieldName == 'btnAllocationPlanDate') {
                var planDate = headTable.get("PLANARRIVEDATE");
                if (planDate != 0) {
                    var list = [];
                    list.push({
                        PLANARRIVEDATE: planDate
                    });
                    var grid = Ext.getCmp(this.winId + 'PURPURCHASEPLANINGDETAILGrid');
                    var records = grid.getView().getSelectionModel().getSelection();
                    for (var i = 0; i < records.length; i++) {
                        var row = records[i];
                        this.parentRow = [];
                        this.parentRow.push({
                            billNo: row.data["BILLNO"],
                            parentRowId: row.data["ROW_ID"],
                            materialId: row.data["MATERIALID"],
                            materialName: row.data["MATERIALNAME"],
                            specification: row.data["SPECIFICATION"],
                            unitId: row.data["UNITID"],
                            attributeName: row.data["ATTRIBUTENAME"],
                            textureId: row.data["TEXTUREID"],
                            figureNo: row.data["FIGURENO"],
                            attributeCode: row.data["ATTRIBUTECODE"],
                            attributeDesc: row.data["ATTRIBUTEDESC"],
                            quantity: row.data["QUANTITY"]
                        });
                        this.fillSubData(row, list, 1, this);
                    }
                }
                else
                    Ext.Msg.alert("系统提示", "请先选择预计到货时间");
            }
            break;
    }
}

proto.fillData = function (records, type) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        if (type == 0) {//如果是齐套分析则删除旧数据局
            this.dataSet.getTable(1).removeAll();//删除当前grid的数据
            this.dataSet.getTable(2).removeAll();//删除当前grid的数据
            this.dataSet.getTable(3).removeAll();//删除当前grid的数据
        }
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据   
        if (records != undefined && records.length > 0) {
            for (var i = 0; i < records.length; i++) {
                var info = records[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set("PRODUCTORDERNO", info.PRODUCTORDERNO);
                newRow.set("CONTRACTNO", info.CONTRACTNO);
                newRow.set("MATERIALID", info.MATERIALID);
                newRow.set("MATERIALNAME", info.MATERIALNAME);
                newRow.set("SPECIFICATION", info.SPECIFICATION);
                newRow.set("TEXTUREID", info.TEXTUREID);
                newRow.set("FIGURENO", info.FIGURENO);
                newRow.set("UNITID", info.UNITID);
                newRow.set("UNITNAME", info.UNITNAME);
                newRow.set("ATTRIBUTEID", info.ATTRIBUTEID);
                newRow.set("ATTRIBUTENAME", info.ATTRIBUTENAME);
                newRow.set("ATTRIBUTECODE", info.ATTRIBUTECODE);
                newRow.set("ATTRIBUTEDESC", info.ATTRIBUTEDESC);
                newRow.set("BOMTYPE", info.BOMTYPE);
                newRow.set("QUANTITY", info.QUANTITY);
                newRow.set("NEEDPURCHASEQTY", info.NEEDPURCHASEQTY);
                newRow.set("PRESTOCKQTY", info.PRESTOCKQTY);
                newRow.set("PRODUCEDATE", info.PRODUCEDATE);
                newRow.set("TOTALNEEDQTY", info.TOTALNEEDQTY);
                newRow.set("STOCKQTY", info.STOCKQTY);
                newRow.set("INWAYQTY", info.INWAYQTY);
                newRow.set("ISSTANDER", info.ISSTANDER);
                newRow.set("FACTORYNO", info.FACTORYNO);
                if (info.QUANTITY != 0 && info.NEEDPURCHASEQTY != 0)
                    newRow.set("ISNEEDPUR", 1);
                else
                    newRow.set("ISNEEDPUR", 0);
            }
        }
        //else {
        //    Ext.Msg.alert("提示", "该来源单号没有可引用行！");
        //}
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}
proto.fillTotalData = function (table) {
    var records = this.invorkBcf("GetTotoalQty", [this.subRecords, table, this.parentRow[0].parentRowId, this.parentRow[0].billNo]);
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(2);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(2).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据   
        if (records != undefined && records.length > 0) {
            for (var i = 0; i < records.length; i++) {
                var info = records[i];
                var newRow = this.addRow(masterRow, 2);
                newRow.set("MATERIALID", info.MaterialId);
                newRow.set("MATERIALNAME", info.MaterialName);
                newRow.set("SPECIFICATION", info.Specification);
                newRow.set("TEXTUREID", info.TextureId);
                newRow.set("FIGURENO", info.FigureNo);
                newRow.set("UNITID", info.UnitId);
                newRow.set("UNITNAME", info.UnitName);
                newRow.set("ATTRIBUTEID", info.AttributeId);
                newRow.set("ATTRIBUTENAME", info.AttributeName);
                newRow.set("ATTRIBUTECODE", info.AttributeCode);
                newRow.set("ATTRIBUTEDESC", info.AttributeDesc);
                newRow.set("SUPPLIERID", info.SupplierId);
                newRow.set("SUPPLYUSERNAME", info.SupplyUserName);
                newRow.set("SUPPLYUSERID", info.supplyUserId);
                newRow.set("SUPPLIERNAME", info.SupplierName);
                newRow.set("PLANARRIVEDATE", info.PlanArriveDate);
                newRow.set("QUANTITY", info.Quantity);
                newRow.set("INSTOCKQTY", info.InStockQty);
                newRow.set("ISURGENT", info.IsUrgent);
                newRow.set("SHORTLEADTIME", info.ShortLeadTime);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}
proto.fillSubData = function (record, list, type) {
    var subTable = this.dataSet.getTable(3).data.items;
    var bodyTable = this.dataSet.getTable(1).data.items;
    var no = [];
    var records = [];
    if (this.subRecords.length > 0) {
        var a = [];
        a = this.invorkBcf("ClearSubTableList", [this.subRecords, record.data["ROW_ID"], record.data["BILLNO"]]);
        this.subRecords = a;
    }

    for (var i = 0; i < subTable.length; i++) {
        if (subTable[i].data["PARENTROWID"] == record.data["ROW_ID"] && subTable[i].data["BILLNO"] == record.data["BILLNO"]) {
            no.push({
                INDEX: i
            });
        }
    }
    var supplierId;
    var supplierName;
    var planArriveDate;
    if (type == 0) {
        supplierId = list[0].SUPPLIERID;
        supplierName = list[0].SUPPLIERID;
        planArriveDate = 0;
    }
    else if (type == 1) {
        planArriveDate = list[0].PLANARRIVEDATE;
        supplierId = "";
        supplierName = "";
    }
    if (no.length > 0) {//存在子子表，则修改子子表
        for (var i = 0; i < no.length; i++) {
            var index = no[i].INDEX;
            if (type == 0) {
                subTable[index].set("SUPPLIERID", supplierId);
                subTable[index].set("SUPPLIERNAME", supplierName);
            }
            else if (type == 1)
                subTable[index].set("PLANARRIVEDATE", planArriveDate);


            records.push({
                BILLNO: subTable[index].data["BILLNO"],
                ROW_ID: subTable[index].data["ROW_ID"],
                PARENTROWID: subTable[index].data["PARENTROWID"],
                MATERIALID: subTable[index].data["MATERIALID"],
                MATERIALNAME: subTable[index].data["MATERIALNAME"],
                SPECIFICATION: subTable[index].data["SPECIFICATION"],
                TEXTUREID: subTable[index].data["TEXTUREID"],
                FIGURENO: subTable[index].data["FIGURENO"],
                UNITID: subTable[index].data["UNITID"],
                UNITNAME: subTable[index].data["UNITNAME"],
                ATTRIBUTEID: subTable[index].data["ATTRIBUTEID"],
                ATTRIBUTENAME: subTable[index].data["ATTRIBUTENAME"],
                ATTRIBUTECODE: subTable[index].data["ATTRIBUTECODE"],
                ATTRIBUTEDESC: subTable[index].data["ATTRIBUTEDESC"],
                SUPPLIERID: subTable[index].data["SUPPLIERID"],
                SUPPLIERNAME: subTable[index].data["SUPPLIERNAME"],
                QUANTITY: subTable[index].data["QUANTITY"],
                INSTOCKQTY: subTable[index].data["INSTOCKQTY"],
                SUPPLYUSERID: subTable[index].data["SUPPLYUSERID"],
                SUPPLYUSERNAME: subTable[index].data["SUPPLYUSERNAME"],
                SHORTLEADTIME: subTable[index].data["SHORTLEADTIME"],
                PLANARRIVEDATE: subTable[index].data["PLANARRIVEDATE"],
                ISURGENT: subTable[index].data["ISURGENT"]
            });
            this.subRecords.push({
                BILLNO: subTable[index].data["BILLNO"],
                ROW_ID: subTable[index].data["ROW_ID"],
                PARENTROWID: subTable[index].data["PARENTROWID"],
                MATERIALID: subTable[index].data["MATERIALID"],
                MATERIALNAME: subTable[index].data["MATERIALNAME"],
                SPECIFICATION: subTable[index].data["SPECIFICATION"],
                TEXTUREID: subTable[index].data["TEXTUREID"],
                FIGURENO: subTable[index].data["FIGURENO"],
                UNITID: subTable[index].data["UNITID"],
                UNITNAME: subTable[index].data["UNITNAME"],
                ATTRIBUTEID: subTable[index].data["ATTRIBUTEID"],
                ATTRIBUTENAME: subTable[index].data["ATTRIBUTENAME"],
                ATTRIBUTECODE: subTable[index].data["ATTRIBUTECODE"],
                ATTRIBUTEDESC: subTable[index].data["ATTRIBUTEDESC"],
                SUPPLIERID: subTable[index].data["SUPPLIERID"],
                SUPPLIERNAME: subTable[index].data["SUPPLIERNAME"],
                QUANTITY: subTable[index].data["QUANTITY"],
                INSTOCKQTY: subTable[index].data["INSTOCKQTY"],
                SUPPLYUSERID: subTable[index].data["SUPPLYUSERID"],
                SUPPLYUSERNAME: subTable[index].data["SUPPLYUSERNAME"],
                SHORTLEADTIME: subTable[index].data["SHORTLEADTIME"],
                PLANARRIVEDATE: subTable[index].data["PLANARRIVEDATE"],
                ISURGENT: subTable[index].data["ISURGENT"]
            });
        }
    }
    else {//不存在子子表，则新增子子表

        Ext.suspendLayouts();//关闭Ext布局
        var curStore = this.dataSet.getTable(3);
        curStore.suspendEvents();//关闭store事件
        try {
            var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据   

            var newRow = this.addRow(masterRow, 3);
            newRow.set("ROW_ID", 1);
            newRow.set("PARENTROWID", record.data["ROW_ID"]);
            newRow.set("MATERIALID", record.data["MATERIALID"]);
            newRow.set("MATERIALNAME", record.data["MATERIALNAME"]);
            newRow.set("SPECIFICATION", record.data["SPECIFICATION"]);
            newRow.set("TEXTUREID", record.data["TEXTUREID"]);
            newRow.set("FIGURENO", record.data["FIGURENO"]);
            newRow.set("UNITID", record.data["UNITID"]);
            newRow.set("UNITNAME", record.data["UNITNAME"]);
            newRow.set("ATTRIBUTEID", record.data["ATTRIBUTEID"]);
            newRow.set("ATTRIBUTENAME", record.data["ATTRIBUTENAME"]);
            newRow.set("ATTRIBUTECODE", record.data["ATTRIBUTECODE"]);
            newRow.set("ATTRIBUTEDESC", record.data["ATTRIBUTEDESC"]);
            newRow.set("SUPPLIERID", supplierId);
            newRow.set("SUPPLIERNAME", supplierName);
            newRow.set("PLANARRIVEDATE", planArriveDate);
            newRow.set("SUPPLYUSERID", "");
            newRow.set("SUPPLYUSERNAME", "");
            newRow.set("QUANTITY", record.data["NEEDPURCHASEQTY"]);
            newRow.set("INSTOCKQTY", record.data["NEEDPURCHASEQTY"]);
            newRow.set("ISURGENT", 0);
            newRow.set("SHORTLEADTIME", 0);

        } finally {
            curStore.resumeEvents();//打开store事件
            if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
                curStore.ownGrid.reconfigure(curStore);
            Ext.resumeLayouts(true);//打开Ext布局
        }

        records.push({
            BILLNO: masterRow.data["BILLNO"],
            ROW_ID: 1,
            PARENTROWID: record.data["ROW_ID"],
            MATERIALID: record.data["MATERIALID"],
            MATERIALNAME: record.data["MATERIALNAME"],
            SPECIFICATION: record.data["SPECIFICATION"],
            TEXTUREID: record.data["TEXTUREID"],
            FIGURENO: record.data["FIGURENO"],
            UNITID: record.data["UNITID"],
            UNITNAME: record.data["UNITNAME"],
            ATTRIBUTEID: record.data["ATTRIBUTEID"],
            ATTRIBUTENAME: record.data["ATTRIBUTENAME"],
            ATTRIBUTECODE: record.data["ATTRIBUTECODE"],
            ATTRIBUTEDESC: record.data["ATTRIBUTEDESC"],
            SUPPLIERID: supplierId,
            SUPPLIERNAME: supplierName,
            QUANTITY: record.data["NEEDPURCHASEQTY"],
            INSTOCKQTY: record.data["NEEDPURCHASEQTY"],
            SUPPLYUSERID: "",
            SUPPLYUSERNAME: "",
            SHORTLEADTIME: 0,
            PLANARRIVEDATE: planArriveDate,
            ISURGENT: 0
        });
        this.subRecords.push({
            BILLNO: masterRow.data["BILLNO"],
            ROW_ID: 1,
            PARENTROWID: record.data["ROW_ID"],
            MATERIALID: record.data["MATERIALID"],
            MATERIALNAME: record.data["MATERIALNAME"],
            SPECIFICATION: record.data["SPECIFICATION"],
            TEXTUREID: record.data["TEXTUREID"],
            FIGURENO: record.data["FIGURENO"],
            UNITID: record.data["UNITID"],
            UNITNAME: record.data["UNITNAME"],
            ATTRIBUTEID: record.data["ATTRIBUTEID"],
            ATTRIBUTENAME: record.data["ATTRIBUTENAME"],
            ATTRIBUTECODE: record.data["ATTRIBUTECODE"],
            ATTRIBUTEDESC: record.data["ATTRIBUTEDESC"],
            SUPPLIERID: supplierId,
            SUPPLIERNAME: supplierName,
            QUANTITY: record.data["NEEDPURCHASEQTY"],
            INSTOCKQTY: record.data["NEEDPURCHASEQTY"],
            SUPPLYUSERID: "",
            SUPPLYUSERNAME: "",
            SHORTLEADTIME: 0,
            PLANARRIVEDATE: planArriveDate,
            ISURGENT: 0
        });

        var sumList = this.invorkBcf("ChangeSubTable", [records]);
        for (var i = 0; i < sumList.length; i++) {
            for (var j = 0; j < bodyTable.length; j++) {
                if (bodyTable[j].data["ROW_ID"] == sumList[i].ParentRowId) {
                    bodyTable[j].set("INSTOCKQTY", sumList[i].SumQty);
                    bodyTable[j].set("SUPPLYDETAIL", 1);
                }
            }
        }
    }
    this.fillTotalData(records, this);
}