/**
 * Created by ZhangKj on 2017/3/31.
 */
/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块编辑/新建的文档与后台交互的js处理方法
 * 创建标识：Zhangkj 2016/12/13
 *
 ************************************************************************/
saveDoc = function (tempFileName, isAddNew, docId, dirId, dirType, UserHandle,realFileName) {
  var isAdd = 0;
  if (isAddNew)
    isAdd = 1;
  var relParam = [isAdd, '"' + tempFileName + '"', '"' + docId + '"', '"' + dirId + '"', dirType, '"' + realFileName + '"'];
  $.ajax({
    type: "POST",
    url: "/billSvc/invorkBcf",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    data: JSON.stringify({ param: { ProgId: "dm.Document", MethodName: "SaveDoc", MethodParam: relParam, Handle: UserHandle } }),
    async: false,
    success: function (message) {

    },
    error: function (message) {

    }

  });

};
