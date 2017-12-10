comMateialDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = comMateialDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comMateialDataFuncVcl;

proto.fromVcl = null;
proto.fromObj = null;
proto.fromMethod = null;
proto.doSetParam = function (vclObj) {
    //判断参数是否为空,代表着是否被呼叫打开
    if (vclObj != undefined) {
        this.fromVcl = vclObj[0];
        this.fromObj = vclObj[1];
        this.fromMethod = vclObj[2];
        this.parms = vclObj[3];
        //给表头赋值
        var masterRow = this.dataSet.getTable(0).data.items[0];
        for (var i in this.parms) {
            masterRow.set(i, this.parms[i]);
        }
        this.forms[0].loadRecord(masterRow);
    }
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.ButtonClick:

            //查询
            if (e.dataInfo.fieldName == "BtnSelectMaterial") {
                var headTable = this.dataSet.getTable(0).data.items[0];
                var MaterialName = this.dataSet.getTable(0).data.items[0].data['MATERIALNAME'];
                var Specification = this.dataSet.getTable(0).data.items[0].data['SPECIFICATION'];
                var TextureId = this.dataSet.getTable(0).data.items[0].data['TEXTUREID'];
                var FigureNo = this.dataSet.getTable(0).data.items[0].data['FIGURENO'];
                var FigureNo1 = this.dataSet.getTable(0).data.items[0].data['FIGURENO1'];
                var TextureIdId = this.dataSet.getTable(0).data.items[0].data['TEXTUREIDID'];
                var IsStandard = this.dataSet.getTable(0).data.items[0].data['ISSTANDARD'];
                if (headTable == "" && MaterialName == "" && Specification == "" && TextureId == "" && TextureIdId == "") {
                    Ext.Msg.alert("系统提示", "表头字段至少填写一个才能查询");
                    return;
                }
                var returnData = this.invorkBcf("GetMaterialData", [MaterialName, Specification, TextureId, FigureNo, FigureNo1, TextureIdId, IsStandard]);
                if (returnData.length == 0) {
                    Ext.Msg.alert("提示", '查询结果为空！');
                    this.deleteAll(1);
                }
                else {
                    fillMaterialDataFunc.call(this, returnData);
                }
            }
            //确认
            if (e.dataInfo.fieldName == "BtnLoadMaterial") {
                //表身行项
                var selectItems = this.dataSet.getTable(1).data.items;
                //数组，用于存储数据
                var records = [];
                //循环所有行项
                for (var i = 0; i < selectItems.length; i++) {
                    //如果打勾
                    if (selectItems[i].data["ISCHOSE"] == true) {
                        var newRow = selectItems[i];
                        //将行项对象加入数组
                        records.push({
                            MaterialId: newRow.get('MATERIALID'),
                            MaterialName: newRow.get('MATERIALNAME'),
                            FigureNo: newRow.get('FIGURENO'),
                            FigureNo1: newRow.get('FIGURENO1'),
                            Specification: newRow.get('SPECIFICATION'),
                            Matstyle: newRow.get('MATSTYLE'),
                            Textureid: newRow.get('TEXTUREID'),
                            Textureidid: newRow.get('TEXTUREIDID'),
                            UnitId: newRow.get('UNITID'),
                            UnitName: newRow.get('UNITNAME')
                        })
                    }
                }

                if (records.length == 0) {
                    Ext.Msg.alert("系统提示", "请选择载入的明细！");
                    return;
                }

                this.fromMethod(this.fromVcl, this.fromObj, records);
                this.win.close();
            }
            if (e.dataInfo.fieldName == "BtnClearMaterial") {
                var headTable = this.dataSet.getTable(0).data.items[0];
                headTable.set('MATERIALNAME', "");
                headTable.set('SPECIFICATION', "");
                headTable.set('TEXTUREID', "");
                headTable.set('FIGURENO', "");
                headTable.set('FIGURENO1', "");
                headTable.set('TEXTUREIDID', "");
                this.forms[0].loadRecord(this.dataSet.getTable(0).data.items[0]);
            }
            break;
    }
}
function fillMaterialDataFunc(returnData) {
    Ext.suspendLayouts();
    var formStore = this.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        this.deleteAll(1);
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('FIGURENO', info.FigureNo);
                newRow.set('FIGURENO1', info.FigureNo1);
                newRow.set('SPECIFICATION', info.Specification);
                newRow.set('MATSTYLE', info.Matstyle);
                newRow.set('TEXTUREID', info.TextureId);
                newRow.set('TEXTUREIDID', info.TextureIdId);
                newRow.set('UNITID', info.UnitId);
                newRow.set('UNITNAME', info.UnitName);
            }
        }
    }
    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}