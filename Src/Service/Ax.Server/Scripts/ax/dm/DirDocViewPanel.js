/**
 * Created by ZhangKj on 2017/3/31.
 */
/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的目录浏览Tab页的页面Panel(集中展示文档目录和文档)
 * 创建标识：Zhangkj 2016/11/28
 *
 * 修改标识：Zhangkj 2016/11/30
 *
 ************************************************************************/
Ext.define('DocumentManage.DirDocViewPanel', {
  extend: 'Ext.panel.Panel',
  //uses 加载需要的类时机是,当前类初始化之后被加载
  uses: [
    'DocumentManage.DMCommon',              //通用代码
    'DocumentManage.DMToolbar',            //操作工具栏
  ],
  //id: 'document-dirView',
  dataRecord: null,//浏览的具体文档目录或文档记录
  docPanelWindow: null,//对于父容器的引用
  curDirTreeNode: null,//当前浏览的目录对象对应的目录树节点
  curDirId: null,//当前所在的文档目录，用于新增目录或文档时，直接指定父目录或目录
  curParentDirId: null,//当前目录所在的父文档目录，用于复制目录时，直接指定父目录


  listingData:null,
  listingLoadData:[],//分页数据
  pageSize :1000,     //设置为1000，表示只取前1000条，不再排序
  allpageCount : 1,
  listingLength: 1000,
  pageCount: 1,

  gridPanel: null,
  filterCbo:null,
  dateCbo: null,

  vcl:undefined,//保存在面板中的vcl属性，便于子控件在查找vcl时找到。刷新数据时根据是文档还是目录构建vcl类

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
  //获取数据并刷新控件
  refreshData: function (condition) {
    if (!condition||condition=='') {
      this.dataRecord.PROGID = 'dm.Directory';
      this.dataRecord.curPks = [this.dataRecord.DIRID];
      this.dataRecord.billType = BillTypeEnum.Master;

      var record = this.dataRecord;
      this.getBillListing(record.CONDITION);
    }
    else
    {
      this.getBillListing(condition);
    }
    var progId=this.dataRecord.PROGID;
    if(!progId)
      progId='dm.Directory';
    this.vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, BillTypeEnum.Master);
    var newend = 0;
    //如果返回数据为空且页数不为一则不读取
    if (this.listingData.Data.length > 0 || this.pageCount == 1) {
      this.gridPanel.pageCount = this.pageCount;
      if (this.gridPanel.store)
        this.gridPanel.store.removeAll();
      else {
        var listingStore = Ext.create('Ext.data.Store', {
          fields: Ext.decode(this.listingData.Fields),
          proxy: {
            type: 'memory',
            reader: {
              type: 'json'
            }
          },
          data: this.listingLoadData
        });
        this.gridPanel.store = listingStore;
      }
      this.gridPanel.store.loadData(this.listingData.Data);
    }
    else {
      this.pageCount--;
    }
  },
  ///获取数据
  getBillListing: function (condition) {
    var me = this;//为了在ajax请求的success回调中使用，需要先定义一个变量指向当前this( DirDocViewPanel)
    var fieldName = this.filterCbo.getValue();
    var curCondition = new Object();
    if (condition&&condition != "") {
      curCondition.QueryFields = Ext.decode(condition).QueryFields;
    }

    var filterFun = Ax.utils.LibQuickSelectBuilder.createSelectBar(BillTypeEnum.Master);
    var filter = filterFun[1]();//使用AX平台的条件筛选，如生效、结案等，默认是全选

    if (fieldName == 'all')
      fieldName = '';
    Ext.Ajax.request({
      url: '/billSvc/getBillListing',
      method: 'POST',
      jsonData: {
        listingQuery: {
          Handle: UserHandle, ProgId: 'dm.Directory',
          TimeFilter: me.dateCbo.getValue(),
          Condition: curCondition,
          Filter: filter,
          EntryParam:null,
          PageCount: me.pageCount,
          PageSize: me.pageSize
        }
      },
      async: false,
      timeout: 90000000,
      success: function (response) {
        var ret = Ext.decode(response.responseText);
        me.listingData = Ext.decode(ret.GetBillListingResult);
        var filterField = Ext.decode(me.listingData.FilterField);
        if (filterField.length > 0)
          me.filterCbo.store.loadData(filterField, true);
        me.listingLength = me.listingData.Data.length;
        if (me.listingLength >= me.pageSize) {
          for (var i = 0; i < me.pageSize; i++) {
            me.listingLoadData.push(me.listingData.Data[i]);
          }
        }
        else {
          me.listingLoadData = me.listingData.Data;
        }
        //allPageCount = Math.ceil(this.this.listingLength / this.pageSize);
        me.allPageCount = 0;
      }
    });
    return this.listingLoadData;
  },

  //创建页面控件
  init: function () {
    var record = this.dataRecord;
    var displayText = this.title;

    var filterStore = Ext.create('Ext.data.Store', {
      fields: ['key', 'value'],
      data: [{ key: 'all', value: '无' }],
      proxy: {
        type: 'memory',
        reader: {
          type: 'json'
        }
      }
    });
    var compareStore = Ext.create('Ext.data.Store', {
      fields: ['key', 'value'],
      data: [{ key: 0, value: '包含' },
        { key: 1, value: '等于' },
        { key: 2, value: '大于等于' },
        { key: 3, value: '小于等于' },
        { key: 4, value: '大于' },
        { key: 5, value: '小于' }]
    });
    var filterCbo = Ext.create('Ext.form.field.ComboBox', {
      fieldLabel: '字段',
      labelAlign: 'right',
      labelWidth: 60,
      margin: '0 2 2 2',
      flex: 1,
      queryMode: 'local',
      displayField: 'value',
      valueField: 'key',
      store: filterStore,
      editable: false,
      value: 'all'
    });

    this.filterCbo = filterCbo;

    var compareCbo = Ext.create('Ext.form.field.ComboBox', {
      labelWidth: 60,
      labelAlign: 'right',
      margin: '0 2 2 2',
      fieldLabel: '比较符',
      flex: 1,
      displayField: 'value',
      queryMode: 'local',
      valueField: 'key',
      store: compareStore,
      editable: false,
      value: 0
    });

    var compareTxt = Ext.create('Ext.form.field.Text', {
      labelWidth: 60,
      labelAlign: 'right',
      margin: '0 2 2 2',
      flex: 2
    });
    var dateStore = Ext.create('Ext.data.Store', {
      fields: ['key', 'value'],
      data: [{ key: 0, value: '无' },
        { key: 1, value: '近一周' },
        { key: 2, value: '近一月' },
        { key: 3, value: '近三月' }]
    });
    var dateCbo = Ext.create('Ext.form.field.ComboBox', {
      labelWidth: 60,
      labelAlign: 'right',
      margin: '0 2 2 2',
      fieldLabel: '时间',
      flex: 1,
      displayField: 'value',
      queryMode: 'local',
      valueField: 'key',
      store: dateStore,
      editable: false,
      value: 0
    });

    this.dateCbo = dateCbo;

    var getSelectPanel = function () {
      return Ext.create('Ext.form.Panel', {
        layout: { type: 'vbox', align: 'stretch' },
        items: [{
          xtype: 'fieldset',
          layout: { type: 'hbox', align: 'stretch' },
          border: false,
          items: [filterCbo, compareCbo, compareTxt, dateCbo]
        }]
      });
    }
    this.getBillListing(record.CONDITION);

    var listingStore = Ext.create('Ext.data.Store', {
      fields: Ext.decode(this.listingData.Fields),
      proxy: {
        type: 'memory',
        reader: {
          type: 'json'
        }
      },
      data: this.listingLoadData
    });
    var me = this;
    var gridPanel = Ext.create('Ext.grid.Panel', {
      pageCount: 1,
      pageSize:this.pageSize,
      flex: 1,
      store: listingStore,
      autoScroll: true,
      Pks:this.listingData.Pk,
      selType: 'checkboxmodel',
      multiSelect: true,

      height: 100,
      width:'100%',

      enableColumnMove: false,
      enableHdMenu: false,
      enableColumnHide: false,//不允许选择列

      columns: Ext.decode(this.listingData.Columns),
      plugins: 'gridfilters',
      listeners: {
        itemclick: function (self, record, item, index, e, eOpts) {
          me.itemClick(record.data);
        },
        itemdblclick: function (self, record, item, index, e, eOpts) {
          me.itemClick(record.data);
        }
      }
    });
    this.gridPanel = gridPanel;

    //this.tbar = Ext.create('DocumentManage.DMToolbar', {
    //    dataRecord: record,
    //    isDir: true,           //是否是目录的工具条
    //    isItemTool: false,       //是否是数据库记录的操作工具条
    //    vcl: Ax.utils.LibVclSystemUtils.getVcl(record.PROGID, BillTypeEnum.Master)
    //});

    this.config.tbar.createToolItem();//为配置的toolbar创建toolItem
    this.config.tbar.dirDocPanel = this;//将自身对象传递给toolbar

    this.bottomDirTabPanel = Ext.create('Ext.tab.Panel', {
      height: 300,
    });

    this.layout ={ type: 'vbox', align: 'stretch' };//垂直可伸缩

    var splitter = Ext.create('Ext.resizer.Splitter', {
      border: 5,
      style: {
        borderColor: '#eae4e4',
        borderStyle: 'solid'
      }
    });
    this.autoDestory = true;
    this.items = [gridPanel, splitter, this.bottomDirTabPanel];
  },
  //单击某行
  itemClick: function (recordData) {
    this.dataRecord = recordData;//更新当前的记录项
    this.config.tbar.dataRecord = recordData;
    if (this.dataRecord.ISDIR) {
      this.dataRecord.CONDITION = '{"QueryFields":[{"Name":"PARENTDIRID","QueryChar":1,"Value":["' + this.dataRecord.DIRID + '"]}]}';//为目录设置浏览条件
      //打开选择的目录
      this.curDirTreeNode = recordData.treeNode;
      this.refreshDir();
      this.showBottomTab(this.dataRecord.ISDIR, BillActionEnum.Browse, false, false, recordData["DIRTYPE"]);
      this.vcl= Ax.utils.LibVclSystemUtils.getVcl('dm.Directory', BillTypeEnum.Master);
    }
    else {
      this.dataRecord.PROGID = 'dm.Document';
      this.dataRecord.curPks = [this.dataRecord.DOCID];
      this.dataRecord.billType = BillTypeEnum.Master;
      this.showBottomTab(this.dataRecord.ISDIR, BillActionEnum.Browse);
      this.vcl= Ax.utils.LibVclSystemUtils.getVcl('dm.Document', BillTypeEnum.Master);
    }
  },
  //显示信息跳转到指定目录和指定文档（docId可空)
  jumpTo: function (dirId, dirType, docId) {
    if (!dirId || dirId == '')
    {
      Ext.Msg.alert('提示', '定位需要的文档目录编号为空。');
      return;
    }
    //先定位到目录
    var condition = '{"QueryFields":[{"Name":"PARENTDIRID","QueryChar":1,"Value":["' + dirId + '"]},{"Name":"DIRTYPE","QueryChar":1,"Value":[' + dirType + ']}]}';//为目录设置浏览条件
    this.refreshData(condition);//直接刷新目录数据

    this.curDirId = dirId; //重置当前目录编号和类型
    this.curDirType = dirType;

    this.curDirTreeNode = null;//因为左侧的目录树无法与跳转的文档目录对应（文档的目录层次可能很深），直接将其置空。

    if (docId && docId != ''&&this.listingLoadData) {
      //再定位到文档
      var docItem;
      for (var i = 0; i < this.listingLoadData.length; i++) {
        if(this.listingLoadData[i]["DOCID"]==docId)
        {
          docItem = this.listingLoadData[i];
          break;
        }
      }
      if (docItem)
        this.itemClick(docItem);//显示文档
    } else {
      this.showBottomTab(true, BillActionEnum.Browse, false, false, dirType);//显示目录面板
      this.vcl= Ax.utils.LibVclSystemUtils.getVcl('dm.Directory', BillTypeEnum.Master);
    }
  },
  //刷新目录
  refreshDir: function (isRefreshParent) {
    if (this.curDirTreeNode) {
      if (!isRefreshParent)
        this.docPanelWindow.refreshDir(this.curDirTreeNode);
      else {
        if (this.curDirTreeNode && this.curDirTreeNode.parentNode)
          this.docPanelWindow.refreshDir(this.curDirTreeNode.parentNode);
      }
    }
    else {
      var dirId= this.curDirId ;
      var dirType= this.curDirType;
      var condition = '{"QueryFields":[{"Name":"PARENTDIRID","QueryChar":1,"Value":["' + dirId + '"]},{"Name":"DIRTYPE","QueryChar":1,"Value":[' + dirType + ']}]}';//为目录设置浏览条件
      this.refreshData(condition);//直接刷新目录数据
    }

  },
  //隐藏所有的底部选项卡
  hideBottomTab: function ()
  {
    if (this.bottomDirTabPanel) {
      this.remove(this.bottomDirTabPanel);
      this.bottomDirTabPanel = null;
    }

    if (this.bottomDocTabPanel) {
      this.remove(this.bottomDocTabPanel);
      this.bottomDocTabPanel = null;
    }
    this.doLayout();
  },
  //显示底部信息的选项卡
  showBottomTab: function (isDir, billAction, isForNew, isToCopy, dirType) {
    this.hideBottomTab();

    var progId = "dm.Directory";
    if (!isDir)
      progId = "dm.Document";
    this.vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, BillTypeEnum.Master);

    if (billAction == BillActionEnum.AddNew) {
      //对于新增操作，增加入口参数
      if (isDir) {
        //仅对目录的AddNew做如下处理，文档的新增已经在另外的地方进行了
        if (this.dataRecord) {
          //增加后台AddNew权限检查时需要的的参数，以入口参数的形式提供
          var paramStore = {};
          if (vcl.entryParam)
            paramStore = vcl.entryParam["ParamStore"];
          else
          {
            vcl.entryParam = {};
            paramStore = {};
            vcl.entryParam["ParamStore"] = paramStore;
          }
          paramStore["ParentDirId"] = '' + this.dataRecord["DIRID"] + '';
          paramStore["ParentDirType"] = dirType;
        }
      }
    }

    var toolBar = this.config.tbar;
    toolBar.isToCopy = isToCopy;

    if (isDir) {

      if (isDir) {
        if (dirType == DirTypeEnum.PublicRoot || dirType == DirTypeEnum.PrivateRoot)
        //如果是公共目录根目录或个人目录根目录，则可以继续向下执行
          this.curDirId = '';//根目录时置空
        else
        {
          if (this.dataRecord) {
            this.curDirId = this.dataRecord.curPks[0];//记录当前目录Id
            this.curDirType = this.dataRecord["DIRTYPE"];
            this.curParentDirId = this.dataRecord["PARENTDIRID"];
          }
          else {
            this.curDirId = '';//置空
            this.curParentDirId = '';
          }

        }
      }

      toolBar.setToolType(true, this.vcl, dirType);//切换顶部按钮工具条
      this.bottomDirTabPanel = this.createBottomTabPanel(true, billAction, isForNew, isToCopy, dirType);
      if (this.bottomDirTabPanel != null) {
        //切换底部面板
        this.add(this.bottomDirTabPanel);
      }
    }
    else {
      toolBar.setToolType(false, this.vcl);//切换顶部按钮工具条
      this.bottomDocTabPanel = this.createBottomTabPanel(false, billAction, isForNew, isToCopy);
      if (this.bottomDocTabPanel != null) {
        //切换底部面板
        this.add(this.bottomDocTabPanel);
      }
    }
    this.doLayout();
  },
  //调用Ax机制创建底部信息面板
  createBottomTabPanel: function (isDir, billAction, isForNew, isToCopy, dirType) {
    if (isDir && (dirType == DirTypeEnum.PublicRoot || dirType == DirTypeEnum.PrivateRoot)) {
      //如果是公共目录根目录或个人目录根目录，则可以继续向下执行
    }
    else if (!isForNew || isToCopy ) {
      if (!this.dataRecord || !this.dataRecord.curPks || this.dataRecord.curPks.length == 0)
        return;//信息为null时不处理
    }
    var progId = "dm.Directory";
    if (!isDir)
      progId = "dm.Document";
    var billType = BillTypeEnum.Master;
    var displayText = this.title;
    var entryParam = '';

    var curPks = null;
    if ((!isForNew || isToCopy) && this.dataRecord && this.dataRecord.curPks) {
      if (this.dataRecord.curPks.length > 0 && this.dataRecord.curPks[0])
        curPks = this.dataRecord.curPks;
    }
    //if (this.dataRecord) {
    //    curPks = this.dataRecord.curPks;
    //}
    if (isForNew && this.dataRecord && this.dataRecord.curPks && this.dataRecord.curPks.length > 0) {
      this.config.tbar.oldPks = this.dataRecord.curPks;//新增业务对象时，保存当前的记录对象的主键，以便撤销时使用
    }
    var paramList = null;

    //Ax.utils.LibVclSystemUtils.openBill(progId, billType, displayText, BillActionEnum.Browse, entryParam != '' ? Ext.decode(entryParam) : undefined, this.dataRecord.curPks);
    //var vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, billType);
    var vcl = this.vcl;

    var view = Ax.utils.LibVclSystemUtils.getView(progId, billType, vcl);
    vcl.billType = billType;
    if (!vcl.entryParam)
      vcl.entryParam = entryParam;

    var checkDirId;
    if (billAction == BillActionEnum.AddNew) {
      //对于新增操作，增加入口参数
      if (isDir) {
        if (this.dataRecord) {
          checkDirId = this.dataRecord["PARENTDIRID"];
        }
      }
    }
    else
    {
      checkDirId = this.dataRecord["DIRID"];
    }
    if (!dirType)
      dirType = DirTypeEnum.Public;
    if (isDir)
      vcl.getTpl(dirType, checkDirId);//获取显示View的模板
    else
    {
      var docId = '';
      if (this.dataRecord && this.dataRecord["DOCID"]) {
        docId = this.dataRecord["DOCID"];
      }
      checkDirId = this.dataRecord["PARENTDIRID"];
      vcl.getTpl(dirType, checkDirId, docId);//获取显示View的模板
    }


    var win = this.up('window');//找到当前窗口
    var id;
    if (win) {
      id = win.id;
      vcl.winId = id;
      DesktopApp.ActiveWindow = id;
    }
    else {
      id =this.id+"_ForNoExistWin";
      vcl.winId = id;
      DesktopApp.ActiveWindow = id;
    }

    var billPanel = view[vcl.funcView.get("default").name](billAction, curPks, dirType, this.config.tbar);//根据自定义的View生成的将是一个TabPanel
    if (!billPanel)
      return null;
    else
    {
      if (win)
        win.vcl = vcl;
      //vcl.win = win;         //设置win窗口对象到vcl中后，业务处理过程中vcl会根据表单记录的生效、审核等状态设置窗口标题，对于文档管理系统不需要。
      var bottomPanel = Ext.create('Ext.panel.Panel', {
        items: [billPanel],
        layout:'fit',
        height:250,
      });

      return bottomPanel;
    }
  },
});
