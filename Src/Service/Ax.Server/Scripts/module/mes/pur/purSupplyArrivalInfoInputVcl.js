/// <reference path="../../../ax/vcl/comm/LibVclDataFunc.js" />


purSupplyArrivalInfoInputVcl = function () {
    Ax.vcl.LibVclDataFunc.apply(this, arguments);
};
var proto = purSupplyArrivalInfoInputVcl.prototype = Object.create(Ax.vcl.LibVclDataFunc.prototype);
proto.constructor = purSupplyArrivalInfoInputVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclDataFunc.prototype.vclHandler.apply(this, arguments);
    //if (e.libEventType == LibEventTypeEnum.Validated)
    //{
    //    debugger;
    //    var table = this.dataSet.getTable(1)
    //    if (e.dataInfo.tableIndex == 1) {
    //        if (e.dataInfo.fieldName == 'SUPPLIERID') {
    //            for (var i = 0; i < table.data.items.length; i++) {
    //                if (e.dataInfo.value == table.data.items[i].data.SUPPLIERID) {
    //                    e.dataInfo.dataRow.set('SUPPLIERID', '');
    //                    e.dataInfo.dataRow.set('SUPPLIERNAME', '');
    //                    Ext.MessageBox.alert("错误提示框", "供应商【" + table.data.items[i].data.SUPPLIERNAME + "】已经存在于列表中！");
    //                    break;
    //                }
    //            }
    //        }
    //    }
    //}

    if (e.libEventType == LibEventTypeEnum.ButtonClick) {
        arrayData = [];
        var masterRow = this.dataSet.getTable(0).data.items[0];
        this.forms[0].updateRecord(masterRow);
        //var supplierid = masterRow.get('SUPPLIERID');
        var personid = masterRow.get('PERSONID');
        var Type = masterRow.get('SUPPLIERTYPE');
        var table = this.dataSet.getTable(1);
        if (e.dataInfo.fieldName == "btnLoad") {
            if (personid == '') {
                Ext.MessageBox.alert("提 示", "采购员不能为空");
            } else {
                var data = this.invorkBcf('GetSupplierTable', [personid, Type]);
                table.removeAll();
                for (var i = 0; i < data.length; i++) {
                    table.add(data[i]);
                }
                
            }
        } else if (e.dataInfo.fieldName == "btnSave") {
            if (table.data.items.length > 0) {
                var j = 0;//用于写入日期
                var ary = new Array();//用于判断是否存在相同元素
                for (var i = 0; i < table.data.items.length; i++) {
                    ary[i] = table.data.items[i].data.SUPPLIERID;
                }
                var nary = ary.sort();//对供应商集合列表进行排序
                for (var i = 0; i < ary.length; i++) {//查找是否存在相同供应商
                    if (nary[i] == nary[i + 1]) {
                        Ext.MessageBox.alert("提 示", "列表中存在相同供应商，编号为:["+nary[i]+"]");
                        return;
                    }

                }

                
                for (var i = 0; i < table.data.items.length; i++)
                {
                    var record = table.data.items[i];
                    if (record.data.CURRENTDATE == true) {
                        arrayData.push({SupplierId: record.data.SUPPLIERID,PersonId: personid,Date: 0});
                    }
                    else {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: -1 });
                    }

                    if (record.data.FIRSTDATE == true) {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: 1 });
                    }
                    else {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: -1 });
                    }

                    if (record.data.SECONDDATE == true) {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: 2 });
                    }
                    else {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: -1 });
                    }

                    if (record.data.THIRDDATE == true) {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: 3 });
                    }
                    else {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: -1 });
                    }

                    if (record.data.FOURTHDATE == true) {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: 4 });
                    }
                    else {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: -1 });
                    }
                    if (record.data.FIFTHDATE == true) {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: 5 });
                    }
                    else {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: -1 });
                    }
                    if (record.data.SIXTHDATE == true) {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: 6 });
                    } else {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: -1 });
                    }
                    if (record.data.SEVENTHDATE == true) {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: 7 });
                    } else {
                        arrayData.push({ SupplierId: record.data.SUPPLIERID, PersonId: personid, Date: -1 });
                    }
                }
                this.invorkBcf('SaveSupplierInfo', [arrayData]);
            }
        }
    }
}


//function Check(i, array) {
//    debugger;
//    if(array.length > 0)
//    {
//        for (var j = 0; j < array.length; j++)
//        {
//            if (array[j].Date == i)
//            {
//                return true;
//            }
//        }
//    }
//    return false;
//}