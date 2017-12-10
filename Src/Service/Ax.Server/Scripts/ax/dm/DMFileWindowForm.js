/**
 * Created by ZhangKj on 2017/3/31.
 */
/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的文件阅读、编辑等的js窗口类
 * 创建标识：Zhangkj 2016/12/13
 *
 ************************************************************************/
Ext.define('DocumentManage.DMFileWindowForm', {
  docId: null,                //文档编号
  dirType:null,
  modifyVerionId: null,       //文档的修订号
  //只读,阅读
  readOnly: function (docId, modifyVerionId, docType, docName) {
    this.docId = docId;//文档的编号
    this.modifyVerionId = modifyVerionId;
    var url = '/document/ReadOnly';//只读
    url += '?fileId=' + this.docId + '&modifyVerionId=' + this.modifyVerionId + '&userHandle=' + UserHandle;
    this.showDocWin(url, docType, true, false, '浏览 【' + docName + '】');
  },
  //打印
  print: function (docId, modifyVerionId, docType, docName) {
    this.docId = docId;//文档的编号
    this.modifyVerionId = modifyVerionId;
    var url = '/document/print';//打印
    url += '?fileId=' + this.docId + '&modifyVerionId=' + this.modifyVerionId + '&userHandle=' + UserHandle;
    this.showDocWin(url, docType, true, false, '打印 【' + docName + '】');
  },
  //编辑文档
  editDoc: function (docId, docType, dirId, dirType, docName) {
    this.docId = docId;//文档的编号
    this.dirType = dirType;

    var url = '/document/Edit';
    this.docType = docType.toLowerCase();
    url += '?fileId=' + this.docId + '&userHandle=' + UserHandle + '&dirId=' + dirId + '&dirType=' + dirType + '&isChrome=' + Ext.isChrome;
    this.showDocWin(url, docType, false, false, '编辑 【' + docName + '】');
  },
  //新建文档
  addDoc: function (dirId, docType, dirType, realfileName) {
    this.dirType = dirType;
    this.dirId = dirId;

    var url = 'document/create';
    url += '?dirId=' + dirId + '&userHandle=' + UserHandle + '&docType=' + docType + '&dirType=' + dirType + '&isChrome=' + Ext.isChrome
      + '&realfileName=' + realfileName;
    this.showDocWin(url, docType, false, true, '新建 【' + realfileName + '】');
  },
  //saveDoc: function (tempFileName,isAddNew) {
  //    this.vcl = Ax.utils.LibVclSystemUtils.getVcl("dm.Document", BillTypeEnum.Master);
  //    var vcl = this.vcl;
  //    var docId = this.docId;
  //    if (isAddNew)
  //        docId = '';
  //    vcl.getTpl(this.dirType, this.dirId, docId);

  //    if (isAddNew)
  //    {
  //        //增加后台AddNew权限检查时需要的的参数，以入口参数的形式提供
  //        var paramStore = {};
  //        if (vcl.entryParam)
  //            paramStore = vcl.entryParam["ParamStore"];
  //        else {
  //            vcl.entryParam = {};
  //            paramStore = {};
  //            vcl.entryParam["ParamStore"] = paramStore;
  //        }
  //        paramStore["DirId"] = '' + this.dirId + '';
  //        paramStore["DirType"] = this.dirType;
  //        if (vcl.addNew() == false)
  //            return;
  //        vcl.extendParam = {};
  //        vcl.extendParam["FileName"] = '\'' + tempFileName + '\'';

  //        var masterRow = vcl.dataSet.getTable(0).data.items[0];
  //        masterRow.data["DOCNAME"] = tempFileName;
  //        masterRow.data["DIRID"] = this.dirId;
  //        masterRow.data["DIRTYPE"] = this.dirType;
  //        vcl.billAction = BillActionEnum.AddNew;
  //        vcl.doSave();
  //    }
  //    else {
  //        //编辑文档
  //        vcl.currentPk = [this.docId];
  //        if (vcl.doEdit()) {
  //            vcl.extendParam = {};
  //            vcl.extendParam["FileName"] = '\'' + tempFileName + '\'';
  //            vcl.billAction = BillActionEnum.Modif;
  //            vcl.doSave();
  //        }
  //    }
  //},
  showDocWin: function (url, docType, isReadOnly, isAddNew, title) {
    var me = this;
    if (!Ext.isChrome || docType === ".txt" || docType === ".pdf" || docType === ".png" ||
      docType === ".gif" || docType === ".jpg" || docType === ".bmp" || docType === ".jpeg") {
      //var win = DesktopApp.getDesktop().createWindow({
      var win = Ext.create('Ext.window.Window', {
        title: title,
        progName: title,
        width: document.body.clientWidth * 0.8,
        height: document.body.clientHeight * 0.9,
        constrainHeader: true,
        layout: 'fit',
        minimizable: true,
        maximizable: true,
        items: [new Ext.Panel({
          resizeTabs: true,
          autoScroll: false,
          html: '<iframe id="DMFileWindowFormFrame" scrolling="auto" src="//' + pageOfficeServerConfig.address + ':' + pageOfficeServerConfig.port + '/' + url + '"  frameborder="0" width="100%" height="100%"></iframe>'
        })],
      });
      win.show();
    } else {
      var realUrl = '';
      Ext.Ajax.request({
        url: '/document/GetFileUrl',
        params: {
          url: 'http://' + pageOfficeServerConfig.address + ':' + pageOfficeServerConfig.port + '/' + url,
          options: 'width=1200px;height=800px;'
        },
        success: function (response) {
          realUrl = response.responseText;
          var win = Ext.create('Ext.window.Window', {
            items: [{
              xtype: 'panel',
              html: '<iframe id="DMFileWindowFormFrame" scrolling="auto" src="' + realUrl + '"></iframe>'
            }]
          });
          win.show();
          win.close();
        },
        failure: function () {
          Ext.Msg.alert("无法获得资源，请联系管理员。");
        }
      });
      //打开窗口
    }
  }
});
