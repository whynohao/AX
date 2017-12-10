
comAttributeItemVcl  = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = comAttributeItemVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comAttributeItemVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                Ext.Msg.confirm('提示', '是否确认删除?', function (button) {
                    if (button == "yes") {
                        vcl.deleteRowForGrid(e.dataInfo.curGrid);
                    }
                    else if (button == "no") {
                        e.dataInfo.cancel = true;
                    }
                }, this);
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnSyncStructBom") {//同步
                if (!vcl.isEdit) {
                    Ext.Msg.confirm('提示', '是否确认同步结构BOM?', function (button) {
                        if (button == "yes") {
                            var attributeItemArray = {};
                            var attributeItemId = this.dataSet.getTable(0).data.items[0].get("ATTRIBUTEITEMID");
                            var items = this.dataSet.getTable(1).data.items;
                            for (var i = 0; i < items.length; i++) {
                                var rowId = items[i].data["ROW_ID"];
                                var attrValue = items[i].data["ATTRVALUE"];
                                attributeItemArray[rowId] = attrValue;
                            }
                            var data = this.invorkBcf('SyncStructBom', [attributeItemArray,attributeItemId]);
                            Ext.Msg.alert("提示", "同步结构BOM成功");
                        }
                        else if (button == "no") {
                            e.dataInfo.cancel = true;
                        }
                    }, this);
                }
                else {
                    Ext.Msg.alert("提示", "修改状态下，不能同步结构BOM");

                }
            }
            break;
    }
}