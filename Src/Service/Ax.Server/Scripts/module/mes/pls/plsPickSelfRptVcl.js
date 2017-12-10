plsPickSelfRptVcl = function () {
    Ax.vcl.LibVclDailyRpt.apply(this, arguments);
};
var proto = plsPickSelfRptVcl.prototype = Object.create(Ax.vcl.LibVclDailyRpt.prototype);
proto.constructor = plsPickSelfRptVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDailyRpt.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == "SALESORDERNO") {
                    var progId = "pls.SalesOrder";
                    var billNo = e.dataInfo.dataRow.data["SALESORDERNO"];//来源单号
                    Ax.utils.LibVclSystemUtils.openBill(progId, 1, "销售订单", BillActionEnum.Modif, this.entryParam, [billNo]);
                }
            }
            break;
    }
}

//获取单据表头日期年月日  XXXX-XX-XX
function ShortTodateTimeSelf(longTime) {
    longTime = longTime.toString();
    var time = "";
    if (longTime > 0) {
        return longTime.substr(0, 4) + "-" + longTime.substr(4, 2) + "-" + longTime.substr(6, 2);
    }
    return time;
}

//获取单据日期月日  XX月XX日
function ShortTodateTimeSelfTwo(longTime) {
    longTime = longTime.toString();
    var time = "";
    if (longTime > 0) {
        return longTime.substr(4, 2) + "月" + longTime.substr(6, 2) + "日"
    }
    return time;
}

proto.print = function () {
    var dt = this.dataSet.getTable(0).data.items;
    if (dt.length == 0) {
        Ext.Msg.alert("提示", '报表为空！');
        return;
    }

    var planLogisticsDate = dt[0].data["PLANLOGISTICSDATE"];
    var strHead = "<div align=center><strong><font size = '5px'> " + ShortTodateTimeSelf(planLogisticsDate) + "物流发货报表</font></strong></div>";
    var strTableStartHtml = "<table border='1' width='100%' bordercolor='#336699' cellpadding='0' cellspacing='0' align='center'>";

    var tr = "<tr >";
    tr += "<th>门</th>";
    tr += "<th>门套</th>";
    tr += "<th>柜门</th>";
    tr += "<th>筒子板</th>";
    tr += "<th>贴脸</th>";
    tr += "<th>踢脚线</th>";
    tr += "<th>其它</th>";
    tr += "</tr>";

    var strTableTheadHtml = "";
    strTableTheadHtml += "<tr>";
    strTableTheadHtml += "<th rowspan='2'><b>下单日期</b></th>";
    strTableTheadHtml += "<th rowspan='2'><b>序号</b></th>";
    strTableTheadHtml += "<th rowspan='2'><b>物流公司</b></th>";
    strTableTheadHtml += "<th rowspan='2'><b>经销网点</b></th>";
    strTableTheadHtml += "<th rowspan='2'><b>生产批号</b></th>";
    strTableTheadHtml += "<th rowspan='2'><b>树种</b></th>";
    strTableTheadHtml += "<th rowspan='2'><b>油漆类型</b></th>";
    strTableTheadHtml += "<th colspan='7'>明细</th>";
    strTableTheadHtml += "<th rowspan='2'><b>总发货件数</b></th>";
    strTableTheadHtml += "<th rowspan='2'><b>备注</b></th>";
    strTableTheadHtml += "</tr>";
    strTableTheadHtml += tr;

    //构建表身
    var strTableTrHtml = "";
    for (var i = 0; i < dt.length; i++) {
        strTableTrHtml += "<tr>";

        strTableTrHtml += "<th>" + ShortTodateTimeSelfTwo(dt[i].data["SINGLEDATE"]) + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["ROWNO"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["LOGISTICSCOMPANYNAME"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["CUSTOMERNAME"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["LOTNO"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["TREESPECIES"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["PAINTTYPE"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["MATERIALTYPEID1"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["MATERIALTYPEID2"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["MATERIALTYPEID3"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["MATERIALTYPEID4"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["MATERIALTYPEID5"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["MATERIALTYPEID6"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["OTHERTYPEID"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["QUANTITY"] + "</th>";
        strTableTrHtml += "<th>" + dt[i].data["REMARK"] + "</th>";

        strTableTrHtml += "</tr>";

    }
    var LODOP = getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
    var strTableEndHtml = "</table>";
    var htmlStr = strHead + strTableStartHtml + strTableTheadHtml + strTableTrHtml + strTableEndHtml;


    LODOP.PRINT_INITA(0, 0, "100%", "100%", "打印控件功能演示_Ext");
    LODOP.ADD_PRINT_HTM(1, 1, "100%", "100%", htmlStr); //ADD_PRINT_HTM(Top,Left,Width,Height,strHtmlContent)

    LODOP.SET_PRINT_PAGESIZE(1, 0, 0, "A4"); //1---纵(正)向打印  2---横向打印 SET_PRINT_PAGESIZE(intOrient,intPageWidth,intPageHeight,strPageName)设定纸张大小

    LODOP.PREVIEW();

}

