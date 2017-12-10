qcPurQualityCheckVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = qcPurQualityCheckVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = qcPurQualityCheckVcl;
var supplierid = "";
var rectivedate = 0;
proto.constructor = qcPurQualityCheckVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var mastertable = this.dataSet.getTable(0);
    if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "buildSpecialNotice") {
            var PurQualityCheck = {};
            var record = mastertable.data.items[0];
            PurQualityCheck.BillNo = record.get("BILLNO");
            PurQualityCheck.CurrentState = record.get("CURRENTSTATE");
            PurQualityCheck.PersonId = record.get("PERSONID");
            PurQualityCheck.BillDate = record.get("BILLDATE");
            PurQualityCheck.ReceiveDate = record.get("RECEIVEDATE");
            PurQualityCheck.SupplierId = record.get("SUPPLIERID");
            var data = this.invorkBcf('ProduceSpecialNotice', [PurQualityCheck]);
            if (data != null) {
                Ax.utils.LibVclSystemUtils.openBill('qc.PurSpecialNotice', '', 1, typeName, BillActionEnum.Browse, Ext.decode(entryParam), curPks);
            }
        }
        else if (e.dataInfo.fieldName == "BtnBarcodePrint") {
            if (!this.isEdit) {
                var frombillno = this.dataSet.getTable(0).data.items[0].data["BILLNO"];
                Ax.utils.LibVclSystemUtils.openDataFunc('stk.BarcodePrintDataFunc', "条码打印", [0, frombillno, this.tpl.ProgId, this.tpl.DisplayText]);
            }
            else {
                Ext.Msg.alert("提示", "编辑状态下不可操作！");
            }
        }

        if (e.dataInfo.fieldName == "DistributeTask") {
            
            //var list=[];
            //var record = this.dataSet.getTable(1);
            //if (record.data.length == 0) {
            //    alert("当前明细表里没有明细");
            //}
            //else {
                //for (var i = 0; i < record.data.length; i++) {
                //    if (record.data.items[i].data["CHECKPSDETAIL"] == 0) {
                //        var PurQualityCheck = {};
                //        PurQualityCheck.BillNo = record.data.items[i].data["BILLNO"];
                //        PurQualityCheck.Row_Id = record.data.items[i].data["ROW_ID"];
                //        PurQualityCheck.FromBillNo = record.data.items[i].data["FROMBILLNO"];
                //        PurQualityCheck.FromRowId = record.data.items[i].data["FROMROWID"];
                //        PurQualityCheck.BatchNo = record.data.items[i].data["BATCHNO"];
                //        PurQualityCheck.SubBatchNo = record.data.items[i].data["SUBBATCHNO"];
                //        PurQualityCheck.MaterialId = record.data.items[i].data["MATERIALID"];
                //        PurQualityCheck.MaterialName = record.data.items[i].data["MATERIALNAME"];
                //        PurQualityCheck.AttributeCode = record.data.items[i].data["ATTRIBUTECODE"];
                //        PurQualityCheck.AttributeDesc = record.data.items[i].data["ATTRIBUTEDESC"];
                //        PurQualityCheck.CheckType = record.data.items[i].data["CHECKTYPE"];
                //        PurQualityCheck.Quantity = record.data.items[i].data["QUANTITY"];
                //        PurQualityCheck.CheckNum = record.data.items[i].data["CHECKNUM"];
                //        list.push(PurQualityCheck);
                //    }
                //}
            gridName = "PURDSTASKDFCDETAIL";
            Ax.utils.LibVclSystemUtils.openDataFunc("qc.PurDistributeTaskDataFunc", "采购质检单明细分发", [this, gridName]);
                //Ax.utils.LibVclSystemUtils.openDataFunc("qc.PurDistributeTaskDataFunc", "采购质检单明细分发", [this, gridName, list]);
            //}
        }
        else if (e.dataInfo.fieldName == "BuildAbnormal") {
            if (!this.isEdit) {
                var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
                this.forms[0].loadRecord(masterRow);
                var currentState = this.dataSet.getTable(0).data.items[0].data["CURRENTSTATE"];
                if (currentState != "2") {
                    Ext.Msg.alert("提示", "未生效单据不能操作！");
                    return;
                }
                var list = [];
                var record = this.dataSet.getTable(1);
                for (var i = 0; i < record.data.length; i++) {
                    if (record.data.items[i].data["UNQUALIFIEDNUM"] > 0) {
                        var PurQualityCheck = {};
                        PurQualityCheck.IsChose = 1;
                        PurQualityCheck.BillNo = record.data.items[i].data["BILLNO"];
                        PurQualityCheck.Row_Id = record.data.items[i].data["ROW_ID"];
                        PurQualityCheck.RowNo = record.data.items[i].data["ROWNO"];
                        PurQualityCheck.FromBillNo = record.data.items[i].data["FROMBILLNO"];
                        PurQualityCheck.FromRowId = record.data.items[i].data["FROMROWID"];
                        PurQualityCheck.PurChaseOrder = record.data.items[i].data["PURCHASEORDER"];
                        PurQualityCheck.BatchNo = record.data.items[i].data["BATCHNO"];
                        PurQualityCheck.SubBatchNo = record.data.items[i].data["SUBBATCHNO"];
                        PurQualityCheck.MaterialId = record.data.items[i].data["MATERIALID"];
                        PurQualityCheck.MaterialName = record.data.items[i].data["MATERIALNAME"];
                        PurQualityCheck.AttributeCode = record.data.items[i].data["ATTRIBUTECODE"];
                        PurQualityCheck.AttributeDesc = record.data.items[i].data["ATTRIBUTEDESC"];
                        PurQualityCheck.CheckType = record.data.items[i].data["CHECKTYPE"];
                        PurQualityCheck.Quantity = record.data.items[i].data["QUANTITY"];
                        PurQualityCheck.CheckNum = record.data.items[i].data["CHECKNUM"];
                        PurQualityCheck.QualifiedNum = record.data.items[i].data["QUALIFIEDNUM"];
                        PurQualityCheck.UnQualifiedNum = record.data.items[i].data["UNQUALIFIEDNUM"];
                        list.push(PurQualityCheck);
                    }
                }
                if (list.length > 0) {
                    Ax.utils.LibVclSystemUtils.openDataFunc("qc.PurQualityCheckDataFunc", "异常报告单发起界面", [this, list]);
                } else {
                    alert("不存在不合格数量的明细，不需要发送异常！");
                    return;
                }
            }
            else { alert("编辑状态下不可操作！"); }
        }
    }
    if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
        if (e.dataInfo.tableIndex == 1) {
            if (e.dataInfo.dataRow != null) {
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
    }
    else if (e.libEventType == LibEventTypeEnum.Validating) {
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
                alert("检测数量要小于等于物料数量");
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
            var Checknum = e.dataInfo.dataRow.get("CHECKNUM");
            var IsUseCheck = e.dataInfo.dataRow.get("ISUSECHECK");
            var IsConfig = e.dataInfo.dataRow.get("ISCONFIG");
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
            } else {
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
            } else {
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
    else if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo.tableIndex == 0) {
            if (e.dataInfo.fieldName == "TYPEID" || e.dataInfo.fieldName == "BILLDATE" || e.dataInfo.fieldName == "RECEIVEDATE" || e.dataInfo.fieldName == "SUPPLIERID" || e.dataInfo.fieldName == "CHECKPEROSONID") {
                this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
            }
        }
        if (e.dataInfo.fieldName == "RECEIVEDATE") {
            var SupplierId = e.dataInfo.dataRow.get("SUPPLIERID");
            var ReceiveDate = e.dataInfo.value;
            var OldDate = e.dataInfo.oldValue;
            if (ReceiveDate != OldDate) {
                rectivedate = this.dataSet.getTable(0).data.items[0].data['RECEIVEDATE'];
                supplierid = this.dataSet.getTable(0).data.items[0].data['SUPPLIERID'];
                if (supplierid != "" && rectivedate != "") {
                    var returnList = this.invorkBcf("GetData", [rectivedate, supplierid]);
                    fillData.call(this, returnList);
                    supplierid = "";
                    rectivedate = 0;
                }
            }
        }
        if (e.dataInfo.fieldName == "SUPPLIERID") {
            var ReceiveDate = e.dataInfo.dataRow.get("RECEIVEDATE");
            var SupplierId = e.dataInfo.value;
            var OldSupplierId = e.dataInfo.oldValue;
            if (SupplierId != OldSupplierId) {
                rectivedate = this.dataSet.getTable(0).data.items[0].data['RECEIVEDATE'];
                supplierid = this.dataSet.getTable(0).data.items[0].data['SUPPLIERID'];
                if (supplierid != "" && rectivedate != "") {
                    var returnList = this.invorkBcf("GetData", [rectivedate, supplierid]);
                    fillData.call(this, returnList);
                    supplierid = "";
                    rectivedate = 0;
                }
            }
        }
        else if (e.dataInfo.fieldName == "QUALIFIEDNUM") {
            var Checknum = e.dataInfo.dataRow.get("CHECKNUM");
            var Qualifiednum = e.dataInfo.value;
            if (Qualifiednum <= Checknum) {
                if (Qualifiednum != null) {
                    e.dataInfo.dataRow.set("UNQUALIFIEDNUM", Checknum - Qualifiednum);
                    e.dataInfo.dataRow.set("QUALIFIEDRATE",Qualifiednum/Checknum);


                }
            }
        }
        else if (e.dataInfo.fieldName == "UNQUALIFIEDNUM") {
            var Checknum = e.dataInfo.dataRow.get("CHECKNUM");
            var Qualifiednum = e.dataInfo.dataRow.get("QUALIFIEDNUM");
            var UnQuanlifiednum = e.dataInfo.value;
            if (UnQuanlifiednum <= Checknum) {
                if (UnQuanlifiednum != null) {
                    e.dataInfo.dataRow.set("QUALIFIEDNUM", Checknum - UnQuanlifiednum);
                    e.dataInfo.dataRow.set("QUALIFIEDRATE", (Checknum - UnQuanlifiednum) / Checknum);
                }
            }
        }
        else if (e.dataInfo.fieldName == "CHECKNUM") {
            var Qualifiednum = e.dataInfo.dataRow.get("QUALIFIEDNUM");
            var UnQualifiednum = e.dataInfo.dataRow.get("UNQUALIFIEDNUM");
            var IsUseCheck = e.dataInfo.dataRow.get("ISUSECHECK");
            var IsConfig = e.dataInfo.dataRow.get("ISCONFIG");
            var Checknum = e.dataInfo.value;
            if (IsUseCheck == 0 & IsConfig == 0) {
                if (Checknum < Qualifiednum + UnQualifiednum) {
                    e.dataInfo.dataRow.set("QUALIFIEDNUM", 0);
                    e.dataInfo.dataRow.set("UNQUALIFIEDNUM", 0);
                    e.dataInfo.dataRow.set("QUALIFIEDRATE", 0);
                    alert("合格数量和不合格数量已清零,请重新填写！");

                }
                else if (Checknum > Qualifiednum + UnQualifiednum) {
                    if (Qualifiednum != 0 || UnQualifiednum != 0) {
                        alert("检测数量已修改，请重新填写合格数量或不合格数量！");
                    }
                    //if (Qualifiednum != null) {
                    //    e.dataInfo.dataRow.set("UNQUALIFIEDNUM", Checknum - Qualifiednum)
                    //}
                }
            }
        }
        if (e.dataInfo.fieldName == "CHECKTYPE") {
            var CheckType = e.dataInfo.value;
            var Quantity = e.dataInfo.dataRow.get("QUANTITY");
            var CheckNum = e.dataInfo.dataRow.get("CHECKNUM");
            if (CheckType == 1) {
                e.dataInfo.dataRow.set("CHECKNUM", Quantity);

            }
        }
    }
    else if (e.libEventType == LibEventTypeEnum.ColumnDbClick) {
        if (e.dataInfo.fieldName == "ATTACHMENTSRC") {
            var table = this.dataSet.getTable(1);
            Ax.utils.LibAttachmentForm.show(vcl, table.data.items[0], table.Name);
        }
    }
}


function fillData(returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        this.deleteAll(2);
        var grid = Ext.getCmp(this.winId + 'PURQUALITYCHECKDETAILGrid');
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        if (returnList !== undefined && returnList.length > 0) {
            for (var i = 0; i < returnList.length; i++) {
                var info = returnList[i];
                var newRow = this.addRowForGrid(grid);//这个方法第一个参数是表头数据行，第二个参数是当前grid数据源store所属第几个表
                newRow.set('FROMBILLNO', info.FromBillNo);
                newRow.set('FROMROWID', info.FromRow_Id);
                newRow.set('PURCHASEORDER', info.PurChaseOrder);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('ATTRIBUTECODE', info.AttributeCode);
                newRow.set('ATTRIBUTEDESC', info.AttributeDesc);
                newRow.set('CHECKTYPE', info.CheckType);
                newRow.set('WORKSTATIONCONFIGID', info.WorkStaionConfigId);
                newRow.set('QUANTITY', info.Quantity);
                newRow.set('CHECKNUM', info.Quantity);
                newRow.set('ISUSECHECK', info.IsUseCheck);
                newRow.set('ISCONFIG', info.IsConfig);

                newRow.set('BATCHNO', info.BatchNo);
                newRow.set('SUBBATCHNO', info.SubBatchNo);
                for (var j = 0; j < info.Dic.length; j++) {
                    if (info.Dic[j].CheckStId != '') {
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
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

