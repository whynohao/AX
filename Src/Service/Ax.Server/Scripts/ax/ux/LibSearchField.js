Ext.define('Ax.ux.form.LibSearchField', {
    extend: 'Ext.form.field.ComboBox',
    alias: 'widget.libSearchfield',
    triggerCls: Ext.baseCSSPrefix + 'form-search-trigger',
    mixins: { eventHelper: 'Ax.ux.LibEventHelper' },
    selectOnFocus: true,
    remoteData: null,
    autoSelect: true,
    initComponent: function () {
        this.enableKeyEvents = true,
          this.fieldType = 'string';
        this.id = this.name + this.tableIndex + '_' + DesktopApp.ActiveWindow;
        var cmptid = this.id
        var modelType = Ext.data.Model.schema.getEntity('FuzzyModel');
        if (modelType === null) {
            modelType = Ext.define("FuzzyModel", {
                extend: "Ext.data.Model",
                fields: [
                  { name: 'Id' },
                  { name: 'Name' },
                ]
            });
        }

        var fuzzyStore = Ext.create('Ext.data.Store', {
            model: modelType,
            proxy: {
                type: 'memory',
                reader: {
                    type: 'json'
                }
            },
            sorters: [{ property: 'Id', direction: 'DESC' }]
        });
        this.valueField = 'Id',
          this.store = fuzzyStore;
        this.minChars = '1';
        this.displayTpl = Ext.create('Ext.XTemplate',
          '<tpl for=".">',
          '<tpl if="Id != &quot;&quot; && Id != undefined && Name !=&quot;&quot; && Name != undefined">',
          '{Id},{Name}',
          '</tpl>',
          '<tpl if="Id != &quot;&quot; && Id != undefined && (Name ==&quot;&quot; || Name == undefined)">',
          '{Id}',
          '</tpl>',
          '</tpl>'
        );
        this.listConfig = {
            loadingText: '搜索中...',
            emptyText: '没有匹配的数据',
            tpl: Ext.create('Ext.XTemplate', '<ul style:"width:auto"><tpl for=".">',
              '<li role="option"  style="font-family:' + this.fontName + '"  class="x-boundlist-item">{Id:this.highlight(true)}{Name:this.highlight(false)}</li>',
              '</tpl></ul>',
              {
                  highlight: function (v, bool) {
                      query = '';
                      if (this.field != null)
                          query = this.field.lastQuery;
                      if (Ext.isEmpty(query)) {
                          if (!bool && v != "") {
                              return ',' + v;
                          }
                          return v;
                      } else {
                          return v.replace(new RegExp(query, 'gi'), function (m) {
                              return "<font color='red'>" + m + "</font>";
                          });
                      }
                  }
              }
            )
        };
        this.listeners = {
            keydown: function (self, e, eOpts) {
                this.keydown(self, e, eOpts)
            },
            select: function (field, records, eOpts) {
                if (!this.readOnly) {
                    if (this.win === undefined)
                        this.win = self.up('window');
                    if (this.win === undefined)
                        this.win = self.up('[isVcl=true]');
                    if (this.win && this.win.vcl && (!this.win.vcl.isEdit && this.win.vcl.proxy !== true)) {
                        // e.stopEvent();
                    } else {
                        var form = field.up('form');
                        if (form) {
                            var curRecord = form.getRecord();
                            if (curRecord && field.relName) {
                                curRecord.set(field.relName, records.get('Name'));
                            }
                        }
                    }
                }
            },
            beforerender: function (field, eOpts) {
                var curForm = field.up('form');
                var record;
                if (curForm) {
                    record = field.up('form').getRecord();
                } else {
                    var grid = field.up('grid') || field.up("treepanel");
                    if (grid) {
                        record = grid.getView().getSelectionModel().getLastSelected();
                    }
                }
                if (record != undefined) {
                    var key = record.get(field.name);
                    if (key != '') {
                        this.store.add({ Id: key, Name: record.get(field.relName) });
                        this.select(key);
                        this.fieldType = typeof (key);
                    }
                }
            },
            afterrender: function (field, eOpts) {
                var inputDom = field.getEl().dom.getElementsByTagName('INPUT')[0];
                inputDom.style = "font-family:" + this.fontName;
            },
            blur: function (self, e, eOpts) {
                var me = this;
                if (!me.readOnly) {
                    if (me.up('form')) {
                        if (!me.validating(me)) {
                            me.focus(false, true);
                            return;
                        }
                        if (me.rawValue == "") {
                            me.setValue(me.rawValue);
                        }
                    }
                    else {
                        self.setValue(self.value);
                        if (self.rawValue == "") {
                            self.setValue(self.rawValue);
                        }
                    }
                }
            },
            focus: function (self, The, eOpts) {
                var curGrid = self.up('grid') || self.up("treepanel");
                if (curGrid) {
                    self.dataRow = curGrid.getSelectionModel().getLastSelected();
                    var record = self.dataRow;
                    if (record != undefined && record.data[self.name] != '') {
                        self.store.add({ Id: record.data[self.name], Name: record.data[self.relName] });
                        self.select(record.data[self.name]);
                    }
                }
                var value = self.getValue();
                if (value)
                    self.selectText(0, value.length);

            },
            keyup: function (self, e, eOpts) {
                if (e.keyCode == e.F4) {
                    var dataRow, tableIndex, curGrid;
                    var form = self.up('form');
                    if (form) {
                        dataRow = form.getRecord();
                        tableIndex = form.tableIndex;
                    } else {
                        curGrid = self.up('grid') || self.up("treepanel");
                        if (curGrid) {
                            dataRow = curGrid.getView().getSelectionModel().getLastSelected();
                            tableIndex = curGrid.tableIndex;
                        }
                    }
                    var dataInfo = Ax.Control.LibDataInfo.getDataInfo(this);
                    var realRelSource;
                    if (this.win === undefined)
                        this.win = self.up('window');
                    if (this.win === undefined)
                        this.win = self.up('[isVcl=true]');
                    if (this.realRelSource) {
                        realRelSource = this.realRelSource;
                    }
                    else {
                        var obj = {};
                        realRelSource = Ax.utils.LibVclSystemUtils.getRelSource(this, dataInfo, this.win.vcl, obj);
                        if (obj.hasRealRelSource) {
                            this.realRelSource = realRelSource;
                        }
                    }
                    if (this.name.indexOf("ROW") == -1) {
                        this.win.vcl.doF4(tableIndex, this.name, realRelSource, this.relPk, this.getValue(), dataRow, curGrid);
                    }
                }
            },
            expand: function (field, eOpts) {
                if (!this.readOnly) {
                    if (this.win === undefined)
                        this.win = self.up('window');
                    if (this.win === undefined)
                        this.win = self.up('[isVcl=true]');
                    if (this.win && this.win.vcl && (!this.win.vcl.isEdit && this.win.vcl.proxy !== true)) {
                        // e.stopEvent();
                    } else {
                        var self = this;
                        var dataInfo = Ax.Control.LibDataInfo.getDataInfo(self);
                        if (self.win.vcl != undefined) {
                            self.win.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.DblClick, dataInfo: dataInfo });
                        }
                    }
                }
            }
        };
        this.callParent();
    },
    doQuery: function (queryString, forceAll, rawQuery) {
        if (queryString.indexOf(',') != -1)
            return false;
        if (!queryString) {
            var value = this.getValue();
            if (value) {
                if (this.fieldType == 'number') {
                    if (value != 0)
                        return false;
                }
                else if (value != '')
                    return false;
            }
        }
        if (this.win === undefined)
            this.win = this.up('window');
        if (this.win === undefined)
            this.win = this.up('[isVcl=true]');
        if (this.win && this.win.vcl && (!this.win.vcl.isEdit && this.win.vcl.proxy !== true))
            return false;//如果不是在编辑状态则不执行查询
        if (this.win && this.win.vcl && this.win.vcl.customFuzzySearchTemplate != undefined && this.win.vcl.customFuzzySearchTemplate != Ext.emptyFn) {
            //调用vcl自定义的模板设定方法  Zhangkj 20170301
            this.win.vcl.customFuzzySearchTemplate(this, this.tableIndex, this.name);
        }
        this.store.removeAll();//先清空数据
        var ret = Ext.form.field.ComboBox.prototype.doQuery.apply(this, arguments);
        if (ret) {
            if (this.win === undefined)
                this.win = this.up('window');
            if (this.win === undefined)
                this.win = this.up('[isVcl=true]');
            var dataInfo = Ax.Control.LibDataInfo.getDataInfo(this);
            var realRelSource;
            if (this.realRelSource) {
                realRelSource = this.realRelSource;
            }
            else {
                var obj = {};
                realRelSource = Ax.utils.LibVclSystemUtils.getRelSource(this, dataInfo, this.win.vcl, obj);
                if (obj.hasRealRelSource) {
                    this.realRelSource = realRelSource;
                }
            }
            var curPks;
            var data;
            if (this.win.vcl && this.win.vcl.proxy !== true) {
                if (this.relPk != '') {
                    curPks = this.win.vcl.getRelPk(this.name, this.relPk, this.tableIndex, dataInfo.dataRow, dataInfo.value, dataInfo.curGrid);
                    curPks.pop();
                }
                selConditionParam = {};
                for (var i = 0; i < this.selParams.length; i++) {
                    selConditionParam[this.selParams[i]] = this.win.vcl.getParamFieldValue(this.win.vcl, this.selParams[i], this.tableIndex, dataInfo.dataRow, dataInfo.curGrid);
                }
                var currentPk = this.win.vcl.currentPk;
                if (this.win.vcl.customFuzzySearch != undefined && this.win.vcl.customFuzzySearch != Ext.emptyFn) {
                    //调用vcl自定义的模糊查询方法 Zhangkj 20170301
                    data = this.win.vcl.customFuzzySearch(this, this.tableIndex, this.name, realRelSource, this.relName, queryString, curPks, selConditionParam, currentPk);
                    if (data) {
                        this.remoteData = data;
                        this.store.loadData(data);
                    }
                    this.doAutoSelect();
                    return ret;
                }
                data = this.win.vcl.invorkBcf('FuzzySearchField', [this.tableIndex, this.name, realRelSource, this.relName, queryString, curPks, selConditionParam, currentPk]);
            } else {
                var call = function () {
                    var condition = typeof this.condition == "function" ? this.condition() : this.condition ? 'and ' + this.condition : '';
                    Ext.Ajax.request({
                        url: '/billSvc/fuzzySearchField',
                        jsonData: {
                            handle: UserHandle,
                            relSource: realRelSource,
                            query: queryString,
                            condition: condition,
                            tableIndex: this.tableIndex
                        },
                        method: 'POST',
                        async: false,
                        timeout: 90000000,
                        success: function (response) {
                            var result = Ext.decode(response.responseText);
                            data = result.FuzzySearchFieldResult;
                        }
                    });
                }
                call.apply(this);
            }
            //if (this.fieldType == 'number') {
            //    for (var i = 0; i < data.length; i++) {
            //        data[i]['Id'] = parseInt(data[i]['Id']);
            //    }
            //}
            this.remoteData = data;
            this.store.loadData(data);
            this.doAutoSelect();
        }
        return ret;
    }
});
