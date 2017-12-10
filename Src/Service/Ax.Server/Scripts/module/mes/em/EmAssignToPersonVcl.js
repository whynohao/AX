EmAssignToPersonVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
    this.dataRow;
};
var btnSelect = 0;//记录全选还是不选
var attId = 0;
var proto = EmAssignToPersonVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = EmAssignToPersonVcl;

//界面加载
proto.doSetParam = function () {
    var returnList = this.invorkBcf("GetRepairListInfo", [0, 0]);
    FillMaintainData.call(this, returnList);
};
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        //全选按钮事件
        if (e.dataInfo.fieldName == "btnSelect") {
            var masterRow = this.dataSet.getTable(0).data.items[0];
            this.forms[0].updateRecord(masterRow);
            var returnList = this.invorkBcf("GetRepairListInfo", [masterRow.data["STARTDATE"], masterRow.data["ENDDATE"]]);
            FillMaintainData.call(this, returnList);
        }
        else if (e.dataInfo.fieldName == "btnAssignTaskToPerson") {
            var allItems = this.dataSet.getTable(1).data.items;
            var checkItems = [];
            var mark = true;
            var errorMessage = "";
            if (allItems.length > 0) {
                for (var i = 0; i < allItems.length; i++) {
                    if (allItems[i].data["ISCHOSE"] == true) {
                        var g = allItems[i].data["EMFAULTID"];
                        var f = allItems[i].data["REPAIRBILLNO"];
                        errorMessage = "第" + allItems[i].data["ROWNO"] + "行";
                        if (allItems[i].data["PERSONGROUPID"] == "" || allItems[i].data["PERSONID"] == "") {
                            mark = false;
                            errorMessage += "人员组和人员不能为空;";
                        }
                        if (allItems[i].data["PLANSTARTTIME"] == 0 || allItems[i].data["PLANENDTIME"] == 0) {
                            mark = false;
                            errorMessage += "计划开始时间和计划结束时间不能为空;";
                        }
                        var flag = this.invorkBcf("IsTaskStart", [allItems[i].data["EMFAULTID"], allItems[i].data["REPAIRBILLNO"], allItems[i].data["PLANSTARTTIME"], allItems[i].data["PLANENDTIME"]]);
                        if (!flag) {
                            mark = false;
                            errorMessage += "报修单对应维修作业已经执行，不能修改计划开始时间和结束时间";
                        }
                        if (!mark)
                            break;
                        else {
                            checkItems.push({
                                BillNo: allItems[i].data["REPAIRBILLNO"],
                                RepairDate: allItems[i].data["REPAIRDATE"],
                                RepairPersonId: allItems[i].data["REPAIRPERSONID"],
                                RepairPersonName: allItems[i].data["REPAIRPERSONNAME"],
                                DeptId: allItems[i].data["DEPTID"],
                                DeptName: allItems[i].data["DEPTNAME"],
                                EquipmentId: allItems[i].data["EQUIPMENTID"],
                                EquipmentName: allItems[i].data["EQUIPMENTNAME"],
                                EmFaultId: allItems[i].data["EMFAULTID"],
                                EmFaultName: allItems[i].data["EMFAULTNAME"],
                                PersonLeader: allItems[i].data["PERSONLEADER"],
                                PersonLeaderName: allItems[i].data["PERSONLEADERNAME"],
                                PlanStartTime: allItems[i].data["PLANSTARTTIME"],
                                PlanEndTime: allItems[i].data["PLANENDTIME"],
                                PersonGroupId: allItems[i].data["PERSONGROUPID"],
                                PersonGroupName: allItems[i].data["PERSONGROUPNAME"],
                                PersonId: allItems[i].data["PERSONID"],
                                PersonName: allItems[i].data["PERSONNAME"],
                                FactoryId: allItems[i].data["FACTORYID"],
                                FactoryName: allItems[i].data["FACTORYNAME"],
                                ProduceLineId: allItems[i].data["PRODUCELINEID"],
                                ProduceLineName: allItems[i].data["PRODUCELINENAME"],
                                Prioritylevel: allItems[i].data["PRIORITYLEVEL"]
                            });
                        }
                    }
                }
            }
            if (mark) {
                if (checkItems.length > 0) {
                    this.invorkBcf("AssiToPerson", [checkItems]);
                    Ext.Msg.alert("系统提示", "指派成功");
                }
            } else {
                Ext.Msg.alert("系统提示", errorMessage);
            }
        }
        else if (e.dataInfo.fieldName == "btnSelectAll") {
            var allItems = this.dataSet.getTable(1).data.items;
            if (btnSelect == 0) {
                for (var i = 0; i < allItems.length; i++) {
                    allItems[i].set("ISCHOSE", true);
                }
                btnSelect = 1;
            }
            else {
                for (var i = 0; i < allItems.length; i++) {
                    allItems[i].set("ISCHOSE", false);
                }
                btnSelect = 0;
            }
        }
    }
    else if (e.libEventType == LibEventTypeEnum.ColumnDbClick) {
        if (e.dataInfo.fieldName == "PERSONGROUPNAME") {
            var emFaultId = e.dataInfo.dataRow.data["EMFAULTID"];
            var factoryId = e.dataInfo.dataRow.data["FACTORYID"];
            var produceLineId = e.dataInfo.dataRow.data["PRODUCELINEID"];
            var regionPersonGroup = e.dataInfo.dataRow.data["REGIONPERSONGROUP"];
            var returnData = this.invorkBcf('GetPersonGroupList', [emFaultId, factoryId, produceLineId, regionPersonGroup]);
            var dataList = {
                BillNo: e.dataInfo.dataRow.data["BILLNO"],
                Row_Id: e.dataInfo.dataRow.data["ROW_ID"]
            };
            CreatAttForm(dataList, returnData, this, e, FillDataRow);
        }
    }
}

function FillMaintainData(returnList) {
    Ext.suspendLayouts();//关闭Ext布局
    var curStore = this.dataSet.getTable(1);
    curStore.suspendEvents();//关闭store事件
    try {
        this.dataSet.getTable(1).removeAll();//删除当前grid的数据
        var masterRow = this.dataSet.getTable(0).data.items[0];//找到表头的数据
        var list = returnList;
        if (list != undefined && list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                var info = list[i];
                var newRow = this.addRow(masterRow, 1);
                newRow.set("REPAIRBILLNO", info.BillNo);
                newRow.set("ROWNO", i + 1);
                newRow.set("REPAIRDATE", info.RepairDate);
                newRow.set("REPAIRPERSONID", info.RepairPersonId);
                newRow.set("REPAIRPERSONNAME", info.RepairPersonName);
                newRow.set("DEPTID", info.DeptId);
                newRow.set("DEPTNAME", info.DeptName);
                newRow.set("EQUIPMENTID", info.EquipmentId);
                newRow.set("EQUIPMENTNAME", info.EquipmentName);
                newRow.set("EQUIPMENTMODEL", info.EquipmentModel);
                newRow.set("EMFAULTID", info.EmFaultId);
                newRow.set("EMFAULTNAME", info.EmFaultName);
                newRow.set("PERSONLEADER", info.PersonLeader);
                newRow.set("PERSONLEADERNAME", info.PersonLeaderName);
                newRow.set("PLANSTARTTIME", info.PlanStartTime);
                newRow.set("PLANENDTIME", info.PlanEndTime);
                newRow.set("PERSONGROUPID", info.PersonGroupId);
                if (info.PersonGroupName != "")
                    newRow.set("PERSONGROUPNAME", info.PersonGroupName);
                else
                    newRow.set("PERSONGROUPNAME", "双击选择");
                newRow.set("PERSONID", info.PersonId);
                newRow.set("PERSONNAME", info.PersonName);
                newRow.set("FACTORYID", info.FactoryId);
                newRow.set("FACTORYNAME", info.FactoryName);
                newRow.set("PRODUCELINEID", info.ProduceLineId);
                newRow.set("PRODUCELINENAME", info.ProduceLineName);
                newRow.set("PRIORITYLEVEL", info.Prioritylevel);
            }
        }
    } finally {
        curStore.resumeEvents();//打开store事件
        if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
            curStore.ownGrid.reconfigure(curStore);
        Ext.resumeLayouts(true);//打开Ext布局
    }
}

//创建窗体
function CreatAttForm(dataList, returnData, This, row, method) {
    var BillNo = dataList.BillNo;
    var Row_Id = dataList.Row_Id;
    var standard = [];
    if (returnData.length == 0) {
        Ext.Msg.alert("提示", '没有符合的人员组！');
        return;
    }
    standard.push(CreatComBox(returnData));
    //Panel
    var attPanel = new Ext.form.Panel({

    })
    //确认按钮
    var btnSaleConfirm = new Ext.Button({
        width: 200,
        height: 35,
        text: "确定",
        type: 'submit',
        handler: function () {
            var yes = true;
            var thisWin = Ext.getCmp("attWin" + BillNo + Row_Id);
            var attPanel = thisWin.items.items[0];
            var attDic = [];
            var msg = '';
            attDic.push({
                PersonGroupId: attPanel.items.items[0].value, PersonGroupName: attPanel.items.items[0].rawValue
            });
            if (attDic.length > 0) {
                yes = method(row, This, attDic);
            }
            else {
                Ext.Msg.alert("提示", '请选择人员组！');
            }
            if (yes) {
                thisWin.close();
            }
        }
    })
    //取消按钮
    var btnSaleCancel = new Ext.Button({
        width: 200,
        height: 35,
        text: "取消",
        type: 'submit',
        handler: function () {
            Ext.getCmp("attWin" + BillNo + Row_Id).close();
        }
    })
    //按钮Panle
    var btnSalePanel = new Ext.form.Panel({
        layout: 'column',
        width: '100%',
        defaults: {
            margin: '10 40 0 40',
            columnWidth: .5
        },
        items: [btnSaleConfirm, btnSaleCancel]
    })

    var Salewin = new Ext.create('Ext.window.Window', {
        id: "attWin" + BillNo + Row_Id,
        title: '人员组信息',
        resizable: false,
        //closeAction: "close",
        modal: true,
        width: 600,
        height: 350,
        autoScroll: true,
        layout: 'column',
        items: [{
            id: 'Att' + attId,
            layout: 'column',
            xtype: 'fieldset',
            title: '人员组选择',
            //collapsed: true,
            collapsible: true,
            width: '96%',
            height: 200,
            defaulType: 'combobox',
            margin: '5 10 5 10',
            autoScroll: true,
            items: standard,
        }, btnSalePanel],
    });
    //attId++;
    Salewin.show();
}
//生成combox空间
function CreatComBox(attData) {
    var attlist = [];
    for (var i = 0; i < attData.length; i++) {
        var data = { PersonGroupId: attData[i]['PersonGroupId'], PersonGroupName: attData[i]['PersonGroupName'] };
        attlist.push(data);
    }
    var Store = Ext.create("Ext.data.Store", {
        fields: ["PersonGroupId", "PersonGroupName"],
        data: attlist
    });
    var combox = new Ext.form.ComboBox({
        mode: 'local',
        forceSelection: true,
        triggerAction: 'all',
        displayField: 'PersonGroupName',
        fieldLabel: '人员组',
        attId: attData.PersonGroupId,
        valueField: 'PersonGroupId',
        fields: ['PersonGroupId', 'PersonGroupName'],
        store: Store,
        //value: attData.DefaultValue,
        //editable: true,
        //text: attData.DefaultValue,
        margin: '5 10 5 10',
        columnWidth: .5,
        labelWidth: 60,
    });
    return combox;
}
//数据回填
function FillDataRow(e, This, CodeDesc) {
    e.dataInfo.dataRow.set("PERSONGROUPID", CodeDesc[0].PersonGroupId);
    e.dataInfo.dataRow.set("PERSONGROUPNAME", CodeDesc[0].PersonGroupName);
    return true;
}