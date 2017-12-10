stkDeliveryNoteDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkDeliveryNoteDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkDeliveryNoteDataFuncVcl;

proto.winId = null;
proto.fromObj = null;
//赋值方法
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj;
    proto.contactsObjectid = vclObj[2];
    proto.contactsObjectname = vclObj[3];
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set("CONTACTSOBJECTID", proto.contactsObjectid);
    masterRow.set("CONTACTSOBJECTNAME", proto.contactsObjectname);
    this.forms[0].loadRecord(masterRow);
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    //表头（取数据用masterRow.get("")）
    var masterRow = this.dataSet.getTable(0).data.items[0];
    //表身（取数据用allBodyRows[i].get("")）
    var allBodyRows = this.dataSet.getTable(1).data.items;

    if (e.dataInfo && e.dataInfo.tableIndex == 1) {
        //新增行
        //表身不可手工新增
        if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
            e.dataInfo.cancel = true;
            return;
        }
        //删除行
        //表身不可手工删除
        if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
            e.dataInfo.cancel = true;
            return;
        }
    }
    //用户自定义按钮
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        //全选
        if (e.dataInfo.fieldName == "BtnSelectAll") {
            for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                this.dataSet.getTable(1).data.items[i].set("ISCHOSE", 1);
            }
        }
        //全反选
        if (e.dataInfo.fieldName == "BtnSelectNone") {
            for (var i = 0; i < this.dataSet.getTable(1).data.items.length; i++) {
                this.dataSet.getTable(1).data.items[i].set("ISCHOSE", 0);
            }
        }
        //查询
        if (e.dataInfo.fieldName == "BtnSelectChaseOrder") {
            if (masterRow.get("RELATIONCODE")) {
                //datafunc表头来源订单
                var purchaseOrderNo = masterRow.get("RELATIONCODE");
                //datafunc表头供应商ID
                var contactsObjectId = masterRow.get("CONTACTSOBJECTID");
                //通知单表头来源订单
                var relationCode = proto.fromObj[0].dataSet.getTable(0).data.items[0].data["RELATIONCODE"];
                //采购通知单子表的来源行标识按行放入数组中
                var purchaseOrderDetail = [];
                for (var i = 0; i < proto.fromObj[0].dataSet.getTable(1).data.items.length; i++) {
                    purchaseOrderDetail.push({
                        FromRow_Id: proto.fromObj[0].dataSet.getTable(1).data.items[i].data["FROMROW_ID"]
                    })
                }
                //调用中间层方法
                var returnData = this.invorkBcf("SelectPurchaseOrder", [purchaseOrderNo, relationCode, purchaseOrderDetail, contactsObjectId]);
                //显示赋值
                this.fillData.call(this, returnData);
            }
            else {
                Ext.Msg.alert("提示", '请输入来源订单号');
                return;
            }
        }
        //生成采购入库通知单
        if (e.dataInfo.fieldName == "BtnCreatePurChaseNotice") {
            //如果销售询价单变了（清空）
            if (proto.fromObj[0].dataSet.getTable(0).data.items[0].data["RELATIONCODE"] != this.dataSet.getTable(0).data.items[0].data["RELATIONCODE"]) {
                proto.fromObj[0].dataSet.getTable(1).removeAll();
            }
            //获取datafunc表身的grid
            var grid = Ext.getCmp(this.winId + 'STKDELIVERYNOTEDATAFUNCDETAILGrid');
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
                        RelationCode: selectItems[i].data["RELATIONCODE"],
                        FromRow_Id: selectItems[i].data["FROMROW_ID"],
                        MaterialId: selectItems[i].data["MATERIALID"],
                        MaterialName: selectItems[i].data["MATERIALNAME"],
                        Specification: selectItems[i].data["SPECIFICATION"],
                        FigureNo: selectItems[i].data["FIGURENO"],
                        MaterialSpec: selectItems[i].data["MATERIALSPEC"],
                        IsCheck: selectItems[i].data["ISCHECK"],
                        TextureId: selectItems[i].data["TEXTUREID"],
                        QualityRequire: selectItems[i].data["QUALITYREQUIRE"],
                        MaterialTypeId: selectItems[i].data["MATERIALTYPEID"],
                        MaterialTypeName: selectItems[i].data["MATERIALTYPENAME"],
                        AttributeId: selectItems[i].data["ATTRIBUTEID"],
                        AttributeName: selectItems[i].data["ATTRIBUTENAME"],
                        AttributeCode: selectItems[i].data["ATTRIBUTECODE"],
                        AttributeDesc: selectItems[i].data["ATTRIBUTEDESC"],
                        PrepareDate: selectItems[i].data["PREPAREDATE"],
                        ReceiveQty: selectItems[i].data["RECEIVEQTY"],
                        DealsUnitId: selectItems[i].data["DEALSUNITID"],
                        DealsUnitName: selectItems[i].data["DEALSUNITNAME"],
                        DealsUnitNo: selectItems[i].data["DEALSUNITNO"],
                        Quantity: selectItems[i].data["QUANTITY"],
                        UnitId: selectItems[i].data["UNITID"],
                        UnitName: selectItems[i].data["UNITNAME"],
                        Price: selectItems[i].data["PRICE"],
                        Amount: selectItems[i].data["AMOUNT"],
                        TaxRate: selectItems[i].data["TAXRATE"],
                        TaxPrice: selectItems[i].data["TAXPRICE"],
                        TaxAmount: selectItems[i].data["TAXAMOUNT"],
                        Taxes: selectItems[i].data["TAXES"],
                        BWAmount: selectItems[i].data["BWAMOUNT"],
                        BWTaxAmount: selectItems[i].data["BWTAXAMOUNT"],
                        BWTaxes: selectItems[i].data["BWTAXES"],
                        CanDealsQty: selectItems[i].data["CANDEALSQTY"],
                        CanQty: selectItems[i].data["CANQTY"],
                        HasDealsQty: selectItems[i].data["HASDEALSQTY"],
                        HasQty: selectItems[i].data["HASQTY"],
                        MtoNo: selectItems[i].data["MTONO"],
                    });
                }
            }
            //datatfunc中未选择任何行就点击生成通知单则提示
            if (records.length == 0) {
                Ext.Msg.alert("系统提示", "请选择载入的明细！");
                return;
            }
            if (masterRow.get("RELATIONCODE") && masterRow.get("CONTACTSOBJECTID")) {
                var isEqual = this.invorkBcf("FromContactsObjectId", [masterRow.get("RELATIONCODE"), masterRow.get("CONTACTSOBJECTID")]);
                if (isEqual == false) {
                    Ext.Msg.alert("系统提示", "表头来源采购订单号与往来对象不符！");
                    return;
                }
            }
            //否则执行生成通知单
            if (records.length > 0) {
                //判断勾选行的来源单号是否相同
                for (var i = 0; i < selectItems.length-1; i++) {
                    if (selectItems[i].data["ISCHOSE"] == true){
                        if (selectItems[i].data["RELATIONCODE"] != selectItems[i+1].data["RELATIONCODE"]) {
                            Ext.Msg.alert("系统提示", "请选择相同来源订单的明细");
                            return;
                        }
                    }
                }

                //将采购入库通知单datafunc的采购订单和合同号带回采购入库通知单表头
                //获取到采购订单的值
                var returnPurChaseOrderNo = this.dataSet.getTable(0).data.items[0].data["RELATIONCODE"];
                //获取到商务合同号的值
                var returnContractNo = this.dataSet.getTable(0).data.items[0].data["CONTRACTCODE"];
                //如果datafunc中表头采购订单不为空
                if (returnPurChaseOrderNo) {

                    /*------------------------------
                    //store中添加Id 和 Name
                    ctr1.store.add({ Id: returnContactsObj, Name: this.dataSet.getTable(0).data.items[0].data["SUPPLIERNAME"] });
                    //选中Id，相当于选择一遍，带出名称
                    ctr1.select(returnContactsObj);
                    --------------------------------*/

                    //把值set到表中，为了使主表能取到值
                    proto.fromObj[0].dataSet.getTable(0).data.items[0].set("RELATIONCODE", returnPurChaseOrderNo);
                    //1、带回合同号
                    Ext.getCmp('CONTRACTCODE0_' + proto.winId).setValue(returnContractNo);
                    //2、带回采购订单号（有引用的字段都需要用store来赋值显示，因为它是ComboBox类型的框）
                    var field = Ext.getCmp('RELATIONCODE0_' + proto.winId);
                    //store
                    field.store.add({ Id: returnPurChaseOrderNo, Name: '' });
                    //选中Id，相当于选择一遍，带出名称
                    field.select(returnPurChaseOrderNo);
                    proto.fromObj[0].forms[0].updateRecord(proto.fromObj[0].dataSet.getTable(0).data.items[0]);
                }
                //采购员
                var personId = this.dataSet.getTable(0).data.items[0].data["PERSONID"];
                var personName = this.dataSet.getTable(0).data.items[0].data["PERSONNAME"];
                if (personId) {
                    proto.fromObj[0].dataSet.getTable(0).data.items[0].set("PERSONID", personId);
                    //Ext.getCmp('PERSONNAME0_' + proto.winId).setValue(personName);
                    var field = Ext.getCmp('PERSONID0_' + proto.winId);
                    field.store.add({ Id: personId, Name: personName });
                    field.select(personId);
                }
                
                //部门
                var deptId = this.dataSet.getTable(0).data.items[0].data["DEPTID"];
                var deptName = this.dataSet.getTable(0).data.items[0].data["DEPTNAME"];
                if (deptId) {
                    proto.fromObj[0].dataSet.getTable(0).data.items[0].set("DEPTID", deptId);
                    //Ext.getCmp('DEPTNAME0_' + proto.winId).setValue(deptName);
                    var field = Ext.getCmp('DEPTID0_' + proto.winId);
                    field.store.add({ Id: deptId, Name: deptName });
                    field.select(deptId);
                }
                //往来对象
                var contactsObjectId = this.dataSet.getTable(0).data.items[0].data["CONTACTSOBJECTID"];
                var contactsObjectName = this.dataSet.getTable(0).data.items[0].data["CONTACTSOBJECTNAME"];
                if (contactsObjectId) {
                    proto.fromObj[0].dataSet.getTable(0).data.items[0].set("CONTACTSOBJECTID", contactsObjectId);
                    //Ext.getCmp('CONTACTSOBJECTNAME0_' + proto.winId).setValue(contactsObjectName);
                    var field = Ext.getCmp('CONTACTSOBJECTID0_' + proto.winId);
                    field.store.add({ Id: contactsObjectId, Name: contactsObjectName });
                    field.select(contactsObjectId);
                }
                //运输方式
                var transportWayId = this.dataSet.getTable(0).data.items[0].data["TRANSPORTWAYID"];
                var transportWayName = this.dataSet.getTable(0).data.items[0].data["TRANSPORTWAYNAME"];
                if (transportWayId) {
                    proto.fromObj[0].dataSet.getTable(0).data.items[0].set("TRANSPORTWAYID", transportWayId);
                    //Ext.getCmp('TRANSPORTWAYNAME0_' + proto.winId).setValue(transportWayName);
                    var field = Ext.getCmp('TRANSPORTWAYID0_' + proto.winId);
                    field.store.add({ Id: transportWayId, Name: transportWayName });
                    field.select(transportWayId);
                }
                //币别
                var currencyId = this.dataSet.getTable(0).data.items[0].data["CURRENCYID"];
                var currencyName = this.dataSet.getTable(0).data.items[0].data["CURRENCYNAME"];
                if (currencyId) {
                    proto.fromObj[0].dataSet.getTable(0).data.items[0].set("CURRENCYID", currencyId);
                    //Ext.getCmp('CURRENCYNAME0_' + proto.winId).setValue(currencyName);
                    var field = Ext.getCmp('CURRENCYID0_' + proto.winId);
                    field.store.add({ Id: currencyId, Name: currencyName });
                    field.select(currencyId);
                }

                proto.fromObj[0].forms[0].updateRecord(proto.fromObj[0].dataSet.getTable(0).data.items[0]);
                //调用fillReturnData方法
                this.fillReturnData.call(this, records);
                this.win.close();
            }
        }
    }
}

//查询数据，填充本DataFunc的GRID数据
proto.fillData = function (returnData) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    curStore.suspendEvents();//关闭store事件
    try {
        //删除当前grid的数据
        this.deleteAll(1);
        //获取采购询价单datafunc的grid
        var grid = Ext.getCmp(this.winId + 'STKDELIVERYNOTEDATAFUNCDETAILGrid');
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            //表头赋值
            masterRow.set("PERSONID", list[0].PersonId);
            masterRow.set("PERSONNAME", list[0].PersonName);
            masterRow.set("DEPTID", list[0].DeptId);
            masterRow.set("DEPTNAME", list[0].DeptName);
            masterRow.set("CONTACTSOBJECTID", list[0].ContactsObjectId);
            masterRow.set("CONTACTSOBJECTNAME", list[0].ContactsObjectName);
            masterRow.set("TRANSPORTWAYID", list[0].TransportWayId);
            masterRow.set("TRANSPORTWAYNAME", list[0].TransportWayName);
            masterRow.set("CURRENCYID", list[0].CurrencyId);
            masterRow.set("CURRENCYNAME", list[0].CurrencyName);
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                //为grid添加行
                //采购询价单datatfunc中的所有字段
                var newRow = this.addRowForGrid(grid);
                newRow.set("RELATIONCODE", info.BillNo);
                newRow.set("FROMROW_ID", info.FromRow_Id);
                newRow.set("MTONO", info.MtoNo);
                newRow.set("MATERIALID", info.MaterialId);
                newRow.set("MATERIALNAME", info.MaterialName);
                newRow.set("SPECIFICATION", info.Specification);
                newRow.set("TEXTUREID", info.TextureId);
                newRow.set("FIGURENO", info.FigureNo);
                newRow.set("MATERIALSPEC", info.MaterialSpec);
                newRow.set("ISCHECK", info.IsCheck);
                newRow.set("UNITID", info.UnitId);
                newRow.set("UNITNAME", info.UnitName);
                newRow.set("MATERIALTYPEID", info.MaterialTypeId);
                newRow.set("MATERIALTYPENAME", info.MaterialTypeName);
                newRow.set("ATTRIBUTEID", info.AttributeId);
                newRow.set("ATTRIBUTENAME", info.AttributeName);
                newRow.set("ATTRIBUTECODE", info.AttributeCode);
                newRow.set("ATTRIBUTEDESC", info.AttributeDesc);
                newRow.set("PREPAREDATE", info.PrepareDate);
                //从采购订单过账表中获取已入库交易数量
                var listReceipt = this.invorkBcf("GetReceiptinDealsQuantity", [masterRow.get("RELATIONCODE"), info.FromRow_Id, info.MaterialId]);
                newRow.set("RECEIVEQTY", info.DealsQuantity - listReceipt[1] - listReceipt[0]);
                newRow.set("DEALSUNITID", info.DealsUnitId);
                newRow.set("DEALSUNITNAME", info.DealsUnitName);
                newRow.set("DEALSUNITNO", info.DealsUnitNo);
                newRow.set("TAXRATE", info.TaxRate);
                newRow.set("TAXPRICE", info.Price * (info.TaxRate + 1));
                newRow.set("QUALITYREQUIRE", info.QualityRequire);
                newRow.set("PRICE", info.Price);
                newRow.set("RECEPTINDEALSQUANTITY", info.ReceptInDealsQuantity);
                //获取单位换算比
                var unitRate = this.invorkBcf("GetUnitRate", [info.MaterialId, info.DealsUnitId, info.DealsUnitNo])
                newRow.set("QUANTITY", (info.DealsQuantity - listReceipt[1] - listReceipt[0]) / unitRate);
                //金额联动计算
                newRow.set("AMOUNT", info.Price * info.DealsQuantity);
                newRow.set("TAXAMOUNT", info.Price * (info.TaxRate + 1) * info.DealsQuantity);
                newRow.set("TAXES", info.TaxRate * info.Price * info.DealsQuantity);
                newRow.set("BWAMOUNT", info.Price * info.DealsQuantity * masterRow.get("STANDARDCOILRATE"));
                newRow.set("BWTAXAMOUNT", info.Price * (info.TaxRate + 1) * info.DealsQuantity * masterRow.get("STANDARDCOILRATE"));
                newRow.set("BWTAXES", info.TaxRate * info.Price * info.DealsQuantity * masterRow.get("STANDARDCOILRATE"));
                //可入库交易数、可入库基本数
                newRow.set("CANDEALSQTY", info.DealsQuantity - listReceipt[1] - listReceipt[0]);
                newRow.set("CANQTY", (info.DealsQuantity - listReceipt[1] - listReceipt[0]) / unitRate);
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
        var grid = Ext.getCmp(proto.winId + 'STKDELIVERYNOTEDETAILGrid');
        var table = proto.fromObj[0].dataSet.getTable(0);
        //入库通知单表头单据编号
        var billno = table.data.items[0].data["BILLNO"];
        //入库通知单子表赋值
        //原采购入库通知单子表的行数
        var countRow = proto.fromObj[0].dataSet.getTable(1).data.length;
        for (var i = 0; i < records.length; i++) {
            //records的行赋给info
            var info = records[i];

            var newRow = proto.fromObj[0].addRowForGrid(grid);
            newRow.set("BILLNO", billno);
            newRow.set("WAREHOUSEID", table.data.items[0].data["WAREHOUSEID"]);
            newRow.set("WAREHOUSENAME", table.data.items[0].data["WAREHOUSENAME"]);
            newRow.set("WAREHOUSEPERSONID", table.data.items[0].data["WAREHOUSEPERSONID"]);
            newRow.set("WAREHOUSEPERSONNAME", table.data.items[0].data["WAREHOUSEPERSONNAME"]);
            newRow.set("FROMBILLNO", info.RelationCode);
            newRow.set("FROMROW_ID", info.FromRow_Id);
            newRow.set("MTONO", info.MtoNo);
            newRow.set("MATERIALID", info.MaterialId);
            newRow.set("MATERIALNAME", info.MaterialName);
            newRow.set("SPECIFICATION", info.Specification);
            newRow.set("FIGURENO", info.FigureNo);
            newRow.set("MATERIALSPEC", info.MaterialSpec);
            newRow.set("ISCHECK", info.IsCheck);
            newRow.set("TEXTUREID", info.TextureId);
            newRow.set("QUALITYREQUIRE", info.QualityRequire);
            newRow.set("MATERIALTYPEID", info.MaterialId);
            newRow.set("MATERIALTYPENAME", info.MaterialTypeName);
            newRow.set("ATTRIBUTEID", info.AttributeId);
            newRow.set("ATTRIBUTENAME", info.AttributeName);
            newRow.set("ATTRIBUTECODE", info.AttributeCode);
            newRow.set("ATTRIBUTEDESC", info.AttributeDesc);
            newRow.set("PREPAREDATE", info.PrepareDate);
            newRow.set("RECEIVEQTY", info.ReceiveQty);
            newRow.set("DEALSUNITID", info.DealsUnitId);
            newRow.set("DEALSUNITNAME", info.DealsUnitName);
            newRow.set("DEALSUNITNO", info.DealsUnitNo);
            newRow.set("QUANTITY", info.Quantity);
            newRow.set("UNITID", info.UnitId);
            newRow.set("UNITNAME", info.UnitName);
            newRow.set("PRICE", info.Price);
            newRow.set("AMOUNT", info.Amount);
            newRow.set("TAXRATE", info.TaxRate);
            newRow.set("TAXPRICE", info.TaxPrice);
            newRow.set("TAXAMOUNT", info.TaxAmount);
            newRow.set("TAXES", info.Taxes);
            newRow.set("BWAMOUNT", info.BWAmount);
            newRow.set("BWTAXAMOUNT", info.BWTaxAmount);
            newRow.set("BWTAXES", info.BWTaxes);
            newRow.set("CANDEALSQTY", info.CanDealsQty);
            newRow.set("CANQTY", info.CanQty);
            newRow.set("HASDEALSQTY", info.HasDealsQty);
            newRow.set("HASQTY", info.HasQty);
        }
    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

