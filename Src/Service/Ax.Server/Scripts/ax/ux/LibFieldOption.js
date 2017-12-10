/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：具有下拉Grid的ComboBox
 * 创建标识：Yangj 2017/06/29
 *
 ************************************************************************/
Ext.define('Ax.ux.form.ComboGridBox', {
  extend: 'Ext.form.field.ComboBox',
  multiSelect: false,
  defaultWidth:0,
  columns:null,
  createPicker: function () {
    var me = this;
    var picker = Ext.create('Ext.grid.Panel', {
      store: me.store,
      frame: true,
      resizable: true,
      columns: me.columns,
      selModel: {
        mode: me.multiSelect ? 'SIMPLE' : 'SINGLE'
      },
      floating: true,
      hidden: true,
      width: me.defaultWidth,
      height: 220,
      columnLines: true,
      focusOnToFront: false,
      scrollable: true,
      listeners: {
        itemclick: function (view, record, item, index, evt)
        {
          me.onItemClick(this, record);
        }
      }
    });

    this.picker = picker;
    return picker;
  },
  onItemClick: function (picker, record) {
    /*
     * If we're doing single selection, the selection change events won't fire when
     * clicking on the selected element. Detect it here.
     */
    var me = this,
      selection = me.picker.getSelectionModel().getSelection(),
      valueField = me.valueField;

    if (!me.multiSelect && selection.length) {
      if (record.get(valueField) === selection[0].get(valueField)) {
        // Make sure we also update the display value if it's only partial
        me.displayTplData = [record.data];
        //me.setRawValue(me.getDisplayValue());
        me.setValue(me.getDisplayValue());
        //me.select(record.data.Id);
        me.collapse();
      }
    }
  },
  matchFieldWidth: false,
  onListSelectionChange: function (list, selectedRecords) {
    var me = this,
      isMulti = me.multiSelect,
      hasRecords = selectedRecords.length > 0;
    // Only react to selection if it is not called from setValue, and if our list is
    // expanded (ignores changes to the selection model triggered elsewhere)
    if (!me.ignoreSelection && me.isExpanded) {
      if (!isMulti) {
        Ext.defer(me.collapse, 1, me);
      }
      /*
       * Only set the value here if we're in multi selection mode or we have
       * a selection. Otherwise setValue will be called with an empty value
       * which will cause the change event to fire twice.
       */
      if (isMulti || hasRecords) {
        me.setValue(selectedRecords, false);
      }
      if (hasRecords) {
        me.fireEvent('select', me, selectedRecords);
      }
      me.inputEl.focus();
    }
    console.log(me.getValue());
  },
  doAutoSelect: function () {
    var me = this,
      picker = me.picker,
      lastSelected, itemNode;
    if (picker && me.autoSelect && me.store.getCount() > 0) {
      // Highlight the last selected item and scroll it into view
      lastSelected = picker.getSelectionModel().lastSelected;
      itemNode = picker.view.getNode(lastSelected || 0);
      if (itemNode) {
        picker.view.highlightItem(itemNode);
        picker.view.el.scrollChildIntoView(itemNode, false);
      }
    }
  }
});

Ext.define('Ax.ux.form.LibFieldOption', {
  extend: 'Ax.ux.form.ComboGridBox',
  alias: 'widget.libFieldOption',
  gridAttr:null,
  valueField: null,
  displayField: null,
  queryMode: 'local',
  columns: null,
  store: null,
  defaultWidth:0,
  initComponent: function () {
    var me = this;
    if (me.gridAttribute && me.gridAttribute != '')
    {
      this.gridAttr = Ext.decode(me.gridAttribute);
    }
    if (this.gridAttr) {
      this.valueField = this.gridAttr.ValueField;
      this.displayField = this.gridAttr.ValueField;

      var showField = this.gridAttr.ShowField;
      var c = [];
      var fields = new Array();
      for (key in showField) {
        if (showField[key].FieldName != undefined) {
          var obj = {};
          obj["text"] = showField[key].TextName;
          obj["dataIndex"] = showField[key].FieldName;
          c.push(obj);
          fields.push(showField[key].FieldName);
        }
      }
      this.columns = c;
      //初始化数据
      var value = [];
      var p = this.gridAttr.ParamField.split(",");
      for (var i = 0; i < p.length; i++) {
        if (p[i] != null && p[i] != "") {
          value.push("")
        }
      }
      var docVcl = Ax.utils.LibVclSystemUtils.getVcl(this.gridAttr.ProgId, BillTypeEnum.Grid);
      //var result = docVcl.invorkBcf(this.gridAttr.FuncName, value);
      var store = Ext.create('Ext.data.JsonStore', {
        fields: fields,
        data: [{}]
      });
      this.defaultWidth = c.length * 100;//grid 默认长度
      this.store = store;
    }
    this.doSetData = function (self) {
      if (this.gridAttr.ParamField)
      {
        var dataInfo = Ax.Control.LibDataInfo.getDataInfo(self);
        var value = [];
        var p = this.gridAttr.ParamField.split(",");
        for (var i = 0; i < p.length; i++) {
          if (p[i] != null && p[i] != "") {
            value.push(dataInfo.dataRow.get(p[i]))
          }
        }
        var docVcl = Ax.utils.LibVclSystemUtils.getVcl(this.gridAttr.ProgId, BillTypeEnum.Grid);
        var data = docVcl.invorkBcf(this.gridAttr.FuncName, value);
        if (data.length == 0) {
          this.store.loadData([{}]);
        } else {
          this.store.loadData(data);
        }
      }
    }
    this.callParent();
  },
  doQuery: function (queryString, forceAll, rawQuery) {
    var ret = Ax.ux.form.ComboGridBox.prototype.doQuery.apply(this, arguments);
    this.doSetData(this);
    return true;
  }
});
