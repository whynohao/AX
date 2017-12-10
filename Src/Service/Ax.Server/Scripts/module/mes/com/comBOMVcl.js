comBomVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    //第一次打开
    this.firstLoad = true;
    //树
    this.tree;
    this.copyData = {};
};
var proto = comBomVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comBomVcl;

proto.getBindingRow = function (e) {
    var items = this.dataSet.dataList[1].data.items;
    var br;
    for (var i = 0; i < items.length; i++) {
        if (e.dataInfo.dataRow.data["ROW_ID"] == items[i].data["ROW_ID"]) {
            br = items[i];
            break;
        }
    }
    return br;
}
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex != 1) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.DblClick:
            //if (e.dataInfo.fieldName == "MATERIALID") {
            //    Ax.utils.LibVclSystemUtils.openDataFunc("com.MateialDataFunc", "物料数据加载", [this, this.selectMaterial]);
            //}
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "MATERIALID") {
                    var dataRow = e.dataInfo.dataRow.data;
                    var parms = new Array();
                    parms["MATERIALNAME"] = dataRow["MATERIALNAME"];
                    //parms["FIGURENO"] = dataRow["FIGURENO"];
                    parms["SPECIFICATION"] = dataRow["SPECIFICATION"];
                    parms["TEXTUREID"] = dataRow["TEXTUREID"];
                    parms["FIGURENO1"] = dataRow["FIGURENO1"];
                    parms["ISSTANDARD"] = 1;
                    Ax.utils.LibVclSystemUtils.openDataFunc("com.MateialDataFunc", "物料数据加载", [this, e.dataInfo.dataRow, this.selectMaterial, parms]);
                }
            }
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.dataRow.data["ROW_ID"] == 0) {
                    e.dataInfo.cancel = true;
                    Ext.Msg.alert("提示", "根目录不能修改！");
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                var bindingRow = e.dataInfo.dataRow.bindingRow || this.getBindingRow.call(this, e);
                if (bindingRow) {
                    bindingRow.set(e.dataInfo.fieldName, e.dataInfo.value);
                    switch (e.dataInfo.fieldName) {
                        case "MATERIALID":
                            e.dataInfo.dataRow.set("NODENAME", e.dataInfo.dataRow.data["MATERIALNAME"]);
                            for (var item in e.dataInfo.dataRow.data) {
                                if (bindingRow.data.hasOwnProperty(item)) {
                                    bindingRow.set(item, e.dataInfo.dataRow.get(item));
                                }
                            }

                            if (e.dataInfo.dataRow.data['ISATTRDETAIL']) {
                                var pid = e.dataInfo.dataRow.data["ROW_ID"];
                                var store = this.dataSet.getTable(6);
                                store.clearFilter();
                                var items = store.data.items;
                                for (var i = items.length - 1; i >= 0; i--)
                                    items[i].get("GRANDPARENTROWID") == pid && store.data.removeAt(i);
                                store = this.dataSet.getTable(3);
                                store.clearFilter();
                                items = store.data.items;
                                for (var i = items.length - 1; i >= 0; i--) {
                                    if (items[i].get("GRANDPARENTROWID") == pid) {
                                        this.deleteRow(3, items[i]);
                                    }
                                }
                                store = this.dataSet.getTable(2);
                                store.clearFilter();
                                items = store.data.items;
                                for (var i = items.length - 1; i >= 0; i--) {
                                    if (items[i].get("PARENTROWID") == pid) {
                                        this.deleteRow(2, items[i]);
                                    }
                                }
                            }
                            e.dataInfo.dataRow.set("ISATTRDETAIL", false);
                            bindingRow.set("ISATTRDETAIL", false);
                            if (e.dataInfo.value == "") {
                                e.dataInfo.dataRow.set("ATTRIBUTEID", "");
                            }
                            e.dataInfo.dataRow.set("ISATTRDETAIL", i > 1);
                            bindingRow.set("ISATTRDETAIL", i > 1);

                            break;
                        case 'MATERIALTYPEID':
                            if (e.dataInfo.dataRow.data['ISATTRDETAIL']) {
                                var pid = e.dataInfo.dataRow.data["ROW_ID"];
                                var store = this.dataSet.getTable(6);
                                store.clearFilter();
                                var items = store.data.items;
                                for (var i = store.data.length - 1; i >= 0; i--)
                                    items[i].get("GRANDPARENTROWID") == pid && store.data.removeAt(i);
                                store = this.dataSet.getTable(3);
                                store.clearFilter();
                                items = store.data.items;
                                for (var i = items.length - 1; i >= 0; i--) {
                                    if (items[i].get("GRANDPARENTROWID") == pid) {
                                        this.deleteRow(3, items[i]);
                                    }
                                }
                                store = this.dataSet.getTable(2);
                                store.clearFilter();
                                items = store.data.items;
                                for (var i = items.length - 1; i >= 0; i--) {
                                    if (items[i].get("PARENTROWID") == pid) {
                                        this.deleteRow(2, items[i]);
                                    }
                                }
                            }
                            e.dataInfo.dataRow.set("ISATTRDETAIL", false);
                            bindingRow.set("ISATTRDETAIL", false);
                            if (e.dataInfo.value == "") {
                                e.dataInfo.dataRow.set("ATTRIBUTEID", "");
                            }
                            bindingRow.set("ATTRIBUTEID", e.dataInfo.dataRow.data["ATTRIBUTEID"]);
                            //新增特征明细
                            var attrDetail = this.invorkBcf('GetAttrDetail', [e.dataInfo.dataRow.data["ATTRIBUTEID"]]);
                            var i = 1;
                            for (var attrItem in attrDetail) {
                                var newRow = this.addRow(bindingRow, 2);
                                newRow.set('ATTRIBUTEITEMID', attrItem);
                                newRow.set('ATTRIBUTEITEMNAME', attrDetail[attrItem][0]);
                                newRow.set('ATTRIBUTECODELEN', attrDetail[attrItem][1]);
                                newRow.set('VALUECALTYPE', attrDetail[attrItem][2]);
                                i++;
                            }
                            e.dataInfo.dataRow.set("ISATTRDETAIL", i > 1);
                            bindingRow.set("ISATTRDETAIL", i > 1);
                            break;
                        case "TECHROUTEROWID":
                            bindingRow.set("WORKPROCESSID", e.dataInfo.dataRow.get("WORKPROCESSID"));
                            bindingRow.set("WORKSHOPSECTIONID", e.dataInfo.dataRow.get("WORKSHOPSECTIONID"));
                            break;
                    }
                }
            }
            break;
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.dataRow.data["ROW_ID"] == 0) {
                    e.dataInfo.cancel = true;
                    Ext.Msg.alert("提示", "根目录不能修改！");
                    return;
                }

                if (this.isEdit) {
                    var bindingRow = e.dataInfo.dataRow.bindingRow || this.getBindingRow.call(this, e);
                    switch (e.dataInfo.fieldName) {
                        case "MATERIALNAME":
                            var dataRow = e.dataInfo.dataRow.data;
                            var parms = new Array();
                            parms["MATERIALNAME"] = dataRow["MATERIALNAME"];
                            //parms["FIGURENO"] = dataRow["FIGURENO"];
                            parms["SPECIFICATION"] = dataRow["SPECIFICATION"];
                            parms["TEXTUREID"] = dataRow["TEXTUREID"];
                            parms["ISSTANDARD"] = 1;
                            Ax.utils.LibVclSystemUtils.openDataFunc("com.MateialDataFunc", "物料数据加载", [this, e, this.selectRowMaterial, parms]);
                            break;
                    }
                }
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BomChange") {
                var masterRow = this.dataSet.getTable(0).data.items[0];//表头
                var billNo = masterRow.data["BOMID"];
                var result = this.invorkBcf('CreateProductBom', [billNo]);
                if (result.MessageList.length > 0) {
                    var ex = [];
                    for (var i = 0; i < result.MessageList.length; i++) {
                        var msgKind = result.MessageList[i].MessageKind;
                        ex.push({ kind: msgKind, msg: result.MessageList[i].Message });
                    }
                    Ax.utils.LibMsg.show(ex);
                }
                else {
                    Ext.Msg.alert("系统提示", "该BOM已启动并生成生产BOM！");
                }
            }
            else if (e.dataInfo.fieldName == "RandomData") {
                if (!this.isEdit) {
                    var contactsObjectid = this.dataSet.getTable(0).data.items[0].data['FROMBILLNO'];
                    var contactsObjectname = this.dataSet.getTable(0).data.items[0].data['CONTRACTNO'];
                    Ax.utils.LibVclSystemUtils.openDataFunc("com.ProductOrderDataFunc", "随机资料查询", [this, contactsObjectid, contactsObjectname]);
                }
                else {
                    Ext.Msg.alert("系统提示", "编辑状态才能使用数据加载按钮！");
                }
            }
        case LibEventTypeEnum.FormClosed:
            break;
    }
}

proto.selectMaterial = function (This, obj, recode) {
    var dataInfo = recode[0];
    var masterRow = obj;
    masterRow.set("MATERIALID", dataInfo.MaterialId);
    masterRow.set("MATERIALNAME", dataInfo.MaterialName);
    masterRow.set("FIGURENO", dataInfo.FigureNo);
    masterRow.set("FIGURENO1", dataInfo.FigureNo1);
    masterRow.set("SPECIFICATION", dataInfo.Specification);
    masterRow.set("TEXTUREID", dataInfo.Textureid);
    This.forms[0].loadRecord(masterRow);
};

proto.selectRowMaterial = function (This, obj, recode) {
    var dataInfo = recode[0];
    var e = obj;
    var masterRow = e.dataInfo.dataRow;
    masterRow.set("MATERIALID", dataInfo.MaterialId);
    masterRow.set("MATERIALNAME", dataInfo.MaterialName);
    masterRow.set("FIGURENO", dataInfo.FigureNo);
    masterRow.set("SPECIFICATION", dataInfo.Specification);
    masterRow.set("UNITID", dataInfo.UnitId);
    masterRow.set("UNITNAME", dataInfo.UnitName);
    masterRow.set("MATSTYLE", dataInfo.Matstyle);
    masterRow.set("FIGURENO", dataInfo.FigureNo);
    masterRow.set("TEXTUREIDID", dataInfo.Textureidid);
    masterRow.set("NODENAME", dataInfo.MaterialName);
    var bindingRow = masterRow.bindingRow || This.getBindingRow.call(This, e);
    for (var item in masterRow.data) {
        if (bindingRow.data.hasOwnProperty(item)) {
            bindingRow.set(item, masterRow.get(item));
        }
    }
}

//ISATTRDETAIL/将数据源转为树形的数据对象
proto.GetTreeStoreData = function (gen, fillStore, rowId, bomLevel) {//bomLevel 层级
    var obj = [];
    if (gen) {
        var items = this.GetTreeStoreData(false, fillStore, rowId, 1);

        var charString = {};
        var record = this.dataSet.getTable(1).data.items[0];
        if (record != undefined) {
            for (var key in record.data) {
                charString[key] = "";
            }

            charString["expanded"] = true;//是否展开层级
            charString["children"] = items;//是否有下一层
            obj.push(charString);
        }
    }
    else {
        var models = this.GetModelCollection(fillStore, rowId);
        for (var i = 0; i < models.length; i++) {
            var record = models[i];
            var items = this.GetTreeStoreData.call(this, false, fillStore, record.data['ROW_ID'], bomLevel + 1);
            record.data['ORDERNUM'] == 0 && (record.data['ORDERNUM'] = i + 1);
            record.data['BOMLEVEL'] == 0 && (record.data['BOMLEVEL'] = bomLevel);

            var charString = {};
            for (var key in record.data) {
                charString[key] = record.data[key];
            }
            charString["expanded"] = false;//是否展开层级
            charString["children"] = items;//是否有下一层
            obj.push(charString);
        }
    }
    return obj;
}

//取行数据
proto.GetModelCollection = function (fillStore, rowid) {
    var items = fillStore.data.items;
    var mc = [];
    for (var i = 0; i < fillStore.data.length; i++) {
        if (items[i].data['PARENTROWID'] == rowid) {
            mc.push(items[i]);
        }
    }
    return mc;
}

proto.GetModelAndSonCollection = function (fillStore, rowid, modelCollection) {
    var items = fillStore.data.items;
    for (var i = 0; i < fillStore.data.length; i++) {
        if (items[i].data['PARENTROWID'] == rowid) {
            modelCollection.push(items[i]);
            this.GetModelAndSonCollection(fillStore, items[i].get("ROW_ID"), modelCollection);
        }
    }
}

//撤销
proto.cancel = function () {
    this.dataSet.rejectChanges();
    var masterRow = this.dataSet.getTable(0).data.items[0];
    for (var i = 0; i < this.forms.length; i++) {
        this.forms[i].loadRecord(masterRow);
    };
    this.tree.setRootNode(this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0)[0]);
};

//保存方案
proto.saveDisplayScheme = function () {
    var needSave = false;
    var displayScheme = { ProgId: this.progId, GridScheme: {} };
    for (var i = 0; i < this.dataSet.dataList.length; i++) {
        if (i == 1) {
            if (!needSave)
                needSave = true;
            var gridScheme = { GridFields: [] };
            var columns = this.tree.headerCt.items.items;
            if (columns.length == 0) {
                gridScheme = this.subGridScheme[i];
            } else {
                var buildBandCol = function (bandColumn, list) {
                    list.push({ Field: { Name: bandColumn.dataIndex, Width: bandColumn.width } });
                }
                for (var l = 0; l < columns.length; l++) {
                    if (columns.xtype == "rownumberer" || columns[l].hidden === true)
                        continue;
                    buildBandCol(columns[l], gridScheme.GridFields);
                }
            }
            if (gridScheme != undefined)
                displayScheme.GridScheme[i] = gridScheme;
        }
        else {
            if (this.dataSet.dataList[i].ownGrid) {
                if (!needSave)
                    needSave = true;
                var gridScheme = { GridFields: [] };
                var gridPanel = this.dataSet.dataList[i].ownGrid;
                var columns = gridPanel.headerCt.items.items;
                if (columns.length == 0) {
                    gridScheme = this.subGridScheme[i];
                } else {
                    var buildBandCol = function (bandColumn, list) {
                        if (bandColumn.items.items.length > 0) {
                            var subList = [];
                            list.push({ Header: bandColumn.text, BandFields: subList });
                            for (var r = 0; r < bandColumn.items.items.length; r++) {
                                if (bandColumn.items.items[r].hidden === true)
                                    continue;
                                buildBandCol(bandColumn.items.items[r], subList);
                            }
                        }
                        else {
                            list.push({ Field: { Name: bandColumn.dataIndex, Width: bandColumn.width } });
                        }
                    }
                    for (var l = 0; l < columns.length; l++) {
                        if (columns.xtype == "rownumberer" || columns[l].hidden === true)
                            continue;
                        buildBandCol(columns[l], gridScheme.GridFields);
                    }
                }
                if (gridScheme != undefined)
                    displayScheme.GridScheme[i] = gridScheme;
            }
        }
    }
    if (needSave) {
        var call = function (displayScheme) {
            Ext.Ajax.request({
                url: '/billSvc/saveDisplayScheme',
                jsonData: { handle: UserHandle, progId: this.progId, entryParam: Ext.encode(this.entryParam), displayScheme: Ext.encode(displayScheme) },
                method: 'POST',
                async: false,
                timeout: 90000000
            });
        }
        call.apply(this, [displayScheme]);
    }
}

//刷新，多用户的时候，其中一个用户更改了数据，其他用户需要点击刷新才能刷新树
proto.browseTo = function (condition) {
    var data = this.invorkBcf("BrowseTo", [condition]);
    this.setDataSet(data, false);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    for (var i = 0; i < this.forms.length; i++) {
        this.forms[i].loadRecord(masterRow);
    };
    if (!this.firstLoad)
        this.tree.setRootNode(this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0)[0]);

};

//导入
proto.importData = function (fileName) {
    var assistObj = {};
    var data = this.invorkBcf('ImportData', [fileName, this.entryParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success) {
        this.restData(false, BillActionEnum.Browse, data);
        this.tree.setRootNode(this.GetTreeStoreData(true, this.dataSet.getTable(1), 0, 0)[0]);
    }
    return success;
}

proto.fillCopyData = function (dataRow, parentItem, isCurrentRow) {
    var copyItem = {};
    copyItem.dataRow = dataRow;
    copyItem.attributesDetail = new Array();
    copyItem.children = new Array();
    var attributeDetails = this.dataSet.getChildren(1, dataRow, 2);
    for (var i = 0; i < attributeDetails.length; i++) {
        var attributeDetailRow = attributeDetails[i];
        var attributesDetail = {};
        attributesDetail.dataRow = attributeDetailRow;
        attributesDetail.noMatchParamDetails = new Array();
        //直接找第4张是没有数据的，应该还是找第6张表
        var store = this.dataSet.getTable(6);
        for (var j = 0; j < store.data.items.length; j++) {
            var record = store.data.items[j];
            if (record.get("STRUCTBOMID") == attributeDetailRow.get("STRUCTBOMID") && record.get("GRANDPARENTROWID") == attributeDetailRow.get("PARENTROWID") && record.get("PARENTROWID") == attributeDetailRow.get("ROW_ID")) {
                attributesDetail.noMatchParamDetails.push(record);
            }
        }
        copyItem.attributesDetail.push(attributesDetail);
    }
    if (isCurrentRow) {
        this.copyData = copyItem;
    }
    else {
        parentItem.children.push(copyItem);
    }
    var childrenRows = this.GetModelCollection(this.dataSet.getTable(1), dataRow.get("ROW_ID"));
    for (var i = 0; i < childrenRows.length; i++) {
        this.fillCopyData(childrenRows[i], copyItem, false);
    }

}
proto.pasteData = function (currNode, parentNode, currCopyItem, isCurrentNode) {
    var headerRow = this.dataSet.getTable(0).data.items[0];
    var newRow = this.addRow(headerRow, 1);
    var copyItem, bindingRow;
    if (isCurrentNode) {
        if (this.copyData.children) {
            copyItem = this.copyData;
        }
        else {
            var copyData = DesktopApp.copyData;
            if (copyData) {
                copyItem = copyData;
            }
        }
        bindingRow = currNode.bindingRow || this.dataSet.FindRow(1, currNode.get("ROW_ID"));
        newRow.set("PARENTROWID", bindingRow.get("PARENTROWID"));
        newRow.set("BOMLEVEL", bindingRow.get("BOMLEVEL"));
    }
    else {
        currNode = parentNode;
        copyItem = currCopyItem;
        bindingRow = currNode.bindingRow || this.dataSet.FindRow(1, currNode.get("ROW_ID"));
        newRow.set("PARENTROWID", bindingRow.get("ROW_ID"));
        newRow.set("BOMLEVEL", bindingRow.get("BOMLEVEL") + 1);
    }
    for (var fieldName in copyItem.dataRow.data) {
        if (fieldName != "STRUCTBOMID" && fieldName != "PARENTROWID" && fieldName != "ROW_ID" && fieldName != "BOMLEVEL" && fieldName != "id") {
            newRow.set(fieldName, copyItem.dataRow.get(fieldName));
        }
    }
    for (var i = 0 ; i < copyItem.attributesDetail.length; i++) {
        var attributesDetailRow = copyItem.attributesDetail[i];
        var attributesNewRow = this.addRow(newRow, 2);
        for (var fieldName in attributesDetailRow.dataRow.data) {
            if (fieldName != "STRUCTBOMID" && fieldName != "PARENTROWID" && fieldName != "ROW_ID" && fieldName != "id") {
                attributesNewRow.set(fieldName, attributesDetailRow.dataRow.get(fieldName));
            }
        }
        for (var k = 0; k < attributesDetailRow.noMatchParamDetails.length; k++) {
            var noMatchParamDetailRow = attributesDetailRow.noMatchParamDetails[k];
            var noMatchParamDetailNewRow = this.addRow(attributesNewRow, 3);
            for (var fieldName in noMatchParamDetailRow.data) {
                if (fieldName != "STRUCTBOMID" && fieldName != "GRANDPARENTROWID" && fieldName != "PARENTROWID" && fieldName != "ROW_ID" && fieldName != "id" && fieldName != "FROMTABLE") {
                    noMatchParamDetailNewRow.set(fieldName, noMatchParamDetailRow.get(fieldName));
                }
            }
            var store = this.dataSet.getTable(6);
            var newRow1 = Ext.decode(this.tpl.Tables[this.dataSet.getTable(6).Name].NewRowObj);
            newRow1["STRUCTBOMID"] = noMatchParamDetailNewRow.get("STRUCTBOMID");
            newRow1["GRANDPARENTROWID"] = noMatchParamDetailNewRow.get("GRANDPARENTROWID");
            newRow1["PARENTROWID"] = noMatchParamDetailNewRow.get("PARENTROWID");
            newRow1["ROW_ID"] = noMatchParamDetailNewRow.get("ROW_ID");
            newRow1["PARAMID"] = noMatchParamDetailNewRow.get("PARAMID");
            newRow1["PARAMNAME"] = noMatchParamDetailNewRow.get("PARAMNAME");
            newRow1["PARAMVALUE"] = noMatchParamDetailNewRow.get("PARAMVALUE");
            newRow1["PARAMVALUENAME"] = noMatchParamDetailNewRow.get("PARAMVALUENAME");
            var newModel = store.add(newRow1)[0];
        }
    }
    var newNode = {};
    for (var fieldName in newRow.data) {
        newNode[fieldName] = newRow.get(fieldName);
    }
    newNode.expanded = true;
    if (isCurrentNode) {
        if (currNode.parentNode == null) {
            newNode = currNode.appendChild(newNode);
        } else {
            newNode = currNode.parentNode.appendChild(newNode);
        }

    }
    else {
        newNode = currNode.appendChild(newNode);
    }
    for (var i = 0 ; i < copyItem.children.length; i++) {
        this.pasteData(currNode, newNode, copyItem.children[i], false);
    }
}

//通过物料ID  获取结构BOM ID
proto.getBomID = function (materialID) {
    return this.invorkBcf('GetBomID', [materialID]);
}