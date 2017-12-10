comAbnormalMaterialStockVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
}
//grid用Ax.vcl.LibVclGrid,单据主数据用Ax.vcl.LibVclData,datafunc用Ax.vcl.LibVclGrid
var proto = comAbnormalMaterialStockVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = comAbnormalMaterialStockVcl;
proto.winId = "";
proto.doSetParam = function () {
    //proto.winId = vclObj[0].winId;
    //proto.fromobj = vclObj[0];
    var name = this.invorkBcf('Person', []);
    this.dataSet.getTable(0).data.items[0].set("HANDERPERSON", name);
};
var ROW_ID = new Array();

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);

    switch (e.libEventType) {

        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnLoad") {//查询按钮
                var table = this.dataSet.getTable(0).data;
                //获取表头日期和处理状态
                var planDate = table.items[0].data['PLANDATE'];
                var factoryId = table.items[0].data["FACTORYID"];
                var fromBillNo = table.items[0].data["FROMBILLNO"];
                var handerPerson = table.items[0].data["HANDERPERSON"];



                //调用后台方法获取异常备料过账表数据
                var data = this.invorkBcf("GetMatearialStockPost", [planDate, factoryId, fromBillNo, handerPerson]);
                FillData.call(this, data);
                //给计划开始日期赋值（当天日期）
                //给计划完成日期赋值（异常结束日期）
                var endDate;
                var myDate = new Date();
                var now = getDate(myDate.toLocaleDateString().split('/'));
                var table1 = this.dataSet.getTable(1).data;
                for (var i = 0; i < table1.length; i++) {
                    if (table1.items[i].data["STARTDATE"] == 0) {
                        table1.items[i].set("STARTDATE", now);
                    }
                    if (table1.items[i].data["FINISHDATE"] == 0) {
                        endDate = table1.items[i].get("ENDDATE");
                        table1.items[i].set("FINISHDATE", endDate);
                    }


                }
                Ext.Msg.alert("提示", "查询成功！");

            }
            if (e.dataInfo.fieldName == "BtnHander") {//处理按钮
                if (this.dataSet.getTable(1).data.length != 0) {
                    var bodyTable = this.dataSet.getTable(1).data.items;
                    var list = new Array();

                    for (var i = 0; i < bodyTable.length; i++) {
                        list.push({//传入的数据
                            ROW_ID: bodyTable[i].data["ROW_ID"],
                            ENDDATE: bodyTable[i].data["ENDDATE"],
                            HANDERWAY: bodyTable[i].data["HANDERWAY"],
                            CURRSTATE: bodyTable[i].data["CURRSTATE"],
                            FROMBILLNO: bodyTable[i].data["FROMBILLNO"],
                            PROGID: bodyTable[i].data["PROGID"],
                            MATERIALID: bodyTable[i].data["MATERIALID"],
                            ATTRBUTECODE: bodyTable[i].data["ATTRBUTECODE"],
                            STARTDATE: bodyTable[i].data["STARTDATE"],
                            ABNORMALDAY: bodyTable[i].data["ABNORMALDAY"],
                            FINISHDATE: bodyTable[i].data["FINISHDATE"],
                        });
                    }
                    //var list = MakeList.call(this);
                    var handerSuccess = false;

                    //调用后台方法，将处理方式和处理结果更新到异常备料过账表中
                    handerSuccess = this.invorkBcf("HanderAbnormalMaterialStock", [list, ROW_ID]);

                    if (handerSuccess == true) {
                        Ext.Msg.alert("提示", "处理成功！");
                        this.dataSet.getTable(1).removeAll();
                    }
                }
                else {
                    Ext.Msg.alert("提示", "暂无可处理项！");
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                this.forms[0].updateRecord(e.dataInfo.dataRow);//失去焦点就更新页面数据，确保按按钮时可以读取到数据
            }
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "HANDERWAY" || e.dataInfo.fieldName == "STARTDATE" || e.dataInfo.fieldName == "FINISHDATE") {
                    var canBeInsert = true;
                    for (var i = 0; i < ROW_ID.length; i++) {
                        if (e.dataInfo.dataRow.data["ROW_ID"] == ROW_ID[i])//如果这行已经是修改过的行则不用添加ROW_ID
                        {
                            canBeInsert = false;
                        }
                    }
                    if (canBeInsert == true) {
                        ROW_ID.push(e.dataInfo.dataRow.data["ROW_ID"]);
                    }
                }
            }
            break;
        case LibEventTypeEnum.BeforeAddRow://只能删除行，不允许新增行
            if (e.dataInfo.tableIndex == 1) {
                e.dataInfo.cancel = true;
            }
            break;
    }


    function FillData(returnData) {
        Ext.suspendLayouts();//关闭Ext布局
        //  console.log(this.dataSet);
        var curStore = this.dataSet.getTable(1);
        curStore.suspendEvents();//关闭store事件
        try {
            this.dataSet.getTable(1).removeAll();
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var list = returnData;
            if (list != undefined && list.length > 0) {
                for (var i = 0; i < list.length; i++) {
                    var info = list[i];
                    var newRow = this.addRow(masterRow, 1);
                    newRow.set("HANDERWAY", info.HanderWay);
                    newRow.set("FROMBILLNO", info.FromBillNo);
                    newRow.set("PROGID", info.ProgId);
                    newRow.set("PROGNAME", info.ProgName);
                    newRow.set("MATERIALID", info.MaterialId);
                    newRow.set("MATERIALNAME", info.MaterialName);
                    newRow.set("ATTRIBUTEID", info.AttributeId);
                    newRow.set("ATTRIBUTENAME", info.AttributeName);
                    newRow.set("ATTRBUTECODE", info.AttrbuteCode);
                    newRow.set("ATTRBUTEDESC", info.AttrbuteDesc);
                    newRow.set("QUANTITY", info.Quantity);
                    newRow.set("ABNORMALDAY", info.AbnormalDay);
                    newRow.set("ENDDATE", info.EndDate);
                    newRow.set("HANDERNUM", info.HanderNum);
                    newRow.set("HANDERPERSON", info.HanderPerson);
                    newRow.set("CURRSTATE", info.CurrState);
                    newRow.set("CREATETIME", info.CreateTime);
                    newRow.set("STARTDATE", info.StartDate);
                    newRow.set("FINISHDATE", info.FinishDate);
                    newRow.set("ACTFINISHDATE", info.ActFinishDate);
                }
            }
        } finally {
            curStore.resumeEvents();//打开store事件
            if (curStore.ownGrid && curStore.ownGrid.getView().store != null)
                curStore.ownGrid.reconfigure(curStore);
            Ext.resumeLayouts(true);//打开Ext布局
        }
    }

}

//日期转YYYYMMDD
function getDate(date) {
    var year = date[0];
    var month = date[1];
    if (month.length == 1) {
        month = '0' + month;
    }
    var day = date[2];
    if (day.length == 1) {
        day = '0' + day;
    }
    return year + month + day;
}