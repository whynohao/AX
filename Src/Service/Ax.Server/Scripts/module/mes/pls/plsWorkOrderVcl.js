plsWorkOrderVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = plsWorkOrderVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = plsWorkOrderVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == "FROMBILLNO") {
                    var progId = "pls.SalesOrder";
                    var billNo = e.dataInfo.dataRow.data["FROMBILLNO"];//来源单号
                    Ax.utils.LibVclSystemUtils.openBill(progId, 1, "销售订单", BillActionEnum.Modif, this.entryParam, [billNo]);
                }
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            switch (e.dataInfo.fieldName) {
                case "BtnPaint":
                    var headTable = this.dataSet.getTable(0).data.items[0];
                    if (headTable.data["PARENTBILLNO"] == '') {
                        printTwo(this, false);
                    }
                    else {
                        printOne(this, false);
                    }
                    break;
            }
            break;
    }
}

//获取单据表头日期年月日
function ShortTodateTime(longTime) {
    longTime = longTime.toString();
    var time = "";
    if (longTime > 0) {
        return longTime.substr(4, 2) + "月" + longTime.substr(6, 2) + "日"
    }
    return time;
}

function ShortTodateTimeTwo(longTime) {
    longTime = longTime.toString();
    var time = "";
    if (longTime > 0) {
        return longTime.substr(0, 4) + "年" + longTime.substr(4, 2) + "月" + longTime.substr(6, 2) + "日"
    }
    return time;
}

//获取油漆类型
function GetPaintType(attributeDesc) {
    var paintType;
    //var regex = "【";
    //attributeDesc = attributeDesc.replacea(new RegExp(regex, "gm"), "");
    var descList = attributeDesc.split("】");
    for (var i = 0; i < descList.length; i++) {
        if (descList[i].indexOf("油漆类型") > 0) {
            paintType = descList[i].replace("【油漆类型:", "");
        }
    }
    return paintType;
}

//打印功能
proto.print = function (headTable, bodyTable) {
    headTable = this.dataSet.getTable(0).data.items[0];
    if (headTable.data["PARENTBILLNO"] == '') {
        printTwo(this,true);
    }
    else {
        printOne(this, true);
    }
}

function printOne(This,isPaint) {
    //debugger;
    headTable = This.dataSet.getTable(0).data.items[0];
    bodyTable = This.dataSet.getTable(1);
    var billNo = This.dataSet.getTable(0).data.items[0].data['BILLNO'];
    var startDate = This.dataSet.getTable(0).data.items[0].data['STARTDATE'];
    var fromBillNo = headTable.data["PARENTBILLNO"];

    //构建表身
    var strTableTrHtml = "";
    var headWorkIndex = headTable.data["WORKINDEX"];
    var productIndex = 1;
    var secondsNum = 0;//秒数
    var orderQuantity = 0;//总计
    var produceLineId = headTable.data["PRODUCELINEID"];
    var produceLineName = headTable.data["PRODUCELINENAME"];
    var startDate = headTable.data['STARTDATE'];
    var lotNoList;

    var bodyTable = This.dataSet.getTable(1).data.items;
    var list = new Array();
    for (var i = 0; i < bodyTable.length; i++) {
        list.push({
            'ROW_ID': i,
            'BILLNO': bodyTable[i].data['BILLNO'],
            'FROMBILLNO': bodyTable[i].data['FROMBILLNO'],
            'CUSTOMERID': bodyTable[i].data['CUSTOMERID'],
            'GROUPNO': "1",
            'CUSTOMERNAME': bodyTable[i].data['CUSTOMERNAME'],
            'LOTNO': bodyTable[i].data['LOTNO'],
            //'MATERIALID': bodyTable[i].data['MATERIALID'],
            'MATERIALNAME': bodyTable[i].data['MATERIALNAME'],
            "GRIDNUM": "1",
            //'ATTRIBUTECODE': bodyTable[i].data['ATTRIBUTECODE'],
            'ATTRIBUTEDESC': bodyTable[i].data['ATTRIBUTEDESC'],
            'ORDERQUANTITY': bodyTable[i].data['ORDERQUANTITY'],
            'SECONDSNUM': bodyTable[i].data['SECONDSNUM'],
            'PRODUCTINDEX': bodyTable[i].data['PRODUCTINDEX']
        });
    }


    var info = This.invorkBcf("CombinatePrintSentence", [list, headWorkIndex, produceLineId, startDate, fromBillNo]);

    strTableTrHtml += info.StrTableTrHtml;
    secondsNum = info.SecondsNum;
    orderQuantity = info.OrderQuantity;
    //添加一行作为秒数和总计的累计数量
    var td = "<tr>";
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";//十单号
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";//生产单号
    td += "<td nowrap align='center' ><font size = '2px' face='宋体'></font></td>";//经销商名称
    td += "<td nowrap align='center'><font size = '2px' face='宋体'></font></td>";//序号
    td += "<td nowrap align='center'><font size = '2px' face='宋体'></font></td>";//组号
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";//尺寸
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";//树种
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";//油漆颜色
    //td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";//门格数
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";//型号
    td += "<td nowrap align='center'><font size = '1px' face='宋体'>" + secondsNum + "</font></td>";//秒数
    td += "<td nowrap align='center'><font size='2px' face='宋体'>" + orderQuantity + "</font></td>";//总计
    td += "<td nowrap align='center'><font size = '2px' face='宋体'></font></td>";//属性
    td += "<td nowrap align='center'><font size = '2px' face='宋体'></font></td>";//完成确认
    td += "<td nowrap align='center'><font size = '2px' face='宋体'></font></td>";//上道签字
    td += "<td nowrap align='center'><font size = '2px' face='宋体'></font></td>";//下道签字
    td += "</tr>";
    strTableTrHtml += td;

    var strTableEndHtml = "</table>";





    var tenBillNum = headTable.data["WORKINDEX"];//第几个十单
    //var tenBillNum = 1;//第几个十单
    var billNoStr = "共计:" + info.count + "个十单 单号:" + tenBillNum;//作业顺序号
 
    var boardStr = produceLineName + "生产看板(" + ShortTodateTime(headTable.get("STARTDATE")) + "第" + tenBillNum + "个十单)";
    var myDate = new Date();
    var year = myDate.getFullYear();
    var realProductDateStr = "实际生产时间：  月   日";

    var planFinishDateStr = "计划完成时间：" + info.Time + "分钟";
    var imagePath = "<img src=./Scripts/desk/images/HYMYmark.jpg height=60 width='70%'>";//./img/bg.jpg
    var billCount = 1;//共几个十单WORKINDEX
    var billAllCountStr = "共" + billCount + "个十单";
    var strboardHead = "<div align=center><strong><font size = '5px' face='宋体' top=6cm>  " + boardStr + "</font></strong></div>";
    var strboardHead = boardStr;
    var strBillCountHead = "<div align=center top=10cm><strong><font size = '4px' face='宋体'> " + billAllCountStr + "</font></strong></div>";
    var strTableStartHtml = "<table border='1'id='theTable' top=10cm width='100%' bordercolor='#336699' cellpadding='0' cellspacing='0' align='center' >";


    var strTableTheadHtml = "<thead style='height: 38px' face='宋体' bgcolor='#efefef'>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>十单号</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>生产单号</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>经销商</font></td>";//nowrap自动换行
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>序号</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>组号</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>尺寸</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>树种</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>油漆颜色</font></td>";
    //strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>门格数</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>&nbsp&nbsp型号&nbsp&nbsp</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>秒数</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>总计</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>属性</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>备注</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>上道签字</font></td>";
    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>下道签字</font></td>";
    strTableTheadHtml += "</thead>";

    var LODOP = getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
    LODOP.ADD_PRINT_TEXT(40, 175, 650, 40, strboardHead);//标题（生产看板）
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 17);
    //LODOP.ADD_PRINT_IMAGE(29, 98, 84, 35, imagePath);//商标
    LODOP.ADD_PRINT_IMAGE(2, 4, "20%", "40%", imagePath);//商标
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 15);
    //LODOP.ADD_PRINT_SHAPE(2, 37, 69, 18, 15, 0, 1, "#0000FF");
    LODOP.ADD_PRINT_SHAPE(2, 37, 69, 18, 15, 0, 1);
    //LODOP.ADD_PRINT_TEXT(70, 60, 117, 20, billAllCountStr);//共几个单
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 15);
    LODOP.SET_PRINT_STYLEA(0, "Alignment", 2);
    LODOP.ADD_PRINT_TEXT(77, 1, 250, 20, billNoStr);//单号
    LODOP.SET_PRINT_STYLEA(0, "Alignment", 2);
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
    LODOP.ADD_PRINT_TEXT(77, 505, 250, 25, realProductDateStr);//实际生产时间
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
    LODOP.SET_PRINT_STYLEA(0, "Alignment", 2);//左右对齐属性
    LODOP.ADD_PRINT_TEXT(77, 261, 190, 20, planFinishDateStr);//计划完成时间
    LODOP.SET_PRINT_STYLEA(0, "Alignment", 2);
    LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);

    //LODOP.PRINT_INITA(0, 0, "100%", "100%", "打印控件功能演示_Ext");


    //var html2Str = strTableS2tartHtml + strTable2TheadHtml + strTable2TrHtml + strTable2EndHtml;
    //LODOP.ADD_PRINT_HTM(130, 1, "100%", "100%", html2Str); //ADD_PRINT_HTM(Top,Left,Width,Height,strHtmlContent)
    //var table = document.getElementById("theTable1");

    //MergeCellsVertical(table, 1);
    //var htmlStr = strboardHead + strTableStartHtml + strTableTheadHtml + strTableTrHtml + strTableEndHtml;
    var htmlStr = strTableStartHtml + strTableTheadHtml + strTableTrHtml + strTableEndHtml;
    LODOP.ADD_PRINT_HTM(94, 1, "100%", "100%", htmlStr); //ADD_PRINT_HTM(Top,Left,Width,Height,strHtmlContent)

    LODOP.SET_PRINT_PAGESIZE(1, 0, 0, "A4"); //1---纵(正)向打印  2---横向打印 SET_PRINT_PAGESIZE(intOrient,intPageWidth,intPageHeight,strPageName)设定纸张大小

    if (isPaint) {
        LODOP.PRINT();
    }
    else {
        LODOP.PREVIEW();
    }
    
}

function printTwo(This, isPaint) {
    //debugger;
    headTable = This.dataSet.getTable(0).data.items[0];
    bodyTable = This.dataSet.getTable(1);
    var startDate = headTable.data['STARTDATE'];
    var billno = headTable.data["BILLNO"];
    var tenBillNum = headTable.data["WORKINDEX"];//第几个十单
    var produceLineId = headTable.data["PRODUCELINEID"];
    var produceLineName = headTable.data["PRODUCELINENAME"];
    var beatTime = "60";
    var info = This.invorkBcf("PrintTwo", [billno, produceLineId, tenBillNum, startDate, beatTime]);

    var myDate = new Date();
    var year = myDate.getFullYear();
    var boardStr = produceLineName + ShortTodateTimeTwo(headTable.get("STARTDATE")) + "生产看板";//

    var realProductDateStr = "实际生产日期：&nbsp&nbsp&nbsp月&nbsp&nbsp&nbsp日&nbsp&nbsp&nbsp";
    var billNoStr = "单号：&nbsp" + tenBillNum + "&nbsp&nbsp";//作业顺序号
    var planFinishDateStr = "计划完成时间：" + info.Time + "&nbsp分钟  ";
    var billAllCountStr = "共" + info.count + "个十单&nbsp&nbsp";
    var imagePath = "<img src=./Scripts/desk/images/HYMYmark.jpg height=60 width='100%'>";//
    var strboardHead = "";
    var strTableStartHtml = "<table border='1'id='theTable' top=10cm width='100%'  bordercolor='#336699' cellpadding='0' cellspacing='0' align='center' >";


    var tr = "<tr >";
    tr += "<th><font size = '1px' face='宋体'>门扇</font></th>";
    tr += "<th><font size = '1px' face='宋体'>门套</font></th>";
    tr += "<th><font size = '1px' face='宋体'>柜门</font></th>";
    tr += "<th><font size = '1px' face='宋体'>筒子板</font></th>";
    tr += "<th><font size = '1px' face='宋体'>贴脸线</font></th>";
    tr += "<th><font size = '1px' face='宋体'>踢脚线</font></th>";
    tr += "<th  nowrap width=50><font size = '1px' face='宋体'>其他</font></th>";
    //tr += "<th><font size = '1px' face='宋体'>门格数</font></th>";
    tr += "</tr>";


    var strTableTheadHtml = "";
    strTableTheadHtml += "<tr>";
    strTableTheadHtml += "<td border-right ='0' pack='left' border-right='#ff0000' colspan=2 >" + imagePath;
    strTableEndHtml += "</td>";
    strTableTheadHtml += "<td  border-left ='0' colspan=20  align='center' ><font size = '5px' face='宋体'>" + boardStr;
    strTableEndHtml += "</td>";
    strTableTheadHtml += "</tr>";

    strTableTheadHtml += "<tr>";
    strTableTheadHtml += "<th colspan=23 height=30><font size = '4px' face='宋体'>" + billAllCountStr + billNoStr + realProductDateStr + planFinishDateStr + "</th>";
    strTableTheadHtml += "</tr>";

    strTableTheadHtml += "<tr>";
    strTableTheadHtml += "<th align='center' rowspan='2' height=30><font size = '2px' face='宋体'>十单号</font></th>";
    strTableTheadHtml += "<th nowrap align='center' rowspan='2'><font size = '2px' face='宋体'>物流线</font></th>";//nowrap自动换行
    strTableTheadHtml += "<th align='center' rowspan='2' width=60><font size = '2px' face='宋体'>订货单位</font></th>";
    //strTableTheadHtml += "<th align='center' rowspan='2'><font size = '1px' face='宋体'>订单编号</font></th>";
    strTableTheadHtml += "<th align='center' rowspan='2'><font size = '2px' face='宋体'>树种</font></th>";
    //strTableTheadHtml += "<th align='center' rowspan='2'><font size = '1px' face='宋体'>油漆颜色</font></th>";
    strTableTheadHtml += "<th align='center' rowspan='2'><font size = '2px' face='宋体'>油漆</font></th>";
    strTableTheadHtml += "<th nowrap align='center' rowspan='2' width=60><font size = '2px' face='宋体'>产品系列</font></th>";
    strTableTheadHtml += "<th align='center' colspan='7'><font size = '2px' face='宋体'>明细</font></th>";
    strTableTheadHtml += "<th align='center' rowspan='2' width=90><font size = '2px' face='宋体'>要求出货时间</font></th>";
    strTableTheadHtml += "<th align='center' rowspan='2'><font size = '2px' face='宋体'>批号</font></th>";
    strTableTheadHtml += "<th align='center' rowspan='2' width=100><font size = '2px' face='宋体'>PMC部下单时间</font></th>";
    strTableTheadHtml += "<th align='center' rowspan='2' width=120 colspan='7'><font size = '2px' face='宋体'>备注</font></th>";
    //strTableTheadHtml += "<th align='center' rowspan='2'><font size = '1px' face='宋体'>PMC部接单时间</font></th>";



    strTableTheadHtml += "</tr>";
    strTableTheadHtml += tr;


    var LODOP = getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
    //LODOP.ADD_PRINT_TEXT(30, 400, 500, 30, strboardHead);//标题（生产看板）
    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 15);
    //LODOP.ADD_PRINT_IMAGE(2, 5, "24%", "50%", imagePath);//商标
    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 15);
    //LODOP.ADD_PRINT_SHAPE(2, 37, 69, 18, 15, 0, 1);
    //LODOP.ADD_PRINT_TEXT(70, 60, 117, 20, billAllCountStr);//共几个单
    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 12);
    //LODOP.SET_PRINT_STYLEA(0, "Alignment", 2);
    //LODOP.ADD_PRINT_TEXT(98, 46, 100, 20, billNoStr);//单号
    //LODOP.SET_PRINT_STYLEA(0, "Alignment", 3);
    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 13);
    //LODOP.ADD_PRINT_TEXT(68, 700, 350, 25, realProductDateStr);//实际生产时间
    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 15);
    //LODOP.SET_PRINT_STYLEA(0, "Alignment", 2);//左右对齐属性
    //LODOP.ADD_PRINT_TEXT(98, 800, 200, 20, planFinishDateStr);//计划完成时间
    //LODOP.SET_PRINT_STYLEA(0, "Alignment", 3);
    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 12);

    //LODOP.PRINT_INITA(0, 0, "100%", "100%", "打印控件功能演示_Ext");

    //构建表身
    var strTableTrHtml = "";

    strTableTrHtml += info.StrTableTrHtml;
    var td = "<tr>";
    td += "<td nowrap align='center' height=30><font size='2px' face='宋体'></font></td>";
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";
    td += "<td nowrap align='center'><font size='2px' face='宋体'>总计：</font></td>";
    td += "<td nowrap align='center'><font size='2px' face='宋体'>" + info.Menquantity + "</font></td>";
    td += "<td nowrap align='center'><font size='2px' face='宋体'>" + info.Mentaoquantity + "</font></td>";
    td += "<td nowrap align='center'><font size='2px' face='宋体'>" + info.Guimenquantity + "</font></td>";
    td += "<td nowrap align='center'><font size='2px' face='宋体'>" + info.Tongzibanquantity + "</font></td>";
    td += "<td nowrap align='center'><font size='2px' face='宋体'>" + info.Tielianquantity + "</font></td>";
    td += "<td nowrap align='center' ><font size='2px' face='宋体'>" + info.Tijiaoxianquantity + "</font></td>";
    td += "<td nowrap width=50 align='center' ><font size='2px' face='宋体'>" + info.Otherquantity + "</font></td>";
    //td += "<td nowrap align='center'><font size='2px' face='宋体'>" + info.Otherquantity + "</font></td>";
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";//
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";//
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";//
    td += "<td nowrap align='center'><font size='2px' face='宋体'></font></td>";//
    td += "</tr>";
    strTableTrHtml += td;
    //添加一行作为秒数和总计的累计数量

    var strTableEndHtml = "</table>";

    var htmlStr = strTableStartHtml + strTableTheadHtml + strTableTrHtml + strTableEndHtml;
    LODOP.ADD_PRINT_HTM(20, 1, "100%", "100%", htmlStr); //ADD_PRINT_HTM(Top,Left,Width,Height,strHtmlContent)
    //LODOP.ADD_PRINT_HTM(130, 1, "100%", "100%", htmlStr); //ADD_PRINT_HTM(Top,Left,Width,Height,strHtmlContent)

    LODOP.SET_PRINT_PAGESIZE(2, 0, 0, "A4"); //1---纵(正)向打印  2---横向打印 SET_PRINT_PAGESIZE(intOrient,intPageWidth,intPageHeight,strPageName)设定纸张大小


    if (isPaint) {
        LODOP.PRINT();
    }
    else {
        LODOP.PREVIEW();
    }

}


//取当前时间
function GetNowFormatDate(date) {
    //var date = new Date();
    var seperator1 = "-";
    var seperator2 = ":";
    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    var strDate = date.getDate();
    if (month >= 1 && month <= 9) {
        month = "0" + month;
    }
    if (strDate >= 0 && strDate <= 9) {
        strDate = "0" + strDate;
    }
    var currentdate = year + "年" + month + "月" + strDate + "日";
    return currentdate;
}





