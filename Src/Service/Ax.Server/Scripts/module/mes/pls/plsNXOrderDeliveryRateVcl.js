//plsNXOrderDeliveryRateVcl
plsNXOrderDeliveryRateVcl = function () {
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
        if (sd.record.data['ORDERQUANTITY' + idx] != 0) {
            var value = sd.record.data['ONTIMEQUANTITY' + idx] / sd.record.data['ORDERQUANTITY' + idx];
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
}
var proto = plsNXOrderDeliveryRateVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = plsNXOrderDeliveryRateVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    var dt = this.dataSet.getTable(0);
    switch (e.libEventType) {
        case LibEventTypeEnum.Validated:
            var len = e.dataInfo.fieldName.length;
            if (e.dataInfo.fieldName != 'PRODUCTTYPEID' && e.dataInfo.fieldName != 'PRODUCTTYPENAME') {
                var i;
                switch (len) {
                    case 15:
                        if (e.dataInfo.fieldName.substring(0, 2) == 'ON') {
                            i = e.dataInfo.fieldName.substring(len - 1);//ONTIMEQUANTITY1~ ONTIMEQUANTITY9
                        }
                        else if (e.dataInfo.fieldName.substring(0, 2) == 'OR') {
                            i = e.dataInfo.fieldName.substring(len - 2);//ORDERQUANTITY10~ORDERQUANTITY13
                        }
                        break;
                    case 16: i = e.dataInfo.fieldName.substring(len - 2); break;//ONTIMEQUANTITY10~ ONTIMEQUANTITY13
                    case 14: i = e.dataInfo.fieldName.substring(len - 1); break;//ORDERQUANTITY1~ORDERQUANTITY9
                }
                //#region 订单按时交货率
                if (i < 13) {
                    //修改按时完成的订单数量后移开光标，重新计算订单按时交货率
                    if (e.dataInfo.fieldName == 'ONTIMEQUANTITY' + i) {
                        var orderQty = e.dataInfo.dataRow.data['ORDERQUANTITY' + i];
                        if (orderQty > 0 && e.dataInfo.value > 0) {
                            e.dataInfo.dataRow.set('DELIVERYRATE' + i,e.dataInfo.value/orderQty);
                        }
                        else {
                            e.dataInfo.dataRow.set('DELIVERYRATE' + i, 0);
                        }
                    }
                    //修改总订单数量后移开光标，重新计算订单按时交货率
                    else if (e.dataInfo.fieldName == 'ORDERQUANTITY' + i) {
                        var onTimeQty = e.dataInfo.dataRow.data['ONTIMEQUANTITY' + i + ''];
                        if (onTimeQty > 0 && e.dataInfo.value > 0) {
                            e.dataInfo.dataRow.set('DELIVERYRATE' + i, onTimeQty/e.dataInfo.value);
                        }
                        else {
                            e.dataInfo.dataRow.set('DELIVERYRATE' + i, 0);
                        }

                    }
                }
                //#endregion

                var drOnTimeQuantity = 0;//当前行12个月的及时交货数总量
                var drOrderQuantity = 0;//当前行12个月的订单数总量
                for (var k = 0; k < dt.data.length; k++) {
                    if (e.dataInfo.dataRow.data['PRODUCTTYPEID'] == dt.data.items[k].data.PRODUCTTYPEID) {
                        //每次都要重新计算总共的值 以免存在多列同时修改的情况
                        for (var j = 1; j <= 12; j++) {
                            if (j == i) {//累加当前列的当前值
                                if (e.dataInfo.fieldName == 'ONTIMEQUANTITY' + j) {
                                    drOnTimeQuantity += e.dataInfo.value;//当前列的当前值
                                    drOrderQuantity += dt.data.items[k].data['ORDERQUANTITY' + j];//订单数的原始值
                                }
                                else if (e.dataInfo.fieldName == 'ORDERQUANTITY' + j) {
                                    drOnTimeQuantity += dt.data.items[k].data['ONTIMEQUANTITY' + j];//及时交货数的原始值
                                    drOrderQuantity += e.dataInfo.value;//当前列的当前值
                                }
                            }
                            else {
                                drOnTimeQuantity += dt.data.items[k].data['ONTIMEQUANTITY' + j];
                                drOrderQuantity += dt.data.items[k].data['ORDERQUANTITY' + j];
                            }
                        }
                        e.dataInfo.dataRow.set('ONTIMEQUANTITY13', drOnTimeQuantity);
                        e.dataInfo.dataRow.set('ORDERQUANTITY13', drOrderQuantity);
                        if (drOnTimeQuantity > 0 && drOrderQuantity > 0) {
                            e.dataInfo.dataRow.set('DELIVERYRATE13', drOnTimeQuantity / drOrderQuantity);
                        }
                        else {
                            e.dataInfo.dataRow.set('DELIVERYRATE13', 0);
                        }
                    }
                }
            }
            break;
    }


}