//金额变动改变的类型
var ChangeTypeEnum = {
    ChangeDealsQuantity: 1,//改变交易数量
    ChangeTaxRate: 2,//改变税率
    ChangePrice: 3,//改变单价
    ChangeTaxPrice: 4,//改变含税单价
    ChangeAmount: 5,//改变金额
    ChangeTaxAmount: 6,//改变含税金额
    ChangeTaxes: 7,//改变税额
    ChangeStandardcoilRate: 8,//改变汇率
    ChangeBWAmount: 9,//改变本币金额
    ChangeBWTaxAmount: 10,//改变本币含税金额

};
//去除打印undefined
function changeUndefined(str) {
    var rstr = "";
    if ((str != null) && str != undefined) {
        rstr = str;
    }
    return rstr;
}
//获取单据表头日期年月日  XXXX-XX-XX
function changeTodate(longTime) {
    longTime = longTime.toString();
    var time = "";
    if (longTime > 0) {
        return longTime.substr(0, 4) + "-" + longTime.substr(4, 2) + "-" + longTime.substr(6, 2);
    }
    return time;
}
//获取单据日期月日  XXXX年XX月XX日
function changeTodateTwo(longTime) {
    longTime = longTime.toString();
    var time = "";
    if (longTime > 0) {
        return longTime.substr(0, 4) + "年" + longTime.substr(4, 2) + "月" + longTime.substr(6, 2) + "日"
    }
    return time;
}
function changeTodateTwoLastYear(longTime) {
    longTime = longTime.toString();
    var time = "";
    if (longTime > 0) {
        var year = parseInt(longTime.substr(0, 4)) + 1;
        var date = parseInt(longTime.substr(6, 2)) - 1;
        if (date < 10)
            date = "0" + date;
        console.log(rdate = year + longTime.substr(4, 2) + date);
        return year + longTime.substr(4, 2) + date;
    }
    return time;
}
//function changeTodateTwoLastYear(longTime) {
//    longTime = longTime.toString();      
//    var time = "";
//    if (longTime > 0) {
//        var year = parseInt(longTime.substr(0, 4)) + 1;
//        var date = parseInt(longTime.substr(6, 2)) - 1;
//        if (date < 10)
//            date = "0" + date;
//        return year + "年" + longTime.substr(4, 2) + "月" + date + "日"
//    }
//    return time;
//}


//金额转中文大写
function changeNumMoneyToChinese(money) {
    var cnNums = new Array("零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖"); //汉字的数字
    var cnIntRadice = new Array("", "拾", "佰", "仟"); //基本单位
    var cnIntUnits = new Array("", "万", "亿", "兆"); //对应整数部分扩展单位
    var cnDecUnits = new Array("角", "分", "毫", "厘"); //对应小数部分单位
    var cnInteger = "整"; //整数金额时后面跟的字符
    var cnIntLast = "元"; //整型完以后的单位
    var maxNum = 999999999999999.9999; //最大处理的数字
    var IntegerNum; //金额整数部分
    var DecimalNum; //金额小数部分
    var ChineseStr = ""; //输出的中文金额字符串
    var parts; //分离金额后用的数组，预定义
    if (money == "") {
        return "";
    }
    money = parseFloat(money);
    if (money >= maxNum) {
        alert('超出最大处理数字');
        return "";
    }
    if (money == 0) {
        ChineseStr = cnNums[0] + cnIntLast + cnInteger;
        return ChineseStr;
    }
    money = money.toString(); //转换为字符串
    if (money.indexOf(".") == -1) {
        IntegerNum = money;
        DecimalNum = '';
    } else {
        parts = money.split(".");
        IntegerNum = parts[0];
        DecimalNum = parts[1].substr(0, 4);
    }
    if (parseInt(IntegerNum, 10) > 0) { //获取整型部分转换
        var zeroCount = 0;
        var IntLen = IntegerNum.length;
        for (var i = 0; i < IntLen; i++) {
            var n = IntegerNum.substr(i, 1);
            var p = IntLen - i - 1;
            var q = p / 4;
            var m = p % 4;
            if (n == "0") {
                zeroCount++;
            } else {
                if (zeroCount > 0) {
                    ChineseStr += cnNums[0];
                }
                zeroCount = 0; //归零
                ChineseStr += cnNums[parseInt(n)] + cnIntRadice[m];
            }
            if (m == 0 && zeroCount < 4) {
                ChineseStr += cnIntUnits[q];
            }
        }
        ChineseStr += cnIntLast;
        //整型部分处理完毕
    }
    if (DecimalNum != '') { //小数部分
        var decLen = DecimalNum.length;
        for (var i = 0; i < decLen; i++) {
            var n = DecimalNum.substr(i, 1);
            if (n != '0') {
                ChineseStr += cnNums[Number(n)] + cnDecUnits[i];
            }
        }
    }
    if (ChineseStr == '') {
        ChineseStr += cnNums[0] + cnIntLast + cnInteger;
    } else if (DecimalNum == '') {
        ChineseStr += cnInteger;
    }
    return ChineseStr;

}
//金额千位符格式化（四舍五入） 参数n 小数显示位数 
function changeMoney(s, n) {
    n = n > 0 && n <= 20 ? n : 2;
    s = parseFloat((s + "").replace(/[^\d\.-]/g, "")).toFixed(n) + "";
    var l = s.split(".")[0].split("").reverse(),
    r = s.split(".")[1];
    t = "";
    for (i = 0; i < l.length; i++) {
        t += l[i] + ((i + 1) % 3 == 0 && (i + 1) != l.length ? "," : "");
    }
    return t.split("").reverse().join("") + "." + r;
}
//把特征"【】"间隔替换为","
function changeAttribute(str) {
    var rstr = new Array()
    if (str != null) {
        re = new RegExp("【", "g");
        re1 = new RegExp("】", "g");
        var Nattribute = str.replace(re, "").replace(re1, ",");
        rstr = Nattribute.substr(0, Nattribute.length - 1).split(",");//去结尾逗号并数组化
    }
    return rstr;
}