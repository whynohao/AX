
Ext.define("MthPlan.DemoGanttPanel", {
    extend: "Gnt.panel.Gantt",
    alias: 'widget.demogantt',

    requires: [
        'Gnt.plugin.TaskEditor',
        'Gnt.column.StartDate',
        'Gnt.column.EndDate',
        'Gnt.column.Duration',
        'Gnt.column.PercentDone',
        'Gnt.column.ResourceAssignment',
        'Gnt.column.ConstraintType',
        'Gnt.column.ConstraintDate',
        'Sch.plugin.TreeCellEditing',
        'Sch.plugin.Pan',
        'MthPlan.FilterField',
        'MthPlan.Toolbar'
    ],
    highlightWeekends: true,
    showTodayLine: true,
    loadMask: true,
    enableProgressBarResize: true,
    enableDependencyDragDrop: false,
    skipWeekendsDuringDragDrop: false,
    eventBorderWidth: 0,
    rowHeight: 28,
    leftLabelField: 'Name',
    initComponent: function () {
        var gantt = this;
        Ext.apply(this, {
            // Define a custom HTML template for regular tasks
            //taskBodyTemplate : '<div class="sch-gantt-progress-bar" style="width:{progressBarWidth}px;{progressBarStyle}" unselectable="on"><span class="sch-gantt-progress-bar-label">{[Math.round(values.percentDone)]}%<span></span></div>',
            // Define properties for the left 'locked' and scrollable tree grid
            lockedGridConfig: {
                width: 500
            },

            leadingBufferZone: 1, // HACK: temp fix for Ext JS 5 buffered renderer issue
            // Define properties for the schedule section
            schedulerConfig: {
            },

            // Add some extra functionality
            plugins: [
                this.editingInterface = Ext.create('Sch.plugin.TreeCellEditing', { clicksToEdit: 2 }),
                Ext.create("Sch.plugin.Pan")
            ],

            // Define an HTML template for the tooltip
            tooltipTpl: new Ext.XTemplate(
                '<strong class="tipHeader">{Name}</strong>',
                '<table cellpadding="0" cellspacing="0" class="taskTip">',
                    '<tr><td>Start:</td> <td align="right">{[values._record.getDisplayStartDate("y-m-d")]}</td></tr>',
                    '<tr><td>End:</td> <td align="right">{[values._record.getDisplayEndDate("y-m-d")]}</td></tr>',
                    '<tr><td>Progress:</td><td align="right">{[ Math.round(values.PercentDone)]}%</td></tr>',
                '</table>'
            ),

            eventRenderer: function (task, tplData) {
                if (task.get('Color')) {
                    var style = Ext.String.format('background-color: #{0};border-color:#{0}', task.get('Color'));

                    if (!tplData.segments) {
                        return {
                            // Here you can add custom per-task styles
                            style: style
                        };

                        // if task is segmented we cannot use above code
                        // since it will set color of background visible between segments
                        // in this case instead we need to provide "style" for each individual segment
                    } else {
                        var segments = tplData.segments;
                        for (var i = 0; i < segments.length; i++) {
                            segments[i].style = style;
                        }
                    }
                }
            },

            // Define the static columns
            columns: this.columns || [
                // Any regular Ext JS columns are ok
                {
                    xtype: 'rownumberer',
                    width: 30,

                    // This CSS class is added to each cell of this column
                    tdCls: 'id'
                },
                {
                    xtype: 'namecolumn',
                    width: 200,
                    renderer: function (v, meta, r) {
                        if (!r.data.leaf) meta.tdCls = 'sch-gantt-parent-cell';
                        return Ext.util.Format.htmlEncode(v);
                    },
                    items: new MthPlan.FilterField({
                        store: this.taskStore,
                        sortable: false
                    }),
                    editor: {
                        xtype: "textfield",
                        readOnly: true
                    }
                },
                {
                    xtype: 'libGanttDatecolumn',
                    dataIndex: "StartDate",
                    text: "开始时间",
                    width: 140,
                    axT: 0,
                    editor: {
                        xtype: 'libGanttDatetimefield',
                        callback: function (v) {
                            gantt.calcTime(v, true)
                        }
                    }
                },
                {
                    xtype: 'libGanttDatecolumn',
                    dataIndex: "EndDate",
                    text: "结束时间",
                    width: 140,
                    axT: 0,
                    editor: {
                        xtype: 'libGanttDatetimefield',
                        callback: function (v) {
                            gantt.calcTime(v, false)
                        }
                    }
                }
            ],
            dockedItems: [{ xtype: 'primarytoolbar', gantt: this }]
        });
        this.callParent(arguments);
    },
    listeners: {
        beforetaskdrag: function (gantt, taskRecord, e, eOpts) {
            if (!this.vcl.isEdit) {
                return false;
            }
        },
        beforetaskresize: function (gantt, taskRecord, e, eOpts) {
            if (!this.vcl.isEdit) {
                return false;
            }
        },
        taskdrop: function (gantt, taskRecord, eOpts) {
            this.setSelection(taskRecord);
            if (this.vcl.isEdit) {
                var v = taskRecord.get('StartDate');
                this.calcTime(v, true);
            }
        },
        aftertaskresize: function (gantt, taskRecord, isStart, eOpts) {
            this.setSelection(taskRecord);
            if (this.vcl.isEdit) {
                var v = isStart ? taskRecord.get('StartDate') : taskRecord.get('EndDate');
                this.calcTime(v, isStart);
            }
        }
    }
});
