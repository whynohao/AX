purInquirerSheetDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = purInquirerSheetDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = purInquirerSheetDataFuncVcl;

proto.winId = null;
proto.fromObj = null;
var inquirerSheetData = [];
//用于给采购询价单子子表的赋值的存储数据
var subSubtable = [];
//赋值方法
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj;
    var tableInquirerSheet = proto.fromObj[0].dataSet.getTable(0);
    var tableInquirerSheetDetail = proto.fromObj[0].dataSet.getTable(1);
    var tableInquirerSheetDetailSub = proto.fromObj[0].dataSet.getTable(2);

    //定义一个数组
    inquirerSheetData = [];
    //表头销售询价单号
    var salInquirerSheetNo = tableInquirerSheet.data.items[0].data["SALEINQUIRYSHEETNO"];
    //如果表头销售询价单号为空
    if (!salInquirerSheetNo) {
        return;
    }
    for (var i = 0; i < tableInquirerSheetDetail.data.items.length; i++) {
        var tableInquirerSheetDetailValue = tableInquirerSheetDetail.data.items[i];
        //定义一个数组
        var inquirySheetDataDetail = [];
        for (j = 0; j < tableInquirerSheetDetailSub.data.items.length; j++) {
            var tableInquirerSheetDetailSubValue = tableInquirerSheetDetailSub.data.items[j];
            if (tableInquirerSheetDetailValue.data["ROW_ID"] == tableInquirerSheetDetailSubValue.data["PARENTROWID"]) {
                //向数组inquirySheetDataDetail中push一个对象（在{}中）
                //采购询价单子子表的所有字段
                inquirySheetDataDetail.push({
                    //属性:值
                    Billno: tableInquirerSheetDetailSubValue.data["BILLNO"],
                    RowId: tableInquirerSheetDetailSubValue.data["ROW_ID"],
                    RowNo: tableInquirerSheetDetailSubValue.data["ROWNO"],
                    ParentRow: tableInquirerSheetDetailSubValue.data["PARENTROWID"],
                    InquiryDate: tableInquirerSheetDetailSubValue.data["INQUIRYDATE"],
                    ContactsObjectId: tableInquirerSheetDetailSubValue.data["CONTACTSOBJECID"],
                    ContactsObjectName: tableInquirerSheetDetailSubValue.data["CONTACTSOBJECNAME"],
                    PaymentTypeId: tableInquirerSheetDetailSubValue.data["PAYMENTTYPEID"],
                    PaymentTypeName: tableInquirerSheetDetailSubValue.data["PAYMENTTYPENAME"],
                    InvoiceTypeId: tableInquirerSheetDetailSubValue.data["INVOICETYPEID"],
                    InvoiceTypeName: tableInquirerSheetDetailSubValue.data["INVOICETYPENAME"],
                    TaxRate: tableInquirerSheetDetailSubValue.data["TAXRATE"],
                    Price: tableInquirerSheetDetailSubValue.data["PRICE"],
                    TaxPrice: tableInquirerSheetDetailSubValue.data["TAXPRICE"],
                    TaxAmount: tableInquirerSheetDetailSubValue.data["TAXAMOUNT"],
                    Taxes: tableInquirerSheetDetailSubValue.data["TAXES"],
                    IsSelected: tableInquirerSheetDetailSubValue.data["ISSELECTED"],
                })
            }
        }
        //向数组saleOrderData中push一个对象（在{}中）
        inquirerSheetData.push({
            //采购询价单表头——销售询价单号
            Billno: salInquirerSheetNo,
            //采购询价单表身——来源销售询价单行标识
            FromRowId: tableInquirerSheetDetailValue.data["FROMROWID"],
            //采购询价单子子表
            InquirySheetDataDetailList: inquirySheetDataDetail
        });
    }
};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);

    //表头（取数据用masterRow.get("")）
    var masterRow = this.dataSet.getTable(0).data.items[0];
    //表身（取数据用allBodyRows[i].get("")）
    var allBodyRows = this.dataSet.getTable(1).data.items;
    //子子表（取数据用subRows[i].get("")）
    var subRows = this.dataSet.getTable(2).data.items;

    //“表头”
    if (e.dataInfo && e.dataInfo.tableIndex == 0) {

    }
    //“表身”
    if (e.dataInfo && e.dataInfo.tableIndex == 1) {
        //Validating
        if (e.libEventType == LibEventTypeEnum.Validating) {

        }
        //BeforeAddRow
        if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
            //表身不可手工新增
            e.dataInfo.cancel = true;
        }
        if (e.libEventType == LibEventTypeEnum.ColumnDbClick) {
            //先清空datafunc中的子子表（关闭再去双击另一行的子子表的时候，把之前的内容清空，否则会使之前浏览的子子表数据还在上面显示）
            this.dataSet.getTable(2).removeAll();
            //采购询价单datafunc的当前双击行的rowid
            var rowId = e.dataInfo.dataRow.get("FROMROWID");
            var n = 1;
            //对datafunc子子表的赋值
            for (var j = 0; j < subSubtable.length; j++) {
                if (rowId == subSubtable[j].ParentRowId) {
                    var newRowSub = this.addRow(masterRow, 2);
                    newRowSub.set("BILLNO", subSubtable[j].Billno);
                    newRowSub.set("PARENTROWID", subSubtable[j].ParentRowId);
                    newRowSub.set("CONTACTSOBJECTID", subSubtable[j].ContactsObjectId);
                    newRowSub.set("CONTACTSOBJECTNAME", subSubtable[j].ContactsObjectName);
                    newRowSub.set("PRICE", subSubtable[j].Price);
                    newRowSub.set("SCHEDULEDATE", subSubtable[j].ScheduleDate);
                    //行号和行标识要set，不然每次双击都会使行标识和行号+1
                    newRowSub.set("ROW_ID", n);
                    newRowSub.set("ROWNO", n);
                    n++;
                }
            }
        }
    }
    //“子子表”
    if (e.dataInfo && e.dataInfo.tableIndex == 2) {
        //BeforeAddRow
        if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
            //表身不可手工新增
            e.dataInfo.cancel = true;
        }
    }
    //点击按钮事件
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        //“查询”按钮
        if (e.dataInfo.fieldName == "BtnSelectSaleInquiry") {
            //如果表头有销售询价单号
            if (masterRow.get("SALEINQUIRYSHEETNO")) {
                var saleInquirySheetNo = masterRow.get("SALEINQUIRYSHEETNO")
                //调用中间层方法
                var returnData = this.invorkBcf("SelectSaleInquirySheet", [saleInquirySheetNo, inquirerSheetData]);
                //显示赋值
                this.fillData.call(this, returnData);
            }
            else {
                Ext.Msg.alert("提示", '请选择销售询价单号');
                return;
            }
        }
        //“载回采购询价单”按钮
        if (e.dataInfo.fieldName == "BtnCreatePurInquirySheet") {
            //如果销售询价单变了（清空）
            if (proto.fromObj[0].dataSet.getTable(0).data.items[0].data["SALEINQUIRYSHEETNO"] != this.dataSet.getTable(0).data.items[0].data["SALEINQUIRYSHEETNO"]) {
                proto.fromObj[0].dataSet.getTable(1).removeAll();
                proto.fromObj[0].dataSet.getTable(2).removeAll();
            }
            //获取datafunc表身的grid
            var grid = Ext.getCmp(this.winId + 'PURINQUIRERSHEETDATAFUNCDETAILGrid');
            //表身行项
            var selectItems = this.dataSet.getTable(1).data.items;
            //数组，用于存储数据
            var records = [];
            //循环所有行项
            for (var i = 0; i < selectItems.length; i++) {
                //如果打勾
                if (selectItems[i].data["ISCHOSE"] == true) {
                    //将行项对象加入数组
                    records.push({
                        FromBillNo: selectItems[i].data["FROMBILLNO"],
                        FromRowId: selectItems[i].data["FROMROWID"],
                        MaterialId: selectItems[i].data["MATERIALID"],
                        MaterialName: selectItems[i].data["MATERIALNAME"],
                        MaterialSpec: selectItems[i].data["MATERIALSPEC"],
                        Specification: selectItems[i].data["SPECIFICATION"],
                        TextureId: selectItems[i].data["TEXTUREID"],
                        FigureNo: selectItems[i].data["FIGURENO"],
                        DealsQuantity: selectItems[i].data["QUANTITY"],
                        Price: selectItems[i].data["AMOUNT"],
                        Schedule: selectItems[i].data["SCHEDULE"],
                        Remark: selectItems[i].data["REMARK"],
                        //InquirerSheetDataDetailList: 
                    })
                }
            }
            //datatfunc中未选择任何行就点击生成询价单则提示
            if (records.length == 0) {
                Ext.Msg.alert("系统提示", "请选择载入的明细！");
                return;
            }
            //否则执行生成询价单
            else {
                //将采购询价单datafunc的销售询价单和商务合同号带回采购询价单表头
                //获取到销售询价单的值
                var returnSaleInquirerSheetNo = this.dataSet.getTable(0).data.items[0].data["SALEINQUIRYSHEETNO"];
                //获取到商务合同号的值
                var returnContractNo = this.dataSet.getTable(0).data.items[0].data["CONTRACTNO"];
                /*----------------------------
                //Ext.getCmp()
                var ctr1S = Ext.getCmp("SALEINQUIRYSHEETNO0_" + proto.winId);
                var ctrlC = Ext.getCmp("CONTRACTNO0_" + proto.winId);
                ------------------------------*/
                //如果datafunc中表头销售询价单和商务合同号不为空
                if (returnSaleInquirerSheetNo) {
                    /*------------------------------
                    //store中添加Id 和 Name
                    ctr1.store.add({ Id: returnContactsObj, Name: this.dataSet.getTable(0).data.items[0].data["SUPPLIERNAME"] });
                    //选中Id，相当于选择一遍，带出名称
                    ctr1.select(returnContactsObj);
                    --------------------------------*/
                    //把值set到表中，为了使主表能取到值
                    //1、带回商务合同号
                    Ext.getCmp('CONTRACTNO0_' + proto.winId).setValue(returnContractNo);
                    //2、带回销售询价单号（有引用的字段都需要用store来赋值显示，因为它是ComboBox类型的框）
                    var field = Ext.getCmp('SALEINQUIRYSHEETNO0_' + proto.winId);
                    //store
                    field.store.add({ Id: returnSaleInquirerSheetNo, Name: '' });
                    //选中Id，相当于选择一遍，带出名称
                    field.select(returnSaleInquirerSheetNo);
                }
                //（自改）传入默认税率参数 raxRate
                this.fillReturnData.call(this, records);
                //删除原来的销售询价单行项

                this.win.close();
            }
        }
    }
}
//查询数据，填充本DataFunc的GRID数据
proto.fillData = function (returnData) {
    //清空
    subSubtable = [];
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    curStore.suspendEvents();//关闭store事件
    try {
        //删除当前grid的数据
        this.deleteAll(1);
        //获取采购询价单datafunc的grid
        var grid = Ext.getCmp(this.winId + 'PURINQUIRERSHEETDATAFUNCDETAILGrid');
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                //为grid添加行
                //采购询价单datatfunc中的所有字段
                var newRow = this.addRowForGrid(grid);
                newRow.set("FROMBILLNO", info.Billno);
                newRow.set("FROMROWID", info.RowId);
                newRow.set("MATERIALID", info.MaterialId);
                newRow.set("MATERIALNAME", info.MaterialName);
                newRow.set("MATERIALSPEC", info.MaterialSpec);
                newRow.set("SPECIFICATION", info.Specification);
                newRow.set("TEXTUREID", info.TextureId);
                newRow.set("FIGURENO", info.FigureNo);
                newRow.set("QUANTITY", info.DealsQuantity);
                newRow.set("AMOUNT", info.Price);
                newRow.set("REMARK", info.Remark);
                //采购询价单datatfunc点击查询的时候的子子表赋值
                //采购询价单datafunc子子表
                var detailList = info.InquirerSheetSubModelList;
                //采购询价单datafunc子子表的List有内容
                if (detailList.length > 0) {
                    for (var j = 0; j < detailList.length; j++) {
                        //把数据push到subSubtable数组中
                        subSubtable.push(detailList[j]);
                    }
                    //打勾
                    newRow.set("SCHEDULE", true);
                }
                else {
                    //不打勾
                    newRow.set("SCHEDULE", false);
                }
            }
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }

}
//返回值赋值
//将选中的行记录数据填回明细中
proto.fillReturnData = function (records) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = proto.fromObj[0].dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        //proto.fromObj[0].deleteAll(1);
        //proto.fromObj[0].deleteAll(2);
        var grid = Ext.getCmp(proto.winId + 'PURINQUIRERSHEETDETAILGrid');
        var table = proto.fromObj[0].dataSet.getTable(0);
        var billno = table.data.items[0].data["BILLNO"];
        //采购询价单子表赋值
        //原采购询价单子表的行数
        var countRow = proto.fromObj[0].dataSet.getTable(1).data.length;
        for (var i = 0; i < records.length; i++) {
            //records的行赋给info
            var info = records[i];

            var newRow = proto.fromObj[0].addRowForGrid(grid);
            newRow.set("BILLNO", billno);
            newRow.set("PREPAREDATE", proto.fromObj[0].dataSet.getTable(0).data.items[0].data["DEMANDDATE"])
            newRow.set("FROMROWID", info.FromRowId);
            newRow.set("MATERIALID", info.MaterialId);
            newRow.set("MATERIALNAME", info.MaterialName);
            newRow.set("MATERIALSPEC", info.MaterialSpec);
            newRow.set("SPECIFICATION", info.Specification);
            newRow.set("TEXTUREID", info.TextureId);
            newRow.set("FIGURENO", info.FigureNo);
            newRow.set("DEALSQUANTITY", info.DealsQuantity);
            newRow.set("PRICE", info.Price);
            
            newRow.set("REMARK", info.Remark);
            //查询图号、标识和基本单位（返回类型是字典Dictionary<string, string>）
            var searchResult = this.invorkBcf("GetFigureNo", [info.MaterialId]);
            newRow.set("FIGURENO", searchResult["figureNo"]);
            newRow.set("TEXTUREID", searchResult["textureId"]);
            newRow.set("MATERIALSPEC", searchResult["materialSpec"]);

            var returnUnit = this.invorkBcf('GetUnitJson', [info.MaterialId]);
            var list = returnUnit;
            var dealsUnitIdT;
            var dealsUnitNoT;
            if (list != undefined && list.length > 0) {
                var infoUnit = list[0];
                newRow.set("DEALSUNITID", infoUnit.UNITID);
                newRow.set("DEALSUNITNO", infoUnit.UNITNO);
                newRow.set("DEALSUNITNAME", infoUnit.UNITNAME);
                dealsUnitIdT = infoUnit.UNITID;
                dealsUnitNoT = infoUnit.UNITNO;
            }

            newRow.set("UNITID", searchResult["unitId"]);
            newRow.set("UNITNAME", searchResult["unitName"]);
            newRow.set("ISCHECK", searchResult["needCheck"]);
            //计算
            newRow.set("AMOUNT", info.DealsQuantity * info.Price);
            newRow.set("TAXPRICE", info.Price * (1 + proto.fromObj[0].dataSet.getTable(1).data.items[i + countRow].data["TAXRATE"]));
            newRow.set("TAXAMOUNT", info.DealsQuantity * info.Price * (1 + proto.fromObj[0].dataSet.getTable(1).data.items[i + countRow].data["TAXRATE"]));
            newRow.set("TAXES", info.DealsQuantity * info.Price * proto.fromObj[0].dataSet.getTable(1).data.items[i + countRow].data["TAXRATE"]);
            
            var unitData = this.invorkBcf("GetData", [info.MaterialId, dealsUnitIdT, dealsUnitNoT, 0, info.DealsQuantity, searchResult["unitId"], 0]);
            //交易单位变更引发基本数量变化
            newRow.set("QUANTITY", unitData.Quantity);
            //如果交易单位 == 基本单位，基本数量变为交易数量
            if (dealsUnitIdT && dealsUnitIdT == searchResult["unitId"] && dealsUnitNoT == "") {
                newRow.set("QUANTITY", info.DealsQuantity)
            }

            ////采购询价单子子表赋值
            var n = 1;
            for (var j = 0; j < subSubtable.length; j++) {
                var infoDetail = subSubtable[j];
                if (info.FromRowId == infoDetail.ParentRowId) {
                    var subRow = proto.fromObj[0].addRow(newRow, 2);
                    subRow.set("BILLNO", billno);
                    subRow.set("ROW_ID", n);
                    subRow.set("ROWNO", n);
                    subRow.set("PRICE", infoDetail.Price);
                    //subRow.set("INQUIRYDATE", infoDetail.);
                    subRow.set("CONTACTSOBJECTID", infoDetail.ContactsObjectId);
                    subRow.set("CONTACTSOBJECTNAME", infoDetail.ContactsObjectName);
                    subRow.set("PAYMENTTYPEID", infoDetail.PaymentTypeId);
                    subRow.set("PAYMENTTYPENAME", infoDetail.PaymentTypeName);
                    subRow.set("INVOICETYPEID", infoDetail.InvoiceTypeId);
                    subRow.set("INVOICETYPENAME", infoDetail.InvoiceTypeName);
                    subRow.set("TAXRATE", infoDetail.TaxRate);
                    //行号和行标识
                    n++;

                    newRow.set("INQUIRYDETAIL", true);
                }
            }
        }
        proto.fromObj[0].forms[0].updateRecord(table.data.items[0]);

    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}