Ext.define("MthPlan.Toolbar", {
    extend : "Ext.Toolbar",
    cls    : 'my-toolbar',
    alias  : 'widget.primarytoolbar',

    gantt : null,

    initComponent : function () {
        var gantt = this.gantt;
        var highlightTaskList = [];
        var taskStore = gantt.taskStore || gantt.crudManager && gantt.crudManager.getTaskStore();

        taskStore.on({
            'filter-set'   : function () {
                this.down('[iconCls*=icon-collapseall]').disable();
                this.down('[iconCls*=icon-expandall]').disable();
            },
            'filter-clear' : function () {
                this.down('[iconCls*=icon-collapseall]').enable();
                this.down('[iconCls*=icon-expandall]').enable();
            },
            scope          : this
        });

        var items = [
            {
                tooltip : '上一个时间跨度',
                iconCls : 'icon icon-left',
                handler : function () {
                    gantt.shiftPrevious();
                }
            },
            {
                tooltip : '下一个时间跨度',
                iconCls : 'icon icon-right',
                handler : function () {
                    gantt.shiftNext();
                }
            },
            {
                tooltip : '折叠',
                iconCls : 'icon icon-collapseall',
                handler : function () {
                    gantt.collapseAll();
                }
            },
            {
                tooltip : '展开',
                iconCls : 'icon icon-expandall',
                handler : function () {
                    gantt.expandAll();
                }
            },
            {
                tooltip : '缩小',
                iconCls : 'icon icon-zoomout',
                handler : function () {
                    gantt.zoomOut();
                }
            },
            {
                tooltip : '放大',
                iconCls : 'icon icon-zoomin',
                handler : function () {
                    gantt.zoomIn();
                }
            },
            {
                tooltip : '合适比例',
                iconCls : 'icon icon-zoomfit',
                handler : function () {
                    gantt.zoomToFit(null, { leftMargin : 100, rightMargin : 100 });
                }
            },
            {
                tooltip      : '突出依赖该项的路径',
                iconCls      : 'icon icon-criticalpath',
                enableToggle : true,
                handler      : function (btn) {
                    var v = gantt.getSchedulingView();
                    gantt.getSelectionModel().selected.each(function (task) {
             		if (!Ext.Array.contains(highlightTaskList,task.id)) {
             			highlightTaskList.push(task.id);
                        v.highlightTask(task,true);
                    } else {
                     	highlightTaskList.pop(task.id);
                        v.unhighlightTask(task,true);
                    }
      			  });
                }
            }
        ];

        Ext.apply(this, {
            defaults : { scale : 'medium' },

            items : items
        });

        this.callParent(arguments);
    },

    applyPercentDone : function (value) {
        this.gantt.getSelectionModel().selected.each(function (task) {
            task.setPercentDone(value);
        });
    },

    showFullScreen : function () {
        this.gantt.el.down('.x-panel-body').dom[this._fullScreenFn](Element.ALLOW_KEYBOARD_INPUT);
    },

    // Experimental, not X-browser
    _fullScreenFn  : (function () {
        var docElm = document.documentElement;

        if (docElm.requestFullscreen) {
            return "requestFullscreen";
        }
        else if (docElm.mozRequestFullScreen) {
            return "mozRequestFullScreen";
        }
        else if (docElm.webkitRequestFullScreen) {
            return "webkitRequestFullScreen";
        }
        else if (docElm.msRequestFullscreen) {
            return "msRequestFullscreen";
        }
    })()
});
