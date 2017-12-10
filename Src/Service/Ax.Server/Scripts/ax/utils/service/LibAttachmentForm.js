/**********************************************************************
 * CopyRight 2017 杭州集控科技有限公司 版权所有
 * 功能描述：表单附件管理
 * 创建标识：未知
 *
 * 修改标识：Zhangkj 2017/01/05-2017/01/06
 * 修改描述：将附件关联到文档管理模块的文档库中。
 上传文件时检查文件大小等
 ************************************************************************/

//显示文档管理窗口，并跳转到相应的目录和文档
function jumpToDocumentWinDoc (dirId, dirType, docId) {
  // //尚未验证
  // if (typeof (myDesktopApp) != "undefined") {
  //     var deskTop = myDesktopApp.getDesktop();
  //     if (!documentWindow || !documentWindow.documentWindowWin) {
  //         // 打开文档管理窗口
  //         var module = myDesktopApp.getModule('document-win');
  //         myDesktopApp.createWindow(module);
  //         documentWindowWin.show();//documentWindowWin是文档管理类中的Window
  //     }
  //     else
  //         deskTop.restoreWindow(documentWindowWin);
  // }
  // else {
  //     if (documentWindowObj) {
  //         if (documentWindowObj.documentWindowWin)
  //             documentWindowObj.documentWindowWin.show();
  //         else
  //             documentWindowObj.createWindow().show();
  //     }
  // }
  //
  // var delay = new Ext.util.DelayedTask(function () {
  //     documentWindow.dirDocViewTab.jumpTo(dirId, DirTypeEnum.Public, docId);
  // });
  // delay.delay(1000);
  DocumentManage.DocumentMain.JumpTo(dirId, dirType, docId);
};
Ax.utils.LibAttachmentForm = {
  //检查文件
  checkFile: function checkFile (o) {
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
      if (fileSize > 1024 * 10) {
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
  show: function (vcl, curRow, tableName) {
    var attachmentData;
    var maxOrderId = 0;
    var attachmentSrc = curRow.get('ATTACHMENTSRC');
    var data;
    var userHandle = Ext.decode(Ext.util.Cookies.get('userHandle'));
    if (!attachmentSrc) {
      data = [];
      var add = function (row) {
        if (row) {
          var temp = [];
          var obj = row.data;
          for (p in obj) {
            if (!obj.hasOwnProperty(p))
              continue;
            temp.push({Key: p, Value: obj[p]});
          }
          data.push(temp);
        }
      }
      var parentRow;
      do {
        parentRow = vcl.dataSet.getParent(tableName, curRow);
        add(parentRow);
      } while (parentRow);
      add(curRow);
    }
    Ext.Ajax.request({
      url: 'billSvc/loadAttachInfo',
      jsonData: {handle: userHandle.handle, attachmentSrc: attachmentSrc, progId: vcl.progId, data: data},
      method: 'POST',
      async: false,
      success: function (response) {
        var temp = Ext.decode(response.responseText);
        var result = temp["LoadAttachInfoResult"];
        attachmentData = result.AttachList;
        maxOrderId = result.MaxOrderId;
      },
      failure: function () {
        Ext.Msg.show({
          title: '错误',
          msg: '载入失败！返回登录页！',
          buttons: Ext.Msg.YES,
          icon: Ext.Msg.INFO,
          fn: function (buttonId) {
            if (buttonId === 'yes') {
              if (window.DesktopApp.router) {
                window.DesktopApp.router.push('/')
              }
            }
          }
        });
      }
    });
    var modelName = 'LibAttachmentModel';
    var modelType = Ext.data.Model.schema.getEntity(modelName);
    if (modelType === null) {
      modelType = Ext.define("LibAttachmentModel", {
        extend: "Ext.data.Model",
        fields: [
          {name: 'OrderId', type: 'number'},
          {name: 'AttachName', type: 'string'},
          {name: 'FileName', type: 'string'}]
      });
    }
    var store = Ext.create('Ext.data.TreeStore', {
      model: modelName,
      proxy: {
        type: 'memory',
        reader: {
          type: 'json'
        }
      },
      root: {
        expanded: true,
        leaf: false
      }
    });

    function getPkList () {
      var pkList = [];
      var pks = vcl.tpl.Tables[tableName].Pk;
      for (var i = 0; i < pks.length; i++) {
        pkList.push({Key: pks[i], Value: curRow.get(pks[i])});
      }
      return pkList;
    };
    //对直接通过vclinvoke的字典值项赋值
    function getPkListForInvoke () {
      //var pkList = [];
      var pkList = {}; //zhangkj 20170106 修改
      var pks = vcl.tpl.Tables[tableName].Pk;
      for (var i = 0; i < pks.length; i++) {
        //pkList.push({ Key: pks[i], Value: curRow.get(pks[i]) });
        pkList[pks[i]] = curRow.get(pks[i]);    //zhangkj 20170106 修改
      }
      return pkList;
    };

    function updateStruct () {
      var attachList = [];
      var idx = 1;
      var tempList = [];
      root.eachChild(function (node) {
        var orderId = node.get('OrderId');
        var attachName = node.get('AttachName');
        var docId = node.get('DocId');//Zhangkj 20170105添加
        var info = assist[orderId];
        if (info) {
          if (info.OrderNum != idx || info.AttachName != attachName) {
            attachList.push({
              OrderId: orderId,
              OrderNum: idx,
              AttachmentName: attachName,
              Status: 1,
              DocId: docId,
            });
            assist[orderId] = {OrderNum: idx, AttachName: attachName};
          }
          tempList.push(orderId.toString());
        } else {
          attachList.push({
            OrderId: ++maxOrderId,
            OrderNum: idx,
            AttachmentName: attachName,
            Status: 0,
            DocId: docId,
          });
          node.set('OrderId', maxOrderId);
          assist[maxOrderId] = {OrderNum: idx, AttachName: attachName};
          tempList.push(maxOrderId.toString());
        }
        idx++;
      });
      for (p in assist) {
        if (!assist.hasOwnProperty(p))
          continue;
        if (!Ext.Array.contains(tempList, p)) {
          attachList.push({
            OrderId: p,
            Status: 2
          });
          delete assist[p];
        }
      }
      return attachList;
    }

    //记录当前对象，以便在事件处理中使用
    var me = this;
    me.vcl = vcl;
    //CHENQI 20170117 上传文档 //CHENQI 20170122 上传文档的同时创建节点
    function upload (treePanel) {

      if (vcl.isEdit) {
        Ext.Msg.alert('提示', '浏览状态下才可以上传。');
        return;
      }

      var panel = Ext.create('Ext.form.Panel', {
        bodyPadding: 10,
        frame: true,
        renderTo: Ext.getBody(),
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
          text: '上传',
          handler: function () {
            var form = this.up('form').getForm();
            if (form.isValid()) {
              //CHENQI 20170122 生成一个节点
              var root = treePanel.getRootNode();
              var tempStrArrays = Ext.getCmp('id_fileField').getValue().split('\\');
              var realFileName = tempStrArrays[tempStrArrays.length - 1];
              var selectedNode = root.appendChild(Ext.create('LibAttachmentModel', {
                AttachName: realFileName,
                FileName: '',
                HistoryList: [],
                leaf: true
              }));
              //检查文件大小
              if (me.checkFile(form.getFields().items[0]) == false)
                return;
              var thisMe = this;
              form.submit({
                //url: '/fileTranSvc/upLoadAttach',
                url: '/fileTranSvc/upLoadDoc',//zhangkj 20170105 修改为使用文档库的上传
                waitMsg: '正在上传文件...',
                success: function (fp, o) {
                  var attachSrc = curRow.get('ATTACHMENTSRC');
                  var attachList = updateStruct();
                  var pkList = getPkListForInvoke();//对直接通过vclinvoke的字典值项赋值
                  //var pkList = getPkList();
                  var orderId = selectedNode.get('OrderId');
                  var personId = Ext.decode(Ext.util.Cookies.get('userHandle')).personId;
                  var attachData = {
                    ProgId: vcl.progId,
                    TableName: tableName,
                    AttachSrc: attachSrc,
                    FileName: o.result.FileName,
                    OrderId: orderId,
                    OrderNum: assist[orderId].OrderNum,
                    PersonId: personId,
                    PkList: pkList,
                    AttachList: attachList,

                    //Zhangkj 20170105修改, CHENQI 20170117修改
                    RealFileName: realFileName,
                  };

                  var docVcl = Ax.utils.LibVclSystemUtils.getVcl("dm.Document", BillTypeEnum.Master);
                  var moveResult = docVcl.MoveAttach(attachData);
                  if (moveResult && moveResult.success == true) {
                    selectedNode.set('DocId', moveResult.DocId);
                    selectedNode.set('DirId', moveResult.DirId);
                    if (!attachSrc) {
                      attachSrc = moveResult.AttachSrc;
                      curRow.set('ATTACHMENTSRC', attachSrc);
                    }
                    Ext.Msg.alert('提示', '附件上传成功.');
                    thisMe.up('window').close();
                  }
                },
                failure: function (fp, o) {
                  root.removeChild(selectedNode);
                  Ext.Msg.alert('错误', '附件上传失败.');
                }
              });
            }
          }
        }]
      });
      win = Ext.create('Ext.window.Window', {
        autoScroll: true,
        width: 400,
        height: 300,
        layout: 'fit',
        vcl: vcl,
        constrainHeader: true,
        minimizable: true,
        maximizable: true,
        items: [panel]
      });
      win.show();
    }

    //CHENQI 20170117 下载文档
    function download (treePanel) {
      var selectedNode = treePanel.getSelectionModel().getLastSelected();
      if (selectedNode) {
        var docId = selectedNode.get('DocId');//对应文档库的文档编号  Zhangkj 20170106添加
        if (docId && docId != '') {
          var docVcl = Ax.utils.LibVclSystemUtils.getVcl("dm.Document", BillTypeEnum.Master);
          if (docVcl.checkCan('', docId, DMPermissonEnum.Download)) {
            docVcl.AddDocOpLog(docId, '下载了文档。');

            var url = '/document/Download';
            url += '?docId=' + docId + '&userHandle=' + UserHandle + '&modifyVerId=-1';

            var win = Ext.create('Ext.window.Window', {
              autoScroll: true,
              width: 100,
              height: 20,
              layout: 'fit',
              constrainHeader: true,
              minimizable: true,
              maximizable: true,
              contentEl: Ext.DomHelper.append(document.body, {
                tag: 'iframe',
                style: "border 0px none;scrollbar:true",
                src: url,
                height: "100%",
                width: "100%"
              })
            });
            //win.show();//窗口不需要显示，内置的iframe因为地址指向了下载地址就能自动下载文件
          }
        }
        else {
          var fileName = selectedNode.get('FileName');
          if (fileName && fileName !== '') {
            Ext.Ajax.request({
              url: 'fileTranSvc/downloadAttach',
              jsonData: {progId: vcl.progId, attachSrc: curRow.get('ATTACHMENTSRC'), fileName: fileName},
              method: 'POST',
              async: false,
              success: function (response) {
                //window.location.href = '/TempData/Attachment/' + fileName;

                var url = '/Desk/Download?filename=' + fileName;//Zhangkj 20170227 调用后台Action采用流文件输出，解决图片类型等文件下载时浏览器转向问题

                //Zhangkj 20170106 替换为新形式
                var win = Ext.create('Ext.window.Window', {
                  autoScroll: true,
                  width: 400,
                  height: 300,
                  layout: 'fit',
                  constrainHeader: true,
                  minimizable: true,
                  maximizable: true,
                  contentEl: Ext.DomHelper.append(document.body, {
                    tag: 'iframe',
                    style: "border 0px none;scrollbar:true",
                    //src: '/TempData/Attachment/' + fileName,
                    src: url,
                    height: "100%",
                    width: "100%"
                  })
                });
                //win.show();//窗口不需要显示，内置的iframe因为地址指向了下载地址就能自动下载文件
              }
            });
          }
          else {
            //没有关联到文档库中的文档。而且老的表单附件上传方式中需要的FileName也是空的
            Ext.Msg.alert('提示', '没有关联到文档库中的文档。');
            return;
          }
        }
      }
      else {
        Ext.Msg.alert("提示", "请选择一个节点");
      }
    }

    //CHENQI 20170117 定位到文档库
    function locationTo (treePanel) {
      var selectedNode = treePanel.getSelectionModel().getLastSelected();
      if (selectedNode) {
        if (selectedNode) {
          //打开文档管理窗口，定位到目标文档
          var docId = selectedNode.get('DocId');
          if (!docId || docId == '') {
            Ext.Msg.alert('提示', '没有关联到文档库中的文档。');
            return;
          }
          var docVcl = Ax.utils.LibVclSystemUtils.getVcl("dm.Document", BillTypeEnum.Master);
          if (docVcl.checkCan('', docId, DMPermissonEnum.Browse)) {
            var dirId = selectedNode.get('DirId');
            jumpToDocumentWinDoc(dirId, DirTypeEnum.Public, docId);
            me.win.close();//定位到文档库时关闭附件窗口
            if (me.vcl.win)
              me.vcl.win.minimize();//最小化
            //me.vcl.win.close();//定位到文档库时关闭表单窗口

          }
        }
      }
      else {
        Ext.Msg.alert("提示", "请选择一个节点");
      }
    }

    var treePanel = Ext.create('Ext.tree.Panel', {
      store: store,
      rootVisible: false,
      displayField: 'AttachName',
      plugins: [
        Ext.create('Ext.grid.plugin.CellEditing', {
          clicksToEdit: 1
        })
      ],
      dockedItems: [{
        xtype: 'toolbar',
        items: [/*{
         text: '新增',
         handler: function () {
         Ax.utils.LibAttachmentForm.showRemanWin(true, treePanel);
         }
         },*/{
          text: '上传',
          handler: function () {
            upload(treePanel);
          }
        }, {
          text: '删除',
          handler: function () {
            var node = treePanel.getSelectionModel().getLastSelected();
            if (node) {
              function removeNode (deleteDocResult, isContains, isCancel) {
                {
                  if (isCancel == true)
                    return;
                  var tip = '';
                  if (isContains) {
                    if (deleteDocResult) {
                      tip = '文档库中的文档已删除!';
                    }
                    else {
                      Ext.Msg.alert('提示', '删除文档库中对应的文档失败!');
                      return;
                    }
                  }
                  root.removeChild(node);
                  if (tip != '')
                    Ext.Msg.alert('提示', tip);
                }


              }

              var docId = node.get('DocId');
              if (docId && docId != '') {
                me.deleteDocInLib(docId, removeNode);
              }
              else
                removeNode(false, false, false);
            }
            else {
              Ext.Msg.alert("提示", "请选择一个节点");
            }
          }
        }, {
          text: '重命名',
          handler: function () {
            var selectedNode = treePanel.getSelectionModel().getLastSelected();
            if (selectedNode) {
              Ax.utils.LibAttachmentForm.showRemanWin(false, treePanel);
            }
            else {
              Ext.Msg.alert("提示", "请选择一个节点");
            }
          }
        }, "-", {
          text: '下载',
          handler: function () {
            download(treePanel);
          }
        }, {
          text: '定位到文档库',
          handler: function () {
            locationTo(treePanel);
          }
        }]
      }],
      viewConfig: {
        plugins: {
          ptype: 'treeviewdragdrop',
          dragText: '{0} 选中节点',
          allowContainerDrop: true,
          allowParentInsert: true,
          containerScroll: true,
          sortOnDrop: true
        }
      },
      listeners: {
        'itemcontextmenu': function (view, record, item, index, e, eOpts) {
          e.preventDefault();
          var menu = new Ext.menu.Menu({
            items: [{
              text: "上传",
              iconCls: 'up',
              handler: function () {
                upload(treePanel);
              }
            }, {
              text: "下载",
              iconCls: 'down',
              handler: function () {
                download(treePanel);
              }
            }, {
              text: "删除",
              iconCls: 'gridDelete',
              handler: function () {
                var selectedNode = treePanel.getSelectionModel().getLastSelected();
                if (selectedNode) {
                  var fileName = selectedNode.get('FileName');
                  var docId = selectedNode.get('DocId');

                  if ((!fileName || fileName == '') && (!docId || docId == '')) {
                    Ext.Msg.alert('提示', '没有对应的附件。');
                    return;
                  }
                  function deleteInfo (deleteDocResult, isContains, isCancel) {
                    if (isCancel == true)
                      return;
                    var tip = '';
                    if (isContains) {
                      if (deleteDocResult) {
                        tip += '文档库中的文档已删除!';
                      }
                      else {
                        Ext.Msg.alert('提示', '删除文档库中对应的文档失败!');
                        return;
                      }
                    }
                    Ext.Ajax.request({
                      url: 'fileTranSvc/removeAttach',
                      jsonData: {
                        attachSrc: curRow.get('ATTACHMENTSRC'),
                        orderId: selectedNode.get('OrderId'),
                        personId: DesktopApp.loginInfo.personId
                      },
                      method: 'POST',
                      async: false,
                      success: function (response) {
                        selectedNode.set('FileName', '');
                        selectedNode.set('DocId', '');
                        selectedNode.set('DirId', '');
                        if (tip != '')
                          tip += '<br />';
                        tip += '附件信息删除成功!';
                        Ext.Msg.alert('提示', tip);
                      },
                      failure: function () {
                        Ext.Msg.alert('错误', '附件删除失败.');
                      }
                    });

                  }

                  if (docId && docId != '') {
                    //删除文档库中的文档
                    me.deleteDocInLib(docId, deleteInfo);
                  }
                  else
                    deleteInfo(false, false, false);

                }
              }
            },
              {
                //定位到文档库中的文档
                text: "定位(文档库)",
                iconCls: 'look',
                handler: function () {
                  locationTo(treePanel);
                }
              },
              //{
              //    text: "历史",
              //    iconCls: 'history',
              //    handler: function () {

              //    }
              , {
                text: "重命名",
                iconCls: 'rename',
                handler: function () {
                  var selectedNode = treePanel.getSelectionModel().getLastSelected();
                  if (selectedNode) {
                    Ax.utils.LibAttachmentForm.showRemanWin(false, treePanel);
                  }
                }
              }]
          }).showAt(e.getXY());
        }
      }
    });
    var assist = {};
    var root = treePanel.getRootNode();
    if (attachmentData != undefined) {
      for (var i = 0; i < attachmentData.length; i++) {
        var orderId = attachmentData[i]['OrderId'];
        if (maxOrderId < orderId)
          maxOrderId = orderId;
        var attachName = attachmentData[i]['AttachName'];
        assist[orderId] = {OrderNum: i + 1, AttachName: attachName};
        var fielName = attachmentData[i]['FileName'];
        var docId = attachmentData[i]['DocId'];//Zhangkj 20170106 修改
        var dirId = attachmentData[i]['DirId'];

        root.appendChild(Ext.create(modelName, {
          OrderId: orderId,
          AttachName: attachName,
          FileName: fielName,
          DocId: docId,//Zhangkj 20170106 修改
          DirId: dirId,

          HistoryList: attachmentData[i]['HistoryList'],
          leaf: true
        }));
      }
    }
    treePanel.expandAll();
    var win = Ext.create('Ext.window.Window', {
      title: '附件',
      layout: 'fit',
      height: 300,
      width: 400,
      constrainHeader: true,
      minimizable: false,
      maximizable: false,
      modal: true,
      items: [treePanel],
      listeners: {
        beforeclose: function () {
          var attachSrc = curRow.get('ATTACHMENTSRC');
          var attachList = updateStruct();
          var pkList = getPkList();
          var attachData = {
            ProgId: vcl.progId,
            TableName: tableName,
            AttachSrc: attachSrc,
            PkList: pkList,
            AttachList: attachList
          };
          Ext.Ajax.request({
            url: 'fileTranSvc/saveAttachStruct',
            jsonData: {attachData: attachData},
            method: 'POST',
            async: false,
            success: function (response) {
              if (!attachSrc) {
                if (response['saveAttachStructResult'] != undefined) {
                  attachSrc = response['saveAttachStructResult'];
                }
                curRow.set('ATTACHMENTSRC', attachSrc);
              }
            }
          });
        }
      }
    });
    this.win = win;
    win.show();
  },
  //删除附件对应的文档库中的文档，有结果时通过回调函数返回
  deleteDocInLib: function (docId, callback) {

    //callback 参数说明 deleteDocResult, isContains, isCancel

    if (!docId || docId == '') {
      callback(false, false, false);
      return;
    }
    var tipInfo = '确定将对应文档库中的附件文档删除吗?';
    var warnTitle = '警告:彻底删除附件文档';
    Ext.MessageBox.confirm(warnTitle,
      tipInfo, function (btn) {
        if (btn == 'yes') {
          var docVcl = Ax.utils.LibVclSystemUtils.getVcl("dm.Document", BillTypeEnum.Master);
          docVcl.currentPk = [docId];
          var success = docVcl.doDelete(docVcl.currentPk);
          if (success) {
            callback(true, true, false);
            return;
          }
          else {
            callback(false, true, false);
            return;
          }
        }
        else {
          callback(false, true, true);
          return;
        }
      });

  },
  showRemanWin: function (isAdd, treePanel) {
    var root = treePanel.getRootNode();
    var renamePanel = Ext.create('Ext.form.Panel', {
      items: [{
        xtype: 'textfield',
        fieldLabel: '附件名',
        id: 'rename',
        margin: '10 10'
      }],
      buttons: [{
        text: '确定',
        handler: function () {
          var name = Ext.getCmp('rename').getValue();
          if (isAdd) {
            root.appendChild(Ext.create('LibAttachmentModel', {
              AttachName: name,
              FileName: '',
              HistoryList: [],
              leaf: true
            }));
          } else {
            var selectedNode = treePanel.getSelectionModel().getLastSelected();
            if (selectedNode)
              selectedNode.set('AttachName', name);
            var docId = selectedNode.get('DocId');
            if (docId && docId != '') {
              Ext.Msg.alert('提示', '文档库中的文档名称需要到文档管理模块中更改。');
            }
          }
          win.close();
        }
      }, {
        text: '取消',
        handler: function () {
          win.close();
        }
      }]
    });
    var win = Ext.create('Ext.window.Window', {
      title: '附件名',
      layout: 'fit',
      height: 150,
      width: 300,
      layout: 'fit',
      constrainHeader: true,
      minimizable: false,
      maximizable: false,
      modal: true,
      items: renamePanel
    });
    this.win = win;
    win.show();
  }
}
