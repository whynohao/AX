plsProducePlanRptVcl = function () {
    Ax.vcl.LibVclRpt.apply(this, arguments);
};
var proto = plsProducePlanRptVcl.prototype = Object.create(Ax.vcl.LibVclRpt.prototype);
proto.constructor = plsProducePlanRptVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclRpt.prototype.vclHandler.apply(this, arguments);
}

//获取单据日期月日  XX月XX日
function ShortTodate(longTime) {
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

    var planDate = dt[0].data["PLANDATE"];
    var strHead = "<div style='margin-bottom:10px' align=center><strong><font size = '5px'> " + ShortTodate(planDate) + "生产计划报表</font></strong></div>";
    var strTableStartHtml = "<table border='1' width='70%' bordercolor='#336699' cellpadding='0' cellspacing='0' align='center'>";


    var strTableTheadHtml = "";
    strTableTheadHtml += "<tr>";
    strTableTheadHtml += "<th><b><font size = '4px' face='黑体'>产品型号</font></b></th>";
    strTableTheadHtml += "<th><b><font size = '4px' face='黑体'>油漆颜色</font></b></th>";
    strTableTheadHtml += "<th><b><font size = '4px' face='黑体'>数量</font></b></th>";
    strTableTheadHtml += "</tr>";

    //构建表身
    var strTableTrHtml = "";
    for (var i = 0; i < dt.length; i++) {
        strTableTrHtml += "<tr>";
        strTableTrHtml += "<th><font size = '3px' face='宋体'>" + dt[i].data["MATERIALNAME"] + "</font></th>";
        strTableTrHtml += "<th><font size = '3px' face='宋体'>" + dt[i].data["PAINTCOLOR"] + "</font></th>";
        strTableTrHtml += "<th><font size = '3px' face='宋体'>" + dt[i].data["QUANTITY"] + "</font></th>";
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

