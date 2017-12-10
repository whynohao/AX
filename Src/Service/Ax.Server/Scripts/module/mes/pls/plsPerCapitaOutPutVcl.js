
plsPerCapitaOutPutVcl = function () {
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
        if (sd.record.data['ACTUALPERSONQUANTITY' + idx] != 0) {
            var value = sd.record.data['ACTUALQUANTITY' + idx] / sd.record.data['ACTUALPERSONQUANTITY' + idx];
            if (value >= 0) {
                return '<span style="color:blue;">' + (value).toFixed(2) + '</span>';
            }
            else {
                return '<span style="color:red;">' + (value).toFixed(2) + '</span>';
            }
        }
        else
            return '<span style="color:blue;">' + (0).toFixed(2) + '</span>';
    }
    this.summaryRenderer.TotalName = function (v, sd, f) {//v代表当前列汇总值，sd包含各列汇总值，f代表当前列字段名
        return '<span style="color:darkred;font-weight:bold;">总计：</span>';
    }
};

 
var proto = plsPerCapitaOutPutVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = plsPerCapitaOutPutVcl;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName.indexOf("ACTUALQUANTITY") >= 0 || e.dataInfo.fieldName.indexOf("ACTUALPERSONQUANTITY") >= 0) {
                var i;
                switch (len) {
                    case 15: i = e.dataInfo.fieldName.substring(len - 1); break;//ACTUALQUANTITY0~ ACTUALQUANTITY9
                    case 16: i = e.dataInfo.fieldName.substring(len - 2); break;//ACTUALQUANTITY10~ ACTUALQUANTITY31
                    case 21: i = e.dataInfo.fieldName.substring(len - 1); break;//ACTUALPERSONQUANTITY1~ACTUALPERSONQUANTITY9
                    case 22: i = e.dataInfo.fieldName.substring(len - 2); break;//ACTUALPERSONQUANTITY10~ACTUALPERSONQUANTITY31

                }
                //#region 重新计算准确率
                if (i <= 31) {
                    //修改实际作业人数后移开光标，重新计算完成率
                    if (e.dataInfo.fieldName == 'ACTUALPERSONQUANTITY' + i) {
                        var actualQty = e.dataInfo.dataRow.data['ACTUALQUANTITY' + i];
                        if (actualQty > 0 && e.dataInfo.value > 0) {
                            e.dataInfo.dataRow.set('CAPITAOUTPUT' + i, actualQty / e.dataInfo.value);
                        }
                        else {
                            e.dataInfo.dataRow.set('CAPITAOUTPUT' + i, 0);
                        }
                    }
                        //修改实际完成数量后移开光标，重新计算完成率
                    else if (e.dataInfo.fieldName == 'ACTUALQUANTITY' + i) {
                        var actualPersonQty = e.dataInfo.dataRow.data['ACTUALPERSONQUANTITY' + i + ''];
                        if (actualPersonQty > 0 && e.dataInfo.value > 0) {
                            e.dataInfo.dataRow.set('CAPITAOUTPUT' + i, e.dataInfo.value / actualPersonQty);
                        }
                        else {
                            e.dataInfo.dataRow.set('CAPITAOUTPUT' + i, 0); 
                        } 
                    }
                }
                //#endregion

                //#region
                var drPersonQty = 0;//实际作业人数 
                var drActualQty = 0;// 实际达成量
                for (var k = 0; k < dt.data.length; k++) {
                    if (e.dataInfo.dataRow.data['YEAR'] == dt.data.items[k].data.YEAR && e.dataInfo.dataRow.data['MONTH'] == dt.data.items[k].data.MONTH
                        && e.dataInfo.dataRow.data['TYPEID'] == dt.data.items[k].data.TYPEID && e.dataInfo.dataRow.data['WORKTEAMID'] == dt.data.items[k].data.WORKTEAMID
                        && e.dataInfo.dataRow.data['WORKSHOPSECTIONID'] == dt.data.items[k].data.WORKSHOPSECTIONID) {
                        //每次都要重新计算总共的值 以免存在多列同时修改的情况
                        for (var j = 1; j <= 31; j++) {
                            if (j == i) {//累加当前列的当前值
                                if (e.dataInfo.fieldName == 'ACTUALPERSONQUANTITY' + j) {
                                    drPersonQty += e.dataInfo.value;//当前列计划值的当前值
                                    drActualQty += dt.data.items[k].data['ACTUALQUANTITY' + j];//实际值的原始值
                                }
                                else if (e.dataInfo.fieldName == 'ACTUALQUANTITY' + j) {
                                    drPersonQty += dt.data.items[k].data['ACTUALPERSONQUANTITY' + j];//计划值的原始值
                                    drActualQty += e.dataInfo.value;//当前列实际值的当前值
                                }
                            }
                            else {
                                drPersonQty += dt.data.items[k].data['ACTUALPERSONQUANTITY' + j];
                                drActualQty += dt.data.items[k].data['ACTUALQUANTITY' + j];
                            }
                        }
                        e.dataInfo.dataRow.set('ACTUALPERSONQUANTITY32', drPersonQty);
                        e.dataInfo.dataRow.set('ACTUALQUANTITY32', drActualQty);
                        if (drPersonQty > 0 && drActualQty > 0) { 
                           e.dataInfo.dataRow.set('CAPITAOUTPUT32', drActualQty / drPersonQty); 
                        }
                        else {
                            e.dataInfo.dataRow.set('CAPITAOUTPUT32', 0);
                        }
                    }
                }
            }
            break;
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "LoadNextData") {  
                var date = this.invorkBcf("GetDateTime"); 
                var getyear = date.substring(0, 4);
                var getmonth = date.substring(5, 7);
                if (dt.data.items.length > 0) {
                    var year = 0;
                    var month = 0;
                    for (var j = 0 ; j < dt.data.items.length; j++)
                    {
                        var record = dt.data.items[j];
                        if (record.get('YEAR')>year)
                        {
                            year = record.get('YEAR');
                        }
                        if (record.get('MONTH') > month)
                        {
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
                        var data = this.invorkBcf('GetNextMonthData');
                        for (var i = 0; i < data.length; i++) {
                            dt.add(data[i]);
                        }
                    }
                }
                else {
                    var data = this.invorkBcf('GetNextMonthData');
                    for (var i = 0; i < data.length; i++) {
                        dt.add(data[i]);
                    }
                }
            }
            break;
    }


}