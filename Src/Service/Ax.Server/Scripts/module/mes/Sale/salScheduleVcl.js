salScheduleVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
}
var proto = salScheduleVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = salScheduleVcl;
proto.doSetParam = function () {
    if (this.billAction == BillActionEnum.AddNew) {
        var mastRow = this.dataSet.getTable(0).data.items[0];
        var num = mastRow.get("VERSIONNUM");
        mastRow.set("VERSIONNUM", num + 1);
        this.forms[0].loadRecord(mastRow);
    }
}
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    if (e.libEventType == LibEventTypeEnum.Validated) {
        if (e.dataInfo && e.dataInfo.tableIndex == 0) {
            if (e.dataInfo.fieldName == "FROMBILLNO") {
                var mast = this.dataSet.getTable(0);
                var body = this.dataSet.getTable(1);
                if (e.dataInfo.value == null) {
                    body.removeAll();
                }
                else {
                    for (var i = 0; i < body.data.length; i++) {
                        body.data.items[i].set("FROMBILLNO", e.dataInfo.value);
                    }
                }
            }
        }
        if (e.dataInfo && e.dataInfo.tableIndex == 1) {
            if (e.dataInfo.fieldName == "AMOUNT") {
                var bodyTable = this.dataSet.getTable(1);
                var sum = e.dataInfo.value;
                for (var i = 0; i < bodyTable.data.length; i++) {
                    if (e.dataInfo.dataRow.get("ROW_ID") != bodyTable.data.items[i].get("ROW_ID")) {
                        sum += bodyTable.data.items[i].get("AMOUNT");
                        console.info(sum);
                    }
                }
                var headTable = this.dataSet.getTable(0);
                headTable.data.items[0].set("AMOUNT", sum);
                this.forms[0].loadRecord(headTable.data.items[0]);
            }
            if (e.dataInfo.fieldName == "SCHEDULEPRICE") {
                var bodyTable = this.dataSet.getTable(1);
                var sun = e.dataInfo.value;
                for (var i = 0; i < bodyTable.data.length; i++) {
                    if (e.dataInfo.dataRow.get("ROW_ID") != bodyTable.data.items[i].get("ROW_ID")) {
                        sun += bodyTable.data.items[i].get("SCHEDULEPRICE");
                        console.info(sun);
                    }
                }
                var headTable = this.dataSet.getTable(0);
                headTable.data.items[0].set("OFFERAMOUNT", sun);
                this.forms[0].loadRecord(headTable.data.items[0]);
            }
        }
         if (e.dataInfo && e.dataInfo.tableIndex == 2) {
            if (e.dataInfo.fieldName == "PRICE") {
                var bodyTable = this.dataSet.getTable(1);
                var bodyDetailTable = this.dataSet.getTable(2);
                var sum = e.dataInfo.value;
                var rowId = e.dataInfo.dataRow.get("PARENTROWID")
                for (var i = 0; i < bodyDetailTable.data.length; i++) {
                    if (e.dataInfo.dataRow.get("PARENTROWID") == bodyTable.data.items[rowId-1].get("ROW_ID") && e.dataInfo.dataRow.get("ROW_ID") != bodyDetailTable.data.items[i].get("ROW_ID")) {
                        sum += bodyDetailTable.data.items[i].get("PRICE");
                    }
                }
                bodyTable.data.items[rowId-1].set("AMOUNT", sum);
                this.forms[0].loadRecord(bodyTable.data.items[0]);
            }
        }
    }
    else if (e.libEventType == LibEventTypeEnum.AddRow)
    {
        if (e.dataInfo.tableIndex == 1) {
            var mastRow = this.dataSet.getTable(0).data.items[0];
            var billno = mastRow.get("FROMBILLNO");
            e.dataInfo.dataRow.set("FROMBILLNO", billno);
        }
    }
    else if (e.libEventType == LibEventTypeEnum.ButtonClick) {
         if (e.dataInfo.fieldName == "ScheduleOut") {
           if (!this.isEdit) {
                var List = [];
                var headTableRow = this.dataSet.getTable(0).data.items[0];
                var head = headTableRow.data["CONTACTSOBJECTID"];
                var proName = headTableRow.data["PROJECTNAME"];
                var billNo = headTableRow.data["BILLNO"];
                List.push({
                    ContactPersonName: headTableRow.data["CONTACTPERSONNAME"],
                    ConPhone: headTableRow.data["CONPHONEN"],
                    SkillPersonName: headTableRow.data["SKILLPERSONNAME"],
                    SkillPhone: headTableRow.data["SKILLPHONENO"],
                })
                //var field = this.invorkBcf('ScheduleOutput', [head, billNo, proName, List]);
                window.open("WebForm1.aspx?head=" + head + "&&billNo=" + billNo + "&&proName=" + proName);
                if (field == null) { Ext.alert.alert("导出提示", "导出的数据为空！"); }
                else {
                    //if (field && field !== '') {
                    //    browseFolder(field);
                    //}
                }
            }
            else { Ext.Msg.alert("系统提示", "编辑状态不能生成单据！"); }
        }
    }
}
//function browseFolder(path) {
//    try {
//        var Message = "\u8bf7\u9009\u62e9\u6587\u4ef6\u5939"; //选择框提示信息
//        var Shell = new ActiveXObject("Shell.Application");
//        var Folder = Shell.BrowseForFolder(0, Message, 64, 17); //起始目录为：我的电脑
//        //var Folder = Shell.BrowseForFolder(0, Message, 0); //起始目录为：桌面
//        if (Folder != null) {
//            Folder = Folder.items(); // 返回 FolderItems 对象
//            Folder = Folder.item(); // 返回 Folderitem 对象
//            Folder = Folder.Path; // 返回路径
//            if (Folder.charAt(Folder.length - 1) != "\\") {
//                Folder = Folder + "\\";
//            }
//            document.getElementById(path).value = Folder;
//            return Folder;
//        }
//    }
//    catch (e) {
//        alert(e.message);
//    }
//}
//function html_encode(str)
//{
//    var s = "";
//    if (str.length == 0) return "";
//    s = str.replace(/&/g, ">");
//    s = s.replace(/</g, "<");
//    s = s.replace(/>/g, ">");
//    s = s.replace(/ /g, " ");
//    s = s.replace(/\'/g, "'");
//    s = s.replace(/\"/g, "\"");
//    s = s.replace(/\n/g, "<br>");
//    return s;
//}
//function printSchedule(This, isPaint, returnList,Rmb) {

//    //构建表身
//    var strTableTrHtml = "";

//    var strTableEndHtml = "</table>";

   

//    var strTableStartHtml = "<table border='1' id='theTable' width='100%'   bordercolor='#000000' cellpadding='0' cellspacing='0' align='center' >";

//    var reMark = "<thead style='height: 33px' face='宋体' bgcolor='#FFFFFF'>";

//    reMark += "<td nowrap align='left' colspan =10 ><font size='3px' face='宋体'><strong>" + "非常感谢贵公司的询价，根据您的要求，现将产品报价如下：" + "</strong></font></td>";
//    reMark += "</thead>";
//    var strTableTheadHtml = "<tr style='height: 38px' face='宋体' bgcolor='#FFFFFF'>";
//    strTableTheadHtml += "<th nowrap align='center' rowspan=2><font size = '2px' face='宋体'>序号</font></th>";
//    strTableTheadHtml += "<th nowrap align='center' rowspan=2><font size = '2px' face='宋体'>仪表位号</font></th>";
//    strTableTheadHtml += "<th nowrap align='center' rowspan=2><font size = '2px' face='宋体'>阀门型式</font></th>";//nowrap自动换行
//    strTableTheadHtml += "<th nowrap align='center' rowspan=2><font size = '2px' face='宋体'>类型</font></th>";
//    strTableTheadHtml += "<th nowrap align='center' rowspan=2><font size = '2px' face='宋体'>型号规格(CM)</font></th>";
//    strTableTheadHtml += "<th nowrap align='center' rowspan=2><font size = '2px' face='宋体'>数量</font></th>";
//    strTableTheadHtml += "<th nowrap align='center' colspan=2><font size = '2px' face='宋体'>现场交货价（含税）</font></th>";
//    strTableTheadHtml += "<th nowrap align='center' rowspan=2><font size = '2px' face='宋体'>备注</font></th>";
//    strTableTheadHtml += "</tr>";

//    strTableTheadHtml += "<tr  style='height: 38px' face='宋体' bgcolor='#FFFFFF'>";
//    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>单价</font></th>";
//    strTableTheadHtml += "<th nowrap align='center'><font size = '2px' face='宋体'>总价</font></th>";
//    strTableTheadHtml += "</tr>";
//    var bodyTable = This.dataSet.getTable(1);
//    var headTable = This.dataSet.getTable(0);
//    var count = 0;
//    var Amount = 0;
//    var Amounter = 0;
//    var date = String(headTable.data.items[0].data["BILLDATE"]);
//    var d = new Date;
//    d.setFullYear(date.substr(0, 4));
//    d.setMonth(date.substr(4, 2));
//    d.setDate(date.substr(6, 2));
//    var weekDay = ["星期天", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"];
//    var dateStr = date.substr(0, 4) + "-"+ date.substr(4, 2)+"-"+date.substr(6, 2)+"-"+" 00:00:00";
//    var myDate = new Date(Date.parse(dateStr.replace(/-/g, "/")));
//    for (var i = 0; i < bodyTable.data.items.length; i++) {
//        count += bodyTable.data.items[i].data["QUANTITY"];
//        Amount = bodyTable.data.items[i].data["QUANTITY"] * bodyTable.data.items[i].data["SCHEDULEPRICE"];
//        strTableTheadHtml += "<tr>";
//        strTableTheadHtml += "<td nowrap align='center' height=30><font size='2px' face='宋体'>" + (i + 1) + "</font></td>";
//        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["METERNO"] + "</font></td>";
//        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["VALVETYPE"] + "</font></td>";
//        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["VTYPE"] + "</font></td>";
//        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["MTYPE"] + "</font></td>";
//        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["QUANTITY"] + "</font></td>";
//        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["SCHEDULEPRICE"] + "</font></td>";

//        strTableTheadHtml += "<td nowrap align='center'><font size='2px' face='宋体'>" + Amount + "</font></td>";
//        strTableTheadHtml += "<td nowrap align='center'  width=100><font size='2px' face='宋体'>" + bodyTable.data.items[i].data["REMARK"] + "</font></td>";
//        //strTableTheadHtml += "<td nowrap align='center' width=120><font size='2px' face='宋体'>" + "这个备注有点长，很长很长的备注" + "</font></td>";

//        strTableTheadHtml += "</tr>";
//    }


//    //strTableTheadHtml += "<thead style='height: 30px' face='宋体' bgcolor='#FFFFFF'>";

//    //strTableTheadHtml += "<td nowrap align='center' colspan =5 ><font size='3px' face='宋体'>" + "合同编号：" + headTable.data.items[0].data["PROJECTID"] + "</font></td>";
//    //strTableTheadHtml += "</thead>";

//    strTableTheadHtml += "<tr style='height: 30px' face='宋体' bgcolor='#FFFFFF'>";
//    strTableTheadHtml += "<td nowrap align='center' colspan =5><font size = '2px' face='宋体'>合计金额大写："+Rmb+"</font></td>";
//    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'>" + count + "</font></td>";
//    strTableTheadHtml += "<td nowrap align='center' colspan =2><font size = '2px' face='宋体'>" + headTable.data.items[0].data["OFFERAMOUNT"] + "</font></td>";
//    strTableTheadHtml += "<td nowrap align='center' ><font size = '2px' face='宋体'></font></td>";
//    strTableTheadHtml += "</tr>";
//    //strTableTheadHtml += "<tr style='height: 30px' face='宋体' bgcolor='#FFFFFF' border='0px'>";
//    //strTableTheadHtml += "<td nowrap align='left' colspan =9 ><font size = '2px' face='宋体'>商务条款：</font></td>";
//    //strTableTheadHtml += "</tr>";
//    strTableTheadHtml += "<tr style='height: 30px' face='宋体' bgcolor='#FFFFFF'>";
//    strTableTheadHtml += "<td nowrap align='left' colspan =9><font size = '2px' face='宋体'>商务条款：<br>" + html_encode(headTable.data.items[0].data["REMARK"]) + "</font></td>";
//    strTableTheadHtml += "</tr>";
//    //strTableTheadHtml += "<tr style='height: 30px' face='宋体' bgcolor='#FFFFFF'>";
//    //strTableTheadHtml += "<td nowrap align='left' colspan =9><font size = '2px' face='宋体'>2.报价有效期: 60天；</font></td>";
//    //strTableTheadHtml += "</tr>";
//    //strTableTheadHtml += "<tr style='height: 30px' face='宋体' bgcolor='#FFFFFF'>";
//    //strTableTheadHtml += "<td nowrap align='left' colspan =9><font size = '2px' face='宋体'>3.交货期：合同签订生效后或接到业主通知后8周。</font></td>";
//    //strTableTheadHtml += "</tr>";

//    var LODOP = getLodop(document.getElementById('LODOP_OB'), document.getElementById('LODOP_EM'));
//    LODOP.ADD_PRINT_TEXT(35, 350, 700, 40, "商务报价单");
//    LODOP.SET_PRINT_STYLEA(0, "FontSize", 20);
//    LODOP.SET_PRINT_STYLEA(0, "Bold", 1);
//    LODOP.ADD_PRINT_IMAGE(20, 40, "30%", "100%", "<img src=./Scripts/desk/images/zhongde.png height=60 width='100%'>");//商标

//    LODOP.ADD_PRINT_TEXT(120, 25, 370, 20, "询价单位：" + headTable.data.items[0].data["CONTACTSOBJECTNAME"]);
//    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
//    LODOP.ADD_PRINT_TEXT(150, 25, 400, 20, "询价日期：" + d.getFullYear() + "年" + d.getMonth() + "月" + d.getDate() + "日 " + weekDay[myDate.getDay()]);
//    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
//    //if (returnList.length != 0) {
//    //    for (var i = 0; i < length; i++) {
//            LODOP.ADD_PRINT_TEXT(180, 25, 370, 20, "联 系 人：" + returnList[0].Person + " " + returnList[0].MobilePhone);
//            LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

//            LODOP.ADD_PRINT_TEXT(210, 25, 370, 20, "传真：    " +  returnList[0].Fax);
//            LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
//            LODOP.ADD_PRINT_TEXT(240, 25, 370, 20, "Email：   " + returnList[0].Email);
//            LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
//    //    }
//    //}
//    //else
//    //{
//    //    LODOP.ADD_PRINT_TEXT(180, 25, 370, 20, "联 系 人：");
//    //    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

//    //    LODOP.ADD_PRINT_TEXT(210, 25, 370, 20, "传真：");
//    //    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
//    //    LODOP.ADD_PRINT_TEXT(240, 25, 370, 20, "Email：   " );
//    //    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
//    //}
//    LODOP.ADD_PRINT_TEXT(270, 25, 370, 20, "项目名称：" + headTable.data.items[0].data["PROJECTNAME"]);
//    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

//    LODOP.ADD_PRINT_TEXT(120, 415, 370, 20, "销售联系人：林梅乃 13353321808");
//    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

//    LODOP.ADD_PRINT_TEXT(150, 415, 370, 20, "商务联系人：" + headTable.data.items[0].data["CONTACTPERSONNAME"] + " " + headTable.data.items[0].data["CONPHONENO"]);
//    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

//    LODOP.ADD_PRINT_TEXT(180, 415, 370, 20, "技术联系人：" + headTable.data.items[0].data["SKILLPERSONNAME"] + " " + headTable.data.items[0].data["SKILLPHONENO"]);
//    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
//    LODOP.ADD_PRINT_TEXT(210, 415, 370, 20, "传      真：0572-6556888");
//    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

//    LODOP.ADD_PRINT_TEXT(240, 415, 370, 20, "Email：     vip@zhongdegroup.com");
//    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);

//    LODOP.ADD_PRINT_TEXT(270, 415, 370, 20, "报价编号：  " + headTable.data.items[0].data["BILLNO"]);
//    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
//    LODOP.ADD_PRINT_TEXT(300, 415, 370, 20, "报价日期：  " + d.getFullYear() + "年" + d.getMonth() + "月" + d.getDate() + "日 " + weekDay[myDate.getDay()]);
//    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);


//    //LODOP.SET_PRINT_STYLEA(0, "Alignment", 2);

//    //LODOP.PRINT_INITA(0, 0, "100%", "100%", "打印控件功能演示_Ext");


//    var htmlStr = strTableStartHtml + reMark + strTableTheadHtml + strTableEndHtml;

//    LODOP.ADD_PRINT_HTM(320, 1, "100%", "100%", htmlStr); //ADD_PRINT_HTM(Top,Left,Width,Height,strHtmlContent)

//    LODOP.SET_PRINT_PAGESIZE(1, 0, 0, "A4"); //1---纵(正)向打印  2---横向打印 SET_PRINT_PAGESIZE(intOrient,intPageWidth,intPageHeight,strPageName)设定纸张大小

//    //if (isPaint) {
//    //    LODOP.PRINT();
//    //}
//    //else {
//        LODOP.PREVIEW();
//    //}

//}