plsProduceMonthPlanVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};

var proto = plsProduceMonthPlanVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = plsProduceMonthPlanVcl;
var workBillNo = "";
proto.calcTime = function (masterRow, bodyRow, fieldName, value) {
    var ret = this.invorkBcf('CalcPlanTime', [masterRow.get('TYPEID'),
                    fieldName, value, masterRow.get('SCHEDULEPRODUCEID'),
                    bodyRow.get('MATERIALID'), bodyRow.get('QUANTITY')]);
    var subStores = [];
    var curStore = this.dataSet.getTable(1);
    for (var i = 2; i < this.dataSet.dataList.length; i++) {
        var table = this.dataSet.dataList[i];
        if (table.Name.indexOf('DYTABLE') == 0) {
            var parentIndex = table['ParentIndex'];
            var model = this.dataSet.getChildren(parentIndex, bodyRow, i);
            if (model && model.length > 0) {
                subStores.push(model[0]);
            }
        }
    }
    Ext.suspendLayouts();
    curStore.beginUpdate();
    try {
        for (p in ret) {
            if (!ret.hasOwnProperty(p))
                continue;
            var value = ret[p];
            if (value !== undefined) {
                if (bodyRow.data[p] !== undefined) {
                    bodyRow.set(p, value);
                } else {
                    for (var r = 0; r < subStores.length; r++) {
                        if (subStores[r].data[p] !== undefined) {
                            subStores[r].set(p, value);
                            continue;
                        }
                    }
                }
            }
        }
    } finally {
        curStore.endUpdate();
        //if (curStore.ownGrid)
        //    curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);
    }
    return ret;
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    me = this;
    if (e.dataInfo && e.dataInfo.tableIndex >= 1) {
        //--------------------------------------------------------------------------------------------------------------------------------
        //修改时
        //--------------------------------------------------------------------------------------------------------------------------------
        if (e.libEventType == LibEventTypeEnum.Validated) {
            if ((e.dataInfo.fieldName == 'PLANFINISHTIME' || sender.isDynamic === true) && e.dataInfo.value != null) {
                var masterRow = this.dataSet.getTable(0).data.items[0];
                var bodyRow = e.dataInfo.tableIndex == 1 ? e.dataInfo.dataRow : e.dataInfo.curGrid.parentRow;
                if (bodyRow.get('ISAUTOCALC') === true) {
                    this.calcTime(masterRow, bodyRow, e.dataInfo.fieldName, e.dataInfo.value);
                }
            }
            if (e.dataInfo.tableIndex == 1)
            {
                //明细表中，作业单号更改
                //检查MIXEDSINGLE字段，是否混单，是则清空后面数据
                if (e.dataInfo.fieldName == 'WORKORDERBILLNO') {
                    if (e.dataInfo.dataRow.get("ISMIXED")) {
                        e.dataInfo.dataRow.set('FROMTYPE', 0);
                        e.dataInfo.dataRow.set('FROMBILLNO', "");
                        e.dataInfo.dataRow.set('FROMROWID', 0);
                        e.dataInfo.dataRow.set('CUSTOMERID', "");
                        e.dataInfo.dataRow.set('CUSTOMERNAME', "");
                        e.dataInfo.dataRow.set('MATERIALID', "");
                        e.dataInfo.dataRow.set('MATERIALNAME', "");
                        e.dataInfo.dataRow.set('MATERIALSPEC', "");
                        e.dataInfo.dataRow.set('UNITID', "");
                        e.dataInfo.dataRow.set('UNITNAME', "");
                        e.dataInfo.dataRow.set('QUANTITY', 0);
                        e.dataInfo.dataRow.set('SALDATE', 0);
                        e.dataInfo.dataRow.set('DELIVERYDATE', 0);
                    }
                }               
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        //新增行
        //--------------------------------------------------------------------------------------------------------------------------------
        else if (e.libEventType == LibEventTypeEnum.AddRow) {
            if (e.dataInfo.tableIndex == 1) {
                var parentRowMap = new Ext.util.MixedCollection();
                parentRowMap.add(1, e.dataInfo.dataRow);
                for (var i = 2; i < this.dataSet.dataList.length; i++) {
                    var table = this.dataSet.dataList[i];
                    if (table.Name.indexOf('DYTABLE') == 0) {
                        var parentIndex = table['ParentIndex'];
                        var parentTable = this.dataSet.getTable(parentIndex);
                        var parent = parentRowMap.get(parentIndex);
                        var newRow = Ext.decode(this.tpl.Tables[table.Name].NewRowObj);
                        newRow['ROW_ID'] = ++table['MaxRowId'];
                        var pks = parentTable.Pks;
                        var curPks = table.Pks;
                        for (var r = 0; r < pks.length; r++) {
                            newRow[curPks[r]] = parent.get(pks[r]);
                        }
                        var key = this.dataSet.getKey(pks, parent);
                        var maxRowNo = this.dataSet.maxRowNo.get((key + '/t' + i)) || 0;
                        var rowNo = maxRowNo + 1;
                        this.dataSet.maxRowNo.replace((key + '/t' + i), rowNo);
                        newRow['ROWNO'] = rowNo;
                        var newModel = table.add(newRow)[0];
                        this.dataSet.addData(i, newModel);
                        parentRowMap.add(i, newModel);
                    }
                }
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        //子表和子子表不能手动新增删除行
        //--------------------------------------------------------------------------------------------------------------------------------
        else if (e.libEventType == LibEventTypeEnum.BeforeAddRow || e.libEventType == LibEventTypeEnum.BeforeDeleteRow)
        {
            if (e.dataInfo.tableIndex == 3 || e.dataInfo.tableIndex == 4)
            {
                e.dataInfo.cancel = true;
            }
        }
        //else if (e.libEventType == LibEventTypeEnum.Validating) {
        //    if (e.tableIndex == 2) {
        //        e.tableIndex = 3;
        //    }
        //}
    }

    //--------------------------------------------------------------------------------------------------------------------------------
    //按钮按下
    //--------------------------------------------------------------------------------------------------------------------------------
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        if (e.dataInfo.fieldName == "btnLoad") {
            var panel = Ext.create('Ext.form.Panel', {
                bodyPadding: 10,
                border: false,
                renderTo: Ext.getBody(),
                items: [{
                    xtype: 'filefield',
                    name: 'txtFile',
                    fieldLabel: '文件',
                    labelWidth: 50,
                    msgTarget: 'side',
                    allowBlank: false,
                    anchor: '100%',
                    buttonText: '选择...'
                }],

                buttons: [{
                    text: '导入',
                    handler: function () {
                        var form = this.up('form').getForm();
                        if (form.isValid()) {
                            form.submit({
                                url: '/fileTranSvc/upLoadFile',
                                waitMsg: '正在导入文件...',
                                success: function (fp, o) {
                                    if (importData(o.result.FileName) == true) {
                                        Ext.Msg.alert('提示', '文件 "' + o.result.FileName + '" 导入成功.');
                                    }
                                },
                                failure: function (fp, o) {
                                    Ext.Msg.alert('错误', '文件 "' + o.result.FileName + '" 导入失败.');
                                }
                            });
                        }
                    }
                }]
            });
            win = Ext.create('Ext.window.Window', {
                width: 400,
                id: "GY",
                height: 300,
                layout: 'fit',
                items: [panel]
            });
            win.show();
        }
    }
};

importData = function (fileName) {

    //Ax.vcl.LibVclData.prototype.importData.apply(this, arguments);
    var assistObj = {};
    var worknoArray = [];
    var items = this.vcl.dataSet.getTable(1).data.items;
    for (var i = 0; i < items.length; i++) {
        if (items[i].data['WORKNO'] != null && items[i].data['WORKNO'] != "") {
            worknoArray[i] = items[i].data['WORKNO'];
        }
    }
    var masterRow = this.vcl.dataSet.getTable(0).data.items[0];
    var data = this.vcl.invorkBcf('MonthPlanImportData', [fileName, worknoArray, masterRow.data['TYPEID'], masterRow.data['SCHEDULEPRODUCEID']], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success)
        var grid = Ext.getCmp(this.vcl.winId + 'PLSPRODUCEMONTHPLANDETAILGrid');
    var k = 0;
    for (var j = 0; j < data.length; j++) {
        var DetailRow = this.vcl.addRowForGrid(grid);
        DetailRow.set("BILLNO", masterRow.data['BILLNO']);
        DetailRow.set("ROW_ID", this.vcl.dataSet.dataList[1].MaxRowId);
        DetailRow.set("WORKNO", data[j].WorkNo);
        DetailRow.set("SALBILLNO", data[j].SalBillNo);
        DetailRow.set("CUSTOMERID", data[j].CustomerId);
        DetailRow.set("MATERIALID", data[j].MaterialId);
        DetailRow.set("QUANTITY", data[j].Quantity);
        DetailRow.set("SALDATE", data[j].SalDate);
        DetailRow.set("DELIVERYDATE", data[j].DeliveryDate);
        DetailRow.set("PLANFINISHTIME", data[j].PlanFinishTime);
        DetailRow.set("BOMID", data[j].BomId);
       
        var subStores = [];
        var curStore = this.vcl.dataSet.getTable(1);
        for (var i = 2; i < this.vcl.dataSet.dataList.length; i++) {
            var table = this.vcl.dataSet.dataList[i];
            if (table.Name.indexOf('DYTABLE') == 0) {
                var parentIndex = table['ParentIndex'];
                var parentTable = this.vcl.dataSet.getTable(parentIndex);
                var model = this.vcl.dataSet.getChildren(parentIndex, DetailRow, i);
                if (model && model.length > 0) {
                    subStores.push(model[0]);
                }
            }
        }
        curStore.suspendEvents();
        var ret = data[j].PlanTime
        try {
            for (p in ret) {
                if (!ret.hasOwnProperty(p))
                    continue;
                var value = ret[p];
                if (value !== undefined) {
                    if (DetailRow.data[p] !== undefined) {
                        DetailRow.set(p, value);
                    } else {
                        for (var r = 0; r < subStores.length; r++) {
                            if (subStores[r].data[p] !== undefined)
                                subStores[r].set(p, value);
                        }
                    }
                }
            }
        } finally {
            curStore.resumeEvents();
            if (curStore.ownGrid)
                curStore.ownGrid.reconfigure(curStore);
        }

    }
    return success;
}
