EmMaintainVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = EmMaintainVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = EmMaintainVcl;

proto.vclHandler = function (sender, e) {
    me = this;
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (!this.isEdit) {
                Ext.Msg.alert("系统提示", "非编辑状态下不可操作！");
            }
            else {
                var fieldName = e.dataInfo.fieldName;
                //加载作业数据事件
                if (fieldName == "LoadTaskData") {
                    var masterRow = this.dataSet.getTable(0).data.items[0];
                    var equipmentid = masterRow.data["EQUIPMENTID"];
                    if (Ext.isEmpty(equipmentid)) {
                        Ext.Msg.alert("系统提示", "请选择设备");
                    } else {
                        //填写弹出DataFunc代码
                        Ax.utils.LibVclSystemUtils.openDataFunc("Em.MaintainDataFunc", "维修作业", [this]);
                    }
                }
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                var hasquantity = e.dataInfo.dataRow.data["HASQUANTITY"]
                if (hasquantity > 0) {
                    Ext.Msg.alert("系统提示", "备品备件已出库，不能删除");
                    e.dataInfo.cancel = true;
                }
            } else if (e.dataInfo.tableIndex == 0) {
                e.dataInfo.cancel = true;
            }
            break;
        case LibEventTypeEnum.Validated:
            var fieldName = e.dataInfo.fieldName;
            if (e.dataInfo.tableIndex == 0) {
                this.forms[0].updateRecord(e.dataInfo.dataRow);
                var newthis = this;

                if (fieldName == "EQUIPMENTID") {
                    if (e.dataInfo.value != e.dataInfo.oldValue) {
                        var hasquantity = 0;
                        var store = this.dataSet.getTable(1);
                        var dtLength = store.data.items.length;

                        if (Ext.isEmpty(e.dataInfo.oldValue)) {
                            //备品配件填充代码
                            var equipmentid = e.dataInfo.value;
                            var returnList = this.invorkBcf("GetEmMaintainDetail", [equipmentid]);
                            FillEmMaintainDetailData.call(this, returnList);
                        } else {
                            for (var i = 0; i < dtLength; i++) {
                                hasquantity += store.data.items[i].data["HASQUANTITY"];
                            }
                            if (hasquantity > 0) {
                                Ext.Msg.alert("系统提示", "备品备件已经出库，不允许修改");
                                Ext.getCmp('EQUIPMENTID0_' + this.winId).setValue(e.dataInfo.oldValue);
                            } else {
                                Ext.Msg.confirm('系统提示', '更改设备会同时更改备品备件、领取备件、退回备件明细信息，确认更改吗？', function (button) {
                                    if (button == "yes") {
                                        if (e.dataInfo.value == "") {
                                            e.dataInfo.dataRow.set("EQUIPMENTNAME", "");
                                            e.dataInfo.dataRow.set("EQUIPMENTMODEL", "");
                                        }
                                        e.dataInfo.dataRow.set("FROMBILLNO", "");
                                        e.dataInfo.dataRow.set("FROMROWID", 0);
                                        e.dataInfo.dataRow.set("EMFAULTID", "");
                                        e.dataInfo.dataRow.set("TASKID", "");
                                        e.dataInfo.dataRow.set("ISANALYSIS", false);
                                        newthis.forms[0].loadRecord(newthis.dataSet.getTable(0).data.items[0]);

                                        if (!Ext.isEmpty(e.dataInfo.value)) {
                                            //备品配件填充代码
                                            var equipmentid = e.dataInfo.value;
                                            var returnList = newthis.invorkBcf("GetEmMaintainDetail", [equipmentid]);
                                            FillEmMaintainDetailData.call(newthis, returnList);
                                        } else {
                                            newthis.dataSet.getTable(1).removeAll();//清空备品备件子表
                                            newthis.dataSet.getTable(2).removeAll();//清空领取备件子表
                                            newthis.dataSet.getTable(3).removeAll();//清空退回备件子表
                                        }
                                    }
                                    else if (button == "no") {
                                        Ext.getCmp('EQUIPMENTID0_' + newthis.winId).setValue(e.dataInfo.oldValue);
                                        newthis.forms[0].updateRecord(newthis.dataSet.getTable(0).data.items[0]);
                                    }
                                });
                            }
                        }
                    }
                }
                else if (fieldName == "PERSONGROUPID") {
                    if (e.dataInfo.value != e.dataInfo.oldValue) {
                        e.dataInfo.dataRow.set("PERSONID", "");
                        newthis.forms[0].loadRecord(newthis.dataSet.getTable(0).data.items[0]);
                    }
                }
                else if (fieldName == "EMFAULTID") {
                    if (e.dataInfo.value != e.dataInfo.oldValue) {
                        //手动更改故障，清空
                        e.dataInfo.dataRow.set("TASKID", "");
                        e.dataInfo.dataRow.set("TASKNAME", "");
                        e.dataInfo.dataRow.set("FROMBILLNO", "");
                        e.dataInfo.dataRow.set("FROMROWID", 0);
                        if (Ext.isEmpty(e.dataInfo.value)) {
                            //在发生更改且当前值为空的情况下清空分析原因
                            e.dataInfo.dataRow.set("ISANALYSIS", false);
                        }
                        newthis.forms[0].loadRecord(newthis.dataSet.getTable(0).data.items[0]);
                    }
                }
            }
            break;
        case LibEventTypeEnum.Validating:
            var fieldName = e.dataInfo.fieldName;
            if (e.dataInfo.tableIndex == 1) {
                if (fieldName == "QUANTITY") {
                    var quantity = e.dataInfo.value;
                    var hasquantity = e.dataInfo.dataRow.get("HASQUANTITY");
                    if (quantity < hasquantity) {
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("系统提示", "数量要大于已领数量");
                    }
                }
                else if (fieldName == "MATERIALID") {
                    var hasquantity = e.dataInfo.dataRow.get("HASQUANTITY");
                    if (hasquantity > 0) {
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("系统提示", "备品备件已经出库，不允许修改");
                    }
                }
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1)
                e.dataInfo.cancel = true;
            break;
    }
}

//保存后刷新
proto.doSave = function () {
    var assistObj = {};
    var data = this.save(this.billAction, this.currentPk, assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success) {
        this.restData(false, BillActionEnum.Browse, data);
        var obj = [];
        obj.push(this.dataSet.getTable(0).data.items[0].data["BILLNO"]);
        this.browseTo(obj);
    }
    return success;
}
//生效取消刷新
proto.doTakeRelease = function (cancel) {
    if (cancel)
        this.billAction = BillActionEnum.Release;
    else
        this.billAction = BillActionEnum.CancelRelease;
    this.setExtendParam();
    var assistObj = {};
    var data = this.invorkBcf("TakeRelease", [this.currentPk, cancel, undefined, this.extendParam], assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success) {
        this.restData(false, BillActionEnum.Browse, data);
        var obj = [];
        obj.push(this.dataSet.getTable(0).data.items[0].data["BILLNO"]);
        this.browseTo(obj);
    }
    return success;
};
//刷新
proto.browseTo = function (condition) {
    var data = this.invorkBcf("BrowseTo", [condition]);
    this.setDataSet(data, false);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    for (var i = 0; i < this.forms.length; i++) {
        this.forms[i].loadRecord(masterRow);
    };
};

function FillEmMaintainDetailData(returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var list = returnList;//一般是中间层返回来的数据，中间可能定义的是dictionary,在前段反序列化之后是对象
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set("ROW_ID", i + 1);
                newRow.set("ROWNO", i + 1);
                newRow.set("MATERIALID", info.MaterialId);
                newRow.set("MATERIALNAME", info.MaterialName);
                newRow.set("MATERIALSPEC", info.MaterialSpec);
                newRow.set("UNITID", info.UnitId);
                newRow.set("UNITNAME", info.UnitName);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}
