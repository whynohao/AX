
//自定义的libSearchField数据模型和展示模板
var customFuzzySearchTemplate_ = function (libSearchField, tableIndex, fieldName) {
    if (fieldName != 'MATERIALID')
        return;
    var modelType = Ext.data.Model.schema.getEntity('MaterialModel');
    if (modelType === null) {
        modelType = Ext.define("MaterialModel", {
            extend: "Ext.data.Model",
            fields: [
                { name: 'Id' },
                { name: 'Name' },
                { name: 'SPECIFICATION' },
                { name: 'TEXTUREID' },
                { name: 'FIGURENO' }
            ]
        });
    }

    var materialStore = Ext.create('Ext.data.Store', {
        model: modelType,
        proxy: {
            type: 'memory',
            reader: {
                type: 'json'
            }
        },
        sorters: [{ property: 'Id', direction: 'DESC' }]
    });
    libSearchField.valueField = 'Id';
    libSearchField.store = materialStore;

    libSearchField.displayTpl = Ext.create('Ext.XTemplate',
                           '<tpl for=".">',
                                '<tpl if="Id != &quot;&quot; && Id != undefined>',
                                   '{Id},{Name},{SPECIFICATION},{TEXTUREID},{FIGURENO}',
                                '</tpl>',
                           '</tpl>'
    );

    libSearchField.listConfig = {
        loadingText: '搜索中...',
        emptyText: '没有匹配的数据',
        tpl: Ext.create('Ext.XTemplate', '<ul><tpl for=".">',
            '<li role="option" class="x-boundlist-item">',
                '<tpl if="Id != &quot;&quot; && Id != undefined>',
                    '{Id:this.highlight(true)},{Name:this.highlight(true)},{SPECIFICATION:this.highlight(true)},{TEXTUREID:this.highlight(true)},{FIGURENO:this.highlight(true)}',
                '</tpl>',
            '</li>',
            '</tpl></ul>',
            {
                highlight: function (v) {
                    query = this.field.lastQuery;
                    if (Ext.isEmpty(query)) {
                        return v;
                    } else {
                        //高亮
                        if (v) {
                            var t = v;
                            if (query.indexOf(',') != -1) {
                                var t1 = query.split(",");
                                for (var i = 0; i < t1.length; i++) {
                                    if (t1[i] != '') {
                                        t = t.replace(new RegExp(t1[i], 'gi'), function (m) {
                                            return "<font color='red'>" + m + "</font>";
                                        });
                                    }
                                }
                            } else {
                                t = v.replace(new RegExp(query, 'gi'), function (m) {
                                    return "<font color='red'>" + m + "</font>";
                                });
                            }
                            return t;
                        }
                    }
                }
            }
        )
    };

    libSearchField.bindStore(libSearchField.store || 'ext-empty-store', true, true);
}
//自定义的libSearchField后台数据查询
var customFuzzySearch_ = function (libSearchField, tableIndex, name, realRelSource, relName, queryString, curPks, selConditionParam) {
    if (realRelSource == "com.Material" && name == "MATERIALID") {
        var docVcl = Ax.utils.LibVclSystemUtils.getVcl("com.Material", BillTypeEnum.Master);
        var data = docVcl.invorkBcf('MaterialSearchField', [tableIndex, name, realRelSource, relName, queryString, libSearchField.win.progName, curPks, selConditionParam]);
        libSearchField.store.loadData(data);
    } else {
        data = this.win.vcl.invorkBcf('FuzzySearchField', [tableIndex, name, realRelSource, relName, queryString, curPks, selConditionParam]);
        libSearchField.store.loadData(data);
    }
}

var mylist = new Array("MATERIALID", "WAREHOUSEID", "STORAGEID", "BATCHNO", "CONTACTOBJECTID");

var getInventroyQty = function (dataRow) {
    var materialId = "", warehoseId = "", storageId = "", batchNo = "";
    if (dataRow.get("MATERIALID") != null && dataRow.get("MATERIALID") != "") {
        materialId = dataRow.get("MATERIALID");
    }
    if (dataRow.get("WAREHOUSEID") != null && dataRow.get("WAREHOUSEID") != "") {
        warehoseId = dataRow.get("WAREHOUSEID");
    }
    if (dataRow.get("STORAGEID") != null && dataRow.get("STORAGEID") != "") {
        storageId = dataRow.get("STORAGEID");
    }
    if (dataRow.get("BATCHNO") != null && dataRow.get("BATCHNO") != "") {
        batchNo = dataRow.get("BATCHNO");
    }
    //if (dataRow.get("CONTACTOBJECTID") != null && dataRow.get("CONTACTOBJECTID") != "") {
    //    contactsObjectId = dataRow.get("CONTACTOBJECTID");
    //}
    if (materialId != "") {
        var docVcl = Ax.utils.LibVclSystemUtils.getVcl("stk.AccountDetail", BillTypeEnum.Grid);
        var data = docVcl.invorkBcf('GetInventoryQty', [materialId, warehoseId, storageId, batchNo]);
        dataRow.set("INVENTORYQTY", data);
    }
}

Ext.override(Ax.vcl.LibVclData, {
    customFuzzySearchTemplate:
        Ext.Function.createSequence(
        Ax.vcl.LibVclData.prototype.customFuzzySearchTemplate, customFuzzySearchTemplate_)
});

Ext.override(Ax.vcl.LibVclData, {
    customFuzzySearch:
        Ext.Function.createSequence(
        Ax.vcl.LibVclData.prototype.customFuzzySearch, customFuzzySearch_)
});