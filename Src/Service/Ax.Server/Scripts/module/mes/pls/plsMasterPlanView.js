

plsMasterPlanView = function () {
    Ax.tpl.LibBillTpl.apply(this, arguments);
    this.vcl.funcView.add('createGantt', { name: 'createGantt', display: '甘特图' });
    Ax.utils.DynamicLoading.css(['Scripts/module/mes/pls/schedule/resources/css/schedule.css']);
};
var proto = plsMasterPlanView.prototype = Object.create(Ax.tpl.LibBillTpl.prototype);
proto.constructor = plsMasterPlanView;

proto.createGantt = function () {
    var me = this;
    var vcl = this.vcl;
    var mainWidth = document.body.clientWidth > 1210 ? document.body.clientWidth - 10 : 1210;
    var mainPanel = Ext.create('Ext.panel.Panel', {
        width: mainWidth,
        height: document.body.clientHeight - 80,
        layout: { type: 'fit' },
        items: me.Gantt.createGantt(vcl),
        border: false,
        tbar: Ax.utils.LibToolBarBuilder.createToolBar([vcl.createChangeView(me, 'createGantt', 'createGantt', '甘特图')])
    });
    return mainPanel;
};

Ext.Loader.setPath('MthPlan', 'Scripts/module/mes/pls/schedule/js');

proto.Gantt = {
    createGantt: function (vcl) {
        var typeId = vcl.dataSet.getTable(0).data.items[0].get('TYPEID');
        var ganttData = vcl.invorkBcf('GetGanntDataStructure', [typeId]);
        var planStruct = Ext.decode(ganttData.Task);
        var dependencyStruct = ganttData.Dependencies;
        var table = vcl.dataSet.getTable(1);
        var taskData = [];
        var dependencyData = [];
        if (!vcl.fieldMap)
            vcl.fieldMap = {};
        function formatDate(v) {
            v = v.toString();
            var d = new Date(v.substr(0, 4) + '-' + v.substr(4, 2) + '-' + v.substr(6, 2));
            //d.setHours(v.substr(8, 2));
            //d.setMinutes(v.substr(10, 2));
            //d.setSeconds(v.substr(12, 2));
            return d;
        }
        var count = 0;
        table.each(function (rec) {
            var planData = Ext.clone(planStruct);
            var rowList = new Ext.util.MixedCollection();
            rowList.add(1, rec);
            var id = rec.get('ROW_ID');
            for (var i = 0; i < dependencyStruct.length; i++) {
                dependencyData.push({
                    Id: ++count,
                    Type: 2,
                    From: dependencyStruct[i]['From'] + '_' + id.toString(),
                    To: dependencyStruct[i]['To'] + '_' + id.toString()
                });
            }
            function fillVal(node) {
                var planId = node["Id"];
                node["Id"] = planId + '_' + id.toString();
                if (!vcl.fieldMap[planId]) {
                    for (var i = 1; i < vcl.dataSet.dataList.length; i++) {
                        var val = vcl.dataSet.getTable(i).getModel().getField(planId);
                        if (val != null) {
                            vcl.fieldMap[planId] = i;
                            break;
                        }
                    }
                }
                if (vcl.fieldMap[planId]) {
                    var idx = vcl.fieldMap[planId];
                    var startName = planId + 'START', endName = planId;
                    node['SchedulingMode'] = 'Manual';
                    if (idx == 1) {
                        node['StartDate'] = formatDate(rec.get(startName));
                        node['EndDate'] = formatDate(rec.get(endName));
                    } else {
                        var subTable = vcl.dataSet.getTable(idx);
                        var parentIdx = subTable['ParentIndex'];
                        var parentRec = null;
                        if (parentIdx == 1) {
                            parentRec = rec;
                        } else {
                            if (!rowList.containsKey(parentIdx)) {
                                var idxArray = [parentIdx];
                                var tempIdx = parentIdx;
                                while (tempIdx != 1) {
                                    tempIdx = vcl.dataSet.getTable(tempIdx)['ParentIndex'];
                                    idxArray.push(tempIdx);
                                }
                                for (var i = idxArray.length - 2; i >= 0; i--) {
                                    var curIdx = idxArray[i];
                                    var preIdx = curIdx[i + 1];
                                    rowList.add(curIdx, vcl.dateSet.getChildren(preIdx, rowList.get(preIdx), curIdx)[0]);
                                }
                            }
                            parentRec = rowList[parentIdx];
                        }
                        var curRec = vcl.dateSet.getChildren(parentIdx, parentRec, idx)[0];
                        node['StartDate'] = formatDate(curRec.get(startName));
                        node['EndDate'] = formatDate(curRec.get(endName));
                    }
                }
                if (node.hasOwnProperty('children')) {
                    var minStart = null, maxEnd = null;
                    for (var i = 0; i < node['children'].length; i++) {
                        var tempDate = fillVal(node['children'][i]);
                        var start = tempDate[0], end = tempDate[1];
                        if (minStart == null)
                            minStart = start;
                        else if (minStart > start)
                            minStart = start;
                        if (maxEnd == null)
                            maxEnd = end;
                        else if (maxEnd < end)
                            maxEnd = end;
                    }
                    node['StartDate'] = minStart;
                    node['EndDate'] = maxEnd;
                }
                return [node['StartDate'], node['EndDate']];
            }
            planData["Id"] = id;
            planData["Name"] = rec.get('WORKNO');
            if (planData.hasOwnProperty('children')) {
                var minStart = null, maxEnd = null;
                for (var i = 0; i < planData['children'].length; i++) {
                    var tempDate = fillVal(planData['children'][i]);
                    var start = tempDate[0], end = tempDate[1];
                    if (minStart == null)
                        minStart = start;
                    else if (minStart > start)
                        minStart = start;
                    if (maxEnd == null)
                        maxEnd = end;
                    else if (maxEnd < end)
                        maxEnd = end;
                }
                planData['StartDate'] = minStart;
                planData['EndDate'] = maxEnd;
            }
            taskData.push(planData);
        });
        var modelName = 'MthPlanTaskModel';
        var modelType = Ext.data.Model.schema.getEntity(modelName);
        if (modelType === null) {
            modelType = Ext.define(modelName, {
                extend: 'Gnt.model.Task'
            });
        }

        var taskStore = Ext.create("Gnt.data.TaskStore", {
            model: modelName,
            calendar: new Gnt.data.Calendar({
                name: 'General',
                calendarId: 'General',
                weekendsAreWorkdays: true
            }),
            data: taskData,
            proxy: 'memory',
            cascadeChanges: true,
            recalculateParents: true,
            moveParentAsGroup: true
        });
        var dependencyStore = Ext.create("Gnt.data.DependencyStore", {
            data: dependencyData,
            proxy: 'memory'
        });
        var gantt = Ext.create("MthPlan.DemoGanttPanel", {
            allowParentTaskMove: true,
            vcl: vcl,
            region: 'center',
            rowHeight: Ext.supports.Touch ? 43 : 28,
            selModel: new Ext.selection.TreeModel({
                ignoreRightMouseSelection: false,
                mode: 'MULTI'
            }),
            taskStore: taskStore,
            dependencyStore: dependencyStore,
            bufferedRenderer: true,
            columnLines: false,
            showTodayLine: true,
            weekendsAreWorkdays: true,
            viewPreset: 'weekAndDay',
            calcTime: function (v, isStart) {
                function toLibDateTime(v) {
                    return (v.getFullYear() * 10000 + (v.getMonth() + 1) * 100 + v.getDate());
                }
                v = toLibDateTime(v);
                var masterRow = vcl.dataSet.getTable(0).data.items[0];
                var nodeRec = gantt.getSelection()[0];
                var id = nodeRec.get('Id');
                var idx = id.lastIndexOf('_');
                var planName = id.substr(0, idx);
                var name = isStart ? planName + 'START' : planName;
                var rowId = id.substr(idx + 1, id.length - 1);
                var bodyRow = vcl.dataSet.FindRow(1, Number.parseInt(rowId));
                function formatDate(v) {
                    v = v.toString();
                    var d = new Date(v.substr(0, 4) + '-' + v.substr(4, 2) + '-' + v.substr(6, 2));
                    d.setHours(v.substr(8, 2));
                    d.setMinutes(v.substr(10, 2));
                    d.setSeconds(v.substr(12, 2));
                    return d;
                }
                if (bodyRow.get('ISAUTOCALC') === true) {
                    var ret = vcl.calcTime(masterRow, bodyRow, name, v);
                    var taskStore = gantt.getStore();
                    taskStore.each(function (rec) {
                        var temp = rec.get('Id');
                        if (typeof temp === 'string') {
                            var index = temp.lastIndexOf('_');
                            var tempName = temp.substr(0, index);
                            var tempRowId = temp.substr(index + 1, temp.length - 1);
                            if (tempRowId == rowId) {
                                if (ret[tempName]) {
                                    var startName = tempName + 'START', endName = tempName;
                                    rec.set('StartDate', formatDate(ret[startName]));
                                    rec.set('EndDate', formatDate(ret[endName]));
                                }
                            }
                        }
                    });
                } else {
                    if (bodyRow.data[name] !== undefined) {
                        bodyRow.set(name, v);
                        if (isStart)
                            bodyRow.set(planName, toLibDateTime(nodeRec.get('EndDate')));
                        else
                            bodyRow.set(planName + 'START', toLibDateTime(nodeRec.get('StartDate')));
                    } else {
                        var subStores = [];
                        for (var i = 2; i < vcl.dataSet.dataList.length; i++) {
                            var table = vcl.dataSet.dataList[i];
                            if (table.Name.indexOf('DYTABLE') == 0) {
                                var parentIndex = table['ParentIndex'];
                                var model = vcl.dataSet.getChildren(parentIndex, bodyRow, i);
                                if (model && model.length > 0) {
                                    subStores.push(model[0]);
                                }
                            }
                        }
                        for (var r = 0; r < subStores.length; r++) {
                            var dest = subStores[r];
                            if (dest.data[name] !== undefined) {
                                dest.set(name, v);
                                if (isStart)
                                    dest.set(planName, toLibDateTime(nodeRec.get('EndDate')));
                                else
                                    dest.set(planName + 'START', toLibDateTime(nodeRec.get('StartDate')));
                                break;
                            }
                        }
                    }
                }
            }
        });
        return gantt;
    }
};




