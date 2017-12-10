purChaseOrderDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = purChaseOrderDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = purChaseOrderDataFuncVcl;

//proto.contactObjectId = "";
//proto.contactsObjectName = "";
proto.relationCode = "";
proto.fromType = "";
proto.taxRate = 0;
proto.standardcoilRate = 1;
proto.winId = null;
proto.fromObj = null;
//赋值方法
proto.doSetParam = function (vclObj) {
    proto.relationCode = vclObj[0];
    proto.fromType = vclObj[1];
    proto.taxRate = vclObj[2];
    proto.standardcoilRate = vclObj[3];
    proto.contactsobjectId = vclObj[4];
    proto.winId = vclObj[5].winId;
    proto.fromObj = vclObj[5];
    var masterRow = this.dataSet.getTable(0).data.items[0];
    masterRow.set("FROMTYPE", proto.fromType);
    masterRow.set("RELATIONCODE", proto.relationCode);
    this.forms[0].loadRecord(masterRow);
};

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    //表头（取数据用masterRow.get("")）
    var masterRow = this.dataSet.getTable(0).data.items[0];
    //表身（取数据用allBodyRows[i].get("")）
    var allBodyRows = this.dataSet.getTable(1).data.items;


    if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }

    if (e.libEventType == LibEventTypeEnum.Validated && e.dataInfo.tableIndex == 0) {
        if (e.dataInfo.fieldName == "FROMTYPE") {
            masterRow.set("RELATIONCODE", "");//来源单号设空
            this.forms[0].loadRecord(e.dataInfo.dataRow);

        }
    }


    //用户自定义按钮
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        //查询
        if (e.dataInfo.fieldName == "BtnSelect") {
            if (masterRow.get("RELATIONCODE")) {
                var fromtype = masterRow.get("FROMTYPE");
                var billno = masterRow.get("RELATIONCODE");
                var contactsobjectid = masterRow.get("CONTACTSOBJECTID");
                //调用中间层方法
                var returnData = this.invorkBcf("GetDetail", [billno, fromtype, contactsobjectid, proto.contactsobjectId]);
                //显示赋值
                this.fillData.call(this, returnData);
            }
            else {
                Ext.Msg.alert("提示", '请选择来源订单号');
                return;
            }
        }

        //载入
        if (e.dataInfo.fieldName == "BtnCreate") {
            //如果来源类型或来源单号改变 清空操作
            if (proto.relationCode != this.dataSet.getTable(0).data.items[0].data["RELATIONCODE"] || proto.fromType != this.dataSet.getTable(0).data.items[0].data["FROMTYPE"]) {
                proto.fromObj.dataSet.getTable(1).removeAll();
            }
            //获取datafunc表身的grid
            var grid = Ext.getCmp(this.winId + 'PURCHASEORDERDATAFUNCDETAILGrid');
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
                        frombillno: selectItems[i].data["FROMBILLNO"],
                        fromrowid: selectItems[i].data["FROMROW_ID"],
                        materialid: selectItems[i].data["MATERIALID"],
                        materialname: selectItems[i].data["MATERIALNAME"],
                        specification: selectItems[i].data["SPECIFICATION"],
                        figureno: selectItems[i].data["FIGURENO"],
                        materialspec: selectItems[i].data["MATERIALSPEC"],
                        ischeck: selectItems[i].data["ISCHECK"],
                        textureid: selectItems[i].data["TEXTUREID"],
                        qualityrequire: selectItems[i].data["QUALITYREQUIRE"],
                        materialtypeid: selectItems[i].data["MATERIALTYPEID"],
                        materialtypename: selectItems[i].data["MATERIALTYPENAME"],
                        attributeid: selectItems[i].data["ATTRIBUTEID"],
                        attributename: selectItems[i].data["ATTRIBUTENAME"],
                        attributecode: selectItems[i].data["ATTRIBUTECODE"],
                        attributedesc: selectItems[i].data["ATTRIBUTEDESC"],
                        dealsunitid: selectItems[i].data["DEALSUNITID"],
                        dealsunitname: selectItems[i].data["DEALSUNITNAME"],
                        dealsunitno: selectItems[i].data["DEALSUNITNO"],
                        dealsquantity: selectItems[i].data["DEALSQUANTITY"],
                        quantity: selectItems[i].data["QUANTITY"],
                        unitid: selectItems[i].data["UNITID"],
                        unitname: selectItems[i].data["UNITNAME"],
                        price: selectItems[i].data["PRICE"],
                        amount: selectItems[i].data["AMOUNT"],
                        fromtype: selectItems[i].data["FROMTYPE"],
                        preparedate: selectItems[i].data["PREPAREDATE"],
                        mtono: selectItems[i].data["MTONO"]

                    });
                }
            }
            if (records.length == 0) {
                Ext.Msg.alert("系统提示", "请选择载入的明细！");
                return;
            }
                //执行载入
            else {

                //来源单号
                var relationcode = this.dataSet.getTable(0).data.items[0].data["RELATIONCODE"];
                if (relationcode) {
                    proto.fromObj.dataSet.getTable(0).data.items[0].set("RELATIONCODE", relationcode);
                    var field = Ext.getCmp('RELATIONCODE0_' + proto.winId);
                    field.store.add({ Id: relationcode, Name: '' });
                    field.select(relationcode);
                }
                Ext.getCmp('FROMTYPE0_' + proto.winId).select(parseFloat(this.dataSet.getTable(0).data.items[0].data["FROMTYPE"]) + 1);
                proto.fromObj.forms[0].updateRecord(proto.fromObj.dataSet.getTable(0).data.items[0]);
                //调用fillReturnData方法
                this.fillReturnData.call(this, records);
                this.win.close();
            }
        }
        if (e.dataInfo.fieldName == "BtnChoseAll") {
            //获取datafunc表身的grid
            var grid = Ext.getCmp(this.winId + 'PURCHASEORDERDATAFUNCDETAILGrid');
            //表身行项
            var selectItems = this.dataSet.getTable(1).data.items;
            for (var i = 0; i < selectItems.length; i++) {
                if (selectItems[i].data["ISCHOSE"] == false) {
                    selectItems[i].set("ISCHOSE", true);
                }
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
        //获取采购订单单datafunc的grid
        var grid = Ext.getCmp(this.winId + 'PURCHASEORDERDATAFUNCDETAILGrid');
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                //为grid添加行
                //采购询价单datatfunc中的所有字段
                var newRow = this.addRowForGrid(grid);

                newRow.set("FROMBILLNO", info.frombillno);
                newRow.set("FROMROW_ID", info.fromrowid);
                newRow.set("FROMTYPE", info.fromtype);
                newRow.set("MATERIALID", info.materialid);
                newRow.set("MATERIALNAME", info.materialname);
                newRow.set("MATERIALSPEC", info.materialspec);
                newRow.set("ATTRIBUTEID", info.attributeid);
                newRow.set("ATTRIBUTENAME", info.attributename);
                newRow.set("ATTRIBUTECODE", info.attributecode);
                newRow.set("ATTRIBUTEDESC", info.attributedesc);
                newRow.set("CONTACTSOBJECTID", info.contactsobjectid);
                newRow.set("CONTACTSOBJECTNAME", info.contactsobjectname);
                newRow.set("UNITID", info.unitid);
                newRow.set("UNITNAME", info.unitname);
                newRow.set("DEALSUNITID", info.dealsunitid);
                newRow.set("DEALSUNITNO", info.dealsunitno);
                newRow.set("ISCHECK", info.ischeck);
                newRow.set("PREPAREDATE", info.preparedate);
                newRow.set("MATERIALTYPEID", info.materialtypeid);
                newRow.set("MATERIALTYPENAME", info.materialtypename);
                newRow.set("DEALSUNITNAME", info.dealsunitname);
                newRow.set("TEXTUREID", info.textureid);
                newRow.set("SPECIFICATION", info.specification);
                newRow.set("FIGURENO", info.figureno);
                newRow.set("DEALSQUANTITY", info.dealsquantity);
                newRow.set("QUANTITY", info.quantity);
                newRow.set("PRICE", info.price);
                newRow.set("AMOUNT", info.amount);
                newRow.set("HASDEALSUQNTITY", info.hasdealsuqntity);
                newRow.set("HASQUANTITY", info.hasquantity);
                newRow.set("MTONO", info.mtono);
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
    var curStore = proto.fromObj.dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    curStore.suspendEvents();//关闭store事件
    try {

        var grid = Ext.getCmp(proto.winId + 'PURCHASEORDERDETAILGrid');
        var table = proto.fromObj.dataSet.getTable(0);
        var billno = table.data.items[0].data["BILLNO"];
        var countRow = proto.fromObj.dataSet.getTable(1).data.length;


        for (var i = 0; i < records.length; i++) {
            var info = records[i];
            if (checkGetChaseOrder(grid, info)) {
                continue;
            }
            var newRow = proto.fromObj.addRowForGrid(grid);
            newRow.set("BILLNO", billno);
            newRow.set("FROMBILLNO", info.frombillno);
            newRow.set("FROMROW_ID", info.fromrowid);
            newRow.set("MATERIALID", info.materialid);
            newRow.set("MATERIALNAME", info.materialname);
            newRow.set("MATERIALSPEC", info.materialspec);
            newRow.set("ATTRIBUTEID", info.attributeid);
            newRow.set("ATTRIBUTENAME", info.attributename);
            newRow.set("ATTRIBUTECODE", info.attributecode);
            newRow.set("ATTRIBUTEDESC", info.attributedesc);
            newRow.set("UNITID", info.unitid);
            newRow.set("UNITNAME", info.unitname);
            newRow.set("DEALSUNITID", info.dealsunitid);
            newRow.set("DEALSUNITNO", info.dealsunitno);
            newRow.set("ISCHECK", info.ischeck);
            newRow.set("PREPAREDATE", info.preparedate);
            newRow.set("MATERIALTYPEID", info.materialtypeid);
            newRow.set("MATERIALTYPENAME", info.materialtypename);
            newRow.set("DEALSUNITNAME", info.dealsunitname);
            newRow.set("TEXTUREID", info.textureid);
            newRow.set("SPECIFICATION", info.specification);
            newRow.set("FIGURENO", info.figureno);
            newRow.set("DEALSQUANTITY", info.dealsquantity);
            newRow.set("QUANTITY", info.quantity);
            newRow.set("PRICE", info.price);
            newRow.set("AMOUNT", info.amount);
            newRow.set("MTONO", info.mtono);
            if (proto.taxRate > 0) {
                var taxes = parseFloat(info.amount) * parseFloat(proto.taxRate);
                var taxamount = parseFloat(info.amount) + parseFloat(taxes);
                var taxprice = info.dealsquantity == 0 ? 0 : parseFloat(taxamount) / parseFloat(info.dealsquantity);
                var bwtaxamount = parseFloat(taxamount) * parseFloat(proto.standardcoilRate);
                var bwtaxes = parseFloat(taxes) * parseFloat(proto.standardcoilRate);

                newRow.set("TAXRATE", proto.taxRate);
                newRow.set("TAXES", taxes);
                newRow.set("TAXAMOUNT", taxamount);
                newRow.set("TAXPRICE", taxprice);
                newRow.set("BWTAXAMOUNT", bwtaxamount);
                newRow.set("BWTAXES", bwtaxes);
            }
            else {
                newRow.set("TAXAMOUNT", info.amount);
                newRow.set("TAXPRICE", info.price);
            }
            newRow.set("BWAMOUNT", parseFloat(info.amount) * parseFloat(proto.standardcoilRate));
            newRow.set("FROMTYPE", parseFloat(info.fromtype) + 1);
        }


        var grid = Ext.getCmp(proto.winId + 'PURCHASEORDERDETAILGrid');
        var items = grid.store.data.items;
        //循环统计 
        //var items = this.dataSet.getTable(1).data.items;
        var items = proto.fromObj.dataSet.getTable(1).data.items;
        var dealsquantity = 0;
        var quantity = 0;
        var amount = 0;
        var taxamount = 0;
        var taxes = 0;
        var bwamount = 0;
        var bwtaxamount = 0;
        var bwtaxes = 0;
        for (var i = 0; i < items.length; i++) {
            var floatDealsQuantity = items[i].data["DEALSQUANTITY"];
            var floatQuantity = items[i].data["QUANTITY"];
            var floatAmount = items[i].data["AMOUNT"];
            var floatTaxAmount = items[i].data["TAXAMOUNT"];
            var floatTaxes = items[i].data["TAXES"];
            var floatBwAmount = items[i].data["BWAMOUNT"];
            var floatBwTaxAmount = items[i].data["BWTAXAMOUNT"];
            var floatBwTaxes = items[i].data["BWTAXES"];
            dealsquantity += parseFloat(floatDealsQuantity);
            quantity += parseFloat(floatQuantity);
            amount += parseFloat(floatAmount);
            taxamount += parseFloat(floatTaxAmount);
            taxes += parseFloat(floatTaxes);
            bwamount += parseFloat(floatBwAmount);
            bwtaxamount += parseFloat(floatBwTaxAmount);
            bwtaxes += parseFloat(floatBwTaxes);
        }
        Ext.getCmp("ALLDEALSQUANTITYS0_" + proto.winId).setValue(dealsquantity);
        Ext.getCmp("ALLQUANTITYS0_" + proto.winId).setValue(quantity);
        Ext.getCmp("ALLAMOUNTS0_" + proto.winId).setValue(amount);
        Ext.getCmp("ALLTAXAMOUNTS0_" + proto.winId).setValue(taxamount);
        Ext.getCmp("ALLTAXES0_" + proto.winId).setValue(taxes);
        Ext.getCmp("ALLBWAMOUNTS0_" + proto.winId).setValue(bwamount);
        Ext.getCmp("ALLBWTAXAMOUNTS0_" + proto.winId).setValue(bwtaxamount);
        Ext.getCmp("ALLBWTAXES0_" + proto.winId).setValue(bwtaxes);
        proto.fromObj.forms[0].updateRecord(proto.fromObj.dataSet.getTable(0).data.items[0]);

    }
    finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

//判断采购订单明细中是否已经存在当前返填写过去的物料
function checkGetChaseOrder(grid, info) {
    var k = 0;
    var records = grid.store.data.items;
    for (var i = 0; i < records.length; i++) {
        if (records[i].get('FROMBILLNO') == info.frombillno && records[i].get('FROMROW_ID') == info.fromrowid && records[i].get('MATERIALID') == info.materialid) {
            k = 1;
        }
    }
    if (k == 1) {
        return true;
    }
    else {
        return false
    }
}

