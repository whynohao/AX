plsSectionOverAllEfficiencyVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
    this.summaryRenderer.TotalRateFun = function (v, sd, f) {
        var idx = '';
        var regx = /^[0-9]*$/;
        for (var i = this.dataIndex.length - 1; i >= 0; i--) {
            var v = this.dataIndex[i];
            if (regx.test(v)) {
                if (idx.length == 0)
                    idx = v;
                else
                    idx = Ext.String.insert(idx, v, 0);
            }
            else
                break;
        }
        if (sd.record.data['WORKTEAMTIME' + idx] != 0) {
            var value = sd.record.data['PIECEWORKINGTIME' + idx] / sd.record.data['WORKTEAMTIME' + idx];
            if (value >= 0) {
                return '<span style="color:blue;">' + (value * 100).toFixed(2) + '%</span>';
            }
            else {
                return '<span style="color:red;">' + (value * 100).toFixed(2) + '%</span>';
            }
        }
        else
            return '<span style="color:blue;">' + (0).toFixed(2) + '%</span>';
    }
    this.summaryRenderer.TotalName = function (v, sd, f) {//v代表当前列汇总值，sd包含各列汇总值，f代表当前列字段名
        return '<span style="color:darkred;font-weight:bold;">总计：</span>';
    }
};

 
var proto = plsSectionOverAllEfficiencyVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = plsSectionOverAllEfficiencyVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName.indexOf("PIECEWORKINGTIME") >= 0 || e.dataInfo.fieldName.indexOf("WORKTEAMTIME") >= 0) {
                var i;
                switch (len) {
                    case 13: i = e.dataInfo.fieldName.substring(len - 1); break;//WORKTEAMTIME0~ WORKTEAMTIME9
                    case 14: i = e.dataInfo.fieldName.substring(len - 2); break;//WORKTEAMTIME10~ WORKTEAMTIME31
                    case 17: i = e.dataInfo.fieldName.substring(len - 1); break;//PIECEWORKINGTIME1~PIECEWORKINGTIME9
                    case 18: i = e.dataInfo.fieldName.substring(len - 2); break;//PIECEWORKINGTIME10~PIECEWORKINGTIME31

                }
                //#region 重新计算准确率
                if (i <= 31) {
                    //修改实际作业人数后移开光标，重新计算完成率
                    if (e.dataInfo.fieldName == 'PIECEWORKINGTIME' + i) {
                        var workteamtime = e.dataInfo.dataRow.data['WORKTEAMTIME' + i];
                        if (workteamtime > 0 && e.dataInfo.value > 0) {
                            e.dataInfo.dataRow.set('EFFICIENCY' + i, e.dataInfo.value / workteamtime);
                        }
                        else {
                            e.dataInfo.dataRow.set('EFFICIENCY' + i, 0);
                        }
                    }
                        //修改实际完成数量后移开光标，重新计算完成率
                    else if (e.dataInfo.fieldName == 'WORKTEAMTIME' + i) {
                        var pieceworkingtime = e.dataInfo.dataRow.data['PIECEWORKINGTIME' + i + ''];
                        if (pieceworkingtime > 0 && e.dataInfo.value > 0) {
                            e.dataInfo.dataRow.set('EFFICIENCY' + i, pieceworkingtime / e.dataInfo.value);
                        }
                        else {
                            e.dataInfo.dataRow.set('EFFICIENCY' + i, 0);
                        }

                    }
                }
                //#endregion

                 //#region
                var drWorkTime = 0;// 班组出勤（分钟）
                var drPieceWorkTime = 0;// 产品计件工时（分钟）
                for (var k = 0; k < dt.data.length; k++) {
                    if (e.dataInfo.dataRow.data['YEAR'] == dt.data.items[k].data.YEAR && e.dataInfo.dataRow.data['MONTH'] == dt.data.items[k].data.MONTH
                        && e.dataInfo.dataRow.data['TYPEID'] == dt.data.items[k].data.TYPEID && e.dataInfo.dataRow.data['WORKTEAMID'] == dt.data.items[k].data.WORKTEAMID
                        && e.dataInfo.dataRow.data['WORKSHOPSECTIONID'] == dt.data.items[k].data.WORKSHOPSECTIONID) {
                        //每次都要重新计算总共的值 以免存在多列同时修改的情况
                        for (var j = 1; j <= 31; j++) {
                            if (j == i) {//累加当前列的当前值
                                if (e.dataInfo.fieldName == 'WORKTEAMTIME' + j) {
                                    drWorkTime += e.dataInfo.value;//当前列计划值的当前值
                                    drPieceWorkTime += dt.data.items[k].data['PIECEWORKINGTIME' + j];//实际值的原始值
                                }
                                else if (e.dataInfo.fieldName == 'PIECEWORKINGTIME' + j) {
                                    drWorkTime += dt.data.items[k].data['WORKTEAMTIME' + j];//计划值的原始值
                                    drPieceWorkTime += e.dataInfo.value;//当前列实际值的当前值
                                }
                            }
                            else {
                                drWorkTime += dt.data.items[k].data['WORKTEAMTIME' + j];
                                drPieceWorkTime += dt.data.items[k].data['PIECEWORKINGTIME' + j];
                            }
                        }
                        e.dataInfo.dataRow.set('WORKTEAMTIME32', drWorkTime);
                        e.dataInfo.dataRow.set('PIECEWORKINGTIME32', drPieceWorkTime);
                        if (drPieceWorkTime > 0 && drWorkTime > 0) { 
                             e.dataInfo.dataRow.set('EFFICIENCY32', drPieceWorkTime / drWorkTime); 
                        }
                        else {
                            e.dataInfo.dataRow.set('EFFICIENCY32', 0);
                        }
                    }
                }
                    //#endregion
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "LoadNextData") {
                var date = this.invorkBcf("GetDateNow"); 
                var getyear = date.substring(0, 4);
                var getmonth = date.substring(5, 7);
                if (dt.data.items.length > 0) {
                    var year = 0;
                    var month = 0;
                    for (var j = 0 ; j < dt.data.items.length; j++) {
                        var record = dt.data.items[j];
                        if (record.get('YEAR') > year) {
                            year = record.get('YEAR');
                        }
                        if (record.get('MONTH') > month) {
                            month = record.get('MONTH');
                        }
                    }
                    if (month.length = 1) {
                        month = "0" + month;
                    }

                    if (month == getmonth && year == getyear) {
                        return;
                    }
                    else {
                        var data = this.invorkBcf('GetLoadNextMonthData');
                        for (var i = 0; i < data.length; i++) {
                            dt.add(data[i]);
                        }
                    }
                }
                else {
                    var data = this.invorkBcf('GetLoadNextMonthData');
                    for (var i = 0; i < data.length; i++) {
                        dt.add(data[i]);
                    }
                } 
            }
            break;
    }


}