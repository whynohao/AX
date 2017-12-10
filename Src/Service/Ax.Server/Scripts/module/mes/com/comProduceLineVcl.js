comProduceLineVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comProduceLineVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comProduceLineVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {

        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "MATERIALTYPEID") {
                    //选择物料类别清空物料
                    try {
                        var table = this.dataSet.getTable(0);
                        var table2 = this.dataSet.getTable(1);
                        var rowid = e.dataInfo.dataRow.get("ROW_ID");
                        var materialId = "";
                        var materialName = "";
                        for (var i = 0; i < table2.data.items.length; i++) {
                            if (table2.data.items[i].data["ROW_ID"].toString() == rowid.toString()) {
                                table2.data.items[i].set("MATERIALID", materialId);//填充数据
                                table2.data.items[i].set("MATERIALNAME", materialName);//填充数据
                            }
                        }
                        this.forms[1].loadRecord(table.data.items[rowid - 1]);
                        this.forms[1].updateRecord(this.dataSet.getTable(1).data.items[rowid - 1]);

                    } catch (e) {

                    }

                }
            }
            break;
        case LibEventTypeEnum.Validated:

            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "MATERIALID") {
                    //选择物料填充物料类型
                    try {
                        var MATERIALTYPE = e.dataInfo.dataRow.get("MATERIALTYPE");
                        //判断物料类型是否为空，为空时需反填数据
                        if (MATERIALTYPE == null) {
                            var table = this.dataSet.getTable(0);
                            var table2 = this.dataSet.getTable(1);
                            var rowno = e.dataInfo.dataRow.get("ROWNO");
                            var rowid = e.dataInfo.dataRow.get("ROW_ID");
                            var materialId = e.dataInfo.dataRow.get("MATERIALID");
                            var type = this.invorkBcf("GetTypeId", [materialId]);//调用后台GetTypeId方法去物料类型
                            var materialTypeId = type.MaterialTypeId;
                            var materialTypeName = type.MaterialTypeName;
                            for (var i = 0; i < table2.data.items.length; i++) {
                                if (table2.data.items[i].data["ROW_ID"].toString() == rowid.toString()) {
                                    if (materialTypeName != null) {
                                        table2.data.items[i].set("MATERIALTYPEID", materialTypeId);//填充数据
                                        table2.data.items[i].set("MATERIALTYPENAME", materialTypeName);//填充数据
                                    }

                                }
                            }

                            this.forms[1].loadRecord(table.data.items[rowid - 1]);
                            this.forms[1].updateRecord(this.dataSet.getTable(1).data.items[rowid - 1]);
                        }
                        else {

                        }


                    } catch (e) {

                    }

                }
            }
            break;
    }
    //if (e.libEventType == LibEventTypeEnum.Validated) {
    //    if (e.dataInfo.fieldName == 'MATERIALTYPEID') {//修改物料类型时，将物料置为空
    //        if (e.dataInfo.value != e.dataInfo.oldValue) {
    //            e.dataInfo.dataRow.set('MATERIALID', "");
    //        }
    //        var rowId = e.dataInfo.dataRow.get("ROW_ID");
    //        this.forms[1].loadRecord(this.dataSet.getTable(1).data.items[rowId - 1]);
    //        this.forms[1].updateRecord(this.dataSet.getTable(1).data.items[rowId - 1]);
    //    }
    //}
    //if (e.libEventType == LibEventTypeEnum.Validating) {
    //    if (e.dataInfo.fieldName == 'MATERIALID')
    //    {
    //        if (e.dataInfo.dataRow.get("MATERIALTYPEID") == "") {//选择物料ID如果物料代码为空则填充改物料的物料类型到物料类型中
    //            var bodyTable = this.dataSet.getTable(1);
    //            var materialId = e.dataInfo.value;
    //            var typeList = this.invorkBcf("GetMaterialType", [materialId]);
    //            var rowId = e.dataInfo.dataRow.get("ROW_ID");
    //            bodyTable.data.items[rowId - 1].set("MATERIALTYPEID", typeList.MaterialTypeId);//填充数据
    //            bodyTable.data.items[rowId - 1].set("MATERIALTYPENAME", typeList.MaterialTypeName);//填充数据
    //            this.forms[1].loadRecord(this.dataSet.getTable(1).data.items[rowId - 1]);
    //            this.forms[1].updateRecord(this.dataSet.getTable(1).data.items[rowId - 1]);
    //        }
    //    }
    //}
}