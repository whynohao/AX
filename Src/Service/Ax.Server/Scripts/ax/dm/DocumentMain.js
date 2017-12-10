/**
 * Created by ZhangKj on 2017/3/31.
 */
/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的主窗口
 * 创建标识：Zhangkj 2016/11/28
 *
 * 修改标识：Zhangkj 2016/11/30
 *
 ************************************************************************/
//Ext.Loader.setPath('DocumentManage', '../../Scripts/desk/DocumentManage/');//定义文档管理模块类的引用路径
//Ext.Loader.setPath('DocumentManage', 'Scripts/desk/DocumentManage/');//定义文档管理模块类的引用路径
//Ext.Loader.setPath('DocumentManage', 'http://localhost:9921/Scripts/desk/DocumentManage/');
// 配置pageoffice服务器的ip和port
var pageOfficeServerConfig = {
  address: 'localhost',
  port: '8888'
}
//保存文档管理类的全局引用
var documentWindow = null;
//文档目录类型枚举
var DirTypeEnum = {
  Public: 0,              //公共目录
  Private: 1,             //个人目录

  PublicRoot: 10,         //公共目录的根目录，功能选项类型
  PrivateRoot: 11,        //个人目录的根目录，功能选项类型
  MyNormal: 12,           //我常用的文档，功能选项类型
  Recycle: 13,            //回收站目录，功能选项类型
};
//文档操作类型枚举。主要针对直接对文件本身的操作
var DocOpTypeEnum = {
  //上传文档
  Upload: 0,
  //新建文档
  AddNew: 1,
  //编辑文档
  Edit: 2,
  //替换
  Replace: 3,
};

var DMPermissonEnum = {
  Use: 1,
  /// <summary>
  /// 浏览权限。对文档来说仅能浏览文档信息，文档的内容还需要具有Read权限
  /// </summary>
  Browse: 2,
  Add: 4,
  Edit: 8,
  Delete: 16,
  Release: 32,
  CancelRelease: 64,
  Audit: 128,
  CancelAudit: 256,
  EndCase: 512,
  CancelEndCase: 1024,
  Invalid: 2048,
  CancelInvalid: 4096,
  /// <summary>
  /// 为目录批量导入文档
  /// </summary>
  Import: 8192,
  /// <summary>
  /// 导出目录下的有下载权限的文档
  /// </summary>
  Export: 16384,
  Print: 32768,
  /// <summary>
  /// 阅读权限
  /// </summary>
  Read: 0x10000,
  /// <summary>
  /// 上传权限
  /// </summary>
  Upload: 0x20000,
  /// <summary>
  /// 下载权限
  /// </summary>
  Download: 0x40000,
  /// <summary>
  /// 移动权限，如将文件从一个目录移动到另外一个目录
  /// </summary>
  Move: 0x80000,
  /// <summary>
  /// 设定版本权限，如设定文件的版本号
  /// </summary>
  SetVersion: 0x100000,
  /// <summary>
  /// 订阅权限
  /// </summary>
  Subscribe: 0x200000,
  /// <summary>
  /// 借出权限
  /// </summary>
  Lend: 0x400000,
  /// <summary>
  /// 链接（发送链接）权限
  /// </summary>
  Link: 0x800000,
  /// <summary>
  /// 关联（发送链接）权限
  /// </summary>
  Associate: 0x1000000,
  /// <summary>
  /// 评论 权限
  /// </summary>
  Comment: 0x2000000,
  /// <summary>
  /// 重命名 权限，如重命名文件的名字
  /// </summary>
  Rename: 0x4000000,
  /// <summary>
  /// 替换 权限，如使用新文件替换旧的文件
  /// </summary>
  Replace: 0x8000000,
  /// <summary>
  /// 管理 权限，如对一个具体对象（如文件目录）的所有权限（子目录的增删改、子目录下的文件所有权限等等、目录和文件的权限设置等）
  /// 如果权限项中没有对应的功能权限，而又需要做控制时可使用管理权限来检查
  /// </summary>
  Manage: 0x10000000,
  /// <summary>
  /// 回退权限。可对文档的修订版进行回退，使之成为最新版
  /// </summary>
  Fallback: 0x20000000,
};

Ext.define('DocumentManage.DocumentMain', {
  //extend: 'Ext.ux.desktop.Module',
  //requires加载需要的类时机是  当前类初始化之前被加载
  requires: [
    'Ext.data.TreeStore',
    'Ext.layout.container.Accordion',
    'Ext.toolbar.Spacer',
    'Ext.tree.Panel',
  ],
  //uses 加载需要的类时机是,当前类初始化之后被加载
  uses: [
    'DocumentManage.DMCommon',
    'DocumentManage.DirDocViewPanel',
    'DocumentManage.DMToolbar'
  ],
  id: 'document-win',
  title: '文档管理',
  iconCls: 'menuIco',
  init: function () {
    this.launcher = {
      text: '文档管理',
      iconCls: 'menuIco'
    };
  },

  documentWindowWin: null,//文档管理主窗口

  dirDocViewTab: null,//目录浏览选项卡

  renderToId: null,//窗口要渲染到的对象

  canUseFunc: function (progId) {
    var canUse = false;
    Ext.Ajax.request({
      url: '/billSvc/canUseFunc',
      method: 'POST',
      jsonData: {
        handle: UserHandle, progId: progId
      },
      async: false,
      timeout: 90000000,
      success: function (response) {
        var ret = Ext.decode(response.responseText);
        canUse = ret.CanUseFuncResult;
      }
    });
    return canUse;
  },

  //刷新某个目录节点
  refreshDir: function (parentNode) {
    var record = parentNode;
    if (record.data.DIRTYPE == DirTypeEnum.MyNormal) {
      //我常用的文档
    }
    else if (record.data.DIRTYPE == DirTypeEnum.Recycle) {
      //回收站
    }
    else {
      var me = this;
      if (me.dirDocViewTab) {
        me.dirDocViewTab.dataRecord = record.data;
        me.dirDocViewTab.config.tbar.dataRecord = record.data;
        me.dirDocViewTab.refreshData();
        me.dirDocViewTab.show();//显式调用一次显示方法，以便被切换到其他Tab页(如全文检索页)时能够激活文档管理主Tab页。
      }
      else {
        me.dirDocViewTab = me.createDirTab(record.data);
      }
      var childDatas = me.dirDocViewTab.listingLoadData;
      me.restSubDirNode(record, childDatas);
      record.expand();

      me.dirDocViewTab.curDirTreeNode = record;//记录当前的目录树父节点
      if (record.data.TAG) {
        me.dirDocViewTab.dataRecord = record.data.TAG;

        me.dirDocViewTab.dataRecord.PROGID = record.data.PROGID;
        me.dirDocViewTab.dataRecord.curPks = [record.data.TAG.DIRID];
        me.dirDocViewTab.dataRecord.billType = BillTypeEnum.Master;
      }
      me.dirDocViewTab.showBottomTab(true, BillActionEnum.Browse, false, false, record.data.DIRTYPE);//显示目录面板
    }
  },
  //重置子级目录节点
  restSubDirNode: function (parentNode, childDatas) {

    function removeChildrenRecursively (node) {
      if (!node) return;
      while (node.hasChildNodes()) {
        removeChildrenRecursively(node.firstChild);
        node.removeChild(node.firstChild);
      }
    }

    removeChildrenRecursively(parentNode);
    var item;
    var treeNode;
    for (var i = 0; i < childDatas.length; i++) {
      item = childDatas[i];
      if (item.ISDIR == false)
        continue;
      treeNode = parentNode.appendChild({
        MENUITEM: item.DIRNAME, text: item.DIRNAME, DIRTYPE: item.DIRTYPE,
        //父目录为空，类型为Public
        CONDITION: '{"QueryFields":[{"Name":"PARENTDIRID","QueryChar":1,"Value":["' + item.DIRID + '"]},{"Name":"DIRTYPE","QueryChar":1,"Value":[' + item.DIRTYPE + ']}]}',
        PROGID: 'dm.Directory', billType: BillTypeEnum.Master, leaf: false, extend: true, loaded: true, children: [],
        TAG: item, ISADDSUB: false,
      });
      item.treeNode = treeNode;
    }
    parentNode.data.ISADDSUB = true;
    //parenetNode.expand();
  },
  //根据菜单项创建左侧的菜单树，
  //并添加每个树节点的点击处理事件:对于Master和Bill会调用createDirTab获取并展示列表数据
  //对于DataFunc会打开DataFunc的窗口
  createTree: function (menuListing) {
    var me = this;
    var modelType = Ext.data.Model.schema.getEntity('LibMenuModel');
    if (modelType === null) {
      modelType = Ext.define("LibMenuModel", {
        extend: "Ext.data.Model",
        fields: [
          {name: 'MENUITEM', type: 'string'},
          {name: 'PROGID', type: 'string'},
          {name: 'PROGNAME', type: 'string'},
          {name: 'BILLTYPE', type: 'int'},
          {name: 'ENTRYPARAM', type: 'string'},
          {name: 'ISVISUAL', type: 'boolean'},
          {name: 'CONDITION', type: 'string'},
          {name: 'DIRTYPE', type: 'int'},//文档目录的类型，遵从DMCommon.js中的DirTypeEnum的定义
          {name: 'ISADDSUB', type: 'bool'},//是否已经根据获取到的数据创建子目录
          {name: 'TAG'},//关联的数据行
        ]
      })
    }
    var tree = Ext.create('Ext.tree.Panel', {
      rootVisible: false,
      flex: 1,
      width: '100%',
      heigth: '100%',
      autoScroll: true,
      displayField: 'MENUITEM',
      listeners: {
        beforeitemexpand: function (node, eOpts) {
          if (node.data.ISADDSUB == false) {
            if (me.dirDocViewTab) {
              var childDatas = me.dirDocViewTab.getBillListing(node.data.CONDITION);
              //如果还没有创建过子级目录节点则设置创建
              me.restSubDirNode(node, childDatas);
            }
          }
        },
        itemclick: function (scope, record, item, index, e, eOpts) {
          var billType = record.data.BILLTYPE;
          var progId = record.data.PROGID;
          var text = record.data.MENUITEM;
          var isVisual = record.data.ISVISUAL;
          switch (billType) {
            case BillTypeEnum.Master:
            case BillTypeEnum.Bill:
              if (progId) {
                if (me.canUseFunc(progId)) {
                  me.refreshDir(record);
                }
                else
                  alert('没有当前功能的使用权限。');
              }
              break;
            case BillTypeEnum.DataFunc:
              if (progId) {
                if (me.canUseFunc(progId))
                  Ax.utils.LibVclSystemUtils.openDataFunc(progId, text);
                else
                  alert('没有当前功能的使用权限。');
              }
              break;
            default:
              break;
          }
        }
      },
      store: Ext.create('Ext.data.TreeStore', {
        model: modelType,
        root: menuListing
      })
    });
    me.dirTree = tree;
    return tree;
  },
  //构造文档目录的一级树节点菜单按钮
  createMenu: function (renderTo) {

    var panel = Ext.create('Ext.panel.Panel', {
      flex: 1,
      height: '100%',
      renderTo: renderTo,
      layout: 'vbox',
      bodyStyle: 'background:transparent;padding:3px; ',//背景透明
      items: [
        this.createTree({
          children: [
            {
              id: 'nodePublic', MENUITEM: '公共文档', text: '公共文档', DIRTYPE: DirTypeEnum.PublicRoot,
              //父目录为空，类型为Public
              CONDITION: '{"QueryFields":[{"Name":"PARENTDIRID","QueryChar":1,"Value":[""]},{"Name":"DIRTYPE","QueryChar":1,"Value":[' + DirTypeEnum.Public + ']}]}',
              PROGID: 'dm.Directory', billType: BillTypeEnum.Master, leaf: false, extend: true, children: [],
              ISADDSUB: false,
            },
            {
              id: 'nodeMe', MENUITEM: '我的文档', text: '我的文档', DIRTYPE: DirTypeEnum.PrivateRoot,
              //父目录为空，类型为Private
              CONDITION: '{"QueryFields":[{"Name":"PARENTDIRID","QueryChar":1,"Value":[""]},{"Name":"DIRTYPE","QueryChar":1,"Value":[' + DirTypeEnum.Private + ']}]}',
              PROGID: 'dm.Directory', billType: BillTypeEnum.Master, leaf: false, extend: true, children: [],
              ISADDSUB: false,
            },
            //{
            //    id: 'nodeNormal', MENUITEM: '我常用的文档', text: '我常用的文档', DIRTYPE: DirTypeEnum.MyNormal,
            //    PROGID: 'dm.Directory', billType: BillTypeEnum.Master, leaf: true, extend: true, children: [],
            //    ISADDSUB: false,
            //},
            //{
            //    id: 'nodeRecycle', MENUITEM: '回收站', text: '回收站', DIRTYPE: DirTypeEnum.Recycle,
            //    PROGID: 'dm.Directory', billType: BillTypeEnum.Master, leaf: true, extend: true, children: [],
            //    ISADDSUB: false,
            //},
          ]
        }),
      ]
    });
    return panel;
  },
  //创建目录浏览窗口Tab页，并获取数据
  createDirTab: function (record) {
    var me = this;
    var entryParam = "";
    var jsonString;
    var temp;
    if (record["ENTRYPARAM"] != "") {
      jsonString = JSON.stringify(JSON.parse(record["ENTRYPARAM"]).ParamStore);
      jsonString = jsonString.substring(jsonString.indexOf(':') + 1, jsonString.indexOf('}'));
      temp = jsonString.split(",");
      for (var i = 0; i < temp.length; i++) {
        entryParam = entryParam + temp[i].substring(temp[i].indexOf(":") + 1);
      }
    }
    var tabId = 'DocumentManage_' + record["PROGID"].replace(".", '') + entryParam.replace(new RegExp('"', 'gm'), '');
    var displayText = '文档管理';
    //var tab = me.tabPanel.items.get(tabId);
    if (this.dirDocViewTab) {
      //如果已经存在，则切换定向到具体的文档目录
      this.dirDocViewTab.dataRecord = record;
      this.dirDocViewTab.show();
    }
    else {
      var curVcl = Ax.utils.LibVclSystemUtils.getVcl(record.PROGID, BillTypeEnum.Master);//初始一般是目录
      //如果目录浏览的标签页还没有，则创建
      this.dirDocViewTab = Ext.create('DocumentManage.DirDocViewPanel', {
        id: tabId,
        title: displayText,
        closable: false,//作为不可关闭的标签页
        layout: {type: 'vbox', align: 'stretch'},
        border: false,
        dataRecord: record,
        docPanelWindow: this,//记录对于父容器的引用
        tbar: Ext.create('DocumentManage.DMToolbar', {
          dataRecord: record,
          isDir: true,             //是否是目录的工具条
          vcl: curVcl
        }),
        vcl: curVcl
      });
      var tab = me.tabPanel.add(this.dirDocViewTab);
      tab.closable = false;
      tab.show();
      //var addTab = function () {
      //    me.tabPanel.add(this.dirDocViewTab).show();
      //}();
    }
    return this.dirDocViewTab;
  },
  addTabPanel: function (panel) {
    var tab = this.tabPanel.add(panel);
    tab.show();
    return tab;
  },
  //创建并显示文档管理主窗口
  createWindow: function (renderToHtmlId) {
    var me = this;
    documentWindow = me;
    var win, desktop;

    if (me.app) {
      desktop = me.app.getDesktop();
      win = desktop.getWindow(me.id);
    }
    if (!win) {

      this.dirDocViewTab = null;//重新打开窗口时将panel 设置为null

      var width, height;
      var items = [];
      items.push(me.createMenu());
      var splitter = Ext.create('Ext.resizer.Splitter', {
        border: 5,
        style: {
          borderColor: '#eae4e4',
          borderStyle: 'solid'
        }
      });
      items.push(splitter);

      this.rightCommonPanel = Ext.create('DocumentManage.DMCommon', {height: 40,});
      this.rightCommonPanel.documentWindow = this;

      me.tabPanel = Ext.create('Ext.tab.Panel', {
        flex: 8,
        height: '80%',
      });

      var rightPanel = Ext.create('Ext.panel.Panel', {
        layout: {type: 'vbox', align: 'stretch'},
        width: '80%',
        height: '100%',
        items: [this.rightCommonPanel, me.tabPanel],
        border: false
      });

      //items.push(me.tabPanel);
      items.push(rightPanel);

      width = document.body.clientWidth * 0.8;
      height = document.body.clientHeight * 0.9;
      var mainWidth = document.body.clientWidth > 1210 ? document.body.clientWidth - 27 : 1210;
      var mainPanel = Ext.create('Ext.panel.Panel', {
        anchor: '100% 100%',
        width: mainWidth,
        height: document.body.clientHeight - 40,
        layout: {type: 'hbox', align: 'stretch'},
        items: items,
        border: false
      });
      if (desktop) {
        win = desktop.createWindow({
          id: me.id,
          title: me.title,
          width: width,
          height: height,
          iconCls: me.iconCls,
          animCollapse: false,
          constrainHeader: true,
          border: false,
          layout: 'anchor',
          //autoScroll: true,
          items: mainPanel
        });
      }
      else {
        var renderTo;
        if (renderToHtmlId)
          renderTo = renderToHtmlId;
        else
          renderTo = me.renderToId;

        win = Ext.create('Ext.window.Window', {
          id: me.id,
          title: me.title,
          width: width,
          height: height,
          iconCls: me.iconCls,
          animCollapse: false,
          constrainHeader: true,
          border: false,
          layout: 'anchor',
          //autoScroll: true,
          items: mainPanel,
          renderTo: renderTo,

          stateful: false,
          isWindow: true,
          constrainHeader: true,
          maximizable: true,
          maximized: true,
        });
      }

      this.documentWindowWin = win;
      win.on('show', function () {
        //自动展开显示公共文档下的子目录
        var publicRoot = me.dirTree.getRootNode().firstChild;
        me.dirTree.getSelectionModel().select(publicRoot);
        me.dirTree.fireEvent("itemclick", me.dirTree, publicRoot);

      });
      win.on('destroy', function () {
        me.documentWindowWin = null;
      });
    }
    win.show();
    return win;
  },
  //创建并显示文档管理主面板
  createMainPanel: function (renderToHtmlId, panelId) {
    if (!panelId)
      panelId = Ext.id();
    var me = this;
    documentWindow = me;
    this.dirDocViewTab = null;//重新打开窗口时将panel 设置为null
    var items = [];
    items.push(me.createMenu());
    var splitter = Ext.create('Ext.resizer.Splitter', {
      border: 5,
      style: {
        borderColor: '#eae4e4',
        borderStyle: 'solid'
      }
    });
    items.push(splitter);

    this.rightCommonPanel = Ext.create('DocumentManage.DMCommon');
    this.rightCommonPanel.documentWindow = this;

    me.tabPanel = Ext.create('Ext.tab.Panel', {
      flex: 1,
    });

    var rightPanel = Ext.create('Ext.panel.Panel', {
      layout: {type: 'vbox', align: 'stretch'},
      flex: 6,
      height: '100%',
      items: [this.rightCommonPanel, me.tabPanel],
      border: false
    });

    items.push(rightPanel);

    var renderTo;
    if (renderToHtmlId)
      renderTo = renderToHtmlId;
    else
      renderTo = me.renderToId;
    const panel = Ext.create('Ext.panel.Panel', {
      containerpanel: true,
      id: panelId,
      isVcl: true,
      title: me.title,
      // autoScroll: true,
      width: '100%',
      height: '100%',
      layout: {type: 'hbox', align: 'stretch'},
      minimizable: false,
      maximizable: false,
      // modal: true,
      items: items,
      renderTo: renderTo
    });
    this.documentWindowWin = me;
    return panel;
  },
  /*
   自动展开第一个节点
   */
  autoExpandFirstNode(){
    var me = this;
    //自动展开显示公共文档下的子目录
    var publicRoot = me.dirTree.getRootNode().firstChild;
    me.dirTree.getSelectionModel().select(publicRoot);
    me.dirTree.fireEvent("itemclick", me.dirTree, publicRoot);
  }
});

