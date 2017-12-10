
comScheduleVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = comScheduleVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comScheduleVcl;

function calcTime(end, start) {
    var value, endTime, startTime;
    if (end < start) {
        end = end.toString();
        end = Format(end);

        start = start.toString();
        start = Format(start);
        endTime = new Date(2014, 0, 2, end.substr(0, 2), end.substr(2, 2), 0);
        startTime = new Date(2014, 0, 1, start.substr(0, 2), start.substr(2, 2), 0);
    } else {
        end = end.toString();
        end = Format(end);

        start = start.toString();
        start=Format(start);
        endTime = new Date(2014, 0, 1, end.substr(0, 2), end.substr(2, 2), 0);
        startTime = new Date(2014, 0, 1, start.substr(0, 2), start.substr(2, 2), 0);
    }
    diff = endTime.getTime() - startTime.getTime();
    value = diff / (1000 * 60 * 60);
    value = value.toFixed(2);
    return value;
}

function Format(str) {
    var len = str.length;
    switch (len) {
        case 1:
            str = '000' + str;
            break;
        case 2:
            str = '00' + str;
            break;
        case 3:
            str = '0' + str;
            break;
    }
    return str;
}

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                var fieldName = e.dataInfo.fieldName;
                //计算 排班时长
                if (fieldName == "SCHEDULEENDTIME") {
                    var endTime = e.dataInfo.value;
                    var startTime = e.dataInfo.dataRow.get("SCHEDULESTARTTIME");
                    if (endTime && startTime)
                        e.dataInfo.dataRow.set('SCHEDULETIME', calcTime(endTime, startTime));
                } else if (fieldName == "SCHEDULESTARTTIME") {
                    var startTime = e.dataInfo.value;
                    var endTime = e.dataInfo.dataRow.get("SCHEDULEENDTIME");
                    if (endTime && startTime)
                        e.dataInfo.dataRow.set('SCHEDULETIME', calcTime(endTime, startTime));
                }
            }
            break;
    }
}
