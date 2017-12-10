/**
 * Created by ZhangKj on 2017/3/31.
 */
/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的文件上传组件类
 * 创建标识：Zhangkj 2016/12/13
 *
 ************************************************************************/
Ext.define('DocumentManage.DMFileUploaderForm', {
  dirId: null,
  dirType:null,
  vcl: null,
  isAddNew: true,          //是否是上传文档，默认为true。替换文档应为false
  docId:null,              //如果isAddNew为false，应指定文档编号

  //检查文件
  checkFile: function checkFile(o) {
    ////验证文件的正则
    //    var img_reg = /\.([jJ][pP][gG]){1}$|\.([jJ][pP][eE][gG]){1}$|\.([gG][iI][fF]){1}$|\.([pP][nN][gG]){1}$|\.([bB][mM][pP]){1}$/;
    //if(!img_reg.test(o.value)){
    //    Ext.Msg.alert('提示','文件类型错误,请选择图片文件(jpe/jpeg/gif/png/bmp)');
    //    o.setRawValue('');
    //    return false;
    //}
    //取控件DOM对象
    var field = document.getElementById('id_fileField');
    //取控件中的input元素
    var inputs = field.getElementsByTagName('input');
    var fileInput = null;
    var il = inputs.length;
    //取出input 类型为file的元素
    for (var i = 0; i < il; i++) {
      if (inputs[i].type == 'file') {
        fileInput = inputs[i];
        break;
      }
    }
    if (fileInput != null) {
      var fileSize = this.getFileSize(fileInput);
      //允许上传不大于10M的文件
      if (fileSize > 1024*10) {
        Ext.Msg.alert('提示', '文件太大，请选择小于10M的文件！');
        o.setRawValue('');
        return false;
      }
    }
    return true;
  },
  //计算文件大小，返回文件大小值，单位K
  getFileSize: function (target) {
    var isIE = /msie/i.test(navigator.userAgent) && !window.opera;
    var fs = 0;
    if (isIE && !target.files) {
      var filePath = target.value;
      var fileSystem = new ActiveXObject("Scripting.FileSystemObject");
      var file = fileSystem.GetFile(filePath);
      fs = file.Size;
    } else if (target.files && target.files.length > 0) {
      fs = target.files[0].size;
    } else {
      fs = 0;
    }
    if (fs > 0) {
      fs = fs / 1024;
    }
    return fs;
  },

  show: function (dirId, dirType, dirDocPanel)
  {
    this.dirId = dirId;//文档所在的目录编号
    this.dirType = dirType;

    this.vcl = Ax.utils.LibVclSystemUtils.getVcl("dm.Document", BillTypeEnum.Master);
    var vcl = this.vcl;
    vcl.getTpl(dirType,dirId,'');

    //增加后台AddNew权限检查时需要的的参数，以入口参数的形式提供
    var paramStore = {};
    if (vcl.entryParam)
      paramStore = vcl.entryParam["ParamStore"];
    else {
      vcl.entryParam = {};
      paramStore = {};
      vcl.entryParam["ParamStore"] = paramStore;
    }
    paramStore["DirId"] = '' + dirId + '';
    paramStore["DirType"] = dirType;

    if (this.isAddNew) {
      //上传
      if (vcl.addNew() == false)
        return;
    } else {
      //替换
      if (vcl.checkCan('', this.docId, DMPermissonEnum.Replace) == false)
        return;//要有替换权限
      vcl.currentPk = [this.docId];
      if (vcl.doEdit() == false)
        return;
    }
    var me=this;

    var panel = Ext.create('Ext.form.Panel', {
      bodyPadding: 10,
      frame: true,
      //anchor: '99%,95%',
      //renderTo: Ext.getBody(),
      items: [{
        xtype: 'filefield',
        name: 'id_fileField',
        id: 'id_fileField',
        fieldLabel: '文件',
        labelWidth: 50,
        msgTarget: 'side',
        allowBlank: false,
        anchor: '100%',
        buttonText: '选择...'
      }],
      buttons: [{
        text: (me.isAddNew)?'上传':'替换',
        handler: function () {
          var form = this.up('form').getForm();
          if (form.isValid()) {
            if (me.checkFile(form.getFields().items[0]) == false)
              return;
            var thisMe = this;

            var splitStr = form.getFields().items[0].getValue().split("\\");
            var selectedFileName = splitStr[splitStr.length-1];
            form.submit({
              url: '/fileTranSvc/upLoadDoc',
              waitMsg: '正在上传文件...',
              success: function (fp, o) {
                vcl.extendParam = {};
                vcl.extendParam["FileName"] = '\'' + o.result.FileName + '\'';
                vcl.extendParam["DocOpType"] = (me.isAddNew) ? DocOpTypeEnum.Upload : DocOpTypeEnum.Replace;

                var masterRow = vcl.dataSet.getTable(0).data.items[0];
                masterRow.data["DOCNAME"] = selectedFileName;
                masterRow.data["DIRID"] = me.dirId;
                masterRow.data["DIRTYPE"] = me.dirType;
                if (me.isAddNew)
                  vcl.billAction = BillActionEnum.AddNew;
                else
                  vcl.billAction = BillActionEnum.Modif;
                if (vcl.doSave()) {
                  dirDocPanel.refreshDir();
                  thisMe.up('window').close();
                }
              },
              failure: function (fp, o) {
                Ext.Msg.alert('错误', '文档上传失败.');
              }
            });
          }
        }
      }]
    });
    win = Ext.create('Ext.window.Window', {
      autoScroll: true,
      width: 400,
      height: 150,
      layout: 'fit',
      constrainHeader: true,
      minimizable: true,
      maximizable: true,
      items: [panel]
    });
    win.show();
  },
});
