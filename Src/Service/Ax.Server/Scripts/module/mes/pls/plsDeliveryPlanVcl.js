plsDeliveryPlanVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = plsDeliveryPlanVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = plsDeliveryPlanVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.BeforeAddRow:
            e.dataInfo.cancel = true;
            break;


        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "PRODUCTORDERID") {
                    var headTableRow = this.dataSet.getTable(0).data.items[0];
                    var bodyTable = this.dataSet.getTable(1);
                    if (e.dataInfo.value == null) {
                        bodyTable.removeAll();
                    }
                    else {
                        var returnData = this.invorkBcf("GetProjectData", [headTableRow.data["PRODUCTORDERID"]]);
                        if (returnData.length == 0) {
                            Ext.Msg.alert("提示", "投产单为空！");
                            return;
                        }
                        fillDeliveryPlan(this, returnData);
                    }
                }
            }
            break;
    
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "LoadProjectDetail":
                    if (this.billAction == BillActionEnum.Modif || this.billAction == BillActionEnum.AddNew) {
                        var headTableRow = this.dataSet.getTable(0).data.items[0];
                        if (headTableRow.data["PRODUCTORDERID"] == '') {
                            Ext.Msg.alert("提示", '请维护表头投产单！');
                        }
                        else {
                            var returnData = this.invorkBcf("GetProjectData", [headTableRow.data["PRODUCTORDERID"]]);
                            if (returnData.length == 0) {
                                Ext.Msg.alert("提示", "项目单物料为空！");
                                this.deleteall(1);
                                return;
                            }
                            fillDeliveryPlan(this, returnData);
                        }
                    }
                    break;

                case "PrintSend":
                    printDeliveryPlan(this, true);
                    Ext.Msg.alert("提示", "打印成功！");
                    break;
                case "CreateMark":
                    var grid = Ext.getCmp(this.winId + 'PLSDELIVERYPLANDETAILGrid');
                    printSend(this, grid);
                    Ext.Msg.alert("提示", "打印成功！");

                    break;
                case "CreateGasForm":
                    if (this.billAction == BillActionEnum.Modif || this.billAction == BillActionEnum.AddNew) {
                        var grid = Ext.getCmp(this.winId + 'PLSDELIVERYPLANDETAILGrid');
                        var records = grid.getView().getSelectionModel().getSelection();
                        if (records.length == 0) {
                            Ext.Msg.alert("提示", "请选择要添加储气罐的行！");

                            return;
                        }
                        else if (records.length > 1) {
                            Ext.Msg.alert("提示", "添加储气罐只能选择一行！");

                            return;
                        }
                        //else if (records[0].data["MATERIALID"] == '') {
                        //    alert("该行不能继续添加储气罐信息！");
                        //    return;
                        //}


                        CreateGasForm(records[0], this);
                    }
                    else {
                        Ext.Msg.alert("提示", "编辑状态下才能添加储气罐信息！");

                    }
                    break;
            }
            break;
    }
}


function fillDeliveryPlan(This, returnData) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        This.deleteAll(1);
        var masterRow = This.dataSet.getTable(0).data.items[0];
        if (returnData !== undefined && returnData.length > 0) {
            for (var i = 0; i < returnData.length; i++) {
                var info = returnData[i];
                var newRow = This.addRow(masterRow, 1);
                newRow.set('ROW_ID', info.RowId);
                newRow.set('ROWNO', info.RowNo);
                newRow.set('METERNO', info.MeterNo);
                newRow.set('MATERIALID', info.MaterialId);
                newRow.set('MATERIALNAME', info.MaterialName);
                newRow.set('MATERIALSPEC', info.MaterialSpec);
                newRow.set('TEXTUREID', info.Textureid);
                newRow.set('SPECIFICATION', info.SpecIfication);
                newRow.set('UNITNAME', info.UnitName);
                newRow.set('QUANTITY', info.Quantity);
                newRow.set('GROUPNO', info.GroupNo);

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

function printDeliveryPlan(This, isPaint) {

    //构建表身
    var strTableTrHtml = "";

    var strTableEndHtml = "</table>";



    var strTableStartHtml = "<table border='1' id='theTable' width='100%'   bordercolor='#000000' cellpadding='0' cellspacing='0' align='center' >";

    var reMark = "<thead style='height: 33px' face='宋体' bgcolor='#FFFFFF'>";

    reMark += "<td nowrap align='center' colspan =10 ><font size='3px' face='宋体'>" + "备注：所有物流产生费用均由发货方支付，承运公司必须送货上门，不得向收货人收取任何费用" + "</font></td>";
    reMark += "</thead>";

    reMark += "<thead style='height: 38px' face='宋体' bgcolor='#FFFFFF'>";
    reMark += "<td nowrap align='center' colspan =10 ><font size='5px' face='宋体'>" + "发货清单" + "</font></td>";
    reMark += "</thead>";

    var strTableTheadHtml = "<thead style='height: 38px' face='宋体' bgcolor='#FFFFFF'>";
    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>序号</font></th>";
    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>仪表位号</font></th>";
    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>产品名称</font></th>";//nowrap自动换行
    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>规格型号</font></th>";
    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>包装尺寸(CM)</font></th>";
    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>毛重(Kg)</font></th>";
    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>净重(Kg)</font></th>";
    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>数量</font></th>";
    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>单位</font></th>";
    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>备注</font></th>";
    strTableTheadHtml += "</thead>";
    var bodyTable = This.dataSet.getTable(1);
    var headTable = This.dataSet.getTable(0);
    var count = 0;
    for (var i = 0; i < bodyTable.data.items.length; i++) {
        count += bodyTable.data.items[i].data["QUANTITY"];
        strTableTheadHtml += "<tr>";
        strTableTheadHtml += "<td nowrap align='center' height=30><font size='2px' face='宋体'>" + (i + 1) + "</font></td>";
        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["METERNO"] + "</font></td>";
        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["MATERIALNAME"] + "</font></td>";
        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["MATERIALSPEC"] + "</font></td>";
        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["LENGTH"] + '*' + bodyTable.data.items[i].data["WIDTH"] + '*' + bodyTable.data.items[i].data["HIGH"] + "</font></td>";
        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["GROSSWEIGHT"] + "</font></td>";
        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["NETWEIGHT"] + "</font></td>";

        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["QUANTITY"] + "</font></td>";
        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["UNITNAME"] + "</font></td>";
        strTableTheadHtml += "<td nowrap align='center'  width=100><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["REMARK"] + "</font></td>";
        //strTableTheadHtml += "<td nowrap align='center' width=120><font size='2px' face='宋体'>" + "这个备注有点长，很长很长的备注" + "</font></td>";

        strTableTheadHtml += "</tr>";
    }


    strTableTheadHtml += "<thead style='height: 30px' face='宋体' bgcolor='#FFFFFF'>";

    strTableTheadHtml += "<td nowrap align='center' colspan =10 ><font size='3px' face='宋体'>" + "合同编号：" + headTable.data.items[0].data["PROJECTID"] + "</font></td>";
    strTableTheadHtml += "</thead>";

    strTableTheadHtml += "<thead style='height: 30px' face='宋体' bgcolor='#FFFFFF'>";
    strTableTheadHtml += "<td nowrap align='center' colspan =10 ><font size='3px' face='宋体'>" + "项目名称：" + headTable.data.items[0].data["PROJECTNAME"] + "</font></td>";
    strTableTheadHtml += "</thead>";


    strTableTheadHtml += "<thead style='height: 30px' face='宋体' bgcolor='#FFFFFF'>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'></font></td>";
    strTableTheadHtml += "<td nowrap align='center' colspan =6><font size = '2px' face='宋体'>合计</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>" + count + "</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>箱</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'></font></td>";
    strTableTheadHtml += "</thead>";

    var LODOP = getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
    LODOP.ADD_PRINT_TEXT(40, 400, 650, 40, "浙江中德自控科技股份有限公司");
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 18);
    LODOP.ADD_PRINT_IMAGE(20, 40, "30%", "100%", "<img src=./Scripts/img/images/zhongdePrint.png height=60 width=60>");//商标

    LODOP.ADD_PRINT_TEXT(100, 25, 370, 20, "收货单位：  " + headTable.data.items[0].data["CUSTOMERNAME"]);
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
    LODOP.ADD_PRINT_TEXT(100, 455, 400, 20, "浙江省长兴县太湖街道长兴大道659号");
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

    LODOP.ADD_PRINT_TEXT(140, 25, 370, 20, "联 系 人：  " + headTable.data.items[0].data["RECPERSON"]);
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

    LODOP.ADD_PRINT_TEXT(180, 25, 370, 20, "电    话：  " + headTable.data.items[0].data["PHONE"]);
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

    var address = headTable.data.items[0].data["ADDRESS"];
    if (address.length > 34) {
        LODOP.ADD_PRINT_TEXT(200, 25, 370, 20, "地    址：  " + address.substring(0, 16));
        LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
        LODOP.ADD_PRINT_TEXT(220, 25, 370, 20, "            " + address.substring(17, 33));
        LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
        LODOP.ADD_PRINT_TEXT(240, 25, 370, 20, "            " + address.substring(34));
        LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

    }
    else if (address.length > 17) {
        LODOP.ADD_PRINT_TEXT(210, 25, 370, 20, "地    址：  " + address.substring(0, 16));
        LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
        LODOP.ADD_PRINT_TEXT(230, 25, 370, 20, "            " + address.substring(17));
        LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
    }
    else {
        LODOP.ADD_PRINT_TEXT(220, 25, 370, 20, "地    址：  " + headTable.data.items[0].data["ADDRESS"]);
        LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
    }


    LODOP.ADD_PRINT_TEXT(260, 25, 370, 20, "签 收 人/时间：");
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

    LODOP.ADD_PRINT_TEXT(300, 25, 370, 20, "包装损坏情况 ：");
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);



    LODOP.ADD_PRINT_TEXT(140, 415, 370, 20, "发货联系人：       徐杨   0572-6660035");
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

    LODOP.ADD_PRINT_TEXT(180, 415, 370, 20, "售后服务部：       卢跃辉 0572-6660121");
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

    LODOP.ADD_PRINT_TEXT(210, 415, 370, 20, "资料员电话：       钱炜   0572-6660113");
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
    LODOP.ADD_PRINT_TEXT(230, 415, 370, 20, "传      真：             0572-6556888");
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

    LODOP.ADD_PRINT_TEXT(260, 415, 370, 20, "合同顺序号：");
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

    LODOP.ADD_PRINT_TEXT(300, 415, 370, 20, "发货日期：");
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);


    //LODOP.SET_PRINT_STYLEA(0, "Alignment", 2);

    //LODOP.PRINT_INITA(0, 0, "100%", "100%", "打印控件功能演示_Ext");


    var htmlStr = strTableStartHtml + reMark + strTableTheadHtml + strTableEndHtml;

    LODOP.ADD_PRINT_HTM(320, 1, "100%", "100%", htmlStr); //ADD_PRINT_HTM(Top,Left,Width,Height,strHtmlContent)

    LODOP.SET_PRINT_PAGESIZE(1, 0, 0, "A4"); //1---纵(正)向打印  2---横向打印 SET_PRINT_PAGESIZE(intOrient,intPageWidth,intPageHeight,strPageName)设定纸张大小

    if (isPaint) {
        LODOP.PRINT();
        //LODOP.PREVIEW();
    }
    else {
        LODOP.PREVIEW();
    }

}

function printSend(This, grid) {
    var returnData = This.invorkBcf("GetTxt", []);
    var LODOP = getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
    //var grid = Ext.getCmp(this.winId + 'PLSDELIVERYPLANDETAILGrid');
    var records = grid.getView().getSelectionModel().getSelection();
    if (records.length == 0) {
        alert("请选择打印的明细！");
        return;
    }
    var headRow = This.dataSet.getTable(0).data.items[0].data;
    var bodytable = This.dataSet.getTable(1).data;
    for (var i = 0; i < records.length; i++) {

        var record = records[i].data;
        var size = record["LENGTH"] + '*' + record["WIDTH"] + '*' + record["HIGH"]
        htmlStr = returnData;
        htmlStr = htmlStr.replace("@收货单位", headRow["CUSTOMERNAME"]);
        htmlStr = htmlStr.replace("@收货地址", headRow["ADDRESS"]);
        htmlStr = htmlStr.replace("@收货人", headRow["RECPERSON"]);
        htmlStr = htmlStr.replace("@联系方式", headRow["PHONE"]);
        htmlStr = htmlStr.replace("@合同编号", headRow["PROJECTID"]);

        htmlStr = htmlStr.replace("@货物名称", record["MATERIALNAME"]);
        htmlStr = htmlStr.replace("@规格型号", record["SPECIFICATION"]);
        htmlStr = htmlStr.replace("@订单位号", record["METERNO"]);
        htmlStr = htmlStr.replace("@物资编码", record["MATERIALID"]);
        htmlStr = htmlStr.replace("@项目名称", headRow["PROJECTNAME"]);
        htmlStr = htmlStr.replace("@外包装尺寸", size);
        htmlStr = htmlStr.replace("@毛重", record["GROSSWEIGHT"]);
        htmlStr = htmlStr.replace("@净重", record["NETWEIGHT"]);
        var Index = 0;
        var AllIndex = 0;
        var MaterialIndex = 1;
        for (var i = 0; i < bodytable.items.length; i++) {
            if (bodytable.items[i].data["GROUPNO"] == record["GROUPNO"]) {
                AllIndex++;
                if (bodytable.items[i].data["ROWNO"] == record["ROWNO"]) {
                    Index = AllIndex;
                }
                if (bodytable.items[i].data["ISGAS"] == 1) {
                    MaterialIndex = AllIndex;
                }
            }
            
        }

        htmlStr = htmlStr.replace("@Index", Index);
        htmlStr = htmlStr.replace("@AllIndex", AllIndex);
        htmlStr = htmlStr.replace("@MaterialIndex", MaterialIndex);

        LODOP.ADD_PRINT_HTM(0, 3, "100%", "100%", htmlStr); //ADD_PRINT_HTM(Top,Left,Width,Height,strHtmlContent)

        LODOP.SET_PRINT_PAGESIZE(1, 0, 0, "A4"); //1---纵(正)向打印  2---横向打印 SET_PRINT_PAGESIZE(intOrient,intPageWidth,intPageHeight,strPageName)设定纸张大小

        //LODOP.PREVIEW();
        LODOP.PRINT();
       

    }


}

function CreateGasForm(row, This) {

    CreatGasForm(row, This);
}


//最新特征窗体
function CreatGasForm(row, This) {
    //确认按钮
    var btnSaleConfirm = new Ext.Button({
        width: 150,
        height: 35,
        text: "确定",
        type: 'submit',
        handler: function () {
            var yes = true;
            var thisWin = Ext.getCmp("attWin");
            if (This.billAction == BillActionEnum.Modif || This.billAction == BillActionEnum.AddNew) {
                var Panel = thisWin.items.items[0];
                var Id = Panel.items.items[1].value;
                var Name = Panel.items.items[2].value;
                var Style = Panel.items.items[3].value;
                if (yes) {
                    AddGasRow(Name, Style, Id, row.data["METERNO"], row.data["GROUPNO"], This);
                    thisWin.close();
                }

            }


        }
    })
    //取消按钮
    var btnSaleCancel = new Ext.Button({
        width: 150,
        height: 35,
        text: "取消",
        type: 'submit',
        handler: function () {
            Ext.getCmp("attWin").close();
        }
    })
    //按钮Panle
    var btnSalePanel = new Ext.form.Panel({
        //layout: 'column',
        width: '100%',
        collapse: false,
        defaults: {
            margin: '0 40 0 80',//上右下左
            columnWidth: .5
        },
        items: [btnSaleConfirm, btnSaleCancel]
    })
    var classPanel = new Ext.form.Panel({
        layout: 'column',
        width: '100%',
        height: 90,
        items: [
             new Ext.form.TextField({
                 fieldLabel: "位号",
                 value: row.data["METERNO"],
                 maxLength: 50,
                 margin: '5 10 5 10',
                 disabled: true,
                 columnWidth: .5,
                 labelWidth: 80,
             }),
              new Ext.form.TextField({
                  fieldLabel: "物料编码",
                  value: row.data["MATERIALID"],
                  maxLength: 50,
                  margin: '5 10 5 10',
                  disabled: true,
                  columnWidth: .5,
                  labelWidth: 80,
              }),
               new Ext.form.TextField({
                   fieldLabel: "储气罐名称",
                   value: "",
                   maxLength: 50,
                   margin: '5 10 5 10',
                   columnWidth: .5,
                   labelWidth: 80,

               }),
                new Ext.form.TextField({
                    fieldLabel: "储气罐型号",
                    value: "",
                    maxLength: 50,
                    margin: '5 10 5 10',
                    columnWidth: .5,
                    labelWidth: 80,
                }),

        ]
    })
    var Salewin = new Ext.create('Ext.window.Window', {
        id: "attWin",
        title: '储气罐信息',
        resizable: false,
        modal: true,
        width: 600,
        height: 180,
        autoScroll: true,
        defaults: {
            margin: '0 0 0 0',//上右下左
        },
        items: [classPanel, btnSalePanel],
    });

    Salewin.show();
}

function AddGasRow(Name, Style, Id, MeterNo, GroupNo, This) {
    Ext.suspendLayouts();
    var formStore = This.dataSet.getTable(1);
    formStore.suspendEvents();
    try {
        var masterRow = This.dataSet.getTable(0).data.items[0];

        var newRow = This.addRow(masterRow, 1);
        newRow.set('METERNO', MeterNo);
        newRow.set('MATERIALNAME', Name);
        newRow.set('MATERIALID', Id);
        newRow.set('ISGAS', 0);
        newRow.set('GROUPNO', GroupNo);
        newRow.set('SPECIFICATION', Style);
    }
    finally {
        formStore.resumeEvents();
        if (formStore.ownGrid && formStore.ownGrid.getView().store != null)
            formStore.ownGrid.reconfigure(formStore);
        Ext.resumeLayouts(true);
    }
}
