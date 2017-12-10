Ax.utils.LibAttributeForm = {
    show: function (curVcl, self, dataInfo) {
        var me = this;
        var vcl = curVcl;
        var curAttrId = dataInfo.dataRow.get(self.attrField);
        var curAttrCode = dataInfo.dataRow.get(self.attrCode);
        var fields, renderer, newRowObj, relationMark, intervalMark;
        function ajaxCall() {
            Ext.Ajax.request({
                url: 'billSvc/getAttrControl',
                method: 'POST',
                jsonData: {
                    attrId: curAttrId, attrCode: curAttrCode
                },
                async: false,
                timeout: 90000000,
                success: function (response) {
                    var ret = Ext.decode(response.responseText);
                    ret = Ext.decode(ret.GetAttributeControlResult);
                    fields = ret.Fields;
                    renderer = Ext.decode(ret.Renderer);
                    newRowObj = Ext.decode(ret.NewRowObj);
                    relationMark = ret.RelationMark;
                    intervalMark = ret.IntervalMark;
                }
            });
        }
        ajaxCall();
        var modelType = Ext.define(Ext.id(), {
            extend: 'Ext.data.Model',
            fields: Ext.decode(fields),
            proxy: {
                type: 'memory',
                reader: {
                    type: 'json'
                }
            }
        });
        var store = Ext.create('Ext.data.Store', {
            model: modelType,
            proxy: {
                type: 'memory',
                reader: {
                    type: 'json'
                }
            }
        });
        var panel = Ext.create('Ext.form.Panel', {
            border: false,
            tableIndex: 0,
            margin: '6 2 6 2',
            items: renderer
        });
        var newModel = store.add(newRowObj)[0];
        panel.loadRecord(newModel);
        var win = Ext.create('Ext.window.Window', {
            title: '特征信息',
            autoScroll: true,
            layout: 'fit',
            constrainHeader: true,
            minimizable: false,
            maximizable: false,
            modal: true,
            vcl: vcl,
            tbar: Ext.create('Ext.toolbar.Toolbar', {
                items: [{
                    text: '确定',
                    handler: function () {
                        if (vcl.isEdit) {
                            var attrCode = '';
                            var attrDesc = '';
                            var relationMarkStr = relationMark == 0 ? ":" : "=";
                            panel.updateRecord(newModel);
                            var list = newModel.fields.items;
                            var destList = renderer[0].items;
                            for (var i = 0; i < list.length - 1; i++) {
                                var value = newModel.get(list[i].name);
                                attrCode += value;
                                var dest = destList[i];
                                var text = '';
                                if (dest.xtype == 'libComboboxField')
                                    text = dest.store.query('key', value).items[0].data['value'];
                                else
                                    text = dest.getValue();
                                //"[]", ";", "换行"
                                switch (intervalMark) {
                                    case 0:
                                        attrDesc += '【' + list[i].name + relationMarkStr + text + '】';
                                        break;
                                    case 1:
                                        attrDesc += list[i].name + relationMarkStr + text + ';';
                                        break;
                                    case 2:
                                        attrDesc += list[i].name + relationMarkStr + text + '\n';
                                        break;
                                }
                            }
                            dataInfo.dataRow.set(self.attrCode, attrCode);
                            dataInfo.dataRow.set(dataInfo.fieldName, attrDesc);
                            if (dataInfo.curForm)
                                dataInfo.curForm.loadRecord(dataInfo.dataRow);
                            vcl.vclHandler(self, { libEventType: LibEventTypeEnum.ConfirmAttribute, dataInfo: dataInfo });
                        }
                        win.close();
                    }
                }, {
                    text: '取消',
                    handler: function () {
                        win.close();
                    }
                }]
            }),
            items: [panel]
        });
        win.show();
    }
};
