ppMachiningExceptionVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
};
var proto = ppMachiningExceptionVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = ppMachiningExceptionVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);

    switch (e.libEventType) {
        case LibEventTypeEnum.Validating:
            var masteRow = this.dataSet.getTable(0).data.items[0];
            if (e.dataInfo.tableIndex == 1) {
                if (masteRow.data["DEALTYPE"] != 2 && masteRow.data["DEALTYPE"] != 3) {
                    e.dataInfo.cancel = true;
                    Ext.Msg.alert("系统提示", "非加工和返修不能对工艺路线进行修改");
                }
                else if (e.dataInfo.dataRow.data["ISCOMPLETE"] == 1) {
                    e.dataInfo.cancel = true;
                    Ext.Msg.alert("系统提示", "已完成的工序不能修改");
                }
                else {
                    //修改转入工序号
                    if (e.dataInfo.fieldName == 'TRANSFERWORKPROCESSNO') {
                        if (this.dataSet.getTable(0).data.items[0].get('ISPARALLELTECHROUTE') == false) {
                            e.dataInfo.cancel = true;
                        }
                        else {
                            var items = this.dataSet.getTable(1).data.items;
                            var maxWorkProcessNo = 0;//最大工序号
                            var contain = false;//记录工序号是否存在
                            for (var i = 0; i < items.length; i++) {
                                var workProcessNo = items[i].get('WORKPROCESSNO');//工序号
                                if (maxWorkProcessNo < workProcessNo) {
                                    maxWorkProcessNo = workProcessNo;
                                }
                                if (e.dataInfo.value == workProcessNo) {
                                    contain = true;
                                }
                            }
                            if (e.dataInfo.dataRow.get('WORKPROCESSNO') == maxWorkProcessNo) {
                                e.dataInfo.cancel = true;
                                Ext.Msg.alert("提示", "当前行工序号为最大，转入工序号不能修改!");
                            }
                            else if (e.dataInfo.value <= e.dataInfo.dataRow.get('WORKPROCESSNO')) {
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
            }
            break;
        case LibEventTypeEnum.Validated:
            if (e.dataInfo.tableIndex == 1) {
                if (e.dataInfo.fieldName == 'WORKPROCESSNO') {//修改工序号
                    var items = this.dataSet.getTable(1).data.items;
                    var length = items.length;
                    var v1;
                    var v2;
                    for (var i = 0; i < length; i++) {
                        if (e.dataInfo.dataRow.get('ROW_ID') != items[i].get('ROW_ID') && e.dataInfo.value == items[i].get('WORKPROCESSNO')) {
                            v1 = e.dataInfo.dataRow.get('WORKPROCESSNO');
                            v2 = e.dataInfo.dataRow.get('TRANSFERWORKPROCESSNO')
                            e.dataInfo.dataRow.set('WORKPROCESSNO', e.dataInfo.value);
                            e.dataInfo.dataRow.set('TRANSFERWORKPROCESSNO', items[i].get('TRANSFERWORKPROCESSNO'));
                            items[i].set('WORKPROCESSNO', v1);
                            items[i].set('TRANSFERWORKPROCESSNO', v2);
                            break;
                        }
                    }
                    if (!this.dataSet.getTable(0).data.items[0].get('ISPARALLELTECHROUTE')) {
                        e.dataInfo.dataRow.set('WORKPROCESSNO', e.dataInfo.value);
                        this.GetTransferWorkprocessNo.call(this);
                    }
                }
                else if (e.dataInfo.fieldName == 'TECHROUTETYPEID') {
                    this.forms[0].updateRecord(e.dataInfo.dataRow);
                    if (e.dataInfo.dataRow.get('ISPARALLELTECHROUTE'))
                        this.GetTransferWorkprocessNo.call(this);
                }
            }
            break;
        case LibEventTypeEnum.BeforeAddRow:
            var masteRow = this.dataSet.getTable(0).data.items[0];
            if (masteRow.data["DEALTYPE"] != 2 && masteRow.data["DEALTYPE"] != 3) {
                e.dataInfo.cancel = true;
                Ext.Msg.alert("系统提示", "非加工和返修不能对工艺路线进行修改");
            }
            break;
        case LibEventTypeEnum.BeforeDeleteRow:
            if (masteRow.data["DEALTYPE"] != 2 && masteRow.data["DEALTYPE"] != 3) {
                e.dataInfo.cancel = true;
                Ext.Msg.alert("系统提示", "非加工和返修不能对工艺路线进行修改");
            }
            break;
        case LibEventTypeEnum.DeleteRow:
            //未启用并行工艺时自动填充转入工序号
            if (!this.dataSet.getTable(0).data.items[0].get('ISPARALLELTECHROUTE'))
                this.GetTransferWorkprocessNo.call(this);
            break;
        case LibEventTypeEnum.AddRow:
            var store = this.dataSet.getTable(1);
            var length = store.data.items.length;
            var maxWp = 0;
            for (var i = 0; i < length; i++) {
                var workprocessNo = store.data.items[i].get('WORKPROCESSNO');
                if (maxWp < workprocessNo)
                    maxWp = workprocessNo;
            }
            e.dataInfo.dataRow.set('WORKPROCESSNO', (maxWp + 5));
            //未启用并行工艺时自动填充转入工序号
            if (!this.dataSet.getTable(0).data.items[0].get('ISPARALLELTECHROUTE'))
                this.GetTransferWorkprocessNo.call(this);
            break;
    }
}
//转入工序号自动排序+加载
proto.GetTransferWorkprocessNo = function () {
    var items = this.dataSet.getTable(1).data.items;
    var list = [];
    for (var i = 0; i < items.length; i++) {
        list.push(items[i].get('WORKPROCESSNO'));
    }
    list.sort(function (a, b) { return a - b });
    list.push(0);
    for (var i = 0; i < list.length - 1; i++) {
        for (var j = 0; j < items.length; j++) {
            if (list[i] == items[j].get('WORKPROCESSNO')) {
                items[j].set('TRANSFERWORKPROCESSNO', list[i + 1]);
            }
        }
    }
}