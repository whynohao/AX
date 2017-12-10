/**
 * Created by ZhangKj on 2017/3/31.
 */
/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的文档或目录的操作工具栏
 * 创建标识：Zhangkj 2016/12/01
 *
 ************************************************************************/
Ext.define('DocumentManage.DMToolbar', {
  extend: 'Ext.toolbar.Toolbar',
  //uses 加载需要的类时机是,当前类初始化之后被加载
  uses: [
    'DocumentManage.DMCommon',
    'DocumentManage.DMFileUploaderForm', //文件上传类
    'DocumentManage.DMFileWindowForm',   //文件阅读、编辑、打开灯的窗口类
  ],
  //id: 'document-docToolbar',
  dataRecord: null,                       //浏览的具体文档目录或文档记录
  dirDocPanel:null,                       //对文档管理主Panel的引用

  isDir: false,                           //是否是用于目录管理的工具条
  isToCopy:false,                         //是否是用于复制
  vcl: null,                              //工具条对应的业务表单处理vcl
  permission: null,                       //当前用户的权限
  actionAddNew: null,                     //添加新记录的Action
  actionEdit:null,                        //编辑记录的Action

  constructor: function (config) {
    // Your preprocessing here
    this.superclass.constructor.apply(this, arguments);
    // Your postprocessing here
  },
  //初始化控件
  initComponent: function () {
    this.init();
    this.isComponentInited = true;
    this.callParent();
  },
  //显示Panel
  show: function () {
    this.callParent();
  },
  //创建工具栏控件
  init: function () {
    var record = this.dataRecord;
    var displayText = this.title;
  },
  //创建工具条
  createToolItem: function () {
    var vcl = this.vcl;
    var permission = this.permission;

    var me = this;

    function newDoc(docType) {
      var dirId = me.dirDocPanel.curDirId;
      var dirType = me.dirDocPanel.curDirType;
      var dirDocPanel = me.dirDocPanel;
      //对于新增的文档需要使用文档的vcl来检查新增权限
      var docVcl = Ax.utils.LibVclSystemUtils.getVcl("dm.Document", BillTypeEnum.Master);
      if (docVcl.checkCan(dirId, '', DMPermissonEnum.Add)) {
        Ext.Msg.prompt("输入新建文档的文件名", "文件名", function (btn, text) {
          if (btn == 'ok') {
            if (text == '') {
              Ext.Msg.alert("提示", "文件名不能为空!");
            }
            else {
              var reg = /[\/\\"<>\?\*:|]/;
              if (reg.test(text))
              {
                Ext.Msg.alert("提示", "文件名不能包含/\"<>?*:|这些特殊字符。");
              }
              else if (text == '.txt' || text == '.doc' || text == '.docx' || text == '.ppt' || text == '.pptx' || text == '.xls' || text == '.xlsx') {
                Ext.Msg.alert("提示", "文件名不能为空!");
              }
              else
              {
                //Ext.Msg.alert("结果", "你输入了：" + text);
                var fileWindow = Ext.create('DocumentManage.DMFileWindowForm', {});
                fileWindow.addDoc(dirId, docType, dirType, text);
              }
            }
          }
          else { }
        }, this, false, "新文档" + docType);
      }
    };

    //新增文档
    var addNewDoc = Ext.create(Ext.Action, {
      xtype: 'splitbutton',
      text: '新增文档',
      handler: function () {
        newDoc('.docx');
      },
      menu: [
        {
          text: 'Word',
          handler: function () {
            newDoc('.docx');
          }
        },
        {
          text: 'Excel',
          handler: function () {
            newDoc('.xlsx');
          }
        },
        {
          text: 'PowerPoint',
          handler: function () {
            newDoc('.pptx');
          }
        },
        {
          text: 'Text',
          handler: function () {
            newDoc('.txt');
          }
        },
        //{
        //    text: 'Visio',
        //    handler: function () {
        //        newDoc('.vsdx');
        //    }
        //},
        //{
        //    text: '其他类型',
        //    handler: function () {
        //        alert('to do new other type doc');
        //    }
        //},
      ]
    });

    //新增
    var addNew = null;
    function addNewOnly(arguments) {
      if (arguments[0]["noOpen"] === undefined) {
        //要新增新的对象，通过调用文档主Panel的重置底部面板的方式构造新的面板
        if (me.dirDocPanel) {
          me.dirDocPanel.showBottomTab(me.isDir, BillActionEnum.AddNew, true, false, me.dirType);
        }
      }
      else {
        if (me.isDir) {
          //如果是目录对象，则根据当前所处的目录类型设定新目录的类型，如“公共”或“个人”
          var masterRow = me.vcl.dataSet.getTable(0).data.items[0];
          var thisDirType = DirTypeEnum.Public;
          if (me.dirType == DirTypeEnum.PublicRoot)
            thisDirType = DirTypeEnum.Public;
          else if (me.dirType == DirTypeEnum.PrivateRoot)
            thisDirType = DirTypeEnum.Private;
          else
            thisDirType = me.dirType;

          masterRow.data.DIRTYPE = thisDirType;

          //新增目录时自动将当前选中的目录作为父目录
          if (me.isToCopy == false)
            masterRow.data.PARENTDIRID = me.dirDocPanel.curDirId;
          else
          {
            if (!masterRow.data.PARENTDIRID)
              masterRow.data.PARENTDIRID = me.dirDocPanel.curParentDirId;
          }

        }
        else {
          var masterRow = me.vcl.dataSet.getTable(0).data.items[0];
          //新增文档时自动将当前选中的目录作为目录
          masterRow.data.DIRID = me.dirDocPanel.curDirId;
        }
        //第一次展示创建新的对象时，noOpen参数非空，此时直接设置按钮状态即可
        setAction(vcl.isEdit);
      }
    }
    if (this.isDir && (this.dirType == DirTypeEnum.PublicRoot || this.dirType == DirTypeEnum.PrivateRoot)) {
      //公共目录根目录或个人目录根目录
      addNew = Ext.create(Ext.Action, {
        text: '新增目录',
        handler: function () {
          addNewOnly(arguments);
        }
      });
    }
    else {
      addNew = Ext.create(Ext.Action, {
        xtype: 'splitbutton',
        text: (this.isDir) ? '新增目录' : '新增文档信息',
        handler: function () {
          addNewOnly(arguments);
        },
        menu: [
          {
            text: '复制',
            handler: function () {
              if (me.dirDocPanel) {
                me.dirDocPanel.showBottomTab(me.isDir, BillActionEnum.AddNew, true, true, me.dirType);
              }
            }
          }
        ]
      });
    }
    this.actionAddNew = addNew;

    var progId = "dm.Directory";
    var queryCondition ;
    var proxyVcl = {
      proxy: true,
      progId: progId,
      formCallBackHandler: function (tag, param) {
        if (tag == 'SYSTEM_QUERY') {
          var containsPARENTDIRID = false;
          var containsDIRTYPE = false;

          var dirType = null;
          if (me.dataRecord["DIRTYPE"] == DirTypeEnum.PublicRoot)
            dirType = DirTypeEnum.Public;
          else if (me.dataRecord["DIRTYPE"] == DirTypeEnum.PrivateRoot)
            dirType = DirTypeEnum.Private;
          else
            dirType = me.dataRecord["DIRTYPE"];

          var queryDirId = '';
          if (me.dataRecord["DIRTYPE"] == DirTypeEnum.PublicRoot ||
            me.dataRecord["DIRTYPE"] == DirTypeEnum.PrivateRoot) {
            queryDirId = '';
          }
          else {
            if (me.isDir)
              queryDirId = me.dataRecord.TAG["DIRID"];
            else
              queryDirId = me.dataRecord["DIRID"];
          }

          for (var i = 0; i < param.condition.length; i++) {
            if(param.condition[i].Name=='PARENTDIRID')
            {
              param.condition[i].QueryChar = 1;
              param.condition[i].Value = [queryDirId];
              containsPARENTDIRID = true;
            }
            if (param.condition[i].Name == 'DIRTYPE') {
              param.condition[i].QueryChar = 1;
              param.condition[i].Value = [dirType];
              containsDIRTYPE = true;
            }
          }
          if (containsPARENTDIRID == false) {
            param.condition.push({
              Name: 'PARENTDIRID',
              QueryChar: 1,
              Value: [queryDirId]
            });
          }
          if (containsDIRTYPE == false) {
            param.condition.push({
              Name: 'DIRTYPE',
              QueryChar: 1,
              Value: [dirType]
            });
          }
          queryCondition = { QueryFields: param.condition };
          me.dirDocPanel.refreshData(Ext.encode(queryCondition));
        }
      }
    };
    //搜索
    var search = Ext.create(Ext.Action, {
      text: '搜索',
      handler: function () {
        var query;
        if (queryCondition)
          query = queryCondition.QueryFields;
        Ax.utils.LibQueryForm.createForm(proxyVcl, progId, query);
      }
    });

    //编辑记录
    var editItem = Ext.create(Ext.Action, {
      text: '编辑信息',
      handler: function () {
        var success = true;
        if (vcl.isEdit) {
          var billAction = vcl.billAction;
          success = vcl.doSave();
          //if (me.isDir == false && success) {
          //    me.refresh(billAction != BillActionEnum.AddNew);//保存后自动刷新父目录
          //}

        }
        else {
          if (me.isDir && (me.dirType == DirTypeEnum.PublicRoot || me.dirType == DirTypeEnum.PrivateRoot))
          {
            Ext.Msg.alert('提示', '根目录不可修改！');
            success = false;
          }
          else
          {
            success = vcl.doEdit();
            if (me.isDir == false) {
              var docId = me.dataRecord["DOCID"];              //文档编号
              vcl.AddDocOpLog(docId, '开始编辑文档信息。',false);
            }
          }

        }
        if (success)
          setAction(vcl.isEdit);
      }
    });
    this.actionEdit = editItem;

    //修改代码
    var editID = Ext.create(Ext.Action, {
      text: '修改代码',
      handler: function () {
        alert('to do edit id');
      }
    });
    //设定版本
    var setVersion = Ext.create(Ext.Action, {
      text: '设定版本',
      handler: function () {
        var docId = me.dataRecord["DOCID"];              //文档编号
        if (vcl.checkCan('', docId, DMPermissonEnum.SetVersion)) {

          var masterRow = me.vcl.dataSet.getTable(0).data.items[0];
          var oldVersion = masterRow.data['DOCVERSION'];

          var panel = Ext.create('Ext.form.Panel', {
            bodyPadding: 10,
            frame: true,
            renderTo: Ext.getBody(),
            items: [
              Ext.create('Ext.form.TextField', {
                fieldLabel: '版本:', name: 'newVersion', anchor: '95%', value: oldVersion,
              }),
              Ext.create('Ext.form.TextArea', {
                fieldLabel: '说明:', name: 'setDesc', anchor: '95%',
              }),
            ],
            buttons: [{
              text:'设定',
              handler: function () {
                var form = this.up('form').getForm();
                var thisMe = this;
                if (form.isValid()) {
                  var newVersion = form.getFields().items[0].getValue();
                  if (newVersion == oldVersion) {
                    Ext.Msg.alert('错误', '新版本号不能于老版本号相同.');
                    return;
                  }
                  var desc = form.getFields().items[1].getValue();
                  if (vcl.SetVersion(docId, oldVersion, newVersion, desc)) {
                    //设定成功
                    Ext.Msg.alert('提示', '设定版本成功.');
                    this.up('window').close();
                  }
                }
              }
            },
              {
                text: '取消',
                handler: function () {
                  this.up('window').close();
                }
              }
            ]
          });
          win = Ext.create('Ext.window.Window', {
            autoScroll: true,
            width: 400,
            height: 200,
            layout: 'fit',
            title:'设定版本',
            constrainHeader: true,
            minimizable: true,
            maximizable: true,
            items: [panel]
          });
          win.show();

        }
      }
    });
    //锁定文档
    var lockDoc = Ext.create(Ext.Action, {
      text: '锁定文档',
      handler: function () {
        alert('to do lock doc');
      }
    });

    //编辑文档
    var editDoc = Ext.create(Ext.Action, {
      text: '编辑文档',
      handler: function () {
        var docId = me.dataRecord["DOCID"];              //文档编号
        var lockState = me.dataRecord["LOCKSTATE"];      //锁定状态
        var docType = me.dataRecord["DOCTYPE"];
        var dirType = me.dataRecord["DIRTYPE"];
        var dirId = me.dirDocPanel.curDirId;
        var docName = me.dataRecord["DOCNAME"];
        if (lockState == 1) {
          Ext.Msg.alert('提示', '文档已经被锁定，不可编辑！');
          return;
        }
        if (me.vcl.checkCan('', docId, DMPermissonEnum.Edit)) {
          vcl.AddDocOpLog(docId, '开始编辑文档。');

          var fileWindow = Ext.create('DocumentManage.DMFileWindowForm', {});
          fileWindow.editDoc(docId, docType, dirId, dirType, docName);
        }
      }
    });
    //删除
    var deleteItem = Ext.create(Ext.Action, {
      text: (this.isDir)?'删除目录':'删除文档',
      handler: function () {
        var tipInfo = '';
        var warnTitle = '';
        if (me.isDir)
        {
          tipInfo = '注意：继续操作将删除整个目录及子目录，且无法恢复，请谨慎操作!<br />如果目录包含大量文件，需要较长时间完成本操作。';
          warnTitle = '警告:彻底删除目录';
        }
        else {
          tipInfo = '确定将所选文档删除吗?';
          warnTitle = '警告:彻底删除文档';
        }
        Ext.MessageBox.confirm(warnTitle,
          tipInfo, function (btn) {
            if (btn == 'yes') {
              var success = vcl.doDelete(vcl.currentPk);
              if (success) {
                me.refresh(me.isDir);//刷新父目录(对于目录)或刷新当前目录(对于文档)
                //me.dirDocPanel.refreshDir(me.isDir);//刷新父目录(对于目录)或刷新当前目录(对于文档)
              }
            }
          });

      }
    });
    //撤销
    var cancel = Ext.create(Ext.Action, {
      text: '撤销',
      handler: function () {
        if (vcl.billAction == BillActionEnum.AddNew) {
          //新增的撤销
          if (me.oldPks) {
            //当根目录新增目录撤销
            if (me.oldPks[0] === undefined) {
              vcl.isEdit = false;
              setAction(vcl.isEdit);
              vcl.billAction = BillActionEnum.Browse;
              vcl.currentPk = me.oldPks;
              me.refresh(false);
            }
            else {
              vcl.isEdit = false;
              setAction(vcl.isEdit);
              vcl.billAction = BillActionEnum.Browse;

              vcl.currentPk = me.oldPks;
              vcl.browseTo(vcl.currentPk);
            }
          }
          else
          {
            me.dirDocPanel.refreshDir();//如果没有oldPks一般为从目录中点击“上传文档”时的操作
          }
        }
        else
        {
          //编辑的撤销
          vcl.cancel(vcl.billAction);
          vcl.isEdit = false;
          setAction(vcl.isEdit);
          vcl.billAction = BillActionEnum.Browse;
        }

      }
    });
    //上传
    var upload = Ext.create(Ext.Action, {
      text: '上传文档',
      handler: function () {
        var uploader= Ext.create('DocumentManage.DMFileUploaderForm', {});
        uploader.show(me.dirDocPanel.curDirId, me.dirDocPanel.curDirType, me.dirDocPanel);

      }
    });
    //下载
    var download = Ext.create(Ext.Action, {
      text: '下载',
      handler: function () {
        var docId = me.dataRecord["DOCID"];              //文档编号
        if (me.vcl.checkCan('', docId, DMPermissonEnum.Download)) {

          vcl.AddDocOpLog(docId, '下载了文档。');

          var url = '/document/Download';
          url += '?docId=' + docId + '&userHandle=' + UserHandle + '&modifyVerId=-1';

          try{
            var elemIF = document.createElement("iframe");
            elemIF.src = url;
            elemIF.style.display = "none";
            document.body.appendChild(elemIF);
          }catch(e){
            console.log(e);
          };
        }
      }
    });
    //阅读
    var read = Ext.create(Ext.Action, {
      text: '阅读',
      handler: function () {
        var docId = me.dataRecord["DOCID"];              //文档编号
        var docType = me.dataRecord["DOCTYPE"];
        var docName = me.dataRecord["DOCNAME"];
        if (me.vcl.checkCan('', docId, DMPermissonEnum.Read)) {
          vcl.AddDocOpLog(docId, '阅读文档。');
          var modifyVerionId = -1;       //文档的修订号
          var fileWindow = Ext.create('DocumentManage.DMFileWindowForm', {});
          fileWindow.readOnly(docId, modifyVerionId, docType, docName);
        }
      }
    });

    //发送
    var send = Ext.create(Ext.Action, {
      xtype: 'splitbutton',
      text: '发送',
      handler: function () {
        alert('to do send to default');
      },
      menu:
        [
          {
            text: '至常用文档',
            handler: function () {
              alert('to do send to normal');
            },
          },
          {
            text: '至目录',
            handler: function () {
              alert('to do send to dir');
            },
          },
          {
            text: '至用户',
            handler: function () {
              alert('to do send to user');
            },
          },
          {
            text: '至邮件',
            handler: function () {
              alert('to do send to email');
            },
          },
          {
            text: '借出',
            handler: function () {
              alert('to do send to lend');
            },
          },
        ]
    });
    //重命名
    var rename = Ext.create(Ext.Action, {
      text: '重命名',
      handler: function () {
        alert('to do rename');
      }
    });
    //更新
    var replace = Ext.create(Ext.Action, {
      text: '更新',
      handler: function () {
        var uploader = Ext.create('DocumentManage.DMFileUploaderForm', { });
        uploader.isAddNew = false;
        uploader.docId = me.dataRecord["DOCID"];
        uploader.show(me.dirDocPanel.curDirId, me.dirDocPanel.curDirType, me.dirDocPanel);
      }
    });
    //移动
    var move = Ext.create(Ext.Action, {
      text: '移动',
      handler: function () {
        alert('to do move');
      }
    });
    //关联
    var associate = Ext.create(Ext.Action, {
      text: '关联',
      handler: function () {
        alert('to do Associate');
      }
    });
    //刷新
    var refresh = Ext.create(Ext.Action, {
      text: '刷新',
      handler: function () {
        me.refresh(false);
        //if (me.isDir) {
        //    //目录的刷新由父容器dirDocPanel负责
        //    me.dirDocPanel.refreshDir();
        //    return;
        //}
        //vcl.browseTo(vcl.currentPk);
        //setAction(false);
      }
    });

    //打印
    var print = Ext.create(Ext.Action, {
      text: '打印',
      handler: function () {
        var docId = me.dataRecord["DOCID"];              //文档编号
        var docType = me.dataRecord["DOCTYPE"];
        var docName = me.dataRecord["DOCNAME"];
        if (me.vcl.checkCan('', docId, DMPermissonEnum.Print)) {

          vcl.AddDocOpLog(docId, '打印文档。');

          var modifyVerionId = -1;       //文档的修订号
          var fileWindow = Ext.create('DocumentManage.DMFileWindowForm', {});
          fileWindow.print(docId, modifyVerionId, docType, docName);
        }
      }
    });
    //导出
    var exportDocs = Ext.create(Ext.Action, {
      text: '导出文档',
      handler: function () {
        alert('to do output docs of dir!');
        //var fileName = vcl.exportData();
        //if (fileName && fileName !== '') {
        //    DesktopApp.IgnoreSkip = true;
        //    try {
        //        window.location.href = '/TempData/ExportData/' + fileName;
        //    } finally {
        //        DesktopApp.IgnoreSkip = false
        //    }
        //}
      }
    });
    var saveDisplayScheme = Ext.create(Ext.Action, {
      xtype: 'splitbutton',
      text: '保存方案',
      handler: function () {
        vcl.saveDisplayScheme();
      },
      menu: [
        {
          text: '存为默认',//管理员可以选择列兵编辑GridScheme
          handler: function () {
            alert('to do save gridscheme for dm module');
          },
        },
        {
          text: '删除方案',
          handler: function () {
            vcl.clearDisplayScheme();
          },
        }
      ]
    });

    //Action列表
    var items = null;
    if (this.isDir == false) {
      items = [search,refresh, upload, addNewDoc, editItem, cancel, deleteItem, read, editDoc, replace, download, print,
        //send, rename, move, associate,
        setVersion,
        //lockDoc,
        //saveDisplayScheme
      ];//文档操作按钮
    }
    else {
      if (this.dirType == DirTypeEnum.PublicRoot || this.dirType == DirTypeEnum.PrivateRoot) {
        items = [search, refresh, addNew, editItem, cancel,
          // exportDocs,
          //saveDisplayScheme
        ];//根目录操作
      }
      else
        items = [search, refresh, addNew, editItem, cancel, deleteItem, upload, addNewDoc,
          //exportDocs,
          //saveDisplayScheme
        ];//目录操作
    }
    //设置按钮状态
    function setAction(isEdit) {
      if (isEdit) {
        addNew.setDisabled(true);
        editItem.setText("保存");
        cancel.setDisabled(false);
        refresh.setDisabled(true);
        deleteItem.setDisabled(true);
      } else {
        addNew.setDisabled(false);
        editItem.setText("修改");
        cancel.setDisabled(true);
        refresh.setDisabled(false);
        deleteItem.setDisabled(false);
      }
    };
    setAction(vcl.isEdit);
    this.setAction = setAction;

    this.removeAll();//移除工具条原先的所有条目
    this.doLayout();

    var toolBaritems = [];
    for (var i = 0; i < items.length; i++) {
      if (items[i].initialConfig.menu)
      {
        toolBaritems.push(Ext.create('Ext.button.Split', items[i]));
      }
      else
      {
        toolBaritems.push(Ext.create('Ext.button.Button', items[i]));
      }
    }
    if (this.items) {
      for (var i = 0; i < toolBaritems.length; i++) {
        this.items.add(toolBaritems[i]);
      }
    }
    else
      this.items = toolBaritems;
    this.doLayout();
  },
  refresh: function (isRefreshParent)
  {
    this.dirDocPanel.refreshDir(isRefreshParent);
    //if (this.isDir) {
    //    //目录的刷新由父容器dirDocPanel负责
    //    this.dirDocPanel.refreshDir(isRefreshParent);
    //    return;
    //}
    //this.setAction(false);
  },

  //设置切换工具条的类型
  setToolType: function (isDir,vcl,dirType) {
    this.isDir = isDir;
    this.vcl = vcl;
    this.dirType = dirType;
    this.createToolItem();
  },
  //调用AddNew方法
  callAddNew: function (config) {
    this.actionAddNew.execute(config);
  },
  //调用Edit方法
  callEdit: function (config) {
    this.actionEdit.execute(config);
  },
});
