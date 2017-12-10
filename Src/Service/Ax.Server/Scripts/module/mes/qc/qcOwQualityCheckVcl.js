qcOwQualityCheckVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = qcOwQualityCheckVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = qcOwQualityCheckVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }
    if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
        if (e.dataInfo.tableIndex == 1) {
            var store = this.dataSet.getTable(e.dataInfo.tableIndex);
            var StartTime = e.dataInfo.dataRow.get("STARTTIME");
            var WorkOrderNo = e.dataInfo.dataRow.get("WORKORDERNO");
            var IsFinished = e.dataInfo.dataRow.get("ISFINISHED");
            if (IsFinished == true) {
                alert("该明细已经完结，无法被删除！");
                e.dataInfo.cancel = true;
            }
            else {
                if (StartTime != 0) {
                    alert("该明细已经开始质检，无法被删除！");
                    e.dataInfo.cancel = true;
                }
                else {
                    if (WorkOrderNo != "") {
                        alert("该明细已经被派工单引用，无法被删除！");
                        e.dataInfo.cancel = true;
                    }
                }
            }
        }
    }
    if (e.libEventType == LibEventTypeEnum.Validating) {
        //控制检测数量，使其无法大于物料数量
        if (e.dataInfo.fieldName == "CHECKNUM") {
            var store = this.dataSet.getTable(e.dataInfo.tableIndex);
            var Quantity = e.dataInfo.dataRow.get("QUANTITY");
            var IsUseCheck = e.dataInfo.dataRow.get("ISUSECHECK");
            var IsConfig = e.dataInfo.dataRow.get("ISCONFIG");
            var IsFinished = e.dataInfo.dataRow.get("ISFINISHED");
            var Qualifiednum = e.dataInfo.dataRow.get("QUALIFIEDNUM");
            var UnQualifiednum = e.dataInfo.dataRow.get("UNQUALIFIEDNUM");
            var Checknum = e.dataInfo.value;
            var CheckType = e.dataInfo.dataRow.get("CHECKTYPE");
            if (CheckType == 1) {
                alert("当前检测方式为全检，无法修改检测数量");
                e.dataInfo.cancel = true;
            }
            else if (Checknum > Quantity) {
                alert("检测数量要小于物料数量");
                e.dataInfo.cancel = true;
            }
            else if (IsUseCheck == 1 || IsConfig == 1) {
                if (Qualifiednum > 0 || UnQualifiednum > 0) {
                    alert("质检已开始,检测数量无法修改");
                    e.dataInfo.cancel = true;
                }
            }
            else if (IsFinished != false) {
                alert("该明细已经完结，检测数量无法修改");
                e.dataInfo.cancel = true;
            }
        }
        //控制合格数量，使其无法大于检测数量，并自动算出未合格数量
        if (e.dataInfo.fieldName == "QUALIFIEDNUM") {
            var store = this.dataSet.getTable(e.dataInfo.tableIndex);
            var IsUseCheck = e.dataInfo.dataRow.get("ISUSECHECK");
            var IsConfig = e.dataInfo.dataRow.get("ISCONFIG");
            var Checknum = e.dataInfo.dataRow.get("CHECKNUM");
            var Qualifiednum = e.dataInfo.value;
            var IsFinished = e.dataInfo.dataRow.get("ISFINISHED");
            if (IsFinished == false) {
                if (Qualifiednum > Checknum) {
                    alert("合格数量要小于等于检测数量");
                    e.dataInfo.cancel = true;
                }
                if (IsUseCheck == 1 || IsConfig == 1) {
                    alert("当启用检测站点或启用检测流程时，合格数量无法修改");
                    e.dataInfo.cancel = true;
                }
            }else{
                alert("该明细已经完结，合格数量无法修改");
                e.dataInfo.cancel = true;
            }
        }
        if (e.dataInfo.fieldName == "UNQUALIFIEDNUM") {
            var IsUseCheck = e.dataInfo.dataRow.get("ISUSECHECK");
            var IsConfig = e.dataInfo.dataRow.get("ISCONFIG");
            var Qualifiednum = e.dataInfo.dataRow.get("QUALIFIEDNUM");
            var Checknum = e.dataInfo.dataRow.get("CHECKNUM");
            var UnQualifiedNum = e.dataInfo.value;
            var IsFinished = e.dataInfo.dataRow.get("ISFINISHED");
            if (IsFinished == false) {
                if (IsUseCheck == 1 || IsConfig == 1) {
                    alert("当启用检测站点或启用检测流程时，不合格数量无法修改");
                    e.dataInfo.cancel = true;
                }
                else {
                    if (UnQualifiedNum < Checknum) {
                        if (Qualifiednum + UnQualifiedNum > Checknum) {
                            alert("检测数量不能小于合格数量和不合格数量之和");
                            e.dataInfo.cancel = true;
                        }
                    } else {
                        alert("不合格数量要小于等于检测数量");
                        e.dataInfo.cancel = true;
                    }
                }
            }
            else {
                alert("该明细已经完结，不合格数量无法修改");
                e.dataInfo.cancel = true;
            }
        }
        if (e.dataInfo.fieldName == "STARTTIME") {
            var IsUseCheck = e.dataInfo.dataRow.get("ISUSECHECK");
            var IsConfig = e.dataInfo.dataRow.get("ISCONFIG");
            var EndTime = e.dataInfo.dataRow.get("ENDTIME");
            var StarTime = e.dataInfo.value;
            if (IsUseCheck == 1 || IsConfig == 1) {
                alert("当启用检测站点或启用检测流程时，开始时间无法修改");
                e.dataInfo.cancel = true;
            }
            if (EndTime != 0) {
                if (StarTime > EndTime) {
                    alert("开始时间不能大于结束时间");
                    e.dataInfo.cancel = true;
                }
            }
        }
        if (e.dataInfo.fieldName == "ENDTIME") {
            var IsUseCheck = e.dataInfo.dataRow.get("ISUSECHECK");
            var IsConfig = e.dataInfo.dataRow.get("ISCONFIG");
            var StarTime = e.dataInfo.dataRow.get("STARTTIME");
            var EndTime = e.dataInfo.value;
            if (IsUseCheck == 1 || IsConfig == 1) {
                alert("当启用检测站点或启用检测流程时，结束时间无法修改");
                e.dataInfo.cancel = true;
            }
            if (StarTime != 0) {
                if (StarTime > EndTime) {
                    alert("开始时间不能大于结束时间");
                    e.dataInfo.cancel = true;
                }
            }
        }
        if (e.dataInfo.fieldName == "CHECKTYPE") {
            var IsFinished = e.dataInfo.dataRow.get("ISFINISHED");
            if (IsFinished == true) {
                alert("该数据已经完结，无法修改！");
                e.dataInfo.cancel = true;
            }
        }
    }
    if (e.libEventType == LibEventTypeEnum.Validated) {            //单元格验证后的计算
        if (e.dataInfo.fieldName == "TYPEID" || e.dataInfo.fieldName == "CHECKPERSONID") {
            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
        }
        else if (e.dataInfo.fieldName == "FROMBILLNO") {
            this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
            var frombillno = this.dataSet.getTable(0).data.items[0].data['FROMBILLNO'];
            if (frombillno == "") {
                this.dataSet.getTable(1).removeAll();
                this.dataSet.getTable(2).removeAll();
            }
            else {
                var returnList = this.invorkBcf("GetData", [frombillno]);
                this.fillData.call(this, returnList);
            }
        }
        else if (e.dataInfo.fieldName == "QUALIFIEDNUM") {              //计算不合格数量
            var Checknum = e.dataInfo.dataRow.get("CHECKNUM");
            var Qualifiednum = e.dataInfo.value;
            if (Checknum >= Qualifiednum) {
                if (Qualifiednum != null) {
                    e.dataInfo.dataRow.set("UNQUALIFIEDNUM", Checknum - Qualifiednum);
                }
            }
        }
        else if (e.dataInfo.fieldName == "UNQUALIFIEDNUM") {              //计算不合格数量
            var Checknum = e.dataInfo.dataRow.get("CHECKNUM");
            var UnQuanlifiednum = e.dataInfo.value;
            var IsFinished = e.dataInfo.dataRow.get("ISFINISHED");
            if (UnQuanlifiednum <= Checknum) {
                if (UnQuanlifiednum != null) {
                    e.dataInfo.dataRow.set("QUALIFIEDNUM", Checknum - UnQuanlifiednum);
                }
            }
        }
        else if (e.dataInfo.fieldName == "CHECKNUM") {
            var Qualifiednum = e.dataInfo.dataRow.get("QUALIFIEDNUM");
            var UnQualifiednum=e.dataInfo.dataRow.get("UNQUALIFIEDNUM");
            var IsUseCheck = e.dataInfo.dataRow.get("ISUSECHECK");
            var IsConfig = e.dataInfo.dataRow.get("ISCONFIG");
            var Checknum = e.dataInfo.value;
            var CheckType = e.dataInfo.dataRow.get("CHECKTYPE");
            if (IsUseCheck == 0 & IsConfig == 0) {
                if (Checknum > Qualifiednum + UnQualifiednum) {
                    if (Qualifiednum != 0 || UnQualifiednum != 0) {
                        alert("检测数量已修改，请重新填写合格数量或不合格数量！");
                    }
                }
                else if (Checknum < Qualifiednum + UnQualifiednum) {
                    e.dataInfo.dataRow.set("QUALIFIEDNUM", 0);
                    e.dataInfo.dataRow.set("UNQUALIFIEDNUM", 0);
                    alert("合格数量和不合格数量已清零,请重新填写！");
                }
            }
        }
        if (e.dataInfo.fieldName == "CHECKTYPE") {
            var CheckType = e.dataInfo.value;
            var MatrialNum = e.dataInfo.dataRow.get("QUANTITY");
            if (CheckType == 1) {
                e.dataInfo.dataRow.set("CHECKNUM",MatrialNum);
            }
        }
    }
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        //if (this.isEdit) {
        if (e.dataInfo.fieldName == "Load") {
            var gridName = "OWQUALITYCHECKDATAFUNCDETAIL";
            Ax.utils.LibVclSystemUtils.openDataFunc("qc.OwQualityCheckDataFunc", "出库质检单数据载入", [this, gridName]);
        }
        //}
    }
}

proto.fillData = function(returnList) {
    //Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    var table = this.dataSet.getTable(2);
    //curStore.suspendEvents();//关闭store事件
    //table.suspendEvents();//关闭store事件
    try {
        this.deleteAll(2);
        this.deleteAll(1);//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        if (returnList !== undefined && returnList.length > 0) {
            for (var i = 0; i < returnList.length; i++) {
                var info = returnList[i];
                    var newRow = this.addRow(masterRow, 1);//这个方法第一个参数是表头数据行，第二个参数是当前grid数据源store所属第几个表
                    newRow.set('FROMBILLNO', info.FromBillNo);
                    newRow.set('FROMROWID', info.FromRowId);
                    newRow.set('MATERIALID', info.MaterialId);
                    newRow.set('MATERIALNAME', info.MaterialName);
                    newRow.set('ATTRIBUTECODE', info.AttributeCode);
                    newRow.set('ATTRIBUTEDESC', info.AttributeDesc);
                    newRow.set('WORKSTATIONCONFIGID', info.WorkStationConfigId);
                    newRow.set('QUANTITY', info.Quantity);
                    newRow.set('CHECKNUM', info.Quantity);
                    newRow.set('ISUSECHECK', info.IsUseCheck);
                    newRow.set('ISCONFIG', info.IsConfig);
                    newRow.set('BATCHNO', info.BatchNo);
                    newRow.set('SUBBATCHNO',info.SubBatchNo);
                    for (var j = 0; j < info.Dic.length; j++) {
                        if (info.Dic[j].CheckStId != "") {
                            var subRow = this.addRow(newRow, 2);
                            subRow.set('CHECKSTID', info.Dic[j].CheckStId);
                            subRow.set('CHECKSTNAME', info.Dic[j].CheckStName);
                            newRow.set('CHECKSTDETAIL', true);
                        }
                    }
            }
        }
    }
    finally {
        //curStore.resumeEvents();//打开store事件
        //if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
        //    curStore.ownGrid.reconfigure(curStore);
        //table.resumeEvents();//打开store事件
        //if (table.ownGrid && table.ownGrid.getView().store != null)
        //    table.ownGrid.reconfigure(table);
        //Ext.resumeLayouts(true);//打开Ext布局
    }
}

