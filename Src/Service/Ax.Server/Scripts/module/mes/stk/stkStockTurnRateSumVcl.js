/// <reference path="../../../ax/vcl/comm/LibVclDataFunc.js" />


stkStockTurnRateSumVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = stkStockTurnRateSumVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = stkStockTurnRateSumVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            var table = this.dataSet.getTable(1);
            var masterRow = this.dataSet.getTable(0).data.items[0];
            this.forms[0].updateRecord(masterRow);
            var year = masterRow.get('YEAR');
            if (e.dataInfo.fieldName == 'btnLoad') {
                var data = this.invorkBcf('GetStockDetail', [year]);
                table.removeAll();
                for (var i = 0; i < data.length; i++) {
                    table.add(data[i]);
                }
            }
            else if (e.dataInfo.fieldName == 'btnSave') {
                var turnRateSumInfo = [];
                for (var i = 0; i < table.data.items.length; i++) {
                    var record = table.data.items[i];
                    turnRateSumInfo.push({
                        Row_Id: record.data.ROW_ID,
                        Year: year,
                        Month: record.data.MONTH,
                        MainCost: record.data.MAINCOST,
                        MonthStockQty: record.data.MONTHSTOCKQTY,
                        AvgStockQty: record.data.AVGSTOCKQTY,
                        TurnOverRate: record.data.TURNOVERRATE,
                        LastMainCost: record.data.LASTMAINCOST,
                        LastMonthStockQty: record.data.LASTMONTHSTOCKQTY,
                        LastAvgStockQty: record.data.LASTAVGSTOCKQTY,
                        LastTurnOverRate: record.data.LASTTURNOVERRATE,
                        TurnOverRateRate: record.data.TURNOVERRATERATE
                    });
                }
                this.invorkBcf('SaveStockDetail', [turnRateSumInfo]);
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                var table = this.dataSet.getTable(1);
                var str = e.dataInfo.dataRow.data['MONTH'];//1月——9月 10月——12月
                var i = str.length == 2 ? str.substr(0, 1) : str.substr(0, 2);
                if (e.dataInfo.fieldName == 'MAINCOST') {//修改主营成本
                    var realStockQty = e.dataInfo.dataRow.data['MONTHSTOCKQTY'];
                    var realMainCost = e.dataInfo.value;

                    switch (str) {
                        case '期初'://修改期初主营成本
                            var realAvgQty = e.dataInfo.dataRow.data['AVGSTOCKQTY'];
                            var realTurnRate = e.dataInfo.dataRow.data['TURNOVERRATE'];
                            this.ChangeQiChuData(realStockQty, realMainCost, realAvgQty, realTurnRate);
                            break;
                        case i + '月'://修改某月主营成本
                            var stockQtySum = realStockQty;
                            for (var j = 0; j < i; j++) {
                                stockQtySum += table.data.items[j].data['MONTHSTOCKQTY'];
                            }
                            var avgStockQty = stockQtySum / (parseInt(i) + 1);
                            var turnRate = avgStockQty > 0 ? realMainCost / avgStockQty : 0;
                            e.dataInfo.dataRow.set('AVGSTOCKQTY', avgStockQty.toFixed(2));
                            e.dataInfo.dataRow.set('TURNOVERRATE', turnRate.toFixed(4));
                            e.dataInfo.dataRow.set('LASTMAINCOST', table.data.items[i - 1].data['MAINCOST']);
                            e.dataInfo.dataRow.set('LASTMONTHSTOCKQTY', table.data.items[i - 1].data['MONTHSTOCKQTY']);
                            e.dataInfo.dataRow.set('LASTAVGSTOCKQTY', table.data.items[i - 1].data['AVGSTOCKQTY']);
                            e.dataInfo.dataRow.set('LASTTURNOVERRATE', table.data.items[i - 1].data['TURNOVERRATE']);
                            if (e.dataInfo.dataRow.data['LASTTURNOVERRATE'] > 0) {
                                e.dataInfo.dataRow.set('TURNOVERRATERATE', (turnRate / e.dataInfo.dataRow.data['LASTTURNOVERRATE'] - 1).toFixed(4));
                            }
                            else {
                                e.dataInfo.dataRow.set('TURNOVERRATERATE', 0);
                            }
                            var realAvgQty = e.dataInfo.dataRow.data['AVGSTOCKQTY'];
                            var realTurnRate = e.dataInfo.dataRow.data['TURNOVERRATE'];
                            this.ChangeMonthData(realStockQty, realMainCost, realAvgQty, realTurnRate, parseInt(i));
                            break;
                    }

                }
                else if (e.dataInfo.fieldName == 'MONTHSTOCKQTY') {
                    var realStockQty = e.dataInfo.value;
                    var realMainCost = e.dataInfo.dataRow.data['MAINCOST'];

                    switch (str) {
                        case '期初':
                            var realAvgQty = e.dataInfo.dataRow.data['AVGSTOCKQTY'];
                            var realTurnRate = e.dataInfo.dataRow.data['TURNOVERRATE'];
                            this.ChangeQiChuData(realStockQty, realMainCost, realAvgQty, realTurnRate);
                            break;
                        case i + '月':
                            var stockQtySum = e.dataInfo.value;
                            for (var j = 0; j < i; j++) {
                                stockQtySum += table.data.items[j].data['MONTHSTOCKQTY'];
                            }
                            var avgStockQty = stockQtySum / (parseInt(i) + 1);
                            var turnRate = avgStockQty > 0 ? table.data.items[i].data['MAINCOST'] / avgStockQty : 0;
                            e.dataInfo.dataRow.set('AVGSTOCKQTY', avgStockQty.toFixed(2));
                            e.dataInfo.dataRow.set('TURNOVERRATE', turnRate.toFixed(4));
                            e.dataInfo.dataRow.set('LASTMAINCOST', table.data.items[i - 1].data['MAINCOST']);
                            e.dataInfo.dataRow.set('LASTMONTHSTOCKQTY', table.data.items[i - 1].data['MONTHSTOCKQTY']);
                            e.dataInfo.dataRow.set('LASTAVGSTOCKQTY', table.data.items[i - 1].data['AVGSTOCKQTY']);
                            e.dataInfo.dataRow.set('LASTTURNOVERRATE', table.data.items[i - 1].data['TURNOVERRATE']);
                            if (e.dataInfo.dataRow.data['LASTTURNOVERRATE'] > 0) {
                                e.dataInfo.dataRow.set('TURNOVERRATERATE', (turnRate / e.dataInfo.dataRow.data['LASTTURNOVERRATE'] - 1).toFixed(4));
                            }
                            else {
                                e.dataInfo.dataRow.set('TURNOVERRATERATE', 0);
                            }
                            var realAvgQty = e.dataInfo.dataRow.data['AVGSTOCKQTY'];
                            var realTurnRate = e.dataInfo.dataRow.data['TURNOVERRATE'];
                            this.ChangeMonthData(realStockQty, realMainCost, realAvgQty, realTurnRate, parseInt(i));
                            break;
                    }
                }

            }
           break;
          
    }
}

proto.ChangeQiChuData = function (realStockQty, realMainCost, realAvgQty, realTurnRate) {
    var table = this.dataSet.getTable(1);
    for (var k = 1; k < table.data.items.length; k++) {
        var stockQtySum = realStockQty;
        for (var j = 1; j <= k; j++) {
            stockQtySum += table.data.items[j].data['MONTHSTOCKQTY'];
        }
        var avgStockQty = stockQtySum / (parseInt(k) + 1);
        var turnRate = avgStockQty > 0 ? table.data.items[k].data['MAINCOST'] / avgStockQty : 0;
        table.data.items[k].set('AVGSTOCKQTY', avgStockQty.toFixed(2));
        table.data.items[k].set('TURNOVERRATE', turnRate.toFixed(4));

        if (k == 1) {
            table.data.items[k].set('LASTMAINCOST', realMainCost);
            table.data.items[k].set('LASTMONTHSTOCKQTY', realStockQty);
            table.data.items[k].set('LASTAVGSTOCKQTY', realAvgQty);
            table.data.items[k].set('LASTTURNOVERRATE', realTurnRate);
        }
        else {
            table.data.items[k].set('LASTMAINCOST', table.data.items[k - 1].data['MAINCOST']);
            table.data.items[k].set('LASTMONTHSTOCKQTY', table.data.items[k - 1].data['MONTHSTOCKQTY']);
            table.data.items[k].set('LASTAVGSTOCKQTY', table.data.items[k - 1].data['AVGSTOCKQTY']);
            table.data.items[k].set('LASTTURNOVERRATE', table.data.items[k - 1].data['TURNOVERRATE']);
        }

        if (table.data.items[k].data['LASTTURNOVERRATE'] > 0) {
            table.data.items[k].set('TURNOVERRATERATE', (table.data.items[k].data['TURNOVERRATE'] / table.data.items[k].data['LASTTURNOVERRATE'] - 1).toFixed(4));
        }
        else {
            table.data.items[k].set('TURNOVERRATERATE', 0);
        }
    }
}

proto.ChangeMonthData = function (realStockQty, realMainCost, realAvgQty, realTurnRate, i) {
    if (i != undefined) {
        var table = this.dataSet.getTable(1);
        for (var k = 1; k < table.data.items.length; k++) {
            if (k != i) {
                var itemMonth = table.data.items[k].data['MONTH'];//1月——9月 10月——12月
                switch (itemMonth) {
                    case k + '月':
                        var stockQtySum = table.data.items[k].data['MONTHSTOCKQTY'];
                        for (var j = 0; j < k; j++) {
                            if (j == i) {
                                stockQtySum += realStockQty;
                            }
                            else {
                                stockQtySum += table.data.items[j].data['MONTHSTOCKQTY'];
                            }
                        }
                        var avgStockQty = stockQtySum / (parseInt(k) + 1);
                        var turnRate = avgStockQty > 0 ? table.data.items[k].data['MAINCOST'] / avgStockQty : 0;
                        table.data.items[k].set('AVGSTOCKQTY', avgStockQty.toFixed(2));
                        table.data.items[k].set('TURNOVERRATE', turnRate.toFixed(4));
                        break;
                }
                if (k == parseInt(i) + 1) {
                    table.data.items[k].set('LASTMAINCOST', realMainCost);
                    table.data.items[k].set('LASTMONTHSTOCKQTY', realStockQty);
                    table.data.items[k].set('LASTAVGSTOCKQTY', realAvgQty);
                    table.data.items[k].set('LASTTURNOVERRATE', realTurnRate);
                }
                else {
                    table.data.items[k].set('LASTMAINCOST', table.data.items[k - 1].data['MAINCOST']);
                    table.data.items[k].set('LASTMONTHSTOCKQTY', table.data.items[k - 1].data['MONTHSTOCKQTY']);
                    table.data.items[k].set('LASTAVGSTOCKQTY', table.data.items[k - 1].data['AVGSTOCKQTY']);
                    table.data.items[k].set('LASTTURNOVERRATE', table.data.items[k - 1].data['TURNOVERRATE']);
                }

                if (table.data.items[k].data['LASTTURNOVERRATE'] > 0) {
                    table.data.items[k].set('TURNOVERRATERATE', (table.data.items[k].data['TURNOVERRATE'] / table.data.items[k].data['LASTTURNOVERRATE'] - 1).toFixed(4));
                }
                else {
                    table.data.items[k].set('TURNOVERRATERATE', 0);
                }
            }
        }
    }
}