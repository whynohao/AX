comAbnormalDayVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};

var proto = comAbnormalDayVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comAbnormalDayVcl;
proto.winId = null;
proto.fromObj = null;
proto.getType = 0;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                //天数不能小于0
                if (e.dataInfo.fieldName == "ABNORMALDAY") {
                    if (e.dataInfo.value < 0) {
                        e.dataInfo.cancel = true;

                    }
                }
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "loaddata") {
                try {
                    this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                    if (this.dataSet.getTable(0).data.items[0].data["ATTRIBUTEID"].toString() != "") {
                        var attributeid = this.dataSet.getTable(0).data.items[0].data["ATTRIBUTEID"];
                        var attributeitemid = this.dataSet.getTable(0).data.items[0].data["ATTRIBUTEITEMID"];
                        var returnData = this.invorkBcf("GetData", [attributeid, attributeitemid]);
                        FillData.call(this, returnData);
                        Ext.Msg.alert('提示', '查询成功！');
                    }
                    else {
                        Ext.Msg.alert('提示', '特征代码不能为空！');
                    }
                } catch (e) {

                }

            }
            if (e.dataInfo.fieldName == "BtnSave") {
                try {
                    if (this.dataSet.getTable(1).data.length == 0) {
                        Ext.Msg.alert('提示', '暂无数据！');
                    }
                    else {
                        var count = this.dataSet.getTable(1).data.length;
                        var DicClear = this.invorkBcf("DicClear");
                        for (var i = 0; i < count; i++) {
                            var ATTRIBUTEITEMID = this.dataSet.getTable(1).data.items[i].data["ATTRIBUTEITEMID"];
                            var ROW_ID = this.dataSet.getTable(1).data.items[i].data["ROW_ID"];
                            var ABNORMALDAY = this.dataSet.getTable(1).data.items[i].data["ABNORMALDAY"];
                            var arr = [];
                            arr = [ATTRIBUTEITEMID.toString(), ROW_ID.toString(), ABNORMALDAY.toString()];
                            //arr.push({
                            //    "ATTRIBUTEITEMID": this.dataSet.getTable(1).data.items[i].data["ATTRIBUTEITEMID"].toString(),
                            //    " ROW_ID": this.dataSet.getTable(1).data.items[i].data["ROW_ID"].toString(),
                            //    "ABNORMALDAY": this.dataSet.getTable(1).data.items[i].data["ABNORMALDAY"].toString()
                            //});
                            var Dic = this.invorkBcf("Dic", [arr, i]);
                            arr = [];
                        }

                        var update = this.invorkBcf("update", [count]);
                        Ext.Msg.alert('提示', '保存成功！');
                    }
                } catch (e) {

                }
            }
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
            break;
    }
}

function FillData(data) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var list = data;//一般是中间层返回来的数据，中间可能定义的是dictionary,在前段反序列化之后是对象
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);//这个方法第一个参数是表头数据行，第二个参数是当前grid数据源store所属第几个表
                newRow.set("ATTRIBUTEITEMID", info.Attributeitemid);
                newRow.set("ROW_ID", info.Row_id);//赋值语句
                newRow.set("ROWNO", info.Rowno);
                newRow.set("ATTRCODE", info.Attrcode);
                newRow.set("ATTRVALUE", info.Attrvalue);
                newRow.set("NONSTANDARD", info.Nonstandard);
                newRow.set("ABNORMALDAY", info.Abnormalday);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}
