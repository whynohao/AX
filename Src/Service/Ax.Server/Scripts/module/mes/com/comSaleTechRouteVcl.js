comSaleTechRouteVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    this.originalValue = [];
};
var proto = comSaleTechRouteVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = comSaleTechRouteVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.AddRow:
            if (e.dataInfo.tableIndex == 1) {
                //自动填充工序号
                var store = this.dataSet.getTable(1);
                var length = store.data.items.length;
                var maxWp = 0;
                for (var i = 0; i < length; i++) {
                    var workprocessNo = store.data.items[i].get('WORKPROCESSNO');
                    if (maxWp < workprocessNo)
                        maxWp = workprocessNo;
                }
                e.dataInfo.dataRow.set('WORKPROCESSNO', (maxWp + 5));
                //新增行自动填写差异
                e.dataInfo.dataRow.set('DIFFERENCE', '{"Add":true}');
                //未启用并行工艺时自动填充转入工序号
                if (!this.dataSet.getTable(0).data.items[0].get('ISPARALLELTECHROUTE'))
                    this.GetTransferWorkprocessNo.call(this);
            }
            break;
        case LibEventTypeEnum.DeleteRow:
            if (e.dataInfo.tableIndex == 1) {
                if (!this.dataSet.getTable(0).data.items[0].get('ISPARALLELTECHROUTE'))
                    this.GetTransferWorkprocessNo.call(this);
            }
            break;
        case LibEventTypeEnum.Validating:
            if (e.dataInfo.tableIndex == 1) {
                //修改转入工序号
                if (e.dataInfo.fieldName == 'TRANSFERWORKPROCESSNO') {
                    if (this.dataSet.getTable(0).data.items[0].get('ISPARALLELTECHROUTE') == false) {
                        e.dataInfo.cancel = true;
                        Ext.Msg.alert("提示", "未启用并行工序不能修改转入工序号!");
                    }
                    else {
                        var items = this.dataSet.getTable(1).data.items;
                        var maxWorkProcessNo = 0;//最大工序号
                        var contain = false;//记录工序号是否存在
                        for (var i = 0; i < items.length; i++) {
                            var workProcessNo = items[i].get('WORKPROCESSNO');
                            if (maxWorkProcessNo < workProcessNo) {
                                maxWorkProcessNo = workProcessNo;
                            }
                            if (e.dataInfo.value == workProcessNo) {
                                contain = true;
                            }
                        }
                        if (e.dataInfo.dataRow.get("WORKPROCESSNO") == maxWorkProcessNo) {
                            e.dataInfo.cancel = true;
                            Ext.Msg.alert("提示", "当前行工序号为最大，转入工序号不能修改!");
                        }
                        else if (e.dataInfo.value <= e.dataInfo.dataRow.get("WORKPROCESSNO")) {
                            e.dataInfo.cancel = true;
                            Ext.Msg.alert("提示", "转入工序号不能小于或等于工序号!");
                        }
                        else if (!contain) {
                            e.dataInfo.cancel = true;
                            Ext.Msg.alert("提示", "转入工序号必须为已存在的工序号!");
                        }
                    }
                }
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == 'WORKPROCESSNO' || e.dataInfo.fieldName == 'WORKSHOPSECTIONID' || e.dataInfo.fieldName == 'WORKPROCESSID' || e.dataInfo.fieldName == 'WORKPROCESSPARAM') {
                    if (e.dataInfo.value != e.dataInfo.oldValue) {
                        this.DifFerence.call(this, e);
                    }
                    //新填工序号与原有工序号重复，两者交换工序号与转入工序号
                    if (e.dataInfo.fieldName == 'WORKPROCESSNO') {
                        var items = this.dataSet.getTable(1).data.items;
                        var length = items.length;
                        var v1;
                        var v2;
                        for (var i = 0; i < length; i++) {
                            if (e.dataInfo.dataRow.get('ROW_ID') != items[i].get('ROW_ID') &&
                                e.dataInfo.value == items[i].get('WORKPROCESSNO')) {
                                v1 = e.dataInfo.dataRow.get("WORKPROCESSNO");
                                v2 = e.dataInfo.dataRow.get("TRANSFERWORKPROCESSNO")
                                e.dataInfo.dataRow.set("WORKPROCESSNO", e.dataInfo.value);
                                e.dataInfo.dataRow.set("TRANSFERWORKPROCESSNO", items[i].get('TRANSFERWORKPROCESSNO'));
                                items[i].set('WORKPROCESSNO', v1);
                                items[i].set('TRANSFERWORKPROCESSNO', v2);
                                break;
                            }
                        }
                        if (!this.dataSet.getTable(0).data.items[0].get('ISPARALLELTECHROUTE')) {
                            e.dataInfo.dataRow.set("WORKPROCESSNO", e.dataInfo.value);
                            this.GetTransferWorkprocessNo.call(this);
                        }
                        else {
                            e.dataInfo.dataRow.set("WORKPROCESSNO", e.dataInfo.value);
                            e.dataInfo.dataRow.set("TRANSFERWORKPROCESSNO", 0);
                        }
                    }
                }
            }
            break;
    }
}

//转入工序号自动排序+加载
proto.GetTransferWorkprocessNo = function () {
    var items = this.dataSet.getTable(1).data.items;
    var list = new Array();
    for (var i = 0; i < items.length; i++) {
        list.push(items[i].get('WORKPROCESSNO'));
    }
    list.sort(function (a, b) {return a-b});
    list.push(0);
    for (var i = 0; i < list.length - 1; i++) {
        for (var j = 0; j < items.length; j++) {
            if (list[i] == items[j].get('WORKPROCESSNO')) {
                items[j].set('TRANSFERWORKPROCESSNO', list[i + 1]);
            }
        }
    }
}

//差异修改
//判断差异字段是否有值—>
//判断是否为新增行—>
//判断全局变量中是否含有此行
proto.DifFerence = function (e) {
    var row = e.dataInfo.dataRow;
    var list = this.originalValue;
    if (!row.get('DIFFERENCE')) {
        var newList = {};
        newList.RowId = row.get('ROW_ID');
        newList.WorkProcessNo = row.get('WORKPROCESSNO');
        newList.WorkShopsectionId = row.get('WORKSHOPSECTIONID');
        newList.WorkProcessId = row.get('WORKPROCESSID');
        newList.WorkProcessParam = row.get('WORKPROCESSPARAM');
        this.originalValue.push(newList);
        var modify = [];
        var c = {};
        c.Add = false;
        c.Modify = [e.dataInfo.fieldName];
        row.set('DIFFERENCE', JSON.stringify(c));
    }
    else {
        var b = true;//判断全局变量 this.originalValue 中是否有此行的值
        var c = JSON.parse(row.data['DIFFERENCE']);
        if (!c.Add) {
            if (list != undefined) {
                for (var i = 0; i < list.length; i++) {
                    if (list[i].RowId == row.get('ROW_ID')) {
                        b = false;
                        this.difference2.call(this, e, list[i],c);
                        break;
                    }
                }
            }
            if (b) {
                var data = this.invorkBcf('LogbookDt', [row.data['SALETECHROUTEID'], row.data['ROW_ID']]);
                for (var i = 0; i < data.length; i++) {
                    if (data[i].RowId == row.data['ROW_ID']) {
                        this.difference2.call(this, e, data[i],c);
                    }
                    this.originalValue.push({
                        RowId: data[i].RowId,
                        WorkProcessNo: data[i].WorkProcessNo,
                        WorkShopsectionId: data[i].WorkShopsectionId,
                        WorkProcessId: data[i].WorkProcessId,
                        WorkProcessParam: data[i].WorkProcessParam
                    });
                }
            }
            row.set('DIFFERENCE', JSON.stringify(c));
        }
    }
}

proto.difference2 = function (e,o,c) {
    var modify = [];
    var row = e.dataInfo.dataRow;
    if (e.dataInfo.fieldName == 'WORKPROCESSNO') {
        if (e.dataInfo.value != o.WorkProcessNo)
            modify.push('WORKPROCESSNO');
    }
    else {
        if (row.get('WORKPROCESSNO') != o.WorkProcessNo)
            modify.push('WORKPROCESSNO');
    }
    if (e.dataInfo.fieldName == 'WORKSHOPSECTIONID') {
        if (e.dataInfo.value != o.WorkShopsectionId)
            modify.push('WORKSHOPSECTIONID');
    }
    else {
        if (row.get('WORKSHOPSECTIONID') != o.WorkShopsectionId)
            modify.push('WORKSHOPSECTIONID');
    }
    if (e.dataInfo.fieldName == 'WORKPROCESSID') {
        if (e.dataInfo.value != o.WorkProcessId)
            modify.push('WORKPROCESSID');
    }
    else {
        if (row.get('WORKPROCESSID') != o.WorkProcessId)
            modify.push('WORKPROCESSID');
    }
    if (e.dataInfo.fieldName == 'WORKPROCESSPARAM') {
        if (e.dataInfo.value != o.WorkProcessParam)
            modify.push('WORKPROCESSPARAM');
    }
    else {
        if (row.get('WORKPROCESSPARAM') != o.WorkProcessParam)
            modify.push('WORKPROCESSPARAM');
    }
    c.Modify = modify;
}

proto.doSave = function () {
    var assistObj = {};
    var salEtechrouteId = this.dataSet.getTable(0).data.items[0].get('SALETECHROUTEID');
    var data = this.save(this.billAction, this.currentPk, assistObj);
    var success = (assistObj.hasError === undefined || !assistObj.hasError);
    if (success) {
        this.invorkBcf('PostLogSheet', [salEtechrouteId, this.originalValue]);
        this.restData(false, BillActionEnum.Browse, data);
    }
    return success;
};