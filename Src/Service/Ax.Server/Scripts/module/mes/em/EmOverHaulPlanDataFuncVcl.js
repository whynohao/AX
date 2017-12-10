EmOverHaulPlanDataFuncVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = EmOverHaulPlanDataFuncVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = EmOverHaulPlanDataFuncVcl;
proto.winId = null;
proto.fromObj = null;
proto.getType = 0;
proto.doSetParam = function (vclObj) {
    proto.winId = vclObj[0].winId;
    proto.fromObj = vclObj[0];
};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    //不允许手工添加行
    if (e.libEventType == LibEventTypeEnum.BeforeAddRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }
        //不允许手工删除行
    else if (e.libEventType == LibEventTypeEnum.BeforeDeleteRow) {
        if (e.dataInfo.tableIndex == 1) {
            e.dataInfo.cancel = true;
        }
    }

    else if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo.tableIndex == 0) {
            if (e.dataInfo.fieldName == "BILLNO") {
                var formBill = this.dataSet.getTable(0).data.items[0];//找到表头的数据
                //this.forms[0].updateRecord(this.dataSet.getTable(0).data.items[0]);
                formBill.set("PLANYEAR", "");
                formBill.set("PLANMONTH", "");
                formBill.set("DEPTID", "");
                this.forms[0].loadRecord(formBill);
                this.deleteAll(1);
                var billNo = this.dataSet.getTable(0).data.items[0].data['BILLNO'];
                ////用单号 去查询 调用cs 端的GetData 方法 返回一个list 
                ////returnData 是返回值，一般中间层返回一个dictionary，然后这边前端反序列化之后得到一个对象
                if (billNo != "") {
                    var returnMainData = this.invorkBcf("GetMainData", [billNo]);
                    var list = returnMainData['main'];
                    var formBill = this.dataSet.getTable(0).data.items[0];//找到表头的数据
                    formBill.set("PLANYEAR", list[0].PLANYEAR);
                    formBill.set("PLANMONTH", list[0].PLANMONTH);
                    formBill.set("DEPTID", list[0].DEPTID);
                    this.forms[0].loadRecord(formBill);
                    var returnData = this.invorkBcf("GetData", [billNo]);
                    fillData.call(this, returnData);
                }
            }
        }
    }

    else if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        //全选按钮事件
        if (e.dataInfo.fieldName == "btnCheckAll") {
            var allItems = this.dataSet.getTable(1).data.items;
            for (var i = 0; i < allItems.length; i++) {
                allItems[i].set("ISCHOSE", "1");
            }
        }

            //取消按钮
        else if (e.dataInfo.fieldName == "btnCheckCancel") {
            var allItems = this.dataSet.getTable(1).data.items;
            for (var i = 0; i < allItems.length; i++) {
                allItems[i].set("ISCHOSE", "0");
            }
        }

        else if (e.dataInfo.fieldName == "btnCheck") {
            var grid = Ext.getCmp(this.winId + 'EMOVERHAULPLANDETAILFUNCGrid');//通过ID找到对应的Grid
            var selectItems = this.dataSet.getTable(1).data.items;
            var checkItems = [];
            for (var i = 0; i < selectItems.length; i++) {
                if (selectItems[i].data["ISCHOSE"] == true) {
                    //获取打勾选中行,组成新的数组
                    checkItems.push({
                        BILLNO: selectItems[i].data["BILLNO"],
                        FROMROWID: selectItems[i].data["FROMROWID"],
                        ROWTYPE: selectItems[i].data["ROWTYPE"],
                        TASKID: selectItems[i].data["TASKID"],
                        TASKNAME: selectItems[i].data["TASKNAME"],
                        TASKATTR: selectItems[i].data["TASKATTR"],

                        EQUIPMENTNUM: selectItems[i].data["EQUIPMENTNUM"],
                        COMPLETENUM: selectItems[i].data["COMPLETENUM"],
                        NONCOMPLETENUM: selectItems[i].data["NONCOMPLETENUM"],
                        EQUIPMENTID: selectItems[i].data["EQUIPMENTID"],

                        EQUIPMENTNAME: selectItems[i].data["EQUIPMENTNAME"],
                        EQUIPMENTMODEL: selectItems[i].data["EQUIPMENTMODEL"],
                        PRIORITYLEVEL: selectItems[i].data["PRIORITYLEVEL"],
                        ISAUTO: selectItems[i].data["ISAUTO"],
                        PLANSTARTTIME: selectItems[i].data["PLANSTARTTIME"],

                        PLANENDTIME: selectItems[i].data["PLANENDTIME"],
                        PERSONGROUPID: selectItems[i].data["PERSONGROUPID"],
                        PERSONGROUPNAME: selectItems[i].data["PERSONGROUPNAME"],
                        PERSONID: selectItems[i].data["PERSONID"],
                        PERSONNAME: selectItems[i].data["PERSONNAME"],
                        EQUTYPEID: selectItems[i].data["EQUTYPEID"],
                        EQUTYPENAME: selectItems[i].data["EQUTYPENAME"]
                    });
                }
            }

            if (checkItems.length <= 0) {
                Ext.Msg.alert("系统提示", "请选择作业计划");
            }
            else {
                this.win.close();
                fillGetnoticeReturnData.call(this, checkItems);
            }
        }
    }
}

//根据返回的数据list 填充本 datafunc 的grid
function fillData(returnData) {
    Ext.suspendLayouts();//关闭Ext布局
    var formStore = this.dataSet.getTable(1);//tableIndex是指当前grid所在的表索引，中间层第几个表，curStore是grid的数据源，在extjs中是指Store
    formStore.suspendEvents();//关闭store事件
    try {
        this.deleteAll(1);//删除当前grid的数据
        var formBill = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var list = returnData['key'];//一般是中间层返回来的数据，中间可能定义的是dictionary,在前段反序列化之后是对象
        if (list !== undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(formBill, 1);//这个方法第一个参数是表头数据行，第二个参数是当前grid数据源store所属第几个表
                newRow.set('BILLNO', info.BillNo);
                newRow.set('FROMROWID', info.Row_Id);
                newRow.set('ROWTYPE', info.ROWTYPE);
                newRow.set('TASKID', info.TASKID);
                newRow.set('TASKNAME', info.TASKNAME);

                newRow.set('TASKATTR', info.TASKATTR);
                newRow.set('EQUIPMENTNUM', info.EQUIPMENTNUM);
                newRow.set('COMPLETENUM', info.COMPLETENUM);
                newRow.set('NONCOMPLETENUM', info.NONCOMPLETENUM);
                newRow.set('EQUIPMENTID', info.EQUIPMENTID);

                newRow.set('EQUIPMENTNAME', info.EQUIPMENTNAME);
                newRow.set('EQUIPMENTMODEL', info.EQUIPMENTMODEL);
                newRow.set('PRIORITYLEVEL', info.PRIORITYLEVEL);
                newRow.set('ISAUTO', info.ISAUTO);
                newRow.set('PLANSTARTTIME', info.PLANSTARTTIME);

                newRow.set('PLANENDTIME', info.PLANENDTIME);
                newRow.set('PERSONGROUPID', info.PERSONGROUPID);
                newRow.set('PERSONGROUPNAME', info.PERSONGROUPNAME);
                newRow.set('PERSONID', info.PERSONID);
                newRow.set('PERSONNAME', info.PERSONNAME);

                newRow.set('EQUTYPEID', info.EQUTYPEID);
                newRow.set('EQUTYPENAME', info.EQUTYPENAME);
            }
        }
    }
    finally {
        formStore.resumeEvents();//打开store事件
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);//打开Ext布局

    }
}


function fillGetnoticeReturnData(records) {

    var grid = Ext.getCmp(proto.winId + 'EMOVERHAULPLANDETAILGrid');
    Ext.suspendLayouts();
    var fromStore = proto.fromObj.dataSet.getTable(1);
    fromStore.suspendEvents();//关闭store事件
    try {
        if (records !== undefined && records.length > 0)
        {
            for (var i = 0; i < records.length; i++)
            {
                var info = records[i];
                if (checkGetNotice(grid,info))
                {
                    Ext.Msg.alert("系统提示", "所选设备作业计划单明细已经在检修计划单中,请重新选取！");
                }
                else
                {
                    var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
                    var newRow = proto.fromObj.addRow(masterRow, 1);
                    newRow.set('FROMBILLNO', info["BILLNO"]);
                    newRow.set('FROMROWID', info["FROMROWID"]);
                    newRow.set('ROWTYPE', info["ROWTYPE"]);
                    newRow.set('TASKID', info["TASKID"]);
                    newRow.set('TASKNAME', info["TASKNAME"]);

                    newRow.set('TASKATTR', info["TASKATTR"]);
                    newRow.set('EQUIPMENTNUM', info["EQUIPMENTNUM"]);
                    newRow.set('COMPLETENUM', info["COMPLETENUM"]);
                    newRow.set('NONCOMPLETENUM', info["NONCOMPLETENUM"]);
                    newRow.set('EQUIPMENTID', info["EQUIPMENTID"]);

                    newRow.set('EQUIPMENTNAME', info["EQUIPMENTNAME"]);
                    newRow.set('EQUIPMENTMODEL', info["EQUIPMENTMODEL"]);
                    newRow.set('PRIORITYLEVEL', info["PRIORITYLEVEL"]);
                    newRow.set('ISAUTO', info["ISAUTO"]);
                    newRow.set('PLANSTARTTIME', info["PLANSTARTTIME"]);

                    newRow.set('PLANENDTIME', info["PLANENDTIME"]);
                    newRow.set('PERSONGROUPID', info["PERSONGROUPID"]);
                    newRow.set('PERSONGROUPNAME', info["PERSONGROUPNAME"]);
                    newRow.set('PERSONID', info["PERSONID"]);
                    newRow.set('PERSONNAME', info["PERSONNAME"]);

                    newRow.set('EQUTYPEID', info["EQUTYPEID"]);
                    newRow.set('EQUTYPENAME', info["EQUTYPENAME"]);
                }
            }
            //设置检修计划单表头开始 和结束 时间
            SetLastTime(grid);
        }
    }
    finally {
        fromStore.resumeEvents();
        if (fromStore.ownGrid && fromStore.ownGrid.getView().store != null)
            fromStore.ownGrid.reconfigure(fromStore);
        Ext.resumeLayouts(true);
    }
}

//判断 设备作业计划单明细 是否 已经在检修计划单中
function checkGetNotice(grid, info) {
    var checkOk = false;
    var records = grid.store.data.items;
    //设备作业计划单明细 重复判断 
    for (var i = 0; i < records.length; i++) {
        if (records[i].get('FROMBILLNO') == info["BILLNO"] && records[i].get('FROMROWID') == info["FROMROWID"]) {
            checkOk = true;
        }
    }
    return checkOk;
}

//设置检修计划单表头开始 和结束 时间(取 原有明细记录开始时间的最小和结束时间的最大值,选中)
function SetLastTime(grid) {
    var masterRow = proto.fromObj.dataSet.getTable(0).data.items[0];
    var records = grid.store.data.items;
    var tempMax = 0;
    var tempMin = 0;
    if (records.length > 0) {
        tempMax = records[0].get('PLANSTARTTIME');
        tempMin = records[0].get('PLANENDTIME');
        for (var i = 0; i < records.length; i++) {
            if (tempMax >= records[i].get('PLANSTARTTIME')) {
                tempMax = records[i].get('PLANSTARTTIME');
            }
            if (tempMin <= records[i].get('PLANENDTIME')) {
                tempMin = records[i].get('PLANENDTIME');
            }
        }
        masterRow.set("STARTTIME", tempMax);
        masterRow.set("ENDTIME", tempMin);
        proto.fromObj.forms[0].loadRecord(masterRow);
    }
}

