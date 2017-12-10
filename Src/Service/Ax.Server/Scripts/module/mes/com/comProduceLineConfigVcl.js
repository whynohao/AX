comProduceLineConfigVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
};
var proto = comProduceLineConfigVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = comProduceLineConfigVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclGrid.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.AddRow:
            if (e.dataInfo.tableIndex == 0) {
                var guid = Ax.utils.LibVclSystemUtils.newGuid();
                e.dataInfo.dataRow.set("RECORDID", guid);
            }
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 0) {
                //判断主表主键(生产线、工序)是否重复
                if (e.dataInfo.fieldName == 'PRODUCELINEID' || e.dataInfo.fieldName == 'WORKPROCESSID') {
                    var items = this.dataSet.getTable(0).data.items;
                    for (var i = 0; i < items.length; i++) {
                        if (e.dataInfo.fieldName == 'PRODUCELINEID') {
                            if (items[i].get('PRODUCELINEID') + items[i].get('WORKPROCESSID') == e.dataInfo.value + e.dataInfo.dataRow.get('WORKPROCESSID')) {
                                //alert("已存在生产线、工序相同的行");
                                //e.dataInfo.cancel = true; break;
                            }
                        }
                        else {
                            if (items[i].get('WORKPROCESSID') + items[i].get('PRODUCELINEID') == e.dataInfo.value + e.dataInfo.dataRow.get('PRODUCELINEID')) {
                                //alert("已存在生产线、工序相同的行");
                                //e.dataInfo.cancel = true; break;
                            }
                        }
                    }
                }
            }
            else if (e.dataInfo.tableIndex == 2) {
                if (e.dataInfo.fieldName == 'PRODUCEUNITID') {
                    //检测是否存在当前行【生产单位】相同的数据行
                    var parentRow = e.dataInfo.curGrid.parentRow; //父行
                    var records = this.dataSet.getChildren(0, parentRow, 2);//与当前行同属一个父行的行的集合
                    for (var i = 0; i < records.length; i++) {
                        if (records[i].get('PRODUCEUNITID') == e.dataInfo.value && records[i].get('ROW_ID') != e.dataInfo.dataRow.get('ROW_ID')) {
                            //alert("存在相同的生产单位");
                            //e.dataInfo.cancel = true; break;
                        }
                    }
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 0) {
                if (e.dataInfo.fieldName == 'RESOURCEID') {
                    if (e.dataInfo.value != e.dataInfo.oldValue) {
                        this.setTableThird.call(this, e);
                    }
                }
            }
            else if (e.dataInfo.tableIndex == 2) {
                //修改产线配置资源明细【时间】【产量】关联主表【节拍时间】【节拍时间单位】
                if (e.dataInfo.fieldName == 'PRODUCETIME' || e.dataInfo.fieldName == 'PRODUCEQTY') {
                    var data = [];
                    var b = true;
                    e.dataInfo.dataRow.set(e.dataInfo.fieldName, e.dataInfo.value);
                    var parentRow = e.dataInfo.curGrid.parentRow; //父行
                    var records = this.dataSet.getChildren(0, parentRow, 2);//与当前行同属一个父行的行的集合
                    for (var i = 0; i < records.length; i++) {
                        if (records[i].get('PRODUCETIME') > 0 && records[i].get('PRODUCEQTY') > 0) {
                            var produceTime = records[i].get('PRODUCETIME');
                            if (records[i].get('TIMEUNIT') == 1) {
                                //把时间一致调整为以秒为单位
                                produceTime *= 60;
                            }
                            data.push({ PRODUCETIME: produceTime, PRODUCEQTY: records[i].get('PRODUCEQTY') });
                        }
                        else {
                            b = false;
                            break;
                        }
                    }
                    if (b && data.length > 0) {
                        var beatTime = 0;
                        var produceQty = 0;
                        for (var i = 0; i < data.length; i++) {
                            beatTime += data[i].PRODUCETIME;
                            produceQty += data[i].PRODUCEQTY;
                        }
                        beatTime /= produceQty;
                        if (beatTime > 60) {
                            beatTime /= 60;
                            e.dataInfo.curGrid.parentRow.set('BEATTIMEUNIT', 1);
                        }
                        else {
                            e.dataInfo.curGrid.parentRow.set('BEATTIMEUNIT', 0);
                        }
                        e.dataInfo.curGrid.parentRow.set('BEATTIME', beatTime);
                    }
                }
            }
            break;
    }
}

//修改资源，重新填写资源明细
proto.setTableThird = function (e) { 
    var tableThird = this.dataSet.getTable(2);
    var length = tableThird.data.items.length;
    var produceLineId=e.dataInfo.dataRow.get('PRODUCELINEID');
    var workProcessId = e.dataInfo.dataRow.get('WORKPROCESSID');
    for (var i = 0; i < length; i++) {
        if (tableThird.data.items[i].get('PRODUCELINEID') == produceLineId && tableThird.data.items[i].get('WORKPROCESSID') == workProcessId) {
            tableThird.data.removeAt(i);
            i -= 1;
            length -= 1;
        }
    }
    var data = this.invorkBcf('GetTableThird', [e.dataInfo.value]);
    if (data !== undefined && data.length > 0) {
        e.dataInfo.dataRow.set('ISRESOURCE', true);
        for (var i = 0; i < data.length; i++) {
            var info = data[i];
            var newRow = vcl.addRow(e.dataInfo.dataRow, 2);
            newRow.set('ROW_ID', i+1);
            newRow.set('ROWNO', i+1);
            newRow.set('PRODUCETYPE', info.ProduceType);
            newRow.set('PRODUCEUNITID', info.ProduceUnitId);
            newRow.set('PRODUCEUNITNAME', info.ProduceUnitName);
        }
    }
    else
        e.dataInfo.dataRow.set('ISRESOURCE', false);
}

