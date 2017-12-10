comTestStructBomVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
    this.list = [];
    this.saleOrderInfo;
    this.journal;
};
var proto = comTestStructBomVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comTestStructBomVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "SimulateStructBom") {//模拟单结构BOM
                if (this.list.MaterialId !=undefined ) {
                    if (this.list.Detail != undefined) {
                        var data = this.invorkBcf('GetSaleBomData', [this.list]);
                        if (data != null)
                            Ax.utils.LibVclSystemUtils.openDataFunc("com.TestStructBomRpt", "结构BOM处理后数据", data);

                    } else {
                        alert("该母件没有特征项，无法模拟");
                    }
                } else  {
                    this.saleOrderInfo = {
                        SaleOrderNo: this.dataSet.getTable(0).data.items[0].get("FROMBILLNO"),
                        SaleOrderRowId: this.dataSet.getTable(0).data.items[0].get("FROMROWID"),
                        SaleOrderSubRowId: this.dataSet.getTable(0).data.items[0].get("SUBROWID"),
                        ProductId: this.dataSet.getTable(0).data.items[0].get("MATERIALID"),
                        AttributeCode: this.dataSet.getTable(0).data.items[0].get("ATTRIBUTECODE"),
                        AttributeDesc: this.dataSet.getTable(0).data.items[0].get("ATTRIBUTEDESC"),
                        ProductQty: this.dataSet.getTable(0).data.items[0].get("QUANTITY"),
                        AttributeId: this.dataSet.getTable(0).data.items[0].get("ATTRIBUTEID")

                    };
                    if (this.saleOrderInfo != undefined) {
                        var data = this.invorkBcf('DoCreate', [this.saleOrderInfo]);
                        if (data != null)
                            Ax.utils.LibVclSystemUtils.openDataFunc("com.TestStructBomRpt", "结构BOM处理后数据", data);
                    } else {
                        alert("数据不完整，无法模拟");
                    }
                }
            }
            else if (e.dataInfo.fieldName == "ClearData") {//数据清空
                //清空表头数据
                Ext.getCmp('FROMBILLNO0_' + this.winId).setValue("");
                Ext.getCmp('FROMROWID0_' + this.winId).setValue("");
                Ext.getCmp('SUBROWID0_' + this.winId).setValue("");
                Ext.getCmp('MATERIALID0_' + this.winId).setValue("");
                Ext.getCmp('ATTRIBUTEID0_' + this.winId).setValue("");
                Ext.getCmp('ATTRIBUTECODE0_' + this.winId).setValue("");
                Ext.getCmp('ATTRIBUTEDESC0_' + this.winId).setValue("");
                Ext.getCmp('QUANTITY0_' + this.winId).setValue("");
                this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                //清空表身数据
                this.dataSet.getTable(1).removeAll();
            }
            break;
        case LibEventTypeEnum.Validating:

            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex === 0) {
                if (e.dataInfo.fieldName === "MATERIALID") {
                    if (this.dataSet.getTable(1).data.items.length > 0) {
                        this.dataSet.getTable(1).removeAll();
                    }
                    var AttributeId = e.dataInfo.dataRow.get("ATTRIBUTEID");//根据特征ID，母件ID 获取对应的特征项
                    this.list = this.invorkBcf("SetAttributeItem", [AttributeId, e.dataInfo.dataRow.get("MATERIALID")]);
                    for (var i = 0; i < this.list.Detail.length; i++) {
                        var masterRow = this.dataSet.getTable(0).data.items[0];
                        masterRow.store = this.dataSet.getTable(1);
                        var newRow = this.addRow(masterRow, 1);
                        newRow.set("ATTRIBUTEITEMID", this.list.Detail[i].AttributeItemId);
                        newRow.set("ATTRIBUTEITEMNAME", this.list.Detail[i].AttributeItemName);
                        newRow.set("ATTRIBUTECODELEN", this.list.Detail[i].AttributeCodeLen);
                    }
                }
                else if (e.dataInfo.fieldName === "SUBROWID") {//获取子键标识明细
                    var billno = e.dataInfo.dataRow.data["FROMBILLNO"];
                    var rowid = e.dataInfo.dataRow.data["FROMROWID"];
                    var subRowId = e.dataInfo.value;
                    this.list = this.invorkBcf("GetMaterialFromSubRowId", [billno, rowid, subRowId]);
                    Ext.getCmp('MATERIALID0_' + this.winId).setValue(this.list.MaterialId);
                    Ext.getCmp('UNITID0_' + this.winId).setValue(this.list.UnitId);
                    Ext.getCmp('ATTRIBUTEID0_' + this.winId).setValue(this.list.AttributeId);
                    Ext.getCmp('ATTRIBUTECODE0_' + this.winId).setValue(this.list.AttributeCode);
                    Ext.getCmp('ATTRIBUTEDESC0_' + this.winId).setValue(this.list.AttributeDesc);
                    this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                }
                //else if (e.dataInfo.fieldName === "QUANTITY") {
                //    var quantity = e.dataInfo.dataRow.data["QUANTITY"];
                //    var masterRow = this.dataSet.getTable(0).data.items[0];
                //    Ext.getCmp('MATERIALID0_' + this.winId).setValue(quantity);
                //}
            }
            if (e.dataInfo.tableIndex === 1) {
                if (e.dataInfo.fieldName === "ATTRIBUTEITEMROWID") {
                    for (var i = 0; i < this.list.Detail.length; i++) {
                        if (this.list.Detail[i].AttributeItemId === e.dataInfo.dataRow.get("ATTRIBUTEITEMID")) {
                            this.list.Detail[i].AttributeItemRowId = e.dataInfo.value;
                            this.list.Detail[i].AttrCode = e.dataInfo.dataRow.get("ATTRCODE");
                            this.list.Detail[i].AttrValue = e.dataInfo.dataRow.get("ATTRVALUE");
                        }
                    }
                }
                if (e.dataInfo.fieldName === "ATTRVALUE") {
                    for (var i = 0; i < this.list.Detail.length; i++) {
                        if (this.list.Detail[i].AttributeItemId === e.dataInfo.dataRow.get("ATTRIBUTEITEMID")) {
                            this.list.Detail[i].AttrValue = e.dataInfo.value;
                        }
                    }
                }
            }
            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
    }
}
