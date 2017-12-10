/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的文档的前端逻辑处理页面
 * 创建标识：Zhangkj 2016/12/05
 *
 *
 ************************************************************************/
dmDocumentVcl = function () {
  Ax.vcl.LibVclData.apply(this, arguments);
}
//grid用Ax.vcl.LibVclGrid,单据主数据用Ax.vcl.LibVclBase,datafunc用Ax.vcl.LibVclGrid
var proto = dmDocumentVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = dmDocumentVcl;
//获取视图模板
proto.getTpl = function (checkDirType, checkDirId, docId) {
  this.tpl = this.invorkBcf('GetViewTemplateOfPermission', [checkDirType, checkDirId, docId, this.entryParam]);
};
//检查是否具有权限
proto.checkCan = function (dirId,docId,permisson) {
  return this.invorkBcf('CheckPermission', [dirId, docId, permisson]);
};
//设定版本
proto.SetVersion = function (docId, oldVersion, newVerison,desc) {
  return this.invorkBcf('SetVersion', [docId, oldVersion, newVerison, desc]);
};
//添加文档审计（操作）记录
proto.AddDocOpLog = function (docId, opDesc, isAddClickCount) {
  if (!isAddClickCount)
    isAddClickCount = true;
  return this.invorkBcf('AddNewDocOpLog', [docId, opDesc, isAddClickCount]);
};
//回退文档至指定修订版
proto.FallBackVersion = function (docId, toFallbackModifyVerId, opDesc) {
  return this.invorkBcf('FallBackVersion', [docId, toFallbackModifyVerId, opDesc]);
};
//表单附件上传完毕后，移动附件到文档库
proto.MoveAttach = function (attachData) {
  return this.invorkBcf('MoveAttach', [attachData]);
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
//设置默认的权限项
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


