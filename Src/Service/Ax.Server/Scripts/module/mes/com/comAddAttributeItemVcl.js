comAddAttributeItemVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
}
var proto = comAddAttributeItemVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comAddAttributeItemVcl;
proto.winId = null;
proto.fromObj = null;
proto.getType = null;

proto.doSetParam = function (vclObj) {

};

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "BtnSave":
                    var bodyTable = this.dataSet.getTable(1).data.items;
                    var headTable = this.dataSet.getTable(0).data.items;
                    var list = new Array();
                    var saveSuccess = false;

                    if (headTable[0].data["ATTRIBUTEID"] == "") {
                        Ext.Msg.alert("警告", "表头特征项不能为空！");
                    }
                    else if (headTable[0].data["ATTRIBUTEITEMID"] == "") {
                        Ext.Msg.alert("警告", "表头特征项代码不能为空！");
                    }
                    else if (headTable[0].data["ATTRIBUTEITEMNAME"] == "") {
                        Ext.Msg.alert("警告", "表头特征项名称不能为空！");
                    }
                    else if (headTable[0].data["ATTRIBUTECODELEN"] <= 0) {
                        Ext.Msg.alert("警告", "表头编码长度必须大于0！");
                    }
                    else {
                        for (var i = 0; i < bodyTable.length; i++) {
                            list.push({//传入的数据
                                ATTRIBUTEID: headTable[0].data["ATTRIBUTEID"],
                                ATTRCODE: bodyTable[i].data["ATTRCODE"],
                                ATTRIBUTEITEMID: bodyTable[i].data["ATTRIBUTEITEMID"],
                                ATTRIBUTEITEMNAME: headTable[0].data["ATTRIBUTEITEMNAME"],
                                ATTRIBUTECODELEN: headTable[0].data["ATTRIBUTECODELEN"],
                                ATTRVALUE: bodyTable[i].data["ATTRVALUE"],
                                ROWNO: bodyTable[i].data["ROWNO"],
                                ROW_ID: bodyTable[i].data["ROW_ID"],
                            });
                        }

                        //调用后台保存方法
                        saveSuccess = this.invorkBcf("SaveAddAttributeItem", [list]);
                    }

                    var grid = Ext.getCmp(this.winId + 'COMADDATTRIBUTEITEMDETAILGrid'); //要加载数据的表名字 + Grid
                    var records = grid.getView().getSelectionModel().getSelection();

                    if (saveSuccess == true) {
                        alert("保存成功");
                        //fillGetnoticeReturnData.call(this, records)
                        //this.win.close();
                    }
                    break;
            }
            break;
        case LibEventTypeEnum.Validated:

            var headTable = this.dataSet.getTable(0).data.items[0];
            var attributeItemId = headTable.data["ATTRIBUTEITEMID"];
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "ATTRIBUTEITEMID") {
                    if (attributeItemId!="") {
                        //如果是点击的是表头的ATTRIBUTEITEMID调用后台方法
                        var data = this.invorkBcf("GetAddAttributeItemDetail", [attributeItemId]);
                        if (data.length == 0) {//如果返回的数据为空，表头的数据设置为空
                            FillData.call(this, data);
                            Ext.getCmp("ATTRIBUTECODELEN0_" + this.winId).setValue(0);
                            Ext.getCmp("ATTRIBUTEITEMNAME0_" + this.winId).setValue();
                        }
                        else {//如果返回的数据不为空，则将编码长度等信息填到表头
                            FillData.call(this, data);
                            Ext.getCmp("ATTRIBUTECODELEN0_" + this.winId).setValue(data[0]["AttributeCodeLen"]);
                            Ext.getCmp("ATTRIBUTEITEMNAME0_" + this.winId).setValue(data[0]["AttributeItemName"]);
                        }
                    }
                    else {
                        Ext.Msg.alert("特征项代码不能为空！");
                    }
                    
                }
            }
            var form = this.dataSet.getTable(0).data.items[0];
            this.forms[0].updateRecord(form);//更新表头数据

            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex==0) {
                var headTable = this.dataSet.getTable(0).data.items[0];
                var attributeItemId = headTable.data["ATTRIBUTEITEMID"];
                var attributeId = headTable.data["ATTRIBUTEID"];

                //当修改的表头是已有的时，取消操作
                if (e.dataInfo.fieldName == "ATTRIBUTEITEMNAME" || e.dataInfo.fieldName == "ATTRIBUTECODELEN") {
                    var isNewRow = false;
                    isNewRow = this.invorkBcf("JudgeHeadIsNewRow", [attributeId, attributeItemId]);
                    if (isNewRow == false) {
                        e.dataInfo.cancel = true;
                    }
                }
            }
            if (e.dataInfo.tableIndex == 1) {
                var form = this.dataSet.getTable(0).data.items[0];
               
                var attriCode = e.dataInfo.dataRow.get("ATTRCODE");
                var attriItemId = e.dataInfo.dataRow.get("ATTRIBUTEITEMID");
                var row_Id = e.dataInfo.dataRow.get("ROW_ID");
                var attriValue = e.dataInfo.dataRow.get("ATTRVALUE");
                var rowNo = e.dataInfo.dataRow.get("ROWNO");//取到旧值
                var codeLen = Ext.getCmp("ATTRIBUTECODELEN0_" + this.winId);

                //当修改的明细表的行是已有行时，取消操作
                if ( e.dataInfo.fieldName == "ATTRVALUE" || e.dataInfo.fieldName == "ROWNO") {
                    var isNewRow = false;
                    //调用后台方法，判断当前行是否是新增行
                    isNewRow = this.invorkBcf("JudgeIsNewRow", [attriItemId,row_Id]);
                    if (isNewRow==false) {
                        e.dataInfo.cancel = true;
                    }
                }
                if (e.dataInfo.fieldName == "ATTRCODE")
                {
                    var isNewRow = false;
                    //调用后台方法，判断当前行是否是新增行
                    isNewRow = this.invorkBcf("JudgeIsNewRow", [attriItemId, row_Id]);
                    if (isNewRow == false) {
                        e.dataInfo.cancel = true;
                    }
                    else {
                        //判断特征项编码长度和表头编码长度
                        if (e.dataInfo.value.length>codeLen.lastValue) {
                            alert("第" + row_Id + "行特征项编码长度不能大于编码长度");
                        }
                    }
                }

                this.forms[0].updateRecord(form);
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            var headTable = this.dataSet.getTable(0).data.items[0];
            var attributeItemId = headTable.data["ATTRIBUTEITEMID"];
            var row_Id = e.dataInfo.dataRow.get("ROW_ID");

            var isNewRow = false;
            //调用后台方法，判断当前行是否是新增行
            isNewRow = this.invorkBcf("JudgeIsNewRow", [attributeItemId, row_Id]);

            //如果不是新增行则不允许删除
            if (isNewRow==false) {
                e.dataInfo.cancel = true;
            }

            break;
    }

    //将特征项已有的数据填充到新增特征项DataFunC的明细表中
    function FillData(data) {
        Ext.suspendLayouts();//关闭Ext布局
        //  console.log(this.dataSet);
        var curStore = this.dataSet.getTable(1);
        curStore.suspendEvents();//关闭store事件
        try {
            this.dataSet.getTable(1).removeAll();
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var list = data;
            if (list != undefined && list.length > 0) {
                for (var i = 0; i < list.length; i++) {
                    var info = list[i];
                    var newRow = this.addRow(masterRow, 1);
                    newRow.set("ATTRIBUTEITEMID", info.AttributeItemId);
                    newRow.set("ROW_ID", info.RowId);
                    newRow.set("ATTRCODE", info.AttrCode);
                    newRow.set("ATTRVALUE", info.AttrValue);
                }
            }
        } finally {
            curStore.resumeEvents();//打开store事件
            if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
                curStore.ownGrid.reconfigure(curStore);
            Ext.resumeLayouts(true);//打开Ext布局
        }
    }

    //将新增特征项的数据反填到特征项的明细表中
    function fillGetnoticeReturnData(returnData) {
        Ext.suspendLayouts();
        var curStore = proto.fromObj.dataSet.getTable(1);
        curStore.suspendEvents();
        try {
            var list = returnData;
            if (list != undefined && list.length > 0) {
                for (var i = 0; i < list.length; i++) {
                    var info = list[i];
                    var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
                    //Ext.getCmp("FROMBILLNO0_" + proto.winId).setValue(info.data["BILLNO"]);
                    proto.fromObj.forms[0].updateRecord(masterRow);
                    var newRow = proto.fromObj.addRow(masterRow, 1);
                    newRow.set("ATTRIBUTEITEMID", info.AttributeItemId);
                    newRow.set("ROW_ID", info.RowId);
                    newRow.set("ATTRCODE", info.AttrCode);
                    newRow.set("ATTRVALUE", info.AttrValue);
                    //newRow.set('TRANSQTY', info.data.TRANSQTY);
                    //newRow.set('UNTRANSQTY', info.data.UNTRANSQTY);

                }
            }
        } finally {
            curStore.resumeEvents();
            if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
                curStore.ownGrid.reconfigure(curStore);
            Ext.resumeLayouts(true);
        }

    }
}