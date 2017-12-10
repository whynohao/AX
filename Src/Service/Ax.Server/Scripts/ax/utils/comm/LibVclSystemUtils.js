Ext.ns('Ax.utils')
var vuexStore = null
var storeTypes = null

Ax.utils.LibVclSystemUtils = {
  loadVueStore: function (store, type) {
    vuexStore = store
    storeTypes = type
  },
  loadJs: function () {
    if (!DesktopApp.JsPath) {
      var ajaxCall = function () {
        Ext.Ajax.request({
          url: '/sysSvc/getJsPath',
          method: 'Get',
          async: false,
          timeout: 90000000,
          success: function (response) {
            var ret = Ext.decode(response.responseText);
            var jsPath = Ext.decode(ret.GetJsPathResult);
            Ext.Loader.setPath(jsPath);
            DesktopApp.JsPath = jsPath;
          }
        });
      }
      ajaxCall(this);
    }
  },
  getVcl: function (progId, billType) {
    var vclClass = progId.replace('.', '') + 'Vcl';
    if (DesktopApp.JsPath.hasOwnProperty(vclClass))
      Ext.Loader.syncRequire(vclClass);
    else
      vclClass = '';

    if (vclClass==='' && progId === 'dm.Document') {
      return undefined;
    }
    if (vclClass)
      vcl = new (eval(vclClass))();
    else {
      switch (billType) {
        case BillTypeEnum.Master:
        case BillTypeEnum.Bill:
          vcl = new Ax.vcl.LibVclData();
          break;
        case BillTypeEnum.Grid:
          vcl = new Ax.vcl.LibVclGrid();
          break;
        case BillTypeEnum.DataFunc:
          vcl = new Ax.vcl.LibVclDataFunc();
          break;
        case BillTypeEnum.Rpt:
          vcl = new Ax.vcl.LibVclRpt();
          break;
        case BillTypeEnum.DailyRpt:
          vcl = new Ax.vcl.LibVclDailyRpt();
          break;
        case BillTypeEnum.Visual:
          vcl = new Ax.vcl.LibVclVisual();
          break;
      }
    }
    vcl.progId = progId;
    return vcl;
  },
  getView: function (progId, billType, vcl) {
    var viewClass = progId.replace('.', '') + 'View';
    if (DesktopApp.JsPath.hasOwnProperty(viewClass))
      Ext.Loader.syncRequire(viewClass);
    else
      viewClass = '';
    if (viewClass) {
      view = new (eval(viewClass))(vcl);
    }
    else {
      switch (billType) {
        case BillTypeEnum.Master:
        case BillTypeEnum.Bill:
          view = new Ax.tpl.LibBillTpl(vcl);
          break;
        case BillTypeEnum.Grid:
          view = new Ax.tpl.LibGridTpl(vcl);
          break;
        case BillTypeEnum.DataFunc:
          view = new Ax.tpl.LibDataFuncTpl(vcl);
          break;
        case BillTypeEnum.Rpt:
          view = new Ax.tpl.LibRptTpl(vcl);
          break;
        case BillTypeEnum.DailyRpt:
          view = new Ax.tpl.LibDailyRptTpl(vcl);
          break;
        case BillTypeEnum.Visual:
          view = new Ax.tpl.LibVisualTpl(vcl);
          break;
      }
    }
    return view;
  },
  loadMenuData: function (setting) {
    var rootData;
    Ext.Ajax.request({
      url: '/fileTranSvc/loadMenuSetting',
      jsonData: {handle: window.UserHandle, setting: setting === true},
      method: 'POST',
      async: false,
      success: function (response) {
        var temp = Ext.decode(response.responseText);
        var menu = temp["LoadMenuSettingResult"];
        if (menu != '')
          rootData = Ext.decode(menu);
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
    return rootData;
  },
  getCurrentStateText: function (currentState) {
    var text, textColor;
    switch (currentState) {
      case 0:
        text = '草稿';
        break;
      case 1:
        text = '未生效';
        break;
      case 2:
        text = '生效';
        break;
      case 3:
        text = '作废';
        break;
      case 4:
        text = '结案';
        break;
    }
    return text;
  },
  getAuditStateText: function (auditState, flowLevel) {
    var text, textColor;
    switch (auditState) {
      case 0:
        text = '未提交审核 审核层级' + flowLevel;
        break;
      case 1:
        text = '已提交审核 审核层级' + flowLevel;
        break;
      case 2:
        text = '已审核 审核层级' + flowLevel;
        break;
      case 3:
        text = '未通过审核 审核层级' + flowLevel;
        break;
    }
    return text;
  },
  setWindowtab: function (vuexStore, types, param, state) {
    switch (state) {
      case 0:
        vuexStore.dispatch(types.WINDOW_TAB_CREATEITEM, {tab: param})
        break
      case 1:
        const index = findTabIndex(vuexStore.state.windowTab.items, param.id)
        vuexStore.dispatch(types.WINDOW_TAB_CHANGEITEM, {tab: param, index})
        break
      case 2:
        const idx = findTabIndex(vuexStore.state.windowTab.items, param)
        vuexStore.dispatch(types.WINDOW_TAB_DESTROYITEM, {index: idx})
        break
    }
    //  找到窗口对应的标签
    function findTabIndex (tabs, id) {
      for (var i = 0; i < tabs.length; i++) {
        if (tabs[i].id === id) {
          return i
        }
      }
    }
  },
  openBill: function (progId, billType, displayText, billAction, entryParam, curPks, paramList) {
    var vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, billType);
    var view = Ax.utils.LibVclSystemUtils.getView(progId, billType, vcl);
    vcl.billType = billType;
    vcl.entryParam = entryParam;
    vcl.getTpl();
    var curPk = "";
    if (curPks != undefined) {
      curPk = curPks[0];
    }
    var id = Ext.id();
    if (billAction == 2) {
      id = Ext.id();
      curPk = "";
    }

    var win = Ext.getCmp(id);
    if (!win) {
      DesktopApp.ActiveWindow = id;
      vcl.winId = id;
      var billPanel = view[vcl.funcView.get("default").name](billAction, curPks);
      if (!billPanel)
        return;
      var masterRow = vcl.dataSet.getTable(0).data.items[0];
      var title = Ax.utils.LibVclSystemUtils.getCurrentStateText(masterRow.get('CURRENTSTATE'));
      if (vcl.tpl.ShowAuditState) {
        var auditTxt = Ax.utils.LibVclSystemUtils.getAuditStateText(masterRow.get('AUDITSTATE'), masterRow.get('FLOWLEVEL'));

        title = curPk + displayText + '【' + title + '】' + '【' + auditTxt + '】';
      } else
        title = curPk + displayText + '【' + title + '】';
      if (vcl.billType == BillTypeEnum.Master) {
        if (!masterRow.get('ISVALIDITY'))
          title += '【已失效】';
      }
      var win = Ext.create("Ext.window.Window", {
        id: id,
        title: title,
        progName: displayText,
        autoScroll: true,
        width: document.body.clientWidth * 0.8,
        height: document.body.clientHeight * 0.9,
        constrainHeader: true,
        minimizable: true,
        maximizable: true,
        items: billPanel,
        vcl: vcl,
        listeners: {
          close: function (self, eOpts) {
            this.vcl.vclHandler(self, {libEventType: LibEventTypeEnum.FormClosed});
            if (vuexStore) {
              Ax.utils.LibVclSystemUtils.setWindowtab(vuexStore, storeTypes, win.id, 2)
            }
          },
          beforeclose: function (self, eOpts) {
            this.vcl.vclHandler(self, {libEventType: LibEventTypeEnum.FormClosing})
          },
          minimize: function (self, eOpts) {
            self.setVisible(false)
          }
        }
      });
      vcl.win = win;
      if (vuexStore) {
        var windowtab = {
          id: win.id,
          name: win.title
        }
        Ax.utils.LibVclSystemUtils.setWindowtab(vuexStore, storeTypes, windowtab, 0)
      }
      vcl.doSetParam(paramList)
    }
    else {
      win.setVisible(true);
    }
    win.show();
    // 2017/6/30 chenq 报表头查询条件
    if(vcl.afterShow){
      vcl.afterShow(paramList)
    }
  },
  openGrid: function (progId, displayText, paramList) {
    var vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, BillTypeEnum.Grid);
    var view = Ax.utils.LibVclSystemUtils.getView(progId, BillTypeEnum.Grid, vcl);
    vcl.billType = BillTypeEnum.Grid;
    vcl.getTpl();
    var id = progId.replace('.', '_');
    DesktopApp.ActiveWindow = id;
    vcl.winId = id;
    var win = Ext.get(id);
    if (!win) {
      var win = Ext.create("Ext.window.Window", {
        id: id,
        title: displayText,
        autoScroll: true,
        width: document.body.clientWidth * 0.8,
        height: document.body.clientHeight * 0.9,
        vcl: vcl,
        constrainHeader: true,
        minimizable: true,
        maximizable: true,
        items: view[vcl.funcView.get("default").name](),
        listeners: {
          close: function (self, eOpts) {
            this.vcl.vclHandler(self, {libEventType: LibEventTypeEnum.FormClosed});
            if (vuexStore) {
              Ax.utils.LibVclSystemUtils.setWindowtab(vuexStore, storeTypes, win.id, 2)
            }
          },
          beforeclose: function (self, eOpts) {
            this.vcl.vclHandler(self, {libEventType: LibEventTypeEnum.FormClosing});
          },
          minimize: function (self, eOpts) {
            self.setVisible(false);
          }
        }
      });
      vcl.win = win;
      if (vuexStore) {
        var windowtab = {
          id: win.id,
          name: win.title
        }
        Ax.utils.LibVclSystemUtils.setWindowtab(vuexStore, storeTypes, windowtab, 0)
      }
      vcl.doSetParam(paramList);
    }
    else {
      win.component.setVisible(true);
    }
    win.show();
  },
  openDataFunc: function (progId, displayText, paramList) {
    var vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, BillTypeEnum.DataFunc);
    var view = Ax.utils.LibVclSystemUtils.getView(progId, BillTypeEnum.DataFunc, vcl);
    vcl.billType = BillTypeEnum.DataFunc;
    vcl.getTpl();
    var id = progId.replace('.', '_');
    DesktopApp.ActiveWindow = id;
    vcl.winId = id;
    var win = Ext.get(id);
    if (!win) {
      var win = Ext.create("Ext.window.Window", {
        id: id,
        title: displayText,
        autoScroll: true,
        width: document.body.clientWidth * 0.8,
        height: document.body.clientHeight * 0.9,
        vcl: vcl,
        constrainHeader: true,
        minimizable: true,
        maximizable: true,
        items: view[vcl.funcView.get("default").name](),
        listeners: {
          close: function (self, eOpts) {
            this.vcl.vclHandler(self, {libEventType: LibEventTypeEnum.FormClosed});
            if (vuexStore) {
              Ax.utils.LibVclSystemUtils.setWindowtab(vuexStore, storeTypes, win.id, 2)
            }
          },
          beforeclose: function (self, eOpts) {
            this.vcl.vclHandler(self, {libEventType: LibEventTypeEnum.FormClosing});
          },
          minimize: function (self, eOpts) {
            self.setVisible(false);
          }
        }
      });
      vcl.win = win;
      if (vuexStore) {
        var windowtab = {
          id: win.id,
          name: win.title
        }
        Ax.utils.LibVclSystemUtils.setWindowtab(vuexStore, storeTypes, windowtab, 0)
      }
      vcl.doSetParam(paramList);
    }
    win.show();
  },
  openDailyRpt: function (progId, displayText, paramList) {
    var vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, BillTypeEnum.DailyRpt);
    var view = Ax.utils.LibVclSystemUtils.getView(progId, BillTypeEnum.DailyRpt, vcl);
    vcl.billType = BillTypeEnum.DailyRpt;
    vcl.getTpl();
    var id = progId.replace('.', '_');
    DesktopApp.ActiveWindow = id;
    vcl.winId = id;
    var win = Ext.get(id);
    if (!win) {
      var win = Ext.create("Ext.window.Window", {
        id: id,
        title: displayText,
        autoScroll: true,
        width: document.body.clientWidth * 0.8,
        height: document.body.clientHeight * 0.9,
        layout: 'fit',
        vcl: vcl,
        constrainHeader: true,
        minimizable: true,
        maximizable: true,
        items: view[vcl.funcView.get("default").name](displayText),
        listeners: {
          close: function (self, eOpts) {
            this.vcl.vclHandler(self, {libEventType: LibEventTypeEnum.FormClosed});
            if (vuexStore) {
              Ax.utils.LibVclSystemUtils.setWindowtab(vuexStore, storeTypes, win.id, 2)
            }
          },
          beforeclose: function (self, eOpts) {
            this.vcl.vclHandler(self, {libEventType: LibEventTypeEnum.FormClosing});
          },
          minimize: function (self, eOpts) {
            self.setVisible(false);
          }
        }
      });
      vcl.win = win;
      if (vuexStore) {
        var windowtab = {
          id: win.id,
          name: win.title
        }
        Ax.utils.LibVclSystemUtils.setWindowtab(vuexStore, storeTypes, windowtab, 0)
      }
      vcl.doSetParam(paramList);
    }
    win.show();
  },
  openRpt: function (progId, displayText, paramList) {
    var vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, BillTypeEnum.Rpt);
    var view = Ax.utils.LibVclSystemUtils.getView(progId, BillTypeEnum.Rpt, vcl);
    vcl.billType = BillTypeEnum.Rpt;
    vcl.getTpl();
    var id = progId.replace('.', '_');
    DesktopApp.ActiveWindow = id;
    vcl.winId = id;
    var win = Ext.get(id);
    if (!win) {
      var win = Ext.create("Ext.window.Window", {
        id: id,
        title: displayText,
        autoScroll: true,
        width: document.body.clientWidth * 0.8,
        height: document.body.clientHeight * 0.9,
        layout: 'fit',
        vcl: vcl,
        constrainHeader: true,
        minimizable: true,
        maximizable: true,
        items: view[vcl.funcView.get("default").name](displayText),
        listeners: {
          close: function (self, eOpts) {
            this.vcl.vclHandler(self, {libEventType: LibEventTypeEnum.FormClosed});
            if (vuexStore) {
              Ax.utils.LibVclSystemUtils.setWindowtab(vuexStore, storeTypes, win.id, 2)
            }
          },
          beforeclose: function (self, eOpts) {
            this.vcl.vclHandler(self, {libEventType: LibEventTypeEnum.FormClosing});
          },
          minimize: function (self, eOpts) {
            self.setVisible(false)
          }
        }
      });
      vcl.win = win;
      if (vuexStore) {
        var windowtab = {
          id: win.id,
          name: win.title
        }
        Ax.utils.LibVclSystemUtils.setWindowtab(vuexStore, storeTypes, windowtab, 0)
      }
      vcl.doSetParam(paramList);
    }
    win.show();
  },
  openVisual: function (progId, displayText, queryCondition, paramList) {
    if (!DesktopApp.visualHost) {
      Ext.Ajax.request({
        url: 'sysSvc/getVisualHostName',
        method: 'Get',
        async: false,
        timeout: 90000000,
        success: function (response) {
          var ret = Ext.decode(response.responseText);
          DesktopApp.visualHost = ret.GetVisualHostNameResult;
        }
      });
    }
    var vcl = Ax.utils.LibVclSystemUtils.getVcl(progId, BillTypeEnum.Visual);
    var view = Ax.utils.LibVclSystemUtils.getView(progId, BillTypeEnum.Visual, vcl);
    vcl.billType = BillTypeEnum.Visual;
    if (queryCondition != '')
      vcl.queryCondition = Ext.decode(queryCondition);
    vcl.title = displayText;
    var id = progId.replace('.', '_');
    DesktopApp.ActiveWindow = id;
    vcl.winId = id;
    //var desktop = DesktopApp.getDesktop();
    var win = Ext.get(id);
    if (!win) {
      var win = Ext.create("Ext.window.Window", {
        id: id,
        title: displayText,
        autoScroll: true,
        width: document.body.clientWidth * 0.8,
        height: document.body.clientHeight * 0.9,
        layout: {type: 'anchor'},
        vcl: vcl,
        constrainHeader: true,
        minimizable: true,
        maximizable: true,
        items: view.create()
      });
      vcl.doSetParam(paramList);
    }
    vcl.win = win;
    win.show();
  },
  openTree: function (progId, displayText) {
    //var vcl, view;
    //var vclClass = progId.replace('.', '') + 'Vcl';
    //var hasDefineVcl = this.vclPath.hasOwnProperty(vclClass);
    //if (hasDefineVcl) {
    //    Ext.Loader.syncRequire(vclClass);
    //    vcl = new (eval(vclClass))();
    //}
    //else {
    //    vclClass = '';
    //    vcl = new Ax.vcl.LibVclData()
    //}

    //view = new Ax.tpl.LibBillTpl(vcl);
    //vcl.progId = progId;
    //vcl.billType = billType;
    //vcl.vclClass = vclClass;
    //vcl.entryParam = entryParam;
    //vcl.getTpl();
    //var id = progId.replace('.', '_');
    //DesktopApp.ActiveWindow = id;
    //vcl.winId = id;
    //var billPanel = view.create(billAction, curPks);
    //if (!billPanel)
    //    return;
    //var masterRow = vcl.dataSet.getTable(0).data.items[0];
    //var title = Ax.utils.LibVclSystemUtils.getCurrentStateText(masterRow.get('CURRENTSTATE'));
    //if (vcl.tpl.ShowAuditState) {
    //    var auditTxt = Ax.utils.LibVclSystemUtils.getAuditStateText(masterRow.get('AUDITSTATE'), masterRow.get('FLOWLEVEL'));
    //    title = displayText + '【' + title + '】' + '【' + auditTxt + '】';
    //} else
    //    title = displayText + '【' + title + '】';
    //if (vcl.billType == BillTypeEnum.Master) {
    //    if (!masterRow.get('ISVALIDITY'))
    //        title += '【已失效】';
    //}
    //var desktop = DesktopApp.getDesktop();
    //var win = desktop.createWindow({
    //    id: id,
    //    title: title,
    //    progName: displayText,
    //    autoScroll: true,
    //    width: document.body.clientWidth * 0.8,
    //    height: document.body.clientHeight * 0.9,
    //    constrainHeader: true,
    //    minimizable: true,
    //    maximizable: true,
    //    items: billPanel,
    //    vcl: vcl,
    //    listeners: {
    //        close: function (self, eOpts) {
    //            this.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.FormClosed });
    //        },
    //        beforeclose: function (self, eOpts) {
    //            this.vcl.vclHandler(self, { libEventType: LibEventTypeEnum.FormClosing });
    //        }
    //    }
    //});
    //vcl.win = win;
    //win.show();
  },
  getRelSource: function (self, dataInfo, vcl, obj) {
    var dest = self.relSource;
    for (p in dest) {
      //"A.BILLNO=1"
      if (!dest.hasOwnProperty(p))
        continue;
      if (dest[p] == '') {
        obj.hasRealRelSource = true;
        return p;
      }
      else {
        var item = dest[p].split('=');
        if (item.length > 0) {
          var temps = item[0].split('.');
          if (temps.length > 0) {
            var index = temps[0].charCodeAt() - 'A'.charCodeAt();
            if (index == dataInfo.tableIndex) {
              if (dataInfo.dataRow.get(temps[1]) == item[1]) {
                return p;
              }
            }
            else {
              var grid = dataInfo.curGrid;
              if (index != 0 && grid.parentGrid) {
                do {
                  if (grid.parentGrid.tableIndex == index) {
                    if (grid.parentRow.get(temps[1]) == item[1]) {
                      return p;
                    }
                    break;
                  }
                  grid = grid.parentGrid;
                } while (grid);
              } else {
                if (vcl.winId) {
                  var ctrl = Ext.getCmp(temps[1] + index + '_' + vcl.winId);
                  if (ctrl && ctrl.getValue() == item[1]) {
                    return p;
                  }
                }
              }
            }
          }
        }
      }
    }
  },
  newGuid: function () {
    function S4 () {
      return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
    }

    return "Ext_" + (S4() + S4() + S4() + S4() + S4() + S4() + S4() + S4());
  }
}

Ax.utils.DynamicLoading = {
  css: function (path) {
    if (!path || path.length === 0) {
      throw new Error('argument "path" is required !');
    }
    var head = document.getElementsByTagName('head')[0];
    for (var i = 0; i < path.length; i++) {
      var link = document.createElement('link');
      link.href = path[i];
      link.rel = 'stylesheet';
      link.type = 'text/css';
      head.appendChild(link);
    }
  },
  js: function (path) {
    if (!path || path.length === 0) {
      throw new Error('argument "path" is required !');
    }
    var head = document.getElementsByTagName('head')[0];
    for (var i = 0; i < path.length; i++) {
      var script = document.createElement('script');
      script.src = path[i];
      script.type = 'text/javascript';
      head.appendChild(script);
    }
  }
}
