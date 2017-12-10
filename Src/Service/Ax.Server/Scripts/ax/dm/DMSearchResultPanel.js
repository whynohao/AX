/**
 * Created by ZhangKj on 2017/3/31.
 */
/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：全文检索搜索后的结果显示tab
 * 创建标识：CHENQI 2016/11/10
 * 修改标识：
 * 修改描述：
 ************************************************************************/

Ext.define("DocumentManage.DMSearchResultPanel", {
  extend: 'Ext.panel.Panel',
  alias: 'widget.searchResultTab',

  dirDocViewTab:null,             //文档管理主Tab页

  constructor: function (config) {
    this.superclass.constructor.apply(this, arguments);
  },
  //初始化控件
  initComponent: function () {
    this.init();
    this.isComponentInited = true;
    this.callParent();
  },

  //创建控件
  init: function () {
    var me = this;
    me.layout = 'border';
    me.autoScroll = true;
    me.bodyStyle = 'overflow-x:auto; overflow-y:auto;';

    me.items =
      [
        {
          region: 'center',
          xtype: 'panel',
          id:'showResults',
          autoScroll: true,
          layout: {
            type: 'vbox',
            align: 'stretch',
            padding: 10
          }
        },
        {
          region: 'south',
          xtype: 'toolbar',
          height: 40,
          id: 'search-footbar',
          style: {
            'background-color': '#46a3ff'
          },
          items: [
            '一共查出0条记录，当前第0页，共0页（共耗时0ms）',
            {
              text:'上一页',
              handler: function () {
                if (me.pageNum && me.keyword) {
                  if (me.pageNum > 1)
                    me.searchDocument(me.keyword, me.pageNum - 1, 1);
                  else
                    alert("无上一页");
                }
              }
            },
            {
              text:'下一页',
              handler: function () {
                if (me.pageNum && me.keyword) {
                  if (me.pageNum + 1 <= me.pageCount)
                    me.searchDocument(me.keyword, me.pageNum + 1, 2);
                  else
                    alert("无下一页");
                }
              }
            }
          ]
        }
      ];
  },

  searchDocument: function (keyword, pageNum, flag) { //flag=0 第一次查询，1：上一页， 2：下一页
    var me = this;
    Ext.Ajax.request({
      url: '/fullTextRetrieval/searchIndex',
      async: false,
      method: 'POST',
      jsonData: {
        "key": keyword,
        "pageNum": pageNum,
        "userHandle": UserHandle,
        "lastFileId": flag != 2 ? '' : me.lastFileId,
        "nextFileId": flag != 1 ? '' : me.nextFileId,
      },
      timeout: 60000,
      success: function (response) {
        var ret = Ext.decode(response.responseText);
        if (ret.SearchIndexResult) {
          me.deleteResults();
          var items = ret.SearchIndexResult.FileInfoItems;
          Ext.each(items, function (item, index) { me.addResultItem(item); if (index == 0) me.nextFileId = item.FileId; if (index == items.length - 1) me.lastFileId = item.FileId; });
          me.pageNum = pageNum;
          me.keyword = keyword;
          if (ret.SearchIndexResult.PageNum == 1 && !flag) {
            me.pageCount = ret.SearchIndexResult.PageCount;
            me.ResultsCount = ret.SearchIndexResult.ResultsCount;
          }
          var toolbar = Ext.getCmp("search-footbar");
          toolbar.items.get(0).getEl().dom.innerHTML = '共查出' + me.ResultsCount + '条记录，' +
            '当前第' + ret.SearchIndexResult.PageNum + '页，共' + me.pageCount + '页（共耗时' +
            ret.SearchIndexResult.SearchTime + 'ms）';

        }
        else {
          Ext.Msg.alert("提示", "未找到符合条件的文档!");
          var toolbar = Ext.getCmp("search-footbar");
          toolbar.items.get(0).getEl().dom.innerHTML = '共查出' + 0 + '条记录，' +
            '当前第' + 0 + '页，共' + 0 + '页（共耗时' +
            0 + 'ms）';
        }
      },
      failure: function () {
        Ext.Msg.alert("提示", "查询失败");
        var toolbar = Ext.getCmp("search-footbar");
        toolbar.items.get(0).getEl().dom.innerHTML = '共查出' + 0 + '条记录，' +
          '当前第' + 0 + '页，共' + 0 + '页（共耗时' +
          0 + 'ms）';
      }
    });
  },

  //添加一条记录
  addResultItem: function (obj) {
    //if (obj.Contents.length >= 200) {
    //    obj.Contents = obj.Contents.substr(0, 200) + '...';
    //}
    var panel = Ext.getCmp("showResults");
    panel.add({
      xtype: 'panel',
      padding: '5 40 0 40',
      html: '<div style="border-bottom: solid 1px #ccc; margin-top: 5px"><h2><a href="javascript:void(0)" onclick="jump(\''+ obj.DirId +'\', '+ obj.DirType +', \''+ obj.FileId +'\');">' + obj.FileName + '</a></h2><p style="text-indent:2em;">' + obj.Contents + '</p><p style="padding: 0px;margin: 0px;color: #555;font-size: 12px;text-align: right;margin-top: 10px;">所在目录:' + obj.Path + '</pclass="smallTag"></div>',
    });
  },

  //清除所有结果
  deleteResults: function () {
    var panel = Ext.getCmp("showResults");
    panel.removeAll();
  },
});


