plsMaterialStockTaskVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
};
var proto = plsMaterialStockTaskVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = plsMaterialStockTaskVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnPrint") {
                var data = this.dataSet.getTable(0).getRange();
                var coll = new Ext.util.MixedCollection();
                var midData = new Array();
                for (var i = 0; i < data.length; i++)
                {
                    var key = data[i].data["WORKSHOPSECTIONID"] + "/t" + data[i].data["WORKNO"] + "/t" + data[i].data["PRODUCTID"] + "/t" + data[i].data["WAREHOUSEPERSONID"];
                    if (coll.containsKey(key)) {
                        coll.get(key).push(data[i].data);
                        //for (var i = 0; i < coll.get(key).length ; i++) {
                        //    midData.push(coll.get(key)[i]);
                        //}
                    }
                    else {
                        midData.push(data[i].data);
                        coll.add(key, midData);
                        midData = [];
                    }
                }
                function prn_Preview() {
                    if (coll.keys.length < 100) {
                        for (var i = 0; i < coll.keys.length; i++) {
                            CreatePrintPage(coll.get(coll.keys[i]));
                            // LODOP.PREVIEW(); //打印预览
                            LODOP.PRINT_DESIGN();
                            // LODOP.PRINT();

                        }
                    }
                    else {
                        var num = coll.keys.length;
                        var k = 0;
                        while (num > 100)
                        {
                            for (var i = k * 100; i < 100*(k+1); i++)
                            {
                                CreatePrintPage(coll.get(coll.keys[i]));
                                // LODOP.PREVIEW(); //打印预览
                                LODOP.PRINT_DESIGN();
                                // LODOP.PRINT();
                            }
                            num = num - 100;
                            k++;
                        }
                        for (var i = k * 100; i < coll.keys.length; i++)
                        {
                            CreatePrintPage(coll.get(coll.keys[i]));
                            // LODOP.PREVIEW(); //打印预览
                            LODOP.PRINT_DESIGN();
                            // LODOP.PRINT();
                        }
                    }
                  
                };
                function CreatePrintPage(items) {
                    LODOP = getLodop(document.getElementById('LODOP'), document.getElementById('LODOP_EM'));
                    LODOP.PRINT_INITA(0, 0,"100%", "100%", "打印控件功能演示_Ext");
                    AddText(items);
                    LODOP.ADD_PRINT_TEXT(3, "80%", 170, 20, "总页号：第#页/共&页");
                    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
                    LODOP.SET_PRINT_STYLEA(0, "ItemType", 2);//页号项
                    LODOP.SET_PRINT_STYLEA(0, "Horient", 1); //右边距锁定
                    //LODOP.ADD_PRINT_TEXT(3, 34, 196, 20, "总页眉：美的MES系统");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
                    //LODOP.SET_PRINT_STYLEA(0, "ItemType", 1);//页眉页脚
                }
                function longTodateTime(longTime)   
                {
                    longTime = longTime.toString();
                    var time = "";
                    if(longTime >0)
                    {
                        return longTime.substr(0, 4) + "-" + longTime.substr(4, 2) + "-" + longTime.substr(6, 2) + " " + longTime.substr(8, 2) + ":" + longTime.substr(10, 2) +":"+ longTime.substr(12,2);
                    }
                    return time;
                }
                function AddText(items) {
                    var strHead = "<DIV style='LINE-HEIGHT: 30px' class=size16 align=center><STRONG><font size = '6px'>备料计划物料清单</font></STRONG></DIV>";
                    strHead += "<TABLE border=0 cellSpacing=0 cellPadding=0 width='100%'>";
                    strHead += "<TBODY>";

                    strHead += "<TR>";
                    strHead += "<TD width ='40%'><font   size = '4px'>作业号：<SPAN>" + items[0]["WORKNO"] + "</SPAN></font></TD>";
                    strHead += "<TD width ='30%'><font   size = '4px'>工段名称：<SPAN >" + items[0]["WORKSHOPSECTIONNAME"] + "</SPAN></font></TD>";
                    strHead += "<TD colspan =2><font  size = '4px'>仓管员：<SPAN> " + items[0]["WAREHOUSEPERSONNAME"] + "</SPAN></font></TD>";
                    strHead += "</TR>";

                    strHead += "<TR>";
                    strHead += "<TD colspan =2> <font size = '4px'>产品名称：<SPAN >" + items[0]["PRODUCTNAME"] + "</SPAN></font></TD> ";
                    strHead += "</TR>";

                    strHead += "<TR>";
                    strHead += "<TD colspan =2><font  size = '4px'>工段条码：<SPAN><div id = materialIdBarcode></SPAN></font><font color='#0000FF'></font></TD>";
                    strHead += "</TR>";

                    strHead += "<TR>";
                    strHead += "<TD ><font size = '4px' >计划备料开始时间：<SPAN>" + longTodateTime(items[0]["STOCKSTARTTIME"]) + "</SPAN></font></TD>";
                    strHead += "</TR>";
                    strHead += "</TBODY></TABLE>";
                    //LODOP.ADD_PRINT_TEXT(25, "34.75%", 355, 30, "备料计划物料清单");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 20);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(60, 0, 103, 30, "作业号：");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(60, "13.63%", 296, 30, "备料计划物料清单");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(59, "51%", 115, 30, "工段名称：");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(59, "66.75%", 256, 30, "备料计划物料清单");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(177, "0%", 191, 30, "计划备料开始时间：");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(177, "23.75%", 195, 30, "2014-06-27 10:10");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(98, 0, 111, 30, "产品名称：");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(98, "13.63%", 288, 30, "备料计划物料清单");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(98, "51.13%", 115, 30, "工段条码：");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(97, "66.88%", 252, 30, "条码");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(177, "48.75%", 201, 30, "计划备料结束时间：");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(177, "73.75%", 195, 30, "2014-06-27 10:10");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 15);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(136, 0, 95, 30, "仓管员：");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(136, "12.38%", 142, 30, "cgy");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(136, "31.75%", 111, 30, "配送地址：");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
                    //LODOP.ADD_PRINT_TEXT(135, "46%", 419, 30, "ps");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 14);
                    //LODOP.SET_PRINT_STYLEA(0, "Bold", 1);


                    //设置表格样式
                    var strTableStyle = "<style type='text/css'>table{width:'100%';border-collapse: collapse;} table thead td b{font-size: 25px;} table tr td{font-size: 13px;} table tfoot td{font-size: 15px;}</style>";
                    //将数据拼成一个table  
                    var strTableStartHtml = "<table border='1' width='100%' bordercolor='#336699' cellpadding='0' cellspacing='0' align='center'>";
                    var strTableEndHtml = "</table>";
                    var strTableTheadHtml = "<thead style='height: 30px' bgcolor='#efefef'>";
                    var strTableTrHtml = "";
                    strTableTheadHtml += "<td nowrap align='center' style=font-size: 15px><b>序号</b></td>";
                    strTableTheadHtml += "<td nowrap align='center' style=font-size: 15px><b>物料编码</b></td>";
                    strTableTheadHtml += "<td nowrap align='center' style=font-size: 15px><b>物料名称</b></td>";
                    strTableTheadHtml += "<td nowrap align='center' style=font-size: 15px width = '120px'><b>物料条码</b></td>";
                    strTableTheadHtml += "<td nowrap align='center' style=font-size: 15px width = '120px'><b>需备料数量</b></td>";
                    strTableTheadHtml += "</thead>";
                    var zjeTotal = 0;
                    for (var i = 0; i < items.length; i++) {
                        var td = "<tr style='height: 60px'><td align='center'>";
                        td += i+1;
                        td += "</td><td align='center'>";
                        td += items[i]["MATERIALID"];
                        td += "</td><td align='center'>";
                        td += items[i]["MATERIALNAME"];
                        td += "</td><td align='center'><div id = materialIdBarcode" + i + "></div>";
                        td += "</td><td align='center'>";
                        td += items[i]["STOCKQUANTITY"];
                        td += "</td></tr>";
                        strTableTrHtml += td;
                    }
                    var strTableTfoot = "<tr style='height: 30px'><td align='center'><b>合计</b></td><td>&nbsp;</td><td>&nbsp;</td><td align='right'><b>测试</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>";
                    var strPageFooter = "<tfoot style='height: 30px'><td align='center'><b>本页合计</b></td><td tdata='pageNO' format='#' align='center'><p align='center'><b>第<font color='#0000FF'>#</font>页</b></p></td><td tdata='pageCount' format='#' align='center'><p align='center'><b>总<font color='#0000FF'>#</font>页</b></td><td>&nbsp;</td><td>&nbsp;</td></tfoot>";
                    $("#printBarcode").append(strTableStartHtml + strTableTheadHtml + strTableTrHtml + strPageFooter + strTableEndHtml);
                    $("#printBarcodeTitle").append(strHead);
                    $("#materialIdBarcode").barcode(items[0]["WORKSHOPSECTIONID"], "code128", { barWidth: 2, barHeight: 40, showHRI: false });
                    for (var i = 0; i < items.length; i++) {
                        $("#materialIdBarcode" + i).barcode(items[i]["MATERIALID"], "code128", { barWidth: 1, barHeight: 40, showHRI: false });
                    }
                    LODOP.ADD_PRINT_HTM(200, 0, 800, 1000, strTableStyle);
                    LODOP.ADD_PRINT_TABLE(215, 0, "100%", "85%", $("#printBarcode")[0].innerHTML);
                    LODOP.ADD_PRINT_HTM(20, 0, 800, 1000, $("#printBarcodeTitle")[0].innerHTML);
                    LODOP.SET_PRINT_STYLEA(0, "ItemType", 1);
                    LODOP.SET_PRINT_STYLEA(0, "LinkedItem", 1000);
                    $("#printBarcode")[0].innerHTML = "";
                    $("#printBarcodeTitle")[0].innerHTML = "";
                    
                   // LODOP.NewPageA();
                };
                prn_Preview();
            }
            break;
    }
}
