comScheduleProductRuleVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comScheduleProductRuleVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comScheduleProductRuleVcl;
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
}