/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的目录的前端逻辑处理页面
 * 创建标识：Zhangkj 2016/12/05
 *
 *
 ************************************************************************/
dmDirectoryVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
//grid用Ax.vcl.LibVclGrid,单据主数据用Ax.vcl.LibVclBase,datafunc用Ax.vcl.LibVclGrid
var proto = dmDirectoryVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = dmDirectoryVcl;
//获取视图模板
proto.getTpl = function (checkDirType, checkDirId) {
    this.tpl = this.invorkBcf('GetViewTemplateOfPermission', [checkDirType, checkDirId, this.entryParam]);
};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 2) {
                //权限项明细表的行不可新增删除
                e.dataInfo.cancel = true;
                alert('不能新增。');
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 2) {
                //权限项明细表的行不可新增删除
                e.dataInfo.cancel = true;
                alert('不能删除。');
            }
            break;
        case LibEventTypeEnum.AddRow:
            break;
        case LibEventTypeEnum.Validating:

            break;
        case LibEventTypeEnum.FormClosed:
            break;
        case LibEventTypeEnum.ButtonClick:
            break;
        case LibEventTypeEnum.Validated:
            break;

    }

};
//自定义的libSearchField数据模型和展示模板
proto.customFuzzySearchTemplate2 = function (libSearchField, tableIndex, fieldName) {
    if (tableIndex != 1 || fieldName != 'BELONGID')
        return;
    var modelType = Ext.data.Model.schema.getEntity('FuzzyModel');
    if (modelType === null) {
        modelType = Ext.define("FuzzyModel", {
            extend: "Ext.data.Model",
            fields: [
              { name: 'DEPTID' },
              { name: 'DEPTNAME' },
              { name: 'SORTORDER' },
              { name: 'DEPTLEVEL' },
              { name: 'REMARK' }
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
        sorters: [{ property: 'DEPTID', direction: 'DESC' }]
    });
    libSearchField.valueField = 'DEPTID';
    libSearchField.store = fuzzyStore;

    libSearchField.displayTpl = Ext.create('Ext.XTemplate',
      '<tpl for=".">',
      '<tpl if="DEPTID != &quot;&quot; && DEPTID != undefined && DEPTNAME !=&quot;&quot; && DEPTNAME != undefined">',
      '{DEPTID},{DEPTNAME}',
      '</tpl>',
      '<tpl if="DEPTID != &quot;&quot; && DEPTID != undefined && (DEPTNAME ==&quot;&quot; || DEPTNAME == undefined)">',
      '{DEPTID}',
      '</tpl>',
      '</tpl>'
    );
    libSearchField.listConfig = {
        loadingText: '搜索中...',
        emptyText: '没有匹配的数据',
        tpl: Ext.create('Ext.XTemplate', '<ul><tpl for=".">',
          '<li role="option" class="x-boundlist-item">{DEPTID:this.highlight},{DEPTNAME:this.highlight},{SORTORDER:this.highlight},{DEPTLEVEL:this.highlight},{REMARK:this.highlight}</li>',
          '</tpl></ul>',
          {
              highlight: function (v) {
                  query = this.field.lastQuery;
                  if (Ext.isEmpty(query)) {
                      return v;
                  } else {
                      //Zhangkj 20170206 需要高亮的对象可能为空
                      if (v) {
                          return v.replace(new RegExp(query, 'gi'), function (m) {
                              return "<font color='red'>" + m + "</font>";
                          });
                      }
                  }
              }
          }
        )
    };
    libSearchField.bindStore(libSearchField.store || 'ext-empty-store', true, true);
}
//自定义的libSearchField后台数据查询
proto.customFuzzySearch2 = function (libSearchField, tableIndex, name, realRelSource, relName, queryString, curPks, selConditionParam) {
    var data = this.invorkBcf('MyFuzzySearchField', [tableIndex, name, realRelSource, relName, queryString, curPks, selConditionParam]);
    libSearchField.store.loadData(data);
}


proto.setDefaultPower = function (curRow, returnValue) {
    var ret = returnValue['OperatePowerData'];
    var operateData = this.dataSet.getChildren(1, curRow, 2);
    if (operateData) {
        var table = this.dataSet.getTable(2);
        for (var i = operateData.length - 1; i >= 0; i--) {
            table.remove(operateData[i]);
            this.dataSet.deleteData(2, operateData[i]);
        }
    }
    var hasData = false;
    if (ret) {
        for (p in ret) {
            if (!ret.hasOwnProperty(p))
                continue;
            var value = ret[p];
            if (value !== undefined) {
                var newRow = this.addRow(curRow, 2);
                newRow.set('OPERATEPOWERID', p);
                newRow.set('OPERATEPOWERNAME', value.DisplayText);
                newRow.set('CANUSE', value.CanUse);
                if (!hasData)
                    hasData = true;
            }
        }
    }
    curRow.set('ISOPERATEPOWER', hasData);
};


proto.checkFieldValue = function (curRow, returnValue, tableIndex, fieldName) {
    Ax.vcl.LibVclData.prototype.checkFieldValue.apply(this, arguments);
    if (tableIndex == 1) {
        switch (fieldName) {
            case 'BELONGID':
                this.setDefaultPower(curRow, returnValue);
                break;
        }
    }
};




