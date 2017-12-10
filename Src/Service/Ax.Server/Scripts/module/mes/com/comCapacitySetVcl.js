comCapacitySetVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = comCapacitySetVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comCapacitySetVcl;


proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    var bodyTable = this.dataSet.getTable(1);
    var masterRow = this.dataSet.getTable(0).data.items[0];
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            if (e.dataInfo.tableIndex == 1) {
                if (masterRow.data["PRODUCELINEID"] === "") {
                    Ext.Msg.alert("提示", '请维护生产线！');
                    e.dataInfo.cancel = true;
                }
            }
            break;

        case LibEventTypeEnum.AddRow:
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.dataRow.set("PRODUCELINENAME", masterRow.data["PRODUCELINENAME"]);
                e.dataInfo.dataRow.set("CAPACITY", masterRow.data["CAPACITY"]);
            }
            break;

        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                switch (e.dataInfo.fieldName) {
                    case "DAY":
                        if (e.dataInfo.value == 0) {
                            e.dataInfo.dataRow.set("CAPACITY", masterRow.data["CAPACITY"]);
                            e.dataInfo.dataRow.set("REALCAPACITY", 0);
                            return;
                        }

                        var rowId = e.dataInfo.dataRow.data["ROW_ID"];
                        for (var i = 0; i < bodyTable.data.items.length; i++) {
                            if (bodyTable.data.items[i].data["ROW_ID"] != rowId && bodyTable.data.items[i].data["DAY"] == e.dataInfo.value && bodyTable.data.items[i].data["DAY"] != '') {
                                Ext.Msg.alert("提示", '日期不能相同！');
                                e.dataInfo.cancel = true;
                                return;
                            }
                        }

                        var result = this.invorkBcf("GetOneData", [masterRow.data["PRODUCELINEID"], e.dataInfo.value]);
                        if (result == null) {
                            e.dataInfo.dataRow.set("CAPACITY", masterRow.data["CAPACITY"]);
                            e.dataInfo.dataRow.set("REALCAPACITY", 0);
                            break;
                        }
                        e.dataInfo.dataRow.set("CAPACITY", result.Capacity);
                        e.dataInfo.dataRow.set("REALCAPACITY", result.RealCapacity);
                        break;
                    case "CAPACITY":
                        if (e.dataInfo.dataRow.data["REALCAPACITY"] > 0) {
                            Ext.Msg.alert("提示", "不能修改已排产的生产线产能！");
                            e.dataInfo.cancel = true;
                        }
                        break;
                }

            }

            if (e.dataInfo.tableIndex == 0) {
                switch (e.dataInfo.fieldName) {
                    case "PRODUCELINEID":
                        if (e.dataInfo.value === "") {
                            Ext.Msg.alert("提示", '生产线不能为空！');
                            e.dataInfo.cancel = true;
                            masterRow.data["PRODUCELINEID"] = e.dataInfo.oldValue;
                            break;

                        }
                        var dayList = [];
                        for (var i = 0; i < bodyTable.data.items.length; i++) {
                            bodyTable.data.items[i].set("PRODUCELINENAME", masterRow.data["PRODUCELINENAME"]);
                            var day = bodyTable.data.items[i].data["DAY"];
                            if (day != 0) {
                                dayList.push(day);
                            }
                        }

                        if (dayList.length > 0) {
                            //级联查询生产线的默认产能，产能和实际产能，返回值是为以日期值为键的字典数据
                            var dic = this.invorkBcf("GetSomeData", [e.dataInfo.value, dayList]);

                            var defaultCapacity = dic["Default"].DefaultCapacity;

                            // 遍历子表所有行
                            for (i = 0; i < bodyTable.data.items.length; i++) {
                                bodyTable.data.items[i].set("DEFAULTCAPACITY", defaultCapacity);

                                day = bodyTable.data.items[i].data["DAY"];
                                if (day == 0) {
                                    continue;
                                }
                                var item = dic["" + day];

                                // 没有该天对应的产能数据
                                if (item === undefined) {
                                    bodyTable.data.items[i].set("CAPACITY", masterRow.data["CAPACITY"]);
                                    bodyTable.data.items[i].set("REALCAPACITY", 0);
                                } else {
                                    bodyTable.data.items[i].set("CAPACITY", item.Capacity);
                                    bodyTable.data.items[i].set("REALCAPACITY", item.RealCapacity);
                                }
                            }
                        }
                        break;

                    case "CAPACITY":
                        if (e.dataInfo.value < 0) {
                            Ext.Msg.alert("提示", "产能不能小于0！");
                            e.dataInfo.cancel = true;
                            break;
                        }
                        for (var i = 0; i < bodyTable.data.items.length; i++) {
                            if (bodyTable.data.items[i].data["REALCAPACITY"] == 0) {
                                bodyTable.data.items[i].set("CAPACITY", e.dataInfo.value);
                            }
                        }
                        break;
                }
            }

            break;


        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
            }

            break;
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "BTNSELECT":
                    var producelineId = this.dataSet.getTable(0).data.items[0].data['PRODUCELINEID'];
                    var startDate = this.dataSet.getTable(0).data.items[0].data['STARTDATE'];
                    var endDate = this.dataSet.getTable(0).data.items[0].data['ENDDATE'];
                    if (producelineId == '') {
                        Ext.Msg.alert("提示", '请维护生产线！');
                        break;
                    }
                    if (startDate == '') {
                        Ext.Msg.alert("提示", '请维护开始日期！');
                        break;
                    }
                    if (startDate > endDate && endDate > 0) {
                        Ext.Msg.alert("提示", '开始日期必须小于结束日期！');
                        break;
                    }
                    var capacity = this.dataSet.getTable(0).data.items[0].data['CAPACITY'];
                    var returnData = this.invorkBcf("GetData", [producelineId, startDate, endDate]);

                    fillCapacityData.call(this, returnData);
                    break;

                case "BTNRESET":
                    this.deleteAll(1)
                    break;

                case "BTNSAVE":
                    var list = [];
                    if (masterRow.data["PRODUCELINEID"] == '') {
                        Ext.Msg.alert("提示", '请维护生产线！');
                        break;
                    }
                    var AllYes = true;
                    var startDate = this.dataSet.getTable(0).data.items[0].data['STARTDATE'];
                    var endDate = this.dataSet.getTable(0).data.items[0].data['ENDDATE'];
                    if (startDate > endDate && endDate > 0) {
                        Ext.Msg.alert("提示", '开始日期必须小于结束日期！');
                        break;
                    }
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        if (bodyTable.data.items[i].data["DAY"] != 0) {
                            list.push({
                                ProduceLineId: masterRow.data["PRODUCELINEID"],
                                Day: bodyTable.data.items[i].data["DAY"],
                                Capacity: bodyTable.data.items[i].data["CAPACITY"],
                                RealCapacity: bodyTable.data.items[i].data["REALCAPACITY"]
                            });

                        }
                        else {
                            Ext.Msg.alert("提示", '请维护所有行项日期！');
                            AllYes = false;
                            break;
                        }
                    }
                    if (AllYes) {
                        if (list.length > 0) {
                            if (this.invorkBcf("SaveData", [list, masterRow.data["PRODUCELINEID"]])) {
                                Ext.Msg.alert("提示", '保存成功！');
                            }
                            else {
                                Ext.Msg.alert("提示", '保存失败！');
                            }
                        }
                        else {
                            Ext.Msg.alert("提示", '没有要更新的数据！');
                        }
                    }


                    break;
            }
            break;
    }
}

function fillCapacityData(returnData) {
    Ext.suspendLayouts();
    var formStore = this.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        this.deleteAll(1);
        var masterRow = this.dataSet.getTable(0).data.items[0];
        var list = returnData;
        if (list !== undefined && list.length > 0) {
            var produceLineId = masterRow.data["PRODUCELINEID"];
            var produceLineName = masterRow.data["PRODUCELINENAME"];
            var capacity = masterRow.data["CAPACITY"];
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set('PRODUCELINEID', produceLineId);
                newRow.set('PRODUCELINENAME', produceLineName);
                newRow.set('DAY', info.Day);
                newRow.set('DEFAULTCAPACITY', info.DefaultCapacity);

                if (info.Capacity == 0) {
                    newRow.set('CAPACITY', capacity);
                } else {
                    newRow.set('CAPACITY', info.Capacity);
                }

                newRow.set('REALCAPACITY', info.RealCapacity);
            }
        }
    }
    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}

