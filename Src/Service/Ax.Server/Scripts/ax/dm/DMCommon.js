/**
 * Created by ZhangKj on 2017/3/31.
 */
/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：文档管理模块的通用操作控件
 * 创建标识：Zhangkj 2016/12/02
 *
 *
 ************************************************************************/
//跳转到文档管理窗口的目录和文档
function jump (dirId, dirType, docId) {
  documentWindow.dirDocViewTab.jumpTo(dirId, dirType, docId);
  documentWindow.dirDocViewTab.show();
};

Ext.define('DocumentManage.DMCommon', {
  //id: 'document-common',
  extend: 'Ext.panel.Panel',
  iconCls: 'menuIco',
  header: false,

  uses: [
    'DocumentManage.DMSearchResultPanel',              //全文检索结果Panel
  ],
  searchTabPanel: null,       //全文检索窗口的Tabpanel
  searchResultPanel: null,     //全文检索结果Panel
  documentWindow: null,        //对文档主窗口的引用

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
  //检索
  search: function () {
    var me = this;
    var keyWord = me.txtFullSearch.getValue();
    if (keyWord == '') {
      Ext.Msg.alert('提示', '请输入检索关键字！');
      return;
    }
    if (me.searchTabPanel == null) {
      if (me.documentWindow) {
        me.searchResultPanel = Ext.create('DocumentManage.DMSearchResultPanel',
          {
            title: '全文检索',
            layout: {type: 'vbox', align: 'stretch'},
            border: false,
            dirDocViewTab: me.documentWindow.dirDocViewTab
          });
        me.searchTabPanel = me.documentWindow.addTabPanel(me.searchResultPanel);
        me.searchResultPanel.searchDocument(keyWord, 1);
      }
    }
    else {
      if (me.searchResultPanel) {
        me.searchResultPanel.deleteResults();
        var keyWord = me.txtFullSearch.getValue();
        me.searchResultPanel.searchDocument(keyWord, 1);
      }
      me.searchTabPanel.show();
    }
  },
  //创建控件
  init: function () {
    this.layout = {
      type: 'vbox',
      align: 'left'
    };
    this.width = '100%'
    this.bodyStyle = 'padding:5px'
    var me = this;
    this.txtFullSearch = Ext.create('Ext.form.TextField', {
      fieldLabel: '全文检索', name: 'fullSearch',
      listeners: {
        specialkey: function (field, e) {
          if (e.getKey() == Ext.EventObject.ENTER) {
            me.search();
          }
        }
      }
    });
    this.btnSearch = Ext.create('Ext.Button', {
      text: '搜索',
      name: 'btnSearch',
      style: 'margin-left:5px',
      width: 100,
      handler: function () {
        me.search();
      }
    });

    this.items =
      [
        new Ext.Panel({
          layout: {
            type: 'hbox',
            align: 'center'
          },
          border: false,
          height: 40,
          items: [this.txtFullSearch, this.btnSearch]
        }),
      ];
  },
});
